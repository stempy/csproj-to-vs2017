namespace ProjectUpgrader.Upgraders
{
    public interface IProjectToVs2017ProjectUpgrader
    {
        void UpgradeProjectFile(string srcProjectFile);
        string UpgradeTargetFramework(string oldFwversionString);
    }
}