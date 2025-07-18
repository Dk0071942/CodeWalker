using CommandLine;
using System.Collections.Generic;

namespace CodeWalker.DLCMerger
{
    public class Options
    {
        [Option('i', "input", Required = true, HelpText = "Input RPF files or directories to merge. Specify multiple files/directories.")]
        public IEnumerable<string> InputFiles { get; set; } = new List<string>();

        [Option('o', "output", Required = true, HelpText = "Output directory path for merged DLC files.")]
        public string OutputFile { get; set; } = string.Empty;

        [Option('s', "show-structure", Default = false, HelpText = "Show detailed structure tree of input RPF files.")]
        public bool ShowStructure { get; set; }

        [Option('n', "expand-nested", Default = true, HelpText = "Expand nested RPF files to show their contents.")]
        public bool ExpandNested { get; set; }

        [Option('m', "merge-meta", Default = false, HelpText = "Enable merging of meta/xml files (experimental).")]
        public bool MergeMeta { get; set; }

        [Option('v', "verbose", Default = false, HelpText = "Enable verbose output.")]
        public bool Verbose { get; set; }


        [Option('f', "force", Default = false, HelpText = "Force overwrite output file if it exists.")]
        public bool Force { get; set; }

        [Option('d', "dry-run", Default = false, HelpText = "Perform a dry run without creating output file.")]
        public bool DryRun { get; set; }

        [Option('a', "merge-all", Default = false, HelpText = "Merge all files without selective filtering (default is to only merge vehicle-related files).")]
        public bool MergeAll { get; set; }

    }
}