using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Presentation;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;
using UnityEngine.UI;
using UnityEngine.VFX;

namespace DimensionBrawl.Editor
{
    public static class IntroGatePodBombingReviewSetup
    {
        private const string ReviewScenePath = "Assets/_Game/Scenes/IntroGatePodBombingReview.unity";
        private const string TimelinePath =
            "Assets/_Game/DesignData/Timelines/Cinematics/DB_Timeline_IntroGatePodBombingReview.playable";
        private const string OlympusStageScenePath = "Assets/_Game/Scenes/OlympusCorridorInvasionStage.unity";
        private const string AwakeningReviewScenePath = "Assets/_Game/Scenes/IntroGatePodCutsceneReview.unity";
        private const string AwakeningTimelinePath =
            "Assets/_Game/DesignData/Timelines/Cinematics/DB_Timeline_IntroGatePodAwakening.playable";
        private const string OlympusCombinedTimelinePath =
            "Assets/_Game/DesignData/Timelines/Cinematics/DB_Timeline_IntroGatePodAwakening_OlympusBombingPrelude.playable";
        private const string AwakeningProfilePath =
            "Assets/_Game/DesignData/Profiles/Cinematics/DB_Cinematic_IntroGatePodAwakening.asset";
        private const string OlympusCombinedProfilePath =
            "Assets/_Game/DesignData/Profiles/Cinematics/DB_Cinematic_IntroGatePodAwakening_OlympusBombingPrelude.asset";
        private const string OlympusStageDefinitionPath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageDefinitions/DB_Stage_OlympusCorridorIntroCombat.asset";
        private const string AnimationRoot =
            "Assets/_Game/DesignData/Animations/Cinematics/IntroGatePodBombingReview";
        private const string ModelRoot = "Assets/_Game/Art/Models/ActionFoundation/IntroGatePodBombingReview";
        private const string TextureRoot = "Assets/_Game/Art/Textures/ActionFoundation/IntroGatePodBombingReview";
        private const string MaterialRoot = "Assets/_Game/Art/Materials/ActionFoundation/IntroGatePodBombingReview";
        private const string MeshRoot = "Assets/_Game/Art/Meshes/ActionFoundation/IntroGatePodBombingReview";
        private const string VfxRoot = "Assets/_Game/Art/VFX/ActionFoundation/IntroGatePodBombingReview";
        private const string VfxPrefabRoot = VfxRoot + "/Prefabs";
        private const string VfxMaterialRoot = VfxRoot + "/Materials";
        private const string VfxTextureRoot = VfxRoot + "/Textures";
        private const string VfxMeshRoot = VfxRoot + "/Meshes";

        private const string SourceJetModelPath = "Assets/_Imported/SpecialSkillsEffectsPack/Models/Jet_04.fbx";
        private const string SourceBomberModelPath = "Assets/_Imported/SpecialSkillsEffectsPack/Models/Bomber_02.fbx";
        private const string SourceBombModelPath = "Assets/_Imported/SpecialSkillsEffectsPack/Models/Bomb_01.fbx";
        private const string SourceAircraftTexturePath =
            "Assets/_Imported/SpecialSkillsEffectsPack/Textures/SimpleTextures/Bomber_Texture.png";
        private const string SourceBomberMaterialPath =
            "Assets/_Imported/SpecialSkillsEffectsPack/Models/Materials/Bomber_02_Body_Texture.mat";
        private const string SourceJetMaterialPath =
            "Assets/_Imported/SpecialSkillsEffectsPack/Models/Materials/Jet_04_Body_Texture.mat";
        private const string SourceBombMaterialPath =
            "Assets/_Imported/SpecialSkillsEffectsPack/Models/Materials/Bomb_01_Texture.mat";
        private const string SourceSciFiBomberBaseTexturePath =
            "Assets/_Imported/AssetStore/Protofactor/Sci Fi/Common/Weapons/Textures/T_Bazooka_BaseColor.png";
        private const string SourceSciFiJetBaseTexturePath =
            "Assets/_Imported/AssetStore/Protofactor/Sci Fi/Common/Weapons/Textures/T_LaserGun_01_BaseColor.png";
        private const string SourceSciFiBombBaseTexturePath =
            "Assets/_Imported/AssetStore/Protofactor/Sci Fi/Common/Weapons/Textures/T_BazookaMagazine_BaseColor.png";
        private const string SourceVefectsExplosion03PrefabPath =
            "Assets/_Imported/AssetStore/VFX/Vefects/Flipbook VFX/Elements/Explosion/VFX_Explosion_03.prefab";
        private const string SourceVefectsExplosionFire02PrefabPath =
            "Assets/_Imported/AssetStore/VFX/Vefects/Flipbook VFX/Elements/Explosion/VFX_ExplosionFire_02.prefab";
        private const string SourceAirstrikeBombExplosionPrefabPath =
            "Assets/_Imported/SpecialSkillsEffectsPack/AllEffects/EffectsSet_1(NotScriptBased)/Effects/Effect_41_Airstrike/Effect_41_Base/Effect_41_BombExplosion.prefab";
        private const string SourceAirstrikeBombTrailPrefabPath =
            "Assets/_Imported/SpecialSkillsEffectsPack/AllEffects/EffectsSet_1(NotScriptBased)/Effects/Effect_41_Airstrike/Effect_41_Base/Effect_41_BombTrailParticle.prefab";
        private const string SourceShellExplosionPrefabPath =
            "Assets/_Imported/SpecialSkillsEffectsPack/AllEffects/EffectsSet_1(NotScriptBased)/Effects/Effect_10_SpaceFleetCall/Effect_10_Base/Effect_10_ShellExplosion.prefab";
        private const string SourceDavfxExplosion09PrefabPath =
            "Assets/_Imported/AssetStore/VFX/DAVFX/Realistic 6D Lighting Explosions/URP/Prefabs/Explosion 9.prefab";
        private const string SourceDavfxExplosion20PrefabPath =
            "Assets/_Imported/AssetStore/VFX/DAVFX/Realistic 6D Lighting Explosions/URP/Prefabs/Explosion 20.prefab";
        private const string SourceCyberBomberPrefabPath =
            "Assets/_Imported/SpecialSkillsEffectsPack/AllEffects/EffectsSet_2(ScriptBased)/Effects/Effect_46_CyberAirTroopInvader/Effect_46_Parts/Effect_46_CyberBomber.prefab";
        private const string SourceCyberBomber2PrefabPath =
            "Assets/_Imported/SpecialSkillsEffectsPack/AllEffects/EffectsSet_2(ScriptBased)/Effects/Effect_46_CyberAirTroopInvader/Effect_46_Parts/Effect_46_CyberBomber_2.prefab";
        private const string SourceCyberBombPrefabPath =
            "Assets/_Imported/SpecialSkillsEffectsPack/AllEffects/EffectsSet_2(ScriptBased)/Effects/Effect_46_CyberAirTroopInvader/Effect_46_Parts/Effect_46_Bomb.prefab";
        private const string SourceAerialBombPrefabPath =
            "Assets/_Imported/SpecialSkillsEffectsPack/AllEffects/EffectsSet_2(ScriptBased)/Effects/Effect_40_AerialBombing/Effect_40_Parts/Effect_40_Bomb.prefab";
        private const string SourceAerialBomb2PrefabPath =
            "Assets/_Imported/SpecialSkillsEffectsPack/AllEffects/EffectsSet_2(ScriptBased)/Effects/Effect_40_AerialBombing/Effect_40_Parts/Effect_40_Bomb_2.prefab";
        private const string SourceAerialBomb3PrefabPath =
            "Assets/_Imported/SpecialSkillsEffectsPack/AllEffects/EffectsSet_2(ScriptBased)/Effects/Effect_40_AerialBombing/Effect_40_Parts/Effect_40_Bomb_3.prefab";
        private const string UniGasFirePrefabPath =
            "Assets/_Game/Art/VFX/UNI VFX/Realistic Explosions, Fire & Smoke/Prefabs/UNI_Gas_Fire.prefab";
        private const string UniGroundFirePrefabPath =
            "Assets/_Game/Art/VFX/UNI VFX/Realistic Explosions, Fire & Smoke/Prefabs/UNI_Ground_Fire.prefab";
        private const string UniLongSmokePrefabPath =
            "Assets/_Game/Art/VFX/UNI VFX/Realistic Explosions, Fire & Smoke/Prefabs/UNI_Long_Smoke.prefab";
        private const string UniDeviceFirePrefabPath =
            "Assets/_Game/Art/VFX/UNI VFX/Realistic Explosions, Fire & Smoke/Prefabs/UNI_Device_Fire.prefab";

        private const string JetModelPath = ModelRoot + "/Jet_04.fbx";
        private const string BomberModelPath = ModelRoot + "/Bomber_02.fbx";
        private const string BombModelPath = ModelRoot + "/Bomb_01.fbx";
        private const string AircraftTexturePath = TextureRoot + "/Bomber_Texture.png";
        private const string ShockRingMeshPath = MeshRoot + "/AF_BombShockRing.mesh";
        private const string BomberMaterialPath = MaterialRoot + "/AF_BombingReview_BomberOriginal.mat";
        private const string JetMaterialPath = MaterialRoot + "/AF_BombingReview_JetOriginal.mat";
        private const string BombMaterialPath = MaterialRoot + "/AF_BombingReview_BombOriginal.mat";
        private const string VefectsExplosion03PrefabPath = VfxPrefabRoot + "/PF_BombingReview_VefectsExplosion03.prefab";
        private const string VefectsExplosionFire02PrefabPath =
            VfxPrefabRoot + "/PF_BombingReview_VefectsExplosionFire02.prefab";
        private const string AirstrikeBombExplosionPrefabPath =
            VfxPrefabRoot + "/PF_BombingReview_AirstrikeBombExplosion.prefab";
        private const string AirstrikeBombTrailPrefabPath = VfxPrefabRoot + "/PF_BombingReview_AirstrikeBombTrail.prefab";
        private const string ShellExplosionPrefabPath = VfxPrefabRoot + "/PF_BombingReview_ShellExplosion.prefab";
        private const string DavfxExplosion09PrefabPath = VfxPrefabRoot + "/PF_BombingReview_DavfxExplosion09.prefab";
        private const string DavfxExplosion20PrefabPath = VfxPrefabRoot + "/PF_BombingReview_DavfxExplosion20.prefab";
        private const string CyberBomberPrefabPath = VfxPrefabRoot + "/PF_BombingReview_CyberBomber.prefab";
        private const string CyberBomber2PrefabPath = VfxPrefabRoot + "/PF_BombingReview_CyberBomber2.prefab";
        private const string CyberBombPrefabPath = VfxPrefabRoot + "/PF_BombingReview_CyberBomb.prefab";
        private const string AerialBombPrefabPath = VfxPrefabRoot + "/PF_BombingReview_AerialBomb.prefab";
        private const string AerialBomb2PrefabPath = VfxPrefabRoot + "/PF_BombingReview_AerialBomb2.prefab";
        private const string AerialBomb3PrefabPath = VfxPrefabRoot + "/PF_BombingReview_AerialBomb3.prefab";

        private const string RootName = "IntroGatePodBombingReview";
        private const string OlympusPreludeRootName = "IntroGatePodBombingPrelude_Olympus";
        private const string AircraftRootName = "BombingReview_AircraftFormation";
        private const string BombDropRootName = "BombingReview_BombDrop";
        private const string ImpactRootName = "BombingReview_ImpactChain";
        private const string TargetZoneName = "BombingReview_TargetZone";
        private const string TargetDeckName = "BombingReview_TargetDeck_Main";
        private const string BombingPreludeTargetZoneTrackName = "Bombing Prelude Target Zone Active";
        private const string ProceduralFireCorePrefix = "BombImpact_ProceduralFireCore";
        private const string ShockRingName = "GroundShockRing";
        private const string ProceduralExplosionCoreMaterialPath =
            MaterialRoot + "/AF_BombingReview_ExplosionCore.mat";
        private const string ProceduralShockRingMaterialPath =
            MaterialRoot + "/AF_BombingReview_ShockRing.mat";
        private const string ProceduralSmokeMaterialPath =
            MaterialRoot + "/AF_BombingReview_Smoke.mat";
        private const string AftermathScorchPatchPrefix = "AftermathScorchPatch";
        private const string AftershockTransitionShakeTrackName = "Bombing Prelude Aftershock Transition Shake";
        private const string HandoffTransitionShakeTrackName = "Bombing Prelude Handoff Transition Shake";
        private const string AftershockTransitionShakeClipName = "AC_OlympusBombingPrelude_AftershockTransitionShake";
        private const string HandoffTransitionShakeClipName = "AC_OlympusBombingPrelude_HandoffTransitionShake";
        private const string AftershockShotName = "cm_06_aftershock";
        private const string HandoffShotName = "cm_07_smoke_handoff";
        private const float OlympusCommandoHoldUntilSeconds = (863f / 30f) + 0.12f;
        private const string SmokeRootName = "BombingReview_AftermathSmoke";
        private const string CameraRootName = "BombingReview_CinemachineShots";
        private const string MainCameraName = "BombingReview_MainCamera";
        private const string TimelineDirectorName = "BombingReview_TimelineDirector";

        private const float TimelineDurationSeconds = 8.8f;
        private const int CaptureWidth = 1600;
        private const int CaptureHeight = 900;
        private const string ReportPath = "C:/tmp/DimensionBrawl-IntroGatePodBombingReview.md";
        private const string ExplosionAuditPath = "C:/tmp/DimensionBrawl-BombingReview-ExplosionAssetAudit.md";
        private const string CoordinateAuditPath = "C:/tmp/DimensionBrawl-BombingReview-FormationCoordinateAudit.md";
        private const string OlympusMergeReportPath = "C:/tmp/DimensionBrawl-IntroGatePodBombingPreludeMerge.md";
        private const float BomberLeadEntryEndSeconds = 1.05f;
        private const float LeftEscortJoinStartSeconds = 0.95f;
        private const float LeftEscortJoinEndSeconds = 3.25f;
        private const float RightEscortJoinStartSeconds = 1.45f;
        private const float RightEscortJoinEndSeconds = 3.55f;
        private const float BombReleaseStartSeconds = 3.72f;
        private const float FormationWingX = 6.60f;
        private const float FormationWingY = -0.12f;
        private const float FormationWingZ = 0.00f;
        private const float MinimumAircraftSeparation = 5.35f;
        private const float FormationLockSampleSeconds = 3.62f;

        private static readonly CaptureSpec[] CaptureSpecs =
        {
            new CaptureSpec("01_lead_entry", 0.35f, "C:/tmp/DimensionBrawl-BombingReview-01-LeadEntry.png"),
            new CaptureSpec("02_left_join", 1.85f, "C:/tmp/DimensionBrawl-BombingReview-02-LeftJoin.png"),
            new CaptureSpec("03_formation_lock", 3.62f, "C:/tmp/DimensionBrawl-BombingReview-03-FormationLock.png"),
            new CaptureSpec("04_bomb_release", 3.95f, "C:/tmp/DimensionBrawl-BombingReview-04-BombRelease.png"),
            new CaptureSpec("05_falling_payload", 5.16f, "C:/tmp/DimensionBrawl-BombingReview-05-FallingPayload.png"),
            new CaptureSpec("06_impact_chain", 6.42f, "C:/tmp/DimensionBrawl-BombingReview-06-ImpactChain.png"),
            new CaptureSpec("07_aftershock", 7.62f, "C:/tmp/DimensionBrawl-BombingReview-07-Aftershock.png")
        };

        [MenuItem("Tools/DimensionBrawl/Intro GatePod/Create Bombing Review Timeline")]
        public static void CreateBombingReviewTimelineMenu()
        {
            CreateBombingReviewTimeline();
        }

        [MenuItem("Tools/DimensionBrawl/Intro GatePod/Validate Bombing Review Timeline")]
        public static void ValidateBombingReviewTimelineMenu()
        {
            ValidateBombingReviewTimeline(writeReport: true);
        }

        public static void RunBatchVerification()
        {
            CreateBombingReviewTimeline();
            ValidateBombingReviewTimeline(writeReport: true, renderCaptures: true, runCoordinateAudit: true);
        }

        public static void RunBatchCoordinateVerification()
        {
            CreateBombingReviewTimeline();
            ValidateBombingReviewTimeline(writeReport: true, renderCaptures: false, runCoordinateAudit: true);
        }

        [MenuItem("Tools/DimensionBrawl/Intro GatePod/Merge Bombing Review Into Olympus Intro")]
        public static void MergeBombingReviewIntoOlympusIntroMenu()
        {
            MergeBombingReviewIntoOlympusIntro(writeReport: true);
        }

        public static void RunBatchMergeBombingReviewIntoOlympusIntro()
        {
            MergeBombingReviewIntoOlympusIntro(writeReport: true);
        }

        [MenuItem("Tools/DimensionBrawl/Intro GatePod/Remove Olympus Bombing Prelude Target Plane")]
        public static void RemoveOlympusBombingPreludeTargetPlaneMenu()
        {
            RemoveOlympusBombingPreludeTargetPlane(writeReport: true);
        }

        public static void RunBatchRemoveOlympusBombingPreludeTargetPlane()
        {
            RemoveOlympusBombingPreludeTargetPlane(writeReport: true);
        }

        [MenuItem("Tools/DimensionBrawl/Intro GatePod/Remove Procedural Explosion Materials")]
        public static void RemoveProceduralExplosionMaterialsMenu()
        {
            RemoveProceduralExplosionMaterials(writeReport: true);
        }

        public static void RunBatchRemoveProceduralExplosionMaterials()
        {
            RemoveProceduralExplosionMaterials(writeReport: true);
        }

        [MenuItem("Tools/DimensionBrawl/Intro GatePod/Add Olympus Bombing Transition Camera Shake")]
        public static void AddOlympusBombingTransitionCameraShakeMenu()
        {
            AddOlympusBombingTransitionCameraShake(writeReport: true);
        }

        public static void RunBatchAddOlympusBombingTransitionCameraShake()
        {
            AddOlympusBombingTransitionCameraShake(writeReport: true);
        }

        private static void CreateBombingReviewTimeline()
        {
            PromoteSourceAssets();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "IntroGatePodBombingReview";

            GameObject root = new GameObject(RootName);
            SceneManager.MoveGameObjectToScene(root, scene);

            Material aircraftMaterial = LoadOrCreateTextureMaterial(
                MaterialRoot + "/AF_BombingReview_Aircraft.mat",
                AircraftTexturePath,
                new Color(0.72f, 0.82f, 0.94f, 1f),
                new Color(0.06f, 0.16f, 0.32f, 1f),
                0.44f,
                0.28f);
            Material bombMaterial = LoadOrCreateLitMaterial(
                MaterialRoot + "/AF_BombingReview_Bomb.mat",
                new Color(0.08f, 0.09f, 0.105f, 1f),
                new Color(0.0f, 0.0f, 0.0f, 1f),
                0.52f,
                0.62f);
            Material skyMaterial = LoadOrCreateUnlitMaterial(
                MaterialRoot + "/AF_BombingReview_Sky.mat",
                new Color(0.62f, 0.66f, 0.62f, 1f),
                Color.black,
                transparent: false);
            Material cloudMaterial = LoadOrCreateLitMaterial(
                MaterialRoot + "/AF_BombingReview_Cloud.mat",
                new Color(0.86f, 0.88f, 0.83f, 0.5f),
                new Color(0.16f, 0.17f, 0.15f, 1f),
                0.18f,
                0.0f);
            ConfigureTransparentMaterial(cloudMaterial, alpha: 0.5f);
            CreateLighting(root.transform);
            CreateEnvironment(
                root.transform,
                skyMaterial,
                cloudMaterial,
                out Transform cloudRoot);

            Transform aircraftRoot = CreateAircraftFormation(root.transform, aircraftMaterial);
            Transform bombDropRoot = CreateBombDrop(root.transform, bombMaterial);
            Transform impactRoot = CreateImpactChain(root.transform);
            Transform smokeRoot = CreateAftermathSmoke(root.transform);
            bombDropRoot.gameObject.SetActive(false);
            impactRoot.gameObject.SetActive(false);
            smokeRoot.gameObject.SetActive(false);

            Camera mainCamera = CreateMainCamera(scene);
            CinemachineBrain brain = mainCamera.GetComponent<CinemachineBrain>();
            TransitionOverlayBindings transitionOverlay = CreateTransitionOverlay(scene, root.transform, mainCamera);
            CameraShot[] shots = CreateCinemachineShots(root.transform, aircraftRoot, bombDropRoot, brain);
            PlayableDirector director = CreateTimeline(
                scene,
                root.transform,
                brain,
                shots,
                aircraftRoot,
                cloudRoot,
                bombDropRoot,
                impactRoot,
                smokeRoot,
                transitionOverlay);

            EditorSceneManager.SaveScene(scene, ReviewScenePath);
            EditorUtility.SetDirty(director);
            AssetDatabase.SaveAssets();
        }

        private static void MergeBombingReviewIntoOlympusIntro(bool writeReport)
        {
            AssetDatabase.Refresh();
            ValidateBombingReviewTimeline(writeReport: true, renderCaptures: false, runCoordinateAudit: true);

            TimelineAsset sourceTimeline = LoadRequired<TimelineAsset>(AwakeningTimelinePath);
            CinematicSequenceProfile combinedProfile = CreateShiftedOlympusProfile();
            TimelineAsset combinedTimeline = CreateShiftedOlympusTimeline(sourceTimeline);

            Scene stageScene = EditorSceneManager.OpenScene(OlympusStageScenePath, OpenSceneMode.Single);
            Scene sourceScene = default;
            try
            {
                sourceScene = EditorSceneManager.OpenScene(AwakeningReviewScenePath, OpenSceneMode.Additive);

                Transform runtimeRoot = RequireObjectInScene(stageScene, "IntroGatePodPortPayload_CutsceneRuntime").transform;
                Transform visualRoot = RequireObjectInScene(stageScene, "IntroGatePodPortPayload_Visuals").transform;
                Camera stageCamera = RequireComponentByObjectName<Camera>(runtimeRoot, "Main Camera");
                CinemachineBrain brain = stageCamera.GetComponent<CinemachineBrain>()
                    ?? throw new InvalidOperationException("Olympus intro runtime camera is missing CinemachineBrain.");
                PlayableDirector director = RequireComponentByObjectName<PlayableDirector>(
                    runtimeRoot,
                    "IntroGatePodReview_TimelineDirector");

                GameObject oldPrelude = FindObjectInScene(stageScene, OlympusPreludeRootName);
                if (oldPrelude != null)
                {
                    UnityEngine.Object.DestroyImmediate(oldPrelude);
                }

                BombingPreludeSceneBindings prelude =
                    CreateBombingPreludeSceneBindings(stageScene, brain, stageCamera);

                combinedTimeline = LoadRequired<TimelineAsset>(OlympusCombinedTimelinePath);
                director.playableAsset = combinedTimeline;
                director.playOnAwake = true;
                director.timeUpdateMode = DirectorUpdateMode.UnscaledGameTime;
                director.extrapolationMode = DirectorWrapMode.Hold;

                IntroGatePodCinemachineShotPlayer.Shot[] shiftedShots =
                    RebindShiftedAwakeningRuntime(sourceScene, runtimeRoot, visualRoot, brain, director, combinedProfile);
                RebindShiftedAwakeningTimelineTracks(combinedTimeline, director, runtimeRoot, visualRoot, brain, shiftedShots);
                AddBombingPreludeTracks(combinedTimeline, director, brain, prelude);
                UpdateOlympusStageDefinitionForBombingPrelude();

                director.time = 0d;
                director.Evaluate();
                EditorUtility.SetDirty(director);
                EditorSceneManager.MarkSceneDirty(stageScene);
                EditorSceneManager.SaveScene(stageScene);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                combinedProfile = LoadRequired<CinematicSequenceProfile>(OlympusCombinedProfilePath);
                combinedTimeline = LoadRequired<TimelineAsset>(OlympusCombinedTimelinePath);

                List<string> issues = ValidateOlympusBombingPreludeMerge(
                    stageScene,
                    director,
                    combinedTimeline,
                    combinedProfile,
                    prelude.Root);
                if (writeReport)
                {
                    WriteOlympusMergeReport(issues, combinedTimeline, combinedProfile);
                }

                if (issues.Count > 0)
                {
                    throw new InvalidOperationException(
                        "Olympus bombing prelude merge validation failed:\n" + string.Join("\n", issues));
                }
            }
            finally
            {
                if (sourceScene.IsValid() && sourceScene.isLoaded)
                {
                    EditorSceneManager.CloseScene(sourceScene, removeScene: true);
                }
            }
        }

