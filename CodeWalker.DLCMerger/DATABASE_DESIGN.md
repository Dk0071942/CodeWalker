# Vehicle Management Database Design

## Overview

This document details the SQLite database schema for the Vehicle Management System, designed to handle 2000+ vehicles efficiently with proper relationships, indexing, and data integrity.

## Database Choice: SQLite

### Why SQLite?
- **Self-contained**: Single file database (vehicles.db)
- **Serverless**: No installation or configuration required
- **Cross-platform**: Works on Windows, Linux, macOS
- **Performance**: Excellent for read-heavy workloads with proper indexing
- **ACID compliant**: Reliable transactions
- **SQL support**: Full SQL query capabilities

### Performance Characteristics
With proper indexing, SQLite can handle:
- **2000+ vehicles**: Sub-millisecond query times
- **Complex joins**: Manufacturer/class relationships
- **Full-text search**: Vehicle name/description search
- **Concurrent reads**: Multiple processes can read simultaneously

---

## Schema Design

### Core Tables

#### 1. Manufacturers
Stores vehicle manufacturer information.

```sql
CREATE TABLE Manufacturers (
    Id TEXT PRIMARY KEY,                -- e.g., "pegassi", "grotti"
    Name TEXT NOT NULL UNIQUE,          -- e.g., "Pegassi", "Grotti"
    DisplayName TEXT,                   -- e.g., "Pegassi Sports Cars"
    Description TEXT,                   -- Optional manufacturer info
    CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),
    UpdatedAt TEXT NOT NULL DEFAULT (datetime('now'))
);

-- Sample data
INSERT INTO Manufacturers VALUES 
('pegassi', 'Pegassi', 'Pegassi Sports Cars', 'Italian luxury sports car manufacturer', datetime('now'), datetime('now')),
('grotti', 'Grotti', 'Grotti', 'Italian luxury vehicle manufacturer', datetime('now'), datetime('now')),
('vapid', 'Vapid', 'Vapid Motor Company', 'American vehicle manufacturer', datetime('now'), datetime('now'));
```

#### 2. VehicleClasses
Defines GTA V vehicle classifications.

```sql
CREATE TABLE VehicleClasses (
    Id TEXT PRIMARY KEY,                -- e.g., "super", "sports", "suv"
    Name TEXT NOT NULL UNIQUE,          -- e.g., "Super", "Sports", "SUV"
    Description TEXT,                   -- Class description
    SortOrder INTEGER DEFAULT 0,        -- Display order
    CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),
    UpdatedAt TEXT NOT NULL DEFAULT (datetime('now'))
);

-- Sample data
INSERT INTO VehicleClasses VALUES 
('super', 'Super', 'High-performance supercars', 1, datetime('now'), datetime('now')),
('sports', 'Sports', 'Sports cars and roadsters', 2, datetime('now'), datetime('now')),
('suv', 'SUV', 'Sport utility vehicles', 3, datetime('now'), datetime('now')),
('muscle', 'Muscle', 'Classic muscle cars', 4, datetime('now'), datetime('now'));
```

#### 3. Vehicles (Core Table)
Main vehicle information with relationships.

