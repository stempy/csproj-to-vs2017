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

rem build .net core packages
call :buildAndPack "%thisDir%\src\CsProjToVs2017Upgrader"
call :buildAndPack "%thisDir%\src\NugetVersion"
exit /b


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
