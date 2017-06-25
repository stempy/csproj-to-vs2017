@echo off
rem publish to local self contained exe file 
SETLOCAL
set thisDir=%~dp0
set thisDir=%thisDir:~0,-1%
set projDir=%~1
set projName=%2
rem call "%thisDir%\CleanBinObj.cmd"
call "%thisDir%\_setPublishOutput.cmd" %projName%
if exist "%publishOutputDir%" rd /q/s "%publishOutputDir%"
rem ------------ now do it
pushd "%projDir%"
call dotnet restore
call dotnet publish -c Release -r win10-x64 -o %publishOutputDir%
popd
ENDLOCAL
