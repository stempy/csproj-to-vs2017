using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CsProjToVs2017Upgrader
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Usage();
                return;
            }

            var ap = new ProjectAnalyzer();
            var slns = new Dictionary<string, string>();

            foreach (var arg in args)
            {
                var projectsAnalyzed = ap.AnalyzeProjectsPath(arg);
            }

            if (Debugger.IsAttached)
            {
                Console.WriteLine("Press any key to continute.");
                Console.ReadKey();
            }
        }

        static void Usage()
        {
            Console.WriteLine("Old Csproj to VS2017 Upgrader\n=============================\nUsage:");
            Console.WriteLine("\tCsProjToVs2017Upgrader slnfile1.sln slnfile2.sln projectfile.csproj projectfile2.csproj");
        }

      
    }
}