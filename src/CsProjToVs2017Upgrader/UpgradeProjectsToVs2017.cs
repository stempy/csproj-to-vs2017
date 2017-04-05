using ProjectUpgrader.Upgraders;
using System;

namespace CsProjToVs2017Upgrader
{
    /// <summary>
    /// Update just references in a project
    /// </summary>
    public class UpgradeProjectReferencesToVs2017
    {
        public UpgradeProjectReferencesToVs2017()
        {
            
        }

        public string UpgradeProject(string projFile)
        {
            try
            {
                //return _projectUpgrader.UpgradeProjectFile(projFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return string.Empty;
        }

    }


    public class UpgradeProjectToVs2017
    {
        IProjectToVs2017ProjectUpgrader _projectUpgrader;

        public UpgradeProjectToVs2017()
        {
            _projectUpgrader = new ProjectToVs2017ProjectUpgrader();
        }

        public string UpgradeProject(string projFile)
        {
            try
            {
                return _projectUpgrader.UpgradeProjectFile(projFile);
            } catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return string.Empty;
        }
    }
}
