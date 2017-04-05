﻿using CsProjToVs2017Upgrader.Models;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace ProjectUpgrader.Upgraders
{
    public class ProjectPackageReferenceXmlHelpers
    {
        private XNamespace _projectNameSpace = "http://schemas.microsoft.com/developer/msbuild/2003";

        public IEnumerable<XElement> GetNugetRefs(XDocument doc)
        {
            var refs = GetReferences(doc);
            var nrefs = new List<XElement>();
            foreach (var r in refs)
            {
                var hintPath = r.Elements(_projectNameSpace + "HintPath").FirstOrDefault();

                if (hintPath != null && !string.IsNullOrEmpty(hintPath.Value) && hintPath.Value.Contains("packages\\"))
                {
                    nrefs.Add(r);
                }
            }
            return nrefs;
        }

        public IEnumerable<XElement> GetReferences(XDocument doc)
        {
            return doc.Descendants(_projectNameSpace + "Reference");
        }


        public IEnumerable<XElement> CreatePackageReferenceItems(IEnumerable<PackageReference> refs)
        {
            var pkgRefs = refs.Select(y => new XElement("PackageReference",
                                            new XAttribute("Include", y.Name),
                                            new XAttribute("Version", y.Version))
                                     );
            return pkgRefs;
        }

        public XElement AddItemGroupReferences(IEnumerable<XElement> items)
        {
            var itemGroup = new XElement("ItemGroup");
            foreach (var xel in items)
            {
                itemGroup.Add(xel);
            }
            return itemGroup;
        }

        public IEnumerable<XElement> CreateReferenceItems(IEnumerable<ProjectReference> refs)
        {
            var projRefs = refs
                            .Where(y => y.ReferenceType == ProjectReferenceType.ProjectReference)
                            .Select(r => new XElement("ProjectReference",
                                            new XAttribute("Include", r.Include)));

            var binRef = refs.Where(y => y.ReferenceType == ProjectReferenceType.Reference)
                             .Select(r => new XElement("Reference",
                                            new XAttribute("Include", r.Name)));

            var allRefs = projRefs.Concat(binRef);
            return allRefs;
        }

        public XElement CreatePropertyGroupsElement(IDictionary<string, string> propGroups, string frameworkVersion)
        {
            //var newFw = UpgradeTargetFramework(meta.TargetFrameworkVersion);
            var propertyGroups = new XElement("PropertyGroup",
                                    new XElement("TargetFramework", frameworkVersion));
            foreach (var p in propGroups)
            {
                propertyGroups.Add(new XElement(p.Key, p.Value));
            }
            return propertyGroups;
        }

    }
}
