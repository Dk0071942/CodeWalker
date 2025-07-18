# GTA Auto Project - Data Constraints Documentation

## Overview
This document outlines all critical constraints that must be preserved when processing GTA V vehicle meta files. These constraints ensure data integrity and compatibility with the game engine.

## File Naming Conventions

### Meta File Prefixes
The system uses specific prefixes to identify different types of meta files:

1. **Vehicle Files**: `tmp_v_*` or `vehicles.meta`
2. **Handling Files**: `tmp_h_*` or `handling.meta`
3. **Car Variations Files**: `tmp_ca_*` or `carvariations.meta`
4. **Car Colors Files**: `tmp_cid_*` or `carcols.meta`
5. **Layout Files**: `tmp_l_*` (vehicle layout configurations)

### DLC Naming Pattern
Files follow a consistent naming pattern:
- `tmp_[type]_[dlc_identifier].meta`
- Examples: `tmp_v_dlc1.meta`, `tmp_h_private1.meta`, `tmp_ca_vip2.meta`

## Critical Data Relationships

### 1. Model Name Linking (Primary Key)
**Constraint**: The `modelName` field must be consistent across all related files.

```xml
<!-- vehicles.meta -->
<modelName>su7</modelName>

<!-- carvariations.meta -->
<modelName>su7</modelName>

<!-- Must match exactly (case-insensitive) -->
```

### 2. Handling ID Linking
**Constraint**: The `handlingId` in vehicles.meta must match `handlingName` in handling.meta.

```xml
<!-- vehicles.meta -->
<handlingId>su7</handlingId>

<!-- handling.meta -->
<handlingName>su7</handlingName>
```

### 3. Kit Name Linking
**Constraint**: Kit names in carvariations.meta must match kit definitions in carcols.meta.

```xml
<!-- carvariations.meta -->
<kits>
    <Item>0_default_modkit</Item>
    <Item>999_emc_template_modkit</Item>
</kits>

<!-- carcols.meta -->
<kitName>999_emc_template_modkit</kitName>
```

### 4. Layout Linking
**Constraint**: Layout files define vehicle animation and entry/exit configurations that may be referenced by vehicles.

```xml
<!-- Layout files contain ClipSetMaps for vehicle animations -->
<Name>ENTRY_CLIPSET_MAP_F250TR_STD_FRONT_RIGHT</Name>
```

### 5. Weapon Linking (for Armed Vehicles)
**Constraint**: Armed vehicles have weapon definitions that must be preserved across files.

```xml
<!-- handling.meta - CVehicleWeaponHandlingData -->
<uWeaponHash>
    <Name>VEHICLE_ADF11F_UAV</Name>
    <Item>VEHICLE_ADF11F_AIM9_ROCKET</Item>
</uWeaponHash>

<!-- vehicleweapons_[model].meta - Must define matching weapons -->
<Name>VEHICLE_ADF11F_UAV</Name>
<Name>VEHICLE_ADF11F_AIM9_ROCKET</Name>
```

### 6. Color Index Relationships
**Constraint**: Color indices in carvariations.meta reference game color palettes.

```xml
<!-- carvariations.meta - indices define primary/secondary/pearl/wheel colors -->
<indices content="char_array">
    132  <!-- Primary color -->
    6    <!-- Secondary color -->
    6    <!-- Pearl color -->
    6    <!-- Wheel color -->
    70   <!-- Interior color (optional) -->
    70   <!-- Dashboard color (optional) -->
</indices>
```

### 7. Livery Configuration
**Constraint**: Livery availability must match the FLAG_HAS_LIVERY in vehicles.meta.

```xml
<!-- vehicles.meta -->
<flags>FLAG_HAS_LIVERY</flags>

<!-- carvariations.meta - Must have corresponding livery entries -->
<liveries>
    <Item value="true"/>  <!-- Livery 1 available -->
    <Item value="false"/> <!-- Livery 2 not available -->
</liveries>
```

## Data Type Constraints

### 1. Numeric Values
**Constraint**: All numeric values must maintain their precision and format.

