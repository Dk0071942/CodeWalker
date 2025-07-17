using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeWalker.DLCMerger
{
    class Program
    {
        static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<Options>(args)
                .MapResult(
                    RunMerger,
                    HandleParseError);
        }

        static int RunMerger(Options options)
        {
            try
            {
                Console.WriteLine("CodeWalker DLC Merger Tool");
                Console.WriteLine("==========================");
                Console.WriteLine();

                // Display initial input info
                var inputPaths = options.InputFiles.ToList();
                if (inputPaths.Count == 0)
                {
                    Console.WriteLine("Error: No input files or directories specified. Use -i to specify input files or directories.");
                    Console.WriteLine("Example: DLCMerger -i dlc1.rpf -i dlc2.rpf -o merged.rpf");
                    Console.WriteLine("Example: DLCMerger -i /path/to/dlc/folder -o merged.rpf");
                    return 1;
                }
                
                Console.WriteLine($"Input paths: {inputPaths.Count}");
                foreach (var path in inputPaths)
                {
                    Console.WriteLine($"  - {path}");
                }
                Console.WriteLine($"Output file: {options.OutputFile}");
                Console.WriteLine($"Merge meta files: {(options.MergeMeta ? "Yes" : "No")}");
                Console.WriteLine($"Encryption: {options.Encryption}");
                Console.WriteLine();

                // Create merger instance
                var merger = new RpfMerger(options, Console.WriteLine);
                
                // Perform merge
                merger.Merge();

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nFatal error: {ex.Message}");
                if (options.Verbose)
                {
                    Console.WriteLine(ex.StackTrace);
                }
                return 1;
            }
        }

        static int HandleParseError(IEnumerable<Error> errors)
        {
            return 1;
        }
    }
}