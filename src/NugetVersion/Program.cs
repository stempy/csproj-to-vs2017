using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ProjectUpgrader;
using ProjectUpgrader.Models;

namespace NugetVersion
{
    class Program
    {
        static readonly PackageReferenceTools PackageReferenceTools = new PackageReferenceTools();

        static void Main(string[] args)
        {
            if (!args.Any())
            {
                Usage();
                WaitDebugger();
                return;
            }

            // parse command line params
            var slnOrProj = args[0];
            var packageName = args.Length > 1 ? args[1]:null;
            var versionInfo = args.Length > 2 ? args[2]:null;
            string newVersion = string.Empty; // only if this is set will we update

            if (!string.IsNullOrEmpty(packageName))
            {
                if (!string.IsNullOrEmpty(versionInfo))
                {
                    if (versionInfo.Contains("-setversion="))
                    {
                        newVersion = versionInfo.Split('=')[1];
                    }
                }
            }

            // get project data from 1st arg, group by sln
            var projectGrps = GetProjectFileGroups(slnOrProj);

            // now process the groups
            var fndProjects=PackageReferenceTools.ProcessProjectGroups(projectGrps, packageName, newVersion);



            


            // end
            WaitDebugger();
        }

       

        private static Dictionary<string, IEnumerable<ProjectMeta>> GetProjectFileGroups(string slnOrProj)
        {
            var slnOrProjFiles = slnOrProj.Split(new char[] {','}).Select(u => u.Trim());
            var fileProcessor = new SolutionOrProjectQuery();
            var projects = fileProcessor.GetProjects(slnOrProjFiles);
            var projectGrps = projects.GroupBy(u => u.BelongsToSolutionFile)
                .ToDictionary(k => k.Key, y => (IEnumerable<ProjectMeta>) y.ToList());
            return projectGrps;
        }


        static void WaitDebugger()
        {
            if (Debugger.IsAttached)
            {
                Console.ReadLine();
            }
        }


        static void Usage()
        {
            Console.WriteLine("\nNugetVersion slnfile.sln,sln2.sln|project1.csproj,projectfile.csproj [packagenamespec] [-setversion=x.y.z]");
            Console.WriteLine("\n\tOnly works for packagereferenced elements <PackageReference>\n\t see: http://blog.nuget.org/20170316/NuGet-now-fully-integrated-into-MSBuild.html");
            Console.WriteLine("\n\tslnfile|projectfile = specify sln or csproj file, comma separated for multiple, or wildcards");
            Console.WriteLine("\t[packagenamespec] = packagename to query or set - can use wildcard ie EntityFramework.*");
            Console.WriteLine("\t[-setversion=x.y.z] = set version of all packages matching packagenamespec to x.y.z");
            Console.WriteLine("\n");
        }
    }
}