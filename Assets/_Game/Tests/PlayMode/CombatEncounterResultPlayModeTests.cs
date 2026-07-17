using System;
using DimensionBrawl.Combat;
using DimensionBrawl.LevelDesign;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DimensionBrawl.Tests
{
    public sealed class CombatEncounterResultPlayModeTests
    {
        [Test]
        public void EnemyDeathPublishesWinExactlyOnce()
        {
            RunTerminalResultTest(
                healthToDefeatIsEnemy: true,
                expectedWinCount: 1,
                expectedFailureCount: 0);
        }

        [Test]
        public void PlayerDeathPublishesFailureExactlyOnce()
        {
            RunTerminalResultTest(
                healthToDefeatIsEnemy: false,
                expectedWinCount: 0,
                expectedFailureCount: 1);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void SameRootTerminalTradeClearsAndRetainsPlayerDown(bool playerQueuedFirst)
        {
            using EncounterFixture fixture = new();

            CombatRootAdmissionResult admission = fixture.Encounter.AdmitCombatRoot(context =>
            {
                if (playerQueuedFirst)
                {
                    Assert.That(context.TryApplyDamage(
                        fixture.PlayerHealth,
                        CreateLethalDamage(fixture.PlayerHealth, DamageTeam.Enemy)), Is.True);
                    Assert.That(context.TryApplyDamage(
                        fixture.EnemyHealth,
                        CreateLethalDamage(fixture.EnemyHealth, DamageTeam.Player)), Is.True);
                    return;
                }

                Assert.That(context.TryApplyDamage(
                    fixture.EnemyHealth,
                    CreateLethalDamage(fixture.EnemyHealth, DamageTeam.Player)), Is.True);
                Assert.That(context.TryApplyDamage(
                    fixture.PlayerHealth,
                    CreateLethalDamage(fixture.PlayerHealth, DamageTeam.Enemy)), Is.True);
            });

            Assert.That(admission.Disposition, Is.EqualTo(CombatRootAdmissionDisposition.Executed));
            Assert.That(fixture.Encounter.IsWon, Is.True);
            Assert.That(fixture.Encounter.IsFailed, Is.False);
            Assert.That(fixture.Encounter.HasTerminalResolution, Is.True);
            Assert.That(
                fixture.Encounter.TerminalResolution.Reason,
                Is.EqualTo(EncounterTerminalReason.SimultaneousTerminalClear));
            Assert.That(fixture.Encounter.TerminalResolution.PlayerDown, Is.True);
            Assert.That(fixture.Encounter.TerminalResolution.BossDead, Is.True);
            EncounterTerminalEpochEvidence closure =
                fixture.Encounter.TerminalCoordinator.TerminalEpochEvidence;
            Assert.That(fixture.Encounter.TerminalCoordinator.HasTerminalEpochEvidence, Is.True);
            Assert.That(closure.QueueDrained, Is.True);
            Assert.That(closure.BothSubjectsFinalized, Is.True);
            Assert.That(closure.ActiveTokenInvalidated, Is.True);
            Assert.That(closure.SubjectSnapshotCount, Is.EqualTo(2));
            Assert.That(
                closure.GetSubjectSnapshot(0).SubjectRole,
                Is.EqualTo(EncounterTerminalSubjectRole.Player));
            Assert.That(
                closure.GetSubjectSnapshot(1).SubjectRole,
                Is.EqualTo(EncounterTerminalSubjectRole.Boss));
            Assert.That(closure.CandidateCoverageCount, Is.EqualTo(2));
            Assert.That(
                closure.GetCandidateCoverage(0).IntraRootSequence,
                Is.LessThan(closure.GetCandidateCoverage(1).IntraRootSequence));
            Assert.That(closure.DiscardedAdmissionCount, Is.Zero);
        }

        [Test]
        public void CallbackOrderPermutationKeepsFinalSummaryDigestByteIdentical()
        {
            PlayableStageDefinition definition = AssetDatabase.LoadAssetAtPath<PlayableStageDefinition>(
                "Assets/_Game/DesignData/Profiles/ActionFoundation/StageDefinitions/DB_PlayableStage_OlympusInvasion.asset");
            Assert.That(definition, Is.Not.Null);
            Assert.That(
                StageRunRouteSnapshot.TryCreate(
                    definition,
                    out StageRunRouteSnapshot routeSnapshot,
                    out string routeError),
                Is.True,
                routeError);

            using EncounterFixture playerFirst = new();
            using EncounterFixture bossFirst = new();
            ResolveSameRootTerminalTrade(playerFirst, playerQueuedFirst: true);
            ResolveSameRootTerminalTrade(bossFirst, playerQueuedFirst: false);
            EncounterTerminalResolution playerFirstResolution = playerFirst.Encounter.TerminalResolution;
            EncounterTerminalResolution bossFirstResolution = bossFirst.Encounter.TerminalResolution;
            Assert.That(playerFirstResolution.RootAdmissionSequence, Is.EqualTo(bossFirstResolution.RootAdmissionSequence));
            Assert.That(playerFirstResolution.Epoch, Is.EqualTo(bossFirstResolution.Epoch));

            Assert.That(
                StageRunResultSummary.TryCreateCallbackOrderDigestComparisonForTests(
                    routeSnapshot,
                    playerFirst.Encounter.TerminalCoordinator.TerminalEpochEvidence,
                    bossFirst.Encounter.TerminalCoordinator.TerminalEpochEvidence,
                    out string playerFirstClosureDigest,
                    out string bossFirstClosureDigest,
                    out string playerFirstSummaryDigest,
                    out string bossFirstSummaryDigest,
                    out string comparisonError),
                Is.True,
                comparisonError);
            Assert.That(
                playerFirstClosureDigest,
                Is.Not.EqualTo(bossFirstClosureDigest),
                "Audit evidence must retain the authoritative queue order.");
            Assert.That(
                playerFirstSummaryDigest,
                Is.EqualTo(bossFirstSummaryDigest));
        }

        [Test]
        public void SameRootDeathReactionDrainsBeforeTerminalResolution()
        {
            using EncounterFixture fixture = new();
            bool resolvedInsideDeathCallback = false;

            fixture.PlayerHealth.Died += HandlePlayerDied;
            try
            {
                CombatRootAdmissionResult admission = fixture.Encounter.AdmitCombatRoot(context =>
                {
                    Assert.That(context.TryApplyDamage(
                        fixture.PlayerHealth,
                        CreateLethalDamage(fixture.PlayerHealth, DamageTeam.Enemy)), Is.True);
                });

                Assert.That(admission.Disposition, Is.EqualTo(CombatRootAdmissionDisposition.Executed));
                Assert.That(resolvedInsideDeathCallback, Is.False);
                Assert.That(fixture.Encounter.IsWon, Is.True);
                Assert.That(
                    fixture.Encounter.TerminalResolution.Reason,
                    Is.EqualTo(EncounterTerminalReason.SimultaneousTerminalClear));
            }
            finally
            {
                fixture.PlayerHealth.Died -= HandlePlayerDied;
            }

            void HandlePlayerDied()
            {
                resolvedInsideDeathCallback = fixture.Encounter.HasTerminalResolution;
                Assert.That(fixture.EnemyHealth.TryApplyDamage(
                    CreateLethalDamage(fixture.EnemyHealth, DamageTeam.Player)), Is.True);
            }
        }

        [TestCase(true)]
        [TestCase(false)]
        public void LowerIndependentRootResolvesBeforeDeferredRoot(bool playerRootFirst)
        {
            using EncounterFixture fixture = new();
            CombatRootAdmissionResult deferredAdmission = default;

            CombatRootAdmissionResult firstAdmission = fixture.Encounter.AdmitCombatRoot(firstContext =>
            {
                CombatHealth firstTarget = playerRootFirst
                    ? fixture.PlayerHealth
                    : fixture.EnemyHealth;
                DamageTeam firstSourceTeam = playerRootFirst
                    ? DamageTeam.Enemy
                    : DamageTeam.Player;
                Assert.That(firstContext.TryApplyDamage(
                    firstTarget,
                    CreateLethalDamage(firstTarget, firstSourceTeam)), Is.True);

                deferredAdmission = fixture.Encounter.AdmitCombatRoot(secondContext =>
                {
                    CombatHealth secondTarget = playerRootFirst
                        ? fixture.EnemyHealth
                        : fixture.PlayerHealth;
                    DamageTeam secondSourceTeam = playerRootFirst
                        ? DamageTeam.Player
                        : DamageTeam.Enemy;
                    secondContext.TryApplyDamage(
                        secondTarget,
                        CreateLethalDamage(secondTarget, secondSourceTeam));
                });
            });

            Assert.That(firstAdmission.RootAdmissionSequence, Is.LessThan(deferredAdmission.RootAdmissionSequence));
            Assert.That(deferredAdmission.Disposition, Is.EqualTo(CombatRootAdmissionDisposition.Deferred));
            Assert.That(fixture.Encounter.IsFailed, Is.EqualTo(playerRootFirst));
            Assert.That(fixture.Encounter.IsWon, Is.EqualTo(!playerRootFirst));
            Assert.That(
                playerRootFirst ? fixture.EnemyHealth.IsAlive : fixture.PlayerHealth.IsAlive,
                Is.True,
                "A higher independent root must be invalidated after the lower root seals a result.");
            EncounterTerminalEpochEvidence closure =
                fixture.Encounter.TerminalCoordinator.TerminalEpochEvidence;
            Assert.That(closure.DiscardedAdmissionCount, Is.EqualTo(1));
            EncounterTerminalDiscardedAdmissionEvidence discarded =
                closure.GetDiscardedAdmission(0);
            Assert.That(discarded.RootAdmissionSequence, Is.EqualTo(deferredAdmission.RootAdmissionSequence));
            Assert.That(discarded.NoTokenIssued, Is.True);
            Assert.That(
                discarded.Disposition,
                Is.EqualTo(EncounterTerminalPendingAdmissionDisposition.DiscardedAfterTerminalClosed));
        }

        [Test]
        public void DirectDamageCallbackRunsInsideAdmittedRoot()
        {
            using EncounterFixture fixture = new();
            EncounterTerminalCoordinatorState callbackState = EncounterTerminalCoordinatorState.Unbound;
            long callbackRootSequence = 0;

            fixture.EnemyHealth.Died += HandleEnemyDied;
            try
            {
                Assert.That(ApplyLethalDamage(fixture.EnemyHealth, DamageTeam.Player), Is.True);
            }
            finally
            {
                fixture.EnemyHealth.Died -= HandleEnemyDied;
            }

            Assert.That(callbackState, Is.EqualTo(EncounterTerminalCoordinatorState.Draining));
            Assert.That(callbackRootSequence, Is.GreaterThan(0));
            Assert.That(fixture.Encounter.IsWon, Is.True);

            void HandleEnemyDied()
            {
                callbackState = fixture.Encounter.TerminalCoordinator.State;
                callbackRootSequence = fixture.Encounter.TerminalCoordinator.ActiveRootAdmissionSequence;
            }
        }

        [Test]
        public void MutationCallbackCannotMintIndependentRoot()
        {
            using EncounterFixture fixture = new();
            bool nestedProducerRan = false;
            CombatRootAdmissionResult callbackAdmission = default;
            fixture.PlayerHealth.Damaged += HandlePlayerDamaged;
            try
            {
                Assert.That(
                    fixture.PlayerHealth.TryApplyDamage(
                        CreateDamage(fixture.PlayerHealth, DamageTeam.Enemy, 1f)),
                    Is.True);
            }
            finally
            {
                fixture.PlayerHealth.Damaged -= HandlePlayerDamaged;
            }

            Assert.That(
                callbackAdmission.Disposition,
                Is.EqualTo(CombatRootAdmissionDisposition.Rejected));
            Assert.That(nestedProducerRan, Is.False);
            Assert.That(fixture.Encounter.IsFaulted, Is.True);
            Assert.That(
                fixture.Encounter.Diagnostic.Reason,
                Is.EqualTo(EncounterTerminalDiagnosticReason.ReentrantCallbackRootAdmission));

            void HandlePlayerDamaged(DamageInfo _)
            {
                callbackAdmission = fixture.Encounter.AdmitCombatRoot(
                    "test.callback-root",
                    _ => nestedProducerRan = true);
            }
        }

        [Test]
        public void ActiveTerminalSubjectRebindAttemptFaultsTheCurrentCoordinator()
        {
            using EncounterFixture fixture = new();
            GameObject replacementObject = new("EncounterResultReplacementEnemy");
            CombatHealth replacementHealth = CreateHealth(replacementObject, DamageTeam.Enemy);
            EncounterTerminalResolutionCoordinator coordinator = fixture.Encounter.TerminalCoordinator;
            long runGeneration = coordinator.RunGeneration;

            try
            {
                fixture.Encounter.ConfigureCombatants(fixture.PlayerHealth, replacementHealth);

                Assert.That(fixture.Encounter.TerminalCoordinator, Is.SameAs(coordinator));
                Assert.That(fixture.Encounter.RunGeneration, Is.EqualTo(runGeneration));
                Assert.That(coordinator.State, Is.EqualTo(EncounterTerminalCoordinatorState.Faulted));
                Assert.That(fixture.Encounter.IsFaulted, Is.True);
                Assert.That(fixture.Encounter.HasTerminalResolution, Is.False);
                Assert.That(
                    fixture.Encounter.Diagnostic.Reason,
                    Is.EqualTo(EncounterTerminalDiagnosticReason.SubjectUnavailable));
                Assert.That(fixture.EnemyHealth.IsTerminalMutationBound, Is.True);
                Assert.That(replacementHealth.IsTerminalMutationBound, Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(replacementObject);
            }
        }

        [Test]
        public void DisabledTerminalSubjectFaultsTheSynchronousFinalSnapshot()
        {
            using EncounterFixture fixture = new();
            float initialPlayerHealth = fixture.PlayerHealth.CurrentHealth;

            try
            {
                CombatRootAdmissionResult admission = fixture.Encounter.AdmitCombatRoot(context =>
                {
                    Assert.That(
                        context.TryApplyDamage(
                            fixture.PlayerHealth,
                            CreateDamage(fixture.PlayerHealth, DamageTeam.Enemy, 1f)),
                        Is.True);
                    fixture.EnemyHealth.enabled = false;
                });

                Assert.That(admission.Disposition, Is.EqualTo(CombatRootAdmissionDisposition.Executed));
                Assert.That(admission.CoordinatorState, Is.EqualTo(EncounterTerminalCoordinatorState.Faulted));
                Assert.That(fixture.PlayerHealth.CurrentHealth, Is.EqualTo(initialPlayerHealth - 1f));
                Assert.That(fixture.Encounter.IsFaulted, Is.True);
                Assert.That(fixture.Encounter.HasTerminalResolution, Is.False);
                Assert.That(
                    fixture.Encounter.Diagnostic.Reason,
                    Is.EqualTo(EncounterTerminalDiagnosticReason.SubjectUnavailable));
            }
            finally
            {
                fixture.EnemyHealth.enabled = true;
            }
        }

        [Test]
        public void FinalSnapshotExceptionBecomesTypedCoordinatorFaultWithoutPartialEvidence()
        {
            using EncounterFixture fixture = new();
            EncounterTerminalResolutionCoordinator coordinator = fixture.Encounter.TerminalCoordinator;
            int boundaryCount = 0;
            coordinator.SetFinalSnapshotBoundaryForTests(_ =>
            {
                boundaryCount++;
                throw new InvalidOperationException("injected final snapshot failure");
            });

            CombatRootAdmissionResult admission = fixture.Encounter.AdmitCombatRoot(context =>
            {
                Assert.That(
                    context.TryApplyDamage(
                        fixture.EnemyHealth,
                        CreateLethalDamage(fixture.EnemyHealth, DamageTeam.Player)),
                    Is.True);
            });

            Assert.That(admission.Disposition, Is.EqualTo(CombatRootAdmissionDisposition.Executed));
            Assert.That(admission.CoordinatorState, Is.EqualTo(EncounterTerminalCoordinatorState.Faulted));
            Assert.That(boundaryCount, Is.EqualTo(1));
            Assert.That(fixture.Encounter.IsFaulted, Is.True);
            Assert.That(fixture.Encounter.HasTerminalResolution, Is.False);
            Assert.That(coordinator.HasTerminalResolution, Is.False);
            Assert.That(coordinator.HasTerminalEpochEvidence, Is.False);
            Assert.That(
                fixture.Encounter.Diagnostic.Reason,
                Is.EqualTo(EncounterTerminalDiagnosticReason.FinalizationException));
            Assert.That(
                fixture.Encounter.Diagnostic.Message,
                Does.Contain("injected final snapshot failure"));
        }

        [Test]
        public void ExplicitCancelDuringOpenDiscardsQueuedMutationAuthority()
        {
            using EncounterFixture fixture = new();
            EncounterTerminalResolutionCoordinator coordinator = fixture.Encounter.TerminalCoordinator;
            float initialHealth = fixture.PlayerHealth.CurrentHealth;

            CombatRootAdmissionResult admission = fixture.Encounter.AdmitCombatRoot(context =>
            {
                Assert.That(coordinator.State, Is.EqualTo(EncounterTerminalCoordinatorState.Open));
                Assert.That(
                    context.TryApplyDamage(
                        fixture.PlayerHealth,
                        CreateDamage(fixture.PlayerHealth, DamageTeam.Enemy, 1f)),
                    Is.True);
                coordinator.Cancel();
            });

            Assert.That(admission.Disposition, Is.EqualTo(CombatRootAdmissionDisposition.Executed));
            Assert.That(admission.CoordinatorState, Is.EqualTo(EncounterTerminalCoordinatorState.Cancelled));
            Assert.That(fixture.PlayerHealth.CurrentHealth, Is.EqualTo(initialHealth));
            Assert.That(coordinator.ActiveRootAdmissionSequence, Is.Zero);
            Assert.That(coordinator.ActiveEpoch, Is.Zero);
            Assert.That(fixture.Encounter.HasTerminalResolution, Is.False);
            Assert.That(fixture.Encounter.HasDiagnostic, Is.False);
        }

        [Test]
        public void ExplicitCancelDuringDrainingDropsRemainingQueuedWork()
        {
            using EncounterFixture fixture = new();
            EncounterTerminalResolutionCoordinator coordinator = fixture.Encounter.TerminalCoordinator;
            float initialHealth = fixture.PlayerHealth.CurrentHealth;
            int damageCallbackCount = 0;
            EncounterTerminalCoordinatorState callbackState = default;
            fixture.PlayerHealth.Damaged += HandleDamaged;
            try
            {
                CombatRootAdmissionResult admission = fixture.Encounter.AdmitCombatRoot(context =>
                {
                    Assert.That(
                        context.TryApplyDamage(
                            fixture.PlayerHealth,
                            CreateDamage(fixture.PlayerHealth, DamageTeam.Enemy, 1f)),
                        Is.True);
                    Assert.That(
                        context.TryApplyDamage(
                            fixture.PlayerHealth,
                            CreateDamage(fixture.PlayerHealth, DamageTeam.Enemy, 1f)),
                        Is.True);
                });

                Assert.That(admission.Disposition, Is.EqualTo(CombatRootAdmissionDisposition.Executed));
                Assert.That(admission.CoordinatorState, Is.EqualTo(EncounterTerminalCoordinatorState.Cancelled));
                Assert.That(callbackState, Is.EqualTo(EncounterTerminalCoordinatorState.Draining));
                Assert.That(damageCallbackCount, Is.EqualTo(1));
                Assert.That(fixture.PlayerHealth.CurrentHealth, Is.EqualTo(initialHealth - 1f));
                Assert.That(coordinator.ActiveRootAdmissionSequence, Is.Zero);
                Assert.That(coordinator.ActiveEpoch, Is.Zero);
                Assert.That(fixture.Encounter.HasTerminalResolution, Is.False);
            }
            finally
            {
                fixture.PlayerHealth.Damaged -= HandleDamaged;
            }

            void HandleDamaged(DamageInfo _)
            {
                damageCallbackCount++;
                callbackState = coordinator.State;
                coordinator.Cancel();
            }
        }

        [Test]
        public void ExplicitCancelAtFinalizationBoundaryPreventsSnapshotAndResolution()
        {
            using EncounterFixture fixture = new();
            EncounterTerminalResolutionCoordinator coordinator = fixture.Encounter.TerminalCoordinator;
            EncounterTerminalCoordinatorState boundaryState = default;
            coordinator.SetFinalizationBoundaryForTests(current =>
            {
                boundaryState = current.State;
                current.Cancel();
            });

            CombatRootAdmissionResult admission = fixture.Encounter.AdmitCombatRoot(context =>
            {
                Assert.That(
                    context.TryApplyDamage(
                        fixture.PlayerHealth,
                        CreateDamage(fixture.PlayerHealth, DamageTeam.Enemy, 1f)),
                    Is.True);
            });

            Assert.That(boundaryState, Is.EqualTo(EncounterTerminalCoordinatorState.Finalizing));
            Assert.That(admission.Disposition, Is.EqualTo(CombatRootAdmissionDisposition.Executed));
            Assert.That(admission.CoordinatorState, Is.EqualTo(EncounterTerminalCoordinatorState.Cancelled));
            Assert.That(coordinator.ActiveRootAdmissionSequence, Is.Zero);
            Assert.That(coordinator.ActiveEpoch, Is.Zero);
            Assert.That(fixture.Encounter.HasTerminalResolution, Is.False);
            Assert.That(fixture.Encounter.HasDiagnostic, Is.False);
        }

        [Test]
        public void WrongRunAndPostTerminalContextsRejectWithoutMutatingCurrentRun()
        {
            using EncounterFixture foreignFixture = new();
            using EncounterFixture currentFixture = new();
            CanonicalCombatRootContext foreignContext = null;
            foreignFixture.Encounter.AdmitCombatRoot(context => foreignContext = context);

            float currentHealth = currentFixture.PlayerHealth.CurrentHealth;
            Assert.That(
                currentFixture.Encounter.TerminalCoordinator.TryQueueForeignContextForTests(
                    foreignContext,
                    currentFixture.PlayerHealth,
                    CreateDamage(currentFixture.PlayerHealth, DamageTeam.Enemy, 10f)),
                Is.False);
            Assert.That(currentFixture.PlayerHealth.CurrentHealth, Is.EqualTo(currentHealth));
            Assert.That(currentFixture.Encounter.IsFaulted, Is.False);

            CanonicalCombatRootContext terminalContext = null;
            currentFixture.Encounter.AdmitCombatRoot(context =>
            {
                terminalContext = context;
                Assert.That(
                    context.TryApplyDamage(
                        currentFixture.EnemyHealth,
                        CreateLethalDamage(currentFixture.EnemyHealth, DamageTeam.Player)),
                    Is.True);
            });
            Assert.That(currentFixture.Encounter.IsWon, Is.True);
            Assert.That(
                terminalContext.TryApplyDamage(
                    currentFixture.PlayerHealth,
                    CreateDamage(currentFixture.PlayerHealth, DamageTeam.Enemy, 10f)),
                Is.False);
            Assert.That(currentFixture.PlayerHealth.CurrentHealth, Is.EqualTo(currentHealth));
            Assert.That(currentFixture.Encounter.IsFaulted, Is.False);
            Assert.That(currentFixture.Encounter.HasDiagnostic, Is.False);
        }

        [Test]
        public void RepeatedNonterminalEpochsReturnIdleWithoutLeakingAuthority()
        {
            using EncounterFixture fixture = new();
            long previousRootSequence = 0;
            for (int i = 0; i < 8; i++)
            {
                CombatRootAdmissionResult admission = fixture.Encounter.AdmitCombatRoot(context =>
                {
                    Assert.That(
                        context.TryApplyDamage(
                            fixture.EnemyHealth,
                            CreateDamage(fixture.EnemyHealth, DamageTeam.Player, 1f)),
                        Is.True);
                });

                Assert.That(admission.Disposition, Is.EqualTo(CombatRootAdmissionDisposition.Executed));
                Assert.That(admission.RootAdmissionSequence, Is.GreaterThan(previousRootSequence));
                Assert.That(
                    fixture.Encounter.TerminalCoordinator.State,
                    Is.EqualTo(EncounterTerminalCoordinatorState.Idle));
                Assert.That(fixture.Encounter.TerminalCoordinator.ActiveRootAdmissionSequence, Is.Zero);
                Assert.That(fixture.Encounter.HasTerminalResolution, Is.False);
                Assert.That(fixture.Encounter.HasDiagnostic, Is.False);
                previousRootSequence = admission.RootAdmissionSequence;
            }

            Assert.That(fixture.Encounter.TerminalCoordinator.LastClosedRootAdmissionSequence, Is.EqualTo(previousRootSequence));
            Assert.That(fixture.Encounter.TerminalCoordinator.LastClosedEpoch, Is.GreaterThan(0));
            Assert.That(ApplyLethalDamage(fixture.EnemyHealth, DamageTeam.Player), Is.True);
            Assert.That(fixture.Encounter.IsWon, Is.True);
        }

        [Test]
        public void ClosedSameRunContextFaultsBeforeMutation()
        {
            using EncounterFixture fixture = new();
            CanonicalCombatRootContext retainedContext = null;

            fixture.Encounter.AdmitCombatRoot(context =>
            {
                retainedContext = context;
                Assert.That(context.TryApplyDamage(
                    fixture.PlayerHealth,
                    CreateDamage(fixture.PlayerHealth, DamageTeam.Enemy, 10f)), Is.True);
            });

            float healthAfterClosedRoot = fixture.PlayerHealth.CurrentHealth;
            Assert.That(retainedContext, Is.Not.Null);
            Assert.That(retainedContext.TryApplyDamage(
                fixture.PlayerHealth,
                CreateDamage(fixture.PlayerHealth, DamageTeam.Enemy, 10f)), Is.False);
            Assert.That(fixture.PlayerHealth.CurrentHealth, Is.EqualTo(healthAfterClosedRoot));
            Assert.That(fixture.Encounter.IsFaulted, Is.True);
            Assert.That(
                fixture.Encounter.Diagnostic.Reason,
                Is.EqualTo(EncounterTerminalDiagnosticReason.ClosedToken));
        }

        [Test]
        public void BoundHealthResetFaultsWithoutRevivingOrRewritingHealth()
        {
            using EncounterFixture fixture = new();
            Assert.That(fixture.PlayerHealth.TryApplyDamage(
                CreateDamage(fixture.PlayerHealth, DamageTeam.Enemy, 25f)), Is.True);
            float damagedHealth = fixture.PlayerHealth.CurrentHealth;

            fixture.PlayerHealth.ResetHealthToFull();

            Assert.That(fixture.PlayerHealth.CurrentHealth, Is.EqualTo(damagedHealth));
            Assert.That(fixture.Encounter.IsFaulted, Is.True);
            Assert.That(
                fixture.Encounter.Diagnostic.Reason,
                Is.EqualTo(EncounterTerminalDiagnosticReason.UnsupportedBoundMutation));
            Assert.That(fixture.Encounter.HasTerminalResolution, Is.False);
        }

        [Test]
        public void UnboundCombatHealthKeepsLegacyDamageAndResetBehavior()
        {
            GameObject owner = new("UnboundCombatHealth");
            try
            {
                CombatHealth health = CreateHealth(owner, DamageTeam.Player);
                Assert.That(health.TryApplyDamage(
                    CreateDamage(health, DamageTeam.Enemy, 25f)), Is.True);
                Assert.That(health.CurrentHealth, Is.EqualTo(75f));

                health.ResetHealthToFull();

                Assert.That(health.CurrentHealth, Is.EqualTo(100f));
                Assert.That(health.IsAlive, Is.True);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(owner);
            }
        }

        private static void RunTerminalResultTest(
            bool healthToDefeatIsEnemy,
            int expectedWinCount,
            int expectedFailureCount)
        {
            using EncounterFixture fixture = new();
            int wonCount = 0;
            int failedCount = 0;
            fixture.Encounter.Won += HandleWon;
            fixture.Encounter.Failed += HandleFailed;

            try
            {
                CombatHealth firstDefeat = healthToDefeatIsEnemy
                    ? fixture.EnemyHealth
                    : fixture.PlayerHealth;
                CombatHealth secondDefeat = healthToDefeatIsEnemy
                    ? fixture.PlayerHealth
                    : fixture.EnemyHealth;
                DamageTeam firstSourceTeam = healthToDefeatIsEnemy
                    ? DamageTeam.Player
                    : DamageTeam.Enemy;
                DamageTeam secondSourceTeam = healthToDefeatIsEnemy
                    ? DamageTeam.Enemy
                    : DamageTeam.Player;

                Assert.That(ApplyLethalDamage(firstDefeat, firstSourceTeam), Is.True);
                Assert.That(fixture.Encounter.IsRunning, Is.False);
                Assert.That(fixture.Encounter.IsWon, Is.EqualTo(healthToDefeatIsEnemy));
                Assert.That(fixture.Encounter.IsFailed, Is.EqualTo(!healthToDefeatIsEnemy));
                Assert.That(wonCount, Is.EqualTo(expectedWinCount));
                Assert.That(failedCount, Is.EqualTo(expectedFailureCount));

                Assert.That(
                    ApplyLethalDamage(secondDefeat, secondSourceTeam),
                    Is.False,
                    "A sealed result must reject a later opposing terminal mutation.");
                Assert.That(wonCount, Is.EqualTo(expectedWinCount));
                Assert.That(failedCount, Is.EqualTo(expectedFailureCount));
                Assert.That(secondDefeat.IsAlive, Is.True);
            }
            finally
            {
                fixture.Encounter.Won -= HandleWon;
                fixture.Encounter.Failed -= HandleFailed;
            }

            void HandleWon()
            {
                wonCount++;
            }

            void HandleFailed()
            {
                failedCount++;
            }
        }

        private static CombatHealth CreateHealth(GameObject owner, DamageTeam team)
        {
            CombatHealth health = owner.AddComponent<CombatHealth>();
            health.ConfigureTeam(team);
            health.ConfigureMaxHealth(100f);
            return health;
        }

        private static bool ApplyLethalDamage(CombatHealth target, DamageTeam sourceTeam)
        {
            return target.TryApplyDamage(CreateLethalDamage(target, sourceTeam));
        }

        private static DamageInfo CreateLethalDamage(CombatHealth target, DamageTeam sourceTeam)
        {
            return CreateDamage(target, sourceTeam, target.MaxHealth + 1f);
        }

        private static DamageInfo CreateDamage(
            CombatHealth target,
            DamageTeam sourceTeam,
            float amount)
        {
            return new DamageInfo(
                null,
                sourceTeam,
                amount,
                target.transform.position,
                Vector3.forward,
                0f,
                DamageResponsePolicy.DamageOnly,
                CombatControlLockPolicy.None);
        }

        private static void ResolveSameRootTerminalTrade(
            EncounterFixture fixture,
            bool playerQueuedFirst)
        {
            CombatRootAdmissionResult admission = fixture.Encounter.AdmitCombatRoot(context =>
            {
                CombatHealth firstTarget = playerQueuedFirst
                    ? fixture.PlayerHealth
                    : fixture.EnemyHealth;
                CombatHealth secondTarget = playerQueuedFirst
                    ? fixture.EnemyHealth
                    : fixture.PlayerHealth;
                DamageTeam firstSource = playerQueuedFirst
                    ? DamageTeam.Enemy
                    : DamageTeam.Player;
                DamageTeam secondSource = playerQueuedFirst
                    ? DamageTeam.Player
                    : DamageTeam.Enemy;
                Assert.That(
                    context.TryApplyDamage(firstTarget, CreateLethalDamage(firstTarget, firstSource)),
                    Is.True);
                Assert.That(
                    context.TryApplyDamage(secondTarget, CreateLethalDamage(secondTarget, secondSource)),
                    Is.True);
            });

            Assert.That(admission.Disposition, Is.EqualTo(CombatRootAdmissionDisposition.Executed));
            Assert.That(fixture.Encounter.IsWon, Is.True);
            Assert.That(
                fixture.Encounter.TerminalResolution.Reason,
                Is.EqualTo(EncounterTerminalReason.SimultaneousTerminalClear));
        }

        private sealed class EncounterFixture : IDisposable
        {
            private readonly GameObject playerObject;
            private readonly GameObject enemyObject;
            private readonly GameObject encounterObject;

            public EncounterFixture()
            {
                playerObject = new GameObject("EncounterResultPlayer");
                enemyObject = new GameObject("EncounterResultEnemy");
                encounterObject = new GameObject("EncounterResultController");
                PlayerHealth = CreateHealth(playerObject, DamageTeam.Player);
                EnemyHealth = CreateHealth(enemyObject, DamageTeam.Enemy);
                Encounter = encounterObject.AddComponent<CombatEncounterController>();
                Encounter.ConfigureTerminalResolutionPolicy(useCoordinator: true);
                Encounter.ConfigureCombatants(PlayerHealth, EnemyHealth);
                Assert.That(Encounter.TerminalCoordinator, Is.Not.Null);
            }

            public CombatHealth PlayerHealth { get; }
            public CombatHealth EnemyHealth { get; }
            public CombatEncounterController Encounter { get; }

            public void Dispose()
            {
                UnityEngine.Object.DestroyImmediate(encounterObject);
                UnityEngine.Object.DestroyImmediate(enemyObject);
                UnityEngine.Object.DestroyImmediate(playerObject);
            }
        }
    }
}
