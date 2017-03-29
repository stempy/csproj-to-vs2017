using System.Collections.Generic;
using CsProjToVs2017Upgrader.Models;

namespace ProjectUpgrader.SolutionReader
{
    public interface ISolutionReader
    {
        IEnumerable<ProjectMeta> ParseProjectsInSolution(string slnFile);
    }
}