# FRAG Format Documentation and Conversion Guide

## Overview

YFT files in GTA V can exist in two formats:
1. **RSC7 Format** - Standard compressed format with RSC7 header
2. **FRAG Format** - Uncompressed format that starts with "FRAG" or raw fragment data

## Format Comparison

### RSC7 Format (Standard)
- **Header**: 16 bytes
  - Magic: "RSC7" (0x37435352)
  - Version: 162 (standard) or 171 (Gen9)
  - SystemFlags: Memory allocation info
  - GraphicsFlags: Graphics memory info
- **Data**: DEFLATE compressed fragment data
- **Size**: Typically smaller due to compression

### FRAG Format
- **Header**: 4 bytes "FRAG" (0x47415246) or no header
- **Data**: Uncompressed raw fragment data
- **Size**: Approximately 2x larger than RSC7
- **Characteristics**:
  - Contains many zero bytes (padding)
  - Readable text strings (texture names, shader names)
  - Direct memory dump format

## Why FRAG Format Exists

The FRAG format appears to be:
- Development/debug format used during asset creation
- Intermediate format before final compression
- Easier to inspect and modify without decompression
- Used by some modding tools that don't implement compression

## Conversion Tools

### 1. YftFragConverter (Recommended)
Simple utility class for automatic format detection and conversion:

```csharp
// Auto-detect and load any YFT format
YftFile yft = YftFragConverter.LoadYft("vehicle.yft");

// Convert FRAG to RSC7
YftFragConverter.ConvertFile("input.yft", "output.yft");
```

### 2. FragToRsc7Converter
Advanced converter with analysis capabilities:

```csharp
// Analyze file format
FragToRsc7Converter.AnalyzeFile("vehicle.yft");

// Convert with specific version
byte[] rsc7Data = FragToRsc7Converter.ConvertFragToRsc7(fragData, version: 171);
```

### 3. Command Line Tool
```batch
ConvertFragToRsc7.exe vehicle_frag.yft vehicle_rsc7.yft -v162
```

## Usage Examples

### Basic Conversion
```csharp
// Check if file is FRAG format
byte[] data = File.ReadAllBytes("vehicle.yft");
if (YftFragConverter.IsFragFormat(data))
{
    // Convert to RSC7
    byte[] converted = YftFragConverter.ConvertToRsc7(data);
    File.WriteAllBytes("vehicle_rsc7.yft", converted);
}
```

### Batch Conversion
```csharp
foreach (string file in Directory.GetFiles(folder, "*.yft"))
{
    if (YftFragConverter.IsFragFormat(File.ReadAllBytes(file)))
    {
        YftFragConverter.ConvertFile(file, file + ".rsc7");
    }
}
```

### Integration with CodeWalker
The converters can be integrated into CodeWalker's YftFile class to automatically handle FRAG format files:

```csharp
public void Load(byte[] data, RpfFileEntry entry = null)
{
    // Auto-convert FRAG format if detected
    if (YftFragConverter.IsFragFormat(data))
    {
        data = YftFragConverter.ConvertToRsc7(data);
    }
    
    // Continue with normal loading
    RpfFile.LoadResourceFile(this, data, (uint)GetVersion(RpfManager.IsGen9));
}
```

## Technical Details

### FRAG Format Structure
```
Offset  Size  Description
0x00    4     Magic "FRAG" (optional)
0x04    ...   Raw FragType structure data
```

### Identifying FRAG Files
1. Check for "FRAG" magic at offset 0
2. If no magic, look for typical YFT string patterns:
   - "vehicle_generic"
   - "smallspecmap"
   - "diffuse", "normal", "specular"
   - ".dds" texture references
   - Shader names

### Compression Process
1. Remove FRAG header if present
2. Compress raw data using DEFLATE algorithm
3. Calculate system/graphics flags based on compressed size
4. Prepend RSC7 header (16 bytes)

## Common Issues and Solutions

### Issue: File size doubles after editing
**Cause**: Tool saved in FRAG format instead of RSC7
**Solution**: Use converters to compress back to RSC7

### Issue: Game crashes loading modified YFT
**Cause**: Incorrect format or corrupted data
**Solution**: Ensure proper RSC7 header and valid compression

### Issue: Can't open YFT in tool
**Cause**: Tool doesn't support FRAG format
**Solution**: Convert to RSC7 first using provided tools

## Best Practices

1. **Always verify converted files**: Load in CodeWalker to ensure validity
2. **Keep backups**: Store original files before conversion
3. **Use appropriate version**: 162 for standard, 171 for Gen9/Next-Gen
4. **Check file sizes**: RSC7 should be ~50% of FRAG size

## Future Improvements

Potential enhancements to the conversion tools:
- GUI interface for batch conversion
- Integration into CodeWalker's file browser
- Automatic format detection in all YFT operations
- Support for other uncompressed formats