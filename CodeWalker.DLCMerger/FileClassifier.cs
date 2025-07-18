using System;
using System.IO;
using System.Linq;

namespace CodeWalker.DLCMerger
{
    /// <summary>
    /// Classifies files to determine which RPF container they should be placed in
    /// </summary>
    public class FileClassifier
    {
        // File extensions for vehicle-related files
        private static readonly string[] VehicleExtensions = { ".yft", ".ycd" };
        
        // File extensions for weapon-related files
        private static readonly string[] WeaponExtensions = { ".ydr" };
        
        // File extensions for meta/data files
        private static readonly string[] MetaExtensions = { ".meta", ".xml", ".gxt2" };
        
        // Known weapon file prefixes
        private static readonly string[] WeaponPrefixes = { "w_", "weapon" };
        
        // Known vehicle file patterns
        private static readonly string[] VehiclePatterns = { "_hi", "va_" };

        public enum FileCategory
        {
            Vehicle,
            Weapon,
            Meta,
            Language,
            Other
        }

        public class ClassificationResult
        {
            public FileCategory Category { get; set; }
            public string? TargetContainer { get; set; }
            public string TargetPath { get; set; } = string.Empty;
        }

        /// <summary>
        /// Classifies a file based on its path and determines where it should be placed in the merged RPF
        /// </summary>
        public static ClassificationResult ClassifyFile(string filePath)
        {
            var fileName = Path.GetFileName(filePath).ToLowerInvariant();
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            var directory = Path.GetDirectoryName(filePath)?.ToLowerInvariant() ?? "";

            var result = new ClassificationResult();

            // Check for language files
            if (directory.Contains("lang") && extension == ".gxt2")
            {
                result.Category = FileCategory.Language;
                result.TargetContainer = null; // Language files stay in their original structure
                result.TargetPath = PreserveLanguageStructure(filePath);
                return result;
            }

            // Check for meta/xml files
            if (MetaExtensions.Contains(extension))
            {
                result.Category = FileCategory.Meta;
                result.TargetContainer = null; // Meta files go directly in data/
                result.TargetPath = GetMetaTargetPath(filePath);
                return result;
            }

            // Check for vehicle files
            if (IsVehicleFile(fileName, extension))
            {
                result.Category = FileCategory.Vehicle;
                result.TargetContainer = "vehicles.rpf";
                result.TargetPath = Path.GetFileName(filePath); // Just the filename in vehicles.rpf
                return result;
            }

            // Check for weapon files
            if (IsWeaponFile(fileName, extension))
            {
                result.Category = FileCategory.Weapon;
                result.TargetContainer = "weapons.rpf";
                result.TargetPath = Path.GetFileName(filePath); // Just the filename in weapons.rpf
                return result;
            }

            // Default case - preserve original structure
            result.Category = FileCategory.Other;
            result.TargetContainer = null;
            result.TargetPath = GetDefaultTargetPath(filePath);
            return result;
        }

        private static bool IsVehicleFile(string fileName, string extension)
        {
            // Check extension first
            if (VehicleExtensions.Contains(extension))
            {
                return true;
            }

            // Check if it's a YTD file with vehicle patterns
            if (extension == ".ytd")
            {
                // Check for vehicle patterns in the name
                if (VehiclePatterns.Any(pattern => fileName.Contains(pattern)))
                {
                    return true;
                }

                // Check if it's NOT a weapon YTD
                if (!WeaponPrefixes.Any(prefix => fileName.StartsWith(prefix)))
                {
                    // Most YTD files without weapon prefixes are vehicle textures
                    return true;
                }
            }

            return false;
        }

        private static bool IsWeaponFile(string fileName, string extension)
        {
            // Check extension first
            if (WeaponExtensions.Contains(extension))
            {
                return true;
            }

            // Check if it's a weapon YTD file
            if (extension == ".ytd" && WeaponPrefixes.Any(prefix => fileName.StartsWith(prefix)))
            {
                return true;
            }

            return false;
        }

        private static string GetMetaTargetPath(string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            
            // Special handling for files that should be in subdirectories
            if (filePath.ToLowerInvariant().Contains("levels\\gta5"))
            {
                return Path.Combine("data", "levels", "gta5", fileName);
            }
            
            if (filePath.ToLowerInvariant().Contains("\\ai\\"))
            {
                return Path.Combine("data", "ai", fileName);
            }

            // Default: put directly in data/
            return Path.Combine("data", fileName);
        }

        private static string PreserveLanguageStructure(string filePath)
        {
            // Find the "lang" directory and preserve structure from there
            var parts = filePath.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
            var langIndex = Array.FindIndex(parts, p => p.ToLowerInvariant() == "lang");
            
            if (langIndex >= 0)
            {
                // Preserve structure from x64/data/lang/...
                var preserved = parts.Skip(Math.Max(0, langIndex - 2)).ToArray();
                return Path.Combine(preserved);
            }

            return filePath;
        }

        private static string GetDefaultTargetPath(string filePath)
        {
            // For other files, try to preserve some of the original structure
            var parts = filePath.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
            
            // Find common structural elements to preserve
            var commonIndex = Array.FindIndex(parts, p => 
                p.ToLowerInvariant() == "common" || 
                p.ToLowerInvariant() == "x64" ||
                p.ToLowerInvariant() == "data");
            
            if (commonIndex >= 0)
            {
                return Path.Combine(parts.Skip(commonIndex).ToArray());
            }

            // Default: preserve full relative path
            return filePath;
        }
    }
}