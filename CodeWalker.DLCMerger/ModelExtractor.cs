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

        public ModelStats ExtractModels(RpfFile rpf, string vehiclesOutputDir, string weaponsOutputDir, bool mergeAll)
        {
            var stats = new ModelStats();
            var allEntries = GetAllEntriesRecursive(rpf);
            
            foreach (var entry in allEntries.OfType<RpfFileEntry>())
            {
                var fileName = entry.Name.ToLowerInvariant();
                
                // Check if it's a model file
                if (!IsModelFile(fileName))
                    continue;
                
                // Determine if it's a vehicle or weapon model
                bool isWeaponModel = IsWeaponModel(fileName);
                
                // Skip non-vehicle models if selective merge is enabled
                if (!mergeAll && !isWeaponModel && !IsVehicleModel(fileName))
                {
                    _log($"    Skipping non-vehicle model: {fileName}");
                    continue;
                }
                
                try
                {
                    // Extract the file
                    var data = rpf.ExtractFile(entry);
                    if (data == null) continue;
                    
                    // Determine output directory
                    var outputDir = isWeaponModel ? weaponsOutputDir : vehiclesOutputDir;
                    var outputPath = Path.Combine(outputDir, entry.Name);
                    
                    // Write the file
                    File.WriteAllBytes(outputPath, data);
                    
                    // Update statistics
                    if (isWeaponModel)
                    {
                        stats.WeaponModels++;
                        _log($"    Extracted weapon model: {entry.Name}");
                    }
                    else
                    {
                        stats.VehicleModels++;
                        _log($"    Extracted vehicle model: {entry.Name}");
                    }
                }
                catch (Exception ex)
                {
                    _log($"    ERROR extracting {entry.Name}: {ex.Message}");
                }
            }
            
            return stats;
        }

        private List<RpfEntry> GetAllEntriesRecursive(RpfFile rpf)
        {
            var entries = new List<RpfEntry>();
            
            if (rpf.AllEntries != null)
            {
                foreach (var entry in rpf.AllEntries)
                {
                    entries.Add(entry);
                    
                    // If it's a nested RPF, process it recursively
                    if (entry is RpfFileEntry fileEntry && 
                        fileEntry.Name.EndsWith(".rpf", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            var data = rpf.ExtractFile(fileEntry);
                            if (data != null)
                            {
                                // Create temporary file for nested RPF
                                var tempPath = Path.GetTempFileName();
                                File.WriteAllBytes(tempPath, data);
                                
                                var nestedRpf = new RpfFile(tempPath, fileEntry.Path);
                                nestedRpf.ScanStructure(null, null);
                                
                                var nestedEntries = GetAllEntriesRecursive(nestedRpf);
                                entries.AddRange(nestedEntries);
                                
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
            
            return entries;
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
            // Weapon models typically start with 'w_'
            return fileName.StartsWith("w_") || 
                   fileName.Contains("weapon") ||
                   fileName.Contains("_w_");
        }

        private bool IsVehicleModel(string fileName)
        {
            // Exclude ped models and other non-vehicle models
            if (fileName.StartsWith("p_") ||      // Prop models
                fileName.StartsWith("s_") ||      // Static models
                fileName.StartsWith("v_") ||      // Some interior models
                fileName.StartsWith("ig_") ||     // Cutscene models
                fileName.StartsWith("cs_") ||     // Cutscene models
                fileName.StartsWith("mp_") ||     // Multiplayer models
                fileName.StartsWith("u_") ||      // Unknown/utility models
                fileName.Contains("ped") ||       // Pedestrian models
                fileName.Contains("player"))      // Player models
            {
                return false;
            }
            
            // If it's not a weapon and not in the exclude list, consider it a vehicle
            return !IsWeaponModel(fileName);
        }
    }
}