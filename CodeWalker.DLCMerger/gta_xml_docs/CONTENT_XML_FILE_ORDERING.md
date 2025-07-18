# Content.xml File Ordering Requirements

## Overview

The `content.xml` file in GTA V DLC packages defines which files should be loaded and in what order. The ordering of files in the `<filesToEnable>` section is critical for proper game loading, as some files have dependencies on others.

## File Loading Order

The DLCMerger now enforces a specific loading order for vehicle-related meta files to ensure proper dependency resolution:

### Priority Order (Must be loaded in this sequence)

1. **handling.meta** - Vehicle handling characteristics and physics properties
2. **vehiclelayouts.meta** - Vehicle seating and occupant positions
3. **vehicles.meta** - Core vehicle definitions and properties
4. **carcols.meta** - Vehicle color variations and liveries
5. **carvariations.meta** - Vehicle model variations and customization options

### Secondary Files (Loaded after priority files)

6. Other meta files (alphabetically sorted)
   - dlctext.meta
   - explosion.meta
   - weaponarchetypes.meta
   - caraddonContentUnlocks.meta
   - etc.

### Vehicle Weapons (Loaded after all other meta files)

7. **vehicleweapons_*.meta** - Vehicle-specific weapon configurations
   - Example: vehicleweapons_m113a1.meta
   - Example: vehicleweapons_t72b3m.meta

### Resource Files (Loaded last)

8. **RPF containers**
   - vehicles.rpf
   - weapons.rpf

## Why Order Matters

The loading order is important because:

1. **Dependency Resolution**: Some files reference data defined in others. For example:
   - `vehicles.meta` references handling data from `handling.meta`
   - `carvariations.meta` references colors from `carcols.meta`
   - `vehicleweapons_*.meta` files reference vehicles from `vehicles.meta`

2. **Game Engine Requirements**: GTA V's game engine expects certain data to be available before processing dependent files.

3. **Crash Prevention**: Loading files in the wrong order can cause the game to crash or behave unexpectedly.

## Implementation Details

The DLCMerger implements this ordering in the `OrderContentXmlFiles()` method:

```csharp
private List<string> OrderContentXmlFiles(List<string> files)
{
    // Define the priority order for vehicle-related files
    var priorityOrder = new Dictionary<string, int>
    {
        { "handling.meta", 1 },
        { "vehiclelayouts.meta", 2 },
        { "vehicles.meta", 3 },
        { "carcols.meta", 4 },
        { "carvariations.meta", 5 }
    };
    
    // Separate files into categories
    // 1. Priority files (ordered)
    // 2. Other meta files (alphabetical)
    // 3. Vehicle weapons files (alphabetical)
    
    // Return combined list in correct order
}
```

## Example Output

Here's an example of a properly ordered `<filesToEnable>` section:

```xml
<filesToEnable>
    <Item>dlc_mymod:/data/handling.meta</Item>
    <Item>dlc_mymod:/data/vehiclelayouts.meta</Item>
    <Item>dlc_mymod:/data/vehicles.meta</Item>
    <Item>dlc_mymod:/data/carcols.meta</Item>
    <Item>dlc_mymod:/data/carvariations.meta</Item>
    <Item>dlc_mymod:/data/dlctext.meta</Item>
    <Item>dlc_mymod:/data/explosion.meta</Item>
    <Item>dlc_mymod:/data/weaponarchetypes.meta</Item>
    <Item>dlc_mymod:/data/vehicleweapons_m113a1.meta</Item>
    <Item>dlc_mymod:/data/vehicleweapons_t72b3m.meta</Item>
    <Item>dlc_mymod:/vehicles.rpf</Item>
    <Item>dlc_mymod:/weapons.rpf</Item>
</filesToEnable>
```

## Notes for Developers

- The ordering logic is automatically applied when generating content.xml files
- Vehicle weapons files are always placed after all other meta files
- RPF containers are always placed last
- Files not in the priority list are sorted alphabetically within their category
- The order in `<dataFiles>` section mirrors the order in `<filesToEnable>`