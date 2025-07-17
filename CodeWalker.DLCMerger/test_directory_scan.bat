@echo off
echo Testing directory scanning...
echo.

echo Current directory:
cd

echo.
echo Listing example_dlcs directory:
dir example_dlcs /s /b

echo.
echo Testing DLC Merger with directory input:
echo Command: DLCMerger -i example_dlcs -o test_merged.rpf -v -s -d
echo.

DLCMerger -i example_dlcs -o test_merged.rpf -v -s -d

echo.
echo Test completed.
pause