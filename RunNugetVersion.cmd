@echo off
SETLOCAL
call "%~dp0_setPublishOutput.cmd" NugetVersion
if not exist "%publishOutputExe%" call "%~dp0BuildNugetVersionExe.cmd"
"%publishOutputExe%" %*
ENDLOCAL
