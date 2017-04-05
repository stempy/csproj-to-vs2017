using CsProjToVs2017Upgrader;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Linq;
using System.Xml;
using System;

namespace ProjectUpgrader.Upgraders
{
    /// <summary>
    /// Upgrades references in a .csproj to package reference where they appear to be nuget packages
    /// </summary>
    public class ProjectToVs2017ReferenceOnlyUpgrader : IProjectToVs2017ReferenceOnlyUpgrader
    {
        ProjectFileReader _projectReader;
        private ILogger _log;
        private PackageConfigReader _packageConfigReader;
        private ProjectPackageReferenceXmlHelpers _xmlHelpers;

        private XNamespace _projectNameSpace;

        public ProjectToVs2017ReferenceOnlyUpgrader(ILogger logger)
        {
            _projectReader = new ProjectFileReader();
            _packageConfigReader = new PackageConfigReader();
            _xmlHelpers = new ProjectPackageReferenceXmlHelpers();
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
            var projectPath = Path.GetDirectoryName(srcProjectFile);

            if (string.IsNullOrEmpty(destProjectFile))
            {
                if (!allowOverwrite)
                {
                    throw new Exception(nameof(destProjectFile) + " not specified and " + nameof(allowOverwrite) + " not set to true");
                }

                destProjectFile = srcProjectFile;
            }
            
            //var meta = _projectReader.LoadProjectFile(srcProjectFile);
            var content = File.ReadAllText(srcProjectFile);
            var doc = XDocument.Parse(content);

            var refs = _xmlHelpers.GetNugetRefs(doc);
            
            var packagesAttrXName = XName.Get("Include");
            var packagesElementToRemove = doc.Descendants(_projectNameSpace+"None")
                                            //.Elements("None")
                                            .Where(u=> u.Attribute(packagesAttrXName)
                                                        .Value=="packages.config").FirstOrDefault();

            if (packagesElementToRemove == null)
            {
                packagesElementToRemove = doc.Descendants(_projectNameSpace + "Content")
                                            //.Elements("None")
                                            .Where(u => u.Attribute(packagesAttrXName)
                                                        .Value == "packages.config").FirstOrDefault();
            }

            if (packagesElementToRemove != null)
            {
                var packagesConfigPath = packagesElementToRemove.Attribute(packagesAttrXName).Value;
                packagesConfigPath = Path.Combine(projectPath, packagesConfigPath);
                var packageRefItems = _packageConfigReader.GetPackageConfigReferences(packagesConfigPath);
                var newPackageReferences = _xmlHelpers.CreatePackageReferenceItems(packageRefItems);

                // remove binary direct references from xdoc
                refs.Remove();

                // remove packages.config from xdoc
                packagesElementToRemove.Remove();


                // Add new package references to xdoc
                var newItemGroup = _xmlHelpers.AddItemGroupReferences(newPackageReferences);
                doc.Element(_projectNameSpace+"Project").Add(newItemGroup);
            }
            
            WriteNewCsProjectFile(srcProjectFile, destProjectFile, doc);

            _log.LogInformation(destProjectFile);
        }

      

        private void WriteNewCsProjectFile(string srcFile,string destFile, XDocument doc)
        {
            using (var fs = new FileStream(destFile, FileMode.Create))
            {
                XmlWriterSettings settings = new XmlWriterSettings()
                {
                    Indent = true,
                    //WriteEndDocumentOnClose = false
                    //OmitXmlDeclaration = true
                };
                XmlWriter xWrite = XmlWriter.Create(fs,settings);
                doc.WriteTo(xWrite);
                xWrite.Flush();
                fs.Flush();
            }

            var destDir = Path.GetDirectoryName(destFile);
            var oldCsProj = Path.GetFileNameWithoutExtension(srcFile) + ".old.csproj";
            File.Copy(srcFile, Path.Combine(destDir, oldCsProj), true);
        }
    }
}
