# Vehicle Management System - Detailed TODO List

## Overview

This document provides a comprehensive, step-by-step implementation plan for transforming the CodeWalker DLC Merger into a full-featured vehicle management system capable of handling 2000+ vehicles with RPF size management, database storage, and carcol ID tracking.

## Database Decision: SQLite (Recommended)

For 2000+ vehicles, **SQLite is strongly recommended** over CSV due to:
- **100-1000x faster queries** with proper indexing
- **ACID compliance** for data integrity
- **Foreign key constraints** for relationship management
- **SQL query capabilities** for complex filtering/sorting
- **Concurrent access** support
- **Single file** database (vehicles.db)

## Task Organization

Tasks are organized by:
- **Priority**: 🔴 Critical | 🟠 High | 🟡 Medium | 🟢 Low
- **Effort**: Estimated hours/days
- **Dependencies**: Required prior tasks
- **Status**: ⬜ Not Started | 🟦 In Progress | ✅ Complete

---

## Phase 1: Core Architecture Refactoring

### 1.1 Project Setup and Dependencies 🔴

**Goal**: Prepare project for modern architecture

#### Tasks:
- [ ] ⬜ **1.1.1** Update CodeWalker.DLCMerger.csproj
  - Add package reference: `Microsoft.Extensions.DependencyInjection` (8.0.0)
  - Add package reference: `Microsoft.Extensions.Logging` (8.0.0)
  - Add package reference: `Microsoft.Extensions.Configuration` (8.0.0)
  - **Effort**: 30 minutes
  
- [ ] ⬜ **1.1.2** Add database packages
  - Add package reference: `Microsoft.Data.Sqlite` (8.0.0)
  - Add package reference: `Microsoft.EntityFrameworkCore.Sqlite` (8.0.0)
  - Add package reference: `Dapper` (2.1.28) - for lightweight queries
  - **Effort**: 30 minutes

- [ ] ⬜ **1.1.3** Add testing frameworks
  - Add package reference: `xunit` (2.6.3)
  - Add package reference: `Moq` (4.20.70)
  - Add package reference: `FluentAssertions` (6.12.0)
  - Create test project: `CodeWalker.DLCMerger.Tests`
  - **Effort**: 1 hour

### 1.2 Create Domain Entities 🔴

**Goal**: Define core business objects

#### Tasks:
- [ ] ⬜ **1.2.1** Create Domain folder structure
  ```
  Domain/
  ├── Entities/
  ├── ValueObjects/
  ├── Interfaces/
  └── Exceptions/
  ```
  - **Effort**: 15 minutes

- [ ] ⬜ **1.2.2** Create Vehicle entity
  ```csharp
  // Domain/Entities/Vehicle.cs
  public class Vehicle
  {
      public Guid Id { get; set; }
      public string ModelName { get; set; } // e.g., "adder"
      public string DisplayName { get; set; } // e.g., "Truffade Adder"
      public string ManufacturerId { get; set; }
      public string Class { get; set; } // e.g., "Super"
      public string HandlingId { get; set; }
      public VehicleMetadata Metadata { get; set; }
      public List<CarcolKit> Kits { get; set; }
      public List<string> Tags { get; set; }
      public DateTime ImportedAt { get; set; }
      public string SourceDlc { get; set; }
      public long EstimatedSize { get; set; } // in bytes
  }
  ```
  - **Effort**: 1 hour

- [ ] ⬜ **1.2.3** Create CarcolKit entity
  ```csharp
  // Domain/Entities/CarcolKit.cs
  public class CarcolKit
  {
      public Guid Id { get; set; }
      public string Name { get; set; }
      public int GameId { get; set; } // The ID in game files
      public int ManagedId { get; set; } // Our managed ID
      public string VehicleModelName { get; set; }
      public List<ModComponent> Components { get; set; }
      public bool IsEmpty { get; set; }
      public DateTime CreatedAt { get; set; }
  }
  ```
  - **Effort**: 45 minutes

- [ ] ⬜ **1.2.4** Create supporting value objects
  - VehicleMetadata (handling stats, audio, flags)
  - ModComponent (kit parts)
  - RpfReference (source file tracking)
  - **Effort**: 2 hours

### 1.3 Service Interface Layer 🔴

