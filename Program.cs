using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
namespace awagame
{
    class Program
    {
        static void Main(string[] args)
        {
            ConsoleTraceListener ctl = new ConsoleTraceListener(false);
            Trace.Listeners.Add(ctl);
            Trace.AutoFlush = true;
            awaseru.BuildCurrentDirectory();
            Console.ReadKey();
        }
    }
}
