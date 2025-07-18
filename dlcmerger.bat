@echo off
setlocal enabledelayedexpansion

:: ==================================================================================
:: DLC Merger - Build and Test Script
:: ==================================================================================
:: Usage: dlcmerger.bat [build|test|all] [debug|release] [options]
:: 
:: Commands:
::   build          - Build the project only
::   test           - Run tests only (requires existing build)
::   all            - Build and test (default)
::   clean          - Clean build artifacts
::   help           - Show this help message
::
:: Build Configurations:
::   debug          - Debug build (default)
::   release        - Release build
::
:: Test Options:
::   -v             - Verbose output
::   -s             - Simplified merger mode
::   -f             - Force overwrite outputs
::   -x             - Extract mode
::   -q             - Quick test (minimal output)
::
:: Examples:
::   dlcmerger.bat                    - Build and test (debug)
::   dlcmerger.bat build release      - Build release version only
::   dlcmerger.bat test -v            - Run verbose tests
::   dlcmerger.bat all release -s -v  - Build release and run simplified verbose tests
:: ==================================================================================

:: Default values
set "COMMAND=all"
set "CONFIG=debug"
set "VERBOSE="
set "SIMPLIFIED="
set "FORCE="
set "EXTRACT="
set "QUICK="
set "PROJECT_DIR=CodeWalker.DLCMerger"
set "BUILD_SUCCESS=0"

:: Parse command line arguments
:parse_args
if "%~1"=="" goto start
if /i "%~1"=="build" set "COMMAND=build" & shift & goto parse_args
if /i "%~1"=="test" set "COMMAND=test" & shift & goto parse_args
if /i "%~1"=="all" set "COMMAND=all" & shift & goto parse_args
if /i "%~1"=="clean" set "COMMAND=clean" & shift & goto parse_args
if /i "%~1"=="help" set "COMMAND=help" & shift & goto parse_args
if /i "%~1"=="debug" set "CONFIG=debug" & shift & goto parse_args
if /i "%~1"=="release" set "CONFIG=release" & shift & goto parse_args
if /i "%~1"=="-v" set "VERBOSE=-v" & shift & goto parse_args
if /i "%~1"=="-s" set "SIMPLIFIED=-s" & shift & goto parse_args
if /i "%~1"=="-f" set "FORCE=-f" & shift & goto parse_args
if /i "%~1"=="-x" set "EXTRACT=-x" & shift & goto parse_args
if /i "%~1"=="-q" set "QUICK=1" & shift & goto parse_args
echo Unknown argument: %~1
shift
goto parse_args

:start
:: Show help if requested
if "%COMMAND%"=="help" goto show_help

:: Display header
echo.
echo ==================================================================================
echo DLC Merger - Build and Test Script
echo ==================================================================================
echo Command: %COMMAND%
echo Configuration: %CONFIG%
if not "%VERBOSE%"=="" echo Verbose: ON
if not "%SIMPLIFIED%"=="" echo Simplified: ON
if not "%FORCE%"=="" echo Force: ON
if not "%EXTRACT%"=="" echo Extract: ON
if not "%QUICK%"=="" echo Quick Test: ON
echo.

:: Validate project directory
if not exist "%PROJECT_DIR%" (
    echo ERROR: Project directory '%PROJECT_DIR%' not found!
    echo Please run this script from the CodeWalker root directory.
    goto error_exit
)

:: Execute requested command
if "%COMMAND%"=="clean" goto clean_build
if "%COMMAND%"=="build" goto build_project
if "%COMMAND%"=="test" goto run_tests
if "%COMMAND%"=="all" goto build_and_test

goto error_exit

:clean_build
echo Cleaning build artifacts...
echo.
cd "%PROJECT_DIR%"
if exist "bin" (
    echo Removing bin directory...
    rd /s /q "bin"
)
if exist "obj" (
    echo Removing obj directory...
    rd /s /q "obj"
)
echo Clean complete!
goto success_exit

:build_project
echo Building DLC Merger (%CONFIG% configuration)...
echo.

:: Change to project directory
cd "%PROJECT_DIR%"

:: Check if dotnet is available
where dotnet >nul 2>&1
if errorlevel 1 (
    echo ERROR: .NET SDK not found! Please install .NET SDK first.
    goto error_exit
)

:: Restore packages
echo [1/3] Restoring packages...
dotnet restore
if errorlevel 1 (
    echo ERROR: Failed to restore packages!
    goto error_exit
)

