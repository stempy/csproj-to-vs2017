@echo off
SETLOCAL
set thisDir=%~dp0
set thisDir=%thisDir:~0,-1%
set buildDir=%thisDir%\Build
set pkgBin=%thisDir%\bin
if NOT DEFINED PackageVersion set PackageVersion=1.0.5
if NOT exist "%NuGet%" echo Nuget var not set & exit /b 1
if NOT exist "%buildDir%" md "%buildDir%"
if NOT exist "%pkgBin%" md "%pkgBin%"
if NOT DEFINED GitVersion set GitVersion=%pkgBin%\GitVersion.CommandLine.3.6.5\tools\gitversion.exe
if "%config%"=="" set config=Release

if not exist "%pkgBin%\gitversion.commandline.*" call :installgitversion
if not exist "%GitVersion%" echo unable to find gitversion & exit /b 1

pushd "%thisDir%"
for /f %%a in ('gitversion /showvariable NuGetVersionV2') do set PackageVersion=%%a

set version=

rem build .net core packages
call :buildAndPack "%thisDir%\src\CsProjToVs2017Upgrader"
call :buildAndPack "%thisDir%\src\NugetVersion"

popd
exit /b


:installgitversion
call "%NuGet%" install GitVersion.CommandLine -Version 3.6.5 -o "%pkgBin%"
exit /b %errorlevel%

:buildAndPack
set pDir=%~1
rem Building %pDir%...
call "%thisDir%\_build.cmd" "%pDir%"
if "%errorlevel%" NEQ "0" goto :fail
rem Creating nuget package...
call :packProj "%pDir%"
exit /b %errorlevel%

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

:fail
exit /b %errorlevel%

ENDLOCAL
