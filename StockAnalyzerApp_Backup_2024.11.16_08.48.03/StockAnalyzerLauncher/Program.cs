using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UltimateChartistLauncher
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Nb Args: " + args.Length);
            foreach (var arg in args)
            {
                Console.WriteLine(arg);
            }
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }
}
