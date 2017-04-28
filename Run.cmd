@echo off
SETLOCAL
set thisDir=%~dp0
set thisDir=%thisDir:~0,-1%
set outputPath=%thisDir%\src\CsProjToVs2017Upgrader\bin\release\publishoutput\CsProjToVs2017Upgrader.exe
rem Run (build exe if needed)
if not exist "%outputPath%" call "%thisDir%\BuildExe.cmd"
"%outputPath%" %*
ENDLOCAL
