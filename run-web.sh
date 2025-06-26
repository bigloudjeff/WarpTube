#!/bin/bash
# WarpTube Web Browser Launch Script

echo "🌐 Starting WarpTube for Web Browser..."
cd "$(dirname "$0")"
dotnet run --project WarpTube.Web/WarpTube.Web.csproj
