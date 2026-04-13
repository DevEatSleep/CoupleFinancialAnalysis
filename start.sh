#!/bin/bash
# Start Couple Chat PWA Backend
# This script starts the .NET Core 10 backend server

echo ""
echo "========================================"
echo "   Starting Couple Chat PWA Backend"
echo "========================================"
echo ""

# Get the script location and navigate to Backend
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
cd "$SCRIPT_DIR/Backend"

echo "Backend directory: $(pwd)"
echo ""

echo "Starting dotnet run..."
echo ""

dotnet run

echo ""
echo "Server stopped."
