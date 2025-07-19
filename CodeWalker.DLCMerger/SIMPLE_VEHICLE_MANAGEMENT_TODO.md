# Simple Vehicle Management System - Focused TODO List

## Overview

This is a **simplified, lightweight approach** focused on essential vehicle management features only. We're building a practical tool for managing vehicles in merged DLCs without unnecessary complexity.

## Core Philosophy: **Keep It Simple**

- ✅ CSV files instead of complex database
- ✅ CLI-focused interface  
- ✅ Direct file operations
- ✅ Only essential features
- ❌ No web UI
- ❌ No complex architecture
- ❌ No advanced analytics

---

## Phase 1: Vehicle Registry System (2 weeks)

### Goal: Simple vehicle inventory with CSV storage

#### 1.1 Create Vehicle Registry Structure 🔴

**Files to create:**
```
Data/
├── vehicles.csv          # Main vehicle inventory
├── carcol_registry.csv   # Carcol ID tracking
├── rpf_manifest.csv      # Which vehicles in which RPFs
└── config.json           # Simple configuration
```

**Tasks:**
- [ ] ⬜ **1.1.1** Create Data folder structure
  - Create `Data/` directory in output
  - Add `.gitignore` for temporary files
  - **Effort**: 15 minutes

- [ ] ⬜ **1.1.2** Design vehicles.csv format
  ```csv
  ModelName,DisplayName,Manufacturer,Class,SourceDLC,CarcolIDs,Tags,EstimatedSize,ImportedAt
  adder,Truffade Adder,Truffade,Super,dlc_cars,"500,501",fast;luxury,15728640,2024-01-15
  zentorno,Pegassi Zentorno,Pegassi,Super,dlc_cars,502,fast;racing,18421760,2024-01-15
  ```
  - **Effort**: 30 minutes

- [ ] ⬜ **1.1.3** Design carcol_registry.csv format
  ```csv
  ID,KitName,VehicleModel,IsUsed,AllocatedAt
  500,adder_modkit,adder,true,2024-01-15
  501,adder_race_kit,adder,true,2024-01-15
  502,zentorno_modkit,zentorno,true,2024-01-15
  ```
  - **Effort**: 30 minutes

#### 1.2 Create Simple Data Models 🔴

**Goal**: Basic C# classes for CSV data

- [ ] ⬜ **1.2.1** Create Vehicle data model
  ```csharp
  // Models/Vehicle.cs
  public class Vehicle
  {
      public string ModelName { get; set; }
      public string DisplayName { get; set; }
      public string Manufacturer { get; set; }
      public string Class { get; set; }
      public string SourceDLC { get; set; }
      public List<int> CarcolIDs { get; set; } = new();
      public List<string> Tags { get; set; } = new();
      public long EstimatedSize { get; set; }
      public DateTime ImportedAt { get; set; }
  }
  ```
  - **Effort**: 45 minutes

- [ ] ⬜ **1.2.2** Create CarcolEntry data model
  ```csharp
  // Models/CarcolEntry.cs
  public class CarcolEntry
  {
      public int ID { get; set; }
      public string KitName { get; set; }
      public string VehicleModel { get; set; }
      public bool IsUsed { get; set; }
      public DateTime AllocatedAt { get; set; }
  }
  ```
  - **Effort**: 30 minutes

#### 1.3 CSV Helper Classes 🔴

**Goal**: Simple CSV read/write operations

- [ ] ⬜ **1.3.1** Create CSV utilities
  ```csharp
  // Utils/CsvHelper.cs
  public static class CsvHelper
  {
      public static List<Vehicle> ReadVehicles(string path);
      public static void WriteVehicles(string path, List<Vehicle> vehicles);
      public static List<CarcolEntry> ReadCarcolRegistry(string path);
      public static void WriteCarcolRegistry(string path, List<CarcolEntry> entries);
  }
  ```
  - Handle CSV parsing with proper escaping
  - Support for comma-separated lists in cells
  - **Effort**: 2 hours