```sql
CREATE TABLE Vehicles (
    Id TEXT PRIMARY KEY DEFAULT (lower(hex(randomblob(16)))), -- UUID
    ModelName TEXT NOT NULL UNIQUE,     -- Game model name (e.g., "adder")
    DisplayName TEXT NOT NULL,          -- Display name (e.g., "Truffade Adder")
    ManufacturerId TEXT NOT NULL,       -- Foreign key to Manufacturers
    ClassId TEXT NOT NULL,              -- Foreign key to VehicleClasses
    
    -- Game-specific properties
    HandlingId TEXT NOT NULL,           -- Handling file reference
    AudioHash TEXT,                     -- Audio reference
    Layout TEXT,                        -- Vehicle layout
    Flags INTEGER DEFAULT 0,            -- Vehicle flags (bitmask)
    
    -- Physical properties (from handling.meta)
    TopSpeed REAL,                      -- km/h
    Acceleration REAL,                  -- 0-1 scale
    Braking REAL,                       -- 0-1 scale
    Traction REAL,                      -- 0-1 scale
    
    -- Management properties
    SourceDlc TEXT,                     -- Source DLC/mod name
    ImportedAt TEXT NOT NULL DEFAULT (datetime('now')),
    EstimatedSize INTEGER DEFAULT 0,    -- Estimated file size in bytes
    IsCustom INTEGER DEFAULT 1,         -- 1 for custom, 0 for vanilla
    
    -- Metadata (JSON for flexibility)
    Metadata TEXT,                      -- JSON: additional properties
    Notes TEXT,                         -- User notes
    
    -- Timestamps
    CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),
    UpdatedAt TEXT NOT NULL DEFAULT (datetime('now')),
    
    -- Foreign key constraints
    FOREIGN KEY (ManufacturerId) REFERENCES Manufacturers(Id) ON DELETE RESTRICT,
    FOREIGN KEY (ClassId) REFERENCES VehicleClasses(Id) ON DELETE RESTRICT
);

-- Metadata JSON structure:
-- {
--   "txdName": "adder",
--   "type": "VEHICLE_TYPE_CAR",
--   "plateType": "VPT_FRONT_AND_BACK_PLATES",
--   "dashboardType": "DT_RACE",
--   "vehicleMakeName": "TRUFFADE",
--   "expressionDictName": "null",
--   "requiredExtras": "",
--   "additionalExtras": "",
--   "pOverridePhysics": "null",
--   "pOverrideRagdollThreshold": "null"
-- }
```

#### 4. CarcolKits
Manages vehicle modification kits and their IDs.

```sql
CREATE TABLE CarcolKits (
    Id TEXT PRIMARY KEY DEFAULT (lower(hex(randomblob(16)))), -- UUID
    Name TEXT NOT NULL,                 -- Kit name (e.g., "adder_modkit")
    GameId INTEGER NOT NULL,            -- Original game ID
    ManagedId INTEGER NOT NULL UNIQUE,  -- Our managed ID
    VehicleModelName TEXT NOT NULL,     -- Links to Vehicles.ModelName
    
    -- Kit properties
    IsEmpty INTEGER NOT NULL DEFAULT 0, -- 1 if kit has no modifications
    HasSpoiler INTEGER DEFAULT 0,       -- Component flags
    HasFrontBumper INTEGER DEFAULT 0,
    HasRearBumper INTEGER DEFAULT 0,
    HasSideSkirt INTEGER DEFAULT 0,
    HasExhaust INTEGER DEFAULT 0,
    HasFrame INTEGER DEFAULT 0,
    HasGrille INTEGER DEFAULT 0,
    HasHood INTEGER DEFAULT 0,
    HasFender INTEGER DEFAULT 0,
    HasRightFender INTEGER DEFAULT 0,
    HasRoof INTEGER DEFAULT 0,
    HasEngine INTEGER DEFAULT 0,
    HasBrakes INTEGER DEFAULT 0,
    HasTransmission INTEGER DEFAULT 0,
    HasHorns INTEGER DEFAULT 0,
    HasSuspension INTEGER DEFAULT 0,
    HasArmor INTEGER DEFAULT 0,
    HasTurbo INTEGER DEFAULT 0,
    HasXenon INTEGER DEFAULT 0,
    
    -- Management
    Components TEXT,                    -- JSON: detailed component list
    CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),
    UpdatedAt TEXT NOT NULL DEFAULT (datetime('now')),
    
    -- Foreign key constraints
    FOREIGN KEY (VehicleModelName) REFERENCES Vehicles(ModelName) ON DELETE CASCADE
);

-- Components JSON structure:
-- {
--   "spoiler": [{"id": 0, "name": "Stock"}, {"id": 1, "name": "Carbon Spoiler"}],
--   "frontBumper": [{"id": 0, "name": "Stock"}, {"id": 1, "name": "Race Bumper"}],
--   "linkedModels": ["adder_spoiler", "adder_bumper"]
-- }
```

#### 5. VehicleTags
Many-to-many relationship for vehicle categorization.

