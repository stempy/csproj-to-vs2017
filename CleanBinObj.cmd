@echo off
set thisDir=%~dp0
set thisDir=%thisDir:~0,-1%
set oldDir=%CD%
echo Clean all bin obj logs _Cache directories
echo ---------------------------------------------------------

rem ------------------ loop through all dirs and subdirs ---------------------------
pushd "%thisDir%"
for /d /r %%d in (*) do call :cleanproj %%d
goto :end

rem --------------- if has .csproj in this dir than process ------------------------
:cleanproj
set folder=%1
SETLOCAL
if NOT exist "%folder%\*.csproj" exit /B
rem for /f "delims=" %%A in ('dir *.csproj /b') do set "CsProjFile=%%A"
rem for /f %%A in ("%CsProjFile%") do set "CsProjFileOnly=%%~nA"
call :cleanBin %folder%
ENDLOCAL
exit /B

rem --------------- clean actual bin obj cache logs etc ---------------------------
:cleanBin
if NOT exist "%1" exit /B
echo Cleaning %1....
pushd "%1"
for /d /r %%A in (bin,obj,logs,_Cache) do @if exist "%%A" (
    echo        -----^>^> "%%A"
    if exist "%%A" rd /s/q "%%A"
) 
popd
exit /B

rem --------------- END ----------------------------
:end
popd