using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using CsProjToVs2017Upgrader.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using ProjectUpgrader.Upgraders;

namespace CsProjToVs2017Upgrader
{
    class Program
    {
        static ILoggerFactory loggerFactory;
        static IConfigurationRoot Configuration { get; set; }
        static IProjectToVs2017ReferenceOnlyUpgrader _refUpgrader;
        static ProjectAnalyzer _projectAnalyzer;


        static void Main(string[] args)
        {
            //var builder = new ConfigurationBuilder()
            // .SetBasePath(Directory.GetCurrentDirectory())
            //.AddJsonFile("appsettings.json");
            //Configuration = builder.Build();

            if (args.Length < 1)
            {
                Usage();
                return;
            }

            ILoggerFactory loggerFactory = new LoggerFactory()
                .AddConsole()
                .AddDebug();

            _refUpgrader = new ProjectToVs2017ReferenceOnlyUpgrader(loggerFactory.CreateLogger<ProjectToVs2017ReferenceOnlyUpgrader>());
            _projectAnalyzer = new ProjectAnalyzer(loggerFactory.CreateLogger<ProjectAnalyzer>());

            bool generateUpgrades = false;
            bool upgradeReferences = false;

            if (args.Contains("--generate") || args.Contains("-g"))
            {
                generateUpgrades = true;
                args = args.Where(m => m != "-g").ToArray();
                args = args.Where(m => m != "--generate").ToArray();
            }

            if (args.Contains("--upgraderefs") || args.Contains("-u"))
            {
                upgradeReferences = true;
                generateUpgrades = false;
                args = args.Where(m => m != "-u" && m != "--upgraderefs").ToArray();
            }


            

            foreach (var arg in args)
            {
                // 1. analyze projects
                var projectsAnalyzed = _projectAnalyzer.AnalyzeProjectsPath(arg);

                if (generateUpgrades)
                {
                    GenerateUpgrades(projectsAnalyzed);
                }

                if (upgradeReferences)
                {
                    var dir = GetDestDir();
                    UpgradeReferences(dir,projectsAnalyzed);
                }
            }

            if (Debugger.IsAttached)
            {
                Console.WriteLine("Press any key to continute.");
                Console.ReadKey();
            }
        }

        static void Usage()
        {
            Console.WriteLine("Old Csproj to VS2017 Upgrader\n=============================\nUsage:");


            Console.WriteLine("\tCsProjToVs2017Upgrader [-g|--generate | -u|-upgraderefs] slnfile1.sln slnfile2.sln projectfile.csproj projectfile2.csproj");
            Console.WriteLine("\t\t-g|--generate   Generate new .NetStandard csproj files.");
            Console.WriteLine("\t\t-u|--upgraderef   Just update legacy csproj nuget packages to new VS2017 packagereference format.");
            Console.WriteLine();
        }


        static string GetDestDir()
        {
            var destDir = Path.Combine(Path.GetTempPath(), "VS2017_UpgradedReferences");
            if (!Directory.Exists(destDir))
                Directory.CreateDirectory(destDir);

            return destDir;
        }

        static string GetFileDestPath(string srcProjPath, string destDir)
        {
            return Path.Combine(destDir, Path.GetFileName(srcProjPath));
        }

        static void UpgradeReferences(string destDir, IEnumerable<ProjectMeta> projects)
        {
            foreach(var p in projects)
            {
                var destFile = GetFileDestPath(p.ProjectFilePath, destDir);

            }
        }

        static void GenerateUpgrades(IEnumerable<ProjectMeta> projects)
        {
            // upgrade if --generate | -g
            bool copySolution = false;
            string newSlnPath = string.Empty;
            string slnSrc = string.Empty;

            var projectUpgrader = new UpgradeProjectToVs2017();

            // now upgrade .csproj where suited
            foreach (var project in projects)
            {
                var newProjFile = projectUpgrader.UpgradeProject(project.ProjectFilePath);
                if (!string.IsNullOrEmpty(project.BelongsToSolutionFile)
                    && !copySolution)
                {
                    slnSrc = project.BelongsToSolutionFile;
                    var newSlnFileName = Path.GetFileNameWithoutExtension(project.BelongsToSolutionFile) + "_upgraded.sln";
                    var slnDir = Path.GetDirectoryName(Path.GetDirectoryName(newProjFile));
                    newSlnPath = Path.Combine(slnDir, newSlnFileName);
                    copySolution = true;
                }
            }

            if (!string.IsNullOrEmpty(slnSrc))
            {
                // has a sln 
                var slnText = File.ReadAllText(slnSrc);
                //slnText = slnText.Replace(".csproj", "_upgraded.csproj");
                File.WriteAllText(newSlnPath, slnText);
                Console.WriteLine("new sln " + newSlnPath);
            }
        }
      
    }
}