- [ ] ⬜ **1.3.2** Add CSV validation
  - Check required fields
  - Validate data types
  - Report parsing errors clearly
  - **Effort**: 1 hour

#### 1.4 Registry Scanner 🔴

**Goal**: Scan existing merged DLC and build registry

- [ ] ⬜ **1.4.1** Create VehicleScanner class
  ```csharp
  // Services/VehicleScanner.cs
  public class VehicleScanner
  {
      public List<Vehicle> ScanMergedDLC(string dlcPath);
      private Vehicle ExtractVehicleInfo(RpfFileEntry vehicleEntry);
      private List<int> FindCarcolIDs(string vehicleName);
  }
  ```
  - Scan vehicles.meta for vehicle list
  - Extract display names and properties
  - Find associated carcol IDs
  - Estimate file sizes
  - **Effort**: 3 hours

- [ ] ⬜ **1.4.2** Add progress reporting
  - Show scanning progress
  - Report found vehicles
  - Log any issues
  - **Effort**: 1 hour

---

## Phase 2: Core Vehicle Operations (1 week)

### Goal: Remove, edit, and manage individual vehicles

#### 2.1 Vehicle Removal System 🔴

**Goal**: Cleanly remove vehicles from merged DLC

- [ ] ⬜ **2.1.1** Create VehicleRemover class
  ```csharp
  // Services/VehicleRemover.cs
  public class VehicleRemover
  {
      public async Task<RemovalResult> RemoveVehicleAsync(string vehicleModel);
      private void RemoveFromXmlFiles(string vehicleModel);
      private void RemoveModelFiles(string vehicleModel);
      private void UpdateCarcolRegistry(string vehicleModel);
  }
  ```
  - **Effort**: 2 hours

- [ ] ⬜ **2.1.2** Implement XML cleanup
  - Remove from vehicles.meta
  - Remove from handling.meta
  - Remove from carvariations.meta
  - Remove from carcols.meta
  - Clean up empty entries
  - **Effort**: 2 hours

- [ ] ⬜ **2.1.3** Implement file cleanup
  - Remove .yft/.ytd/.ycd files
  - Remove from vehicles.rpf
  - Update RPF size tracking
  - **Effort**: 1.5 hours

#### 2.2 Vehicle Editing System 🟠

**Goal**: Edit vehicle properties without full reimport

- [ ] ⬜ **2.2.1** Create VehicleEditor class
  ```csharp
  // Services/VehicleEditor.cs
  public class VehicleEditor
  {
      public void UpdateDisplayName(string vehicleModel, string newName);
      public void UpdateTags(string vehicleModel, List<string> tags);
      public void UpdateManufacturer(string vehicleModel, string manufacturer);
      public void UpdateClass(string vehicleModel, string vehicleClass);
  }
  ```
  - **Effort**: 1.5 hours

- [ ] ⬜ **2.2.2** Registry synchronization
  - Update CSV files
  - Validate changes
  - Backup before changes
  - **Effort**: 1 hour

#### 2.3 Carcol ID Management 🟠

**Goal**: Manage carcol IDs when vehicles are removed

- [ ] ⬜ **2.3.1** Create CarcolManager class
  ```csharp
  // Services/CarcolManager.cs
  public class CarcolManager
  {
      public List<int> GetAvailableIDs();
      public int AllocateNextID();
      public void ReleaseIDs(List<int> ids);
      public List<int> FindConflicts();
  }
  ```
  - **Effort**: 1.5 hours

- [ ] ⬜ **2.3.2** Add ID reallocation
  - Detect gaps in ID sequence
  - Suggest ID optimizations
  - Handle ID conflicts
  - **Effort**: 1 hour

---

## Phase 3: Organization and Search Tools (1 week)

### Goal: Easy vehicle discovery and organization

#### 3.1 Search and Filter System 🟠

**Goal**: Quick vehicle lookup

