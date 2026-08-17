[CmdletBinding()]
param()

Set-StrictMode -Version 2.0
$ErrorActionPreference = 'Stop'

function Test-AstInsideFunction {
    param([System.Management.Automation.Language.Ast]$Node)

    $ancestor = $Node.Parent
    while ($null -ne $ancestor) {
        if ($ancestor -is
            [System.Management.Automation.Language.FunctionDefinitionAst]) {
            return $true
        }
        $ancestor = $ancestor.Parent
    }
    $false
}

function Get-FileFingerprint {
    param([string]$Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        return '<missing>'
    }
    $item = Get-Item -LiteralPath $Path
    '{0}|{1}|{2}' -f $item.Length, $item.LastWriteTimeUtc.Ticks,
        (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()
}

$target = Join-Path $PSScriptRoot 'Invoke-AuditionPv60Capture.ps1'
if (-not (Test-Path -LiteralPath $target -PathType Leaf)) {
    throw "Orchestrator script is missing: $target"
}

$tokens = $null
$parseErrors = $null
$ast = [System.Management.Automation.Language.Parser]::ParseFile(
    $target,
    [ref]$tokens,
    [ref]$parseErrors)
if ($parseErrors.Count -gt 0) {
    $details = @($parseErrors | ForEach-Object {
        '{0}:{1}: {2}' -f $_.Extent.StartLineNumber,
            $_.Extent.StartColumnNumber,
            $_.Message
    }) -join [Environment]::NewLine
    throw "PowerShell parser validation failed:`n$details"
}

# Structural guards make removal/bypass of the production terminal audit fail
# independently from the in-memory 19-call verifier seam.
$allOutputFunctions = @($ast.FindAll({
    param($node)
    $node -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
        $node.Name -eq 'Assert-AllCompletedOutputs'
}, $true))
if ($allOutputFunctions.Count -ne 1) {
    throw 'Expected exactly one Assert-AllCompletedOutputs function.'
}
$allOutputFunction = $allOutputFunctions[0]
$captureVerifierCalls = @($allOutputFunction.Body.FindAll({
    param($node)
    $node -is [System.Management.Automation.Language.CommandAst] -and
        $node.GetCommandName() -eq 'Assert-CaptureOutput'
}, $true))
if ($captureVerifierCalls.Count -ne 1 -or
    $allOutputFunction.Extent.Text -notmatch '(?s)for\s*\(\s*\$index\s*=\s*0\s*;\s*\$index\s*-lt\s*19\s*;' -or
    $allOutputFunction.Extent.Text -notmatch '(?s)Assert-CompletedStateMetadata.+-RequireFiles') {
    throw 'Terminal output audit lost its exact 19-loop, real output verifier, or required-file gate.'
}
$topLevelFinalAuditCalls = @($ast.FindAll({
    param($node)
    $node -is [System.Management.Automation.Language.CommandAst] -and
        $node.GetCommandName() -eq 'Assert-AllCompletedOutputs'
}, $true) | Where-Object { -not (Test-AstInsideFunction $_) })
if ($topLevelFinalAuditCalls.Count -ne 1) {
    throw 'Main orchestration must invoke the final 19-output audit exactly once.'
}
$topLevelCompleteAssignments = @($ast.FindAll({
    param($node)
    $node -is [System.Management.Automation.Language.AssignmentStatementAst] -and
        $node.Left.Extent.Text -eq '$script:State.status' -and
        $node.Right.Extent.Text -eq "'complete'"
}, $true) | Where-Object { -not (Test-AstInsideFunction $_) })
if ($topLevelCompleteAssignments.Count -ne 1 -or
    $topLevelFinalAuditCalls[0].Extent.EndOffset -ge
        $topLevelCompleteAssignments[0].Extent.StartOffset) {
    throw 'Final 19-output audit must precede the complete-state assignment.'
}

$gitReadFunctions = @($ast.FindAll({
    param($node)
    $node -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
        $node.Name -eq 'Invoke-GitRead'
}, $true))
$directGitCalls = @($ast.FindAll({
    param($node)
    $node -is [System.Management.Automation.Language.CommandAst] -and
        $node.GetCommandName() -eq 'git'
}, $true))
if ($gitReadFunctions.Count -ne 1 -or $directGitCalls.Count -ne 1 -or
    $gitReadFunctions[0].Extent.Text -notmatch
        '(?s)&\s+git\s+--no-optional-locks\s+-C\s+\$ProjectRoot') {
    throw 'Every orchestrator Git read must use the single no-optional-locks gateway.'
}

$dryRunBranches = @($ast.FindAll({
    param($node)
    $node -is [System.Management.Automation.Language.IfStatementAst] -and
        $node.Extent.Text -match '^if\s*\(\$DryRun\)'
}, $true) | Where-Object { -not (Test-AstInsideFunction $_) })
$topLevelCaptureCalls = @($ast.FindAll({
    param($node)
    $node -is [System.Management.Automation.Language.CommandAst] -and
        $node.GetCommandName() -eq 'Invoke-CaptureRun'
}, $true) | Where-Object { -not (Test-AstInsideFunction $_) })
if ($dryRunBranches.Count -ne 1 -or $topLevelCaptureCalls.Count -ne 1 -or
    $dryRunBranches[0].Extent.StartOffset -ge
        $topLevelCaptureCalls[0].Extent.StartOffset) {
    throw 'DryRun must return before the main live capture call.'
}

$result = @(& $target -SelfTest)
$json = ($result -join [Environment]::NewLine) | ConvertFrom-Json
if ($json.status -ne 'passed' -or [int]$json.runCount -ne 19 -or
    [int]$json.mutationCount -ne 15 -or
    [int]$json.evidenceReceiptCheckCount -ne 37 -or
    [int]$json.finalOutputVerifierCallCount -ne 19 -or
    $json.mutexExclusion -ne 'passed-two-process-named-mutex-probe') {
    throw 'Orchestrator self-test did not return the exact passing 19-run contract.'
}

# Exercise the real DryRun entry point. It may return ready on a clean tree or
# fail closed on current external state; either outcome must leave the unique C
# RunRoot, Git index, and guarded tool files byte-for-byte untouched.
$projectRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$headOutput = @(& git --no-optional-locks -C $projectRoot rev-parse HEAD 2>&1)
if ($LASTEXITCODE -ne 0) {
    throw "Could not read HEAD without optional locks: $($headOutput -join ' ')"
}
$head = ($headOutput -join '').Trim()
$probeRunRoot = Join-Path ([System.IO.Path]::GetTempPath()) `
    ('DimensionBrawl-PV60-DryRun-NoWrite-' + [Guid]::NewGuid().ToString('N'))
if (Test-Path -LiteralPath $probeRunRoot) {
    throw "DryRun probe path unexpectedly exists: $probeRunRoot"
}
$guardedPaths = @(
    $target,
    $PSCommandPath,
    (Join-Path $PSScriptRoot 'README.md'),
    (Join-Path $projectRoot '.git\index')
)
$beforeFingerprints = @{}
foreach ($path in $guardedPaths) {
    $beforeFingerprints[$path] = Get-FileFingerprint $path
}

$dryRunOutcome = 'ready'
$dryRunFailure = ''
$dryRunResult = @()
try {
    $dryRunResult = @(& $target -PinnedHead $head -RunRoot $probeRunRoot `
        -DryRun 2>&1)
}
catch {
    $dryRunOutcome = 'expected-fail-closed-on-current-external-state'
    $dryRunFailure = $_.Exception.Message
}
if ($dryRunOutcome -eq 'ready') {
    $dryRunJson = ($dryRunResult -join [Environment]::NewLine) |
        ConvertFrom-Json
    if ($dryRunJson.status -ne 'ready' -or
        [bool]$dryRunJson.writesPerformed -or
        [bool]$dryRunJson.unityStarted) {
        throw 'DryRun returned a non-ready or mutating result.'
    }
}

