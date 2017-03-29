using CsProjToVs2017Upgrader.Models;
using ProjectUpgrader.SolutionReader;
using System;
using System.Diagnostics;
using System.Linq;

namespace CsProjToVs2017Upgrader
{
    class Program
    {
        static void Main(string[] args)
        {
            var actualPath = slnPath;

            ProcessPath(actualPath);
        }

        static void ProcessPath(string path)
        {
            if (path.ToLower().EndsWith(".sln"))
            {
                Console.WriteLine($"Parsing solution file \"{path}\" ...");
                ISolutionReader solutionReader = new SolutionReader();
                var projList = solutionReader.ParseProjectsInSolution(path);

                var projListGrped = projList.GroupBy(n => n.ProjectType);
                foreach(var g in projListGrped)
                {
                    Console.WriteLine($"{g.Key}\n------------------------------");
                    foreach(var item in g)
                    {
                        DisplayInfo(item);
                    }
                    Console.WriteLine();
                }


                var numLibs = projList.Count(n => n.ProjectType == ProjectType.LegacyClassLibrary);
                var numMvc = projList.Count(n => n.ProjectType == ProjectType.LegacyMvcApplication);
                var numConsole = projList.Count(n => n.ProjectType == ProjectType.LegacyConsole);



                Console.WriteLine($"{projList.Count()} project files found libs:{numLibs} mvc:{numMvc} console:{numConsole}");
            } else
            {
                var projectReader = new ProjectFileReader();
                var projInfo = projectReader.LoadProjectFile(path);
                DisplayInfo(projInfo);
            }


            if (Debugger.IsAttached)
            {
                Console.WriteLine("Press any key to continute.");
                Console.ReadKey();
            }
        }

        static void DisplayInfo(ProjectMeta meta)
        {
            var line = $"{meta.ProjectName} {meta.AssemblyName} [{meta.ProjectType}]";
            Console.WriteLine(line);
        }
    }
}