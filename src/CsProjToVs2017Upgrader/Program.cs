using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

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
            var projectUpgrader = new UpgradeProjectToVs2017();
            var slns = new Dictionary<string, string>();

            foreach (var arg in args)
            {
                var projectsAnalyzed = ap.AnalyzeProjectsPath(arg);

                bool copySolution = false;
                string newSlnPath = string.Empty;
                string slnSrc = string.Empty;

                // now upgrade .csproj where suited
                foreach (var project in projectsAnalyzed)
                {
                    var newProjFile=projectUpgrader.UpgradeProject(project.ProjectFilePath);
                    if (!string.IsNullOrEmpty(project.BelongsToSolutionFile) 
                        &&  !copySolution)
                    {
                        slnSrc = project.BelongsToSolutionFile;
                        var newSlnFileName = Path.GetFileNameWithoutExtension(project.BelongsToSolutionFile) + "_upgraded.sln";
                        var slnDir = Path.GetDirectoryName(Path.GetDirectoryName(newProjFile));
                        newSlnPath = Path.Combine(slnDir, newSlnFileName);
                        copySolution = true;
                    }
                }

                if (!string.IsNullOrEmpty(slnSrc))
                {
                    // has a sln 
                    var slnText = File.ReadAllText(slnSrc);
                    //slnText = slnText.Replace(".csproj", "_upgraded.csproj");
                    File.WriteAllText(newSlnPath, slnText);
                    Console.WriteLine("new sln " + newSlnPath);
                }



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