using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectUpgrader.Models;

namespace NugetVersion
{
    public class NugetVersionCompareDictionary : Dictionary<string,NugetVersionCompare>
    {
        public void Add(string name)
        {
            base.Add(name,new NugetVersionCompare(){Name = name,ProjectVersionDictionary = new Dictionary<string, string>()});
        }
    }


    public class NugetVersionCompare 
    {
        public string Name { get; set; }
        public IDictionary<string,string> ProjectVersionDictionary { get; set; }

        public IDictionary<string, string> CompareVersions()
        {
            var d = new Dictionary<string,string>();
            var vergrps = ProjectVersionDictionary.GroupBy(u => u.Value).ToDictionary(y=>y.Key,v=>v.Select(i=>i.Key).ToList());
            foreach (var pair in vergrps)
            {
                var key = pair.Key;
                var items = pair.Value;
                //var common = items.Intersect(vergrps)
                // find all common in grps excl this one

            }

            
            return d;
        }
    }
}
