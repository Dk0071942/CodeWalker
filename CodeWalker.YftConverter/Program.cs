using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CodeWalker.YftConverter
{
    class Program
    {
        static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<ConvertOptions>(args)
                .MapResult(
                    (ConvertOptions opts) => RunConvert(opts),
                    HandleParseError);
        }

        static int RunConvert(ConvertOptions options)
        {
            try
            {
                Console.WriteLine("Uncompressed YFT to Compressed YFT Converter");
                Console.WriteLine("=============================================");

                var converter = new YftConverter(options.Verbose);
                
                Console.WriteLine($"Converting {Path.GetFileName(options.Input)} to {Path.GetFileName(options.Output)}");
                converter.ConvertUncompressedToCompressed(options.Input, options.Output);
                Console.WriteLine("Conversion complete");
                
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
            var isVersion = errors.Any(e => e is VersionRequestedError);
            var isHelp = errors.Any(e => e is HelpRequestedError || e is HelpVerbRequestedError);
            
            if (!isVersion && !isHelp)
            {
                Console.WriteLine("Error parsing command line arguments. Use --help for usage information.");
            }
            
            return isHelp || isVersion ? 0 : 1;
        }
    }

    class ConvertOptions
    {
        [Value(0, MetaName = "input", Required = true, HelpText = "Input uncompressed YFT file")]
        public string Input { get; set; }

        [Value(1, MetaName = "output", Required = true, HelpText = "Output compressed YFT file")]  
        public string Output { get; set; }

        [Option('v', "verbose", Default = false, HelpText = "Enable verbose output")]
        public bool Verbose { get; set; }
    }
}