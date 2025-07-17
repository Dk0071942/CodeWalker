# YFT Binary Encoding Analysis - Custom Protection Guide

## Binary Structure Overview

### RSC7 Header (16 bytes)
```
Offset  Size  Field           Typical Value    Notes
0x00    4     Magic           0x37435352       "RSC7" - DO NOT MODIFY
0x04    4     Version         162 or 171       Upper bits potentially usable
0x08    4     SystemFlags     Variable         Page allocation info
0x0C    4     GraphicsFlags   Variable         Page allocation info
```

### Post-Header Structure
- Data is DEFLATE compressed after header
- Decompressed data contains FragType structure starting at offset 0

## FragType Binary Layout (Core Structure)

```
Offset  Size  Field                 Usage              Safe to Modify?
0x00    8     VFT                   0x4061D3C8         NO - Virtual table
0x08    8     Unknown_08h           Always 1           NO
0x10    8     Unknown_10h           Always 0           YES - 8 bytes
0x18    8     Unknown_18h           Always 0           YES - 8 bytes
0x20    12    BoundingSphereCenter  Vector3            NO
0x2C    4     BoundingSphereRadius  Float              NO
0x30    8     DrawablePointer       Memory address     NO
0x38    8     DrawableArrayPointer  Memory address     NO
0x40    8     DrawableArrayCount    Array count        NO
0x48    8     Unknown_48h           Unknown ptr        MAYBE
0x50    8     Unknown_50h           Always 0           YES - 8 bytes
0x58    8     NamePointer           String pointer     NO
0x60    8     BoneTransformsPointer Memory address     NO
0x68    8     Unknown_68h           Unknown            MAYBE
0x70    8     Unknown_70h           Always 0           YES - 8 bytes
0x78    8     Unknown_78h           Always 0           YES - 8 bytes
0x80    8     Unknown_80h           Always 0           YES - 8 bytes
0x88    8     Unknown_88h           Always 0           YES - 8 bytes
0x90    8     Unknown_90h           Always 0           YES - 8 bytes
0x98    8     Unknown_98h           Always 0           YES - 8 bytes
0xA0    8     Unknown_A0h           Always 0           YES - 8 bytes
0xA8    8     PhysicsLODPointer     Memory address     NO
0xB0    4     Unknown_B0h           Limited values     PARTIALLY
0xB4    4     Unknown_B4h           0 or 1             PARTIALLY
0xB8    4     Unknown_B8h           Limited range      PARTIALLY
0xBC    4     Unknown_BCh           Limited range      PARTIALLY
0xC0    4     Unknown_C0h           0-2 range          PARTIALLY
0xC4    4     Unknown_C4h           0, 1, or 65281     PARTIALLY
0xC8    4     GravityFactor         Float              NO
0xCC    4     Unknown_CCh           Float              PARTIALLY
0xD0    8     VehicleGlassPointer   Memory address     NO
0xD8    8     Unknown_D8h           Always 0           YES - 8 bytes
0xE0    8     Unknown_E0h           Always 0           YES - 8 bytes
0xE8    8     Unknown_E8h           Always 0           YES - 8 bytes
0xF0    8     Unknown_F0h           Usually 0          YES - 8 bytes
0xF8    4     Unknown_F8h           Float              NO
0xFC    4     Unknown_FCh           Float              NO
0x100   8     Unknown_100h          Always 0           YES - 8 bytes
0x108   8     Unknown_108h          Always 0           YES - 8 bytes
0x110   8     LightAttributesPtr    Memory address     NO
0x118   8     ClothDrawablePtr      Memory address     NO
0x120   8     Unknown_120h          Always 0           YES - 8 bytes
0x128   8     Unknown_128h          Always 0           YES - 8 bytes
```

## Safe Modification Areas

### 1. **Zero-Filled Reserved Fields** (SAFEST)
These fields are always 0 and never referenced:
- `Unknown_10h` (8 bytes at 0x10)
- `Unknown_18h` (8 bytes at 0x18)
- `Unknown_50h` (8 bytes at 0x50)
- `Unknown_70h` to `Unknown_A0h` (56 bytes total)
- `Unknown_D8h` to `Unknown_F0h` (32 bytes)
- `Unknown_100h`, `Unknown_108h` (16 bytes)
- `Unknown_120h`, `Unknown_128h` (16 bytes)

**Total Safe Space: 144 bytes** for custom data

### 2. **Partially Safe Fields**
Fields with limited value ranges that could encode data:
- `Unknown_B0h`: Values 0-32, upper bits potentially usable
- `Unknown_B4h`: Binary flag (0 or 1), other bits available
- `Unknown_C0h`: Values 0-2, upper bits available
- `Unknown_C4h`: Values 0, 1, or 65281 - specific bit patterns

### 3. **Version Field Exploitation**
```csharp
// Version field (4 bytes) at offset 0x04
// Lower 16 bits: actual version (162 or 171)
// Upper 16 bits: potentially usable
uint version = 162 | (customData << 16);
```

### 4. **Padding Exploitation**
Due to 16-byte alignment, padding bytes at block ends can store data:
```csharp
// After writing actual data
int written = stream.Position % 16;
if (written != 0)
{
    int padding = 16 - written;
    // Use padding bytes for custom data
}
```

## Encryption/Protection Strategies

