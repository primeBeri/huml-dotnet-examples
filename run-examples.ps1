#!/usr/bin/env pwsh
# Runs every example console project and aggregates the results. Each example asserts via the
# shared Check helper and exits non-zero on failure, so this doubles as the end-to-end test
# suite. Used by CI (examples.yml) and runnable locally:  ./run-examples.ps1
#
# Pass -HumlNetVersion to test a specific package version:
#   ./run-examples.ps1 -HumlNetVersion 0.2.0-beta.1

param([string]$HumlNetVersion = "")

$ErrorActionPreference = "Stop"
$repo = $PSScriptRoot
$examples = Get-ChildItem (Join-Path $repo "src/examples") -Directory | Sort-Object Name
$verArg = if ($HumlNetVersion) { "-p:HumlNetVersion=$HumlNetVersion" } else { "" }

$failed = @()
foreach ($ex in $examples) {
    $proj = Get-ChildItem $ex.FullName -Filter *.csproj | Select-Object -First 1
    if (-not $proj) { continue }
    Write-Host "── Running $($ex.Name) ──" -ForegroundColor Cyan
    if ($verArg) { dotnet run --project $proj.FullName -c Release $verArg }
    else { dotnet run --project $proj.FullName -c Release }
    if ($LASTEXITCODE -ne 0) { $failed += $ex.Name }
}

Write-Host ""
if ($failed.Count -eq 0) {
    Write-Host "All $($examples.Count) examples passed." -ForegroundColor Green
    exit 0
}
Write-Host "FAILED: $($failed -join ', ')" -ForegroundColor Red
exit 1
