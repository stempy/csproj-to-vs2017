using System.Collections.Generic;
using ProjectUpgrader.Models;

namespace ProjectUpgrader.SolutionReader
{
    public interface ISolutionReader
    {
        IEnumerable<ProjectMeta> ParseProjectsInSolution(string slnFile);
    }
}