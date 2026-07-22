using System;
using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using DimensionBrawl.AI;
using DimensionBrawl.Combat;
using DimensionBrawl.Enemies;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Player;
using DimensionBrawl.UI;
using DimensionBrawl.UI.StageClear;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace DimensionBrawl.Tests
{
    public sealed class StageRunRoutePlayModeTests
    {
        private const string CorridorScenePath = "Assets/_Game/Scenes/OlympusCorridorInvasionStage.unity";
        private const string StationScenePath = "Assets/_Game/Scenes/OlympusStationCombatStage.unity";
        private const string LobbyScenePath = "Assets/_Game/Scenes/UI/UI_Lobby.unity";
        private const string RouteDigest = "878dac821103cdca2d2ad29a3fab8bce27109e9a5c1d551b14eccb736fd252d0";
        private const string StationEntryConditionId = "corridor.station-entry.reached";
        private const string StationStageId = "OLYMPUS-STATION-COMBAT-01";
        private const string OneRowPlayableStageId = "B0-ONE-ROW-TEST-01";
        private const string OneRowSegmentId = "b0_one_row_entry_final";
        private const string OneRowTerminalConditionId = "b0.one-row.encounter.terminal";
        private const string OneRowReplayActionId = "b0-one-row.replay";
        private const string OneRowRetryActionId = "b0-one-row.retry";
        private const string OneRowLobbyActionId = "b0-one-row.to-lobby";
        private const string HistoricalRouteDigest = "2b912058cefb5b9ad14ed9d11336e2344dd12efa9789fc2df676a7ac74e821b9";
        private const string OlympusTutorialPlanDigest = "b1b00dd84e27fe8d06c6736d85b16ff6bfe141b7ccb70b01ea851144dd8182f2";

        [UnityTearDown]
        public IEnumerator ResetStageRunRuntime()
        {
            StageRunRuntime.ResetForTests();
            Time.timeScale = 1f;
            yield return null;
        }

        [UnityTest]
        public IEnumerator OneRowEntryFinalRouteAdmitsTerminalActiveWithoutHandoffEvidence()
        {
            StageRunRuntime.ResetForTests();
            yield return LoadSingleScene(CorridorScenePath);

            Scene hostScene = SceneManager.GetActiveScene();
            OlympusCorridorCombatFlowController flow =
                RequireSingleSceneComponent<OlympusCorridorCombatFlowController>(hostScene);
            PlayableStageDefinition source = ReadPrivateField<PlayableStageDefinition>(
                flow,
                "playableStageDefinition");
            flow.enabled = false;
            StageRunRuntime.ResetForTests();

            PlayableStageDefinition route = CreateOneRowEntryFinalRoute(source);
            Scene foreignScene = default;
            try
            {
                Assert.That(
                    StageRunRouteSnapshot.TryCreate(
                        route,
                        out StageRunRouteSnapshot snapshot,
                        out string snapshotError),
                    Is.True,
                    snapshotError);
                Assert.That(snapshot.SegmentCount, Is.EqualTo(1));
                Assert.That(snapshot.PlayableStageId, Is.EqualTo(OneRowPlayableStageId));
                Assert.That(
                    snapshot.GetSegmentRoles(0),
                    Is.EqualTo(StageRunSegmentRole.Entry | StageRunSegmentRole.Terminal));
                Assert.That(snapshot.IsEntrySegment(0), Is.True);
                Assert.That(snapshot.IsTerminalSegment(0), Is.True);
                Assert.That(snapshot.GetSegment(0).SegmentId, Is.EqualTo(OneRowSegmentId));
                Assert.That(
                    snapshot.GetSegment(0).ExitConditionId,
                    Is.EqualTo(OneRowTerminalConditionId));

                Assert.That(
                    StageRunRuntime.TryAdmitFirstSegment(
                        route,
                        hostScene,
                        out StageRunContext context,
                        out string admissionError),
                    Is.True,
                    admissionError);
                Assert.That(context, Is.SameAs(StageRunRuntime.ActiveContext));
                Assert.That(
                    context.Identity.PlayableStageId,
                    Is.EqualTo(OneRowPlayableStageId));
                Assert.That(
                    context.ResultProgressionJoinSnapshot.PlayableStageId,
                    Is.EqualTo(OneRowPlayableStageId));
                Assert.That(context.CurrentSegmentIndex, Is.Zero);
                Assert.That(
                    context.CurrentSegmentRoles,
                    Is.EqualTo(StageRunSegmentRole.Entry | StageRunSegmentRole.Terminal));
                Assert.That(context.IsCurrentSegmentActive, Is.True);
                Assert.That(context.IsCurrentSegmentTerminalActive, Is.True);
                Assert.That(context.LifecycleState, Is.EqualTo(StageRunLifecycleState.StationActive));
                Assert.That(context.PendingHandoffToken, Is.Null);
                Assert.That(context.SegmentEntryReceipt, Is.Null);
                Assert.That(context.HandoffTerminalReceipt, Is.Null);
                string runId = context.Identity.RunId;

                Assert.That(
                    StageRunRuntime.TryAdmitFirstSegment(
                        route,
                        hostScene,
                        out StageRunContext replayedContext,
                        out string replayError),
                    Is.True,
                    replayError);
                Assert.That(replayedContext, Is.SameAs(context));
                Assert.That(replayedContext.Identity.RunId, Is.EqualTo(runId));

                PlayableStageDefinition staleRoute = UnityEngine.Object.Instantiate(route);
                staleRoute.hideFlags = HideFlags.HideAndDontSave;
                try
                {
                    SetPrivateField(staleRoute, "canonicalRouteDigest", "stale-route-digest");
                    Assert.That(
                        StageRunRuntime.TryAdmitFirstSegment(
                            staleRoute,
                            hostScene,
                            out StageRunContext staleContext,
                            out string staleError),
                        Is.False);
                    Assert.That(staleContext, Is.Null);
                    Assert.That(staleError, Does.Contain("digest mismatch"));
                    Assert.That(StageRunRuntime.ActiveContext, Is.SameAs(context));
                    Assert.That(StageRunRuntime.ActiveContext.Identity.RunId, Is.EqualTo(runId));
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(staleRoute);
                }

                PlayableStageDefinition foreignJoin = UnityEngine.Object.Instantiate(route);
                foreignJoin.hideFlags = HideFlags.HideAndDontSave;
                try
                {
                    SetPrivateField(foreignJoin.ResultProgressionJoin, "revision", 1);
                    SetPrivateField(
                        foreignJoin.ResultProgressionJoin,
                        "canonicalDigest",
                        string.Empty);
                    Assert.That(
                        foreignJoin.TryComputeResultProgressionJoinDigest(
                            out string foreignJoinDigest,
                            out string foreignJoinDigestError),
                        Is.True,
                        foreignJoinDigestError);
                    SetPrivateField(
                        foreignJoin.ResultProgressionJoin,
                        "canonicalDigest",
                        foreignJoinDigest);
                    Assert.That(
                        StageRunRuntime.TryAdmitFirstSegment(
                            foreignJoin,
                            hostScene,
                            out StageRunContext foreignJoinContext,
                            out string foreignJoinError),
                        Is.False);
                    Assert.That(foreignJoinContext, Is.Null);
                    Assert.That(foreignJoinError, Does.Contain("stale"));
                    Assert.That(StageRunRuntime.ActiveContext, Is.SameAs(context));
                    Assert.That(StageRunRuntime.ActiveContext.Identity.RunId, Is.EqualTo(runId));
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(foreignJoin);
                }

                foreignScene = SceneManager.CreateScene("B0_1_ForeignAdmissionHost");
                Assert.That(
                    StageRunRuntime.TryAdmitFirstSegment(
                        route,
                        foreignScene,
                        out StageRunContext foreignContext,
                        out string foreignError),
                    Is.False);
                Assert.That(foreignContext, Is.Null);
                Assert.That(foreignError, Does.Contain("does not match"));
                Assert.That(StageRunRuntime.ActiveContext, Is.SameAs(context));
                Assert.That(StageRunRuntime.ActiveContext.Identity.RunId, Is.EqualTo(runId));
            }
            finally
            {
                StageRunRuntime.ResetForTests();
                DestroyOneRowEntryFinalRoute(route);
                if (foreignScene.IsValid() && foreignScene.isLoaded)
                {
                    SceneManager.UnloadSceneAsync(foreignScene);
                }
            }
        }

        [UnityTest]
        public IEnumerator OneRowTerminalCommitSealsTruthfulFactsAndPresentsExactlyOnce()
        {
            StageRunRuntime.ResetForTests();
            yield return LoadSingleScene(CorridorScenePath);

            Scene hostScene = SceneManager.GetActiveScene();
            OlympusCorridorCombatFlowController flow =
                RequireSingleSceneComponent<OlympusCorridorCombatFlowController>(hostScene);
            PlayableStageDefinition source = ReadPrivateField<PlayableStageDefinition>(
                flow,
                "playableStageDefinition");
            DisableOneRowHostRuntime(hostScene);
            StageRunRuntime.ResetForTests();

            OneRowCombatFixture primary = CreateOneRowCombatFixture(hostScene, "Primary");
            OneRowCombatFixture foreign = CreateOneRowCombatFixture(hostScene, "Foreign");
            yield return null;
            yield return null;
            Assert.That(primary.Encounter.TerminalCoordinator, Is.Not.Null);
            Assert.That(foreign.Encounter.TerminalCoordinator, Is.Not.Null);
            Assert.That(
                foreign.Encounter.TryRestartCoordinatedRunWithGenerationForTests(
                    primary.Encounter.TerminalCoordinator.RunGeneration,
                    out string alignedGenerationError),
                Is.True,
                alignedGenerationError);
            Assert.That(
                foreign.PlayerHealth.TryApplyDamage(new DamageInfo(
                    null,
                    DamageTeam.Enemy,
                    4.5f,
                    foreign.PlayerHealth.transform.position,
                    Vector3.forward,
                    0f,
                    DamageResponsePolicy.DamageOnly,
                    CombatControlLockPolicy.None)),
                Is.True);
            Assert.That(foreign.PlayerHealth.CurrentHealth, Is.EqualTo(95.5f).Within(0.0001f));
            Assert.That(ApplyLethalDamage(foreign.EnemyHealth, DamageTeam.Player), Is.True);
            Assert.That(foreign.Encounter.HasTerminalResolution, Is.True);

            PlayableStageDefinition route = CreateOneRowEntryFinalRoute(source);
            Scene foreignScene = default;
            OneRowCombatFixture foreignSceneFixture = null;
            Action<DamageInfo> primaryDamageFactBridge = null;
            try
            {
                Assert.That(
                    StageRunRuntime.TryAdmitFirstSegment(
                        route,
                        hostScene,
                        out StageRunContext context,
                        out string admissionError),
                    Is.True,
                    admissionError);
                Assert.That(
                    context.TutorialFactRequirement,
                    Is.EqualTo(StageRunTutorialFactRequirement.None));
                Assert.That(
                    context.TrySealTutorialRouteCompletion(out string tutorialError),
                    Is.False);
                Assert.That(tutorialError, Does.Contain("no tutorial fact requirement"));
                Assert.That(context.TutorialRouteSummaryFact, Is.Null);

                foreignScene = SceneManager.CreateScene("B0_2_ForeignCollectorHost");
                Assert.That(
                    StageRunRuntime.TryBindTerminalFactCollectorForTests(
                        foreignScene.handle,
                        out string foreignCollectorError),
                    Is.False);
                Assert.That(foreignCollectorError, Does.Contain("does not belong"));
                Assert.That(context.LifecycleState, Is.EqualTo(StageRunLifecycleState.StationActive));

                foreignSceneFixture = CreateOneRowCombatFixture(foreignScene, "ForeignScene");
                yield return null;
                yield return null;
                Assert.That(
                    ApplyLethalDamage(
                        foreignSceneFixture.EnemyHealth,
                        DamageTeam.Player),
                    Is.True);
                Assert.That(
                    StageRunRuntime.TryCommitTerminalResolution(
                        foreignSceneFixture.Encounter,
                        foreignSceneFixture.Encounter.TerminalResolution,
                        out StageRunResultSummary foreignSceneSummary,
                        out StageRunResultCommitReceipt foreignSceneReceipt,
                        out string foreignSceneCommitError),
                    Is.False);
                Assert.That(foreignSceneSummary, Is.Null);
                Assert.That(foreignSceneReceipt, Is.Null);
                Assert.That(foreignSceneCommitError, Does.Contain("active terminal encounter"));
                Assert.That(context.TerminalRecordReceiptCount, Is.Zero);
                Assert.That(context.TerminalRecord, Is.Null);
                Assert.That(context.AbortRecord, Is.Null);
                Assert.That(context.LifecycleState, Is.EqualTo(StageRunLifecycleState.StationActive));

                Assert.That(
                    StageRunRuntime.TryRegisterTerminalCoordinatorForTests(
                        foreign.Encounter,
                        out string staleCoordinatorError),
                    Is.False);
                Assert.That(staleCoordinatorError, Does.Contain("live and Idle"));
                Assert.That(context.TerminalRecordReceiptCount, Is.Zero);
                Assert.That(context.TerminalRecord, Is.Null);
                Assert.That(context.AbortRecord, Is.Null);
                Assert.That(context.LifecycleState, Is.EqualTo(StageRunLifecycleState.StationActive));

                Assert.That(
                    StageRunRuntime.TryBindTerminalFactCollectorForTests(
                        hostScene.handle,
                        out string collectorError),
                    Is.True,
                    collectorError);
                Assert.That(
                    StageRunRuntime.TryRegisterTerminalCoordinatorForTests(
                        primary.Encounter,
                        out string coordinatorError),
                    Is.True,
                    coordinatorError);
                primaryDamageFactBridge = damageInfo =>
                {
                    Assert.That(
                        StageRunRuntime.TryRecordResolvedPlayerDamageForTests(
                            damageInfo.Amount,
                            out string factError),
                        Is.True,
                        factError);
                };
                primary.PlayerHealth.Damaged += primaryDamageFactBridge;
                Assert.That(
                    context.TryPulseActiveTime(
                        10d,
                        true,
                        false,
                        true,
                        true,
                        out string firstPulseError),
                    Is.True,
                    firstPulseError);
                Assert.That(
                    context.TryPulseActiveTime(
                        10.25d,
                        true,
                        false,
                        true,
                        true,
                        out string secondPulseError),
                    Is.True,
                    secondPulseError);
                Assert.That(
                    primary.PlayerHealth.TryApplyDamage(new DamageInfo(
                        null,
                        DamageTeam.Enemy,
                        4.5f,
                        primary.PlayerHealth.transform.position,
                        Vector3.forward,
                        0f,
                        DamageResponsePolicy.DamageOnly,
                        CombatControlLockPolicy.None)),
                    Is.True,
                    "The fixture must publish the same resolved player-damage event recorded as a fact.");
                Assert.That(primary.PlayerHealth.CurrentHealth, Is.EqualTo(95.5f).Within(0.0001f));

                Assert.That(
                    StageRunRuntime.TryCommitTerminalResolution(
                        foreign.Encounter,
                        foreign.Encounter.TerminalResolution,
                        out StageRunResultSummary foreignSummary,
                        out StageRunResultCommitReceipt foreignReceipt,
                        out string foreignCoordinatorError),
                    Is.False);
                Assert.That(foreignSummary, Is.Null);
                Assert.That(foreignReceipt, Is.Null);
                Assert.That(foreignCoordinatorError, Does.Contain("exact registered"));
                Assert.That(context.TerminalRecordReceiptCount, Is.Zero);
                Assert.That(context.TerminalRecord, Is.Null);
                Assert.That(context.AbortRecord, Is.Null);
                Assert.That(context.LifecycleState, Is.EqualTo(StageRunLifecycleState.StationActive));

                StageRunRuntime.InjectTransientResultDecisionIoFailuresForTests(0, 1);
                Assert.That(ApplyLethalDamage(primary.EnemyHealth, DamageTeam.Player), Is.True);
                Assert.That(primary.Encounter.HasTerminalResolution, Is.True);
                Assert.That(
                    primary.Encounter.TerminalResolution.PlayerHealth,
                    Is.EqualTo(95.5f).Within(0.0001f));
                Assert.That(
                    StageRunRuntime.TryCommitForgedTerminalResolutionForTests(
                        primary.Encounter,
                        out StageRunResultSummary forgedSummary,
                        out StageRunResultCommitReceipt forgedReceipt,
                        out string forgedError),
                    Is.False);
                Assert.That(forgedSummary, Is.Null);
                Assert.That(forgedReceipt, Is.Null);
                Assert.That(forgedError, Does.Contain("does not exactly match"));
                Assert.That(context.LifecycleState, Is.EqualTo(StageRunLifecycleState.StationActive));
                Assert.That(context.TerminalRecordReceiptCount, Is.Zero);
                Assert.That(context.TerminalRecord, Is.Null);
                Assert.That(context.AbortRecord, Is.Null);
                Assert.That(
                    StageRunRuntime.TryCommitTerminalResolution(
                        primary.Encounter,
                        primary.Encounter.TerminalResolution,
                        out StageRunResultSummary pendingSummary,
                        out StageRunResultCommitReceipt pendingReceipt,
                        out string initialCommitError),
                    Is.False);
                Assert.That(pendingSummary, Is.Null);
                Assert.That(pendingReceipt, Is.Null);
                Assert.That(initialCommitError, Is.Not.Empty);
                Assert.That(
                    context.LifecycleState,
                    Is.EqualTo(StageRunLifecycleState.CommitRecoveryPending));
                Assert.That(context.TerminalRecordReceiptCount, Is.EqualTo(1));
                Assert.That(context.CommittedSummary, Is.Null);

                Assert.That(
                    StageRunRuntime.TryRecoverPendingResultCommit(
                        out StageRunResultSummary summary,
                        out StageRunResultCommitReceipt receipt,
                        out string recoveryError),
                    Is.True,
                    recoveryError);
                Assert.That(summary, Is.SameAs(context.CommittedSummary));
                Assert.That(receipt, Is.SameAs(context.CommitReceipt));
                Assert.That(context.LifecycleState, Is.EqualTo(StageRunLifecycleState.Committed));
                Assert.That(summary.SegmentResultCount, Is.EqualTo(1));
                StageSceneSegmentResult segmentResult = summary.GetSegmentResult(0);
                Assert.That(segmentResult.SegmentId, Is.EqualTo(OneRowSegmentId));
                Assert.That(segmentResult.SegmentSequenceIndex, Is.Zero);
                Assert.That(segmentResult.Entered, Is.True);
                Assert.That(segmentResult.Completed, Is.True);
                Assert.That(
                    segmentResult.ExitReason,
                    Is.EqualTo(StageSceneSegmentExitReason.Completed));
                Assert.That(summary.HasTutorialRouteSummaryFact, Is.False);
                Assert.That(summary.TutorialRouteSummaryFact, Is.Null);
                Assert.That(summary.TutorialRouteSummaryFactDigest, Is.Empty);
                Assert.That(
                    summary.OutcomeFact.OutcomeSegmentId,
                    Is.EqualTo(OneRowSegmentId));
                Assert.That(
                    summary.OutcomeFact.OutcomeDisposition,
                    Is.EqualTo(StageOutcomeDisposition.Clear));
                Assert.That(segmentResult.ActiveElapsedMilliseconds, Is.EqualTo(250));
                Assert.That(summary.OutcomeFact.TotalActiveElapsedMilliseconds, Is.EqualTo(250));
                Assert.That(summary.OutcomeFact.CombatActiveElapsedMilliseconds, Is.EqualTo(250));
                Assert.That(summary.CombatFacts.PlayerDamageTaken, Is.EqualTo(4.5d));
                Assert.That(summary.CombatFacts.PerfectDodgeCount, Is.Zero);
                Assert.That(summary.CombatFacts.SummonUseCount, Is.Zero);
                Assert.That(summary.CombatFacts.HasForwardRiskElapsed, Is.True);
                Assert.That(
                    summary.CombatFacts.ForwardRiskElapsedMilliseconds,
                    Is.EqualTo(250));
                Assert.That(
                    summary.TryGetSemanticProof(
                        StageRunFactVocabulary.MovementForwardRiskTimeProofId,
                        out StageRunSemanticProofFact forwardRiskProof),
                    Is.True);
                Assert.That(
                    forwardRiskProof.SourceKind,
                    Is.EqualTo(StageRunFactVocabulary.ForwardRiskClockSourceKind));
                Assert.That(forwardRiskProof.ActualValue, Is.EqualTo(250d));
                Assert.That(forwardRiskProof.Qualified, Is.True);
                Assert.That(
                    context.OwnerCoverageRecord.FinalizationContext,
                    Is.EqualTo(StageTerminalFinalizationContext.NonCourseStageTerminal));
                Assert.That(receipt.ResultSummaryDigest, Is.EqualTo(summary.ResultSummaryDigest));
                Assert.That(receipt.SummaryCommittedAtSequence, Is.EqualTo(1));
                Assert.That(receipt.CanonicalDigest, Has.Length.EqualTo(64));
                Assert.That(receipt.EnvelopeChecksum, Has.Length.EqualTo(64));
                Assert.That(context.PendingHandoffToken, Is.Null);
                Assert.That(context.SegmentEntryReceipt, Is.Null);
                Assert.That(context.HandoffTerminalReceipt, Is.Null);

                Assert.That(
                    StageRunRuntime.TryCommitTerminalResolution(
                        primary.Encounter,
                        primary.Encounter.TerminalResolution,
                        out StageRunResultSummary duplicateSummary,
                        out StageRunResultCommitReceipt duplicateReceipt,
                        out string duplicateError),
                    Is.True,
                    duplicateError);
                Assert.That(duplicateSummary, Is.SameAs(summary));
                Assert.That(duplicateReceipt, Is.SameAs(receipt));
                Assert.That(context.TerminalRecordReceiptCount, Is.EqualTo(1));
                Assert.That(
                    context.TerminalRecord.Matches(foreign.Encounter.TerminalResolution),
                    Is.True,
                    "The foreign replay fixture must carry a value-identical terminal tuple.");
                Assert.That(
                    context.TerminalEpochClosureRecord.SourceEvidenceId,
                    Is.EqualTo(
                        foreign.Encounter.TerminalCoordinator.TerminalEpochEvidence.EvidenceId),
                    "The foreign replay fixture must carry value-identical epoch evidence.");
                Assert.That(
                    context.TerminalEpochClosureRecord.SourceEvidenceDigest,
                    Is.EqualTo(
                        foreign.Encounter.TerminalCoordinator.TerminalEpochEvidence.CanonicalDigest));
                Assert.That(
                    StageRunRuntime.TryCommitTerminalResolution(
                        foreign.Encounter,
                        foreign.Encounter.TerminalResolution,
                        out StageRunResultSummary foreignReplaySummary,
                        out StageRunResultCommitReceipt foreignReplayReceipt,
                        out string foreignReplayError),
                    Is.False);
                Assert.That(foreignReplaySummary, Is.Null);
                Assert.That(foreignReplayReceipt, Is.Null);
                Assert.That(foreignReplayError, Does.Contain("different terminal encounter"));
                Assert.That(context.CommittedSummary, Is.SameAs(summary));
                Assert.That(context.CommitReceipt, Is.SameAs(receipt));
                Assert.That(context.TerminalRecordReceiptCount, Is.EqualTo(1));

                string decisionPath =
                    StageRunRuntime.GetResultCommitDecisionPathForTests(context.Identity.RunId);
                Assert.That(System.IO.File.Exists(decisionPath), Is.True);
                byte[] decisionBytes = System.IO.File.ReadAllBytes(decisionPath);
                StageRunRuntime.ClearResultCommitMemoryCacheForTests();
                Assert.That(
                    StageRunRuntime.TryReadCommittedResultDecision(
                        context.Identity.RunId,
                        out StageRunResultCommitReceipt recoveredReceipt,
                        out string readError),
                    Is.True,
                    readError);
                Assert.That(recoveredReceipt.CanonicalDigest, Is.EqualTo(receipt.CanonicalDigest));
                Assert.That(
                    recoveredReceipt.EnvelopeChecksum,
                    Is.EqualTo(receipt.EnvelopeChecksum));
                Assert.That(System.IO.File.ReadAllBytes(decisionPath), Is.EqualTo(decisionBytes));
                string mismatchedRunId = context.Identity.RunId + "x";
                string mismatchedDecisionPath =
                    StageRunRuntime.GetResultCommitDecisionPathForTests(mismatchedRunId);
                try
                {
                    System.IO.File.Copy(decisionPath, mismatchedDecisionPath, true);
                    Assert.That(
                        StageRunRuntime.TryReadCommittedResultDecision(
                            mismatchedRunId,
                            out StageRunResultCommitReceipt mismatchedReceipt,
                            out string mismatchedReadError),
                        Is.False);
                    Assert.That(mismatchedReceipt, Is.Null);
                    Assert.That(mismatchedReadError, Does.Contain("requested run ID"));
                }
                finally
                {
                    if (System.IO.File.Exists(mismatchedDecisionPath))
                    {
                        System.IO.File.Delete(mismatchedDecisionPath);
                    }
                }

                Assert.That(
                    StageRunRuntime.TryPrepareResultPresentation(
                        summary,
                        context.ResultProgressionJoinSnapshot,
                        "ko-KR",
                        out StageResultPresentationSnapshot presentation,
                        out StageResultPresentationAuditEnvelope audit,
                        out string presentationError),
                    Is.True,
                    presentationError);
                Assert.That(
                    StageRunRuntime.TryPrepareResultPresentation(
                        summary,
                        context.ResultProgressionJoinSnapshot,
                        "ko-KR",
                        out StageResultPresentationSnapshot duplicatePresentation,
                        out StageResultPresentationAuditEnvelope duplicateAudit,
                        out string duplicatePresentationError),
                    Is.True,
                    duplicatePresentationError);
                Assert.That(duplicatePresentation, Is.SameAs(presentation));
                Assert.That(duplicateAudit, Is.SameAs(audit));
                Assert.That(
                    StageRunRuntime.TryMarkResultPresented(
                        summary,
                        presentation,
                        audit,
                        out string markError),
                    Is.True,
                    markError);
                Assert.That(
                    StageRunRuntime.TryMarkResultPresented(
                        summary,
                        presentation,
                        audit,
                        out string duplicateMarkError),
                    Is.True,
                    duplicateMarkError);
                Assert.That(context.LifecycleState, Is.EqualTo(StageRunLifecycleState.Presented));
            }
            finally
            {
                if (primaryDamageFactBridge != null)
                {
                    primary.PlayerHealth.Damaged -= primaryDamageFactBridge;
                }

                StageRunRuntime.ResetForTests();
                primary.Destroy();
                foreign.Destroy();
                foreignSceneFixture?.Destroy();
                DestroyOneRowEntryFinalRoute(route);
                if (foreignScene.IsValid() && foreignScene.isLoaded)
                {
                    SceneManager.UnloadSceneAsync(foreignScene);
                }
            }
        }

        [UnityTest]
        public IEnumerator OneRowFailureSealsPlayerDownWithoutTutorialOrSurvivalProof()
        {
            StageRunRuntime.ResetForTests();
            yield return LoadSingleScene(CorridorScenePath);

            Scene hostScene = SceneManager.GetActiveScene();
            OlympusCorridorCombatFlowController flow =
                RequireSingleSceneComponent<OlympusCorridorCombatFlowController>(hostScene);
            PlayableStageDefinition source = ReadPrivateField<PlayableStageDefinition>(
                flow,
                "playableStageDefinition");
            DisableOneRowHostRuntime(hostScene);
            StageRunRuntime.ResetForTests();
            OneRowCombatFixture fixture = CreateOneRowCombatFixture(hostScene, "Failure");
            yield return null;
            yield return null;

            PlayableStageDefinition route = CreateOneRowEntryFinalRoute(source);
            Action<DamageInfo> playerDamageFactBridge = null;
            try
            {
                Assert.That(
                    StageRunRuntime.TryAdmitFirstSegment(
                        route,
                        hostScene,
                        out StageRunContext context,
                        out string admissionError),
                    Is.True,
                    admissionError);
                Assert.That(
                    StageRunRuntime.TryBindTerminalFactCollectorForTests(
                        hostScene.handle,
                        out string collectorError),
                    Is.True,
                    collectorError);
                Assert.That(
                    StageRunRuntime.TryRegisterTerminalCoordinatorForTests(
                        fixture.Encounter,
                        out string coordinatorError),
                    Is.True,
                    coordinatorError);
                playerDamageFactBridge = damageInfo =>
                {
                    Assert.That(
                        StageRunRuntime.TryRecordResolvedPlayerDamageForTests(
                            damageInfo.Amount,
                            out string factError),
                        Is.True,
                        factError);
                };
                fixture.PlayerHealth.Damaged += playerDamageFactBridge;

                Assert.That(ApplyLethalDamage(fixture.PlayerHealth, DamageTeam.Enemy), Is.True);
                Assert.That(
                    StageRunRuntime.TryCommitTerminalResolution(
                        fixture.Encounter,
                        fixture.Encounter.TerminalResolution,
                        out StageRunResultSummary summary,
                        out StageRunResultCommitReceipt receipt,
                        out string commitError),
                    Is.True,
                    commitError);
                Assert.That(summary.Outcome, Is.EqualTo(StageRouteOutcome.Fail));
                Assert.That(
                    summary.OutcomeFact.FailureReason,
                    Is.EqualTo(StageFailureReason.PlayerDefeated));
                Assert.That(summary.OutcomeFact.OutcomeSegmentId, Is.EqualTo(OneRowSegmentId));
                Assert.That(summary.CombatFacts.PlayerDownCount, Is.EqualTo(1));
                Assert.That(
                    summary.CombatFacts.PlayerDamageTaken,
                    Is.EqualTo(fixture.PlayerHealth.MaxHealth + 1d));
                Assert.That(fixture.PlayerHealth.CurrentHealth, Is.Zero);
                Assert.That(fixture.Encounter.TerminalResolution.PlayerHealth, Is.Zero);
                Assert.That(summary.HasTutorialRouteSummaryFact, Is.False);
                Assert.That(summary.TutorialRouteSummaryFactDigest, Is.Empty);
                Assert.That(
                    summary.TryGetSemanticProof(
                        StageRunFactVocabulary.SurvivalNoPlayerDownProofId,
                        out _),
                    Is.False);
                Assert.That(summary.SegmentResultCount, Is.EqualTo(1));
                Assert.That(summary.GetSegmentResult(0).Completed, Is.True);
                Assert.That(summary.OfferedActionCount, Is.EqualTo(2));
                Assert.That(
                    summary.TryGetOfferedAction(OneRowRetryActionId, out _),
                    Is.True);
                Assert.That(
                    summary.TryGetOfferedAction(OneRowLobbyActionId, out _),
                    Is.True);
                Assert.That(
                    summary.TryGetOfferedAction(OneRowReplayActionId, out _),
                    Is.False);
                Assert.That(receipt.ResultSummaryDigest, Is.EqualTo(summary.ResultSummaryDigest));
                Assert.That(context.TerminalRecordReceiptCount, Is.EqualTo(1));
            }
            finally
            {
                if (playerDamageFactBridge != null)
                {
                    fixture.PlayerHealth.Damaged -= playerDamageFactBridge;
                }

                StageRunRuntime.ResetForTests();
                fixture.Destroy();
                DestroyOneRowEntryFinalRoute(route);
            }
        }

        [UnityTest]
        public IEnumerator OneRowTerminalBeforeCollectorReadyClosesWithoutProductResult()
        {
            StageRunRuntime.ResetForTests();
            yield return LoadSingleScene(CorridorScenePath);

            Scene hostScene = SceneManager.GetActiveScene();
            OlympusCorridorCombatFlowController flow =
                RequireSingleSceneComponent<OlympusCorridorCombatFlowController>(hostScene);
            PlayableStageDefinition source = ReadPrivateField<PlayableStageDefinition>(
                flow,
                "playableStageDefinition");
            DisableOneRowHostRuntime(hostScene);
            StageRunRuntime.ResetForTests();
            OneRowCombatFixture fixture = CreateOneRowCombatFixture(hostScene, "CollectorMissing");
            yield return null;
            yield return null;

            PlayableStageDefinition route = CreateOneRowEntryFinalRoute(source);
            try
            {
                Assert.That(
                    StageRunRuntime.TryAdmitFirstSegment(
                        route,
                        hostScene,
                        out StageRunContext context,
                        out string admissionError),
                    Is.True,
                    admissionError);
                Assert.That(
                    StageRunRuntime.TryRegisterTerminalCoordinatorForTests(
                        fixture.Encounter,
                        out string coordinatorError),
                    Is.True,
                    coordinatorError);
                string decisionPath =
                    StageRunRuntime.GetResultCommitDecisionPathForTests(context.Identity.RunId);

                Assert.That(ApplyLethalDamage(fixture.EnemyHealth, DamageTeam.Player), Is.True);
                Assert.That(
                    StageRunRuntime.TryCommitTerminalResolution(
                        fixture.Encounter,
                        fixture.Encounter.TerminalResolution,
                        out StageRunResultSummary summary,
                        out StageRunResultCommitReceipt receipt,
                        out string commitError),
                    Is.False);
                Assert.That(commitError, Does.Contain("bound collector"));
                Assert.That(summary, Is.Null);
                Assert.That(receipt, Is.Null);
                Assert.That(context.CommittedSummary, Is.Null);
                Assert.That(context.CommitReceipt, Is.Null);
                Assert.That(context.TerminalRecordReceiptCount, Is.EqualTo(1));
                Assert.That(context.AbortRecord, Is.Not.Null);
                Assert.That(
                    context.AbortRecord.AbortReason,
                    Is.EqualTo(StageRunAbortReason.TerminalFinalizationFailed));
                Assert.That(
                    context.AbortRecord.RouteHandoffCoverage.Disposition,
                    Is.EqualTo(StageRunRouteHandoffCoverageDisposition.NotIssued));
                Assert.That(context.LifecycleState, Is.EqualTo(StageRunLifecycleState.Disposed));
                Assert.That(System.IO.File.Exists(decisionPath), Is.False);
                Assert.That(context.ResultPresentationSnapshot, Is.Null);
                Assert.That(context.ResultPresentationAudit, Is.Null);
            }
            finally
            {
                StageRunRuntime.ResetForTests();
                fixture.Destroy();
                DestroyOneRowEntryFinalRoute(route);
            }
        }

        [UnityTest]
        [Timeout(30000)]
        public IEnumerator OneRowNeutralAdaptersCommitClearFailAndDispatchReplayRetryLobby()
        {
            StageRunRuntime.ResetForTests();
            yield return LoadSingleScene(CorridorScenePath);

            Scene hostScene = SceneManager.GetActiveScene();
            OlympusCorridorCombatFlowController flow =
                RequireSingleSceneComponent<OlympusCorridorCombatFlowController>(hostScene);
            PlayableStageDefinition source = ReadPrivateField<PlayableStageDefinition>(
                flow,
                "playableStageDefinition");
            DisableOneRowHostRuntime(hostScene);
            StageRunRuntime.ResetForTests();

            PlayableStageDefinition route = CreateOneRowEntryFinalRoute(source);
            (string ActionId, StageRouteActionKind Kind, bool FailRun)[] cases =
            {
                (OneRowReplayActionId, StageRouteActionKind.Replay, false),
                (OneRowRetryActionId, StageRouteActionKind.Retry, true),
                (OneRowLobbyActionId, StageRouteActionKind.UIRoute, false)
            };
            try
            {
                for (int caseIndex = 0; caseIndex < cases.Length; caseIndex++)
                {
                    StageRunRuntime.ResetForTests();
                    OneRowAdapterFixture fixture = CreateOneRowAdapterFixture(
                        hostScene,
                        route,
                        $"Dispatch_{caseIndex}");
                    try
                    {
                        yield return null;
                        yield return null;

                        StageRunContext context = StageRunRuntime.ActiveContext;
                        Assert.That(context, Is.Not.Null);
                        Assert.That(fixture.Bootstrap.HasAdmittedRun, Is.True,
                            fixture.Bootstrap.LastAdmissionError);
                        Assert.That(fixture.FactAdapter.IsBound, Is.True,
                            fixture.FactAdapter.LastFactError);
                        Assert.That(fixture.Presenter.HasCanonicalStageRun, Is.True,
                            fixture.Presenter.CanonicalStageRunBindingError);
                        Assert.That(context.CurrentSegmentIndex, Is.Zero);
                        Assert.That(context.IsCurrentSegmentTerminalActive, Is.True);
                        Assert.That(context.PendingHandoffToken, Is.Null);
                        Assert.That(context.SegmentEntryReceipt, Is.Null);
                        Assert.That(context.HandoffTerminalReceipt, Is.Null);

                        if (cases[caseIndex].FailRun)
                        {
                            Assert.That(
                                ApplyLethalDamage(fixture.PlayerHealth, DamageTeam.Enemy),
                                Is.True);
                        }
                        else
                        {
                            Assert.That(
                                fixture.PlayerHealth.TryApplyDamage(new DamageInfo(
                                    null,
                                    DamageTeam.Enemy,
                                    4.5f,
                                    fixture.PlayerHealth.transform.position,
                                    Vector3.forward,
                                    0f,
                                    DamageResponsePolicy.DamageOnly,
                                    CombatControlLockPolicy.None)),
                                Is.True);
                            Assert.That(
                                ApplyLethalDamage(fixture.EnemyHealth, DamageTeam.Player),
                                Is.True);
                        }

                        yield return null;
                        StageRunResultSummary summary = fixture.Presenter.CommittedSummary;
                        Assert.That(summary, Is.Not.Null, fixture.Presenter.LastCommitError);
                        Assert.That(fixture.Presenter.CommitReceipt, Is.Not.Null);
                        Assert.That(fixture.ResultOverlay.ShowCount, Is.EqualTo(1));
                        Assert.That(fixture.ResultOverlay.Summary, Is.SameAs(summary));
                        Assert.That(fixture.ResultSurface.DismissCount, Is.EqualTo(1));
                        Assert.That(
                            summary.Outcome,
                            Is.EqualTo(cases[caseIndex].FailRun
                                ? StageRouteOutcome.Fail
                                : StageRouteOutcome.Clear));
                        Assert.That(summary.HasTutorialRouteSummaryFact, Is.False);
                        Assert.That(summary.TutorialRouteSummaryFactDigest, Is.Empty);
                        Assert.That(summary.SegmentResultCount, Is.EqualTo(1));
                        Assert.That(summary.GetSegmentResult(0).Completed, Is.True);
                        if (!cases[caseIndex].FailRun)
                        {
                            Assert.That(
                                summary.CombatFacts.PlayerDamageTaken,
                                Is.EqualTo(4.5d).Within(0.0001d));
                        }

                        Assert.That(
                            context.LifecycleState,
                            Is.EqualTo(StageRunLifecycleState.Presented));
                        Assert.That(context.ResultPresentationSnapshot, Is.Not.Null);
                        Assert.That(context.ResultPresentationAudit, Is.Not.Null);

                        var loader = new RecordingOneRowSceneLoader();
                        StageRunRuntime.SetSceneLoaderForTests(loader);
                        IStageRunUiRouteResolver resolver = cases[caseIndex].Kind
                            == StageRouteActionKind.UIRoute
                                ? new FixedOneRowUiRouteResolver(
                                    StageUiRouteId.Lobby,
                                    "UI_Lobby",
                                    LobbyScenePath)
                                : null;
                        Assert.That(
                            StageRunRuntime.TryDispatchTerminalAction(
                                summary,
                                cases[caseIndex].ActionId,
                                resolver,
                                out StageRunResolvedTerminalAction selection,
                                out string dispatchError),
                            Is.True,
                            dispatchError);
                        Assert.That(selection.ActionKind, Is.EqualTo(cases[caseIndex].Kind));
                        Assert.That(loader.CallCount, Is.EqualTo(1));
                        Assert.That(
                            loader.ScenePath,
                            Is.EqualTo(cases[caseIndex].Kind == StageRouteActionKind.UIRoute
                                ? LobbyScenePath
                                : CorridorScenePath));
                        Assert.That(StageRunRuntime.ActiveContext, Is.Null);
                    }
                    finally
                    {
                        fixture.Destroy();
                        StageRunRuntime.ResetForTests();
                    }
                }
            }
            finally
            {
                StageRunRuntime.ResetForTests();
                DestroyOneRowEntryFinalRoute(route);
            }
        }

        [UnityTest]
        [Timeout(30000)]
        public IEnumerator OneRowCommitRecoverySurvivesAndPublishesOnceWhilePresenterDisabled()
        {
            StageRunRuntime.ResetForTests();
            yield return LoadSingleScene(CorridorScenePath);

            Scene hostScene = SceneManager.GetActiveScene();
            OlympusCorridorCombatFlowController flow =
                RequireSingleSceneComponent<OlympusCorridorCombatFlowController>(hostScene);
            PlayableStageDefinition source = ReadPrivateField<PlayableStageDefinition>(
                flow,
                "playableStageDefinition");
            DisableOneRowHostRuntime(hostScene);
            StageRunRuntime.ResetForTests();

            PlayableStageDefinition route = CreateOneRowEntryFinalRoute(source);
            OneRowAdapterFixture fixture = CreateOneRowAdapterFixture(
                hostScene,
                route,
                "Recovery");
            try
            {
                yield return null;
                yield return null;
                StageRunContext context = StageRunRuntime.ActiveContext;
                Assert.That(context, Is.Not.Null);
                StageRunRuntime.InjectTransientResultDecisionIoFailuresForTests(0, 7);

                Assert.That(
                    ApplyLethalDamage(fixture.EnemyHealth, DamageTeam.Player),
                    Is.True);
                Assert.That(
                    context.LifecycleState,
                    Is.EqualTo(StageRunLifecycleState.CommitRecoveryPending));
                Assert.That(fixture.ResultOverlay.ShowCount, Is.Zero);

                fixture.Presenter.enabled = false;
                float timeoutAt = Time.realtimeSinceStartup + 10f;
                while (context.LifecycleState == StageRunLifecycleState.CommitRecoveryPending
                    && Time.realtimeSinceStartup < timeoutAt)
                {
                    yield return null;
                }

                Assert.That(context.LifecycleState, Is.EqualTo(StageRunLifecycleState.Presented));
                Assert.That(context.CommittedSummary, Is.Not.Null);
                Assert.That(context.CommitReceipt, Is.Not.Null);
                Assert.That(context.ResultPresentationSnapshot, Is.Not.Null);
                Assert.That(context.ResultPresentationAudit, Is.Not.Null);
                Assert.That(fixture.ResultOverlay.ShowCount, Is.EqualTo(1));
                Assert.That(fixture.ResultOverlay.Summary, Is.SameAs(context.CommittedSummary));
                Assert.That(fixture.ResultSurface.DismissCount, Is.EqualTo(1));

                fixture.Presenter.enabled = true;
                yield return null;
                Assert.That(fixture.Presenter.CommittedSummary, Is.SameAs(context.CommittedSummary));
                Assert.That(fixture.Presenter.CommitReceipt, Is.SameAs(context.CommitReceipt));
                Assert.That(fixture.ResultOverlay.ShowCount, Is.EqualTo(1));
                Assert.That(fixture.ResultOverlay.Summary, Is.SameAs(context.CommittedSummary));
                Assert.That(fixture.ResultSurface.DismissCount, Is.EqualTo(1));

                fixture.Presenter.enabled = false;
                fixture.Presenter.enabled = true;
                yield return null;
                Assert.That(fixture.ResultOverlay.ShowCount, Is.EqualTo(1));
                Assert.That(fixture.ResultSurface.DismissCount, Is.EqualTo(1));
            }
            finally
            {
                fixture.Destroy();
                StageRunRuntime.ResetForTests();
                DestroyOneRowEntryFinalRoute(route);
            }
        }

        [UnityTest]
        [Timeout(30000)]
        public IEnumerator OneRowAdapterLossAbortsExactFactOrPresenterOwner()
        {
            StageRunRuntime.ResetForTests();
            yield return LoadSingleScene(CorridorScenePath);

            Scene hostScene = SceneManager.GetActiveScene();
            OlympusCorridorCombatFlowController flow =
                RequireSingleSceneComponent<OlympusCorridorCombatFlowController>(hostScene);
            PlayableStageDefinition source = ReadPrivateField<PlayableStageDefinition>(
                flow,
                "playableStageDefinition");
            DisableOneRowHostRuntime(hostScene);
            StageRunRuntime.ResetForTests();

            PlayableStageDefinition route = CreateOneRowEntryFinalRoute(source);
            StageRunAbortReason[] expectedReasons =
            {
                StageRunAbortReason.TerminalFactAdapterLost,
                StageRunAbortReason.TerminalResultPresenterLost
            };
            try
            {
                for (int caseIndex = 0; caseIndex < expectedReasons.Length; caseIndex++)
                {
                    StageRunRuntime.ResetForTests();
                    OneRowAdapterFixture fixture = CreateOneRowAdapterFixture(
                        hostScene,
                        route,
                        $"Loss_{caseIndex}");
                    try
                    {
                        yield return null;
                        yield return null;
                        StageRunContext context = StageRunRuntime.ActiveContext;
                        Assert.That(context, Is.Not.Null);
                        Assert.That(context.LifecycleState, Is.EqualTo(StageRunLifecycleState.StationActive));
                        Assert.That(fixture.Encounter.TerminalCoordinator, Is.Not.Null);

                        switch (caseIndex)
                        {
                            case 0:
                                fixture.FactAdapter.enabled = false;
                                break;
                            default:
                                fixture.Presenter.enabled = false;
                                break;
                        }

                        Assert.That(StageRunRuntime.LastAbortRecord, Is.Not.Null);
                        Assert.That(
                            StageRunRuntime.LastAbortRecord.AbortReason,
                            Is.EqualTo(expectedReasons[caseIndex]));
                        Assert.That(StageRunRuntime.LastAbortRecord.HasValidIntegrity(), Is.True);
                        Assert.That(context.LifecycleState, Is.EqualTo(StageRunLifecycleState.Disposed));
                        Assert.That(context.CommittedSummary, Is.Null);
                        Assert.That(context.CommitReceipt, Is.Null);
                    }
                    finally
                    {
                        fixture.Destroy();
                        StageRunRuntime.ResetForTests();
                    }
                }
            }
            finally
            {
                StageRunRuntime.ResetForTests();
                DestroyOneRowEntryFinalRoute(route);
            }
        }

        [UnityTest]
        [Timeout(30000)]
        public IEnumerator OneRowBootstrapRejectsIncompleteSceneBeforeAdmission()
        {
            StageRunRuntime.ResetForTests();
            yield return LoadSingleScene(CorridorScenePath);

            Scene hostScene = SceneManager.GetActiveScene();
            OlympusCorridorCombatFlowController flow =
                RequireSingleSceneComponent<OlympusCorridorCombatFlowController>(hostScene);
            PlayableStageDefinition source = ReadPrivateField<PlayableStageDefinition>(
                flow,
                "playableStageDefinition");
            DisableOneRowHostRuntime(hostScene);
            StageRunRuntime.ResetForTests();

            PlayableStageDefinition route = CreateOneRowEntryFinalRoute(source);
            Action<OneRowAdapterFixture>[] defects =
            {
                fixture => fixture.FactAdapter.enabled = false,
                fixture => fixture.Presenter.enabled = false,
                fixture => fixture.PlayerHealth.gameObject.SetActive(false),
                fixture => fixture.ResultSurface.enabled = false,
                fixture => fixture.ResultOverlay.enabled = false
            };
            try
            {
                for (int caseIndex = 0; caseIndex < defects.Length; caseIndex++)
                {
                    StageRunRuntime.ResetForTests();
                    OneRowAdapterFixture fixture = CreateOneRowAdapterFixture(
                        hostScene,
                        route,
                        $"StartupReject_{caseIndex}",
                        defects[caseIndex]);
                    try
                    {
                        yield return null;
                        yield return null;

                        Assert.That(fixture.Bootstrap.HasAdmittedRun, Is.False);
                        Assert.That(fixture.Bootstrap.LastAdmissionError, Is.Not.Empty);
                        Assert.That(StageRunRuntime.ActiveContext, Is.Null);
                        Assert.That(StageRunRuntime.LastAbortRecord, Is.Null);
                        Assert.That(fixture.Encounter.enabled, Is.False);
                        Assert.That(fixture.Encounter.TerminalCoordinator, Is.Null);
                        Assert.That(fixture.ResultOverlay.ShowCount, Is.Zero);
                    }
                    finally
                    {
                        fixture.Destroy();
                        StageRunRuntime.ResetForTests();
                    }
                }
            }
            finally
            {
                StageRunRuntime.ResetForTests();
                DestroyOneRowEntryFinalRoute(route);
            }
        }

        [UnityTest]
        [Timeout(30000)]
        public IEnumerator OneRowInvalidDuplicateBootstrapCannotDisableAdmittedEncounter()
        {
            StageRunRuntime.ResetForTests();
            yield return LoadSingleScene(CorridorScenePath);

            Scene hostScene = SceneManager.GetActiveScene();
            OlympusCorridorCombatFlowController flow =
                RequireSingleSceneComponent<OlympusCorridorCombatFlowController>(hostScene);
            PlayableStageDefinition source = ReadPrivateField<PlayableStageDefinition>(
                flow,
                "playableStageDefinition");
            DisableOneRowHostRuntime(hostScene);
            StageRunRuntime.ResetForTests();

            PlayableStageDefinition route = CreateOneRowEntryFinalRoute(source);
            OneRowAdapterFixture fixture = CreateOneRowAdapterFixture(
                hostScene,
                route,
                "DuplicateBootstrapPrimary");
            var duplicateRoot = new GameObject("B0_3_InvalidDuplicateBootstrap");
            duplicateRoot.SetActive(false);
            SceneManager.MoveGameObjectToScene(duplicateRoot, hostScene);
            try
            {
                yield return null;
                yield return null;
                StageRunContext context = StageRunRuntime.ActiveContext;
                Assert.That(context, Is.Not.Null);
                EncounterTerminalResolutionCoordinator primaryCoordinator =
                    fixture.Encounter.TerminalCoordinator;
                Assert.That(primaryCoordinator, Is.Not.Null);

                OneRowStageRunBootstrap duplicate =
                    duplicateRoot.AddComponent<OneRowStageRunBootstrap>();
                SetPrivateField(duplicate, "playableStageDefinition", route);
                SetPrivateField(duplicate, "encounter", fixture.Encounter);
                duplicateRoot.SetActive(true);
                yield return null;

                Assert.That(duplicate.HasAdmittedRun, Is.False);
                Assert.That(duplicate.LastAdmissionError, Is.Not.Empty);
                Assert.That(StageRunRuntime.ActiveContext, Is.SameAs(context));
                Assert.That(context.LifecycleState, Is.EqualTo(StageRunLifecycleState.StationActive));
                Assert.That(context.FaultReason, Is.Empty);
                Assert.That(StageRunRuntime.LastAbortRecord, Is.Null);
                Assert.That(fixture.Encounter.enabled, Is.True);
                Assert.That(fixture.Encounter.IsRunning, Is.True);
                Assert.That(fixture.Encounter.TerminalCoordinator, Is.SameAs(primaryCoordinator));

                Assert.That(
                    ApplyLethalDamage(fixture.EnemyHealth, DamageTeam.Player),
                    Is.True);
                yield return null;
                Assert.That(fixture.Presenter.CommittedSummary, Is.Not.Null);
                Assert.That(context.LifecycleState, Is.EqualTo(StageRunLifecycleState.Presented));
                Assert.That(fixture.ResultOverlay.ShowCount, Is.EqualTo(1));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(duplicateRoot);
                fixture.Destroy();
                StageRunRuntime.ResetForTests();
                DestroyOneRowEntryFinalRoute(route);
            }
        }

        [UnityTest]
        [Timeout(30000)]
        public IEnumerator OneRowAdapterLossBeforeCoordinatorStartAbortsAndStopsEncounter()
        {
            StageRunRuntime.ResetForTests();
            yield return LoadSingleScene(CorridorScenePath);

            Scene hostScene = SceneManager.GetActiveScene();
            OlympusCorridorCombatFlowController flow =
                RequireSingleSceneComponent<OlympusCorridorCombatFlowController>(hostScene);
            PlayableStageDefinition source = ReadPrivateField<PlayableStageDefinition>(
                flow,
                "playableStageDefinition");
            DisableOneRowHostRuntime(hostScene);
            StageRunRuntime.ResetForTests();

            PlayableStageDefinition route = CreateOneRowEntryFinalRoute(source);
            StageRunAbortReason[] expectedReasons =
            {
                StageRunAbortReason.TerminalFactAdapterLost,
                StageRunAbortReason.TerminalResultPresenterLost
            };
            try
            {
                for (int caseIndex = 0; caseIndex < expectedReasons.Length; caseIndex++)
                {
                    StageRunRuntime.ResetForTests();
                    int capturedCase = caseIndex;
                    OneRowAdapterFixture fixture = CreateOneRowAdapterFixture(
                        hostScene,
                        route,
                        $"PreCoordinatorLoss_{caseIndex}",
                        configuredFixture =>
                        {
                            OneRowPreCoordinatorAdapterDisableProbe probe =
                                configuredFixture.Root.AddComponent<
                                    OneRowPreCoordinatorAdapterDisableProbe>();
                            probe.Target = capturedCase == 0
                                ? configuredFixture.FactAdapter
                                : configuredFixture.Presenter;
                        });
                    try
                    {
                        yield return null;
                        yield return null;

                        StageRunContext context = StageRunRuntime.ActiveContext;
                        Assert.That(context, Is.Not.Null);
                        Assert.That(StageRunRuntime.LastAbortRecord, Is.Not.Null);
                        Assert.That(
                            StageRunRuntime.LastAbortRecord.AbortReason,
                            Is.EqualTo(expectedReasons[caseIndex]));
                        Assert.That(
                            context.AbortCloseAuthority.CoordinatorInvalidationDisposition,
                            Is.EqualTo(
                                StageRunTerminalCoordinatorInvalidationDisposition
                                    .NotBoundBeforeTerminalCoordinator));
                        Assert.That(context.LifecycleState, Is.EqualTo(StageRunLifecycleState.Disposed));
                        Assert.That(context.CommittedSummary, Is.Null);
                        Assert.That(fixture.Encounter.enabled, Is.False);
                        Assert.That(fixture.Encounter.TerminalCoordinator, Is.Null);
                        Assert.That(fixture.ResultOverlay.ShowCount, Is.Zero);
                    }
                    finally
                    {
                        fixture.Destroy();
                        StageRunRuntime.ResetForTests();
                    }
                }
            }
            finally
            {
                StageRunRuntime.ResetForTests();
                DestroyOneRowEntryFinalRoute(route);
            }
        }

        [UnityTest]
        [Timeout(30000)]
        public IEnumerator OneRowSealedFactSourceLossAbortsAndStopsEncounter()
        {
            StageRunRuntime.ResetForTests();
            yield return LoadSingleScene(CorridorScenePath);

            Scene hostScene = SceneManager.GetActiveScene();
            OlympusCorridorCombatFlowController flow =
                RequireSingleSceneComponent<OlympusCorridorCombatFlowController>(hostScene);
            PlayableStageDefinition source = ReadPrivateField<PlayableStageDefinition>(
                flow,
                "playableStageDefinition");
            DisableOneRowHostRuntime(hostScene);
            StageRunRuntime.ResetForTests();

            PlayableStageDefinition route = CreateOneRowEntryFinalRoute(source);
            OneRowAdapterFixture fixture = CreateOneRowAdapterFixture(
                hostScene,
                route,
                "FactSourceLoss");
            try
            {
                yield return null;
                yield return null;
                StageRunContext context = StageRunRuntime.ActiveContext;
                Assert.That(context, Is.Not.Null);
                Assert.That(fixture.FactAdapter.IsBound, Is.True);
                Assert.That(fixture.Encounter.TerminalCoordinator, Is.Not.Null);

                fixture.ResultSurface.enabled = false;
                yield return null;

                Assert.That(StageRunRuntime.LastAbortRecord, Is.Not.Null);
                Assert.That(
                    StageRunRuntime.LastAbortRecord.AbortReason,
                    Is.EqualTo(StageRunAbortReason.TerminalFactAdapterLost));
                Assert.That(
                    context.AbortCloseAuthority.CoordinatorInvalidationDisposition,
                    Is.EqualTo(
                        StageRunTerminalCoordinatorInvalidationDisposition
                            .CancellationRequested));
                Assert.That(context.LifecycleState, Is.EqualTo(StageRunLifecycleState.Disposed));
                Assert.That(context.CommittedSummary, Is.Null);
                Assert.That(context.ResultPresentationSnapshot, Is.Null);
                Assert.That(fixture.Encounter.enabled, Is.False);
                Assert.That(fixture.Encounter.TerminalCoordinator, Is.Null);
                Assert.That(fixture.ResultOverlay.ShowCount, Is.Zero);
            }
            finally
            {
                fixture.Destroy();
                StageRunRuntime.ResetForTests();
                DestroyOneRowEntryFinalRoute(route);
            }
        }

        [UnityTest]
        [Timeout(30000)]
        public IEnumerator OneRowFactSourceSealRejectsRuntimeReferenceSwapWithoutRebinding()
        {
            StageRunRuntime.ResetForTests();
            yield return LoadSingleScene(CorridorScenePath);

            Scene hostScene = SceneManager.GetActiveScene();
            OlympusCorridorCombatFlowController flow =
                RequireSingleSceneComponent<OlympusCorridorCombatFlowController>(hostScene);
            PlayableStageDefinition source = ReadPrivateField<PlayableStageDefinition>(
                flow,
                "playableStageDefinition");
            DisableOneRowHostRuntime(hostScene);
            StageRunRuntime.ResetForTests();

            PlayableStageDefinition route = CreateOneRowEntryFinalRoute(source);
            OneRowAdapterFixture fixture = CreateOneRowAdapterFixture(
                hostScene,
                route,
                "FactSealReferenceSwap");
            try
            {
                yield return null;
                yield return null;
                Assert.That(fixture.FactAdapter.IsBound, Is.True);

                RecordingCombatSessionOverlay alternateSurface =
                    fixture.Root.AddComponent<RecordingCombatSessionOverlay>();
                SetPrivateField(
                    fixture.FactAdapter,
                    "resultSurfaceBehaviour",
                    alternateSurface);
                Assert.That(fixture.FactAdapter.BindToActiveRun(), Is.False);
                Assert.That(fixture.FactAdapter.LastFactError, Does.Contain("sealed"));

                SetPrivateField(
                    fixture.FactAdapter,
                    "resultSurfaceBehaviour",
                    fixture.ResultSurface);
                Assert.That(
                    fixture.FactAdapter.BindToActiveRun(),
                    Is.True,
                    fixture.FactAdapter.LastFactError);

                Assert.That(
                    fixture.PlayerHealth.TryApplyDamage(new DamageInfo(
                        null,
                        DamageTeam.Enemy,
                        6.5f,
                        fixture.PlayerHealth.transform.position,
                        Vector3.forward,
                        0f,
                        DamageResponsePolicy.DamageOnly,
                        CombatControlLockPolicy.None)),
                    Is.True);
                Assert.That(
                    ApplyLethalDamage(fixture.EnemyHealth, DamageTeam.Player),
                    Is.True);
                yield return null;

                Assert.That(fixture.Presenter.CommittedSummary, Is.Not.Null);
                Assert.That(
                    fixture.Presenter.CommittedSummary.CombatFacts.PlayerDamageTaken,
                    Is.EqualTo(6.5d).Within(0.0001d));
                Assert.That(fixture.ResultOverlay.ShowCount, Is.EqualTo(1));
            }
            finally
            {
                fixture.Destroy();
                StageRunRuntime.ResetForTests();
                DestroyOneRowEntryFinalRoute(route);
            }
        }

        [UnityTest]
        [Timeout(30000)]
        public IEnumerator OneRowTerminalRescanRejectsNewlyActivatedFactSource()
        {
            StageRunRuntime.ResetForTests();
            yield return LoadSingleScene(CorridorScenePath);

            Scene hostScene = SceneManager.GetActiveScene();
            OlympusCorridorCombatFlowController flow =
                RequireSingleSceneComponent<OlympusCorridorCombatFlowController>(hostScene);
            PlayableStageDefinition source = ReadPrivateField<PlayableStageDefinition>(
                flow,
                "playableStageDefinition");
            DisableOneRowHostRuntime(hostScene);
            StageRunRuntime.ResetForTests();

            PlayableStageDefinition route = CreateOneRowEntryFinalRoute(source);
            GameObject lateActionObject = null;
            OneRowAdapterFixture fixture = CreateOneRowAdapterFixture(
                hostScene,
                route,
                "TerminalFactRescan",
                configuredFixture =>
                {
                    lateActionObject = new GameObject("LatePlayerActionSource");
                    lateActionObject.SetActive(false);
                    lateActionObject.transform.SetParent(
                        configuredFixture.PlayerHealth.transform,
                        false);
                    lateActionObject.AddComponent<PlayerActionController>();
                });
            try
            {
                yield return null;
                yield return null;
                StageRunContext context = StageRunRuntime.ActiveContext;
                Assert.That(context, Is.Not.Null);
                Assert.That(fixture.FactAdapter.IsBound, Is.True);

                lateActionObject.SetActive(true);
                Assert.That(
                    ApplyLethalDamage(fixture.EnemyHealth, DamageTeam.Player),
                    Is.True);
                yield return null;

                Assert.That(StageRunRuntime.LastAbortRecord, Is.Not.Null);
                Assert.That(
                    StageRunRuntime.LastAbortRecord.AbortReason,
                    Is.EqualTo(StageRunAbortReason.TerminalFactAdapterLost));
                Assert.That(
                    context.AbortCloseAuthority.CoordinatorInvalidationDisposition,
                    Is.EqualTo(
                        StageRunTerminalCoordinatorInvalidationDisposition
                            .TerminalAuthorityInvalidated));
                Assert.That(context.LifecycleState, Is.EqualTo(StageRunLifecycleState.Disposed));
                Assert.That(context.CommittedSummary, Is.Null);
                Assert.That(fixture.Encounter.enabled, Is.False);
                Assert.That(fixture.Encounter.TerminalCoordinator, Is.Null);
                Assert.That(fixture.ResultOverlay.ShowCount, Is.Zero);
            }
            finally
            {
                fixture.Destroy();
                StageRunRuntime.ResetForTests();
                DestroyOneRowEntryFinalRoute(route);
            }
        }

        [UnityTest]
        [Timeout(30000)]
        public IEnumerator OneRowTerminalClosedEncounterStopSealsTruthfulAbort()
        {
            StageRunRuntime.ResetForTests();
            yield return LoadSingleScene(CorridorScenePath);

            Scene hostScene = SceneManager.GetActiveScene();
            OlympusCorridorCombatFlowController flow =
                RequireSingleSceneComponent<OlympusCorridorCombatFlowController>(hostScene);
            PlayableStageDefinition source = ReadPrivateField<PlayableStageDefinition>(
                flow,
                "playableStageDefinition");
            DisableOneRowHostRuntime(hostScene);
            StageRunRuntime.ResetForTests();

            PlayableStageDefinition route = CreateOneRowEntryFinalRoute(source);
            OneRowAdapterFixture fixture = CreateOneRowAdapterFixture(
                hostScene,
                route,
                "TerminalClosedStop",
                configuredFixture =>
                    configuredFixture.Encounter.TerminalResolved +=
                        _ => configuredFixture.Encounter.enabled = false);
            try
            {
                yield return null;
                yield return null;
                StageRunContext context = StageRunRuntime.ActiveContext;
                Assert.That(context, Is.Not.Null);

                Assert.That(
                    ApplyLethalDamage(fixture.EnemyHealth, DamageTeam.Player),
                    Is.True);
                yield return null;

                Assert.That(StageRunRuntime.LastAbortRecord, Is.Not.Null);
                Assert.That(
                    StageRunRuntime.LastAbortRecord.AbortReason,
                    Is.EqualTo(StageRunAbortReason.UnexpectedSceneExit));
                Assert.That(
                    context.AbortCloseAuthority.CoordinatorInvalidationDisposition,
                    Is.EqualTo(
                        StageRunTerminalCoordinatorInvalidationDisposition
                            .TerminalAuthorityInvalidated));
                Assert.That(context.LifecycleState, Is.EqualTo(StageRunLifecycleState.Disposed));
                Assert.That(context.CommittedSummary, Is.Null);
                Assert.That(fixture.Encounter.enabled, Is.False);
                Assert.That(fixture.Encounter.TerminalCoordinator, Is.Null);
                Assert.That(fixture.ResultOverlay.ShowCount, Is.Zero);
            }
            finally
            {
                fixture.Destroy();
                StageRunRuntime.ResetForTests();
                DestroyOneRowEntryFinalRoute(route);
            }
        }

        [UnityTest]
        [Timeout(30000)]
        public IEnumerator OneRowRecoveryPumpPublishesRunCommittedByConcurrentRecovery()
        {
            StageRunRuntime.ResetForTests();
            yield return LoadSingleScene(CorridorScenePath);

            Scene hostScene = SceneManager.GetActiveScene();
            OlympusCorridorCombatFlowController flow =
                RequireSingleSceneComponent<OlympusCorridorCombatFlowController>(hostScene);
            PlayableStageDefinition source = ReadPrivateField<PlayableStageDefinition>(
                flow,
                "playableStageDefinition");
            DisableOneRowHostRuntime(hostScene);
            StageRunRuntime.ResetForTests();

            PlayableStageDefinition route = CreateOneRowEntryFinalRoute(source);
            OneRowAdapterFixture fixture = CreateOneRowAdapterFixture(
                hostScene,
                route,
                "ConcurrentRecovery");
            try
            {
                yield return null;
                yield return null;
                StageRunContext context = StageRunRuntime.ActiveContext;
                Assert.That(context, Is.Not.Null);
                StageRunRuntime.InjectTransientResultDecisionIoFailuresForTests(0, 1);

                Assert.That(
                    ApplyLethalDamage(fixture.EnemyHealth, DamageTeam.Player),
                    Is.True);
                Assert.That(
                    context.LifecycleState,
                    Is.EqualTo(StageRunLifecycleState.CommitRecoveryPending));
                Assert.That(fixture.ResultOverlay.ShowCount, Is.Zero);

                Assert.That(
                    StageRunRuntime.TryRecoverPendingResultCommit(
                        out StageRunResultSummary recoveredSummary,
                        out StageRunResultCommitReceipt recoveredReceipt,
                        out string recoveryError),
                    Is.True,
                    recoveryError);
                Assert.That(context.LifecycleState, Is.EqualTo(StageRunLifecycleState.Committed));
                Assert.That(recoveredSummary, Is.SameAs(context.CommittedSummary));
                Assert.That(recoveredReceipt, Is.SameAs(context.CommitReceipt));

                float timeoutAt = Time.realtimeSinceStartup + 3f;
                while (context.LifecycleState != StageRunLifecycleState.Presented
                    && Time.realtimeSinceStartup < timeoutAt)
                {
                    yield return null;
                }

                Assert.That(context.LifecycleState, Is.EqualTo(StageRunLifecycleState.Presented));
                Assert.That(fixture.ResultOverlay.ShowCount, Is.EqualTo(1));
                Assert.That(fixture.ResultOverlay.Summary, Is.SameAs(recoveredSummary));
                Assert.That(fixture.ResultSurface.DismissCount, Is.EqualTo(1));
            }
            finally
            {
                fixture.Destroy();
                StageRunRuntime.ResetForTests();
                DestroyOneRowEntryFinalRoute(route);
            }
        }

        [UnityTest]
        [Timeout(30000)]
        public IEnumerator OneRowPresentedEncounterCannotRestartCanonicalCombat()
        {
            StageRunRuntime.ResetForTests();
            yield return LoadSingleScene(CorridorScenePath);

            Scene hostScene = SceneManager.GetActiveScene();
            OlympusCorridorCombatFlowController flow =
                RequireSingleSceneComponent<OlympusCorridorCombatFlowController>(hostScene);
            PlayableStageDefinition source = ReadPrivateField<PlayableStageDefinition>(
                flow,
                "playableStageDefinition");
            DisableOneRowHostRuntime(hostScene);
            StageRunRuntime.ResetForTests();

            PlayableStageDefinition route = CreateOneRowEntryFinalRoute(source);
            OneRowAdapterFixture fixture = CreateOneRowAdapterFixture(
                hostScene,
                route,
                "PresentedRestart");
            try
            {
                yield return null;
                yield return null;
                StageRunContext context = StageRunRuntime.ActiveContext;
                Assert.That(
                    ApplyLethalDamage(fixture.EnemyHealth, DamageTeam.Player),
                    Is.True);
                yield return null;
                Assert.That(context.LifecycleState, Is.EqualTo(StageRunLifecycleState.Presented));
                StageRunResultSummary summary = context.CommittedSummary;
                Assert.That(summary, Is.Not.Null);

                fixture.Encounter.enabled = false;
                fixture.Encounter.enabled = true;
                yield return null;

                Assert.That(fixture.Encounter.enabled, Is.False);
                Assert.That(fixture.Encounter.TerminalCoordinator, Is.Null);
                Assert.That(context.LifecycleState, Is.EqualTo(StageRunLifecycleState.Presented));
                Assert.That(context.CommittedSummary, Is.SameAs(summary));
                Assert.That(StageRunRuntime.LastAbortRecord, Is.Null);
                Assert.That(fixture.ResultOverlay.ShowCount, Is.EqualTo(1));
            }
            finally
            {
                fixture.Destroy();
                StageRunRuntime.ResetForTests();
                DestroyOneRowEntryFinalRoute(route);
            }
        }

        [UnityTest]
        [Timeout(30000)]
        public IEnumerator OneRowResultPresentationRetriesPastInitialFailuresUntilAcknowledged()
        {
            StageRunRuntime.ResetForTests();
            yield return LoadSingleScene(CorridorScenePath);

            Scene hostScene = SceneManager.GetActiveScene();
            OlympusCorridorCombatFlowController flow =
                RequireSingleSceneComponent<OlympusCorridorCombatFlowController>(hostScene);
            PlayableStageDefinition source = ReadPrivateField<PlayableStageDefinition>(
                flow,
                "playableStageDefinition");
            DisableOneRowHostRuntime(hostScene);
            StageRunRuntime.ResetForTests();

            PlayableStageDefinition route = CreateOneRowEntryFinalRoute(source);
            OneRowAdapterFixture fixture = CreateOneRowAdapterFixture(
                hostScene,
                route,
                "PresentationRetry");
            try
            {
                yield return null;
                yield return null;
                StageRunContext context = StageRunRuntime.ActiveContext;
                Assert.That(context, Is.Not.Null);
                fixture.ResultOverlay.FailuresRemaining = 4;

                Assert.That(
                    ApplyLethalDamage(fixture.EnemyHealth, DamageTeam.Player),
                    Is.True);
                float timeoutAt = Time.realtimeSinceStartup + 10f;
                while (context.LifecycleState != StageRunLifecycleState.Presented
                    && Time.realtimeSinceStartup < timeoutAt)
                {
                    yield return null;
                }

                Assert.That(context.LifecycleState, Is.EqualTo(StageRunLifecycleState.Presented));
                Assert.That(context.CommittedSummary, Is.Not.Null);
                Assert.That(context.ResultPresentationSnapshot, Is.Not.Null);
                Assert.That(context.ResultPresentationAudit, Is.Not.Null);
                Assert.That(fixture.ResultOverlay.ShowCount, Is.EqualTo(5));
                Assert.That(fixture.ResultOverlay.FailuresRemaining, Is.Zero);
                Assert.That(
                    fixture.ResultOverlay.PresentedResultDigest,
                    Is.EqualTo(context.CommittedSummary.ResultSummaryDigest));
                Assert.That(fixture.ResultSurface.DismissCount, Is.EqualTo(1));
                Assert.That(fixture.Presenter.LastCommitError, Is.Empty);
            }
            finally
            {
                fixture.Destroy();
                StageRunRuntime.ResetForTests();
                DestroyOneRowEntryFinalRoute(route);
            }
        }

        [UnityTest]
        [Timeout(30000)]
        public IEnumerator OneRowResultPresentationWatchdogRecoversLostPendingCallback()
        {
            StageRunRuntime.ResetForTests();
            yield return LoadSingleScene(CorridorScenePath);

            Scene hostScene = SceneManager.GetActiveScene();
            OlympusCorridorCombatFlowController flow =
                RequireSingleSceneComponent<OlympusCorridorCombatFlowController>(hostScene);
            PlayableStageDefinition source = ReadPrivateField<PlayableStageDefinition>(
                flow,
                "playableStageDefinition");
            DisableOneRowHostRuntime(hostScene);
            StageRunRuntime.ResetForTests();

            PlayableStageDefinition route = CreateOneRowEntryFinalRoute(source);
            OneRowAdapterFixture fixture = CreateOneRowAdapterFixture(
                hostScene,
                route,
                "PresentationPendingWatchdog");
            try
            {
                yield return null;
                yield return null;
                StageRunContext context = StageRunRuntime.ActiveContext;
                Assert.That(context, Is.Not.Null);
                fixture.ResultOverlay.AcceptPendingWithoutAcknowledgement = true;

                Assert.That(
                    ApplyLethalDamage(fixture.EnemyHealth, DamageTeam.Player),
                    Is.True);
                yield return null;
                Assert.That(context.LifecycleState, Is.EqualTo(StageRunLifecycleState.Committed));
                Assert.That(fixture.ResultOverlay.ShowCount, Is.EqualTo(1));
                Assert.That(
                    fixture.ResultOverlay.PendingResultDigest,
                    Is.EqualTo(context.CommittedSummary.ResultSummaryDigest));
                Assert.That(fixture.ResultOverlay.PresentedResultDigest, Is.Empty);

                fixture.ResultOverlay.EmitSuccessWithoutAcknowledgement();
                yield return null;
                Assert.That(context.LifecycleState, Is.EqualTo(StageRunLifecycleState.Committed));
                Assert.That(fixture.ResultOverlay.PresentedResultDigest, Is.Empty);

                fixture.ResultOverlay.ReleasePendingWithoutCallback();
                float timeoutAt = Time.realtimeSinceStartup + 5f;
                while (context.LifecycleState != StageRunLifecycleState.Presented
                    && Time.realtimeSinceStartup < timeoutAt)
                {
                    yield return null;
                }

                Assert.That(context.LifecycleState, Is.EqualTo(StageRunLifecycleState.Presented));
                Assert.That(fixture.ResultOverlay.ShowCount, Is.EqualTo(2));
                Assert.That(
                    fixture.ResultOverlay.PresentedResultDigest,
                    Is.EqualTo(context.CommittedSummary.ResultSummaryDigest));
                Assert.That(context.ResultPresentationSnapshot, Is.Not.Null);
                Assert.That(context.ResultPresentationAudit, Is.Not.Null);
                Assert.That(fixture.ResultSurface.DismissCount, Is.EqualTo(1));
            }
            finally
            {
                fixture.Destroy();
                StageRunRuntime.ResetForTests();
                DestroyOneRowEntryFinalRoute(route);
            }
        }

        [UnityTest]
        [Timeout(30000)]
        public IEnumerator OneRowNeutralAdaptersPresentThroughAuthoredStageClearScene()
        {
            StageRunRuntime.ResetForTests();
            yield return LoadSingleScene(CorridorScenePath);

            Scene hostScene = SceneManager.GetActiveScene();
            OlympusCorridorCombatFlowController flow =
                RequireSingleSceneComponent<OlympusCorridorCombatFlowController>(hostScene);
            PlayableStageDefinition source = ReadPrivateField<PlayableStageDefinition>(
                flow,
                "playableStageDefinition");
            DisableOneRowHostRuntime(hostScene);
            StageRunRuntime.ResetForTests();

            PlayableStageDefinition route = CreateOneRowEntryFinalRoute(source);
            OlympusStageClearOverlay productionOverlay = null;
            OneRowAdapterFixture fixture = CreateOneRowAdapterFixture(
                hostScene,
                route,
                "AuthoredStageClear",
                configuredFixture =>
                {
                    productionOverlay =
                        configuredFixture.Root.AddComponent<OlympusStageClearOverlay>();
                    SetPrivateField(productionOverlay, "combatHudExitSeconds", 0f);
                    SetPrivateField(productionOverlay, "postBossDefeatHoldSeconds", 0f);
                    SetPrivateField(
                        configuredFixture.Presenter,
                        "resultOverlayBehaviour",
                        productionOverlay);
                });
            try
            {
                yield return null;
                yield return null;
                StageRunContext context = StageRunRuntime.ActiveContext;
                Assert.That(context, Is.Not.Null);

                Assert.That(
                    ApplyLethalDamage(fixture.EnemyHealth, DamageTeam.Player),
                    Is.True);
                StageClearScreenPresenter stageClearPresenter = null;
                float timeoutAt = Time.realtimeSinceStartup + 8f;
                while (Time.realtimeSinceStartup < timeoutAt)
                {
                    Scene clearScene = SceneManager.GetSceneByName("UI_StageClear");
                    if (clearScene.IsValid() && clearScene.isLoaded)
                    {
                        StageClearScreenPresenter presenter =
                            FindSingleSceneComponentOrNull<StageClearScreenPresenter>(clearScene);
                        if (presenter != null && presenter.IsConfigured)
                        {
                            stageClearPresenter = presenter;
                            break;
                        }
                    }

                    yield return null;
                }

                Assert.That(stageClearPresenter, Is.Not.Null);
                Assert.That(context.LifecycleState, Is.EqualTo(StageRunLifecycleState.Presented));
                Assert.That(productionOverlay.ResultSummary, Is.SameAs(context.CommittedSummary));
                Assert.That(
                    productionOverlay.PresentedResultDigest,
                    Is.EqualTo(context.CommittedSummary.ResultSummaryDigest));
                Assert.That(stageClearPresenter.ResultSummary, Is.SameAs(context.CommittedSummary));
                Assert.That(stageClearPresenter.PresentationSnapshot, Is.SameAs(
                    context.ResultPresentationSnapshot));
                Assert.That(stageClearPresenter.PresentationAudit, Is.SameAs(
                    context.ResultPresentationAudit));
                Assert.That(stageClearPresenter.PrimaryActionId, Is.EqualTo(OneRowReplayActionId));
                Assert.That(stageClearPresenter.LobbyActionId, Is.EqualTo(OneRowLobbyActionId));
                Assert.That(fixture.ResultOverlay.ShowCount, Is.Zero);
            }
            finally
            {
                fixture.Destroy();
                StageRunRuntime.ResetForTests();
                DestroyOneRowEntryFinalRoute(route);
            }
        }

        [UnityTest]
        public IEnumerator OneRowAdapterLeasesRejectDuplicateOwnersAndForeignPlayerFacts()
        {
            StageRunRuntime.ResetForTests();
            yield return LoadSingleScene(CorridorScenePath);

            Scene hostScene = SceneManager.GetActiveScene();
            OlympusCorridorCombatFlowController flow =
                RequireSingleSceneComponent<OlympusCorridorCombatFlowController>(hostScene);
            PlayableStageDefinition source = ReadPrivateField<PlayableStageDefinition>(
                flow,
                "playableStageDefinition");
            DisableOneRowHostRuntime(hostScene);
            StageRunRuntime.ResetForTests();

            PlayableStageDefinition route = CreateOneRowEntryFinalRoute(source);
            OneRowAdapterFixture fixture = CreateOneRowAdapterFixture(
                hostScene,
                route,
                "LeasePrimary");
            var duplicateFactRoot = new GameObject("B0_3_DuplicateFactAdapter");
            var foreignFactRoot = new GameObject("B0_3_ForeignPlayerFactAdapter");
            var duplicatePresenterRoot = new GameObject("B0_3_DuplicateResultPresenter");
            OneRowCombatFixture foreignEncounter = null;
            duplicateFactRoot.SetActive(false);
            foreignFactRoot.SetActive(false);
            duplicatePresenterRoot.SetActive(false);
            SceneManager.MoveGameObjectToScene(duplicateFactRoot, hostScene);
            SceneManager.MoveGameObjectToScene(foreignFactRoot, hostScene);
            SceneManager.MoveGameObjectToScene(duplicatePresenterRoot, hostScene);
            try
            {
                yield return null;
                yield return null;
                Assert.That(fixture.FactAdapter.IsBound, Is.True);
                Assert.That(fixture.Presenter.HasCanonicalStageRun, Is.True);

                OneRowStageRunFactAdapter duplicateFact =
                    duplicateFactRoot.AddComponent<OneRowStageRunFactAdapter>();
                SetPrivateField(duplicateFact, "encounter", fixture.Encounter);
                SetPrivateField(duplicateFact, "playerHealth", fixture.PlayerHealth);
                SetPrivateField(
                    duplicateFact,
                    "supportSummonActions",
                    Array.Empty<PlayerSupportSummonSlotAction>());
                SetPrivateField(
                    duplicateFact,
                    "resultSurfaceBehaviour",
                    fixture.ResultSurface);

                CombatHealth foreignPlayerHealth = foreignFactRoot.AddComponent<CombatHealth>();
                foreignPlayerHealth.ConfigureTeam(DamageTeam.Player);
                foreignPlayerHealth.ConfigureMaxHealth(100f);
                OneRowStageRunFactAdapter foreignFact =
                    foreignFactRoot.AddComponent<OneRowStageRunFactAdapter>();
                SetPrivateField(foreignFact, "encounter", fixture.Encounter);
                SetPrivateField(foreignFact, "playerHealth", foreignPlayerHealth);
                SetPrivateField(
                    foreignFact,
                    "supportSummonActions",
                    Array.Empty<PlayerSupportSummonSlotAction>());
                SetPrivateField(
                    foreignFact,
                    "resultSurfaceBehaviour",
                    fixture.ResultSurface);

                RecordingStageRunResultOverlay duplicateResultOverlay =
                    duplicatePresenterRoot.AddComponent<RecordingStageRunResultOverlay>();
                RecordingCombatSessionOverlay duplicateResultSurface =
                    duplicatePresenterRoot.AddComponent<RecordingCombatSessionOverlay>();
                OneRowStageRunResultPresenter duplicatePresenter =
                    duplicatePresenterRoot.AddComponent<OneRowStageRunResultPresenter>();
                SetPrivateField(duplicatePresenter, "encounter", fixture.Encounter);
                SetPrivateField(
                    duplicatePresenter,
                    "resultOverlayBehaviour",
                    duplicateResultOverlay);
                SetPrivateField(
                    duplicatePresenter,
                    "resultSurfaceBehaviour",
                    duplicateResultSurface);
                SetPrivateField(duplicatePresenter, "factAdapter", fixture.FactAdapter);

                duplicateFactRoot.SetActive(true);
                foreignFactRoot.SetActive(true);
                duplicatePresenterRoot.SetActive(true);
                yield return null;

                Assert.That(duplicateFact.IsBound, Is.False);
                Assert.That(duplicateFact.LastFactError, Does.Contain("different FactCollection"));
                Assert.That(foreignFact.IsBound, Is.False);
                Assert.That(
                    foreignFact.LastFactError,
                    Does.Contain("exact same-scene player health"));
                Assert.That(duplicatePresenter.HasCanonicalStageRun, Is.False);
                Assert.That(
                    duplicatePresenter.CanonicalStageRunBindingError,
                    Does.Contain("different ResultPresentation"));

                EncounterTerminalResolutionCoordinator primaryCoordinator =
                    fixture.Encounter.TerminalCoordinator;
                Assert.That(primaryCoordinator, Is.Not.Null);
                foreignEncounter = CreateOneRowCombatFixture(hostScene, "ForeignAfterAdmission");
                yield return null;
                yield return null;
                Assert.That(foreignEncounter.Encounter.enabled, Is.False);
                Assert.That(foreignEncounter.Encounter.TerminalCoordinator, Is.Null);
                Assert.That(fixture.Encounter.TerminalCoordinator, Is.SameAs(primaryCoordinator));
                Assert.That(primaryCoordinator.State, Is.Not.EqualTo(
                    EncounterTerminalCoordinatorState.Cancelled));
                Assert.That(
                    StageRunRuntime.ActiveContext.LifecycleState,
                    Is.EqualTo(StageRunLifecycleState.StationActive));
                Assert.That(StageRunRuntime.ActiveContext.FaultReason, Is.Empty);
                Assert.That(StageRunRuntime.LastAbortRecord, Is.Null);

                Assert.That(
                    fixture.PlayerHealth.TryApplyDamage(new DamageInfo(
                        null,
                        DamageTeam.Enemy,
                        4.5f,
                        fixture.PlayerHealth.transform.position,
                        Vector3.forward,
                        0f,
                        DamageResponsePolicy.DamageOnly,
                        CombatControlLockPolicy.None)),
                    Is.True);
                Assert.That(
                    ApplyLethalDamage(fixture.EnemyHealth, DamageTeam.Player),
                    Is.True);
                yield return null;

                Assert.That(fixture.Presenter.CommittedSummary, Is.Not.Null);
                Assert.That(
                    fixture.Presenter.CommittedSummary.CombatFacts.PlayerDamageTaken,
                    Is.EqualTo(4.5d).Within(0.0001d));
                Assert.That(fixture.ResultOverlay.ShowCount, Is.EqualTo(1));
                Assert.That(duplicateResultOverlay.ShowCount, Is.Zero);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(duplicateFactRoot);
                UnityEngine.Object.DestroyImmediate(foreignFactRoot);
                UnityEngine.Object.DestroyImmediate(duplicatePresenterRoot);
                foreignEncounter?.Destroy();
                fixture.Destroy();
                StageRunRuntime.ResetForTests();
                DestroyOneRowEntryFinalRoute(route);
            }
        }

        [UnityTest]
        public IEnumerator OneRowNeutralAdaptersNeverAdvanceAnExistingTwoRowContext()
        {
            StageRunRuntime.ResetForTests();
            yield return LoadSingleScene(CorridorScenePath);

            Scene hostScene = SceneManager.GetActiveScene();
            OlympusCorridorCombatFlowController flow =
                RequireSingleSceneComponent<OlympusCorridorCombatFlowController>(hostScene);
            PlayableStageDefinition source = ReadPrivateField<PlayableStageDefinition>(
                flow,
                "playableStageDefinition");
            DisableOneRowHostRuntime(hostScene);
            StageRunRuntime.ResetForTests();

            Assert.That(
                StageRunRuntime.TryAdmitFirstSegment(
                    source,
                    hostScene,
                    out StageRunContext context,
                    out string admissionError),
                Is.True,
                admissionError);
            Assert.That(context.LifecycleState, Is.EqualTo(StageRunLifecycleState.CorridorActive));
            Assert.That(context.CurrentSegmentIndex, Is.Zero);

            OneRowAdapterFixture fixture = CreateOneRowAdapterFixture(
                hostScene,
                source,
                "TwoRowRejected");
            try
            {
                yield return null;
                yield return null;
                Assert.That(StageRunRuntime.ActiveContext, Is.SameAs(context));
                Assert.That(context.LifecycleState, Is.EqualTo(StageRunLifecycleState.CorridorActive));
                Assert.That(context.CurrentSegmentIndex, Is.Zero);
                Assert.That(context.PendingHandoffToken, Is.Null);
                Assert.That(fixture.Bootstrap.HasAdmittedRun, Is.False);
                Assert.That(fixture.Bootstrap.LastAdmissionError, Does.Contain("exactly one"));
                Assert.That(fixture.FactAdapter.IsBound, Is.False);
                Assert.That(fixture.Presenter.HasCanonicalStageRun, Is.False);
                Assert.That(fixture.ResultOverlay.ShowCount, Is.Zero);
            }
            finally
            {
                fixture.Destroy();
                StageRunRuntime.ResetForTests();
            }
        }

        [UnityTest]
        public IEnumerator MalformedBoundedRouteTopologyRejectsBeforeContextCreation()
        {
            StageRunRuntime.ResetForTests();
            yield return LoadSingleScene(CorridorScenePath);

            Scene hostScene = SceneManager.GetActiveScene();
            OlympusCorridorCombatFlowController flow =
                RequireSingleSceneComponent<OlympusCorridorCombatFlowController>(hostScene);
            PlayableStageDefinition source = ReadPrivateField<PlayableStageDefinition>(
                flow,
                "playableStageDefinition");
            flow.enabled = false;
            StageRunRuntime.ResetForTests();

            (string Name, Action<PlayableStageDefinition> Mutate)[] cases =
            {
                ("zero rows", route =>
                    SetPrivateField(route, "sceneSegments", Array.Empty<StageSceneSegmentRef>())),
                ("three rows", route =>
                {
                    StageSceneSegmentRef[] segments =
                        ReadPrivateField<StageSceneSegmentRef[]>(route, "sceneSegments");
                    SetPrivateField(
                        route,
                        "sceneSegments",
                        new[] { segments[0], segments[1], segments[1] });
                }),
                ("duplicate segment ID", route =>
                    SetPrivateField(
                        route.GetSceneSegment(1),
                        "segmentId",
                        route.GetSceneSegment(0).SegmentId)),
                ("sequence gap", route =>
                    SetPrivateField(route.GetSceneSegment(1), "sequenceIndex", 2)),
                ("wrong first-entry ID", route =>
                    SetPrivateField(
                        route.GetSceneSegment(0),
                        "entryConditionId",
                        "wrong.run.entry")),
                ("boundary mismatch", route =>
                    SetPrivateField(
                        route.GetSceneSegment(1),
                        "entryConditionId",
                        "wrong.boundary")),
                ("empty final condition", route =>
                    SetPrivateField(route.GetSceneSegment(1), "exitConditionId", string.Empty)),
                ("wrong final condition kind", route =>
                    SetPrivateField(
                        route.GetSceneSegment(1),
                        "exitConditionKind",
                        StageSegmentConditionKind
                            .CorridorTutorialFactsAndClosureSealedForSingleLoad)),
                ("final successor", route =>
                    SetPrivateField(
                        route.GetSceneSegment(1),
                        "successorKind",
                        StageSegmentSuccessorKind.NextOrderedSegment)),
                ("SingleLoad missing successor", route =>
                {
                    StageSceneSegmentRef first = route.GetSceneSegment(0);
                    StageSceneSegmentRef second = route.GetSceneSegment(1);
                    SetPrivateField(first, "handoffPolicy", StageSceneHandoffPolicy.SingleLoad);
                    SetPrivateField(
                        first,
                        "exitConditionKind",
                        StageSegmentConditionKind
                            .CorridorTutorialFactsAndClosureSealedForSingleLoad);
                    SetPrivateField(
                        second,
                        "entryConditionKind",
                        StageSegmentConditionKind
                            .CorridorTutorialFactsAndClosureSealedForSingleLoad);
                    SetPrivateField(first, "successorKind", StageSegmentSuccessorKind.None);
                    SetPrivateField(
                        first,
                        "loaderGenerationKind",
                        StageSegmentLoaderGenerationKind.ActiveRunRouteLoaderGeneration);
                }),
                ("reserved Olympus terminal ID", route =>
                    SetPrivateField(route, "playableStageId", "B0-NON-OLYMPUS-ROUTE")),
                ("duplicate action ID", route =>
                    SetPrivateField(
                        route.GetTerminalAction(1),
                        "actionId",
                        route.GetTerminalAction(0).ActionId))
            };

            for (int caseIndex = 0; caseIndex < cases.Length; caseIndex++)
            {
                (string caseName, Action<PlayableStageDefinition> mutate) = cases[caseIndex];
                PlayableStageDefinition candidate = UnityEngine.Object.Instantiate(source);
                candidate.hideFlags = HideFlags.HideAndDontSave;
                try
                {
                    SetPrivateField(candidate, "routeRevision", 3);
                    mutate(candidate);
                    SetPrivateField(candidate, "canonicalRouteDigest", string.Empty);
                    SetPrivateField(
                        candidate,
                        "canonicalRouteDigest",
                        candidate.ComputeCanonicalRouteDigest());

                    Assert.That(
                        StageRunRouteSnapshot.TryCreate(
                            candidate,
                            out StageRunRouteSnapshot malformedSnapshot,
                            out string snapshotError),
                        Is.False,
                        caseName);
                    Assert.That(malformedSnapshot, Is.Null, caseName);
                    Assert.That(snapshotError, Is.Not.Empty, caseName);

                    StageRunRuntime.ResetForTests();
                    Assert.That(
                        StageRunRuntime.TryAdmitFirstSegment(
                            candidate,
                            hostScene,
                            out StageRunContext context,
                            out string admissionError),
                        Is.False,
                        caseName);
                    Assert.That(context, Is.Null, caseName);
                    Assert.That(admissionError, Is.Not.Empty, caseName);
                    Assert.That(StageRunRuntime.HasActiveContext, Is.False, caseName);
                }
                finally
                {
                    StageRunRuntime.ResetForTests();
                    UnityEngine.Object.DestroyImmediate(candidate);
                }
            }
        }

        [UnityTest]
        public IEnumerator HistoricalOlympusRevisionOneSnapshotRetainsFrozenSingleLoadIdentity()
        {
            StageRunRuntime.ResetForTests();
            yield return LoadSingleScene(CorridorScenePath);

            Scene hostScene = SceneManager.GetActiveScene();
            OlympusCorridorCombatFlowController flow =
                RequireSingleSceneComponent<OlympusCorridorCombatFlowController>(hostScene);
            PlayableStageDefinition source = ReadPrivateField<PlayableStageDefinition>(
                flow,
                "playableStageDefinition");
            flow.enabled = false;
            StageRunRuntime.ResetForTests();

            PlayableStageDefinition historical = UnityEngine.Object.Instantiate(source);
            historical.hideFlags = HideFlags.HideAndDontSave;
            StageDefinitionProfile historicalStation = UnityEngine.Object.Instantiate(
                historical.GetSceneSegment(1).StageDefinition);
            historicalStation.hideFlags = HideFlags.HideAndDontSave;
            try
            {
                SetPrivateField(historical, "routeRevision", 1);
                StageSceneSegmentRef corridor = historical.GetSceneSegment(0);
                StageSceneSegmentRef station = historical.GetSceneSegment(1);
                SetPrivateField(
                    corridor,
                    "exitConditionId",
                    "corridor.tutorial.completed");
                SetPrivateField(
                    corridor,
                    "exitConditionKind",
                    StageSegmentConditionKind
                        .CorridorTutorialFactsAndClosureSealedForSingleLoad);
                SetPrivateField(
                    corridor,
                    "handoffPolicy",
                    StageSceneHandoffPolicy.SingleLoad);
                SetPrivateField(
                    corridor,
                    "loaderGenerationKind",
                    StageSegmentLoaderGenerationKind.ActiveRunRouteLoaderGeneration);
                SetPrivateField(
                    historicalStation,
                    "mapScenePath",
                    StationScenePath);
                SetPrivateField(station, "stageDefinition", historicalStation);
                SetPrivateField(
                    station,
                    "entryConditionId",
                    "corridor.tutorial.completed");
                SetPrivateField(
                    station,
                    "entryConditionKind",
                    StageSegmentConditionKind
                        .CorridorTutorialFactsAndClosureSealedForSingleLoad);
                SetPrivateField(historical, "canonicalRouteDigest", string.Empty);
                string computedHistoricalDigest = historical.ComputeCanonicalRouteDigest();
                Assert.That(computedHistoricalDigest, Is.EqualTo(HistoricalRouteDigest));
                SetPrivateField(
                    historical,
                    "canonicalRouteDigest",
                    computedHistoricalDigest);

                Assert.That(
                    StageRunRouteSnapshot.TryCreate(
                        historical,
                        out StageRunRouteSnapshot snapshot,
                        out string snapshotError),
                    Is.True,
                    snapshotError);
                Assert.That(snapshot.RouteRevision, Is.EqualTo(1));
                Assert.That(snapshot.CanonicalDigest, Is.EqualTo(HistoricalRouteDigest));
                Assert.That(
                    snapshot.GetSegmentRoles(0),
                    Is.EqualTo(StageRunSegmentRole.Entry));
                Assert.That(
                    snapshot.GetSegment(0).HandoffPolicy,
                    Is.EqualTo(StageSceneHandoffPolicy.SingleLoad));
                Assert.That(
                    snapshot.GetSegment(0).LoaderGenerationKind,
                    Is.EqualTo(
                        StageSegmentLoaderGenerationKind.ActiveRunRouteLoaderGeneration));
                Assert.That(
                    snapshot.GetSegmentRoles(1),
                    Is.EqualTo(StageRunSegmentRole.Terminal));
                Assert.That(snapshot.GetSegment(1).ScenePath, Is.EqualTo(StationScenePath));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(historical);
                UnityEngine.Object.DestroyImmediate(historicalStation);
            }
        }

        [UnityTest]
        public IEnumerator CorridorEntryAtomicallyAdmitsDeepImmutableRouteSnapshot()
        {
            StageRunRuntime.ResetForTests();
            yield return LoadSingleScene(CorridorScenePath);

            Scene corridor = SceneManager.GetActiveScene();
            OlympusCorridorCombatFlowController flow =
                RequireSingleSceneComponent<OlympusCorridorCombatFlowController>(corridor);
            Assert.That(flow.HasCanonicalStageRun, Is.True);
            Assert.That(flow.CanonicalStageRunId, Is.Not.Empty);
            Assert.That(StageRunRuntime.HasActiveContext, Is.True);

            StageRunContext context = StageRunRuntime.ActiveContext;
            Assert.That(context, Is.Not.Null);
            Assert.That(context.Identity.RunId, Is.EqualTo(flow.CanonicalStageRunId));
            Assert.That(context.Identity.PlayableStageId, Is.EqualTo("OLYMPUS-INVASION-01"));
            Assert.That(context.Identity.RouteRevision, Is.EqualTo(2));
            Assert.That(context.Identity.RouteSnapshotDigest, Is.EqualTo(RouteDigest));
            Assert.That(context.Identity.EntrySegmentId, Is.EqualTo("corridor_intro_tutorial"));
            Assert.That(context.LifecycleState, Is.EqualTo(StageRunLifecycleState.CorridorActive));
            Assert.That(context.RouteSnapshot.SegmentCount, Is.EqualTo(2));
            Assert.That(
                context.TutorialFactRequirement,
                Is.EqualTo(StageRunTutorialFactRequirement.LegacyCorridorCompletion));
            Assert.That(context.RouteSnapshot.ActionCount, Is.EqualTo(3));
            Assert.That(context.RouteSnapshot.ComputeCanonicalDigest(), Is.EqualTo(RouteDigest));
            StageRunSegmentSnapshot corridorSegment = context.RouteSnapshot.GetSegment(0);
            StageRunSegmentSnapshot stationSegment = context.RouteSnapshot.GetSegment(1);
            Assert.That(corridorSegment.ScenePath, Is.EqualTo(CorridorScenePath));
            Assert.That(stationSegment.ScenePath, Is.EqualTo(CorridorScenePath));
            Assert.That(corridorSegment.ExitConditionId, Is.EqualTo(StationEntryConditionId));
            Assert.That(
                corridorSegment.ExitConditionKind,
                Is.EqualTo(
                    StageSegmentConditionKind
                        .CorridorTutorialFactsSealedAndStationEntryReachedForInSceneAdvance));
            Assert.That(corridorSegment.HandoffPolicy, Is.EqualTo(StageSceneHandoffPolicy.InSceneAdvance));
            Assert.That(
                corridorSegment.LoaderGenerationKind,
                Is.EqualTo(StageSegmentLoaderGenerationKind.None));
            Assert.That(stationSegment.EntryConditionId, Is.EqualTo(StationEntryConditionId));
            Assert.That(stationSegment.EntryConditionKind, Is.EqualTo(corridorSegment.ExitConditionKind));

            AssertContainsNoUnityObjectFields(typeof(StageRunRouteSnapshot));
            AssertContainsNoUnityObjectFields(typeof(StageRunSegmentSnapshot));
            AssertContainsNoUnityObjectFields(typeof(StageRunActionSnapshot));
            AssertContainsNoUnityObjectFields(typeof(StageRunTerminalPolicySnapshot));
            AssertContainsNoUnityObjectFields(typeof(StageRunIdentity));
            AssertContainsNoUnityObjectFields(typeof(StageSceneSegmentResult));
            AssertContainsNoUnityObjectFields(typeof(StageTutorialRouteSummaryFact));
            AssertContainsNoUnityObjectFields(typeof(StageTutorialFactCoverage));
            AssertContainsNoUnityObjectFields(typeof(StageRunSummonUseFact));
            AssertContainsNoUnityObjectFields(typeof(StageRunSemanticProofFact));
            AssertContainsNoUnityObjectFields(typeof(StageRunCombatFacts));
            AssertContainsNoUnityObjectFields(typeof(StageOutcomeFact));
            AssertContainsNoUnityObjectFields(typeof(EncounterTerminalEpochEvidence));
            AssertContainsNoUnityObjectFields(typeof(EncounterTerminalSubjectSnapshotEvidence));
            AssertContainsNoUnityObjectFields(typeof(EncounterTerminalCandidateEvidence));
            AssertContainsNoUnityObjectFields(typeof(EncounterTerminalDiscardedAdmissionEvidence));
            AssertContainsNoUnityObjectFields(typeof(StageRootResolutionTokenRecord));
            AssertContainsNoUnityObjectFields(typeof(StageTerminalSubjectFinalSnapshot));
            AssertContainsNoUnityObjectFields(typeof(StageTerminalCandidateCoverageRow));
            AssertContainsNoUnityObjectFields(typeof(StageDiscardedPendingAdmissionCoverageRow));
            AssertContainsNoUnityObjectFields(typeof(TerminalEpochClosureRecord));
            AssertContainsNoUnityObjectFields(typeof(TerminalFinalizationAuthority));
            AssertContainsNoUnityObjectFields(typeof(TerminalFinalizationOwnerCoverageRow));
            AssertContainsNoUnityObjectFields(typeof(TerminalFinalizationOwnerCoverageRecord));
            AssertContainsNoUnityObjectFields(typeof(StageRunHandoffToken));
            AssertContainsNoUnityObjectFields(typeof(StageRunSingleLoadDispatch));
            AssertContainsNoUnityObjectFields(typeof(StageSegmentEntryReceipt));
            AssertContainsNoUnityObjectFields(typeof(StageSegmentHandoffTerminalReceipt));
            AssertContainsNoUnityObjectFields(typeof(StageRunAbortCloseAuthority));
            AssertContainsNoUnityObjectFields(typeof(StageRunRouteHandoffCoverage));
            AssertContainsNoUnityObjectFields(typeof(StageRunOutcomeFactCoverage));
            AssertContainsNoUnityObjectFields(typeof(StageRunClosureBarrierCoverageRow));
            AssertContainsNoUnityObjectFields(typeof(StageRunAbortRecord));
            AssertContainsNoUnityObjectFields(typeof(StageDispatchClosureFaultRecord));
        }

        [UnityTest]
        public IEnumerator SealedCorridorInSceneAdvanceIsIdempotentAndConsumedByExactStationEntry()
        {
            StageRunRuntime.ResetForTests();
            yield return LoadSingleScene(CorridorScenePath);

            StageRunContext context = StageRunRuntime.ActiveContext;
            Assert.That(context, Is.Not.Null);
            Scene corridor = SceneManager.GetActiveScene();
            int corridorHandle = corridor.handle;
            Assert.That(
                context.TrySealTutorialRouteCompletion(out string tutorialFactError),
                Is.True,
                tutorialFactError);
            Assert.That(
                context.TryAdvanceCurrentSegmentInScene(
                    StationEntryConditionId,
                    corridor,
                    out StageSegmentEntryReceipt firstReceipt,
                    out string firstError),
                Is.True,
                firstError);
            Assert.That(context.LifecycleState, Is.EqualTo(StageRunLifecycleState.StationActive));
            Assert.That(firstReceipt.RunId, Is.EqualTo(context.Identity.RunId));
            Assert.That(firstReceipt.RouteSnapshotDigest, Is.EqualTo(RouteDigest));
            Assert.That(firstReceipt.SourceSegmentId, Is.EqualTo("corridor_intro_tutorial"));
            Assert.That(firstReceipt.DestinationSegmentId, Is.EqualTo("station_entry_combat"));
            Assert.That(firstReceipt.DestinationStageDefinitionId, Is.EqualTo(StationStageId));
            Assert.That(firstReceipt.RequestedScenePath, Is.EqualTo(CorridorScenePath));
            Assert.That(firstReceipt.ActualScenePath, Is.EqualTo(CorridorScenePath));
            Assert.That(firstReceipt.RequestedSceneName, Is.EqualTo(corridor.name));
            Assert.That(firstReceipt.ActualSceneName, Is.EqualTo(corridor.name));
            Assert.That(firstReceipt.FromHandoffPending, Is.False);
            Assert.That(firstReceipt.ToDestinationActive, Is.True);
            Assert.That(firstReceipt.TransitionTokenDigest, Has.Length.EqualTo(64));
            Assert.That(firstReceipt.CanonicalDigest, Has.Length.EqualTo(64));

            Assert.That(
                context.TryAdvanceCurrentSegmentInScene(
                    StationEntryConditionId,
                    corridor,
                    out StageSegmentEntryReceipt duplicateReceipt,
                    out string duplicateError),
                Is.True,
                duplicateError);
            Assert.That(duplicateReceipt, Is.SameAs(firstReceipt));

            Scene station = SceneManager.GetActiveScene();
            Assert.IsTrue(
                station.handle == corridorHandle,
                $"In-scene Station activation must retain Corridor scene handle {corridorHandle}; "
                + $"actual={station.handle}.");
            StageDefinitionSceneBinding stationBinding = RequireStationSceneBinding(station);
            StageCountOneEncounterExecutor executor =
                RequireSingleSceneComponent<StageCountOneEncounterExecutor>(station);
            Assert.That(executor.SceneBinding, Is.SameAs(stationBinding));
            Assert.That(StageRunRuntime.ActiveContext, Is.SameAs(context));
            Assert.That(context.LifecycleState, Is.EqualTo(StageRunLifecycleState.StationActive));
            Assert.That(context.CurrentSegment.SegmentId, Is.EqualTo("station_entry_combat"));
            Assert.That(context.PendingHandoffToken, Is.Null);
            Assert.That(context.SegmentEntryReceipt, Is.Not.Null);
            Assert.That(context.HandoffTerminalReceipt, Is.Not.Null);
            Assert.That(
                context.SegmentEntryReceipt.TransitionTokenId,
                Is.EqualTo(firstReceipt.TransitionTokenId));
            Assert.That(
                context.SegmentEntryReceipt.TransitionTokenDigest,
                Is.EqualTo(firstReceipt.TransitionTokenDigest));
            Assert.That(
                context.HandoffTerminalReceipt.Disposition,
                Is.EqualTo(StageSegmentHandoffClosedDisposition.DestinationBound));
            Assert.That(
                context.HandoffTerminalReceipt.SegmentEntryReceiptId,
                Is.EqualTo(context.SegmentEntryReceipt.SegmentEntryReceiptId));
            Assert.That(
                context.HandoffTerminalReceipt.SegmentEntryReceiptDigest,
                Is.EqualTo(context.SegmentEntryReceipt.CanonicalDigest));
            Assert.That(context.HandoffTerminalReceipt.LoaderGeneration, Is.Zero);
            Assert.That(context.HandoffTerminalReceipt.LoaderGenerationInvalidated, Is.False);
            Assert.That(context.HandoffTerminalReceipt.PendingLoadCallbackCount, Is.Zero);
            Assert.That(context.HandoffTerminalReceipt.PendingBindCallbackCount, Is.Zero);
            Assert.That(context.HandoffTerminalReceipt.PendingUnloadCallbackCount, Is.Zero);
            Assert.That(context.CurrentSceneHandle == station.handle, Is.True);
            Assert.That(context.TutorialRouteSummaryFact, Is.Not.Null);
            Assert.That(
                context.TutorialRouteSummaryFact.PlanSemanticDigest,
                Is.EqualTo(StageRunFactVocabulary.OlympusCorridorTutorialPlanSemanticDigest));
            Assert.That(context.TutorialRouteSummaryFact.CoverageCount, Is.EqualTo(7));
            for (int i = 0; i < context.TutorialRouteSummaryFact.CoverageCount; i++)
            {
                StageTutorialFactCoverage coverage = context.TutorialRouteSummaryFact.GetCoverage(i);
                Assert.That(coverage.PlanOrdinal, Is.EqualTo(i));
                Assert.That(coverage.CoverageKind, Is.EqualTo(StageTutorialFactCoverageKind.LegacyOpaque));
                Assert.That(coverage.ResultExpectation, Is.EqualTo(StageTutorialResultExpectation.NoResultExpected));
            }
        }

        [UnityTest]
        public IEnumerator DirectStationLoadCannotManufactureCanonicalRun()
        {
            StageRunRuntime.ResetForTests();
            yield return LoadSingleScene(StationScenePath);

            Scene station = SceneManager.GetActiveScene();
            OlympusStationCombatResultPresenter presenter =
                RequireSingleSceneComponent<OlympusStationCombatResultPresenter>(station);
            Assert.That(presenter.HasCanonicalStageRun, Is.False);
            Assert.That(presenter.CanonicalStageRunEntryError, Does.Contain("No active canonical stage run"));
            Assert.That(StageRunRuntime.HasActiveContext, Is.False);
        }

        [UnityTest]
        public IEnumerator FreshCorridorSceneInstanceCreatesFreshRunIdentity()
        {
            StageRunRuntime.ResetForTests();
            yield return LoadSingleScene(CorridorScenePath);
            string firstRunId = StageRunRuntime.ActiveContext.Identity.RunId;
            int firstSceneHandle = SceneManager.GetActiveScene().handle;

            yield return LoadSingleScene(CorridorScenePath);
            string secondRunId = StageRunRuntime.ActiveContext.Identity.RunId;
            int secondSceneHandle = SceneManager.GetActiveScene().handle;

            Assert.That(secondSceneHandle, Is.Not.EqualTo(firstSceneHandle));
            Assert.That(secondRunId, Is.Not.EqualTo(firstRunId));
            Assert.That(StageRunRuntime.ActiveContext.LifecycleState, Is.EqualTo(StageRunLifecycleState.CorridorActive));
        }

        [UnityTest]
        public IEnumerator StationCollectorSealsDamageDodgeSummonProofAndActiveClocks()
        {
            StageRunRuntime.ResetForTests();
            yield return EnterCanonicalStation();

            Scene station = SceneManager.GetActiveScene();
            StageRunContext context = StageRunRuntime.ActiveContext;
            OlympusStationRunFactCollector collector =
                RequireSingleSceneComponent<OlympusStationRunFactCollector>(station);
            CombatEncounterController encounter =
                RequireSingleSceneComponent<CombatEncounterController>(station);
            CombatHealth playerHealth = ReadPrivateField<CombatHealth>(encounter, "playerHealth");
            CombatHealth enemyHealth = ReadPrivateField<CombatHealth>(encounter, "enemyHealth");
            PlayerActionController actionController = playerHealth.GetComponent<PlayerActionController>();
            SummonEnergyLadder energyLadder = playerHealth.GetComponent<SummonEnergyLadder>();
            PlayerSummonSlot1Action summonSlot1 = playerHealth.GetComponent<PlayerSummonSlot1Action>();

            Assert.That(collector.IsBound, Is.True, collector.LastFactError);
            Assert.That(collector.GuideState, Is.EqualTo(CombatEntryGuideState.Released));
            Assert.That(actionController, Is.Not.Null);
            Assert.That(energyLadder, Is.Not.Null);
            Assert.That(summonSlot1, Is.Not.Null);

            Assert.That(
                playerHealth.TryApplyDamage(new DamageInfo(
                    null,
                    DamageTeam.Enemy,
                    7f,
                    playerHealth.transform.position,
                    Vector3.forward,
                    0f,
                    DamageResponsePolicy.DamageOnly,
                    CombatControlLockPolicy.None)),
                Is.True);

            actionController.QueueDodge();
            float dodgeDeadline = Time.realtimeSinceStartup + 2f;
            while (!actionController.IsDodging)
            {
                Assert.Less(Time.realtimeSinceStartup, dodgeDeadline, "Player never entered the authored dodge state.");
                yield return null;
            }

            playerHealth.TryApplyDamage(new DamageInfo(
                null,
                DamageTeam.Enemy,
                3f,
                playerHealth.transform.position,
                Vector3.forward,
                0f,
                DamageResponsePolicy.DamageOnly,
                CombatControlLockPolicy.None));
            yield return null;

            energyLadder.GrantCurrentTierEnergy(200f);
            Assert.That(summonSlot1.TryUseSummonSlot1(), Is.True);
            InvokePrivate(summonSlot1, "NotifySummonPressureBlocked", 2);
            yield return new WaitForSecondsRealtime(0.06f);

            Assert.That(ApplyLethalDamage(enemyHealth, DamageTeam.Player), Is.True);
            OlympusStationCombatResultPresenter presenter =
                RequireSingleSceneComponent<OlympusStationCombatResultPresenter>(station);
            float resultDeadline = Time.realtimeSinceStartup + 4f;
            while (presenter.CommittedSummary == null)
            {
                Assert.Less(Time.realtimeSinceStartup, resultDeadline, presenter.LastCommitError);
                yield return null;
            }

            StageRunResultSummary summary = presenter.CommittedSummary;
            Assert.That(summary.Outcome, Is.EqualTo(StageRouteOutcome.Clear));
            Assert.That(summary.HasTutorialRouteSummaryFact, Is.True);
            Assert.That(summary.TutorialRouteSummaryFact, Is.Not.Null);
            Assert.That(
                summary.TutorialRouteSummaryFactDigest,
                Is.EqualTo(summary.TutorialRouteSummaryFact.CanonicalDigest));
            Assert.That(
                summary.TutorialRouteSummaryFact.PlanSemanticDigest,
                Is.EqualTo(OlympusTutorialPlanDigest));
            Assert.That(summary.OutcomeFact.OutcomeDisposition, Is.EqualTo(StageOutcomeDisposition.Clear));
            Assert.That(summary.OutcomeFact.TotalActiveElapsedMilliseconds, Is.GreaterThan(0));
            Assert.That(summary.OutcomeFact.CombatActiveElapsedMilliseconds, Is.GreaterThan(0));
            Assert.That(summary.CombatFacts.PlayerDamageTaken, Is.GreaterThanOrEqualTo(7d));
            Assert.That(summary.CombatFacts.PlayerDownCount, Is.Zero);
            Assert.That(summary.CombatFacts.PerfectDodgeCount, Is.EqualTo(1));
            Assert.That(summary.CombatFacts.SummonUseCount, Is.EqualTo(1));
            StageRunSummonUseFact summonUse = summary.CombatFacts.GetSummonUse(0);
            Assert.That(summonUse.SummonAdmissionSequence, Is.EqualTo(1));
            Assert.That(summonUse.SlotRoleId, Is.EqualTo("SummonSlot1"));
            Assert.That(summonUse.SpentTier, Is.EqualTo(2));
            Assert.That(
                summary.TryGetSemanticProof(
                    StageRunFactVocabulary.SummonPressureBlockProofId,
                    out StageRunSemanticProofFact pressureProof),
                Is.True);
            Assert.That(pressureProof.SourceKind, Is.EqualTo(StageRunFactVocabulary.SummonPressureScreenSourceKind));
            Assert.That(pressureProof.Count, Is.EqualTo(1));
            Assert.That(pressureProof.ActualValue, Is.EqualTo(2d));
            Assert.That(pressureProof.Qualified, Is.True);
            Assert.That(
                summary.TryGetSemanticProof(
                    StageRunFactVocabulary.SurvivalNoPlayerDownProofId,
                    out StageRunSemanticProofFact survivalProof),
                Is.True);
            Assert.That(survivalProof.Qualified, Is.True);
            Assert.That(context.LifecycleState, Is.EqualTo(StageRunLifecycleState.Committed));
        }

        [UnityTest]
        public IEnumerator OlympusTerminalFactsStillRequireExplicitGuideRelease()
        {
            StageRunRuntime.ResetForTests();
            yield return EnterCanonicalStation(releaseEntryGuide: false);

            StageRunContext context = StageRunRuntime.ActiveContext;
            Scene station = SceneManager.GetActiveScene();
            OlympusStationRunFactCollector collector =
                RequireSingleSceneComponent<OlympusStationRunFactCollector>(station);
            OlympusStationCombatResultPresenter presenter =
                RequireSingleSceneComponent<OlympusStationCombatResultPresenter>(station);
            CombatEncounterController encounter =
                RequireSingleSceneComponent<CombatEncounterController>(station);
            CombatHealth enemyHealth = ReadPrivateField<CombatHealth>(encounter, "enemyHealth");
            InvokePrivate(presenter, "UnsubscribeEncounter");

            Assert.That(
                context.TutorialFactRequirement,
                Is.EqualTo(StageRunTutorialFactRequirement.LegacyCorridorCompletion));
            Assert.That(context.TutorialRouteSummaryFact, Is.Not.Null);
            Assert.That(collector.IsBound, Is.True, collector.LastFactError);
            Assert.That(collector.GuideState, Is.Not.EqualTo(CombatEntryGuideState.Released));

            enemyHealth.SetInvulnerableUntil(0f);
            Assert.That(ApplyLethalDamage(enemyHealth, DamageTeam.Player), Is.True);
            Assert.That(encounter.HasTerminalResolution, Is.True);
            Assert.That(
                StageRunRuntime.TryCommitTerminalResolution(
                    encounter,
                    encounter.TerminalResolution,
                    out StageRunResultSummary summary,
                    out StageRunResultCommitReceipt receipt,
                    out string commitError),
                Is.False);
            Assert.That(commitError, Does.Contain("guide Released state"));
            Assert.That(summary, Is.Null);
            Assert.That(receipt, Is.Null);
            Assert.That(context.CommittedSummary, Is.Null);
            Assert.That(context.CommitReceipt, Is.Null);
            Assert.That(context.AbortRecord, Is.Not.Null);
            Assert.That(
                context.AbortRecord.AbortReason,
                Is.EqualTo(StageRunAbortReason.TerminalFinalizationFailed));
            Assert.That(context.LifecycleState, Is.EqualTo(StageRunLifecycleState.Disposed));
        }

        [UnityTest]
        public IEnumerator ConflictingDurableDecisionQuarantinesRunWithoutOpeningResultUi()
        {
            StageRunRuntime.ResetForTests();
            yield return EnterCanonicalStation();

            StageRunContext context = StageRunRuntime.ActiveContext;
            Assert.That(
                StageRunRuntime.SeedConflictingResultDecisionForTests(
                    context.Identity,
                    out string seedError),
                Is.True,
                seedError);
            Scene station = SceneManager.GetActiveScene();
            CombatEncounterController encounter =
                RequireSingleSceneComponent<CombatEncounterController>(station);
            CombatHealth enemyHealth = ReadPrivateField<CombatHealth>(encounter, "enemyHealth");
            LogAssert.Expect(
                LogType.Error,
                new Regex("Canonical terminal commit rejected:.*different committed comparison value"));

            Time.timeScale = 1f;
            Assert.That(ApplyLethalDamage(enemyHealth, DamageTeam.Player), Is.True);
            yield return null;
            yield return null;

            OlympusStationCombatResultPresenter presenter =
                RequireSingleSceneComponent<OlympusStationCombatResultPresenter>(station);
            Assert.That(context.LifecycleState, Is.EqualTo(StageRunLifecycleState.CommitPersistenceFaulted));
            Assert.That(context.TerminalRecordReceiptCount, Is.EqualTo(1));
            Assert.That(context.CommittedSummary, Is.Null);
            Assert.That(presenter.CommittedSummary, Is.Null);
            Assert.That(SceneManager.GetSceneByName("UI_StageClear").isLoaded, Is.False);
            Assert.That(
                StageRunRuntime.TryReadCommittedResultDecision(
                    context.Identity.RunId,
                    out StageRunResultCommitReceipt conflictingReceipt,
                    out string readError),
                Is.True,
                readError);
            Assert.That(conflictingReceipt.ResultSummaryDigest, Is.EqualTo(new string('a', 64)));
        }

        [UnityTest]
        public IEnumerator CorruptDurableDecisionQuarantinesRunWithoutOpeningResultUi()
        {
            StageRunRuntime.ResetForTests();
            yield return EnterCanonicalStation();

            StageRunContext context = StageRunRuntime.ActiveContext;
            Assert.That(
                StageRunRuntime.SeedCorruptResultDecisionForTests(
                    context.Identity.RunId,
                    out string seedError),
                Is.True,
                seedError);
            Scene station = SceneManager.GetActiveScene();
            CombatEncounterController encounter =
                RequireSingleSceneComponent<CombatEncounterController>(station);
            CombatHealth enemyHealth = ReadPrivateField<CombatHealth>(encounter, "enemyHealth");
            LogAssert.Expect(
                LogType.Error,
                new Regex("Canonical terminal commit rejected:.*invalid schema or required field"));

            Time.timeScale = 1f;
            Assert.That(ApplyLethalDamage(enemyHealth, DamageTeam.Player), Is.True);
            yield return null;
            yield return null;

            OlympusStationCombatResultPresenter presenter =
                RequireSingleSceneComponent<OlympusStationCombatResultPresenter>(station);
            Assert.That(context.LifecycleState, Is.EqualTo(StageRunLifecycleState.CommitPersistenceFaulted));
            Assert.That(context.TerminalRecordReceiptCount, Is.EqualTo(1));
            Assert.That(context.CommittedSummary, Is.Null);
            Assert.That(presenter.CommittedSummary, Is.Null);
            Assert.That(SceneManager.GetSceneByName("UI_StageClear").isLoaded, Is.False);
            Assert.That(
                StageRunRuntime.TryReadCommittedResultDecision(
                    context.Identity.RunId,
                    out _,
                    out string readError),
                Is.False);
            Assert.That(readError, Does.Contain("invalid schema or required field"));
        }

        [UnityTest]
        public IEnumerator TransientWriteFailureRetriesExactCandidateBeforeOpeningResultUi()
        {
            yield return VerifyTransientCommitRecovery(
                writeFailureCount: 1,
                readFailureCount: 0,
                decisionExistsAfterInitialAttempt: false);
        }

        [UnityTest]
        public IEnumerator TransientReadFailureReconcilesExistingDecisionBeforeOpeningResultUi()
        {
            yield return VerifyTransientCommitRecovery(
                writeFailureCount: 0,
                readFailureCount: 1,
                decisionExistsAfterInitialAttempt: true);
        }

        [UnityTest]
        public IEnumerator CorridorReplacementSealsDiagnosticAbortBeforeFreshRunAdmission()
        {
            StageRunRuntime.ResetForTests();
            yield return LoadSingleScene(CorridorScenePath);

            StageRunContext oldContext = StageRunRuntime.ActiveContext;
            Assert.That(oldContext, Is.Not.Null);
            string oldRunId = oldContext.Identity.RunId;

            yield return LoadSingleScene(CorridorScenePath);

            StageRunContext freshContext = StageRunRuntime.ActiveContext;
            Assert.That(freshContext, Is.Not.Null);
            Assert.That(freshContext, Is.Not.SameAs(oldContext));
            Assert.That(freshContext.Identity.RunId, Is.Not.EqualTo(oldRunId));
            Assert.That(freshContext.LifecycleState, Is.EqualTo(StageRunLifecycleState.CorridorActive));
            Assert.That(oldContext.LifecycleState, Is.EqualTo(StageRunLifecycleState.Disposed));
            Assert.That(oldContext.CommittedSummary, Is.Null);
            Assert.That(oldContext.AbortRecord, Is.Not.Null);
            Assert.That(
                oldContext.AbortRecord.AbortReason,
                Is.EqualTo(StageRunAbortReason.UnexpectedSceneExit));
            Assert.That(
                oldContext.AbortCloseAuthority.CoordinatorInvalidationDisposition,
                Is.EqualTo(StageRunTerminalCoordinatorInvalidationDisposition.NotBoundBeforeStation));
            Assert.That(oldContext.AbortCloseAuthority.CoordinatorRootAdmissionSequence, Is.Zero);
            Assert.That(oldContext.AbortCloseAuthority.CoordinatorEpoch, Is.Zero);
            Assert.That(
                oldContext.AbortRecord.RouteHandoffCoverage.Disposition,
                Is.EqualTo(StageRunRouteHandoffCoverageDisposition.NotIssued));
            Assert.That(
                oldContext.AbortRecord.OutcomeFactCoverage.Disposition,
                Is.EqualTo(StageRunOutcomeFactCoverageDisposition.NotSealedBeforeAbort));
            Assert.That(StageRunRuntime.LastAbortRecord, Is.SameAs(oldContext.AbortRecord));
            AssertCurrentSchemaClosureRows(oldContext.AbortRecord);
        }

        [UnityTest]
        public IEnumerator DirectCorridorReentryCancelsRegisteredStationBeforeFreshAdmission()
        {
            StageRunRuntime.ResetForTests();
            yield return EnterCanonicalStation();

            Scene station = SceneManager.GetActiveScene();
            StageRunContext oldContext = StageRunRuntime.ActiveContext;
            CombatEncounterController encounter =
                RequireSingleSceneComponent<CombatEncounterController>(station);
            EncounterTerminalResolutionCoordinator coordinator = encounter.TerminalCoordinator;
            CombatHealth playerHealth = ReadPrivateField<CombatHealth>(encounter, "playerHealth");
            Assert.That(oldContext.LifecycleState, Is.EqualTo(StageRunLifecycleState.StationActive));
            Assert.That(coordinator.State, Is.EqualTo(EncounterTerminalCoordinatorState.Idle));

            Scene corridor = default;
            PlayableStageDefinition route = null;
            SceneManager.sceneLoaded += HandleCorridorLoaded;
            try
            {
                EditorSceneManager.LoadSceneInPlayMode(
                    CorridorScenePath,
                    new LoadSceneParameters(LoadSceneMode.Additive));
                for (int frame = 0; frame < 60 && !corridor.isLoaded; frame++)
                {
                    yield return null;
                }
            }
            finally
            {
                SceneManager.sceneLoaded -= HandleCorridorLoaded;
            }

            Assert.That(corridor.IsValid() && corridor.isLoaded, Is.True);
            Assert.That(route, Is.Not.Null);

            EncounterTerminalCoordinatorState callbackState = default;
            long callbackRoot = 0;
            long callbackEpoch = 0;
            StageRunContext freshContext = null;
            string replacementError = string.Empty;
            bool replacementAccepted = false;
            playerHealth.Damaged += HandlePlayerDamaged;
            try
            {
                CombatRootAdmissionResult admission = encounter.AdmitCombatRoot(
                    "test.direct-corridor-reentry-during-draining",
                    root =>
                    {
                        Assert.That(
                            root.TryApplyDamage(
                                playerHealth,
                                new DamageInfo(
                                    null,
                                    DamageTeam.Enemy,
                                    1f,
                                    playerHealth.transform.position,
                                    Vector3.forward,
                                    0f,
                                    DamageResponsePolicy.DamageOnly,
                                    CombatControlLockPolicy.None)),
                            Is.True);
                    });

                Assert.That(admission.Disposition, Is.EqualTo(CombatRootAdmissionDisposition.Executed));
                Assert.That(admission.CoordinatorState, Is.EqualTo(EncounterTerminalCoordinatorState.Cancelled));
            }
            finally
            {
                playerHealth.Damaged -= HandlePlayerDamaged;
            }

            Assert.That(callbackState, Is.EqualTo(EncounterTerminalCoordinatorState.Draining));
            Assert.That(callbackRoot, Is.GreaterThan(0));
            Assert.That(callbackEpoch, Is.GreaterThan(0));
            Assert.That(replacementAccepted, Is.True, replacementError);
            Assert.That(SceneManager.GetActiveScene().handle == station.handle, Is.True);
            Assert.That(coordinator.State, Is.EqualTo(EncounterTerminalCoordinatorState.Cancelled));
            Assert.That(oldContext.LifecycleState, Is.EqualTo(StageRunLifecycleState.Disposed));
            Assert.That(oldContext.AbortRecord, Is.Not.Null);
            Assert.That(
                oldContext.AbortRecord.AbortReason,
                Is.EqualTo(StageRunAbortReason.RunReplacedBeforeCommit));
            Assert.That(
                oldContext.AbortCloseAuthority.CoordinatorInvalidationDisposition,
                Is.EqualTo(StageRunTerminalCoordinatorInvalidationDisposition.CancellationRequested));
            Assert.That(
                oldContext.AbortCloseAuthority.CoordinatorRootAdmissionSequence,
                Is.EqualTo(callbackRoot));
            Assert.That(oldContext.AbortCloseAuthority.CoordinatorEpoch, Is.EqualTo(callbackEpoch));
            Assert.That(freshContext, Is.Not.Null);
            Assert.That(freshContext, Is.Not.SameAs(oldContext));
            Assert.That(freshContext.CurrentSceneHandle == corridor.handle, Is.True);
            Assert.That(freshContext.LifecycleState, Is.EqualTo(StageRunLifecycleState.CorridorActive));
            AssertCurrentSchemaClosureRows(oldContext.AbortRecord);

            StageRunRuntime.ResetForTests();
            AsyncOperation unload = SceneManager.UnloadSceneAsync(corridor);
            if (unload != null)
            {
                yield return unload;
            }

            void HandlePlayerDamaged(DamageInfo _)
            {
                callbackState = coordinator.State;
                callbackRoot = coordinator.ActiveRootAdmissionSequence;
                callbackEpoch = coordinator.ActiveEpoch;
                replacementAccepted = StageRunRuntime.TryAdmitFirstSegment(
                    route,
                    corridor,
                    out freshContext,
                    out replacementError);
            }

            void HandleCorridorLoaded(Scene loaded, LoadSceneMode mode)
            {
                if (mode != LoadSceneMode.Additive
                    || !string.Equals(loaded.path, CorridorScenePath, StringComparison.Ordinal))
                {
                    return;
                }

                corridor = loaded;
                OlympusCorridorCombatFlowController corridorFlow =
                    RequireSingleSceneComponent<OlympusCorridorCombatFlowController>(loaded);
                route = ReadPrivateField<PlayableStageDefinition>(
                    corridorFlow,
                    "playableStageDefinition");
                corridorFlow.enabled = false;
            }
        }

        [UnityTest]
        public IEnumerator FutureSchemaCannotFabricateCurrentNotAdmittedAbortClosure()
        {
            StageRunRuntime.ResetForTests();
            yield return LoadSingleScene(CorridorScenePath);

            StageRunContext context = StageRunRuntime.ActiveContext;
            SetPrivateField(
                context.Identity,
                "<SchemaVersion>k__BackingField",
                StageRunIdentity.CurrentSchemaVersion + 1);

            Assert.That(
                StageRunRuntime.TryAbortActiveRun(
                    null,
                    StageRunAbortReason.ExplicitAbort,
                    out _,
                    out string error),
                Is.False);
            Assert.That(error, Does.Contain("future route schema"));
            Assert.That(context.LifecycleState, Is.EqualTo(StageRunLifecycleState.ClosureFaulted));
            Assert.That(context.AbortRecord, Is.Null);
            Assert.That(StageRunRuntime.LastAbortRecord, Is.Null);
        }

        [UnityTest]
        public IEnumerator ForeignHostCannotAdvanceCanonicalRouteInSceneOrIssueLoaderGeneration()
        {
            StageRunRuntime.ResetForTests();
            yield return LoadSingleScene(CorridorScenePath);

            StageRunContext context = StageRunRuntime.ActiveContext;
            Assert.That(
                context.TrySealTutorialRouteCompletion(out string tutorialError),
                Is.True,
                tutorialError);

            EditorSceneManager.LoadSceneInPlayMode(
                LobbyScenePath,
                new LoadSceneParameters(LoadSceneMode.Additive));
            yield return null;
            yield return null;
            Scene foreignHost = SceneManager.GetSceneByPath(LobbyScenePath);
            Assert.That(foreignHost.IsValid() && foreignHost.isLoaded, Is.True);

            Assert.That(
                context.TryAdvanceCurrentSegmentInScene(
                    StationEntryConditionId,
                    foreignHost,
                    out StageSegmentEntryReceipt receipt,
                    out string advanceError),
                Is.False);
            Assert.That(receipt, Is.Null);
            Assert.That(advanceError, Does.Contain("does not match"));
            Assert.That(context.LifecycleState, Is.EqualTo(StageRunLifecycleState.Faulted));
            Assert.That(context.PendingHandoffToken, Is.Null);
            Assert.That(context.SegmentEntryReceipt, Is.Null);
            Assert.That(context.HandoffTerminalReceipt, Is.Null);
            Assert.That(context.AbortRecord, Is.Null);

            AsyncOperation unload = SceneManager.UnloadSceneAsync(foreignHost);
            if (unload != null)
            {
                yield return unload;
            }
        }

        [UnityTest]
        public IEnumerator CoordinatorDiagnosticSealsAbortWithoutProductResultOrFailureUi()
        {
            StageRunRuntime.ResetForTests();
            yield return EnterCanonicalStation();

            StageRunContext context = StageRunRuntime.ActiveContext;
            Scene station = SceneManager.GetActiveScene();
            CombatEncounterController encounter =
                RequireSingleSceneComponent<CombatEncounterController>(station);
            CombatHealth playerHealth = ReadPrivateField<CombatHealth>(encounter, "playerHealth");

            playerHealth.ResetHealthToFull();
            yield return null;

            Assert.That(encounter.IsFaulted, Is.True);
            Assert.That(context.LifecycleState, Is.EqualTo(StageRunLifecycleState.Disposed));
            Assert.That(context.CommittedSummary, Is.Null);
            Assert.That(context.AbortRecord, Is.Not.Null);
            Assert.That(
                context.AbortRecord.AbortReason,
                Is.EqualTo(StageRunAbortReason.CoordinatorDiagnostic));
            Assert.That(
                context.AbortCloseAuthority.CoordinatorInvalidationDisposition,
                Is.EqualTo(StageRunTerminalCoordinatorInvalidationDisposition.Faulted));
            Assert.That(context.AbortCloseAuthority.HasTerminalFinalizationAuthority, Is.False);
            Assert.That(
                context.AbortRecord.RouteHandoffCoverage.Disposition,
                Is.EqualTo(StageRunRouteHandoffCoverageDisposition.Succeeded));
            Assert.That(
                context.AbortRecord.OutcomeFactCoverage.Disposition,
                Is.EqualTo(StageRunOutcomeFactCoverageDisposition.NotSealedBeforeAbort));
            Assert.That(SceneManager.GetSceneByName("UI_StageClear").isLoaded, Is.False);
            AssertCurrentSchemaClosureRows(context.AbortRecord);
        }

        [UnityTest]
        public IEnumerator CoordinatorDiagnosticIngressRequiresRegisteredExactPublishedFault()
        {
            StageRunRuntime.ResetForTests();
            yield return EnterCanonicalStation();

            StageRunContext context = StageRunRuntime.ActiveContext;
            Scene station = SceneManager.GetActiveScene();
            CombatEncounterController encounter =
                RequireSingleSceneComponent<CombatEncounterController>(station);
            OlympusStationCombatResultPresenter presenter =
                RequireSingleSceneComponent<OlympusStationCombatResultPresenter>(station);
            InvokePrivate(presenter, "UnsubscribeEncounter");
            CombatHealth playerHealth = ReadPrivateField<CombatHealth>(encounter, "playerHealth");

            playerHealth.ResetHealthToFull();
            Assert.That(encounter.IsFaulted, Is.True);
            Assert.That(encounter.HasDiagnostic, Is.True);
            Assert.That(encounter.TerminalCoordinator.HasDiagnostic, Is.True);
            Assert.That(context.LifecycleState, Is.EqualTo(StageRunLifecycleState.StationActive));
            Assert.That(context.AbortRecord, Is.Null);

            EncounterTerminalDiagnostic exactDiagnostic = encounter.Diagnostic;
            EncounterTerminalDiagnostic forgedDiagnostic = CreateDiagnosticForTest(
                exactDiagnostic.Reason,
                exactDiagnostic.RunGeneration,
                exactDiagnostic.RootAdmissionSequence,
                exactDiagnostic.Epoch,
                exactDiagnostic.Message + " / forged");

            Assert.That(
                StageRunRuntime.TryAbortFromCoordinatorDiagnostic(
                    encounter,
                    forgedDiagnostic,
                    out StageRunAbortRecord forgedRecord,
                    out string forgedError),
                Is.False);
            Assert.That(forgedRecord, Is.Null);
            Assert.That(forgedError, Does.Contain("exact fault"));
            Assert.That(context.LifecycleState, Is.EqualTo(StageRunLifecycleState.StationActive));

            GameObject foreignEncounterObject = new("ForeignDiagnosticEncounter");
            foreignEncounterObject.SetActive(false);
            CombatEncounterController foreignEncounter =
                foreignEncounterObject.AddComponent<CombatEncounterController>();
            try
            {
                Assert.That(
                    StageRunRuntime.TryAbortFromCoordinatorDiagnostic(
                        foreignEncounter,
                        exactDiagnostic,
                        out StageRunAbortRecord foreignRecord,
                        out string foreignError),
                    Is.False);
                Assert.That(foreignRecord, Is.Null);
                Assert.That(foreignError, Does.Contain("exact fault"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(foreignEncounterObject);
            }

            Assert.That(
                StageRunRuntime.TryAbortFromCoordinatorDiagnostic(
                    encounter,
                    exactDiagnostic,
                    out StageRunAbortRecord exactRecord,
                    out string exactError),
                Is.True,
                exactError);
            Assert.That(exactRecord, Is.SameAs(context.AbortRecord));
            Assert.That(context.LifecycleState, Is.EqualTo(StageRunLifecycleState.Disposed));
            Assert.That(
                context.AbortCloseAuthority.CoordinatorInvalidationDisposition,
                Is.EqualTo(StageRunTerminalCoordinatorInvalidationDisposition.Faulted));
            AssertCurrentSchemaClosureRows(exactRecord);
        }

        [UnityTest]
        public IEnumerator ProducerExceptionFaultsCoordinatorAndSealsOneDiagnosticAbort()
        {
            StageRunRuntime.ResetForTests();
            yield return EnterCanonicalStation();

            StageRunContext context = StageRunRuntime.ActiveContext;
            Scene station = SceneManager.GetActiveScene();
            CombatEncounterController encounter =
                RequireSingleSceneComponent<CombatEncounterController>(station);

            CombatRootAdmissionResult admission = encounter.AdmitCombatRoot(
                "test.producer-exception",
                _ => throw new InvalidOperationException("injected producer exception"));

            Assert.That(admission.Disposition, Is.EqualTo(CombatRootAdmissionDisposition.Executed));
            Assert.That(
                admission.CoordinatorState,
                Is.EqualTo(EncounterTerminalCoordinatorState.Faulted));
            Assert.That(encounter.IsFaulted, Is.True);
            Assert.That(encounter.Diagnostic.Reason, Is.EqualTo(EncounterTerminalDiagnosticReason.ProducerException));
            Assert.That(context.LifecycleState, Is.EqualTo(StageRunLifecycleState.Disposed));
            Assert.That(context.CommittedSummary, Is.Null);
            Assert.That(context.AbortRecord, Is.Not.Null);
            Assert.That(context.AbortRecord.AbortReason, Is.EqualTo(StageRunAbortReason.CoordinatorDiagnostic));
            Assert.That(StageRunRuntime.LastAbortRecord, Is.SameAs(context.AbortRecord));
            Assert.That(SceneManager.GetSceneByName("UI_StageClear").isLoaded, Is.False);
            AssertCurrentSchemaClosureRows(context.AbortRecord);
        }

        [UnityTest]
        public IEnumerator FinalSnapshotExceptionSealsOneDiagnosticAbortWithoutSummaryOrResultUi()
        {
            StageRunRuntime.ResetForTests();
            yield return EnterCanonicalStation();

            StageRunContext context = StageRunRuntime.ActiveContext;
            Scene station = SceneManager.GetActiveScene();
            CombatEncounterController encounter =
                RequireSingleSceneComponent<CombatEncounterController>(station);
            EncounterTerminalResolutionCoordinator coordinator = encounter.TerminalCoordinator;
            CombatHealth enemyHealth = ReadPrivateField<CombatHealth>(encounter, "enemyHealth");
            int diagnosticCount = 0;
            encounter.DiagnosticAborted += HandleDiagnostic;
            coordinator.SetFinalSnapshotBoundaryForTests(_ =>
                throw new InvalidOperationException("injected route final snapshot failure"));
            try
            {
                Assert.That(ApplyLethalDamage(enemyHealth, DamageTeam.Player), Is.True);
            }
            finally
            {
                encounter.DiagnosticAborted -= HandleDiagnostic;
            }

            Assert.That(diagnosticCount, Is.EqualTo(1));
            Assert.That(coordinator.State, Is.EqualTo(EncounterTerminalCoordinatorState.Faulted));
            Assert.That(coordinator.HasTerminalResolution, Is.False);
            Assert.That(coordinator.HasTerminalEpochEvidence, Is.False);
            Assert.That(encounter.IsFaulted, Is.True);
            Assert.That(
                encounter.Diagnostic.Reason,
                Is.EqualTo(EncounterTerminalDiagnosticReason.FinalizationException));
            Assert.That(context.LifecycleState, Is.EqualTo(StageRunLifecycleState.Disposed));
            Assert.That(context.CommittedSummary, Is.Null);
            Assert.That(context.AbortRecord, Is.Not.Null);
            Assert.That(
                context.AbortRecord.AbortReason,
                Is.EqualTo(StageRunAbortReason.CoordinatorDiagnostic));
            Assert.That(StageRunRuntime.LastAbortRecord, Is.SameAs(context.AbortRecord));
            Assert.That(SceneManager.GetSceneByName("UI_StageClear").isLoaded, Is.False);
            AssertCurrentSchemaClosureRows(context.AbortRecord);

            void HandleDiagnostic(EncounterTerminalDiagnostic _)
            {
                diagnosticCount++;
            }
        }

        [UnityTest]
        public IEnumerator CandidateFinalMismatchSealsTerminalFinalizationDiagnosticAbort()
        {
            StageRunRuntime.ResetForTests();
            yield return EnterCanonicalStation();

            StageRunContext context = StageRunRuntime.ActiveContext;
            Scene station = SceneManager.GetActiveScene();
            CombatEncounterController encounter =
                RequireSingleSceneComponent<CombatEncounterController>(station);
            OlympusStationCombatResultPresenter presenter =
                RequireSingleSceneComponent<OlympusStationCombatResultPresenter>(station);
            InvokePrivate(presenter, "UnsubscribeEncounter");
            CombatHealth enemyHealth = ReadPrivateField<CombatHealth>(encounter, "enemyHealth");
            Assert.That(ApplyLethalDamage(enemyHealth, DamageTeam.Player), Is.True);
            Assert.That(encounter.HasTerminalResolution, Is.True);
            Assert.That(context.LifecycleState, Is.EqualTo(StageRunLifecycleState.StationActive));

            EncounterTerminalResolution valid = encounter.TerminalResolution;
            Assert.That(
                StageRunRuntime.TryCommitCandidateFinalMismatchForTests(
                    encounter,
                    out string error),
                Is.False);

            Assert.That(error, Does.Contain("candidate and final subject state"));
            Assert.That(context.LifecycleState, Is.EqualTo(StageRunLifecycleState.Disposed));
            Assert.That(context.CommittedSummary, Is.Null);
            Assert.That(context.AbortRecord, Is.Not.Null);
            Assert.That(
                context.AbortRecord.AbortReason,
                Is.EqualTo(StageRunAbortReason.TerminalFinalizationFailed));
            Assert.That(
                context.AbortCloseAuthority.CoordinatorInvalidationDisposition,
                Is.EqualTo(StageRunTerminalCoordinatorInvalidationDisposition.TerminalAuthorityInvalidated));
            Assert.That(
                context.AbortCloseAuthority.CoordinatorRootAdmissionSequence,
                Is.EqualTo(valid.RootAdmissionSequence));
            Assert.That(SceneManager.GetSceneByName("UI_StageClear").isLoaded, Is.False);
            AssertCurrentSchemaClosureRows(context.AbortRecord);
        }

        [UnityTest]
        public IEnumerator FactCollectorLossDuringRootCancelsWorkAndSealsTypedAbort()
        {
            StageRunRuntime.ResetForTests();
            yield return EnterCanonicalStation();

            StageRunContext context = StageRunRuntime.ActiveContext;
            Scene station = SceneManager.GetActiveScene();
            CombatEncounterController encounter =
                RequireSingleSceneComponent<CombatEncounterController>(station);
            EncounterTerminalResolutionCoordinator coordinator = encounter.TerminalCoordinator;
            OlympusStationRunFactCollector collector =
                RequireSingleSceneComponent<OlympusStationRunFactCollector>(station);
            CombatHealth playerHealth = ReadPrivateField<CombatHealth>(encounter, "playerHealth");
            bool acceptedAfterLoss = true;

            CombatRootAdmissionResult admission = encounter.AdmitCombatRoot(
                "test.station-fact-collector-loss",
                root =>
                {
                    collector.enabled = false;
                    acceptedAfterLoss = root.TryApplyDamage(
                        playerHealth,
                        new DamageInfo(
                            null,
                            DamageTeam.Enemy,
                            1f,
                            playerHealth.transform.position,
                            Vector3.forward,
                            0f,
                            DamageResponsePolicy.DamageOnly,
                            CombatControlLockPolicy.None));
                });

            Assert.That(admission.Disposition, Is.EqualTo(CombatRootAdmissionDisposition.Executed));
            Assert.That(admission.CoordinatorState, Is.EqualTo(EncounterTerminalCoordinatorState.Cancelled));
            Assert.That(coordinator.State, Is.EqualTo(EncounterTerminalCoordinatorState.Cancelled));
            Assert.That(acceptedAfterLoss, Is.False);
            Assert.That(playerHealth.CurrentHealth, Is.EqualTo(playerHealth.MaxHealth));
            Assert.That(context.LifecycleState, Is.EqualTo(StageRunLifecycleState.Disposed));
            Assert.That(context.CommittedSummary, Is.Null);
            Assert.That(context.AbortRecord.AbortReason, Is.EqualTo(StageRunAbortReason.StationFactCollectorLost));
            Assert.That(
                context.AbortCloseAuthority.CoordinatorRootAdmissionSequence,
                Is.EqualTo(admission.RootAdmissionSequence));
            Assert.That(context.AbortCloseAuthority.CoordinatorEpoch, Is.GreaterThan(0));
            Assert.That(
                encounter.AdmitCombatRoot(_ => { }).Disposition,
                Is.EqualTo(CombatRootAdmissionDisposition.Rejected));
            Assert.That(SceneManager.GetSceneByName("UI_StageClear").isLoaded, Is.False);
            AssertCurrentSchemaClosureRows(context.AbortRecord);

            StageRunAbortRecord sealedAbort = context.AbortRecord;
            collector.enabled = true;
            yield return null;
            Assert.That(collector.IsBound, Is.False);
            Assert.That(collector.BindToActiveRun(), Is.False);
            Assert.That(context.AbortRecord, Is.SameAs(sealedAbort));
            Assert.That(StageRunRuntime.LastAbortRecord, Is.SameAs(sealedAbort));
        }

        [UnityTest]
        public IEnumerator ResultPresenterLossCancelsIdleCoordinatorAndDetachesFactCollector()
        {
            StageRunRuntime.ResetForTests();
            yield return EnterCanonicalStation();

            StageRunContext context = StageRunRuntime.ActiveContext;
            Scene station = SceneManager.GetActiveScene();
            CombatEncounterController encounter =
                RequireSingleSceneComponent<CombatEncounterController>(station);
            EncounterTerminalResolutionCoordinator coordinator = encounter.TerminalCoordinator;
            OlympusStationCombatResultPresenter presenter =
                RequireSingleSceneComponent<OlympusStationCombatResultPresenter>(station);
            OlympusStationRunFactCollector collector =
                RequireSingleSceneComponent<OlympusStationRunFactCollector>(station);

            presenter.enabled = false;
            yield return null;

            Assert.That(presenter.HasCanonicalStageRun, Is.False);
            Assert.That(coordinator.State, Is.EqualTo(EncounterTerminalCoordinatorState.Cancelled));
            Assert.That(context.LifecycleState, Is.EqualTo(StageRunLifecycleState.Disposed));
            Assert.That(context.CommittedSummary, Is.Null);
            Assert.That(context.AbortRecord.AbortReason, Is.EqualTo(StageRunAbortReason.StationResultPresenterLost));
            Assert.That(collector.IsBound, Is.False);
            Assert.That(StageRunRuntime.LastAbortRecord, Is.SameAs(context.AbortRecord));
            Assert.That(SceneManager.GetSceneByName("UI_StageClear").isLoaded, Is.False);
            AssertCurrentSchemaClosureRows(context.AbortRecord);

            StageRunAbortRecord sealedAbort = context.AbortRecord;
            presenter.enabled = true;
            yield return null;
            Assert.That(presenter.HasCanonicalStageRun, Is.False);
            Assert.That(context.AbortRecord, Is.SameAs(sealedAbort));
            Assert.That(StageRunRuntime.LastAbortRecord, Is.SameAs(sealedAbort));
        }

        [UnityTest]
        public IEnumerator ExplicitAbortDuringDrainingCancelsAuthorityAndSealsOnce()
        {
            StageRunRuntime.ResetForTests();
            yield return EnterCanonicalStation();

            StageRunContext context = StageRunRuntime.ActiveContext;
            Scene station = SceneManager.GetActiveScene();
            CombatEncounterController encounter =
                RequireSingleSceneComponent<CombatEncounterController>(station);
            EncounterTerminalResolutionCoordinator coordinator = encounter.TerminalCoordinator;
            CombatHealth playerHealth = ReadPrivateField<CombatHealth>(encounter, "playerHealth");
            float initialHealth = playerHealth.CurrentHealth;
            EncounterTerminalCoordinatorState callbackState = default;
            StageRunAbortRecord callbackRecord = null;
            string callbackError = string.Empty;
            bool callbackAbortAccepted = false;
            int damageCount = 0;
            playerHealth.Damaged += HandleDamaged;
            try
            {
                CombatRootAdmissionResult admission = encounter.AdmitCombatRoot(
                    "test.explicit-abort-during-draining",
                    root =>
                    {
                        Assert.That(
                            root.TryApplyDamage(
                                playerHealth,
                                new DamageInfo(
                                    null,
                                    DamageTeam.Enemy,
                                    1f,
                                    playerHealth.transform.position,
                                    Vector3.forward,
                                    0f,
                                    DamageResponsePolicy.DamageOnly,
                                    CombatControlLockPolicy.None)),
                            Is.True);
                        Assert.That(
                            root.TryApplyDamage(
                                playerHealth,
                                new DamageInfo(
                                    null,
                                    DamageTeam.Enemy,
                                    1f,
                                    playerHealth.transform.position,
                                    Vector3.forward,
                                    0f,
                                    DamageResponsePolicy.DamageOnly,
                                    CombatControlLockPolicy.None)),
                            Is.True);
                    });

                Assert.That(admission.Disposition, Is.EqualTo(CombatRootAdmissionDisposition.Executed));
                Assert.That(admission.CoordinatorState, Is.EqualTo(EncounterTerminalCoordinatorState.Cancelled));
            }
            finally
            {
                playerHealth.Damaged -= HandleDamaged;
            }

            Assert.That(callbackState, Is.EqualTo(EncounterTerminalCoordinatorState.Draining));
            Assert.That(callbackAbortAccepted, Is.True, callbackError);
            Assert.That(damageCount, Is.EqualTo(1));
            Assert.That(playerHealth.CurrentHealth, Is.EqualTo(initialHealth - 1f));
            Assert.That(coordinator.State, Is.EqualTo(EncounterTerminalCoordinatorState.Cancelled));
            Assert.That(context.LifecycleState, Is.EqualTo(StageRunLifecycleState.Disposed));
            Assert.That(context.CommittedSummary, Is.Null);
            Assert.That(callbackRecord, Is.SameAs(context.AbortRecord));
            Assert.That(callbackRecord.AbortReason, Is.EqualTo(StageRunAbortReason.ExplicitAbort));
            Assert.That(callbackRecord.HasValidIntegrity(), Is.True);
            Assert.That(StageRunRuntime.LastAbortRecord, Is.SameAs(callbackRecord));
            Assert.That(SceneManager.GetSceneByName("UI_StageClear").isLoaded, Is.False);
            AssertCurrentSchemaClosureRows(callbackRecord);

            Assert.That(
                StageRunRuntime.TryAbortActiveRun(
                    encounter,
                    StageRunAbortReason.ExplicitAbort,
                    out StageRunAbortRecord duplicateRecord,
                    out _),
                Is.True);
            Assert.That(duplicateRecord, Is.SameAs(callbackRecord));
            Assert.That(
                StageRunRuntime.TryAbortActiveRun(
                    null,
                    StageRunAbortReason.ExplicitAbort,
                    out StageRunAbortRecord nullEncounterReplay,
                    out string nullEncounterError),
                Is.False);
            Assert.That(nullEncounterReplay, Is.Null);
            Assert.That(nullEncounterError, Does.Contain("does not match"));

            GameObject foreignEncounterObject = new("ForeignExplicitAbortEncounter");
            foreignEncounterObject.SetActive(false);
            CombatEncounterController foreignEncounter =
                foreignEncounterObject.AddComponent<CombatEncounterController>();
            try
            {
                Assert.That(
                    StageRunRuntime.TryAbortActiveRun(
                        foreignEncounter,
                        StageRunAbortReason.ExplicitAbort,
                        out StageRunAbortRecord foreignEncounterReplay,
                        out string foreignEncounterError),
                    Is.False);
                Assert.That(foreignEncounterReplay, Is.Null);
                Assert.That(foreignEncounterError, Does.Contain("does not match"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(foreignEncounterObject);
            }

            Assert.That(
                StageRunRuntime.TryAbortActiveRun(
                    encounter,
                    StageRunAbortReason.UnexpectedSceneExit,
                    out StageRunAbortRecord conflictingRecord,
                    out _),
                Is.False);
            Assert.That(conflictingRecord, Is.Null);
            Assert.That(
                StageRunRuntime.TryReplayAbortTupleForTests(
                    StageRunAbortReason.ExplicitAbort,
                    StageRunTerminalCoordinatorInvalidationDisposition.CancellationRequested,
                    0,
                    0,
                    out StageRunAbortRecord mismatchedTupleRecord,
                    out _),
                Is.False);
            Assert.That(mismatchedTupleRecord, Is.Null);
            Assert.That(StageRunRuntime.LastAbortRecord, Is.SameAs(callbackRecord));

            void HandleDamaged(DamageInfo _)
            {
                damageCount++;
                callbackState = coordinator.State;
                callbackAbortAccepted = StageRunRuntime.TryAbortActiveRun(
                    encounter,
                    StageRunAbortReason.ExplicitAbort,
                    out callbackRecord,
                    out callbackError);
            }
        }

        [UnityTest]
        public IEnumerator FinalizingActiveSceneExitCancelsBeforeActualUnloadWithoutSecondAbort()
        {
            StageRunRuntime.ResetForTests();
            yield return EnterCanonicalStation();

            StageRunContext context = StageRunRuntime.ActiveContext;
            Scene station = SceneManager.GetActiveScene();
            CombatEncounterController encounter =
                RequireSingleSceneComponent<CombatEncounterController>(station);
            EncounterTerminalResolutionCoordinator coordinator = encounter.TerminalCoordinator;
            CombatHealth playerHealth = ReadPrivateField<CombatHealth>(encounter, "playerHealth");

            Scene host = SceneManager.CreateScene("StageRunFinalizingExitHost");
            Assert.That(SceneManager.GetActiveScene().handle, Is.EqualTo(station.handle));
            EncounterTerminalCoordinatorState boundaryState = default;
            StageRunAbortRecord recordAtBoundary = null;
            coordinator.SetFinalizationBoundaryForTests(current =>
            {
                boundaryState = current.State;
                Assert.That(SceneManager.SetActiveScene(host), Is.True);
                recordAtBoundary = context.AbortRecord;
            });

            CombatRootAdmissionResult admission = encounter.AdmitCombatRoot(
                "test.finalizing-active-scene-exit",
                root =>
                {
                    Assert.That(
                        root.TryApplyDamage(
                            playerHealth,
                            new DamageInfo(
                                null,
                                DamageTeam.Enemy,
                                1f,
                                playerHealth.transform.position,
                                Vector3.forward,
                                0f,
                                DamageResponsePolicy.DamageOnly,
                                CombatControlLockPolicy.None)),
                        Is.True);
                });

            Assert.That(boundaryState, Is.EqualTo(EncounterTerminalCoordinatorState.Finalizing));
            Assert.That(admission.Disposition, Is.EqualTo(CombatRootAdmissionDisposition.Executed));
            Assert.That(admission.CoordinatorState, Is.EqualTo(EncounterTerminalCoordinatorState.Cancelled));
            Assert.That(coordinator.State, Is.EqualTo(EncounterTerminalCoordinatorState.Cancelled));
            Assert.That(context.LifecycleState, Is.EqualTo(StageRunLifecycleState.Disposed));
            Assert.That(context.CommittedSummary, Is.Null);
            Assert.That(context.AbortRecord, Is.Not.Null);
            Assert.That(recordAtBoundary, Is.SameAs(context.AbortRecord));
            Assert.That(context.AbortRecord.AbortReason, Is.EqualTo(StageRunAbortReason.UnexpectedSceneExit));
            Assert.That(
                context.AbortCloseAuthority.CoordinatorInvalidationDisposition,
                Is.EqualTo(StageRunTerminalCoordinatorInvalidationDisposition.CancellationRequested));
            Assert.That(context.AbortCloseAuthority.CoordinatorRootAdmissionSequence, Is.GreaterThan(0));
            Assert.That(context.AbortCloseAuthority.CoordinatorEpoch, Is.GreaterThan(0));
            Assert.That(context.AbortRecord.HasValidIntegrity(), Is.True);
            AssertCurrentSchemaClosureRows(context.AbortRecord);

            StageRunAbortRecord sealedAbort = context.AbortRecord;
            AsyncOperation unload = SceneManager.UnloadSceneAsync(station);
            Assert.That(unload, Is.Not.Null);
            while (!unload.isDone)
            {
                yield return null;
            }

            yield return null;
            Assert.That(context.AbortRecord, Is.SameAs(sealedAbort));
            Assert.That(StageRunRuntime.LastAbortRecord, Is.SameAs(sealedAbort));
            Assert.That(SceneManager.GetSceneByName("UI_StageClear").isLoaded, Is.False);
        }

        [UnityTest]
        public IEnumerator StationSceneExitCancelsCoordinatorBeforeSceneSubjectsAreLost()
        {
            StageRunRuntime.ResetForTests();
            yield return EnterCanonicalStation();

            StageRunContext context = StageRunRuntime.ActiveContext;
            Scene station = SceneManager.GetActiveScene();
            CombatEncounterController encounter =
                RequireSingleSceneComponent<CombatEncounterController>(station);
            EncounterTerminalResolutionCoordinator coordinator = encounter.TerminalCoordinator;
            StageCountOneEncounterExecutor executor =
                RequireSingleSceneComponent<StageCountOneEncounterExecutor>(station);
            float activationDeadline = Time.realtimeSinceStartup + 2f;
            while (executor.State != StageCountOneEncounterState.Active)
            {
                Assert.Less(Time.realtimeSinceStartup, activationDeadline, executor.LastError);
                yield return null;
            }

            Assert.That(executor.ActivationCount, Is.EqualTo(1));
            Assert.That(executor.TicketCount, Is.EqualTo(2));
            Assert.That(executor.ActivatedTicketCount, Is.EqualTo(2));
            Assert.That(executor.OwnedObjectCount, Is.EqualTo(2));
            Assert.That(executor.ActiveParticipationCount, Is.EqualTo(2));
            Assert.That(executor.HasCombatantParticipation, Is.True);
            Assert.That(executor.HasSceneLease, Is.True);

            StageAddEncounterTicketSnapshot rangedTicket = executor.GetTicketSnapshot(1);
            BasicSoldierProjectileAttackDriver rangedDriver = rangedTicket.ProjectileDriver;
            Assert.That(rangedDriver, Is.Not.Null);
            MoveCombatSubjectToAnchor(
                encounter.PlayerHealth,
                RequireSingleSceneTransform(station, "PlayerForwardBoundaryAnchor"));
            float projectileDeadline = Time.realtimeSinceStartup + 18f;
            while (rangedDriver.ActiveProjectileCount == 0)
            {
                Assert.Less(
                    Time.realtimeSinceStartup,
                    projectileDeadline,
                    "Timed out waiting for an in-flight RifleCrossfire projectile before scene unload.");
                yield return null;
            }

            int executorDisableCallbackCount = 0;
            StageCountOneEncounterState executorStateAtDisable = default;
            int executorCancellationCountAtDisable = -1;
            int executorCompletionCountAtDisable = -1;
            bool executorOwnershipClearedAtDisable = false;
            bool executorParticipationClearedAtDisable = false;
            bool executorSceneLeaseReleasedAtDisable = false;
            int activeProjectilesAtDisable = -1;
            StageCountOneExecutorShutdownProbe shutdownProbe =
                executor.gameObject.AddComponent<StageCountOneExecutorShutdownProbe>();
            shutdownProbe.Disabled = () =>
            {
                executorDisableCallbackCount++;
                executorStateAtDisable = executor.State;
                executorCancellationCountAtDisable = executor.CancellationCount;
                executorCompletionCountAtDisable = executor.CompletionCount;
                executorOwnershipClearedAtDisable = executor.OwnedObjectCount == 0
                    && executor.OwnedRoot == null
                    && executor.OwnedHealth == null
                    && executor.OwnedAgent == null
                    && executor.OwnedSensor == null;
                executorParticipationClearedAtDisable = !executor.HasCombatantParticipation;
                executorSceneLeaseReleasedAtDisable = !executor.HasSceneLease;
                activeProjectilesAtDisable = rangedDriver.ActiveProjectileCount;
            };
            SceneManager.CreateScene("StageRunRouteUnloadHost");

            AsyncOperation unload = SceneManager.UnloadSceneAsync(station);
            Assert.That(unload, Is.Not.Null);
            while (!unload.isDone)
            {
                yield return null;
            }

            yield return null;
            Assert.That(executorDisableCallbackCount, Is.EqualTo(1));
            Assert.That(
                executorStateAtDisable,
                Is.EqualTo(StageCountOneEncounterState.Cancelled));
            Assert.That(executorCancellationCountAtDisable, Is.EqualTo(1));
            Assert.That(executorCompletionCountAtDisable, Is.Zero);
            Assert.That(
                executorOwnershipClearedAtDisable,
                Is.True,
                "Scene unload must clear the runtime Add ownership before the executor disable boundary returns.");
            Assert.That(
                executorParticipationClearedAtDisable,
                Is.True,
                "Scene unload must clear the Add participation lease before the executor disable boundary returns.");
            Assert.That(
                executorSceneLeaseReleasedAtDisable,
                Is.True,
                "Scene unload must release the count-one scene lease before the executor disable boundary returns.");
            Assert.That(
                activeProjectilesAtDisable,
                Is.Zero,
                "Scene unload must synchronously deactivate the in-flight RifleCrossfire projectile.");
            Assert.That(coordinator.State, Is.EqualTo(EncounterTerminalCoordinatorState.Cancelled));
            Assert.That(context.LifecycleState, Is.EqualTo(StageRunLifecycleState.Disposed));
            Assert.That(context.CommittedSummary, Is.Null);
            Assert.That(context.AbortRecord, Is.Not.Null);
            StageRunAbortReason reason = context.AbortRecord.AbortReason;
            Assert.That(
                reason == StageRunAbortReason.UnexpectedSceneExit
                    || reason == StageRunAbortReason.StationFactCollectorLost
                    || reason == StageRunAbortReason.StationResultPresenterLost,
                Is.True,
                $"Unexpected Station exit abort reason: {reason}.");
            Assert.That(StageRunRuntime.LastAbortRecord, Is.SameAs(context.AbortRecord));
            Assert.That(SceneManager.GetSceneByName("UI_StageClear").isLoaded, Is.False);
            AssertCurrentSchemaClosureRows(context.AbortRecord);
        }

        [UnityTest]
        public IEnumerator LegacyStandaloneStationCannotActivateCountOneAddWithoutCanonicalRun()
        {
            StageRunRuntime.ResetForTests();
            yield return LoadSingleScene(StationScenePath);
            yield return null;

            StageCountOneEncounterExecutor executor =
                RequireSingleSceneComponent<StageCountOneEncounterExecutor>(SceneManager.GetActiveScene());
            Assert.That(executor.State, Is.EqualTo(StageCountOneEncounterState.Faulted));
            Assert.That(
                executor.LastError,
                Does.Contain("does not own the executor scene path"));
            Assert.That(executor.ActivationCount, Is.Zero);
            Assert.That(executor.OwnedObjectCount, Is.Zero);
            Assert.That(executor.HasSceneLease, Is.False);
        }

        [UnityTest]
        public IEnumerator CanonicalStationStagesBothOrderedAddsInactiveUntilGuideRelease()
        {
            StageRunRuntime.ResetForTests();
            yield return EnterCanonicalStation(releaseEntryGuide: false);

            Scene station = SceneManager.GetActiveScene();
            StageCountOneEncounterExecutor executor =
                RequireSingleSceneComponent<StageCountOneEncounterExecutor>(station);
            float stagingDeadline = Time.realtimeSinceStartup + 2f;
            while (executor.State != StageCountOneEncounterState.WaitingForActivation
                || executor.TicketCount != 2)
            {
                Assert.That(executor.State, Is.Not.EqualTo(StageCountOneEncounterState.Faulted), executor.LastError);
                Assert.Less(Time.realtimeSinceStartup, stagingDeadline, executor.LastError);
                yield return null;
            }

            Assert.That(executor.PendingTicketCount, Is.EqualTo(2));
            Assert.That(executor.ActiveTicketCount, Is.Zero);
            Assert.That(executor.ActivatedTicketCount, Is.Zero);
            Assert.That(executor.OwnedObjectCount, Is.EqualTo(2));
            Assert.That(executor.ActiveParticipationCount, Is.Zero);
            Assert.That(executor.PlayerTargetSelector.RuntimeTargetCandidateCount, Is.Zero);
            Assert.That(executor.HasSceneLease, Is.True);
            Assert.That(executor.LastReceipt, Is.Null);
            for (int i = 0; i < executor.TicketCount; i++)
            {
                StageAddEncounterTicketSnapshot staged = executor.GetTicketSnapshot(i);
                Assert.That(staged.SourceOrdinal, Is.EqualTo(i));
                Assert.That(staged.State, Is.EqualTo(StageAddEncounterTicketState.Pending));
                Assert.That(staged.ActivationSequence, Is.Zero);
                Assert.That(staged.TerminalSequence, Is.Zero);
                Assert.That(staged.Root, Is.Not.Null);
                Assert.That(staged.Root.activeSelf, Is.False);
                Assert.That(staged.ParticipationRegistered, Is.False);
            }

            yield return ReleaseStationEntryGuide(station);
            float activationDeadline = Time.realtimeSinceStartup + 2f;
            while (executor.ActiveTicketCount != 2)
            {
                Assert.That(executor.State, Is.Not.EqualTo(StageCountOneEncounterState.Faulted), executor.LastError);
                Assert.Less(Time.realtimeSinceStartup, activationDeadline, executor.LastError);
                yield return null;
            }

            RequireOrderedActiveAddTickets(executor);
        }

        [UnityTest]
        public IEnumerator CanonicalStationLaterTicketActivationFailureRollsBackWholeStagedPlan()
        {
            StageRunRuntime.ResetForTests();
            yield return EnterCanonicalStation(releaseEntryGuide: false);

            Scene station = SceneManager.GetActiveScene();
            StageCountOneEncounterExecutor executor =
                RequireSingleSceneComponent<StageCountOneEncounterExecutor>(station);
            float stagingDeadline = Time.realtimeSinceStartup + 2f;
            while (executor.State != StageCountOneEncounterState.WaitingForActivation
                || executor.TicketCount != 2)
            {
                Assert.That(executor.State, Is.Not.EqualTo(StageCountOneEncounterState.Faulted), executor.LastError);
                Assert.Less(Time.realtimeSinceStartup, stagingDeadline, executor.LastError);
                yield return null;
            }

            StageAddEncounterTicketSnapshot left = executor.GetTicketSnapshot(0);
            StageAddEncounterTicketSnapshot right = executor.GetTicketSnapshot(1);
            Assert.That(left.Root.activeSelf, Is.False);
            Assert.That(right.Root.activeSelf, Is.False);
            right.Sensor.enabled = false;

            yield return ReleaseStationEntryGuide(station);
            float faultDeadline = Time.realtimeSinceStartup + 2f;
            while (executor.State != StageCountOneEncounterState.Faulted)
            {
                Assert.Less(Time.realtimeSinceStartup, faultDeadline, executor.LastError);
                yield return null;
            }

            Assert.That(executor.ActivationCount, Is.EqualTo(1));
            Assert.That(executor.ActivatedTicketCount, Is.EqualTo(1));
            Assert.That(executor.GetTicketSnapshot(0).State, Is.EqualTo(StageAddEncounterTicketState.Cancelled));
            Assert.That(executor.GetTicketSnapshot(1).State, Is.EqualTo(StageAddEncounterTicketState.Faulted));
            Assert.That(executor.OwnedObjectCount, Is.Zero);
            Assert.That(executor.ActiveParticipationCount, Is.Zero);
            Assert.That(executor.PlayerTargetSelector.RuntimeTargetCandidateCount, Is.Zero);
            Assert.That(executor.IsQuiescent, Is.True);
            AssertCapturedAddRootsInactive(new[] { left, right });
            Assert.That(executor.LastReceipt, Is.Not.Null);
            Assert.That(executor.LastReceipt.FinalState, Is.EqualTo(StageCountOneEncounterState.Faulted));
            Assert.That(
                executor.LastReceipt.TryValidateIntegrity(out string rollbackReceiptError),
                Is.True,
                rollbackReceiptError);
        }

        [UnityTest]
        public IEnumerator CanonicalStationPendingRangedDriverLossFaultsActivationAndRollsBackPlan()
        {
            StageRunRuntime.ResetForTests();
            yield return EnterCanonicalStation(releaseEntryGuide: false);

            Scene station = SceneManager.GetActiveScene();
            StageCountOneEncounterExecutor executor =
                RequireSingleSceneComponent<StageCountOneEncounterExecutor>(station);
            float stagingDeadline = Time.realtimeSinceStartup + 2f;
            while (executor.State != StageCountOneEncounterState.WaitingForActivation
                || executor.TicketCount != 2)
            {
                Assert.That(executor.State, Is.Not.EqualTo(StageCountOneEncounterState.Faulted), executor.LastError);
                Assert.Less(Time.realtimeSinceStartup, stagingDeadline, executor.LastError);
                yield return null;
            }

            StageAddEncounterTicketSnapshot left = executor.GetTicketSnapshot(0);
            StageAddEncounterTicketSnapshot right = executor.GetTicketSnapshot(1);
            Assert.That(right.ProjectileDriver, Is.Not.Null);
            right.ProjectileDriver.enabled = false;

            yield return ReleaseStationEntryGuide(station);
            float faultDeadline = Time.realtimeSinceStartup + 2f;
            while (executor.State != StageCountOneEncounterState.Faulted)
            {
                Assert.Less(Time.realtimeSinceStartup, faultDeadline, executor.LastError);
                yield return null;
            }

            Assert.That(executor.LastError, Does.Contain("projectile participation lease"));
            Assert.That(executor.GetTicketSnapshot(0).State, Is.EqualTo(StageAddEncounterTicketState.Cancelled));
            Assert.That(executor.GetTicketSnapshot(1).State, Is.EqualTo(StageAddEncounterTicketState.Faulted));
            Assert.That(executor.OwnedObjectCount, Is.Zero);
            Assert.That(executor.ActiveParticipationCount, Is.Zero);
            Assert.That(executor.IsQuiescent, Is.True);
            AssertCapturedAddRootsInactive(new[] { left, right });
            Assert.That(executor.LastReceipt, Is.Not.Null);
            Assert.That(
                executor.LastReceipt.TryValidateIntegrity(out string receiptError),
                Is.True,
                receiptError);
        }

        [UnityTest]
        public IEnumerator CanonicalStationActiveRangedDriverLossFaultsAndCleansInFlightProjectile()
        {
            StageRunRuntime.ResetForTests();
            yield return EnterCanonicalStation();

            StageCountOneEncounterExecutor executor =
                RequireSingleSceneComponent<StageCountOneEncounterExecutor>(SceneManager.GetActiveScene());
            float activationDeadline = Time.realtimeSinceStartup + 2f;
            while (executor.State != StageCountOneEncounterState.Active
                || executor.ActiveTicketCount != 2)
            {
                Assert.Less(Time.realtimeSinceStartup, activationDeadline, executor.LastError);
                yield return null;
            }

            StageAddEncounterTicketSnapshot[] tickets = RequireOrderedActiveAddTickets(executor);
            BasicSoldierProjectileAttackDriver rangedDriver = tickets[1].ProjectileDriver;
            Assert.That(rangedDriver, Is.Not.Null);
            CombatEncounterController encounter =
                RequireSingleSceneComponent<CombatEncounterController>(SceneManager.GetActiveScene());
            Vector3 firingLaneDirection = Vector3.ProjectOnPlane(
                encounter.PlayerHealth.transform.position - tickets[1].Health.transform.position,
                Vector3.up).normalized;
            if (firingLaneDirection.sqrMagnitude < 0.99f)
            {
                firingLaneDirection = Vector3.forward;
            }

            MoveCombatSubjectToPosition(
                encounter.PlayerHealth,
                tickets[1].Health.transform.position + firingLaneDirection * 4.5f);
            yield return null;

            float projectileDeadline = Time.realtimeSinceStartup + 8f;
            while (rangedDriver.ActiveProjectileCount == 0)
            {
                Assert.That(executor.State, Is.EqualTo(StageCountOneEncounterState.Active), executor.LastError);
                Assert.Less(
                    Time.realtimeSinceStartup,
                    projectileDeadline,
                    "The RifleCrossfire ticket did not produce an in-flight projectile before lease loss. "
                    + $"fired={rangedDriver.FiredCount}, lastImpact={rangedDriver.LastFiredProjectile?.LastImpactResult}, "
                    + $"distance={Vector3.ProjectOnPlane(tickets[1].Health.transform.position - encounter.PlayerHealth.transform.position, Vector3.up).magnitude:F2}.");
                yield return null;
            }

            rangedDriver.enabled = false;
            Assert.That(
                rangedDriver.ActiveProjectileCount,
                Is.Zero,
                "Driver disable must synchronously park every in-flight projectile.");
            float faultDeadline = Time.realtimeSinceStartup + 2f;
            while (executor.State != StageCountOneEncounterState.Faulted)
            {
                Assert.Less(Time.realtimeSinceStartup, faultDeadline, executor.LastError);
                yield return null;
            }

            Assert.That(executor.LastError, Does.Contain("projectile participation lease"));
            Assert.That(executor.GetTicketSnapshot(0).State, Is.EqualTo(StageAddEncounterTicketState.Cancelled));
            Assert.That(executor.GetTicketSnapshot(1).State, Is.EqualTo(StageAddEncounterTicketState.Faulted));
            Assert.That(executor.OwnedObjectCount, Is.Zero);
            Assert.That(executor.ActiveParticipationCount, Is.Zero);
            Assert.That(executor.IsQuiescent, Is.True);
            AssertCapturedAddRootsInactive(tickets);
            Assert.That(rangedDriver.ActiveProjectileCount, Is.Zero);
            Assert.That(executor.LastReceipt, Is.Not.Null);
            Assert.That(
                executor.LastReceipt.TryValidateIntegrity(out string receiptError),
                Is.True,
                receiptError);
        }

        [UnityTest]
        public IEnumerator CanonicalStationGuideReleaseActivatesAndCompletesOrderedTwoAddPlan()
        {
            StageRunRuntime.ResetForTests();
            yield return EnterCanonicalStation();

            Scene station = SceneManager.GetActiveScene();
            StageCountOneEncounterExecutor executor =
                RequireSingleSceneComponent<StageCountOneEncounterExecutor>(station);
            float deadline = Time.realtimeSinceStartup + 2f;
            while (executor.State != StageCountOneEncounterState.Active)
            {
                Assert.Less(Time.realtimeSinceStartup, deadline, executor.LastError);
                yield return null;
            }

            StageAddEncounterTicketSnapshot[] addTickets =
                RequireOrderedActiveAddTickets(executor);
            CombatHealth addHealth = addTickets[0].Health;
            ICombatAiAgent addAgent = addTickets[0].Agent;
            CombatTargetSensor addSensor = addTickets[0].Sensor;
            CombatHealth secondAddHealth = addTickets[1].Health;
            ICombatAiAgent secondAddAgent = addTickets[1].Agent;
            CombatTargetSensor secondAddSensor = addTickets[1].Sensor;
            BasicSoldierProjectileAttackDriver rangedDriver = addTickets[1].ProjectileDriver;
            Transform rangedProjectileRoot = addTickets[1].ProjectileRoot;
            PlayerCombatTargetSelector playerTargetSelector = executor.PlayerTargetSelector;
            GameObject ownedRoot = addTickets[0].Root;
            GameObject secondOwnedRoot = addTickets[1].Root;
            CombatEncounterController encounter =
                RequireSingleSceneComponent<CombatEncounterController>(station);
            CombatHealth playerHealth = encounter.PlayerHealth;
            CombatHealth bossHealth = encounter.EnemyHealth;
            Assert.That(addHealth, Is.Not.Null);
            Assert.That(addHealth.Team, Is.EqualTo(DamageTeam.Enemy));
            Assert.That(addHealth.IsAlive, Is.True);
            Assert.That(addAgent, Is.Not.Null);
            Assert.That(addAgent.SelfHealth, Is.SameAs(addHealth));
            Assert.That(addAgent.PatternProfile, Is.Not.Null);
            Assert.That(addAgent.PatternProfile.PatternId, Is.EqualTo("HeavyWindup"));
            Assert.That(addAgent.PatternProfile.AttackShape, Is.EqualTo(CombatAiAttackShape.MeleeArc));
            Assert.That(addSensor, Is.Not.Null);
            Assert.That(
                addSensor.SearchRadius,
                Is.GreaterThanOrEqualTo(addAgent.PatternProfile.AttackRange));
            Assert.That(addAgent.TargetSensor, Is.SameAs(addSensor));
            Assert.That(addSensor.SelfHealth, Is.SameAs(addHealth));
            Assert.That(secondAddHealth, Is.Not.Null);
            Assert.That(secondAddAgent, Is.Not.Null);
            Assert.That(secondAddAgent.PatternProfile.PatternId, Is.EqualTo("RifleCrossfire"));
            Assert.That(
                secondAddAgent.PatternProfile.AttackShape,
                Is.EqualTo(CombatAiAttackShape.ProjectileLine));
            Assert.That(rangedDriver, Is.Not.Null);
            Assert.That(rangedProjectileRoot, Is.Not.Null);
            Assert.That(rangedDriver.SourceHealth, Is.SameAs(secondAddHealth));
            Assert.That(rangedDriver.TargetSensor, Is.SameAs(secondAddSensor));
            Assert.That(rangedDriver.RuntimeProjectileRoot, Is.SameAs(rangedProjectileRoot));
            Assert.That(rangedDriver.HasIndependentRuntimeProjectileRoot, Is.True);
            Assert.That(rangedProjectileRoot.parent.gameObject, Is.SameAs(secondOwnedRoot));
            Assert.That(playerHealth, Is.Not.Null);
            Assert.That(bossHealth, Is.Not.Null);
            Assert.That(bossHealth, Is.Not.SameAs(addHealth));
            Assert.That(playerTargetSelector, Is.Not.Null);
            Assert.That(playerTargetSelector.SelfHealth, Is.SameAs(playerHealth));
            Assert.That(playerTargetSelector.ContainsAuthoredTargetCandidate(bossHealth), Is.True);
            Assert.That(playerTargetSelector.ContainsRuntimeTargetCandidate(addHealth), Is.True);
            Assert.That(playerTargetSelector.ContainsRuntimeTargetCandidate(secondAddHealth), Is.True);
            Assert.That(playerTargetSelector.RuntimeTargetCandidateCount, Is.EqualTo(2));
            Assert.That(executor.HasCombatantParticipation, Is.True);
            Assert.That(addSensor.TargetCandidateCount, Is.EqualTo(1));
            Assert.That(addSensor.ContainsTargetCandidate(playerHealth), Is.True);
            if (addSensor.TryGetCurrentTarget(out _, out CombatHealth initialSensedHealth))
            {
                Assert.That(initialSensedHealth, Is.SameAs(playerHealth));
            }
            Assert.That(executor.ActivationCount, Is.EqualTo(1));
            Assert.That(executor.OwnedObjectCount, Is.EqualTo(2));
            Assert.That(executor.HasSceneLease, Is.True);
            Assert.That(executor.TryActivate(out string duplicateError), Is.True, duplicateError);
            Assert.That(executor.ActivationCount, Is.EqualTo(1));

            Vector3 lockViewDirection = Vector3.ProjectOnPlane(
                addHealth.transform.position - playerHealth.transform.position,
                Vector3.up).normalized;
            Assert.That(lockViewDirection.sqrMagnitude, Is.GreaterThan(0.99f));
            Assert.That(
                playerTargetSelector.TryGetBestLockTarget(
                    playerHealth.transform.position,
                    lockViewDirection,
                    50f,
                    15f,
                    addHealth,
                    10f,
                    out CombatHealth lockTarget,
                    out _,
                    out _),
                Is.True,
                "The player's directed lock query did not admit the runtime Add candidate.");
            Assert.That(lockTarget, Is.SameAs(addHealth));

            Vector3 initialAddPosition = addHealth.transform.position;
            float initialPlanarDistance = Vector3.ProjectOnPlane(
                addHealth.transform.position - playerHealth.transform.position,
                Vector3.up).magnitude;
            float closestPlanarDistance = initialPlanarDistance;
            float approachProofDeadline = Time.realtimeSinceStartup + 3f;
            while (initialPlanarDistance - closestPlanarDistance < 0.25f)
            {
                closestPlanarDistance = Mathf.Min(
                    closestPlanarDistance,
                    Vector3.ProjectOnPlane(
                        addHealth.transform.position - playerHealth.transform.position,
                        Vector3.up).magnitude);
                Assert.That(executor.State, Is.EqualTo(StageCountOneEncounterState.Active));
                Assert.Less(
                    Time.realtimeSinceStartup,
                    approachProofDeadline,
                    "The runtime Add did not begin authored approach motion toward the lower entry position.");
                yield return null;
            }

            Transform playerForwardBoundary = RequireSingleSceneTransform(
                station,
                "PlayerForwardBoundaryAnchor");
            MoveCombatSubjectToAnchor(playerHealth, playerForwardBoundary);
            yield return null;

            float playerHealthBeforeAddAttack = playerHealth.CurrentHealth;
            bool observedWindup = addAgent.CurrentPatternState == CombatAiPatternState.Windup;
            int exactAddDamageCount = 0;
            DamageInfo exactAddDamage = default;
            playerHealth.Damaged += HandlePlayerDamaged;
            addAgent.PatternStateChanged += HandleAddPatternStateChanged;
            try
            {
                float attackDeadline = Time.realtimeSinceStartup + 18f;
                while (exactAddDamageCount == 0)
                {
                    closestPlanarDistance = Mathf.Min(
                        closestPlanarDistance,
                        Vector3.ProjectOnPlane(
                            addHealth.transform.position - playerHealth.transform.position,
                            Vector3.up).magnitude);
                    observedWindup |= addAgent.CurrentPatternState == CombatAiPatternState.Windup;
                    Assert.That(executor.State, Is.EqualTo(StageCountOneEncounterState.Active));
                    Assert.That(addHealth.IsAlive, Is.True);
                    Assert.Less(
                        Time.realtimeSinceStartup,
                        attackDeadline,
                        "Timed out waiting for the runtime Add to approach, wind up, and damage the player. "
                        + $"initialDistance={initialPlanarDistance:F2}, closestDistance={closestPlanarDistance:F2}, "
                        + $"currentDistance={Vector3.ProjectOnPlane(addHealth.transform.position - playerHealth.transform.position, Vector3.up).magnitude:F2}, "
                        + $"initialAdd={initialAddPosition:F2}, currentAdd={addHealth.transform.position:F2}, player={playerHealth.transform.position:F2}, "
                        + $"state={addAgent.CurrentPatternState}, sensorTarget={addSensor.CurrentTargetHealth?.name ?? "none"}, "
                        + $"timeScale={Time.timeScale:F2}.");
                    yield return null;
                }
            }
            finally
            {
                playerHealth.Damaged -= HandlePlayerDamaged;
                addAgent.PatternStateChanged -= HandleAddPatternStateChanged;
            }

            Assert.That(
                closestPlanarDistance,
                Is.LessThan(initialPlanarDistance - 0.1f),
                "The runtime Add never closed measurable planar distance toward the terminal player.");
            Assert.That(observedWindup, Is.True, "The runtime Add never published a Windup state.");
            Assert.That(exactAddDamageCount, Is.EqualTo(1));
            Assert.That(exactAddDamage.Source, Is.SameAs(addHealth));
            Assert.That(exactAddDamage.SourceTeam, Is.EqualTo(DamageTeam.Enemy));
            Assert.That(exactAddDamage.Amount, Is.GreaterThan(0f));
            Assert.That(playerHealth.CurrentHealth, Is.LessThan(playerHealthBeforeAddAttack));
            Assert.That(
                addSensor.TryGetCurrentTarget(
                    out Transform sensedPlayerTransform,
                    out CombatHealth sensedPlayerHealth),
                Is.True,
                "The runtime Add reached attack range without its sensor acquiring the exact player.");
            Assert.That(sensedPlayerHealth, Is.SameAs(playerHealth));
            Assert.That(sensedPlayerTransform, Is.SameAs(playerHealth.transform));

            Assert.That(ApplyLethalDamage(addHealth, DamageTeam.Player), Is.True);
            StageAddEncounterTicketSnapshot completedFirst = executor.GetTicketSnapshot(0);
            StageAddEncounterTicketSnapshot survivingSecond = executor.GetTicketSnapshot(1);
            Assert.That(executor.State, Is.EqualTo(StageCountOneEncounterState.Active));
            Assert.That(executor.CompletionCount, Is.Zero);
            Assert.That(executor.LastReceipt, Is.Null);
            Assert.That(executor.CompletedTicketCount, Is.EqualTo(1));
            Assert.That(executor.ActiveTicketCount, Is.EqualTo(1));
            Assert.That(executor.OwnedObjectCount, Is.EqualTo(1));
            Assert.That(executor.ActiveParticipationCount, Is.EqualTo(1));
            Assert.That(completedFirst.State, Is.EqualTo(StageAddEncounterTicketState.Completed));
            Assert.That(completedFirst.TerminalSequence, Is.GreaterThan(0));
            Assert.That(survivingSecond.State, Is.EqualTo(StageAddEncounterTicketState.Active));
            Assert.That(survivingSecond.Health, Is.SameAs(secondAddHealth));
            Assert.That(secondAddHealth.IsAlive, Is.True);
            Assert.That(secondOwnedRoot.activeSelf, Is.True);
            Assert.That(ownedRoot.activeSelf, Is.False);
            Assert.That(playerTargetSelector.ContainsRuntimeTargetCandidate(addHealth), Is.False);
            Assert.That(playerTargetSelector.ContainsRuntimeTargetCandidate(secondAddHealth), Is.True);
            Assert.That(playerTargetSelector.RuntimeTargetCandidateCount, Is.EqualTo(1));
            Assert.That(playerTargetSelector.ContainsAuthoredTargetCandidate(bossHealth), Is.True);
            Assert.That(bossHealth.IsAlive, Is.True);

            Vector3 rangedLaneDirection = Vector3.ProjectOnPlane(
                playerHealth.transform.position - secondAddHealth.transform.position,
                Vector3.up).normalized;
            if (rangedLaneDirection.sqrMagnitude < 0.99f)
            {
                rangedLaneDirection = Vector3.forward;
            }

            MoveCombatSubjectToPosition(
                playerHealth,
                secondAddHealth.transform.position + rangedLaneDirection * 4.5f);
            yield return null;

            int exactRangedDamageCount = 0;
            DamageInfo exactRangedDamage = default;
            playerHealth.Damaged += HandleRangedPlayerDamaged;
            try
            {
                float rangedHitDeadline = Time.realtimeSinceStartup + 10f;
                while (exactRangedDamageCount == 0)
                {
                    Assert.That(executor.State, Is.EqualTo(StageCountOneEncounterState.Active));
                    Assert.Less(
                        Time.realtimeSinceStartup,
                        rangedHitDeadline,
                        "The admitted RifleCrossfire Add did not hit the terminal player with a physical projectile. "
                        + $"fired={rangedDriver.FiredCount}, active={rangedDriver.ActiveProjectileCount}, "
                        + $"distance={Vector3.ProjectOnPlane(secondAddHealth.transform.position - playerHealth.transform.position, Vector3.up).magnitude:F2}, "
                        + $"lastImpact={rangedDriver.LastFiredProjectile?.LastImpactResult}, "
                        + $"lastTarget={rangedDriver.LastFiredProjectile?.LastImpactTargetHealth?.name ?? "none"}, "
                        + $"projectile={rangedDriver.LastFiredProjectile?.transform.position.ToString("F2") ?? "none"}, "
                        + $"player={playerHealth.transform.position:F2}, source={secondAddHealth.transform.position:F2}.");
                    yield return null;
                }
            }
            finally
            {
                playerHealth.Damaged -= HandleRangedPlayerDamaged;
            }

            Assert.That(rangedDriver.FiredCount, Is.GreaterThan(0));
            Assert.That(rangedDriver.OwnedProjectileCount, Is.InRange(1, rangedDriver.MaxOwnedProjectileCount));
            Assert.That(rangedDriver.ActiveProjectileCount, Is.LessThanOrEqualTo(rangedDriver.OwnedProjectileCount));
            Assert.That(exactRangedDamageCount, Is.EqualTo(1));
            Assert.That(exactRangedDamage.Source, Is.SameAs(secondAddHealth));
            Assert.That(exactRangedDamage.SourceTeam, Is.EqualTo(DamageTeam.Enemy));
            Assert.That(
                exactRangedDamage.Amount,
                Is.EqualTo(secondAddAgent.PatternProfile.Damage).Within(0.001f));
            Assert.That(
                exactRangedDamage.HitStopSeconds,
                Is.EqualTo(secondAddAgent.PatternProfile.HitStopSeconds).Within(0.001f));
            Assert.That(
                exactRangedDamage.ResponsePolicy,
                Is.EqualTo(secondAddAgent.PatternProfile.DamageResponsePolicy));
            Assert.That(
                exactRangedDamage.ControlLockPolicy,
                Is.EqualTo(secondAddAgent.PatternProfile.ControlLockPolicy));

            Assert.That(ApplyLethalDamage(secondAddHealth, DamageTeam.Player), Is.True);

            Assert.That(executor.State, Is.EqualTo(StageCountOneEncounterState.Completed));
            Assert.That(executor.CompletionCount, Is.EqualTo(1));
            Assert.That(executor.CancellationCount, Is.Zero);
            Assert.That(executor.FaultCount, Is.Zero);
            Assert.That(executor.CompletedTicketCount, Is.EqualTo(2));
            Assert.That(executor.OwnedObjectCount, Is.Zero);
            Assert.That(executor.OwnedRoot, Is.Null);
            Assert.That(executor.OwnedHealth, Is.Null);
            Assert.That(executor.OwnedAgent, Is.Null);
            Assert.That(executor.OwnedSensor, Is.Null);
            Assert.That(executor.HasCombatantParticipation, Is.False);
            Assert.That(executor.HasSceneLease, Is.True);
            Assert.That(ownedRoot == null || !ownedRoot.activeSelf, Is.True);
            Assert.That(secondOwnedRoot == null || !secondOwnedRoot.activeSelf, Is.True);
            Assert.That(rangedDriver.ActiveProjectileCount, Is.Zero);
            Assert.That(playerTargetSelector.ContainsRuntimeTargetCandidate(addHealth), Is.False);
            Assert.That(playerTargetSelector.ContainsRuntimeTargetCandidate(secondAddHealth), Is.False);
            Assert.That(playerTargetSelector.RuntimeTargetCandidateCount, Is.Zero);
            Assert.That(playerTargetSelector.ContainsAuthoredTargetCandidate(bossHealth), Is.True);
            Assert.That(addSensor.TargetCandidateCount, Is.Zero);
            Assert.That(addSensor.CurrentTargetHealth, Is.Null);
            Assert.That(secondAddSensor.TargetCandidateCount, Is.Zero);
            Assert.That(secondAddSensor.CurrentTargetHealth, Is.Null);
            Assert.That(executor.LastReceipt, Is.Not.Null);
            Assert.That(executor.LastReceipt.FinalState, Is.EqualTo(StageCountOneEncounterState.Completed));
            Assert.That(executor.LastReceipt.TicketCount, Is.EqualTo(2));
            Assert.That(
                executor.LastReceipt.TryValidateIntegrity(out string receiptError),
                Is.True,
                receiptError);
            Assert.That(
                executor.LastReceipt.GetTicket(0).ActivationSequence,
                Is.LessThan(executor.LastReceipt.GetTicket(1).ActivationSequence));
            Assert.That(
                executor.LastReceipt.GetTicket(0).FinalState,
                Is.EqualTo(StageAddEncounterTicketState.Completed));
            Assert.That(
                executor.LastReceipt.GetTicket(1).FinalState,
                Is.EqualTo(StageAddEncounterTicketState.Completed));
            Assert.That(
                executor.LastReceipt.GetTicket(0).TerminalSequence,
                Is.LessThan(executor.LastReceipt.GetTicket(1).TerminalSequence));
            Assert.That(
                executor.LastReceipt.CloseSequence,
                Is.GreaterThan(executor.LastReceipt.GetTicket(1).TerminalSequence));

            StageAddEncounterPlanReceipt completedReceipt = executor.LastReceipt;
            Assert.That(ApplyLethalDamage(bossHealth, DamageTeam.Player), Is.True);
            Assert.That(executor.LastReceipt, Is.SameAs(completedReceipt));

            void HandlePlayerDamaged(DamageInfo damageInfo)
            {
                if (!ReferenceEquals(damageInfo.Source, addHealth))
                {
                    return;
                }

                exactAddDamage = damageInfo;
                exactAddDamageCount++;
            }

            void HandleAddPatternStateChanged(
                CombatAiPatternState state,
                CombatAiPatternProfile _)
            {
                observedWindup |= state == CombatAiPatternState.Windup;
            }

            void HandleRangedPlayerDamaged(DamageInfo damageInfo)
            {
                if (!ReferenceEquals(damageInfo.Source, secondAddHealth))
                {
                    return;
                }

                exactRangedDamage = damageInfo;
                exactRangedDamageCount++;
            }
        }

        [UnityTest]
        public IEnumerator CanonicalStationTerminalOutcomeCancelsBothLivingOrderedAdds()
        {
            StageRunRuntime.ResetForTests();
            yield return EnterCanonicalStation();

            Scene station = SceneManager.GetActiveScene();
            StageCountOneEncounterExecutor executor =
                RequireSingleSceneComponent<StageCountOneEncounterExecutor>(station);
            float activationDeadline = Time.realtimeSinceStartup + 2f;
            while (executor.State != StageCountOneEncounterState.Active)
            {
                Assert.Less(Time.realtimeSinceStartup, activationDeadline, executor.LastError);
                yield return null;
            }

            StageAddEncounterTicketSnapshot[] addTickets =
                RequireOrderedActiveAddTickets(executor);
            CombatHealth addHealth = addTickets[0].Health;
            ICombatAiAgent addAgent = addTickets[0].Agent;
            CombatTargetSensor addSensor = addTickets[0].Sensor;
            CombatHealth secondAddHealth = addTickets[1].Health;
            CombatTargetSensor secondAddSensor = addTickets[1].Sensor;
            BasicSoldierProjectileAttackDriver rangedDriver = addTickets[1].ProjectileDriver;
            PlayerCombatTargetSelector playerTargetSelector = executor.PlayerTargetSelector;
            GameObject ownedRoot = addTickets[0].Root;
            GameObject secondOwnedRoot = addTickets[1].Root;
            CombatEncounterController encounter =
                RequireSingleSceneComponent<CombatEncounterController>(station);
            CombatHealth playerHealth = encounter.PlayerHealth;
            CombatHealth bossHealth = encounter.EnemyHealth;
            Assert.That(addHealth, Is.Not.Null);
            Assert.That(addAgent, Is.Not.Null);
            Assert.That(addSensor, Is.Not.Null);
            Assert.That(rangedDriver, Is.Not.Null);
            Assert.That(playerTargetSelector, Is.Not.Null);
            Assert.That(ownedRoot, Is.Not.Null);
            Assert.That(bossHealth, Is.Not.SameAs(addHealth));
            Assert.That(playerTargetSelector.ContainsRuntimeTargetCandidate(addHealth), Is.True);
            Assert.That(playerTargetSelector.ContainsRuntimeTargetCandidate(secondAddHealth), Is.True);
            Assert.That(playerTargetSelector.ContainsAuthoredTargetCandidate(bossHealth), Is.True);

            Transform playerForwardBoundary = RequireSingleSceneTransform(
                station,
                "PlayerForwardBoundaryAnchor");
            MoveCombatSubjectToAnchor(playerHealth, playerForwardBoundary);
            yield return null;

            int exactAddDamageCount = 0;
            playerHealth.Damaged += HandlePlayerDamaged;
            float projectileDeadline = Time.realtimeSinceStartup + 18f;
            while (rangedDriver.ActiveProjectileCount == 0)
            {
                Assert.That(executor.State, Is.EqualTo(StageCountOneEncounterState.Active));
                Assert.That(addHealth.IsAlive, Is.True);
                Assert.Less(
                    Time.realtimeSinceStartup,
                    projectileDeadline,
                    "Timed out waiting to cancel the RifleCrossfire Add during physical projectile flight. "
                    + $"distance={Vector3.ProjectOnPlane(addHealth.transform.position - playerHealth.transform.position, Vector3.up).magnitude:F2}, "
                    + $"meleeState={addAgent.CurrentPatternState}, rangedFired={rangedDriver.FiredCount}, "
                    + $"sensorTarget={addSensor.CurrentTargetHealth?.name ?? "none"}, "
                    + $"timeScale={Time.timeScale:F2}.");
                yield return null;
            }

            int damageCountAtTerminal = exactAddDamageCount;
            Assert.That(ApplyLethalDamage(bossHealth, DamageTeam.Player), Is.True);

            Assert.That(executor.State, Is.EqualTo(StageCountOneEncounterState.Cancelled));
            Assert.That(executor.ActivationCount, Is.EqualTo(1));
            Assert.That(executor.ActivatedTicketCount, Is.EqualTo(2));
            Assert.That(executor.CompletionCount, Is.Zero);
            Assert.That(executor.CancellationCount, Is.EqualTo(1));
            Assert.That(executor.CancelledTicketCount, Is.EqualTo(2));
            Assert.That(executor.OwnedObjectCount, Is.Zero);
            Assert.That(executor.ActiveParticipationCount, Is.Zero);
            Assert.That(executor.IsQuiescent, Is.True);
            Assert.That(executor.HasCombatantParticipation, Is.False);
            Assert.That(ownedRoot.activeSelf, Is.False);
            Assert.That(secondOwnedRoot.activeSelf, Is.False);
            Assert.That(rangedDriver.ActiveProjectileCount, Is.Zero);
            Assert.That(playerTargetSelector.ContainsRuntimeTargetCandidate(addHealth), Is.False);
            Assert.That(playerTargetSelector.ContainsRuntimeTargetCandidate(secondAddHealth), Is.False);
            Assert.That(playerTargetSelector.RuntimeTargetCandidateCount, Is.Zero);
            Assert.That(playerTargetSelector.ContainsAuthoredTargetCandidate(bossHealth), Is.True);
            Assert.That(addSensor.TargetCandidateCount, Is.Zero);
            Assert.That(addSensor.CurrentTargetHealth, Is.Null);
            Assert.That(secondAddSensor.TargetCandidateCount, Is.Zero);
            Assert.That(secondAddSensor.CurrentTargetHealth, Is.Null);
            Assert.That(executor.LastReceipt, Is.Not.Null);
            Assert.That(executor.LastReceipt.FinalState, Is.EqualTo(StageCountOneEncounterState.Cancelled));
            Assert.That(
                executor.LastReceipt.TryValidateIntegrity(out string cancellationReceiptError),
                Is.True,
                cancellationReceiptError);

            yield return new WaitForSecondsRealtime(1.25f);
            playerHealth.Damaged -= HandlePlayerDamaged;
            Assert.That(
                exactAddDamageCount,
                Is.EqualTo(damageCountAtTerminal),
                "The cancelled Add applied delayed damage after the boss terminal boundary.");
            Assert.That(executor.State, Is.EqualTo(StageCountOneEncounterState.Cancelled));
            Assert.That(executor.CancellationCount, Is.EqualTo(1));

            void HandlePlayerDamaged(DamageInfo damageInfo)
            {
                if (ReferenceEquals(damageInfo.Source, addHealth)
                    || ReferenceEquals(damageInfo.Source, secondAddHealth))
                {
                    exactAddDamageCount++;
                }
            }
        }

        [UnityTest]
        public IEnumerator CanonicalStationExecutorDisableSynchronouslyCleansOrderedAddsWithoutRespawn()
        {
            StageRunRuntime.ResetForTests();
            yield return EnterCanonicalStation();

            Scene station = SceneManager.GetActiveScene();
            StageCountOneEncounterExecutor executor =
                RequireSingleSceneComponent<StageCountOneEncounterExecutor>(station);
            float activationDeadline = Time.realtimeSinceStartup + 2f;
            while (executor.State != StageCountOneEncounterState.Active)
            {
                Assert.Less(Time.realtimeSinceStartup, activationDeadline, executor.LastError);
                yield return null;
            }

            StageAddEncounterTicketSnapshot[] addTickets =
                RequireOrderedActiveAddTickets(executor);
            CombatHealth addHealth = addTickets[0].Health;
            CombatTargetSensor addSensor = addTickets[0].Sensor;
            CombatHealth secondAddHealth = addTickets[1].Health;
            CombatTargetSensor secondAddSensor = addTickets[1].Sensor;
            PlayerCombatTargetSelector playerTargetSelector = executor.PlayerTargetSelector;
            GameObject ownedRoot = addTickets[0].Root;
            GameObject secondOwnedRoot = addTickets[1].Root;
            CombatHealth bossHealth =
                RequireSingleSceneComponent<CombatEncounterController>(station).EnemyHealth;
            Assert.That(addHealth, Is.Not.Null);
            Assert.That(addSensor, Is.Not.Null);
            Assert.That(playerTargetSelector.ContainsRuntimeTargetCandidate(addHealth), Is.True);
            Assert.That(playerTargetSelector.ContainsRuntimeTargetCandidate(secondAddHealth), Is.True);
            Assert.That(playerTargetSelector.ContainsAuthoredTargetCandidate(bossHealth), Is.True);
            Assert.That(executor.HasSceneLease, Is.True);

            executor.enabled = false;

            Assert.That(executor.State, Is.EqualTo(StageCountOneEncounterState.Cancelled));
            Assert.That(executor.ActivationCount, Is.EqualTo(1));
            Assert.That(executor.ActivatedTicketCount, Is.EqualTo(2));
            Assert.That(executor.CompletionCount, Is.Zero);
            Assert.That(executor.CancellationCount, Is.EqualTo(1));
            Assert.That(executor.CancelledTicketCount, Is.EqualTo(2));
            Assert.That(executor.OwnedObjectCount, Is.Zero);
            Assert.That(executor.ActiveParticipationCount, Is.Zero);
            Assert.That(executor.IsQuiescent, Is.True);
            Assert.That(executor.HasCombatantParticipation, Is.False);
            Assert.That(executor.HasSceneLease, Is.False);
            Assert.That(ownedRoot.activeSelf, Is.False);
            Assert.That(secondOwnedRoot.activeSelf, Is.False);
            Assert.That(playerTargetSelector.ContainsRuntimeTargetCandidate(addHealth), Is.False);
            Assert.That(playerTargetSelector.ContainsRuntimeTargetCandidate(secondAddHealth), Is.False);
            Assert.That(playerTargetSelector.RuntimeTargetCandidateCount, Is.Zero);
            Assert.That(playerTargetSelector.ContainsAuthoredTargetCandidate(bossHealth), Is.True);
            Assert.That(addSensor.TargetCandidateCount, Is.Zero);
            Assert.That(addSensor.CurrentTargetHealth, Is.Null);
            Assert.That(secondAddSensor.TargetCandidateCount, Is.Zero);
            Assert.That(secondAddSensor.CurrentTargetHealth, Is.Null);
            Assert.That(executor.LastReceipt, Is.Not.Null);
            Assert.That(
                executor.LastReceipt.TryValidateIntegrity(out string disableReceiptError),
                Is.True,
                disableReceiptError);
            StageAddEncounterPlanReceipt disabledReceipt = executor.LastReceipt;

            executor.enabled = true;
            yield return null;
            yield return null;

            Assert.That(executor.State, Is.EqualTo(StageCountOneEncounterState.Cancelled));
            Assert.That(executor.ActivationCount, Is.EqualTo(1));
            Assert.That(executor.CancellationCount, Is.EqualTo(1));
            Assert.That(executor.OwnedObjectCount, Is.Zero);
            Assert.That(executor.HasCombatantParticipation, Is.False);
            Assert.That(executor.HasSceneLease, Is.False);
            Assert.That(executor.LastReceipt, Is.SameAs(disabledReceipt));
        }

        [UnityTest]
        public IEnumerator CanonicalStationExplicitRunLossSynchronouslyCancelsOrderedAdds()
        {
            StageRunRuntime.ResetForTests();
            yield return EnterCanonicalStation();

            Scene station = SceneManager.GetActiveScene();
            StageCountOneEncounterExecutor executor =
                RequireSingleSceneComponent<StageCountOneEncounterExecutor>(station);
            float activationDeadline = Time.realtimeSinceStartup + 2f;
            while (executor.State != StageCountOneEncounterState.Active)
            {
                Assert.Less(Time.realtimeSinceStartup, activationDeadline, executor.LastError);
                yield return null;
            }

            StageAddEncounterTicketSnapshot[] activeTickets =
                RequireOrderedActiveAddTickets(executor);
            StageRunRuntime.ResetForTests();
            Assert.That(executor.TryActivate(out string runLossError), Is.False);
            Assert.That(runLossError, Does.Contain("No canonical stage run"));
            Assert.That(executor.State, Is.EqualTo(StageCountOneEncounterState.Cancelled));
            Assert.That(executor.CancellationCount, Is.EqualTo(1));
            Assert.That(executor.CancelledTicketCount, Is.EqualTo(2));
            Assert.That(executor.OwnedObjectCount, Is.Zero);
            Assert.That(executor.ActiveParticipationCount, Is.Zero);
            Assert.That(executor.PlayerTargetSelector.RuntimeTargetCandidateCount, Is.Zero);
            Assert.That(executor.IsQuiescent, Is.True);
            AssertCapturedAddRootsInactive(activeTickets);
            Assert.That(executor.LastReceipt, Is.Not.Null);
            Assert.That(
                executor.LastReceipt.TryValidateIntegrity(out string runLossReceiptError),
                Is.True,
                runLossReceiptError);
        }

        [UnityTest]
        public IEnumerator CanonicalStationLaterTicketSensorLeaseLossFaultsAndCleansWholePlan()
        {
            StageRunRuntime.ResetForTests();
            yield return EnterCanonicalStation();

            Scene station = SceneManager.GetActiveScene();
            StageCountOneEncounterExecutor executor =
                RequireSingleSceneComponent<StageCountOneEncounterExecutor>(station);
            float activationDeadline = Time.realtimeSinceStartup + 2f;
            while (executor.State != StageCountOneEncounterState.Active)
            {
                Assert.Less(Time.realtimeSinceStartup, activationDeadline, executor.LastError);
                yield return null;
            }

            StageAddEncounterTicketSnapshot[] addTickets =
                RequireOrderedActiveAddTickets(executor);
            CombatHealth addHealth = addTickets[0].Health;
            CombatTargetSensor addSensor = addTickets[0].Sensor;
            CombatHealth secondAddHealth = addTickets[1].Health;
            CombatTargetSensor secondAddSensor = addTickets[1].Sensor;
            PlayerCombatTargetSelector playerTargetSelector = executor.PlayerTargetSelector;
            GameObject ownedRoot = addTickets[0].Root;
            GameObject secondOwnedRoot = addTickets[1].Root;
            CombatHealth bossHealth =
                RequireSingleSceneComponent<CombatEncounterController>(station).EnemyHealth;
            Assert.That(addHealth, Is.Not.Null);
            Assert.That(addSensor, Is.Not.Null);
            Assert.That(playerTargetSelector.ContainsRuntimeTargetCandidate(addHealth), Is.True);
            Assert.That(playerTargetSelector.ContainsRuntimeTargetCandidate(secondAddHealth), Is.True);

            secondAddSensor.enabled = false;
            yield return null;

            Assert.That(executor.State, Is.EqualTo(StageCountOneEncounterState.Faulted));
            Assert.That(executor.LastError, Does.Contain("bidirectional combatant participation lease"));
            Assert.That(executor.ActivationCount, Is.EqualTo(1));
            Assert.That(executor.ActivatedTicketCount, Is.EqualTo(2));
            Assert.That(executor.CompletionCount, Is.Zero);
            Assert.That(executor.CancellationCount, Is.Zero);
            Assert.That(executor.FaultCount, Is.EqualTo(1));
            Assert.That(executor.CancelledTicketCount, Is.EqualTo(1));
            Assert.That(executor.FaultedTicketCount, Is.EqualTo(1));
            Assert.That(executor.OwnedObjectCount, Is.Zero);
            Assert.That(executor.OwnedRoot, Is.Null);
            Assert.That(executor.OwnedHealth, Is.Null);
            Assert.That(executor.OwnedAgent, Is.Null);
            Assert.That(executor.OwnedSensor, Is.Null);
            Assert.That(executor.HasCombatantParticipation, Is.False);
            Assert.That(executor.IsQuiescent, Is.True);
            Assert.That(executor.HasSceneLease, Is.True);
            Assert.That(
                ownedRoot == null || !ownedRoot.activeSelf,
                Is.True,
                "Fault cleanup must destroy or deactivate the owned Add root.");
            Assert.That(
                secondOwnedRoot == null || !secondOwnedRoot.activeSelf,
                Is.True,
                "Fault cleanup must destroy or deactivate the later Add root.");
            Assert.That(playerTargetSelector.ContainsRuntimeTargetCandidate(addHealth), Is.False);
            Assert.That(playerTargetSelector.ContainsRuntimeTargetCandidate(secondAddHealth), Is.False);
            Assert.That(playerTargetSelector.RuntimeTargetCandidateCount, Is.Zero);
            Assert.That(playerTargetSelector.ContainsAuthoredTargetCandidate(bossHealth), Is.True);
            Assert.That(addSensor.TargetCandidateCount, Is.Zero);
            Assert.That(addSensor.CurrentTargetHealth, Is.Null);
            Assert.That(secondAddSensor.TargetCandidateCount, Is.Zero);
            Assert.That(secondAddSensor.CurrentTargetHealth, Is.Null);
            Assert.That(
                executor.GetTicketSnapshot(0).State,
                Is.EqualTo(StageAddEncounterTicketState.Cancelled));
            Assert.That(
                executor.GetTicketSnapshot(1).State,
                Is.EqualTo(StageAddEncounterTicketState.Faulted));
            Assert.That(executor.LastReceipt, Is.Not.Null);
            Assert.That(executor.LastReceipt.FinalState, Is.EqualTo(StageCountOneEncounterState.Faulted));
            Assert.That(
                executor.LastReceipt.TryValidateIntegrity(out string faultReceiptError),
                Is.True,
                faultReceiptError);

            yield return null;
            yield return null;
            Assert.That(executor.State, Is.EqualTo(StageCountOneEncounterState.Faulted));
            Assert.That(executor.ActivationCount, Is.EqualTo(1));
            Assert.That(executor.OwnedObjectCount, Is.Zero);
        }

        private static StageAddEncounterTicketSnapshot[] RequireOrderedActiveAddTickets(
            StageCountOneEncounterExecutor executor)
        {
            Assert.That(executor, Is.Not.Null);
            Assert.That(executor.TicketCount, Is.EqualTo(2));
            Assert.That(executor.PendingTicketCount, Is.Zero);
            Assert.That(executor.ActiveTicketCount, Is.EqualTo(2));
            Assert.That(executor.ActivatedTicketCount, Is.EqualTo(2));
            Assert.That(executor.ActiveParticipationCount, Is.EqualTo(2));
            Assert.That(executor.OwnedObjectCount, Is.EqualTo(2));
            var snapshots = new StageAddEncounterTicketSnapshot[2];
            for (int i = 0; i < snapshots.Length; i++)
            {
                snapshots[i] = executor.GetTicketSnapshot(i);
                Assert.That(snapshots[i].SourceOrdinal, Is.EqualTo(i));
                Assert.That(snapshots[i].State, Is.EqualTo(StageAddEncounterTicketState.Active));
                Assert.That(snapshots[i].DelaySeconds, Is.Zero);
                Assert.That(snapshots[i].Root, Is.Not.Null);
                Assert.That(snapshots[i].Root.activeInHierarchy, Is.True);
                Assert.That(snapshots[i].Health, Is.Not.Null);
                Assert.That(snapshots[i].Agent, Is.Not.Null);
                Assert.That(snapshots[i].Sensor, Is.Not.Null);
                Assert.That(snapshots[i].ParticipationRegistered, Is.True);
            }

            Assert.That(snapshots[0].SpawnId, Is.EqualTo("add-left"));
            Assert.That(snapshots[0].AnchorId, Is.EqualTo("Add_LeftLaneAnchor"));
            Assert.That(snapshots[0].PositionId, Is.EqualTo(2101));
            Assert.That(snapshots[1].SpawnId, Is.EqualTo("add-right"));
            Assert.That(snapshots[1].AnchorId, Is.EqualTo("Add_RightLaneAnchor"));
            Assert.That(snapshots[1].PositionId, Is.EqualTo(2102));
            Assert.That(
                snapshots[1].ActivationSequence,
                Is.EqualTo(snapshots[0].ActivationSequence + 1),
                "Equal-delay Add tickets must activate in serialized source order.");
            Assert.That(snapshots[0].Health, Is.Not.SameAs(snapshots[1].Health));
            Assert.That(snapshots[0].Root, Is.Not.SameAs(snapshots[1].Root));
            return snapshots;
        }

        private static void AssertCapturedAddRootsInactive(
            StageAddEncounterTicketSnapshot[] snapshots)
        {
            Assert.That(snapshots, Is.Not.Null);
            for (int i = 0; i < snapshots.Length; i++)
            {
                Assert.That(
                    snapshots[i].Root == null || !snapshots[i].Root.activeSelf,
                    Is.True,
                    $"Ordered Add ticket {i} retained an active hierarchy after cleanup.");
            }
        }

        private static IEnumerator EnterCanonicalStation(bool releaseEntryGuide = true)
        {
            yield return LoadSingleScene(CorridorScenePath);
            StageRunContext context = StageRunRuntime.ActiveContext;
            Assert.That(context, Is.Not.Null);
            Scene station = SceneManager.GetActiveScene();
            int corridorHandle = station.handle;
            StageDefinitionSceneBinding stationBinding = RequireStationSceneBinding(station);
            OlympusCorridorCombatFlowController flow =
                RequireSingleSceneComponent<OlympusCorridorCombatFlowController>(station);
            flow.SkipIntroCutscene();
            yield return null;
            yield return null;
            Assert.That(flow.CanonicalStageRunId, Is.EqualTo(context.Identity.RunId));
            Assert.That(
                context.TrySealTutorialRouteCompletion(out string tutorialFactError),
                Is.True,
                tutorialFactError);
            InvokePrivate(flow, "BeginWaitingForStairEntry");
            InvokePrivate(flow, "BeginCorridorCombat");

            StageSegmentEntryReceipt entryReceipt = context.SegmentEntryReceipt;
            Assert.That(entryReceipt, Is.Not.Null);
            Assert.That(entryReceipt.FromHandoffPending, Is.False);
            Assert.That(entryReceipt.RequestedScenePath, Is.EqualTo(CorridorScenePath));
            Assert.That(entryReceipt.ActualScenePath, Is.EqualTo(CorridorScenePath));
            Assert.IsTrue(
                SceneManager.GetActiveScene().handle == corridorHandle,
                $"In-scene Station activation must retain Corridor scene handle {corridorHandle}; "
                + $"actual={SceneManager.GetActiveScene().handle}.");

            AssertCanonicalStationRuntimeWiring(station, stationBinding);
            yield return null;
            OlympusStationCombatResultPresenter presenter =
                RequireSingleSceneComponent<OlympusStationCombatResultPresenter>(station);
            Assert.That(presenter.HasCanonicalStageRun, Is.True, presenter.CanonicalStageRunEntryError);
            Assert.That(context.LifecycleState, Is.EqualTo(StageRunLifecycleState.StationActive));
            if (!releaseEntryGuide)
            {
                Time.timeScale = 1f;
                yield break;
            }

            yield return ReleaseStationEntryGuide(station);

            CombatEncounterController encounter =
                RequireSingleSceneComponent<CombatEncounterController>(station);
            CombatHealth playerHealth = ReadPrivateField<CombatHealth>(encounter, "playerHealth");
            float invulnerabilityDeadline = Time.realtimeSinceStartup + 2.25f;
            while (playerHealth.IsInvulnerable)
            {
                Assert.Less(
                    Time.realtimeSinceStartup,
                    invulnerabilityDeadline,
                    "Tutorial invulnerability did not expire during the physical stair-entry interval.");
                yield return null;
            }

            Time.timeScale = 1f;
        }

        private static StageDefinitionSceneBinding RequireStationSceneBinding(Scene scene)
        {
            StageDefinitionSceneBinding found = null;
            int stationMatchCount = 0;
            GameObject[] roots = scene.GetRootGameObjects();
            for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
            {
                StageDefinitionSceneBinding[] bindings =
                    roots[rootIndex].GetComponentsInChildren<StageDefinitionSceneBinding>(true);
                for (int bindingIndex = 0; bindingIndex < bindings.Length; bindingIndex++)
                {
                    StageDefinitionSceneBinding candidate = bindings[bindingIndex];
                    if (candidate.StageDefinition != null
                        && string.Equals(
                            candidate.StageDefinition.StageId,
                            StationStageId,
                            StringComparison.Ordinal))
                    {
                        found = candidate;
                        stationMatchCount++;
                    }
                }
            }

            Assert.That(
                stationMatchCount,
                Is.EqualTo(1),
                $"Scene {scene.path} must contain one exact {StationStageId} binding.");
            Assert.That(found, Is.Not.Null);
            Assert.That(found.gameObject.scene.handle, Is.EqualTo(scene.handle));
            Assert.That(found.StageDefinition.MapScenePath, Is.EqualTo(CorridorScenePath));
            return found;
        }

        private static void AssertCanonicalStationRuntimeWiring(
            Scene scene,
            StageDefinitionSceneBinding stationBinding)
        {
            StageCountOneEncounterExecutor executor =
                RequireSingleSceneComponent<StageCountOneEncounterExecutor>(scene);
            CombatEncounterController encounter =
                RequireSingleSceneComponent<CombatEncounterController>(scene);
            OlympusStationCombatResultPresenter presenter =
                RequireSingleSceneComponent<OlympusStationCombatResultPresenter>(scene);
            OlympusStationRunFactCollector collector =
                RequireSingleSceneComponent<OlympusStationRunFactCollector>(scene);
            ICombatEntryGuideGate guide = RequireSingleSceneInterface<ICombatEntryGuideGate>(scene);
            Component guideComponent = guide as Component;

            Assert.That(executor.SceneBinding, Is.SameAs(stationBinding));
            Assert.That(encounter.gameObject.scene.handle, Is.EqualTo(scene.handle));
            Assert.That(presenter.gameObject.scene.handle, Is.EqualTo(scene.handle));
            Assert.That(collector.gameObject.scene.handle, Is.EqualTo(scene.handle));
            Assert.That(
                ReadPrivateField<CombatEncounterController>(presenter, "encounter"),
                Is.SameAs(encounter));
            Assert.That(
                ReadPrivateField<OlympusStationRunFactCollector>(presenter, "factCollector"),
                Is.SameAs(collector));
            Assert.That(
                ReadPrivateField<CombatEncounterController>(collector, "encounter"),
                Is.SameAs(encounter));
            Assert.That(guideComponent, Is.Not.Null);
            Assert.That(guideComponent.gameObject.scene.handle, Is.EqualTo(scene.handle));
            Assert.That(encounter.isActiveAndEnabled, Is.True);
            Assert.That(presenter.isActiveAndEnabled, Is.True);
            Assert.That(collector.isActiveAndEnabled, Is.True);
            Assert.That(guideComponent.gameObject.activeInHierarchy, Is.True);
        }

        private static IEnumerator VerifyTransientCommitRecovery(
            int writeFailureCount,
            int readFailureCount,
            bool decisionExistsAfterInitialAttempt)
        {
            StageRunRuntime.ResetForTests();
            yield return EnterCanonicalStation();

            StageRunContext context = StageRunRuntime.ActiveContext;
            StageRunResultProgressionJoinSnapshot admissionJoin =
                context.ResultProgressionJoinSnapshot;
            string admissionJoinDigest = admissionJoin.CanonicalDigest;
            Scene station = SceneManager.GetActiveScene();
            CombatEncounterController encounter =
                RequireSingleSceneComponent<CombatEncounterController>(station);
            CombatHealth enemyHealth = ReadPrivateField<CombatHealth>(encounter, "enemyHealth");
            OlympusStationCombatResultPresenter presenter =
                RequireSingleSceneComponent<OlympusStationCombatResultPresenter>(station);
            string decisionPath =
                StageRunRuntime.GetResultCommitDecisionPathForTests(context.Identity.RunId);
            StageRunRuntime.InjectTransientResultDecisionIoFailuresForTests(
                writeFailureCount,
                readFailureCount);

            Time.timeScale = 1f;
            Assert.That(ApplyLethalDamage(enemyHealth, DamageTeam.Player), Is.True);

            Assert.That(context.LifecycleState, Is.EqualTo(StageRunLifecycleState.CommitRecoveryPending));
            Assert.That(context.CommittedSummary, Is.Null);
            Assert.That(presenter.CommittedSummary, Is.Null);
            Assert.That(SceneManager.GetSceneByName("UI_StageClear").isLoaded, Is.False);
            Assert.That(
                System.IO.File.Exists(decisionPath),
                Is.EqualTo(decisionExistsAfterInitialAttempt));
            StageRunRuntime.ClearResultCommitMemoryCacheForTests();

            float deadline = Time.realtimeSinceStartup + 4f;
            while (presenter.CommittedSummary == null
                || context.LifecycleState != StageRunLifecycleState.Presented
                || !SceneManager.GetSceneByName("UI_StageClear").isLoaded)
            {
                Assert.Less(
                    Time.realtimeSinceStartup,
                    deadline,
                    "Bounded result-commit recovery did not reconcile and present the exact candidate.");
                yield return null;
            }

            Assert.That(presenter.CommitRecoveryAttemptCount, Is.EqualTo(1));
            Assert.That(context.CommittedSummary, Is.SameAs(presenter.CommittedSummary));
            Assert.That(context.CommitReceipt, Is.SameAs(presenter.CommitReceipt));
            Assert.That(context.ResultProgressionJoinSnapshot, Is.SameAs(admissionJoin));
            Assert.That(
                context.ResultProgressionJoinSnapshot.CanonicalDigest,
                Is.EqualTo(admissionJoinDigest));
            Assert.That(
                context.ResultProgressionJoinSnapshot.TryValidateIntegrity(out string joinError),
                Is.True,
                joinError);
            Assert.That(context.CommitReceipt.SummaryCommittedAtSequence, Is.EqualTo(1));
            Assert.That(System.IO.File.Exists(decisionPath), Is.True);
            Assert.That(context.AbortRecord, Is.Null);
        }

        private static void AssertCurrentSchemaClosureRows(StageRunAbortRecord abortRecord)
        {
            Assert.That(abortRecord.HasValidIntegrity(), Is.True);
            Assert.That(abortRecord.ClosureBarrierCount, Is.EqualTo(5));
            Assert.That(abortRecord.PendingClosureOwnerCount, Is.Zero);
            Assert.That(abortRecord.AggregateClosureDigest, Has.Length.EqualTo(64));
            for (int i = 0; i < abortRecord.ClosureBarrierCount; i++)
            {
                StageRunClosureBarrierCoverageRow row = abortRecord.GetClosureBarrier(i);
                Assert.That((int)row.OwnerKind, Is.EqualTo(i + 1));
                Assert.That(row.Disposition, Is.EqualTo(StageRunClosureDisposition.NotAdmitted));
                Assert.That(row.ReceiptId, Is.Empty);
                Assert.That(row.FaultEvidenceId, Is.Empty);
            }
        }

        private static IEnumerator ReleaseStationEntryGuide(Scene station)
        {
            ICombatEntryGuideGate gate = RequireSingleSceneInterface<ICombatEntryGuideGate>(station);
            Assert.That(
                gate.State,
                Is.Not.EqualTo(CombatEntryGuideState.Released),
                "The initial non-playing state must not be mistaken for an already released guide.");
            bool observedPlaying = gate.State == CombatEntryGuideState.Playing;
            float deadline = Time.realtimeSinceStartup + 8f;
            while (gate.State != CombatEntryGuideState.Released)
            {
                observedPlaying |= gate.State == CombatEntryGuideState.Playing;
                Assert.That(
                    gate.State,
                    Is.Not.EqualTo(CombatEntryGuideState.Interrupted),
                    "Station entry guide was interrupted before gameplay release.");
                Assert.Less(
                    Time.realtimeSinceStartup,
                    deadline,
                    "Timed out waiting for the Station entry guide to release gameplay.");
                if (gate.IsAwaitingAdvance)
                {
                    gate.RequestAdvance();
                }

                yield return null;
            }

            Assert.That(observedPlaying, Is.True, "Station guide never published its explicit Playing state.");
        }

        private static bool ApplyLethalDamage(CombatHealth health, DamageTeam sourceTeam)
        {
            return health.TryApplyDamage(new DamageInfo(
                null,
                sourceTeam,
                health.MaxHealth + 1f,
                health.transform.position,
                Vector3.forward,
                0f,
                DamageResponsePolicy.DamageOnly,
                CombatControlLockPolicy.None));
        }

        private static void InvokePrivate(object target, string methodName, params object[] arguments)
        {
            MethodInfo method = target.GetType().GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, $"Missing private source seam {target.GetType().Name}.{methodName}.");
            method.Invoke(target, arguments);
        }

        private static EncounterTerminalDiagnostic CreateDiagnosticForTest(
            EncounterTerminalDiagnosticReason reason,
            long runGeneration,
            long rootAdmissionSequence,
            long epoch,
            string message)
        {
            ConstructorInfo constructor = typeof(EncounterTerminalDiagnostic).GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new[]
                {
                    typeof(EncounterTerminalDiagnosticReason),
                    typeof(long),
                    typeof(long),
                    typeof(long),
                    typeof(string)
                },
                null);
            Assert.That(constructor, Is.Not.Null, "Missing EncounterTerminalDiagnostic test constructor.");
            return (EncounterTerminalDiagnostic)constructor.Invoke(new object[]
            {
                reason,
                runGeneration,
                rootAdmissionSequence,
                epoch,
                message
            });
        }

        private static IEnumerator LoadSingleScene(string path)
        {
            Time.timeScale = 1f;
            EditorSceneManager.LoadSceneInPlayMode(path, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            yield return null;
        }

        private static T RequireSingleSceneComponent<T>(Scene scene)
            where T : Component
        {
            T found = null;
            GameObject[] roots = scene.GetRootGameObjects();
            for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
            {
                T[] components = roots[rootIndex].GetComponentsInChildren<T>(true);
                for (int componentIndex = 0; componentIndex < components.Length; componentIndex++)
                {
                    Assert.That(found, Is.Null, $"Scene {scene.path} contains duplicate {typeof(T).Name} components.");
                    found = components[componentIndex];
                }
            }

            Assert.That(found, Is.Not.Null, $"Scene {scene.path} is missing {typeof(T).Name}.");
            return found;
        }

        private static T FindSingleSceneComponentOrNull<T>(Scene scene)
            where T : Component
        {
            T found = null;
            GameObject[] roots = scene.GetRootGameObjects();
            for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
            {
                T[] components = roots[rootIndex].GetComponentsInChildren<T>(true);
                for (int componentIndex = 0; componentIndex < components.Length; componentIndex++)
                {
                    Assert.That(
                        found,
                        Is.Null,
                        $"Scene {scene.path} contains duplicate {typeof(T).Name} components.");
                    found = components[componentIndex];
                }
            }

            return found;
        }

        private static T RequireSingleSceneInterface<T>(Scene scene)
            where T : class
        {
            T found = null;
            GameObject[] roots = scene.GetRootGameObjects();
            for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
            {
                MonoBehaviour[] behaviours = roots[rootIndex].GetComponentsInChildren<MonoBehaviour>(true);
                for (int behaviourIndex = 0; behaviourIndex < behaviours.Length; behaviourIndex++)
                {
                    if (behaviours[behaviourIndex] is not T candidate)
                    {
                        continue;
                    }

                    Assert.That(found, Is.Null, $"Scene {scene.path} contains duplicate {typeof(T).Name} implementations.");
                    found = candidate;
                }
            }

            Assert.That(found, Is.Not.Null, $"Scene {scene.path} is missing {typeof(T).Name}.");
            return found;
        }

        private static Transform RequireSingleSceneTransform(Scene scene, string objectName)
        {
            Transform found = null;
            GameObject[] roots = scene.GetRootGameObjects();
            for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
            {
                Transform[] transforms = roots[rootIndex].GetComponentsInChildren<Transform>(true);
                for (int transformIndex = 0; transformIndex < transforms.Length; transformIndex++)
                {
                    Transform candidate = transforms[transformIndex];
                    if (!string.Equals(candidate.name, objectName, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    Assert.That(
                        found,
                        Is.Null,
                        $"Scene {scene.path} contains duplicate transform name {objectName}.");
                    found = candidate;
                }
            }

            Assert.That(found, Is.Not.Null, $"Scene {scene.path} is missing transform {objectName}.");
            return found;
        }

        private static void MoveCombatSubjectToAnchor(CombatHealth health, Transform anchor)
        {
            Assert.That(health, Is.Not.Null);
            Assert.That(anchor, Is.Not.Null);
            Vector3 destination = anchor.position;
            destination.y = health.transform.position.y;
            MoveCombatSubjectToPosition(health, destination);
        }

        private static void MoveCombatSubjectToPosition(CombatHealth health, Vector3 destination)
        {
            Assert.That(health, Is.Not.Null);
            CharacterController controller = health.GetComponent<CharacterController>();
            bool restoreController = controller != null && controller.enabled;
            if (restoreController)
            {
                controller.enabled = false;
            }

            destination.y = health.transform.position.y;
            health.transform.position = destination;
            if (restoreController)
            {
                controller.enabled = true;
            }

            Physics.SyncTransforms();
        }

        private sealed class OneRowCombatFixture
        {
            public OneRowCombatFixture(
                GameObject root,
                CombatEncounterController encounter,
                CombatHealth playerHealth,
                CombatHealth enemyHealth)
            {
                Root = root;
                Encounter = encounter;
                PlayerHealth = playerHealth;
                EnemyHealth = enemyHealth;
            }

            public GameObject Root { get; }
            public CombatEncounterController Encounter { get; }
            public CombatHealth PlayerHealth { get; }
            public CombatHealth EnemyHealth { get; }

            public void Destroy()
            {
                if (Root != null)
                {
                    UnityEngine.Object.DestroyImmediate(Root);
                }
            }
        }

        private sealed class OneRowAdapterFixture
        {
            public OneRowAdapterFixture(
                GameObject root,
                CombatEncounterController encounter,
                CombatHealth playerHealth,
                CombatHealth enemyHealth,
                OneRowStageRunBootstrap bootstrap,
                OneRowStageRunFactAdapter factAdapter,
                OneRowStageRunResultPresenter presenter,
                RecordingStageRunResultOverlay resultOverlay,
                RecordingCombatSessionOverlay resultSurface)
            {
                Root = root;
                Encounter = encounter;
                PlayerHealth = playerHealth;
                EnemyHealth = enemyHealth;
                Bootstrap = bootstrap;
                FactAdapter = factAdapter;
                Presenter = presenter;
                ResultOverlay = resultOverlay;
                ResultSurface = resultSurface;
            }

            public GameObject Root { get; }
            public CombatEncounterController Encounter { get; }
            public CombatHealth PlayerHealth { get; }
            public CombatHealth EnemyHealth { get; }
            public OneRowStageRunBootstrap Bootstrap { get; }
            public OneRowStageRunFactAdapter FactAdapter { get; }
            public OneRowStageRunResultPresenter Presenter { get; }
            public RecordingStageRunResultOverlay ResultOverlay { get; }
            public RecordingCombatSessionOverlay ResultSurface { get; }

            public void Destroy()
            {
                if (Root != null)
                {
                    UnityEngine.Object.DestroyImmediate(Root);
                }
            }
        }

        private sealed class RecordingStageRunResultOverlay : MonoBehaviour, IStageRunResultOverlay
        {
            private string pendingResultDigest = string.Empty;
            private string presentedResultDigest = string.Empty;

            public int ShowCount { get; private set; }
            public int FailuresRemaining { get; set; }
            public bool AcceptPendingWithoutAcknowledgement { get; set; }
            public StageRunResultSummary Summary { get; private set; }
            public string PendingResultDigest => pendingResultDigest;
            public string PresentedResultDigest => presentedResultDigest;

            public event Action<StageRunResultSummary> PresentationSucceeded;
            public event Action<StageRunResultSummary, string> PresentationFailed;

            public bool TryShow(StageRunResultSummary summary, out string error)
            {
                error = string.Empty;
                ShowCount++;
                Summary = summary;
                if (summary == null)
                {
                    error = "A committed stage-run summary is required.";
                    PresentationFailed?.Invoke(summary, error);
                    return false;
                }

                pendingResultDigest = summary.ResultSummaryDigest;
                if (FailuresRemaining > 0)
                {
                    FailuresRemaining--;
                    pendingResultDigest = string.Empty;
                    error = "Injected result-presentation failure.";
                    PresentationFailed?.Invoke(summary, error);
                    return false;
                }

                if (AcceptPendingWithoutAcknowledgement)
                {
                    return true;
                }

                StageRunContext context = StageRunRuntime.ActiveContext;
                if (context == null)
                {
                    pendingResultDigest = string.Empty;
                    error = "No active canonical stage run exists for result presentation.";
                    PresentationFailed?.Invoke(summary, error);
                    return false;
                }

                if (!StageRunRuntime.TryPrepareResultPresentation(
                        summary,
                        context.ResultProgressionJoinSnapshot,
                        "ko-KR",
                        out StageResultPresentationSnapshot presentation,
                        out StageResultPresentationAuditEnvelope audit,
                        out error)
                    || !StageRunRuntime.TryMarkResultPresented(
                        summary,
                        presentation,
                        audit,
                        out error))
                {
                    pendingResultDigest = string.Empty;
                    PresentationFailed?.Invoke(summary, error);
                    return false;
                }

                pendingResultDigest = string.Empty;
                presentedResultDigest = summary.ResultSummaryDigest;
                PresentationSucceeded?.Invoke(summary);
                return true;
            }

            public void ReleasePendingWithoutCallback()
            {
                pendingResultDigest = string.Empty;
                AcceptPendingWithoutAcknowledgement = false;
            }

            public void EmitSuccessWithoutAcknowledgement()
            {
                PresentationSucceeded?.Invoke(Summary);
            }
        }

        private sealed class RecordingCombatSessionOverlay : MonoBehaviour, ICombatSessionOverlay
        {
            public CombatSessionOverlayMode Mode { get; private set; }
            public bool IsVisible => Mode != CombatSessionOverlayMode.Hidden;
            public int DismissCount { get; private set; }

            public event Action<bool> CombatInputBlockChanged;

            public void Configure(
                BossBarrageEncounterController resultSource,
                DimensionBrawl.Presentation.ActionScreenCuePresenter screenCuePresenter)
            {
            }

            public void ShowPause()
            {
                Mode = CombatSessionOverlayMode.Pause;
                CombatInputBlockChanged?.Invoke(true);
            }

            public void ShowSettings()
            {
                Mode = CombatSessionOverlayMode.Settings;
                CombatInputBlockChanged?.Invoke(true);
            }

            public void ShowFailure()
            {
                Mode = CombatSessionOverlayMode.Failure;
                CombatInputBlockChanged?.Invoke(true);
            }

            public void Resume()
            {
                Mode = CombatSessionOverlayMode.Hidden;
                CombatInputBlockChanged?.Invoke(false);
            }

            public void DismissForStageClear()
            {
                DismissCount++;
                Mode = CombatSessionOverlayMode.Hidden;
                CombatInputBlockChanged?.Invoke(true);
            }
        }

        private sealed class RecordingOneRowSceneLoader : IStageRunSceneLoader
        {
            public StageRunSceneLoadCompletionMode CompletionMode =>
                StageRunSceneLoadCompletionMode.RequestAccepted;
            public int CallCount { get; private set; }
            public string SceneName { get; private set; } = string.Empty;
            public string ScenePath { get; private set; } = string.Empty;

            public bool TryLoadSingle(string sceneName, string scenePath, out string error)
            {
                CallCount++;
                SceneName = sceneName;
                ScenePath = scenePath;
                error = string.Empty;
                return true;
            }
        }

        private sealed class FixedOneRowUiRouteResolver : IStageRunUiRouteResolver
        {
            private readonly StageRunUiRouteTarget target;

            public FixedOneRowUiRouteResolver(
                StageUiRouteId routeId,
                string sceneName,
                string scenePath)
            {
                target = new StageRunUiRouteTarget(routeId, sceneName, scenePath);
            }

            public bool TryResolve(
                StageUiRouteId routeId,
                out StageRunUiRouteTarget resolvedTarget,
                out string error)
            {
                resolvedTarget = routeId == target.RouteId ? target : null;
                error = resolvedTarget == null ? "Unexpected UI route." : string.Empty;
                return resolvedTarget != null;
            }
        }

        private static OneRowCombatFixture CreateOneRowCombatFixture(
            Scene scene,
            string suffix)
        {
            var root = new GameObject($"B0_2_OneRowEncounter_{suffix}");
            root.SetActive(false);
            SceneManager.MoveGameObjectToScene(root, scene);

            var playerObject = new GameObject("PlayerHealth");
            playerObject.transform.SetParent(root.transform, false);
            CombatHealth playerHealth = playerObject.AddComponent<CombatHealth>();
            playerHealth.ConfigureTeam(DamageTeam.Player);
            playerHealth.ConfigureMaxHealth(100f);

            var enemyObject = new GameObject("EnemyHealth");
            enemyObject.transform.SetParent(root.transform, false);
            CombatHealth enemyHealth = enemyObject.AddComponent<CombatHealth>();
            enemyHealth.ConfigureTeam(DamageTeam.Enemy);
            enemyHealth.ConfigureMaxHealth(100f);

            CombatEncounterController encounter = root.AddComponent<CombatEncounterController>();
            encounter.ConfigureCombatants(playerHealth, enemyHealth);
            encounter.ConfigureTerminalResolutionPolicy(true);
            root.SetActive(true);
            return new OneRowCombatFixture(root, encounter, playerHealth, enemyHealth);
        }

        private static OneRowAdapterFixture CreateOneRowAdapterFixture(
            Scene scene,
            PlayableStageDefinition route,
            string suffix,
            Action<OneRowAdapterFixture> beforeActivate = null)
        {
            var root = new GameObject($"B0_3_OneRowAdapters_{suffix}");
            root.SetActive(false);
            SceneManager.MoveGameObjectToScene(root, scene);

            var playerObject = new GameObject("PlayerHealth");
            playerObject.transform.SetParent(root.transform, false);
            CombatHealth playerHealth = playerObject.AddComponent<CombatHealth>();
            playerHealth.ConfigureTeam(DamageTeam.Player);
            playerHealth.ConfigureMaxHealth(100f);

            var enemyObject = new GameObject("EnemyHealth");
            enemyObject.transform.SetParent(root.transform, false);
            CombatHealth enemyHealth = enemyObject.AddComponent<CombatHealth>();
            enemyHealth.ConfigureTeam(DamageTeam.Enemy);
            enemyHealth.ConfigureMaxHealth(100f);

            CombatEncounterController encounter = root.AddComponent<CombatEncounterController>();
            encounter.ConfigureCombatants(playerHealth, enemyHealth);
            encounter.ConfigureTerminalResolutionPolicy(true);

            RecordingStageRunResultOverlay resultOverlay =
                root.AddComponent<RecordingStageRunResultOverlay>();
            RecordingCombatSessionOverlay resultSurface =
                root.AddComponent<RecordingCombatSessionOverlay>();
            OneRowStageRunBootstrap bootstrap = root.AddComponent<OneRowStageRunBootstrap>();
            OneRowStageRunFactAdapter factAdapter = root.AddComponent<OneRowStageRunFactAdapter>();
            OneRowStageRunResultPresenter presenter =
                root.AddComponent<OneRowStageRunResultPresenter>();

            SetPrivateField(bootstrap, "playableStageDefinition", route);
            SetPrivateField(bootstrap, "encounter", encounter);
            SetPrivateField(factAdapter, "encounter", encounter);
            SetPrivateField(factAdapter, "playerHealth", playerHealth);
            SetPrivateField(
                factAdapter,
                "supportSummonActions",
                Array.Empty<PlayerSupportSummonSlotAction>());
            SetPrivateField(factAdapter, "resultSurfaceBehaviour", resultSurface);
            SetPrivateField(presenter, "encounter", encounter);
            SetPrivateField(presenter, "resultOverlayBehaviour", resultOverlay);
            SetPrivateField(presenter, "resultSurfaceBehaviour", resultSurface);
            SetPrivateField(presenter, "factAdapter", factAdapter);
            SetPrivateField(bootstrap, "factAdapter", factAdapter);
            SetPrivateField(bootstrap, "resultPresenter", presenter);

            var fixture = new OneRowAdapterFixture(
                root,
                encounter,
                playerHealth,
                enemyHealth,
                bootstrap,
                factAdapter,
                presenter,
                resultOverlay,
                resultSurface);
            beforeActivate?.Invoke(fixture);
            root.SetActive(true);
            return fixture;
        }

        private static void DisableOneRowHostRuntime(Scene scene)
        {
            DisableSceneBehaviours<OlympusCorridorCombatFlowController>(scene);
            DisableSceneBehaviours<OlympusStationRunFactCollector>(scene);
            DisableSceneBehaviours<OlympusStationCombatResultPresenter>(scene);
            DisableSceneBehaviours<StageCountOneEncounterExecutor>(scene);
            DisableSceneBehaviours<CombatEncounterController>(scene);
        }

        private static void DisableSceneBehaviours<T>(Scene scene)
            where T : Behaviour
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
            {
                T[] behaviours = roots[rootIndex].GetComponentsInChildren<T>(true);
                for (int behaviourIndex = 0; behaviourIndex < behaviours.Length; behaviourIndex++)
                {
                    behaviours[behaviourIndex].enabled = false;
                }
            }
        }

        private static PlayableStageDefinition CreateOneRowEntryFinalRoute(
            PlayableStageDefinition source)
        {
            Assert.That(source, Is.Not.Null);
            PlayableStageDefinition route = null;
            LinearStageTemplateProfile template = null;
            StageResultPresentationProfile presentationProfile = null;
            StageResultPresentationCatalog presentationCatalog = null;
            StageResultDefinition resultDefinition = null;
            StageProgressionNode node = null;
            StageProgressionGraph graph = null;
            try
            {
                route = UnityEngine.Object.Instantiate(source);
                route.name = "B0_OneRowEntryFinal_TestFixture";
                route.hideFlags = HideFlags.HideAndDontSave;
                Assert.That(route.ReferenceBlock, Is.Not.SameAs(source.ReferenceBlock));
                Assert.That(
                    route.ResultProgressionJoin,
                    Is.Not.SameAs(source.ResultProgressionJoin));

                SetPrivateField(route, "playableStageId", OneRowPlayableStageId);
                SetPrivateField(route, "routeRevision", 3);
                StageSceneSegmentRef segment = route.GetSceneSegment(0);
                SetPrivateField(segment, "segmentId", OneRowSegmentId);
                SetPrivateField(segment, "sequenceIndex", 0);
                SetPrivateField(segment, "entryConditionId", "run.entry.admitted");
                SetPrivateField(
                    segment,
                    "entryConditionKind",
                    StageSegmentConditionKind.RunEntrySnapshotValidatedAndFirstSegmentActivated);
                SetPrivateField(segment, "exitConditionId", OneRowTerminalConditionId);
                SetPrivateField(
                    segment,
                    "exitConditionKind",
                    StageSegmentConditionKind
                        .StationTerminalQueueDrainedSubjectsFinalizedAndEvidenceMatched);
                SetPrivateField(segment, "handoffPolicy", StageSceneHandoffPolicy.ReturnToOwner);
                SetPrivateField(segment, "successorKind", StageSegmentSuccessorKind.None);
                SetPrivateField(
                    segment,
                    "destinationSceneKind",
                    StageSegmentDestinationSceneKind.None);
                SetPrivateField(
                    segment,
                    "transitionTokenKind",
                    StageSegmentTransitionTokenKind.None);
                SetPrivateField(
                    segment,
                    "loaderGenerationKind",
                    StageSegmentLoaderGenerationKind.None);
                SetPrivateField(
                    segment,
                    "navigationAuthorityKind",
                    StageSegmentNavigationAuthorityKind.None);
                SetPrivateField(
                    segment,
                    "returnOwnerKind",
                    StageSegmentReturnOwnerKind.P1AStageRunRouteOwner);
                SetPrivateField(
                    segment,
                    "returnOwnerReceiptPolicy",
                    StageReturnOwnerReceiptPolicy
                        .ExactTerminalRecordExactlyOnceToTerminalFinalizingCommittedPresented);
                SetPrivateField(route, "sceneSegments", new[] { segment });
                for (int actionIndex = 0;
                    actionIndex < route.TerminalActionCount;
                    actionIndex++)
                {
                    StageRouteActionRef action = route.GetTerminalAction(actionIndex);
                    switch (action.ActionKind)
                    {
                        case StageRouteActionKind.Replay:
                            SetPrivateField(action, "actionId", OneRowReplayActionId);
                            SetPrivateField(
                                action,
                                "targetPlayableStageId",
                                OneRowPlayableStageId);
                            break;
                        case StageRouteActionKind.Retry:
                            SetPrivateField(action, "actionId", OneRowRetryActionId);
                            SetPrivateField(
                                action,
                                "targetPlayableStageId",
                                OneRowPlayableStageId);
                            break;
                        case StageRouteActionKind.UIRoute:
                            SetPrivateField(action, "actionId", OneRowLobbyActionId);
                            break;
                    }
                }

                SetPrivateField(route, "canonicalRouteDigest", string.Empty);
                SetPrivateField(
                    route,
                    "canonicalRouteDigest",
                    route.ComputeCanonicalRouteDigest());

                template = UnityEngine.Object.Instantiate(source.ReferenceBlock.StageTemplate);
                template.name = "B0_OneRowEntryFinal_Template_TestFixture";
                template.hideFlags = HideFlags.HideAndDontSave;
                SetPrivateField(
                    template,
                    "stageTemplateId",
                    "b0.one-row.entry-final.template");
                StageTemplateRouteSegmentRef templateSegment =
                    ReadPrivateField<StageTemplateRouteSegmentRef[]>(
                        template,
                        "canonicalRouteSegments")[0];
                SetPrivateField(
                    templateSegment,
                    "templateSegmentId",
                    "b0.one-row.entry-final");
                SetPrivateField(templateSegment, "routeSegmentId", OneRowSegmentId);
                SetPrivateField(templateSegment, "routeSequenceIndex", 0);
                SetPrivateField(
                    template,
                    "canonicalRouteSegments",
                    new[] { templateSegment });
                SetPrivateField(template, "canonicalTemplateDigest", string.Empty);
                SetPrivateField(
                    template,
                    "canonicalTemplateDigest",
                    template.ComputeCanonicalTemplateDigest());
                SetPrivateField(route.ReferenceBlock, "stageTemplate", template);
                SetPrivateField(
                    route.ReferenceBlock,
                    "canonicalReferenceDigest",
                    string.Empty);
                SetPrivateField(
                    route.ReferenceBlock,
                    "canonicalReferenceDigest",
                    route.ComputeCanonicalReferenceDigest());
                SetPrivateField(
                    route.ReferenceBlock,
                    "canonicalBriefingDigest",
                    string.Empty);
                Assert.That(
                    route.TryComputeCanonicalBriefingDigest(
                        out string briefingDigest,
                        out StageBriefingBuildRejectReason briefingRejectReason),
                    Is.True,
                    briefingRejectReason.ToString());
                SetPrivateField(
                    route.ReferenceBlock,
                    "canonicalBriefingDigest",
                    briefingDigest);

                StageResultProgressionJoinBlock sourceJoin = source.ResultProgressionJoin;
                StageResultDefinition sourceResultDefinition = sourceJoin.ResultDefinition;
                presentationProfile = UnityEngine.Object.Instantiate(
                    sourceResultDefinition.PresentationProfile);
                presentationProfile.name =
                    "B0_OneRowEntryFinal_ResultProfile_TestFixture";
                presentationProfile.hideFlags = HideFlags.HideAndDontSave;
                SetPrivateField(
                    presentationProfile,
                    "profileId",
                    "stage-result.b0-one-row");
                SetPrivateField(
                    presentationProfile,
                    "playableStageId",
                    OneRowPlayableStageId);
                SetPrivateField(presentationProfile, "stageCode", "B0-1");

                presentationCatalog = UnityEngine.Object.Instantiate(
                    sourceResultDefinition.CanonicalPresentationCatalog);
                presentationCatalog.name =
                    "B0_OneRowEntryFinal_ResultCatalog_TestFixture";
                presentationCatalog.hideFlags = HideFlags.HideAndDontSave;
                SetPrivateField(
                    presentationCatalog,
                    "catalogId",
                    "b0.one-row.result-presentation-catalog");
                SetPrivateField(
                    presentationCatalog,
                    "profiles",
                    new[] { presentationProfile });
                Assert.That(
                    presentationCatalog.TryValidate(out string presentationCatalogError),
                    Is.True,
                    presentationCatalogError);

                resultDefinition = UnityEngine.Object.Instantiate(sourceResultDefinition);
                resultDefinition.name =
                    "B0_OneRowEntryFinal_ResultDefinition_TestFixture";
                resultDefinition.hideFlags = HideFlags.HideAndDontSave;
                SetPrivateField(
                    resultDefinition,
                    "resultDefinitionId",
                    "b0.one-row.result-definition");
                SetPrivateField(
                    resultDefinition,
                    "playableStageId",
                    OneRowPlayableStageId);
                SetPrivateField(
                    resultDefinition,
                    "canonicalPresentationCatalog",
                    presentationCatalog);
                SetPrivateField(
                    resultDefinition,
                    "presentationProfile",
                    presentationProfile);
                StageResultActionPresentationMapping[] mappings =
                    ReadPrivateField<StageResultActionPresentationMapping[]>(
                        resultDefinition,
                        "actionMappings");
                for (int mappingIndex = 0; mappingIndex < mappings.Length; mappingIndex++)
                {
                    StageResultActionPresentationMapping mapping = mappings[mappingIndex];
                    string actionId = mapping.ActionId switch
                    {
                        "olympus-invasion.replay" => OneRowReplayActionId,
                        "olympus-invasion.retry" => OneRowRetryActionId,
                        "olympus-invasion.to-lobby" => OneRowLobbyActionId,
                        _ => mapping.ActionId
                    };
                    SetPrivateField(mapping, "actionId", actionId);
                }

                SetPrivateField(resultDefinition, "evaluationContentDigest", string.Empty);
                SetPrivateField(resultDefinition, "presentationBindingDigest", string.Empty);
                SetPrivateField(resultDefinition, "presentationSourceDigest", string.Empty);
                Assert.That(
                    resultDefinition.TryComputeCanonicalDigests(
                        out string resultEvaluationDigest,
                        out string resultBindingDigest,
                        out string resultSourceDigest,
                        out string resultDigestError),
                    Is.True,
                    resultDigestError);
                SetPrivateField(
                    resultDefinition,
                    "evaluationContentDigest",
                    resultEvaluationDigest);
                SetPrivateField(
                    resultDefinition,
                    "presentationBindingDigest",
                    resultBindingDigest);
                SetPrivateField(
                    resultDefinition,
                    "presentationSourceDigest",
                    resultSourceDigest);
                Assert.That(
                    resultDefinition.TryCreateSnapshot(out _, out string resultSnapshotError),
                    Is.True,
                    resultSnapshotError);

                node = UnityEngine.Object.Instantiate(sourceJoin.ProgressionNode);
                node.name = "B0_OneRowEntryFinal_Node_TestFixture";
                node.hideFlags = HideFlags.HideAndDontSave;
                SetPrivateField(
                    node,
                    "progressionNodeId",
                    "b0.one-row.progression-node");
                SetPrivateField(node, "playableStageId", OneRowPlayableStageId);
                SetPrivateField(node, "routeRevision", route.RouteRevision);
                SetPrivateField(node, "canonicalRouteDigest", route.CanonicalRouteDigest);
                SetPrivateField(
                    node,
                    "progressionGraphId",
                    "b0.one-row.progression-graph");
                SetPrivateField(node, "contentDigest", string.Empty);
                SetPrivateField(node, "bindingDigest", string.Empty);
                Assert.That(
                    node.TryComputeCanonicalDigests(
                        out string contentDigest,
                        out string bindingDigest,
                        out string nodeDigestError),
                    Is.True,
                    nodeDigestError);
                SetPrivateField(node, "contentDigest", contentDigest);
                SetPrivateField(node, "bindingDigest", bindingDigest);

                graph = UnityEngine.Object.Instantiate(sourceJoin.ProgressionGraph);
                graph.name = "B0_OneRowEntryFinal_Graph_TestFixture";
                graph.hideFlags = HideFlags.HideAndDontSave;
                SetPrivateField(
                    graph,
                    "progressionGraphId",
                    "b0.one-row.progression-graph");
                SetPrivateField(graph, "nodes", new[] { node });
                SetPrivateField(graph, "canonicalDigest", string.Empty);
                Assert.That(
                    graph.TryComputeCanonicalDigest(
                        out string graphDigest,
                        out string graphDigestError),
                    Is.True,
                    graphDigestError);
                SetPrivateField(graph, "canonicalDigest", graphDigest);

                SetPrivateField(
                    route.ResultProgressionJoin,
                    "resultDefinition",
                    resultDefinition);
                SetPrivateField(
                    route.ResultProgressionJoin,
                    "canonicalPresentationCatalog",
                    presentationCatalog);
                SetPrivateField(route.ResultProgressionJoin, "progressionNode", node);
                SetPrivateField(route.ResultProgressionJoin, "progressionGraph", graph);
                SetPrivateField(
                    route.ResultProgressionJoin,
                    "canonicalDigest",
                    string.Empty);
                Assert.That(
                    route.TryComputeResultProgressionJoinDigest(
                        out string joinDigest,
                        out string joinDigestError),
                    Is.True,
                    joinDigestError);
                SetPrivateField(route.ResultProgressionJoin, "canonicalDigest", joinDigest);
                Assert.That(
                    StageRunResultProgressionJoinSnapshot.TryCreate(
                        route,
                        out _,
                        out string joinError),
                    Is.True,
                    joinError);
                return route;
            }
            catch
            {
                if (route != null)
                {
                    UnityEngine.Object.DestroyImmediate(route);
                }

                if (graph != null)
                {
                    UnityEngine.Object.DestroyImmediate(graph);
                }

                if (node != null)
                {
                    UnityEngine.Object.DestroyImmediate(node);
                }

                if (resultDefinition != null)
                {
                    UnityEngine.Object.DestroyImmediate(resultDefinition);
                }

                if (presentationCatalog != null)
                {
                    UnityEngine.Object.DestroyImmediate(presentationCatalog);
                }

                if (presentationProfile != null)
                {
                    UnityEngine.Object.DestroyImmediate(presentationProfile);
                }

                if (template != null)
                {
                    UnityEngine.Object.DestroyImmediate(template);
                }

                throw;
            }
        }

        private static void DestroyOneRowEntryFinalRoute(PlayableStageDefinition route)
        {
            LinearStageTemplateProfile template = route?.ReferenceBlock?.StageTemplate;
            StageResultDefinition resultDefinition =
                route?.ResultProgressionJoin?.ResultDefinition;
            StageResultPresentationCatalog presentationCatalog =
                route?.ResultProgressionJoin?.CanonicalPresentationCatalog;
            StageResultPresentationProfile presentationProfile =
                resultDefinition?.PresentationProfile;
            StageProgressionNode node = route?.ResultProgressionJoin?.ProgressionNode;
            StageProgressionGraph graph = route?.ResultProgressionJoin?.ProgressionGraph;
            if (route != null)
            {
                UnityEngine.Object.DestroyImmediate(route);
            }

            if (graph != null)
            {
                UnityEngine.Object.DestroyImmediate(graph);
            }

            if (node != null)
            {
                UnityEngine.Object.DestroyImmediate(node);
            }

            if (resultDefinition != null)
            {
                UnityEngine.Object.DestroyImmediate(resultDefinition);
            }

            if (presentationCatalog != null)
            {
                UnityEngine.Object.DestroyImmediate(presentationCatalog);
            }

            if (presentationProfile != null)
            {
                UnityEngine.Object.DestroyImmediate(presentationProfile);
            }

            if (template != null)
            {
                UnityEngine.Object.DestroyImmediate(template);
            }
        }

        private static T ReadPrivateField<T>(object owner, string fieldName)
        {
            FieldInfo field = owner.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Missing field {owner.GetType().Name}.{fieldName}.");
            return (T)field.GetValue(owner);
        }

        private static void SetPrivateField(object owner, string fieldName, object value)
        {
            FieldInfo field = owner.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Missing field {owner.GetType().Name}.{fieldName}.");
            field.SetValue(owner, value);
        }

        private static void AssertContainsNoUnityObjectFields(Type type)
        {
            FieldInfo[] fields = type.GetFields(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            for (int i = 0; i < fields.Length; i++)
            {
                Type fieldType = fields[i].FieldType;
                Type elementType = fieldType.IsArray ? fieldType.GetElementType() : fieldType;
                Assert.That(
                    typeof(UnityEngine.Object).IsAssignableFrom(elementType),
                    Is.False,
                    $"{type.FullName}.{fields[i].Name} retains a Unity object reference.");
            }
        }
    }

    [DefaultExecutionOrder(9400)]
    public sealed class OneRowPreCoordinatorAdapterDisableProbe : MonoBehaviour
    {
        public Behaviour Target { get; set; }

        private void Start()
        {
            if (Target != null)
            {
                Target.enabled = false;
            }
        }
    }

    [DefaultExecutionOrder(10000)]
    public sealed class StageCountOneExecutorShutdownProbe : MonoBehaviour
    {
        public Action Disabled { get; set; }

        private void OnDisable()
        {
            Disabled?.Invoke();
        }
    }
}
