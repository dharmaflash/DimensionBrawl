using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using DimensionBrawl.Combat;

namespace DimensionBrawl.LevelDesign
{
    public static class StageRunFactVocabulary
    {
        public const string OlympusCorridorTutorialPlanId = "olympus.corridor.core-tutorial";
        public const int OlympusCorridorTutorialPlanRevision = 1;

        public const string SummonPressureBlockProofId = "summon.pressure_block";
        public const string SummonFollowupHitProofId = "summon.followup_hit";
        public const string SummonCounterRecoveryProofId = "summon.counter_recovery";
        public const string SurvivalNoPlayerDownProofId = "survival.no_player_down";
        public const string MovementForwardRiskTimeProofId = "movement.forward_risk_time";

        public const string SummonPressureScreenSourceKind = "summon_pressure_screen";
        public const string BossFollowupConfirmationSourceKind = "boss_followup_confirmation";
        public const string CounterWaveStabilizationSourceKind = "counter_wave_stabilization";
        public const string TerminalSubjectSnapshotSourceKind = "terminal_subject_snapshot";
        public const string ForwardRiskClockSourceKind = "forward_risk_clock";

        public static string OlympusCorridorTutorialPlanSemanticDigest { get; } =
            ComputeOlympusCorridorTutorialPlanSemanticDigest();

        private static string ComputeOlympusCorridorTutorialPlanSemanticDigest()
        {
            StringBuilder builder = new(512);
            StageCanonicalDigest.Append(builder, "tutorialPlan.id", OlympusCorridorTutorialPlanId);
            StageCanonicalDigest.Append(builder, "tutorialPlan.revision", OlympusCorridorTutorialPlanRevision);
            StageCanonicalDigest.Append(builder, "tutorialPlan.step[0]", "soldier_challenge");
            StageCanonicalDigest.Append(builder, "tutorialPlan.step[1]", "melee");
            StageCanonicalDigest.Append(builder, "tutorialPlan.step[2]", "move");
            StageCanonicalDigest.Append(builder, "tutorialPlan.step[3]", "swap_to_ranged");
            StageCanonicalDigest.Append(builder, "tutorialPlan.step[4]", "fire");
            StageCanonicalDigest.Append(builder, "tutorialPlan.step[5]", "dodge");
            StageCanonicalDigest.Append(builder, "tutorialPlan.step[6]", "clear_targets");
            StageCanonicalDigest.Append(builder, "tutorialPlan.completion", "all_steps_committed");
            return StageCanonicalDigest.Compute(builder.ToString());
        }
    }

    public enum StageSceneSegmentExitReason
    {
        None = 0,
        Completed = 1
    }

    public enum StageTutorialRouteState
    {
        Completed = 1
    }

    public enum StageTutorialTerminationReason
    {
        Completed = 1
    }

    public enum StageTutorialRouteProofDisposition
    {
        NoProof = 0
    }

    public enum StageTutorialFactCoverageKind
    {
        LegacyOpaque = 0
    }

    public enum StageTutorialResultExpectation
    {
        NoResultExpected = 0
    }

    public enum StageOutcomeDisposition
    {
        Clear = 1,
        Fail = 2
    }

    public enum StageClearReason
    {
        None = 0,
        BossTerminal = 1,
        SimultaneousTerminalClear = 2
    }

    public enum StageFailureReason
    {
        None = 0,
        PlayerDefeated = 1
    }

    public sealed class StageSceneSegmentResult
    {
        internal StageSceneSegmentResult(
            string segmentId,
            int segmentSequenceIndex,
            bool entered,
            bool completed,
            StageSceneSegmentExitReason exitReason,
            long activeElapsedMilliseconds)
        {
            SegmentId = segmentId ?? string.Empty;
            SegmentSequenceIndex = segmentSequenceIndex;
            Entered = entered;
            Completed = completed;
            ExitReason = exitReason;
            ActiveElapsedMilliseconds = Math.Max(0L, activeElapsedMilliseconds);
            CanonicalDigest = ComputeCanonicalDigest();
        }

        public string SegmentId { get; }
        public int SegmentSequenceIndex { get; }
        public bool Entered { get; }
        public bool Completed { get; }
        public StageSceneSegmentExitReason ExitReason { get; }
        public long ActiveElapsedMilliseconds { get; }
        public string CanonicalDigest { get; }

