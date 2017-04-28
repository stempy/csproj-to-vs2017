@echo off
set thisDir=%~dp0
set thisDir=%thisDir:~0,-1%
set outDir=%thisDir%\src\CsProjToVs2017Upgrader\bin\Release\publishoutput
call "%thisDir%\CleanBinObj.cmd"
if exist "%outDir%" rd /q/s "%outDir%"

rem publish to local self contained exe file 

pushd "%thisDir%\src\CsProjToVs2017Upgrader"
dotnet restore
dotnet publish -c Release -r win10-x64 -o %outDir%
popd
