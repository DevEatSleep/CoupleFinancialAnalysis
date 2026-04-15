@echo off
REM Start Couple Chat - Backend and Blazor Frontend
REM This script starts both the .NET Core 10 backend server and Blazor frontend

echo.
echo ========================================
echo   Starting Couple Chat PWA
echo   Backend + Blazor Frontend
echo ========================================
echo.

REM Store the base directory
set BASEDIR=%cd%

echo Starting Backend on http://localhost:5000...
echo.
start "Couple Chat - Backend" cmd /k "cd /d %BASEDIR%\Backend && dotnet run"

timeout /t 3 /nobreak

echo.
echo Starting Blazor Frontend on http://localhost:3000...
echo.
start "Couple Chat - Blazor Frontend" cmd /k "cd /d %BASEDIR%\Frontend && dotnet run --urls http://localhost:3000"

echo.
echo ========================================
echo Both servers are starting...
echo Backend:  http://localhost:5000
echo Frontend: http://localhost:3000
echo Swagger:  http://localhost:5000/swagger
echo ========================================
echo.
echo To stop the servers, close the command windows or press Ctrl+C in each.
echo.
pause
