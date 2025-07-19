# CodeWalker Build and Deployment Guide

## Prerequisites

### Development Environment
- **Visual Studio 2022** or later (Community Edition or higher)
- **Windows 10/11** (64-bit)
- **.NET Framework 4.8 Developer Pack**
- **.NET 8.0 SDK** (for DLCMerger project)
- **DirectX SDK** (June 2010) or Windows SDK with DirectX headers

### Required Visual Studio Workloads
- .NET desktop development
- Desktop development with C++ (for Shaders project)
- .NET Framework 4.8 targeting pack

### Graphics Requirements
- DirectX 11 compatible graphics card
- Graphics drivers with shader model 5.0 support

## Building CodeWalker

### 1. Clone the Repository
```bash
git clone https://github.com/dexyfex/CodeWalker.git
cd CodeWalker
```

### 2. Restore NuGet Packages
```bash
# Using Visual Studio Package Manager Console
PM> Update-Package -reinstall

# Or using .NET CLI
dotnet restore CodeWalker.sln
```

### 3. Build Configuration

#### Solution Configurations
- **Debug**: Full debugging symbols, no optimizations
- **Release**: Optimized build with minimal debug info
- **Platform**: x64 (recommended) or Any CPU

#### Build Order
The solution is configured with project dependencies, but the build order is:
1. CodeWalker.Core
2. CodeWalker.WinForms
3. CodeWalker.Shaders
4. All other projects

### 4. Building from Visual Studio
1. Open `CodeWalker.sln` in Visual Studio
2. Select configuration (Debug/Release) and platform (x64)
3. Build → Build Solution (Ctrl+Shift+B)
4. Check Output window for any errors

### 5. Building from Command Line
```bash
# Using MSBuild
msbuild CodeWalker.sln /p:Configuration=Release /p:Platform=x64

# Using .NET CLI (for .NET 8 projects)
dotnet build CodeWalker.DLCMerger/CodeWalker.DLCMerger.csproj -c Release
```

## Project-Specific Build Notes

### CodeWalker.Shaders (C++ Project)
- Requires DirectX SDK or Windows SDK
- Compiles HLSL shaders to .cso files
- Output copied to main project on build

### CodeWalker.DLCMerger (.NET 8.0)
- Targets .NET 8.0 instead of Framework 4.8
- Can be published as self-contained executable
- Supports ReadyToRun compilation

## Deployment

### 1. Output Structure
After building, the output structure should be:
```
bin/x64/Release/
├── CodeWalker.exe
├── CodeWalker.Core.dll
├── CodeWalker.WinForms.dll
├── CodeWalker RPF Explorer.exe
├── CodeWalker Vehicle Viewer.exe
├── CodeWalker Ped Viewer.exe
├── CodeWalker.ErrorReport.exe
├── CodeWalker.DLCMerger.exe
├── Shaders/
│   ├── BasicVS_PNCT.cso
│   ├── BasicPS.cso
│   └── ... (other compiled shaders)
├── SharpDX.*.dll (DirectX libraries)
├── WeifenLuo.WinFormsUI.Docking.dll
└── ... (other dependencies)
```

### 2. Required Files for Distribution

#### Core Files (Required)
```
CodeWalker.exe
CodeWalker.exe.config
CodeWalker.Core.dll
CodeWalker.WinForms.dll
```

#### DirectX Dependencies
```
SharpDX.dll
SharpDX.Direct3D11.dll
SharpDX.DXGI.dll
SharpDX.Mathematics.dll
```

#### UI Dependencies
```
WeifenLuo.WinFormsUI.Docking.dll
WeifenLuo.WinFormsUI.Docking.ThemeVS2015.dll
```

#### Shaders (Required)
```
Shaders/*.cso (all compiled shader files)
```

#### Optional Tools
```
CodeWalker RPF Explorer.exe
CodeWalker Vehicle Viewer.exe
CodeWalker Ped Viewer.exe
CodeWalker.ErrorReport.exe
CodeWalker.DLCMerger.exe
```

### 3. Creating a Release Package

#### Manual Packaging
```powershell
# Create release directory
$releaseDir = "CodeWalker_Release_v1.0"
New-Item -ItemType Directory -Path $releaseDir

# Copy main executables
Copy-Item "bin\x64\Release\*.exe" $releaseDir
Copy-Item "bin\x64\Release\*.dll" $releaseDir
Copy-Item "bin\x64\Release\*.config" $releaseDir

# Copy shaders
New-Item -ItemType Directory -Path "$releaseDir\Shaders"
Copy-Item "bin\x64\Release\Shaders\*.cso" "$releaseDir\Shaders"

# Create zip
Compress-Archive -Path $releaseDir\* -DestinationPath "$releaseDir.zip"
```

