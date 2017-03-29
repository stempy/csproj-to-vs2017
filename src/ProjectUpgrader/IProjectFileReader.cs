using System.Xml.Linq;
using CsProjToVs2017Upgrader.Models;

namespace CsProjToVs2017Upgrader
{
    public interface IProjectFileReader
    {
        XNamespace CsProjxmlns { get; set; }
        ProjectMeta LoadProjectFile(string file);
    }
}