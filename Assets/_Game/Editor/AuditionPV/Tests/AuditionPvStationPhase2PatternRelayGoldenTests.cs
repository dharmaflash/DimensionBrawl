using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DimensionBrawl.Combat;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DimensionBrawl.Editor.AuditionPV.Tests
{
    public sealed class AuditionPvStationPhase2PatternRelayGoldenTests
    {
        [Test]
        public void Contract_IsIndependentExactSevenSecondG07()
        {
            Assert.That(AuditionPvStationPhase2PatternRelayCapture.ShotId, Is.EqualTo("g07"));
            Assert.That(AuditionPvStationPhase2PatternRelayCapture.FirstFrame, Is.Zero);
            Assert.That(AuditionPvStationPhase2PatternRelayCapture.LastFrame, Is.EqualTo(419));
            Assert.That(AuditionPvStationPhase2PatternRelayCapture.ExpectedFrameCount, Is.EqualTo(420));
            Assert.That(AuditionPvStationPhase2PatternRelayCapture.PhaseTwoSettleFrames, Is.EqualTo(90));
            Assert.That(
                AuditionPvStationPhase2PatternRelayCapture.PostRecordingSettleFrameBudget,
                Is.EqualTo(240));
            Assert.That(AuditionPvStationPhase2PatternRelayCapture.CurtainMoveFirstFrame, Is.EqualTo(17));
            Assert.That(AuditionPvStationPhase2PatternRelayCapture.CurtainMoveLastFrame, Is.EqualTo(46));
            Assert.That(AuditionPvStationPhase2PatternRelayCapture.CurtainStopFrame, Is.EqualTo(47));
            Assert.That(AuditionPvStationPhase2PatternRelayCapture.HoverMoveFirstFrame, Is.EqualTo(374));
            Assert.That(AuditionPvStationPhase2PatternRelayCapture.HoverMoveLastFrame, Is.EqualTo(406));
            Assert.That(AuditionPvStationPhase2PatternRelayCapture.HoverStopFrame, Is.EqualTo(407));
            Assert.That(AuditionPvStationPhase2PatternRelayCapture.CurtainWindupFrame, Is.EqualTo(10));
            Assert.That(AuditionPvStationPhase2PatternRelayCapture.CurtainFireFrame, Is.EqualTo(68));
            Assert.That(AuditionPvStationPhase2PatternRelayCapture.HoverWindupFrame, Is.EqualTo(368));
            Assert.That(AuditionPvStationPhase2PatternRelayCapture.HoverFireFrame, Is.EqualTo(418));
            Assert.That(AuditionPvStationPhase2PatternRelayCapture.FrameTimeSeconds(419),
                Is.EqualTo(419f / 60f).Within(0.000001f));
            Assert.That(AuditionPvStationPhase2PatternRelayCapture.FrameFileName(419),
                Is.EqualTo("frame_0419.png"));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                AuditionPvStationPhase2PatternRelayCapture.FrameFileName(420));
        }

        [Test]
        public void AuthoredProfiles_IndependentlyPinFloat32ScheduleInputsAndColors()
        {
            BossBarragePatternProfile curtain = Load<BossBarragePatternProfile>(
                AuditionPvStationPhase2PatternRelayCapture.CurtainProfilePath);
            BossBarragePatternProfile hover = Load<BossBarragePatternProfile>(
                AuditionPvStationPhase2PatternRelayCapture.HoverProfilePath);
            Assert.That(
                AssetDatabase.AssetPathToGUID(
                    "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_BossBarrage_Phase2_AkazaSummonCurtain.asset"),
                Is.EqualTo("031a4022a43b0d94da2839f6c10ba846"));
            Assert.That(
                AssetDatabase.AssetPathToGUID(
                    "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_BossBarrage_Phase2_AkazaHoverLance.asset"),
                Is.EqualTo("d6ddf85506fc3a64593c1f3627179c8d"));

            Assert.That(curtain.PatternId, Is.EqualTo("AkazaSummonCurtain"));
            Assert.That(curtain.InitialDelaySeconds, Is.EqualTo(0.18f));
            Assert.That(curtain.WindupSeconds, Is.EqualTo(0.96f));
            Assert.That(curtain.WaveIntervalSeconds, Is.EqualTo(5f));
            Assert.That(curtain.ProjectilesPerWave, Is.EqualTo(7));
            AssertRgb(curtain.TelegraphWindupColor, 0.16f, 1f, 0.66f);
            AssertRgb(curtain.TelegraphReleaseColor, 0.75f, 1f, 0.9f);
            Assert.That(curtain.TelegraphMarkerWidthScale, Is.EqualTo(1.1f));
            Assert.That(curtain.TelegraphMarkerDepthScale, Is.EqualTo(1.08f));

            Assert.That(hover.PatternId, Is.EqualTo("AkazaHoverLance"));
            Assert.That(hover.InitialDelaySeconds, Is.EqualTo(0.35f));
            Assert.That(hover.WindupSeconds, Is.EqualTo(0.82f));
            Assert.That(hover.WaveIntervalSeconds, Is.EqualTo(4.6f));
            Assert.That(hover.ProjectilesPerWave, Is.EqualTo(4));
            AssertRgb(hover.TelegraphWindupColor, 0.2f, 0.9f, 1f);
            AssertRgb(hover.TelegraphReleaseColor, 0.72f, 1f, 1f);
            Assert.That(hover.TelegraphMarkerWidthScale, Is.EqualTo(0.42f));
            Assert.That(hover.TelegraphMarkerDepthScale, Is.EqualTo(2.15f));

            AuditionPvStationPhase2PatternRelayCapture.PatternSchedule explanatory =
                AuditionPvStationPhase2PatternRelayCapture.DeriveFloat32Schedule();
            Assert.That(new[]
            {
                explanatory.curtainWindupFrame,
                explanatory.curtainFireFrame,
                explanatory.hoverWindupFrame,
                explanatory.hoverFireFrame
            }, Is.EqualTo(new[] { 10, 68, 368, 418 }));
        }

        [Test]
        public void ProductEmitter_PublicTickActuallyEmitsExactRelaySchedule()
        {
            BossBarragePatternProfile curtain = Load<BossBarragePatternProfile>(
                AuditionPvStationPhase2PatternRelayCapture.CurtainProfilePath);
            BossBarragePatternProfile hover = Load<BossBarragePatternProfile>(
                AuditionPvStationPhase2PatternRelayCapture.HoverProfilePath);
            var root = new GameObject("g07-product-tick-fixture");
            var tracked = new GameObject("tracked-player");
            tracked.transform.SetParent(root.transform, false);
            try
            {
                SummonLaneSpace lane = root.AddComponent<SummonLaneSpace>();
                BossBarrageEmitter emitter = root.AddComponent<BossBarrageEmitter>();
                var serialized = new SerializedObject(emitter);
                serialized.FindProperty("laneSpace").objectReferenceValue = lane;
                serialized.FindProperty("trackedPlayer").objectReferenceValue =
                    tracked.transform;
                serialized.FindProperty("patternProfile").objectReferenceValue = hover;
                SerializedProperty sequence = serialized.FindProperty("patternSequence");
                sequence.arraySize = 1;
                sequence.GetArrayElementAtIndex(0).objectReferenceValue = hover;
                serialized.FindProperty("wavesPerPattern").intValue = 1;
                serialized.ApplyModifiedPropertiesWithoutUndo();

                Transform[] orderedOrigins = Enumerable.Range(0, 6)
                    .Select(index =>
                    {
                        var origin = new GameObject("origin-" + index).transform;
                        origin.SetParent(root.transform, false);
                        return origin;
                    })
                    .ToArray();
                var cursor = typeof(BossBarrageEmitter).GetField(
                    "spawnOriginWaveCursor",
                    System.Reflection.BindingFlags.Instance
                        | System.Reflection.BindingFlags.NonPublic);
                Assert.That(cursor, Is.Not.Null);
                cursor.SetValue(emitter, 4);
                emitter.ConfigureSpawnOrigins(orderedOrigins);
                Assert.That(cursor.GetValue(emitter), Is.EqualTo(0));
                Assert.That(emitter.ConfiguredSpawnOriginCount, Is.EqualTo(6));
                serialized.Update();
                SerializedProperty configuredOrigins =
                    serialized.FindProperty("projectileSpawnOrigins");
                Assert.That(configuredOrigins.arraySize, Is.EqualTo(6));
                for (int index = 0; index < orderedOrigins.Length; index++)
                {
                    Assert.That(
                        configuredOrigins.GetArrayElementAtIndex(index).objectReferenceValue,
                        Is.SameAs(orderedOrigins[index]));
                }

                var observed = new List<string>();
                int logicalFrame = -1;
                emitter.WindupStarted += (source, pattern) => observed.Add(
                    $"w:{logicalFrame}:{pattern.PatternId}:{source.CurrentPatternIsPriority}");
                emitter.WaveFired += (source, pattern, _) => observed.Add(
                    $"f:{logicalFrame}:{pattern.PatternId}:{source.LastFiredWaveWasPriority}");
                emitter.SetFiringEnabled(false);
                Assert.That(emitter.QueuePriorityPatternForNextFiringWindow(curtain, 1),
                    Is.True);
                emitter.SetFiringEnabled(true);
                for (logicalFrame = 0; logicalFrame < 420; logicalFrame++)
                {
                    emitter.Tick(1f / 60f);
                }

                Assert.That(observed, Is.EqualTo(new[]
                {
                    "w:10:AkazaSummonCurtain:True",
                    "f:68:AkazaSummonCurtain:True",
                    "w:368:AkazaHoverLance:False",
                    "f:418:AkazaHoverLance:False"
                }));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void TelegraphPresenter_AppliesProfileColorToOwnedRuntimeMaterialAndRestoresAsset()
        {
            BossBarragePatternProfile curtain = Load<BossBarragePatternProfile>(
                AuditionPvStationPhase2PatternRelayCapture.CurtainProfilePath);
            Material authoredMaterial = Load<Material>(
                "Assets/_Game/Art/Materials/ActionFoundation/AF_BossBarrageIncomingTelegraph.mat");
            var root = new GameObject("g07-telegraph-material-fixture");
            var tracked = new GameObject("tracked-player");
            tracked.transform.SetParent(root.transform, false);
            var marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marker.name = "marker";
            marker.transform.SetParent(root.transform, false);
            marker.SetActive(false);
            MeshRenderer markerRenderer = marker.GetComponent<MeshRenderer>();
            BoxCollider markerCollider = marker.GetComponent<BoxCollider>();
            markerRenderer.sharedMaterial = authoredMaterial;
            BossBarrageLaneTelegraphPresenter presenter = null;
            try
            {
                SummonLaneSpace lane = root.AddComponent<SummonLaneSpace>();
                BossBarrageEmitter emitter = root.AddComponent<BossBarrageEmitter>();
                var serialized = new SerializedObject(emitter);
                serialized.FindProperty("laneSpace").objectReferenceValue = lane;
                serialized.FindProperty("trackedPlayer").objectReferenceValue =
                    tracked.transform;
                serialized.FindProperty("patternProfile").objectReferenceValue = curtain;
                SerializedProperty sequence = serialized.FindProperty("patternSequence");
                sequence.arraySize = 1;
                sequence.GetArrayElementAtIndex(0).objectReferenceValue = curtain;
                serialized.FindProperty("wavesPerPattern").intValue = 1;
                serialized.ApplyModifiedPropertiesWithoutUndo();

                presenter = root.AddComponent<BossBarrageLaneTelegraphPresenter>();
                presenter.Configure(
                    emitter,
                    lane,
                    root.transform,
                    new[] { marker.transform },
                    new Renderer[] { markerRenderer });
                Assert.That(markerCollider.enabled, Is.False,
                    "Presentation-only lane markers must never become solid gameplay obstacles.");
                Assert.That(presenter.EnabledMarkerColliderCount, Is.Zero);
                emitter.SetFiringEnabled(true);
                for (int frame = 0; frame <= 10; frame++)
                {
                    emitter.Tick(1f / 60f);
                }

                Assert.That(presenter.VisiblePattern, Is.SameAs(curtain));
                Assert.That(presenter.VisibleMarkerCount, Is.EqualTo(1));
                Material runtimeMaterial = markerRenderer.sharedMaterial;
                Assert.That(runtimeMaterial, Is.Not.SameAs(authoredMaterial));
                Assert.That(runtimeMaterial.hideFlags & HideFlags.DontSave,
                    Is.EqualTo(HideFlags.DontSave));
                AssertRgb(runtimeMaterial.GetColor("_BaseColor"), 0.16f, 1f, 0.66f);
                Material firstRuntimeMaterial = runtimeMaterial;
                presenter.RefreshNow();
                Assert.That(markerRenderer.sharedMaterial,
                    Is.SameAs(firstRuntimeMaterial),
                    "Repeated refreshes must reuse the owned runtime material.");
                Assert.That(markerCollider.enabled, Is.False);
                Assert.That(presenter.EnabledMarkerColliderCount, Is.Zero);

                presenter.Configure(
                    null,
                    null,
                    root.transform,
                    Array.Empty<Transform>(),
                    Array.Empty<Renderer>());
                Assert.That(markerRenderer.sharedMaterial, Is.SameAs(authoredMaterial));
            }
            finally
            {
                if (presenter != null)
                {
                    UnityEngine.Object.DestroyImmediate(presenter);
                }

                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void SceneAndAssets_HaveExactIndependentIdentitiesAndBindings()
        {
            const string ExpectedProjectilePath =
                "Assets/_Game/Prefabs/Combat/PF_BossBarrageProjectile_AkazaPhase2.prefab";
            const string ExpectedProjectileGuid = "2aa4017b4610ff84ebad4e8f59cb2daf";
            const string ExpectedVfxPath =
                "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_CombatVfxCues_ActionFoundation.asset";
            const string ExpectedVfxGuid = "c00395ebb8d9682459aa3144cc6d2853";
            Assert.That(
                AuditionPvStationPhase2PatternRelayCapture.PhaseTwoProjectilePrefabPath,
                Is.EqualTo(ExpectedProjectilePath));
            Assert.That(
                AssetDatabase.AssetPathToGUID(ExpectedProjectilePath),
                Is.EqualTo(ExpectedProjectileGuid));
            Assert.That(AuditionPvStationPhase2PatternRelayCapture.CombatVfxProfilePath,
                Is.EqualTo(ExpectedVfxPath));
            Assert.That(AssetDatabase.AssetPathToGUID(ExpectedVfxPath),
                Is.EqualTo(ExpectedVfxGuid));
            string[] paths =
            {
                AuditionPvStationPhase2PatternRelayCapture.CurtainProfilePath,
                AuditionPvStationPhase2PatternRelayCapture.HoverProfilePath,
                AuditionPvStationPhase2PatternRelayCapture.SpiralProfilePath,
                AuditionPvStationPhase2PatternRelayCapture.CrushNetProfilePath,
                AuditionPvStationPhase2PatternRelayCapture.PressureActionDeckPath,
                AuditionPvStationPhase2PatternRelayCapture.PhaseTwoProjectilePrefabPath,
                AuditionPvStationPhase2PatternRelayCapture.CombatVfxProfilePath,
                AuditionPvStationPhase2PatternRelayCapture.GameplayPostProcessPath,
                AuditionPvStationPhase2PatternRelayCapture.NoCrossWallPrefabPath
            };
            foreach (string path in paths)
            {
                Assert.That(AssetDatabase.LoadMainAssetAtPath(path), Is.Not.Null, path);
            }

            SceneSetup[] setup = EditorSceneManager.GetSceneManagerSetup();
            bool setupIsRestorable = setup != null
                && setup.Length > 0
                && setup.Any(value => value.isLoaded)
                && setup.All(value => !string.IsNullOrWhiteSpace(value.path)
                    && AssetDatabase.LoadAssetAtPath<SceneAsset>(value.path) != null);
            Scene scene = SceneManager.GetSceneByPath(
                AuditionPvStationPhase2PatternRelayCapture.StationScenePath);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                scene = EditorSceneManager.OpenScene(
                    AuditionPvStationPhase2PatternRelayCapture.StationScenePath,
                    OpenSceneMode.Additive);
            }
            try
            {
                OlympusStationAkazaPhase2FlowController flow = scene
                    .GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<
                        OlympusStationAkazaPhase2FlowController>(true))
                    .Single();
                var flowSerialized = new SerializedObject(flow);
                BossBarrageEmitter emitter = flowSerialized
                    .FindProperty("bossBarrageEmitter")
                    .objectReferenceValue as BossBarrageEmitter;
                Assert.That(emitter, Is.Not.Null);

                BossBarragePatternProfile curtain = Load<BossBarragePatternProfile>(
                    "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_BossBarrage_Phase2_AkazaSummonCurtain.asset");
                BossBarragePatternProfile hover = Load<BossBarragePatternProfile>(
                    "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_BossBarrage_Phase2_AkazaHoverLance.asset");
                BossBarragePatternProfile spiral = Load<BossBarragePatternProfile>(
                    "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_BossBarrage_Phase2_AkazaSpiralVolley.asset");
                BossBarragePatternProfile crush = Load<BossBarragePatternProfile>(
                    "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_BossBarrage_Phase2_AkazaCrushNet.asset");
                SerializedProperty sequence = flowSerialized.FindProperty(
                    "phaseTwoPatternSequence");
                BossBarragePatternProfile[] expectedSequence =
                    { hover, curtain, spiral, hover, curtain, crush };
                Assert.That(sequence.arraySize, Is.EqualTo(expectedSequence.Length));
                for (int index = 0; index < expectedSequence.Length; index++)
                {
                    Assert.That(
                        sequence.GetArrayElementAtIndex(index).objectReferenceValue,
                        Is.SameAs(expectedSequence[index]));
                }

                Assert.That(flowSerialized.FindProperty("phaseTwoOpeningPattern")
                    .objectReferenceValue, Is.SameAs(curtain));
                Assert.That(flowSerialized.FindProperty("phaseTwoWavesPerPattern")
                    .intValue, Is.EqualTo(1));
                GameObject projectileRoot = Load<GameObject>(ExpectedProjectilePath);
                Assert.That(flowSerialized.FindProperty("phaseTwoProjectilePrefab")
                    .objectReferenceValue,
                    Is.SameAs(projectileRoot.GetComponent<BossBarrageProjectile>()));
                Assert.That(flowSerialized.FindProperty("phaseTwoActionDeckProfile")
                    .objectReferenceValue,
                    Is.SameAs(AssetDatabase.LoadMainAssetAtPath(
                        "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_BossPressureActionDeck_AkazaPhase2.asset")));
                SerializedProperty origins = flowSerialized.FindProperty(
                    "phaseTwoBarrageSpawnOrigins");
                Assert.That(origins.arraySize, Is.EqualTo(6));
                for (int index = 0; index < origins.arraySize; index++)
                {
                    Assert.That(origins.GetArrayElementAtIndex(index)
                        .objectReferenceValue, Is.Not.Null);
                }

                BossBarrageVisualCueDriver visual = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<
                        BossBarrageVisualCueDriver>(true))
                    .Single(candidate => candidate.BossBarrageEmitter == emitter);
                Assert.That(visual.CuePlayer, Is.Not.Null);
                Assert.That(visual.CuePlayer.Profile,
                    Is.SameAs(Load<CombatVfxCueProfile>(ExpectedVfxPath)));
                BossBarrageLaneTelegraphPresenter telegraph = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<
                        BossBarrageLaneTelegraphPresenter>(true))
                    .Single(candidate => candidate.BossBarrageEmitter == emitter);
                Assert.That(telegraph.gameObject.activeSelf, Is.True);
                Assert.That(telegraph.enabled, Is.True);
                BossBarrageCameraCueDriver cameraCue = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<
                        BossBarrageCameraCueDriver>(true))
                    .Single(candidate => new SerializedObject(candidate)
                        .FindProperty("bossBarrageEmitter").objectReferenceValue == emitter);
                SerializedProperty cameraOverrides = new SerializedObject(cameraCue)
                    .FindProperty("patternWindupCueOverrides");
                SerializedProperty crushNetOverride = Enumerable.Range(
                        0,
                        cameraOverrides.arraySize)
                    .Select(cameraOverrides.GetArrayElementAtIndex)
                    .Single(value => value.FindPropertyRelative("patternId").stringValue
                        == "AkazaCrushNet");
                float crushNetCameraSeconds = crushNetOverride
                    .FindPropertyRelative("cue")
                    .FindPropertyRelative("durationSeconds")
                    .floatValue;
                Assert.That(crushNetCameraSeconds, Is.EqualTo(3.2f));
                Assert.That(
                    AuditionPvStationPhase2PatternRelayCapture.PostRecordingSettleFrameBudget,
                    Is.GreaterThan(Mathf.CeilToInt(crushNetCameraSeconds * 60f)));

                const string CorridorScenePath =
                    "Assets/_Game/Scenes/OlympusCorridorInvasionStage.unity";
                Scene corridor = SceneManager.GetSceneByPath(CorridorScenePath);
                if (!corridor.IsValid() || !corridor.isLoaded)
                {
                    corridor = EditorSceneManager.OpenScene(
                        CorridorScenePath,
                        OpenSceneMode.Additive);
                }

                BossBarrageLaneTelegraphPresenter corridorTelegraph = corridor
                    .GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<
                        BossBarrageLaneTelegraphPresenter>(true))
                    .Single(candidate => candidate.gameObject.name
                        == "BossBarrageLaneReview_BossBarrageTelegraphMarkers");
                Assert.That(corridorTelegraph.gameObject.activeSelf, Is.True);
                Assert.That(corridorTelegraph.enabled, Is.True);
            }
            finally
            {
                if (setupIsRestorable)
                {
                    EditorSceneManager.RestoreSceneManagerSetup(setup);
                }
                else
                {
                    EditorSceneManager.NewScene(
                        NewSceneSetup.EmptyScene,
                        NewSceneMode.Single);
                }
            }
        }

        [Test]
        public void ManifestAndBaselines_UseActualFireFramesAndFinalHero()
        {
            AuditionPvShotManifestEntry shot =
                AuditionPvStationPhase2PatternRelayCapture.CreateShotManifestEntry();
            AuditionPvBaselineManifestEntry[] baselines =
                AuditionPvStationPhase2PatternRelayCapture.CreateBaselineManifestEntries();
            Assert.That(shot.startFrame, Is.Zero);
            Assert.That(shot.endFrame, Is.EqualTo(419));
            Assert.That(shot.expectedFrameCount, Is.EqualTo(420));
            Assert.That(shot.hudMode, Is.EqualTo("hud-on"));
            Assert.That(shot.notes, Does.Contain("f419"));
            Assert.That(baselines.Select(value => value.sourceFrame),
                Is.EqualTo(new[] { 68, 418 }));
            Assert.That(baselines[0].fileName, Is.EqualTo(
                "BL08_AKAZA_PHASE2_SUMMON_CURTAIN__HUDON__t01.133333.png"));
            Assert.That(baselines[1].fileName, Is.EqualTo(
                "BL09_AKAZA_PHASE2_HOVER_LANCE__HUDON__t06.966667.png"));
        }

        [Test]
        public void RecordingRegion_LintsForbiddenDirectControlMutationTokens()
        {
            string source = ReadSource(
                AuditionPvStationPhase2PatternRelayCapture.CaptureScriptPath);
            string recording = Slice(
                source,
                "// RECORDING CONTRACT BEGIN",
                "// RECORDING CONTRACT END");
            foreach (string forbidden in new[]
            {
                "BeginWindup(", "FirePendingWave(", "ConfigurePattern",
                "QueuePriorityPattern(", "QueuePriorityPatternForNextFiringWindow(",
                "SetPositionAndRotation(", ".position =", ".rotation =",
                "Camera.main", "Material", "SetInvulnerable"
            })
            {
                Assert.That(recording, Does.Not.Contain(forbidden), forbidden);
            }
        }

        [Test]
        public void PlayerResponse_LintsForbiddenPoseAndControllerMutationTokens()
        {
            string source = ReadSource(
                AuditionPvStationPhase2PatternRelayCapture.CaptureScriptPath);
            string response = Slice(
                source,
                "private void ApplyPlayerResponseInput",
                "private void CapturePresentationAfterEvent");
            Assert.That(response, Does.Not.Contain("transform.position ="));
            Assert.That(response, Does.Not.Contain("CharacterController.Move"));
        }

        [Test]
        public void Cleanup_LintsForbiddenUnboundedOrDirectSequenceMutationTokens()
        {
            string source = ReadSource(
                AuditionPvStationPhase2PatternRelayCapture.CaptureScriptPath);
            Assert.That(source, Does.Not.Contain("emitter.Tick(float.MaxValue)"));
        }

        [Test]
        public void Runner_RawWarmupMapsExactlyToLogicalFrames()
        {
            string root = Path.Combine(
                Path.GetTempPath(),
                "dimension-brawl-g07-remap-" + Guid.NewGuid().ToString("N"));
            string frames = Path.Combine(root, "frames");
            string evidence = Path.Combine(root, "evidence");
            Directory.CreateDirectory(frames);
            try
            {
                for (int raw = 0; raw <= 420; raw++)
                {
                    File.WriteAllBytes(
                        Path.Combine(frames,
                            AuditionPvStationPhase2PatternRelayGoldenRunner
                                .RawFrameFileName(raw)),
                        new[] { (byte)(raw & 0xff) });
                }

                string warmup =
                    AuditionPvStationPhase2PatternRelayGoldenRunner.RemapRawFrames(
                        frames,
                        evidence);
                AuditionPvStationPhase2PatternRelayGoldenRunner
                    .ValidateLogicalFrameSequence(frames);
                Assert.That(File.ReadAllBytes(warmup)[0], Is.Zero);
                Assert.That(File.ReadAllBytes(Path.Combine(frames, "frame_0000.png"))[0],
                    Is.EqualTo(1));
                Assert.That(File.ReadAllBytes(Path.Combine(frames, "frame_0419.png"))[0],
                    Is.EqualTo((byte)(420 & 0xff)));
            }
            finally
            {
                if (Directory.Exists(root))
                {
                    Directory.Delete(root, recursive: true);
                }
            }
        }

        [Test]
        public void HeadfulArgumentsAndFullPngValidation_ExecutePositiveAndNegativePaths()
        {
            Assert.DoesNotThrow(() =>
                AuditionPvStationPhase2PatternRelayGoldenRunner
                    .ValidateBatchCommandLine(new[] { "Unity", "-noaudio" }));
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvStationPhase2PatternRelayGoldenRunner
                    .ValidateBatchCommandLine(new[] { "Unity" }));
            foreach (string forbidden in new[] { "-batchmode", "-quit", "-nographics" })
            {
                Assert.Throws<InvalidOperationException>(() =>
                    AuditionPvStationPhase2PatternRelayGoldenRunner
                        .ValidateBatchCommandLine(
                            new[] { "Unity", "-noaudio", forbidden }));
            }

            string root = Path.Combine(Path.GetTempPath(),
                "dimension-brawl-g07-png-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            try
            {
                string valid = Path.Combine(root, "valid.png");
                string wrong = Path.Combine(root, "wrong.png");
                string truncated = Path.Combine(root, "truncated.png");
                string decoded = Path.Combine(root, "decoded.png");
                WritePngHeader(valid, 2560, 1440);
                WritePngHeader(wrong, 1920, 1080);
                File.WriteAllBytes(truncated, new byte[] { 137, 80, 78 });
                Assert.DoesNotThrow(() =>
                    AuditionPvStationPhase2PatternRelayGoldenRunner.ValidatePngFile(
                        valid, 2560, 1440));
                Assert.Throws<InvalidDataException>(() =>
                    AuditionPvStationPhase2PatternRelayGoldenRunner.ValidatePngFile(
                        wrong, 2560, 1440));
                Assert.Throws<InvalidDataException>(() =>
                    AuditionPvStationPhase2PatternRelayGoldenRunner.ValidatePngFile(
                        truncated, 2560, 1440));
                var texture = new Texture2D(4, 4, TextureFormat.RGBA32, false);
                try
                {
                    texture.SetPixels32(Enumerable.Repeat(
                        new Color32(20, 40, 80, 255), 16).ToArray());
                    texture.Apply(false, false);
                    File.WriteAllBytes(decoded, texture.EncodeToPNG());
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(texture);
                }

                Assert.DoesNotThrow(() =>
                    AuditionPvStationPhase2PatternRelayGoldenRunner
                        .ValidateDecodablePngFile(decoded, 4, 4));
                Assert.Throws<InvalidDataException>(() =>
                    AuditionPvStationPhase2PatternRelayGoldenRunner
                        .ValidateDecodablePngFile(valid, 2560, 1440));
            }
            finally
            {
                Directory.Delete(root, recursive: true);
            }
        }

        [Test]
        public void RuntimeProof_AcceptsLockedCalibrationAndCanReplayFirstTakeFailure()
        {
            AuditionPvStationPhase2PatternRelayGoldenRunner.RuntimeProof proof =
                CreateValidProof();
            Assert.DoesNotThrow(() =>
                AuditionPvStationPhase2PatternRelayGoldenRunner
                    .ValidateRuntimeProofBeforePixelCalibration(proof));
            Assert.DoesNotThrow(() =>
                AuditionPvStationPhase2PatternRelayGoldenRunner
                    .ValidateRuntimeProof(proof));
            Assert.That(
                AuditionPvStationPhase2PatternRelayGoldenRunner
                    .PatternPixelCalibrationLocked,
                Is.True);

            InvalidOperationException firstTake = Assert.Throws<InvalidOperationException>(() =>
                AuditionPvStationPhase2PatternRelayGoldenRunner
                    .ValidatePatternPixelEvidence(
                        proof,
                        false,
                        AuditionPvStationPhase2PatternRelayGoldenRunner
                            .MinimumCurtainGreenSamples,
                        AuditionPvStationPhase2PatternRelayGoldenRunner
                            .MinimumHoverCyanSamples,
                        AuditionPvStationPhase2PatternRelayGoldenRunner
                            .MinimumCurtainLocalizedFireMeanAbsoluteRgb,
                        AuditionPvStationPhase2PatternRelayGoldenRunner
                            .MinimumHoverLocalizedFireMeanAbsoluteRgb,
                        AuditionPvStationPhase2PatternRelayGoldenRunner
                            .MinimumCurtainFireOverQuietMeanMargin,
                        AuditionPvStationPhase2PatternRelayGoldenRunner
                            .MinimumHoverFireOverQuietMeanMargin));
            Assert.That(firstTake.Message, Does.Contain("CalibrationRequired"));
        }

        [Test]
        public void CalibratedPixelThresholds_AcceptBoundariesAndRejectEveryUnderflow()
        {
            AuditionPvStationPhase2PatternRelayGoldenRunner.RuntimeProof Boundary()
            {
                AuditionPvStationPhase2PatternRelayGoldenRunner.RuntimeProof proof =
                    CreateValidProof();
                proof.curtainWindupColors.curtainGreenSampleCount =
                    AuditionPvStationPhase2PatternRelayGoldenRunner
                        .MinimumCurtainGreenSamples;
                proof.hoverWindupColors.hoverCyanSampleCount =
                    AuditionPvStationPhase2PatternRelayGoldenRunner
                        .MinimumHoverCyanSamples;
                proof.curtainFireDelta.meanAbsoluteRgb =
                    AuditionPvStationPhase2PatternRelayGoldenRunner
                        .MinimumCurtainLocalizedFireMeanAbsoluteRgb;
                proof.curtainQuietDelta.meanAbsoluteRgb =
                    proof.curtainFireDelta.meanAbsoluteRgb
                    - AuditionPvStationPhase2PatternRelayGoldenRunner
                        .MinimumCurtainFireOverQuietMeanMargin
                    - 0.000000001d;
                proof.hoverFireDelta.meanAbsoluteRgb =
                    AuditionPvStationPhase2PatternRelayGoldenRunner
                        .MinimumHoverLocalizedFireMeanAbsoluteRgb;
                proof.hoverQuietDelta.meanAbsoluteRgb =
                    proof.hoverFireDelta.meanAbsoluteRgb
                    - AuditionPvStationPhase2PatternRelayGoldenRunner
                        .MinimumHoverFireOverQuietMeanMargin
                    - 0.000000001d;
                return proof;
            }

            Assert.DoesNotThrow(() =>
                AuditionPvStationPhase2PatternRelayGoldenRunner
                    .ValidateCalibratedPatternPixelEvidence(Boundary()));
            void Reject(Action<AuditionPvStationPhase2PatternRelayGoldenRunner.RuntimeProof> mutate)
            {
                AuditionPvStationPhase2PatternRelayGoldenRunner.RuntimeProof proof = Boundary();
                mutate(proof);
                Assert.Throws<InvalidOperationException>(() =>
                    AuditionPvStationPhase2PatternRelayGoldenRunner
                        .ValidateCalibratedPatternPixelEvidence(proof));
            }

            Reject(proof => proof.curtainWindupColors.curtainGreenSampleCount--);
            Reject(proof => proof.hoverWindupColors.hoverCyanSampleCount--);
            Reject(proof =>
            {
                proof.curtainWindupColors.curtainGreenSampleCount = 0;
                proof.curtainWindupColors.hoverCyanSampleCount = 1000;
            });
            Reject(proof =>
            {
                proof.hoverWindupColors.hoverCyanSampleCount = 0;
                proof.hoverWindupColors.curtainGreenSampleCount = 1000;
            });
            Reject(proof => proof.curtainFireDelta.meanAbsoluteRgb -= 0.000001d);
            Reject(proof => proof.hoverFireDelta.meanAbsoluteRgb -= 0.000001d);
            Reject(proof => proof.curtainQuietDelta.meanAbsoluteRgb += 0.000000002d);
            Reject(proof => proof.hoverQuietDelta.meanAbsoluteRgb += 0.000000002d);
        }

        [Test]
        public void FirstTakeCalibrationFailure_WritesTelemetryAndRemovesSuccessArtifacts()
        {
            string root = Path.Combine(
                Path.GetTempPath(),
                "dimension-brawl-g07-calibration-failure-"
                + Guid.NewGuid().ToString("N"));
            const string CaptureId = "g07-calibration-first-take";
            string output = AuditionPvOutputPaths.ResolveOutputDirectory(root, CaptureId);
            string evidence = Path.Combine(output,
                AuditionPvStationPhase2PatternRelayGoldenRunner.EvidenceFolderName);
            string baselines = Path.Combine(output,
                AuditionPvStationPhase2PatternRelayCapture.BaselinesFolderName);
            Directory.CreateDirectory(evidence);
            Directory.CreateDirectory(baselines);
            try
            {
                string[] successArtifacts =
                {
                    Path.Combine(output, AuditionPvCaptureContract.ManifestFileName),
                    Path.Combine(evidence,
                        AuditionPvStationPhase2PatternRelayGoldenRunner.RuntimeProofFileName),
                    Path.Combine(evidence,
                        AuditionPvStationPhase2PatternRelayGoldenRunner.FrameHashLedgerFileName),
                    Path.Combine(baselines,
                        AuditionPvStationPhase2PatternRelayCapture.Bl08FileName),
                    Path.Combine(baselines,
                        AuditionPvStationPhase2PatternRelayCapture.Bl09FileName)
                };
                foreach (string path in successArtifacts)
                {
                    File.WriteAllText(path, "must-be-removed");
                }

                AuditionPvStationPhase2PatternRelayGoldenRunner.RuntimeProof proof =
                    CreateValidProof();
                InvalidOperationException calibrationFailure =
                    Assert.Throws<InvalidOperationException>(() =>
                        AuditionPvStationPhase2PatternRelayGoldenRunner
                            .ValidatePatternPixelEvidence(
                                proof,
                                false,
                                AuditionPvStationPhase2PatternRelayGoldenRunner
                                    .MinimumCurtainGreenSamples,
                                AuditionPvStationPhase2PatternRelayGoldenRunner
                                    .MinimumHoverCyanSamples,
                                AuditionPvStationPhase2PatternRelayGoldenRunner
                                    .MinimumCurtainLocalizedFireMeanAbsoluteRgb,
                                AuditionPvStationPhase2PatternRelayGoldenRunner
                                    .MinimumHoverLocalizedFireMeanAbsoluteRgb,
                                AuditionPvStationPhase2PatternRelayGoldenRunner
                                    .MinimumCurtainFireOverQuietMeanMargin,
                                AuditionPvStationPhase2PatternRelayGoldenRunner
                                    .MinimumHoverFireOverQuietMeanMargin));
                var state = new AuditionPvStationPhase2PatternRelayGoldenRunner
                    .PersistedRunnerState
                {
                    captureId = CaptureId,
                    outputRoot = root,
                    outputDirectory = output,
                    baselineDirectory = baselines,
                    gitCommitSha = new string('a', 40),
                    gitBranch = "main",
                    dependencyHashesAtStart = Array.Empty<AuditionPvDependencyHash>()
                };
                AuditionPvStationPhase2PatternRelayGoldenRunner.WriteFailureArtifactForRoot(
                    output,
                    "AwaitingEditMode",
                    calibrationFailure,
                    proof,
                    state,
                    root,
                    new AuditionPvGitSnapshot { probeSucceeded = true },
                    pixelCalibrationLocked: false);

                string failurePath = Path.Combine(
                    output,
                    AuditionPvStationPhase2PatternRelayGoldenRunner.FailureFileName);
                var artifact = JsonUtility.FromJson<CalibrationFailureArtifactProbe>(
                    File.ReadAllText(failurePath));
                Assert.That(artifact.pixelCalibrationLocked, Is.False);
                Assert.That(artifact.exception, Does.Contain("CalibrationRequired"));
                Assert.That(artifact.runtime, Is.Not.Null);
                Assert.That(
                    artifact.runtime.curtainWindupColors.curtainGreenSampleCount,
                    Is.EqualTo(proof.curtainWindupColors.curtainGreenSampleCount));
                Assert.That(
                    artifact.runtime.hoverWindupColors.hoverCyanSampleCount,
                    Is.EqualTo(proof.hoverWindupColors.hoverCyanSampleCount));
                Assert.That(
                    artifact.runtime.curtainFireDelta.meanAbsoluteRgb,
                    Is.EqualTo(proof.curtainFireDelta.meanAbsoluteRgb));
                Assert.That(
                    artifact.runtime.hoverQuietDelta.meanAbsoluteRgb,
                    Is.EqualTo(proof.hoverQuietDelta.meanAbsoluteRgb));
                Assert.That(successArtifacts.All(path => !File.Exists(path)), Is.True);
            }
            finally
            {
                if (Directory.Exists(root))
                {
                    Directory.Delete(root, recursive: true);
                }
            }
        }

        [Test]
        public void RuntimeProof_RejectsScheduleActionsCleanupIdentityAndFinalHeroMutations()
        {
            AssertMutation(proof => proof.hoverFireFrame = 419);
            AssertMutation(proof => proof.curtainSpawnedCount = 6);
            AssertMutation(proof => proof.hoverWasPriority = true);
            AssertMutation(proof => proof.emitterTickCount = 419);
            AssertMutation(proof => proof.minimumEmitterTimeScale = 0.999f);
            AssertMutation(proof => proof.curtainMoveFirstAppliedFrame = 18);
            AssertMutation(proof => proof.curtainMoveLastAppliedFrame = 45);
            AssertMutation(proof => proof.curtainZeroAppliedFrame = 48);
            AssertMutation(proof => proof.hoverMoveFirstAppliedFrame = 375);
            AssertMutation(proof => proof.hoverMoveLastAppliedFrame = 405);
            AssertMutation(proof => proof.hoverZeroAppliedFrame = 408);
            AssertMutation(proof => proof.pressureActionEventCount = 1);
            AssertMutation(proof => proof.playerDamageEventCount = 1);
            AssertMutation(proof => proof.runStartedCount = 1);
            AssertMutation(proof => proof.hoverDirectionDot = 0.98f);
            AssertMutation(proof => proof.exactProjectileAndVfxBindings = false);
            AssertMutation(proof => proof.telegraphMarkerCollidersNonBlocking = false);
            AssertMutation(proof => proof.lifecycleEmergencyResetUsed = true);
            AssertMutation(proof => proof.stateRestored = false);
            AssertMutation(proof => proof.eventsReleased = false);
            AssertMutation(proof => proof.presentationClockReleased = false);
            AssertMutation(proof => proof.cadenceReleased = false);
            AssertMutation(proof => proof.emitterRestored = false);
            AssertMutation(proof => proof.spawnOriginOrderRestored = false);
            AssertMutation(proof => proof.cameraStateRestored = false);
            AssertMutation(proof => proof.globalStateRestored = false);
            AssertMutation(proof => proof.captureStartProvenanceSha256 = "not-a-sha");
            AssertMutation(proof => proof.cleanupFailure = "cleanup leaked");
            AssertMutation(proof => proof.frame67Sha256 = proof.bl08Sha256);
            AssertMutation(proof => proof.frame417Sha256 = proof.bl09Sha256);
            AssertMutation(proof => proof.curtainFireMarkerColor = Color.red);
            AssertMutation(proof => proof.curtainWindupColors.roiX = 1);
            AssertMutation(proof => proof.renderEvents[4].finalHeroComposition = false);
            AssertMutation(proof =>
            {
                var renderEvent = proof.renderEvents[1];
                foreach (var marker in renderEvent.markers)
                {
                    marker.frustumIntersects = false;
                }

                renderEvent.markerBoundsIntersectFrustum = false;
                renderEvent.allMarkerRenderersIntersectFrustum = false;
            });
        }

        [Test]
        public void VisualHelpers_AreFailClosedAtMaskDeltaHudAndRoiBoundaries()
        {
            Assert.That(
                AuditionPvStationPhase2PatternRelayGoldenRunner.IsCurtainGreen(
                    new Color32(146, 170, 162, 255)), Is.True);
            Assert.That(
                AuditionPvStationPhase2PatternRelayGoldenRunner.IsCurtainGreen(
                    new Color32(147, 170, 162, 255)), Is.False);
            Assert.That(
                AuditionPvStationPhase2PatternRelayGoldenRunner.IsHoverCyan(
                    new Color32(125, 170, 180, 255)), Is.True);
            Assert.That(
                AuditionPvStationPhase2PatternRelayGoldenRunner.IsHoverCyan(
                    new Color32(126, 170, 180, 255)), Is.False);
            Assert.That(
                AuditionPvStationPhase2PatternRelayGoldenRunner.IsHoverCyan(
                    new Color32(100, 170, 221, 255)), Is.False);

            Color32[] before = Enumerable.Repeat(new Color32(0, 0, 0, 255), 16)
                .ToArray();
            Color32[] after = before.ToArray();
            // GetPixels32 is semantic RGBA in Unity bottom-left order.
            after[1 * 4 + 1] = new Color32(30, 30, 30, 255);
            var delta = AuditionPvStationPhase2PatternRelayGoldenRunner
                .EvaluateFrameDelta(before, after, 4, 4, new RectInt(1, 1, 1, 1));
            Assert.That(delta.sampleCount, Is.EqualTo(1));
            Assert.That(delta.changedSampleCount, Is.EqualTo(1));
            after = before.ToArray();
            after[2 * 4 + 1] = new Color32(30, 30, 30, 255);
            delta = AuditionPvStationPhase2PatternRelayGoldenRunner
                .EvaluateFrameDelta(before, after, 4, 4, new RectInt(1, 1, 1, 1));
            Assert.That(delta.changedSampleCount, Is.Zero,
                "A vertically mirrored raw-row mutation must be outside the bottom-left ROI.");
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                AuditionPvStationPhase2PatternRelayGoldenRunner.EvaluateFrameDelta(
                    before, after, 4, 4, new RectInt(4, 0, 1, 1)));

            RectInt expanded = AuditionPvStationPhase2PatternRelayGoldenRunner
                .ExpandAndClamp(new RectInt(0, 0, 2, 2), 8, 10, 10);
            Assert.That(expanded, Is.EqualTo(new RectInt(0, 0, 10, 10)));

            var independentlyObservedG06Hud =
                new AuditionPvStationPhase2PatternRelayGoldenRunner.HudVisualMetrics
            {
                frameCount = 420,
                minimumFramePinkSamples = 569,
                maximumFramePinkSamples = 622,
                minimumFrameDarkSamples = 227,
                maximumFrameDarkSamples = 228,
                minimumFrameBrightSamples = 877,
                maximumFrameBrightSamples = 966,
                minimumFrameMeanLuma = 149.1,
                maximumFrameMeanLuma = 159.7,
                roiX = 688,
                roiY = 8,
                roiWidth = 176,
                roiHeight = 176,
                sampleStride = 4
            };
            Assert.That(
                AuditionPvStationPhase2PatternRelayGoldenRunner
                    .G06HudCalibrationManifestSha256,
                Is.EqualTo(
                    "2f6d7ccf4a87b98055a6557674aa7c1487748e70093c20e15b2c9bac21b0053d"));
            Assert.DoesNotThrow(() =>
                AuditionPvStationPhase2PatternRelayGoldenRunner.ValidateHudMetrics(
                    independentlyObservedG06Hud));
            independentlyObservedG06Hud.roiX = 689;
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvStationPhase2PatternRelayGoldenRunner.ValidateHudMetrics(
                    independentlyObservedG06Hud));
            independentlyObservedG06Hud.roiX = 688;
            independentlyObservedG06Hud.sampleStride = 3;
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvStationPhase2PatternRelayGoldenRunner.ValidateHudMetrics(
                    independentlyObservedG06Hud));
            independentlyObservedG06Hud.sampleStride = 4;
            independentlyObservedG06Hud.minimumFramePinkSamples = 539;
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvStationPhase2PatternRelayGoldenRunner.ValidateHudMetrics(
                    independentlyObservedG06Hud));
        }

        [Test]
        public void PixelRoiHelpers_UseSemanticRgbaAndBottomLeftCoordinatesAfterPngDecode()
        {
            const int width = 4;
            const int height = 4;
            var source = new Texture2D(
                width, height, TextureFormat.RGBA32, false, true);
            var decoded = new Texture2D(
                2, 2, TextureFormat.RGBA32, false, true);
            try
            {
                Color32[] hud = Enumerable.Repeat(
                    new Color32(100, 100, 100, 255), width * height).ToArray();
                for (int y = 2; y < height; y++)
                {
                    for (int x = 0; x < 2; x++)
                    {
                        hud[y * width + x] = new Color32(255, 0, 0, 255);
                    }
                }

                hud[0 * width + 0] = new Color32(200, 100, 100, 255);
                hud[0 * width + 1] = new Color32(10, 10, 10, 255);
                hud[1 * width + 0] = new Color32(240, 240, 240, 255);
                hud[1 * width + 1] = new Color32(100, 100, 100, 255);
                source.SetPixels32(hud);
                source.Apply(updateMipmaps: false, makeNoLongerReadable: false);
                Assert.That(ImageConversion.LoadImage(
                    decoded,
                    ImageConversion.EncodeToPNG(source),
                    markNonReadable: false), Is.True);
                var hudMetrics = AuditionPvStationPhase2PatternRelayGoldenRunner
                    .EvaluateHudRoi(decoded, new RectInt(0, 0, 2, 2), 1);
                Assert.That(hudMetrics.minimumFramePinkSamples, Is.EqualTo(1));
                Assert.That(hudMetrics.minimumFrameDarkSamples, Is.EqualTo(1));
                Assert.That(hudMetrics.minimumFrameBrightSamples, Is.EqualTo(1));
                Assert.That(hudMetrics.minimumFrameMeanLuma, Is.EqualTo(117.75d));

                Color32[] pattern = Enumerable.Repeat(
                    new Color32(100, 100, 100, 255), width * height).ToArray();
                for (int y = 2; y < height; y++)
                {
                    for (int x = 0; x < 2; x++)
                    {
                        pattern[y * width + x] = x == 0
                            ? new Color32(146, 170, 162, 255)
                            : new Color32(125, 170, 180, 255);
                    }
                }

                pattern[0 * width + 0] = new Color32(146, 170, 162, 255);
                pattern[0 * width + 1] = new Color32(125, 170, 180, 255);
                source.SetPixels32(pattern);
                source.Apply(updateMipmaps: false, makeNoLongerReadable: false);
                Assert.That(ImageConversion.LoadImage(
                    decoded,
                    ImageConversion.EncodeToPNG(source),
                    markNonReadable: false), Is.True);
                var colors = AuditionPvStationPhase2PatternRelayGoldenRunner
                    .EvaluatePatternColors(
                        decoded.GetPixels32(),
                        width,
                        height,
                        new RectInt(0, 0, 2, 2),
                        1);
                Assert.That(colors.sampleCount, Is.EqualTo(4));
                Assert.That(colors.curtainGreenSampleCount, Is.EqualTo(1));
                Assert.That(colors.hoverCyanSampleCount, Is.EqualTo(1));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(source);
                UnityEngine.Object.DestroyImmediate(decoded);
            }
        }

        [Test]
        public void RuntimeProof_AllowsPartialMarkerIntersectionButRejectsZeroIntersection()
        {
            var partial = CreateValidProof();
            for (int index = 1;
                index < partial.renderEvents[0].markers.Length;
                index++)
            {
                partial.renderEvents[0].markers[index].frustumIntersects = false;
            }

            partial.renderEvents[0].allMarkerRenderersIntersectFrustum = false;
            Assert.DoesNotThrow(() =>
                AuditionPvStationPhase2PatternRelayGoldenRunner
                    .ValidateRuntimeProofBeforePixelCalibration(partial));

            var inconsistent = CreateValidProof();
            inconsistent.renderEvents[0].markers[0].frustumIntersects = false;
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvStationPhase2PatternRelayGoldenRunner
                    .ValidateRuntimeProofBeforePixelCalibration(inconsistent));

            var absent = CreateValidProof();
            foreach (var marker in absent.renderEvents[0].markers)
            {
                marker.frustumIntersects = false;
            }

            absent.renderEvents[0].markerBoundsIntersectFrustum = false;
            absent.renderEvents[0].allMarkerRenderersIntersectFrustum = false;
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvStationPhase2PatternRelayGoldenRunner
                    .ValidateRuntimeProofBeforePixelCalibration(absent));
        }


        [Test]
        public void FrameLedgerAndFailureCleanup_RejectCorruptionAndEscapeTargets()
        {
            string root = Path.Combine(Path.GetTempPath(),
                "dimension-brawl-g07-owned-" + Guid.NewGuid().ToString("N"));
            string outsideRoot = Path.Combine(Path.GetTempPath(),
                "dimension-brawl-g07-outside-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            Directory.CreateDirectory(outsideRoot);
            try
            {
                string captureId = "g07-test-capture";
                string output = AuditionPvOutputPaths.ResolveOutputDirectory(root, captureId);
                Directory.CreateDirectory(output);
                string frames = Path.Combine(output, "frames", "g07");
                Directory.CreateDirectory(frames);
                for (int frame = 0; frame < 420; frame++)
                {
                    File.WriteAllText(Path.Combine(frames,
                        AuditionPvStationPhase2PatternRelayCapture.FrameFileName(frame)),
                        "frame-" + frame);
                }

                string ledger =
                    AuditionPvStationPhase2PatternRelayGoldenRunner
                        .BuildFrameHashLedger(frames);
                string ledgerPath = Path.Combine(output, "ledger.sha256");
                File.WriteAllText(ledgerPath, ledger);
                Assert.DoesNotThrow(() =>
                    AuditionPvStationPhase2PatternRelayGoldenRunner
                        .ValidateFrameHashLedger(
                            frames,
                            ledgerPath,
                            AuditionPvSha256.TextHash(ledger)));
                File.AppendAllText(Path.Combine(frames, "frame_0123.png"), "tamper");
                Assert.Throws<InvalidOperationException>(() =>
                    AuditionPvStationPhase2PatternRelayGoldenRunner
                        .ValidateFrameHashLedger(
                            frames,
                            ledgerPath,
                            AuditionPvSha256.TextHash(ledger)));

                string baselines = Path.Combine(output, "baselines");
                string evidence = Path.Combine(output, "evidence");
                Directory.CreateDirectory(baselines);
                Directory.CreateDirectory(evidence);
                string outsideBl = Path.Combine(outsideRoot,
                    AuditionPvStationPhase2PatternRelayCapture.Bl08FileName);
                File.WriteAllText(outsideBl, "must-survive");
                string[] ownedSuccessArtifacts =
                {
                    Path.Combine(output, AuditionPvCaptureContract.ManifestFileName),
                    Path.Combine(evidence,
                        AuditionPvStationPhase2PatternRelayGoldenRunner.RuntimeProofFileName),
                    Path.Combine(evidence,
                        AuditionPvStationPhase2PatternRelayGoldenRunner.FrameHashLedgerFileName),
                    Path.Combine(baselines,
                        AuditionPvStationPhase2PatternRelayCapture.Bl08FileName),
                    Path.Combine(baselines,
                        AuditionPvStationPhase2PatternRelayCapture.Bl09FileName)
                };
                foreach (string owned in ownedSuccessArtifacts)
                {
                    File.WriteAllText(owned, "owned");
                }

                var state = new AuditionPvStationPhase2PatternRelayGoldenRunner
                    .PersistedRunnerState
                {
                    captureId = captureId,
                    outputRoot = root,
                    outputDirectory = output,
                    baselineDirectory = outsideRoot
                };
                string cleanup = AuditionPvStationPhase2PatternRelayGoldenRunner
                    .DeleteUncommittedSuccessArtifactsForRoot(output, state, root);
                Assert.That(cleanup, Is.Empty);
                Assert.That(File.Exists(outsideBl), Is.True);
                Assert.That(
                    ownedSuccessArtifacts.All(path => !File.Exists(path)),
                    Is.True,
                    "Failure-only cleanup must remove manifest, runtime proof, ledger, BL08 and BL09.");

                string escapedId = "g07-escaped";
                string escapedOutput = AuditionPvOutputPaths.ResolveOutputDirectory(
                    outsideRoot, escapedId);
                Directory.CreateDirectory(escapedOutput);
                string escapedManifest = Path.Combine(escapedOutput,
                    AuditionPvCaptureContract.ManifestFileName);
                File.WriteAllText(escapedManifest, "must-survive");
                state.captureId = escapedId;
                state.outputRoot = outsideRoot;
                state.outputDirectory = escapedOutput;
                cleanup = AuditionPvStationPhase2PatternRelayGoldenRunner
                    .DeleteUncommittedSuccessArtifactsForRoot(
                        escapedOutput, state, root);
                Assert.That(cleanup, Is.Not.Empty);
                Assert.That(File.Exists(escapedManifest), Is.True);
            }
            finally
            {
                Directory.Delete(root, recursive: true);
                Directory.Delete(outsideRoot, recursive: true);
            }
        }

        [Test]
        public void LateProbe_OrderAndCameraRendererFilteringAreExact()
        {
            var runnerOrder = (DefaultExecutionOrder)Attribute.GetCustomAttribute(
                typeof(AuditionPvStationPhase2PatternRelayGoldenRunnerBehaviour),
                typeof(DefaultExecutionOrder));
            var directorOrder = (DefaultExecutionOrder)Attribute.GetCustomAttribute(
                typeof(AuditionPvStationPhase2PatternRelayDirector),
                typeof(DefaultExecutionOrder));
            var probeOrder = (DefaultExecutionOrder)Attribute.GetCustomAttribute(
                typeof(AuditionPvStationPhase2PatternRelayRenderProbe),
                typeof(DefaultExecutionOrder));
            var cameraOrder = (DefaultExecutionOrder)Attribute.GetCustomAttribute(
                typeof(ActionCameraController),
                typeof(DefaultExecutionOrder));
            var motionOrder = (DefaultExecutionOrder)Attribute.GetCustomAttribute(
                typeof(AkazaPhase2CombatMotionDriver),
                typeof(DefaultExecutionOrder));
            Assert.That(runnerOrder.order, Is.EqualTo(-32500));
            Assert.That(directorOrder.order, Is.EqualTo(-32000));
            Assert.That(probeOrder.order, Is.EqualTo(32000));
            Assert.That(cameraOrder.order, Is.EqualTo(200));
            Assert.That(motionOrder.order, Is.EqualTo(10100));
            Assert.That(probeOrder.order, Is.GreaterThan(cameraOrder.order));
            Assert.That(probeOrder.order, Is.GreaterThan(motionOrder.order));

            var cameraObject = new GameObject("g07-test-camera");
            var rendererObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            try
            {
                Camera camera = cameraObject.AddComponent<Camera>();
                Renderer renderer = rendererObject.GetComponent<Renderer>();
                rendererObject.layer = 7;
                camera.cullingMask = 0;
                Assert.That(AuditionPvStationPhase2PatternRelayRenderProbe
                    .IsRenderedByCamera(camera, renderer), Is.False);
                camera.cullingMask = 1 << 7;
                Assert.That(AuditionPvStationPhase2PatternRelayRenderProbe
                    .IsRenderedByCamera(camera, renderer), Is.True);
                renderer.forceRenderingOff = true;
                Assert.That(AuditionPvStationPhase2PatternRelayRenderProbe
                    .IsRenderedByCamera(camera, renderer), Is.False);
                renderer.forceRenderingOff = false;
                renderer.enabled = false;
                Assert.That(AuditionPvStationPhase2PatternRelayRenderProbe
                    .IsRenderedByCamera(camera, renderer), Is.False);
                renderer.enabled = true;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
                Assert.That(AuditionPvStationPhase2PatternRelayRenderProbe
                    .IsRenderedByCamera(camera, renderer), Is.False);
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                rendererObject.SetActive(false);
                Assert.That(AuditionPvStationPhase2PatternRelayRenderProbe
                    .IsRenderedByCamera(camera, renderer), Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(cameraObject);
                UnityEngine.Object.DestroyImmediate(rendererObject);
            }
        }

        [Test]
        public void PersistedAndSessionPaths_RejectUnknownPhaseAndEveryEscape()
        {
            string root = Path.Combine(
                Path.GetTempPath(),
                "dimension-brawl-g07-state-root-" + Guid.NewGuid().ToString("N"));
            string outside = Path.Combine(
                Path.GetTempPath(),
                "dimension-brawl-g07-state-outside-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            Directory.CreateDirectory(outside);
            try
            {
                const string CaptureId = "g07-state-fixture";
                string output = AuditionPvOutputPaths.ResolveOutputDirectory(root, CaptureId);
                string baseline = Path.Combine(
                    output,
                    AuditionPvStationPhase2PatternRelayCapture.BaselinesFolderName);
                string statePath = Path.Combine(
                    output,
                    AuditionPvStationPhase2PatternRelayGoldenRunner.StateFileName);
                Directory.CreateDirectory(baseline);
                var state = new AuditionPvStationPhase2PatternRelayGoldenRunner
                    .PersistedRunnerState
                {
                    schema = "dimension-brawl.audition-pv.g07-runner-state.v1",
                    phase = "AwaitingPlayMode",
                    captureId = CaptureId,
                    outputRoot = root,
                    outputDirectory = output,
                    baselineDirectory = baseline
                };

                Assert.DoesNotThrow(() =>
                    AuditionPvStationPhase2PatternRelayGoldenRunner
                        .ValidateSessionRecoveryLocationForRoot(
                            statePath.Replace('\\', '/'),
                            output.Replace('\\', '/'),
                            CaptureId,
                            root.Replace('\\', '/')));
                Assert.DoesNotThrow(() =>
                    AuditionPvStationPhase2PatternRelayGoldenRunner
                        .ValidatePersistedStateLocationForRoot(
                            statePath,
                            state,
                            root));
                Assert.DoesNotThrow(() =>
                    AuditionPvStationPhase2PatternRelayGoldenRunner
                        .ValidateSessionBatchAuthority(false, state));
                Assert.Throws<InvalidDataException>(() =>
                    AuditionPvStationPhase2PatternRelayGoldenRunner
                        .ValidateSessionBatchAuthority(true, state));

                state.phase = "UnknownCorruptPhase";
                Assert.Throws<InvalidDataException>(() =>
                    AuditionPvStationPhase2PatternRelayGoldenRunner
                        .ValidatePersistedStateLocationForRoot(statePath, state, root));
                state.phase = "999";
                Assert.Throws<InvalidDataException>(() =>
                    AuditionPvStationPhase2PatternRelayGoldenRunner
                        .ValidatePersistedStateLocationForRoot(statePath, state, root));
                state.phase = "AwaitingPlayMode";
                state.outputRoot = outside;
                Assert.Throws<InvalidDataException>(() =>
                    AuditionPvStationPhase2PatternRelayGoldenRunner
                        .ValidatePersistedStateLocationForRoot(statePath, state, root));
                state.outputRoot = root;
                state.baselineDirectory = outside;
                Assert.Throws<InvalidDataException>(() =>
                    AuditionPvStationPhase2PatternRelayGoldenRunner
                        .ValidatePersistedStateLocationForRoot(statePath, state, root));
                state.baselineDirectory = baseline;
                Assert.Throws<InvalidDataException>(() =>
                    AuditionPvStationPhase2PatternRelayGoldenRunner
                        .ValidateSessionRecoveryLocationForRoot(
                            Path.Combine(outside,
                                AuditionPvStationPhase2PatternRelayGoldenRunner.StateFileName),
                            output,
                            CaptureId,
                            root));
            }
            finally
            {
                Directory.Delete(root, recursive: true);
                Directory.Delete(outside, recursive: true);
            }
        }

        [Test]
        public void TerminalHandoff_SaveFaultStillRecordsAndForcesTerminalExit()
        {
            bool sessionActive = true;
            bool failureRecorded = false;
            bool exitRequested = false;
            Exception failure = AuditionPvStationPhase2PatternRelayGoldenRunner
                .ExecuteTerminalHandoff(
                    () => throw new IOException("injected SaveState failure"),
                    exception =>
                    {
                        Assert.That(exception, Is.TypeOf<IOException>());
                        failureRecorded = true;
                    },
                    () =>
                    {
                        sessionActive = false;
                        exitRequested = true;
                    });
            Assert.That(failure, Is.TypeOf<IOException>());
            Assert.That(failureRecorded, Is.True);
            Assert.That(exitRequested, Is.True);
            Assert.That(sessionActive, Is.False);
        }

        [Test]
        public void TerminalFaultResume_RemovesSuccessArtifactsClearsSessionAndExitsOne()
        {
            string root = Path.Combine(
                Path.GetTempPath(),
                "dimension-brawl-g07-terminal-root-" + Guid.NewGuid().ToString("N"));
            const string CaptureId = "g07-terminal-fault";
            string output = AuditionPvOutputPaths.ResolveOutputDirectory(root, CaptureId);
            string evidence = Path.Combine(output, "evidence");
            string baselines = Path.Combine(output, "baselines");
            Directory.CreateDirectory(evidence);
            Directory.CreateDirectory(baselines);
            try
            {
                string[] successArtifacts =
                {
                    Path.Combine(output, AuditionPvCaptureContract.ManifestFileName),
                    Path.Combine(evidence,
                        AuditionPvStationPhase2PatternRelayGoldenRunner.RuntimeProofFileName),
                    Path.Combine(evidence,
                        AuditionPvStationPhase2PatternRelayGoldenRunner.FrameHashLedgerFileName),
                    Path.Combine(baselines,
                        AuditionPvStationPhase2PatternRelayCapture.Bl08FileName),
                    Path.Combine(baselines,
                        AuditionPvStationPhase2PatternRelayCapture.Bl09FileName)
                };
                foreach (string artifact in successArtifacts)
                {
                    File.WriteAllText(artifact, "partial-success");
                }

                bool sessionCleared = false;
                int? exitCode = null;
                Exception recoveryFailure =
                    AuditionPvStationPhase2PatternRelayGoldenRunner
                        .RecoverTerminalPersistenceFaultForRoot(
                            output,
                            CaptureId,
                            root,
                            "injected SaveState failure",
                            () => sessionCleared = true,
                            code => exitCode = code);
                Assert.That(recoveryFailure, Is.Null);
                Assert.That(sessionCleared, Is.True);
                Assert.That(exitCode, Is.EqualTo(1));
                Assert.That(successArtifacts.All(path => !File.Exists(path)), Is.True);
                Assert.That(File.Exists(Path.Combine(
                    output,
                    AuditionPvStationPhase2PatternRelayGoldenRunner.FailureFileName)),
                    Is.True);
            }
            finally
            {
                Directory.Delete(root, recursive: true);
            }
        }

        [Test]
        public void FailedReservationCleanup_HandlesRevisionAndInjectedCreationFaults()
        {
            string root = Path.Combine(
                Path.GetTempPath(),
                "dimension-brawl-g07-reserve-root-" + Guid.NewGuid().ToString("N"));
            string outside = Path.Combine(
                Path.GetTempPath(),
                "dimension-brawl-g07-reserve-outside-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            Directory.CreateDirectory(outside);
            try
            {
                const string CaptureId = "g07-failed-reservation";
                string collision = AuditionPvOutputPaths.ResolveOutputDirectory(root, CaptureId);
                Directory.CreateDirectory(collision);
                File.WriteAllText(Path.Combine(collision, "must-survive.txt"), "x");
                string owned = AuditionPvOutputPaths.CreateUniqueOutputDirectory(
                    root,
                    CaptureId);
                string actualCaptureId = new DirectoryInfo(owned).Name;
                Assert.That(actualCaptureId, Does.EndWith("_r002"));
                Directory.CreateDirectory(Path.Combine(owned, "baselines"));
                File.WriteAllText(Path.Combine(owned, "baselines", "partial"), "x");
                AuditionPvStationPhase2PatternRelayCapture
                    .CleanupFailedReservationForRoot(root, actualCaptureId, owned);
                Assert.That(Directory.Exists(owned), Is.False);
                Assert.That(File.Exists(Path.Combine(collision, "must-survive.txt")), Is.True);

                const string BaselineFaultId = "g07-baseline-fault";
                Assert.Throws<IOException>(() =>
                    AuditionPvStationPhase2PatternRelayCapture.ReserveNewOutputForRoot(
                        root,
                        BaselineFaultId,
                        _ => throw new IOException("injected baseline fault")));
                Assert.That(Directory.Exists(
                    AuditionPvOutputPaths.ResolveOutputDirectory(root, BaselineFaultId)),
                    Is.False);

                const string FactoryFaultId = "g07-factory-fault";
                Assert.Throws<IOException>(() =>
                    AuditionPvStationPhase2PatternRelayCapture.ReserveNewOutputForRoot(
                        root,
                        FactoryFaultId,
                        null,
                        _ => throw new IOException("injected factory fault")));
                Assert.That(Directory.Exists(
                    AuditionPvOutputPaths.ResolveOutputDirectory(root, FactoryFaultId)),
                    Is.False);

                string sentinel = Path.Combine(outside, "must-survive.txt");
                File.WriteAllText(sentinel, "x");
                Assert.Throws<InvalidOperationException>(() =>
                    AuditionPvStationPhase2PatternRelayCapture
                        .CleanupFailedReservationForRoot(root, CaptureId, outside));
                Assert.That(File.Exists(sentinel), Is.True);
            }
            finally
            {
                Directory.Delete(root, recursive: true);
                Directory.Delete(outside, recursive: true);
            }
        }

        [Test]
        public void Dependencies_IncludeDirectClosureMetasAndUrp()
        {
            string[] dependencies =
                AuditionPvStationPhase2PatternRelayGoldenRunner
                    .CollectCaptureDependencyPaths();
            foreach (string path in new[]
            {
                AuditionPvStationPhase2PatternRelayCapture.StationScenePath,
                AuditionPvStationPhase2PatternRelayCapture.CaptureScriptPath,
                AuditionPvStationPhase2PatternRelayGoldenRunner.RunnerScriptPath,
                AuditionPvStationPhase2PatternRelayGoldenRunner.RunnerTestPath,
                AuditionPvStationPhase2PatternRelayCapture.EmitterPath,
                AuditionPvStationPhase2PatternRelayCapture.PlayerMovementPath,
                AuditionPvStationPhase2PatternRelayCapture.VisualCueDriverPath,
                AuditionPvStationPhase2PatternRelayCapture.PhaseTwoProjectilePrefabPath
            })
            {
                Assert.That(dependencies, Does.Contain(path), path);
                Assert.That(dependencies, Does.Contain(path + ".meta"), path + ".meta");
            }

            Assert.That(dependencies.Any(path => path.StartsWith(
                "Packages/com.unity.render-pipelines.universal/",
                StringComparison.Ordinal)), Is.True);
        }

        [Test]
        public void ExactEngineProvenance_IsSharedAndRejectsEachMismatch()
        {
            void Validate(
                string unity = "6000.3.5f2",
                string revision = "6000.3.5f2 (3fa8bc678cb0)",
                string recorder = "5.1.6",
                string urp = "17.3.0",
                string pipeline = "Assets/Settings/PC_RPAsset.asset") =>
                AuditionPvStationPhase2PatternRelayGoldenRunner
                    .ValidateExactEngineProvenance(
                        unity, revision, recorder, urp, pipeline);

            Assert.DoesNotThrow(() => Validate());
            Assert.Throws<InvalidOperationException>(() => Validate(unity: "6000.3.6f1"));
            Assert.Throws<InvalidOperationException>(() => Validate(revision: "6000.3.6f1 (bad)"));
            Assert.Throws<InvalidOperationException>(() => Validate(
                revision: "6000.3.5f2 (bad)"));
            Assert.Throws<InvalidOperationException>(() => Validate(
                revision: "6000.3.5f2 (3fa8bc678cb0"));
            Assert.Throws<InvalidOperationException>(() => Validate(recorder: "5.1.7"));
            Assert.Throws<InvalidOperationException>(() => Validate(urp: "17.4.0"));
            Assert.Throws<InvalidOperationException>(() => Validate(pipeline: "Assets/Other.asset"));
        }

        private static AuditionPvStationPhase2PatternRelayGoldenRunner.RuntimeProof
            CreateValidProof()
        {
            string Sha(char value) => new(value, 64);
            var proof = new AuditionPvStationPhase2PatternRelayGoldenRunner.RuntimeProof
            {
                directorCompleted = true,
                lastLogicalFrame = 419,
                presentedFrameCount = 420,
                presentedFramesExact = true,
                presentationClockExact = true,
                transitionCompletedEventCount = 1,
                windupEventCount = 2,
                waveEventCount = 2,
                curtainWindupFrame = 10,
                curtainFireFrame = 68,
                curtainSpawnedCount = 7,
                curtainWasPriority = true,
                hoverWindupFrame = 368,
                hoverFireFrame = 418,
                hoverSpawnedCount = 4,
                hoverWasPriority = false,
                hoverSequenceIndexAfterFire = 1,
                curtainWindupPatternId = "AkazaSummonCurtain",
                curtainFirePatternId = "AkazaSummonCurtain",
                hoverWindupPatternId = "AkazaHoverLance",
                hoverFirePatternId = "AkazaHoverLance",
                emitterTickCount = 420,
                minimumEmitterTimeScale = 1f,
                maximumEmitterTimeScale = 1f,
                runStartedCount = 2,
                stopSettleCount = 2,
                curtainMoveFirstAppliedFrame = 17,
                curtainMoveLastAppliedFrame = 46,
                curtainZeroAppliedFrame = 47,
                hoverMoveFirstAppliedFrame = 374,
                hoverMoveLastAppliedFrame = 406,
                hoverZeroAppliedFrame = 407,
                curtainRiskBefore = 0.8f,
                curtainRiskAfter = 0.6f,
                stayedInsideForwardBoundary = true,
                hoverPreviewCount = 4,
                hoverLateralDisplacement = 1.5f,
                hoverDirectionDot = 0.981f,
                visualWindupDelta = 2,
                visualReleaseDelta = 2,
                telegraphWindupDelta = 2,
                telegraphReleaseDelta = 2,
                cameraWindupDelta = 2,
                cameraFireDelta = 2,
                motionReleaseDelta = 2,
                curtainWindupVisibleMarkerCount = 7,
                curtainFireVisibleMarkerCount = 7,
                hoverWindupVisibleMarkerCount = 4,
                hoverFireVisibleMarkerCount = 4,
                curtainWindupVisibleRendererCount = 7,
                curtainFireVisibleRendererCount = 7,
                hoverWindupVisibleRendererCount = 4,
                hoverFireVisibleRendererCount = 4,
                telegraphMarkerCollidersNonBlocking = true,
                curtainWindupMarkerColor = new Color(0.16f, 1f, 0.66f, 1f),
                curtainFireMarkerColor = new Color(0.75f, 1f, 0.9f, 1f),
                hoverWindupMarkerColor = new Color(0.2f, 0.9f, 1f, 1f),
                hoverFireMarkerColor = new Color(0.72f, 1f, 1f, 1f),
                playerHealthUnchanged = true,
                bossHealthUnchanged = true,
                resourcesUnchanged = true,
                exactHudAndBindings = true,
                exactProjectileAndVfxBindings = true,
                recorderWarmupEndOfFrameCount = 2,
                recorderPaddingActiveAtLogicalFrameZero = true,
                recorderAutoStoppedAfterLastFrame = true,
                stateRestored = true,
                eventsReleased = true,
                presentationClockReleased = true,
                cadenceReleased = true,
                emitterRestored = true,
                spawnOriginOrderRestored = true,
                playerStateRestored = true,
                bossStateRestored = true,
                cameraStateRestored = true,
                hudStateRestored = true,
                globalStateRestored = true,
                postRecordingSettleFrames = 30,
                postRecordingSettleSeconds = 0.5f,
                stationScenePath = AuditionPvStationPhase2PatternRelayCapture.StationScenePath,
                stationSceneSha256 = Sha('f'),
                curtainProfilePath = AuditionPvStationPhase2PatternRelayCapture.CurtainProfilePath,
                curtainProfileGuid = AssetDatabase.AssetPathToGUID(
                    AuditionPvStationPhase2PatternRelayCapture.CurtainProfilePath),
                hoverProfilePath = AuditionPvStationPhase2PatternRelayCapture.HoverProfilePath,
                hoverProfileGuid = AssetDatabase.AssetPathToGUID(
                    AuditionPvStationPhase2PatternRelayCapture.HoverProfilePath),
                dependencyHashCount = 1,
                captureStartProvenanceSha256 = Sha('e'),
                warmupEvidenceSha256 = Sha('1'),
                frame67Sha256 = Sha('a'),
                frame66Sha256 = Sha('2'),
                frame417Sha256 = Sha('c'),
                frame416Sha256 = Sha('3'),
                frame419Sha256 = Sha('e'),
                frameHashLedgerPath = "C:/capture/evidence/frame_hashes.sha256",
                frameHashLedgerSha256 = Sha('4'),
                bl08Sha256 = Sha('b'),
                bl09Sha256 = Sha('d'),
                visualMetrics = new AuditionPvStationPhase2PatternRelayGoldenRunner
                    .SequenceVisualMetrics
                {
                    sampleCount = 100,
                    healthyFrameCount = 420,
                    minimumSampledLuma = 0,
                    maximumSampledLuma = 255
                },
                hudMetrics = new AuditionPvStationPhase2PatternRelayGoldenRunner
                    .HudVisualMetrics
                {
                    frameCount = 420,
                    minimumFramePinkSamples = 540,
                    minimumFrameDarkSamples = 210,
                    minimumFrameBrightSamples = 830,
                    minimumFrameMeanLuma = 140,
                    maximumFrameMeanLuma = 170,
                    roiX = 688,
                    roiY = 8,
                    roiWidth = 176,
                    roiHeight = 176,
                    sampleStride = 4
                },
                curtainWindupDelta = ValidWindupDelta(),
                hoverWindupDelta = ValidWindupDelta(),
                curtainFireDelta = new AuditionPvStationPhase2PatternRelayGoldenRunner
                    .FrameDeltaMetrics { sampleCount = 1, meanAbsoluteRgb = 10 },
                curtainQuietDelta = new AuditionPvStationPhase2PatternRelayGoldenRunner
                    .FrameDeltaMetrics { sampleCount = 1, meanAbsoluteRgb = 1 },
                hoverFireDelta = new AuditionPvStationPhase2PatternRelayGoldenRunner
                    .FrameDeltaMetrics { sampleCount = 1, meanAbsoluteRgb = 10 },
                hoverQuietDelta = new AuditionPvStationPhase2PatternRelayGoldenRunner
                    .FrameDeltaMetrics { sampleCount = 1, meanAbsoluteRgb = 1 },
                curtainWindupColors = new AuditionPvStationPhase2PatternRelayGoldenRunner
                    .PatternColorMetrics
                    {
                        sampleCount = 81,
                        curtainGreenSampleCount = 80,
                        roiWidth = 34,
                        roiHeight = 34,
                        sampleStride = 4
                    },
                hoverWindupColors = new AuditionPvStationPhase2PatternRelayGoldenRunner
                    .PatternColorMetrics
                    {
                        sampleCount = 81,
                        hoverCyanSampleCount = 80,
                        roiWidth = 34,
                        roiHeight = 34,
                        sampleStride = 4
                    }
            };
            proof.renderEvents = new[]
            {
                ValidEvent(10, 7), ValidEvent(68, 7),
                ValidEvent(368, 4), ValidEvent(418, 4), ValidHero()
            };
            return proof;
        }

        private static AuditionPvStationPhase2PatternRelayGoldenRunner.FrameDeltaMetrics
            ValidWindupDelta()
        {
            return new AuditionPvStationPhase2PatternRelayGoldenRunner.FrameDeltaMetrics
            {
                sampleCount = 100,
                changedSampleCount = 8,
                meanAbsoluteRgb = 2,
                changedSampleRatio = 0.08
            };
        }

        private static AuditionPvStationPhase2PatternRelayGoldenRunner.RenderEventEvidence
            ValidEvent(int frame, int markerCount)
        {
            return new AuditionPvStationPhase2PatternRelayGoldenRunner.RenderEventEvidence
            {
                logicalFrame = frame,
                cameraActiveAndEnabled = true,
                cameraPerspective = true,
                cameraFullRect = true,
                cameraTargetTextureNull = true,
                player = ValidSubject(true),
                boss = ValidSubject(true),
                visibleMarkerCount = markerCount,
                visibleMarkerRendererCount = markerCount,
                markerBoundsIntersectFrustum = true,
                allMarkerRenderersIntersectFrustum = true,
                markers = Enumerable.Range(0, markerCount)
                    .Select(_ => ValidSubject(false)).ToArray(),
                markerPixelWidth = 20,
                markerPixelHeight = 20
            };
        }

        private static AuditionPvStationPhase2PatternRelayGoldenRunner.RenderEventEvidence
            ValidHero()
        {
            return new AuditionPvStationPhase2PatternRelayGoldenRunner.RenderEventEvidence
            {
                logicalFrame = 419,
                cameraActiveAndEnabled = true,
                cameraPerspective = true,
                cameraFullRect = true,
                cameraTargetTextureNull = true,
                player = ValidSubject(true),
                boss = ValidSubject(true),
                finalHeroComposition = true
            };
        }

        private static AuditionPvStationPhase2PatternRelayGoldenRunner.SubjectViewportEvidence
            ValidSubject(bool safe)
        {
            return new AuditionPvStationPhase2PatternRelayGoldenRunner.SubjectViewportEvidence
            {
                rendererBoundsFound = true,
                frustumIntersects = true,
                centerInFront = true,
                centerInsideSafeViewport = safe,
                safeViewport = safe,
                viewportCenter = new Vector3(0.5f, 0.5f, 5f),
                pixelWidth = 10,
                pixelHeight = 10
            };
        }

        private static void AssertMutation(
            Action<AuditionPvStationPhase2PatternRelayGoldenRunner.RuntimeProof> mutate)
        {
            var proof = CreateValidProof();
            mutate(proof);
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvStationPhase2PatternRelayGoldenRunner
                    .ValidateRuntimeProofBeforePixelCalibration(proof));
        }

        private static T Load<T>(string path) where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            Assert.That(asset, Is.Not.Null, path);
            return asset;
        }

        private static void AssertRgb(Color actual, float r, float g, float b)
        {
            Assert.That(actual.r, Is.EqualTo(r).Within(0.0001f));
            Assert.That(actual.g, Is.EqualTo(g).Within(0.0001f));
            Assert.That(actual.b, Is.EqualTo(b).Within(0.0001f));
        }

        private static string ReadSource(string path)
        {
            return File.ReadAllText(ProjectAbsolutePath(path));
        }

        private static string ProjectAbsolutePath(string projectRelativePath)
        {
            return Path.GetFullPath(Path.Combine(
                Directory.GetParent(Application.dataPath)?.FullName
                    ?? throw new InvalidOperationException("Project root missing."),
                projectRelativePath));
        }

        private static string Slice(string source, string start, string end)
        {
            int first = source.IndexOf(start, StringComparison.Ordinal);
            int last = source.IndexOf(end, first + start.Length, StringComparison.Ordinal);
            Assert.That(first, Is.GreaterThanOrEqualTo(0), start);
            Assert.That(last, Is.GreaterThan(first), end);
            return source.Substring(first, last - first);
        }

        [Serializable]
#pragma warning disable CS0649 // Populated only by JsonUtility in the executable round-trip test.
        private sealed class CalibrationFailureArtifactProbe
        {
            public string exception = string.Empty;
            public bool pixelCalibrationLocked;
            public AuditionPvStationPhase2PatternRelayGoldenRunner.RuntimeProof runtime;
        }
#pragma warning restore CS0649

        private static void WritePngHeader(string path, int width, int height)
        {
            byte[] bytes = new byte[36];
            byte[] signature = { 137, 80, 78, 71, 13, 10, 26, 10 };
            Array.Copy(signature, bytes, signature.Length);
            bytes[11] = 13;
            bytes[12] = (byte)'I';
            bytes[13] = (byte)'H';
            bytes[14] = (byte)'D';
            bytes[15] = (byte)'R';
            bytes[16] = (byte)(width >> 24);
            bytes[17] = (byte)(width >> 16);
            bytes[18] = (byte)(width >> 8);
            bytes[19] = (byte)width;
            bytes[20] = (byte)(height >> 24);
            bytes[21] = (byte)(height >> 16);
            bytes[22] = (byte)(height >> 8);
            bytes[23] = (byte)height;
            File.WriteAllBytes(path, bytes);
        }
    }
}
