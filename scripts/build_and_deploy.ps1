# Build and Deploy Script for Revit AI Architect
# Combines auto-build convenience with robust deployment logic

param(
    [string]$RevitVersion = "2026",
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug"
)

$ErrorActionPreference = "Stop"

# === 1. Setup Paths ===
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$SolutionDir = Resolve-Path "$ScriptDir\.."
$ProjectDir = "$SolutionDir\RevitAIArchitect"
$BuildOutput = "$ProjectDir\bin\$Configuration\net8.0-windows"

# Revit Addins Folder
$AddinFolder = "$env:APPDATA\Autodesk\Revit\Addins\$RevitVersion"

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Revit AI Architect - Build & Deploy   " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Configuration: $Configuration" -ForegroundColor Yellow
Write-Host "Revit Version: $RevitVersion" -ForegroundColor Yellow
Write-Host ""

# === 2. Build Solution ===
Write-Host "[1/4] Building Solution..." -ForegroundColor White
dotnet build "$SolutionDir\RevitAIArchitect.sln" --configuration $Configuration

if ($LASTEXITCODE -ne 0) {
    Write-Error "Build Failed. Aborting deployment."
}

# === 3. Run Tests ===
Write-Host "[2/4] Running Tests..." -ForegroundColor White
dotnet test "$SolutionDir\RevitAIArchitect.sln" --configuration $Configuration

if ($LASTEXITCODE -ne 0) {
    Write-Error "Tests Failed. Aborting deployment."
}

# === 4. Create Target Directory ===
if (-not (Test-Path $AddinFolder)) {
    Write-Host "Creating Addins folder: $AddinFolder" -ForegroundColor Yellow
    New-Item -ItemType Directory -Force -Path $AddinFolder | Out-Null
}

# === 5. Copy Files ===
Write-Host "[3/4] Deploying Files..." -ForegroundColor White

# Copy DLL
$DllPath = "$BuildOutput\RevitAIArchitect.dll"
if (Test-Path $DllPath) {
    Copy-Item $DllPath $AddinFolder -Force
    Write-Host "      -> RevitAIArchitect.dll" -ForegroundColor Green
}
else {
    Write-Error "Build output not found at $DllPath"
}

# Copy Dependencies (if any specific ones are needed, add here)
# For now, let's copy everything from output to be safe for a simple plugin
Get-ChildItem $BuildOutput -Filter "*.dll" | ForEach-Object {
    if ($_.Name -ne "RevitAIArchitect.dll" -and $_.Name -notlike "RevitAPI*") {
        Copy-Item $_.FullName $AddinFolder -Force
        Write-Host "      -> $($_.Name)" -ForegroundColor Gray
    }
}

# === 6. Generate Manifest (.addin) ===
Write-Host "[4/4] Generating Manifest..." -ForegroundColor White

# We point to the deployed DLL in the Addins folder
$AssemblyPath = "$AddinFolder\RevitAIArchitect.dll"

$AddinContent = @"
<?xml version="1.0" encoding="utf-8"?>
<RevitAddIns>
  <AddIn Type="Command">
    <Name>Revit AI Architect</Name>
    <Assembly>$AssemblyPath</Assembly>
    <AddInId>A1B2C3D4-E5F6-4789-0123-456789ABCDEF</AddInId>
    <FullClassName>RevitAIArchitect.Command</FullClassName>
    <Text>Ask AI</Text>
    <Description>Open AI Chat Assistant</Description>
    <VendorId>ADSK</VendorId>
    <VendorDescription>Autodesk, www.autodesk.com</VendorDescription>
  </AddIn>
</RevitAddIns>
"@

$AddinFilePath = "$AddinFolder\RevitAIArchitect.addin"
$AddinContent | Out-File -FilePath $AddinFilePath -Encoding UTF8
Write-Host "      -> RevitAIArchitect.addin" -ForegroundColor Green

# === 6. Summary ===
Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  Success! Deployed to Revit $RevitVersion  " -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