        private string ComputeCanonicalDigest()
        {
            StringBuilder builder = new(384);
            StageCanonicalDigest.Append(builder, "segment.id", SegmentId);
            StageCanonicalDigest.Append(builder, "segment.sequenceIndex", SegmentSequenceIndex);
            StageCanonicalDigest.Append(builder, "segment.entered", Entered);
            StageCanonicalDigest.Append(builder, "segment.completed", Completed);
            StageCanonicalDigest.Append(builder, "segment.exitReason", (int)ExitReason);
            StageCanonicalDigest.Append(builder, "segment.activeElapsedMilliseconds", ActiveElapsedMilliseconds);
            return StageCanonicalDigest.Compute(builder.ToString());
        }
    }

    public sealed class StageTutorialRouteSummaryFact
    {
        private static readonly string[] LegacyLessonIds =
        {
            "soldier_challenge",
            "melee",
            "move",
            "swap_to_ranged",
            "fire",
            "dodge",
            "clear_targets"
        };

        private readonly StageTutorialFactCoverage[] coverage;

        internal StageTutorialRouteSummaryFact(
            StageRunIdentity identity,
            string segmentId,
            long observationElapsedMilliseconds)
        {
            SchemaVersion = 1;
            TutorialFactId = $"{identity.RunId}:tutorial-route-summary:1";
            PlanId = StageRunFactVocabulary.OlympusCorridorTutorialPlanId;
            PlanSemanticDigest = StageRunFactVocabulary.OlympusCorridorTutorialPlanSemanticDigest;
            RouteState = StageTutorialRouteState.Completed;
            TerminationReason = StageTutorialTerminationReason.Completed;
            RouteProofDisposition = StageTutorialRouteProofDisposition.NoProof;
            RouteProofAbsenceReason = "legacy_route_summary_has_no_typed_lesson_proof";
            ObservationElapsedMilliseconds = Math.Max(0L, observationElapsedMilliseconds);
            SegmentId = segmentId ?? string.Empty;
            coverage = new StageTutorialFactCoverage[LegacyLessonIds.Length];
            for (int i = 0; i < coverage.Length; i++)
            {
                coverage[i] = new StageTutorialFactCoverage(i, LegacyLessonIds[i]);
            }

            CoverageDigest = ComputeCoverageDigest();
            CanonicalDigest = ComputeCanonicalDigest();
        }

        public int SchemaVersion { get; }
        public string TutorialFactId { get; }
        public string PlanId { get; }
        public string PlanSemanticDigest { get; }
        public StageTutorialRouteState RouteState { get; }
        public StageTutorialTerminationReason TerminationReason { get; }
        public StageTutorialRouteProofDisposition RouteProofDisposition { get; }
        public string RouteProofAbsenceReason { get; }
        public long ObservationElapsedMilliseconds { get; }
        public string SegmentId { get; }
        public int CoverageCount => coverage.Length;
        public string CoverageDigest { get; }
        public string CanonicalDigest { get; }

        public StageTutorialFactCoverage GetCoverage(int index)
        {
            if (index < 0 || index >= coverage.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return coverage[index];
        }

        private string ComputeCoverageDigest()
        {
            StringBuilder builder = new(512);
            StageCanonicalDigest.Append(builder, "tutorialCoverage.count", coverage.Length);
            for (int i = 0; i < coverage.Length; i++)
            {
                StageCanonicalDigest.Append(
                    builder,
                    $"tutorialCoverage[{i}].digest",
                    coverage[i].CanonicalDigest);
            }

            return StageCanonicalDigest.Compute(builder.ToString());
        }

        private string ComputeCanonicalDigest()
        {
            StringBuilder builder = new(640);
            StageCanonicalDigest.Append(builder, "tutorialFact.schemaVersion", SchemaVersion);
            StageCanonicalDigest.Append(builder, "tutorialFact.id", TutorialFactId);
            StageCanonicalDigest.Append(builder, "tutorialFact.payloadKind", "tutorial_route_summary");
            StageCanonicalDigest.Append(builder, "tutorialFact.planId", PlanId);
            StageCanonicalDigest.Append(builder, "tutorialFact.planSemanticDigest", PlanSemanticDigest);
            StageCanonicalDigest.Append(builder, "tutorialFact.routeState", (int)RouteState);
            StageCanonicalDigest.Append(builder, "tutorialFact.terminationReason", (int)TerminationReason);
            StageCanonicalDigest.Append(builder, "tutorialFact.proofDisposition", (int)RouteProofDisposition);
            StageCanonicalDigest.Append(builder, "tutorialFact.proofAbsenceReason", RouteProofAbsenceReason);
            StageCanonicalDigest.Append(builder, "tutorialFact.proofId", string.Empty);
            StageCanonicalDigest.Append(builder, "tutorialFact.proofValue", string.Empty);
            StageCanonicalDigest.Append(builder, "tutorialFact.proofSourceRecordId", string.Empty);
            StageCanonicalDigest.Append(builder, "tutorialFact.proofSourceDigest", string.Empty);
            StageCanonicalDigest.Append(
                builder,
                "tutorialFact.observationElapsedMilliseconds",
                ObservationElapsedMilliseconds);
            StageCanonicalDigest.Append(builder, "tutorialFact.segmentId", SegmentId);
            StageCanonicalDigest.Append(builder, "tutorialFact.coverageCount", coverage.Length);
            StageCanonicalDigest.Append(builder, "tutorialFact.coverageDigest", CoverageDigest);
            StageCanonicalDigest.Append(builder, "tutorialFact.lessonAttemptFields", "typed_absence");
            return StageCanonicalDigest.Compute(builder.ToString());
        }
    }

