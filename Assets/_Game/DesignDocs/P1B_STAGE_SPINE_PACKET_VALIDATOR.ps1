[CmdletBinding()]
param(
    [ValidateSet('Readiness', 'LiveAcceptance')]
    [string]$Mode = 'Readiness',

    [string]$EvidenceIndexPath,

    [string]$BacklogPath,

    [switch]$ReportOnly
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$validatorId = 'P1B-STAGE-SPINE-PACKET-VALIDATOR-02'
$packetId = 'P1B-PGR-HI3-STAGE-SPINE-01'
$script:Failures = @()
$script:Warnings = @()

function Add-Failure {
    param([string]$Code, [string]$Message)
    $script:Failures += [pscustomobject][ordered]@{ code = $Code; message = $Message }
}

function Add-Warning {
    param([string]$Code, [string]$Message)
    $script:Warnings += [pscustomobject][ordered]@{ code = $Code; message = $Message }
}

function Get-PropertyValue {
    param($Object, [string]$Name)
    if ($null -eq $Object) { return $null }
    $property = $Object.PSObject.Properties[$Name]
    if ($null -eq $property) { return $null }
    return $property.Value
}

function Has-Property {
    param($Object, [string]$Name)
    return $null -ne $Object -and $null -ne $Object.PSObject.Properties[$Name]
}

function Has-Text {
    param($Value)
    return $null -ne $Value -and -not [string]::IsNullOrWhiteSpace([string]$Value)
}

function Test-Sha256Text {
    param($Value)
    return (Has-Text $Value) -and ([string]$Value -match '^[0-9A-Fa-f]{64}$')
}

function Assert-Condition {
    param([bool]$Condition, [string]$Code, [string]$Message)
    if (-not $Condition) { Add-Failure $Code $Message }
}

function Read-JsonFile {
    param([string]$Path, [string]$Code)
    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        Add-Failure $Code "JSON file does not exist: $Path"
        return $null
    }

    try {
        return Get-Content -LiteralPath $Path -Raw -Encoding UTF8 | ConvertFrom-Json
    }
    catch {
        Add-Failure $Code "JSON parse failed for ${Path}: $($_.Exception.Message)"
        return $null
    }
}

function Resolve-ArtifactPath {
    param([string]$RecordedPath)
    if (-not (Has-Text $RecordedPath)) { return $null }
    if ([System.IO.Path]::IsPathRooted($RecordedPath)) { return $RecordedPath }

    $normalized = $RecordedPath -replace '/', [System.IO.Path]::DirectorySeparatorChar
    if ($normalized.StartsWith('_Game' + [System.IO.Path]::DirectorySeparatorChar)) {
        return Join-Path $script:AssetsRoot $normalized
    }

    return Join-Path $PSScriptRoot $normalized
}

function Test-RecordedArtifact {
    param($Container, [string]$PathProperty, [string]$HashProperty, [string]$Code)
    $recordedPath = Get-PropertyValue $Container $PathProperty
    $recordedHash = Get-PropertyValue $Container $HashProperty
    Assert-Condition (Has-Text $recordedPath) "$Code-PATH" "$PathProperty is missing."
    Assert-Condition (Test-Sha256Text $recordedHash) "$Code-HASH-FORMAT" "$HashProperty must be a SHA-256 value."
    if (-not (Has-Text $recordedPath)) { return }

    $resolved = Resolve-ArtifactPath ([string]$recordedPath)
    Assert-Condition (Test-Path -LiteralPath $resolved -PathType Leaf) "$Code-MISSING" "Artifact is missing: $resolved"
    if ((Test-Path -LiteralPath $resolved -PathType Leaf) -and (Test-Sha256Text $recordedHash)) {
        $recordedSize = Get-PropertyValue $Container 'reportSizeBytes'
        if ($null -ne $recordedSize) {
            $actualSize = (Get-Item -LiteralPath $resolved).Length
            Assert-Condition ($recordedSize -is [ValueType] -and [int64]$recordedSize -eq $actualSize) "$Code-SIZE-MISMATCH" "Artifact size mismatch for $recordedPath."
        }
        $actual = (Get-FileHash -LiteralPath $resolved -Algorithm SHA256).Hash.ToUpperInvariant()
        Assert-Condition ($actual -eq ([string]$recordedHash).ToUpperInvariant()) "$Code-HASH-MISMATCH" "Artifact hash mismatch for $recordedPath."
    }
}

function Get-CanonicalCellProjection {
    param([object[]]$Cells)
    $projected = @(
        $Cells |
            Sort-Object @{ Expression = { [string](Get-PropertyValue $_ 'foreignSourceId') } },
                        @{ Expression = { [string](Get-PropertyValue $_ 'foreignSourceSnapshotId') } },
                        @{ Expression = { [string](Get-PropertyValue $_ 'foreignRowOrKey') } },
                        @{ Expression = { [string](Get-PropertyValue $_ 'semanticSlotId') } } |
            ForEach-Object {
                [pscustomobject][ordered]@{
                    foreignSourceId = Get-PropertyValue $_ 'foreignSourceId'
                    foreignSourceSnapshotId = Get-PropertyValue $_ 'foreignSourceSnapshotId'
                    foreignSourceOrdinal = Get-PropertyValue $_ 'foreignSourceOrdinal'
                    foreignRowOrKey = Get-PropertyValue $_ 'foreignRowOrKey'
                    semanticSlotId = Get-PropertyValue $_ 'semanticSlotId'
                    valueState = Get-PropertyValue $_ 'valueState'
                    foreignFieldPaths = @((Get-PropertyValue $_ 'foreignFieldPaths'))
                    claimId = Get-PropertyValue $_ 'claimId'
                    sourceMappingRef = Get-PropertyValue $_ 'sourceMappingRef'
                    supportedStatement = Get-PropertyValue $_ 'supportedStatement'
                    foreignClassification = Get-PropertyValue $_ 'foreignClassification'
                    foreignEvidenceRef = Get-PropertyValue $_ 'foreignEvidenceRef'
                    dimensionBrawlOwnerState = Get-PropertyValue $_ 'dimensionBrawlOwnerState'
                    dimensionBrawlOwner = Get-PropertyValue $_ 'dimensionBrawlOwner'
                    dimensionBrawlField = Get-PropertyValue $_ 'dimensionBrawlField'
                    dimensionBrawlClassification = Get-PropertyValue $_ 'dimensionBrawlClassification'
                    dimensionBrawlEvidenceRef = Get-PropertyValue $_ 'dimensionBrawlEvidenceRef'
                    dimensionBrawlCutoffRef = Get-PropertyValue $_ 'dimensionBrawlCutoffRef'
                    dimensionBrawlOwnerBoundary = Get-PropertyValue $_ 'dimensionBrawlOwnerBoundary'
                    negativeBoundary = Get-PropertyValue $_ 'negativeBoundary'
                    sourceValueCopied = Get-PropertyValue $_ 'sourceValueCopied'
                }
            }
    )
    return ($projected | ConvertTo-Json -Compress -Depth 8)
}

