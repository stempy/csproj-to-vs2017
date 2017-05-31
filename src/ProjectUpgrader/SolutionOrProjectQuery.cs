using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProjectUpgrader.Models;
using ProjectUpgrader.ProjectReader;
using ProjectUpgrader.SolutionReader;

namespace ProjectUpgrader
{
    public class SolutionOrProjectQuery
    {
        readonly ISolutionReader _solutionReader = new SolutionReader.SolutionReader();
        readonly IProjectFileReader _projectFileReader = new ProjectFileReader();

        public IEnumerable<string> GetFilesToProcess(IEnumerable<string> fileSpec)
        {
            var filesToProcess = new List<string>();
            foreach (var slnOrProjectSpec in fileSpec)
            {
                if (slnOrProjectSpec.Contains("*") || slnOrProjectSpec.Contains("?"))
                {
                    // wildcard search
                    var dir = Path.GetDirectoryName(slnOrProjectSpec);
                    var filePattern = Path.GetFileName(slnOrProjectSpec);

                    var files = Directory.GetFiles(dir, filePattern, SearchOption.TopDirectoryOnly).ToArray();
                    filesToProcess.AddRange(files);
                }
                else
                {
                    filesToProcess.Add(slnOrProjectSpec);
                }
            }
            return filesToProcess;
        }

        public IEnumerable<ProjectMeta> GetProjects(IEnumerable<string> fileSpec)
        {
            var fileList = GetFilesToProcess(fileSpec);
            var proj = fileList.SelectMany(ProcessFiles);
            return proj;
        }


        public IEnumerable<ProjectMeta> ProcessFiles(string filePath)
        {
            var isSln = filePath.EndsWith(".sln", StringComparison.OrdinalIgnoreCase);
            var isProj = filePath.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase);
            var slnType = isSln ? "Solution" : isProj ? "Project" : "none";
            Console.Write($"File: \"{filePath}\"");
            Console.Write($" FileType:{slnType}");

            var proj = new List<ProjectMeta>();
            if (isSln)
            {
                proj = ParseProjectsFromSln(filePath).ToList();
            }
            else if (isProj)
            {
                proj.Add(ParseProjectFile(filePath));
            }

            return proj;
        }

        private IEnumerable<ProjectMeta> ParseProjectsFromSln(string slnFile)
        {
            return _solutionReader.ParseProjectsInSolution(slnFile);
        }

        private ProjectMeta ParseProjectFile(string proj)
        {
            return _projectFileReader.LoadProjectFile(proj);
        }

    }
}