    public sealed class StageTutorialFactCoverage
    {
        internal StageTutorialFactCoverage(int planOrdinal, string lessonId)
        {
            PlanOrdinal = planOrdinal;
            LessonId = lessonId ?? string.Empty;
            CoverageKind = StageTutorialFactCoverageKind.LegacyOpaque;
            ResultExpectation = StageTutorialResultExpectation.NoResultExpected;
            CanonicalDigest = ComputeCanonicalDigest();
        }

        public int PlanOrdinal { get; }
        public string LessonId { get; }
        public StageTutorialFactCoverageKind CoverageKind { get; }
        public StageTutorialResultExpectation ResultExpectation { get; }
        public string CanonicalDigest { get; }

        private string ComputeCanonicalDigest()
        {
            StringBuilder builder = new(320);
            StageCanonicalDigest.Append(builder, "tutorialCoverage.planOrdinal", PlanOrdinal);
            StageCanonicalDigest.Append(builder, "tutorialCoverage.lessonId", LessonId);
            StageCanonicalDigest.Append(builder, "tutorialCoverage.kind", (int)CoverageKind);
            StageCanonicalDigest.Append(builder, "tutorialCoverage.resultExpectation", (int)ResultExpectation);
            StageCanonicalDigest.Append(builder, "tutorialCoverage.attemptCount", 0);
            return StageCanonicalDigest.Compute(builder.ToString());
        }
    }

    public sealed class StageRunSummonUseFact
    {
        internal StageRunSummonUseFact(
            long summonAdmissionSequence,
            string slotRoleId,
            int spentTier,
            long segmentElapsedMilliseconds)
        {
            SummonAdmissionSequence = summonAdmissionSequence;
            SlotRoleId = slotRoleId ?? string.Empty;
            SpentTier = spentTier;
            SegmentElapsedMilliseconds = Math.Max(0L, segmentElapsedMilliseconds);
            CanonicalDigest = ComputeCanonicalDigest();
        }

        public long SummonAdmissionSequence { get; }
        public string SlotRoleId { get; }
        public int SpentTier { get; }
        public long SegmentElapsedMilliseconds { get; }
        public string CanonicalDigest { get; }

        private string ComputeCanonicalDigest()
        {
            StringBuilder builder = new(320);
            StageCanonicalDigest.Append(builder, "summon.sequence", SummonAdmissionSequence);
            StageCanonicalDigest.Append(builder, "summon.slotRoleId", SlotRoleId);
            StageCanonicalDigest.Append(builder, "summon.spentTier", SpentTier);
            StageCanonicalDigest.Append(
                builder,
                "summon.segmentElapsedMilliseconds",
                SegmentElapsedMilliseconds);
            return StageCanonicalDigest.Compute(builder.ToString());
        }
    }

    public sealed class StageRunSemanticProofFact
    {
        internal StageRunSemanticProofFact(
            string proofId,
            string sourceKind,
            int count,
            double actualValue,
            long firstObservedSegmentMilliseconds,
            bool qualified)
        {
            ProofId = proofId ?? string.Empty;
            SourceKind = sourceKind ?? string.Empty;
            Count = Math.Max(0, count);
            ActualValue = Math.Max(0d, actualValue);
            FirstObservedSegmentMilliseconds = Math.Max(0L, firstObservedSegmentMilliseconds);
            Qualified = qualified;
            CanonicalDigest = ComputeCanonicalDigest();
        }

        public string ProofId { get; }
        public string SourceKind { get; }
        public int Count { get; }
        public double ActualValue { get; }
        public long FirstObservedSegmentMilliseconds { get; }
        public bool Qualified { get; }
        public string CanonicalDigest { get; }

