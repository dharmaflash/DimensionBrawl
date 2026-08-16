using System;
using System.Collections;
using System.IO;
using System.Linq;
using DimensionBrawl.Combat;
using DimensionBrawl.Player;
using DimensionBrawl.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;

namespace DimensionBrawl.Editor.AuditionPV.Tests
{
    public sealed class AuditionPvStationBossDeathAftermathGoldenTests
    {
        private const string ShaA =
            "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        private const string ShaB =
            "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";

        [Test]
        public void Contract_IsExactSixSecondG08WithThreeByteExactBaselines()
        {
            Assert.That(AuditionPvStationBossDeathAftermathCapture.ShotId,
                Is.EqualTo("g08"));
            Assert.That(AuditionPvStationBossDeathAftermathCapture.FirstFrame,
                Is.Zero);
            Assert.That(AuditionPvStationBossDeathAftermathCapture.LastFrame,
                Is.EqualTo(359));
            Assert.That(AuditionPvStationBossDeathAftermathCapture.ExpectedFrameCount,
                Is.EqualTo(360));
            Assert.That(AuditionPvStationBossDeathAftermathCapture.FireFrame,
                Is.EqualTo(1));
            Assert.That(AuditionPvStationBossDeathAftermathCapture.ImpactFrame,
                Is.EqualTo(62));
            Assert.That(AuditionPvStationBossDeathAftermathCapture.AftermathHeroFrame,
                Is.EqualTo(116));
            Assert.That(AuditionPvStationBossDeathAftermathCapture.FinisherStabilityFrame,
                Is.EqualTo(181));
            Assert.That(AuditionPvStationBossDeathAftermathCapture.ResultRequestFrame,
                Is.EqualTo(218));
            Assert.That(AuditionPvStationBossDeathAftermathCapture.InteractiveResultFrame,
                Is.EqualTo(246));
            Assert.That(AuditionPvStationBossDeathAftermathCapture.PreparedBossHealth,
                Is.EqualTo(12f));
            Assert.That(AuditionPvStationBossDeathAftermathCapture.AuthoredProjectileDamage,
                Is.EqualTo(12f));
            Assert.That(AuditionPvStationBossDeathAftermathCapture.AuthoredProjectileSpeed,
                Is.EqualTo(24f));
            Assert.That(AuditionPvStationBossDeathAftermathCapture.AuthoredProjectileRadius,
                Is.EqualTo(0.31f));
            Assert.That(
                AuditionPvStationBossDeathAftermathCapture
                    .PlayerRangedProjectilePrefabPath,
                Is.EqualTo(
                    "Assets/_Game/Prefabs/Combat/PF_PlayerRangedBasicProjectile_AimBolt.prefab"));
            Assert.That(
                AuditionPvStationBossDeathAftermathCapture
                    .PlayerRangedProjectilePrefabGuid,
                Is.EqualTo("404ed7d823e769c45871b221fe7e3c95"));
            GameObject projectilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Game/Prefabs/Combat/PF_PlayerRangedBasicProjectile_AimBolt.prefab");
            Assert.That(projectilePrefab, Is.Not.Null);
            Assert.That(AssetDatabase.GetAssetPath(projectilePrefab), Is.EqualTo(
                "Assets/_Game/Prefabs/Combat/PF_PlayerRangedBasicProjectile_AimBolt.prefab"));
            Assert.That(AssetDatabase.AssetPathToGUID(
                AssetDatabase.GetAssetPath(projectilePrefab)), Is.EqualTo(
                "404ed7d823e769c45871b221fe7e3c95"));
            Assert.That(projectilePrefab.transform.localScale,
                Is.EqualTo(new Vector3(0.28f, 0.28f, 0.28f)));
            LaneActionProjectile projectile = projectilePrefab
                .GetComponent<LaneActionProjectile>();
            SphereCollider projectileCollider = projectilePrefab
                .GetComponent<SphereCollider>();
            Assert.That(projectile, Is.Not.Null);
            Assert.That(projectileCollider, Is.Not.Null);
            Assert.That(projectile.gameObject, Is.SameAs(projectilePrefab));
            Assert.That(projectileCollider.gameObject, Is.SameAs(projectilePrefab));
            Assert.That(typeof(PlayerRangedBasicAttackAction).GetProperty(
                "ConfiguredProjectilePrefab"), Is.Not.Null);
            Assert.That(typeof(PlayerRangedBasicAttackAction).GetProperty(
                "ConfiguredProjectileRadius"), Is.Not.Null);
            Assert.That(
                AuditionPvStationBossDeathAftermathCapture
                    .ResolveConfiguredProjectileWorldRadius(
                        0.31f,
                        projectilePrefab.transform.localScale,
                        Vector3.one),
                Is.EqualTo(0.0868f).Within(0.000001f));
            Assert.That(
                AuditionPvStationBossDeathAftermathCapture
                    .AuthoredProjectileWorldRadius,
                Is.EqualTo(0.0868f));
            Assert.That(
                AuditionPvStationBossDeathAftermathCapture.PredictNaturalImpactFrame(
                    AuditionPvStationBossDeathAftermathCapture
                        .NaturalImpactTargetDistance),
                Is.EqualTo(62));
            Assert.That(AuditionPvStationBossDeathAftermathCapture.FrameTimeSeconds(359),
                Is.EqualTo(359f / 60f).Within(0.000001f));
            Assert.That(AuditionPvStationBossDeathAftermathGoldenRunner.RawWarmupFrame,
                Is.Zero);
            Assert.That(AuditionPvStationBossDeathAftermathGoldenRunner.RawFirstShotFrame,
                Is.EqualTo(1));
            Assert.That(AuditionPvStationBossDeathAftermathGoldenRunner.RawLastShotFrame,
                Is.EqualTo(360));
            Assert.That(AuditionPvStationBossDeathAftermathGoldenRunner.ExpectedRawFrameCount,
                Is.EqualTo(361));

            AuditionPvShotManifestEntry shot =
                AuditionPvStationBossDeathAftermathCapture.CreateShotManifestEntry();
            Assert.That(shot.scenePath,
                Is.EqualTo(AuditionPvStationBossDeathAftermathCapture.StationScenePath));
            Assert.That(shot.hudMode, Is.EqualTo("hud-on-to-result"));
            Assert.That(shot.notes, Does.Contain("natural"));
            Assert.That(shot.notes, Does.Contain("f62"));
            Assert.That(shot.notes, Does.Contain("f218"));
            Assert.That(shot.notes, Does.Contain("f246"));

            AuditionPvBaselineManifestEntry[] baselines =
                AuditionPvStationBossDeathAftermathCapture
                    .CreateBaselineManifestEntries();
            Assert.That(baselines.Select(value => value.id),
                Is.EqualTo(new[] { "bl10", "bl11", "bl12" }));
            Assert.That(baselines.Select(value => value.sourceFrame),
                Is.EqualTo(new[] { 62, 116, 246 }));
            Assert.That(baselines.Select(value => value.hudMode),
                Is.EqualTo(new[] { "hud-on", "hud-off", "authored-result" }));
            Assert.That(baselines[1].fileName, Does.Contain("__HUDOFF__"));
            Assert.That(baselines[2].fileName, Does.Contain("__AUTHOREDRESULT__"));
            Assert.That(baselines.All(value => value.status == "captured"), Is.True);
        }

        [Test]
        public void FinisherAssets_HaveExactTransformCurvesAndSingleHeldTimelineClip()
        {
            const string AnimationPath =
                "Assets/_Game/DesignData/Timelines/Cinematics/DB_Anim_OlympusStationBossTerminalFinisherCamera.anim";
            const string TimelinePath =
                "Assets/_Game/DesignData/Timelines/Cinematics/DB_Timeline_OlympusStationBossTerminalFinisher.playable";
            AnimationClip animation = AssetDatabase.LoadAssetAtPath<AnimationClip>(
                AnimationPath);
            TimelineAsset timeline = AssetDatabase.LoadAssetAtPath<TimelineAsset>(
                TimelinePath);
            Assert.That(animation, Is.Not.Null);
            Assert.That(timeline, Is.Not.Null);

            EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(animation);
            string[] expectedProperties =
            {
                "m_LocalPosition.x", "m_LocalPosition.y", "m_LocalPosition.z",
                "m_LocalRotation.x", "m_LocalRotation.y", "m_LocalRotation.z",
                "m_LocalRotation.w"
            };
            Assert.That(bindings.Length, Is.EqualTo(7));
            Assert.That(bindings.All(value => value.type == typeof(Transform)), Is.True);
            Assert.That(bindings.All(value => string.IsNullOrEmpty(value.path)), Is.True);
            Assert.That(
                bindings.Select(value => value.propertyName)
                    .OrderBy(value => value, StringComparer.Ordinal),
                Is.EqualTo(expectedProperties.OrderBy(
                    value => value,
                    StringComparer.Ordinal)));
            Assert.That(animation.frameRate, Is.EqualTo(60f).Within(0.0001f));
            Assert.That(animation.length, Is.EqualTo(2.6f).Within(0.0001f));

            TrackAsset[] rootTracks = timeline.GetRootTracks().ToArray();
            Assert.That(rootTracks.Length, Is.EqualTo(1));
            AnimationTrack track = rootTracks.Single() as AnimationTrack;
            Assert.That(track, Is.Not.Null);
            TimelineClip[] clips = track.GetClips().ToArray();
            Assert.That(clips.Length, Is.EqualTo(1));
            TimelineClip timelineClip = clips.Single();
            AnimationPlayableAsset playable =
                timelineClip.asset as AnimationPlayableAsset;
            Assert.That(playable, Is.Not.Null);
            Assert.That(playable.clip, Is.SameAs(animation));
            Assert.That(timeline.durationMode,
                Is.EqualTo(TimelineAsset.DurationMode.FixedLength));
            Assert.That(timeline.fixedDuration, Is.EqualTo(2.6d).Within(0.0001d));
            Assert.That(timeline.duration, Is.EqualTo(2.6d).Within(0.0001d));
            Assert.That(timeline.editorSettings.frameRate,
                Is.EqualTo(60d).Within(0.0001d));
            Assert.That(timelineClip.start, Is.Zero.Within(0.0001d));
            Assert.That(timelineClip.clipIn, Is.Zero.Within(0.0001d));
            Assert.That(timelineClip.duration, Is.EqualTo(2.6d).Within(0.0001d));
            Assert.That(timelineClip.timeScale, Is.EqualTo(1d).Within(0.0001d));
            Assert.That(timelineClip.preExtrapolationMode,
                Is.EqualTo(TimelineClip.ClipExtrapolation.None));
            Assert.That(timelineClip.postExtrapolationMode,
                Is.EqualTo(TimelineClip.ClipExtrapolation.Hold));
        }

        [Test]
        public void StationPlayerComposition_UsesTheSingleActiveRenderedAnimatorRoot()
        {
            Scene previousActiveScene = SceneManager.GetActiveScene();
            Scene station = SceneManager.GetSceneByPath(
                AuditionPvStationBossDeathAftermathCapture.StationScenePath);
            bool openedByTest = !station.IsValid() || !station.isLoaded;

            try
            {
                if (openedByTest)
                {
                    station = EditorSceneManager.OpenScene(
                        AuditionPvStationBossDeathAftermathCapture.StationScenePath,
                        OpenSceneMode.Additive);
                }

                PlayerMovementController movement = station.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<
                        PlayerMovementController>(true))
                    .Single();
                Animator[] activeRenderedAnimators = movement
                    .GetComponentsInChildren<Animator>(true)
                    .Where(value => value != null
                        && value.gameObject.activeInHierarchy
                        && value.GetComponentsInChildren<SkinnedMeshRenderer>(false)
                            .Any(renderer => renderer != null
                                && renderer.enabled
                                && renderer.gameObject.activeInHierarchy))
                    .ToArray();

                Assert.That(activeRenderedAnimators.Length, Is.EqualTo(1));
                Assert.That(
                    activeRenderedAnimators[0].gameObject.name,
                    Is.EqualTo("BossBarrageLaneReview_RangedModel_Inori"));
            }
            finally
            {
                if (openedByTest && station.IsValid() && station.isLoaded)
                {
                    EditorSceneManager.CloseScene(station, removeScene: true);
                }

                if (previousActiveScene.IsValid() && previousActiveScene.isLoaded)
                {
                    SceneManager.SetActiveScene(previousActiveScene);
                }
            }
        }

