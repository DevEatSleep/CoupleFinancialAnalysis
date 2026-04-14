#!/bin/bash
# Start Couple Chat PWA - Backend and Blazor Frontend
# This script starts both the .NET Core 10 backend server and Blazor frontend

echo ""
echo "========================================"
echo "   Starting Couple Chat PWA"
echo "   Backend + Blazor Frontend"
echo "========================================"
echo ""

# Get the script location
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

echo "Starting Backend on http://localhost:5000..."
echo ""
cd "$SCRIPT_DIR/Backend"
dotnet run &
BACKEND_PID=$!

echo "Waiting for backend to initialize..."
sleep 3

echo ""
echo "Starting Blazor Frontend on http://localhost:3000..."
echo ""
cd "$SCRIPT_DIR/BlazorFrontend"
dotnet run --urls http://localhost:3000 &
BLAZOR_PID=$!

echo ""
echo "========================================"
echo "Both servers are starting..."
echo "Backend:  http://localhost:5000"
echo "Frontend: http://localhost:3000"
echo "Swagger:  http://localhost:5000/swagger"
echo "========================================"
echo ""
echo "Press Ctrl+C to stop all servers"
echo ""

# Wait for both processes
wait $BACKEND_PID $BLAZOR_PID
