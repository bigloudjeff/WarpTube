#!/bin/bash
# WarpTube Default Launch Script (Web Browser)

echo "🚀 Starting WarpTube (Web Browser)..."
echo "💡 Use ./run-mac.sh for Mac Catalyst or ./run-web.sh to be explicit"
echo ""

cd "$(dirname "$0")"
dotnet run --project WarpTube.Web/WarpTube.Web.csproj