- [ ] ⬜ **3.1.1** Create VehicleQuery class
  ```csharp
  // Services/VehicleQuery.cs
  public class VehicleQuery
  {
      public List<Vehicle> Search(string term);
      public List<Vehicle> FilterByManufacturer(string manufacturer);
      public List<Vehicle> FilterByClass(string vehicleClass);
      public List<Vehicle> FilterByTags(List<string> tags);
      public List<Vehicle> FilterBySource(string sourceDLC);
  }
  ```
  - **Effort**: 2 hours

- [ ] ⬜ **3.1.2** Add sorting options
  - Sort by name, manufacturer, class
  - Sort by size, import date
  - Custom sort orders
  - **Effort**: 1 hour

#### 3.2 Overview and Statistics 🟡

**Goal**: Quick overview of vehicle collection

- [ ] ⬜ **3.2.1** Create summary generator
  ```csharp
  // Services/VehicleSummary.cs
  public class VehicleSummary
  {
      public SummaryReport GenerateReport();
      public Dictionary<string, int> GetManufacturerCounts();
      public Dictionary<string, int> GetClassCounts();
      public long GetTotalSize();
  }
  ```
  - **Effort**: 1.5 hours

- [ ] ⬜ **3.2.2** Add size management
  - Show RPF sizes
  - Warn about size limits
  - Suggest optimizations
  - **Effort**: 1 hour

#### 3.3 Tagging System 🟡

**Goal**: Simple vehicle categorization

- [ ] ⬜ **3.3.1** Tag management
  - Add/remove tags from vehicles
  - List all available tags
  - Tag-based filtering
  - **Effort**: 1 hour

- [ ] ⬜ **3.3.2** Bulk tagging
  - Tag multiple vehicles at once
  - Tag by criteria (manufacturer, class)
  - Import tags from CSV
  - **Effort**: 1 hour

---

## Phase 4: Enhanced CLI Interface (3 days)

### Goal: User-friendly command line interface

#### 4.1 Core Commands 🔴

**Essential commands for daily use:**

- [ ] ⬜ **4.1.1** Implement `list` command
  ```bash
  dlcmerger list                    # Show all vehicles
  dlcmerger list --manufacturer Pegassi
  dlcmerger list --class Super
  dlcmerger list --tag racing
  dlcmerger list --source dlc_cars
  ```
  - **Effort**: 1.5 hours

- [ ] ⬜ **4.1.2** Implement `info` command  
  ```bash
  dlcmerger info adder              # Show vehicle details
  dlcmerger info adder --full       # Include file details
  ```
  - **Effort**: 1 hour

- [ ] ⬜ **4.1.3** Implement `remove` command
  ```bash
  dlcmerger remove adder            # Remove vehicle
  dlcmerger remove adder --dry-run  # Preview removal
  dlcmerger remove adder --keep-files # Remove from registry only
  ```
  - **Effort**: 1.5 hours

- [ ] ⬜ **4.1.4** Implement `search` command
  ```bash
  dlcmerger search "truffade"       # Search by name
  dlcmerger search --fast           # Search by tag
  ```
  - **Effort**: 1 hour

#### 4.2 Management Commands 🟠

- [ ] ⬜ **4.2.1** Implement `edit` command
  ```bash
  dlcmerger edit adder --name "Custom Adder"
  dlcmerger edit adder --tags "fast,luxury,custom"
  dlcmerger edit adder --manufacturer "Custom"
  ```
  - **Effort**: 1.5 hours

- [ ] ⬜ **4.2.2** Implement `tag` command
  ```bash
  dlcmerger tag adder racing        # Add tag
  dlcmerger tag adder --remove luxury # Remove tag
  dlcmerger tag --list              # List all tags
  ```
  - **Effort**: 1 hour

- [ ] ⬜ **4.2.3** Implement `carcol` command
  ```bash
  dlcmerger carcol list             # Show ID usage
  dlcmerger carcol conflicts        # Show conflicts
  dlcmerger carcol next             # Show next available ID
  ```
  - **Effort**: 1 hour

#### 4.3 Utility Commands 🟡

- [ ] ⬜ **4.3.1** Implement `summary` command
  ```bash
  dlcmerger summary                 # Overview of collection
  dlcmerger summary --detailed      # Detailed breakdown
  ```
  - **Effort**: 1 hour

