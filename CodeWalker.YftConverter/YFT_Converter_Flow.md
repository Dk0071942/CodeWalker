# YFT Converter Flow Diagram

## Overview
This document contains Mermaid diagrams that visualize the YFT converter's operation, making it easier to understand the conversion process, memory layout, and validation logic.

## 1. High-Level Conversion Flow

```mermaid
flowchart TD
    Start([YFT Converter Start]) --> Input{Input File}
    Input --> CheckFormat{Check Format}
    
    CheckFormat -->|Starts with 'FRAG'| Uncompressed[Uncompressed YFT<br/>Memory Dump]
    CheckFormat -->|Starts with 'RSC7'| AlreadyCompressed[Already Compressed]
    CheckFormat -->|No header| CheckPatterns{Check String<br/>Patterns}
    
    CheckPatterns -->|Found patterns| Uncompressed
    CheckPatterns -->|No patterns| InvalidFile[Invalid File]
    
    AlreadyCompressed --> End([End - No Conversion Needed])
    InvalidFile --> End
    
    Uncompressed --> OutputFormat{Output Format?}
    
    OutputFormat -->|YFT| CompressedConversion[Compressed YFT<br/>Conversion]
    OutputFormat -->|XML| XMLConversion[XML Conversion]
    
    CompressedConversion --> ThreeTier[Three-Tier<br/>Conversion Process]
    XMLConversion --> LoadForXML[Load YFT Structure<br/>for XML]
    
    ThreeTier --> Success{Success?}
    Success -->|Yes| WriteCompressed[Write Compressed<br/>YFT File]
    Success -->|No| NextMethod[Try Next Method]
    
    LoadForXML --> GenerateXML[Generate XML<br/>with YftXml.GetXml()]
    GenerateXML --> WriteXML[Write XML File]
    
    WriteCompressed --> End
    WriteXML --> End
    NextMethod --> End
    
    style Uncompressed fill:#e1f5e1
    style CompressedConversion fill:#ffe1e1
    style XMLConversion fill:#e1e1ff
```

## 2. Three-Tier Conversion Process

```mermaid
flowchart TD
    Start([Three-Tier Process]) --> Method1[Method 1: Native YFT Loading]
    
    Method1 --> M1Load[Load using CodeWalker<br/>Native YFT Handling]
    M1Load --> M1Parse[Parse FragType Structure]
    M1Parse --> M1Success{Success?}
    
    M1Success -->|Yes| M1Save[Save Compressed YFT]
    M1Success -->|No| Method2[Method 2: Direct Memory<br/>Conversion]
    
    Method2 --> M2Scan[Scan for Pointers]
    M2Scan --> M2Convert[Convert Pointers In-Place]
    M2Convert --> M2Parse[Create ResourceDataReader<br/>Parse FragType]
    M2Parse --> M2Success{Success?}
    
    M2Success -->|Yes| M2Save[Save Compressed YFT]
    M2Success -->|No| Method3[Method 3: Simple<br/>Compression Fallback]
    
    Method3 --> M3Compress[DEFLATE Compress<br/>Entire Memory Dump]
    M3Compress --> M3Header[Create RSC7 Header]
    M3Header --> M3Save[Save as System Data]
    
    M1Save --> End([Conversion Complete])
    M2Save --> End
    M3Save --> End
    
    style Method1 fill:#d4edda
    style Method2 fill:#fff3cd
    style Method3 fill:#f8d7da
```

## 3. Memory Layout and Pointer System

```mermaid
graph TD
    subgraph "Memory Regions"
        SysMem[System Memory<br/>0x50000000 - 0x5FFFFFFF]
        GfxMem[Graphics Memory<br/>0x60000000 - 0x6FFFFFFF]
    end
    
    subgraph "Memory Dump Structure"
        Header[FRAG Header<br/>4 bytes: 0x47415246]
        FragType[FragType Structure<br/>~304 bytes]
        SysData[System Data<br/>Variable size]
        GfxData[Graphics Data<br/>Variable size]
    end
    
    subgraph "Pointer Detection"
        Scan[Scan 64-bit values]
        Check{In valid range?}
        SysPtr[System Pointer<br/>0x50xxxxxx]
        GfxPtr[Graphics Pointer<br/>0x60xxxxxx]
        Regular[Regular Data]
    end
    
    Header --> FragType
    FragType --> SysData
    SysData --> GfxData
    
    Scan --> Check
    Check -->|0x50000000-0x5FFFFFFF| SysPtr
    Check -->|0x60000000-0x6FFFFFFF| GfxPtr
    Check -->|Outside ranges| Regular
    
    SysPtr --> SysMem
    GfxPtr --> GfxMem
    
    style Header fill:#ffebcd
    style FragType fill:#e6f3ff
    style SysMem fill:#e1f5e1
    style GfxMem fill:#ffe1e1
```

## 4. Pointer Conversion Process

```mermaid
flowchart LR
    subgraph "Original Pointer"
        OldPtr[0x50001234<br/>Absolute Address]
    end
    
    subgraph "Conversion Steps"
        Extract[Extract Offset<br/>Using 0x7FFFFFFF mask]
        Determine[Determine Target<br/>Memory Region]
        Calculate[Calculate New<br/>Relative Offset]
        Create[Create New Pointer<br/>Base + Offset]
    end
    
    subgraph "New Pointer"
        NewPtr[0x50000100<br/>Relative Offset]
    end
    
    OldPtr --> Extract
    Extract --> Determine
    Determine --> Calculate
    Calculate --> Create
    Create --> NewPtr
```