        private static void RemoveOlympusBombingPreludeTargetPlane(bool writeReport)
        {
            AssetDatabase.Refresh();
            Scene stageScene = EditorSceneManager.OpenScene(OlympusStageScenePath, OpenSceneMode.Single);
            GameObject preludeRoot = FindObjectInScene(stageScene, OlympusPreludeRootName)
                ?? throw new InvalidOperationException($"Missing `{OlympusPreludeRootName}` in Olympus stage scene.");

            Transform targetDeck = FindDescendantOrSelf(preludeRoot.transform, TargetDeckName);
            Transform targetZone = FindDescendantOrSelf(preludeRoot.transform, TargetZoneName);
            if (targetZone != null)
            {
                UnityEngine.Object.DestroyImmediate(targetZone.gameObject);
            }
            else if (targetDeck != null)
            {
                UnityEngine.Object.DestroyImmediate(targetDeck.gameObject);
            }

            TimelineAsset combinedTimeline = LoadRequired<TimelineAsset>(OlympusCombinedTimelinePath);
            PlayableDirector director = FindComponentInScene<PlayableDirector>(stageScene)
                ?? throw new InvalidOperationException("Missing Olympus intro PlayableDirector.");
            director.playableAsset = combinedTimeline;
            RemoveTimelineTrack(combinedTimeline, BombingPreludeTargetZoneTrackName, director);

            director.time = 0d;
            director.Evaluate();
            EditorUtility.SetDirty(director);
            EditorUtility.SetDirty(combinedTimeline);
            EditorSceneManager.MarkSceneDirty(stageScene);
            EditorSceneManager.SaveScene(stageScene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            CinematicSequenceProfile combinedProfile = LoadRequired<CinematicSequenceProfile>(OlympusCombinedProfilePath);
            combinedTimeline = LoadRequired<TimelineAsset>(OlympusCombinedTimelinePath);
            List<string> issues = ValidateOlympusBombingPreludeMerge(
                stageScene,
                director,
                combinedTimeline,
                combinedProfile,
                preludeRoot);
            if (writeReport)
            {
                WriteOlympusMergeReport(issues, combinedTimeline, combinedProfile);
            }

            if (issues.Count > 0)
            {
                throw new InvalidOperationException(
                    "Olympus bombing prelude target-plane removal validation failed:\n" + string.Join("\n", issues));
            }
        }

        private static void RemoveProceduralExplosionMaterials(bool writeReport)
        {
            AssetDatabase.Refresh();
            Scene stageScene = EditorSceneManager.OpenScene(OlympusStageScenePath, OpenSceneMode.Single);
            GameObject preludeRoot = FindObjectInScene(stageScene, OlympusPreludeRootName)
                ?? throw new InvalidOperationException($"Missing `{OlympusPreludeRootName}` in Olympus stage scene.");
            RemoveProceduralExplosionObjects(preludeRoot.transform);
            EditorSceneManager.MarkSceneDirty(stageScene);
            EditorSceneManager.SaveScene(stageScene);

            Scene reviewScene = EditorSceneManager.OpenScene(ReviewScenePath, OpenSceneMode.Single);
            GameObject reviewRoot = FindObjectInScene(reviewScene, RootName);
            if (reviewRoot != null)
            {
                RemoveProceduralExplosionObjects(reviewRoot.transform);
                EditorSceneManager.MarkSceneDirty(reviewScene);
                EditorSceneManager.SaveScene(reviewScene);
            }

            DeleteAssetIfExists(ProceduralExplosionCoreMaterialPath);
            DeleteAssetIfExists(ProceduralShockRingMaterialPath);
            DeleteAssetIfExists(ProceduralSmokeMaterialPath);
            DeleteAssetIfExists(ShockRingMeshPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            stageScene = EditorSceneManager.OpenScene(OlympusStageScenePath, OpenSceneMode.Single);
            preludeRoot = FindObjectInScene(stageScene, OlympusPreludeRootName)
                ?? throw new InvalidOperationException($"Missing `{OlympusPreludeRootName}` in Olympus stage scene after cleanup.");
            PlayableDirector director = FindComponentInScene<PlayableDirector>(stageScene)
                ?? throw new InvalidOperationException("Missing Olympus intro PlayableDirector.");
            TimelineAsset combinedTimeline = LoadRequired<TimelineAsset>(OlympusCombinedTimelinePath);
            CinematicSequenceProfile combinedProfile = LoadRequired<CinematicSequenceProfile>(OlympusCombinedProfilePath);
            director.playableAsset = combinedTimeline;
            List<string> issues = ValidateOlympusBombingPreludeMerge(
                stageScene,
                director,
                combinedTimeline,
                combinedProfile,
                preludeRoot);
            ValidateProceduralExplosionMaterialsRemoved(preludeRoot.transform, issues);
            if (writeReport)
            {
                WriteOlympusMergeReport(issues, combinedTimeline, combinedProfile);
            }

            if (issues.Count > 0)
            {
                throw new InvalidOperationException(
                    "Procedural bombing-prelude explosion cleanup validation failed:\n" + string.Join("\n", issues));
            }
        }

        private static void AddOlympusBombingTransitionCameraShake(bool writeReport)
        {
            AssetDatabase.Refresh();
            Scene stageScene = EditorSceneManager.OpenScene(OlympusStageScenePath, OpenSceneMode.Single);
            GameObject preludeRoot = FindObjectInScene(stageScene, OlympusPreludeRootName)
                ?? throw new InvalidOperationException($"Missing `{OlympusPreludeRootName}` in Olympus stage scene.");
            Transform aftershockShot = RequireDescendantOrSelf(preludeRoot.transform, AftershockShotName);
            Transform handoffShot = RequireDescendantOrSelf(preludeRoot.transform, HandoffShotName);
            Animator aftershockAnimator = EnsureTimelineAnimator(aftershockShot.gameObject);
            Animator handoffAnimator = EnsureTimelineAnimator(handoffShot.gameObject);

            TimelineAsset combinedTimeline = LoadRequired<TimelineAsset>(OlympusCombinedTimelinePath);
            PlayableDirector director = FindComponentInScene<PlayableDirector>(stageScene)
                ?? throw new InvalidOperationException("Missing Olympus intro PlayableDirector.");
            director.playableAsset = combinedTimeline;
            RemoveTimelineTrack(combinedTimeline, AftershockTransitionShakeTrackName, director);
            RemoveTimelineTrack(combinedTimeline, HandoffTransitionShakeTrackName, director);
            AddBombingTransitionCameraShakeTracks(
                combinedTimeline,
                director,
                aftershockShot,
                aftershockAnimator,
                handoffShot,
                handoffAnimator);

            director.time = 0d;
            director.Evaluate();
            EditorUtility.SetDirty(director);
            EditorUtility.SetDirty(combinedTimeline);
            EditorSceneManager.MarkSceneDirty(stageScene);
            EditorSceneManager.SaveScene(stageScene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            combinedTimeline = LoadRequired<TimelineAsset>(OlympusCombinedTimelinePath);
            CinematicSequenceProfile combinedProfile = LoadRequired<CinematicSequenceProfile>(OlympusCombinedProfilePath);
            List<string> issues = ValidateOlympusBombingPreludeMerge(
                stageScene,
                director,
                combinedTimeline,
                combinedProfile,
                preludeRoot);
            if (writeReport)
            {
                WriteOlympusMergeReport(issues, combinedTimeline, combinedProfile);
            }

            if (issues.Count > 0)
            {
                throw new InvalidOperationException(
                    "Olympus bombing transition camera-shake validation failed:\n" + string.Join("\n", issues));
            }
        }

        private static CinematicSequenceProfile CreateShiftedOlympusProfile()
        {
            EnsureFolder(PathParent(OlympusCombinedProfilePath));
            if (AssetDatabase.LoadAssetAtPath<CinematicSequenceProfile>(OlympusCombinedProfilePath) != null)
            {
                AssetDatabase.DeleteAsset(OlympusCombinedProfilePath);
            }

            if (!AssetDatabase.CopyAsset(AwakeningProfilePath, OlympusCombinedProfilePath))
            {
                throw new InvalidOperationException(
                    $"Failed to copy `{AwakeningProfilePath}` to `{OlympusCombinedProfilePath}`.");
            }

            CinematicSequenceProfile profile = LoadRequired<CinematicSequenceProfile>(OlympusCombinedProfilePath);
            profile.name = Path.GetFileNameWithoutExtension(OlympusCombinedProfilePath);
            SerializedObject serialized = new SerializedObject(profile);
            RequireProperty(serialized, "sequenceId").stringValue =
                Path.GetFileNameWithoutExtension(OlympusCombinedProfilePath);
            RequireProperty(serialized, "displayName").stringValue =
                "Intro GatePod Awakening - Olympus Bombing Prelude";
            RequireProperty(serialized, "reviewerIntent").stringValue =
                "Olympus stage runtime intro with the validated BombingReview airstrike prepended before the protected GatePod awakening beats.";
            RequireProperty(serialized, "authoredDurationSeconds").floatValue += TimelineDurationSeconds;
            AddOffsetToCueStartTimes(RequireProperty(serialized, "cameraCues"), TimelineDurationSeconds);
            AddOffsetToCueStartTimes(RequireProperty(serialized, "actorCues"), TimelineDurationSeconds);
            AddOffsetToCueStartTimes(RequireProperty(serialized, "vfxCues"), TimelineDurationSeconds);
            AddOffsetToCueStartTimes(RequireProperty(serialized, "tutorialCues"), TimelineDurationSeconds);
            SerializedProperty handoff = RequireProperty(serialized, "gameplayHandoff");
            SerializedProperty handoffStart = handoff.FindPropertyRelative("startSeconds");
            if (handoffStart != null)
            {
                handoffStart.floatValue += TimelineDurationSeconds;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            return profile;
        }

        private static TimelineAsset CreateShiftedOlympusTimeline(TimelineAsset sourceTimeline)
        {
            EnsureFolder(PathParent(OlympusCombinedTimelinePath));
            if (AssetDatabase.LoadAssetAtPath<TimelineAsset>(OlympusCombinedTimelinePath) != null)
            {
                AssetDatabase.DeleteAsset(OlympusCombinedTimelinePath);
            }

            if (!AssetDatabase.CopyAsset(AwakeningTimelinePath, OlympusCombinedTimelinePath))
            {
                throw new InvalidOperationException(
                    $"Failed to copy `{AwakeningTimelinePath}` to `{OlympusCombinedTimelinePath}`.");
            }

            TimelineAsset timeline = LoadRequired<TimelineAsset>(OlympusCombinedTimelinePath);
            timeline.name = Path.GetFileNameWithoutExtension(OlympusCombinedTimelinePath);
            timeline.durationMode = TimelineAsset.DurationMode.FixedLength;
            timeline.fixedDuration = sourceTimeline.fixedDuration + TimelineDurationSeconds;
            timeline.editorSettings.frameRate = 30d;
            ShiftTimelineClips(timeline, TimelineDurationSeconds);
            EditorUtility.SetDirty(timeline);
            AssetDatabase.SaveAssets();
            return timeline;
        }

        private static BombingPreludeSceneBindings CreateBombingPreludeSceneBindings(
            Scene scene,
            CinemachineBrain brain,
            Camera stageCamera)
        {
            GameObject rootObject = new GameObject(OlympusPreludeRootName);
            SceneManager.MoveGameObjectToScene(rootObject, scene);
            Transform root = rootObject.transform;
            root.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            root.localScale = Vector3.one;

            Material aircraftMaterial = LoadOrCreateTextureMaterial(
                MaterialRoot + "/AF_BombingReview_Aircraft.mat",
                AircraftTexturePath,
                new Color(0.72f, 0.82f, 0.94f, 1f),
                new Color(0.06f, 0.16f, 0.32f, 1f),
                0.44f,
                0.28f);
            Material bombMaterial = LoadOrCreateLitMaterial(
                MaterialRoot + "/AF_BombingReview_Bomb.mat",
                new Color(0.08f, 0.09f, 0.105f, 1f),
                Color.black,
                0.52f,
                0.62f);
            Material cloudMaterial = LoadOrCreateLitMaterial(
                MaterialRoot + "/AF_BombingReview_Cloud.mat",
                new Color(0.86f, 0.88f, 0.83f, 0.5f),
                new Color(0.16f, 0.17f, 0.15f, 1f),
                0.18f,
                0.0f);
            ConfigureTransparentMaterial(cloudMaterial, alpha: 0.5f);
            CreateLighting(root);
            CreateEnvironment(root, null, cloudMaterial, out Transform cloudRoot);
            Transform aircraftRoot = CreateAircraftFormation(root, aircraftMaterial);
            Transform bombDropRoot = CreateBombDrop(root, bombMaterial);
            Transform impactRoot = CreateImpactChain(root);
            Transform smokeRoot = CreateAftermathSmoke(root);
            bombDropRoot.gameObject.SetActive(false);
            impactRoot.gameObject.SetActive(false);
            smokeRoot.gameObject.SetActive(false);

            TransitionOverlayBindings transitionOverlay = CreateTransitionOverlay(scene, root, stageCamera);
            CameraShot[] shots = CreateCinemachineShots(root, aircraftRoot, bombDropRoot, brain);
            return new BombingPreludeSceneBindings(
                rootObject,
                shots,
                aircraftRoot,
                cloudRoot,
                bombDropRoot,
                impactRoot,
                smokeRoot,
                transitionOverlay);
        }

        private static IntroGatePodCinemachineShotPlayer.Shot[] RebindShiftedAwakeningRuntime(
            Scene sourceScene,
            Transform runtimeRoot,
            Transform visualRoot,
            CinemachineBrain brain,
            PlayableDirector director,
            CinematicSequenceProfile combinedProfile)
        {
            IntroGatePodCinemachineShotPlayer sourceShotPlayer =
                FindComponentInScene<IntroGatePodCinemachineShotPlayer>(sourceScene)
                ?? throw new InvalidOperationException("Missing source IntroGatePodCinemachineShotPlayer.");
            IntroGatePodCinemachineShotPlayer targetShotPlayer =
                RequireComponentByObjectName<IntroGatePodCinemachineShotPlayer>(
                    runtimeRoot,
                    "IntroGatePodReview_CinemachineShotPlayer");
            IntroGatePodCinemachineShotPlayer.Shot[] sourceShots = sourceShotPlayer.Shots;
            IntroGatePodCinemachineShotPlayer.Shot[] targetShots =
                new IntroGatePodCinemachineShotPlayer.Shot[sourceShots.Length];
            for (int i = 0; i < sourceShots.Length; i++)
            {
                IntroGatePodCinemachineShotPlayer.Shot sourceShot = sourceShots[i];
                CinemachineCamera targetCamera =
                    RequireComponentByObjectName<CinemachineCamera>(runtimeRoot, sourceShot.Camera.name);
                targetCamera.Priority = 0;
                targetCamera.gameObject.SetActive(true);
                targetShots[i] = new IntroGatePodCinemachineShotPlayer.Shot(
                    sourceShot.ShotId,
                    sourceShot.StartSeconds + TimelineDurationSeconds,
                    targetCamera,
                    sourceShot.BlendStyle,
                    sourceShot.BlendSeconds);
                EditorUtility.SetDirty(targetCamera);
            }

            targetShotPlayer.Configure(brain, targetShots, false, true);
            targetShotPlayer.enabled = false;
            for (int i = 0; i < targetShots.Length; i++)
            {
                if (targetShots[i].Camera != null)
                {
                    targetShots[i].Camera.Priority = 0;
                    EditorUtility.SetDirty(targetShots[i].Camera);
                }
            }

            RebindShiftedCueDirector(sourceScene, runtimeRoot);
            RebindShiftedFirstPersonMask(sourceScene, runtimeRoot, visualRoot, director);
            RebindRunnerProfile(runtimeRoot, visualRoot, combinedProfile);
            RebindShiftedInvasionBridge(sourceScene, runtimeRoot, visualRoot, director);
            return targetShots;
        }

        private static void RebindShiftedAwakeningTimelineTracks(
            TimelineAsset timeline,
            PlayableDirector director,
            Transform runtimeRoot,
            Transform visualRoot,
            CinemachineBrain brain,
            IntroGatePodCinemachineShotPlayer.Shot[] shiftedShots)
        {
            CinemachineTrack cameraTrack = FindTimelineTrack<CinemachineTrack>(timeline, "Cinemachine Shots")
                ?? throw new InvalidOperationException("Combined Timeline is missing the shifted Cinemachine Shots track.");
            director.SetGenericBinding(cameraTrack, brain);
            BindCinemachineTimelineClips(director, cameraTrack, shiftedShots);

            AnimationTrack openingDollyTrack = FindTimelineTrack<AnimationTrack>(timeline, "Opening Dolly")
                ?? throw new InvalidOperationException("Combined Timeline is missing the shifted Opening Dolly track.");
            CinemachineSplineDolly openingDolly = ResolveOpeningDolly(shiftedShots);
            Animator openingAnimator = openingDolly.GetComponent<Animator>()
                ?? throw new InvalidOperationException("Shifted opening dolly camera is missing Animator.");
            director.SetGenericBinding(openingDollyTrack, openingAnimator);

            AnimationTrack inoriBodyTrack = FindTimelineTrack<AnimationTrack>(timeline, "Inori Body")
                ?? throw new InvalidOperationException("Combined Timeline is missing the shifted Inori Body track.");
            director.SetGenericBinding(inoriBodyTrack, RequireVisualInoriAnimator(visualRoot));

            AudioTrack voiceTrack = FindTimelineTrack<AudioTrack>(timeline, "Voice")
                ?? throw new InvalidOperationException("Combined Timeline is missing the shifted Voice track.");
            director.SetGenericBinding(
                voiceTrack,
                RequireComponentByObjectName<AudioSource>(runtimeRoot, "IntroGatePodReview_VoiceTimelineAudio"));

            AudioTrack bgmTrack = FindTimelineTrack<AudioTrack>(timeline, "BGM")
                ?? throw new InvalidOperationException("Combined Timeline is missing the shifted BGM track.");
            director.SetGenericBinding(
                bgmTrack,
                RequireComponentByObjectName<AudioSource>(runtimeRoot, "IntroGatePodReview_BgmTimelineAudio"));

            IntroGatePodFadeTrack fadeTrack = FindTimelineTrack<IntroGatePodFadeTrack>(timeline, "Fade")
                ?? throw new InvalidOperationException("Combined Timeline is missing the shifted Fade track.");
            IntroGatePodTimelineFadeOverlay fadeOverlay =
                RequireComponentByObjectName<IntroGatePodTimelineFadeOverlay>(
                    runtimeRoot,
                    "IntroGatePodReview_TimelineFadeOverlay");
            director.SetGenericBinding(fadeTrack, fadeOverlay);
            fadeOverlay.enabled = true;
            EditorUtility.SetDirty(fadeOverlay);
        }

        private static void AddBombingPreludeTracks(
            TimelineAsset timeline,
            PlayableDirector director,
            CinemachineBrain brain,
            BombingPreludeSceneBindings prelude)
        {
            CreateActivationTrack(timeline, director, "Bombing Prelude Root Active", prelude.Root, 0f, TimelineDurationSeconds);
            CreateCinemachineTrack(timeline, director, brain, prelude.Shots, "Bombing Prelude Cinemachine Shots", "bombing_prelude");
            CreateAnimationTrack(
                timeline,
                director,
                "Bombing Prelude Aircraft Formation Move",
                prelude.AircraftRoot.GetComponent<Animator>(),
                CreateFormationRootMoveClip("AC_OlympusBombingPrelude_AircraftFormationMove"));
            CreateAnimationTrack(
                timeline,
                director,
                "Bombing Prelude Bomber Lead Entry",
                RequireChildAnimator(prelude.AircraftRoot, "Bomber_Leader"),
                CreateAircraftJoinClip(
                    "AC_OlympusBombingPrelude_BomberLeadEntry",
                    0f,
                    BomberLeadEntryEndSeconds,
                    new Vector3(0f, -0.08f, -4.25f),
                    Vector3.zero,
                    -1.6f,
                    0.45f));
            CreateAnimationTrack(
                timeline,
                director,
                "Bombing Prelude Left Escort Join",
                RequireChildAnimator(prelude.AircraftRoot, "Jet_Escort_Left"),
                CreateAircraftJoinClip(
                    "AC_OlympusBombingPrelude_LeftEscortJoin",
                    LeftEscortJoinStartSeconds,
                    LeftEscortJoinEndSeconds,
                    new Vector3(-16.8f, -0.48f, -8.20f),
                    new Vector3(-FormationWingX, FormationWingY, FormationWingZ),
                    13.5f,
                    1.0f));
            CreateAnimationTrack(
                timeline,
                director,
                "Bombing Prelude Right Escort Join",
                RequireChildAnimator(prelude.AircraftRoot, "Jet_Escort_Right"),
                CreateAircraftJoinClip(
                    "AC_OlympusBombingPrelude_RightEscortJoin",
                    RightEscortJoinStartSeconds,
                    RightEscortJoinEndSeconds,
                    new Vector3(16.8f, -0.50f, -8.70f),
                    new Vector3(FormationWingX, FormationWingY, FormationWingZ),
                    -13.5f,
                    -1.0f));
            CreateAnimationTrack(
                timeline,
                director,
                "Bombing Prelude Bomb Drop Move",
                prelude.BombDropRoot.GetComponent<Animator>(),
                CreateBombDropClip("AC_OlympusBombingPrelude_BombDropMove"));
            CreateAnimationTrack(
                timeline,
                director,
                "Bombing Prelude Impact Camera Recoil",
                prelude.Shots[4].Animator,
                CreateRecoilClip(
                    "AC_OlympusBombingPrelude_ImpactCameraRecoil",
                    prelude.Shots[4].Camera.transform.localPosition,
                    6.24f));
            AddBombingTransitionCameraShakeTracks(
                timeline,
                director,
                prelude.Shots[5].Camera.transform,
                prelude.Shots[5].Animator,
                prelude.Shots[6].Camera.transform,
                prelude.Shots[6].Animator);
            CreateActivationTrack(timeline, director, "Bombing Prelude Cloud Deck Active", prelude.CloudRoot.gameObject, 0f, 5.48f);
            CreateActivationTrack(timeline, director, "Bombing Prelude Bombs Active", prelude.BombDropRoot.gameObject, BombReleaseStartSeconds, 2.42f);
            CreateActivationTrack(timeline, director, "Bombing Prelude Impact Chain Active", prelude.ImpactRoot.gameObject, 6.10f, 2.52f);
            CreateActivationTrack(timeline, director, "Bombing Prelude Impact Burst 01 Active", RequireChild(prelude.ImpactRoot, "ImpactBurst_01"), 6.22f, 1.72f);
            CreateActivationTrack(timeline, director, "Bombing Prelude Impact Burst 02 Active", RequireChild(prelude.ImpactRoot, "ImpactBurst_02"), 6.52f, 1.74f);
            CreateActivationTrack(timeline, director, "Bombing Prelude Impact Burst 03 Active", RequireChild(prelude.ImpactRoot, "ImpactBurst_03"), 6.82f, 1.76f);
            CreateActivationTrack(timeline, director, "Bombing Prelude Impact Burst 04 Active", RequireChild(prelude.ImpactRoot, "ImpactBurst_04"), 7.12f, 1.78f);
            CreateActivationTrack(timeline, director, "Bombing Prelude Aftermath Smoke Active", prelude.SmokeRoot.gameObject, 7.38f, 1.42f);
            CreateAnimationTrack(
                timeline,
                director,
                "Bombing Prelude Black Screen Transition",
                prelude.TransitionOverlay.CurtainAnimator,
                CreateCanvasGroupAlphaClip(
                    "AC_OlympusBombingPrelude_BlackScreenTransition",
                    (0.00f, 0.52f),
                    (0.08f, 0.28f),
                    (0.24f, 0.00f),
                    (8.18f, 0.00f),
                    (8.44f, 0.40f),
                    (8.80f, 0.90f)));
            EditorUtility.SetDirty(timeline);
        }

        private static void AddBombingTransitionCameraShakeTracks(
            TimelineAsset timeline,
            PlayableDirector director,
            Transform aftershockShot,
            Animator aftershockAnimator,
            Transform handoffShot,
            Animator handoffAnimator)
        {
            CreateAnimationTrack(
                timeline,
                director,
                AftershockTransitionShakeTrackName,
                aftershockAnimator,
                CreateTransitionCameraShakeClip(
                    AftershockTransitionShakeClipName,
                    aftershockShot.localPosition,
                    (7.88f, aftershockShot.localPosition),
                    (7.97f, aftershockShot.localPosition + new Vector3(0.18f, 0.10f, -0.09f)),
                    (8.07f, aftershockShot.localPosition + new Vector3(-0.13f, -0.07f, 0.06f)),
                    (8.19f, aftershockShot.localPosition + new Vector3(0.09f, 0.05f, -0.04f)),
                    (8.31f, aftershockShot.localPosition + new Vector3(-0.045f, -0.025f, 0.025f)),
                    (8.36f, aftershockShot.localPosition)));
            CreateAnimationTrack(
                timeline,
                director,
                HandoffTransitionShakeTrackName,
                handoffAnimator,
                CreateTransitionCameraShakeClip(
                    HandoffTransitionShakeClipName,
                    handoffShot.localPosition,
                    (8.36f, handoffShot.localPosition),
                    (8.42f, handoffShot.localPosition + new Vector3(-0.12f, 0.065f, -0.045f)),
                    (8.50f, handoffShot.localPosition + new Vector3(0.085f, -0.045f, 0.035f)),
                    (8.59f, handoffShot.localPosition + new Vector3(-0.055f, 0.028f, -0.022f)),
                    (8.70f, handoffShot.localPosition + new Vector3(0.024f, -0.012f, 0.010f)),
                    (TimelineDurationSeconds, handoffShot.localPosition)));
        }

        private static void RebindShiftedCueDirector(Scene sourceScene, Transform runtimeRoot)
        {
            IntroGatePodCutsceneCueDirector sourceCueDirector =
                FindComponentInScene<IntroGatePodCutsceneCueDirector>(sourceScene)
                ?? throw new InvalidOperationException("Missing source IntroGatePodCutsceneCueDirector.");
            IntroGatePodCutsceneCueDirector targetCueDirector =
                RequireComponentByObjectName<IntroGatePodCutsceneCueDirector>(
                    runtimeRoot,
                    "IntroGatePodReview_CueDirector");

            IntroGatePodCutsceneCueDirector.DollyCue[] sourceDollyCues = sourceCueDirector.DollyCues;
            IntroGatePodCutsceneCueDirector.DollyCue[] shiftedDollyCues =
                new IntroGatePodCutsceneCueDirector.DollyCue[sourceDollyCues.Length];
            for (int i = 0; i < sourceDollyCues.Length; i++)
            {
                IntroGatePodCutsceneCueDirector.DollyCue cue = sourceDollyCues[i];
                CinemachineSplineDolly targetDolly = cue.Dolly != null
                    ? RequireComponentByObjectName<CinemachineSplineDolly>(runtimeRoot, cue.Dolly.gameObject.name)
                    : null;
                shiftedDollyCues[i] = new IntroGatePodCutsceneCueDirector.DollyCue(
                    cue.CueId,
                    cue.StartSeconds + TimelineDurationSeconds,
                    cue.DurationSeconds,
                    targetDolly,
                    cue.FromPosition,
                    cue.ToPosition);
            }

            IntroGatePodCutsceneCueDirector.VoiceCue[] sourceVoiceCues = sourceCueDirector.VoiceCues;
            IntroGatePodCutsceneCueDirector.VoiceCue[] shiftedVoiceCues =
                new IntroGatePodCutsceneCueDirector.VoiceCue[sourceVoiceCues.Length];
            for (int i = 0; i < sourceVoiceCues.Length; i++)
            {
                IntroGatePodCutsceneCueDirector.VoiceCue cue = sourceVoiceCues[i];
                AudioSource targetSource = cue.AudioSource != null
                    ? RequireComponentByObjectName<AudioSource>(runtimeRoot, cue.AudioSource.name)
                    : null;
                shiftedVoiceCues[i] = new IntroGatePodCutsceneCueDirector.VoiceCue(
                    cue.CueId,
                    cue.StartSeconds + TimelineDurationSeconds,
                    targetSource);
            }

            IntroGatePodCutsceneCueDirector.FadeCue[] sourceFadeCues = sourceCueDirector.FadeCues;
            IntroGatePodCutsceneCueDirector.FadeCue[] shiftedFadeCues =
                new IntroGatePodCutsceneCueDirector.FadeCue[sourceFadeCues.Length];
            for (int i = 0; i < sourceFadeCues.Length; i++)
            {
                IntroGatePodCutsceneCueDirector.FadeCue cue = sourceFadeCues[i];
                shiftedFadeCues[i] = new IntroGatePodCutsceneCueDirector.FadeCue(
                    cue.CueId,
                    cue.StartSeconds + TimelineDurationSeconds,
                    cue.DurationSeconds,
                    cue.FromAlpha,
                    cue.ToAlpha);
            }

            targetCueDirector.Configure(shiftedDollyCues, shiftedVoiceCues, shiftedFadeCues, false, true);
            targetCueDirector.enabled = true;
            EditorUtility.SetDirty(targetCueDirector);
        }

        private static void RebindShiftedFirstPersonMask(
            Scene sourceScene,
            Transform runtimeRoot,
            Transform visualRoot,
            PlayableDirector director)
        {
            IntroGatePodFirstPersonRendererMask sourceMask =
                FindComponentInScene<IntroGatePodFirstPersonRendererMask>(sourceScene)
                ?? throw new InvalidOperationException("Missing source IntroGatePodFirstPersonRendererMask.");
            IntroGatePodFirstPersonRendererMask targetMask =
                RequireComponentByObjectName<IntroGatePodFirstPersonRendererMask>(
                    runtimeRoot,
                    "IntroGatePodReview_FirstPersonRendererMask");
            SerializedObject sourceSerialized = new SerializedObject(sourceMask);
            targetMask.Configure(
                director,
                ResolveFirstPersonHiddenRenderers(RequireVisualInori(visualRoot)),
                GetFloat(sourceSerialized, "hideStartSeconds") + TimelineDurationSeconds,
                GetFloat(sourceSerialized, "hideEndSeconds") + TimelineDurationSeconds);
            targetMask.enabled = true;
            EditorUtility.SetDirty(targetMask);
        }

        private static void RebindRunnerProfile(
            Transform runtimeRoot,
            Transform visualRoot,
            CinematicSequenceProfile combinedProfile)
        {
            CinematicSequenceRunner runner =
                RequireComponentByObjectName<CinematicSequenceRunner>(runtimeRoot, "IntroGatePodReview_Runner");
            SerializedObject serializedRunner = new SerializedObject(runner);
            RequireProperty(serializedRunner, "sequenceProfile").objectReferenceValue = combinedProfile;
            RequireProperty(serializedRunner, "cueSpace").objectReferenceValue = RequireVisualInori(visualRoot).transform;
            serializedRunner.ApplyModifiedPropertiesWithoutUndo();
            runner.enabled = true;
            EditorUtility.SetDirty(runner);
        }

        private static void RebindShiftedInvasionBridge(
            Scene sourceScene,
            Transform runtimeRoot,
            Transform visualRoot,
            PlayableDirector director)
        {
            IntroGatePodInvasionBridgeCue sourceBridge =
                FindComponentInScene<IntroGatePodInvasionBridgeCue>(sourceScene)
                ?? throw new InvalidOperationException("Missing source IntroGatePodInvasionBridgeCue.");
            IntroGatePodInvasionBridgeCue targetBridge =
                FindComponentByObjectName<IntroGatePodInvasionBridgeCue>(visualRoot, "IntroGatePodReview_InvasionBridge")
                ?? throw new InvalidOperationException("Missing target IntroGatePodReview_InvasionBridge.");

            IntroGatePodInvasionBridgeCue.CommandoCue[] sourceCommandos = sourceBridge.Commandos;
            IntroGatePodInvasionBridgeCue.CommandoCue[] shiftedCommandos =
                new IntroGatePodInvasionBridgeCue.CommandoCue[sourceCommandos.Length];
            for (int i = 0; i < sourceCommandos.Length; i++)
            {
                IntroGatePodInvasionBridgeCue.CommandoCue cue = sourceCommandos[i];
                Transform targetRoot = RequireDescendantOrSelf(visualRoot, cue.Root.name);
                Animator targetAnimator = targetRoot.GetComponentInChildren<Animator>(includeInactive: true);
                float shiftedStartSeconds = cue.StartSeconds + TimelineDurationSeconds;
                float shiftedEndSeconds = cue.EndSeconds + TimelineDurationSeconds;
                float extendedEndSeconds = Mathf.Max(shiftedEndSeconds, OlympusCommandoHoldUntilSeconds + (i * 0.04f));
                float shiftedAttackStartSeconds = string.IsNullOrWhiteSpace(cue.AttackStateName)
                    ? extendedEndSeconds
                    : cue.AttackStartSeconds + TimelineDurationSeconds;
                float shiftedHitStartSeconds = string.IsNullOrWhiteSpace(cue.HitStateName)
                    ? extendedEndSeconds
                    : cue.HitStartSeconds + TimelineDurationSeconds;
                Vector3 shiftedEndLocalPosition = string.IsNullOrWhiteSpace(cue.AttackStateName)
                    ? ExtrapolateCommandoEndLocalPosition(
                        cue.StartLocalPosition,
                        cue.EndLocalPosition,
                        shiftedStartSeconds,
                        shiftedEndSeconds,
                        extendedEndSeconds)
                    : cue.EndLocalPosition;
                shiftedCommandos[i] = new IntroGatePodInvasionBridgeCue.CommandoCue(
                    targetRoot,
                    targetAnimator,
                    cue.RunStateName,
                    cue.AttackStateName,
                    cue.HitStateName,
                    shiftedStartSeconds,
                    shiftedAttackStartSeconds,
                    shiftedHitStartSeconds,
                    extendedEndSeconds,
                    cue.StartLocalPosition,
                    shiftedEndLocalPosition,
                    cue.HitLocalPositionOffset,
                    cue.LocalEulerAngles,
                    cue.NormalizedTimeOffset);
            }

            IntroGatePodInvasionBridgeCue.TimedObjectCue[] sourceTimedObjects = sourceBridge.TimedObjects;
            IntroGatePodInvasionBridgeCue.TimedObjectCue[] shiftedTimedObjects =
                new IntroGatePodInvasionBridgeCue.TimedObjectCue[sourceTimedObjects.Length];
            for (int i = 0; i < sourceTimedObjects.Length; i++)
            {
                IntroGatePodInvasionBridgeCue.TimedObjectCue cue = sourceTimedObjects[i];
                Transform targetRoot = RequireDescendantOrSelf(visualRoot, cue.Root.name);
                shiftedTimedObjects[i] = new IntroGatePodInvasionBridgeCue.TimedObjectCue(
                    targetRoot,
                    cue.StartSeconds + TimelineDurationSeconds,
                    cue.EndSeconds + TimelineDurationSeconds,
                    cue.StartLocalPosition,
                    cue.EndLocalPosition,
                    cue.LocalEulerAngles,
                    cue.StartLocalScale,
                    cue.EndLocalScale,
                    cue.PulseScale,
                    cue.PulseScaleAmplitude);
            }

            SerializedObject sourceSerialized = new SerializedObject(sourceBridge);
            GameObject targetExplosionRoot = sourceBridge.ExplosionRoot != null
                ? RequireDescendantOrSelf(visualRoot, sourceBridge.ExplosionRoot.name).gameObject
                : null;
            Light targetExplosionLight = targetExplosionRoot != null
                ? targetExplosionRoot.GetComponentInChildren<Light>(includeInactive: true)
                : null;
            targetBridge.Configure(
                director,
                shiftedCommandos,
                targetExplosionRoot,
                targetExplosionLight,
                GetFloat(sourceSerialized, "explosionStartSeconds") + TimelineDurationSeconds,
                GetFloat(sourceSerialized, "explosionDurationSeconds"),
                GetVector3(sourceSerialized, "explosionRestScale"),
                GetVector3(sourceSerialized, "explosionPeakScale"),
                GetFloat(sourceSerialized, "explosionPeakLightIntensity"));
            targetBridge.ConfigureTimedObjects(
                shiftedTimedObjects,
                ReadShiftedFloatArray(sourceSerialized, "impactCueSeconds", TimelineDurationSeconds));
            targetBridge.ConfigurePresentation(
                RequireComponentByObjectName<Camera>(runtimeRoot, "Main Camera"),
                RequireComponentByObjectName<CanvasGroup>(runtimeRoot, "IntroGatePodReview_InvasionImpactFlash"),
                RequireComponentByObjectName<CanvasGroup>(runtimeRoot, "IntroGatePodReview_InvasionWarningSweep"),
                GetFloat(sourceSerialized, "explosionAfterSmokeSeconds"),
                GetFloat(sourceSerialized, "warningSweepLeadSeconds"),
                GetFloat(sourceSerialized, "warningSweepDurationSeconds"),
                GetFloat(sourceSerialized, "impactFlashPeakAlpha"),
                GetVector3(sourceSerialized, "cameraShakePositionAmplitude"),
                GetVector3(sourceSerialized, "cameraShakeEulerAmplitude"),
                GetFloat(sourceSerialized, "cameraShakeDurationSeconds"));
            targetBridge.enabled = true;
            targetBridge.Sample(0f);
            EditorUtility.SetDirty(targetBridge);
        }

        private static void BindCinemachineTimelineClips(
            PlayableDirector director,
            CinemachineTrack track,
            IntroGatePodCinemachineShotPlayer.Shot[] shots)
        {
            int clipIndex = 0;
            foreach (TimelineClip clip in track.GetClips())
            {
                CinemachineShot shotAsset = clip.asset as CinemachineShot;
                if (shotAsset == null)
                {
                    continue;
                }

                CinemachineCamera camera = FindShotCamera(shots, clip.displayName);
                if (camera == null && clipIndex < shots.Length)
                {
                    camera = shots[clipIndex].Camera;
                }

                if (camera == null)
                {
                    throw new InvalidOperationException(
                        $"Timeline Cinemachine clip `{clip.displayName}` has no target camera binding.");
                }

                PropertyName exposedName = shotAsset.VirtualCamera.exposedName;
                if (string.IsNullOrWhiteSpace(exposedName.ToString()))
                {
                    exposedName = new PropertyName($"cm_olympus_{clipIndex + 1:00}_{SanitizeAssetName(clip.displayName)}");
                    shotAsset.VirtualCamera.exposedName = exposedName;
                    EditorUtility.SetDirty(shotAsset);
                }

                director.SetReferenceValue(exposedName, camera);
                clipIndex++;
            }
        }

        private static Vector3 ExtrapolateCommandoEndLocalPosition(
            Vector3 startLocalPosition,
            Vector3 authoredEndLocalPosition,
            float startSeconds,
            float authoredEndSeconds,
            float extendedEndSeconds)
        {
            float authoredDuration = Mathf.Max(0.01f, authoredEndSeconds - startSeconds);
            float extendedDuration = Mathf.Max(authoredDuration, extendedEndSeconds - startSeconds);
            return startLocalPosition + ((authoredEndLocalPosition - startLocalPosition) * (extendedDuration / authoredDuration));
        }

        private static CinemachineCamera FindShotCamera(
            IntroGatePodCinemachineShotPlayer.Shot[] shots,
            string shotId)
        {
            for (int i = 0; i < shots.Length; i++)
            {
                if (string.Equals(shots[i].ShotId, shotId, StringComparison.Ordinal))
                {
                    return shots[i].Camera;
                }
            }

            return null;
        }

        private static CinemachineSplineDolly ResolveOpeningDolly(
            IntroGatePodCinemachineShotPlayer.Shot[] shots)
        {
            for (int i = 0; i < shots.Length; i++)
            {
                if (!string.Equals(shots[i].ShotId, "src_c01_capsule_left_dolly", StringComparison.Ordinal)
                    || shots[i].Camera == null)
                {
                    continue;
                }

                CinemachineSplineDolly dolly = shots[i].Camera.GetComponent<CinemachineSplineDolly>();
                if (dolly != null)
                {
                    return dolly;
                }
            }

            throw new InvalidOperationException("Shifted awakening opening shot is missing CinemachineSplineDolly.");
        }

        private static void UpdateOlympusStageDefinitionForBombingPrelude()
        {
            StageDefinitionProfile profile = LoadRequired<StageDefinitionProfile>(OlympusStageDefinitionPath);
            SerializedObject serialized = new SerializedObject(profile);
            SerializedProperty handoffs = RequireProperty(serialized, "cutsceneHandoffs");
            bool updated = false;
            for (int i = 0; i < handoffs.arraySize; i++)
            {
                SerializedProperty handoff = handoffs.GetArrayElementAtIndex(i);
                SerializedProperty handoffId = handoff.FindPropertyRelative("handoffId");
                if (handoffId == null || !string.Equals(handoffId.stringValue, "intro-to-stage", StringComparison.Ordinal))
                {
                    continue;
                }

                handoff.FindPropertyRelative("cinematicProfileId").stringValue =
                    Path.GetFileNameWithoutExtension(OlympusCombinedProfilePath);
                handoff.FindPropertyRelative("timelineAssetPath").stringValue = OlympusCombinedTimelinePath;
                updated = true;
            }

            if (!updated)
            {
                throw new InvalidOperationException("Stage definition is missing intro-to-stage handoff.");
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static List<string> ValidateOlympusBombingPreludeMerge(
            Scene stageScene,
            PlayableDirector director,
            TimelineAsset timeline,
            CinematicSequenceProfile profile,
            GameObject preludeRoot)
        {
            List<string> issues = new List<string>();
            if (director.playableAsset != timeline)
            {
                issues.Add("Olympus intro PlayableDirector is not bound to the bombing-prelude combined Timeline.");
            }

            if (profile == null || !string.Equals(
                    profile.SequenceId,
                    Path.GetFileNameWithoutExtension(OlympusCombinedProfilePath),
                    StringComparison.Ordinal))
            {
                issues.Add("Combined cinematic profile has the wrong sequence id.");
            }

            if (Math.Abs(timeline.fixedDuration - (LoadRequired<TimelineAsset>(AwakeningTimelinePath).fixedDuration + TimelineDurationSeconds)) > 0.01d)
            {
                issues.Add("Combined Timeline duration does not equal BombingReview duration plus protected awakening duration.");
            }

            RequireTrack<CinemachineTrack>(timeline, "Bombing Prelude Cinemachine Shots", issues);
            RequireTrack<ActivationTrack>(timeline, "Bombing Prelude Root Active", issues);
            if (FindTimelineTrack<ActivationTrack>(timeline, BombingPreludeTargetZoneTrackName) != null)
            {
                issues.Add("Bombing prelude still contains the removed target-plane activation track.");
            }

            CinemachineTrack shiftedCameraTrack = RequireTrack<CinemachineTrack>(timeline, "Cinemachine Shots", issues);
            if (shiftedCameraTrack != null)
            {
                foreach (TimelineClip clip in shiftedCameraTrack.GetClips())
                {
                    if (clip.start < TimelineDurationSeconds - 0.01d)
                    {
                        issues.Add($"Shifted awakening camera clip `{clip.displayName}` starts before the bombing prelude ends.");
                    }
                }
            }

            if (preludeRoot == null)
            {
                issues.Add("Missing Olympus bombing prelude scene root.");
            }
            else
            {
                if (preludeRoot.GetComponentInChildren<Camera>(includeInactive: true) != null)
                {
                    issues.Add("Bombing prelude must not add a second scene Camera; it should use the Olympus intro camera Brain.");
                }

                if (FindDescendantOrSelf(preludeRoot.transform, AircraftRootName) == null)
                {
                    issues.Add("Bombing prelude is missing aircraft formation root.");
                }

                if (FindDescendantOrSelf(preludeRoot.transform, BombDropRootName) == null)
                {
                    issues.Add("Bombing prelude is missing bomb drop root.");
                }

                if (FindDescendantContaining(preludeRoot.transform, "Distort") != null)
                {
                    issues.Add("Bombing prelude still contains a Distort object.");
                }

                if (FindDescendantOrSelf(preludeRoot.transform, TargetZoneName) != null
                    || FindDescendantOrSelf(preludeRoot.transform, TargetDeckName) != null)
                {
                    issues.Add("Bombing prelude still contains the removed target-plane object.");
                }

                ValidateProceduralExplosionMaterialsRemoved(preludeRoot.transform, issues);
                ValidateBombingTransitionCameraShake(timeline, preludeRoot.transform, issues);
            }

            StageDefinitionProfile stageProfile = LoadRequired<StageDefinitionProfile>(OlympusStageDefinitionPath);
            bool hasCombinedHandoff = false;
            for (int i = 0; i < stageProfile.CutsceneHandoffCount; i++)
            {
                StageDefinitionProfile.CutsceneHandoffRef handoff = stageProfile.GetCutsceneHandoff(i);
                if (string.Equals(handoff.HandoffId, "intro-to-stage", StringComparison.Ordinal))
                {
                    hasCombinedHandoff =
                        string.Equals(handoff.TimelineAssetPath, OlympusCombinedTimelinePath, StringComparison.Ordinal)
                        && string.Equals(
                            handoff.CinematicProfileId,
                            Path.GetFileNameWithoutExtension(OlympusCombinedProfilePath),
                            StringComparison.Ordinal);
                }
            }

            if (!hasCombinedHandoff)
            {
                issues.Add("Stage definition intro-to-stage handoff is not pointed at the combined bombing-prelude profile/timeline.");
            }

            ValidateShiftedRuntimeTiming(stageScene, issues);
            ValidateNoImportedDependencies(
                new[]
                {
                    OlympusCombinedTimelinePath,
                    OlympusCombinedProfilePath,
                    MaterialRoot,
                    MeshRoot,
                    ModelRoot,
                    TextureRoot,
                    VfxRoot
                },
                issues);
            ValidatePreludeCoordinates(director, preludeRoot, issues);
            return issues;
        }

        private static void ValidateShiftedRuntimeTiming(Scene stageScene, List<string> issues)
        {
            IntroGatePodFirstPersonRendererMask mask =
                FindComponentInScene<IntroGatePodFirstPersonRendererMask>(stageScene);
            if (mask != null)
            {
                SerializedObject serializedMask = new SerializedObject(mask);
                if (GetFloat(serializedMask, "hideStartSeconds") < TimelineDurationSeconds - 0.01f)
                {
                    issues.Add("First-person renderer mask still starts before the bombing prelude ends.");
                }
            }

            IntroGatePodInvasionBridgeCue bridge =
                FindComponentInScene<IntroGatePodInvasionBridgeCue>(stageScene);
            if (bridge != null)
            {
                SerializedObject serializedBridge = new SerializedObject(bridge);
                if (GetFloat(serializedBridge, "explosionStartSeconds") < TimelineDurationSeconds - 0.01f)
                {
                    issues.Add("Invasion bridge explosion still starts before the bombing prelude ends.");
                }
            }
        }

        private static void ValidatePreludeCoordinates(
            PlayableDirector director,
            GameObject preludeRoot,
            List<string> issues)
        {
            if (preludeRoot == null)
            {
                return;
            }

            director.time = 3.95d;
            director.Evaluate();
            Transform aircraftRoot = FindDescendantOrSelf(preludeRoot.transform, AircraftRootName);
            Transform bombRoot = FindDescendantOrSelf(preludeRoot.transform, BombDropRootName);
            Transform bomber = aircraftRoot != null ? aircraftRoot.Find("Bomber_Leader") : null;
            Transform left = aircraftRoot != null ? aircraftRoot.Find("Jet_Escort_Left") : null;
            Transform right = aircraftRoot != null ? aircraftRoot.Find("Jet_Escort_Right") : null;
            if (aircraftRoot == null || bombRoot == null || bomber == null || left == null || right == null)
            {
                issues.Add("Could not sample bombing prelude formation coordinates from the Olympus combined Timeline.");
                return;
            }

            float leftDistance = Vector3.Distance(left.localPosition, bomber.localPosition);
            float rightDistance = Vector3.Distance(right.localPosition, bomber.localPosition);
            if (leftDistance < MinimumAircraftSeparation || rightDistance < MinimumAircraftSeparation)
            {
                issues.Add("Bombing prelude formation spacing collapsed after Olympus merge.");
            }

            if (bombRoot.position.y >= aircraftRoot.position.y)
            {
                issues.Add("Bombing prelude bombs are not below the aircraft at release sample.");
            }

            director.time = TimelineDurationSeconds + 0.05d;
            director.Evaluate();
            if (preludeRoot.activeSelf)
            {
                issues.Add("Bombing prelude root remains active after the protected awakening timeline starts.");
            }

            director.time = 0d;
            director.Evaluate();
        }

        private static void WriteOlympusMergeReport(
            List<string> issues,
            TimelineAsset timeline,
            CinematicSequenceProfile profile)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# Intro GatePod Olympus Bombing Prelude Merge");
            builder.AppendLine();
            builder.AppendLine(issues.Count == 0 ? "Status: PASS" : "Status: FAIL");
            builder.AppendLine();
            builder.AppendLine($"- Stage scene: `{OlympusStageScenePath}`");
            builder.AppendLine($"- Combined Timeline: `{OlympusCombinedTimelinePath}`");
            builder.AppendLine($"- Combined profile: `{OlympusCombinedProfilePath}`");
            builder.AppendLine($"- Prepended BombingReview duration: `{TimelineDurationSeconds:0.00}s`");
            builder.AppendLine($"- Combined Timeline duration: `{timeline.fixedDuration:0.###}s`");
            builder.AppendLine($"- Combined profile estimated duration: `{profile.EstimatedDurationSeconds:0.###}s`");
            builder.AppendLine("- Existing GatePod awakening clips are shifted as a block after the bombing prelude.");
            builder.AppendLine("- Captures were intentionally not rendered; this pass validates Timeline bindings and transform coordinates.");
            if (issues.Count > 0)
            {
                builder.AppendLine();
                builder.AppendLine("## Issues");
                for (int i = 0; i < issues.Count; i++)
                {
                    builder.AppendLine($"- {issues[i]}");
                }
            }

            Directory.CreateDirectory(Path.GetDirectoryName(OlympusMergeReportPath) ?? "C:/tmp");
            File.WriteAllText(OlympusMergeReportPath, builder.ToString(), Encoding.UTF8);
        }

        private static void RemoveProceduralExplosionObjects(Transform root)
        {
            DestroyDescendantsMatching(
                root,
                transform =>
                    transform.name.StartsWith(ProceduralFireCorePrefix, StringComparison.Ordinal)
                    || string.Equals(transform.name, ShockRingName, StringComparison.Ordinal)
                    || transform.name.StartsWith(AftermathScorchPatchPrefix, StringComparison.Ordinal));
        }

        private static int DestroyDescendantsMatching(
            Transform root,
            Predicate<Transform> predicate)
        {
            List<GameObject> matches = new List<GameObject>();
            CollectDescendantsMatching(root, predicate, matches);
            for (int i = 0; i < matches.Count; i++)
            {
                UnityEngine.Object.DestroyImmediate(matches[i]);
            }

            return matches.Count;
        }

        private static void CollectDescendantsMatching(
            Transform current,
            Predicate<Transform> predicate,
            List<GameObject> matches)
        {
            for (int i = 0; i < current.childCount; i++)
            {
                Transform child = current.GetChild(i);
                if (predicate(child))
                {
                    matches.Add(child.gameObject);
                    continue;
                }

                CollectDescendantsMatching(child, predicate, matches);
            }
        }

        private static void DeleteAssetIfExists(string assetPath)
        {
            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            if (asset == null)
            {
                return;
            }

            if (!AssetDatabase.DeleteAsset(assetPath))
            {
                throw new InvalidOperationException($"Failed to delete generated asset `{assetPath}`.");
            }
        }

        private static void ValidateProceduralExplosionMaterialsRemoved(
            Transform preludeRoot,
            List<string> issues)
        {
            if (FindDescendantContaining(preludeRoot, ProceduralFireCorePrefix) != null)
            {
                issues.Add("Bombing prelude still contains procedural fire-core geometry.");
            }

            if (FindDescendantOrSelf(preludeRoot, ShockRingName) != null)
            {
                issues.Add("Bombing prelude still contains procedural shock-ring geometry.");
            }

            if (FindDescendantContaining(preludeRoot, AftermathScorchPatchPrefix) != null)
            {
                issues.Add("Bombing prelude still contains procedural scorch-patch geometry.");
            }

            string[] deletedAssetPaths =
            {
                ProceduralExplosionCoreMaterialPath,
                ProceduralShockRingMaterialPath,
                ProceduralSmokeMaterialPath,
                ShockRingMeshPath
            };
            for (int i = 0; i < deletedAssetPaths.Length; i++)
            {
                if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(deletedAssetPaths[i]) != null)
                {
                    issues.Add($"Generated procedural explosion asset still exists: `{deletedAssetPaths[i]}`.");
                }
            }
        }

        private static void ValidateBombingTransitionCameraShake(
            TimelineAsset timeline,
            Transform preludeRoot,
            List<string> issues)
        {
            RequireTrack<AnimationTrack>(timeline, AftershockTransitionShakeTrackName, issues);
            RequireTrack<AnimationTrack>(timeline, HandoffTransitionShakeTrackName, issues);
            Transform aftershockShot = FindDescendantOrSelf(preludeRoot, AftershockShotName);
            Transform handoffShot = FindDescendantOrSelf(preludeRoot, HandoffShotName);
            if (aftershockShot == null)
            {
                issues.Add($"Bombing transition camera shake is missing `{AftershockShotName}`.");
            }
            else if (aftershockShot.GetComponent<Animator>() == null)
            {
                issues.Add($"Bombing transition camera shake target `{AftershockShotName}` has no Animator.");
            }

            if (handoffShot == null)
            {
                issues.Add($"Bombing transition camera shake is missing `{HandoffShotName}`.");
            }
            else if (handoffShot.GetComponent<Animator>() == null)
            {
                issues.Add($"Bombing transition camera shake target `{HandoffShotName}` has no Animator.");
            }
        }

        private static void AddOffsetToCueStartTimes(SerializedProperty array, float offsetSeconds)
        {
            for (int i = 0; i < array.arraySize; i++)
            {
                SerializedProperty start = array.GetArrayElementAtIndex(i).FindPropertyRelative("startSeconds");
                if (start != null)
                {
                    start.floatValue += offsetSeconds;
                }
            }
        }

        private static void ShiftTimelineClips(TimelineAsset timeline, float offsetSeconds)
        {
            foreach (TrackAsset track in timeline.GetOutputTracks())
            {
                if (track == null)
                {
                    continue;
                }

                foreach (TimelineClip clip in track.GetClips())
                {
                    clip.start += offsetSeconds;
                }

                EditorUtility.SetDirty(track);
            }
        }

        private static bool RemoveTimelineTrack(
            TimelineAsset timeline,
            string trackName,
            PlayableDirector director)
        {
            List<TrackAsset> matches = new List<TrackAsset>();
            foreach (TrackAsset track in timeline.GetOutputTracks())
            {
                if (track != null && string.Equals(track.name, trackName, StringComparison.Ordinal))
                {
                    matches.Add(track);
                }
            }

            for (int i = 0; i < matches.Count; i++)
            {
                if (director != null)
                {
                    director.ClearGenericBinding(matches[i]);
                }

                timeline.DeleteTrack(matches[i]);
            }

            if (matches.Count > 0)
            {
                EditorUtility.SetDirty(timeline);
            }

            return matches.Count > 0;
        }

        private static float[] ReadShiftedFloatArray(
            SerializedObject serializedObject,
            string propertyName,
            float offsetSeconds)
        {
            SerializedProperty property = RequireProperty(serializedObject, propertyName);
            float[] values = new float[property.arraySize];
            for (int i = 0; i < property.arraySize; i++)
            {
                values[i] = property.GetArrayElementAtIndex(i).floatValue + offsetSeconds;
            }

            return values;
        }

        private static void ValidateNoImportedDependencies(string[] assetPaths, List<string> issues)
        {
            foreach (string dependency in AssetDatabase.GetDependencies(assetPaths, recursive: true))
            {
                string normalized = dependency.Replace('\\', '/');
                if (normalized.Contains("/_Imported/", StringComparison.Ordinal))
                {
                    issues.Add($"Combined Olympus bombing-prelude dependency points at raw imported asset: {dependency}");
                }
            }
        }

        private static T LoadRequired<T>(string assetPath)
            where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset == null)
            {
                throw new InvalidOperationException($"Missing required asset at `{assetPath}`.");
            }

            return asset;
        }

        private static SerializedProperty RequireProperty(SerializedObject serializedObject, string propertyName)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                throw new InvalidOperationException(
                    $"{serializedObject.targetObject.GetType().Name} is missing serialized property `{propertyName}`.");
            }

            return property;
        }