**Goal**: Define contracts for all services

#### Tasks:
- [ ] ⬜ **1.3.1** Create IVehicleRepository interface
  ```csharp
  public interface IVehicleRepository
  {
      Task<Vehicle> GetByIdAsync(Guid id);
      Task<Vehicle> GetByModelNameAsync(string modelName);
      Task<IEnumerable<Vehicle>> GetAllAsync();
      Task<IEnumerable<Vehicle>> SearchAsync(VehicleSearchCriteria criteria);
      Task<Guid> AddAsync(Vehicle vehicle);
      Task UpdateAsync(Vehicle vehicle);
      Task DeleteAsync(Guid id);
      Task<int> CountAsync();
  }
  ```
  - **Effort**: 1 hour

- [ ] ⬜ **1.3.2** Create service interfaces
  - IRpfService (RPF operations)
  - IXmlMergeService (XML processing)
  - ICarcolIdService (ID management)
  - IImportExportService (data transfer)
  - IValidationService (data validation)
  - **Effort**: 2 hours

- [ ] ⬜ **1.3.3** Create factory interfaces
  - IRpfSplitterFactory
  - IXmlProcessorFactory
  - **Effort**: 30 minutes

### 1.4 Dependency Injection Setup 🔴

**Goal**: Configure DI container

#### Tasks:
- [ ] ⬜ **1.4.1** Create ServiceCollectionExtensions
  ```csharp
  public static class ServiceCollectionExtensions
  {
      public static IServiceCollection AddDlcMergerServices(
          this IServiceCollection services,
          IConfiguration configuration)
      {
          // Add repositories
          services.AddScoped<IVehicleRepository, SqliteVehicleRepository>();
          
          // Add services
          services.AddScoped<IRpfService, RpfService>();
          
          // Add factories
          services.AddSingleton<IRpfSplitterFactory, RpfSplitterFactory>();
          
          return services;
      }
  }
  ```
  - **Effort**: 1 hour

- [ ] ⬜ **1.4.2** Update Program.cs for DI
  - Build service provider
  - Resolve services
  - Handle disposal
  - **Effort**: 1 hour

### 1.5 Error Handling Framework 🟠

**Goal**: Implement consistent error handling

#### Tasks:
- [ ] ⬜ **1.5.1** Create custom exceptions
  - VehicleNotFoundException
  - RpfSizeLimitException
  - CarcolIdConflictException
  - XmlMergeException
  - **Effort**: 1 hour

- [ ] ⬜ **1.5.2** Implement global error handler
  - Catch and log exceptions
  - User-friendly error messages
  - Error recovery strategies
  - **Effort**: 2 hours

---

## Phase 2: Database Implementation

### 2.1 Database Schema Design 🔴

**Goal**: Create optimized database schema

#### Tasks:
- [ ] ⬜ **2.1.1** Create initial migration script
  ```sql
  -- Migrations/001_InitialSchema.sql
  CREATE TABLE Manufacturers (
      Id TEXT PRIMARY KEY,
      Name TEXT NOT NULL UNIQUE,
      DisplayName TEXT
  );
  
  CREATE TABLE VehicleClasses (
      Id TEXT PRIMARY KEY,
      Name TEXT NOT NULL UNIQUE,
      Description TEXT
  );
  
  CREATE TABLE Vehicles (
      Id TEXT PRIMARY KEY,
      ModelName TEXT NOT NULL UNIQUE,
      DisplayName TEXT NOT NULL,
      ManufacturerId TEXT NOT NULL,
      ClassId TEXT NOT NULL,
      HandlingId TEXT NOT NULL,
      AudioHash TEXT,
      Layout TEXT,
      Flags INTEGER,
      SourceDlc TEXT,
      ImportedAt TEXT NOT NULL,
      EstimatedSize INTEGER,
      Metadata TEXT, -- JSON
      FOREIGN KEY (ManufacturerId) REFERENCES Manufacturers(Id),
      FOREIGN KEY (ClassId) REFERENCES VehicleClasses(Id)
  );
  
  CREATE TABLE CarcolKits (
      Id TEXT PRIMARY KEY,
      Name TEXT NOT NULL,
      GameId INTEGER NOT NULL,
      ManagedId INTEGER NOT NULL UNIQUE,
      VehicleModelName TEXT NOT NULL,
      IsEmpty INTEGER NOT NULL DEFAULT 0,
      Components TEXT, -- JSON
      CreatedAt TEXT NOT NULL,
      FOREIGN KEY (VehicleModelName) REFERENCES Vehicles(ModelName)
  );
  
  CREATE TABLE VehicleTags (
      VehicleId TEXT NOT NULL,
      Tag TEXT NOT NULL,
      PRIMARY KEY (VehicleId, Tag),
      FOREIGN KEY (VehicleId) REFERENCES Vehicles(Id)
  );
  ```
  - **Effort**: 2 hours

