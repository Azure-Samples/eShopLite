#requires -Version 7.0
<#
.SYNOPSIS
    Runs offline regression checks with mocked Azure and Aspire commands. No cloud or secret writes.
.EXAMPLE
    pwsh .\scripts\Tests\Test-FoundrySetup.ps1
#>
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = Join-Path ([IO.Path]::GetTempPath()) ("eshoplite-setup-test-" + [Guid]::NewGuid().ToString('N'))
$scriptsRoot = Split-Path $PSScriptRoot -Parent
$initialize = Join-Path $scriptsRoot 'Initialize-FoundryModels.ps1'
$setSecrets = Join-Path $scriptsRoot 'Set-AzureOpenAISecrets.ps1'
$fixture = @{
    Case = 'new'
    Calls = [Collections.Generic.List[string]]::new()
}

function Assert-True {
    param([bool]$Condition, [string]$Message)
    if (-not $Condition) { throw "Assertion failed: $Message" }
}

function az {
    param([Parameter(ValueFromRemainingArguments)][string[]]$Arguments)
    $global:LASTEXITCODE = 0
    $command = $Arguments -join ' '
    $fixture.Calls.Add($command)
    $account = @{
        name = 'eshoplite-test'; kind = 'AIServices'; location = 'eastus2'
        properties = @{ endpoint = 'https://eshoplite-test.cognitiveservices.azure.com/'
            endpoints = @{ OpenAI = 'https://eshoplite-test.openai.azure.com/' }
            disableLocalAuth = ($fixture.Case -eq 'disabled-auth') }
    }
    $existing = $fixture.Case -in @('existing', 'conflict', 'disabled-auth')
    $result = switch -Regex ($command) {
        '^account show ' { @{ id = 'offline-test' }; break }
        '^cognitiveservices model list ' {
            if ($fixture.Case -eq 'unavailable') { @() }
            else {
                @(
                    @{ model = @{ name = 'gpt-6-luna'; version = 'test-chat-version'; format = 'OpenAI'; isDefaultVersion = $true }
                        skus = @(@{ name = 'GlobalStandard'; capacity = @{ minimum = 1; maximum = 10; step = 1 } }) }
                    @{ model = @{ name = 'text-embedding-3-small'; version = '1'; format = 'OpenAI'; isDefaultVersion = $true }
                        skus = @(@{ name = 'Standard'; capacity = @{ minimum = 1; maximum = 10; step = 1 } }) }
                )
            }
            break
        }
        '^cognitiveservices usage list ' {
            @('OpenAI.GlobalStandard.gpt-6-luna', 'OpenAI.Standard.text-embedding-3-small') |
                ForEach-Object { @{ name = @{ value = $_ }; limit = $(if ($fixture.Case -eq 'no-quota') { 0 } else { 10 }); currentValue = 0 } }
            break
        }
        '^group exists ' { $existing; break }
        '^cognitiveservices account list ' { @($account); break }
        '^cognitiveservices account deployment list ' {
            @(
                @{ name = 'gpt-6-luna'; sku = @{ name = 'GlobalStandard'; capacity = 1 }
                    properties = @{ provisioningState = 'Succeeded'; model = @{ name = 'gpt-6-luna'; version = $(if ($fixture.Case -eq 'conflict') { 'conflict' } else { 'test-chat-version' }) } } }
                @{ name = 'text-embedding-3-small'; sku = @{ name = 'Standard'; capacity = 1 }
                    properties = @{ provisioningState = 'Succeeded'; model = @{ name = 'text-embedding-3-small'; version = '1' } } }
            )
            break
        }
        '^cognitiveservices account show ' { $account; break }
        '^cognitiveservices account keys list ' { @{ key1 = 'offline-key-redaction-sentinel' }; break }
        '^cognitiveservices account deployment create ' { @{ properties = @{ provisioningState = 'Succeeded' } }; break }
        '^(group create|cognitiveservices account create) ' { @{}; break }
        default { throw "Unexpected mocked Azure operation: $command" }
    }
    if ($fixture.Case -eq 'unavailable' -and $command -match '^cognitiveservices model list ') {
        '[]'
    }
    else { ConvertTo-Json -InputObject $result -Depth 20 -Compress }
}

