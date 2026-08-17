[CmdletBinding()]
param(
    [ValidatePattern('^[0-9a-fA-F]{40}$')]
    [string]$PinnedHead = '',

    [string]$UnityExe =
        'C:\Program Files\Unity\Hub\Editor\6000.3.5f2\Editor\Unity.exe',

    [string]$ProjectRoot = 'C:\Git\DimensionBrawl',

    [string]$OutputRoot =
        'D:\DimensionBrawl_PV\01_capture_video\PREEDIT_GOLD',

    [string]$RunRoot =
        'C:\tmp\DimensionBrawl-PV60-Capture-Orchestrator',

    [string]$StatePath = '',

    [switch]$Resume,
    [switch]$DryRun,
    [switch]$SelfTest,

    # Internal, zero-filesystem-write synchronization probe used only by -SelfTest.
    [string]$MutexProbeHold = '',
    [string]$MutexProbeReadyEvent = '',
    [string]$MutexProbeReleaseEvent = '',

    [ValidateRange(1, 360)]
    [int]$PollSeconds = 5,

    [ValidateRange(10, 1440)]
    [int]$RunTimeoutMinutes = 180,

    [ValidateRange(1, 1024)]
    [double]$MinimumAvailableRamGiB = 10,

    [ValidateRange(1, 2048)]
    [double]$MinimumCommitHeadroomGiB = 24,

    [ValidateRange(1, 2048)]
    [double]$MinimumCFreeGiB = 30,

    [ValidateRange(1, 8192)]
    [double]$MinimumDFreeGiB = 256,

    [ValidateRange(1, 1024)]
    [double]$CriticalAvailableRamGiB = 3,

    [ValidateRange(1, 2048)]
    [double]$CriticalCommitHeadroomGiB = 8,

    [ValidateRange(1, 2048)]
    [double]$CriticalCFreeGiB = 10,

    [ValidateRange(1, 8192)]
    [double]$CriticalDFreeGiB = 64
)

Set-StrictMode -Version 2.0
$ErrorActionPreference = 'Stop'

$script:ManifestSchema = 'dimension-brawl.audition-pv.capture-manifest.v1'
$script:EvidenceReceiptSchema =
    'dimension-brawl.audition-pv.sixty-second-evidence-bundle.v1'
$script:ExpectedSourceFormat = 'png_sequence_srgb_8bit_lossless'
$script:StateSchema = 'dimension-brawl.audition-pv.pv60-capture-orchestrator.v2'
$script:ReportSchema = 'dimension-brawl.audition-pv.pv60-capture-report.v2'
$script:EventSchema = 'dimension-brawl.audition-pv.pv60-capture-event.v2'
$script:ExpectedProjectRoot = 'C:\Git\DimensionBrawl'
$script:ExpectedOutputRoot =
    'D:\DimensionBrawl_PV\01_capture_video\PREEDIT_GOLD'
$script:ExpectedUnityExe =
    'C:\Program Files\Unity\Hub\Editor\6000.3.5f2\Editor\Unity.exe'
$script:ExpectedUnityVersion = '6000.3.5f2'
$script:ExpectedUnityRevision = '3fa8bc678cb0'
$script:ExpectedUnityVersionWithRevision =
    '6000.3.5f2 (3fa8bc678cb0)'
$script:ExpectedUnityProductVersion = '6000.3.5f2_3fa8bc678cb0'
$script:ExpectedRecorderVersion = '5.1.6'
$script:ExpectedUrpVersion = '17.3.0'
$script:ExpectedRenderPipelineAsset = 'Assets/Settings/PC_RPAsset.asset'
$script:GoldenScheduleSha256 =
    '10ff100cb0e40b854967e159d9122028e5c67bc663d6e4c182aefdb47557f538'
$script:OrchestratorMutexName =
    'Local\DimensionBrawl_AuditionPV_PV60_Capture_6000_3_5f2'
$script:OwnedUnityProcess = $null
$script:EventJournalPath = ''
$script:ReportPath = ''
$script:State = $null
$script:OrchestratorMutex = $null
$script:OrchestratorMutexAcquired = $false
$script:SelfTestMutationCount = 0

function New-EvidenceRange {
    param(
        [string]$ShotId,
        [int]$SourceStart,
        [int]$SourceEnd,
        [int]$SelectStart,
        [int]$SelectEnd
    )

    [pscustomobject][ordered]@{
        shotId = $ShotId
        sourceStart = $SourceStart
        sourceEnd = $SourceEnd
        selectStart = $SelectStart
        selectEnd = $SelectEnd
    }
}

function New-ShotContract {
    param([string]$Id, [int]$Start, [int]$End)

    [pscustomobject][ordered]@{
        id = $Id
        startFrame = $Start
        endFrame = $End
        expectedFrameCount = $End - $Start + 1
    }
}

function New-FamilyDefinition {
    param(
        [string]$Id,
        [string]$Method,
        [bool]$Headful,
        [object[]]$Shots,
        [object[]]$EvidenceRanges,
        [string[]]$RequiredFiles,
        [string[]]$FailureFiles,
        [string[]]$ExtraArguments = @()
    )

    [pscustomobject][ordered]@{
        id = $Id
        method = $Method
        headful = $Headful
        shots = @($Shots)
        evidenceRanges = @($EvidenceRanges)
        requiredFiles = @($RequiredFiles)
        failureFiles = @($FailureFiles)
        extraArguments = @($ExtraArguments)
    }
}

function Get-FamilyDefinitions {
    $commonFailures = @('CAPTURE_FAILED.txt')

    $city = New-FamilyDefinition -Id 'city' -Headful $true `
        -Method 'DimensionBrawl.Editor.AuditionPV.AuditionPvCityHeroPocketGoldenRunner.RunBatchCapture' `
        -Shots @(
            (New-ShotContract 'g01' 0 599),
            (New-ShotContract 'g02' 0 779),
            (New-ShotContract 'g03' 0 659)
        ) `
        -EvidenceRanges @(
            (New-EvidenceRange 'g01' 0 539 180 359),
            (New-EvidenceRange 'g02' 60 779 240 599),
            (New-EvidenceRange 'g03' 0 419 180 239),
            (New-EvidenceRange 'g03' 60 659 240 479)
        ) `
        -RequiredFiles @(
            'city_g01_g03_runner_state.json',
            'evidence/city_g01_g03_runtime_proof.json',
            'evidence/frame_hashes.sha256'
        ) `
        -FailureFiles ($commonFailures + @('city_g01_g03_capture_failure.json'))

    $s030 = New-FamilyDefinition -Id 's030' -Headful $true `
        -Method 'DimensionBrawl.Editor.AuditionPV.AuditionPvCityHitDodgeSummonGoldenRunner.RunBatchCapture' `
        -Shots @((New-ShotContract 's030' 0 719)) `
        -EvidenceRanges @((New-EvidenceRange 's030' 0 719 180 539)) `
        -RequiredFiles @(
            's030_runner_state.json',
            'evidence/s030_runtime_proof.json',
            'evidence/s030_source_ledger.json',
            'evidence/recorder_padding_raw_frame_0000.png'
        ) `
        -FailureFiles ($commonFailures + @('s030_capture_failure.json'))

    $s050 = New-FamilyDefinition -Id 's050' -Headful $true `
        -Method 'DimensionBrawl.Editor.AuditionPV.AuditionPvStationPhaseOneBossLowAngleGoldenRunner.RunBatchCapture' `
        -Shots @((New-ShotContract 's050' 0 599)) `
        -EvidenceRanges @((New-EvidenceRange 's050' 0 599 180 419)) `
        -RequiredFiles @(
            's050_batch_state.json',
            'evidence/s050_runtime_proof.json',
            'evidence/s050_source_ledger.json',
            'evidence/s050_source_frames.sha256',
            'evidence/s050_shot_authorship.json',
            'evidence/recorder_padding_raw_frame_0000.png'
        ) `
        -FailureFiles ($commonFailures + @('s050_capture_failure.json')) `
        -ExtraArguments @('-s050TakeOrdinal=1')

    $g04 = New-FamilyDefinition -Id 'g04' -Headful $false `
        -Method 'DimensionBrawl.Editor.AuditionPV.AuditionPvStationTransitionGoldenCapture.RunBatchCapture' `
        -Shots @(
            (New-ShotContract 'g04' 0 597),
            (New-ShotContract 'g04-clean' 0 597)
        ) `
        -EvidenceRanges @(
            (New-EvidenceRange 'g04' 0 479 180 299),
            (New-EvidenceRange 'g04' 118 597 298 417),
            (New-EvidenceRange 'g04-clean' 118 597 298 417)
        ) `
        -RequiredFiles @(
            'evidence/g04_runtime_proof.json',
            'evidence/frame_hashes.sha256',
            'evidence/g04_clean_plate_companion_proof.json'
        ) `
        -FailureFiles $commonFailures

    $g06 = New-FamilyDefinition -Id 'g06' -Headful $true `
        -Method 'DimensionBrawl.Editor.AuditionPV.AuditionPvStationPhase2SummonCounterGoldenRunner.RunBatchCapture' `
        -Shots @((New-ShotContract 'g06' 0 719)) `
        -EvidenceRanges @(
            (New-EvidenceRange 'g06' 0 659 180 479),
            (New-EvidenceRange 'g06' 60 719 240 539)
        ) `
        -RequiredFiles @(
            'g06_runner_state.json',
            'evidence/g06_runtime_proof.json',
            'evidence/frame_hashes.sha256',
            'evidence/recorder_warmup_raw_frame_0000.png'
        ) `
        -FailureFiles ($commonFailures + @('g06_capture_failure.json'))

    $g07 = New-FamilyDefinition -Id 'g07' -Headful $true `
        -Method 'DimensionBrawl.Editor.AuditionPV.AuditionPvStationPhase2PatternRelayGoldenRunner.RunBatchCapture' `
        -Shots @((New-ShotContract 'g07' 0 779)) `
        -EvidenceRanges @((New-EvidenceRange 'g07' 0 779 180 599)) `
        -RequiredFiles @(
            'g07_runner_state.json',
            'evidence/g07_runtime_proof.json',
            'evidence/frame_hashes.sha256',
            'evidence/g07_shot_authorship.json',
            'evidence/recorder_warmup_raw_frame_0000.png'
        ) `
        -FailureFiles ($commonFailures + @('g07_capture_failure.json'))

    $g08 = New-FamilyDefinition -Id 'g08' -Headful $true `
        -Method 'DimensionBrawl.Editor.AuditionPV.AuditionPvStationBossDeathAftermathGoldenRunner.RunBatchCapture' `
        -Shots @((New-ShotContract 'g08' 0 719)) `
        -EvidenceRanges @((New-EvidenceRange 'g08' 60 719 240 539)) `
        -RequiredFiles @(
            'g08_runner_state.json',
            'evidence/g08_runtime_proof.json',
            'evidence/frame_hashes.sha256',
            'evidence/g08_shot_authorship.json',
            'evidence/recorder_warmup_raw_frame_0000.png'
        ) `
        -FailureFiles ($commonFailures + @('g08_capture_failure.json'))

    [ordered]@{
        city = $city
        s030 = $s030
        s050 = $s050
        g04 = $g04
        g06 = $g06
        g07 = $g07
        g08 = $g08
    }
}

function Get-RunSchedule {
    param([System.Collections.IDictionary]$Families)

    $order = @(
        'g04', 's050', 'g08', 'g07', 'g06', 's030', 'city',
        'g04',         'g08', 'g07', 'g06', 's030', 'city',
        'g04',         'g08', 'g07', 'g06', 's030', 'city'
    )
    $ordinals = @{}
    $schedule = New-Object System.Collections.Generic.List[object]
    for ($index = 0; $index -lt $order.Count; $index++) {
        $familyId = $order[$index]
        if (-not $ordinals.ContainsKey($familyId)) {
            $ordinals[$familyId] = 0
        }
        $ordinals[$familyId] = [int]$ordinals[$familyId] + 1
        $definition = $Families[$familyId]
        $schedule.Add([pscustomobject][ordered]@{
            sequence = $index + 1
            runId = ('{0:D2}-{1}-take{2}' -f ($index + 1), $familyId,
                $ordinals[$familyId])
            familyId = $familyId
            takeOrdinal = [int]$ordinals[$familyId]
            definition = $definition
        })
    }
    $schedule.ToArray()
}

function Get-Sha256Text {
    param([string]$Text)

    $sha = [System.Security.Cryptography.SHA256]::Create()
    try {
        $bytes = [System.Text.Encoding]::UTF8.GetBytes($Text)
        $hash = $sha.ComputeHash($bytes)
        -join ($hash | ForEach-Object { $_.ToString('x2') })
    }
    finally {
        $sha.Dispose()
    }
}

