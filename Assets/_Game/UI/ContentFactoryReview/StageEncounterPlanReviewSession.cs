using System;
using System.Collections.Generic;
using DimensionBrawl.LevelDesign;

namespace DimensionBrawl.UI.ContentFactoryReview
{
    public enum StageEncounterPlanReviewState
    {
        Ready = 0,
        WaveActive = 10,
        WaveTransition = 20,
        Completed = 30,
        Interrupted = 40
    }

    public enum StageEncounterWaveReviewStatus
    {
        Pending = 0,
        Active = 10,
        Cleared = 20,
        Interrupted = 30
    }

    public sealed class StageEncounterPlanReviewSession
    {
        private readonly StageEncounterPlanProfile.WaveDefinition[] waves;
        private readonly StageEncounterWaveReviewStatus[] waveStatuses;
        private readonly Dictionary<string, int> remainingBySpawnId =
            new(StringComparer.Ordinal);

        private int currentWaveIndex = -1;
        private int currentRemainingCombatantCount;

        public StageEncounterPlanReviewSession(StageEncounterPlanProfile profile)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            if (!profile.TryValidate(out string validationError))
            {
                throw new ArgumentException(validationError, nameof(profile));
            }

            SchemaVersion = profile.SchemaVersion;
            Revision = profile.Revision;
            PlanId = profile.PlanId;
            StageId = profile.StageId;
            EncounterId = profile.EncounterId;
            CanonicalDigest = profile.CanonicalDigest;
            AdmissionDisposition = profile.AdmissionDisposition;
            OutcomeOwner = profile.OutcomeOwner;
            RewardOwner = profile.RewardOwner;

            StageEncounterPlanProfile.EncounterDefinition encounter =
                profile.Encounter;
            waves = encounter.Waves;
            waveStatuses = new StageEncounterWaveReviewStatus[waves.Length];
            AttemptGeneration = 1;
        }

        public int SchemaVersion { get; }
        public int Revision { get; }
        public string PlanId { get; }
        public string StageId { get; }
        public string EncounterId { get; }
        public string CanonicalDigest { get; }
        public StageEncounterPlanAdmissionDisposition AdmissionDisposition { get; }
        public StageEncounterPlanOutcomeOwner OutcomeOwner { get; }
        public StageEncounterPlanRewardOwner RewardOwner { get; }
        public StageEncounterPlanReviewState State { get; private set; } =
            StageEncounterPlanReviewState.Ready;
        public int WaveCount => waves.Length;
        public int CurrentWaveIndex => currentWaveIndex;
        public string CurrentWaveId => CurrentWave?.WaveId ?? string.Empty;
        public StageEncounterPlanProfile.WaveDefinition CurrentWave =>
            currentWaveIndex >= 0 && currentWaveIndex < waves.Length
                ? CloneWave(waves[currentWaveIndex])
                : null;
        public StageEncounterWaveReviewStatus CurrentWaveStatus =>
            currentWaveIndex >= 0 && currentWaveIndex < waveStatuses.Length
                ? waveStatuses[currentWaveIndex]
                : StageEncounterWaveReviewStatus.Pending;
        public int CurrentRemainingCombatantCount => currentRemainingCombatantCount;
        public int ClearedWaveCount { get; private set; }
        public int AttemptGeneration { get; private set; }
        public int CompletionCount { get; private set; }
        public int InterruptionCount { get; private set; }
        public bool IsCompleted => State == StageEncounterPlanReviewState.Completed;
        public bool IsInterrupted => State == StageEncounterPlanReviewState.Interrupted;

