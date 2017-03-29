using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace CsProjToVs2017Upgrader
{
    public class ProjectTypesMapper
    {
        private static Dictionary<Guid, string> _projectTypeDictionary;


        public string GetProjectTypeDescription(Guid projectTypeGuid)
        {
            return _projectTypeDictionary.ContainsKey(projectTypeGuid) ? _projectTypeDictionary[projectTypeGuid] : null;
        }

        public Guid GetProjectTypeGuid(string projectTypeString)
        {
            return _projectTypeDictionary.ContainsValue(projectTypeString)
                ? _projectTypeDictionary.FirstOrDefault(x => x.Value == projectTypeString).Key
                : Guid.Empty;
        }

        public ProjectTypesMapper()
        {
            if (_projectTypeDictionary == null)
            {
                _projectTypeDictionary = new Dictionary<Guid, string>();
                var ptypesContent = File.ReadLines(@"visual_studio_project_type_guids_list.csv");
                foreach (var line in ptypesContent)
                {
                    var lineArr = line.Split(',');
                    var desc = lineArr[0];
                    var guidStr = lineArr[1];
                    if (!string.IsNullOrEmpty(guidStr))
                    {
                        var g = Guid.Parse(guidStr);
                        if (!_projectTypeDictionary.ContainsKey(g))
                            _projectTypeDictionary.Add(g, desc);
                    }
                }
            }
        }
    }
}
