@echo off
SETLOCAL
set thisDir=%~dp0
set thisDir=%thisDir:~0,-1%
set projDir=%~1
set outDir=%projDir%\bin\Release\publishoutput
call "%thisDir%\CleanBinObj.cmd"
if exist "%outDir%" rd /q/s "%outDir%"

rem publish to local self contained exe file 

pushd "%projDir%"
dotnet restore
dotnet publish -c Release -r win10-x64 -o %outDir%
popd
ENDLOCAL
