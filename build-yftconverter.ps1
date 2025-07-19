# PowerShell build script for CodeWalker YFT Converter
# Provides more control over build process

param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    
    [ValidateSet("x64", "AnyCPU")]
    [string]$Platform = "x64",
    
    [switch]$Clean,
    [switch]$NoBuild,
    [switch]$NoPackage,
    [switch]$Verbose
)

# Function to write colored output
function Write-ColorOutput($message, $color = "White") {
    Write-Host $message -ForegroundColor $color
}

Write-ColorOutput "========================================" "Cyan"
Write-ColorOutput "CodeWalker YFT Converter Build Script" "Cyan"
Write-ColorOutput "========================================" "Cyan"
Write-Host

# Check for MSBuild
$msbuild = Get-Command msbuild -ErrorAction SilentlyContinue
if (-not $msbuild) {
    Write-ColorOutput "ERROR: MSBuild not found in PATH" "Red"
    Write-ColorOutput "Please install Visual Studio or Build Tools for Visual Studio" "Yellow"
    Write-ColorOutput "Or run this script from Developer PowerShell" "Yellow"
    exit 1
}

$projectPath = "CodeWalker.YftConverter\CodeWalker.YftConverter.csproj"
$solutionPath = "CodeWalker.sln"
$outputPath = "CodeWalker.YftConverter\bin\$Configuration"

# Clean if requested
if ($Clean) {
    Write-ColorOutput "Cleaning previous build..." "Yellow"
    & msbuild $projectPath /t:Clean /p:Configuration=$Configuration /p:Platform=$Platform /v:quiet /nologo
    if (Test-Path $outputPath) {
        Remove-Item -Path $outputPath -Recurse -Force
    }
}

if (-not $NoBuild) {
    # Restore NuGet packages
    Write-ColorOutput "Restoring NuGet packages..." "Yellow"
    $nuget = Get-Command nuget -ErrorAction SilentlyContinue
    if ($nuget) {
        & nuget restore $solutionPath -Verbosity quiet
    } else {
        Write-ColorOutput "WARNING: NuGet not found, skipping package restore" "DarkYellow"
    }

    # Build the project
    Write-Host
    Write-ColorOutput "Building YftConverter project..." "Yellow"
    Write-ColorOutput "Configuration: $Configuration" "Gray"
    Write-ColorOutput "Platform: $Platform" "Gray"
    Write-Host

    $verbosity = if ($Verbose) { "normal" } else { "minimal" }
    
    & msbuild $projectPath /p:Configuration=$Configuration /p:Platform=$Platform /v:$verbosity /nologo

    if ($LASTEXITCODE -ne 0) {
        Write-Host
        Write-ColorOutput "ERROR: Build failed!" "Red"
        exit 1
    }

    Write-Host
    Write-ColorOutput "Build completed successfully!" "Green"
}

# Package the output
if (-not $NoPackage) {
    Write-Host
    Write-ColorOutput "Packaging output files..." "Yellow"
    
    $releaseDir = "Release\YftConverter_$Configuration"
    
    # Create release directory
    if (Test-Path $releaseDir) {
        Remove-Item -Path $releaseDir -Recurse -Force
    }
    New-Item -ItemType Directory -Path $releaseDir | Out-Null
    
    # Copy files
    $filesToCopy = @(
        "*.exe",
        "*.dll",
        "*.config",
        "*.pdb"  # Include debug symbols for Release builds too
    )
    
    foreach ($pattern in $filesToCopy) {
        $files = Get-ChildItem -Path $outputPath -Filter $pattern -ErrorAction SilentlyContinue
        if ($files) {
            Copy-Item -Path $files.FullName -Destination $releaseDir
        }
    }
    
    # Copy the icon
    $iconPath = "CodeWalker.YftConverter\CW.ico"
    if (Test-Path $iconPath) {
        Copy-Item -Path $iconPath -Destination $releaseDir
    }
    
    # Create a README for the package
    $readmeContent = @"
CodeWalker YFT Converter
========================

Version: 1.0.0
Build Configuration: $Configuration
Build Date: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")

A standalone tool for converting YFT files between Gen8 XML and Gen9 compressed formats.

Usage:
------
1. Run "CodeWalker YFT Converter.exe"
2. Select input and output folders
3. Choose output format (Gen8 XML or Gen9 YFT)
4. Click Process to convert files

Requirements:
-------------
- Windows 10/11 (64-bit)
- .NET Framework 4.8

For more information, visit: https://github.com/dexyfex/CodeWalker
"@
    
    $readmeContent | Out-File -FilePath "$releaseDir\README.txt" -Encoding UTF8
    
    Write-ColorOutput "Files copied to: $releaseDir\" "Green"
    
    # Optional: Create ZIP archive
    $zipPath = "$releaseDir.zip"
    if (Get-Command Compress-Archive -ErrorAction SilentlyContinue) {
        Write-ColorOutput "Creating ZIP archive..." "Yellow"
        if (Test-Path $zipPath) {
            Remove-Item $zipPath -Force
        }
        Compress-Archive -Path "$releaseDir\*" -DestinationPath $zipPath -CompressionLevel Optimal
        Write-ColorOutput "Archive created: $zipPath" "Green"
    }
}

Write-Host
Write-ColorOutput "========================================" "Cyan"
Write-ColorOutput "Output location: $outputPath" "White"
Write-ColorOutput "Executable: CodeWalker YFT Converter.exe" "White"
Write-ColorOutput "========================================" "Cyan"

# Show file sizes
if (Test-Path $outputPath) {
    Write-Host
    Write-ColorOutput "Output files:" "Yellow"
    Get-ChildItem -Path $outputPath -Filter "*.exe" | ForEach-Object {
        $size = [math]::Round($_.Length / 1MB, 2)
        Write-Host "  $($_.Name) - $size MB"
    }
}

Write-Host
Write-ColorOutput "Done!" "Green"