function Get-ScheduleDigest {
    param([object[]]$Schedule)

    $contract = @($Schedule | ForEach-Object {
        [pscustomobject][ordered]@{
            sequence = $_.sequence
            runId = $_.runId
            familyId = $_.familyId
            takeOrdinal = $_.takeOrdinal
            method = $_.definition.method
            headful = $_.definition.headful
            shots = $_.definition.shots
            evidenceRanges = $_.definition.evidenceRanges
            requiredFiles = $_.definition.requiredFiles
            failureFiles = $_.definition.failureFiles
            extraArguments = $_.definition.extraArguments
        }
    })
    Get-Sha256Text ($contract | ConvertTo-Json -Depth 10 -Compress)
}

function Get-PropertyValue {
    param([object]$Object, [string]$Name, $Default = $null)

    if ($null -eq $Object) {
        return $Default
    }
    $property = $Object.PSObject.Properties[$Name]
    if ($null -eq $property) {
        return $Default
    }
    $property.Value
}

function Read-JsonBounded {
    param([string]$Path, [long]$MaximumBytes = 67108864)

    $item = Get-Item -LiteralPath $Path -ErrorAction Stop
    if ($item.Extension -ieq '.png') {
        throw "Refusing to load PNG bytes as JSON: $Path"
    }
    if ($item.Length -le 0 -or $item.Length -gt $MaximumBytes) {
        throw "JSON size is outside 1..$MaximumBytes bytes: $Path ($($item.Length))"
    }
    $text = [System.IO.File]::ReadAllText($item.FullName,
        [System.Text.Encoding]::UTF8)
    try {
        $text | ConvertFrom-Json
    }
    catch {
        throw "Invalid JSON at $Path`: $($_.Exception.Message)"
    }
}

function Get-FileSha256 {
    param([string]$Path)

    (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()
}

function Assert-PinnedArtifact {
    param([object]$Pin, [string]$Label)

    $path = [string](Get-PropertyValue $Pin 'path' '')
    $expectedHash = [string](Get-PropertyValue $Pin 'sha256' '')
    if ([string]::IsNullOrWhiteSpace($path)) {
        throw "Pinned artifact path is empty: $Label"
    }
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Pinned artifact is missing: $Label => $path"
    }
    if ($expectedHash -notmatch '^[0-9a-fA-F]{64}$') {
        throw "Pinned artifact SHA-256 is malformed: $Label"
    }
    $actualHash = Get-FileSha256 $path
    if ($actualHash -ne $expectedHash.ToLowerInvariant()) {
        throw "Pinned artifact SHA-256 mismatch: $Label => $path"
    }
}

function Invoke-GitRead {
    param([string[]]$GitArguments)

    # Even read-only Git commands must not take optional index locks or refresh
    # the index; DryRun is contractually zero-write.
    $output = @(& git --no-optional-locks -C $ProjectRoot @GitArguments 2>&1)
    if ($LASTEXITCODE -ne 0) {
        throw "git $($GitArguments -join ' ') failed: $($output -join [Environment]::NewLine)"
    }
    ($output -join "`n").Trim()
}

function Assert-GitPin {
    param([string]$ExpectedHead, [string]$Phase)

    $actualHead = (Invoke-GitRead @('rev-parse', 'HEAD')).ToLowerInvariant()
    if ($actualHead -ne $ExpectedHead.ToLowerInvariant()) {
        throw "Git HEAD changed at $Phase. Expected $ExpectedHead, got $actualHead."
    }
    $status = Invoke-GitRead @('status', '--porcelain=v1', '--untracked-files=all')
    if (-not [string]::IsNullOrWhiteSpace($status)) {
        $preview = (($status -split "`n") | Select-Object -First 20) -join '; '
        throw "Git worktree is not exactly clean at $Phase`: $preview"
    }
    $actualHead
}

function Assert-FrozenCaptureEnvironment {
    Assert-PathEquivalent $ProjectRoot $script:ExpectedProjectRoot `
        'ProjectRoot override'
    Assert-PathEquivalent $OutputRoot $script:ExpectedOutputRoot `
        'OutputRoot override'
    Assert-PathEquivalent $UnityExe $script:ExpectedUnityExe `
        'UnityExe override'

    if (-not (Test-Path -LiteralPath $UnityExe -PathType Leaf)) {
        throw "Frozen Unity executable is missing: $UnityExe"
    }
    $productVersion = [string](
        (Get-Item -LiteralPath $UnityExe).VersionInfo.ProductVersion)
    if ($productVersion.Trim() -ne $script:ExpectedUnityProductVersion) {
        throw "Unity executable ProductVersion mismatch. Expected '$($script:ExpectedUnityProductVersion)', got '$productVersion'."
    }

    $projectVersionPath = Join-Path $ProjectRoot `
        'ProjectSettings\ProjectVersion.txt'
    if (-not (Test-Path -LiteralPath $projectVersionPath -PathType Leaf)) {
        throw "Unity ProjectVersion.txt is missing: $projectVersionPath"
    }
    $projectVersionText = [System.IO.File]::ReadAllText(
        $projectVersionPath,
        [System.Text.Encoding]::UTF8)
    $expectedEditorLine = 'm_EditorVersion: ' + $script:ExpectedUnityVersion
    $expectedRevisionLine = 'm_EditorVersionWithRevision: ' +
        $script:ExpectedUnityVersionWithRevision
    $lines = @($projectVersionText -split "`r?`n")
    if ($lines -notcontains $expectedEditorLine -or
        $lines -notcontains $expectedRevisionLine) {
        throw "ProjectVersion.txt is not pinned to $($script:ExpectedUnityVersionWithRevision)."
    }
}

function Enter-NamedCaptureMutex {
    param([string]$Name)

    $mutex = New-Object System.Threading.Mutex($false, $Name)
    $acquired = $false
    try {
        try {
            $acquired = $mutex.WaitOne(0, $false)
        }
        catch [System.Threading.AbandonedMutexException] {
            # WaitOne transfers ownership for an abandoned mutex.
            $acquired = $true
        }
        if (-not $acquired) {
            throw "PV60 capture mutex is already held by another process: $Name"
        }
        $mutex
    }
    catch {
        if (-not $acquired) {
            $mutex.Dispose()
        }
        throw
    }
}

function Exit-NamedCaptureMutex {
    param([System.Threading.Mutex]$Mutex, [bool]$Acquired)

    if ($null -eq $Mutex) {
        return
    }
    try {
        if ($Acquired) {
            $Mutex.ReleaseMutex()
        }
    }
    finally {
        $Mutex.Dispose()
    }
}

function Enter-OrchestratorMutex {
    $script:OrchestratorMutex =
        Enter-NamedCaptureMutex $script:OrchestratorMutexName
    $script:OrchestratorMutexAcquired = $true
}