- **Float values**: Must include decimal point (e.g., `value="1.000000"`)
- **Integer values**: No decimal point (e.g., `value="50000"`)
- **Vector values**: Must maintain x, y, z structure

### 2. String Values
**Constraint**: Certain string fields have specific requirements:

- **audioNameHash**: Must be a valid game audio reference or "null"
- **gameName**: Must be a valid game vehicle class name
- **vehicleMakeName**: Must be a valid manufacturer name
- **layout**: Must match predefined layout constants

### 3. Boolean Values
**Constraint**: Boolean values must be lowercase "true" or "false".

```xml
<Item value="true"/>
<Item value="false"/>
```

## XML Structure Constraints

### 1. Root Element Names
**Constraint**: Each file type has a specific root element that must be preserved:

- **vehicles.meta**: `<CVehicleModelInfo__InitDataList>`
- **handling.meta**: `<CHandlingDataMgr>`
- **carvariations.meta**: `<CVehicleModelInfoVariation>`
- **carcols.meta**: `<CVehicleModelInfoVarGlobal>`
- **layout files**: `<CVehicleMetadataMgr>`

### 2. Nested Structure
**Constraint**: The hierarchical structure must be maintained exactly:

```xml
<CVehicleModelInfo__InitDataList>
    <residentTxd>vehshare</residentTxd>
    <InitDatas>
        <Item>
            <!-- Vehicle data -->
        </Item>
    </InitDatas>
</CVehicleModelInfo__InitDataList>
```

### 3. Array Elements
**Constraint**: Arrays use specific patterns that must be preserved:

```xml
<!-- Character array format -->
<indices content="char_array">
    0
    0
    0
    156
</indices>

<!-- Item list format -->
<linkedModels>
    <Item>model_name_1</Item>
    <Item>model_name_2</Item>
</linkedModels>
```

## Processing Constraints

### 1. Case Sensitivity
**Constraint**: All comparisons must be case-insensitive, but original case must be preserved in output.

### 2. File Encoding
**Constraint**: All files must maintain UTF-8 encoding with XML declaration:
```xml
<?xml version='1.0' encoding='UTF-8'?>
```

### 3. Whitespace Preservation
**Constraint**: Original formatting and indentation should be preserved where possible.

### 4. Comment Preservation
**Constraint**: XML comments should be preserved during processing.

## Validation Constraints

### 1. Required Fields
**Constraint**: Each vehicle entry must have minimum required fields:

**vehicles.meta**:
- modelName
- txdName
- handlingId
- gameName
- vehicleMakeName

**handling.meta**:
- handlingName
- fMass
- fInitialDriveForce
- nInitialDriveGears

**carvariations.meta**:
- modelName
- colors (with indices)
- kits

### 2. Value Ranges
**Constraint**: Numeric values must be within acceptable ranges:

- **fMass**: 1.0 - 50000.0
- **nInitialDriveGears**: 1 - 10
- **Color indices**: 0 - 255
- **Kit ID**: 0 - 999

### 3. Reference Integrity
**Constraint**: All referenced entities must exist:

- Handling IDs referenced in vehicles.meta must exist in handling.meta
- Kit names referenced in carvariations.meta must exist in carcols.meta
- Model names must be consistent across all files

## Merge/Separate Constraints

### 1. Duplicate Handling
**Constraint**: When merging files with duplicate entries:
- Use the latest version (based on file order)
- Log all conflicts for review
- Never silently drop data

### 2. Orphan Prevention
**Constraint**: When separating files:
- Include all related data for each vehicle
- Never create partial vehicle entries
- Maintain referential integrity

### 3. Kit Preservation
**Constraint**: Tuning kits require special handling:
- Preserve all kit definitions
- Maintain kit-to-vehicle associations
- Include all linked models

## Additional Relationships

### 1. Vehicle Type and Class Consistency
**Constraint**: Vehicle type must match handling characteristics and class definitions.

