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
                var inputPaths = options.InputFiles.ToList();
                if (inputPaths.Count == 0)
                {
                    Console.WriteLine("Error: No input files specified. Use -i to specify input files.");
                    Console.WriteLine("Usage: DLCMerger -i dlc1.rpf -i dlc2.rpf -o merged.rpf");
                    return 1;
                }
                
                Console.WriteLine($"DLCMerger: {inputPaths.Count} inputs -> {options.OutputFile}");

                // Create merger instance based on extract mode
                if (options.ExtractMode)
                {
                    var merger = new SimplifiedRpfMerger(options, Console.WriteLine);
                    merger.Merge();
                }
                else
                {
                    var merger = new RpfMerger(options, Console.WriteLine);
                    merger.Merge();
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
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