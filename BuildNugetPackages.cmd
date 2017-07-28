@echo off
SETLOCAL
set thisDir=%~dp0
set thisDir=%thisDir:~0,-1%
set buildDir=%thisDir%\Build
if NOT DEFINED PackageVersion set PackageVersion=1.0.5
if NOT exist "%NuGet%" echo Nuget var not set & exit /b 1
if NOT exist "%buildDir%" md "%buildDir%"

if "%config%"=="" set config=Release
set version=
rem build actual executables
call "%thisDir%\BuildExe.cmd"
call "%thisDir%\BuildNugetVersionExe.cmd"

call :packProj "%thisDir%\src\CsProjToVs2017Upgrader"
call :packProj "%thisDir%\src\NugetVersion"

exit /b


:packProj
SETLOCAL
set projDir=%~1
for /f %%a in ("%projDir%") do set projName=%%~na
set projNuSpec=%projName%.nuspec
set version=
if NOT "%PackageVersion%"=="" (
    set version=-Version %PackageVersion%
)
if NOT exist "%projDir%\%projNuSpec%" echo No nuspec found for "%projDir%\%projNuSpec%" && exit /b 1
echo config=%config%
rem pack it
pushd "%projDir%"
call "%NuGet%" pack %projNuSpec% -o "%buildDir%" -NonInteractive %version% -Properties Configuration="%config%"
popd
exit /b %errorlevel%

ENDLOCAL
