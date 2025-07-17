# YFT XML Structure Analysis

## Overview
YFT (Yft) files are Fragment files in GTA V that contain vehicle and prop models. They consist of drawable objects, physics data, and associated textures.

## XML Structure

Based on the code analysis, here's the complete YFT XML structure:

```xml
<?xml version="1.0" encoding="UTF-8"?>
<Fragment>
  <!-- Basic Fragment Properties -->
  <Name>prop_name</Name>
  <BoundingSphereCenter x="0" y="0" z="0"/>
  <BoundingSphereRadius value="10.5"/>
  
  <!-- Unknown/Reserved values that must be preserved -->
  <UnknownB0 value="0"/>
  <UnknownB8 value="0"/>
  <UnknownBC value="0"/>
  <UnknownC0 value="0"/>
  <UnknownC4 value="1"/>
  <UnknownCC value="0"/>
  
  <!-- Physics Properties -->
  <GravityFactor value="1.0"/>
  <BuoyancyFactor value="1.0"/>
  
  <!-- Main Drawable (optional) -->
  <Drawable>
    <!-- Drawable properties (see below) -->
  </Drawable>
  
  <!-- Drawable Array (optional) -->
  <DrawableArray>
    <Item>
      <!-- Drawable properties (see below) -->
    </Item>
  </DrawableArray>
  
  <!-- Bone Transforms (optional) -->
  <BoneTransforms>
    <!-- Bone transform data -->
  </BoneTransforms>
  
  <!-- Physics (optional) -->
  <Physics>
    <!-- Physics LOD group data -->
  </Physics>
  
  <!-- Vehicle Glass Windows (optional) -->
  <VehicleGlassWindows>
    <!-- Vehicle glass window definitions -->
  </VehicleGlassWindows>
  
  <!-- Glass Windows (optional) -->
  <GlassWindows>
    <!-- Glass window items -->
  </GlassWindows>
  
  <!-- Lights (optional) -->
  <Lights>
    <!-- Light attribute items -->
  </Lights>
  
  <!-- Cloths (optional) -->
  <Cloths>
    <Item>
      <!-- Environment cloth data -->
    </Item>
  </Cloths>
</Fragment>
```

## Drawable Structure

Each Drawable (FragDrawable) contains:

```xml
<Drawable>
  <!-- Bounding Information -->
  <BoundingSphereCenter x="0" y="0" z="0"/>
  <BoundingSphereRadius value="5.0"/>
  <BoundingBoxMin x="-1" y="-1" z="-1"/>
  <BoundingBoxMax x="1" y="1" z="1"/>
  
  <!-- LOD Distances -->
  <LodDistHigh value="50.0"/>
  <LodDistMed value="100.0"/>
  <LodDistLow value="200.0"/>
  <LodDistVlow value="500.0"/>
  
  <!-- Render Flags -->
  <FlagsHigh value="0"/>
  <FlagsMed value="0"/>
  <FlagsLow value="0"/>
  <FlagsVlow value="0"/>
  
  <!-- Shader Group with Textures -->
  <ShaderGroup>
    <TextureDictionary>
      <Item>
        <Name>texture_name</Name>
        <NameHash>texture_hash</NameHash>
        <Width value="512"/>
        <Height value="512"/>
        <MipLevels value="9"/>
        <Format>DXT5</Format>
        <FileName>texture_name.dds</FileName>
      </Item>
    </TextureDictionary>
    <Shaders>
      <Item>
        <!-- Shader parameters -->
      </Item>
    </Shaders>
  </ShaderGroup>
  
  <!-- Skeleton (optional) -->
  <Skeleton>
    <!-- Bone definitions -->
  </Skeleton>
  
  <!-- Joints (optional) -->
  <Joints>
    <!-- Joint data -->
  </Joints>
  
  <!-- DrawableModels -->
  <DrawableModelsHigh>
    <!-- High LOD models -->
  </DrawableModelsHigh>
  <DrawableModelsMedium>
    <!-- Medium LOD models -->
  </DrawableModelsMedium>
  <DrawableModelsLow>
    <!-- Low LOD models -->
  </DrawableModelsLow>
  <DrawableModelsVeryLow>
    <!-- Very low LOD models -->
  </DrawableModelsVeryLow>
</Drawable>
```

