# YFT File Format Documentation

This folder contains comprehensive documentation about YFT (Fragment) file formats used in GTA V.

## Documents

### FRAG_Format_Documentation.md
Complete guide to understanding and converting between FRAG and RSC7 formats:
- Format comparison (RSC7 vs FRAG)
- Why FRAG format exists
- Conversion tools and usage
- Technical details
- Common issues and solutions

### yft_binary_encoding_analysis.md
In-depth analysis of YFT binary structure:
- RSC7 header format
- FragType binary layout
- Safe modification areas for custom data
- Protection and encryption strategies
- Memory layout details

### yft_xml_analysis.md
Comprehensive XML structure documentation:
- Complete YFT XML schema
- Drawable and texture handling
- Critical fields to preserve
- Common issues and validation
- Code references for XML processing

## Related Resources

- **Tools/FragConversion/** - Conversion tools for FRAG to RSC7
- **Examples/YFT/** - Example YFT files and usage code
- **CodeWalker.Core/Utils/YftFragConverter.cs** - Utility class for format conversion

## Quick Reference

### Check YFT Format
```csharp
bool isFrag = YftFragConverter.IsFragFormat(data);
```

### Convert FRAG to RSC7
```csharp
YftFragConverter.ConvertFile("input.yft", "output.yft");
```

### Load YFT (auto-converts if needed)
```csharp
YftFile yft = YftFragConverter.LoadYft("vehicle.yft");
```