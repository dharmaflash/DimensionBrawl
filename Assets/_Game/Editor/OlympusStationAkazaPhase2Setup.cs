using System;
using System.Collections.Generic;
using System.Linq;
using DimensionBrawl.Combat;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Player;
using DimensionBrawl.Presentation;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;
using UnityEngine.UI;

namespace DimensionBrawl.Editor
{
    /// <summary>
    /// Authors the product Station phase-two handoff without importing the legacy
    /// scene, shaders, background, audio, or custom Timeline tracks. Only the
    /// verified C33/C34 actor and camera grammar is retained.
    /// </summary>
    public static class OlympusStationAkazaPhase2Setup
    {
        public const string StationScenePath =
            "Assets/_Game/Scenes/OlympusStationCombatStage.unity";
        public const string TimelinePath =
            "Assets/_Game/DesignData/Profiles/Cinematics/DB_Timeline_OlympusStationAkazaPhase2Intro.playable";

        private const string AkazaPrefabPath =
            "Assets/_Game/Prefabs/Enemies/ActionFoundation/PF_Boss_Akaza_Phase2Review.prefab";
        private const string AkazaProjectilePrefabPath =
            "Assets/_Game/Prefabs/Combat/PF_BossBarrageProjectile_AkazaPhase2.prefab";
        private const string C33ActorPath =
            "Assets/_Game/Art/Characters/Bosses/Akaza/Animations/Source/C33_Akaza.fbx";
        private const string C34ActorPath =
            "Assets/_Game/Art/Characters/Bosses/Akaza/Animations/Source/C34_Akaza.fbx";
        private const string C33CameraPath =
            "Assets/_Game/Art/Animations/Cinematics/LegacyCameraGrammar/C33_Cam.fbx";
        private const string C34CameraPath =
            "Assets/_Game/Art/Animations/Cinematics/LegacyCameraGrammar/C34_Cam.fbx";
        private const string C34InPlacePath =
            "Assets/_Game/Art/Characters/Bosses/Akaza/Animations/Sanitized/DB_Akaza_C34_InPlace.anim";
        private const string C27InPlacePath =
            "Assets/_Game/Art/Characters/Bosses/Akaza/Animations/Sanitized/DB_Akaza_C27_InPlace.anim";
        private const string PhaseTwoHoldPath =
            "Assets/_Game/Art/Characters/Bosses/Akaza/Animations/Sanitized/DB_Akaza_Phase2_DeployedEyeOpenHold.anim";
        private const string PhaseTwoControllerPath =
            "Assets/_Game/Art/Characters/Bosses/Akaza/Animations/DB_Akaza_Phase2Boss.controller";
        private const string CombatCueClockPath =
            "Assets/_Game/Art/Characters/Bosses/Akaza/Animations/Sanitized/DB_Akaza_CombatCueClock.anim";
        private const string AkazaIntroMaterialFolder =
            "Assets/_Game/Art/Characters/Bosses/Akaza/IntroSource/Materials";
        private const string PhaseTwoLookMaterialFolder =
            "Assets/_Game/Art/Characters/Bosses/Akaza/Generated/Materials";
        private const string PhaseTwoLookProfilePath =
            "Assets/_Game/DesignData/Profiles/Cinematics/DB_Volume_OlympusStationAkazaPhase2Intro.asset";

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

        internal const string CrushNetCameraPatternId = "AkazaCrushNet";
        internal const float CrushNetCameraSustainSeconds = 3.2f;
        internal const float CrushNetCameraReleaseSeconds = 0.18f;
        internal const float CrushNetCameraFieldOfViewDelta = -11.8f;
        internal const float CrushNetCameraDistanceDelta = -0.9f;
        internal const float StationMaxCueFieldOfViewDelta = 12f;
        internal const float StationMaxCueCameraDistanceDelta = 0.95f;

        private const string BossRootName = "BossBarrageLaneReview_BossProxy_NeedleLock";
        private const string PhaseOneVisualName =
            "BossBarrageLaneReview_HumanoidBossVisual_SciFiSoldier_01_Commando";
        private const string PhaseTwoVisualName = "OlympusStation_AkazaPhase2GameplayVisual";
        private const string PhaseTwoPulseAnchorName = "AkazaPhase2_WingPulseAnchor";
        private const string TransitionRootName = "OlympusStation_AkazaPhase2TransitionRig";
        private const string CombatHudRootName = "BossBarrageLaneReview_CombatHudCanvas";
        private const int CinematicActorLayer = 9;

        private const double WingClipInSeconds = 6.10d;
        private const double WingDurationSeconds = 1.60d;
        private const double EyeDurationSeconds = 2.3666667d;
        private const double MasterDurationSeconds = WingDurationSeconds + EyeDurationSeconds;

        [MenuItem("DimensionBrawl/Stage/Olympus/Apply Station Akaza Phase 2")]
        public static void ApplyMenu()
        {
            Apply(openScene: true);
            Debug.Log($"Station Akaza phase two authored in {StationScenePath}.");
        }

