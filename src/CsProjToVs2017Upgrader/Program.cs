using CsProjToVs2017Upgrader.Models;
using ProjectUpgrader.SolutionReader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

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

            foreach (var arg in args)
            {
                ProcessPath(arg);
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

        static void ProcessPath(string path)
        {
            if (path.ToLower().EndsWith(".sln"))
            {
                Console.WriteLine($"\nParsing solution file \"{path}\"");
                Console.WriteLine("============================================================");
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
                Console.WriteLine($"\nParsing project file \"{path}\"");
                Console.WriteLine("============================================================");

                var projectReader = new ProjectFileReader();
                var projInfo = projectReader.LoadProjectFile(path);
                DisplayInfo(projInfo);
            }
        }

        static void DisplayInfo(ProjectMeta meta)
        {
            var filename = Path.GetFileName(meta.ProjectFilePath);

            var line = $"{filename} {meta.ProjectName} {meta.AssemblyName} [{meta.ProjectType}]";
            // get reference lists
            var refListNotNuget = meta.ProjectReferences.Where(u => !meta.PackageReferences.Any(i => i.Name == u.Name));
            var binaryRef = refListNotNuget.Where(u => u.ReferenceType == ProjectReferenceType.Reference);
            var projRef = refListNotNuget.Except(binaryRef);

            var binRefList = GetRefListString(binaryRef);
            var projRefList = GetRefListString(projRef);

            //var refList = refListNotNuget.Select(m => $"\t{m.Name} => {m.Version} [{m.ReferenceType}]");
            var packageList = meta.PackageReferences.Select(m => $"\t{m.Name} => {m.Version}");
            Console.WriteLine(line);
            DisplayReferenceList("Binary References",binRefList);
            DisplayReferenceList("Project References",projRefList);
            DisplayReferenceList("Packages", packageList);
        }

        static void DisplayReferenceList(string title, IEnumerable<string> strArr)
        {
            if (strArr.Any())
            {
                Console.WriteLine(title+":");
                Console.WriteLine(string.Join("\n", strArr));
            }
        }

        static IEnumerable<string> GetRefListString(IEnumerable<ProjectReference> items)
        {
            return items.Select(m => $"\t{m.Name} => {m.Version} [{m.ReferenceType}]");
        }
    }
}