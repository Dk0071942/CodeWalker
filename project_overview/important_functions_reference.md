# CodeWalker Important Functions Reference

This document provides a comprehensive list of important public methods and functions in the CodeWalker project, organized by category and class.

## Table of Contents
- [GameFile Classes](#gamefile-classes)
- [File Management](#file-management)
- [Rendering Functions](#rendering-functions)
- [World Management](#world-management)
- [Model Viewing](#model-viewing)
- [Utility Functions](#utility-functions)
- [Tool Functions](#tool-functions)

## GameFile Classes

### YmapFile (Map Files)
**Location**: `CodeWalker.Core/GameFiles/FileTypes/YmapFile.cs`

| Method | Purpose |
|--------|---------|
| `Load(byte[] data)` | Load from raw compressed ymap data |
| `Load(byte[] data, RpfFileEntry entry)` | Load from RPF entry |
| `Save()` | Save ymap to byte array |
| `BuildCEntityDefs()` | Build entity definitions |
| `BuildCCarGens()` | Build car generators |
| `BuildInstances()` | Build instance data |
| `BuildLodLights()` | Build LOD lights |
| `BuildDistantLodLights()` | Build distant LOD lights |
| `AddEntity(YmapEntityDef ent)` | Add entity to map |
| `RemoveEntity(YmapEntityDef ent)` | Remove entity from map |
| `AddCarGen(YmapCarGen cargen)` | Add car generator |
| `RemoveCarGen(YmapCarGen cargen)` | Remove car generator |
| `AddLodLight(YmapLODLight lodlight)` | Add LOD light |
| `RemoveLodLight(YmapLODLight lodlight)` | Remove LOD light |
| `AddGrassBatch(YmapGrassInstanceBatch batch)` | Add grass batch |
| `RemoveGrassBatch(YmapGrassInstanceBatch batch)` | Remove grass batch |
| `SetName(string newname)` | Set map name |
| `CalcExtents()` | Calculate map extents |
| `CalcFlags()` | Calculate map flags |
| `InitYmapEntityArchetypes(GameFileCache gfc)` | Initialize entity archetypes |

### YtypFile (Type/Archetype Files)
**Location**: `CodeWalker.Core/GameFiles/FileTypes/YtypFile.cs`

| Method | Purpose |
|--------|---------|
| `Load(byte[] data)` | Load from raw data |
| `Load(byte[] data, RpfFileEntry entry)` | Load from RPF entry |
| `Save()` | Save ytyp to byte array |
| `AddArchetype(Archetype archetype)` | Add archetype |
| `AddArchetype()` | Create and add new archetype |
| `RemoveArchetype(Archetype archetype)` | Remove archetype |

### YdrFile (Drawable Files)
**Location**: `CodeWalker.Core/GameFiles/FileTypes/YdrFile.cs`

| Method | Purpose |
|--------|---------|
| `Load(byte[] data)` | Load from raw data |
| `Load(byte[] data, RpfFileEntry entry)` | Load from RPF entry |
| `Save()` | Save ydr to byte array |
| `GetVersion(bool gen9)` | Get file version |
| `GetXml(YdrFile ydr, string outputFolder)` | Export to XML |
| `GetYdr(string xml, string inputFolder)` | Import from XML |

### YmapEntityDef (Map Entities)
**Location**: `CodeWalker.Core/GameFiles/FileTypes/YmapFile.cs`

| Method | Purpose |
|--------|---------|
| `SetArchetype(Archetype arch)` | Set entity archetype |
| `SetPosition(Vector3 pos)` | Set entity position |
| `SetOrientation(Quaternion ori)` | Set entity rotation |
| `SetScale(Vector3 s)` | Set entity scale |
| `UpdateEntityHash()` | Update entity hash |
| `SetPivotPosition(Vector3 pos)` | Set pivot position |
| `SetPivotOrientation(Quaternion ori)` | Set pivot rotation |

## File Management

### GameFileCache
**Location**: `CodeWalker.Core/GameFiles/GameFileCache.cs`

| Method | Purpose |
|--------|---------|
| `Init(Action<string> updateStatus, Action<string> errorLog)` | Initialize cache |
| `Clear()` | Clear cache |
| `SetDlcLevel(string dlc, bool enable)` | Enable/disable DLC |
| `SetModsEnabled(bool enable)` | Enable/disable mods |
| `GetArchetype(uint hash)` | Get archetype by hash |
| `GetYdr(uint hash)` | Get YDR file by hash |
| `GetYdd(uint hash)` | Get YDD file by hash |
| `GetYtd(uint hash)` | Get YTD file by hash |
| `GetYmap(uint hash)` | Get YMAP file by hash |
| `GetYft(uint hash)` | Get YFT file by hash |
| `GetYbn(uint hash)` | Get YBN file by hash |
| `GetYcd(uint hash)` | Get YCD file by hash |
| `GetYnv(uint hash)` | Get YNV file by hash |
| `AddProjectFile(GameFile f)` | Add project file |
| `RemoveProjectFile(GameFile f)` | Remove project file |
| `AddProjectArchetype(Archetype a)` | Add project archetype |
| `RemoveProjectArchetype(Archetype a)` | Remove project archetype |

### RpfManager
**Location**: `CodeWalker.Core/GameFiles/RpfManager.cs`

| Method | Purpose |
|--------|---------|
| `Init(string folder, bool gen9, ...)` | Initialize RPF manager |
| `FindRpfFile(string path)` | Find RPF file by path |
| `GetEntry(string path)` | Get RPF entry by path |
| `GetFileData(string path)` | Get file data as byte array |
| `GetFileUTF8Text(string path)` | Get file as UTF8 text |
| `GetFileXml(string path)` | Get file as XML document |
| `GetFile<T>(string path)` | Get typed game file |
| `LoadFile<T>(T file, RpfEntry e)` | Load file from entry |
| `BuildBaseJenkIndex()` | Build Jenkins hash index |

## Rendering Functions

### Renderer
**Location**: `CodeWalker/Rendering/Renderer.cs`

| Method | Purpose |
|--------|---------|
| `Init()` | Initialize renderer |
| `Start()` | Start rendering |
| `DeviceCreated(Device device, int width, int height)` | Handle device creation |
| `DeviceDestroyed()` | Handle device destruction |
| `BuffersResized(int width, int height)` | Handle buffer resize |
| `Update(float elapsed, int mouseX, int mouseY)` | Update renderer |
| `BeginRender(DeviceContext ctx)` | Begin render frame |
| `RenderSkyAndClouds()` | Render sky and clouds |
| `RenderQueued()` | Render queued objects |
| `RenderFinalPass()` | Render final pass |
| `EndRender()` | End render frame |
| `SetTimeOfDay(float hour)` | Set time of day |
| `SetWeatherType(string name)` | Set weather type |
| `SetCameraMode(string modestr)` | Set camera mode |
| `RenderMarkers(List<MapMarker> batch)` | Render map markers |
| `RenderTransformWidget(TransformWidget widget)` | Render transform widget |
| `RenderSelectionEntityPivot(YmapEntityDef ent)` | Render entity pivot |
| `RenderBrushRadiusOutline(...)` | Render brush outline |
| `Invalidate(Bounds bounds)` | Invalidate bounds |
| `Invalidate(BasePathData path)` | Invalidate path |
| `Invalidate(YmapGrassInstanceBatch batch)` | Invalidate grass |

## World Management

### WorldForm
**Location**: `CodeWalker/WorldForm.cs`

| Method | Purpose |
|--------|---------|
| `InitScene(Device device)` | Initialize 3D scene |
| `RenderScene(DeviceContext context)` | Render world scene |
| `SetCameraTransform(Vector3 pos, Quaternion rot)` | Set camera transform |
| `SetCameraClipPlanes(float znear, float zfar)` | Set clip planes |
| `GetCameraPosition()` | Get camera position |
| `GetCameraViewDir()` | Get camera direction |
| `SelectObject(object obj, object parent, bool addSelection)` | Select object |
| `SelectItem(MapSelection mhit, bool addSelection, ...)` | Select map item |
| `SelectMulti(MapSelection[] items, bool addSelection, ...)` | Select multiple items |
| `SetWidgetPosition(Vector3 pos, bool enableUndo)` | Set widget position |
| `SetWidgetRotation(Quaternion q, bool enableUndo)` | Set widget rotation |
| `SetWidgetScale(Vector3 s, bool enableUndo)` | Set widget scale |
| `UpdatePathYndGraphics(YndFile ynd, bool fullupdate)` | Update path graphics |
| `UpdateCollisionBoundsGraphics(Bounds b)` | Update collision graphics |
| `UpdateNavYnvGraphics(YnvFile ynv, bool fullupdate)` | Update navmesh graphics |
| `UpdateScenarioGraphics(YmtFile ymt, bool fullupdate)` | Update scenario graphics |
| `UpdateGrassBatchGraphics(YmapGrassInstanceBatch batch)` | Update grass graphics |
| `UpdateAudioPlacementGraphics(RelFile rel)` | Update audio graphics |
| `GetSpaceMouseRay()` | Get mouse ray in 3D space |
| `Raycast(Ray ray)` | Perform raycast |
| `ShowModel(string name)` | Show specific model |

## Model Viewing

### ModelForm
**Location**: `CodeWalker/Forms/ModelForm.cs`

| Method | Purpose |
|--------|---------|
| `InitScene(Device device)` | Initialize 3D scene |
| `RenderScene(DeviceContext context)` | Render model scene |
| `LoadModel(YdrFile ydr)` | Load YDR model |
| `LoadModels(YddFile ydd)` | Load YDD models |
| `LoadModel(YftFile yft)` | Load YFT model |
| `LoadModel(YbnFile ybn)` | Load collision model |
| `LoadParticles(YptFile ypt)` | Load particle effects |
| `LoadNavmesh(YnvFile ynv)` | Load navigation mesh |
| `SetCameraPosition(Vector3 p, float distance)` | Set camera position |
| `SetWidgetTransform(Vector3 p, Quaternion q, Vector3 s)` | Set widget transform |
| `SetWidgetMode(WidgetMode mode)` | Set widget mode |
| `UpdateEmbeddedTextures()` | Update embedded textures |

## Utility Functions

### GTACrypto
**Location**: `CodeWalker.Core/GameFiles/Utils/GTACrypto.cs`

| Method | Purpose |
|--------|---------|
| `DecryptAES(byte[] data)` | Decrypt AES data |
| `EncryptAES(byte[] data)` | Encrypt AES data |
| `DecryptAESData(byte[] data, byte[] key, int rounds)` | Decrypt with key |
| `EncryptAESData(byte[] data, byte[] key, int rounds)` | Encrypt with key |
| `GetNGKey(string name, uint length)` | Get NG encryption key |
| `DecryptNG(byte[] data, string name, uint length)` | Decrypt NG data |
| `EncryptNG(byte[] data, string name, uint length)` | Encrypt NG data |

### Jenk (Jenkins Hash)
**Location**: `CodeWalker.Core/GameFiles/Utils/Jenk.cs`

Common static methods for string hashing used throughout the codebase.

## Tool Functions

### BrowseForm (RPF Explorer)
**Location**: `CodeWalker/Tools/BrowseForm.cs`

Main form for browsing and extracting RPF archives.

### Project Forms
**Location**: `CodeWalker/Project/`

Various panels for editing different game file types:
- EditProjectPanel - Main project settings
- EditYmapEntityPanel - Entity editing
- EditYmapCarGenPanel - Car generator editing
- EditYtypArchetypePanel - Archetype editing
- EditYndNodePanel - Path node editing
- EditYnvPolyPanel - Navigation polygon editing
- EditScenarioNodePanel - Scenario editing
- EditAudioZonePanel - Audio zone editing

### MetaForm
**Location**: `CodeWalker/Forms/MetaForm.cs`

Form for viewing and editing Meta/PSO/RBF files.

### YtdForm
**Location**: `CodeWalker/Forms/YtdForm.cs`

Form for viewing and editing texture dictionaries.

### YcdForm
**Location**: `CodeWalker/Forms/YcdForm.cs`

Form for viewing clip dictionaries and animations.

## Common Usage Patterns

### Loading a File
```csharp
// Using GameFileCache
var ydr = gameFileCache.GetYdr(hash);

// Using RpfManager
var data = rpfManager.GetFileData("path/to/file.ydr");
var ydr = new YdrFile();
ydr.Load(data);
```

### Creating/Modifying Entities
```csharp
// Add entity to ymap
var entity = new YmapEntityDef();
entity.SetPosition(new Vector3(x, y, z));
entity.SetArchetype(archetype);
ymap.AddEntity(entity);
```

### Rendering Objects
```csharp
// In render loop
renderer.BeginRender(context);
renderer.RenderSkyAndClouds();
renderer.RenderQueued();
renderer.RenderFinalPass();
renderer.EndRender();
```

This reference covers the most commonly used public methods across the CodeWalker project. For more detailed information, refer to the source code documentation and comments.