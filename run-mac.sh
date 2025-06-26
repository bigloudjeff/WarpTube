#!/bin/bash
# WarpTube Mac Catalyst Launch Script

echo "🚀 Starting WarpTube for Mac Catalyst..."
cd "$(dirname "$0")"
dotnet run --project WarpTube/WarpTube.csproj --framework net9.0-maccatalyst
