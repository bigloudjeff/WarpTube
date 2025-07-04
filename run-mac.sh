#!/bin/bash
# WarpTube Mac Catalyst Launch Script

# Clear the terminal screen
clear

echo "🚀 Starting WarpTube for Mac Catalyst..."
cd "$(dirname "$0")"
dotnet run --project WarpTube/WarpTube.csproj --framework net9.0-maccatalyst
