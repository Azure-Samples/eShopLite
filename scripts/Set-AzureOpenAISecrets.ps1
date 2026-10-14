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
    a src sub-folder (e.g. 14-MAFDevUI).

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

.PARAMETER Endpoint
    Existing Azure OpenAI inference endpoint. Prompts when omitted.

.PARAMETER ApiKey
    API key as a SecureString. Prompts when omitted, except in preview mode.

.PARAMETER ChatDeployment
    Chat deployment name. Defaults to gpt-6-luna.

.PARAMETER EmbeddingsDeployment
    Embeddings deployment name. Defaults to text-embedding-3-small.

.PARAMETER NonInteractive
    Fail instead of prompting when required configuration is missing.

.PARAMETER DotNetRoot
    Optional SDK installation directory. Used only for child commands; does not change
    the terminal or machine environment. The SDK must satisfy each AppHost global.json.

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

    [string]$ScenariosRoot = (Join-Path $PSScriptRoot '..\scenarios'),

    [string]$Endpoint,

    [System.Security.SecureString]$ApiKey,

    [ValidateNotNullOrEmpty()]
    [string]$ChatDeployment = 'gpt-6-luna',

    [ValidateNotNullOrEmpty()]
    [string]$EmbeddingsDeployment = 'text-embedding-3-small',

    [switch]$NonInteractive,

    [string]$DotNetRoot
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

function Invoke-WithDotNet {
    param([scriptblock]$Command)
    $previousRoot = $env:DOTNET_ROOT
    $previousPath = $env:PATH
    try {
        if ($DotNetRoot) {
            $env:DOTNET_ROOT = $DotNetRoot
            $env:PATH = "$DotNetRoot$([IO.Path]::PathSeparator)$previousPath"
        }
        & $Command
    }
    finally {
        $env:DOTNET_ROOT = $previousRoot
        $env:PATH = $previousPath
    }
}

# The CLI requires a managed plaintext string; release the reference immediately after use.
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
    $relative = [System.IO.Path]::GetRelativePath($scenariosAbs, $CsprojPath)
    if ($relative -in @('src\eShopAppHost\eShopAppHost.csproj', 'eShopAppHost\eShopAppHost.csproj')) {
        return [System.IO.Path]::GetFileName($scenariosAbs)
    }
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

if (-not ($DryRun -or $WhatIfPreference) -and -not (Get-Command aspire -ErrorAction SilentlyContinue)) {
    Write-Error @'
The "aspire" CLI was not found on PATH.

To install it as a global .NET tool, run:
    dotnet tool install -g aspire.cli

Then restart your terminal so the tool is on PATH, and re-run this script.
See: https://aspire.dev/reference/cli/commands/aspire-secret/
'@
    exit 1
}

if (-not ($DryRun -or $WhatIfPreference)) { Write-Ok 'aspire CLI found' }

#-------------------------------------------------------------------------------
# Resolve scenarios root
#-------------------------------------------------------------------------------

$ScenariosRoot = (Resolve-Path $ScenariosRoot -ErrorAction Stop).Path
Write-Info "Scenarios root : $ScenariosRoot"

#-------------------------------------------------------------------------------
# Discover AppHost projects
#-------------------------------------------------------------------------------

Write-Header 'Discovering AppHost projects'

$appHostProjects = @(Get-ChildItem -Path $ScenariosRoot -Recurse -Filter 'eShopAppHost.csproj' |
    Sort-Object FullName)

if ($appHostProjects.Count -eq 0) {
    Write-Error "No eShopAppHost.csproj files found under '$ScenariosRoot'. Check the -ScenariosRoot parameter."
    exit 1
}

foreach ($p in $appHostProjects) {
    Write-Info "Found: $($p.FullName)"
}
Write-Info "$($appHostProjects.Count) AppHost project(s) discovered."
foreach ($scenario in Get-ChildItem -Path $ScenariosRoot -Directory) {
    $prefix = $scenario.FullName + [System.IO.Path]::DirectorySeparatorChar
    if (-not @($appHostProjects | Where-Object { $_.FullName.StartsWith($prefix, [StringComparison]::OrdinalIgnoreCase) }).Count) {
        Write-Warning "Skipping '$($scenario.Name)': no eShopAppHost.csproj yet. It will be included automatically once implemented."
    }
}

if (-not ($DryRun -or $WhatIfPreference)) {
    Write-Header 'Checking AppHost SDK availability'
    if ($DotNetRoot) {
        $DotNetRoot = (Resolve-Path -LiteralPath $DotNetRoot).Path
        $dotnetName = if ($IsWindows) { 'dotnet.exe' } else { 'dotnet' }
        if (-not (Test-Path -LiteralPath (Join-Path $DotNetRoot $dotnetName) -PathType Leaf)) {
            throw 'DotNetRoot must point to an SDK installation containing the dotnet executable.'
        }
    }
    if (-not (Get-Command dotnet -ErrorAction SilentlyContinue) -and -not $DotNetRoot) {
        throw 'Install the .NET SDK required by the AppHost global.json, or specify -DotNetRoot.'
    }
    foreach ($appHost in $appHostProjects) {
        Push-Location -LiteralPath $appHost.DirectoryName
        try {
            $sdkOutput = Invoke-WithDotNet { & dotnet --version 2>&1 }
            $sdkExitCode = $LASTEXITCODE
        }
        finally { Pop-Location }
        if ($sdkExitCode -ne 0 -or ($sdkOutput | Out-String).Trim() -notmatch '^\d+\.\d+\.\d+[\w.+-]*$') {
            throw "The dotnet SDK cannot evaluate '$($appHost.FullName)'. Install the SDK selected by its nearest global.json, or pass -DotNetRoot to that SDK installation. No secrets were written."
        }
        Write-Ok "$(Get-ScenarioName $appHost.FullName): SDK $(($sdkOutput | Out-String).Trim())"
    }
}

