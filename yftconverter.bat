@echo off
setlocal

:: YFT Converter - Uncompressed to Compressed YFT
:: Usage: yftconverter.bat [build|test|clean|all] [debug|release]

set command=%1
if "%command%"=="" set command=all

set config=%2
if "%config%"=="" set config=debug

echo ==================================================================================
echo YFT Converter - Uncompressed to Compressed YFT
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
echo Running YFT Converter test...
echo.

set exe=CodeWalker.YftConverter\bin\%config%\net48\YftConverter.exe
if not exist "%exe%" (
    echo ERROR: YftConverter.exe not found. Run 'yftconverter.bat build' first.
    goto error
)

:: Test with the locked.yft file
set testfile=CodeWalker.YftConverter\locked.yft
set outputfile=CodeWalker.YftConverter\bin\%config%\net48\locked_compressed.yft

if exist "%testfile%" (
    echo Testing conversion: locked.yft to locked_compressed.yft
    echo Command: %exe% "%testfile%" "%outputfile%" -v
    echo.
    "%exe%" "%testfile%" "%outputfile%" -v
    if errorlevel 1 (
        echo.
        echo ERROR: Conversion failed
        goto error
    )
    echo.
    echo Test completed successfully
    if exist "%outputfile%" (
        echo Output file created: %outputfile%
    )
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
echo   test           - Test conversion with locked.yft
echo   clean          - Clean build output
echo   all            - Clean, build, and test (default)
echo.
echo Configuration:
echo   debug          - Debug build (default)
echo   release        - Release build
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