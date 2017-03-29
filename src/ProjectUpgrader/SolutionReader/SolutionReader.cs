using CsProjToVs2017Upgrader;
using CsProjToVs2017Upgrader.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ProjectUpgrader.SolutionReader
{
    public class SolutionReader : ISolutionReader
    {
        protected const string SlnProjectRegex =
            "Project\\(\"\\{([\\w-]*)\\}\"\\) = \"([\\w _]*.*)\", \"(.*\\.(cs|vcx|vb)proj)\", \"\\{([\\w-]*)\\}\"";
        protected const string SlnProjectReplace =
            "Project(\"{0}\") = \"{1}\", \"{2}\", \"{3}\"";

        /// <summary>
        /// Get Project Items from .sln file
        /// </summary>
        /// <param name="slnFile"></param>
        /// <returns></returns>
        public IEnumerable<ProjectMeta> ParseProjectsInSolution(string slnFile)
        {
            var Content = File.ReadAllText(slnFile);
            Regex projReg = new Regex(SlnProjectRegex, RegexOptions.Compiled);
            var matches = projReg.Matches(Content).Cast<Match>();
            var Projects = matches.Select(x => new ProjectMeta()
            {
                ProjectTypeGuid = Guid.Parse(x.Groups[1].Value),
                ProjectName = x.Groups[2].Value,
                ProjectFileRelativePath = x.Groups[3].Value,
                ProjectFilePath = x.Groups[3].Value,
                ProjectGuid = Guid.Parse(x.Groups[5].Value)
            }).ToList();


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
                    projList.Add(projReader.LoadProjectFile(path));
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
