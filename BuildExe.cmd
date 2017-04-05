@echo off
set thisDir=%~dp0
set thisDir=%thisDir:~0,-1%
rem publish to local self contained exe file 

pushd "%thisDir%\src\CsProjToVs2017Upgrader"
dotnet restore
dotnet publish -c Release -r win10-x64 -o bin\Release\publishoutput
popd