## Texture Handling

### 1. Texture Dictionary Structure
- Textures are stored in the ShaderGroup's TextureDictionary
- Each texture has:
  - Name: Human-readable name
  - NameHash: Hash value used for lookups
  - Dimensions: Width, Height
  - MipLevels: Number of mipmap levels
  - Format: Texture format (DXT1, DXT5, etc.)
  - FileName: Reference to external .dds file

### 2. Texture References in Shaders
- Shaders reference textures by their NameHash
- During XML import, texture references are resolved from the TextureDictionary
- The system automatically links shader texture parameters to embedded textures
- Texture name hashes are typically the Jenkins hash of the texture name
- When modifying textures, ensure the NameHash matches the actual texture name

### 3. External Texture Files
- When exporting to XML, textures are saved as .dds files in a specified folder
- When importing from XML, .dds files are loaded and embedded into the YFT

## Critical Fields to Preserve

### Fragment Level
1. **Name**: The fragment name
2. **BoundingSphere**: Center and radius for culling
3. **Unknown fields**: UnknownB0, B8, BC, C0, C4, CC - preserve exact values
4. **Physics factors**: GravityFactor, BuoyancyFactor

### Drawable Level
1. **Bounding data**: All bounding box and sphere values
2. **LOD distances**: Must match the game's LOD system
3. **Flags**: Render flags for each LOD level
4. **Texture references**: Must maintain hash consistency

### Texture Level
1. **NameHash**: Critical for texture lookups
2. **Format**: Must be a supported GTA V texture format
3. **Dimensions**: Must be power of 2
4. **MipLevels**: Should match the texture data

## Common Issues and Solutions

### 1. Missing Textures
- Ensure all referenced textures exist in the TextureDictionary
- Check that texture hashes match between references and dictionary entries

### 2. Broken Physics
- Preserve all Unknown fields exactly as they were
- Ensure physics LOD groups match drawable LODs

### 3. Rendering Issues
- Verify shader parameters match texture formats
- Check that all LOD levels have appropriate models
- Ensure render flags are set correctly

## Validation Checklist

When modifying YFT XML:
1. ✓ All texture references resolve to dictionary entries
2. ✓ Texture formats are valid GTA V formats
3. ✓ All Unknown fields preserved with original values
4. ✓ Bounding spheres/boxes encompass all geometry
5. ✓ LOD distances are in ascending order
6. ✓ Physics data matches drawable structure
7. ✓ All referenced .dds files exist (for import)
8. ✓ Texture dimensions are power of 2
9. ✓ MipLevels match texture size
10. ✓ Fragment name is set

## Code References

Key classes for YFT XML handling:
- `YftFile`: Main YFT file handler
- `FragType`: Fragment data structure
- `FragDrawable`: Drawable within fragment
- `YftXml`: XML export logic
- `XmlYft`: XML import logic
- `TextureDictionary`: Texture storage
- `ShaderGroup`: Shader and texture references
- `FragVehicleGlassWindows`: Vehicle glass data
- `FragPhysicsLODGroup`: Physics collision data

## Important Notes

1. **Gen9 Format**: The code supports both legacy and Gen9 (Next-Gen) formats. Version 171 is Gen9, version 162 is legacy.

2. **Texture Embedding**: Unlike YDR files where textures can be external, YFT files typically embed all textures in the fragment's TextureDictionary.

3. **Physics Integration**: The physics data is tightly integrated with the drawable data. Changes to geometry should be reflected in physics bounds.

4. **Glass Windows**: Vehicle YFTs have special glass window data that includes shatter maps and projection matrices for realistic glass breaking effects.

5. **Memory Layout**: The file uses careful memory alignment and specific data structures that must be preserved when converting to/from XML.