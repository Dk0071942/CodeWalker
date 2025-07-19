using System;
using System.IO;
using System.Windows.Forms;

namespace CodeWalker.YftConverter
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                // Console mode
                RunConsoleMode(args);
            }
            else
            {
                // GUI mode
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new YftConverterForm());
            }
        }

        static void RunConsoleMode(string[] args)
        {
            Console.WriteLine("CodeWalker YFT Converter - Console Mode");
            Console.WriteLine("=======================================\n");

            if (args.Length < 2 || args[0] == "--help" || args[0] == "-h")
            {
                ShowHelp();
                return;
            }

            string inputPath = args[0];
            string outputPath = args[1];
            OutputFormat format = OutputFormat.Gen9YFT; // Default
            bool verbose = false;

            // Parse optional arguments
            for (int i = 2; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "--gen8":
                    case "-g8":
                        format = OutputFormat.Gen8YFT;
                        break;
                    case "--gen9":
                    case "-g9":
                        format = OutputFormat.Gen9YFT;
                        break;
                    case "--xml":
                    case "-x":
                        format = OutputFormat.XML;
                        break;
                    case "--verbose":
                    case "-v":
                        verbose = true;
                        break;
                }
            }

            try
            {
                var converter = new YftConverter(verbose);
                converter.ConvertUncompressedYFT(inputPath, outputPath, format);
                Console.WriteLine($"\nConversion successful!");
                Console.WriteLine($"Output: {outputPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nERROR: {ex.Message}");
                if (verbose && ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                Environment.Exit(1);
            }
        }

        static void ShowHelp()
        {
            Console.WriteLine("Usage: YftConverter.exe <input_file> <output_file> [options]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  --gen8, -g8     Output as Gen8 compressed YFT (version 162)");
            Console.WriteLine("  --gen9, -g9     Output as Gen9 compressed YFT (version 171) [default]");
            Console.WriteLine("  --xml, -x       Output as XML format");
            Console.WriteLine("  --verbose, -v   Show detailed conversion information");
            Console.WriteLine("  --help, -h      Show this help message");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  YftConverter.exe input.yft output.yft --gen9");
            Console.WriteLine("  YftConverter.exe dump.yft vehicle.xml --xml --verbose");
            Console.WriteLine();
            Console.WriteLine("Note: This converter is designed for uncompressed YFT files (memory dumps).");
            Console.WriteLine("      Files should start with 'FRAG' header.");
        }
    }
}