<#
.SYNOPSIS
Build script for EricksonLopez.SharedKernel

.DESCRIPTION
Restores, builds, tests, and verifies code formatting for the solution.
This script ensures local development matches the CI pipeline standards.
#>

$ErrorActionPreference = "Stop"
$Configuration = "Release"

Write-Host "🚀 Starting Build Process ($Configuration)" -ForegroundColor Cyan

Write-Host "`n📦 Restoring dependencies..." -ForegroundColor Yellow
dotnet restore
if ($LASTEXITCODE -ne 0) { throw "Restore failed" }

Write-Host "`n🧹 Verifying code format..." -ForegroundColor Yellow
dotnet format --verify-no-changes
if ($LASTEXITCODE -ne 0) { throw "Format check failed. Run 'dotnet format' to fix." }

Write-Host "`n🔨 Building solution..." -ForegroundColor Yellow
dotnet build --configuration $Configuration --no-restore
if ($LASTEXITCODE -ne 0) { throw "Build failed" }

Write-Host "`n🧪 Running unit tests..." -ForegroundColor Yellow
dotnet test --configuration $Configuration --no-build
if ($LASTEXITCODE -ne 0) { throw "Tests failed" }

Write-Host "`n✅ Build completed successfully!" -ForegroundColor Green
