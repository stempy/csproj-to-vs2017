using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ProjectUpgrader.Models;

namespace NugetVersion
{
    public class PackageReferenceTools
    {
        //private readonly SetProjectPackageReferenceVersions _setProjectPackageReferenceVersions = new SetProjectPackageReferenceVersions();
        
        
        // If you want to implement both "*" and "?"
        private static string WildCardToRegular(string value)
        {
            return "^" + Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*") + "$";
        }


        /// <summary>
        /// Process the project groups by sln (or project files)
        /// </summary>
        /// <param name="projectGrps"></param>
        public IDictionary<string,IEnumerable<ProjectMeta>> ProcessProjectGroups(Dictionary<string, IEnumerable<ProjectMeta>> projectGrps, string packageName, string newVersion)
        {
            var fndProjectsWithVersion = FindSlnAndProjectsMatchingCriteria(projectGrps, packageName);
            var nugetCompare = new NugetVersionCompareDictionary();

            foreach (var projectGrp in fndProjectsWithVersion)
            {
                var key = projectGrp.Key;
                var items = projectGrp.Value;
                Console.WriteLine((string)key);
                foreach (var projectMeta in items)
                {
                    var projectAsmName = projectMeta.AssemblyName;

                    Console.WriteLine($"\t{projectAsmName}");
                    var pr = projectMeta.PackageReferences;
                    var pkgs = string.IsNullOrEmpty(packageName) ? pr : FilterPackageReferences(pr, packageName);
                    foreach (var pkg in pkgs)
                    {
                        var name = pkg.Name;
                        var ver = pkg.Version.Trim();
                        string newVerStr = string.Empty;

                        if (!string.IsNullOrEmpty(newVersion))
                        {
                            newVersion = newVersion.Trim();
                            if (newVersion != ver)
                            {
                                newVerStr = " >>> " + newVersion;
                            }
                        }
                        Console.WriteLine($"\t\t{name} = {ver}{newVerStr}");
                        if (!nugetCompare.ContainsKey(name))
                        {
                            nugetCompare.Add(name);
                        }

                        nugetCompare[name].ProjectVersionDictionary[projectAsmName] = ver;
                    }
                }
            }


            if (!string.IsNullOrEmpty(newVersion))
            {
                Console.WriteLine("================================================================================");
                Console.WriteLine($"Update Version to {newVersion} Process:");
                Console.WriteLine("================================================================================");

                // update to new version
                foreach (var p in fndProjectsWithVersion)
                {
                    var key = p.Key;
                    var items = p.Value;
                    Console.WriteLine(key);
                    foreach (var projectMeta in items)
                    {
                        Console.WriteLine($"\t{projectMeta.AssemblyName}");
                    }
                }

                Console.WriteLine("================================================================================");
            }
            //foreach (var k in nugetCompare)
            //{
            //    Console.WriteLine($"{k.Key}");
            //    foreach (var pair in k.Value.ProjectVersionDictionary)
            //    {
            //        Console.WriteLine($"\t{pair.Key} = {pair.Value}");
            //    }

            //    //var d = k.Value.CompareVersions();

            //}
            return fndProjectsWithVersion;
        }

        /// <summary>
        /// filter package references by name
        /// </summary>
        /// <param name="pr"></param>
        /// <param name="packageNameSpec"></param>
        /// <returns></returns>
        public IEnumerable<PackageReference> FilterPackageReferences(IEnumerable<PackageReference> pr, string packageNameSpec)
        {
            if (packageNameSpec.Contains("*") || packageNameSpec.Contains("?"))
            {
                var regexPattern = WildCardToRegular(packageNameSpec);
                var r = new Regex(regexPattern,RegexOptions.IgnoreCase);
                return pr.Where(u => r.IsMatch(u.Name));
            }

            return pr.Where(u => u.Name.Contains(packageNameSpec));
        }

        private Dictionary<string, IEnumerable<ProjectMeta>> FindSlnAndProjectsMatchingCriteria(Dictionary<string, IEnumerable<ProjectMeta>> projectGrps, string packageName)
        {
            var fndProjectsWithVersion = new Dictionary<string, IEnumerable<ProjectMeta>>();

            // grab only slns and projects that match filter criteria
            foreach (var projectGrp in projectGrps)
            {
                if (fndProjectsWithVersion.ContainsKey(projectGrp.Key))
                {
                    continue;
                }
                var projectsWithPackages = new List<ProjectMeta>();

                //Console.WriteLine(key);
                foreach (var projectMeta in projectGrp.Value)
                {
                    //Console.WriteLine($"\t{projectMeta.AssemblyName}");
                    var pr = projectMeta.PackageReferences;
                    var pkgs = string.IsNullOrEmpty(packageName) ? pr : FilterPackageReferences(pr, packageName);
                    if (pkgs.Any())
                    {
                        projectMeta.PackageReferences = pkgs;
                        projectsWithPackages.Add(projectMeta);
                    }
                }

                if (projectsWithPackages.Any())
                {
                    fndProjectsWithVersion[projectGrp.Key] = projectsWithPackages;
                }
            }
            return fndProjectsWithVersion;
        }
    }
}