- [ ] ⬜ **2.1.2** Create indexes for performance
  ```sql
  CREATE INDEX idx_vehicles_model ON Vehicles(ModelName);
  CREATE INDEX idx_vehicles_class ON Vehicles(ClassId);
  CREATE INDEX idx_vehicles_manufacturer ON Vehicles(ManufacturerId);
  CREATE INDEX idx_kits_managed_id ON CarcolKits(ManagedId);
  CREATE INDEX idx_kits_vehicle ON CarcolKits(VehicleModelName);
  ```
  - **Effort**: 30 minutes

- [ ] ⬜ **2.1.3** Create views for common queries
  ```sql
  CREATE VIEW VehicleDetails AS
  SELECT 
      v.*,
      m.DisplayName as ManufacturerName,
      c.Name as ClassName
  FROM Vehicles v
  JOIN Manufacturers m ON v.ManufacturerId = m.Id
  JOIN VehicleClasses c ON v.ClassId = c.Id;
  ```
  - **Effort**: 1 hour

### 2.2 Repository Implementation 🔴

**Goal**: Implement data access layer

#### Tasks:
- [ ] ⬜ **2.2.1** Create DatabaseContext
  ```csharp
  public class VehicleDbContext : DbContext
  {
      public DbSet<Vehicle> Vehicles { get; set; }
      public DbSet<CarcolKit> CarcolKits { get; set; }
      public DbSet<Manufacturer> Manufacturers { get; set; }
      public DbSet<VehicleClass> VehicleClasses { get; set; }
      
      protected override void OnModelCreating(ModelBuilder modelBuilder)
      {
          // Configure entity mappings
      }
  }
  ```
  - **Effort**: 2 hours

- [ ] ⬜ **2.2.2** Implement SqliteVehicleRepository
  - CRUD operations
  - Search with filters
  - Bulk operations
  - Transaction support
  - **Effort**: 4 hours

- [ ] ⬜ **2.2.3** Implement CarcolKitRepository
  - ID allocation logic
  - Conflict detection
  - Bulk reassignment
  - **Effort**: 3 hours

### 2.3 Database Migration System 🟠

**Goal**: Version-controlled schema updates

#### Tasks:
- [ ] ⬜ **2.3.1** Create migration runner
  ```csharp
  public class MigrationRunner
  {
      public async Task RunMigrationsAsync(string connectionString)
      {
          // Check current version
          // Run pending migrations
          // Update version table
      }
  }
  ```
  - **Effort**: 2 hours

- [ ] ⬜ **2.3.2** Add migration tracking table
  - Track applied migrations
  - Support rollback
  - **Effort**: 1 hour

---

## Phase 3: RPF Size Management

### 3.1 RPF Splitter Implementation 🔴

**Goal**: Handle 4GB RPF size limit

#### Tasks:
- [ ] ⬜ **3.1.1** Create RpfSizeCalculator
  ```csharp
  public class RpfSizeCalculator
  {
      public long CalculateSize(IEnumerable<RpfEntry> entries);
      public long EstimateCompressedSize(RpfEntry entry);
      public bool WillExceedLimit(long currentSize, RpfEntry newEntry);
  }
  ```
  - **Effort**: 2 hours

- [ ] ⬜ **3.1.2** Implement RpfSplitter
  ```csharp
  public class RpfSplitter
  {
      private const long MaxRpfSize = 3_500_000_000; // 3.5GB safety margin
      
      public async Task<SplitResult> SplitRpfAsync(
          string sourcePath,
          SplitOptions options)
      {
          // Monitor size during build
          // Split when approaching limit
          // Maintain relationships
      }
  }
  ```
  - **Effort**: 4 hours

