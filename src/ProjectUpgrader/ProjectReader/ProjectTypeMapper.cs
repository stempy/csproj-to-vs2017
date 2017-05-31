using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using ProjectUpgrader.Models;

namespace ProjectUpgrader.ProjectReader
{
    public enum ProjectOutputType
    {
        Exe,
        Library
    }


    public class ProjectTypesMapper
    {
        private static Dictionary<Guid, string> _projectTypeDictionary;


        public string GetProjectTypeDescription(Guid projectTypeGuid)
        {
            return _projectTypeDictionary.ContainsKey(projectTypeGuid) ? _projectTypeDictionary[projectTypeGuid] : null;
        }

        public ProjectType GetProjectType(Guid projectTypeGuid, ProjectOutputType projectOutputType=ProjectOutputType.Library)
        {
            var pDesc = GetProjectTypeDescription(projectTypeGuid);
            var isMvc = pDesc?.ToLower().Contains("mvc");

            switch (projectOutputType)
            {
                case ProjectOutputType.Exe:
                    return ProjectType.LegacyConsole;
            }

            return isMvc!=null && isMvc==true ? ProjectType.LegacyMvcApplication : ProjectType.LegacyClassLibrary;
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

                var asm = Assembly.GetEntryAssembly();
                var asmName = asm.GetName().Name;
                var resName = $"{asmName}.visual_studio_project_type_guids_list.csv";

                var resStream =
                    asm.GetManifestResourceStream(resName);

                


                string[] lines;
                using (var reader = new StreamReader(resStream, Encoding.UTF8))
                {
                    var res = reader.ReadToEnd();
                    lines = res.Split('\n');
                }
                var ptypesContent = lines.Select(y=>y.TrimEnd(new char[]{'\r'}));

                //var ptypesContent = File.ReadLines(@"visual_studio_project_type_guids_list.csv");
                foreach (var line in ptypesContent)
                {
                    if (!string.IsNullOrWhiteSpace(line))
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
}
