using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace CodeWalker.DLCMerger
{
    /// <summary>
    /// Handles XML/meta file merging with template system
    /// </summary>
    public class XmlMerger
    {
        private readonly Action<string> _log;
        private readonly Dictionary<string, List<(string source, byte[] data)>> _collectedFiles;
        private readonly List<string> _vehicleWeaponsFiles;
        private string _dlcName;
        private SelectiveMerger? _selectiveMerger;

        // XML Templates for each meta file type
        // CRITICAL: These templates must match exact GTA V XML container structures
        // Validated against manually extracted reference files from dlc1.rpf/common/data/
        // See XML_TEMPLATE_STRUCTURE_DOCUMENTATION.md for detailed specifications
        private static readonly Dictionary<string, string> XmlTemplates = new Dictionary<string, string>
        {
            ["vehicles.meta"] = @"<CVehicleModelInfo__InitDataList>
  <residentTxd>vehshare</residentTxd>
  <residentAnims />
  <InitDatas>
  </InitDatas>
  <txdRelationships>
  </txdRelationships>
</CVehicleModelInfo__InitDataList>",

            ["handling.meta"] = @"<CHandlingDataMgr>
  <HandlingData>
  </HandlingData>
</CHandlingDataMgr>",

            // Vehicle modification colors and kits - CORRECTED: CVehicleModelInfoVarGlobal (was CVehicleModColours)
            // Must include <Lights /> element for proper GTA V compatibility
            ["carcols.meta"] = @"<CVehicleModelInfoVarGlobal>
  <Kits>
  </Kits>
  <Lights />
</CVehicleModelInfoVarGlobal>",

            // Vehicle color variations and liveries - CORRECTED: CVehicleModelInfoVariation (was CVehicleVariations)
            ["carvariations.meta"] = @"<CVehicleModelInfoVariation>
  <variationData>
  </variationData>
</CVehicleModelInfoVariation>",

            ["vehiclelayouts.meta"] = @"<CVehicleMetadataMgr>
  <VehicleLayoutInfos>
  </VehicleLayoutInfos>
  <VehicleEntryPointInfos>
  </VehicleEntryPointInfos>
  <VehicleExtraPointsInfos>
  </VehicleExtraPointsInfos>
  <VehicleEntryPointAnimInfos>
  </VehicleEntryPointAnimInfos>
  <VehicleSeatInfos>
  </VehicleSeatInfos>
  <VehicleSeatAnimInfos>
  </VehicleSeatAnimInfos>
</CVehicleMetadataMgr>",

            ["weaponarchetypes.meta"] = @"<CWeaponArchetypeDef>
  <weaponArchetypes>
  </weaponArchetypes>
</CWeaponArchetypeDef>",

            ["explosion.meta"] = @"<CExplosionManager>
  <ExplosionFx>
  </ExplosionFx>
</CExplosionManager>",

            ["contentunlocks.meta"] = @"<CContentUnlocks>
  <VehicleUnlocks>
  </VehicleUnlocks>
</CContentUnlocks>",

            ["caraddoncontentunlocks.meta"] = @"<CCarAddonContentUnlocks>
  <VehicleAddonUnlocks>
  </VehicleAddonUnlocks>
</CCarAddonContentUnlocks>"
        };

        public XmlMerger(Action<string> log)
        {
            _log = log;
            _collectedFiles = new Dictionary<string, List<(string, byte[])>>();
            _vehicleWeaponsFiles = new List<string>();
            _dlcName = "merged";
        }

        public void Initialize(string dlcName, SelectiveMerger? selectiveMerger)
        {
            _dlcName = dlcName;
            _selectiveMerger = selectiveMerger;
        }

        public void AddFile(string fileName, byte[] data, string source)
        {
            var lowerFileName = fileName.ToLowerInvariant();
            
            // Special handling for vehicleweapons_*.meta files
            if (lowerFileName.StartsWith("vehicleweapons_") && lowerFileName.EndsWith(".meta"))
            {
                _vehicleWeaponsFiles.Add(fileName);
                // Store with unique key to prevent merging
                var uniqueKey = $"{Path.GetFileNameWithoutExtension(source)}_{fileName}";
                _collectedFiles[uniqueKey] = new List<(string, byte[])> { (source, data) };
                return;
            }
            
            // Normalize vehiclelayouts_*.meta to vehiclelayouts.meta for merging
            if (lowerFileName.Contains("vehiclelayouts") && lowerFileName.EndsWith(".meta"))
            {
                lowerFileName = "vehiclelayouts.meta";
            }
            
            // Regular meta files - collect for merging
            if (!_collectedFiles.ContainsKey(lowerFileName))
            {
                _collectedFiles[lowerFileName] = new List<(string, byte[])>();
            }
            _collectedFiles[lowerFileName].Add((source, data));
        }

        public Dictionary<string, byte[]> GetMergedFiles()
        {
            var mergedFiles = new Dictionary<string, byte[]>();
            
            foreach (var kvp in _collectedFiles)
            {
                var fileName = kvp.Key;
                var files = kvp.Value;
                
                _log($"  Merging {fileName} from {files.Count} sources");
                
                try
                {
                    byte[] mergedData;
                    
                    // Check if it's a special vehicleweapons file (already unique)
                    if (fileName.Contains("_vehicleweapons_"))
                    {
                        // Just use the single file data
                        mergedData = files[0].data;
                        // Extract the original filename
                        var parts = fileName.Split('_');
                        if (parts.Length >= 2)
                        {
                            fileName = string.Join("_", parts.Skip(1));
                        }
                    }
                    else if (files.Count == 1)
                    {
                        // Single file, no merging needed - but ensure it has XML header
                        var content = Encoding.UTF8.GetString(files[0].data);
                        if (!content.StartsWith("<?xml"))
                        {
                            content = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" + content;
                        }
                        mergedData = Encoding.UTF8.GetBytes(content);
                    }
                    else
                    {
                        // Multiple files, need to merge
                        mergedData = MergeXmlFiles(fileName, files);
                    }
                    
                    mergedFiles[fileName] = mergedData;
                }
                catch (Exception ex)
                {
                    _log($"    ERROR merging {fileName}: {ex.Message}");
                    // Use first file as fallback
                    if (files.Count > 0)
                    {
                        mergedFiles[fileName] = files[0].data;
                    }
                }
            }
            
            // Apply validation to clean up unused carcols items and empty kits
            ValidateAndCleanupFiles(mergedFiles);
            
            return mergedFiles;
        }

        private byte[] MergeXmlFiles(string fileName, List<(string source, byte[] data)> files)
        {
            // Get template if available
            string? template = null;
            foreach (var key in XmlTemplates.Keys)
            {
                if (fileName.Equals(key, StringComparison.OrdinalIgnoreCase))
                {
                    template = XmlTemplates[key];
                    break;
                }
            }
            
            if (template == null)
            {
                _log($"    No template for {fileName}, using first file as base");
                return files[0].data;
            }
            
            // Parse template
            var mergedDoc = XDocument.Parse(template);
            
            // Find all containers in the template that can hold items
            var containers = FindAllContainers(mergedDoc);
            
            if (containers.Count == 0)
            {
                _log($"    No containers found in template for {fileName}");
                return files[0].data;
            }
            
            _log($"    Found {containers.Count} containers in template: {string.Join(", ", containers.Keys)}");
            
            // Collect items from all sources and organize by container
            var containerItems = new Dictionary<string, List<XElement>>();
            foreach (var containerName in containers.Keys)
            {
                containerItems[containerName] = new List<XElement>();
            }
            
            foreach (var (source, data) in files)
            {
                try
                {
                    var content = Encoding.UTF8.GetString(data);
                    var doc = XDocument.Parse(content);
                    var sourceContainers = FindAllContainers(doc);
                    
                    int totalItems = 0;
                    foreach (var (containerName, containerElement) in sourceContainers)
                    {
                        if (containerItems.ContainsKey(containerName))
                        {
                            var items = containerElement.Elements("Item").ToList();
                            containerItems[containerName].AddRange(items);
                            totalItems += items.Count;
                        }
                    }
                    
                    _log($"    Extracted {totalItems} items from {source}");
                }
                catch (Exception ex)
                {
                    _log($"    ERROR parsing {fileName} from {source}: {ex.Message}");
                }
            }
            
            // Add all items to their respective containers in the merged document
            int mergedTotal = 0;
            foreach (var (containerName, items) in containerItems)
            {
                var container = containers[containerName];
                foreach (var item in items)
                {
                    container.Add(new XElement(item));
                    mergedTotal++;
                }
            }
            
            _log($"    Merged {mergedTotal} items from {files.Count} sources across {containers.Count} containers");
            
            // Add XML declaration header
            var xmlString = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" + mergedDoc.ToString();
            return Encoding.UTF8.GetBytes(xmlString);
        }

        private Dictionary<string, XElement> FindAllContainers(XDocument doc)
        {
            var containers = new Dictionary<string, XElement>();
            
            if (doc.Root == null) return containers;
            
            // Find all child elements of the root that can contain items
            foreach (var element in doc.Root.Elements())
            {
                containers[element.Name.LocalName] = element;
            }
            
            return containers;
        }

        private XElement? FindItemsParent(XDocument doc, string fileName)
        {
            var lowerFileName = fileName.ToLowerInvariant();
            
            return lowerFileName switch
            {
                "vehicles.meta" => doc.Root?.Element("InitDatas"),
                "handling.meta" => doc.Root?.Element("HandlingData"),
                "carcols.meta" => doc.Root?.Element("Kits"),
                "carvariations.meta" => doc.Root?.Element("variationData"),
                "vehiclelayouts.meta" => doc.Root?.Element("VehicleLayoutInfos"),
                "weaponarchetypes.meta" => doc.Root?.Element("weaponArchetypes"),
                "explosion.meta" => doc.Root?.Element("ExplosionFx"),
                "contentunlocks.meta" => doc.Root?.Element("VehicleUnlocks"),
                "caraddoncontentunlocks.meta" => doc.Root?.Element("VehicleAddonUnlocks"),
                _ => null
            };
        }

        private List<XElement> ExtractItems(XDocument doc, string fileName)
        {
            var items = new List<XElement>();
            var itemsParent = FindItemsParent(doc, fileName);
            
            if (itemsParent != null)
            {
                items.AddRange(itemsParent.Elements("Item"));
            }
            
            return items;
        }

        public string GenerateContentXml(string dlcName, string? customNameHash = null, string? customDeviceName = null)
        {
            var nameHash = customNameHash ?? dlcName;
            var dlcDeviceName = customDeviceName ?? $"dlc_{SanitizeDlcName(dlcName)}";
            var sb = new StringBuilder();
            
            sb.AppendLine(@"<?xml version=""1.0"" encoding=""UTF-8""?>");
            sb.AppendLine("<CDataFileMgr__ContentsOfDataFileXml>");
            sb.AppendLine("\t<disabledFiles/>");
            sb.AppendLine("\t<includedXmlFiles/>");
            sb.AppendLine("\t<includedDataFiles/>");
            sb.AppendLine("\t<dataFiles>");
            
            // Add entries for all meta files
            var dataFiles = new List<string>();
            foreach (var fileName in _collectedFiles.Keys)
            {
                // Skip special vehicleweapons files for now
                if (fileName.Contains("_vehicleweapons_")) continue;
                
                // Convert backslashes to forward slashes for paths
                var cleanFileName = fileName.Replace('\\', '/');
                dataFiles.Add(cleanFileName);
            }
            
            // Add vehicleweapons files (clean paths)
            foreach (var weaponFile in _vehicleWeaponsFiles)
            {
                dataFiles.Add(weaponFile.Replace('\\', '/'));
            }
            
            // Sort and add to content.xml
            dataFiles.Sort();
            
            foreach (var fileName in dataFiles)
            {
                var fileType = GetFileType(fileName);
                sb.AppendLine("\t\t<Item>");
                sb.AppendLine($"\t\t\t<filename>{dlcDeviceName}:/data/{fileName}</filename>");
                sb.AppendLine($"\t\t\t<fileType>{fileType}</fileType>");
                sb.AppendLine("\t\t\t<overlay value=\"false\"/>");
                sb.AppendLine("\t\t\t<disabled value=\"true\"/>");
                sb.AppendLine("\t\t\t<persistent value=\"false\"/>");
                sb.AppendLine("\t\t</Item>");
            }
            
            // Add RPF containers
            sb.AppendLine("\t\t<Item>");
            sb.AppendLine($"\t\t\t<filename>{dlcDeviceName}:/vehicles.rpf</filename>");
            sb.AppendLine("\t\t\t<fileType>RPF_FILE</fileType>");
            sb.AppendLine("\t\t\t<overlay value=\"false\"/>");
            sb.AppendLine("\t\t\t<disabled value=\"true\"/>");
            sb.AppendLine("\t\t\t<persistent value=\"true\"/>");
            sb.AppendLine("\t\t</Item>");
            
            sb.AppendLine("\t\t<Item>");
            sb.AppendLine($"\t\t\t<filename>{dlcDeviceName}:/weapons.rpf</filename>");
            sb.AppendLine("\t\t\t<fileType>RPF_FILE</fileType>");
            sb.AppendLine("\t\t\t<overlay value=\"false\"/>");
            sb.AppendLine("\t\t\t<disabled value=\"true\"/>");
            sb.AppendLine("\t\t\t<persistent value=\"true\"/>");
            sb.AppendLine("\t\t</Item>");
            
            sb.AppendLine("\t</dataFiles>");
            
            // Add content change sets
            sb.AppendLine("\t<contentChangeSets>");
            sb.AppendLine("\t\t<Item>");
            sb.AppendLine($"\t\t\t<changeSetName>{nameHash}_AUTOGEN</changeSetName>");
            sb.AppendLine("\t\t\t<mapChangeSetData/>");
            sb.AppendLine("\t\t\t<filesToInvalidate/>");
            sb.AppendLine("\t\t\t<filesToDisable/>");
            sb.AppendLine("\t\t\t<filesToEnable>");
            
            // Add all files to enable
            foreach (var fileName in dataFiles)
            {
                sb.AppendLine($"\t\t\t\t<Item>{dlcDeviceName}:/data/{fileName}</Item>");
            }
            
            sb.AppendLine($"\t\t\t\t<Item>{dlcDeviceName}:/vehicles.rpf</Item>");
            sb.AppendLine($"\t\t\t\t<Item>{dlcDeviceName}:/weapons.rpf</Item>");
            
            sb.AppendLine("\t\t\t</filesToEnable>");
            sb.AppendLine("\t\t\t<txdToLoad/>");
            sb.AppendLine("\t\t\t<txdToUnload/>");
            sb.AppendLine("\t\t\t<residentResources/>");
            sb.AppendLine("\t\t\t<unregisterResources/>");
            sb.AppendLine("\t\t</Item>");
            sb.AppendLine("\t</contentChangeSets>");
            sb.AppendLine("\t<patchFiles/>");
            sb.AppendLine("</CDataFileMgr__ContentsOfDataFileXml>");
            
            return sb.ToString();
        }

        public string GenerateSetup2Xml(string dlcName, string? customNameHash = null, string? customDeviceName = null)
        {
            var nameHash = customNameHash ?? dlcName;
            var dlcDeviceName = customDeviceName ?? $"dlc_{SanitizeDlcName(dlcName)}";
            var sb = new StringBuilder();
            
            sb.AppendLine(@"<?xml version=""1.0"" encoding=""UTF-8""?>");
            sb.AppendLine("<SSetupData>");
            sb.AppendLine($"\t<deviceName>{dlcDeviceName}</deviceName>");
            sb.AppendLine("\t<datFile>content.xml</datFile>");
            sb.AppendLine($"\t<timeStamp>{DateTime.Now:MM/dd/yyyy HH:mm:ss}</timeStamp>");
            sb.AppendLine($"\t<nameHash>{nameHash}</nameHash>");
            sb.AppendLine("\t<contentChangeSets/>");
            sb.AppendLine("\t<contentChangeSetGroups>");
            sb.AppendLine("\t\t<Item>");
            sb.AppendLine("\t\t\t<NameHash>GROUP_STARTUP</NameHash>");
            sb.AppendLine("\t\t\t<ContentChangeSets>");
            sb.AppendLine($"\t\t\t\t<Item>{nameHash}_AUTOGEN</Item>");
            sb.AppendLine("\t\t\t</ContentChangeSets>");
            sb.AppendLine("\t\t</Item>");
            sb.AppendLine("\t</contentChangeSetGroups>");
            sb.AppendLine("\t<startupScript/>");
            sb.AppendLine("\t<scriptCallstackSize value=\"0\"/>");
            sb.AppendLine("\t<type>EXTRACONTENT_COMPAT_PACK</type>");
            sb.AppendLine("\t<order value=\"31\"/>");
            sb.AppendLine("\t<minorOrder value=\"0\"/>");
            sb.AppendLine("\t<isLevelPack value=\"false\"/>");
            sb.AppendLine("\t<dependencyPackHash/>");
            sb.AppendLine("\t<requiredVersion/>");
            sb.AppendLine("\t<subPackCount value=\"1\"/>");
            sb.AppendLine("</SSetupData>");
            
            return sb.ToString();
        }

        private string SanitizeDlcName(string dlcName)
        {
            // Remove all non-alphanumeric characters and convert to lowercase
            var sanitized = new string(dlcName.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
            
            // Log if sanitization changed the name
            if (sanitized != dlcName.ToLowerInvariant())
            {
                _log($"  DLC device name sanitized from '{dlcName}' to '{sanitized}'");
            }
            
            return sanitized;
        }

        private void ValidateAndCleanupFiles(Dictionary<string, byte[]> mergedFiles)
        {
            try
            {
                var carcolsKey = mergedFiles.Keys.FirstOrDefault(k => k.Contains("carcols.meta"));
                var carvariationsKey = mergedFiles.Keys.FirstOrDefault(k => k.Contains("carvariations.meta"));
                
                if (carcolsKey == null || carvariationsKey == null)
                {
                    _log("    Skipping validation - missing carcols.meta or carvariations.meta");
                    return;
                }
                
                _log("  Validating carcols.meta and carvariations.meta relationships...");
                
                // Parse carvariations.meta to get referenced kit names
                var referencedKits = ExtractReferencedKits(mergedFiles[carvariationsKey]);
                _log($"    Found {referencedKits.Count} referenced kits in carvariations.meta");
                
                // Parse and clean up carcols.meta, get the final list of valid kits
                var (cleanedCarcolsData, validKitsAfterCleanup) = CleanupCarcolsKits(mergedFiles[carcolsKey], referencedKits);
                mergedFiles[carcolsKey] = cleanedCarcolsData;
                
                // Update carvariations.meta to replace empty kit references with default kit
                var cleanedCarvariationsData = CleanupCarvariationsKits(mergedFiles[carvariationsKey], validKitsAfterCleanup);
                mergedFiles[carvariationsKey] = cleanedCarvariationsData;
                
                _log("    Validation and cleanup completed successfully");
            }
            catch (Exception ex)
            {
                _log($"    ERROR during validation: {ex.Message}");
            }
        }
        
        private HashSet<string> ExtractReferencedKits(byte[] carvariationsData)
        {
            var referencedKits = new HashSet<string>();
            
            try
            {
                var content = Encoding.UTF8.GetString(carvariationsData);
                var doc = XDocument.Parse(content);
                
                // Find all kit references in carvariations.meta
                var kitItems = doc.Descendants("variationData")
                    .Elements("Item")
                    .SelectMany(item => item.Elements("kits"))
                    .SelectMany(kits => kits.Elements("Item"))
                    .Select(item => item.Value.Trim())
                    .Where(kitName => !string.IsNullOrEmpty(kitName));
                
                foreach (var kitName in kitItems)
                {
                    referencedKits.Add(kitName);
                }
            }
            catch (Exception ex)
            {
                _log($"    ERROR parsing carvariations.meta: {ex.Message}");
            }
            
            return referencedKits;
        }
        
        private (byte[], HashSet<string>) CleanupCarcolsKits(byte[] carcolsData, HashSet<string> referencedKits)
        {
            var validKitsAfterCleanup = new HashSet<string>();
            
            try
            {
                var content = Encoding.UTF8.GetString(carcolsData);
                var doc = XDocument.Parse(content);
                
                var kitsContainer = doc.Root?.Element("Kits");
                if (kitsContainer == null)
                {
                    _log("    WARNING: No Kits container found in carcols.meta");
                    return (carcolsData, validKitsAfterCleanup);
                }
                
                var kitsToRemove = new List<XElement>();
                var kitsProcessed = 0;
                var kitsRemoved = 0;
                
                foreach (var kitItem in kitsContainer.Elements("Item"))
                {
                    kitsProcessed++;
                    var kitNameElement = kitItem.Element("kitName");
                    if (kitNameElement == null) continue;
                    
                    var kitName = kitNameElement.Value.Trim();
                    
                    // Check if this kit is referenced by any vehicle in carvariations.meta
                    if (!referencedKits.Contains(kitName))
                    {
                        kitsToRemove.Add(kitItem);
                        kitsRemoved++;
                        continue;
                    }
                    
                    // Check if kit has empty visibleMods
                    var visibleModsElement = kitItem.Element("visibleMods");
                    if (visibleModsElement != null)
                    {
                        var visibleModItems = visibleModsElement.Elements("Item").ToList();
                        if (visibleModItems.Count == 0)
                        {
                            // Kit has empty visibleMods, remove it
                            kitsToRemove.Add(kitItem);
                            kitsRemoved++;
                            continue;
                        }
                    }
                    
                    // Kit is valid, add to final list
                    validKitsAfterCleanup.Add(kitName);
                }
                
                // Remove unused kits
                foreach (var kitToRemove in kitsToRemove)
                {
                    kitToRemove.Remove();
                }
                
                _log($"    Processed {kitsProcessed} kits, removed {kitsRemoved} unused/empty kits, {validKitsAfterCleanup.Count} valid kits remain");
                
                // Convert back to bytes
                var xmlString = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" + doc.ToString();
                return (Encoding.UTF8.GetBytes(xmlString), validKitsAfterCleanup);
            }
            catch (Exception ex)
            {
                _log($"    ERROR cleaning up carcols.meta: {ex.Message}");
                return (carcolsData, validKitsAfterCleanup);
            }
        }
        
        private byte[] CleanupCarvariationsKits(byte[] carvariationsData, HashSet<string> validKitsAfterCleanup)
        {
            try
            {
                var content = Encoding.UTF8.GetString(carvariationsData);
                var doc = XDocument.Parse(content);
                
                var variationItems = doc.Descendants("variationData").Elements("Item");
                var vehiclesProcessed = 0;
                var kitsReplaced = 0;
                
                foreach (var variationItem in variationItems)
                {
                    vehiclesProcessed++;
                    var kitsElement = variationItem.Element("kits");
                    if (kitsElement == null) continue;
                    
                    var kitItems = kitsElement.Elements("Item").ToList();
                    var validKits = new List<string>();
                    
                    // Collect all valid kit names (ones that still exist in carcols after cleanup)
                    foreach (var kitItem in kitItems)
                    {
                        var kitName = kitItem.Value.Trim();
                        if (!string.IsNullOrEmpty(kitName) && validKitsAfterCleanup.Contains(kitName))
                        {
                            validKits.Add(kitName);
                        }
                    }
                    
                    // If no valid kits found or all kits were removed, replace with default kit
                    if (validKits.Count == 0)
                    {
                        kitsElement.RemoveAll();
                        kitsElement.Add(new XElement("Item", "0_default_modkit"));
                        kitsReplaced++;
                    }
                    else if (validKits.Count != kitItems.Count)
                    {
                        // Some kits were invalid, rebuild the kits element with only valid ones
                        kitsElement.RemoveAll();
                        foreach (var validKit in validKits)
                        {
                            kitsElement.Add(new XElement("Item", validKit));
                        }
                    }
                }
                
                _log($"    Processed {vehiclesProcessed} vehicles, replaced {kitsReplaced} empty kit references with default kit");
                
                // Convert back to bytes
                var xmlString = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" + doc.ToString();
                return Encoding.UTF8.GetBytes(xmlString);
            }
            catch (Exception ex)
            {
                _log($"    ERROR cleaning up carvariations.meta: {ex.Message}");
                return carvariationsData;
            }
        }

        private string GetFileType(string fileName)
        {
            var lowerFileName = fileName.ToLowerInvariant();
            
            if (lowerFileName.Contains("vehicles.meta")) return "VEHICLE_METADATA_FILE";
            if (lowerFileName.Contains("handling.meta")) return "HANDLING_FILE";
            if (lowerFileName.Contains("carcols.meta")) return "CARCOLS_FILE";
            if (lowerFileName.Contains("carvariations.meta")) return "VEHICLE_VARIATION_FILE";
            if (lowerFileName.Contains("vehiclelayouts.meta")) return "VEHICLE_LAYOUTS_FILE";
            if (lowerFileName.Contains("vehicleweapons")) return "WEAPONINFO_FILE";
            if (lowerFileName.Contains("weaponarchetypes.meta")) return "WEAPON_METADATA_FILE";
            if (lowerFileName.Contains("explosion.meta")) return "EXPLOSION_INFO_FILE";
            if (lowerFileName.Contains("contentunlocks.meta")) return "CONTENT_UNLOCKING_META_FILE";
            if (lowerFileName.Contains("caraddoncontentunlocks.meta")) return "CONTENT_UNLOCKING_META_FILE";
            
            return "TEXTFILE_METAFILE"; // Default
        }
    }
}