- [ ] ⬜ **4.3.2** Implement `validate` command
  ```bash
  dlcmerger validate                # Check for issues
  dlcmerger validate --fix          # Auto-fix simple issues
  ```
  - **Effort**: 1.5 hours

- [ ] ⬜ **4.3.3** Implement `scan` command
  ```bash
  dlcmerger scan /path/to/dlc       # Build registry from existing DLC
  dlcmerger scan --update           # Update existing registry
  ```
  - **Effort**: 1 hour

---

## Phase 5: RPF Size Management (2 days)

### Goal: Handle 4GB RPF limits

#### 5.1 Size Monitoring 🟠

- [ ] ⬜ **5.1.1** Add size tracking
  - Monitor RPF sizes
  - Warn when approaching 3.5GB
  - Show size breakdown by vehicle
  - **Effort**: 2 hours

- [ ] ⬜ **5.1.2** Size optimization
  - Find unused files
  - Suggest vehicles to remove
  - Compress textures if possible
  - **Effort**: 2 hours

#### 5.2 Simple Splitting 🟡

- [ ] ⬜ **5.2.1** Basic RPF splitting
  - Split when size limit reached
  - Maintain cross-references
  - Update manifests
  - **Effort**: 3 hours

---

## Implementation Schedule

### **Week 1: Core Foundation**
- Days 1-2: Vehicle registry and CSV system
- Days 3-4: Vehicle scanning and data models
- Day 5: Testing and validation

### **Week 2: Vehicle Operations**  
- Days 1-2: Vehicle removal system
- Days 3-4: Vehicle editing and carcol management
- Day 5: Testing and refinement

### **Week 3: Interface and Polish**
- Days 1-3: CLI commands implementation
- Days 4-5: Size management and final testing

### **Week 4: Documentation and Optimization**
- Days 1-2: User documentation
- Days 3-4: Performance optimization
- Day 5: Release preparation

---

## Key Simplifications Made

### **Removed Complex Features:**
- ❌ Full database system (SQLite)
- ❌ Web interface
- ❌ Complex import/export wizards
- ❌ Advanced analytics
- ❌ Multi-user support
- ❌ Plugin system
- ❌ REST API

### **Kept Essential Features:**
- ✅ Vehicle inventory (CSV-based)
- ✅ Remove individual vehicles
- ✅ Edit vehicle properties
- ✅ Search and filter vehicles
- ✅ Carcol ID management
- ✅ Simple CLI interface
- ✅ Basic size management

---

## Files Structure

```
CodeWalker.DLCMerger/
├── Models/
│   ├── Vehicle.cs
│   ├── CarcolEntry.cs
│   └── SummaryReport.cs
├── Services/
│   ├── VehicleScanner.cs
│   ├── VehicleRemover.cs
│   ├── VehicleEditor.cs
│   ├── CarcolManager.cs
│   ├── VehicleQuery.cs
│   └── VehicleSummary.cs
├── Utils/
│   ├── CsvHelper.cs
│   └── PathHelper.cs
├── Commands/
│   ├── ListCommand.cs
│   ├── RemoveCommand.cs
│   ├── EditCommand.cs
│   └── ScanCommand.cs
└── Data/ (output directory)
    ├── vehicles.csv
    ├── carcol_registry.csv
    ├── rpf_manifest.csv
    └── config.json
```

---

## Success Criteria

1. **Functionality**
   - ✅ Remove any vehicle in < 30 seconds
   - ✅ List/search 2000+ vehicles in < 1 second
   - ✅ Edit vehicle properties instantly
   - ✅ Detect carcol conflicts automatically

2. **Simplicity**
   - ✅ All data in human-readable CSV files
   - ✅ No database installation required
   - ✅ Single executable file
   - ✅ Intuitive command names

3. **Reliability**
   - ✅ Never corrupt existing DLC files
   - ✅ Always backup before changes
   - ✅ Clear error messages
   - ✅ Validate operations before execution

This simplified approach gives you exactly what you need - easy vehicle management - without the complexity of a full-featured system.