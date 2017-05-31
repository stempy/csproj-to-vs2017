using System.Xml.Linq;
using ProjectUpgrader.Models;

namespace ProjectUpgrader.ProjectReader
{
    public interface IProjectFileReader
    {
        XNamespace CsProjxmlns { get; set; }
        ProjectMeta LoadProjectFile(string file);
    }
}