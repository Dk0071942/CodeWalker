# DLC Merger Build Script

## Overview
Replaced the original `build_dlcmerger.bat` and `test_dlcmerger.bat` files with a single comprehensive `dlcmerger.bat` script that provides enhanced functionality, better error handling, and improved user experience.

## Single Script Solution: dlcmerger.bat

### Features:
- **Combined Functionality**: Build and test in one script
- **Multiple Commands**: 
  - `build` - Build only
  - `test` - Test only (requires existing build)
  - `all` - Build and test (default)
  - `clean` - Clean build artifacts
  - `help` - Show help information

- **Build Configurations**:
  - `debug` - Debug build (default)
  - `release` - Release build

- **Test Options**:
  - `-v` - Verbose output
  - `-s` - Simplified merger mode
  - `-f` - Force overwrite outputs
  - `-x` - Extract mode
  - `-q` - Quick test (minimal output)

- **Advanced Features**:
  - Comprehensive error handling
  - Build verification
  - Argument parsing
  - Help system
  - Progress indication
  - Prerequisites validation

### Usage Examples:
```batch
dlcmerger.bat                    # Build and test (debug)
dlcmerger.bat build release      # Build release version only
dlcmerger.bat test -v            # Run verbose tests
dlcmerger.bat all release -s -v  # Build release and run simplified verbose tests
dlcmerger.bat clean              # Clean build artifacts
dlcmerger.bat help               # Show help
```

## Key Improvements

### 🔧 **Technical Enhancements**
- **Error Handling**: Comprehensive error checking with proper exit codes
- **Validation**: Prerequisites, build output, and test data validation
- **Flexibility**: Multiple execution modes and configuration options
- **Robustness**: Better handling of edge cases and failure scenarios

### 🎯 **User Experience**
- **Clear Output**: Professional formatting with progress indicators
- **Help System**: Built-in help and usage information
- **Options**: Customizable build and test parameters
- **Feedback**: Detailed success/failure messages

### 🧹 **Simplified Structure**
- **Single Script**: One file handles all build and test operations
- **No Duplication**: Eliminated redundant functionality
- **Clean Interface**: Consistent command structure and options

## Usage Guide

### Quick Start:
```batch
dlcmerger.bat                    # Build and test (most common usage)
```

### Development Workflow:
```batch
dlcmerger.bat build debug        # Build for development
dlcmerger.bat test -v            # Run verbose tests
dlcmerger.bat clean              # Clean when needed
```

### Release Workflow:
```batch
dlcmerger.bat build release      # Build for release
dlcmerger.bat test -q            # Quick test validation
```

### Help and Options:
```batch
dlcmerger.bat help               # Show all available options
```

## Testing Results

✅ **Script properly structured** with:
- Correct batch file syntax
- Error handling implementation
- User-friendly output
- Proper exit codes

✅ **Enhanced functionality** including:
- Multiple execution modes
- Configuration options
- Comprehensive help system
- Build verification
- Test customization

## Recommendation

Use `dlcmerger.bat` for all build and test operations. The single script approach provides a cleaner, more maintainable solution with enhanced functionality.