```sql
CREATE TABLE VehicleTags (
    VehicleId TEXT NOT NULL,
    Tag TEXT NOT NULL,
    CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),
    CreatedBy TEXT DEFAULT 'system',    -- Who added the tag
    
    PRIMARY KEY (VehicleId, Tag),
    FOREIGN KEY (VehicleId) REFERENCES Vehicles(Id) ON DELETE CASCADE
);

-- Sample tags: "fast", "luxury", "racing", "drift", "custom", "vanilla"
```

#### 6. RpfFiles
Tracks RPF files and their contents.

```sql
CREATE TABLE RpfFiles (
    Id TEXT PRIMARY KEY DEFAULT (lower(hex(randomblob(16)))), -- UUID
    FileName TEXT NOT NULL,             -- e.g., "vehicles1.rpf"
    FilePath TEXT NOT NULL,             -- Full path
    FileSize INTEGER NOT NULL,          -- Size in bytes
    MaxSize INTEGER DEFAULT 3758096384, -- 3.5GB default limit
    IsSplit INTEGER DEFAULT 0,          -- 1 if this is part of a split
    SplitIndex INTEGER DEFAULT 0,       -- 0 for unsplit, 1+ for split parts
    ParentRpfId TEXT,                   -- For split files, reference to original
    
    -- Checksums for integrity
    Md5Hash TEXT,                       -- MD5 checksum
    Sha256Hash TEXT,                    -- SHA256 checksum
    
    -- Timestamps
    CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),
    UpdatedAt TEXT NOT NULL DEFAULT (datetime('now')),
    
    FOREIGN KEY (ParentRpfId) REFERENCES RpfFiles(Id) ON DELETE SET NULL
);
```

#### 7. VehicleRpfMapping
Maps vehicles to their RPF files.

```sql
CREATE TABLE VehicleRpfMapping (
    VehicleId TEXT NOT NULL,
    RpfFileId TEXT NOT NULL,
    ModelSize INTEGER DEFAULT 0,        -- Size contribution to RPF
    TextureSize INTEGER DEFAULT 0,      -- Texture size
    CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),
    
    PRIMARY KEY (VehicleId, RpfFileId),
    FOREIGN KEY (VehicleId) REFERENCES Vehicles(Id) ON DELETE CASCADE,
    FOREIGN KEY (RpfFileId) REFERENCES RpfFiles(Id) ON DELETE CASCADE
);
```

#### 8. CarcolIdAllocations
Tracks ID allocation history and reservations.

```sql
CREATE TABLE CarcolIdAllocations (
    Id INTEGER PRIMARY KEY,             -- The allocated carcol ID
    IsReserved INTEGER DEFAULT 0,       -- 1 if reserved, 0 if allocated
    AllocatedTo TEXT,                   -- Kit name or reservation name
    AllocatedAt TEXT NOT NULL DEFAULT (datetime('now')),
    ReservedFor TEXT,                   -- Modder/project name for reservations
    ReservedUntil TEXT,                 -- Expiration date for reservations
    Notes TEXT                          -- Allocation notes
);

-- Pre-populate with vanilla game IDs (0-999 reserved)
INSERT INTO CarcolIdAllocations (Id, IsReserved, AllocatedTo, ReservedFor) 
SELECT 
    value as Id,
    1 as IsReserved,
    'VANILLA_GAME' as AllocatedTo,
    'Rockstar Games' as ReservedFor
FROM generate_series(0, 999);
```

### Lookup Tables

#### 9. VehicleProperties
Key-value store for additional vehicle properties.

```sql
CREATE TABLE VehicleProperties (
    VehicleId TEXT NOT NULL,
    PropertyName TEXT NOT NULL,
    PropertyValue TEXT,
    PropertyType TEXT DEFAULT 'string', -- string, integer, float, boolean, json
    CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),
    
    PRIMARY KEY (VehicleId, PropertyName),
    FOREIGN KEY (VehicleId) REFERENCES Vehicles(Id) ON DELETE CASCADE
);
```

#### 10. ImportLog
Tracks import operations for auditing.

