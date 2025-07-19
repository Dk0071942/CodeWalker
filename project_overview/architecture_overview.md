# CodeWalker Architecture Overview

## System Architecture

```mermaid
graph TB
    subgraph "User Interface Layer"
        MainApp[CodeWalker Main Application]
        RPFExp[RPF Explorer]
        VehEdit[Vehicle Editor]
        PedEdit[Ped Editor]
        ModMgr[Mod Manager]
        DLCMrg[DLC Merger]
    end
    
    subgraph "Application Layer"
        Forms[Forms System]
        Render[Rendering Engine]
        Project[Project System]
        World[World System]
        Tools[Tools System]
    end
    
    subgraph "Core Library Layer"
        GameFiles[Game Files System]
        Resources[Resources System]
        MetaTypes[Meta Types System]
        Utils[Utilities]
    end
    
    subgraph "Platform Layer"
        WinForms[WinForms Controls]
        DX11[DirectX 11]
        Shaders[HLSL Shaders]
        FileIO[File I/O]
    end
    
    MainApp --> Forms
    MainApp --> Render
    MainApp --> Project
    MainApp --> World
    MainApp --> Tools
    
    RPFExp --> GameFiles
    VehEdit --> GameFiles
    PedEdit --> GameFiles
    ModMgr --> GameFiles
    DLCMrg --> GameFiles
    
    Forms --> WinForms
    Render --> DX11
    Render --> Shaders
    
    Project --> GameFiles
    World --> GameFiles
    Tools --> GameFiles
    
    GameFiles --> Resources
    GameFiles --> MetaTypes
    GameFiles --> Utils
    GameFiles --> FileIO
```

## Component Interactions

```mermaid
sequenceDiagram
    participant User
    participant UI as UI Layer
    participant Core as Core Library
    participant Files as File System
    participant GPU as GPU/DirectX
    
    User->>UI: Open RPF Archive
    UI->>Core: Load RPF
    Core->>Files: Read RPF Data
    Files-->>Core: Raw Data
    Core->>Core: Parse File Structure
    Core-->>UI: File Entries
    
    User->>UI: View Model File
    UI->>Core: Load YDR/YDD
    Core->>Files: Read Model Data
    Files-->>Core: Model Bytes
    Core->>Core: Parse Model Structure
    Core->>GPU: Create DirectX Resources
    GPU-->>UI: Render Model
```

## Data Flow Architecture

```mermaid
flowchart LR
    subgraph "Input Sources"
        RPF[RPF Archives]
        XML[XML Files]
        Models[Model Files]
        Textures[Texture Files]
    end
    
    subgraph "Processing Pipeline"
        Parser[File Parser]
        Builder[File Builder]
        Converter[Format Converter]
        Validator[Data Validator]
    end
    
    subgraph "Output Targets"
        Memory[In-Memory Cache]
        Display[3D Viewport]
        Export[Export Files]
        Project[Project Files]
    end
    
    RPF --> Parser
    XML --> Parser
    Models --> Parser
    Textures --> Parser
    
    Parser --> Validator
    Validator --> Memory
    
    Memory --> Builder
    Memory --> Converter
    Memory --> Display
    
    Builder --> Export
    Converter --> Export
    Builder --> Project
```

## Module Dependencies

```mermaid
graph BT
    Core[CodeWalker.Core]
    WinForms[CodeWalker.WinForms]
    Shaders[CodeWalker.Shaders]
    
    Main[CodeWalker]
    RPF[RPFExplorer]
    Vehicles[Vehicles]
    Peds[Peds]
    ModManager[ModManager]
    DLCMerger[DLCMerger]
    Gen9[Gen9Converter]
    Frag[FragConversion]
    Error[ErrorReport]
    
    Main --> Core
    Main --> WinForms
    Main --> Shaders
    
    RPF --> Core
    RPF --> WinForms
    
    Vehicles --> Core
    Vehicles --> WinForms
    
    Peds --> Core
    Peds --> WinForms
    
    ModManager --> Core
    
    DLCMerger --> Core
    
    Gen9 --> Core
    
    Frag --> Core
    
    Error --> Core
```

## Key Design Patterns

### 1. **Resource Management Pattern**
- Base classes for all game resources
- Lazy loading and caching
- Reference counting for memory management

### 2. **File Type Registry**
- Factory pattern for file type handlers
- Extension-based routing
- Pluggable file format support

### 3. **Rendering Pipeline**
- Command pattern for render operations
- State management for DirectX
- Shader resource binding

### 4. **Project System**
- Observer pattern for project changes
- Command pattern for undo/redo
- Visitor pattern for project traversal

### 5. **Data Conversion**
- Strategy pattern for format converters
- Builder pattern for file construction
- Chain of responsibility for validation

## Technology Stack Details

### Core Technologies
- **Language**: C# 12.0 / .NET Framework 4.8 & .NET 8.0
- **UI Framework**: Windows Forms
- **Graphics API**: DirectX 11
- **Shader Language**: HLSL 5.0
- **Build System**: MSBuild / Visual Studio 2022

### Key Libraries
- **System.Numerics**: Vector/Matrix math
- **System.Drawing**: Image processing
- **System.IO.Compression**: Archive handling
- **DirectX Tool Kit**: DirectX helpers

### File Format Support
- **Archives**: RPF (RAGE Package Format)
- **Models**: YDR, YDD, YFT (Drawable/Fragment)
- **Textures**: YTD (Texture Dictionary), DDS
- **Maps**: YMAP, YTYP, YBN, YND, YNV
- **Audio**: AWC, REL
- **Data**: META, PSO, XML, GXT2