        private string ComputeCanonicalDigest()
        {
            StringBuilder builder = new(384);
            StageCanonicalDigest.Append(builder, "proof.id", ProofId);
            StageCanonicalDigest.Append(builder, "proof.sourceKind", SourceKind);
            StageCanonicalDigest.Append(builder, "proof.count", Count);
            StageCanonicalDigest.Append(
                builder,
                "proof.actualValue",
                ActualValue.ToString("R", CultureInfo.InvariantCulture));
            StageCanonicalDigest.Append(
                builder,
                "proof.firstObservedSegmentMilliseconds",
                FirstObservedSegmentMilliseconds);
            StageCanonicalDigest.Append(builder, "proof.qualified", Qualified);
            return StageCanonicalDigest.Compute(builder.ToString());
        }
    }

    public sealed class StageRunCombatFacts
    {
        private readonly StageRunSummonUseFact[] summonUses;

        internal StageRunCombatFacts(
            double playerDamageTaken,
            int playerDownCount,
            int perfectDodgeCount,
            StageRunSummonUseFact[] summonUses,
            long forwardRiskElapsedMilliseconds)
        {
            PlayerDamageTaken = Math.Max(0d, playerDamageTaken);
            PlayerDownCount = Math.Max(0, playerDownCount);
            PerfectDodgeCount = Math.Max(0, perfectDodgeCount);
            this.summonUses = summonUses ?? Array.Empty<StageRunSummonUseFact>();
            HasForwardRiskElapsed = forwardRiskElapsedMilliseconds > 0;
            ForwardRiskElapsedMilliseconds = Math.Max(0L, forwardRiskElapsedMilliseconds);
            HasStructureBreakCount = false;
            CanonicalDigest = ComputeCanonicalDigest();
        }

        public double PlayerDamageTaken { get; }
        public int PlayerDownCount { get; }
        public int PerfectDodgeCount { get; }
        public int SummonUseCount => summonUses.Length;
        public bool HasForwardRiskElapsed { get; }
        public long ForwardRiskElapsedMilliseconds { get; }
        public bool HasStructureBreakCount { get; }
        public string CanonicalDigest { get; }

        public StageRunSummonUseFact GetSummonUse(int index)
        {
            if (index < 0 || index >= summonUses.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return summonUses[index];
        }

        private string ComputeCanonicalDigest()
        {
            StringBuilder builder = new(768);
            StageCanonicalDigest.Append(
                builder,
                "combat.playerDamageTaken",
                PlayerDamageTaken.ToString("R", CultureInfo.InvariantCulture));
            StageCanonicalDigest.Append(builder, "combat.playerDownCount", PlayerDownCount);
            StageCanonicalDigest.Append(builder, "combat.perfectDodgeCount", PerfectDodgeCount);
            StageCanonicalDigest.Append(builder, "combat.summonUseCount", summonUses.Length);
            for (int i = 0; i < summonUses.Length; i++)
            {
                StageCanonicalDigest.Append(builder, $"combat.summonUse[{i}].digest", summonUses[i].CanonicalDigest);
            }

            StageCanonicalDigest.Append(builder, "combat.hasForwardRiskElapsed", HasForwardRiskElapsed);
            StageCanonicalDigest.Append(
                builder,
                "combat.forwardRiskElapsedMilliseconds",
                ForwardRiskElapsedMilliseconds);
            StageCanonicalDigest.Append(builder, "combat.hasStructureBreakCount", HasStructureBreakCount);
            StageCanonicalDigest.Append(builder, "combat.structureBreakCount", string.Empty);
            return StageCanonicalDigest.Compute(builder.ToString());
        }
    }

    public sealed class StageOutcomeFact
    {
        internal StageOutcomeFact(
            StageRunIdentity identity,
            EncounterTerminalResolution resolution,
            string outcomeSegmentId,
            long totalActiveElapsedMilliseconds,
            long combatActiveElapsedMilliseconds,
            long outcomeFactsSealedAtSequence)
        {
            SchemaVersion = 1;
            StageOutcomeFactId = $"{identity.RunId}:stage-outcome:1";
            OutcomeDisposition = resolution.Outcome == EncounterTerminalOutcome.Clear
                ? StageOutcomeDisposition.Clear
                : StageOutcomeDisposition.Fail;
            ClearReason = resolution.Reason switch
            {
                EncounterTerminalReason.BossTerminal => StageClearReason.BossTerminal,
                EncounterTerminalReason.SimultaneousTerminalClear => StageClearReason.SimultaneousTerminalClear,
                _ => StageClearReason.None
            };
            FailureReason = resolution.Reason == EncounterTerminalReason.PlayerTerminal
                ? StageFailureReason.PlayerDefeated
                : StageFailureReason.None;
            OutcomeSegmentId = outcomeSegmentId ?? string.Empty;
            RootAdmissionSequence = resolution.RootAdmissionSequence;
            TerminalEpochSequence = resolution.Epoch;
            TotalActiveElapsedMilliseconds = Math.Max(0L, totalActiveElapsedMilliseconds);
            CombatActiveElapsedMilliseconds = Math.Max(0L, combatActiveElapsedMilliseconds);
            OutcomeFactsSealedAtSequence = outcomeFactsSealedAtSequence;
            CanonicalDigest = ComputeCanonicalDigest();
        }

