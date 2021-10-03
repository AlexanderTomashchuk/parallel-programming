using System;
using System.Threading;

namespace SimpleCancellationToken
{
    class Program
    {
        static void Main(string[] args)
        {
            var cts = new CancellationTokenSource(5000);
            
            // Передаем операции CancellationToken и число
            ThreadPool.QueueUserWorkItem(o => Count(cts.Token, 1000));
            
            Console.WriteLine("Press <Enter> to cancel the operation.");
            Console.ReadLine();
            cts.Cancel();

            // Cancel немедленно возвращает управление, метод продолжает работу...
            Console.WriteLine("Press <Enter> to exit...");
            Console.ReadLine();
        }
        
        private static void Count(CancellationToken token, Int32 countTo) {
            for (Int32 count = 0; count <countTo; count++) {
                if (token.IsCancellationRequested) {
                    Console.WriteLine("Count is cancelled");
                    break; // Выход их цикла для остановки операции
                }
                Console.WriteLine(count);
                Thread.Sleep(200);   // Для демонстрационных целей просто ждем
            }
            Console.WriteLine("Count is done");
        }
    }
}