### Strategy 1: Header Checksum Protection
```csharp
public static void AddProtection(YftFile yft)
{
    // Calculate checksum of critical data
    uint checksum = CalculateChecksum(yft.Fragment);
    
    // Store in unused field
    yft.Fragment.Unknown_10h = checksum;
    
    // XOR encrypt other unused fields
    byte[] key = GenerateKey();
    yft.Fragment.Unknown_18h = XorEncrypt(customData, key);
}
```

### Strategy 2: Distributed Data Storage
```csharp
public class ProtectedYft
{
    public static void EmbedLicense(FragType frag, byte[] licenseData)
    {
        // Split license across multiple unused fields
        var chunks = SplitIntoChunks(licenseData, 8);
        
        frag.Unknown_70h = BitConverter.ToUInt64(chunks[0]);
        frag.Unknown_78h = BitConverter.ToUInt64(chunks[1]);
        frag.Unknown_80h = BitConverter.ToUInt64(chunks[2]);
        // Continue for other fields...
    }
}
```

### Strategy 3: Custom Compression Layer
```csharp
public static byte[] SaveProtectedYft(YftFile yft)
{
    // Standard save
    byte[] standardData = yft.Save();
    
    // Add custom header after RSC7
    using (var ms = new MemoryStream())
    {
        // Write RSC7 header
        ms.Write(standardData, 0, 16);
        
        // Insert custom data
        byte[] protection = CreateProtectionData();
        ms.Write(BitConverter.GetBytes(protection.Length), 0, 4);
        ms.Write(protection, 0, protection.Length);
        
        // Write rest of file
        ms.Write(standardData, 16, standardData.Length - 16);
        
        return ms.ToArray();
    }
}
```

### Strategy 4: Pointer Manipulation
```csharp
// Use null pointers as data storage
// If pointer is 0x0000000000000000, it's safe to use
if (frag.Unknown_48h == 0)
{
    // Store 8 bytes of custom data
    frag.Unknown_48h = BitConverter.ToUInt64(customData);
}
```

## Implementation Example

```csharp
public class YftProtection
{
    private const ulong PROTECTION_MAGIC = 0x5950524F54454354; // "YPROTECT"
    
    public static void ProtectYft(YftFile yft, string authorId)
    {
        var frag = yft.Fragment;
        
        // 1. Store magic identifier
        frag.Unknown_10h = PROTECTION_MAGIC;
        
        // 2. Store encrypted author ID
        byte[] authorBytes = Encoding.UTF8.GetBytes(authorId);
        byte[] encrypted = SimpleEncrypt(authorBytes);
        frag.Unknown_18h = BitConverter.ToUInt64(encrypted, 0);
        
        // 3. Store timestamp
        frag.Unknown_70h = (ulong)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        
        // 4. Calculate and store checksum
        ulong checksum = CalculateFragmentChecksum(frag);
        frag.Unknown_78h = checksum;
        
        // 5. Set protection flags in version field
        // Preserve lower 16 bits (version), use upper 16 for flags
        uint version = yft.Version;
        uint protectionFlags = 0xABCD; // Custom protection identifier
        yft.Version = (version & 0xFFFF) | (protectionFlags << 16);
    }
    
    public static bool VerifyProtection(YftFile yft)
    {
        var frag = yft.Fragment;
        
        // Check magic
        if (frag.Unknown_10h != PROTECTION_MAGIC)
            return false;
            
        // Verify checksum
        ulong storedChecksum = frag.Unknown_78h;
        frag.Unknown_78h = 0; // Zero before calculation
        ulong calcChecksum = CalculateFragmentChecksum(frag);
        frag.Unknown_78h = storedChecksum; // Restore
        
        return storedChecksum == calcChecksum;
    }
}
```

## Risks and Limitations

### 1. **Game Updates**
- Future game patches might start using currently unused fields
- Version checks might become stricter

### 2. **Mod Tool Compatibility**
- Other modding tools might overwrite custom data
- Some tools might "clean" unused fields

### 3. **Detection**
- Anti-cheat systems might flag modified files
- Online play could be affected

### 4. **Size Limitations**
- Only ~144 bytes of guaranteed safe space
- Larger data requires more complex strategies

## Best Practices

1. **Always Test Thoroughly**
   - Test on different game versions
   - Verify physics and rendering still work

2. **Document Your Format**
   - Keep track of which fields you use
   - Version your protection scheme

3. **Graceful Degradation**
   - Ensure files still work if protection is removed
   - Don't break core functionality

4. **Avoid Critical Fields**
   - Never modify pointers unless they're null
   - Don't change bounding data or physics values
   - Preserve all non-zero unknown values

## Validation Bypass Considerations

The game's validation is minimal:
1. RSC7 header magic check
2. Version compatibility (only lower 16 bits)
3. Basic structure size validation
4. Pointer dereferencing (only for non-null pointers)

This means:
- Unused fields are not validated
- Zero fields are not checked
- Upper bits of version field are ignored
- Padding bytes are not examined
- Compressed data can contain extra bytes

## Advanced Protection Ideas

1. **Steganography in Texture Data**
   - Hide data in texture mipmaps
   - Use least significant bits

2. **Physics Data Manipulation**
   - Encode data in collision mesh vertices
   - Use floating-point precision limits

3. **String Table Exploitation**
   - Add custom strings to name table
   - Use string padding for data

4. **Dynamic Deobfuscation**
   - Store encrypted model data
   - Decrypt at runtime via injected code

Remember: The goal is protection without breaking functionality. Always maintain compatibility with the game's loading system.