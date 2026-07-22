using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using DimensionBrawl.Combat;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.UI;
using DimensionBrawl.UI.StageClear;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace DimensionBrawl.Tests
{
    public sealed class OlympusCourtyardProductRoutePlayModeTests
    {
        private const string StageSelectScenePath =
            "Assets/_Game/Scenes/UI/UI_StageSelect.unity";
        private const string CourtyardScenePath =
            "Assets/_Game/Scenes/OlympusCourtyardDrillStage.unity";
        private const string LobbyScenePath =
            "Assets/_Game/Scenes/UI/UI_Lobby.unity";
        private const string StageClearSceneName = "UI_StageClear";
        private const string CourtyardCatalogEntryId =
            "story_v1_courtyard_drill_route";
        private const string CourtyardPlayableStageId =
            "OLYMPUS-COURTYARD-DRILL-01";
        private const string ReplayActionId = "olympus-courtyard-drill.replay";
        private const string RetryActionId = "olympus-courtyard-drill.retry";
        private const string LobbyActionId = "olympus-courtyard-drill.to-lobby";
        private const string CleanupSceneName =
            "OlympusCourtyardProductRoutePlayModeTests_Cleanup";

        [UnityTearDown]
        public IEnumerator ResetRuntimeAndUnloadProductScenes()
        {
            Time.timeScale = 1f;
            StageRunRuntime.ResetForTests();

            Scene cleanupScene = SceneManager.GetSceneByName(CleanupSceneName);
            if (!cleanupScene.IsValid() || !cleanupScene.isLoaded)
            {
                cleanupScene = SceneManager.CreateScene(CleanupSceneName);
            }

            Assert.That(SceneManager.SetActiveScene(cleanupScene), Is.True);
            var scenesToUnload = new List<Scene>();
            for (int sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++)
            {
                Scene scene = SceneManager.GetSceneAt(sceneIndex);
                if (scene.handle != cleanupScene.handle && scene.IsValid() && scene.isLoaded)
                {
                    scenesToUnload.Add(scene);
                }
            }

            for (int sceneIndex = 0; sceneIndex < scenesToUnload.Count; sceneIndex++)
            {
                AsyncOperation unload = SceneManager.UnloadSceneAsync(scenesToUnload[sceneIndex]);
                float deadline = Time.realtimeSinceStartup + 8f;
                while (unload != null && !unload.isDone)
                {
                    Assert.Less(
                        Time.realtimeSinceStartup,
                        deadline,
                        $"Timed out unloading {scenesToUnload[sceneIndex].path}.");
                    yield return null;
                }
            }

            StageRunRuntime.ResetForTests();
            Time.timeScale = 1f;
            yield return null;
        }

        [UnityTest]
        [Timeout(30000)]
        public IEnumerator StageSelectCourtyardStartLoadsSingleSceneAndAdmitsExactRun()
        {
            var run = new CourtyardRunFixture();
            yield return StartCourtyardFromProductStageSelect(run);

            Assert.That(SceneManager.sceneCount, Is.EqualTo(1));
            Assert.That(SceneManager.GetSceneByName(StageClearSceneName).isLoaded, Is.False);
            Assert.That(run.Scene.path.Replace('\\', '/'), Is.EqualTo(CourtyardScenePath));
            Assert.That(run.Bootstrap.HasAdmittedRun, Is.True, run.Bootstrap.LastAdmissionError);
            Assert.That(run.Context, Is.SameAs(StageRunRuntime.ActiveContext));
            Assert.That(run.Context.Identity.PlayableStageId, Is.EqualTo(CourtyardPlayableStageId));
            Assert.That(run.Context.LifecycleState, Is.EqualTo(StageRunLifecycleState.StationActive));
            Assert.That(CountSceneComponents<OneRowStageRunBootstrap>(run.Scene), Is.EqualTo(1));
            Assert.That(CountSceneComponents<CombatEncounterController>(run.Scene), Is.EqualTo(1));
            AssertNoRewardOrUnlockContract(run.Context);
        }

        [UnityTest]
        [Timeout(40000)]
        public IEnumerator CourtyardClearReplayLoadsFreshOneRowRun()
        {
            var retiredRun = new CourtyardRunFixture();
            yield return StartCourtyardFromProductStageSelect(retiredRun);
            var result = new ResultFixture();
            yield return ResolveCourtyardAndWaitForAdditiveResult(
                retiredRun,
                StageRouteOutcome.Clear,
                result);

            AssertCourtyardResult(result.Presenter, StageRouteOutcome.Clear);
            Button replayButton = ReadPrivateField<Button>(result.Presenter, "retryButton");
            Assert.That(replayButton.IsInteractable(), Is.True);
            replayButton.onClick.Invoke();

            var freshRun = new CourtyardRunFixture();
            yield return WaitForFreshCourtyardRun(retiredRun, freshRun, 10f);
            AssertTerminalSelection(
                retiredRun,
                ReplayActionId,
                StageRouteActionKind.Replay,
                StageRouteOutcome.Clear,
                CourtyardPlayableStageId,
                StageUiRouteId.None,
                "OlympusCourtyardDrillStage",
                CourtyardScenePath);
            AssertFreshRun(retiredRun, freshRun);
        }

        [UnityTest]
        [Timeout(40000)]
        public IEnumerator CourtyardFailRetryLoadsFreshOneRowRun()
        {
            var retiredRun = new CourtyardRunFixture();
            yield return StartCourtyardFromProductStageSelect(retiredRun);
            var result = new ResultFixture();
            yield return ResolveCourtyardAndWaitForAdditiveResult(
                retiredRun,
                StageRouteOutcome.Fail,
                result);

            AssertCourtyardResult(result.Presenter, StageRouteOutcome.Fail);
            Button retryButton = ReadPrivateField<Button>(result.Presenter, "retryButton");
            Assert.That(retryButton.IsInteractable(), Is.True);
            retryButton.onClick.Invoke();

            var freshRun = new CourtyardRunFixture();
            yield return WaitForFreshCourtyardRun(retiredRun, freshRun, 10f);
            AssertTerminalSelection(
                retiredRun,
                RetryActionId,
                StageRouteActionKind.Retry,
                StageRouteOutcome.Fail,
                CourtyardPlayableStageId,
                StageUiRouteId.None,
                "OlympusCourtyardDrillStage",
                CourtyardScenePath);
            AssertFreshRun(retiredRun, freshRun);
        }

        [UnityTest]
        [Timeout(40000)]
        public IEnumerator CourtyardResultLobbyCleansRunAndAdditiveResultOwnership()
        {
            var retiredRun = new CourtyardRunFixture();
            yield return StartCourtyardFromProductStageSelect(retiredRun);
            var result = new ResultFixture();
            yield return ResolveCourtyardAndWaitForAdditiveResult(
                retiredRun,
                StageRouteOutcome.Clear,
                result);

            AssertCourtyardResult(result.Presenter, StageRouteOutcome.Clear);
            Button lobbyButton = ReadPrivateField<Button>(result.Presenter, "lobbyButton");
            Assert.That(lobbyButton.IsInteractable(), Is.True);
            lobbyButton.onClick.Invoke();

            yield return WaitForActiveScenePath(LobbyScenePath, 10f);
            Scene lobbyScene = SceneManager.GetActiveScene();
            Assert.That(lobbyScene.handle, Is.Not.EqualTo(retiredRun.SceneHandle));
            Assert.That(retiredRun.Scene.isLoaded, Is.False);
            Assert.That(result.Scene.isLoaded, Is.False);
            Assert.That(SceneManager.GetSceneByName(StageClearSceneName).isLoaded, Is.False);
            Assert.That(SceneManager.sceneCount, Is.EqualTo(1));
            Assert.That(StageRunRuntime.HasActiveContext, Is.False);
            Assert.That(StageRunRuntime.ActiveContext, Is.Null);
            Assert.That(retiredRun.Context.LifecycleState, Is.EqualTo(StageRunLifecycleState.Disposed));
            AssertTerminalSelection(
                retiredRun,
                LobbyActionId,
                StageRouteActionKind.UIRoute,
                StageRouteOutcome.Clear,
                string.Empty,
                StageUiRouteId.Lobby,
                "UI_Lobby",
                LobbyScenePath);
            Assert.That(Time.timeScale, Is.EqualTo(1f).Within(0.0001f));
            Component lobbyPresenter = RequireSingleSceneComponent(
                lobbyScene,
                RequireProductType("DimensionBrawl.UI.LobbyScreenPresenter"));
            Assert.That(lobbyPresenter.gameObject.activeInHierarchy, Is.True);
            Behaviour lobbyBehaviour = lobbyPresenter as Behaviour;
            Assert.That(lobbyBehaviour, Is.Not.Null);
            Assert.That(lobbyBehaviour.isActiveAndEnabled, Is.True);
            Button primaryCtaButton = ReadPrivateField<Button>(
                lobbyPresenter,
                "primaryCtaButton");
            Assert.That(primaryCtaButton, Is.Not.Null);
            Assert.That(primaryCtaButton.gameObject.activeInHierarchy, Is.True);
            Assert.That(primaryCtaButton.IsInteractable(), Is.True);
            Assert.That(CountSceneComponents<CombatEncounterController>(lobbyScene), Is.Zero);
        }

        private static IEnumerator StartCourtyardFromProductStageSelect(
            CourtyardRunFixture fixture)
        {
            Time.timeScale = 1f;
            StageRunRuntime.ResetForTests();
            EditorSceneManager.LoadSceneInPlayMode(
                StageSelectScenePath,
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            yield return null;

            Scene stageSelectScene = SceneManager.GetActiveScene();
            Assert.That(
                stageSelectScene.path.Replace('\\', '/'),
                Is.EqualTo(StageSelectScenePath));
            Component presenter = RequireSingleSceneComponent(
                stageSelectScene,
                RequireProductType("DimensionBrawl.UI.StageSelectScreenPresenter"));
            GameObject courtyardCard = FindSceneObject(stageSelectScene, "01-2_StageCard");
            Assert.That(courtyardCard, Is.Not.Null);
            Button courtyardButton = courtyardCard.GetComponent<Button>();
            Assert.That(courtyardButton, Is.Not.Null);
            Assert.That(courtyardButton.IsInteractable(), Is.True);

            courtyardButton.onClick.Invoke();
            yield return null;
            object projection = ReadProperty(presenter, "SelectedRouteProjection");
            Assert.That(projection, Is.Not.Null);
            Assert.That(ReadProperty(projection, "CatalogEntryId"),
                Is.EqualTo(CourtyardCatalogEntryId));
            Assert.That(ReadProperty(projection, "PlayableStageId"),
                Is.EqualTo(CourtyardPlayableStageId));
            Assert.That(ReadProperty(projection, "EntryScenePath"),
                Is.EqualTo(CourtyardScenePath));
            Assert.That(ReadProperty(projection, "EntrySceneName"),
                Is.EqualTo("OlympusCourtyardDrillStage"));
            Assert.That(ReadProperty(projection, "RewardPreview"), Is.Empty);
            fixture.SelectedPlayableStage =
                (PlayableStageDefinition)ReadProperty(projection, "PlayableStage");
            fixture.SelectedRouteRevision =
                (int)ReadProperty(projection, "RouteRevision");
            fixture.SelectedRouteDigest =
                (string)ReadProperty(projection, "StoredCanonicalRouteDigest");
            fixture.SelectedEntrySegmentId =
                (string)ReadProperty(projection, "EntrySegmentId");
            fixture.SelectedEntrySequenceIndex =
                (int)ReadProperty(projection, "EntrySequenceIndex");
            Assert.That(fixture.SelectedPlayableStage, Is.Not.Null);
            Assert.That(fixture.SelectedRouteDigest, Has.Length.EqualTo(64));
            StageRunResultProgressionJoinSnapshot selectionJoin =
                (StageRunResultProgressionJoinSnapshot)ReadProperty(
                    projection,
                    "ResultProgressionJoinPreflight");
            Assert.That(
                selectionJoin.RewardPlanDisposition,
                Is.EqualTo(StageResultProgressionReferenceDisposition.NotAdmittedByCurrentSchema));

            Button startButton = ReadPrivateField<Button>(presenter, "startButton");
            Assert.That(startButton, Is.Not.Null);
            Assert.That(startButton.IsInteractable(), Is.True);
            startButton.onClick.Invoke();

            yield return WaitForActiveScenePath(CourtyardScenePath, 12f);
            fixture.Scene = SceneManager.GetActiveScene();
            fixture.SceneHandle = fixture.Scene.handle;
            fixture.Bootstrap = RequireSingleSceneComponent<OneRowStageRunBootstrap>(fixture.Scene);
            float deadline = Time.realtimeSinceStartup + 6f;
            while (!fixture.Bootstrap.HasAdmittedRun
                && string.IsNullOrWhiteSpace(fixture.Bootstrap.LastAdmissionError)
                && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.That(
                fixture.Bootstrap.HasAdmittedRun,
                Is.True,
                fixture.Bootstrap.LastAdmissionError);
            fixture.Context = fixture.Bootstrap.AdmittedContext;
            Assert.That(fixture.Context, Is.Not.Null);
            Assert.That(fixture.Context, Is.SameAs(StageRunRuntime.ActiveContext));
            Assert.That(
                ReadPrivateField<PlayableStageDefinition>(
                    fixture.Bootstrap,
                    "playableStageDefinition"),
                Is.SameAs(fixture.SelectedPlayableStage));
            Assert.That(fixture.Context.Identity.RouteRevision,
                Is.EqualTo(fixture.SelectedRouteRevision));
            Assert.That(fixture.Context.Identity.RouteSnapshotDigest,
                Is.EqualTo(fixture.SelectedRouteDigest));
            Assert.That(fixture.Context.Identity.EntrySegmentId,
                Is.EqualTo(fixture.SelectedEntrySegmentId));
            Assert.That(fixture.Context.RouteSnapshot.CanonicalDigest,
                Is.EqualTo(fixture.SelectedRouteDigest));
            Assert.That(fixture.Context.RouteSnapshot.GetSegment(0).SequenceIndex,
                Is.EqualTo(fixture.SelectedEntrySequenceIndex));
        }

        private static IEnumerator ResolveCourtyardAndWaitForAdditiveResult(
            CourtyardRunFixture run,
            StageRouteOutcome outcome,
            ResultFixture result)
        {
            CombatEncounterController encounter =
                RequireSingleSceneComponent<CombatEncounterController>(run.Scene);
            OneRowStageRunResultPresenter resultPresenter =
                RequireSingleSceneComponent<OneRowStageRunResultPresenter>(run.Scene);
            CombatHealth terminalHealth = outcome == StageRouteOutcome.Clear
                ? encounter.EnemyHealth
                : encounter.PlayerHealth;
            DamageTeam sourceTeam = outcome == StageRouteOutcome.Clear
                ? DamageTeam.Player
                : DamageTeam.Enemy;

            Assert.That(encounter.IsRunning, Is.True);
            Time.timeScale = 0.37f;
            Assert.That(Time.timeScale, Is.EqualTo(0.37f).Within(0.0001f));
            Assert.That(
                terminalHealth.TryApplyDamage(new DamageInfo(
                    null,
                    sourceTeam,
                    terminalHealth.MaxHealth + 1f,
                    terminalHealth.transform.position,
                    Vector3.forward,
                    0f,
                    DamageResponsePolicy.DamageOnly,
                    CombatControlLockPolicy.None)),
                Is.True);

            float deadline = Time.realtimeSinceStartup + 12f;
            StageClearScreenPresenter presenter = null;
            Scene clearScene = default;
            while (presenter == null || !IsPresenterInteractive(presenter))
            {
                Assert.Less(
                    Time.realtimeSinceStartup,
                    deadline,
                    "Timed out waiting for the additive Courtyard result surface.");
                clearScene = SceneManager.GetSceneByName(StageClearSceneName);
                if (clearScene.IsValid()
                    && clearScene.isLoaded
                    && CountSceneComponents<StageClearScreenPresenter>(clearScene) == 1)
                {
                    presenter = RequireSingleSceneComponent<StageClearScreenPresenter>(clearScene);
                }

                yield return null;
            }

            result.Scene = clearScene;
            result.Presenter = presenter;
            result.Summary = resultPresenter.CommittedSummary;
            Assert.That(result.Scene.handle, Is.Not.EqualTo(run.SceneHandle));
            Assert.That(result.Scene.isLoaded, Is.True);
            Assert.That(run.Scene.isLoaded, Is.True);
            Assert.That(
                SceneManager.GetActiveScene().handle == run.SceneHandle,
                Is.True,
                "The additive result surface must not steal active-scene ownership.");
            Assert.That(SceneManager.sceneCount, Is.EqualTo(2));
            Assert.That(result.Summary, Is.Not.Null, resultPresenter.LastCommitError);
            Assert.That(result.Summary.Outcome, Is.EqualTo(outcome));
            Assert.That(run.Context.LifecycleState, Is.EqualTo(StageRunLifecycleState.Presented));
            Assert.That(StageRunRuntime.ActiveContext, Is.SameAs(run.Context));
            Assert.That(Time.timeScale, Is.Zero);
            AssertNoRewardOrUnlockContract(run.Context);
        }

        private static IEnumerator WaitForFreshCourtyardRun(
            CourtyardRunFixture retiredRun,
            CourtyardRunFixture freshRun,
            float timeoutSeconds)
        {
            float deadline = Time.realtimeSinceStartup + timeoutSeconds;
            while (Time.realtimeSinceStartup < deadline)
            {
                Scene active = SceneManager.GetActiveScene();
                if (active.IsValid()
                    && active.isLoaded
                    && active.handle != retiredRun.SceneHandle
                    && string.Equals(
                        active.path.Replace('\\', '/'),
                        CourtyardScenePath,
                        StringComparison.Ordinal)
                    && CountSceneComponents<OneRowStageRunBootstrap>(active) == 1)
                {
                    OneRowStageRunBootstrap bootstrap =
                        RequireSingleSceneComponent<OneRowStageRunBootstrap>(active);
                    if (bootstrap.HasAdmittedRun)
                    {
                        freshRun.Scene = active;
                        freshRun.SceneHandle = active.handle;
                        freshRun.Bootstrap = bootstrap;
                        freshRun.Context = bootstrap.AdmittedContext;
                        yield break;
                    }

                    if (!string.IsNullOrWhiteSpace(bootstrap.LastAdmissionError))
                    {
                        Assert.Fail(bootstrap.LastAdmissionError);
                    }
                }

                yield return null;
            }

            Assert.Fail("Timed out waiting for a freshly admitted Courtyard run.");
        }

        private static void AssertFreshRun(
            CourtyardRunFixture retiredRun,
            CourtyardRunFixture freshRun)
        {
            Assert.That(retiredRun.Scene.isLoaded, Is.False);
            Assert.That(freshRun.SceneHandle, Is.Not.EqualTo(retiredRun.SceneHandle));
            Assert.That(freshRun.Context, Is.Not.Null);
            Assert.That(freshRun.Context, Is.SameAs(StageRunRuntime.ActiveContext));
            Assert.That(freshRun.Context, Is.Not.SameAs(retiredRun.Context));
            Assert.That(
                freshRun.Context.Identity.RunId,
                Is.Not.EqualTo(retiredRun.Context.Identity.RunId));
            Assert.That(
                freshRun.Context.Identity.PlayableStageId,
                Is.EqualTo(CourtyardPlayableStageId));
            Assert.That(
                freshRun.Context.LifecycleState,
                Is.EqualTo(StageRunLifecycleState.StationActive));
            Assert.That(
                retiredRun.Context.LifecycleState,
                Is.EqualTo(StageRunLifecycleState.Disposed));
            Assert.That(SceneManager.GetSceneByName(StageClearSceneName).isLoaded, Is.False);
            Assert.That(SceneManager.sceneCount, Is.EqualTo(1));
            Assert.That(Time.timeScale, Is.EqualTo(1f).Within(0.0001f));

            CombatEncounterController encounter =
                RequireSingleSceneComponent<CombatEncounterController>(freshRun.Scene);
            Assert.That(encounter.IsRunning, Is.True);
            Assert.That(encounter.HasTerminalResolution, Is.False);
            Assert.That(encounter.PlayerHealth.IsAlive, Is.True);
            Assert.That(encounter.EnemyHealth.IsAlive, Is.True);
            AssertNoRewardOrUnlockContract(freshRun.Context);
        }

        private static void AssertTerminalSelection(
            CourtyardRunFixture retiredRun,
            string expectedActionId,
            StageRouteActionKind expectedActionKind,
            StageRouteOutcome expectedOutcome,
            string expectedPlayableStageId,
            StageUiRouteId expectedUiRouteId,
            string expectedSceneName,
            string expectedScenePath)
        {
            StageRunResolvedTerminalAction selection =
                retiredRun.Context.SelectedTerminalAction;
            Assert.That(selection, Is.Not.Null);
            Assert.That(selection.RunId, Is.EqualTo(retiredRun.Context.Identity.RunId));
            Assert.That(selection.RouteDigest,
                Is.EqualTo(retiredRun.Context.Identity.RouteSnapshotDigest));
            Assert.That(selection.ResultSummaryDigest,
                Is.EqualTo(retiredRun.Context.CommittedSummary.ResultSummaryDigest));
            Assert.That(selection.ActionId, Is.EqualTo(expectedActionId));
            Assert.That(selection.ActionKind, Is.EqualTo(expectedActionKind));
            Assert.That(selection.Outcome, Is.EqualTo(expectedOutcome));
            Assert.That(selection.TargetPlayableStageId, Is.EqualTo(expectedPlayableStageId));
            Assert.That(selection.TargetUiRouteId, Is.EqualTo(expectedUiRouteId));
            Assert.That(selection.DestinationSceneName, Is.EqualTo(expectedSceneName));
            Assert.That(selection.DestinationScenePath, Is.EqualTo(expectedScenePath));
            Assert.That(selection.SelectionId,
                Is.EqualTo($"{retiredRun.Context.Identity.RunId}:terminal-action:1"));
            Assert.That(selection.CanonicalDigest, Has.Length.EqualTo(64));
        }

        private static void AssertCourtyardResult(
            StageClearScreenPresenter presenter,
            StageRouteOutcome outcome)
        {
            Assert.That(presenter.IsConfigured, Is.True, presenter.LastActionError);
            Assert.That(presenter.ResultSummary, Is.Not.Null);
            Assert.That(presenter.ResultSummary.Outcome, Is.EqualTo(outcome));
            Assert.That(presenter.ResultSummary.Identity.PlayableStageId,
                Is.EqualTo(CourtyardPlayableStageId));
            Assert.That(presenter.ResultSummary.SegmentResultCount, Is.EqualTo(1));
            Assert.That(presenter.ResultSummary.OfferedActionCount, Is.EqualTo(2));
            Assert.That(
                presenter.PrimaryActionId,
                Is.EqualTo(outcome == StageRouteOutcome.Clear
                    ? ReplayActionId
                    : RetryActionId));
            Assert.That(presenter.LobbyActionId, Is.EqualTo(LobbyActionId));
            Assert.That(presenter.PresentationSnapshot, Is.Not.Null);
            Assert.That(
                presenter.PresentationSnapshot.PlayableStageId,
                Is.EqualTo(CourtyardPlayableStageId));
            Assert.That(presenter.PresentationSnapshot.Outcome, Is.EqualTo(outcome));
            Assert.That(
                presenter.PresentationSnapshot.ProfileId,
                Is.EqualTo("stage-result.olympus-courtyard-drill"));
        }

        private static void AssertNoRewardOrUnlockContract(StageRunContext context)
        {
            StageRunResultProgressionJoinSnapshot join =
                context.ResultProgressionJoinSnapshot;
            Assert.That(join, Is.Not.Null);
            Assert.That(
                join.RewardPlanDisposition,
                Is.EqualTo(StageResultProgressionReferenceDisposition.NotAdmittedByCurrentSchema));
            Assert.That(join.RewardPlanDigest, Is.Empty);

            StageProgressionNodeSnapshot node = join.ProgressionNode;
            Assert.That(node, Is.Not.Null);
            Assert.That(node.PrerequisiteCount, Is.Zero);
            Assert.That(node.RecommendedNextCount, Is.Zero);
            Assert.That(
                node.AfterClearScriptDisposition,
                Is.EqualTo(StageResultProgressionReferenceDisposition.NotAuthoredForCurrentSchema));
            Assert.That(node.AfterClearScriptId, Is.Empty);
            Assert.That(
                node.RewardPlanDisposition,
                Is.EqualTo(StageResultProgressionReferenceDisposition.NotAdmittedByCurrentSchema));
            Assert.That(node.RewardPlanId, Is.Empty);
            Assert.That(node.RewardPlanRevision, Is.Zero);
            Assert.That(node.RewardPlanDigest, Is.Empty);
        }

        private static IEnumerator WaitForActiveScenePath(
            string expectedPath,
            float timeoutSeconds)
        {
            float deadline = Time.realtimeSinceStartup + timeoutSeconds;
            while (!string.Equals(
                SceneManager.GetActiveScene().path.Replace('\\', '/'),
                expectedPath,
                StringComparison.Ordinal))
            {
                Assert.Less(
                    Time.realtimeSinceStartup,
                    deadline,
                    $"Timed out waiting for active scene {expectedPath}.");
                yield return null;
            }

            yield return null;
            yield return null;
        }

        private static bool IsPresenterInteractive(StageClearScreenPresenter presenter)
        {
            if (presenter == null || !presenter.isActiveAndEnabled || !presenter.IsConfigured)
            {
                return false;
            }

            CanvasGroup canvasGroup = ReadPrivateField<CanvasGroup>(presenter, "canvasGroup");
            return canvasGroup != null
                && canvasGroup.interactable
                && canvasGroup.blocksRaycasts;
        }

        private static GameObject FindSceneObject(Scene scene, string objectName)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
            {
                Transform[] transforms = roots[rootIndex].GetComponentsInChildren<Transform>(true);
                for (int transformIndex = 0; transformIndex < transforms.Length; transformIndex++)
                {
                    if (transforms[transformIndex].name == objectName)
                    {
                        return transforms[transformIndex].gameObject;
                    }
                }
            }

            return null;
        }

        private static int CountSceneComponents<T>(Scene scene)
            where T : Component
        {
            int count = 0;
            GameObject[] roots = scene.GetRootGameObjects();
            for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
            {
                count += roots[rootIndex].GetComponentsInChildren<T>(true).Length;
            }

            return count;
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
                    Assert.That(found, Is.Null,
                        $"{scene.path} owns duplicate {typeof(T).Name} components.");
                    found = components[componentIndex];
                }
            }

            Assert.That(found, Is.Not.Null, $"{scene.path} is missing {typeof(T).Name}.");
            return found;
        }

        private static Component RequireSingleSceneComponent(Scene scene, Type componentType)
        {
            Component found = null;
            GameObject[] roots = scene.GetRootGameObjects();
            for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
            {
                Component[] components = roots[rootIndex].GetComponentsInChildren(
                    componentType,
                    true);
                for (int componentIndex = 0; componentIndex < components.Length; componentIndex++)
                {
                    Assert.That(found, Is.Null,
                        $"{scene.path} owns duplicate {componentType.Name} components.");
                    found = components[componentIndex];
                }
            }

            Assert.That(found, Is.Not.Null, $"{scene.path} is missing {componentType.Name}.");
            return found;
        }

        private static Type RequireProductType(string fullName)
        {
            Type type = Type.GetType(fullName + ", DimensionBrawl.Runtime")
                ?? Type.GetType(fullName + ", Assembly-CSharp");
            Assert.That(type, Is.Not.Null, $"Missing product type {fullName}.");
            return type;
        }

        private static object ReadProperty(object target, string propertyName)
        {
            Assert.That(target, Is.Not.Null);
            PropertyInfo property = target.GetType().GetProperty(
                propertyName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.That(property, Is.Not.Null,
                $"{target.GetType().FullName}.{propertyName} was not found.");
            return property.GetValue(target);
        }

        private static T ReadPrivateField<T>(object target, string fieldName)
        {
            Assert.That(target, Is.Not.Null);
            FieldInfo field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null,
                $"{target.GetType().FullName}.{fieldName} was not found.");
            return (T)field.GetValue(target);
        }

        private sealed class CourtyardRunFixture
        {
            public Scene Scene;
            public int SceneHandle;
            public OneRowStageRunBootstrap Bootstrap;
            public StageRunContext Context;
            public PlayableStageDefinition SelectedPlayableStage;
            public int SelectedRouteRevision;
            public string SelectedRouteDigest;
            public string SelectedEntrySegmentId;
            public int SelectedEntrySequenceIndex;
        }

        private sealed class ResultFixture
        {
            public Scene Scene;
            public StageClearScreenPresenter Presenter;
            public StageRunResultSummary Summary;
        }
    }
}
