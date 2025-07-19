# CodeWalker Project Documentation

## Overview

CodeWalker is a comprehensive modding tool and file viewer for Grand Theft Auto V. It provides a suite of applications for exploring, viewing, and editing the various file formats used by the RAGE engine. This documentation provides a detailed technical overview of the entire CodeWalker ecosystem.

## Project Structure

The CodeWalker solution consists of 12 projects organized into several categories:

### Core Infrastructure
- **CodeWalker.Core** - Core library handling all game file operations
- **CodeWalker.WinForms** - Shared Windows Forms controls and UI components
- **CodeWalker.Shaders** - DirectX 11 shader implementation (C++)

### Main Applications
- **CodeWalker** - Primary application with world viewer and editing tools
- **CodeWalker.RPFExplorer** - Standalone RPF archive explorer
- **CodeWalker.Vehicles** - Specialized vehicle model viewer
- **CodeWalker.Peds** - Specialized pedestrian model viewer

### Utility Tools
- **CodeWalker.DLCMerger** - Tool for merging DLC packages
- **CodeWalker.ModManager** - Mod management system
- **CodeWalker.Gen9Converter** - Gen9 shader conversion utility
- **CodeWalker.FragConversion** - Fragment file conversion tool
- **CodeWalker.ErrorReport** - Error reporting utility

## Documentation Contents

### Quick References
- **[Function Index](function_index.md)** - Complete searchable index of all functions and methods
- **[Quick Reference](quick_reference.md)** - Common operations and code examples
- **[Data Structures Reference](data_structures_reference.md)** - Core data types and relationships

### Core Documentation
### 1. [Architecture Overview](architecture_overview.md)
Comprehensive overview of the system architecture, component interactions, and design patterns used throughout CodeWalker.

### 2. [Core Library Documentation](core_library_documentation.md)
Detailed documentation of the CodeWalker.Core library, including:
- GameFile system architecture
- Resource management
- Caching mechanisms
- File loading and saving

### 3. [File Types Documentation](file_types_documentation.md)
Complete reference for all supported GTA V file types:
- Map/World files (YMAP, YTYP, YBN, YND, YNV)
- Model/Graphics files (YDR, YDD, YFT, YTD)
- Audio files (AWC, REL)
- Data files (META, PSO, XML, GXT2)

### Application Documentation
### 4. [Main Application Documentation](main_application_documentation.md)
Documentation for the primary CodeWalker application:
- Forms and UI architecture
- Rendering system (DirectX 11)
- Project management
- World space system
- Tools and editors

### 5. [DLC Merger Documentation](dlcmerger_documentation.md)
Technical details of the DLC merging tool:
- Model extraction process
- XML merging algorithms
- Manifest generation
- Selective merging strategies

### 6. [RPF Explorer Documentation](rpfexplorer_documentation.md)
Documentation for the standalone RPF archive explorer:
- Purpose and design philosophy
- Feature comparison with integrated explorer
- Performance characteristics

### 7. [Vehicle and Ped Viewers Documentation](vehicles_peds_documentation.md)
Documentation for specialized model viewers:
- Vehicle viewer features
- Ped viewer component system
- Animation playback
- Metadata integration

### Technical Deep Dives
### 8. [RPF Extraction System Documentation](rpf_extraction_documentation.md)
Detailed documentation of the RPF extraction algorithms:
- Core extraction pipeline and architecture
- Encryption and compression handling
- API usage examples and code samples
- Integration with UI and common use cases

### 9. [YFT Compression and Conversion Documentation](yft_compression_documentation.md)
Comprehensive guide to YFT file compression and format conversion:
- RCS7 header format and structure
- DEFLATE compression/decompression process
- XML conversion capabilities (bidirectional)
- Complete API examples for all conversion scenarios

## Key Technologies

### Programming Languages
- **C#** (.NET Framework 4.8 / .NET 8.0) - Primary language
- **C++** - DirectX shader implementation
- **HLSL** - Shader programming

