#requires -Version 7.0
<#
.SYNOPSIS
    Verifies scenario-local AppHost selection without starting services or reading secrets.
.EXAMPLE
    pwsh .\scripts\Tests\Test-ScenarioAppHosts.ps1
#>
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$scenarios = Join-Path $PSScriptRoot '..\..\scenarios'
if (-not (Get-Command aspire -ErrorAction SilentlyContinue)) {
    throw 'The Aspire CLI is required to verify effective AppHost selection.'
}

$count = 0
foreach ($scenario in Get-ChildItem $scenarios -Directory) {
    $projects = @(Get-ChildItem $scenario.FullName -Recurse -Filter eShopAppHost.csproj)
    if ($projects.Count -eq 0) { continue }
    if ($projects.Count -ne 1) { throw "Expected one AppHost in '$($scenario.Name)'." }
    $configPath = Join-Path $scenario.FullName 'aspire.config.json'
    $config = Get-Content $configPath -Raw | ConvertFrom-Json
    $relativePath = $config.appHost.path
    if ([string]::IsNullOrWhiteSpace($relativePath) -or [IO.Path]::IsPathRooted($relativePath)) {
        throw "Expected a portable relative AppHost path in '$configPath'."
    }
    $expected = $projects[0].FullName
    $resolved = [IO.Path]::GetFullPath((Join-Path $scenario.FullName $relativePath))
    if ($resolved -ne $expected) {
        throw "Configuration for '$($scenario.Name)' selects a different AppHost."
    }
    Push-Location $scenario.FullName
    try {
        $effective = & aspire config get appHost.path --non-interactive
        if ($LASTEXITCODE -ne 0 -or ($effective -join "`n").Trim() -ne $relativePath) {
            throw "Aspire did not select the local AppHost for '$($scenario.Name)'."
        }
    }
    finally { Pop-Location }
    $count++
    Write-Host "PASS: $($scenario.Name)"
}
if ($count -eq 0) { throw 'No scenario AppHosts were checked.' }
Write-Host "All $count scenario AppHost selections passed."
