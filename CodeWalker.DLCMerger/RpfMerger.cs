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

        public RpfMerger(Options options, Action<string> log)
        {
            _options = options;
            _log = log;
            _analyzer = new RpfStructureAnalyzer(log);
            _contentAnalyzer = new ContentXmlAnalyzer(log);
            _nestedReader = new NestedRpfReader(log);
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

                // Step 7: Create merged RPF (unless dry run)
                if (!_options.DryRun)
                {
                    CreateMergedRpf(inputRpfs);
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
                var fullPath = Path.GetFullPath(input);
                _log($"Checking input path: {input} -> {fullPath}");
                
                if (Directory.Exists(input))
                {
                    // Recursively scan directory for RPF files
                    var rpfFiles = Directory.GetFiles(input, "*.rpf", SearchOption.AllDirectories);
                    allFiles.AddRange(rpfFiles);
                    _log($"Found {rpfFiles.Length} RPF files in directory (recursive): {input}");
                    
                    if (_options.Verbose && rpfFiles.Length > 0)
                    {
                        _log("  RPF files found:");
                        foreach (var file in rpfFiles)
                        {
                            var relativePath = Path.GetRelativePath(input, file);
                            _log($"    - {relativePath}");
                        }
                    }
                    
                    if (rpfFiles.Length == 0)
                    {
                        _log($"  No RPF files found in directory: {input}");
                    }
                }
                else if (File.Exists(input))
                {
                    // Single file
                    allFiles.Add(input);
                    _log($"Added RPF file: {input}");
                }
                else
                {
                    _log($"Warning: Path not found: {input}");
                    _log($"  Current directory: {Directory.GetCurrentDirectory()}");
                    _log($"  Full path checked: {fullPath}");
                }
            }

            _log($"Total RPF files discovered: {allFiles.Count}");
            return allFiles;
        }

        private List<RpfFile> LoadInputRpfs(List<string> inputFiles)
        {
            var rpfs = new List<RpfFile>();

            foreach (var inputFile in inputFiles)
            {
                try
                {
                    _log($"Loading RPF: {inputFile}");
                    var rpf = new RpfFile(inputFile, inputFile);
                    
                    // Scan the structure to load all entries
                    rpf.ScanStructure(null, null);
                    
                    rpfs.Add(rpf);
                    _log($"Successfully loaded: {inputFile} (Entries: {rpf.AllEntries?.Count ?? 0})");
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
            _log("\n=== DLC CONTENT ANALYSIS ===");
            
            foreach (var rpf in rpfs)
            {
                var dlcInfo = _contentAnalyzer.AnalyzeContentXml(rpf);
                _contentAnalyzer.PrintDlcInfo(dlcInfo);
            }
        }

        private void AnalyzeAndMergeStructure(List<RpfFile> rpfs)
        {
            var expandNested = _options.ExpandNested;
            _log($"Analyzing RPF structures{(expandNested ? " (including nested RPFs)" : "")}...");

            var entryPaths = new Dictionary<string, List<string>>(); // For debugging

            foreach (var rpf in rpfs)
            {
                _log($"\nProcessing RPF: {rpf.Path}");
                
                // Get entries - either with or without nested expansion
                var allEntries = expandNested ? 
                    _nestedReader.GetAllEntriesRecursive(rpf) : 
                    rpf.AllEntries?.Skip(1).ToList() ?? new List<RpfEntry>(); // Skip root

                _log($"  Total entries{(expandNested ? " (including nested)" : "")}: {allEntries.Count}");
                
                foreach (var entry in allEntries)
                {
                    var relativePath = GetRelativePath(entry);
                    
                    // Skip empty paths
                    if (string.IsNullOrEmpty(relativePath)) continue;

                    // Debug logging
                    if (_options.Verbose)
                    {
                        var depthInfo = entry is NestedRpfEntry nestedEntry ? $" [depth: {nestedEntry.NestingDepth}]" : "";
                        _log($"  Entry: {entry.Path} -> RelativePath: {relativePath} ({entry.GetType().Name}){depthInfo}");
                    }

                    // Track entry paths for debugging
                    if (!entryPaths.ContainsKey(relativePath))
                    {
                        entryPaths[relativePath] = new List<string>();
                    }
                    entryPaths[relativePath].Add(rpf.Path);

                    if (_mergedEntries.ContainsKey(relativePath))
                    {
                        // Conflict detected
                        if (!_conflictingEntries.ContainsKey(relativePath))
                        {
                            _conflictingEntries[relativePath] = new List<RpfEntry> { _mergedEntries[relativePath] };
                        }
                        _conflictingEntries[relativePath].Add(entry);

                        // Handle conflict resolution
                        ResolveConflict(relativePath, entry);

                        _log($"CONFLICT: {relativePath} (from {rpf.Path})");
                    }
                    else
                    {
                        _mergedEntries[relativePath] = entry;
                    }
                }
            }

            // Debug: Show paths that appear in multiple RPFs
            if (_options.Verbose)
            {
                _log("\n=== PATHS ANALYSIS ===");
                var multipleRpfPaths = entryPaths.Where(p => p.Value.Count > 1).ToList();
                _log($"Paths appearing in multiple RPFs: {multipleRpfPaths.Count}");
                
                foreach (var path in multipleRpfPaths.Take(15)) // Show first 15
                {
                    _log($"  {path.Key} appears in: {string.Join(", ", path.Value)}");
                }
                
                if (multipleRpfPaths.Count > 15)
                {
                    _log($"  ... and {multipleRpfPaths.Count - 15} more");
                }
            }

            _log($"Total unique entries: {_mergedEntries.Count}");
            _log($"Conflicting entries: {_conflictingEntries.Count}");
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
            if (_conflictingEntries.Count == 0)
            {
                _log("No conflicts detected.");
                return;
            }

            _log("\n=== DETAILED CONFLICTS REPORT ===");
            
            var fileConflicts = new List<string>();
            var dirConflicts = new List<string>();
            var metaConflicts = new List<string>();

            foreach (var conflict in _conflictingEntries)
            {
                var path = conflict.Key;
                var entries = conflict.Value;
                
                _log($"\nConflict: {path}");
                
                var isMetaFile = path.EndsWith(".meta", StringComparison.OrdinalIgnoreCase) || 
                                path.EndsWith(".xml", StringComparison.OrdinalIgnoreCase);
                
                var isDirectory = entries.Any(e => e is RpfDirectoryEntry);
                
                if (isMetaFile) metaConflicts.Add(path);
                else if (isDirectory) dirConflicts.Add(path);
                else fileConflicts.Add(path);

                foreach (var entry in entries)
                {
                    var typeStr = entry is RpfDirectoryEntry ? "DIR" : "FILE";
                    if (entry is RpfFileEntry fileEntry)
                    {
                        _log($"  - {typeStr} from {entry.File?.Path ?? "unknown"}: {fileEntry.GetFileSize()} bytes");
                    }
                    else
                    {
                        _log($"  - {typeStr} from {entry.File?.Path ?? "unknown"}");
                    }
                }
            }

            _log($"\n=== CONFLICT SUMMARY ===");
            _log($"File conflicts: {fileConflicts.Count}");
            _log($"Directory conflicts: {dirConflicts.Count}");
            _log($"Meta/XML conflicts: {metaConflicts.Count}");

            if (metaConflicts.Count > 0 && _options.MergeMeta)
            {
                _log($"\nMeta/XML files will be merged: {metaConflicts.Count}");
            }
        }

        private void CreateMergedRpf(List<RpfFile> sourceRpfs)
        {
            if (File.Exists(_options.OutputFile) && !_options.Force)
            {
                throw new Exception($"Output file already exists: {_options.OutputFile}. Use --force to overwrite.");
            }

            _log($"\nCreating merged RPF: {_options.OutputFile}");

            // Parse encryption type
            var encryption = ParseEncryption(_options.Encryption);

            // Create new RPF file
            var outputDir = Path.GetDirectoryName(Path.GetFullPath(_options.OutputFile)) ?? ".";
            var outputName = Path.GetFileName(_options.OutputFile);
            var outputRpf = RpfFile.CreateNew(outputDir, outputName, encryption);

            try
            {
                // Build directory structure first
                var processedDirs = new HashSet<string>();
                BuildDirectoryStructure(outputRpf.Root, processedDirs);

                // Copy all files (excluding directories)
                var filesToCopy = _mergedEntries.Where(e => e.Value is RpfFileEntry).ToList();
                int processedCount = 0;
                int totalCount = filesToCopy.Count;

                _log($"Copying {totalCount} files...");

                foreach (var kvp in filesToCopy)
                {
                    var entry = kvp.Value as RpfFileEntry;
                    var relativePath = kvp.Key;

                    if (_options.Verbose)
                    {
                        _log($"Processing: {relativePath}");
                    }

                    try
                    {
                        CopyFileToOutput(entry, outputRpf, relativePath);
                        processedCount++;
                    }
                    catch (Exception ex)
                    {
                        _log($"Error copying {relativePath}: {ex.Message}");
                    }

                    if (processedCount % 50 == 0)
                    {
                        _log($"Progress: {processedCount}/{totalCount} files processed");
                    }
                }

                // Handle meta/xml merging if enabled
                if (_options.MergeMeta)
                {
                    HandleMetaMerging(outputRpf, sourceRpfs);
                }

                // Save the RPF file
                // The RpfFile class handles saving automatically when entries are added
                _log($"\nMerge completed successfully!");
                _log($"Output file: {_options.OutputFile}");
                _log($"Total files copied: {processedCount}");
                _log($"Total directories created: {processedDirs.Count}");
            }
            finally
            {
                // RpfFile cleanup is handled automatically
            }
        }

        private void BuildDirectoryStructure(RpfDirectoryEntry rootDir, HashSet<string> processedDirs)
        {
            var directories = _mergedEntries.Where(e => e.Value is RpfDirectoryEntry).ToList();
            _log($"Creating {directories.Count} directories...");

            foreach (var entry in directories)
            {
                var dirPath = entry.Key;
                if (processedDirs.Contains(dirPath))
                    continue;

                if (_options.Verbose)
                {
                    _log($"Creating directory: {dirPath}");
                }

                CreateDirectoryPath(rootDir, dirPath);
                processedDirs.Add(dirPath);
            }
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

        private void CopyFileToOutput(RpfFileEntry sourceEntry, RpfFile outputRpf, string relativePath)
        {
            // Extract file data from source
            byte[] fileData = sourceEntry.File.ExtractFile(sourceEntry);
            if (fileData == null)
            {
                throw new Exception($"Failed to extract file data for {relativePath}");
            }

            // Determine parent directory path
            var lastSlash = relativePath.LastIndexOfAny(new[] { '\\', '/' });
            var dirPath = lastSlash >= 0 ? relativePath.Substring(0, lastSlash) : "";
            var fileName = lastSlash >= 0 ? relativePath.Substring(lastSlash + 1) : relativePath;

            // Find or create parent directory
            var parentDir = string.IsNullOrEmpty(dirPath) 
                ? outputRpf.Root 
                : FindOrCreateDirectory(outputRpf.Root, dirPath);

            // Create file in output RPF
            RpfFile.CreateFile(parentDir, fileName, fileData);
        }

        private RpfDirectoryEntry FindOrCreateDirectory(RpfDirectoryEntry root, string path)
        {
            return CreateDirectoryPath(root, path);
        }

        private void HandleMetaMerging(RpfFile outputRpf, List<RpfFile> sourceRpfs)
        {
            _log("\nMerging meta/xml files...");
            // This is where the meta/xml merging logic would go
            // For now, this is a placeholder for the second phase
            _log("Meta merging not yet implemented - using first file for conflicts");
        }

        private string GetRelativePath(RpfEntry entry)
        {
            var path = entry.Path;
            
            // Remove ALL RPF file names from the path to get the true relative path
            // This handles nested RPF structures properly
            if (path.Contains('\\'))
            {
                var parts = path.Split('\\').ToList();
                
                // Remove all parts that end with .rpf (these are RPF containers)
                var filteredParts = new List<string>();
                foreach (var part in parts)
                {
                    if (!part.EndsWith(".rpf", StringComparison.OrdinalIgnoreCase))
                    {
                        filteredParts.Add(part);
                    }
                }
                
                if (filteredParts.Count > 0)
                {
                    return string.Join('\\', filteredParts);
                }
            }
            
            // If no valid path after filtering, return the entry name
            return entry.Name ?? path;
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
    }
}