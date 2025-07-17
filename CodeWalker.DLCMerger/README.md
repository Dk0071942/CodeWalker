# CodeWalker DLC Merger

A command-line tool for merging multiple GTA V DLC RPF files into a single RPF archive.

## Features

- **Merge multiple DLC RPF files** into one consolidated archive
- **Recursive directory scanning** - automatically finds all RPF files in subdirectories
- **RPF structure visualization** - shows detailed file/directory trees for debugging
- **Intelligent conflict detection** - distinguishes between file, directory, and meta conflicts
- **Flexible input methods** - individual files or entire directories
- **Multiple encryption support** - NONE, OPEN, AES, NG encryption types
- **Dry-run mode** - preview merge results without creating files
- **Comprehensive logging** - verbose output with progress tracking
- **Future-ready** - prepared for meta/xml file merging (Phase 2)

## Usage

```bash
DLCMerger -i input1.rpf -i input2.rpf -o merged.rpf [options]
```

**Or with directory input (recommended):**

```bash
DLCMerger -i /path/to/dlc/folder -o merged.rpf [options]
```

### Required Arguments

- `-i, --input` (multiple): Input RPF files or directories to merge. 
  - **Files**: Specify individual RPF files: `-i file1.rpf -i file2.rpf`
  - **Directories**: Specify directories to scan recursively: `-i /path/to/dlc/folder`
  - **Mixed**: Combine both approaches: `-i file1.rpf -i /path/to/folder`
- `-o, --output`: Output RPF file path.

### Optional Arguments

- `-s, --show-structure`: Show detailed structure tree of input RPF files (helpful for debugging).
- `-n, --expand-nested`: Expand nested RPF files to show their contents (enabled by default).
- `-m, --merge-meta`: Enable merging of meta/xml files (experimental, not fully implemented yet).
- `-v, --verbose`: Enable verbose output for detailed logging.
- `-e, --encryption`: Encryption type for output RPF (NONE, OPEN, AES, NG). Default: OPEN.
- `-f, --force`: Force overwrite output file if it exists.
- `-d, --dry-run`: Perform a dry run without creating the output file.

### Input Directory Behavior

When you specify a directory with `-i`, the tool will:
- **Recursively scan** all subdirectories for `.rpf` files
- **Automatically discover** all RPF files in the directory tree
- **Report** the number of RPF files found in each directory
- **Include** all found RPF files in the merge operation

This is particularly useful for DLC folders that contain multiple RPF files in different subdirectories.

## Nested RPF Handling

The tool can recursively read nested RPF files (RPF files within RPF files) to provide complete visibility:

### What are Nested RPFs?
- DLC RPF files often contain other RPF files like `vehicles.rpf`, `weapons.rpf`, etc.
- These nested RPFs contain the actual game assets (models, textures, etc.)
- Traditional tools only show the outer RPF structure

### Nested Expansion Features:
- **Automatic Detection**: Finds and opens RPF files within RPF files
- **Recursive Reading**: Supports multiple levels of nesting (up to 3 levels deep)
- **Complete Structure**: Shows the full file hierarchy including nested contents
- **Conflict Detection**: Detects conflicts in nested RPF contents
- **Temporary Handling**: Safely extracts and reads nested RPFs without permanent files

### Example Structure:
```
dlc.rpf
├── content.xml
├── setup2.xml
├── common/data/
│   ├── carcols.meta
│   └── vehicles.meta
└── x64/levels/gta5/vehicles/
    └── vehicles.rpf [NESTED]
        ├── vehshare.ytd          <- These files are now visible
        ├── vehicle_models/
        │   ├── model1.yft
        │   └── model2.ytd
        └── vehicle_textures/
            └── textures.ytd
```

### Control Options:
- **`--expand-nested`** (default): Shows nested RPF contents
- **`--no-expand-nested`**: Shows only top-level RPF structure
- **Verbose mode**: Shows nesting depth and extraction process

## Examples

### 1. Merge all RPF files in a directory (recommended):
```bash
# Recursively finds all RPF files in the directory and subdirectories
DLCMerger -i /path/to/dlc/folder -o merged_dlc.rpf -v -s
```

### 2. Dry run with full nested structure analysis:
```bash
# Preview the merge with nested RPF expansion, shows complete structure trees
DLCMerger -i /path/to/dlc/folder -o merged_dlc.rpf -d -v -s -n
```

### 2b. Dry run without nested expansion (faster):
```bash
# Preview the merge showing only top-level RPF structure
DLCMerger -i /path/to/dlc/folder -o merged_dlc.rpf -d -v -s --no-expand-nested
```

### 3. Mixed input sources:
```bash
# Combine specific files with directory scanning
DLCMerger -i specific.rpf -i /path/to/folder -i another.rpf -o merged.rpf -v
```

### 4. Basic merge of two DLC files:
```bash
DLCMerger -i dlc1.rpf -i dlc2.rpf -o merged_dlc.rpf
```

### 5. Merge with verbose output and force overwrite:
```bash
DLCMerger -i dlc1.rpf -i dlc2.rpf -o merged_dlc.rpf -v -f
```

### 6. Debug merge issues with structure trees:
```bash
# Shows complete file structure of all input RPFs
DLCMerger -i dlc1.rpf -i dlc2.rpf -o merged_dlc.rpf -s -v -d
```

### 7. Merge with specific encryption:
```bash
DLCMerger -i dlc1.rpf -i dlc2.rpf -o merged_dlc.rpf -e AES
```

