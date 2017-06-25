@echo off
SETLOCAL
call "%~dp0_setPublishOutput.cmd" CsProjToVs2017Upgrader
if not exist "%publishOutputExe%" call "%~dp0BuildExe.cmd"
"%publishOutputExe%" %*
ENDLOCAL