function Test-CellSet {
    param(
        [object[]]$Cells,
        [string[]]$SemanticSlots,
        [string[]]$AllowedValueStates,
        [string[]]$AllowedOwnerStates,
        [string[]]$AllowedClassifications,
        [string[]]$ForbiddenSourceIds,
        [switch]$RequireSeventy
    )

    $allowedProperties = @(
        'foreignSourceId',
        'foreignSourceSnapshotId',
        'foreignSourceOrdinal',
        'foreignRowOrKey',
        'semanticSlotId',
        'valueState',
        'foreignFieldPaths',
        'claimId',
        'sourceMappingRef',
        'supportedStatement',
        'foreignClassification',
        'foreignEvidenceRef',
        'dimensionBrawlOwnerState',
        'dimensionBrawlOwner',
        'dimensionBrawlField',
        'dimensionBrawlClassification',
        'dimensionBrawlEvidenceRef',
        'dimensionBrawlCutoffRef',
        'dimensionBrawlOwnerBoundary',
        'negativeBoundary',
        'sourceValueCopied'
    )

    if ($RequireSeventy) {
        Assert-Condition ($Cells.Count -eq 70) 'LIVE-CELL-COUNT' "Live packet must contain exactly 70 cells; found $($Cells.Count)."
    }
    else {
        Assert-Condition ($Cells.Count -in @(0, 70)) 'READINESS-CELL-COUNT' "Crosswalk rows must be empty or a complete 70-cell candidate; found $($Cells.Count)."
    }

    $identities = @()
    foreach ($cell in $Cells) {
        $properties = @($cell.PSObject.Properties.Name)
        foreach ($name in $properties) {
            Assert-Condition ($name -in $allowedProperties) 'CELL-UNEXPECTED-PROPERTY' "Crosswalk cell contains an unapproved property '$name'."
        }
        foreach ($name in $allowedProperties) {
            Assert-Condition (Has-Property $cell $name) 'CELL-MISSING-PROPERTY' "Crosswalk cell is missing required property '$name'."
        }

        $sourceId = [string](Get-PropertyValue $cell 'foreignSourceId')
        $snapshotId = [string](Get-PropertyValue $cell 'foreignSourceSnapshotId')
        $rowKey = [string](Get-PropertyValue $cell 'foreignRowOrKey')
        $slot = [string](Get-PropertyValue $cell 'semanticSlotId')
        $valueState = [string](Get-PropertyValue $cell 'valueState')
        $ownerState = [string](Get-PropertyValue $cell 'dimensionBrawlOwnerState')
        $foreignClassification = [string](Get-PropertyValue $cell 'foreignClassification')
        $dimensionBrawlClassification = [string](Get-PropertyValue $cell 'dimensionBrawlClassification')
        $fieldPaths = @((Get-PropertyValue $cell 'foreignFieldPaths'))
        $sourceValueCopied = Get-PropertyValue $cell 'sourceValueCopied'

        Assert-Condition (Has-Text $sourceId) 'CELL-SOURCE' 'foreignSourceId is required.'
        Assert-Condition (Has-Text $snapshotId) 'CELL-SNAPSHOT' 'foreignSourceSnapshotId is required.'
        Assert-Condition (Has-Text $rowKey) 'CELL-ROW' 'foreignRowOrKey is required.'
        Assert-Condition ($slot -in $SemanticSlots) 'CELL-SLOT' "Unknown semanticSlotId '$slot'."
        Assert-Condition ($valueState -in $AllowedValueStates) 'CELL-VALUE-STATE' "Unknown valueState '$valueState'."
        Assert-Condition ($ownerState -in $AllowedOwnerStates) 'CELL-OWNER-STATE' "Unknown dimensionBrawlOwnerState '$ownerState'."
        Assert-Condition ($foreignClassification -in $AllowedClassifications) 'CELL-FOREIGN-CLASSIFICATION' "Unknown foreignClassification '$foreignClassification'."
        Assert-Condition ($dimensionBrawlClassification -in $AllowedClassifications) 'CELL-DIMENSIONBRAWL-CLASSIFICATION' "Unknown dimensionBrawlClassification '$dimensionBrawlClassification'."
        Assert-Condition ($sourceId -notin $ForbiddenSourceIds) 'CELL-NEGATIVE-CONTROL-SOURCE' "Historical negative-control source '$sourceId' cannot populate live crosswalk cells."
        Assert-Condition ($sourceValueCopied -is [bool] -and -not $sourceValueCopied) 'CELL-SOURCE-VALUE-COPIED' 'sourceValueCopied must be the boolean false.'
        Assert-Condition (Has-Text (Get-PropertyValue $cell 'claimId')) 'CELL-CLAIM-ID' 'claimId is required.'
        Assert-Condition (Has-Text (Get-PropertyValue $cell 'sourceMappingRef')) 'CELL-SOURCE-MAPPING-REF' 'sourceMappingRef is required.'
        Assert-Condition (Has-Text (Get-PropertyValue $cell 'supportedStatement')) 'CELL-SUPPORTED-STATEMENT' 'supportedStatement is required.'
        Assert-Condition (Has-Text (Get-PropertyValue $cell 'foreignEvidenceRef')) 'CELL-FOREIGN-EVIDENCE-REF' 'foreignEvidenceRef is required.'
        Assert-Condition (Has-Text (Get-PropertyValue $cell 'dimensionBrawlEvidenceRef')) 'CELL-DIMENSIONBRAWL-EVIDENCE-REF' 'dimensionBrawlEvidenceRef is required.'
        Assert-Condition (Has-Text (Get-PropertyValue $cell 'dimensionBrawlCutoffRef')) 'CELL-DIMENSIONBRAWL-CUTOFF-REF' 'dimensionBrawlCutoffRef is required.'
        Assert-Condition (Has-Text (Get-PropertyValue $cell 'negativeBoundary')) 'CELL-NEGATIVE-BOUNDARY' 'negativeBoundary is required.'
        Assert-Condition (Has-Text (Get-PropertyValue $cell 'dimensionBrawlOwnerBoundary')) 'CELL-OWNER-BOUNDARY' 'dimensionBrawlOwnerBoundary is required.'
        if ($valueState -eq 'present') {
            Assert-Condition ($fieldPaths.Count -gt 0) 'CELL-PRESENT-FIELDS' 'present requires at least one foreignFieldPath.'
        }
        if ($valueState -in @('absent', 'not-applicable')) {
            Assert-Condition ($fieldPaths.Count -eq 0) 'CELL-ABSENCE-FIELDS' "$valueState must not carry foreignFieldPaths."
        }
        $owner = Get-PropertyValue $cell 'dimensionBrawlOwner'
        $ownerField = Get-PropertyValue $cell 'dimensionBrawlField'
        if ($ownerState -eq 'present') {
            Assert-Condition ((Has-Text $owner) -and (Has-Text $ownerField)) 'CELL-OWNER-PRESENT' 'present owner state requires dimensionBrawlOwner and dimensionBrawlField.'
            Assert-Condition ($dimensionBrawlClassification -in @('proven-static', 'proven-runtime')) 'CELL-OWNER-PRESENT-CLASSIFICATION' 'present owner state requires proven-static or proven-runtime DimensionBrawl classification.'
        }
        if ($ownerState -eq 'absent') {
            Assert-Condition (-not (Has-Text $owner) -and -not (Has-Text $ownerField)) 'CELL-OWNER-ABSENT' 'absent owner state must use null owner and field plus the explicit boundary.'
            Assert-Condition ($dimensionBrawlClassification -eq 'proven-static') 'CELL-OWNER-ABSENT-CLASSIFICATION' 'absent owner state requires proven-static DimensionBrawl classification.'
        }
        if ($ownerState -eq 'unresolved') {
            Assert-Condition ((Has-Text $owner) -eq (Has-Text $ownerField)) 'CELL-OWNER-UNRESOLVED' 'unresolved owner candidates must name both owner and field or neither.'
            Assert-Condition ($dimensionBrawlClassification -eq 'unknown') 'CELL-OWNER-UNRESOLVED-CLASSIFICATION' 'unresolved owner state requires unknown DimensionBrawl classification.'
        }

        $identity = "$sourceId`u001f$snapshotId`u001f$rowKey`u001f$slot"
        $identities += $identity
    }

    Assert-Condition ((@($identities | Sort-Object -Unique)).Count -eq $identities.Count) 'CELL-DUPLICATE-IDENTITY' 'Composite crosswalk cell identities must be unique.'
}

