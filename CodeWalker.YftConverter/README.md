# YFT Converter

A versatile tool to convert uncompressed YFT memory dumps to either compressed YFT format or XML format.

## Usage

```bash
YftConverter.exe input.yft output.[yft|xml] [options]
```

## Options

- `-v, --verbose` - Enable verbose output
- `-g, --generation` - Compression generation (gen8 or gen9). Default: gen8 (YFT format only)
- `-f, --format` - Output format (yft or xml). Default: yft
- `-r, --resource-folder` - Folder for extracting resources/textures (XML format only)

## Key Conversion Logic

### Input File Requirements

The YFT converter expects **uncompressed YFT memory dumps** as input. These files must:

1. **Start with "FRAG" header** (magic number: 0x47415246)
   - The first 4 bytes must spell "FRAG" in ASCII
   - This identifies the file as an uncompressed fragment memory dump
   
2. **Contain valid memory pointers** in these ranges:
   - System memory: `0x50000000` to `0x5FFFFFFF`
   - Graphics memory: `0x60000000` to `0x6FFFFFFF`

### File Format Detection

The converter uses multiple methods to detect file formats:

```
Uncompressed YFT: Starts with "FRAG" (0x47415246)
Compressed YFT:   Starts with "RSC7" (0x37435352)
```

If a file doesn't have a recognized header, the converter checks for typical YFT string patterns like:
- "vehicle_generic"
- "smallspecmap"
- "diffuse", "normal", "specular"
- ".dds", "shader", "drawable"

### Memory Layout and Pointer System

Uncompressed YFT files are direct memory dumps from the game with a specific structure:

1. **Memory Regions**:
   - **System Memory Base**: `0x50000000` - Contains FragType structure and system data
   - **Graphics Memory Base**: `0x60000000` - Contains graphics resources and buffers
   - **Pointer Mask**: `0x7FFFFFFF` - Used to extract offset from absolute pointers

2. **Pointer Detection and Conversion**:
   - The converter scans for 64-bit values that fall within valid memory ranges
   - Each pointer is analyzed to determine if it references system or graphics memory
   - Pointers are converted from absolute memory addresses to relative offsets

### Conversion Process

The converter uses a three-tier approach:

#### 1. Native YFT Loading (Preferred)
- Loads the memory dump using CodeWalker's native YFT handling
- Automatically handles pointer resolution and structure parsing
- Preserves all Fragment data including Drawable, Physics, Glass, etc.

#### 2. Direct Memory Conversion (Fallback)
- Converts pointers in-place within the memory dump
- Creates proper ResourceDataReader with memory base addresses
- Parses FragType structure directly from memory

#### 3. Simple Compression (Last Resort)
- Used when pointer conversion fails
- Compresses the entire memory dump as system data
- Creates RSC7 header with appropriate flags

### Compression Details

1. **DEFLATE Compression**:
   - Uses optimal compression level
   - Typically achieves 40-60% compression ratio
   
2. **RSC7 Header Structure** (16 bytes):
   - Bytes 0-3: Magic "RSC7" (0x37435352)
   - Bytes 4-7: Version (162 for Gen8, 171 for Gen9)
   - Bytes 8-11: System memory flags
   - Bytes 12-15: Graphics memory flags

3. **Memory Flags Calculation**:
   - Flags are calculated based on compressed data size
   - Different calculations for Gen8 vs Gen9 formats
   - Determines how the game allocates memory for the resource

## Examples

### YFT to Compressed YFT Conversion

```bash
# Convert using default Gen8 compression (version 162)
YftConverter.exe locked.yft locked_compressed.yft -v

# Convert using Gen9 compression (version 171)
YftConverter.exe locked.yft locked_compressed.yft -g gen9 -v

# Convert using Gen8 compression explicitly
YftConverter.exe locked.yft locked_compressed.yft -g gen8 -v
```

### YFT to XML Conversion

```bash
# Convert to XML format
YftConverter.exe locked.yft locked.xml -f xml -v

# Convert to XML with resource extraction
YftConverter.exe locked.yft locked.xml -f xml -r extracted_resources -v
```

## Output Formats

### Compressed YFT Format
- Binary format with RSC7 header
- Compressed using DEFLATE algorithm
- Ready for use in GTA V
- Supports both Gen8 (version 162) and Gen9 (version 171)

### XML Format
- Human-readable text format
- Complete representation of YFT structure
- Can be edited with any text editor
- Supports round-trip conversion (XML back to YFT using CodeWalker)
- Optionally extracts textures and resources to specified folder

## Generation Differences (YFT Format)

- **Gen8**: Uses version 162, compatible with GTA V base game and older mods
- **Gen9**: Uses version 171, compatible with newer GTA V versions and enhanced edition

## Typical Workflow

1. **Extract uncompressed YFT** from game using CodeWalker's "Extract Uncompressed" function
2. **Convert to desired format**:
   - For game use: Convert to compressed YFT (gen8 or gen9)
   - For analysis/editing: Convert to XML
3. **Edit XML** if needed (modify properties, adjust values)
4. **Convert XML back to YFT** using CodeWalker (if XML was used)

## Technical Validation

### What Must Match for Successful Conversion

1. **Header Validation**:
   - First 4 bytes MUST be "FRAG" (0x47415246)
   - Without this header, the file is rejected as invalid input

2. **Pointer Validation**:
   - All 64-bit values in these ranges are treated as pointers:
     - `0x50000000 - 0x5FFFFFFF` (System memory)
     - `0x60000000 - 0x6FFFFFFF` (Graphics memory)
   - Pointers outside these ranges are preserved as regular data

3. **Structure Alignment**:
   - Data regions are aligned to 16-byte boundaries
   - The converter automatically handles alignment padding

4. **FragType Structure**:
   - The memory dump must start with a valid FragType structure
   - Key fields validated: BoundingSphere, Physics LOD Group, Drawable references

### Debugging Conversion Issues

If conversion fails, the converter provides detailed diagnostics:

```
First 20 uint64 values in file:
  Offset 0x0000: 0x47415246...  (Shows the FRAG header)
  Offset 0x0008: 0x50000123...  (Shows pointer values)
  ...
```

Common issues and solutions:

1. **"Input file is not an uncompressed YFT"**
   - File doesn't start with FRAG header
   - Solution: Ensure you're using an uncompressed memory dump from CodeWalker

2. **"Failed to parse FragType from memory dump"**
   - Corrupted or incomplete memory dump
   - Solution: Re-extract the uncompressed YFT from CodeWalker

3. **Pointer conversion warnings**
   - Some pointers may reference invalid memory regions
   - The converter will attempt fallback methods automatically

### Memory Region Analysis

The converter performs comprehensive memory analysis:

1. **Initial Scan**: Identifies all memory regions referenced by pointers
2. **Region Size Estimation**: Determines data boundaries using:
   - Zero-padding detection (64+ consecutive zeros indicate boundary)
   - Pointer pattern changes (new structure indicators)
   - Maximum region size cap (1MB for safety)
3. **Recursive Scanning**: Follows pointer chains to discover all data

## Notes

- Input file must be an uncompressed YFT memory dump (starts with "FRAG" header)
- The tool preserves all Fragment data including Drawable, Physics, Glass, etc.
- XML files will be significantly larger than binary YFT files
- Gen8 is set as default for maximum compatibility
- Resource extraction creates a folder with textures and other embedded resources
- The converter uses intelligent fallback mechanisms to handle edge cases
- Verbose mode (`-v`) provides detailed conversion diagnostics