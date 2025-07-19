using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using CodeWalker.GameFiles;

namespace CodeWalker.YftConverter
{
    public partial class YftConverterForm : Form
    {
        private volatile bool Running = false;

        public YftConverterForm()
        {
            InitializeComponent();
        }

        private void YftConverterForm_Load(object sender, EventArgs e)
        {
            // Set default selection to Gen9 YFT
            Gen9YftRadioButton.Checked = true;
        }

        private void InputFolderBrowseButton_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Select the input folder containing YFT files";
                if (!string.IsNullOrEmpty(InputFolderTextBox.Text) && Directory.Exists(InputFolderTextBox.Text))
                {
                    fbd.SelectedPath = InputFolderTextBox.Text;
                }

                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    InputFolderTextBox.Text = fbd.SelectedPath;
                }
            }
        }

        private void OutputFolderBrowseButton_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Select the output folder for converted files";
                if (!string.IsNullOrEmpty(OutputFolderTextBox.Text) && Directory.Exists(OutputFolderTextBox.Text))
                {
                    fbd.SelectedPath = OutputFolderTextBox.Text;
                }
                else if (!string.IsNullOrEmpty(InputFolderTextBox.Text) && Directory.Exists(InputFolderTextBox.Text))
                {
                    fbd.SelectedPath = InputFolderTextBox.Text;
                }

                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    OutputFolderTextBox.Text = fbd.SelectedPath;
                }
            }
        }

        private void ProcessButton_Click(object sender, EventArgs e)
        {
            if (Running) return;

            if (string.IsNullOrEmpty(InputFolderTextBox.Text) || !Directory.Exists(InputFolderTextBox.Text))
            {
                MessageBox.Show("Please select a valid input folder.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(OutputFolderTextBox.Text))
            {
                MessageBox.Show("Please select an output folder.", "Invalid Output", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!Directory.Exists(OutputFolderTextBox.Text))
            {
                try
                {
                    Directory.CreateDirectory(OutputFolderTextBox.Text);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to create output directory: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            ProcessButton.Enabled = false;
            Running = true;
            StatusProgressBar.Visible = true;

            Task.Run(() => ProcessFiles());
        }

        private void ProcessFiles()
        {
            try
            {
                var inputPath = InputFolderTextBox.Text;
                var outputPath = OutputFolderTextBox.Text;
                var searchOption = IncludeSubfoldersCheckBox.Checked ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                var overwrite = OverwriteFilesCheckBox.Checked;
                var outputFormat = Gen9YftRadioButton.Checked ? OutputFormat.Gen9Yft : OutputFormat.Gen8Xml;

                var files = Directory.GetFiles(inputPath, "*.yft", searchOption);
                
                UpdateLog($"Found {files.Length} YFT files to process.");
                UpdateLog($"Output format: {(outputFormat == OutputFormat.Gen9Yft ? "Gen9 YFT (Compressed)" : "Gen8 XML")}");
                UpdateLog("Starting conversion...\n");

                UpdateProgress(0, files.Length);

                for (int i = 0; i < files.Length; i++)
                {
                    var file = files[i];
                    var relativePath = GetRelativePath(inputPath, file);
                    var outputFilePath = Path.Combine(outputPath, relativePath);

                    // Change extension based on output format
                    if (outputFormat == OutputFormat.Gen8Xml)
                    {
                        outputFilePath = Path.ChangeExtension(outputFilePath, ".yft.xml");
                    }

                    var outputDir = Path.GetDirectoryName(outputFilePath);
                    if (!Directory.Exists(outputDir))
                    {
                        Directory.CreateDirectory(outputDir);
                    }

                    if (File.Exists(outputFilePath) && !overwrite)
                    {
                        UpdateLog($"Skipping: {relativePath} (already exists)");
                    }
                    else
                    {
                        try
                        {
                            ConvertYftFile(file, outputFilePath, outputFormat);
                            UpdateLog($"Converted: {relativePath}");
                        }
                        catch (Exception ex)
                        {
                            UpdateLog($"ERROR: Failed to convert {relativePath}: {ex.Message}");
                        }
                    }

                    UpdateProgress(i + 1, files.Length);
                }

                UpdateLog("\nConversion complete!");
            }
            catch (Exception ex)
            {
                UpdateLog($"\nERROR: {ex.Message}");
            }
            finally
            {
                BeginInvoke(new Action(() =>
                {
                    Running = false;
                    ProcessButton.Enabled = true;
                    StatusProgressBar.Visible = false;
                }));
            }
        }

        private void ConvertYftFile(string inputFile, string outputFile, OutputFormat format)
        {
            // Load the YFT file
            var yft = new YftFile();
            yft.Load(File.ReadAllBytes(inputFile));

            if (format == OutputFormat.Gen8Xml)
            {
                // Convert to XML format
                var xml = YftXml.GetXml(yft);
                var xdoc = new XmlDocument();
                xdoc.LoadXml(xml);
                
                // Save as formatted XML
                using (var writer = XmlWriter.Create(outputFile, new XmlWriterSettings
                {
                    Indent = true,
                    IndentChars = "  ",
                    NewLineChars = "\r\n",
                    NewLineHandling = NewLineHandling.Replace
                }))
                {
                    xdoc.Save(writer);
                }
            }
            else // OutputFormat.Gen9Yft
            {
                // Convert to Gen9 YFT compressed format
                // The YFT is already in the correct format after loading
                // If you need specific Gen9 conversion, add it here:
                // yft.ConvertToGen9Format(); // Implement this method in YftFile if needed
                
                // Save the YFT file
                var data = yft.Save();
                File.WriteAllBytes(outputFile, data);
            }
        }

        private void UpdateLog(string message)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => UpdateLog(message)));
                return;
            }

            LogTextBox.AppendText(message + Environment.NewLine);
            LogTextBox.SelectionStart = LogTextBox.Text.Length;
            LogTextBox.ScrollToCaret();
        }

        private void UpdateProgress(int current, int total)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => UpdateProgress(current, total)));
                return;
            }

            StatusProgressBar.Maximum = total;
            StatusProgressBar.Value = current;
        }

        private static string GetRelativePath(string fromPath, string toPath)
        {
            // This is a simple implementation of GetRelativePath for .NET Framework 4.8
            // which doesn't have Path.GetRelativePath
            
            if (string.IsNullOrEmpty(fromPath))
                throw new ArgumentNullException(nameof(fromPath));
            if (string.IsNullOrEmpty(toPath))
                throw new ArgumentNullException(nameof(toPath));

            Uri fromUri = new Uri(AppendDirectorySeparatorChar(fromPath));
            Uri toUri = new Uri(toPath);

            if (fromUri.Scheme != toUri.Scheme)
                return toPath; // Different schemes, cannot make relative

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            if (toUri.Scheme.Equals("file", StringComparison.OrdinalIgnoreCase))
            {
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }

            return relativePath;
        }

        private static string AppendDirectorySeparatorChar(string path)
        {
            // Ensure the path ends with a directory separator for proper URI handling
            if (!path.EndsWith(Path.DirectorySeparatorChar.ToString()) && 
                !path.EndsWith(Path.AltDirectorySeparatorChar.ToString()))
            {
                return path + Path.DirectorySeparatorChar;
            }
            return path;
        }

        private enum OutputFormat
        {
            Gen8Xml,
            Gen9Yft
        }
    }
}