```xml
<!-- vehicles.meta -->
<type>VEHICLE_TYPE_PLANE</type>
<vehicleClass>VC_PLANE</vehicleClass>
<layout>LAYOUT_PLANE_MAMMATUS</layout>

<!-- handling.meta - Must have matching sub-handling data -->
<SubHandlingData>
    <Item type="CFlyingHandlingData">
        <handlingType>HANDLING_TYPE_FLYING</handlingType>
    </Item>
</SubHandlingData>
```

### 2. Audio Hash Relationships
**Constraint**: Audio references must be valid game audio definitions.

```xml
<!-- vehicles.meta -->
<audioNameHash>lazer</audioNameHash>

<!-- Must match predefined game audio entries -->
```

### 3. Kit ID Uniqueness
**Constraint**: Kit IDs must be unique across all carcols files.

```xml
<!-- carcols.meta -->
<kitName>565_adf11f_modkit</kitName>
<id value="565"/> <!-- Must be unique -->
```

### 4. Linked Models in Kits
**Constraint**: All linked models in kit definitions must exist as separate model files.

```xml
<!-- carcols.meta -->
<modelName>adf_pylon_r</modelName>
<linkedModels>
    <Item>adf_pylon_l</Item> <!-- Must exist as model file -->
</linkedModels>
```

### 5. Plate Type and Probability
**Constraint**: Plate types in carvariations must sum to 100% probability.

```xml
<!-- vehicles.meta -->
<plateType>VPT_NONE</plateType>

<!-- carvariations.meta -->
<plateProbabilities>
    <Item>
        <Name>police guv plate</Name>
        <Value value="100"/> <!-- Total must be 100 -->
    </Item>
</plateProbabilities>
```

## Special Cases

### 1. Template Files
**Constraint**: Template files in `data/template/` contain reusable kit definitions that must be preserved and can be referenced by multiple vehicles.

### 2. Weapon Vehicle Integration
**Constraint**: Vehicles with weapons (found in `tmp_weapons/`) require additional weapon archetype definitions that must be included when processing these vehicles.

### 3. Character and Animation Data
**Constraint**: Files in `tmp_characters/` and `tmp_clipsets/` contain animation and character data that may be referenced by vehicles and must be preserved.

## Output Constraints

### 1. File Organization
**Constraint**: Output must maintain logical organization:
- Separate folders for vehicles with/without tuning
- Consistent file naming
- Preserve source file references

### 2. Metadata Generation
**Constraint**: Generated metadata files must include:
- Source file information
- Processing timestamp
- Conflict resolution log
- Validation results

### 3. Backward Compatibility
**Constraint**: Output files must remain compatible with:
- Original game engine
- Common mod tools
- FiveM/RageMP servers

## Summary of All Critical Relationships

### Primary Relationships Map
```
vehicles.meta (V) ←→ handling.meta (H) ←→ carvariations.meta (CA) ←→ carcols.meta (CID)
       ↓                    ↓                      ↓                        ↓
   modelName           handlingName            modelName                kitName
   handlingId ─────→   handlingName            colors                   id (unique)
   flags               SubHandlingData         kits ─────────────────→  kitName
   type                handlingType            liveries                 visibleMods
   vehicleClass        weaponData              plateProbabilities      linkedModels
   audioNameHash                               lightSettings
   layout                                      sirenSettings
```

### Complete Constraint Checklist
1. **Model Name**: Consistent across V, CA files (case-insensitive)
2. **Handling ID**: V.handlingId = H.handlingName
3. **Kit Names**: CA.kits items = CID.kitName
4. **Kit IDs**: Must be unique across all CID files
5. **Vehicle Type**: Must match handling type and class
6. **Weapons**: H.uWeaponHash items must exist in weapon files
7. **Colors**: 4-6 indices in specific order
8. **Liveries**: Must match FLAG_HAS_LIVERY
9. **Plate Probabilities**: Must sum to 100%
10. **Linked Models**: Must exist as model files
11. **Audio References**: Must be valid game audio
12. **Layout References**: Must match vehicle type

## Summary
These constraints ensure that the GTA Auto Project maintains data integrity throughout all processing operations. Any modifications to the processing logic must respect these constraints to prevent game crashes, missing features, or data corruption.