        private static float GetFloat(SerializedObject serializedObject, string propertyName)
        {
            return RequireProperty(serializedObject, propertyName).floatValue;
        }

        private static Vector3 GetVector3(SerializedObject serializedObject, string propertyName)
        {
            return RequireProperty(serializedObject, propertyName).vector3Value;
        }

        private static GameObject RequireObjectInScene(Scene scene, string objectName)
        {
            GameObject gameObject = FindObjectInScene(scene, objectName);
            if (gameObject == null)
            {
                throw new InvalidOperationException($"Scene `{scene.name}` is missing `{objectName}`.");
            }

            return gameObject;
        }

        private static T RequireComponentByObjectName<T>(Transform root, string objectName)
            where T : Component
        {
            T component = FindComponentByObjectName<T>(root, objectName);
            if (component == null)
            {
                throw new InvalidOperationException($"{root.name} is missing {typeof(T).Name} on `{objectName}`.");
            }

            return component;
        }

        private static T FindComponentByObjectName<T>(Transform root, string objectName)
            where T : Component
        {
            Transform target = FindDescendantOrSelf(root, objectName);
            return target != null ? target.GetComponent<T>() : null;
        }

        private static Transform RequireDescendantOrSelf(Transform root, string objectName)
        {
            Transform target = FindDescendantOrSelf(root, objectName);
            if (target == null)
            {
                throw new InvalidOperationException($"{root.name} is missing descendant `{objectName}`.");
            }

            return target;
        }