```sql
CREATE TABLE ImportLog (
    Id TEXT PRIMARY KEY DEFAULT (lower(hex(randomblob(16)))),
    ImportType TEXT NOT NULL,           -- 'rpf', 'csv', 'json', 'mod_package'
    SourcePath TEXT NOT NULL,           -- Source file/directory
    VehiclesImported INTEGER DEFAULT 0, -- Count of imported vehicles
    VehiclesSkipped INTEGER DEFAULT 0,  -- Count of skipped vehicles
    ErrorCount INTEGER DEFAULT 0,       -- Count of errors
    Status TEXT DEFAULT 'pending',      -- pending, completed, failed
    ErrorLog TEXT,                      -- JSON array of errors
    StartedAt TEXT NOT NULL DEFAULT (datetime('now')),
    CompletedAt TEXT,
    
    -- Configuration used
    ImportSettings TEXT                 -- JSON of import settings
);
```

---

## Indexes for Performance

### Primary Indexes

```sql
-- Vehicle search indexes
CREATE INDEX idx_vehicles_model_name ON Vehicles(ModelName);
CREATE INDEX idx_vehicles_display_name ON Vehicles(DisplayName);
CREATE INDEX idx_vehicles_manufacturer ON Vehicles(ManufacturerId);
CREATE INDEX idx_vehicles_class ON Vehicles(ClassId);
CREATE INDEX idx_vehicles_source_dlc ON Vehicles(SourceDlc);
CREATE INDEX idx_vehicles_is_custom ON Vehicles(IsCustom);

-- Performance-based searches
CREATE INDEX idx_vehicles_top_speed ON Vehicles(TopSpeed);
CREATE INDEX idx_vehicles_acceleration ON Vehicles(Acceleration);

-- Date-based searches
CREATE INDEX idx_vehicles_imported_at ON Vehicles(ImportedAt);
CREATE INDEX idx_vehicles_created_at ON Vehicles(CreatedAt);

-- Carcol kit indexes
CREATE INDEX idx_carcol_kits_managed_id ON CarcolKits(ManagedId);
CREATE INDEX idx_carcol_kits_vehicle ON CarcolKits(VehicleModelName);
CREATE INDEX idx_carcol_kits_game_id ON CarcolKits(GameId);
CREATE INDEX idx_carcol_kits_empty ON CarcolKits(IsEmpty);

-- Tag searches
CREATE INDEX idx_vehicle_tags_tag ON VehicleTags(Tag);
CREATE INDEX idx_vehicle_tags_vehicle ON VehicleTags(VehicleId);

-- RPF mapping indexes
CREATE INDEX idx_rpf_mapping_vehicle ON VehicleRpfMapping(VehicleId);
CREATE INDEX idx_rpf_mapping_rpf ON VehicleRpfMapping(RpfFileId);

-- ID allocation indexes
CREATE INDEX idx_carcol_allocation_reserved ON CarcolIdAllocations(IsReserved);
CREATE INDEX idx_carcol_allocation_allocated_to ON CarcolIdAllocations(AllocatedTo);
```

### Composite Indexes

```sql
-- Multi-column searches
CREATE INDEX idx_vehicles_class_manufacturer ON Vehicles(ClassId, ManufacturerId);
CREATE INDEX idx_vehicles_custom_class ON Vehicles(IsCustom, ClassId);
CREATE INDEX idx_vehicles_manufacturer_name ON Vehicles(ManufacturerId, DisplayName);

-- Performance filtering
CREATE INDEX idx_vehicles_speed_accel ON Vehicles(TopSpeed, Acceleration);
CREATE INDEX idx_vehicles_class_speed ON Vehicles(ClassId, TopSpeed);

-- Kit management
CREATE INDEX idx_kits_vehicle_empty ON CarcolKits(VehicleModelName, IsEmpty);
```

---

## Views for Common Queries

### VehicleDetails
Complete vehicle information with relationships.

