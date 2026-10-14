<#
.SYNOPSIS
    Creates or reuses a Foundry account, deploys chat and embedding models, and sets local AppHost secrets.
.DESCRIPTION
    Requires PowerShell 7, an authenticated Azure CLI, and the Aspire CLI.
    Explicit subscription, resource group, account name and location are required.
    Validates model availability, SKU support and quota before provisioning.
    Does not replace conflicting deployments or enable key authentication on existing accounts.
    Configures common model secrets, not scenario-specific services such as DeepSeek or realtime audio.
.PARAMETER DryRun
    Prints the intended operations without Azure calls, secret reads, or local writes.
.EXAMPLE
    .\scripts\Initialize-FoundryModels.ps1 -SubscriptionId '<subscription>' -ResourceGroupName 'rg-eshoplite' -AccountName '<unique-name>' -Location 'eastus2' -DryRun
.EXAMPLE
    .\scripts\Initialize-FoundryModels.ps1 -SubscriptionId '<subscription>' -ResourceGroupName 'rg-eshoplite' -AccountName '<unique-name>' -Location 'eastus2'
#>
#requires -Version 7.0
[CmdletBinding(SupportsShouldProcess)]
param(
    [Parameter(Mandatory)][ValidateNotNullOrEmpty()][string]$SubscriptionId,
    [Parameter(Mandatory)][ValidateNotNullOrEmpty()][string]$ResourceGroupName,
    [Parameter(Mandatory)][ValidatePattern('^[a-zA-Z0-9][a-zA-Z0-9-]{1,62}$')][string]$AccountName,
    [Parameter(Mandatory)][ValidateNotNullOrEmpty()][string]$Location,
    [ValidateNotNullOrEmpty()][string]$ChatModel = 'gpt-6-luna',
    [string]$ChatModelVersion,
    [ValidateNotNullOrEmpty()][string]$ChatDeployment = 'gpt-6-luna',
    [ValidateNotNullOrEmpty()][string]$ChatSku = 'GlobalStandard',
    [ValidateRange(1, 1000000)][int]$ChatCapacity = 1,
    [ValidateNotNullOrEmpty()][string]$EmbeddingsModel = 'text-embedding-3-small',
    [string]$EmbeddingsModelVersion,
    [ValidateNotNullOrEmpty()][string]$EmbeddingsDeployment = 'text-embedding-3-small',
    [ValidateNotNullOrEmpty()][string]$EmbeddingsSku = 'Standard',
    [ValidateRange(1, 1000000)][int]$EmbeddingsCapacity = 1,
    [string]$ScenariosRoot = (Join-Path $PSScriptRoot '..\scenarios'),
    [switch]$DryRun
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Invoke-AzureJson {
    param([string[]]$Arguments)
    # Never forward raw output or arguments: key retrieval output is sensitive.
    $output = & az @Arguments --subscription $SubscriptionId --only-show-errors --output json 2>$null
    if ($LASTEXITCODE -ne 0) {
        throw "Azure CLI operation '$($Arguments[0..1] -join ' ')' failed. Check authentication, permissions, region, and quota."
    }
    try { $output | ConvertFrom-Json -Depth 100 }
    finally { $output = $null }
}

function Resolve-Model {
    param([string]$Name, [string]$Version, [string]$Sku, [int]$Capacity)
    $matches = @($catalog | Where-Object { $null -ne $_ -and $_.model.name -eq $Name -and $_.model.format -eq 'OpenAI' })
    if ($Version) {
        $matches = @($matches | Where-Object { $_.model.version -eq $Version })
    }
    else {
        $matches = @($matches | Where-Object {
            $_.model.PSObject.Properties['isDefaultVersion'] -and $_.model.isDefaultVersion
        })
    }
    if ($matches.Count -ne 1) {
        throw "Model '$Name' has no unique available $(if ($Version) { "version '$Version'" } else { 'default version' }) in '$Location'. Specify a catalog version or choose another region/model. No fallback model will be substituted."
    }
    $entry = $matches[0]
    $skus = if ($entry.PSObject.Properties['skus']) {
        @($entry.skus)
    } elseif ($entry.model.PSObject.Properties['skus']) {
        @($entry.model.skus)
    } else { @() }
    $supported = @($skus | Where-Object { $_.name -eq $Sku })
    if ($supported.Count -ne 1) { throw "Model '$Name' version '$($entry.model.version)' does not support SKU '$Sku' in '$Location'." }
    $limits = $supported[0]
    if ($limits.PSObject.Properties['capacity'] -and $limits.capacity) {
        $range = $limits.capacity
        if (($range.PSObject.Properties['minimum'] -and $Capacity -lt $range.minimum) -or
            ($range.PSObject.Properties['maximum'] -and $Capacity -gt $range.maximum) -or
            ($range.PSObject.Properties['step'] -and $range.step -gt 0 -and $Capacity % $range.step -ne 0)) {
            throw "Capacity '$Capacity' is outside the supported range or step for '$Name' / '$Sku'."
        }
    }
    [pscustomobject]@{ Name = $Name; Version = $entry.model.version; Sku = $Sku; Capacity = $Capacity }
}

if ($ChatDeployment -eq $EmbeddingsDeployment) { throw 'Chat and embeddings deployment names must be different.' }
$ScenariosRoot = (Resolve-Path $ScenariosRoot).Path
if (-not @(Get-ChildItem $ScenariosRoot -Recurse -Filter eShopAppHost.csproj).Count) {
    throw "No eShopAppHost.csproj found under '$ScenariosRoot'."
}
Write-Host "Target: subscription=$SubscriptionId; resource group=$ResourceGroupName; account=$AccountName; region=$Location"
Write-Host "Chat: $ChatModel -> $ChatDeployment ($ChatSku, capacity $ChatCapacity)"
Write-Host "Embeddings: $EmbeddingsModel -> $EmbeddingsDeployment ($EmbeddingsSku, capacity $EmbeddingsCapacity)"

if ($DryRun -or $WhatIfPreference) {
    Write-Host 'PREVIEW ONLY: no Azure queries, resource creation, key reads, or secret writes.'
    Write-Host 'Would validate the model catalog, selected versions/SKUs, deployment conflicts, and available quota.'
    Write-Host 'Would create or reuse the resource group and AIServices account, then create missing model deployments.'
    Write-Host 'Model availability is not established by this preview; unsupported models fail without substitution.'
    & (Join-Path $PSScriptRoot 'Set-AzureOpenAISecrets.ps1') -ScenariosRoot $ScenariosRoot `
        -Endpoint "https://$AccountName.openai.azure.com/" -ChatDeployment $ChatDeployment `
        -EmbeddingsDeployment $EmbeddingsDeployment -NonInteractive -DryRun
    exit $LASTEXITCODE
}

foreach ($tool in 'az', 'aspire') {
    if (-not (Get-Command $tool -ErrorAction SilentlyContinue)) { throw "Required CLI '$tool' not found on PATH." }
}

$null = Invoke-AzureJson @('account', 'show')
$catalog = @(Invoke-AzureJson @('cognitiveservices', 'model', 'list', '--location', $Location))
$models = @(
    Resolve-Model $ChatModel $ChatModelVersion $ChatSku $ChatCapacity
    Resolve-Model $EmbeddingsModel $EmbeddingsModelVersion $EmbeddingsSku $EmbeddingsCapacity
)
$models[0] | Add-Member NoteProperty Deployment $ChatDeployment
$models[1] | Add-Member NoteProperty Deployment $EmbeddingsDeployment
$usages = @(Invoke-AzureJson @('cognitiveservices', 'usage', 'list', '--location', $Location))
$groupExists = Invoke-AzureJson @('group', 'exists', '--name', $ResourceGroupName)
$accounts = if ($groupExists) {
    @(Invoke-AzureJson @('cognitiveservices', 'account', 'list', '--resource-group', $ResourceGroupName))
} else { @() }
$existing = @($accounts | Where-Object { $_.name -eq $AccountName })
$deployments = @()
if ($existing.Count -gt 0) {
    $account = $existing[0]
    if ($account.kind -notin @('AIServices', 'OpenAI') -or $account.location -ne $Location) {
        throw 'Existing account kind or location does not match the requested setup.'
    }
    if ($account.properties.PSObject.Properties['disableLocalAuth'] -and $account.properties.disableLocalAuth) {
        throw 'Existing account disables key authentication. This local user-secrets setup will not change that policy.'
    }
    $deployments = @(Invoke-AzureJson @('cognitiveservices', 'account', 'deployment', 'list', '--name', $AccountName, '--resource-group', $ResourceGroupName))
}

foreach ($model in $models) {
    $current = @($deployments | Where-Object { $_.name -eq $model.Deployment })
    if ($current.Count -gt 0) {
        $deployment = $current[0]
        if ($deployment.properties.model.name -ne $model.Name -or
            $deployment.properties.model.version -ne $model.Version -or
            $deployment.sku.name -ne $model.Sku -or $deployment.sku.capacity -ne $model.Capacity -or
            $deployment.properties.provisioningState -ne 'Succeeded') {
            throw "Existing deployment '$($model.Deployment)' conflicts with the requested model/version/SKU/capacity or is not healthy. It will not be overwritten."
        }
    }
    else {
        $quotaName = "OpenAI.$($model.Sku).$($model.Name)"
        $quota = @($usages | Where-Object { $_.name.value -eq $quotaName })
        if ($quota.Count -ne 1 -or ($quota[0].limit - $quota[0].currentValue) -lt $model.Capacity) {
            throw "Insufficient or unknown quota for '$quotaName' in '$Location'. Check quota before retrying."
        }
    }
}

if (-not $PSCmdlet.ShouldProcess("$SubscriptionId/$ResourceGroupName/$AccountName", 'Provision model deployments and configure local AppHost secrets')) { return }
if (-not $groupExists) {
    $null = Invoke-AzureJson @('group', 'create', '--name', $ResourceGroupName, '--location', $Location)
}
if (-not $existing.Count) {
    $null = Invoke-AzureJson @('cognitiveservices', 'account', 'create', '--name', $AccountName,
        '--resource-group', $ResourceGroupName, '--location', $Location, '--kind', 'AIServices',
        '--sku', 'S0', '--custom-domain', $AccountName, '--yes')
}
foreach ($model in $models) {
    if (-not @($deployments | Where-Object { $_.name -eq $model.Deployment }).Count) {
        Write-Host "Deploying $($model.Name) version $($model.Version) as '$($model.Deployment)'."
        $created = Invoke-AzureJson @('cognitiveservices', 'account', 'deployment', 'create',
            '--name', $AccountName, '--resource-group', $ResourceGroupName,
            '--deployment-name', $model.Deployment, '--model-format', 'OpenAI',
            '--model-name', $model.Name, '--model-version', $model.Version,
            '--sku-name', $model.Sku, '--sku-capacity', "$($model.Capacity)")
        if ($created.properties.provisioningState -ne 'Succeeded') {
            throw "Deployment '$($model.Deployment)' did not finish successfully. Secrets have not been configured."
        }
    }
}
$account = Invoke-AzureJson @('cognitiveservices', 'account', 'show', '--name', $AccountName, '--resource-group', $ResourceGroupName)
$endpoint = $account.properties.endpoint
if ($account.properties.PSObject.Properties['endpoints'] -and
    $account.properties.endpoints -and $account.properties.endpoints.PSObject.Properties['OpenAI']) {
    $endpoint = $account.properties.endpoints.OpenAI
}
if (-not $endpoint) { throw 'Account did not return an inference endpoint. Secrets have not been configured.' }
$keys = $null
$secureKey = $null
try {
    $keys = Invoke-AzureJson @('cognitiveservices', 'account', 'keys', 'list', '--name', $AccountName, '--resource-group', $ResourceGroupName)
    if (-not $keys.key1) { throw 'Account did not return an API key. Secrets have not been configured.' }
    $secureKey = ConvertTo-SecureString $keys.key1 -AsPlainText -Force
    $keys = $null
    & (Join-Path $PSScriptRoot 'Set-AzureOpenAISecrets.ps1') -Endpoint $endpoint -ApiKey $secureKey `
        -ChatDeployment $ChatDeployment -EmbeddingsDeployment $EmbeddingsDeployment `
        -ScenariosRoot $ScenariosRoot -NonInteractive -Confirm:$false
    if ($LASTEXITCODE -ne 0) { throw 'Model deployments are ready, but some local secrets failed. Review the scenario summary and rerun.' }
}
finally {
    $keys = $null
    if ($secureKey) { $secureKey.Dispose() }
}
Write-Host 'Common chat/embedding deployments and local AppHost secrets are ready.'
