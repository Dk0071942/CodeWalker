# CodeWalker Function Index

## Overview

This document provides a comprehensive index of all major functions, methods, and classes in CodeWalker. Use this as a quick reference to find existing functionality.

## Quick Navigation

- [Core File Operations](#core-file-operations)
- [GameFile Methods](#gamefile-methods)
- [Rendering Functions](#rendering-functions)
- [World Management](#world-management)
- [UI and Tools](#ui-and-tools)
- [Utility Functions](#utility-functions)
- [Resource Management](#resource-management)
- [Data Conversion](#data-conversion)

---

## Core File Operations

### GameFileCache - Central File Management
**Location**: `CodeWalker.Core/GameFiles/GameFileCache.cs`

| Method | Purpose | Usage |
|--------|---------|-------|
| `Init(Action<string> updateStatus, Action<string> errorLog)` | Initialize cache system | Called once at startup |
| `GetYmap(string name)` | Load YMAP file | `var ymap = cache.GetYmap("vespucci_01_0.ymap");` |
| `GetYtyp(string name)` | Load YTYP file | `var ytyp = cache.GetYtyp("v_int_1.ytyp");` |
| `GetYdr(string name)` | Load YDR model | `var ydr = cache.GetYdr("prop_bench_01a.ydr");` |
| `GetYtd(string name)` | Load texture dict | `var ytd = cache.GetYtd("prop_textures.ytd");` |
| `GetYbn(string name)` | Load collision bounds | `var ybn = cache.GetYbn("vespucci_01_0.ybn");` |
| `GetYnd(string name)` | Load path nodes | `var ynd = cache.GetYnd("nodes0.ynd");` |
| `GetYnv(string name)` | Load nav mesh | `var ynv = cache.GetYnv("navmesh0.ynv");` |
| `TryGetFile<T>(string name)` | Generic file loader | `if (cache.TryGetFile<YmapFile>(name, out var file))` |

### RpfManager - Archive Management
**Location**: `CodeWalker.Core/GameFiles/RpfManager.cs`

| Method | Purpose | Usage |
|--------|---------|-------|
| `Init(string folder, Action<string> updateStatus)` | Initialize RPF system | Called at startup |
| `GetFile(string path)` | Get file from any RPF | `var entry = RpfMan.GetFile("x64a.rpf\\models\\cdimages\\weapons.rpf");` |
| `GetAllFiles(string ext)` | Find all files by extension | `var allYmaps = RpfMan.GetAllFiles(".ymap");` |
| `CreateFileEntry(string name, string path, ref byte[] data)` | Create new RPF entry | For creating mods |

---

## GameFile Methods

### YmapFile - Map Placement Files
**Location**: `CodeWalker.Core/GameFiles/FileTypes/YmapFile.cs`

| Method | Purpose | Parameters |
|--------|---------|------------|
| `CreateEntity()` | Create new entity | Returns `YmapEntityDef` |
| `RemoveEntity(YmapEntityDef ent)` | Remove entity | Entity to remove |
| `CalcExtents()` | Recalculate bounds | None |
| `CalcFlags()` | Update content flags | None |
| `GetRootEntities()` | Get top-level entities | Returns entity array |
| `GetEntities(BoundingBox box)` | Spatial query | Bounding box |
| `GetEntity(uint hash)` | Find by hash | Entity name hash |

### YmapEntityDef - Entity Manipulation
**Location**: `CodeWalker.Core/GameFiles/FileTypes/YmapFile.cs`

| Method | Purpose | Parameters |
|--------|---------|------------|
| `SetPosition(Vector3 pos)` | Set world position | Position vector |
| `SetOrientation(Quaternion q)` | Set rotation | Quaternion rotation |
| `SetScale(Vector3 s)` | Set entity scale | Scale vector |
| `UpdateBB()` | Update bounding box | None |
| `GetArchetype()` | Get entity archetype | Returns `Archetype` |
| `SetArchetype(Archetype arch)` | Set archetype | New archetype |

### YtypFile - Archetype Definitions
**Location**: `CodeWalker.Core/GameFiles/FileTypes/YtypFile.cs`

| Method | Purpose | Parameters |
|--------|---------|------------|
| `AddArchetype(Archetype arch)` | Add new archetype | Archetype to add |
| `RemoveArchetype(Archetype arch)` | Remove archetype | Archetype to remove |
| `GetArchetype(uint hash)` | Find by hash | Name hash |

### YdrFile - Drawable Models
**Location**: `CodeWalker.Core/GameFiles/FileTypes/YdrFile.cs`

| Method | Purpose | Parameters |
|--------|---------|------------|
| `Load(byte[] data, RpfFileEntry entry)` | Load from data | Binary data |
| `Save()` | Save to bytes | Returns byte[] |
| `GetDrawable()` | Get drawable object | Returns `Drawable` |

### YbnFile - Collision Bounds
**Location**: `CodeWalker.Core/GameFiles/FileTypes/YbnFile.cs`

| Method | Purpose | Parameters |
|--------|---------|------------|
| `Load(byte[] data, RpfFileEntry entry)` | Load collision data | Binary data |
| `GetBounds()` | Get bounds object | Returns `Bounds` |
| `AddBounds(Bounds b)` | Add collision bounds | Bounds to add |

---

## Rendering Functions

### Renderer - Main Rendering Engine
**Location**: `CodeWalker/Rendering/Renderer.cs`

| Method | Purpose | Parameters |
|--------|---------|------------|
| `Init(DXManager dxman, ShaderManager shaders)` | Initialize renderer | DirectX managers |
| `Update(float elapsed, MouseState mouse, KeyboardState keyboard)` | Update frame | Input states |
| `BeginRender(DeviceContext context)` | Start rendering | DX context |
| `RenderSkyAndClouds()` | Render skybox | None |
| `RenderWorld(YmapFile[] ymaps)` | Render world geometry | Visible YMAPs |
| `RenderModel(DrawableBase drawable)` | Render single model | Model to render |
| `RenderBounds(Bounds bounds)` | Render collision | Collision bounds |
| `RenderSelectionEntityPivot(YmapEntityDef ent)` | Render gizmo | Selected entity |

### Camera - View Control
**Location**: `CodeWalker/Rendering/Camera.cs`

| Method | Purpose | Parameters |
|--------|---------|------------|
| `SetMousePosition(int x, int y)` | Update mouse | Screen coordinates |
| `MouseRotate(int dx, int dy)` | Rotate view | Mouse delta |
| `MouseZoom(int dz)` | Zoom in/out | Scroll delta |
| `SetFollowEntity(Entity e)` | Follow entity | Target entity |
| `GetMouseRay(Vector2 mouse)` | Screen to world ray | Mouse position |

---

## World Management

### WorldForm - Main Application
**Location**: `CodeWalker/Forms/WorldForm.cs`

| Method | Purpose | Parameters |
|--------|---------|------------|
| `SelectEntity(YmapEntityDef entity)` | Select entity | Entity to select |
| `GoToEntity(YmapEntityDef entity)` | Focus on entity | Target entity |
| `LoadYmaps()` | Load visible maps | None |
| `UpdateGraphics(float elapsed)` | Update frame | Delta time |
| `GetCameraRay()` | Get view ray | Returns `Ray` |
| `GetMouseHit()` | Ray cast test | Returns hit info |
| `BeginInvoke(Action action)` | Thread-safe UI update | Action to run |

### Space - World Space Management
**Location**: `CodeWalker/World/Space.cs`

| Method | Purpose | Parameters |
|--------|---------|------------|
| `Init(GameFileCache cache)` | Initialize world | File cache |
| `GetVisibleYmaps(Camera cam, List<YmapFile> ymaps)` | Get visible maps | Camera, output list |
| `GetYmaps(BoundingBox box)` | Spatial query | Bounding box |
| `AddYmap(YmapFile ymap)` | Add to world | YMAP to add |

---

## UI and Tools

### ProjectForm - Project Management
**Location**: `CodeWalker/Project/ProjectForm.cs`

| Method | Purpose | Parameters |
|--------|---------|------------|
| `NewProject()` | Create project | None |
| `OpenProject(string file)` | Load project | Project path |
| `SaveProject()` | Save current | None |
| `NewYmap()` | Create YMAP | None |
| `ImportYmapToProject(YmapFile ymap)` | Import YMAP | YMAP to import |

### BrowseForm - RPF Explorer
**Location**: `CodeWalker/Forms/BrowseForm.cs`

| Method | Purpose | Parameters |
|--------|---------|------------|
| `RefreshMainTreeView()` | Refresh tree | None |
| `SelectFile(RpfFileEntry file)` | Select in tree | File to select |
| `ExtractFile(RpfFileEntry entry)` | Extract to disk | File entry |
| `ImportFile()` | Import to RPF | None |

### ModelForm - Model Viewer
**Location**: `CodeWalker/Forms/ModelForm.cs`

| Method | Purpose | Parameters |
|--------|---------|------------|
| `LoadModel(YdrFile ydr)` | Load YDR model | Model file |
| `LoadModels(YddFile ydd)` | Load dictionary | Dictionary file |
| `LoadVehicle(YftFile yft)` | Load vehicle | Vehicle file |
| `LoadPed(YftFile yft)` | Load ped model | Ped file |

---

## Utility Functions

### JenkHash - String Hashing
**Location**: `CodeWalker.Core/GameFiles/Utils/JenkHash.cs`

| Method | Purpose | Usage |
|--------|---------|-------|
| `GenHash(string text)` | Generate hash | `uint hash = JenkHash.GenHash("prop_bench_01a");` |
| `GetString(uint hash)` | Reverse lookup | `string name = JenkHash.GetString(0x12345678);` |
| `TryGetString(uint hash)` | Safe lookup | `if (JenkHash.TryGetString(hash, out string name))` |

### GTACrypto - Encryption/Decryption
**Location**: `CodeWalker.Core/GameFiles/Utils/GTACrypto.cs`

| Method | Purpose | Parameters |
|--------|---------|------------|
| `DecryptNG(byte[] data, string name, uint length)` | Decrypt NG | Encrypted data |
| `EncryptNG(byte[] data, string name, uint length)` | Encrypt NG | Plain data |
| `DecryptAES(byte[] data)` | Decrypt AES | Encrypted data |
| `EncryptAES(byte[] data)` | Encrypt AES | Plain data |

### Vectors - Math Extensions
**Location**: `CodeWalker.Core/Utils/Vectors.cs`

| Method | Purpose | Usage |
|--------|---------|-------|
| `Vector3.Round(int decimals)` | Round vector | `pos = pos.Round(2);` |
| `Quaternion.ToEuler()` | Convert to Euler | `var euler = quat.ToEuler();` |
| `BoundingBox.Transform(Matrix m)` | Transform bounds | `bb = bb.Transform(matrix);` |

---

## Resource Management

### ResourceBuilder - Resource Compilation
**Location**: `CodeWalker.Core/GameFiles/Resources/ResourceBuilder.cs`

| Method | Purpose | Parameters |
|--------|---------|------------|
| `Build(IResourceBlock block, int version)` | Build resource | Root block, version |
| `AddBlock(IResourceBlock block)` | Add to build | Block to add |
| `GetData()` | Get built data | Returns byte[] |

### ResourceDataReader - Resource Reading
**Location**: `CodeWalker.Core/GameFiles/Resources/ResourceDataReader.cs`

| Method | Purpose | Usage |
|--------|---------|-------|
| `ReadBlock<T>()` | Read typed block | `var block = reader.ReadBlock<DrawableBase>();` |
| `ReadArray<T>(uint count)` | Read array | `var items = reader.ReadArray<Vertex>(vertexCount);` |
| `ReadString()` | Read string | `string text = reader.ReadString();` |

---

## Data Conversion

### MetaXml - META/XML Conversion
**Location**: `CodeWalker.Core/GameFiles/MetaTypes/MetaXml.cs`

| Method | Purpose | Usage |
|--------|---------|-------|
| `GetXml(MetaFile meta)` | META to XML | `string xml = MetaXml.GetXml(metaFile);` |
| `GetMeta(XmlDocument doc)` | XML to META | `var meta = MetaXml.GetMeta(xmlDoc);` |

### YmapXml - YMAP/XML Conversion
**Location**: `CodeWalker.Core/GameFiles/FileTypes/YmapFile.cs`

| Method | Purpose | Usage |
|--------|---------|-------|
| `YmapXml.GetXml(YmapFile ymap)` | YMAP to XML | `string xml = YmapXml.GetXml(ymap);` |
| `YmapXml.GetYmap(XmlDocument doc)` | XML to YMAP | `var ymap = YmapXml.GetYmap(xmlDoc);` |

### YtypXml - YTYP/XML Conversion
**Location**: `CodeWalker.Core/GameFiles/FileTypes/YtypFile.cs`

| Method | Purpose | Usage |
|--------|---------|-------|
| `YtypXml.GetXml(YtypFile ytyp)` | YTYP to XML | `string xml = YtypXml.GetXml(ytyp);` |
| `YtypXml.GetYtyp(XmlDocument doc)` | XML to YTYP | `var ytyp = YtypXml.GetYtyp(xmlDoc);` |

---

## Common Usage Examples

### Loading and Modifying a YMAP
```csharp
// Load YMAP
var ymap = gameFileCache.GetYmap("vespucci_01_0.ymap");

// Create new entity
var entity = ymap.CreateEntity();
entity.SetPosition(new Vector3(100, 200, 50));
entity.SetArchetype(archetype);

// Save changes
var data = ymap.Save();
File.WriteAllBytes("modified.ymap", data);
```

### Extracting Files from RPF
```csharp
// Get file entry
var entry = RpfMan.GetFile("x64a.rpf\\models\\cdimages\\weapons.rpf\\w_ar_assaultrifle.ydr");

// Extract data
var data = entry.File.ExtractFile(entry);

// Save to disk
File.WriteAllBytes("weapon.ydr", data);
```

### Ray Casting for Selection
```csharp
// Get mouse ray
var ray = camera.GetMouseRay(mousePosition);

// Test against entities
foreach (var entity in visibleEntities)
{
    if (ray.Intersects(entity.BoundingBox))
    {
        SelectEntity(entity);
        break;
    }
}
```

### Converting Files to XML
```csharp
// Load file
var ymap = gameFileCache.GetYmap("example.ymap");

// Convert to XML
string xml = YmapXml.GetXml(ymap);

// Save XML
File.WriteAllText("example.ymap.xml", xml);
```

---

## Tips for Finding Functions

1. **By File Type**: Look for classes named after the file extension (YmapFile, YtypFile, etc.)
2. **By Operation**: 
   - Loading: Check GameFileCache
   - Rendering: Check Renderer class
   - UI: Check form classes (WorldForm, ModelForm, etc.)
3. **By Feature**:
   - Selection: Search for "Select" methods
   - Import/Export: Search for "Import", "Export", "Extract"
   - Conversion: Look for "Xml" suffix classes
4. **Common Patterns**:
   - Get* methods: Retrieve data
   - Set* methods: Modify data
   - Create* methods: Create new objects
   - Update* methods: Refresh state

## See Also

- [API Reference](api_reference.md) - Detailed API documentation
- [Architecture Overview](architecture_overview.md) - System design
- [File Types Documentation](file_types_documentation.md) - File format details