#### Using Build Script
Create `build-release.ps1`:
```powershell
param(
    [string]$Version = "1.0.0",
    [string]$Configuration = "Release",
    [string]$Platform = "x64"
)

# Build solution
msbuild CodeWalker.sln /p:Configuration=$Configuration /p:Platform=$Platform

# Create release package
$outputDir = "bin\$Platform\$Configuration"
$releaseDir = "CodeWalker_v$Version"

# ... (packaging logic)
```

### 4. Installer Creation (Optional)

Using Inno Setup:
```iss
[Setup]
AppName=CodeWalker
AppVersion=1.0.0
DefaultDirName={pf}\CodeWalker
DefaultGroupName=CodeWalker
OutputBaseFilename=CodeWalker_Setup

[Files]
Source: "bin\x64\Release\*"; DestDir: "{app}"; Flags: recursesubdirs

[Icons]
Name: "{group}\CodeWalker"; Filename: "{app}\CodeWalker.exe"
Name: "{group}\RPF Explorer"; Filename: "{app}\CodeWalker RPF Explorer.exe"
```

## Configuration

### 1. Application Settings
Default settings location: `%APPDATA%\CodeWalker\settings.xml`

Key settings to configure:
```xml
<Settings>
    <GTAFolder>C:\Program Files\Rockstar Games\Grand Theft Auto V</GTAFolder>
    <AntiAlias>4</AntiAlias>
    <WindowMaximized>false</WindowMaximized>
    <FullScreen>false</FullScreen>
</Settings>
```

### 2. First Run Setup
On first run, CodeWalker will:
1. Prompt for GTA V installation directory
2. Create settings file
3. Initialize game file cache

## Troubleshooting

### Common Build Issues

#### Missing DirectX SDK
**Error**: Cannot find d3dcompiler.h
**Solution**: Install DirectX SDK or use Windows SDK

#### NuGet Package Errors
**Error**: Package restore failed
**Solution**: Clear NuGet cache and restore
```bash
nuget locals all -clear
nuget restore CodeWalker.sln
```

#### Shader Compilation Errors
**Error**: fxc.exe not found
**Solution**: Ensure DirectX SDK or Windows SDK is in PATH

### Runtime Issues

#### DirectX Initialization Failed
**Cause**: Missing or outdated graphics drivers
**Solution**: Update graphics drivers, install DirectX runtime

#### Game Files Not Found
**Cause**: Incorrect GTA V directory
**Solution**: Update GTAFolder in settings

#### Out of Memory
**Cause**: Large file operations
**Solution**: Increase virtual memory, use 64-bit build

## Performance Optimization

### Build Optimizations
```xml
<!-- In .csproj files -->
<PropertyGroup Condition="'$(Configuration)'=='Release'">
    <Optimize>true</Optimize>
    <DebugType>pdbonly</DebugType>
    <PlatformTarget>x64</PlatformTarget>
    <Prefer32Bit>false</Prefer32Bit>
</PropertyGroup>
```

### Runtime Optimizations
1. **Memory Settings**: Adjust cache limits in settings
2. **LOD Settings**: Configure level-of-detail distances
3. **Shader Quality**: Balance quality vs performance

## Continuous Integration

### GitHub Actions Example
```yaml
name: Build CodeWalker

on: [push, pull_request]

jobs:
  build:
    runs-on: windows-latest
    
    steps:
    - uses: actions/checkout@v2
    
    - name: Setup MSBuild
      uses: microsoft/setup-msbuild@v1
    
    - name: Setup NuGet
      uses: NuGet/setup-nuget@v1
    
    - name: Restore packages
      run: nuget restore CodeWalker.sln
    
    - name: Build solution
      run: msbuild CodeWalker.sln /p:Configuration=Release /p:Platform=x64
    
    - name: Upload artifacts
      uses: actions/upload-artifact@v2
      with:
        name: CodeWalker-Release
        path: bin/x64/Release/
```

## Security Considerations

### Code Signing
For distribution, consider signing executables:
```powershell
signtool sign /f certificate.pfx /p password /t http://timestamp.url CodeWalker.exe
```

### Permissions
CodeWalker requires:
- Read access to GTA V directory
- Write access to user's AppData folder
- Network access for error reporting (optional)

## Summary

Building and deploying CodeWalker is straightforward with Visual Studio 2022. The key requirements are:
1. Proper development environment setup
2. All project dependencies resolved
3. DirectX SDK for shader compilation
4. Correct file structure for distribution

For production deployments, consider creating an installer and signing the executables for better user experience and security.