function Exit-OrchestratorMutex {
    Exit-NamedCaptureMutex $script:OrchestratorMutex `
        $script:OrchestratorMutexAcquired
    $script:OrchestratorMutex = $null
    $script:OrchestratorMutexAcquired = $false
}

function Get-DriveFreeGiB {
    param([string]$Path)

    $root = [System.IO.Path]::GetPathRoot([System.IO.Path]::GetFullPath($Path))
    $drive = New-Object System.IO.DriveInfo($root)
    [math]::Round($drive.AvailableFreeSpace / 1GB, 3)
}

function Get-ResourceSnapshot {
    $memory = Get-CimInstance Win32_PerfFormattedData_PerfOS_Memory
    [pscustomobject][ordered]@{
        capturedAtUtc = [DateTime]::UtcNow.ToString('O')
        availableRamGiB = [math]::Round($memory.AvailableMBytes / 1024.0, 3)
        commitHeadroomGiB = [math]::Round(
            ([double]$memory.CommitLimit - [double]$memory.CommittedBytes) / 1GB,
            3)
        cFreeGiB = Get-DriveFreeGiB $ProjectRoot
        dFreeGiB = Get-DriveFreeGiB $OutputRoot
    }
}

function Assert-ResourceFloor {
    param([object]$Snapshot, [switch]$Critical)

    if ($Critical) {
        $ram = $CriticalAvailableRamGiB
        $commit = $CriticalCommitHeadroomGiB
        $cFree = $CriticalCFreeGiB
        $dFree = $CriticalDFreeGiB
        $label = 'critical in-run'
    }
    else {
        $ram = $MinimumAvailableRamGiB
        $commit = $MinimumCommitHeadroomGiB
        $cFree = $MinimumCFreeGiB
        $dFree = $MinimumDFreeGiB
        $label = 'pre/post-run'
    }

    $failures = New-Object System.Collections.Generic.List[string]
    if ($Snapshot.availableRamGiB -lt $ram) {
        $failures.Add("RAM $($Snapshot.availableRamGiB) GiB < $ram GiB")
    }
    if ($Snapshot.commitHeadroomGiB -lt $commit) {
        $failures.Add("commit headroom $($Snapshot.commitHeadroomGiB) GiB < $commit GiB")
    }
    if ($Snapshot.cFreeGiB -lt $cFree) {
        $failures.Add("C free $($Snapshot.cFreeGiB) GiB < $cFree GiB")
    }
    if ($Snapshot.dFreeGiB -lt $dFree) {
        $failures.Add("D free $($Snapshot.dFreeGiB) GiB < $dFree GiB")
    }
    if ($failures.Count -gt 0) {
        throw "$label resource floor failed: $($failures -join '; ')"
    }
}

function Get-RelevantProcesses {
    $all = @(Get-CimInstance Win32_Process)
    [pscustomobject]@{
        unity = @($all | Where-Object { $_.Name -ieq 'Unity.exe' })
        media = @($all | Where-Object {
            $_.Name -match '^(AfterFX|aerender|ffmpeg|Adobe Media Encoder|AMECommand)(\.exe)?$'
        })
    }
}

function Format-ProcessList {
    param([object[]]$Processes)

    (@($Processes | ForEach-Object { "$($_.Name)[$($_.ProcessId)]" })) -join ', '
}

function Assert-PreRunProcessExclusion {
    $processes = Get-RelevantProcesses
    if ($processes.media.Count -gt 0) {
        throw 'AE/AME/aerender/ffmpeg must be fully exited: ' +
            (Format-ProcessList $processes.media)
    }
    if ($processes.unity.Count -gt 0) {
        throw 'A Unity.exe process is already running: ' +
            (Format-ProcessList $processes.unity)
    }
}

function Assert-InRunProcessExclusion {
    param([int]$OwnedProcessId)

    $processes = Get-RelevantProcesses
    if ($processes.media.Count -gt 0) {
        throw 'AE/AME/aerender/ffmpeg started during capture: ' +
            (Format-ProcessList $processes.media)
    }
    if ($processes.unity.Count -gt 1) {
        throw 'More than one Unity.exe is running: ' +
            (Format-ProcessList $processes.unity)
    }
    if ($processes.unity.Count -eq 1 -and
        [int]$processes.unity[0].ProcessId -ne $OwnedProcessId) {
        throw 'The only Unity.exe is not the orchestrator-owned PID: ' +
            (Format-ProcessList $processes.unity)
    }
}

function Stop-OwnedUnityProcess {
    param([System.Diagnostics.Process]$Process, [string]$Reason)

    if ($null -eq $Process) {
        return
    }
    try {
        $Process.Refresh()
        if (-not $Process.HasExited) {
            Write-Warning "Stopping exact owned Unity PID $($Process.Id): $Reason"
            Stop-Process -Id $Process.Id -Force -ErrorAction Stop
            Wait-Process -Id $Process.Id -Timeout 30 -ErrorAction SilentlyContinue
        }
    }
    catch {
        Write-Warning "Could not stop exact owned Unity PID $($Process.Id): $($_.Exception.Message)"
    }
}

function Get-CaptureIds {
    if (-not (Test-Path -LiteralPath $OutputRoot -PathType Container)) {
        return @()
    }
    @(Get-ChildItem -LiteralPath $OutputRoot -Directory -ErrorAction Stop |
        Select-Object -ExpandProperty Name)
}

function Get-NewCaptureDirectories {
    param([string[]]$BeforeIds)

    $before = @{}
    foreach ($id in @($BeforeIds)) {
        $before[$id.ToLowerInvariant()] = $true
    }
    @(Get-ChildItem -LiteralPath $OutputRoot -Directory -ErrorAction Stop |
        Where-Object { -not $before.ContainsKey($_.Name.ToLowerInvariant()) } |
        Select-Object -ExpandProperty FullName)
}

function Assert-PathEquivalent {
    param([string]$Actual, [string]$Expected, [string]$Label)

    $a = [System.IO.Path]::GetFullPath($Actual).TrimEnd('\', '/')
    $e = [System.IO.Path]::GetFullPath($Expected).TrimEnd('\', '/')
    if (-not [string]::Equals($a, $e, [StringComparison]::OrdinalIgnoreCase)) {
        throw "$Label mismatch. Expected '$e', got '$a'."
    }
}

function Test-FailureArtifactName {
    param([string]$CandidateName, [string]$ContractName)

    $stem = [System.IO.Path]::GetFileNameWithoutExtension($ContractName)
    $extension = [System.IO.Path]::GetExtension($ContractName)
    $pattern = '^{0}(?:_[0-9]{{17}})?{1}$' -f `
        [regex]::Escape($stem), [regex]::Escape($extension)
    [regex]::IsMatch($CandidateName, $pattern,
        [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
}

function Get-FailureArtifactLocations {
    param([string]$CaptureDirectory)

    @(
        [System.IO.Path]::GetFullPath($CaptureDirectory),
        [System.IO.Path]::GetFullPath(
            (Join-Path $CaptureDirectory 'evidence'))
    )
}

function Get-FailureArtifacts {
    param([string]$CaptureDirectory, [object]$Definition)

    # Runner failure writers use either the capture root or its direct evidence
    # directory. Enumerate only those two shallow locations, never PNG trees.
    $locations = @(Get-FailureArtifactLocations $CaptureDirectory)
    $matches = New-Object System.Collections.Generic.List[object]
    foreach ($location in $locations) {
        if (-not (Test-Path -LiteralPath $location -PathType Container)) {
            continue
        }
        foreach ($file in @(Get-ChildItem -LiteralPath $location -File `
                -ErrorAction Stop)) {
            $knownFailure = $false
            foreach ($contractName in $Definition.failureFiles) {
                if (Test-FailureArtifactName $file.Name $contractName) {
                    $knownFailure = $true
                    break
                }
            }
            if ($knownFailure -or
                $file.Name -match '(?i)(capture[_-]?(failed|failure)|terminal[_-]?fault)') {
                $matches.Add($file)
            }
        }
    }
    $matches.ToArray()
}

function Assert-ManifestEngineProvenance {
    param([object]$Manifest, [string]$Label)

    $checks = [ordered]@{
        unityVersion = $script:ExpectedUnityVersion
        unityVersionWithRevision = $script:ExpectedUnityVersionWithRevision
        recorderPackageVersion = $script:ExpectedRecorderVersion
        urpPackageVersion = $script:ExpectedUrpVersion
        activeRenderPipelineAssetPath = $script:ExpectedRenderPipelineAsset
    }
    foreach ($entry in $checks.GetEnumerator()) {
        $actual = [string](Get-PropertyValue $Manifest $entry.Key '')
        if ($actual -ne [string]$entry.Value) {
            throw "$Label engine provenance '$($entry.Key)' mismatch. Expected '$($entry.Value)', got '$actual'."
        }
    }
}

function Assert-ReceiptPins {
    param([object]$Receipt, [string]$ReceiptPath)

    foreach ($name in @(
        'sourceFrameLedger', 'automatedProof', 'contactSheet',
        'filmstripSkeleton', 'humanReviewSkeleton', 'rec709Config',
        'rec709OutputLedger', 'rendererRuntimeWorkload')) {
        Assert-PinnedArtifact (Get-PropertyValue $Receipt $name) "$ReceiptPath::$name"
    }

    $hud = Get-PropertyValue $Receipt 'hudRuntimeWorkload'
    if (-not [string]::IsNullOrWhiteSpace(
        [string](Get-PropertyValue $hud 'path' ''))) {
        Assert-PinnedArtifact $hud "$ReceiptPath::hudRuntimeWorkload"
    }

    foreach ($result in @(Get-PropertyValue $Receipt 'checkResults' @())) {
        $id = [string](Get-PropertyValue $result 'id' '<unnamed>')
        Assert-PinnedArtifact (Get-PropertyValue $result 'artifact') `
            "$ReceiptPath::checkResults/$id"
    }
}

function Get-RangeKey {
    param([object]$Value)

    $shotId = [string](Get-PropertyValue $Value 'shotId' '')
    if ([string]::IsNullOrWhiteSpace($shotId)) {
        $shotId = [string](Get-PropertyValue $Value 'sourceShotId' '')
    }
    $sourceStart = Get-PropertyValue $Value 'sourceStart' $null
    if ($null -eq $sourceStart) {
        $sourceStart = Get-PropertyValue $Value 'sourceRangeStartFrame' -1
    }
    $sourceEnd = Get-PropertyValue $Value 'sourceEnd' $null
    if ($null -eq $sourceEnd) {
        $sourceEnd = Get-PropertyValue $Value 'sourceRangeEndFrame' -1
    }
    $selectStart = Get-PropertyValue $Value 'selectStart' $null
    if ($null -eq $selectStart) {
        $selectStart = Get-PropertyValue $Value 'selectStartFrame' -1
    }
    $selectEnd = Get-PropertyValue $Value 'selectEnd' $null
    if ($null -eq $selectEnd) {
        $selectEnd = Get-PropertyValue $Value 'selectEndFrame' -1
    }

    '{0}|{1}|{2}|{3}|{4}' -f `
        $shotId, [int]$sourceStart, [int]$sourceEnd,
        [int]$selectStart, [int]$selectEnd
}

function Assert-EvidenceReceipts {
    param(
        [string]$CaptureDirectory,
        [object]$Definition,
        [string]$CaptureId
    )

    $receiptFiles = @(Get-ChildItem -LiteralPath $CaptureDirectory -File -Recurse `
        -Filter 'evidence_bundle_receipt.json' -ErrorAction Stop)
    if ($receiptFiles.Count -ne $Definition.evidenceRanges.Count) {
        throw "Expected $($Definition.evidenceRanges.Count) PV60 evidence receipts, got $($receiptFiles.Count)."
    }

    $expected = @{}
    foreach ($range in $Definition.evidenceRanges) {
        $key = Get-RangeKey $range
        if ($expected.ContainsKey($key)) {
            throw "Duplicate expected evidence range: $key"
        }
        $expected[$key] = $true
    }

    foreach ($file in $receiptFiles) {
        $receipt = Read-JsonBounded $file.FullName
        if ((Get-PropertyValue $receipt 'schemaVersion' '') -ne
            $script:EvidenceReceiptSchema) {
            throw "Unexpected evidence receipt schema: $($file.FullName)"
        }
        if ((Get-PropertyValue $receipt 'status' '') -ne
            'physical-evidence-complete-human-review-required') {
            throw "Evidence receipt is not physically complete: $($file.FullName)"
        }
        if ((Get-PropertyValue $receipt 'captureId' '') -ne $CaptureId) {
            throw "Evidence receipt captureId mismatch: $($file.FullName)"
        }
        $key = Get-RangeKey $receipt
        if (-not $expected.ContainsKey($key)) {
            throw "Unexpected evidence range '$key': $($file.FullName)"
        }
        $expected.Remove($key)
        Assert-ReceiptPins $receipt $file.FullName
    }
    if ($expected.Count -ne 0) {
        throw "Missing evidence ranges: $(@($expected.Keys) -join ', ')"
    }
}

function Assert-CaptureOutput {
    param(
        [string]$CaptureDirectory,
        [object]$Definition,
        [string]$ExpectedHead
    )

    $failureArtifacts = @(Get-FailureArtifacts $CaptureDirectory $Definition)
    if ($failureArtifacts.Count -gt 0) {
        throw "Terminal/failure artifact exists: $($failureArtifacts[0].FullName)"
    }

    foreach ($relativePath in $Definition.requiredFiles) {
        $requiredPath = Join-Path $CaptureDirectory `
            ($relativePath.Replace('/', [System.IO.Path]::DirectorySeparatorChar))
        if (-not (Test-Path -LiteralPath $requiredPath -PathType Leaf)) {
            throw "Required capture evidence is missing: $requiredPath"
        }
        if ((Get-Item -LiteralPath $requiredPath).Length -le 0) {
            throw "Required capture evidence is empty: $requiredPath"
        }
    }

    $manifestPath = Join-Path $CaptureDirectory 'capture_manifest.json'
    if (-not (Test-Path -LiteralPath $manifestPath -PathType Leaf)) {
        throw "Terminal capture manifest is missing: $manifestPath"
    }
    $manifest = Read-JsonBounded $manifestPath
    if ((Get-PropertyValue $manifest 'schemaVersion' '') -ne $script:ManifestSchema) {
        throw "Unexpected capture manifest schema: $manifestPath"
    }
    if ((Get-PropertyValue $manifest 'sourceFormat' '') -ne
        $script:ExpectedSourceFormat) {
        throw "Unexpected capture source format: $manifestPath"
    }
    if ([int](Get-PropertyValue $manifest 'width' 0) -ne 2560 -or
        [int](Get-PropertyValue $manifest 'height' 0) -ne 1440 -or
        [int](Get-PropertyValue $manifest 'fps' 0) -ne 60) {
        throw "Capture manifest is not exact QHD60: $manifestPath"
    }
    Assert-ManifestEngineProvenance $manifest $manifestPath
    if ([string](Get-PropertyValue $manifest 'gitCommitSha' '') -ne
        $ExpectedHead.ToLowerInvariant()) {
        throw "Capture manifest Git SHA does not match the pinned HEAD: $manifestPath"
    }
    if ([bool](Get-PropertyValue $manifest 'gitWorktreeDirty' $true)) {
        throw "Capture manifest reports a dirty worktree: $manifestPath"
    }

    $captureId = [string](Get-PropertyValue $manifest 'captureId' '')
    if ($captureId -ne (Split-Path $CaptureDirectory -Leaf)) {
        throw "Capture manifest captureId does not match its directory: $manifestPath"
    }
    Assert-PathEquivalent (Get-PropertyValue $manifest 'outputRoot' '') `
        $OutputRoot 'manifest outputRoot'
    Assert-PathEquivalent (Get-PropertyValue $manifest 'outputDirectory' '') `
        $CaptureDirectory 'manifest outputDirectory'

    $actualShots = @(Get-PropertyValue $manifest 'shots' @())
    if ($actualShots.Count -ne $Definition.shots.Count) {
        throw "Shot count mismatch for $($Definition.id): expected $($Definition.shots.Count), got $($actualShots.Count)."
    }
    $shotMap = @{}
    foreach ($shot in $actualShots) {
        $id = [string](Get-PropertyValue $shot 'id' '')
        if ([string]::IsNullOrWhiteSpace($id) -or $shotMap.ContainsKey($id)) {
            throw "Empty or duplicate shot id in $manifestPath`: '$id'"
        }
        $shotMap[$id] = $shot
    }
    foreach ($expectedShot in $Definition.shots) {
        if (-not $shotMap.ContainsKey($expectedShot.id)) {
            throw "Required shot '$($expectedShot.id)' is absent from $manifestPath"
        }
        $actual = $shotMap[$expectedShot.id]
        if ([int](Get-PropertyValue $actual 'startFrame' -1) -ne
                $expectedShot.startFrame -or
            [int](Get-PropertyValue $actual 'endFrame' -1) -ne
                $expectedShot.endFrame -or
            [int](Get-PropertyValue $actual 'expectedFrameCount' -1) -ne
                $expectedShot.expectedFrameCount) {
            throw "Shot range mismatch for '$($expectedShot.id)' in $manifestPath"
        }
    }

    $tests = @(Get-PropertyValue $manifest 'testResults' @())
    if ($tests.Count -eq 0) {
        throw "Capture manifest has no test results: $manifestPath"
    }
    foreach ($test in $tests) {
        $status = [string](Get-PropertyValue $test 'status' '')
        $name = [string](Get-PropertyValue $test 'name' '<unnamed>')
        if ($status -ne 'passed') {
            throw "Manifest test '$name' is not passed ('$status'): $manifestPath"
        }
        $artifact = [string](Get-PropertyValue $test 'artifactPath' '')
        if (-not [string]::IsNullOrWhiteSpace($artifact) -and
            -not (Test-Path -LiteralPath $artifact)) {
            throw "Manifest test artifact is missing for '$name': $artifact"
        }
    }

    Assert-EvidenceReceipts $CaptureDirectory $Definition $captureId

    [pscustomobject][ordered]@{
        captureId = $captureId
        outputDirectory = [System.IO.Path]::GetFullPath($CaptureDirectory)
        manifestPath = $manifestPath
        manifestSha256 = Get-FileSha256 $manifestPath
        testResultCount = $tests.Count
        evidenceReceiptCount = $Definition.evidenceRanges.Count
    }
}

function Quote-WindowsProcessArgument {
    param([string]$Value)

    if ($null -eq $Value -or $Value.Length -eq 0) {
        return '""'
    }
    if ($Value -notmatch '[\s"]') {
        return $Value
    }
    $builder = New-Object System.Text.StringBuilder
    [void]$builder.Append('"')
    $backslashes = 0
    foreach ($character in $Value.ToCharArray()) {
        if ($character -eq '\') {
            $backslashes++
            continue
        }
        if ($character -eq '"') {
            [void]$builder.Append(('\' * ($backslashes * 2 + 1)))
            [void]$builder.Append('"')
            $backslashes = 0
            continue
        }
        if ($backslashes -gt 0) {
            [void]$builder.Append(('\' * $backslashes))
            $backslashes = 0
        }
        [void]$builder.Append($character)
    }
    if ($backslashes -gt 0) {
        [void]$builder.Append(('\' * ($backslashes * 2)))
    }
    [void]$builder.Append('"')
    $builder.ToString()
}

function New-UniqueLogPath {
    param([object]$Run, [string]$Head)

    $stamp = [DateTime]::UtcNow.ToString('yyyyMMddTHHmmssfffZ')
    for ($revision = 0; $revision -lt 1000; $revision++) {
        $suffix = if ($revision -eq 0) { '' } else { "-r$('{0:D3}' -f $revision)" }
        $name = 'PV60-{0}-{1}-{2}{3}.log' -f $Run.runId, $stamp,
            $Head.Substring(0, 12), $suffix
        $candidate = Join-Path $RunRoot $name
        if (-not (Test-Path -LiteralPath $candidate)) {
            return $candidate
        }
    }
    throw "Could not reserve a unique Unity log name for $($Run.runId)."
}

function Get-UnityArguments {
    param([object]$Run, [string]$LogPath)

    $arguments = New-Object System.Collections.Generic.List[string]
    $arguments.Add('-projectPath')
    $arguments.Add([System.IO.Path]::GetFullPath($ProjectRoot))
    $arguments.Add('-executeMethod')
    $arguments.Add($Run.definition.method)
    if (-not $Run.definition.headful) {
        $arguments.Add('-batchmode')
    }
    $arguments.Add('-noaudio')
    $arguments.Add('-logFile')
    $arguments.Add($LogPath)
    $arguments.Add('-pv60ApprovedEvidence')
    foreach ($extra in $Run.definition.extraArguments) {
        $arguments.Add($extra)
    }
    $arguments.ToArray()
}

function Write-JsonAtomic {
    param([string]$Path, [object]$Value)

    $directory = Split-Path $Path -Parent
    if (-not (Test-Path -LiteralPath $directory -PathType Container)) {
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
    }
    $temporary = "$Path.tmp.$([Guid]::NewGuid().ToString('N'))"
    try {
        $json = $Value | ConvertTo-Json -Depth 20
        [System.IO.File]::WriteAllText($temporary, $json,
            (New-Object System.Text.UTF8Encoding($false)))
        Move-Item -LiteralPath $temporary -Destination $Path -Force
    }
    finally {
        if (Test-Path -LiteralPath $temporary) {
            Remove-Item -LiteralPath $temporary -Force
        }
    }
}

function Write-Event {
    param([string]$Type, [object]$Data)

    if ([string]::IsNullOrWhiteSpace($script:EventJournalPath)) {
        return
    }
    $identity = [string](Get-PropertyValue $script:State 'runIdentity' '')
    $eventCount = [int](Get-PropertyValue $script:State 'eventCount' -1)
    if ([string]::IsNullOrWhiteSpace($identity) -or $eventCount -lt 0) {
        throw 'Cannot write an event without sealed runIdentity/eventCount state.'
    }
    $journalExists = Test-Path -LiteralPath $script:EventJournalPath `
        -PathType Leaf
    if ($eventCount -eq 0) {
        if ($Type -ne 'orchestrator-created' -or $journalExists) {
            throw 'The first event must exclusively create a fresh orchestrator-created journal.'
        }
    }
    elseif (-not $journalExists) {
        throw 'Existing run event journal disappeared; refusing a new append chain.'
    }
    $event = [pscustomobject][ordered]@{
        schema = $script:EventSchema
        runIdentity = $identity
        sequence = $eventCount + 1
        atUtc = [DateTime]::UtcNow.ToString('O')
        type = $Type
        data = $Data
    }
    ($event | ConvertTo-Json -Depth 12 -Compress) |
        Add-Content -LiteralPath $script:EventJournalPath -Encoding UTF8
    $script:State.eventCount = $eventCount + 1
    Save-State
}

function Save-State {
    if ($null -eq $script:State) {
        return
    }
    if (Test-Path -LiteralPath $StatePath -PathType Leaf) {
        $existingState = Read-JsonBounded $StatePath
        if ((Get-PropertyValue $existingState 'runIdentity' '') -ne
            (Get-PropertyValue $script:State 'runIdentity' '')) {
            throw "State runIdentity contamination; refusing overwrite: $StatePath"
        }
        $existingEventCount = [int](Get-PropertyValue $existingState `
            'eventCount' -1)
        $nextEventCount = [int](Get-PropertyValue $script:State `
            'eventCount' -2)
        if ($existingEventCount -lt 0 -or
            $nextEventCount -lt $existingEventCount) {
            throw "State eventCount rollback/contamination; refusing overwrite: $StatePath"
        }
    }
    $script:State.updatedAtUtc = [DateTime]::UtcNow.ToString('O')
    Write-JsonAtomic $StatePath $script:State
}

function Write-Report {
    param([string]$Status, [string]$Failure = '')

    if ($null -eq $script:State -or
        [string]::IsNullOrWhiteSpace($script:ReportPath)) {
        return
    }
    if (Test-Path -LiteralPath $script:ReportPath -PathType Leaf) {
        $existingReport = Read-JsonBounded $script:ReportPath
        if ((Get-PropertyValue $existingReport 'runIdentity' '') -ne
            (Get-PropertyValue $script:State 'runIdentity' '')) {
            throw "Report runIdentity contamination; refusing overwrite: $script:ReportPath"
        }
        $existingReportEventCount = [int](Get-PropertyValue $existingReport `
            'eventCount' -1)
        $nextReportEventCount = [int](Get-PropertyValue $script:State `
            'eventCount' -2)
        if ($existingReportEventCount -lt 0 -or
            $nextReportEventCount -lt $existingReportEventCount) {
            throw "Report eventCount rollback/contamination; refusing overwrite: $script:ReportPath"
        }
    }
    $completed = @($script:State.runs | Where-Object { $_.status -eq 'completed' })
    $report = [pscustomobject][ordered]@{
        schema = $script:ReportSchema
        runIdentity = $script:State.runIdentity
        status = $Status
        failure = $Failure
        generatedAtUtc = [DateTime]::UtcNow.ToString('O')
        pinnedHead = $script:State.pinnedHead
        sequenceSha256 = $script:State.sequenceSha256
        eventCount = [int]$script:State.eventCount
        orchestratorPath = $PSCommandPath
        orchestratorSha256 = Get-FileSha256 $PSCommandPath
        completedRunCount = $completed.Count
        expectedRunCount = $script:State.runs.Count
        outputs = @($completed | ForEach-Object {
            [pscustomobject][ordered]@{
                runId = $_.runId
                captureId = $_.captureId
                outputDirectory = $_.outputDirectory
                manifestSha256 = $_.manifestSha256
                logPath = $_.logPath
            }
        })
        lastResourceSnapshot = $script:State.lastResourceSnapshot
        runRoot = $RunRoot
        statePath = $StatePath
        eventJournalPath = $script:EventJournalPath
        reportPath = $script:ReportPath
    }
    Write-JsonAtomic $script:ReportPath $report
}

function New-OrchestratorState {
    param([object[]]$Schedule, [string]$Head, [string]$SequenceSha)

    [pscustomobject][ordered]@{
        schema = $script:StateSchema
        runIdentity = [Guid]::NewGuid().ToString('D')
        eventCount = 0
        status = 'running'
        pinnedHead = $Head
        sequenceSha256 = $SequenceSha
        projectRoot = [System.IO.Path]::GetFullPath($ProjectRoot)
        outputRoot = [System.IO.Path]::GetFullPath($OutputRoot)
        runRoot = [System.IO.Path]::GetFullPath($RunRoot)
        statePath = [System.IO.Path]::GetFullPath($StatePath)
        eventJournalPath = [System.IO.Path]::GetFullPath(
            $script:EventJournalPath)
        reportPath = [System.IO.Path]::GetFullPath($script:ReportPath)
        createdAtUtc = [DateTime]::UtcNow.ToString('O')
        updatedAtUtc = [DateTime]::UtcNow.ToString('O')
        failure = ''
        lastResourceSnapshot = $null
        runs = @($Schedule | ForEach-Object {
            [pscustomobject][ordered]@{
                sequence = $_.sequence
                runId = $_.runId
                familyId = $_.familyId
                takeOrdinal = $_.takeOrdinal
                method = $_.definition.method
                headful = $_.definition.headful
                expectedEvidenceReceiptCount =
                    $_.definition.evidenceRanges.Count
                status = 'pending'
                startedAtUtc = ''
                completedAtUtc = ''
                unityProcessId = 0
                unityExitCode = $null
                recoveredFromTerminalManifest = $false
                unityArguments = @()
                logPath = ''
                beforeCaptureIds = @()
                failureOutputCandidates = @()
                captureId = ''
                outputDirectory = ''
                manifestPath = ''
                manifestSha256 = ''
                testResultCount = 0
                evidenceReceiptCount = 0
                failure = ''
            }
        })
    }
}

function Assert-StateCompatible {
    param(
        [object]$Loaded,
        [string]$SequenceSha,
        [object[]]$Schedule
    )

    if ((Get-PropertyValue $Loaded 'schema' '') -ne $script:StateSchema) {
        throw "Unsupported orchestrator state schema: $StatePath"
    }
    if ((Get-PropertyValue $Loaded 'sequenceSha256' '') -ne $SequenceSha) {
        throw "Run sequence contract changed since the saved state: $StatePath"
    }
    Assert-PathEquivalent (Get-PropertyValue $Loaded 'projectRoot' '') `
        $ProjectRoot 'state projectRoot'
    Assert-PathEquivalent (Get-PropertyValue $Loaded 'outputRoot' '') `
        $OutputRoot 'state outputRoot'
    Assert-PathEquivalent (Get-PropertyValue $Loaded 'runRoot' '') `
        $RunRoot 'state runRoot'
    Assert-PathEquivalent (Get-PropertyValue $Loaded 'statePath' '') `
        $StatePath 'state statePath'
    Assert-PathEquivalent (Get-PropertyValue $Loaded 'eventJournalPath' '') `
        $script:EventJournalPath 'state eventJournalPath'
    Assert-PathEquivalent (Get-PropertyValue $Loaded 'reportPath' '') `
        $script:ReportPath 'state reportPath'
    $runIdentity = [string](Get-PropertyValue $Loaded 'runIdentity' '')
    $parsedIdentity = [Guid]::Empty
    if (-not [Guid]::TryParse($runIdentity, [ref]$parsedIdentity) -or
        $parsedIdentity -eq [Guid]::Empty) {
        throw "Saved orchestrator runIdentity is not a non-empty GUID: $StatePath"
    }
    $loadedRuns = @(Get-PropertyValue $Loaded 'runs' @())
    if ($loadedRuns.Count -ne 19 -or $Schedule.Count -ne 19) {
        throw "Saved orchestrator state does not contain exactly 19 runs: $StatePath"
    }

    $seenLogs = @{}
    for ($index = 0; $index -lt 19; $index++) {
        $saved = $loadedRuns[$index]
        $expected = $Schedule[$index]
        $savedHeadful = Get-PropertyValue $saved 'headful' $null
        if ($null -eq $savedHeadful -or
            [int](Get-PropertyValue $saved 'sequence' -1) -ne
                [int]$expected.sequence -or
            [string](Get-PropertyValue $saved 'runId' '') -cne
                [string]$expected.runId -or
            [string](Get-PropertyValue $saved 'familyId' '') -cne
                [string]$expected.familyId -or
            [int](Get-PropertyValue $saved 'takeOrdinal' -1) -ne
                [int]$expected.takeOrdinal -or
            [string](Get-PropertyValue $saved 'method' '') -cne
                [string]$expected.definition.method -or
            [bool]$savedHeadful -ne [bool]$expected.definition.headful -or
            [int](Get-PropertyValue $saved `
                'expectedEvidenceReceiptCount' -1) -ne
                [int]$expected.definition.evidenceRanges.Count) {
            throw "Saved run row is not bound to the sealed schedule at index $index ($($expected.runId))."
        }

        $savedStatus = [string](Get-PropertyValue $saved 'status' '')
        if ($savedStatus -notin @('pending','running','completed','failed')) {
            throw "Saved run has an unsupported status at index $index`: '$savedStatus'."
        }
        $savedLog = [string](Get-PropertyValue $saved 'logPath' '')
        $savedArguments = @(Get-PropertyValue $saved 'unityArguments' @())
        if ([string]::IsNullOrWhiteSpace($savedLog)) {
            if ($savedArguments.Count -ne 0 -or $savedStatus -ne 'pending') {
                throw "Saved run log/argument metadata is incomplete: $($expected.runId)"
            }
            continue
        }
        if ($savedStatus -eq 'pending') {
            throw "Pending run unexpectedly owns a fixed log path: $($expected.runId)"
        }
        Assert-PathEquivalent (Split-Path $savedLog -Parent) $RunRoot `
            "saved log parent/$($expected.runId)"
        $logKey = [System.IO.Path]::GetFullPath($savedLog).ToLowerInvariant()
        if ($seenLogs.ContainsKey($logKey)) {
            throw "Saved run log path is reused: $savedLog"
        }
        $seenLogs[$logKey] = $true
        $expectedArguments = @(Get-UnityArguments $expected $savedLog)
        $separator = [string][char]31
        if ($savedArguments.Count -ne $expectedArguments.Count -or
            (($savedArguments -join $separator) -cne
                ($expectedArguments -join $separator))) {
            throw "Saved Unity argument vector is not sealed: $($expected.runId)"
        }
    }
}

function Assert-CanonicalOrchestrationPaths {
    $canonicalStatePath = Join-Path $RunRoot 'capture_state.json'
    Assert-PathEquivalent $StatePath $canonicalStatePath `
        'StatePath must be the canonical RunRoot state file'
    Assert-PathEquivalent $script:EventJournalPath `
        (Join-Path $RunRoot 'capture_events.ndjson') `
        'event journal path'
    Assert-PathEquivalent $script:ReportPath `
        (Join-Path $RunRoot 'capture_report.json') `
        'report path'
}

function Assert-FreshRunRoot {
    if (Test-Path -LiteralPath $RunRoot -PathType Leaf) {
        throw "RunRoot is a file, not a fresh directory: $RunRoot"
    }
    if (Test-Path -LiteralPath $RunRoot -PathType Container) {
        $entries = @(Get-ChildItem -LiteralPath $RunRoot -Force `
            -ErrorAction Stop)
        if ($entries.Count -gt 0) {
            $preview = @($entries | Select-Object -First 10 -ExpandProperty Name) `
                -join ', '
            throw "Fresh run requires an empty RunRoot; refusing journal/report/state contamination: $RunRoot => $preview"
        }
    }
}

function Assert-InvocationStorageDisposition {
    param([bool]$ResumeMode)

    Assert-CanonicalOrchestrationPaths
    if ($ResumeMode) {
        if (-not (Test-Path -LiteralPath $StatePath -PathType Leaf)) {
            throw "-Resume requires the canonical existing state file: $StatePath"
        }
    }
    else {
        Assert-FreshRunRoot
    }
}

function Assert-ResumeJournalIdentity {
    param([string]$ExpectedIdentity)

    if (-not (Test-Path -LiteralPath $script:EventJournalPath -PathType Leaf)) {
        throw "Resume event journal is missing: $script:EventJournalPath"
    }
    $item = Get-Item -LiteralPath $script:EventJournalPath
    if ($item.Length -le 0 -or $item.Length -gt 67108864) {
        throw "Resume event journal size is outside 1..67108864 bytes: $script:EventJournalPath"
    }
    $reader = New-Object System.IO.StreamReader(
        $script:EventJournalPath,
        [System.Text.Encoding]::UTF8,
        $true,
        65536)
    $count = 0
    try {
        while (-not $reader.EndOfStream) {
            $line = $reader.ReadLine()
            if ([string]::IsNullOrWhiteSpace($line)) {
                throw "Resume event journal contains a blank line at $($count + 1)."
            }
            if ($line.Length -gt 1048576) {
                throw "Resume event line exceeds 1 MiB at $($count + 1)."
            }
            try {
                $event = $line | ConvertFrom-Json
            }
            catch {
                throw "Resume event journal JSON is invalid at line $($count + 1): $($_.Exception.Message)"
            }
            if ((Get-PropertyValue $event 'schema' '') -ne $script:EventSchema -or
                (Get-PropertyValue $event 'runIdentity' '') -ne
                    $ExpectedIdentity) {
                throw "Resume event journal identity/schema contamination at line $($count + 1)."
            }
            if ([int](Get-PropertyValue $event 'sequence' -1) -ne
                ($count + 1)) {
                throw "Resume event journal sequence contamination at line $($count + 1)."
            }
            if ($count -eq 0 -and
                (Get-PropertyValue $event 'type' '') -ne
                    'orchestrator-created') {
                throw 'Resume event journal does not begin with orchestrator-created.'
            }
            $count++
        }
    }
    finally {
        $reader.Dispose()
    }
    if ($count -le 0) {
        throw "Resume event journal is empty: $script:EventJournalPath"
    }
    $count
}

function Assert-ResumeArtifactsIdentity {
    param([object]$Loaded)

    $expectedIdentity = [string](Get-PropertyValue $Loaded 'runIdentity' '')
    $journalCount = Assert-ResumeJournalIdentity $expectedIdentity
    if ([int](Get-PropertyValue $Loaded 'eventCount' -1) -ne
        [int]$journalCount) {
        throw "Resume state/journal eventCount mismatch: state=$((Get-PropertyValue $Loaded 'eventCount' -1)), journal=$journalCount"
    }
    if (-not (Test-Path -LiteralPath $script:ReportPath -PathType Leaf)) {
        throw "Resume report is missing: $script:ReportPath"
    }
    $report = Read-JsonBounded $script:ReportPath
    $reportEventCount = [int](Get-PropertyValue $report 'eventCount' -1)
    $stateEventCount = [int](Get-PropertyValue $Loaded 'eventCount' -2)
    if ((Get-PropertyValue $report 'schema' '') -ne $script:ReportSchema -or
        (Get-PropertyValue $report 'runIdentity' '') -ne $expectedIdentity -or
        $reportEventCount -lt 1 -or
        $reportEventCount -gt $stateEventCount -or
        (Get-PropertyValue $report 'pinnedHead' '') -ne
            (Get-PropertyValue $Loaded 'pinnedHead' '') -or
        (Get-PropertyValue $report 'sequenceSha256' '') -ne
            (Get-PropertyValue $Loaded 'sequenceSha256' '')) {
        throw "Resume report identity/schema/HEAD/sequence contamination: $script:ReportPath"
    }
    Assert-PathEquivalent (Get-PropertyValue $report 'runRoot' '') `
        $RunRoot 'resume report runRoot'
    Assert-PathEquivalent (Get-PropertyValue $report 'statePath' '') `
        $StatePath 'resume report statePath'
    Assert-PathEquivalent (Get-PropertyValue $report 'eventJournalPath' '') `
        $script:EventJournalPath 'resume report eventJournalPath'
    Assert-PathEquivalent (Get-PropertyValue $report 'reportPath' '') `
        $script:ReportPath 'resume report reportPath'
}

function Recover-InterruptedRun {
    param([object]$SavedRun, [object]$ScheduledRun, [string]$Head)

    $newDirectories = @(Get-NewCaptureDirectories `
        @(Get-PropertyValue $SavedRun 'beforeCaptureIds' @()))
    $SavedRun.failureOutputCandidates = $newDirectories
    if ($newDirectories.Count -ne 1) {
        throw "Interrupted run $($SavedRun.runId) has $($newDirectories.Count) candidate output directories; automatic retry is forbidden. Preserved: $($newDirectories -join ', ')"
    }
    $verified = Assert-CaptureOutput $newDirectories[0] `
        $ScheduledRun.definition $Head
    $SavedRun.status = 'completed'
    $SavedRun.completedAtUtc = [DateTime]::UtcNow.ToString('O')
    $SavedRun.captureId = $verified.captureId
    $SavedRun.outputDirectory = $verified.outputDirectory
    $SavedRun.manifestPath = $verified.manifestPath
    $SavedRun.manifestSha256 = $verified.manifestSha256
    $SavedRun.testResultCount = $verified.testResultCount
    $SavedRun.evidenceReceiptCount = $verified.evidenceReceiptCount
    $SavedRun.recoveredFromTerminalManifest = $true
    $SavedRun.failure = ''
    Write-Event 'run-recovered' $SavedRun
    Save-State
}

function Invoke-CaptureRun {
    param([object]$Run, [object]$SavedRun, [string]$Head)

    Assert-GitPin $Head "before $($Run.runId)" | Out-Null
    Assert-PreRunProcessExclusion
    $resources = Get-ResourceSnapshot
    Assert-ResourceFloor $resources
    $script:State.lastResourceSnapshot = $resources

    $logPath = New-UniqueLogPath $Run $Head
    $beforeIds = @(Get-CaptureIds)
    $arguments = @(Get-UnityArguments $Run $logPath)
    $SavedRun.status = 'running'
    $SavedRun.startedAtUtc = [DateTime]::UtcNow.ToString('O')
    $SavedRun.logPath = $logPath
    $SavedRun.unityArguments = $arguments
    $SavedRun.beforeCaptureIds = $beforeIds
    $SavedRun.failureOutputCandidates = @()
    $SavedRun.recoveredFromTerminalManifest = $false
    Save-State
    Write-Event 'run-starting' ([pscustomobject]@{
        runId = $Run.runId
        method = $Run.definition.method
        headful = $Run.definition.headful
        arguments = $arguments
        logPath = $logPath
        resourceSnapshot = $resources
    })

    $quotedArguments = @($arguments | ForEach-Object {
        Quote-WindowsProcessArgument $_
    }) -join ' '
    $owned = $null
    try {
        $owned = Start-Process -FilePath $UnityExe -ArgumentList $quotedArguments `
            -WorkingDirectory $ProjectRoot -PassThru
        $script:OwnedUnityProcess = $owned
        $SavedRun.unityProcessId = $owned.Id
        Save-State
        Write-Event 'unity-started' ([pscustomobject]@{
            runId = $Run.runId
            processId = $owned.Id
        })

        $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
        $nextProgress = [TimeSpan]::FromSeconds(60)
        while ($true) {
            $owned.Refresh()
            if ($owned.HasExited) {
                break
            }
            if ($stopwatch.Elapsed.TotalMinutes -gt $RunTimeoutMinutes) {
                throw "Unity capture exceeded $RunTimeoutMinutes minutes."
            }
            Assert-InRunProcessExclusion $owned.Id
            $inRunResources = Get-ResourceSnapshot
            Assert-ResourceFloor $inRunResources -Critical
            $script:State.lastResourceSnapshot = $inRunResources
            if ($stopwatch.Elapsed -ge $nextProgress) {
                Write-Host ('[{0}] {1:N1} min; RAM {2} GiB; commit {3} GiB; C {4} GiB; D {5} GiB' -f `
                    $Run.runId, $stopwatch.Elapsed.TotalMinutes,
                    $inRunResources.availableRamGiB,
                    $inRunResources.commitHeadroomGiB,
                    $inRunResources.cFreeGiB,
                    $inRunResources.dFreeGiB)
                $nextProgress = $nextProgress.Add([TimeSpan]::FromSeconds(60))
            }
            Start-Sleep -Seconds $PollSeconds
        }
        $stopwatch.Stop()
        $SavedRun.unityExitCode = $owned.ExitCode
        if ($owned.ExitCode -ne 0) {
            throw "Unity exited with code $($owned.ExitCode). See $logPath"
        }

        $processDrainDeadline = [DateTime]::UtcNow.AddSeconds(60)
        while ((Get-RelevantProcesses).unity.Count -gt 0) {
            if ([DateTime]::UtcNow -ge $processDrainDeadline) {
                throw 'Unity.exe remained alive after the owned process exited.'
            }
            Start-Sleep -Seconds 2
        }

        Assert-GitPin $Head "after $($Run.runId)" | Out-Null
        Assert-PreRunProcessExclusion
        $postResources = Get-ResourceSnapshot
        Assert-ResourceFloor $postResources
        $script:State.lastResourceSnapshot = $postResources

        $newDirectories = @(Get-NewCaptureDirectories $beforeIds)
        $SavedRun.failureOutputCandidates = $newDirectories
        if ($newDirectories.Count -ne 1) {
            throw "Expected exactly one new output for $($Run.runId), got $($newDirectories.Count): $($newDirectories -join ', ')"
        }
        $verified = Assert-CaptureOutput $newDirectories[0] `
            $Run.definition $Head

        $SavedRun.status = 'completed'
        $SavedRun.completedAtUtc = [DateTime]::UtcNow.ToString('O')
        $SavedRun.captureId = $verified.captureId
        $SavedRun.outputDirectory = $verified.outputDirectory
        $SavedRun.manifestPath = $verified.manifestPath
        $SavedRun.manifestSha256 = $verified.manifestSha256
        $SavedRun.testResultCount = $verified.testResultCount
        $SavedRun.evidenceReceiptCount = $verified.evidenceReceiptCount
        $SavedRun.failure = ''
        Save-State
        Write-Event 'run-completed' $SavedRun
        Write-Report 'running'
    }
    catch {
        if ($null -ne $owned) {
            Stop-OwnedUnityProcess $owned $_.Exception.Message
        }
        $SavedRun.failureOutputCandidates = @(Get-NewCaptureDirectories $beforeIds)
        $SavedRun.status = 'failed'
        $SavedRun.failure = $_.Exception.ToString()
        Save-State
        Write-Event 'run-failed' $SavedRun
        throw
    }
    finally {
        $script:OwnedUnityProcess = $null
    }
}

function Assert-CompletedStateMetadata {
    param(
        [object]$StateValue,
        [object[]]$Schedule,
        [string]$Head,
        [switch]$RequireFiles
    )

    Assert-StateCompatible $StateValue $script:GoldenScheduleSha256 `
        $Schedule
    if ((Get-PropertyValue $StateValue 'pinnedHead' '') -ne $Head -or
        (Get-PropertyValue $StateValue 'sequenceSha256' '') -ne
            $script:GoldenScheduleSha256) {
        throw 'Final state HEAD or golden sequence metadata mismatch.'
    }
    $runs = @(Get-PropertyValue $StateValue 'runs' @())
    if ($runs.Count -ne 19 -or $Schedule.Count -ne 19) {
        throw 'Final state and schedule must each contain exactly 19 runs.'
    }

    $captureIds = @{}
    $outputDirectories = @{}
    $manifestPaths = @{}
    $logPaths = @{}
    $receiptTotal = 0
    for ($index = 0; $index -lt 19; $index++) {
        $saved = $runs[$index]
        $expected = $Schedule[$index]
        if ([int](Get-PropertyValue $saved 'sequence' -1) -ne
                [int]$expected.sequence -or
            (Get-PropertyValue $saved 'runId' '') -ne $expected.runId -or
            (Get-PropertyValue $saved 'familyId' '') -ne
                $expected.familyId -or
            [int](Get-PropertyValue $saved 'takeOrdinal' -1) -ne
                [int]$expected.takeOrdinal -or
            (Get-PropertyValue $saved 'method' '') -ne
                $expected.definition.method -or
            [bool](Get-PropertyValue $saved 'headful' (-not
                $expected.definition.headful)) -ne
                [bool]$expected.definition.headful) {
            throw "Final state run contract mismatch at index $index."
        }
        if ((Get-PropertyValue $saved 'status' '') -ne 'completed' -or
            -not [string]::IsNullOrWhiteSpace(
                [string](Get-PropertyValue $saved 'failure' ''))) {
            throw "Final state run is not cleanly completed: $($expected.runId)"
        }

        $expectedReceiptCount = $expected.definition.evidenceRanges.Count
        if ([int](Get-PropertyValue $saved 'expectedEvidenceReceiptCount' -1) -ne
                $expectedReceiptCount -or
            [int](Get-PropertyValue $saved 'evidenceReceiptCount' -1) -ne
                $expectedReceiptCount) {
            throw "Final evidence receipt count mismatch: $($expected.runId)"
        }
        $receiptTotal += $expectedReceiptCount

        $captureId = [string](Get-PropertyValue $saved 'captureId' '')
        $output = [string](Get-PropertyValue $saved 'outputDirectory' '')
        $manifest = [string](Get-PropertyValue $saved 'manifestPath' '')
        $log = [string](Get-PropertyValue $saved 'logPath' '')
        if ([string]::IsNullOrWhiteSpace($captureId) -or
            [string]::IsNullOrWhiteSpace($output) -or
            [string]::IsNullOrWhiteSpace($manifest) -or
            [string]::IsNullOrWhiteSpace($log)) {
            throw "Final output metadata is incomplete: $($expected.runId)"
        }
        if ($captureId -ne (Split-Path $output -Leaf)) {
            throw "Final captureId/output leaf mismatch: $($expected.runId)"
        }
        Assert-PathEquivalent (Split-Path $output -Parent) $OutputRoot `
            "final output parent/$($expected.runId)"
        Assert-PathEquivalent $manifest (Join-Path $output `
            'capture_manifest.json') "final manifest/$($expected.runId)"
        Assert-PathEquivalent (Split-Path $log -Parent) $RunRoot `
            "final log parent/$($expected.runId)"
        $actualArguments = @(Get-PropertyValue $saved 'unityArguments' @())
        $expectedArguments = @(Get-UnityArguments $expected $log)
        $argumentSeparator = [string][char]31
        if ($actualArguments.Count -ne $expectedArguments.Count -or
            (($actualArguments -join $argumentSeparator) -cne
                ($expectedArguments -join $argumentSeparator))) {
            throw "Final Unity argument vector mismatch: $($expected.runId)"
        }

        $uniqueEntries = @(
            [pscustomobject]@{ map=$captureIds; key=$captureId;
                label='captureId' },
            [pscustomobject]@{ map=$outputDirectories;
                key=[System.IO.Path]::GetFullPath($output);
                label='outputDirectory' },
            [pscustomobject]@{ map=$manifestPaths;
                key=[System.IO.Path]::GetFullPath($manifest);
                label='manifestPath' },
            [pscustomobject]@{ map=$logPaths;
                key=[System.IO.Path]::GetFullPath($log); label='logPath' }
        )
        foreach ($entry in $uniqueEntries) {
            $map = $entry.map
            $key = ([string]$entry.key).ToLowerInvariant()
            if ($map.ContainsKey($key)) {
                throw "Final $($entry.label) is reused: $($entry.key)"
            }
            $map[$key] = $true
        }

        $start = [DateTimeOffset]::MinValue
        $complete = [DateTimeOffset]::MinValue
        if (-not [DateTimeOffset]::TryParse(
                [string](Get-PropertyValue $saved 'startedAtUtc' ''),
                [ref]$start) -or
            -not [DateTimeOffset]::TryParse(
                [string](Get-PropertyValue $saved 'completedAtUtc' ''),
                [ref]$complete) -or $complete -lt $start) {
            throw "Final state timestamps are invalid: $($expected.runId)"
        }
        $recovered = [bool](Get-PropertyValue $saved `
            'recoveredFromTerminalManifest' $false)
        if (-not $recovered -and
            [int](Get-PropertyValue $saved 'unityExitCode' -1) -ne 0) {
            throw "Final Unity exit metadata is not zero: $($expected.runId)"
        }

        if ($RequireFiles) {
            foreach ($file in @($manifest, $log)) {
                if (-not (Test-Path -LiteralPath $file -PathType Leaf) -or
                    (Get-Item -LiteralPath $file).Length -le 0) {
                    throw "Final state file is missing or empty: $file"
                }
            }
        }
    }
    if ($receiptTotal -ne 37) {
        throw "Final schedule must bind exactly 37 evidence receipts, got $receiptTotal."
    }
}

function Assert-AllCompletedOutputs {
    param(
        [object[]]$Schedule,
        [string]$Head,
        [scriptblock]$OutputVerifier = $null,
        [switch]$SelfTestSkipFilePresence
    )

    if ($SelfTestSkipFilePresence) {
        Assert-CompletedStateMetadata $script:State $Schedule $Head
    }
    else {
        Assert-CompletedStateMetadata $script:State $Schedule $Head `
            -RequireFiles
    }
    for ($index = 0; $index -lt 19; $index++) {
        $saved = $script:State.runs[$index]
        if ($null -eq $OutputVerifier) {
            $verified = Assert-CaptureOutput $saved.outputDirectory `
                $Schedule[$index].definition $Head
        }
        else {
            $verified = & $OutputVerifier $saved.outputDirectory `
                $Schedule[$index].definition $Head
        }
        Assert-PathEquivalent $saved.manifestPath $verified.manifestPath `
            "final verified manifest/$($saved.runId)"
        if ($saved.captureId -ne $verified.captureId -or
            $saved.manifestSha256 -ne $verified.manifestSha256 -or
            [int]$saved.testResultCount -ne
                [int]$verified.testResultCount -or
            [int]$saved.evidenceReceiptCount -ne
                [int]$verified.evidenceReceiptCount) {
            throw "Final verified output changed from saved state: $($saved.runId)"
        }
    }
}

function Get-GoldenFamilyContracts {
    [ordered]@{
        city = [pscustomobject]@{
            method='DimensionBrawl.Editor.AuditionPV.AuditionPvCityHeroPocketGoldenRunner.RunBatchCapture'
            headful=$true
            shots=@('g01|0|599|600','g02|0|779|780','g03|0|659|660')
            evidence=@('g01|0|539|180|359','g02|60|779|240|599',
                'g03|0|419|180|239','g03|60|659|240|479')
            extra=@()
            runCount=3
        }
        s030 = [pscustomobject]@{
            method='DimensionBrawl.Editor.AuditionPV.AuditionPvCityHitDodgeSummonGoldenRunner.RunBatchCapture'
            headful=$true
            shots=@('s030|0|719|720')
            evidence=@('s030|0|719|180|539')
            extra=@()
            runCount=3
        }
        s050 = [pscustomobject]@{
            method='DimensionBrawl.Editor.AuditionPV.AuditionPvStationPhaseOneBossLowAngleGoldenRunner.RunBatchCapture'
            headful=$true
            shots=@('s050|0|599|600')
            evidence=@('s050|0|599|180|419')
            extra=@('-s050TakeOrdinal=1')
            runCount=1
        }
        g04 = [pscustomobject]@{
            method='DimensionBrawl.Editor.AuditionPV.AuditionPvStationTransitionGoldenCapture.RunBatchCapture'
            headful=$false
            shots=@('g04|0|597|598','g04-clean|0|597|598')
            evidence=@('g04|0|479|180|299','g04|118|597|298|417',
                'g04-clean|118|597|298|417')
            extra=@()
            runCount=3
        }
        g06 = [pscustomobject]@{
            method='DimensionBrawl.Editor.AuditionPV.AuditionPvStationPhase2SummonCounterGoldenRunner.RunBatchCapture'
            headful=$true
            shots=@('g06|0|719|720')
            evidence=@('g06|0|659|180|479','g06|60|719|240|539')
            extra=@()
            runCount=3
        }
        g07 = [pscustomobject]@{
            method='DimensionBrawl.Editor.AuditionPV.AuditionPvStationPhase2PatternRelayGoldenRunner.RunBatchCapture'
            headful=$true
            shots=@('g07|0|779|780')
            evidence=@('g07|0|779|180|599')
            extra=@()
            runCount=3
        }
        g08 = [pscustomobject]@{
            method='DimensionBrawl.Editor.AuditionPV.AuditionPvStationBossDeathAftermathGoldenRunner.RunBatchCapture'
            headful=$true
            shots=@('g08|0|719|720')
            evidence=@('g08|60|719|240|539')
            extra=@()
            runCount=3
        }
    }
}

function Get-ShotContractKey {
    param([object]$Shot)

    '{0}|{1}|{2}|{3}' -f $Shot.id, [int]$Shot.startFrame,
        [int]$Shot.endFrame, [int]$Shot.expectedFrameCount
}

function Assert-DefinitionContracts {
    param([System.Collections.IDictionary]$Families, [object[]]$Schedule)

    $golden = Get-GoldenFamilyContracts
    if ($Schedule.Count -ne 19) {
        throw "Schedule must contain exactly 19 runs, got $($Schedule.Count)."
    }
    $expectedRunIds = @(
        '01-g04-take1','02-s050-take1','03-g08-take1',
        '04-g07-take1','05-g06-take1','06-s030-take1','07-city-take1',
        '08-g04-take2','09-g08-take2','10-g07-take2',
        '11-g06-take2','12-s030-take2','13-city-take2',
        '14-g04-take3','15-g08-take3','16-g07-take3',
        '17-g06-take3','18-s030-take3','19-city-take3'
    )
    if ((@($Schedule | ForEach-Object runId) -join ',') -cne
        ($expectedRunIds -join ',')) {
        throw 'Schedule does not match the independent exact 19-run golden order.'
    }

    $counts = @{}
    $receiptChecks = 0
    foreach ($run in $Schedule) {
        if (-not $counts.ContainsKey($run.familyId)) { $counts[$run.familyId] = 0 }
        $counts[$run.familyId]++
        $receiptChecks += $run.definition.evidenceRanges.Count
        if (-not $golden.Contains($run.familyId)) {
            throw "Unknown family in schedule: $($run.familyId)"
        }
        $expectedFamily = $golden[$run.familyId]
        if ($run.definition.method -cne $expectedFamily.method -or
            [bool]$run.definition.headful -ne [bool]$expectedFamily.headful) {
            throw "Method/headful contract drift: $($run.familyId)"
        }
        $probeLog = "C:\tmp\probe $($run.runId).log"
        $args = @(Get-UnityArguments $run $probeLog)
        $expectedArgs = @('-projectPath', $script:ExpectedProjectRoot,
            '-executeMethod', $expectedFamily.method)
        if (-not $expectedFamily.headful) {
            $expectedArgs += '-batchmode'
        }
        $expectedArgs += @('-noaudio','-logFile',$probeLog,
            '-pv60ApprovedEvidence')
        $expectedArgs += @($expectedFamily.extra)
        $separator = [string][char]31
        if ($args.Count -ne $expectedArgs.Count -or
            (($args -join $separator) -cne ($expectedArgs -join $separator))) {
            throw "Exact Unity argument contract drift: $($run.runId)"
        }

        $actualShots = @($run.definition.shots | ForEach-Object {
            Get-ShotContractKey $_
        })
        if (($actualShots -join ',') -cne
            (@($expectedFamily.shots) -join ',')) {
            throw "Exact shot contract drift: $($run.familyId)"
        }
        $actualEvidence = @($run.definition.evidenceRanges |
            ForEach-Object { Get-RangeKey $_ })
        if (($actualEvidence -join ',') -cne
            (@($expectedFamily.evidence) -join ',')) {
            throw "Exact evidence range contract drift: $($run.familyId)"
        }
    }

    foreach ($familyId in @($golden.Keys)) {
        if ([int]$counts[$familyId] -ne
            [int]$golden[$familyId].runCount) {
            throw "Family count mismatch for $familyId."
        }
    }
    if ($receiptChecks -ne 37) {
        throw "The exact 19 runs must perform 37 evidence receipt checks, got $receiptChecks."
    }
    $digest = Get-ScheduleDigest $Schedule
    if ($digest -cne $script:GoldenScheduleSha256) {
        throw "Schedule/contract SHA-256 drift. Expected $($script:GoldenScheduleSha256), got $digest."
    }
}

function Assert-SelfTestThrows {
    param([scriptblock]$Action, [string]$Label)

    $threw = $false
    try {
        & $Action
    }
    catch {
        $threw = $true
    }
    if (-not $threw) {
        throw "Mutation self-test did not fail: $Label"
    }
    $script:SelfTestMutationCount++
}

function New-SyntheticCompletedState {
    param([object[]]$Schedule)

    $head = 'aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa'
    $originalEventPath = $script:EventJournalPath
    $originalReportPath = $script:ReportPath
    $StatePath = Join-Path $RunRoot 'capture_state.json'
    $script:EventJournalPath = Join-Path $RunRoot 'capture_events.ndjson'
    $script:ReportPath = Join-Path $RunRoot 'capture_report.json'
    try {
        $state = New-OrchestratorState $Schedule $head `
            $script:GoldenScheduleSha256
    }
    finally {
        $script:EventJournalPath = $originalEventPath
        $script:ReportPath = $originalReportPath
    }
    $started = [DateTimeOffset]::UtcNow.AddMinutes(-2).ToString('O')
    $completed = [DateTimeOffset]::UtcNow.AddMinutes(-1).ToString('O')
    for ($index = 0; $index -lt 19; $index++) {
        $run = $Schedule[$index]
        $saved = $state.runs[$index]
        $captureId = 'selftest-' + $run.runId
        $output = Join-Path $OutputRoot $captureId
        $log = Join-Path $RunRoot ($run.runId + '.log')
        $saved.status = 'completed'
        $saved.startedAtUtc = $started
        $saved.completedAtUtc = $completed
        $saved.unityExitCode = 0
        $saved.logPath = $log
        $saved.unityArguments = @(Get-UnityArguments $run $log)
        $saved.captureId = $captureId
        $saved.outputDirectory = $output
        $saved.manifestPath = Join-Path $output 'capture_manifest.json'
        $saved.manifestSha256 = 'b' * 64
        $saved.testResultCount = 1
        $saved.evidenceReceiptCount = $run.definition.evidenceRanges.Count
        $saved.failure = ''
    }
    $state
}

function Invoke-MutexProbeHolder {
    param([string]$MutexName, [string]$ReadyName, [string]$ReleaseName)

    if ([string]::IsNullOrWhiteSpace($MutexName) -or
        [string]::IsNullOrWhiteSpace($ReadyName) -or
        [string]::IsNullOrWhiteSpace($ReleaseName)) {
        throw 'Mutex probe requires mutex, ready-event, and release-event names.'
    }
    $probeMutex = $null
    $ready = $null
    $release = $null
    try {
        $probeMutex = Enter-NamedCaptureMutex $MutexName
        $ready = [System.Threading.EventWaitHandle]::OpenExisting($ReadyName)
        $release = [System.Threading.EventWaitHandle]::OpenExisting($ReleaseName)
        [void]$ready.Set()
        if (-not $release.WaitOne([TimeSpan]::FromSeconds(15))) {
            throw 'Mutex probe release event timed out.'
        }
    }
    finally {
        if ($null -ne $ready) { $ready.Dispose() }
        if ($null -ne $release) { $release.Dispose() }
        Exit-NamedCaptureMutex $probeMutex ($null -ne $probeMutex)
    }
}

function Assert-MutexExclusionSelfTest {
    $suffix = [Guid]::NewGuid().ToString('N')
    $mutexName = 'Local\DimensionBrawl_PV60_MutexSelfTest_' + $suffix
    $readyName = 'Local\DimensionBrawl_PV60_ReadySelfTest_' + $suffix
    $releaseName = 'Local\DimensionBrawl_PV60_ReleaseSelfTest_' + $suffix
    $ready = New-Object System.Threading.EventWaitHandle(
        $false,
        [System.Threading.EventResetMode]::ManualReset,
        $readyName)
    $release = New-Object System.Threading.EventWaitHandle(
        $false,
        [System.Threading.EventResetMode]::ManualReset,
        $releaseName)
    $child = $null
    try {
        $powerShellExe = Join-Path $PSHOME 'powershell.exe'
        $childArguments = @(
            '-NoProfile','-ExecutionPolicy','Bypass','-File',$PSCommandPath,
            '-MutexProbeHold',$mutexName,
            '-MutexProbeReadyEvent',$readyName,
            '-MutexProbeReleaseEvent',$releaseName
        )
        $quoted = @($childArguments | ForEach-Object {
            Quote-WindowsProcessArgument $_
        }) -join ' '
        $child = Start-Process -FilePath $powerShellExe `
            -ArgumentList $quoted -WindowStyle Hidden -PassThru
        if (-not $ready.WaitOne([TimeSpan]::FromSeconds(10))) {
            throw 'Mutex self-test child did not signal readiness.'
        }
        Assert-SelfTestThrows {
            $unexpectedMutex = Enter-NamedCaptureMutex $mutexName
            Exit-NamedCaptureMutex $unexpectedMutex $true
        } 'named mutex excludes a second process'
        [void]$release.Set()
        Wait-Process -Id $child.Id -Timeout 10 -ErrorAction Stop
        $child.Refresh()
        if ($child.ExitCode -ne 0) {
            throw "Mutex self-test child exited with code $($child.ExitCode)."
        }
    }
    finally {
        [void]$release.Set()
        if ($null -ne $child) {
            try {
                $child.Refresh()
                if (-not $child.HasExited) {
                    Stop-Process -Id $child.Id -Force -ErrorAction SilentlyContinue
                }
            }
            finally {
                $child.Dispose()
            }
        }
        $ready.Dispose()
        $release.Dispose()
    }
}

function Invoke-SelfTest {
    $script:SelfTestMutationCount = 0
    $families = Get-FamilyDefinitions
    $schedule = @(Get-RunSchedule $families)
    Assert-DefinitionContracts $families $schedule
    $digest1 = Get-ScheduleDigest $schedule
    $digest2 = Get-ScheduleDigest @(Get-RunSchedule (Get-FamilyDefinitions))
    if ($digest1 -notmatch '^[0-9a-f]{64}$' -or $digest1 -ne $digest2 -or
        $digest1 -ne $script:GoldenScheduleSha256) {
        throw 'Schedule digest is not deterministic SHA-256.'
    }
    if ((Quote-WindowsProcessArgument 'C:\path with space\x') -ne
        '"C:\path with space\x"') {
        throw 'Windows command-line quoting self-test failed.'
    }

    $s030Exact = Test-FailureArtifactName `
        's030_capture_failure.json' 's030_capture_failure.json'
    $s030Timestamp = Test-FailureArtifactName `
        's030_capture_failure_20260817123456789.json' `
        's030_capture_failure.json'
    $s050Timestamp = Test-FailureArtifactName `
        's050_capture_failure_20260817123456789.json' `
        's050_capture_failure.json'
    $s050WrongTimestamp = Test-FailureArtifactName `
        's050_capture_failure_2026081712345678.json' `
        's050_capture_failure.json'
    if (-not $s030Exact -or -not $s030Timestamp -or
        -not $s050Timestamp -or $s050WrongTimestamp) {
        throw 'Failure-artifact exact/timestamp pattern self-test failed.'
    }
    $failureLocations = @(Get-FailureArtifactLocations `
        'C:\tmp\PV60-Failure-Location-SelfTest')
    if ($failureLocations.Count -ne 2 -or
        (Split-Path $failureLocations[0] -Leaf) -ne
            'PV60-Failure-Location-SelfTest' -or
        (Split-Path $failureLocations[1] -Leaf) -ne 'evidence' -or
        (Split-Path $failureLocations[1] -Parent) -ne
            $failureLocations[0]) {
        throw 'Failure-artifact capture-root/direct-evidence search self-test failed.'
    }

    $validManifestProvenance = [pscustomobject]@{
        unityVersion=$script:ExpectedUnityVersion
        unityVersionWithRevision=$script:ExpectedUnityVersionWithRevision
        recorderPackageVersion=$script:ExpectedRecorderVersion
        urpPackageVersion=$script:ExpectedUrpVersion
        activeRenderPipelineAssetPath=$script:ExpectedRenderPipelineAsset
    }
    Assert-ManifestEngineProvenance $validManifestProvenance 'self-test'
    $validManifestProvenance.unityVersion = '6000.3.5f1'
    Assert-SelfTestThrows {
        Assert-ManifestEngineProvenance $validManifestProvenance `
            'engine mutation'
    } 'manifest Unity version mutation'

    Assert-SelfTestThrows {
        Assert-PathEquivalent 'D:\DimensionBrawl_PV\WRONG' `
            $script:ExpectedOutputRoot 'frozen OutputRoot mutation'
    } 'frozen OutputRoot mutation'

    $mutatedFamilies = Get-FamilyDefinitions
    $mutatedSchedule = @(Get-RunSchedule $mutatedFamilies)
    $mutatedSchedule[0].runId = '01-g08-take1'
    Assert-SelfTestThrows {
        Assert-DefinitionContracts $mutatedFamilies $mutatedSchedule
    } 'schedule order mutation'

    $mutatedFamilies = Get-FamilyDefinitions
    $mutatedFamilies.g04.method = 'Wrong.Method'
    $mutatedSchedule = @(Get-RunSchedule $mutatedFamilies)
    Assert-SelfTestThrows {
        Assert-DefinitionContracts $mutatedFamilies $mutatedSchedule
    } 'executeMethod mutation'

    $mutatedFamilies = Get-FamilyDefinitions
    $mutatedFamilies.g04.headful = $true
    $mutatedSchedule = @(Get-RunSchedule $mutatedFamilies)
    Assert-SelfTestThrows {
        Assert-DefinitionContracts $mutatedFamilies $mutatedSchedule
    } 'G04 batch/headful flag mutation'

    $mutatedFamilies = Get-FamilyDefinitions
    $mutatedFamilies.g06.evidenceRanges[0].selectEnd = 478
    $mutatedSchedule = @(Get-RunSchedule $mutatedFamilies)
    Assert-SelfTestThrows {
        Assert-DefinitionContracts $mutatedFamilies $mutatedSchedule
    } 'evidence range mutation'

    $savedSyntheticStatePath = $StatePath
    $savedSyntheticEventPath = $script:EventJournalPath
    $savedSyntheticReportPath = $script:ReportPath
    try {
        $StatePath = Join-Path $RunRoot 'capture_state.json'
        $script:EventJournalPath = Join-Path $RunRoot `
            'capture_events.ndjson'
        $script:ReportPath = Join-Path $RunRoot 'capture_report.json'
        $syntheticState = New-SyntheticCompletedState $schedule
        Assert-CompletedStateMetadata $syntheticState $schedule `
            $syntheticState.pinnedHead
        $syntheticState.runs[1].captureId =
            $syntheticState.runs[0].captureId
        $syntheticState.runs[1].outputDirectory =
            $syntheticState.runs[0].outputDirectory
        $syntheticState.runs[1].manifestPath =
            $syntheticState.runs[0].manifestPath
        Assert-SelfTestThrows {
            Assert-CompletedStateMetadata $syntheticState $schedule `
                $syntheticState.pinnedHead
        } 'final output uniqueness mutation'

        $syntheticState = New-SyntheticCompletedState $schedule
        $syntheticState.runs[0].method = 'Wrong.Final.State.Method'
        Assert-SelfTestThrows {
            Assert-CompletedStateMetadata $syntheticState $schedule `
                $syntheticState.pinnedHead
        } 'final state run metadata mutation'

        $syntheticState = New-SyntheticCompletedState $schedule
        $syntheticState.runIdentity = ''
        Assert-SelfTestThrows {
            Assert-CompletedStateMetadata $syntheticState $schedule `
                $syntheticState.pinnedHead
        } 'final state runIdentity mutation'

        $syntheticState = New-SyntheticCompletedState $schedule
        $syntheticState.runs[5].takeOrdinal = 99
        Assert-SelfTestThrows {
            Assert-StateCompatible $syntheticState `
                $script:GoldenScheduleSha256 $schedule
        } 'resume sealed schedule-row mutation before DryRun'

        $syntheticState = New-SyntheticCompletedState $schedule
        $syntheticState.runs[0].unityArguments += '-quit'
        Assert-SelfTestThrows {
            Assert-StateCompatible $syntheticState `
                $script:GoldenScheduleSha256 $schedule
        } 'resume fixed Unity-argument mutation before DryRun'
    }
    finally {
        $StatePath = $savedSyntheticStatePath
        $script:EventJournalPath = $savedSyntheticEventPath
        $script:ReportPath = $savedSyntheticReportPath
    }

    $savedRunRoot = $RunRoot
    $savedStatePath = $StatePath
    $savedEventPath = $script:EventJournalPath
    $savedReportPath = $script:ReportPath
    try {
        $RunRoot = $PSScriptRoot
        Assert-SelfTestThrows { Assert-FreshRunRoot } `
            'non-empty fresh RunRoot mutation'
        $RunRoot = 'C:\tmp\DimensionBrawl-PV60-NoWrite-SelfTest'
        $StatePath = Join-Path $RunRoot 'alternate_state.json'
        $script:EventJournalPath = Join-Path $RunRoot `
            'capture_events.ndjson'
        $script:ReportPath = Join-Path $RunRoot 'capture_report.json'
        Assert-SelfTestThrows { Assert-CanonicalOrchestrationPaths } `
            'alternate StatePath mutation'
        $StatePath = Join-Path $RunRoot 'capture_state.json'
        Assert-SelfTestThrows {
            Assert-InvocationStorageDisposition $true
        } 'Resume with missing canonical state mutation'
    }
    finally {
        $RunRoot = $savedRunRoot
        $StatePath = $savedStatePath
        $script:EventJournalPath = $savedEventPath
        $script:ReportPath = $savedReportPath
    }

    $finalAuditVerifierCallCount = 0
    $savedScriptState = $script:State
    $savedFinalAuditStatePath = $StatePath
    $savedFinalAuditEventPath = $script:EventJournalPath
    $savedFinalAuditReportPath = $script:ReportPath
    try {
        $StatePath = Join-Path $RunRoot 'capture_state.json'
        $script:EventJournalPath = Join-Path $RunRoot `
            'capture_events.ndjson'
        $script:ReportPath = Join-Path $RunRoot 'capture_report.json'
        $script:State = New-SyntheticCompletedState $schedule
        $finalAuditState = $script:State
        $finalAuditCounter = [pscustomobject]@{ count = 0 }
        $finalAuditVerifier = {
            param($CaptureDirectory, $Definition, $ExpectedHead)

            $index = [int]$finalAuditCounter.count
            $saved = $finalAuditState.runs[$index]
            if ($CaptureDirectory -ne $saved.outputDirectory -or
                $Definition.id -ne $saved.familyId -or
                $ExpectedHead -ne $finalAuditState.pinnedHead) {
                throw "Final 19-output verifier seam received the wrong run at index $index."
            }
            $finalAuditCounter.count = $index + 1
            [pscustomobject]@{
                captureId = $saved.captureId
                outputDirectory = $saved.outputDirectory
                manifestPath = $saved.manifestPath
                manifestSha256 = $saved.manifestSha256
                testResultCount = $saved.testResultCount
                evidenceReceiptCount = $saved.evidenceReceiptCount
            }
        }.GetNewClosure()
        Assert-AllCompletedOutputs $schedule $script:State.pinnedHead `
            -OutputVerifier $finalAuditVerifier -SelfTestSkipFilePresence
        $finalAuditVerifierCallCount = [int]$finalAuditCounter.count
        if ($finalAuditVerifierCallCount -ne 19) {
            throw "Final output audit did not invoke its verifier exactly 19 times: $finalAuditVerifierCallCount"
        }
    }
    finally {
        $script:State = $savedScriptState
        $StatePath = $savedFinalAuditStatePath
        $script:EventJournalPath = $savedFinalAuditEventPath
        $script:ReportPath = $savedFinalAuditReportPath
    }

    Assert-MutexExclusionSelfTest
    [pscustomobject][ordered]@{
        status = 'passed'
        runCount = $schedule.Count
        scheduleSha256 = $digest1
        mutationCount = $script:SelfTestMutationCount
        evidenceReceiptCheckCount = 37
        finalOutputVerifierCallCount = $finalAuditVerifierCallCount
        mutexExclusion = 'passed-two-process-named-mutex-probe'
        firstRound = @($schedule | Select-Object -First 7 |
            ForEach-Object familyId)
        repetitionCounts = [pscustomobject]@{
            city=3; s030=3; s050=1; g04=3; g06=3; g07=3; g08=3
        }
        pngPolicy = 'metadata/hash streaming only; no PNG pixel decode or raw byte load'
    }
}

if (-not [string]::IsNullOrWhiteSpace($MutexProbeHold) -or
    -not [string]::IsNullOrWhiteSpace($MutexProbeReadyEvent) -or
    -not [string]::IsNullOrWhiteSpace($MutexProbeReleaseEvent)) {
    Invoke-MutexProbeHolder $MutexProbeHold $MutexProbeReadyEvent `
        $MutexProbeReleaseEvent
    return
}

if ($SelfTest) {
    Invoke-SelfTest | ConvertTo-Json -Depth 8
    return
}

Enter-OrchestratorMutex
try {
$families = Get-FamilyDefinitions
$schedule = @(Get-RunSchedule $families)
Assert-DefinitionContracts $families $schedule
$sequenceSha = Get-ScheduleDigest $schedule

$ProjectRoot = [System.IO.Path]::GetFullPath($ProjectRoot)
$OutputRoot = [System.IO.Path]::GetFullPath($OutputRoot)
$RunRoot = [System.IO.Path]::GetFullPath($RunRoot)
if ([string]::IsNullOrWhiteSpace($StatePath)) {
    $StatePath = Join-Path $RunRoot 'capture_state.json'
}
else {
    $StatePath = [System.IO.Path]::GetFullPath($StatePath)
}
$script:EventJournalPath = Join-Path $RunRoot 'capture_events.ndjson'
$script:ReportPath = Join-Path $RunRoot 'capture_report.json'

try {
    Assert-FrozenCaptureEnvironment
    if (-not (Test-Path -LiteralPath $ProjectRoot -PathType Container)) {
        throw "Project root is missing: $ProjectRoot"
    }
    if (-not (Test-Path -LiteralPath (Join-Path $ProjectRoot '.git'))) {
        throw "Project root is not the Git worktree root: $ProjectRoot"
    }
    if (-not (Test-Path -LiteralPath $UnityExe -PathType Leaf)) {
        throw "Unity executable is missing: $UnityExe"
    }
    if (-not (Test-Path -LiteralPath $OutputRoot -PathType Container)) {
        throw "Capture output root is missing: $OutputRoot"
    }
    Assert-InvocationStorageDisposition ([bool]$Resume)

    Assert-PreRunProcessExclusion
    $preflightResources = Get-ResourceSnapshot
    Assert-ResourceFloor $preflightResources

    $currentHead = (Invoke-GitRead @('rev-parse', 'HEAD')).ToLowerInvariant()
    if (-not [string]::IsNullOrWhiteSpace($PinnedHead) -and
        $currentHead -ne $PinnedHead.ToLowerInvariant()) {
        throw "Current HEAD $currentHead does not match -PinnedHead $PinnedHead."
    }
    $effectiveHead = if ([string]::IsNullOrWhiteSpace($PinnedHead)) {
        $currentHead
    } else {
        $PinnedHead.ToLowerInvariant()
    }
    Assert-GitPin $effectiveHead 'orchestrator preflight' | Out-Null

    $validatedResumeState = $null
    if ($Resume) {
        $validatedResumeState = Read-JsonBounded $StatePath
        Assert-StateCompatible $validatedResumeState $sequenceSha $schedule
        $stateHead = [string](Get-PropertyValue $validatedResumeState `
            'pinnedHead' '')
        if ($stateHead -ne $effectiveHead) {
            throw "Saved state HEAD $stateHead does not match current pinned HEAD $effectiveHead."
        }
        if ((Get-PropertyValue $validatedResumeState 'status' '') -eq
            'complete') {
            throw "The saved 19-run sequence is already complete: $StatePath"
        }
        if (@($validatedResumeState.runs | Where-Object {
                $_.status -eq 'failed'
            }).Count -gt 0) {
            throw 'Saved state contains a failed run. Automatic retry is forbidden; retained failure outputs require operator disposition.'
        }
        Assert-ResumeArtifactsIdentity $validatedResumeState
    }
    if ($DryRun) {
        [pscustomobject][ordered]@{
            schema = 'dimension-brawl.audition-pv.pv60-capture-dry-run.v2'
            status = 'ready'
            writesPerformed = $false
            unityStarted = $false
            pinnedHead = $effectiveHead
            sequenceSha256 = $sequenceSha
            goldenSequenceSha256 = $script:GoldenScheduleSha256
            evidenceReceiptCheckCount = 37
            invocationMode = if ($Resume) { 'resume' } else { 'fresh' }
            runIdentity = if ($Resume) {
                $validatedResumeState.runIdentity
            } else { '<created-only-by-live-run>' }
            runRootDisposition = if ($Resume) {
                'existing-identity-validated'
            } else { 'missing-or-empty-and-write-free' }
            resourceSnapshot = $preflightResources
            schedule = @($schedule | ForEach-Object {
                $probe = "<unique-log-$($_.runId)>"
                [pscustomobject][ordered]@{
                    sequence = $_.sequence
                    runId = $_.runId
                    familyId = $_.familyId
                    takeOrdinal = $_.takeOrdinal
                    mode = if ($_.definition.headful) { 'headful' } else { 'g04-batch-graphics' }
                    arguments = @(Get-UnityArguments $_ $probe)
                    expectedShots = $_.definition.shots
                    evidenceRanges = $_.definition.evidenceRanges
                }
            })
        } | ConvertTo-Json -Depth 12
        return
    }

    if ($Resume) {
        $script:State = $validatedResumeState
    }
    else {
        if (-not (Test-Path -LiteralPath $RunRoot -PathType Container)) {
            New-Item -ItemType Directory -Path $RunRoot -Force | Out-Null
        }
        $script:State = New-OrchestratorState $schedule $effectiveHead $sequenceSha
        $script:State.lastResourceSnapshot = $preflightResources
        Save-State
        Write-Event 'orchestrator-created' ([pscustomobject]@{
            pinnedHead = $effectiveHead
            sequenceSha256 = $sequenceSha
            runCount = 19
        })
        Write-Report 'running'
    }

    for ($index = 0; $index -lt $schedule.Count; $index++) {
        $run = $schedule[$index]
        $savedRun = $script:State.runs[$index]
        if ($savedRun.runId -ne $run.runId) {
            throw "Saved run identity mismatch at sequence $($run.sequence)."
        }
        if ($savedRun.status -eq 'completed') {
            $verified = Assert-CaptureOutput $savedRun.outputDirectory `
                $run.definition $effectiveHead
            if ($verified.manifestSha256 -ne $savedRun.manifestSha256) {
                throw "Completed output manifest changed for $($run.runId)."
            }
            continue
        }
        if ($savedRun.status -eq 'running') {
            Recover-InterruptedRun $savedRun $run $effectiveHead
            continue
        }
        if ($savedRun.status -ne 'pending') {
            throw "Run $($run.runId) has unsupported status '$($savedRun.status)'."
        }
        Invoke-CaptureRun $run $savedRun $effectiveHead
    }

    Assert-GitPin $effectiveHead 'orchestrator finalization' | Out-Null
    Assert-PreRunProcessExclusion
    Assert-AllCompletedOutputs $schedule $effectiveHead
    Assert-GitPin $effectiveHead 'after final 19-output audit' | Out-Null
    $finalResources = Get-ResourceSnapshot
    Assert-ResourceFloor $finalResources
    $script:State.lastResourceSnapshot = $finalResources
    $script:State.status = 'complete'
    $script:State.failure = ''
    Save-State
    Write-Event 'orchestrator-completed' ([pscustomobject]@{
        completedRunCount = 19
        finalResourceSnapshot = $finalResources
    })
    Write-Report 'complete'
    Write-Host "PV60 capture sequence complete. Report: $script:ReportPath"
}
catch {
    if ($null -ne $script:OwnedUnityProcess) {
        Stop-OwnedUnityProcess $script:OwnedUnityProcess $_.Exception.Message
    }
    if ($null -ne $script:State) {
        $script:State.status = 'failed'
        $script:State.failure = $_.Exception.ToString()
        Save-State
        Write-Event 'orchestrator-failed' ([pscustomobject]@{
            failure = $_.Exception.ToString()
        })
        Write-Report 'failed' $_.Exception.ToString()
    }
    throw
}
}
finally {
    Exit-OrchestratorMutex
}
