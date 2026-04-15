# Start Couple Chat PWA - Backend and Blazor Frontend
# This PowerShell script starts both the .NET Core 10 backend server and Blazor frontend

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   Starting Couple Chat PWA" -ForegroundColor Cyan
Write-Host "   Backend + Blazor Frontend" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Get the script location
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$backendPath = Join-Path $scriptPath "Backend"
$blazorPath = Join-Path $scriptPath "Frontend"

# Start Backend in a background job
Write-Host "Starting Backend on http://localhost:5000..." -ForegroundColor Green
$backendJob = Start-Job -Name "Backend" -ScriptBlock {
    param($path)
    Set-Location $path
    & dotnet run
} -ArgumentList $backendPath

# Wait a few seconds for the backend to start
Write-Host "Waiting for backend to initialize..." -ForegroundColor Yellow
Start-Sleep -Seconds 3

# Start Blazor Frontend in a background job
Write-Host "Starting Blazor Frontend on http://localhost:3000..." -ForegroundColor Green
$blazorJob = Start-Job -Name "Frontend" -ScriptBlock {
    param($path)
    Set-Location $path
    & dotnet run --urls http://localhost:3000
} -ArgumentList $blazorPath

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Both servers are starting..." -ForegroundColor Green
Write-Host "Backend:  http://localhost:5000" -ForegroundColor Yellow
Write-Host "Frontend: http://localhost:3000" -ForegroundColor Yellow
Write-Host "Swagger:  http://localhost:5000/swagger" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Press Ctrl+C to stop all servers" -ForegroundColor Yellow
Write-Host ""

# Keep the script running and monitor the jobs
while ($true) {
    if ((Get-Job -Name "Backend").State -eq "Failed" -or (Get-Job -Name "Backend").State -eq "Completed") {
        Write-Host "Backend job ended. Output:" -ForegroundColor Red
        Get-Job -Name "Backend" | Receive-Job
    }
    if ((Get-Job -Name "Frontend").State -eq "Failed" -or (Get-Job -Name "Frontend").State -eq "Completed") {
        Write-Host "Frontend job ended. Output:" -ForegroundColor Red
        Get-Job -Name "Frontend" | Receive-Job
    }
    Start-Sleep -Seconds 1
}
