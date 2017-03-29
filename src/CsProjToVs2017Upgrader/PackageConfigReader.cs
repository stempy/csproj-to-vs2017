﻿using CsProjToVs2017Upgrader.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace CsProjToVs2017Upgrader
{
    public class PackageConfigReader
    {
        /// <summary>
        /// from packages.config nuget
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public IEnumerable<PackageReference> GetPackageConfigReferences(string filename)
        {
            var packages = new List<PackageReference>();
            var source = Path.GetFileName(filename);
            XDocument doc = XDocument.Load(filename);
            IEnumerable<XElement> childList =
                from el in doc.Elements().FirstOrDefault(x => x.Name == "packages").Elements()
                select el;
            foreach (XElement e in childList)
            {
                var name = e.Attribute("id").Value;
                var version = e.Attribute("version").Value;
                packages.Add(new PackageReference()
                {
                    Name = name,
                    Version = version,
                });
            }
            return packages;
        }
    }
}
