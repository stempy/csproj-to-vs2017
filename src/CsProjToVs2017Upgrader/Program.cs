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

            InitServices();

            bool generateUpgrades = false;
            bool upgradeReferences = false;

            ProcessCommandLineArgs(ref args, ref generateUpgrades, ref upgradeReferences);

            foreach (var arg in args)
            {
                // 1. analyze projects
                var projectsAnalyzed = _projectAnalyzer.AnalyzeProjectsPath(arg);
                var isSln = arg.EndsWith(".sln") && projectsAnalyzed.FirstOrDefault().BelongsToSolutionFile==arg;
                var slnFile = isSln? arg:null;


                // -u upgrade references only
                if (upgradeReferences)
                {
                    var dir = GetDestDir();
                    UpgradeReferences(dir, projectsAnalyzed,slnFile);
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

        private static void ProcessCommandLineArgs(ref string[] args, ref bool generateUpgrades, ref bool upgradeReferences)
        {
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
        }

        static void Usage()
        {
            Console.WriteLine("Old Csproj to VS2017 Upgrader\n=============================\nUsage:");


            Console.WriteLine("\tCsProjToVs2017Upgrader [-g|--generate | -u|-upgraderefs] slnfile1.sln slnfile2.sln projectfile.csproj projectfile2.csproj");
            Console.WriteLine("\t\t-g|--generate   Generate new .NetStandard csproj files.");
            Console.WriteLine("\t\t-u|--upgraderef   Just update legacy csproj nuget packages to new VS2017 packagereference format.");
            Console.WriteLine();
        }


        static string GetDestDir(string relativePath="")
        {
            var destDir = Path.Combine(Path.GetTempPath()+Path.DirectorySeparatorChar, "VS2017_Upgrade");
            CreateDirIfNotExist(destDir);
            return destDir;
        }

        static string GetFileDestPath(string srcProjPath, string destDir)
        {
            var projDirName = Path.GetFileName( Path.GetDirectoryName(srcProjPath));
            var projDestDir = Path.Combine(destDir, projDirName);
            var fullPath= Path.Combine(projDestDir, Path.GetFileName(srcProjPath));
            CreateDirIfNotExist(Path.GetDirectoryName(fullPath));
            return fullPath;
        }

        static string GetDirectoryAsFileName(string fullPath)
        {
            return Path.GetFileName(Path.GetDirectoryName(fullPath));
        }

        static void CreateDirIfNotExist(string dirPath)
        {
            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);
        }


        static void UpgradeReferences(string destDir, IEnumerable<ProjectMeta> projects, string slnFile)
        {
            // copy sln file to destination
            if (!string.IsNullOrEmpty(slnFile))
            {
                destDir = Path.Combine(destDir, Path.GetFileName(slnFile));
                CreateDirIfNotExist(destDir);
                File.Copy(slnFile, Path.Combine(destDir, Path.GetFileName(slnFile)));
            }

            // now process csproj files and save to destination instead of overwriting
            foreach (var p in projects)
            {
                var destProjFile = GetFileDestPath(p.ProjectFilePath, destDir);
                _refUpgrader.UpgradeProjectFile(p.ProjectFilePath, false, destProjFile);
            }
        }


        static void CopySlnFile(string slnFile,string destDir)
        {

        }

        static void GenerateUpgrades(IEnumerable<ProjectMeta> projects, string slnFile)
        {
            var destDir = GetDestDir();
            destDir = Path.Combine(destDir, "FullUpgrades");


            // copy sln file to destination
            if (!string.IsNullOrEmpty(slnFile))
            {
                destDir = Path.Combine(destDir, Path.GetFileName(slnFile));
                CreateDirIfNotExist(destDir);
                File.Copy(slnFile, Path.Combine(destDir, Path.GetFileName(slnFile)));
            } else
            {
                CreateDirIfNotExist(destDir);
            }
            


            // upgrade if --generate | -g
            bool copySolution = false;
            string newSlnPath = string.Empty;
            string slnSrc = string.Empty;

            var projectUpgrader = new UpgradeProjectToVs2017();

            // now upgrade .csproj where suited
            foreach (var project in projects)
            {
                var projDir = Path.Combine(destDir, GetDirectoryAsFileName(project.ProjectFilePath));
                var projFileDest = Path.Combine(projDir, Path.GetFileName(project.ProjectFilePath));
                CreateDirIfNotExist(projDir);
                var newProjFile = projectUpgrader.UpgradeProject(project.ProjectFilePath, projFileDest);
                if (!string.IsNullOrEmpty(slnFile)
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