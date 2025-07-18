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

            ["carcols.meta"] = @"<CVehicleModColours>
  <Kits>
  </Kits>
</CVehicleModColours>",

            ["carvariations.meta"] = @"<CVehicleVariations>
  <variationData>
  </variationData>
</CVehicleVariations>",

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

        public string GenerateContentXml(string dlcName)
        {
            var dlcDeviceName = $"dlc_{dlcName}";
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
            sb.AppendLine($"\t\t\t<changeSetName>{dlcName}_AUTOGEN</changeSetName>");
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

        public string GenerateSetup2Xml(string dlcName)
        {
            var dlcDeviceName = $"dlc_{dlcName}";
            var sb = new StringBuilder();
            
            sb.AppendLine(@"<?xml version=""1.0"" encoding=""UTF-8""?>");
            sb.AppendLine("<SSetupData>");
            sb.AppendLine($"\t<deviceName>{dlcDeviceName}</deviceName>");
            sb.AppendLine("\t<datFile>content.xml</datFile>");
            sb.AppendLine($"\t<timeStamp>{DateTime.Now:MM/dd/yyyy HH:mm:ss}</timeStamp>");
            sb.AppendLine($"\t<nameHash>{dlcName}</nameHash>");
            sb.AppendLine("\t<contentChangeSetGroups>");
            sb.AppendLine("\t\t<Item>");
            sb.AppendLine("\t\t\t<NameHash>GROUP_STARTUP</NameHash>");
            sb.AppendLine("\t\t\t<ContentChangeSets>");
            sb.AppendLine($"\t\t\t\t<Item>{dlcName}_AUTOGEN</Item>");
            sb.AppendLine("\t\t\t</ContentChangeSets>");
            sb.AppendLine("\t\t</Item>");
            sb.AppendLine("\t</contentChangeSetGroups>");
            sb.AppendLine("\t<type>EXTRACONTENT_COMPAT_PACK</type>");
            sb.AppendLine("\t<order value=\"9\"/>");
            sb.AppendLine("\t<subPackCount value=\"0\"/>");
            sb.AppendLine("</SSetupData>");
            
            return sb.ToString();
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