        public int SchemaVersion { get; }
        public string StageOutcomeFactId { get; }
        public StageOutcomeDisposition OutcomeDisposition { get; }
        public StageClearReason ClearReason { get; }
        public StageFailureReason FailureReason { get; }
        public string OutcomeSegmentId { get; }
        public long RootAdmissionSequence { get; }
        public long TerminalEpochSequence { get; }
        public long TotalActiveElapsedMilliseconds { get; }
        public long CombatActiveElapsedMilliseconds { get; }
        public long OutcomeFactsSealedAtSequence { get; }
        public string CanonicalDigest { get; }

        private string ComputeCanonicalDigest()
        {
            StringBuilder builder = new(640);
            StageCanonicalDigest.Append(builder, "outcome.schemaVersion", SchemaVersion);
            StageCanonicalDigest.Append(builder, "outcome.id", StageOutcomeFactId);
            StageCanonicalDigest.Append(builder, "outcome.disposition", (int)OutcomeDisposition);
            StageCanonicalDigest.Append(builder, "outcome.clearReason", (int)ClearReason);
            StageCanonicalDigest.Append(builder, "outcome.failureReason", (int)FailureReason);
            StageCanonicalDigest.Append(builder, "outcome.segmentId", OutcomeSegmentId);
            StageCanonicalDigest.Append(builder, "outcome.rootAdmissionSequence", RootAdmissionSequence);
            StageCanonicalDigest.Append(builder, "outcome.terminalEpochSequence", TerminalEpochSequence);
            StageCanonicalDigest.Append(
                builder,
                "outcome.totalActiveElapsedMilliseconds",
                TotalActiveElapsedMilliseconds);
            StageCanonicalDigest.Append(
                builder,
                "outcome.combatActiveElapsedMilliseconds",
                CombatActiveElapsedMilliseconds);
            StageCanonicalDigest.Append(
                builder,
                "outcome.factsSealedAtSequence",
                OutcomeFactsSealedAtSequence);
            return StageCanonicalDigest.Compute(builder.ToString());
        }
    }

    internal sealed class StageRunFactBundle
    {
        public StageRunFactBundle(
            StageSceneSegmentResult[] segmentResults,
            StageTutorialRouteSummaryFact tutorialRouteSummary,
            StageRunCombatFacts combatFacts,
            StageRunSemanticProofFact[] semanticProofs,
            StageOutcomeFact outcome)
        {
            SegmentResults = segmentResults ?? Array.Empty<StageSceneSegmentResult>();
            TutorialRouteSummary = tutorialRouteSummary;
            CombatFacts = combatFacts;
            SemanticProofs = semanticProofs ?? Array.Empty<StageRunSemanticProofFact>();
            Outcome = outcome;
        }

        public StageSceneSegmentResult[] SegmentResults { get; }
        public StageTutorialRouteSummaryFact TutorialRouteSummary { get; }
        public StageRunCombatFacts CombatFacts { get; }
        public StageRunSemanticProofFact[] SemanticProofs { get; }
        public StageOutcomeFact Outcome { get; }
    }

    internal sealed class StageRunFactAccumulator
    {
        private sealed class MutableSegment
        {
            public string SegmentId;
            public int SequenceIndex;
            public bool Entered;
            public bool Completed;
            public double ActiveElapsedMilliseconds;
        }

        private sealed class MutableProof
        {
            public string ProofId;
            public string SourceKind;
            public int Count;
            public double ActualValue;
            public long FirstObservedSegmentMilliseconds;
            public bool Qualified;
        }