        public static void RunBatchSetup()
        {
            try
            {
                Apply(openScene: false);
                Debug.Log("Station Akaza phase-two setup passed.");
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        public static void Apply(bool openScene)
        {
            EnsureFolderForAsset(TimelinePath);
            ImportRequiredSource(C33ActorPath);
            ImportRequiredSource(C34ActorPath);
            ImportRequiredSource(C33CameraPath);
            ImportRequiredSource(C34CameraPath);
            ConfigureAkazaProjectileForMobileCombat();
            Dictionary<Material, Material> phaseTwoLookMaterials =
                EnsurePhaseTwoSourceLookMaterials();
            VolumeProfile phaseTwoLookProfile = EnsurePhaseTwoSourceLookProfile();

            AnimationClip phaseTwoHoldClip = EnsurePhaseTwoHoldClip();
            AnimationClip phaseTwoHeavyReleaseClip = RequireAsset<AnimationClip>(C27InPlacePath);
            ConfigurePhaseTwoAnimatorController(phaseTwoHoldClip, phaseTwoHeavyReleaseClip);

            AnimationClip c33ActorClip = RequirePrimaryClip(C33ActorPath, 7.70f);
            AnimationClip c34ActorClip = RequirePrimaryClip(C34ActorPath, (float)EyeDurationSeconds);
            AnimationClip c33CameraClip = RequirePrimaryClip(C33CameraPath, 7.70f);
            AnimationClip c34CameraClip = RequirePrimaryClip(C34CameraPath, (float)EyeDurationSeconds);
            TimelineBindings timeline = EnsureTimeline(
                c33ActorClip,
                c34ActorClip,
                c33CameraClip,
                c34CameraClip);

            Scene scene = EditorSceneManager.OpenScene(StationScenePath, OpenSceneMode.Single);
            GameObject bossRoot = RequireSceneObject(scene, BossRootName);
            CombatHealth bossHealth = RequireComponent<CombatHealth>(bossRoot, "Station boss health");
            BossBarrageEncounterController encounter = RequireSingle<BossBarrageEncounterController>(scene);

            DestroyNamedSceneObject(scene, TransitionRootName);
            DestroyNamedChild(bossRoot.transform, PhaseTwoVisualName);

            GameObject phaseOneVisual = RequireChildRecursive(bossRoot.transform, PhaseOneVisualName).gameObject;
            GameObject phaseTwoVisual = CreateGameplayVisual(
                bossRoot.transform,
                phaseTwoLookMaterials);
            Animator phaseTwoAnimator = RequireComponentInChildren<Animator>(phaseTwoVisual, "Akaza gameplay Animator");
            TransitionRig transitionRig = CreateTransitionRig(
                scene,
                timeline,
                bossRoot.transform,
                phaseTwoLookMaterials,
                phaseTwoLookProfile);
            AlignGameplayVisualToCinematicTerminal(phaseTwoVisual, transitionRig);
            GameplayLookStateSetup.ConfigureLoadedScene(scene);

            OlympusStationAkazaPhase2FlowController flow =
                bossRoot.GetComponent<OlympusStationAkazaPhase2FlowController>()
                ?? bossRoot.AddComponent<OlympusStationAkazaPhase2FlowController>();
            ConfigureFlow(
                scene,
                flow,
                bossHealth,
                encounter,
                phaseOneVisual,
                phaseTwoVisual,
                phaseTwoAnimator,
                transitionRig);
            ConfigurePhaseTwoCombatMotion(
                phaseTwoVisual,
                phaseTwoAnimator,
                bossHealth,
                RequireComponent<BossBarrageEmitter>(bossRoot, "Station barrage emitter"),
                RequireComponent<BossBasicFireEmitter>(bossRoot, "Station basic-fire emitter"));

            ValidateProductWiring(
                scene,
                flow,
                bossHealth,
                phaseOneVisual,
                phaseTwoVisual,
                transitionRig,
                timeline);

            transitionRig.Root.SetActive(false);
            phaseOneVisual.SetActive(true);
            phaseTwoVisual.SetActive(false);
            EditorUtility.SetDirty(flow);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, StationScenePath))
            {
                throw new InvalidOperationException($"Could not save {StationScenePath}.");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            if (!openScene)
            {
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            }
        }

        private static Dictionary<Material, Material> EnsurePhaseTwoSourceLookMaterials()
        {
            var remap = new Dictionary<Material, Material>();
            AddPhaseTwoSourceLookMaterial(
                remap,
                "Arm",
                new Color(0.8f, 0.8f, 0.8f, 1f),
                Color.white,
                Color.white,
                baseStep: 0.506f,
                firstShadeStep: 0.506f,
                firstShadeFeather: 0.0001f,
                secondShadeStep: 0f,
                systemShadowLevel: 0f,
                lightSecondShade: true,
                outlineWidth: 7f,
                nearestDistance: 36.13f,
                farthestDistance: -2.4f,
                outlineDepthOffset: -0.01f,
                clearShadingGradeMap: true);
            AddPhaseTwoSourceLookMaterial(
                remap,
                "Body",
                new Color(0.637f, 0.637f, 0.637f, 1f),
                Color.white,
                Color.white,
                baseStep: 0.6f,
                firstShadeStep: 0.5f,
                firstShadeFeather: 0.0001f,
                secondShadeStep: 0f,
                systemShadowLevel: 0f,
                lightSecondShade: true,
                outlineWidth: 3.3f,
                nearestDistance: 0.5f,
                farthestDistance: 10f,
                outlineDepthOffset: 0f,
                clearShadingGradeMap: true);
            AddPhaseTwoSourceLookMaterial(
                remap,
                "Face",
                Color.white,
                Color.white,
                Color.white,
                baseStep: 0.091f,
                firstShadeStep: 0.091f,
                firstShadeFeather: 0.005f,
                secondShadeStep: 0f,
                systemShadowLevel: -0.5f,
                lightSecondShade: false,
                outlineWidth: 5.2f,
                nearestDistance: 22.76f,
                farthestDistance: -8.7f,
                outlineDepthOffset: 0f,
                clearShadingGradeMap: true);
            AddPhaseTwoSourceLookMaterial(
                remap,
                "Eyes",
                Color.white,
                Color.white,
                new Color(0.6714965f, 0.6943085f, 0.8455882f, 1f),
                baseStep: 0.373f,
                firstShadeStep: 0.373f,
                firstShadeFeather: 0.0001f,
                secondShadeStep: 0.017f,
                systemShadowLevel: -0.41f,
                lightSecondShade: true,
                outlineWidth: 5.2f,
                nearestDistance: 22.76f,
                farthestDistance: -6.61f,
                outlineDepthOffset: 0f,
                clearShadingGradeMap: false);
            AddPhaseTwoSourceLookMaterial(
                remap,
                "HairSpow",
                Color.white,
                Color.white,
                Color.white,
                baseStep: 0.502f,
                firstShadeStep: 0.502f,
                firstShadeFeather: 0.034f,
                secondShadeStep: 0f,
                systemShadowLevel: 0f,
                lightSecondShade: true,
                outlineWidth: 7f,
                nearestDistance: 10.47f,
                farthestDistance: -2.4f,
                outlineDepthOffset: 0f,
                clearShadingGradeMap: false);
            AddPhaseTwoSourceLookMaterial(
                remap,
                "Skin",
                new Color(0.637f, 0.637f, 0.637f, 1f),
                Color.white,
                Color.black,
                baseStep: 0.5f,
                firstShadeStep: 0.5f,
                firstShadeFeather: 0.0001f,
                secondShadeStep: 0f,
                systemShadowLevel: 0f,
                lightSecondShade: true,
                outlineWidth: 6.29f,
                nearestDistance: 13.47f,
                farthestDistance: -4.02f,
                outlineDepthOffset: 0f,
                clearShadingGradeMap: true);
            return remap;
        }

        private static void AddPhaseTwoSourceLookMaterial(
            IDictionary<Material, Material> remap,
            string materialSuffix,
            Color baseTint,
            Color firstShade,
            Color secondShade,
            float baseStep,
            float firstShadeStep,
            float firstShadeFeather,
            float secondShadeStep,
            float systemShadowLevel,
            bool lightSecondShade,
            float outlineWidth,
            float nearestDistance,
            float farthestDistance,
            float outlineDepthOffset,
            bool clearShadingGradeMap)
        {
            string sourcePath =
                $"{AkazaIntroMaterialFolder}/M_C08_Akaza_{materialSuffix}.mat";
            string targetPath =
                $"{PhaseTwoLookMaterialFolder}/M_Akaza_Phase2_{materialSuffix}_SourceSoft.mat";
            EnsureFolderForAsset(targetPath);
            Material source = RequireAsset<Material>(sourcePath);
            Material target = AssetDatabase.LoadAssetAtPath<Material>(targetPath);
            if (target == null)
            {
                target = new Material(source)
                {
                    name = $"M_Akaza_Phase2_{materialSuffix}_SourceSoft"
                };
                AssetDatabase.CreateAsset(target, targetPath);
            }
            else
            {
                target.CopyPropertiesFromMaterial(source);
            }

            SetColorIfPresent(target, "_Color", baseTint);
            SetColorIfPresent(target, "_BaseColor", baseTint);
            SetColorIfPresent(target, "_1st_ShadeColor", firstShade);
            SetColorIfPresent(target, "_2nd_ShadeColor", secondShade);
            SetColorIfPresent(target, "_Outline_Color", Color.black);
            SetFloatIfPresent(target, "_BaseColor_Step", baseStep);
            SetFloatIfPresent(target, "_BaseShade_Feather", firstShadeFeather);
            SetFloatIfPresent(target, "_1st_ShadeColor_Step", firstShadeStep);
            SetFloatIfPresent(target, "_1st_ShadeColor_Feather", firstShadeFeather);
            SetFloatIfPresent(target, "_1st2nd_Shades_Feather", firstShadeFeather);
            SetFloatIfPresent(target, "_2nd_ShadeColor_Step", secondShadeStep);
            SetFloatIfPresent(target, "_2nd_ShadeColor_Feather", 0.0001f);
            SetFloatIfPresent(target, "_Tweak_SystemShadowsLevel", systemShadowLevel);
            SetFloatIfPresent(target, "_Glossiness", 0.5f);
            SetFloatIfPresent(target, "_Smoothness", 0.5f);
            SetFloatIfPresent(target, "_GI_Intensity", 0f);
            SetFloatIfPresent(target, "_Set_SystemShadowsToBase", 1f);
            SetFloatIfPresent(target, "_Is_Filter_LightColor", 1f);
            SetFloatIfPresent(target, "_Is_LightColor_Base", 1f);
            SetFloatIfPresent(target, "_Is_LightColor_1st_Shade", 1f);
            SetFloatIfPresent(target, "_Is_LightColor_2nd_Shade", lightSecondShade ? 1f : 0f);
            SetFloatIfPresent(target, "_Use_BaseAs1st", 0f);
            SetFloatIfPresent(target, "_Use_1stAs2nd", 0f);
            SetFloatIfPresent(target, "_RimLight", 0f);
            SetFloatIfPresent(target, "_MatCap", 0f);
            SetFloatIfPresent(target, "_AngelRing", 0f);
            SetFloatIfPresent(target, "_OUTLINE", 0f);
            SetFloatIfPresent(target, "_Outline_Width", outlineWidth);
            SetFloatIfPresent(target, "_Nearest_Distance", nearestDistance);
            SetFloatIfPresent(target, "_Farthest_Distance", farthestDistance);
            SetFloatIfPresent(target, "_Offset_Z", outlineDepthOffset);
            SetFloatIfPresent(target, "_utsTechnique", 1f);
            if (clearShadingGradeMap && target.HasProperty("_ShadingGradeMap"))
            {
                target.SetTexture("_ShadingGradeMap", null);
            }

            SetKeyword(target, "_SHADINGGRADEMAP", true);
            SetKeyword(target, "_OUTLINE_NML", true);
            SetKeyword(target, "_SET_SYSTEMSHADOWSTOBASE_ON", true);
            SetKeyword(target, "_IS_FILTER_LIGHTCOLOR_ON", true);
            SetKeyword(target, "_IS_LIGHTCOLOR_BASE_ON", true);
            SetKeyword(target, "_IS_LIGHTCOLOR_1ST_SHADE_ON", true);
            SetKeyword(target, "_IS_LIGHTCOLOR_2ND_SHADE_ON", lightSecondShade);
            target.SetShaderPassEnabled("SRPDefaultUnlit", true);
            EditorUtility.SetDirty(target);
            remap.Add(source, target);
        }

        private static VolumeProfile EnsurePhaseTwoSourceLookProfile()
        {
            EnsureFolderForAsset(PhaseTwoLookProfilePath);
            VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(
                PhaseTwoLookProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                profile.name = "DB_Volume_OlympusStationAkazaPhase2Intro";
                AssetDatabase.CreateAsset(profile, PhaseTwoLookProfilePath);
            }

            Tonemapping tonemapping = EnsureVolumeOverride<Tonemapping>(profile);
            tonemapping.mode.Override(TonemappingMode.Neutral);

            WhiteBalance whiteBalance = EnsureVolumeOverride<WhiteBalance>(profile);
            // Preserve the soft source presentation without carrying the legacy
            // orange grade into the blue-white Olympus product stage.
            whiteBalance.temperature.Override(0f);
            whiteBalance.tint.Override(0f);

            ColorAdjustments color = EnsureVolumeOverride<ColorAdjustments>(profile);
            color.postExposure.Override(0.2f);
            color.contrast.Override(0f);
            color.colorFilter.Override(Color.white);
            color.hueShift.Override(0f);
            color.saturation.Override(-10.7f);

            LiftGammaGain wheels = EnsureVolumeOverride<LiftGammaGain>(profile);
            wheels.lift.Override(new Vector4(1f, 1f, 1f, 0f));
            wheels.gamma.Override(new Vector4(1f, 1f, 1f, 0f));
            wheels.gain.Override(new Vector4(1f, 1f, 1f, 0f));

            Bloom bloom = EnsureVolumeOverride<Bloom>(profile);
            bloom.threshold.Override(1f);
            bloom.intensity.Override(0.7f);
            bloom.scatter.Override(0.85f);
            bloom.downscale.Override(BloomDownscaleMode.Half);
            bloom.maxIterations.Override(8);
            bloom.highQualityFiltering.Override(true);

            ChromaticAberration chromatic = EnsureVolumeOverride<ChromaticAberration>(profile);
            // PPv1 scales the authored intensity by .03; URP uses .05.
            chromatic.intensity.Override(0.15f);

            Vignette vignette = EnsureVolumeOverride<Vignette>(profile);
            vignette.color.Override(Color.black);
            vignette.center.Override(new Vector2(0.5f, 0.5f));
            // PPv1 classic vignette uses a different power curve. This is the
            // closest URP edge/corner attenuation fit to source intensity .385.
            vignette.intensity.Override(0.1834666f);
            vignette.smoothness.Override(0.63f);
            vignette.rounded.Override(false);

            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static T EnsureVolumeOverride<T>(VolumeProfile profile)
            where T : VolumeComponent
        {
            if (profile.TryGet(out T component))
            {
                component.active = true;
                return component;
            }

            component = profile.Add<T>(overrides: true);
            component.name = typeof(T).Name;
            if (!AssetDatabase.Contains(component))
            {
                AssetDatabase.AddObjectToAsset(component, profile);
            }

            component.active = true;
            EditorUtility.SetDirty(component);
            return component;
        }

        private static void ApplyPhaseTwoLookMaterials(
            GameObject root,
            IReadOnlyDictionary<Material, Material> remap)
        {
            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(includeInactive: true))
            {
                Material[] materials = renderer.sharedMaterials;
                bool changed = false;
                for (int index = 0; index < materials.Length; index++)
                {
                    Material source = materials[index];
                    if (source != null && remap.TryGetValue(source, out Material target))
                    {
                        materials[index] = target;
                        changed = true;
                    }
                }

                if (changed)
                {
                    renderer.sharedMaterials = materials;
                    EditorUtility.SetDirty(renderer);
                }
            }
        }

        private static void SetFloatIfPresent(Material material, string propertyName, float value)
        {
            if (material.HasProperty(propertyName))
            {
                material.SetFloat(propertyName, value);
            }
        }

        private static void SetColorIfPresent(Material material, string propertyName, Color value)
        {
            if (material.HasProperty(propertyName))
            {
                material.SetColor(propertyName, value);
            }
        }

        private static void SetKeyword(Material material, string keyword, bool enabled)
        {
            if (enabled)
            {
                material.EnableKeyword(keyword);
            }
            else
            {
                material.DisableKeyword(keyword);
            }
        }

        private static void ConfigureAkazaProjectileForMobileCombat()
        {
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(AkazaProjectilePrefabPath);
            if (prefabRoot == null)
            {
                throw new InvalidOperationException(
                    $"Could not load Akaza phase-two projectile prefab at {AkazaProjectilePrefabPath}.");
            }

            try
            {
                Light[] projectileLights = prefabRoot.GetComponentsInChildren<Light>(true);
                foreach (Light projectileLight in projectileLights)
                {
                    projectileLight.enabled = false;
                    EditorUtility.SetDirty(projectileLight);
                }

                PrefabUtility.SaveAsPrefabAsset(prefabRoot, AkazaProjectilePrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        private static TimelineBindings EnsureTimeline(
            AnimationClip c33Actor,
            AnimationClip c34Actor,
            AnimationClip c33Camera,
            AnimationClip c34Camera)
        {
            TimelineAsset asset = AssetDatabase.LoadAssetAtPath<TimelineAsset>(TimelinePath);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<TimelineAsset>();
                AssetDatabase.CreateAsset(asset, TimelinePath);
            }

            foreach (TrackAsset track in asset.GetRootTracks().ToArray())
            {
                asset.DeleteTrack(track);
            }

            asset.name = "DB_Timeline_OlympusStationAkazaPhase2Intro";
            asset.durationMode = TimelineAsset.DurationMode.FixedLength;
            asset.fixedDuration = MasterDurationSeconds;
            asset.editorSettings.frameRate = 60f;

            AnimationTrack actorTrack = asset.CreateTrack<AnimationTrack>(
                "Akaza Actor - C33 Deploy then C34 Eye Open");
            ConfigureTrack(actorTrack);
            CreateSourceClip(
                actorTrack,
                c33Actor,
                start: 0d,
                clipIn: WingClipInSeconds,
                duration: WingDurationSeconds,
                "C33 Wing Deploy 6.10-7.70");
            CreateSourceClip(
                actorTrack,
                c34Actor,
                start: WingDurationSeconds,
                clipIn: 0d,
                duration: EyeDurationSeconds,
                "C34 Eye Open Exact");

            AnimationTrack wingCameraTrack = asset.CreateTrack<AnimationTrack>(
                "C33 Wing Deploy Camera");
            ConfigureTrack(wingCameraTrack);
            CreateSourceClip(
                wingCameraTrack,
                c33Camera,
                start: 0d,
                clipIn: WingClipInSeconds,
                duration: WingDurationSeconds,
                "C33 Camera 6.10-7.70");

            AnimationTrack eyeCameraTrack = asset.CreateTrack<AnimationTrack>(
                "C34 Eye Open Camera");
            ConfigureTrack(eyeCameraTrack);
            CreateSourceClip(
                eyeCameraTrack,
                c34Camera,
                start: WingDurationSeconds,
                clipIn: 0d,
                duration: EyeDurationSeconds,
                "C34 Camera Exact");

            EditorUtility.SetDirty(actorTrack);
            EditorUtility.SetDirty(wingCameraTrack);
            EditorUtility.SetDirty(eyeCameraTrack);
            EditorUtility.SetDirty(asset);
            return new TimelineBindings(asset, actorTrack, wingCameraTrack, eyeCameraTrack);
        }

        private static void ConfigureTrack(AnimationTrack track)
        {
            track.trackOffset = TrackOffset.Auto;
            SerializedObject serializedTrack = new SerializedObject(track);
            SerializedProperty applyOffsets = serializedTrack.FindProperty("m_ApplyOffsets");
            if (applyOffsets != null)
            {
                applyOffsets.boolValue = false;
                serializedTrack.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static AnimationClip EnsurePhaseTwoHoldClip()
        {
            AnimationClip source = RequireAsset<AnimationClip>(C34InPlacePath);
            EnsureFolderForAsset(PhaseTwoHoldPath);
            AnimationClip hold = AssetDatabase.LoadAssetAtPath<AnimationClip>(PhaseTwoHoldPath);
            if (hold == null)
            {
                hold = new AnimationClip();
                AssetDatabase.CreateAsset(hold, PhaseTwoHoldPath);
            }

            foreach (EditorCurveBinding binding in AnimationUtility.GetCurveBindings(hold))
            {
                AnimationUtility.SetEditorCurve(hold, binding, null);
            }

            foreach (EditorCurveBinding binding in AnimationUtility.GetObjectReferenceCurveBindings(hold))
            {
                AnimationUtility.SetObjectReferenceCurve(hold, binding, null);
            }

            float terminalTime = Mathf.Max(0f, source.length);
            foreach (EditorCurveBinding binding in AnimationUtility.GetCurveBindings(source))
            {
                if (IsShotSpaceRootPositionBinding(binding))
                {
                    throw new InvalidOperationException(
                        $"Phase-two hold source unexpectedly contains shot-space root motion: "
                        + $"{binding.path}/{binding.propertyName}.");
                }

                AnimationCurve sourceCurve = AnimationUtility.GetEditorCurve(source, binding);
                if (sourceCurve == null)
                {
                    continue;
                }

                float value = sourceCurve.Evaluate(terminalTime);
                AnimationUtility.SetEditorCurve(
                    hold,
                    binding,
                    AnimationCurve.Constant(0f, 1f, value));
            }

            foreach (EditorCurveBinding binding in AnimationUtility.GetObjectReferenceCurveBindings(source))
            {
                ObjectReferenceKeyframe[] sourceKeys = AnimationUtility.GetObjectReferenceCurve(source, binding);
                if (sourceKeys == null || sourceKeys.Length == 0)
                {
                    continue;
                }

                UnityEngine.Object terminalValue = sourceKeys[sourceKeys.Length - 1].value;
                AnimationUtility.SetObjectReferenceCurve(
                    hold,
                    binding,
                    new[]
                    {
                        new ObjectReferenceKeyframe { time = 0f, value = terminalValue },
                        new ObjectReferenceKeyframe { time = 1f, value = terminalValue }
                    });
            }

            hold.name = "DB_Akaza_Phase2_DeployedEyeOpenHold";
            hold.frameRate = 30f;
            hold.wrapMode = WrapMode.ClampForever;
            hold.EnsureQuaternionContinuity();
            EditorUtility.SetDirty(hold);

            EditorCurveBinding[] holdBindings = AnimationUtility.GetCurveBindings(hold);
            for (char suffix = 'A'; suffix <= 'F'; suffix++)
            {
                string bladeName = $"akWpBlade{suffix}_jnt";
                if (!holdBindings.Any(binding =>
                        binding.path.IndexOf(bladeName, StringComparison.Ordinal) >= 0))
                {
                    throw new InvalidOperationException(
                        $"Phase-two hold clip lost floating blade binding {bladeName}.");
                }
            }

            return hold;
        }

        private static bool IsShotSpaceRootPositionBinding(EditorCurveBinding binding)
        {
            if (binding.propertyName.IndexOf("LocalPosition", StringComparison.OrdinalIgnoreCase) < 0)
            {
                return false;
            }

            string leaf = binding.path.Split('/').LastOrDefault() ?? string.Empty;
            return leaf == "CHakazaA:Reference"
                || leaf == "CHakazaA:hip_C"
                || leaf == "CHakazaA:world_trs"
                || leaf == "CHakazaA:hip_jnt_C";
        }

        private static void ConfigurePhaseTwoAnimatorController(
            AnimationClip hold,
            AnimationClip heavyRelease)
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(
                PhaseTwoControllerPath);
            AnimationClip cueClock = RequireAsset<AnimationClip>(CombatCueClockPath);
            if (controller == null)
            {
                throw new InvalidOperationException(
                    $"Required Phase 2 AnimatorController missing: {PhaseTwoControllerPath}");
            }

            var states = new List<AnimatorState>();
            foreach (AnimatorControllerLayer layer in controller.layers)
            {
                CollectAnimatorStates(layer.stateMachine, states);
            }

            int assignedCount = 0;
            foreach (AnimatorState state in states)
            {
                if (state.motion != null
                    && state.motion != cueClock
                    && state.motion != hold
                    && state.motion != heavyRelease)
                {
                    continue;
                }

                state.motion = UsesHeavyReleaseMotion(state.name) ? heavyRelease : hold;
                assignedCount++;
                EditorUtility.SetDirty(state);
            }

            AnimatorState hover = states.FirstOrDefault(state => state.name == "Hover");
            AnimatorState linePressure = states.FirstOrDefault(state => state.name == "LinePressure");
            AnimatorState fanPressure = states.FirstOrDefault(state => state.name == "FanPressure");
            AnimatorState heavyCrush = states.FirstOrDefault(state => state.name == "HeavyCrush");
            AnimatorState[] deathStates = states
                .Where(state => state.name == "Death")
                .ToArray();
            if (hover == null
                || hover.motion != hold
                || linePressure == null
                || linePressure.motion != heavyRelease
                || fanPressure == null
                || fanPressure.motion != heavyRelease
                || heavyCrush == null
                || heavyCrush.motion != heavyRelease
                || deathStates.Length != 1
                || deathStates[0].transitions.Length != 0
                || assignedCount == 0)
            {
                throw new InvalidOperationException(
                    "Phase-two controller must preserve the C34 hold in Hover and use the "
                    + "authored C27 six-wing release for barrage attack states, with one "
                    + "terminal Death state that has no outgoing transitions.");
            }

            EditorUtility.SetDirty(controller);
        }

        private static bool UsesHeavyReleaseMotion(string stateName)
        {
            return stateName == "LinePressure"
                || stateName == "FanPressure"
                || stateName == "HeavyCrush"
                || stateName == "BasicAttack"
                || stateName == "Windup"
                || stateName == "RetreatShot";
        }

        private static void CollectAnimatorStates(
            AnimatorStateMachine stateMachine,
            List<AnimatorState> states)
        {
            if (stateMachine == null)
            {
                return;
            }

            foreach (ChildAnimatorState child in stateMachine.states)
            {
                if (child.state != null)
                {
                    states.Add(child.state);
                }
            }

            foreach (ChildAnimatorStateMachine child in stateMachine.stateMachines)
            {
                CollectAnimatorStates(child.stateMachine, states);
            }
        }

        private static TimelineClip CreateSourceClip(
            AnimationTrack track,
            AnimationClip clip,
            double start,
            double clipIn,
            double duration,
            string displayName)
        {
            TimelineClip timelineClip = track.CreateClip(clip);
            timelineClip.displayName = displayName;
            timelineClip.start = start;
            timelineClip.clipIn = clipIn;
            timelineClip.duration = duration;
            timelineClip.timeScale = 1d;
            timelineClip.easeInDuration = 0d;
            timelineClip.easeOutDuration = 0d;
            SetClipExtrapolation(timelineClip, TimelineClip.ClipExtrapolation.None);

            if (timelineClip.asset is not AnimationPlayableAsset playable)
            {
                throw new InvalidOperationException($"{displayName} did not create AnimationPlayableAsset.");
            }

            playable.clip = clip;
            playable.position = Vector3.zero;
            playable.rotation = Quaternion.identity;
            playable.removeStartOffset = true;
            playable.applyFootIK = false;
            playable.loop = AnimationPlayableAsset.LoopMode.Off;
            playable.useTrackMatchFields = false;
            EditorUtility.SetDirty(playable);
            return timelineClip;
        }

        private static void SetClipExtrapolation(
            TimelineClip clip,
            TimelineClip.ClipExtrapolation extrapolation)
        {
            const System.Reflection.BindingFlags Flags =
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
            typeof(TimelineClip).GetField("m_PreExtrapolationMode", Flags)?.SetValue(clip, extrapolation);
            typeof(TimelineClip).GetField("m_PostExtrapolationMode", Flags)?.SetValue(clip, extrapolation);
        }

        private static TransitionRig CreateTransitionRig(
            Scene scene,
            TimelineBindings timeline,
            Transform bossRoot,
            IReadOnlyDictionary<Material, Material> phaseTwoLookMaterials,
            VolumeProfile phaseTwoLookProfile)
        {
            GameObject root = new GameObject(TransitionRootName);
            SceneManager.MoveGameObjectToScene(root, scene);

            GameObject actor = InstantiatePrefab(AkazaPrefabPath, scene, root.transform);
            actor.name = "AkazaPhase2_CinematicActor";
            PrefabUtility.UnpackPrefabInstance(
                actor,
                PrefabUnpackMode.Completely,
                InteractionMode.AutomatedAction);
            actor.transform.localPosition = Vector3.zero;
            actor.transform.localRotation = Quaternion.identity;
            actor.transform.localScale = Vector3.one;
            ApplyPhaseTwoLookMaterials(actor, phaseTwoLookMaterials);
            SetLayerRecursively(actor.transform, CinematicActorLayer);
            StripCinematicActorRuntime(actor);
            Animator actorAnimator = EnsureControllerFreeAnimator(actor, "C33/C34 actor");

            GameObject wingCameraRig = InstantiateAsset(C33CameraPath, scene, root.transform);
            wingCameraRig.name = "C33_WingDeployCameraRig";
            Animator wingCameraAnimator = EnsureControllerFreeAnimator(wingCameraRig, "C33 camera");
            Camera wingCamera = ConfigureCinematicCamera(wingCameraRig, "C33 camera");

            GameObject eyeCameraRig = InstantiateAsset(C34CameraPath, scene, root.transform);
            eyeCameraRig.name = "C34_EyeOpenCameraRig";
            Animator eyeCameraAnimator = EnsureControllerFreeAnimator(eyeCameraRig, "C34 camera");
            Camera eyeCamera = ConfigureCinematicCamera(eyeCameraRig, "C34 camera");

            CanvasGroup curtain = CreateCurtain(root.transform);
            CinematicLights cinematicLights = CreateSourceNeutralLights(root.transform);
            CreateSourceSoftPostVolume(root.transform, phaseTwoLookProfile);

            GameObject directorObject = new GameObject("AkazaPhase2_MasterPlayableDirector");
            directorObject.transform.SetParent(root.transform, false);
            PlayableDirector director = directorObject.AddComponent<PlayableDirector>();
            director.playableAsset = timeline.Asset;
            director.playOnAwake = false;
            director.extrapolationMode = DirectorWrapMode.None;
            director.timeUpdateMode = DirectorUpdateMode.UnscaledGameTime;
            director.SetGenericBinding(timeline.ActorTrack, actorAnimator);
            director.SetGenericBinding(timeline.WingCameraTrack, wingCameraAnimator);
            director.SetGenericBinding(timeline.EyeCameraTrack, eyeCameraAnimator);
            director.RebuildGraph();
            director.time = 0d;
            director.Evaluate();
            Physics.SyncTransforms();

            Light[] competingDirectionalLights = FindSceneComponents<Light>(scene)
                .Where(light =>
                    light != null
                    && light.type == LightType.Directional
                    && !light.transform.IsChildOf(root.transform))
                .ToArray();
            AkazaPhase2CinematicLookDriver lookDriver =
                root.AddComponent<AkazaPhase2CinematicLookDriver>();
            lookDriver.Configure(
                director,
                cinematicLights.WingKey,
                cinematicLights.EyeKey,
                cinematicLights.BackgroundKey,
                competingDirectionalLights,
                WingDurationSeconds,
                sourceApplyCinematicFog: false,
                sourceFogColor: new Color(0.2941f, 0.2197f, 0.1081f, 0.604f),
                sourceFogMode: FogMode.Linear,
                sourceFogDensity: 0.13f,
                sourceFogStartDistance: -30.1f,
                sourceFogEndDistance: 600f);

            AlignTransitionRigToBoss(root.transform, actor, bossRoot);
            director.Evaluate();
            Physics.SyncTransforms();
            // Configure previews the shot while this authoring root is active.
            // Explicitly release that lease before the scene can be serialized;
            // edit-mode SetActive callbacks are not a reliable restoration boundary.
            lookDriver.EndManualLightingLease();
            return new TransitionRig(
                root,
                actor,
                actorAnimator,
                wingCamera,
                eyeCamera,
                curtain,
                director);
        }

        private static GameObject CreateGameplayVisual(
            Transform bossRoot,
            IReadOnlyDictionary<Material, Material> phaseTwoLookMaterials)
        {
            GameObject visual = InstantiatePrefab(AkazaPrefabPath, bossRoot.gameObject.scene, bossRoot);
            visual.name = PhaseTwoVisualName;
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = Vector3.one;
            visual.SetActive(true);
            ApplyPhaseTwoLookMaterials(visual, phaseTwoLookMaterials);

            foreach (Collider collider in visual.GetComponentsInChildren<Collider>(includeInactive: true))
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }

            Animator gameplayAnimator = RequireComponentInChildren<Animator>(
                visual,
                "Akaza deployed-pose Animator");
            if (gameplayAnimator.runtimeAnimatorController == null)
            {
                throw new InvalidOperationException(
                    "Akaza gameplay visual lost its Phase 2 AnimatorController.");
            }

            gameplayAnimator.Rebind();
            gameplayAnimator.Play("Hover", 0, 0f);
            gameplayAnimator.Update(0f);
            Physics.SyncTransforms();

            FitVisual(visual, targetHeight: 3.32f, desiredWorldBottom: 0.52f);
            AkazaPhase2GameplayMeshCombiner.CombineGameplayInstance(visual);
            ConfigureGameplayRendererBudget(visual);
            ActionFoundationArenaTransformMotion hover =
                visual.GetComponent<ActionFoundationArenaTransformMotion>()
                ?? visual.AddComponent<ActionFoundationArenaTransformMotion>();
            hover.Configure(
                Vector3.zero,
                Vector3.up,
                0.28f,
                0.42f,
                0.15f,
                lockLocalRotation: true,
                lockLocalScale: true);
            EditorUtility.SetDirty(visual);
            return visual;
        }

        private static void ConfigurePhaseTwoCombatMotion(
            GameObject phaseTwoVisual,
            Animator phaseTwoAnimator,
            CombatHealth bossHealth,
            BossBarrageEmitter barrageEmitter,
            BossBasicFireEmitter basicFireEmitter)
        {
            Transform[] wingRoots = Enumerable.Range(0, 6)
                .Select(index => FindChildRecursive(
                    phaseTwoVisual.transform,
                    $"CHakazaA:akArmRoot{(char)('A' + index)}_jnt"))
                .ToArray();
            if (wingRoots.Any(root => root == null))
            {
                throw new InvalidOperationException(
                    "Akaza Phase 2 combat motion requires all six floating-arm roots.");
            }

            AkazaPhase2CombatMotionDriver motionDriver =
                phaseTwoVisual.GetComponent<AkazaPhase2CombatMotionDriver>()
                ?? phaseTwoVisual.AddComponent<AkazaPhase2CombatMotionDriver>();
            motionDriver.Configure(
                phaseTwoAnimator,
                bossHealth,
                phaseTwoAnimator.transform,
                wingRoots,
                barrageEmitter,
                basicFireEmitter);
            var serializedMotion = new SerializedObject(motionDriver);
            serializedMotion.FindProperty("deathSettleSeconds").floatValue =
                AkazaPhase2CombatMotionDriver.RequiredDeathSettleSeconds;
            serializedMotion.FindProperty("deathDropDistance").floatValue =
                AkazaPhase2CombatMotionDriver.RequiredDeathDropDistance;
            serializedMotion.FindProperty("deathBackDistance").floatValue =
                AkazaPhase2CombatMotionDriver.RequiredDeathBackDistance;
            serializedMotion.FindProperty("deathPitchDegrees").floatValue =
                AkazaPhase2CombatMotionDriver.RequiredDeathPitchDegrees;
            serializedMotion.FindProperty("deathRollDegrees").floatValue =
                AkazaPhase2CombatMotionDriver.RequiredDeathRollDegrees;
            serializedMotion.FindProperty("deathPivotLocalHeight").floatValue =
                AkazaPhase2CombatMotionDriver.RequiredDeathPivotLocalHeight;
            serializedMotion.FindProperty("deathWingFoldDegrees").floatValue =
                AkazaPhase2CombatMotionDriver.RequiredDeathWingFoldDegrees;
            serializedMotion.FindProperty("deathWingYawDegrees").floatValue =
                AkazaPhase2CombatMotionDriver.RequiredDeathWingYawDegrees;
            serializedMotion.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(motionDriver);
        }

        private static void ConfigureGameplayRendererBudget(GameObject visual)
        {
            Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(includeInactive: true);
            foreach (Renderer renderer in renderers)
            {
                if (renderer == null)
                {
                    continue;
                }

                renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
                if (renderer is SkinnedMeshRenderer skinned)
                {
                    skinned.updateWhenOffscreen = false;
                }

                if (IsPhaseTwoStructureRenderer(renderer.name))
                {
                    renderer.shadowCastingMode = ShadowCastingMode.Off;
                }
            }
        }

        private static void ConfigureFlow(
            Scene scene,
            OlympusStationAkazaPhase2FlowController flow,
            CombatHealth bossHealth,
            BossBarrageEncounterController encounter,
            GameObject phaseOneVisual,
            GameObject phaseTwoVisual,
            Animator phaseTwoAnimator,
            TransitionRig transitionRig)
        {
            GameObject bossRoot = flow.gameObject;
            BossBarrageEmitter barrage = RequireComponent<BossBarrageEmitter>(bossRoot, "boss barrage");
            BossBasicFireEmitter basic = RequireComponent<BossBasicFireEmitter>(bossRoot, "boss basic fire");
            BossPressureActionDirector actions = RequireComponent<BossPressureActionDirector>(bossRoot, "boss actions");
            BossSummonPressureAction summon = RequireComponent<BossSummonPressureAction>(bossRoot, "boss summon pressure");
            EnemySummonPacingDirector summonPacing = RequireComponent<EnemySummonPacingDirector>(
                bossRoot,
                "enemy summon pacing");
            BossPressurePositionController position = RequireComponent<BossPressurePositionController>(bossRoot, "boss movement");
            BossBarrageVisualCueDriver cues = RequireComponent<BossBarrageVisualCueDriver>(bossRoot, "boss visual cues");

            BossBarragePatternProfile hover = RequireAsset<BossBarragePatternProfile>(HoverLancePath);
            BossBarragePatternProfile curtain = RequireAsset<BossBarragePatternProfile>(SummonCurtainPath);
            BossBarragePatternProfile spiral = RequireAsset<BossBarragePatternProfile>(SpiralVolleyPath);
            BossBarragePatternProfile crush = RequireAsset<BossBarragePatternProfile>(CrushNetPath);
            BossBarragePatternProfile[] sequence =
            {
                hover,
                curtain,
                spiral,
                hover,
                curtain,
                crush
            };

            Transform phaseTwoStructureRoot = FindChildRecursive(
                phaseTwoVisual.transform,
                "CHakazaA:weaponRoot_jnt");
            Transform phaseTwoBasicFireOrigin = FindChildRecursive(
                phaseTwoVisual.transform,
                "CHakazaA:spineC_C");
            Transform[] phaseTwoBarrageSpawnOrigins = EnsurePhaseTwoBarrageSpawnOrigins(phaseTwoVisual);
            Renderer[] pulseRenderers = ResolvePhaseTwoStructureRenderers(phaseTwoVisual);
            if (phaseTwoStructureRoot == null || phaseTwoBasicFireOrigin == null)
            {
                throw new InvalidOperationException(
                    "Akaza Phase 2 requires its floating-structure root and active torso fire origin.");
            }

            Transform phaseTwoPulseAnchor = EnsurePhaseTwoPulseAnchor(phaseTwoStructureRoot);

            if (pulseRenderers.Length == 0)
            {
                throw new InvalidOperationException(
                    "Akaza Phase 2 did not resolve any floating-structure renderers for pattern cues.");
            }
            CanvasGroup hudGroup = FindHudCanvasGroup(scene);
            ActionCameraController cameraController = RequireSingle<ActionCameraController>(scene);
            Camera gameplayCamera = cameraController.GetComponent<Camera>()
                ?? RequireSceneObject(scene, "Main Camera").GetComponent<Camera>();
            PlayerMovementController playerMovement = RequireSingle<PlayerMovementController>(scene);
            BossBarrageCameraCueDriver bossCameraCueDriver =
                RequireSingle<BossBarrageCameraCueDriver>(scene);
            CombatHealth playerHealth = playerMovement.GetComponent<CombatHealth>()
                ?? playerMovement.GetComponentInParent<CombatHealth>();
            if (playerHealth == null)
            {
                throw new InvalidOperationException(
                    "Akaza Phase 2 transition could not resolve the canonical player health owner.");
            }

            ConfigureBossBarrageCameraComposition(
                bossCameraCueDriver,
                barrage,
                actions,
                cameraController,
                playerMovement.transform,
                crush);

            PlayerSupportSummonSlotAction[] supportSummons = FindSceneComponents<PlayerSupportSummonSlotAction>(scene);

            SerializedObject serialized = new SerializedObject(flow);
            SetObject(serialized, "bossHealth", bossHealth);
            SetFloat(serialized, "phaseThreshold01", 0.5f);
            SetObject(serialized, "bossBarrageEncounterController", encounter);
            SetObject(serialized, "bossBarrageEmitter", barrage);
            SetObject(serialized, "bossBasicFireEmitter", basic);
            SetObject(serialized, "bossPressureActionDirector", actions);
            SetObject(serialized, "bossSummonPressureAction", summon);
            SetObject(serialized, "enemySummonPacingDirector", summonPacing);
            SetObject(serialized, "bossPressurePositionController", position);
            SetObject(serialized, "bossVisualCueDriver", cues);
            SetObjectArray(serialized, "phaseTwoPatternSequence", sequence);
            SetObject(serialized, "phaseTwoOpeningPattern", curtain);
            SetObject(serialized, "phaseTwoBasicFireProfile", RequireAsset<BossBasicFireProfile>(BasicFirePath));
            SetObject(serialized, "phaseTwoActionDeckProfile", RequireAsset<BossPressureActionDeckProfile>(ActionDeckPath));
            SetObject(serialized, "phaseTwoSummonPressureProfile", RequireAsset<BossSummonPressureProfile>(SummonPressurePath));
            SetObject(
                serialized,
                "phaseTwoProjectilePrefab",
                RequireAsset<GameObject>(AkazaProjectilePrefabPath).GetComponent<BossBarrageProjectile>());
            SetObject(serialized, "phaseTwoBasicFireOrigin", phaseTwoBasicFireOrigin);
            SetObjectArray(serialized, "phaseTwoBarrageSpawnOrigins", phaseTwoBarrageSpawnOrigins);
            SetInt(serialized, "phaseTwoWavesPerPattern", 1);
            SetInt(serialized, "phaseTwoBarragePrewarmCount", 16);
            SetInt(serialized, "phaseTwoBasicPrewarmCount", 12);
            SetObject(serialized, "phaseOneVisualRoot", phaseOneVisual);
            SetObject(serialized, "phaseTwoVisualRoot", phaseTwoVisual);
            SetObject(serialized, "phaseTwoAnimator", phaseTwoAnimator);
            SetObject(serialized, "phaseTwoPulseRoot", phaseTwoPulseAnchor);
            SetObjectArray(serialized, "phaseTwoPulseRenderers", pulseRenderers);
            ConfigurePhaseTwoCueArrays(serialized);

            SetObject(serialized, "transitionRoot", transitionRig.Root);
            SetObject(serialized, "transitionDirector", transitionRig.Director);
            SetObject(serialized, "eyeOpenCamera", transitionRig.EyeCamera);
            SetObject(serialized, "wingDeployCamera", transitionRig.WingCamera);
            SetObject(serialized, "transitionCurtain", transitionRig.Curtain);
            SetBool(serialized, "wingDeployFirst", true);
            SetFloat(serialized, "cinematicCameraSwitchSeconds", (float)WingDurationSeconds);
            SetFloat(serialized, "curtainFadeInStartSeconds", 1.45f);
            SetFloat(serialized, "curtainFadeOutEndSeconds", 1.78f);
            SetFloat(serialized, "transitionDurationSeconds", (float)MasterDurationSeconds);
            SetFloat(serialized, "transitionTimeoutSeconds", (float)MasterDurationSeconds + 1.25f);
            SetFloat(serialized, "handoffCoverSeconds", 0.10f);
            SetFloat(serialized, "handoffRevealSeconds", 0.18f);
            SetBool(serialized, "allowEscapeSkip", true);

            SetObject(serialized, "gameplayCameraController", cameraController);
            SetObject(serialized, "gameplayCamera", gameplayCamera);
            SetObject(serialized, "combatHudCanvasGroup", hudGroup);
            SetObject(serialized, "playerHealth", playerHealth);
            SetObject(serialized, "playerMovement", playerMovement);
            SetObject(serialized, "playerActionController", RequireSingle<PlayerActionController>(scene));
            SetObject(serialized, "playerSkill1Action", RequireSingle<PlayerSkill1Action>(scene));
            SetObject(serialized, "playerSummonSlot1Action", RequireSingle<PlayerSummonSlot1Action>(scene));
            SetObject(
                serialized,
                "playerSummonSlot2Action",
                supportSummons.FirstOrDefault(candidate => candidate.SlotActionName == "SummonSlot2"));
            SetObject(
                serialized,
                "playerSummonSlot3Action",
                supportSummons.FirstOrDefault(candidate => candidate.SlotActionName == "SummonSlot3"));
            SetObject(serialized, "playerRangedBasicAttackAction", RequireSingle<PlayerRangedBasicAttackAction>(scene));
            SetObject(serialized, "playerCombatModeController", RequireSingle<PlayerCombatModeController>(scene));
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        internal static void ConfigureBossBarrageCameraComposition(
            BossBarrageCameraCueDriver cueDriver,
            BossBarrageEmitter barrageEmitter,
            BossPressureActionDirector pressureActionDirector,
            ActionCameraController cameraController,
            Transform cueSpace,
            BossBarragePatternProfile crushNetPattern)
        {
            if (cueDriver == null
                || barrageEmitter == null
                || pressureActionDirector == null
                || cameraController == null
                || cueSpace == null
                || crushNetPattern == null)
            {
                throw new InvalidOperationException(
                    "Station CrushNet camera composition requires its canonical driver, boss sources, camera, cue space, and pattern.");
            }

            if (!string.Equals(
                    crushNetPattern.PatternId,
                    CrushNetCameraPatternId,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Expected {CrushNetCameraPatternId}, but resolved {crushNetPattern.PatternId}.");
            }

            var crushNetCue = new ActionCameraCueProfile.CameraCue
            {
                enabled = true,
                localOffset = Vector3.zero,
                planarDirectionOffset = 0f,
                fieldOfViewDelta = CrushNetCameraFieldOfViewDelta,
                cameraDistanceDelta = CrushNetCameraDistanceDelta,
                focusHeightDelta = 0f,
                durationSeconds = CrushNetCameraSustainSeconds,
                finisherScale = 1f
            };

            cueDriver.Configure(
                barrageEmitter,
                cameraController,
                cueSpace,
                pressureActionDirector);
            cueDriver.ConfigurePatternWindupCueOverrides(
                CrushNetCameraReleaseSeconds,
                new BossBarrageCameraCueDriver.PatternWindupCueOverride(
                    CrushNetCameraPatternId,
                    crushNetCue));

            SerializedObject cameraSerialized = new SerializedObject(cameraController);
            SetFloat(
                cameraSerialized,
                "maxCueFieldOfViewDelta",
                StationMaxCueFieldOfViewDelta);
            SetFloat(
                cameraSerialized,
                "maxCueCameraDistanceDelta",
                StationMaxCueCameraDistanceDelta);
            cameraSerialized.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(cueDriver);
            EditorUtility.SetDirty(cameraController);
        }

        private static void ConfigurePhaseTwoCueArrays(SerializedObject serialized)
        {
            SerializedProperty patterns = RequireProperty(serialized, "phaseTwoPatternCues");
            patterns.arraySize = 4;
            ConfigurePatternCue(patterns.GetArrayElementAtIndex(0), "AkazaHoverLance", "EliteAuraBuffer", "AttackLinePressure", new Color(0.24f, 0.9f, 1f), new Color(0.74f, 1f, 1f), 0.24f, 0.44f, CombatVfxCueId.EnemyLinePressureWindup, CombatVfxCueId.EnemyLinePressureActive);
            ConfigurePatternCue(patterns.GetArrayElementAtIndex(1), "AkazaSpiralVolley", "ElitePhaseSwap", "AttackFanPressure", new Color(0.62f, 0.36f, 1f), new Color(0.95f, 0.76f, 1f), 0.28f, 0.48f, CombatVfxCueId.ElitePhaseSwapSignal, CombatVfxCueId.EnemyFanPressureActive);
            ConfigurePatternCue(patterns.GetArrayElementAtIndex(2), "AkazaSummonCurtain", "EliteSummonPackage", "AttackFanPressure", new Color(0.28f, 1f, 0.66f), new Color(0.76f, 1f, 0.86f), 0.30f, 0.46f, CombatVfxCueId.EliteSummonSignal, CombatVfxCueId.EnemyFanPressureActive);
            ConfigurePatternCue(patterns.GetArrayElementAtIndex(3), "AkazaCrushNet", "ElitePhaseSwap", "AttackHeavy", new Color(1f, 0.2f, 0.38f), new Color(1f, 0.68f, 0.72f), 0.34f, 0.56f, CombatVfxCueId.EnemyGuardBreakWindup, CombatVfxCueId.EnemyGuardBreakActive);

            SerializedProperty pressures = RequireProperty(serialized, "phaseTwoPressureActionCues");
            pressures.arraySize = 3;
            ConfigurePressureCue(pressures.GetArrayElementAtIndex(0), BossPressureActionKind.SkillPattern, "AttackLinePressure", new Color(0.48f, 0.96f, 1f), 0.28f, 0.32f, 0.08f);
            ConfigurePressureCue(pressures.GetArrayElementAtIndex(1), BossPressureActionKind.SummonPressure, "EliteSummonPackage", new Color(0.42f, 1f, 0.72f), 0.36f, 0.38f, 0.09f);
            ConfigurePressureCue(pressures.GetArrayElementAtIndex(2), BossPressureActionKind.PunishOverextend, "AttackHeavy", new Color(1f, 0.24f, 0.32f), 0.42f, 0.50f, 0.11f);
        }

        private static void ConfigurePatternCue(
            SerializedProperty property,
            string patternId,
            string windup,
            string release,
            Color windupColor,
            Color releaseColor,
            float windupScale,
            float releaseScale,
            CombatVfxCueId windupCue,
            CombatVfxCueId releaseCue)
        {
            property.FindPropertyRelative("patternId").stringValue = patternId;
            property.FindPropertyRelative("windupTrigger").stringValue = windup;
            property.FindPropertyRelative("releaseTrigger").stringValue = release;
            property.FindPropertyRelative("windupColor").colorValue = windupColor;
            property.FindPropertyRelative("releaseColor").colorValue = releaseColor;
            property.FindPropertyRelative("windupPulseScale").floatValue = windupScale;
            property.FindPropertyRelative("releasePulseScale").floatValue = releaseScale;
            property.FindPropertyRelative("useWorldVfxCueOverride").boolValue = true;
            property.FindPropertyRelative("windupWorldCueId").enumValueIndex = (int)windupCue;
            property.FindPropertyRelative("releaseWorldCueId").enumValueIndex = (int)releaseCue;
            property.FindPropertyRelative("windupWorldCueIntensity").floatValue = 1f;
            property.FindPropertyRelative("releaseWorldCueIntensity").floatValue = 1f;
        }

        private static void ConfigurePressureCue(
            SerializedProperty property,
            BossPressureActionKind kind,
            string trigger,
            Color color,
            float duration,
            float pulseScale,
            float tierBonus)
        {
            property.FindPropertyRelative("actionKind").enumValueIndex = (int)kind;
            property.FindPropertyRelative("trigger").stringValue = trigger;
            property.FindPropertyRelative("color").colorValue = color;
            property.FindPropertyRelative("durationSeconds").floatValue = duration;
            property.FindPropertyRelative("pulseScale").floatValue = pulseScale;
            property.FindPropertyRelative("tierPulseBonus").floatValue = tierBonus;
        }

        private static void StripCinematicActorRuntime(GameObject actor)
        {
            foreach (MonoBehaviour behaviour in actor.GetComponentsInChildren<MonoBehaviour>(includeInactive: true))
            {
                UnityEngine.Object.DestroyImmediate(behaviour);
            }

            foreach (AudioSource audio in actor.GetComponentsInChildren<AudioSource>(includeInactive: true))
            {
                UnityEngine.Object.DestroyImmediate(audio);
            }

            foreach (Collider collider in actor.GetComponentsInChildren<Collider>(includeInactive: true))
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }

            foreach (ParticleSystem particles in actor.GetComponentsInChildren<ParticleSystem>(includeInactive: true))
            {
                UnityEngine.Object.DestroyImmediate(particles.gameObject);
            }

            foreach (SkinnedMeshRenderer renderer in actor.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true))
            {
                renderer.updateWhenOffscreen = true;
                renderer.forceMatrixRecalculationPerRender = true;
            }
        }

        private static Animator EnsureControllerFreeAnimator(GameObject root, string label)
        {
            Animator animator = root.GetComponent<Animator>() ?? root.AddComponent<Animator>();
            foreach (Animator candidate in root.GetComponentsInChildren<Animator>(includeInactive: true))
            {
                candidate.runtimeAnimatorController = null;
                candidate.applyRootMotion = false;
                candidate.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                candidate.updateMode = AnimatorUpdateMode.UnscaledTime;
                candidate.enabled = true;
            }

            if (!animator.enabled || animator.runtimeAnimatorController != null)
            {
                throw new InvalidOperationException($"Could not configure controller-free {label} Animator.");
            }

            return animator;
        }

        private static Camera ConfigureCinematicCamera(GameObject rig, string label)
        {
            Camera camera = rig.GetComponentInChildren<Camera>(includeInactive: true)
                ?? throw new InvalidOperationException($"{label} source has no Camera.");
            foreach (AudioListener listener in rig.GetComponentsInChildren<AudioListener>(includeInactive: true))
            {
                UnityEngine.Object.DestroyImmediate(listener);
            }

            camera.enabled = false;
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.nearClipPlane = Mathf.Max(0.01f, camera.nearClipPlane);
            camera.farClipPlane = Mathf.Max(500f, camera.farClipPlane);
            camera.allowHDR = true;
            camera.allowMSAA = true;
            UniversalAdditionalCameraData cameraData =
                camera.GetComponent<UniversalAdditionalCameraData>()
                ?? camera.gameObject.AddComponent<UniversalAdditionalCameraData>();
            cameraData.renderPostProcessing = true;
            cameraData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            cameraData.antialiasingQuality = AntialiasingQuality.High;
            cameraData.stopNaN = true;
            cameraData.dithering = true;
            cameraData.volumeLayerMask |= 1 << 0;
            return camera;
        }

        private static CanvasGroup CreateCurtain(Transform parent)
        {
            GameObject canvasObject = new GameObject(
                "AkazaPhase2_TransitionCurtain",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster),
                typeof(CanvasGroup));
            canvasObject.transform.SetParent(parent, false);
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32000;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(2560f, 1440f);
            scaler.matchWidthOrHeight = 1f;

            CanvasGroup group = canvasObject.GetComponent<CanvasGroup>();
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;

            GameObject imageObject = new GameObject("Black", typeof(RectTransform), typeof(Image));
            imageObject.transform.SetParent(canvasObject.transform, false);
            RectTransform rect = imageObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            Image image = imageObject.GetComponent<Image>();
            image.color = Color.black;
            image.raycastTarget = false;
            return group;
        }

        private static CinematicLights CreateSourceNeutralLights(Transform parent)
        {
            Light wingKey = CreateSourceDirectionalLight(
                parent,
                "AkazaPhase2_C33NeutralKey",
                new Quaternion(
                    0.6775798f,
                    -0.6077754f,
                    0.30155137f,
                    0.2838336f),
                intensity: 1.42f,
                shadows: LightShadows.None,
                cullingMask: 1 << CinematicActorLayer);
            Light eyeKey = CreateSourceDirectionalLight(
                parent,
                "AkazaPhase2_C34NeutralKey",
                new Quaternion(
                0.09356828f,
                -0.43303898f,
                0.89536166f,
                    0.045274504f),
                intensity: 1.42f,
                shadows: LightShadows.Soft,
                cullingMask: 1 << CinematicActorLayer);
            eyeKey.shadowStrength = 0.5f;
            eyeKey.shadowBias = 0.193f;
            eyeKey.shadowNormalBias = 0f;
            eyeKey.shadowNearPlane = 0.1f;

            Light backgroundKey = CreateSourceDirectionalLight(
                parent,
                "AkazaPhase2_C33BackgroundNeutralKey",
                new Quaternion(
                    -0.43647152f,
                    -0.33780316f,
                    0.6118709f,
                    -0.5665648f),
                intensity: 1f,
                shadows: LightShadows.None,
                cullingMask: ~(1 << CinematicActorLayer));
            return new CinematicLights(wingKey, eyeKey, backgroundKey);
        }

        private static Light CreateSourceDirectionalLight(
            Transform parent,
            string name,
            Quaternion rotation,
            float intensity,
            LightShadows shadows,
            int cullingMask)
        {
            GameObject lightObject = new GameObject(name);
            lightObject.transform.SetParent(parent, false);
            lightObject.transform.rotation = rotation;
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = Color.white;
            light.intensity = intensity;
            light.shadows = shadows;
            light.cullingMask = cullingMask;
            return light;
        }

        private static void CreateSourceSoftPostVolume(
            Transform parent,
            VolumeProfile profile)
        {
            GameObject volumeObject = new GameObject("AkazaPhase2_SourceSoftPostVolume");
            volumeObject.transform.SetParent(parent, false);
            Volume volume = volumeObject.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 220f;
            volume.weight = 0f;
            volume.sharedProfile = profile;
        }

        private static void SetLayerRecursively(Transform root, int layer)
        {
            foreach (Transform transform in root.GetComponentsInChildren<Transform>(includeInactive: true))
            {
                transform.gameObject.layer = layer;
            }
        }

        private static void AlignTransitionRigToBoss(Transform rig, GameObject actor, Transform bossRoot)
        {
            Bounds bounds = CalculateRendererBounds(actor);
            Vector3 targetCenter = new Vector3(bossRoot.position.x, bounds.center.y, bossRoot.position.z);
            Vector3 offset = targetCenter - bounds.center;
            offset.y = 0.52f - bounds.min.y;
            rig.position += offset;
        }

        private static void AlignGameplayVisualToCinematicTerminal(
            GameObject gameplayVisual,
            TransitionRig transitionRig)
        {
            PlayableDirector director = transitionRig.Director;
            if (director == null || director.playableAsset == null)
            {
                throw new InvalidOperationException(
                    "Akaza gameplay alignment requires the authored C33/C34 director.");
            }

            double savedTime = director.time;
            director.time = Math.Max(0d, MasterDurationSeconds - (1d / 30d));
            director.Evaluate();

            Animator gameplayAnimator = RequireComponentInChildren<Animator>(
                gameplayVisual,
                "Akaza gameplay terminal-pose Animator");
            gameplayAnimator.Rebind();
            gameplayAnimator.Play("Hover", 0, 0f);
            gameplayAnimator.Update(0f);
            Physics.SyncTransforms();

            Transform cinematicFacing = FindChildRecursive(
                transitionRig.Actor.transform,
                "CHakazaA:world_trs");
            Transform gameplayFacing = FindChildRecursive(
                gameplayVisual.transform,
                "CHakazaA:world_trs");
            if (cinematicFacing == null || gameplayFacing == null)
            {
                throw new InvalidOperationException(
                    "Akaza terminal alignment requires the shared CHakazaA:world_trs facing bone.");
            }

            Quaternion facingDelta = cinematicFacing.rotation * Quaternion.Inverse(gameplayFacing.rotation);
            gameplayVisual.transform.rotation = facingDelta * gameplayVisual.transform.rotation;
            Physics.SyncTransforms();

            // Both sides are the same Akaza hierarchy in the same terminal C34
            // pose. Renderer bounds are not an identity contract here: the
            // cinematic actor has many source SMRs while gameplay has four merged
            // SMRs with deliberately larger animation/culling envelopes. Match the
            // shared rig scale and carrier bone directly so renderer authoring can
            // never resize or floor-shift the handoff.
            Vector3 cinematicScale = transitionRig.Actor.transform.lossyScale;
            Vector3 gameplayScale = gameplayVisual.transform.lossyScale;
            gameplayVisual.transform.localScale = Vector3.Scale(
                gameplayVisual.transform.localScale,
                new Vector3(
                    SafeScaleRatio(cinematicScale.x, gameplayScale.x),
                    SafeScaleRatio(cinematicScale.y, gameplayScale.y),
                    SafeScaleRatio(cinematicScale.z, gameplayScale.z)));
            Physics.SyncTransforms();

            gameplayVisual.transform.position += cinematicFacing.position - gameplayFacing.position;
            Physics.SyncTransforms();

            ActionFoundationArenaTransformMotion hover =
                gameplayVisual.GetComponent<ActionFoundationArenaTransformMotion>();
            hover?.Configure(
                Vector3.zero,
                Vector3.up,
                0.28f,
                0.42f,
                0.15f,
                lockLocalRotation: true,
                lockLocalScale: true);

            director.time = savedTime;
            director.Evaluate();
            Physics.SyncTransforms();
        }

        private static float SafeScaleRatio(float target, float current)
        {
            if (!float.IsFinite(target)
                || !float.IsFinite(current)
                || Mathf.Abs(current) <= 0.0001f)
            {
                throw new InvalidOperationException(
                    $"Akaza terminal alignment received invalid world scales: target={target}, current={current}.");
            }

            return target / current;
        }

        private static void FitVisual(GameObject visual, float targetHeight, float desiredWorldBottom)
        {
            Bounds bounds = CalculateRendererBounds(visual);
            if (bounds.size.y <= 0.0001f)
            {
                throw new InvalidOperationException("Akaza gameplay visual has no measurable renderer bounds.");
            }

            float scale = Mathf.Clamp(targetHeight / bounds.size.y, 0.08f, 8f);
            visual.transform.localScale = Vector3.one * scale;
            bounds = CalculateRendererBounds(visual);
            visual.transform.position += Vector3.up * (desiredWorldBottom - bounds.min.y);
        }

        private static Bounds CalculateRendererBounds(GameObject root)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(includeInactive: true);
            Renderer first = renderers.FirstOrDefault(renderer => renderer != null && renderer.enabled);
            if (first == null)
            {
                return new Bounds(root.transform.position, Vector3.one);
            }

            Bounds bounds = first.bounds;
            foreach (Renderer renderer in renderers)
            {
                if (renderer != null && renderer.enabled)
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            return bounds;
        }

        private static Renderer[] ResolvePhaseTwoStructureRenderers(GameObject phaseTwoVisual)
        {
            if (phaseTwoVisual == null)
            {
                return Array.Empty<Renderer>();
            }

            return phaseTwoVisual
                .GetComponentsInChildren<Renderer>(includeInactive: true)
                .Where(renderer => renderer != null
                    && renderer.enabled
                    && IsPhaseTwoStructureRenderer(renderer.name))
                .Distinct()
                .ToArray();
        }

        private static Transform[] EnsurePhaseTwoBarrageSpawnOrigins(GameObject phaseTwoVisual)
        {
            string[] mirroredOrder = { "A", "F", "B", "E", "C", "D" };
            var origins = new Transform[mirroredOrder.Length];
            for (int i = 0; i < mirroredOrder.Length; i++)
            {
                string suffix = mirroredOrder[i];
                Transform bladeJoint = FindChildRecursive(
                    phaseTwoVisual.transform,
                    $"CHakazaA:akWpBlade{suffix}_jnt");
                if (bladeJoint == null)
                {
                    throw new InvalidOperationException(
                        $"Akaza Phase 2 lost blade joint {suffix} for its barrage origin rig.");
                }

                string muzzleName = $"AkazaPhase2_BarrageMuzzle_{suffix}";
                Transform muzzle = bladeJoint.Find(muzzleName);
                if (muzzle == null)
                {
                    var muzzleObject = new GameObject(muzzleName);
                    muzzle = muzzleObject.transform;
                    muzzle.SetParent(bladeJoint, worldPositionStays: false);
                }

                muzzle.localPosition = new Vector3(-0.63f, 0f, -0.025f);
                muzzle.localRotation = Quaternion.identity;
                muzzle.localScale = Vector3.one;
                origins[i] = muzzle;
            }

            return origins;
        }

        private static Transform EnsurePhaseTwoPulseAnchor(Transform structureRoot)
        {
            Transform pulseAnchor = structureRoot.Find(PhaseTwoPulseAnchorName);
            if (pulseAnchor == null)
            {
                var pulseAnchorObject = new GameObject(PhaseTwoPulseAnchorName);
                pulseAnchor = pulseAnchorObject.transform;
                pulseAnchor.SetParent(structureRoot, worldPositionStays: false);
            }

            pulseAnchor.localPosition = Vector3.zero;
            pulseAnchor.localRotation = Quaternion.identity;
            pulseAnchor.localScale = Vector3.one;
            return pulseAnchor;
        }

        private static bool IsPhaseTwoStructureRenderer(string objectName)
        {
            if (string.IsNullOrEmpty(objectName))
            {
                return false;
            }

            return objectName == "CHakazaA:BackParts"
                || objectName.StartsWith("CHakazaA:akArm", StringComparison.Ordinal)
                || objectName.StartsWith("CHakazaA:akWp_", StringComparison.Ordinal);
        }

        private static void ValidateProductWiring(
            Scene scene,
            OlympusStationAkazaPhase2FlowController flow,
            CombatHealth bossHealth,
            GameObject phaseOneVisual,
            GameObject phaseTwoVisual,
            TransitionRig rig,
            TimelineBindings timeline)
        {
            if (flow == null || flow.gameObject != bossHealth.gameObject)
            {
                throw new InvalidOperationException("Akaza phase-two flow must share the canonical boss-health root.");
            }

            if (phaseOneVisual == phaseTwoVisual || rig.Actor == phaseTwoVisual)
            {
                throw new InvalidOperationException("Phase one, cinematic actor, and gameplay Akaza must be distinct owners.");
            }

            if (phaseTwoVisual.GetComponentsInChildren<Collider>(includeInactive: true).Length != 0)
            {
                throw new InvalidOperationException("Akaza floating structures must not add gameplay colliders.");
            }

            if (phaseTwoVisual.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true).Length == 0
                || FindChildRecursive(phaseTwoVisual.transform, "CHakazaA:BackParts") == null)
            {
                throw new InvalidOperationException("Akaza gameplay visual must retain the floating-structure rig.");
            }

            Renderer[] structureRenderers = ResolvePhaseTwoStructureRenderers(phaseTwoVisual);
            if (structureRenderers.Length != 1
                || structureRenderers[0].name
                    != AkazaPhase2GameplayMeshCombiner.WingRendererName)
            {
                throw new InvalidOperationException(
                    "Akaza gameplay visual must expose one combined six-wing renderer to combat cues.");
            }

            AkazaPhase2CombatMotionDriver[] motionDrivers =
                phaseTwoVisual.GetComponentsInChildren<AkazaPhase2CombatMotionDriver>(
                    includeInactive: true);
            if (motionDrivers.Length != 1
                || motionDrivers[0].BossHealth != bossHealth
                || motionDrivers[0].ConfiguredWingCount != 6
                || Mathf.Abs(
                    motionDrivers[0].DeathSettleDurationSeconds
                    - AkazaPhase2CombatMotionDriver.RequiredDeathSettleSeconds) > 0.0001f
                || Mathf.Abs(
                    motionDrivers[0].DeathDropDistance
                    - AkazaPhase2CombatMotionDriver.RequiredDeathDropDistance) > 0.0001f
                || Mathf.Abs(
                    motionDrivers[0].DeathBackDistance
                    - AkazaPhase2CombatMotionDriver.RequiredDeathBackDistance) > 0.0001f
                || Mathf.Abs(
                    motionDrivers[0].DeathPitchDegrees
                    - AkazaPhase2CombatMotionDriver.RequiredDeathPitchDegrees) > 0.0001f
                || Mathf.Abs(
                    motionDrivers[0].DeathRollDegrees
                    - AkazaPhase2CombatMotionDriver.RequiredDeathRollDegrees) > 0.0001f
                || Mathf.Abs(
                    motionDrivers[0].DeathPivotLocalHeight
                    - AkazaPhase2CombatMotionDriver.RequiredDeathPivotLocalHeight) > 0.0001f
                || Mathf.Abs(
                    motionDrivers[0].DeathWingFoldDegrees
                    - AkazaPhase2CombatMotionDriver.RequiredDeathWingFoldDegrees) > 0.0001f
                || Mathf.Abs(
                    motionDrivers[0].DeathWingYawDegrees
                    - AkazaPhase2CombatMotionDriver.RequiredDeathWingYawDegrees) > 0.0001f)
            {
                throw new InvalidOperationException(
                    "Akaza gameplay visual requires one exact six-wing hover, recoil, pivoted-collapse, and death-motion owner.");
            }

            SkinnedMeshRenderer[] allGameplayRenderers = phaseTwoVisual
                .GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true);
            SkinnedMeshRenderer[] enabledGameplayRenderers = allGameplayRenderers
                .Where(renderer => renderer.enabled)
                .ToArray();
            int gameplayMaterialSlots = enabledGameplayRenderers
                .Sum(renderer => renderer.sharedMaterials.Length);
            int gameplayShadowCasters = enabledGameplayRenderers.Count(renderer =>
                renderer.shadowCastingMode != ShadowCastingMode.Off);
            if (enabledGameplayRenderers.Length > 4
                || gameplayMaterialSlots > 12
                || gameplayShadowCasters > 1
                || allGameplayRenderers.Any(renderer => renderer.updateWhenOffscreen)
                || phaseTwoVisual.GetComponentsInChildren<Renderer>(includeInactive: true)
                    .Any(renderer => renderer.motionVectorGenerationMode
                        != MotionVectorGenerationMode.ForceNoMotion)
                || structureRenderers.Any(renderer => renderer.shadowCastingMode != ShadowCastingMode.Off))
            {
                throw new InvalidOperationException(
                    $"Akaza gameplay renderers exceed the authored mobile budget: "
                    + $"renderers={enabledGameplayRenderers.Length}, "
                    + $"materialSlots={gameplayMaterialSlots}, "
                    + $"shadowCasters={gameplayShadowCasters}.");
            }

            if (rig.Root.GetComponentsInChildren<AudioSource>(includeInactive: true).Length != 0
                || rig.Root.GetComponentsInChildren<ParticleSystem>(includeInactive: true).Length != 0)
            {
                throw new InvalidOperationException("Phase-two transition may not import legacy audio or VFX owners.");
            }

            AkazaPhase2CinematicLookDriver[] lookDrivers =
                rig.Root.GetComponentsInChildren<AkazaPhase2CinematicLookDriver>(
                    includeInactive: true);
            Volume[] lookVolumes = rig.Root.GetComponentsInChildren<Volume>(includeInactive: true);
            GameplayLookStateController gameplayLookState =
                RequireSingle<GameplayLookStateController>(scene);
            UniversalAdditionalCameraData wingCameraData =
                rig.WingCamera.GetComponent<UniversalAdditionalCameraData>();
            UniversalAdditionalCameraData eyeCameraData =
                rig.EyeCamera.GetComponent<UniversalAdditionalCameraData>();
            if (lookDrivers.Length != 1
                || lookDrivers[0].SuppressedDirectionalLightCount == 0
                || lookDrivers[0].LookStateController != gameplayLookState
                || !gameplayLookState.HasBinding(GameplayLookState.Phase2Cinematic)
                || lookVolumes.Length != 1
                || !lookVolumes[0].isGlobal
                || lookVolumes[0].weight > 0.0001f
                || gameplayLookState.GetOverlayVolume(GameplayLookState.Phase2Cinematic)
                    != lookVolumes[0]
                || lookVolumes[0].sharedProfile
                    != RequireAsset<VolumeProfile>(PhaseTwoLookProfilePath)
                || wingCameraData == null
                || eyeCameraData == null
                || !wingCameraData.renderPostProcessing
                || !eyeCameraData.renderPostProcessing)
            {
                throw new InvalidOperationException(
                    "Akaza Phase 2 must retain its isolated C33/C34 lights and source-soft post profile.");
            }

            Material[] phaseTwoMaterials = phaseTwoVisual
                .GetComponentsInChildren<Renderer>(includeInactive: true)
                .SelectMany(renderer => renderer.sharedMaterials)
                .Where(material => material != null)
                .ToArray();
            foreach (string suffix in new[] { "Arm", "Body", "Face", "Eyes", "HairSpow", "Skin" })
            {
                Material expected = RequireAsset<Material>(
                    $"{PhaseTwoLookMaterialFolder}/M_Akaza_Phase2_{suffix}_SourceSoft.mat");
                if (!phaseTwoMaterials.Contains(expected)
                    || !expected.IsKeywordEnabled("_SHADINGGRADEMAP")
                    || Mathf.Abs(expected.GetFloat("_utsTechnique") - 1f) > 0.001f)
                {
                    throw new InvalidOperationException(
                        $"Akaza Phase 2 lost its source-soft {suffix} toon material contract.");
                }
            }

            if (rig.Director.playableAsset != timeline.Asset
                || Math.Abs(timeline.Asset.fixedDuration - MasterDurationSeconds) > 0.001d)
            {
                throw new InvalidOperationException("Phase-two master Timeline contract is incomplete.");
            }

            int flowCount = FindSceneComponents<OlympusStationAkazaPhase2FlowController>(scene).Length;
            if (flowCount != 1)
            {
                throw new InvalidOperationException($"Station requires exactly one phase-two flow; found {flowCount}.");
            }
        }

