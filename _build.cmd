@echo off
rem publish to local self contained exe file 
SETLOCAL
set thisDir=%~dp0
set thisDir=%thisDir:~0,-1%
set projDir=%~1
for %%a in ("%projDir%") do set projName=%%~na

rem call "%thisDir%\CleanBinObj.cmd"
call "%thisDir%\_setPublishOutput.cmd" %projName%
if exist "%publishCoreDir%" rd /q/s "%publishCoreDir%"
rem ------------ now do it
pushd "%projDir%"
call dotnet restore
call dotnet publish -c Release -f netcoreapp1.1 -o %publishCoreDir%
popd
ENDLOCAL
