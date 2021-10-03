using System;
using System.Threading;
using System.Threading.Tasks;

namespace CreateSimpleTasks
{
    class Program
    {
        static void Main(string[] args)
        {
            // Example 1
            new Task(() => { Console.WriteLine("Task1"); }).Start();
            
            // Example 2
            Task.Run(() =>
            {
                Thread.Sleep(1000);
                Console.WriteLine("Task2");
                throw new Exception("This exception will not be exposed");
            });
            
            // Example 3
            // при больших state выдается System.OverflowException
            Task<Int32> t = new Task<Int32>(n => Sum((Int32)n), 1000000000);

            // Можно начать выполнение задания через некоторое время
            t.Start();

            // Можно ожидать завершения задания в явном виде
            //t.Wait(); // ПРИМЕЧАНИЕ. Существует перегруженная версия,
                      // принимающая тайм-аут/CancellationToken

            // Получение результата (свойство Result вызывает метод Wait)
            Console.WriteLine("The Sum is: " + t.Result); // Значение Int32
            
            Thread.Sleep(5000);
            
            Console.WriteLine("Returning from Main");
        }
        
        private static Int32 Sum(Int32 n) {
            Int32 sum = 0;
            for (; n > 0; n--)
                checked { sum += n; } // при больших n выдается System.OverflowException
            return sum;
        }
    }
}