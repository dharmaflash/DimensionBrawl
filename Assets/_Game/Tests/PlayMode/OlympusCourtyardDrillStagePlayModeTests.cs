using System;
using System.Collections;
using System.Collections.Generic;
using DimensionBrawl.Combat;
using DimensionBrawl.LevelDesign;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace DimensionBrawl.Tests
{
    public sealed class OlympusCourtyardDrillStagePlayModeTests
    {
        private const string ScenePath =
            "Assets/_Game/Scenes/OlympusCourtyardDrillStage.unity";
        private const string PlayableStageId = "OLYMPUS-COURTYARD-DRILL-01";
        private const string SegmentId = "courtyard_drill_combat";
        private const string TerminalConditionId = "courtyard-drill.encounter.terminal";

        [UnitySetUp]
        public IEnumerator LoadIsolatedStage()
        {
            Time.timeScale = 1f;
            StageRunRuntime.ResetForTests();
            EditorSceneManager.LoadSceneInPlayMode(
                ScenePath,
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            yield return null;

            OneRowStageRunBootstrap bootstrap =
                RequireSingleSceneComponent<OneRowStageRunBootstrap>();
            float deadline = Time.realtimeSinceStartup + 4f;
            while (!bootstrap.HasAdmittedRun
                && string.IsNullOrWhiteSpace(bootstrap.LastAdmissionError)
                && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.That(bootstrap.HasAdmittedRun, Is.True, bootstrap.LastAdmissionError);
        }

        [UnityTearDown]
        public IEnumerator ResetRuntime()
        {
            StageRunRuntime.ResetForTests();
            Time.timeScale = 1f;
            yield return null;
        }

        [UnityTest]
        public IEnumerator DirectEditorLoadAdmitsExactAuthoredOneRowContext()
        {
            OneRowStageRunBootstrap bootstrap =
                RequireSingleSceneComponent<OneRowStageRunBootstrap>();
            CombatEncounterController encounter =
                RequireSingleSceneComponent<CombatEncounterController>();
            OneRowStageRunFactAdapter factAdapter =
                RequireSingleSceneComponent<OneRowStageRunFactAdapter>();
            OneRowStageRunResultPresenter presenter =
                RequireSingleSceneComponent<OneRowStageRunResultPresenter>();
            StageCountOneEncounterExecutor addExecutor =
                RequireSingleSceneComponent<StageCountOneEncounterExecutor>();

            StageRunContext context = bootstrap.AdmittedContext;
            Assert.That(context, Is.Not.Null);
            Assert.That(context, Is.SameAs(StageRunRuntime.ActiveContext));
            Assert.That(context.Identity.PlayableStageId, Is.EqualTo(PlayableStageId));
            Assert.That(context.RouteSnapshot.SegmentCount, Is.EqualTo(1));
            Assert.That(context.CurrentSegmentIndex, Is.Zero);
            Assert.That(context.CurrentSegmentRoles,
                Is.EqualTo(StageRunSegmentRole.Entry | StageRunSegmentRole.Terminal));
            Assert.That(context.CurrentSegment.SegmentId, Is.EqualTo(SegmentId));
            Assert.That(context.CurrentSegment.ExitConditionId, Is.EqualTo(TerminalConditionId));
            Assert.That(context.TutorialFactRequirement,
                Is.EqualTo(StageRunTutorialFactRequirement.None));
            Assert.That(context.PendingHandoffToken, Is.Null);
            Assert.That(context.SegmentEntryReceipt, Is.Null);
            Assert.That(context.HandoffTerminalReceipt, Is.Null);
            Assert.That(encounter.UsesCoordinatedTerminalResolution, Is.True);
            Assert.That(encounter.PlayerHealth, Is.Not.Null);
            Assert.That(encounter.EnemyHealth, Is.Not.Null);
            Assert.That(encounter.PlayerHealth, Is.Not.SameAs(encounter.EnemyHealth));
            Assert.That(factAdapter.IsBound, Is.True, factAdapter.LastFactError);
            Assert.That(presenter.HasCanonicalStageRun, Is.True,
                presenter.CanonicalStageRunBindingError);
            Assert.That(addExecutor.ActivationKind,
                Is.EqualTo(StageEncounterActivationKind.SceneReady));
            Assert.That(addExecutor.TicketCount, Is.EqualTo(1));

            float deadline = Time.realtimeSinceStartup + 4f;
            while (addExecutor.ActivatedTicketCount == 0
                && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.That(addExecutor.ActivatedTicketCount, Is.EqualTo(1), addExecutor.LastError);
            StageAddEncounterTicketSnapshot ticket = addExecutor.GetTicketSnapshot(0);
            Assert.That(ticket.PayloadId, Is.EqualTo("SciFiSoldier.Ranged"));
            Assert.That(ticket.Health, Is.Not.Null);
            Assert.That(ticket.Health, Is.Not.SameAs(encounter.EnemyHealth),
                "The Rifle Crossfire Add must never masquerade as the terminal boss subject.");
        }

        [UnityTest]
        [Timeout(20000)]
        public IEnumerator RifleCrossfireAddDealsRealPlayerDamageBeforeTerminal()
        {
            CombatEncounterController encounter =
                RequireSingleSceneComponent<CombatEncounterController>();
            StageCountOneEncounterExecutor addExecutor =
                RequireSingleSceneComponent<StageCountOneEncounterExecutor>();
            CombatHealth player = encounter.PlayerHealth;
            float startingHealth = player.CurrentHealth;

            float deadline = Time.realtimeSinceStartup + 12f;
            while (Mathf.Approximately(player.CurrentHealth, startingHealth)
                && player.IsAlive
                && encounter.IsRunning
                && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.That(addExecutor.ActivatedTicketCount, Is.EqualTo(1), addExecutor.LastError);
            Assert.That(player.CurrentHealth, Is.LessThan(startingHealth),
                "The reviewed Rifle Crossfire Add never produced real hostile damage.");
            Assert.That(encounter.HasTerminalResolution, Is.False,
                "An independent Add hit must not own the stage terminal outcome.");
        }

        [UnityTest]
        [Timeout(20000)]
        public IEnumerator AuthoredBossClearCommitsOneTruthfulSegmentWithoutTutorialOrHandoff()
        {
            CombatEncounterController encounter =
                RequireSingleSceneComponent<CombatEncounterController>();
            OneRowStageRunResultPresenter presenter =
                RequireSingleSceneComponent<OneRowStageRunResultPresenter>();
            StageCountOneEncounterExecutor addExecutor =
                RequireSingleSceneComponent<StageCountOneEncounterExecutor>();

            Assert.That(ApplyLethalDamage(encounter.EnemyHealth, DamageTeam.Player), Is.True);
            yield return WaitForCommittedSummary(presenter, 8f);

            StageRunResultSummary summary = presenter.CommittedSummary;
            Assert.That(summary, Is.Not.Null, presenter.LastCommitError);
            Assert.That(summary.Outcome, Is.EqualTo(StageRouteOutcome.Clear));
            Assert.That(summary.SegmentResultCount, Is.EqualTo(1));
            Assert.That(summary.GetSegmentResult(0).SegmentId, Is.EqualTo(SegmentId));
            Assert.That(summary.HasTutorialRouteSummaryFact, Is.False);
            Assert.That(summary.RouteSnapshot.SegmentCount, Is.EqualTo(1));
            Assert.That(summary.OfferedActionCount, Is.EqualTo(2));
            Assert.That(addExecutor.State,
                Is.EqualTo(StageCountOneEncounterState.Cancelled),
                "Terminal boss clear must synchronously cancel the independent Add plan.");
            Assert.That(addExecutor.IsQuiescent, Is.True);
        }

        [UnityTest]
        [Timeout(20000)]
        public IEnumerator AuthoredPlayerDownCommitsFailAndKeepsRetryLobbyOnly()
        {
            CombatEncounterController encounter =
                RequireSingleSceneComponent<CombatEncounterController>();
            OneRowStageRunResultPresenter presenter =
                RequireSingleSceneComponent<OneRowStageRunResultPresenter>();
            StageCountOneEncounterExecutor addExecutor =
                RequireSingleSceneComponent<StageCountOneEncounterExecutor>();

            Assert.That(ApplyLethalDamage(encounter.PlayerHealth, DamageTeam.Enemy), Is.True);
            yield return WaitForCommittedSummary(presenter, 8f);

            StageRunResultSummary summary = presenter.CommittedSummary;
            Assert.That(summary, Is.Not.Null, presenter.LastCommitError);
            Assert.That(summary.Outcome, Is.EqualTo(StageRouteOutcome.Fail));
            Assert.That(summary.SegmentResultCount, Is.EqualTo(1));
            Assert.That(summary.HasTutorialRouteSummaryFact, Is.False);
            Assert.That(summary.OfferedActionCount, Is.EqualTo(2));
            Assert.That(addExecutor.State,
                Is.EqualTo(StageCountOneEncounterState.Cancelled));
            Assert.That(addExecutor.IsQuiescent, Is.True);
        }

        private static IEnumerator WaitForCommittedSummary(
            OneRowStageRunResultPresenter presenter,
            float timeoutSeconds)
        {
            float deadline = Time.realtimeSinceStartup + timeoutSeconds;
            while (presenter.CommittedSummary == null
                && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }
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

        private static T RequireSingleSceneComponent<T>()
            where T : Component
        {
            Scene scene = SceneManager.GetActiveScene();
            var found = new List<T>();
            GameObject[] roots = scene.GetRootGameObjects();
            for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
            {
                found.AddRange(roots[rootIndex].GetComponentsInChildren<T>(true));
            }

            Assert.That(found.Count, Is.EqualTo(1),
                $"Expected exactly one {typeof(T).Name} in '{scene.path}'.");
            return found[0];
        }
    }
}