```sql
CREATE VIEW VehicleDetails AS
SELECT 
    v.Id,
    v.ModelName,
    v.DisplayName,
    v.HandlingId,
    v.AudioHash,
    v.Layout,
    v.Flags,
    v.TopSpeed,
    v.Acceleration,
    v.Braking,
    v.Traction,
    v.SourceDlc,
    v.EstimatedSize,
    v.IsCustom,
    v.ImportedAt,
    v.CreatedAt,
    v.UpdatedAt,
    
    -- Manufacturer info
    m.Name as ManufacturerName,
    m.DisplayName as ManufacturerDisplayName,
    
    -- Class info
    c.Name as ClassName,
    c.Description as ClassDescription,
    
    -- Kit count
    (SELECT COUNT(*) FROM CarcolKits ck WHERE ck.VehicleModelName = v.ModelName) as KitCount,
    
    -- RPF info
    (SELECT COUNT(*) FROM VehicleRpfMapping vrm WHERE vrm.VehicleId = v.Id) as RpfCount,
    
    -- Tag list
    (SELECT GROUP_CONCAT(vt.Tag, ', ') FROM VehicleTags vt WHERE vt.VehicleId = v.Id) as Tags

FROM Vehicles v
LEFT JOIN Manufacturers m ON v.ManufacturerId = m.Id
LEFT JOIN VehicleClasses c ON v.ClassId = c.Id;
```

### VehicleStats
Statistical summary for dashboard.

```sql
CREATE VIEW VehicleStats AS
SELECT 
    (SELECT COUNT(*) FROM Vehicles) as TotalVehicles,
    (SELECT COUNT(*) FROM Vehicles WHERE IsCustom = 1) as CustomVehicles,
    (SELECT COUNT(*) FROM Vehicles WHERE IsCustom = 0) as VanillaVehicles,
    (SELECT COUNT(DISTINCT ManufacturerId) FROM Vehicles) as TotalManufacturers,
    (SELECT COUNT(DISTINCT ClassId) FROM Vehicles) as TotalClasses,
    (SELECT COUNT(*) FROM CarcolKits) as TotalKits,
    (SELECT COUNT(*) FROM CarcolKits WHERE IsEmpty = 0) as ActiveKits,
    (SELECT SUM(EstimatedSize) FROM Vehicles) as TotalEstimatedSize,
    (SELECT COUNT(*) FROM RpfFiles) as TotalRpfFiles,
    (SELECT MAX(ManagedId) FROM CarcolKits) as HighestCarcolId;
```

### CarcolIdStatus
ID allocation overview.

```sql
CREATE VIEW CarcolIdStatus AS
SELECT 
    (SELECT MAX(Id) FROM CarcolIdAllocations) as HighestId,
    (SELECT COUNT(*) FROM CarcolIdAllocations WHERE IsReserved = 1) as ReservedIds,
    (SELECT COUNT(*) FROM CarcolIdAllocations WHERE IsReserved = 0) as AllocatedIds,
    (SELECT MIN(Id) FROM CarcolIdAllocations WHERE IsReserved = 0 AND Id > 999) as FirstCustomId,
    (SELECT COUNT(*) FROM CarcolIdAllocations WHERE Id BETWEEN 0 AND 999) as VanillaIds;
```

---

## Triggers for Data Integrity

### Update Timestamps

```sql
-- Update timestamps on vehicle changes
CREATE TRIGGER update_vehicle_timestamp 
AFTER UPDATE ON Vehicles
FOR EACH ROW
BEGIN
    UPDATE Vehicles SET UpdatedAt = datetime('now') WHERE Id = NEW.Id;
END;

-- Update timestamps on kit changes
CREATE TRIGGER update_kit_timestamp 
AFTER UPDATE ON CarcolKits
FOR EACH ROW
BEGIN
    UPDATE CarcolKits SET UpdatedAt = datetime('now') WHERE Id = NEW.Id;
END;
```

### Maintain ID Allocations

```sql
-- Auto-create allocation record when kit is created
CREATE TRIGGER create_id_allocation 
AFTER INSERT ON CarcolKits
FOR EACH ROW
BEGIN
    INSERT OR REPLACE INTO CarcolIdAllocations 
    (Id, IsReserved, AllocatedTo, AllocatedAt)
    VALUES 
    (NEW.ManagedId, 0, NEW.Name, datetime('now'));
END;

-- Clean up allocation when kit is deleted
CREATE TRIGGER cleanup_id_allocation 
AFTER DELETE ON CarcolKits
FOR EACH ROW
BEGIN
    DELETE FROM CarcolIdAllocations 
    WHERE Id = OLD.ManagedId AND AllocatedTo = OLD.Name;
END;
```

