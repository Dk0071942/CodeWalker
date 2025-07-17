using CodeWalker.GameFiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CodeWalker.DLCMerger
{
    public class RpfStructureAnalyzer
    {
        private readonly Action<string> _log;
        private readonly NestedRpfReader _nestedReader;

        public RpfStructureAnalyzer(Action<string> log)
        {
            _log = log;
            _nestedReader = new NestedRpfReader(log);
        }

        public void PrintRpfStructure(RpfFile rpf, string title, bool expandNested = true)
        {
            _log($"\n=== {title} Structure{(expandNested ? " (with nested RPF expansion)" : "")} ===");
            _log($"File: {rpf.Path}");
            _log($"Encryption: {rpf.Encryption}");
            _log("");

            if (expandNested)
            {
                // Get all entries including nested RPF contents
                var allEntries = _nestedReader.GetAllEntriesRecursive(rpf);
                _log($"Total entries (including nested): {allEntries.Count}");
                _log($"Original entries: {rpf.AllEntries?.Count ?? 0}");
                _log("");

                if (allEntries.Count == 0)
                {
                    _log("No entries found!");
                    return;
                }

                // Build tree structure from all entries
                var treeNodes = BuildTreeStructureFromEntries(allEntries, rpf);
                PrintTreeNode(treeNodes, "", true);
            }
            else
            {
                // Use original structure without nested expansion
                _log($"Total entries: {rpf.AllEntries?.Count ?? 0}");
                _log("");

                if (rpf.AllEntries == null || rpf.AllEntries.Count == 0)
                {
                    _log("No entries found!");
                    return;
                }

                // Build tree structure
                var treeNodes = BuildTreeStructure(rpf);
                PrintTreeNode(treeNodes, "", true);
            }
        }

        public void PrintAllRpfStructures(List<RpfFile> rpfs, bool expandNested = true)
        {
            for (int i = 0; i < rpfs.Count; i++)
            {
                PrintRpfStructure(rpfs[i], $"INPUT RPF {i + 1}", expandNested);
            }
        }

        public void PrintMergedStructureAnalysis(Dictionary<string, RpfEntry> mergedEntries, 
            Dictionary<string, List<RpfEntry>> conflicts)
        {
            _log("\n=== MERGED STRUCTURE ANALYSIS ===");
            _log($"Total unique entries: {mergedEntries.Count}");
            _log($"Conflicting entries: {conflicts.Count}");
            _log("");

            // Group by type
            var files = mergedEntries.Where(e => e.Value is RpfFileEntry).ToList();
            var directories = mergedEntries.Where(e => e.Value is RpfDirectoryEntry).ToList();

            _log($"Files: {files.Count}");
            _log($"Directories: {directories.Count}");
            _log("");

            // Print file type analysis
            PrintFileTypeAnalysis(files);

            // Print conflict analysis
            PrintConflictAnalysis(conflicts);
        }

        private void PrintFileTypeAnalysis(List<KeyValuePair<string, RpfEntry>> files)
        {
            _log("=== FILE TYPE ANALYSIS ===");
            
            var fileTypes = new Dictionary<string, int>();
            foreach (var file in files)
            {
                var path = file.Key;
                var extension = Path.GetExtension(path).ToLowerInvariant();
                if (string.IsNullOrEmpty(extension))
                    extension = "[no extension]";
                
                fileTypes[extension] = fileTypes.GetValueOrDefault(extension, 0) + 1;
            }

            foreach (var ft in fileTypes.OrderByDescending(x => x.Value))
            {
                _log($"  {ft.Key}: {ft.Value} files");
            }
            _log("");
        }

        private void PrintConflictAnalysis(Dictionary<string, List<RpfEntry>> conflicts)
        {
            _log("=== CONFLICT ANALYSIS ===");

            var metaConflicts = conflicts.Where(c => 
                c.Key.EndsWith(".meta", StringComparison.OrdinalIgnoreCase) || 
                c.Key.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var dirConflicts = conflicts.Where(c => 
                c.Value.FirstOrDefault() is RpfDirectoryEntry)
                .ToList();

            var fileConflicts = conflicts.Where(c => 
                c.Value.FirstOrDefault() is RpfFileEntry)
                .ToList();

            _log($"Directory conflicts: {dirConflicts.Count}");
            _log($"File conflicts: {fileConflicts.Count}");
            _log($"Meta/XML conflicts: {metaConflicts.Count}");
            _log("");

            if (metaConflicts.Any())
            {
                _log("Critical meta/XML conflicts (should be merged):");
                foreach (var conflict in metaConflicts)
                {
                    _log($"  {conflict.Key}");
                    foreach (var entry in conflict.Value)
                    {
                        if (entry is RpfFileEntry fileEntry)
                        {
                            _log($"    - {entry.File?.Path}: {fileEntry.GetFileSize()} bytes");
                        }
                    }
                }
                _log("");
            }

            if (dirConflicts.Any())
            {
                _log("Directory conflicts (should be merged, not treated as conflicts):");
                foreach (var conflict in dirConflicts.Take(5))
                {
                    _log($"  {conflict.Key}");
                }
                if (dirConflicts.Count > 5)
                {
                    _log($"  ... and {dirConflicts.Count - 5} more");
                }
                _log("");
            }
        }

        private List<TreeNode> BuildTreeStructure(RpfFile rpf)
        {
            var rootNodes = new List<TreeNode>();
            var nodeMap = new Dictionary<string, TreeNode>();

            // Create root node
            var rootNode = new TreeNode
            {
                Name = Path.GetFileName(rpf.Path),
                Path = "",
                IsDirectory = true,
                Entry = rpf.Root
            };
            rootNodes.Add(rootNode);
            nodeMap[""] = rootNode;

            // Process all entries
            foreach (var entry in rpf.AllEntries.Skip(1)) // Skip root
            {
                var relativePath = GetRelativePath(entry);
                var pathParts = relativePath.Split('\\').Where(p => !string.IsNullOrEmpty(p)).ToArray();
                
                if (pathParts.Length == 0) continue;

                var currentPath = "";
                TreeNode currentNode = rootNode;

                // Build path hierarchy
                for (int i = 0; i < pathParts.Length; i++)
                {
                    var part = pathParts[i];
                    var newPath = string.IsNullOrEmpty(currentPath) ? part : currentPath + "\\" + part;
                    
                    if (!nodeMap.ContainsKey(newPath))
                    {
                        var node = new TreeNode
                        {
                            Name = part,
                            Path = newPath,
                            IsDirectory = i < pathParts.Length - 1 || entry is RpfDirectoryEntry,
                            Entry = (i == pathParts.Length - 1) ? entry : null
                        };
                        
                        currentNode.Children.Add(node);
                        nodeMap[newPath] = node;
                    }
                    
                    currentNode = nodeMap[newPath];
                    currentPath = newPath;
                }
            }

            return rootNodes;
        }

        private List<TreeNode> BuildTreeStructureFromEntries(List<RpfEntry> allEntries, RpfFile rootRpf)
        {
            var rootNodes = new List<TreeNode>();
            var nodeMap = new Dictionary<string, TreeNode>();

            // Create root node
            var rootNode = new TreeNode
            {
                Name = Path.GetFileName(rootRpf.Path),
                Path = "",
                IsDirectory = true,
                Entry = rootRpf.Root
            };
            rootNodes.Add(rootNode);
            nodeMap[""] = rootNode;

            // Process all entries (including nested)
            foreach (var entry in allEntries)
            {
                var relativePath = GetRelativePath(entry);
                var pathParts = relativePath.Split('\\').Where(p => !string.IsNullOrEmpty(p)).ToArray();
                
                if (pathParts.Length == 0) continue;

                var currentPath = "";
                TreeNode currentNode = rootNode;

                // Build path hierarchy
                for (int i = 0; i < pathParts.Length; i++)
                {
                    var part = pathParts[i];
                    var newPath = string.IsNullOrEmpty(currentPath) ? part : currentPath + "\\" + part;
                    
                    if (!nodeMap.ContainsKey(newPath))
                    {
                        var node = new TreeNode
                        {
                            Name = part,
                            Path = newPath,
                            IsDirectory = i < pathParts.Length - 1 || entry is RpfDirectoryEntry,
                            Entry = (i == pathParts.Length - 1) ? entry : null
                        };
                        
                        // Add nesting depth info for nested entries
                        if (entry is NestedRpfEntry nestedEntry)
                        {
                            node.Name += $" [depth: {nestedEntry.NestingDepth}]";
                        }
                        
                        currentNode.Children.Add(node);
                        nodeMap[newPath] = node;
                    }
                    
                    currentNode = nodeMap[newPath];
                    currentPath = newPath;
                }
            }

            return rootNodes;
        }

        private void PrintTreeNode(List<TreeNode> nodes, string prefix, bool isLast)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                var isLastNode = i == nodes.Count - 1;
                var connector = isLastNode ? "└── " : "├── ";
                
                var info = "";
                if (node.Entry is RpfFileEntry fileEntry)
                {
                    info = $" ({fileEntry.GetFileSize()} bytes)";
                }
                else if (node.Entry is RpfDirectoryEntry dirEntry)
                {
                    var childCount = dirEntry.Directories.Count + dirEntry.Files.Count;
                    info = childCount > 0 ? $" ({childCount} items)" : "";
                }

                _log($"{prefix}{connector}{node.Name}{info}");

                if (node.Children.Count > 0)
                {
                    var childPrefix = prefix + (isLastNode ? "    " : "│   ");
                    PrintTreeNode(node.Children, childPrefix, isLastNode);
                }
            }
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

        private class TreeNode
        {
            public string Name { get; set; } = "";
            public string Path { get; set; } = "";
            public bool IsDirectory { get; set; }
            public RpfEntry? Entry { get; set; }
            public List<TreeNode> Children { get; set; } = new();
        }
    }
}