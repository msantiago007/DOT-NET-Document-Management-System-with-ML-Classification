# build-verify.ps1
$ErrorActionPreference = "Stop"

Write-Host "Building DocumentManagementML solution..." -ForegroundColor Cyan

# Find the solution file
$solutionFile = Get-ChildItem -Path . -Filter *.sln -Recurse | Select-Object -First 1

if ($null -eq $solutionFile) {
    Write-Host "Error: Solution file (.sln) not found!" -ForegroundColor Red
    exit 1
}

Write-Host "Found solution file: $($solutionFile.FullName)" -ForegroundColor Cyan

# Restore packages
Write-Host "Restoring NuGet packages..." -ForegroundColor Cyan
dotnet restore $solutionFile.FullName

if ($LASTEXITCODE -ne 0) {
    Write-Host "Package restoration failed!" -ForegroundColor Red
    exit 1
}

# Build the solution
Write-Host "Building solution..." -ForegroundColor Cyan
dotnet build $solutionFile.FullName --configuration Debug

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "Build completed successfully!" -ForegroundColor Green