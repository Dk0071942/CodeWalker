using CodeWalker.GameFiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CodeWalker.DLCMerger
{
    /// <summary>
    /// Simplified RPF merger that extracts to directory structure only
    /// </summary>
    public class SimplifiedRpfMerger
    {
        private readonly Options _options;
        private readonly Action<string> _log;
        private readonly ModelExtractor _modelExtractor;
        private readonly XmlMerger _xmlMerger;
        private readonly SelectiveMerger _selectiveMerger;

        public SimplifiedRpfMerger(Options options, Action<string> log)
        {
            _options = options;
            _log = log;
            _modelExtractor = new ModelExtractor(log);
            _xmlMerger = new XmlMerger(log);
            _selectiveMerger = new SelectiveMerger(log);
        }

        public void Merge()
        {
            try
            {
                _log("=== Starting Simplified RPF Merger ===");
                
                // Step 1: Discover and load input RPF files
                var inputFiles = DiscoverInputFiles();
                if (inputFiles.Count < 2)
                {
                    throw new Exception($"At least 2 RPF files required. Found {inputFiles.Count}");
                }
                
                _log($"Found {inputFiles.Count} RPF files to merge");
                
                var inputRpfs = LoadInputRpfs(inputFiles);
                _log($"Successfully loaded {inputRpfs.Count} RPF files");
                
                // Step 2: Create output directory structure
                var outputDir = _options.OutputFile;
                if (Directory.Exists(outputDir) && !_options.Force)
                {
                    throw new Exception($"Output directory already exists: {outputDir}. Use --force to overwrite.");
                }
                
                if (Directory.Exists(outputDir))
                {
                    Directory.Delete(outputDir, true);
                }
                
                CreateOutputStructure(outputDir);
                _log($"Created output directory structure at {outputDir}");
                
                // Step 3: Extract model files first (vehicles.rpf and weapons.rpf)
                _log("\n=== Phase 1: Extracting Model Files ===");
                ExtractModelFiles(inputRpfs, outputDir);
                
                // Step 4: Analyze vehicles.meta to determine what vehicles we're merging
                _log("\n=== Phase 2: Analyzing Vehicle Metadata ===");
                AnalyzeVehicleMetadata(inputRpfs);
                
                // Step 5: Extract and merge XML/meta files
                _log("\n=== Phase 3: Merging XML/Meta Files ===");
                MergeXmlFiles(inputRpfs, outputDir);
                
                // Step 6: Generate content.xml and setup2.xml
                _log("\n=== Phase 4: Generating Manifest Files ===");
                GenerateManifestFiles(outputDir);
                
                _log("\n=== Merge Complete ===");
                _log($"Output directory: {outputDir}");
            }
            catch (Exception ex)
            {
                _log($"ERROR: {ex.Message}");
                throw;
            }
        }

        private List<string> DiscoverInputFiles()
        {
            var allFiles = new List<string>();
            
            foreach (var input in _options.InputFiles)
            {
                if (Directory.Exists(input))
                {
                    var rpfFiles = Directory.GetFiles(input, "*.rpf", SearchOption.AllDirectories);
                    allFiles.AddRange(rpfFiles);
                    _log($"  Found {rpfFiles.Length} RPF files in directory: {input}");
                }
                else if (File.Exists(input) && input.EndsWith(".rpf", StringComparison.OrdinalIgnoreCase))
                {
                    allFiles.Add(input);
                    _log($"  Added RPF file: {input}");
                }
            }
            
            return allFiles;
        }

        private List<RpfFile> LoadInputRpfs(List<string> inputFiles)
        {
            var rpfs = new List<RpfFile>();
            
            foreach (var file in inputFiles)
            {
                try
                {
                    var rpf = new RpfFile(file, file);
                    rpf.ScanStructure(null, null);
                    rpfs.Add(rpf);
                    _log($"  Loaded: {Path.GetFileName(file)} ({rpf.AllEntries?.Count ?? 0} entries)");
                }
                catch (Exception ex)
                {
                    _log($"  ERROR loading {file}: {ex.Message}");
                }
            }
            
            return rpfs;
        }

        private void CreateOutputStructure(string outputDir)
        {
            // Create main directory structure
            Directory.CreateDirectory(outputDir);
            
            var dlcDir = Path.Combine(outputDir, "dlc.rpf");
            Directory.CreateDirectory(dlcDir);
            
            var dataDir = Path.Combine(dlcDir, "data");
            Directory.CreateDirectory(dataDir);
            
            var vehiclesDir = Path.Combine(dlcDir, "vehicles.rpf");
            Directory.CreateDirectory(vehiclesDir);
            
            var weaponsDir = Path.Combine(dlcDir, "weapons.rpf");
            Directory.CreateDirectory(weaponsDir);
            
            _log("  Created directories: dlc.rpf/, data/, vehicles.rpf/, weapons.rpf/");
        }

        private void ExtractModelFiles(List<RpfFile> inputRpfs, string outputDir)
        {
            var vehiclesDir = Path.Combine(outputDir, "dlc.rpf", "vehicles.rpf");
            var weaponsDir = Path.Combine(outputDir, "dlc.rpf", "weapons.rpf");
            
            foreach (var rpf in inputRpfs)
            {
                _log($"\nProcessing models from: {Path.GetFileName(rpf.Path)}");
                var modelStats = _modelExtractor.ExtractModels(rpf, vehiclesDir, weaponsDir, _options.MergeAll);
                
                _log($"  Extracted: {modelStats.VehicleModels} vehicle models, {modelStats.WeaponModels} weapon models");
            }
        }

        private void AnalyzeVehicleMetadata(List<RpfFile> inputRpfs)
        {
            foreach (var rpf in inputRpfs)
            {
                var vehiclesMetaEntry = FindFileEntry(rpf, "vehicles.meta");
                if (vehiclesMetaEntry != null)
                {
                    try
                    {
                        var data = rpf.ExtractFile(vehiclesMetaEntry);
                        if (data != null)
                        {
                            _selectiveMerger.AnalyzeVehiclesMeta(data);
                            _log($"  Analyzed vehicles.meta from {Path.GetFileName(rpf.Path)}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _log($"  ERROR analyzing vehicles.meta: {ex.Message}");
                    }
                }
            }
        }

        private void MergeXmlFiles(List<RpfFile> inputRpfs, string outputDir)
        {
            var dataDir = Path.Combine(outputDir, "dlc.rpf", "data");
            
            // Extract DLC name for the merger
            var outputDirName = Path.GetFileName(outputDir);
            var dlcName = outputDirName.StartsWith("dlc_") ? outputDirName.Substring(4) : outputDirName;
            
            _xmlMerger.Initialize(dlcName, _options.MergeAll ? null : _selectiveMerger);
            
            // Collect all XML/meta files from all RPFs
            foreach (var rpf in inputRpfs)
            {
                _log($"\nCollecting XML files from: {Path.GetFileName(rpf.Path)}");
                CollectXmlFiles(rpf, _xmlMerger);
            }
            
            // Merge and write files
            _log("\nMerging collected XML files...");
            var mergedFiles = _xmlMerger.GetMergedFiles();
            
            foreach (var kvp in mergedFiles)
            {
                var fileName = kvp.Key;
                var fileData = kvp.Value;
                
                // All files go directly in data/ folder - no subdirectories
                var outputPath = Path.Combine(dataDir, Path.GetFileName(fileName));
                
                File.WriteAllBytes(outputPath, fileData);
                _log($"  Created: {Path.GetFileName(fileName)}");
            }
            
            _log($"Merged {mergedFiles.Count} XML/meta files");
        }

        private void CollectXmlFiles(RpfFile rpf, XmlMerger xmlMerger)
        {
            var allEntries = rpf.AllEntries?.Where(e => e is RpfFileEntry).Cast<RpfFileEntry>() ?? new List<RpfFileEntry>();
            
            foreach (var entry in allEntries)
            {
                var fileName = entry.Name.ToLowerInvariant();
                
                // Skip non-XML/meta files
                if (!fileName.EndsWith(".meta") && !fileName.EndsWith(".xml"))
                    continue;
                
                // Skip content.xml and setup2.xml - these are manifest files, not data files
                if (fileName.Equals("content.xml", StringComparison.OrdinalIgnoreCase) || 
                    fileName.Equals("setup2.xml", StringComparison.OrdinalIgnoreCase))
                    continue;
                
                // Skip if selective merge is enabled and file shouldn't be included
                if (!_options.MergeAll && !_selectiveMerger.ShouldIncludeFile(entry.Path, fileName))
                    continue;
                
                try
                {
                    var data = rpf.ExtractFile(entry);
                    if (data != null)
                    {
                        // Don't preserve subdirectory structure - all files go directly in data/
                        // This includes files from ai/ subdirectory
                        
                        xmlMerger.AddFile(fileName, data, Path.GetFileName(rpf.Path));
                        _log($"  Collected: {fileName}");
                    }
                }
                catch (Exception ex)
                {
                    _log($"  ERROR extracting {fileName}: {ex.Message}");
                }
            }
        }

        private void GenerateManifestFiles(string outputDir)
        {
            var dlcDir = Path.Combine(outputDir, "dlc.rpf");
            var dataDir = Path.Combine(dlcDir, "data");
            
            // Extract DLC name
            var outputDirName = Path.GetFileName(outputDir);
            var dlcName = outputDirName.StartsWith("dlc_") ? outputDirName.Substring(4) : outputDirName;
            
            // Generate content.xml
            var contentXmlPath = Path.Combine(dlcDir, "content.xml");
            var contentXml = _xmlMerger.GenerateContentXml(dlcName);
            File.WriteAllText(contentXmlPath, contentXml);
            _log("  Created: content.xml");
            
            // Generate setup2.xml
            var setup2XmlPath = Path.Combine(dlcDir, "setup2.xml");
            var setup2Xml = _xmlMerger.GenerateSetup2Xml(dlcName);
            File.WriteAllText(setup2XmlPath, setup2Xml);
            _log("  Created: setup2.xml");
        }

        private RpfFileEntry? FindFileEntry(RpfFile rpf, string fileName)
        {
            return rpf.AllEntries?
                .OfType<RpfFileEntry>()
                .FirstOrDefault(e => e.Name.Equals(fileName, StringComparison.OrdinalIgnoreCase));
        }

        private string GetRelativePath(string fullPath)
        {
            // Extract relative path from full RPF path
            if (fullPath.Contains("\\common\\data\\"))
            {
                var index = fullPath.IndexOf("\\common\\data\\");
                return fullPath.Substring(index + "\\common\\".Length);
            }
            return fullPath;
        }
    }
}