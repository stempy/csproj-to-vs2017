using System.IO;

namespace CsProjToVs2017Upgrader
{
    internal static class PathHelpers
    {
        public static string GetTempDestDir(string relativePath = "")
        {
            var destDir = Path.Combine(Path.GetTempPath() + Path.DirectorySeparatorChar, "VS2017_Upgrade");
            CreateDirIfNotExist(destDir);
            return destDir;
        }

        public static string GetFileDestPath(string srcProjPath, string destDir)
        {
            var projDirName = Path.GetFileName(Path.GetDirectoryName(srcProjPath));
            var projDestDir = Path.Combine(destDir, projDirName);
            var fullPath = Path.Combine(projDestDir, Path.GetFileName(srcProjPath));
            CreateDirIfNotExist(Path.GetDirectoryName(fullPath));
            return fullPath;
        }

        public static string GetDirectoryAsFileName(string fullPath)
        {
            return Path.GetFileName(Path.GetDirectoryName(fullPath));
        }

        public static void CreateDirIfNotExist(string dirPath)
        {
            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);
        }
    }
}