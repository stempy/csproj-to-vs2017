using ProjectUpgrader.Upgraders;
using System;

namespace CsProjToVs2017Upgrader
{
    public class UpgradeProjectToVs2017
    {
        IProjectToVs2017ProjectUpgrader _projectUpgrader;

        public UpgradeProjectToVs2017()
        {
            _projectUpgrader = new ProjectToVs2017ProjectUpgrader();
        }

        public string UpgradeProject(string projFile, string projFileDest)
        {
            try
            {
                return _projectUpgrader.UpgradeProjectFile(projFile, projFileDest);
            } catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return string.Empty;
        }
    }
}