        private static Transform FindDescendantOrSelf(Transform root, string objectName)
        {
            if (root == null)
            {
                return null;
            }

            if (string.Equals(root.name, objectName, StringComparison.Ordinal))
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindDescendantOrSelf(root.GetChild(i), objectName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static Transform FindDescendantContaining(Transform root, string nameFragment)
        {
            if (root == null || string.IsNullOrWhiteSpace(nameFragment))
            {
                return null;
            }

            if (root.name.IndexOf(nameFragment, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindDescendantContaining(root.GetChild(i), nameFragment);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static GameObject RequireVisualInori(Transform visualRoot)
        {
            return RequireDescendantOrSelf(visualRoot, "IntroGatePodReview_Inori").gameObject;
        }

        private static Animator RequireVisualInoriAnimator(Transform visualRoot)
        {
            Animator animator = RequireVisualInori(visualRoot).GetComponentInChildren<Animator>(includeInactive: true);
            if (animator == null)
            {
                throw new InvalidOperationException("Copied Olympus intro Inori visual is missing an Animator.");
            }

            return animator;
        }

        private static Renderer[] ResolveFirstPersonHiddenRenderers(GameObject inori)
        {
            Renderer[] renderers = inori.GetComponentsInChildren<Renderer>(includeInactive: true);
            List<Renderer> hidden = new List<Renderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                for (Transform current = renderer.transform; current != null; current = current.parent)
                {
                    string name = current.name;
                    if (name.IndexOf("Hair", StringComparison.OrdinalIgnoreCase) >= 0
                        || name.IndexOf("Head", StringComparison.OrdinalIgnoreCase) >= 0
                        || name.IndexOf("Face", StringComparison.OrdinalIgnoreCase) >= 0
                        || name.IndexOf("Eye", StringComparison.OrdinalIgnoreCase) >= 0
                        || name.IndexOf("Brow", StringComparison.OrdinalIgnoreCase) >= 0
                        || name.IndexOf("Mouth", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        hidden.Add(renderer);
                        break;
                    }

                    if (string.Equals(current.name, "IntroGatePodReview_Inori", StringComparison.Ordinal))
                    {
                        break;
                    }
                }
            }

            return hidden.ToArray();
        }

        private static void PromoteSourceAssets()
        {
            CopyAssetIfMissing(SourceJetModelPath, JetModelPath);
            CopyAssetIfMissing(SourceBomberModelPath, BomberModelPath);
            CopyAssetIfMissing(SourceBombModelPath, BombModelPath);
            CopyAssetIfMissing(SourceAircraftTexturePath, AircraftTexturePath);
            PromoteModelMaterial(
                SourceBomberMaterialPath,
                BomberMaterialPath,
                SourceSciFiBomberBaseTexturePath,
                new Color(0.78f, 0.82f, 0.78f, 1f));
            PromoteModelMaterial(
                SourceJetMaterialPath,
                JetMaterialPath,
                SourceSciFiJetBaseTexturePath,
                new Color(0.48f, 0.56f, 0.64f, 1f));
            PromoteModelMaterial(
                SourceBombMaterialPath,
                BombMaterialPath,
                SourceSciFiBombBaseTexturePath,
                new Color(0.42f, 0.42f, 0.38f, 1f));
            PromoteParticleEffectPrefab(
                SourceVefectsExplosion03PrefabPath,
                VefectsExplosion03PrefabPath,
                "VefectsExplosion03");
            PromoteParticleEffectPrefab(
                SourceVefectsExplosionFire02PrefabPath,
                VefectsExplosionFire02PrefabPath,
                "VefectsExplosionFire02");
            PromoteParticleEffectPrefab(
                SourceAirstrikeBombExplosionPrefabPath,
                AirstrikeBombExplosionPrefabPath,
                "AirstrikeBombExplosion",
                removeNameFragment: "Distort");
            PromoteParticleEffectPrefab(
                SourceAirstrikeBombTrailPrefabPath,
                AirstrikeBombTrailPrefabPath,
                "AirstrikeBombTrail");
            PromoteParticleEffectPrefab(
                SourceShellExplosionPrefabPath,
                ShellExplosionPrefabPath,
                "ShellExplosion");
            PromoteParticleEffectPrefab(
                SourceDavfxExplosion09PrefabPath,
                DavfxExplosion09PrefabPath,
                "DavfxExplosion09");
            PromoteParticleEffectPrefab(
                SourceDavfxExplosion20PrefabPath,
                DavfxExplosion20PrefabPath,
                "DavfxExplosion20");
            PromoteParticleEffectPrefab(
                SourceCyberBomberPrefabPath,
                CyberBomberPrefabPath,
                "CyberBomber",
                stripCustomMonoBehaviours: true);
            PromoteParticleEffectPrefab(
                SourceCyberBomber2PrefabPath,
                CyberBomber2PrefabPath,
                "CyberBomber2",
                stripCustomMonoBehaviours: true);
            PromoteParticleEffectPrefab(
                SourceCyberBombPrefabPath,
                CyberBombPrefabPath,
                "CyberBomb",
                stripCustomMonoBehaviours: true);
            PromoteParticleEffectPrefab(
                SourceAerialBombPrefabPath,
                AerialBombPrefabPath,
                "AerialBomb",
                stripCustomMonoBehaviours: true,
                removeNameFragment: "Effect_40_Bomb");
            PromoteParticleEffectPrefab(
                SourceAerialBomb2PrefabPath,
                AerialBomb2PrefabPath,
                "AerialBomb2",
                stripCustomMonoBehaviours: true,
                removeNameFragment: "Effect_40_Bomb");
            PromoteParticleEffectPrefab(
                SourceAerialBomb3PrefabPath,
                AerialBomb3PrefabPath,
                "AerialBomb3",
                stripCustomMonoBehaviours: true,
                removeNameFragment: "Effect_40_Bomb");
            WriteExplosionAssetAudit();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void CopyAssetIfMissing(string sourcePath, string targetPath)
        {
            EnsureFolder(PathParent(targetPath));
            if (AssetDatabase.LoadMainAssetAtPath(targetPath) != null)
            {
                return;
            }

            if (AssetDatabase.LoadMainAssetAtPath(sourcePath) == null)
            {
                throw new InvalidOperationException($"Missing source asset `{sourcePath}`.");
            }

            if (!AssetDatabase.CopyAsset(sourcePath, targetPath))
            {
                throw new InvalidOperationException($"Failed to copy `{sourcePath}` to `{targetPath}`.");
            }

            AssetDatabase.ImportAsset(targetPath, ImportAssetOptions.ForceUpdate);
        }

        private static Material PromoteModelMaterial(
            string sourceMaterialPath,
            string targetMaterialPath,
            string fallbackTexturePath,
            Color fallbackColor)
        {
            EnsureFolder(PathParent(targetMaterialPath));
            Material sourceMaterial = AssetDatabase.LoadAssetAtPath<Material>(sourceMaterialPath);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("Standard");
            Material material = AssetDatabase.LoadAssetAtPath<Material>(targetMaterialPath);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, targetMaterialPath);
            }
            else
            {
                material.shader = shader;
            }

            Color baseColor = sourceMaterial != null ? ReadMaterialColor(sourceMaterial) : fallbackColor;
            if (baseColor.maxColorComponent <= 0.001f)
            {
                baseColor = fallbackColor;
            }
            if (baseColor.r > 0.98f
                && baseColor.g > 0.98f
                && baseColor.b > 0.98f
                && fallbackColor.maxColorComponent < 0.98f)
            {
                baseColor = fallbackColor;
            }

            SetMaterialBase(material, baseColor, Color.black);
            Texture texture = null;
            if (!string.IsNullOrWhiteSpace(fallbackTexturePath))
            {
                texture = AssetDatabase.LoadAssetAtPath<Texture>(fallbackTexturePath);
            }
            if (texture == null && sourceMaterial != null)
            {
                texture = ReadFirstMaterialTexture(sourceMaterial);
            }
            if (texture != null)
            {
                Texture promotedTexture = PromoteTextureAsset(texture, "AircraftModels");
                if (material.HasProperty("_BaseMap"))
                {
                    material.SetTexture("_BaseMap", promotedTexture);
                }
                if (material.HasProperty("_MainTex"))
                {
                    material.SetTexture("_MainTex", promotedTexture);
                }
            }

            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", 0.46f);
            }
            if (material.HasProperty("_Metallic"))
            {
                material.SetFloat("_Metallic", 0.22f);
            }

            material.enableInstancing = true;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Texture ReadFirstMaterialTexture(Material material)
        {
            string[] textureProperties = material.GetTexturePropertyNames();
            for (int i = 0; i < textureProperties.Length; i++)
            {
                Texture texture = material.GetTexture(textureProperties[i]);
                if (texture != null)
                {
                    return texture;
                }
            }

            return null;
        }

        private static void PromoteParticleEffectPrefab(
            string sourcePrefabPath,
            string targetPrefabPath,
            string materialGroup,
            bool stripCustomMonoBehaviours = false,
            string removeNameFragment = null)
        {
            EnsureFolder(PathParent(targetPrefabPath));
            GameObject sourcePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(sourcePrefabPath);
            if (sourcePrefab == null)
            {
                throw new InvalidOperationException($"Missing source particle prefab `{sourcePrefabPath}`.");
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(sourcePrefab) as GameObject;
            if (instance == null)
            {
                throw new InvalidOperationException($"Failed to instantiate source particle prefab `{sourcePrefabPath}`.");
            }

            try
            {
                PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                instance.name = Path.GetFileNameWithoutExtension(targetPrefabPath);
                if (stripCustomMonoBehaviours)
                {
                    RemoveCustomMonoBehaviours(instance);
                }
                else
                {
                    RequireNoCustomMonoBehaviours(instance, sourcePrefabPath);
                }
                RemoveObjectsContainingName(instance, removeNameFragment);
                RemoveAudioSources(instance);
                NormalizeParticleSystems(instance);
                PromoteParticleRenderers(instance, materialGroup);
                RemoveColliders(instance);

                if (AssetDatabase.LoadAssetAtPath<GameObject>(targetPrefabPath) != null)
                {
                    AssetDatabase.DeleteAsset(targetPrefabPath);
                }

                GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(instance, targetPrefabPath);
                if (savedPrefab == null)
                {
                    throw new InvalidOperationException($"Failed to save promoted particle prefab `{targetPrefabPath}`.");
                }

                AssetDatabase.ImportAsset(targetPrefabPath, ImportAssetOptions.ForceUpdate);
                AssetDatabase.SaveAssets();
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }

            AssertNoImportedDependencies(targetPrefabPath);
        }

        private static void RequireNoCustomMonoBehaviours(GameObject instance, string sourcePrefabPath)
        {
            MonoBehaviour[] behaviours = instance.GetComponentsInChildren<MonoBehaviour>(includeInactive: true);
            if (behaviours.Length == 0)
            {
                return;
            }

            List<string> behaviourNames = new List<string>();
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] != null)
                {
                    behaviourNames.Add(behaviours[i].GetType().Name);
                }
            }

            throw new InvalidOperationException(
                $"Source particle prefab `{sourcePrefabPath}` contains custom MonoBehaviours: "
                + string.Join(", ", behaviourNames));
        }

        private static void RemoveCustomMonoBehaviours(GameObject instance)
        {
            MonoBehaviour[] behaviours = instance.GetComponentsInChildren<MonoBehaviour>(includeInactive: true);
            for (int i = behaviours.Length - 1; i >= 0; i--)
            {
                if (behaviours[i] != null)
                {
                    UnityEngine.Object.DestroyImmediate(behaviours[i]);
                }
            }
        }

        private static void RemoveObjectsContainingName(GameObject instance, string nameFragment)
        {
            if (string.IsNullOrWhiteSpace(nameFragment))
            {
                return;
            }

            Transform[] transforms = instance.GetComponentsInChildren<Transform>(includeInactive: true);
            for (int i = transforms.Length - 1; i >= 0; i--)
            {
                Transform current = transforms[i];
                if (current == null
                    || current == instance.transform
                    || current.name.IndexOf(nameFragment, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                UnityEngine.Object.DestroyImmediate(current.gameObject);
            }
        }

        private static void RemoveAudioSources(GameObject instance)
        {
            AudioSource[] audioSources = instance.GetComponentsInChildren<AudioSource>(includeInactive: true);
            for (int i = audioSources.Length - 1; i >= 0; i--)
            {
                UnityEngine.Object.DestroyImmediate(audioSources[i]);
            }
        }

        private static void NormalizeParticleSystems(GameObject instance)
        {
            ParticleSystem[] particleSystems = instance.GetComponentsInChildren<ParticleSystem>(includeInactive: true);
            for (int i = 0; i < particleSystems.Length; i++)
            {
                ParticleSystem particleSystem = particleSystems[i];
                ParticleSystem.LightsModule lights = particleSystem.lights;
                lights.enabled = false;
                lights.light = null;
                ParticleSystem.MainModule main = particleSystem.main;
                main.playOnAwake = true;
                EditorUtility.SetDirty(particleSystem);
            }
        }

        private static void PromoteParticleRenderers(GameObject instance, string materialGroup)
        {
            ParticleSystemRenderer[] renderers =
                instance.GetComponentsInChildren<ParticleSystemRenderer>(includeInactive: true);
            if (renderers.Length == 0)
            {
                throw new InvalidOperationException($"Promoted particle prefab `{instance.name}` has no particle renderers.");
            }

            for (int i = 0; i < renderers.Length; i++)
            {
                ParticleSystemRenderer renderer = renderers[i];
                Material[] materials = renderer.sharedMaterials;
                for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                {
                    materials[materialIndex] = PromoteParticleMaterial(materials[materialIndex], materialGroup);
                }

                renderer.sharedMaterials = materials;
                Mesh mesh = renderer.mesh;
                if (mesh != null)
                {
                    renderer.mesh = PromoteParticleMesh(mesh, materialGroup);
                }

                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                renderer.allowOcclusionWhenDynamic = false;
                EditorUtility.SetDirty(renderer);
            }
        }

        private static Material PromoteParticleMaterial(Material sourceMaterial, string materialGroup)
        {
            if (sourceMaterial == null)
            {
                return LoadOrCreateUnlitMaterial(
                    $"{VfxMaterialRoot}/{materialGroup}/M_{materialGroup}_Fallback.mat",
                    Color.white,
                    Color.black,
                    transparent: true);
            }

            string materialName = SanitizeAssetName(sourceMaterial.name);
            string targetPath = $"{VfxMaterialRoot}/{materialGroup}/{materialName}.mat";
            EnsureFolder(PathParent(targetPath));

            Shader shader = ResolveParticleShader();
            Material material = AssetDatabase.LoadAssetAtPath<Material>(targetPath);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, targetPath);
            }
            else
            {
                material.shader = shader;
            }

            Color baseColor = ReadMaterialColor(sourceMaterial);
            Color emission = ReadMaterialEmission(sourceMaterial, baseColor);
            SetMaterialBase(material, baseColor, emission);

            Texture firstTexture = null;
            string firstTextureProperty = null;
            string[] textureProperties = sourceMaterial.GetTexturePropertyNames();
            for (int i = 0; i < textureProperties.Length; i++)
            {
                string propertyName = textureProperties[i];
                Texture sourceTexture = sourceMaterial.GetTexture(propertyName);
                if (sourceTexture == null)
                {
                    continue;
                }

                Texture texture = PromoteTextureAsset(sourceTexture, materialGroup);
                if (firstTexture == null)
                {
                    firstTexture = texture;
                    firstTextureProperty = propertyName;
                }

                if (material.HasProperty(propertyName))
                {
                    material.SetTexture(propertyName, texture);
                    CopyTextureScaleAndOffset(sourceMaterial, material, propertyName, propertyName);
                }
            }

            if (firstTexture != null)
            {
                if (material.HasProperty("_BaseMap"))
                {
                    material.SetTexture("_BaseMap", firstTexture);
                    CopyTextureScaleAndOffset(sourceMaterial, material, firstTextureProperty, "_BaseMap");
                }
                if (material.HasProperty("_MainTex"))
                {
                    material.SetTexture("_MainTex", firstTexture);
                    CopyTextureScaleAndOffset(sourceMaterial, material, firstTextureProperty, "_MainTex");
                }
                if (LooksHot(sourceMaterial.name) && material.HasProperty("_EmissionMap"))
                {
                    material.SetTexture("_EmissionMap", firstTexture);
                }
            }

            ConfigureParticleMaterial(material, WantsAdditiveBlend(sourceMaterial));
            material.enableInstancing = true;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Texture PromoteTextureAsset(Texture sourceTexture, string materialGroup)
        {
            string sourcePath = AssetDatabase.GetAssetPath(sourceTexture);
            if (string.IsNullOrWhiteSpace(sourcePath))
            {
                return sourceTexture;
            }

            string normalized = sourcePath.Replace('\\', '/');
            if (!normalized.Contains("/_Imported/", StringComparison.Ordinal))
            {
                return sourceTexture;
            }

            string extension = Path.GetExtension(normalized);
            if (string.IsNullOrWhiteSpace(extension))
            {
                extension = ".asset";
            }

            string guid = AssetDatabase.AssetPathToGUID(sourcePath);
            string suffix = string.IsNullOrEmpty(guid) ? "copied" : guid.Substring(0, Mathf.Min(8, guid.Length));
            string targetPath = $"{VfxTextureRoot}/{materialGroup}/{SanitizeAssetName(sourceTexture.name)}_{suffix}{extension}";
            CopyAssetIfMissing(sourcePath, targetPath);
            Texture promotedTexture = AssetDatabase.LoadAssetAtPath<Texture>(targetPath);
            return promotedTexture != null ? promotedTexture : sourceTexture;
        }

        private static Mesh PromoteParticleMesh(Mesh sourceMesh, string materialGroup)
        {
            string sourcePath = AssetDatabase.GetAssetPath(sourceMesh);
            if (string.IsNullOrWhiteSpace(sourcePath))
            {
                return sourceMesh;
            }

            string normalized = sourcePath.Replace('\\', '/');
            if (!normalized.Contains("/_Imported/", StringComparison.Ordinal))
            {
                return sourceMesh;
            }

            string extension = Path.GetExtension(normalized);
            if (string.IsNullOrWhiteSpace(extension))
            {
                extension = ".asset";
            }

            string guid = AssetDatabase.AssetPathToGUID(sourcePath);
            string suffix = string.IsNullOrEmpty(guid) ? "copied" : guid.Substring(0, Mathf.Min(8, guid.Length));
            string targetPath = $"{VfxMeshRoot}/{materialGroup}/{SanitizeAssetName(sourceMesh.name)}_{suffix}{extension}";
            CopyAssetIfMissing(sourcePath, targetPath);

            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(targetPath);
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is Mesh mesh && string.Equals(mesh.name, sourceMesh.name, StringComparison.Ordinal))
                {
                    return mesh;
                }
            }

            Mesh promotedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(targetPath);
            return promotedMesh != null ? promotedMesh : sourceMesh;
        }

        private static Shader ResolveParticleShader()
        {
            return Shader.Find("Universal Render Pipeline/Particles/Unlit")
                ?? Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Particles/Standard Unlit")
                ?? Shader.Find("Unlit/Transparent")
                ?? Shader.Find("Standard");
        }

        private static Color ReadMaterialColor(Material material)
        {
            if (material.HasProperty("_BaseColor"))
            {
                return material.GetColor("_BaseColor");
            }
            if (material.HasProperty("_Color"))
            {
                return material.GetColor("_Color");
            }
            if (material.HasProperty("_TintColor"))
            {
                return material.GetColor("_TintColor");
            }

            return Color.white;
        }

        private static Color ReadMaterialEmission(Material material, Color baseColor)
        {
            Color emission = Color.black;
            if (material.HasProperty("_EmissionColor"))
            {
                emission = material.GetColor("_EmissionColor");
            }

            if (LooksHot(material.name) && emission.maxColorComponent < 0.5f)
            {
                emission = baseColor * 2.7f;
                emission.a = 1f;
            }

            return emission;
        }

        private static bool LooksHot(string materialName)
        {
            string lower = materialName.ToLowerInvariant();
            return lower.Contains("fire")
                || lower.Contains("spark")
                || lower.Contains("glow")
                || lower.Contains("explosion")
                || lower.Contains("leash");
        }

        private static bool WantsAdditiveBlend(Material material)
        {
            string lower = material.name.ToLowerInvariant();
            if (lower.Contains("smoke") || lower.Contains("dark"))
            {
                return false;
            }

            return LooksHot(material.name);
        }

        private static void CopyTextureScaleAndOffset(
            Material sourceMaterial,
            Material targetMaterial,
            string sourceProperty,
            string targetProperty)
        {
            if (string.IsNullOrEmpty(sourceProperty)
                || !sourceMaterial.HasProperty(sourceProperty)
                || !targetMaterial.HasProperty(targetProperty))
            {
                return;
            }

            targetMaterial.SetTextureScale(targetProperty, sourceMaterial.GetTextureScale(sourceProperty));
            targetMaterial.SetTextureOffset(targetProperty, sourceMaterial.GetTextureOffset(sourceProperty));
        }

        private static void ConfigureParticleMaterial(Material material, bool additive)
        {
            if (material.HasProperty("_Surface"))
            {
                material.SetFloat("_Surface", 1f);
            }
            if (material.HasProperty("_Blend"))
            {
                material.SetFloat("_Blend", additive ? 2f : 0f);
            }
            if (material.HasProperty("_AlphaClip"))
            {
                material.SetFloat("_AlphaClip", 0f);
            }
            if (material.HasProperty("_SrcBlend"))
            {
                material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            }
            if (material.HasProperty("_DstBlend"))
            {
                material.SetFloat(
                    "_DstBlend",
                    additive
                        ? (float)UnityEngine.Rendering.BlendMode.One
                        : (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            }
            if (material.HasProperty("_ZWrite"))
            {
                material.SetFloat("_ZWrite", 0f);
            }
            if (material.HasProperty("_Cull"))
            {
                material.SetFloat("_Cull", (float)UnityEngine.Rendering.CullMode.Off);
            }

            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.EnableKeyword("_ALPHABLEND_ON");
        }

        private static void AssertNoImportedDependencies(string assetPath)
        {
            foreach (string dependency in AssetDatabase.GetDependencies(assetPath, recursive: true))
            {
                string normalized = dependency.Replace('\\', '/');
                if (normalized.Contains("/_Imported/", StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        $"Promoted bombing review asset `{assetPath}` still depends on raw imported asset `{dependency}`.");
                }
            }
        }

        private static void WriteExplosionAssetAudit()
        {
            ExplosionCandidate[] candidates =
            {
                new ExplosionCandidate(
                    "Promoted but rejected for close aircraft shot",
                    SourceCyberBomber2PrefabPath,
                    CyberBomber2PrefabPath,
                    "CyberAirTroopInvader bomber visual part; useful as an effect source, but too emissive/oversized for the requested rear-chase aircraft silhouette."),
                new ExplosionCandidate(
                    "Promoted but rejected for close aircraft shot",
                    SourceCyberBomberPrefabPath,
                    CyberBomberPrefabPath,
                    "Cyber bomber visual part inspected and promoted script-free; not used in the opening shot after visual QA."),
                new ExplosionCandidate(
                    "Selected dropping bomb payloads",
                    SourceAerialBombPrefabPath,
                    AerialBombPrefabPath,
                    "AerialBombing bomb VFX part promoted script-free for visible falling ordnance."),
                new ExplosionCandidate(
                    "Selected dropping bomb payload variant",
                    SourceAerialBomb2PrefabPath,
                    AerialBomb2PrefabPath,
                    "Second AerialBombing payload variant for staggered bomb silhouettes."),
                new ExplosionCandidate(
                    "Selected dropping bomb payload variant",
                    SourceAerialBomb3PrefabPath,
                    AerialBomb3PrefabPath,
                    "Third AerialBombing payload variant for staggered bomb silhouettes."),
                new ExplosionCandidate(
                    "Promoted but removed from final bomb release shot",
                    SourceCyberBombPrefabPath,
                    CyberBombPrefabPath,
                    "CyberAirTroopInvader bomb visual part was inspected, but its red silhouette pulled focus from the promoted Bomb_01 payloads."),
                new ExplosionCandidate(
                    "Promoted but held out of final impact shot",
                    SourceDavfxExplosion09PrefabPath,
                    DavfxExplosion09PrefabPath,
                    "Realistic 6D Lighting Explosions URP prefab inspected; large smoke cards showed visible rectangular bounds in review capture."),
                new ExplosionCandidate(
                    "Promoted but held out of final impact shot",
                    SourceDavfxExplosion20PrefabPath,
                    DavfxExplosion20PrefabPath,
                    "Second DAVFX blast inspected; retained for later tuning but not used in the final bombing-review impact pass."),
                new ExplosionCandidate(
                    "Selected support ground blast",
                    SourceVefectsExplosion03PrefabPath,
                    VefectsExplosion03PrefabPath,
                    "12 layered flipbook particle renderers; retained as promoted fallback/support but not the dominant screen-filling layer."),
                new ExplosionCandidate(
                    "Selected secondary fireball",
                    SourceVefectsExplosionFire02PrefabPath,
                    VefectsExplosionFire02PrefabPath,
                    "Compact fire burst that reads well when staggered between impact points."),
                new ExplosionCandidate(
                    "Selected bomb trail",
                    SourceAirstrikeBombTrailPrefabPath,
                    AirstrikeBombTrailPrefabPath,
                    "Airstrike pack bomb trail particle for visible falling ordnance in the release shot."),
                new ExplosionCandidate(
                    "Selected airstrike payload hit",
                    SourceAirstrikeBombExplosionPrefabPath,
                    AirstrikeBombExplosionPrefabPath,
                    "Native Airstrike pack bomb explosion with the screen-covering Distort child stripped during promotion."),
                new ExplosionCandidate(
                    "Selected shell accent",
                    SourceShellExplosionPrefabPath,
                    ShellExplosionPrefabPath,
                    "Small shell explosion accent for variation around the main bomb impacts."),
                new ExplosionCandidate(
                    "Reviewed alternate",
                    "Assets/_Imported/SpecialSkillsEffectsPack/AllEffects/EffectsSet_1(NotScriptBased)/Effects/Effect_13_DangerClose/Effect_13_Base/Effect_13_Explosion.prefab",
                    string.Empty,
                    "Good candidate, held back to keep this pass tight and avoid overfilling the shot."),
                new ExplosionCandidate(
                    "Reviewed alternate",
                    "Assets/_Imported/AssetStore/VFX/PixPlays/ElementalBlastVFX/FireBlast/Version_BuiltIn/FireBlast.prefab",
                    string.Empty,
                    "Stylized elemental blast; less appropriate for military bombing than the selected flipbook/Airstrike assets.")
            };

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# Bombing Review Explosion Asset Audit");
            builder.AppendLine();
            builder.AppendLine("Status: PASS");
            builder.AppendLine();
            for (int i = 0; i < candidates.Length; i++)
            {
                ExplosionCandidate candidate = candidates[i];
                GameObject sourcePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(candidate.SourcePath);
                int particleSystems = sourcePrefab != null
                    ? sourcePrefab.GetComponentsInChildren<ParticleSystem>(includeInactive: true).Length
                    : 0;
                int renderers = sourcePrefab != null
                    ? sourcePrefab.GetComponentsInChildren<ParticleSystemRenderer>(includeInactive: true).Length
                    : 0;
                int customBehaviours = sourcePrefab != null
                    ? sourcePrefab.GetComponentsInChildren<MonoBehaviour>(includeInactive: true).Length
                    : 0;
                builder.AppendLine($"- {candidate.Label}: `{candidate.SourcePath}`");
                builder.AppendLine($"  - Particle systems: `{particleSystems}`, renderer layers: `{renderers}`, custom MonoBehaviours: `{customBehaviours}`");
                if (!string.IsNullOrEmpty(candidate.TargetPath))
                {
                    builder.AppendLine($"  - Promoted target: `{candidate.TargetPath}`");
                }
                builder.AppendLine($"  - Note: {candidate.Note}");
            }

            Directory.CreateDirectory(Path.GetDirectoryName(ExplosionAuditPath) ?? "C:/tmp");
            File.WriteAllText(ExplosionAuditPath, builder.ToString(), Encoding.UTF8);
        }

        private static void CreateLighting(Transform root)
        {
            GameObject sunObject = new GameObject("BombingReview_KeySun");
            sunObject.transform.SetParent(root, worldPositionStays: false);
            sunObject.transform.SetLocalPositionAndRotation(
                new Vector3(-6f, 12f, -10f),
                Quaternion.Euler(48f, -34f, 0f));
            Light sun = sunObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(0.92f, 0.96f, 1f, 1f);
            sun.intensity = 1.85f;
            sun.shadows = LightShadows.Soft;

            GameObject warmObject = new GameObject("BombingReview_ImpactFillLight");
            warmObject.transform.SetParent(root, worldPositionStays: false);
            warmObject.transform.localPosition = new Vector3(0f, 4.5f, 6.5f);
            Light warm = warmObject.AddComponent<Light>();
            warm.type = LightType.Point;
            warm.color = new Color(1f, 0.42f, 0.14f, 1f);
            warm.intensity = 6.4f;
            warm.range = 16f;
            warm.shadows = LightShadows.None;
        }

        private static Transform CreateEnvironment(
            Transform root,
            Material skyMaterial,
            Material cloudMaterial,
            out Transform cloudRoot)
        {
            cloudRoot = new GameObject("BombingReview_CloudDeck").transform;
            cloudRoot.SetParent(root, worldPositionStays: false);
            Vector3[] cloudPositions =
            {
                new Vector3(-17f, 2.25f, 11f),
                new Vector3(-8f, 2.4f, 16f),
                new Vector3(1f, 2.28f, 13.5f),
                new Vector3(10f, 2.36f, 19f),
                new Vector3(18f, 2.22f, 12f),
                new Vector3(-2f, 2.65f, 24f)
            };
            Vector3[] cloudScales =
            {
                new Vector3(7.6f, 0.34f, 2.7f),
                new Vector3(8.8f, 0.38f, 3.2f),
                new Vector3(9.8f, 0.35f, 3.0f),
                new Vector3(8.2f, 0.36f, 2.8f),
                new Vector3(7.0f, 0.32f, 2.4f),
                new Vector3(10.8f, 0.31f, 3.6f)
            };
            for (int i = 0; i < cloudPositions.Length; i++)
            {
                CreateSphere(
                    cloudRoot,
                    $"CloudMass_{i + 1:00}",
                    cloudPositions[i],
                    Quaternion.Euler(0f, i * 18f, 0f),
                    cloudScales[i],
                    cloudMaterial);
            }

            for (int row = 0; row < 5; row++)
            {
                for (int column = 0; column < 9; column++)
                {
                    float x = -33f + column * 8.2f + (row % 2 == 0 ? 0f : 3.8f);
                    float z = 12f + row * 9.5f + (column % 3) * 1.4f;
                    float y = 4.4f + row * 0.28f + (column % 2) * 0.12f;
                    float scaleX = 10.8f + (column % 4) * 1.8f;
                    float scaleZ = 4.2f + (row % 3) * 0.85f;
                    CreateSphere(
                        cloudRoot,
                        $"CloudSea_{row + 1:00}_{column + 1:00}",
                        new Vector3(x, y, z),
                        Quaternion.Euler(0f, column * 17f + row * 9f, 0f),
                        new Vector3(scaleX, 0.36f, scaleZ),
                        cloudMaterial);
                }
            }

            return cloudRoot;
        }

        private static Transform CreateAircraftFormation(Transform root, Material aircraftMaterial)
        {
            Transform formation = new GameObject(AircraftRootName).transform;
            formation.SetParent(root, worldPositionStays: false);
            formation.localPosition = new Vector3(0f, 8.2f, -23.5f);

            Material bomberMaterial = AssetDatabase.LoadAssetAtPath<Material>(BomberMaterialPath)
                ?? aircraftMaterial;
            Material jetMaterial = AssetDatabase.LoadAssetAtPath<Material>(JetMaterialPath)
                ?? aircraftMaterial;

            GameObject bomber = InstantiateModel(BomberModelPath, "Bomber_Leader");
            bomber.transform.SetParent(formation, worldPositionStays: false);
            bomber.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            ScaleToMaxDimension(bomber.transform, 6.0f);
            AssignMaterial(bomber, bomberMaterial);
            EnsureTimelineAnimator(bomber);

            GameObject leftJet = InstantiateModel(JetModelPath, "Jet_Escort_Left");
            leftJet.transform.SetParent(formation, worldPositionStays: false);
            leftJet.transform.SetLocalPositionAndRotation(new Vector3(-FormationWingX, FormationWingY, FormationWingZ), Quaternion.identity);
            ScaleToMaxDimension(leftJet.transform, 2.85f);
            AssignMaterial(leftJet, jetMaterial);
            EnsureTimelineAnimator(leftJet);

            GameObject rightJet = InstantiateModel(JetModelPath, "Jet_Escort_Right");
            rightJet.transform.SetParent(formation, worldPositionStays: false);
            rightJet.transform.SetLocalPositionAndRotation(new Vector3(FormationWingX, FormationWingY, FormationWingZ), Quaternion.identity);
            ScaleToMaxDimension(rightJet.transform, 2.85f);
            AssignMaterial(rightJet, jetMaterial);
            EnsureTimelineAnimator(rightJet);

            CreateThrusterGlow(bomber.transform, "Bomber_EngineGlow_Left", new Vector3(-0.48f, 0.12f, -2.45f), 0.62f);
            CreateThrusterGlow(bomber.transform, "Bomber_EngineGlow_Center", new Vector3(0f, 0.1f, -2.58f), 0.72f);
            CreateThrusterGlow(bomber.transform, "Bomber_EngineGlow_Right", new Vector3(0.48f, 0.12f, -2.45f), 0.62f);
            CreateThrusterGlow(leftJet.transform, "LeftJet_EngineGlow", new Vector3(0f, 0.08f, -1.82f), 0.54f);
            CreateThrusterGlow(rightJet.transform, "RightJet_EngineGlow", new Vector3(0f, 0.08f, -1.82f), 0.54f);

            Animator animator = formation.gameObject.AddComponent<Animator>();
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            return formation;
        }

        private static Animator EnsureTimelineAnimator(GameObject target)
        {
            Animator animator = target.GetComponent<Animator>();
            if (animator == null)
            {
                animator = target.AddComponent<Animator>();
            }

            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            EditorUtility.SetDirty(animator);
            return animator;
        }

        private static Transform CreateBombDrop(
            Transform root,
            Material bombMaterial)
        {
            Transform bombRoot = new GameObject(BombDropRootName).transform;
            bombRoot.SetParent(root, worldPositionStays: false);
            bombRoot.localPosition = new Vector3(0f, 6.35f, 6.25f);

            Vector3[] positions =
            {
                new Vector3(-9.8f, 0.16f, -1.60f),
                new Vector3(-6.4f, -0.06f, -0.80f),
                new Vector3(-3.2f, 0.10f, -0.05f),
                new Vector3(0.0f, -0.12f, 0.65f),
                new Vector3(3.4f, 0.14f, 1.20f),
                new Vector3(6.8f, -0.08f, 1.85f),
                new Vector3(9.9f, 0.08f, 2.45f)
            };
            string[] bombPrefabs =
            {
                AerialBombPrefabPath,
                AerialBomb2PrefabPath,
                AerialBomb3PrefabPath,
                AerialBombPrefabPath,
                AerialBomb2PrefabPath,
                AerialBomb3PrefabPath,
                AerialBombPrefabPath
            };
            for (int i = 0; i < positions.Length; i++)
            {
                GameObject bomb = InstantiateGameOwnedVfx(
                    bombPrefabs[i],
                    bombRoot,
                    $"FallingBombPayload_{i + 1:00}",
                    positions[i],
                    Quaternion.Euler(90f, -27f + i * 9f, 0f),
                    Vector3.one * (0.78f + (i % 3) * 0.07f));
                GameObject bombBody = InstantiateModel(BombModelPath, $"FallingBombBody_{i + 1:00}");
                bombBody.transform.SetParent(bomb.transform, worldPositionStays: false);
                bombBody.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                ScaleToMaxDimension(bombBody.transform, 2.34f + (i % 3) * 0.12f);
                Material promotedBombMaterial = AssetDatabase.LoadAssetAtPath<Material>(BombMaterialPath)
                    ?? bombMaterial;
                AssignMaterial(bombBody, promotedBombMaterial);
                InstantiateGameOwnedVfx(
                    AirstrikeBombTrailPrefabPath,
                    bomb.transform,
                    $"AirstrikeBombTrail_Attached_{i + 1:00}",
                    new Vector3(0f, 0.62f, 0.08f),
                    Quaternion.Euler(-90f, 0f, 0f),
                    Vector3.one * 0.92f);
            }

            Animator animator = bombRoot.gameObject.AddComponent<Animator>();
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            return bombRoot;
        }

        private static Transform CreateImpactChain(Transform root)
        {
            Transform impactRoot = new GameObject(ImpactRootName).transform;
            impactRoot.SetParent(root, worldPositionStays: false);

            Vector3[] impacts =
            {
                new Vector3(-5.6f, 0.25f, 4.2f),
                new Vector3(-1.8f, 0.25f, 6.8f),
                new Vector3(2.7f, 0.25f, 5.4f),
                new Vector3(6.2f, 0.25f, 8.5f)
            };
            for (int i = 0; i < impacts.Length; i++)
            {
                Transform burst = new GameObject($"ImpactBurst_{i + 1:00}").transform;
                burst.SetParent(impactRoot, worldPositionStays: false);
                burst.localPosition = impacts[i];
                InstantiateGameOwnedVfx(
                    AirstrikeBombExplosionPrefabPath,
                    burst,
                    $"Airstrike_BombExplosion_Core_{i + 1:00}",
                    new Vector3(0.08f, 0.10f, -0.05f),
                    Quaternion.Euler(0f, i * -19f, 0f),
                    Vector3.one * (0.64f + i * 0.05f));
                InstantiateGameOwnedVfx(
                    i % 2 == 0 ? VefectsExplosionFire02PrefabPath : ShellExplosionPrefabPath,
                    burst,
                    $"BombImpact_SecondaryBlast_{i + 1:00}",
                    new Vector3(-0.18f, 0.16f, 0.14f),
                    Quaternion.Euler(0f, i * 47f, 0f),
                    Vector3.one * (0.34f + i * 0.04f));
            }

            return impactRoot;
        }

        private static Transform CreateAftermathSmoke(Transform root)
        {
            Transform smokeRoot = new GameObject(SmokeRootName).transform;
            smokeRoot.SetParent(root, worldPositionStays: false);
            Vector3[] smokePositions =
            {
                new Vector3(-6f, 0.24f, 4.4f),
                new Vector3(-1.2f, 0.25f, 6.7f),
                new Vector3(3.2f, 0.24f, 5.7f),
                new Vector3(6.3f, 0.26f, 8.8f)
            };
            for (int i = 0; i < smokePositions.Length; i++)
            {
                InstantiateGameOwnedVfx(
                    UniLongSmokePrefabPath,
                    smokeRoot,
                    $"AftermathSmokeColumn_{i + 1:00}",
                    smokePositions[i] + new Vector3(0f, 0.18f, 0f),
                    Quaternion.Euler(0f, i * 41f, 0f),
                    Vector3.one * (0.72f + i * 0.06f));
                if (i % 2 == 0)
                {
                    InstantiateGameOwnedVfx(
                        UniGroundFirePrefabPath,
                        smokeRoot,
                        $"AftermathGroundFire_{i + 1:00}",
                        smokePositions[i] + new Vector3(0f, 0.10f, 0f),
                        Quaternion.Euler(0f, i * 31f, 0f),
                        Vector3.one * 0.38f);
                }
            }

            return smokeRoot;
        }

        private static Camera CreateMainCamera(Scene scene)
        {
            GameObject cameraObject = new GameObject(MainCameraName);
            SceneManager.MoveGameObjectToScene(cameraObject, scene);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.62f, 0.66f, 0.64f, 1f);
            camera.fieldOfView = 38f;
            camera.nearClipPlane = 0.03f;
            camera.farClipPlane = 220f;
            cameraObject.tag = "MainCamera";
            cameraObject.AddComponent<AudioListener>();

            CinemachineBrain brain = cameraObject.AddComponent<CinemachineBrain>();
            brain.IgnoreTimeScale = true;
            brain.UpdateMethod = CinemachineBrain.UpdateMethods.LateUpdate;
            brain.BlendUpdateMethod = CinemachineBrain.BrainUpdateMethods.LateUpdate;
            brain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.Cut, 0f);
            return camera;
        }

        private static TransitionOverlayBindings CreateTransitionOverlay(Scene scene, Transform root, Camera camera)
        {
            GameObject overlay = new GameObject("BombingReview_TransitionOverlay", typeof(RectTransform));
            SceneManager.MoveGameObjectToScene(overlay, scene);
            overlay.transform.SetParent(root, worldPositionStays: false);
            ConfigureFullScreenRect(overlay.GetComponent<RectTransform>());

            Canvas canvas = overlay.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 0.05f;
            canvas.sortingOrder = 32020;
            CanvasScaler scaler = overlay.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            CanvasGroup curtainGroup = CreateScreenEffectGroup(scene, overlay.transform, "BombingReview_BlackScreenTransition");
            CreateScreenEffectImage(
                scene,
                curtainGroup.transform,
                "BombingReview_BlackScreenTransition_Image",
                Color.black,
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                0f);

            EditorUtility.SetDirty(canvas);
            EditorUtility.SetDirty(scaler);
            return new TransitionOverlayBindings(EnsureTimelineAnimator(curtainGroup.gameObject));
        }

        private static CanvasGroup CreateScreenEffectGroup(Scene scene, Transform parent, string name)
        {
            GameObject groupObject = new GameObject(name, typeof(RectTransform));
            SceneManager.MoveGameObjectToScene(groupObject, scene);
            groupObject.transform.SetParent(parent, worldPositionStays: false);
            ConfigureFullScreenRect(groupObject.GetComponent<RectTransform>());
            CanvasGroup group = groupObject.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;
            EditorUtility.SetDirty(group);
            return group;
        }

        private static void CreateScreenEffectImage(
            Scene scene,
            Transform parent,
            string name,
            Color color,
            Vector2 anchor,
            Vector2 sizeDelta,
            float zRotation)
        {
            GameObject imageObject = new GameObject(name, typeof(RectTransform));
            SceneManager.MoveGameObjectToScene(imageObject, scene);
            imageObject.transform.SetParent(parent, worldPositionStays: false);
            RectTransform rect = imageObject.GetComponent<RectTransform>();
            if (sizeDelta == Vector2.zero)
            {
                ConfigureFullScreenRect(rect);
            }
            else
            {
                rect.anchorMin = anchor;
                rect.anchorMax = anchor;
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = sizeDelta;
            }

            rect.localRotation = Quaternion.Euler(0f, 0f, zRotation);
            Image image = imageObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            EditorUtility.SetDirty(rect);
            EditorUtility.SetDirty(image);
        }

        private static void ConfigureFullScreenRect(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.localScale = Vector3.one;
        }

        private static CameraShot[] CreateCinemachineShots(
            Transform root,
            Transform aircraftRoot,
            Transform bombDropRoot,
            CinemachineBrain brain)
        {
            Transform cameraRoot = new GameObject(CameraRootName).transform;
            cameraRoot.SetParent(root, worldPositionStays: false);

            CameraShot[] shots =
            {
                CreateLocalShot(
                    aircraftRoot,
                    "cm_01_formation_join",
                    0f,
                    BombReleaseStartSeconds,
                    new Vector3(0f, 1.14f, -11.0f),
                    new Vector3(0f, 4.75f, 33.0f),
                    48f),
                CreateLocalShot(
                    bombDropRoot,
                    "cm_02_bomb_release",
                    BombReleaseStartSeconds,
                    1.12f,
                    new Vector3(-10.0f, 2.75f, -8.6f),
                    new Vector3(0f, 0.12f, 0.15f),
                    58f),
                CreateLocalShot(
                    bombDropRoot,
                    "cm_03_falling_payload",
                    4.84f,
                    1.02f,
                    new Vector3(5.4f, -1.05f, -6.4f),
                    new Vector3(0.4f, -2.18f, 0.85f),
                    56f),
                CreateShot(
                    cameraRoot,
                    "cm_04_target_reframe",
                    5.86f,
                    0.36f,
                    new Vector3(-7.6f, 2.20f, -3.2f),
                    new Vector3(0.45f, 0.42f, 7.2f),
                    49f),
                CreateShot(
                    cameraRoot,
                    "cm_05_impact_chain",
                    6.22f,
                    1.18f,
                    new Vector3(10.2f, 5.2f, -6.8f),
                    new Vector3(0.8f, 0.74f, 7.1f),
                    47f),
                CreateShot(
                    cameraRoot,
                    "cm_06_aftershock",
                    7.40f,
                    0.96f,
                    new Vector3(-9.6f, 4.6f, 13.6f),
                    new Vector3(0.8f, 1.16f, 7.1f),
                    42f),
                CreateShot(
                    cameraRoot,
                    "cm_07_smoke_handoff",
                    8.36f,
                    0.44f,
                    new Vector3(2.2f, 3.15f, 17.6f),
                    new Vector3(0.2f, 0.8f, 6.9f),
                    36f)
            };

            for (int i = 0; i < shots.Length; i++)
            {
                shots[i].Camera.Priority = 0;
                shots[i].Camera.StandbyUpdate = CinemachineVirtualCameraBase.StandbyUpdateMode.Never;
            }

            EditorUtility.SetDirty(brain);
            return shots;
        }

        private static CameraShot CreateShot(
            Transform parent,
            string shotId,
            float startSeconds,
            float durationSeconds,
            Vector3 position,
            Vector3 lookAt,
            float fov)
        {
            GameObject cameraObject = new GameObject(shotId);
            cameraObject.transform.SetParent(parent, worldPositionStays: false);
            cameraObject.transform.SetPositionAndRotation(position, ResolveLookRotation(position, lookAt));

            GameObject lookAtObject = new GameObject($"{shotId}_LookAt");
            lookAtObject.transform.SetParent(parent, worldPositionStays: false);
            lookAtObject.transform.position = lookAt;

            CinemachineCamera cm = cameraObject.AddComponent<CinemachineCamera>();
            cm.LookAt = lookAtObject.transform;
            LensSettings lens = LensSettings.Default;
            lens.ModeOverride = LensSettings.OverrideModes.Perspective;
            lens.FieldOfView = fov;
            lens.NearClipPlane = 0.03f;
            lens.FarClipPlane = 220f;
            cm.Lens = lens;
            cameraObject.AddComponent<CinemachineHardLookAt>();

            Animator animator = cameraObject.AddComponent<Animator>();
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            return new CameraShot(shotId, startSeconds, durationSeconds, cm, animator);
        }

        private static CameraShot CreateLocalShot(
            Transform parent,
            string shotId,
            float startSeconds,
            float durationSeconds,
            Vector3 localPosition,
            Vector3 localLookAt,
            float fov)
        {
            GameObject cameraObject = new GameObject(shotId);
            cameraObject.transform.SetParent(parent, worldPositionStays: false);
            cameraObject.transform.SetLocalPositionAndRotation(
                localPosition,
                ResolveLookRotation(localPosition, localLookAt));

            GameObject lookAtObject = new GameObject($"{shotId}_LookAt");
            lookAtObject.transform.SetParent(parent, worldPositionStays: false);
            lookAtObject.transform.localPosition = localLookAt;

            CinemachineCamera cm = cameraObject.AddComponent<CinemachineCamera>();
            cm.LookAt = lookAtObject.transform;
            LensSettings lens = LensSettings.Default;
            lens.ModeOverride = LensSettings.OverrideModes.Perspective;
            lens.FieldOfView = fov;
            lens.NearClipPlane = 0.03f;
            lens.FarClipPlane = 220f;
            cm.Lens = lens;
            cameraObject.AddComponent<CinemachineHardLookAt>();

            Animator animator = cameraObject.AddComponent<Animator>();
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            return new CameraShot(shotId, startSeconds, durationSeconds, cm, animator);
        }

        private static PlayableDirector CreateTimeline(
            Scene scene,
            Transform root,
            CinemachineBrain brain,
            CameraShot[] shots,
            Transform aircraftRoot,
            Transform cloudRoot,
            Transform bombDropRoot,
            Transform impactRoot,
            Transform smokeRoot,
            TransitionOverlayBindings transitionOverlay)
        {
            EnsureFolder(PathParent(TimelinePath));
            TimelineAsset existing = AssetDatabase.LoadAssetAtPath<TimelineAsset>(TimelinePath);
            if (existing != null)
            {
                AssetDatabase.DeleteAsset(TimelinePath);
            }

            TimelineAsset timeline = ScriptableObject.CreateInstance<TimelineAsset>();
            timeline.name = Path.GetFileNameWithoutExtension(TimelinePath);
            timeline.durationMode = TimelineAsset.DurationMode.FixedLength;
            timeline.fixedDuration = TimelineDurationSeconds;
            timeline.editorSettings.frameRate = 30d;
            AssetDatabase.CreateAsset(timeline, TimelinePath);

            GameObject directorObject = new GameObject(TimelineDirectorName);
            SceneManager.MoveGameObjectToScene(directorObject, scene);
            directorObject.transform.SetParent(root, worldPositionStays: false);
            PlayableDirector director = directorObject.AddComponent<PlayableDirector>();
            director.playableAsset = timeline;
            director.playOnAwake = true;
            director.timeUpdateMode = DirectorUpdateMode.UnscaledGameTime;
            director.extrapolationMode = DirectorWrapMode.Hold;

            CreateCinemachineTrack(timeline, director, brain, shots);
            CreateAnimationTrack(
                timeline,
                director,
                "Aircraft Formation Move",
                aircraftRoot.GetComponent<Animator>(),
                CreateFormationRootMoveClip("AC_AircraftFormationMove"));
            CreateAnimationTrack(
                timeline,
                director,
                "Bomber Lead Entry",
                RequireChildAnimator(aircraftRoot, "Bomber_Leader"),
                CreateAircraftJoinClip(
                    "AC_BomberLeadEntry",
                    0f,
                    BomberLeadEntryEndSeconds,
                    new Vector3(0f, -0.08f, -4.25f),
                    Vector3.zero,
                    -1.6f,
                    0.45f));
            CreateAnimationTrack(
                timeline,
                director,
                "Left Escort Join",
                RequireChildAnimator(aircraftRoot, "Jet_Escort_Left"),
                CreateAircraftJoinClip(
                    "AC_LeftEscortJoin",
                    LeftEscortJoinStartSeconds,
                    LeftEscortJoinEndSeconds,
                    new Vector3(-16.8f, -0.48f, -8.20f),
                    new Vector3(-FormationWingX, FormationWingY, FormationWingZ),
                    13.5f,
                    1.0f));
            CreateAnimationTrack(
                timeline,
                director,
                "Right Escort Join",
                RequireChildAnimator(aircraftRoot, "Jet_Escort_Right"),
                CreateAircraftJoinClip(
                    "AC_RightEscortJoin",
                    RightEscortJoinStartSeconds,
                    RightEscortJoinEndSeconds,
                    new Vector3(16.8f, -0.50f, -8.70f),
                    new Vector3(FormationWingX, FormationWingY, FormationWingZ),
                    -13.5f,
                    -1.0f));
            CreateAnimationTrack(
                timeline,
                director,
                "Bomb Drop Move",
                bombDropRoot.GetComponent<Animator>(),
                CreateBombDropClip("AC_BombDropMove"));
            CreateAnimationTrack(
                timeline,
                director,
                "Impact Camera Recoil",
                shots[4].Animator,
                CreateRecoilClip("AC_ImpactCameraRecoil", shots[4].Camera.transform.localPosition, 6.24f));
            CreateActivationTrack(timeline, director, "Cloud Deck Active", cloudRoot.gameObject, 0f, 5.48f);
            CreateActivationTrack(timeline, director, "Bombs Active", bombDropRoot.gameObject, BombReleaseStartSeconds, 2.42f);
            CreateActivationTrack(timeline, director, "Impact Chain Active", impactRoot.gameObject, 6.10f, 2.52f);
            CreateActivationTrack(timeline, director, "Impact Burst 01 Active", RequireChild(impactRoot, "ImpactBurst_01"), 6.22f, 1.72f);
            CreateActivationTrack(timeline, director, "Impact Burst 02 Active", RequireChild(impactRoot, "ImpactBurst_02"), 6.52f, 1.74f);
            CreateActivationTrack(timeline, director, "Impact Burst 03 Active", RequireChild(impactRoot, "ImpactBurst_03"), 6.82f, 1.76f);
            CreateActivationTrack(timeline, director, "Impact Burst 04 Active", RequireChild(impactRoot, "ImpactBurst_04"), 7.12f, 1.78f);
            CreateActivationTrack(timeline, director, "Aftermath Smoke Active", smokeRoot.gameObject, 7.38f, 1.42f);
            CreateAnimationTrack(
                timeline,
                director,
                "Black Screen Transition",
                transitionOverlay.CurtainAnimator,
                CreateCanvasGroupAlphaClip(
                    "AC_BombingReview_BlackScreenTransition",
                    (0.00f, 0.52f),
                    (0.08f, 0.28f),
                    (0.24f, 0.00f),
                    (8.18f, 0.00f),
                    (8.44f, 0.40f),
                    (8.80f, 0.90f)));

            EditorUtility.SetDirty(timeline);
            EditorUtility.SetDirty(director);
            AssetDatabase.SaveAssets();
            return director;
        }

        private static void CreateCinemachineTrack(
            TimelineAsset timeline,
            PlayableDirector director,
            CinemachineBrain brain,
            CameraShot[] shots)
        {
            CreateCinemachineTrack(timeline, director, brain, shots, "Cinemachine Shots", "cm");
        }

        private static void CreateCinemachineTrack(
            TimelineAsset timeline,
            PlayableDirector director,
            CinemachineBrain brain,
            CameraShot[] shots,
            string trackName,
            string exposedPrefix)
        {
            CinemachineTrack track = timeline.CreateTrack<CinemachineTrack>(trackName);
            track.TrackPriority = 200;
            director.SetGenericBinding(track, brain);
            for (int i = 0; i < shots.Length; i++)
            {
                TimelineClip clip = track.CreateClip<CinemachineShot>();
                clip.displayName = shots[i].ShotId;
                clip.start = shots[i].StartSeconds;
                clip.duration = shots[i].DurationSeconds;
                if (i > 0)
                {
                    clip.blendInDuration = i == 1 ? 0.16d : 0.10d;
                    clip.easeInDuration = clip.blendInDuration;
                }

                CinemachineShot shotAsset = clip.asset as CinemachineShot;
                if (shotAsset == null)
                {
                    continue;
                }

                shotAsset.DisplayName = shots[i].ShotId;
                PropertyName exposedName = new PropertyName($"{exposedPrefix}_{i + 1:00}_{shots[i].ShotId}");
                shotAsset.VirtualCamera.exposedName = exposedName;
                director.SetReferenceValue(exposedName, shots[i].Camera);
                EditorUtility.SetDirty(shotAsset);
            }

            EditorUtility.SetDirty(track);
        }

        private static void CreateAnimationTrack(
            TimelineAsset timeline,
            PlayableDirector director,
            string trackName,
            Animator animator,
            AnimationClip clipAsset)
        {
            if (animator == null)
            {
                throw new InvalidOperationException($"Missing Animator for `{trackName}`.");
            }

            AnimationTrack track = timeline.CreateTrack<AnimationTrack>(trackName);
            track.trackOffset = TrackOffset.Auto;
            director.SetGenericBinding(track, animator);
            TimelineClip clip = track.CreateClip(clipAsset);
            clip.displayName = clipAsset.name;
            clip.start = 0d;
            clip.duration = TimelineDurationSeconds;
            AnimationPlayableAsset playableAsset = clip.asset as AnimationPlayableAsset;
            if (playableAsset != null)
            {
                playableAsset.removeStartOffset = false;
                playableAsset.applyFootIK = false;
                playableAsset.loop = AnimationPlayableAsset.LoopMode.Off;
                EditorUtility.SetDirty(playableAsset);
            }

            EditorUtility.SetDirty(track);
        }

        private static void CreateActivationTrack(
            TimelineAsset timeline,
            PlayableDirector director,
            string trackName,
            GameObject target,
            float startSeconds,
            float durationSeconds)
        {
            ActivationTrack track = timeline.CreateTrack<ActivationTrack>(trackName);
            director.SetGenericBinding(track, target);
            TimelineClip clip = track.CreateDefaultClip();
            clip.displayName = trackName;
            clip.start = startSeconds;
            clip.duration = durationSeconds;
            track.postPlaybackState = ActivationTrack.PostPlaybackState.Inactive;
            EditorUtility.SetDirty(track);
        }

        private static GameObject RequireChild(Transform parent, string childName)
        {
            Transform child = parent.Find(childName);
            if (child == null)
            {
                throw new InvalidOperationException($"Missing child `{childName}` under `{parent.name}`.");
            }

            return child.gameObject;
        }

        private static Animator RequireChildAnimator(Transform parent, string childName)
        {
            GameObject child = RequireChild(parent, childName);
            Animator animator = child.GetComponent<Animator>();
            if (animator == null)
            {
                throw new InvalidOperationException($"Missing Animator on `{childName}` under `{parent.name}`.");
            }

            return animator;
        }

        private static AnimationClip CreateFormationRootMoveClip(string clipName)
        {
            AnimationClip clip = CreateOrReplaceAnimationClip(clipName);
            clip.frameRate = 30f;
            clip.wrapMode = WrapMode.ClampForever;
            SetCurve(
                clip,
                "m_LocalPosition.x",
                LinearValueKeyed(
                    0f,
                    (0.00f, 0.00f),
                    (0.62f, -0.28f),
                    (1.36f, 0.34f),
                    (2.18f, -0.24f),
                    (3.02f, 0.42f),
                    (3.62f, -0.06f),
                    (4.36f, 0.04f),
                    (5.36f, 0.03f),
                    (6.62f, 0.01f),
                    (TimelineDurationSeconds, 0.00f)));
            SetCurve(
                clip,
                "m_LocalPosition.y",
                LinearValueKeyed(
                    8.2f,
                    (0.00f, 8.20f),
                    (0.90f, 8.30f),
                    (1.72f, 8.22f),
                    (2.62f, 8.36f),
                    (3.62f, 8.30f),
                    (4.36f, 8.33f),
                    (5.36f, 8.32f),
                    (6.62f, 8.31f),
                    (TimelineDurationSeconds, 8.31f)));
            SetCurve(
                clip,
                "m_LocalPosition.z",
                LinearValueKeyed(
                    -23.5f,
                    (0.00f, -25.50f),
                    (0.72f, -20.40f),
                    (1.42f, -14.20f),
                    (2.22f, -7.20f),
                    (3.02f, 0.30f),
                    (3.62f, 5.20f),
                    (4.36f, 12.10f),
                    (5.36f, 19.60f),
                    (6.62f, 25.20f),
                    (TimelineDurationSeconds, 27.40f)));
            SetCurve(clip, "m_LocalRotation.x", LinearValueKeyed(0f, (0f, 0f), (TimelineDurationSeconds, 0f)));
            SetCurve(clip, "m_LocalRotation.y", LinearValueKeyed(0f, (0f, 0f), (TimelineDurationSeconds, 0f)));
            SetCurve(
                clip,
                "m_LocalRotation.z",
                LinearValueKeyed(
                    0f,
                    (0.00f, 0f),
                    (0.62f, 0.020f),
                    (1.36f, -0.018f),
                    (2.18f, 0.016f),
                    (3.02f, -0.014f),
                    (3.62f, 0.004f),
                    (4.36f, -0.001f),
                    (5.36f, 0.000f),
                    (6.62f, 0.000f),
                    (TimelineDurationSeconds, 0.000f)));
            SetCurve(
                clip,
                "m_LocalRotation.w",
                LinearValueKeyed(
                    1f,
                    (0.00f, 1f),
                    (0.62f, 0.9998f),
                    (1.36f, 0.9998f),
                    (2.18f, 0.9999f),
                    (3.02f, 0.9999f),
                    (3.62f, 0.9999f),
                    (4.36f, 1.0000f),
                    (5.36f, 1.0000f),
                    (6.62f, 1.0000f),
                    (TimelineDurationSeconds, 1.0000f)));
            EditorUtility.SetDirty(clip);
            return clip;
        }

        private static AnimationClip CreateAircraftJoinClip(
            string clipName,
            float motionStart,
            float motionEnd,
            Vector3 startPosition,
            Vector3 formationPosition,
            float entryRollDegrees,
            float swaySign)
        {
            AnimationClip clip = CreateOrReplaceAnimationClip(clipName);
            clip.frameRate = 30f;
            clip.wrapMode = WrapMode.ClampForever;

            float midTime = Mathf.Lerp(motionStart, motionEnd, 0.56f);
            float preLockTime = Mathf.Max(motionStart + 0.02f, motionEnd - 0.34f);
            Vector3 midPosition = Vector3.Lerp(startPosition, formationPosition, 0.56f)
                + new Vector3(swaySign * 0.24f, 0.05f, -0.18f);
            Vector3 preLockPosition = Vector3.Lerp(startPosition, formationPosition, 0.88f)
                + new Vector3(swaySign * -0.12f, -0.02f, 0.07f);

            (float Time, Vector3 Value)[] positions =
            {
                (0f, startPosition),
                (motionStart, startPosition),
                (midTime, midPosition),
                (preLockTime, preLockPosition),
                (motionEnd, formationPosition),
                (TimelineDurationSeconds, formationPosition)
            };
            SetVector3Curves(clip, positions);

            (float Time, float RollDegrees)[] rolls =
            {
                (0f, entryRollDegrees),
                (motionStart, entryRollDegrees),
                (midTime, entryRollDegrees * 0.42f),
                (preLockTime, entryRollDegrees * 0.16f),
                (motionEnd, 0f),
                (TimelineDurationSeconds, 0f)
            };
            SetCurve(clip, "m_LocalRotation.x", LinearValueKeyed(0f, (0f, 0f), (TimelineDurationSeconds, 0f)));
            SetCurve(clip, "m_LocalRotation.y", LinearValueKeyed(0f, (0f, 0f), (TimelineDurationSeconds, 0f)));
            SetCurve(clip, "m_LocalRotation.z", LinearRollQuaternionKeyed(0f, QuaternionComponent.Z, rolls));
            SetCurve(clip, "m_LocalRotation.w", LinearRollQuaternionKeyed(1f, QuaternionComponent.W, rolls));

            EditorUtility.SetDirty(clip);
            return clip;
        }

        private static AnimationClip CreateBombDropClip(string clipName)
        {
            AnimationClip clip = CreateOrReplaceAnimationClip(clipName);
            clip.frameRate = 30f;
            clip.wrapMode = WrapMode.ClampForever;

            (float Time, Vector3 Value)[] positions =
            {
                (0f, new Vector3(0f, 6.35f, 6.25f)),
                (BombReleaseStartSeconds, new Vector3(0f, 6.35f, 6.25f)),
                (BombReleaseStartSeconds + 0.36f, new Vector3(0.05f, 5.88f, 6.28f)),
                (4.84f, new Vector3(0.18f, 3.15f, 6.42f)),
                (5.92f, new Vector3(0.42f, 0.36f, 6.65f)),
                (TimelineDurationSeconds, new Vector3(0.42f, 0.36f, 6.65f))
            };
            SetVector3Curves(clip, positions);
            SetCurve(clip, "m_LocalRotation.x", LinearValueKeyed(0f, (0f, 0f), (TimelineDurationSeconds, 0f)));
            SetCurve(clip, "m_LocalRotation.y", LinearValueKeyed(0f, (0f, 0f), (TimelineDurationSeconds, 0f)));
            SetCurve(clip, "m_LocalRotation.z", LinearValueKeyed(0f, (0f, 0f), (TimelineDurationSeconds, 0f)));
            SetCurve(clip, "m_LocalRotation.w", LinearValueKeyed(1f, (0f, 1f), (TimelineDurationSeconds, 1f)));
            EditorUtility.SetDirty(clip);
            return clip;
        }

        private static AnimationClip CreateTransformClip(
            string clipName,
            float motionStart,
            float motionEnd,
            Vector3 startPosition,
            Vector3 endPosition,
            Quaternion startRotation,
            Quaternion endRotation)
        {
            AnimationClip clip = CreateOrReplaceAnimationClip(clipName);
            clip.frameRate = 30f;
            clip.wrapMode = WrapMode.ClampForever;
            SetCurve(clip, "m_LocalPosition.x", ConstantThenEase(motionStart, motionEnd, startPosition.x, endPosition.x));
            SetCurve(clip, "m_LocalPosition.y", ConstantThenEase(motionStart, motionEnd, startPosition.y, endPosition.y));
            SetCurve(clip, "m_LocalPosition.z", ConstantThenEase(motionStart, motionEnd, startPosition.z, endPosition.z));
            SetCurve(clip, "m_LocalRotation.x", ConstantThenEase(motionStart, motionEnd, startRotation.x, endRotation.x));
            SetCurve(clip, "m_LocalRotation.y", ConstantThenEase(motionStart, motionEnd, startRotation.y, endRotation.y));
            SetCurve(clip, "m_LocalRotation.z", ConstantThenEase(motionStart, motionEnd, startRotation.z, endRotation.z));
            SetCurve(clip, "m_LocalRotation.w", ConstantThenEase(motionStart, motionEnd, startRotation.w, endRotation.w));
            EditorUtility.SetDirty(clip);
            return clip;
        }

        private static void SetVector3Curves(AnimationClip clip, params (float Time, Vector3 Value)[] positions)
        {
            (float Time, float Value)[] xKeys = new (float Time, float Value)[positions.Length];
            (float Time, float Value)[] yKeys = new (float Time, float Value)[positions.Length];
            (float Time, float Value)[] zKeys = new (float Time, float Value)[positions.Length];
            for (int i = 0; i < positions.Length; i++)
            {
                xKeys[i] = (positions[i].Time, positions[i].Value.x);
                yKeys[i] = (positions[i].Time, positions[i].Value.y);
                zKeys[i] = (positions[i].Time, positions[i].Value.z);
            }

            SetCurve(clip, "m_LocalPosition.x", LinearValueKeyed(positions.Length > 0 ? positions[0].Value.x : 0f, xKeys));
            SetCurve(clip, "m_LocalPosition.y", LinearValueKeyed(positions.Length > 0 ? positions[0].Value.y : 0f, yKeys));
            SetCurve(clip, "m_LocalPosition.z", LinearValueKeyed(positions.Length > 0 ? positions[0].Value.z : 0f, zKeys));
        }

        private static AnimationClip CreateCanvasGroupAlphaClip(
            string clipName,
            params (float Time, float Alpha)[] keys)
        {
            AnimationClip clip = CreateOrReplaceAnimationClip(clipName);
            clip.frameRate = 30f;
            clip.wrapMode = WrapMode.ClampForever;
            AnimationCurve curve = EaseInOutKeyed(0f, keys);
            clip.SetCurve(string.Empty, typeof(CanvasGroup), "m_Alpha", curve);
            EditorUtility.SetDirty(clip);
            return clip;
        }

        private static AnimationClip CreateRecoilClip(string clipName, Vector3 baseLocalPosition, float impact)
        {
            AnimationClip clip = CreateOrReplaceAnimationClip(clipName);
            clip.frameRate = 30f;
            clip.wrapMode = WrapMode.ClampForever;
            SetCurve(
                clip,
                "m_LocalPosition.x",
                Keyed(baseLocalPosition.x, (impact, baseLocalPosition.x + 0.32f), (impact + 0.08f, baseLocalPosition.x - 0.20f), (impact + 0.24f, baseLocalPosition.x)));
            SetCurve(
                clip,
                "m_LocalPosition.y",
                Keyed(baseLocalPosition.y, (impact, baseLocalPosition.y + 0.18f), (impact + 0.10f, baseLocalPosition.y - 0.13f), (impact + 0.28f, baseLocalPosition.y)));
            SetCurve(
                clip,
                "m_LocalPosition.z",
                Keyed(baseLocalPosition.z, (impact, baseLocalPosition.z - 0.24f), (impact + 0.18f, baseLocalPosition.z + 0.11f), (impact + 0.35f, baseLocalPosition.z)));
            EditorUtility.SetDirty(clip);
            return clip;
        }

        private static AnimationClip CreateTransitionCameraShakeClip(
            string clipName,
            Vector3 baseLocalPosition,
            params (float Time, Vector3 Position)[] keys)
        {
            AnimationClip clip = CreateOrReplaceAnimationClip(clipName);
            clip.frameRate = 30f;
            clip.wrapMode = WrapMode.ClampForever;
            List<(float Time, float Value)> xKeys = new List<(float Time, float Value)>();
            List<(float Time, float Value)> yKeys = new List<(float Time, float Value)>();
            List<(float Time, float Value)> zKeys = new List<(float Time, float Value)>();
            for (int i = 0; i < keys.Length; i++)
            {
                xKeys.Add((keys[i].Time, keys[i].Position.x));
                yKeys.Add((keys[i].Time, keys[i].Position.y));
                zKeys.Add((keys[i].Time, keys[i].Position.z));
            }

            SetCurve(clip, "m_LocalPosition.x", Keyed(baseLocalPosition.x, xKeys.ToArray()));
            SetCurve(clip, "m_LocalPosition.y", Keyed(baseLocalPosition.y, yKeys.ToArray()));
            SetCurve(clip, "m_LocalPosition.z", Keyed(baseLocalPosition.z, zKeys.ToArray()));
            EditorUtility.SetDirty(clip);
            return clip;
        }

        private static AnimationClip CreateOrReplaceAnimationClip(string clipName)
        {
            EnsureFolder(AnimationRoot);
            string path = $"{AnimationRoot}/{clipName}.anim";
            AnimationClip existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (existing != null)
            {
                AssetDatabase.DeleteAsset(path);
            }

            AnimationClip clip = new AnimationClip
            {
                name = clipName,
                legacy = false
            };
            AssetDatabase.CreateAsset(clip, path);
            return clip;
        }

        private static AnimationCurve ConstantThenEase(float start, float end, float valueA, float valueB)
        {
            AnimationCurve curve = new AnimationCurve(
                new Keyframe(0f, valueA),
                new Keyframe(Mathf.Max(0.001f, start), valueA),
                new Keyframe(Mathf.Max(start + 0.001f, end), valueB),
                new Keyframe(TimelineDurationSeconds, valueB));
            for (int i = 0; i < curve.length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
                AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
            }

            return curve;
        }

        private static AnimationCurve Keyed(float defaultValue, params (float Time, float Value)[] keys)
        {
            List<Keyframe> frames = new List<Keyframe>
            {
                new Keyframe(0f, defaultValue)
            };
            for (int i = 0; i < keys.Length; i++)
            {
                frames.Add(new Keyframe(keys[i].Time, keys[i].Value));
            }
            frames.Add(new Keyframe(TimelineDurationSeconds, defaultValue));
            AnimationCurve curve = new AnimationCurve(frames.ToArray());
            for (int i = 0; i < curve.length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Auto);
                AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Auto);
            }

