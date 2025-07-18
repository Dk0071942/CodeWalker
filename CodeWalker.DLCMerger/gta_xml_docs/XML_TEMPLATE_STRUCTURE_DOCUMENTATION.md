# XML Template Structure Documentation

## Overview

This document provides comprehensive documentation for the XML template structures used in the DLCMerger component of CodeWalker. The templates define the correct XML container structures for Grand Theft Auto V meta files during DLC merging operations.

## Critical Importance

**XML template structures must match GTA V's exact format specifications.** Incorrect container names or missing elements will result in:
- Game crashes during DLC loading
- Mod incompatibility issues  
- Silent failures where content is not loaded
- Corrupted game state

## Related Documentation

- [Content.xml File Ordering Requirements](./CONTENT_XML_FILE_ORDERING.md) - Critical information about file loading order in content.xml

## Template Structure Reference

### 1. carcols.meta - Vehicle Modification Colors

**Purpose**: Defines vehicle modification kits, colors, and visual customization options.

**Correct Structure**:
```xml
<CVehicleModelInfoVarGlobal>
  <Kits>
    <!-- Vehicle modification kit items -->
  </Kits>
  <Lights />
</CVehicleModelInfoVarGlobal>
```

**Critical Elements**:
- **Container**: `CVehicleModelInfoVarGlobal` (NOT `CVehicleModColours`)
- **Required Elements**: `<Kits>` for modification data, `<Lights />` empty element
- **Item Container**: `<Kits>` contains `<Item>` elements with kit definitions

**Common Issues Fixed**:
- ❌ Previous: `<CVehicleModColours>` (incorrect container name)
- ✅ Current: `<CVehicleModelInfoVarGlobal>` (correct GTA V format)
- ❌ Previous: Missing `<Lights />` element
- ✅ Current: Includes required empty `<Lights />` element

### 2. carvariations.meta - Vehicle Variations

**Purpose**: Defines color schemes, liveries, and kit associations for vehicles.

**Correct Structure**:
```xml
<CVehicleModelInfoVariation>
  <variationData>
    <!-- Vehicle variation items -->
  </variationData>
</CVehicleModelInfoVariation>
```

**Critical Elements**:
- **Container**: `CVehicleModelInfoVariation` (NOT `CVehicleVariations`)
- **Data Container**: `<variationData>` contains `<Item>` elements
- **Item Structure**: Each `<Item>` defines model-specific color and kit data

**Common Issues Fixed**:
- ❌ Previous: `<CVehicleVariations>` (incorrect container name)
- ✅ Current: `<CVehicleModelInfoVariation>` (correct GTA V format)

### 3. vehicles.meta - Vehicle Model Information

**Purpose**: Defines core vehicle properties, handling references, and game behavior.

**Correct Structure**:
```xml
<CVehicleModelInfo__InitDataList>
  <residentTxd>vehshare</residentTxd>
  <residentAnims />
  <InitDatas>
    <!-- Vehicle initialization data items -->
  </InitDatas>
  <txdRelationships>
    <!-- Texture dictionary relationships -->
  </txdRelationships>
</CVehicleModelInfo__InitDataList>
```

**Critical Elements**:
- **Container**: `CVehicleModelInfo__InitDataList` (note double underscore)
- **Required Elements**: `residentTxd`, `residentAnims`, `InitDatas`, `txdRelationships`
- **Item Container**: `<InitDatas>` contains vehicle model definitions

**Status**: ✅ Already correct in current implementation

### 4. handling.meta - Vehicle Handling Data

**Purpose**: Defines vehicle physics, handling characteristics, and weapon systems.

**Correct Structure**:
```xml
<CHandlingDataMgr>
  <HandlingData>
    <!-- Vehicle handling items -->
  </HandlingData>
</CHandlingDataMgr>
```

**Critical Elements**:
- **Container**: `CHandlingDataMgr`
- **Data Container**: `<HandlingData>` contains `<Item>` elements
- **Item Structure**: Each `<Item>` defines vehicle physics parameters

**Status**: ✅ Already correct in current implementation

### 5. vehiclelayouts.meta - Vehicle Layout Information

**Purpose**: Defines seat positions, entry points, and animation data for vehicles.

