@echo off
set thisDir=%~dp0
set thisDir=%thisDir:~0,-1%
rem Run (build exe if needed)
if not exist "%thisDir%\bin\release\publishoutput\CsProjToVs2017Upgrader.exe" call "%thisDir%\BuildExe.cmd"

pushd "%thisDir%\bin\release\publishoutput"
CsProjToVs2017Upgrader.exe %*
popd
