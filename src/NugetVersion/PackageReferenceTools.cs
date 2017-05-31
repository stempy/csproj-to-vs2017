using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ProjectUpgrader.Models;

namespace NugetVersion
{
    public class PackageReferenceTools
    {
        // If you want to implement both "*" and "?"
        private static string WildCardToRegular(string value)
        {
            return "^" + Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*") + "$";
        }


        /// <summary>
        /// Process the project groups by sln (or project files)
        /// </summary>
        /// <param name="projectGrps"></param>
        public void ProcessProjectGroups(Dictionary<string, IEnumerable<ProjectMeta>> projectGrps, string packageName, string newVersion)
        {
            var fndProjectsWithVersion = FindSlnAndProjectsMatchingCriteria(projectGrps, packageName);

            foreach (var projectGrp in fndProjectsWithVersion)
            {
                var key = projectGrp.Key;
                var items = projectGrp.Value;
                Console.WriteLine((string)key);
                foreach (var projectMeta in items)
                {
                    Console.WriteLine($"\t{projectMeta.AssemblyName}");
                    var pr = projectMeta.PackageReferences;
                    var pkgs = string.IsNullOrEmpty(packageName) ? pr : FilterPackageReferences(pr, packageName);
                    foreach (var pkg in pkgs)
                    {
                        Console.WriteLine($"\t\t{pkg}");
                    }
                }
            }
        }

        /// <summary>
        /// filter package references by name
        /// </summary>
        /// <param name="pr"></param>
        /// <param name="packageNameSpec"></param>
        /// <returns></returns>
        private IEnumerable<PackageReference> FilterPackageReferences(IEnumerable<PackageReference> pr, string packageNameSpec)
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