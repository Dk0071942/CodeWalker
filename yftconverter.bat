@echo off
setlocal

:: YFT Converter - Uncompressed to Compressed YFT
:: Usage: yftconverter.bat [build|test|clean|all] [debug|release]

set command=%1
if "%command%"=="" set command=all

set config=%2
if "%config%"=="" set config=debug

echo ==================================================================================
echo YFT Converter - Uncompressed YFT to Compressed YFT/XML Converter
echo ==================================================================================
echo Command: %command%
echo Configuration: %config%
echo.

if /i "%command%"=="build" goto build
if /i "%command%"=="test" goto test
if /i "%command%"=="clean" goto clean
if /i "%command%"=="all" goto all
goto usage

:all
echo Executing all steps...
echo.
call :clean
call :build
if errorlevel 1 goto error
call :test
goto end

:build
echo Building YFT Converter (%config% configuration)...
echo.

:: Check if dotnet is available
where dotnet >nul 2>&1
if errorlevel 1 (
    echo Using MSBuild instead of dotnet...
    msbuild CodeWalker.YftConverter\CodeWalker.YftConverter.csproj /p:Configuration=%config% /verbosity:minimal
) else (
    cd CodeWalker.YftConverter
    echo [1/3] Restoring packages...
    dotnet restore
    
    echo [2/3] Building project...
    dotnet build -c %config% --no-restore
    cd ..
)

if errorlevel 1 (
    echo ERROR: Build failed
    goto error
)
echo.
echo Build completed successfully
goto :eof

:test
echo Running YFT Converter tests...
echo.

set exe=CodeWalker.YftConverter\bin\%config%\net48\YftConverter.exe
if not exist "%exe%" (
    echo ERROR: YftConverter.exe not found. Run 'yftconverter.bat build' first.
    goto error
)

:: Test with the locked.yft file
set testfile=CodeWalker.YftConverter\locked.yft
set outputfile_gen8=CodeWalker.YftConverter\locked_compressed_gen8.yft
set outputfile_gen9=CodeWalker.YftConverter\locked_compressed_gen9.yft
set outputfile_xml=CodeWalker.YftConverter\locked.xml
set resourcefolder=CodeWalker.YftConverter\locked_resources

if exist "%testfile%" (
    echo ==================================================================================
    echo Test 1: YFT to YFT conversion (Gen8 - default)
    echo ==================================================================================
    echo Command: %exe% "%testfile%" "%outputfile_gen8%" -v
    echo.
    "%exe%" "%testfile%" "%outputfile_gen8%" -v
    if errorlevel 1 (
        echo.
        echo ERROR: Gen8 conversion failed
        goto error
    )
    echo.
    echo Test 1 completed successfully
    if exist "%outputfile_gen8%" (
        echo Output file created: %outputfile_gen8%
    )
    echo.
    
    echo ==================================================================================
    echo Test 2: YFT to YFT conversion (Gen9)
    echo ==================================================================================
    echo Command: %exe% "%testfile%" "%outputfile_gen9%" -g gen9 -v
    echo.
    "%exe%" "%testfile%" "%outputfile_gen9%" -g gen9 -v
    if errorlevel 1 (
        echo.
        echo ERROR: Gen9 conversion failed
        goto error
    )
    echo.
    echo Test 2 completed successfully
    if exist "%outputfile_gen9%" (
        echo Output file created: %outputfile_gen9%
    )
    echo.
    
    echo ==================================================================================
    echo Test 3: YFT to XML conversion
    echo ==================================================================================
    echo Command: %exe% "%testfile%" "%outputfile_xml%" -f xml -r "%resourcefolder%" -v
    echo.
    "%exe%" "%testfile%" "%outputfile_xml%" -f xml -r "%resourcefolder%" -v
    if errorlevel 1 (
        echo.
        echo ERROR: XML conversion failed
        goto error
    )
    echo.
    echo Test 3 completed successfully
    if exist "%outputfile_xml%" (
        echo Output file created: %outputfile_xml%
        for %%I in ("%outputfile_xml%") do echo XML file size: %%~zI bytes
    )
    echo.
    
    echo ==================================================================================
    echo All tests completed successfully!
    echo ==================================================================================
    echo.
    echo Output files:
    if exist "%outputfile_gen8%" echo   - %outputfile_gen8%
    if exist "%outputfile_gen9%" echo   - %outputfile_gen9%
    if exist "%outputfile_xml%" echo   - %outputfile_xml%
    if exist "%resourcefolder%" echo   - %resourcefolder% (resources folder)
) else (
    echo WARNING: Test file locked.yft not found
    echo Place an uncompressed YFT file at: %testfile%
)
goto :eof

:clean
echo Cleaning build output...
if exist CodeWalker.YftConverter\bin rmdir /s /q CodeWalker.YftConverter\bin
if exist CodeWalker.YftConverter\obj rmdir /s /q CodeWalker.YftConverter\obj
echo Clean completed
goto :eof

:usage
echo Usage: yftconverter.bat [command] [configuration]
echo.
echo Commands:
echo   build          - Build the YFT converter
echo   test           - Test all conversion modes (YFT Gen8, Gen9, and XML)
echo   clean          - Clean build output
echo   all            - Clean, build, and test (default)
echo.
echo Configuration:
echo   debug          - Debug build (default)
echo   release        - Release build
echo.
echo The test command will run three conversions:
echo   1. Uncompressed to compressed YFT (Gen8)
echo   2. Uncompressed to compressed YFT (Gen9)
echo   3. Uncompressed to XML format
echo.
echo Example:
echo   yftconverter.bat build release
echo   yftconverter.bat test
goto end

:error
echo.
echo ==================================================================================
echo Script failed with errors
echo ==================================================================================
pause
exit /b 1

:end
echo.
echo ==================================================================================
echo Script completed successfully
echo ==================================================================================
pause