**Correct Structure**:
```xml
<CVehicleMetadataMgr>
  <VehicleLayoutInfos>
    <!-- Layout definitions -->
  </VehicleLayoutInfos>
  <VehicleEntryPointInfos>
    <!-- Entry point definitions -->
  </VehicleEntryPointInfos>
  <VehicleExtraPointsInfos>
    <!-- Extra point definitions -->
  </VehicleExtraPointsInfos>
  <VehicleEntryPointAnimInfos>
    <!-- Animation definitions -->
  </VehicleEntryPointAnimInfos>
  <VehicleSeatInfos>
    <!-- Seat definitions -->
  </VehicleSeatInfos>
  <VehicleSeatAnimInfos>
    <!-- Seat animation definitions -->
  </VehicleSeatAnimInfos>
</CVehicleMetadataMgr>
```

**Critical Elements**:
- **Container**: `CVehicleMetadataMgr`
- **Multiple Containers**: Six different data containers for various aspects
- **Complex Structure**: Most complex template with multiple related data types

**Status**: ✅ Already correct in current implementation

## Implementation Location

**File**: `CodeWalker.DLCMerger/XmlMerger.cs`
**Method**: Private static field `XmlTemplates` (lines 22-82)
**Usage**: Referenced in `MergeXmlFiles()` method for template-based XML generation

## Validation Methodology

### Reference Source
Templates validated against manually extracted files from:
```
CodeWalker.DLCMerger/bin/Debug/net8.0/manually_extracted/dlc1.rpf/common/data/
```

### Validation Process
1. **Extract Reference Files**: Use working DLC content as ground truth
2. **Parse Structure**: Analyze XML container hierarchy and required elements
3. **Compare Templates**: Identify discrepancies between templates and references
4. **Apply Fixes**: Update template structures to match exact GTA V format
5. **Test Generation**: Verify generated XML files load correctly in game

### Evidence-Based Corrections
All corrections made based on direct comparison with functional GTA V DLC files:

| File Type | Reference File | Issue Found | Resolution |
|-----------|----------------|-------------|------------|
| carcols.meta | `/data/carcols.meta` | Wrong container name | `CVehicleModColours` → `CVehicleModelInfoVarGlobal` |
| carcols.meta | `/data/carcols.meta` | Missing element | Added required `<Lights />` element |
| carvariations.meta | `/data/carvariations.meta` | Wrong container name | `CVehicleVariations` → `CVehicleModelInfoVariation` |

## Best Practices

### Template Maintenance
1. **Always validate against working GTA V content**
2. **Use exact container names from game files**
3. **Include all required elements, even if empty**
4. **Maintain proper XML structure and indentation**
5. **Test generated content in game environment**

### Quality Assurance
1. **Reference Validation**: Compare against manually extracted DLC files
2. **Structure Verification**: Ensure all required elements present
3. **Name Accuracy**: Use exact class names from GTA V system
4. **Empty Element Handling**: Include required empty elements (e.g., `<Lights />`)
5. **Game Testing**: Verify generated DLCs load without errors

## Technical Notes

### Container Naming Convention
GTA V uses specific C++ class names as XML container elements:
- `CVehicleModelInfoVarGlobal` - Vehicle modification data
- `CVehicleModelInfoVariation` - Vehicle variation data  
- `CVehicleModelInfo__InitDataList` - Vehicle initialization data
- `CHandlingDataMgr` - Vehicle handling data
- `CVehicleMetadataMgr` - Vehicle metadata and layout

### XML Generation Process
1. **Template Selection**: Choose appropriate template based on file name
2. **Container Discovery**: Find all valid containers in template
3. **Item Extraction**: Extract items from source files
4. **Container Population**: Add items to correct containers
5. **Validation**: Ensure structure matches GTA V requirements

## Change History

### 2024-07-18 - Critical Structure Fixes
- **Fixed carcols.meta**: Updated container to `CVehicleModelInfoVarGlobal`, added `<Lights />` element
- **Fixed carvariations.meta**: Updated container to `CVehicleModelInfoVariation`
- **Validated all templates**: Compared against manually extracted reference files
- **Documented methodology**: Established evidence-based validation process

## Impact Assessment

### Before Fixes
- Generated DLCs could fail to load due to incorrect XML structure
- Game crashes possible with malformed carcols.meta files
- Silent failures where vehicle modifications not recognized

### After Fixes  
- Generated DLCs match exact GTA V format specifications
- Reliable loading and compatibility with game engine
- Proper vehicle modification and variation support

## Maintenance Guidelines

1. **Reference Files**: Always maintain current set of working reference files
2. **Validation Process**: Run validation against references before releases
3. **Testing Protocol**: Test generated DLCs in actual game environment
4. **Documentation Updates**: Update this document when templates change
5. **Version Control**: Track all template changes with detailed commit messages

---

**Last Updated**: 2024-07-18  
**Validated Against**: GTA V DLC Reference Files (dlc1.rpf)  
**Status**: All templates validated and corrected