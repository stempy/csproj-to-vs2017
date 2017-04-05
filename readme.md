# CsProj to VS 2017 CsProj

This project is designed to convert legacy C# projects (basic class libaries) to the new leaner VS2017 supported .csproj types.

To build exe for windows, run

```sh
BuildExe.cmd
```

Usage:

shortcut
```sh
run.cmd [-g|--generate | -u|--upgraderef] "solutionfile1.sln" "solutionfile2.sln" "projectfile.csproj" [sln3,sln4,...]
```

Or actual exe that is built
```sh
src\CsProjToVs2017Upgrader\bin\Release\publishoutput\CsProjToVs2017Upgrader.exe [-g|--generate | -u|--upgraderef] "solutionfile1.sln" "solutionfile2.sln" "projectfile.csproj" [sln3,sln4,...]
``` 

Just specifying the solution or project files will just analyze solution projects to determine binary/project/nuget references. Currently outputs to folder(s) in %TEMP% dir.

There are two approaches:

1. `-u|--upgraderef` Upgrade just the nuget references that are referenced as binary dlls using `<Reference>` element in .csproj, and convert them to `<PackageReference>` elements in csproj, and remove `packages.config`, This is least destructive, and should work fine on class library projects and projects referencing them. see (MSBuild PackageReference post)<http://blog.nuget.org/20170316/NuGet-now-fully-integrated-into-MSBuild.html?utm_content=buffera4d96&utm_medium=social&utm_source=twitter.com&utm_campaign=buffer> for more information.

2. `-g|--generate` Generate completely new csproj files for .NETStandard approach. This will require more work. see http://www.natemcmaster.com/blog/2017/03/09/vs2015-to-vs2017-upgrade/