        private static CanvasGroup FindHudCanvasGroup(Scene scene)
        {
            GameObject hudRoot = RequireSceneObject(scene, CombatHudRootName);
            CanvasGroup[] groups = hudRoot.GetComponentsInChildren<CanvasGroup>(includeInactive: true);
            return groups.OrderBy(group => GetDepth(group.transform, hudRoot.transform)).FirstOrDefault();
        }

        private static int GetDepth(Transform transform, Transform root)
        {
            int depth = 0;
            while (transform != null && transform != root)
            {
                depth++;
                transform = transform.parent;
            }

            return depth;
        }

        private static AnimationClip RequirePrimaryClip(string path, float minimumLength)
        {
            AnimationClip clip = AssetDatabase.LoadAllAssetsAtPath(path)
                .OfType<AnimationClip>()
                .FirstOrDefault(candidate => !candidate.name.StartsWith("__preview__", StringComparison.Ordinal));
            if (clip == null || clip.length + 0.001f < minimumLength)
            {
                throw new InvalidOperationException(
                    $"Required source clip is missing or too short at {path}; length={clip?.length ?? 0f:0.000}.");
            }

            return clip;
        }

        private static void ImportRequiredSource(string path)
        {
            if (!System.IO.File.Exists(path))
            {
                throw new InvalidOperationException($"Required source asset missing: {path}");
            }

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }

