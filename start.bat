@echo off
REM Start Couple Chat PWA - Backend Server
REM This script starts the .NET Core 10 backend server

echo.
echo ========================================
echo   Starting Couple Chat PWA Backend
echo ========================================
echo.

cd /d "%~dp0Backend"

echo Backend directory: %cd%
echo.

echo Starting dotnet run...
echo.

dotnet run

pause
