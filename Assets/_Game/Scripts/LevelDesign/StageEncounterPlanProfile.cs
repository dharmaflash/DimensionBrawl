using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace DimensionBrawl.LevelDesign
{
    public enum StageEncounterPlanAdmissionDisposition
    {
        None = 0,
        ReviewOnlyNotAdmitted = 1
    }

    public enum StageEncounterPlanOutcomeOwner
    {
        None = 0,
        ExistingStageRun = 1
    }

    public enum StageEncounterPlanRewardOwner
    {
        None = 0,
        ExternalRewardLedger = 1
    }

    public enum StageEncounterWaveActivation
    {
        None = 0,
        EncounterStart = 1,
        PreviousWaveDefeated = 2
    }

    public enum StageEncounterObjective
    {
        None = 0,
        DefeatAll = 1
    }

    [CreateAssetMenu(
        menuName = "DimensionBrawl/Profiles/Stage Encounter Plan Profile",
        fileName = "DB_StageEncounterPlan")]
    public sealed class StageEncounterPlanProfile : ScriptableObject
    {
        [Serializable]
        public sealed class SpawnDefinition
        {
            [SerializeField] private string spawnId = string.Empty;
            [SerializeField] private string payloadId = string.Empty;
            [SerializeField] private string anchorId = string.Empty;
            [SerializeField, Min(1)] private int count = 1;
            [SerializeField, Min(0f)] private float delaySeconds;

            public SpawnDefinition()
            {
            }

            public SpawnDefinition(
                string spawnId,
                string payloadId,
                string anchorId,
                int count,
                float delaySeconds)
            {
                Configure(spawnId, payloadId, anchorId, count, delaySeconds);
            }

            public string SpawnId => spawnId;
            public string PayloadId => payloadId;
            public string AnchorId => anchorId;
            public int Count => count;
            public float DelaySeconds => delaySeconds;

            public void Configure(
                string newSpawnId,
                string newPayloadId,
                string newAnchorId,
                int newCount,
                float newDelaySeconds)
            {
                spawnId = newSpawnId ?? string.Empty;
                payloadId = newPayloadId ?? string.Empty;
                anchorId = newAnchorId ?? string.Empty;
                count = newCount;
                delaySeconds = newDelaySeconds;
            }

            internal SpawnDefinition DeepCopy()
            {
                return new SpawnDefinition(
                    SpawnId,
                    PayloadId,
                    AnchorId,
                    Count,
                    DelaySeconds);
            }
        }

        [Serializable]
        public sealed class WaveDefinition
        {
            [SerializeField] private string waveId = string.Empty;
            [SerializeField, Min(0)] private int waveIndex;
            [SerializeField] private StageEncounterWaveActivation activation;
            [SerializeField] private StageEncounterObjective objective;
            [SerializeField] private SpawnDefinition[] spawns = Array.Empty<SpawnDefinition>();

            public WaveDefinition()
            {
            }

            public WaveDefinition(
                string waveId,
                int waveIndex,
                StageEncounterWaveActivation activation,
                StageEncounterObjective objective,
                SpawnDefinition[] spawns)
            {
                Configure(waveId, waveIndex, activation, objective, spawns);
            }

            public string WaveId => waveId;
            public int WaveIndex => waveIndex;
            public StageEncounterWaveActivation Activation => activation;
            public StageEncounterObjective Objective => objective;
            public int SpawnCount => spawns?.Length ?? 0;
            public int TotalCombatantCount
            {
                get
                {
                    long total = 0;
                    SpawnDefinition[] resolved = spawns ?? Array.Empty<SpawnDefinition>();
                    for (int i = 0; i < resolved.Length; i++)
                    {
                        if (resolved[i] != null)
                        {
                            total += resolved[i].Count;
                        }
                    }

                    return checked((int)total);
                }
            }

            public SpawnDefinition[] Spawns => CreateSpawnSnapshot();

            public void Configure(
                string newWaveId,
                int newWaveIndex,
                StageEncounterWaveActivation newActivation,
                StageEncounterObjective newObjective,
                SpawnDefinition[] newSpawns)
            {
                waveId = newWaveId ?? string.Empty;
                waveIndex = newWaveIndex;
                activation = newActivation;
                objective = newObjective;
                spawns = CloneSpawns(newSpawns);
            }

            public SpawnDefinition GetSpawn(int index)
            {
                if (index < 0 || index >= SpawnCount)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                return spawns[index]?.DeepCopy();
            }

            internal SpawnDefinition[] CreateSpawnSnapshot()
            {
                return CloneSpawns(spawns);
            }

            internal WaveDefinition DeepCopy()
            {
                return new WaveDefinition(
                    WaveId,
                    WaveIndex,
                    Activation,
                    Objective,
                    spawns);
            }

            private static SpawnDefinition[] CloneSpawns(SpawnDefinition[] source)
            {
                if (source == null || source.Length == 0)
                {
                    return Array.Empty<SpawnDefinition>();
                }

                var clone = new SpawnDefinition[source.Length];
                for (int i = 0; i < source.Length; i++)
                {
                    clone[i] = source[i]?.DeepCopy();
                }

                return clone;
            }
        }

        [Serializable]
        public sealed class EncounterDefinition
        {
            [SerializeField] private string encounterId = string.Empty;
            [SerializeField] private WaveDefinition[] waves = Array.Empty<WaveDefinition>();

            public EncounterDefinition()
            {
            }

            public EncounterDefinition(string encounterId, WaveDefinition[] waves)
            {
                Configure(encounterId, waves);
            }

            public string EncounterId => encounterId;
            public int WaveCount => waves?.Length ?? 0;
            public WaveDefinition[] Waves => CreateWaveSnapshot();

            public void Configure(string newEncounterId, WaveDefinition[] newWaves)
            {
                encounterId = newEncounterId ?? string.Empty;
                waves = CloneWaves(newWaves);
            }

            public WaveDefinition GetWave(int index)
            {
                if (index < 0 || index >= WaveCount)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                return waves[index]?.DeepCopy();
            }

            internal WaveDefinition[] CreateWaveSnapshot()
            {
                return CloneWaves(waves);
            }

            internal EncounterDefinition DeepCopy()
            {
                return new EncounterDefinition(EncounterId, waves);
            }

            private static WaveDefinition[] CloneWaves(WaveDefinition[] source)
            {
                if (source == null || source.Length == 0)
                {
                    return Array.Empty<WaveDefinition>();
                }

                var clone = new WaveDefinition[source.Length];
                for (int i = 0; i < source.Length; i++)
                {
                    clone[i] = source[i]?.DeepCopy();
                }

                return clone;
            }
        }

        [Header("Canonical Identity")]
        [SerializeField, Min(1)] private int schemaVersion = 1;
        [SerializeField, Min(1)] private int revision = 1;
        [SerializeField] private string planId = string.Empty;
        [SerializeField] private string stageId = string.Empty;
        [SerializeField] private string canonicalDigest = string.Empty;

        [Header("Ownership Boundary")]
        [SerializeField]
        private StageEncounterPlanAdmissionDisposition admissionDisposition;
        [SerializeField] private StageEncounterPlanOutcomeOwner outcomeOwner;
        [SerializeField] private StageEncounterPlanRewardOwner rewardOwner;

        [Header("Stage Encounter")]
        [SerializeField] private EncounterDefinition encounter;

        public int SchemaVersion => schemaVersion;
        public int Revision => revision;
        public string PlanId => planId;
        public string StageId => stageId;
        public string CanonicalDigest => canonicalDigest;
        public StageEncounterPlanAdmissionDisposition AdmissionDisposition =>
            admissionDisposition;
        public StageEncounterPlanOutcomeOwner OutcomeOwner => outcomeOwner;
        public StageEncounterPlanRewardOwner RewardOwner => rewardOwner;
        public string EncounterId => encounter?.EncounterId ?? string.Empty;
        public int WaveCount => encounter?.WaveCount ?? 0;
        public EncounterDefinition Encounter => encounter?.DeepCopy();

        public void Configure(
            int newSchemaVersion,
            int newRevision,
            string newPlanId,
            string newStageId,
            StageEncounterPlanAdmissionDisposition newAdmissionDisposition,
            StageEncounterPlanOutcomeOwner newOutcomeOwner,
            StageEncounterPlanRewardOwner newRewardOwner,
            EncounterDefinition newEncounter)
        {
            schemaVersion = newSchemaVersion;
            revision = newRevision;
            planId = newPlanId ?? string.Empty;
            stageId = newStageId ?? string.Empty;
            admissionDisposition = newAdmissionDisposition;
            outcomeOwner = newOutcomeOwner;
            rewardOwner = newRewardOwner;
            encounter = newEncounter?.DeepCopy();
            canonicalDigest = ComputeCanonicalDigest();
        }

        public WaveDefinition GetWave(int index)
        {
            if (encounter == null)
            {
                throw new InvalidOperationException("The encounter definition is missing.");
            }

            return encounter.GetWave(index);
        }

        public bool TryValidate(out string error)
        {
            var issues = new List<string>();
            CollectValidationIssues(issues);
            error = issues.Count > 0 ? string.Join("\n", issues) : string.Empty;
            return issues.Count == 0;
        }

        public void CollectValidationIssues(List<string> issues)
        {
            if (issues == null)
            {
                return;
            }

            string label = string.IsNullOrWhiteSpace(name)
                ? nameof(StageEncounterPlanProfile)
                : name;
            if (schemaVersion <= 0)
            {
                issues.Add($"{label}: schema version must be positive.");
            }

            if (revision <= 0)
            {
                issues.Add($"{label}: revision must be positive.");
            }

            ValidateStableId(issues, label, "plan id", planId);
            ValidateStableId(issues, label, "stage id", stageId);

            if (admissionDisposition
                != StageEncounterPlanAdmissionDisposition.ReviewOnlyNotAdmitted)
            {
                issues.Add(
                    $"{label}: admission must be ReviewOnlyNotAdmitted for CF-01.");
            }

            if (outcomeOwner != StageEncounterPlanOutcomeOwner.ExistingStageRun)
            {
                issues.Add($"{label}: outcome owner must be ExistingStageRun.");
            }

            if (rewardOwner != StageEncounterPlanRewardOwner.ExternalRewardLedger)
            {
                issues.Add($"{label}: reward owner must be ExternalRewardLedger.");
            }

            if (encounter == null)
            {
                issues.Add($"{label}: encounter definition is missing.");
            }
            else
            {
                ValidateEncounter(issues, label, encounter);
            }

            string computedDigest = ComputeCanonicalDigest();
            if (!IsLowercaseSha256(canonicalDigest))
            {
                issues.Add($"{label}: canonical digest must be a lowercase SHA-256 value.");
            }
            else if (!string.Equals(
                         canonicalDigest,
                         computedDigest,
                         StringComparison.Ordinal))
            {
                issues.Add($"{label}: canonical digest does not match the authored plan.");
            }
        }

        public string ComputeCanonicalDigest()
        {
            var builder = new StringBuilder(2048);
            StageCanonicalDigest.Append(builder, "encounterPlan.schemaVersion", schemaVersion);
            StageCanonicalDigest.Append(builder, "encounterPlan.revision", revision);
            StageCanonicalDigest.Append(builder, "encounterPlan.planId", planId);
            StageCanonicalDigest.Append(builder, "encounterPlan.stageId", stageId);
            StageCanonicalDigest.Append(
                builder,
                "encounterPlan.admissionDisposition",
                (int)admissionDisposition);
            StageCanonicalDigest.Append(
                builder,
                "encounterPlan.outcomeOwner",
                (int)outcomeOwner);
            StageCanonicalDigest.Append(
                builder,
                "encounterPlan.rewardOwner",
                (int)rewardOwner);
            StageCanonicalDigest.Append(
                builder,
                "encounterPlan.encounterId",
                encounter?.EncounterId);
            int waveCount = encounter?.WaveCount ?? 0;
            StageCanonicalDigest.Append(builder, "encounterPlan.waveCount", waveCount);
            for (int waveIndex = 0; waveIndex < waveCount; waveIndex++)
            {
                WaveDefinition wave = encounter.GetWave(waveIndex);
                string wavePrefix = $"encounterPlan.wave[{waveIndex}]";
                StageCanonicalDigest.Append(builder, wavePrefix + ".waveId", wave?.WaveId);
                StageCanonicalDigest.Append(
                    builder,
                    wavePrefix + ".waveIndex",
                    wave?.WaveIndex ?? -1);
                StageCanonicalDigest.Append(
                    builder,
                    wavePrefix + ".activation",
                    wave != null ? (int)wave.Activation : 0);
                StageCanonicalDigest.Append(
                    builder,
                    wavePrefix + ".objective",
                    wave != null ? (int)wave.Objective : 0);
                int spawnCount = wave?.SpawnCount ?? 0;
                StageCanonicalDigest.Append(builder, wavePrefix + ".spawnCount", spawnCount);
                for (int spawnIndex = 0; spawnIndex < spawnCount; spawnIndex++)
                {
                    SpawnDefinition spawn = wave.GetSpawn(spawnIndex);
                    string spawnPrefix = $"{wavePrefix}.spawn[{spawnIndex}]";
                    StageCanonicalDigest.Append(
                        builder,
                        spawnPrefix + ".spawnId",
                        spawn?.SpawnId);
                    StageCanonicalDigest.Append(
                        builder,
                        spawnPrefix + ".payloadId",
                        spawn?.PayloadId);
                    StageCanonicalDigest.Append(
                        builder,
                        spawnPrefix + ".anchorId",
                        spawn?.AnchorId);
                    StageCanonicalDigest.Append(
                        builder,
                        spawnPrefix + ".count",
                        spawn?.Count ?? 0);
                    StageCanonicalDigest.Append(
                        builder,
                        spawnPrefix + ".delaySeconds",
                        spawn != null
                            ? spawn.DelaySeconds.ToString("R", CultureInfo.InvariantCulture)
                            : string.Empty);
                }
            }

            return StageCanonicalDigest.Compute(builder.ToString());
        }

        internal EncounterDefinition CreateEncounterSnapshot()
        {
            return encounter?.DeepCopy();
        }

        private static void ValidateEncounter(
            List<string> issues,
            string label,
            EncounterDefinition resolvedEncounter)
        {
            ValidateStableId(
                issues,
                label,
                "encounter id",
                resolvedEncounter.EncounterId);
            if (resolvedEncounter.WaveCount < 2)
            {
                issues.Add($"{label}: the encounter must contain at least two waves.");
            }

            var waveIds = new HashSet<string>(StringComparer.Ordinal);
            var spawnIds = new HashSet<string>(StringComparer.Ordinal);
            for (int waveIndex = 0; waveIndex < resolvedEncounter.WaveCount; waveIndex++)
            {
                WaveDefinition wave = resolvedEncounter.GetWave(waveIndex);
                if (wave == null)
                {
                    issues.Add($"{label}: wave {waveIndex} is null.");
                    continue;
                }

                ValidateStableId(issues, label, $"wave {waveIndex} id", wave.WaveId);
                if (!string.IsNullOrEmpty(wave.WaveId) && !waveIds.Add(wave.WaveId))
                {
                    issues.Add($"{label}: wave id '{wave.WaveId}' is duplicated.");
                }

                if (wave.WaveIndex != waveIndex)
                {
                    issues.Add(
                        $"{label}: wave indices must be contiguous from zero; position {waveIndex} declares {wave.WaveIndex}.");
                }

                StageEncounterWaveActivation expectedActivation = waveIndex == 0
                    ? StageEncounterWaveActivation.EncounterStart
                    : StageEncounterWaveActivation.PreviousWaveDefeated;
                if (wave.Activation != expectedActivation)
                {
                    issues.Add(
                        $"{label}: wave {waveIndex} activation must be {expectedActivation}.");
                }

                if (wave.Objective != StageEncounterObjective.DefeatAll)
                {
                    issues.Add($"{label}: wave {waveIndex} objective must be DefeatAll.");
                }

                if (wave.SpawnCount < 1)
                {
                    issues.Add($"{label}: wave {waveIndex} must contain at least one spawn.");
                    continue;
                }

                long waveCombatantCount = 0;
                for (int spawnIndex = 0; spawnIndex < wave.SpawnCount; spawnIndex++)
                {
                    SpawnDefinition spawn = wave.GetSpawn(spawnIndex);
                    if (spawn == null)
                    {
                        issues.Add(
                            $"{label}: wave {waveIndex} spawn {spawnIndex} is null.");
                        continue;
                    }

                    ValidateStableId(
                        issues,
                        label,
                        $"wave {waveIndex} spawn {spawnIndex} id",
                        spawn.SpawnId);
                    if (!string.IsNullOrEmpty(spawn.SpawnId)
                        && !spawnIds.Add(spawn.SpawnId))
                    {
                        issues.Add($"{label}: spawn id '{spawn.SpawnId}' is duplicated.");
                    }

                    ValidateStableId(
                        issues,
                        label,
                        $"spawn '{spawn.SpawnId}' payload id",
                        spawn.PayloadId);
                    ValidateStableId(
                        issues,
                        label,
                        $"spawn '{spawn.SpawnId}' anchor id",
                        spawn.AnchorId);
                    if (spawn.Count <= 0)
                    {
                        issues.Add($"{label}: spawn '{spawn.SpawnId}' count must be positive.");
                    }
                    else
                    {
                        waveCombatantCount += spawn.Count;
                    }

                    if (float.IsNaN(spawn.DelaySeconds)
                        || float.IsInfinity(spawn.DelaySeconds)
                        || spawn.DelaySeconds < 0f)
                    {
                        issues.Add(
                            $"{label}: spawn '{spawn.SpawnId}' delay must be finite and nonnegative.");
                    }
                }

                if (waveCombatantCount > int.MaxValue)
                {
                    issues.Add(
                        $"{label}: wave {waveIndex} total combatant count must not exceed {int.MaxValue}.");
                }
            }
        }

        private static void ValidateStableId(
            List<string> issues,
            string label,
            string fieldLabel,
            string value)
        {
            if (!IsStableId(value))
            {
                issues.Add(
                    $"{label}: {fieldLabel} '{value ?? string.Empty}' is not a stable id.");
            }
        }

        private static bool IsStableId(string value)
        {
            if (string.IsNullOrWhiteSpace(value)
                || !string.Equals(value, value.Trim(), StringComparison.Ordinal))
            {
                return false;
            }

            for (int i = 0; i < value.Length; i++)
            {
                char character = value[i];
                if (!char.IsLetterOrDigit(character)
                    && character != '.'
                    && character != '-'
                    && character != '_'
                    && character != ':'
                    && character != '/')
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsLowercaseSha256(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length != 64)
            {
                return false;
            }

            for (int i = 0; i < value.Length; i++)
            {
                char character = value[i];
                if ((character < '0' || character > '9')
                    && (character < 'a' || character > 'f'))
                {
                    return false;
                }
            }

            return true;
        }

        private void OnValidate()
        {
            if (encounter == null)
            {
                canonicalDigest = string.Empty;
            }
        }
    }
}
