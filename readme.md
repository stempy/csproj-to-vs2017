# CsProj to VS 2017 CsProj

This project is designed to convert legacy C# projects (basic class libaries) to the new leaner VS2017 supported .csproj types.

To build exe for windows, run

```sh
src\CsProjToVs2017Upgrader\BuildExe.cmd
```

Usage:

```sh
src\CsProjToVs2017Upgrader\bin\Release\publishoutput\CsProjToVs2017Upgrader.exe [-g|--generate | -u|--upgraderef] "solutionfile1.sln" "solutionfile2.sln" "projectfile.csproj" [sln3,sln4,...]

or shortcut

src\CsProjToVs2017Upgrader\run.cmd [-g|--generate | -u|--upgraderef] "solutionfile1.sln" "solutionfile2.sln" "projectfile.csproj" [sln3,sln4,...]
``` 

Just specifying the solution files will just anaylize project to determine binary/project/nuget references. Currently outputs to folder(s) in %TEMP% dir.

* `-g|--generate` - Generate completely new csproj files for .NETStandard approach. see http://www.natemcmaster.com/blog/2017/03/09/vs2015-to-vs2017-upgrade/
* `-u|--upgradered` - Upgrade legacy project nuget references to package reference format. see (MSBuild PackageReference post)<http://blog.nuget.org/20170316/NuGet-now-fully-integrated-into-MSBuild.html?utm_content=buffera4d96&utm_medium=social&utm_source=twitter.com&utm_campaign=buffer> for more