function aspire {
    param([Parameter(ValueFromRemainingArguments)][string[]]$Arguments)
    $fixture.Calls.Add("aspire:$($Arguments[2])")
    $global:LASTEXITCODE = if ($fixture.Case -eq 'secret-failure') { 1 } else { 0 }
    # Simulate a CLI that echoes sensitive values. The script must suppress it.
    Write-Output ($Arguments -join ' ')
}

function dotnet {
    param([Parameter(ValueFromRemainingArguments)][string[]]$Arguments)
    if ($Arguments.Count -ne 1 -or $Arguments[0] -ne '--version') {
        throw 'Unexpected mocked SDK operation.'
    }
    $global:LASTEXITCODE = if ($fixture.Case -eq 'missing-sdk') { 1 } else { 0 }
    if ($fixture.Case -eq 'missing-sdk') { 'A compatible .NET SDK was not found.' }
    else { '10.0.402' }
}

try {
    $null = New-Item (Join-Path $root '01-Baseline\src\eShopAppHost') -ItemType Directory -Force
    $null = New-Item (Join-Path $root '01-Baseline\src\eShopAppHost\eShopAppHost.csproj') -ItemType File
    $null = New-Item (Join-Path $root '18-BlazorAgenticUI') -ItemType Directory
    $options = @{
        SubscriptionId = 'offline-test'; ResourceGroupName = 'rg-test'; AccountName = 'eshoplite-test'
        Location = 'eastus2'; ScenariosRoot = $root; Confirm = $false
    }
    foreach ($script in $initialize, $setSecrets) {
        $tokens = $null
        $errors = $null
        $null = [Management.Automation.Language.Parser]::ParseFile($script, [ref]$tokens, [ref]$errors)
        Assert-True ($errors.Count -eq 0) "Syntax: $script"
    }

    $output = & $initialize @options -DryRun 6>&1 3>&1 | Out-String
    Assert-True ($LASTEXITCODE -eq 0 -and $fixture.Calls.Count -eq 0) 'DryRun must not call Azure or Aspire.'
    Assert-True ($output.Contains('gpt-6-luna') -and $output.Contains("Skipping '18-BlazorAgenticUI'")) 'Default model and plan-only scenario warning.'
    $null = & $initialize @options -WhatIf 6>&1 3>&1
    Assert-True ($fixture.Calls.Count -eq 0) 'WhatIf must not call Azure or Aspire.'
    Write-Output 'PASS: offline previews, model default, and plan-only warning'

    $output = & $initialize @options 6>&1 3>&1 | Out-String
    Assert-True ($LASTEXITCODE -eq 0) 'New resource setup succeeds.'
    Assert-True (@($fixture.Calls | Where-Object { $_ -match '^cognitiveservices account deployment create ' }).Count -eq 2) 'Creates exactly two deployments.'
    Assert-True (@($fixture.Calls | Where-Object { $_ -match '^aspire:' }).Count -eq 4) 'Writes exactly four secrets for a single AppHost.'
    Assert-True (-not $output.Contains('offline-key-redaction-sentinel')) 'API key must not appear in output.'
    Write-Output 'PASS: new resources, single AppHost, and key redaction'

    $null = New-Item (Join-Path $root '18-BlazorAgenticUI\eShopAppHost') -ItemType Directory
    $null = New-Item (Join-Path $root '18-BlazorAgenticUI\eShopAppHost\eShopAppHost.csproj') -ItemType File
    $fixture.Case = 'existing'
    $fixture.Calls.Clear()
    $output = & $initialize @options 6>&1 3>&1 | Out-String
    Assert-True (-not @($fixture.Calls | Where-Object { $_ -match ' create ' }).Count) 'Matching resources must not be recreated.'
    Assert-True (@($fixture.Calls | Where-Object { $_ -match '^aspire:' }).Count -eq 8) 'Future AppHost discovered, including layout without src.'
    Write-Output 'PASS: idempotent resource reuse and automatic new scenario discovery'

    $expectedErrors = @{
        unavailable = 'no unique available'
        'no-quota' = 'Insufficient or unknown quota'
        conflict = 'conflicts with the requested'
        'disabled-auth' = 'disables key authentication'
    }
    foreach ($case in 'unavailable', 'no-quota', 'conflict', 'disabled-auth') {
        $fixture.Case = $case
        $fixture.Calls.Clear()
        $failed = $false
        try { $null = & $initialize @options 6>&1 3>&1 }
        catch {
            $failed = $true
            Assert-True ($_.Exception.Message.Contains($expectedErrors[$case])) "Correct failure reason for ${case}: $($_.Exception.Message)"
        }
        Assert-True $failed "Expected failure for $case."
        Assert-True (-not @($fixture.Calls | Where-Object { $_ -match ' create |^aspire:| keys list ' }).Count) "No mutation or key access for $case."
        Write-Output "PASS: preflight failure without mutations ($case)"
    }

    $fixture.Case = 'new'
    $fixture.Calls.Clear()
    $secretOptions = @{ ScenariosRoot = $root; Endpoint = 'https://eshoplite-test.openai.azure.com/'; NonInteractive = $true }
    $fixture.Case = 'missing-sdk'
    $failed = $false
    try { $null = & $setSecrets @secretOptions 6>&1 3>&1 }
    catch {
        $failed = $true
        Assert-True ($_.Exception.Message.Contains('global.json')) 'SDK failures must explain SDK selection.'
    }
    Assert-True ($failed -and $fixture.Calls.Count -eq 0) 'Missing SDK must fail before requesting a key or writing secrets.'
    $fixture.Case = 'new'
    foreach ($scenario in '01-Baseline', '18-BlazorAgenticUI') {
        $output = & $setSecrets -ScenariosRoot (Join-Path $root $scenario) -Endpoint $secretOptions.Endpoint -NonInteractive -DryRun 6>&1 3>&1 | Out-String
        Assert-True ($output.Contains("Scenario: $scenario")) 'Scoped setup must display the scenario name for both AppHost layouts.'
    }
    $null = & $setSecrets @secretOptions -WhatIf 6>&1 3>&1
    Assert-True ($fixture.Calls.Count -eq 0) 'Secrets WhatIf must not write.'
    $failed = $false
    try { $null = & $setSecrets @secretOptions 6>&1 3>&1 }
    catch { $failed = $true }
    Assert-True $failed 'Non-interactive writes must require an API key.'
    $failed = $false
    try { $null = & $setSecrets -ScenariosRoot $root -Endpoint 'http://insecure.example' -NonInteractive -DryRun 6>&1 3>&1 }
    catch { $failed = $true }
    Assert-True $failed 'Reject insecure endpoints.'
    $fixture.Case = 'secret-failure'
    $key = ConvertTo-SecureString 'offline-key-redaction-sentinel' -AsPlainText -Force
    $previousRoot = $env:DOTNET_ROOT
    $previousPath = $env:PATH
    $sdkRoot = Join-Path $root 'sdk'
    $null = New-Item $sdkRoot -ItemType Directory
    $dotnetName = if ($IsWindows) { 'dotnet.exe' } else { 'dotnet' }
    $null = New-Item (Join-Path $sdkRoot $dotnetName) -ItemType File
    try { $output = & $setSecrets @secretOptions -ApiKey $key -DotNetRoot $sdkRoot 6>&1 3>&1 | Out-String }
    finally { $key.Dispose() }
    Assert-True ($LASTEXITCODE -eq 1 -and -not $output.Contains('offline-key-redaction-sentinel')) 'Secret CLI failures must return nonzero and redact keys.'
    Assert-True ($env:DOTNET_ROOT -eq $previousRoot -and $env:PATH -eq $previousPath) 'SDK overrides must restore the caller environment even after failure.'
    Write-Output 'PASS: missing keys, insecure endpoints, WhatIf, and failed-secret redaction'
    Write-Output 'All offline setup checks passed.'
}
finally {
    # Only remove the exact disposable fixture created above.
    if (Test-Path $root) { Remove-Item -LiteralPath $root -Recurse -Force }
}
exit 0