            return curve;
        }

        private static AnimationCurve LinearKeyed(float defaultValue, params (float Time, float Value)[] keys)
        {
            List<Keyframe> frames = new List<Keyframe>();
            if (keys.Length == 0 || keys[0].Time > 0.0001f)
            {
                frames.Add(new Keyframe(0f, defaultValue));
            }

            for (int i = 0; i < keys.Length; i++)
            {
                frames.Add(new Keyframe(keys[i].Time, Mathf.Clamp01(keys[i].Value)));
            }
            if (frames.Count == 0 || frames[frames.Count - 1].time < TimelineDurationSeconds - 0.0001f)
            {
                frames.Add(new Keyframe(TimelineDurationSeconds, frames.Count == 0 ? defaultValue : frames[frames.Count - 1].value));
            }
            AnimationCurve curve = new AnimationCurve(frames.ToArray());
            for (int i = 0; i < curve.length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
                AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
            }

            return curve;
        }

        private static AnimationCurve LinearValueKeyed(float defaultValue, params (float Time, float Value)[] keys)
        {
            List<Keyframe> frames = new List<Keyframe>();
            if (keys.Length == 0 || keys[0].Time > 0.0001f)
            {
                frames.Add(new Keyframe(0f, defaultValue));
            }

            for (int i = 0; i < keys.Length; i++)
            {
                frames.Add(new Keyframe(keys[i].Time, keys[i].Value));
            }
            if (frames.Count == 0 || frames[frames.Count - 1].time < TimelineDurationSeconds - 0.0001f)
            {
                frames.Add(new Keyframe(TimelineDurationSeconds, frames.Count == 0 ? defaultValue : frames[frames.Count - 1].value));
            }

            AnimationCurve curve = new AnimationCurve(frames.ToArray());
            for (int i = 0; i < curve.length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
                AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
            }

            return curve;
        }