- [ ] ⬜ **3.1.3** Create split manifest system
  - Track which vehicles in which RPF
  - Maintain cross-references
  - Generate loading order
  - **Effort**: 2 hours

### 3.2 Vehicle Distribution Algorithm 🟠

**Goal**: Optimize vehicle placement across RPFs

#### Tasks:
- [ ] ⬜ **3.2.1** Implement distribution strategy
  ```csharp
  public interface IDistributionStrategy
  {
      List<RpfBundle> DistributeVehicles(
          List<Vehicle> vehicles,
          DistributionOptions options);
  }
  
  public class OptimalDistributionStrategy : IDistributionStrategy
  {
      // Keep related vehicles together
      // Balance sizes across RPFs
      // Minimize cross-references
  }
  ```
  - **Effort**: 3 hours

- [ ] ⬜ **3.2.2** Create relationship analyzer
  - Detect vehicle variants
  - Find shared resources
  - Group by manufacturer/class
  - **Effort**: 2 hours

### 3.3 RPF Chain Management 🟠

**Goal**: Handle multiple related RPFs

#### Tasks:
- [ ] ⬜ **3.3.1** Create RpfChainManager
  - Track RPF relationships
  - Generate load order
  - Validate dependencies
  - **Effort**: 2 hours

- [ ] ⬜ **3.3.2** Implement cross-RPF references
  - Update content.xml
  - Maintain reference integrity
  - **Effort**: 2 hours

---

## Phase 4: Carcol ID Management

### 4.1 ID Registry System 🔴

**Goal**: Global carcol ID management

#### Tasks:
- [ ] ⬜ **4.1.1** Create CarcolIdRegistry
  ```csharp
  public class CarcolIdRegistry
  {
      private readonly ICarcolKitRepository _repository;
      
      public async Task<int> AllocateIdAsync(string kitName)
      {
          // Check existing allocations
          // Find next available ID
          // Reserve ID
          // Return allocated ID
      }
      
      public async Task<bool> IsIdAvailableAsync(int id);
      public async Task<Dictionary<int, string>> GetAllAllocationsAsync();
  }
  ```
  - **Effort**: 2 hours

- [ ] ⬜ **4.1.2** Implement ID allocation strategies
  - Sequential (0, 1, 2, 3...)
  - Grouped (by vehicle/manufacturer)
  - Reserved ranges (modder-specific)
  - Custom patterns
  - **Effort**: 3 hours

- [ ] ⬜ **4.1.3** Create conflict detection
  ```csharp
  public class CarcolConflictDetector
  {
      public async Task<List<CarcolConflict>> DetectConflictsAsync();
      public async Task<Resolution> SuggestResolutionAsync(CarcolConflict conflict);
  }
  ```
  - **Effort**: 2 hours

### 4.2 Bulk ID Operations 🟠

**Goal**: Efficient ID management for many vehicles

#### Tasks:
- [ ] ⬜ **4.2.1** Implement bulk reassignment
  ```csharp
  public async Task ReassignIdsAsync(
      IdReassignmentStrategy strategy,
      int startId = 0)
  {
      // Get all kits
      // Apply strategy
      // Update in batches
      // Update XML files
  }
  ```
  - **Effort**: 3 hours

- [ ] ⬜ **4.2.2** Create ID import/export
  - Export current mappings to CSV
  - Import predefined mappings
  - Validate imported IDs
  - **Effort**: 2 hours

### 4.3 XML Update System 🟠

**Goal**: Update game files with new IDs

#### Tasks:
- [ ] ⬜ **4.3.1** Create XmlIdUpdater
  - Parse carcols.meta
  - Update kit IDs
  - Preserve formatting
  - **Effort**: 2 hours

- [ ] ⬜ **4.3.2** Implement batch XML updates
  - Process multiple files
  - Transaction support
  - Rollback capability
  - **Effort**: 2 hours

---

## Phase 5: Import/Export System

### 5.1 CSV Import/Export 🟠

**Goal**: Bulk data management via CSV

