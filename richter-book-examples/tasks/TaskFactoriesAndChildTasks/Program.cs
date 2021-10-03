using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TaskFactoriesAndChildTasks
{
    class Program
    {
        static void Main(string[] args)
        {
            Task parent = new Task(() =>
            {
                var cts = new CancellationTokenSource();

                var tf = new TaskFactory<Int32>(cts.Token,
                    TaskCreationOptions.AttachedToParent,
                    TaskContinuationOptions.ExecuteSynchronously,
                    TaskScheduler.Default);

                var childTasks = new[]
                {
                    tf.StartNew(() => Sum(10000, cts.Token)),
                    tf.StartNew(() => Sum(20000, cts.Token)),
                    tf.StartNew(() => Sum(int.MaxValue, cts.Token)) // Исключение OverflowException
                };

                // Если дочернее задание становится источником исключения,
                // отменяем все дочерние задания
                foreach (var task in childTasks)
                    task.ContinueWith(t => cts.Cancel(), TaskContinuationOptions.OnlyOnFaulted);

                // После завершения дочерних заданий получаем максимальное
                // возвращенное значение и передаем его другому заданию для вывода
                tf.ContinueWhenAll(
                        childTasks,
                        completedTasks =>
                            completedTasks.Where(t => !t.IsFaulted && !t.IsCanceled).Max(t => t.Result),
                        CancellationToken.None)
                    .ContinueWith(t => Console.WriteLine("The maximum is: " + t.Result),
                        TaskContinuationOptions.ExecuteSynchronously);
            });

            parent.ContinueWith(p =>
            {
                // Текст помещен в StringBuilder и однократно вызван
                // метод Console.WriteLine просто потому, что это задание
                // может выполняться параллельно с предыдущим,
                // и я не хочу путаницы в выводимом результате
                StringBuilder sb = new StringBuilder(
                    "The following exception(s) occurred:" + Environment.NewLine);
                foreach (var e in p.Exception.Flatten().InnerExceptions)
                {
                    sb.AppendLine(" " + e.GetType());
                }

                Console.WriteLine(sb.ToString());
            }, TaskContinuationOptions.OnlyOnFaulted);

// Запуск родительского задания, которое может запускать дочерние
            parent.Start();

            Thread.Sleep(1000);
        }

        private static Int32 Sum(Int32 n, CancellationToken ct = default)
        {
            Console.WriteLine("Long running process...");
            Int32 sum = 0;
            for (; n > 0; n--)
            {
                ct.ThrowIfCancellationRequested();
                checked
                {
                    sum += n;
                } // при больших n появляется исключение System.OverflowException
            }

            return sum;
        }
    }
}