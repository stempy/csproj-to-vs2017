namespace ProjectUpgrader.Upgraders
{
    public interface IProjectToVs2017ReferenceOnlyUpgrader
    {
        void UpgradeProjectFile(string srcProjectFile, bool allowOverwrite, string destProjectFile = null);
    }
}