### Frameworks and Libraries
- **Windows Forms** - UI framework
- **SharpDX** - DirectX 11 wrapper for C#
- **WeifenLuo.WinFormsUI.Docking** - Docking panel system

### Graphics and Rendering
- **DirectX 11** - 3D rendering
- **HLSL 5.0** - Shader model
- **DirectX Tool Kit** - Helper utilities

## Getting Started

### For Users
1. Download the latest release from the repository
2. Extract to a folder on your system
3. Run `CodeWalker.exe`
4. Configure the GTA V game folder in settings
5. Start exploring and modding!

### For Developers

#### Prerequisites
- Visual Studio 2022 or later
- .NET Framework 4.8 SDK
- .NET 8.0 SDK (for DLCMerger)
- Windows 10 or later
- DirectX 11 compatible graphics card

#### Building the Project
1. Clone the repository
2. Open `CodeWalker.sln` in Visual Studio
3. Restore NuGet packages
4. Build the solution

#### Finding Functions Quickly
- **[Function Index](function_index.md)** - Comprehensive function catalog with search capability
- **[Quick Reference](quick_reference.md)** - Common operations and code patterns
- **[Data Structures Reference](data_structures_reference.md)** - Core data types and relationships

#### Development Workflow
1. **Understand the Architecture**: Start with [Architecture Overview](architecture_overview.md)
2. **Find Existing Functions**: Use [Function Index](function_index.md) to locate existing functionality
3. **Learn Data Structures**: Reference [Data Structures Reference](data_structures_reference.md)
4. **Follow Patterns**: Use [Quick Reference](quick_reference.md) for common operations
5. **Extend and Modify**: Follow existing patterns in the codebase

## File Format Support

CodeWalker supports extensive GTA V file formats:

### Archives
- **RPF** - RAGE Package Format (game archives)

### Models and Graphics
- **YDR** - Drawable models
- **YDD** - Drawable dictionaries
- **YFT** - Fragment models
- **YTD** - Texture dictionaries

### World and Map
- **YMAP** - Map placements
- **YTYP** - Archetype definitions
- **YBN** - Collision bounds
- **YND** - Path nodes
- **YNV** - Navigation meshes

### Data and Configuration
- **META** - Metadata files
- **PSO** - Platform-specific objects
- **XML** - Configuration files
- **GXT2** - Game text

### Audio
- **AWC** - Audio wave containers
- **REL** - Audio relationships

## Architecture Highlights

### Modular Design
- Clear separation of concerns
- Reusable components across applications
- Shared core infrastructure

### Performance Optimizations
- Efficient caching system
- Lazy loading strategies
- Memory management
- LOD (Level of Detail) support

### Extensibility
- Plugin-friendly architecture
- Support for custom file types
- Modular tool system

## Contributing

When contributing to CodeWalker:
1. Follow existing code conventions
2. Maintain compatibility with game formats
3. Include appropriate error handling
4. Update relevant documentation

## License

CodeWalker is provided under its specific license terms. See the LICENSE file in the repository for details.

## Additional Resources

- [GTA V Modding Community](https://www.gta5-mods.com/)
- [OpenIV](https://openiv.com/) - Alternative modding tool
- [RAGE Research](https://gtamods.com/wiki/RAGE_Formats) - File format documentation

## Documentation Progress

See [documentation_todo.md](documentation_todo.md) for the current documentation progress and remaining tasks.

### Recent Documentation Additions

The following new reference documents have been added to improve developer experience:

- **[Function Index](function_index.md)** - Complete searchable catalog of all functions and methods in CodeWalker. Organized by category with usage examples and file locations.
- **[Quick Reference](quick_reference.md)** - Quick lookup guide for common operations with code examples. Perfect for finding how to do specific tasks.
- **[Data Structures Reference](data_structures_reference.md)** - Comprehensive reference for all core data types, classes, and their relationships. Includes memory layouts and usage patterns.

These additions make it much easier to:
- **Find existing functions** instead of reimplementing functionality
- **Understand data relationships** between different file types
- **Follow established patterns** when extending the codebase
- **Quickly implement common operations** with copy-paste examples