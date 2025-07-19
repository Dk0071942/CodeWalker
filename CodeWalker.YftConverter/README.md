# YFT Converter

A simple tool to convert uncompressed YFT memory dumps to compressed YFT format.

## Usage

```bash
YftConverter.exe input.yft output.yft
```

## Options

- `-v, --verbose` - Enable verbose output

## Example

```bash
YftConverter.exe locked.yft locked_compressed.yft -v
```

## Notes

- Input file must be an uncompressed YFT memory dump (created using CodeWalker's "Extract Uncompressed" function)
- The tool will compress the data and add the proper RSC7 header
- Due to memory dump limitations, some data may be lost in conversion