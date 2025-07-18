using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CodeWalker.GameFiles;

namespace CodeWalker.DLCMerger
{
    /// <summary>
    /// Handles intelligent merging of GTA V metadata XML files
    /// </summary>
    public class MetaMerger
    {
        private readonly Action<string> _log;
        private readonly SelectiveMerger _selectiveMerger;
        private readonly Dictionary<string, List<(string sourcePath, byte[] data)>> _metaFiles = new();
        private readonly Dictionary<string, byte[]> _mergedFiles = new();
        private readonly string _outputDlcName;

        // Mapping of file types to their container elements
        private static readonly Dictionary<string, string> MetaFileContainers = new()
        {
            { "vehicles.meta", "InitDatas" },
            { "handling.meta", "HandlingData" },
            { "carcols.meta", "Kits" },
            { "carvariations.meta", "variationData" },
            { "vehiclelayouts.meta", "VehicleLayoutInfos" },
            { "dlctext.meta", "DlcText" },
            { "weaponarchetypes.meta", "Archetypes" }
        };

        public MetaMerger(Action<string> log, SelectiveMerger selectiveMerger, string outputDlcName = "merged_vehicles")
        {
            _log = log;
            _selectiveMerger = selectiveMerger;
            _outputDlcName = outputDlcName;
        }

        /// <summary>
        /// Merges multiple XML meta files of the same type
        /// </summary>
        public byte[] MergeMetaFiles(List<(string sourcePath, byte[] data)> metaFiles, string metaType)
        {
            try
            {
                _log($"Merging {metaFiles.Count} {metaType} files");

                // Parse all XML documents
                var documents = new List<(string source, XDocument doc)>();
                foreach (var (sourcePath, data) in metaFiles)
                {
                    try
                    {
                        var content = Encoding.UTF8.GetString(data);
                        var doc = XDocument.Parse(content);
                        documents.Add((sourcePath, doc));
                    }
                    catch (Exception ex)
                    {
                        _log($"Failed to parse {metaType} from {sourcePath}: {ex.Message}");
                    }
                }

                if (documents.Count == 0)
                {
                    _log($"No valid {metaType} files to merge");
                    return metaFiles.FirstOrDefault().data ?? Array.Empty<byte>();
                }

                // Use the first document as base
                var mergedDoc = new XDocument(documents[0].doc);
                var containerName = GetContainerName(metaType);

                if (string.IsNullOrEmpty(containerName))
                {
                    _log($"Unknown meta file type: {metaType}");
                    return Encoding.UTF8.GetBytes(mergedDoc.ToString());
                }

                // Find the container element in the merged document
                var mergedContainer = FindContainerElement(mergedDoc, containerName);
                if (mergedContainer == null)
                {
                    _log($"Container element '{containerName}' not found in {metaType}");
                    return Encoding.UTF8.GetBytes(mergedDoc.ToString());
                }

                // Merge items from other documents
                for (int i = 1; i < documents.Count; i++)
                {
                    var (source, doc) = documents[i];
                    var sourceContainer = FindContainerElement(doc, containerName);
                    
                    if (sourceContainer == null)
                    {
                        _log($"Container element '{containerName}' not found in {source}");
                        continue;
                    }

                    MergeContainerItems(mergedContainer, sourceContainer, metaType);
                }

                _log($"Successfully merged {metaType} with {mergedContainer.Elements("Item").Count()} total items");
                return Encoding.UTF8.GetBytes(mergedDoc.ToString());
            }
            catch (Exception ex)
            {
                _log($"Error merging {metaType}: {ex.Message}");
                return metaFiles.FirstOrDefault().data ?? Array.Empty<byte>();
            }
        }

