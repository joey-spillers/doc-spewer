#Requires -RunAsAdministrator
<#
.SYNOPSIS
    Builds and installs the OneView Virtual Scanner TWAIN Data Source.
    Must be run as Administrator (TWAIN folders require elevated access).
#>

$ErrorActionPreference = "Stop"

$root     = Split-Path $PSScriptRoot -Parent
$srcProj  = Join-Path $root "src\OneView.VirtualScanner.Source\OneView.VirtualScanner.Source.csproj"
$pubDir64 = Join-Path $root "src\OneView.VirtualScanner.Source\bin\publish\win-x64"
$twain64  = "C:\Windows\twain_64\OneView"

Write-Host "=== OneView Virtual Scanner - Source Installer ===" -ForegroundColor Cyan

# Build x64 NativeAOT
Write-Host "`nPublishing x64 NativeAOT source..." -ForegroundColor Yellow
dotnet publish $srcProj -c Release -r win-x64 --self-contained true `
    PublishSingleFile=true
    PublishTrimmed=true
    -o $pubDir64

# Copy .dll → .ds in TWAIN folder
$dll = Join-Path $pubDir64 "OneView.VirtualScanner.Source.dll"
if (-not (Test-Path $dll)) {
    Write-Host "ERROR: Published DLL not found at $dll" -ForegroundColor Red
    exit 1
}

New-Item -ItemType Directory -Force -Path $twain64 | Out-Null
$dest = Join-Path $twain64 "OneViewVS.ds"
Copy-Item $dll $dest -Force
Write-Host "Installed x64 source => $dest" -ForegroundColor Green

# Ensure config directories exist
$configRoot = Join-Path $env:ProgramData "OneView\VirtualScanner"
@("", "profiles", "cache", "logs") | ForEach-Object {
    $d = Join-Path $configRoot $_
    if (-not (Test-Path $d)) { New-Item -ItemType Directory -Force -Path $d | Out-Null }
}
Write-Host "Config directories ready at $configRoot" -ForegroundColor Green

# Set permissions so non-admin users can read/write config
icacls $configRoot /grant "Users:(OI)(CI)F" /T | Out-Null

Write-Host "`n=== Installation complete ===" -ForegroundColor Cyan
Write-Host "Open TWACKER or your scan app and select OneView Virtual Scanner"