        private readonly StageRunIdentity identity;
        private readonly MutableSegment[] segments;
        private readonly List<decimal> playerDamageValues = new();
        private readonly List<StageRunSummonUseFact> summonUses = new();
        private readonly Dictionary<string, MutableProof> proofs = new(StringComparer.Ordinal);
        private int currentSegmentIndex;
        private int playerDownCount;
        private int perfectDodgeCount;
        private long summonAdmissionSequence;
        private long factSequence;
        private double combatActiveElapsedMilliseconds;
        private double forwardRiskElapsedMilliseconds;
        private bool stationCollectorBound;
        private bool stationGuideReleased;
        private bool hasClockSample;
        private double lastRealtimeSeconds;
        private bool previousRouteActive;
        private bool previousFocused;
        private bool previousExplicitPause;
        private bool previousCombatEligible;
        private bool previousForwardRiskEligible;
        private StageTutorialRouteSummaryFact tutorialRouteSummary;

        public StageRunFactAccumulator(StageRunIdentity identity, StageRunRouteSnapshot routeSnapshot)
        {
            this.identity = identity ?? throw new ArgumentNullException(nameof(identity));
            if (routeSnapshot == null)
            {
                throw new ArgumentNullException(nameof(routeSnapshot));
            }

            segments = new MutableSegment[routeSnapshot.SegmentCount];
            for (int i = 0; i < segments.Length; i++)
            {
                segments[i] = new MutableSegment
                {
                    SegmentId = routeSnapshot.GetSegment(i).SegmentId,
                    SequenceIndex = i
                };
            }
        }

        public StageTutorialRouteSummaryFact TutorialRouteSummary => tutorialRouteSummary;

        public void ActivateFirstSegment()
        {
            currentSegmentIndex = 0;
            segments[0].Entered = true;
            RebaseClock();
        }

        public void EnterSegment(int segmentIndex)
        {
            currentSegmentIndex = segmentIndex;
            segments[segmentIndex].Entered = true;
            RebaseClock();
        }

        public bool TryPulse(
            double realtimeSeconds,
            StageRunLifecycleState lifecycleState,
            bool isFocused,
            bool isExplicitlyPaused,
            bool combatEligible,
            bool forwardRiskEligible,
            out string error)
        {
            error = string.Empty;
            if (double.IsNaN(realtimeSeconds)
                || double.IsInfinity(realtimeSeconds)
                || realtimeSeconds < 0d)
            {
                error = "Run clock received an invalid monotonic timestamp.";
                return false;
            }

            bool routeActive = lifecycleState == StageRunLifecycleState.CorridorActive
                || lifecycleState == StageRunLifecycleState.StationActive;
            if (hasClockSample)
            {
                double deltaSeconds = realtimeSeconds - lastRealtimeSeconds;
                if (deltaSeconds < -0.000001d)
                {
                    error = "Run clock moved backwards.";
                    return false;
                }

                if (deltaSeconds > 0d
                    && previousRouteActive
                    && previousFocused
                    && !previousExplicitPause)
                {
                    double deltaMilliseconds = deltaSeconds * 1000d;
                    segments[currentSegmentIndex].ActiveElapsedMilliseconds += deltaMilliseconds;
                    if (previousCombatEligible)
                    {
                        combatActiveElapsedMilliseconds += deltaMilliseconds;
                    }

                    if (previousForwardRiskEligible)
                    {
                        forwardRiskElapsedMilliseconds += deltaMilliseconds;
                    }
                }
            }

            hasClockSample = true;
            lastRealtimeSeconds = realtimeSeconds;
            previousRouteActive = routeActive;
            previousFocused = isFocused;
            previousExplicitPause = isExplicitlyPaused;
            previousCombatEligible = routeActive && combatEligible;
            previousForwardRiskEligible = routeActive && combatEligible && forwardRiskEligible;
            return true;
        }

        public bool TrySealTutorialRouteCompletion(out string error)
        {
            error = string.Empty;
            if (currentSegmentIndex != 0 || !segments[0].Entered || segments[0].Completed)
            {
                if (tutorialRouteSummary != null && segments[0].Completed)
                {
                    return true;
                }

                error = "Tutorial route completion is not legal for the current segment state.";
                return false;
            }

            tutorialRouteSummary = new StageTutorialRouteSummaryFact(
                identity,
                segments[0].SegmentId,
                ToCanonicalMilliseconds(segments[0].ActiveElapsedMilliseconds));
            return true;
        }

        public bool TryCompleteCurrentSegment(out string error)
        {
            error = string.Empty;
            MutableSegment segment = segments[currentSegmentIndex];
            if (!segment.Entered)
            {
                error = "The current segment was never entered.";
                return false;
            }

            if (segment.Completed)
            {
                return true;
            }

            if (currentSegmentIndex == 0 && tutorialRouteSummary == null)
            {
                error = "Corridor tutorial facts must be sealed before the single-load handoff.";
                return false;
            }

            segment.Completed = true;
            RebaseClock();
            return true;
        }

