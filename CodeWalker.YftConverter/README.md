# YFT Converter - Standalone Application

A standalone Windows Forms application for converting YFT files between different formats for GTA V.

## Features

- **Convert YFT files** between Gen8 XML and Gen9 YFT compressed formats
- **Batch processing** with folder selection and recursive subfolder support
- **User-friendly GUI** following CodeWalker design patterns
- **Progress tracking** with real-time log output
- **Multi-threaded** processing to keep UI responsive

## Output Formats

1. **Gen8 XML**: Converts YFT files to XML format for editing and inspection
2. **Gen9 YFT (Compressed)**: Converts to compressed Gen9 YFT format

## Building the Application

1. Open `CodeWalker.sln` in Visual Studio
2. The YftConverter project should appear in the solution
3. Build the solution in Release mode (x64)
4. The executable will be in `CodeWalker.YftConverter/bin/Release/`

## Usage

1. **Run** `CodeWalker YFT Converter.exe`
2. **Select Input Folder**: Browse to folder containing YFT files
3. **Select Output Folder**: Choose where converted files will be saved
4. **Choose Output Format**: 
   - Gen8 XML - Creates .yft.xml files
   - Gen9 YFT (Compressed) - Creates .yft files
5. **Configure Options**:
   - Include subfolders - Process files in subdirectories
   - Overwrite existing files - Replace existing output files
6. **Click Process** to start conversion
7. **Monitor Progress** in the log window

## Integration Point

The core conversion logic is implemented in the `ConvertYftFile` method at line ~156 in `YftConverterForm.cs`:

```csharp
private void ConvertYftFile(string inputFile, string outputFile, OutputFormat format)
{
    // Load the YFT file
    var yft = new YftFile();
    yft.Load(File.ReadAllBytes(inputFile));

    if (format == OutputFormat.Gen8Xml)
    {
        // Convert to XML format
        var xml = YftXml.GetXml(yft);
        // Save XML...
    }
    else // OutputFormat.Gen9Yft
    {
        // Save as Gen9 compressed format
        var data = yft.Save();
        File.WriteAllBytes(outputFile, data);
    }
}
```

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

## Error Handling

- Invalid folder selections show warning dialogs
- File-level errors are logged but don't stop batch processing
- Progress bar shows overall completion status
- All errors and status messages appear in the log window