#!/usr/bin/env pwsh

# Script to start the Identity Provider Web frontend
Write-Host "Starting Identity Provider Web frontend..." -ForegroundColor Green

# Change to the Web project directory
Set-Location "src/IdentityProvider.Web.LoginPage"

# Check if node_modules exists, if not install dependencies
if (-not (Test-Path "node_modules")) {
    Write-Host "Installing dependencies..." -ForegroundColor Yellow
    npm install
}

# Run the Vite development server
npm run dev
