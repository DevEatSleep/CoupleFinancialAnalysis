@echo off
REM Start Couple Chat - Backend + Blazor Frontend
REM Backend on http://localhost:5000 (API + old frontend)
REM Blazor Frontend on http://localhost:5173

echo.
echo ========================================
echo   Starting Couple Chat Application
echo ========================================
echo.

echo Starting Backend on port 5000...
start "Backend" cmd /c "cd /d %~dp0Backend && dotnet run"

echo Starting Blazor Frontend on port 3000...
timeout /t 3 /nobreak >nul
start "Frontend" cmd /c "cd /d %~dp0Frontend && dotnet run --urls http://localhost:3000"

echo.
echo Both servers are starting...
echo   Backend:  http://localhost:5000
echo   Frontend: http://localhost:3000
echo.
echo Press any key to stop...
pause >nul
taskkill /FI "WINDOWTITLE eq Backend" >nul 2>&1
taskkill /FI "WINDOWTITLE eq Frontend" >nul 2>&1