        public bool TryBindStationCollector(out string error)
        {
            error = string.Empty;
            if (currentSegmentIndex != 1 || !segments[1].Entered)
            {
                error = "Station fact collector cannot bind outside the entered Station segment.";
                return false;
            }

            stationCollectorBound = true;
            return true;
        }

        public bool TryMarkStationGuideReleased(out string error)
        {
            error = string.Empty;
            if (!stationCollectorBound || currentSegmentIndex != 1 || !segments[1].Entered)
            {
                error = "Station guide release has no bound current-run fact collector.";
                return false;
            }

            stationGuideReleased = true;
            return true;
        }

        public bool TryRecordPlayerDamage(float amount, out string error)
        {
            error = string.Empty;
            if (!CanRecordStationFact(out error) || amount <= 0f || float.IsNaN(amount) || float.IsInfinity(amount))
            {
                if (string.IsNullOrEmpty(error))
                {
                    error = "Resolved player damage must be finite and positive.";
                }

                return false;
            }

            playerDamageValues.Add((decimal)amount);
            return true;
        }

        public bool TryRecordPlayerDown(out string error)
        {
            if (!CanRecordStationFact(out error))
            {
                return false;
            }

            playerDownCount = Math.Max(1, playerDownCount);
            return true;
        }

        public bool TryRecordPerfectDodge(out string error)
        {
            if (!CanRecordStationFact(out error))
            {
                return false;
            }

            perfectDodgeCount++;
            return true;
        }

        public bool TryRecordSummonUse(string slotRoleId, int spentTier, out string error)
        {
            if (!CanRecordStationFact(out error)
                || string.IsNullOrWhiteSpace(slotRoleId)
                || spentTier < 1
                || spentTier > 3)
            {
                if (string.IsNullOrEmpty(error))
                {
                    error = "Summon use requires a stable slot/role ID and spent tier 1-3.";
                }

                return false;
            }

            summonUses.Add(new StageRunSummonUseFact(
                ++summonAdmissionSequence,
                slotRoleId,
                spentTier,
                CurrentSegmentElapsedMilliseconds));
            return true;
        }

        public bool TryRecordSemanticProof(
            string proofId,
            string sourceKind,
            double actualValue,
            bool qualified,
            out string error)
        {
            if (!CanRecordStationFact(out error))
            {
                return false;
            }

            return TryRecordSemanticProofCore(proofId, sourceKind, actualValue, qualified, out error);
        }

