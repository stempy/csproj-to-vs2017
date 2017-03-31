using System;
using System.Collections.Generic;
using System.Text;

namespace CsProjToVs2017Upgrader.Models
{
    public class ProjectMeta
    {
        public string ProjectName { get; set; }
        public string ProjectFilePath { get; set; }
        public string ProjectFileRelativePath { get; set; }
        public Guid ProjectGuid { get; set; }

        public string RootNameSpace { get; set; }
        public string AssemblyName { get; set; }
        public string TargetFrameworkVersion { get; set; }
        public string FileAlignment { get; set; }

        public Guid ProjectTypeGuid { get; set; }
        /// <summary>
        /// Websites have 2 project types
        /// </summary>
        public Guid ProjectTypeGuid2 { get; set; }

        public string ProjectTypeDesc { get; set; }

        public IEnumerable<ProjectReference> ProjectReferences { get; set; }
        public IEnumerable<PackageReference> PackageReferences { get; set; }

        public string OutputType { get; set; }
        public ProjectType ProjectType { get; set; }

        public string BelongsToSolutionFile { get; set; }
    }
}
