using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using ProjectUpgrader.Models;

namespace ProjectUpgrader.ProjectReader
{
    public class ProjectFileReader : IProjectFileReader
    {
        public XNamespace CsProjxmlns { get; set; } = "http://schemas.microsoft.com/developer/msbuild/2003";

        public ProjectMeta LoadProjectFile(string file)
        {
            if (!File.Exists(file))
            {
                throw new FileNotFoundException(file);
            }
            var p = new ProjectMeta()
            {
                ProjectFilePath = file
            };
            var content = File.ReadAllText(file);
            var doc = XDocument.Parse(content);
            p = InitMetaRoot(p, doc);

            p.ProjectReferences = GetProjectReferences(doc, file);
            p.PackageReferences = GetPackageReferences(file);

            return p;
        }

        /// <summary>
        /// read packages.config
        /// </summary>
        /// <param name="projectFile"></param>
        /// <returns></returns>
        private IEnumerable<PackageReference> GetPackageReferences(string projectFile)
        {
            var packagePath = Path.GetDirectoryName(projectFile);
            var packageFile = Path.Combine(packagePath, "packages.config");
            var packageReader = new PackageConfigReader();
            var pkgs = packageReader.GetPackageConfigReferences(packageFile);
            return pkgs;
        }

        /// <summary>
        /// Get project meta data
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        private ProjectMeta InitMetaRoot(ProjectMeta obj, XDocument doc)
        {
            obj.ProjectGuid = Guid.Parse(doc.Descendants(CsProjxmlns + "ProjectGuid").FirstOrDefault().Value);
            var projectTypeGuids = doc.Descendants(CsProjxmlns + "ProjectTypeGuids").FirstOrDefault()?.Value;
            if (projectTypeGuids != null)
            {
                // we have project type guids
                var pGuids = projectTypeGuids.Split(';');
                if (pGuids.Length > 1)
                {
                    obj.ProjectTypeGuid = Guid.Parse(pGuids[0]); // likely MVC type
                    obj.ProjectTypeGuid2 = Guid.Parse(pGuids[1]); // likely C# {fae04ec0-301f-11d3-bf4b-00c04f79efbc}
                }
            }

            obj.RootNameSpace = doc.Descendants(CsProjxmlns + "RootNamespace").FirstOrDefault().Value;
            obj.AssemblyName = doc.Descendants(CsProjxmlns + "AssemblyName").FirstOrDefault().Value;
            obj.TargetFrameworkVersion = doc.Descendants(CsProjxmlns + "TargetFrameworkVersion").FirstOrDefault().Value;
            obj.OutputType = doc.Descendants(CsProjxmlns + "OutputType").FirstOrDefault().Value;

            var pMapper = new ProjectTypesMapper();

            obj.ProjectTypeDesc = pMapper.GetProjectTypeDescription(obj.ProjectTypeGuid);
            ProjectOutputType outType;
            Enum.TryParse<ProjectOutputType>(obj.OutputType, out outType);
            obj.ProjectType = pMapper.GetProjectType(obj.ProjectTypeGuid,outType);

            return obj;
        }

        /// <summary>
        /// from .csproj, read references and project references
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        private IEnumerable<ProjectReference> GetProjectReferences(XDocument doc, string file)
        {
            var packages = new List<ProjectReference>();
            var source = Path.GetFileName(file);
            var references = GetCsProjectReferences(doc, source, "Reference");
            var projReferences = GetCsProjectReferences(doc, source, "ProjectReference");
            var packageReferences = GetCsProjectReferences(doc, source, "PackageReference"); // new VS2017 package reference
            packages.AddRange(references);
            packages.AddRange(projReferences);
            packages.AddRange(packageReferences);
            return packages;
        }

        /// <summary>
        /// Get all included files in project
        /// ie Compile Include=""
        ///    Content Include="" 
        /// etc
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        private IEnumerable<string> GetProjectIncludedFiles(XDocument doc)
        {
            var inc = new List<string>();
            var compiles = doc.Descendants(CsProjxmlns + "Compile").Select(x => x.Attribute("Include")?.Value).ToArray();
            var content = doc.Descendants(CsProjxmlns + "Content").Select(x => x.Attribute("Include")?.Value).ToArray();
            var none = doc.Descendants(CsProjxmlns + "None").Select(x => x.Attribute("Include")?.Value).ToArray();
            var folder = doc.Descendants(CsProjxmlns + "Folder").Select(x => x.Attribute("Include")?.Value).ToArray();

            inc.AddRange(compiles);
            inc.AddRange(content);
            inc.AddRange(none);
            inc.AddRange(folder);
            return inc;
        }

        private IEnumerable<ProjectReference> GetCsProjectReferences(XDocument doc, string source, string referenceElement)
        {
            List<ProjectReference> references = new List<ProjectReference>();
            var refType = (ProjectReferenceType)Enum.Parse(typeof(ProjectReferenceType), referenceElement);

            IEnumerable<XElement> projReferences =
                from el in doc.Descendants(CsProjxmlns + referenceElement)
                select el;
            foreach (XElement e in projReferences)
            {
                var inc = e.Attribute("Include").Value;
                var subName = e.Element(CsProjxmlns + "Name")?.Value;
                var name = subName ?? (inc.Contains(",") ? inc.Remove(inc.IndexOf(",")) : inc);
                var hintPath = e.Elements(CsProjxmlns + "HintPath").FirstOrDefault();
                var specificVersion = e.Elements(CsProjxmlns + "SpecificVersion").FirstOrDefault();
                var isPrivate = e.Elements(CsProjxmlns + "Private").FirstOrDefault() != null;
                
                references.Add(new ProjectReference()
                {
                    //ReferenceElement = referenceElement,
                    ReferenceType = refType,
                    Name = name,
                    Source = source,
                    Include = inc,
                    HintPath = hintPath?.Value,
                    IsPrivate = isPrivate,
                    IsSpecificVersion = specificVersion != null && (specificVersion.Value == "True" ? true : false)
                });
            }

            return references;
        }
    }
}