        public bool TrySealTerminalFacts(
            EncounterTerminalResolution resolution,
            out StageRunFactBundle bundle,
            out string error)
        {
            bundle = null;
            error = string.Empty;
            if (currentSegmentIndex != 1
                || !segments[1].Entered
                || !stationCollectorBound
                || !stationGuideReleased)
            {
                error = "Station facts require an entered segment, bound collector, and explicit guide Released state.";
                return false;
            }

            if (tutorialRouteSummary == null || !segments[0].Completed)
            {
                error = "Corridor tutorial facts and segment closure are missing.";
                return false;
            }

            bool validClear = resolution.Outcome == EncounterTerminalOutcome.Clear
                && (resolution.Reason == EncounterTerminalReason.BossTerminal
                    || resolution.Reason == EncounterTerminalReason.SimultaneousTerminalClear);
            bool validFail = resolution.Outcome == EncounterTerminalOutcome.Fail
                && resolution.Reason == EncounterTerminalReason.PlayerTerminal;
            if (!validClear && !validFail)
            {
                error = "Terminal resolution cannot map to the closed revision-1 stage outcome vocabulary.";
                return false;
            }

            if (resolution.Outcome == EncounterTerminalOutcome.Fail && resolution.PlayerDown)
            {
                playerDownCount = Math.Max(1, playerDownCount);
            }

            if (playerDownCount == 0
                && !TryRecordSemanticProofCore(
                    StageRunFactVocabulary.SurvivalNoPlayerDownProofId,
                    StageRunFactVocabulary.TerminalSubjectSnapshotSourceKind,
                    1d,
                    true,
                    out error))
            {
                return false;
            }

            long forwardRiskMilliseconds = ToCanonicalMilliseconds(forwardRiskElapsedMilliseconds);
            if (forwardRiskMilliseconds > 0
                && !TryRecordSemanticProofCore(
                    StageRunFactVocabulary.MovementForwardRiskTimeProofId,
                    StageRunFactVocabulary.ForwardRiskClockSourceKind,
                    forwardRiskMilliseconds,
                    true,
                    out error))
            {
                return false;
            }

            segments[1].Completed = true;
            StageSceneSegmentResult[] segmentResults = new StageSceneSegmentResult[segments.Length];
            long totalActiveElapsedMilliseconds = 0;
            for (int i = 0; i < segments.Length; i++)
            {
                MutableSegment source = segments[i];
                long activeMilliseconds = ToCanonicalMilliseconds(source.ActiveElapsedMilliseconds);
                totalActiveElapsedMilliseconds += activeMilliseconds;
                segmentResults[i] = new StageSceneSegmentResult(
                    source.SegmentId,
                    source.SequenceIndex,
                    source.Entered,
                    source.Completed,
                    source.Completed ? StageSceneSegmentExitReason.Completed : StageSceneSegmentExitReason.None,
                    activeMilliseconds);
            }

            decimal damageTotal = 0m;
            for (int i = 0; i < playerDamageValues.Count; i++)
            {
                damageTotal += playerDamageValues[i];
            }

            var combatFacts = new StageRunCombatFacts(
                (double)damageTotal,
                playerDownCount,
                perfectDodgeCount,
                summonUses.ToArray(),
                forwardRiskMilliseconds);

            var sortedProofs = new List<MutableProof>(proofs.Values);
            sortedProofs.Sort((left, right) => string.Compare(left.ProofId, right.ProofId, StringComparison.Ordinal));
            StageRunSemanticProofFact[] semanticProofs = new StageRunSemanticProofFact[sortedProofs.Count];
            for (int i = 0; i < sortedProofs.Count; i++)
            {
                MutableProof source = sortedProofs[i];
                semanticProofs[i] = new StageRunSemanticProofFact(
                    source.ProofId,
                    source.SourceKind,
                    source.Count,
                    source.ActualValue,
                    source.FirstObservedSegmentMilliseconds,
                    source.Qualified);
            }

            long sealSequence = ++factSequence;
            var outcome = new StageOutcomeFact(
                identity,
                resolution,
                segments[1].SegmentId,
                totalActiveElapsedMilliseconds,
                ToCanonicalMilliseconds(combatActiveElapsedMilliseconds),
                sealSequence);
            bundle = new StageRunFactBundle(
                segmentResults,
                tutorialRouteSummary,
                combatFacts,
                semanticProofs,
                outcome);
            RebaseClock();
            return true;
        }

        private long CurrentSegmentElapsedMilliseconds =>
            ToCanonicalMilliseconds(segments[currentSegmentIndex].ActiveElapsedMilliseconds);

        private bool CanRecordStationFact(out string error)
        {
            error = string.Empty;
            if (currentSegmentIndex != 1 || !segments[1].Entered || !stationCollectorBound || segments[1].Completed)
            {
                error = "Station fact is outside the active bound Station collection window.";
                return false;
            }

            return true;
        }

        private bool TryRecordSemanticProofCore(
            string proofId,
            string sourceKind,
            double actualValue,
            bool qualified,
            out string error)
        {
            error = string.Empty;
            if (string.IsNullOrWhiteSpace(proofId)
                || string.IsNullOrWhiteSpace(sourceKind)
                || double.IsNaN(actualValue)
                || double.IsInfinity(actualValue)
                || actualValue < 0d)
            {
                error = "Semantic proof requires stable IDs and a finite nonnegative value.";
                return false;
            }

            if (!proofs.TryGetValue(proofId, out MutableProof proof))
            {
                proofs.Add(
                    proofId,
                    new MutableProof
                    {
                        ProofId = proofId,
                        SourceKind = sourceKind,
                        Count = 1,
                        ActualValue = actualValue,
                        FirstObservedSegmentMilliseconds = CurrentSegmentElapsedMilliseconds,
                        Qualified = qualified
                    });
                return true;
            }

            if (!string.Equals(proof.SourceKind, sourceKind, StringComparison.Ordinal))
            {
                error = $"Semantic proof {proofId} changed source kind within one run.";
                return false;
            }

            proof.Count++;
            proof.ActualValue = Math.Max(proof.ActualValue, actualValue);
            proof.Qualified |= qualified;
            return true;
        }

        private void RebaseClock()
        {
            hasClockSample = false;
            previousRouteActive = false;
            previousFocused = false;
            previousExplicitPause = false;
            previousCombatEligible = false;
            previousForwardRiskEligible = false;
        }

        private static long ToCanonicalMilliseconds(double elapsedMilliseconds)
        {
            if (elapsedMilliseconds <= 0d)
            {
                return 0L;
            }

            if (elapsedMilliseconds >= long.MaxValue)
            {
                return long.MaxValue;
            }

            return (long)Math.Ceiling(elapsedMilliseconds);
        }
    }
}
