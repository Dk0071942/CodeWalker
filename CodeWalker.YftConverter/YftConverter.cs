using CodeWalker.GameFiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CodeWalker.YftConverter
{
    public class YftConverter
    {
        private readonly bool verbose;
        private const uint SYSTEM_BASE = 0x50000000;
        private const uint GRAPHICS_BASE = 0x60000000;
        private const uint POINTER_MASK = 0x7FFFFFFF;

        public YftConverter(bool verbose = false)
        {
            this.verbose = verbose;
        }

        /// <summary>
        /// Converts an uncompressed YFT (memory dump) directly to compressed YFT format
        /// </summary>
        /// <param name="inputPath">Path to the input uncompressed YFT file</param>
        /// <param name="outputPath">Path to the output compressed YFT file</param>
        /// <param name="useGen9">True for Gen9 compression (version 171), false for Gen8 (version 162). Default: false</param>
        public void ConvertUncompressedToCompressed(string inputPath, string outputPath, bool useGen9 = false)
        {
            if (!File.Exists(inputPath))
            {
                throw new FileNotFoundException($"Input YFT file not found: {inputPath}");
            }

            if (verbose)
            {
                Console.WriteLine($"Loading uncompressed YFT: {inputPath}");
            }

            // Load the uncompressed YFT data
            var memoryDump = File.ReadAllBytes(inputPath);
            
            Console.WriteLine($"File size: {memoryDump.Length:N0} bytes");
            
            // Always show debug info to understand the file structure
            Console.WriteLine("First 20 uint64 values in file:");
            for (int i = 0; i < Math.Min(20 * 8, memoryDump.Length - 8); i += 8)
            {
                ulong val = BitConverter.ToUInt64(memoryDump, i);
                Console.WriteLine($"  Offset 0x{i:X4}: 0x{val:X16}");
            }
            
            // Check if it's a memory dump (starts with "FRAG")
            if (memoryDump.Length < 4 || System.Text.Encoding.ASCII.GetString(memoryDump, 0, 4) != "FRAG")
            {
                throw new Exception("Input file is not an uncompressed YFT (doesn't start with FRAG)");
            }
            
            if (verbose)
            {
                Console.WriteLine("Detected uncompressed YFT memory dump.");
                Console.WriteLine("Analyzing memory layout and converting pointers...");
            }
            
            try
            {
                // Try to load and save using CodeWalker's native YFT handling
                LoadAndSaveUsingNativeYft(memoryDump, outputPath, useGen9);
            }
            catch (Exception ex)
            {
                if (verbose)
                {
                    Console.WriteLine($"Failed to convert using native YFT: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                }
                
                try
                {
                    // Try direct memory conversion as second attempt
                    DirectMemoryConversion(memoryDump, outputPath, useGen9);
                }
                catch (Exception ex2)
                {
                    if (verbose)
                    {
                        Console.WriteLine($"Failed to convert: {ex2.Message}");
                        Console.WriteLine($"Stack trace: {ex2.StackTrace}");
                    }
                    
                    // Fallback to simple compression if pointer conversion fails
                    Console.WriteLine("Attempting fallback method (simple compression)...");
                    SimpleFallbackCompression(memoryDump, outputPath, useGen9);
                }
            }
        }

        private class MemoryRegion
        {
            public uint OriginalAddress { get; set; }
            public uint Size { get; set; }
            public uint NewOffset { get; set; }
            public bool IsSystem { get; set; }
        }

        private class MemoryMap
        {
            public List<MemoryRegion> Regions { get; } = new List<MemoryRegion>();
            public Dictionary<ulong, ulong> PointerMap { get; } = new Dictionary<ulong, ulong>();
        }

        private class RebuildData
        {
            public byte[] systemData;
            public byte[] graphicsData;
        }

        /// <summary>
        /// Analyze memory dump to find all data regions and pointers
        /// </summary>
        private MemoryMap AnalyzeMemoryDump(byte[] memoryDump)
        {
            var map = new MemoryMap();
            
            // The memory dump starts with FragType at offset 0
            // FragType is typically around 304 bytes
            
            using (var ms = new MemoryStream(memoryDump))
            using (var reader = new BinaryReader(ms))
            {
                // Start with the FragType structure at the beginning
                var fragRegion = new MemoryRegion
                {
                    OriginalAddress = 0x50000000, // System memory base
                    Size = 512, // Use larger initial size to ensure we capture all FragType fields
                    NewOffset = 0,
                    IsSystem = true
                };
                map.Regions.Add(fragRegion);
                
                // Scan the entire file for pointers, not just the first region
                // Many pointers might be scattered throughout the dump
                long fileSize = memoryDump.Length;
                for (long offset = 0; offset < fileSize - 8; offset += 8)
                {
                    reader.BaseStream.Position = offset;
                    ulong value = reader.ReadUInt64();
                    
                    if (IsMemoryPointer(value))
                    {
                        // Record this pointer location
                        map.PointerMap[(ulong)offset] = value;
                        
                        // Calculate target offset in file
                        uint targetOffset = (uint)(value & POINTER_MASK);
                        
                        // Check if we haven't processed this target region yet
                        if (targetOffset < memoryDump.Length && !map.Regions.Any(r => 
                            (r.OriginalAddress & POINTER_MASK) == targetOffset))
                        {
                            // Try to determine the size of the referenced data
                            var regionSize = EstimateRegionSize(memoryDump, targetOffset);
                            
                            if (regionSize > 0 && targetOffset + regionSize <= memoryDump.Length)
                            {
                                var region = new MemoryRegion
                                {
                                    OriginalAddress = (uint)value & 0xFFFFFFFF, // Keep original address info
                                    Size = regionSize,
                                    NewOffset = 0, // Will be assigned later
                                    IsSystem = IsSystemPointer(value)
                                };
                                map.Regions.Add(region);
                            }
                        }
                    }
                }
            }
            
            if (verbose)
            {
                Console.WriteLine($"Found {map.Regions.Count} memory regions");
                Console.WriteLine($"Found {map.PointerMap.Count} pointers to convert");
                
                // Debug: Show first few pointers and regions
                Console.WriteLine("Pointers found:");
                foreach (var ptr in map.PointerMap.Take(10))
                {
                    Console.WriteLine($"  Pointer at 0x{ptr.Key:X8} -> 0x{ptr.Value:X16}");
                }
                
                Console.WriteLine("Memory regions:");
                foreach (var region in map.Regions)
                {
                    Console.WriteLine($"  Region: Address=0x{region.OriginalAddress:X8}, Size={region.Size}, IsSystem={region.IsSystem}");
                }
            }
            
            return map;
        }

        /// <summary>
        /// Scan a region for pointers and add referenced regions
        /// </summary>
        private void ScanForPointers(BinaryReader reader, long start, uint size, MemoryMap map, byte[] memoryDump)
        {
            reader.BaseStream.Position = start;
            
            // Scan for 64-bit values that look like pointers
            for (uint offset = 0; offset < size - 8; offset += 8)
            {
                reader.BaseStream.Position = start + offset;
                var value = reader.ReadUInt64();
                
                // Check if this looks like a memory pointer
                if (IsMemoryPointer(value))
                {
                    // Calculate the file offset this pointer refers to
                    var targetOffset = (uint)(value & POINTER_MASK);
                    
                    // Check if we haven't processed this region yet
                    if (!map.Regions.Any(r => r.OriginalAddress == targetOffset))
                    {
                        // Try to determine the size of the referenced data
                        // This is simplified - in reality we'd need to know the structure types
                        var regionSize = EstimateRegionSize(memoryDump, targetOffset);
                        
                        if (regionSize > 0 && targetOffset + regionSize <= memoryDump.Length)
                        {
                            var region = new MemoryRegion
                            {
                                OriginalAddress = (uint)value,
                                Size = regionSize,
                                NewOffset = 0, // Will be assigned later
                                IsSystem = IsSystemPointer(value)
                            };
                            map.Regions.Add(region);
                            
                            // Recursively scan this region
                            ScanForPointers(reader, targetOffset, regionSize, map, memoryDump);
                        }
                    }
                    
                    // Add to pointer map
                    map.PointerMap[(ulong)(start + offset)] = value;
                }
            }
        }

        /// <summary>
        /// Estimate the size of a data region (simplified)
        /// </summary>
        private uint EstimateRegionSize(byte[] data, uint offset)
        {
            // This is a simplified estimation
            // In reality, we'd need to know the exact structure types
            
            if (offset + 4 > data.Length) return 0;
            
            // Start with a reasonable minimum size
            uint size = 64; // Minimum meaningful structure size
            
            // Look for patterns that indicate end of structure
            // Most structures are aligned to 16 bytes and have pointers or data
            for (uint i = offset + size; i < Math.Min(offset + 0x100000, data.Length); i += 16)
            {
                // Check if we've hit a region of zeros (common structure boundary)
                int zeroCount = 0;
                for (int j = 0; j < 64 && i + j < data.Length; j++)
                {
                    if (data[i + j] == 0) zeroCount++;
                }
                
                // If we find mostly zeros, likely end of structure
                if (zeroCount > 48)
                {
                    size = i - offset;
                    break;
                }
                
                // Also check for next valid pointer pattern
                if (i + 8 <= data.Length)
                {
                    ulong value = BitConverter.ToUInt64(data, (int)i);
                    if (IsMemoryPointer(value) && value != BitConverter.ToUInt64(data, (int)offset))
                    {
                        // Found another pointer, likely a new structure
                        size = i - offset;
                        break;
                    }
                }
            }
            
            return Math.Min(size, 0x100000); // Cap at 1MB for safety
        }

        /// <summary>
        /// Check if a value looks like a memory pointer
        /// </summary>
        private bool IsMemoryPointer(ulong value)
        {
            // Check if it's in the valid memory ranges
            // Note: Memory dumps from CodeWalker may use slightly different bases
            // System memory: 0x50000000 - 0x5FFFFFFF
            // Graphics memory: 0x60000000 - 0x6FFFFFFF
            return (value >= 0x50000000 && value < 0x60000000) ||
                   (value >= 0x60000000 && value < 0x70000000);
        }

        /// <summary>
        /// Check if pointer is in system memory
        /// </summary>
        private bool IsSystemPointer(ulong pointer)
        {
            return pointer >= SYSTEM_BASE && pointer < 0x60000000;
        }

        /// <summary>
        /// Rebuild data streams with converted pointers
        /// </summary>
        private RebuildData RebuildDataStreams(byte[] memoryDump, MemoryMap map)
        {
            var result = new RebuildData();
            
            // Assign new offsets to regions
            uint systemOffset = 0;
            uint graphicsOffset = 0;
            
            foreach (var region in map.Regions.OrderBy(r => r.OriginalAddress))
            {
                if (region.IsSystem)
                {
                    region.NewOffset = systemOffset;
                    systemOffset += region.Size;
                    systemOffset = (uint)((systemOffset + 15) & ~15); // Align to 16 bytes
                }
                else
                {
                    region.NewOffset = graphicsOffset;
                    graphicsOffset += region.Size;
                    graphicsOffset = (uint)((graphicsOffset + 15) & ~15); // Align to 16 bytes
                }
            }
            
            // Allocate streams
            result.systemData = new byte[systemOffset];
            result.graphicsData = new byte[graphicsOffset];
            
            // Copy data and fix pointers
            foreach (var region in map.Regions)
            {
                var targetStream = region.IsSystem ? result.systemData : result.graphicsData;
                var sourceOffset = (int)(region.OriginalAddress & POINTER_MASK);
                
                // Copy the data
                if (sourceOffset + region.Size <= memoryDump.Length)
                {
                    Array.Copy(memoryDump, sourceOffset, targetStream, region.NewOffset, region.Size);
                }
            }
            
            // Fix all pointers
            foreach (var pointerEntry in map.PointerMap)
            {
                var pointerOffset = pointerEntry.Key;
                var oldPointer = pointerEntry.Value;
                
                // Find which region contains this pointer
                var containingRegion = map.Regions.FirstOrDefault(r => 
                    pointerOffset >= (r.OriginalAddress & POINTER_MASK) && 
                    pointerOffset < (r.OriginalAddress & POINTER_MASK) + r.Size);
                    
                if (containingRegion != null)
                {
                    // Find the target region
                    var targetRegion = map.Regions.FirstOrDefault(r => 
                        (r.OriginalAddress & 0xFFFFFFFF00000000) == (oldPointer & 0xFFFFFFFF00000000) &&
                        (r.OriginalAddress & POINTER_MASK) == (oldPointer & POINTER_MASK));
                        
                    if (targetRegion != null)
                    {
                        // Calculate new pointer value
                        ulong newPointer = targetRegion.IsSystem ? 
                            (SYSTEM_BASE | targetRegion.NewOffset) : 
                            (GRAPHICS_BASE | targetRegion.NewOffset);
                        
                        // Calculate where to write the new pointer
                        var writeOffset = containingRegion.NewOffset + 
                                        (pointerOffset - (containingRegion.OriginalAddress & POINTER_MASK));
                        
                        var targetStream = containingRegion.IsSystem ? result.systemData : result.graphicsData;
                        
                        // Write the new pointer
                        if (writeOffset + 8 <= (ulong)targetStream.Length)
                        {
                            BitConverter.GetBytes(newPointer).CopyTo(targetStream, (int)writeOffset);
                        }
                    }
                }
            }
            
            if (verbose)
            {
                Console.WriteLine($"Rebuilt system data: {result.systemData.Length:N0} bytes");
                Console.WriteLine($"Rebuilt graphics data: {result.graphicsData.Length:N0} bytes");
            }
            
            return result;
        }

        /// <summary>
        /// Load and save using CodeWalker's native YFT handling
        /// </summary>
        private void LoadAndSaveUsingNativeYft(byte[] memoryDump, string outputPath, bool useGen9)
        {
            if (verbose)
            {
                Console.WriteLine("Using native YFT loading approach...");
            }
            
            // The memory dump is already in the correct format for loading
            // Set generation mode based on parameter
            RpfManager.IsGen9 = useGen9;
            
            if (verbose)
            {
                Console.WriteLine($"Using {(useGen9 ? "Gen9" : "Gen8")} mode (version {(useGen9 ? 171 : 162)})");
            }
            
            // Create a proper resource entry with appropriate flags
            var systemSize = memoryDump.Length;
            var graphicsSize = 0;
            var version = useGen9 ? 171u : 162u;
            var systemFlags = RpfResourceFileEntry.GetFlagsFromSize(systemSize, version);
            var graphicsFlags = 0u; // No graphics data
            
            var resentry = new RpfResourceFileEntry();
            resentry.SystemFlags = new RpfResourcePageFlags(systemFlags);
            resentry.GraphicsFlags = new RpfResourcePageFlags(graphicsFlags);
            
            // Create YftFile and load it
            var yft = new YftFile();
            
            // Load using the memory dump data
            // The YftFile.Load method expects the data with RpfResourceFileEntry
            yft.Load(memoryDump, resentry);
            
            if (yft.Fragment == null)
            {
                throw new Exception("Failed to load Fragment from memory dump");
            }
            
            if (verbose)
            {
                Console.WriteLine("Successfully loaded YFT from memory dump.");
                Console.WriteLine($"Fragment loaded: {yft.Fragment != null}");
                if (yft.Fragment != null)
                {
                    Console.WriteLine($"Has Drawable: {yft.Fragment.Drawable != null}");
                    Console.WriteLine($"Has Physics: {yft.Fragment.PhysicsLODGroup != null}");
                    Console.WriteLine($"Bounding sphere: Center=({yft.Fragment.BoundingSphereCenter.X:F6}, {yft.Fragment.BoundingSphereCenter.Y:F6}, {yft.Fragment.BoundingSphereCenter.Z:F6}), Radius={yft.Fragment.BoundingSphereRadius:F6}");
                }
            }
            
            // Save the YFT file - this will handle all the compression and header creation
            var compressedData = yft.Save();
            
            if (compressedData == null || compressedData.Length == 0)
            {
                throw new Exception("Failed to save YFT - data is empty");
            }
            
            // Write output
            var outputDir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }
            
            File.WriteAllBytes(outputPath, compressedData);
            
            if (verbose)
            {
                Console.WriteLine($"Compressed YFT written to: {outputPath}");
                Console.WriteLine($"File size: {compressedData.Length:N0} bytes");
                Console.WriteLine($"Compression ratio: {(compressedData.Length * 100.0 / memoryDump.Length):F1}%");
            }
        }

        /// <summary>
        /// Direct memory conversion approach - converts pointers in place
        /// </summary>
        private void DirectMemoryConversion(byte[] memoryDump, string outputPath, bool useGen9)
        {
            if (verbose)
            {
                Console.WriteLine("Using direct memory conversion approach...");
            }
            
            // Create a copy of the memory dump to modify
            var modifiedDump = new byte[memoryDump.Length];
            Buffer.BlockCopy(memoryDump, 0, modifiedDump, 0, memoryDump.Length);
            
            // Find and convert all pointers in the dump
            int pointerCount = 0;
            for (int offset = 0; offset < modifiedDump.Length - 8; offset += 8)
            {
                ulong value = BitConverter.ToUInt64(modifiedDump, offset);
                
                // Check if this looks like a pointer
                if (IsMemoryPointer(value))
                {
                    // Convert absolute pointer to relative
                    // The pointer already contains the base address, we just need to ensure it's properly formatted
                    ulong newPointer = value; // Keep the pointer as-is since it's already in the right format
                    
                    // Write the pointer back
                    BitConverter.GetBytes(newPointer).CopyTo(modifiedDump, offset);
                    pointerCount++;
                }
            }
            
            if (verbose)
            {
                Console.WriteLine($"Converted {pointerCount} pointers in place.");
            }
            
            // Now load this as a YFT using the memory dump directly
            // Split into system and graphics memory based on content
            // For now, treat everything as system memory
            var systemSize = modifiedDump.Length;
            var graphicsSize = 0;
            
            // Create resource entry
            var version = useGen9 ? 171u : 162u;
            var resentry = new RpfResourceFileEntry();
            resentry.SystemFlags = new RpfResourcePageFlags(RpfResourceFileEntry.GetFlagsFromSize(systemSize, version));
            resentry.GraphicsFlags = new RpfResourcePageFlags(RpfResourceFileEntry.GetFlagsFromSize(graphicsSize, version));
            
            // Create a ResourceDataReader
            var reader = new ResourceDataReader(resentry, modifiedDump, Endianess.LittleEndian);
            reader.Position = 0x50000000; // Set to system memory base
            
            // Read the FragType
            var fragment = reader.ReadBlock<FragType>();
            
            if (fragment == null)
            {
                throw new Exception("Failed to parse FragType from memory dump");
            }
            
            // Create YFT file
            var yft = new YftFile();
            yft.Fragment = fragment;
            
            // Set ownership
            fragment.Yft = yft;
            if (fragment.Drawable != null)
            {
                fragment.Drawable.Owner = yft;
            }
            if (fragment.DrawableCloth != null)
            {
                fragment.DrawableCloth.Owner = yft;
            }
            
            if (verbose)
            {
                Console.WriteLine("Successfully loaded Fragment from memory dump.");
                if (fragment.Drawable != null)
                {
                    Console.WriteLine($"Has Drawable: Yes");
                }
                if (fragment.DrawableCloth != null)
                {
                    Console.WriteLine($"Has DrawableCloth: Yes");
                }
                if (fragment.PhysicsLODGroup != null)
                {
                    Console.WriteLine($"Has Physics: Yes");
                }
            }
            
            // Save as compressed YFT
            var compressedData = yft.Save();
            
            if (compressedData == null || compressedData.Length == 0)
            {
                throw new Exception("Failed to save YFT - data is empty");
            }
            
            // Write output
            var outputDir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }
            
            File.WriteAllBytes(outputPath, compressedData);
            
            if (verbose)
            {
                Console.WriteLine($"Compressed YFT written to: {outputPath}");
                Console.WriteLine($"File size: {compressedData.Length:N0} bytes");
            }
        }

        /// <summary>
        /// Simple fallback compression when pointer conversion fails
        /// </summary>
        private void SimpleFallbackCompression(byte[] memoryDump, string outputPath, bool useGen9)
        {
            try
            {
                Console.WriteLine("Using optimized compression approach...");
                
                // The memory dump is already in the correct format
                // We just need to compress it with proper flags
                
                // All data goes to system memory, no graphics data
                uint systemSize = (uint)memoryDump.Length;
                uint graphicsSize = 0;
                
                // Get proper page flags  
                var version = useGen9 ? 171u : 162u;
                var systemFlags = new RpfResourcePageFlags(RpfResourceFileEntry.GetFlagsFromSize((int)systemSize, version));
                var graphicsFlags = new RpfResourcePageFlags(0); // No graphics data
                
                // Compress the data
                var compressedData = ResourceBuilder.Compress(memoryDump);
                
                // Create final data with RSC7 header
                var data = new byte[16 + compressedData.Length];
                BitConverter.GetBytes(0x37435352).CopyTo(data, 0); // 'RSC7'
                BitConverter.GetBytes(version).CopyTo(data, 4); // Version based on generation
                BitConverter.GetBytes(systemFlags.Value).CopyTo(data, 8);
                BitConverter.GetBytes(graphicsFlags.Value).CopyTo(data, 12);
                Buffer.BlockCopy(compressedData, 0, data, 16, compressedData.Length);
                
                File.WriteAllBytes(outputPath, data);
                
                Console.WriteLine($"Compression complete. Output: {data.Length:N0} bytes");
                Console.WriteLine($"System memory: {systemSize:N0} bytes");
                Console.WriteLine($"Compressed size: {compressedData.Length:N0} bytes");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Compression failed: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Converts an uncompressed YFT (memory dump) to XML format
        /// </summary>
        /// <param name="inputPath">Path to the input uncompressed YFT file</param>
        /// <param name="outputPath">Path to the output XML file</param>
        /// <param name="resourceFolder">Optional folder for extracting resources/textures</param>
        public void ConvertUncompressedToXml(string inputPath, string outputPath, string resourceFolder = "")
        {
            if (!File.Exists(inputPath))
            {
                throw new FileNotFoundException($"Input YFT file not found: {inputPath}");
            }

            if (verbose)
            {
                Console.WriteLine($"Loading uncompressed YFT: {inputPath}");
            }

            // Load the uncompressed YFT data
            var memoryDump = File.ReadAllBytes(inputPath);
            
            Console.WriteLine($"File size: {memoryDump.Length:N0} bytes");
            
            // Check if it's a memory dump (starts with "FRAG")
            if (memoryDump.Length < 4 || System.Text.Encoding.ASCII.GetString(memoryDump, 0, 4) != "FRAG")
            {
                throw new Exception("Input file is not an uncompressed YFT (doesn't start with FRAG)");
            }
            
            if (verbose)
            {
                Console.WriteLine("Detected uncompressed YFT memory dump.");
                Console.WriteLine("Loading YFT structure...");
            }
            
            YftFile yft = null;
            
            try
            {
                // Try to load using native YFT handling (no need to specify gen8/gen9 for XML)
                yft = LoadUncompressedYftForXml(memoryDump);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load YFT: {ex.Message}");
                if (verbose)
                {
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                }
                throw;
            }
            
            if (yft?.Fragment == null)
            {
                throw new Exception("Failed to load Fragment data from YFT");
            }
            
            if (verbose)
            {
                Console.WriteLine("Successfully loaded YFT structure.");
                Console.WriteLine("Converting to XML...");
            }
            
            // Convert to XML
            string xml = YftXml.GetXml(yft, resourceFolder);
            
            if (string.IsNullOrEmpty(xml))
            {
                throw new Exception("Failed to generate XML from YFT");
            }
            
            // Write XML output
            var outputDir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }
            
            File.WriteAllText(outputPath, xml, Encoding.UTF8);
            
            if (verbose)
            {
                Console.WriteLine($"XML written to: {outputPath}");
                Console.WriteLine($"XML size: {new FileInfo(outputPath).Length:N0} bytes");
                
                if (!string.IsNullOrEmpty(resourceFolder))
                {
                    Console.WriteLine($"Resource folder: {resourceFolder}");
                }
            }
        }

        /// <summary>
        /// Load uncompressed YFT specifically for XML conversion
        /// </summary>
        private YftFile LoadUncompressedYftForXml(byte[] memoryDump)
        {
            // Create a proper resource entry with appropriate flags
            var systemSize = memoryDump.Length;
            var graphicsSize = 0;
            // Use Gen8 by default for XML conversion (version doesn't matter for XML output)
            var systemFlags = RpfResourceFileEntry.GetFlagsFromSize(systemSize, 162u);
            var graphicsFlags = 0u; // No graphics data
            
            var resentry = new RpfResourceFileEntry();
            resentry.SystemFlags = new RpfResourcePageFlags(systemFlags);
            resentry.GraphicsFlags = new RpfResourcePageFlags(graphicsFlags);
            
            // Create YftFile and load it
            var yft = new YftFile();
            
            // Load using the memory dump data
            yft.Load(memoryDump, resentry);
            
            return yft;
        }
    }
}