#### Tasks:
- [ ] ⬜ **5.1.1** Define CSV format
  ```csv
  ModelName,DisplayName,Manufacturer,Class,HandlingId,Tags,KitIds
  adder,"Truffade Adder",Truffade,Super,ADDER,"fast,luxury","0,501"
  ```
  - **Effort**: 30 minutes

- [ ] ⬜ **5.1.2** Implement CSV exporter
  ```csharp
  public class CsvExporter
  {
      public async Task ExportVehiclesAsync(
          string path,
          ExportOptions options)
      {
          // Query vehicles
          // Apply filters
          // Write CSV with proper escaping
      }
  }
  ```
  - **Effort**: 2 hours

- [ ] ⬜ **5.1.3** Implement CSV importer
  - Parse CSV
  - Validate data
  - Handle conflicts
  - Batch insert
  - **Effort**: 3 hours

### 5.2 Mod Package Import 🟡

**Goal**: Direct import from mod archives

#### Tasks:
- [ ] ⬜ **5.2.1** Create mod package analyzer
  - Detect package type (RPF, ZIP, folder)
  - Extract metadata
  - Find vehicle files
  - **Effort**: 3 hours

- [ ] ⬜ **5.2.2** Implement import wizard
  - Preview vehicles
  - Detect conflicts
  - Choose import options
  - **Effort**: 3 hours

### 5.3 Export Templates 🟡

**Goal**: Generate ready-to-use mod packages

#### Tasks:
- [ ] ⬜ **5.3.1** Create package builder
  - Generate folder structure
  - Copy required files
  - Create manifests
  - **Effort**: 2 hours

- [ ] ⬜ **5.3.2** Add FiveM/SP support
  - Generate resource.lua
  - Create installation instructions
  - **Effort**: 2 hours

---

## Phase 6: User Interface Enhancements

### 6.1 Enhanced CLI Commands 🟠

**Goal**: Comprehensive command-line interface

#### Tasks:
- [ ] ⬜ **6.1.1** Add database commands
  ```bash
  dlcmerger db init                    # Initialize database
  dlcmerger db stats                   # Show statistics
  dlcmerger db backup <path>           # Backup database
  dlcmerger db restore <path>          # Restore database
  ```
  - **Effort**: 2 hours

- [ ] ⬜ **6.1.2** Add vehicle management commands
  ```bash
  dlcmerger vehicle add <rpf>          # Add vehicles from RPF
  dlcmerger vehicle list [--filter]    # List vehicles
  dlcmerger vehicle remove <model>     # Remove vehicle
  dlcmerger vehicle tag <model> <tags> # Tag vehicle
  ```
  - **Effort**: 3 hours

- [ ] ⬜ **6.1.3** Add carcol commands
  ```bash
  dlcmerger carcol list                # List all IDs
  dlcmerger carcol assign <kit> <id>   # Assign specific ID
  dlcmerger carcol reassign --strategy # Bulk reassign
  dlcmerger carcol conflicts           # Show conflicts
  ```
  - **Effort**: 2 hours

### 6.2 Progress Monitoring 🟡

**Goal**: Real-time operation feedback

#### Tasks:
- [ ] ⬜ **6.2.1** Implement progress reporting
  ```csharp
  public interface IProgressReporter
  {
      void Report(ProgressInfo info);
      event EventHandler<ProgressInfo> ProgressChanged;
  }
  ```
  - **Effort**: 1 hour

- [ ] ⬜ **6.2.2** Add progress bars
  - Console progress bars
  - ETA calculation
  - Detailed status
  - **Effort**: 2 hours

### 6.3 Configuration System 🟡

**Goal**: Flexible configuration

#### Tasks:
- [ ] ⬜ **6.3.1** Create configuration schema
  ```json
  {
    "database": {
      "path": "vehicles.db",
      "backupOnStart": true
    },
    "rpf": {
      "maxSize": 3500000000,
      "compressionLevel": 5
    },
    "carcol": {
      "startId": 1000,
      "reservedRanges": {
        "vanilla": "0-999",
        "custom": "1000-9999"
      }
    }
  }
  ```
  - **Effort**: 1 hour

- [ ] ⬜ **6.3.2** Implement configuration loader
  - JSON/YAML support
  - Environment variables
  - Command-line overrides
  - **Effort**: 2 hours

