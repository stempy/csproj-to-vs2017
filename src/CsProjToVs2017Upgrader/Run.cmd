@echo off
set thisDir=%~dp0
set thisDir=%thisDir:~0,-1%
rem publish to local self contained exe file 

pushd "%thisDir%\bin\release\publishoutput"
CsProjToVs2017Upgrader.exe %*
popd
