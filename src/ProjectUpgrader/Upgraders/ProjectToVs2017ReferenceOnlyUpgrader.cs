using CsProjToVs2017Upgrader;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Linq;

namespace ProjectUpgrader.Upgraders
{
    /// <summary>
    /// Upgrades references in a .csproj to package reference where they appear to be nuget packages
    /// </summary>
    public class ProjectToVs2017ReferenceOnlyUpgrader : IProjectToVs2017ReferenceOnlyUpgrader
    {
        ProjectFileReader _projectReader;
        private ILogger _log;

        private XNamespace _projectNameSpace;

        public ProjectToVs2017ReferenceOnlyUpgrader(ILogger logger)
        {
            _projectReader = new ProjectFileReader();
            _projectNameSpace = _projectReader.CsProjxmlns;
            _log = logger;
        }

        /// <summary>
        /// Upgrade references to PackageReference with optional dest file
        /// </summary>
        /// <param name="srcProjectFile"></param>
        /// <param name="allowOverwrite"></param>
        /// <param name="destProjectFile">If specified destination file to write to, otherwise write to src directly</param>
        /// <returns></returns>
        public void UpgradeProjectFile(string srcProjectFile, bool allowOverwrite, string destProjectFile=null)
        {
            _log.LogDebug($"UpgradeProjectFile: srcProjectFile:{srcProjectFile} destProjectFile:{destProjectFile}");
            var meta = _projectReader.LoadProjectFile(srcProjectFile);
            var content = File.ReadAllText(srcProjectFile);
            var doc = XDocument.Parse(content);

            var refs = GetNugetRefs(doc);


            var packagesAttrXName = XName.Get("Include");
            var packagesElementToRemove = doc.Descendants(_projectNameSpace+"ItemGroup")
                                            .Elements("None")
                                            .Where(u=> u.Attribute(packagesAttrXName)
                                                        .Value=="packages.config");
        }

        IEnumerable<XElement> GetNugetRefs(XDocument doc)
        {
            var refs = GetReferences(doc);
            var nrefs = new List<XElement>();
            foreach (var r in refs)
            {
                var hintPath = r.Elements(_projectNameSpace + "HintPath").FirstOrDefault();
                
                if (!string.IsNullOrEmpty(hintPath.Value) && hintPath.Value.Contains("packages\\"))
                {
                    nrefs.Add(r);
                }
            }
            return nrefs;
        }

        IEnumerable<XElement> GetReferences(XDocument doc)
        {
            return doc.Descendants(_projectNameSpace + "Reference");
        }
    }
}
