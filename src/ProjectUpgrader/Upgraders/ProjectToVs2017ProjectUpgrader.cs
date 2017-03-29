using CsProjToVs2017Upgrader;
using CsProjToVs2017Upgrader.Models;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ProjectUpgrader.Upgraders
{
    /// <summary>
    /// see: http://www.natemcmaster.com/blog/2017/03/09/vs2015-to-vs2017-upgrade/
    /// </summary>
    public class ProjectToVs2017ProjectUpgrader
    {
        IProjectFileReader _projectReader;
        ReferenceToPackageReferenceUpgrader _nugetRefUpdater;

        public ProjectToVs2017ProjectUpgrader()
        {
            _projectReader = new ProjectFileReader();
            _nugetRefUpdater = new ReferenceToPackageReferenceUpgrader();
        }

        public void UpgradeProjectFile(string srcProjectFile)
        {
            var projFileDest = Path.Combine(Path.GetDirectoryName(srcProjectFile),Path.GetFileNameWithoutExtension(srcProjectFile)+"_upgraded.csproj");
            var projInfo = _projectReader.LoadProjectFile(srcProjectFile);

            // get binary references split up into nuget and non
            var references = projInfo.ProjectReferences.Where(m => m.ReferenceType == ProjectReferenceType.Reference);

            // Translates to
            // <Reference Include="System.Configuration" />
            var nonNugetRefs = references.Where(m =>  !m.IsNugetPackage);

            // we must combine these refs to see which ones are the same in both
            // Translates to
            // <ItemGroup>
            //      <PackageReference Include="My.Nuget.Name" Version="2.3.3" />
            // </ItemGroup>
            var nugetRefs = references.Where(m => m.IsNugetPackage);
            var nugetPackageRefs = projInfo.PackageReferences;
            var newNugetPackageReferenes = GetFilteredPackageReferences(nugetRefs, nugetPackageRefs);

            // get project based references
            // <ProjectReference Include="..\ClassLibrary1\ClassLibrary1.csproj" />
            var projectRefs = projInfo.ProjectReferences.Where(m => m.ReferenceType == ProjectReferenceType.ProjectReference);

        }

        private IEnumerable<PackageReference> GetFilteredPackageReferences(IEnumerable<ProjectReference> projRefs, IEnumerable<PackageReference> existingPackRefs)
        {
            // TODO: need to combine projrefs with existing
            // TODO: 29/3/2017
            return existingPackRefs;
        }

        /// <summary>
        /// Create default vs 2017 propertygroups
        /// see: http://www.natemcmaster.com/blog/2017/03/09/vs2015-to-vs2017-upgrade/
        /// "What to keep" for understanding
        /// </summary>
        /// <param name="meta"></param>
        /// <returns></returns>
        private IDictionary<string,string> CreatePropertyGroups(ProjectMeta meta)
        {
            var d = new Dictionary<string, string>();
            var projectFilename = Path.GetFileNameWithoutExtension(meta.ProjectFilePath);
            if (meta.RootNameSpace != projectFilename)
            {
                d.Add("RootNamespace", meta.RootNameSpace);
            }

            if (meta.AssemblyName != projectFilename)
            {
                d.Add("AssemblyName", meta.AssemblyName);
            }

            return d;
        }

        public string UpgradeTargetFramework(string oldFwversionString)
        {
            if (oldFwversionString.StartsWith("v4.6"))
                return oldFwversionString.Replace("v4", "net4").Replace(".", "");

            switch (oldFwversionString)
            {
                case "v4.6.1":
                    return "net461";
                case "v4.6.2":
                    return "net462";
                case "v4.5.2":
                    return "net452";
            }
            return oldFwversionString;
        }
    }
}
