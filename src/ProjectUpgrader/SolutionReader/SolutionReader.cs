using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ProjectUpgrader.Models;
using ProjectUpgrader.ProjectReader;

namespace ProjectUpgrader.SolutionReader
{
    public class SolutionReader : ISolutionReader
    {
        protected const string SlnProjectRegex =
            "Project\\(\"\\{([\\w-]*)\\}\"\\) = \"([\\w _]*.*)\", \"(.*\\.(cs|vcx|vb)proj)\", \"\\{([\\w-]*)\\}\"";
        protected const string SlnProjectReplace =
            "Project(\"{0}\") = \"{1}\", \"{2}\", \"{3}\"";


        public IEnumerable<string> GetProjectFilesInSolution(string slnFile)
        {
            var projList = GetBaseProjectMetaFromSln(slnFile, File.ReadAllText(slnFile));
            return projList.Select(m => m.ProjectFilePath);
        }

        private IEnumerable<ProjectMeta> GetBaseProjectMetaFromSln(string slnFile,string slnFileContent)
        {
            Regex projReg = new Regex(SlnProjectRegex, RegexOptions.Compiled);
            var matches = projReg.Matches(slnFileContent).Cast<Match>();
            var Projects = matches.Select(x => new ProjectMeta()
            {
                ProjectTypeGuid = Guid.Parse(x.Groups[1].Value),
                ProjectName = x.Groups[2].Value,
                ProjectFileRelativePath = x.Groups[3].Value,
                ProjectFilePath = x.Groups[3].Value,
                ProjectGuid = Guid.Parse(x.Groups[5].Value),
                BelongsToSolutionFile = slnFile
            }).ToList();
            return Projects;
        }

        /// <summary>
        /// Get Project Items from .sln file
        /// </summary>
        /// <param name="slnFile"></param>
        /// <returns></returns>
        public IEnumerable<ProjectMeta> ParseProjectsInSolution(string slnFile)
        {
            var Content = File.ReadAllText(slnFile);
            var Projects = GetBaseProjectMetaFromSln(slnFile, Content).ToList();
            var projList = new List<ProjectMeta>();
            var projReader = new ProjectFileReader();
            foreach(var p in Projects)
            {
                var path = p.ProjectFilePath;
                if (!Path.IsPathRooted(path))
                    path = Path.Combine(Path.GetDirectoryName(slnFile), path);
                path = Path.GetFullPath(path);
                if (path.EndsWith(".csproj"))
                {
                    var projItem = projReader.LoadProjectFile(path);
                    projItem.BelongsToSolutionFile = slnFile;
                    projList.Add(projItem);
                }
                
            }
            return projList;
            //foreach (var ProjectMeta in Projects)
            //{
            //    var path = ProjectMeta.ProjectFilePath;

            //    if (!Path.IsPathRooted(path))
            //        path = Path.Combine(Path.GetDirectoryName(slnFile), path);
            //    path = Path.GetFullPath(path);
            //    ProjectMeta.ProjectFilePath = path;

            //    if (ProjectMeta.ProjectTypeGuid==Guid.Empty)
            //    {
            //        ProjectMeta.ProjectType = ProjectType.LegacyClassLibrary;
            //    } else
            //    {
            //        var pm = new ProjectTypesMapper();
            //        var outputType = ProjectOutputType.Library;
            //        ProjectMeta.ProjectType = pm.GetProjectType(ProjectMeta.ProjectTypeGuid,outputType);
            //    }
            //}
            //return Projects;
        }
    }
}