## 5. Validation and Error Handling

```mermaid
flowchart TD
    Start([Validation Start]) --> HeaderCheck{FRAG Header?}
    
    HeaderCheck -->|Yes| SizeCheck{File Size Valid?}
    HeaderCheck -->|No| Error1[Error: Not uncompressed YFT]
    
    SizeCheck -->|Yes| PointerScan[Scan for Pointers]
    SizeCheck -->|No| Error2[Error: Invalid file size]
    
    PointerScan --> ValidPointers{Valid Pointers<br/>Found?}
    
    ValidPointers -->|Yes| RegionAnalysis[Analyze Memory<br/>Regions]
    ValidPointers -->|No| Warning1[Warning: No pointers<br/>Try fallback]
    
    RegionAnalysis --> EstimateSize[Estimate Region<br/>Sizes]
    
    EstimateSize --> Alignment[Check 16-byte<br/>Alignment]
    
    Alignment --> FragTypeCheck{Valid FragType<br/>Structure?}
    
    FragTypeCheck -->|Yes| Success[Validation Success]
    FragTypeCheck -->|No| Error3[Error: Invalid FragType]
    
    Error1 --> HandleError[Error Handler]
    Error2 --> HandleError
    Error3 --> HandleError
    Warning1 --> FallbackMethod[Try Fallback<br/>Method]
    
    Success --> Continue([Continue Conversion])
    FallbackMethod --> Continue
    HandleError --> Abort([Abort Conversion])
    
    style Success fill:#d4edda
    style Error1 fill:#f8d7da
    style Error2 fill:#f8d7da
    style Error3 fill:#f8d7da
    style Warning1 fill:#fff3cd
```

## 6. Compression Process

```mermaid
flowchart TD
    Start([Compression Start]) --> DetermineGen{Generation?}
    
    DetermineGen -->|Gen8| Ver162[Version = 162]
    DetermineGen -->|Gen9| Ver171[Version = 171]
    
    Ver162 --> CalcFlags[Calculate Memory Flags<br/>Based on Data Size]
    Ver171 --> CalcFlags
    
    CalcFlags --> Compress[DEFLATE Compression<br/>Optimal Level]
    
    Compress --> CreateHeader[Create RSC7 Header<br/>16 bytes]
    
    subgraph "RSC7 Header Structure"
        Magic[Bytes 0-3: 'RSC7'<br/>0x37435352]
        Version[Bytes 4-7: Version<br/>162 or 171]
        SysFlags[Bytes 8-11: System Flags]
        GfxFlags[Bytes 12-15: Graphics Flags]
    end
    
    CreateHeader --> Combine[Combine Header +<br/>Compressed Data]
    
    Combine --> Output([Output Compressed YFT])
    
    style Ver162 fill:#e1e1ff
    style Ver171 fill:#ffe1e1
```

## 7. Region Size Estimation Algorithm

```mermaid
flowchart TD
    Start([Estimate Region Size]) --> InitSize[Initialize Size = 64 bytes<br/>Minimum Structure Size]
    
    InitSize --> Loop{Scan Forward<br/>16-byte aligned}
    
    Loop --> CheckZeros[Count Zeros in<br/>Next 64 bytes]
    
    CheckZeros --> ZeroCount{> 48 zeros?}
    
    ZeroCount -->|Yes| Boundary[Found Structure<br/>Boundary]
    ZeroCount -->|No| CheckPointer[Check for New<br/>Pointer Pattern]
    
    CheckPointer --> NewPointer{Found Different<br/>Pointer?}
    
    NewPointer -->|Yes| NewStructure[Found New<br/>Structure]
    NewPointer -->|No| Increment[Increment Offset<br/>by 16 bytes]
    
    Increment --> MaxCheck{Reached 1MB<br/>Limit?}
    
    MaxCheck -->|Yes| MaxSize[Cap at 1MB<br/>Safety Limit]
    MaxCheck -->|No| Loop
    
    Boundary --> ReturnSize[Return Calculated<br/>Size]
    NewStructure --> ReturnSize
    MaxSize --> ReturnSize
    
    ReturnSize --> End([Size Determined])
```

## Understanding the Diagrams

### Key Concepts:

1. **Input Validation**: The converter strictly validates that input files are uncompressed YFT memory dumps starting with "FRAG"

2. **Three-Tier Approach**: The converter tries three different methods in order of preference:
   - Native loading (most reliable)
   - Direct conversion (handles edge cases)
   - Simple compression (last resort)

3. **Memory System**: YFT files use a two-region memory system:
   - System memory (0x50000000 range) for game logic data
   - Graphics memory (0x60000000 range) for rendering data

4. **Pointer Conversion**: The core challenge is converting absolute memory addresses to relative offsets that work in the compressed format

5. **Validation**: Multiple validation steps ensure data integrity throughout the conversion process

6. **Compression**: Final output uses DEFLATE compression with RSC7 header for game compatibility

These diagrams should help visualize the complex conversion process and make it easier to understand how the YFT converter handles different scenarios and edge cases.