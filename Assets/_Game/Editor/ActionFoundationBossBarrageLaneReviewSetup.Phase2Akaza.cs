using System;
using System.Collections.Generic;
using System.IO;
using DimensionBrawl.Combat;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Player;
using DimensionBrawl.Presentation;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace DimensionBrawl.Editor
{
    public static partial class ActionFoundationBossBarrageLaneReviewSetup
    {
        public const string Phase2AkazaReviewScenePath =
            "Assets/_Game/Scenes/ActionFoundationPhase2BossAkazaReview.unity";

        private const string Phase2AkazaArtRoot = "Assets/_Game/Art/Characters/Bosses/Akaza";
        private const string Phase2AkazaTextureRoot = Phase2AkazaArtRoot + "/Textures";
        private const string Phase2AkazaMaterialRoot = Phase2AkazaArtRoot + "/Materials";
        private const string Phase2AkazaModelPath = Phase2AkazaArtRoot + "/Models/Akaza_model.fbx";
        private const string Phase2AkazaAnimationRoot = Phase2AkazaArtRoot + "/Animations";
        private const string Phase2AkazaAnimationSourceRoot = Phase2AkazaAnimationRoot + "/Source";
        private const string Phase2AkazaAnimationSanitizedRoot = Phase2AkazaAnimationRoot + "/Sanitized";
        private const string Phase2AkazaIntroSourceRoot = Phase2AkazaArtRoot + "/IntroSource";
        private const string Phase2AkazaIntroSourceModelRoot = Phase2AkazaIntroSourceRoot + "/Models";
        private const string Phase2AkazaIntroSourceTextureRoot = Phase2AkazaIntroSourceRoot + "/Textures";
        private const string Phase2AkazaIntroSourceMaterialRoot = Phase2AkazaIntroSourceRoot + "/Materials";
        private const string Phase2AkazaIntroSourceProfileRoot = Phase2AkazaIntroSourceRoot + "/Profiles";
        private const string Phase2AkazaIntroSourceMeshRoot = Phase2AkazaIntroSourceRoot + "/Meshes";
        private const string Phase2AkazaC08OriginalTextureRoot =
            Phase2AkazaIntroSourceRoot + "/C08Original/Textures";
        private const string Phase2AkazaC08CutoutTextureRoot =
            Phase2AkazaIntroSourceRoot + "/C08Original/CutoutTextures";
        private const string Phase2AkazaIntroSourceSkinTexturePath =
            Phase2AkazaIntroSourceTextureRoot + "/Unity2016_C_Skin.psd";
        private const string Phase2AkazaIntroSourceFaceTexturePath =
            Phase2AkazaIntroSourceTextureRoot + "/face3_main.psd";
        private const string Phase2AkazaIntroSourceBodyTexturePath =
            Phase2AkazaIntroSourceTextureRoot + "/Unity2016_C_Body.psd";
        private const string Phase2AkazaIntroSourceAddTexturePath =
            Phase2AkazaIntroSourceTextureRoot + "/Unity2016_C_Add.psd";
        private const string Phase2AkazaIntroSourceHairTexturePath =
            Phase2AkazaIntroSourceTextureRoot + "/Unity2016_C_Hair.psd";
        private const string Phase2AkazaIntroSourceHairSpowTexturePath =
            Phase2AkazaIntroSourceTextureRoot + "/Unity2016_C_Hair_Spow.psd";
        private const string Phase2AkazaIntroSourceWeaponTexturePath =
            Phase2AkazaIntroSourceTextureRoot + "/Unity2016_Wep.psd";
        private const string Phase2AkazaIntroSourceGateTexturePath =
            Phase2AkazaIntroSourceTextureRoot + "/gate.psd";
        private const string Phase2AkazaIntroSourceEfx02TexturePath =
            Phase2AkazaIntroSourceTextureRoot + "/efx_02.psd";
        private const string Phase2AkazaC08FaceShadowOverlayTexturePath =
            Phase2AkazaIntroSourceTextureRoot + "/DB_C08_FaceShadowOverlay.png";
        private const string Phase2AkazaC08OriginalSkyLoopTexturePath =
            Phase2AkazaC08OriginalTextureRoot + "/unity_sora_loop.psd";
        private const string Phase2AkazaC08OriginalSkinTexturePath =
            Phase2AkazaC08OriginalTextureRoot + "/Akaza_Skin.psd";
        private const string Phase2AkazaC08CutoutSkinTexturePath =
            Phase2AkazaC08CutoutTextureRoot + "/Akaza_Skin_cutout.png";
        private const string Phase2AkazaC08OriginalSkinShadowTexturePath =
            Phase2AkazaC08OriginalTextureRoot + "/Akaza_Skin_Sdw.psd";
        private const string Phase2AkazaC08CutoutSkinShadowTexturePath =
            Phase2AkazaC08CutoutTextureRoot + "/Akaza_Skin_Sdw_cutout.png";
        private const string Phase2AkazaC08OriginalFaceTexturePath =
            Phase2AkazaC08OriginalTextureRoot + "/Akaza_face_main.psd";
        private const string Phase2AkazaC08OriginalFaceBTexturePath =
            Phase2AkazaC08OriginalTextureRoot + "/Akaza_face_mainB.psd";
        private const string Phase2AkazaC08OriginalFaceOutlineTexturePath =
            Phase2AkazaC08OriginalTextureRoot + "/face3_lpow2.psd";
        private const string Phase2AkazaC08OriginalFaceEyesOutlineTexturePath =
            Phase2AkazaC08OriginalTextureRoot + "/face3_lpow.psd";
        private const string Phase2AkazaC08OriginalFaceEyesSpowTexturePath =
            Phase2AkazaC08OriginalTextureRoot + "/face3_main_spow.psd";
        private const string Phase2AkazaC08CorrectedEyesTexturePath =
            Phase2AkazaIntroSourceTextureRoot + "/DB_C08_Akaza_EyesCorrected.png";
        private const string Phase2AkazaC08OriginalBodyTexturePath =
            Phase2AkazaC08OriginalTextureRoot + "/Akaza_Body.psd";
        private const string Phase2AkazaC08CutoutBodyTexturePath =
            Phase2AkazaC08CutoutTextureRoot + "/Akaza_Body_cutout.png";
        private const string Phase2AkazaC08OriginalBodyShadowTexturePath =
            Phase2AkazaC08OriginalTextureRoot + "/Akaza_Body_Sdw.psd";
        private const string Phase2AkazaC08CutoutBodyShadowTexturePath =
            Phase2AkazaC08CutoutTextureRoot + "/Akaza_Body_Sdw_cutout.png";
        private const string Phase2AkazaC08OriginalArmTexturePath =
            Phase2AkazaC08OriginalTextureRoot + "/Akaza_Arm.psd";
        private const string Phase2AkazaC08CutoutArmTexturePath =
            Phase2AkazaC08CutoutTextureRoot + "/Akaza_Arm_cutout.png";
        private const string Phase2AkazaC08OriginalArmShadowTexturePath =
            Phase2AkazaC08OriginalTextureRoot + "/Akaza_Arm_sdw.psd";
        private const string Phase2AkazaC08CutoutArmShadowTexturePath =
            Phase2AkazaC08CutoutTextureRoot + "/Akaza_Arm_sdw_cutout.png";
        private const string Phase2AkazaC08OriginalHairTexturePath =
            Phase2AkazaC08OriginalTextureRoot + "/Akaza_hair.psd";
        private const string Phase2AkazaC08CutoutHairTexturePath =
            Phase2AkazaC08CutoutTextureRoot + "/Akaza_hair_cutout.png";
        private const string Phase2AkazaC08OriginalHairBTexturePath =
            Phase2AkazaC08OriginalTextureRoot + "/Akaza_hairB.psd";
        private const string Phase2AkazaC08CutoutHairBTexturePath =
            Phase2AkazaC08CutoutTextureRoot + "/Akaza_hairB_cutout.png";
        private const string Phase2AkazaC08OriginalHairLpowTexturePath =
            Phase2AkazaC08OriginalTextureRoot + "/Akaza_hairLpow.psd";
        private const string Phase2AkazaC08CutoutHairLpowTexturePath =
            Phase2AkazaC08CutoutTextureRoot + "/Akaza_hairLpow_cutout.png";
        private const string Phase2AkazaC08OriginalHairSpowTexturePath =
            Phase2AkazaC08OriginalTextureRoot + "/Akaza_hairSpow.psd";
        private const string Phase2AkazaC08CutoutHairSpowTexturePath =
            Phase2AkazaC08CutoutTextureRoot + "/Akaza_hairSpow_cutout.png";
        private const string Phase2AkazaHairSpowTexturePath =
            Phase2AkazaTextureRoot + "/Akaza_hairSpow.psd";
        private const string Phase2AkazaKohakuModelPath =
            Phase2AkazaIntroSourceModelRoot + "/Kohaku_model.fbx";
        private const string Phase2AkazaGateModelPath =
            Phase2AkazaIntroSourceModelRoot + "/gate.fbx";
        private const string Phase2AkazaAnimatorControllerPath =
            Phase2AkazaAnimationRoot + "/DB_Akaza_Phase2Boss.controller";
        private const string Phase2AkazaC08ActorSourcePath =
            Phase2AkazaAnimationSourceRoot + "/C08_akaza.fbx";
        private const string Phase2AkazaC08CameraSourcePath =
            Phase2AkazaAnimationSourceRoot + "/C08_cam.fbx";
        private const string Phase2AkazaC18GateSourcePath =
            Phase2AkazaAnimationSourceRoot + "/C18_gate.fbx";
        private const string Phase2AkazaC18KohakuSourcePath =
            Phase2AkazaAnimationSourceRoot + "/C18_kohaku.fbx";
        private const string Phase2AkazaC19KohakuSourcePath =
            Phase2AkazaAnimationSourceRoot + "/C19_kohaku.fbx";
        private const string Phase2AkazaC20KohakuSourcePath =
            Phase2AkazaAnimationSourceRoot + "/C20_kohaku.fbx";
        private const string Phase2AkazaC21KohakuSourcePath =
            Phase2AkazaAnimationSourceRoot + "/C21_kohaku.fbx";
        private const string Phase2AkazaC22KohakuSourcePath =
            Phase2AkazaAnimationSourceRoot + "/C22_kohaku.fbx";
        private const string Phase2AkazaC23KohakuSourcePath =
            Phase2AkazaAnimationSourceRoot + "/C23_kohaku.fbx";
        private const string Phase2AkazaC24KohakuSourcePath =
            Phase2AkazaAnimationSourceRoot + "/C24_kohaku.fbx";
        private const string Phase2AkazaC25KohakuSourcePath =
            Phase2AkazaAnimationSourceRoot + "/C25_kohaku.fbx";
        private const string Phase2AkazaC23ActorSourcePath =
            Phase2AkazaAnimationSourceRoot + "/C23_akaza.fbx";
        private const string Phase2AkazaC23CameraSourcePath =
            Phase2AkazaAnimationSourceRoot + "/C23_cam.fbx";
        private const string Phase2AkazaC18CameraSourcePath =
            Phase2AkazaAnimationSourceRoot + "/C18_cam.fbx";
        private const string Phase2AkazaC20CameraSourcePath =
            Phase2AkazaAnimationSourceRoot + "/C20_cam.fbx";
        private const string Phase2AkazaC21CameraSourcePath =
            Phase2AkazaAnimationSourceRoot + "/C21_cam.fbx";
        private const string Phase2AkazaC24CameraSourcePath =
            Phase2AkazaAnimationSourceRoot + "/C24_cam.fbx";
        private const string Phase2AkazaC25CameraSourcePath =
            Phase2AkazaAnimationSourceRoot + "/C25_cam.fbx";
        private const string Phase2AkazaC25ActorSourcePath =
            Phase2AkazaAnimationSourceRoot + "/C25_akaza.fbx";
        private const string Phase2AkazaC23IntroClipPath =
            Phase2AkazaAnimationSanitizedRoot + "/DB_Akaza_C08_Intro1412_1562_InPlace.anim";
        private const string Phase2AkazaC25InPlaceClipPath =
            Phase2AkazaAnimationSanitizedRoot + "/DB_Akaza_C25_InPlace.anim";
        private const string Phase2AkazaC27InPlaceClipPath =
            Phase2AkazaAnimationSanitizedRoot + "/DB_Akaza_C27_InPlace.anim";
        private const string Phase2AkazaC30InPlaceClipPath =
            Phase2AkazaAnimationSanitizedRoot + "/DB_Akaza_C30_InPlace.anim";
        private const string Phase2AkazaC34InPlaceClipPath =
            Phase2AkazaAnimationSanitizedRoot + "/DB_Akaza_C34_InPlace.anim";
        private const string Phase2AkazaCombatCueClipPath =
            Phase2AkazaAnimationSanitizedRoot + "/DB_Akaza_CombatCueClock.anim";
        private const string Phase2AkazaPrefabPath =
            "Assets/_Game/Prefabs/Enemies/ActionFoundation/PF_Boss_Akaza_Phase2Review.prefab";
        private const string Phase2AkazaProjectilePrefabPath =
            "Assets/_Game/Prefabs/Combat/PF_BossBarrageProjectile_AkazaPhase2.prefab";
        private const string Phase2AkazaIntroProfilePath =
            CinematicProfileRoot + "/DB_Cinematic_AkazaPhase2BossIntro_1412_1562.asset";
        private const string Phase2AkazaVisualName = ReviewRootPrefix + "HumanoidBossVisual_AkazaPhase2";
        private const string Phase2AkazaStaleC23CameraRigWrapperName =
            ReviewRootPrefix + "C23OriginalCameraRig";
        private const string Phase2AkazaC23CameraRigWrapperName =
            ReviewRootPrefix + "C08OriginalCameraRig";
        private const string Phase2AkazaC23CameraRigSourceName = "C08_cam";
        private const string Phase2AkazaC23ActorRigSourceName = "C08_akaza";
        private const string Phase2AkazaC08SourceSceneContextName = "C08_SourceSceneContext";
        private const string Phase2AkazaC08SkyBackdropName = "Sky_C08_SourceBackdrop";
        private const string Phase2AkazaC08HeadShadowPlaneName = "C08_SourceHeadShadowPlane";
        private const string Phase2AkazaC08FaceShadowOverlayName = "C08_SourceFaceShadowOverlay";
        private const string Phase2AkazaC08DirectionalLightName = "Directional light_C08";
        private const string Phase2AkazaC08PostProcessName = "C08_SourcePostProcess";
        private const string Phase2AkazaC08CombatLookSceneContextName =
            ReviewRootPrefix + "C08CombatLookSceneContext";
        private const string Phase2AkazaC08CombatLookPostProcessName =
            ReviewRootPrefix + "C08CombatLookPostProcess";
        private const string Phase2AkazaC08PostProcessProfilePath =
            Phase2AkazaIntroSourceProfileRoot + "/DB_C08_SourcePostProcess.asset";
        private const string Phase2AkazaC08FaceShadowOverlayMeshPath =
            Phase2AkazaIntroSourceMeshRoot + "/DB_C08_FaceShadowOverlay.asset";
        private const string Phase2AkazaKohakuActorRigSourceName = "Kohaku_model";
        private const string Phase2AkazaC18GateRigSourceName = "C18_gate";
        private const string Phase2AkazaC18SourceSceneContextName = "C18_SourceSceneContext";
        private const string Phase2AkazaC18BasePlaneName = "Plane";
        private const string Phase2AkazaC18PlaneAName = "Plane_C18A";
        private const string Phase2AkazaC18PlaneBName = "Plane_C18B";
        private const string Phase2AkazaC18DirectionalLightName = "Directional light_C18B";
        private const string Phase2AkazaAuraName = "AkazaPhase2_AuraCore";
        private const string Phase2AkazaCombatCueClockName = "AkazaPhase2_CombatCueClock";
        private const string Phase2AkazaDeckProfilePath =
            ActionFoundationProfileSetup.ProfileRoot + "/DB_BossPressureActionDeck_AkazaPhase2.asset";
        private const string Phase2AkazaSummonPressureProfilePath =
            ActionFoundationProfileSetup.ProfileRoot + "/DB_BossSummonPressure_AkazaPhase2.asset";
        private const string Phase2AkazaBasicFireProfilePath =
            ActionFoundationProfileSetup.ProfileRoot + "/DB_BossBasicFire_AkazaPhase2LanePoke.asset";
        private const string Phase2AkazaHoverLancePatternProfilePath =
            ActionFoundationProfileSetup.ProfileRoot + "/DB_BossBarrage_Phase2_AkazaHoverLance.asset";
        private const string Phase2AkazaSpiralVolleyPatternProfilePath =
            ActionFoundationProfileSetup.ProfileRoot + "/DB_BossBarrage_Phase2_AkazaSpiralVolley.asset";
        private const string Phase2AkazaSummonCurtainPatternProfilePath =
            ActionFoundationProfileSetup.ProfileRoot + "/DB_BossBarrage_Phase2_AkazaSummonCurtain.asset";
        private const string Phase2AkazaCrushNetPatternProfilePath =
            ActionFoundationProfileSetup.ProfileRoot + "/DB_BossBarrage_Phase2_AkazaCrushNet.asset";
        private const string Phase2AkazaProjectileMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_BossBarrageProjectile_AkazaPhase2.mat";
        private const string Phase2AkazaBasicProjectileMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_BossBasicFireProjectile_AkazaPhase2.mat";
        private const string Phase2AkazaCoreMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_Boss_Akaza_Phase2Core.mat";
        private const string Phase2AkazaAuraMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_Boss_Akaza_Phase2Aura.mat";
        private const string Phase2AkazaPlayInspectActiveKey = "DimensionBrawl.Phase2AkazaPlayInspect.Active";
        private const string Phase2AkazaPlayInspectStageKey = "DimensionBrawl.Phase2AkazaPlayInspect.Stage";
        private const string Phase2AkazaPlayInspectFramesKey = "DimensionBrawl.Phase2AkazaPlayInspect.Frames";
        private const string Phase2AkazaPlayInspectStartTicksKey = "DimensionBrawl.Phase2AkazaPlayInspect.StartTicks";
        private const string Phase2AkazaPlayInspectFailureKey = "DimensionBrawl.Phase2AkazaPlayInspect.Failure";
        private const string Phase2AkazaPlayInspectPrefix = "[Phase2AkazaPlayInspect]";
        private const string Phase2AkazaPlayInspectWaitingStage = "waiting";
        private const string Phase2AkazaPlayInspectPlayingStage = "playing";
        private const string Phase2AkazaPlayInspectExitingStage = "exiting";
        private const string Phase2AkazaPlayInspectCaptureDirectory =
            "Library/Phase2AkazaPlayInspectCaptures";
        private const float Phase2AkazaPlayInspectCaptureDeltaSeconds =
            1f / Phase2AkazaIntroSourceFrameRate;
        private const string Phase2AkazaTimelineParityCaptureDirectory =
            "Library/Phase2AkazaTimelineParityCaptures";
        private const int Phase2AkazaIntroSourceStartFrame = 1412;
        private const int Phase2AkazaIntroSourceEndFrame = 1562;
        private const float Phase2AkazaIntroSourceFrameRate = 60f;
        private const string Phase2AkazaC08ScreenFadeCanvasName =
            "BossBarrageLaneReview_C08OriginalScreenFadeCanvas";
        private const float Phase2AkazaOriginalC23CameraDurationSeconds = 2.5f;
        private const string Phase2AkazaTimelineParityPrefix = "[Phase2AkazaTimelineParity]";
        private static readonly Vector3 Phase2AkazaOriginalC23ActorWorldPosition =
            new Vector3(-3.231605f, -0.60994744f, -0.54573727f);
        private static readonly PromotedAkazaAsset[] Phase2AkazaPromotedAssets =
        {
            new PromotedAkazaAsset(
                "Models/Character/02_Akaza/FBX/Akaza_model.fbx",
                Phase2AkazaModelPath,
                AkazaImportKind.Model),
            new PromotedAkazaAsset(
                "Models/Character/01_Kohaku/FBX/Kohaku_model.fbx",
                Phase2AkazaKohakuModelPath,
                AkazaImportKind.Model),
            new PromotedAkazaAsset(
                "Models/BG/03_Gate/FBX/gate.fbx",
                Phase2AkazaGateModelPath,
                AkazaImportKind.Model),
            new PromotedAkazaAsset(
                "Models/Character/01_Kohaku/Images/Unity2016_C_Skin.psd",
                Phase2AkazaIntroSourceSkinTexturePath,
                AkazaImportKind.Texture),
            new PromotedAkazaAsset(
                "Models/Character/01_Kohaku/Images/face3_main.psd",
                Phase2AkazaIntroSourceFaceTexturePath,
                AkazaImportKind.Texture),
            new PromotedAkazaAsset(
                "Models/Character/01_Kohaku/Images/Unity2016_C_Body.psd",
                Phase2AkazaIntroSourceBodyTexturePath,
                AkazaImportKind.Texture),
            new PromotedAkazaAsset(
                "Models/Character/01_Kohaku/Images/Unity2016_C_Add.psd",
                Phase2AkazaIntroSourceAddTexturePath,
                AkazaImportKind.Texture),
            new PromotedAkazaAsset(
                "Models/Character/01_Kohaku/Images/Unity2016_C_Hair.psd",
                Phase2AkazaIntroSourceHairTexturePath,
                AkazaImportKind.Texture),
            new PromotedAkazaAsset(
                "Models/Character/01_Kohaku/Images/Unity2016_C_Hair_Spow.psd",
                Phase2AkazaIntroSourceHairSpowTexturePath,
                AkazaImportKind.Texture),
            new PromotedAkazaAsset(
                "Models/Character/01_Kohaku/Images/Unity2016_Wep.psd",
                Phase2AkazaIntroSourceWeaponTexturePath,
                AkazaImportKind.Texture),
            new PromotedAkazaAsset(
                "Models/BG/03_Gate/TEX/gate.psd",
                Phase2AkazaIntroSourceGateTexturePath,
                AkazaImportKind.Texture),
            new PromotedAkazaAsset(
                "Models/BG/01_ServerRoom/TEX/efx_02.psd",
                Phase2AkazaIntroSourceEfx02TexturePath,
                AkazaImportKind.Texture),
            new PromotedAkazaAsset(
                "Models/BG/04_HighWay/images/unity_sora_loop.psd",
                Phase2AkazaC08OriginalSkyLoopTexturePath,
                AkazaImportKind.TextureWithSourceMeta),
            new PromotedAkazaAsset(
                "Models/Character/02_Akaza/Images/Akaza_Skin.psd",
                Phase2AkazaC08OriginalSkinTexturePath,
                AkazaImportKind.TextureWithSourceMeta),
            new PromotedAkazaAsset(
                "Models/Character/02_Akaza/Images/Akaza_Skin_Sdw.psd",
                Phase2AkazaC08OriginalSkinShadowTexturePath,
                AkazaImportKind.TextureWithSourceMeta),
            new PromotedAkazaAsset(
                "Models/Character/02_Akaza/Images/Akaza_face_main.psd",
                Phase2AkazaC08OriginalFaceTexturePath,
                AkazaImportKind.TextureWithSourceMeta),
            new PromotedAkazaAsset(
                "Models/Character/02_Akaza/Images/Akaza_face_mainB.psd",
                Phase2AkazaC08OriginalFaceBTexturePath,
                AkazaImportKind.TextureWithSourceMeta),
            new PromotedAkazaAsset(
                "Models/Character/01_Kohaku/Images/face3_lpow2.psd",
                Phase2AkazaC08OriginalFaceOutlineTexturePath,
                AkazaImportKind.TextureWithSourceMeta),
            new PromotedAkazaAsset(
                "Models/Character/01_Kohaku/Images/face3_lpow.psd",
                Phase2AkazaC08OriginalFaceEyesOutlineTexturePath,
                AkazaImportKind.TextureWithSourceMeta),
            new PromotedAkazaAsset(
                "Models/Character/01_Kohaku/Images/face3_main_spow.psd",
                Phase2AkazaC08OriginalFaceEyesSpowTexturePath,
                AkazaImportKind.TextureWithSourceMeta),
            new PromotedAkazaAsset(
                "Models/Character/02_Akaza/Images/Akaza_Body.psd",
                Phase2AkazaC08OriginalBodyTexturePath,
                AkazaImportKind.TextureWithSourceMeta),
            new PromotedAkazaAsset(
                "Models/Character/02_Akaza/Images/Akaza_Body_Sdw.psd",
                Phase2AkazaC08OriginalBodyShadowTexturePath,
                AkazaImportKind.TextureWithSourceMeta),
            new PromotedAkazaAsset(
                "Models/Character/02_Akaza/Images/Akaza_Arm.psd",
                Phase2AkazaC08OriginalArmTexturePath,
                AkazaImportKind.TextureWithSourceMeta),
            new PromotedAkazaAsset(
                "Models/Character/02_Akaza/Images/Akaza_Arm_sdw.psd",
                Phase2AkazaC08OriginalArmShadowTexturePath,
                AkazaImportKind.TextureWithSourceMeta),
            new PromotedAkazaAsset(
                "Models/Character/02_Akaza/Images/Akaza_hair.psd",
                Phase2AkazaC08OriginalHairTexturePath,
                AkazaImportKind.TextureWithSourceMeta),
            new PromotedAkazaAsset(
                "Models/Character/02_Akaza/Images/Akaza_hairB.psd",
                Phase2AkazaC08OriginalHairBTexturePath,
                AkazaImportKind.TextureWithSourceMeta),
            new PromotedAkazaAsset(
                "Models/Character/02_Akaza/Images/Akaza_hairLpow.psd",
                Phase2AkazaC08OriginalHairLpowTexturePath,
                AkazaImportKind.TextureWithSourceMeta),
            new PromotedAkazaAsset(
                "Models/Character/02_Akaza/Images/Akaza_hairSpow.psd",
                Phase2AkazaC08OriginalHairSpowTexturePath,
                AkazaImportKind.TextureWithSourceMeta),
            new PromotedAkazaAsset(
                "Scenes/01_Master/C08/C08_akaza.fbx",
                Phase2AkazaC08ActorSourcePath,
                AkazaImportKind.Animation),
            new PromotedAkazaAsset(
                "Scenes/01_Master/C08/C08_cam.fbx",
                Phase2AkazaC08CameraSourcePath,
                AkazaImportKind.CameraAnimation),
            new PromotedAkazaAsset(
                "Scenes/01_Master/C19-32/C25_akaza.fbx",
                Phase2AkazaC25ActorSourcePath,
                AkazaImportKind.Animation),
            new PromotedAkazaAsset(
                "Scenes/01_Master/C19-32/C27_akaza.fbx",
                Phase2AkazaAnimationSourceRoot + "/C27_akaza.fbx",
                AkazaImportKind.Animation),
            new PromotedAkazaAsset(
                "Scenes/01_Master/C19-32/C30_akaza.fbx",
                Phase2AkazaAnimationSourceRoot + "/C30_akaza.fbx",
                AkazaImportKind.Animation),
            new PromotedAkazaAsset(
                "Scenes/01_Master/C33/C33_Akaza.fbx",
                Phase2AkazaAnimationSourceRoot + "/C33_Akaza.fbx",
                AkazaImportKind.Animation),
            new PromotedAkazaAsset(
                "Scenes/01_Master/C33/C34_Akaza.fbx",
                Phase2AkazaAnimationSourceRoot + "/C34_Akaza.fbx",
                AkazaImportKind.Animation),
            new PromotedAkazaAsset(
                "Models/Character/02_Akaza/Images/Akaza_Arm.psd",
                Phase2AkazaTextureRoot + "/Akaza_Arm.psd",
                AkazaImportKind.Texture),
            new PromotedAkazaAsset(
                "Models/Character/02_Akaza/Images/Akaza_Body.psd",
                Phase2AkazaTextureRoot + "/Akaza_Body.psd",
                AkazaImportKind.Texture),
            new PromotedAkazaAsset(
                "Models/Character/02_Akaza/Images/Akaza_face_main.psd",
                Phase2AkazaTextureRoot + "/Akaza_face_main.psd",
                AkazaImportKind.Texture),
            new PromotedAkazaAsset(
                "Models/Character/02_Akaza/Images/Akaza_hair.psd",
                Phase2AkazaTextureRoot + "/Akaza_hair.psd",
                AkazaImportKind.Texture),
            new PromotedAkazaAsset(
                "Models/Character/02_Akaza/Images/Akaza_hairSpow.psd",
                Phase2AkazaHairSpowTexturePath,
                AkazaImportKind.Texture),
            new PromotedAkazaAsset(
                "Models/Character/02_Akaza/Images/Akaza_Skin.psd",
                Phase2AkazaTextureRoot + "/Akaza_Skin.psd",
                AkazaImportKind.Texture),
            new PromotedAkazaAsset(
                "Models/Character/02_Akaza/Images/wires.tga",
                Phase2AkazaTextureRoot + "/wires.tga",
                AkazaImportKind.Texture)
        };

        private enum AkazaImportKind
        {
            Model,
            Animation,
            CameraAnimation,
            Texture,
            TextureWithSourceMeta
        }

        [MenuItem("DimensionBrawl/Reapply Action Foundation Phase2 Akaza Boss Review Scene")]
        public static void ReapplyPhase2AkazaBossReviewSceneMenu()
        {
            EnsurePhase2AkazaBossReviewScene();
            Debug.Log("Reapplied ActionFoundation phase2 Akaza boss review scene.");
        }

        [MenuItem("DimensionBrawl/Validate Action Foundation Phase2 Akaza Boss Review Scene")]
        public static void ValidatePhase2AkazaBossReviewSceneMenu()
        {
            ValidatePhase2AkazaBossReviewScene();
            Debug.Log("ActionFoundation phase2 Akaza boss review scene validation passed.");
        }

        [MenuItem("DimensionBrawl/Diagnostics/Inspect Phase2 Akaza Play State")]
        public static void InspectPhase2AkazaPlayStateMenu()
        {
            StartPhase2AkazaPlayInspect();
        }

        [MenuItem("DimensionBrawl/Diagnostics/Validate Phase2 Akaza Timeline Frame Parity")]
        public static void ValidatePhase2AkazaTimelineFrameParityMenu()
        {
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            Scene scene = EditorSceneManager.OpenScene(Phase2AkazaReviewScenePath, OpenSceneMode.Single);
            ValidatePhase2AkazaTimelineFrameParity(scene);
            Debug.Log(
                $"{Phase2AkazaTimelineParityPrefix} validated source timeline frames "
                + $"{Phase2AkazaIntroSourceStartFrame}-{Phase2AkazaIntroSourceEndFrame}.");

            if (Application.isBatchMode)
            {
                EditorApplication.Exit(0);
            }
        }

        [MenuItem("DimensionBrawl/Diagnostics/Capture Phase2 Akaza Source Frame 1412")]
        public static void CapturePhase2AkazaSourceFrame1412Menu()
        {
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            EnsurePhase2AkazaBossReviewScene();
            Scene scene = EditorSceneManager.OpenScene(Phase2AkazaReviewScenePath, OpenSceneMode.Single);
            CapturePhase2AkazaSourceFrame(scene, Phase2AkazaIntroSourceStartFrame);

            if (Application.isBatchMode)
            {
                EditorApplication.Exit(0);
            }
        }

        [MenuItem("DimensionBrawl/Diagnostics/Capture Phase2 Akaza Source Frame Samples")]
        public static void CapturePhase2AkazaSourceFrameSamplesMenu()
        {
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            EnsurePhase2AkazaBossReviewScene();
            Scene scene = EditorSceneManager.OpenScene(Phase2AkazaReviewScenePath, OpenSceneMode.Single);
            int[] sourceFrames =
            {
                Phase2AkazaIntroSourceStartFrame,
                1450,
                1500,
                1532,
                Phase2AkazaIntroSourceEndFrame
            };
            for (int i = 0; i < sourceFrames.Length; i++)
            {
                CapturePhase2AkazaSourceFrame(scene, sourceFrames[i]);
            }

            if (Application.isBatchMode)
            {
                EditorApplication.Exit(0);
            }
        }

        [MenuItem("DimensionBrawl/Diagnostics/Dump Phase2 Akaza Animation Curves")]
        public static void DumpPhase2AkazaAnimationCurvesMenu()
        {
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            DumpPhase2AkazaAnimationCurves();
        }

        [InitializeOnLoadMethod]
        private static void ResumePhase2AkazaPlayInspect()
        {
            if (!SessionState.GetBool(Phase2AkazaPlayInspectActiveKey, false))
            {
                return;
            }

            RegisterPhase2AkazaPlayInspectCallbacks();
        }

        private static void StartPhase2AkazaPlayInspect()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                throw new InvalidOperationException("Stop Play Mode before starting the phase2 Akaza play inspect.");
            }

            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            SessionState.SetBool(Phase2AkazaPlayInspectActiveKey, true);
            SessionState.SetString(Phase2AkazaPlayInspectStageKey, Phase2AkazaPlayInspectWaitingStage);
            SessionState.SetInt(Phase2AkazaPlayInspectFramesKey, 0);
            SessionState.SetString(Phase2AkazaPlayInspectStartTicksKey, DateTime.UtcNow.Ticks.ToString());
            SessionState.SetBool(Phase2AkazaPlayInspectFailureKey, false);
            RegisterPhase2AkazaPlayInspectCallbacks();

            Scene scene = EditorSceneManager.OpenScene(Phase2AkazaReviewScenePath, OpenSceneMode.Single);
            LogPhase2AkazaPlaySnapshot("edit-before-play", scene);
            Debug.Log($"{Phase2AkazaPlayInspectPrefix} entering Play Mode for object-state inspection.");
            EditorApplication.EnterPlaymode();
        }

        private static void RegisterPhase2AkazaPlayInspectCallbacks()
        {
            EditorApplication.update -= UpdatePhase2AkazaPlayInspect;
            EditorApplication.update += UpdatePhase2AkazaPlayInspect;
            EditorApplication.playModeStateChanged -= OnPhase2AkazaPlayInspectStateChanged;
            EditorApplication.playModeStateChanged += OnPhase2AkazaPlayInspectStateChanged;
        }

        private static void CleanupPhase2AkazaPlayInspect()
        {
            Time.captureDeltaTime = 0f;
            EditorApplication.update -= UpdatePhase2AkazaPlayInspect;
            EditorApplication.playModeStateChanged -= OnPhase2AkazaPlayInspectStateChanged;
            SessionState.EraseBool(Phase2AkazaPlayInspectActiveKey);
            SessionState.EraseString(Phase2AkazaPlayInspectStageKey);
            SessionState.EraseInt(Phase2AkazaPlayInspectFramesKey);
            SessionState.EraseString(Phase2AkazaPlayInspectStartTicksKey);
            SessionState.EraseBool(Phase2AkazaPlayInspectFailureKey);
        }

        private static void OnPhase2AkazaPlayInspectStateChanged(PlayModeStateChange stateChange)
        {
            if (!SessionState.GetBool(Phase2AkazaPlayInspectActiveKey, false))
            {
                return;
            }

            Debug.Log($"{Phase2AkazaPlayInspectPrefix} playModeStateChanged={stateChange}");
            if (stateChange == PlayModeStateChange.EnteredPlayMode)
            {
                Time.captureDeltaTime = Phase2AkazaPlayInspectCaptureDeltaSeconds;
                SessionState.SetString(Phase2AkazaPlayInspectStageKey, Phase2AkazaPlayInspectPlayingStage);
                SessionState.SetInt(Phase2AkazaPlayInspectFramesKey, 0);
                LogPhase2AkazaPlaySnapshot("entered-play-mode", SceneManager.GetActiveScene());
            }
            else if (stateChange == PlayModeStateChange.EnteredEditMode
                && string.Equals(
                    SessionState.GetString(Phase2AkazaPlayInspectStageKey, string.Empty),
                    Phase2AkazaPlayInspectExitingStage,
                    StringComparison.Ordinal))
            {
                bool failed = SessionState.GetBool(Phase2AkazaPlayInspectFailureKey, false);
                Debug.Log(
                    $"{Phase2AkazaPlayInspectPrefix} completed Play Mode object-state inspection. "
                    + $"failed={failed}");
                CleanupPhase2AkazaPlayInspect();
                EditorApplication.Exit(failed ? 2 : 0);
            }
        }

        private static void UpdatePhase2AkazaPlayInspect()
        {
            if (!SessionState.GetBool(Phase2AkazaPlayInspectActiveKey, false))
            {
                return;
            }

            if (HasPhase2AkazaPlayInspectTimedOut())
            {
                RecordPhase2AkazaPlayInspectFailure("timed out before inspection completed.");
                CleanupPhase2AkazaPlayInspect();
                EditorApplication.Exit(2);
                return;
            }

            string stage = SessionState.GetString(Phase2AkazaPlayInspectStageKey, string.Empty);
            if (!EditorApplication.isPlaying)
            {
                if (string.Equals(stage, Phase2AkazaPlayInspectExitingStage, StringComparison.Ordinal)
                    && !EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    bool failed = SessionState.GetBool(Phase2AkazaPlayInspectFailureKey, false);
                    Debug.Log(
                        $"{Phase2AkazaPlayInspectPrefix} completed Play Mode object-state inspection. "
                        + $"failed={failed}");
                    CleanupPhase2AkazaPlayInspect();
                    EditorApplication.Exit(failed ? 2 : 0);
                }

                return;
            }

            if (!string.Equals(stage, Phase2AkazaPlayInspectPlayingStage, StringComparison.Ordinal))
            {
                SessionState.SetString(Phase2AkazaPlayInspectStageKey, Phase2AkazaPlayInspectPlayingStage);
            }

            Time.captureDeltaTime = Phase2AkazaPlayInspectCaptureDeltaSeconds;
            int frame = SessionState.GetInt(Phase2AkazaPlayInspectFramesKey, 0) + 1;
            SessionState.SetInt(Phase2AkazaPlayInspectFramesKey, frame);
            if (frame == 1 || frame == 10 || frame == 30 || frame == 60
                || frame == 120 || frame == 180 || frame == 240 || frame == 300)
            {
                LogPhase2AkazaPlaySnapshot($"play-frame-{frame}", SceneManager.GetActiveScene());
            }

            if (frame >= 300)
            {
                SessionState.SetString(Phase2AkazaPlayInspectStageKey, Phase2AkazaPlayInspectExitingStage);
                Debug.Log($"{Phase2AkazaPlayInspectPrefix} exiting Play Mode after frame {frame}.");
                EditorApplication.ExitPlaymode();
            }
        }

        private static bool HasPhase2AkazaPlayInspectTimedOut()
        {
            string value = SessionState.GetString(Phase2AkazaPlayInspectStartTicksKey, string.Empty);
            if (!long.TryParse(value, out long ticks))
            {
                return false;
            }

            return (DateTime.UtcNow - new DateTime(ticks, DateTimeKind.Utc)).TotalSeconds > 90d;
        }

        private static void LogPhase2AkazaPlaySnapshot(string label, Scene scene)
        {
            try
            {
                GameObject bossProxy = RequireRoot(scene, BossProxyRootName);
                Transform visual = FindPhase2AkazaVisual(bossProxy.transform);
                if (visual == null)
                {
                    RecordPhase2AkazaPlayInspectFailure($"{label} visual missing under {bossProxy.name}.");
                    return;
                }

                LogPhase2AkazaTransformChain(label, visual);
                LogPhase2AkazaAnimator(label, visual);
                LogPhase2AkazaRenderers(label, visual);
                LogPhase2AkazaCameraProjection(label, scene, visual.gameObject);
                LogPhase2AkazaSourceIntroState(label, scene, visual.gameObject);
                CapturePhase2AkazaPlayInspectFrame(label, scene);
                LogPhase2AkazaBehaviours(label, scene, bossProxy.transform, visual);
            }
            catch (Exception exception)
            {
                RecordPhase2AkazaPlayInspectFailure($"{label} snapshot failed: {exception}");
            }
        }

        private static Transform FindPhase2AkazaVisual(Transform root)
        {
            Transform directChild = root.Find(Phase2AkazaVisualName);
            return directChild != null ? directChild : FindChildRecursive(root, Phase2AkazaVisualName);
        }

        private static Transform FindChildRecursive(Transform root, string childName)
        {
            for (int i = 0; i < root.childCount; i++)
            {
                Transform child = root.GetChild(i);
                if (string.Equals(child.name, childName, StringComparison.Ordinal))
                {
                    return child;
                }

                Transform nested = FindChildRecursive(child, childName);
                if (nested != null)
                {
                    return nested;
                }
            }

            return null;
        }

        private static Transform RequireChildRecursive(Transform root, string childName)
        {
            Transform child = FindChildRecursive(root, childName);
            if (child == null)
            {
                throw new InvalidOperationException($"{root.name} is missing descendant {childName}.");
            }

            return child;
        }

        private static void LogPhase2AkazaTransformChain(string label, Transform visual)
        {
            List<string> chain = new List<string>();
            for (Transform current = visual; current != null; current = current.parent)
            {
                chain.Add(
                    $"{current.name}[self={current.gameObject.activeSelf},hier={current.gameObject.activeInHierarchy},"
                    + $"localPos={FormatVector3(current.localPosition)},worldPos={FormatVector3(current.position)},"
                    + $"localScale={FormatVector3(current.localScale)},localRot={FormatQuaternion(current.localRotation)}]");
            }

            chain.Reverse();
            Debug.Log($"{Phase2AkazaPlayInspectPrefix} {label} hierarchy {string.Join(" -> ", chain)}");
        }

        private static void LogPhase2AkazaAnimator(string label, Transform visual)
        {
            Animator animator = visual.GetComponentInChildren<Animator>(includeInactive: true);
            if (animator == null)
            {
                Debug.LogError($"{Phase2AkazaPlayInspectPrefix} {label} animator missing.");
                return;
            }

            string state = "unavailable";
            int fullPathHash = 0;
            float normalizedTime = 0f;
            bool inTransition = false;
            if (animator.isActiveAndEnabled && animator.runtimeAnimatorController != null && animator.layerCount > 0)
            {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                fullPathHash = stateInfo.fullPathHash;
                normalizedTime = stateInfo.normalizedTime;
                state = ResolveAnimatorStateName(animator, fullPathHash);
                inTransition = animator.IsInTransition(0);
            }

            Debug.Log(
                $"{Phase2AkazaPlayInspectPrefix} {label} animator "
                + $"object={animator.gameObject.name}, enabled={animator.enabled}, "
                + $"activeHier={animator.gameObject.activeInHierarchy}, controller={NameOrNull(animator.runtimeAnimatorController)}, "
                + $"avatar={NameOrNull(animator.avatar)}, avatarValid={(animator.avatar != null && animator.avatar.isValid)}, "
                + $"applyRootMotion={animator.applyRootMotion}, culling={animator.cullingMode}, "
                + $"state={state}, stateHash={fullPathHash}, normalized={normalizedTime:0.000}, "
                + $"inTransition={inTransition}, rootPos={FormatVector3(animator.rootPosition)}, "
                + $"rootRot={FormatQuaternion(animator.rootRotation)}");
        }

        private static string ResolveAnimatorStateName(Animator animator, int fullPathHash)
        {
            if (animator.runtimeAnimatorController is not AnimatorController controller)
            {
                return $"hash:{fullPathHash}";
            }

            for (int layerIndex = 0; layerIndex < controller.layers.Length; layerIndex++)
            {
                AnimatorControllerLayer layer = controller.layers[layerIndex];
                ChildAnimatorState[] states = layer.stateMachine.states;
                for (int stateIndex = 0; stateIndex < states.Length; stateIndex++)
                {
                    string path = $"{layer.name}.{states[stateIndex].state.name}";
                    if (Animator.StringToHash(path) == fullPathHash)
                    {
                        return path;
                    }
                }
            }

            return $"hash:{fullPathHash}";
        }

        private static void LogPhase2AkazaRenderers(string label, Transform visual)
        {
            Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(includeInactive: true);
            int activeEnabled = 0;
            int enabled = 0;
            int forceOff = 0;
            int skinned = 0;
            int skinnedOffscreenDisabled = 0;
            int dynamicOccludees = 0;
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                if (renderer.enabled)
                {
                    enabled++;
                }

                if (renderer.enabled && renderer.gameObject.activeInHierarchy && !renderer.forceRenderingOff)
                {
                    activeEnabled++;
                }

                if (renderer.forceRenderingOff)
                {
                    forceOff++;
                }

                if (renderer.allowOcclusionWhenDynamic)
                {
                    dynamicOccludees++;
                }

                if (renderer is SkinnedMeshRenderer skinnedMeshRenderer)
                {
                    skinned++;
                    if (!skinnedMeshRenderer.updateWhenOffscreen)
                    {
                        skinnedOffscreenDisabled++;
                    }
                }
            }

            Bounds bounds = CalculateRendererBounds(visual.gameObject);
            Debug.Log(
                $"{Phase2AkazaPlayInspectPrefix} {label} renderers total={renderers.Length}, "
                + $"enabled={enabled}, activeEnabled={activeEnabled}, forceOff={forceOff}, "
                + $"skinned={skinned}, skinnedUpdateOffscreenOff={skinnedOffscreenDisabled}, "
                + $"dynamicOccludees={dynamicOccludees}, boundsCenter={FormatVector3(bounds.center)}, "
                + $"boundsSize={FormatVector3(bounds.size)}, boundsMin={FormatVector3(bounds.min)}, "
                + $"boundsMax={FormatVector3(bounds.max)}");

            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                bool shouldLog =
                    i < 12
                    || !renderer.gameObject.activeInHierarchy
                    || !renderer.enabled
                    || renderer.forceRenderingOff
                    || (renderer is SkinnedMeshRenderer skinnedMeshRenderer && !skinnedMeshRenderer.updateWhenOffscreen);
                if (!shouldLog)
                {
                    continue;
                }

                string skinnedState = renderer is SkinnedMeshRenderer skinnedRenderer
                    ? $", updateWhenOffscreen={skinnedRenderer.updateWhenOffscreen}"
                    : string.Empty;
                Debug.Log(
                    $"{Phase2AkazaPlayInspectPrefix} {label} renderer[{i}] {renderer.name} "
                    + $"activeHier={renderer.gameObject.activeInHierarchy}, enabled={renderer.enabled}, "
                    + $"forceOff={renderer.forceRenderingOff}, dynamicOccludee={renderer.allowOcclusionWhenDynamic}"
                    + $"{skinnedState}, boundsCenter={FormatVector3(renderer.bounds.center)}, "
                    + $"boundsSize={FormatVector3(renderer.bounds.size)}, materials={renderer.sharedMaterials.Length}");
            }
        }

        private static void LogPhase2AkazaSourceIntroState(string label, Scene scene, GameObject primaryVisual)
        {
            GameObject wrapper = RequireRoot(scene, Phase2AkazaC23CameraRigWrapperName);
            string c08CameraRigName = Path.GetFileNameWithoutExtension(Phase2AkazaC08CameraSourcePath);
            string c08SourceRigName = Path.GetFileNameWithoutExtension(Phase2AkazaC08ActorSourcePath);
            GameObject sourceCameraRig = RequireChildRecursive(wrapper.transform, c08CameraRigName).gameObject;
            GameObject sourceActor = RequireChildRecursive(wrapper.transform, c08SourceRigName).gameObject;
            Camera mainCamera = ResolvePhase2AkazaValidationCamera(scene);
            Camera sourceCamera = sourceCameraRig.GetComponentInChildren<Camera>(includeInactive: true);
            if (sourceCamera == null)
            {
                RecordPhase2AkazaPlayInspectFailure($"{label} source frame 1412 camera missing.");
                return;
            }

            Bounds bounds = CalculateRendererBounds(sourceActor);
            int activeRendererCount = CountActiveRenderableRenderers(sourceActor);
            Vector3 viewportCenter = mainCamera.WorldToViewportPoint(bounds.center);
            float viewportHeight = EstimateViewportHeight(mainCamera, bounds);
            CalculateViewportRect(mainCamera, bounds, out Vector2 viewportMin, out Vector2 viewportMax, out int projectedCorners);
            bool inFrustum = GeometryUtility.TestPlanesAABB(
                GeometryUtility.CalculateFrustumPlanes(mainCamera),
                bounds);
            float cameraDistance = Vector3.Distance(mainCamera.transform.position, sourceCamera.transform.position);
            float cameraAngle = Quaternion.Angle(mainCamera.transform.rotation, sourceCamera.transform.rotation);
            float fovDelta = Mathf.Abs(mainCamera.fieldOfView - sourceCamera.fieldOfView);

            Debug.Log(
                $"{Phase2AkazaPlayInspectPrefix} {label} sourceFrame1412 "
                + $"wrapperActive={wrapper.activeInHierarchy}, sourceSelf={sourceActor.activeSelf}, "
                + $"sourceHier={sourceActor.activeInHierarchy}, "
                + $"activeRenderers={activeRendererCount}, "
                + $"mainMatchesSourceCameraPosDelta={cameraDistance:0.000}, "
                + $"mainMatchesSourceCameraAngle={cameraAngle:0.000}, fovDelta={fovDelta:0.000}, "
                + $"viewportCenter={FormatVector3(viewportCenter)}, viewportHeight={viewportHeight:0.000}, "
                + $"viewportMin={FormatVector2(viewportMin)}, viewportMax={FormatVector2(viewportMax)}, "
                + $"projectedCorners={projectedCorners}, inFrustum={inFrustum}, "
                + $"sourceCameraEnabled={sourceCamera.enabled}, primaryVisualActive={primaryVisual.activeInHierarchy}");

            if (ShouldValidatePhase2AkazaSourceIntroFrame(label))
            {
                bool actorReadable = sourceActor.activeInHierarchy
                    && activeRendererCount > 0
                    && inFrustum
                    && projectedCorners > 0
                    && viewportCenter.z > mainCamera.nearClipPlane
                    && viewportCenter.z < mainCamera.farClipPlane
                    && viewportHeight >= 0.08f;
                bool cameraMatchesSource = cameraDistance <= 0.05f
                    && cameraAngle <= 0.5f
                    && fovDelta <= 0.1f;
                if (!actorReadable || !cameraMatchesSource || sourceCamera.enabled)
                {
                    RecordPhase2AkazaPlayInspectFailure(
                        $"{label} original source intro is not actually visible through the original camera: "
                        + $"actorReadable={actorReadable}, cameraMatchesSource={cameraMatchesSource}, "
                        + $"sourceCameraEnabled={sourceCamera.enabled}, activeRenderers={activeRendererCount}, "
                        + $"center={FormatVector3(viewportCenter)}, height={viewportHeight:0.000}, "
                        + $"rect={FormatVector2(viewportMin)}-{FormatVector2(viewportMax)}, "
                        + $"cameraDistance={cameraDistance:0.000}, cameraAngle={cameraAngle:0.000}, fovDelta={fovDelta:0.000}.");
                }
            }
        }

        private static void LogPhase2AkazaCameraProjection(string label, Scene scene, GameObject visual)
        {
            Camera camera = ResolvePhase2AkazaValidationCamera(scene);
            Bounds bounds = CalculateRendererBounds(visual);
            Vector3 viewportCenter = camera.WorldToViewportPoint(bounds.center);
            float viewportHeight = EstimateViewportHeight(camera, bounds);
            CalculateViewportRect(camera, bounds, out Vector2 viewportMin, out Vector2 viewportMax, out int projectedCorners);
            bool inFrustum = GeometryUtility.TestPlanesAABB(
                GeometryUtility.CalculateFrustumPlanes(camera),
                bounds);
            Debug.Log(
                $"{Phase2AkazaPlayInspectPrefix} {label} camera {camera.name} "
                + $"pos={FormatVector3(camera.transform.position)}, rot={FormatQuaternion(camera.transform.rotation)}, "
                + $"viewportCenter={FormatVector3(viewportCenter)}, viewportHeight={viewportHeight:0.000}, "
                + $"viewportMin={FormatVector2(viewportMin)}, viewportMax={FormatVector2(viewportMax)}, "
                + $"projectedCorners={projectedCorners}, inFrustum={inFrustum}, "
                + $"near={camera.nearClipPlane:0.###}, far={camera.farClipPlane:0.###}");

            if (!visual.activeInHierarchy)
            {
                return;
            }

            if (!ShouldValidatePhase2AkazaPlayFrame(label))
            {
                return;
            }

            bool depthReadable = viewportCenter.z > camera.nearClipPlane && viewportCenter.z < camera.farClipPlane;
            bool centerReadable = viewportCenter.x >= 0.12f
                && viewportCenter.x <= 0.88f
                && viewportCenter.y >= 0.08f
                && viewportCenter.y <= 0.92f;
            bool scaleReadable = viewportHeight >= 0.08f && viewportHeight <= 2.35f;
            if (!depthReadable || !centerReadable || !scaleReadable || !inFrustum || projectedCorners == 0)
            {
                RecordPhase2AkazaPlayInspectFailure(
                    $"{label} Akaza intro camera framing is not readable: "
                    + $"center={FormatVector3(viewportCenter)}, height={viewportHeight:0.000}, "
                    + $"rect={FormatVector2(viewportMin)}-{FormatVector2(viewportMax)}, "
                    + $"inFrustum={inFrustum}, projectedCorners={projectedCorners}.");
            }
        }

        private static void LogPhase2AkazaBehaviours(string label, Scene scene, Transform bossProxy, Transform visual)
        {
            MonoBehaviour[] behaviours = visual.GetComponentsInChildren<MonoBehaviour>(includeInactive: true);
            List<string> disabled = new List<string>();
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour != null && !behaviour.enabled)
                {
                    disabled.Add($"{behaviour.GetType().Name}@{behaviour.gameObject.name}");
                }
            }

            Debug.Log(
                $"{Phase2AkazaPlayInspectPrefix} {label} behaviours total={behaviours.Length}, "
                + $"disabled={disabled.Count}"
                + (disabled.Count > 0 ? $", disabledList={string.Join(",", disabled)}" : string.Empty));

            LogPhase2AkazaPlaybackLockState(label, scene, bossProxy.gameObject, visual.gameObject);
        }

        private static void LogPhase2AkazaPlaybackLockState(string label, Scene scene, GameObject bossProxy, GameObject visual)
        {
            Behaviour[] locks =
            {
                bossProxy.GetComponent<BossPressureCostLadder>(),
                bossProxy.GetComponent<BossPressureActionDirector>(),
                bossProxy.GetComponent<BossPressurePositionController>(),
                bossProxy.GetComponent<BossBarrageEmitter>(),
                bossProxy.GetComponent<BossBasicFireEmitter>(),
                bossProxy.GetComponent<BossSummonPressureAction>(),
                visual.GetComponent<ActionFoundationArenaTransformMotion>()
            };
            string[] names =
            {
                nameof(BossPressureCostLadder),
                nameof(BossPressureActionDirector),
                nameof(BossPressurePositionController),
                nameof(BossBarrageEmitter),
                nameof(BossBasicFireEmitter),
                nameof(BossSummonPressureAction),
                nameof(ActionFoundationArenaTransformMotion)
            };

            List<string> states = new List<string>(locks.Length);
            bool cinematicPlaying = TryFindPhase2AkazaIntroRunner(scene, out CinematicSequenceRunner runner)
                && runner.IsPlaying;
            bool anyLockDisabled = false;
            for (int i = 0; i < locks.Length; i++)
            {
                Behaviour current = locks[i];
                if (current != null && !current.enabled)
                {
                    anyLockDisabled = true;
                    break;
                }
            }

            bool shouldBeLocked = ShouldValidatePhase2AkazaPlayLockFrame(
                label,
                cinematicPlaying,
                anyLockDisabled);
            for (int i = 0; i < locks.Length; i++)
            {
                Behaviour current = locks[i];
                string state = current != null ? current.enabled.ToString() : "missing";
                states.Add($"{names[i]}={state}");
                if (shouldBeLocked && (current == null || current.enabled))
                {
                    RecordPhase2AkazaPlayInspectFailure(
                        $"{label} expected {names[i]} to be disabled during the boss intro, found {state}.");
                }
            }

            Debug.Log(
                $"{Phase2AkazaPlayInspectPrefix} {label} playbackLocks "
                + $"bossPos={FormatVector3(bossProxy.transform.position)}, visualPos={FormatVector3(visual.transform.position)}, "
                + $"cinematicPlaying={cinematicPlaying}, "
                + string.Join(", ", states));
        }

        private static bool ShouldValidatePhase2AkazaPlayFrame(string label)
        {
            return TryParsePhase2AkazaPlayFrame(label, out int frame) && frame >= 30 && frame <= 300;
        }

        private static bool ShouldValidatePhase2AkazaSourceIntroFrame(string label)
        {
            return string.Equals(label, "entered-play-mode", StringComparison.Ordinal)
                || (TryParsePhase2AkazaPlayFrame(label, out int frame) && frame >= 1 && frame <= 2);
        }

        private static bool ShouldValidatePhase2AkazaPostSourceHandoffFrame(string label)
        {
            return TryParsePhase2AkazaPlayFrame(label, out int frame) && frame >= 30 && frame <= 300;
        }

        private static bool ShouldValidatePhase2AkazaPlayLockFrame(
            string label,
            bool cinematicPlaying,
            bool anyLockDisabled)
        {
            return TryParsePhase2AkazaPlayFrame(label, out int frame)
                && frame >= 10
                && (cinematicPlaying || anyLockDisabled);
        }

        private static bool ShouldCapturePhase2AkazaPlayInspectFrame(string label)
        {
            return string.Equals(label, "edit-before-play", StringComparison.Ordinal)
                || string.Equals(label, "entered-play-mode", StringComparison.Ordinal)
                || string.Equals(label, "play-frame-1", StringComparison.Ordinal)
                || string.Equals(label, "play-frame-10", StringComparison.Ordinal)
                || string.Equals(label, "play-frame-30", StringComparison.Ordinal)
                || string.Equals(label, "play-frame-60", StringComparison.Ordinal)
                || string.Equals(label, "play-frame-120", StringComparison.Ordinal)
                || string.Equals(label, "play-frame-180", StringComparison.Ordinal)
                || string.Equals(label, "play-frame-240", StringComparison.Ordinal)
                || string.Equals(label, "play-frame-300", StringComparison.Ordinal);
        }

        private static void CapturePhase2AkazaPlayInspectFrame(string label, Scene scene)
        {
            if (!ShouldCapturePhase2AkazaPlayInspectFrame(label))
            {
                return;
            }

            const int CaptureWidth = 1280;
            const int CaptureHeight = 720;

            Camera camera = ResolvePhase2AkazaValidationCamera(scene);
            string projectRoot = Path.GetDirectoryName(Application.dataPath) ?? Directory.GetCurrentDirectory();
            string outputDirectory = Path.Combine(projectRoot, Phase2AkazaPlayInspectCaptureDirectory);
            Directory.CreateDirectory(outputDirectory);
            string outputPath = Path.Combine(
                outputDirectory,
                $"phase2-akaza-{label}.png");

            RenderTexture previousTarget = camera.targetTexture;
            RenderTexture previousActive = RenderTexture.active;
            RenderTexture renderTexture = new RenderTexture(
                CaptureWidth,
                CaptureHeight,
                24,
                RenderTextureFormat.ARGB32);
            Texture2D image = new Texture2D(
                CaptureWidth,
                CaptureHeight,
                TextureFormat.RGBA32,
                mipChain: false);

            try
            {
                RenderPhase2AkazaCameraToTexture(camera, renderTexture);
                RenderTexture.active = renderTexture;
                image.ReadPixels(new Rect(0, 0, CaptureWidth, CaptureHeight), 0, 0);
                image.Apply();

                AnalyzePhase2AkazaCaptureTexture(
                    image,
                    out int nonBlackSamples,
                    out int saturatedSamples);
                if (nonBlackSamples == 0)
                {
                    RecordPhase2AkazaPlayInspectFailure(
                        $"{label} camera capture was blank.");
                }

                File.WriteAllBytes(outputPath, image.EncodeToPNG());
                Debug.Log(
                    $"{Phase2AkazaPlayInspectPrefix} {label} capture path={outputPath}, "
                    + $"nonBlackSamples={nonBlackSamples}, saturatedSamples={saturatedSamples}, "
                    + $"size={CaptureWidth}x{CaptureHeight}");

                if (string.Equals(label, "edit-before-play", StringComparison.Ordinal))
                {
                    CapturePhase2AkazaCombatCloseup(scene, outputDirectory, CaptureWidth, CaptureHeight);
                }
            }
            finally
            {
                camera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                UnityEngine.Object.DestroyImmediate(image);
                UnityEngine.Object.DestroyImmediate(renderTexture);
            }
        }

        private static void CapturePhase2AkazaCombatCloseup(
            Scene scene,
            string outputDirectory,
            int captureWidth,
            int captureHeight)
        {
            GameObject bossProxy = RequireRoot(scene, BossProxyRootName);
            Transform visual = FindPhase2AkazaVisual(bossProxy.transform);
            if (visual == null)
            {
                return;
            }

            Bounds bounds = CalculateRendererBounds(visual.gameObject);
            Vector3 target = bounds.center + Vector3.up * 0.18f;
            GameObject cameraObject = new GameObject("Phase2Akaza_CombatCloseupCaptureCamera");
            SceneManager.MoveGameObjectToScene(cameraObject, scene);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.enabled = false;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.31f, 0.33f, 0.34f, 1f);
            camera.nearClipPlane = 0.03f;
            camera.farClipPlane = 120f;
            camera.fieldOfView = 28f;
            ConfigurePhase2AkazaC08SourceCameraPostProcessing(camera);
            camera.transform.position = target + Vector3.back * 5.1f + Vector3.up * 0.1f;
            camera.transform.rotation = Quaternion.LookRotation(target - camera.transform.position, Vector3.up);

            RenderTexture previousActive = RenderTexture.active;
            RenderTexture renderTexture = new RenderTexture(
                captureWidth,
                captureHeight,
                24,
                RenderTextureFormat.ARGB32);
            Texture2D image = new Texture2D(
                captureWidth,
                captureHeight,
                TextureFormat.RGBA32,
                mipChain: false);
            string outputPath = Path.Combine(outputDirectory, "phase2-akaza-edit-before-play-combat-closeup.png");

            try
            {
                RenderPhase2AkazaCameraToTexture(camera, renderTexture);
                RenderTexture.active = renderTexture;
                image.ReadPixels(new Rect(0, 0, captureWidth, captureHeight), 0, 0);
                image.Apply();
                File.WriteAllBytes(outputPath, image.EncodeToPNG());
                Debug.Log($"{Phase2AkazaPlayInspectPrefix} edit-before-play combat closeup path={outputPath}");
            }
            finally
            {
                RenderTexture.active = previousActive;
                UnityEngine.Object.DestroyImmediate(image);
                UnityEngine.Object.DestroyImmediate(renderTexture);
                UnityEngine.Object.DestroyImmediate(cameraObject);
            }
        }

        private static void RenderPhase2AkazaCameraToTexture(Camera camera, RenderTexture renderTexture)
        {
            UniversalRenderPipeline.SingleCameraRequest request =
                new UniversalRenderPipeline.SingleCameraRequest
                {
                    destination = renderTexture
                };
            if (RenderPipeline.SupportsRenderRequest(camera, request))
            {
                RenderPipeline.SubmitRenderRequest(camera, request);
                return;
            }

            camera.targetTexture = renderTexture;
            camera.Render();
        }

        private static void CapturePhase2AkazaSourceFrame(Scene scene, int sourceFrame)
        {
            CinematicSequenceProfile introProfile =
                LoadAsset<CinematicSequenceProfile>(Phase2AkazaIntroProfilePath);
            if (!TryFindPhase2AkazaIntroRunner(scene, out CinematicSequenceRunner runner))
            {
                throw new InvalidOperationException("Phase2 Akaza intro runner missing for source-frame capture.");
            }

            float sequenceSecond =
                (sourceFrame - Phase2AkazaIntroSourceStartFrame) / Phase2AkazaIntroSourceFrameRate;
            if (!runner.TryApplyProfileSampleForReview(introProfile, sequenceSecond, Vector3.back))
            {
                throw new InvalidOperationException(
                    $"Phase2 Akaza intro runner could not sample source frame {sourceFrame}.");
            }

            Camera mainCamera = ResolvePhase2AkazaValidationCamera(scene);
            GameObject wrapper = RequireRoot(scene, Phase2AkazaC23CameraRigWrapperName);
            string cameraRigName = Path.GetFileNameWithoutExtension(Phase2AkazaC08CameraSourcePath);
            string sourceActorRigName = Path.GetFileNameWithoutExtension(Phase2AkazaC08ActorSourcePath);
            GameObject sourceCameraRig = RequireChildRecursive(wrapper.transform, cameraRigName).gameObject;
            GameObject sourceActor = RequireChildRecursive(wrapper.transform, sourceActorRigName).gameObject;
            Camera sourceCamera = sourceCameraRig.GetComponentInChildren<Camera>(includeInactive: true);
            if (sourceCamera == null)
            {
                throw new InvalidOperationException($"{cameraRigName} must contain a source camera.");
            }

            Bounds bounds = CalculateRendererBounds(sourceActor);
            int activeRendererCount = CountActiveRenderableRenderers(sourceActor);
            Vector3 viewportCenter = mainCamera.WorldToViewportPoint(bounds.center);
            float viewportHeight = EstimateViewportHeight(mainCamera, bounds);
            CalculateViewportRect(mainCamera, bounds, out Vector2 viewportMin, out Vector2 viewportMax, out int projectedCorners);
            bool inFrustum = GeometryUtility.TestPlanesAABB(
                GeometryUtility.CalculateFrustumPlanes(mainCamera),
                bounds);
            float cameraDistance = Vector3.Distance(mainCamera.transform.position, sourceCamera.transform.position);
            float cameraAngle = Quaternion.Angle(mainCamera.transform.rotation, sourceCamera.transform.rotation);
            float fovDelta = Mathf.Abs(mainCamera.fieldOfView - sourceCamera.fieldOfView);
            bool actorReadable = sourceActor.activeInHierarchy
                && activeRendererCount > 0
                && inFrustum
                && projectedCorners > 0
                && viewportCenter.z > mainCamera.nearClipPlane
                && viewportCenter.z < mainCamera.farClipPlane
                && viewportHeight >= 0.08f;
            bool cameraMatchesSource = cameraDistance <= 0.05f
                && cameraAngle <= 0.5f
                && fovDelta <= 0.1f;
            LogPhase2AkazaSourceFrameCaptureState(
                sourceCameraRig,
                sourceCamera,
                sourceActor,
                bounds);
            if (!actorReadable || !cameraMatchesSource || sourceCamera.enabled)
            {
                throw new InvalidOperationException(
                    $"{Phase2AkazaTimelineParityPrefix} source-frame-{sourceFrame} capture is not on the original source camera: "
                    + $"actorReadable={actorReadable}, cameraMatchesSource={cameraMatchesSource}, "
                    + $"sourceCameraEnabled={sourceCamera.enabled}, "
                    + $"activeRenderers={activeRendererCount}, center={FormatVector3(viewportCenter)}, "
                    + $"height={viewportHeight:0.000}, rect={FormatVector2(viewportMin)}-{FormatVector2(viewportMax)}, "
                    + $"cameraDistance={cameraDistance:0.000}, cameraAngle={cameraAngle:0.000}, fovDelta={fovDelta:0.000}.");
            }

            string projectRoot = Path.GetDirectoryName(Application.dataPath) ?? Directory.GetCurrentDirectory();
            string outputDirectory = Path.Combine(projectRoot, Phase2AkazaTimelineParityCaptureDirectory);
            Directory.CreateDirectory(outputDirectory);
            string outputPath = Path.Combine(outputDirectory, $"phase2-akaza-source-frame-{sourceFrame}.png");
            float screenFadeAlpha = EvaluatePhase2AkazaScreenFadeAlpha(introProfile, sequenceSecond);
            RenderPhase2AkazaCameraCapture(
                mainCamera,
                outputPath,
                Phase2AkazaTimelineParityPrefix,
                $"source-frame-{sourceFrame}",
                screenFadeAlpha);

            Debug.Log(
                $"{Phase2AkazaTimelineParityPrefix} source-frame-{sourceFrame} "
                + $"cameraRig={sourceCameraRig.name}, sourceRig={sourceActor.name}, "
                + $"viewportCenter={FormatVector3(viewportCenter)}, viewportHeight={viewportHeight:0.000}, "
                + $"viewportMin={FormatVector2(viewportMin)}, viewportMax={FormatVector2(viewportMax)}, "
                + $"projectedCorners={projectedCorners}, inFrustum={inFrustum}, capture={outputPath}");
        }

        private static void RenderPhase2AkazaCameraCapture(
            Camera camera,
            string outputPath,
            string logPrefix,
            string label,
            float blackFadeAlpha = 0f)
        {
            const int CaptureWidth = 1280;
            const int CaptureHeight = 720;

            RenderTexture previousTarget = camera.targetTexture;
            RenderTexture previousActive = RenderTexture.active;
            RenderTexture renderTexture = new RenderTexture(
                CaptureWidth,
                CaptureHeight,
                24,
                RenderTextureFormat.ARGB32);
            Texture2D image = new Texture2D(
                CaptureWidth,
                CaptureHeight,
                TextureFormat.RGBA32,
                mipChain: false);

            try
            {
                RenderPhase2AkazaCameraToTexture(camera, renderTexture);
                RenderTexture.active = renderTexture;
                image.ReadPixels(new Rect(0, 0, CaptureWidth, CaptureHeight), 0, 0);
                image.Apply();

                AnalyzePhase2AkazaCaptureTexture(
                    image,
                    out int sourceNonBlackSamples,
                    out int sourceSaturatedSamples);
                if (sourceNonBlackSamples == 0)
                {
                    throw new InvalidOperationException($"{label} camera capture was blank.");
                }

                ApplyPhase2AkazaCaptureBlackFade(image, blackFadeAlpha);
                AnalyzePhase2AkazaCaptureTexture(
                    image,
                    out int fadedNonBlackSamples,
                    out int fadedSaturatedSamples);

                File.WriteAllBytes(outputPath, image.EncodeToPNG());
                Debug.Log(
                    $"{logPrefix} {label} capture path={outputPath}, "
                    + $"sourceNonBlackSamples={sourceNonBlackSamples}, sourceSaturatedSamples={sourceSaturatedSamples}, "
                    + $"fadedNonBlackSamples={fadedNonBlackSamples}, fadedSaturatedSamples={fadedSaturatedSamples}, "
                    + $"blackFadeAlpha={Mathf.Clamp01(blackFadeAlpha):0.###}, size={CaptureWidth}x{CaptureHeight}");
            }
            finally
            {
                camera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                UnityEngine.Object.DestroyImmediate(image);
                UnityEngine.Object.DestroyImmediate(renderTexture);
            }
        }

        private static float EvaluatePhase2AkazaScreenFadeAlpha(
            CinematicSequenceProfile introProfile,
            float sequenceSecond)
        {
            if (introProfile == null)
            {
                return 0f;
            }

            CinematicSequenceProfile.ScreenFadeCue[] cues = introProfile.ScreenFadeCues;
            int selectedIndex = -1;
            float selectedStartSeconds = -1f;
            for (int i = 0; i < cues.Length; i++)
            {
                CinematicSequenceProfile.ScreenFadeCue cue = cues[i];
                if (!cue.Enabled || sequenceSecond < cue.StartSeconds)
                {
                    continue;
                }

                if (selectedIndex < 0 || cue.StartSeconds >= selectedStartSeconds)
                {
                    selectedIndex = i;
                    selectedStartSeconds = cue.StartSeconds;
                }
            }

            return selectedIndex >= 0 ? cues[selectedIndex].EvaluateAlpha(sequenceSecond) : 0f;
        }

        private static void ApplyPhase2AkazaCaptureBlackFade(Texture2D image, float blackFadeAlpha)
        {
            float alpha = Mathf.Clamp01(blackFadeAlpha);
            if (image == null || alpha <= 0.0001f)
            {
                return;
            }

            byte multiplier = (byte)Mathf.RoundToInt(255f * (1f - alpha));
            Color32[] pixels = image.GetPixels32();
            for (int i = 0; i < pixels.Length; i++)
            {
                Color32 pixel = pixels[i];
                pixel.r = (byte)((pixel.r * multiplier) / 255);
                pixel.g = (byte)((pixel.g * multiplier) / 255);
                pixel.b = (byte)((pixel.b * multiplier) / 255);
                pixels[i] = pixel;
            }

            image.SetPixels32(pixels);
            image.Apply();
        }

        private static void LogPhase2AkazaSourceFrameCaptureState(
            GameObject sourceCameraRig,
            Camera sourceCamera,
            GameObject sourceActor,
            Bounds actorBounds)
        {
            Debug.Log(
                $"{Phase2AkazaTimelineParityPrefix} source-frame-debug "
                + $"cameraPath={BuildPhase2AkazaTransformPath(sourceCamera.transform)}, "
                + $"cameraPos={FormatVector3(sourceCamera.transform.position)}, "
                + $"cameraRot={FormatQuaternion(sourceCamera.transform.rotation)}, "
                + $"cameraEuler={FormatVector3(sourceCamera.transform.rotation.eulerAngles)}, "
                + $"fov={sourceCamera.fieldOfView:0.###}, "
                + $"cameraRigPrefab={NormalizeAssetPath(AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromOriginalSource(sourceCameraRig)))}, "
                + $"actorPath={BuildPhase2AkazaTransformPath(sourceActor.transform)}, "
                + $"actorPos={FormatVector3(sourceActor.transform.position)}, "
                + $"actorBoundsCenter={FormatVector3(actorBounds.center)}, "
                + $"actorBoundsSize={FormatVector3(actorBounds.size)}, "
                + $"actorPrefab={NormalizeAssetPath(AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromOriginalSource(sourceActor)))}");

            Renderer[] renderers = sourceActor.GetComponentsInChildren<Renderer>(includeInactive: true);
            Array.Sort(
                renderers,
                (left, right) => string.Compare(
                    BuildPhase2AkazaTransformPath(left.transform),
                    BuildPhase2AkazaTransformPath(right.transform),
                    StringComparison.Ordinal));
            int logged = 0;
            for (int i = 0; i < renderers.Length && logged < 96; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy)
                {
                    continue;
                }

                string materialSummary = BuildPhase2AkazaRendererMaterialSummary(renderer);
                Debug.Log(
                    $"{Phase2AkazaTimelineParityPrefix} source-frame-renderer[{logged}] "
                    + $"{BuildPhase2AkazaTransformPath(renderer.transform)} "
                    + $"center={FormatVector3(renderer.bounds.center)} size={FormatVector3(renderer.bounds.size)} "
                    + $"materials={materialSummary}");
                logged++;
            }
        }

        private static string BuildPhase2AkazaRendererMaterialSummary(Renderer renderer)
        {
            Material[] materials = renderer.sharedMaterials;
            string[] names = new string[materials.Length];
            for (int i = 0; i < materials.Length; i++)
            {
                Material material = materials[i];
                if (material == null)
                {
                    names[i] = "<null>";
                    continue;
                }

                float alphaClip = material.HasProperty("_AlphaClip") ? material.GetFloat("_AlphaClip") : -1f;
                float surface = material.HasProperty("_Surface") ? material.GetFloat("_Surface") : -1f;
                names[i] = $"{material.name}(shader={material.shader.name},surface={surface:0.#},alphaClip={alphaClip:0.#})";
            }

            return string.Join("|", names);
        }

        private static void AnalyzePhase2AkazaCaptureTexture(
            Texture2D image,
            out int nonBlackSamples,
            out int saturatedSamples)
        {
            nonBlackSamples = 0;
            saturatedSamples = 0;
            int stepX = Mathf.Max(1, image.width / 32);
            int stepY = Mathf.Max(1, image.height / 18);
            for (int y = 0; y < image.height; y += stepY)
            {
                for (int x = 0; x < image.width; x += stepX)
                {
                    Color32 pixel = image.GetPixel(x, y);
                    int max = Mathf.Max(pixel.r, Mathf.Max(pixel.g, pixel.b));
                    int min = Mathf.Min(pixel.r, Mathf.Min(pixel.g, pixel.b));
                    if (max > 8)
                    {
                        nonBlackSamples++;
                    }

                    if (max - min > 24)
                    {
                        saturatedSamples++;
                    }
                }
            }
        }

        private static bool TryParsePhase2AkazaPlayFrame(string label, out int frame)
        {
            const string Prefix = "play-frame-";
            frame = 0;
            return label.StartsWith(Prefix, StringComparison.Ordinal)
                && int.TryParse(label.Substring(Prefix.Length), out frame);
        }

        private static bool TryFindPhase2AkazaIntroRunner(
            Scene scene,
            out CinematicSequenceRunner runner)
        {
            if (scene.IsValid())
            {
                GameObject[] roots = scene.GetRootGameObjects();
                for (int i = 0; i < roots.Length; i++)
                {
                    CinematicSequenceRunner[] runners =
                        roots[i].GetComponentsInChildren<CinematicSequenceRunner>(includeInactive: true);
                    for (int j = 0; j < runners.Length; j++)
                    {
                        CinematicSequenceProfile profile = runners[j].SequenceProfile;
                        if (profile != null
                            && string.Equals(
                                AssetDatabase.GetAssetPath(profile),
                                Phase2AkazaIntroProfilePath,
                                StringComparison.Ordinal))
                        {
                            runner = runners[j];
                            return true;
                        }
                    }
                }
            }

            runner = null;
            return false;
        }

        private static void RecordPhase2AkazaPlayInspectFailure(string message)
        {
            SessionState.SetBool(Phase2AkazaPlayInspectFailureKey, true);
            Debug.LogError($"{Phase2AkazaPlayInspectPrefix} {message}");
        }

        private static string FormatVector3(Vector3 value)
        {
            return $"({value.x:0.###},{value.y:0.###},{value.z:0.###})";
        }

        private static string FormatVector2(Vector2 value)
        {
            return $"({value.x:0.###},{value.y:0.###})";
        }

        private static string FormatQuaternion(Quaternion value)
        {
            return $"({value.x:0.###},{value.y:0.###},{value.z:0.###},{value.w:0.###})";
        }

        private static string BuildPhase2AkazaTransformPath(Transform transform)
        {
            if (transform == null)
            {
                return "null";
            }

            Stack<string> names = new Stack<string>();
            Transform current = transform;
            while (current != null)
            {
                names.Push(current.name);
                current = current.parent;
            }

            return string.Join("/", names.ToArray());
        }

        private static string NameOrNull(UnityEngine.Object unityObject)
        {
            return unityObject != null ? unityObject.name : "null";
        }

        private static void DumpPhase2AkazaAnimationCurves()
        {
            string[] clipPaths =
            {
                Phase2AkazaC08CameraSourcePath,
                Phase2AkazaC08ActorSourcePath,
                Phase2AkazaAnimationSourceRoot + "/C25_akaza.fbx",
                Phase2AkazaAnimationSourceRoot + "/C27_akaza.fbx",
                Phase2AkazaAnimationSourceRoot + "/C30_akaza.fbx",
                Phase2AkazaAnimationSourceRoot + "/C33_Akaza.fbx",
                Phase2AkazaAnimationSourceRoot + "/C34_Akaza.fbx"
            };

            for (int i = 0; i < clipPaths.Length; i++)
            {
                AnimationClip clip = LoadPrimaryAnimationClip(clipPaths[i]);
                if (clip == null)
                {
                    Debug.Log($"{Phase2AkazaPlayInspectPrefix} curves {clipPaths[i]} missing clip.");
                    continue;
                }

                DumpPhase2AkazaAnimationCurveRanges(clip);
            }
        }

        private static void DumpPhase2AkazaAnimationCurveRanges(AnimationClip clip)
        {
            List<KeyValuePair<float, string>> rows = new List<KeyValuePair<float, string>>();
            EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(clip);
            for (int i = 0; i < bindings.Length; i++)
            {
                EditorCurveBinding binding = bindings[i];
                if (!binding.propertyName.StartsWith("m_LocalPosition.", StringComparison.Ordinal))
                {
                    continue;
                }

                AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);
                if (curve == null || curve.length == 0)
                {
                    continue;
                }

                float min = float.PositiveInfinity;
                float max = float.NegativeInfinity;
                Keyframe[] keys = curve.keys;
                for (int keyIndex = 0; keyIndex < keys.Length; keyIndex++)
                {
                    float value = keys[keyIndex].value;
                    min = Mathf.Min(min, value);
                    max = Mathf.Max(max, value);
                }

                float maxAbs = Mathf.Max(Mathf.Abs(min), Mathf.Abs(max));
                if (maxAbs < 1.5f)
                {
                    continue;
                }

                string row =
                    $"{clip.name} path='{binding.path}' property={binding.propertyName} "
                    + $"min={min:0.###} max={max:0.###} keys={curve.length}";
                rows.Add(new KeyValuePair<float, string>(maxAbs, row));
            }

            rows.Sort((left, right) => right.Key.CompareTo(left.Key));
            int count = Mathf.Min(24, rows.Count);
            Debug.Log(
                $"{Phase2AkazaPlayInspectPrefix} curves {clip.name} suspiciousPositionCurves={rows.Count}");
            for (int i = 0; i < count; i++)
            {
                Debug.Log($"{Phase2AkazaPlayInspectPrefix} curves {rows[i].Value}");
            }
        }

        public static void EnsurePhase2AkazaBossReviewScene()
        {
            EnsurePhase2AkazaPromotedAssets();

            BossBarragePatternProfile hoverLance = EnsurePhase2AkazaHoverLancePatternProfile();
            BossBarragePatternProfile spiralVolley = EnsurePhase2AkazaSpiralVolleyPatternProfile();
            BossBarragePatternProfile summonCurtain = EnsurePhase2AkazaSummonCurtainPatternProfile();
            BossBarragePatternProfile crushNet = EnsurePhase2AkazaCrushNetPatternProfile();
            BossBasicFireProfile basicFireProfile = EnsurePhase2AkazaBasicFireProfile();
            BossSummonPressureProfile summonPressureProfile = EnsurePhase2AkazaSummonPressureProfile();
            BossPressureActionDeckProfile actionDeckProfile = EnsurePhase2AkazaPressureActionDeck(
                hoverLance,
                spiralVolley,
                summonCurtain,
                crushNet);
            EnsurePhase2AkazaProjectilePrefab();
            GameObject akazaPrefab = EnsurePhase2AkazaPrefab();
            CinematicSequenceProfile introProfile = EnsurePhase2AkazaBossIntroProfile();

            EnsurePhase2AkazaSceneAssetExists();
            Scene scene = EditorSceneManager.OpenScene(Phase2AkazaReviewScenePath, OpenSceneMode.Single);
            GameObject bossProxy = RequireRoot(scene, BossProxyRootName);
            ApplyPhase2AkazaBossProxy(
                scene,
                bossProxy,
                hoverLance,
                spiralVolley,
                summonCurtain,
                crushNet,
                basicFireProfile,
                summonPressureProfile,
                actionDeckProfile,
                akazaPrefab,
                introProfile);

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, Phase2AkazaReviewScenePath))
            {
                throw new InvalidOperationException(
                    $"Failed to save phase2 Akaza boss review scene at {Phase2AkazaReviewScenePath}.");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void ValidatePhase2AkazaBossReviewScene()
        {
            ValidateNoImportedAssetReference(Phase2AkazaReviewScenePath);
            ValidateNoImportedAssetReference(Phase2AkazaPrefabPath);
            ValidateNoImportedAssetReference(Phase2AkazaProjectilePrefabPath);
            ValidateNoImportedAssetReference(Phase2AkazaAnimatorControllerPath);
            ValidateNoImportedAssetReference(Phase2AkazaC08CameraSourcePath);
            ValidateNoImportedAssetReference(Phase2AkazaC08ActorSourcePath);
            ValidateNoImportedAssetReference(Phase2AkazaC23IntroClipPath);
            ValidateNoImportedAssetReference(Phase2AkazaC25InPlaceClipPath);
            ValidateNoImportedAssetReference(Phase2AkazaC27InPlaceClipPath);
            ValidateNoImportedAssetReference(Phase2AkazaC30InPlaceClipPath);
            ValidateNoImportedAssetReference(Phase2AkazaC34InPlaceClipPath);
            ValidateNoImportedAssetReference(Phase2AkazaCombatCueClipPath);
            ValidateNoImportedAssetReference(Phase2AkazaIntroProfilePath);
            ValidateNoImportedAssetReference(Phase2AkazaDeckProfilePath);
            ValidateNoImportedAssetReference(Phase2AkazaSummonPressureProfilePath);
            ValidateNoImportedAssetReference(Phase2AkazaHoverLancePatternProfilePath);
            ValidateNoImportedAssetReference(Phase2AkazaSpiralVolleyPatternProfilePath);
            ValidateNoImportedAssetReference(Phase2AkazaSummonCurtainPatternProfilePath);
            ValidateNoImportedAssetReference(Phase2AkazaCrushNetPatternProfilePath);

            Scene scene = EditorSceneManager.OpenScene(Phase2AkazaReviewScenePath, OpenSceneMode.Single);
            GameObject bossProxy = RequireRoot(scene, BossProxyRootName);
            Transform visual = RequireChild(bossProxy.transform, Phase2AkazaVisualName);
            Animator animator = visual.GetComponentInChildren<Animator>(includeInactive: true);
            if (animator == null)
            {
                throw new InvalidOperationException($"{Phase2AkazaVisualName} must expose an Animator.");
            }

            ValidateGameOwnedAsset(animator.runtimeAnimatorController, "phase2 Akaza Animator Controller");
            ValidateGameOwnedAsset(animator.avatar, "phase2 Akaza Avatar");
            ValidateGameOwnedAsset(
                AssetDatabase.LoadAssetAtPath<GameObject>(Phase2AkazaPrefabPath),
                "phase2 Akaza prefab");
            ValidateAkazaAnimatorTrigger(animator, "EliteAuraBuffer");
            ValidateAkazaAnimatorTrigger(animator, "AttackLinePressure");
            ValidateAkazaAnimatorTrigger(animator, "EliteSummonPackage");
            ValidateAkazaAnimatorTrigger(animator, "AttackFanPressure");
            ValidateAkazaAnimatorTrigger(animator, "AttackHeavy");
            ValidateAkazaAnimatorTrigger(animator, "ElitePhaseSwap");
            ValidateAkazaAnimatorPlayStartPose(animator);
            ValidatePhase2AkazaInPlaceClip(Phase2AkazaC23IntroClipPath);
            ValidatePhase2AkazaInPlaceClip(Phase2AkazaC25InPlaceClipPath);
            ValidatePhase2AkazaInPlaceClip(Phase2AkazaC27InPlaceClipPath);
            ValidatePhase2AkazaInPlaceClip(Phase2AkazaC30InPlaceClipPath);
            ValidatePhase2AkazaInPlaceClip(Phase2AkazaC34InPlaceClipPath);
            ValidatePhase2AkazaCombatCueClock(visual);
            AnimationClip combatCueClip = LoadAsset<AnimationClip>(Phase2AkazaCombatCueClipPath);
            ValidateAkazaAnimatorStateMotion(animator, "IntroThreatRise", combatCueClip);
            ValidateAkazaAnimatorStateMotion(
                animator,
                "IntroReveal1412_1562",
                combatCueClip);
            ValidateAkazaAnimatorStateMotion(animator, "IntroPressureHandoff", combatCueClip);
            ValidateAkazaAnimatorStateMotion(animator, "Windup", combatCueClip);
            ValidateAkazaAnimatorStateMotion(animator, "LinePressure", combatCueClip);
            ValidateAkazaAnimatorStateMotion(animator, "RetreatShot", combatCueClip);
            ValidateAkazaAnimatorStateMotion(animator, "FanPressure", combatCueClip);
            ValidateAkazaAnimatorStateMotion(animator, "HeavyCrush", combatCueClip);
            ValidateAkazaAnimatorStateMotion(animator, "SummonPackage", combatCueClip);
            ValidateAkazaAnimatorStateMotion(animator, "PhaseSwap", combatCueClip);
            ValidateAkazaAnimatorStateMotion(animator, "BasicAttack", combatCueClip);
            ValidateAkazaAnimatorStateMotion(animator, "Hit", combatCueClip);
            ValidateAkazaAnimatorStateMotion(animator, "Death", combatCueClip);

            ActionFoundationArenaTransformMotion hoverMotion =
                visual.GetComponent<ActionFoundationArenaTransformMotion>();
            if (hoverMotion == null)
            {
                throw new InvalidOperationException($"{Phase2AkazaVisualName} must keep hover motion.");
            }

            ValidateBool(hoverMotion, "lockAuthoredLocalRotation", true);
            ValidateBool(hoverMotion, "lockAuthoredLocalScale", true);
            ValidatePhase2AkazaRenderableVisual(visual);
            Bounds bounds = CalculateRendererBounds(visual.gameObject);
            if (bounds.min.y < 0.28f)
            {
                throw new InvalidOperationException(
                    $"{Phase2AkazaVisualName} should hover above the lane floor; bottom was {bounds.min.y:0.00}.");
            }

            ValidatePhase2AkazaCameraVisibility(scene, visual.gameObject, bounds);
            ValidatePhase2AkazaBossIntro(scene, visual.gameObject, animator);

            BossBarrageEmitter emitter = RequireComponent<BossBarrageEmitter>(bossProxy, "phase2 boss emitter");
            ValidateObjectReference(
                emitter,
                "projectilePrefab",
                LoadPrefabComponent<BossBarrageProjectile>(Phase2AkazaProjectilePrefabPath));
            ValidatePhase2AkazaPatternSequence(emitter);

            BossBasicFireEmitter basicFire = RequireComponent<BossBasicFireEmitter>(
                bossProxy,
                "phase2 boss basic fire");
            ValidateObjectReference(
                basicFire,
                "fireProfile",
                LoadAsset<BossBasicFireProfile>(Phase2AkazaBasicFireProfilePath));
            ValidateFloat(
                LoadAsset<BossBasicFireProfile>(Phase2AkazaBasicFireProfilePath),
                "initialDelaySeconds",
                0.85f);

            BossPressureActionDirector actionDirector = RequireComponent<BossPressureActionDirector>(
                bossProxy,
                "phase2 boss pressure director");
            ValidateObjectReference(
                actionDirector,
                "actionDeckProfile",
                LoadAsset<BossPressureActionDeckProfile>(Phase2AkazaDeckProfilePath));
            ValidateBool(actionDirector, "holdForNextTierActionWhenGateAllows", true);

            BossSummonPressureAction summonPressure = RequireComponent<BossSummonPressureAction>(
                bossProxy,
                "phase2 boss summon pressure");
            ValidateObjectReference(
                summonPressure,
                "pressureProfile",
                LoadAsset<BossSummonPressureProfile>(Phase2AkazaSummonPressureProfilePath));
            ValidateInt(summonPressure, "maxActiveSummonActors", 2);

            BossPressurePositionController positionController = RequireComponent<BossPressurePositionController>(
                bossProxy,
                "phase2 boss pressure position");
            ValidateFloat(positionController, "restRisk01", 0.2f);
            ValidateFloat(positionController, "maxCommitRisk01", 0.82f);

            BossBarrageVisualCueDriver cueDriver = RequireComponent<BossBarrageVisualCueDriver>(
                bossProxy,
                "phase2 boss cue driver");
            ValidateObjectReference(cueDriver, "animator", animator);
            ValidateIntValueAtLeast(cueDriver.PatternCueCount, 4, "phase2 Akaza pattern cue count");
            ValidateIntValueAtLeast(cueDriver.PressureActionCueCount, 3, "phase2 Akaza pressure cue count");

            Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(includeInactive: true);
            if (renderers.Length == 0)
            {
                throw new InvalidOperationException($"{Phase2AkazaVisualName} must expose promoted renderers.");
            }

            for (int i = 0; i < renderers.Length; i++)
            {
                ValidateRendererAssets(renderers[i], $"{Phase2AkazaVisualName}.{renderers[i].name}");
            }

            ValidatePhase2AkazaCombatToonMaterials(visual);
            ValidateNoDirectImportedSceneDependencies(Phase2AkazaReviewScenePath);
        }

        private static void ValidatePhase2AkazaCombatToonMaterials(Transform visual)
        {
            Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(includeInactive: true);
            int c08MaterialCount = 0;
            List<string> simpleMaterialUsers = new List<string>();
            List<string> overwideOutlineUsers = new List<string>();
            for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
            {
                Renderer renderer = renderers[rendererIndex];
                if (renderer == null)
                {
                    continue;
                }

                Material[] materials = renderer.sharedMaterials;
                for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                {
                    Material material = materials[materialIndex];
                    if (material == null)
                    {
                        continue;
                    }

                    string materialPath = NormalizeAssetPath(AssetDatabase.GetAssetPath(material));
                    if (materialPath.StartsWith(
                            NormalizeAssetPath(Phase2AkazaIntroSourceMaterialRoot + "/M_C08_Akaza_"),
                            StringComparison.Ordinal))
                    {
                        c08MaterialCount++;
                        if (material.HasProperty("_Outline_Width"))
                        {
                            float outlineWidth = material.GetFloat("_Outline_Width");
                            if (outlineWidth > 0.45f)
                            {
                                overwideOutlineUsers.Add($"{renderer.name}:{material.name}({outlineWidth:0.###})");
                            }
                        }
                    }

                    if (materialPath.StartsWith(
                            NormalizeAssetPath(Phase2AkazaMaterialRoot + "/M_Akaza_"),
                            StringComparison.Ordinal))
                    {
                        simpleMaterialUsers.Add($"{renderer.name}:{material.name}");
                    }
                }
            }

            if (simpleMaterialUsers.Count > 0)
            {
                throw new InvalidOperationException(
                    $"{Phase2AkazaVisualName} must use C08 source toon materials, not simple combat materials: "
                    + string.Join(", ", simpleMaterialUsers));
            }

            if (overwideOutlineUsers.Count > 0)
            {
                throw new InvalidOperationException(
                    $"{Phase2AkazaVisualName} C08 toon materials must keep restrained combat outline widths: "
                    + string.Join(", ", overwideOutlineUsers));
            }

            if (c08MaterialCount < 6)
            {
                throw new InvalidOperationException(
                    $"{Phase2AkazaVisualName} must keep the C08 Akaza toon material set; found only {c08MaterialCount} C08 materials.");
            }
        }

        private static void EnsurePhase2AkazaPromotedAssets()
        {
            string sourceRoot = ResolveSiblingThePhantomKnowledgeAssetsPath();
            for (int i = 0; i < Phase2AkazaPromotedAssets.Length; i++)
            {
                PromotedAkazaAsset promotedAsset = Phase2AkazaPromotedAssets[i];
                string sourcePath = Path.Combine(
                    sourceRoot,
                    promotedAsset.SourceRelativePath.Replace('/', Path.DirectorySeparatorChar));
                if (!File.Exists(sourcePath))
                {
                    throw new InvalidOperationException(
                        $"Missing ThePhantomKnowledge Akaza source asset at {sourcePath}.");
                }

                EnsureFolderForAsset(promotedAsset.TargetAssetPath);
                string targetPath = ResolveProjectAbsolutePath(promotedAsset.TargetAssetPath);
                if (!File.Exists(targetPath))
                {
                    FileUtil.CopyFileOrDirectory(sourcePath, targetPath);
                }

                if (promotedAsset.ImportKind == AkazaImportKind.TextureWithSourceMeta)
                {
                    CopyPhase2AkazaSourceMetaIfMissing(sourcePath, targetPath);
                }

                AssetDatabase.ImportAsset(promotedAsset.TargetAssetPath, ImportAssetOptions.ForceUpdate);
                ConfigurePromotedAkazaImporter(promotedAsset);
            }
        }

        private static void CopyPhase2AkazaSourceMetaIfMissing(string sourcePath, string targetPath)
        {
            string sourceMetaPath = sourcePath + ".meta";
            string targetMetaPath = targetPath + ".meta";
            if (!File.Exists(sourceMetaPath))
            {
                return;
            }

            bool targetMetaHasGuid = File.Exists(targetMetaPath)
                && File.ReadAllText(targetMetaPath).Contains("guid:", StringComparison.Ordinal);
            if (targetMetaHasGuid)
            {
                return;
            }

            File.Copy(sourceMetaPath, targetMetaPath, overwrite: true);
        }

        private static string ResolveSiblingThePhantomKnowledgeAssetsPath()
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            if (string.IsNullOrWhiteSpace(projectRoot))
            {
                throw new InvalidOperationException("Could not resolve Unity project root.");
            }

            string parentRoot = Directory.GetParent(projectRoot)?.FullName;
            if (string.IsNullOrWhiteSpace(parentRoot))
            {
                throw new InvalidOperationException("Could not resolve sibling project root.");
            }

            string sourceRoot = Path.Combine(parentRoot, "ThePhantomKnowledge-1.0.0f3", "Assets");
            if (!Directory.Exists(sourceRoot))
            {
                throw new InvalidOperationException(
                    $"Expected ThePhantomKnowledge assets beside this project at {sourceRoot}.");
            }

            return sourceRoot;
        }

        private static string ResolveProjectAbsolutePath(string assetPath)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            if (string.IsNullOrWhiteSpace(projectRoot))
            {
                throw new InvalidOperationException("Could not resolve Unity project root.");
            }

            return Path.Combine(projectRoot, assetPath.Replace('/', Path.DirectorySeparatorChar));
        }

        private static void ConfigurePromotedAkazaImporter(PromotedAkazaAsset promotedAsset)
        {
            AssetImporter importer = AssetImporter.GetAtPath(promotedAsset.TargetAssetPath);
            switch (promotedAsset.ImportKind)
            {
                case AkazaImportKind.Model:
                    if (importer is ModelImporter modelImporter)
                    {
                        modelImporter.animationType = ModelImporterAnimationType.Generic;
                        modelImporter.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                        modelImporter.importAnimation = false;
                        modelImporter.importCameras = false;
                        modelImporter.importLights = false;
                        modelImporter.isReadable = true;
                        modelImporter.materialImportMode = ModelImporterMaterialImportMode.None;
                        modelImporter.SaveAndReimport();
                    }

                    break;
                case AkazaImportKind.Animation:
                    if (importer is ModelImporter animationImporter)
                    {
                        animationImporter.animationType = ModelImporterAnimationType.Generic;
                        animationImporter.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                        animationImporter.importAnimation = true;
                        animationImporter.importCameras = false;
                        animationImporter.importLights = false;
                        animationImporter.isReadable = true;
                        animationImporter.materialImportMode = ModelImporterMaterialImportMode.None;
                        animationImporter.SaveAndReimport();
                    }

                    break;
                case AkazaImportKind.CameraAnimation:
                    if (importer is ModelImporter cameraAnimationImporter)
                    {
                        cameraAnimationImporter.animationType = ModelImporterAnimationType.Generic;
                        cameraAnimationImporter.avatarSetup = ModelImporterAvatarSetup.NoAvatar;
                        cameraAnimationImporter.importAnimation = true;
                        cameraAnimationImporter.importCameras = true;
                        cameraAnimationImporter.importLights = false;
                        cameraAnimationImporter.isReadable = true;
                        cameraAnimationImporter.materialImportMode = ModelImporterMaterialImportMode.None;
                        cameraAnimationImporter.SaveAndReimport();
                    }

                    break;
                case AkazaImportKind.Texture:
                    if (importer is TextureImporter textureImporter)
                    {
                        textureImporter.textureType = TextureImporterType.Default;
                        textureImporter.sRGBTexture = true;
                        textureImporter.mipmapEnabled = true;
                        textureImporter.SaveAndReimport();
                    }

                    break;
                case AkazaImportKind.TextureWithSourceMeta:
                    break;
            }
        }

        private static void ConfigurePhase2AkazaAnimationRootLocks(ModelImporter animationImporter)
        {
            ModelImporterClipAnimation[] clips = animationImporter.defaultClipAnimations;
            if (clips == null || clips.Length == 0)
            {
                return;
            }

            for (int i = 0; i < clips.Length; i++)
            {
                ModelImporterClipAnimation clip = clips[i];
                clip.lockRootRotation = true;
                clip.keepOriginalOrientation = false;
                clip.lockRootHeightY = true;
                clip.keepOriginalPositionY = false;
                clip.heightFromFeet = false;
                clip.lockRootPositionXZ = true;
                clip.keepOriginalPositionXZ = false;
                clips[i] = clip;
            }

            animationImporter.clipAnimations = clips;
        }

        private static BossBarragePatternProfile EnsurePhase2AkazaHoverLancePatternProfile()
        {
            Material projectileMaterial =
                LoadOrCreateMaterial(Phase2AkazaProjectileMaterialPath, new Color(0.3f, 0.95f, 1f, 1f));
            BossBarragePatternProfile profile = LoadOrCreateBossBarragePatternProfile(
                Phase2AkazaHoverLancePatternProfilePath);
            var serializedObject = new SerializedObject(profile);
            RequireProperty(serializedObject, "patternId").stringValue = "AkazaHoverLance";
            ApplySkillGrammar(
                serializedObject,
                LaneSkillPatternFamily.LinePressure,
                LaneSkillTransferMode.SharedPvpSkillCandidate,
                "A phase-two rail lance that can become a costed committed lane skill, never basic fire.",
                "Read the rail windup, leave the marked side, or spend a summon screen before release.");
            ApplyTelegraphRead(
                serializedObject,
                new Color(0.2f, 0.9f, 1f, 0.76f),
                new Color(0.72f, 1f, 1f, 0.96f),
                0.42f,
                2.15f,
                1.45f);
            ApplyProjectileRead(
                serializedObject,
                new Color(0.32f, 0.95f, 1f, 1f),
                new Vector3(0.82f, 0.82f, 2.75f),
                projectileMaterial);
            RequireProperty(serializedObject, "targetingRule").enumValueIndex =
                (int)BossBarrageTargetingRule.TrackedPlayer;
            RequireProperty(serializedObject, "lateralShape").enumValueIndex =
                (int)BossBarrageLateralShape.LinePressure;
            RequireProperty(serializedObject, "initialDelaySeconds").floatValue = 0.35f;
            RequireProperty(serializedObject, "windupSeconds").floatValue = 0.82f;
            RequireProperty(serializedObject, "waveIntervalSeconds").floatValue = 4.6f;
            RequireProperty(serializedObject, "projectilesPerWave").intValue = 4;
            RequireProperty(serializedObject, "damage").floatValue = 11f;
            RequireProperty(serializedObject, "projectileSpeed").floatValue = 15.2f;
            RequireProperty(serializedObject, "projectileLifetimeSeconds").floatValue = 5.4f;
            RequireProperty(serializedObject, "projectileRadius").floatValue = 0.31f;
            RequireProperty(serializedObject, "backlineHalfSpread").floatValue = 4.15f;
            RequireProperty(serializedObject, "forwardHalfSpread").floatValue = 0f;
            RequireProperty(serializedObject, "linePressureDirection").floatValue = -1f;
            RequireProperty(serializedObject, "linePressureCenterRatio").floatValue = 0.78f;
            RequireProperty(serializedObject, "linePressureHalfSpreadRatio").floatValue = 0.07f;
            RequireProperty(serializedObject, "backlineDepthSpread").floatValue = 2.6f;
            RequireProperty(serializedObject, "forwardDepthSpread").floatValue = 0.72f;
            RequireProperty(serializedObject, "spawnHeight").floatValue = 2.35f;
            RequireProperty(serializedObject, "targetHeight").floatValue = 1.08f;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static BossBarragePatternProfile EnsurePhase2AkazaSpiralVolleyPatternProfile()
        {
            Material projectileMaterial =
                LoadOrCreateMaterial(Phase2AkazaProjectileMaterialPath, new Color(0.7f, 0.42f, 1f, 1f));
            BossBarragePatternProfile profile = LoadOrCreateBossBarragePatternProfile(
                Phase2AkazaSpiralVolleyPatternProfilePath);
            var serializedObject = new SerializedObject(profile);
            RequireProperty(serializedObject, "patternId").stringValue = "AkazaSpiralVolley";
            ApplySkillGrammar(
                serializedObject,
                LaneSkillPatternFamily.StaggeredCrossfire,
                LaneSkillTransferMode.SharedPvpSkillCandidate,
                "A crossed phase-two volley that keeps its windup and answer window as a skill verb.",
                "Bait the first cross, then dodge through the late inner gap instead of holding one lane.");
            ApplyTelegraphRead(
                serializedObject,
                new Color(0.58f, 0.32f, 1f, 0.72f),
                new Color(0.95f, 0.78f, 1f, 0.96f),
                1.18f,
                0.82f,
                1.22f);
            ApplyProjectileRead(
                serializedObject,
                new Color(0.75f, 0.48f, 1f, 1f),
                new Vector3(1.22f, 0.72f, 1.18f),
                projectileMaterial);
            RequireProperty(serializedObject, "targetingRule").enumValueIndex =
                (int)BossBarrageTargetingRule.LaneCenter;
            RequireProperty(serializedObject, "laneCenterLateralRatio").floatValue = 0f;
            RequireProperty(serializedObject, "lateralShape").enumValueIndex =
                (int)BossBarrageLateralShape.StaggeredCrossfire;
            RequireProperty(serializedObject, "initialDelaySeconds").floatValue = 0.2f;
            RequireProperty(serializedObject, "windupSeconds").floatValue = 1.0f;
            RequireProperty(serializedObject, "waveIntervalSeconds").floatValue = 5.2f;
            RequireProperty(serializedObject, "projectilesPerWave").intValue = 8;
            RequireProperty(serializedObject, "damage").floatValue = 9.5f;
            RequireProperty(serializedObject, "projectileSpeed").floatValue = 12.8f;
            RequireProperty(serializedObject, "projectileLifetimeSeconds").floatValue = 5.8f;
            RequireProperty(serializedObject, "projectileRadius").floatValue = 0.34f;
            RequireProperty(serializedObject, "backlineHalfSpread").floatValue = 4.55f;
            RequireProperty(serializedObject, "forwardHalfSpread").floatValue = 1.64f;
            RequireProperty(serializedObject, "crossfireInnerGapRatio").floatValue = 0.26f;
            RequireProperty(serializedObject, "backlineDepthSpread").floatValue = 3.1f;
            RequireProperty(serializedObject, "forwardDepthSpread").floatValue = 0.9f;
            RequireProperty(serializedObject, "spawnHeight").floatValue = 2.42f;
            RequireProperty(serializedObject, "targetHeight").floatValue = 1.08f;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static BossBarragePatternProfile EnsurePhase2AkazaSummonCurtainPatternProfile()
        {
            Material projectileMaterial =
                LoadOrCreateMaterial(Phase2AkazaProjectileMaterialPath, new Color(0.18f, 1f, 0.68f, 1f));
            BossBarragePatternProfile profile = LoadOrCreateBossBarragePatternProfile(
                Phase2AkazaSummonCurtainPatternProfilePath);
            var serializedObject = new SerializedObject(profile);
            RequireProperty(serializedObject, "patternId").stringValue = "AkazaSummonCurtain";
            ApplySkillGrammar(
                serializedObject,
                LaneSkillPatternFamily.EscortScreen,
                LaneSkillTransferMode.SharedPvpSkillCandidate,
                "A summon-backed curtain pattern that preserves the 3-slot frontline answer grammar.",
                "Spend a summon slot into the pressure screen or sidestep around the escorted center gap.");
            ApplyTelegraphRead(
                serializedObject,
                new Color(0.16f, 1f, 0.66f, 0.72f),
                new Color(0.75f, 1f, 0.9f, 0.96f),
                1.1f,
                1.08f,
                1.18f);
            ApplyProjectileRead(
                serializedObject,
                new Color(0.24f, 1f, 0.72f, 1f),
                new Vector3(1.04f, 0.64f, 1.45f),
                projectileMaterial);
            RequireProperty(serializedObject, "targetingRule").enumValueIndex =
                (int)BossBarrageTargetingRule.LaneCenter;
            RequireProperty(serializedObject, "laneCenterLateralRatio").floatValue = 0f;
            RequireProperty(serializedObject, "lateralShape").enumValueIndex =
                (int)BossBarrageLateralShape.EscortScreen;
            RequireProperty(serializedObject, "initialDelaySeconds").floatValue = 0.18f;
            RequireProperty(serializedObject, "windupSeconds").floatValue = 0.96f;
            RequireProperty(serializedObject, "waveIntervalSeconds").floatValue = 5.0f;
            RequireProperty(serializedObject, "projectilesPerWave").intValue = 7;
            RequireProperty(serializedObject, "damage").floatValue = 8.5f;
            RequireProperty(serializedObject, "projectileSpeed").floatValue = 12.2f;
            RequireProperty(serializedObject, "projectileLifetimeSeconds").floatValue = 5.5f;
            RequireProperty(serializedObject, "projectileRadius").floatValue = 0.3f;
            RequireProperty(serializedObject, "backlineHalfSpread").floatValue = 4.45f;
            RequireProperty(serializedObject, "forwardHalfSpread").floatValue = 1.52f;
            RequireProperty(serializedObject, "escortScreenInnerGapRatio").floatValue = 0.32f;
            RequireProperty(serializedObject, "backlineDepthSpread").floatValue = 2.8f;
            RequireProperty(serializedObject, "forwardDepthSpread").floatValue = 0.84f;
            RequireProperty(serializedObject, "spawnHeight").floatValue = 2.32f;
            RequireProperty(serializedObject, "targetHeight").floatValue = 1.08f;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static BossBarragePatternProfile EnsurePhase2AkazaCrushNetPatternProfile()
        {
            Material projectileMaterial =
                LoadOrCreateMaterial(Phase2AkazaProjectileMaterialPath, new Color(1f, 0.22f, 0.42f, 1f));
            BossBarragePatternProfile profile = LoadOrCreateBossBarragePatternProfile(
                Phase2AkazaCrushNetPatternProfilePath);
            var serializedObject = new SerializedObject(profile);
            RequireProperty(serializedObject, "patternId").stringValue = "AkazaCrushNet";
            ApplySkillGrammar(
                serializedObject,
                LaneSkillPatternFamily.PunishNet,
                LaneSkillTransferMode.CostedPlayerSkillCandidate,
                "A high-tier overextend punish that remains a committed skill, never boss basic fire.",
                "Retreat from forward risk or answer with a high-tier summon before the net collapses.");
            ApplyTelegraphRead(
                serializedObject,
                new Color(1f, 0.18f, 0.38f, 0.78f),
                new Color(1f, 0.68f, 0.72f, 0.96f),
                1.32f,
                0.92f,
                1.36f);
            ApplyProjectileRead(
                serializedObject,
                new Color(1f, 0.28f, 0.46f, 1f),
                new Vector3(1.28f, 0.78f, 1.28f),
                projectileMaterial);
            RequireProperty(serializedObject, "targetingRule").enumValueIndex =
                (int)BossBarrageTargetingRule.TrackedPlayer;
            RequireProperty(serializedObject, "lateralShape").enumValueIndex =
                (int)BossBarrageLateralShape.PunishNet;
            RequireProperty(serializedObject, "initialDelaySeconds").floatValue = 0.12f;
            RequireProperty(serializedObject, "windupSeconds").floatValue = 1.16f;
            RequireProperty(serializedObject, "waveIntervalSeconds").floatValue = 6.0f;
            RequireProperty(serializedObject, "projectilesPerWave").intValue = 7;
            RequireProperty(serializedObject, "damage").floatValue = 12f;
            RequireProperty(serializedObject, "projectileSpeed").floatValue = 13.4f;
            RequireProperty(serializedObject, "projectileLifetimeSeconds").floatValue = 5.8f;
            RequireProperty(serializedObject, "projectileRadius").floatValue = 0.36f;
            RequireProperty(serializedObject, "backlineHalfSpread").floatValue = 3.85f;
            RequireProperty(serializedObject, "forwardHalfSpread").floatValue = 0.88f;
            RequireProperty(serializedObject, "punishNetInnerSpreadRatio").floatValue = 0.3f;
            RequireProperty(serializedObject, "spawnHeight").floatValue = 2.48f;
            RequireProperty(serializedObject, "targetHeight").floatValue = 1.08f;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static BossBasicFireProfile EnsurePhase2AkazaBasicFireProfile()
        {
            EnsureFolderForAsset(Phase2AkazaBasicFireProfilePath);
            BossBasicFireProfile profile =
                AssetDatabase.LoadAssetAtPath<BossBasicFireProfile>(Phase2AkazaBasicFireProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<BossBasicFireProfile>();
                AssetDatabase.CreateAsset(profile, Phase2AkazaBasicFireProfilePath);
            }

            Material material = LoadOrCreateMaterial(
                Phase2AkazaBasicProjectileMaterialPath,
                new Color(0.42f, 0.95f, 1f, 1f));
            var serializedObject = new SerializedObject(profile);
            RequireProperty(serializedObject, "fireId").stringValue = "AkazaPhase2LanePoke";
            RequireProperty(serializedObject, "readoutLabel").stringValue = "Akaza Lane Poke";
            RequireProperty(serializedObject, "initialDelaySeconds").floatValue = 0.85f;
            RequireProperty(serializedObject, "fireIntervalSeconds").floatValue = 1.55f;
            RequireProperty(serializedObject, "projectilesPerVolley").intValue = 2;
            RequireProperty(serializedObject, "damage").floatValue = 4.5f;
            RequireProperty(serializedObject, "projectileSpeed").floatValue = 13.2f;
            RequireProperty(serializedObject, "projectileLifetimeSeconds").floatValue = 5.2f;
            RequireProperty(serializedObject, "projectileRadius").floatValue = 0.23f;
            RequireProperty(serializedObject, "damageResponsePolicy").enumValueIndex =
                (int)DamageResponsePolicy.FlashOnly;
            RequireProperty(serializedObject, "controlLockPolicy").enumValueIndex =
                (int)CombatControlLockPolicy.None;
            RequireProperty(serializedObject, "backlineHalfSpread").floatValue = 1.6f;
            RequireProperty(serializedObject, "forwardHalfSpread").floatValue = 0.42f;
            RequireProperty(serializedObject, "spawnLateralFollowRatio").floatValue = 0.24f;
            RequireProperty(serializedObject, "spawnHeight").floatValue = 2.2f;
            RequireProperty(serializedObject, "targetHeight").floatValue = 1.06f;
            RequireProperty(serializedObject, "projectileColor").colorValue = new Color(0.42f, 0.95f, 1f, 1f);
            RequireProperty(serializedObject, "projectileVisualScale").vector3Value =
                new Vector3(0.68f, 0.68f, 0.68f);
            RequireProperty(serializedObject, "projectileMaterial").objectReferenceValue = material;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static BossSummonPressureProfile EnsurePhase2AkazaSummonPressureProfile()
        {
            EnsureFolderForAsset(Phase2AkazaSummonPressureProfilePath);
            BossSummonPressureProfile profile =
                AssetDatabase.LoadAssetAtPath<BossSummonPressureProfile>(Phase2AkazaSummonPressureProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<BossSummonPressureProfile>();
                AssetDatabase.CreateAsset(profile, Phase2AkazaSummonPressureProfilePath);
            }

            var serializedObject = new SerializedObject(profile);
            RequireProperty(serializedObject, "pressureId").stringValue = "AkazaPhase2SummonPressure";
            SerializedProperty tierSettings = RequireProperty(serializedObject, "tierSettings");
            tierSettings.arraySize = 3;
            ConfigureAkazaSummonTier(
                tierSettings.GetArrayElementAtIndex(0),
                0.34f,
                1.05f,
                0.28f,
                0f,
                2.16f,
                "AkazaPhase2.EdgeCaller",
                660f,
                3.5f,
                2.8f,
                1.36f,
                1.3f,
                48f,
                0.74f,
                4,
                1.34f,
                3.6f);
            ConfigureAkazaSummonTier(
                tierSettings.GetArrayElementAtIndex(1),
                0.43f,
                1.58f,
                0.32f,
                0f,
                2.6f,
                "AkazaPhase2.ScreenGuard",
                920f,
                3.85f,
                4.2f,
                1.82f,
                1.45f,
                68f,
                0.82f,
                6,
                1.68f,
                4.25f);
            ConfigureAkazaSummonTier(
                tierSettings.GetArrayElementAtIndex(2),
                0.56f,
                2.12f,
                0.36f,
                0f,
                3.08f,
                "AkazaPhase2.CrushGuard",
                1280f,
                4.2f,
                5.4f,
                2.28f,
                1.58f,
                92f,
                0.9f,
                9,
                2.05f,
                5.1f);

            SerializedProperty tierReadouts = RequireProperty(serializedObject, "tierReadouts");
            tierReadouts.arraySize = 3;
            ConfigureAkazaSummonReadout(
                tierReadouts.GetArrayElementAtIndex(0),
                "LV1 Edge Caller",
                "A light boss summon steps over the frontline and asks for slot-1 interruption.",
                "Keep moving; do not trade player basic fire into the boss as your main answer.",
                "Slot 1 can block the screen long enough to keep the front from collapsing.");
            ConfigureAkazaSummonReadout(
                tierReadouts.GetArrayElementAtIndex(1),
                "LV2 Screen Guard",
                "The boss summon arrives with a larger projectile screen and forces a lane choice.",
                "Dodge through the readable side gap or retreat out of forward-risk charging.",
                "Slot 2 timing should answer the screen while the boss prepares a projectile pattern.");
            ConfigureAkazaSummonReadout(
                tierReadouts.GetArrayElementAtIndex(2),
                "LV3 Crush Guard",
                "A heavy summon protects the boss punish window and threatens the forward boundary.",
                "Back off from overextend reads and spend stored EN instead of face-trading.",
                "Slot 3 should be a committed frontline answer, not a hidden damage tick.");

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static BossPressureActionDeckProfile EnsurePhase2AkazaPressureActionDeck(
            BossBarragePatternProfile hoverLance,
            BossBarragePatternProfile spiralVolley,
            BossBarragePatternProfile summonCurtain,
            BossBarragePatternProfile crushNet)
        {
            EnsureFolderForAsset(Phase2AkazaDeckProfilePath);
            BossPressureActionDeckProfile profile =
                AssetDatabase.LoadAssetAtPath<BossPressureActionDeckProfile>(Phase2AkazaDeckProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<BossPressureActionDeckProfile>();
                AssetDatabase.CreateAsset(profile, Phase2AkazaDeckProfilePath);
            }

            var serializedObject = new SerializedObject(profile);
            RequireProperty(serializedObject, "deckId").stringValue = "AkazaPhase2Boss";
            RequireProperty(serializedObject, "globalRecoverySeconds").floatValue = 0.78f;
            SerializedProperty slots = RequireProperty(serializedObject, "actionSlots");
            slots.arraySize = 5;
            ConfigurePhase2AkazaActionSlot(
                slots.GetArrayElementAtIndex(0),
                hoverLance,
                BossPressureActionKind.SkillPattern,
                1,
                1,
                2.8f,
                false,
                0f,
                1f,
                false,
                1,
                "AkazaDodgeRailOrSpendSlot1",
                "The boss floats forward and marks a narrow rail after the player farms EN.",
                "Sidestep off the marked rail before release.",
                "A low-tier summon screen can buy space but does not replace dodging.");
            ConfigurePhase2AkazaActionSlot(
                slots.GetArrayElementAtIndex(1),
                summonCurtain,
                BossPressureActionKind.SummonPressure,
                1,
                1,
                3.4f,
                false,
                0f,
                1f,
                false,
                1,
                "AkazaSummonCurtainSlotRead",
                "The boss calls a frontline actor and fires a curtain that preserves a readable gap.",
                "Do not cross the forward boundary; reposition behind it.",
                "Slot 1 or 2 should meet the screen before the boss pattern lands.");
            ConfigurePhase2AkazaActionSlot(
                slots.GetArrayElementAtIndex(2),
                spiralVolley,
                BossPressureActionKind.SkillPattern,
                2,
                1,
                4.2f,
                false,
                0f,
                1f,
                true,
                2,
                "AkazaCrossfireAfterPlayerSummon",
                "The boss answers a visible player summon with a crossed projectile burst.",
                "Bait the first crossing pair and dodge through the late inner gap.",
                "Slot 2 should absorb the summon screen while the player moves.");
            ConfigurePhase2AkazaActionSlot(
                slots.GetArrayElementAtIndex(3),
                summonCurtain,
                BossPressureActionKind.SummonPressure,
                2,
                1,
                4.6f,
                false,
                0f,
                1f,
                true,
                2,
                "AkazaDoubleFrontlineAnswer",
                "A stronger summon-pressure beat tests whether the player kept EN for the frontline.",
                "Retreat into the safer backline if EN is not ready.",
                "Slot 2 or 3 should contest the boss actor instead of pure player DPS.");
            ConfigurePhase2AkazaActionSlot(
                slots.GetArrayElementAtIndex(4),
                crushNet,
                BossPressureActionKind.PunishOverextend,
                3,
                1,
                6.0f,
                true,
                0.62f,
                1f,
                false,
                1,
                "AkazaOverextendCrushNet",
                "The boss spends a high tier only when the player stays forward too long.",
                "Back out of forward risk before the net closes.",
                "A tier-3 summon can hold the front while the player retreats.");
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static BossBarrageProjectile EnsurePhase2AkazaProjectilePrefab()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(ProjectilePrefabPath) == null)
            {
                EnsureProjectilePrefab();
            }

            EnsureFolderForAsset(Phase2AkazaProjectilePrefabPath);
            if (AssetDatabase.LoadAssetAtPath<GameObject>(Phase2AkazaProjectilePrefabPath) == null)
            {
                if (!AssetDatabase.CopyAsset(ProjectilePrefabPath, Phase2AkazaProjectilePrefabPath))
                {
                    throw new InvalidOperationException(
                        $"Failed to copy {ProjectilePrefabPath} to {Phase2AkazaProjectilePrefabPath}.");
                }
            }

            Material projectileMaterial = LoadOrCreateMaterial(
                Phase2AkazaProjectileMaterialPath,
                new Color(0.64f, 0.48f, 1f, 1f));
            GameObject editableRoot = PrefabUtility.LoadPrefabContents(Phase2AkazaProjectilePrefabPath);
            try
            {
                editableRoot.name = "PF_BossBarrageProjectile_AkazaPhase2";
                Renderer[] visualRenderers = ResolveProjectileVisualRenderers(editableRoot);
                for (int i = 0; i < visualRenderers.Length; i++)
                {
                    if (visualRenderers[i] != null)
                    {
                        visualRenderers[i].sharedMaterial = projectileMaterial;
                    }
                }

                TrailRenderer[] trails = editableRoot.GetComponentsInChildren<TrailRenderer>(includeInactive: true);
                for (int i = 0; i < trails.Length; i++)
                {
                    TrailRenderer trail = trails[i];
                    if (trail == null)
                    {
                        continue;
                    }

                    trail.sharedMaterial = projectileMaterial;
                    trail.time = 0.24f;
                    trail.startWidth = 0.26f;
                    trail.endWidth = 0.035f;
                    EditorUtility.SetDirty(trail);
                }

                BossBarrageProjectile projectile = EnsureComponent<BossBarrageProjectile>(editableRoot);
                SetObjectReferenceArray(projectile, "visualRenderers", ToObjectArray(visualRenderers));
                SetObjectReferenceArray(projectile, "trailRenderers", ToObjectArray(trails));
                PrefabUtility.SaveAsPrefabAsset(editableRoot, Phase2AkazaProjectilePrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(editableRoot);
            }

            ValidateNoImportedAssetReference(Phase2AkazaProjectilePrefabPath);
            return LoadPrefabComponent<BossBarrageProjectile>(Phase2AkazaProjectilePrefabPath);
        }

        private static GameObject EnsurePhase2AkazaPrefab()
        {
            AnimatorController controller = EnsurePhase2AkazaAnimatorController();
            GameObject modelSource = LoadAsset<GameObject>(Phase2AkazaModelPath);
            bool prefabExists = AssetDatabase.LoadAssetAtPath<GameObject>(Phase2AkazaPrefabPath) != null;
            EnsureFolderForAsset(Phase2AkazaPrefabPath);
            GameObject editableRoot = prefabExists
                ? PrefabUtility.LoadPrefabContents(Phase2AkazaPrefabPath)
                : UnityEngine.Object.Instantiate(modelSource);

            try
            {
                editableRoot.name = "PF_Boss_Akaza_Phase2Review";
                editableRoot.transform.localPosition = Vector3.zero;
                editableRoot.transform.localRotation = Quaternion.identity;
                editableRoot.transform.localScale = Vector3.one;

                Animator animator = editableRoot.GetComponent<Animator>();
                if (animator == null)
                {
                    animator = editableRoot.AddComponent<Animator>();
                }

                animator.runtimeAnimatorController = controller;
                animator.avatar = LoadPhase2AkazaAvatar();
                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

                ActionFoundationArenaTransformMotion hoverMotion =
                    EnsureComponent<ActionFoundationArenaTransformMotion>(editableRoot);
                hoverMotion.Configure(
                    Vector3.zero,
                    Vector3.up,
                    0.28f,
                    0.42f,
                    0.15f,
                    lockLocalRotation: true,
                    lockLocalScale: true);

                ApplyPhase2AkazaMaterials(editableRoot);
                EnsurePhase2AkazaAuraCore(editableRoot.transform);
                EnsurePhase2AkazaCombatCueClock(editableRoot.transform);
                PrefabUtility.SaveAsPrefabAsset(editableRoot, Phase2AkazaPrefabPath);
            }
            finally
            {
                if (prefabExists)
                {
                    PrefabUtility.UnloadPrefabContents(editableRoot);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(editableRoot);
                }
            }

            ValidateNoImportedAssetReference(Phase2AkazaPrefabPath);
            return LoadAsset<GameObject>(Phase2AkazaPrefabPath);
        }

        private static AnimatorController EnsurePhase2AkazaAnimatorController()
        {
            EnsureFolderForAsset(Phase2AkazaAnimatorControllerPath);
            AnimatorController controller =
                AssetDatabase.LoadAssetAtPath<AnimatorController>(Phase2AkazaAnimatorControllerPath);
            if (controller == null)
            {
                controller = AnimatorController.CreateAnimatorControllerAtPath(Phase2AkazaAnimatorControllerPath);
            }

            ResetAnimatorController(controller);
            string[] triggers =
            {
                "EliteAuraBuffer",
                "AttackRetreatShot",
                "EliteSummonPackage",
                "AttackFanPressure",
                "AttackHeavy",
                "ElitePhaseSwap",
                "AttackLinePressure",
                "Attack",
                "Hit",
                "Death"
            };
            for (int i = 0; i < triggers.Length; i++)
            {
                controller.AddParameter(triggers[i], AnimatorControllerParameterType.Trigger);
            }

            controller.AddParameter("MoveSpeed", AnimatorControllerParameterType.Float);

            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
            EnsurePhase2AkazaSourceReferenceClips();
            AnimationClip combatCueClip = EnsurePhase2AkazaCombatCueClip();

            AnimatorState hover = AddAkazaAnimatorState(stateMachine, "Hover", null, 1f);
            stateMachine.defaultState = hover;
            AddAkazaTimedState(stateMachine, hover, "IntroThreatRise", combatCueClip, 0.76f, 0.9f);
            AddAkazaTimedState(
                stateMachine,
                hover,
                "IntroReveal1412_1562",
                combatCueClip,
                0.72f,
                0.95f);
            AddAkazaTimedState(stateMachine, hover, "IntroPressureHandoff", combatCueClip, 0.88f, 0.86f);
            AddAkazaTriggeredState(stateMachine, hover, "Windup", combatCueClip, "EliteAuraBuffer", 0.72f);
            AddAkazaTriggeredState(stateMachine, hover, "LinePressure", combatCueClip, "AttackLinePressure", 0.95f);
            AddAkazaTriggeredState(stateMachine, hover, "RetreatShot", combatCueClip, "AttackRetreatShot", 0.95f);
            AddAkazaTriggeredState(stateMachine, hover, "SummonPackage", combatCueClip, "EliteSummonPackage", 0.82f);
            AddAkazaTriggeredState(stateMachine, hover, "FanPressure", combatCueClip, "AttackFanPressure", 0.92f);
            AddAkazaTriggeredState(stateMachine, hover, "HeavyCrush", combatCueClip, "AttackHeavy", 0.86f);
            AddAkazaTriggeredState(stateMachine, hover, "PhaseSwap", combatCueClip, "ElitePhaseSwap", 0.72f);
            AddAkazaTriggeredState(stateMachine, hover, "BasicAttack", combatCueClip, "Attack", 0.98f);
            AddAkazaTriggeredState(stateMachine, hover, "Hit", combatCueClip, "Hit", 1.05f);
            AddAkazaTriggeredState(stateMachine, hover, "Death", combatCueClip, "Death", 0.72f);
            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static void EnsurePhase2AkazaSceneAssetExists()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(Phase2AkazaReviewScenePath) != null)
            {
                return;
            }

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ReviewScenePath) == null)
            {
                EnsureBossBarrageLaneReviewScene();
            }

            EnsureFolderForAsset(Phase2AkazaReviewScenePath);
            if (!AssetDatabase.CopyAsset(ReviewScenePath, Phase2AkazaReviewScenePath))
            {
                throw new InvalidOperationException(
                    $"Failed to copy {ReviewScenePath} to {Phase2AkazaReviewScenePath}.");
            }

            AssetDatabase.ImportAsset(Phase2AkazaReviewScenePath, ImportAssetOptions.ForceUpdate);
        }

        private static void ApplyPhase2AkazaBossProxy(
            Scene scene,
            GameObject bossProxy,
            BossBarragePatternProfile hoverLance,
            BossBarragePatternProfile spiralVolley,
            BossBarragePatternProfile summonCurtain,
            BossBarragePatternProfile crushNet,
            BossBasicFireProfile basicFireProfile,
            BossSummonPressureProfile summonPressureProfile,
            BossPressureActionDeckProfile actionDeckProfile,
            GameObject akazaPrefab,
            CinematicSequenceProfile introProfile)
        {
            RemovePhase2InheritedSupportDragon(scene);
            RefreshPhase2InheritedInoriReferences(scene);
            SummonLaneSpace laneSpace = RequireComponent<SummonLaneSpace>(
                RequireRoot(scene, LaneRootName),
                "phase2 lane space");
            PlayerMovementController player = RequireObject<PlayerMovementController>(scene, "phase2 player movement");
            CombatHealth bossHealth = RequireComponent<CombatHealth>(bossProxy, "phase2 boss health");
            SetFloat(bossHealth, "maxHealth", 12500f);

            BossBarrageProjectile projectilePrefab =
                LoadPrefabComponent<BossBarrageProjectile>(Phase2AkazaProjectilePrefabPath);
            GameObject projectilePrefabObject = LoadAsset<GameObject>(Phase2AkazaProjectilePrefabPath);

            BossBarrageEmitter emitter = RequireComponent<BossBarrageEmitter>(bossProxy, "phase2 boss emitter");
            SetObjectReference(emitter, "patternProfile", hoverLance);
            SetObjectReferenceArray(
                emitter,
                "patternSequence",
                new UnityEngine.Object[]
                {
                    hoverLance,
                    summonCurtain,
                    spiralVolley,
                    hoverLance,
                    summonCurtain,
                    crushNet
                });
            SetInt(emitter, "wavesPerPattern", 1);
            SetObjectReference(emitter, "projectilePrefab", projectilePrefab);
            SetObjectReference(emitter, "projectilePrefabObject", projectilePrefabObject);
            SetInt(emitter, "prewarmCount", 36);
            SetBool(emitter, "firingEnabled", true);

            BossBasicFireEmitter basicFire =
                RequireComponent<BossBasicFireEmitter>(bossProxy, "phase2 boss basic fire");
            SetObjectReference(basicFire, "fireProfile", basicFireProfile);
            SetObjectReference(basicFire, "projectilePrefab", projectilePrefab);
            SetObjectReference(basicFire, "projectilePrefabObject", projectilePrefabObject);
            SetInt(basicFire, "prewarmCount", 12);
            SetBool(basicFire, "firingEnabled", true);

            BossSummonPressureAction summonPressure =
                RequireComponent<BossSummonPressureAction>(bossProxy, "phase2 boss summon pressure");
            summonPressure.ConfigurePressureProfile(summonPressureProfile);
            SetObjectReference(summonPressure, "pressureProfile", summonPressureProfile);
            SetInt(summonPressure, "actorPrewarmCount", 3);
            SetInt(summonPressure, "maxActiveSummonActors", 2);
            SetFloat(summonPressure, "actorEntryCatchupSecondsPerMeter", 0.48f);
            SetFloat(summonPressure, "minimumPlayerSideTargetDepth", 1.35f);

            BossPressureActionDirector actionDirector =
                RequireComponent<BossPressureActionDirector>(bossProxy, "phase2 boss pressure director");
            actionDirector.ConfigureActionDeck(actionDeckProfile);
            actionDirector.SetHoldForNextTierActionWhenGateAllows(true);
            SetObjectReference(actionDirector, "actionDeckProfile", actionDeckProfile);
            SetBool(actionDirector, "actionsEnabled", true);
            SetFloat(actionDirector, "playerSummonResponseWindowSeconds", 4.4f);

            BossPressurePositionController positionController =
                RequireComponent<BossPressurePositionController>(bossProxy, "phase2 boss pressure position");
            positionController.ConfigureReferences(
                laneSpace,
                RequireComponent<BossPressureCostLadder>(bossProxy, "phase2 boss cost ladder"),
                actionDirector,
                bossProxy.transform);
            SetFloat(positionController, "restRisk01", 0.2f);
            SetFloat(positionController, "maxCommitRisk01", 0.82f);
            SetFloat(positionController, "advanceRiskPerSecond", 0.52f);
            SetFloat(positionController, "retreatRiskPerSecond", 0.44f);
            SetBool(positionController, "movementEnabled", true);

            GameObject visual = ApplyPhase2AkazaVisual(bossProxy, akazaPrefab);
            ConfigurePhase2AkazaProjectileCore(bossProxy);
            ConfigurePhase2AkazaVisualCueDriver(bossProxy, visual, emitter, actionDirector, player.transform);
            ConfigurePhase2AkazaBossIntro(scene, bossProxy, visual, introProfile, player.transform);
            EditorUtility.SetDirty(bossProxy);
        }

        private static void RefreshPhase2InheritedInoriReferences(Scene scene)
        {
            ActionFoundationInoriPlayerVisualAssetSetup.EnsureInoriPlayerVisualAssets();

            GameObject rangedVisual = FindPhase2SceneObject(scene, RangedPlayerVisualRootName);
            if (rangedVisual != null)
            {
                RemapInoriPlayerMeshes(rangedVisual);
                AssignInoriPlayerMaterials(rangedVisual);
                EditorUtility.SetDirty(rangedVisual);
            }

            GameObject rangedModel = FindPhase2SceneObject(scene, RangedPlayerModelName);
            if (rangedModel != null && rangedModel != rangedVisual)
            {
                RemapInoriPlayerMeshes(rangedModel);
                AssignInoriPlayerMaterials(rangedModel);
                EditorUtility.SetDirty(rangedModel);
            }
        }

        private static GameObject FindPhase2SceneObject(Scene scene, string objectName)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
            {
                Transform[] transforms = roots[rootIndex].GetComponentsInChildren<Transform>(includeInactive: true);
                for (int transformIndex = 0; transformIndex < transforms.Length; transformIndex++)
                {
                    if (string.Equals(transforms[transformIndex].name, objectName, StringComparison.Ordinal))
                    {
                        return transforms[transformIndex].gameObject;
                    }
                }
            }

            return null;
        }

        private static void RemovePhase2InheritedSupportDragon(Scene scene)
        {
            GameObject supportDragon = FindRoot(scene, CinematicSupportDragonRootName);
            if (supportDragon != null)
            {
                UnityEngine.Object.DestroyImmediate(supportDragon);
            }
        }

        private static GameObject ApplyPhase2AkazaVisual(GameObject bossProxy, GameObject akazaPrefab)
        {
            RemoveBossProxyHumanoidVisualChildren(bossProxy.transform);
            GameObject visual = PrefabUtility.InstantiatePrefab(akazaPrefab, bossProxy.transform) as GameObject;
            if (visual == null)
            {
                throw new InvalidOperationException($"Could not instantiate {Phase2AkazaPrefabPath}.");
            }

            visual.name = Phase2AkazaVisualName;
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = Vector3.one;
            FitAkazaVisualToBossProxy(visual, 3.32f, 0.52f);

            ActionFoundationArenaTransformMotion hoverMotion =
                EnsureComponent<ActionFoundationArenaTransformMotion>(visual);
            hoverMotion.Configure(
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

        private static void ConfigurePhase2AkazaProjectileCore(GameObject bossProxy)
        {
            Transform projectileCore = bossProxy.transform.Find(BossProxyMarkerName);
            if (projectileCore == null)
            {
                CreateBossProjectileCore(bossProxy.transform);
                projectileCore = RequireChild(bossProxy.transform, BossProxyMarkerName);
            }

            projectileCore.localPosition = new Vector3(0f, 1.1f, 0.2f);
            projectileCore.localScale = new Vector3(0.045f, 0.045f, 0.045f);
            MeshRenderer renderer = projectileCore.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.enabled = false;
                renderer.sharedMaterial = LoadOrCreateMaterial(
                    Phase2AkazaCoreMaterialPath,
                    new Color(0.18f, 0.68f, 0.85f, 0.32f));
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                EditorUtility.SetDirty(renderer);
            }

            ActionFoundationArenaFloatingShape floatingShape =
                EnsureComponent<ActionFoundationArenaFloatingShape>(projectileCore.gameObject);
            floatingShape.Configure(
                new Vector3(0f, 32f, 0f),
                Vector3.up,
                0.035f,
                0.6f,
                0.24f,
                new Color(0.18f, 0.72f, 1f, 0.42f),
                new Color(0.24f, 0.95f, 1.22f, 0.72f),
                0.16f,
                0.72f);
            EditorUtility.SetDirty(floatingShape);
        }

        private static void ConfigurePhase2AkazaVisualCueDriver(
            GameObject bossProxy,
            GameObject visual,
            BossBarrageEmitter emitter,
            BossPressureActionDirector actionDirector,
            Transform directionTarget)
        {
            Animator animator = visual.GetComponentInChildren<Animator>(includeInactive: true);
            if (animator == null)
            {
                throw new InvalidOperationException($"{Phase2AkazaVisualName} is missing Animator.");
            }

            Transform projectileCore = RequireChild(bossProxy.transform, BossProxyMarkerName);
            BossBarrageVisualCueDriver cueDriver = EnsureComponent<BossBarrageVisualCueDriver>(bossProxy);
            cueDriver.ConfigurePresentation(
                emitter,
                animator,
                projectileCore,
                projectileCore.GetComponentsInChildren<Renderer>(includeInactive: true));
            cueDriver.ConfigurePressureActionSource(actionDirector);
            CombatVfxCuePlayer cuePlayer = directionTarget != null
                ? directionTarget.GetComponent<CombatVfxCuePlayer>()
                : null;
            cueDriver.ConfigureWorldVfx(cuePlayer, projectileCore, directionTarget);
            ConfigurePhase2AkazaCueDriverData(cueDriver);
            EditorUtility.SetDirty(cueDriver);
        }

        private static void ConfigurePhase2AkazaCueDriverData(BossBarrageVisualCueDriver cueDriver)
        {
            var serializedObject = new SerializedObject(cueDriver);
            RequireProperty(serializedObject, "baseColor").colorValue = new Color(0.28f, 0.86f, 1f, 1f);
            RequireProperty(serializedObject, "defaultWindupTrigger").stringValue = "EliteAuraBuffer";
            RequireProperty(serializedObject, "defaultReleaseTrigger").stringValue = "AttackLinePressure";
            RequireProperty(serializedObject, "releaseFlashSeconds").floatValue = 0.2f;
            RequireProperty(serializedObject, "pulseSpeed").floatValue = 18f;
            RequireProperty(serializedObject, "windupCueIntensity").floatValue = 1.08f;
            RequireProperty(serializedObject, "releaseCueIntensity").floatValue = 1.26f;
            RequireProperty(serializedObject, "pressureActionCueIntensity").floatValue = 1.15f;
            RequireProperty(serializedObject, "tierCueIntensityStep").floatValue = 0.12f;

            SerializedProperty patternCues = RequireProperty(serializedObject, "patternCues");
            patternCues.arraySize = 4;
            ConfigurePhase2AkazaPatternCue(
                patternCues.GetArrayElementAtIndex(0),
                "AkazaHoverLance",
                "EliteAuraBuffer",
                "AttackLinePressure",
                new Color(0.24f, 0.9f, 1f, 1f),
                new Color(0.74f, 1f, 1f, 1f),
                0.24f,
                0.44f,
                CombatVfxCueId.EnemyLinePressureWindup,
                CombatVfxCueId.EnemyLinePressureActive);
            ConfigurePhase2AkazaPatternCue(
                patternCues.GetArrayElementAtIndex(1),
                "AkazaSpiralVolley",
                "ElitePhaseSwap",
                "AttackFanPressure",
                new Color(0.62f, 0.36f, 1f, 1f),
                new Color(0.95f, 0.76f, 1f, 1f),
                0.28f,
                0.48f,
                CombatVfxCueId.ElitePhaseSwapSignal,
                CombatVfxCueId.EnemyFanPressureActive);
            ConfigurePhase2AkazaPatternCue(
                patternCues.GetArrayElementAtIndex(2),
                "AkazaSummonCurtain",
                "EliteSummonPackage",
                "AttackFanPressure",
                new Color(0.28f, 1f, 0.66f, 1f),
                new Color(0.76f, 1f, 0.86f, 1f),
                0.3f,
                0.46f,
                CombatVfxCueId.EliteSummonSignal,
                CombatVfxCueId.EnemyFanPressureActive);
            ConfigurePhase2AkazaPatternCue(
                patternCues.GetArrayElementAtIndex(3),
                "AkazaCrushNet",
                "ElitePhaseSwap",
                "AttackHeavy",
                new Color(1f, 0.2f, 0.38f, 1f),
                new Color(1f, 0.68f, 0.72f, 1f),
                0.34f,
                0.56f,
                CombatVfxCueId.EnemyGuardBreakWindup,
                CombatVfxCueId.EnemyGuardBreakActive);

            SerializedProperty pressureActionCues = RequireProperty(serializedObject, "pressureActionCues");
            pressureActionCues.arraySize = 3;
            ConfigurePhase2AkazaPressureCue(
                pressureActionCues.GetArrayElementAtIndex(0),
                BossPressureActionKind.SkillPattern,
                "AttackLinePressure",
                new Color(0.48f, 0.96f, 1f, 1f),
                0.28f,
                0.32f,
                0.08f);
            ConfigurePhase2AkazaPressureCue(
                pressureActionCues.GetArrayElementAtIndex(1),
                BossPressureActionKind.SummonPressure,
                "EliteSummonPackage",
                new Color(0.42f, 1f, 0.72f, 1f),
                0.36f,
                0.38f,
                0.09f);
            ConfigurePhase2AkazaPressureCue(
                pressureActionCues.GetArrayElementAtIndex(2),
                BossPressureActionKind.PunishOverextend,
                "AttackHeavy",
                new Color(1f, 0.24f, 0.32f, 1f),
                0.42f,
                0.5f,
                0.11f);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static CinematicSequenceProfile EnsurePhase2AkazaBossIntroProfile()
        {
            EnsureFolderForAsset(Phase2AkazaIntroProfilePath);
            CinematicSequenceProfile profile =
                AssetDatabase.LoadAssetAtPath<CinematicSequenceProfile>(Phase2AkazaIntroProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CinematicSequenceProfile>();
                AssetDatabase.CreateAsset(profile, Phase2AkazaIntroProfilePath);
            }

            float sourceStartSeconds =
                Phase2AkazaIntroSourceStartFrame / Phase2AkazaIntroSourceFrameRate;
            float sourceEndSeconds =
                Phase2AkazaIntroSourceEndFrame / Phase2AkazaIntroSourceFrameRate;
            string reviewerIntent =
                $"Boss entry intro sourced from ThePhantomKnowledge GeneralTimeline_nD 2.playable frames "
                + $"{Phase2AkazaIntroSourceStartFrame}-{Phase2AkazaIntroSourceEndFrame} at "
                + $"{Phase2AkazaIntroSourceFrameRate:0}fps ({sourceStartSeconds:0.00}-{sourceEndSeconds:0.00}s). "
                + "Uses promoted original source camera and actor bindings for the 1412-1562 frame window; append "
                + "later intro shots as additional CinematicSequencePlaylistRunner profiles after this source entry.";

            profile.Configure(
                "akaza_phase2_boss_intro_1412_1562",
                "Akaza Phase2 Boss Intro 1412-1562",
                CinematicSequenceProfile.SequenceCategory.BossIntro,
                reviewerIntent,
                3.12f,
                92,
                newLockMovement: true,
                newLockInput: true,
                newHideHud: true,
                newCanSkip: true,
                newUseUnscaledClock: true,
                Array.Empty<CinematicSequenceProfile.CameraCue>(),
                new[]
                {
                    Phase2AkazaBossBodyCue(
                        "akaza_intro_c08_reveal_body",
                        2.5f,
                        0.46f,
                        "IntroReveal1412_1562"),
                    Phase2AkazaBossBodyCue("akaza_intro_pressure_handoff_body", 2.92f, 1.24f, "IntroPressureHandoff")
                },
                new[]
                {
                    new CinematicSequenceProfile.VfxCue(
                        "akaza_intro_phase_swap_signal",
                        2.52f,
                        0.28f,
                        CombatVfxCueId.ElitePhaseSwapSignal,
                        new Vector3(0f, 1.1f, 0f),
                        1.08f),
                    new CinematicSequenceProfile.VfxCue(
                        "akaza_intro_aura_hold",
                        2.88f,
                        0.84f,
                        CombatVfxCueId.EliteAuraSignal,
                        new Vector3(0f, 1.12f, 0f),
                        1.12f),
                    new CinematicSequenceProfile.VfxCue(
                        "akaza_intro_lane_pressure_windup",
                        1.36f,
                        0.62f,
                        CombatVfxCueId.EnemyLinePressureWindup,
                        new Vector3(0f, 1.08f, 0.35f),
                        1.06f),
                    new CinematicSequenceProfile.VfxCue(
                        "akaza_intro_lane_pressure_active",
                        1.98f,
                        0.42f,
                        CombatVfxCueId.EnemyLinePressureActive,
                        new Vector3(0f, 1.08f, 0.48f),
                        1.14f)
                },
                Array.Empty<CinematicSequenceProfile.TutorialCue>(),
                new CinematicSequenceProfile.GameplayHandoffCue(
                    CinematicSequenceProfile.GameplayReturnMode.ActionCameraController,
                    3.02f,
                    ActionCinematicCueProfile.GameplayReturnTargetId,
                    inputReleaseDelaySeconds: 0.18f,
                    restoreHud: true,
                    restoreTimeScale: true,
                    restoreCamera: true));
            profile.ConfigureSourceCameraAnimations(BuildPhase2AkazaTimelineSourceCameraCues());
            profile.ConfigureSourceActorAnimations(BuildPhase2AkazaTimelineSourceActorCues());
            profile.ConfigureSourceActorGrades(BuildPhase2AkazaTimelineSourceActorGradeCues());
            profile.ConfigureScreenFades(BuildPhase2AkazaTimelineScreenFadeCues());
            profile.ConfigureStageContext(
                null,
                string.Empty,
                string.Empty,
                string.Empty,
                "Phase2 Akaza local review intro; append later shots through CinematicSequencePlaylistRunner.",
                newRequiresStageDefinition: false);
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static CinematicSequenceProfile.SourceCameraAnimationCue[] BuildPhase2AkazaTimelineSourceCameraCues()
        {
            TimelineSourceClip[] clips = BuildPhase2AkazaExpectedTimelineCameraClips();
            CinematicSequenceProfile.SourceCameraAnimationCue[] cues =
                new CinematicSequenceProfile.SourceCameraAnimationCue[clips.Length];
            for (int i = 0; i < clips.Length; i++)
            {
                cues[i] = CreatePhase2AkazaTimelineSourceCameraCue(
                    $"akaza_intro_timeline_camera_{Path.GetFileNameWithoutExtension(clips[i].AssetPath)}",
                    clips[i]);
            }

            return cues;
        }

        private static CinematicSequenceProfile.SourceActorAnimationCue[] BuildPhase2AkazaTimelineSourceActorCues()
        {
            TimelineSourceClip[] clips = BuildPhase2AkazaExpectedTimelineSourceActorClips();
            CinematicSequenceProfile.SourceActorAnimationCue[] cues =
                new CinematicSequenceProfile.SourceActorAnimationCue[clips.Length];
            for (int i = 0; i < clips.Length; i++)
            {
                cues[i] = CreatePhase2AkazaTimelineSourceActorCue(
                    $"akaza_intro_timeline_actor_{Path.GetFileNameWithoutExtension(clips[i].AssetPath)}",
                    clips[i]);
            }

            return cues;
        }

        private static CinematicSequenceProfile.SourceActorGradeCue[] BuildPhase2AkazaTimelineSourceActorGradeCues()
        {
            return Array.Empty<CinematicSequenceProfile.SourceActorGradeCue>();
        }

        private static CinematicSequenceProfile.ScreenFadeCue[] BuildPhase2AkazaTimelineScreenFadeCues()
        {
            return Array.Empty<CinematicSequenceProfile.ScreenFadeCue>();
        }

        private static CinematicSequenceProfile.SourceCameraAnimationCue CreatePhase2AkazaTimelineSourceCameraCue(
            string cueId,
            TimelineSourceClip sourceClip)
        {
            ResolvePhase2AkazaTimelineWindow(
                sourceClip,
                out float sequenceStartSeconds,
                out float clipInSeconds,
                out float durationSeconds);
            return new CinematicSequenceProfile.SourceCameraAnimationCue(
                cueId,
                LoadPrimaryAnimationClip(sourceClip.AssetPath),
                sequenceStartSeconds,
                clipInSeconds,
                durationSeconds);
        }

        private static CinematicSequenceProfile.SourceActorAnimationCue CreatePhase2AkazaTimelineSourceActorCue(
            string cueId,
            TimelineSourceClip sourceClip)
        {
            ResolvePhase2AkazaTimelineWindow(
                sourceClip,
                out float sequenceStartSeconds,
                out float clipInSeconds,
                out float durationSeconds);
            return new CinematicSequenceProfile.SourceActorAnimationCue(
                cueId,
                LoadPrimaryAnimationClip(sourceClip.AssetPath),
                sequenceStartSeconds,
                clipInSeconds,
                durationSeconds);
        }

        private static void ResolvePhase2AkazaTimelineWindow(
            TimelineSourceClip sourceClip,
            out float sequenceStartSeconds,
            out float clipInSeconds,
            out float durationSeconds)
        {
            double windowStart = Phase2AkazaIntroSourceStartFrame / (double)Phase2AkazaIntroSourceFrameRate;
            double windowEnd = Phase2AkazaIntroSourceEndFrame / (double)Phase2AkazaIntroSourceFrameRate;
            double activeStart = Math.Max(sourceClip.TimelineStartSeconds, windowStart);
            double activeEnd = Math.Min(sourceClip.TimelineEndSeconds, windowEnd);
            sequenceStartSeconds = (float)(activeStart - windowStart);
            clipInSeconds = (float)(activeStart - sourceClip.TimelineStartSeconds);
            durationSeconds = Mathf.Max(0.01f, (float)(activeEnd - activeStart));
        }

        private static CinematicSequenceProfile.CameraCue Phase2AkazaIntroCameraCue(
            string cueId,
            CinematicSequenceProfile.ShotPurpose purpose,
            CinematicSequenceProfile.CameraBlendKind blendKind,
            float startSeconds,
            float durationSeconds,
            Vector3 cameraLocalPosition,
            Vector3 lookAtLocalPosition,
            float fieldOfView)
        {
            return new CinematicSequenceProfile.CameraCue(
                cueId,
                purpose,
                blendKind,
                startSeconds,
                durationSeconds,
                Vector3.zero,
                planarDirectionOffset: 0f,
                fieldOfViewDelta: 0f,
                cameraDistanceDelta: 0f,
                focusHeightDelta: 0f,
                cameraLocalPosition,
                lookAtLocalPosition,
                fieldOfView,
                impulseScale: 1f);
        }

        private static CinematicSequenceProfile.ActorCue Phase2AkazaBossBodyCue(
            string cueId,
            float startSeconds,
            float durationSeconds,
            string stateName)
        {
            return new CinematicSequenceProfile.ActorCue(
                cueId,
                CinematicSequenceProfile.ActorRole.Boss,
                CinematicSequenceProfile.ActorCueKind.BodyState,
                startSeconds,
                durationSeconds,
                stateName);
        }

        private static void ConfigurePhase2AkazaBossIntro(
            Scene scene,
            GameObject bossProxy,
            GameObject visual,
            CinematicSequenceProfile introProfile,
            Transform playerTransform)
        {
            Animator animator = visual.GetComponentInChildren<Animator>(includeInactive: true);
            if (animator == null)
            {
                throw new InvalidOperationException($"{Phase2AkazaVisualName} is missing Animator.");
            }

            ActionCameraController cameraController =
                RequireObject<ActionCameraController>(scene, "phase2 Akaza intro camera controller");
            Camera camera = cameraController.GetComponent<Camera>();
            if (camera == null)
            {
                throw new InvalidOperationException($"{cameraController.name} must keep a Camera component.");
            }

            ConfigurePhase2AkazaC08SourceCameraPostProcessing(camera);
            ConfigurePhase2AkazaC08SourceRenderSettings();
            EnsurePhase2AkazaC08CombatLookSceneContext(scene);

            GameObject sourceCameraRig = EnsurePhase2AkazaOriginalC23CameraRig(scene, visual.transform);
            Transform sourceTimelineWrapper = sourceCameraRig.transform.parent;
            GameObject sourceActorRig = EnsurePhase2AkazaOriginalTimelineActorRigs(sourceTimelineWrapper);
            Camera sourceCamera = sourceCameraRig.GetComponentInChildren<Camera>(includeInactive: true);
            if (sourceCamera == null)
            {
                throw new InvalidOperationException(
                    $"{Phase2AkazaC08CameraSourcePath} scene rig must expose a Camera.");
            }

            CinematicSequenceRunner runner = EnsureComponent<CinematicSequenceRunner>(cameraController.gameObject);
            SerializedObject serializedRunner = new SerializedObject(runner);
            RequireProperty(serializedRunner, "sequenceProfile").objectReferenceValue = introProfile;
            RequireProperty(serializedRunner, "bodyControllerOverride").objectReferenceValue = null;
            RequireProperty(serializedRunner, "cameraController").objectReferenceValue = cameraController;
            RequireProperty(serializedRunner, "combatVfxCuePlayer").objectReferenceValue =
                playerTransform != null ? playerTransform.GetComponent<CombatVfxCuePlayer>() : null;
            RequireProperty(serializedRunner, "tutorialPromptPresenter").objectReferenceValue = null;
            RequireProperty(serializedRunner, "cueSpace").objectReferenceValue = visual.transform;
            RequireProperty(serializedRunner, "cinematicCamera").objectReferenceValue = camera;
            RequireProperty(serializedRunner, "driveCameraTransformFromProfile").boolValue = true;
            RequireProperty(serializedRunner, "disableActionCameraControllerDuringPoseDrive").boolValue = true;
            RequireProperty(serializedRunner, "maxPlaybackDeltaSeconds").floatValue =
                1f / Phase2AkazaIntroSourceFrameRate;
            RequireProperty(serializedRunner, "sourceCameraRigRoot").objectReferenceValue = sourceCameraRig;
            RequireProperty(serializedRunner, "sourceCameraTransform").objectReferenceValue = sourceCamera.transform;
            RequireProperty(serializedRunner, "sourceCameraComponent").objectReferenceValue = sourceCamera;
            ConfigurePhase2AkazaSourceCameraBindings(serializedRunner, sourceTimelineWrapper, introProfile);
            RequireProperty(serializedRunner, "sourceActorRigRoot").objectReferenceValue = sourceActorRig;
            RequireProperty(serializedRunner, "sourceActorVisibilityRoot").objectReferenceValue = sourceActorRig;
            ConfigurePhase2AkazaSourceActorBindings(serializedRunner, sourceTimelineWrapper, introProfile);
            ConfigurePhase2AkazaSourceActorGradeExclusions(serializedRunner, sourceActorRig);
            RequireProperty(serializedRunner, "primaryActorRootHiddenDuringSourceActorAnimation").objectReferenceValue =
                visual;
            ConfigurePhase2AkazaIntroScreenFade(serializedRunner, scene, camera);
            RequireProperty(serializedRunner, "playLinkedActionCueOnStart").boolValue = false;
            RequireProperty(serializedRunner, "linkedActionCueDirector").objectReferenceValue = null;

            SerializedProperty actorBindings = RequireProperty(serializedRunner, "actorBindings");
            actorBindings.arraySize = 1;
            SerializedProperty bossBinding = actorBindings.GetArrayElementAtIndex(0);
            bossBinding.FindPropertyRelative("role").enumValueIndex =
                (int)CinematicSequenceProfile.ActorRole.Boss;
            bossBinding.FindPropertyRelative("bodyAnimator").objectReferenceValue = animator;
            bossBinding.FindPropertyRelative("faceAnimator").objectReferenceValue = null;
            bossBinding.FindPropertyRelative("expressionPlayer").objectReferenceValue = null;
            bossBinding.FindPropertyRelative("anchor").objectReferenceValue = visual.transform;
            ConfigurePhase2AkazaIntroPlaybackLocks(serializedRunner, bossProxy, visual);
            serializedRunner.ApplyModifiedPropertiesWithoutUndo();
            SetBehaviourEnabled(runner, true);

            CinematicSequencePlaylistRunner playlist =
                EnsureComponent<CinematicSequencePlaylistRunner>(cameraController.gameObject);
            ConfigurePhase2AkazaIntroPlaylist(playlist, runner, introProfile);

            ActionCinematicSequenceBridge sequenceBridge =
                EnsureComponent<ActionCinematicSequenceBridge>(cameraController.gameObject);
            ConfigurePhase2AkazaIntroSequenceBridge(sequenceBridge, runner, introProfile);
            ActionCinematicCueDirector cueDirector =
                EnsureComponent<ActionCinematicCueDirector>(cameraController.gameObject);
            ConfigurePhase2AkazaIntroCueDirector(cueDirector, cameraController, sequenceBridge, playerTransform);
            ConfigurePhase2AkazaIntroAutoPlay(cameraController.gameObject, cueDirector);

            EditorUtility.SetDirty(runner);
            EditorUtility.SetDirty(playlist);
            EditorUtility.SetDirty(sequenceBridge);
            EditorUtility.SetDirty(cueDirector);
        }

        private static GameObject EnsurePhase2AkazaOriginalC23CameraRig(Scene scene, Transform visualTransform)
        {
            RemovePhase2AkazaStaleSourceRigRoot(scene);

            GameObject wrapper = FindRoot(scene, Phase2AkazaC23CameraRigWrapperName);
            if (wrapper == null)
            {
                wrapper = CreateRoot(scene, Phase2AkazaC23CameraRigWrapperName);
            }

            Vector3 visualWorldPosition = visualTransform != null ? visualTransform.position : Vector3.zero;
            wrapper.SetActive(true);
            wrapper.transform.SetPositionAndRotation(
                visualWorldPosition - Phase2AkazaOriginalC23ActorWorldPosition,
                Quaternion.identity);
            wrapper.transform.localScale = Vector3.one;

            TimelineSourceClip[] cameraClips = BuildPhase2AkazaExpectedTimelineCameraClips();
            for (int i = 0; i < cameraClips.Length; i++)
            {
                EnsurePhase2AkazaOriginalTimelineCameraRig(wrapper.transform, cameraClips[i].AssetPath);
            }

            Transform existingRig = wrapper.transform.Find(Phase2AkazaC23CameraRigSourceName);
            if (existingRig == null)
            {
                throw new InvalidOperationException($"{Phase2AkazaC23CameraRigSourceName} source rig was not created.");
            }

            return existingRig.gameObject;
        }

        private static void RemovePhase2AkazaStaleSourceRigRoot(Scene scene)
        {
            GameObject staleRoot = FindRoot(scene, Phase2AkazaStaleC23CameraRigWrapperName);
            if (staleRoot != null)
            {
                UnityEngine.Object.DestroyImmediate(staleRoot);
            }
        }

        private static GameObject EnsurePhase2AkazaOriginalTimelineCameraRig(
            Transform wrapper,
            string sourceAssetPath)
        {
            string rigName = Path.GetFileNameWithoutExtension(sourceAssetPath);
            Transform existingRig = wrapper.Find(rigName);
            GameObject sourceRig = existingRig != null ? existingRig.gameObject : null;
            if (sourceRig == null)
            {
                GameObject sourcePrefab = LoadAsset<GameObject>(sourceAssetPath);
                sourceRig = PrefabUtility.InstantiatePrefab(sourcePrefab, wrapper.gameObject.scene) as GameObject;
                if (sourceRig == null)
                {
                    throw new InvalidOperationException($"Could not instantiate {sourceAssetPath}.");
                }

                sourceRig.transform.SetParent(wrapper, worldPositionStays: false);
            }

            sourceRig.name = rigName;
            sourceRig.SetActive(true);
            sourceRig.transform.localPosition = Vector3.zero;
            sourceRig.transform.localRotation = Quaternion.identity;
            sourceRig.transform.localScale = Vector3.one;

            EnsurePhase2AkazaSourceCameraComponent(sourceRig);
            Camera[] cameras = sourceRig.GetComponentsInChildren<Camera>(includeInactive: true);

            for (int i = 0; i < cameras.Length; i++)
            {
                cameras[i].enabled = false;
                cameras[i].gameObject.SetActive(true);
                EditorUtility.SetDirty(cameras[i]);
            }

            EditorUtility.SetDirty(sourceRig);
            return sourceRig;
        }

        private static Camera EnsurePhase2AkazaSourceCameraComponent(GameObject sourceRig)
        {
            Transform c08CameraTransform = FindChildRecursive(sourceRig.transform, "C08_CamTrans");
            Transform originalCameraTransform = c08CameraTransform != null
                ? c08CameraTransform.Find("Camera")
                : null;
            if (c08CameraTransform != null && originalCameraTransform == null)
            {
                GameObject cameraObject = new GameObject("Camera");
                Undo.RegisterCreatedObjectUndo(cameraObject, "Create Phase2 Akaza source camera");
                originalCameraTransform = cameraObject.transform;
                originalCameraTransform.SetParent(c08CameraTransform, worldPositionStays: false);
                originalCameraTransform.localPosition = new Vector3(0.39f, 0f, 0f);
                originalCameraTransform.localRotation = Quaternion.Euler(-1.286f, 0f, 0f);
                originalCameraTransform.localScale = Vector3.one;
                EditorUtility.SetDirty(cameraObject);
            }

            Transform cameraTransform =
                originalCameraTransform
                ?? FindChildRecursive(sourceRig.transform, "Camera")
                ?? c08CameraTransform
                ?? FindChildRecursive(sourceRig.transform, "C08_Cam")
                ?? sourceRig.transform;

            Camera existingCamera = sourceRig.GetComponentInChildren<Camera>(includeInactive: true);
            if (existingCamera != null && existingCamera.transform != cameraTransform)
            {
                UnityEngine.Object.DestroyImmediate(existingCamera, true);
                existingCamera = null;
            }

            Camera camera = existingCamera;
            if (existingCamera != null)
            {
                camera = existingCamera;
            }
            else
            {
                camera = EnsureComponent<Camera>(cameraTransform.gameObject);
            }

            camera.clearFlags = CameraClearFlags.Skybox;
            camera.backgroundColor = new Color(0.19215687f, 0.3019608f, 0.4745098f, 0f);
            camera.fieldOfView = 18f;
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 1000f;
            camera.depth = 0f;
            camera.enabled = false;
            if (c08CameraTransform != null)
            {
                EnsurePhase2AkazaC08FaceShadowOverlay(camera);
            }

            EditorUtility.SetDirty(camera);
            return camera;
        }

        private static void DisablePhase2AkazaC08FaceShadowOverlay(Camera camera)
        {
            if (camera == null)
            {
                return;
            }

            Transform overlay = camera.transform.Find(Phase2AkazaC08FaceShadowOverlayName);
            if (overlay == null)
            {
                return;
            }

            overlay.gameObject.SetActive(false);
            MeshRenderer renderer = overlay.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.enabled = false;
                renderer.forceRenderingOff = true;
                EditorUtility.SetDirty(renderer);
            }

            EditorUtility.SetDirty(overlay.gameObject);
        }

        private static void EnsurePhase2AkazaC08FaceShadowOverlay(Camera camera)
        {
            if (camera == null)
            {
                return;
            }

            Transform overlay = EnsureChild(camera.transform, Phase2AkazaC08FaceShadowOverlayName);
            overlay.localPosition = Vector3.zero;
            overlay.localRotation = Quaternion.identity;
            overlay.localScale = Vector3.one;
            overlay.gameObject.SetActive(true);

            MeshFilter meshFilter = EnsureComponent<MeshFilter>(overlay.gameObject);
            meshFilter.sharedMesh = EnsurePhase2AkazaC08FaceShadowOverlayMesh(camera.fieldOfView);

            EnsurePhase2AkazaC08FaceShadowOverlayTexture();
            Material material = LoadOrCreateTransparentTexturedMaterial(
                Phase2AkazaIntroSourceMaterialRoot + "/M_C08_FaceShadowOverlay.mat",
                new Color(0.42f, 0.12f, 0.055f, 0.38f),
                Phase2AkazaC08FaceShadowOverlayTexturePath);
            SetMaterialFloatIfPresent(material, "_Cull", 0f);
            SetMaterialFloatIfPresent(material, "_CullMode", 0f);
            SetMaterialFloatIfPresent(material, "_ZWrite", 0f);
            material.renderQueue = (int)RenderQueue.Transparent + 80;

            MeshRenderer renderer = EnsureComponent<MeshRenderer>(overlay.gameObject);
            renderer.sharedMaterial = material;
            renderer.enabled = true;
            renderer.forceRenderingOff = false;
            renderer.allowOcclusionWhenDynamic = false;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            EditorUtility.SetDirty(material);
            EditorUtility.SetDirty(overlay.gameObject);
        }

        private static Mesh EnsurePhase2AkazaC08FaceShadowOverlayMesh(float cameraFieldOfView)
        {
            EnsureFolderForAsset(Phase2AkazaC08FaceShadowOverlayMeshPath);
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(Phase2AkazaC08FaceShadowOverlayMeshPath);
            if (mesh == null)
            {
                mesh = new Mesh
                {
                    name = "DB_C08_FaceShadowOverlay"
                };
                AssetDatabase.CreateAsset(mesh, Phase2AkazaC08FaceShadowOverlayMeshPath);
            }

            const float overlayDistance = 0.45f;
            const float sourceAspect = 16f / 9f;
            float halfHeight = Mathf.Tan(cameraFieldOfView * 0.5f * Mathf.Deg2Rad) * overlayDistance;
            float halfWidth = halfHeight * sourceAspect;
            Vector3[] vertices =
            {
                ToPhase2AkazaC08OverlayPoint(0f, 0f, halfWidth, halfHeight, overlayDistance),
                ToPhase2AkazaC08OverlayPoint(1f, 0f, halfWidth, halfHeight, overlayDistance),
                ToPhase2AkazaC08OverlayPoint(1f, 1f, halfWidth, halfHeight, overlayDistance),
                ToPhase2AkazaC08OverlayPoint(0f, 1f, halfWidth, halfHeight, overlayDistance),
            };

            mesh.Clear();
            mesh.vertices = vertices;
            mesh.triangles = new[]
            {
                0, 2, 1,
                0, 3, 2,
            };
            mesh.uv = new[]
            {
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(1f, 1f),
                new Vector2(0f, 1f),
            };
            mesh.RecalculateBounds();
            mesh.UploadMeshData(false);
            EditorUtility.SetDirty(mesh);
            return mesh;
        }

        private static void EnsurePhase2AkazaC08FaceShadowOverlayTexture()
        {
            EnsureFolderForAsset(Phase2AkazaC08FaceShadowOverlayTexturePath);
            const int textureSize = 512;
            Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false, false)
            {
                name = "DB_C08_FaceShadowOverlay"
            };

            Vector2[] shadowPolygon =
            {
                new Vector2(0.305f, 0.94f),
                new Vector2(0.548f, 0.885f),
                new Vector2(0.512f, 0.66f),
                new Vector2(0.458f, 0.535f),
                new Vector2(0.382f, 0.34f),
                new Vector2(0.312f, 0.36f),
                new Vector2(0.298f, 0.585f),
            };

            Color[] pixels = new Color[textureSize * textureSize];
            for (int y = 0; y < textureSize; y++)
            {
                for (int x = 0; x < textureSize; x++)
                {
                    Vector2 uv = new Vector2(
                        (x + 0.5f) / textureSize,
                        (y + 0.5f) / textureSize);
                    float alpha = CalculatePhase2AkazaC08FaceShadowAlpha(uv, shadowPolygon);
                    pixels[y * textureSize + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply(false, false);
            File.WriteAllBytes(Phase2AkazaC08FaceShadowOverlayTexturePath, texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(Phase2AkazaC08FaceShadowOverlayTexturePath, ImportAssetOptions.ForceUpdate);
            TextureImporter importer =
                AssetImporter.GetAtPath(Phase2AkazaC08FaceShadowOverlayTexturePath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Default;
                importer.alphaSource = TextureImporterAlphaSource.FromInput;
                importer.mipmapEnabled = false;
                importer.sRGBTexture = true;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.filterMode = FilterMode.Bilinear;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
        }

        private static void EnsurePhase2AkazaC08CorrectedEyesTexture()
        {
            EnsureFolderForAsset(Phase2AkazaC08CorrectedEyesTexturePath);
            Texture2D main = LoadReadablePhase2AkazaTexture(Phase2AkazaC08OriginalFaceTexturePath);
            Texture2D tint = LoadReadablePhase2AkazaTexture(Phase2AkazaC08OriginalFaceBTexturePath);
            if (main == null || tint == null)
            {
                return;
            }

            int width = main.width;
            int height = main.height;
            Color32[] mainPixels = main.GetPixels32();
            Color32[] tintPixels = tint.width == width && tint.height == height
                ? tint.GetPixels32()
                : null;
            Texture2D output = new Texture2D(width, height, TextureFormat.RGBA32, false, false)
            {
                name = "DB_C08_Akaza_EyesCorrected"
            };

            Color32[] outputPixels = new Color32[mainPixels.Length];
            for (int i = 0; i < mainPixels.Length; i++)
            {
                Color32 pixel = mainPixels[i];
                if (IsPhase2AkazaC08IrisPixel(pixel))
                {
                    Color32 tintPixel = tintPixels != null ? tintPixels[i] : new Color32(45, 150, 145, pixel.a);
                    float sourceLuma =
                        (pixel.r * 0.299f + pixel.g * 0.587f + pixel.b * 0.114f) / 255f;
                    float detailScale = Mathf.Lerp(
                        0.68f,
                        1.18f,
                        Mathf.Clamp01((sourceLuma - 0.48f) / 0.34f));
                    pixel.r = (byte)Mathf.Clamp(Mathf.RoundToInt(tintPixel.r * 0.9f * detailScale), 0, 255);
                    pixel.g = (byte)Mathf.Clamp(Mathf.RoundToInt(tintPixel.g * 0.98f * detailScale), 0, 255);
                    pixel.b = (byte)Mathf.Clamp(Mathf.RoundToInt(tintPixel.b * 0.92f * detailScale), 0, 255);
                }

                outputPixels[i] = pixel;
            }

            output.SetPixels32(outputPixels);
            output.Apply(false, false);
            File.WriteAllBytes(Phase2AkazaC08CorrectedEyesTexturePath, output.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(output);
            AssetDatabase.ImportAsset(Phase2AkazaC08CorrectedEyesTexturePath, ImportAssetOptions.ForceUpdate);

            TextureImporter importer =
                AssetImporter.GetAtPath(Phase2AkazaC08CorrectedEyesTexturePath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Default;
                importer.alphaSource = TextureImporterAlphaSource.FromInput;
                importer.mipmapEnabled = false;
                importer.sRGBTexture = true;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
        }

        private static Texture2D LoadReadablePhase2AkazaTexture(string texturePath)
        {
            TextureImporter importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
            if (importer != null && !importer.isReadable)
            {
                importer.isReadable = true;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
        }

        private static bool IsPhase2AkazaC08IrisPixel(Color32 pixel)
        {
            return pixel.a > 0
                && pixel.g >= 170
                && pixel.r >= 110
                && pixel.b <= 80
                && pixel.g - pixel.b >= 95;
        }

        private static float CalculatePhase2AkazaC08FaceShadowAlpha(Vector2 uv, Vector2[] polygon)
        {
            if (!IsPointInPhase2AkazaPolygon(uv, polygon))
            {
                return 0f;
            }

            float edgeDistance = DistanceToPhase2AkazaPolygonEdge(uv, polygon);
            float edgeFade = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(edgeDistance / 0.006f));
            float lowerCheekFade = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((uv.y - 0.245f) / 0.055f));
            float upperFaceFade = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((0.91f - uv.y) / 0.09f));
            float centerWeight = Mathf.Clamp01(1f - Mathf.Abs(uv.x - 0.41f) / 0.18f);
            float diagonalRightLimit = Mathf.Lerp(0.405f, 0.545f, Mathf.Clamp01((uv.y - 0.25f) / 0.58f));
            float rightFalloff = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((uv.x - diagonalRightLimit) / 0.025f));
            float leftFalloff = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((uv.x - 0.298f) / 0.025f));
            float lowerSoftness = Mathf.Min(lowerCheekFade + 0.26f, 1f);
            float upperSoftness = Mathf.Min(upperFaceFade + 0.35f, 1f);
            return 0.78f * edgeFade * Mathf.Lerp(0.78f, 1f, centerWeight) * leftFalloff * rightFalloff
                * lowerSoftness * upperSoftness;
        }

        private static bool IsPointInPhase2AkazaPolygon(Vector2 point, Vector2[] polygon)
        {
            bool inside = false;
            for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
            {
                bool crosses = (polygon[i].y > point.y) != (polygon[j].y > point.y)
                    && point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y)
                    / (polygon[j].y - polygon[i].y) + polygon[i].x;
                if (crosses)
                {
                    inside = !inside;
                }
            }

            return inside;
        }

        private static float DistanceToPhase2AkazaPolygonEdge(Vector2 point, Vector2[] polygon)
        {
            float closest = float.PositiveInfinity;
            for (int i = 0; i < polygon.Length; i++)
            {
                Vector2 a = polygon[i];
                Vector2 b = polygon[(i + 1) % polygon.Length];
                closest = Mathf.Min(closest, DistanceToPhase2AkazaSegment(point, a, b));
            }

            return closest;
        }

        private static float DistanceToPhase2AkazaSegment(Vector2 point, Vector2 a, Vector2 b)
        {
            Vector2 segment = b - a;
            float segmentLength = Vector2.Dot(segment, segment);
            if (segmentLength <= Mathf.Epsilon)
            {
                return Vector2.Distance(point, a);
            }

            float t = Mathf.Clamp01(Vector2.Dot(point - a, segment) / segmentLength);
            return Vector2.Distance(point, a + segment * t);
        }

        private static Vector3 ToPhase2AkazaC08OverlayPoint(
            float viewportX,
            float viewportY,
            float halfWidth,
            float halfHeight,
            float distance)
        {
            return new Vector3(
                (viewportX - 0.5f) * 2f * halfWidth,
                (viewportY - 0.5f) * 2f * halfHeight,
                distance);
        }

        private static void ConfigurePhase2AkazaSourceCameraBindings(
            SerializedObject serializedRunner,
            Transform sourceTimelineWrapper,
            CinematicSequenceProfile introProfile)
        {
            CinematicSequenceProfile.SourceCameraAnimationCue[] cues =
                introProfile.SourceCameraAnimations;
            SerializedProperty bindings = RequireProperty(serializedRunner, "sourceCameraBindings");
            bindings.arraySize = cues.Length;
            for (int i = 0; i < cues.Length; i++)
            {
                string rigName = ResolvePhase2AkazaCameraRigNameForCue(cues[i]);
                Transform rig = sourceTimelineWrapper.Find(rigName);
                if (rig == null)
                {
                    throw new InvalidOperationException($"Missing source camera rig {rigName} for {cues[i].CueId}.");
                }

                Camera rigCamera = rig.GetComponentInChildren<Camera>(includeInactive: true);
                if (rigCamera == null)
                {
                    throw new InvalidOperationException($"{rigName} must contain a Camera.");
                }

                SerializedProperty binding = bindings.GetArrayElementAtIndex(i);
                binding.FindPropertyRelative("cueId").stringValue = cues[i].CueId;
                binding.FindPropertyRelative("rigRoot").objectReferenceValue = rig.gameObject;
                binding.FindPropertyRelative("cameraTransform").objectReferenceValue = rigCamera.transform;
                binding.FindPropertyRelative("cameraComponent").objectReferenceValue = rigCamera;
            }
        }

        private static string ResolvePhase2AkazaCameraRigNameForCue(
            CinematicSequenceProfile.SourceCameraAnimationCue cue)
        {
            string assetPath = AssetDatabase.GetAssetPath(cue.Clip);
            return Path.GetFileNameWithoutExtension(assetPath);
        }

        private static GameObject EnsurePhase2AkazaOriginalTimelineActorRigs(Transform wrapper)
        {
            if (wrapper == null)
            {
                throw new InvalidOperationException($"{Phase2AkazaC23CameraRigWrapperName} is missing a wrapper.");
            }

            TimelineSourceClip[] sourceActorClips = BuildPhase2AkazaExpectedTimelineSourceActorClips();
            HashSet<string> rigNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            GameObject fallbackActor = null;
            for (int i = 0; i < sourceActorClips.Length; i++)
            {
                TimelineSourceClip sourceClip = sourceActorClips[i];
                string rigName = ResolvePhase2AkazaActorRigNameForSourceClip(sourceClip);
                if (!rigNames.Add(rigName))
                {
                    continue;
                }

                bool useC08AkazaIntroMaterials = string.Equals(
                    sourceClip.AssetPath,
                    Phase2AkazaC08ActorSourcePath,
                    StringComparison.OrdinalIgnoreCase);
                bool applyAkazaMaterials = rigName.IndexOf("akaza", StringComparison.OrdinalIgnoreCase) >= 0
                    && !useC08AkazaIntroMaterials;
                string modelPrefabPath = ResolvePhase2AkazaModelPrefabPathForSourceClip(sourceClip);
                GameObject actor = EnsurePhase2AkazaOriginalTimelineActorRig(
                    wrapper,
                    rigName,
                    modelPrefabPath,
                    sourceClip.AssetPath,
                    applyAkazaMaterials,
                    useC08AkazaIntroMaterials);
                if (fallbackActor == null
                    || string.Equals(rigName, Phase2AkazaC23ActorRigSourceName, StringComparison.OrdinalIgnoreCase))
                {
                    fallbackActor = actor;
                }
            }

            if (fallbackActor == null)
            {
                throw new InvalidOperationException($"{Phase2AkazaC23CameraRigWrapperName} has no source actor rigs.");
            }

            return fallbackActor;
        }

        private static GameObject EnsurePhase2AkazaOriginalTimelineActorRig(
            Transform wrapper,
            string rigName,
            string prefabAssetPath,
            string sampleClipPath,
            bool applyAkazaMaterials,
            bool useC08AkazaIntroMaterials)
        {
            Transform existingActor = FindChildRecursive(wrapper, rigName);
            GameObject sourceActor = existingActor != null ? existingActor.gameObject : null;
            if (sourceActor != null && !IsPrefabInstanceFromAsset(sourceActor, prefabAssetPath))
            {
                UnityEngine.Object.DestroyImmediate(sourceActor);
                sourceActor = null;
            }

            if (sourceActor == null)
            {
                GameObject sourcePrefab = LoadAsset<GameObject>(prefabAssetPath);
                sourceActor = PrefabUtility.InstantiatePrefab(sourcePrefab, wrapper.gameObject.scene) as GameObject;
                if (sourceActor == null)
                {
                    throw new InvalidOperationException($"Could not instantiate {prefabAssetPath}.");
                }

                sourceActor.transform.SetParent(wrapper, worldPositionStays: false);
            }

            sourceActor.name = rigName;
            sourceActor.transform.localPosition = Vector3.zero;
            sourceActor.transform.localRotation = Quaternion.identity;
            sourceActor.transform.localScale = Vector3.one;

            Animator sourceAnimator = sourceActor.GetComponentInChildren<Animator>(includeInactive: true);
            if (sourceAnimator != null)
            {
                sourceAnimator.runtimeAnimatorController = null;
                sourceAnimator.applyRootMotion = false;
                sourceAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                EditorUtility.SetDirty(sourceAnimator);
            }

            if (useC08AkazaIntroMaterials)
            {
                ApplyPhase2AkazaIntroAkazaMaterials(sourceActor);
                EnsurePhase2AkazaC08SourceSceneContext(sourceActor.transform);
            }
            else if (applyAkazaMaterials)
            {
                ApplyPhase2AkazaMaterials(sourceActor);
            }
            else
            {
                ApplyPhase2AkazaIntroSourceMaterials(sourceActor, rigName);
            }

            AnimationClip sourceActorClip = LoadPrimaryAnimationClip(sampleClipPath);
            sourceActor.SetActive(true);
            if (sourceActorClip != null)
            {
                sourceActorClip.SampleAnimation(sourceActor, 0f);
            }

            sourceActor.SetActive(false);
            EditorUtility.SetDirty(sourceActor);
            return sourceActor;
        }

        private static void EnsurePhase2AkazaC08SourceSceneContext(Transform sourceActor)
        {
            Transform context = EnsureChild(sourceActor, Phase2AkazaC08SourceSceneContextName);
            context.localPosition = Vector3.zero;
            context.localRotation = Quaternion.identity;
            context.localScale = Vector3.one;
            context.gameObject.SetActive(true);

            EnsurePhase2AkazaC08SourceSkyBackdrop(context);
            DisablePhase2AkazaC08HeadShadowPlane(sourceActor);
            EnsurePhase2AkazaC08SourceLight(context);
            EnsurePhase2AkazaC08SourcePostProcess(context);
            EditorUtility.SetDirty(context.gameObject);
        }

        private static void EnsurePhase2AkazaC08SourceSkyBackdrop(Transform context)
        {
            Transform backdrop = EnsureChild(context, Phase2AkazaC08SkyBackdropName);
            backdrop.localPosition = new Vector3(-1f, 47.9f, 99.6f);
            backdrop.localRotation = Quaternion.Euler(69.1f, 0f, 180f);
            backdrop.localScale = new Vector3(12.17f, 7.14f, 3.19f);
            backdrop.gameObject.SetActive(true);

            MeshFilter meshFilter = EnsureComponent<MeshFilter>(backdrop.gameObject);
            meshFilter.sharedMesh = LoadPrimitiveMesh(PrimitiveType.Plane);

            Material sky = LoadOrCreateTexturedMaterial(
                Phase2AkazaIntroSourceMaterialRoot + "/M_C08_UnitySoraLoop.mat",
                new Color(0.8f, 0.8f, 0.8f, 1f),
                Phase2AkazaC08OriginalSkyLoopTexturePath);
            SetMaterialFloatIfPresent(sky, "_Cull", 0f);
            SetMaterialFloatIfPresent(sky, "_CullMode", 0f);
            MeshRenderer renderer = EnsureComponent<MeshRenderer>(backdrop.gameObject);
            renderer.sharedMaterial = sky;
            ConfigurePhase2AkazaRendererVisibility(renderer);
            EditorUtility.SetDirty(backdrop.gameObject);
        }

        private static void DisablePhase2AkazaC08HeadShadowPlane(Transform sourceActor)
        {
            if (sourceActor == null)
            {
                return;
            }

            Transform plane = sourceActor.Find(Phase2AkazaC08HeadShadowPlaneName);
            if (plane == null)
            {
                return;
            }

            UnityEngine.Object.DestroyImmediate(plane.gameObject);
        }

        private static void EnsurePhase2AkazaC08SourceLight(Transform context)
        {
            Transform lightTransform = EnsureChild(context, Phase2AkazaC08DirectionalLightName);
            lightTransform.localPosition = new Vector3(1.181f, 13.923f, 42.055f);
            lightTransform.localRotation = new Quaternion(-0.2018489f, 0.10208921f, -0.34166265f, -0.912196f);
            lightTransform.localScale = Vector3.one;
            lightTransform.gameObject.SetActive(true);

            Light light = EnsureComponent<Light>(lightTransform.gameObject);
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.6608519f, 0.44117647f, 1f);
            light.intensity = 1.42f;
            light.bounceIntensity = 1f;
            light.shadows = LightShadows.Soft;
            light.shadowStrength = 1f;
            light.cullingMask = ~0;
            RenderSettings.sun = light;
            EditorUtility.SetDirty(light);
            EditorUtility.SetDirty(lightTransform.gameObject);
        }

        private static void EnsurePhase2AkazaC08SourcePostProcess(Transform context)
        {
            Transform postProcessTransform = EnsureChild(context, Phase2AkazaC08PostProcessName);
            postProcessTransform.localPosition = Vector3.zero;
            postProcessTransform.localRotation = Quaternion.identity;
            postProcessTransform.localScale = Vector3.one;
            postProcessTransform.gameObject.SetActive(true);

            Volume volume = EnsureComponent<Volume>(postProcessTransform.gameObject);
            volume.isGlobal = true;
            volume.priority = 120f;
            volume.weight = 1f;
            volume.sharedProfile = EnsurePhase2AkazaC08SourcePostProcessProfile();
            EditorUtility.SetDirty(volume);
            EditorUtility.SetDirty(postProcessTransform.gameObject);
        }

        private static void EnsurePhase2AkazaC08CombatLookSceneContext(Scene scene)
        {
            GameObject contextObject = FindRoot(scene, Phase2AkazaC08CombatLookSceneContextName);
            if (contextObject == null)
            {
                contextObject = new GameObject(Phase2AkazaC08CombatLookSceneContextName);
                SceneManager.MoveGameObjectToScene(contextObject, scene);
            }

            contextObject.SetActive(true);
            contextObject.layer = 0;
            contextObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            contextObject.transform.localScale = Vector3.one;

            EnsurePhase2AkazaC08CombatSkyBackdrop(contextObject.transform);
            EnsurePhase2AkazaC08SourceLight(contextObject.transform);
            EnsurePhase2AkazaC08CombatLookPostProcess(scene);
            EditorUtility.SetDirty(contextObject);
        }

        private static void EnsurePhase2AkazaC08CombatSkyBackdrop(Transform context)
        {
            Transform backdrop = EnsureChild(context, Phase2AkazaC08SkyBackdropName);
            backdrop.localPosition = new Vector3(0f, 8f, 42f);
            backdrop.localRotation = Quaternion.Euler(90f, 0f, 180f);
            backdrop.localScale = new Vector3(14f, 1f, 8f);
            backdrop.gameObject.SetActive(true);

            MeshFilter meshFilter = EnsureComponent<MeshFilter>(backdrop.gameObject);
            meshFilter.sharedMesh = LoadPrimitiveMesh(PrimitiveType.Plane);

            Material sky = LoadOrCreateTexturedMaterial(
                Phase2AkazaIntroSourceMaterialRoot + "/M_C08_UnitySoraLoop.mat",
                new Color(0.8f, 0.8f, 0.8f, 1f),
                Phase2AkazaC08OriginalSkyLoopTexturePath);
            SetMaterialFloatIfPresent(sky, "_Cull", 0f);
            SetMaterialFloatIfPresent(sky, "_CullMode", 0f);
            MeshRenderer renderer = EnsureComponent<MeshRenderer>(backdrop.gameObject);
            renderer.sharedMaterial = sky;
            ConfigurePhase2AkazaRendererVisibility(renderer);
            EditorUtility.SetDirty(backdrop.gameObject);
        }

        private static void EnsurePhase2AkazaC08CombatLookPostProcess(Scene scene)
        {
            GameObject postProcessObject = FindRoot(scene, Phase2AkazaC08CombatLookPostProcessName);
            if (postProcessObject == null)
            {
                postProcessObject = new GameObject(Phase2AkazaC08CombatLookPostProcessName);
                SceneManager.MoveGameObjectToScene(postProcessObject, scene);
            }

            postProcessObject.SetActive(true);
            postProcessObject.layer = 0;
            postProcessObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            postProcessObject.transform.localScale = Vector3.one;

            Volume volume = EnsureComponent<Volume>(postProcessObject);
            volume.enabled = true;
            volume.isGlobal = true;
            volume.priority = 121f;
            volume.weight = 1f;
            volume.sharedProfile = EnsurePhase2AkazaC08SourcePostProcessProfile();
            EditorUtility.SetDirty(volume);
            EditorUtility.SetDirty(postProcessObject);
        }

        private static void ConfigurePhase2AkazaC08SourceCameraPostProcessing(Camera camera)
        {
            if (camera == null)
            {
                return;
            }

            camera.allowHDR = true;
            camera.allowMSAA = true;
            UniversalAdditionalCameraData cameraData = camera.GetUniversalAdditionalCameraData();
            cameraData.renderPostProcessing = true;
            cameraData.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
            cameraData.antialiasingQuality = AntialiasingQuality.High;
            EditorUtility.SetDirty(camera);
            EditorUtility.SetDirty(cameraData);
        }

        private static void ConfigurePhase2AkazaC08SourceRenderSettings()
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.29411763f, 0.21972317f, 0.108131476f, 0.6039216f);
            RenderSettings.fogDensity = 0.13f;
            RenderSettings.fogStartDistance = -30.1f;
            RenderSettings.fogEndDistance = 600f;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.1f, 0.1f, 0.1f, 1f);
            RenderSettings.ambientEquatorColor = new Color(0.114f, 0.125f, 0.133f, 1f);
            RenderSettings.ambientGroundColor = new Color(0.047f, 0.043f, 0.035f, 1f);
            RenderSettings.ambientIntensity = 1f;
        }

        private static VolumeProfile EnsurePhase2AkazaC08SourcePostProcessProfile()
        {
            EnsureFolderForAsset(Phase2AkazaC08PostProcessProfilePath);
            VolumeProfile profile =
                AssetDatabase.LoadAssetAtPath<VolumeProfile>(Phase2AkazaC08PostProcessProfilePath);
            bool createdProfile = false;
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, Phase2AkazaC08PostProcessProfilePath);
                createdProfile = true;
            }
            else
            {
                profile.components.RemoveAll(component => component == null);
            }

            Bloom bloom = GetOrAddPhase2AkazaVolumeComponent<Bloom>(profile, out bool bloomAdded);
            bloom.active = true;
            if (createdProfile || bloomAdded)
            {
                SetPhase2AkazaVolumeParameter(bloom.threshold, 0.58f);
                SetPhase2AkazaVolumeParameter(bloom.intensity, 0.78f);
                SetPhase2AkazaVolumeParameter(bloom.scatter, 0.9f);
                SetPhase2AkazaVolumeParameter(bloom.clamp, 65472f);
                SetPhase2AkazaVolumeParameter(
                    bloom.tint,
                    new Color(1f, 0.93f, 0.78f, 1f));
            }

            Tonemapping tonemapping = GetOrAddPhase2AkazaVolumeComponent<Tonemapping>(
                profile,
                out bool tonemappingAdded);
            tonemapping.active = true;
            if (createdProfile || tonemappingAdded)
            {
                SetPhase2AkazaVolumeParameter(tonemapping.mode, TonemappingMode.Neutral);
            }

            WhiteBalance whiteBalance = GetOrAddPhase2AkazaVolumeComponent<WhiteBalance>(
                profile,
                out bool whiteBalanceAdded);
            whiteBalance.active = true;
            if (createdProfile || whiteBalanceAdded)
            {
                SetPhase2AkazaVolumeParameter(whiteBalance.temperature, 20f);
                SetPhase2AkazaVolumeParameter(whiteBalance.tint, 4f);
            }

            ColorAdjustments colorAdjustments = GetOrAddPhase2AkazaVolumeComponent<ColorAdjustments>(
                profile,
                out bool colorAdjustmentsAdded);
            colorAdjustments.active = true;
            if (createdProfile || colorAdjustmentsAdded)
            {
                SetPhase2AkazaVolumeParameter(colorAdjustments.postExposure, -0.03f);
                SetPhase2AkazaVolumeParameter(colorAdjustments.contrast, -6f);
                SetPhase2AkazaVolumeParameter(colorAdjustments.saturation, -3f);
                SetPhase2AkazaVolumeParameter(
                    colorAdjustments.colorFilter,
                    new Color(1f, 0.992f, 0.965f, 1f));
            }

            LiftGammaGain liftGammaGain = GetOrAddPhase2AkazaVolumeComponent<LiftGammaGain>(
                profile,
                out bool liftGammaGainAdded);
            liftGammaGain.active = true;
            if (createdProfile || liftGammaGainAdded)
            {
                SetPhase2AkazaVolumeParameter(
                    liftGammaGain.lift,
                    new Vector4(1f, 1f, 1f, -0.002f));
                SetPhase2AkazaVolumeParameter(
                    liftGammaGain.gamma,
                    new Vector4(1f, 0.998f, 0.982f, -0.001f));
                SetPhase2AkazaVolumeParameter(
                    liftGammaGain.gain,
                    new Vector4(1.055f, 0.99f, 0.94f, 0.008f));
            }

            ShadowsMidtonesHighlights shadowsMidtonesHighlights =
                GetOrAddPhase2AkazaVolumeComponent<ShadowsMidtonesHighlights>(
                    profile,
                    out bool shadowsMidtonesHighlightsAdded);
            shadowsMidtonesHighlights.active = true;
            if (createdProfile || shadowsMidtonesHighlightsAdded)
            {
                SetPhase2AkazaVolumeParameter(
                    shadowsMidtonesHighlights.shadows,
                    new Vector4(1f, 1f, 0.985f, -0.002f));
                SetPhase2AkazaVolumeParameter(
                    shadowsMidtonesHighlights.midtones,
                    new Vector4(1.02f, 1f, 0.975f, 0f));
                SetPhase2AkazaVolumeParameter(
                    shadowsMidtonesHighlights.highlights,
                    new Vector4(1.06f, 0.995f, 0.92f, 0.008f));
                SetPhase2AkazaVolumeParameter(shadowsMidtonesHighlights.shadowsStart, 0f);
                SetPhase2AkazaVolumeParameter(shadowsMidtonesHighlights.shadowsEnd, 0.32f);
                SetPhase2AkazaVolumeParameter(shadowsMidtonesHighlights.highlightsStart, 0.6f);
                SetPhase2AkazaVolumeParameter(shadowsMidtonesHighlights.highlightsEnd, 1f);
            }

            ChromaticAberration chromaticAberration =
                GetOrAddPhase2AkazaVolumeComponent<ChromaticAberration>(
                    profile,
                    out bool chromaticAberrationAdded);
            chromaticAberration.active = true;
            if (createdProfile || chromaticAberrationAdded)
            {
                SetPhase2AkazaVolumeParameter(chromaticAberration.intensity, 0.02f);
            }

            Vignette vignette = GetOrAddPhase2AkazaVolumeComponent<Vignette>(
                profile,
                out bool vignetteAdded);
            vignette.active = true;
            if (createdProfile || vignetteAdded)
            {
                SetPhase2AkazaVolumeParameter(vignette.color, Color.black);
                SetPhase2AkazaVolumeParameter(vignette.center, new Vector2(0.5f, 0.5f));
                SetPhase2AkazaVolumeParameter(vignette.intensity, 0.08f);
                SetPhase2AkazaVolumeParameter(vignette.smoothness, 0.6f);
                SetPhase2AkazaVolumeParameter(vignette.rounded, false);
            }

            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssetIfDirty(profile);
            return profile;
        }

        private static T GetOrAddPhase2AkazaVolumeComponent<T>(
            VolumeProfile profile,
            out bool added)
            where T : VolumeComponent
        {
            if (!profile.TryGet(out T component) || component == null)
            {
                component = profile.Add<T>(overrides: true);
                added = true;
            }
            else
            {
                added = false;
            }

            EnsurePhase2AkazaVolumeComponentPersisted(profile, component);
            return component;
        }

        private static void EnsurePhase2AkazaVolumeComponentPersisted(
            VolumeProfile profile,
            VolumeComponent component)
        {
            if (profile == null || component == null || AssetDatabase.Contains(component))
            {
                return;
            }

            component.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
            AssetDatabase.AddObjectToAsset(component, profile);
            EditorUtility.SetDirty(component);
            EditorUtility.SetDirty(profile);
        }

        private static void SetPhase2AkazaVolumeParameter<T>(VolumeParameter<T> parameter, T value)
        {
            parameter.overrideState = true;
            parameter.value = value;
        }

        private static bool IsPrefabInstanceFromAsset(GameObject instance, string prefabAssetPath)
        {
            UnityEngine.Object source = PrefabUtility.GetCorrespondingObjectFromOriginalSource(instance);
            if (source == null)
            {
                return false;
            }

            string sourcePath = AssetDatabase.GetAssetPath(source);
            return string.Equals(
                NormalizeAssetPath(sourcePath),
                NormalizeAssetPath(prefabAssetPath),
                StringComparison.Ordinal);
        }

        private static void ConfigurePhase2AkazaSourceActorBindings(
            SerializedObject serializedRunner,
            Transform sourceTimelineWrapper,
            CinematicSequenceProfile introProfile)
        {
            CinematicSequenceProfile.SourceActorAnimationCue[] cues =
                introProfile.SourceActorAnimations;
            SerializedProperty bindings = RequireProperty(serializedRunner, "sourceActorBindings");
            bindings.arraySize = cues.Length;
            for (int i = 0; i < cues.Length; i++)
            {
                string rigName = ResolvePhase2AkazaActorRigNameForCue(cues[i]);
                Transform rig = FindChildRecursive(sourceTimelineWrapper, rigName);
                if (rig == null)
                {
                    throw new InvalidOperationException($"Missing source actor rig {rigName} for {cues[i].CueId}.");
                }

                SerializedProperty binding = bindings.GetArrayElementAtIndex(i);
                binding.FindPropertyRelative("cueId").stringValue = cues[i].CueId;
                binding.FindPropertyRelative("rigRoot").objectReferenceValue = rig.gameObject;
                binding.FindPropertyRelative("visibilityRoot").objectReferenceValue = rig.gameObject;
            }
        }

        private static void ConfigurePhase2AkazaSourceActorGradeExclusions(
            SerializedObject serializedRunner,
            GameObject sourceActorRig)
        {
            SerializedProperty exclusions = RequireProperty(serializedRunner, "sourceActorGradeExcludedRoots");
            Transform context = sourceActorRig != null
                ? sourceActorRig.transform.Find(Phase2AkazaC08SourceSceneContextName)
                : null;
            if (context == null)
            {
                exclusions.arraySize = 0;
                return;
            }

            exclusions.arraySize = 1;
            exclusions.GetArrayElementAtIndex(0).objectReferenceValue = context;
        }

        private static void EnsurePhase2AkazaC18SourceSceneContext(Transform wrapper)
        {
            Transform context = EnsureChild(wrapper, Phase2AkazaC18SourceSceneContextName);
            context.localPosition = Vector3.zero;
            context.localRotation = Quaternion.identity;
            context.localScale = Vector3.one;

            Transform gate = FindChildRecursive(wrapper, Phase2AkazaC18GateRigSourceName);
            if (gate != null)
            {
                if (gate.parent != context)
                {
                    gate.SetParent(context, worldPositionStays: false);
                }

                gate.localPosition = Vector3.zero;
                gate.localRotation = Quaternion.identity;
                gate.localScale = Vector3.one;
                gate.gameObject.SetActive(true);
                ApplyPhase2AkazaIntroSourceMaterials(gate.gameObject, Phase2AkazaC18GateRigSourceName);
                EditorUtility.SetDirty(gate.gameObject);
            }

            Material planeA = LoadOrCreateTransparentTexturedMaterial(
                Phase2AkazaIntroSourceMaterialRoot + "/M_C18_PlaneA.mat",
                new Color(0.1840398f, 0.6764706f, 0.6357177f, 0.5f),
                Phase2AkazaIntroSourceEfx02TexturePath);
            Material planeB = LoadOrCreateTransparentTexturedMaterial(
                Phase2AkazaIntroSourceMaterialRoot + "/M_C18_PlaneB.mat",
                new Color(0.9558824f, 0f, 0.17799227f, 0.503f),
                Phase2AkazaIntroSourceEfx02TexturePath);
            Material basePlane = LoadOrCreateTexturedMaterial(
                Phase2AkazaIntroSourceMaterialRoot + "/M_C18_GateBasePlane.mat",
                new Color(1f, 0f, 0f, 1f),
                string.Empty);

            EnsurePhase2AkazaC18SourcePlane(
                context,
                Phase2AkazaC18BasePlaneName,
                new Vector3(2.866898f, -0.028244466f, -3.577126f),
                new Quaternion(0.54943544f, -0.6933315f, -0.37067157f, 0.28286904f),
                Vector3.one,
                basePlane);
            EnsurePhase2AkazaC18SourcePlane(
                context,
                Phase2AkazaC18PlaneAName,
                new Vector3(0.992f, 0.597f, 1.296f),
                new Quaternion(-0.29463175f, -0.6740303f, 0.66110027f, -0.14772183f),
                new Vector3(0.024696488f, 0.52177036f, 0.048150294f),
                planeA);
            EnsurePhase2AkazaC18SourcePlane(
                context,
                Phase2AkazaC18PlaneBName,
                new Vector3(0.864f, 0.629f, 1.431f),
                new Quaternion(-0.17189156f, -0.65839034f, 0.7326153f, 0.01582208f),
                new Vector3(0.1065652f, 4.345954f, 0.004012611f),
                planeB);
            EnsurePhase2AkazaC18SourceLight(context);

            context.gameObject.SetActive(false);
            EditorUtility.SetDirty(context.gameObject);
        }

        private static void EnsurePhase2AkazaC18SourcePlane(
            Transform context,
            string planeName,
            Vector3 localPosition,
            Quaternion localRotation,
            Vector3 localScale,
            Material material)
        {
            Transform plane = EnsureChild(context, planeName);
            plane.localPosition = localPosition;
            plane.localRotation = localRotation;
            plane.localScale = localScale;
            plane.gameObject.SetActive(true);

            MeshFilter meshFilter = EnsureComponent<MeshFilter>(plane.gameObject);
            meshFilter.sharedMesh = LoadPrimitiveMesh(PrimitiveType.Plane);
            MeshRenderer renderer = EnsureComponent<MeshRenderer>(plane.gameObject);
            renderer.sharedMaterial = material;
            ConfigurePhase2AkazaRendererVisibility(renderer);
            EditorUtility.SetDirty(plane.gameObject);
        }

        private static void EnsurePhase2AkazaC18SourceLight(Transform context)
        {
            Transform lightTransform = EnsureChild(context, Phase2AkazaC18DirectionalLightName);
            lightTransform.localPosition = new Vector3(1.4886861f, 0.98707366f, 0.68515664f);
            lightTransform.localRotation =
                new Quaternion(-0.09145473f, 0.108552106f, 0.7794295f, -0.6101985f);
            lightTransform.localScale = Vector3.one;
            lightTransform.gameObject.SetActive(true);

            Light light = EnsureComponent<Light>(lightTransform.gameObject);
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.58823526f, 0.6137931f, 1f);
            light.intensity = 1.34f;
            light.bounceIntensity = 1.01f;
            light.cullingMask = ~0;
            EditorUtility.SetDirty(light);
            EditorUtility.SetDirty(lightTransform.gameObject);
        }

        private static string ResolvePhase2AkazaActorRigNameForCue(
            CinematicSequenceProfile.SourceActorAnimationCue cue)
        {
            return ResolvePhase2AkazaActorRigNameForSourceAsset(AssetDatabase.GetAssetPath(cue.Clip));
        }

        private static string ResolvePhase2AkazaActorRigNameForSourceClip(TimelineSourceClip sourceClip)
        {
            return ResolvePhase2AkazaActorRigNameForSourceAsset(sourceClip.AssetPath);
        }

        private static string ResolvePhase2AkazaActorRigNameForSourceAsset(string assetPath)
        {
            return Path.GetFileNameWithoutExtension(assetPath);
        }

        private static string ResolvePhase2AkazaModelPrefabPathForSourceClip(TimelineSourceClip sourceClip)
        {
            string rigName = ResolvePhase2AkazaActorRigNameForSourceClip(sourceClip);
            if (string.Equals(rigName, Phase2AkazaC18GateRigSourceName, StringComparison.OrdinalIgnoreCase))
            {
                return Phase2AkazaGateModelPath;
            }

            if (rigName.IndexOf("kohaku", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return Phase2AkazaKohakuModelPath;
            }

            if (string.Equals(sourceClip.AssetPath, Phase2AkazaC08ActorSourcePath, StringComparison.OrdinalIgnoreCase))
            {
                return Phase2AkazaC08ActorSourcePath;
            }

            if (rigName.IndexOf("akaza", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return sourceClip.AssetPath;
            }

            return sourceClip.AssetPath;
        }

        private static GameObject EnsurePhase2AkazaOriginalC23ActorRig(Transform wrapper)
        {
            if (wrapper == null)
            {
                throw new InvalidOperationException($"{Phase2AkazaC23CameraRigWrapperName} is missing a wrapper.");
            }

            Transform existingActor = wrapper.Find(Phase2AkazaC23ActorRigSourceName);
            GameObject sourceActor = existingActor != null ? existingActor.gameObject : null;
            if (sourceActor == null)
            {
                GameObject sourcePrefab = LoadAsset<GameObject>(Phase2AkazaModelPath);
                sourceActor = PrefabUtility.InstantiatePrefab(sourcePrefab, wrapper.gameObject.scene) as GameObject;
                if (sourceActor == null)
                {
                    throw new InvalidOperationException($"Could not instantiate {Phase2AkazaModelPath}.");
                }

                sourceActor.transform.SetParent(wrapper, worldPositionStays: false);
            }

            sourceActor.name = Phase2AkazaC23ActorRigSourceName;
            sourceActor.transform.localPosition = Vector3.zero;
            sourceActor.transform.localRotation = Quaternion.identity;
            sourceActor.transform.localScale = Vector3.one;

            Animator sourceAnimator = sourceActor.GetComponent<Animator>();
            if (sourceAnimator != null)
            {
                sourceAnimator.runtimeAnimatorController = null;
                sourceAnimator.applyRootMotion = false;
                sourceAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                EditorUtility.SetDirty(sourceAnimator);
            }

            ApplyPhase2AkazaMaterials(sourceActor);
            AnimationClip sourceActorClip = LoadPrimaryAnimationClip(Phase2AkazaC23ActorSourcePath);
            sourceActor.SetActive(true);
            if (sourceActorClip != null)
            {
                sourceActorClip.SampleAnimation(sourceActor, 0f);
            }

            sourceActor.SetActive(false);
            EditorUtility.SetDirty(sourceActor);
            return sourceActor;
        }

        private static void ConfigurePhase2AkazaIntroPlaybackLocks(
            SerializedObject serializedRunner,
            GameObject bossProxy,
            GameObject visual)
        {
            Behaviour[] playbackLocks =
            {
                RequireComponent<BossPressureCostLadder>(bossProxy, "phase2 Akaza intro cost ladder lock"),
                RequireComponent<BossPressureActionDirector>(bossProxy, "phase2 Akaza intro action director lock"),
                RequireComponent<BossPressurePositionController>(bossProxy, "phase2 Akaza intro position lock"),
                RequireComponent<BossBarrageEmitter>(bossProxy, "phase2 Akaza intro barrage lock"),
                RequireComponent<BossBasicFireEmitter>(bossProxy, "phase2 Akaza intro basic fire lock"),
                RequireComponent<BossSummonPressureAction>(bossProxy, "phase2 Akaza intro summon pressure lock"),
                RequireComponent<ActionFoundationArenaTransformMotion>(visual, "phase2 Akaza intro hover motion lock")
            };

            SerializedProperty locks = RequireProperty(serializedRunner, "behavioursDisabledDuringPlayback");
            locks.arraySize = playbackLocks.Length;
            for (int i = 0; i < playbackLocks.Length; i++)
            {
                locks.GetArrayElementAtIndex(i).objectReferenceValue = playbackLocks[i];
            }
        }

        private static void ConfigurePhase2AkazaIntroScreenFade(
            SerializedObject serializedRunner,
            Scene scene,
            Camera camera)
        {
            GameObject canvasObject = FindRoot(scene, Phase2AkazaC08ScreenFadeCanvasName);
            if (canvasObject != null)
            {
                UnityEngine.Object.DestroyImmediate(canvasObject);
            }

            RequireProperty(serializedRunner, "screenFadeCanvasGroup").objectReferenceValue = null;
            RequireProperty(serializedRunner, "screenFadeImage").objectReferenceValue = null;
        }

        private static void ConfigurePhase2AkazaIntroPlaylist(
            CinematicSequencePlaylistRunner playlist,
            CinematicSequenceRunner runner,
            CinematicSequenceProfile introProfile)
        {
            SerializedObject serializedPlaylist = new SerializedObject(playlist);
            RequireProperty(serializedPlaylist, "runner").objectReferenceValue = runner;
            RequireProperty(serializedPlaylist, "playOnStart").boolValue = false;
            RequireProperty(serializedPlaylist, "startDelaySeconds").floatValue = 0f;
            RequireProperty(serializedPlaylist, "loop").boolValue = false;

            SerializedProperty entries = RequireProperty(serializedPlaylist, "entries");
            entries.arraySize = 1;
            SerializedProperty entry = entries.GetArrayElementAtIndex(0);
            entry.FindPropertyRelative("profile").objectReferenceValue = introProfile;
            entry.FindPropertyRelative("delayAfterSeconds").floatValue = 0f;
            entry.FindPropertyRelative("usePlanarDirectionOverride").boolValue = true;
            entry.FindPropertyRelative("planarDirectionOverride").vector3Value = Vector3.back;
            serializedPlaylist.ApplyModifiedPropertiesWithoutUndo();
            SetBehaviourEnabled(playlist, true);
        }

        private static void ConfigurePhase2AkazaIntroSequenceBridge(
            ActionCinematicSequenceBridge sequenceBridge,
            CinematicSequenceRunner runner,
            CinematicSequenceProfile introProfile)
        {
            SetObjectReference(sequenceBridge, "runner", runner);
            SetBool(sequenceBridge, "blockLegacyCameraShotsWhenPlayed", true);
            SetBool(sequenceBridge, "blockLegacySignalsWhenPlayed", true);
            SetFloat(sequenceBridge, "minimumLockSeconds", 0.12f);
            SetObjectReference(sequenceBridge, "bossIntroProfile", introProfile);
            SetBehaviourEnabled(sequenceBridge, true);
        }

        private static void ConfigurePhase2AkazaIntroCueDirector(
            ActionCinematicCueDirector cueDirector,
            ActionCameraController cameraController,
            ActionCinematicSequenceBridge sequenceBridge,
            Transform playerTransform)
        {
            SetBehaviourEnabled(cueDirector, true);
            SetObjectReference(
                cueDirector,
                "cueProfile",
                ActionFoundationProfileSetup.EnsureCinematicCueProfileAsset());
            SetObjectReference(cueDirector, "cameraController", cameraController);
            SetObjectReference(cueDirector, "cueSpace", playerTransform);
            if (playerTransform != null)
            {
                SetObjectReference(
                    cueDirector,
                    "movement",
                    playerTransform.GetComponent<PlayerMovementController>());
                SetObjectReference(
                    cueDirector,
                    "actionController",
                    playerTransform.GetComponent<PlayerActionController>());
                SetObjectReference(
                    cueDirector,
                    "skill1Action",
                    playerTransform.GetComponent<PlayerSkill1Action>());
                SetObjectReference(
                    cueDirector,
                    "summonSlot1Action",
                    playerTransform.GetComponent<PlayerSummonSlot1Action>());
                SetObjectReference(
                    cueDirector,
                    "rangedBasicAttackAction",
                    playerTransform.GetComponent<PlayerRangedBasicAttackAction>());
                SetObjectReference(
                    cueDirector,
                    "cuePlayer",
                    playerTransform.GetComponent<CombatVfxCuePlayer>());
                SetObjectReference(cueDirector, "vfxAnchor", playerTransform);
                Animator playerAnimator = playerTransform.GetComponentInChildren<Animator>(includeInactive: true);
                SetObjectReference(cueDirector, "cueAnimator", playerAnimator);
            }

            SetObjectReference(cueDirector, "sequenceBridge", sequenceBridge);
            SetBool(cueDirector, "allowCuePlayback", true);
            SetBool(cueDirector, "allowSequenceBridgePlayback", true);
            SetBool(cueDirector, "useUnscaledClock", true);
            SetBool(cueDirector, "drawCinematicBars", true);
        }

        private static void ConfigurePhase2AkazaIntroAutoPlay(
            GameObject cameraObject,
            ActionCinematicCueDirector cueDirector)
        {
            ActionCinematicCueAutoPlay autoPlay = EnsureComponent<ActionCinematicCueAutoPlay>(cameraObject);
            SerializedObject serializedAutoPlay = new SerializedObject(autoPlay);
            RequireProperty(serializedAutoPlay, "cueDirector").objectReferenceValue = cueDirector;
            RequireProperty(serializedAutoPlay, "playOnStart").boolValue = true;
            RequireProperty(serializedAutoPlay, "cueKind").enumValueIndex =
                (int)ActionCinematicCueProfile.CueKind.BossIntro;
            RequireProperty(serializedAutoPlay, "tier").intValue = 1;
            RequireProperty(serializedAutoPlay, "usePlanarDirectionOverride").boolValue = true;
            RequireProperty(serializedAutoPlay, "planarDirectionOverride").vector3Value = Vector3.back;
            serializedAutoPlay.ApplyModifiedPropertiesWithoutUndo();
            SetBehaviourEnabled(autoPlay, true);
            EditorUtility.SetDirty(autoPlay);
        }

        private static void ValidatePhase2AkazaBossIntro(
            Scene scene,
            GameObject visual,
            Animator animator)
        {
            CinematicSequenceProfile introProfile =
                LoadAsset<CinematicSequenceProfile>(Phase2AkazaIntroProfilePath);
            ValidateGameOwnedAsset(introProfile, "phase2 Akaza boss intro profile");
            List<string> issues = new List<string>();
            introProfile.CollectValidationIssues(issues);
            if (issues.Count > 0)
            {
                throw new InvalidOperationException(
                    $"{introProfile.name} validation failed: {string.Join("; ", issues)}");
            }

            if (introProfile.Category != CinematicSequenceProfile.SequenceCategory.BossIntro)
            {
                throw new InvalidOperationException($"{introProfile.name} must stay a BossIntro profile.");
            }

            ValidatePhase2AkazaTimelineSourceCueProfile(introProfile);

            if (introProfile.CameraCues.Length != 0 || introProfile.ActorCues.Length < 2)
            {
                throw new InvalidOperationException(
                    $"{introProfile.name} must use source timeline composition and Akaza body handoff cues.");
            }

            ActionCameraController cameraController =
                RequireObject<ActionCameraController>(scene, "phase2 Akaza intro camera controller");
            Camera camera = cameraController.GetComponent<Camera>();
            if (camera == null)
            {
                throw new InvalidOperationException($"{cameraController.name} must keep a Camera component.");
            }

            CinematicSequenceRunner runner =
                RequireComponent<CinematicSequenceRunner>(cameraController.gameObject, "phase2 Akaza intro runner");
            CinematicSequencePlaylistRunner playlist =
                RequireComponent<CinematicSequencePlaylistRunner>(
                    cameraController.gameObject,
                    "phase2 Akaza intro playlist");
            ValidateBehaviourEnabled(runner, true);
            ValidateBehaviourEnabled(playlist, true);
            ValidateObjectReference(runner, "sequenceProfile", introProfile);
            ValidateObjectReference(runner, "bodyControllerOverride", null);
            ValidateObjectReference(runner, "cameraController", cameraController);
            ValidateObjectReference(runner, "cueSpace", visual.transform);
            ValidateObjectReference(runner, "cinematicCamera", camera);
            ValidateBool(runner, "driveCameraTransformFromProfile", true);
            ValidateBool(runner, "disableActionCameraControllerDuringPoseDrive", true);
            ValidatePhase2AkazaC08CombatLookSceneContext(scene);
            ValidatePhase2AkazaC08CombatLookPostProcess(scene, camera);
            ValidatePhase2AkazaSourceCameraRig(scene, runner, introProfile, visual.transform);
            ValidateRunnerActorBinding(
                runner,
                CinematicSequenceProfile.ActorRole.Boss,
                animator,
                visual.transform);
            ValidatePhase2AkazaIntroPlaybackLocks(runner, RequireRoot(scene, BossProxyRootName), visual);
            ValidatePhase2AkazaIntroScreenFade(scene, runner, camera);

            ValidateObjectReference(playlist, "runner", runner);
            ValidateBool(playlist, "playOnStart", false);
            ValidateFloat(playlist, "startDelaySeconds", 0f);
            ValidateBool(playlist, "loop", false);
            SerializedProperty entries = RequireProperty(new SerializedObject(playlist), "entries");
            if (entries.arraySize == 0)
            {
                throw new InvalidOperationException("Phase2 Akaza intro playlist must contain the intro profile.");
            }

            SerializedProperty firstEntry = entries.GetArrayElementAtIndex(0);
            if (firstEntry.FindPropertyRelative("profile").objectReferenceValue != introProfile)
            {
                throw new InvalidOperationException("Phase2 Akaza intro playlist first entry must be the intro profile.");
            }

            ValidatePhase2AkazaIntroCueBridge(cameraController, runner, introProfile);
        }

        private static void ValidatePhase2AkazaIntroCueBridge(
            ActionCameraController cameraController,
            CinematicSequenceRunner runner,
            CinematicSequenceProfile introProfile)
        {
            ActionCinematicSequenceBridge sequenceBridge =
                RequireComponent<ActionCinematicSequenceBridge>(
                    cameraController.gameObject,
                    "phase2 Akaza intro sequence bridge");
            ActionCinematicCueDirector cueDirector =
                RequireComponent<ActionCinematicCueDirector>(
                    cameraController.gameObject,
                    "phase2 Akaza intro cue director");
            ActionCinematicCueAutoPlay autoPlay =
                RequireComponent<ActionCinematicCueAutoPlay>(
                    cameraController.gameObject,
                    "phase2 Akaza intro autoplay trigger");

            ValidateBehaviourEnabled(sequenceBridge, true);
            ValidateObjectReference(sequenceBridge, "runner", runner);
            ValidateBool(sequenceBridge, "blockLegacyCameraShotsWhenPlayed", true);
            ValidateBool(sequenceBridge, "blockLegacySignalsWhenPlayed", true);
            ValidateObjectReference(sequenceBridge, "bossIntroProfile", introProfile);

            ValidateBehaviourEnabled(cueDirector, true);
            ValidateObjectReference(
                cueDirector,
                "cueProfile",
                LoadAsset<ActionCinematicCueProfile>(ActionFoundationProfileSetup.CinematicCueProfilePath));
            ValidateObjectReference(cueDirector, "cameraController", cameraController);
            ValidateObjectReference(cueDirector, "sequenceBridge", sequenceBridge);
            ValidateBool(cueDirector, "allowCuePlayback", true);
            ValidateBool(cueDirector, "allowSequenceBridgePlayback", true);

            ValidateBehaviourEnabled(autoPlay, true);
            ValidateObjectReference(autoPlay, "cueDirector", cueDirector);
            ValidateBool(autoPlay, "playOnStart", true);
            ValidateEnum(
                autoPlay,
                "cueKind",
                (int)ActionCinematicCueProfile.CueKind.BossIntro);
            ValidateInt(autoPlay, "tier", 1);
            ValidateBool(autoPlay, "usePlanarDirectionOverride", true);
        }

        private static void ValidatePhase2AkazaSourceCameraRig(
            Scene scene,
            CinematicSequenceRunner runner,
            CinematicSequenceProfile introProfile,
            Transform visualTransform)
        {
            GameObject wrapper = RequireRoot(scene, Phase2AkazaC23CameraRigWrapperName);
            GameObject sourceRig = RequireChild(wrapper.transform, Phase2AkazaC23CameraRigSourceName).gameObject;
            GameObject sourceActor = RequireChild(wrapper.transform, Phase2AkazaC23ActorRigSourceName).gameObject;
            TimelineSourceClip[] expectedSourceActorClips = BuildPhase2AkazaExpectedTimelineSourceActorClips();
            Dictionary<string, GameObject> sourceActorRigs = ResolvePhase2AkazaSourceActorRigs(
                wrapper.transform,
                expectedSourceActorClips);
            Camera sourceCamera = sourceRig.GetComponentInChildren<Camera>(includeInactive: true);
            if (sourceCamera == null)
            {
                throw new InvalidOperationException($"{Phase2AkazaC23CameraRigSourceName} must contain a source camera.");
            }

            Vector3 visualWorldPosition = visualTransform != null ? visualTransform.position : Vector3.zero;
            Vector3 expectedWrapperPosition = visualWorldPosition - Phase2AkazaOriginalC23ActorWorldPosition;
            if ((wrapper.transform.position - expectedWrapperPosition).sqrMagnitude > 0.0001f
                || Quaternion.Angle(wrapper.transform.rotation, Quaternion.identity) > 0.1f)
            {
                throw new InvalidOperationException(
                    $"{Phase2AkazaC23CameraRigWrapperName} must preserve the original C08 camera composition offset.");
            }

            if (sourceCamera.enabled)
            {
                throw new InvalidOperationException($"{Phase2AkazaC23CameraRigSourceName} camera must stay disabled.");
            }

            if (sourceActor.activeSelf)
            {
                throw new InvalidOperationException($"{Phase2AkazaC23ActorRigSourceName} actor must stay hidden before playback.");
            }

            foreach (KeyValuePair<string, GameObject> sourceActorRig in sourceActorRigs)
            {
                if (sourceActorRig.Value.activeSelf)
                {
                    throw new InvalidOperationException(
                        $"{sourceActorRig.Key} source actor rig must stay hidden before playback.");
                }
            }

            ValidateObjectReference(runner, "sourceCameraRigRoot", sourceRig);
            ValidateObjectReference(runner, "sourceCameraTransform", sourceCamera.transform);
            ValidateObjectReference(runner, "sourceCameraComponent", sourceCamera);
            ValidatePhase2AkazaSourceCameraBindings(runner, wrapper.transform, introProfile);
            ValidateObjectReference(runner, "sourceActorRigRoot", sourceActor);
            ValidateObjectReference(runner, "sourceActorVisibilityRoot", sourceActor);
            ValidatePhase2AkazaSourceActorBindings(runner, wrapper.transform, introProfile);
            ValidateObjectReference(
                runner,
                "primaryActorRootHiddenDuringSourceActorAnimation",
                visualTransform.gameObject);
            List<string> sourceAssetIssues = new List<string>();
            ValidatePhase2AkazaExpectedTimelineSourceAssets(
                BuildPhase2AkazaExpectedTimelineCameraClips(),
                "camera",
                sourceAssetIssues);
            ValidatePhase2AkazaExpectedTimelineSourceAssets(
                expectedSourceActorClips,
                "source actor",
                sourceAssetIssues);
            ValidatePhase2AkazaC08SourceSceneContext(sourceActor);
            if (sourceAssetIssues.Count > 0)
            {
                throw new InvalidOperationException(
                    $"Phase2 Akaza source timeline assets are missing: {string.Join("; ", sourceAssetIssues)}");
            }

            ValidatePhase2AkazaTimelineFrameParity(scene);
        }

        private static void ValidatePhase2AkazaC08SourceSceneContext(GameObject sourceActor)
        {
            Transform context = RequireChild(sourceActor.transform, Phase2AkazaC08SourceSceneContextName);
            Transform backdrop = RequireChild(context, Phase2AkazaC08SkyBackdropName);
            MeshFilter backdropMesh = RequireComponent<MeshFilter>(
                backdrop.gameObject,
                "phase2 Akaza C08 source sky mesh");
            if (backdropMesh.sharedMesh == null)
            {
                throw new InvalidOperationException($"{Phase2AkazaC08SkyBackdropName} must keep a sky mesh.");
            }

            MeshRenderer backdropRenderer = RequireComponent<MeshRenderer>(
                backdrop.gameObject,
                "phase2 Akaza C08 source sky renderer");
            if (backdropRenderer.sharedMaterial == null)
            {
                throw new InvalidOperationException($"{Phase2AkazaC08SkyBackdropName} must keep a sky material.");
            }

            Texture skyTexture = backdropRenderer.sharedMaterial.HasProperty("_BaseMap")
                ? backdropRenderer.sharedMaterial.GetTexture("_BaseMap")
                : null;
            if (skyTexture == null && backdropRenderer.sharedMaterial.HasProperty("_MainTex"))
            {
                skyTexture = backdropRenderer.sharedMaterial.GetTexture("_MainTex");
            }

            if (skyTexture == null)
            {
                throw new InvalidOperationException($"{Phase2AkazaC08SkyBackdropName} must keep the C08 sky texture.");
            }

            string skyTexturePath = NormalizeAssetPath(AssetDatabase.GetAssetPath(skyTexture));
            if (!string.Equals(
                    skyTexturePath,
                    NormalizeAssetPath(Phase2AkazaC08OriginalSkyLoopTexturePath),
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"{Phase2AkazaC08SkyBackdropName} must use the original C08 unity_sora_loop texture.");
            }

            if (Vector3.Distance(backdrop.localPosition, new Vector3(-1f, 47.9f, 99.6f)) > 0.01f
                || Quaternion.Angle(backdrop.localRotation, Quaternion.Euler(69.1f, 0f, 180f)) > 0.1f
                || Vector3.Distance(backdrop.localScale, new Vector3(12.17f, 7.14f, 3.19f)) > 0.01f)
            {
                throw new InvalidOperationException(
                    $"{Phase2AkazaC08SkyBackdropName} must preserve the original C08 Plane transform.");
            }

            ValidateGameOwnedAsset(backdropRenderer.sharedMaterial, "phase2 Akaza C08 source sky material");
            ValidateGameOwnedAsset(skyTexture, "phase2 Akaza C08 source sky texture");

            Transform lightTransform = RequireChild(context, Phase2AkazaC08DirectionalLightName);
            Light sourceLight = RequireComponent<Light>(
                lightTransform.gameObject,
                "phase2 Akaza C08 source light");
            if (sourceLight.type != LightType.Directional || sourceLight.intensity < 1f)
            {
                throw new InvalidOperationException($"{Phase2AkazaC08DirectionalLightName} must preserve the C08 key light.");
            }
        }

        private static void ValidatePhase2AkazaC08CombatLookSceneContext(Scene scene)
        {
            GameObject contextObject = RequireRoot(scene, Phase2AkazaC08CombatLookSceneContextName);
            if (!contextObject.activeSelf)
            {
                throw new InvalidOperationException($"{Phase2AkazaC08CombatLookSceneContextName} must stay active.");
            }

            Transform backdrop = RequireChild(contextObject.transform, Phase2AkazaC08SkyBackdropName);
            MeshRenderer backdropRenderer = RequireComponent<MeshRenderer>(
                backdrop.gameObject,
                "phase2 Akaza C08 combat sky renderer");
            if (backdropRenderer.sharedMaterial == null)
            {
                throw new InvalidOperationException(
                    $"{Phase2AkazaC08CombatLookSceneContextName} must keep the C08 combat sky material.");
            }

            Texture skyTexture = backdropRenderer.sharedMaterial.HasProperty("_BaseMap")
                ? backdropRenderer.sharedMaterial.GetTexture("_BaseMap")
                : null;
            if (skyTexture == null && backdropRenderer.sharedMaterial.HasProperty("_MainTex"))
            {
                skyTexture = backdropRenderer.sharedMaterial.GetTexture("_MainTex");
            }

            string skyTexturePath = skyTexture != null
                ? NormalizeAssetPath(AssetDatabase.GetAssetPath(skyTexture))
                : string.Empty;
            if (!string.Equals(
                    skyTexturePath,
                    NormalizeAssetPath(Phase2AkazaC08OriginalSkyLoopTexturePath),
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"{Phase2AkazaC08CombatLookSceneContextName} must use the C08 sky loop texture.");
            }

            Transform lightTransform = RequireChild(contextObject.transform, Phase2AkazaC08DirectionalLightName);
            Light light = RequireComponent<Light>(
                lightTransform.gameObject,
                "phase2 Akaza C08 combat key light");
            if (light.type != LightType.Directional
                || light.intensity < 1f
                || light.color.r < light.color.b)
            {
                throw new InvalidOperationException(
                    $"{Phase2AkazaC08CombatLookSceneContextName} must keep the warm C08 key light.");
            }

            ValidateGameOwnedAsset(backdropRenderer.sharedMaterial, "phase2 Akaza C08 combat sky material");
            ValidateGameOwnedAsset(skyTexture, "phase2 Akaza C08 combat sky texture");
        }

        private static void ValidatePhase2AkazaC08CombatLookPostProcess(Scene scene, Camera camera)
        {
            UniversalAdditionalCameraData cameraData = camera.GetUniversalAdditionalCameraData();
            if (!cameraData.renderPostProcessing)
            {
                throw new InvalidOperationException($"{camera.name} must render the C08 combat look post-process stack.");
            }

            GameObject postProcessObject = RequireRoot(scene, Phase2AkazaC08CombatLookPostProcessName);
            if (!postProcessObject.activeSelf)
            {
                throw new InvalidOperationException($"{Phase2AkazaC08CombatLookPostProcessName} must stay active.");
            }

            if ((cameraData.volumeLayerMask.value & (1 << postProcessObject.layer)) == 0)
            {
                throw new InvalidOperationException(
                    $"{camera.name} must include {Phase2AkazaC08CombatLookPostProcessName}'s layer in its volume mask.");
            }

            Volume volume = RequireComponent<Volume>(
                postProcessObject,
                "phase2 Akaza C08 combat look post-process");
            VolumeProfile profile = LoadAsset<VolumeProfile>(Phase2AkazaC08PostProcessProfilePath);
            ValidateGameOwnedAsset(profile, "phase2 Akaza C08 combat look profile");
            if (!volume.enabled
                || !volume.isGlobal
                || volume.weight < 0.999f
                || volume.priority < 121f
                || volume.sharedProfile != profile)
            {
                throw new InvalidOperationException(
                    $"{Phase2AkazaC08CombatLookPostProcessName} must be an active global Volume using the C08 source profile.");
            }
        }

        private static void ValidatePhase2AkazaTimelineSourceCueProfile(
            CinematicSequenceProfile introProfile)
        {
            List<string> issues = new List<string>();
            ValidatePhase2AkazaTimelineCueArray(
                "source camera",
                introProfile.SourceCameraAnimations,
                BuildPhase2AkazaExpectedTimelineCameraClips(),
                issues);
            ValidatePhase2AkazaTimelineCueArray(
                "source actor",
                introProfile.SourceActorAnimations,
                BuildPhase2AkazaExpectedTimelineSourceActorClips(),
                issues);
            CinematicSequenceProfile.SourceActorGradeCue[] grades = introProfile.SourceActorGrades;
            for (int i = 0; i < grades.Length; i++)
            {
                if (grades[i].Enabled
                    && string.Equals(
                        grades[i].CueId,
                        "akaza_intro_timeline_actor_C08_akaza",
                        StringComparison.Ordinal))
                {
                    issues.Add("C08 source actor grade must stay disabled; the source UCTS shader and light/post stack own the Akaza silhouette.");
                }
            }

            CinematicSequenceProfile.ScreenFadeCue[] screenFades = introProfile.ScreenFadeCues;
            for (int i = 0; i < screenFades.Length; i++)
            {
                if (screenFades[i].Enabled
                    && string.Equals(
                        screenFades[i].CueId,
                        "akaza_intro_timeline_fader_C08_fade_out",
                        StringComparison.Ordinal))
                {
                    issues.Add("C08 source screen fade must stay disabled during the sampled 1412-1562 window.");
                }
            }

            if (issues.Count > 0)
            {
                throw new InvalidOperationException(
                    $"{introProfile.name} source timeline cues are not faithful: "
                    + string.Join("; ", issues));
            }
        }

        private static void ValidatePhase2AkazaSourceCameraBindings(
            CinematicSequenceRunner runner,
            Transform wrapper,
            CinematicSequenceProfile introProfile)
        {
            SerializedProperty bindings = RequireProperty(new SerializedObject(runner), "sourceCameraBindings");
            CinematicSequenceProfile.SourceCameraAnimationCue[] cues = introProfile.SourceCameraAnimations;
            if (bindings.arraySize != cues.Length)
            {
                throw new InvalidOperationException(
                    $"Phase2 Akaza runner sourceCameraBindings expected {cues.Length}, found {bindings.arraySize}.");
            }

            for (int i = 0; i < cues.Length; i++)
            {
                SerializedProperty binding = bindings.GetArrayElementAtIndex(i);
                string rigName = ResolvePhase2AkazaCameraRigNameForCue(cues[i]);
                Transform rig = wrapper.Find(rigName);
                if (rig == null)
                {
                    throw new InvalidOperationException($"Missing source camera rig {rigName}.");
                }

                Camera camera = rig.GetComponentInChildren<Camera>(includeInactive: true);
                if (camera == null)
                {
                    throw new InvalidOperationException($"{rigName} must expose a Camera.");
                }

                if (camera.enabled)
                {
                    throw new InvalidOperationException($"{rigName} source Camera component must stay disabled.");
                }

                if (binding.FindPropertyRelative("cueId").stringValue != cues[i].CueId
                    || binding.FindPropertyRelative("rigRoot").objectReferenceValue != rig.gameObject
                    || binding.FindPropertyRelative("cameraTransform").objectReferenceValue != camera.transform
                    || binding.FindPropertyRelative("cameraComponent").objectReferenceValue != camera)
                {
                    throw new InvalidOperationException(
                        $"sourceCameraBindings[{i}] must bind {cues[i].CueId} to {rigName}.");
                }
            }
        }

        private static void ValidatePhase2AkazaSourceActorBindings(
            CinematicSequenceRunner runner,
            Transform wrapper,
            CinematicSequenceProfile introProfile)
        {
            SerializedProperty bindings = RequireProperty(new SerializedObject(runner), "sourceActorBindings");
            CinematicSequenceProfile.SourceActorAnimationCue[] cues = introProfile.SourceActorAnimations;
            if (bindings.arraySize != cues.Length)
            {
                throw new InvalidOperationException(
                    $"Phase2 Akaza runner sourceActorBindings expected {cues.Length}, found {bindings.arraySize}.");
            }

            for (int i = 0; i < cues.Length; i++)
            {
                SerializedProperty binding = bindings.GetArrayElementAtIndex(i);
                string rigName = ResolvePhase2AkazaActorRigNameForCue(cues[i]);
                Transform rig = FindChildRecursive(wrapper, rigName);
                if (rig == null)
                {
                    throw new InvalidOperationException($"Missing source actor rig {rigName}.");
                }

                if (binding.FindPropertyRelative("cueId").stringValue != cues[i].CueId
                    || binding.FindPropertyRelative("rigRoot").objectReferenceValue != rig.gameObject
                    || binding.FindPropertyRelative("visibilityRoot").objectReferenceValue != rig.gameObject)
                {
                    throw new InvalidOperationException(
                        $"sourceActorBindings[{i}] must bind {cues[i].CueId} to {rigName}.");
                }
            }
        }

        private static void ValidatePhase2AkazaSourceCameraProjection(
            GameObject sourceRig,
            Camera sourceCamera,
            GameObject sourceActor,
            CinematicSequenceProfile introProfile)
        {
            if (sourceRig == null || sourceCamera == null || sourceActor == null || introProfile == null)
            {
                throw new InvalidOperationException("Phase2 Akaza source camera projection validation is missing inputs.");
            }

            CinematicSequenceProfile.SourceCameraAnimationCue sourceCameraCue = introProfile.SourceCameraAnimation;
            CinematicSequenceProfile.SourceActorAnimationCue sourceActorCue = introProfile.SourceActorAnimation;
            float[] sampleSeconds =
            {
                0f,
                Phase2AkazaOriginalC23CameraDurationSeconds * 0.5f,
                Phase2AkazaOriginalC23CameraDurationSeconds
            };

            bool sourceActorWasActive = sourceActor.activeSelf;
            sourceActor.SetActive(true);
            try
            {
                for (int i = 0; i < sampleSeconds.Length; i++)
                {
                    float sampleSecond = sampleSeconds[i];
                    sourceActorCue.Clip.SampleAnimation(sourceActor, sourceActorCue.ClipInSeconds + sampleSecond);
                    sourceCameraCue.Clip.SampleAnimation(sourceRig, sourceCameraCue.ClipInSeconds + sampleSecond);
                    Bounds bounds = CalculateRendererBounds(sourceActor);
                    Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(sourceCamera);
                    Vector3 viewportCenter = sourceCamera.WorldToViewportPoint(bounds.center);
                    if (!GeometryUtility.TestPlanesAABB(frustumPlanes, bounds))
                    {
                        throw new InvalidOperationException(
                            $"Phase2 Akaza original C08 camera sample {sampleSecond:0.000}s misses source Akaza bounds. "
                            + $"camera={sourceCamera.transform.position}, rotation={sourceCamera.transform.rotation}, "
                            + $"forward={sourceCamera.transform.forward}, viewport={viewportCenter}, "
                            + $"fov={sourceCamera.fieldOfView:0.0}, center={bounds.center}, size={bounds.size}.");
                    }

                    if (viewportCenter.z <= sourceCamera.nearClipPlane || viewportCenter.z >= sourceCamera.farClipPlane)
                    {
                        throw new InvalidOperationException(
                            $"Phase2 Akaza original C08 camera sample {sampleSecond:0.000}s depth is invalid: "
                            + $"{viewportCenter}.");
                    }
                }
            }
            finally
            {
                sourceActorCue.Clip.SampleAnimation(sourceActor, sourceActorCue.ClipInSeconds);
                sourceCameraCue.Clip.SampleAnimation(sourceRig, sourceCameraCue.ClipInSeconds);
                sourceActor.SetActive(sourceActorWasActive);
            }
        }

        private static void ValidatePhase2AkazaTimelineFrameParity(Scene scene)
        {
            CinematicSequenceProfile introProfile =
                LoadAsset<CinematicSequenceProfile>(Phase2AkazaIntroProfilePath);
            CinematicSequenceProfile.SourceCameraAnimationCue[] sourceCameraCues =
                introProfile.SourceCameraAnimations;
            CinematicSequenceProfile.SourceActorAnimationCue[] sourceActorCues =
                introProfile.SourceActorAnimations;

            double windowStartSeconds = Phase2AkazaIntroSourceStartFrame / (double)Phase2AkazaIntroSourceFrameRate;
            double windowEndSeconds = Phase2AkazaIntroSourceEndFrame / (double)Phase2AkazaIntroSourceFrameRate;
            double windowDurationSeconds = windowEndSeconds - windowStartSeconds;
            TimelineSourceClip[] expectedCameraClips = BuildPhase2AkazaExpectedTimelineCameraClips();
            TimelineSourceClip[] expectedSourceActorClips = BuildPhase2AkazaExpectedTimelineSourceActorClips();
            TimelineSourceClip[] expectedAkazaActorClips = BuildPhase2AkazaExpectedTimelineAkazaActorClips();

            List<string> issues = new List<string>();
            ValidatePhase2AkazaExpectedTimelineSourceAssets(expectedCameraClips, "camera", issues);
            ValidatePhase2AkazaExpectedTimelineSourceAssets(expectedSourceActorClips, "source actor", issues);
            ValidatePhase2AkazaTimelineCueArray(
                "source camera",
                sourceCameraCues,
                expectedCameraClips,
                issues);
            ValidatePhase2AkazaTimelineCueArray(
                "source actor",
                sourceActorCues,
                expectedSourceActorClips,
                issues);

            ValidatePhase2AkazaTimelineFrameCoverage(
                ToRuntimeSourceCues(sourceCameraCues),
                expectedCameraClips,
                "camera",
                issues);
            ValidatePhase2AkazaTimelineFrameCoverage(
                ToRuntimeSourceCues(sourceActorCues),
                expectedSourceActorClips,
                "source actor",
                issues);
            ValidatePhase2AkazaRunnerTimelineSampling(
                scene,
                introProfile,
                expectedCameraClips,
                expectedSourceActorClips,
                issues);

            LogPhase2AkazaTimelineParitySummary(
                scene,
                expectedCameraClips,
                expectedSourceActorClips,
                expectedAkazaActorClips);
            if (issues.Count > 0)
            {
                throw new InvalidOperationException(
                    $"{Phase2AkazaTimelineParityPrefix} source timeline frame parity failed: "
                    + string.Join(" | ", issues));
            }
        }

        private static TimelineSourceClip[] BuildPhase2AkazaExpectedTimelineCameraClips()
        {
            return new[]
            {
                new TimelineSourceClip(Phase2AkazaC08CameraSourcePath, 23.533333333333335d, 26.133333333333336d)
            };
        }

        private static TimelineSourceClip[] BuildPhase2AkazaExpectedTimelineSourceActorClips()
        {
            return new[]
            {
                new TimelineSourceClip(Phase2AkazaC08ActorSourcePath, 23.533333333333335d, 26.133333333333336d)
            };
        }

        private static TimelineSourceClip[] BuildPhase2AkazaExpectedTimelineAkazaActorClips()
        {
            return new[]
            {
                new TimelineSourceClip(Phase2AkazaC08ActorSourcePath, 23.533333333333335d, 26.133333333333336d)
            };
        }

        private static void ValidatePhase2AkazaExpectedTimelineSourceAssets(
            TimelineSourceClip[] expectedClips,
            string label,
            List<string> issues)
        {
            for (int i = 0; i < expectedClips.Length; i++)
            {
                TimelineSourceClip clip = expectedClips[i];
                if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(clip.AssetPath) == null)
                {
                    issues.Add(
                        $"{label} source clip {clip.AssetPath} from original timeline "
                        + $"{clip.TimelineStartSeconds:0.###}-{clip.TimelineEndSeconds:0.###}s is not promoted.");
                    continue;
                }

                AnimationClip animationClip = LoadPrimaryAnimationClip(clip.AssetPath);
                if (animationClip == null)
                {
                    issues.Add($"{label} source clip {clip.AssetPath} has no usable AnimationClip.");
                }
            }
        }

        private static void ValidatePhase2AkazaTimelineCueArray(
            string label,
            CinematicSequenceProfile.SourceCameraAnimationCue[] cues,
            TimelineSourceClip[] expectedClips,
            List<string> issues)
        {
            if (cues.Length != expectedClips.Length)
            {
                issues.Add($"{label} cue count {cues.Length} does not match original {expectedClips.Length} clips.");
                return;
            }

            for (int i = 0; i < expectedClips.Length; i++)
            {
                ValidatePhase2AkazaTimelineCue(
                    label,
                    cues[i].Enabled,
                    cues[i].Clip,
                    cues[i].StartSeconds,
                    cues[i].ClipInSeconds,
                    cues[i].DurationSeconds,
                    expectedClips[i],
                    issues);
            }
        }

        private static void ValidatePhase2AkazaTimelineCueArray(
            string label,
            CinematicSequenceProfile.SourceActorAnimationCue[] cues,
            TimelineSourceClip[] expectedClips,
            List<string> issues)
        {
            if (cues.Length != expectedClips.Length)
            {
                issues.Add($"{label} cue count {cues.Length} does not match original {expectedClips.Length} clips.");
                return;
            }

            for (int i = 0; i < expectedClips.Length; i++)
            {
                ValidatePhase2AkazaTimelineCue(
                    label,
                    cues[i].Enabled,
                    cues[i].Clip,
                    cues[i].StartSeconds,
                    cues[i].ClipInSeconds,
                    cues[i].DurationSeconds,
                    expectedClips[i],
                    issues);
            }
        }

        private static void ValidatePhase2AkazaTimelineCue(
            string label,
            bool cueEnabled,
            AnimationClip cueClip,
            float cueStartSeconds,
            float cueClipInSeconds,
            float cueDurationSeconds,
            TimelineSourceClip expected,
            List<string> issues)
        {
            if (!cueEnabled || cueClip == null)
            {
                issues.Add($"{label} cue for {expected.AssetPath} is not enabled.");
                return;
            }

            ResolvePhase2AkazaTimelineWindow(
                expected,
                out float expectedSequenceStart,
                out float expectedClipIn,
                out float expectedDuration);
            string cuePath = NormalizeAssetPath(AssetDatabase.GetAssetPath(cueClip));
            if (!ApproximatelyEqual(cueStartSeconds, expectedSequenceStart)
                || !ApproximatelyEqual(cueClipInSeconds, expectedClipIn)
                || !ApproximatelyEqual(cueDurationSeconds, expectedDuration)
                || !string.Equals(cuePath, NormalizeAssetPath(expected.AssetPath), StringComparison.Ordinal))
            {
                issues.Add(
                    $"{label} cue maps {cuePath} start={cueStartSeconds:0.###}, "
                    + $"clipIn={cueClipInSeconds:0.###}, duration={cueDurationSeconds:0.###}; expected "
                    + $"{expected.AssetPath} at sequence {expectedSequenceStart:0.###}s, "
                    + $"clipIn={expectedClipIn:0.###}s for {expectedDuration:0.###}s.");
            }
        }

        private static void ValidatePhase2AkazaTimelineFrameCoverage(
            RuntimeSourceCue[] currentCues,
            TimelineSourceClip[] expectedClips,
            string label,
            List<string> issues)
        {
            int mismatchCount = 0;
            List<string> samples = new List<string>();
            List<TimelineSourceClip> expectedActiveClips = new List<TimelineSourceClip>();
            List<RuntimeSourceCue> currentActiveCues = new List<RuntimeSourceCue>();
            double windowStartSeconds = Phase2AkazaIntroSourceStartFrame / (double)Phase2AkazaIntroSourceFrameRate;
            const int MaxSamples = 8;

            for (int frame = Phase2AkazaIntroSourceStartFrame; frame <= Phase2AkazaIntroSourceEndFrame; frame++)
            {
                double timelineSecond = frame / (double)Phase2AkazaIntroSourceFrameRate;
                double sequenceSecond = timelineSecond - windowStartSeconds;
                CollectTimelineSourceClips(expectedClips, timelineSecond, expectedActiveClips);
                CollectRuntimeSourceCues(currentCues, sequenceSecond, currentActiveCues);

                if (TimelineSourceClipSetsMatch(
                        expectedActiveClips,
                        currentActiveCues,
                        timelineSecond,
                        sequenceSecond))
                {
                    continue;
                }

                mismatchCount++;
                if (samples.Count < MaxSamples)
                {
                    string expectedText = FormatTimelineSourceClipSet(expectedActiveClips, timelineSecond);
                    string currentText = FormatRuntimeSourceCueSet(currentActiveCues, sequenceSecond);
                    samples.Add(
                        $"frame {frame} timeline={timelineSecond:0.###}s expected {expectedText}, current {currentText}");
                }
            }

            if (mismatchCount > 0)
            {
                issues.Add(
                    $"{label} frame coverage mismatched on {mismatchCount}/"
                    + $"{Phase2AkazaIntroSourceEndFrame - Phase2AkazaIntroSourceStartFrame + 1} frames: "
                    + string.Join("; ", samples));
            }
        }

        private static void CollectTimelineSourceClips(
            TimelineSourceClip[] clips,
            double timelineSecond,
            List<TimelineSourceClip> results)
        {
            results.Clear();
            for (int i = 0; i < clips.Length; i++)
            {
                if (IsTimelineSecondWithinClip(clips[i], timelineSecond))
                {
                    results.Add(clips[i]);
                }
            }
        }

        private static void CollectRuntimeSourceCues(
            RuntimeSourceCue[] cues,
            double sequenceSecond,
            List<RuntimeSourceCue> results)
        {
            results.Clear();
            for (int i = 0; i < cues.Length; i++)
            {
                if (IsSequenceSecondWithinCue(cues[i], sequenceSecond))
                {
                    results.Add(cues[i]);
                }
            }
        }

        private static bool TimelineSourceClipSetsMatch(
            List<TimelineSourceClip> expectedClips,
            List<RuntimeSourceCue> currentCues,
            double timelineSecond,
            double sequenceSecond)
        {
            if (expectedClips.Count != currentCues.Count)
            {
                return false;
            }

            bool[] matched = new bool[currentCues.Count];
            for (int expectedIndex = 0; expectedIndex < expectedClips.Count; expectedIndex++)
            {
                TimelineSourceClip expected = expectedClips[expectedIndex];
                bool found = false;
                for (int currentIndex = 0; currentIndex < currentCues.Count; currentIndex++)
                {
                    if (matched[currentIndex])
                    {
                        continue;
                    }

                    RuntimeSourceCue current = currentCues[currentIndex];
                    double expectedLocalSeconds = timelineSecond - expected.TimelineStartSeconds;
                    double currentLocalSeconds = current.ClipInSeconds + sequenceSecond - current.StartSeconds;
                    if (string.Equals(
                            NormalizeAssetPath(current.AssetPath),
                            NormalizeAssetPath(expected.AssetPath),
                            StringComparison.Ordinal)
                        && Math.Abs(expectedLocalSeconds - currentLocalSeconds) <= 0.0006d)
                    {
                        matched[currentIndex] = true;
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    return false;
                }
            }

            return true;
        }

        private static string FormatTimelineSourceClipSet(
            List<TimelineSourceClip> clips,
            double timelineSecond)
        {
            if (clips.Count == 0)
            {
                return "no authored clip/hold";
            }

            List<string> parts = new List<string>(clips.Count);
            for (int i = 0; i < clips.Count; i++)
            {
                parts.Add($"{clips[i].AssetPath}@{timelineSecond - clips[i].TimelineStartSeconds:0.###}s");
            }

            return string.Join(", ", parts);
        }

        private static string FormatRuntimeSourceCueSet(
            List<RuntimeSourceCue> cues,
            double sequenceSecond)
        {
            if (cues.Count == 0)
            {
                return "no source cue";
            }

            List<string> parts = new List<string>(cues.Count);
            for (int i = 0; i < cues.Count; i++)
            {
                parts.Add(
                    $"{NormalizeAssetPath(cues[i].AssetPath)}@"
                    + $"{cues[i].ClipInSeconds + sequenceSecond - cues[i].StartSeconds:0.###}s");
            }

            return string.Join(", ", parts);
        }

        private static RuntimeSourceCue[] ToRuntimeSourceCues(
            CinematicSequenceProfile.SourceCameraAnimationCue[] cues)
        {
            RuntimeSourceCue[] results = new RuntimeSourceCue[cues.Length];
            for (int i = 0; i < cues.Length; i++)
            {
                results[i] = new RuntimeSourceCue(
                    AssetDatabase.GetAssetPath(cues[i].Clip),
                    cues[i].StartSeconds,
                    cues[i].ClipInSeconds,
                    cues[i].DurationSeconds);
            }

            return results;
        }

        private static RuntimeSourceCue[] ToRuntimeSourceCues(
            CinematicSequenceProfile.SourceActorAnimationCue[] cues)
        {
            RuntimeSourceCue[] results = new RuntimeSourceCue[cues.Length];
            for (int i = 0; i < cues.Length; i++)
            {
                results[i] = new RuntimeSourceCue(
                    AssetDatabase.GetAssetPath(cues[i].Clip),
                    cues[i].StartSeconds,
                    cues[i].ClipInSeconds,
                    cues[i].DurationSeconds);
            }

            return results;
        }

        private static bool TryFindRuntimeSourceCue(
            RuntimeSourceCue[] cues,
            double sequenceSecond,
            out RuntimeSourceCue result)
        {
            for (int i = 0; i < cues.Length; i++)
            {
                if (IsSequenceSecondWithinCue(cues[i], sequenceSecond))
                {
                    result = cues[i];
                    return true;
                }
            }

            result = default;
            return false;
        }

        private static bool IsSequenceSecondWithinCue(RuntimeSourceCue cue, double sequenceSecond)
        {
            const double Epsilon = 0.000001d;
            double sourceWindowDuration =
                (Phase2AkazaIntroSourceEndFrame - Phase2AkazaIntroSourceStartFrame)
                / (double)Phase2AkazaIntroSourceFrameRate;
            bool isFinalWindowFrame = Math.Abs(sequenceSecond - cue.EndSeconds) <= Epsilon
                && Math.Abs(cue.EndSeconds - sourceWindowDuration) <= Epsilon;
            return sequenceSecond + Epsilon >= cue.StartSeconds
                && (sequenceSecond < cue.EndSeconds - Epsilon || isFinalWindowFrame);
        }

        private static void ValidatePhase2AkazaRunnerTimelineSampling(
            Scene scene,
            CinematicSequenceProfile introProfile,
            TimelineSourceClip[] expectedCameraClips,
            TimelineSourceClip[] expectedSourceActorClips,
            List<string> issues)
        {
            ActionCameraController cameraController =
                RequireObject<ActionCameraController>(scene, "phase2 Akaza timeline parity camera controller");
            Camera mainCamera = cameraController.GetComponent<Camera>();
            CinematicSequenceRunner runner =
                RequireComponent<CinematicSequenceRunner>(
                    cameraController.gameObject,
                    "phase2 Akaza timeline parity runner");
            GameObject wrapper = RequireRoot(scene, Phase2AkazaC23CameraRigWrapperName);
            Dictionary<string, GameObject> sourceActorRigs = ResolvePhase2AkazaSourceActorRigs(
                wrapper.transform,
                expectedSourceActorClips);
            Dictionary<string, bool> originalSourceActorActiveStates =
                new Dictionary<string, bool>(StringComparer.Ordinal);
            foreach (KeyValuePair<string, GameObject> sourceActorRig in sourceActorRigs)
            {
                originalSourceActorActiveStates[sourceActorRig.Key] = sourceActorRig.Value.activeSelf;
            }

            Vector3 originalCameraPosition = mainCamera.transform.position;
            Quaternion originalCameraRotation = mainCamera.transform.rotation;
            float originalFieldOfView = mainCamera.fieldOfView;
            int cameraMismatchCount = 0;
            int actorVisibilityMismatchCount = 0;
            List<string> cameraSamples = new List<string>();
            List<string> actorSamples = new List<string>();
            const int MaxSamples = 8;

            try
            {
                double windowStartSeconds =
                    Phase2AkazaIntroSourceStartFrame / (double)Phase2AkazaIntroSourceFrameRate;
                for (int frame = Phase2AkazaIntroSourceStartFrame; frame <= Phase2AkazaIntroSourceEndFrame; frame++)
                {
                    double timelineSecond = frame / (double)Phase2AkazaIntroSourceFrameRate;
                    float sequenceSecond = (float)(timelineSecond - windowStartSeconds);
                    if (!runner.TryApplyProfileSampleForReview(introProfile, sequenceSecond, Vector3.back))
                    {
                        issues.Add($"runner refused review sample at source frame {frame}.");
                        return;
                    }

                    if (TryFindTimelineSourceClip(expectedCameraClips, timelineSecond, out TimelineSourceClip expectedCameraClip))
                    {
                        GameObject expectedRig =
                            RequireChild(
                                wrapper.transform,
                                Path.GetFileNameWithoutExtension(expectedCameraClip.AssetPath)).gameObject;
                        AnimationClip expectedClip = LoadPrimaryAnimationClip(expectedCameraClip.AssetPath);
                        expectedClip.SampleAnimation(
                            expectedRig,
                            (float)(timelineSecond - expectedCameraClip.TimelineStartSeconds));
                        Camera expectedCamera = expectedRig.GetComponentInChildren<Camera>(includeInactive: true);
                        float positionDelta =
                            Vector3.Distance(mainCamera.transform.position, expectedCamera.transform.position);
                        float angleDelta =
                            Quaternion.Angle(mainCamera.transform.rotation, expectedCamera.transform.rotation);
                        float fovDelta = Mathf.Abs(mainCamera.fieldOfView - expectedCamera.fieldOfView);
                        if (positionDelta > 0.01f || angleDelta > 0.1f || fovDelta > 0.05f)
                        {
                            cameraMismatchCount++;
                            if (cameraSamples.Count < MaxSamples)
                            {
                                cameraSamples.Add(
                                    $"frame {frame} expected {expectedCameraClip.AssetPath} "
                                    + $"posDelta={positionDelta:0.###}, angleDelta={angleDelta:0.###}, "
                                    + $"fovDelta={fovDelta:0.###}");
                            }
                        }
                    }

                    foreach (KeyValuePair<string, GameObject> sourceActorRig in sourceActorRigs)
                    {
                        bool expectedActorVisible = IsPhase2AkazaSourceActorRigActive(
                            expectedSourceActorClips,
                            sourceActorRig.Key,
                            timelineSecond);
                        if (sourceActorRig.Value.activeInHierarchy == expectedActorVisible)
                        {
                            continue;
                        }

                        actorVisibilityMismatchCount++;
                        if (actorSamples.Count < MaxSamples)
                        {
                            actorSamples.Add(
                                $"frame {frame} {sourceActorRig.Key} expectedActive={expectedActorVisible}, "
                                + $"actual={sourceActorRig.Value.activeInHierarchy}");
                        }
                    }
                }
            }
            finally
            {
                mainCamera.transform.SetPositionAndRotation(originalCameraPosition, originalCameraRotation);
                mainCamera.fieldOfView = originalFieldOfView;
                foreach (KeyValuePair<string, GameObject> sourceActorRig in sourceActorRigs)
                {
                    if (originalSourceActorActiveStates.TryGetValue(sourceActorRig.Key, out bool active))
                    {
                        sourceActorRig.Value.SetActive(active);
                    }
                }
            }

            if (cameraMismatchCount > 0)
            {
                issues.Add(
                    $"runner camera sampling mismatched on {cameraMismatchCount} frames: "
                    + string.Join("; ", cameraSamples));
            }

            if (actorVisibilityMismatchCount > 0)
            {
                issues.Add(
                    $"runner source actor visibility mismatched on {actorVisibilityMismatchCount} rig frames: "
                    + string.Join("; ", actorSamples));
            }
        }

        private static Dictionary<string, GameObject> ResolvePhase2AkazaSourceActorRigs(
            Transform wrapper,
            TimelineSourceClip[] expectedSourceActorClips)
        {
            Dictionary<string, GameObject> rigs = new Dictionary<string, GameObject>(StringComparer.Ordinal);
            for (int i = 0; i < expectedSourceActorClips.Length; i++)
            {
                string rigName = ResolvePhase2AkazaActorRigNameForSourceClip(expectedSourceActorClips[i]);
                if (rigs.ContainsKey(rigName))
                {
                    continue;
                }

                rigs.Add(rigName, RequireChildRecursive(wrapper, rigName).gameObject);
            }

            return rigs;
        }

        private static bool IsPhase2AkazaSourceActorRigActive(
            TimelineSourceClip[] expectedSourceActorClips,
            string rigName,
            double timelineSecond)
        {
            for (int i = 0; i < expectedSourceActorClips.Length; i++)
            {
                if (string.Equals(
                        ResolvePhase2AkazaActorRigNameForSourceClip(expectedSourceActorClips[i]),
                        rigName,
                        StringComparison.Ordinal)
                    && IsTimelineSecondWithinClip(expectedSourceActorClips[i], timelineSecond))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TryFindTimelineSourceClip(
            TimelineSourceClip[] clips,
            double timelineSecond,
            out TimelineSourceClip result)
        {
            for (int i = 0; i < clips.Length; i++)
            {
                if (IsTimelineSecondWithinClip(clips[i], timelineSecond))
                {
                    result = clips[i];
                    return true;
                }
            }

            result = default;
            return false;
        }

        private static bool IsTimelineSecondWithinClip(TimelineSourceClip clip, double timelineSecond)
        {
            const double Epsilon = 0.000001d;
            return timelineSecond + Epsilon >= clip.TimelineStartSeconds
                && timelineSecond < clip.TimelineEndSeconds - Epsilon;
        }

        private static void LogPhase2AkazaTimelineParitySummary(
            Scene scene,
            TimelineSourceClip[] expectedCameraClips,
            TimelineSourceClip[] expectedSourceActorClips,
            TimelineSourceClip[] expectedAkazaActorClips)
        {
            double windowStartSeconds = Phase2AkazaIntroSourceStartFrame / (double)Phase2AkazaIntroSourceFrameRate;
            double windowEndSeconds = Phase2AkazaIntroSourceEndFrame / (double)Phase2AkazaIntroSourceFrameRate;
            Debug.Log(
                $"{Phase2AkazaTimelineParityPrefix} scene={scene.path}, sourceWindow="
                + $"{Phase2AkazaIntroSourceStartFrame}-{Phase2AkazaIntroSourceEndFrame} "
                + $"({windowStartSeconds:0.###}-{windowEndSeconds:0.###}s), "
                + $"cameraClips={expectedCameraClips.Length}, "
                + $"sourceActorClips={expectedSourceActorClips.Length}, "
                + $"akazaActorClips={expectedAkazaActorClips.Length}");
        }

        private static string NormalizeAssetPath(string assetPath)
        {
            return string.IsNullOrWhiteSpace(assetPath)
                ? string.Empty
                : assetPath.Replace('\\', '/');
        }

        private static void ValidatePhase2AkazaIntroPlaybackLocks(
            CinematicSequenceRunner runner,
            GameObject bossProxy,
            GameObject visual)
        {
            UnityEngine.Object[] expectedLocks =
            {
                RequireComponent<BossPressureCostLadder>(bossProxy, "phase2 Akaza intro cost ladder lock"),
                RequireComponent<BossPressureActionDirector>(bossProxy, "phase2 Akaza intro action director lock"),
                RequireComponent<BossPressurePositionController>(bossProxy, "phase2 Akaza intro position lock"),
                RequireComponent<BossBarrageEmitter>(bossProxy, "phase2 Akaza intro barrage lock"),
                RequireComponent<BossBasicFireEmitter>(bossProxy, "phase2 Akaza intro basic fire lock"),
                RequireComponent<BossSummonPressureAction>(bossProxy, "phase2 Akaza intro summon pressure lock"),
                RequireComponent<ActionFoundationArenaTransformMotion>(visual, "phase2 Akaza intro hover motion lock")
            };
            SerializedProperty locks = RequireProperty(new SerializedObject(runner), "behavioursDisabledDuringPlayback");
            if (locks.arraySize != expectedLocks.Length)
            {
                throw new InvalidOperationException(
                    $"Phase2 Akaza intro runner must lock {expectedLocks.Length} gameplay behaviours, found {locks.arraySize}.");
            }

            for (int i = 0; i < expectedLocks.Length; i++)
            {
                UnityEngine.Object actual = locks.GetArrayElementAtIndex(i).objectReferenceValue;
                if (actual != expectedLocks[i])
                {
                    throw new InvalidOperationException(
                        $"Phase2 Akaza intro runner lock {i} expected {expectedLocks[i]}, found {actual}.");
                }
            }
        }

        private static void ValidatePhase2AkazaIntroScreenFade(
            Scene scene,
            CinematicSequenceRunner runner,
            Camera camera)
        {
            if (FindRoot(scene, Phase2AkazaC08ScreenFadeCanvasName) != null)
            {
                throw new InvalidOperationException(
                    "Phase2 Akaza C08 screen fade canvas is unused and must be removed from the review scene.");
            }

            SerializedObject serializedRunner = new SerializedObject(runner);
            if (RequireProperty(serializedRunner, "screenFadeCanvasGroup").objectReferenceValue != null
                || RequireProperty(serializedRunner, "screenFadeImage").objectReferenceValue != null)
            {
                throw new InvalidOperationException(
                    "Phase2 Akaza intro runner must not keep unused C08 screen fade references.");
            }
        }

        private static void ApplyPhase2AkazaMaterials(GameObject root)
        {
            ApplyPhase2AkazaIntroAkazaMaterials(root);
        }

        private static void ConfigurePhase2AkazaRendererVisibility(Renderer renderer)
        {
            renderer.enabled = true;
            renderer.forceRenderingOff = false;
            renderer.allowOcclusionWhenDynamic = false;
            renderer.shadowCastingMode = ShadowCastingMode.On;
            renderer.receiveShadows = true;

            if (renderer is SkinnedMeshRenderer skinnedMeshRenderer)
            {
                skinnedMeshRenderer.updateWhenOffscreen = true;
            }
        }

        private static void ApplyPhase2AkazaIntroAkazaMaterials(GameObject root)
        {
            EnsurePhase2AkazaC08CorrectedEyesTexture();

            Material skin = LoadOrCreateSourceToonMaterial(
                Phase2AkazaIntroSourceMaterialRoot + "/M_C08_Akaza_Skin.mat",
                Color.white,
                Phase2AkazaC08OriginalSkinTexturePath,
                Phase2AkazaC08OriginalSkinShadowTexturePath,
                string.Empty,
                string.Empty,
                0.5f,
                0.0001f,
                3.73f);
            ApplySourceToonMaterialOverrides(
                skin,
                Color.white,
                new Color(0.98f, 0.92f, 0.84f, 1f),
                new Color(0.82f, 0.64f, 0.54f, 1f),
                new Color(0.62f, 0.4f, 0.34f, 1f),
                0f,
                0f,
                0f,
                0f,
                1f,
                Phase2AkazaC08OriginalSkinTexturePath,
                string.Empty);
            ApplySourceToonOutlineOverrides(skin, 0.34f, 0.5f, 100f, 0f);
            Material face = LoadOrCreateSourceToonMaterial(
                Phase2AkazaIntroSourceMaterialRoot + "/M_C08_Akaza_Face.mat",
                Color.white,
                Phase2AkazaC08OriginalFaceTexturePath,
                Phase2AkazaC08OriginalFaceBTexturePath,
                Phase2AkazaC08OriginalFaceOutlineTexturePath,
                string.Empty,
                0.091f,
                0.005f,
                5.2f);
            ApplySourceToonMaterialOverrides(
                face,
                Color.white,
                new Color(1f, 0.96f, 0.9f, 1f),
                new Color(0.86f, 0.66f, 0.56f, 1f),
                new Color(0.64f, 0.43f, 0.36f, 1f),
                0f,
                -0.5f,
                0f,
                0.1f,
                0f,
                Phase2AkazaC08OriginalFaceTexturePath,
                string.Empty);
            ApplySourceToonOutlineOverrides(face, 0.3f, 0.5f, 100f, 0f);
            Material eyes = LoadOrCreateSourceToonMaterial(
                Phase2AkazaIntroSourceMaterialRoot + "/M_C08_Akaza_Eyes.mat",
                Color.white,
                Phase2AkazaC08CorrectedEyesTexturePath,
                Phase2AkazaC08OriginalFaceBTexturePath,
                Phase2AkazaC08OriginalFaceEyesOutlineTexturePath,
                Phase2AkazaC08OriginalFaceEyesSpowTexturePath,
                0.373f,
                0.0001f,
                5.2f);
            AssignTextureToMaterialProperty(eyes, "_BaseMap", Phase2AkazaC08CorrectedEyesTexturePath);
            ApplySourceToonMaterialOverrides(
                eyes,
                Color.white,
                Color.white,
                Color.white,
                new Color(0.6714965f, 0.6943085f, 0.8455882f, 1f),
                0.017f,
                -0.41f,
                0f,
                0.1f,
                1f,
                Phase2AkazaC08CorrectedEyesTexturePath,
                Phase2AkazaC08OriginalFaceBTexturePath);
            ApplySourceToonOutlineOverrides(eyes, 0.24f, 0.5f, 100f, 0f);
            Material faceHighlight = LoadOrCreateMaterial(
                Phase2AkazaIntroSourceMaterialRoot + "/M_C08_Akaza_FaceHighlight.mat",
                new Color(1f, 0.88f, 0.68f, 1f));
            SetMaterialFloatIfPresent(faceHighlight, "_Cull", 2f);
            SetMaterialFloatIfPresent(faceHighlight, "_CullMode", 2f);
            EditorUtility.SetDirty(faceHighlight);
            Material tooth = LoadOrCreateMaterial(
                Phase2AkazaIntroSourceMaterialRoot + "/M_C08_Akaza_Tooth.mat",
                new Color(0.96f, 0.9f, 0.82f, 1f));
            SetMaterialFloatIfPresent(tooth, "_Cull", 2f);
            SetMaterialFloatIfPresent(tooth, "_CullMode", 2f);
            EditorUtility.SetDirty(tooth);
            Material body = LoadOrCreateSourceToonMaterial(
                Phase2AkazaIntroSourceMaterialRoot + "/M_C08_Akaza_Body.mat",
                Color.white,
                Phase2AkazaC08OriginalBodyTexturePath,
                Phase2AkazaC08OriginalBodyShadowTexturePath,
                string.Empty,
                string.Empty,
                0.5f,
                0.0001f,
                7f);
            ApplySourceToonMaterialOverrides(
                body,
                Color.white,
                new Color(0.96f, 0.9f, 0.82f, 1f),
                new Color(0.78f, 0.62f, 0.52f, 1f),
                new Color(0.52f, 0.38f, 0.32f, 1f),
                0f,
                0f,
                0f,
                0f,
                1f,
                Phase2AkazaC08OriginalBodyTexturePath,
                string.Empty);
            ApplySourceToonOutlineOverrides(body, 0.32f, 0.5f, 100f, 0f);
            Material arm = LoadOrCreateSourceToonMaterial(
                Phase2AkazaIntroSourceMaterialRoot + "/M_C08_Akaza_Arm.mat",
                Color.white,
                Phase2AkazaC08OriginalArmTexturePath,
                Phase2AkazaC08OriginalArmShadowTexturePath,
                string.Empty,
                string.Empty,
                0.506f,
                0.0001f,
                7f);
            ApplySourceToonMaterialOverrides(
                arm,
                Color.white,
                new Color(0.96f, 0.82f, 0.58f, 1f),
                new Color(0.78f, 0.58f, 0.4f, 1f),
                new Color(0.54f, 0.36f, 0.24f, 1f),
                0f,
                0f,
                0.1f,
                0.1f,
                1f,
                Phase2AkazaC08OriginalArmTexturePath,
                string.Empty);
            ApplySourceToonHighlightOverrides(
                arm,
                new Color(0.625f, 0.5215518f, 0.390625f, 1f),
                1f);
            ApplySourceToonOutlineOverrides(arm, 0.3f, 0.5f, 100f, 0f);
            Material hair = LoadOrCreateSourceToonMaterial(
                Phase2AkazaIntroSourceMaterialRoot + "/M_C08_Akaza_Hair.mat",
                Color.white,
                Phase2AkazaC08OriginalHairTexturePath,
                Phase2AkazaC08OriginalHairBTexturePath,
                Phase2AkazaC08OriginalHairLpowTexturePath,
                Phase2AkazaC08OriginalHairSpowTexturePath,
                0.502f,
                0.034f,
                7f);
            ApplySourceToonMaterialOverrides(
                hair,
                Color.white,
                new Color(1f, 0.82f, 0.68f, 1f),
                new Color(0.96f, 0.48f, 0.58f, 1f),
                new Color(0.72f, 0.18f, 0.34f, 1f),
                0f,
                0f,
                0f,
                0.1f,
                1f,
                Phase2AkazaC08OriginalHairTexturePath,
                string.Empty);
            ApplySourceToonOutlineOverrides(hair, 0.28f, 0.5f, 100f, 0f);
            EditorUtility.SetDirty(hair);
            Material hairSpow = LoadOrCreateSourceToonMaterial(
                Phase2AkazaIntroSourceMaterialRoot + "/M_C08_Akaza_HairSpow.mat",
                Color.white,
                Phase2AkazaC08OriginalHairTexturePath,
                Phase2AkazaC08OriginalHairBTexturePath,
                Phase2AkazaC08OriginalHairLpowTexturePath,
                Phase2AkazaC08OriginalHairSpowTexturePath,
                0.502f,
                0.034f,
                7f);
            ApplySourceToonMaterialOverrides(
                hairSpow,
                Color.white,
                new Color(1f, 0.86f, 0.72f, 1f),
                new Color(0.98f, 0.56f, 0.64f, 1f),
                new Color(0.76f, 0.22f, 0.38f, 1f),
                0f,
                0f,
                0f,
                0.1f,
                1f,
                Phase2AkazaC08OriginalHairTexturePath,
                string.Empty);
            ApplySourceToonOutlineOverrides(hairSpow, 0.28f, 0.5f, 100f, 0f);
            EditorUtility.SetDirty(hairSpow);
            Material wire = LoadOrCreateSourceToonMaterial(
                Phase2AkazaIntroSourceMaterialRoot + "/M_C08_Akaza_Wire.mat",
                Color.white,
                Phase2AkazaC08OriginalArmTexturePath,
                Phase2AkazaC08OriginalArmShadowTexturePath,
                string.Empty,
                string.Empty,
                0.506f,
                0.0001f,
                0f);
            ApplySourceToonMaterialOverrides(
                wire,
                Color.white,
                new Color(0.96f, 0.82f, 0.58f, 1f),
                new Color(0.78f, 0.58f, 0.4f, 1f),
                new Color(0.54f, 0.36f, 0.24f, 1f),
                0f,
                0f,
                0.1f,
                0.1f,
                1f,
                Phase2AkazaC08OriginalArmTexturePath,
                string.Empty);
            Material defaultPlane = LoadOrCreateTexturedLitMaterial(
                Phase2AkazaIntroSourceMaterialRoot + "/M_C08_DefaultPlane.mat",
                Color.white,
                string.Empty);
            SetMaterialFloatIfPresent(defaultPlane, "_Cull", 0f);
            SetMaterialFloatIfPresent(defaultPlane, "_CullMode", 0f);
            EditorUtility.SetDirty(defaultPlane);

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(includeInactive: true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                ConfigurePhase2AkazaRendererVisibility(renderer);
                Material[] materials = renderer.sharedMaterials;
                bool hideDefaultPlaneRenderer = false;
                bool keepC08HeadShadowPlaneRenderer = false;
                for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                {
                    string key = ResolveMaterialKey(renderer, materials[materialIndex]);
                    bool defaultPlaneMaterial = IsPhase2AkazaDefaultPlaneMaterialKey(key)
                        && string.Equals(renderer.gameObject.name, "Plane", StringComparison.OrdinalIgnoreCase);
                    keepC08HeadShadowPlaneRenderer |= defaultPlaneMaterial
                        && IsPhase2AkazaC08HeadShadowPlane(renderer);
                    hideDefaultPlaneRenderer |= defaultPlaneMaterial
                        && !IsPhase2AkazaC08HeadShadowPlane(renderer);
                    materials[materialIndex] = ResolvePhase2AkazaIntroAkazaMaterial(
                        renderer,
                        key,
                        skin,
                        face,
                        eyes,
                        faceHighlight,
                        tooth,
                        body,
                        arm,
                        hair,
                        hairSpow,
                        wire,
                        defaultPlane);
                }

                renderer.sharedMaterials = materials;
                if (hideDefaultPlaneRenderer)
                {
                    renderer.enabled = false;
                    renderer.forceRenderingOff = true;
                    renderer.shadowCastingMode = ShadowCastingMode.Off;
                    renderer.receiveShadows = false;
                }
                else if (keepC08HeadShadowPlaneRenderer)
                {
                    renderer.enabled = true;
                    renderer.forceRenderingOff = false;
                    renderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
                    renderer.receiveShadows = false;
                }

                EditorUtility.SetDirty(renderer);
            }
        }

        private static Material ResolvePhase2AkazaIntroAkazaMaterial(
            Renderer renderer,
            string key,
            Material skin,
            Material face,
            Material eyes,
            Material faceHighlight,
            Material tooth,
            Material body,
            Material arm,
            Material hair,
            Material hairSpow,
            Material wire,
            Material defaultPlane)
        {
            if (IsPhase2AkazaDefaultPlaneMaterialKey(key))
            {
                return defaultPlane;
            }

            string rendererName = renderer != null ? renderer.gameObject.name.ToLowerInvariant() : string.Empty;
            int namespaceIndex = rendererName.LastIndexOf(':');
            if (namespaceIndex >= 0 && namespaceIndex + 1 < rendererName.Length)
            {
                rendererName = rendererName.Substring(namespaceIndex + 1);
            }

            if (rendererName == "eyeball")
            {
                return eyes;
            }

            if (rendererName == "eyehighlight")
            {
                return faceHighlight;
            }

            if (rendererName == "tooth")
            {
                return tooth;
            }

            if (rendererName == "head"
                || rendererName == "headnose"
                || rendererName == "eyeline"
                || rendererName == "mayu"
                || rendererName == "tongue")
            {
                return face;
            }

            if (rendererName == "skin"
                || rendererName == "tatoo")
            {
                return skin;
            }

            if (rendererName == "akamimi"
                || rendererName.StartsWith("hair", StringComparison.Ordinal))
            {
                return hairSpow;
            }

            if (rendererName.StartsWith("akarm", StringComparison.Ordinal)
                || rendererName.StartsWith("akwp", StringComparison.Ordinal))
            {
                return arm;
            }

            if (key.Contains("hairspow", StringComparison.Ordinal)
                || key.Contains("spow", StringComparison.Ordinal)
                || key.Contains("lpow", StringComparison.Ordinal))
            {
                return hairSpow;
            }

            if (key.Contains("hair", StringComparison.Ordinal)
                || key.Contains("mimi", StringComparison.Ordinal))
            {
                return hair;
            }

            if (key.Contains("eyeball", StringComparison.Ordinal))
            {
                return eyes;
            }

            if (key.Contains("highlight", StringComparison.Ordinal))
            {
                return faceHighlight;
            }

            if (key.Contains("face", StringComparison.Ordinal)
                || key.Contains("headnose", StringComparison.Ordinal)
                || key.Contains("mayu", StringComparison.Ordinal)
                || key.Contains("eye", StringComparison.Ordinal)
                || key.Contains("brow", StringComparison.Ordinal)
                || key.Contains("eyeline", StringComparison.Ordinal)
                || key.Contains("eyelid", StringComparison.Ordinal)
                || key.Contains("lip", StringComparison.Ordinal)
                || key.Contains("tongue", StringComparison.Ordinal)
                || key.Contains("jaw", StringComparison.Ordinal)
                || key.Contains("cheek", StringComparison.Ordinal))
            {
                return face;
            }

            if (key.Contains("tooth", StringComparison.Ordinal))
            {
                return tooth;
            }

            if (key.Contains("arm", StringComparison.Ordinal)
                || key.Contains("claw", StringComparison.Ordinal)
                || key.Contains("blade", StringComparison.Ordinal)
                || key.Contains("weapon", StringComparison.Ordinal)
                || key.Contains("wp_", StringComparison.Ordinal))
            {
                return arm;
            }

            if (key.Contains("body", StringComparison.Ordinal)
                || key.Contains("belt", StringComparison.Ordinal)
                || key.Contains("skirt", StringComparison.Ordinal)
                || key.Contains("pants", StringComparison.Ordinal)
                || key.Contains("boots", StringComparison.Ordinal)
                || key.Contains("tie", StringComparison.Ordinal)
                || key.Contains("flower", StringComparison.Ordinal)
                || key.Contains("backparts", StringComparison.Ordinal)
                || key.Contains("legguard", StringComparison.Ordinal)
                || key.Contains("wire", StringComparison.Ordinal))
            {
                return body;
            }

            if (key.Contains("skin", StringComparison.Ordinal)
                || key.Contains("hand", StringComparison.Ordinal)
                || key.Contains("leg", StringComparison.Ordinal)
                || key.Contains("head", StringComparison.Ordinal)
                || key.Contains("ear", StringComparison.Ordinal)
                || key.Contains("neck", StringComparison.Ordinal)
                || key.Contains("elbow", StringComparison.Ordinal)
                || key.Contains("knee", StringComparison.Ordinal)
                || key.Contains("calf", StringComparison.Ordinal)
                || key.Contains("clavicle", StringComparison.Ordinal)
                || key.Contains("tatoo", StringComparison.Ordinal))
            {
                return skin;
            }

            return wire;
        }

        private static void ApplySourceToonMaterialOverrides(
            Material material,
            Color baseColor,
            Color color,
            Color firstShadeColor,
            Color secondShadeColor,
            float secondShadeStep,
            float tweakSystemShadowsLevel,
            float highColorPower,
            float rimLightPower,
            float isLightColorSecondShade,
            string mainTexturePath,
            string secondShadeTexturePath)
        {
            SetMaterialColorIfPresent(material, "_BaseColor", baseColor);
            SetMaterialColorIfPresent(material, "_Color", color);
            SetMaterialColorIfPresent(material, "_1st_ShadeColor", firstShadeColor);
            SetMaterialColorIfPresent(material, "_2nd_ShadeColor", secondShadeColor);
            SetMaterialFloatIfPresent(material, "_2nd_ShadeColor_Step", secondShadeStep);
            SetMaterialFloatIfPresent(material, "_Tweak_SystemShadowsLevel", tweakSystemShadowsLevel);
            SetMaterialFloatIfPresent(material, "_HighColor_Power", highColorPower);
            SetMaterialFloatIfPresent(material, "_RimLight_Power", rimLightPower);
            SetMaterialFloatIfPresent(material, "_Ap_RimLight_Power", 0.1f);
            SetMaterialToggleKeywordIfPresent(
                material,
                "_Is_LightColor_2nd_Shade",
                isLightColorSecondShade,
                "_IS_LIGHTCOLOR_2ND_SHADE_ON");
            SetMaterialFloatIfPresent(material, "_GI_Intensity", 0f);
            AssignTextureToMaterialProperty(material, "_MainTex", mainTexturePath);
            AssignTextureToMaterialProperty(material, "_BaseMap", mainTexturePath);
            AssignTextureToMaterialProperty(material, "_2nd_ShadeMap", secondShadeTexturePath);
            EditorUtility.SetDirty(material);
        }

        private static void ApplySourceToonHighlightOverrides(
            Material material,
            Color highColor,
            float blendAddToHiColor)
        {
            SetMaterialColorIfPresent(material, "_HighColor", highColor);
            SetMaterialToggleKeywordIfPresent(
                material,
                "_Is_BlendAddToHiColor",
                blendAddToHiColor,
                "_IS_BLENDADDTOHICOLOR_ON");
            EditorUtility.SetDirty(material);
        }

        private static void ApplySourceToonOutlineOverrides(
            Material material,
            float outlineWidth,
            float nearestDistance,
            float farthestDistance,
            float offsetZ)
        {
            SetMaterialFloatIfPresent(material, "_Outline_Width", outlineWidth);
            SetMaterialFloatIfPresent(material, "_Nearest_Distance", nearestDistance);
            SetMaterialFloatIfPresent(material, "_Farthest_Distance", farthestDistance);
            SetMaterialFloatIfPresent(material, "_Offset_Z", offsetZ);
            SetMaterialFloatIfPresent(material, "_OutlineVisible", outlineWidth > 0f ? 1f : 0f);
            SetMaterialFloatIfPresent(material, "_OutlineOverridden", 0f);
            SetMaterialFloatIfPresent(material, "_OUTLINE", 0f);
            SetMaterialColorIfPresent(material, "_Outline_Color", new Color(0.065f, 0.044f, 0.035f, 1f));
            if (material != null)
            {
                material.EnableKeyword("_OUTLINE_NML");
                material.DisableKeyword("_OUTLINE_POS");
                material.DisableKeyword("_DISABLE_OUTLINE");
                EditorUtility.SetDirty(material);
            }
        }

        private static Material LoadOrCreateFlatSourceMaterial(string assetPath, Color color)
        {
            EnsureFolderForAsset(assetPath);
            Material material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            Shader shader = ResolveUnlitShader();
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, assetPath);
            }

            if (material.shader != shader)
            {
                material.shader = shader;
            }

            SetMaterialColorIfPresent(material, "_BaseColor", Color.white);
            SetMaterialColorIfPresent(material, "_Color", color);
            SetMaterialColorIfPresent(material, "_EmissionColor", Color.black);
            SetMaterialFloatIfPresent(material, "_Surface", 0f);
            SetMaterialFloatIfPresent(material, "_AlphaClip", 0f);
            SetMaterialFloatIfPresent(material, "_Cull", 2f);
            material.DisableKeyword("_ALPHATEST_ON");
            material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = -1;
            material.SetOverrideTag("RenderType", "Opaque");
            EditorUtility.SetDirty(material);
            return material;
        }

        private static bool IsPhase2AkazaDefaultPlaneMaterialKey(string key)
        {
            return key.Contains("default-material", StringComparison.Ordinal)
                || string.Equals(key.Trim(), "plane", StringComparison.Ordinal)
                || key.StartsWith("plane ", StringComparison.Ordinal);
        }

        private static bool IsPhase2AkazaC08HeadShadowPlane(Renderer renderer)
        {
            if (renderer == null)
            {
                return false;
            }

            string path = BuildPhase2AkazaTransformPath(renderer.transform);
            return path.IndexOf(":head_C/Plane", StringComparison.OrdinalIgnoreCase) >= 0
                || path.IndexOf("/head_C/Plane", StringComparison.OrdinalIgnoreCase) >= 0
                || path.IndexOf(":head_C/CHakazaA:Plane", StringComparison.OrdinalIgnoreCase) >= 0
                || path.IndexOf("/head_C/CHakazaA:Plane", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static void ApplyPhase2AkazaIntroSourceMaterials(GameObject root, string rigName)
        {
            if (rigName.IndexOf("gate", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                ApplyPhase2AkazaIntroGateMaterials(root);
                return;
            }

            if (rigName.IndexOf("kohaku", StringComparison.OrdinalIgnoreCase) < 0)
            {
                return;
            }

            Material skin = LoadOrCreateTexturedMaterial(
                Phase2AkazaIntroSourceMaterialRoot + "/M_C18_Source_Skin.mat",
                new Color(1f, 0.72f, 0.56f, 1f),
                Phase2AkazaIntroSourceSkinTexturePath);
            Material face = LoadOrCreateTexturedMaterial(
                Phase2AkazaIntroSourceMaterialRoot + "/M_C18_Source_Face.mat",
                new Color(1f, 0.78f, 0.62f, 1f),
                Phase2AkazaIntroSourceFaceTexturePath);
            Material body = LoadOrCreateTexturedMaterial(
                Phase2AkazaIntroSourceMaterialRoot + "/M_C18_Source_Body.mat",
                new Color(0.22f, 0.62f, 0.58f, 1f),
                Phase2AkazaIntroSourceBodyTexturePath);
            Material add = LoadOrCreateTexturedMaterial(
                Phase2AkazaIntroSourceMaterialRoot + "/M_C18_Source_Add.mat",
                new Color(0.95f, 0.36f, 0.48f, 1f),
                Phase2AkazaIntroSourceAddTexturePath);
            Material hair = LoadOrCreateTexturedMaterial(
                Phase2AkazaIntroSourceMaterialRoot + "/M_C18_Source_Hair.mat",
                new Color(1f, 0.42f, 0.58f, 1f),
                Phase2AkazaIntroSourceHairTexturePath);
            Material hairSpow = LoadOrCreateTexturedMaterial(
                Phase2AkazaIntroSourceMaterialRoot + "/M_C18_Source_HairSpow.mat",
                new Color(1f, 0.56f, 0.7f, 1f),
                Phase2AkazaIntroSourceHairSpowTexturePath);
            Material weapon = LoadOrCreateTexturedMaterial(
                Phase2AkazaIntroSourceMaterialRoot + "/M_C18_Source_Weapon.mat",
                new Color(0.97f, 0.78f, 0.58f, 1f),
                Phase2AkazaIntroSourceWeaponTexturePath);

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(includeInactive: true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                ConfigurePhase2AkazaRendererVisibility(renderer);
                Material[] materials = renderer.sharedMaterials;
                for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                {
                    string key = ResolveMaterialKey(renderer, materials[materialIndex]);
                    materials[materialIndex] = ResolvePhase2AkazaIntroSourceActorMaterial(
                        key,
                        skin,
                        face,
                        body,
                        add,
                        hair,
                        hairSpow,
                        weapon);
                }

                renderer.sharedMaterials = materials;
                EditorUtility.SetDirty(renderer);
            }
        }

        private static void ApplyPhase2AkazaIntroGateMaterials(GameObject root)
        {
            Material gate = LoadOrCreateTexturedMaterial(
                Phase2AkazaIntroSourceMaterialRoot + "/M_C18_Gate.mat",
                new Color(1f, 0.55f, 0.18f, 1f),
                Phase2AkazaIntroSourceGateTexturePath);
            SetMaterialFloatIfPresent(gate, "_Cull", 0f);

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(includeInactive: true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                ConfigurePhase2AkazaRendererVisibility(renderer);
                Material[] materials = renderer.sharedMaterials;
                for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                {
                    materials[materialIndex] = gate;
                }

                renderer.sharedMaterials = materials;
                EditorUtility.SetDirty(renderer);
            }
        }

        private static Material ResolvePhase2AkazaIntroSourceActorMaterial(
            string key,
            Material skin,
            Material face,
            Material body,
            Material add,
            Material hair,
            Material hairSpow,
            Material weapon)
        {
            if (key.Contains("face3", StringComparison.Ordinal))
            {
                return face;
            }

            if (key.Contains(":head", StringComparison.Ordinal)
                || key.Contains("headnose", StringComparison.Ordinal)
                || key.Contains("mayu", StringComparison.Ordinal)
                || key.Contains("brow", StringComparison.Ordinal)
                || key.Contains("tongue", StringComparison.Ordinal)
                || key.Contains("tooth", StringComparison.Ordinal))
            {
                return face;
            }

            if (key.Contains("unity2016_c_skin", StringComparison.Ordinal))
            {
                return skin;
            }

            if (key.Contains("unity2016_c_body", StringComparison.Ordinal))
            {
                return body;
            }

            if (key.Contains("unity2016_c_add", StringComparison.Ordinal))
            {
                return add;
            }

            if (key.Contains("unity2016_c_hair_spow", StringComparison.Ordinal))
            {
                return hairSpow;
            }

            if (key.Contains("unity2016_c_hair", StringComparison.Ordinal))
            {
                return hair;
            }

            if (key.Contains("unity2016_wep", StringComparison.Ordinal))
            {
                return weapon;
            }

            if (key.Contains("wep", StringComparison.Ordinal)
                || key.Contains("weapon", StringComparison.Ordinal)
                || key.Contains("blade", StringComparison.Ordinal)
                || key.Contains("sword", StringComparison.Ordinal))
            {
                return weapon;
            }

            if (key.Contains("face", StringComparison.Ordinal)
                || key.Contains("eye", StringComparison.Ordinal)
                || key.Contains("mouth", StringComparison.Ordinal))
            {
                return face;
            }

            if (key.Contains("hair", StringComparison.Ordinal))
            {
                return key.Contains("spow", StringComparison.Ordinal) ? hairSpow : hair;
            }

            if (key.Contains("skin", StringComparison.Ordinal)
                || key.Contains("hand", StringComparison.Ordinal)
                || key.Contains("arm", StringComparison.Ordinal)
                || key.Contains("leg", StringComparison.Ordinal)
                || key.Contains("neck", StringComparison.Ordinal))
            {
                return skin;
            }

            if (key.Contains("add", StringComparison.Ordinal)
                || key.Contains("ribbon", StringComparison.Ordinal)
                || key.Contains("horn", StringComparison.Ordinal)
                || key.Contains("gear", StringComparison.Ordinal)
                || key.Contains("headset", StringComparison.Ordinal))
            {
                return add;
            }

            if (key.Contains("body", StringComparison.Ordinal)
                || key.Contains("cloth", StringComparison.Ordinal)
                || key.Contains("costume", StringComparison.Ordinal)
                || key.Contains("skirt", StringComparison.Ordinal)
                || key.Contains("torso", StringComparison.Ordinal)
                || key.Contains("shirt", StringComparison.Ordinal)
                || key.Contains("coat", StringComparison.Ordinal))
            {
                return body;
            }

            return skin;
        }

        private static void EnsurePhase2AkazaAuraCore(Transform parent)
        {
            Transform aura = EnsureChild(parent, Phase2AkazaAuraName);
            aura.localPosition = new Vector3(0f, 1.1f, 0.16f);
            aura.localRotation = Quaternion.identity;
            aura.localScale = new Vector3(0.025f, 0.025f, 0.025f);

            MeshFilter meshFilter = EnsureComponent<MeshFilter>(aura.gameObject);
            meshFilter.sharedMesh = LoadPrimitiveMesh(PrimitiveType.Sphere);
            MeshRenderer renderer = EnsureComponent<MeshRenderer>(aura.gameObject);
            renderer.enabled = false;
            renderer.sharedMaterial = LoadOrCreateMaterial(
                Phase2AkazaAuraMaterialPath,
                new Color(0.1f, 0.58f, 0.76f, 0.24f));
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            ActionFoundationArenaFloatingShape floatingShape =
                EnsureComponent<ActionFoundationArenaFloatingShape>(aura.gameObject);
            floatingShape.Configure(
                new Vector3(0f, 54f, 0f),
                Vector3.up,
                0.024f,
                0.52f,
                0.26f,
                new Color(0.14f, 0.72f, 1f, 0.28f),
                new Color(0.2f, 0.9f, 1.18f, 0.58f),
                0.12f,
                0.68f);
        }

        private static void EnsurePhase2AkazaCombatCueClock(Transform parent)
        {
            Transform cueClock = EnsureChild(parent, Phase2AkazaCombatCueClockName);
            cueClock.localPosition = Vector3.zero;
            cueClock.localRotation = Quaternion.identity;
            cueClock.localScale = Vector3.one;
        }

        private static Material LoadOrCreateTexturedMaterial(string assetPath, Color color, string texturePath)
        {
            Material material = LoadOrCreateMaterial(assetPath, color);
            AssignTextureToMaterial(material, texturePath);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material LoadOrCreateTexturedLitMaterial(string assetPath, Color color, string texturePath)
        {
            EnsureFolderForAsset(assetPath);
            Material material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            Shader litShader = ResolveLitShader();
            if (material == null)
            {
                material = new Material(litShader);
                AssetDatabase.CreateAsset(material, assetPath);
            }

            if (material.shader != litShader)
            {
                material.shader = litShader;
            }

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }

            if (material.HasProperty("_EmissionColor"))
            {
                material.SetColor("_EmissionColor", Color.black);
            }

            SetMaterialFloatIfPresent(material, "_Metallic", 0f);
            SetMaterialFloatIfPresent(material, "_Smoothness", 0.18f);
            SetMaterialFloatIfPresent(material, "_Glossiness", 0.18f);
            SetMaterialFloatIfPresent(material, "_Surface", 0f);
            SetMaterialFloatIfPresent(material, "_Blend", 0f);
            SetMaterialFloatIfPresent(material, "_AlphaClip", 0f);
            SetMaterialFloatIfPresent(material, "_Cutoff", 0.5f);
            SetMaterialFloatIfPresent(material, "_Cull", 2f);
            material.DisableKeyword("_ALPHATEST_ON");
            material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = -1;
            material.SetOverrideTag("RenderType", "Opaque");
            AssignTextureToMaterial(material, texturePath);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material LoadOrCreateSourceToonMaterial(
            string assetPath,
            Color color,
            string baseTexturePath,
            string shadeTexturePath,
            string outlineTexturePath,
            string shadingGradeTexturePath,
            float shadeStep,
            float shadeFeather,
            float outlineWidth)
        {
            EnsureFolderForAsset(assetPath);
            Material material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            Shader shader = ResolveSourceToonShader();
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, assetPath);
            }

            if (material.shader != shader)
            {
                material.shader = shader;
            }

            SetMaterialColorIfPresent(material, "_BaseColor", color);
            SetMaterialColorIfPresent(material, "_Color", color);
            SetMaterialColorIfPresent(material, "_1st_ShadeColor", Color.white);
            SetMaterialColorIfPresent(material, "_2nd_ShadeColor", Color.white);
            SetMaterialColorIfPresent(material, "_EmissionColor", Color.black);
            SetMaterialColorIfPresent(material, "_Emissive_Color", Color.black);
            SetMaterialColorIfPresent(material, "_Outline_Color", Color.black);
            SetMaterialColorIfPresent(material, "_HighColor", Color.white);
            SetMaterialColorIfPresent(material, "_MatCapColor", Color.white);
            SetMaterialFloatIfPresent(material, "_Surface", 0f);
            SetMaterialFloatIfPresent(material, "_SurfaceType", 0f);
            SetMaterialFloatIfPresent(material, "_TransparentEnabled", 0f);
            SetMaterialFloatIfPresent(material, "_Blend", 0f);
            SetMaterialFloatIfPresent(material, "_Cull", 2f);
            SetMaterialFloatIfPresent(material, "_CullMode", 2f);
            SetMaterialFloatIfPresent(material, "_AlphaClip", 0f);
            SetMaterialFloatIfPresent(material, "_ClippingMode", 0f);
            SetMaterialFloatIfPresent(material, "_Cutoff", 0.5f);
            SetMaterialFloatIfPresent(material, "_Metallic", 0f);
            SetMaterialFloatIfPresent(material, "_Smoothness", 0.5f);
            SetMaterialFloatIfPresent(material, "_Glossiness", 0.5f);
            SetMaterialFloatIfPresent(material, "_BaseColor_Step", shadeStep);
            SetMaterialFloatIfPresent(material, "_BaseShade_Feather", shadeFeather);
            SetMaterialFloatIfPresent(material, "_1st_ShadeColor_Step", shadeStep);
            SetMaterialFloatIfPresent(material, "_1st_ShadeColor_Feather", shadeFeather);
            SetMaterialFloatIfPresent(material, "_1st2nd_Shades_Feather", 0.0001f);
            SetMaterialFloatIfPresent(material, "_2nd_ShadeColor_Step", 0f);
            SetMaterialFloatIfPresent(material, "_2nd_ShadeColor_Feather", 0.0001f);
            SetMaterialToggleKeywordIfPresent(
                material,
                "_Set_SystemShadowsToBase",
                1f,
                "_SET_SYSTEMSHADOWSTOBASE_ON");
            SetMaterialFloatIfPresent(material, "_Use_BaseAs1st", 0f);
            SetMaterialFloatIfPresent(material, "_Use_1stAs2nd", 0f);
            SetMaterialToggleKeywordIfPresent(
                material,
                "_Is_LightColor_Base",
                1f,
                "_IS_LIGHTCOLOR_BASE_ON");
            SetMaterialToggleKeywordIfPresent(
                material,
                "_Is_LightColor_1st_Shade",
                1f,
                "_IS_LIGHTCOLOR_1ST_SHADE_ON");
            SetMaterialToggleKeywordIfPresent(
                material,
                "_Is_LightColor_2nd_Shade",
                1f,
                "_IS_LIGHTCOLOR_2ND_SHADE_ON");
            SetMaterialFloatIfPresent(material, "_Is_BlendBaseColor", 0f);
            SetMaterialFloatIfPresent(material, "_Is_BrendBaseColor", 0f);
            SetMaterialToggleKeywordIfPresent(
                material,
                "_Is_BlendAddToHiColor",
                0f,
                "_IS_BLENDADDTOHICOLOR_ON");
            SetMaterialToggleKeywordIfPresent(
                material,
                "_Is_BlendAddToMatCap",
                1f,
                "_IS_BLENDADDTOMATCAP_ON");
            SetMaterialToggleKeywordIfPresent(
                material,
                "_Is_LightColor_Ap_RimLight",
                1f,
                "_IS_LIGHTCOLOR_AP_RIMLIGHT_ON");
            SetMaterialToggleKeywordIfPresent(
                material,
                "_Is_LightColor_HighColor",
                1f,
                "_IS_LIGHTCOLOR_HIGHCOLOR_ON");
            SetMaterialToggleKeywordIfPresent(
                material,
                "_Is_LightColor_MatCap",
                1f,
                "_IS_LIGHTCOLOR_MATCAP_ON");
            SetMaterialToggleKeywordIfPresent(
                material,
                "_Is_LightColor_RimLight",
                1f,
                "_IS_LIGHTCOLOR_RIMLIGHT_ON");
            SetMaterialFloatIfPresent(material, "_MatCap", 0f);
            SetMaterialFloatIfPresent(material, "_RimLight", 0f);
            SetMaterialFloatIfPresent(material, "_RimLight_Power", 0.1f);
            SetMaterialFloatIfPresent(material, "_Ap_RimLight_Power", 0.1f);
            SetMaterialFloatIfPresent(material, "_HighColor_Power", 0f);
            SetMaterialFloatIfPresent(material, "_GI_Intensity", 0f);
            SetMaterialFloatIfPresent(material, "_Unlit_Intensity", 1.2f);
            SetMaterialFloatIfPresent(material, "_Mode", 0f);
            SetMaterialFloatIfPresent(material, "_SrcBlend", 1f);
            SetMaterialFloatIfPresent(material, "_DstBlend", 0f);
            SetMaterialFloatIfPresent(material, "_AutoRenderQueue", 1f);
            SetMaterialFloatIfPresent(material, "_StencilMode", 0f);
            SetMaterialFloatIfPresent(material, "_StencilComp", 8f);
            SetMaterialFloatIfPresent(material, "_StencilNo", 1f);
            SetMaterialFloatIfPresent(material, "_StencilOpPass", 0f);
            SetMaterialFloatIfPresent(material, "_StencilOpFail", 0f);
            SetMaterialFloatIfPresent(material, "_SPRDefaultUnlitColorMask", 15f);
            SetMaterialFloatIfPresent(material, "_SRPDefaultUnlitColMode", 1f);
            SetMaterialFloatIfPresent(material, "_Tweak_ShadingGradeMapLevel", 0f);
            SetMaterialFloatIfPresent(material, "_BlurLevelSGM", 0f);
            SetMaterialFloatIfPresent(material, "_Outline_Width", outlineWidth);
            SetMaterialFloatIfPresent(material, "_OutlineVisible", outlineWidth > 0f ? 1f : 0f);
            SetMaterialFloatIfPresent(material, "_Is_LightColor_Outline", 0f);
            SetMaterialFloatIfPresent(material, "_Is_OutlineTex", 0f);
            SetMaterialFloatIfPresent(material, "_OUTLINE", 0f);
            SetMaterialFloatIfPresent(material, "_isUnityToonshader", 1f);
            SetMaterialFloatIfPresent(material, "_simpleUI", 0f);
            SetMaterialFloatIfPresent(
                material,
                "_utsTechnique",
                string.IsNullOrEmpty(shadingGradeTexturePath) ? 0f : 1f);
            SetMaterialFloatIfPresent(material, "_utsVersion", 2.075f);
            SetMaterialFloatIfPresent(material, "_utsVersionX", 0f);
            SetMaterialFloatIfPresent(material, "_utsVersionY", 10f);
            SetMaterialFloatIfPresent(material, "_utsVersionZ", 2f);
            SetMaterialFloatIfPresent(material, "_ZWrite", 1f);
            SetMaterialFloatIfPresent(material, "_ZWriteMode", 1f);
            AssignTextureToMaterial(material, baseTexturePath);
            AssignTextureToMaterialProperty(material, "_BaseMap", baseTexturePath);
            AssignTextureToMaterialProperty(material, "_MainTex", baseTexturePath);
            AssignTextureToMaterialProperty(material, "_1st_ShadeMap", shadeTexturePath);
            AssignTextureToMaterialProperty(material, "_ShadingGradeMap", shadingGradeTexturePath);
            AssignTextureToMaterialProperty(material, "_Outline_Sampler", outlineTexturePath);
            material.EnableKeyword("_EMISSIVE_SIMPLE");
            material.EnableKeyword("_OUTLINE_NML");
            if (string.IsNullOrEmpty(shadingGradeTexturePath))
            {
                material.DisableKeyword("_SHADINGGRADEMAP");
            }
            else
            {
                material.EnableKeyword("_SHADINGGRADEMAP");
            }

            material.DisableKeyword("_ALPHATEST_ON");
            material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.DisableKeyword("_DISABLE_OUTLINE");
            material.renderQueue = -1;
            material.SetOverrideTag("RenderType", "Opaque");
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material LoadOrCreateTransparentTexturedMaterial(
            string assetPath,
            Color color,
            string texturePath)
        {
            Material material = LoadOrCreateTransparentMaterial(assetPath, color);
            AssignTextureToMaterial(material, texturePath);
            SetMaterialFloatIfPresent(material, "_Cull", 0f);
            material.enableInstancing = true;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void AssignTextureToMaterial(Material material, string texturePath)
        {
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
        }

        private static void AssignTextureToMaterialProperty(
            Material material,
            string propertyName,
            string texturePath)
        {
            if (material == null
                || string.IsNullOrEmpty(propertyName)
                || string.IsNullOrEmpty(texturePath)
                || !material.HasProperty(propertyName))
            {
                return;
            }

            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            if (texture != null)
            {
                material.SetTexture(propertyName, texture);
            }
        }

        private static void SetMaterialToggleKeywordIfPresent(
            Material material,
            string propertyName,
            float value,
            string keyword)
        {
            SetMaterialFloatIfPresent(material, propertyName, value);
            if (material == null || string.IsNullOrEmpty(keyword))
            {
                return;
            }

            if (value > 0.5f)
            {
                material.EnableKeyword(keyword);
            }
            else
            {
                material.DisableKeyword(keyword);
            }
        }

        private static Shader ResolveLitShader()
        {
            return Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("Standard")
                ?? ResolveUnlitShader();
        }

        private static Shader ResolveSourceToonShader()
        {
            return Shader.Find("Universal Render Pipeline/Unity Toon Shader")
                ?? Shader.Find("Universal Render Pipeline/Toon")
                ?? Shader.Find("Universal Render Pipeline/Toon Lit")
                ?? Shader.Find("UnityChanToonShader/Toon_ShadingGradeMap")
                ?? Shader.Find("UnityChanToonShader/Toon_DoubleShadeWithFeather")
                ?? Shader.Find("Toon")
                ?? ResolveLitShader();
        }

        private static BossBarragePatternProfile LoadOrCreateBossBarragePatternProfile(string assetPath)
        {
            EnsureFolderForAsset(assetPath);
            BossBarragePatternProfile profile =
                AssetDatabase.LoadAssetAtPath<BossBarragePatternProfile>(assetPath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
                AssetDatabase.CreateAsset(profile, assetPath);
            }

            return profile;
        }

        private static void ConfigureAkazaSummonTier(
            SerializedProperty tier,
            float entryForwardBlend01,
            float lateralOffset,
            float entryHeight,
            float actorLifetimeSeconds,
            float actorScale,
            string actorRoleId,
            float actorMaxHealth,
            float actorMoveSpeed,
            float actorAdvanceDistance,
            float actorAdvanceSeconds,
            float actorEngageRadius,
            float actorAttackDamagePerSecond,
            float actorAttackIntervalSeconds,
            int screenIntercepts,
            float screenRadius,
            float screenLifetimeSeconds)
        {
            RequireRelativeProperty(tier, "EntryForwardBlend01").floatValue = entryForwardBlend01;
            RequireRelativeProperty(tier, "LateralOffset").floatValue = lateralOffset;
            RequireRelativeProperty(tier, "EntryHeight").floatValue = entryHeight;
            RequireRelativeProperty(tier, "ActorLifetimeSeconds").floatValue = actorLifetimeSeconds;
            RequireRelativeProperty(tier, "ActorScale").floatValue = actorScale;
            RequireRelativeProperty(tier, "ActorRoleId").stringValue = actorRoleId;
            RequireRelativeProperty(tier, "ActorMaxHealth").floatValue = actorMaxHealth;
            RequireRelativeProperty(tier, "ActorMoveSpeed").floatValue = actorMoveSpeed;
            RequireRelativeProperty(tier, "ActorAdvanceDistance").floatValue = actorAdvanceDistance;
            RequireRelativeProperty(tier, "ActorAdvanceSeconds").floatValue = actorAdvanceSeconds;
            RequireRelativeProperty(tier, "ActorEngageRadius").floatValue = actorEngageRadius;
            RequireRelativeProperty(tier, "ActorAttackDamagePerSecond").floatValue =
                actorAttackDamagePerSecond;
            RequireRelativeProperty(tier, "ActorAttackIntervalSeconds").floatValue =
                actorAttackIntervalSeconds;
            RequireRelativeProperty(tier, "ScreenIntercepts").intValue = screenIntercepts;
            RequireRelativeProperty(tier, "ScreenRadius").floatValue = screenRadius;
            RequireRelativeProperty(tier, "ScreenLifetimeSeconds").floatValue = screenLifetimeSeconds;
        }

        private static void ConfigureAkazaSummonReadout(
            SerializedProperty readout,
            string tierLabel,
            string stageRole,
            string playerRead,
            string summonRead)
        {
            RequireRelativeProperty(readout, "TierLabel").stringValue = tierLabel;
            RequireRelativeProperty(readout, "StageRole").stringValue = stageRole;
            RequireRelativeProperty(readout, "PlayerRead").stringValue = playerRead;
            RequireRelativeProperty(readout, "SummonRead").stringValue = summonRead;
        }

        private static void ConfigurePhase2AkazaActionSlot(
            SerializedProperty slot,
            BossBarragePatternProfile pattern,
            BossPressureActionKind actionKind,
            int minimumTier,
            int queuedWaves,
            float minimumIntervalSeconds,
            bool usePlayerForwardRiskGate,
            float minimumPlayerForwardRisk01,
            float maximumPlayerForwardRisk01,
            bool usePlayerSummonResponseGate,
            int minimumPlayerSummonTier,
            string responseId,
            string stageLoopRole,
            string playerAnswer,
            string summonAnswer)
        {
            RequireRelativeProperty(slot, "Pattern").objectReferenceValue = pattern;
            RequireRelativeProperty(slot, "ActionKind").enumValueIndex = (int)actionKind;
            RequireRelativeProperty(slot, "MinimumTier").intValue = minimumTier;
            RequireRelativeProperty(slot, "QueuedWaves").intValue = queuedWaves;
            RequireRelativeProperty(slot, "MinimumIntervalSeconds").floatValue = minimumIntervalSeconds;
            RequireRelativeProperty(slot, "UsePlayerForwardRiskGate").boolValue = usePlayerForwardRiskGate;
            RequireRelativeProperty(slot, "MinimumPlayerForwardRisk01").floatValue = minimumPlayerForwardRisk01;
            RequireRelativeProperty(slot, "MaximumPlayerForwardRisk01").floatValue = maximumPlayerForwardRisk01;
            RequireRelativeProperty(slot, "UsePlayerSummonResponseGate").boolValue = usePlayerSummonResponseGate;
            RequireRelativeProperty(slot, "MinimumPlayerSummonTier").intValue = minimumPlayerSummonTier;
            RequireRelativeProperty(slot, "ResponseId").stringValue = responseId;
            RequireRelativeProperty(slot, "StageLoopRole").stringValue = stageLoopRole;
            RequireRelativeProperty(slot, "PlayerAnswer").stringValue = playerAnswer;
            RequireRelativeProperty(slot, "SummonAnswer").stringValue = summonAnswer;
        }

        private static void ConfigurePhase2AkazaPatternCue(
            SerializedProperty cue,
            string patternId,
            string windupTrigger,
            string releaseTrigger,
            Color windupColor,
            Color releaseColor,
            float windupPulseScale,
            float releasePulseScale,
            CombatVfxCueId windupCueId,
            CombatVfxCueId releaseCueId)
        {
            RequireRelativeProperty(cue, "patternId").stringValue = patternId;
            RequireRelativeProperty(cue, "windupTrigger").stringValue = windupTrigger;
            RequireRelativeProperty(cue, "releaseTrigger").stringValue = releaseTrigger;
            RequireRelativeProperty(cue, "windupColor").colorValue = windupColor;
            RequireRelativeProperty(cue, "releaseColor").colorValue = releaseColor;
            RequireRelativeProperty(cue, "windupPulseScale").floatValue = windupPulseScale;
            RequireRelativeProperty(cue, "releasePulseScale").floatValue = releasePulseScale;
            RequireRelativeProperty(cue, "useWorldVfxCueOverride").boolValue = true;
            RequireRelativeProperty(cue, "windupWorldCueId").enumValueIndex = (int)windupCueId;
            RequireRelativeProperty(cue, "releaseWorldCueId").enumValueIndex = (int)releaseCueId;
            RequireRelativeProperty(cue, "windupWorldCueIntensity").floatValue = 1.12f;
            RequireRelativeProperty(cue, "releaseWorldCueIntensity").floatValue = 1.18f;
        }

        private static void ConfigurePhase2AkazaPressureCue(
            SerializedProperty cue,
            BossPressureActionKind actionKind,
            string trigger,
            Color color,
            float durationSeconds,
            float pulseScale,
            float tierPulseBonus)
        {
            RequireRelativeProperty(cue, "actionKind").enumValueIndex = (int)actionKind;
            RequireRelativeProperty(cue, "trigger").stringValue = trigger;
            RequireRelativeProperty(cue, "color").colorValue = color;
            RequireRelativeProperty(cue, "durationSeconds").floatValue = durationSeconds;
            RequireRelativeProperty(cue, "pulseScale").floatValue = pulseScale;
            RequireRelativeProperty(cue, "tierPulseBonus").floatValue = tierPulseBonus;
        }

        private static Renderer[] ResolveProjectileVisualRenderers(GameObject root)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(includeInactive: true);
            var results = new List<Renderer>(renderers.Length);
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null && renderers[i] is not TrailRenderer)
                {
                    results.Add(renderers[i]);
                }
            }

            return results.ToArray();
        }

        private static UnityEngine.Object[] ToObjectArray(UnityEngine.Object[] values)
        {
            return values != null ? values : Array.Empty<UnityEngine.Object>();
        }

        private static void ResetAnimatorController(AnimatorController controller)
        {
            if (controller == null)
            {
                throw new ArgumentNullException(nameof(controller));
            }

            AnimatorControllerParameter[] parameters = controller.parameters;
            for (int i = parameters.Length - 1; i >= 0; i--)
            {
                controller.RemoveParameter(parameters[i]);
            }

            AnimatorControllerLayer[] layers = controller.layers;
            if (layers == null || layers.Length == 0)
            {
                controller.AddLayer("Base Layer");
                layers = controller.layers;
            }

            AnimatorStateMachine stateMachine = layers[0].stateMachine;
            ChildAnimatorState[] states = stateMachine.states;
            for (int i = states.Length - 1; i >= 0; i--)
            {
                stateMachine.RemoveState(states[i].state);
            }

            ChildAnimatorStateMachine[] childMachines = stateMachine.stateMachines;
            for (int i = childMachines.Length - 1; i >= 0; i--)
            {
                stateMachine.RemoveStateMachine(childMachines[i].stateMachine);
            }

            AnimatorStateTransition[] anyStateTransitions = stateMachine.anyStateTransitions;
            for (int i = anyStateTransitions.Length - 1; i >= 0; i--)
            {
                stateMachine.RemoveAnyStateTransition(anyStateTransitions[i]);
            }

            AnimatorTransition[] entryTransitions = stateMachine.entryTransitions;
            for (int i = entryTransitions.Length - 1; i >= 0; i--)
            {
                stateMachine.RemoveEntryTransition(entryTransitions[i]);
            }
        }

        private static AnimationClip LoadPrimaryAnimationClip(string assetPath)
        {
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is AnimationClip clip && IsUsableAkazaClip(clip))
                {
                    return clip;
                }
            }

            return null;
        }

        private static AnimationClip EnsurePhase2AkazaInPlaceClip(
            string sourcePath,
            string destinationPath,
            string clipName)
        {
            AnimationClip sourceClip = LoadPrimaryAnimationClip(sourcePath);
            if (sourceClip == null)
            {
                return null;
            }

            EnsureFolderForAsset(destinationPath);
            AnimationClip inPlaceClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(destinationPath);
            bool createdClip = inPlaceClip == null;
            if (createdClip)
            {
                inPlaceClip = new AnimationClip();
            }

            EditorUtility.CopySerialized(sourceClip, inPlaceClip);
            inPlaceClip.name = clipName;
            int strippedCurveCount = StripPhase2AkazaCutscenePositionCurves(inPlaceClip);
            if (createdClip)
            {
                AssetDatabase.CreateAsset(inPlaceClip, destinationPath);
            }

            EditorUtility.SetDirty(inPlaceClip);
            Debug.Log(
                $"{Phase2AkazaPlayInspectPrefix} created {clipName} with {strippedCurveCount} cutscene position curves stripped.");
            return inPlaceClip;
        }

        private static void EnsurePhase2AkazaSourceReferenceClips()
        {
            EnsurePhase2AkazaInPlaceClip(
                Phase2AkazaC08ActorSourcePath,
                Phase2AkazaC23IntroClipPath,
                "DB_Akaza_C08_Intro1412_1562_InPlace");
            EnsurePhase2AkazaInPlaceClip(
                Phase2AkazaAnimationSourceRoot + "/C25_akaza.fbx",
                Phase2AkazaC25InPlaceClipPath,
                "DB_Akaza_C25_InPlace");
            EnsurePhase2AkazaInPlaceClip(
                Phase2AkazaAnimationSourceRoot + "/C27_akaza.fbx",
                Phase2AkazaC27InPlaceClipPath,
                "DB_Akaza_C27_InPlace");
            EnsurePhase2AkazaInPlaceClip(
                Phase2AkazaAnimationSourceRoot + "/C30_akaza.fbx",
                Phase2AkazaC30InPlaceClipPath,
                "DB_Akaza_C30_InPlace");
            EnsurePhase2AkazaInPlaceClip(
                Phase2AkazaAnimationSourceRoot + "/C34_Akaza.fbx",
                Phase2AkazaC34InPlaceClipPath,
                "DB_Akaza_C34_InPlace");
        }

        private static AnimationClip EnsurePhase2AkazaCombatCueClip()
        {
            EnsureFolderForAsset(Phase2AkazaCombatCueClipPath);
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(Phase2AkazaCombatCueClipPath);
            if (clip == null)
            {
                clip = new AnimationClip();
                AssetDatabase.CreateAsset(clip, Phase2AkazaCombatCueClipPath);
            }

            clip.name = "DB_Akaza_CombatCueClock";
            clip.frameRate = 30f;
            AnimationUtility.SetAnimationClipSettings(
                clip,
                new AnimationClipSettings
                {
                    loopTime = false,
                    stopTime = 1f
                });
            AnimationUtility.SetEditorCurve(
                clip,
                EditorCurveBinding.FloatCurve(
                    Phase2AkazaCombatCueClockName,
                    typeof(Transform),
                    "m_LocalPosition.x"),
                AnimationCurve.Linear(0f, 0f, 1f, 0f));
            EditorUtility.SetDirty(clip);
            AssetDatabase.SaveAssetIfDirty(clip);
            return clip;
        }

        private static int StripPhase2AkazaCutscenePositionCurves(AnimationClip clip)
        {
            int strippedCurveCount = 0;
            EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(clip);
            for (int i = 0; i < bindings.Length; i++)
            {
                EditorCurveBinding binding = bindings[i];
                if (!binding.propertyName.StartsWith("m_LocalPosition.", StringComparison.Ordinal))
                {
                    continue;
                }

                AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);
                if (!ShouldStripPhase2AkazaPositionCurve(binding, curve))
                {
                    continue;
                }

                AnimationUtility.SetEditorCurve(clip, binding, null);
                strippedCurveCount++;
            }

            return strippedCurveCount;
        }

        private static bool ShouldStripPhase2AkazaPositionCurve(
            EditorCurveBinding binding,
            AnimationCurve curve)
        {
            string path = binding.path;
            if (string.Equals(path, "CHakazaA:Reference", StringComparison.Ordinal)
                || path.EndsWith("/CHakazaA:world_trs", StringComparison.Ordinal)
                || path.EndsWith("/CHakazaA:hip_jnt_C", StringComparison.Ordinal)
                || path.EndsWith("/CHakazaA:hip_C", StringComparison.Ordinal))
            {
                return true;
            }

            return path.EndsWith("/CHakazaA:weaponRoot_jnt", StringComparison.Ordinal)
                && GetCurveMaxAbs(curve) > 8f;
        }

        private static float GetCurveMaxAbs(AnimationCurve curve)
        {
            if (curve == null)
            {
                return 0f;
            }

            float maxAbs = 0f;
            Keyframe[] keys = curve.keys;
            for (int i = 0; i < keys.Length; i++)
            {
                maxAbs = Mathf.Max(maxAbs, Mathf.Abs(keys[i].value));
            }

            return maxAbs;
        }

        private static Avatar LoadPhase2AkazaAvatar()
        {
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(Phase2AkazaModelPath);
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is Avatar avatar)
                {
                    return avatar;
                }
            }

            throw new InvalidOperationException($"{Phase2AkazaModelPath} must expose a Generic Avatar.");
        }

        private static bool IsUsableAkazaClip(AnimationClip clip)
        {
            if (clip == null)
            {
                return false;
            }

            return !clip.name.StartsWith("__preview", StringComparison.OrdinalIgnoreCase)
                && !clip.name.Contains("preview", StringComparison.OrdinalIgnoreCase);
        }

        private static AnimatorState AddAkazaAnimatorState(
            AnimatorStateMachine stateMachine,
            string stateName,
            Motion motion,
            float speed)
        {
            AnimatorState state = stateMachine.AddState(stateName);
            state.motion = motion;
            state.speed = Mathf.Max(0.01f, speed);
            return state;
        }

        private static void AddAkazaTriggeredState(
            AnimatorStateMachine stateMachine,
            AnimatorState hover,
            string stateName,
            Motion motion,
            string trigger,
            float speed)
        {
            AnimatorState state = AddAkazaAnimatorState(stateMachine, stateName, motion, speed);
            AnimatorStateTransition enter = stateMachine.AddAnyStateTransition(state);
            enter.hasExitTime = false;
            enter.duration = 0.05f;
            enter.canTransitionToSelf = false;
            enter.AddCondition(AnimatorConditionMode.If, 0f, trigger);

            AnimatorStateTransition exit = state.AddTransition(hover);
            exit.hasExitTime = true;
            exit.exitTime = 0.86f;
            exit.duration = 0.12f;
        }

        private static void AddAkazaTimedState(
            AnimatorStateMachine stateMachine,
            AnimatorState hover,
            string stateName,
            Motion motion,
            float speed,
            float exitTime)
        {
            AnimatorState state = AddAkazaAnimatorState(stateMachine, stateName, motion, speed);
            AnimatorStateTransition exit = state.AddTransition(hover);
            exit.hasExitTime = true;
            exit.exitTime = Mathf.Clamp01(exitTime);
            exit.duration = 0.12f;
        }

        private static string ResolveMaterialKey(Renderer renderer, Material material)
        {
            string materialName = material != null ? material.name : string.Empty;
            if (materialName.StartsWith("M_C18_Source_", StringComparison.Ordinal))
            {
                materialName = string.Empty;
            }

            return (renderer.name + " " + materialName).ToLowerInvariant();
        }

        private static Material ResolveAkazaMaterial(
            string key,
            Material skin,
            Material face,
            Material body,
            Material arm,
            Material hair,
            Material eyes,
            Material accent)
        {
            if (key.Contains("eyeball", StringComparison.Ordinal)
                || key.Contains("eyehighlight", StringComparison.Ordinal)
                || key.Contains("eye_l", StringComparison.Ordinal)
                || key.Contains("eye_r", StringComparison.Ordinal))
            {
                return eyes;
            }

            if (key.Contains("eyeline", StringComparison.Ordinal)
                || key.Contains("eyelid", StringComparison.Ordinal)
                || key.Contains("mayu", StringComparison.Ordinal)
                || key.Contains("brow", StringComparison.Ordinal)
                || key.Contains("tongue", StringComparison.Ordinal)
                || key.Contains("tooth", StringComparison.Ordinal)
                || key.Contains("headnose", StringComparison.Ordinal)
                || key.Contains("face", StringComparison.Ordinal)
                || key.Contains("head", StringComparison.Ordinal))
            {
                return face;
            }

            if (key.Contains("hair", StringComparison.Ordinal)
                || key.Contains("mimi", StringComparison.Ordinal))
            {
                return hair;
            }

            if (key.Contains("akarm", StringComparison.Ordinal)
                || key.Contains("akwp", StringComparison.Ordinal)
                || key.Contains("arm", StringComparison.Ordinal)
                || key.Contains("hand", StringComparison.Ordinal)
                || key.Contains("claw", StringComparison.Ordinal)
                || key.Contains("blade", StringComparison.Ordinal)
                || key.Contains("weapon", StringComparison.Ordinal)
                || key.Contains("wp_", StringComparison.Ordinal))
            {
                return arm;
            }

            if (key.Contains("body", StringComparison.Ordinal)
                || key.Contains("cloth", StringComparison.Ordinal)
                || key.Contains("costume", StringComparison.Ordinal)
                || key.Contains("belt", StringComparison.Ordinal)
                || key.Contains("skirt", StringComparison.Ordinal)
                || key.Contains("pants", StringComparison.Ordinal)
                || key.Contains("boots", StringComparison.Ordinal)
                || key.Contains("tie", StringComparison.Ordinal)
                || key.Contains("flower", StringComparison.Ordinal)
                || key.Contains("backparts", StringComparison.Ordinal)
                || key.Contains("legguard", StringComparison.Ordinal))
            {
                return body;
            }

            if (key.Contains("skin", StringComparison.Ordinal)
                || key.Contains("leg", StringComparison.Ordinal)
                || key.Contains("tatoo", StringComparison.Ordinal)
                || key.Contains("neck", StringComparison.Ordinal)
                || key.Contains("calf", StringComparison.Ordinal)
                || key.Contains("knee", StringComparison.Ordinal))
            {
                return skin;
            }

            if (key.Contains("wire", StringComparison.Ordinal))
            {
                return accent;
            }

            return body;
        }

        private static void FitAkazaVisualToBossProxy(GameObject visual, float targetHeight, float desiredBottomY)
        {
            Bounds bounds = CalculateRendererBounds(visual);
            if (bounds.size.y <= 0.0001f)
            {
                return;
            }

            float scale = Mathf.Clamp(targetHeight / bounds.size.y, 0.08f, 8f);
            visual.transform.localScale = Vector3.one * scale;
            bounds = CalculateRendererBounds(visual);
            float bottomDelta = desiredBottomY - bounds.min.y;
            visual.transform.position += Vector3.up * bottomDelta;
        }

        private static Bounds CalculateRendererBounds(GameObject root)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(includeInactive: true);
            bool hasBounds = false;
            Bounds bounds = new Bounds(root.transform.position, Vector3.zero);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                if (!hasBounds)
                {
                    bounds = renderer.bounds;
                    hasBounds = true;
                    continue;
                }

                bounds.Encapsulate(renderer.bounds);
            }

            return hasBounds ? bounds : new Bounds(root.transform.position, Vector3.one);
        }

        private static int CountActiveRenderableRenderers(GameObject root)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(includeInactive: true);
            int count = 0;
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer != null
                    && renderer.enabled
                    && renderer.gameObject.activeInHierarchy
                    && !renderer.forceRenderingOff)
                {
                    count++;
                }
            }

            return count;
        }

        private static void ValidatePhase2AkazaRenderableVisual(Transform visual)
        {
            Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(includeInactive: false);
            int enabledRenderers = 0;
            int skinnedRenderers = 0;
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || !renderer.enabled || renderer.forceRenderingOff)
                {
                    continue;
                }

                enabledRenderers++;
                if (renderer.sharedMaterials == null || renderer.sharedMaterials.Length == 0)
                {
                    throw new InvalidOperationException($"{renderer.name} must keep a visible material.");
                }

                if (renderer is SkinnedMeshRenderer skinnedMeshRenderer)
                {
                    skinnedRenderers++;
                    if (!skinnedMeshRenderer.updateWhenOffscreen)
                    {
                        throw new InvalidOperationException(
                            $"{renderer.name} must update while offscreen so the 2017 Akaza mesh is not culled at start.");
                    }
                }
            }

            if (enabledRenderers == 0)
            {
                throw new InvalidOperationException($"{Phase2AkazaVisualName} must have enabled renderers.");
            }

            if (skinnedRenderers == 0)
            {
                throw new InvalidOperationException($"{Phase2AkazaVisualName} must keep Akaza skinned mesh renderers.");
            }
        }

        private static void ValidatePhase2AkazaCameraVisibility(Scene scene, GameObject visual, Bounds bounds)
        {
            Camera camera = ResolvePhase2AkazaValidationCamera(scene);
            Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(camera);
            if (!GeometryUtility.TestPlanesAABB(frustumPlanes, bounds))
            {
                throw new InvalidOperationException(
                    $"{Phase2AkazaVisualName} bounds are outside the start camera frustum. "
                    + $"center={bounds.center}, size={bounds.size}, camera={camera.transform.position}.");
            }

            Vector3 viewportCenter = camera.WorldToViewportPoint(bounds.center);
            if (viewportCenter.z <= camera.nearClipPlane || viewportCenter.z >= camera.farClipPlane)
            {
                throw new InvalidOperationException(
                    $"{Phase2AkazaVisualName} is outside the camera depth range. viewport={viewportCenter}.");
            }

            float viewportHeight = EstimateViewportHeight(camera, bounds);
            if (viewportHeight < 0.055f)
            {
                throw new InvalidOperationException(
                    $"{Phase2AkazaVisualName} is too small at the start camera distance. "
                    + $"viewportHeight={viewportHeight:0.000}, bounds={bounds.size}, visual={visual.transform.position}.");
            }
        }

        private static Camera ResolvePhase2AkazaValidationCamera(Scene scene)
        {
            Camera[] cameras = CollectComponents<Camera>(scene);
            Camera fallback = null;
            for (int i = 0; i < cameras.Length; i++)
            {
                Camera camera = cameras[i];
                if (camera == null || !camera.enabled || !camera.gameObject.activeInHierarchy)
                {
                    continue;
                }

                if (camera.CompareTag("MainCamera"))
                {
                    return camera;
                }

                fallback ??= camera;
            }

            if (fallback != null)
            {
                return fallback;
            }

            throw new InvalidOperationException($"{Phase2AkazaReviewScenePath} must keep an enabled camera.");
        }

        private static float EstimateViewportHeight(Camera camera, Bounds bounds)
        {
            Vector3 extents = bounds.extents;
            Vector3 center = bounds.center;
            float minY = float.PositiveInfinity;
            float maxY = float.NegativeInfinity;
            for (int x = -1; x <= 1; x += 2)
            {
                for (int y = -1; y <= 1; y += 2)
                {
                    for (int z = -1; z <= 1; z += 2)
                    {
                        Vector3 corner = center + Vector3.Scale(extents, new Vector3(x, y, z));
                        Vector3 viewport = camera.WorldToViewportPoint(corner);
                        if (viewport.z <= camera.nearClipPlane)
                        {
                            continue;
                        }

                        minY = Mathf.Min(minY, viewport.y);
                        maxY = Mathf.Max(maxY, viewport.y);
                    }
                }
            }

            bool hasProjectedCorner =
                !float.IsInfinity(minY)
                && !float.IsInfinity(maxY)
                && !float.IsNaN(minY)
                && !float.IsNaN(maxY);
            return hasProjectedCorner ? maxY - minY : 0f;
        }

        private static void CalculateViewportRect(
            Camera camera,
            Bounds bounds,
            out Vector2 viewportMin,
            out Vector2 viewportMax,
            out int projectedCornerCount)
        {
            Vector3 extents = bounds.extents;
            Vector3 center = bounds.center;
            float minX = float.PositiveInfinity;
            float minY = float.PositiveInfinity;
            float maxX = float.NegativeInfinity;
            float maxY = float.NegativeInfinity;
            projectedCornerCount = 0;
            for (int x = -1; x <= 1; x += 2)
            {
                for (int y = -1; y <= 1; y += 2)
                {
                    for (int z = -1; z <= 1; z += 2)
                    {
                        Vector3 corner = center + Vector3.Scale(extents, new Vector3(x, y, z));
                        Vector3 viewport = camera.WorldToViewportPoint(corner);
                        if (viewport.z <= camera.nearClipPlane)
                        {
                            continue;
                        }

                        minX = Mathf.Min(minX, viewport.x);
                        minY = Mathf.Min(minY, viewport.y);
                        maxX = Mathf.Max(maxX, viewport.x);
                        maxY = Mathf.Max(maxY, viewport.y);
                        projectedCornerCount++;
                    }
                }
            }

            if (projectedCornerCount == 0
                || float.IsInfinity(minX)
                || float.IsInfinity(minY)
                || float.IsInfinity(maxX)
                || float.IsInfinity(maxY)
                || float.IsNaN(minX)
                || float.IsNaN(minY)
                || float.IsNaN(maxX)
                || float.IsNaN(maxY))
            {
                viewportMin = Vector2.zero;
                viewportMax = Vector2.zero;
                projectedCornerCount = 0;
                return;
            }

            viewportMin = new Vector2(minX, minY);
            viewportMax = new Vector2(maxX, maxY);
        }

        private static void ValidateAkazaAnimatorTrigger(Animator animator, string triggerName)
        {
            AnimatorControllerParameter[] parameters = animator.parameters;
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].type == AnimatorControllerParameterType.Trigger
                    && string.Equals(parameters[i].name, triggerName, StringComparison.Ordinal))
                {
                    return;
                }
            }

            throw new InvalidOperationException($"{animator.name} is missing trigger {triggerName}.");
        }

        private static void ValidateAkazaAnimatorPlayStartPose(Animator animator)
        {
            if (animator.applyRootMotion)
            {
                throw new InvalidOperationException($"{animator.name} must not apply root motion at play start.");
            }

            if (animator.cullingMode != AnimatorCullingMode.AlwaysAnimate)
            {
                throw new InvalidOperationException($"{animator.name} must keep AlwaysAnimate culling.");
            }

            if (animator.runtimeAnimatorController is not AnimatorController controller)
            {
                throw new InvalidOperationException($"{animator.name} must use an AnimatorController asset.");
            }

            if (controller.layers == null || controller.layers.Length == 0)
            {
                throw new InvalidOperationException($"{controller.name} must keep a base layer.");
            }

            AnimatorState defaultState = controller.layers[0].stateMachine.defaultState;
            if (defaultState == null || !string.Equals(defaultState.name, "Hover", StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"{controller.name} must start in the Hover state.");
            }

            if (defaultState.motion != null)
            {
                throw new InvalidOperationException(
                    $"{controller.name} Hover state must not play a cutscene FBX clip at play start.");
            }
        }

        private static void ValidateAkazaAnimatorStateMotion(
            Animator animator,
            string stateName,
            Motion expectedMotion)
        {
            ValidateGameOwnedAsset(expectedMotion, $"phase2 Akaza {stateName} motion");
            if (animator.runtimeAnimatorController is not AnimatorController controller)
            {
                throw new InvalidOperationException($"{animator.name} must use an AnimatorController asset.");
            }

            for (int layerIndex = 0; layerIndex < controller.layers.Length; layerIndex++)
            {
                ChildAnimatorState[] states = controller.layers[layerIndex].stateMachine.states;
                for (int stateIndex = 0; stateIndex < states.Length; stateIndex++)
                {
                    AnimatorState state = states[stateIndex].state;
                    if (!string.Equals(state.name, stateName, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    if (state.motion != expectedMotion)
                    {
                        throw new InvalidOperationException(
                            $"{controller.name} state {stateName} must use {expectedMotion.name}.");
                    }

                    return;
                }
            }

            throw new InvalidOperationException($"{controller.name} is missing state {stateName}.");
        }

        private static void ValidatePhase2AkazaInPlaceClip(string clipPath)
        {
            AnimationClip clip = LoadAsset<AnimationClip>(clipPath);
            ValidateGameOwnedAsset(clip, $"phase2 Akaza in-place clip {clipPath}");
            EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(clip);
            for (int i = 0; i < bindings.Length; i++)
            {
                EditorCurveBinding binding = bindings[i];
                if (!binding.propertyName.StartsWith("m_LocalPosition.", StringComparison.Ordinal))
                {
                    continue;
                }

                AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);
                if (!ShouldStripPhase2AkazaPositionCurve(binding, curve))
                {
                    continue;
                }

                throw new InvalidOperationException(
                    $"{clip.name} still contains cutscene position curve {binding.path} {binding.propertyName}.");
            }
        }

        private static void ValidatePhase2AkazaCombatCueClock(Transform visual)
        {
            Transform cueClock = visual.Find(Phase2AkazaCombatCueClockName);
            if (cueClock == null)
            {
                throw new InvalidOperationException(
                    $"{Phase2AkazaVisualName} must keep {Phase2AkazaCombatCueClockName} for safe combat cue timing.");
            }

            if (cueClock.GetComponentsInChildren<Renderer>(includeInactive: true).Length > 0)
            {
                throw new InvalidOperationException(
                    $"{Phase2AkazaCombatCueClockName} must not render; it only carries safe Animator timing curves.");
            }
        }

        private static void ValidatePhase2AkazaPatternSequence(BossBarrageEmitter emitter)
        {
            ValidateFloat(
                LoadAsset<BossBarragePatternProfile>(Phase2AkazaHoverLancePatternProfilePath),
                "initialDelaySeconds",
                0.35f);
            ValidateArrayReference(
                emitter,
                "patternSequence",
                0,
                LoadAsset<BossBarragePatternProfile>(Phase2AkazaHoverLancePatternProfilePath));
            ValidateArrayReference(
                emitter,
                "patternSequence",
                1,
                LoadAsset<BossBarragePatternProfile>(Phase2AkazaSummonCurtainPatternProfilePath));
            ValidateArrayReference(
                emitter,
                "patternSequence",
                2,
                LoadAsset<BossBarragePatternProfile>(Phase2AkazaSpiralVolleyPatternProfilePath));
            ValidateArrayContainsReference(
                emitter,
                "patternSequence",
                LoadAsset<BossBarragePatternProfile>(Phase2AkazaCrushNetPatternProfilePath),
                "Akaza crush net");
        }

        private static void ValidateIntValueAtLeast(int actual, int minimum, string label)
        {
            if (actual < minimum)
            {
                throw new InvalidOperationException($"{label} expected at least {minimum}, found {actual}.");
            }
        }

        private static void ValidateNoDirectImportedSceneDependencies(string scenePath)
        {
            string[] dependencies = AssetDatabase.GetDependencies(scenePath, recursive: true);
            for (int i = 0; i < dependencies.Length; i++)
            {
                string dependency = dependencies[i].Replace('\\', '/');
                if (dependency.Contains("/_Imported/", StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        $"{scenePath} must not depend on raw imported asset {dependency}.");
                }
            }
        }

        private readonly struct PromotedAkazaAsset
        {
            public PromotedAkazaAsset(
                string sourceRelativePath,
                string targetAssetPath,
                AkazaImportKind importKind)
            {
                SourceRelativePath = sourceRelativePath;
                TargetAssetPath = targetAssetPath;
                ImportKind = importKind;
            }

            public string SourceRelativePath { get; }
            public string TargetAssetPath { get; }
            public AkazaImportKind ImportKind { get; }
        }

        private readonly struct TimelineSourceClip
        {
            public TimelineSourceClip(
                string assetPath,
                double timelineStartSeconds,
                double timelineEndSeconds)
            {
                AssetPath = assetPath;
                TimelineStartSeconds = timelineStartSeconds;
                TimelineEndSeconds = timelineEndSeconds;
            }

            public string AssetPath { get; }
            public double TimelineStartSeconds { get; }
            public double TimelineEndSeconds { get; }
            public double DurationSeconds => TimelineEndSeconds - TimelineStartSeconds;
        }

        private readonly struct RuntimeSourceCue
        {
            public RuntimeSourceCue(
                string assetPath,
                double startSeconds,
                double clipInSeconds,
                double durationSeconds)
            {
                AssetPath = assetPath;
                StartSeconds = startSeconds;
                ClipInSeconds = clipInSeconds;
                DurationSeconds = durationSeconds;
            }

            public string AssetPath { get; }
            public double StartSeconds { get; }
            public double ClipInSeconds { get; }
            public double DurationSeconds { get; }
            public double EndSeconds => StartSeconds + DurationSeconds;
        }
    }
}
