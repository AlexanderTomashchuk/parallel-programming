using System;
using System.Threading;
using System.Threading.Tasks;

namespace TasksCancellation
{
    class Program
    {
        static void Main(string[] args)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            Task<Int32> t = new Task<Int32>(() => Sum(100, cts.Token), cts.Token);
            t.Start();

            // Позднее отменим CancellationTokenSource, чтобы отменить Task
            cts.Cancel(); // Это асинхронный запрос, задача уже может быть завершена

            try
            {
                // В случае отмены задания метод Result генерирует
                // исключение AggregateException
                Console.WriteLine("The sum is: " + t.Result); // Значение Int32
            }
            catch (AggregateException e)
            {
                // Считаем обработанными все объекты OperationCanceledException
                // Все остальные исключения попадают в новый объект AggregateException,
                // состоящий только из необработанных исключений
                e.Handle(innerE => innerE is OperationCanceledException);
                Console.WriteLine("Exception handled");

                // Строка выполняется, если все исключения уже обработаны
                Console.WriteLine("Sum was canceled");
            }

            Console.ReadKey();
            cts.Cancel();
        }

        private static Int32 Sum(Int32 n, CancellationToken ct)
        {
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