#-------------------------------------------------------------------------------
# Collect configuration interactively
#-------------------------------------------------------------------------------

Write-Header 'Azure OpenAI configuration'
Write-Host '  Please enter your Azure OpenAI details.  Press Enter to accept a suggested default.' -ForegroundColor Yellow

# 1. Endpoint
if (-not $Endpoint) {
    if ($NonInteractive) { throw 'Endpoint is required in non-interactive mode.' }
    $Endpoint = Read-Host '  AzureOpenAIEndpoint (e.g. https://<name>.openai.azure.com/)'
}
$endpointUri = $null
if (-not [Uri]::TryCreate($Endpoint, [UriKind]::Absolute, [ref]$endpointUri) -or
    $endpointUri.Scheme -ne 'https' -or -not $endpointUri.Host -or
    $endpointUri.UserInfo -or $endpointUri.Query -or $endpointUri.Fragment) {
    throw 'Endpoint must be an absolute HTTPS inference URL without credentials, query, or fragment.'
}

# 2. API Key (masked)
$apiKeySecure = $ApiKey
if (-not ($DryRun -or $WhatIfPreference)) {
    if (-not $apiKeySecure) {
        if ($NonInteractive) { throw 'ApiKey is required in non-interactive mode.' }
        $apiKeySecure = Read-Host '  AzureOpenAIApiKey (input is masked)' -AsSecureString
    }
    if ($apiKeySecure.Length -eq 0) { throw 'API key cannot be empty.' }
}

# 3. Chat deployment name
if (-not $NonInteractive -and -not $PSBoundParameters.ContainsKey('ChatDeployment')) {
    $chatDeploymentRaw = Read-Host "  AzureOpenAIDeploymentName [default: $ChatDeployment]"
    if ($chatDeploymentRaw.Trim()) { $ChatDeployment = $chatDeploymentRaw.Trim() }
}

# 4. Embeddings deployment name
if (-not $NonInteractive -and -not $PSBoundParameters.ContainsKey('EmbeddingsDeployment')) {
    $embeddingsDeploymentRaw = Read-Host "  AzureOpenAIEmbeddingsDeploymentName [default: $EmbeddingsDeployment]"
    if ($embeddingsDeploymentRaw.Trim()) { $EmbeddingsDeployment = $embeddingsDeploymentRaw.Trim() }
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
    $scenarioSkipped = $false

    Write-Header "Scenario: $scenarioName"

    foreach ($param in $parameters) {

        $displayValue = if ($param.IsSensitive) { '***' } else { $param.Value }

        if ($DryRun) {
            Write-Info "[DRY-RUN] aspire secret set `"$($param.Name)`" `"$displayValue`" --apphost `"$appHostPath`" --non-interactive"
        }
        elseif ($PSCmdlet.ShouldProcess($appHostPath, "Set $($param.Name)")) {
            Write-Info "Setting $($param.Name) = $displayValue"
            $plainValue = $null
            try {
                $plainValue = if ($param.IsSensitive) { ConvertFrom-SecureStringToPlainText $apiKeySecure } else { $param.Value }
                # Do not expose CLI output: it may include the value on success or failure.
                Invoke-WithDotNet {
                    & aspire secret set "$($param.Name)" "$plainValue" --apphost "$appHostPath" --non-interactive *> $null
                }
                $exitCode = $LASTEXITCODE
            }
            finally {
                $plainValue = $null
            }
            if ($exitCode -ne 0) {
                Write-Fail "$($param.Name) (exit $exitCode)"
                if ($exitCode -eq 7) {
                    Write-Warning 'Aspire could not evaluate the AppHost. Check its SDK/global.json and try -DotNetRoot. This is a local setup failure, not an Azure sign-in error.'
                }
                $scenarioOk = $false
            }
            else {
                Write-Ok "$($param.Name)"
            }
        }
        else {
            $scenarioSkipped = $true
        }
    }

    if ($DryRun -or $WhatIfPreference) {
        $status = 'DRY-RUN'
    }
    elseif (-not $scenarioOk) {
        $status = 'FAILED'
        $failCount++
    }
    elseif ($scenarioSkipped) {
        $status = 'SKIPPED'
    }
    else {
        $status = 'OK'
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

if ($DryRun -or $WhatIfPreference) {
    Write-Host "  Dry-run complete. $total scenario(s) would be processed (0 actually executed)." -ForegroundColor Magenta
    exit 0
}

$okCount = @($results | Where-Object { $_.Status -eq 'OK' }).Count

if ($failCount -gt 0) {
    Write-Host "  $okCount / $total scenario(s) succeeded. $failCount FAILED." -ForegroundColor Red
    exit 1
}

if ($okCount -ne $total) {
    Write-Host "  $okCount / $total scenario(s) updated. Some operations were declined; see SKIPPED entries." -ForegroundColor Yellow
    exit 1
}

Write-Host "  All $total scenario(s) updated successfully." -ForegroundColor Green
exit 0
