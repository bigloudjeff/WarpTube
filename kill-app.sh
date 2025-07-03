#!/bin/bash
# WarpTube Kill Script
# Terminates all running instances of the WarpTube application

echo "🛑 Stopping all WarpTube instances..."

# Find and kill all WarpTube.Web processes
WARPTUBE_PIDS=$(ps aux | grep -E "(dotnet.*WarpTube|WarpTube\.Web)" | grep -v grep | awk '{print $2}')

if [ -z "$WARPTUBE_PIDS" ]; then
    echo "✅ No WarpTube instances are currently running."
else
    echo "Found WarpTube processes: $WARPTUBE_PIDS"
    
    # Kill each process
    for PID in $WARPTUBE_PIDS; do
        echo "Killing process $PID..."
        kill -9 $PID 2>/dev/null
        if [ $? -eq 0 ]; then
            echo "✅ Process $PID terminated"
        else
            echo "⚠️  Could not terminate process $PID (may have already exited)"
        fi
    done
    
    # Wait a moment for processes to fully terminate
    sleep 1
    
    # Verify all processes are killed
    REMAINING=$(ps aux | grep -E "(dotnet.*WarpTube|WarpTube\.Web)" | grep -v grep | wc -l)
    if [ $REMAINING -eq 0 ]; then
        echo "✅ All WarpTube instances have been terminated."
    else
        echo "⚠️  $REMAINING WarpTube process(es) may still be running."
        echo "You may need to run this script with sudo or check for zombie processes."
    fi
fi

# Optional: Also kill any dotnet processes on common WarpTube ports
echo ""
echo "Checking for processes on WarpTube ports..."

# Check common ports used by WarpTube (5198, 5067, 5099)
for PORT in 5198 5067 5099; do
    PORT_PID=$(lsof -ti :$PORT 2>/dev/null)
    if [ ! -z "$PORT_PID" ]; then
        echo "Found process $PORT_PID on port $PORT"
        kill -9 $PORT_PID 2>/dev/null
        if [ $? -eq 0 ]; then
            echo "✅ Process on port $PORT terminated"
        fi
    fi
done

echo ""
echo "🏁 Cleanup complete!"