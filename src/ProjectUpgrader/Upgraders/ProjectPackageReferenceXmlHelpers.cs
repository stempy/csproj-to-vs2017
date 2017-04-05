using CsProjToVs2017Upgrader.Models;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace ProjectUpgrader.Upgraders
{
    public class ProjectPackageReferenceXmlHelpers
    {
        private XNamespace _projectNameSpace = "http://schemas.microsoft.com/developer/msbuild/2003";

        /// <summary>
        /// Makes parsing easier by removing the need to specify namespaces for every element.
        /// </summary>
        public void RemoveNamespaces(XDocument document)
        {
            var elements = document.Descendants();
            elements.Attributes().Where(a => a.IsNamespaceDeclaration).Remove();
            foreach (var element in elements)
                element.Name = element.Name.LocalName;
        }

        public void RemoveEmptyNamespaces(XDocument document)
        {
            var elements = document.Descendants();
            elements.Attributes().Where(a => a.IsNamespaceDeclaration && a.Value=="").Remove();
            foreach (var element in elements)
                element.Name = element.Name.LocalName;

        }


        public XElement StripNamespaces(XElement rootElement)
        {
            foreach (var element in rootElement.DescendantsAndSelf())
            {
                // update element name if a namespace is available
                if (element.Name.Namespace != XNamespace.None)
                {
                    element.Name = XNamespace.None.GetName(element.Name.LocalName);
                }

                // check if the element contains attributes with defined namespaces (ignore xml and empty namespaces)
                bool hasDefinedNamespaces = element.Attributes().Any(attribute => attribute.IsNamespaceDeclaration ||
                        (attribute.Name.Namespace != XNamespace.None && attribute.Name.Namespace != XNamespace.Xml));

                if (hasDefinedNamespaces)
                {
                    // ignore attributes with a namespace declaration
                    // strip namespace from attributes with defined namespaces, ignore xml / empty namespaces
                    // xml namespace is ignored to retain the space preserve attribute
                    var attributes = element.Attributes()
                                            .Where(attribute => !attribute.IsNamespaceDeclaration)
                                            .Select(attribute =>
                                                (attribute.Name.Namespace != XNamespace.None && attribute.Name.Namespace != XNamespace.Xml) ?
                                                    new XAttribute(XNamespace.None.GetName(attribute.Name.LocalName), attribute.Value) :
                                                    attribute
                                            );

                    // replace with attributes result
                    element.ReplaceAttributes(attributes);
                }
            }
            return rootElement;
        }


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
            var pkgRefs = refs.Select(y => new XElement(_projectNameSpace+ "PackageReference",
                                            new XAttribute("Include", y.Name),
                                                new XElement(_projectNameSpace+"Version",y.Version)
                                            
                                     ));
            return pkgRefs;
        }

        public XElement AddItemGroupReferences(IEnumerable<XElement> items)
        {
            var itemGroup = new XElement(_projectNameSpace+ "ItemGroup");
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
