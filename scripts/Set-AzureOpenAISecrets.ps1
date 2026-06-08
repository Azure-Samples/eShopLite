<#
.SYNOPSIS
    Sets Azure OpenAI configuration as Aspire AppHost parameter secrets across all eShopLite scenarios.

.DESCRIPTION
    Interactively collects four Azure OpenAI configuration values and writes them to each scenario's
    AppHost project using the Aspire CLI "aspire secret set" command.  The secrets are stored in the
    AppHost's user-secrets store (local dev only) and are consumed by services at runtime via the
    Aspire Parameters binding mechanism.

    Parameters set (names are exact and case-sensitive):
        Parameters:AzureOpenAIEndpoint
        Parameters:AzureOpenAIApiKey                  (stored masked; never echoed)
        Parameters:AzureOpenAIDeploymentName
        Parameters:AzureOpenAIEmbeddingsDeploymentName

    All eShopAppHost.csproj files are discovered automatically under $ScenariosRoot, so the script
    handles both the standard layout (scenarios\NN-Name\src\eShopAppHost) and any scenario without
    a src sub-folder (e.g. 18-MAFDevUI).

    Requires the .NET Aspire CLI.  Install with:
        dotnet tool install -g aspire.cli
    See: https://aspire.dev/reference/cli/commands/aspire-secret/

.PARAMETER DryRun
    Print the "aspire secret set" commands that would be executed, but do not run them.
    The API key value is replaced with *** in dry-run output.

.PARAMETER ScenariosRoot
    Path to the root "scenarios" folder.  Defaults to the "scenarios" directory one level above the
    directory containing this script (i.e. $PSScriptRoot\..\scenarios), which resolves to the repo
    root scenarios folder when the script lives at scripts\Set-AzureOpenAISecrets.ps1.

.EXAMPLE
    .\Set-AzureOpenAISecrets.ps1
    Interactive run: prompts for all values and sets secrets in every AppHost.

.EXAMPLE
    .\Set-AzureOpenAISecrets.ps1 -DryRun
    Shows all commands that would be executed without actually running them (API key masked as ***).

.EXAMPLE
    .\Set-AzureOpenAISecrets.ps1 -ScenariosRoot "C:\repos\eShopLite\scenarios"
    Uses an explicit path to the scenarios root instead of auto-detecting from script location.

.NOTES
    Author    : Apone (DevOps, eShopLite)
    Requestor : Bruno Capuano
    Created   : 2026-06-06

    IMPORTANT: This script targets LOCAL DEVELOPMENT only.
    In Azure deployments (azd up) the endpoint and deployment names are injected automatically via
    Managed Identity and Aspire resource bindings.  The API key is not used in publish mode because
    bicep provisions Azure OpenAI with disableLocalAuth: true.
#>