        /// <summary>
        /// Merges content.xml files intelligently
        /// </summary>
        public byte[] MergeContentXmlFiles(List<(string sourcePath, byte[] data)> contentFiles, string outputDlcName)
        {
            try
            {
                _log($"Merging {contentFiles.Count} content.xml files");

                // Create a new content.xml with the output DLC name
                var mergedDoc = new XDocument(
                    new XDeclaration("1.0", "UTF-8", null),
                    new XElement("CDataFileMgr__ContentsOfDataFileXml")
                );

                var root = mergedDoc.Root;
                root.Add(new XElement("disabledFiles"));
                root.Add(new XElement("includedXmlFiles"));
                root.Add(new XElement("includedDataFiles"));
                
                var dataFiles = new XElement("dataFiles");
                root.Add(dataFiles);

                var contentChangeSets = new XElement("contentChangeSets");
                root.Add(contentChangeSets);

                root.Add(new XElement("patchFiles"));

                // Collect all data files and change sets
                var allDataFiles = new List<XElement>();
                var allChangeSets = new List<XElement>();
                var filesToEnable = new HashSet<string>();

                foreach (var (sourcePath, data) in contentFiles)
                {
                    try
                    {
                        var content = Encoding.UTF8.GetString(data);
                        var doc = XDocument.Parse(content);

                        // Extract data files
                        var sourceDataFiles = doc.Root?.Element("dataFiles")?.Elements("Item") ?? Enumerable.Empty<XElement>();
                        foreach (var item in sourceDataFiles)
                        {
                            var filename = item.Element("filename")?.Value;
                            if (!string.IsNullOrEmpty(filename))
                            {
                                // Update filename to use the output DLC name
                                var updatedFilename = UpdateDlcPath(filename, outputDlcName);
                                item.Element("filename").Value = updatedFilename;
                                allDataFiles.Add(new XElement(item));
                            }
                        }

                        // Extract files to enable from change sets
                        var sourceChangeSets = doc.Root?.Element("contentChangeSets")?.Elements("Item") ?? Enumerable.Empty<XElement>();
                        foreach (var changeSet in sourceChangeSets)
                        {
                            var files = changeSet.Element("filesToEnable")?.Elements("Item") ?? Enumerable.Empty<XElement>();
                            foreach (var file in files)
                            {
                                if (!string.IsNullOrEmpty(file.Value))
                                {
                                    filesToEnable.Add(UpdateDlcPath(file.Value, outputDlcName));
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _log($"Failed to parse content.xml from {sourcePath}: {ex.Message}");
                    }
                }

                // Add unique data files
                var addedFiles = new HashSet<string>();
                foreach (var dataFile in allDataFiles)
                {
                    var filename = dataFile.Element("filename")?.Value;
                    if (!string.IsNullOrEmpty(filename) && !addedFiles.Contains(filename))
                    {
                        dataFiles.Add(dataFile);
                        addedFiles.Add(filename);
                    }
                }

                // Create a single merged change set
                var mergedChangeSet = new XElement("Item",
                    new XElement("changeSetName", $"{outputDlcName}_MERGED_AUTOGEN"),
                    new XElement("mapChangeSetData"),
                    new XElement("filesToInvalidate"),
                    new XElement("filesToDisable"),
                    new XElement("filesToEnable", filesToEnable.Select(f => new XElement("Item", f))),
                    new XElement("txdToLoad"),
                    new XElement("txdToUnload"),
                    new XElement("residentResources"),
                    new XElement("unregisterResources")
                );
                contentChangeSets.Add(mergedChangeSet);

                _log($"Merged content.xml with {dataFiles.Elements().Count()} data files and {filesToEnable.Count} files to enable");
                return Encoding.UTF8.GetBytes(mergedDoc.ToString());
            }
            catch (Exception ex)
            {
                _log($"Error merging content.xml files: {ex.Message}");
                return contentFiles.FirstOrDefault().data ?? Array.Empty<byte>();
            }
        }

        /// <summary>
        /// Merges setup2.xml files intelligently
        /// </summary>
        public byte[] MergeSetup2XmlFiles(List<(string sourcePath, byte[] data)> setup2Files, string outputDlcName)
        {
            try
            {
                _log($"Merging {setup2Files.Count} setup2.xml files");

                // Create a new setup2.xml
                var mergedDoc = new XDocument(
                    new XDeclaration("1.0", "UTF-8", null),
                    new XElement("SSetupData")
                );

                var root = mergedDoc.Root;
                
                // Set up basic structure
                root.Add(new XElement("deviceName", $"dlc_{outputDlcName}"));
                root.Add(new XElement("datFile", "content.xml"));
                root.Add(new XElement("timeStamp", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")));
                root.Add(new XElement("nameHash", outputDlcName));
                root.Add(new XElement("contentChangeSets"));
                
                var contentChangeSetGroups = new XElement("contentChangeSetGroups");
                root.Add(contentChangeSetGroups);

                // Use the first file's order and type
                var firstDoc = XDocument.Parse(Encoding.UTF8.GetString(setup2Files[0].data));
                var order = firstDoc.Root?.Element("order")?.Value ?? "999";
                var type = firstDoc.Root?.Element("type")?.Value ?? "EXTRACONTENT_COMPAT_PACK";
                var isLevelPack = firstDoc.Root?.Element("isLevelPack")?.Value ?? "false";

                root.Add(new XElement("order", new XAttribute("value", order)));
                root.Add(new XElement("minorOrder", new XAttribute("value", "0")));
                root.Add(new XElement("isLevelPack", new XAttribute("value", isLevelPack)));
                root.Add(new XElement("dependencyPackHash"));
                root.Add(new XElement("requiredVersion"));
                root.Add(new XElement("subPackCount", new XAttribute("value", "0")));
                root.Add(new XElement("type", type));

                // Create a merged content change set group
                var mergedGroup = new XElement("Item",
                    new XElement("NameHash", "GROUP_STARTUP"),
                    new XElement("ContentChangeSets",
                        new XElement("Item", $"{outputDlcName}_MERGED_AUTOGEN")
                    )
                );
                contentChangeSetGroups.Add(mergedGroup);

                _log($"Merged setup2.xml for {outputDlcName}");
                return Encoding.UTF8.GetBytes(mergedDoc.ToString());
            }
            catch (Exception ex)
            {
                _log($"Error merging setup2.xml files: {ex.Message}");
                return setup2Files.FirstOrDefault().data ?? Array.Empty<byte>();
            }
        }

        private string GetContainerName(string metaType)
        {
            var fileName = System.IO.Path.GetFileName(metaType).ToLowerInvariant();
            return MetaFileContainers.TryGetValue(fileName, out var container) ? container : null;
        }

        private XElement FindContainerElement(XDocument doc, string containerName)
        {
            // Try direct child first
            var container = doc.Root?.Element(containerName);
            if (container != null) return container;

            // Try nested search
            return doc.Root?.Descendants(containerName).FirstOrDefault();
        }

        private void MergeContainerItems(XElement targetContainer, XElement sourceContainer, string metaType)
        {
            var itemsAdded = 0;
            var itemsSkipped = 0;

            foreach (var sourceItem in sourceContainer.Elements("Item"))
            {
                // Check if item already exists (based on name/modelName)
                var itemName = GetItemIdentifier(sourceItem, metaType);
                
                if (!string.IsNullOrEmpty(itemName))
                {
                    var exists = targetContainer.Elements("Item")
                        .Any(item => GetItemIdentifier(item, metaType) == itemName);

                    if (exists)
                    {
                        itemsSkipped++;
                        _log($"  Skipping duplicate item: {itemName}");
                        continue;
                    }
                }

                // Add the item
                targetContainer.Add(new XElement(sourceItem));
                itemsAdded++;
            }

            _log($"  Added {itemsAdded} items, skipped {itemsSkipped} duplicates");
        }

        private string GetItemIdentifier(XElement item, string metaType)
        {
            var fileName = System.IO.Path.GetFileName(metaType).ToLowerInvariant();

            return fileName switch
            {
                "vehicles.meta" => item.Element("modelName")?.Value ?? item.Element("txdName")?.Value,
                "handling.meta" => item.Element("handlingName")?.Value,
                "carcols.meta" => item.Element("kitName")?.Value,
                "carvariations.meta" => item.Element("modelName")?.Value,
                "vehiclelayouts.meta" => item.Element("Id")?.Value ?? item.Element("Name")?.Value,
                "dlctext.meta" => item.Element("Hash")?.Value,
                "weaponarchetypes.meta" => item.Element("Name")?.Value,
                _ => null
            };
        }

        private string UpdateDlcPath(string path, string newDlcName)
        {
            // Replace dlc_XXX:/ prefix with new DLC name
            var colonIndex = path.IndexOf(":/");
            if (colonIndex > 0)
            {
                return $"dlc_{newDlcName}:/{path.Substring(colonIndex + 2)}";
            }
            return path;
        }

        /// <summary>
        /// Adds a meta file to the merger for processing
        /// </summary>
        public void AddMetaFile(string fileName, byte[] data, string sourcePath)
        {
            var lowerFileName = fileName.ToLowerInvariant();
            
            if (!_metaFiles.ContainsKey(lowerFileName))
            {
                _metaFiles[lowerFileName] = new List<(string sourcePath, byte[] data)>();
            }
            
            _metaFiles[lowerFileName].Add((sourcePath, data));
        }

        /// <summary>
        /// Gets all merged files
        /// </summary>
        public Dictionary<string, byte[]> GetMergedFiles()
        {
            // Process all collected meta files
            foreach (var kvp in _metaFiles)
            {
                var fileName = kvp.Key;
                var files = kvp.Value;

                if (files.Count == 1)
                {
                    // Single file, no merging needed
                    _mergedFiles[fileName] = files[0].data;
                }
                else if (fileName == "content.xml")
                {
                    // Special handling for content.xml
                    _mergedFiles[fileName] = MergeContentXmlFiles(files, _outputDlcName);
                }
                else if (fileName == "setup2.xml")
                {
                    // Special handling for setup2.xml
                    _mergedFiles[fileName] = MergeSetup2XmlFiles(files, _outputDlcName);
                }
                else
                {
                    // Regular meta file merging
                    _mergedFiles[fileName] = MergeMetaFiles(files, fileName);
                }
            }

            return _mergedFiles;
        }
    }
}