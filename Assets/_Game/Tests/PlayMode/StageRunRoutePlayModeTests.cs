using System;
using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using DimensionBrawl.AI;
using DimensionBrawl.Combat;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Player;
using DimensionBrawl.UI;
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

        [UnityTearDown]
        public IEnumerator ResetStageRunRuntime()
        {
            StageRunRuntime.ResetForTests();
            Time.timeScale = 1f;
            yield return null;
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
            Assert.That(executor.OwnedObjectCount, Is.EqualTo(1));
            Assert.That(executor.HasCombatantParticipation, Is.True);
            Assert.That(executor.HasSceneLease, Is.True);

            int executorDisableCallbackCount = 0;
            StageCountOneEncounterState executorStateAtDisable = default;
            int executorCancellationCountAtDisable = -1;
            int executorCompletionCountAtDisable = -1;
            bool executorOwnershipClearedAtDisable = false;
            bool executorParticipationClearedAtDisable = false;
            bool executorSceneLeaseReleasedAtDisable = false;
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
        public IEnumerator CanonicalStationGuideReleaseActivatesAndCompletesCountOneAdd()
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

            CombatHealth addHealth = executor.OwnedHealth;
            ICombatAiAgent addAgent = executor.OwnedAgent;
            CombatTargetSensor addSensor = executor.OwnedSensor;
            PlayerCombatTargetSelector playerTargetSelector = executor.PlayerTargetSelector;
            GameObject ownedRoot = executor.OwnedRoot;
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
            Assert.That(playerHealth, Is.Not.Null);
            Assert.That(bossHealth, Is.Not.Null);
            Assert.That(bossHealth, Is.Not.SameAs(addHealth));
            Assert.That(playerTargetSelector, Is.Not.Null);
            Assert.That(playerTargetSelector.SelfHealth, Is.SameAs(playerHealth));
            Assert.That(playerTargetSelector.ContainsAuthoredTargetCandidate(bossHealth), Is.True);
            Assert.That(playerTargetSelector.ContainsRuntimeTargetCandidate(addHealth), Is.True);
            Assert.That(playerTargetSelector.RuntimeTargetCandidateCount, Is.EqualTo(1));
            Assert.That(executor.HasCombatantParticipation, Is.True);
            Assert.That(addSensor.TargetCandidateCount, Is.EqualTo(1));
            Assert.That(addSensor.ContainsTargetCandidate(playerHealth), Is.True);
            if (addSensor.TryGetCurrentTarget(out _, out CombatHealth initialSensedHealth))
            {
                Assert.That(initialSensedHealth, Is.SameAs(playerHealth));
            }
            Assert.That(executor.ActivationCount, Is.EqualTo(1));
            Assert.That(executor.OwnedObjectCount, Is.EqualTo(1));
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
            Assert.That(ApplyLethalDamage(bossHealth, DamageTeam.Player), Is.True);

            Assert.That(executor.State, Is.EqualTo(StageCountOneEncounterState.Completed));
            Assert.That(executor.CompletionCount, Is.EqualTo(1));
            Assert.That(executor.CancellationCount, Is.Zero);
            Assert.That(executor.OwnedObjectCount, Is.Zero);
            Assert.That(executor.OwnedRoot, Is.Null);
            Assert.That(executor.OwnedHealth, Is.Null);
            Assert.That(executor.OwnedAgent, Is.Null);
            Assert.That(executor.OwnedSensor, Is.Null);
            Assert.That(executor.HasCombatantParticipation, Is.False);
            Assert.That(executor.HasSceneLease, Is.True);
            Assert.That(ownedRoot.activeSelf, Is.False);
            Assert.That(playerTargetSelector.ContainsRuntimeTargetCandidate(addHealth), Is.False);
            Assert.That(playerTargetSelector.RuntimeTargetCandidateCount, Is.Zero);
            Assert.That(playerTargetSelector.ContainsAuthoredTargetCandidate(bossHealth), Is.True);
            Assert.That(addSensor.TargetCandidateCount, Is.Zero);
            Assert.That(addSensor.CurrentTargetHealth, Is.Null);

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
        }

        [UnityTest]
        public IEnumerator CanonicalStationTerminalOutcomeCancelsLivingCountOneAdd()
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

            CombatHealth addHealth = executor.OwnedHealth;
            ICombatAiAgent addAgent = executor.OwnedAgent;
            CombatTargetSensor addSensor = executor.OwnedSensor;
            PlayerCombatTargetSelector playerTargetSelector = executor.PlayerTargetSelector;
            GameObject ownedRoot = executor.OwnedRoot;
            CombatEncounterController encounter =
                RequireSingleSceneComponent<CombatEncounterController>(station);
            CombatHealth playerHealth = encounter.PlayerHealth;
            CombatHealth bossHealth = encounter.EnemyHealth;
            Assert.That(addHealth, Is.Not.Null);
            Assert.That(addAgent, Is.Not.Null);
            Assert.That(addSensor, Is.Not.Null);
            Assert.That(playerTargetSelector, Is.Not.Null);
            Assert.That(ownedRoot, Is.Not.Null);
            Assert.That(bossHealth, Is.Not.SameAs(addHealth));
            Assert.That(playerTargetSelector.ContainsRuntimeTargetCandidate(addHealth), Is.True);
            Assert.That(playerTargetSelector.ContainsAuthoredTargetCandidate(bossHealth), Is.True);

            Transform playerForwardBoundary = RequireSingleSceneTransform(
                station,
                "PlayerForwardBoundaryAnchor");
            MoveCombatSubjectToAnchor(playerHealth, playerForwardBoundary);
            yield return null;

            int exactAddDamageCount = 0;
            playerHealth.Damaged += HandlePlayerDamaged;
            float windupDeadline = Time.realtimeSinceStartup + 18f;
            while (addAgent.CurrentPatternState != CombatAiPatternState.Windup)
            {
                Assert.That(executor.State, Is.EqualTo(StageCountOneEncounterState.Active));
                Assert.That(addHealth.IsAlive, Is.True);
                Assert.Less(
                    Time.realtimeSinceStartup,
                    windupDeadline,
                    "Timed out waiting to cancel the runtime Add during its authored Windup. "
                    + $"distance={Vector3.ProjectOnPlane(addHealth.transform.position - playerHealth.transform.position, Vector3.up).magnitude:F2}, "
                    + $"state={addAgent.CurrentPatternState}, sensorTarget={addSensor.CurrentTargetHealth?.name ?? "none"}, "
                    + $"timeScale={Time.timeScale:F2}.");
                yield return null;
            }

            int damageCountAtTerminal = exactAddDamageCount;
            Assert.That(ApplyLethalDamage(bossHealth, DamageTeam.Player), Is.True);

            Assert.That(executor.State, Is.EqualTo(StageCountOneEncounterState.Cancelled));
            Assert.That(executor.ActivationCount, Is.EqualTo(1));
            Assert.That(executor.CompletionCount, Is.Zero);
            Assert.That(executor.CancellationCount, Is.EqualTo(1));
            Assert.That(executor.OwnedObjectCount, Is.Zero);
            Assert.That(executor.HasCombatantParticipation, Is.False);
            Assert.That(ownedRoot.activeSelf, Is.False);
            Assert.That(playerTargetSelector.ContainsRuntimeTargetCandidate(addHealth), Is.False);
            Assert.That(playerTargetSelector.RuntimeTargetCandidateCount, Is.Zero);
            Assert.That(playerTargetSelector.ContainsAuthoredTargetCandidate(bossHealth), Is.True);
            Assert.That(addSensor.TargetCandidateCount, Is.Zero);
            Assert.That(addSensor.CurrentTargetHealth, Is.Null);

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
                if (ReferenceEquals(damageInfo.Source, addHealth))
                {
                    exactAddDamageCount++;
                }
            }
        }

        [UnityTest]
        public IEnumerator CanonicalStationExecutorDisableSynchronouslyCleansCountOneAddWithoutRespawn()
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

            CombatHealth addHealth = executor.OwnedHealth;
            CombatTargetSensor addSensor = executor.OwnedSensor;
            PlayerCombatTargetSelector playerTargetSelector = executor.PlayerTargetSelector;
            GameObject ownedRoot = executor.OwnedRoot;
            CombatHealth bossHealth =
                RequireSingleSceneComponent<CombatEncounterController>(station).EnemyHealth;
            Assert.That(addHealth, Is.Not.Null);
            Assert.That(addSensor, Is.Not.Null);
            Assert.That(playerTargetSelector.ContainsRuntimeTargetCandidate(addHealth), Is.True);
            Assert.That(playerTargetSelector.ContainsAuthoredTargetCandidate(bossHealth), Is.True);
            Assert.That(executor.HasSceneLease, Is.True);

            executor.enabled = false;

            Assert.That(executor.State, Is.EqualTo(StageCountOneEncounterState.Cancelled));
            Assert.That(executor.ActivationCount, Is.EqualTo(1));
            Assert.That(executor.CompletionCount, Is.Zero);
            Assert.That(executor.CancellationCount, Is.EqualTo(1));
            Assert.That(executor.OwnedObjectCount, Is.Zero);
            Assert.That(executor.HasCombatantParticipation, Is.False);
            Assert.That(executor.HasSceneLease, Is.False);
            Assert.That(ownedRoot.activeSelf, Is.False);
            Assert.That(playerTargetSelector.ContainsRuntimeTargetCandidate(addHealth), Is.False);
            Assert.That(playerTargetSelector.RuntimeTargetCandidateCount, Is.Zero);
            Assert.That(playerTargetSelector.ContainsAuthoredTargetCandidate(bossHealth), Is.True);
            Assert.That(addSensor.TargetCandidateCount, Is.Zero);
            Assert.That(addSensor.CurrentTargetHealth, Is.Null);

            executor.enabled = true;
            yield return null;
            yield return null;

            Assert.That(executor.State, Is.EqualTo(StageCountOneEncounterState.Cancelled));
            Assert.That(executor.ActivationCount, Is.EqualTo(1));
            Assert.That(executor.CancellationCount, Is.EqualTo(1));
            Assert.That(executor.OwnedObjectCount, Is.Zero);
            Assert.That(executor.HasCombatantParticipation, Is.False);
            Assert.That(executor.HasSceneLease, Is.False);
        }

        [UnityTest]
        public IEnumerator CanonicalStationSensorLeaseLossFaultsAndCleansCountOneAddWithoutRespawn()
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

            CombatHealth addHealth = executor.OwnedHealth;
            CombatTargetSensor addSensor = executor.OwnedSensor;
            PlayerCombatTargetSelector playerTargetSelector = executor.PlayerTargetSelector;
            GameObject ownedRoot = executor.OwnedRoot;
            CombatHealth bossHealth =
                RequireSingleSceneComponent<CombatEncounterController>(station).EnemyHealth;
            Assert.That(addHealth, Is.Not.Null);
            Assert.That(addSensor, Is.Not.Null);
            Assert.That(playerTargetSelector.ContainsRuntimeTargetCandidate(addHealth), Is.True);

            addSensor.enabled = false;
            yield return null;

            Assert.That(executor.State, Is.EqualTo(StageCountOneEncounterState.Faulted));
            Assert.That(executor.LastError, Does.Contain("bidirectional combatant participation lease"));
            Assert.That(executor.ActivationCount, Is.EqualTo(1));
            Assert.That(executor.CompletionCount, Is.Zero);
            Assert.That(executor.CancellationCount, Is.Zero);
            Assert.That(executor.OwnedObjectCount, Is.Zero);
            Assert.That(executor.OwnedRoot, Is.Null);
            Assert.That(executor.OwnedHealth, Is.Null);
            Assert.That(executor.OwnedAgent, Is.Null);
            Assert.That(executor.OwnedSensor, Is.Null);
            Assert.That(executor.HasCombatantParticipation, Is.False);
            Assert.That(executor.HasSceneLease, Is.True);
            Assert.That(
                ownedRoot == null || !ownedRoot.activeSelf,
                Is.True,
                "Fault cleanup must destroy or deactivate the owned Add root.");
            Assert.That(playerTargetSelector.ContainsRuntimeTargetCandidate(addHealth), Is.False);
            Assert.That(playerTargetSelector.RuntimeTargetCandidateCount, Is.Zero);
            Assert.That(playerTargetSelector.ContainsAuthoredTargetCandidate(bossHealth), Is.True);
            Assert.That(addSensor.TargetCandidateCount, Is.Zero);
            Assert.That(addSensor.CurrentTargetHealth, Is.Null);

            yield return null;
            yield return null;
            Assert.That(executor.State, Is.EqualTo(StageCountOneEncounterState.Faulted));
            Assert.That(executor.ActivationCount, Is.EqualTo(1));
            Assert.That(executor.OwnedObjectCount, Is.Zero);
        }

        private static IEnumerator EnterCanonicalStation()
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
            CharacterController controller = health.GetComponent<CharacterController>();
            bool restoreController = controller != null && controller.enabled;
            if (restoreController)
            {
                controller.enabled = false;
            }

            Vector3 destination = anchor.position;
            destination.y = health.transform.position.y;
            health.transform.position = destination;
            if (restoreController)
            {
                controller.enabled = true;
            }

            Physics.SyncTransforms();
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