[CmdletBinding(SupportsShouldProcess)]
param(
    [switch]$DryRun,

    [string]$ScenariosRoot = (Join-Path $PSScriptRoot '..\scenarios')
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

#-------------------------------------------------------------------------------
# Helpers
#-------------------------------------------------------------------------------

function Write-Header {
    param([string]$Text)
    Write-Host ''
    Write-Host "==> $Text" -ForegroundColor Cyan
}

function Write-Ok   { param([string]$T) Write-Host "  [OK]     $T" -ForegroundColor Green }
function Write-Fail { param([string]$T) Write-Host "  [FAILED] $T" -ForegroundColor Red }
function Write-Info { param([string]$T) Write-Host "  $T" -ForegroundColor Gray }

# Converts a SecureString to a plaintext string using Marshal to avoid .NET managed heap exposure.
# The caller is responsible for zeroing the returned string as soon as it is no longer needed.
function ConvertFrom-SecureStringToPlainText {
    param([System.Security.SecureString]$Secure)
    $bstr = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($Secure)
    try {
        return [System.Runtime.InteropServices.Marshal]::PtrToStringBSTR($bstr)
    }
    finally {
        [System.Runtime.InteropServices.Marshal]::ZeroFreeBSTR($bstr)
    }
}

# Derives a friendly scenario name from a csproj full path, e.g. "01-SemanticSearch".
function Get-ScenarioName {
    param([string]$CsprojPath)
    $scenariosAbs = (Resolve-Path $ScenariosRoot).Path.TrimEnd('\')
    $dir = [System.IO.Path]::GetDirectoryName($CsprojPath)
    while ($dir -and ($dir -ne [System.IO.Path]::GetPathRoot($dir))) {
        $parent = [System.IO.Path]::GetDirectoryName($dir)
        if ($parent -and ($parent.TrimEnd('\') -ieq $scenariosAbs)) {
            return [System.IO.Path]::GetFileName($dir)
        }
        $dir = $parent
    }
    # Fallback: return immediate parent folder name of the csproj
    return [System.IO.Path]::GetFileName([System.IO.Path]::GetDirectoryName($CsprojPath))
}

#-------------------------------------------------------------------------------
# Prerequisite check
#-------------------------------------------------------------------------------

Write-Header 'Checking prerequisites'

if (-not (Get-Command aspire -ErrorAction SilentlyContinue)) {
    Write-Error @'
The "aspire" CLI was not found on PATH.

To install it as a global .NET tool, run:
    dotnet tool install -g aspire.cli

Then restart your terminal so the tool is on PATH, and re-run this script.
See: https://aspire.dev/reference/cli/commands/aspire-secret/
'@
    exit 1
}

Write-Ok 'aspire CLI found'

#-------------------------------------------------------------------------------
# Resolve scenarios root
#-------------------------------------------------------------------------------

$ScenariosRoot = (Resolve-Path $ScenariosRoot -ErrorAction Stop).Path
Write-Info "Scenarios root : $ScenariosRoot"

#-------------------------------------------------------------------------------
# Discover AppHost projects
#-------------------------------------------------------------------------------

Write-Header 'Discovering AppHost projects'

$appHostProjects = Get-ChildItem -Path $ScenariosRoot -Recurse -Filter 'eShopAppHost.csproj' |
    Sort-Object FullName

if ($appHostProjects.Count -eq 0) {
    Write-Error "No eShopAppHost.csproj files found under '$ScenariosRoot'. Check the -ScenariosRoot parameter."
    exit 1
}

foreach ($p in $appHostProjects) {
    Write-Info "Found: $($p.FullName)"
}
Write-Info "$($appHostProjects.Count) AppHost project(s) discovered."

#-------------------------------------------------------------------------------
# Collect configuration interactively
#-------------------------------------------------------------------------------

Write-Header 'Azure OpenAI configuration'
Write-Host '  Please enter your Azure OpenAI details.  Press Enter to accept a suggested default.' -ForegroundColor Yellow

# 1. Endpoint
$endpoint = ''
while ($true) {
    $endpoint = Read-Host '  AzureOpenAIEndpoint (e.g. https://<name>.openai.azure.com/)'
    if ($endpoint -match '^https?://') { break }
    if ($endpoint -eq '') {
        Write-Host '  [WARN] Endpoint cannot be empty.' -ForegroundColor Yellow
    }
    else {
        Write-Host '  [WARN] Value does not look like a URL (must start with http). Please re-enter.' -ForegroundColor Yellow
    }
}

# 2. API Key (masked)
$apiKeySecure = $null
while ($true) {
    $apiKeySecure = Read-Host '  AzureOpenAIApiKey (input is masked)' -AsSecureString
    # Measure length without retaining plaintext
    $tmpCheck = ConvertFrom-SecureStringToPlainText $apiKeySecure
    $keyLen = $tmpCheck.Length
    Remove-Variable tmpCheck
    if ($keyLen -gt 0) { break }
    Write-Host '  [WARN] API key cannot be empty.' -ForegroundColor Yellow
}

# 3. Chat deployment name
$chatDeploymentDefault = 'gpt-4.1-mini'
$chatDeploymentRaw = Read-Host "  AzureOpenAIDeploymentName [default: $chatDeploymentDefault]"
if ($chatDeploymentRaw.Trim() -eq '') {
    $chatDeployment = $chatDeploymentDefault
}
else {
    $chatDeployment = $chatDeploymentRaw.Trim()
}

# 4. Embeddings deployment name
$embeddingsDeploymentDefault = 'text-embedding-3-small'
$embeddingsDeploymentRaw = Read-Host "  AzureOpenAIEmbeddingsDeploymentName [default: $embeddingsDeploymentDefault]"
if ($embeddingsDeploymentRaw.Trim() -eq '') {
    $embeddingsDeployment = $embeddingsDeploymentDefault
}
else {
    $embeddingsDeployment = $embeddingsDeploymentRaw.Trim()
}

#-------------------------------------------------------------------------------
# Summary of values (API key is never echoed)
#-------------------------------------------------------------------------------

Write-Header 'Values to be applied'
Write-Info "AzureOpenAIEndpoint                    : $endpoint"
Write-Info "AzureOpenAIApiKey                      : [MASKED]"
Write-Info "AzureOpenAIDeploymentName              : $chatDeployment"
Write-Info "AzureOpenAIEmbeddingsDeploymentName    : $embeddingsDeployment"

if ($DryRun) {
    Write-Host ''
    Write-Host '  DRY-RUN mode -- commands will be printed but NOT executed.' -ForegroundColor Magenta
}

#-------------------------------------------------------------------------------
# Parameter definitions
# IsSensitive = $true defers plaintext extraction to use-time and prints *** in DryRun
#-------------------------------------------------------------------------------

$parameters = @(
    [pscustomobject]@{ Name = 'Parameters:AzureOpenAIEndpoint';                 Value = $endpoint;             IsSensitive = $false }
    [pscustomobject]@{ Name = 'Parameters:AzureOpenAIApiKey';                   Value = $null;                 IsSensitive = $true  }
    [pscustomobject]@{ Name = 'Parameters:AzureOpenAIDeploymentName';           Value = $chatDeployment;       IsSensitive = $false }
    [pscustomobject]@{ Name = 'Parameters:AzureOpenAIEmbeddingsDeploymentName'; Value = $embeddingsDeployment; IsSensitive = $false }
)

$results   = [System.Collections.Generic.List[pscustomobject]]::new()
$failCount = 0

#-------------------------------------------------------------------------------
# Apply secrets
#-------------------------------------------------------------------------------

foreach ($appHost in $appHostProjects) {
    $scenarioName = Get-ScenarioName $appHost.FullName
    $appHostPath  = $appHost.FullName
    $scenarioOk   = $true

    Write-Header "Scenario: $scenarioName"

    foreach ($param in $parameters) {

        # Resolve plaintext value (sensitive params extracted fresh each time)
        if ($param.IsSensitive) {
            $plainValue = ConvertFrom-SecureStringToPlainText $apiKeySecure
        }
        else {
            $plainValue = $param.Value
        }

        $displayValue = if ($param.IsSensitive) { '***' } else { $plainValue }

        if ($DryRun) {
            Write-Info "[DRY-RUN] aspire secret set `"$($param.Name)`" `"$displayValue`" --apphost `"$appHostPath`" --non-interactive"
        }
        else {
            Write-Info "Setting $($param.Name) = $displayValue"
            & aspire secret set "$($param.Name)" "$plainValue" --apphost "$appHostPath" --non-interactive
            $exitCode = $LASTEXITCODE
            if ($exitCode -ne 0) {
                Write-Fail "$($param.Name) (exit $exitCode)"
                $scenarioOk = $false
            }
            else {
                Write-Ok "$($param.Name)"
            }
        }

        # Zero out the sensitive plaintext immediately after use
        if ($param.IsSensitive) {
            $plainValue = $null
            [System.GC]::Collect()
        }
    }

    if ($DryRun) {
        $status = 'DRY-RUN'
    }
    elseif ($scenarioOk) {
        $status = 'OK'
    }
    else {
        $status = 'FAILED'
        $failCount++
    }

    $results.Add([pscustomobject]@{
        Scenario = $scenarioName
        Status   = $status
    })
}

#-------------------------------------------------------------------------------
# Summary table
#-------------------------------------------------------------------------------

Write-Header 'Summary'
$results | Format-Table -AutoSize | Out-String | Write-Host

$total = $results.Count

if ($DryRun) {
    Write-Host "  Dry-run complete. $total scenario(s) would be processed (0 actually executed)." -ForegroundColor Magenta
    exit 0
}

$okCount = ($results | Where-Object { $_.Status -eq 'OK' }).Count

if ($failCount -gt 0) {
    Write-Host "  $okCount / $total scenario(s) succeeded. $failCount FAILED." -ForegroundColor Red
    exit 1
}

Write-Host "  All $total scenario(s) updated successfully." -ForegroundColor Green
exit 0
