using CodeWalker.GameFiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CodeWalker.DLCMerger
{
    /// <summary>
    /// Handles extraction of model files (.yft, .ytd, .ycd) from RPF files
    /// </summary>
    public class ModelExtractor
    {
        private readonly Action<string> _log;

        public class ModelStats
        {
            public int VehicleModels { get; set; }
            public int WeaponModels { get; set; }
        }

        public ModelExtractor(Action<string> log)
        {
            _log = log;
        }

        public ModelStats ExtractModels(RpfFile rpf, string vehiclesOutputDir, string weaponsOutputDir)
        {
            var stats = new ModelStats();
            
            // Extract models recursively, processing each RPF file directly
            ExtractModelsRecursive(rpf, vehiclesOutputDir, weaponsOutputDir, stats);
            
            return stats;
        }
        
        private void ExtractModelsRecursive(RpfFile rpf, string vehiclesOutputDir, string weaponsOutputDir, ModelStats stats)
        {
            if (rpf.AllEntries == null) return;
            
            foreach (var entry in rpf.AllEntries)
            {
                if (entry is RpfFileEntry fileEntry)
                {
                    var fileName = fileEntry.Name.ToLowerInvariant();
                    
                    // If it's a model file, extract it immediately
                    if (IsModelFile(fileName))
                    {
                        // Simple classification: weapon files go to weapons.rpf, everything else to vehicles.rpf
                        bool isWeaponModel = IsWeaponModel(fileName);
                        
                        try
                        {
                            // Extract the file using raw extraction like RPF Explorer
                            var data = fileEntry.File.ExtractFile(fileEntry);
                            if (data == null) continue;
                            
                            // Add resource header if this is a resource file (like RPF Explorer does)
                            RpfResourceFileEntry rrfe = fileEntry as RpfResourceFileEntry;
                            if (rrfe != null)
                            {
                                // Compress the data first, then add header (matches RPF Explorer behavior)
                                data = ResourceBuilder.Compress(data);
                                data = ResourceBuilder.AddResourceHeader(rrfe, data);
                            }
                            
                            // Determine output directory
                            var outputDir = isWeaponModel ? weaponsOutputDir : vehiclesOutputDir;
                            var outputPath = Path.Combine(outputDir, fileEntry.Name);
                            
                            // Write the file
                            File.WriteAllBytes(outputPath, data);
                            
                            // Update statistics
                            if (isWeaponModel)
                            {
                                stats.WeaponModels++;
                                _log($"    Extracted weapon model: {fileEntry.Name}");
                            }
                            else
                            {
                                stats.VehicleModels++;
                                _log($"    Extracted model: {fileEntry.Name}");
                            }
                        }
                        catch (Exception ex)
                        {
                            _log($"    ERROR extracting {fileEntry.Name}: {ex.Message}");
                        }
                    }
                    // If it's a nested RPF, process it recursively
                    else if (fileEntry.Name.EndsWith(".rpf", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            // Use raw extraction for nested RPFs
                            var data = fileEntry.File.ExtractFile(fileEntry);
                            if (data != null)
                            {
                                // Create temporary file for nested RPF
                                var tempPath = Path.GetTempFileName();
                                File.WriteAllBytes(tempPath, data);
                                
                                var nestedRpf = new RpfFile(tempPath, fileEntry.Path);
                                nestedRpf.ScanStructure(null, null);
                                
                                _log($"    Scanning nested RPF: {fileEntry.Name} ({nestedRpf.AllEntries?.Count ?? 0} entries)");
                                
                                // Recursively extract models from nested RPF
                                ExtractModelsRecursive(nestedRpf, vehiclesOutputDir, weaponsOutputDir, stats);
                                
                                // Clean up temp file
                                try { File.Delete(tempPath); } catch { }
                            }
                        }
                        catch (Exception ex)
                        {
                            _log($"    Warning: Could not process nested RPF {fileEntry.Name}: {ex.Message}");
                        }
                    }
                }
            }
        }


        private bool IsModelFile(string fileName)
        {
            return fileName.EndsWith(".yft") || 
                   fileName.EndsWith(".ytd") || 
                   fileName.EndsWith(".ycd") ||
                   fileName.EndsWith(".ydr");
        }

        private bool IsWeaponModel(string fileName)
        {
            // Simple weapon detection - anything that starts with 'w_' or contains 'weapon'
            return fileName.StartsWith("w_") || 
                   fileName.Contains("weapon");
        }
    }
}