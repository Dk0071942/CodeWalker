#!/bin/bash

# Build script for CodeWalker DLC Merger

echo "Building CodeWalker DLC Merger..."
echo "================================"
echo ""
echo "Prerequisites:"
echo "- .NET 8.0 SDK"
echo "- Visual Studio 2022 or MSBuild"
echo ""
echo "To build from command line:"
echo "  dotnet build CodeWalker.DLCMerger.csproj"
echo ""
echo "To build entire solution:"
echo "  dotnet build ../CodeWalker.sln"
echo ""
echo "To run the tool:"
echo "  dotnet run --project CodeWalker.DLCMerger.csproj -- [arguments]"
echo ""
echo "Example usage after build:"
echo "  ./bin/Debug/net8.0/DLCMerger -i dlc1.rpf -i dlc2.rpf -o merged.rpf"