        public StageEncounterPlanProfile.WaveDefinition GetWave(int index)
        {
            if (index < 0 || index >= waves.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return CloneWave(waves[index]);
        }

        public StageEncounterWaveReviewStatus GetWaveStatus(int index)
        {
            if (index < 0 || index >= waveStatuses.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return waveStatuses[index];
        }

        public StageEncounterWaveReviewStatus[] CreateWaveStatusSnapshot()
        {
            return (StageEncounterWaveReviewStatus[])waveStatuses.Clone();
        }

        public bool TryBegin()
        {
            if (State != StageEncounterPlanReviewState.Ready
                || waves.Length == 0
                || currentWaveIndex != -1)
            {
                return false;
            }

            ActivateWave(0);
            return true;
        }

        public bool TryResolveCombatant(string spawnId)
        {
            if (State != StageEncounterPlanReviewState.WaveActive
                || string.IsNullOrWhiteSpace(spawnId)
                || !remainingBySpawnId.TryGetValue(spawnId, out int remaining)
                || remaining <= 0
                || currentRemainingCombatantCount <= 0)
            {
                return false;
            }

            remainingBySpawnId[spawnId] = remaining - 1;
            currentRemainingCombatantCount--;
            if (currentRemainingCombatantCount > 0)
            {
                return true;
            }

            waveStatuses[currentWaveIndex] = StageEncounterWaveReviewStatus.Cleared;
            ClearedWaveCount++;
            if (currentWaveIndex == waves.Length - 1)
            {
                State = StageEncounterPlanReviewState.Completed;
                CompletionCount++;
            }
            else
            {
                State = StageEncounterPlanReviewState.WaveTransition;
            }

            return true;
        }

        public bool TryAdvanceWave()
        {
            if (State != StageEncounterPlanReviewState.WaveTransition
                || currentRemainingCombatantCount != 0
                || currentWaveIndex < 0
                || currentWaveIndex >= waves.Length - 1)
            {
                return false;
            }

            ActivateWave(currentWaveIndex + 1);
            return true;
        }

        public bool TryInterrupt()
        {
            if (State != StageEncounterPlanReviewState.WaveActive
                && State != StageEncounterPlanReviewState.WaveTransition)
            {
                return false;
            }

            if (State == StageEncounterPlanReviewState.WaveActive
                && currentWaveIndex >= 0)
            {
                waveStatuses[currentWaveIndex] =
                    StageEncounterWaveReviewStatus.Interrupted;
            }

            State = StageEncounterPlanReviewState.Interrupted;
            InterruptionCount++;
            return true;
        }

        public void Reset()
        {
            AttemptGeneration++;
            State = StageEncounterPlanReviewState.Ready;
            currentWaveIndex = -1;
            currentRemainingCombatantCount = 0;
            ClearedWaveCount = 0;
            remainingBySpawnId.Clear();
            Array.Clear(waveStatuses, 0, waveStatuses.Length);
        }

        public bool TryGetNextUnresolvedSpawn(
            out StageEncounterPlanProfile.SpawnDefinition spawn,
            out int remainingCount)
        {
            spawn = null;
            remainingCount = 0;
            if (State != StageEncounterPlanReviewState.WaveActive
                || currentWaveIndex < 0
                || currentWaveIndex >= waves.Length)
            {
                return false;
            }

            StageEncounterPlanProfile.WaveDefinition wave = waves[currentWaveIndex];
            for (int i = 0; i < wave.SpawnCount; i++)
            {
                StageEncounterPlanProfile.SpawnDefinition candidate = wave.GetSpawn(i);
                if (candidate != null
                    && remainingBySpawnId.TryGetValue(
                        candidate.SpawnId,
                        out int candidateRemaining)
                    && candidateRemaining > 0)
                {
                    spawn = candidate;
                    remainingCount = candidateRemaining;
                    return true;
                }
            }

            return false;
        }

        public bool TryGetRemainingCombatantCount(
            string spawnId,
            out int remainingCount)
        {
            if (!string.IsNullOrWhiteSpace(spawnId)
                && currentWaveIndex >= 0
                && remainingBySpawnId.TryGetValue(spawnId, out remainingCount))
            {
                return true;
            }

            remainingCount = 0;
            return false;
        }

        private void ActivateWave(int waveIndex)
        {
            currentWaveIndex = waveIndex;
            remainingBySpawnId.Clear();

            StageEncounterPlanProfile.WaveDefinition wave = waves[waveIndex];
            long totalCombatantCount = 0;
            for (int i = 0; i < wave.SpawnCount; i++)
            {
                StageEncounterPlanProfile.SpawnDefinition spawn = wave.GetSpawn(i);
                remainingBySpawnId.Add(spawn.SpawnId, spawn.Count);
                totalCombatantCount += spawn.Count;
            }

            currentRemainingCombatantCount = checked((int)totalCombatantCount);

            waveStatuses[waveIndex] = StageEncounterWaveReviewStatus.Active;
            State = StageEncounterPlanReviewState.WaveActive;
        }

        private static StageEncounterPlanProfile.WaveDefinition CloneWave(
            StageEncounterPlanProfile.WaveDefinition source)
        {
            return source == null
                ? null
                : new StageEncounterPlanProfile.WaveDefinition(
                    source.WaveId,
                    source.WaveIndex,
                    source.Activation,
                    source.Objective,
                    source.Spawns);
        }
    }
}
