using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using ProjectUpgrader.Models;
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

            InitServices();

            bool generateUpgrades = false;
            bool upgradeReferences = false;
            bool overwriteSource=false;
            ProcessCommandLineArgs(ref args, ref generateUpgrades, ref upgradeReferences, ref overwriteSource);

            foreach (var arg in args)
            {
                // 1. analyze projects
                var projectsAnalyzed = _projectAnalyzer.AnalyzeProjectsPath(arg);
                var isSln = arg.EndsWith(".sln") && projectsAnalyzed.FirstOrDefault().BelongsToSolutionFile==arg;
                var slnFile = isSln? arg:null;


                // -u upgrade references only
                if (upgradeReferences)
                {
                    var dir = PathHelpers.GetDestDir();
                    UpgradeReferences(dir, projectsAnalyzed,slnFile, overwriteSource);
                } else
                {
                    // -g generate
                    if (generateUpgrades)
                    {
                        GenerateUpgrades(projectsAnalyzed,slnFile);
                    }
                }
            }

            // finished
            if (Debugger.IsAttached)
            {
                Console.WriteLine("Press any key to continute.");
                Console.ReadKey();
            }
        }

        private static void InitServices()
        {
            ILoggerFactory loggerFactory = new LoggerFactory()
                .AddConsole()
                .AddDebug();

            _refUpgrader = new ProjectToVs2017ReferenceOnlyUpgrader(loggerFactory.CreateLogger<ProjectToVs2017ReferenceOnlyUpgrader>());
            _projectAnalyzer = new ProjectAnalyzer(loggerFactory.CreateLogger<ProjectAnalyzer>());
        }

        private static void ProcessCommandLineArgs(ref string[] args, ref bool generateUpgrades, ref bool upgradeReferences, ref bool overwriteSource)
        {
            var gen = SimpleCommandParser.ParseOption("g", "generate",ref args);
            var upd = SimpleCommandParser.ParseOption("u", "upgraderefs", ref args);
            var overwriteSourceOpt = SimpleCommandParser.ParseOption("s", "source", ref args);



            upgradeReferences = !string.IsNullOrEmpty(upd);
            generateUpgrades = !string.IsNullOrEmpty(gen);
            overwriteSource = !string.IsNullOrEmpty(overwriteSourceOpt);


            if (upgradeReferences && generateUpgrades)
            {
                Console.WriteLine("Error: unable to use both upgrade (-u) and generate (-g) switches");

                if (Debugger.IsAttached)
                {
                    Console.ReadKey();
                }

                Environment.Exit(1);
            }
        }

        static void Usage()
        {
            Console.WriteLine("Old Csproj to VS2017 Upgrader\n=============================\nUsage:");


            Console.WriteLine("\tCsProjToVs2017Upgrader [-g|--generate | -u|-upgraderefs] [-s|--source] slnfile1.sln slnfile2.sln projectfile.csproj projectfile2.csproj");
            Console.WriteLine("\t\t-g|--generate   Generate new .NetStandard csproj files.");
            Console.WriteLine("\t\t-u|--upgraderef   Just update legacy csproj nuget packages to new VS2017 packagereference format.");
            Console.WriteLine("\t\t-s|--source   Overwrite Source csproj files...WARNING this modifies source .csproj files and packages.config files.");
            Console.WriteLine();
        }

        static void UpgradeReferences(string destDir, IEnumerable<ProjectMeta> projects, string slnFile, bool overwriteSource)
        {
            if (!overwriteSource)
                destDir = CopySlnFile(slnFile, destDir);

            // now process csproj files and save to destination instead of overwriting
            foreach (var p in projects)
            {
                var destProjFile = overwriteSource? p.ProjectFilePath
                                        : PathHelpers.GetFileDestPath(p.ProjectFilePath, destDir);

                _refUpgrader.UpgradeProjectFile(p.ProjectFilePath, overwriteSource, destProjFile);
            }
        }

        static void GenerateUpgrades(IEnumerable<ProjectMeta> projects, string slnFile)
        {
            var projectUpgrader = new UpgradeProjectToVs2017();
            var destDir = PathHelpers.GetDestDir();
            destDir = Path.Combine(destDir, "FullUpgrades");

            // copy sln file to destination
            destDir=CopySlnFile(slnFile, destDir);

            // now upgrade .csproj where suited
            foreach (var project in projects)
            {
                var projDir = Path.Combine(destDir, PathHelpers.GetDirectoryAsFileName(project.ProjectFilePath));
                var projFileDest = Path.Combine(projDir, Path.GetFileName(project.ProjectFilePath));
                PathHelpers.CreateDirIfNotExist(projDir);
                var newProjFile = projectUpgrader.UpgradeProject(project.ProjectFilePath, projFileDest);
                
            }
        }


        static string CopySlnFile(string slnFile, string destDir)
        {
            // copy sln file to destination
            if (!string.IsNullOrEmpty(slnFile))
            {
                destDir = Path.Combine(destDir, Path.GetFileName(slnFile));
                PathHelpers.CreateDirIfNotExist(destDir);
                File.Copy(slnFile, Path.Combine(destDir, Path.GetFileName(slnFile)), true);
            }
            else
            {
                PathHelpers.CreateDirIfNotExist(destDir);
            }

            return destDir;
        }

      
    }
}