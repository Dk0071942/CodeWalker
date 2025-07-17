using CommandLine;
using System.Collections.Generic;

namespace CodeWalker.DLCMerger
{
    public class Options
    {
        [Option('i', "input", Required = true, HelpText = "Input RPF files or directories to merge. Specify multiple files/directories.")]
        public IEnumerable<string> InputFiles { get; set; } = new List<string>();

        [Option('o', "output", Required = true, HelpText = "Output RPF file path.")]
        public string OutputFile { get; set; } = string.Empty;

        [Option('s', "show-structure", Default = false, HelpText = "Show detailed structure tree of input RPF files.")]
        public bool ShowStructure { get; set; }

        [Option('n', "expand-nested", Default = true, HelpText = "Expand nested RPF files to show their contents.")]
        public bool ExpandNested { get; set; }

        [Option('m', "merge-meta", Default = false, HelpText = "Enable merging of meta/xml files (experimental).")]
        public bool MergeMeta { get; set; }

        [Option('v', "verbose", Default = false, HelpText = "Enable verbose output.")]
        public bool Verbose { get; set; }

        [Option('e', "encryption", Default = "OPEN", HelpText = "Encryption type for output RPF (NONE, OPEN, AES, NG).")]
        public string Encryption { get; set; } = "OPEN";

        [Option('f', "force", Default = false, HelpText = "Force overwrite output file if it exists.")]
        public bool Force { get; set; }

        [Option('d', "dry-run", Default = false, HelpText = "Perform a dry run without creating output file.")]
        public bool DryRun { get; set; }
    }
}