        private static GameObject InstantiatePrefab(string path, Scene scene, Transform parent)
        {
            GameObject prefab = RequireAsset<GameObject>(path);
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab, scene) as GameObject
                ?? throw new InvalidOperationException($"Could not instantiate {path}.");
            instance.transform.SetParent(parent, false);
            return instance;
        }

        private static GameObject InstantiateAsset(string path, Scene scene, Transform parent)
        {
            GameObject prefab = RequireAsset<GameObject>(path);
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab, scene) as GameObject
                ?? throw new InvalidOperationException($"Could not instantiate {path}.");
            instance.transform.SetParent(parent, false);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;
            return instance;
        }

        private static T RequireAsset<T>(string path) where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            return asset != null
                ? asset
                : throw new InvalidOperationException($"Required asset missing: {path}");
        }

        private static T RequireSingle<T>(Scene scene) where T : Component
        {
            T[] components = FindSceneComponents<T>(scene);
            if (components.Length != 1)
            {
                throw new InvalidOperationException(
                    $"{scene.name} requires exactly one {typeof(T).Name}; found {components.Length}.");
            }

            return components[0];
        }

        private static T[] FindSceneComponents<T>(Scene scene) where T : Component
        {
            return scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<T>(includeInactive: true))
                .ToArray();
        }

        private static GameObject RequireSceneObject(Scene scene, string name)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                Transform found = root.GetComponentsInChildren<Transform>(includeInactive: true)
                    .FirstOrDefault(candidate => candidate.name == name);
                if (found != null)
                {
                    return found.gameObject;
                }
            }

            throw new InvalidOperationException($"{scene.name} is missing {name}.");
        }

        private static Transform RequireChildRecursive(Transform root, string name)
        {
            return FindChildRecursive(root, name)
                ?? throw new InvalidOperationException($"{root.name} is missing child {name}.");
        }

        private static Transform FindChildRecursive(Transform root, string name)
        {
            return root.GetComponentsInChildren<Transform>(includeInactive: true)
                .FirstOrDefault(candidate => candidate.name == name);
        }

        private static T RequireComponent<T>(GameObject root, string label) where T : Component
        {
            return root.GetComponent<T>()
                ?? throw new InvalidOperationException($"{label} is missing {typeof(T).Name}.");
        }

        private static T RequireComponentInChildren<T>(GameObject root, string label) where T : Component
        {
            return root.GetComponentInChildren<T>(includeInactive: true)
                ?? throw new InvalidOperationException($"{label} is missing {typeof(T).Name}.");
        }

        private static void DestroyNamedSceneObject(Scene scene, string name)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                Transform[] matches = root.GetComponentsInChildren<Transform>(includeInactive: true)
                    .Where(candidate => candidate.name == name)
                    .ToArray();
                foreach (Transform match in matches)
                {
                    if (match != null)
                    {
                        UnityEngine.Object.DestroyImmediate(match.gameObject);
                    }
                }
            }
        }

        private static void DestroyNamedChild(Transform root, string name)
        {
            Transform child = FindChildRecursive(root, name);
            if (child != null)
            {
                UnityEngine.Object.DestroyImmediate(child.gameObject);
            }
        }

        private static void EnsureFolderForAsset(string assetPath)
        {
            string folder = System.IO.Path.GetDirectoryName(assetPath)?.Replace('\\', '/');
            if (string.IsNullOrWhiteSpace(folder))
            {
                return;
            }

            string[] pieces = folder.Split('/');
            string current = pieces[0];
            for (int i = 1; i < pieces.Length; i++)
            {
                string next = current + "/" + pieces[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, pieces[i]);
                }

                current = next;
            }
        }

        private static SerializedProperty RequireProperty(SerializedObject serialized, string name)
        {
            return serialized.FindProperty(name)
                ?? throw new InvalidOperationException(
                    $"{serialized.targetObject.GetType().Name} is missing serialized field {name}.");
        }

        private static void SetObject(SerializedObject serialized, string name, UnityEngine.Object value)
        {
            RequireProperty(serialized, name).objectReferenceValue = value;
        }

        private static void SetFloat(SerializedObject serialized, string name, float value)
        {
            RequireProperty(serialized, name).floatValue = value;
        }

        private static void SetInt(SerializedObject serialized, string name, int value)
        {
            RequireProperty(serialized, name).intValue = value;
        }

        private static void SetBool(SerializedObject serialized, string name, bool value)
        {
            RequireProperty(serialized, name).boolValue = value;
        }

        private static void SetObjectArray(
            SerializedObject serialized,
            string name,
            IReadOnlyList<UnityEngine.Object> values)
        {
            SerializedProperty property = RequireProperty(serialized, name);
            property.arraySize = values?.Count ?? 0;
            for (int i = 0; i < property.arraySize; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
        }

        private readonly struct TimelineBindings
        {
            public TimelineBindings(
                TimelineAsset asset,
                AnimationTrack actorTrack,
                AnimationTrack wingCameraTrack,
                AnimationTrack eyeCameraTrack)
            {
                Asset = asset;
                ActorTrack = actorTrack;
                WingCameraTrack = wingCameraTrack;
                EyeCameraTrack = eyeCameraTrack;
            }

            public TimelineAsset Asset { get; }
            public AnimationTrack ActorTrack { get; }
            public AnimationTrack WingCameraTrack { get; }
            public AnimationTrack EyeCameraTrack { get; }
        }

        private readonly struct CinematicLights
        {
            public CinematicLights(Light wingKey, Light eyeKey, Light backgroundKey)
            {
                WingKey = wingKey;
                EyeKey = eyeKey;
                BackgroundKey = backgroundKey;
            }

            public Light WingKey { get; }
            public Light EyeKey { get; }
            public Light BackgroundKey { get; }
        }

        private readonly struct TransitionRig
        {
            public TransitionRig(
                GameObject root,
                GameObject actor,
                Animator actorAnimator,
                Camera wingCamera,
                Camera eyeCamera,
                CanvasGroup curtain,
                PlayableDirector director)
            {
                Root = root;
                Actor = actor;
                ActorAnimator = actorAnimator;
                WingCamera = wingCamera;
                EyeCamera = eyeCamera;
                Curtain = curtain;
                Director = director;
            }

            public GameObject Root { get; }
            public GameObject Actor { get; }
            public Animator ActorAnimator { get; }
            public Camera WingCamera { get; }
            public Camera EyeCamera { get; }
            public CanvasGroup Curtain { get; }
            public PlayableDirector Director { get; }
        }
    }
}
