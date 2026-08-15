using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using DimensionBrawl.Combat;
using DimensionBrawl.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace DimensionBrawl.Tests
{
    public sealed class OlympusStationAkazaPhase2FlowPlayModeTests
    {
        private const string FlowTypeName =
            "DimensionBrawl.LevelDesign.OlympusStationAkazaPhase2FlowController";

        private const string StationScenePath =
            "Assets/_Game/Scenes/OlympusStationCombatStage.unity";
        private const string MasterTimelinePath =
            "Assets/_Game/DesignData/Profiles/Cinematics/DB_Timeline_OlympusStationAkazaPhase2Intro.playable";
        private const string BossRootName = "BossBarrageLaneReview_BossProxy_NeedleLock";
        private const string PhaseOneVisualName =
            "BossBarrageLaneReview_HumanoidBossVisual_SciFiSoldier_01_Commando";
        private const string PhaseTwoVisualName = "OlympusStation_AkazaPhase2GameplayVisual";
        private const string TransitionRootName = "OlympusStation_AkazaPhase2TransitionRig";
        private const string CinematicActorName = "AkazaPhase2_CinematicActor";
        private const string WingCameraRigName = "C33_WingDeployCameraRig";
        private const string EyeCameraRigName = "C34_EyeOpenCameraRig";
        private const string C33ActorPath =
            "Assets/_Game/Art/Characters/Bosses/Akaza/Animations/Source/C33_Akaza.fbx";
        private const string C34ActorPath =
            "Assets/_Game/Art/Characters/Bosses/Akaza/Animations/Source/C34_Akaza.fbx";
        private const string C33CameraPath =
            "Assets/_Game/Art/Animations/Cinematics/LegacyCameraGrammar/C33_Cam.fbx";
        private const string C34CameraPath =
            "Assets/_Game/Art/Animations/Cinematics/LegacyCameraGrammar/C34_Cam.fbx";
        private const string PhaseTwoHoldPath =
            "Assets/_Game/Art/Characters/Bosses/Akaza/Animations/Sanitized/DB_Akaza_Phase2_DeployedEyeOpenHold.anim";
        private const string PhaseTwoHeavyReleasePath =
            "Assets/_Game/Art/Characters/Bosses/Akaza/Animations/Sanitized/DB_Akaza_C27_InPlace.anim";
        private const string PhaseTwoProjectilePath =
            "Assets/_Game/Prefabs/Combat/PF_BossBarrageProjectile_AkazaPhase2.prefab";
        private const string PhaseTwoProjectileCoreMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_BossBarrageProjectile_AkazaPhase2_Core.mat";
        private const string PhaseTwoProjectileAccentMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_BossBarrageProjectile_AkazaPhase2_Accent.mat";
        private const string PhaseTwoProjectileTrailMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_BossBarrageProjectile_AkazaPhase2_Trail.mat";
        private const string PhaseTwoProjectileSmokeMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_BossBarrageProjectile_AkazaPhase2_Smoke.mat";
        private const string MagicMissileFlareTexturePath =
            "Assets/_Game/Art/VFX/MagicMissiles/Textures/shared_flare.png";
        private const string MagicMissileRaysTexturePath =
            "Assets/_Game/Art/VFX/MagicMissiles/Textures/shared_rays.png";
        private const string MagicMissileTrailTexturePath =
            "Assets/_Game/Art/VFX/MagicMissiles/Textures/shared_trail.png";
        private const string MagicMissileSmokeTexturePath =
            "Assets/_Game/Art/VFX/MagicMissiles/Textures/shared_smoke.png";
        private const string PhaseTwoLookProfilePath =
            "Assets/_Game/DesignData/Profiles/Cinematics/DB_Volume_OlympusStationAkazaPhase2Intro.asset";
        private const string PhaseTwoLookMaterialFolder =
            "Assets/_Game/Art/Characters/Bosses/Akaza/Generated/Materials";

        [UnityTearDown]
        public IEnumerator RestoreNeutralSceneAfterProductAuthoringCheck()
        {
            Time.timeScale = 1f;
            Scene station = SceneManager.GetSceneByPath(StationScenePath);
            if (!station.IsValid() || !station.isLoaded)
            {
                yield break;
            }

            Scene neutral = SceneManager.CreateScene(
                $"AkazaPhase2TestNeutral_{Guid.NewGuid():N}");
            SceneManager.SetActiveScene(neutral);
            AsyncOperation unload = SceneManager.UnloadSceneAsync(station);
            while (unload != null && !unload.isDone)
            {
                yield return null;
            }
        }

        private const string HoverLancePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_BossBarrage_Phase2_AkazaHoverLance.asset";
        private const string SummonCurtainPath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_BossBarrage_Phase2_AkazaSummonCurtain.asset";
        private const string SpiralVolleyPath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_BossBarrage_Phase2_AkazaSpiralVolley.asset";
        private const string CrushNetPath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_BossBarrage_Phase2_AkazaCrushNet.asset";
        private const string BasicFirePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_BossBasicFire_AkazaPhase2LanePoke.asset";
        private const string ActionDeckPath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_BossPressureActionDeck_AkazaPhase2.asset";
        private const string SummonPressurePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_BossSummonPressure_AkazaPhase2.asset";

        [Test]
        public void PhaseTwoProjectileUsesLayeredMagicMissileMaterialsWithoutRuntimeOverride()
        {
            GameObject projectilePrefab = RequireAsset<GameObject>(PhaseTwoProjectilePath);
            Material core = RequireAsset<Material>(PhaseTwoProjectileCoreMaterialPath);
            Material accent = RequireAsset<Material>(PhaseTwoProjectileAccentMaterialPath);
            Material trail = RequireAsset<Material>(PhaseTwoProjectileTrailMaterialPath);
            Material smoke = RequireAsset<Material>(PhaseTwoProjectileSmokeMaterialPath);

            ParticleSystem coreParticles = RequireChild(
                projectilePrefab,
                "BossBarrageProjectileVfx_MagicMissilesFireShot").GetComponent<ParticleSystem>();
            ParticleSystem waveParticles = RequireChild(
                projectilePrefab,
                "WaveTrail").GetComponent<ParticleSystem>();
            ParticleSystem smokeParticles = RequireChild(
                projectilePrefab,
                "SmokeTrail").GetComponent<ParticleSystem>();
            Assert.That(coreParticles, Is.Not.Null);
            Assert.That(waveParticles, Is.Not.Null);
            Assert.That(smokeParticles, Is.Not.Null);

            Assert.That(
                coreParticles.GetComponent<ParticleSystemRenderer>().sharedMaterial,
                Is.SameAs(core));
            Assert.That(
                waveParticles.GetComponent<ParticleSystemRenderer>().sharedMaterial,
                Is.SameAs(accent));
            Assert.That(
                smokeParticles.GetComponent<ParticleSystemRenderer>().sharedMaterial,
                Is.SameAs(smoke));
            Assert.That(projectilePrefab.GetComponent<TrailRenderer>().sharedMaterial, Is.SameAs(trail));
            Assert.That(projectilePrefab.GetComponent<MeshRenderer>().sharedMaterial, Is.SameAs(core));
            Assert.That(
                RequireChild(projectilePrefab, "FireTrail")
                    .GetComponent<ParticleSystemRenderer>().sharedMaterial,
                Is.SameAs(accent));

            Assert.That(coreParticles.textureSheetAnimation.enabled, Is.False);
            Assert.That(waveParticles.textureSheetAnimation.enabled, Is.False);
            Assert.That(smokeParticles.textureSheetAnimation.enabled, Is.True);
            Assert.That(smokeParticles.textureSheetAnimation.numTilesX, Is.EqualTo(2));
            Assert.That(smokeParticles.textureSheetAnimation.numTilesY, Is.EqualTo(2));
            foreach (ParticleSystem particles in projectilePrefab
                         .GetComponentsInChildren<ParticleSystem>(includeInactive: true))
            {
                ParticleSystem.ColorOverLifetimeModule colorOverLifetime =
                    particles.colorOverLifetime;
                if (!colorOverLifetime.enabled)
                {
                    continue;
                }

                ParticleSystem.MinMaxGradient tint = colorOverLifetime.color;
                Assert.That(
                    tint.mode,
                    Is.EqualTo(ParticleSystemGradientMode.Gradient),
                    $"{particles.name} must expose one deterministic lifetime tint.");
                AssertNeutralGradient(tint.gradient, particles.name);
            }

            AssertNeutralGradient(
                projectilePrefab.GetComponent<TrailRenderer>().colorGradient,
                "projectile trail");

            AssertProjectileMaterial(core, MagicMissileFlareTexturePath, additive: true);
            AssertProjectileMaterial(accent, MagicMissileRaysTexturePath, additive: true);
            AssertProjectileMaterial(trail, MagicMissileTrailTexturePath, additive: true);
            AssertProjectileMaterial(smoke, MagicMissileSmokeTexturePath, additive: false);

            foreach (string profilePath in new[]
                     {
                         HoverLancePath,
                         SummonCurtainPath,
                         SpiralVolleyPath,
                         CrushNetPath
                     })
            {
                Assert.That(
                    RequireAsset<BossBarragePatternProfile>(profilePath).ProjectileMaterial,
                    Is.Null,
                    $"{profilePath} must preserve the prefab's layered particle materials.");
            }

            Assert.That(
                RequireAsset<BossBasicFireProfile>(BasicFirePath).ProjectileMaterial,
                Is.Null,
                "Phase 2 basic fire must preserve the same layered particle materials.");
            Assert.That(
                projectilePrefab.GetComponentsInChildren<Light>(includeInactive: true)
                    .All(light => !light.enabled),
                Is.True,
                "The material upgrade must not restore the projectile's mobile-costly point light.");
        }

        [UnityTest]
        [Timeout(30000)]
        public IEnumerator AuthoredStationUsesOneCanonicalFlowAndExactWingEyeTimelineBindings()
        {
            Time.timeScale = 1f;
            EditorSceneManager.LoadSceneInPlayMode(
                StationScenePath,
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            yield return null;

            Scene scene = SceneManager.GetSceneByPath(StationScenePath);
            Assert.That(scene.IsValid() && scene.isLoaded, Is.True, "Station scene did not load.");

            Type flowType = RequireType(FlowTypeName);
            Component[] flows = FindSceneComponents(scene, flowType);
            Assert.That(flows, Has.Length.EqualTo(1), "Station must own exactly one Phase 2 flow.");
            Component flow = flows[0];

            GameObject bossRoot = RequireSceneObject(scene, BossRootName);
            CombatHealth bossHealth = bossRoot.GetComponent<CombatHealth>();
            Assert.That(bossHealth, Is.Not.Null, "Canonical Station boss root lost CombatHealth.");
            Assert.That(flow.gameObject, Is.SameAs(bossRoot), "Phase 2 flow moved off the boss-health root.");
            Assert.That(ReadPrivate<CombatHealth>(flow, "bossHealth"), Is.SameAs(bossHealth));

            BossBarrageEncounterController[] encounters =
                FindSceneComponents<BossBarrageEncounterController>(scene);
            Assert.That(encounters, Has.Length.EqualTo(1), "Station must retain one encounter owner.");
            Assert.That(
                ReadPrivate<BossBarrageEncounterController>(flow, "bossBarrageEncounterController"),
                Is.SameAs(encounters[0]));
            EnemySummonPacingDirector summonPacing = bossRoot.GetComponent<EnemySummonPacingDirector>();
            Assert.That(summonPacing, Is.Not.Null, "Station lost enemy summon pacing.");
            Assert.That(
                ReadPrivate<EnemySummonPacingDirector>(flow, "enemySummonPacingDirector"),
                Is.SameAs(summonPacing),
                "The transition must own the same summon-pacing lease as the boss root.");
            Assert.That(
                ReadPrivate<CombatHealth>(encounters[0], "bossHealth"),
                Is.SameAs(bossHealth),
                "Phase 1 and Phase 2 must resolve through the same canonical boss health.");
            Assert.That(
                ReadPrivate<CombatHealth>(flow, "playerHealth"),
                Is.SameAs(ReadPrivate<CombatHealth>(encounters[0], "playerHealth")),
                "The transition invulnerability lease must protect the canonical player health.");

            GameObject phaseOne = ReadPrivate<GameObject>(flow, "phaseOneVisualRoot");
            GameObject phaseTwo = ReadPrivate<GameObject>(flow, "phaseTwoVisualRoot");
            GameObject transition = ReadPrivate<GameObject>(flow, "transitionRoot");
            Assert.That(phaseOne.name, Is.EqualTo(PhaseOneVisualName));
            Assert.That(phaseTwo.name, Is.EqualTo(PhaseTwoVisualName));
            Assert.That(transition.name, Is.EqualTo(TransitionRootName));
            Assert.That(phaseOne.transform.IsChildOf(bossRoot.transform), Is.True);
            Assert.That(phaseTwo.transform.IsChildOf(bossRoot.transform), Is.True);
            Assert.That(phaseOne.activeSelf, Is.True, "Station must author Phase 1 visible.");
            Assert.That(phaseTwo.activeSelf, Is.False, "Gameplay Akaza must wait for the handoff.");
            Assert.That(transition.activeSelf, Is.False, "Cinematic rig must be dormant outside transition.");
            Assert.That(transition.scene, Is.EqualTo(scene));

            Assert.That(
                phaseTwo.GetComponentsInChildren<Collider>(includeInactive: true),
                Is.Empty,
                "Floating Phase 2 structures must remain presentation-only.");
            string[] structureBones =
            {
                "CHakazaA:BackParts",
                "CHakazaA:akArmRootA_jnt",
                "CHakazaA:akArmRootB_jnt",
                "CHakazaA:akArmRootC_jnt",
                "CHakazaA:akArmRootD_jnt",
                "CHakazaA:akArmRootE_jnt",
                "CHakazaA:akArmRootF_jnt"
            };
            Transform[] phaseTwoTransforms =
                phaseTwo.GetComponentsInChildren<Transform>(includeInactive: true);
            foreach (string boneName in structureBones)
            {
                Assert.That(
                    phaseTwoTransforms.Any(candidate => candidate.name == boneName),
                    Is.True,
                    $"Gameplay Akaza lost floating-structure bone {boneName}.");
            }

            Transform phaseTwoFireOrigin = ReadPrivate<Transform>(flow, "phaseTwoBasicFireOrigin");
            Assert.That(phaseTwoFireOrigin, Is.Not.Null);
            Assert.That(
                phaseTwoFireOrigin.IsChildOf(phaseTwo.transform),
                Is.True,
                "Phase 2 basic fire must leave from the active Akaza rig, not the hidden Phase 1 muzzle.");
            Transform[] barrageOrigins = ReadPrivate<Transform[]>(flow, "phaseTwoBarrageSpawnOrigins");
            Assert.That(barrageOrigins, Has.Length.EqualTo(6));
            Assert.That(
                barrageOrigins.Select(origin => origin.name).ToArray(),
                Is.EqualTo(new[]
                {
                    "AkazaPhase2_BarrageMuzzle_A",
                    "AkazaPhase2_BarrageMuzzle_F",
                    "AkazaPhase2_BarrageMuzzle_B",
                    "AkazaPhase2_BarrageMuzzle_E",
                    "AkazaPhase2_BarrageMuzzle_C",
                    "AkazaPhase2_BarrageMuzzle_D"
                }),
                "Six-wing barrage origins must alternate mirrored blade pairs deterministically.");
            Assert.That(
                barrageOrigins.All(origin => origin != null && origin.IsChildOf(phaseTwo.transform)),
                Is.True);
            Transform pulseRoot = ReadPrivate<Transform>(flow, "phaseTwoPulseRoot");
            Assert.That(pulseRoot, Is.Not.Null);
            Assert.That(pulseRoot.IsChildOf(phaseTwo.transform), Is.True);
            Renderer[] pulseRenderers = ReadPrivate<Renderer[]>(flow, "phaseTwoPulseRenderers");
            Assert.That(
                pulseRenderers,
                Is.Not.Empty,
                "Akaza patterns must pulse the deployed structure, not a hidden Phase 1 marker.");
            Assert.That(
                pulseRenderers.All(renderer =>
                    renderer != null
                    && renderer.enabled
                    && renderer.transform.IsChildOf(phaseTwo.transform)
                    && (renderer.name == "CHakazaA:BackParts"
                        || renderer.name.StartsWith("CHakazaA:akArm", StringComparison.Ordinal)
                        || renderer.name.StartsWith("CHakazaA:akWp_", StringComparison.Ordinal))),
                Is.True,
                "Phase 2 pulse bindings must target only the six-wing structure renderers.");
            Assert.That(
                pulseRenderers,
                Has.Length.EqualTo(1),
                "All six floating structures must share one gameplay pulse renderer.");
            SkinnedMeshRenderer[] allPhaseTwoRenderers = phaseTwo
                .GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true);
            SkinnedMeshRenderer[] enabledPhaseTwoRenderers = allPhaseTwoRenderers
                .Where(renderer => renderer.enabled)
                .ToArray();
            SkinnedMeshRenderer[] disabledSourceRenderers = allPhaseTwoRenderers
                .Where(renderer => !renderer.enabled
                    && !renderer.transform.IsChildOf(
                        enabledPhaseTwoRenderers[0].transform.parent))
                .ToArray();
            Assert.That(
                enabledPhaseTwoRenderers.Length,
                Is.LessThanOrEqualTo(4),
                "Gameplay Akaza must render through at most four combined skinned meshes.");
            Assert.That(
                enabledPhaseTwoRenderers.Sum(renderer => renderer.sharedMaterials.Length),
                Is.LessThanOrEqualTo(12),
                "Gameplay Akaza must use at most twelve active material slots.");
            Assert.That(
                enabledPhaseTwoRenderers.Count(renderer =>
                    renderer.shadowCastingMode != ShadowCastingMode.Off),
                Is.LessThanOrEqualTo(1),
                "Gameplay Akaza must use at most one combined shadow caster.");
            Assert.That(
                disabledSourceRenderers,
                Is.Not.Empty,
                "The source renderer components must remain disabled on their original animation paths.");
            Assert.That(
                disabledSourceRenderers.Sum(renderer => renderer.sharedMesh.vertexCount),
                Is.EqualTo(enabledPhaseTwoRenderers.Sum(renderer => renderer.sharedMesh.vertexCount)),
                "Combined gameplay meshes must preserve every source vertex.");
            Assert.That(
                disabledSourceRenderers
                    .SelectMany(renderer => renderer.sharedMaterials)
                    .ToHashSet()
                    .SetEquals(enabledPhaseTwoRenderers
                        .SelectMany(renderer => renderer.sharedMaterials)),
                Is.True,
                "Combined gameplay meshes must preserve every canonical source material.");
            Assert.That(
                allPhaseTwoRenderers.All(renderer => !renderer.updateWhenOffscreen),
                Is.True,
                "Gameplay Akaza must not skin every hidden renderer offscreen.");
            Assert.That(
                phaseTwo.GetComponentsInChildren<Renderer>(includeInactive: true)
                    .All(renderer => renderer.motionVectorGenerationMode
                        == MotionVectorGenerationMode.ForceNoMotion),
                Is.True,
                "Gameplay Akaza does not need per-part motion-vector passes.");
            Assert.That(
                pulseRenderers.All(renderer => renderer.shadowCastingMode == ShadowCastingMode.Off),
                Is.True,
                "The 43-part floating structure must not duplicate its material slots in the shadow pass.");
            AkazaPhase2CombatMotionDriver[] motionDrivers =
                phaseTwo.GetComponentsInChildren<AkazaPhase2CombatMotionDriver>(
                    includeInactive: true);
            Assert.That(motionDrivers, Has.Length.EqualTo(1));
            Assert.That(motionDrivers[0].BossHealth, Is.SameAs(bossHealth));
            Assert.That(motionDrivers[0].Animator, Is.SameAs(ReadPrivate<Animator>(flow, "phaseTwoAnimator")));
            Assert.That(motionDrivers[0].ConfiguredWingCount, Is.EqualTo(6));

            GameObject projectilePrefab = RequireAsset<GameObject>(PhaseTwoProjectilePath);
            Assert.That(
                projectilePrefab.GetComponentsInChildren<Light>(includeInactive: true)
                    .All(light => !light.enabled),
                Is.True,
                "Phase 2 projectile bursts may not create an unbounded moving-light swarm.");

            AnimationClip holdClip = RequireAsset<AnimationClip>(PhaseTwoHoldPath);
            AnimationClip heavyReleaseClip = RequireAsset<AnimationClip>(PhaseTwoHeavyReleasePath);
            Animator phaseTwoAnimator = ReadPrivate<Animator>(flow, "phaseTwoAnimator");
            Assert.That(phaseTwoAnimator.runtimeAnimatorController, Is.Not.Null);
            Assert.That(
                phaseTwoAnimator.runtimeAnimatorController.animationClips,
                Does.Contain(holdClip),
                "Gameplay Akaza must preserve the deployed C34 terminal pose after reveal.");
            AnimatorController controller = phaseTwoAnimator.runtimeAnimatorController as AnimatorController;
            Assert.That(controller, Is.Not.Null);
            AnimatorState[] authoredStates = controller.layers
                .SelectMany(layer => layer.stateMachine.states)
                .Select(child => child.state)
                .Where(state => state != null)
                .ToArray();
            Assert.That(authoredStates, Is.Not.Empty);
            Assert.That(
                authoredStates.All(state => state.motion == holdClip),
                Is.False,
                "Phase 2 may not remain a static terminal-pose model through every attack.");
            Assert.That(
                authoredStates.Single(state => state.name == "Hover").motion,
                Is.SameAs(holdClip),
                "Hover must preserve the deployed C34 terminal pose after reveal.");
            foreach (string attackState in new[]
                     {
                         "BasicAttack",
                         "LinePressure",
                         "FanPressure",
                         "RetreatShot",
                         "Windup",
                         "HeavyCrush"
                     })
            {
                Assert.That(
                    authoredStates.Single(state => state.name == attackState).motion,
                    Is.SameAs(heavyReleaseClip),
                    $"{attackState} must articulate the six-wing C27 release instead of holding still.");
            }
            Assert.That(
                authoredStates
                    .Where(state => state.name is "Hit" or "Death")
                    .All(state => state.motion == holdClip),
                Is.True,
                "Hit/death states keep the stable deployed pose for the procedural reaction owner.");

            PlayableAsset timeline = RequireAsset<PlayableAsset>(MasterTimelinePath);
            Assert.That(
                ReadNumericProperty(timeline, "fixedDuration"),
                Is.EqualTo(3.9666667d).Within(0.0001d));
            PlayableBinding[] outputs = timeline.outputs.ToArray();
            Assert.That(outputs, Has.Length.EqualTo(3));

            PlayableBinding actorTrack = RequireOutput(
                outputs,
                "Akaza Actor - C33 Deploy then C34 Eye Open");
            PlayableBinding wingCameraTrack = RequireOutput(outputs, "C33 Wing Deploy Camera");
            PlayableBinding eyeCameraTrack = RequireOutput(outputs, "C34 Eye Open Camera");
            AssertAnimationTrack(
                actorTrack,
                new ClipExpectation(0d, 6.1d, 1.6d, C33ActorPath),
                new ClipExpectation(1.6d, 0d, 2.3666667d, C34ActorPath));
            AssertAnimationTrack(
                wingCameraTrack,
                new ClipExpectation(0d, 6.1d, 1.6d, C33CameraPath));
            AssertAnimationTrack(
                eyeCameraTrack,
                new ClipExpectation(1.6d, 0d, 2.3666667d, C34CameraPath));

            PlayableDirector director = ReadPrivate<PlayableDirector>(flow, "transitionDirector");
            Assert.That(director, Is.Not.Null);
            Assert.That(director.transform.IsChildOf(transition.transform), Is.True);
            Assert.That(director.playableAsset, Is.SameAs(timeline));

            GameObject cinematicActor = RequireChild(transition, CinematicActorName);
            GameObject wingCameraRig = RequireChild(transition, WingCameraRigName);
            GameObject eyeCameraRig = RequireChild(transition, EyeCameraRigName);
            AssertAnimatorBinding(director, actorTrack, cinematicActor);
            AssertAnimatorBinding(director, wingCameraTrack, wingCameraRig);
            AssertAnimatorBinding(director, eyeCameraTrack, eyeCameraRig);
            Assert.That(
                ReadPrivate<Camera>(flow, "wingDeployCamera").transform.IsChildOf(wingCameraRig.transform),
                Is.True);
            Assert.That(
                ReadPrivate<Camera>(flow, "eyeOpenCamera").transform.IsChildOf(eyeCameraRig.transform),
                Is.True);

            AssertAuthoredSourceSoftLook(
                transition,
                cinematicActor,
                phaseTwo,
                ReadPrivate<Camera>(flow, "wingDeployCamera"),
                ReadPrivate<Camera>(flow, "eyeOpenCamera"));
        }

        private static void AssertAuthoredSourceSoftLook(
            GameObject transition,
            GameObject cinematicActor,
            GameObject gameplayActor,
            Camera wingCamera,
            Camera eyeCamera)
        {
            AkazaPhase2CinematicLookDriver[] lookDrivers =
                transition.GetComponentsInChildren<AkazaPhase2CinematicLookDriver>(
                    includeInactive: true);
            Assert.That(lookDrivers, Has.Length.EqualTo(1));
            AkazaPhase2CinematicLookDriver look = lookDrivers[0];
            Assert.That(look.EyeOpenStartSeconds, Is.EqualTo(1.6d).Within(0.0001d));
            Assert.That(look.SuppressedDirectionalLightCount, Is.GreaterThan(0));
            Assert.That(
                look.LightingLeaseHeld,
                Is.False,
                "An inactive transition must not keep the Station lighting lease.");
            SerializedProperty suppressedLights = new SerializedObject(look)
                .FindProperty("suppressedDirectionalLights");
            Assert.That(suppressedLights, Is.Not.Null);
            int enabledGameplayLightCount = 0;
            for (int index = 0; index < suppressedLights.arraySize; index++)
            {
                Light stationLight = suppressedLights.GetArrayElementAtIndex(index)
                    .objectReferenceValue as Light;
                Assert.That(stationLight, Is.Not.Null);
                if (stationLight.name == "Directional Light"
                    || stationLight.name == "HeavenlyCorridorSun"
                    || stationLight.name == "PaleProductStageWash")
                {
                    enabledGameplayLightCount++;
                    Assert.That(
                        stationLight.enabled,
                        Is.True,
                        $"Inactive transition leaked a disabled Station light: {stationLight.name}.");
                }
            }

            Assert.That(enabledGameplayLightCount, Is.EqualTo(3));
            Assert.That(
                look.AppliesCinematicFog,
                Is.False,
                "The source-soft pass must not tint the Olympus stage with the legacy brown fog.");
            Assert.That(look.CinematicFogMode, Is.EqualTo(FogMode.Linear));
            Assert.That(look.CinematicFogStartDistance, Is.EqualTo(-30.1f).Within(0.001f));
            Assert.That(look.CinematicFogEndDistance, Is.EqualTo(600f).Within(0.001f));
            Assert.That(
                Vector4.Distance(
                    look.CinematicFogColor,
                    new Color(0.2941f, 0.2197f, 0.1081f, 0.604f)),
                Is.LessThan(0.0001f));

            bool previousFog = RenderSettings.fog;
            Color previousFogColor = RenderSettings.fogColor;
            FogMode previousFogMode = RenderSettings.fogMode;
            float previousFogDensity = RenderSettings.fogDensity;
            float previousFogStart = RenderSettings.fogStartDistance;
            float previousFogEnd = RenderSettings.fogEndDistance;
            PlayableDirector lookDirector = ReadPrivate<PlayableDirector>(look, "director");
            double previousDirectorTime = lookDirector.time;
            try
            {
                look.BeginManualLightingLease();
                for (int index = 0; index < suppressedLights.arraySize; index++)
                {
                    Light stationLight = suppressedLights.GetArrayElementAtIndex(index)
                        .objectReferenceValue as Light;
                    if (stationLight != null)
                    {
                        Assert.That(
                            stationLight.enabled,
                            Is.False,
                            $"Cinematic lease failed to suppress Station light {stationLight.name}.");
                    }
                }

                Assert.That(RenderSettings.fog, Is.EqualTo(previousFog));
                Assert.That(RenderSettings.fogColor, Is.EqualTo(previousFogColor));
                Assert.That(RenderSettings.fogMode, Is.EqualTo(previousFogMode));
                Assert.That(RenderSettings.fogDensity, Is.EqualTo(previousFogDensity).Within(0.0001f));
                Assert.That(RenderSettings.fogStartDistance, Is.EqualTo(previousFogStart).Within(0.0001f));
                Assert.That(RenderSettings.fogEndDistance, Is.EqualTo(previousFogEnd).Within(0.0001f));
                lookDirector.time = 0d;
                look.ApplyCurrentTime();
                Assert.That(look.WingDeployKey.enabled, Is.True);
                Assert.That(look.EyeOpenKey.enabled, Is.False);
                Assert.That(look.BackgroundKey.enabled, Is.True);
                lookDirector.time = look.EyeOpenStartSeconds;
                look.ApplyCurrentTime();
                Assert.That(look.WingDeployKey.enabled, Is.False);
                Assert.That(look.EyeOpenKey.enabled, Is.True);
                Assert.That(look.BackgroundKey.enabled, Is.True);
            }
            finally
            {
                lookDirector.time = previousDirectorTime;
                look.ApplyCurrentTime();
                look.EndManualLightingLease();
            }

            Assert.That(RenderSettings.fog, Is.EqualTo(previousFog));
            Assert.That(RenderSettings.fogColor, Is.EqualTo(previousFogColor));
            Assert.That(RenderSettings.fogMode, Is.EqualTo(previousFogMode));
            Assert.That(RenderSettings.fogDensity, Is.EqualTo(previousFogDensity).Within(0.0001f));
            Assert.That(RenderSettings.fogStartDistance, Is.EqualTo(previousFogStart).Within(0.0001f));
            Assert.That(RenderSettings.fogEndDistance, Is.EqualTo(previousFogEnd).Within(0.0001f));
            Assert.That(look.WingDeployKey.shadows, Is.EqualTo(LightShadows.None));
            Assert.That(look.EyeOpenKey.shadows, Is.EqualTo(LightShadows.Soft));
            Assert.That(look.EyeOpenKey.shadowStrength, Is.EqualTo(0.5f).Within(0.001f));
            Assert.That(look.WingDeployKey.intensity, Is.EqualTo(1.42f).Within(0.001f));
            Assert.That(look.EyeOpenKey.intensity, Is.EqualTo(1.42f).Within(0.001f));
            Assert.That(
                Vector4.Distance(look.WingDeployKey.color, Color.white),
                Is.LessThan(0.0001f));
            Assert.That(
                Vector4.Distance(look.EyeOpenKey.color, Color.white),
                Is.LessThan(0.0001f));
            Assert.That(
                Vector4.Distance(look.BackgroundKey.color, Color.white),
                Is.LessThan(0.0001f));
            Assert.That(look.WingDeployKey.cullingMask, Is.EqualTo(1 << 9));
            Assert.That(look.EyeOpenKey.cullingMask, Is.EqualTo(1 << 9));
            Assert.That(
                cinematicActor.GetComponentsInChildren<Transform>(includeInactive: true)
                    .All(transform => transform.gameObject.layer == 9),
                Is.True,
                "The neutral cinematic key must own the isolated Akaza layer.");

            Volume[] volumes = transition.GetComponentsInChildren<Volume>(includeInactive: true);
            Assert.That(volumes, Has.Length.EqualTo(1));
            Assert.That(volumes[0].isGlobal, Is.True);
            Assert.That(volumes[0].priority, Is.GreaterThan(200f));
            VolumeProfile profile = RequireAsset<VolumeProfile>(PhaseTwoLookProfilePath);
            Assert.That(volumes[0].sharedProfile, Is.SameAs(profile));
            Assert.That(profile.TryGet(out Tonemapping tonemapping), Is.True);
            Assert.That(tonemapping.mode.value, Is.EqualTo(TonemappingMode.Neutral));
            Assert.That(profile.TryGet(out WhiteBalance whiteBalance), Is.True);
            Assert.That(whiteBalance.temperature.value, Is.EqualTo(0f).Within(0.001f));
            Assert.That(whiteBalance.tint.value, Is.EqualTo(0f).Within(0.001f));
            Assert.That(profile.TryGet(out ColorAdjustments color), Is.True);
            Assert.That(color.postExposure.value, Is.EqualTo(0.2f).Within(0.001f));
            Assert.That(color.saturation.value, Is.EqualTo(-10.7f).Within(0.001f));
            Assert.That(profile.TryGet(out LiftGammaGain wheels), Is.True);
            (Vector4 preparedLift, Vector4 preparedGamma, Vector4 preparedGain) =
                ColorUtils.PrepareLiftGammaGain(
                    wheels.lift.value,
                    wheels.gamma.value,
                    wheels.gain.value);
            Assert.That(
                Vector4.Distance(
                    preparedLift,
                    Vector4.zero),
                Is.LessThan(0.0001f));
            Assert.That(
                Vector4.Distance(
                    preparedGamma,
                    new Vector4(1f, 1f, 1f, 0f)),
                Is.LessThan(0.0001f));
            Assert.That(
                Vector4.Distance(
                    preparedGain,
                    new Vector4(1f, 1f, 1f, 0f)),
                Is.LessThan(0.0001f));
            Assert.That(profile.TryGet(out Bloom bloom), Is.True);
            Assert.That(bloom.intensity.value, Is.EqualTo(0.7f).Within(0.001f));
            Assert.That(bloom.threshold.value, Is.EqualTo(1f).Within(0.001f));
            Assert.That(bloom.scatter.value, Is.EqualTo(0.85f).Within(0.001f));
            Assert.That(bloom.downscale.value, Is.EqualTo(BloomDownscaleMode.Half));
            Assert.That(bloom.maxIterations.value, Is.EqualTo(8));
            Assert.That(bloom.highQualityFiltering.value, Is.True);
            Assert.That(profile.TryGet(out ChromaticAberration chromatic), Is.True);
            Assert.That(chromatic.intensity.value, Is.EqualTo(0.15f).Within(0.001f));
            Assert.That(profile.TryGet(out Vignette vignette), Is.True);
            Assert.That(vignette.intensity.value, Is.EqualTo(0.1834666f).Within(0.001f));

            foreach (Camera camera in new[] { wingCamera, eyeCamera })
            {
                UniversalAdditionalCameraData data =
                    camera.GetComponent<UniversalAdditionalCameraData>();
                Assert.That(data, Is.Not.Null);
                Assert.That(data.renderPostProcessing, Is.True);
                Assert.That(
                    data.antialiasing,
                    Is.EqualTo(AntialiasingMode.SubpixelMorphologicalAntiAliasing));
            }

            Material[] expectedMaterials = new[]
            {
                "Arm",
                "Body",
                "Face",
                "Eyes",
                "HairSpow",
                "Skin"
            }
                .Select(suffix => RequireAsset<Material>(
                    $"{PhaseTwoLookMaterialFolder}/M_Akaza_Phase2_{suffix}_SourceSoft.mat"))
                .ToArray();
            Material[] cinematicMaterials = cinematicActor
                .GetComponentsInChildren<Renderer>(includeInactive: true)
                .SelectMany(renderer => renderer.sharedMaterials)
                .Where(material => material != null)
                .ToArray();
            Material[] gameplayMaterials = gameplayActor
                .GetComponentsInChildren<Renderer>(includeInactive: true)
                .SelectMany(renderer => renderer.sharedMaterials)
                .Where(material => material != null)
                .ToArray();
            foreach (Material material in expectedMaterials)
            {
                Assert.That(cinematicMaterials, Does.Contain(material));
                Assert.That(gameplayMaterials, Does.Contain(material));
                Assert.That(material.GetFloat("_utsTechnique"), Is.EqualTo(1f));
                Assert.That(material.IsKeywordEnabled("_SHADINGGRADEMAP"), Is.True);
                Assert.That(material.GetColor("_Outline_Color"), Is.EqualTo(Color.black));
            }

            Assert.That(
                expectedMaterials[0].GetColor("_BaseColor"),
                Is.EqualTo(new Color(0.8f, 0.8f, 0.8f, 1f)));
            Assert.That(
                expectedMaterials[1].GetColor("_BaseColor"),
                Is.EqualTo(new Color(0.637f, 0.637f, 0.637f, 1f)));
            Assert.That(
                expectedMaterials[5].GetColor("_BaseColor"),
                Is.EqualTo(new Color(0.637f, 0.637f, 0.637f, 1f)));
        }

        [UnityTest]
        [Timeout(30000)]
        public IEnumerator AuthoredStationRunsIntegratedPhaseTwoCombatPathThroughTerminalCleanup()
        {
            Time.timeScale = 1f;
            EditorSceneManager.LoadSceneInPlayMode(
                StationScenePath,
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            yield return null;

            Scene scene = SceneManager.GetSceneByPath(StationScenePath);
            Assert.That(scene.IsValid() && scene.isLoaded, Is.True, "Station scene did not load.");

            Type flowType = RequireType(FlowTypeName);
            Component[] flows = FindSceneComponents(scene, flowType);
            Assert.That(flows, Has.Length.EqualTo(1));
            Component flow = flows[0];

            CombatHealth bossHealth = ReadPublic<CombatHealth>(flow, "BossHealth");
            CombatHealth playerHealth = ReadPrivate<CombatHealth>(flow, "playerHealth");
            BossBarrageEncounterController encounter =
                ReadPrivate<BossBarrageEncounterController>(flow, "bossBarrageEncounterController");
            BossBarrageEmitter barrage = ReadPrivate<BossBarrageEmitter>(flow, "bossBarrageEmitter");
            BossBasicFireEmitter basicFire =
                ReadPrivate<BossBasicFireEmitter>(flow, "bossBasicFireEmitter");
            BossPressureActionDirector actionDirector =
                ReadPrivate<BossPressureActionDirector>(flow, "bossPressureActionDirector");
            BossSummonPressureAction summonPressure =
                ReadPrivate<BossSummonPressureAction>(flow, "bossSummonPressureAction");
            EnemySummonPacingDirector enemyPacing =
                ReadPrivate<EnemySummonPacingDirector>(flow, "enemySummonPacingDirector");
            BossPressurePositionController positionController =
                ReadPrivate<BossPressurePositionController>(flow, "bossPressurePositionController");
            GameObject phaseOneVisual = ReadPrivate<GameObject>(flow, "phaseOneVisualRoot");
            GameObject phaseTwoVisual = ReadPrivate<GameObject>(flow, "phaseTwoVisualRoot");
            Transform[] phaseTwoMuzzles =
                ReadPrivate<Transform[]>(flow, "phaseTwoBarrageSpawnOrigins");
            AkazaPhase2CombatMotionDriver[] motionDrivers =
                phaseTwoVisual.GetComponentsInChildren<AkazaPhase2CombatMotionDriver>(
                    includeInactive: true);

            Assert.That(bossHealth, Is.Not.Null);
            Assert.That(playerHealth, Is.Not.Null);
            Assert.That(encounter, Is.Not.Null);
            Assert.That(barrage, Is.Not.Null);
            Assert.That(basicFire, Is.Not.Null);
            Assert.That(actionDirector, Is.Not.Null);
            Assert.That(summonPressure, Is.Not.Null);
            Assert.That(enemyPacing, Is.Not.Null);
            Assert.That(positionController, Is.Not.Null);
            Assert.That(motionDrivers, Has.Length.EqualTo(1));
            Assert.That(phaseTwoMuzzles, Has.Length.EqualTo(6));
            Assert.That(ReadPhase(flow), Is.EqualTo("Phase1"));
            Assert.That(bossHealth.IsAlive, Is.True);
            Assert.That(phaseOneVisual.activeSelf, Is.True);
            Assert.That(phaseTwoVisual.activeSelf, Is.False);

            float thresholdHealth = bossHealth.MaxHealth
                * ReadPublic<float>(flow, "PhaseThreshold01");
            Assert.That(
                ApplyAuthoredPlayerDamage(bossHealth, playerHealth, bossHealth.MaxHealth),
                Is.True,
                "The canonical player must be able to cross the authored Phase 2 threshold.");
            Assert.That(bossHealth.CurrentHealth, Is.EqualTo(thresholdHealth).Within(0.001f));
            Assert.That(ReadPhase(flow), Is.EqualTo("Transitioning"));
            Assert.That(TrySkip(flow), Is.True);

            yield return WaitForPhase(flow, "Phase2", 5f);

            AkazaPhase2CombatMotionDriver motionDriver = motionDrivers[0];
            Assert.That(ReadPublic<bool>(flow, "PhaseTwoApplied"), Is.True);
            Assert.That(phaseOneVisual.activeSelf, Is.False);
            Assert.That(phaseTwoVisual.activeSelf, Is.True);
            Assert.That(bossHealth.CurrentHealth, Is.EqualTo(thresholdHealth).Within(0.001f));
            Assert.That(encounter.IsRunning, Is.True);
            Assert.That(
                encounter.CurrentPhase,
                Is.EqualTo(BossBarrageEncounterController.EncounterPhase.SummonBlock));
            Assert.That(encounter.CloseThreatDefeated, Is.True);
            Assert.That(encounter.IsExternalCombatSuspended, Is.False);
            Assert.That(motionDriver.BossHealth, Is.SameAs(bossHealth));
            Assert.That(motionDriver.BossBarrageEmitter, Is.SameAs(barrage));
            Assert.That(motionDriver.BossBasicFireEmitter, Is.SameAs(basicFire));
            Assert.That(motionDriver.HeavyReleaseTrigger, Is.EqualTo("AttackHeavy"));

            Transform[] liveEmitterOrigins =
                ReadPrivate<Transform[]>(barrage, "projectileSpawnOrigins");
            Assert.That(liveEmitterOrigins, Is.EqualTo(phaseTwoMuzzles));
            Assert.That(
                liveEmitterOrigins.All(origin =>
                    origin != null
                    && origin.IsChildOf(phaseTwoVisual.transform)
                    && origin.name.StartsWith("AkazaPhase2_BarrageMuzzle_", StringComparison.Ordinal)
                    && !origin.name.Contains("BossProxy", StringComparison.Ordinal)),
                Is.True,
                "The live Phase 2 emitter must use only the six Akaza blade muzzles.");

            encounter.Tick(encounter.PressureReliefRemainingSeconds + 0.05f);
            Assert.That(encounter.IsPressureReliefActive, Is.False);
            Assert.That(barrage.IsFiringEnabled, Is.True);
            Assert.That(basicFire.IsFiringEnabled, Is.True);
            Assert.That(actionDirector.ActionsEnabled, Is.True);
            Assert.That(enemyPacing.PacingEnabled, Is.True);
            Assert.That(positionController.MovementEnabled, Is.True);

            BossBarragePatternProfile openingPattern =
                ReadPrivate<BossBarragePatternProfile>(flow, "phaseTwoOpeningPattern");
            Assert.That(
                openingPattern,
                Is.SameAs(RequireAsset<BossBarragePatternProfile>(SummonCurtainPath)));
            Assert.That(barrage.QueuedPriorityPattern, Is.SameAs(openingPattern));
            BossBarragePatternProfile firedPattern = null;
            barrage.WaveFired += (_, pattern, _) => firedPattern = pattern;
            int heavyRequestsBeforeWindup = motionDriver.HeavyReleaseRequestCount;
            int[] activeProjectileIdsBeforeWave = ReadBarrageProjectilePool(barrage)
                .Where(projectile => projectile.IsActive)
                .Select(projectile => projectile.GetInstanceID())
                .ToArray();
            Assert.That(barrage.BeginWindup(), Is.True);
            Assert.That(barrage.IsWindupActive, Is.True);
            Assert.That(
                motionDriver.HeavyReleaseRequestCount,
                Is.EqualTo(heavyRequestsBeforeWindup + 1));
            Assert.That(motionDriver.IsHeavyReleaseActive, Is.True);
            Assert.That(barrage.CurrentPattern, Is.SameAs(openingPattern));
            Vector3[] launchMuzzlePositions = liveEmitterOrigins
                .Select(origin => origin.position)
                .ToArray();

            int firedProjectileCount = barrage.FirePendingWave();
            BossBarrageProjectile[] launchedProjectiles = ReadBarrageProjectilePool(barrage)
                .Where(projectile =>
                    projectile.IsActive
                    && !activeProjectileIdsBeforeWave.Contains(projectile.GetInstanceID()))
                .ToArray();
            Assert.That(firedProjectileCount, Is.GreaterThan(0));
            Assert.That(
                launchedProjectiles,
                Has.Length.EqualTo(firedProjectileCount),
                "The integrated check must observe every projectile created by the driven wave.");
            Assert.That(firedPattern, Is.SameAs(openingPattern));
            Assert.That(barrage.LastFiredWaveWasPriority, Is.True);
            foreach (BossBarrageProjectile projectile in launchedProjectiles)
            {
                AssertProjectileLaunchedFromAkazaMuzzle(
                    projectile,
                    liveEmitterOrigins,
                    launchMuzzlePositions);
            }

            int basicProjectileCount = basicFire.FireVolley();
            Assert.That(basicProjectileCount, Is.GreaterThan(0));
            Assert.That(basicFire.ActiveProjectileCount, Is.EqualTo(basicProjectileCount));

            Assert.That(summonPressure.ActiveSummonActorCount, Is.Zero);
            Assert.That(
                summonPressure.TryReleasePressureSummon(1),
                Is.True,
                "The authored Phase 2 summon owner must be live before terminal cleanup.");
            Assert.That(summonPressure.ActiveSummonActorCount, Is.GreaterThan(0));

            int hitRequestsBeforeDamage = motionDriver.HitReactionRequestCount;
            float healthBeforeHit = bossHealth.CurrentHealth;
            Assert.That(ApplyAuthoredPlayerDamage(bossHealth, playerHealth, 1f), Is.True);
            Assert.That(bossHealth.CurrentHealth, Is.EqualTo(healthBeforeHit - 1f).Within(0.001f));
            Assert.That(
                motionDriver.HitReactionRequestCount,
                Is.EqualTo(hitRequestsBeforeDamage + 1));
            Assert.That(motionDriver.IsHitReactionActive, Is.True);

            int deathRequestsBeforeLethal = motionDriver.DeathRequestCount;
            Assert.That(
                ApplyAuthoredPlayerDamage(bossHealth, playerHealth, bossHealth.MaxHealth * 2f),
                Is.True);
            Assert.That(bossHealth.IsAlive, Is.False);
            Assert.That(ReadPublic<bool>(flow, "BossTerminalized"), Is.True);
            Assert.That(encounter.IsExternalCombatSuspended, Is.True);
            Assert.That(barrage.IsFiringEnabled, Is.False);
            Assert.That(barrage.ActiveProjectileCount, Is.Zero);
            Assert.That(basicFire.IsFiringEnabled, Is.False);
            Assert.That(basicFire.ActiveProjectileCount, Is.Zero);
            Assert.That(actionDirector.ActionsEnabled, Is.False);
            Assert.That(enemyPacing.PacingEnabled, Is.False);
            Assert.That(positionController.MovementEnabled, Is.False);
            Assert.That(summonPressure.ActiveSummonActorCount, Is.Zero);
            Assert.That(motionDriver.IsDead, Is.True);
            Assert.That(motionDriver.AttacksStopped, Is.True);
            Assert.That(
                motionDriver.DeathRequestCount,
                Is.EqualTo(deathRequestsBeforeLethal + 1));

            motionDriver.TickPresentation(motionDriver.DeathSettleDurationSeconds + 0.1f);
            Assert.That(motionDriver.DeathProgress01, Is.EqualTo(1f).Within(0.0001f));
        }

        [UnityTest]
        [Timeout(10000)]
        public IEnumerator ThresholdDamageCapsAtHalfAndBlocksFurtherDamageUntilSkip()
        {
            using var fixture = new FlowFixture(TransitionAssetMode.Long);
            yield return null;

            Assert.That(ReadPhase(fixture.Flow), Is.EqualTo("Phase1"));
            Assert.That(ReadPublic<CombatHealth>(fixture.Flow, "BossHealth"), Is.SameAs(fixture.BossHealth));
            Assert.That(fixture.BossHealth.MaxHealth, Is.EqualTo(1000f).Within(0.001f));
            Assert.That(
                fixture.Barrage.GetProjectilePoolCountForPrefab(fixture.PhaseTwoProjectile),
                Is.EqualTo(3),
                "Phase 2 barrage projectiles must be ready before the threshold frame.");
            Assert.That(
                fixture.BasicFire.GetProjectilePoolCountForPrefab(fixture.PhaseTwoProjectile),
                Is.EqualTo(2),
                "Phase 2 basic projectiles must be ready before the threshold frame.");

            int acceptedDamageCount = 0;
            fixture.BossHealth.Damaged += _ => acceptedDamageCount++;

            Assert.That(fixture.ApplyPlayerDamage(350f), Is.True);
            Assert.That(fixture.BossHealth.CurrentHealth, Is.EqualTo(650f).Within(0.001f));
            Assert.That(ReadPhase(fixture.Flow), Is.EqualTo("Phase1"));

            Assert.That(fixture.ApplyPlayerDamage(400f), Is.True);
            Assert.That(
                fixture.BossHealth.CurrentHealth,
                Is.EqualTo(500f).Within(0.001f),
                "The crossing hit must be capped at the exact phase threshold.");
            Assert.That(ReadPhase(fixture.Flow), Is.EqualTo("Transitioning"));
            Assert.That(ReadPublic<int>(fixture.Flow, "TransitionStartCount"), Is.EqualTo(1));
            Assert.That(ReadPublic<int>(fixture.Flow, "TransitionCompletionCount"), Is.Zero);
            Assert.That(
                fixture.PhaseOneVisual.activeSelf,
                Is.False,
                "The Phase 1 body must not overlap the cinematic Akaza actor.");
            Assert.That(acceptedDamageCount, Is.EqualTo(2));
            Assert.That(
                fixture.EnemyPacing.PacingEnabled,
                Is.False,
                "Enemy summon pacing must be suspended for the entire cinematic.");
            Assert.That(ReadPublic<bool>(fixture.Flow, "PlayerDamageLeaseActive"), Is.True);
            Assert.That(
                fixture.ApplyBossDamageToPlayer(25f),
                Is.False,
                "The input-locked player must be invulnerable for the complete transition lease.");
            Assert.That(fixture.SourceHealth.CurrentHealth, Is.EqualTo(100f).Within(0.001f));

            Assert.That(
                fixture.ApplyPlayerDamage(120f),
                Is.False,
                "Transition protection must reject actual CombatHealth damage, not only hide feedback.");
            Assert.That(fixture.BossHealth.CurrentHealth, Is.EqualTo(500f).Within(0.001f));
            Assert.That(acceptedDamageCount, Is.EqualTo(2));
            Assert.That(ReadPublic<int>(fixture.Flow, "TransitionStartCount"), Is.EqualTo(1));

            Assert.That(TrySkip(fixture.Flow), Is.True);
            Assert.That(
                TrySkip(fixture.Flow),
                Is.False,
                "The black-cover/reveal handoff may only be started once.");
            yield return WaitForPhase(fixture.Flow, "Phase2", 2f);

            AssertPhaseTwoCompletedOnce(fixture);
            Assert.That(TrySkip(fixture.Flow), Is.False, "A completed transition cannot be skipped again.");
            Assert.That(ReadPublic<int>(fixture.Flow, "TransitionStartCount"), Is.EqualTo(1));
            Assert.That(ReadPublic<int>(fixture.Flow, "TransitionCompletionCount"), Is.EqualTo(1));
            Assert.That(fixture.EnemyPacing.PacingEnabled, Is.True);
            Assert.That(ReadPublic<bool>(fixture.Flow, "PlayerDamageLeaseActive"), Is.False);
            Assert.That(
                fixture.ApplyBossDamageToPlayer(10f),
                Is.True,
                "Reveal cleanup must release only the temporary transition invulnerability.");
            Assert.That(fixture.SourceHealth.CurrentHealth, Is.EqualTo(90f).Within(0.001f));

            Assert.That(
                fixture.ApplyPlayerDamage(25f),
                Is.True,
                "Skip cleanup must release phase-transition damage protection immediately.");
            Assert.That(fixture.BossHealth.CurrentHealth, Is.EqualTo(475f).Within(0.001f));
            Assert.That(acceptedDamageCount, Is.EqualTo(3));
        }

        [UnityTest]
        [Timeout(10000)]
        public IEnumerator MovingPhaseOneBossCarriesTheAuthoredTransitionRigWithoutWorldSnap()
        {
            using var fixture = new FlowFixture(TransitionAssetMode.Long);
            yield return null;

            Vector3 authoredTransitionPosition = fixture.TransitionRoot.transform.position;
            Quaternion authoredTransitionRotation = fixture.TransitionRoot.transform.rotation;
            Vector3 authoredLocalOffset = ReadPrivate<Vector3>(
                fixture.Flow,
                "transitionRootBossLocalPosition");
            Quaternion authoredLocalRotation = ReadPrivate<Quaternion>(
                fixture.Flow,
                "transitionRootBossLocalRotation");
            Assert.That(
                Vector3.Distance(
                    fixture.Root.transform.TransformPoint(authoredLocalOffset),
                    authoredTransitionPosition),
                Is.LessThan(0.0001f),
                "The runtime anchor must capture the authored transition offset before combat movement.");
            Assert.That(
                Quaternion.Angle(
                    fixture.Root.transform.rotation * authoredLocalRotation,
                    authoredTransitionRotation),
                Is.LessThan(0.001f),
                "The runtime anchor must capture the authored transition facing before combat movement.");

            fixture.Root.transform.SetPositionAndRotation(
                new Vector3(3.25f, 0.4f, -4.75f),
                Quaternion.Euler(0f, 137f, 0f));
            Physics.SyncTransforms();

            Assert.That(fixture.ApplyPlayerDamage(1000f), Is.True);
            Assert.That(ReadPhase(fixture.Flow), Is.EqualTo("Transitioning"));
            Vector3 expectedPosition = fixture.Root.transform.TransformPoint(authoredLocalOffset);
            Quaternion expectedRotation = fixture.Root.transform.rotation
                * authoredLocalRotation;
            Assert.That(
                Vector3.Distance(fixture.TransitionRoot.transform.position, expectedPosition),
                Is.LessThan(0.0001f),
                "The C33/C34 rig must follow the boss's live combat position at 50%.");
            Assert.That(
                Quaternion.Angle(fixture.TransitionRoot.transform.rotation, expectedRotation),
                Is.LessThan(0.001f),
                "The C33/C34 rig must preserve its authored facing relative to the live boss.");
        }

        [UnityTest]
        [Timeout(10000)]
        public IEnumerator NaturalDirectorCompletionUsesTheSameIdempotentCleanupPath()
        {
            using var fixture = new FlowFixture(TransitionAssetMode.Short);
            yield return null;

            Assert.That(fixture.ApplyPlayerDamage(900f), Is.True);
            Assert.That(fixture.BossHealth.CurrentHealth, Is.EqualTo(500f).Within(0.001f));

            yield return WaitForPhase(fixture.Flow, "Phase2", 2f);
            AssertPhaseTwoCompletedOnce(fixture);

            for (int i = 0; i < 4; i++)
            {
                yield return null;
            }

            Assert.That(
                ReadPublic<int>(fixture.Flow, "TransitionCompletionCount"),
                Is.EqualTo(1),
                "PlayableDirector completion and polling must not double-complete the handoff.");
            Assert.That(fixture.ApplyPlayerDamage(30f), Is.True);
            Assert.That(fixture.BossHealth.CurrentHealth, Is.EqualTo(470f).Within(0.001f));
        }

        [UnityTest]
        [Timeout(10000)]
        public IEnumerator TransitionStartedSubscriberSkipDoesNotRestartTheTimelineAtFrameZero()
        {
            using var fixture = new FlowFixture(TransitionAssetMode.Long);
            yield return null;

            EventInfo startedEvent = fixture.Flow.GetType().GetEvent(
                "TransitionStarted",
                BindingFlags.Instance | BindingFlags.Public);
            Assert.That(startedEvent, Is.Not.Null);
            Action skipOnStart = () => Assert.That(TrySkip(fixture.Flow), Is.True);
            startedEvent.AddEventHandler(fixture.Flow, skipOnStart);

            Assert.That(fixture.ApplyPlayerDamage(1000f), Is.True);
            yield return WaitForPhase(fixture.Flow, "Phase2", 2f);

            AssertPhaseTwoCompletedOnce(fixture);
            Assert.That(fixture.Director.state, Is.Not.EqualTo(PlayState.Playing));
            Assert.That(
                ReadPublic<float>(fixture.Flow, "TransitionElapsedSeconds"),
                Is.GreaterThanOrEqualTo(0.10f),
                "A skip from TransitionStarted may not restart C33 at frame zero.");
        }

        [UnityTest]
        [Timeout(10000)]
        public IEnumerator MissingPlayableAssetFailsOpenWithoutLeavingCombatSuspended()
        {
            using var fixture = new FlowFixture(TransitionAssetMode.MissingAsset);
            yield return null;

            Assert.That(fixture.Director, Is.Not.Null);
            Assert.That(fixture.Director.playableAsset, Is.Null);
            Assert.That(fixture.ApplyPlayerDamage(1000f), Is.True);

            yield return WaitForPhase(fixture.Flow, "Phase2", 1f);
            AssertPhaseTwoCompletedOnce(fixture);
            Assert.That(fixture.Encounter.IsRunning, Is.True);
            Assert.That(ReadPublic<bool>(fixture.Encounter, "IsExternalCombatSuspended"), Is.False);
            Assert.That(
                fixture.Barrage.IsFiringEnabled,
                Is.False,
                "The authored SummonBlock relief must pause barrage fire after handoff.");
            Assert.That(fixture.BasicFire.IsFiringEnabled, Is.False);
            Assert.That(fixture.ActionDirector.ActionsEnabled, Is.True);
            Assert.That(fixture.PositionController.MovementEnabled, Is.True);
            Assert.That(fixture.EnemyPacing.PacingEnabled, Is.True);

            Assert.That(fixture.ApplyPlayerDamage(40f), Is.True);
            Assert.That(fixture.BossHealth.CurrentHealth, Is.EqualTo(460f).Within(0.001f));
        }

        [UnityTest]
        [Timeout(10000)]
        public IEnumerator PersistentPhaseTwoCommitFailureRestoresPlayableCombatWithoutFalseCompletion()
        {
            using var fixture = new FlowFixture(TransitionAssetMode.MissingAsset);
            var failingRoot = new GameObject("Akaza Phase 2 Failing Encounter");
            failingRoot.SetActive(false);
            CombatHealth deadBossHealth = failingRoot.AddComponent<CombatHealth>();
            ConfigureHealth(deadBossHealth, DamageTeam.Enemy, 10f);
            BossBarrageEncounterController failingEncounter =
                failingRoot.AddComponent<BossBarrageEncounterController>();
            failingEncounter.Configure(
                newPlayerHealth: fixture.SourceHealth,
                newCloseThreatHealth: null,
                newBossHealth: deadBossHealth,
                newEnergyLadder: null,
                newSkill1Action: null,
                newSummonSlot1Action: null,
                newBossBarrageEmitter: fixture.Barrage,
                newClearMarker: null,
                newFailMarker: null,
                newBossPressureCostLadder: null,
                newBossPressureActionDirector: fixture.ActionDirector,
                newBossBasicFireEmitter: fixture.BasicFire);
            failingRoot.SetActive(true);
            Assert.That(
                deadBossHealth.TryApplyDamage(new DamageInfo(
                    fixture.SourceHealth,
                    DamageTeam.Player,
                    10f,
                    deadBossHealth.transform.position,
                    Vector3.forward,
                    0f)),
                Is.True);
            Assert.That(deadBossHealth.IsAlive, Is.False);
            SetPrivate(fixture.Flow, "bossBarrageEncounterController", failingEncounter);

            bool previousIgnoreFailingMessages = LogAssert.ignoreFailingMessages;
            LogAssert.ignoreFailingMessages = true;
            try
            {
                Assert.That(fixture.ApplyPlayerDamage(1000f), Is.True);
                float deadline = Time.realtimeSinceStartup + 1f;
                while (!ReadPublic<bool>(fixture.Flow, "TransitionFaultedOpen")
                    && Time.realtimeSinceStartup < deadline)
                {
                    yield return null;
                }

                Assert.That(ReadPublic<bool>(fixture.Flow, "TransitionFaultedOpen"), Is.True);
                Assert.That(ReadPhase(fixture.Flow), Is.EqualTo("Phase2"));
                Assert.That(ReadPublic<bool>(fixture.Flow, "PhaseTwoApplied"), Is.True);
                Assert.That(
                    ReadPublic<int>(fixture.Flow, "TransitionCompletionCount"),
                    Is.Zero,
                    "A partial Phase 2 commit may not publish a successful transition receipt.");
                Assert.That(fixture.Barrage.IsFiringEnabled, Is.True);
                Assert.That(fixture.BasicFire.IsFiringEnabled, Is.True);
                Assert.That(fixture.ActionDirector.ActionsEnabled, Is.True);
                Assert.That(fixture.PositionController.MovementEnabled, Is.True);
                Assert.That(fixture.EnemyPacing.PacingEnabled, Is.True);
                Assert.That(fixture.ApplyPlayerDamage(25f), Is.True);
                Assert.That(fixture.BossHealth.CurrentHealth, Is.EqualTo(475f).Within(0.001f));
            }
            finally
            {
                LogAssert.ignoreFailingMessages = previousIgnoreFailingMessages;
                SetPrivate(fixture.Flow, "bossBarrageEncounterController", fixture.Encounter);
                Object.DestroyImmediate(failingRoot);
            }
        }

        [UnityTest]
        [Timeout(10000)]
        public IEnumerator PhaseTwoHandoffAppliesCanonicalAkazaLoadoutAndSummonBlockOpening()
        {
            using var fixture = new FlowFixture(TransitionAssetMode.MissingAsset);
            yield return null;

            Assert.That(fixture.ApplyPlayerDamage(1000f), Is.True);
            yield return WaitForPhase(fixture.Flow, "Phase2", 1f);

            AssertPhaseTwoCompletedOnce(fixture);
            Assert.That(fixture.PhaseOneVisual.activeSelf, Is.False);
            Assert.That(fixture.PhaseTwoVisual.activeSelf, Is.True);
            Assert.That(fixture.BasicFire.FireProfile, Is.SameAs(fixture.BasicFireProfile));
            Assert.That(fixture.ActionDirector.ActionDeckProfile, Is.SameAs(fixture.ActionDeckProfile));
            Assert.That(fixture.SummonPressure.PressureProfile, Is.SameAs(fixture.SummonPressureProfile));
            Assert.That(fixture.Barrage.PooledProjectilePrefab, Is.SameAs(fixture.PhaseTwoProjectile));
            Assert.That(fixture.BasicFire.PooledProjectilePrefab, Is.SameAs(fixture.PhaseTwoProjectile));
            Assert.That(fixture.BasicFire.FireOrigin, Is.SameAs(fixture.PhaseTwoFireOrigin));
            Assert.That(fixture.Barrage.ConfiguredSpawnOriginCount, Is.EqualTo(6));
            Assert.That(fixture.Barrage.PooledProjectileCount, Is.EqualTo(3));
            Assert.That(fixture.BasicFire.PooledProjectileCount, Is.EqualTo(2));
            Assert.That(fixture.VisualCueDriver.Animator, Is.SameAs(fixture.PhaseTwoAnimator));
            Assert.That(
                fixture.VisualCueDriver.DamageFlashRendererCount,
                Is.EqualTo(1),
                "Switching presentation owners must rebind hit flashes to the Phase 2 model.");
            Assert.That(
                ReadPrivate<Animator>(fixture.PositionController, "movementAnimator"),
                Is.SameAs(fixture.PhaseTwoAnimator));

            Assert.That(fixture.Barrage.HasQueuedPriorityPattern, Is.True);
            Assert.That(
                fixture.Barrage.QueuedPriorityPattern,
                Is.SameAs(fixture.SummonCurtain),
                "AkazaSummonCurtain must be the first Phase 2 gameplay read.");
            Assert.That(
                fixture.Barrage.CurrentPattern,
                Is.SameAs(fixture.SummonCurtain),
                "The deferred priority pattern must remain the active read while firing is paused.");

            Assert.That(fixture.Encounter.CloseThreatDefeated, Is.True);
            Assert.That(
                fixture.Encounter.CurrentPhase,
                Is.EqualTo(BossBarrageEncounterController.EncounterPhase.SummonBlock));
            Assert.That(fixture.Encounter.IsSummonBlockOpportunityCueActive, Is.True);
            fixture.Encounter.Tick(fixture.Encounter.PressureReliefRemainingSeconds + 0.01f);
            Assert.That(fixture.Barrage.IsFiringEnabled, Is.True);
            Assert.That(
                fixture.Barrage.QueuedPriorityPattern,
                Is.SameAs(fixture.SummonCurtain),
                "The deferred opening pattern must survive the relief pause and fire next.");
            Assert.That(fixture.Barrage.CancelQueuedPriorityPattern(fixture.SummonCurtain), Is.True);
            Assert.That(
                fixture.Barrage.CurrentPattern,
                Is.SameAs(fixture.HoverLance),
                "After the opening curtain, the authored Phase 2 sequence must begin at HoverLance.");
        }

        [UnityTest]
        [Timeout(10000)]
        public IEnumerator PhaseTwoRearmsSummonBlockAfterPhaseOneAlreadyConsumedCloseThreat()
        {
            using var fixture = new FlowFixture(TransitionAssetMode.MissingAsset);
            yield return null;

            Assert.That(
                fixture.Encounter.BeginAtSummonBlock(),
                Is.True,
                "The fixture must first consume the Phase 1 close-threat handoff.");
            Assert.That(fixture.Encounter.CloseThreatDefeated, Is.True);

            Assert.That(fixture.ApplyPlayerDamage(1000f), Is.True);
            yield return WaitForPhase(fixture.Flow, "Phase2", 1f);

            AssertPhaseTwoCompletedOnce(fixture);
            Assert.That(fixture.Encounter.IsRunning, Is.True);
            Assert.That(
                fixture.Encounter.CurrentPhase,
                Is.EqualTo(BossBarrageEncounterController.EncounterPhase.SummonBlock));
            Assert.That(fixture.Encounter.IsSummonBlockOpportunityCueActive, Is.True);
            Assert.That(fixture.Barrage.QueuedPriorityPattern, Is.SameAs(fixture.SummonCurtain));
        }

        [UnityTest]
        [Timeout(10000)]
        public IEnumerator PhaseTwoCounterObjectiveKeepsTheLivingBossCombatLoopRunning()
        {
            using var fixture = new FlowFixture(TransitionAssetMode.MissingAsset);
            yield return null;

            Assert.That(fixture.ApplyPlayerDamage(1000f), Is.True);
            yield return WaitForPhase(fixture.Flow, "Phase2", 1f);
            fixture.Encounter.Tick(fixture.Encounter.PressureReliefRemainingSeconds + 0.01f);

            int pocketClearedCount = 0;
            fixture.Encounter.PocketCleared += () => pocketClearedCount++;
            SetPrivate(fixture.Encounter, "usedSummonSlot1", true);
            SetPrivate(fixture.Encounter, "blockedBossPressureWithSummon", true);
            SetPrivate(fixture.Encounter, "skill1FollowupHitConfirmed", true);
            SetPrivate(fixture.Encounter, "skill1FollowupClearTimer", 0f);
            fixture.Encounter.Tick(0.01f);

            Assert.That(fixture.BossHealth.IsAlive, Is.True);
            Assert.That(fixture.Encounter.IsRunning, Is.True);
            Assert.That(fixture.Encounter.IsCleared, Is.False);
            Assert.That(fixture.Encounter.PhaseTwoPocketObjectiveCompleted, Is.True);
            Assert.That(pocketClearedCount, Is.Zero);
            Assert.That(fixture.Barrage.IsFiringEnabled, Is.True);
            Assert.That(fixture.ActionDirector.ActionsEnabled, Is.True);

            fixture.Encounter.Tick(0.25f);
            Assert.That(pocketClearedCount, Is.Zero, "The completed Phase 2 answer is idempotent.");
            Assert.That(fixture.Encounter.IsRunning, Is.True);
        }

        [UnityTest]
        [Timeout(10000)]
        public IEnumerator PhaseTwoBossDeathStopsEveryCombatAndSummonOwner()
        {
            using var fixture = new FlowFixture(TransitionAssetMode.MissingAsset);
            yield return null;

            Assert.That(fixture.ApplyPlayerDamage(1000f), Is.True);
            yield return WaitForPhase(fixture.Flow, "Phase2", 1f);
            fixture.Encounter.Tick(fixture.Encounter.PressureReliefRemainingSeconds + 0.01f);
            Assert.That(fixture.Barrage.IsFiringEnabled, Is.True);
            Assert.That(fixture.ActionDirector.ActionsEnabled, Is.True);
            Assert.That(fixture.EnemyPacing.PacingEnabled, Is.True);

            Assert.That(fixture.ApplyPlayerDamage(1000f), Is.True);
            yield return null;

            fixture.Barrage.Tick(1f);
            fixture.BasicFire.Tick(1f);
            fixture.ActionDirector.Tick(1f);
            fixture.EnemyPacing.Tick(1f);

            Assert.That(fixture.BossHealth.IsAlive, Is.False);
            Assert.That(ReadPublic<bool>(fixture.Flow, "BossTerminalized"), Is.True);
            Assert.That(ReadPublic<bool>(fixture.Encounter, "IsExternalCombatSuspended"), Is.True);
            Assert.That(fixture.Barrage.IsFiringEnabled, Is.False);
            Assert.That(fixture.Barrage.ActiveProjectileCount, Is.Zero);
            Assert.That(fixture.BasicFire.IsFiringEnabled, Is.False);
            Assert.That(fixture.BasicFire.ActiveProjectileCount, Is.Zero);
            Assert.That(fixture.ActionDirector.ActionsEnabled, Is.False);
            Assert.That(fixture.EnemyPacing.PacingEnabled, Is.False);
            Assert.That(fixture.PositionController.MovementEnabled, Is.False);
            Assert.That(fixture.SummonPressure.ActiveSummonActorCount, Is.Zero);
        }

        [UnityTest]
        [Timeout(10000)]
        public IEnumerator PhaseTwoPlayerFailureStopsEnemyPacingAndPressureSummons()
        {
            using var fixture = new FlowFixture(TransitionAssetMode.MissingAsset);
            yield return null;

            Assert.That(fixture.ApplyPlayerDamage(1000f), Is.True);
            yield return WaitForPhase(fixture.Flow, "Phase2", 1f);
            fixture.Encounter.Tick(fixture.Encounter.PressureReliefRemainingSeconds + 0.01f);
            Assert.That(fixture.EnemyPacing.PacingEnabled, Is.True);

            Assert.That(fixture.ApplyBossDamageToPlayer(1000f), Is.True);
            yield return null;
            int activeSummonsAtFailure = fixture.SummonPressure.ActiveSummonActorCount;

            fixture.EnemyPacing.Tick(30f);
            fixture.ActionDirector.Tick(30f);

            Assert.That(fixture.Encounter.IsFailed, Is.True);
            Assert.That(fixture.EnemyPacing.PacingEnabled, Is.False);
            Assert.That(fixture.ActionDirector.ActionsEnabled, Is.False);
            Assert.That(fixture.PositionController.MovementEnabled, Is.False);
            Assert.That(fixture.SummonPressure.ActiveSummonActorCount, Is.Zero);
            Assert.That(activeSummonsAtFailure, Is.Zero);
        }

        [UnityTest]
        [Timeout(10000)]
        public IEnumerator PlayerFailureDuringCinematicAbortsTheTransitionWithoutCompletion()
        {
            using var fixture = new FlowFixture(TransitionAssetMode.Long);
            yield return null;

            Assert.That(fixture.ApplyPlayerDamage(1000f), Is.True);
            Assert.That(ReadPhase(fixture.Flow), Is.EqualTo("Transitioning"));
            Assert.That(ReadPublic<bool>(fixture.Flow, "PlayerDamageLeaseActive"), Is.True);
            Assert.That(fixture.Director.state, Is.EqualTo(PlayState.Playing));

            SetPrivate(fixture.SourceHealth, "currentHealth", 0f);
            SetPrivate(fixture.SourceHealth, "isDead", true);
            fixture.Encounter.Tick(0f);
            yield return null;

            Assert.That(fixture.Encounter.IsFailed, Is.True);
            Assert.That(fixture.Director.state, Is.Not.EqualTo(PlayState.Playing));
            Assert.That(ReadPhase(fixture.Flow), Is.EqualTo("Phase1"));
            Assert.That(ReadPublic<int>(fixture.Flow, "TransitionCompletionCount"), Is.Zero);
            Assert.That(ReadPublic<bool>(fixture.Flow, "PlayerDamageLeaseActive"), Is.False);
            Assert.That(fixture.PhaseOneVisual.activeSelf, Is.True);
            Assert.That(fixture.PhaseTwoVisual.activeSelf, Is.False);
            Assert.That(fixture.Barrage.IsFiringEnabled, Is.False);
            Assert.That(fixture.BasicFire.IsFiringEnabled, Is.False);
            Assert.That(fixture.ActionDirector.ActionsEnabled, Is.False);
            Assert.That(fixture.EnemyPacing.PacingEnabled, Is.False);
            Assert.That(fixture.PositionController.MovementEnabled, Is.False);
            Assert.That(TrySkip(fixture.Flow), Is.False);
        }

        [UnityTest]
        [Timeout(10000)]
        public IEnumerator DisableAfterOpaqueSwapResumesCommittedPhaseTwoExactlyOnceOnEnable()
        {
            using var fixture = new FlowFixture(TransitionAssetMode.MissingAsset);
            SetPrivate(fixture.Flow, "handoffRevealSeconds", 0.50f);
            yield return null;

            Assert.That(fixture.ApplyPlayerDamage(1000f), Is.True);
            float deadline = Time.realtimeSinceStartup + 1f;
            while ((ReadPhase(fixture.Flow) != "Phase2"
                    || ReadPublic<int>(fixture.Flow, "TransitionCompletionCount") != 0)
                && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.That(ReadPhase(fixture.Flow), Is.EqualTo("Phase2"));
            Assert.That(ReadPublic<int>(fixture.Flow, "TransitionCompletionCount"), Is.Zero);
            Assert.That(fixture.Barrage.QueuedPriorityPattern, Is.SameAs(fixture.SummonCurtain));
            Assert.That(
                fixture.ApplyPlayerDamage(25f),
                Is.False,
                "The opaque/reveal presentation lease must retain transition damage protection.");

            fixture.Root.SetActive(false);
            Assert.That(ReadPublic<int>(fixture.Flow, "TransitionCompletionCount"), Is.Zero);
            fixture.Root.SetActive(true);
            yield return null;

            AssertPhaseTwoCompletedOnce(fixture);
            Assert.That(fixture.Barrage.QueuedPriorityPattern, Is.SameAs(fixture.SummonCurtain));
            Assert.That(ReadPublic<bool>(fixture.Encounter, "IsExternalCombatSuspended"), Is.False);
            Assert.That(fixture.PositionController.MovementEnabled, Is.True);
            Assert.That(fixture.EnemyPacing.PacingEnabled, Is.True);
        }

        [UnityTest]
        [Timeout(10000)]
        public IEnumerator DisableBeforeGameplayCommitRestoresPhaseOneAndRetriesOnEnable()
        {
            using var fixture = new FlowFixture(TransitionAssetMode.Long);
            yield return null;

            Assert.That(fixture.ApplyPlayerDamage(1000f), Is.True);
            Assert.That(ReadPhase(fixture.Flow), Is.EqualTo("Transitioning"));
            Assert.That(fixture.PhaseOneVisual.activeSelf, Is.False);

            fixture.Root.SetActive(false);
            Assert.That(ReadPhase(fixture.Flow), Is.EqualTo("Phase1"));
            Assert.That(fixture.PhaseOneVisual.activeSelf, Is.True);
            Assert.That(fixture.PhaseTwoVisual.activeSelf, Is.False);
            Assert.That(ReadPublic<int>(fixture.Flow, "TransitionCompletionCount"), Is.Zero);

            fixture.Root.SetActive(true);
            yield return null;
            Assert.That(ReadPhase(fixture.Flow), Is.EqualTo("Transitioning"));
            Assert.That(fixture.PhaseOneVisual.activeSelf, Is.False);
            Assert.That(ReadPublic<int>(fixture.Flow, "TransitionStartCount"), Is.EqualTo(2));

            Assert.That(TrySkip(fixture.Flow), Is.True);
            yield return WaitForPhase(fixture.Flow, "Phase2", 2f);
            AssertPhaseTwoCompletedOnce(fixture, expectedStartCount: 2);
            Assert.That(fixture.PhaseOneVisual.activeSelf, Is.False);
        }

        private static Component[] FindSceneComponents(Scene scene, Type componentType)
        {
            return scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren(componentType, includeInactive: true))
                .Cast<Component>()
                .ToArray();
        }

        private static T[] FindSceneComponents<T>(Scene scene)
            where T : Component
        {
            return scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<T>(includeInactive: true))
                .ToArray();
        }

        private static bool ApplyAuthoredPlayerDamage(
            CombatHealth bossHealth,
            CombatHealth playerHealth,
            float amount)
        {
            Vector3 direction = bossHealth.transform.position - playerHealth.transform.position;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction = Vector3.forward;
            }

            return bossHealth.TryApplyDamage(new DamageInfo(
                playerHealth,
                DamageTeam.Player,
                amount,
                bossHealth.transform.position,
                direction.normalized,
                0f));
        }

        private static BossBarrageProjectile[] ReadBarrageProjectilePool(
            BossBarrageEmitter barrage)
        {
            IEnumerable pool = ReadPrivate<IEnumerable>(barrage, "pool");
            return pool.Cast<object>()
                .OfType<BossBarrageProjectile>()
                .Where(projectile => projectile != null)
                .ToArray();
        }

        private static void AssertProjectileLaunchedFromAkazaMuzzle(
            BossBarrageProjectile projectile,
            Transform[] muzzles,
            Vector3[] launchMuzzlePositions)
        {
            Assert.That(launchMuzzlePositions, Has.Length.EqualTo(muzzles.Length));
            Assert.That(
                projectile.LastSpawnUsedAuthoredOrigin,
                Is.True,
                $"{projectile.name} fell back to the legacy lane-space boss proxy origin.");
            Assert.That(
                launchMuzzlePositions.Any(position =>
                    Vector3.Distance(position, projectile.LastAuthoredSpawnPosition) < 0.01f),
                Is.True,
                $"{projectile.name} did not record one of the six Akaza muzzle transforms.");
            Vector3 travelDirection = ReadPrivate<Vector3>(projectile, "travelDirection");
            Assert.That(travelDirection.sqrMagnitude, Is.GreaterThan(0.0001f));
            travelDirection.Normalize();
            Vector3 expectedDirection = (
                projectile.LastConfiguredTargetPosition
                - projectile.LastConfiguredSpawnPosition).normalized;
            Assert.That(
                Vector3.Dot(travelDirection, expectedDirection),
                Is.GreaterThan(0.999f),
                "An authored Akaza blade muzzle must preserve the full 3D aim toward its lane "
                + "target; lower muzzles may aim up and elevated muzzles aim down.");

            int matchedMuzzleIndex = -1;
            for (int index = 0; index < muzzles.Length; index++)
            {
                Vector3 displacement = projectile.LastConfiguredSpawnPosition
                    - launchMuzzlePositions[index];
                float forwardDistance = Vector3.Dot(displacement, travelDirection);
                Vector3 lateralOffset = displacement - travelDirection * forwardDistance;
                if (forwardDistance >= -0.05f
                    && forwardDistance <= 1.5f
                    && lateralOffset.sqrMagnitude <= 0.01f)
                {
                    matchedMuzzleIndex = index;
                    break;
                }
            }

            Assert.That(
                matchedMuzzleIndex,
                Is.GreaterThanOrEqualTo(0),
                $"{projectile.name} did not start near any authored Akaza blade muzzle. "
                + $"authored={projectile.LastAuthoredSpawnPosition}, "
                + $"configured={projectile.LastConfiguredSpawnPosition}, "
                + $"current={projectile.transform.position}, direction={travelDirection}; "
                + string.Join(
                    "; ",
                    muzzles.Select((muzzle, index) =>
                    {
                        Vector3 displacement = projectile.LastConfiguredSpawnPosition
                            - launchMuzzlePositions[index];
                        float forward = Vector3.Dot(displacement, travelDirection);
                        float lateral = (displacement - travelDirection * forward).magnitude;
                        return $"{muzzle.name}:forward={forward:F3},lateral={lateral:F3}";
                    })));
            Assert.That(
                muzzles[matchedMuzzleIndex].name,
                Does.StartWith("AkazaPhase2_BarrageMuzzle_"));
            Assert.That(
                Vector3.Distance(
                    projectile.transform.position,
                    projectile.LastConfiguredSpawnPosition),
                Is.LessThan(0.01f),
                $"{projectile.name} moved before the firing frame completed.");
        }

        private static GameObject RequireSceneObject(Scene scene, string objectName)
        {
            Transform[] matches = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Transform>(includeInactive: true))
                .Where(candidate => candidate.name == objectName)
                .ToArray();
            Assert.That(matches, Has.Length.EqualTo(1), $"Expected one scene object named {objectName}.");
            return matches[0].gameObject;
        }

        private static GameObject RequireChild(GameObject root, string childName)
        {
            Transform[] matches = root.GetComponentsInChildren<Transform>(includeInactive: true)
                .Where(candidate => candidate.name == childName)
                .ToArray();
            Assert.That(matches, Has.Length.EqualTo(1), $"Expected one {childName} under {root.name}.");
            return matches[0].gameObject;
        }

        private static PlayableBinding RequireOutput(
            PlayableBinding[] outputs,
            string streamName)
        {
            PlayableBinding[] matches = outputs
                .Where(output => output.streamName == streamName)
                .ToArray();
            Assert.That(matches, Has.Length.EqualTo(1), $"Missing Timeline track {streamName}.");
            Assert.That(matches[0].sourceObject, Is.Not.Null);
            return matches[0];
        }

        private static void AssertAnimationTrack(
            PlayableBinding track,
            params ClipExpectation[] expected)
        {
            MethodInfo getClips = track.sourceObject.GetType().GetMethod(
                "GetClips",
                BindingFlags.Instance | BindingFlags.Public);
            Assert.That(getClips, Is.Not.Null, $"{track.streamName} is not a Timeline clip track.");
            var clipEnumerable = getClips.Invoke(track.sourceObject, Array.Empty<object>()) as IEnumerable;
            Assert.That(clipEnumerable, Is.Not.Null);
            object[] clips = clipEnumerable.Cast<object>()
                .OrderBy(clip => ReadNumericProperty(clip, "start"))
                .ToArray();
            Assert.That(clips, Has.Length.EqualTo(expected.Length));

            for (int i = 0; i < expected.Length; i++)
            {
                ClipExpectation contract = expected[i];
                object clip = clips[i];
                Assert.That(
                    ReadNumericProperty(clip, "start"),
                    Is.EqualTo(contract.Start).Within(0.0001d),
                    $"{track.streamName} clip {i} start drifted.");
                Assert.That(
                    ReadNumericProperty(clip, "clipIn"),
                    Is.EqualTo(contract.ClipIn).Within(0.0001d),
                    $"{track.streamName} clip {i} source crop drifted.");
                Assert.That(
                    ReadNumericProperty(clip, "duration"),
                    Is.EqualTo(contract.Duration).Within(0.0001d),
                    $"{track.streamName} clip {i} duration drifted.");

                Object playable = ReadPublic<Object>(clip, "asset");
                AnimationClip sourceClip = ReadPublic<AnimationClip>(playable, "clip");
                Assert.That(
                    AssetDatabase.GetAssetPath(sourceClip),
                    Is.EqualTo(contract.SourceAssetPath),
                    $"{track.streamName} clip {i} is bound to the wrong source take.");
            }
        }

        private static void AssertAnimatorBinding(
            PlayableDirector director,
            PlayableBinding track,
            GameObject expectedOwner)
        {
            Object binding = director.GetGenericBinding(track.sourceObject);
            Assert.That(
                binding,
                Is.InstanceOf<Animator>(),
                $"{track.streamName} must bind to an Animator.");
            var animator = (Animator)binding;
            Assert.That(
                animator.transform == expectedOwner.transform
                    || animator.transform.IsChildOf(expectedOwner.transform),
                Is.True,
                $"{track.streamName} is bound outside {expectedOwner.name}.");
        }

        private static double ReadNumericProperty(object target, string propertyName)
        {
            object value = ReadPublicProperty(target, propertyName);
            Assert.That(value, Is.Not.Null, $"{target.GetType().Name}.{propertyName} is null.");
            return Convert.ToDouble(value);
        }

        private static void AssertPhaseTwoCompletedOnce(
            FlowFixture fixture,
            int expectedStartCount = 1)
        {
            Assert.That(ReadPhase(fixture.Flow), Is.EqualTo("Phase2"));
            Assert.That(ReadPublic<bool>(fixture.Flow, "PhaseTwoApplied"), Is.True);
            Assert.That(
                ReadPublic<int>(fixture.Flow, "TransitionStartCount"),
                Is.EqualTo(expectedStartCount));
            Assert.That(ReadPublic<int>(fixture.Flow, "TransitionCompletionCount"), Is.EqualTo(1));
            Assert.That(ReadPublic<CombatHealth>(fixture.Flow, "BossHealth"), Is.SameAs(fixture.BossHealth));
            Assert.That(fixture.BossHealth.MaxHealth, Is.EqualTo(1000f).Within(0.001f));
            Assert.That(fixture.Director.state, Is.Not.EqualTo(PlayState.Playing));
            Assert.That(fixture.EnemyPacing.PacingEnabled, Is.True);
        }

        private static IEnumerator WaitForPhase(Component flow, string expectedPhase, float timeoutSeconds)
        {
            float deadline = Time.realtimeSinceStartup + Mathf.Max(0.1f, timeoutSeconds);
            while ((ReadPhase(flow) != expectedPhase
                    || (expectedPhase == "Phase2"
                        && ReadPublic<int>(flow, "TransitionCompletionCount") == 0))
                && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.That(
                ReadPhase(flow),
                Is.EqualTo(expectedPhase),
                $"Timed out waiting for {FlowTypeName} phase {expectedPhase}.");
            if (expectedPhase == "Phase2")
            {
                Assert.That(
                    ReadPublic<int>(flow, "TransitionCompletionCount"),
                    Is.EqualTo(1),
                    $"Timed out waiting for {FlowTypeName} phase-two reveal cleanup.");
            }
        }

        private static string ReadPhase(Component flow)
        {
            object value = ReadPublicProperty(flow, "CurrentPhase");
            return value != null ? value.ToString() : string.Empty;
        }

        private static bool TrySkip(Component flow)
        {
            MethodInfo method = flow.GetType().GetMethod(
                "TrySkipTransition",
                BindingFlags.Instance | BindingFlags.Public);
            Assert.That(method, Is.Not.Null, $"{FlowTypeName} must expose bool TrySkipTransition().");
            Assert.That(method.ReturnType, Is.EqualTo(typeof(bool)));
            return (bool)method.Invoke(flow, Array.Empty<object>());
        }

        private static T ReadPublic<T>(object target, string propertyName)
        {
            object value = ReadPublicProperty(target, propertyName);
            Assert.That(value, Is.InstanceOf<T>(), $"{target.GetType().Name}.{propertyName} has the wrong type.");
            return (T)value;
        }

        private static object ReadPublicProperty(object target, string propertyName)
        {
            Assert.That(target, Is.Not.Null);
            PropertyInfo property = target.GetType().GetProperty(
                propertyName,
                BindingFlags.Instance | BindingFlags.Public);
            Assert.That(
                property,
                Is.Not.Null,
                $"{target.GetType().FullName} must expose public {propertyName}.");
            return property.GetValue(target);
        }

        private static T ReadPrivate<T>(object target, string fieldName)
        {
            FieldInfo field = FindField(target.GetType(), fieldName);
            Assert.That(field, Is.Not.Null, $"Missing field {target.GetType().FullName}.{fieldName}.");
            return (T)field.GetValue(target);
        }

        private static void SetPrivate(object target, string fieldName, object value)
        {
            FieldInfo field = FindField(target.GetType(), fieldName);
            Assert.That(field, Is.Not.Null, $"Missing field {target.GetType().FullName}.{fieldName}.");
            field.SetValue(target, value);
        }

        private static void SetPrivateAlias(object target, object value, params string[] fieldNames)
        {
            for (int i = 0; i < fieldNames.Length; i++)
            {
                FieldInfo field = FindField(target.GetType(), fieldNames[i]);
                if (field == null)
                {
                    continue;
                }

                field.SetValue(target, value);
                return;
            }

            Assert.Fail(
                $"Missing required field on {target.GetType().FullName}. "
                + $"Expected one of: {string.Join(", ", fieldNames)}.");
        }

        private static FieldInfo FindField(Type type, string fieldName)
        {
            for (Type current = type; current != null; current = current.BaseType)
            {
                FieldInfo field = current.GetField(
                    fieldName,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (field != null)
                {
                    return field;
                }
            }

            return null;
        }

        private static Type RequireType(string fullName)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(fullName, throwOnError: false);
                if (type != null)
                {
                    return type;
                }
            }

            Assert.Fail($"Required product type was not found: {fullName}");
            return null;
        }

        private static T RequireAsset<T>(string path)
            where T : Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            Assert.That(asset, Is.Not.Null, $"Required Phase 2 test asset is missing: {path}");
            return asset;
        }

        private static void AssertProjectileMaterial(
            Material material,
            string expectedTexturePath,
            bool additive)
        {
            Assert.That(material.shader, Is.Not.Null);
            Assert.That(material.shader.isSupported, Is.True, $"Unsupported shader on {material.name}.");
            Assert.That(
                material.shader.name,
                Does.Contain("Particles/Unlit"),
                $"{material.name} must preserve ParticleSystem vertex color and lifetime fade.");
            Assert.That(material.renderQueue, Is.EqualTo(3000));
            Assert.That(material.HasProperty("_Surface"), Is.True);
            Assert.That(material.GetFloat("_Surface"), Is.EqualTo(1f).Within(0.001f));
            Assert.That(material.HasProperty("_ZWrite"), Is.True);
            Assert.That(material.GetFloat("_ZWrite"), Is.EqualTo(0f).Within(0.001f));
            Assert.That(material.HasProperty("_DstBlend"), Is.True);
            Assert.That(
                material.GetFloat("_DstBlend"),
                Is.EqualTo(additive ? 1f : 10f).Within(0.001f));
            Assert.That(material.HasProperty("_BaseMap"), Is.True);
            Assert.That(
                AssetDatabase.GetAssetPath(material.GetTexture("_BaseMap")),
                Is.EqualTo(expectedTexturePath));
        }

        private static void AssertNeutralGradient(Gradient gradient, string ownerName)
        {
            Assert.That(gradient, Is.Not.Null);
            foreach (GradientColorKey key in gradient.colorKeys)
            {
                Assert.That(key.color.r, Is.EqualTo(1f).Within(0.001f), ownerName);
                Assert.That(key.color.g, Is.EqualTo(1f).Within(0.001f), ownerName);
                Assert.That(key.color.b, Is.EqualTo(1f).Within(0.001f), ownerName);
            }
        }

        private static void ConfigureHealth(CombatHealth health, DamageTeam team, float maxHealth)
        {
            var serialized = new SerializedObject(health);
            serialized.FindProperty("team").enumValueIndex = (int)team;
            serialized.FindProperty("maxHealth").floatValue = maxHealth;
            serialized.FindProperty("startAtFullHealth").boolValue = true;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private enum TransitionAssetMode
        {
            Long,
            Short,
            MissingAsset
        }

        private readonly struct ClipExpectation
        {
            public ClipExpectation(double start, double clipIn, double duration, string sourceAssetPath)
            {
                Start = start;
                ClipIn = clipIn;
                Duration = duration;
                SourceAssetPath = sourceAssetPath;
            }

            public double Start { get; }
            public double ClipIn { get; }
            public double Duration { get; }
            public string SourceAssetPath { get; }
        }

        private sealed class FlowFixture : IDisposable
        {
            private readonly GameObject sourceRoot;
            private readonly GameObject projectilePrefabRoot;
            private readonly DurationPlayableAsset playableAsset;

            public FlowFixture(TransitionAssetMode transitionAssetMode)
            {
                Root = new GameObject("Akaza Phase 2 Flow Test Root");
                Root.SetActive(false);

                BossHealth = Root.AddComponent<CombatHealth>();
                ConfigureHealth(BossHealth, DamageTeam.Enemy, 1000f);

                Barrage = Root.AddComponent<BossBarrageEmitter>();
                BasicFire = Root.AddComponent<BossBasicFireEmitter>();
                SummonPressure = Root.AddComponent<BossSummonPressureAction>();
                EnemyPacing = Root.AddComponent<EnemySummonPacingDirector>();
                EnemyPacing.ConfigureReferences(SummonPressure);
                ActionDirector = Root.AddComponent<BossPressureActionDirector>();
                PositionController = Root.AddComponent<BossPressurePositionController>();
                VisualCueDriver = Root.AddComponent<BossBarrageVisualCueDriver>();
                Encounter = Root.AddComponent<BossBarrageEncounterController>();

                PhaseOneVisual = new GameObject("Phase 1 Visual");
                PhaseOneVisual.transform.SetParent(Root.transform, false);
                PhaseTwoVisual = new GameObject("Phase 2 Akaza Visual");
                PhaseTwoVisual.transform.SetParent(Root.transform, false);
                PhaseTwoVisual.SetActive(false);
                PhaseTwoAnimator = PhaseTwoVisual.AddComponent<Animator>();
                GameObject phaseTwoBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
                phaseTwoBody.name = "Phase 2 Damage Flash Probe";
                phaseTwoBody.transform.SetParent(PhaseTwoVisual.transform, false);
                Object.DestroyImmediate(phaseTwoBody.GetComponent<Collider>());
                PhaseTwoFireOrigin = new GameObject("Phase 2 Basic Fire Origin").transform;
                PhaseTwoFireOrigin.SetParent(PhaseTwoVisual.transform, false);
                PhaseTwoBarrageSpawnOrigins = Enumerable.Range(0, 6)
                    .Select(index =>
                    {
                        Transform origin = new GameObject($"Phase 2 Barrage Origin {index}").transform;
                        origin.SetParent(PhaseTwoVisual.transform, false);
                        return origin;
                    })
                    .ToArray();

                TransitionRoot = new GameObject("Akaza Phase 2 Transition Rig");
                TransitionRoot.transform.SetPositionAndRotation(
                    new Vector3(0.65f, 0.2f, -0.35f),
                    Quaternion.Euler(0f, 18f, 0f));
                Director = TransitionRoot.AddComponent<PlayableDirector>();
                Director.playOnAwake = false;
                Director.extrapolationMode = DirectorWrapMode.None;
                if (transitionAssetMode != TransitionAssetMode.MissingAsset)
                {
                    playableAsset = ScriptableObject.CreateInstance<DurationPlayableAsset>();
                    playableAsset.DurationSeconds = transitionAssetMode == TransitionAssetMode.Short
                        ? 0.08d
                        : 30d;
                    Director.playableAsset = playableAsset;
                }

                HoverLance = RequireAsset<BossBarragePatternProfile>(HoverLancePath);
                SummonCurtain = RequireAsset<BossBarragePatternProfile>(SummonCurtainPath);
                SpiralVolley = RequireAsset<BossBarragePatternProfile>(SpiralVolleyPath);
                CrushNet = RequireAsset<BossBarragePatternProfile>(CrushNetPath);
                BasicFireProfile = RequireAsset<BossBasicFireProfile>(BasicFirePath);
                ActionDeckProfile = RequireAsset<BossPressureActionDeckProfile>(ActionDeckPath);
                SummonPressureProfile = RequireAsset<BossSummonPressureProfile>(SummonPressurePath);

                projectilePrefabRoot = new GameObject("Akaza Phase 2 Projectile Prefab Probes");
                projectilePrefabRoot.SetActive(false);
                GameObject phaseOneProjectileObject = new GameObject("Phase One Projectile Source");
                phaseOneProjectileObject.transform.SetParent(projectilePrefabRoot.transform, false);
                PhaseOneProjectile = phaseOneProjectileObject.AddComponent<BossBarrageProjectile>();
                GameObject phaseTwoProjectileObject = new GameObject("Phase Two Akaza Projectile Source");
                phaseTwoProjectileObject.transform.SetParent(projectilePrefabRoot.transform, false);
                PhaseTwoProjectile = phaseTwoProjectileObject.AddComponent<BossBarrageProjectile>();
                Barrage.ConfigurePattern(HoverLance, PhaseOneProjectile, 2);
                BasicFire.ConfigureProfile(BasicFireProfile, PhaseOneProjectile, 1);

                PhaseTwoPatternSequence = new[]
                {
                    HoverLance,
                    SummonCurtain,
                    SpiralVolley,
                    HoverLance,
                    SummonCurtain,
                    CrushNet
                };

                Type flowType = RequireType(FlowTypeName);
                Flow = Root.AddComponent(flowType);
                SetPrivate(Flow, "bossHealth", BossHealth);
                SetPrivate(Flow, "transitionDirector", Director);
                SetPrivate(Flow, "transitionRoot", TransitionRoot);
                SetPrivate(Flow, "phaseOneVisualRoot", PhaseOneVisual);
                SetPrivate(Flow, "phaseTwoVisualRoot", PhaseTwoVisual);
                SetPrivate(Flow, "bossBarrageEmitter", Barrage);
                SetPrivate(Flow, "bossBasicFireEmitter", BasicFire);
                SetPrivate(Flow, "bossPressureActionDirector", ActionDirector);
                SetPrivate(Flow, "bossSummonPressureAction", SummonPressure);
                SetPrivate(Flow, "enemySummonPacingDirector", EnemyPacing);
                SetPrivate(Flow, "bossPressurePositionController", PositionController);
                SetPrivate(Flow, "bossVisualCueDriver", VisualCueDriver);
                SetPrivateAlias(
                    Flow,
                    Encounter,
                    "bossBarrageEncounterController",
                    "encounterController");
                SetPrivate(Flow, "phaseTwoAnimator", PhaseTwoAnimator);
                SetPrivate(Flow, "phaseTwoPatternSequence", PhaseTwoPatternSequence);
                SetPrivate(Flow, "phaseTwoOpeningPattern", SummonCurtain);
                SetPrivate(Flow, "phaseTwoBasicFireProfile", BasicFireProfile);
                SetPrivate(Flow, "phaseTwoActionDeckProfile", ActionDeckProfile);
                SetPrivate(Flow, "phaseTwoSummonPressureProfile", SummonPressureProfile);
                SetPrivate(Flow, "phaseTwoProjectilePrefab", PhaseTwoProjectile);
                SetPrivate(Flow, "phaseTwoBasicFireOrigin", PhaseTwoFireOrigin);
                SetPrivate(Flow, "phaseTwoBarrageSpawnOrigins", PhaseTwoBarrageSpawnOrigins);
                SetPrivate(Flow, "phaseTwoBarragePrewarmCount", 3);
                SetPrivate(Flow, "phaseTwoBasicPrewarmCount", 2);
                SetPrivate(Flow, "transitionDurationSeconds", 0.10f);
                SetPrivate(Flow, "transitionTimeoutSeconds", 0.75f);
                SetPrivate(Flow, "handoffCoverSeconds", 0.01f);
                SetPrivate(Flow, "handoffRevealSeconds", 0.01f);
                SetPrivate(Flow, "phaseThreshold01", 0.5f);

                Encounter.Configure(
                    newPlayerHealth: null,
                    newCloseThreatHealth: null,
                    newBossHealth: BossHealth,
                    newEnergyLadder: null,
                    newSkill1Action: null,
                    newSummonSlot1Action: null,
                    newBossBarrageEmitter: Barrage,
                    newClearMarker: null,
                    newFailMarker: null,
                    newBossPressureCostLadder: null,
                    newBossPressureActionDirector: ActionDirector,
                    newBossBasicFireEmitter: BasicFire);

                sourceRoot = new GameObject("Akaza Phase 2 Flow Test Damage Source");
                sourceRoot.SetActive(false);
                SourceHealth = sourceRoot.AddComponent<CombatHealth>();
                ConfigureHealth(SourceHealth, DamageTeam.Player, 100f);
                SetPrivate(Flow, "playerHealth", SourceHealth);

                Encounter.Configure(
                    newPlayerHealth: SourceHealth,
                    newCloseThreatHealth: null,
                    newBossHealth: BossHealth,
                    newEnergyLadder: null,
                    newSkill1Action: null,
                    newSummonSlot1Action: null,
                    newBossBarrageEmitter: Barrage,
                    newClearMarker: null,
                    newFailMarker: null,
                    newBossPressureCostLadder: null,
                    newBossPressureActionDirector: ActionDirector,
                    newBossBasicFireEmitter: BasicFire);

                sourceRoot.SetActive(true);
                Root.SetActive(true);
            }

            public GameObject Root { get; }
            public Component Flow { get; }
            public CombatHealth BossHealth { get; }
            public CombatHealth SourceHealth { get; }
            public BossBarrageEmitter Barrage { get; }
            public BossBasicFireEmitter BasicFire { get; }
            public BossSummonPressureAction SummonPressure { get; }
            public EnemySummonPacingDirector EnemyPacing { get; }
            public BossPressureActionDirector ActionDirector { get; }
            public BossPressurePositionController PositionController { get; }
            public BossBarrageVisualCueDriver VisualCueDriver { get; }
            public BossBarrageEncounterController Encounter { get; }
            public GameObject PhaseOneVisual { get; }
            public GameObject PhaseTwoVisual { get; }
            public Animator PhaseTwoAnimator { get; }
            public Transform PhaseTwoFireOrigin { get; }
            public Transform[] PhaseTwoBarrageSpawnOrigins { get; }
            public PlayableDirector Director { get; }
            public GameObject TransitionRoot { get; }
            public BossBarragePatternProfile HoverLance { get; }
            public BossBarragePatternProfile SummonCurtain { get; }
            public BossBarragePatternProfile SpiralVolley { get; }
            public BossBarragePatternProfile CrushNet { get; }
            public BossBasicFireProfile BasicFireProfile { get; }
            public BossPressureActionDeckProfile ActionDeckProfile { get; }
            public BossSummonPressureProfile SummonPressureProfile { get; }
            public BossBarrageProjectile PhaseOneProjectile { get; }
            public BossBarrageProjectile PhaseTwoProjectile { get; }
            public BossBarragePatternProfile[] PhaseTwoPatternSequence { get; }

            public bool ApplyPlayerDamage(float amount)
            {
                return BossHealth.TryApplyDamage(new DamageInfo(
                    SourceHealth,
                    DamageTeam.Player,
                    amount,
                    BossHealth.transform.position,
                    Vector3.forward,
                    0f));
            }

            public bool ApplyBossDamageToPlayer(float amount)
            {
                return SourceHealth.TryApplyDamage(new DamageInfo(
                    BossHealth,
                    DamageTeam.Enemy,
                    amount,
                    SourceHealth.transform.position,
                    Vector3.back,
                    0f));
            }

            public void Dispose()
            {
                if (Root != null)
                {
                    Object.DestroyImmediate(Root);
                }

                if (sourceRoot != null)
                {
                    Object.DestroyImmediate(sourceRoot);
                }

                if (TransitionRoot != null)
                {
                    Object.DestroyImmediate(TransitionRoot);
                }

                if (projectilePrefabRoot != null)
                {
                    Object.DestroyImmediate(projectilePrefabRoot);
                }

                if (playableAsset != null)
                {
                    Object.DestroyImmediate(playableAsset);
                }
            }
        }

        private sealed class DurationPlayableAsset : PlayableAsset
        {
            public double DurationSeconds { get; set; } = 0.1d;
            public override double duration => DurationSeconds;

            public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
            {
                Playable playable = Playable.Create(graph);
                playable.SetDuration(duration);
                return playable;
            }
        }
    }
}
