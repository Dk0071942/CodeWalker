@echo off
REM Build script for CodeWalker YFT Converter
REM This script builds the YftConverter project in Release mode

echo ==========================================
echo Building CodeWalker YFT Converter
echo ==========================================
echo.

REM Check if MSBuild is available
where msbuild >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: MSBuild not found in PATH
    echo Please install Visual Studio or Build Tools for Visual Studio
    echo Or run this script from Developer Command Prompt
    exit /b 1
)

REM Set build configuration
set Configuration=Release
set Platform=x64
set ProjectPath=CodeWalker.YftConverter\CodeWalker.YftConverter.csproj

REM Restore NuGet packages first
echo Restoring NuGet packages...
nuget restore CodeWalker.sln >nul 2>&1
if %errorlevel% neq 0 (
    echo WARNING: NuGet restore failed. Continuing anyway...
)

REM Build the YftConverter project
echo.
echo Building YftConverter project...
echo Configuration: %Configuration%
echo Platform: %Platform%
echo.

msbuild %ProjectPath% /p:Configuration=%Configuration% /p:Platform=%Platform% /v:minimal /nologo

if %errorlevel% neq 0 (
    echo.
    echo ERROR: Build failed!
    exit /b 1
)

echo.
echo ==========================================
echo Build completed successfully!
echo ==========================================
echo.
echo Output location: CodeWalker.YftConverter\bin\%Configuration%\
echo Executable: "CodeWalker YFT Converter.exe"
echo.

REM Optional: Copy to release folder
set ReleaseDir=Release\YftConverter
if not exist %ReleaseDir% mkdir %ReleaseDir%

echo Copying files to release folder...
xcopy /Y "CodeWalker.YftConverter\bin\%Configuration%\*.exe" %ReleaseDir%\ >nul
xcopy /Y "CodeWalker.YftConverter\bin\%Configuration%\*.dll" %ReleaseDir%\ >nul
xcopy /Y "CodeWalker.YftConverter\bin\%Configuration%\*.config" %ReleaseDir%\ >nul

echo.
echo Files copied to: %ReleaseDir%\
echo.
echo Done!

pause