        [Test]
        public void StationBossCoreComposition_BindsOnlyExactBodyMeshesAndAuthoredAxis()
        {
            Scene previousActiveScene = SceneManager.GetActiveScene();
            Scene station = SceneManager.GetSceneByPath(
                AuditionPvStationBossDeathAftermathCapture.StationScenePath);
            bool openedByTest = !station.IsValid() || !station.isLoaded;

            try
            {
                if (openedByTest)
                {
                    station = EditorSceneManager.OpenScene(
                        AuditionPvStationBossDeathAftermathCapture.StationScenePath,
                        OpenSceneMode.Additive);
                }

                AkazaPhase2CombatMotionDriver motion = station.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<
                        AkazaPhase2CombatMotionDriver>(true))
                    .Single();
                Assert.That(motion.Animator, Is.Not.Null);
                SkinnedMeshRenderer[] allBossRenderers = motion.Animator
                    .GetComponentsInChildren<SkinnedMeshRenderer>(true);
                SkinnedMeshRenderer[] core =
                    AuditionPvStationBossDeathAftermathDirector
                        .ResolveExactBossCoreBodyRenderers(
                            motion.Animator.transform);

                Assert.That(core.Select(value => value.gameObject.name),
                    Is.EqualTo(new[]
                    {
                        "DB_AkazaPhase2Combined_BodySilhouette",
                        "DB_AkazaPhase2Combined_FaceHairDetail"
                    }));
                Assert.That(core.Distinct().Count(), Is.EqualTo(2));
                Assert.That(
                    AuditionPvStationBossDeathAftermathDirector
                        .HasExactBossCoreRendererAuthoring(core),
                    Is.True);
                Assert.That(core.All(value => !value.gameObject.activeInHierarchy),
                    Is.True,
                    "The exact body renderers remain beneath the intentionally inactive Phase2 root before product activation.");
                foreach (SkinnedMeshRenderer renderer in core)
                {
                    Assert.That(renderer.enabled, Is.True,
                        $"{renderer.name} must be enabled.");
                    Assert.That(renderer.forceRenderingOff, Is.False,
                        $"{renderer.name} must not force rendering off.");
                    Assert.That(renderer.gameObject.activeSelf, Is.True,
                        $"{renderer.name} must be authored active under the runtime-owned Phase2 root.");
                    Assert.That(renderer.sharedMesh, Is.Not.Null,
                        $"{renderer.name} must retain its authored mesh.");
                    Assert.That(renderer.sharedMesh.vertexCount, Is.GreaterThan(0),
                        $"{renderer.name} must contain renderable vertices.");
                }
                Assert.That(allBossRenderers.Length, Is.GreaterThan(core.Length));
                Assert.That(core.Any(value => value.gameObject.name.IndexOf(
                    "Wing",
                    StringComparison.OrdinalIgnoreCase) >= 0), Is.False);

                AuditionPvStationBossDeathAftermathDirector
                    .ResolveExactBossCoreAxisTransforms(
                        motion.Animator.transform,
                        out Transform hips,
                        out Transform head);
                Assert.That(hips, Is.Not.Null);
                Assert.That(head, Is.Not.Null);
                Assert.That(head, Is.Not.SameAs(hips));
                Assert.That(hips.name, Is.EqualTo("CHakazaA:hip_C"));
                Assert.That(head.name, Is.EqualTo("CHakazaA:head_C"));
                Assert.That(hips.IsChildOf(motion.Animator.transform), Is.True);
                Assert.That(head.IsChildOf(motion.Animator.transform), Is.True);

                Assert.That(AkazaPhase2CombatMotionDriver
                    .RequiredDeathSettleSeconds, Is.EqualTo(0.90f));
                Assert.That(AkazaPhase2CombatMotionDriver
                    .RequiredDeathPivotLocalHeight, Is.EqualTo(0.72f));
                Assert.That(AkazaPhase2CombatMotionDriver
                    .RequiredDeathDropDistance, Is.EqualTo(0.50f));
                Assert.That(AkazaPhase2CombatMotionDriver
                    .RequiredDeathBackDistance, Is.EqualTo(0.22f));
                Assert.That(AkazaPhase2CombatMotionDriver
                    .RequiredDeathPitchDegrees, Is.EqualTo(20f));
                Assert.That(AkazaPhase2CombatMotionDriver
                    .RequiredDeathRollDegrees, Is.EqualTo(62f));
                Assert.That(AkazaPhase2CombatMotionDriver
                    .RequiredDeathWingFoldDegrees, Is.EqualTo(52f));
                Assert.That(AkazaPhase2CombatMotionDriver
                    .RequiredDeathWingYawDegrees, Is.EqualTo(20f));
                Assert.That(motion.DeathSettleDurationSeconds, Is.EqualTo(0.90f));
                Assert.That(motion.DeathPivotLocalHeight, Is.EqualTo(0.72f));
                Assert.That(motion.DeathDropDistance, Is.EqualTo(0.50f));
                Assert.That(motion.DeathBackDistance, Is.EqualTo(0.22f));
                Assert.That(motion.DeathPitchDegrees, Is.EqualTo(20f));
                Assert.That(motion.DeathRollDegrees, Is.EqualTo(62f));
                Assert.That(motion.DeathWingFoldDegrees, Is.EqualTo(52f));
                Assert.That(motion.DeathWingYawDegrees, Is.EqualTo(20f));
            }
            finally
            {
                if (openedByTest && station.IsValid() && station.isLoaded)
                {
                    EditorSceneManager.CloseScene(station, removeScene: true);
                }

                if (previousActiveScene.IsValid() && previousActiveScene.isLoaded)
                {
                    SceneManager.SetActiveScene(previousActiveScene);
                }
            }
        }

        [Test]
        public void StationFinisherDirector_EvaluatesExactStartSettleAndHeldRigPose()
        {
            Scene previousActiveScene = SceneManager.GetActiveScene();
            Scene station = SceneManager.GetSceneByPath(
                AuditionPvStationBossDeathAftermathCapture.StationScenePath);
            bool openedByTest = !station.IsValid() || !station.isLoaded;
            bool sceneWasDirty = station.IsValid() && station.isLoaded && station.isDirty;
            OlympusStationBossTerminalFinisherCameraController controller = null;
            PlayableDirector director = null;
            Transform rig = null;
            Vector3 savedLocalPosition = default;
            Quaternion savedLocalRotation = Quaternion.identity;
            Vector3 savedLocalScale = Vector3.one;
            double savedDirectorTime = 0d;
            DirectorUpdateMode savedUpdateMode = DirectorUpdateMode.GameTime;
            bool directorMutationStarted = false;

            try
            {
                if (openedByTest)
                {
                    station = EditorSceneManager.OpenScene(
                        AuditionPvStationBossDeathAftermathCapture.StationScenePath,
                        OpenSceneMode.Additive);
                    sceneWasDirty = station.isDirty;
                }

                controller = station.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<
                        OlympusStationBossTerminalFinisherCameraController>(true))
                    .Single();
                director = controller.FinisherDirector;
                rig = controller.FinisherCamera != null
                    ? controller.FinisherCamera.transform
                    : null;
                Assert.That(director, Is.Not.Null);
                Assert.That(rig, Is.Not.Null);
                Assert.That(rig, Is.SameAs(controller.transform));
                Assert.That(controller.FinisherCamera.fieldOfView,
                    Is.EqualTo(44f).Within(0.0001f));
                Assert.That(controller.FinisherTimeline, Is.Not.Null);
                Assert.That(director.playableAsset,
                    Is.SameAs(controller.FinisherTimeline));
                AnimationTrack track = controller.FinisherTimeline.GetRootTracks()
                    .Single() as AnimationTrack;
                Assert.That(track, Is.Not.Null);
                Assert.That(director.GetGenericBinding(track),
                    Is.SameAs(controller.GetComponent<Animator>()));
                if (director.state == PlayState.Playing)
                {
                    Assert.Ignore(
                        "The Station finisher Director is already playing; its live state was left untouched.");
                }

                savedLocalPosition = rig.localPosition;
                savedLocalRotation = rig.localRotation;
                savedLocalScale = rig.localScale;
                savedDirectorTime = director.time;
                savedUpdateMode = director.timeUpdateMode;
                directorMutationStarted = true;
                director.timeUpdateMode = DirectorUpdateMode.Manual;

                AssertFinisherRigPose(
                    director,
                    rig,
                    0d,
                    new Vector3(0f, 1.45f, 5.35f),
                    Quaternion.LookRotation(
                        new Vector3(0f, -0.40f, 0f)
                            - new Vector3(0f, 1.45f, 5.35f),
                        Vector3.up));
                AssertFinisherRigPose(
                    director,
                    rig,
                    0.14d,
                    new Vector3(0f, 1.40f, 5.60f),
                    Quaternion.LookRotation(
                        new Vector3(0f, -0.78f, 0f)
                            - new Vector3(0f, 1.40f, 5.60f),
                        Vector3.up));
                AssertFinisherRigPose(
                    director,
                    rig,
                    2.6d,
                    new Vector3(0f, 1.40f, 5.60f),
                    Quaternion.LookRotation(
                        new Vector3(0f, -0.78f, 0f)
                            - new Vector3(0f, 1.40f, 5.60f),
                        Vector3.up));
            }
            finally
            {
                try
                {
                    if (directorMutationStarted && director != null)
                    {
                        director.Stop();
                        director.timeUpdateMode = savedUpdateMode;
                        director.time = savedDirectorTime;
                    }

                    if (directorMutationStarted && rig != null)
                    {
                        rig.localPosition = savedLocalPosition;
                        rig.localRotation = savedLocalRotation;
                        rig.localScale = savedLocalScale;
                    }

                    if (!openedByTest
                        && !sceneWasDirty
                        && station.IsValid()
                        && station.isLoaded
                        && station.isDirty)
                    {
                        System.Reflection.MethodInfo clearDirtiness =
                            typeof(EditorSceneManager).GetMethod(
                                "ClearSceneDirtiness",
                                System.Reflection.BindingFlags.Static
                                    | System.Reflection.BindingFlags.NonPublic);
                        if (clearDirtiness == null)
                        {
                            throw new MissingMethodException(
                                "Unity Editor cannot restore Station scene dirtiness after the finisher pose test.");
                        }

                        clearDirtiness.Invoke(null, new object[] { station });
                    }
                }
                finally
                {
                    try
                    {
                        if (openedByTest && station.IsValid() && station.isLoaded)
                        {
                            EditorSceneManager.CloseScene(station, true);
                        }
                    }
                    finally
                    {
                        if (previousActiveScene.IsValid() && previousActiveScene.isLoaded)
                        {
                            SceneManager.SetActiveScene(previousActiveScene);
                        }
                    }
                }
            }
        }

        [Test]
        public void RecordingRegion_HasOneTryFireAndNoGameplayPresentationInjection()
        {
            string source = ReadProjectFile(
                AuditionPvStationBossDeathAftermathCapture.CaptureScriptPath);
            const string Begin = "// RECORDING CONTRACT BEGIN";
            const string End = "// RECORDING CONTRACT END";
            int begin = source.IndexOf(Begin, StringComparison.Ordinal);
            int end = source.IndexOf(End, StringComparison.Ordinal);
            Assert.That(begin, Is.GreaterThanOrEqualTo(0));
            Assert.That(end, Is.GreaterThan(begin));
            string executable = string.Join("\n",
                source.Substring(begin, end - begin)
                    .Split('\n')
                    .Where(line => !line.TrimStart().StartsWith(
                        "//", StringComparison.Ordinal)));
            Assert.That(Count(executable, "ranged.TryFire()"), Is.EqualTo(1));
            foreach (string forbidden in new[]
                     {
                         "TryApplyDamage", "TryApplyImpact", "PlayDeath",
                         "PlayWorldVfx", "RequestBossDeath", ".TryShow(",
                         ".Show(", "PublishCommitted", "RecordTerminal",
                         "transform.position =", ".velocity ="
                     })
            {
                Assert.That(executable, Does.Not.Contain(forbidden), forbidden);
            }
        }

        [Test]
        public void Preparation_UsesPublicCorridorHandoffAndStrictlyNonLethalSetupOnly()
        {
            string source = ReadProjectFile(
                AuditionPvStationBossDeathAftermathCapture.CaptureScriptPath);
            Assert.That(source, Does.Not.Contain("SceneManager.LoadScene("));
            Assert.That(source, Does.Not.Contain("SceneManager.LoadSceneAsync("));
            Assert.That(source, Does.Not.Contain("TrySealCurrentSegmentForSingleLoad"));
            Assert.That(source, Does.Not.Contain("SetExternalAimPreviewHeld"));
            Assert.That(source, Does.Contain("SetFireHeld(true)"));
            Assert.That(source, Does.Not.Contain("SetMoveInput(Vector2.up)"));
            Assert.That(source, Does.Contain("FindProperty(\"stairTriggerCenter\")"));
            Assert.That(source, Does.Contain("FindProperty(\"referenceCamera\")"));
            Assert.That(source, Does.Contain(
                ".ResolveCameraRelativeTutorialMoveInput("));
            Assert.That(source, Does.Contain("corridorMovement.SetMoveInput(moveInput);"));
            Assert.That(source, Does.Contain("PendingHandoffToken"));
            Assert.That(source, Does.Contain("TrySkipTransition()"));
            Assert.That(source, Does.Contain("ApplyStrictlyNonlethalSetupDamage"));
            Assert.That(source, Does.Contain("DismissActivePressureSummons()"));
            Assert.That(source, Does.Contain(
                "bossPressurePosition.SetMovementEnabled(false);"));
            Assert.That(source, Does.Contain(
                "bossPressurePosition.SetMovementEnabled(savedBossPressureMovementEnabled);"));
            Assert.That(source, Does.Contain("BeginAuthoredPlanarStep("));
            Assert.That(source, Does.Contain("Physics.SphereCastNonAlloc("));
            Assert.That(Count(source, "bossHealth.TryApplyDamage(damage)"),
                Is.EqualTo(1));
            Assert.That(source, Does.Not.Contain("projectile.transform.position ="));
            Assert.That(source, Does.Not.Contain("firedProjectile.transform.position ="));
            Assert.That(source, Does.Not.Contain("projectile.Velocity ="));
            Assert.That(source, Does.Not.Contain("firedProjectile.Velocity ="));
            Assert.That(source, Does.Not.Contain(".linearVelocity ="));
            Assert.That(source, Does.Not.Contain(".MovePosition("));
            Assert.That(source, Does.Not.Contain(".SetPositionAndRotation("));
            Assert.That(source, Does.Not.Contain("TryApplyImpact("));
            Assert.That(source, Does.Not.Contain("ResolveImpact("));
        }

        [Test]
        public void CorridorTutorialMoveInput_UsesTheAuthoredCameraBasis()
        {
            var cameraRoot = new GameObject("G08_TutorialMoveCamera");
            Camera movementCamera = cameraRoot.AddComponent<Camera>();
            try
            {
                cameraRoot.transform.rotation = Quaternion.LookRotation(
                    Vector3.right,
                    Vector3.up);

                Vector2 cameraForward = AuditionPvStationBossDeathAftermathCapture
                    .ResolveCameraRelativeTutorialMoveInput(
                        Vector3.zero,
                        Vector3.right * 4f,
                        movementCamera);
                Vector2 cameraLeft = AuditionPvStationBossDeathAftermathCapture
                    .ResolveCameraRelativeTutorialMoveInput(
                        Vector3.zero,
                        Vector3.forward * 4f,
                        movementCamera);

                Assert.That(cameraForward.x, Is.Zero.Within(0.0001f));
                Assert.That(cameraForward.y, Is.EqualTo(1f).Within(0.0001f));
                Assert.That(cameraLeft.x, Is.EqualTo(-1f).Within(0.0001f));
                Assert.That(cameraLeft.y, Is.Zero.Within(0.0001f));
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    AuditionPvStationBossDeathAftermathCapture
                        .ResolveCameraRelativeTutorialMoveInput(
                            Vector3.zero,
                            Vector3.zero,
                            movementCamera));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(cameraRoot);
            }
        }

        [Test]
        public void NaturalImpactCalibration_UsesPublicPreRollAndCentersOnlyF62()
        {
            Assert.That(
                AuditionPvStationBossDeathAftermathCapture
                    .PredictNaturalImpactFrame(24f),
                Is.EqualTo(61));
            Assert.That(
                AuditionPvStationBossDeathAftermathCapture
                    .PredictNaturalImpactFrame(24.0001f),
                Is.EqualTo(62));
            Assert.That(
                AuditionPvStationBossDeathAftermathCapture
                    .PredictNaturalImpactFrame(24.3999f),
                Is.EqualTo(62));
            Assert.That(
                AuditionPvStationBossDeathAftermathCapture
                    .PredictNaturalImpactFrame(24.4001f),
                Is.EqualTo(63));
            Assert.That(
                AuditionPvStationBossDeathAftermathCapture
                    .ResolveNaturalImpactAdjustmentStep(-3.305f, 0f),
                Is.EqualTo(-3f));
            Assert.That(
                AuditionPvStationBossDeathAftermathCapture
                    .ResolveNaturalImpactAdjustmentStep(-0.305f, 3f),
                Is.EqualTo(-0.305f).Within(0.000001f));
            Assert.That(
                AuditionPvStationBossDeathAftermathCapture
                    .ResolveNaturalImpactAdjustmentStep(2.25f, 0f),
                Is.EqualTo(2.25f));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                AuditionPvStationBossDeathAftermathCapture
                    .ResolveNaturalImpactAdjustmentStep(-1.01f, 3f));
            Assert.That(
                AuditionPvStationBossDeathAftermathCapture
                    .ResolveConfiguredProjectileWorldRadius(
                        0.31f,
                        new Vector3(0.28f, 0.28f, 0.28f),
                        Vector3.one),
                Is.EqualTo(0.0868f).Within(0.000001f));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                AuditionPvStationBossDeathAftermathCapture
                    .ResolveConfiguredProjectileWorldRadius(
                        0f,
                        Vector3.one,
                        Vector3.one));

            string source = ReadProjectFile(
                AuditionPvStationBossDeathAftermathCapture.CaptureScriptPath);
            int dismissal = source.IndexOf(
                "DismissActivePressureSummons()",
                StringComparison.Ordinal);
            int movementHold = source.IndexOf(
                "AcquireBossPressureMovementHold();",
                StringComparison.Ordinal);
            int publicStep = source.IndexOf(
                "movement.BeginAuthoredPlanarStep(",
                StringComparison.Ordinal);
            int preparationCall = source.IndexOf(
                "yield return PrepareNaturalBossImpactOwnership();",
                StringComparison.Ordinal);
            int shotArm = source.IndexOf(
                "public void BeginShotForRecorder()",
                StringComparison.Ordinal);
            Assert.That(movementHold, Is.GreaterThanOrEqualTo(0));
            Assert.That(dismissal, Is.GreaterThan(movementHold));
            Assert.That(publicStep, Is.GreaterThan(dismissal));
            Assert.That(preparationCall, Is.GreaterThanOrEqualTo(0));
            Assert.That(shotArm, Is.GreaterThan(preparationCall));
            Assert.That(source, Does.Contain(
                "ObserveBossPoseThroughImpact(atPhysicalImpact: true);"));

            int calibrationStart = source.IndexOf(
                "private IEnumerator PrepareNaturalBossImpactOwnership()",
                StringComparison.Ordinal);
            int calibrationEnd = source.IndexOf(
                "private void AcquireBossPressureMovementHold()",
                calibrationStart,
                StringComparison.Ordinal);
            Assert.That(calibrationStart, Is.GreaterThanOrEqualTo(0));
            Assert.That(calibrationEnd, Is.GreaterThan(calibrationStart));
            string calibration = source.Substring(
                calibrationStart,
                calibrationEnd - calibrationStart);
            Assert.That(
                Count(calibration, "ResolveNaturalImpactAdjustmentStep("),
                Is.EqualTo(1));
            Assert.That(
                Count(calibration, "movement.BeginAuthoredPlanarStep("),
                Is.EqualTo(1));
            Assert.That(calibration, Does.Contain(
                "cumulativeRequestedStepDistance += stepDistance;"));
            int boundStepCall = calibration.IndexOf(
                "movement.BeginAuthoredPlanarStep(",
                StringComparison.Ordinal);
            int boundStepDistance = calibration.IndexOf(
                "stepDistance,",
                boundStepCall,
                StringComparison.Ordinal);
            int boundStepDuration = calibration.IndexOf(
                "StepSeconds);",
                boundStepCall,
                StringComparison.Ordinal);
            Assert.That(boundStepDistance, Is.GreaterThan(boundStepCall));
            Assert.That(boundStepDuration, Is.GreaterThan(boundStepDistance));
            Assert.That(calibration, Does.Not.Contain(
                "movement.transform.position ="));
            Assert.That(calibration, Does.Not.Contain(".MovePosition("));
            Assert.That(calibration, Does.Not.Contain(
                ".SetPositionAndRotation("));
            Assert.That(source, Does.Contain(
                "AssetDatabase.LoadAssetAtPath<GameObject>("));
            Assert.That(source, Does.Contain(
                ".PlayerRangedProjectilePrefabPath);"));
            Assert.That(source, Does.Contain(
                "ranged.ConfiguredProjectilePrefab"));
            Assert.That(source, Does.Contain(
                "ranged.ConfiguredProjectileRadius"));
            Assert.That(source, Does.Contain(
                "projectileConfiguredWorldRadius,"));
            Assert.That(source, Does.Contain(
                "Mathf.Abs(localRadius"));
            Assert.That(source, Does.Contain(
                "Mathf.Abs(worldRadius"));
            Assert.That(source, Does.Contain(
                "maximumBossRotationDriftThroughImpact"));
            Assert.That(source, Does.Contain(
                "$\"before={pressureScreensBeforeDismiss}, \""));

            AuditionPvStationBossDeathAftermathGoldenRunner.RuntimeProof proof =
                CreateValidProof();
            Assert.That(proof.pressureScreensBeforeDismiss, Is.Zero);
            Assert.That(proof.pressureSummonsDismissed, Is.Zero);
            Assert.DoesNotThrow(() =>
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .ValidateRuntimeProofBeforePixelCalibration(proof));
            proof.pressureScreensBeforeDismiss = 2;
            proof.pressureSummonsDismissed = 2;
            Assert.DoesNotThrow(() =>
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .ValidateRuntimeProofBeforePixelCalibration(proof));
        }

        [Test]
        public void GuardedTransaction_AggregatesDepthTwoMoveAndDisposeFaultsThenCleansAndNotifiesOnce()
        {
            int rootDisposed = 0;
            int middleDisposed = 0;
            int leafDisposed = 0;
            var leaf = new ThrowingMoveNextAndDisposeIterator(
                () => leafDisposed++);
            bool coreProofCaptured = false;
            bool cleanupProofCaptured = false;
            bool cleanupCompleted = false;
            int cleanupDisposed = 0;
            int notifyCount = 0;
            int sequence = 0;
            int coreProofOrder = 0;
            int cleanupCompleteOrder = 0;
            int cleanupProofOrder = 0;
            int notifyOrder = 0;
            Exception notifiedFailure = null;
            IEnumerator transaction = G08GuardedCoroutineTransaction.Run(
                ThrowingIteratorRoot(
                    () => rootDisposed++,
                    () => middleDisposed++,
                    leaf),
                () =>
                {
                    coreProofCaptured = true;
                    coreProofOrder = ++sequence;
                    return null;
                },
                () => CleanupIteratorRoot(
                    () =>
                    {
                        cleanupCompleted = true;
                        cleanupCompleteOrder = ++sequence;
                    },
                    () => cleanupDisposed++),
                () =>
                {
                    cleanupProofCaptured = true;
                    cleanupProofOrder = ++sequence;
                    return null;
                },
                failure =>
                {
                    notifyCount++;
                    notifyOrder = ++sequence;
                    notifiedFailure = failure;
                });

            int yieldedCount = 0;
            while (transaction.MoveNext())
            {
                Assert.That(transaction.Current, Is.Null);
                yieldedCount++;
            }

            Assert.That(yieldedCount, Is.EqualTo(2));
            Assert.That(notifiedFailure, Is.Not.Null);
            Assert.That(notifiedFailure, Is.TypeOf<AggregateException>());
            string[] failureMessages = ((AggregateException)notifiedFailure)
                .Flatten()
                .InnerExceptions
                .Select(value => value.Message)
                .ToArray();
            Assert.That(failureMessages, Does.Contain(
                "depth-two-move-sentinel"));
            Assert.That(failureMessages, Does.Contain(
                "depth-two-dispose-sentinel"));
            Assert.That(rootDisposed, Is.EqualTo(1));
            Assert.That(middleDisposed, Is.EqualTo(1));
            Assert.That(leafDisposed, Is.EqualTo(1));
            Assert.That(coreProofCaptured, Is.True);
            Assert.That(cleanupCompleted, Is.True);
            Assert.That(cleanupDisposed, Is.EqualTo(2));
            Assert.That(cleanupProofCaptured, Is.True);
            Assert.That(notifyCount, Is.EqualTo(1));
            Assert.That(coreProofOrder, Is.LessThan(cleanupCompleteOrder));
            Assert.That(cleanupCompleteOrder, Is.LessThan(cleanupProofOrder));
            Assert.That(cleanupProofOrder, Is.LessThan(notifyOrder));

            string source = ReadProjectFile(
                AuditionPvStationBossDeathAftermathGoldenRunner.RunnerScriptPath);
            Assert.That(source, Does.Contain(
                "return G08GuardedCoroutineTransaction.Run("));
            Assert.That(source, Does.Contain(
                "yield return director.PrepareFreshProductState();"));
            Assert.That(source, Does.Contain(
                "CleanupAfterRecorder,"));
            Assert.That(source, Does.Contain(
                "CaptureCleanupProof,"));
            Assert.That(source, Does.Contain(
                "NotifyFinished);"));
            Assert.That(source, Does.Contain("value is IEnumerator nested"));
            Assert.That(source, Does.Contain(
                "!(value is CustomYieldInstruction)"));

            string captureSource = ReadProjectFile(
                AuditionPvStationBossDeathAftermathCapture.CaptureScriptPath);
            Assert.That(captureSource, Does.Contain(
                "yield return EnterCanonicalStation();"));
            Assert.That(captureSource, Does.Contain(
                "yield return ReleaseEntryGuide();"));
            Assert.That(captureSource, Does.Contain(
                "yield return PreparePhaseTwoAndHealth();"));
            Assert.That(captureSource, Does.Contain(
                "yield return PrepareNaturalBossImpactOwnership();"));
            Assert.That(source, Does.Contain(
                "yield return director.RestoreAfterRecording();"));
        }

        [Test]
        public void EditorResumeWatchdog_WaitsThroughUpdatingThenRunsWhenIdle()
        {
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .DetermineResumeWatchdogAction(
                        ownedSession: true,
                        isPlayingOrWillChangePlaymode: false,
                        isCompiling: false,
                        isUpdating: true),
                Is.EqualTo(AuditionPvStationBossDeathAftermathGoldenRunner
                    .ResumeWatchdogAction.KeepWaiting));
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .DetermineResumeWatchdogAction(
                        ownedSession: true,
                        isPlayingOrWillChangePlaymode: true,
                        isCompiling: false,
                        isUpdating: false),
                Is.EqualTo(AuditionPvStationBossDeathAftermathGoldenRunner
                    .ResumeWatchdogAction.KeepWaiting));
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .DetermineResumeWatchdogAction(
                        ownedSession: true,
                        isPlayingOrWillChangePlaymode: false,
                        isCompiling: true,
                        isUpdating: false),
                Is.EqualTo(AuditionPvStationBossDeathAftermathGoldenRunner
                    .ResumeWatchdogAction.KeepWaiting));
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .DetermineResumeWatchdogAction(
                        ownedSession: true,
                        isPlayingOrWillChangePlaymode: false,
                        isCompiling: false,
                        isUpdating: false),
                Is.EqualTo(AuditionPvStationBossDeathAftermathGoldenRunner
                    .ResumeWatchdogAction.Run));
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .DetermineResumeWatchdogAction(
                        ownedSession: false,
                        isPlayingOrWillChangePlaymode: false,
                        isCompiling: false,
                        isUpdating: false),
                Is.EqualTo(AuditionPvStationBossDeathAftermathGoldenRunner
                    .ResumeWatchdogAction.Unregister));

            string source = ReadProjectFile(
                AuditionPvStationBossDeathAftermathGoldenRunner.RunnerScriptPath);
            Assert.That(source, Does.Contain(
                "EditorApplication.update += ResumeOwnedSessionWatchdog"));
            Assert.That(source, Does.Contain(
                "change == PlayModeStateChange.ExitingPlayMode"));
            int notify = source.IndexOf(
                "internal static void NotifyPlayModeFinished",
                StringComparison.Ordinal);
            int watchdog = source.IndexOf(
                "EnsureResumeWatchdog();",
                notify,
                StringComparison.Ordinal);
            int requestEditMode = source.IndexOf(
                "EditorApplication.isPlaying = false;",
                notify,
                StringComparison.Ordinal);
            Assert.That(watchdog, Is.GreaterThan(notify));
            Assert.That(requestEditMode, Is.GreaterThan(watchdog));
        }

        [Test]
        public void RawWarmupRemap_IsExactAndRejectsAnyExtraOrMissingFrame()
        {
            string root = NewTempRoot("g08-remap");
            string frames = Path.Combine(root, "frames", "g08");
            string evidence = Path.Combine(root, "evidence");
            Directory.CreateDirectory(frames);
            try
            {
                for (int raw = 0;
                    raw < AuditionPvStationBossDeathAftermathGoldenRunner
                        .ExpectedRawFrameCount;
                    raw++)
                {
                    File.WriteAllText(
                        Path.Combine(
                            frames,
                            AuditionPvStationBossDeathAftermathGoldenRunner
                                .RawFrameFileName(raw)),
                        "raw-" + raw);
                }

                string warmup = AuditionPvStationBossDeathAftermathGoldenRunner
                    .RemapRawFrames(frames, evidence);
                Assert.That(File.ReadAllText(warmup), Is.EqualTo("raw-0"));
                Assert.That(File.ReadAllText(Path.Combine(
                    frames,
                    AuditionPvStationBossDeathAftermathCapture.FrameFileName(0))),
                    Is.EqualTo("raw-1"));
                Assert.That(File.ReadAllText(Path.Combine(
                    frames,
                    AuditionPvStationBossDeathAftermathCapture.FrameFileName(359))),
                    Is.EqualTo("raw-360"));
                Assert.DoesNotThrow(() =>
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .ValidateLogicalFrameSequence(frames));
                File.WriteAllText(Path.Combine(frames, "frame_0360.png"), "extra");
                Assert.Throws<InvalidDataException>(() =>
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .ValidateLogicalFrameSequence(frames));
            }
            finally
            {
                DeleteTempRoot(root);
            }
        }

        [Test]
        public void PixelCalibration_IsLockedToReviewedRuntimeTakeWithHeadroom()
        {
            AuditionPvStationBossDeathAftermathGoldenRunner.RuntimeProof proof =
                CreateValidProof();
            proof.sequenceBlackRatio = 0d;
            proof.sequenceMagentaRatio = 0d;
            proof.maximumFrameMagentaRatio = 0d;
            proof.healthyFramePercent = 100d;
            proof.impactMeanAbsoluteRgb = 13.542403067130082d;
            proof.impactChangedRatio = 0.29932291666666668d;
            proof.aftermathEvolutionMeanAbsoluteRgb = 30.48984809027858d;
            proof.aftermathEvolutionChangedRatio = 0.5495182291666667d;
            proof.resultAppearanceMeanAbsoluteRgb = 8.468068576389213d;
            proof.resultAppearanceChangedRatio = 0.32838541666666665d;
            proof.resultEntranceMeanAbsoluteRgb = 35.30529513888843d;
            proof.resultEntranceChangedRatio = 0.8523480902777778d;
            proof.resultBrightSamples = 76646;
            proof.resultNavySamples = 630;
            proof.resultBlueSamples = 80369;

            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner.PixelCalibrationLocked,
                Is.True);
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner.PixelCalibrationCaptureId,
                Is.EqualTo(
                    "20260816t084414z_g08-station-boss-death-aftermath_g174d6862472a_clean"));
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner.PixelCalibrationHeadSha,
                Is.EqualTo("174d6862472abf89b295749e37fdd1b280f97c49"));
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .PixelCalibrationFailureSha256,
                Is.EqualTo(
                    "e44e24e74c31f9ad6b6b1e0e6ef903ee10f7181cce5fd22afca0e1eda5defa9a"));
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .PixelCalibrationReconstructedLedgerSha256,
                Is.EqualTo(
                    "66577dd2934bae05f50c9812026d5e46e98f9de45de23c3c00393e1196d24de1"));
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner.ImpactDeltaFromFrame,
                Is.EqualTo(61));
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner.ImpactDeltaToFrame,
                Is.EqualTo(62));
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .AftermathDeltaFromFrame,
                Is.EqualTo(62));
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner.AftermathDeltaToFrame,
                Is.EqualTo(116));
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .ResultAppearanceFromFrame,
                Is.EqualTo(218));
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .ResultAppearanceToFrame,
                Is.EqualTo(221));
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .ResultEntranceFromFrame,
                Is.EqualTo(221));
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .ResultEntranceToFrame,
                Is.EqualTo(246));
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner.ResultSurfaceFrame,
                Is.EqualTo(246));
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .SequencePixelSampleStride,
                Is.EqualTo(8));
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .ExpectedSequencePixelSampleCount,
                Is.EqualTo(20736000L));
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .FrameDeltaPixelSampleStride,
                Is.EqualTo(4));
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .ExpectedFrameDeltaPixelSampleCount,
                Is.EqualTo(230400));
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .FrameDeltaChangedRgbSumCutoff,
                Is.EqualTo(24));
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .ResultSurfaceRawBottomLeftRoi.x,
                Is.EqualTo(256));
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .ResultSurfaceRawBottomLeftRoi.y,
                Is.EqualTo(180));
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .ResultSurfaceRawBottomLeftRoi.width,
                Is.EqualTo(2048));
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .ResultSurfaceRawBottomLeftRoi.height,
                Is.EqualTo(1080));
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .ResultSurfaceSampleStride,
                Is.EqualTo(4));
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .ExpectedResultSurfaceSampleCount,
                Is.EqualTo(138240));
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .ResultBrightMinimumChannel,
                Is.EqualTo(200));
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .ResultNavyMaximumLuma,
                Is.EqualTo(75));
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .ResultBlueMinimumChannel,
                Is.EqualTo(120));
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .ResultBlueMinimumRedDelta,
                Is.EqualTo(25));
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .ResultBlueMinimumGreenDelta,
                Is.EqualTo(10));
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .MaximumSequenceBlackRatio,
                Is.EqualTo(0.05d));
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .MaximumSequenceMagentaRatio,
                Is.EqualTo(0.001d));
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .MaximumFrameMagentaRatio,
                Is.EqualTo(0.005d));
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .MinimumHealthyFramePercent,
                Is.EqualTo(100));
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .MinimumImpactMeanAbsoluteRgb,
                Is.EqualTo(6d));
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .MinimumImpactChangedRatio,
                Is.EqualTo(0.12d));
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .MinimumAftermathEvolutionMeanAbsoluteRgb,
                Is.EqualTo(12d));
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .MinimumAftermathEvolutionChangedRatio,
                Is.EqualTo(0.20d));
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .MinimumResultAppearanceMeanAbsoluteRgb,
                Is.EqualTo(3d));
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .MinimumResultAppearanceChangedRatio,
                Is.EqualTo(0.08d));
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .MinimumResultEntranceMeanAbsoluteRgb,
                Is.EqualTo(15d));
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .MinimumResultEntranceChangedRatio,
                Is.EqualTo(0.30d));
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .MinimumResultBrightSamples,
                Is.EqualTo(60000));
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .MinimumResultNavySamples,
                Is.EqualTo(500));
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .MinimumResultBlueSamples,
                Is.EqualTo(60000));
            Assert.DoesNotThrow(() =>
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .ValidateRuntimeProofBeforePixelCalibration(proof));
            Assert.DoesNotThrow(() =>
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .ValidateRuntimeProof(proof));
            Assert.That(proof.impactMeanAbsoluteRgb,
                Is.GreaterThan(AuditionPvStationBossDeathAftermathGoldenRunner
                    .MinimumImpactMeanAbsoluteRgb));
            Assert.That(proof.aftermathEvolutionChangedRatio,
                Is.GreaterThan(AuditionPvStationBossDeathAftermathGoldenRunner
                    .MinimumAftermathEvolutionChangedRatio));
            Assert.That(proof.resultAppearanceChangedRatio,
                Is.GreaterThan(AuditionPvStationBossDeathAftermathGoldenRunner
                    .MinimumResultAppearanceChangedRatio));
            Assert.That(proof.resultEntranceChangedRatio,
                Is.GreaterThan(AuditionPvStationBossDeathAftermathGoldenRunner
                    .MinimumResultEntranceChangedRatio));
            Assert.That(proof.resultBlueSamples,
                Is.GreaterThan(AuditionPvStationBossDeathAftermathGoldenRunner
                    .MinimumResultBlueSamples));
        }

        [Test]
        public void VisualCompositionAcceptance_FailsClosedAfterCompleteTelemetryValidation()
        {
            AuditionPvStationBossDeathAftermathGoldenRunner.RuntimeProof proof =
                CreateValidProof();
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .VisualCompositionAcceptanceLocked,
                Is.False);
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .CompositionEvidenceFrames,
                Is.EqualTo(new[] { 61, 62, 116, 181, 246 }));
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .MinimumFinisherBossBodyHeightRatio,
                Is.EqualTo(0.25f));
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .MaximumFinisherBossBodyHeightRatio,
                Is.EqualTo(0.40f));
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .MinimumTerminalBossBodyMaxExtentRatio,
                Is.EqualTo(0.25f));
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .MaximumTerminalBossBodyMaxExtentRatio,
                Is.EqualTo(0.40f));
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .MinimumVisiblePlayerBodyHeightRatio,
                Is.EqualTo(0.25f));
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .MaximumVisiblePlayerBodyHeightRatio,
                Is.EqualTo(0.32f));
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .MaximumFinisherBossCenterDrift,
                Is.EqualTo(0.08f));
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .MaximumTerminalBossBodyMaxExtentSpread,
                Is.EqualTo(0.05f));
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .MinimumBossEnvelopeReadableExtentRatio,
                Is.EqualTo(0.05f));
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .MinimumBossCoreAxisViewportLength,
                Is.EqualTo(0.08f));
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .MinimumTerminalBossCoreAxisOrientationDeltaDegrees,
                Is.EqualTo(35f));
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .MaximumTerminalBossCoreAxisHoldDriftDegrees,
                Is.EqualTo(8f));
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .ExpectedCompositionProjectionAspect,
                Is.EqualTo(16f / 9f));
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .ExpectedCompositionProjectionAspect,
                Is.EqualTo(AuditionPvCaptureContract.Width
                    / (float)AuditionPvCaptureContract.Height));
            Assert.DoesNotThrow(() =>
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .ValidateRuntimeProofBeforeVisualCompositionAcceptance(proof));
            Assert.Throws<AuditionPvStationBossDeathAftermathGoldenRunner
                .G08VisualCompositionAcceptanceRequiredException>(() =>
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .ValidateRuntimeProofForPublication(proof));
        }

        [Test]
        public void EngineProvenance_JsonRoundTripsThroughStateAndFailsClosedPerField()
        {
            string root = Path.Combine(
                Path.GetTempPath(),
                "DimensionBrawl_G08_EngineProvenance_RoundTrip");
            AuditionPvStationBossDeathAftermathGoldenRunner.PersistedRunnerState state =
                CreateState(
                    root,
                    "20260816t120000z_g08-station-boss-death-aftermath_gaaaaaaaaaaaa_clean",
                    new DateTime(2026, 8, 16, 12, 0, 0, DateTimeKind.Utc));
            string engineJson = JsonUtility.ToJson(state.engine);
            AuditionPvEngineSnapshot engineRoundTrip =
                JsonUtility.FromJson<AuditionPvEngineSnapshot>(engineJson);
            AssertEngineSnapshotExact(engineRoundTrip);

            string before = AuditionPvStationBossDeathAftermathGoldenRunner
                .ComputeCaptureStartProvenanceSha256(state);
            string stateJson = JsonUtility.ToJson(state);
            AuditionPvStationBossDeathAftermathGoldenRunner.PersistedRunnerState
                stateRoundTrip = JsonUtility.FromJson<
                    AuditionPvStationBossDeathAftermathGoldenRunner.PersistedRunnerState>(
                        stateJson);
            Assert.That(stateRoundTrip, Is.Not.Null);
            AssertEngineSnapshotExact(stateRoundTrip.engine);
            string after = AuditionPvStationBossDeathAftermathGoldenRunner
                .ComputeCaptureStartProvenanceSha256(stateRoundTrip);
            Assert.That(after, Is.EqualTo(before));
            Assert.That(AuditionPvSha256.IsSha256(after), Is.True);

            Assert.Throws<InvalidDataException>(() =>
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .ValidateRequiredEngineProvenance(null));
            Action<AuditionPvEngineSnapshot>[] blankOneField =
            {
                value => value.unityVersion = string.Empty,
                value => value.unityVersionWithRevision = " ",
                value => value.recorderPackageVersion = string.Empty,
                value => value.urpPackageVersion = "\t",
                value => value.activeRenderPipelineAssetPath = string.Empty
            };
            foreach (Action<AuditionPvEngineSnapshot> blank in blankOneField)
            {
                AuditionPvEngineSnapshot candidate =
                    JsonUtility.FromJson<AuditionPvEngineSnapshot>(engineJson);
                blank(candidate);
                Assert.Throws<InvalidDataException>(() =>
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .ValidateRequiredEngineProvenance(candidate));
            }
        }

        [Test]
        public void CompositionEvidence_AllowsPeripheralEnvelopeCropButNotCoreCrop()
        {
            AuditionPvStationBossDeathAftermathGoldenRunner.RuntimeProof proof =
                CreateValidProof();
            foreach (AuditionPvStationBossDeathAftermathGoldenRunner.RenderEvidence value
                in proof.renderEvidence.Skip(1).Take(3))
            {
                value.bossEnvelopeFullyInsideFrustum = false;
                value.bossEnvelopePartiallyClipped = true;
            }

            Assert.DoesNotThrow(() =>
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .ValidateRuntimeProofBeforePixelCalibration(proof));

            proof.renderEvidence[2].bossPartiallyClipped = true;
            proof.renderEvidence[2].bossFullyInsideFrustum = false;
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .ValidateRuntimeProofBeforePixelCalibration(proof));
        }

        [Test]
        public void RenderEvidence_CopyAndJsonRoundTripPreserveCoreEnvelopeAndAxisTelemetry()
        {
            var owner = new GameObject("G08_RenderProbe_Copy_Test");
            try
            {
                AuditionPvStationBossDeathAftermathRenderProbe probe =
                    owner.AddComponent<AuditionPvStationBossDeathAftermathRenderProbe>();
                AuditionPvStationBossDeathAftermathGoldenRunner.RenderEvidence source =
                    FinisherEvidence(116, 0.31f, new Vector2(0.52f, 0.51f));
                var values = (IList)typeof(
                        AuditionPvStationBossDeathAftermathRenderProbe)
                    .GetField(
                        "evidence",
                        System.Reflection.BindingFlags.Instance
                            | System.Reflection.BindingFlags.NonPublic)
                    ?.GetValue(probe);
                Assert.That(values, Is.Not.Null);
                values.Add(source);

                AuditionPvStationBossDeathAftermathGoldenRunner.RenderEvidence copy =
                    probe.CopyEvidence().Single();
                Assert.That(copy, Is.Not.SameAs(source));
                Assert.That(copy.bossBodyRendererNames, Is.EqualTo(
                    "DB_AkazaPhase2Combined_BodySilhouette|DB_AkazaPhase2Combined_FaceHairDetail"));
                Assert.That(copy.bossBodyRendererCount, Is.EqualTo(2));
                Assert.That(copy.projectionAspect, Is.EqualTo(16f / 9f));
                Assert.That(copy.bossBodyWidthRatio,
                    Is.EqualTo(0.31f / (16f / 9f)));
                Assert.That(copy.bossBodyMaxExtentRatio, Is.EqualTo(0.31f));
                Assert.That(copy.bossEnvelopeVisible, Is.True);
                Assert.That(copy.bossEnvelopePartiallyClipped, Is.True);
                Assert.That(copy.bossEnvelopeRendererCount, Is.EqualTo(4));
                Assert.That(copy.bossCoreAxisSource,
                    Is.EqualTo("akaza-generic-hip_C-to-head_C"));
                Assert.That(copy.bossCoreAxisHipsViewport,
                    Is.EqualTo(source.bossCoreAxisHipsViewport));
                Assert.That(copy.bossCoreAxisHeadViewport,
                    Is.EqualTo(source.bossCoreAxisHeadViewport));

                string json = JsonUtility.ToJson(copy);
                AuditionPvStationBossDeathAftermathGoldenRunner.RenderEvidence
                    roundTrip = JsonUtility.FromJson<
                        AuditionPvStationBossDeathAftermathGoldenRunner.RenderEvidence>(json);
                Assert.That(roundTrip.bossBodyRendererNames,
                    Is.EqualTo(copy.bossBodyRendererNames));
                Assert.That(roundTrip.bossBodyWidthRatio,
                    Is.EqualTo(copy.bossBodyWidthRatio));
                Assert.That(roundTrip.bossEnvelopeMaxExtentRatio,
                    Is.EqualTo(copy.bossEnvelopeMaxExtentRatio));
                Assert.That(roundTrip.bossCoreAxisViewportLength,
                    Is.EqualTo(copy.bossCoreAxisViewportLength));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(owner);
            }
        }

        [Test]
        public void PixelTelemetry_RejectsNonFiniteNegativeAndOutOfDomainValues()
        {
            AssertRuntimeMutation(proof => proof.sequenceBlackRatio = double.PositiveInfinity);
            AssertRuntimeMutation(proof => proof.sequenceMagentaRatio = -0.0001d);
            AssertRuntimeMutation(proof => proof.maximumFrameMagentaRatio = double.NaN);
            AssertRuntimeMutation(proof => proof.healthyFramePercent = 100.0001d);
            AssertRuntimeMutation(proof => proof.impactMeanAbsoluteRgb = -1d);
            AssertRuntimeMutation(proof => proof.impactMeanAbsoluteRgb = 255.0001d);
            AssertRuntimeMutation(proof => proof.impactChangedRatio = 1.0001d);
            AssertRuntimeMutation(proof =>
                proof.aftermathEvolutionMeanAbsoluteRgb = 255.0001d);
            AssertRuntimeMutation(proof => proof.aftermathEvolutionChangedRatio = -1d);
            AssertRuntimeMutation(proof =>
                proof.resultAppearanceMeanAbsoluteRgb = 255.0001d);
            AssertRuntimeMutation(proof =>
                proof.resultEntranceMeanAbsoluteRgb = 255.0001d);
            AssertRuntimeMutation(proof => proof.resultEntranceChangedRatio = double.NaN);
            AssertRuntimeMutation(proof => proof.resultBrightSamples = -1);
            AssertRuntimeMutation(proof => proof.resultNavySamples = -1);
            AssertRuntimeMutation(proof => proof.resultBlueSamples = -1);
            AssertRuntimeMutation(proof =>
                proof.resultBrightSamples = proof.resultSurfaceSampleCount + 1);
            AssertRuntimeMutation(proof =>
                proof.resultNavySamples = proof.resultSurfaceSampleCount + 1);
            AssertRuntimeMutation(proof =>
                proof.resultBlueSamples = proof.resultSurfaceSampleCount + 1);
            AssertRuntimeMutation(proof => proof.pixelSampleStride = 7);
            AssertRuntimeMutation(proof => proof.pixelSampleCount--);
            AssertRuntimeMutation(proof => proof.frameDeltaPixelSampleStride = 3);
            AssertRuntimeMutation(proof => proof.frameDeltaPixelSampleCount--);
            AssertRuntimeMutation(proof => proof.frameDeltaChangedRgbSumCutoff = 23);
            AssertRuntimeMutation(proof => proof.impactDeltaFromFrame = 60);
            AssertRuntimeMutation(proof => proof.impactDeltaToFrame = 61);
            AssertRuntimeMutation(proof => proof.aftermathDeltaFromFrame = 61);
            AssertRuntimeMutation(proof => proof.aftermathDeltaToFrame = 115);
            AssertRuntimeMutation(proof => proof.resultAppearanceFromFrame = 217);
            AssertRuntimeMutation(proof => proof.resultAppearanceToFrame = 220);
            AssertRuntimeMutation(proof => proof.resultEntranceFromFrame = 220);
            AssertRuntimeMutation(proof => proof.resultEntranceToFrame = 245);
            AssertRuntimeMutation(proof => proof.resultSurfaceFrame = 245);
            AssertRuntimeMutation(proof => proof.resultSurfaceRoiX++);
            AssertRuntimeMutation(proof => proof.resultSurfaceRoiY++);
            AssertRuntimeMutation(proof => proof.resultSurfaceRoiWidth--);
            AssertRuntimeMutation(proof => proof.resultSurfaceRoiHeight--);
            AssertRuntimeMutation(proof => proof.resultSurfaceSampleStride++);
            AssertRuntimeMutation(proof => proof.resultSurfaceSampleCount--);
            AssertRuntimeMutation(proof => proof.resultBrightMinimumChannel--);
            AssertRuntimeMutation(proof => proof.resultNavyMaximumLuma--);
            AssertRuntimeMutation(proof => proof.resultBlueMinimumChannel--);
            AssertRuntimeMutation(proof => proof.resultBlueMinimumRedDelta--);
            AssertRuntimeMutation(proof => proof.resultBlueMinimumGreenDelta--);
            AssertRuntimeMutation(proof => proof.cleanupFailure = "cleanup leaked");
            AssertRuntimeMutation(proof => proof.aftermathElapsedSeconds = 2.599f);
            AssertRuntimeMutation(proof => proof.overlayPresentationSucceededCount = 0);
            AssertRuntimeMutation(proof => proof.outcomeFactDigest = string.Empty);
            AssertRuntimeMutation(proof => proof.projectileImpactDirection = Vector3.zero);
        }

        [Test]
        public void LockedPixelThresholds_AcceptEveryBoundaryAndRejectEveryCrossing()
        {
            AuditionPvStationBossDeathAftermathGoldenRunner.RuntimeProof Boundary()
            {
                AuditionPvStationBossDeathAftermathGoldenRunner.RuntimeProof proof =
                    CreateValidProof();
                proof.sequenceBlackRatio = AuditionPvStationBossDeathAftermathGoldenRunner
                    .MaximumSequenceBlackRatio;
                proof.sequenceMagentaRatio = AuditionPvStationBossDeathAftermathGoldenRunner
                    .MaximumSequenceMagentaRatio;
                proof.maximumFrameMagentaRatio =
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .MaximumFrameMagentaRatio;
                proof.healthyFramePercent = AuditionPvStationBossDeathAftermathGoldenRunner
                    .MinimumHealthyFramePercent;
                proof.impactMeanAbsoluteRgb =
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .MinimumImpactMeanAbsoluteRgb;
                proof.impactChangedRatio =
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .MinimumImpactChangedRatio;
                proof.aftermathEvolutionMeanAbsoluteRgb =
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .MinimumAftermathEvolutionMeanAbsoluteRgb;
                proof.aftermathEvolutionChangedRatio =
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .MinimumAftermathEvolutionChangedRatio;
                proof.resultAppearanceMeanAbsoluteRgb =
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .MinimumResultAppearanceMeanAbsoluteRgb;
                proof.resultAppearanceChangedRatio =
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .MinimumResultAppearanceChangedRatio;
                proof.resultEntranceMeanAbsoluteRgb =
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .MinimumResultEntranceMeanAbsoluteRgb;
                proof.resultEntranceChangedRatio =
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .MinimumResultEntranceChangedRatio;
                proof.resultBrightSamples = AuditionPvStationBossDeathAftermathGoldenRunner
                    .MinimumResultBrightSamples;
                proof.resultNavySamples = AuditionPvStationBossDeathAftermathGoldenRunner
                    .MinimumResultNavySamples;
                proof.resultBlueSamples = AuditionPvStationBossDeathAftermathGoldenRunner
                    .MinimumResultBlueSamples;
                return proof;
            }

            Assert.DoesNotThrow(() =>
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .ValidateLockedPixelThresholdsForTests(Boundary()));
            void Reject(Action<AuditionPvStationBossDeathAftermathGoldenRunner.RuntimeProof>
                mutate)
            {
                AuditionPvStationBossDeathAftermathGoldenRunner.RuntimeProof proof =
                    Boundary();
                mutate(proof);
                Assert.Throws<InvalidOperationException>(() =>
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .ValidateLockedPixelThresholdsForTests(proof));
            }

            Reject(value => value.sequenceBlackRatio += 0.000001d);
            Reject(value => value.sequenceMagentaRatio += 0.000001d);
            Reject(value => value.maximumFrameMagentaRatio += 0.000001d);
            Reject(value => value.healthyFramePercent -= 0.000001d);
            Reject(value => value.impactMeanAbsoluteRgb -= 0.000001d);
            Reject(value => value.impactChangedRatio -= 0.000001d);
            Reject(value => value.aftermathEvolutionMeanAbsoluteRgb -= 0.000001d);
            Reject(value => value.aftermathEvolutionChangedRatio -= 0.000001d);
            Reject(value => value.resultAppearanceMeanAbsoluteRgb -= 0.000001d);
            Reject(value => value.resultAppearanceChangedRatio -= 0.000001d);
            Reject(value => value.resultEntranceMeanAbsoluteRgb -= 0.000001d);
            Reject(value => value.resultEntranceChangedRatio -= 0.000001d);
            Reject(value => value.resultBrightSamples--);
            Reject(value => value.resultNavySamples--);
            Reject(value => value.resultBlueSamples--);
        }

        [Test]
        public void CalibrationFailureReplay_WritesTelemetryAndLeavesNoSuccessArtifacts()
        {
            string root = NewTempRoot("g08-calibration");
            const string CaptureId = "g08-calibration-first-take";
            string output = AuditionPvOutputPaths.ResolveOutputDirectory(root, CaptureId);
            string evidence = Path.Combine(output,
                AuditionPvStationBossDeathAftermathGoldenRunner.EvidenceFolderName);
            string baselines = Path.Combine(output,
                AuditionPvStationBossDeathAftermathCapture.BaselinesFolderName);
            Directory.CreateDirectory(evidence);
            Directory.CreateDirectory(baselines);
            string[] successArtifacts = SuccessArtifactPaths(output);
            try
            {
                foreach (string path in successArtifacts)
                {
                    File.WriteAllText(path, "must-be-removed");
                }

                var state = CreateState(root, CaptureId);
                var proof = CreateValidProof();
                var exception = new AuditionPvStationBossDeathAftermathGoldenRunner
                    .G08PixelCalibrationRequiredException(
                        "CalibrationRequired historical first-take replay");
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .WriteFailureArtifactForRoot(
                        output,
                        "AwaitingEditMode",
                        exception,
                        proof,
                        state,
                        root);

                string failurePath = Path.Combine(
                    output,
                    AuditionPvStationBossDeathAftermathGoldenRunner.FailureFileName);
                CalibrationFailureProbe artifact = JsonUtility.FromJson<
                    CalibrationFailureProbe>(File.ReadAllText(failurePath));
                Assert.That(artifact.pixelCalibrationLocked, Is.True);
                Assert.That(artifact.calibrationRequired, Is.True);
                Assert.That(artifact.exception, Does.Contain("CalibrationRequired"));
                Assert.That(artifact.runtime, Is.Not.Null);
                Assert.That(artifact.runtime.impactMeanAbsoluteRgb,
                    Is.EqualTo(proof.impactMeanAbsoluteRgb));
                Assert.That(successArtifacts.All(path => !File.Exists(path)), Is.True);
                Assert.That(File.Exists(failurePath), Is.True);
            }
            finally
            {
                DeleteTempRoot(root);
            }
        }

        [Test]
        public void VisualAcceptanceFirstTake_WritesCompositionTelemetryAndNoSuccessArtifacts()
        {
            string root = NewTempRoot("g08-visual-acceptance");
            const string CaptureId = "g08-finisher-visual-first-take";
            string output = AuditionPvOutputPaths.ResolveOutputDirectory(root, CaptureId);
            Directory.CreateDirectory(Path.Combine(
                output,
                AuditionPvStationBossDeathAftermathGoldenRunner.EvidenceFolderName));
            Directory.CreateDirectory(Path.Combine(
                output,
                AuditionPvStationBossDeathAftermathCapture.BaselinesFolderName));
            string[] successArtifacts = SuccessArtifactPaths(output);
            try
            {
                foreach (string path in successArtifacts)
                {
                    File.WriteAllText(path, "must-be-removed");
                }

                var state = CreateState(root, CaptureId);
                var proof = CreateValidProof();
                Assert.DoesNotThrow(() =>
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .ValidateRuntimeProofBeforeVisualCompositionAcceptance(proof));
                var exception = Assert.Throws<
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .G08VisualCompositionAcceptanceRequiredException>(() =>
                            AuditionPvStationBossDeathAftermathGoldenRunner
                                .ValidateRuntimeProofForPublication(proof));
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .WriteFailureArtifactForRoot(
                        output,
                        "AwaitingEditMode",
                        exception,
                        proof,
                        state,
                        root);

                CalibrationFailureProbe artifact = JsonUtility.FromJson<
                    CalibrationFailureProbe>(File.ReadAllText(Path.Combine(
                        output,
                        AuditionPvStationBossDeathAftermathGoldenRunner
                            .FailureFileName)));
                Assert.That(artifact.pixelCalibrationLocked, Is.True);
                Assert.That(artifact.calibrationRequired, Is.False);
                Assert.That(artifact.visualCompositionAcceptanceLocked, Is.False);
                Assert.That(artifact.visualCompositionAcceptanceRequired, Is.True);
                Assert.That(artifact.runtime.renderEvidence.Select(value => value.frame),
                    Is.EqualTo(new[] { 61, 62, 116, 181, 246 }));
                Assert.That(artifact.runtime.finisherCameraSampleCount,
                    Is.EqualTo(156));
                Assert.That(artifact.runtime.finisherCameraResultCoverReleaseSampleCount,
                    Is.EqualTo(28));
                Assert.That(successArtifacts.All(path => !File.Exists(path)), Is.True);
            }
            finally
            {
                DeleteTempRoot(root);
            }
        }

        [Test]
        public void FailureCleanup_IsExhaustiveAndReportsInjectedDeleteFault()
        {
            string root = NewTempRoot("g08-delete-fault");
            const string CaptureId = "g08-delete-fault-fixture";
            string output = AuditionPvOutputPaths.ResolveOutputDirectory(root, CaptureId);
            string[] artifacts = SuccessArtifactPaths(output);
            foreach (string path in artifacts)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, "owned");
            }

            try
            {
                int calls = 0;
                string diagnostic = AuditionPvStationBossDeathAftermathGoldenRunner
                    .DeleteUncommittedSuccessArtifactsForRoot(
                        output,
                        CaptureId,
                        root,
                        path =>
                        {
                            calls++;
                            if (calls == 1)
                            {
                                throw new IOException("injected first delete fault");
                            }

                            File.Delete(path);
                        });
                Assert.That(diagnostic, Does.Contain("injected first delete fault"));
                Assert.That(calls, Is.EqualTo(artifacts.Length));
                Assert.That(File.Exists(artifacts[0]), Is.True);
                Assert.That(artifacts.Skip(1).All(path => !File.Exists(path)), Is.True);
                Assert.That(
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .DeleteUncommittedSuccessArtifactsForRoot(
                            output,
                            CaptureId,
                            root),
                    Is.Empty);
                Assert.That(artifacts.All(path => !File.Exists(path)), Is.True);
            }
            finally
            {
                DeleteTempRoot(root);
            }
        }

        [Test]
        public void FailureArtifact_RecordsCleanupFaultWithoutSkippingLaterTargets()
        {
            string root = NewTempRoot("g08-failure-cleanup-artifact");
            const string CaptureId = "g08-failure-cleanup-artifact";
            string output = AuditionPvOutputPaths.ResolveOutputDirectory(root, CaptureId);
            string[] artifacts = SuccessArtifactPaths(output);
            foreach (string path in artifacts)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, "owned");
            }

            try
            {
                int calls = 0;
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .WriteFailureArtifactForRoot(
                        output,
                        "injected-cleanup",
                        new InvalidOperationException("primary capture failure"),
                        CreateValidProof(),
                        CreateState(root, CaptureId),
                        root,
                        path =>
                        {
                            calls++;
                            if (calls == 1)
                            {
                                throw new IOException("injected cleanup persistence fault");
                            }

                            File.Delete(path);
                        });
                string failurePath = Path.Combine(
                    output,
                    AuditionPvStationBossDeathAftermathGoldenRunner.FailureFileName);
                CalibrationFailureProbe failure = JsonUtility.FromJson<
                    CalibrationFailureProbe>(File.ReadAllText(failurePath));
                Assert.That(failure.successArtifactCleanupFailure,
                    Does.Contain("injected cleanup persistence fault"));
                Assert.That(calls, Is.EqualTo(artifacts.Length));
                Assert.That(artifacts.Skip(1).All(path => !File.Exists(path)), Is.True);
                Assert.That(File.Exists(failurePath), Is.True);
            }
            finally
            {
                DeleteTempRoot(root);
            }
        }

        [Test]
        public void PersistedState_RejectsBaselineOutputRootAndSessionAuthorityEscapes()
        {
            string root = NewTempRoot("g08-state-root");
            string outside = NewTempRoot("g08-state-outside");
            const string CaptureId = "g08-state-fixture";
            var state = CreateState(root, CaptureId);
            string statePath = Path.Combine(
                state.outputDirectory,
                AuditionPvStationBossDeathAftermathGoldenRunner.StateFileName);
            try
            {
                Assert.DoesNotThrow(() =>
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .ValidatePersistedStateLocationForRoot(
                            statePath,
                            state,
                            root));
                Assert.DoesNotThrow(() =>
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .ValidateSessionStateAuthority(
                            state.outputDirectory,
                            state.captureId,
                            state.batchMode,
                            state));

                state.baselineDirectory = outside;
                Assert.Throws<InvalidDataException>(() =>
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .ValidatePersistedStateLocationForRoot(statePath, state, root));
                state.baselineDirectory = Path.Combine(
                    state.outputDirectory,
                    AuditionPvStationBossDeathAftermathCapture.BaselinesFolderName);
                state.outputRoot = outside;
                Assert.Throws<InvalidDataException>(() =>
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .ValidatePersistedStateLocationForRoot(statePath, state, root));
                state.outputRoot = root;
                Assert.Throws<InvalidDataException>(() =>
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .ValidateSessionStateAuthority(
                            outside,
                            CaptureId,
                            false,
                            state));
                Assert.Throws<InvalidDataException>(() =>
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .ValidateSessionRecoveryLocationForRoot(
                            Path.Combine(outside, "state.json"),
                            state.outputDirectory,
                            CaptureId,
                            root));
            }
            finally
            {
                DeleteTempRoot(root);
                DeleteTempRoot(outside);
            }
        }

        [Test]
        public void TerminalPersistenceRecovery_ClearsSessionWritesFailureAndRequestsExitOne()
        {
            string root = NewTempRoot("g08-terminal-fault");
            const string CaptureId = "g08-terminal-fault-fixture";
            string output = AuditionPvOutputPaths.ResolveOutputDirectory(root, CaptureId);
            Directory.CreateDirectory(output);
            bool cleared = false;
            int exitCode = -1;
            try
            {
                Exception failure = AuditionPvStationBossDeathAftermathGoldenRunner
                    .RecoverTerminalPersistenceFaultForRoot(
                        output,
                        CaptureId,
                        root,
                        "injected SaveState fault",
                        () => cleared = true,
                        code => exitCode = code);
                Assert.That(failure, Is.Null);
                Assert.That(cleared, Is.True);
                Assert.That(exitCode, Is.EqualTo(1));
                string failurePath = Path.Combine(
                    output,
                    AuditionPvStationBossDeathAftermathGoldenRunner.FailureFileName);
                Assert.That(File.Exists(failurePath), Is.True);
                Assert.That(File.ReadAllText(failurePath),
                    Does.Contain("stale Recording state was not resumed"));
                Assert.That(SuccessArtifactPaths(output).All(path => !File.Exists(path)),
                    Is.True);
            }
            finally
            {
                DeleteTempRoot(root);
            }
        }

        [Test]
        public void Manifest_IsExactAndBoundToCaptureStartStateAndRuntimeProof()
        {
            string root = NewTempRoot("g08-manifest");
            DateTime started = new(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);
            const string CaptureId =
                "20260102t030405z_g08-station-boss-death-aftermath_fixture";
            var state = CreateState(root, CaptureId, started);
            string output = state.outputDirectory;
            string evidence = Path.Combine(output,
                AuditionPvStationBossDeathAftermathGoldenRunner.EvidenceFolderName);
            var proof = CreateValidProof();
            proof.frameHashLedgerPath = Path.Combine(
                evidence,
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .FrameHashLedgerFileName).Replace('\\', '/');
            proof.warmupEvidencePath = Path.Combine(
                evidence,
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .WarmupEvidenceFileName).Replace('\\', '/');
            string[] paths = AuditionPvStationBossDeathAftermathGoldenRunner
                .CollectCaptureDependencyPaths();
            AuditionPvDependencyHash[] dependencies = paths.Select(path =>
                new AuditionPvDependencyHash
                {
                    path = path,
                    exists = true,
                    byteLength = 1,
                    sha256 = ShaA
                }).ToArray();
            state.dependencyPaths = paths;
            state.dependencyHashesAtStart = dependencies;
            proof.dependencyHashCount = dependencies.Length;
            proof.captureStartProvenanceSha256 =
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .ComputeCaptureStartProvenanceSha256(state);
            AuditionPvTestResult[] tests =
                AuditionPvStationBossDeathAftermathGoldenRunner.CreateTestResults(
                    state,
                    proof,
                    Path.Combine(
                        evidence,
                        AuditionPvStationBossDeathAftermathGoldenRunner
                            .RuntimeProofFileName),
                    started);
            AuditionPvCaptureManifest manifest =
                AuditionPvCaptureManifestFactory.CreateForRoot(
                    CaptureId,
                    root,
                    output,
                    new[]
                    {
                        AuditionPvStationBossDeathAftermathCapture
                            .CreateShotManifestEntry()
                    },
                    AuditionPvStationBossDeathAftermathCapture
                        .CreateBaselineManifestEntries(),
                    tests,
                    createdAtUtc: started,
                    gitSnapshot: new AuditionPvGitSnapshot
                    {
                        probeSucceeded = true,
                        commitSha = state.gitCommitSha,
                        branch = state.gitBranch,
                        isDirty = false,
                        dirtyStateHashSha256 = state.gitDirtyHashSha256
                    },
                    engineSnapshot: state.engine,
                    dependencyHashSnapshot: dependencies);
            try
            {
                Assert.DoesNotThrow(() =>
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .ValidateManifestInMemory(manifest, CaptureId));
                Assert.DoesNotThrow(() =>
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .ValidateManifestMatchesRecordedState(state, manifest));
                Assert.DoesNotThrow(() =>
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .ValidateManifestProofProvenance(manifest, proof));

                string notes = manifest.shots[0].notes;
                manifest.shots[0].notes = "semantic substitution";
                Assert.Throws<InvalidOperationException>(() =>
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .ValidateManifestInMemory(manifest, CaptureId));
                manifest.shots[0].notes = notes;
                string commit = manifest.gitCommitSha;
                manifest.gitCommitSha = new string('b', 40);
                Assert.Throws<InvalidOperationException>(() =>
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .ValidateManifestMatchesRecordedState(state, manifest));
                manifest.gitCommitSha = commit;
                proof.captureStartProvenanceSha256 = ShaB;
                Assert.Throws<InvalidOperationException>(() =>
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .ValidateManifestProofProvenance(manifest, proof));
            }
            finally
            {
                DeleteTempRoot(root);
            }
        }

        [Test]
        public void CommittedPackageValidator_AcceptsExactPackageRejectsCorruptionAndWinsStaleFault()
        {
            string root = NewTempRoot("g08-committed-package");
            DateTime started = new(2026, 1, 2, 3, 4, 6, DateTimeKind.Utc);
            const string CaptureId =
                "20260102t030406z_g08-station-boss-death-aftermath_fixture";
            try
            {
                AuditionPvStationBossDeathAftermathGoldenRunner.PersistedRunnerState state =
                    WriteCommittedPackageFixture(root, CaptureId, started);
                bool valid = AuditionPvStationBossDeathAftermathGoldenRunner
                    .IsValidCommittedManifestAtForTests(
                        state.outputDirectory,
                        CaptureId,
                        root,
                        state);
                Assert.That(valid, Is.True);
                Assert.That(
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .DetermineSessionRecoveryDecision(
                            editorPlaying: false,
                            committedManifestIsValid: valid,
                            terminalFault: "stale injected terminal marker"),
                    Is.EqualTo(AuditionPvStationBossDeathAftermathGoldenRunner
                        .SessionRecoveryDecision.CommittedManifest));

                string baseline = Path.Combine(
                    state.baselineDirectory,
                    AuditionPvStationBossDeathAftermathCapture.Bl10FileName);
                byte[] baselineBytes = File.ReadAllBytes(baseline);
                File.AppendAllText(baseline, "tamper");
                Assert.That(AuditionPvStationBossDeathAftermathGoldenRunner
                    .IsValidCommittedManifestAtForTests(
                        state.outputDirectory,
                        CaptureId,
                        root,
                        state), Is.False);
                File.WriteAllBytes(baseline, baselineBytes);

                string manifestPath = Path.Combine(
                    state.outputDirectory,
                    AuditionPvCaptureContract.ManifestFileName);
                string manifestJson = File.ReadAllText(manifestPath);
                AuditionPvCaptureManifest manifest = JsonUtility.FromJson<
                    AuditionPvCaptureManifest>(manifestJson);
                manifest.shots[0].notes = "semantic substitution";
                File.WriteAllText(manifestPath, JsonUtility.ToJson(manifest, true));
                Assert.That(AuditionPvStationBossDeathAftermathGoldenRunner
                    .IsValidCommittedManifestAtForTests(
                        state.outputDirectory,
                        CaptureId,
                        root,
                        state), Is.False);
                File.WriteAllText(manifestPath, manifestJson);

                string originalCommit = state.gitCommitSha;
                state.gitCommitSha = new string('b', 40);
                Assert.That(AuditionPvStationBossDeathAftermathGoldenRunner
                    .IsValidCommittedManifestAtForTests(
                        state.outputDirectory,
                        CaptureId,
                        root,
                        state), Is.False);
                state.gitCommitSha = originalCommit;
                Assert.That(AuditionPvStationBossDeathAftermathGoldenRunner
                    .IsValidCommittedManifestAtForTests(
                        state.outputDirectory,
                        CaptureId,
                        root,
                        state), Is.True);
            }
            finally
            {
                DeleteTempRoot(root);
            }
        }

        [Test]
        public void DependenciesAndBatchContract_AreHeadfulAndPinCaptureClosure()
        {
            Assert.DoesNotThrow(() =>
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .ValidateBatchCommandLine(new[] { "Unity", "-noaudio" }));
            foreach (string forbidden in new[] { "-batchmode", "-quit", "-nographics" })
            {
                Assert.Throws<InvalidOperationException>(() =>
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .ValidateBatchCommandLine(
                            new[] { "Unity", "-noaudio", forbidden }));
            }

            string[] dependencies = AuditionPvStationBossDeathAftermathGoldenRunner
                .CollectCaptureDependencyPaths();
            Assert.That(dependencies, Does.Contain(
                "Assets/_Game/Prefabs/Combat/PF_PlayerRangedBasicProjectile_AimBolt.prefab"));
            Assert.That(dependencies, Does.Not.Contain(
                "Assets/_Game/Prefabs/Combat/PF_PlayerRangedProjectile_ActionFoundation.prefab"));
            Assert.That(dependencies, Does.Contain(
                "Assets/_Game/Art/Characters/Bosses/Akaza/Animations/DB_Akaza_Phase2Boss.controller"));
            Assert.That(dependencies, Does.Not.Contain(
                "Assets/_Game/Animations/Controllers/DB_Akaza_Phase2Boss.controller"));
            Assert.That(dependencies, Does.Contain(
                AuditionPvStationBossDeathAftermathGoldenRunner.RunnerScriptPath));
            Assert.That(dependencies, Does.Contain(
                AuditionPvStationBossDeathAftermathGoldenRunner.RunnerTestPath));
            Assert.That(dependencies, Does.Contain(
                AuditionPvStationBossDeathAftermathGoldenRunner.ReadmePath));
            Assert.That(dependencies, Does.Contain(
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .ExpectedRenderPipelineAssetPath));
            Assert.That(dependencies, Does.Contain(
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .ExpectedRenderPipelineAssetPath + ".meta"));
            Assert.That(dependencies, Does.Contain(
                "Assets/_Game/Scripts/Presentation/PresentationClock.cs"));
            Assert.That(dependencies, Does.Contain(
                "Assets/_Game/Scripts/Presentation/PresentationClock.cs.meta"));
            foreach (string finisherDependency in new[]
                     {
                         "Assets/_Game/Scripts/Presentation/OlympusStationBossTerminalFinisherCameraController.cs",
                         "Assets/_Game/DesignData/Timelines/Cinematics/DB_Timeline_OlympusStationBossTerminalFinisher.playable",
                         "Assets/_Game/DesignData/Timelines/Cinematics/DB_Anim_OlympusStationBossTerminalFinisherCamera.anim",
                         "Assets/_Game/Scripts/Combat/BossBarrageEncounterController.cs",
                         "Assets/_Game/Scripts/LevelDesign/FrontlineWaveStageProfile.cs",
                         "Assets/_Game/UI/CombatHud/CombatHudPresenter.cs",
                         "Assets/_Game/UI/CombatHud/BossBarrageLaneReviewCombatHudBinder.cs",
                          "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_FrontlineWaveStage_MotivationReview.asset",
                          "Assets/_Game/Editor/OlympusContinuousStageSetup.cs",
                          "Assets/_Game/Editor/OlympusStationAkazaPhase2Setup.cs",
                          "Assets/_Game/Editor/RuntimeSceneWiringReadinessReporter.cs"
                     })
            {
                Assert.That(dependencies, Does.Contain(finisherDependency));
                Assert.That(dependencies, Does.Contain(finisherDependency + ".meta"));
            }
            Assert.That(dependencies, Does.Contain(
                "Assets/_Game/Scripts/LevelDesign/StageRunFinalization.cs"));
            Assert.That(dependencies, Does.Contain(
                "Assets/_Game/Scripts/LevelDesign/StageRunFinalization.cs.meta"));
            foreach (string partial in new[]
                     {
                         "Assets/_Game/Scripts/Presentation/ActionCinematicCueDirector.cs",
                         "Assets/_Game/Scripts/Presentation/ActionCinematicCueDirector.Timing.cs",
                         "Assets/_Game/Scripts/Presentation/ActionCinematicCueDirector.Camera.cs",
                         "Assets/_Game/Scripts/Presentation/ActionCinematicCueDirector.Signals.cs",
                         "Assets/_Game/Scripts/Presentation/ActionCinematicCueDirector.Bindings.cs"
                     })
            {
                Assert.That(dependencies, Does.Contain(partial));
                Assert.That(dependencies, Does.Contain(partial + ".meta"));
            }

            Assert.That(dependencies.Distinct(StringComparer.OrdinalIgnoreCase).Count(),
                Is.EqualTo(dependencies.Length));
            AuditionPvDependencyHash[] hashes =
                AuditionPvEnvironmentProbe.HashDependencies(dependencies);
            Assert.That(hashes.Length, Is.EqualTo(dependencies.Length));
            Assert.That(
                hashes.Select(value => value.path).ToArray(),
                Is.EqualTo(dependencies),
                "Collected dependency paths must already be the exact canonical "
                + "project/package/meta path set emitted by HashDependencies.");
            Assert.That(hashes.All(value => value != null
                && value.exists
                && value.byteLength >= 0
                && AuditionPvSha256.IsSha256(value.sha256)), Is.True);
        }

        [Test]
        public void ProductPresentationSchedule_IsBoundToTheManualClockAndExactResultFrames()
        {
            string aftermath = ReadProjectFile(
                "Assets/_Game/Scripts/LevelDesign/OlympusStationBossTerminalAftermathPresenter.cs");
            string overlay = ReadProjectFile(
                "Assets/_Game/Scripts/LevelDesign/OlympusStageClearOverlay.cs");
            string result = ReadProjectFile(
                "Assets/_Game/Scripts/UI/StageClear/StageClearScreenPresenter.cs");
            string motion = ReadProjectFile(
                "Assets/_Game/Scripts/Presentation/AkazaPhase2CombatMotionDriver.cs");
            string capture = ReadProjectFile(
                AuditionPvStationBossDeathAftermathCapture.CaptureScriptPath);

            Assert.That(Count(aftermath, "PresentationClock.UnscaledDeltaTime"),
                Is.EqualTo(2));
            Assert.That(Count(overlay, "PresentationClock.UnscaledTime"),
                Is.GreaterThanOrEqualTo(4));
            Assert.That(overlay, Does.Contain(
                "elapsed += PresentationClock.UnscaledDeltaTime"));
            Assert.That(overlay, Does.Contain(
                "AftermathHandoffImminent +="));
            Assert.That(overlay, Does.Contain(
                "AftermathCompleted += HandleAftermathCompleted"));
            Assert.That(overlay, Does.Contain(
                "HandleAftermathHandoffImminent"));
            Assert.That(overlay, Does.Contain(
                "TryRequestStageClearSceneLoad"));
            Assert.That(overlay, Does.Contain(
                "CancelOwnedResultSceneLoad"));
            Assert.That(overlay, Does.Contain(
                "SceneManager.sceneLoaded += HandleSceneLoaded"));
            Assert.That(overlay, Does.Contain(
                "operation.completed += _ => CompleteUnload(token)"));
            Assert.That(overlay, Does.Contain(
                "ResultScenePreloadLease.IsBusy"));
            Assert.That(overlay, Does.Contain(
                "ResolveRequestedResultScene()"));
            Assert.That(overlay, Does.Contain(
                "presentationFailureFinalized"));
            Assert.That(result, Does.Not.Contain("WaitForSecondsRealtime"));
            Assert.That(Count(result, "PresentationClock.UnscaledDeltaTime"),
                Is.EqualTo(2));
            Assert.That(motion, Does.Contain(
                "TickPresentation(PresentationClock.UnscaledDeltaTime);"));
            Assert.That(aftermath, Does.Contain(
                "SignalHandoffImminent();"));

            Assert.That(capture, Does.Contain(
                "TerminalEpochClosureRecord closure ="));
            Assert.That(capture, Does.Contain(
                "HudYieldedAtResult = !combatHud.gameObject.activeInHierarchy;"));
            Assert.That(capture, Does.Contain(
                "bossDeathUsedPhaseTwoAnchorAtImpact ="));
        }

        [Test]
        public void CaptureSubscriptionsAndLateRenderOrdering_PinPhysicalF62EvidenceOwners()
        {
            string source = ReadProjectFile(
                AuditionPvStationBossDeathAftermathCapture.CaptureScriptPath);
            Assert.That(source, Does.Contain(
                "ranged.RangedProjectileFired += HandleProjectileFired"));
            Assert.That(source, Does.Contain(
                "projectile.DamageApplied += HandleProjectileDamageApplied"));
            Assert.That(source, Does.Contain("bossHealth.Died += HandleBossDied"));
            Assert.That(source, Does.Contain(
                "projectile != firedProjectile"));
            Assert.That(source, Does.Contain(
                "projectile.GetInstanceID() != projectileInstanceId"));
            Assert.That(source, Does.Contain(
                "projectilePositionAtFrame61 = firedProjectile.transform.position"));
            Assert.That(source, Does.Contain(
                "Vector3.Distance(projectileSpawnPosition, projectilePositionAtFrame61)"));
            Assert.That(source, Does.Contain(
                "projectileImpactSequence <= bossDiedSequence"));

            var runnerOrder = (DefaultExecutionOrder)Attribute.GetCustomAttribute(
                typeof(AuditionPvStationBossDeathAftermathGoldenRunnerBehaviour),
                typeof(DefaultExecutionOrder));
            var directorOrder = (DefaultExecutionOrder)Attribute.GetCustomAttribute(
                typeof(AuditionPvStationBossDeathAftermathDirector),
                typeof(DefaultExecutionOrder));
            var probeOrder = (DefaultExecutionOrder)Attribute.GetCustomAttribute(
                typeof(AuditionPvStationBossDeathAftermathRenderProbe),
                typeof(DefaultExecutionOrder));
            var cameraOrder = (DefaultExecutionOrder)Attribute.GetCustomAttribute(
                typeof(DimensionBrawl.Presentation.ActionCameraController),
                typeof(DefaultExecutionOrder));
            var motionOrder = (DefaultExecutionOrder)Attribute.GetCustomAttribute(
                typeof(DimensionBrawl.Presentation.AkazaPhase2CombatMotionDriver),
                typeof(DefaultExecutionOrder));
            Assert.That(runnerOrder.order, Is.EqualTo(-32500));
            Assert.That(directorOrder.order, Is.EqualTo(-32000));
            Assert.That(probeOrder.order, Is.EqualTo(32000));
            Assert.That(probeOrder.order, Is.GreaterThan(cameraOrder.order));
            Assert.That(probeOrder.order, Is.GreaterThan(motionOrder.order));

            string runner = ReadProjectFile(
                AuditionPvStationBossDeathAftermathGoldenRunner.RunnerScriptPath);
            Assert.That(runner, Does.Contain("renderer.BakeMesh(bakedCoreMesh)"));
            Assert.That(runner, Does.Contain(
                "localToWorld.MultiplyPoint3x4(localVertex)"));
            Assert.That(runner, Does.Contain(
                "result.projectionAspect = camera.aspect"));
            Assert.That(runner, Does.Contain(
                "bakedCoreMesh ??= new Mesh"));
            Assert.That(runner, Does.Contain("ReleaseBakedCoreMesh()"));
        }

        [Test]
        public void TransactionSource_IsPixelThenVisualAcceptanceFirstAndManifestLast()
        {
            string source = ReadProjectFile(
                AuditionPvStationBossDeathAftermathGoldenRunner.RunnerScriptPath);
            int finalize = source.IndexOf(
                "private static void FinalizeSuccessfulCapture",
                StringComparison.Ordinal);
            int calibration = source.IndexOf(
                "if (!PixelCalibrationLocked)",
                finalize,
                StringComparison.Ordinal);
            int baselineWrite = source.IndexOf(
                "CopyBaselines(state, frames, proof)",
                finalize,
                StringComparison.Ordinal);
            int visualAcceptance = source.IndexOf(
                "ValidateRuntimeProofForPublication(proof)",
                calibration,
                StringComparison.Ordinal);
            int manifestWrite = source.IndexOf(
                "AuditionPvCaptureManifestWriter.WriteNew(manifest);",
                finalize,
                StringComparison.Ordinal);
            int nextMethod = source.IndexOf(
                "private static void AnalyzeFrames",
                manifestWrite,
                StringComparison.Ordinal);
            Assert.That(finalize, Is.GreaterThanOrEqualTo(0));
            Assert.That(calibration, Is.GreaterThan(finalize));
            Assert.That(visualAcceptance, Is.GreaterThan(calibration));
            Assert.That(baselineWrite, Is.GreaterThan(visualAcceptance));
            Assert.That(manifestWrite, Is.GreaterThan(baselineWrite));
            Assert.That(nextMethod, Is.GreaterThan(manifestWrite));
            string afterCommit = source.Substring(
                manifestWrite
                    + "AuditionPvCaptureManifestWriter.WriteNew(manifest);".Length,
                nextMethod - manifestWrite
                    - "AuditionPvCaptureManifestWriter.WriteNew(manifest);".Length);
            Assert.That(afterCommit, Does.Not.Contain("Write"));
            Assert.That(afterCommit, Does.Not.Contain("SaveState"));

            int resume = source.IndexOf(
                "private static void ResumeOwnedSession",
                StringComparison.Ordinal);
            int manifestRecovery = source.IndexOf(
                "IsValidCommittedManifestAt(",
                resume,
                StringComparison.Ordinal);
            int terminalFault = source.IndexOf(
                "SessionTerminalFaultKey",
                resume,
                StringComparison.Ordinal);
            Assert.That(manifestRecovery, Is.GreaterThan(resume));
            Assert.That(terminalFault, Is.GreaterThan(manifestRecovery));
        }

        [Test]
        public void RuntimeProof_RejectsEveryCanonicalTimelineAndLifecycleSubstitution()
        {
            AssertRuntimeMutation(value => value.fireFrame = 2);
            AssertRuntimeMutation(value => value.projectileImpactFrame = 61);
            AssertRuntimeMutation(value => value.bossDiedFrame = 63);
            AssertRuntimeMutation(value => value.pressureScreensBeforeDismiss = -1);
            AssertRuntimeMutation(value => value.pressureSummonsDismissed = -1);
            AssertRuntimeMutation(value =>
            {
                value.pressureScreensBeforeDismiss = 2;
                value.pressureSummonsDismissed = 1;
            });
            AssertRuntimeMutation(value => value.pressureScreensAfterDismiss = 1);
            AssertRuntimeMutation(value => value.predictedNaturalImpactFrame = 63);
            AssertRuntimeMutation(value => value.predictedBossSweepDistance = 24.5f);
            AssertRuntimeMutation(value =>
                value.preShotPlayerPlanarStepDistance =
                    AuditionPvStationBossDeathAftermathCapture
                        .MaximumNaturalImpactTotalStepMeters + 0.001f);
            AssertRuntimeMutation(value =>
                value.projectileConfiguredLocalRadius = 0.32f);
            AssertRuntimeMutation(value =>
                value.projectileConfiguredWorldRadius = 0f);
            AssertRuntimeMutation(value =>
                value.projectilePrefabAssetPath =
                    "Assets/_Game/Prefabs/Combat/PF_Unrelated.prefab");
            AssertRuntimeMutation(value =>
                value.projectilePrefabAssetGuid =
                    "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
            AssertRuntimeMutation(value =>
                value.projectilePrefabLocalScale =
                    new Vector3(0.29f, 0.28f, 0.28f));
            AssertRuntimeMutation(value =>
                value.projectileRootLossyScale =
                    new Vector3(2f, 1f, 1f));
            AssertRuntimeMutation(value =>
                value.projectileObservedLocalRadius = float.NaN);
            AssertRuntimeMutation(value =>
                value.projectileObservedWorldRadius = float.NaN);
            AssertRuntimeMutation(value =>
                value.projectileObservedLossyScale =
                    new Vector3(float.NaN, 0.28f, 0.28f));
            AssertRuntimeMutation(value =>
            {
                value.projectilePrefabLocalScale =
                    new Vector3(0.29f, 0.29f, 0.29f);
                value.projectileConfiguredWorldRadius = 0.0899f;
                value.projectileObservedLossyScale =
                    new Vector3(0.29f, 0.29f, 0.29f);
                value.projectileObservedWorldRadius = 0.0899f;
            });
            AssertRuntimeMutation(value =>
            {
                value.projectileRootLossyScale = new Vector3(2f, 2f, 2f);
                value.projectileConfiguredWorldRadius = 0.1736f;
                value.projectileObservedLossyScale =
                    new Vector3(0.56f, 0.56f, 0.56f);
                value.projectileObservedWorldRadius = 0.1736f;
            });
            AssertRuntimeMutation(value =>
            {
                value.projectilePrefabAssetPath =
                    "Assets/_Game/Prefabs/Combat/PF_Unrelated.prefab";
                value.projectilePrefabAssetGuid =
                    "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            });
            AssertRuntimeMutation(value => value.bossPressureMovementWasEnabled = false);
            AssertRuntimeMutation(value => value.bossPressureMovementHoldAcquired = false);
            AssertRuntimeMutation(value => value.bossPoseStableThroughImpact = false);
            AssertRuntimeMutation(value => value.bossPositionAtImpact += Vector3.right);
            AssertRuntimeMutation(value =>
                value.maximumBossRotationDriftThroughImpact = 0.01f);
            AssertRuntimeMutation(value => value.aftermathCompletedFrame = 217);
            AssertRuntimeMutation(value => value.inputLeaseReleasedFrame = 219);
            AssertRuntimeMutation(value => value.firstFreezeFrame = 217);
            AssertRuntimeMutation(value => value.firstResultSceneFrame = 219);
            AssertRuntimeMutation(value => value.firstInteractiveFrame = 245);
            AssertRuntimeMutation(value => value.allEightLocksObservedAtImpact = false);
            AssertRuntimeMutation(value => value.allEightLocksReleasedAtResult = false);
            AssertRuntimeMutation(value => value.exclusiveCameraScheduleExact = false);
            AssertRuntimeMutation(value => value.cameraRoleTransitionCount = 1);
            AssertRuntimeMutation(value => value.firstFinisherCameraFrame = 63);
            AssertRuntimeMutation(value => value.firstGameplayCameraRestoreFrame = 245);
            AssertRuntimeMutation(value => value.finisherTerminalHoldExactAt218 = false);
            AssertRuntimeMutation(value => value.finisherReleaseExactAt246 = false);
            AssertRuntimeMutation(value => value.finisherCameraSucceeded = false);
            AssertRuntimeMutation(value => value.finisherCameraReleaseScheduled = false);
            AssertRuntimeMutation(value => value.finisherCameraInterrupted = true);
            AssertRuntimeMutation(value => value.fallbackCameraCueSucceeded = true);
            AssertRuntimeMutation(value => value.finisherCameraRequestVersion = 2);
            AssertRuntimeMutation(value => value.finisherCameraAcquireCount = 2);
            AssertRuntimeMutation(value => value.finisherCameraReleaseCount = 0);
            AssertRuntimeMutation(value =>
                value.finisherCameraControllerRequestVersion = 2);
            AssertRuntimeMutation(value => value.finisherCameraSampleCount = 155);
            AssertRuntimeMutation(value =>
                value.finisherCameraResultCoverReleaseSampleCount = 27);
            AssertRuntimeMutation(value => value.finisherCameraLastSampledSeconds = 2.59d);
            AssertRuntimeMutation(value =>
                value.finisherCameraResultCoverReleaseElapsedSeconds = 0.45f);
            AssertRuntimeMutation(value => value.finisherCameraReachedTerminalSample = false);
            AssertRuntimeMutation(value => value.finisherCameraLeaseReleased = false);
            AssertRuntimeMutation(value => value.finisherCameraGameplayRestored = false);
            AssertRuntimeMutation(value => value.finisherCameraDisabledAtResult = false);
            AssertRuntimeMutation(value => value.bossDeathCameraRequestCount = 1);
            AssertRuntimeMutation(value => value.bossDeathCameraVersion = 0);
            AssertRuntimeMutation(value => value.bossDeathCameraInterrupted = true);
            AssertRuntimeMutation(value => value.bossDeathCameraComplete = true);
            AssertRuntimeMutation(value => value.bossDeathVfxRequestCount = 2);
            AssertRuntimeMutation(value => value.bossDeathAudioSourceDelta = 0);
            AssertRuntimeMutation(value => value.bossDeathUsesPhaseTwoAnchor = false);
            AssertRuntimeMutation(value => value.deathMotionRequestCount = 2);
            AssertRuntimeMutation(value => value.resultSummarySameInstance = false);
            AssertRuntimeMutation(value => value.presentedSummarySameInstance = false);
            AssertRuntimeMutation(value => value.eventsReleased = false);
            AssertRuntimeMutation(value => value.bossPressureMovementRestored = false);
            AssertRuntimeMutation(value => value.editModeGlobalCleanupExact = false);
            AssertRuntimeMutation(value => value.pocketClearMarkerReferenceUnbound = false);
            AssertRuntimeMutation(value => value.pocketClearMarkerInactiveAtEnd = false);
            AssertRuntimeMutation(value => value.terminalBoundaryVisualHiddenAtEnd = false);
            AssertRuntimeMutation(value => value.renderEvidence[0].frame = 60);
            AssertRuntimeMutation(value => value.renderEvidence[0].gameplayCameraExact = false);
            AssertRuntimeMutation(value => value.renderEvidence[0].combatHudVisible = false);
            AssertRuntimeMutation(value => value.renderEvidence[0].bossEnvelopeVisible = false);
            AssertRuntimeMutation(value => value.renderEvidence[0]
                .bossEnvelopeFullyOutsideFrustum = true);
            AssertRuntimeMutation(value => value.renderEvidence[0].objectiveText =
                "Build EN for SummonSlot1");
            AssertRuntimeMutation(value =>
                value.renderEvidence[0].objectiveForbiddenInternalTokensAbsent = false);
            AssertRuntimeMutation(value => value.renderEvidence[0].bossLabelText =
                "ARCHON PROXY");
            AssertRuntimeMutation(value =>
                value.renderEvidence[0].pocketClearMarkerInactive = false);
            AssertRuntimeMutation(value => value.renderEvidence[1].finisherCameraExact = false);
            AssertRuntimeMutation(value => value.renderEvidence[1].combatHudVisible = false);
            AssertRuntimeMutation(value => value.renderEvidence[1].objectiveText =
                "Build EN for SummonSlot1");
            AssertRuntimeMutation(value => value.renderEvidence[1].bossLabelText =
                "ARCHON PROXY");
            AssertRuntimeMutation(value => value.renderEvidence[1].bossBodyHeightRatio =
                0.249f);
            AssertRuntimeMutation(value => value.renderEvidence[1].bossBodyHeightRatio =
                0.401f);
            AssertRuntimeMutation(value => value.renderEvidence[1]
                .bossBodyRendererNames =
                    "DB_AkazaPhase2Combined_WingSilhouette|DB_AkazaPhase2Combined_FaceHairDetail");
            AssertRuntimeMutation(value => value.renderEvidence[1]
                .bossBodyRendererCount = 3);
            AssertRuntimeMutation(value => value.renderEvidence[1]
                .bossBodyMaxExtentRatio = 0.33f);
            AssertRuntimeMutation(value => value.renderEvidence[1].bossSafeViewport = false);
            AssertRuntimeMutation(value => value.renderEvidence[1].bossPartiallyClipped = true);
            AssertRuntimeMutation(value => value.renderEvidence[1]
                .bossEnvelopeVisible = false);
            AssertRuntimeMutation(value => value.renderEvidence[1]
                .projectionAspect = 1f);
            AssertRuntimeMutation(value => value.renderEvidence[0]
                .projectionAspect = float.NaN);
            AssertRuntimeMutation(value => value.renderEvidence[1].playerFullyOutsideFrustum =
                false);
            AssertRuntimeMutation(value =>
            {
                value.renderEvidence[1].playerFullyOutsideFrustum = false;
                value.renderEvidence[1].playerFullyInsideFrustum = true;
                value.renderEvidence[1].playerSafeViewport = true;
                value.renderEvidence[1].playerBodyHeightRatio = 0.249f;
            });
            AssertRuntimeMutation(value =>
            {
                value.renderEvidence[1].playerFullyOutsideFrustum = false;
                value.renderEvidence[1].playerFullyInsideFrustum = true;
                value.renderEvidence[1].playerSafeViewport = false;
                value.renderEvidence[1].playerBodyHeightRatio = 0.28f;
            });
            AssertRuntimeMutation(value => value.renderEvidence[1]
                .terminalBoundaryVisualHidden = false);
            AssertRuntimeMutation(value => value.renderEvidence[2].combatHudVisible = true);
            AssertRuntimeMutation(value =>
            {
                value.renderEvidence[2].bossBodyWidthRatio =
                    0.249f / (16f / 9f);
                value.renderEvidence[2].bossBodyMaxExtentRatio = 0.249f;
            });
            AssertRuntimeMutation(value =>
            {
                value.renderEvidence[2].bossBodyWidthRatio =
                    0.401f / (16f / 9f);
                value.renderEvidence[2].bossBodyMaxExtentRatio = 0.401f;
            });
            AssertRuntimeMutation(value =>
            {
                value.renderEvidence[3].bossBodyWidthRatio =
                    (value.renderEvidence[2].bossBodyMaxExtentRatio + 0.051f)
                    / (16f / 9f);
                value.renderEvidence[3].bossBodyMaxExtentRatio =
                    value.renderEvidence[2].bossBodyMaxExtentRatio + 0.051f;
            });
            AssertRuntimeMutation(value => value.renderEvidence[2]
                .bossCoreAxisSource = "motion-driver-flag");
            AssertRuntimeMutation(value =>
            {
                value.renderEvidence[2].bossCoreAxisHeadViewport =
                    value.renderEvidence[2].bossCoreAxisHipsViewport
                    + new Vector3(0f, 0.16f, 0f);
                value.renderEvidence[2].bossCoreAxisViewportLength = 0.16f;
            });
            AssertRuntimeMutation(value =>
            {
                value.renderEvidence[3].bossCoreAxisHeadViewport =
                    value.renderEvidence[3].bossCoreAxisHipsViewport
                    + new Vector3(0f, 0.16f, 0f);
                value.renderEvidence[3].bossCoreAxisViewportLength = 0.16f;
            });
            AssertRuntimeMutation(value =>
            {
                var heldAxis = new Vector2(0.13f, 0.10f);
                value.renderEvidence[3].bossCoreAxisHeadViewport =
                    value.renderEvidence[3].bossCoreAxisHipsViewport
                    + new Vector3(heldAxis.x, heldAxis.y, 0f);
                value.renderEvidence[3].bossCoreAxisViewportLength =
                    new Vector2(
                        heldAxis.x * (16f / 9f),
                        heldAxis.y).magnitude;
            });
            AssertRuntimeMutation(value => value.renderEvidence[2]
                .bossCoreAxisViewportLength = 0.01f);
            AssertRuntimeMutation(value => value.renderEvidence[3].bossViewport =
                value.renderEvidence[1].bossViewport + new Vector3(0.081f, 0f, 0f));
            AssertRuntimeMutation(value => value.renderEvidence[4].finisherLeaseReleased = false);
            AssertRuntimeMutation(value => value.renderEvidence[4]
                .redundantClearTextInactive = false);
            AssertRuntimeMutation(value => value.renderEvidence[4].realClearIconActive = false);
            AssertRuntimeMutation(value => value.renderEvidence[4]
                .terminalBoundaryVisualHidden = false);
        }

        private static void AssertRuntimeMutation(
            Action<AuditionPvStationBossDeathAftermathGoldenRunner.RuntimeProof> mutate)
        {
            AuditionPvStationBossDeathAftermathGoldenRunner.RuntimeProof proof =
                CreateValidProof();
            mutate(proof);
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .ValidateRuntimeProofBeforePixelCalibration(proof));
        }

        private static IEnumerator ThrowingIteratorRoot(
            Action onDispose,
            Action onMiddleDispose,
            IEnumerator leaf)
        {
            try
            {
                yield return ThrowingIteratorMiddle(
                    onMiddleDispose,
                    leaf);
            }
            finally
            {
                onDispose?.Invoke();
            }
        }

        private static IEnumerator ThrowingIteratorMiddle(
            Action onDispose,
            IEnumerator leaf)
        {
            try
            {
                yield return leaf;
            }
            finally
            {
                onDispose?.Invoke();
            }
        }

        private sealed class ThrowingMoveNextAndDisposeIterator
            : IEnumerator, IDisposable
        {
            private readonly Action onDispose;
            private bool yielded;
            private bool disposed;

            internal ThrowingMoveNextAndDisposeIterator(Action onDispose)
            {
                this.onDispose = onDispose;
            }

            public object Current => null;

            public bool MoveNext()
            {
                if (!yielded)
                {
                    yielded = true;
                    return true;
                }

                throw new InvalidOperationException(
                    "depth-two-move-sentinel");
            }

            public void Reset()
            {
                throw new NotSupportedException();
            }

            public void Dispose()
            {
                if (disposed)
                {
                    return;
                }

                disposed = true;
                onDispose?.Invoke();
                throw new InvalidOperationException(
                    "depth-two-dispose-sentinel");
            }
        }

        private static IEnumerator CleanupIteratorRoot(
            Action onComplete,
            Action onDispose)
        {
            try
            {
                yield return CleanupIteratorLeaf(onComplete, onDispose);
            }
            finally
            {
                onDispose?.Invoke();
            }
        }

        private static IEnumerator CleanupIteratorLeaf(
            Action onComplete,
            Action onDispose)
        {
            try
            {
                yield return null;
                onComplete?.Invoke();
            }
            finally
            {
                onDispose?.Invoke();
            }
        }

        private static AuditionPvStationBossDeathAftermathGoldenRunner.RuntimeProof
            CreateValidProof()
        {
            return new AuditionPvStationBossDeathAftermathGoldenRunner.RuntimeProof
            {
                directorCompleted = true,
                lastLogicalFrame = 359,
                presentedFrameCount = 360,
                presentedFramesExact = true,
                presentationClockExact = true,
                recorderWarmupEndOfFrameCount = 2,
                recorderAutoStoppedAfterLastFrame = true,
                runId = "run",
                playableStageId = "olympus-station",
                routeRevision = 1,
                routeDigest = "route-digest",
                transitionTokenId = "token",
                transitionTokenDigest = "token-digest",
                loaderGeneration = 1,
                segmentEntryReceiptId = "entry",
                segmentEntryReceiptDigest = "entry-digest",
                handoffTerminalReceiptId = "terminal",
                handoffTerminalReceiptDigest = "terminal-digest",
                enteredFromHandoffPending = true,
                exactHandoffReceiptChain = true,
                productTransitionProviderObserved = true,
                productTransitionDestinationArrived = true,
                productTransitionHandoffCompleted = true,
                productTransitionGeneration = 1,
                entryGuideObservedPlaying = true,
                entryGuideReleased = true,
                phaseTransitionStartCount = 1,
                phaseTransitionCompletionCount = 1,
                phaseTwoApplied = true,
                preparedHealth = 12f,
                bossHealthBeforeShot = 12f,
                pressureScreensBeforeDismiss = 0,
                pressureSummonsDismissed = 0,
                pressureScreensAfterDismiss = 0,
                predictedBossSweepDistance = 24.2f,
                predictedNaturalImpactFrame = 62,
                preShotPlayerPlanarStepDistance = 1.2f,
                projectileConfiguredLocalRadius = 0.31f,
                projectileConfiguredWorldRadius = 0.0868f,
                projectilePrefabLocalScale = new Vector3(0.28f, 0.28f, 0.28f),
                projectileRootLossyScale = Vector3.one,
                projectilePrefabAssetPath =
                    "Assets/_Game/Prefabs/Combat/PF_PlayerRangedBasicProjectile_AimBolt.prefab",
                projectilePrefabAssetGuid =
                    "404ed7d823e769c45871b221fe7e3c95",
                projectileObservedLocalRadius = 0.31f,
                projectileObservedWorldRadius = 0.0868f,
                projectileObservedLossyScale =
                    new Vector3(0.28f, 0.28f, 0.28f),
                bossPressureMovementWasEnabled = true,
                bossPressureMovementHoldAcquired = true,
                bossPoseStableThroughImpact = true,
                bossPositionAtShotArm = new Vector3(0f, 1f, 16f),
                bossPositionAtImpact = new Vector3(0f, 1f, 16f),
                maximumBossPositionDriftThroughImpact = 0f,
                maximumBossRotationDriftThroughImpact = 0f,
                fireFrame = 1,
                projectileFiredFrame = 1,
                bossDiedFrame = 62,
                projectileImpactFrame = 62,
                terminalResolvedFrame = 62,
                firstFreezeFrame = 218,
                firstResultSceneFrame = 218,
                firstResultConfiguredFrame = 218,
                firstInteractiveFrame = 246,
                aftermathCompletedFrame = 218,
                inputLeaseReleasedFrame = 218,
                deathStateHeldFrame = 129,
                rangedFireStartedCount = 1,
                projectileFiredCount = 1,
                projectileDamageAppliedCount = 1,
                bossDamagedDuringShotCount = 1,
                bossDiedCount = 1,
                encounterTerminalResolvedCount = 1,
                overlayPresentationSucceededCount = 1,
                aftermathStartedCount = 1,
                aftermathCompletedCount = 1,
                projectileInstanceId = 42,
                projectileFiredSequence = 1,
                bossDiedSequence = 2,
                projectileImpactSequence = 3,
                terminalResolvedSequence = 4,
                projectileSpawnPosition = Vector3.zero,
                projectilePositionAtFrame61 = new Vector3(11f, 0f, 0f),
                projectileImpactPoint = new Vector3(12f, 1f, 0f),
                projectileImpactDirection = Vector3.right,
                physicalProjectileObservedActiveBeforeImpact = true,
                projectileMovedBeforeImpact = true,
                noEarlyFreeze = true,
                resultAbsentBeforeRequest = true,
                allEightLocksObservedAtImpact = true,
                allEightLocksReleasedAtResult = true,
                deathStateAtAftermathHero = true,
                aftermathCompletedSuccessfully = true,
                aftermathScaleOneObserved = true,
                aftermathScaleOneViolated = false,
                aftermathBeginCount = 1,
                aftermathCompleteCount = 1,
                aftermathElapsedSeconds = 2.6f,
                exclusiveCameraScheduleExact = true,
                cameraRoleTransitionCount = 2,
                firstFinisherCameraFrame = 62,
                firstGameplayCameraRestoreFrame = 246,
                finisherTerminalHoldExactAt218 = true,
                finisherReleaseExactAt246 = true,
                finisherCameraSucceeded = true,
                finisherCameraReleaseScheduled = true,
                finisherCameraInterrupted = false,
                fallbackCameraCueSucceeded = false,
                finisherCameraRequestVersion = 1,
                finisherCameraAcquireCount = 1,
                finisherCameraReleaseCount = 1,
                finisherCameraControllerRequestVersion = 1,
                finisherCameraSampleCount = 156,
                finisherCameraResultCoverReleaseSampleCount = 28,
                finisherCameraLastSampledSeconds = 2.6d,
                finisherCameraResultCoverReleaseElapsedSeconds = 28f / 60f,
                finisherCameraReachedTerminalSample = true,
                finisherCameraLeaseReleased = true,
                finisherCameraGameplayRestored = true,
                finisherCameraDisabledAtResult = true,
                bossDeathCameraRequestCount = 0,
                bossDeathCameraVersion = -1,
                bossDeathCameraInterrupted = false,
                bossDeathCameraComplete = false,
                bossDeathVfxRequestCount = 1,
                bossDeathAudioSourceDelta = 1,
                bossDeathUsesPhaseTwoAnchor = true,
                deathMotionRequestCount = 1,
                motionIsDead = true,
                motionAttacksStopped = true,
                animatorInDeathState = true,
                overlayShown = true,
                overlayFrozen = true,
                resultSummarySameInstance = true,
                presentedSummarySameInstance = true,
                committedSummaryDigest = "summary-digest",
                presentedSummaryDigest = "summary-digest",
                outcomeFactDigest = "fact-digest",
                rootAdmissionSequence = 1,
                terminalEpoch = 1,
                terminalEpochEvidenceDigest = "epoch-digest",
                terminalClosureDigest = "closure-digest",
                terminalRecordReceiptCount = 1,
                terminalFactsExact = true,
                hudWasActiveAtFire = true,
                hudWasActiveAtImpact = true,
                hudYieldedAtResult = true,
                resultInteractiveAt246 = true,
                pocketClearMarkerReferenceUnbound = true,
                pocketClearMarkerInactiveAtEnd = true,
                terminalBoundaryVisualHiddenAtEnd = true,
                stateRestored = true,
                eventsReleased = true,
                presentationClockReleased = true,
                cadenceReleased = true,
                bossPressureMovementRestored = true,
                transitionCaptureStateReleased = true,
                globalCaptureStateRestored = true,
                editModeSceneCleanupExact = true,
                editModeGlobalCleanupExact = true,
                cleanupFailure = string.Empty,
                renderEvidence = new[]
                {
                    GameplayHandleEvidence(),
                    FinisherEvidence(62, 0.32f, new Vector2(0.50f, 0.52f)),
                    FinisherEvidence(116, 0.31f, new Vector2(0.52f, 0.51f)),
                    FinisherEvidence(181, 0.30f, new Vector2(0.49f, 0.50f)),
                    ResultEvidence()
                },
                pixelSampleStride = AuditionPvStationBossDeathAftermathGoldenRunner
                    .SequencePixelSampleStride,
                pixelSampleCount = AuditionPvStationBossDeathAftermathGoldenRunner
                    .ExpectedSequencePixelSampleCount,
                sequenceBlackRatio = 0d,
                sequenceMagentaRatio = 0d,
                maximumFrameMagentaRatio = 0d,
                healthyFramePercent = 100d,
                frameDeltaPixelSampleStride =
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .FrameDeltaPixelSampleStride,
                frameDeltaPixelSampleCount =
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .ExpectedFrameDeltaPixelSampleCount,
                frameDeltaChangedRgbSumCutoff =
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .FrameDeltaChangedRgbSumCutoff,
                impactDeltaFromFrame = AuditionPvStationBossDeathAftermathGoldenRunner
                    .ImpactDeltaFromFrame,
                impactDeltaToFrame = AuditionPvStationBossDeathAftermathGoldenRunner
                    .ImpactDeltaToFrame,
                impactMeanAbsoluteRgb = 10d,
                impactChangedRatio = 0.5d,
                aftermathDeltaFromFrame =
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .AftermathDeltaFromFrame,
                aftermathDeltaToFrame = AuditionPvStationBossDeathAftermathGoldenRunner
                    .AftermathDeltaToFrame,
                aftermathEvolutionMeanAbsoluteRgb = 20d,
                aftermathEvolutionChangedRatio = 0.5d,
                resultAppearanceFromFrame =
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .ResultAppearanceFromFrame,
                resultAppearanceToFrame =
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .ResultAppearanceToFrame,
                resultAppearanceMeanAbsoluteRgb = 10d,
                resultAppearanceChangedRatio = 0.5d,
                resultEntranceFromFrame =
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .ResultEntranceFromFrame,
                resultEntranceToFrame = AuditionPvStationBossDeathAftermathGoldenRunner
                    .ResultEntranceToFrame,
                resultEntranceMeanAbsoluteRgb = 30d,
                resultEntranceChangedRatio = 0.5d,
                resultSurfaceFrame = AuditionPvStationBossDeathAftermathGoldenRunner
                    .ResultSurfaceFrame,
                resultSurfaceRoiX = AuditionPvStationBossDeathAftermathGoldenRunner
                    .ResultSurfaceRawBottomLeftRoi.x,
                resultSurfaceRoiY = AuditionPvStationBossDeathAftermathGoldenRunner
                    .ResultSurfaceRawBottomLeftRoi.y,
                resultSurfaceRoiWidth = AuditionPvStationBossDeathAftermathGoldenRunner
                    .ResultSurfaceRawBottomLeftRoi.width,
                resultSurfaceRoiHeight = AuditionPvStationBossDeathAftermathGoldenRunner
                    .ResultSurfaceRawBottomLeftRoi.height,
                resultSurfaceSampleStride =
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .ResultSurfaceSampleStride,
                resultSurfaceSampleCount =
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .ExpectedResultSurfaceSampleCount,
                resultBrightMinimumChannel =
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .ResultBrightMinimumChannel,
                resultNavyMaximumLuma =
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .ResultNavyMaximumLuma,
                resultBlueMinimumChannel =
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .ResultBlueMinimumChannel,
                resultBlueMinimumRedDelta =
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .ResultBlueMinimumRedDelta,
                resultBlueMinimumGreenDelta =
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .ResultBlueMinimumGreenDelta,
                resultBrightSamples = 70000,
                resultNavySamples = 600,
                resultBlueSamples = 70000,
                frameHashLedgerSha256 = ShaA,
                warmupEvidenceSha256 = ShaA,
                bl10Sha256 = ShaA,
                bl11Sha256 = ShaA,
                bl12Sha256 = ShaA,
                dependencyHashCount = 1,
                captureStartProvenanceSha256 = ShaA
            };
        }

        private static void AssertFinisherRigPose(
            PlayableDirector director,
            Transform rig,
            double time,
            Vector3 expectedLocalPosition,
            Quaternion expectedLocalRotation)
        {
            director.time = time;
            director.Evaluate();
            Assert.That(
                Vector3.Distance(rig.localPosition, expectedLocalPosition),
                Is.LessThanOrEqualTo(0.0001f),
                $"Station finisher rig local position drifted at t={time:R}s.");
            Assert.That(
                Quaternion.Angle(rig.localRotation, expectedLocalRotation),
                Is.LessThanOrEqualTo(0.001f),
                $"Station finisher rig local rotation drifted at t={time:R}s.");
            Assert.That(rig.localScale, Is.EqualTo(Vector3.one));
        }

        private static AuditionPvStationBossDeathAftermathGoldenRunner.RenderEvidence
            GameplayHandleEvidence()
        {
            return new AuditionPvStationBossDeathAftermathGoldenRunner.RenderEvidence
            {
                frame = 61,
                cameraRole = "gameplay",
                gameplayCameraExact = true,
                finisherCameraExact = false,
                exclusiveCameraRoleExact = true,
                combatHudVisible = true,
                projectionAspect = 16f / 9f,
                playerSafeViewport = true,
                bossSafeViewport = true,
                playerFullyInsideFrustum = true,
                bossFullyInsideFrustum = true,
                playerBodyHeightRatio = 0.28f,
                bossBodyRendererNames =
                    "DB_AkazaPhase2Combined_BodySilhouette|DB_AkazaPhase2Combined_FaceHairDetail",
                bossBodyRendererCount = 2,
                bossBodyWidthRatio = 0.08f,
                bossBodyHeightRatio = 0.12f,
                bossBodyMaxExtentRatio = 0.12f,
                bossEnvelopeVisible = true,
                bossEnvelopeFullyInsideFrustum = true,
                bossEnvelopeRendererCount = 4,
                bossEnvelopeWidthRatio = 0.18f,
                bossEnvelopeHeightRatio = 0.16f,
                bossEnvelopeMaxExtentRatio = 0.32f,
                objectiveText = AuditionPvStationBossDeathAftermathCapture
                    .ExpectedPlayerFacingKoObjective,
                bossLabelText = AuditionPvStationBossDeathAftermathCapture
                    .ExpectedBossDisplayName,
                objectiveForbiddenInternalTokensAbsent = true,
                pocketClearMarkerReferenceUnbound = true,
                pocketClearMarkerPresent = true,
                pocketClearMarkerInactive = true,
                playerViewport = new Vector3(0.25f, 0.5f, 10f),
                bossViewport = new Vector3(0.75f, 0.5f, 10f),
                bossEnvelopeViewport = new Vector3(0.75f, 0.5f, 10f),
                playerPixelExtent = new Vector2(100f, 200f),
                bossPixelExtent = new Vector2(205f, 173f),
                bossEnvelopePixelExtent = new Vector2(461f, 230f)
            };
        }

        private static AuditionPvStationBossDeathAftermathGoldenRunner.RenderEvidence
            FinisherEvidence(int frame, float bossExtent, Vector2 bossCenter)
        {
            const float ProjectionAspect = 16f / 9f;
            bool impact = frame == 62;
            float bodyWidth = impact ? 0.18f : bossExtent / ProjectionAspect;
            float bodyHeight = impact ? bossExtent : 0.13f;
            Vector3 hipsViewport = impact
                ? new Vector3(0.50f, 0.42f, 10f)
                : new Vector3(0.42f, frame == 116 ? 0.50f : 0.496f, 10f);
            Vector3 headViewport = impact
                ? new Vector3(0.50f, 0.58f, 10f)
                : new Vector3(0.58f, frame == 116 ? 0.50f : 0.504f, 10f);
            return new AuditionPvStationBossDeathAftermathGoldenRunner.RenderEvidence
            {
                frame = frame,
                cameraRole = "finisher",
                gameplayCameraExact = false,
                finisherCameraExact = true,
                exclusiveCameraRoleExact = true,
                combatHudVisible = frame == 62,
                projectionAspect = ProjectionAspect,
                playerFullyOutsideFrustum = true,
                playerFullyInsideFrustum = false,
                playerPartiallyClipped = false,
                bossFullyInsideFrustum = true,
                bossPartiallyClipped = false,
                bossSafeViewport = true,
                bossBodyRendererNames =
                    "DB_AkazaPhase2Combined_BodySilhouette|DB_AkazaPhase2Combined_FaceHairDetail",
                bossBodyRendererCount = 2,
                bossBodyWidthRatio = bodyWidth,
                bossBodyHeightRatio = bodyHeight,
                bossBodyMaxExtentRatio = Mathf.Max(
                    bodyWidth * ProjectionAspect,
                    bodyHeight),
                bossViewport = new Vector3(bossCenter.x, bossCenter.y, 10f),
                bossPixelExtent = new Vector2(
                    bodyWidth * 2560f,
                    bodyHeight * 1440f),
                bossEnvelopeVisible = true,
                bossEnvelopeFullyInsideFrustum = impact,
                bossEnvelopePartiallyClipped = !impact,
                bossEnvelopeRendererCount = 4,
                bossEnvelopeWidthRatio = impact ? 0.38f : 0.66f,
                bossEnvelopeHeightRatio = impact ? 0.37f : 0.44f,
                bossEnvelopeMaxExtentRatio = Mathf.Max(
                    (impact ? 0.38f : 0.66f) * ProjectionAspect,
                    impact ? 0.37f : 0.44f),
                bossEnvelopeViewport = new Vector3(
                    bossCenter.x,
                    bossCenter.y,
                    10f),
                bossEnvelopePixelExtent = new Vector2(
                    (impact ? 0.38f : 0.66f) * 2560f,
                    (impact ? 0.37f : 0.44f) * 1440f),
                bossCoreAxisSource = "akaza-generic-hip_C-to-head_C",
                bossCoreAxisHipsViewport = hipsViewport,
                bossCoreAxisHeadViewport = headViewport,
                bossCoreAxisViewportLength = new Vector2(
                    (headViewport.x - hipsViewport.x) * ProjectionAspect,
                    headViewport.y - hipsViewport.y).magnitude,
                objectiveText = frame == 62
                    ? AuditionPvStationBossDeathAftermathCapture
                        .ExpectedPlayerFacingKoObjective
                    : string.Empty,
                bossLabelText = frame == 62
                    ? AuditionPvStationBossDeathAftermathCapture
                        .ExpectedBossDisplayName
                    : string.Empty,
                objectiveForbiddenInternalTokensAbsent = frame == 62,
                pocketClearMarkerReferenceUnbound = true,
                pocketClearMarkerPresent = true,
                pocketClearMarkerInactive = true,
                terminalBoundaryVisualPresent = true,
                terminalBoundaryVisualHidden = true
            };
        }

        private static AuditionPvStationBossDeathAftermathGoldenRunner.RenderEvidence
            ResultEvidence()
        {
            return new AuditionPvStationBossDeathAftermathGoldenRunner.RenderEvidence
            {
                frame = 246,
                cameraRole = "gameplay",
                gameplayCameraExact = true,
                finisherCameraExact = false,
                exclusiveCameraRoleExact = true,
                finisherLeaseReleased = true,
                combatHudVisible = false,
                resultCanvasVisible = true,
                resultInteractive = true,
                pocketClearMarkerReferenceUnbound = true,
                pocketClearMarkerPresent = true,
                pocketClearMarkerInactive = true,
                redundantClearTextPresent = true,
                redundantClearTextInactive = true,
                realClearIconPresent = true,
                realClearIconActive = true,
                terminalBoundaryVisualPresent = true,
                terminalBoundaryVisualHidden = true
            };
        }

        private static AuditionPvStationBossDeathAftermathGoldenRunner.PersistedRunnerState
            CreateState(string root, string captureId, DateTime? startedAt = null)
        {
            DateTime time = (startedAt ?? DateTime.UtcNow).ToUniversalTime();
            string output = AuditionPvOutputPaths.ResolveOutputDirectory(root, captureId);
            return new AuditionPvStationBossDeathAftermathGoldenRunner.PersistedRunnerState
            {
                schema = "dimension-brawl.audition-pv.g08-runner-state.v1",
                phase = AuditionPvStationBossDeathAftermathGoldenRunner.RunnerPhase
                    .AwaitingEditMode.ToString(),
                startedAtUtc = time.ToString("O"),
                captureId = captureId,
                outputRoot = root,
                outputDirectory = output,
                baselineDirectory = Path.Combine(
                    output,
                    AuditionPvStationBossDeathAftermathCapture.BaselinesFolderName),
                gitCommitSha = new string('a', 40),
                gitBranch = "main",
                gitWorktreeDirty = false,
                gitDirtyHashSha256 = ShaA,
                engine = new AuditionPvEngineSnapshot
                {
                    unityVersion = AuditionPvStationBossDeathAftermathGoldenRunner
                        .ExpectedUnityVersion,
                    unityVersionWithRevision =
                        AuditionPvStationBossDeathAftermathGoldenRunner
                            .ExpectedUnityVersionWithRevision,
                    recorderPackageVersion = AuditionPvCaptureContract
                        .RecorderPackageVersion,
                    urpPackageVersion = AuditionPvStationBossDeathAftermathGoldenRunner
                        .ExpectedUrpPackageVersion,
                    activeRenderPipelineAssetPath =
                        AuditionPvStationBossDeathAftermathGoldenRunner
                            .ExpectedRenderPipelineAssetPath
                },
                dependencyPaths = Array.Empty<string>(),
                dependencyHashesAtStart = Array.Empty<AuditionPvDependencyHash>()
            };
        }

        private static void AssertEngineSnapshotExact(AuditionPvEngineSnapshot value)
        {
            Assert.That(value, Is.Not.Null);
            Assert.That(value.unityVersion, Is.EqualTo("6000.3.5f2"));
            Assert.That(value.unityVersionWithRevision,
                Is.EqualTo("6000.3.5f2 (3fa8bc678cb0)"));
            Assert.That(value.recorderPackageVersion, Is.EqualTo("5.1.6"));
            Assert.That(value.urpPackageVersion, Is.EqualTo("17.3.0"));
            Assert.That(value.activeRenderPipelineAssetPath,
                Is.EqualTo("Assets/Settings/PC_RPAsset.asset"));
        }

        private static AuditionPvStationBossDeathAftermathGoldenRunner.PersistedRunnerState
            WriteCommittedPackageFixture(
                string root,
                string captureId,
                DateTime started)
        {
            var state = CreateState(root, captureId, started);
            string output = state.outputDirectory;
            string frames = Path.Combine(output, "frames", "g08");
            string evidence = Path.Combine(output,
                AuditionPvStationBossDeathAftermathGoldenRunner.EvidenceFolderName);
            Directory.CreateDirectory(frames);
            Directory.CreateDirectory(evidence);
            Directory.CreateDirectory(state.baselineDirectory);
            byte[] frameBytes = FakeQhdPngHeader();
            for (int frame = 0;
                frame < AuditionPvStationBossDeathAftermathCapture.ExpectedFrameCount;
                frame++)
            {
                File.WriteAllBytes(
                    Path.Combine(
                        frames,
                        AuditionPvStationBossDeathAftermathCapture.FrameFileName(frame)),
                    frameBytes);
            }

            string warmup = Path.Combine(
                evidence,
                AuditionPvStationBossDeathAftermathGoldenRunner.WarmupEvidenceFileName);
            var texture = new Texture2D(
                AuditionPvCaptureContract.Width,
                AuditionPvCaptureContract.Height,
                TextureFormat.RGBA32,
                false,
                true);
            try
            {
                File.WriteAllBytes(warmup, texture.EncodeToPNG());
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(texture);
            }

            foreach (AuditionPvBaselineManifestEntry baseline in
                     AuditionPvStationBossDeathAftermathCapture
                         .CreateBaselineManifestEntries())
            {
                File.Copy(
                    Path.Combine(
                        frames,
                        AuditionPvStationBossDeathAftermathCapture.FrameFileName(
                            baseline.sourceFrame)),
                    Path.Combine(state.baselineDirectory, baseline.fileName));
            }

            var proof = CreateValidProof();
            string ledger = AuditionPvStationBossDeathAftermathGoldenRunner
                .BuildFrameHashLedger(frames);
            proof.frameHashLedgerPath = Path.Combine(
                evidence,
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .FrameHashLedgerFileName).Replace('\\', '/');
            File.WriteAllText(proof.frameHashLedgerPath, ledger);
            proof.frameHashLedgerSha256 = AuditionPvSha256.TextHash(ledger);
            proof.warmupEvidencePath = warmup.Replace('\\', '/');
            proof.warmupEvidenceSha256 = AuditionPvSha256.FileHash(warmup);
            proof.bl10Sha256 = AuditionPvSha256.FileHash(Path.Combine(
                frames,
                AuditionPvStationBossDeathAftermathCapture.FrameFileName(62)));
            proof.bl11Sha256 = AuditionPvSha256.FileHash(Path.Combine(
                frames,
                AuditionPvStationBossDeathAftermathCapture.FrameFileName(116)));
            proof.bl12Sha256 = AuditionPvSha256.FileHash(Path.Combine(
                frames,
                AuditionPvStationBossDeathAftermathCapture.FrameFileName(246)));
            string[] paths = AuditionPvStationBossDeathAftermathGoldenRunner
                .CollectCaptureDependencyPaths();
            AuditionPvDependencyHash[] dependencies = paths.Select(path =>
                new AuditionPvDependencyHash
                {
                    path = path,
                    exists = true,
                    byteLength = 1,
                    sha256 = ShaA
                }).ToArray();
            state.dependencyPaths = paths;
            state.dependencyHashesAtStart = dependencies;
            proof.dependencyHashCount = dependencies.Length;
            proof.captureStartProvenanceSha256 =
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .ComputeCaptureStartProvenanceSha256(state);
            string proofPath = Path.Combine(
                evidence,
                AuditionPvStationBossDeathAftermathGoldenRunner.RuntimeProofFileName);
            File.WriteAllText(proofPath, JsonUtility.ToJson(new RuntimeProofFileProbe
            {
                schema = AuditionPvStationBossDeathAftermathGoldenRunner.RuntimeProofSchema,
                captureId = captureId,
                mapping = AuditionPvStationBossDeathAftermathGoldenRunner
                    .RuntimeMappingDescription,
                gameplay = AuditionPvStationBossDeathAftermathGoldenRunner
                    .RuntimeGameplayDescription,
                runtime = proof
            }, true));
            AuditionPvTestResult[] results =
                AuditionPvStationBossDeathAftermathGoldenRunner.CreateTestResults(
                    state,
                    proof,
                    proofPath,
                    started);
            AuditionPvCaptureManifest manifest =
                AuditionPvCaptureManifestFactory.CreateForRoot(
                    captureId,
                    root,
                    output,
                    new[]
                    {
                        AuditionPvStationBossDeathAftermathCapture
                            .CreateShotManifestEntry()
                    },
                    AuditionPvStationBossDeathAftermathCapture
                        .CreateBaselineManifestEntries(),
                    results,
                    createdAtUtc: started,
                    gitSnapshot: new AuditionPvGitSnapshot
                    {
                        probeSucceeded = true,
                        commitSha = state.gitCommitSha,
                        branch = state.gitBranch,
                        isDirty = false,
                        dirtyStateHashSha256 = state.gitDirtyHashSha256
                    },
                    engineSnapshot: state.engine,
                    dependencyHashSnapshot: dependencies);
            AuditionPvCaptureManifestWriter.WriteNew(manifest);
            return state;
        }

        private static byte[] FakeQhdPngHeader()
        {
            byte[] bytes = new byte[24];
            byte[] signature = { 137, 80, 78, 71, 13, 10, 26, 10 };
            Array.Copy(signature, bytes, signature.Length);
            bytes[12] = (byte)'I';
            bytes[13] = (byte)'H';
            bytes[14] = (byte)'D';
            bytes[15] = (byte)'R';
            WriteBigEndian(bytes, 16, AuditionPvCaptureContract.Width);
            WriteBigEndian(bytes, 20, AuditionPvCaptureContract.Height);
            return bytes;
        }

        private static void WriteBigEndian(byte[] bytes, int offset, int value)
        {
            bytes[offset] = (byte)(value >> 24);
            bytes[offset + 1] = (byte)(value >> 16);
            bytes[offset + 2] = (byte)(value >> 8);
            bytes[offset + 3] = (byte)value;
        }

        private static string[] SuccessArtifactPaths(string output)
        {
            string evidence = Path.Combine(output,
                AuditionPvStationBossDeathAftermathGoldenRunner.EvidenceFolderName);
            string baselines = Path.Combine(output,
                AuditionPvStationBossDeathAftermathCapture.BaselinesFolderName);
            return new[]
            {
                Path.Combine(output, AuditionPvCaptureContract.ManifestFileName),
                Path.Combine(evidence,
                    AuditionPvStationBossDeathAftermathGoldenRunner.RuntimeProofFileName),
                Path.Combine(evidence,
                    AuditionPvStationBossDeathAftermathGoldenRunner.FrameHashLedgerFileName),
                Path.Combine(baselines,
                    AuditionPvStationBossDeathAftermathCapture.Bl10FileName),
                Path.Combine(baselines,
                    AuditionPvStationBossDeathAftermathCapture.Bl11FileName),
                Path.Combine(baselines,
                    AuditionPvStationBossDeathAftermathCapture.Bl12FileName)
            };
        }

        private static string ReadProjectFile(string projectRelativePath)
        {
            string project = Directory.GetParent(Application.dataPath)?.FullName
                ?? throw new InvalidOperationException("Project root missing.");
            return File.ReadAllText(Path.Combine(project, projectRelativePath));
        }

        private static int Count(string text, string token)
        {
            int count = 0;
            int cursor = 0;
            while ((cursor = text.IndexOf(token, cursor, StringComparison.Ordinal)) >= 0)
            {
                count++;
                cursor += token.Length;
            }

            return count;
        }

        private static string NewTempRoot(string label)
        {
            string root = Path.Combine(
                Path.GetTempPath(),
                "dimension-brawl-" + label + "-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            return root;
        }

        private static void DeleteTempRoot(string root)
        {
            string full = Path.GetFullPath(root);
            string temp = Path.GetFullPath(Path.GetTempPath()).TrimEnd('\\', '/');
            if (!full.StartsWith(temp + Path.DirectorySeparatorChar,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Refused test cleanup outside the OS temp root.");
            }

            if (Directory.Exists(full))
            {
                Directory.Delete(full, recursive: true);
            }
        }

        [Serializable]
        private sealed class CalibrationFailureProbe
        {
            public string exception;
            public bool pixelCalibrationLocked;
            public bool calibrationRequired;
            public bool visualCompositionAcceptanceLocked;
            public bool visualCompositionAcceptanceRequired;
            public string successArtifactCleanupFailure;
            public AuditionPvStationBossDeathAftermathGoldenRunner.RuntimeProof runtime;
        }

        [Serializable]
        private sealed class RuntimeProofFileProbe
        {
            public string schema;
            public string captureId;
            public string mapping;
            public string gameplay;
            public AuditionPvStationBossDeathAftermathGoldenRunner.RuntimeProof runtime;
        }
    }
}
