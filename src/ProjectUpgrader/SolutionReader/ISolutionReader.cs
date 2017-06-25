using System.Collections.Generic;
using ProjectUpgrader.Models;

namespace ProjectUpgrader.SolutionReader
{
    public interface ISolutionReader
    {
        IEnumerable<string> GetProjectFilesInSolution(string slnFile);
        IEnumerable<ProjectMeta> ParseProjectsInSolution(string slnFile);
    }
}