#!/bin/bash
# WarpTube Web Browser Launch Script
# Usage: ./run-web.sh [--open-browser]

# Clear the screen
clear

URL="http://localhost:5198"
OPEN_BROWSER=false

# Parse command line arguments
if [[ "$1" == "--open-browser" || "$1" == "-o" ]]; then
    OPEN_BROWSER=true
fi

echo "🌐 Starting WarpTube for Web Browser..."
echo "📍 Server will be available at: $URL"

if [ "$OPEN_BROWSER" = true ]; then
    echo "🚀 Will open browser automatically once server is ready..."
fi

echo ""
cd "$(dirname "$0")"

# Start the server in background if opening browser
if [ "$OPEN_BROWSER" = true ]; then
    echo "Starting server..."
    dotnet run --project WarpTube.Web/WarpTube.Web.csproj &
    SERVER_PID=$!
    
    # Wait for server to be ready
    echo "Waiting for server to start..."
    sleep 3
    
    # Check if server is responding
    until curl -s $URL > /dev/null 2>&1; do
        sleep 1
    done
    
    echo "Opening browser..."
    open $URL
    
    # Wait for the background process
    wait $SERVER_PID
else
    dotnet run --project WarpTube.Web/WarpTube.Web.csproj
fi
