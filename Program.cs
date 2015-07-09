using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
namespace awagame
{
    class Program
    {
        internal static bool Verbose = false;
        internal static bool OpenVGDB = false;
        static void Main(string[] args)
        {
            ConsoleTraceListener ctl = new ConsoleTraceListener(false);
            Trace.Listeners.Add(ctl);
            Trace.AutoFlush = true;
            string fileName;
            if (args.Contains("--help") || args.Contains("-H") || args.Contains("/?") || args.Length <= 0)
            {
                Console.WriteLine("awagame is a utility to generate SQLite or JSON versions of XML ROM Datafiles");
                Console.WriteLine("Start it in a folder full of XML ROM Datafiles!");
                Console.WriteLine("awagame does not save any platform information.");
                Console.WriteLine("Usage: awagame --output=filename|-o=filename [--help|-H] [--openvgdb] [--json]");
                return;
            }
           
            if (!(args[0].StartsWith("--output=") || args[0].StartsWith("-o=")))
            {
                Console.WriteLine("You must specify output filename");
                Console.WriteLine("Usage: awagame --output=filename|-o=filename [--help|-H] [--openvgdb] [--json]");
                return;
            }
            else
            {
                fileName = args[0].Split('=')[1];
            }
            if (args.Contains("--verbose") || args.Contains("-v"))
            {
                Program.Verbose = true;
            }
            if(args.Contains("--openvgdb") && File.Exists("openvgdb.sqlite")){
                Program.OpenVGDB = true;
            }
            if (args.Contains("--openvgdb") && !File.Exists("openvgdb.sqlite"))
            {
                Console.WriteLine("[ERROR] OpenVGDB specified but openvgdb.sqlite not found");
                return;
            }
            if (args.Contains("--json"))
            {
                awaseru.BuildCurrentDirectoryJson(fileName);
            }
            else
            {
                awaseru.BuildCurrentDirectorySqlite(fileName);
            }
        }
    }
}
