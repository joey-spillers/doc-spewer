<#
.SYNOPSIS
    Builds the entire OneView Virtual Scanner solution.
#>

$ErrorActionPreference = "Stop"
$root = Split-Path $PSScriptRoot -Parent

Write-Host "=== Building OneView Virtual Scanner ===" -ForegroundColor Cyan

Write-Host "`n[1/3] Building Core..." -ForegroundColor Yellow
dotnet build "$root\src\OneView.VirtualScanner.Core" -c Release

Write-Host "`n[2/3] Building Manager..." -ForegroundColor Yellow
dotnet build "$root\src\OneView.VirtualScanner.Manager" -c Release

Write-Host "`n[3/3] Building Source (regular build, not AOT)..." -ForegroundColor Yellow
dotnet build "$root\src\OneView.VirtualScanner.Source" -c Release

Write-Host "`n=== All projects built ===" -ForegroundColor Green
Write-Host "To install the TWAIN source, run scripts\install-source.ps1 as Administrator."
Write-Host "To run the Manager: dotnet run --project src\OneView.VirtualScanner.Manager"
