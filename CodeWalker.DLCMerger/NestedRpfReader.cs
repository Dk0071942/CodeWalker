using CodeWalker.GameFiles;
using System;
using System.Collections.Generic;
using System.IO;

namespace CodeWalker.DLCMerger
{
    public class NestedRpfReader
    {
        private readonly Action<string> _log;
        private readonly Dictionary<string, RpfFile> _nestedRpfCache = new();

        public NestedRpfReader(Action<string> log)
        {
            _log = log;
        }

        public List<RpfEntry> GetAllEntriesRecursive(RpfFile rootRpf, int maxDepth = 3)
        {
            var allEntries = new List<RpfEntry>();
            ProcessRpfRecursive(rootRpf, allEntries, "", 0, maxDepth);
            return allEntries;
        }

        private void ProcessRpfRecursive(RpfFile rpf, List<RpfEntry> allEntries, string basePath, int currentDepth, int maxDepth)
        {
            if (currentDepth >= maxDepth || rpf.AllEntries == null)
                return;

            foreach (var entry in rpf.AllEntries)
            {
                // Skip root entry
                if (entry == rpf.Root) continue;

                // Create the full path for this entry
                var entryPath = string.IsNullOrEmpty(basePath) ? entry.Path : $"{basePath}\\{entry.Name}";
                
                // Add this entry to our list
                allEntries.Add(new NestedRpfEntry(entry, entryPath, currentDepth));

                // If this entry is an RPF file, try to read its contents
                if (entry is RpfFileEntry fileEntry && 
                    entry.Name.EndsWith(".rpf", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        var nestedRpf = LoadNestedRpf(fileEntry, entryPath);
                        if (nestedRpf != null)
                        {
                            // Recursively process the nested RPF
                            ProcessRpfRecursive(nestedRpf, allEntries, entryPath, currentDepth + 1, maxDepth);
                        }
                    }
                    catch (Exception ex)
                    {
                        _log($"Warning: Could not read nested RPF {entry.Name}: {ex.Message}");
                    }
                }
            }
        }

        private RpfFile? LoadNestedRpf(RpfFileEntry rpfEntry, string fullPath)
        {
            // Check cache first
            if (_nestedRpfCache.ContainsKey(fullPath))
            {
                return _nestedRpfCache[fullPath];
            }

            try
            {
                // Extract the RPF data from the parent RPF
                var rpfData = rpfEntry.File.ExtractFile(rpfEntry);
                if (rpfData == null || rpfData.Length == 0)
                {
                    return null;
                }

                // Create a temporary file to load the nested RPF
                var tempFile = Path.GetTempFileName();
                File.WriteAllBytes(tempFile, rpfData);

                try
                {
                    // Load the nested RPF
                    var nestedRpf = new RpfFile(tempFile, fullPath);
                    nestedRpf.ScanStructure(null, null);

                    // Cache it
                    _nestedRpfCache[fullPath] = nestedRpf;

                    _log($"Loaded nested RPF: {fullPath} ({nestedRpf.AllEntries?.Count ?? 0} entries)");
                    return nestedRpf;
                }
                finally
                {
                    // Clean up temp file
                    try { File.Delete(tempFile); } catch { }
                }
            }
            catch (Exception ex)
            {
                _log($"Error loading nested RPF {fullPath}: {ex.Message}");
                return null;
            }
        }

        public void ClearCache()
        {
            _nestedRpfCache.Clear();
        }
    }

    public class NestedRpfEntry : RpfEntry
    {
        public RpfEntry OriginalEntry { get; }
        public string FullNestedPath { get; }
        public int NestingDepth { get; }

        public NestedRpfEntry(RpfEntry originalEntry, string fullPath, int depth)
        {
            OriginalEntry = originalEntry;
            FullNestedPath = fullPath;
            NestingDepth = depth;

            // Copy properties from original entry
            Name = originalEntry.Name;
            NameLower = originalEntry.NameLower;
            Path = fullPath; // Use the full nested path
            File = originalEntry.File;
            Parent = originalEntry.Parent;
            NameOffset = originalEntry.NameOffset;
        }

        public override void Read(DataReader reader)
        {
            // Delegate to the original entry
            OriginalEntry.Read(reader);
        }

        public override void Write(DataWriter writer)
        {
            // Delegate to the original entry
            OriginalEntry.Write(writer);
        }

        public override string ToString()
        {
            return $"{FullNestedPath} (depth: {NestingDepth})";
        }
    }
}