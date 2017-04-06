using CsProjToVs2017Upgrader.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ProjectUpgrader.ProjectHelpers
{
    public class ProjectHelpers
    {
        public static IDictionary<string, List<string>> GroupReferencesByProjects(IEnumerable<ProjectMeta> meta)
        {
            return meta.ToDictionary(y => y.ProjectFilePath, x => x.ProjectReferences.Select(r => r.Name).ToList());
        }

        public static IDictionary<string, List<string>> GroupProjectsByReferences(IEnumerable<ProjectMeta> meta)
        {
            var allRefs = meta.SelectMany(u => u.ProjectReferences.Select(r => r.Name))
                              .Distinct()
                              .ToArray();

            var d = new Dictionary<string, List<string>>();
            foreach (var a in allRefs)
            {
                d.Add(a, new List<string>());
                d[a].AddRange(meta.Where(p => p.ProjectReferences.Any(r=>r.Name==a)).Select(u => u.ProjectFilePath));
            }
            return d;
        }
    }
}
