using CodeWalker.GameFiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CodeWalker.DLCMerger
{
    /// <summary>
    /// Manages the creation and organization of nested RPF containers within the merged RPF
    /// </summary>
    public class RpfContainerManager
    {
        private readonly Action<string> _log;
        private readonly RpfFile _outputRpf;
        private readonly Dictionary<string, RpfFile> _nestedRpfs = new();
        private readonly Dictionary<string, RpfDirectoryEntry> _containerRoots = new();

        public RpfContainerManager(RpfFile outputRpf, Action<string> log)
        {
            _outputRpf = outputRpf;
            _log = log;
        }

        /// <summary>
        /// Ensures a nested RPF container exists and returns its root directory
        /// </summary>
        public RpfDirectoryEntry GetOrCreateContainer(string containerName)
        {
            if (_containerRoots.ContainsKey(containerName))
            {
                return _containerRoots[containerName];
            }


            // Create a new nested RPF file
            var nestedRpf = RpfFile.CreateNew(_outputRpf.Path, containerName, RpfEncryption.OPEN);
            nestedRpf.Parent = _outputRpf;
            
            // Create the RPF file entry in the parent
            var rpfEntry = new RpfBinaryFileEntry
            {
                Name = containerName,
                NameLower = containerName.ToLowerInvariant(),
                Parent = _outputRpf.Root,
                File = _outputRpf,
                Path = _outputRpf.Root.Path + "\\" + containerName,
                FileOffset = 0, // Will be set when written
                FileSize = 0, // Will be set when written
                FileUncompressedSize = 0, // Will be set when written
                EncryptionType = 0,
                IsEncrypted = false
            };

            // Add to parent's file list
            _outputRpf.Root.Files.Add(rpfEntry);
            
            // Store references
            _nestedRpfs[containerName] = nestedRpf;
            _containerRoots[containerName] = nestedRpf.Root;

            return nestedRpf.Root;
        }

        /// <summary>
        /// Adds a file to the appropriate container based on classification
        /// </summary>
        public void AddFileToContainer(RpfFileEntry sourceEntry, FileClassifier.ClassificationResult classification)
        {
            if (classification.TargetContainer != null)
            {
                // File goes into a nested RPF container
                var containerRoot = GetOrCreateContainer(classification.TargetContainer);
                CopyFileToDirectory(sourceEntry, containerRoot, classification.TargetPath);
            }
            else
            {
                // File goes directly into the output RPF
                CopyFileToOutputRpf(sourceEntry, classification.TargetPath);
            }
        }

        /// <summary>
        /// Copies a file to a specific directory within the output RPF
        /// </summary>
        private void CopyFileToOutputRpf(RpfFileEntry sourceEntry, string targetPath)
        {
            // Extract file data
            var fileData = sourceEntry.File.ExtractFile(sourceEntry);
            if (fileData == null)
            {
                return;
            }

            // Parse the target path
            var pathParts = targetPath.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
            var fileName = pathParts.Last();
            var directories = pathParts.Take(pathParts.Length - 1).ToArray();

            // Find or create the parent directory
            var parentDir = _outputRpf.Root;
            foreach (var dirName in directories)
            {
                parentDir = FindOrCreateDirectory(parentDir, dirName);
            }

            // Create the file
            RpfFile.CreateFile(parentDir, fileName, fileData);
        }

        /// <summary>
        /// Copies a file to a directory within a nested container
        /// </summary>
        private void CopyFileToDirectory(RpfFileEntry sourceEntry, RpfDirectoryEntry containerRoot, string targetPath)
        {
            // Extract file data
            var fileData = sourceEntry.File.ExtractFile(sourceEntry);
            if (fileData == null)
            {
                return;
            }

            // Parse the target path (within the container)
            var pathParts = targetPath.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
            var fileName = pathParts.Last();
            var directories = pathParts.Take(pathParts.Length - 1).ToArray();

            // Find or create parent directories within the container
            var parentDir = containerRoot;
            foreach (var dirName in directories)
            {
                parentDir = FindOrCreateDirectory(parentDir, dirName);
            }

            // Create the file in the container
            RpfFile.CreateFile(parentDir, fileName, fileData);
        }

        /// <summary>
        /// Finds or creates a directory within a parent directory
        /// </summary>
        private RpfDirectoryEntry FindOrCreateDirectory(RpfDirectoryEntry parent, string dirName)
        {
            // Check if directory already exists
            var existing = parent.Directories.FirstOrDefault(d =>
                d.Name.Equals(dirName, StringComparison.OrdinalIgnoreCase));

            if (existing != null)
            {
                return existing;
            }

            // Create new directory
            var newDir = new RpfDirectoryEntry
            {
                Name = dirName,
                NameLower = dirName.ToLowerInvariant(),
                Parent = parent,
                File = parent.File,
                Path = parent.Path + "\\" + dirName
            };

            parent.Directories.Add(newDir);
            return newDir;
        }

        /// <summary>
        /// Finalizes all nested containers and ensures they are properly saved
        /// </summary>
        public void FinalizeContainers()
        {
            // The nested RPFs are automatically handled by the parent RPF's save process
        }

        /// <summary>
        /// Gets statistics about the containers
        /// </summary>
        public Dictionary<string, int> GetContainerStats()
        {
            var stats = new Dictionary<string, int>();
            
            foreach (var kvp in _containerRoots)
            {
                var containerName = kvp.Key;
                var root = kvp.Value;
                
                // Count files in the container
                var fileCount = CountFilesRecursive(root);
                stats[containerName] = fileCount;
            }
            
            return stats;
        }

        private int CountFilesRecursive(RpfDirectoryEntry dir)
        {
            int count = dir.Files?.Count ?? 0;
            
            if (dir.Directories != null)
            {
                foreach (var subDir in dir.Directories)
                {
                    count += CountFilesRecursive(subDir);
                }
            }
            
            return count;
        }
    }
}