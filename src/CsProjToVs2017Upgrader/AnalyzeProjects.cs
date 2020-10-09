using ProjectUpgrader.SolutionReader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Extensions.Logging;
using ProjectUpgrader.Models;
using ProjectUpgrader.ProjectReader;

namespace CsProjToVs2017Upgrader
{
    public class ProjectAnalyzer
    {
        private ILogger _log;

        public ProjectAnalyzer(ILogger logger)
        {
            _log = logger;
        }
        
        /// <summary>
        /// Analyze Projects to get reference and package information
        /// Display results verbose
        /// </summary>
        /// <param name="path"></param>
        public IEnumerable<ProjectMeta> AnalyzeProjectsPath(string path)
        {
            var isSln = path.ToLower().EndsWith(".sln");
            var projList = GetProjectItemsInfo(path);

            if (isSln)
            {
                var projListGrped = projList.GroupBy(n => n.ProjectType);
                foreach (var g in projListGrped)
                {
                    Console.WriteLine($"{g.Key}\n------------------------------");
                    foreach (var item in g)
                    {
                        DisplayProjectInfo(item);
                    }
                    Console.WriteLine();
                }
                var numLibs = projList.Count(n => n.ProjectType == ProjectType.LegacyClassLibrary);
                var numMvc = projList.Count(n => n.ProjectType == ProjectType.LegacyMvcApplication);
                var numConsole = projList.Count(n => n.ProjectType == ProjectType.LegacyConsole);
                Console.WriteLine($"{projList.Count()} project files found libs:{numLibs} mvc:{numMvc} console:{numConsole}");
            }
            else
            {
                var projInfo = projList.FirstOrDefault();
                DisplayProjectInfo(projInfo);
            }

            return projList;
        }

        /// <summary>
        /// Just parse sln or csproj file to get items. Not verbose
        /// </summary>
        /// <param name="slnOrProj"></param>
        /// <returns></returns>
        public IEnumerable<ProjectMeta> GetProjectItemsInfo(string slnOrProj)
        {
            var projList = new List<ProjectMeta>();
            if (slnOrProj.ToLower().EndsWith(".sln"))
            {
                Console.WriteLine($"\nParsing solution file \"{slnOrProj}\"");
                Console.WriteLine("============================================================");
                ISolutionReader solutionReader = new SolutionReader();
                projList = solutionReader.ParseProjectsInSolution(slnOrProj).ToList();
            }
            else
            {
                Console.WriteLine($"\nParsing project file \"{slnOrProj}\"");
                Console.WriteLine("============================================================");
                var projectReader = new ProjectFileReader();
                var projInfo = projectReader.LoadProjectFile(slnOrProj);
                projList.Add(projInfo);
            }
            return projList;
        }


        void DisplayProjectInfo(ProjectMeta meta)
        {
            if (meta.Exception != null)
            {
                var aline = $"Project: {Path.GetFileName(meta.ProjectFilePath)} {meta.ProjectName} {meta.AssemblyName} [{meta.ProjectType}-{meta.TargetFrameworkVersion}]";
                Console.WriteLine(aline+Environment.NewLine+"\t\t"+ meta.Exception);
                return;
            }
            
            
            var filename = Path.GetFileName(meta.ProjectFilePath);

            // C# 7 local function to get reflist string
            IEnumerable<string> GetRefListString(IEnumerable<ProjectReference> items)
            {
                return items.Select(m => $"\t{m.Name} => [{m.ReferenceType}:{m.Version}] {m.HintPath}");
            }

            // get reference lists
            var binRefList = GetRefListString(meta.GetBinaryRefs());
            var binNugetList = GetRefListString(meta.GetNugetRefs());
            var projRefList = GetRefListString(meta.GetProjectRefs());

            //var refList = refListNotNuget.Select(m => $"\t{m.Name} => {m.Version} [{m.ReferenceType}]");
            var packageList = meta.PackageReferences.Select(m => $"\t{m.Name} => {m.Version}");
            var line = $"Project:{filename} {meta.ProjectName} {meta.AssemblyName} [{meta.ProjectType}-{meta.TargetFrameworkVersion}]";
            Console.WriteLine(line);
            DisplayReferenceList("Binary References", binRefList);
            DisplayReferenceList("Binary References (Nuget)", binNugetList);
            DisplayReferenceList("Project References", projRefList);
            DisplayReferenceList("Packages", packageList);
            Console.WriteLine();
        }

        void DisplayReferenceList(string title, IEnumerable<string> strArr)
        {
            if (strArr.Any())
            {
                Console.WriteLine(title + ":");
                Console.WriteLine(string.Join("\n", strArr));
            }
        }
    }
}