### 8. Complex debugging scenario:
```bash
# Comprehensive analysis with all debug options
DLCMerger -i /complex/dlc/structure -o debug_merged.rpf -s -v -d -f
```

## Recursive Directory Scanning

The tool supports recursive directory scanning with the following behavior:

- **Automatic Discovery**: When you specify a directory with `-i`, the tool automatically finds all `.rpf` files in that directory and all subdirectories
- **Flexible Structure**: Works with any directory structure, including nested DLC folders
- **Progress Reporting**: Shows how many RPF files were found in each directory
- **Combines with Files**: You can mix directory scanning with individual file inputs

### Example Directory Structure:
```
dlc_collection/
├── vehicle_pack_1/
│   ├── dlc.rpf
│   └── weapons.rpf
├── vehicle_pack_2/
│   └── addon/
│       └── dlc.rpf
└── standalone_mod.rpf
```

Using `-i dlc_collection/` would automatically include all 4 RPF files.

## Conflict Resolution

The tool provides intelligent conflict detection and resolution:

### Conflict Types:
1. **File Conflicts**: Same file path in multiple RPFs
2. **Directory Conflicts**: Directory structure overlaps (automatically merged)
3. **Meta/XML Conflicts**: Configuration files that could be merged (future feature)

### Resolution Strategy:
1. **Directory Conflicts**: Automatically merged (not treated as conflicts)
2. **File Conflicts**: First occurrence kept (from first input RPF/directory)
3. **Meta/XML Files**: Will be intelligently merged in future versions with `-m` flag

### Conflict Reporting:
- Detailed conflict analysis with file sizes and source RPFs
- Categorized by conflict type (file, directory, meta)
- Suggestions for resolution strategies

## RPF Structure Visualization

The `-s, --show-structure` flag provides detailed tree visualization of RPF contents:

### Features:
- **Hierarchical Tree Display**: Shows complete directory and file structure
- **File Size Information**: Displays file sizes for all files
- **Directory Item Counts**: Shows number of items in each directory
- **Multiple RPF Support**: Shows structure for each input RPF separately

### Example Output:
```
=== INPUT RPF 1 Structure ===
File: dlc1.rpf
Total entries: 39
Encryption: OPEN

dlc1.rpf
├── content.xml (735 bytes)
├── setup2.xml (301 bytes)
├── common (2 items)
│   └── data (5 items)
│       ├── carcols.meta (611 bytes)
│       ├── handling.meta (1080 bytes)
│       └── levels (1 items)
│           └── gta5 (1 items)
│               └── vehicles.meta (1695 bytes)
└── x64 (2 items)
    ├── data (12 items)
    │   └── lang (12 items)
    │       ├── americandlc.rpf (1024 bytes)
    │       └── ... (more files)
    └── levels (1 items)
        └── gta5 (1 items)
            └── vehicles (1 items)
                └── vehicles.rpf (29955072 bytes)
```

This visualization helps identify:
- Which files are being merged
- Where conflicts occur
- Directory structure differences
- File size variations

## Building

The tool is part of the CodeWalker solution. To build:

1. Open CodeWalker.sln in Visual Studio
2. Build the CodeWalker.DLCMerger project
3. The executable will be in `bin/Debug/net8.0/DLCMerger.exe`

## Output and Logging

The tool provides comprehensive logging with multiple verbosity levels:

### Standard Output:
- Discovery phase: Shows RPF files found in directories
- Analysis phase: Reports total entries and conflicts
- Processing phase: Shows progress during file copying
- Completion: Summary of files copied and directories created

### Verbose Output (`-v` flag):
- Individual file processing status
- Detailed conflict information during analysis
- List of all RPF files discovered in each directory
- Progress updates every 50 files processed

### Structure Visualization (`-s` flag):
- Complete tree structure for each input RPF
- File sizes and directory item counts
- Merged structure analysis with file type breakdown
- Conflict categorization and analysis

## Current Limitations

- **Meta/XML file merging**: Not yet implemented (placeholder only)
- **Nested RPF merging**: While nested RPFs are read and analyzed for conflict detection, they are not extracted and merged (only top-level files are merged)
- **Resource optimization**: Files are copied as-is without re-compression optimization
- **Performance**: Large RPF files may take significant time to process, especially with nested RPF expansion enabled

## Future Enhancements (Phase 2)

- **Intelligent meta/xml file merging**: Combine vehicle variations, handling data, carcols, etc.
- **Advanced conflict resolution**: User-configurable conflict resolution strategies
- **Nested RPF merging**: Actually merge contents of nested RPF archives (currently only analyzed)
- **Resource optimization**: Re-compress and optimize resources during merge
- **Performance improvements**: Parallel processing and streaming operations
- **GUI version**: Integrated with CodeWalker's main interface
- **Plugin system**: Support for custom merge logic plugins

## Troubleshooting

### Common Issues:

1. **"No RPF files found"**: Ensure the directory path is correct and contains `.rpf` files
2. **"Output file already exists"**: Use `-f` flag to force overwrite
3. **Large merge takes too long**: Use `-d` flag for dry run testing first
4. **Conflicts not expected**: Use `-s` flag to visualize structure differences

### Performance Tips:

- Use dry run (`-d`) to test before actual merge
- Enable verbose mode (`-v`) only when debugging
- Structure visualization (`-s`) adds processing time for large RPFs
- Consider merging in smaller batches for very large collections