        private static AnimationCurve LinearRollQuaternionKeyed(
            float defaultValue,
            QuaternionComponent component,
            params (float Time, float RollDegrees)[] rolls)
        {
            (float Time, float Value)[] keys = new (float Time, float Value)[rolls.Length];
            for (int i = 0; i < rolls.Length; i++)
            {
                Quaternion rotation = Quaternion.Euler(0f, 0f, rolls[i].RollDegrees);
                keys[i] = (rolls[i].Time, component == QuaternionComponent.Z ? rotation.z : rotation.w);
            }

            return LinearValueKeyed(defaultValue, keys);
        }

        private static AnimationCurve EaseInOutKeyed(float defaultValue, params (float Time, float Value)[] keys)
        {
            List<Keyframe> frames = new List<Keyframe>();
            if (keys.Length == 0 || keys[0].Time > 0.0001f)
            {
                frames.Add(new Keyframe(0f, Mathf.Clamp01(defaultValue)));
            }

            for (int i = 0; i < keys.Length; i++)
            {
                frames.Add(new Keyframe(keys[i].Time, Mathf.Clamp01(keys[i].Value)));
            }

            if (frames.Count == 0 || frames[frames.Count - 1].time < TimelineDurationSeconds - 0.0001f)
            {
                frames.Add(new Keyframe(TimelineDurationSeconds, frames.Count == 0 ? Mathf.Clamp01(defaultValue) : frames[frames.Count - 1].value));
            }

            AnimationCurve curve = new AnimationCurve(frames.ToArray());
            for (int i = 0; i < curve.length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Auto);
                AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Auto);
            }

            return curve;
        }

        private static void SetCurve(AnimationClip clip, string propertyName, AnimationCurve curve)
        {
            clip.SetCurve(string.Empty, typeof(Transform), propertyName, curve);
        }

        private static GameObject InstantiateModel(string modelPath, string objectName)
        {
            GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            if (model == null)
            {
                throw new InvalidOperationException($"Missing model `{modelPath}`.");
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(model) as GameObject;
            if (instance == null)
            {
                throw new InvalidOperationException($"Failed to instantiate `{modelPath}`.");
            }

            PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            instance.name = objectName;
            RemoveColliders(instance);
            return instance;
        }

        private static void AssignMaterial(GameObject root, Material material)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(includeInactive: true);
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    renderers[i].sharedMaterial = material;
                    EditorUtility.SetDirty(renderers[i]);
                }
            }
        }

        private static void ScaleToMaxDimension(Transform root, float maxDimension)
        {
            Bounds bounds = CalculateBounds(root);
            float currentMax = Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z));
            if (currentMax <= 0.0001f)
            {
                return;
            }

            root.localScale *= maxDimension / currentMax;
        }

        private static void CreateThrusterGlow(Transform parent, string name, Vector3 localPosition, float scale)
        {
            Material material = LoadOrCreateUnlitMaterial(
                MaterialRoot + "/AF_BombingReview_ThrusterGlow.mat",
                new Color(1f, 0.48f, 0.12f, 0.74f),
                new Color(4.8f, 1.45f, 0.18f, 1f),
                transparent: true);
            CreateSphere(parent, name, localPosition, Quaternion.identity, new Vector3(scale, scale, scale * 1.8f), material);
        }

        private static void CreateParticleColumn(
            Transform parent,
            string objectName,
            Material material,
            int burstCount,
            float startSpeed)
        {
            GameObject objectRoot = new GameObject(objectName);
            objectRoot.transform.SetParent(parent, worldPositionStays: false);
            objectRoot.transform.localPosition = Vector3.zero;

            ParticleSystem particleSystem = objectRoot.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particleSystem.main;
            main.duration = 1.7f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.8f, 1.8f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(startSpeed * 0.52f, startSpeed);
            main.startSize = new ParticleSystem.MinMaxCurve(0.18f, 0.74f);
            main.startColor = material.color;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;

            ParticleSystem.EmissionModule emission = particleSystem.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)burstCount) });

            ParticleSystem.ShapeModule shape = particleSystem.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 22f;
            shape.radius = 0.55f;

            ParticleSystem.VelocityOverLifetimeModule velocity = particleSystem.velocityOverLifetime;
            velocity.enabled = true;
            velocity.y = new ParticleSystem.MinMaxCurve(0.8f, 2.2f);
            velocity.x = new ParticleSystem.MinMaxCurve(-0.75f, 0.75f);
            velocity.z = new ParticleSystem.MinMaxCurve(-0.75f, 0.75f);

            ParticleSystemRenderer renderer = objectRoot.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sharedMaterial = material;
        }

        private static void CreateShockRing(Transform parent, string objectName, Material material, float radius)
        {
            GameObject ring = new GameObject(objectName);
            ring.transform.SetParent(parent, worldPositionStays: false);
            ring.transform.SetLocalPositionAndRotation(new Vector3(0f, 0.08f, 0f), Quaternion.Euler(90f, 0f, 0f));
            ring.transform.localScale = Vector3.one * radius;
            MeshFilter filter = ring.AddComponent<MeshFilter>();
            filter.sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(ShockRingMeshPath);
            MeshRenderer renderer = ring.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
        }

        private static GameObject InstantiateGameOwnedVfx(
            string prefabPath,
            Transform parent,
            string objectName,
            Vector3 localPosition,
            Quaternion localRotation,
            Vector3 localScale)
        {
            if (!prefabPath.StartsWith("Assets/_Game/", StringComparison.Ordinal)
                || prefabPath.Contains("/_Imported/", StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Bombing review VFX must be game-owned: {prefabPath}");
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                throw new InvalidOperationException($"Missing game-owned VFX prefab `{prefabPath}`.");
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null)
            {
                throw new InvalidOperationException($"Failed to instantiate VFX prefab `{prefabPath}`.");
            }

            PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            if (prefabPath.Equals(AirstrikeBombExplosionPrefabPath, StringComparison.Ordinal))
            {
                RemoveObjectsContainingName(instance, "Distort");
            }
            instance.name = objectName;
            instance.transform.SetParent(parent, worldPositionStays: false);
            instance.transform.SetLocalPositionAndRotation(localPosition, localRotation);
            instance.transform.localScale = localScale;
            EditorUtility.SetDirty(instance);
            return instance;
        }

        private static GameObject CreatePanel(
            Transform parent,
            string objectName,
            Vector3 localPosition,
            Quaternion localRotation,
            Vector3 localScale,
            Material material)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = objectName;
            cube.transform.SetParent(parent, worldPositionStays: false);
            cube.transform.SetLocalPositionAndRotation(localPosition, localRotation);
            cube.transform.localScale = localScale;
            Renderer renderer = cube.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }

            RemoveColliders(cube);
            return cube;
        }

        private static GameObject CreateSphere(
            Transform parent,
            string objectName,
            Vector3 localPosition,
            Quaternion localRotation,
            Vector3 localScale,
            Material material)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = objectName;
            sphere.transform.SetParent(parent, worldPositionStays: false);
            sphere.transform.SetLocalPositionAndRotation(localPosition, localRotation);
            sphere.transform.localScale = localScale;
            Renderer renderer = sphere.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }

            RemoveColliders(sphere);
            return sphere;
        }

        private static void RemoveColliders(GameObject root)
        {
            Collider[] colliders = root.GetComponentsInChildren<Collider>(includeInactive: true);
            for (int i = colliders.Length - 1; i >= 0; i--)
            {
                UnityEngine.Object.DestroyImmediate(colliders[i]);
            }
        }

        private static Material LoadOrCreateLitMaterial(
            string path,
            Color color,
            Color emission,
            float smoothness,
            float metallic)
        {
            EnsureFolder(PathParent(path));
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit")
                    ?? Shader.Find("Standard");
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            SetMaterialBase(material, color, emission);
            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", smoothness);
            }
            if (material.HasProperty("_Metallic"))
            {
                material.SetFloat("_Metallic", metallic);
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material LoadOrCreateTextureMaterial(
            string path,
            string texturePath,
            Color color,
            Color emission,
            float smoothness,
            float metallic)
        {
            Material material = LoadOrCreateLitMaterial(path, color, emission, smoothness, metallic);
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            if (texture != null)
            {
                if (material.HasProperty("_BaseMap"))
                {
                    material.SetTexture("_BaseMap", texture);
                }
                if (material.HasProperty("_MainTex"))
                {
                    material.SetTexture("_MainTex", texture);
                }
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material LoadOrCreateUnlitMaterial(
            string path,
            Color color,
            Color emission,
            bool transparent)
        {
            EnsureFolder(PathParent(path));
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Unlit")
                    ?? Shader.Find("Unlit/Color")
                    ?? Shader.Find("Standard");
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            SetMaterialBase(material, color, emission);
            if (transparent)
            {
                ConfigureTransparentMaterial(material, color.a);
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static void SetMaterialBase(Material material, Color color, Color emission)
        {
            material.color = color;
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }
            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }

            if (emission.maxColorComponent > 0.001f)
            {
                material.EnableKeyword("_EMISSION");
                if (material.HasProperty("_EmissionColor"))
                {
                    material.SetColor("_EmissionColor", emission);
                }
            }
            else
            {
                material.DisableKeyword("_EMISSION");
            }
        }

        private static void ConfigureTransparentMaterial(Material material, float alpha)
        {
            Color color = material.color;
            color.a = alpha;
            material.color = color;
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }
            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }
            if (material.HasProperty("_Surface"))
            {
                material.SetFloat("_Surface", 1f);
            }
            if (material.HasProperty("_Blend"))
            {
                material.SetFloat("_Blend", 0f);
            }
            if (material.HasProperty("_SrcBlend"))
            {
                material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            }
            if (material.HasProperty("_DstBlend"))
            {
                material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            }
            if (material.HasProperty("_ZWrite"))
            {
                material.SetFloat("_ZWrite", 0f);
            }
            if (material.HasProperty("_Cull"))
            {
                material.SetFloat("_Cull", (float)UnityEngine.Rendering.CullMode.Off);
            }
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        }

        private static void EnsureShockRingMesh()
        {
            EnsureFolder(PathParent(ShockRingMeshPath));
            Mesh existing = AssetDatabase.LoadAssetAtPath<Mesh>(ShockRingMeshPath);
            if (existing != null)
            {
                return;
            }

            const int Segments = 96;
            const float InnerRadius = 0.72f;
            const float OuterRadius = 1.0f;
            Vector3[] vertices = new Vector3[Segments * 2];
            Vector2[] uvs = new Vector2[vertices.Length];
            int[] triangles = new int[Segments * 6];
            for (int i = 0; i < Segments; i++)
            {
                float angle = (i / (float)Segments) * Mathf.PI * 2f;
                float cos = Mathf.Cos(angle);
                float sin = Mathf.Sin(angle);
                vertices[i * 2] = new Vector3(cos * InnerRadius, sin * InnerRadius, 0f);
                vertices[i * 2 + 1] = new Vector3(cos * OuterRadius, sin * OuterRadius, 0f);
                uvs[i * 2] = new Vector2(0f, i / (float)Segments);
                uvs[i * 2 + 1] = new Vector2(1f, i / (float)Segments);

                int next = (i + 1) % Segments;
                int tri = i * 6;
                triangles[tri] = i * 2;
                triangles[tri + 1] = next * 2;
                triangles[tri + 2] = i * 2 + 1;
                triangles[tri + 3] = i * 2 + 1;
                triangles[tri + 4] = next * 2;
                triangles[tri + 5] = next * 2 + 1;
            }

            Mesh mesh = new Mesh
            {
                name = "AF_BombShockRing"
            };
            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh, ShockRingMeshPath);
        }

        private static void ValidateBombingReviewTimeline(
            bool writeReport,
            bool renderCaptures = true,
            bool runCoordinateAudit = false)
        {
            List<string> issues = new List<string>();
            Scene scene = EditorSceneManager.OpenScene(ReviewScenePath, OpenSceneMode.Single);
            TimelineAsset timeline = AssetDatabase.LoadAssetAtPath<TimelineAsset>(TimelinePath);
            if (timeline == null)
            {
                issues.Add($"Missing TimelineAsset at {TimelinePath}.");
            }

            PlayableDirector director = FindComponentInScene<PlayableDirector>(scene);
            if (director == null)
            {
                issues.Add("Missing PlayableDirector.");
            }
            else if (director.playableAsset != timeline)
            {
                issues.Add("PlayableDirector is not bound to the bombing review TimelineAsset.");
            }

            Camera camera = FindComponentInScene<Camera>(scene);
            CinemachineBrain brain = camera != null ? camera.GetComponent<CinemachineBrain>() : null;
            if (camera == null || brain == null)
            {
                issues.Add("Missing main Camera or CinemachineBrain.");
            }

            if (timeline != null)
            {
                RequireTrack<CinemachineTrack>(timeline, "Cinemachine Shots", issues);
                RequireTrack<AnimationTrack>(timeline, "Aircraft Formation Move", issues);
                RequireTrack<AnimationTrack>(timeline, "Bomber Lead Entry", issues);
                RequireTrack<AnimationTrack>(timeline, "Left Escort Join", issues);
                RequireTrack<AnimationTrack>(timeline, "Right Escort Join", issues);
                RequireTrack<AnimationTrack>(timeline, "Bomb Drop Move", issues);
                RequireTrack<AnimationTrack>(timeline, "Impact Camera Recoil", issues);
                RequireTrack<AnimationTrack>(timeline, "Black Screen Transition", issues);
                RequireTrack<ActivationTrack>(timeline, "Cloud Deck Active", issues);
                RequireTrack<ActivationTrack>(timeline, "Bombs Active", issues);
                RequireTrack<ActivationTrack>(timeline, "Impact Chain Active", issues);
                RequireTrack<ActivationTrack>(timeline, "Impact Burst 01 Active", issues);
                RequireTrack<ActivationTrack>(timeline, "Impact Burst 02 Active", issues);
                RequireTrack<ActivationTrack>(timeline, "Impact Burst 03 Active", issues);
                RequireTrack<ActivationTrack>(timeline, "Impact Burst 04 Active", issues);
                RequireTrack<ActivationTrack>(timeline, "Aftermath Smoke Active", issues);
                if (Math.Abs(timeline.fixedDuration - TimelineDurationSeconds) > 0.01d)
                {
                    issues.Add($"Timeline duration is {timeline.fixedDuration:0.###}, expected {TimelineDurationSeconds:0.###}.");
                }
            }

            string[] dependencyRoots =
            {
                ReviewScenePath,
                TimelinePath,
                AnimationRoot,
                MaterialRoot,
                MeshRoot,
                ModelRoot,
                TextureRoot,
                VfxRoot
            };
            foreach (string dependency in AssetDatabase.GetDependencies(dependencyRoots, recursive: true))
            {
                string normalized = dependency.Replace('\\', '/');
                if (normalized.Contains("/_Imported/", StringComparison.Ordinal))
                {
                    issues.Add($"Generated bombing review asset depends on raw imported asset: {dependency}");
                }
            }

            if (runCoordinateAudit && director != null)
            {
                ValidateFormationCoordinates(scene, director, issues);
            }

            if (issues.Count == 0 && renderCaptures && director != null && camera != null)
            {
                RenderCaptures(scene, director, camera, CaptureSpecs);
            }

            if (writeReport)
            {
                WriteReport(issues, timeline, renderCaptures, runCoordinateAudit);
            }

            if (issues.Count > 0)
            {
                throw new InvalidOperationException("Intro GatePod bombing review validation failed:\n" + string.Join("\n", issues));
            }
        }

        private static void RenderCaptures(
            Scene scene,
            PlayableDirector director,
            Camera camera,
            CaptureSpec[] specs)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath) ?? "C:/tmp");
            for (int i = 0; i < specs.Length; i++)
            {
                director.time = specs[i].TimeSeconds;
                director.Evaluate();
                ApplyManualCinemachineSample(scene, camera, specs[i].TimeSeconds);
                float warmupSeconds = specs[i].TimeSeconds >= 5.85f ? 1.85f : 0.85f;
                AdvanceVisibleParticleSystems(scene, warmupSeconds);
                AdvanceVisibleVisualEffects(scene, warmupSeconds);
                SceneView.RepaintAll();
                camera.Render();
                RenderCamera(camera, specs[i].Path);
            }
        }

        private static void AdvanceVisibleParticleSystems(Scene scene, float seconds)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                ParticleSystem[] particleSystems =
                    roots[i].GetComponentsInChildren<ParticleSystem>(includeInactive: false);
                for (int j = 0; j < particleSystems.Length; j++)
                {
                    ParticleSystem particleSystem = particleSystems[j];
                    if (particleSystem == null || !particleSystem.gameObject.activeInHierarchy)
                    {
                        continue;
                    }

                    particleSystem.Stop(withChildren: false, ParticleSystemStopBehavior.StopEmittingAndClear);
                    particleSystem.Simulate(seconds, withChildren: false, restart: true, fixedTimeStep: false);
                    particleSystem.Play(withChildren: false);
                }
            }
        }

        private static void AdvanceVisibleVisualEffects(Scene scene, float seconds)
        {
            int frames = Mathf.Max(1, Mathf.CeilToInt(seconds * 30f));
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                VisualEffect[] effects = roots[i].GetComponentsInChildren<VisualEffect>(includeInactive: false);
                for (int j = 0; j < effects.Length; j++)
                {
                    VisualEffect effect = effects[j];
                    if (effect == null || !effect.gameObject.activeInHierarchy)
                    {
                        continue;
                    }

                    effect.Reinit();
                    for (int frame = 0; frame < frames; frame++)
                    {
                        effect.AdvanceOneFrame();
                    }
                }
            }
        }

        private static void ApplyManualCinemachineSample(Scene scene, Camera camera, float timeSeconds)
        {
            string shotName = timeSeconds < BombReleaseStartSeconds
                ? "cm_01_formation_join"
                : timeSeconds < 3.56f
                    ? "cm_02_bomb_release"
                    : timeSeconds < 4.22f
                        ? "cm_03_falling_payload"
                        : timeSeconds < 4.68f
                            ? "cm_04_target_reframe"
                            : timeSeconds < 5.85f
                                ? "cm_05_impact_chain"
                                : timeSeconds < 6.80f
                                    ? "cm_06_aftershock"
                                    : "cm_07_smoke_handoff";
            GameObject shotObject = FindObjectInScene(scene, shotName);
            if (shotObject == null)
            {
                return;
            }

            CinemachineCamera cinemachineCamera = shotObject.GetComponent<CinemachineCamera>();
            camera.transform.SetPositionAndRotation(shotObject.transform.position, shotObject.transform.rotation);
            if (cinemachineCamera != null)
            {
                camera.fieldOfView = cinemachineCamera.Lens.FieldOfView;
                camera.nearClipPlane = cinemachineCamera.Lens.NearClipPlane;
                camera.farClipPlane = cinemachineCamera.Lens.FarClipPlane;
            }
        }

        private static void RenderCamera(Camera camera, string path)
        {
            RenderTexture previousTarget = camera.targetTexture;
            RenderTexture previousActive = RenderTexture.active;
            RenderTexture renderTexture = new RenderTexture(CaptureWidth, CaptureHeight, 24, RenderTextureFormat.ARGB32);
            Texture2D texture = new Texture2D(CaptureWidth, CaptureHeight, TextureFormat.RGB24, false);
            try
            {
                camera.targetTexture = renderTexture;
                RenderTexture.active = renderTexture;
                camera.Render();
                texture.ReadPixels(new Rect(0f, 0f, CaptureWidth, CaptureHeight), 0, 0);
                texture.Apply();
                Directory.CreateDirectory(Path.GetDirectoryName(path) ?? "C:/tmp");
                File.WriteAllBytes(path, texture.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                UnityEngine.Object.DestroyImmediate(renderTexture);
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        private static void ValidateFormationCoordinates(
            Scene scene,
            PlayableDirector director,
            List<string> issues)
        {
            List<string> coordinateIssues = new List<string>();
            List<FormationCoordinateSample> samples = new List<FormationCoordinateSample>();
            GameObject aircraftRootObject = FindObjectInScene(scene, AircraftRootName);
            GameObject bombDropObject = FindObjectInScene(scene, BombDropRootName);
            if (aircraftRootObject == null)
            {
                coordinateIssues.Add($"Missing `{AircraftRootName}`.");
                WriteFormationCoordinateAudit(samples, coordinateIssues);
                issues.AddRange(coordinateIssues);
                return;
            }

            Transform aircraftRoot = aircraftRootObject.transform;
            Transform bomber = aircraftRoot.Find("Bomber_Leader");
            Transform left = aircraftRoot.Find("Jet_Escort_Left");
            Transform right = aircraftRoot.Find("Jet_Escort_Right");
            if (bomber == null || left == null || right == null)
            {
                coordinateIssues.Add("Missing one or more aircraft transforms under the formation root.");
                WriteFormationCoordinateAudit(samples, coordinateIssues);
                issues.AddRange(coordinateIssues);
                return;
            }

            float[] sampleTimes =
            {
                0.35f,
                1.85f,
                2.75f,
                FormationLockSampleSeconds,
                BombReleaseStartSeconds + 0.23f,
                5.16f,
                7.40f
            };

            for (int i = 0; i < sampleTimes.Length; i++)
            {
                samples.Add(SampleFormationCoordinate(
                    director,
                    sampleTimes[i],
                    aircraftRoot,
                    bomber,
                    left,
                    right,
                    bombDropObject));
            }

            FormationCoordinateSample midJoin = samples[1];
            if (midJoin.Left.x > -9.50f)
            {
                coordinateIssues.Add(
                    $"Left escort joins too quickly at {midJoin.TimeSeconds:0.00}s: x={midJoin.Left.x:0.00}, expected <= -9.50 before final lock.");
            }

            if (midJoin.Right.x < 9.50f)
            {
                coordinateIssues.Add(
                    $"Right escort joins too quickly at {midJoin.TimeSeconds:0.00}s: x={midJoin.Right.x:0.00}, expected >= 9.50 before final lock.");
            }

            FormationCoordinateSample preLock = samples[2];
            if (preLock.LeftBomberDistance < MinimumAircraftSeparation + 0.75f
                || preLock.RightBomberDistance < MinimumAircraftSeparation + 0.75f)
            {
                coordinateIssues.Add(
                    $"Escorts crowd the bomber before lock at {preLock.TimeSeconds:0.00}s: left={preLock.LeftBomberDistance:0.00}, right={preLock.RightBomberDistance:0.00}.");
            }

            FormationCoordinateSample lockSample = samples[3];
            if (lockSample.Root.z < 2.50f)
            {
                coordinateIssues.Add(
                    $"Formation root is not advancing enough by lock time: z={lockSample.Root.z:0.00}, expected >= 2.50.");
            }

            if (lockSample.Root.z - samples[0].Root.z < 25.00f)
            {
                coordinateIssues.Add(
                    $"Formation root forward delta is too small by lock time: delta={lockSample.Root.z - samples[0].Root.z:0.00}, expected >= 25.00.");
            }

            float minRootX = samples[0].Root.x;
            float maxRootX = samples[0].Root.x;
            float minRootRoll = samples[0].RootRollZ;
            float maxRootRoll = samples[0].RootRollZ;
            for (int i = 1; i < samples.Count; i++)
            {
                minRootX = Mathf.Min(minRootX, samples[i].Root.x);
                maxRootX = Mathf.Max(maxRootX, samples[i].Root.x);
                minRootRoll = Mathf.Min(minRootRoll, samples[i].RootRollZ);
                maxRootRoll = Mathf.Max(maxRootRoll, samples[i].RootRollZ);
            }

            if (maxRootX - minRootX < 0.35f)
            {
                coordinateIssues.Add(
                    $"Formation root lateral drift is too flat: range={maxRootX - minRootX:0.00}, expected >= 0.35.");
            }

            if (maxRootRoll - minRootRoll < 1.20f)
            {
                coordinateIssues.Add(
                    $"Formation root bank roll is too flat: range={maxRootRoll - minRootRoll:0.00} degrees, expected >= 1.20.");
            }

            ValidatePositionNear(lockSample.Bomber, Vector3.zero, 0.25f, "Bomber locked formation position", coordinateIssues);
            ValidatePositionNear(
                lockSample.Left,
                new Vector3(-FormationWingX, FormationWingY, FormationWingZ),
                0.35f,
                "Left escort locked formation position",
                coordinateIssues);
            ValidatePositionNear(
                lockSample.Right,
                new Vector3(FormationWingX, FormationWingY, FormationWingZ),
                0.35f,
                "Right escort locked formation position",
                coordinateIssues);

            if (lockSample.LeftBomberDistance < MinimumAircraftSeparation)
            {
                coordinateIssues.Add(
                    $"Left escort overlaps bomber at formation lock: separation={lockSample.LeftBomberDistance:0.00}, expected >= {MinimumAircraftSeparation:0.00}.");
            }

            if (lockSample.RightBomberDistance < MinimumAircraftSeparation)
            {
                coordinateIssues.Add(
                    $"Right escort overlaps bomber at formation lock: separation={lockSample.RightBomberDistance:0.00}, expected >= {MinimumAircraftSeparation:0.00}.");
            }

            if (lockSample.LeftRightDistance < MinimumAircraftSeparation * 2f)
            {
                coordinateIssues.Add(
                    $"Escorts are too close to each other at formation lock: separation={lockSample.LeftRightDistance:0.00}, expected >= {MinimumAircraftSeparation * 2f:0.00}.");
            }

            if (lockSample.BombsActive)
            {
                coordinateIssues.Add(
                    $"Bomb drop root is active before formation lock finishes at {lockSample.TimeSeconds:0.00}s.");
            }

            FormationCoordinateSample releaseSample = samples[4];
            if (releaseSample.Root.z <= lockSample.Root.z + 1.20f)
            {
                coordinateIssues.Add(
                    $"Formation root stalls after lock: lockZ={lockSample.Root.z:0.00}, releaseZ={releaseSample.Root.z:0.00}, expected release at least 1.20 farther forward.");
            }

            if (!releaseSample.BombsActive)
            {
                coordinateIssues.Add(
                    $"Bomb drop root is not active after bomb release start at {releaseSample.TimeSeconds:0.00}s.");
            }

            float releaseBombZDelta = Mathf.Abs(releaseSample.BombRoot.z - releaseSample.Root.z);
            float releaseBombVerticalDrop = releaseSample.Root.y - releaseSample.BombRoot.y;
            if (releaseBombZDelta > 2.35f)
            {
                coordinateIssues.Add(
                    $"Bomb drop starts too far from aircraft at release: aircraftZ={releaseSample.Root.z:0.00}, bombRootZ={releaseSample.BombRoot.z:0.00}, delta={releaseBombZDelta:0.00}, expected <= 2.35.");
            }

            if (releaseBombVerticalDrop < 1.10f || releaseBombVerticalDrop > 3.20f)
            {
                coordinateIssues.Add(
                    $"Bomb drop does not read as under-aircraft at release: aircraftY={releaseSample.Root.y:0.00}, bombRootY={releaseSample.BombRoot.y:0.00}, verticalDrop={releaseBombVerticalDrop:0.00}, expected 1.10-3.20.");
            }

            FormationCoordinateSample postLockEarly = samples[5];
            if (postLockEarly.BombsActive && postLockEarly.BombRoot.y > 3.25f)
            {
                coordinateIssues.Add(
                    $"Bombs have not visibly fallen by {postLockEarly.TimeSeconds:0.00}s: bombRootY={postLockEarly.BombRoot.y:0.00}, expected <= 3.25.");
            }

            FormationCoordinateSample postLockLate = samples[6];
            ValidatePositionNear(postLockEarly.Bomber, Vector3.zero, 0.08f, "Post-lock bomber stable position", coordinateIssues);
            ValidatePositionNear(
                postLockEarly.Left,
                new Vector3(-FormationWingX, FormationWingY, FormationWingZ),
                0.08f,
                "Post-lock left escort stable position",
                coordinateIssues);
            ValidatePositionNear(
                postLockEarly.Right,
                new Vector3(FormationWingX, FormationWingY, FormationWingZ),
                0.08f,
                "Post-lock right escort stable position",
                coordinateIssues);
            ValidatePositionNear(postLockLate.Bomber, Vector3.zero, 0.08f, "Late post-lock bomber stable position", coordinateIssues);
            ValidatePositionNear(
                postLockLate.Left,
                new Vector3(-FormationWingX, FormationWingY, FormationWingZ),
                0.08f,
                "Late post-lock left escort stable position",
                coordinateIssues);
            ValidatePositionNear(
                postLockLate.Right,
                new Vector3(FormationWingX, FormationWingY, FormationWingZ),
                0.08f,
                "Late post-lock right escort stable position",
                coordinateIssues);

            float postLockRootXRange = Max3(lockSample.Root.x, postLockEarly.Root.x, postLockLate.Root.x)
                - Min3(lockSample.Root.x, postLockEarly.Root.x, postLockLate.Root.x);
            float postLockRollRange = Max3(lockSample.RootRollZ, postLockEarly.RootRollZ, postLockLate.RootRollZ)
                - Min3(lockSample.RootRollZ, postLockEarly.RootRollZ, postLockLate.RootRollZ);
            if (postLockRootXRange > 0.18f)
            {
                coordinateIssues.Add(
                    $"Formation root keeps weaving after lock: post-lock x range={postLockRootXRange:0.00}, expected <= 0.18.");
            }

            if (postLockRollRange > 0.85f)
            {
                coordinateIssues.Add(
                    $"Formation root keeps banking after lock: post-lock roll range={postLockRollRange:0.00} degrees, expected <= 0.85.");
            }

            WriteFormationCoordinateAudit(samples, coordinateIssues);
            issues.AddRange(coordinateIssues);
        }

        private static FormationCoordinateSample SampleFormationCoordinate(
            PlayableDirector director,
            float timeSeconds,
            Transform aircraftRoot,
            Transform bomber,
            Transform left,
            Transform right,
            GameObject bombDropObject)
        {
            director.time = timeSeconds;
            director.Evaluate();
            Vector3 rootPosition = aircraftRoot.localPosition;
            float rootRollZ = NormalizeSignedAngle(aircraftRoot.localEulerAngles.z);
            Vector3 bomberPosition = bomber.localPosition;
            Vector3 leftPosition = left.localPosition;
            Vector3 rightPosition = right.localPosition;
            Vector3 bombRootPosition = bombDropObject != null
                ? bombDropObject.transform.localPosition
                : Vector3.zero;
            return new FormationCoordinateSample(
                timeSeconds,
                rootPosition,
                rootRollZ,
                bomberPosition,
                leftPosition,
                rightPosition,
                bombRootPosition,
                DistanceXZ(bomberPosition, leftPosition),
                DistanceXZ(bomberPosition, rightPosition),
                DistanceXZ(leftPosition, rightPosition),
                bombDropObject != null && bombDropObject.activeInHierarchy);
        }

        private static void ValidatePositionNear(
            Vector3 actual,
            Vector3 expected,
            float tolerance,
            string label,
            List<string> coordinateIssues)
        {
            float distance = Vector3.Distance(actual, expected);
            if (distance > tolerance)
            {
                coordinateIssues.Add(
                    $"{label} is off by {distance:0.00}: actual={FormatVector(actual)}, expected={FormatVector(expected)}, tolerance={tolerance:0.00}.");
            }
        }

        private static float DistanceXZ(Vector3 a, Vector3 b)
        {
            float dx = a.x - b.x;
            float dz = a.z - b.z;
            return Mathf.Sqrt(dx * dx + dz * dz);
        }

        private static float Min3(float a, float b, float c)
        {
            return Mathf.Min(a, Mathf.Min(b, c));
        }

        private static float Max3(float a, float b, float c)
        {
            return Mathf.Max(a, Mathf.Max(b, c));
        }

        private static void WriteFormationCoordinateAudit(
            List<FormationCoordinateSample> samples,
            List<string> coordinateIssues)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# Bombing Review Formation Coordinate Audit");
            builder.AppendLine();
            builder.AppendLine(coordinateIssues.Count == 0 ? "Status: PASS" : "Status: FAIL");
            builder.AppendLine();
            builder.AppendLine($"- Expected locked bomber local position: `{FormatVector(Vector3.zero)}`");
            builder.AppendLine($"- Expected locked left escort local position: `{FormatVector(new Vector3(-FormationWingX, FormationWingY, FormationWingZ))}`");
            builder.AppendLine($"- Expected locked right escort local position: `{FormatVector(new Vector3(FormationWingX, FormationWingY, FormationWingZ))}`");
            builder.AppendLine($"- Minimum bomber/escort XZ separation: `{MinimumAircraftSeparation:0.00}`");
            builder.AppendLine("- Formation root must advance forward while the aircraft keep locked local spacing.");
            builder.AppendLine("- Formation root may bank during join, then must settle after lock instead of periodic weaving.");
            builder.AppendLine($"- Left join window: `{LeftEscortJoinStartSeconds:0.00}s-{LeftEscortJoinEndSeconds:0.00}s`");
            builder.AppendLine($"- Right join window: `{RightEscortJoinStartSeconds:0.00}s-{RightEscortJoinEndSeconds:0.00}s`");
            builder.AppendLine($"- Bomb release starts after formation lock: `{BombReleaseStartSeconds:0.00}s`");
            builder.AppendLine("- Bomb drop root must begin close under the aircraft, then fall before the impact cut.");
            builder.AppendLine();
            builder.AppendLine("## Samples");
            builder.AppendLine("| Time | Root Local | Root Roll Z | Bomber Local | Left Local | Right Local | Bomb Root | B-L XZ | B-R XZ | L-R XZ | Bombs Active |");
            builder.AppendLine("| ---: | --- | ---: | --- | --- | --- | --- | ---: | ---: | ---: | --- |");
            for (int i = 0; i < samples.Count; i++)
            {
                FormationCoordinateSample sample = samples[i];
                builder.AppendLine(
                    $"| {sample.TimeSeconds:0.00}s | `{FormatVector(sample.Root)}` | {sample.RootRollZ:0.00} | `{FormatVector(sample.Bomber)}` | `{FormatVector(sample.Left)}` | `{FormatVector(sample.Right)}` | `{FormatVector(sample.BombRoot)}` | {sample.LeftBomberDistance:0.00} | {sample.RightBomberDistance:0.00} | {sample.LeftRightDistance:0.00} | `{sample.BombsActive}` |");
            }

            if (coordinateIssues.Count > 0)
            {
                builder.AppendLine();
                builder.AppendLine("## Issues");
                for (int i = 0; i < coordinateIssues.Count; i++)
                {
                    builder.AppendLine($"- {coordinateIssues[i]}");
                }
            }

            Directory.CreateDirectory(Path.GetDirectoryName(CoordinateAuditPath) ?? "C:/tmp");
            File.WriteAllText(CoordinateAuditPath, builder.ToString(), Encoding.UTF8);
        }

        private static string FormatVector(Vector3 value)
        {
            return $"{value.x:0.00}, {value.y:0.00}, {value.z:0.00}";
        }

        private static float NormalizeSignedAngle(float angle)
        {
            return Mathf.DeltaAngle(0f, angle);
        }

        private static void WriteReport(
            List<string> issues,
            TimelineAsset timeline,
            bool renderCaptures,
            bool runCoordinateAudit)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# Intro GatePod Bombing Review Verification");
            builder.AppendLine();
            builder.AppendLine(issues.Count == 0 ? "Status: PASS" : "Status: FAIL");
            builder.AppendLine();
            builder.AppendLine($"- Scene: `{ReviewScenePath}`");
            builder.AppendLine($"- Timeline: `{TimelinePath}`");
            builder.AppendLine($"- Duration: `{(timeline != null ? timeline.fixedDuration : 0d):0.###}s`");
            builder.AppendLine("- Protected intro Timeline/Profile/first-person scene are not edited by this generator.");
            builder.AppendLine("- Aircraft and bomb materials use promoted Protofactor Sci-Fi panel textures under `_Game`.");
            builder.AppendLine("- Promoted aircraft/bomb models and textures live under `_Game`; generated review dependencies reject `_Imported`.");
            builder.AppendLine("- Shot rhythm pass follows the local ArkData evidence: a held opening formation beat, then compressed bomb-release/impact beats with additive recoil instead of frantic early cutting.");
            builder.AppendLine("- Transition pass uses only opening/outro curtain; impact flash, pre-bomb red warning UI, and Distort screen cover are intentionally removed.");
            if (runCoordinateAudit)
            {
                builder.AppendLine($"- Formation coordinate audit: `{CoordinateAuditPath}`");
            }
            builder.AppendLine($"- Promoted explosion prefab/material/texture audit: `{ExplosionAuditPath}`");
            builder.AppendLine();
            builder.AppendLine("## Captures");
            if (!renderCaptures)
            {
                builder.AppendLine("- Skipped for this batch run; formation was judged from Timeline-evaluated transform coordinates.");
            }
            else
            {
                for (int i = 0; i < CaptureSpecs.Length; i++)
                {
                    builder.AppendLine($"- `{CaptureSpecs[i].Name}` at `{CaptureSpecs[i].TimeSeconds:0.###}s`: `{CaptureSpecs[i].Path}`");
                }
            }

            if (issues.Count > 0)
            {
                builder.AppendLine();
                builder.AppendLine("## Issues");
                for (int i = 0; i < issues.Count; i++)
                {
                    builder.AppendLine($"- {issues[i]}");
                }
            }

            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath) ?? "C:/tmp");
            File.WriteAllText(ReportPath, builder.ToString(), Encoding.UTF8);
        }

        private static T RequireTrack<T>(TimelineAsset timeline, string trackName, List<string> issues)
            where T : TrackAsset
        {
            T track = FindTimelineTrack<T>(timeline, trackName);
            if (track == null)
            {
                issues.Add($"Timeline is missing {typeof(T).Name} `{trackName}`.");
            }
            else if (track.GetClips().GetEnumerator().MoveNext() == false)
            {
                issues.Add($"Timeline track `{trackName}` has no clips.");
            }

            return track;
        }

        private static T FindTimelineTrack<T>(TimelineAsset timeline, string trackName)
            where T : TrackAsset
        {
            foreach (TrackAsset track in timeline.GetOutputTracks())
            {
                if (track == null)
                {
                    continue;
                }

                if (track is T typed && string.Equals(track.name, trackName, StringComparison.Ordinal))
                {
                    return typed;
                }
            }

            return null;
        }

        private static T FindComponentInScene<T>(Scene scene)
            where T : Component
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                T component = roots[i].GetComponentInChildren<T>(includeInactive: true);
                if (component != null)
                {
                    return component;
                }
            }

            return null;
        }

        private static GameObject FindObjectInScene(Scene scene, string objectName)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                GameObject found = FindObjectRecursive(roots[i].transform, objectName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static GameObject FindObjectRecursive(Transform root, string objectName)
        {
            if (root == null)
            {
                return null;
            }

            if (string.Equals(root.name, objectName, StringComparison.Ordinal))
            {
                return root.gameObject;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                GameObject found = FindObjectRecursive(root.GetChild(i), objectName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static Bounds CalculateBounds(Transform root)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(includeInactive: true);
            if (renderers.Length == 0)
            {
                return new Bounds(root.position, Vector3.one);
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            return bounds;
        }

        private static Quaternion ResolveLookRotation(Vector3 position, Vector3 lookAt)
        {
            Vector3 forward = lookAt - position;
            if (forward.sqrMagnitude < 0.0001f)
            {
                forward = Vector3.forward;
            }

            return Quaternion.LookRotation(forward.normalized, Vector3.up);
        }

        private static string SanitizeAssetName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "Asset";
            }

            StringBuilder builder = new StringBuilder(value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                char character = value[i];
                bool allowed = char.IsLetterOrDigit(character)
                    || character == '_'
                    || character == '-'
                    || character == '.';
                builder.Append(allowed ? character : '_');
            }

            string result = builder.ToString().Trim('_', '.', '-');
            return string.IsNullOrWhiteSpace(result) ? "Asset" : result;
        }

        private static void EnsureFolder(string folder)
        {
            if (string.IsNullOrWhiteSpace(folder) || AssetDatabase.IsValidFolder(folder))
            {
                return;
            }

            string parent = PathParent(folder);
            EnsureFolder(parent);
            string name = Path.GetFileName(folder);
            AssetDatabase.CreateFolder(parent, name);
        }

        private static string PathParent(string path)
        {
            string normalized = path.Replace('\\', '/');
            int index = normalized.LastIndexOf('/');
            return index > 0 ? normalized.Substring(0, index) : "Assets";
        }

        private readonly struct CameraShot
        {
            public CameraShot(string shotId, float startSeconds, float durationSeconds, CinemachineCamera camera, Animator animator)
            {
                ShotId = shotId;
                StartSeconds = startSeconds;
                DurationSeconds = durationSeconds;
                Camera = camera;
                Animator = animator;
            }

            public readonly string ShotId;
            public readonly float StartSeconds;
            public readonly float DurationSeconds;
            public readonly CinemachineCamera Camera;
            public readonly Animator Animator;
        }

        private readonly struct TransitionOverlayBindings
        {
            public TransitionOverlayBindings(Animator curtainAnimator)
            {
                CurtainAnimator = curtainAnimator;
            }

            public readonly Animator CurtainAnimator;
        }

        private readonly struct BombingPreludeSceneBindings
        {
            public BombingPreludeSceneBindings(
                GameObject root,
                CameraShot[] shots,
                Transform aircraftRoot,
                Transform cloudRoot,
                Transform bombDropRoot,
                Transform impactRoot,
                Transform smokeRoot,
                TransitionOverlayBindings transitionOverlay)
            {
                Root = root;
                Shots = shots;
                AircraftRoot = aircraftRoot;
                CloudRoot = cloudRoot;
                BombDropRoot = bombDropRoot;
                ImpactRoot = impactRoot;
                SmokeRoot = smokeRoot;
                TransitionOverlay = transitionOverlay;
            }

            public readonly GameObject Root;
            public readonly CameraShot[] Shots;
            public readonly Transform AircraftRoot;
            public readonly Transform CloudRoot;
            public readonly Transform BombDropRoot;
            public readonly Transform ImpactRoot;
            public readonly Transform SmokeRoot;
            public readonly TransitionOverlayBindings TransitionOverlay;
        }

        private enum QuaternionComponent
        {
            Z,
            W
        }

        private readonly struct ExplosionCandidate
        {
            public ExplosionCandidate(string label, string sourcePath, string targetPath, string note)
            {
                Label = label;
                SourcePath = sourcePath;
                TargetPath = targetPath;
                Note = note;
            }

            public readonly string Label;
            public readonly string SourcePath;
            public readonly string TargetPath;
            public readonly string Note;
        }

        private readonly struct FormationCoordinateSample
        {
            public FormationCoordinateSample(
                float timeSeconds,
                Vector3 root,
                float rootRollZ,
                Vector3 bomber,
                Vector3 left,
                Vector3 right,
                Vector3 bombRoot,
                float leftBomberDistance,
                float rightBomberDistance,
                float leftRightDistance,
                bool bombsActive)
            {
                TimeSeconds = timeSeconds;
                Root = root;
                RootRollZ = rootRollZ;
                Bomber = bomber;
                Left = left;
                Right = right;
                BombRoot = bombRoot;
                LeftBomberDistance = leftBomberDistance;
                RightBomberDistance = rightBomberDistance;
                LeftRightDistance = leftRightDistance;
                BombsActive = bombsActive;
            }

            public readonly float TimeSeconds;
            public readonly Vector3 Root;
            public readonly float RootRollZ;
            public readonly Vector3 Bomber;
            public readonly Vector3 Left;
            public readonly Vector3 Right;
            public readonly Vector3 BombRoot;
            public readonly float LeftBomberDistance;
            public readonly float RightBomberDistance;
            public readonly float LeftRightDistance;
            public readonly bool BombsActive;
        }

        private readonly struct CaptureSpec
        {
            public CaptureSpec(string name, float timeSeconds, string path)
            {
                Name = name;
                TimeSeconds = timeSeconds;
                Path = path;
            }

            public readonly string Name;
            public readonly float TimeSeconds;
            public readonly string Path;
        }
    }
}
