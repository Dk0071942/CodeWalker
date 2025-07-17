using CodeWalker.GameFiles;
using System;
using System.Xml.Linq;

namespace CodeWalker.DLCMerger
{
    public class ContentXmlAnalyzer
    {
        private readonly Action<string> _log;

        public ContentXmlAnalyzer(Action<string> log)
        {
            _log = log;
        }

        public DlcInfo AnalyzeContentXml(RpfFile rpf)
        {
            var dlcInfo = new DlcInfo { RpfPath = rpf.Path };

            try
            {
                // Find content.xml in the RPF
                var contentXmlEntry = FindContentXml(rpf);
                if (contentXmlEntry != null)
                {
                    var xmlData = rpf.ExtractFile(contentXmlEntry);
                    if (xmlData != null)
                    {
                        var xmlText = System.Text.Encoding.UTF8.GetString(xmlData);
                        ParseContentXml(xmlText, dlcInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                _log($"Error analyzing content.xml for {rpf.Path}: {ex.Message}");
            }

            return dlcInfo;
        }

        private RpfFileEntry? FindContentXml(RpfFile rpf)
        {
            if (rpf.AllEntries == null) return null;

            foreach (var entry in rpf.AllEntries)
            {
                if (entry is RpfFileEntry fileEntry && 
                    string.Equals(entry.Name, "content.xml", StringComparison.OrdinalIgnoreCase))
                {
                    return fileEntry;
                }
            }
            return null;
        }

        private void ParseContentXml(string xmlText, DlcInfo dlcInfo)
        {
            try
            {
                var doc = XDocument.Parse(xmlText);
                var root = doc.Root;

                if (root?.Name.LocalName == "CDataFileMgr__ContentsOfDataFileXml")
                {
                    // Parse DLC package name
                    var packageName = root.Element("subPackages")?.Element("Item")?.Element("packageName")?.Value;
                    if (!string.IsNullOrEmpty(packageName))
                    {
                        dlcInfo.PackageName = packageName;
                    }

                    // Parse path
                    var path = root.Element("subPackages")?.Element("Item")?.Element("path")?.Value;
                    if (!string.IsNullOrEmpty(path))
                    {
                        dlcInfo.Path = path;
                    }

                    // Parse data files
                    var dataFiles = root.Element("subPackages")?.Element("Item")?.Element("filesToInvalidate");
                    if (dataFiles != null)
                    {
                        foreach (var item in dataFiles.Elements("Item"))
                        {
                            var filename = item.Element("filename")?.Value;
                            if (!string.IsNullOrEmpty(filename))
                            {
                                dlcInfo.DataFiles.Add(filename);
                            }
                        }
                    }

                    // Parse DLC type/category
                    var type = root.Element("subPackages")?.Element("Item")?.Element("type")?.Value;
                    if (!string.IsNullOrEmpty(type))
                    {
                        dlcInfo.Type = type;
                    }

                    _log($"DLC Info: {dlcInfo.PackageName} ({dlcInfo.Type}) - {dlcInfo.DataFiles.Count} data files");
                }
            }
            catch (Exception ex)
            {
                _log($"Error parsing content.xml: {ex.Message}");
            }
        }

        public void PrintDlcInfo(DlcInfo dlcInfo)
        {
            _log($"\n=== DLC Information ===");
            _log($"RPF: {dlcInfo.RpfPath}");
            _log($"Package Name: {dlcInfo.PackageName ?? "Unknown"}");
            _log($"Path: {dlcInfo.Path ?? "Unknown"}");
            _log($"Type: {dlcInfo.Type ?? "Unknown"}");
            _log($"Data Files: {dlcInfo.DataFiles.Count}");

            if (dlcInfo.DataFiles.Count > 0)
            {
                _log("Data Files:");
                foreach (var file in dlcInfo.DataFiles)
                {
                    _log($"  - {file}");
                }
            }
        }
    }

    public class DlcInfo
    {
        public string RpfPath { get; set; } = "";
        public string? PackageName { get; set; }
        public string? Path { get; set; }
        public string? Type { get; set; }
        public List<string> DataFiles { get; set; } = new();
    }
}