using System;
using System.Threading;
using System.Threading.Tasks;

namespace TasksContinuation
{
    class Program
    {
        static void Main(string[] args)
        {
            var cts = new CancellationTokenSource();

            // Создание и запуск задания с продолжением
            Task<Int32> t = Task.Run(() => Sum(1000000, cts.Token), cts.Token);

            Console.WriteLine("Press any key to stop...");
            cts.Cancel();
            Console.ReadKey();

            // Метод ContinueWith возвращает объект Task, но обычно
            // он не используется
            t.ContinueWith(task => Console.WriteLine("The sum is: " + task.Result),
                TaskContinuationOptions.OnlyOnRanToCompletion);
            t.ContinueWith(task => Console.WriteLine("Sum threw: " + task.Exception),
                TaskContinuationOptions.OnlyOnFaulted);
            t.ContinueWith(task => Console.WriteLine("Sum was canceled"),
                TaskContinuationOptions.OnlyOnCanceled);

            t.Wait();
        }

        private static Int32 Sum(Int32 n, CancellationToken ct = default)
        {
            Console.WriteLine("Long running process...");
            Thread.Sleep(5000);
            Int32 sum = 0;
            for (; n > 0; n--)
            {
                // Следующая строка приводит к исключению OperationCanceledException
                // при вызове метода Cancel для объекта CancellationTokenSource,
                // на который ссылается маркер
                ct.ThrowIfCancellationRequested();
                checked
                {
                    sum += n;
                } // при больших n появляется
                // исключение System.OverflowException
            }

            return sum;
        }
    }
}