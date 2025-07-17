@echo off
rem Example script to merge the two example DLCs

set EXAMPLE_DIR=example_dlcs
set DLC1=%EXAMPLE_DIR%\0dd32d-T72B3M_Arena\t72b3m\dlc.rpf
set DLC2=%EXAMPLE_DIR%\31dc8a-M113A1\m113a1\dlc.rpf
set OUTPUT=merged_vehicles.rpf

echo Merging example DLC files...
echo ============================
echo.
echo NEW: Enhanced merger with debugging features!
echo.
echo Method 1: Merge specific files
echo Input files:
echo   1. %DLC1%
echo   2. %DLC2%
echo.
echo Method 2: Merge entire directory (recommended)
echo Input directory: %EXAMPLE_DIR%
echo.
echo Output file: %OUTPUT%
echo.
echo === RECOMMENDED COMMANDS ===
echo.
echo 1. Dry run with structure analysis:
echo DLCMerger -i "%EXAMPLE_DIR%" -o "%OUTPUT%" -v -d -s
echo.
echo 2. Full merge with debugging:
echo DLCMerger -i "%EXAMPLE_DIR%" -o "%OUTPUT%" -v -s -f
echo.
echo 3. Individual file merge:
echo DLCMerger -i "%DLC1%" -i "%DLC2%" -o "%OUTPUT%" -v -s -f
echo.
echo === RUNNING RECOMMENDED COMMAND ===
echo.
echo Running: DLCMerger -i "%EXAMPLE_DIR%" -o "%OUTPUT%" -v -s -d
echo.

DLCMerger -i "%EXAMPLE_DIR%" -o "%OUTPUT%" -v -s -d

echo.
echo === Test completed ===
pause