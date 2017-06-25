using System.Collections.Generic;
using System.Xml.Linq;
using ProjectUpgrader.Models;

namespace ProjectUpgrader.ProjectReader
{
    public interface IProjectFileReader
    {
        XNamespace CsProjxmlns { get; set; }
        IEnumerable<PackageReference> GetPackageReferencesFromProject(XDocument doc, string file);
        ProjectMeta LoadProjectFile(string file);
    }
}