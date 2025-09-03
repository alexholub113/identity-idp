#!/usr/bin/env pwsh

# Script to start the Identity Provider API server
Write-Host "Starting Identity Provider API server..." -ForegroundColor Green

# Change to the API project directory
Set-Location "src/IdentityProvider.Api"

# Run the .NET API
dotnet run