---

## Phase 7: Testing and Validation

### 7.1 Unit Tests 🔴

**Goal**: Comprehensive test coverage

#### Tasks:
- [ ] ⬜ **7.1.1** Test domain entities
  - Vehicle validation
  - CarcolKit logic
  - Value objects
  - **Effort**: 3 hours

- [ ] ⬜ **7.1.2** Test services
  - Repository operations
  - ID allocation
  - RPF splitting
  - **Effort**: 4 hours

- [ ] ⬜ **7.1.3** Test utilities
  - XML parsing
  - CSV import/export
  - Size calculations
  - **Effort**: 2 hours

### 7.2 Integration Tests 🟠

**Goal**: Test component interactions

#### Tasks:
- [ ] ⬜ **7.2.1** Database integration tests
  - Repository operations
  - Transaction handling
  - Migration runner
  - **Effort**: 3 hours

- [ ] ⬜ **7.2.2** RPF processing tests
  - File extraction
  - Size management
  - Split/merge operations
  - **Effort**: 3 hours

### 7.3 Performance Tests 🟡

**Goal**: Ensure scalability

#### Tasks:
- [ ] ⬜ **7.3.1** Load testing
  - 2000+ vehicle operations
  - Query performance
  - Memory usage
  - **Effort**: 2 hours

- [ ] ⬜ **7.3.2** Optimization
  - Profile bottlenecks
  - Optimize queries
  - Add caching
  - **Effort**: 3 hours

---

## Phase 8: Documentation

### 8.1 User Documentation 🟡

**Goal**: Comprehensive user guide

#### Tasks:
- [ ] ⬜ **8.1.1** Create user manual
  - Installation guide
  - Command reference
  - Workflow examples
  - **Effort**: 4 hours

- [ ] ⬜ **8.1.2** Add video tutorials
  - Basic usage
  - Advanced features
  - Troubleshooting
  - **Effort**: 4 hours

### 8.2 Developer Documentation 🟡

**Goal**: Enable contributions

#### Tasks:
- [ ] ⬜ **8.2.1** API documentation
  - Service interfaces
  - Extension points
  - Plugin system
  - **Effort**: 3 hours

- [ ] ⬜ **8.2.2** Architecture guide
  - Design decisions
  - Code structure
  - Contributing guide
  - **Effort**: 2 hours

---

## Implementation Schedule

### Week 1-2: Foundation
- Phase 1.1-1.3: Core architecture
- Phase 2.1-2.2: Database implementation

### Week 3-4: Core Features  
- Phase 3: RPF size management
- Phase 4: Carcol ID management

### Week 5-6: Import/Export
- Phase 5: Import/export system
- Phase 6.1: CLI enhancements

### Week 7-8: Polish
- Phase 7: Testing
- Phase 8: Documentation

### Week 9-10: Advanced Features
- Web UI (if desired)
- API development
- Performance optimization

---

## Risk Mitigation

### Technical Risks
1. **RPF Format Changes**: Maintain compatibility layer
2. **Database Corruption**: Regular backups, transaction logs
3. **Memory Issues**: Streaming operations, chunked processing
4. **ID Conflicts**: Comprehensive validation, rollback support

### Mitigation Strategies
- Incremental development with working system at each phase
- Comprehensive testing at each stage
- Backward compatibility maintenance
- Clear rollback procedures

---

## Success Metrics

1. **Performance**
   - Query response < 100ms for 2000+ vehicles
   - RPF split operation < 5 minutes for 4GB file
   - Memory usage < 500MB during operations

2. **Reliability**
   - 99.9% operation success rate
   - Zero data loss incidents
   - Automatic error recovery

3. **Usability**
   - 90% of operations completable via CLI
   - Clear error messages
   - Comprehensive documentation

---

## Notes for Implementation

1. **Start Simple**: Begin with basic CRUD operations, add complexity gradually
2. **Test Early**: Write tests alongside implementation
3. **Document As You Go**: Update documentation with each feature
4. **User Feedback**: Get feedback after each phase
5. **Performance First**: Profile and optimize from the beginning

This TODO list provides a clear, step-by-step path to implementing a robust vehicle management system. Each task is small enough to be completed independently, yet contributes to the larger goal.