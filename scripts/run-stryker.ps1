# Copyright © Erickson Lopez. MIT License.
param(
    [string]$Config = "stryker-config.json",
    [string]$MutationLevel = "Standard"
)

$ErrorActionPreference = "Stop"

Write-Host "Running Stryker.NET with configuration: $Config" -ForegroundColor Cyan

dotnet tool restore 2>$null
if ($LASTEXITCODE -ne 0) {
    dotnet tool install --global dotnet-stryker
}

dotnet stryker `
    --project src/EricksonLopez.SharedKernel/EricksonLopez.SharedKernel.csproj `
    --test-project tests/EricksonLopez.SharedKernel.UnitTests/EricksonLopez.SharedKernel.UnitTests.csproj `
    --config-file $Config `
    --mutation-level $MutationLevel `
    --reporter html `
    --reporter progress `
    --output StrykerOutput/local
