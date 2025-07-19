# YFT Converter - Standalone Application

A robust standalone application for converting YFT (Fragment Type) files between different formats for GTA V modding. Supports both GUI and command-line interfaces.

## Features

- **Three Output Formats**:
  - **Gen8 YFT (Compressed)** - Version 162 format
  - **Gen9 YFT (Compressed)** - Version 171 format
  - **XML** - Human-readable format for editing and inspection
- **Dual Interface**: Windows Forms GUI and command-line support
- **Robust Error Handling**: Handles corrupted or incomplete files gracefully
- **Enhanced Diagnostics**: File validation and verbose debugging output
- **Batch Processing**: Process entire folders with recursive subfolder support
- **Uncompressed YFT Support**: Specifically designed for memory dump YFT files

## Building the Application

1. Open `CodeWalker.sln` in Visual Studio
2. The YftConverter project should appear in the solution
3. Build the solution in Release mode (x64)
4. The executable will be in `CodeWalker.YftConverter/bin/Release/`

Alternatively, use the build script:
```bash
yftconverter.bat
```

## Usage

### GUI Mode

1. **Run** `CodeWalker YFT Converter.exe`
2. **Select Input Folder**: Browse to folder containing YFT files
3. **Select Output Folder**: Choose where converted files will be saved
4. **Choose Output Format**: 
   - Gen8 YFT (Compressed) - Creates .yft files with version 162
   - Gen9 YFT (Compressed) - Creates .yft files with version 171
   - XML - Creates .yft.xml files for editing
5. **Configure Options**:
   - Include subfolders - Process files in subdirectories
   - Overwrite existing files - Replace existing output files
6. **Click Process** to start conversion
7. **Monitor Progress** in the log window

### Console Mode

```bash
YftConverter.exe <input_file> <output_file> [options]
```

#### Options:
- `--gen8`, `-g8` - Output as Gen8 compressed YFT (version 162)
- `--gen9`, `-g9` - Output as Gen9 compressed YFT (version 171) [default]
- `--xml`, `-x` - Output as XML format
- `--verbose`, `-v` - Show detailed conversion information
- `--help`, `-h` - Show help message

#### Examples:
```bash
# Convert to Gen9 YFT (default)
YftConverter.exe zondazun.yft zondazun_compressed.yft

# Convert to Gen8 YFT
YftConverter.exe zondazun.yft zondazun_gen8.yft --gen8

# Convert to XML with verbose output
YftConverter.exe zondazun.yft zondazun.xml --xml --verbose
```

## Input File Requirements

This converter is designed for **uncompressed YFT files** (memory dumps):
- Files must start with 'FRAG' header
- Typical size: Several MB to hundreds of MB
- Usually extracted from game memory or decompressed from RPF archives

## Enhanced Error Handling

The converter now provides better error handling for common issues:

1. **"Invalid data while decoding"** - The file contains corrupted or invalid compressed data
2. **"File doesn't start with expected header"** - File is not a valid YFT
3. **"Input file is too large"** - Files over 500MB are not supported
4. **"File structure appears to be incomplete"** - Memory dump was not extracted completely

### Troubleshooting Tips:
- Use `--verbose` flag to get detailed diagnostic information
- Check that input files have the 'FRAG' header (not 'RSC7')
- Ensure files are complete memory dumps, not partial extractions

## Integration Point

The converter has been enhanced with:

1. **Robust YftConverter class** (`YftConverter.cs`) that handles:
   - Uncompressed YFT (memory dump) conversion
   - Multiple output format support
   - Advanced error handling and recovery
   - Diagnostic output for troubleshooting

2. **Improved ConvertYftFile method** that:
   - Automatically detects file format (compressed vs uncompressed)
   - Routes to appropriate conversion method
   - Provides clear error messages

3. **Better error handling** throughout:
   - Specific handling for DeflateStream exceptions
   - File validation before processing
   - Helpful error messages for common issues

## Dependencies

- .NET Framework 4.8
- CodeWalker.Core (included via project reference)
- Windows Forms

## Architecture

This follows the same pattern as the Gen9Converter:
- Standalone executable with minimal footprint
- Reuses core functionality from CodeWalker.Core
- Simple WinForms UI for ease of use
- No installation required - just run the EXE

## Technical Implementation

### Key Components:

1. **YftConverter.cs** - Core conversion engine
   - `ConvertUncompressedYFT()` - Main conversion method
   - `LoadUncompressedYft()` - Robust YFT loading with fallbacks
   - `ConvertToCompressedYft()` - Gen8/Gen9 compression
   - `ConvertToXml()` - XML export functionality

2. **YftConverterForm.cs** - Windows Forms GUI
   - Batch processing support
   - Real-time progress tracking
   - Error logging

3. **Program.cs** - Dual-mode entry point
   - Console mode for command-line usage
   - GUI mode for interactive use

## Error Handling

- **File Validation**: Checks header, size, and structure before processing
- **Graceful Degradation**: Multiple fallback methods for loading YFT data
- **Detailed Diagnostics**: Verbose mode shows file structure and processing steps
- **Batch Resilience**: File-level errors don't stop batch processing
- **Clear Error Messages**: Specific guidance for common issues