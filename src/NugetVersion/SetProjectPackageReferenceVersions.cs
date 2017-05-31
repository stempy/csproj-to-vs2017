using System.IO;
using System.Linq;
using System.Xml.Linq;
using ProjectUpgrader.ProjectReader;
using ProjectUpgrader.Upgraders;

namespace NugetVersion
{
    /// <summary>
    /// Set nuget package reference versions for a project
    /// </summary>
    public class SetProjectPackageReferenceVersions
    {
        private readonly IProjectFileReader _projectFileReader = new ProjectFileReader();
        private readonly PackageReferenceTools _packageReferenceTools = new PackageReferenceTools();
        private readonly ProjectPackageReferenceXmlHelpers _xmlHelpers = new ProjectPackageReferenceXmlHelpers();

        public void SetProjectNugetVersions(string projFile, string packagenameSpec, string newVersion)
        {
            var content = File.ReadAllText(projFile);
            var doc = XDocument.Parse(content);

            var pr = _projectFileReader.GetPackageReferencesFromProject(doc, projFile);
            var filteredPr=_packageReferenceTools.FilterPackageReferences(pr, packagenameSpec);

            var filteredPrKeys = filteredPr.Select(y => y.Name).ToList();

            var packageReferenceElements = _xmlHelpers.GetPackageReferences(doc);
            var filteredPackageElemets = packageReferenceElements
                .Where(u => filteredPrKeys.Contains(u.Attribute("Include").Value))
                .ToList();



        }
    }
}