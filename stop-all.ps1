#!/usr/bin/env pwsh

# Script to stop all Identity Provider services
Write-Host "Stopping Identity Provider services..." -ForegroundColor Red

# Stop any .NET processes running the Identity Provider API
$dotnetProcesses = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | Where-Object {
    $_.MainWindowTitle -like "*IdentityProvider*" -or 
    $_.ProcessName -eq "dotnet"
}

if ($dotnetProcesses) {
    Write-Host "Stopping .NET API processes..." -ForegroundColor Yellow
    $dotnetProcesses | Stop-Process -Force
    Write-Host "Stopped .NET API processes." -ForegroundColor Green
}

# Stop any Node.js processes (Vite dev server)
$nodeProcesses = Get-Process -Name "node" -ErrorAction SilentlyContinue

if ($nodeProcesses) {
    Write-Host "Stopping Node.js processes..." -ForegroundColor Yellow
    $nodeProcesses | Stop-Process -Force
    Write-Host "Stopped Node.js processes." -ForegroundColor Green
}

Write-Host "All Identity Provider services stopped." -ForegroundColor Green