---

## Sample Queries

### Common Searches

```sql
-- Find all sports cars by Pegassi
SELECT * FROM VehicleDetails 
WHERE ManufacturerName = 'Pegassi' AND ClassName = 'Sports'
ORDER BY DisplayName;

-- Find fastest vehicles
SELECT DisplayName, ManufacturerName, TopSpeed
FROM VehicleDetails 
WHERE TopSpeed IS NOT NULL
ORDER BY TopSpeed DESC
LIMIT 10;

-- Find vehicles with tuning kits
SELECT v.DisplayName, COUNT(ck.Id) as KitCount
FROM Vehicles v
JOIN CarcolKits ck ON v.ModelName = ck.VehicleModelName
WHERE ck.IsEmpty = 0
GROUP BY v.Id
ORDER BY KitCount DESC;

-- Find next available carcol ID
SELECT MIN(Id + 1) as NextId
FROM CarcolIdAllocations ca1
WHERE NOT EXISTS (
    SELECT 1 FROM CarcolIdAllocations ca2 
    WHERE ca2.Id = ca1.Id + 1
) AND Id >= 1000; -- Start from 1000 to avoid vanilla IDs
```

### Performance Monitoring

```sql
-- Check database size
SELECT 
    page_count * page_size as database_size_bytes,
    (page_count * page_size) / (1024 * 1024) as database_size_mb
FROM pragma_page_count(), pragma_page_size();

-- Check table sizes
SELECT 
    name,
    COUNT(*) as row_count
FROM sqlite_master sm
JOIN (
    SELECT 'Vehicles' as name, COUNT(*) as count FROM Vehicles UNION ALL
    SELECT 'CarcolKits', COUNT(*) FROM CarcolKits UNION ALL
    SELECT 'VehicleTags', COUNT(*) FROM VehicleTags UNION ALL
    SELECT 'RpfFiles', COUNT(*) FROM RpfFiles
) tc ON sm.name = tc.name
WHERE sm.type = 'table'
ORDER BY tc.count DESC;

-- Analyze query performance
EXPLAIN QUERY PLAN 
SELECT * FROM VehicleDetails 
WHERE ManufacturerName = 'Pegassi' 
AND TopSpeed > 200;
```

---

## Migration Scripts

### Initial Setup

```sql
-- Migration 001: Initial schema
PRAGMA foreign_keys = ON;

-- Create all tables, indexes, views, and triggers
-- (Include all the CREATE statements above)

-- Insert default data
INSERT INTO Manufacturers VALUES 
('rockstar', 'Rockstar', 'Rockstar Games', 'Game developer', datetime('now'), datetime('now')),
('unknown', 'Unknown', 'Unknown Manufacturer', 'Unknown or custom manufacturer', datetime('now'), datetime('now'));

INSERT INTO VehicleClasses VALUES 
('unknown', 'Unknown', 'Unknown or custom class', 999, datetime('now'), datetime('now'));

-- Log migration
CREATE TABLE IF NOT EXISTS schema_migrations (
    version INTEGER PRIMARY KEY,
    applied_at TEXT NOT NULL DEFAULT (datetime('now'))
);

INSERT INTO schema_migrations VALUES (1, datetime('now'));
```

### Version Control

```sql
-- Check current schema version
SELECT MAX(version) FROM schema_migrations;

-- Apply migration
INSERT INTO schema_migrations VALUES (?, datetime('now'));
```

---

## Backup and Maintenance

### Backup Strategy

```sql
-- Full database backup
.backup vehicles_backup.db

-- Export to SQL
.dump > vehicles_backup.sql

-- Vacuum to optimize
VACUUM;

-- Analyze for query optimization
ANALYZE;
```

### Maintenance Queries

```sql
-- Check referential integrity
PRAGMA foreign_key_check;

-- Check database integrity
PRAGMA integrity_check;

-- Get database statistics
PRAGMA stats;
```

---

This database design provides a solid foundation for managing 2000+ vehicles with excellent performance, data integrity, and scalability. The schema supports all the required features while maintaining flexibility for future enhancements.