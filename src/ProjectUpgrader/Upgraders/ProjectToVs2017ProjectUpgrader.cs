using CsProjToVs2017Upgrader;
using CsProjToVs2017Upgrader.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace ProjectUpgrader.Upgraders
{
    /// <summary>
    /// see: http://www.natemcmaster.com/blog/2017/03/09/vs2015-to-vs2017-upgrade/
    /// </summary>
    public class ProjectToVs2017ProjectUpgrader : IProjectToVs2017ProjectUpgrader
    {
        IProjectFileReader _projectReader;
        ReferenceToPackageReferenceUpgrader _nugetRefUpdater;
        ProjectPackageReferenceXmlHelpers _xmlHelpers;

        public ProjectToVs2017ProjectUpgrader()
        {
            _projectReader = new ProjectFileReader();
            _nugetRefUpdater = new ReferenceToPackageReferenceUpgrader();
            _xmlHelpers = new ProjectPackageReferenceXmlHelpers();
        }

        public string UpgradeProjectFile(string srcProjectFile)
        {
            var projFileDest = Path.Combine(Path.GetDirectoryName(srcProjectFile), 
                                            Path.GetFileName(srcProjectFile));

            var projInfo = _projectReader.LoadProjectFile(srcProjectFile);

            // get binary references split up into nuget and non
            var references = projInfo.ProjectReferences.Where(m => m.ReferenceType == ProjectReferenceType.Reference);

            // Translates to
            // <Reference Include="System.Configuration" />
            var binRefs = references.Where(m => !m.IsNugetPackage);

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

            // Create xml
            XElement root =
                new XElement("Project", new XAttribute("Sdk", "Microsoft.NET.Sdk"));

            // propertygroups
            var props = CreatePropertyGroups(projInfo);
            var propertyGroups = _xmlHelpers.CreatePropertyGroupsElement(props, UpgradeTargetFramework(projInfo.TargetFrameworkVersion));


            root.Add(propertyGroups);

            // build referencegroups
            var projRefEl = _xmlHelpers.CreateReferenceItems(projectRefs);
            var binReflEl = _xmlHelpers.CreateReferenceItems(binRefs);
            var pkgRefEl = _xmlHelpers.CreatePackageReferenceItems(newNugetPackageReferenes);

            // add <ItemGroup> element with all references to project root

            var itemGroups = new List<XElement>();
            itemGroups.Add(_xmlHelpers.AddItemGroupReferences(projRefEl));
            itemGroups.Add(_xmlHelpers.AddItemGroupReferences(binReflEl));
            itemGroups.Add(_xmlHelpers.AddItemGroupReferences(pkgRefEl));
            foreach(var i in itemGroups)
            {
                // add as separate itemgroups
                root.Add(i);
            }

            // write xml file
            var outputDir = Path.Combine(Path.GetTempPath(), "Cs2017ProjectUpgrader");
            var destFile = Path.Combine(outputDir, 
                                    Path.GetFileNameWithoutExtension(projFileDest)
                                    +Path.DirectorySeparatorChar
                                    +Path.GetFileName(projFileDest));
            Console.WriteLine(destFile);

            WriteNewCsProjectFile(srcProjectFile, destFile, root);
            return destFile;
        }

     

        private void WriteNewCsProjectFile(string srcFile,string destFile, XElement rootElement)
        {
            var dir = Path.GetDirectoryName(destFile);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            
            using(var fs = new FileStream(destFile, FileMode.OpenOrCreate))
            {
                XmlWriterSettings settings = new XmlWriterSettings() {
                    Indent = true,OmitXmlDeclaration=true };
                XmlWriter xWrite = XmlWriter.Create(fs, settings);
                //XDocument xDoc = new XDocument(rootElement);
                //xDoc.Save(fs);
                rootElement.WriteTo(xWrite);
                xWrite.Flush();
                fs.Flush();
            }

            var oldCsProj = Path.GetFileNameWithoutExtension(srcFile) + "_old.csproj";
            File.Copy(srcFile, Path.Combine(dir, oldCsProj),true);
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
            if (meta.OutputType!= "Library")
            {
                d.Add("OutputType", meta.OutputType);
            }

            return d;
        }

        public string UpgradeTargetFramework(string oldFwversionString)
        {
            if (oldFwversionString.StartsWith("v4.6"))
                return oldFwversionString.Replace("v4", "net4").Replace(".", "");

            switch (oldFwversionString)
            {
                case "v4.0":
                    return "net40";
                case "v4.6.1":
                    return "net461";
                case "v4.6.2":
                    return "net462";
                case "v4.5.2":
                    return "net452";
                case "v4.5.1":
                    return "net451";
            }
            return oldFwversionString;
        }
    }
}