if (-not (Has-Text $EvidenceIndexPath)) {
    $EvidenceIndexPath = Join-Path $PSScriptRoot 'SUBCULTURE_DATASET_EVIDENCE_INDEX.json'
}
if (-not (Has-Text $BacklogPath)) {
    $BacklogPath = Join-Path $PSScriptRoot 'SUBCULTURE_GAP_BACKLOG.json'
}

$script:AssetsRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$index = Read-JsonFile $EvidenceIndexPath 'INDEX'
$backlog = Read-JsonFile $BacklogPath 'BACKLOG'

$packet = $null
$backlogItem = $null
$observations = [ordered]@{}

if ($null -ne $index -and $null -ne $backlog) {
    Assert-Condition ((Get-PropertyValue $index 'schemaVersion') -eq 2) 'INDEX-SCHEMA' 'Evidence index schemaVersion must be 2.'

    $sources = @((Get-PropertyValue $index 'sources'))
    $claims = @((Get-PropertyValue $index 'claims'))
    $sourceIds = @($sources | ForEach-Object { [string](Get-PropertyValue $_ 'sourceId') })
    $claimIds = @($claims | ForEach-Object { [string](Get-PropertyValue $_ 'claimId') })
    Assert-Condition ((@($sourceIds | Sort-Object -Unique)).Count -eq $sourceIds.Count) 'SOURCE-ID-UNIQUE' 'Source IDs must be unique.'
    Assert-Condition ((@($claimIds | Sort-Object -Unique)).Count -eq $claimIds.Count) 'CLAIM-ID-UNIQUE' 'Claim IDs must be unique.'

    $packets = @((Get-PropertyValue $index 'boundedPackets') | Where-Object { (Get-PropertyValue $_ 'packetId') -eq $packetId })
    Assert-Condition ($packets.Count -eq 1) 'PACKET-CARDINALITY' "Expected exactly one $packetId packet; found $($packets.Count)."
    if ($packets.Count -eq 1) { $packet = $packets[0] }

    $items = @((Get-PropertyValue $backlog 'items') | Where-Object { (Get-PropertyValue $_ 'itemId') -eq 'EVID-P1B-STAGE-SPINE' })
    Assert-Condition ($items.Count -eq 1) 'BACKLOG-ITEM-CARDINALITY' 'Expected exactly one EVID-P1B-STAGE-SPINE backlog item.'
    if ($items.Count -eq 1) { $backlogItem = $items[0] }

    foreach ($claim in $claims) {
        foreach ($sourceId in @((Get-PropertyValue $claim 'sourceIds')) + @((Get-PropertyValue $claim 'sourceMappings') | ForEach-Object { Get-PropertyValue $_ 'sourceId' })) {
            if (Has-Text $sourceId) {
                Assert-Condition ([string]$sourceId -in $sourceIds) 'CLAIM-SOURCE-REF' "Claim $(Get-PropertyValue $claim 'claimId') references missing source '$sourceId'."
            }
        }
    }

    if ($null -ne $packet) {
        $packetStatusVocabulary = @((Get-PropertyValue (Get-PropertyValue $index 'statusVocabulary') 'packetStatus') | ForEach-Object { [string]$_ })
        Assert-Condition ([string](Get-PropertyValue $packet 'status') -in $packetStatusVocabulary) 'PACKET-STATUS-VOCABULARY' "Packet status '$(Get-PropertyValue $packet 'status')' is outside statusVocabulary.packetStatus."

        $semanticSlots = @((Get-PropertyValue $packet 'foreignCrosswalkFields') | ForEach-Object { [string]$_ })
        $expectedSlots = @(
            'logicalStageId', 'physicalSceneOrScript', 'briefingAndCatalog', 'recommendedPowerOrLevel',
            'loadout', 'restrictions', 'entryCost', 'recordOrTargetTime', 'prerequisite', 'recommendedNext',
            'storyEntry', 'storyExit', 'challengeReference', 'resultReference'
        )
        Assert-Condition (($semanticSlots -join "`u001f") -eq ($expectedSlots -join "`u001f")) 'SEMANTIC-SLOT-ORDER' 'The fixed fourteen semantic slots or their order changed.'
        Assert-Condition ((Get-PropertyValue $packet 'outputArtifactSchemaVersion') -eq 2) 'OUTPUT-SCHEMA' 'Output artifact schemaVersion must be 2.'

        $recordSemantics = Get-PropertyValue $index 'recordSemantics'
        $valueVocabulary = Get-PropertyValue $recordSemantics 'packetCrosswalkValueStateVocabulary'
        $allowedValueStates = @($valueVocabulary.PSObject.Properties.Name)
        $expectedValueStates = @('present', 'absent', 'not-applicable', 'unresolved')
        Assert-Condition ((@($allowedValueStates | Sort-Object) -join '|') -eq (@($expectedValueStates | Sort-Object) -join '|')) 'VALUE-STATE-VOCABULARY' 'Crosswalk value-state vocabulary changed.'
        $ownerVocabulary = Get-PropertyValue $recordSemantics 'packetCrosswalkOwnerStateVocabulary'
        $allowedOwnerStates = @($ownerVocabulary.PSObject.Properties.Name)
        $expectedOwnerStates = @('present', 'absent', 'unresolved')
        Assert-Condition ((@($allowedOwnerStates | Sort-Object) -join '|') -eq (@($expectedOwnerStates | Sort-Object) -join '|')) 'OWNER-STATE-VOCABULARY' 'Crosswalk DimensionBrawl owner-state vocabulary changed.'
        $allowedClassifications = @('proven-static', 'proven-runtime', 'unknown', 'rejected')

        $inScopeSourceIds = @((Get-PropertyValue $packet 'inScopeSourceIds') | ForEach-Object { [string]$_ })
        Assert-Condition ((@($inScopeSourceIds | Sort-Object -Unique)).Count -eq $inScopeSourceIds.Count) 'IN-SCOPE-SOURCE-UNIQUE' 'inScopeSourceIds must be unique.'
        foreach ($sourceId in $inScopeSourceIds) {
            Assert-Condition ($sourceId -in $sourceIds) 'IN-SCOPE-SOURCE-REF' "inScopeSourceIds references missing source '$sourceId'."
        }

        $negativeControlClaimIds = @((Get-PropertyValue $packet 'negativeControlClaimIds') | ForEach-Object { [string]$_ })
        $expectedNegativeClaims = @('PGR-GUIDEFIGHT-2020-STATIC-NEGATIVE-CONTROL-01', 'HI3-STAGEDATA-2021-STATIC-NEGATIVE-CONTROL-01')
        Assert-Condition ((@($negativeControlClaimIds | Sort-Object) -join '|') -eq (@($expectedNegativeClaims | Sort-Object) -join '|')) 'NEGATIVE-CONTROL-CLAIMS' 'Both exact historical negative-control claims are required.'
        foreach ($claimId in $negativeControlClaimIds) {
            $matching = @($claims | Where-Object { (Get-PropertyValue $_ 'claimId') -eq $claimId })
            Assert-Condition ($matching.Count -eq 1 -and (Get-PropertyValue $matching[0] 'mappingStatus') -eq 'exact') 'NEGATIVE-CONTROL-CLAIM-EXACT' "Negative-control claim '$claimId' must exist and be exact."
        }
        $negativeSourceIds = @(
            $claims |
                Where-Object { (Get-PropertyValue $_ 'claimId') -in $negativeControlClaimIds } |
                ForEach-Object { @((Get-PropertyValue $_ 'sourceIds')) }
        )
        foreach ($sourceId in $negativeSourceIds) {
            Assert-Condition ([string]$sourceId -notin $inScopeSourceIds) 'NEGATIVE-CONTROL-ADMITTED' "Historical negative-control source '$sourceId' cannot be in inScopeSourceIds."
        }

        $captureReadiness = Get-PropertyValue $packet 'captureReadiness'
        Assert-Condition ((Get-PropertyValue $captureReadiness 'requiredRegisteredSourceCountAfterAdmission') -eq 11) 'REQUIRED-SOURCE-COUNT' 'requiredRegisteredSourceCountAfterAdmission must remain 11.'
        Assert-Condition ((Get-PropertyValue $captureReadiness 'requiredLiveRawOrBoundedSources') -eq 2) 'REQUIRED-LIVE-SOURCE-COUNT' 'requiredLiveRawOrBoundedSources must remain 2.'

        Test-RecordedArtifact (Get-PropertyValue $packet 'localPreflight') 'reportPath' 'reportSha256' 'LOCAL-PREFLIGHT'
        Test-RecordedArtifact (Get-PropertyValue $packet 'localStaticSupplement') 'reportPath' 'reportSha256' 'LOCAL-SUPPLEMENT'
        Test-RecordedArtifact (Get-PropertyValue $packet 'historicalHi3NegativeControl') 'reportPath' 'reportSha256' 'HI3-CONTROL'

        $liveAdmission = Get-PropertyValue $packet 'liveRawSourceAdmission'
        $pgrAdmission = Get-PropertyValue $liveAdmission 'pgr'
        $hi3Admission = Get-PropertyValue $liveAdmission 'hi3'
        $pgrLiveSourceId = Get-PropertyValue $pgrAdmission 'sourceId'
        $hi3LiveSourceId = Get-PropertyValue $hi3Admission 'sourceId'
        $hasPgrLive = Has-Text $pgrLiveSourceId
        $hasHi3Live = Has-Text $hi3LiveSourceId
        Assert-Condition ($hasPgrLive -eq $hasHi3Live) 'HALF-LIVE-ADMISSION' 'PGR and HI3 live sources must be admitted atomically, never one at a time.'
        if (-not $hasPgrLive) {
            Assert-Condition ($inScopeSourceIds.Count -eq 9) 'PENDING-SOURCE-COUNT' "Pending packet must retain exactly nine historical/report sources; found $($inScopeSourceIds.Count)."
            foreach ($claimId in @((Get-PropertyValue $packet 'inScopeClaimIds'))) {
                $claim = @($claims | Where-Object { (Get-PropertyValue $_ 'claimId') -eq $claimId }) | Select-Object -First 1
                Assert-Condition ($null -ne $claim -and (Get-PropertyValue $claim 'mappingStatus') -eq 'section-only') 'PENDING-CLAIM-STATUS' "Pending live claim '$claimId' must remain section-only."
                if ($null -ne $claim) {
                    Assert-Condition (@((Get-PropertyValue $claim 'sourceMappings')).Count -eq 0) 'PENDING-CLAIM-MAPPINGS' "Pending live claim '$claimId' must not carry exact sourceMappings."
                }
            }
        }
        else {
            Assert-Condition ($inScopeSourceIds.Count -eq 11) 'ADMITTED-SOURCE-COUNT' "Admitted packet must contain exactly eleven sources; found $($inScopeSourceIds.Count)."
        }

        $cells = @((Get-PropertyValue $packet 'crosswalkRows'))
        Test-CellSet $cells $semanticSlots $allowedValueStates $allowedOwnerStates $allowedClassifications $negativeSourceIds

        $generatedReportPath = Get-PropertyValue $packet 'generatedReportPath'
        $generatedReportSha = Get-PropertyValue $packet 'generatedReportSha256'
        if ($cells.Count -eq 0) {
            Assert-Condition (-not (Has-Text $generatedReportPath)) 'EMPTY-PACKET-REPORT-PATH' 'Empty crosswalkRows must not claim a generated report path.'
            Assert-Condition (-not (Has-Text $generatedReportSha)) 'EMPTY-PACKET-REPORT-HASH' 'Empty crosswalkRows must not claim a generated report hash.'
        }
        if ($cells.Count -eq 70) {
            Assert-Condition (Has-Text $generatedReportPath) 'COMPLETE-PACKET-REPORT-PATH' 'A 70-cell candidate requires generatedReportPath.'
            Assert-Condition (Test-Sha256Text $generatedReportSha) 'COMPLETE-PACKET-REPORT-HASH' 'A 70-cell candidate requires generatedReportSha256.'
        }

        $requiredOpenIds = @(
            'ACC-EVID-P1B-LIVE-PROVENANCE',
            'ACC-EVID-P1B-EXACT-ROWS',
            'ACC-EVID-P1B-DRIFT-CLASSIFICATION'
        )
        if ($null -ne $backlogItem) {
            $acceptance = @((Get-PropertyValue $backlogItem 'acceptance'))
            $acceptanceIds = @($acceptance | ForEach-Object { [string](Get-PropertyValue $_ 'acceptanceId') })
            Assert-Condition ((@($acceptanceIds | Sort-Object -Unique)).Count -eq $acceptanceIds.Count) 'BACKLOG-ACCEPTANCE-ID-UNIQUE' 'Backlog acceptance IDs must be unique.'
            $promotionGateIds = @((Get-PropertyValue (Get-PropertyValue $backlogItem 'promotionGate') 'allOf') | ForEach-Object { [string]$_ })
            Assert-Condition ((@($promotionGateIds | Sort-Object -Unique)).Count -eq $promotionGateIds.Count) 'BACKLOG-PROMOTION-ID-UNIQUE' 'promotionGate.allOf IDs must be unique.'
            foreach ($acceptanceId in $promotionGateIds) {
                Assert-Condition ($acceptanceId -in $acceptanceIds) 'BACKLOG-PROMOTION-REF' "promotionGate references missing acceptance '$acceptanceId'."
            }

            $evidenceIds = @((Get-PropertyValue $backlog 'evidenceRefs') | ForEach-Object { [string](Get-PropertyValue $_ 'evidenceRefId') })
            foreach ($evidenceId in @((Get-PropertyValue $backlogItem 'evidenceRefIds')) + @($acceptance | ForEach-Object { @((Get-PropertyValue $_ 'proofRefIds')) })) {
                if (Has-Text $evidenceId) {
                    Assert-Condition ([string]$evidenceId -in $evidenceIds) 'BACKLOG-EVIDENCE-REF' "Backlog item references missing evidence '$evidenceId'."
                }
            }
            Assert-Condition ($packetId -in @((Get-PropertyValue $backlogItem 'evidencePacketRefs'))) 'BACKLOG-PACKET-REF' "Backlog item must reference $packetId."

            if ($cells.Count -eq 0) {
                $actualOpen = @($acceptance | Where-Object { (Get-PropertyValue $_ 'required') -eq $true -and (Get-PropertyValue $_ 'result') -eq 'open' } | ForEach-Object { [string](Get-PropertyValue $_ 'acceptanceId') })
                Assert-Condition ((@($actualOpen | Sort-Object) -join '|') -eq (@($requiredOpenIds | Sort-Object) -join '|')) 'PENDING-OPEN-GATES' 'Current partial packet must expose exactly the three live-source acceptance gates as open.'
                Assert-Condition ((Get-PropertyValue $backlogItem 'lifecycleStatus') -eq 'partial') 'PENDING-BACKLOG-STATUS' 'Current evidence item must remain partial while crosswalkRows is empty.'
                $requiredPassCount = @($acceptance | Where-Object { (Get-PropertyValue $_ 'required') -eq $true -and (Get-PropertyValue $_ 'result') -eq 'pass' }).Count
                $requiredNonPassCount = @($acceptance | Where-Object { (Get-PropertyValue $_ 'required') -eq $true -and (Get-PropertyValue $_ 'result') -ne 'pass' }).Count
                Assert-Condition ($requiredPassCount -gt 0 -and $requiredNonPassCount -gt 0) 'PENDING-PARTIAL-TRUTH' 'Partial evidence requires at least one required pass and one required non-pass row.'
            }
        }

        $crossSnapshotWarnings = @((Get-PropertyValue $index 'crossSnapshotWarnings'))
        foreach ($warningId in @('PGR-GUIDEFIGHT-ROWCOUNT-DRIFT-01', 'HI3-STAGEDATA-2021-HISTORICAL-CONTROL-01')) {
            Assert-Condition ((@($crossSnapshotWarnings | Where-Object { (Get-PropertyValue $_ 'warningId') -eq $warningId })).Count -eq 1) 'CROSS-SNAPSHOT-WARNING' "Missing cross-snapshot warning '$warningId'."
        }

        $runLiveChecks = $Mode -eq 'LiveAcceptance' -or $cells.Count -gt 0 -or $hasPgrLive -or $hasHi3Live
        if ($runLiveChecks) {
            Assert-Condition ((Get-PropertyValue $packet 'status') -eq 'exact') 'LIVE-PACKET-STATUS' 'Live acceptance requires packet status exact.'
            Assert-Condition ($hasPgrLive -and $hasHi3Live) 'LIVE-SOURCE-ADMISSION' 'Both live source IDs are required.'
            if ($hasPgrLive -and $hasHi3Live) {
                Assert-Condition ([string]$pgrLiveSourceId -ne [string]$hi3LiveSourceId) 'LIVE-SOURCE-DISTINCT' 'PGR and HI3 live source IDs must be distinct.'
                foreach ($sourceId in @([string]$pgrLiveSourceId, [string]$hi3LiveSourceId)) {
                    Assert-Condition ($sourceId -in $inScopeSourceIds) 'LIVE-SOURCE-IN-SCOPE' "Live source '$sourceId' must be in inScopeSourceIds."
                    Assert-Condition ($sourceId -notin $negativeSourceIds) 'LIVE-SOURCE-NOT-CONTROL' "Live source '$sourceId' cannot be a historical control."
                }
            }

            foreach ($arm in @($pgrAdmission, $hi3Admission)) {
                foreach ($name in @('rawOrBoundedArchivePath', 'sourceRecordPath', 'producerManifestPath')) {
                    Assert-Condition (Has-Text (Get-PropertyValue $arm $name)) 'LIVE-ADMISSION-PROVENANCE' "Live admission arm is missing $name."
                }
            }

            $pgrIds = @((Get-PropertyValue $pgrAdmission 'requiredExactRowIds') | ForEach-Object { [string]$_ })
            $hi3Ids = @((Get-PropertyValue $hi3Admission 'requiredExactRowIds') | ForEach-Object { [string]$_ })
            Assert-Condition ($pgrIds.Count -eq 4 -and (@($pgrIds | Sort-Object -Unique)).Count -eq 4) 'PGR-LIVE-ROW-IDS' 'Live PGR admission requires four distinct exact row IDs.'
            Assert-Condition ($hi3Ids.Count -eq 1 -and $hi3Ids[0] -eq '10101') 'HI3-LIVE-ROW-ID' 'Live HI3 admission requires exact row ID 10101.'

            foreach ($sourceId in $inScopeSourceIds) {
                $source = @($sources | Where-Object { (Get-PropertyValue $_ 'sourceId') -eq $sourceId }) | Select-Object -First 1
                if ($null -eq $source) { continue }
                Assert-Condition (Has-Text (Get-PropertyValue $source 'sourceSnapshotId')) 'SOURCE-SNAPSHOT-ID' "Source '$sourceId' requires sourceSnapshotId."
                Assert-Condition (Has-Text (Get-PropertyValue $source 'snapshotDate')) 'SOURCE-SNAPSHOT-DATE' "Source '$sourceId' requires snapshotDate."
                Assert-Condition ((Get-PropertyValue $source 'sizeBytes') -is [ValueType] -and [int64](Get-PropertyValue $source 'sizeBytes') -gt 0) 'SOURCE-SIZE' "Source '$sourceId' requires positive sizeBytes."
                Assert-Condition (Test-Sha256Text (Get-PropertyValue $source 'sha256')) 'SOURCE-SHA256' "Source '$sourceId' requires SHA-256."
                Assert-Condition ((Get-PropertyValue $source 'hashStatus') -eq 'verified') 'SOURCE-HASH-STATUS' "Source '$sourceId' hashStatus must be verified."
                Assert-Condition ((Has-Text (Get-PropertyValue $source 'evidenceGrade')) -and (Get-PropertyValue $source 'evidenceGrade') -ne 'unassigned') 'SOURCE-EVIDENCE-GRADE' "Source '$sourceId' requires a non-unassigned evidence grade."
                Assert-Condition ((Has-Text (Get-PropertyValue $source 'licenseStatus')) -and (Get-PropertyValue $source 'licenseStatus') -ne 'pending-live-recheck') 'SOURCE-LICENSE' "Source '$sourceId' requires a resolved or explicitly unresolved license status."
                Assert-Condition (Has-Text (Get-PropertyValue $source 'sourceRecordPath')) 'SOURCE-RECORD' "Source '$sourceId' requires sourceRecordPath."
                Assert-Condition (Has-Text (Get-PropertyValue $source 'producerManifestPath')) 'SOURCE-MANIFEST' "Source '$sourceId' requires producerManifestPath."
                Assert-Condition (Has-Text (Get-PropertyValue $source 'upstreamRevision')) 'SOURCE-UPSTREAM-REVISION' "Source '$sourceId' requires upstreamRevision."
                Assert-Condition (Has-Text (Get-PropertyValue $source 'producerRevision')) 'SOURCE-PRODUCER-REVISION' "Source '$sourceId' requires producerRevision."
                Assert-Condition (Has-Text (Get-PropertyValue $source 'extractTool')) 'SOURCE-EXTRACT-TOOL' "Source '$sourceId' requires extractTool."
                Assert-Condition (Has-Text (Get-PropertyValue $source 'extractCommand')) 'SOURCE-EXTRACT-COMMAND' "Source '$sourceId' requires the retained exact extractCommand."
                $sourceKind = [string](Get-PropertyValue $source 'kind')
                if ($sourceKind -in @('report', 'derived')) {
                    $inputSourceIds = @((Get-PropertyValue $source 'inputSourceIds') | ForEach-Object { [string]$_ })
                    Assert-Condition ($inputSourceIds.Count -gt 0) 'DERIVED-INPUT-SOURCES' "Derived/report source '$sourceId' requires inputSourceIds."
                    foreach ($inputSourceId in $inputSourceIds) {
                        Assert-Condition ($inputSourceId -in $sourceIds) 'DERIVED-INPUT-SOURCE-REF' "Derived/report source '$sourceId' references missing input '$inputSourceId'."
                    }
                    Assert-Condition (Has-Text (Get-PropertyValue $source 'generatedReportPath')) 'DERIVED-REPORT-PATH' "Derived/report source '$sourceId' requires generatedReportPath."
                    Assert-Condition (Test-Sha256Text (Get-PropertyValue $source 'generatedReportSha256')) 'DERIVED-REPORT-SHA256' "Derived/report source '$sourceId' requires generatedReportSha256."
                    Assert-Condition (Has-Text (Get-PropertyValue $source 'deterministicNormalization')) 'DERIVED-NORMALIZATION' "Derived/report source '$sourceId' requires deterministicNormalization."
                }
            }

            Test-CellSet $cells $semanticSlots $allowedValueStates $allowedOwnerStates $allowedClassifications $negativeSourceIds -RequireSeventy

            foreach ($cell in @($cells | Where-Object { (Get-PropertyValue $_ 'foreignClassification') -eq 'proven-runtime' })) {
                $cellSourceId = [string](Get-PropertyValue $cell 'foreignSourceId')
                $cellEvidenceRef = [string](Get-PropertyValue $cell 'foreignEvidenceRef')
                $cellSource = @($sources | Where-Object { (Get-PropertyValue $_ 'sourceId') -eq $cellSourceId }) | Select-Object -First 1
                $runtimeRefs = if ($null -ne $cellSource) { @((Get-PropertyValue $cellSource 'runtimeTraceEvidenceRefs') | ForEach-Object { [string]$_ }) } else { @() }
                Assert-Condition ($cellEvidenceRef -in $runtimeRefs) 'CELL-RUNTIME-TRACE' "proven-runtime cell '$cellSourceId/$cellEvidenceRef' requires an exact runtimeTraceEvidenceRefs entry on its source record."
            }

            if ($pgrIds.Count -eq 4 -and $hi3Ids.Count -eq 1) {
                $expectedRows = @($pgrIds | ForEach-Object { "Id=$_" }) + @('levelId=10101')
                $actualRows = @($cells | ForEach-Object { [string](Get-PropertyValue $_ 'foreignRowOrKey') } | Sort-Object -Unique)
                Assert-Condition ((@($actualRows | Sort-Object) -join '|') -eq (@($expectedRows | Sort-Object) -join '|')) 'LIVE-ROW-KEY-SET' 'Crosswalk row keys do not match the four PGR IDs plus HI3 10101.'

                foreach ($rowKey in $expectedRows) {
                    $rowCells = @($cells | Where-Object { (Get-PropertyValue $_ 'foreignRowOrKey') -eq $rowKey })
                    Assert-Condition ($rowCells.Count -eq 14) 'LIVE-ROW-SLOT-COUNT' "Row '$rowKey' must contain exactly fourteen cells."
                    $rowSlots = @($rowCells | ForEach-Object { [string](Get-PropertyValue $_ 'semanticSlotId') } | Sort-Object)
                    Assert-Condition (($rowSlots -join '|') -eq (@($semanticSlots | Sort-Object) -join '|')) 'LIVE-ROW-SLOT-SET' "Row '$rowKey' does not contain the fixed fourteen semantic slots."
                    $rowSources = @($rowCells | ForEach-Object { [string](Get-PropertyValue $_ 'foreignSourceId') } | Sort-Object -Unique)
                    $expectedSource = if ($rowKey.StartsWith('Id=')) { [string]$pgrLiveSourceId } else { [string]$hi3LiveSourceId }
                    Assert-Condition ($rowSources.Count -eq 1 -and $rowSources[0] -eq $expectedSource) 'LIVE-ROW-SOURCE' "Row '$rowKey' is not bound to its exact live source."
                    $expectedClaim = if ($rowKey.StartsWith('Id=')) { 'PGR-STAGE-SPINE-01' } else { 'HI3-STAGE-SPINE-01' }
                    $rowClaims = @($rowCells | ForEach-Object { [string](Get-PropertyValue $_ 'claimId') } | Sort-Object -Unique)
                    Assert-Condition ($rowClaims.Count -eq 1 -and $rowClaims[0] -eq $expectedClaim) 'LIVE-ROW-CLAIM' "Row '$rowKey' is not bound to the exact in-scope claim."
                    $rowSnapshots = @($rowCells | ForEach-Object { [string](Get-PropertyValue $_ 'foreignSourceSnapshotId') } | Sort-Object -Unique)
                    Assert-Condition ($rowSnapshots.Count -eq 1) 'LIVE-ROW-SNAPSHOT' "Row '$rowKey' must use exactly one source snapshot ID."
                    $rowOrdinals = @($rowCells | ForEach-Object { Get-PropertyValue $_ 'foreignSourceOrdinal' } | Sort-Object -Unique)
                    Assert-Condition ($rowOrdinals.Count -eq 1 -and $rowOrdinals[0] -is [ValueType] -and [int]$rowOrdinals[0] -gt 0) 'LIVE-ROW-ORDINAL' "Row '$rowKey' requires one positive source ordinal."
                }
            }

            foreach ($claimId in @((Get-PropertyValue $packet 'inScopeClaimIds'))) {
                $claim = @($claims | Where-Object { (Get-PropertyValue $_ 'claimId') -eq $claimId }) | Select-Object -First 1
                Assert-Condition ($null -ne $claim -and (Get-PropertyValue $claim 'mappingStatus') -eq 'exact') 'LIVE-CLAIM-EXACT' "In-scope claim '$claimId' must be exact."
                if ($null -ne $claim) {
                    $expectedSource = if ($claimId -eq 'PGR-STAGE-SPINE-01') { [string]$pgrLiveSourceId } else { [string]$hi3LiveSourceId }
                    $mappings = @((Get-PropertyValue $claim 'sourceMappings') | Where-Object { (Get-PropertyValue $_ 'sourceId') -eq $expectedSource })
                    Assert-Condition ($mappings.Count -gt 0) 'LIVE-CLAIM-MAPPING' "Claim '$claimId' requires a mapping to its exact live source."
                    foreach ($mapping in $mappings) {
                        foreach ($name in @('sourceSnapshotId', 'sourceOrdinal', 'exactRowOrKey', 'supportedStatement', 'negativeBoundary', 'evidenceRef')) {
                            Assert-Condition (Has-Text (Get-PropertyValue $mapping $name)) 'LIVE-CLAIM-MAPPING-FIELD' "Claim '$claimId' live mapping is missing $name."
                        }
                        Assert-Condition (@((Get-PropertyValue $mapping 'fieldPaths')).Count -gt 0) 'LIVE-CLAIM-MAPPING-PATHS' "Claim '$claimId' live mapping requires fieldPaths."
                    }
                }
            }

            if (Has-Text $generatedReportPath) {
                $resolvedReportPath = Resolve-ArtifactPath ([string]$generatedReportPath)
                Assert-Condition (Test-Path -LiteralPath $resolvedReportPath -PathType Leaf) 'LIVE-REPORT-MISSING' "Generated report is missing: $resolvedReportPath"
                if (Test-Path -LiteralPath $resolvedReportPath -PathType Leaf) {
                    $actualHash = (Get-FileHash -LiteralPath $resolvedReportPath -Algorithm SHA256).Hash.ToUpperInvariant()
                    Assert-Condition ($actualHash -eq ([string]$generatedReportSha).ToUpperInvariant()) 'LIVE-REPORT-HASH' 'Generated report SHA-256 does not match the recorded digest.'
                    $report = Read-JsonFile $resolvedReportPath 'LIVE-REPORT'
                    if ($null -ne $report) {
                        Assert-Condition ((Get-PropertyValue $report 'schemaVersion') -eq 2) 'LIVE-REPORT-SCHEMA' 'Generated report schemaVersion must be 2.'
                        Assert-Condition ((Get-PropertyValue $report 'packetId') -eq $packetId) 'LIVE-REPORT-PACKET' 'Generated report packetId mismatch.'
                        $reportCells = @((Get-PropertyValue $report 'crosswalkRows'))
                        Test-CellSet $reportCells $semanticSlots $allowedValueStates $allowedOwnerStates $allowedClassifications $negativeSourceIds -RequireSeventy
                        Assert-Condition ((Get-CanonicalCellProjection $reportCells) -eq (Get-CanonicalCellProjection $cells)) 'LIVE-REPORT-CELL-DRIFT' 'Generated report crosswalkRows differ from the evidence index cells.'
                    }
                }
            }

            $pgrWarning = @($crossSnapshotWarnings | Where-Object { (Get-PropertyValue $_ 'warningId') -eq 'PGR-GUIDEFIGHT-ROWCOUNT-DRIFT-01' }) | Select-Object -First 1
            $pgrDisposition = Get-PropertyValue $pgrWarning 'liveDisposition'
            Assert-Condition ($null -ne $pgrDisposition) 'PGR-DRIFT-DISPOSITION' 'PGR live drift disposition is required.'
            if ($null -ne $pgrDisposition) {
                Assert-Condition ((Get-PropertyValue $pgrDisposition 'classificationComplete') -eq $true) 'PGR-DRIFT-COMPLETE' 'PGR drift classification must be complete.'
                Assert-Condition ((Get-PropertyValue $pgrDisposition 'snapshotsUnioned') -eq $false) 'PGR-DRIFT-NO-UNION' 'PGR snapshotsUnioned must be false.'
                Assert-Condition (@((Get-PropertyValue $pgrDisposition 'liveRowIds')).Count -eq 4) 'PGR-DRIFT-ROW-COUNT' 'PGR drift disposition requires four live row IDs.'
                Assert-Condition (Has-Text (Get-PropertyValue $pgrDisposition 'evidenceRef')) 'PGR-DRIFT-EVIDENCE' 'PGR drift disposition requires evidenceRef.'
            }

            $hi3Warning = @($crossSnapshotWarnings | Where-Object { (Get-PropertyValue $_ 'warningId') -eq 'HI3-STAGEDATA-2021-HISTORICAL-CONTROL-01' }) | Select-Object -First 1
            $hi3Observation = Get-PropertyValue $hi3Warning 'targetLiveObservation'
            Assert-Condition ($null -ne $hi3Observation) 'HI3-LIVE-RECONCILIATION' 'HI3 targetLiveObservation is required.'
            if ($null -ne $hi3Observation) {
                Assert-Condition ((Get-PropertyValue $hi3Observation 'snapshotsUnioned') -eq $false) 'HI3-LIVE-NO-UNION' 'HI3 snapshotsUnioned must be false.'
                Assert-Condition ((Get-PropertyValue $hi3Observation 'sourceValuesCopied') -eq $false) 'HI3-LIVE-NO-VALUES' 'HI3 sourceValuesCopied must be false.'
                Assert-Condition ((Get-PropertyValue $hi3Observation 'targetRowOrKey') -eq 'levelId=10101') 'HI3-LIVE-ROW-KEY' 'HI3 live reconciliation must target levelId=10101.'
                Assert-Condition (Has-Text (Get-PropertyValue $hi3Observation 'evidenceRef')) 'HI3-LIVE-EVIDENCE' 'HI3 live reconciliation requires evidenceRef.'
            }

            if ($null -ne $backlogItem) {
                $acceptance = @((Get-PropertyValue $backlogItem 'acceptance'))
                foreach ($acceptanceId in $requiredOpenIds) {
                    $row = @($acceptance | Where-Object { (Get-PropertyValue $_ 'acceptanceId') -eq $acceptanceId }) | Select-Object -First 1
                    Assert-Condition ($null -ne $row -and (Get-PropertyValue $row 'result') -eq 'pass') 'LIVE-BACKLOG-GATE' "Backlog acceptance '$acceptanceId' must pass."
                    if ($null -ne $row) {
                        Assert-Condition (@((Get-PropertyValue $row 'proofRefIds')).Count -gt 0) 'LIVE-BACKLOG-PROOF' "Backlog acceptance '$acceptanceId' requires proofRefIds."
                    }
                }
                Assert-Condition ((Get-PropertyValue $backlogItem 'lifecycleStatus') -eq 'accepted') 'LIVE-BACKLOG-STATUS' 'Live evidence item must be accepted.'
            }
        }

        $observations.packetStatus = Get-PropertyValue $packet 'status'
        $observations.inScopeSourceCount = $inScopeSourceIds.Count
        $observations.livePgrSourceId = $pgrLiveSourceId
        $observations.liveHi3SourceId = $hi3LiveSourceId
        $observations.crosswalkCellCount = $cells.Count
        $observations.distinctForeignRowCount = @($cells | ForEach-Object { [string](Get-PropertyValue $_ 'foreignRowOrKey') } | Sort-Object -Unique).Count
        $observations.generatedReportPath = $generatedReportPath
        if ($null -ne $backlogItem) {
            $observations.openRequiredAcceptanceIds = @((Get-PropertyValue $backlogItem 'acceptance') | Where-Object { (Get-PropertyValue $_ 'required') -eq $true -and (Get-PropertyValue $_ 'result') -eq 'open' } | ForEach-Object { Get-PropertyValue $_ 'acceptanceId' })
        }
    }
}

$wouldPass = $script:Failures.Count -eq 0
$status = if ($wouldPass) {
    if ($Mode -eq 'LiveAcceptance') { 'live-acceptance-pass' } else { 'readiness-pass' }
}
else {
    if ($Mode -eq 'LiveAcceptance') { 'live-acceptance-open' } else { 'readiness-fail' }
}

$result = [pscustomobject][ordered]@{
    schemaVersion = 1
    validatorId = $validatorId
    mode = $Mode
    status = $status
    wouldPass = $wouldPass
    evidenceIndexPath = [System.IO.Path]::GetFullPath($EvidenceIndexPath)
    backlogPath = [System.IO.Path]::GetFullPath($BacklogPath)
    observations = [pscustomobject]$observations
    failureCount = $script:Failures.Count
    failures = @($script:Failures)
    warningCount = $script:Warnings.Count
    warnings = @($script:Warnings)
}

$result | ConvertTo-Json -Depth 12

if (-not $ReportOnly -and -not $wouldPass) {
    if ($Mode -eq 'LiveAcceptance') { exit 2 }
    exit 1
}

exit 0