$changedGuardedPaths = New-Object System.Collections.Generic.List[string]
foreach ($path in $guardedPaths) {
    if ((Get-FileFingerprint $path) -cne $beforeFingerprints[$path]) {
        $changedGuardedPaths.Add($path)
    }
}
$probeRunRootCreated = Test-Path -LiteralPath $probeRunRoot
$writesPerformed = $probeRunRootCreated -or $changedGuardedPaths.Count -gt 0
if ($writesPerformed) {
    throw "DryRun write regression: RunRootCreated=$probeRunRootCreated; changed=$($changedGuardedPaths -join ', ')"
}

[pscustomobject][ordered]@{
    status = 'passed'
    parserErrorCount = $parseErrors.Count
    tokenCount = $tokens.Count
    runCount = [int]$json.runCount
    mutationCount = [int]$json.mutationCount
    evidenceReceiptCheckCount = [int]$json.evidenceReceiptCheckCount
    finalOutputVerifierCallCount = [int]$json.finalOutputVerifierCallCount
    terminalAuditStructure = 'passed-main-call-real-verifier-exact-19-loop'
    mutexExclusion = [string]$json.mutexExclusion
    scheduleSha256 = [string]$json.scheduleSha256
    targetSha256 = (Get-FileHash -LiteralPath $target -Algorithm SHA256).Hash.ToLowerInvariant()
    dryRunOutcome = $dryRunOutcome
    dryRunFailure = $dryRunFailure
    probeRunRootCreated = $probeRunRootCreated
    changedGuardedPathCount = $changedGuardedPaths.Count
    writesPerformed = $writesPerformed
} | ConvertTo-Json -Depth 5
