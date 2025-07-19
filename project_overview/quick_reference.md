# CodeWalker Quick Reference Guide

## Overview

This guide provides quick access to the most commonly used operations in CodeWalker. Each section includes the relevant function calls and file locations.

## Table of Contents

1. [File Operations](#file-operations)
2. [Entity Manipulation](#entity-manipulation)
3. [Model Loading](#model-loading)
4. [Rendering Operations](#rendering-operations)
5. [Project Management](#project-management)
6. [Data Conversion](#data-conversion)
7. [Common Patterns](#common-patterns)

---

## File Operations

### Loading Game Files

```csharp
// Location: CodeWalker.Core/GameFiles/GameFileCache.cs

// Load specific file types
YmapFile ymap = gameFileCache.GetYmap("vespucci_01_0.ymap");
YtypFile ytyp = gameFileCache.GetYtyp("v_int_1.ytyp");
YdrFile ydr = gameFileCache.GetYdr("prop_bench_01a.ydr");
YtdFile ytd = gameFileCache.GetYtd("prop_textures.ytd");
YbnFile ybn = gameFileCache.GetYbn("vespucci_01_0.ybn");

// Generic loading
T file = gameFileCache.GetFile<T>("filename");
```

### Finding Files in RPFs

```csharp
// Location: CodeWalker.Core/GameFiles/RpfManager.cs

// Get specific file
RpfFileEntry entry = RpfMan.GetFile("x64a.rpf\\models\\cdimages\\weapons.rpf\\w_ar_assaultrifle.ydr");

// Find all files of type
List<RpfFileEntry> allYmaps = RpfMan.GetAllFiles(".ymap");

// Extract file data
byte[] data = entry.File.ExtractFile(entry);
```

### Saving Files

```csharp
// Save modified game file
byte[] data = gameFile.Save();
File.WriteAllBytes("output.ymap", data);

// Create new RPF entry
RpfMan.CreateFileEntry("newfile.ydr", "path/in/rpf", ref data);
```

---

## Entity Manipulation

### Creating Entities

```csharp
// Location: CodeWalker.Core/GameFiles/FileTypes/YmapFile.cs

// Create new entity
YmapEntityDef entity = ymap.CreateEntity();
entity.SetPosition(new Vector3(100, 200, 50));
entity.SetOrientation(Quaternion.Identity);
entity.SetScale(Vector3.One);

// Set archetype
Archetype arch = ytyp.GetArchetype(JenkHash.GenHash("prop_bench_01a"));
entity.SetArchetype(arch);
```

### Modifying Entities

```csharp
// Location: CodeWalker.Core/GameFiles/FileTypes/YmapFile.cs

// Transform entity
entity.SetPosition(new Vector3(x, y, z));
entity.SetOrientation(Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll));
entity.SetScale(new Vector3(sx, sy, sz));

// Update properties
entity.flags = 1572872;
entity.lodDist = 200.0f;
entity.UpdateBB(); // Update bounding box
```

### Finding Entities

```csharp
// By hash
YmapEntityDef entity = ymap.GetEntity(JenkHash.GenHash("entity_name"));

// By spatial query
YmapEntityDef[] entities = ymap.GetEntities(boundingBox);

// Get all root entities
YmapEntityDef[] roots = ymap.GetRootEntities();
```

---

## Model Loading

### Loading Models

```csharp
// Location: CodeWalker/Forms/ModelForm.cs

// Load single model
YdrFile ydr = gameFileCache.GetYdr("model.ydr");
modelForm.LoadModel(ydr);

// Load model dictionary
YddFile ydd = gameFileCache.GetYdd("models.ydd");
modelForm.LoadModels(ydd);

// Load vehicle
YftFile yft = gameFileCache.GetYft("vehicle.yft");
modelForm.LoadVehicle(yft);
```

### Accessing Model Data

```csharp
// Get drawable from YDR
Drawable drawable = ydr.Drawable;
DrawableModel[] models = drawable.DrawableModels?.data_items;

// Get bounds
Bounds bounds = ybn.Bounds;
BoundComposite composite = bounds as BoundComposite;
```

---

## Rendering Operations

### Camera Control

```csharp
// Location: CodeWalker/Rendering/Camera.cs

// Set camera position
camera.Position = new Vector3(x, y, z);
camera.SetLookAt(targetPosition);

// Mouse control
camera.MouseRotate(deltaX, deltaY);
camera.MouseZoom(scrollDelta);

// Get view ray
Ray mouseRay = camera.GetMouseRay(mousePosition);
```

### Rendering Objects

```csharp
// Location: CodeWalker/Rendering/Renderer.cs

// Render model
renderer.RenderModel(drawable);

// Render collision bounds
renderer.RenderBounds(bounds);

// Render selection
renderer.RenderSelectionEntityPivot(selectedEntity);
```

### Selection and Ray Casting

```csharp
// Location: CodeWalker/Forms/WorldForm.cs

// Get mouse hit
MouseRayHit hit = GetMouseHit();
if (hit.Hit)
{
    SelectEntity(hit.Entity);
}

// Manual ray test
Ray ray = GetCameraRay();
foreach (var entity in visibleEntities)
{
    if (ray.Intersects(entity.BoundingBox))
    {
        // Hit detected
    }
}
```

---

## Project Management

### Project Operations

```csharp
// Location: CodeWalker/Project/ProjectForm.cs

// Create new project
projectForm.NewProject();

// Open existing project
projectForm.OpenProject("myproject.cwproj");

// Save project
projectForm.SaveProject();
```

### Adding Files to Project

```csharp
// Import YMAP
projectForm.ImportYmapToProject(ymap);

// Create new YMAP
projectForm.NewYmap();

// Add entity to project
projectForm.AddEntityToProject(entity);
```

---

## Data Conversion

### XML Conversion

```csharp
// Location: Various Xml classes

// YMAP to XML
string ymapXml = YmapXml.GetXml(ymap);
YmapFile ymap2 = YmapXml.GetYmap(XmlDocument.Parse(ymapXml));

// YTYP to XML
string ytypXml = YtypXml.GetXml(ytyp);
YtypFile ytyp2 = YtypXml.GetYtyp(XmlDocument.Parse(ytypXml));

// META to XML
string metaXml = MetaXml.GetXml(metaFile);
MetaFile meta2 = MetaXml.GetMeta(XmlDocument.Parse(metaXml));
```

### Hash Operations

```csharp
// Location: CodeWalker.Core/GameFiles/Utils/JenkHash.cs

// Generate hash
uint hash = JenkHash.GenHash("prop_bench_01a");

// Reverse lookup
string name = JenkHash.GetString(hash);

// Safe lookup
if (JenkHash.TryGetString(hash, out string name))
{
    // Use name
}
```

---

## Common Patterns

### Thread-Safe UI Updates

```csharp
// Update UI from background thread
BeginInvoke(new Action(() =>
{
    statusLabel.Text = "Loading...";
    progressBar.Value = 50;
}));
```

### Resource Management

```csharp
// Using statements for disposal
using (var stream = File.OpenRead(path))
using (var reader = new ResourceDataReader(stream))
{
    // Read data
}

// Manual disposal
gameFile?.Dispose();
gameFile = null;
```

### Error Handling

```csharp
try
{
    var file = gameFileCache.GetYdr(path);
    // Process file
}
catch (Exception ex)
{
    MessageBox.Show($"Error loading {path}: {ex.Message}");
    // Log error
}
```

### Caching Pattern

```csharp
// Check cache first
if (cache.TryGetValue(key, out var cachedValue))
{
    return cachedValue;
}

// Load and cache
var value = LoadExpensiveResource();
cache[key] = value;
return value;
```

---

## Quick Function Finder

### By Task

| Task | Function | Location |
|------|----------|----------|
| Load YMAP | `GetYmap()` | GameFileCache |
| Create entity | `CreateEntity()` | YmapFile |
| Select entity | `SelectEntity()` | WorldForm |
| Extract file | `ExtractFile()` | RpfFile |
| Convert to XML | `GetXml()` | Various Xml classes |
| Generate hash | `GenHash()` | JenkHash |
| Render model | `RenderModel()` | Renderer |
| Save file | `Save()` | GameFile classes |

### By File Type

| File Type | Load Function | Save Function | XML Convert |
|-----------|---------------|---------------|-------------|
| YMAP | `GetYmap()` | `Save()` | `YmapXml` |
| YTYP | `GetYtyp()` | `Save()` | `YtypXml` |
| YDR | `GetYdr()` | `Save()` | - |
| YTD | `GetYtd()` | `Save()` | - |
| YBN | `GetYbn()` | `Save()` | `YbnXml` |
| META | `GetMeta()` | `Save()` | `MetaXml` |

### By Operation Type

| Operation | Classes | Key Methods |
|-----------|---------|-------------|
| File I/O | GameFileCache, RpfManager | Get*, Save |
| Rendering | Renderer, Camera | Render*, Update |
| UI | WorldForm, ModelForm | Select*, Load* |
| Conversion | *Xml classes | GetXml, Get* |
| Utilities | JenkHash, GTACrypto | GenHash, Decrypt* |

---

## See Also

- [Function Index](function_index.md) - Complete function reference
- [API Reference](api_reference.md) - Detailed API documentation
- [Architecture Overview](architecture_overview.md) - System design