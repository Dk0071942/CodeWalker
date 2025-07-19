# CodeWalker API Reference

## Overview

This document provides a comprehensive API reference for developers extending or integrating with CodeWalker. The APIs are organized by namespace and functionality.

## Core APIs

### CodeWalker.GameFiles

#### GameFile Class
Base class for all game file types.

```csharp
public abstract class GameFile : Cacheable<GameFileCacheKey>
{
    // Properties
    public GameFileType Type { get; set; }
    public RpfFileEntry RpfFileEntry { get; set; }
    public string Name { get; set; }
    public string FilePath { get; set; }
    public bool Loaded { get; set; }
    public DateTime LastLoadTime { get; }
    public long MemoryUsage { get; }
    
    // Methods
    public virtual void Load(byte[] data, RpfFileEntry entry) { }
    public virtual byte[] Save() { return null; }
}
```

#### GameFileCache Class
Central cache for managing game files.

```csharp
public class GameFileCache
{
    // Initialization
    public void Init(Action<string> updateStatus, Action<string> errorLog);
    
    // File Loading
    public T GetFile<T>(string path) where T : GameFile, new();
    public YmapFile GetYmap(string path);
    public YtypFile GetYtyp(string path);
    public YdrFile GetYdr(string path);
    public YddFile GetYdd(string path);
    public YtdFile GetYtd(string path);
    public YbnFile GetYbn(string path);
    public YndFile GetYnd(string path);
    public YnvFile GetYnv(string path);
    
    // RPF Management
    public RpfFile GetRpf(string path);
    public RpfEntry GetEntry(string path);
    
    // Cache Control
    public void Clear();
    public void Invalidate(string path);
    public long GetMemoryUsage();
}
```

### CodeWalker.World

#### Space Class
Manages the 3D world space.

```csharp
public class Space
{
    // Properties
    public MapDataStoreNode RootMapNode { get; }
    public bool Inited { get; }
    public float CurrentHour { get; set; }
    public Weather CurrentWeather { get; set; }
    
    // Methods
    public void Init(GameFileCache cache);
    public void Update(float elapsed);
    public void GetVisibleYmaps(Camera cam, List<YmapFile> ymaps);
    public YmapEntityDef GetEntity(uint entityHash);
    public void SelectEntity(YmapEntityDef entity);
}
```

### CodeWalker.Rendering

#### Renderer Class
Main rendering engine.

```csharp
public class Renderer
{
    // Initialization
    public void Init(DXManager dxman, ShaderManager shaders);
    
    // Rendering
    public void Render(DeviceContext context, float elapsed);
    public void RenderWorld(YmapFile[] ymaps);
    public void RenderModel(DrawableBase drawable);
    public void RenderBounds(Bounds bounds);
    
    // Camera
    public Camera Camera { get; set; }
    
    // Settings
    public float LODMultiplier { get; set; }
    public bool ShowCollisionMeshes { get; set; }
    public bool ShowWireframe { get; set; }
}
```

#### Camera Class
3D camera control.

```csharp
public class Camera
{
    // Properties
    public Vector3 Position { get; set; }
    public Vector3 Rotation { get; set; }
    public float FieldOfView { get; set; }
    public float AspectRatio { get; set; }
    public float NearClip { get; set; }
    public float FarClip { get; set; }
    
    // Matrices
    public Matrix ViewMatrix { get; }
    public Matrix ProjectionMatrix { get; }
    public Matrix ViewProjectionMatrix { get; }
    
    // Methods
    public void Update(float elapsed);
    public void SetLookAt(Vector3 target);
    public Ray GetMouseRay(Vector2 mousePos);
}
```

## File Format APIs

### Resource Files

#### YdrFile (Drawable)
```csharp
public class YdrFile : GameFile, PackedFile
{
    public Drawable Drawable { get; set; }
    
    public void Load(byte[] data, RpfFileEntry entry);
    public byte[] Save();
}
```

#### YmapFile (Map Placement)
```csharp
public class YmapFile : GameFile, PackedFile
{
    public CMapData CMapData { get; set; }
    public YmapEntityDef[] AllEntities { get; set; }
    public YmapCarGen[] CarGenerators { get; set; }
    
    // Entity Management
    public YmapEntityDef CreateEntity();
    public void RemoveEntity(YmapEntityDef entity);
    public void UpdateEntity(YmapEntityDef entity);
    
    // Spatial Queries
    public YmapEntityDef[] GetEntities(BoundingBox box);
    public YmapEntityDef GetEntity(uint hash);
}
```

#### YtypFile (Archetypes)
```csharp
public class YtypFile : GameFile, PackedFile
{
    public CMapTypes CMapTypes { get; set; }
    public Archetype[] AllArchetypes { get; set; }
    
    // Archetype Management
    public Archetype CreateArchetype(CArchetypeDef def);
    public void RemoveArchetype(Archetype arch);
    public Archetype FindArchetype(uint hash);
}
```

### Resource System

#### ResourceFileBase
```csharp
public abstract class ResourceFileBase : IResourceBlock
{
    public uint FileVFT { get; set; }
    public uint FileUnknown { get; set; }
    public uint FileSize { get; set; }
    
    public abstract void Read(ResourceDataReader reader, params object[] parameters);
    public abstract void Write(ResourceDataWriter writer, params object[] parameters);
}
```

#### IResourceBlock Interface
```csharp
public interface IResourceBlock
{
    long FilePosition { get; set; }
    uint BlockLength { get; }
    
    void Read(ResourceDataReader reader, params object[] parameters);
    void Write(ResourceDataWriter writer, params object[] parameters);
}
```

## Extension Points

### Custom File Types

To add support for a new file type:

```csharp
// 1. Define the file type enum
public enum GameFileType
{
    // ... existing types ...
    MyCustomType = 100
}

// 2. Create the file class
public class MyCustomFile : GameFile, PackedFile
{
    public MyCustomData Data { get; set; }
    
    public void Load(byte[] data, RpfFileEntry entry)
    {
        // Parse the data
        using (var ms = new MemoryStream(data))
        {
            // Load implementation
        }
        Loaded = true;
    }
    
    public byte[] Save()
    {
        // Serialize the data
        using (var ms = new MemoryStream())
        {
            // Save implementation
            return ms.ToArray();
        }
    }
}

// 3. Register in GameFileCache
public MyCustomFile GetMyCustomFile(string path)
{
    return GetFile<MyCustomFile>(path);
}
```

### Custom Resource Blocks

To create custom resource data:

```csharp
public class MyResourceBlock : ResourceSystemBlock
{
    // Resource pointers
    public ulong DataPointer { get; set; }
    public uint DataCount { get; set; }
    
    // Actual data
    public MyDataItem[] Data { get; set; }
    
    public override void Read(ResourceDataReader reader, params object[] parameters)
    {
        base.Read(reader, parameters);
        
        DataPointer = reader.ReadUInt64();
        DataCount = reader.ReadUInt32();
        
        Data = reader.ReadBlockAt<ResourceSimpleArray<MyDataItem>>(
            DataPointer, DataCount);
    }
    
    public override void Write(ResourceDataWriter writer, params object[] parameters)
    {
        base.Write(writer, parameters);
        
        DataPointer = writer.Position;
        writer.WriteBlock(Data);
        writer.Write(DataCount);
    }
}
```

### Custom Tools

To create a custom tool window:

```csharp
public partial class MyToolForm : Form
{
    private GameFileCache GameFileCache;
    private Renderer Renderer;
    
    public MyToolForm(GameFileCache cache)
    {
        InitializeComponent();
        GameFileCache = cache;
    }
    
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        // Initialize your tool
    }
    
    // Tool implementation
}

// Register in main menu
private void myToolMenuItem_Click(object sender, EventArgs e)
{
    var tool = new MyToolForm(GameFileCache);
    tool.Show(this);
}
```

## Utility APIs

### Math Extensions

```csharp
public static class VectorExtensions
{
    public static Vector3 Round(this Vector3 v, int decimals);
    public static float Length(this Vector3 v);
    public static Vector3 Normalized(this Vector3 v);
    public static Quaternion ToQuaternion(this Vector3 euler);
}
```

### Hash Functions

```csharp
public static class JenkHash
{
    public static uint GenHash(string text);
    public static uint GenHash(byte[] data);
    public static string GetString(uint hash);
    public static string TryGetString(uint hash);
}
```

### XML Conversion

```csharp
public static class XmlMeta
{
    public static string GetXml(MetaFile meta);
    public static MetaFile GetMeta(XmlDocument doc);
}

public static class YmapXml
{
    public static string GetXml(YmapFile ymap);
    public static YmapFile GetYmap(XmlDocument doc);
}
```

## Event System

### Selection Events

```csharp
public class SelectionManager
{
    public event EventHandler<SelectionChangedEventArgs> SelectionChanged;
    
    public void Select(object item);
    public void Deselect();
    public object SelectedItem { get; }
}

public class SelectionChangedEventArgs : EventArgs
{
    public object OldSelection { get; set; }
    public object NewSelection { get; set; }
}
```

### Progress Events

```csharp
public interface IProgressReporter
{
    event EventHandler<ProgressEventArgs> ProgressChanged;
    void ReportProgress(float progress, string status);
}

public class ProgressEventArgs : EventArgs
{
    public float Progress { get; set; } // 0.0 to 1.0
    public string Status { get; set; }
}
```

## Best Practices

### Memory Management
```csharp
// Use using statements for resources
using (var reader = new ResourceDataReader(stream))
{
    // Read data
}

// Clear references to large objects
largeObject = null;
GC.Collect();
```

### Error Handling
```csharp
try
{
    var file = GameFileCache.GetYdr(path);
    // Process file
}
catch (Exception ex)
{
    // Log error
    Logger.Error($"Failed to load {path}: {ex.Message}");
    // Handle gracefully
}
```

### Thread Safety
```csharp
// Use locks for shared resources
private readonly object cacheLock = new object();

public void UpdateCache(string key, object value)
{
    lock (cacheLock)
    {
        cache[key] = value;
    }
}
```

## Common Patterns

### Visitor Pattern for File Processing
```csharp
public interface IGameFileVisitor
{
    void Visit(YmapFile ymap);
    void Visit(YtypFile ytyp);
    void Visit(YdrFile ydr);
    // ... other file types
}

public abstract class GameFile
{
    public abstract void Accept(IGameFileVisitor visitor);
}
```

### Factory Pattern for File Creation
```csharp
public static class GameFileFactory
{
    public static GameFile Create(GameFileType type)
    {
        switch (type)
        {
            case GameFileType.Ymap: return new YmapFile();
            case GameFileType.Ytyp: return new YtypFile();
            case GameFileType.Ydr: return new YdrFile();
            // ... other types
            default: throw new NotSupportedException();
        }
    }
}
```

## Version Compatibility

### Checking Game Version
```csharp
public enum GameVersion
{
    Unknown = 0,
    v1_0_335_2,
    v1_0_350_1,
    // ... other versions
}

public static GameVersion GetGameVersion()
{
    // Check executable version
    var exePath = Path.Combine(GTAFolder, "GTA5.exe");
    var version = FileVersionInfo.GetVersionInfo(exePath);
    // Map to GameVersion enum
}
```

This API reference provides the foundation for extending CodeWalker. For specific implementation examples, refer to the existing tools and forms in the codebase.