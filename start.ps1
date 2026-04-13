# Start Couple Chat PWA Backend
# This PowerShell script starts the .NET Core 10 backend server

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   Starting Couple Chat PWA Backend" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Get the script location and navigate to Backend
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$backendPath = Join-Path $scriptPath "Backend"

Set-Location $backendPath
Write-Host "Backend directory: $(Get-Location)" -ForegroundColor Green
Write-Host ""

Write-Host "Starting dotnet run..." -ForegroundColor Yellow
Write-Host ""

dotnet run

Write-Host ""
Write-Host "Server stopped." -ForegroundColor Red
