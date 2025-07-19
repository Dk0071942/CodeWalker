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
                Console.WriteLine("Uncompressed YFT Converter");
                Console.WriteLine("==========================");

                var converter = new YftConverter(options.Verbose);
                
                // Parse output format
                switch (options.Format.ToLowerInvariant())
                {
                    case "yft":
                        // Parse generation option for YFT output
                        bool useGen9;
                        switch (options.Generation.ToLowerInvariant())
                        {
                            case "gen8":
                                useGen9 = false;
                                break;
                            case "gen9":
                                useGen9 = true;
                                break;
                            default:
                                throw new ArgumentException($"Invalid generation '{options.Generation}'. Use 'gen8' or 'gen9'.");
                        }
                        
                        Console.WriteLine($"Converting {Path.GetFileName(options.Input)} to compressed YFT using {options.Generation.ToUpperInvariant()}");
                        converter.ConvertUncompressedToCompressed(options.Input, options.Output, useGen9);
                        break;
                        
                    case "xml":
                        Console.WriteLine($"Converting {Path.GetFileName(options.Input)} to XML format");
                        if (!string.IsNullOrEmpty(options.ResourceFolder))
                        {
                            Console.WriteLine($"Resource folder: {options.ResourceFolder}");
                        }
                        converter.ConvertUncompressedToXml(options.Input, options.Output, options.ResourceFolder);
                        break;
                        
                    default:
                        throw new ArgumentException($"Invalid format '{options.Format}'. Use 'yft' or 'xml'.");
                }
                
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

        [Value(1, MetaName = "output", Required = true, HelpText = "Output file (compressed YFT or XML)")]  
        public string Output { get; set; }

        [Option('v', "verbose", Default = false, HelpText = "Enable verbose output")]
        public bool Verbose { get; set; }

        [Option('g', "generation", Default = "gen8", HelpText = "Compression generation (gen8 or gen9). Default: gen8")]
        public string Generation { get; set; }

        [Option('f', "format", Default = "yft", HelpText = "Output format (yft or xml). Default: yft")]
        public string Format { get; set; }

        [Option('r', "resource-folder", Default = "", HelpText = "Folder for extracting resources/textures (XML format only)")]
        public string ResourceFolder { get; set; }
    }
}