:: Build project
echo [2/3] Building project...
if /i "%CONFIG%"=="release" (
    dotnet build -c Release --no-restore
) else (
    dotnet build -c Debug --no-restore
)
if errorlevel 1 (
    echo ERROR: Build failed!
    goto error_exit
)

:: Verify build output
echo [3/3] Verifying build output...
if /i "%CONFIG%"=="release" (
    set "BUILD_DIR=bin\Release\net8.0"
) else (
    set "BUILD_DIR=bin\Debug\net8.0"
)

if not exist "%BUILD_DIR%\DLCMerger.exe" (
    echo ERROR: DLCMerger.exe not found in build output!
    goto error_exit
)

set "BUILD_SUCCESS=1"
echo Build completed successfully!
echo Build output: %CD%\%BUILD_DIR%
echo.

if "%COMMAND%"=="build" goto success_exit
goto run_tests

:build_and_test
call :build_project
if errorlevel 1 goto error_exit
goto run_tests

:run_tests
if "%BUILD_SUCCESS%"=="0" (
    echo Checking for existing build...
    cd "%PROJECT_DIR%"
    if /i "%CONFIG%"=="release" (
        set "BUILD_DIR=bin\Release\net8.0"
    ) else (
        set "BUILD_DIR=bin\Debug\net8.0"
    )
    
    if not exist "%BUILD_DIR%\DLCMerger.exe" (
        echo ERROR: No build found! Please build the project first.
        echo Run: dlcmerger.bat build
        goto error_exit
    )
)

echo Running DLC Merger Tests...
echo.

:: Change to build directory
cd "%BUILD_DIR%"

:: Check if example data exists
if not exist "example_dlcs" (
    echo WARNING: example_dlcs directory not found!
    echo Test data may not be available.
)

:: Prepare test command
set "TEST_CMD=DLCMerger.exe -i example_dlcs -o merged_output_test"
if not "%VERBOSE%"=="" set "TEST_CMD=!TEST_CMD! %VERBOSE%"
if not "%SIMPLIFIED%"=="" set "TEST_CMD=!TEST_CMD! %SIMPLIFIED%"
if not "%FORCE%"=="" set "TEST_CMD=!TEST_CMD! %FORCE%"
if not "%EXTRACT%"=="" set "TEST_CMD=!TEST_CMD! %EXTRACT%"

:: Run tests
if "%QUICK%"=="1" (
    echo Running quick test...
    echo Command: !TEST_CMD!
    echo.
    !TEST_CMD!
) else (
    echo Running comprehensive test...
    echo Command: !TEST_CMD!
    echo ==================================================================================
    echo.
    !TEST_CMD!
    echo.
    echo ==================================================================================
)

if errorlevel 1 (
    echo ERROR: Test execution failed!
    goto error_exit
) else (
    echo Test completed successfully!
)

goto success_exit

:show_help
echo.
echo ==================================================================================
echo DLC Merger - Build and Test Script
echo ==================================================================================
echo.
echo Usage: dlcmerger.bat [command] [configuration] [options]
echo.
echo Commands:
echo   build          - Build the project only
echo   test           - Run tests only (requires existing build)
echo   all            - Build and test (default)
echo   clean          - Clean build artifacts
echo   help           - Show this help message
echo.
echo Build Configurations:
echo   debug          - Debug build (default)
echo   release        - Release build
echo.
echo Test Options:
echo   -v             - Verbose output
echo   -s             - Simplified merger mode
echo   -f             - Force overwrite outputs
echo   -x             - Extract mode
echo   -q             - Quick test (minimal output)
echo.
echo Examples:
echo   dlcmerger.bat                    - Build and test (debug)
echo   dlcmerger.bat build release      - Build release version only
echo   dlcmerger.bat test -v            - Run verbose tests
echo   dlcmerger.bat all release -s -v  - Build release and run simplified verbose tests
echo   dlcmerger.bat clean              - Clean build artifacts
echo.
echo ==================================================================================
goto success_exit

:success_exit
echo.
echo ==================================================================================
echo Script completed successfully!
echo ==================================================================================
cd /d "%~dp0"
if not "%QUICK%"=="1" pause
exit /b 0

:error_exit
echo.
echo ==================================================================================
echo Script failed with errors!
echo ==================================================================================
cd /d "%~dp0"
pause
exit /b 1