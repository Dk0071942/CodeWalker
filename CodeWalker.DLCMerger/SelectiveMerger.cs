using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeWalker.DLCMerger
{
    /// <summary>
    /// Provides logic for selective merging of DLC content based on vehicle dependencies
    /// </summary>
    public class SelectiveMerger
    {
        private readonly Action<string> _log;
        private readonly HashSet<string> _vehicleNames = new();
        private readonly HashSet<string> _requiredFiles = new();

        // Essential meta files for vehicles
        private static readonly HashSet<string> EssentialVehicleMetaFiles = new(StringComparer.OrdinalIgnoreCase)
        {
            "vehicles.meta",
            "handling.meta",
            "carcols.meta",
            "carvariations.meta",
            "vehiclelayouts.meta"
        };

        // Files that should be skipped unless directly related to vehicles
        private static readonly HashSet<string> SkippableFiles = new(StringComparer.OrdinalIgnoreCase)
        {
            "weaponarchetypes.meta", // Skip unless vehicle has weapons
            "explosion.meta", // Skip unless vehicle uses custom explosions
            "pedpersonality.meta",
            "shop_weapon.meta",
            "combatbehaviour.meta",
            "loadouts.meta",
            "pedaccuracy.meta",
            "pedbounds.meta",
            "pedhealth.meta",
            "pedmodelinfo.meta",
            "pedperception.meta",
            "scenarios.meta",
            "speechparams.meta",
            "tattoos.meta",
            "zonebind.meta"
        };

        public SelectiveMerger(Action<string> log)
        {
            _log = log;
        }

        /// <summary>
        /// Determines if a file should be included in the merge based on vehicle dependencies
        /// </summary>
        public bool ShouldIncludeFile(string filePath, string fileName)
        {
            var lowerFileName = fileName.ToLowerInvariant();
            
            // Always include content.xml and setup2.xml
            if (lowerFileName == "content.xml" || lowerFileName == "setup2.xml")
            {
                return true;
            }

            // Always include essential vehicle meta files
            if (EssentialVehicleMetaFiles.Contains(lowerFileName))
            {
                return true;
            }

            // Check if it's a skippable file
            if (SkippableFiles.Contains(lowerFileName))
            {
                // Special handling for vehicle weapons and layouts
                if (lowerFileName.StartsWith("vehicleweapons") ||
                    lowerFileName.StartsWith("vehiclelayouts"))
                {
                    return true; // Include vehicle weapon and layout files
                }
                
                // Skip other files unless explicitly required
                return _requiredFiles.Contains(lowerFileName);
            }

            // Include vehicle model files
            if (IsVehicleModelFile(fileName))
            {
                return true;
            }

            // Include language files for vehicle names
            if (fileName.EndsWith(".gxt2", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Include any file that contains a vehicle name
            if (_vehicleNames.Any(vehicleName => 
                fileName.Contains(vehicleName, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            // Default: exclude
            return false;
        }

        /// <summary>
        /// Registers a vehicle name for dependency tracking
        /// </summary>
        public void RegisterVehicle(string vehicleName)
        {
            _vehicleNames.Add(vehicleName.ToLowerInvariant());
            _log($"Registered vehicle: {vehicleName}");
        }

        /// <summary>
        /// Registers a required file that should be included
        /// </summary>
        public void RegisterRequiredFile(string fileName)
        {
            _requiredFiles.Add(fileName.ToLowerInvariant());
        }

        /// <summary>
        /// Analyzes vehicles.meta to extract vehicle names and dependencies
        /// </summary>
        public void AnalyzeVehiclesMeta(byte[] vehiclesMetaData)
        {
            try
            {
                var content = System.Text.Encoding.UTF8.GetString(vehiclesMetaData);
                var doc = System.Xml.Linq.XDocument.Parse(content);

                // Extract vehicle model names
                var modelNames = doc.Descendants("modelName")
                    .Select(e => e.Value)
                    .Where(name => !string.IsNullOrEmpty(name));

                foreach (var modelName in modelNames)
                {
                    RegisterVehicle(modelName);
                }

                // Extract texture names
                var txdNames = doc.Descendants("txdName")
                    .Select(e => e.Value)
                    .Where(name => !string.IsNullOrEmpty(name));

                foreach (var txdName in txdNames)
                {
                    RegisterVehicle(txdName);
                }

                // Check for vehicle weapons
                var hasWeapons = doc.Descendants("layout")
                    .Any(e => e.Value.Contains("WEAPONIZED", StringComparison.OrdinalIgnoreCase));

                if (hasWeapons)
                {
                    RegisterRequiredFile("weaponarchetypes.meta");
                    _log("Detected weaponized vehicles, including weapon archetypes");
                }

                // Check for custom explosions
                var explosionInfos = doc.Descendants("explosionInfo")
                    .Select(e => e.Value)
                    .Where(name => !string.IsNullOrEmpty(name) && name != "EXPLOSION_INFO_DEFAULT");

                if (explosionInfos.Any())
                {
                    RegisterRequiredFile("explosion.meta");
                    _log("Detected custom explosions, including explosion.meta");
                }
            }
            catch (Exception ex)
            {
                _log($"Error analyzing vehicles.meta: {ex.Message}");
            }
        }

        /// <summary>
        /// Checks if a file is a vehicle model file
        /// </summary>
        private bool IsVehicleModelFile(string fileName)
        {
            var lowerFileName = fileName.ToLowerInvariant();
            
            // Vehicle model files
            if (lowerFileName.EndsWith(".yft") || 
                lowerFileName.EndsWith(".ytd") ||
                lowerFileName.EndsWith(".ycd"))
            {
                // Exclude weapon models
                if (lowerFileName.StartsWith("w_") || 
                    lowerFileName.Contains("weapon"))
                {
                    return false;
                }
                
                return true;
            }

            return false;
        }

        /// <summary>
        /// Creates selective content.xml with only vehicle-related entries
        /// </summary>
        public byte[] CreateSelectiveContentXml(byte[] originalContentXml, string dlcName)
        {
            try
            {
                var content = System.Text.Encoding.UTF8.GetString(originalContentXml);
                var doc = System.Xml.Linq.XDocument.Parse(content);
                var root = doc.Root;

                if (root == null) return originalContentXml;

                // Filter dataFiles entries
                var dataFiles = root.Element("dataFiles");
                if (dataFiles != null)
                {
                    var itemsToKeep = new List<System.Xml.Linq.XElement>();
                    
                    foreach (var item in dataFiles.Elements("Item").ToList())
                    {
                        var filename = item.Element("filename")?.Value ?? "";
                        var fileType = item.Element("fileType")?.Value ?? "";
                        
                        // Keep vehicle-related file types
                        if (IsVehicleRelatedFileType(fileType) || 
                            filename.Contains(".rpf", StringComparison.OrdinalIgnoreCase))
                        {
                            itemsToKeep.Add(item);
                        }
                    }
                    
                    dataFiles.RemoveAll();
                    foreach (var item in itemsToKeep)
                    {
                        dataFiles.Add(item);
                    }
                }

                // Filter contentChangeSets to only include vehicle-related files
                var changeSets = root.Element("contentChangeSets");
                if (changeSets != null)
                {
                    foreach (var changeSet in changeSets.Elements("Item"))
                    {
                        var filesToEnable = changeSet.Element("filesToEnable");
                        if (filesToEnable != null)
                        {
                            var filesToKeep = new List<System.Xml.Linq.XElement>();
                            
                            foreach (var file in filesToEnable.Elements("Item").ToList())
                            {
                                var fileName = file.Value;
                                if (ShouldIncludeInContentXml(fileName))
                                {
                                    filesToKeep.Add(file);
                                }
                            }
                            
                            filesToEnable.RemoveAll();
                            foreach (var file in filesToKeep)
                            {
                                filesToEnable.Add(file);
                            }
                        }
                    }
                }

                return System.Text.Encoding.UTF8.GetBytes(doc.ToString());
            }
            catch (Exception ex)
            {
                _log($"Error creating selective content.xml: {ex.Message}");
                return originalContentXml;
            }
        }

        private bool IsVehicleRelatedFileType(string fileType)
        {
            var vehicleFileTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "VEHICLE_METADATA_FILE",
                "VEHICLE_VARIATION_FILE",
                "VEHICLE_LAYOUTS_FILE",
                "HANDLING_FILE",
                "CARCOLS_FILE",
                "TEXTFILE_METAFILE", // For language files
                "RPF_FILE", // For nested containers
                "DLC_TEXT_FILE"
            };

            return vehicleFileTypes.Contains(fileType);
        }

        private bool ShouldIncludeInContentXml(string fileName)
        {
            var lowerFileName = fileName.ToLowerInvariant();
            
            // Always include RPF containers
            if (lowerFileName.EndsWith(".rpf"))
            {
                return true;
            }

            // Include essential vehicle files
            if (lowerFileName.Contains("vehicles.meta") ||
                lowerFileName.Contains("handling.meta") ||
                lowerFileName.Contains("carcols.meta") ||
                lowerFileName.Contains("carvariations.meta") ||
                lowerFileName.Contains("vehiclelayouts.meta") ||
                lowerFileName.Contains("dlctext.meta"))
            {
                return true;
            }

            // Include vehicle weapons if present
            if (lowerFileName.Contains("vehicleweapons"))
            {
                return true;
            }

            // Check against registered required files
            var justFileName = System.IO.Path.GetFileName(lowerFileName);
            if (_requiredFiles.Contains(justFileName))
            {
                return true;
            }

            return false;
        }
    }
}