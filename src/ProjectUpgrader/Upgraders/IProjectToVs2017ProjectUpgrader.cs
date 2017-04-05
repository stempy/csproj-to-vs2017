﻿namespace ProjectUpgrader.Upgraders
{
    public interface IProjectToVs2017ProjectUpgrader
    {
        string UpgradeProjectFile(string srcProjectFile, string projFileDest = null);
        string UpgradeTargetFramework(string oldFwversionString);
    }
}