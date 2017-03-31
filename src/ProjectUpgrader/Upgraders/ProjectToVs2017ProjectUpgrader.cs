﻿using CsProjToVs2017Upgrader;
using CsProjToVs2017Upgrader.Models;
using System;
using System.Collections;
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

        public ProjectToVs2017ProjectUpgrader()
        {
            _projectReader = new ProjectFileReader();
            _nugetRefUpdater = new ReferenceToPackageReferenceUpgrader();
        }


        public void UpgradeProjectFile(string srcProjectFile)
        {
            var projFileDest = Path.Combine(Path.GetDirectoryName(srcProjectFile), Path.GetFileNameWithoutExtension(srcProjectFile) + "_upgraded.csproj");
            var projInfo = _projectReader.LoadProjectFile(srcProjectFile);

            if (projInfo.ProjectType != ProjectType.LegacyClassLibrary)
            {
                throw new Exception($"{srcProjectFile} is not a legacy class library. Cannot update");
            }

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

            var newFw = UpgradeTargetFramework(projInfo.TargetFrameworkVersion);

            // Create xml
            XElement root =
                new XElement("Project", new XAttribute("Sdk", "Microsoft.NET.Sdk"));

            // propertygroups
            root.Add(new XElement("PropertyGroup",
                        new XElement("TargetFramework", newFw)
                ));

            // build referencegroups
            var projRefEl = CreateReferenceItems(projectRefs);
            var binReflEl = CreateReferenceItems(binRefs);

            var itemGroup = new XElement("ItemGroup");
            foreach (var xel in binReflEl)
            {
                itemGroup.Add(xel);
            }
            foreach (var xel in projRefEl)
            {
                itemGroup.Add(xel);
            }

            root.Add(itemGroup);

            var outputDir = Path.Combine(Path.GetTempPath(), "Cs2017ProjectUpgrader");
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            var destFile = Path.Combine(outputDir, Path.GetFileName(projFileDest));

            Console.WriteLine(destFile);
            var fs = new FileStream(destFile, FileMode.Create);
            XmlWriterSettings settings = new XmlWriterSettings() { Indent = true };
            XmlWriter xWrite = XmlWriter.Create(fs,settings);
            XDocument xDoc = new XDocument(root);
            xDoc.Save(fs);
            xWrite.Flush();
            fs.Flush();
        }

        private IEnumerable<XElement> CreateReferenceItems(IEnumerable<ProjectReference> refs)
        {
            var projRefs = refs
                            .Where(y=>y.ReferenceType==ProjectReferenceType.ProjectReference)
                            .Select(r => new XElement("ProjectReference",
                                            new XAttribute("Include", r.Include)));

            var binRef = refs.Where(y => y.ReferenceType == ProjectReferenceType.Reference)
                             .Select(r => new XElement("Reference", 
                                            new XAttribute("Include",r.Include)));

            var allRefs = projRefs.Concat(binRef);
            return allRefs;
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
