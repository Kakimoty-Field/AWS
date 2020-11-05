using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static async Task<string> waitwait(int i)
        {
            Console.WriteLine($"[{i}])  -  0");
            await Task.Delay(3000);
            Console.WriteLine($"[{i}])  -  1");
            await Task.Delay(3000);
            Console.WriteLine($"[{i}])  -  2");
            return "1";
        }

        static void Main(string[] args)
        {
            var list = new List<Task>();
            for (var i = 1; i <= 100; i++)
            {
                list.Add(waitwait(i));
            }
            Console.WriteLine("  ----  wait");
            var w = Task.WhenAll(list.ToArray());
            w.Wait();
            Console.WriteLine("  ----  done");

        }
    }
}
