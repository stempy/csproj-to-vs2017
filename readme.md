# CsProj to VS 2017 CsProj

This project is designed to convert legacy C# projects (basic class libaries) to the new leaner VS2017 supported .csproj types.

To build exe for windows, run

```sh
src\CsProjToVs2017Upgrader\BuildExe.cmd
```

Usage:

```sh
src\CsProjToVs2017Upgrader\bin\Release\publishoutput\CsProjToVs2017Upgrader.exe [-g|--generate] "solutionfile1.sln" "solutionfile2.sln" "projectfile.csproj" [sln3,sln4,...]
``` 

Just specifying the solution files will just anaylize project to determine binary/project/nuget references, specify  `-g|--generate` option to actually generate results. Currently output to "%TEMP" directory.

