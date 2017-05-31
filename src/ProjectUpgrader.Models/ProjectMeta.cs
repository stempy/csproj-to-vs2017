using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectUpgrader.Models
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


        public IEnumerable<ProjectReference> GetNugetRefs()
        {
            return ProjectReferences?.Where(u => PackageReferences.Any(i => i.Name == u.Name));
        }

        public IEnumerable<ProjectReference> GetProjectRefs()
        {
            return ProjectReferences?.Where(u => u.ReferenceType==ProjectReferenceType.ProjectReference);
        }

        public IEnumerable<ProjectReference> GetBinaryRefs()
        {
            return ProjectReferences?.Where(u => u.ReferenceType == ProjectReferenceType.Reference);
        }
    }

}
