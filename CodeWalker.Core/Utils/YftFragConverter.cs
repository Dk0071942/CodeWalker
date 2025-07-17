using System;
using System.IO;
using System.IO.Compression;
using CodeWalker.GameFiles;

namespace CodeWalker.Utils
{
    /// <summary>
    /// Simple FRAG to RSC7 conversion utility for YFT files
    /// </summary>
    public static class YftFragConverter
    {
        /// <summary>
        /// Detects if a YFT file is in FRAG format (uncompressed)
        /// </summary>
        public static bool IsFragFormat(byte[] data)
        {
            if (data == null || data.Length < 4) return false;
            
            // Check for FRAG magic
            uint magic = BitConverter.ToUInt32(data, 0);
            if (magic == 0x47415246) return true; // "FRAG"
            
            // Check for RSC7 magic
            if (magic == 0x37435352) return false; // "RSC7" - already compressed
            
            // If no recognized header, check if it looks like uncompressed fragment data
            // Look for readable strings that are typical in YFT files
            return ContainsTypicalYftStrings(data);
        }
        
        /// <summary>
        /// Check if data contains typical YFT string patterns
        /// </summary>
        private static bool ContainsTypicalYftStrings(byte[] data)
        {
            if (data.Length < 100) return false;
            
            // Common strings found in YFT files
            string[] patterns = {
                "vehicle_generic",
                "smallspecmap",
                "diffuse",
                "normal",
                "specular",
                ".dds",
                "shader",
                "drawable"
            };
            
            // Convert portion of data to string for pattern matching
            try
            {
                string dataStr = System.Text.Encoding.ASCII.GetString(data, 0, Math.Min(data.Length, 4096));
                foreach (var pattern in patterns)
                {
                    if (dataStr.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0)
                        return true;
                }
            }
            catch { }
            
            return false;
        }
        
        /// <summary>
        /// Convert FRAG format to RSC7 format
        /// </summary>
        public static byte[] ConvertToRsc7(byte[] fragData, bool isGen9 = false)
        {
            if (fragData == null || fragData.Length == 0)
                throw new ArgumentException("Invalid input data");
            
            // Check if already RSC7
            uint magic = BitConverter.ToUInt32(fragData, 0);
            if (magic == 0x37435352) // "RSC7"
            {
                return fragData; // Already in correct format
            }
            
            // Remove FRAG header if present
            byte[] rawData = fragData;
            if (magic == 0x47415246) // "FRAG"
            {
                rawData = new byte[fragData.Length - 4];
                Buffer.BlockCopy(fragData, 4, rawData, 0, rawData.Length);
            }
            
            // Compress the data
            byte[] compressed = CompressData(rawData);
            
            // Determine version
            uint version = isGen9 ? 171u : 162u;
            
            // Calculate flags
            uint systemFlags = RpfResourceFileEntry.GetFlagsFromSize(compressed.Length, 0);
            uint graphicsFlags = RpfResourceFileEntry.GetFlagsFromSize(0, version);
            
            // Build RSC7 file
            using (var ms = new MemoryStream())
            {
                ms.Write(BitConverter.GetBytes(0x37435352), 0, 4); // "RSC7"
                ms.Write(BitConverter.GetBytes((int)version), 0, 4);
                ms.Write(BitConverter.GetBytes(systemFlags), 0, 4);
                ms.Write(BitConverter.GetBytes(graphicsFlags), 0, 4);
                ms.Write(compressed, 0, compressed.Length);
                return ms.ToArray();
            }
        }
        
        /// <summary>
        /// Compress data using DEFLATE
        /// </summary>
        private static byte[] CompressData(byte[] data)
        {
            using (var output = new MemoryStream())
            {
                using (var deflate = new DeflateStream(output, CompressionLevel.Optimal))
                {
                    deflate.Write(data, 0, data.Length);
                }
                return output.ToArray();
            }
        }
        
        /// <summary>
        /// Load a YFT file, automatically handling FRAG format conversion
        /// </summary>
        public static YftFile LoadYft(string filepath)
        {
            byte[] data = File.ReadAllBytes(filepath);
            return LoadYft(data);
        }
        
        /// <summary>
        /// Load a YFT file from byte array, automatically handling FRAG format conversion
        /// </summary>
        public static YftFile LoadYft(byte[] data)
        {
            // Check if conversion is needed
            if (IsFragFormat(data))
            {
                Console.WriteLine("Detected FRAG format, converting to RSC7...");
                data = ConvertToRsc7(data);
            }
            
            // Load normally
            var yft = new YftFile();
            yft.Load(data);
            return yft;
        }
        
        /// <summary>
        /// Quick conversion method for file-to-file
        /// </summary>
        public static void ConvertFile(string inputPath, string outputPath, bool isGen9 = false)
        {
            byte[] input = File.ReadAllBytes(inputPath);
            
            if (!IsFragFormat(input))
            {
                throw new InvalidOperationException("Input file is not in FRAG format");
            }
            
            byte[] output = ConvertToRsc7(input, isGen9);
            File.WriteAllBytes(outputPath, output);
            
            Console.WriteLine($"Converted {inputPath} -> {outputPath}");
            Console.WriteLine($"Size: {input.Length:N0} -> {output.Length:N0} bytes ({(1.0 - (double)output.Length / input.Length) * 100:F1}% compression)");
        }
    }
}