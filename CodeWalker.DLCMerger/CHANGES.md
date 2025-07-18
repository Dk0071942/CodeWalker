# DLC Merger - Selective Merging Implementation

## Summary of Changes

This update implements selective merging for the DLC Merger tool, allowing it to intelligently merge only vehicle-related files and their dependencies rather than merging everything from all DLCs.

## Key Features Added

### 1. SelectiveMerger Class
A new class that provides intelligent filtering logic for determining which files should be included in the merge:
- Analyzes `vehicles.meta` to identify vehicle models and their dependencies
- Maintains lists of essential vehicle meta files
- Identifies skippable files that aren't needed for vehicles
- Detects when vehicle weapons or custom explosions are used

### 2. Selective Filtering Logic
The merger now operates in two modes:
- **Default (Selective)**: Only merges vehicle-related files and dependencies
- **Merge All**: Previous behavior, merges everything (enabled with `--merge-all` flag)

### 3. File Categories
Files are classified into categories:
- **Essential Vehicle Meta Files**: Always included (vehicles.meta, handling.meta, carcols.meta, etc.)
- **Vehicle Model Files**: .yft, .ytd, .ycd files for vehicles
- **Optional Dependencies**: Files like weaponarchetypes.meta only included when needed
- **Language Files**: .gxt2 files for vehicle display names
- **Skippable Files**: Ped-related files, unrelated weapon files, etc.

### 4. Smart content.xml Generation
When in selective mode, the merger creates a filtered content.xml that only includes:
- Vehicle-related metadata entries
- Required RPF containers (vehicles.rpf, weapons.rpf if needed)
- Language files for vehicle names

## Usage

### Default Behavior (Selective Merging)
```bash
dotnet CodeWalker.DLCMerger.dll -i dlc1.rpf dlc2.rpf -o merged.rpf
```
This will merge only vehicle-related content from the input DLCs.

### Merge Everything (Previous Behavior)
```bash
dotnet CodeWalker.DLCMerger.dll -i dlc1.rpf dlc2.rpf -o merged.rpf --merge-all
```
This will merge all content from the input DLCs.

## Implementation Details

### How It Works
1. **First Pass**: Analyzes all `vehicles.meta` files to identify vehicle models
2. **Dependency Detection**: Checks for vehicle weapons, custom explosions, etc.
3. **Selective Collection**: Only includes files that match the filtering criteria
4. **Smart Merging**: Meta files are merged intelligently, combining vehicle entries
5. **Manifest Generation**: Creates filtered content.xml and setup2.xml files

### Files Modified
- `RpfMerger.cs`: Added selective filtering logic and two-pass analysis
- `Options.cs`: Added `--merge-all` flag
- `MetaMerger.cs`: Added selective filtering support
- `SelectiveMerger.cs`: New class implementing the filtering logic

## Benefits
- Smaller output files containing only necessary data
- Cleaner merged DLCs without unrelated content
- Faster processing by skipping unnecessary files
- Better compatibility with game engine by avoiding conflicts

## Future Enhancements
- Automatic setup2.xml generation with proper naming
- Support for merging specific vehicle types only
- Configuration file support for custom filtering rules
- Better handling of vehicle dependencies (audio, effects, etc.)