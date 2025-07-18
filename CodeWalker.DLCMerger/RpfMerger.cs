using CodeWalker.GameFiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CodeWalker.DLCMerger
{
    public class RpfMerger
    {
        private readonly Options _options;
        private readonly Action<string> _log;
        private readonly Dictionary<string, RpfEntry> _mergedEntries = new();
        private readonly Dictionary<string, List<RpfEntry>> _conflictingEntries = new();
        private readonly RpfStructureAnalyzer _analyzer;
        private readonly ContentXmlAnalyzer _contentAnalyzer;
        private readonly NestedRpfReader _nestedReader;
        private readonly SelectiveMerger _selectiveMerger;

        public RpfMerger(Options options, Action<string> log)
        {
            _options = options;
            _log = log;
            _analyzer = new RpfStructureAnalyzer(log);
            _contentAnalyzer = new ContentXmlAnalyzer(log);
            _nestedReader = new NestedRpfReader(log);
            _selectiveMerger = new SelectiveMerger(log);
        }

        public void Merge()
        {
            try
            {
                // Step 1: Discover and load all input RPF files
                var inputFiles = DiscoverInputFiles();
                if (inputFiles.Count == 0)
                {
                    throw new Exception("No RPF files found to merge.");
                }
                
                if (inputFiles.Count < 2)
                {
                    throw new Exception($"At least 2 RPF files are required for merging. Found only {inputFiles.Count} RPF file(s).");
                }

                var inputRpfs = LoadInputRpfs(inputFiles);
                if (inputRpfs.Count == 0)
                {
                    throw new Exception("No valid RPF files to merge.");
                }
                
                if (inputRpfs.Count < 2)
                {
                    throw new Exception($"At least 2 valid RPF files are required for merging. Successfully loaded only {inputRpfs.Count} RPF file(s).");
                }

                // Step 2: Analyze content.xml files for DLC information
                AnalyzeDlcContent(inputRpfs);

                // Step 3: Show structure trees if requested
                if (_options.ShowStructure)
                {
                    _analyzer.PrintAllRpfStructures(inputRpfs, _options.ExpandNested);
                }

                // Step 4: Analyze and merge file structures
                AnalyzeAndMergeStructure(inputRpfs);

                // Step 5: Show detailed analysis
                _analyzer.PrintMergedStructureAnalysis(_mergedEntries, _conflictingEntries);

                // Step 6: Report conflicts
                ReportConflicts();

                // Step 7: Create merged output (unless dry run)
                if (!_options.DryRun)
                {
                    if (_options.ExtractMode)
                    {
                        ExtractAndMergeToDirectory(inputRpfs);
                    }
                    else
                    {
                        CreateMergedRpf(inputRpfs);
                    }
                }
                else
                {
                    _log("Dry run completed. No output file created.");
                }
            }
            catch (Exception ex)
            {
                _log($"Error during merge: {ex.Message}");
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
                    // Recursively scan directory for RPF files
                    var rpfFiles = Directory.GetFiles(input, "*.rpf", SearchOption.AllDirectories);
                    allFiles.AddRange(rpfFiles);
                    if (_options.Verbose)
                    {
                        _log($"Found {rpfFiles.Length} RPF files in {input}");
                    }
                }
                else if (File.Exists(input))
                {
                    // Single file
                    allFiles.Add(input);
                }
                else
                {
                    _log($"Warning: Path not found: {input}");
                }
            }

            if (_options.Verbose)
            {
                _log($"Found {allFiles.Count} RPF files total");
            }
            return allFiles;
        }

        private List<RpfFile> LoadInputRpfs(List<string> inputFiles)
        {
            var rpfs = new List<RpfFile>();

            foreach (var inputFile in inputFiles)
            {
                try
                {
                    var rpf = new RpfFile(inputFile, inputFile);
                    rpf.ScanStructure(null, null);
                    rpfs.Add(rpf);
                    
                    if (_options.Verbose)
                    {
                        _log($"Loaded {Path.GetFileName(inputFile)} ({rpf.AllEntries?.Count ?? 0} entries)");
                    }
                }
                catch (Exception ex)
                {
                    _log($"Error loading RPF {inputFile}: {ex.Message}");
                }
            }

            return rpfs;
        }

        private void AnalyzeDlcContent(List<RpfFile> rpfs)
        {
            if (_options.Verbose)
            {
                foreach (var rpf in rpfs)
                {
                    var dlcInfo = _contentAnalyzer.AnalyzeContentXml(rpf);
                    _contentAnalyzer.PrintDlcInfo(dlcInfo);
                }
            }
        }

        private void AnalyzeAndMergeStructure(List<RpfFile> rpfs)
        {
            var expandNested = _options.ExpandNested;

            // First pass: Analyze vehicles.meta files to determine what vehicles we're merging
            foreach (var rpf in rpfs)
            {
                var vehiclesMetaEntry = FindVehiclesMetaEntry(rpf);
                if (vehiclesMetaEntry != null && vehiclesMetaEntry is RpfFileEntry fileEntry)
                {
                    try
                    {
                        var fileData = rpf.ExtractFile(fileEntry);
                        if (fileData != null)
                        {
                            _selectiveMerger.AnalyzeVehiclesMeta(fileData);
                        }
                    }
                    catch (Exception ex)
                    {
                        _log($"Error analyzing vehicles.meta from {rpf.Name}: {ex.Message}");
                    }
                }
            }

            // Second pass: Collect entries with selective filtering
            foreach (var rpf in rpfs)
            {
                var allEntries = expandNested ? 
                    _nestedReader.GetAllEntriesRecursive(rpf) : 
                    rpf.AllEntries?.Skip(1).ToList() ?? new List<RpfEntry>(); // Skip root
                
                foreach (var entry in allEntries)
                {
                    var relativePath = GetRelativePath(entry);
                    
                    // Skip empty paths
                    if (string.IsNullOrEmpty(relativePath)) continue;

                    // Apply selective filtering for file entries
                    if (entry is RpfFileEntry && !_options.MergeAll)
                    {
                        var fileName = Path.GetFileName(relativePath);
                        if (!_selectiveMerger.ShouldIncludeFile(relativePath, fileName))
                        {
                            if (_options.Verbose)
                            {
                                _log($"  Skipping non-vehicle file: {relativePath}");
                            }
                            continue;
                        }
                    }

                    if (_mergedEntries.ContainsKey(relativePath))
                    {
                        var existingEntry = _mergedEntries[relativePath];
                        
                        // Directory vs Directory: Not a real conflict, just merge
                        if (existingEntry is RpfDirectoryEntry && entry is RpfDirectoryEntry)
                        {
                            // Keep the existing directory entry, no conflict
                            continue;
                        }
                        
                        // File conflict detected
                        if (!_conflictingEntries.ContainsKey(relativePath))
                        {
                            _conflictingEntries[relativePath] = new List<RpfEntry> { existingEntry };
                        }
                        _conflictingEntries[relativePath].Add(entry);

                        ResolveConflict(relativePath, entry);
                    }
                    else
                    {
                        _mergedEntries[relativePath] = entry;
                    }
                }
            }

            _log($"Analyzed {_mergedEntries.Count} unique entries, {_conflictingEntries.Count} conflicts");
        }

        private RpfEntry? FindVehiclesMetaEntry(RpfFile rpf)
        {
            var allEntries = rpf.AllEntries;
            if (allEntries == null) return null;

            foreach (var entry in allEntries)
            {
                if (entry is RpfFileEntry fileEntry && 
                    fileEntry.Name.Equals("vehicles.meta", StringComparison.OrdinalIgnoreCase))
                {
                    return fileEntry;
                }
            }

            return null;
        }

        private void ResolveConflict(string path, RpfEntry newEntry)
        {
            var existingEntry = _mergedEntries[path];

            // Directory conflicts: merge directories, don't treat as file conflicts
            if (existingEntry is RpfDirectoryEntry && newEntry is RpfDirectoryEntry)
            {
                // Directories should be merged, not conflicted
                // Keep the existing directory entry
                return;
            }

            // For files, keep the first one (can be enhanced later)
            // In the future, this is where meta/xml merging would happen
            if (existingEntry is RpfFileEntry && newEntry is RpfFileEntry)
            {
                // Keep existing file for now
                return;
            }

            // Mixed types (file vs directory) - prefer directory
            if (existingEntry is RpfFileEntry && newEntry is RpfDirectoryEntry)
            {
                _mergedEntries[path] = newEntry;
            }
        }

        private void ReportConflicts()
        {
            if (_conflictingEntries.Count == 0) return;

            var metaConflicts = _conflictingEntries.Where(c => 
                c.Key.EndsWith(".meta", StringComparison.OrdinalIgnoreCase) || 
                c.Key.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)).Count();
            
            _log($"Found {_conflictingEntries.Count} conflicts ({metaConflicts} meta files)");
            
            if (_options.Verbose)
            {
                foreach (var conflict in _conflictingEntries.Take(10))
                {
                    _log($"  Conflict: {conflict.Key}");
                }
                if (_conflictingEntries.Count > 10)
                {
                    _log($"  ... and {_conflictingEntries.Count - 10} more");
                }
            }
        }

        private void CreateMergedRpf(List<RpfFile> sourceRpfs)
        {
            if (File.Exists(_options.OutputFile) && !_options.Force)
            {
                throw new Exception($"Output file already exists: {_options.OutputFile}. Use --force to overwrite.");
            }

            _log($"Creating merged RPF: {_options.OutputFile}");

            // Parse encryption type
            var encryption = ParseEncryption(_options.Encryption);

            // Create new RPF file
            var outputDir = Path.GetDirectoryName(Path.GetFullPath(_options.OutputFile)) ?? ".";
            var outputName = Path.GetFileName(_options.OutputFile);
            var outputRpf = RpfFile.CreateNew(outputDir, outputName, encryption);

            try
            {
                // Create container manager
                var containerManager = new RpfContainerManager(outputRpf, _log);
                
                // Group files by their classification
                var classifiedFiles = new Dictionary<string, (RpfFileEntry entry, FileClassifier.ClassificationResult classification)>();
                var metaFiles = new List<(string path, RpfFileEntry entry)>();
                
                // First pass: classify all files (excluding conflicting meta files that will be merged)
                foreach (var kvp in _mergedEntries.Where(e => e.Value is RpfFileEntry))
                {
                    var entry = kvp.Value as RpfFileEntry;
                    if (entry == null) continue; // Skip if cast failed
                    
                    var relativePath = kvp.Key;
                    var fileName = Path.GetFileName(relativePath);
                    
                    // Skip meta files that have conflicts - they'll be handled by MetaMerger
                    if (_conflictingEntries.ContainsKey(relativePath) && 
                        (fileName.EndsWith(".meta", StringComparison.OrdinalIgnoreCase) || 
                         fileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)))
                    {
                        if (_options.Verbose)
                        {
                            _log($"  Skipping conflicting meta file for merging: {relativePath}");
                        }
                        continue;
                    }
                    
                    var classification = FileClassifier.ClassifyFile(relativePath);
                    classifiedFiles[relativePath] = (entry, classification);
                    
                    // Collect meta files for potential merging
                    if (classification.Category == FileClassifier.FileCategory.Meta)
                    {
                        metaFiles.Add((relativePath, entry));
                    }
                    
                    if (_options.Verbose)
                    {
                        _log($"  {relativePath} -> {classification.Category} ({classification.TargetContainer ?? "root"})");
                    }
                }
                
                CreateDirectoryStructure(outputRpf);
                
                int processedCount = 0;
                int totalCount = classifiedFiles.Count;
                _log($"Processing {totalCount} files...");
                
                foreach (var kvp in classifiedFiles)
                {
                    var relativePath = kvp.Key;
                    var (entry, classification) = kvp.Value;
                    
                    
                    try
                    {
                        containerManager.AddFileToContainer(entry, classification);
                        processedCount++;
                    }
                    catch (Exception ex)
                    {
                        _log($"Error copying {relativePath}: {ex.Message}");
                    }
                    
                    if (processedCount % 100 == 0 && _options.Verbose)
                    {
                        _log($"  Progress: {processedCount}/{totalCount}");
                    }
                }
                
                // Always handle meta/xml merging for conflicts
                HandleMetaMerging(outputRpf, sourceRpfs);

                // Handle content.xml and setup2.xml specially for selective merging
                if (!_options.MergeAll)
                {
                    HandleSelectiveManifestFiles(outputRpf, sourceRpfs);
                }
                
                // Finalize containers
                containerManager.FinalizeContainers();
                
                // Get and display statistics
                var containerStats = containerManager.GetContainerStats();
                
                _log($"Merge completed: {processedCount} files -> {_options.OutputFile}");
                
                if (containerStats.Count > 0)
                {
                    var summary = string.Join(", ", containerStats.Select(s => $"{s.Key}: {s.Value}"));
                    _log($"Containers: {summary}");
                }
            }
            finally
            {
                // RpfFile cleanup is handled automatically
            }
        }

        private void CreateDirectoryStructure(RpfFile outputRpf)
        {
            // Create the main data directory
            var dataDir = new RpfDirectoryEntry
            {
                Name = "data",
                NameLower = "data",
                Parent = outputRpf.Root,
                File = outputRpf,
                Path = outputRpf.Root.Path + "\\data"
            };
            outputRpf.Root.Directories.Add(dataDir);
            
            _log("Created base directory structure");
        }

        private RpfDirectoryEntry CreateDirectoryPath(RpfDirectoryEntry root, string path)
        {
            if (string.IsNullOrEmpty(path))
                return root;

            var parts = path.Split('\\', '/');
            var current = root;

            foreach (var part in parts)
            {
                if (string.IsNullOrEmpty(part))
                    continue;

                // Check if directory already exists
                var existing = current.Directories.FirstOrDefault(d => 
                    d.Name.Equals(part, StringComparison.OrdinalIgnoreCase));

                if (existing != null)
                {
                    current = existing;
                }
                else
                {
                    // Create new directory
                    var newDir = new RpfDirectoryEntry();
                    newDir.Name = part;
                    newDir.NameLower = part.ToLowerInvariant();
                    newDir.Parent = current;
                    newDir.File = root.File;
                    newDir.Path = current.Path + "\\" + part;

                    current.Directories.Add(newDir);
                    current = newDir;
                }
            }

            return current;
        }


        private void HandleMetaMerging(RpfFile outputRpf, List<RpfFile> sourceRpfs)
        {
            // Extract DLC name from output file
            var outputFileName = Path.GetFileNameWithoutExtension(_options.OutputFile);
            var dlcName = outputFileName.StartsWith("dlc_") ? outputFileName.Substring(4) : outputFileName;
            
            var merger = new MetaMerger(_log, _options.MergeAll ? null : _selectiveMerger, dlcName);
            var metaFilesToMerge = new Dictionary<string, List<(RpfFileEntry entry, string source)>>();
            
            // Group conflicting meta files by name
            foreach (var conflict in _conflictingEntries)
            {
                var path = conflict.Key;
                var entries = conflict.Value;
                
                var fileName = Path.GetFileName(path);
                var isMetaFile = fileName.EndsWith(".meta", StringComparison.OrdinalIgnoreCase) || 
                                fileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase);
                
                if (isMetaFile && entries.All(e => e is RpfFileEntry))
                {
                    if (!metaFilesToMerge.ContainsKey(fileName))
                    {
                        metaFilesToMerge[fileName] = new List<(RpfFileEntry, string)>();
                    }
                    
                    foreach (var entry in entries.Cast<RpfFileEntry>())
                    {
                        metaFilesToMerge[fileName].Add((entry, entry.File?.Path ?? "unknown"));
                    }
                }
            }
            
            // Add meta files to merger
            foreach (var kvp in metaFilesToMerge)
            {
                var fileName = kvp.Key;
                var fileEntries = kvp.Value;
                
                if (_options.Verbose)
                {
                    _log($"  Merging {fileName} from {fileEntries.Count} sources");
                }
                
                foreach (var (entry, source) in fileEntries)
                {
                    try
                    {
                        var fileData = entry.File.ExtractFile(entry);
                        if (fileData != null)
                        {
                            merger.AddMetaFile(fileName, fileData, source);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (_options.Verbose)
                        {
                            _log($"  Error extracting {fileName}: {ex.Message}");
                        }
                    }
                }
            }
            
            // Get merged files and write them to output
            var mergedFiles = merger.GetMergedFiles();
            var dataDir = outputRpf.Root.Directories.FirstOrDefault(d => d.Name == "data");
            
            if (dataDir == null)
            {
                _log("Warning: data directory not found in output RPF");
                return;
            }
            
            foreach (var kvp in mergedFiles)
            {
                var fileName = kvp.Key;
                var fileData = kvp.Value;
                
                try
                {
                    // Determine subdirectory based on file type
                    var targetDir = dataDir;
                    var classification = FileClassifier.ClassifyFile(Path.Combine("data", fileName));
                    
                    if (classification.TargetPath.Contains("\\"))
                    {
                        // Create subdirectories if needed
                        var pathParts = classification.TargetPath.Split('\\');
                        for (int i = 1; i < pathParts.Length - 1; i++) // Skip "data" and filename
                        {
                            var dirName = pathParts[i];
                            var subDir = targetDir.Directories.FirstOrDefault(d => d.Name == dirName);
                            if (subDir == null)
                            {
                                subDir = new RpfDirectoryEntry
                                {
                                    Name = dirName,
                                    NameLower = dirName.ToLowerInvariant(),
                                    Parent = targetDir,
                                    File = outputRpf,
                                    Path = targetDir.Path + "\\" + dirName
                                };
                                targetDir.Directories.Add(subDir);
                            }
                            targetDir = subDir;
                        }
                    }
                    
                    // Create the merged file
                    RpfFile.CreateFile(targetDir, fileName, fileData);
                }
                catch (Exception ex)
                {
                    if (_options.Verbose)
                    {
                        _log($"  Error creating {fileName}: {ex.Message}");
                    }
                }
            }
            
            if (mergedFiles.Count > 0)
            {
                _log($"Merged {mergedFiles.Count} meta files");
            }
        }

        private string GetRelativePath(RpfEntry entry)
        {
            var path = entry.Path;
            
            // We need to preserve the internal structure but remove only the source DLC RPF name
            if (path.Contains('\\'))
            {
                var parts = path.Split('\\').ToList();
                
                // Find and remove only the first RPF (the source DLC RPF)
                // But preserve internal RPF containers like vehicles.rpf, weapons.rpf
                var startIndex = 0;
                for (int i = 0; i < parts.Count; i++)
                {
                    if (parts[i].EndsWith(".rpf", StringComparison.OrdinalIgnoreCase))
                    {
                        startIndex = i + 1;
                        break;
                    }
                }
                
                if (startIndex < parts.Count)
                {
                    var relativeParts = parts.Skip(startIndex).ToList();
                    
                    // Preserve important directory structures
                    var preservedPath = string.Join('\\', relativeParts);
                    
                    // Handle special cases where we need to adjust the path
                    if (preservedPath.StartsWith("common\\data\\", StringComparison.OrdinalIgnoreCase))
                    {
                        // Remove "common\\" prefix for data files
                        preservedPath = preservedPath.Substring(7); // Remove "common\\"
                    }
                    else if (preservedPath.StartsWith("x64\\", StringComparison.OrdinalIgnoreCase))
                    {
                        // Preserve x64 structure for non-data files
                        // No modification needed
                    }
                    
                    return preservedPath;
                }
            }
            
            // If no valid path after filtering, return the entry name
            return entry.Name ?? path;
        }

        private void HandleSelectiveManifestFiles(RpfFile outputRpf, List<RpfFile> sourceRpfs)
        {
            try
            {
                // Find the first content.xml from source RPFs
                RpfFileEntry? contentXmlEntry = null;
                RpfFile? sourceRpf = null;

                foreach (var rpf in sourceRpfs)
                {
                    var entry = rpf.AllEntries?.FirstOrDefault(e => 
                        e is RpfFileEntry fe && 
                        fe.Name.Equals("content.xml", StringComparison.OrdinalIgnoreCase));
                    
                    if (entry != null)
                    {
                        contentXmlEntry = entry as RpfFileEntry;
                        sourceRpf = rpf;
                        break;
                    }
                }

                if (contentXmlEntry != null && sourceRpf != null)
                {
                    var contentData = sourceRpf.ExtractFile(contentXmlEntry);
                    if (contentData != null)
                    {
                        // Create selective content.xml
                        var selectiveContent = _selectiveMerger.CreateSelectiveContentXml(
                            contentData, 
                            Path.GetFileNameWithoutExtension(_options.OutputFile));

                        // Write the selective content.xml to the output
                        RpfFile.CreateFile(outputRpf.Root, "content.xml", selectiveContent);
                        _log("Created selective content.xml with only vehicle-related entries");
                    }
                }

                // Also handle setup2.xml - we can keep it mostly as-is since it just references content.xml
                RpfFileEntry? setup2Entry = null;
                sourceRpf = null;

                foreach (var rpf in sourceRpfs)
                {
                    var entry = rpf.AllEntries?.FirstOrDefault(e => 
                        e is RpfFileEntry fe && 
                        fe.Name.Equals("setup2.xml", StringComparison.OrdinalIgnoreCase));
                    
                    if (entry != null)
                    {
                        setup2Entry = entry as RpfFileEntry;
                        sourceRpf = rpf;
                        break;
                    }
                }

                if (setup2Entry != null && sourceRpf != null)
                {
                    var setup2Data = sourceRpf.ExtractFile(setup2Entry);
                    if (setup2Data != null)
                    {
                        // For now, just copy setup2.xml as-is
                        // In a more advanced implementation, we would update the deviceName and nameHash
                        RpfFile.CreateFile(outputRpf.Root, "setup2.xml", setup2Data);
                        _log("Copied setup2.xml");
                    }
                }
            }
            catch (Exception ex)
            {
                _log($"Error handling selective manifest files: {ex.Message}");
            }
        }

        private RpfEncryption ParseEncryption(string encryptionStr)
        {
            return encryptionStr.ToUpper() switch
            {
                "NONE" => RpfEncryption.NONE,
                "OPEN" => RpfEncryption.OPEN,
                "AES" => RpfEncryption.AES,
                "NG" => RpfEncryption.NG,
                _ => RpfEncryption.OPEN
            };
        }

        private void ExtractAndMergeToDirectory(List<RpfFile> sourceRpfs)
        {
            _log($"Extracting and merging to directory: {_options.OutputFile}");

            // Create output directory
            var outputDir = _options.OutputFile;
            if (Directory.Exists(outputDir))
            {
                if (!_options.Force)
                {
                    throw new Exception($"Output directory already exists: {outputDir}. Use --force to overwrite.");
                }
                Directory.Delete(outputDir, true);
            }
            Directory.CreateDirectory(outputDir);

            // Create subdirectories
            var dlcDir = Path.Combine(outputDir, "dlc.rpf");
            Directory.CreateDirectory(dlcDir);
            var dataDir = Path.Combine(dlcDir, "data");
            Directory.CreateDirectory(dataDir);
            var vehiclesDir = Path.Combine(dlcDir, "vehicles.rpf");
            Directory.CreateDirectory(vehiclesDir);
            var weaponsDir = Path.Combine(dlcDir, "weapons.rpf");
            Directory.CreateDirectory(weaponsDir);

            // Extract DLC name from output directory name
            var outputDirName = Path.GetFileName(outputDir);
            var dlcName = outputDirName.StartsWith("dlc_") ? outputDirName.Substring(4) : outputDirName;

            // Collect all meta files for merging
            var metaFilesToMerge = new Dictionary<string, List<(string sourcePath, byte[] data)>>();
            var vehicleFiles = new List<(string fileName, byte[] data, string source)>();
            var weaponFiles = new List<(string fileName, byte[] data, string source)>();

            // Process each source RPF
            foreach (var rpf in sourceRpfs)
            {
                _log($"Processing {rpf.Name}...");
                
                var allEntries = _options.ExpandNested ? 
                    _nestedReader.GetAllEntriesRecursive(rpf) : 
                    rpf.AllEntries?.Skip(1).ToList() ?? new List<RpfEntry>();

                foreach (var entry in allEntries.Where(e => e is RpfFileEntry))
                {
                    var fileEntry = entry as RpfFileEntry;
                    if (fileEntry == null) continue;

                    var relativePath = GetRelativePath(entry);
                    var fileName = Path.GetFileName(relativePath);

                    // Skip non-vehicle files if not merging all
                    if (!_options.MergeAll && !_selectiveMerger.ShouldIncludeFile(relativePath, fileName))
                    {
                        continue;
                    }

                    try
                    {
                        var data = rpf.ExtractFile(fileEntry);
                        if (data == null) continue;

                        // Categorize files
                        if (fileName.EndsWith(".meta", StringComparison.OrdinalIgnoreCase) || 
                            fileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                        {
                            // Meta/XML files - collect for merging
                            var metaFileName = fileName.ToLowerInvariant();
                            
                            // Handle special meta files that should not be merged
                            if (metaFileName.StartsWith("vehicleweapons_") || 
                                metaFileName == "vehiclelayouts.meta" ||
                                metaFileName == "explosion.meta")
                            {
                                // These files are vehicle-specific, don't merge them
                                var uniqueName = $"{Path.GetFileNameWithoutExtension(rpf.Name)}_{metaFileName}";
                                metaFilesToMerge[uniqueName] = new List<(string, byte[])> { (rpf.Name, data) };
                            }
                            else
                            {
                                if (!metaFilesToMerge.ContainsKey(metaFileName))
                                {
                                    metaFilesToMerge[metaFileName] = new List<(string, byte[])>();
                                }
                                metaFilesToMerge[metaFileName].Add((rpf.Name, data));
                            }
                        }
                        else if (relativePath.Contains("vehicles.rpf"))
                        {
                            // Vehicle model files
                            vehicleFiles.Add((fileName, data, rpf.Name));
                        }
                        else if (relativePath.Contains("weapons.rpf") || fileName.StartsWith("w_"))
                        {
                            // Weapon files
                            weaponFiles.Add((fileName, data, rpf.Name));
                        }
                    }
                    catch (Exception ex)
                    {
                        _log($"Error extracting {fileName}: {ex.Message}");
                    }
                }
            }

            // Merge meta files
            var merger = new MetaMerger(_log, _selectiveMerger, dlcName);
            foreach (var kvp in metaFilesToMerge)
            {
                var fileName = kvp.Key;
                var files = kvp.Value;

                _log($"Merging {fileName} from {files.Count} sources");

                byte[] mergedData;
                if (files.Count == 1)
                {
                    mergedData = files[0].data;
                }
                else if (fileName == "content.xml")
                {
                    mergedData = merger.MergeContentXmlFiles(files, dlcName);
                }
                else if (fileName == "setup2.xml")
                {
                    mergedData = merger.MergeSetup2XmlFiles(files, dlcName);
                }
                else
                {
                    mergedData = merger.MergeMetaFiles(files, fileName);
                }

                // Determine output path and final filename
                string outputPath;
                string outputFileName;
                
                // Check if this was a unique file (not merged)
                if (fileName.Contains("_vehicleweapons_") || 
                    fileName.Contains("_vehiclelayouts.meta") ||
                    fileName.Contains("_explosion.meta"))
                {
                    // Remove the prefix for the output
                    var underscoreIndex = fileName.IndexOf('_');
                    outputFileName = underscoreIndex > 0 ? fileName.Substring(underscoreIndex + 1) : fileName;
                }
                else
                {
                    outputFileName = fileName;
                }
                
                if (outputFileName == "content.xml" || outputFileName == "setup2.xml")
                {
                    outputPath = Path.Combine(dlcDir, outputFileName);
                }
                else
                {
                    outputPath = Path.Combine(dataDir, outputFileName);
                }

                File.WriteAllBytes(outputPath, mergedData);
                _log($"Created {outputPath}");
            }

            // Write vehicle files
            foreach (var (fileName, data, source) in vehicleFiles)
            {
                var outputPath = Path.Combine(vehiclesDir, fileName);
                File.WriteAllBytes(outputPath, data);
                if (_options.Verbose)
                {
                    _log($"Extracted {fileName} from {source}");
                }
            }

            // Write weapon files
            foreach (var (fileName, data, source) in weaponFiles)
            {
                var outputPath = Path.Combine(weaponsDir, fileName);
                File.WriteAllBytes(outputPath, data);
                if (_options.Verbose)
                {
                    _log($"Extracted {fileName} from {source}");
                }
            }

            _log($"Extraction completed to {outputDir}");
            _log($"Merged {metaFilesToMerge.Count} meta files");
            _log($"Extracted {vehicleFiles.Count} vehicle files");
            _log($"Extracted {weaponFiles.Count} weapon files");
        }
    }
}