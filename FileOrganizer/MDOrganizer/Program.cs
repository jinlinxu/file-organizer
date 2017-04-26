using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace MDOrganizer
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args == null || args.Length != 3)
            {
                Console.WriteLine("Please specifiy parameters e.g. MDOrganizer.exe <md directory> <toc file> <destination directory>");
                return;
            }
            Worker work = new Worker(args[0],args[1], args[2]);
            work.Start();
            work.Finish();
        }
    }
}
