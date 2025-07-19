using CodeWalker.GameFiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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
        /// Converts an uncompressed YFT (memory dump) to various output formats
        /// </summary>
        /// <param name="inputPath">Path to the input uncompressed YFT file</param>
        /// <param name="outputPath">Path to the output file</param>
        /// <param name="outputFormat">Output format: Gen8YFT, Gen9YFT, or XML</param>
        public void ConvertUncompressedYFT(string inputPath, string outputPath, OutputFormat outputFormat)
        {
            if (!File.Exists(inputPath))
            {
                throw new FileNotFoundException($"Input YFT file not found: {inputPath}");
            }

            if (verbose)
            {
                Console.WriteLine($"Loading uncompressed YFT: {inputPath}");
            }

            // Validate file size
            var fileInfo = new FileInfo(inputPath);
            if (fileInfo.Length == 0)
            {
                throw new Exception("Input file is empty");
            }
            
            if (fileInfo.Length > 500 * 1024 * 1024) // 500MB limit
            {
                throw new Exception($"Input file is too large ({fileInfo.Length:N0} bytes). Maximum supported size is 500MB.");
            }

            // Load the YFT data
            byte[] memoryDump;
            try
            {
                memoryDump = File.ReadAllBytes(inputPath);
            }
            catch (IOException ioEx)
            {
                throw new Exception($"Failed to read input file: {ioEx.Message}", ioEx);
            }
            
            Console.WriteLine($"File: {Path.GetFileName(inputPath)}");
            Console.WriteLine($"Size: {memoryDump.Length:N0} bytes ({memoryDump.Length / 1024.0 / 1024.0:F2} MB)");
            
            // Validate file content
            if (memoryDump.Length < 16)
            {
                throw new Exception("Input file is too small to be a valid YFT file (minimum 16 bytes required)");
            }
            
            // Check file header
            string header = System.Text.Encoding.ASCII.GetString(memoryDump, 0, 4);
            bool isUncompressed = (header == "FRAG");
            bool isCompressed = (header == "RSC7");
            
            if (!isUncompressed && !isCompressed)
            {
                // Show diagnostic info for unknown format
                Console.WriteLine("WARNING: File doesn't start with expected header (FRAG or RSC7)");
                Console.WriteLine($"Header found: {header} (0x{BitConverter.ToUInt32(memoryDump, 0):X8})");
                
                if (verbose)
                {
                    Console.WriteLine("\nFirst 256 bytes of file (hex):");
                    for (int i = 0; i < Math.Min(256, memoryDump.Length); i += 16)
                    {
                        Console.Write($"  0x{i:X4}: ");
                        for (int j = 0; j < 16 && i + j < memoryDump.Length; j++)
                        {
                            Console.Write($"{memoryDump[i + j]:X2} ");
                        }
                        Console.WriteLine();
                    }
                }
            }
            
            if (isCompressed)
            {
                throw new Exception("Input file appears to be a compressed YFT (RSC7 header). This converter is for uncompressed YFT files.");
            }
            
            if (!isUncompressed)
            {
                throw new Exception($"Input file is not a valid uncompressed YFT. Expected 'FRAG' header but found '{header}'.");
            }
            
            // Additional validation for FRAG files
            if (verbose)
            {
                Console.WriteLine("\nFile structure analysis:");
                Console.WriteLine($"  Header: {header}");
                
                // Show first few pointers/values for debugging
                Console.WriteLine("\n  First 10 uint64 values:");
                for (int i = 0; i < Math.Min(10 * 8, memoryDump.Length - 8); i += 8)
                {
                    ulong val = BitConverter.ToUInt64(memoryDump, i);
                    string ptrInfo = "";
                    if (IsMemoryPointer(val))
                    {
                        ptrInfo = " [Pointer]";
                    }
                    Console.WriteLine($"    Offset 0x{i:X4}: 0x{val:X16}{ptrInfo}");
                }
            }
            
            if (verbose)
            {
                Console.WriteLine("Detected uncompressed YFT memory dump.");
                Console.WriteLine("Analyzing memory layout and converting pointers...");
            }
            
            try
            {
                // Route to appropriate conversion method based on output format
                switch (outputFormat)
                {
                    case OutputFormat.XML:
                        ConvertToXml(memoryDump, outputPath);
                        break;
                    case OutputFormat.Gen8YFT:
                        ConvertToCompressedYft(memoryDump, outputPath, false);
                        break;
                    case OutputFormat.Gen9YFT:
                        ConvertToCompressedYft(memoryDump, outputPath, true);
                        break;
                    default:
                        throw new ArgumentException($"Unknown output format: {outputFormat}");
                }
            }
            catch (Exception ex)
            {
                if (verbose)
                {
                    Console.WriteLine($"Failed to convert using native YFT: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                }
                
                // Provide more helpful error messages
                string errorMessage = ex.Message;
                
                if (ex.InnerException is InvalidDataException)
                {
                    errorMessage = "The file contains invalid or corrupted data that cannot be processed. " +
                                 "This often happens with incomplete memory dumps or damaged files.";
                }
                else if (ex.Message.IndexOf("out of range", StringComparison.OrdinalIgnoreCase) >= 0 ||
                         ex.Message.IndexOf("index", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    errorMessage = "The file structure appears to be incomplete or truncated. " +
                                 "Ensure the entire YFT memory dump was extracted correctly.";
                }
                
                if (verbose)
                {
                    Console.WriteLine($"\nERROR: {errorMessage}");
                    Console.WriteLine($"\nOriginal error: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    }
                    Console.WriteLine($"\nStack trace:\n{ex.StackTrace}");
                }
                
                throw new Exception(errorMessage, ex);
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
        /// Convert to compressed YFT format (Gen8 or Gen9)
        /// </summary>
        private void ConvertToCompressedYft(byte[] memoryDump, string outputPath, bool useGen9)
        {
            try
            {
                if (verbose)
                {
                    Console.WriteLine($"Converting to {(useGen9 ? "Gen9" : "Gen8")} compressed YFT format...");
                }

                // Load the YFT from memory dump
                var yft = LoadUncompressedYft(memoryDump, useGen9);
                
                if (yft?.Fragment == null)
                {
                    throw new Exception("Failed to load Fragment from memory dump");
                }
                
                if (verbose)
                {
                    Console.WriteLine("Successfully loaded YFT from memory dump.");
                    Console.WriteLine($"Has Drawable: {yft.Fragment.Drawable != null}");
                    Console.WriteLine($"Has Physics: {yft.Fragment.PhysicsLODGroup != null}");
                    if (yft.Fragment.BoundingSphereRadius > 0)
                    {
                        Console.WriteLine($"Bounding sphere: Center=({yft.Fragment.BoundingSphereCenter.X:F6}, {yft.Fragment.BoundingSphereCenter.Y:F6}, {yft.Fragment.BoundingSphereCenter.Z:F6}), Radius={yft.Fragment.BoundingSphereRadius:F6}");
                    }
                }
                
                // Save the YFT file - this will handle all the compression and header creation
                RpfManager.IsGen9 = useGen9;
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
            catch (Exception ex)
            {
                throw new Exception($"Failed to convert to compressed YFT: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Convert to XML format with better error handling
        /// </summary>
        private void ConvertToXml(byte[] memoryDump, string outputPath)
        {
            try
            {
                if (verbose)
                {
                    Console.WriteLine("Converting to XML format...");
                }

                // Try to load the YFT structure from memory dump
                var yft = LoadUncompressedYft(memoryDump, false); // Use Gen8 for XML
                
                if (yft?.Fragment == null)
                {
                    throw new Exception("Failed to load Fragment data from YFT");
                }

                // Extract resource folder from output path if needed
                var resourceFolder = Path.Combine(Path.GetDirectoryName(outputPath), Path.GetFileNameWithoutExtension(outputPath) + "_resources");
                
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
                    if (Directory.Exists(resourceFolder) && Directory.GetFiles(resourceFolder).Length > 0)
                    {
                        Console.WriteLine($"Resources extracted to: {resourceFolder}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to convert to XML: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Load uncompressed YFT from memory dump with robust error handling
        /// </summary>
        private YftFile LoadUncompressedYft(byte[] memoryDump, bool useGen9)
        {
            YftFile yft = null;
            
            try
            {
                // Create a proper resource entry with appropriate flags
                var systemSize = memoryDump.Length;
                // var graphicsSize = 0; // Currently unused
                var version = useGen9 ? 171u : 162u;
                var systemFlags = RpfResourceFileEntry.GetFlagsFromSize((int)systemSize, version);
                var graphicsFlags = 0u; // No graphics data
                
                var resentry = new RpfResourceFileEntry();
                resentry.SystemFlags = new RpfResourcePageFlags(systemFlags);
                resentry.GraphicsFlags = new RpfResourcePageFlags(graphicsFlags);
                
                // Try multiple approaches to load the YFT
                try
                {
                    // Approach 1: Direct load using YftFile
                    yft = new YftFile();
                    yft.Load(memoryDump, resentry);
                    
                    if (yft.Fragment != null)
                    {
                        if (verbose)
                        {
                            Console.WriteLine("Successfully loaded YFT using direct load method.");
                        }
                        return yft;
                    }
                }
                catch (Exception ex1)
                {
                    if (verbose)
                    {
                        Console.WriteLine($"Direct load attempt failed: {ex1.Message}");
                        
                        // Check for common issues
                        if (ex1.Message.IndexOf("DeflateStream", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            ex1.InnerException is InvalidDataException)
                        {
                            Console.WriteLine("  Note: This appears to be a decompression error. The data might already be decompressed.");
                        }
                    }
                }
                
                // Approach 2: Try using ResourceDataReader directly
                try
                {
                    var reader = new ResourceDataReader(resentry, memoryDump, Endianess.LittleEndian);
                    reader.Position = 0x50000000; // Set to system memory base
                    
                    var fragment = reader.ReadBlock<FragType>();
                    if (fragment != null)
                    {
                        yft = new YftFile();
                        yft.Fragment = fragment;
                        fragment.Yft = yft;
                        
                        if (fragment.Drawable != null)
                        {
                            fragment.Drawable.Owner = yft;
                        }
                        if (fragment.DrawableCloth != null)
                        {
                            fragment.DrawableCloth.Owner = yft;
                        }
                        
                        return yft;
                    }
                }
                catch (Exception ex2)
                {
                    if (verbose)
                    {
                        Console.WriteLine($"ResourceDataReader approach failed: {ex2.Message}");
                    }
                }
                
                // If all approaches fail
                throw new Exception("Unable to load YFT data from memory dump using any available method");
            }
            catch (Exception ex)
            {
                // Check if it's a DeflateStream error
                if (ex.InnerException is InvalidDataException || 
                    ex.Message.IndexOf("invalid data", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    ex.Message.IndexOf("corrupt", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    throw new Exception("The YFT file appears to be corrupted or contains invalid compressed data. " +
                                      "This often happens when the file structure is damaged or incompletely extracted.", ex);
                }
                throw;
            }
        }

    }

    public enum OutputFormat
    {
        Gen8YFT,
        Gen9YFT,
        XML
    }
}