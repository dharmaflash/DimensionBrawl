using System;
using System.Collections.Generic;
using DimensionBrawl.AI;
using DimensionBrawl.Combat;
using DimensionBrawl.Enemies;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Player;
using DimensionBrawl.Presentation;
using DimensionBrawl.Test;
using DimensionBrawl.UI;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace DimensionBrawl.Editor
{
    public static partial class ActionFoundationBossBarrageLaneReviewSetup
    {
        public const string ReviewScenePath = "Assets/_Game/Scenes/ActionFoundationBossBarrageLaneReview.unity";
        public const string DuelReviewScenePath = "Assets/_Game/Scenes/ActionFoundationBossSummonDuelReview.unity";
        private const string OlympusInvasionStageScenePath = "Assets/_Game/Scenes/OlympusCorridorInvasionStage.unity";
        public const string PatternProfilePath =
            ActionFoundationProfileSetup.ProfileRoot + "/DB_BossBarrage_NeedleLock.asset";
        public const string CoverFirePatternProfilePath =
            ActionFoundationProfileSetup.ProfileRoot + "/DB_BossBarrage_CoverFire.asset";
        public const string EscortScreenPatternProfilePath =
            ActionFoundationProfileSetup.ProfileRoot + "/DB_BossBarrage_EscortScreen.asset";
        public const string LayeredSalvoPatternProfilePath =
            ActionFoundationProfileSetup.ProfileRoot + "/DB_BossBarrage_LayeredSalvo.asset";
        public const string StaggeredCrossfirePatternProfilePath =
            ActionFoundationProfileSetup.ProfileRoot + "/DB_BossBarrage_StaggeredCrossfire.asset";
        public const string TwinSweepPatternProfilePath =
            ActionFoundationProfileSetup.ProfileRoot + "/DB_BossBarrage_TwinSweep.asset";
        public const string LeftClampPatternProfilePath =
            ActionFoundationProfileSetup.ProfileRoot + "/DB_BossBarrage_LeftClamp.asset";
        public const string RightClampPatternProfilePath =
            ActionFoundationProfileSetup.ProfileRoot + "/DB_BossBarrage_RightClamp.asset";
        public const string PunishNetPatternProfilePath =
            ActionFoundationProfileSetup.ProfileRoot + "/DB_BossBarrage_PunishNet.asset";
        public const string LinePressurePatternProfilePath =
            ActionFoundationProfileSetup.ProfileRoot + "/DB_BossBarrage_LinePressure.asset";
        public const string BossBasicFireProfilePath =
            ActionFoundationProfileSetup.ProfileRoot + "/DB_BossBasicFire_LanePoke.asset";
        public const string StageProfilePath =
            ActionFoundationProfileSetup.ProfileRoot + "/DB_FrontlineWaveStage_MotivationReview.asset";
        public const string ProjectilePrefabPath =
            "Assets/_Game/Prefabs/Combat/PF_BossBarrageProjectile_NeedleLock.prefab";
        private const string BossBarrageProjectileTrailMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_BossBarrageProjectileTrail.mat";
        public const string LocalDefenseProfilePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_PlayerAction_BossBarrageLocalDefense.asset";
        public const string MeleeActionProfilePath =
            ActionFoundationProfileSetup.PlayerActionProfilePath;
        public const string ProjectileMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_BossBarrageProjectile.mat";
        private const string LinePressureProjectileMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_BossBarrageProjectile_LinePressure.mat";
        private const string LayeredSalvoProjectileMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_BossBarrageProjectile_LayeredSalvo.mat";
        private const string BossBasicFireProjectileMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_BossBasicFireProjectile.mat";
        private const string Forge3DMissileProjectileMeshPath =
            "Assets/_Imported/AssetStore/FORGE3D/Sci-Fi Effects/Effects/Missiles/Meshes/missile_004_lod0.FBX";
        public const string Skill1ProjectilePrefabPath =
            "Assets/_Game/Prefabs/Combat/PF_PlayerSkill1Projectile_LaneBolt.prefab";
        public const string RangedBasicProjectilePrefabPath =
            "Assets/_Game/Prefabs/Combat/PF_PlayerRangedBasicProjectile_AimBolt.prefab";
        public const string SummonSlot1ProjectilePrefabPath =
            "Assets/_Game/Prefabs/Combat/PF_SummonSlot1Projectile_AssistBolt.prefab";
        public const string SummonSlot2ProjectilePrefabPath =
            "Assets/_Game/Prefabs/Combat/PF_SummonSlot2Projectile_LaserBolt.prefab";
        public const string SummonSlot3ProjectilePrefabPath =
            "Assets/_Game/Prefabs/Combat/PF_SummonSlot3Projectile_FireBreath.prefab";
        public const string SummonSlot1EntryCuePrefabPath =
            "Assets/_Game/Prefabs/Combat/PF_SummonSlot1EntryCue_MagicCircle.prefab";
        public const string SummonSlot1ActorPrefabPath =
            "Assets/_Game/Prefabs/Combat/PF_SummonSlot1Actor_Proxy.prefab";
        public const string SummonSlot2ActorPrefabPath =
            "Assets/_Game/Prefabs/Combat/PF_SummonSlot2Actor_MarksmanProxy.prefab";
        public const string SummonSlot3ActorPrefabPath =
            "Assets/_Game/Prefabs/Combat/PF_SummonSlot3Actor_VanguardProxy.prefab";
        public const string BossSummonPressureActorPrefabPath =
            "Assets/_Game/Prefabs/Combat/PF_BossSummonPressureActor_Proxy.prefab";
        public const string BossLaserSummonActorPrefabPath =
            "Assets/_Game/Prefabs/Combat/PF_BossLaserSummonActor_Proxy.prefab";
        public const string BossLaserTelegraphVfxPrefabPath =
            "Assets/_Game/Art/VFX/ActionFoundation/Summons/Prefabs/PF_SummonLaserBeam_FORGE3D.prefab";
        public const string BossLaserTelegraphSfxClipPath =
            "Assets/_Game/Art/Audio/SFX/Reviewed/DB_SFX_Enemy_Telegraph_01.mp3";
        public const string BossLaserFireSfxClipPath =
            "Assets/_Game/Art/Audio/SFX/Guns/DB_SFX_PlayerRanged_Gunshot_05.wav";
        private const string BossBasicFireSfxClipPath =
            "Assets/_Game/Art/Audio/SFX/Reviewed/DB_SFX_Boss_Bullet_F_01.mp3";
        private const string PlayerRangedReloadSfxClipPath =
            "Assets/_Game/Art/Audio/SFX/Guns/DB_SFX_PlayerRanged_Reload_02.mp3";
        public const string SummonSlot1ActionProfilePath =
            ActionFoundationProfileSetup.ProfileRoot + "/DB_SummonSlot1_ChargeBruiser.asset";
        public const string SummonSlot2ActionProfilePath =
            ActionFoundationProfileSetup.ProfileRoot + "/DB_SummonSlot2_LaserSoldier.asset";
        public const string SummonSlot3ActionProfilePath =
            ActionFoundationProfileSetup.ProfileRoot + "/DB_SummonSlot3_FireDragon.asset";
        public const string BossSummonPressureProfilePath =
            ActionFoundationProfileSetup.ProfileRoot + "/DB_BossSummonPressure_SummonCaller.asset";
        public const string BossPressureActionDeckProfilePath =
            ActionFoundationProfileSetup.ProfileRoot + "/DB_BossPressureActionDeck_PocketReview.asset";
        public const string SummonOpportunityProfilePath =
            ActionFoundationProfileSetup.ProfileRoot + "/DB_SummonOpportunity_BossPressureBlock.asset";
        private const string UIRouteTablePath =
            "Assets/_Game/DesignData/UI/DB_UIRouteTable.asset";
        public const string SummonSlot1PresentationCandidateProfilePath =
            ActionFoundationProfileSetup.ProfileRoot + "/DB_SummonPresentation_PlayerChargeBruiser.asset";
        public const string SummonSlot2PresentationCandidateProfilePath =
            ActionFoundationProfileSetup.ProfileRoot + "/DB_SummonPresentation_PlayerLaserSoldier.asset";
        public const string SummonSlot3PresentationCandidateProfilePath =
            ActionFoundationProfileSetup.ProfileRoot + "/DB_SummonPresentation_PlayerFireDragon.asset";
        public const string BossSummonPressurePresentationCandidateProfilePath =
            ActionFoundationProfileSetup.ProfileRoot + "/DB_SummonPresentation_BossAuraCaptain.asset";
        private const string SummonSlot1ActorVisualName = "SummonSlot1Visual_ShieldBreakerElite";
        private const string SummonSlot2ActorVisualName = "SummonSlot2Visual_LaserRifleman";
        private const string SummonSlot3ActorVisualName = "SummonSlot3Visual_FireDragon";
        private const string BossSummonPressureActorVisualName = "BossSummonPressureVisual_AuraCaptainElite";
        private const string SummonSlot1ActorVisualRoleId = "SciFiSoldier.Elite.ShieldBreaker";
        private const string SummonSlot2ActorVisualRoleId = "SciFiSoldier.LineCaster";
        private const string SummonSlot3ActorVisualRoleId = "Summon.FireDragon.VolcanoDragon";
        private const string BossSummonPressureActorVisualRoleId = "SciFiSoldier.Elite.AuraCaptain";
        private const string SummonActorMoveSpeedParameter = "MoveSpeed";
        private const string SummonActorSpawnTrigger = "EliteSummonPackage";
        private const string SummonActorAttackTrigger = "Attack";
        private const string SummonActorHitTrigger = "Hit";
        private const string SummonActorDeathTrigger = "Death";
        private const string Skill1ProjectileMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_PlayerSkill1Projectile.mat";
        private const string RangedBasicProjectileMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_PlayerRangedBasicProjectile.mat";
        private const string ImportedRifleShotLoopedVfxPrefabPath =
            "Assets/_Imported/AssetStore/VFX/Vefects_ShotsVFXURP/Shots VFX URP/Shots/Muzzle Flash/Looped/VFX_Muzzle_Flash_Rifle_Looped.prefab";
        private const string PerfectDodgeScreenDomainShaderPath =
            "Assets/_Game/Art/VFX/CombatCues/Shaders/DB_PerfectDodgeScreenDomain.shader";
        private const string PerfectDodgeWorldFxShaderPath =
            "Assets/_Game/Art/VFX/CombatCues/Shaders/DB_PerfectDodgeWorldFx.shader";
        private const string PerfectDodgeAfterimageShaderPath =
            "Assets/_Game/Art/VFX/CombatCues/Shaders/DB_PerfectDodgeAfterimage.shader";
        private const string PerfectDodgeScreenDomainMaterialPath =
            "Assets/_Game/Art/VFX/CombatCues/Materials/DB_PerfectDodgeScreenDomain.mat";
        private const string PerfectDodgeWorldFxMaterialPath =
            "Assets/_Game/Art/VFX/CombatCues/Materials/DB_PerfectDodgeWorldFx.mat";
        private const string PerfectDodgeAfterimageMaterialPath =
            "Assets/_Game/Art/VFX/CombatCues/Materials/DB_PerfectDodgeAfterimage.mat";
        private const string PerfectDodgeGlitchOverlayMaterialPath =
            "Assets/_Game/Art/Materials/Cinematics/IntroGatePod/DB_UI_FirstPersonGlitchOverlay.mat";
        private const float PerfectDodgeScreenShaderIntensity = 0.92f;
        private const float PerfectDodgeScreenRadialWarpStrength = 0.72f;
        private const float PerfectDodgeScreenScanlineStrength = 0.34f;
        private const float PerfectDodgeScreenRadialBlurStrength = 0.54f;
        private const float PerfectDodgeScreenGridStrength = 0.68f;
        private const float PerfectDodgeScreenFractureStrength = 0.74f;
        private const float PerfectDodgeScreenChromaticStrength = 0.86f;
        private const float PerfectDodgeGlitchOverlayAlpha = 0.16f;
        private const float PerfectDodgeGlitchNoiseStrength = 1.25f;
        private const float PerfectDodgeGlitchJitterStrength = 0.42f;
        private const string ImportedMagicMissilesPrefabRoot =
            "Assets/_Imported/AssetStore/VFX/MagicMissiles/Prefabs";
        private const string ImportedMagicMissilesFireMissilePrefabPath =
            ImportedMagicMissilesPrefabRoot + "/Missiles/FireMissile.prefab";
        private const string ImportedMagicMissilesLightMissilePrefabPath =
            ImportedMagicMissilesPrefabRoot + "/Missiles/LightMissile.prefab";
        private const string ImportedMagicMissilesArcaneMissilePrefabPath =
            ImportedMagicMissilesPrefabRoot + "/Missiles/ArcaneMissile.prefab";
        private const string ImportedMagicMissilesHolyMissilePrefabPath =
            ImportedMagicMissilesPrefabRoot + "/Missiles/HolyMissile.prefab";
        private const string ImportedMagicMissilesArcaneCirclePrefabPath =
            ImportedMagicMissilesPrefabRoot + "/Circles/ArcaneCircle3.prefab";
        private const string ImportedMagicMissilesShieldCirclePrefabPath =
            ImportedMagicMissilesPrefabRoot + "/Circles/ArcaneCircle4.prefab";
        private const string ImportedMagicMissilesPulsePrefabPath =
            ImportedMagicMissilesPrefabRoot + "/Muzzleflash/HolyMuzzle.prefab";
        private const string ImportedMagicMissilesHealingAuraPrefabPath =
            ImportedMagicMissilesPrefabRoot + "/AreaEffect/AOE_Healing2.prefab";
        private const string ImportedMagicMissilesArcaneAuraPrefabPath =
            ImportedMagicMissilesPrefabRoot + "/AreaEffect/AOE_Purple.prefab";
        private const string ImportedMagicMissilesPressureAuraPrefabPath =
            ImportedMagicMissilesPrefabRoot + "/AreaEffect/AOE_PsychStorm.prefab";
        private const string ImportedMagicMissilesLightImpactPrefabPath =
            ImportedMagicMissilesPrefabRoot + "/Explosions/LightExplosion.prefab";
        private const string ImportedMagicMissilesArcaneImpactPrefabPath =
            ImportedMagicMissilesPrefabRoot + "/Explosions/ArcaneExplosion.prefab";
        private const string ImportedMagicMissilesHolyImpactPrefabPath =
            ImportedMagicMissilesPrefabRoot + "/Explosions/HolyExplosion.prefab";
        private const string ImportedMagicMissilesDeathImpactPrefabPath =
            ImportedMagicMissilesPrefabRoot + "/Explosions/DeathExplosion.prefab";
        private const string ImportedHovlSciFiEffectsPrefabRoot =
            "Assets/_Imported/AssetStore/VFX/Hovl Studio/Sci-fi effects 2/Prefabs";
        private const string ImportedHovlProjectileBulletPrefabPath =
            ImportedHovlSciFiEffectsPrefabRoot + "/Projectile bullet.prefab";
        private const string ImportedHovlLaserHitPrefabPath =
            ImportedHovlSciFiEffectsPrefabRoot + "/Laser hit.prefab";
        private const string ImportedForge3DMissileExamplePrefabPath =
            "Assets/_Imported/AssetStore/FORGE3D/Sci-Fi Effects/Effects/Missiles/Example/missile_example.prefab";
        private const string HovlSciFiEffectsPromotedRoot =
            "Assets/_Game/Art/VFX/HovlSciFiEffects";
        private const string HovlSciFiEffectsMaterialRoot =
            HovlSciFiEffectsPromotedRoot + "/Materials";
        private const string HovlSciFiEffectsTextureRoot =
            HovlSciFiEffectsPromotedRoot + "/Textures";
        private const string HovlSciFiEffectsShaderRoot =
            HovlSciFiEffectsPromotedRoot + "/Shaders";
        private const string HovlSciFiEffectsMeshRoot =
            HovlSciFiEffectsPromotedRoot + "/Meshes";
        private const string BossBarrageHovlProjectileChildName =
            "BossBarrageProjectileVfx_HovlProjectileBullet";
        private const string Forge3DMissilePromotedRoot =
            "Assets/_Game/Art/VFX/Forge3DMissiles";
        private const string Forge3DMissileMaterialRoot =
            Forge3DMissilePromotedRoot + "/Materials";
        private const string Forge3DMissileTextureRoot =
            Forge3DMissilePromotedRoot + "/Textures";
        private const string Forge3DMissileShaderRoot =
            Forge3DMissilePromotedRoot + "/Shaders";
        private const string Forge3DMissileMeshRoot =
            Forge3DMissilePromotedRoot + "/Meshes";
        private const string BossBarrageForge3DMissileChildName =
            "BossBarrageProjectileVfx_Forge3DMissileExample";
        private const string MagicMissilesPromotedRoot =
            "Assets/_Game/Art/VFX/MagicMissiles";
        private const string MagicMissilesMaterialRoot =
            MagicMissilesPromotedRoot + "/Materials";
        private const string MagicMissilesTextureRoot =
            MagicMissilesPromotedRoot + "/Textures";
        private const string MagicMissilesMeshRoot =
            MagicMissilesPromotedRoot + "/Meshes";
        private const string CombatVfxMaterialRoot =
            "Assets/_Game/Art/VFX/CombatCues/Materials";
        private const string CombatVfxMeshRoot =
            "Assets/_Game/Art/VFX/CombatCues/Meshes";
        private const string ActionFoundationPrimitiveMeshRoot =
            "Assets/_Game/Art/Meshes/ActionFoundation";
        private const string CombatVfxPrefabRoot =
            "Assets/_Game/Art/VFX/CombatCues/Prefabs";
        private const string MuzzleFlashFrontMaterialPath =
            CombatVfxMaterialRoot + "/DB_CombatVfx_MuzzleFlashFront.mat";
        private const string MuzzleFlashSideMaterialPath =
            CombatVfxMaterialRoot + "/DB_CombatVfx_MuzzleFlashSide.mat";
        private const string MuzzleSmokeMaterialPath =
            CombatVfxMaterialRoot + "/DB_CombatVfx_Smoke.mat";
        private const string SummonSlot1ProjectileMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_SummonSlot1Projectile.mat";
        private const string SummonSlot2ProjectileMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_SummonSlot2Projectile.mat";
        private const string SummonSlot3ProjectileMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_SummonSlot3Projectile.mat";
        private const string SummonSlot1EntryCueMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_SummonSlot1EntryCue.mat";
        private const string SummonSlot1EntryCueAccentMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_SummonSlot1EntryCueAccent.mat";
        private const string SummonSlot1ActorMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_SummonSlot1Actor.mat";
        private const string SummonSlot2ActorMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_SummonSlot2Actor.mat";
        private const string SummonSlot3ActorMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_SummonSlot3Actor.mat";
        private const string SummonPressureScreenMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_SummonPressureScreen.mat";
        private const string SummonSlot1ActorPulseMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_SummonSlot1ActorPulse.mat";
        private const string SummonSlot2ActorPulseMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_SummonSlot2ActorPulse.mat";
        private const string SummonSlot3ActorPulseMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_SummonSlot3ActorPulse.mat";
        private const string SummonSlot3PressureScreenMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_SummonSlot3PressureScreen.mat";
        private const string SummonSlot1SlamImpactMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_SummonSlot1SlamImpact.mat";
        private const string SummonSlot2LaserBeamMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_SummonSlot2LaserBeam.mat";
        private const string SummonSlot3FireBreathMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_SummonSlot3FireBreath.mat";
        private const string SummonSlot3DragonBodyMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_SummonSlot3DragonBody.mat";
        private const string SummonSlot3DragonWingMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_SummonSlot3DragonWing.mat";
        private const string BossSummonPressureActorMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_BossSummonPressureActor.mat";
        private const string BossSummonPressureScreenMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_BossSummonPressureScreen.mat";
        private const string BossSummonPressureActorPulseMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_BossSummonPressureActorPulse.mat";
        private const string SummonHealthBarBackMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_SummonHealthBarBack.mat";
        private const string SummonHealthBarAllyFillMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_SummonHealthBarAllyFill.mat";
        private const string SummonHealthBarEnemyFillMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_SummonHealthBarEnemyFill.mat";
        private const string SummonHealthBarRootName = "HealthBarRoot";
        private const string SummonHealthBarBackName = "HealthBarBack";
        private const string SummonHealthBarFillName = "HealthBarFill";

        private const string ReviewRootPrefix = "BossBarrageLaneReview_";
        private const string LaneRootName = ReviewRootPrefix + "SummonLaneSpace";
        private const string BossProxyRootName = ReviewRootPrefix + "BossProxy_NeedleLock";
        private const string CloseThreatRootName = ReviewRootPrefix + "CloseThreat_ClosePunish";
        private const string ProjectilePoolRootName = ReviewRootPrefix + "ProjectilePool";
        private const string ActionCuePoolRootName = ReviewRootPrefix + "ActionCuePool";
        private const string SummonActorPoolRootName = ReviewRootPrefix + "SummonActorPool";
        private const string BossSummonActorPoolRootName = ReviewRootPrefix + "BossSummonActorPool";
        private const string PocketOwnerRootName = ReviewRootPrefix + "PocketOwner";
        private const string DuelOwnerRootName = ReviewRootPrefix + "DuelOwner";
        private const string DuelClearMarkerName = ReviewRootPrefix + "DuelClearMarker";
        private const string DuelFailMarkerName = ReviewRootPrefix + "DuelFailMarker";
        private const string HudRootName = ReviewRootPrefix + "DebugHud";
        private const string ArenaVfxRootName = "ActionFoundation_ArenaVfx";
        private const string MarkerRootName = ReviewRootPrefix + "Markers";
        private const string AmbientVfxRootName = ReviewRootPrefix + "AmbientVfx";
        private const string AmbientAudioRootName = ReviewRootPrefix + "AmbientAudio";
        private const string BgmAudioRootName = ReviewRootPrefix + "BgmAudio";
        private const string PlayerFootstepAudioName = "ReviewedFootstepAudio_Player";
        private const string CloseThreatFootstepAudioName = "ReviewedFootstepAudio_CloseThreat";
        private const string BossProxyFootstepAudioName = "ReviewedFootstepAudio_BossProxy";
        private const string PlayerRangedReloadAudioName = "ReviewedReloadAudio_PlayerRanged";
        private const string SummonActorFootstepAudioName = "ReviewedFootstepAudio_Actor";
        private const string PocketClearMarkerName = ReviewRootPrefix + "PocketClearMarker";
        private const string PocketFailMarkerName = ReviewRootPrefix + "PocketFailMarker";
        private const string SummonEntryMarkerName = ReviewRootPrefix + "SummonEntryMarker";
        private const string BossProxyMarkerName = ReviewRootPrefix + "BossProxyMarker";
        private const string BossBasicFireMuzzleName = "BossBasicFireMuzzle";
        private const float BossProxyReviewMaxHealth = 3600f;
        private const float PlayerSummonBaseEnergyPerSecond = 8f;
        private const float PlayerSummonBackSafetyGainScale = 0.35f;
        private const float PlayerSummonMidChargeGainScale = 0.75f;
        private const float PlayerSummonForwardRiskGainScale = 1.25f;
        private const float BossPressureBaseCostPerSecond = 8f;
        private const float EnemySummonPacingInitialDelaySeconds = 10.5f;
        private const float EnemySummonPacingRespawnIntervalSeconds = 14.5f;
        private const float EnemySummonPacingRetryIntervalSeconds = 1.1f;
        private const float BossProxyBodyHitboxRadius = 1.05f;
        private static readonly Vector3 BossProxyBodyHitboxCenter = new Vector3(0f, -0.35f, -0.05f);
        private const int PlayerRangedBasicPrewarmCount = 16;
        private const float PlayerRangedBasicDamage = 12f;
        private const float PlayerRangedBasicProjectileSpeed = 24f;
        private const float PlayerRangedBasicProjectileLifetimeSeconds = 1.75f;
        private const float PlayerRangedBasicProjectileRadius = 0.31f;
        private const float PlayerRangedBasicFireIntervalSeconds = 0.12f;
        private const int PlayerRangedBasicMagazineSize = 24;
        private const float PlayerRangedBasicCameraAimFallbackDistance = 32f;
        private const float PlayerRangedBasicCameraAimRaycastDistance = 96f;
        private const float PlayerRangedBasicTargetHeight = 1.1f;
        private const float PlayerRangedBasicAimAssistDistance = 30f;
        private const float PlayerRangedBasicAimAssistAngleDegrees = 28f;
        private const string CloseThreatBodyHitboxName = ReviewRootPrefix + "CloseThreatBodyHitbox";
        private const float CloseThreatBodyHitboxRadius = 0.68f;
        private static readonly Vector3 CloseThreatBodyHitboxCenter = new Vector3(0f, 1f, 0f);
        private const string BossTelegraphRootName = ReviewRootPrefix + "BossBarrageTelegraphMarkers";
        private const string BossProxyHumanoidVisualName = ReviewRootPrefix + "HumanoidBossVisual_SciFiSoldier_01_Commando";
        private const string BossProxyHumanoidImportedRoot =
            "Assets/_Imported/AssetStore/Protofactor/Sci Fi";
        private const string BossProxyHumanoidShooterRoot =
            BossProxyHumanoidImportedRoot + "/SciFiCharactersMegaPackVol3/SciFiShooterCharactersPackVol3";
        private const string BossProxyHumanoidCommonWeaponRoot =
            BossProxyHumanoidImportedRoot + "/Common/Weapons";
        private const string BossProxyHumanoidSourcePrefabPath =
            BossProxyHumanoidShooterRoot + "/SciFiSoldier_01/Prefabs/SciFiSoldier_01_Commando.prefab";
        private const string BossProxyHumanoidSourceModelPath =
            BossProxyHumanoidShooterRoot + "/SciFiSoldier_01/FBX Files/SK_SciFiSoldier_01.fbx";
        private const string BossProxyHumanoidSourceAssaultRifleModelPath =
            BossProxyHumanoidCommonWeaponRoot + "/FBX Files/SM_SciFiAssaultRifle_01.FBX";
        private const string BossProxyHumanoidSourceAssaultRifleName = "SM_SciFiAssaultRifle_01";
        private const string BossProxyLineCasterVariantModelPath =
            "Assets/_Game/Art/Characters/Enemies/SciFiSoldiers/RoleVariants/LineCaster/Models/SK_LineCaster_SciFiSoldier01.fbx";
        private const string CinematicSupportDragonRootName = ReviewRootPrefix + "CinematicSupportDragon_Volcano";
        private const string RangedPlayerVisualRootName = ReviewRootPrefix + "RangedVisual_Inori";
        private const string RetiredRifleGirlRangedPlayerVisualRootName = ReviewRootPrefix + "RangedVisual_RifleGirl";
        private const string RangedPlayerModelName = ReviewRootPrefix + "RangedModel_Inori";
        private const string RangedPlayerWeaponName = ReviewRootPrefix + "RangedWeapon_Rifle";
        private const string MeleePlayerWeaponRootName = ReviewRootPrefix + "MeleeWeapons_CombatGirlSwordShield";
        private const string CinematicSupportDragonSourcePrefabPath =
            "Assets/_Imported/AssetStore/HEROIC FANTASY CREATURES FULL PACK VOL3/Elemental Dragons Pack/Volcano Dragon/Prefabs/VolcanoDragon_PBR.prefab";
        private const string CinematicSupportDragonAttackStateName = "FlyStationarySpitFireBall";
        private const string RifleGirlSourcePrefabPath =
            "Assets/_Imported/AssetStore/CombatGirlsCharacterPack_RifleGirl/RifleGirl/Prefab/Rifle_Full_Body.prefab";
        private const string InoriSourcePrefabPath =
            ActionFoundationInoriPlayerVisualAssetSetup.SourcePrefabPath;
        private const string RifleGirlRangedControllerPath =
            ActionFoundationPlayerCombatModeAssetSetup.RangedCandidateControllerPath;
        private const string InoriRifleAnimatorControllerPath =
            "Assets/_Game/Art/Animations/Player/Inori/DB_Inori_Rifle_ActionFoundation.controller";
        private const string InoriCinematicAnimatorControllerPath =
            "Assets/_Game/Art/Animations/Cinematics/Inori/DB_Inori_CinematicP0.controller";
        private const string CinematicProfileRoot =
            "Assets/_Game/DesignData/Profiles/Cinematics";
        private const string CinematicQteAssistProfilePath =
            CinematicProfileRoot + "/DB_Cinematic_QTEAssist.asset";
        private const string CinematicUltimateProfilePath =
            CinematicProfileRoot + "/DB_Cinematic_UltimateCutIn.asset";
        private const string CinematicDangerProfilePath =
            CinematicProfileRoot + "/DB_Cinematic_DangerCue.asset";
        private const string CinematicBossIntroProfilePath =
            CinematicProfileRoot + "/DB_Cinematic_BossIntro.asset";
        private const string CinematicPhaseTransitionProfilePath =
            CinematicProfileRoot + "/DB_Cinematic_PhaseTransition.asset";
        private const string CinematicBreakProfilePath =
            CinematicProfileRoot + "/DB_Cinematic_BreakMoment.asset";
        private const string CinematicDialogueReactionBeatProfilePath =
            CinematicProfileRoot + "/DB_Cinematic_DialogueReactionBeat.asset";
        private const string CinematicBossSummonPressureProfilePath =
            CinematicProfileRoot + "/DB_Cinematic_BossSummonPressure.asset";
        private const string CinematicResultProfilePath =
            CinematicProfileRoot + "/DB_Cinematic_ResultBridge.asset";
        private const string CinematicSummonProfilePath =
            CinematicProfileRoot + "/DB_Cinematic_SummonEntry.asset";
        private const string CinematicSummonFollowupProfilePath =
            CinematicProfileRoot + "/DB_Cinematic_SummonFollowupHit.asset";
        private const string CinematicSummonEmpowerProfilePath =
            CinematicProfileRoot + "/DB_Cinematic_SummonEmpower.asset";
        private const string CinematicSummonRecallProfilePath =
            CinematicProfileRoot + "/DB_Cinematic_SummonRecall.asset";
        private const string CombatGirlAnimatorControllerPath =
            "Assets/_Game/Art/Animations/Player/CombatGirlSwordShield/DB_CombatGirl_ActionFoundation.controller";
        private static readonly Vector3 InoriRifleMuzzleFallbackLocalPosition = new Vector3(-0.92f, 0.03f, 0f);
        private static readonly string[] PreservedImportedRuntimeScriptPrefixes =
        {
            "Assets/_Imported/AssetStore/MagicaCloth2/"
        };
        private const string BossProxyVisualMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_BossProxy.mat";
        private const string BossTelegraphMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_BossBarrageIncomingTelegraph.mat";
        private const string LaneRailMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_BossBarrageLaneRail.mat";
        private const string PlayerBoundaryMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_BossBarragePlayerBoundary.mat";
        private const string SummonBoundaryMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_BossBarrageSummonBoundary.mat";
        private const string LaneAmbientFlowMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_BossBarrageLaneAmbientFlow.mat";
        private const string BossPressureHorizonMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_BossBarragePressureHorizon.mat";
        private const string SummonRouteWispMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_BossBarrageSummonRouteWisp.mat";
        private const string AmbientArenaStormClipPath =
            "Assets/_Game/Art/Audio/Ambience/DB_AMB_BossBarrage_ArenaStorm.mp3";
        private const string AmbientRailDustFlowClipPath =
            "Assets/_Game/Art/Audio/Ambience/DB_AMB_BossBarrage_RailDustFlow.wav";
        private const string AmbientArenaEnergyWindClipPath =
            "Assets/_Game/Art/Audio/Ambience/DB_AMB_Arena_EnergyWind_01.mp3";
        private const string AmbientArenaEnergyWaveClipPath =
            "Assets/_Game/Art/Audio/Ambience/DB_AMB_Arena_EnergyWave_01.mp3";
        private static readonly string[] PlayerFootstepClipPaths =
        {
            "Assets/_Game/Art/Audio/SFX/Footsteps/DB_SFX_Footstep_PlayerBootHardGround_01.wav",
            "Assets/_Game/Art/Audio/SFX/Footsteps/DB_SFX_Footstep_PlayerBootHardGround_02.wav",
            "Assets/_Game/Art/Audio/SFX/Footsteps/DB_SFX_Footstep_PlayerBootHardGround_03.wav"
        };

        private static readonly string[] ArmoredFootstepClipPaths =
        {
            "Assets/_Game/Art/Audio/SFX/Footsteps/DB_SFX_Footstep_ArmoredMedium_01.wav",
            "Assets/_Game/Art/Audio/SFX/Footsteps/DB_SFX_Footstep_ArmoredMedium_02.wav",
            "Assets/_Game/Art/Audio/SFX/Footsteps/DB_SFX_Footstep_ArmoredMedium_03.wav"
        };

        private static readonly string[] HeavyFootstepClipPaths =
        {
            "Assets/_Game/Art/Audio/SFX/Footsteps/DB_SFX_Footstep_HeavyGround_01.wav",
            "Assets/_Game/Art/Audio/SFX/Footsteps/DB_SFX_Footstep_HeavyGround_02.wav",
            "Assets/_Game/Art/Audio/SFX/Footsteps/DB_SFX_Footstep_HeavyGround_03.wav"
        };

        private static readonly Vector3 PlayerStartPosition = new Vector3(0f, 0f, -8.5f);
        private static readonly Vector3 CameraStartOffset = new Vector3(0.14f, 0.68f, -4.25f);
        private static readonly Vector3 CameraLookOffset = new Vector3(0f, 1.18f, 1.5f);
        private static readonly Vector3 CameraAimOffset = new Vector3(0.45f, 0.88f, 2.72f);
        private static readonly Vector3 CameraAimFocusOffset = new Vector3(0.89f, 0.06f, 1.05f);
        private const float CameraAimFieldOfViewDelta = -5.5f;
        private const float CameraAimBlendInSpeed = 14f;
        private const float CameraAimBlendOutSpeed = 18f;
        private const float CameraAimYawLimitDegrees = 45f;
        private const float CameraAimPitchLimitDegrees = 16f;
        private static readonly string[] RequiredBossPatternCueIds =
        {
            "NeedleLock",
            "CoverFire",
            "EscortScreen",
            "LayeredSalvo",
            "StaggeredCrossfire",
            "TwinSweep",
            "LeftClamp",
            "RightClamp",
            "PunishNet",
            "LinePressure"
        };
        private static readonly BossPressureActionKind[] RequiredBossPressureActionCueKinds =
        {
            BossPressureActionKind.SpecialSkill,
            BossPressureActionKind.SummonPressure,
            BossPressureActionKind.PunishOverextend
        };
        private static readonly int[] BossEnemySummonPacingTierSequence = { 1, 1, 2, 1 };

        [MenuItem("DimensionBrawl/Reapply Action Foundation Boss Barrage Lane Review Scene")]
        public static void ReapplyBossBarrageLaneReviewSceneMenu()
        {
            EnsureBossBarrageLaneReviewScene();
            Debug.Log("Reapplied ActionFoundation boss barrage lane review scene.");
        }

        [MenuItem("DimensionBrawl/Patch Action Foundation Boss Barrage Boss Commando Visual")]
        public static void PatchBossBarrageLaneReviewBossCommandoVisualMenu()
        {
            PatchBossBarrageLaneReviewBossCommandoVisual();
            Debug.Log("Patched ActionFoundation boss barrage boss visual to SciFiSoldier01 Commando.");
        }

        public static void PatchBossBarrageLaneReviewBossCommandoVisual()
        {
            string[] scenePaths =
            {
                ReviewScenePath,
                DuelReviewScenePath,
                ActionFoundationFrontlineMotivationReviewSetup.ScenePath,
                OlympusInvasionStageScenePath
            };

            for (int i = 0; i < scenePaths.Length; i++)
            {
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePaths[i]) == null)
                {
                    continue;
                }

                PatchBossBarrageLaneReviewBossCommandoVisual(scenePaths[i]);
            }
        }

        private static void PatchBossBarrageLaneReviewBossCommandoVisual(string scenePath)
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            GameObject bossProxy = RequireSceneObject(scene, BossProxyRootName);
            ReplaceBossProxyHumanoidVisual(bossProxy);

            BossBarrageEmitter emitter = RequireComponent<BossBarrageEmitter>(bossProxy, "boss barrage emitter");
            BossPressureActionDirector bossPressureActionDirector =
                RequireComponent<BossPressureActionDirector>(bossProxy, "boss pressure action director");
            ConfigureBossProxyVisualCueDriver(bossProxy, emitter, bossPressureActionDirector);

            BossBarrageVisualCueDriver cueDriver =
                RequireComponent<BossBarrageVisualCueDriver>(bossProxy, "boss barrage visual cue driver");
            CombatVfxCuePlayer playerCuePlayer = ResolveScenePlayerCuePlayer(scene, cueDriver.CuePlayer);
            if (playerCuePlayer == null)
            {
                throw new InvalidOperationException($"Missing player combat VFX cue player in {scenePath}.");
            }

            ConfigureBossProxyWorldVfxCueDriver(
                bossProxy,
                playerCuePlayer,
                ResolveScenePlayerDirectionTarget(scene, playerCuePlayer));
            ValidateBossProxyVisual(bossProxy, playerCuePlayer);

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, scenePath))
            {
                throw new InvalidOperationException($"Failed to save boss commando visual patch at {scenePath}.");
            }
        }

        [MenuItem("DimensionBrawl/Refresh Action Foundation Boss Barrage Lane Review Ambient Audio")]
        public static void RefreshBossBarrageLaneReviewAmbientAudioMenu()
        {
            RefreshBossBarrageLaneReviewAmbientAudio();
            Debug.Log("Refreshed ActionFoundation boss barrage lane review ambient audio.");
        }

        [MenuItem("DimensionBrawl/Refresh Action Foundation Boss Barrage Lane Review Footstep Audio")]
        public static void RefreshBossBarrageLaneReviewFootstepAudioMenu()
        {
            RefreshBossBarrageLaneReviewFootstepAudio();
            Debug.Log("Refreshed ActionFoundation boss barrage lane review footstep audio.");
        }

        [MenuItem("DimensionBrawl/Reapply Action Foundation Boss Barrage Lane Review Balance")]
        public static void ReapplyBossBarrageLaneReviewBalanceMenu()
        {
            EnsureBossBarrageLaneReviewBalance(ReviewScenePath);
            Debug.Log("Reapplied ActionFoundation boss barrage lane review balance tuning.");
        }

        [MenuItem("DimensionBrawl/Reapply Action Foundation Boss Enemy Summon Pacing")]
        public static void ReapplyBossBarrageLaneReviewEnemySummonPacingMenu()
        {
            PatchBossBarrageLaneReviewEnemySummonPacing(ReviewScenePath);
            Debug.Log("Reapplied ActionFoundation boss enemy summon pacing.");
        }

        [MenuItem("DimensionBrawl/Rebind Action Foundation Boss Barrage Lane Review Single Character Mode")]
        public static void RebindBossBarrageLaneReviewSingleCharacterModeMenu()
        {
            Scene scene = EditorSceneManager.OpenScene(ReviewScenePath, OpenSceneMode.Single);
            RebindBossBarrageLaneReviewSingleCharacterMode(scene);
            if (!EditorSceneManager.SaveScene(scene, ReviewScenePath))
            {
                throw new InvalidOperationException($"Failed to save boss barrage lane single-character combat binding at {ReviewScenePath}.");
            }

            Debug.Log("Rebound ActionFoundation boss barrage lane review single-character combat mode.");
        }

        [MenuItem("DimensionBrawl/Reapply Action Foundation Boss Basic Fire Bindings")]
        public static void ReapplyBossBasicFireBindingsMenu()
        {
            EnsureBossBasicFireBindings();
            Debug.Log("Reapplied ActionFoundation boss basic fire bindings.");
        }

        [MenuItem("DimensionBrawl/Validate Action Foundation Boss Barrage Lane Review Scene")]
        public static void ValidateBossBarrageLaneReviewSceneMenu()
        {
            ValidateBossBarrageLaneReviewScene();
            Debug.Log("ActionFoundation boss barrage lane review scene validation passed.");
        }

        [MenuItem("DimensionBrawl/Reapply Action Foundation Boss Summon Duel Review Scene")]
        public static void ReapplyBossSummonDuelReviewSceneMenu()
        {
            EnsureBossSummonDuelReviewScene();
            Debug.Log("Reapplied ActionFoundation boss summon duel review scene.");
        }

        [MenuItem("DimensionBrawl/Validate Action Foundation Boss Summon Duel Review Scene")]
        public static void ValidateBossSummonDuelReviewSceneMenu()
        {
            ValidateBossSummonDuelReviewScene();
            Debug.Log("ActionFoundation boss summon duel review scene validation passed.");
        }

        [MenuItem("DimensionBrawl/Reapply Action Foundation Boss Proxy Body Hitboxes")]
        public static void ReapplyBossProxyBodyHitboxesMenu()
        {
            EnsureBossProxyBodyHitboxes();
            Debug.Log("Reapplied ActionFoundation boss proxy body hitboxes.");
        }

        [MenuItem("DimensionBrawl/Reapply Action Foundation Boss Summon Duel Review End State")]
        public static void ReapplyBossSummonDuelReviewEndStateMenu()
        {
            EnsureBossSummonDuelReviewEndStateBindings();
            Debug.Log("Reapplied ActionFoundation boss summon duel review end-state bindings.");
        }

        [MenuItem("DimensionBrawl/Reapply Action Foundation Player Summon Presentation")]
        public static void ReapplyPlayerSummonPresentationMenu()
        {
            EnsureSummonEntryCuePrefab();
            EnsureSummonActorPrefab();
            EnsureSummonSlot2ActorPrefab();
            EnsureSummonSlot3ActorPrefab();
            EnsureSupportSummonActionProfiles();
            EnsureSummonPresentationCandidateProfiles();
            EnsurePlayerSummonReviewHudBindings(ReviewScenePath);
            EnsurePlayerSummonReviewHudBindings(DuelReviewScenePath);
            Debug.Log("Reapplied ActionFoundation player summon presentation assets.");
        }

        [MenuItem("DimensionBrawl/Reapply Action Foundation Boss Summon Presentation")]
        public static void ReapplyBossSummonPresentationMenu()
        {
            EnsureBossSummonPressureActorPrefab();
            EnsureBossSummonPressureProfile();
            EnsureSummonPresentationCandidateProfiles();
            Debug.Log("Reapplied ActionFoundation boss summon presentation assets.");
        }

        public static void EnsureBossProxyBodyHitboxes()
        {
            EnsureBossProxyBodyHitbox(ReviewScenePath);
            EnsureBossProxyBodyHitbox(DuelReviewScenePath);
        }

        public static void EnsureBossBasicFireBindings()
        {
            EnsureBossBasicFireProfile();
            EnsureBossBasicFireBinding(ReviewScenePath);
            EnsureBossBasicFireBinding(DuelReviewScenePath);
            AssetDatabase.SaveAssets();
        }

        [MenuItem("DimensionBrawl/Reapply Action Foundation Boss Projectile VFX")]
        public static void ReapplyBossProjectileVfxMenu()
        {
            EnsureBossProjectileVfx();
            Debug.Log("Reapplied ActionFoundation boss projectile VFX assets.");
        }

        [MenuItem("DimensionBrawl/Reapply Action Foundation Boss Missile Projectile And Basic Fire Bindings")]
        public static void ReapplyBossMissileProjectileAndBasicFireBindingsMenu()
        {
            EnsureBossProjectileVfx();
            EnsureBossBasicFireBindings();
            Debug.Log("Reapplied ActionFoundation boss missile projectile VFX and basic fire bindings.");
        }

        public static void EnsureBossProjectileVfx()
        {
            EnsureProjectilePrefab();
            EnsureBossBasicFireProfile();
            AssetDatabase.SaveAssets();
        }

        [MenuItem("DimensionBrawl/Reapply Action Foundation Boss Barrage Combat Cue Asset Overlays")]
        public static void ReapplyBossBarrageCombatCueAssetOverlaysMenu()
        {
            EnsureBossBarrageCombatCueAssetOverlays();
            AssetDatabase.SaveAssets();
            Debug.Log("Reapplied ActionFoundation boss barrage combat cue asset overlays.");
        }

        [MenuItem("DimensionBrawl/Reapply Action Foundation Boss Projectile And Perfect Dodge Shield VFX")]
        public static void ReapplyBossProjectileAndPerfectDodgeShieldVfxMenu()
        {
            ActionFoundationCombatVfxSetup.EnsureCombatVfxAssets();
            EnsureBossProjectileVfx();
            EnsureBossBarrageCombatCueAssetOverlays();
            AssetDatabase.SaveAssets();
            Debug.Log("Reapplied ActionFoundation boss projectile and perfect dodge shield VFX.");
        }

        [MenuItem("DimensionBrawl/Reapply Action Foundation Player Ranged Basic VFX")]
        public static void ReapplyPlayerRangedBasicVfxMenu()
        {
            EnsurePlayerRangedBasicVfx();
            Debug.Log("Reapplied ActionFoundation player ranged basic VFX assets and bindings.");
        }

        public static void EnsurePlayerRangedBasicVfx()
        {
            ActionFoundationCombatVfxSetup.EnsureCombatVfxAssets();
            EnsureLaneActionProjectilePrefab(
                RangedBasicProjectilePrefabPath,
                "PF_PlayerRangedBasicProjectile_AimBolt",
                RangedBasicProjectileMaterialPath,
                new Color(0.24f, 0.92f, 1f, 1f),
                0.28f,
                true);
            EnsurePlayerRangedBasicVfxBinding(ReviewScenePath);
            EnsurePlayerRangedBasicVfxBinding(DuelReviewScenePath);
            AssetDatabase.SaveAssets();
        }

        [MenuItem("DimensionBrawl/Reapply Action Foundation Player Skill1 Laser")]
        public static void ReapplyPlayerSkill1LaserMenu()
        {
            EnsurePlayerSkill1Laser();
            Debug.Log("Reapplied ActionFoundation player Skill1 laser VFX assets and bindings.");
        }

        public static void EnsurePlayerSkill1Laser()
        {
            EnsureSummonSlot2PromotedLaserBeamPrefab();
            EnsureLaneActionProjectilePrefab(
                Skill1ProjectilePrefabPath,
                "PF_PlayerSkill1Projectile_LaneBolt",
                Skill1ProjectileMaterialPath,
                new Color(0.45f, 0.9f, 1f, 1f),
                0.42f,
                allowVerticalTravel: false);
            AssetDatabase.SaveAssets();
        }

        private static void EnsurePlayerRangedBasicVfxBinding(string scenePath)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) == null)
            {
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            PlayerMovementController player = RequireObject<PlayerMovementController>(scene, "player movement");
            PlayerRangedBasicAttackAction rangedBasicAttackAction =
                RequireComponent<PlayerRangedBasicAttackAction>(player.gameObject, "player ranged basic attack action");
            CombatVfxCuePlayer cuePlayer =
                RequireComponent<CombatVfxCuePlayer>(player.gameObject, "player combat VFX cue player");
            Transform fireOrigin = RequireReferencedObject<Transform>(rangedBasicAttackAction, "fireOrigin");
            ConfigurePlayerRangedBasicVfxCueDriver(player.gameObject, rangedBasicAttackAction, cuePlayer, fireOrigin);

            if (!EditorSceneManager.SaveScene(scene, scenePath))
            {
                throw new InvalidOperationException($"Failed to save player ranged basic VFX bindings in {scenePath}.");
            }
        }

        private static void EnsureBossBasicFireBinding(string scenePath)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) == null)
            {
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            PlayerMovementController player = RequireObject<PlayerMovementController>(scene, "player movement");
            SummonLaneSpace laneSpace = RequireComponent<SummonLaneSpace>(RequireRoot(scene, LaneRootName), "lane space");
            GameObject bossProxy = RequireRoot(scene, BossProxyRootName);
            CombatHealth bossHealth = RequireComponent<CombatHealth>(bossProxy, "boss proxy health");
            Transform projectileRoot = RequireRoot(scene, ProjectilePoolRootName).transform;
            BossBasicFireEmitter bossBasicFireEmitter = ConfigureBossBasicFireEmitter(
                bossProxy,
                laneSpace,
                player.transform,
                bossHealth,
                projectileRoot);

            GameObject pocketRoot = FindRoot(scene, PocketOwnerRootName);
            if (pocketRoot != null
                && pocketRoot.TryGetComponent(out BossBarragePocketReviewOwner pocketOwner))
            {
                SetObjectReference(pocketOwner, "bossBasicFireEmitter", bossBasicFireEmitter);
            }

            GameObject duelRoot = FindRoot(scene, DuelOwnerRootName);
            if (duelRoot != null
                && duelRoot.TryGetComponent(out BossSummonDuelReviewOwner duelOwner))
            {
                SetObjectReference(duelOwner, "bossBasicFireEmitter", bossBasicFireEmitter);
            }

            GameObject hudRoot = FindRoot(scene, HudRootName);
            if (hudRoot != null
                && hudRoot.TryGetComponent(out BossBarrageLaneReviewHud reviewHud))
            {
                SetObjectReference(reviewHud, "bossBasicFireEmitter", bossBasicFireEmitter);
            }

            if (!EditorSceneManager.SaveScene(scene, scenePath))
            {
                throw new InvalidOperationException($"Failed to save boss basic fire bindings in {scenePath}.");
            }
        }

        private static void EnsureBossBarrageLaneReviewBalance(string scenePath)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) == null)
            {
                return;
            }

            EnsureSupportSummonActionProfiles();

            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            PlayerMovementController player = RequireObject<PlayerMovementController>(scene, "player movement");
            PlayerSkill1Action skill1Action =
                RequireComponent<PlayerSkill1Action>(player.gameObject, "player Skill1 action");
            PlayerSummonSlot1Action summonSlot1Action =
                RequireComponent<PlayerSummonSlot1Action>(player.gameObject, "player SummonSlot1 action");
            SummonEnergyLadder energyLadder =
                RequireComponent<SummonEnergyLadder>(player.gameObject, "summon energy ladder");
            GameObject bossProxy = RequireRoot(scene, BossProxyRootName);
            CombatHealth bossHealth = RequireComponent<CombatHealth>(bossProxy, "boss proxy health");
            GameObject closeThreat = RequireRoot(scene, CloseThreatRootName);
            CombatHealth closeThreatHealth = RequireComponent<CombatHealth>(closeThreat, "close threat health");

            ApplyPlayerSummonEnergyTuning(energyLadder);
            SetFloat(bossHealth, "maxHealth", BossProxyReviewMaxHealth);
            ConfigureSkill1TierSettings(skill1Action);
            summonSlot1Action.ConfigureSummonActionProfile(LoadAsset<SummonSlotActionProfile>(SummonSlot1ActionProfilePath));
            EditorUtility.SetDirty(summonSlot1Action);
            ConfigureCloseThreatBodyHitbox(closeThreat);
            ValidateCloseThreatBodyContract(closeThreat, closeThreatHealth);

            if (!EditorSceneManager.SaveScene(scene, scenePath))
            {
                throw new InvalidOperationException($"Failed to save boss barrage lane review balance tuning in {scenePath}.");
            }
        }

        private static void ApplyPlayerSummonEnergyTuning(SummonEnergyLadder energyLadder)
        {
            SetFloat(energyLadder, "baseEnergyPerSecond", PlayerSummonBaseEnergyPerSecond);
            SetFloat(energyLadder, "backSafetyGainScale", PlayerSummonBackSafetyGainScale);
            SetFloat(energyLadder, "midChargeGainScale", PlayerSummonMidChargeGainScale);
            SetFloat(energyLadder, "forwardRiskGainScale", PlayerSummonForwardRiskGainScale);
        }

        private static void EnsureBossProxyBodyHitbox(string scenePath)
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            GameObject bossProxy = RequireRoot(scene, BossProxyRootName);
            CombatHealth bossHealth = RequireComponent<CombatHealth>(bossProxy, "boss proxy health");
            ConfigureBossProxyBodyHitbox(bossProxy);
            ValidateBossProxyBodyContract(bossProxy, bossHealth);

            if (!EditorSceneManager.SaveScene(scene, scenePath))
            {
                throw new InvalidOperationException($"Failed to save boss proxy body hitbox in {scenePath}.");
            }
        }

        public static void EnsureBossBarrageLaneReviewScene()
        {
            ActionFoundationPlayerCombatModeAssetSetup.EnsureRangedCandidateAssets();
            ActionFoundationCombatVfxSetup.EnsureCombatVfxAssets();
            EnsureBossBarrageCombatCueAssetOverlays();
            BossBarragePatternProfile patternProfile = EnsurePatternProfile();
            BossBarragePatternProfile coverFirePatternProfile = EnsureCoverFirePatternProfile();
            BossBarragePatternProfile escortScreenPatternProfile = EnsureEscortScreenPatternProfile();
            BossBarragePatternProfile layeredSalvoPatternProfile = EnsureLayeredSalvoPatternProfile();
            BossBarragePatternProfile staggeredCrossfirePatternProfile = EnsureStaggeredCrossfirePatternProfile();
            BossBarragePatternProfile twinSweepPatternProfile = EnsureTwinSweepPatternProfile();
            BossBarragePatternProfile leftClampPatternProfile = EnsureLeftClampPatternProfile();
            BossBarragePatternProfile rightClampPatternProfile = EnsureRightClampPatternProfile();
            BossBarragePatternProfile punishNetPatternProfile = EnsurePunishNetPatternProfile();
            BossBarragePatternProfile linePressurePatternProfile = EnsureLinePressurePatternProfile();
            BossBasicFireProfile bossBasicFireProfile = EnsureBossBasicFireProfile();
            BossBarrageProjectile projectilePrefab = EnsureProjectilePrefab();
            PlayerActionProfile localDefenseProfile = EnsureLocalDefenseProfile();
            LaneActionProjectile skill1ProjectilePrefab = EnsureLaneActionProjectilePrefab(
                Skill1ProjectilePrefabPath,
                "PF_PlayerSkill1Projectile_LaneBolt",
                Skill1ProjectileMaterialPath,
                new Color(0.45f, 0.9f, 1f, 1f),
                0.42f,
                allowVerticalTravel: false);
            LaneActionProjectile rangedBasicProjectilePrefab = EnsureLaneActionProjectilePrefab(
                RangedBasicProjectilePrefabPath,
                "PF_PlayerRangedBasicProjectile_AimBolt",
                RangedBasicProjectileMaterialPath,
                new Color(0.75f, 0.98f, 1f, 1f),
                0.28f,
                allowVerticalTravel: true);
            LaneActionProjectile summonSlot1ProjectilePrefab = EnsureLaneActionProjectilePrefab(
                SummonSlot1ProjectilePrefabPath,
                "PF_SummonSlot1Projectile_AssistBolt",
                SummonSlot1ProjectileMaterialPath,
                new Color(0.55f, 1f, 0.72f, 1f),
                0.58f,
                allowVerticalTravel: false);
            LaneActionProjectile summonSlot2ProjectilePrefab = EnsureLaneActionProjectilePrefab(
                SummonSlot2ProjectilePrefabPath,
                "PF_SummonSlot2Projectile_LaserBolt",
                SummonSlot2ProjectileMaterialPath,
                new Color(0.86f, 0.94f, 1f, 1f),
                0.36f,
                allowVerticalTravel: false);
            LaneActionProjectile summonSlot3ProjectilePrefab = EnsureLaneActionProjectilePrefab(
                SummonSlot3ProjectilePrefabPath,
                "PF_SummonSlot3Projectile_FireBreath",
                SummonSlot3ProjectileMaterialPath,
                new Color(1f, 0.82f, 0.38f, 1f),
                0.64f,
                allowVerticalTravel: false);
            GameObject summonEntryCuePrefab = EnsureSummonEntryCuePrefab();
            SummonFrontlineProxy summonActorPrefab = EnsureSummonActorPrefab();
            SummonFrontlineProxy summonSlot2ActorPrefab = EnsureSummonSlot2ActorPrefab();
            SummonFrontlineProxy summonSlot3ActorPrefab = EnsureSummonSlot3ActorPrefab();
            SummonFrontlineProxy bossSummonActorPrefab = EnsureBossSummonPressureActorPrefab();
            EnsureSupportSummonActionProfiles();
            EnsureBossSummonPressureProfile();
            EnsureSummonPresentationCandidateProfiles();
            ActionCinematicCueProfile cinematicCueProfile = ActionFoundationProfileSetup.EnsureCinematicCueProfileAsset();
            Scene scene = EditorSceneManager.OpenScene(ActionFoundationProfileSetup.ScenePath, OpenSceneMode.Single);
            patternProfile = LoadAsset<BossBarragePatternProfile>(PatternProfilePath);
            coverFirePatternProfile = LoadAsset<BossBarragePatternProfile>(CoverFirePatternProfilePath);
            escortScreenPatternProfile = LoadAsset<BossBarragePatternProfile>(EscortScreenPatternProfilePath);
            layeredSalvoPatternProfile = LoadAsset<BossBarragePatternProfile>(LayeredSalvoPatternProfilePath);
            staggeredCrossfirePatternProfile = LoadAsset<BossBarragePatternProfile>(StaggeredCrossfirePatternProfilePath);
            twinSweepPatternProfile = LoadAsset<BossBarragePatternProfile>(TwinSweepPatternProfilePath);
            leftClampPatternProfile = LoadAsset<BossBarragePatternProfile>(LeftClampPatternProfilePath);
            rightClampPatternProfile = LoadAsset<BossBarragePatternProfile>(RightClampPatternProfilePath);
            punishNetPatternProfile = LoadAsset<BossBarragePatternProfile>(PunishNetPatternProfilePath);
            linePressurePatternProfile = LoadAsset<BossBarragePatternProfile>(LinePressurePatternProfilePath);
            bossBasicFireProfile = LoadAsset<BossBasicFireProfile>(BossBasicFireProfilePath);
            projectilePrefab = LoadPrefabComponent<BossBarrageProjectile>(ProjectilePrefabPath);
            localDefenseProfile = LoadAsset<PlayerActionProfile>(LocalDefenseProfilePath);
            skill1ProjectilePrefab = LoadPrefabComponent<LaneActionProjectile>(Skill1ProjectilePrefabPath);
            rangedBasicProjectilePrefab = LoadPrefabComponent<LaneActionProjectile>(RangedBasicProjectilePrefabPath);
            summonSlot1ProjectilePrefab = LoadPrefabComponent<LaneActionProjectile>(SummonSlot1ProjectilePrefabPath);
            summonSlot2ProjectilePrefab = LoadPrefabComponent<LaneActionProjectile>(SummonSlot2ProjectilePrefabPath);
            summonSlot3ProjectilePrefab = LoadPrefabComponent<LaneActionProjectile>(SummonSlot3ProjectilePrefabPath);
            summonEntryCuePrefab = LoadAsset<GameObject>(SummonSlot1EntryCuePrefabPath);
            summonActorPrefab = LoadPrefabComponent<SummonFrontlineProxy>(SummonSlot1ActorPrefabPath);
            summonSlot2ActorPrefab = LoadPrefabComponent<SummonFrontlineProxy>(SummonSlot2ActorPrefabPath);
            summonSlot3ActorPrefab = LoadPrefabComponent<SummonFrontlineProxy>(SummonSlot3ActorPrefabPath);
            bossSummonActorPrefab = LoadPrefabComponent<SummonFrontlineProxy>(BossSummonPressureActorPrefabPath);
            RemoveReviewAndEnemyRoots(scene);

            PlayerMovementController player = RequireObject<PlayerMovementController>(scene, "player movement");
            CombatHealth playerHealth = RequireComponent<CombatHealth>(player.gameObject, "player health");
            PlayerActionController playerActionController =
                RequireComponent<PlayerActionController>(player.gameObject, "player action controller");
            PlayerCombatTargetSelector targetSelector = RequireObject<PlayerCombatTargetSelector>(scene, "player target selector");
            ActionCameraController cameraController = RequireObject<ActionCameraController>(scene, "action camera");
            ActionCameraTargetBridge cameraTargetBridge = RequireObject<ActionCameraTargetBridge>(scene, "action camera target bridge");
            ActionFoundationTestEncounter encounter = RequireObject<ActionFoundationTestEncounter>(scene, "test encounter");

            SummonLaneSpace laneSpace = CreateLaneSpace(scene);
            player.transform.SetPositionAndRotation(PlayerStartPosition, Quaternion.LookRotation(Vector3.forward, Vector3.up));
            SetObjectReference(player, "laneSpace", laneSpace);

            SummonEnergyLadder energyLadder = EnsureComponent<SummonEnergyLadder>(player.gameObject);
            SetObjectReference(energyLadder, "laneSpace", laneSpace);
            SetObjectReference(energyLadder, "trackedPlayer", player.transform);
            ApplyPlayerSummonEnergyTuning(energyLadder);

            GameObject projectileRoot = CreateRoot(scene, ProjectilePoolRootName);
            GameObject actionCueRoot = CreateRoot(scene, ActionCuePoolRootName);
            GameObject summonActorRoot = CreateRoot(scene, SummonActorPoolRootName);
            GameObject bossSummonActorRoot = CreateRoot(scene, BossSummonActorPoolRootName);
            GameObject bossProxy = CreateBossProxy(
                scene,
                laneSpace,
                patternProfile,
                coverFirePatternProfile,
                escortScreenPatternProfile,
                layeredSalvoPatternProfile,
                staggeredCrossfirePatternProfile,
                twinSweepPatternProfile,
                leftClampPatternProfile,
                rightClampPatternProfile,
                punishNetPatternProfile,
                linePressurePatternProfile,
                bossBasicFireProfile,
                projectilePrefab,
                projectileRoot.transform,
                bossSummonActorPrefab,
                bossSummonActorRoot.transform);
            CombatHealth bossHealth = RequireComponent<CombatHealth>(bossProxy, "boss proxy health");
            GameObject closeThreat = CreateCloseThreat(scene, laneSpace, player.transform, playerHealth, cameraController);
            CombatHealth closeThreatHealth = RequireComponent<CombatHealth>(closeThreat, "close threat health");
            GameObject cinematicSupportDragon = CreateCinematicSupportDragon(scene, laneSpace);
            Animator cinematicSupportDragonAnimator =
                cinematicSupportDragon.GetComponentInChildren<Animator>(includeInactive: true);
            ConfigureLocalDefenseProfile(playerActionController, localDefenseProfile);
            ConfigurePlayerEnergyActions(
                player.gameObject,
                playerHealth,
                targetSelector,
                bossHealth,
                energyLadder,
                laneSpace,
                skill1ProjectilePrefab,
                summonSlot1ProjectilePrefab,
                summonSlot2ProjectilePrefab,
                summonSlot3ProjectilePrefab,
                summonEntryCuePrefab,
                summonActorPrefab,
                summonSlot2ActorPrefab,
                summonSlot3ActorPrefab,
                projectileRoot.transform,
                actionCueRoot.transform,
                summonActorRoot.transform);
            PlayerSkill1Action skill1Action = RequireComponent<PlayerSkill1Action>(player.gameObject, "player Skill1 action");
            PlayerSummonSlot1Action summonSlot1Action =
                RequireComponent<PlayerSummonSlot1Action>(player.gameObject, "player SummonSlot1 action");
            PlayerSupportSummonSlotAction summonSlot2Action =
                RequireSupportSummonSlotAction(player.gameObject, "SummonSlot2");
            PlayerSupportSummonSlotAction summonSlot3Action =
                RequireSupportSummonSlotAction(player.gameObject, "SummonSlot3");
            ConfigureTargetReferences(targetSelector, cameraTargetBridge, cameraController, player, playerHealth, closeThreatHealth, bossHealth);
            ConfigureEncounter(encounter, playerHealth, closeThreatHealth);
            BossBarrageEmitter bossBarrageEmitter = RequireComponent<BossBarrageEmitter>(bossProxy, "boss barrage emitter");
            BossBasicFireEmitter bossBasicFireEmitter =
                RequireComponent<BossBasicFireEmitter>(bossProxy, "boss basic fire emitter");
            BossPressureCostLadder bossPressureCost =
                RequireComponent<BossPressureCostLadder>(bossProxy, "boss pressure cost ladder");
            BossPressureActionDirector bossPressureActionDirector =
                RequireComponent<BossPressureActionDirector>(bossProxy, "boss pressure action director");
            BossSummonPressureAction bossSummonPressureAction =
                RequireComponent<BossSummonPressureAction>(bossProxy, "boss summon pressure action");
            FrontlineWaveStageProfile stageProfile = LoadAsset<FrontlineWaveStageProfile>(StageProfilePath);
            BossBarragePocketReviewOwner pocketOwner = CreatePocketOwner(
                scene,
                playerHealth,
                closeThreatHealth,
                bossHealth,
                energyLadder,
                skill1Action,
                summonSlot1Action,
                summonSlot2Action,
                summonSlot3Action,
                bossBarrageEmitter,
                bossBasicFireEmitter,
                stageProfile,
                bossPressureCost,
                bossPressureActionDirector,
                laneSpace);
            ConfigureFixedRearCamera(cameraController, player.transform, bossProxy.transform, laneSpace.transform);
            PlayerCombatModeVisualBinding combatModeVisuals = CreatePlayerCombatModeVisuals(scene, player.gameObject);
            ConfigurePlayerDamageShaderFeedback(player.gameObject, playerHealth, combatModeVisuals);
            ConfigureCombatModeController(player.gameObject, playerActionController, player, localDefenseProfile, combatModeVisuals);
            ConfigureRangedAimController(player.gameObject, cameraController, combatModeVisuals.RangedAnimator);
            PlayerCombatModeController combatModeController =
                RequireComponent<PlayerCombatModeController>(player.gameObject, "player combat mode controller");
            PlayerRangedAimController rangedAimController =
                RequireComponent<PlayerRangedAimController>(player.gameObject, "player ranged aim controller");
            PlayerRangedBasicAttackAction rangedBasicAttackAction = ConfigurePlayerRangedBasicAttack(
                player.gameObject,
                combatModeController,
                rangedAimController,
                player,
                targetSelector,
                playerHealth,
                cameraController,
                combatModeVisuals.RangedAnimator,
                rangedBasicProjectilePrefab,
                projectileRoot.transform,
                combatModeVisuals.RangedFireOrigin);
            CombatVfxCuePlayer playerCuePlayer =
                RequireComponent<CombatVfxCuePlayer>(player.gameObject, "player combat VFX cue player");
            ConfigurePlayerRangedBasicVfxCueDriver(
                player.gameObject,
                rangedBasicAttackAction,
                playerCuePlayer,
                combatModeVisuals.RangedFireOrigin);
            ConfigurePlayerCombatVfxCueDriver(
                player.gameObject,
                playerActionController,
                playerHealth,
                playerCuePlayer);
            ConfigureSummonEnergyVfxCuePresenter(
                player.gameObject,
                energyLadder,
                playerCuePlayer,
                bossProxy.transform);
            ConfigurePerfectDodgeTimeWarp(player.gameObject, playerActionController);
            ConfigureBossProxyWorldVfxCueDriver(
                bossProxy,
                playerCuePlayer,
                player.transform);
            ConfigureCombatModeActionLinks(combatModeController, rangedAimController, rangedBasicAttackAction);
            if (combatModeVisuals.NativeAnimatorBridge == null)
            {
                throw new InvalidOperationException("RifleGirl ranged visual requires the native animator bridge.");
            }

            ConfigureRifleGirlNativeBridge(
                combatModeVisuals.NativeAnimatorBridge,
                combatModeVisuals.RangedAnimator,
                player,
                playerActionController,
                combatModeController,
                rangedAimController,
                rangedBasicAttackAction);
            CreateReviewHud(
                scene,
                playerHealth,
                closeThreatHealth,
                bossHealth,
                energyLadder,
                laneSpace,
                player.transform,
                combatModeController,
                rangedAimController,
                rangedBasicAttackAction,
                skill1Action,
                summonSlot1Action,
                summonSlot2Action,
                summonSlot3Action,
                bossBarrageEmitter,
                bossBasicFireEmitter,
                pocketOwner,
                stageProfile,
                bossPressureCost,
                RequireComponent<BossPressurePositionController>(bossBarrageEmitter.gameObject, "boss pressure position controller"),
                bossPressureActionDirector,
                bossSummonPressureAction);
            ConfigureActionCameraCueDriver(
                cameraController,
                playerActionController,
                player,
                skill1Action,
                summonSlot1Action,
                rangedBasicAttackAction,
                playerCuePlayer,
                combatModeVisuals.RangedAnimator,
                cinematicSupportDragonAnimator,
                cinematicSupportDragon.transform,
                cinematicCueProfile);
            ConfigurePocketCueBridges(
                pocketOwner,
                summonSlot1Action,
                RequireComponent<ActionCameraCueDriver>(cameraController.gameObject, "action camera cue driver"),
                RequireComponent<ActionCinematicCueDirector>(cameraController.gameObject, "action cinematic cue director"),
                RequireComponent<PlayerCombatVfxCueDriver>(player.gameObject, "player combat VFX cue driver"),
                RequireComponent<CombatVfxCuePlayer>(player.gameObject, "player combat VFX cue player"),
                bossProxy.transform);
            ConfigureBossBarrageCameraCueDriver(
                cameraController,
                bossBarrageEmitter,
                bossPressureActionDirector,
                player.transform);
            ConfigureArenaInfluenceTargets(scene, player.transform, bossProxy.transform, closeThreat.transform);
            CreateLaneMarkers(scene, laneSpace);
            CreateLaneAmbientVfx(scene, laneSpace);
            DeactivateArenaDressingVfx(scene);
            CreateLaneAmbientAudio(scene, laneSpace);
            CreateReviewSceneBgmSlot(scene);
            ConfigureBossBarrageLaneReviewFootstepAudio(scene);
            CreateBossBarrageTelegraphMarkers(scene, laneSpace, bossBarrageEmitter);
            // Keep the serialized default aligned with the ranged starting mode after all visual swaps are rebuilt.
            ConfigureLocalDefenseProfile(playerActionController, localDefenseProfile);

            if (!EditorSceneManager.SaveScene(scene, ReviewScenePath))
            {
                throw new InvalidOperationException($"Failed to save boss barrage lane review scene at {ReviewScenePath}.");
            }

            AssetDatabase.SaveAssets();
        }

        public static void ValidateBossBarrageLaneReviewScene()
        {
            Scene scene = EditorSceneManager.OpenScene(ReviewScenePath, OpenSceneMode.Single);
            PlayerMovementController player = RequireObject<PlayerMovementController>(scene, "player movement");
            CombatHealth playerHealth = RequireComponent<CombatHealth>(player.gameObject, "player health");
            PlayerActionController playerActionController =
                RequireComponent<PlayerActionController>(player.gameObject, "player action controller");
            PlayerCombatTargetSelector targetSelector = RequireObject<PlayerCombatTargetSelector>(scene, "player target selector");
            ActionCameraController cameraController = RequireObject<ActionCameraController>(scene, "action camera");
            ActionFoundationTestEncounter encounter = RequireObject<ActionFoundationTestEncounter>(scene, "test encounter");
            SummonEnergyLadder energyLadder = RequireComponent<SummonEnergyLadder>(player.gameObject, "summon energy ladder");
            SummonLaneSpace laneSpace = RequireComponent<SummonLaneSpace>(RequireRoot(scene, LaneRootName), "lane space");
            GameObject bossProxy = RequireRoot(scene, BossProxyRootName);
            BossBarrageEmitter emitter = RequireComponent<BossBarrageEmitter>(bossProxy, "boss barrage emitter");
            BossBasicFireEmitter bossBasicFireEmitter =
                RequireComponent<BossBasicFireEmitter>(bossProxy, "boss basic fire emitter");
            BossPressureCostLadder bossPressureCost =
                RequireComponent<BossPressureCostLadder>(bossProxy, "boss pressure cost ladder");
            BossPressureActionDirector bossPressureActionDirector =
                RequireComponent<BossPressureActionDirector>(bossProxy, "boss pressure action director");
            BossSummonPressureAction bossSummonPressureAction =
                RequireComponent<BossSummonPressureAction>(bossProxy, "boss summon pressure action");
            FrontlineWaveStageProfile stageProfile = LoadAsset<FrontlineWaveStageProfile>(StageProfilePath);
            BossPressurePositionController bossPressurePosition =
                RequireComponent<BossPressurePositionController>(bossProxy, "boss pressure position controller");
            CombatHealth bossHealth = RequireComponent<CombatHealth>(bossProxy, "boss proxy health");
            ValidateBossProxyBodyContract(bossProxy, bossHealth);
            ValidateBossProxyVisual(bossProxy);
            GameObject closeThreat = RequireRoot(scene, CloseThreatRootName);
            CombatHealth closeThreatHealth = RequireComponent<CombatHealth>(closeThreat, "close threat health");
            PlayerCombatModeController combatModeController =
                RequireComponent<PlayerCombatModeController>(player.gameObject, "player combat mode controller");
            PlayerRangedAimController rangedAimController =
                RequireComponent<PlayerRangedAimController>(player.gameObject, "player ranged aim controller");
            PlayerRangedBasicAttackAction rangedBasicAttackAction =
                RequireComponent<PlayerRangedBasicAttackAction>(player.gameObject, "player ranged basic attack action");
            ValidatePlayerDamageShaderFeedback(scene, player.gameObject, playerHealth, closeThreat, closeThreatHealth);
            PlayerSkill1Action skill1Action = RequireComponent<PlayerSkill1Action>(player.gameObject, "player Skill1 action");
            PlayerSummonSlot1Action summonSlot1Action =
                RequireComponent<PlayerSummonSlot1Action>(player.gameObject, "player SummonSlot1 action");
            PlayerSupportSummonSlotAction summonSlot2Action =
                RequireSupportSummonSlotAction(player.gameObject, "SummonSlot2");
            PlayerSupportSummonSlotAction summonSlot3Action =
                RequireSupportSummonSlotAction(player.gameObject, "SummonSlot3");
            BossBarragePocketReviewOwner pocketOwner =
                RequireComponent<BossBarragePocketReviewOwner>(RequireRoot(scene, PocketOwnerRootName), "boss barrage pocket owner");
            BossBarrageLaneReviewHud reviewHud =
                RequireComponent<BossBarrageLaneReviewHud>(RequireRoot(scene, HudRootName), "boss barrage review HUD");
            BossBarrageLaneReviewMobileHud mobileHud =
                RequireComponent<BossBarrageLaneReviewMobileHud>(RequireRoot(scene, HudRootName), "boss barrage mobile review HUD");
            ActionScreenCuePresenter screenCuePresenter =
                RequireComponent<ActionScreenCuePresenter>(RequireRoot(scene, HudRootName), "action screen cue presenter");
            ActionCameraCueDriver actionCameraCueDriver =
                RequireComponent<ActionCameraCueDriver>(cameraController.gameObject, "action camera cue driver");
            ActionCinematicCueDirector cinematicCueDirector =
                RequireComponent<ActionCinematicCueDirector>(cameraController.gameObject, "action cinematic cue director");
            PlayerCombatVfxCueDriver playerVfxCueDriver =
                RequireComponent<PlayerCombatVfxCueDriver>(player.gameObject, "player combat VFX cue driver");
            PerfectDodgeTimeWarp perfectDodgeTimeWarp =
                RequireComponent<PerfectDodgeTimeWarp>(player.gameObject, "perfect dodge time warp");
            CombatVfxCuePlayer playerCuePlayer =
                RequireComponent<CombatVfxCuePlayer>(player.gameObject, "player combat VFX cue player");
            PlayerRangedBasicVfxCueDriver rangedBasicVfxCueDriver =
                RequireComponent<PlayerRangedBasicVfxCueDriver>(player.gameObject, "player ranged basic VFX cue driver");
            PlayerRangedReloadSfxDriver rangedReloadSfxDriver =
                RequireComponent<PlayerRangedReloadSfxDriver>(player.gameObject, "player ranged reload SFX driver");
            GameObject cinematicSupportDragon = RequireRoot(scene, CinematicSupportDragonRootName);
            Animator cinematicSupportDragonAnimator =
                cinematicSupportDragon.GetComponentInChildren<Animator>(includeInactive: true)
                ?? throw new InvalidOperationException("Boss barrage cinematic support dragon is missing an Animator.");

            ValidateObjectReference(player, "laneSpace", laneSpace);
            ValidateObjectReference(playerActionController, "actionProfile", LoadAsset<PlayerActionProfile>(LocalDefenseProfilePath));
            ValidateCombatModeController(
                combatModeController,
                playerActionController,
                player,
                rangedAimController,
                rangedBasicAttackAction);
            Animator rangedAnimator = RequireReferencedObject<Animator>(combatModeController, "rangedAnimator");
            ValidateRangedAimController(
                rangedAimController,
                combatModeController,
                cameraController,
                rangedAnimator);
            Transform rangedFireOrigin = RequireReferencedObject<Transform>(rangedBasicAttackAction, "fireOrigin");
            ValidatePlayerRangedBasicAttack(
                rangedBasicAttackAction,
                combatModeController,
                rangedAimController,
                player,
                targetSelector,
                playerHealth,
                cameraController,
                rangedAnimator,
                RequireRoot(scene, ProjectilePoolRootName).transform,
                rangedFireOrigin);
            ValidateRangedBasicProjectilePrefab();
            ValidateBossBarrageCombatCueAssetOverlays();
            ValidatePromotedLaserLaneProjectilePrefab(
                Skill1ProjectilePrefabPath,
                "LaneActionProjectileVfx_PlayerSkill1LaserBeam_FORGE3D",
                "Skill1 laser bolt");
            ValidateMagicMissilesLaneProjectilePrefab(
                SummonSlot1ProjectilePrefabPath,
                "LaneActionProjectileVfx_MagicMissilesLightAssistBolt",
                "SummonSlot1 assist bolt");
            ValidatePrimitiveLaneProjectilePrefab(
                SummonSlot2ProjectilePrefabPath,
                "LaneActionProjectileVfx_LaserBolt",
                "SummonSlot2 laser bolt");
            ValidatePromotedLaserLaneProjectilePrefab(
                SummonSlot3ProjectilePrefabPath,
                "LaneActionProjectileVfx_DragonFireBreath_FORGE3D",
                "SummonSlot3 fire breath",
                minimumParticleSystems: 1);
            ValidatePlayerRangedBasicVfxCueDriver(
                rangedBasicVfxCueDriver,
                rangedBasicAttackAction,
                playerCuePlayer,
                rangedFireOrigin);
            ValidatePlayerRangedReloadSfxDriver(rangedReloadSfxDriver, rangedBasicAttackAction);
            ValidatePlayerCombatVfxCueDriver(
                playerVfxCueDriver,
                playerActionController,
                playerHealth,
                playerCuePlayer);
            ValidatePerfectDodgeTimeWarp(perfectDodgeTimeWarp, playerActionController);
            ValidateSummonEnergyVfxCuePresenter(
                RequireComponent<SummonEnergyVfxCuePresenter>(player.gameObject, "summon energy VFX cue presenter"),
                energyLadder,
                playerCuePlayer,
                bossProxy.transform,
                playerVfxCueDriver);
            RifleGirlNativeGameplayAnimatorBridge nativeBridge =
                RequireComponent<RifleGirlNativeGameplayAnimatorBridge>(
                    rangedAnimator.gameObject,
                    "RifleGirl native animator bridge");
            ValidateRifleGirlNativeBridge(
                nativeBridge,
                rangedAnimator,
                player,
                playerActionController,
                combatModeController,
                rangedAimController,
                rangedBasicAttackAction);
            ValidateObjectReference(energyLadder, "laneSpace", laneSpace);
            ValidateObjectReference(energyLadder, "trackedPlayer", player.transform);
            ValidateFloat(energyLadder, "baseEnergyPerSecond", PlayerSummonBaseEnergyPerSecond);
            ValidateFloat(energyLadder, "backSafetyGainScale", PlayerSummonBackSafetyGainScale);
            ValidateFloat(energyLadder, "midChargeGainScale", PlayerSummonMidChargeGainScale);
            ValidateFloat(energyLadder, "forwardRiskGainScale", PlayerSummonForwardRiskGainScale);
            ValidatePlayerEnergyActions(skill1Action, summonSlot1Action, energyLadder, playerHealth, targetSelector, bossHealth, laneSpace);
            ValidateSupportSummonSlotAction(
                summonSlot2Action,
                BossBarrageSummonReviewContract.Slot2ActionName,
                energyLadder,
                playerHealth,
                targetSelector,
                bossHealth,
                laneSpace,
                SummonSlot2ProjectilePrefabPath,
                SummonSlot2ActorPrefabPath,
                SummonSlot2ActorVisualName,
                SummonSlot2ActionProfilePath,
                BossBarrageSummonReviewContract.Slot2MinimumTier,
                BossBarrageSummonReviewContract.Slot2RequiredMana,
                170f,
                false,
                0.18f,
                1.05f,
                3);
            ValidateSupportSummonSlotAction(
                summonSlot3Action,
                BossBarrageSummonReviewContract.Slot3ActionName,
                energyLadder,
                playerHealth,
                targetSelector,
                bossHealth,
                laneSpace,
                SummonSlot3ProjectilePrefabPath,
                SummonSlot3ActorPrefabPath,
                SummonSlot3ActorVisualName,
                SummonSlot3ActionProfilePath,
                BossBarrageSummonReviewContract.Slot3MinimumTier,
                BossBarrageSummonReviewContract.Slot3RequiredMana,
                520f,
                false,
                0.65f,
                2.4f,
                1);
            ValidateObjectReference(emitter, "laneSpace", laneSpace);
            ValidateObjectReference(emitter, "trackedPlayer", player.transform);
            ValidateObjectReference(emitter, "sourceHealth", bossHealth);
            ValidateObjectReference(emitter, "patternProfile", LoadAsset<BossBarragePatternProfile>(PatternProfilePath));
            ValidateArrayReference(
                emitter,
                "patternSequence",
                0,
                LoadAsset<BossBarragePatternProfile>(LinePressurePatternProfilePath));
            ValidateArrayReference(
                emitter,
                "patternSequence",
                1,
                LoadAsset<BossBarragePatternProfile>(LayeredSalvoPatternProfilePath));
            ValidateArrayReference(
                emitter,
                "patternSequence",
                2,
                LoadAsset<BossBarragePatternProfile>(PatternProfilePath));
            ValidateArrayReference(
                emitter,
                "patternSequence",
                3,
                LoadAsset<BossBarragePatternProfile>(CoverFirePatternProfilePath));
            ValidateArrayReference(
                emitter,
                "patternSequence",
                4,
                LoadAsset<BossBarragePatternProfile>(EscortScreenPatternProfilePath));
            ValidateArrayReference(
                emitter,
                "patternSequence",
                5,
                LoadAsset<BossBarragePatternProfile>(StaggeredCrossfirePatternProfilePath));
            ValidateArrayReference(
                emitter,
                "patternSequence",
                6,
                LoadAsset<BossBarragePatternProfile>(TwinSweepPatternProfilePath));
            ValidateArrayReference(
                emitter,
                "patternSequence",
                7,
                LoadAsset<BossBarragePatternProfile>(LeftClampPatternProfilePath));
            ValidateArrayReference(
                emitter,
                "patternSequence",
                8,
                LoadAsset<BossBarragePatternProfile>(RightClampPatternProfilePath));
            ValidateArrayReference(
                emitter,
                "patternSequence",
                9,
                LoadAsset<BossBarragePatternProfile>(PunishNetPatternProfilePath));
            ValidateInt(emitter, "wavesPerPattern", 1);
            ValidateObjectReference(emitter, "projectilePrefabObject", LoadAsset<GameObject>(ProjectilePrefabPath));
            ValidateBossBarrageProjectilePrefab();
            ValidateBossBasicFire(
                bossBasicFireEmitter,
                laneSpace,
                player.transform,
                bossHealth,
                RequireRoot(scene, ProjectilePoolRootName).transform);
            ValidateBossPressureLoop(
                bossPressureCost,
                bossPressureActionDirector,
                bossBasicFireEmitter,
                bossSummonPressureAction,
                bossPressurePosition,
                laneSpace,
                bossProxy.transform,
                emitter,
                player.transform);
            ValidateObjectReference(targetSelector, "selfHealth", playerHealth);
            ValidateArrayReference(targetSelector, "targetCandidates", 0, closeThreatHealth);
            ValidateArrayReference(targetSelector, "targetCandidates", 1, bossHealth);
            ValidateCloseThreat(closeThreat, closeThreatHealth, playerHealth, cameraController);
            ValidateObjectReference(cameraController, "target", player.transform);
            ValidateObjectReference(cameraController, "threat", bossProxy.transform);
            ValidateActionCameraCueDriver(
                actionCameraCueDriver,
                playerActionController,
                player,
                cameraController,
                skill1Action,
                summonSlot1Action,
                cinematicCueDirector);
            ValidateActionCinematicCueDirector(
                cinematicCueDirector,
                cameraController,
                player.transform,
                player,
                playerActionController,
                skill1Action,
                summonSlot1Action,
                rangedBasicAttackAction,
                playerCuePlayer,
                rangedAnimator,
                cinematicSupportDragonAnimator,
                cinematicSupportDragon.transform);
            ValidateBossBarrageCameraCueDriver(
                RequireComponent<BossBarrageCameraCueDriver>(cameraController.gameObject, "boss barrage camera cue driver"),
                cameraController,
                emitter,
                player.transform);
            ValidateBossBarrageLaneTelegraphPresenter(
                RequireComponent<BossBarrageLaneTelegraphPresenter>(
                    RequireRoot(scene, BossTelegraphRootName),
                    "boss barrage lane telegraph presenter"),
                emitter,
                laneSpace);
            ValidateSuppressedSceneVfxRoot(scene, ArenaVfxRootName);
            ValidateSuppressedSceneVfxRoot(scene, MarkerRootName);
            ValidateLaneAmbientVfx(scene);
            ValidateLaneAmbientAudio(scene);
            ValidateReviewSceneBgmSlot(scene);
            ValidateBossBarrageLaneReviewFootstepAudio(scene);
            ValidateObjectReference(encounter, "playerHealth", playerHealth);
            ValidateObjectReference(encounter, "enemyHealth", closeThreatHealth);
            ValidatePocketOwner(
                pocketOwner,
                playerHealth,
                closeThreatHealth,
                bossHealth,
                energyLadder,
                skill1Action,
                summonSlot1Action,
                summonSlot2Action,
                summonSlot3Action,
                emitter,
                bossBasicFireEmitter,
                stageProfile,
                bossPressureCost,
                bossPressureActionDirector);
            ValidatePocketCueBridges(
                pocketOwner,
                summonSlot1Action,
                actionCameraCueDriver,
                cinematicCueDirector,
                playerVfxCueDriver,
                playerCuePlayer,
                bossProxy.transform);
            ValidateReviewHud(
                reviewHud,
                playerHealth,
                closeThreatHealth,
                bossHealth,
                energyLadder,
                laneSpace,
                player.transform,
                combatModeController,
                rangedAimController,
                rangedBasicAttackAction,
                skill1Action,
                summonSlot1Action,
                emitter,
                bossBasicFireEmitter,
                pocketOwner,
                stageProfile,
                bossPressureCost,
                bossPressurePosition,
                bossPressureActionDirector,
                bossSummonPressureAction,
                summonSlot2Action,
                summonSlot3Action);
            ValidateMobileReviewHud(
                mobileHud,
                player,
                playerActionController,
                combatModeController,
                rangedAimController,
                rangedBasicAttackAction,
                skill1Action,
                summonSlot1Action,
                summonSlot2Action,
                summonSlot3Action,
                energyLadder);
            ValidateActionScreenCuePresenter(
                screenCuePresenter,
                playerActionController,
                playerHealth,
                rangedBasicAttackAction,
                energyLadder,
                skill1Action,
                summonSlot1Action,
                summonSlot2Action,
                summonSlot3Action,
                emitter,
                bossPressureActionDirector,
                pocketOwner);
            ValidateFixedRearCamera(cameraController, player.transform, laneSpace.transform);
            ValidateSummonForwardSpace(laneSpace);
            ValidateSummonPresentationCandidateProfiles();
            ValidateBossProjectileForge3DAssetReference(ProjectilePrefabPath);
            ValidateNoImportedAssetReference(PatternProfilePath);
            ValidateNoImportedAssetReference(CoverFirePatternProfilePath);
            ValidateNoImportedAssetReference(EscortScreenPatternProfilePath);
            ValidateNoImportedAssetReference(LayeredSalvoPatternProfilePath);
            ValidateNoImportedAssetReference(StaggeredCrossfirePatternProfilePath);
            ValidateNoImportedAssetReference(TwinSweepPatternProfilePath);
            ValidateNoImportedAssetReference(LeftClampPatternProfilePath);
            ValidateNoImportedAssetReference(RightClampPatternProfilePath);
            ValidateNoImportedAssetReference(PunishNetPatternProfilePath);
            ValidateNoImportedAssetReference(LinePressurePatternProfilePath);
            ValidateNoImportedAssetReference(BossBasicFireProfilePath);
            ValidateNoImportedAssetReference(BossBasicFireProjectileMaterialPath);
            ValidateNoImportedAssetReference(BossBarrageProjectileTrailMaterialPath);
            ValidateNoImportedAssetReference(LocalDefenseProfilePath);
            ValidateNoImportedAssetReference(Skill1ProjectilePrefabPath);
            ValidateNoImportedAssetReference(RangedBasicProjectilePrefabPath);
            ValidateNoImportedAssetReference(SummonSlot1ProjectilePrefabPath);
            ValidateNoImportedAssetReference(SummonSlot2ProjectilePrefabPath);
            ValidateNoImportedAssetReference(SummonSlot3ProjectilePrefabPath);
            ValidateNoImportedAssetReference(SummonSlot1EntryCuePrefabPath);
            ValidateNoImportedAssetReference(SummonSlot1EntryCueAccentMaterialPath);
            ValidateNoImportedAssetReference(SummonSlot1ActorPrefabPath);
            ValidateNoImportedAssetReference(BossSummonPressureActorPrefabPath);
            ValidateNoImportedAssetReference(SummonSlot1PresentationCandidateProfilePath);
            ValidateNoImportedAssetReference(SummonSlot2PresentationCandidateProfilePath);
            ValidateNoImportedAssetReference(SummonSlot3PresentationCandidateProfilePath);
            ValidateNoImportedAssetReference(BossSummonPressurePresentationCandidateProfilePath);
            ValidateNoImportedAssetReference(SummonPressureScreenMaterialPath);
            ValidateNoImportedAssetReference(SummonSlot1ActorPulseMaterialPath);
            ValidateNoImportedAssetReference(SummonSlot1PromotedChargeImpactPrefabPath);
            ValidateNoImportedAssetReference(SummonSlot1PromotedRushTrailPrefabPath);
            ValidateNoImportedAssetReference(SummonSlot2LaserBeamMaterialPath);
            ValidateNoImportedAssetReference(SummonSlot3FireBreathMaterialPath);
            ValidateNoImportedAssetReference(SummonSlot3DragonBodyMaterialPath);
            ValidateNoImportedAssetReference(SummonSlot3DragonWingMaterialPath);
            ValidateNoImportedAssetReference(SummonSlot2PromotedLaserBeamPrefabPath);
            ValidateNoImportedAssetReference(SummonSlot3PromotedFireBreathPrefabPath);
            ValidateNoImportedAssetReference(SummonSlot3DragonVisualPrefabPath);
            ValidateNoImportedAssetReference(SummonSlot3DragonControllerPath);
            ValidateNoImportedAssetReference(BossSummonPressureActorMaterialPath);
            ValidateNoImportedAssetReference(BossSummonPressureScreenMaterialPath);
            ValidateNoImportedAssetReference(BossSummonPressureActorPulseMaterialPath);
            ValidateNoImportedAssetReference(BossTelegraphMaterialPath);
            ValidateNoImportedAssetReference(LaneAmbientFlowMaterialPath);
            ValidateNoImportedAssetReference(BossPressureHorizonMaterialPath);
            ValidateNoImportedAssetReference(SummonRouteWispMaterialPath);
            ValidateNoImportedAssetReference(AmbientArenaStormClipPath);
            ValidateNoImportedAssetReference(AmbientRailDustFlowClipPath);
            ValidateNoImportedAssetReference(AmbientArenaEnergyWindClipPath);
            ValidateNoImportedAssetReference(AmbientArenaEnergyWaveClipPath);
            ValidateNoImportedAssetReference(BossBasicFireSfxClipPath);
            ValidateNoImportedAssetReference(BossLaserTelegraphSfxClipPath);
            ValidateNoImportedAssetReference(PlayerRangedReloadSfxClipPath);
            ValidateNoImportedAssetReferences(PlayerFootstepClipPaths);
            ValidateNoImportedAssetReferences(ArmoredFootstepClipPaths);
            ValidateNoImportedAssetReferences(HeavyFootstepClipPaths);
        }

        private static void RefreshBossBarrageLaneReviewAmbientAudio()
        {
            Scene scene = EditorSceneManager.OpenScene(ReviewScenePath, OpenSceneMode.Single);
            SummonLaneSpace laneSpace = RequireObject<SummonLaneSpace>(scene, "summon lane space");
            CreateLaneAmbientAudio(scene, laneSpace);
            CreateReviewSceneBgmSlot(scene);
            ValidateLaneAmbientAudio(scene);
            ValidateReviewSceneBgmSlot(scene);

            if (!EditorSceneManager.SaveScene(scene, ReviewScenePath))
            {
                throw new InvalidOperationException($"Failed to save boss barrage lane ambient audio at {ReviewScenePath}.");
            }

            AssetDatabase.SaveAssets();
        }

        private static void RemoveLaneAmbientAudio(Scene scene)
        {
            GameObject existingRoot = FindRoot(scene, AmbientAudioRootName);
            if (existingRoot != null)
            {
                UnityEngine.Object.DestroyImmediate(existingRoot);
            }
        }

        private static void RefreshBossBarrageLaneReviewFootstepAudio()
        {
            AssetDatabase.Refresh();
            Scene scene = EditorSceneManager.OpenScene(ReviewScenePath, OpenSceneMode.Single);
            ConfigureBossBarrageLaneReviewFootstepAudio(scene);
            ValidateBossBarrageLaneReviewFootstepAudio(scene);

            if (!EditorSceneManager.SaveScene(scene, ReviewScenePath))
            {
                throw new InvalidOperationException($"Failed to save boss barrage lane footstep audio at {ReviewScenePath}.");
            }

            AssetDatabase.SaveAssets();
        }

        private static void ValidateNoImportedAssetReferences(IEnumerable<string> assetPaths)
        {
            foreach (string assetPath in assetPaths)
            {
                ValidateNoImportedAssetReference(assetPath);
            }
        }

        private static void ValidateBossProjectileForge3DAssetReference(string assetPath)
        {
            if (assetPath.Replace('\\', '/').Contains("/_Imported/", StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"{assetPath} must stay as a game-owned boss projectile asset.");
            }

            // Boss bullets intentionally use the exact FORGE3D missile mesh for silhouette readability.
            // Materials and runtime tinting stay game-owned and are validated on the projectile prefab.
        }

        public static void EnsureBossSummonDuelReviewScene()
        {
            EnsureBossBarrageLaneReviewScene();
            Scene scene = EditorSceneManager.OpenScene(ReviewScenePath, OpenSceneMode.Single);

            PlayerMovementController player = RequireObject<PlayerMovementController>(scene, "player movement");
            CombatHealth playerHealth = RequireComponent<CombatHealth>(player.gameObject, "player health");
            PlayerCombatTargetSelector targetSelector = RequireObject<PlayerCombatTargetSelector>(scene, "player target selector");
            ActionFoundationTestEncounter encounter = RequireObject<ActionFoundationTestEncounter>(scene, "test encounter");
            SummonEnergyLadder energyLadder = RequireComponent<SummonEnergyLadder>(player.gameObject, "summon energy ladder");
            PlayerSkill1Action skill1Action = RequireComponent<PlayerSkill1Action>(player.gameObject, "player Skill1 action");
            PlayerSummonSlot1Action summonSlot1Action =
                RequireComponent<PlayerSummonSlot1Action>(player.gameObject, "player SummonSlot1 action");
            PlayerSupportSummonSlotAction summonSlot2Action =
                RequireSupportSummonSlotAction(player.gameObject, "SummonSlot2");
            PlayerSupportSummonSlotAction summonSlot3Action =
                RequireSupportSummonSlotAction(player.gameObject, "SummonSlot3");
            SummonLaneSpace laneSpace = RequireObject<SummonLaneSpace>(scene, "summon lane space");
            GameObject bossProxy = RequireRoot(scene, BossProxyRootName);
            CombatHealth bossHealth = RequireComponent<CombatHealth>(bossProxy, "boss proxy health");
            ValidateBossProxyBodyContract(bossProxy, bossHealth);
            BossBarrageEmitter emitter = RequireComponent<BossBarrageEmitter>(bossProxy, "boss barrage emitter");
            BossBasicFireEmitter bossBasicFireEmitter =
                RequireComponent<BossBasicFireEmitter>(bossProxy, "boss basic fire emitter");
            BossPressureCostLadder bossPressureCost =
                RequireComponent<BossPressureCostLadder>(bossProxy, "boss pressure cost ladder");
            BossPressureActionDirector bossPressureActionDirector =
                RequireComponent<BossPressureActionDirector>(bossProxy, "boss pressure action director");
            BossSummonPressureAction bossSummonPressureAction =
                RequireComponent<BossSummonPressureAction>(bossProxy, "boss summon pressure action");
            GameObject closeThreat = RequireRoot(scene, CloseThreatRootName);
            GameObject pocketOwner = RequireRoot(scene, PocketOwnerRootName);
            BossBarrageLaneReviewHud reviewHud =
                RequireComponent<BossBarrageLaneReviewHud>(RequireRoot(scene, HudRootName), "boss barrage review HUD");

            closeThreat.SetActive(false);
            pocketOwner.SetActive(false);
            SetObjectReferenceArray(targetSelector, "targetCandidates", new UnityEngine.Object[] { bossHealth });
            SetObjectReference(encounter, "enemyHealth", bossHealth);

            GameObject duelOwnerRoot = CreateRoot(scene, DuelOwnerRootName);
            BossSummonDuelReviewOwner duelOwner = EnsureComponent<BossSummonDuelReviewOwner>(duelOwnerRoot);
            GameObject clearMarker = EnsureResultMarker(
                duelOwnerRoot.transform,
                DuelClearMarkerName,
                laneSpace.GetBattlefieldWorldPoint(-laneSpace.HalfWidth - 1.35f, laneSpace.ForwardBoundaryZ + 1.0f, 0.75f),
                new Color(0.25f, 1f, 0.5f, 1f));
            GameObject failMarker = EnsureResultMarker(
                duelOwnerRoot.transform,
                DuelFailMarkerName,
                laneSpace.GetBattlefieldWorldPoint(laneSpace.HalfWidth + 1.35f, laneSpace.ForwardBoundaryZ + 1.0f, 0.75f),
                new Color(1f, 0.16f, 0.18f, 1f));
            ConfigureBossSummonDuelOwner(
                duelOwner,
                playerHealth,
                bossHealth,
                energyLadder,
                skill1Action,
                summonSlot1Action,
                summonSlot2Action,
                summonSlot3Action,
                emitter,
                bossBasicFireEmitter,
                bossPressureCost,
                bossPressureActionDirector,
                bossSummonPressureAction,
                clearMarker,
                failMarker);

            SetObjectReference(reviewHud, "closeThreatHealth", null);
            SetObjectReference(reviewHud, "pocketReviewOwner", null);
            SetObjectReference(reviewHud, "duelReviewOwner", duelOwner);
            ActionScreenCuePresenter screenCuePresenter =
                RequireComponent<ActionScreenCuePresenter>(RequireRoot(scene, HudRootName), "action screen cue presenter");
            SetObjectReference(screenCuePresenter, "pocketReviewOwner", null);
            SetObjectReference(screenCuePresenter, "duelReviewOwner", duelOwner);
            ConfigurePerfectDodgeScreenCueMaterials(screenCuePresenter);

            if (!EditorSceneManager.SaveScene(scene, DuelReviewScenePath))
            {
                throw new InvalidOperationException($"Failed to save boss summon duel review scene at {DuelReviewScenePath}.");
            }

            AssetDatabase.SaveAssets();
        }

        public static void EnsureBossSummonDuelReviewEndStateBindings()
        {
            Scene scene = EditorSceneManager.OpenScene(DuelReviewScenePath, OpenSceneMode.Single);
            PlayerMovementController player = RequireObject<PlayerMovementController>(scene, "player movement");
            CombatHealth playerHealth = RequireComponent<CombatHealth>(player.gameObject, "player health");
            SummonEnergyLadder energyLadder = RequireComponent<SummonEnergyLadder>(player.gameObject, "summon energy ladder");
            PlayerSkill1Action skill1Action = RequireComponent<PlayerSkill1Action>(player.gameObject, "player Skill1 action");
            PlayerSummonSlot1Action summonSlot1Action =
                RequireComponent<PlayerSummonSlot1Action>(player.gameObject, "player SummonSlot1 action");
            PlayerSupportSummonSlotAction summonSlot2Action =
                RequireSupportSummonSlotAction(player.gameObject, "SummonSlot2");
            PlayerSupportSummonSlotAction summonSlot3Action =
                RequireSupportSummonSlotAction(player.gameObject, "SummonSlot3");
            SummonLaneSpace laneSpace = RequireObject<SummonLaneSpace>(scene, "summon lane space");
            GameObject bossProxy = RequireRoot(scene, BossProxyRootName);
            CombatHealth bossHealth = RequireComponent<CombatHealth>(bossProxy, "boss proxy health");
            BossBarrageEmitter emitter = RequireComponent<BossBarrageEmitter>(bossProxy, "boss barrage emitter");
            BossBasicFireEmitter bossBasicFireEmitter =
                RequireComponent<BossBasicFireEmitter>(bossProxy, "boss basic fire emitter");
            BossPressureCostLadder bossPressureCost =
                RequireComponent<BossPressureCostLadder>(bossProxy, "boss pressure cost ladder");
            BossPressureActionDirector bossPressureActionDirector =
                RequireComponent<BossPressureActionDirector>(bossProxy, "boss pressure action director");
            BossSummonPressureAction bossSummonPressureAction =
                RequireComponent<BossSummonPressureAction>(bossProxy, "boss summon pressure action");
            GameObject duelOwnerRoot = RequireRoot(scene, DuelOwnerRootName);
            BossSummonDuelReviewOwner duelOwner =
                RequireComponent<BossSummonDuelReviewOwner>(duelOwnerRoot, "boss summon duel owner");
            ActionScreenCuePresenter screenCuePresenter =
                RequireComponent<ActionScreenCuePresenter>(RequireRoot(scene, HudRootName), "action screen cue presenter");

            ApplyPlayerSummonEnergyTuning(energyLadder);
            GameObject clearMarker = EnsureResultMarker(
                duelOwnerRoot.transform,
                DuelClearMarkerName,
                laneSpace.GetBattlefieldWorldPoint(-laneSpace.HalfWidth - 1.35f, laneSpace.ForwardBoundaryZ + 1.0f, 0.75f),
                new Color(0.25f, 1f, 0.5f, 1f));
            GameObject failMarker = EnsureResultMarker(
                duelOwnerRoot.transform,
                DuelFailMarkerName,
                laneSpace.GetBattlefieldWorldPoint(laneSpace.HalfWidth + 1.35f, laneSpace.ForwardBoundaryZ + 1.0f, 0.75f),
                new Color(1f, 0.16f, 0.18f, 1f));

            ConfigureBossSummonDuelOwner(
                duelOwner,
                playerHealth,
                bossHealth,
                energyLadder,
                skill1Action,
                summonSlot1Action,
                summonSlot2Action,
                summonSlot3Action,
                emitter,
                bossBasicFireEmitter,
                bossPressureCost,
                bossPressureActionDirector,
                bossSummonPressureAction,
                clearMarker,
                failMarker);
            SetObjectReference(screenCuePresenter, "pocketReviewOwner", null);
            SetObjectReference(screenCuePresenter, "duelReviewOwner", duelOwner);
            ConfigurePerfectDodgeScreenCueMaterials(screenCuePresenter);

            if (!EditorSceneManager.SaveScene(scene, DuelReviewScenePath))
            {
                throw new InvalidOperationException($"Failed to save boss summon duel review scene at {DuelReviewScenePath}.");
            }

            AssetDatabase.SaveAssets();
        }

        private static void EnsurePlayerSummonReviewHudBindings(string scenePath)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) == null)
            {
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            PlayerMovementController player = RequireObject<PlayerMovementController>(scene, "player movement");
            SummonEnergyLadder energyLadder =
                RequireComponent<SummonEnergyLadder>(player.gameObject, "summon energy ladder");
            BossBarrageLaneReviewMobileHud mobileHud =
                RequireComponent<BossBarrageLaneReviewMobileHud>(
                    RequireRoot(scene, HudRootName),
                    "boss barrage mobile review HUD");

            SetObjectReference(mobileHud, "energyLadder", energyLadder);
            SetBool(mobileHud, "useSingleSummonButton", BossBarrageSummonReviewContract.UseSingleSummonButton);
            SetString(mobileHud, "summonSlot1Label", BossBarrageSummonReviewContract.Slot1HudLabel);
            SetString(mobileHud, "summonSlot2Label", BossBarrageSummonReviewContract.Slot2HudLabel);
            SetString(mobileHud, "summonSlot3Label", BossBarrageSummonReviewContract.Slot3HudLabel);
            SetString(mobileHud, "lockedSummonLabel", BossBarrageSummonReviewContract.LockedSummonLabel);
            EditorUtility.SetDirty(mobileHud);

            if (!EditorSceneManager.SaveScene(scene, scenePath))
            {
                throw new InvalidOperationException($"Failed to save player summon review HUD bindings at {scenePath}.");
            }
        }

        public static void ValidateBossSummonDuelReviewScene()
        {
            Scene scene = EditorSceneManager.OpenScene(DuelReviewScenePath, OpenSceneMode.Single);
            PlayerMovementController player = RequireObject<PlayerMovementController>(scene, "player movement");
            CombatHealth playerHealth = RequireComponent<CombatHealth>(player.gameObject, "player health");
            PlayerCombatTargetSelector targetSelector = RequireObject<PlayerCombatTargetSelector>(scene, "player target selector");
            ActionFoundationTestEncounter encounter = RequireObject<ActionFoundationTestEncounter>(scene, "test encounter");
            SummonEnergyLadder energyLadder = RequireComponent<SummonEnergyLadder>(player.gameObject, "summon energy ladder");
            PlayerSkill1Action skill1Action = RequireComponent<PlayerSkill1Action>(player.gameObject, "player Skill1 action");
            PlayerSummonSlot1Action summonSlot1Action =
                RequireComponent<PlayerSummonSlot1Action>(player.gameObject, "player SummonSlot1 action");
            PlayerSupportSummonSlotAction summonSlot2Action =
                RequireSupportSummonSlotAction(player.gameObject, "SummonSlot2");
            PlayerSupportSummonSlotAction summonSlot3Action =
                RequireSupportSummonSlotAction(player.gameObject, "SummonSlot3");
            GameObject bossProxy = RequireRoot(scene, BossProxyRootName);
            CombatHealth bossHealth = RequireComponent<CombatHealth>(bossProxy, "boss proxy health");
            BossBarrageEmitter emitter = RequireComponent<BossBarrageEmitter>(bossProxy, "boss barrage emitter");
            BossBasicFireEmitter bossBasicFireEmitter =
                RequireComponent<BossBasicFireEmitter>(bossProxy, "boss basic fire emitter");
            BossPressureCostLadder bossPressureCost =
                RequireComponent<BossPressureCostLadder>(bossProxy, "boss pressure cost ladder");
            BossPressureActionDirector bossPressureActionDirector =
                RequireComponent<BossPressureActionDirector>(bossProxy, "boss pressure action director");
            BossSummonPressureAction bossSummonPressureAction =
                RequireComponent<BossSummonPressureAction>(bossProxy, "boss summon pressure action");
            GameObject closeThreat = RequireRoot(scene, CloseThreatRootName);
            GameObject pocketOwner = RequireRoot(scene, PocketOwnerRootName);
            BossSummonDuelReviewOwner duelOwner =
                RequireComponent<BossSummonDuelReviewOwner>(RequireRoot(scene, DuelOwnerRootName), "boss summon duel owner");
            GameObject clearMarker = RequireChild(duelOwner.transform, DuelClearMarkerName).gameObject;
            GameObject failMarker = RequireChild(duelOwner.transform, DuelFailMarkerName).gameObject;
            BossBarrageLaneReviewHud reviewHud =
                RequireComponent<BossBarrageLaneReviewHud>(RequireRoot(scene, HudRootName), "boss barrage review HUD");
            ActionScreenCuePresenter screenCuePresenter =
                RequireComponent<ActionScreenCuePresenter>(RequireRoot(scene, HudRootName), "action screen cue presenter");

            if (closeThreat.activeSelf)
            {
                throw new InvalidOperationException("Boss summon duel scene should disable the close-threat pocket sample.");
            }

            if (pocketOwner.activeSelf)
            {
                throw new InvalidOperationException("Boss summon duel scene should disable the one-pocket review owner.");
            }

            SerializedProperty targetCandidates = RequireProperty(new SerializedObject(targetSelector), "targetCandidates");
            if (targetCandidates.arraySize != 1)
            {
                throw new InvalidOperationException("Boss summon duel target selector should keep only the far boss candidate.");
            }

            ValidateArrayReference(targetSelector, "targetCandidates", 0, bossHealth);
            ValidateObjectReference(encounter, "enemyHealth", bossHealth);
            ValidateBossSummonDuelOwner(
                duelOwner,
                playerHealth,
                bossHealth,
                energyLadder,
                skill1Action,
                summonSlot1Action,
                summonSlot2Action,
                summonSlot3Action,
                emitter,
                bossBasicFireEmitter,
                bossPressureCost,
                bossPressureActionDirector,
                bossSummonPressureAction,
                clearMarker,
                failMarker);
            ValidateObjectReference(reviewHud, "summonSlot2Action", summonSlot2Action);
            ValidateObjectReference(reviewHud, "summonSlot3Action", summonSlot3Action);
            ValidateObjectReference(reviewHud, "pocketReviewOwner", null);
            ValidateObjectReference(reviewHud, "duelReviewOwner", duelOwner);
            ValidateObjectReference(screenCuePresenter, "pocketReviewOwner", null);
            ValidateObjectReference(screenCuePresenter, "duelReviewOwner", duelOwner);
            ValidateStringContains(
                reviewHud.CompactObjectiveReadout,
                duelOwner.CompactObjectiveCue,
                "duel compact objective readout");
            ValidateStringContains(
                reviewHud.RouteIncentiveReadout,
                duelOwner.RouteIncentiveCue,
                "duel route incentive readout");
            ValidateStringContains(
                reviewHud.CompactCombatCueReadout,
                duelOwner.CompactObjectiveCue,
                "duel compact combat cue readout");
        }

        private static void ApplySkillGrammar(
            SerializedObject serializedObject,
            LaneSkillPatternFamily family,
            LaneSkillTransferMode transferMode,
            string playerSkillTranslationNote,
            string counterplayNote)
        {
            RequireProperty(serializedObject, "skillPatternFamily").enumValueIndex = (int)family;
            RequireProperty(serializedObject, "skillTransferMode").enumValueIndex = (int)transferMode;
            RequireProperty(serializedObject, "playerSkillTranslationNote").stringValue = playerSkillTranslationNote;
            RequireProperty(serializedObject, "counterplayNote").stringValue = counterplayNote;
        }

        private static void ApplyTelegraphRead(
            SerializedObject serializedObject,
            Color windupColor,
            Color releaseColor,
            float markerWidthScale,
            float markerDepthScale,
            float markerPulseScale)
        {
            RequireProperty(serializedObject, "telegraphWindupColor").colorValue = windupColor;
            RequireProperty(serializedObject, "telegraphReleaseColor").colorValue = releaseColor;
            RequireProperty(serializedObject, "telegraphMarkerWidthScale").floatValue = markerWidthScale;
            RequireProperty(serializedObject, "telegraphMarkerDepthScale").floatValue = markerDepthScale;
            RequireProperty(serializedObject, "telegraphPulseScale").floatValue = markerPulseScale;
        }

        private static void ApplyProjectileRead(
            SerializedObject serializedObject,
            Color projectileColor,
            Vector3 projectileVisualScale,
            Material projectileMaterial)
        {
            RequireProperty(serializedObject, "projectileColor").colorValue = projectileColor;
            RequireProperty(serializedObject, "projectileVisualScale").vector3Value = projectileVisualScale;
            RequireProperty(serializedObject, "projectileMaterial").objectReferenceValue = projectileMaterial;
        }

        private static BossBarragePatternProfile EnsurePatternProfile()
        {
            EnsureFolderForAsset(PatternProfilePath);
            BossBarragePatternProfile profile = AssetDatabase.LoadAssetAtPath<BossBarragePatternProfile>(PatternProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
                AssetDatabase.CreateAsset(profile, PatternProfilePath);
            }

            var serializedObject = new SerializedObject(profile);
            RequireProperty(serializedObject, "patternId").stringValue = "NeedleLock";
            ApplySkillGrammar(
                serializedObject,
                LaneSkillPatternFamily.DirectLock,
                LaneSkillTransferMode.BossOnly,
                "Keep out of player basic fire; reuse only as a locked skill with startup.",
                "Dodge the tracked line after windup or answer overlapping frontline pressure with a summon.");
            RequireProperty(serializedObject, "lateralShape").enumValueIndex = (int)BossBarrageLateralShape.CenterSpread;
            RequireProperty(serializedObject, "initialDelaySeconds").floatValue = 0.8f;
            RequireProperty(serializedObject, "windupSeconds").floatValue = 0.9f;
            RequireProperty(serializedObject, "waveIntervalSeconds").floatValue = 5.6f;
            RequireProperty(serializedObject, "projectilesPerWave").intValue = 2;
            RequireProperty(serializedObject, "damage").floatValue = 9f;
            RequireProperty(serializedObject, "projectileSpeed").floatValue = 13.5f;
            RequireProperty(serializedObject, "projectileLifetimeSeconds").floatValue = 4.6f;
            RequireProperty(serializedObject, "projectileRadius").floatValue = 0.34f;
            RequireProperty(serializedObject, "backlineHalfSpread").floatValue = 3.2f;
            RequireProperty(serializedObject, "forwardHalfSpread").floatValue = 1.05f;
            RequireProperty(serializedObject, "spawnHeight").floatValue = 1.35f;
            RequireProperty(serializedObject, "targetHeight").floatValue = 1.05f;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static BossBasicFireProfile EnsureBossBasicFireProfile()
        {
            EnsureFolderForAsset(BossBasicFireProfilePath);
            BossBasicFireProfile profile =
                AssetDatabase.LoadAssetAtPath<BossBasicFireProfile>(BossBasicFireProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<BossBasicFireProfile>();
                AssetDatabase.CreateAsset(profile, BossBasicFireProfilePath);
            }

            var serializedObject = new SerializedObject(profile);
            RequireProperty(serializedObject, "fireId").stringValue = "LanePoke";
            RequireProperty(serializedObject, "readoutLabel").stringValue = "Rifle Poke";
            RequireProperty(serializedObject, "initialDelaySeconds").floatValue = 1.05f;
            RequireProperty(serializedObject, "fireIntervalSeconds").floatValue = 1.95f;
            RequireProperty(serializedObject, "projectilesPerVolley").intValue = 2;
            RequireProperty(serializedObject, "damage").floatValue = 3.6f;
            RequireProperty(serializedObject, "projectileSpeed").floatValue = 24f;
            RequireProperty(serializedObject, "projectileLifetimeSeconds").floatValue = 1.35f;
            RequireProperty(serializedObject, "projectileRadius").floatValue = 0.22f;
            RequireProperty(serializedObject, "backlineHalfSpread").floatValue = 0.45f;
            RequireProperty(serializedObject, "forwardHalfSpread").floatValue = 0.18f;
            RequireProperty(serializedObject, "spawnLateralFollowRatio").floatValue = 0.92f;
            RequireProperty(serializedObject, "spawnHeight").floatValue = 1.2f;
            RequireProperty(serializedObject, "targetHeight").floatValue = 1.1f;
            RequireProperty(serializedObject, "projectileColor").colorValue = new Color(1f, 0.55f, 0.18f, 1f);
            RequireProperty(serializedObject, "projectileVisualScale").vector3Value = Vector3.one;
            RequireProperty(serializedObject, "projectileMaterial").objectReferenceValue =
                LoadAsset<Material>(BossBasicFireProjectileMaterialPath);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static BossBarragePatternProfile EnsurePunishNetPatternProfile()
        {
            EnsureFolderForAsset(PunishNetPatternProfilePath);
            BossBarragePatternProfile profile =
                AssetDatabase.LoadAssetAtPath<BossBarragePatternProfile>(PunishNetPatternProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
                AssetDatabase.CreateAsset(profile, PunishNetPatternProfilePath);
            }

            var serializedObject = new SerializedObject(profile);
            RequireProperty(serializedObject, "patternId").stringValue = "PunishNet";
            ApplySkillGrammar(
                serializedObject,
                LaneSkillPatternFamily.PunishNet,
                LaneSkillTransferMode.CostedPlayerSkillCandidate,
                "Can become an overextend-punish skill that targets a committed opponent, never a basic shot.",
                "Avoid overcommitting forward; break lock with dodge or summon cover during windup.");
            RequireProperty(serializedObject, "lateralShape").enumValueIndex = (int)BossBarrageLateralShape.PunishNet;
            RequireProperty(serializedObject, "initialDelaySeconds").floatValue = 0.2f;
            RequireProperty(serializedObject, "windupSeconds").floatValue = 1.25f;
            RequireProperty(serializedObject, "waveIntervalSeconds").floatValue = 7.0f;
            RequireProperty(serializedObject, "projectilesPerWave").intValue = 4;
            RequireProperty(serializedObject, "damage").floatValue = 7.5f;
            RequireProperty(serializedObject, "projectileSpeed").floatValue = 12.4f;
            RequireProperty(serializedObject, "projectileLifetimeSeconds").floatValue = 5.2f;
            RequireProperty(serializedObject, "projectileRadius").floatValue = 0.29f;
            RequireProperty(serializedObject, "backlineHalfSpread").floatValue = 3.45f;
            RequireProperty(serializedObject, "forwardHalfSpread").floatValue = 0.92f;
            RequireProperty(serializedObject, "punishNetInnerSpreadRatio").floatValue = 0.34f;
            RequireProperty(serializedObject, "spawnHeight").floatValue = 1.52f;
            RequireProperty(serializedObject, "targetHeight").floatValue = 1.05f;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static BossBarragePatternProfile EnsureCoverFirePatternProfile()
        {
            EnsureFolderForAsset(CoverFirePatternProfilePath);
            BossBarragePatternProfile profile =
                AssetDatabase.LoadAssetAtPath<BossBarragePatternProfile>(CoverFirePatternProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
                AssetDatabase.CreateAsset(profile, CoverFirePatternProfilePath);
            }

            var serializedObject = new SerializedObject(profile);
            RequireProperty(serializedObject, "patternId").stringValue = "CoverFire";
            ApplySkillGrammar(
                serializedObject,
                LaneSkillPatternFamily.CenterCover,
                LaneSkillTransferMode.CostedPlayerSkillCandidate,
                "Can become a costed center-lane suppress skill with fixed lane aim and no hard tracking.",
                "Read center lane windup and move to side gaps before the spread tightens.");
            RequireProperty(serializedObject, "targetingRule").enumValueIndex = (int)BossBarrageTargetingRule.LaneCenter;
            RequireProperty(serializedObject, "laneCenterLateralRatio").floatValue = 0f;
            RequireProperty(serializedObject, "lateralShape").enumValueIndex = (int)BossBarrageLateralShape.CenterSpread;
            RequireProperty(serializedObject, "initialDelaySeconds").floatValue = 0.25f;
            RequireProperty(serializedObject, "windupSeconds").floatValue = 1.05f;
            RequireProperty(serializedObject, "waveIntervalSeconds").floatValue = 6.4f;
            RequireProperty(serializedObject, "projectilesPerWave").intValue = 4;
            RequireProperty(serializedObject, "damage").floatValue = 8f;
            RequireProperty(serializedObject, "projectileSpeed").floatValue = 13.5f;
            RequireProperty(serializedObject, "projectileLifetimeSeconds").floatValue = 4.8f;
            RequireProperty(serializedObject, "projectileRadius").floatValue = 0.3f;
            RequireProperty(serializedObject, "backlineHalfSpread").floatValue = 3.2f;
            RequireProperty(serializedObject, "forwardHalfSpread").floatValue = 1.25f;
            RequireProperty(serializedObject, "spawnHeight").floatValue = 1.38f;
            RequireProperty(serializedObject, "targetHeight").floatValue = 1.05f;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static BossBarragePatternProfile EnsureEscortScreenPatternProfile()
        {
            EnsureFolderForAsset(EscortScreenPatternProfilePath);
            BossBarragePatternProfile profile =
                AssetDatabase.LoadAssetAtPath<BossBarragePatternProfile>(EscortScreenPatternProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
                AssetDatabase.CreateAsset(profile, EscortScreenPatternProfilePath);
            }

            var serializedObject = new SerializedObject(profile);
            RequireProperty(serializedObject, "patternId").stringValue = "EscortScreen";
            ApplySkillGrammar(
                serializedObject,
                LaneSkillPatternFamily.EscortScreen,
                LaneSkillTransferMode.SharedPvpSkillCandidate,
                "Can become a costed screen skill that protects a summon or denies side lanes.",
                "Answer with side reposition, projectile block, or summon screen before the curtain closes.");
            RequireProperty(serializedObject, "targetingRule").enumValueIndex = (int)BossBarrageTargetingRule.LaneCenter;
            RequireProperty(serializedObject, "laneCenterLateralRatio").floatValue = 0f;
            RequireProperty(serializedObject, "lateralShape").enumValueIndex = (int)BossBarrageLateralShape.EscortScreen;
            RequireProperty(serializedObject, "initialDelaySeconds").floatValue = 0.2f;
            RequireProperty(serializedObject, "windupSeconds").floatValue = 1.15f;
            RequireProperty(serializedObject, "waveIntervalSeconds").floatValue = 6.8f;
            RequireProperty(serializedObject, "projectilesPerWave").intValue = 5;
            RequireProperty(serializedObject, "damage").floatValue = 7.5f;
            RequireProperty(serializedObject, "projectileSpeed").floatValue = 12.6f;
            RequireProperty(serializedObject, "projectileLifetimeSeconds").floatValue = 5.1f;
            RequireProperty(serializedObject, "projectileRadius").floatValue = 0.28f;
            RequireProperty(serializedObject, "backlineHalfSpread").floatValue = 4.0f;
            RequireProperty(serializedObject, "forwardHalfSpread").floatValue = 1.6f;
            RequireProperty(serializedObject, "escortScreenInnerGapRatio").floatValue = 0.35f;
            RequireProperty(serializedObject, "backlineDepthSpread").floatValue = 2.4f;
            RequireProperty(serializedObject, "forwardDepthSpread").floatValue = 0.9f;
            RequireProperty(serializedObject, "spawnHeight").floatValue = 1.45f;
            RequireProperty(serializedObject, "targetHeight").floatValue = 1.05f;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static BossBarragePatternProfile EnsureLayeredSalvoPatternProfile()
        {
            EnsureFolderForAsset(LayeredSalvoPatternProfilePath);
            BossBarragePatternProfile profile =
                AssetDatabase.LoadAssetAtPath<BossBarragePatternProfile>(LayeredSalvoPatternProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
                AssetDatabase.CreateAsset(profile, LayeredSalvoPatternProfilePath);
            }

            var serializedObject = new SerializedObject(profile);
            Material projectileMaterial = LoadAsset<Material>(ProjectileMaterialPath);
            RequireProperty(serializedObject, "patternId").stringValue = "LayeredSalvo";
            ApplySkillGrammar(
                serializedObject,
                LaneSkillPatternFamily.LayeredSalvo,
                LaneSkillTransferMode.SharedPvpSkillCandidate,
                "Can become a committed multi-row skill with authored row timing and visible release beats.",
                "Read row depth telegraphs and dodge through the widest late gap.");
            ApplyTelegraphRead(
                serializedObject,
                new Color(1f, 0.24f, 0.72f, 0.7f),
                new Color(1f, 0.8f, 0.35f, 0.95f),
                1.28f,
                0.58f,
                0.85f);
            ApplyProjectileRead(
                serializedObject,
                new Color(1f, 0.28f, 0.78f, 1f),
                Vector3.one,
                projectileMaterial);
            RequireProperty(serializedObject, "targetingRule").enumValueIndex = (int)BossBarrageTargetingRule.LaneCenter;
            RequireProperty(serializedObject, "laneCenterLateralRatio").floatValue = 0f;
            RequireProperty(serializedObject, "lateralShape").enumValueIndex = (int)BossBarrageLateralShape.LayeredSalvo;
            RequireProperty(serializedObject, "initialDelaySeconds").floatValue = 0.35f;
            RequireProperty(serializedObject, "windupSeconds").floatValue = 1.25f;
            RequireProperty(serializedObject, "waveIntervalSeconds").floatValue = 7.8f;
            RequireProperty(serializedObject, "projectilesPerWave").intValue = 6;
            RequireProperty(serializedObject, "damage").floatValue = 6.5f;
            RequireProperty(serializedObject, "projectileSpeed").floatValue = 11.8f;
            RequireProperty(serializedObject, "projectileLifetimeSeconds").floatValue = 5.6f;
            RequireProperty(serializedObject, "projectileRadius").floatValue = 0.3f;
            RequireProperty(serializedObject, "backlineHalfSpread").floatValue = 4.2f;
            RequireProperty(serializedObject, "forwardHalfSpread").floatValue = 1.75f;
            RequireProperty(serializedObject, "layeredSalvoRowCount").intValue = 3;
            RequireProperty(serializedObject, "backlineDepthSpread").floatValue = 3.2f;
            RequireProperty(serializedObject, "forwardDepthSpread").floatValue = 1.1f;
            RequireProperty(serializedObject, "spawnHeight").floatValue = 1.5f;
            RequireProperty(serializedObject, "targetHeight").floatValue = 1.05f;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static BossBarragePatternProfile EnsureLinePressurePatternProfile()
        {
            EnsureFolderForAsset(LinePressurePatternProfilePath);
            BossBarragePatternProfile profile =
                AssetDatabase.LoadAssetAtPath<BossBarragePatternProfile>(LinePressurePatternProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
                AssetDatabase.CreateAsset(profile, LinePressurePatternProfilePath);
            }

            var serializedObject = new SerializedObject(profile);
            Material projectileMaterial = LoadAsset<Material>(ProjectileMaterialPath);
            RequireProperty(serializedObject, "patternId").stringValue = "LinePressure";
            ApplySkillGrammar(
                serializedObject,
                LaneSkillPatternFamily.LinePressure,
                LaneSkillTransferMode.SharedPvpSkillCandidate,
                "Can become a committed rail-pressure skill with narrow scatter and depth spacing.",
                "Move off the marked rail or block it with tank-screen summon timing.");
            ApplyTelegraphRead(
                serializedObject,
                new Color(0.12f, 0.9f, 1f, 0.72f),
                new Color(0.56f, 1f, 1f, 0.96f),
                0.48f,
                1.85f,
                1.35f);
            ApplyProjectileRead(
                serializedObject,
                new Color(0.2f, 0.95f, 1f, 1f),
                Vector3.one,
                projectileMaterial);
            RequireProperty(serializedObject, "lateralShape").enumValueIndex = (int)BossBarrageLateralShape.LinePressure;
            RequireProperty(serializedObject, "initialDelaySeconds").floatValue = 0.2f;
            RequireProperty(serializedObject, "windupSeconds").floatValue = 1.15f;
            RequireProperty(serializedObject, "waveIntervalSeconds").floatValue = 6.8f;
            RequireProperty(serializedObject, "projectilesPerWave").intValue = 3;
            RequireProperty(serializedObject, "damage").floatValue = 8.5f;
            RequireProperty(serializedObject, "projectileSpeed").floatValue = 13.0f;
            RequireProperty(serializedObject, "projectileLifetimeSeconds").floatValue = 5.1f;
            RequireProperty(serializedObject, "projectileRadius").floatValue = 0.3f;
            RequireProperty(serializedObject, "backlineHalfSpread").floatValue = 4.0f;
            RequireProperty(serializedObject, "forwardHalfSpread").floatValue = 0f;
            RequireProperty(serializedObject, "linePressureDirection").floatValue = 1f;
            RequireProperty(serializedObject, "linePressureCenterRatio").floatValue = 0.72f;
            RequireProperty(serializedObject, "linePressureHalfSpreadRatio").floatValue = 0.08f;
            RequireProperty(serializedObject, "backlineDepthSpread").floatValue = 2.2f;
            RequireProperty(serializedObject, "forwardDepthSpread").floatValue = 0.85f;
            RequireProperty(serializedObject, "spawnHeight").floatValue = 1.5f;
            RequireProperty(serializedObject, "targetHeight").floatValue = 1.05f;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static BossBarragePatternProfile EnsureStaggeredCrossfirePatternProfile()
        {
            EnsureFolderForAsset(StaggeredCrossfirePatternProfilePath);
            BossBarragePatternProfile profile =
                AssetDatabase.LoadAssetAtPath<BossBarragePatternProfile>(StaggeredCrossfirePatternProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
                AssetDatabase.CreateAsset(profile, StaggeredCrossfirePatternProfilePath);
            }

            var serializedObject = new SerializedObject(profile);
            RequireProperty(serializedObject, "patternId").stringValue = "StaggeredCrossfire";
            ApplySkillGrammar(
                serializedObject,
                LaneSkillPatternFamily.StaggeredCrossfire,
                LaneSkillTransferMode.SharedPvpSkillCandidate,
                "Can become a heavy crossed-pair skill with delayed correction rows.",
                "Bait the first pair, then sidestep the reversed correction lane.");
            RequireProperty(serializedObject, "targetingRule").enumValueIndex = (int)BossBarrageTargetingRule.LaneCenter;
            RequireProperty(serializedObject, "laneCenterLateralRatio").floatValue = 0f;
            RequireProperty(serializedObject, "lateralShape").enumValueIndex = (int)BossBarrageLateralShape.StaggeredCrossfire;
            RequireProperty(serializedObject, "initialDelaySeconds").floatValue = 0.3f;
            RequireProperty(serializedObject, "windupSeconds").floatValue = 1.3f;
            RequireProperty(serializedObject, "waveIntervalSeconds").floatValue = 7.4f;
            RequireProperty(serializedObject, "projectilesPerWave").intValue = 4;
            RequireProperty(serializedObject, "damage").floatValue = 8f;
            RequireProperty(serializedObject, "projectileSpeed").floatValue = 10.6f;
            RequireProperty(serializedObject, "projectileLifetimeSeconds").floatValue = 5.8f;
            RequireProperty(serializedObject, "projectileRadius").floatValue = 0.38f;
            RequireProperty(serializedObject, "backlineHalfSpread").floatValue = 4.35f;
            RequireProperty(serializedObject, "forwardHalfSpread").floatValue = 1.95f;
            RequireProperty(serializedObject, "crossfireInnerGapRatio").floatValue = 0.30f;
            RequireProperty(serializedObject, "backlineDepthSpread").floatValue = 2.8f;
            RequireProperty(serializedObject, "forwardDepthSpread").floatValue = 0.95f;
            RequireProperty(serializedObject, "spawnHeight").floatValue = 1.65f;
            RequireProperty(serializedObject, "targetHeight").floatValue = 1.05f;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static BossBarragePatternProfile EnsureTwinSweepPatternProfile()
        {
            EnsureFolderForAsset(TwinSweepPatternProfilePath);
            BossBarragePatternProfile profile =
                AssetDatabase.LoadAssetAtPath<BossBarragePatternProfile>(TwinSweepPatternProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
                AssetDatabase.CreateAsset(profile, TwinSweepPatternProfilePath);
            }

            var serializedObject = new SerializedObject(profile);
            RequireProperty(serializedObject, "patternId").stringValue = "TwinSweep";
            ApplySkillGrammar(
                serializedObject,
                LaneSkillPatternFamily.TwinSweep,
                LaneSkillTransferMode.SharedPvpSkillCandidate,
                "Can become a costed twin-column sweep that leaves a readable center or side lane.",
                "Hold the readable gap and shift before the second column closes.");
            RequireProperty(serializedObject, "lateralShape").enumValueIndex = (int)BossBarrageLateralShape.TwinColumns;
            RequireProperty(serializedObject, "initialDelaySeconds").floatValue = 0.2f;
            RequireProperty(serializedObject, "windupSeconds").floatValue = 1.1f;
            RequireProperty(serializedObject, "waveIntervalSeconds").floatValue = 6.4f;
            RequireProperty(serializedObject, "projectilesPerWave").intValue = 3;
            RequireProperty(serializedObject, "damage").floatValue = 8.5f;
            RequireProperty(serializedObject, "projectileSpeed").floatValue = 12.2f;
            RequireProperty(serializedObject, "projectileLifetimeSeconds").floatValue = 4.9f;
            RequireProperty(serializedObject, "projectileRadius").floatValue = 0.31f;
            RequireProperty(serializedObject, "backlineHalfSpread").floatValue = 3.65f;
            RequireProperty(serializedObject, "forwardHalfSpread").floatValue = 1.45f;
            RequireProperty(serializedObject, "twinColumnInnerSpreadRatio").floatValue = 0.42f;
            RequireProperty(serializedObject, "spawnHeight").floatValue = 1.42f;
            RequireProperty(serializedObject, "targetHeight").floatValue = 1.05f;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static BossBarragePatternProfile EnsureLeftClampPatternProfile()
        {
            EnsureFolderForAsset(LeftClampPatternProfilePath);
            BossBarragePatternProfile profile =
                AssetDatabase.LoadAssetAtPath<BossBarragePatternProfile>(LeftClampPatternProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
                AssetDatabase.CreateAsset(profile, LeftClampPatternProfilePath);
            }

            var serializedObject = new SerializedObject(profile);
            RequireProperty(serializedObject, "patternId").stringValue = "LeftClamp";
            ApplySkillGrammar(
                serializedObject,
                LaneSkillPatternFamily.SideClamp,
                LaneSkillTransferMode.SharedPvpSkillCandidate,
                "Can become a mirrored side-clamp skill authored per side instead of hidden aim logic.",
                "Identify the closing side and escape through the opposite gap.");
            RequireProperty(serializedObject, "lateralShape").enumValueIndex = (int)BossBarrageLateralShape.SideClamp;
            RequireProperty(serializedObject, "initialDelaySeconds").floatValue = 0.2f;
            RequireProperty(serializedObject, "windupSeconds").floatValue = 1.2f;
            RequireProperty(serializedObject, "waveIntervalSeconds").floatValue = 6.6f;
            RequireProperty(serializedObject, "projectilesPerWave").intValue = 4;
            RequireProperty(serializedObject, "damage").floatValue = 8f;
            RequireProperty(serializedObject, "projectileSpeed").floatValue = 12.8f;
            RequireProperty(serializedObject, "projectileLifetimeSeconds").floatValue = 5.0f;
            RequireProperty(serializedObject, "projectileRadius").floatValue = 0.3f;
            RequireProperty(serializedObject, "backlineHalfSpread").floatValue = 3.85f;
            RequireProperty(serializedObject, "forwardHalfSpread").floatValue = 1.7f;
            RequireProperty(serializedObject, "sideClampDirection").floatValue = -1f;
            RequireProperty(serializedObject, "sideClampCrossReachRatio").floatValue = 0.24f;
            RequireProperty(serializedObject, "spawnHeight").floatValue = 1.48f;
            RequireProperty(serializedObject, "targetHeight").floatValue = 1.05f;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static BossBarragePatternProfile EnsureRightClampPatternProfile()
        {
            EnsureFolderForAsset(RightClampPatternProfilePath);
            BossBarragePatternProfile profile =
                AssetDatabase.LoadAssetAtPath<BossBarragePatternProfile>(RightClampPatternProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
                AssetDatabase.CreateAsset(profile, RightClampPatternProfilePath);
            }

            var serializedObject = new SerializedObject(profile);
            RequireProperty(serializedObject, "patternId").stringValue = "RightClamp";
            ApplySkillGrammar(
                serializedObject,
                LaneSkillPatternFamily.SideClamp,
                LaneSkillTransferMode.SharedPvpSkillCandidate,
                "Can become a mirrored side-clamp skill authored per side instead of hidden aim logic.",
                "Identify the closing side and escape through the opposite gap.");
            RequireProperty(serializedObject, "lateralShape").enumValueIndex = (int)BossBarrageLateralShape.SideClamp;
            RequireProperty(serializedObject, "initialDelaySeconds").floatValue = 0.2f;
            RequireProperty(serializedObject, "windupSeconds").floatValue = 1.2f;
            RequireProperty(serializedObject, "waveIntervalSeconds").floatValue = 6.6f;
            RequireProperty(serializedObject, "projectilesPerWave").intValue = 4;
            RequireProperty(serializedObject, "damage").floatValue = 8f;
            RequireProperty(serializedObject, "projectileSpeed").floatValue = 12.8f;
            RequireProperty(serializedObject, "projectileLifetimeSeconds").floatValue = 5.0f;
            RequireProperty(serializedObject, "projectileRadius").floatValue = 0.3f;
            RequireProperty(serializedObject, "backlineHalfSpread").floatValue = 3.85f;
            RequireProperty(serializedObject, "forwardHalfSpread").floatValue = 1.7f;
            RequireProperty(serializedObject, "sideClampDirection").floatValue = 1f;
            RequireProperty(serializedObject, "sideClampCrossReachRatio").floatValue = 0.24f;
            RequireProperty(serializedObject, "spawnHeight").floatValue = 1.48f;
            RequireProperty(serializedObject, "targetHeight").floatValue = 1.05f;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static PlayerActionProfile EnsureLocalDefenseProfile()
        {
            EnsureFolderForAsset(LocalDefenseProfilePath);
            PlayerActionProfile profile = AssetDatabase.LoadAssetAtPath<PlayerActionProfile>(LocalDefenseProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<PlayerActionProfile>();
                AssetDatabase.CreateAsset(profile, LocalDefenseProfilePath);
            }

            var serializedObject = new SerializedObject(profile);
            SerializedProperty basicCombo = RequireProperty(serializedObject, "basicCombo");
            basicCombo.arraySize = 1;
            SerializedProperty step = basicCombo.GetArrayElementAtIndex(0);
            RequireRelativeProperty(step, "animationTrigger").stringValue = "Attack1";
            RequireRelativeProperty(step, "startupSeconds").floatValue = 0.1f;
            RequireRelativeProperty(step, "activeSeconds").floatValue = 0.08f;
            RequireRelativeProperty(step, "recoverySeconds").floatValue = 0.26f;
            RequireRelativeProperty(step, "inputBufferSeconds").floatValue = 0.08f;
            RequireRelativeProperty(step, "dodgeCancelAfterSeconds").floatValue = 0.05f;
            RequireRelativeProperty(step, "forwardAdvanceDistance").floatValue = 0.22f;
            RequireRelativeProperty(step, "forwardAdvanceDurationSeconds").floatValue = 0.10f;
            RequireRelativeProperty(step, "damage").floatValue = 42f;
            RequireRelativeProperty(step, "hitRadius").floatValue = 0.78f;
            RequireRelativeProperty(step, "hitDistance").floatValue = 1.65f;
            RequireRelativeProperty(step, "hitStopSeconds").floatValue = 0f;
            RequireRelativeProperty(step, "responsePolicy").enumValueIndex = (int)DamageResponsePolicy.Stagger;
            RequireRelativeProperty(step, "controlLockPolicy").enumValueIndex = (int)CombatControlLockPolicy.InterruptAction;

            RequireProperty(serializedObject, "comboResetSeconds").floatValue = 0.32f;
            RequireProperty(serializedObject, "comboQueueOpenAfterSeconds").floatValue = 0.12f;
            RequireProperty(serializedObject, "comboChainRecoveryRatio").floatValue = 1f;
            RequireProperty(serializedObject, "attackFacingHoldPaddingSeconds").floatValue = 0.04f;
            RequireProperty(serializedObject, "snapBasicAttackFacing").boolValue = true;
            RequireProperty(serializedObject, "basicAttackMoveInputSpeedScale").floatValue = 0f;
            RequireProperty(serializedObject, "dodgeDurationSeconds").floatValue = 0.56f;
            RequireProperty(serializedObject, "dodgeInvulnerableFromSeconds").floatValue = 0.02f;
            RequireProperty(serializedObject, "dodgeInvulnerableToSeconds").floatValue = 0.40f;
            RequireProperty(serializedObject, "dodgeRecoverySeconds").floatValue = 0.14f;
            RequireProperty(serializedObject, "dodgeCooldownSeconds").floatValue = 1.15f;
            RequireProperty(serializedObject, "perfectDodgeProtectionSeconds").floatValue = 1.15f;
            RequireProperty(serializedObject, "perfectDodgeTimingGraceSeconds").floatValue = 0.08f;
            RequireProperty(serializedObject, "dodgeSpeed").floatValue = 10.2f;
            RequireProperty(serializedObject, "dodgeTrigger").stringValue = "DodgeForward";
            RequireProperty(serializedObject, "dodgeBackTrigger").stringValue = "DodgeBack";
            RequireProperty(serializedObject, "dodgeLeftTrigger").stringValue = "DodgeLeft";
            RequireProperty(serializedObject, "dodgeRightTrigger").stringValue = "DodgeRight";
            RequireProperty(serializedObject, "dodgingParameter").stringValue = "IsDodging";
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static BossBarrageProjectile EnsureProjectilePrefab()
        {
            EnsureFolderForAsset(ProjectilePrefabPath);
            Material material = LoadAsset<Material>(ProjectileMaterialPath);
            bool prefabExists = AssetDatabase.LoadAssetAtPath<GameObject>(ProjectilePrefabPath) != null;
            GameObject editableRoot = prefabExists
                ? PrefabUtility.LoadPrefabContents(ProjectilePrefabPath)
                : GameObject.CreatePrimitive(PrimitiveType.Sphere);

            try
            {
                editableRoot.name = "PF_BossBarrageProjectile_NeedleLock";
                editableRoot.transform.localPosition = Vector3.zero;
                editableRoot.transform.localRotation = Quaternion.identity;
                editableRoot.transform.localScale = Vector3.one * 0.62f;

                MeshFilter meshFilter = EnsureComponent<MeshFilter>(editableRoot);
                meshFilter.sharedMesh = LoadPrimitiveMesh(PrimitiveType.Sphere);

                MeshRenderer renderer = EnsureComponent<MeshRenderer>(editableRoot);
                renderer.sharedMaterial = material;
                renderer.enabled = false;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                renderer.allowOcclusionWhenDynamic = false;

                SphereCollider collider = EnsureComponent<SphereCollider>(editableRoot);
                collider.isTrigger = true;
                collider.radius = 0.5f;

                Rigidbody rigidbody = EnsureComponent<Rigidbody>(editableRoot);
                rigidbody.useGravity = false;
                rigidbody.isKinematic = true;

                ConfigureBossBarrageProjectileVisuals(editableRoot, material);
                BossBarrageProjectile projectile = EnsureComponent<BossBarrageProjectile>(editableRoot);
                // Keep runtime tint/material presentation isolated from the authored Hovl particle materials.
                SetObjectReferenceArray(projectile, "visualRenderers", new UnityEngine.Object[] { renderer });
                PrefabUtility.SaveAsPrefabAsset(editableRoot, ProjectilePrefabPath);
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

            return LoadPrefabComponent<BossBarrageProjectile>(ProjectilePrefabPath);
        }

        private static void ConfigureBossBarrageProjectileVisuals(GameObject projectileRoot, Material coreMaterial)
        {
            const string VisualPrefix = "BossBarrageProjectileVfx_";
            RemoveChildrenWithPrefix(projectileRoot.transform, VisualPrefix);

            TrailRenderer oldTrail = projectileRoot.GetComponent<TrailRenderer>();
            if (oldTrail != null)
            {
                UnityEngine.Object.DestroyImmediate(oldTrail);
            }

            AttachPromotedForge3DMissilePrefab(
                projectileRoot.transform,
                BossBarrageForge3DMissileChildName,
                ImportedForge3DMissileExamplePrefabPath,
                Vector3.zero,
                Vector3.zero,
                Vector3.one * 1.42f);

            EditorUtility.SetDirty(projectileRoot);
        }

        private static void EnsureBossBarrageCombatCueAssetOverlays()
        {
            CombatVfxCueProfile profile =
                LoadAsset<CombatVfxCueProfile>(ActionFoundationCombatVfxSetup.CombatVfxCueProfilePath);
            SetCombatVfxCuePlaybackMode(profile, CombatVfxCuePlaybackMode.ReviewedCombatFeedbackOnly);
            EnsureCombatCueAssetOverlay(
                profile,
                CombatVfxCueId.PlayerRangedProjectileImpact,
                "CueAssetVfx_MagicMissilesLightImpact",
                ImportedMagicMissilesLightImpactPrefabPath,
                new Vector3(0f, 0.08f, 0f),
                Vector3.zero,
                Vector3.one * 0.3f,
                loopParticles: false);
            EnsureCombatCueAssetOverlay(
                profile,
                CombatVfxCueId.EnemyDeath,
                "CueAssetVfx_MagicMissilesDeathBurst",
                ImportedMagicMissilesDeathImpactPrefabPath,
                new Vector3(0f, 0.04f, 0f),
                Vector3.zero,
                Vector3.one * 0.5f,
                loopParticles: false);
            EnsureCombatCueAssetOverlay(
                profile,
                CombatVfxCueId.EliteShieldSignal,
                "CueAssetVfx_MagicMissilesGuardState",
                ImportedMagicMissilesHolyImpactPrefabPath,
                new Vector3(0f, 0.08f, 0f),
                Vector3.zero,
                Vector3.one * 0.42f,
                loopParticles: false);
            EnsureCombatCueAssetOverlay(
                profile,
                CombatVfxCueId.EliteAuraSignal,
                "CueAssetVfx_MagicMissilesActiveAura",
                ImportedMagicMissilesHealingAuraPrefabPath,
                new Vector3(0f, 0.08f, 0f),
                Vector3.zero,
                Vector3.one * 0.64f,
                loopParticles: true);
            EnsureCombatCueAssetOverlay(
                profile,
                CombatVfxCueId.EliteSummonSignal,
                "CueAssetVfx_MagicMissilesSummonState",
                ImportedMagicMissilesArcaneAuraPrefabPath,
                new Vector3(0f, 0.08f, 0f),
                Vector3.zero,
                Vector3.one * 0.6f,
                loopParticles: true);
            RemoveCombatCueAssetOverlay(
                profile,
                CombatVfxCueId.SummonBlockOpportunity,
                "CueAssetVfx_MagicMissilesPressureStorm");
            RemoveCombatCueAssetOverlay(
                profile,
                CombatVfxCueId.SummonFollowupWindow,
                "CueAssetVfx_MagicMissilesFollowupCircle");
        }

        private static void SetCombatVfxCuePlaybackMode(
            CombatVfxCueProfile profile,
            CombatVfxCuePlaybackMode playbackMode)
        {
            SerializedObject serializedObject = new SerializedObject(profile);
            RequireProperty(serializedObject, "playbackMode").enumValueIndex = (int)playbackMode;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static void EnsureCombatCueAssetOverlay(
            CombatVfxCueProfile profile,
            CombatVfxCueId cueId,
            string childName,
            string sourcePrefabPath,
            Vector3 localPosition,
            Vector3 localEuler,
            Vector3 localScale,
            bool loopParticles)
        {
            if (!profile.TryGetCue(cueId, out CombatVfxCue cue) || cue.Prefab == null)
            {
                throw new InvalidOperationException($"Boss barrage combat cue profile is missing {cueId}.");
            }

            string prefabPath = AssetDatabase.GetAssetPath(cue.Prefab).Replace('\\', '/');
            if (string.IsNullOrWhiteSpace(prefabPath))
            {
                throw new InvalidOperationException($"{cueId} should reference a saved combat VFX prefab.");
            }

            GameObject editableRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                AttachPromotedVfxPrefab(
                    editableRoot.transform,
                    childName,
                    sourcePrefabPath,
                    localPosition,
                    localEuler,
                    localScale,
                    loopParticles,
                    playOnAwake: true);
                PrefabUtility.SaveAsPrefabAsset(editableRoot, prefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(editableRoot);
            }
        }

        private static void RemoveCombatCueAssetOverlay(
            CombatVfxCueProfile profile,
            CombatVfxCueId cueId,
            string childName)
        {
            if (!profile.TryGetCue(cueId, out CombatVfxCue cue) || cue.Prefab == null)
            {
                throw new InvalidOperationException($"Boss barrage combat cue profile is missing {cueId}.");
            }

            string prefabPath = AssetDatabase.GetAssetPath(cue.Prefab).Replace('\\', '/');
            if (string.IsNullOrWhiteSpace(prefabPath))
            {
                throw new InvalidOperationException($"{cueId} should reference a saved combat VFX prefab.");
            }

            GameObject editableRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                Transform existing = editableRoot.transform.Find(childName);
                if (existing != null)
                {
                    UnityEngine.Object.DestroyImmediate(existing.gameObject);
                    PrefabUtility.SaveAsPrefabAsset(editableRoot, prefabPath);
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(editableRoot);
            }
        }

        private static LaneActionProjectile EnsureLaneActionProjectilePrefab(
            string prefabPath,
            string prefabName,
            string materialPath,
            Color color,
            float scale,
            bool allowVerticalTravel)
        {
            EnsureFolderForAsset(prefabPath);
            Material material = LoadOrCreateMaterial(materialPath, color);
            bool prefabExists = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null;
            GameObject editableRoot = prefabExists
                ? PrefabUtility.LoadPrefabContents(prefabPath)
                : GameObject.CreatePrimitive(PrimitiveType.Sphere);

            try
            {
                editableRoot.name = prefabName;
                editableRoot.transform.localPosition = Vector3.zero;
                editableRoot.transform.localRotation = Quaternion.identity;
                editableRoot.transform.localScale = Vector3.one * Mathf.Max(0.01f, scale);

                MeshRenderer renderer = EnsureComponent<MeshRenderer>(editableRoot);
                renderer.sharedMaterial = material;
                bool usesAuthoredProjectileVfx =
                    UsesAuthoredLaneProjectileVfx(prefabPath);
                renderer.enabled = !usesAuthoredProjectileVfx;
                if (usesAuthoredProjectileVfx)
                {
                    renderer.shadowCastingMode = ShadowCastingMode.Off;
                    renderer.receiveShadows = false;
                    renderer.allowOcclusionWhenDynamic = false;
                }

                SphereCollider collider = EnsureComponent<SphereCollider>(editableRoot);
                collider.isTrigger = true;
                collider.radius = 0.5f;

                Rigidbody rigidbody = EnsureComponent<Rigidbody>(editableRoot);
                rigidbody.useGravity = false;
                rigidbody.isKinematic = true;

                LaneActionProjectile projectile = EnsureComponent<LaneActionProjectile>(editableRoot);
                SetBool(projectile, "allowVerticalTravel", allowVerticalTravel);
                if (string.Equals(prefabPath, RangedBasicProjectilePrefabPath, StringComparison.Ordinal))
                {
                    ConfigureRangedBasicProjectileVisuals(editableRoot, material);
                }
                else if (TryConfigureSkill1LaserProjectileVisuals(prefabPath, editableRoot))
                {
                }
                else if (TryConfigurePrimitiveSummonProjectileVisuals(prefabPath, editableRoot))
                {
                }
                else if (TryGetMagicMissilesProjectileVfxSpec(
                    prefabPath,
                    out string sourcePrefabPath,
                    out string visualName,
                    out Vector3 localScale))
                {
                    ConfigureMagicMissilesLaneProjectileVisuals(
                        editableRoot,
                        sourcePrefabPath,
                        visualName,
                        localScale);
                }

                PrefabUtility.SaveAsPrefabAsset(editableRoot, prefabPath);
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

            return LoadPrefabComponent<LaneActionProjectile>(prefabPath);
        }

        private static bool UsesAuthoredLaneProjectileVfx(string prefabPath)
        {
            return string.Equals(prefabPath, RangedBasicProjectilePrefabPath, StringComparison.Ordinal)
                || IsSkill1LaserProjectileVfx(prefabPath)
                || IsPrimitiveSummonProjectileVfx(prefabPath)
                || TryGetMagicMissilesProjectileVfxSpec(
                    prefabPath,
                    out _,
                    out _,
                    out _);
        }

        private static bool IsSkill1LaserProjectileVfx(string prefabPath)
        {
            return string.Equals(prefabPath, Skill1ProjectilePrefabPath, StringComparison.Ordinal);
        }

        private static bool IsPrimitiveSummonProjectileVfx(string prefabPath)
        {
            return string.Equals(prefabPath, SummonSlot2ProjectilePrefabPath, StringComparison.Ordinal)
                || string.Equals(prefabPath, SummonSlot3ProjectilePrefabPath, StringComparison.Ordinal);
        }

        private static bool TryConfigureSkill1LaserProjectileVisuals(string prefabPath, GameObject projectileRoot)
        {
            if (!IsSkill1LaserProjectileVfx(prefabPath))
            {
                return false;
            }

            const string VisualPrefix = "LaneActionProjectileVfx_";
            RemoveChildrenWithPrefix(projectileRoot.transform, VisualPrefix);
            TrailRenderer oldTrail = projectileRoot.GetComponent<TrailRenderer>();
            if (oldTrail != null)
            {
                UnityEngine.Object.DestroyImmediate(oldTrail);
            }

            EnsureSummonSlot2PromotedLaserBeamPrefab();
            GameObject beam = AttachPromotedVfxPrefab(
                projectileRoot.transform,
                VisualPrefix + "PlayerSkill1LaserBeam_FORGE3D",
                SummonSlot2PromotedLaserBeamPrefabPath,
                new Vector3(0f, 0f, 0.32f),
                Vector3.zero,
                new Vector3(0.34f, 0.34f, 0.62f),
                loopParticles: true,
                playOnAwake: true);
            EditorUtility.SetDirty(beam);
            EditorUtility.SetDirty(projectileRoot);
            return true;
        }

        private static bool TryConfigurePrimitiveSummonProjectileVisuals(string prefabPath, GameObject projectileRoot)
        {
            const string VisualPrefix = "LaneActionProjectileVfx_";
            if (string.Equals(prefabPath, SummonSlot2ProjectilePrefabPath, StringComparison.Ordinal))
            {
                RemoveChildrenWithPrefix(projectileRoot.transform, VisualPrefix);
                TrailRenderer oldTrail = projectileRoot.GetComponent<TrailRenderer>();
                if (oldTrail != null)
                {
                    UnityEngine.Object.DestroyImmediate(oldTrail);
                }

                AddProjectileVisualPrimitive(
                    projectileRoot.transform,
                    VisualPrefix + "LaserBolt",
                    PrimitiveType.Cube,
                    LoadOrCreateTransparentMaterial(
                        SummonSlot2LaserBeamMaterialPath,
                        new Color(0.18f, 0.92f, 1f, 0.72f)),
                    new Vector3(0f, 0f, -0.12f),
                    Vector3.zero,
                    new Vector3(0.08f, 0.08f, 1.62f));
                EditorUtility.SetDirty(projectileRoot);
                return true;
            }

            if (string.Equals(prefabPath, SummonSlot3ProjectilePrefabPath, StringComparison.Ordinal))
            {
                RemoveChildrenWithPrefix(projectileRoot.transform, VisualPrefix);
                TrailRenderer oldTrail = projectileRoot.GetComponent<TrailRenderer>();
                if (oldTrail != null)
                {
                    UnityEngine.Object.DestroyImmediate(oldTrail);
                }

                AttachPromotedVfxPrefab(
                    projectileRoot.transform,
                    VisualPrefix + "DragonFireBreath_FORGE3D",
                    SummonSlot3PromotedFireBreathPrefabPath,
                    new Vector3(0f, 0f, -0.18f),
                    Vector3.zero,
                    new Vector3(0.56f, 0.56f, 0.82f),
                    loopParticles: true,
                    playOnAwake: true);
                EditorUtility.SetDirty(projectileRoot);
                return true;
            }

            return false;
        }

        private static bool TryGetMagicMissilesProjectileVfxSpec(
            string prefabPath,
            out string sourcePrefabPath,
            out string visualName,
            out Vector3 localScale)
        {
            if (string.Equals(prefabPath, Skill1ProjectilePrefabPath, StringComparison.Ordinal))
            {
                sourcePrefabPath = ImportedMagicMissilesArcaneMissilePrefabPath;
                visualName = "MagicMissilesArcaneBolt";
                localScale = new Vector3(0.34f, 0.34f, 0.78f);
                return true;
            }

            if (string.Equals(prefabPath, SummonSlot1ProjectilePrefabPath, StringComparison.Ordinal))
            {
                sourcePrefabPath = ImportedMagicMissilesLightMissilePrefabPath;
                visualName = "MagicMissilesLightAssistBolt";
                localScale = new Vector3(0.38f, 0.38f, 0.86f);
                return true;
            }

            sourcePrefabPath = string.Empty;
            visualName = string.Empty;
            localScale = Vector3.one;
            return false;
        }

        private static void ConfigureMagicMissilesLaneProjectileVisuals(
            GameObject projectileRoot,
            string sourcePrefabPath,
            string visualName,
            Vector3 localScale)
        {
            const string VisualPrefix = "LaneActionProjectileVfx_";
            RemoveChildrenWithPrefix(projectileRoot.transform, VisualPrefix);
            TrailRenderer oldTrail = projectileRoot.GetComponent<TrailRenderer>();
            if (oldTrail != null)
            {
                UnityEngine.Object.DestroyImmediate(oldTrail);
            }

            AttachPromotedVfxPrefab(
                projectileRoot.transform,
                VisualPrefix + visualName,
                sourcePrefabPath,
                Vector3.zero,
                Vector3.zero,
                localScale,
                loopParticles: true,
                playOnAwake: true);
            EditorUtility.SetDirty(projectileRoot);
        }

        private static void ConfigureRangedBasicProjectileVisuals(GameObject projectileRoot, Material coreMaterial)
        {
            const string VisualPrefix = "RangedBasicProjectileVfx_";
            RemoveChildrenWithPrefix(projectileRoot.transform, VisualPrefix);
            TrailRenderer oldTrail = projectileRoot.GetComponent<TrailRenderer>();
            if (oldTrail != null)
            {
                UnityEngine.Object.DestroyImmediate(oldTrail);
            }

            GameObject shotVfx = CreateRangedBasicProjectileAssetVfx(projectileRoot.transform, VisualPrefix);
            EditorUtility.SetDirty(shotVfx);
            EditorUtility.SetDirty(projectileRoot);
        }

        private static GameObject CreateRangedBasicProjectileAssetVfx(Transform parent, string visualPrefix)
        {
            GameObject sourcePrefab = LoadAsset<GameObject>(ImportedRifleShotLoopedVfxPrefabPath);
            GameObject vfxInstance = PrefabUtility.InstantiatePrefab(sourcePrefab, parent.gameObject.scene) as GameObject;
            if (vfxInstance == null)
            {
                vfxInstance = UnityEngine.Object.Instantiate(sourcePrefab);
            }

            if (PrefabUtility.IsPartOfPrefabInstance(vfxInstance))
            {
                PrefabUtility.UnpackPrefabInstance(
                    vfxInstance,
                    PrefabUnpackMode.Completely,
                    InteractionMode.AutomatedAction);
            }

            vfxInstance.name = visualPrefix + "VefectsRifleShotLoop";
            vfxInstance.transform.SetParent(parent, worldPositionStays: false);
            vfxInstance.transform.localPosition = new Vector3(0f, 0f, -0.08f);
            vfxInstance.transform.localRotation = Quaternion.identity;
            vfxInstance.transform.localScale = new Vector3(0.72f, 0.72f, 1.55f);

            UnpackNestedPrefabInstances(vfxInstance);
            ConfigureRangedBasicProjectileAssetParticles(vfxInstance);
            RemapRangedBasicProjectileAssetRenderers(vfxInstance);
            DisableVfxAudioSources(vfxInstance);
            return vfxInstance;
        }

        private static void ConfigureRangedBasicProjectileAssetParticles(GameObject vfxRoot)
        {
            ParticleSystem[] particleSystems = vfxRoot.GetComponentsInChildren<ParticleSystem>(includeInactive: true);
            for (int i = 0; i < particleSystems.Length; i++)
            {
                ParticleSystem particleSystem = particleSystems[i];
                ParticleSystem.MainModule main = particleSystem.main;
                main.loop = true;
                main.playOnAwake = true;
                main.simulationSpace = ParticleSystemSimulationSpace.Local;
                main.scalingMode = ParticleSystemScalingMode.Hierarchy;
                ParticleSystem.EmissionModule emission = particleSystem.emission;
                emission.enabled = true;
                ParticleSystem.LightsModule lights = particleSystem.lights;
                if (lights.enabled && lights.light != null)
                {
                    lights.light = EnsurePromotedVefectsLight(lights.light);
                }

                particleSystem.Clear(withChildren: true);
                particleSystem.Play(withChildren: true);
                EditorUtility.SetDirty(particleSystem);
            }
        }

        private static void RemapRangedBasicProjectileAssetRenderers(GameObject vfxRoot)
        {
            Material frontMaterial = LoadAsset<Material>(MuzzleFlashFrontMaterialPath);
            Material sideMaterial = LoadAsset<Material>(MuzzleFlashSideMaterialPath);
            Material smokeMaterial = LoadAsset<Material>(MuzzleSmokeMaterialPath);

            ParticleSystemRenderer[] renderers =
                vfxRoot.GetComponentsInChildren<ParticleSystemRenderer>(includeInactive: true);
            for (int i = 0; i < renderers.Length; i++)
            {
                ParticleSystemRenderer renderer = renderers[i];
                string normalizedName = NormalizeTransformName(renderer.name);
                if (normalizedName.Contains("smoke", StringComparison.Ordinal))
                {
                    renderer.sharedMaterial = smokeMaterial;
                }
                else if (normalizedName.Contains("side", StringComparison.Ordinal)
                    && !normalizedName.Contains("frontside", StringComparison.Ordinal))
                {
                    renderer.sharedMaterial = sideMaterial;
                }
                else
                {
                    renderer.sharedMaterial = frontMaterial;
                }

                Mesh mesh = renderer.mesh;
                if (mesh != null)
                {
                    renderer.mesh = EnsurePromotedVefectsMesh(mesh);
                }

                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                renderer.allowOcclusionWhenDynamic = false;
                EditorUtility.SetDirty(renderer);
            }
        }

        private static Light EnsurePromotedVefectsLight(Light sourceLight)
        {
            string sourcePath = AssetDatabase.GetAssetPath(sourceLight).Replace('\\', '/');
            if (sourcePath.StartsWith("Assets/_Game/", StringComparison.Ordinal))
            {
                return sourceLight;
            }

            string sourceName = string.IsNullOrWhiteSpace(sourcePath)
                ? sourceLight.name
                : System.IO.Path.GetFileNameWithoutExtension(sourcePath);
            string targetPath = CombatVfxPrefabRoot + "/DB_Vefects_"
                + SanitizeAssetFileName(sourceName)
                + ".prefab";
            EnsureFolderForAsset(targetPath);

            GameObject promotedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(targetPath);
            if (promotedPrefab == null)
            {
                GameObject lightRoot = new GameObject("DB_Vefects_" + SanitizeAssetFileName(sourceName));
                try
                {
                    Light promotedLight = lightRoot.AddComponent<Light>();
                    promotedLight.type = sourceLight.type;
                    promotedLight.color = sourceLight.color;
                    promotedLight.intensity = sourceLight.intensity;
                    promotedLight.range = sourceLight.range;
                    promotedLight.spotAngle = sourceLight.spotAngle;
                    promotedLight.shadows = sourceLight.shadows;
                    promotedLight.shadowStrength = sourceLight.shadowStrength;
                    promotedLight.renderMode = sourceLight.renderMode;
                    PrefabUtility.SaveAsPrefabAsset(lightRoot, targetPath);
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(lightRoot);
                }

                promotedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(targetPath);
            }

            Light promotedPrefabLight = promotedPrefab != null ? promotedPrefab.GetComponent<Light>() : null;
            if (promotedPrefabLight == null)
            {
                throw new InvalidOperationException($"Failed to promote Vefects light prefab at {targetPath}.");
            }

            return promotedPrefabLight;
        }

        private static Mesh EnsurePromotedVefectsMesh(Mesh sourceMesh)
        {
            string sourcePath = AssetDatabase.GetAssetPath(sourceMesh).Replace('\\', '/');
            if (sourcePath.StartsWith("Assets/_Game/", StringComparison.Ordinal))
            {
                return sourceMesh;
            }

            if (string.IsNullOrWhiteSpace(sourcePath)
                || !sourcePath.StartsWith("Assets/_Imported/", StringComparison.Ordinal))
            {
                return sourceMesh;
            }

            string targetPath = CombatVfxMeshRoot + "/" + System.IO.Path.GetFileName(sourcePath);
            EnsureFolderForAsset(targetPath);
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(targetPath) == null)
            {
                if (!AssetDatabase.CopyAsset(sourcePath, targetPath))
                {
                    throw new InvalidOperationException($"Failed to promote Vefects mesh from {sourcePath} to {targetPath}.");
                }

                AssetDatabase.ImportAsset(targetPath, ImportAssetOptions.ForceUpdate);
            }

            UnityEngine.Object[] promotedAssets = AssetDatabase.LoadAllAssetsAtPath(targetPath);
            for (int i = 0; i < promotedAssets.Length; i++)
            {
                if (promotedAssets[i] is Mesh promotedMesh
                    && string.Equals(promotedMesh.name, sourceMesh.name, StringComparison.Ordinal))
                {
                    return promotedMesh;
                }
            }

            for (int i = 0; i < promotedAssets.Length; i++)
            {
                if (promotedAssets[i] is Mesh promotedMesh)
                {
                    return promotedMesh;
                }
            }

            throw new InvalidOperationException($"Failed to load promoted Vefects mesh {sourceMesh.name} from {targetPath}.");
        }

        private static GameObject AttachPromotedVfxPrefab(
            Transform parent,
            string childName,
            string sourcePrefabPath,
            Vector3 localPosition,
            Vector3 localEuler,
            Vector3 localScale,
            bool loopParticles,
            bool playOnAwake)
        {
            DestroyChildIfPresent(parent, childName);
            GameObject sourcePrefab = LoadAsset<GameObject>(sourcePrefabPath);
            GameObject vfxInstance = PrefabUtility.InstantiatePrefab(sourcePrefab, parent.gameObject.scene) as GameObject;
            if (vfxInstance == null)
            {
                vfxInstance = UnityEngine.Object.Instantiate(sourcePrefab);
            }

            if (PrefabUtility.IsPartOfPrefabInstance(vfxInstance))
            {
                PrefabUtility.UnpackPrefabInstance(
                    vfxInstance,
                    PrefabUnpackMode.Completely,
                    InteractionMode.AutomatedAction);
            }

            vfxInstance.name = childName;
            vfxInstance.transform.SetParent(parent, worldPositionStays: false);
            vfxInstance.transform.localPosition = localPosition;
            vfxInstance.transform.localRotation = Quaternion.Euler(localEuler);
            vfxInstance.transform.localScale = localScale;

            UnpackNestedPrefabInstances(vfxInstance);
            StripNonGameMonoBehaviours(vfxInstance);
            RemoveColliders(vfxInstance);
            DisableVfxAudioSources(vfxInstance);
            ConfigurePromotedVfxParticles(vfxInstance, loopParticles, playOnAwake);
            RemapPromotedVfxRenderers(vfxInstance);
            EditorUtility.SetDirty(vfxInstance);
            return vfxInstance;
        }

        private static void UnpackNestedPrefabInstances(GameObject root)
        {
            Transform[] transforms = root.GetComponentsInChildren<Transform>(includeInactive: true);
            for (int i = transforms.Length - 1; i >= 0; i--)
            {
                GameObject candidate = transforms[i].gameObject;
                if (candidate != root
                    && PrefabUtility.IsAnyPrefabInstanceRoot(candidate)
                    && PrefabUtility.IsPartOfPrefabInstance(candidate))
                {
                    PrefabUtility.UnpackPrefabInstance(
                        candidate,
                        PrefabUnpackMode.Completely,
                        InteractionMode.AutomatedAction);
                }
            }
        }

        private static void ConfigurePromotedVfxParticles(
            GameObject vfxRoot,
            bool loopParticles,
            bool playOnAwake)
        {
            ParticleSystem[] particleSystems = vfxRoot.GetComponentsInChildren<ParticleSystem>(includeInactive: true);
            for (int i = 0; i < particleSystems.Length; i++)
            {
                ParticleSystem particleSystem = particleSystems[i];
                ParticleSystem.MainModule main = particleSystem.main;
                main.loop = loopParticles;
                main.playOnAwake = playOnAwake;
                main.simulationSpace = ParticleSystemSimulationSpace.Local;
                main.scalingMode = ParticleSystemScalingMode.Hierarchy;

                ParticleSystem.EmissionModule emission = particleSystem.emission;
                emission.enabled = true;
                particleSystem.Clear(withChildren: true);
                if (playOnAwake)
                {
                    particleSystem.Play(withChildren: true);
                }

                EditorUtility.SetDirty(particleSystem);
            }
        }

        private static void RemapPromotedVfxRenderers(GameObject vfxRoot)
        {
            Renderer[] renderers = vfxRoot.GetComponentsInChildren<Renderer>(includeInactive: true);
            Material fallbackMaterial = null;
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                Material[] materials = renderer.sharedMaterials;
                for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                {
                    if (materials[materialIndex] != null)
                    {
                        materials[materialIndex] = EnsurePromotedMagicMissilesMaterial(materials[materialIndex]);
                        fallbackMaterial ??= materials[materialIndex];
                    }
                }

                renderer.sharedMaterials = materials;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                renderer.allowOcclusionWhenDynamic = false;
                EditorUtility.SetDirty(renderer);
            }

            if (fallbackMaterial != null)
            {
                for (int i = 0; i < renderers.Length; i++)
                {
                    Renderer renderer = renderers[i];
                    if (renderer == null)
                    {
                        continue;
                    }

                    Material[] materials = renderer.sharedMaterials;
                    bool changed = false;
                    for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                    {
                        if (materials[materialIndex] == null)
                        {
                            materials[materialIndex] = fallbackMaterial;
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

            ParticleSystemRenderer[] particleRenderers =
                vfxRoot.GetComponentsInChildren<ParticleSystemRenderer>(includeInactive: true);
            for (int i = 0; i < particleRenderers.Length; i++)
            {
                ParticleSystemRenderer renderer = particleRenderers[i];
                if (renderer.mesh != null)
                {
                    renderer.mesh = EnsurePromotedMagicMissilesMesh(renderer.mesh);
                }

                EditorUtility.SetDirty(renderer);
            }

            MeshFilter[] meshFilters = vfxRoot.GetComponentsInChildren<MeshFilter>(includeInactive: true);
            for (int i = 0; i < meshFilters.Length; i++)
            {
                if (meshFilters[i].sharedMesh != null)
                {
                    meshFilters[i].sharedMesh = EnsurePromotedMagicMissilesMesh(meshFilters[i].sharedMesh);
                    EditorUtility.SetDirty(meshFilters[i]);
                }
            }
        }

        private static Material EnsurePromotedMagicMissilesMaterial(Material sourceMaterial)
        {
            string sourcePath = AssetDatabase.GetAssetPath(sourceMaterial).Replace('\\', '/');
            if (sourcePath.StartsWith("Assets/_Game/", StringComparison.Ordinal))
            {
                return sourceMaterial;
            }

            string targetPath = MagicMissilesMaterialRoot + "/DB_MagicMissiles_"
                + SanitizeAssetFileName(sourceMaterial.name)
                + ".mat";
            EnsureFolderForAsset(targetPath);
            Material material = AssetDatabase.LoadAssetAtPath<Material>(targetPath);
            if (material == null)
            {
                material = new Material(ResolveUnlitShader());
                AssetDatabase.CreateAsset(material, targetPath);
            }

            material.shader = ResolveUnlitShader();
            ConfigureTransparentVfxMaterial(material, ResolveMagicMissilesMaterialColor(sourceMaterial));

            string[] textureProperties = sourceMaterial.GetTexturePropertyNames();
            for (int i = 0; i < textureProperties.Length; i++)
            {
                Texture texture = sourceMaterial.GetTexture(textureProperties[i]);
                if (texture == null)
                {
                    continue;
                }

                Texture promotedTexture = EnsurePromotedMagicMissilesTexture(texture);
                SetTextureIfPresent(material, textureProperties[i], promotedTexture);
                SetTextureIfPresent(material, "_MainTex", promotedTexture);
                SetTextureIfPresent(material, "_BaseMap", promotedTexture);
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static void ConfigureTransparentVfxMaterial(Material material, Color color)
        {
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
                material.SetColor("_EmissionColor", color * 1.35f);
            }

            SetMaterialFloatIfPresent(material, "_Surface", 1f);
            SetMaterialFloatIfPresent(material, "_Blend", 2f);
            SetMaterialFloatIfPresent(material, "_SrcBlend", (float)BlendMode.SrcAlpha);
            SetMaterialFloatIfPresent(material, "_DstBlend", (float)BlendMode.One);
            SetMaterialFloatIfPresent(material, "_ZWrite", 0f);
            material.renderQueue = (int)RenderQueue.Transparent;
            material.SetOverrideTag("RenderType", "Transparent");
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
        }

        private static Color ResolveMagicMissilesMaterialColor(Material sourceMaterial)
        {
            if (sourceMaterial.HasProperty("_TintColor"))
            {
                return sourceMaterial.GetColor("_TintColor");
            }

            if (sourceMaterial.HasProperty("_BaseColor"))
            {
                return sourceMaterial.GetColor("_BaseColor");
            }

            if (sourceMaterial.HasProperty("_Color"))
            {
                return sourceMaterial.GetColor("_Color");
            }

            return Color.white;
        }

        private static void SetTextureIfPresent(Material material, string propertyName, Texture texture)
        {
            if (material != null && texture != null && material.HasProperty(propertyName))
            {
                material.SetTexture(propertyName, texture);
            }
        }

        private static Texture EnsurePromotedMagicMissilesTexture(Texture sourceTexture)
        {
            string sourcePath = AssetDatabase.GetAssetPath(sourceTexture).Replace('\\', '/');
            if (sourcePath.StartsWith("Assets/_Game/", StringComparison.Ordinal) || string.IsNullOrWhiteSpace(sourcePath))
            {
                return sourceTexture;
            }

            string targetPath = MagicMissilesTextureRoot + "/"
                + SanitizeAssetFileName(System.IO.Path.GetFileNameWithoutExtension(sourcePath))
                + System.IO.Path.GetExtension(sourcePath);
            EnsureFolderForAsset(targetPath);
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(targetPath) == null)
            {
                if (!AssetDatabase.CopyAsset(sourcePath, targetPath))
                {
                    throw new InvalidOperationException(
                        $"Failed to promote MagicMissiles texture from {sourcePath} to {targetPath}.");
                }

                AssetDatabase.ImportAsset(targetPath, ImportAssetOptions.ForceUpdate);
            }

            Texture promotedTexture = AssetDatabase.LoadAssetAtPath<Texture>(targetPath);
            if (promotedTexture == null)
            {
                throw new InvalidOperationException($"Failed to load promoted MagicMissiles texture at {targetPath}.");
            }

            return promotedTexture;
        }

        private static void DisableVfxAudioSources(GameObject vfxRoot)
        {
            AudioSource[] audioSources = vfxRoot.GetComponentsInChildren<AudioSource>(includeInactive: true);
            for (int i = 0; i < audioSources.Length; i++)
            {
                AudioSource audioSource = audioSources[i];
                audioSource.clip = null;
                audioSource.playOnAwake = false;
                audioSource.loop = false;
                audioSource.Stop();
                EditorUtility.SetDirty(audioSource);
            }
        }

        private static Mesh EnsurePromotedMagicMissilesMesh(Mesh sourceMesh)
        {
            string sourcePath = AssetDatabase.GetAssetPath(sourceMesh).Replace('\\', '/');
            if (sourcePath.StartsWith("Assets/_Game/", StringComparison.Ordinal))
            {
                return sourceMesh;
            }

            if (string.IsNullOrWhiteSpace(sourcePath)
                || sourcePath.StartsWith("Library/", StringComparison.Ordinal))
            {
                string generatedTargetPath = MagicMissilesMeshRoot + "/DB_MagicMissiles_"
                    + SanitizeAssetFileName(sourceMesh.name)
                    + ".asset";
                EnsureFolderForAsset(generatedTargetPath);
                Mesh generatedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(generatedTargetPath);
                if (generatedMesh == null)
                {
                    generatedMesh = UnityEngine.Object.Instantiate(sourceMesh);
                    generatedMesh.name = sourceMesh.name;
                    AssetDatabase.CreateAsset(generatedMesh, generatedTargetPath);
                    AssetDatabase.ImportAsset(generatedTargetPath, ImportAssetOptions.ForceUpdate);
                }

                return generatedMesh;
            }

            if (!sourcePath.StartsWith("Assets/_Imported/", StringComparison.Ordinal))
            {
                string generatedTargetPath = MagicMissilesMeshRoot + "/DB_MagicMissiles_"
                    + SanitizeAssetFileName(sourceMesh.name)
                    + ".asset";
                EnsureFolderForAsset(generatedTargetPath);
                Mesh generatedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(generatedTargetPath);
                if (generatedMesh == null)
                {
                    generatedMesh = UnityEngine.Object.Instantiate(sourceMesh);
                    generatedMesh.name = sourceMesh.name;
                    AssetDatabase.CreateAsset(generatedMesh, generatedTargetPath);
                    AssetDatabase.ImportAsset(generatedTargetPath, ImportAssetOptions.ForceUpdate);
                }

                return generatedMesh;
            }

            string targetPath = MagicMissilesMeshRoot + "/"
                + SanitizeAssetFileName(System.IO.Path.GetFileName(sourcePath));
            EnsureFolderForAsset(targetPath);
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(targetPath) == null)
            {
                if (!AssetDatabase.CopyAsset(sourcePath, targetPath))
                {
                    throw new InvalidOperationException(
                        $"Failed to promote MagicMissiles mesh from {sourcePath} to {targetPath}.");
                }

                AssetDatabase.ImportAsset(targetPath, ImportAssetOptions.ForceUpdate);
            }

            UnityEngine.Object[] promotedAssets = AssetDatabase.LoadAllAssetsAtPath(targetPath);
            for (int i = 0; i < promotedAssets.Length; i++)
            {
                if (promotedAssets[i] is Mesh promotedMesh
                    && string.Equals(promotedMesh.name, sourceMesh.name, StringComparison.Ordinal))
                {
                    return promotedMesh;
                }
            }

            for (int i = 0; i < promotedAssets.Length; i++)
            {
                if (promotedAssets[i] is Mesh promotedMesh)
                {
                    return promotedMesh;
                }
            }

            return sourceMesh;
        }

        private static void RemoveColliders(GameObject root)
        {
            Collider[] colliders = root.GetComponentsInChildren<Collider>(includeInactive: true);
            for (int i = colliders.Length - 1; i >= 0; i--)
            {
                UnityEngine.Object.DestroyImmediate(colliders[i]);
            }
        }

        private static string SanitizeAssetFileName(string rawName)
        {
            if (string.IsNullOrWhiteSpace(rawName))
            {
                return "Asset";
            }

            char[] invalid = System.IO.Path.GetInvalidFileNameChars();
            string safe = rawName.Trim();
            for (int i = 0; i < invalid.Length; i++)
            {
                safe = safe.Replace(invalid[i], '_');
            }

            return safe.Replace(' ', '_');
        }

        private static Renderer AddProjectileVisualPrimitive(
            Transform parent,
            string name,
            PrimitiveType primitiveType,
            Material material,
            Vector3 localPosition,
            Vector3 localEuler,
            Vector3 localScale)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent, worldPositionStays: false);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.Euler(localEuler);
            primitive.transform.localScale = localScale;

            MeshFilter meshFilter = EnsureComponent<MeshFilter>(primitive);
            meshFilter.sharedMesh = LoadPrimitiveMesh(primitiveType);

            MeshRenderer renderer = EnsureComponent<MeshRenderer>(primitive);
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.allowOcclusionWhenDynamic = false;

            Collider collider = primitive.GetComponent<Collider>();
            if (collider != null)
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }

            EditorUtility.SetDirty(primitive);
            return renderer;
        }

        private static SummonLaneSpace CreateLaneSpace(Scene scene)
        {
            GameObject laneRoot = CreateRoot(scene, LaneRootName);
            SummonLaneSpace laneSpace = laneRoot.AddComponent<SummonLaneSpace>();
            laneRoot.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            SetFloat(laneSpace, "halfWidth", 5.25f);
            SetFloat(laneSpace, "backLimitZ", -12f);
            SetFloat(laneSpace, "forwardBoundaryZ", 0f);
            SetFloat(laneSpace, "bossProxyZ", 18f);
            SetFloat(laneSpace, "summonEntryZ", 2.25f);
            return laneSpace;
        }

        private static GameObject CreateBossProxy(
            Scene scene,
            SummonLaneSpace laneSpace,
            BossBarragePatternProfile patternProfile,
            BossBarragePatternProfile coverFirePatternProfile,
            BossBarragePatternProfile escortScreenPatternProfile,
            BossBarragePatternProfile layeredSalvoPatternProfile,
            BossBarragePatternProfile staggeredCrossfirePatternProfile,
            BossBarragePatternProfile twinSweepPatternProfile,
            BossBarragePatternProfile leftClampPatternProfile,
            BossBarragePatternProfile rightClampPatternProfile,
            BossBarragePatternProfile punishNetPatternProfile,
            BossBarragePatternProfile linePressurePatternProfile,
            BossBasicFireProfile bossBasicFireProfile,
            BossBarrageProjectile projectilePrefab,
            Transform projectileRoot,
            SummonFrontlineProxy bossSummonActorPrefab,
            Transform bossSummonActorRoot)
        {
            GameObject bossProxy = CreateRoot(scene, BossProxyRootName);
            bossProxy.transform.SetPositionAndRotation(
                laneSpace.GetLaneWorldPoint(0f, laneSpace.BossProxyZ, 1.6f),
                Quaternion.LookRotation(Vector3.back, Vector3.up));
            Transform playerTransform = RequireObject<PlayerMovementController>(scene, "player movement").transform;

            CombatHealth bossHealth = EnsureComponent<CombatHealth>(bossProxy);
            bossHealth.ConfigureTeam(DamageTeam.Enemy);
            SetFloat(bossHealth, "maxHealth", BossProxyReviewMaxHealth);
            ConfigureBossProxyBodyHitbox(bossProxy);
            CreateBossProxyVisual(bossProxy.transform);

            BossBarrageEmitter emitter = EnsureComponent<BossBarrageEmitter>(bossProxy);
            SetObjectReference(emitter, "laneSpace", laneSpace);
            SetObjectReference(emitter, "trackedPlayer", playerTransform);
            SetObjectReference(emitter, "sourceHealth", bossHealth);
            SetObjectReference(emitter, "patternProfile", patternProfile);
            SetObjectReferenceArray(
                emitter,
                "patternSequence",
                new UnityEngine.Object[]
                {
                    linePressurePatternProfile,
                    layeredSalvoPatternProfile,
                    patternProfile,
                    coverFirePatternProfile,
                    escortScreenPatternProfile,
                    staggeredCrossfirePatternProfile,
                    twinSweepPatternProfile,
                    leftClampPatternProfile,
                    rightClampPatternProfile,
                    punishNetPatternProfile
                });
            SetInt(emitter, "wavesPerPattern", 1);
            SetObjectReference(emitter, "projectilePrefab", projectilePrefab);
            SetObjectReference(emitter, "projectilePrefabObject", LoadAsset<GameObject>(ProjectilePrefabPath));
            SetObjectReference(emitter, "projectileRoot", projectileRoot);
            SetInt(emitter, "sourceTeam", (int)DamageTeam.Enemy);
            SetBool(emitter, "firingEnabled", true);
            SetInt(emitter, "prewarmCount", 24);

            BossBasicFireEmitter basicFireEmitter = ConfigureBossBasicFireEmitter(
                bossProxy,
                laneSpace,
                playerTransform,
                bossHealth,
                projectileRoot);

            BossPressureCostLadder bossPressureCost = EnsureComponent<BossPressureCostLadder>(bossProxy);
            bossPressureCost.ConfigureReferences(laneSpace, bossProxy.transform);
            SetFloat(bossPressureCost, "baseCostPerSecond", BossPressureBaseCostPerSecond);
            SetFloat(bossPressureCost, "fallbackBossForwardRisk01", 0.25f);

            BossSummonPressureAction bossSummonPressureAction = EnsureComponent<BossSummonPressureAction>(bossProxy);
            CombatVfxCuePlayer playerCuePlayer =
                RequireComponent<CombatVfxCuePlayer>(playerTransform.gameObject, "player combat VFX cue player");
            bossSummonPressureAction.ConfigureReferences(
                laneSpace,
                playerTransform,
                bossSummonActorPrefab,
                bossSummonActorRoot,
                playerCuePlayer);
            SetObjectReference(bossSummonPressureAction, "summonActorPrefab", bossSummonActorPrefab);
            SetObjectReference(
                bossSummonPressureAction,
                "summonActorPrefabObject",
                LoadAsset<GameObject>(BossSummonPressureActorPrefabPath));
            SetObjectReference(bossSummonPressureAction, "summonActorRoot", bossSummonActorRoot);
            SetObjectReference(bossSummonPressureAction, "combatVfxCuePlayer", playerCuePlayer);
            SetEnum(bossSummonPressureAction, "ownerTeam", (int)DamageTeam.Enemy);
            SetInt(bossSummonPressureAction, "actorPrewarmCount", 3);
            SetInt(bossSummonPressureAction, "maxActiveSummonActors", 2);
            SetFloat(bossSummonPressureAction, "actorEntryCatchupSecondsPerMeter", 0.55f);
            SetFloat(bossSummonPressureAction, "minimumPlayerSideTargetDepth", 1.2f);
            bossSummonPressureAction.ConfigurePressureProfile(
                LoadAsset<BossSummonPressureProfile>(BossSummonPressureProfilePath));

            EnemySummonPacingDirector enemySummonPacingDirector =
                ConfigureEnemySummonPacingDirector(bossProxy, bossSummonPressureAction);

            BossPressureActionDirector bossPressureActionDirector =
                EnsureComponent<BossPressureActionDirector>(bossProxy);
            bossPressureActionDirector.ConfigureReferences(
                bossPressureCost,
                emitter,
                bossSummonPressureAction,
                laneSpace,
                playerTransform,
                basicFireEmitter);
            bossPressureActionDirector.ConfigureActionDeck(
                LoadAsset<BossPressureActionDeckProfile>(BossPressureActionDeckProfilePath));
            bossPressureActionDirector.SetHoldForNextTierActionWhenGateAllows(true);
            SetBool(bossPressureActionDirector, "actionsEnabled", true);
            SetFloat(bossPressureActionDirector, "playerSummonResponseWindowSeconds", 4f);
            SetFloat(bossPressureActionDirector, "basicFireSuppressionSecondsAfterPressureAction", 0.2f);
            SetInt(bossPressureActionDirector, "minimumBasicFireVolleysBeforePressureAction", 4);
            SetFloat(bossPressureActionDirector, "minimumBasicFireAgeBeforePressureActionSeconds", 0.08f);

            BossPressurePositionController bossPressurePosition =
                EnsureComponent<BossPressurePositionController>(bossProxy);
            bossPressurePosition.ConfigureReferences(
                laneSpace,
                bossPressureCost,
                bossPressureActionDirector,
                bossProxy.transform,
                playerTransform);
            SetObjectReference(bossPressurePosition, "movedTransform", bossProxy.transform);
            SetObjectReference(bossPressurePosition, "trackedPlayer", playerTransform);
            SetFloat(bossPressurePosition, "restRisk01", 0.18f);
            SetFloat(bossPressurePosition, "maxCommitRisk01", 0.9f);
            SetFloat(bossPressurePosition, "advanceRiskPerSecond", 0.46f);
            SetFloat(bossPressurePosition, "retreatRiskPerSecond", 0.38f);
            SetBool(bossPressurePosition, "returnToRestWhenActionsDisabled", true);
            SetBool(bossPressurePosition, "movementEnabled", true);
            SetFloat(bossPressurePosition, "actionIntentHoldSeconds", 1.65f);
            SetFloat(bossPressurePosition, "holdBacklineRisk01", 0.22f);
            SetFloat(bossPressurePosition, "strafeFireRisk01", 0.52f);
            SetFloat(bossPressurePosition, "specialCommitRisk01", 0.82f);
            SetFloat(bossPressurePosition, "summonRetreatRisk01", 0.1f);
            SetFloat(bossPressurePosition, "punishCommitRisk01", 0.9f);
            SetBool(bossPressurePosition, "lateralStrafeEnabled", true);
            SetFloat(bossPressurePosition, "lateralStrafeUnitsPerSecond", 0.9f);
            SetFloat(bossPressurePosition, "lateralStrafeHalfWidthRatio", 0.34f);
            SetBool(bossPressurePosition, "playerResponseEnabled", true);
            SetFloat(bossPressurePosition, "playerLateralFollowStrength", 0.82f);
            SetFloat(bossPressurePosition, "playerResponseHalfWidthRatio", 0.52f);
            SetFloat(bossPressurePosition, "playerResponseLateralUnitsPerSecond", 2.6f);
            SetFloat(bossPressurePosition, "playerFlankOffsetRatio", 0.18f);
            SetFloat(bossPressurePosition, "playerFlankSwitchSeconds", 0.9f);
            SetFloat(bossPressurePosition, "commitPlayerFollowBoost", 0.24f);
            SetBool(bossPressurePosition, "faceTrackedPlayer", true);
            SetFloat(bossPressurePosition, "turnDegreesPerSecond", 780f);
            SetBool(bossPressurePosition, "forwardPressureOscillationEnabled", true);
            SetFloat(bossPressurePosition, "idleForwardRiskAmplitude", 0.025f);
            SetFloat(bossPressurePosition, "actionForwardRiskAmplitude", 0.05f);
            SetFloat(bossPressurePosition, "forwardOscillationSeconds", 2.35f);
            SetFloat(bossPressurePosition, "commitRiskBoost", 0.04f);
            SetFloat(bossPressurePosition, "retreatRiskDip", 0.035f);
            SetString(bossPressurePosition, "movementSpeedParameter", "MoveSpeed");
            SetString(bossPressurePosition, "alternateMovementSpeedParameter", "Speed");
            SetString(bossPressurePosition, "basicFireTrigger", "Attack");
            SetString(bossPressurePosition, "retreatStepTrigger", "RetreatBackstep");
            SetFloat(bossPressurePosition, "animatorMoveSpeedScale", 0.28f);
            SetFloat(bossPressurePosition, "animatorDampSeconds", 0.1f);
            SetFloat(bossPressurePosition, "basicFireMovementLockSeconds", 0.34f);
            SetFloat(bossPressurePosition, "retreatAnimationRiskDelta", 0.025f);
            SetFloat(bossPressurePosition, "retreatTriggerCooldownSeconds", 1.05f);
            EditorUtility.SetDirty(bossPressureCost);
            EditorUtility.SetDirty(basicFireEmitter);
            EditorUtility.SetDirty(bossSummonPressureAction);
            EditorUtility.SetDirty(enemySummonPacingDirector);
            EditorUtility.SetDirty(bossPressureActionDirector);
            EditorUtility.SetDirty(bossPressurePosition);

            ConfigureBossProxyVisualCueDriver(bossProxy, emitter, bossPressureActionDirector);
            return bossProxy;
        }

        private static void PatchBossBarrageLaneReviewEnemySummonPacing(string scenePath)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) == null)
            {
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            GameObject bossProxy = RequireRoot(scene, BossProxyRootName);
            BossSummonPressureAction bossSummonPressureAction =
                RequireComponent<BossSummonPressureAction>(bossProxy, "boss summon pressure action");
            EnemySummonPacingDirector enemySummonPacingDirector =
                ConfigureEnemySummonPacingDirector(bossProxy, bossSummonPressureAction);
            EditorUtility.SetDirty(enemySummonPacingDirector);
            EditorUtility.SetDirty(bossProxy);

            if (!EditorSceneManager.SaveScene(scene, scenePath))
            {
                throw new InvalidOperationException($"Failed to save boss enemy summon pacing in {scenePath}.");
            }
        }

        private static EnemySummonPacingDirector ConfigureEnemySummonPacingDirector(
            GameObject bossProxy,
            BossSummonPressureAction bossSummonPressureAction)
        {
            EnemySummonPacingDirector enemySummonPacingDirector =
                EnsureComponent<EnemySummonPacingDirector>(bossProxy);
            enemySummonPacingDirector.ConfigureReferences(bossSummonPressureAction);
            enemySummonPacingDirector.ConfigurePacing(
                newInitialDelaySeconds: EnemySummonPacingInitialDelaySeconds,
                newRespawnIntervalSeconds: EnemySummonPacingRespawnIntervalSeconds,
                newSummonTier: 1,
                newRetryIntervalSeconds: EnemySummonPacingRetryIntervalSeconds,
                newSummonTierSequence: BossEnemySummonPacingTierSequence);
            enemySummonPacingDirector.SetPacingEnabled(true);
            SetObjectReference(enemySummonPacingDirector, "summonPressureAction", bossSummonPressureAction);
            SetBool(enemySummonPacingDirector, "pacingEnabled", true);
            SetInt(enemySummonPacingDirector, "summonTier", 1);
            SetFloat(enemySummonPacingDirector, "initialDelaySeconds", EnemySummonPacingInitialDelaySeconds);
            SetFloat(enemySummonPacingDirector, "respawnIntervalSeconds", EnemySummonPacingRespawnIntervalSeconds);
            SetFloat(enemySummonPacingDirector, "retryIntervalSeconds", EnemySummonPacingRetryIntervalSeconds);
            SetIntArray(enemySummonPacingDirector, "summonTierSequence", BossEnemySummonPacingTierSequence);
            return enemySummonPacingDirector;
        }

        private static BossBasicFireEmitter ConfigureBossBasicFireEmitter(
            GameObject bossProxy,
            SummonLaneSpace laneSpace,
            Transform playerTransform,
            CombatHealth bossHealth,
            Transform projectileRoot)
        {
            BossBasicFireEmitter basicFireEmitter = EnsureComponent<BossBasicFireEmitter>(bossProxy);
            SetObjectReference(basicFireEmitter, "laneSpace", laneSpace);
            SetObjectReference(basicFireEmitter, "trackedPlayer", playerTransform);
            SetObjectReference(basicFireEmitter, "sourceHealth", bossHealth);
            SetObjectReference(basicFireEmitter, "fireOrigin", EnsureBossBasicFireOrigin(bossProxy));
            SetObjectReference(basicFireEmitter, "fireProfile", LoadAsset<BossBasicFireProfile>(BossBasicFireProfilePath));
            SetObjectReference(basicFireEmitter, "projectilePrefab", LoadPrefabComponent<BossBarrageProjectile>(ProjectilePrefabPath));
            SetObjectReference(basicFireEmitter, "projectilePrefabObject", LoadAsset<GameObject>(ProjectilePrefabPath));
            SetObjectReference(basicFireEmitter, "projectileRoot", projectileRoot);
            SetInt(basicFireEmitter, "sourceTeam", (int)DamageTeam.Enemy);
            SetBool(basicFireEmitter, "firingEnabled", true);
            SetFloat(basicFireEmitter, "resumeCooldownAfterSuppressionSeconds", 0.25f);
            SetInt(basicFireEmitter, "prewarmCount", 10);
            Transform audioAnchor = EnsureChild(bossProxy.transform, "BossBasicFireAudio");
            audioAnchor.localPosition = new Vector3(0f, 1.35f, 0f);
            audioAnchor.localRotation = Quaternion.identity;
            audioAnchor.localScale = Vector3.one;
            AudioSource volleyAudioSource = EnsureComponent<AudioSource>(audioAnchor.gameObject);
            volleyAudioSource.playOnAwake = false;
            volleyAudioSource.loop = false;
            volleyAudioSource.volume = 0.34f;
            volleyAudioSource.pitch = 1f;
            volleyAudioSource.spatialBlend = 0.25f;
            volleyAudioSource.dopplerLevel = 0f;
            volleyAudioSource.rolloffMode = AudioRolloffMode.Linear;
            volleyAudioSource.minDistance = 4f;
            volleyAudioSource.maxDistance = 32f;
            volleyAudioSource.priority = 138;
            AudioClip basicFireClip = LoadAsset<AudioClip>(BossBasicFireSfxClipPath);
            basicFireEmitter.ConfigureVolleyAudio(
                volleyAudioSource,
                new[] { basicFireClip },
                0.34f,
                new Vector2(0.96f, 1.04f));
            SetObjectReference(basicFireEmitter, "volleyAudioSource", volleyAudioSource);
            SetObjectReferenceArray(basicFireEmitter, "volleySfxClips", new UnityEngine.Object[] { basicFireClip });
            SetFloat(basicFireEmitter, "volleySfxVolume", 0.34f);
            SetVector2(basicFireEmitter, "volleySfxPitchRange", new Vector2(0.96f, 1.04f));
            EditorUtility.SetDirty(audioAnchor.gameObject);
            EditorUtility.SetDirty(volleyAudioSource);
            return basicFireEmitter;
        }

        private static GameObject CreateCloseThreat(
            Scene scene,
            SummonLaneSpace laneSpace,
            Transform player,
            CombatHealth playerHealth,
            ActionCameraController cameraController)
        {
            GameObject prefab = LoadAsset<GameObject>(ActionFoundationEnemyPrefabSetup.MeleeSoldierPrefabPath);
            GameObject closeThreat = PrefabUtility.InstantiatePrefab(prefab, scene) as GameObject;
            if (closeThreat == null)
            {
                throw new InvalidOperationException($"Could not instantiate close-threat prefab {ActionFoundationEnemyPrefabSetup.MeleeSoldierPrefabPath}.");
            }

            Vector3 position = laneSpace.GetLaneWorldPoint(-0.35f, -2.65f, 0f);
            Vector3 toPlayer = Vector3.ProjectOnPlane(player.position - position, Vector3.up);
            if (toPlayer.sqrMagnitude <= 0.0001f)
            {
                toPlayer = Vector3.back;
            }

            closeThreat.name = CloseThreatRootName;
            closeThreat.transform.SetPositionAndRotation(position, Quaternion.LookRotation(toPlayer.normalized, Vector3.up));
            closeThreat.transform.localScale = Vector3.one;
            closeThreat.SetActive(true);

            BasicSoldierEnemy soldier = RequireComponent<BasicSoldierEnemy>(closeThreat, "close threat soldier");
            CombatHealth closeThreatHealth = RequireComponent<CombatHealth>(closeThreat, "close threat health");
            CombatTargetSensor targetSensor = RequireComponent<CombatTargetSensor>(closeThreat, "close threat target sensor");
            EnemyActionCameraCueDriver cameraCueDriver =
                RequireComponent<EnemyActionCameraCueDriver>(closeThreat, "close threat camera cue driver");

            SetObjectReference(targetSensor, "selfHealth", closeThreatHealth);
            SetObjectReferenceArray(targetSensor, "targetCandidates", new UnityEngine.Object[] { playerHealth });
            SetObjectReference(soldier, "targetSensor", targetSensor);
            SetObjectReference(soldier, "target", null);
            SetObjectReference(soldier, "targetHealth", null);
            SetObjectReference(soldier, "selfHealth", closeThreatHealth);
            SetObjectReference(cameraCueDriver, "agentSource", soldier);
            SetObjectReference(cameraCueDriver, "cameraController", cameraController);
            SetObjectReference(cameraCueDriver, "cueSpace", closeThreat.transform);
            SetFloat(closeThreatHealth, "maxHealth", 72f);
            ConfigureCloseThreatBodyHitbox(closeThreat);
            return closeThreat;
        }

        private static GameObject CreateCinematicSupportDragon(Scene scene, SummonLaneSpace laneSpace)
        {
            GameObject prefab = LoadAsset<GameObject>(CinematicSupportDragonSourcePrefabPath);
            GameObject dragon = PrefabUtility.InstantiatePrefab(prefab, scene) as GameObject;
            if (dragon == null)
            {
                throw new InvalidOperationException($"Could not instantiate cinematic support dragon prefab {CinematicSupportDragonSourcePrefabPath}.");
            }

            dragon.name = CinematicSupportDragonRootName;
            Vector3 position = laneSpace.GetBattlefieldWorldPoint(4.85f, laneSpace.SummonEntryZ + 0.85f, 2.08f);
            dragon.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, 236f, 0f));
            dragon.transform.localScale = Vector3.one * 0.18f;
            Animator animator = dragon.GetComponentInChildren<Animator>(includeInactive: true);
            if (animator == null)
            {
                throw new InvalidOperationException("Cinematic support dragon must expose an Animator.");
            }

            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            if (animator.runtimeAnimatorController != null)
            {
                animator.Play(CinematicSupportDragonAttackStateName, 0, 0.18f);
            }

            dragon.SetActive(false);
            EditorUtility.SetDirty(animator);
            EditorUtility.SetDirty(dragon);
            return dragon;
        }

        private static void ConfigureCloseThreatBodyHitbox(GameObject closeThreat)
        {
            closeThreat.SetActive(true);

            SphereCollider oldRootCollider = closeThreat.GetComponent<SphereCollider>();
            if (oldRootCollider != null
                && Mathf.Abs(oldRootCollider.radius - CloseThreatBodyHitboxRadius) < 0.001f
                && (oldRootCollider.center - CloseThreatBodyHitboxCenter).sqrMagnitude < 0.0001f)
            {
                UnityEngine.Object.DestroyImmediate(oldRootCollider, true);
            }

            Transform hitbox = closeThreat.transform.Find(CloseThreatBodyHitboxName);
            if (hitbox == null)
            {
                GameObject hitboxObject = new GameObject(CloseThreatBodyHitboxName);
                hitboxObject.transform.SetParent(closeThreat.transform, worldPositionStays: false);
                hitbox = hitboxObject.transform;
            }

            hitbox.gameObject.layer = closeThreat.layer;
            hitbox.localPosition = Vector3.zero;
            hitbox.localRotation = Quaternion.identity;
            hitbox.localScale = Vector3.one;

            SphereCollider bodyCollider = EnsureComponent<SphereCollider>(hitbox.gameObject);
            bodyCollider.isTrigger = false;
            bodyCollider.radius = CloseThreatBodyHitboxRadius;
            bodyCollider.center = CloseThreatBodyHitboxCenter;

            Rigidbody bodyRigidbody = EnsureComponent<Rigidbody>(closeThreat);
            bodyRigidbody.isKinematic = true;
            bodyRigidbody.useGravity = false;

            EditorUtility.SetDirty(hitbox.gameObject);
            EditorUtility.SetDirty(hitbox);
            EditorUtility.SetDirty(bodyCollider);
            EditorUtility.SetDirty(bodyRigidbody);
            EditorUtility.SetDirty(closeThreat);
        }

        private static void ConfigureBossProxyBodyHitbox(GameObject bossProxy)
        {
            SphereCollider bodyCollider = EnsureComponent<SphereCollider>(bossProxy);
            bodyCollider.isTrigger = false;
            bodyCollider.radius = BossProxyBodyHitboxRadius;
            bodyCollider.center = BossProxyBodyHitboxCenter;

            Rigidbody bodyRigidbody = EnsureComponent<Rigidbody>(bossProxy);
            bodyRigidbody.isKinematic = true;
            bodyRigidbody.useGravity = false;

            EditorUtility.SetDirty(bodyCollider);
            EditorUtility.SetDirty(bodyRigidbody);
        }

        private static void CreateBossProxyVisual(Transform parent)
        {
            RemoveBossProxyHumanoidVisualChildren(parent);
            CreateHumanoidBossProxyVisual(parent);
            CreateBossProjectileCore(parent);
        }

        private static void ReplaceBossProxyHumanoidVisual(GameObject bossProxy)
        {
            if (bossProxy == null)
            {
                throw new ArgumentNullException(nameof(bossProxy));
            }

            RemoveBossProxyHumanoidVisualChildren(bossProxy.transform);
            CreateHumanoidBossProxyVisual(bossProxy.transform);
        }

        private static void RemoveBossProxyHumanoidVisualChildren(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Transform child = parent.GetChild(i);
                if (child != null
                    && child.name.StartsWith(ReviewRootPrefix + "HumanoidBossVisual_", StringComparison.Ordinal))
                {
                    UnityEngine.Object.DestroyImmediate(child.gameObject);
                }
            }
        }

        private static void CreateHumanoidBossProxyVisual(Transform parent)
        {
            GameObject prefabAsset = LoadAsset<GameObject>(BossProxyHumanoidSourcePrefabPath);
            string prefabPath = AssetDatabase.GetAssetPath(prefabAsset).Replace('\\', '/');
            if (!string.Equals(prefabPath, BossProxyHumanoidSourcePrefabPath, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"SciFiSoldier01 Commando boss source should be {BossProxyHumanoidSourcePrefabPath}, found {prefabPath}.");
            }

            GameObject visual = PrefabUtility.InstantiatePrefab(prefabAsset, parent) as GameObject;
            if (visual == null)
            {
                throw new InvalidOperationException($"Failed to instantiate {BossProxyHumanoidSourcePrefabPath}.");
            }

            visual.name = BossProxyHumanoidVisualName;
            visual.transform.localPosition = new Vector3(0f, -1.58f, 0f);
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = new Vector3(1.22f, 1.22f, 1.22f);

            Animator animator = visual.GetComponent<Animator>();
            if (animator == null)
            {
                throw new InvalidOperationException($"{BossProxyHumanoidSourcePrefabPath} is missing its source Animator.");
            }

            animator.runtimeAnimatorController =
                LoadAsset<RuntimeAnimatorController>(ActionFoundationSciFiSoldier01VisualSetup.ControllerPath);
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            EditorUtility.SetDirty(animator);
        }

        private static void CreateBossProjectileCore(Transform parent)
        {
            Material material = LoadOrCreateMaterial(BossProxyVisualMaterialPath, new Color(1f, 0.55f, 0.05f, 1f));
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            visual.name = BossProxyMarkerName;
            visual.transform.SetParent(parent, worldPositionStays: false);
            visual.transform.localPosition = new Vector3(0f, 0.15f, -0.25f);
            visual.transform.localScale = new Vector3(0.46f, 0.46f, 0.46f);
            MeshRenderer renderer = visual.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.enabled = false;
        }

        private static Transform EnsureBossBasicFireOrigin(GameObject bossProxy)
        {
            Transform visual = bossProxy.transform.Find(BossProxyHumanoidVisualName);
            Transform weapon = visual != null
                ? FindDescendant(visual, BossProxyHumanoidSourceAssaultRifleName)
                : null;
            if (weapon != null)
            {
                Transform muzzle = EnsureChild(weapon, BossBasicFireMuzzleName);
                PositionBossBasicFireMuzzle(muzzle, weapon);
                return muzzle;
            }

            Transform fallback = bossProxy.transform.Find(BossProxyMarkerName);
            if (fallback != null)
            {
                return fallback;
            }

            fallback = EnsureChild(bossProxy.transform, BossBasicFireMuzzleName);
            fallback.localPosition = new Vector3(0f, 0.15f, -0.25f);
            fallback.localRotation = Quaternion.identity;
            fallback.localScale = Vector3.one;
            EditorUtility.SetDirty(fallback.gameObject);
            return fallback;
        }

        private static void PositionBossBasicFireMuzzle(Transform muzzle, Transform weapon)
        {
            if (TryCalculateWorldRenderBounds(weapon, out Bounds bounds))
            {
                Vector3 worldPosition = new Vector3(
                    bounds.center.x,
                    Mathf.Lerp(bounds.min.y, bounds.max.y, 0.55f),
                    bounds.min.z - 0.08f);
                muzzle.SetPositionAndRotation(worldPosition, Quaternion.LookRotation(Vector3.back, Vector3.up));
            }
            else
            {
                muzzle.localPosition = new Vector3(0f, 0f, -0.65f);
                muzzle.localRotation = Quaternion.identity;
            }

            muzzle.localScale = Vector3.one;
            EditorUtility.SetDirty(muzzle.gameObject);
        }

        private static bool TryCalculateWorldRenderBounds(Transform root, out Bounds bounds)
        {
            bounds = default;
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(includeInactive: true);
            bool hasBounds = false;
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || !renderer.enabled)
                {
                    continue;
                }

                if (!hasBounds)
                {
                    bounds = renderer.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            return hasBounds;
        }

        private static void ConfigureBossProxyVisualCueDriver(
            GameObject bossProxy,
            BossBarrageEmitter emitter,
            BossPressureActionDirector bossPressureActionDirector)
        {
            Transform visual = bossProxy.transform.Find(BossProxyHumanoidVisualName);
            if (visual == null)
            {
                throw new InvalidOperationException($"Boss proxy should include {BossProxyHumanoidVisualName} before cue binding.");
            }

            Animator animator = visual.GetComponent<Animator>();
            if (animator == null)
            {
                throw new InvalidOperationException($"{BossProxyHumanoidVisualName} is missing Animator for boss barrage cues.");
            }

            Transform projectileCore = bossProxy.transform.Find(BossProxyMarkerName);
            if (projectileCore == null)
            {
                throw new InvalidOperationException($"Boss proxy should include {BossProxyMarkerName} before cue binding.");
            }

            BossBarrageVisualCueDriver cueDriver = EnsureComponent<BossBarrageVisualCueDriver>(bossProxy);
            cueDriver.ConfigurePresentation(
                emitter,
                animator,
                projectileCore,
                projectileCore.GetComponentsInChildren<Renderer>(includeInactive: true));
            cueDriver.ConfigurePressureActionSource(bossPressureActionDirector);
            cueDriver.ResetToDefaultPatternCues();
            cueDriver.ResetToDefaultPressureActionCues();
            EditorUtility.SetDirty(cueDriver);
        }

        private static void ConfigureBossProxyWorldVfxCueDriver(
            GameObject bossProxy,
            CombatVfxCuePlayer cuePlayer,
            Transform directionTarget)
        {
            if (bossProxy == null)
            {
                throw new ArgumentNullException(nameof(bossProxy));
            }

            Transform projectileCore = bossProxy.transform.Find(BossProxyMarkerName);
            if (projectileCore == null)
            {
                throw new InvalidOperationException($"Boss proxy should include {BossProxyMarkerName} before world VFX cue binding.");
            }

            BossBarrageVisualCueDriver cueDriver = EnsureComponent<BossBarrageVisualCueDriver>(bossProxy);
            cueDriver.ConfigureWorldVfx(cuePlayer, projectileCore, directionTarget);
            EditorUtility.SetDirty(cueDriver);
        }

        private static void CreateLaneMarkers(Scene scene, SummonLaneSpace laneSpace)
        {
            GameObject markerRoot = CreateRoot(scene, MarkerRootName);
            markerRoot.SetActive(false);
            Material railMaterial = LoadOrCreateMaterial(LaneRailMaterialPath, new Color(0.15f, 0.72f, 1f, 1f));
            Material boundaryMaterial = LoadOrCreateMaterial(PlayerBoundaryMaterialPath, new Color(1f, 0.18f, 0.65f, 1f));
            Material summonMaterial = LoadOrCreateMaterial(SummonBoundaryMaterialPath, new Color(0.25f, 1f, 0.65f, 1f));

            float length = laneSpace.BossProxyZ - laneSpace.BackLimitZ;
            float centerZ = (laneSpace.BossProxyZ + laneSpace.BackLimitZ) * 0.5f;
            CreateMarker(
                markerRoot.transform,
                "Left_PlayerLaneRail",
                laneSpace.GetLaneWorldPoint(-laneSpace.HalfWidth, centerZ, 0.035f),
                new Vector3(0.08f, 0.05f, length),
                railMaterial);
            CreateMarker(
                markerRoot.transform,
                "Right_PlayerLaneRail",
                laneSpace.GetLaneWorldPoint(laneSpace.HalfWidth, centerZ, 0.035f),
                new Vector3(0.08f, 0.05f, length),
                railMaterial);
            CreateMarker(
                markerRoot.transform,
                "PlayerForwardBoundary_DoNotCross",
                laneSpace.GetLaneWorldPoint(0f, laneSpace.ForwardBoundaryZ, 0.06f),
                new Vector3(laneSpace.HalfWidth * 2f, 0.08f, 0.12f),
                boundaryMaterial);
            CreateMarker(
                markerRoot.transform,
                "SummonEntryLine_CanCross",
                laneSpace.GetLaneWorldPoint(0f, laneSpace.SummonEntryZ, 0.08f),
                new Vector3(laneSpace.HalfWidth * 2f, 0.08f, 0.12f),
                summonMaterial);
            CreateMarker(
                markerRoot.transform,
                SummonEntryMarkerName,
                laneSpace.GetLaneWorldPoint(0f, laneSpace.SummonEntryZ, 0.6f),
                new Vector3(0.7f, 1.2f, 0.7f),
                summonMaterial);
            CreateMarker(
                markerRoot.transform,
                "SummonOffLaneReach_CanCrossRail",
                laneSpace.GetBattlefieldWorldPoint(laneSpace.HalfWidth + 1.2f, laneSpace.SummonEntryZ, 0.45f),
                new Vector3(0.55f, 0.9f, 0.55f),
                summonMaterial);
        }

        private static void DeactivateArenaDressingVfx(Scene scene)
        {
            GameObject root = FindRoot(scene, ArenaVfxRootName);
            if (root == null)
            {
                return;
            }

            root.SetActive(false);
            EditorUtility.SetDirty(root);
        }

        private static void CreateBossBarrageTelegraphMarkers(
            Scene scene,
            SummonLaneSpace laneSpace,
            BossBarrageEmitter bossBarrageEmitter)
        {
            GameObject root = CreateRoot(scene, BossTelegraphRootName);
            root.SetActive(false);
            Material material = LoadOrCreateTransparentMaterial(
                BossTelegraphMaterialPath,
                new Color(1f, 0.62f, 0.18f, 0.56f));
            var markerTransforms = new Transform[9];
            var markerRenderers = new Renderer[markerTransforms.Length];
            for (int i = 0; i < markerTransforms.Length; i++)
            {
                float lateral01 = markerTransforms.Length <= 1 ? 0.5f : (float)i / (markerTransforms.Length - 1);
                float lateralX = Mathf.Lerp(-laneSpace.HalfWidth, laneSpace.HalfWidth, lateral01);
                GameObject marker = CreateMarker(
                    root.transform,
                    $"IncomingLaneTelegraph_{i:00}",
                    laneSpace.GetLaneWorldPoint(lateralX, laneSpace.ForwardBoundaryZ - 1.4f, 0.075f),
                    new Vector3(0.85f, 0.035f, 0.9f),
                    material);
                marker.SetActive(false);
                markerTransforms[i] = marker.transform;
                markerRenderers[i] = marker.GetComponent<MeshRenderer>();
            }

            BossBarrageLaneTelegraphPresenter presenter = root.AddComponent<BossBarrageLaneTelegraphPresenter>();
            presenter.Configure(bossBarrageEmitter, laneSpace, root.transform, markerTransforms, markerRenderers);
            EditorUtility.SetDirty(presenter);
        }

        private static void CreateLaneAmbientVfx(Scene scene, SummonLaneSpace laneSpace)
        {
            GameObject root = CreateRoot(scene, AmbientVfxRootName);
            root.SetActive(false);
            Material flowMaterial = LoadOrCreateTransparentMaterial(
                LaneAmbientFlowMaterialPath,
                new Color(0.22f, 0.88f, 1f, 0.28f));
            Material pressureMaterial = LoadOrCreateTransparentMaterial(
                BossPressureHorizonMaterialPath,
                new Color(1f, 0.28f, 0.16f, 0.34f));
            Material summonMaterial = LoadOrCreateTransparentMaterial(
                SummonRouteWispMaterialPath,
                new Color(0.24f, 1f, 0.58f, 0.42f));

            float backZ = laneSpace.BackLimitZ;
            float forwardZ = laneSpace.ForwardBoundaryZ;
            float bossZ = laneSpace.BossProxyZ;
            float width = laneSpace.HalfWidth * 2f;

            CreateAmbientPrimitive(
                root.transform,
                "AmbientFlow_LeftRail_00",
                PrimitiveType.Cube,
                laneSpace.GetLaneWorldPoint(-laneSpace.HalfWidth * 0.78f, Mathf.Lerp(backZ, forwardZ, 0.28f), 0.12f),
                new Vector3(0.045f, 0.035f, 4.9f),
                flowMaterial)
                .AddComponent<ActionFoundationArenaTransformMotion>()
                .Configure(Vector3.zero, Vector3.forward, 0.42f, 0.18f, 0.1f);
            CreateAmbientPrimitive(
                root.transform,
                "AmbientFlow_RightRail_00",
                PrimitiveType.Cube,
                laneSpace.GetLaneWorldPoint(laneSpace.HalfWidth * 0.78f, Mathf.Lerp(backZ, forwardZ, 0.58f), 0.12f),
                new Vector3(0.045f, 0.035f, 5.2f),
                flowMaterial)
                .AddComponent<ActionFoundationArenaTransformMotion>()
                .Configure(Vector3.zero, Vector3.forward, 0.38f, 0.2f, 0.5f);
            CreateAmbientPrimitive(
                root.transform,
                "AmbientFlow_CenterLane_00",
                PrimitiveType.Cube,
                laneSpace.GetLaneWorldPoint(0f, Mathf.Lerp(backZ, forwardZ, 0.44f), 0.105f),
                new Vector3(0.035f, 0.03f, 6.4f),
                flowMaterial)
                .AddComponent<ActionFoundationArenaTransformMotion>()
                .Configure(Vector3.zero, Vector3.forward, 0.32f, 0.16f, 0.8f);

            for (int i = 0; i < 5; i++)
            {
                float risk01 = i / 4f;
                float z = Mathf.Lerp(backZ, forwardZ, risk01);
                Material tickMaterial = risk01 > 0.66f ? pressureMaterial : flowMaterial;
                CreateAmbientPrimitive(
                    root.transform,
                    $"AmbientDepthTick_{i:00}",
                    PrimitiveType.Cube,
                    laneSpace.GetLaneWorldPoint(0f, z, 0.09f),
                    new Vector3(width * 0.92f, 0.02f, 0.035f),
                    tickMaterial);
            }

            CreateAmbientPrimitive(
                root.transform,
                "BossPressureHorizon_Curtain",
                PrimitiveType.Cube,
                laneSpace.GetLaneWorldPoint(0f, Mathf.Lerp(forwardZ, bossZ, 0.28f), 0.38f),
                new Vector3(width * 1.05f, 0.12f, 0.18f),
                pressureMaterial)
                .AddComponent<ActionFoundationArenaTransformMotion>()
                .Configure(Vector3.zero, Vector3.up, 0.06f, 0.28f, 0.2f);

            for (int i = 0; i < 4; i++)
            {
                float t = (i + 1f) / 5f;
                float side = i % 2 == 0 ? -1f : 1f;
                GameObject wisp = CreateAmbientPrimitive(
                    root.transform,
                    $"SummonRouteWisp_{i:00}",
                    PrimitiveType.Sphere,
                    laneSpace.GetLaneWorldPoint(side * laneSpace.HalfWidth * 0.32f, Mathf.Lerp(forwardZ, laneSpace.SummonEntryZ, t), 0.55f),
                    new Vector3(0.22f, 0.22f, 0.22f),
                    summonMaterial);
                wisp.AddComponent<ActionFoundationArenaFloatingShape>().Configure(
                    new Vector3(9f, 24f * side, 5f),
                    Vector3.up,
                    0.09f,
                    0.42f,
                    i * 0.21f,
                    new Color(0.24f, 1f, 0.58f, 0.56f),
                    new Color(0.12f, 1.2f, 0.62f, 1f),
                    0.28f,
                    0.7f);
            }

            EditorUtility.SetDirty(root);
        }

        private static GameObject CreateAmbientPrimitive(
            Transform parent,
            string name,
            PrimitiveType primitiveType,
            Vector3 position,
            Vector3 scale,
            Material material)
        {
            GameObject marker = GameObject.CreatePrimitive(primitiveType);
            marker.name = name;
            marker.transform.SetParent(parent, worldPositionStays: true);
            marker.transform.position = position;
            marker.transform.rotation = Quaternion.identity;
            marker.transform.localScale = scale;
            MeshRenderer renderer = EnsureComponent<MeshRenderer>(marker);
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.allowOcclusionWhenDynamic = false;
            Collider collider = marker.GetComponent<Collider>();
            if (collider != null)
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }

            EditorUtility.SetDirty(marker);
            return marker;
        }

        private static void CreateLaneAmbientAudio(Scene scene, SummonLaneSpace laneSpace)
        {
            RemoveLaneAmbientAudio(scene);
            GameObject root = CreateRoot(scene, AmbientAudioRootName);
            float backZ = laneSpace.BackLimitZ;
            float forwardZ = laneSpace.ForwardBoundaryZ;

            CreateAmbientAudioSource(
                root.transform,
                "AmbientAudio_ArenaStormBed",
                LoadAsset<AudioClip>(AmbientArenaStormClipPath),
                laneSpace.GetLaneWorldPoint(0f, Mathf.Lerp(backZ, forwardZ, 0.36f), 1.6f),
                0.055f,
                0f,
                8f,
                80f,
                0.98f);
            CreateAmbientAudioSource(
                root.transform,
                "AmbientAudio_ArenaEnergyWind",
                LoadAsset<AudioClip>(AmbientArenaEnergyWindClipPath),
                laneSpace.GetLaneWorldPoint(-laneSpace.HalfWidth * 0.18f, Mathf.Lerp(backZ, forwardZ, 0.48f), 1.25f),
                0.035f,
                0.2f,
                7f,
                72f,
                0.99f);
            CreateAmbientAudioSource(
                root.transform,
                "AmbientAudio_ArenaEnergyWave",
                LoadAsset<AudioClip>(AmbientArenaEnergyWaveClipPath),
                laneSpace.GetLaneWorldPoint(laneSpace.HalfWidth * 0.22f, Mathf.Lerp(backZ, forwardZ, 0.64f), 1.05f),
                0.032f,
                0.24f,
                7f,
                72f,
                1.01f);
            CreateAmbientAudioSource(
                root.transform,
                "AmbientAudio_LeftRailDustFlow",
                LoadAsset<AudioClip>(AmbientRailDustFlowClipPath),
                laneSpace.GetLaneWorldPoint(-laneSpace.HalfWidth * 0.72f, Mathf.Lerp(backZ, forwardZ, 0.42f), 0.25f),
                0.042f,
                0.55f,
                4f,
                36f,
                0.97f);
            CreateAmbientAudioSource(
                root.transform,
                "AmbientAudio_RightRailDustFlow",
                LoadAsset<AudioClip>(AmbientRailDustFlowClipPath),
                laneSpace.GetLaneWorldPoint(laneSpace.HalfWidth * 0.72f, Mathf.Lerp(backZ, forwardZ, 0.58f), 0.25f),
                0.042f,
                0.55f,
                4f,
                36f,
                1.03f);

            EditorUtility.SetDirty(root);
        }

        private static void CreateAmbientAudioSource(
            Transform parent,
            string name,
            AudioClip clip,
            Vector3 position,
            float volume,
            float spatialBlend,
            float minDistance,
            float maxDistance,
            float pitch)
        {
            GameObject audioObject = new GameObject(name);
            audioObject.transform.SetParent(parent, worldPositionStays: true);
            audioObject.transform.position = position;
            AudioSource source = audioObject.AddComponent<AudioSource>();
            source.clip = clip;
            source.loop = true;
            source.playOnAwake = true;
            source.volume = volume;
            source.pitch = pitch;
            source.spatialBlend = spatialBlend;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = minDistance;
            source.maxDistance = maxDistance;
            source.dopplerLevel = 0f;
            source.priority = 210;
            EditorUtility.SetDirty(audioObject);
            EditorUtility.SetDirty(source);
        }

        private static void CreateReviewSceneBgmSlot(Scene scene)
        {
            GameObject root = FindRoot(scene, BgmAudioRootName);
            if (root == null)
            {
                root = CreateRoot(scene, BgmAudioRootName);
            }

            AudioSource source = EnsureComponent<AudioSource>(root);
            source.playOnAwake = true;
            source.loop = true;
            source.volume = Mathf.Clamp(source.volume <= 0f ? 0.42f : source.volume, 0f, 0.72f);
            source.pitch = 1f;
            source.spatialBlend = 0f;
            source.dopplerLevel = 0f;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = 1f;
            source.maxDistance = 500f;
            source.priority = 40;
            source.bypassEffects = true;
            source.bypassListenerEffects = true;
            source.bypassReverbZones = true;
            EditorUtility.SetDirty(root);
            EditorUtility.SetDirty(source);
        }

        private static void ConfigureBossBarrageLaneReviewFootstepAudio(Scene scene)
        {
            PlayerMovementController player = RequireObject<PlayerMovementController>(scene, "player movement");
            ConfigureFootstepAudio(
                player.gameObject,
                PlayerFootstepAudioName,
                PlayerFootstepClipPaths,
                player,
                0.3f,
                0.55f,
                1.25f,
                0.16f,
                0.32f,
                2f,
                26f,
                150,
                0.82f);

            ConfigureFootstepAudio(
                RequireRoot(scene, CloseThreatRootName),
                CloseThreatFootstepAudioName,
                ArmoredFootstepClipPaths,
                null,
                0.28f,
                0.35f,
                1.18f,
                0.14f,
                0.72f,
                2.4f,
                34f,
                155,
                0.74f);

            ConfigureFootstepAudio(
                RequireRoot(scene, BossProxyRootName),
                BossProxyFootstepAudioName,
                ArmoredFootstepClipPaths,
                null,
                0.2f,
                0.28f,
                1.55f,
                0.18f,
                0.82f,
                3f,
                44f,
                165,
                0.7f);

            ConfigurePrefabFootstepAudio(SummonSlot1ActorPrefabPath, HeavyFootstepClipPaths, 0.24f, 1.2f, 0.7f, 156, 0.58f);
            ConfigurePrefabFootstepAudio(SummonSlot2ActorPrefabPath, ArmoredFootstepClipPaths, 0.2f, 1.28f, 0.68f, 160, 0.54f);
            ConfigurePrefabFootstepAudio(SummonSlot3ActorPrefabPath, HeavyFootstepClipPaths, 0.3f, 1.35f, 0.76f, 152, 0.6f);
            ConfigurePrefabFootstepAudio(BossSummonPressureActorPrefabPath, HeavyFootstepClipPaths, 0.28f, 1.3f, 0.78f, 154, 0.62f);
        }

        private static void ConfigurePrefabFootstepAudio(
            string prefabPath,
            string[] clipPaths,
            float baseVolume,
            float metersPerStep,
            float spatialBlend,
            int priority,
            float playbackVolumeScale)
        {
            GameObject editableRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                ConfigureFootstepAudio(
                    editableRoot,
                    SummonActorFootstepAudioName,
                    clipPaths,
                    null,
                    baseVolume,
                    0.32f,
                    metersPerStep,
                    0.15f,
                    spatialBlend,
                    2.5f,
                    36f,
                    priority,
                    playbackVolumeScale);
                PrefabUtility.SaveAsPrefabAsset(editableRoot, prefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(editableRoot);
            }
        }

        private static void ConfigureFootstepAudio(
            GameObject root,
            string childName,
            string[] clipPaths,
            PlayerMovementController playerMovement,
            float baseVolume,
            float minimumSpeed,
            float metersPerStep,
            float minimumIntervalSeconds,
            float spatialBlend,
            float minDistance,
            float maxDistance,
            int priority,
            float playbackVolumeScale)
        {
            Transform child = EnsureChild(root.transform, childName);
            child.localPosition = Vector3.zero;
            child.localRotation = Quaternion.identity;
            child.localScale = Vector3.one;

            AudioSource source = EnsureComponent<AudioSource>(child.gameObject);
            source.clip = null;
            source.loop = false;
            source.playOnAwake = false;
            source.volume = baseVolume;
            source.pitch = 1f;
            source.spatialBlend = spatialBlend;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = minDistance;
            source.maxDistance = maxDistance;
            source.dopplerLevel = 0f;
            source.priority = priority;

            MovementFootstepAudioPresenter presenter = EnsureComponent<MovementFootstepAudioPresenter>(child.gameObject);
            presenter.Configure(
                source,
                root.transform,
                playerMovement,
                LoadFootstepClips(clipPaths),
                baseVolume,
                minimumSpeed,
                metersPerStep,
                minimumIntervalSeconds,
                0.96f,
                1.05f,
                0.84f,
                1.08f,
                playbackVolumeScale);

            EditorUtility.SetDirty(child.gameObject);
            EditorUtility.SetDirty(source);
            EditorUtility.SetDirty(presenter);
            EditorUtility.SetDirty(root);
        }

        private static AudioClip[] LoadFootstepClips(string[] clipPaths)
        {
            var clips = new AudioClip[clipPaths.Length];
            for (int i = 0; i < clipPaths.Length; i++)
            {
                clips[i] = LoadAsset<AudioClip>(clipPaths[i]);
            }

            return clips;
        }

        private static GameObject CreateMarker(
            Transform parent,
            string name,
            Vector3 position,
            Vector3 scale,
            Material material,
            bool removeCollider = false)
        {
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marker.name = name;
            marker.transform.SetParent(parent, worldPositionStays: true);
            marker.transform.position = position;
            marker.transform.rotation = Quaternion.identity;
            marker.transform.localScale = scale;
            marker.GetComponent<MeshRenderer>().sharedMaterial = material;
            if (removeCollider)
            {
                Collider collider = marker.GetComponent<Collider>();
                if (collider != null)
                {
                    UnityEngine.Object.DestroyImmediate(collider);
                }
            }

            return marker;
        }

        private static void ConfigureTargetReferences(
            PlayerCombatTargetSelector targetSelector,
            ActionCameraTargetBridge cameraTargetBridge,
            ActionCameraController cameraController,
            PlayerMovementController player,
            CombatHealth playerHealth,
            CombatHealth closeThreatHealth,
            CombatHealth bossHealth)
        {
            ActionFoundationProfileSetup.ConfigurePlayerTargetSelector(
                targetSelector,
                player.transform,
                playerHealth,
                cameraController.transform,
                new[] { closeThreatHealth, bossHealth });
            // Seed the boss-lane review radius so a rebuilt scene can see the far proxy.
            // Designers may tune these values in the scene; validation does not exact-lock them.
            SetFloat(targetSelector, "selectionRadius", 35f);
            SetFloat(targetSelector, "attackAimRadius", 9f);
            SetObjectReference(cameraTargetBridge, "cameraController", cameraController);
            SetObjectReference(cameraTargetBridge, "targetSelector", targetSelector);
            SetObjectReference(cameraTargetBridge, "followTarget", player.transform);
            SetObjectReference(cameraController, "target", player.transform);
            SetObjectReference(cameraController, "threat", bossHealth.transform);
        }

        private static void ConfigureEncounter(
            ActionFoundationTestEncounter encounter,
            CombatHealth playerHealth,
            CombatHealth enemyHealth)
        {
            SetObjectReference(encounter, "playerHealth", playerHealth);
            SetObjectReference(encounter, "enemyHealth", enemyHealth);
        }

        private static void ConfigureLocalDefenseProfile(
            PlayerActionController playerActionController,
            PlayerActionProfile localDefenseProfile)
        {
            SetObjectReference(playerActionController, "actionProfile", localDefenseProfile);
        }

        private static AnimatorController EnsureInoriRifleAnimatorController()
        {
            AnimatorController controller =
                AssetDatabase.LoadAssetAtPath<AnimatorController>(InoriRifleAnimatorControllerPath);
            if (controller != null && IsInoriRifleAnimatorControllerCurrent(controller))
            {
                return controller;
            }

            if (controller != null && !AssetDatabase.DeleteAsset(InoriRifleAnimatorControllerPath))
            {
                throw new InvalidOperationException($"Failed to replace {InoriRifleAnimatorControllerPath}.");
            }

            EnsureFolderForAsset(InoriRifleAnimatorControllerPath);
            controller = AnimatorController.CreateAnimatorControllerAtPath(InoriRifleAnimatorControllerPath);
            if (controller == null)
            {
                throw new InvalidOperationException($"Failed to create {InoriRifleAnimatorControllerPath}.");
            }

            AddTrigger(controller, "IDLE");
            AddTrigger(controller, "IDLE 0");
            AddTrigger(controller, "SHOOT");
            AddTrigger(controller, "AUTO SHOOT");
            AddTrigger(controller, "RELOAD");
            AddTrigger(controller, "JOG");
            AddTrigger(controller, "WALK");
            AddTrigger(controller, "RUN");
            AddTrigger(controller, "WALK F");
            AddTrigger(controller, "WALK FL");
            AddTrigger(controller, "WALK FR");
            AddTrigger(controller, "WALK B");
            AddTrigger(controller, "WALK BL");
            AddTrigger(controller, "WALK BR");
            AddTrigger(controller, "EVADE");

            AnimatorControllerLayer[] layers = controller.layers;
            AnimatorControllerLayer layer = layers[0];
            layer.iKPass = true;
            AnimatorStateMachine stateMachine = layer.stateMachine;
            stateMachine.name = "Inori Rifle";

            AnimatorState normalIdle = CreateState(
                stateMachine,
                "R_Idle",
                "Assets/_Game/Art/Animations/Player/RifleGirl/RG_Idle.fbx",
                true,
                new Vector3(80f, 80f, 0f));
            AnimatorState normalWalk = CreateState(
                stateMachine,
                "R_Walk",
                "Assets/_Game/Art/Animations/Player/RifleGirl/RG_Walk.fbx",
                true,
                new Vector3(80f, 160f, 0f));
            AnimatorState normalRun = CreateState(
                stateMachine,
                "R_Run",
                "Assets/_Game/Art/Animations/Player/RifleGirl/RG_Run.fbx",
                true,
                new Vector3(80f, 240f, 0f));
            AnimatorState aimIdle = CreateState(
                stateMachine,
                "R_AimIdle",
                "Assets/_Game/Art/Animations/Player/RifleGirl/RG_AimIdle.fbx",
                true,
                new Vector3(360f, 80f, 0f));
            AnimatorState shoot = CreateState(
                stateMachine,
                "R_Shoot",
                "Assets/_Game/Art/Animations/Player/RifleGirl/RG_Shoot.fbx",
                false,
                new Vector3(640f, 80f, 0f));
            AnimatorState autoShoot = CreateState(
                stateMachine,
                "R_AimIdleAutoShoot",
                "Assets/_Game/Art/Animations/Player/RifleGirl/RG_AimIdleAutoShoot.fbx",
                true,
                new Vector3(640f, 160f, 0f));
            AnimatorState reload = CreateState(
                stateMachine,
                "R_Reload",
                "Assets/_Game/Art/Animations/Player/RifleGirl/RG_Reload.fbx",
                false,
                new Vector3(640f, 240f, 0f));
            AnimatorState aimJog = CreateState(
                stateMachine,
                "R_AimJog",
                "Assets/_Game/Art/Animations/Player/RifleGirl/RG_AimJog.fbx",
                true,
                new Vector3(360f, 240f, 0f));
            AnimatorState walkForward = CreateState(
                stateMachine,
                "R_AimWalkForward",
                "Assets/_Game/Art/Animations/Player/RifleGirl/RG_AimWalkForward.fbx",
                true,
                new Vector3(360f, 320f, 0f));
            AnimatorState walkBack = CreateState(
                stateMachine,
                "R_AimWalkBack",
                "Assets/_Game/Art/Animations/Player/RifleGirl/RG_AimWalkBack.fbx",
                true,
                new Vector3(160f, 320f, 0f));
            AnimatorState walkForwardLeft = CreateState(
                stateMachine,
                "R_AimWalkForwardLeft",
                "Assets/_Game/Art/Animations/Player/RifleGirl/RG_AimWalkForwardLeft.fbx",
                true,
                new Vector3(160f, 400f, 0f));
            AnimatorState walkForwardRight = CreateState(
                stateMachine,
                "R_AimWalkForwardRight",
                "Assets/_Game/Art/Animations/Player/RifleGirl/RG_AimWalkForwardRight.fbx",
                true,
                new Vector3(560f, 400f, 0f));
            AnimatorState walkBackLeft = CreateState(
                stateMachine,
                "R_AimWalkBackLeft",
                "Assets/_Game/Art/Animations/Player/RifleGirl/RG_AimWalkBackLeft.fbx",
                true,
                new Vector3(160f, 480f, 0f));
            AnimatorState walkBackRight = CreateState(
                stateMachine,
                "R_AimWalkBackRight",
                "Assets/_Game/Art/Animations/Player/RifleGirl/RG_AimWalkBackRight.fbx",
                true,
                new Vector3(560f, 480f, 0f));
            AnimatorState evade = CreateState(
                stateMachine,
                "R_Evade",
                "Assets/_Game/Art/Animations/Player/RifleGirl/RG_Evade.fbx",
                false,
                new Vector3(640f, 320f, 0f));

            stateMachine.defaultState = normalIdle;
            AddAnyTriggerTransition(stateMachine, "IDLE", normalIdle);
            AddAnyTriggerTransition(stateMachine, "IDLE 0", aimIdle);
            AddAnyTriggerTransition(stateMachine, "SHOOT", shoot);
            AddAnyTriggerTransition(stateMachine, "AUTO SHOOT", autoShoot);
            AddAnyTriggerTransition(stateMachine, "RELOAD", reload, 0f, true);
            AddAnyTriggerTransition(stateMachine, "JOG", aimJog);
            AddAnyTriggerTransition(stateMachine, "WALK", normalWalk);
            AddAnyTriggerTransition(stateMachine, "RUN", normalRun);
            AddAnyTriggerTransition(stateMachine, "WALK F", walkForward);
            AddAnyTriggerTransition(stateMachine, "WALK FL", walkForwardLeft);
            AddAnyTriggerTransition(stateMachine, "WALK FR", walkForwardRight);
            AddAnyTriggerTransition(stateMachine, "WALK B", walkBack);
            AddAnyTriggerTransition(stateMachine, "WALK BL", walkBackLeft);
            AddAnyTriggerTransition(stateMachine, "WALK BR", walkBackRight);
            AddAnyTriggerTransition(stateMachine, "EVADE", evade);
            AddReturnTransition(shoot, aimIdle);
            AddReturnTransition(evade, normalIdle);

            layers[0] = layer;
            controller.layers = layers;
            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            return controller;
        }

        private static bool IsInoriRifleAnimatorControllerCurrent(AnimatorController controller)
        {
            if (controller.layers.Length == 0 || !controller.layers[0].iKPass)
            {
                return false;
            }

            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
            return HasTrigger(controller, "IDLE")
                && HasTrigger(controller, "IDLE 0")
                && HasTrigger(controller, "SHOOT")
                && HasTrigger(controller, "AUTO SHOOT")
                && HasTrigger(controller, "RELOAD")
                && HasTrigger(controller, "JOG")
                && HasTrigger(controller, "WALK")
                && HasTrigger(controller, "RUN")
                && HasTrigger(controller, "WALK F")
                && HasTrigger(controller, "WALK FL")
                && HasTrigger(controller, "WALK FR")
                && HasTrigger(controller, "WALK B")
                && HasTrigger(controller, "WALK BL")
                && HasTrigger(controller, "WALK BR")
                && HasTrigger(controller, "EVADE")
                && stateMachine.defaultState != null
                && string.Equals(stateMachine.defaultState.name, "R_Idle", StringComparison.Ordinal)
                && HasState(stateMachine, "R_Idle")
                && HasState(stateMachine, "R_Walk")
                && HasState(stateMachine, "R_Run")
                && HasState(stateMachine, "R_AimIdle")
                && HasState(stateMachine, "R_Shoot")
                && HasState(stateMachine, "R_AimIdleAutoShoot")
                && HasState(stateMachine, "R_Reload")
                && HasState(stateMachine, "R_AimJog")
                && HasState(stateMachine, "R_AimWalkForward")
                && HasState(stateMachine, "R_AimWalkBack")
                && HasState(stateMachine, "R_AimWalkForwardLeft")
                && HasState(stateMachine, "R_AimWalkForwardRight")
                && HasState(stateMachine, "R_AimWalkBackLeft")
                && HasState(stateMachine, "R_AimWalkBackRight")
                && HasState(stateMachine, "R_Evade")
                && HasAnyStateTriggerTransition(stateMachine, "RELOAD", "R_Reload");
        }

        private static bool HasTrigger(AnimatorController controller, string parameterName)
        {
            AnimatorControllerParameter[] parameters = controller.parameters;
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].type == AnimatorControllerParameterType.Trigger
                    && string.Equals(parameters[i].name, parameterName, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasState(AnimatorStateMachine stateMachine, string stateName)
        {
            ChildAnimatorState[] states = stateMachine.states;
            for (int i = 0; i < states.Length; i++)
            {
                if (string.Equals(states[i].state.name, stateName, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            ChildAnimatorStateMachine[] childMachines = stateMachine.stateMachines;
            for (int i = 0; i < childMachines.Length; i++)
            {
                if (HasState(childMachines[i].stateMachine, stateName))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasAnyStateTriggerTransition(
            AnimatorStateMachine stateMachine,
            string trigger,
            string destinationStateName)
        {
            AnimatorStateTransition[] transitions = stateMachine.anyStateTransitions;
            for (int i = 0; i < transitions.Length; i++)
            {
                AnimatorStateTransition transition = transitions[i];
                if (transition.destinationState == null
                    || !string.Equals(transition.destinationState.name, destinationStateName, StringComparison.Ordinal))
                {
                    continue;
                }

                AnimatorCondition[] conditions = transition.conditions;
                for (int j = 0; j < conditions.Length; j++)
                {
                    if (conditions[j].mode == AnimatorConditionMode.If
                        && string.Equals(conditions[j].parameter, trigger, StringComparison.Ordinal))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static void AddTrigger(AnimatorController controller, string parameterName)
        {
            controller.AddParameter(parameterName, AnimatorControllerParameterType.Trigger);
        }

        private static AnimatorState CreateState(
            AnimatorStateMachine stateMachine,
            string stateName,
            string clipPath,
            bool loopTime,
            Vector3 position)
        {
            AnimatorState state = stateMachine.AddState(stateName, position);
            AnimationClip clip = LoadAsset<AnimationClip>(clipPath);
            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            if (settings.loopTime != loopTime)
            {
                settings.loopTime = loopTime;
                AnimationUtility.SetAnimationClipSettings(clip, settings);
                EditorUtility.SetDirty(clip);
            }

            state.motion = clip;
            state.writeDefaultValues = true;
            EditorUtility.SetDirty(state);
            return state;
        }

        private static void AddAnyTriggerTransition(
            AnimatorStateMachine stateMachine,
            string trigger,
            AnimatorState destination,
            float duration = 0.04f,
            bool canTransitionToSelf = false)
        {
            AnimatorStateTransition transition = stateMachine.AddAnyStateTransition(destination);
            transition.hasExitTime = false;
            transition.duration = duration;
            transition.canTransitionToSelf = canTransitionToSelf;
            transition.AddCondition(AnimatorConditionMode.If, 0f, trigger);
            EditorUtility.SetDirty(transition);
        }

        private static void AddReturnTransition(AnimatorState source, AnimatorState destination)
        {
            AnimatorStateTransition transition = source.AddTransition(destination);
            transition.hasExitTime = true;
            transition.exitTime = 0.96f;
            transition.duration = 0.06f;
            transition.canTransitionToSelf = false;
            EditorUtility.SetDirty(transition);
        }

        private static PlayerCombatModeVisualBinding CreatePlayerCombatModeVisuals(Scene scene, GameObject player)
        {
            ActionFoundationInoriPlayerVisualAssetSetup.EnsureInoriPlayerVisualAssets();
            Transform playerTransform = player.transform;
            DestroyChildIfPresent(playerTransform, RangedPlayerVisualRootName);
            DestroyChildIfPresent(playerTransform, RetiredRifleGirlRangedPlayerVisualRootName);

            GameObject rangedRoot = new GameObject(RangedPlayerVisualRootName);
            rangedRoot.transform.SetParent(playerTransform, worldPositionStays: false);
            rangedRoot.transform.localPosition = Vector3.zero;
            rangedRoot.transform.localRotation = Quaternion.identity;
            rangedRoot.transform.localScale = Vector3.one;

            GameObject modelAsset = LoadAsset<GameObject>(InoriSourcePrefabPath);
            GameObject modelInstance = PrefabUtility.InstantiatePrefab(modelAsset, scene) as GameObject;
            if (modelInstance == null)
            {
                throw new InvalidOperationException("Failed to instantiate Inori source prefab for ranged combat mode.");
            }

            PrefabUtility.UnpackPrefabInstance(
                modelInstance,
                PrefabUnpackMode.Completely,
                InteractionMode.AutomatedAction);
            modelInstance.name = RangedPlayerModelName;
            modelInstance.transform.SetParent(rangedRoot.transform, worldPositionStays: false);
            modelInstance.transform.localPosition = Vector3.zero;
            modelInstance.transform.localRotation = Quaternion.identity;
            modelInstance.transform.localScale = Vector3.one;
            StripNonGameMonoBehaviours(modelInstance);
            RemapInoriPlayerMeshes(modelInstance);
            AssignInoriPlayerMaterials(modelInstance);

            Animator rangedAnimator = modelInstance.GetComponentInChildren<Animator>(includeInactive: true)
                ?? modelInstance.AddComponent<Animator>();
            rangedAnimator.runtimeAnimatorController = EnsureInoriRifleAnimatorController();
            rangedAnimator.avatar = ActionFoundationInoriPlayerVisualAssetSetup.LoadPromotedAvatar();
            rangedAnimator.applyRootMotion = false;
            rangedAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            RifleGirlNativeGameplayAnimatorBridge nativeBridge =
                rangedAnimator.gameObject.GetComponent<RifleGirlNativeGameplayAnimatorBridge>()
                ?? rangedAnimator.gameObject.AddComponent<RifleGirlNativeGameplayAnimatorBridge>();

            GameObject weaponInstance = CreateInoriRangedWeapon(scene, modelInstance.transform);
            Transform muzzle = FindOrCreateRifleMuzzle(weaponInstance.transform);
            RifleGirlWeaponSocketDriver weaponSocketDriver =
                ConfigureInoriRangedWeaponSocketDriver(rangedAnimator.gameObject, rangedAnimator, weaponInstance);

            GameObject meleeSourceRoot = FindPlayerMeleeVisualRoot(playerTransform);
            if (meleeSourceRoot == null)
            {
                throw new InvalidOperationException("CombatGirl melee source visual root is required for sword/shield extraction.");
            }

            Animator meleeSourceAnimator = meleeSourceRoot.GetComponentInChildren<Animator>(includeInactive: true);
            if (meleeSourceAnimator == null)
            {
                throw new InvalidOperationException("CombatGirl melee source visual root must keep its Animator.");
            }

            meleeSourceAnimator.runtimeAnimatorController = LoadAsset<RuntimeAnimatorController>(CombatGirlAnimatorControllerPath);
            meleeSourceAnimator.applyRootMotion = false;
            meleeSourceAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            GameObject meleeWeaponRoot = CreateMeleeWeaponRoot(
                scene,
                rangedRoot.transform,
                modelInstance.transform,
                meleeSourceRoot);
            if (meleeWeaponRoot == null)
            {
                throw new InvalidOperationException("Inori single-character combat mode requires extracted melee weapons.");
            }

            rangedRoot.SetActive(true);
            meleeSourceRoot.SetActive(false);
            meleeWeaponRoot.SetActive(false);

            EditorUtility.SetDirty(rangedRoot);
            EditorUtility.SetDirty(modelInstance);
            EditorUtility.SetDirty(weaponInstance);
            EditorUtility.SetDirty(weaponSocketDriver);
            EditorUtility.SetDirty(meleeSourceRoot);
            EditorUtility.SetDirty(meleeSourceAnimator);
            EditorUtility.SetDirty(meleeWeaponRoot);

            return new PlayerCombatModeVisualBinding(
                rangedRoot,
                meleeSourceRoot,
                weaponInstance,
                muzzle,
                meleeWeaponRoot,
                nativeBridge,
                rangedAnimator,
                rangedAnimator);
        }

        private static void ConfigurePlayerDamageShaderFeedback(
            GameObject player,
            CombatHealth playerHealth,
            PlayerCombatModeVisualBinding combatModeVisuals)
        {
            CombatHitFeedback feedback = EnsureComponent<CombatHitFeedback>(player);
            Renderer[] renderers = CollectPlayerDamageFeedbackRenderers(combatModeVisuals);
            if (renderers.Length == 0)
            {
                throw new InvalidOperationException("Player damage shader feedback needs at least one promoted player renderer.");
            }

            SetObjectReference(feedback, "health", playerHealth);
            SetObjectReferenceArray(feedback, "flashRenderers", ToObjectArray(renderers));
            SetBool(feedback, "renderHitFeedback", true);
            SetBool(feedback, "applyIdleColorOnEnable", false);
            SetFloat(feedback, "flashSeconds", 0.12f);
            SetColor(feedback, "hitColor", new Color(1f, 0.46f, 0.38f, 1f));
            SetColor(feedback, "deathColor", new Color(0.12f, 0.02f, 0.025f, 1f));
            EditorUtility.SetDirty(player);
            EditorUtility.SetDirty(feedback);
        }

        private static Renderer[] CollectPlayerDamageFeedbackRenderers(PlayerCombatModeVisualBinding combatModeVisuals)
        {
            var renderers = new List<Renderer>();
            AddEnabledRenderers(renderers, combatModeVisuals.RangedRoot);
            AddEnabledRenderers(renderers, combatModeVisuals.RangedWeaponRoot);
            AddEnabledRenderers(renderers, combatModeVisuals.MeleeRoot);
            AddEnabledRenderers(renderers, combatModeVisuals.MeleeWeaponRoot);
            return renderers.ToArray();
        }

        private static void AddEnabledRenderers(List<Renderer> renderers, GameObject root)
        {
            if (renderers == null || root == null)
            {
                return;
            }

            Renderer[] found = CollectEnabledRenderers(root);
            for (int i = 0; i < found.Length; i++)
            {
                if (found[i] != null && !renderers.Contains(found[i]))
                {
                    renderers.Add(found[i]);
                }
            }
        }

        private static UnityEngine.Object[] ToObjectArray(Renderer[] renderers)
        {
            if (renderers == null || renderers.Length == 0)
            {
                return Array.Empty<UnityEngine.Object>();
            }

            var objects = new UnityEngine.Object[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                objects[i] = renderers[i];
            }

            return objects;
        }

        private static GameObject CreateInoriRangedWeapon(Scene scene, Transform inoriModelRoot)
        {
            GameObject sourceAsset = LoadAsset<GameObject>(RifleGirlSourcePrefabPath);
            GameObject sourceInstance = PrefabUtility.InstantiatePrefab(sourceAsset, scene) as GameObject;
            if (sourceInstance == null)
            {
                throw new InvalidOperationException("Failed to instantiate RifleGirl source prefab for Inori rifle extraction.");
            }

            PrefabUtility.UnpackPrefabInstance(
                sourceInstance,
                PrefabUnpackMode.Completely,
                InteractionMode.AutomatedAction);
            StripNonGameMonoBehaviours(sourceInstance);
            RemapRangedCandidateMeshes(sourceInstance);
            AssignRangedCandidateMaterials(sourceInstance);

            InoriRiflePoseTuningProfile tuningProfile = LoadInoriRiflePoseTuningProfile();
            Transform sourceWeapon = FindRifleConstraintWeaponRoot(sourceInstance.transform)?.transform;
            Transform sourceRightHand = FindLikelyRightHandSocket(sourceInstance.transform);
            Transform targetRightHand = FindLikelyRightHandSocket(inoriModelRoot);
            if (sourceWeapon == null || sourceRightHand == null || targetRightHand == null)
            {
                UnityEngine.Object.DestroyImmediate(sourceInstance);
                throw new InvalidOperationException("Inori rifle extraction requires a RifleGirl source weapon/right hand and Inori right hand.");
            }

            GameObject socketObject = new GameObject("Inori_RifleSocket_Adjust");
            socketObject.transform.SetParent(targetRightHand, worldPositionStays: false);
            ApplyRetargetedRifleSocket(
                sourceWeapon,
                sourceRightHand,
                targetRightHand,
                socketObject.transform,
                tuningProfile);
            socketObject.transform.localScale = Vector3.one;

            GameObject weaponClone = UnityEngine.Object.Instantiate(sourceWeapon.gameObject);
            weaponClone.name = RangedPlayerWeaponName;
            weaponClone.transform.SetParent(socketObject.transform, worldPositionStays: false);
            weaponClone.transform.localPosition = Vector3.zero;
            weaponClone.transform.localRotation = Quaternion.identity;
            weaponClone.transform.localScale = sourceWeapon.localScale;
            ApplyInoriRetargetedRifleMeshCorrection(weaponClone.transform, tuningProfile);

            ParentConstraint[] constraints = weaponClone.GetComponentsInChildren<ParentConstraint>(includeInactive: true);
            for (int i = constraints.Length - 1; i >= 0; i--)
            {
                UnityEngine.Object.DestroyImmediate(constraints[i]);
            }

            UnityEngine.Object.DestroyImmediate(sourceInstance);
            EditorUtility.SetDirty(socketObject);
            EditorUtility.SetDirty(weaponClone);
            return weaponClone;
        }

        private static void ApplyInoriRetargetedRifleMeshCorrection(
            Transform weaponRoot,
            InoriRiflePoseTuningProfile tuningProfile)
        {
            Transform rifleMesh = FindDescendant(weaponRoot, tuningProfile.RifleMeshName);
            if (rifleMesh == null)
            {
                throw new InvalidOperationException($"{weaponRoot.name} is missing {tuningProfile.RifleMeshName}.");
            }

            rifleMesh.localPosition = tuningProfile.RifleMeshLocalPosition;
            rifleMesh.localRotation = tuningProfile.RifleMeshLocalRotation;
            rifleMesh.localScale = Vector3.one;
            EditorUtility.SetDirty(rifleMesh);
        }

        private static void ApplyRetargetedRifleSocket(
            Transform sourceWeapon,
            Transform sourceHand,
            Transform targetHand,
            Transform targetSocket,
            InoriRiflePoseTuningProfile tuningProfile)
        {
            Quaternion handAxisCorrection = Quaternion.Inverse(targetHand.rotation) * sourceHand.rotation;
            Vector3 sourceLocalPosition = sourceHand.InverseTransformPoint(sourceWeapon.position);
            Quaternion sourceLocalRotation = Quaternion.Inverse(sourceHand.rotation) * sourceWeapon.rotation;
            targetSocket.localPosition = (handAxisCorrection * sourceLocalPosition)
                + tuningProfile.RightGripLocalPosition;
            targetSocket.localRotation = (handAxisCorrection * sourceLocalRotation)
                * tuningProfile.RightGripLocalRotation;
        }

        private static GameObject CreateMeleeWeaponRoot(
            Scene scene,
            Transform parent,
            Transform rangedModelRoot,
            GameObject meleeRoot)
        {
            DestroyChildIfPresent(parent, MeleePlayerWeaponRootName);

            if (meleeRoot == null)
            {
                return null;
            }

            Transform rightHand = FindLikelyRightHandSocket(rangedModelRoot);
            Transform leftHand = FindLikelyLeftHandSocket(rangedModelRoot);
            if (rightHand == null || leftHand == null)
            {
                throw new InvalidOperationException("Ranged player model must expose both hand sockets before melee weapons can be attached.");
            }

            Transform sourceRightHand = FindLikelyRightHandSocket(meleeRoot.transform);
            Transform sourceLeftHand = FindLikelyLeftHandSocket(meleeRoot.transform);
            if (sourceRightHand == null || sourceLeftHand == null)
            {
                throw new InvalidOperationException("CombatGirl source visual must expose both hand sockets for preserving sword/shield offsets.");
            }

            Transform sourceRightWeapon = FindDescendant(meleeRoot.transform, "add_weapon_r");
            Transform sourceLeftWeapon = FindDescendant(meleeRoot.transform, "add_weapon_l");
            if (sourceRightWeapon == null || sourceLeftWeapon == null)
            {
                throw new InvalidOperationException("CombatGirl melee visual must expose add_weapon_r and add_weapon_l weapon objects.");
            }

            GameObject root = new GameObject(MeleePlayerWeaponRootName);
            root.transform.SetParent(parent, worldPositionStays: false);
            root.transform.localPosition = Vector3.zero;
            root.transform.localRotation = Quaternion.identity;
            root.transform.localScale = Vector3.one;

            Transform rightWeapon = CreateMeleeWeaponAnchor(
                sourceRightWeapon,
                sourceRightHand,
                rightHand,
                "MeleeWeapon_RightHand");
            Transform leftWeapon = CreateMeleeWeaponAnchor(
                sourceLeftWeapon,
                sourceLeftHand,
                leftHand,
                "MeleeWeapon_LeftHand");
            CombatGirlWeaponSocketBinder binder = root.AddComponent<CombatGirlWeaponSocketBinder>();
            binder.ConfigureWeaponSockets(leftHand, leftWeapon, rightHand, rightWeapon);
            binder.ApplyBindings();
            EditorUtility.SetDirty(binder);
            return root;

            Transform CreateMeleeWeaponAnchor(
                Transform sourceWeapon,
                Transform sourceHand,
                Transform targetHand,
                string cloneName)
            {
                GameObject clone = UnityEngine.Object.Instantiate(sourceWeapon.gameObject, root.transform);
                clone.name = cloneName;
                ApplyRetargetedHandAttachment(sourceWeapon, sourceHand, targetHand, clone.transform);
                clone.transform.localScale = sourceWeapon.localScale;
                EditorUtility.SetDirty(clone);
                return clone.transform;
            }
        }

        private static void ApplyRetargetedHandAttachment(
            Transform sourceWeapon,
            Transform sourceHand,
            Transform targetHand,
            Transform targetWeapon)
        {
            Quaternion handAxisCorrection = Quaternion.Inverse(targetHand.rotation) * sourceHand.rotation;
            Vector3 sourceLocalPosition = sourceHand.InverseTransformPoint(sourceWeapon.position);
            Quaternion sourceLocalRotation = Quaternion.Inverse(sourceHand.rotation) * sourceWeapon.rotation;
            Vector3 correctedLocalPosition = handAxisCorrection * sourceLocalPosition;
            Quaternion correctedLocalRotation = handAxisCorrection * sourceLocalRotation;

            targetWeapon.SetPositionAndRotation(
                targetHand.TransformPoint(correctedLocalPosition),
                targetHand.rotation * correctedLocalRotation);
        }

        private static GameObject FindRifleConstraintWeaponRoot(Transform modelRoot)
        {
            ParentConstraint[] constraints = modelRoot.GetComponentsInChildren<ParentConstraint>(includeInactive: true);
            for (int i = 0; i < constraints.Length; i++)
            {
                if (constraints[i] != null
                    && constraints[i].name.Contains("Weapon_Rifle", StringComparison.Ordinal))
                {
                    return constraints[i].gameObject;
                }
            }

            Transform[] candidates = modelRoot.GetComponentsInChildren<Transform>(includeInactive: true);
            for (int i = 0; i < candidates.Length; i++)
            {
                if (string.Equals(candidates[i].name, "Weapon_Rifle", StringComparison.Ordinal)
                    && candidates[i].GetComponent<ParentConstraint>() != null)
                {
                    return candidates[i].gameObject;
                }
            }

            return null;
        }

        private static RifleGirlWeaponSocketDriver ConfigureInoriRangedWeaponSocketDriver(
            GameObject modelInstance,
            Animator rangedAnimator,
            GameObject weaponInstance)
        {
            InoriRiflePoseTuningProfile tuningProfile = LoadInoriRiflePoseTuningProfile();
            Transform leftHandle = FindDescendant(weaponInstance.transform, "Left_Handle");
            if (leftHandle == null)
            {
                GameObject handle = new GameObject("Left_Handle");
                handle.transform.SetParent(weaponInstance.transform, worldPositionStays: false);
                handle.transform.localScale = Vector3.one;
                leftHandle = handle.transform;
                EditorUtility.SetDirty(handle);
            }

            leftHandle.localPosition = tuningProfile.LeftHandleLocalPosition;
            leftHandle.localRotation = tuningProfile.LeftHandleLocalRotation;
            leftHandle.localScale = Vector3.one;

            RifleGirlWeaponSocketDriver driver =
                modelInstance.GetComponent<RifleGirlWeaponSocketDriver>()
                ?? modelInstance.AddComponent<RifleGirlWeaponSocketDriver>();
            driver.Configure(rangedAnimator, null, leftHandle);
            SetObjectReference(driver, "animator", rangedAnimator);
            SetObjectReference(driver, "rifleConstraint", null);
            SetObjectReference(driver, "leftHandIkTarget", leftHandle);
            const string defaultCommands = "To_Hand_R_Socket, IK_OFF_Left_Handle";
            float leftIkPositionWeight = tuningProfile.EnabledForGameplay
                ? tuningProfile.LeftIkPositionWeight
                : 0f;
            float leftIkRotationWeight = tuningProfile.EnabledForGameplay
                ? tuningProfile.LeftIkRotationWeight
                : 0f;

            SetString(driver, "defaultCommands", defaultCommands);
            SetString(driver, "handSocketCommand", "To_Hand_R_Socket");
            SetString(driver, "holsterSocketCommand", "To_Put_Socket_Rifle");
            SetString(driver, "aimSocketCommand", "To_add_weapon_r");
            SetString(driver, "leftIkOnCommand", "IK_ON_Left_Handle");
            SetString(driver, "leftIkOffCommand", "IK_OFF_Left_Handle");
            SetBool(driver, "ignoreRedundantSocketCommands", true);
            SetFloat(driver, "leftIkMaxWeight", leftIkPositionWeight);
            SetFloat(driver, "leftIkRotationMaxWeight", leftIkRotationWeight);
            SetFloat(driver, "leftIkBlendSpeed", 15f);
            driver.SwitchSocketByString(defaultCommands);
            EditorUtility.SetDirty(driver);
            return driver;
        }

        private static Transform FindOrCreateRifleMuzzle(Transform weaponRoot)
        {
            Transform existing = weaponRoot.Find("Muzzle");
            Transform muzzle = existing;
            if (muzzle == null)
            {
                muzzle = new GameObject("Muzzle").transform;
                muzzle.SetParent(weaponRoot, worldPositionStays: false);
            }

            muzzle.localPosition = TryCalculateLocalRendererBounds(weaponRoot, out Bounds localBounds)
                ? new Vector3(localBounds.min.x, localBounds.center.y, localBounds.center.z)
                : InoriRifleMuzzleFallbackLocalPosition;
            muzzle.localRotation = Quaternion.LookRotation(Vector3.left, Vector3.up);
            muzzle.localScale = Vector3.one;
            EditorUtility.SetDirty(muzzle);
            return muzzle;
        }

        private static bool TryCalculateLocalRendererBounds(Transform root, out Bounds localBounds)
        {
            localBounds = default;
            bool hasBounds = false;
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(includeInactive: true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                Bounds worldBounds = renderer.bounds;
                Vector3 min = worldBounds.min;
                Vector3 max = worldBounds.max;
                Vector3[] corners =
                {
                    new Vector3(min.x, min.y, min.z),
                    new Vector3(min.x, min.y, max.z),
                    new Vector3(min.x, max.y, min.z),
                    new Vector3(min.x, max.y, max.z),
                    new Vector3(max.x, min.y, min.z),
                    new Vector3(max.x, min.y, max.z),
                    new Vector3(max.x, max.y, min.z),
                    new Vector3(max.x, max.y, max.z),
                };

                for (int cornerIndex = 0; cornerIndex < corners.Length; cornerIndex++)
                {
                    Vector3 localCorner = root.InverseTransformPoint(corners[cornerIndex]);
                    if (!hasBounds)
                    {
                        localBounds = new Bounds(localCorner, Vector3.zero);
                        hasBounds = true;
                    }
                    else
                    {
                        localBounds.Encapsulate(localCorner);
                    }
                }
            }

            return hasBounds;
        }

        private static RifleGirlWeaponSocketDriver ConfigureRifleGirlWeaponSocketDriver(
            GameObject modelInstance,
            Animator rangedAnimator,
            GameObject weaponInstance)
        {
            ParentConstraint weaponConstraint = weaponInstance.GetComponent<ParentConstraint>();
            if (weaponConstraint == null)
            {
                throw new InvalidOperationException("RifleGirl ranged weapon must keep its ParentConstraint.");
            }

            Transform leftHandle = FindDescendant(weaponInstance.transform, "Left_Handle");
            if (leftHandle == null)
            {
                throw new InvalidOperationException("RifleGirl ranged weapon must expose Left_Handle for support-hand IK.");
            }

            RifleGirlWeaponSocketDriver driver =
                modelInstance.GetComponent<RifleGirlWeaponSocketDriver>()
                ?? modelInstance.AddComponent<RifleGirlWeaponSocketDriver>();
            driver.Configure(rangedAnimator, weaponConstraint, leftHandle);
            SetObjectReference(driver, "animator", rangedAnimator);
            SetObjectReference(driver, "rifleConstraint", weaponConstraint);
            SetObjectReference(driver, "leftHandIkTarget", leftHandle);
            SetString(driver, "defaultCommands", "To_Hand_R_Socket, IK_ON_Left_Handle");
            SetString(driver, "handSocketCommand", "To_Hand_R_Socket");
            SetString(driver, "holsterSocketCommand", "To_Put_Socket_Rifle");
            SetString(driver, "aimSocketCommand", "To_add_weapon_r");
            SetString(driver, "leftIkOnCommand", "IK_ON_Left_Handle");
            SetString(driver, "leftIkOffCommand", "IK_OFF_Left_Handle");
            SetBool(driver, "ignoreRedundantSocketCommands", true);
            SetFloat(driver, "leftIkMaxWeight", 1f);
            SetFloat(driver, "leftIkRotationMaxWeight", 1f);
            SetFloat(driver, "leftIkBlendSpeed", 15f);
            driver.SwitchSocketByString("To_Hand_R_Socket, IK_ON_Left_Handle");
            EditorUtility.SetDirty(driver);
            return driver;
        }

        private static void ConfigureCombatModeController(
            GameObject player,
            PlayerActionController playerActionController,
            PlayerMovementController playerMovementController,
            PlayerActionProfile localDefenseProfile,
            PlayerCombatModeVisualBinding visualBinding)
        {
            PlayerCombatModeController combatModeController = EnsureComponent<PlayerCombatModeController>(player);
            SetObjectReference(combatModeController, "actionController", playerActionController);
            SetObjectReference(combatModeController, "movementController", playerMovementController);
            SetObjectReference(combatModeController, "rangedActionProfile", localDefenseProfile);
            SetObjectReference(combatModeController, "meleeActionProfile", LoadAsset<PlayerActionProfile>(MeleeActionProfilePath));
            SetObjectReference(combatModeController, "rangedVisualRoot", visualBinding.RangedRoot);
            SetObjectReference(combatModeController, "meleeVisualRoot", visualBinding.MeleeRoot);
            SetObjectReference(combatModeController, "rangedWeaponRoot", visualBinding.RangedWeaponRoot);
            SetObjectReference(combatModeController, "meleeWeaponRoot", visualBinding.MeleeWeaponRoot);
            SetObjectReference(combatModeController, "rangedAnimator", visualBinding.RangedAnimator);
            SetObjectReference(combatModeController, "meleeAnimator", visualBinding.MeleeAnimator);
            SetObjectReference(
                combatModeController,
                "rangedAnimatorController",
                EnsureInoriRifleAnimatorController());
            SetObjectReference(
                combatModeController,
                "meleeAnimatorController",
                LoadAsset<RuntimeAnimatorController>(CombatGirlAnimatorControllerPath));
            SetBool(combatModeController, "routeAnimatorsByMode", true);
            SetBool(combatModeController, "rangedAnimatorUsesExternalPresentationBridge", true);
            SetBool(combatModeController, "useSingleCharacterVisual", true);
            SetEnum(combatModeController, "startingMode", (int)PlayerCombatMode.Ranged);
            SetObjectReference(playerActionController, "combatModeController", combatModeController);
            SetObjectReference(playerActionController, "animator", null);
            SetObjectReference(playerMovementController, "animator", null);
            SetBool(playerActionController, "blockBasicAttackInRangedMode", true);
        }

        private static void RebindBossBarrageLaneReviewSingleCharacterMode(Scene scene)
        {
            PlayerCombatModeController combatModeController =
                RequireObject<PlayerCombatModeController>(scene, "player combat mode controller");
            GameObject player = combatModeController.gameObject;
            PlayerActionController playerActionController =
                RequireComponent<PlayerActionController>(player, "player action controller");
            PlayerMovementController playerMovementController =
                RequireComponent<PlayerMovementController>(player, "player movement controller");
            PlayerRangedAimController rangedAimController =
                RequireComponent<PlayerRangedAimController>(player, "player ranged aim controller");
            PlayerRangedBasicAttackAction rangedBasicAttackAction =
                RequireComponent<PlayerRangedBasicAttackAction>(player, "player ranged basic attack action");

            GameObject rangedRoot = RequireReferencedObject<GameObject>(combatModeController, "rangedVisualRoot");
            GameObject meleeSourceRoot = RequireReferencedObject<GameObject>(combatModeController, "meleeVisualRoot");
            GameObject rangedWeaponRoot = RequireReferencedObject<GameObject>(combatModeController, "rangedWeaponRoot");
            Animator rangedAnimator = RequireReferencedObject<Animator>(combatModeController, "rangedAnimator");
            rangedAnimator.runtimeAnimatorController = EnsureInoriRifleAnimatorController();
            GameObject meleeWeaponRoot = CreateMeleeWeaponRoot(
                scene,
                rangedRoot.transform,
                rangedAnimator.transform,
                meleeSourceRoot);
            if (meleeWeaponRoot == null)
            {
                throw new InvalidOperationException("Single-character combat mode requires extracted melee weapons.");
            }

            rangedRoot.SetActive(true);
            meleeSourceRoot.SetActive(false);
            rangedWeaponRoot.SetActive(true);
            meleeWeaponRoot.SetActive(false);

            SetObjectReference(combatModeController, "meleeWeaponRoot", meleeWeaponRoot);
            SetObjectReference(combatModeController, "meleeAnimator", rangedAnimator);
            SetObjectReference(
                combatModeController,
                "rangedAnimatorController",
                EnsureInoriRifleAnimatorController());
            SetObjectReference(
                combatModeController,
                "meleeAnimatorController",
                LoadAsset<RuntimeAnimatorController>(CombatGirlAnimatorControllerPath));
            SetBool(combatModeController, "routeAnimatorsByMode", true);
            SetBool(combatModeController, "rangedAnimatorUsesExternalPresentationBridge", true);
            SetBool(combatModeController, "useSingleCharacterVisual", true);
            SetObjectReference(playerActionController, "animator", null);
            SetObjectReference(playerMovementController, "animator", null);
            SetBool(playerActionController, "blockBasicAttackInRangedMode", true);

            ValidateCombatModeController(
                combatModeController,
                playerActionController,
                playerMovementController,
                rangedAimController,
                rangedBasicAttackAction);
            EditorUtility.SetDirty(combatModeController);
            EditorUtility.SetDirty(playerActionController);
            EditorUtility.SetDirty(playerMovementController);
            EditorUtility.SetDirty(rangedAnimator);
            EditorUtility.SetDirty(rangedRoot);
            EditorUtility.SetDirty(meleeSourceRoot);
            EditorUtility.SetDirty(rangedWeaponRoot);
            EditorUtility.SetDirty(meleeWeaponRoot);
            EditorSceneManager.MarkSceneDirty(scene);
        }

        private static void ConfigureCombatModeActionLinks(
            PlayerCombatModeController combatModeController,
            PlayerRangedAimController rangedAimController,
            PlayerRangedBasicAttackAction rangedBasicAttackAction)
        {
            SetObjectReference(combatModeController, "rangedAimController", rangedAimController);
            SetObjectReference(combatModeController, "rangedBasicAttackAction", rangedBasicAttackAction);
        }

        private static void ConfigureRifleGirlNativeBridge(
            RifleGirlNativeGameplayAnimatorBridge nativeBridge,
            Animator rangedAnimator,
            PlayerMovementController movement,
            PlayerActionController actionController,
            PlayerCombatModeController combatModeController,
            PlayerRangedAimController rangedAimController,
            PlayerRangedBasicAttackAction rangedBasicAttackAction)
        {
            nativeBridge.Configure(
                rangedAnimator,
                movement,
                actionController,
                combatModeController,
                rangedAimController,
                rangedBasicAttackAction);
            SetObjectReference(nativeBridge, "animator", rangedAnimator);
            SetObjectReference(nativeBridge, "movement", movement);
            SetObjectReference(nativeBridge, "actionController", actionController);
            SetObjectReference(nativeBridge, "combatModeController", combatModeController);
            SetObjectReference(nativeBridge, "rangedAimController", rangedAimController);
            SetObjectReference(nativeBridge, "rangedBasicAttackAction", rangedBasicAttackAction);
            SetString(nativeBridge, "normalIdleTrigger", "IDLE");
            SetString(nativeBridge, "normalWalkTrigger", "WALK");
            SetString(nativeBridge, "normalRunTrigger", "RUN");
            SetString(nativeBridge, "idleTrigger", "IDLE 0");
            SetString(nativeBridge, "shootTrigger", "SHOOT");
            SetString(nativeBridge, "autoShootTrigger", "AUTO SHOOT");
            SetString(nativeBridge, "jogTrigger", "JOG");
            SetString(nativeBridge, "walkForwardTrigger", "WALK F");
            SetString(nativeBridge, "walkBackTrigger", "WALK B");
            SetString(nativeBridge, "walkForwardLeftTrigger", "WALK FL");
            SetString(nativeBridge, "walkForwardRightTrigger", "WALK FR");
            SetString(nativeBridge, "walkBackLeftTrigger", "WALK BL");
            SetString(nativeBridge, "walkBackRightTrigger", "WALK BR");
            SetString(nativeBridge, "dodgeTrigger", "EVADE");
            SetBool(nativeBridge, "useNativeAutoShootLoop", false);
            SetBool(nativeBridge, "triggerAutoShootOncePerHold", true);
            SetFloat(nativeBridge, "stationaryFirePoseHoldSeconds", 0.36f);
            SetBool(nativeBridge, "keepMovingLocomotionDuringFire", true);
            SetFloat(nativeBridge, "locomotionTriggerHoldSeconds", 0.18f);
            EditorUtility.SetDirty(nativeBridge);
        }

        private static void ConfigurePlayerRangedBasicVfxCueDriver(
            GameObject player,
            PlayerRangedBasicAttackAction rangedBasicAttackAction,
            CombatVfxCuePlayer cuePlayer,
            Transform muzzleAnchor)
        {
            PlayerRangedBasicVfxCueDriver driver = EnsureComponent<PlayerRangedBasicVfxCueDriver>(player);
            driver.Configure(rangedBasicAttackAction, cuePlayer, muzzleAnchor);
            SetObjectReference(driver, "rangedBasicAttackAction", rangedBasicAttackAction);
            SetObjectReference(driver, "cuePlayer", cuePlayer);
            SetObjectReference(driver, "muzzleAnchor", muzzleAnchor);
            SetEnum(driver, "muzzleFlashCueId", (int)CombatVfxCueId.PlayerRangedMuzzleFlash);
            SetFloat(driver, "muzzleFlashIntensity", 1f);
            SetFloat(driver, "muzzleFlashAudioIntensity", 1f);
            SetBool(driver, "playImpactVfx", PlayerRangedBasicVfxCueDriver.DefaultPlayImpactVfx);
            SetBool(driver, "playImpactAudio", PlayerRangedBasicVfxCueDriver.DefaultPlayImpactAudio);
            SetEnum(driver, "impactCueId", (int)PlayerRangedBasicVfxCueDriver.DefaultImpactCueId);
            SetFloat(driver, "impactIntensity", PlayerRangedBasicVfxCueDriver.DefaultImpactIntensity);
            SetFloat(driver, "impactAudioIntensity", PlayerRangedBasicVfxCueDriver.DefaultImpactAudioIntensity);
            ConfigurePlayerRangedReloadSfxDriver(player, rangedBasicAttackAction);
            EditorUtility.SetDirty(player);
            EditorUtility.SetDirty(driver);
        }

        private static void ConfigurePlayerRangedReloadSfxDriver(
            GameObject player,
            PlayerRangedBasicAttackAction rangedBasicAttackAction)
        {
            PlayerRangedReloadSfxDriver reloadDriver = EnsureComponent<PlayerRangedReloadSfxDriver>(player);
            Transform audioRoot = EnsureChild(player.transform, PlayerRangedReloadAudioName);
            audioRoot.localPosition = new Vector3(0f, 1.1f, 0.1f);
            AudioSource source = EnsureComponent<AudioSource>(audioRoot.gameObject);
            source.playOnAwake = false;
            source.loop = false;
            source.volume = 0.62f;
            source.pitch = 1f;
            source.spatialBlend = 0f;
            source.dopplerLevel = 0f;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = 3f;
            source.maxDistance = 18f;
            source.priority = 128;

            AudioClip reloadClip = LoadAsset<AudioClip>(PlayerRangedReloadSfxClipPath);
            reloadDriver.Configure(rangedBasicAttackAction, source, new[] { reloadClip });
            SetObjectReference(reloadDriver, "rangedBasicAttackAction", rangedBasicAttackAction);
            SetObjectReference(reloadDriver, "audioSource", source);
            SetObjectReferenceArray(reloadDriver, "reloadClips", new UnityEngine.Object[] { reloadClip });
            SetFloat(reloadDriver, "baseVolume", 0.62f);
            SetFloat(reloadDriver, "minimumPitch", 0.97f);
            SetFloat(reloadDriver, "maximumPitch", 1.03f);
            SetFloat(reloadDriver, "spatialBlend", 0f);
            EditorUtility.SetDirty(audioRoot.gameObject);
            EditorUtility.SetDirty(source);
            EditorUtility.SetDirty(reloadDriver);
        }

        private static void ConfigurePlayerCombatVfxCueDriver(
            GameObject player,
            PlayerActionController actionController,
            CombatHealth playerHealth,
            CombatVfxCuePlayer cuePlayer)
        {
            PlayerCombatVfxCueDriver driver = EnsureComponent<PlayerCombatVfxCueDriver>(player);
            PerfectDodgeVfxDirector perfectDodgeDirector =
                ConfigurePerfectDodgeVfxDirector(player, actionController, playerHealth);
            Transform attackAnchor = EnsureChild(player.transform, "Player_CombatVfx_AttackAnchor");
            Transform dodgeAnchor = EnsureChild(player.transform, "Player_CombatVfx_DodgeAnchor");
            attackAnchor.localPosition = new Vector3(0f, 1.05f, 0.65f);
            dodgeAnchor.localPosition = new Vector3(0f, 0.18f, -0.22f);
            SetObjectReference(driver, "actionController", actionController);
            SetObjectReference(driver, "playerHealth", playerHealth);
            SetObjectReference(driver, "perfectDodgeVfxDirector", perfectDodgeDirector);
            SetObjectReference(driver, "cuePlayer", cuePlayer);
            SetObjectReference(driver, "attackAnchor", attackAnchor);
            SetObjectReference(driver, "dodgeAnchor", dodgeAnchor);
            SetObjectReference(driver, "damageAnchor", attackAnchor);
            SetEnum(driver, "damagedCueId", (int)CombatVfxCueId.PlayerDamaged);
            SetEnum(driver, "criticalCueId", (int)CombatVfxCueId.PlayerCritical);
            ConfigurePerfectDodgeCueDriverDefaults(driver);
            SetFloat(driver, "pressureDamageCueScale", 0.62f);
            SetBool(driver, "playDamageVfx", true);
            SetBool(driver, "playCriticalVfx", true);
            EditorUtility.SetDirty(player);
            EditorUtility.SetDirty(driver);
        }

        private static void ConfigurePerfectDodgeCueDriverDefaults(PlayerCombatVfxCueDriver driver)
        {
            SetEnum(driver, "perfectDodgeTimeFieldCueId", (int)CombatVfxCueId.PlayerPerfectDodgeTimeField);
            SetEnum(driver, "perfectDodgePulsewaveCueId", (int)CombatVfxCueId.PlayerPerfectDodgePulsewave);
            SetEnum(driver, "perfectDodgeHoloCubeCueId", (int)CombatVfxCueId.PlayerPerfectDodgeHoloCube);
            SetEnum(driver, "perfectDodgeWindowCueId", (int)CombatVfxCueId.PlayerPerfectDodgeWindow);
            SetEnum(driver, "perfectDodgeProjectileBlockCueId", (int)CombatVfxCueId.PlayerPerfectDodgeShieldBlockImpact);
            SetFloat(driver, "perfectDodgeCueIntensity", 1.55f);
            SetFloat(driver, "perfectDodgeTimeFieldIntensity", 1f);
            SetFloat(driver, "perfectDodgePulsewaveIntensity", 1.12f);
            SetFloat(driver, "perfectDodgeHoloCubeIntensity", 0.92f);
            SetFloat(driver, "perfectDodgeWindowIntensity", 1f);
            SetFloat(driver, "perfectDodgeProjectileBlockIntensity", 1.18f);
            SetFloat(driver, "perfectDodgeShieldBlockRadius", 0.86f);
            SetFloat(driver, "perfectDodgeAudioIntensity", 1f);
            SetBool(driver, "playPerfectDodgeProjectileBlockVfx", true);
        }

        private static PerfectDodgeVfxDirector ConfigurePerfectDodgeVfxDirector(
            GameObject player,
            PlayerActionController actionController,
            CombatHealth playerHealth)
        {
            PerfectDodgeVfxDirector director = EnsureComponent<PerfectDodgeVfxDirector>(player);
            SetObjectReference(director, "actionController", actionController);
            SetObjectReference(director, "playerHealth", playerHealth);
            SetObjectReference(director, "worldFxMaterial", LoadOrCreatePerfectDodgeWorldFxMaterial());
            SetObjectReference(director, "afterimageMaterial", LoadOrCreatePerfectDodgeAfterimageMaterial());
            SetObjectReferenceArray(
                director,
                "timeWarpClips",
                LoadAudioClipArray(ActionFoundationCombatVfxSetup.GetPlayerPerfectDodgeTimeWarpClipPaths()));
            SetObjectReferenceArray(
                director,
                "successClips",
                LoadAudioClipArray(ActionFoundationCombatVfxSetup.GetPlayerPerfectDodgeSuccessClipPaths()));
            SetFloat(director, "domainSeconds", 3f);
            SetFloat(director, "shockwaveSeconds", 0.72f);
            SetFloat(director, "counterWindowSeconds", 1.05f);
            SetFloat(director, "afterimageSeconds", 0.48f);
            SetFloat(director, "matrixDomainRadius", 7.2f);
            SetFloat(director, "shockwaveRadius", 14.5f);
            SetFloat(director, "worldIntensity", 1.35f);
            SetFloat(director, "threatRadius", 42f);
            return director;
        }

        private static void ConfigurePerfectDodgeTimeWarp(GameObject player, PlayerActionController actionController)
        {
            PerfectDodgeTimeWarp timeWarp = EnsureComponent<PerfectDodgeTimeWarp>(player);
            SetObjectReference(timeWarp, "actionController", actionController);
            SetFloat(timeWarp, "timeScale", 0.18f);
            SetFloat(timeWarp, "durationSeconds", 3f);
            SetFloat(timeWarp, "blendOutSeconds", 0.42f);
            SetFloat(timeWarp, "globalHitStopTimeScale", 0.08f);
            SetFloat(timeWarp, "globalHitStopSeconds", 0.055f);
            SetFloat(timeWarp, "radius", 42f);
            SetFloat(timeWarp, "innerRadius", 18f);
            SetFloat(timeWarp, "receiverRefreshIntervalSeconds", 0.08f);
            EditorUtility.SetDirty(timeWarp);
        }

        private static void ConfigureSummonEnergyVfxCuePresenter(
            GameObject player,
            SummonEnergyLadder energyLadder,
            CombatVfxCuePlayer cuePlayer,
            Transform directionTarget)
        {
            SummonEnergyVfxCuePresenter presenter = EnsureComponent<SummonEnergyVfxCuePresenter>(player);
            Transform cueAnchor = EnsureChild(player.transform, "Player_CombatVfx_AttackAnchor");
            cueAnchor.localPosition = new Vector3(0f, 1.05f, 0.65f);
            presenter.Configure(energyLadder, cuePlayer, cueAnchor, directionTarget);
            SetObjectReference(presenter, "energyLadder", energyLadder);
            SetObjectReference(presenter, "cuePlayer", cuePlayer);
            SetObjectReference(presenter, "cueAnchor", cueAnchor);
            SetObjectReference(presenter, "directionTarget", directionTarget);
            SetEnum(presenter, "forwardRiskCueId", (int)CombatVfxCueId.EliteAuraSignal);
            SetEnum(presenter, "tierReadyCueId", (int)CombatVfxCueId.SummonFollowupWindow);
            SetEnum(presenter, "spendCueId", (int)CombatVfxCueId.SummonFollowupMissed);
            SetFloat(presenter, "forwardRiskCueIntensity", 0.05f);
            SetFloat(presenter, "tierReadyCueIntensity", 0.82f);
            SetFloat(presenter, "spendCueIntensity", 0.5f);
            SetFloat(presenter, "tierIntensityStep", 0.12f);
            SetFloat(presenter, "forwardRiskCueCooldownSeconds", 0.75f);
            EditorUtility.SetDirty(player);
            EditorUtility.SetDirty(presenter);
        }

        private static void ValidateCombatModeController(
            PlayerCombatModeController combatModeController,
            PlayerActionController playerActionController,
            PlayerMovementController playerMovementController,
            PlayerRangedAimController rangedAimController,
            PlayerRangedBasicAttackAction rangedBasicAttackAction)
        {
            GameObject rangedRoot = RequireReferencedObject<GameObject>(combatModeController, "rangedVisualRoot");
            Animator rangedAnimator = RequireReferencedObject<Animator>(combatModeController, "rangedAnimator");
            GameObject meleeRoot = RequireReferencedObject<GameObject>(combatModeController, "meleeVisualRoot");
            Animator meleeAnimator = RequireReferencedObject<Animator>(combatModeController, "meleeAnimator");
            GameObject rangedWeaponRoot = RequireReferencedObject<GameObject>(combatModeController, "rangedWeaponRoot");
            GameObject meleeWeaponRoot = RequireReferencedObject<GameObject>(combatModeController, "meleeWeaponRoot");
            ValidateObjectReference(combatModeController, "actionController", playerActionController);
            ValidateObjectReference(combatModeController, "movementController", playerMovementController);
            ValidateObjectReference(combatModeController, "rangedAimController", rangedAimController);
            ValidateObjectReference(combatModeController, "rangedBasicAttackAction", rangedBasicAttackAction);
            ValidateObjectReference(
                combatModeController,
                "rangedActionProfile",
                LoadAsset<PlayerActionProfile>(LocalDefenseProfilePath));
            ValidateObjectReference(
                combatModeController,
                "meleeActionProfile",
                LoadAsset<PlayerActionProfile>(MeleeActionProfilePath));
            ValidateObjectReference(
                rangedAnimator,
                "m_Controller",
                LoadAsset<RuntimeAnimatorController>(InoriRifleAnimatorControllerPath));
            ValidateObjectReference(
                combatModeController,
                "rangedAnimatorController",
                LoadAsset<RuntimeAnimatorController>(InoriRifleAnimatorControllerPath));
            ValidateObjectReference(
                combatModeController,
                "meleeAnimatorController",
                LoadAsset<RuntimeAnimatorController>(CombatGirlAnimatorControllerPath));
            ValidateBool(combatModeController, "routeAnimatorsByMode", true);
            ValidateBool(combatModeController, "rangedAnimatorUsesExternalPresentationBridge", true);
            ValidateBool(combatModeController, "useSingleCharacterVisual", true);
            ValidateEnum(combatModeController, "startingMode", (int)PlayerCombatMode.Ranged);
            ValidatePlayerCombatModeVisual(rangedRoot, rangedAnimator, rangedWeaponRoot, meleeWeaponRoot);
            if (meleeRoot.activeSelf)
            {
                throw new InvalidOperationException("CombatGirl melee source visual root should stay inactive while the review starts.");
            }

            if (meleeAnimator != rangedAnimator)
            {
                throw new InvalidOperationException("Single-character combat presentation should route both modes through the RifleGirl Animator.");
            }

            ValidateObjectReference(playerActionController, "combatModeController", combatModeController);
            ValidateObjectReference(playerActionController, "animator", null);
            ValidateObjectReference(playerMovementController, "animator", null);
            ValidateBool(playerActionController, "blockBasicAttackInRangedMode", true);
        }

        private static void ValidateRifleGirlNativeBridge(
            RifleGirlNativeGameplayAnimatorBridge nativeBridge,
            Animator rangedAnimator,
            PlayerMovementController movement,
            PlayerActionController actionController,
            PlayerCombatModeController combatModeController,
            PlayerRangedAimController rangedAimController,
            PlayerRangedBasicAttackAction rangedBasicAttackAction)
        {
            ValidateObjectReference(nativeBridge, "animator", rangedAnimator);
            ValidateObjectReference(nativeBridge, "movement", movement);
            ValidateObjectReference(nativeBridge, "actionController", actionController);
            ValidateObjectReference(nativeBridge, "combatModeController", combatModeController);
            ValidateObjectReference(nativeBridge, "rangedAimController", rangedAimController);
            ValidateObjectReference(nativeBridge, "rangedBasicAttackAction", rangedBasicAttackAction);
            ValidateString(nativeBridge, "normalIdleTrigger", "IDLE");
            ValidateString(nativeBridge, "normalWalkTrigger", "WALK");
            ValidateString(nativeBridge, "normalRunTrigger", "RUN");
            ValidateString(nativeBridge, "idleTrigger", "IDLE 0");
            ValidateString(nativeBridge, "shootTrigger", "SHOOT");
            ValidateString(nativeBridge, "autoShootTrigger", "AUTO SHOOT");
            ValidateString(nativeBridge, "jogTrigger", "JOG");
            ValidateString(nativeBridge, "walkForwardTrigger", "WALK F");
            ValidateString(nativeBridge, "walkBackTrigger", "WALK B");
            ValidateString(nativeBridge, "walkForwardLeftTrigger", "WALK FL");
            ValidateString(nativeBridge, "walkForwardRightTrigger", "WALK FR");
            ValidateString(nativeBridge, "walkBackLeftTrigger", "WALK BL");
            ValidateString(nativeBridge, "walkBackRightTrigger", "WALK BR");
            ValidateString(nativeBridge, "dodgeTrigger", "EVADE");
            ValidateBool(nativeBridge, "useNativeAutoShootLoop", false);
            ValidateBool(nativeBridge, "triggerAutoShootOncePerHold", true);
            ValidateFloat(nativeBridge, "stationaryFirePoseHoldSeconds", 0.36f);
            ValidateBool(nativeBridge, "keepMovingLocomotionDuringFire", true);
            ValidateFloat(nativeBridge, "locomotionTriggerHoldSeconds", 0.18f);
        }

        private static void ValidatePlayerRangedBasicVfxCueDriver(
            PlayerRangedBasicVfxCueDriver driver,
            PlayerRangedBasicAttackAction rangedBasicAttackAction,
            CombatVfxCuePlayer cuePlayer,
            Transform muzzleAnchor)
        {
            ValidateObjectReference(driver, "rangedBasicAttackAction", rangedBasicAttackAction);
            ValidateObjectReference(driver, "cuePlayer", cuePlayer);
            ValidateObjectReference(driver, "muzzleAnchor", muzzleAnchor);
            ValidateEnum(driver, "muzzleFlashCueId", (int)CombatVfxCueId.PlayerRangedMuzzleFlash);
            ValidateFloat(driver, "muzzleFlashIntensity", 1f);
            ValidateFloat(driver, "muzzleFlashAudioIntensity", 1f);
            ValidateBool(driver, "playImpactVfx", PlayerRangedBasicVfxCueDriver.DefaultPlayImpactVfx);
            ValidateBool(driver, "playImpactAudio", PlayerRangedBasicVfxCueDriver.DefaultPlayImpactAudio);
            ValidateEnum(driver, "impactCueId", (int)PlayerRangedBasicVfxCueDriver.DefaultImpactCueId);
            ValidateFloat(driver, "impactIntensity", PlayerRangedBasicVfxCueDriver.DefaultImpactIntensity);
            ValidateFloat(driver, "impactAudioIntensity", PlayerRangedBasicVfxCueDriver.DefaultImpactAudioIntensity);
        }

        private static void ValidatePlayerRangedReloadSfxDriver(
            PlayerRangedReloadSfxDriver driver,
            PlayerRangedBasicAttackAction rangedBasicAttackAction)
        {
            ValidateObjectReference(driver, "rangedBasicAttackAction", rangedBasicAttackAction);
            AudioSource source = RequireReferencedObject<AudioSource>(driver, "audioSource");
            Transform audioRoot = source.transform;
            if (!string.Equals(audioRoot.name, PlayerRangedReloadAudioName, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Player ranged reload SFX should use the reviewed reload audio child.");
            }

            ValidateAudioClipArray(driver, "reloadClips", new[] { PlayerRangedReloadSfxClipPath });
            ValidateFloat(driver, "baseVolume", 0.62f);
            ValidateFloat(driver, "minimumPitch", 0.97f);
            ValidateFloat(driver, "maximumPitch", 1.03f);
            ValidateFloat(driver, "spatialBlend", 0f);
            if (driver.ReloadClipCount != 1)
            {
                throw new InvalidOperationException("Player ranged reload SFX driver should expose one reviewed reload clip.");
            }
        }

        private static void ValidatePlayerCombatVfxCueDriver(
            PlayerCombatVfxCueDriver driver,
            PlayerActionController actionController,
            CombatHealth playerHealth,
            CombatVfxCuePlayer cuePlayer)
        {
            ValidateObjectReference(driver, "actionController", actionController);
            ValidateObjectReference(driver, "playerHealth", playerHealth);
            ValidateObjectReference(driver, "cuePlayer", cuePlayer);
            Transform attackAnchor = RequireReferencedObject<Transform>(driver, "attackAnchor");
            RequireReferencedObject<Transform>(driver, "dodgeAnchor");
            ValidateObjectReference(driver, "damageAnchor", attackAnchor);
            PerfectDodgeVfxDirector perfectDodgeDirector =
                RequireReferencedObject<PerfectDodgeVfxDirector>(driver, "perfectDodgeVfxDirector");
            ValidatePerfectDodgeVfxDirector(perfectDodgeDirector, actionController, playerHealth);
            ValidateEnum(driver, "damagedCueId", (int)CombatVfxCueId.PlayerDamaged);
            ValidateEnum(driver, "criticalCueId", (int)CombatVfxCueId.PlayerCritical);
            ValidateEnum(driver, "perfectDodgeTimeFieldCueId", (int)CombatVfxCueId.PlayerPerfectDodgeTimeField);
            ValidateEnum(driver, "perfectDodgePulsewaveCueId", (int)CombatVfxCueId.PlayerPerfectDodgePulsewave);
            ValidateEnum(driver, "perfectDodgeHoloCubeCueId", (int)CombatVfxCueId.PlayerPerfectDodgeHoloCube);
            ValidateEnum(driver, "perfectDodgeWindowCueId", (int)CombatVfxCueId.PlayerPerfectDodgeWindow);
            ValidateEnum(driver, "perfectDodgeProjectileBlockCueId", (int)CombatVfxCueId.PlayerPerfectDodgeShieldBlockImpact);
            ValidateFloat(driver, "perfectDodgeCueIntensity", 1.55f);
            ValidateFloat(driver, "perfectDodgeTimeFieldIntensity", 1f);
            ValidateFloat(driver, "perfectDodgePulsewaveIntensity", 1.12f);
            ValidateFloat(driver, "perfectDodgeHoloCubeIntensity", 0.92f);
            ValidateFloat(driver, "perfectDodgeWindowIntensity", 1f);
            ValidateFloat(driver, "perfectDodgeProjectileBlockIntensity", 1.18f);
            ValidateFloat(driver, "perfectDodgeShieldBlockRadius", 0.86f);
            ValidateFloat(driver, "perfectDodgeAudioIntensity", 1f);
            ValidateFloat(driver, "pressureDamageCueScale", 0.62f);
            ValidateBool(driver, "playDamageVfx", true);
            ValidateBool(driver, "playCriticalVfx", true);
            ValidateBool(driver, "playPerfectDodgeProjectileBlockVfx", true);
        }

        private static void ValidatePerfectDodgeVfxDirector(
            PerfectDodgeVfxDirector director,
            PlayerActionController actionController,
            CombatHealth playerHealth)
        {
            ValidateObjectReference(director, "actionController", actionController);
            ValidateObjectReference(director, "playerHealth", playerHealth);
            ValidateObjectReference(director, "worldFxMaterial", LoadOrCreatePerfectDodgeWorldFxMaterial());
            ValidateObjectReference(director, "afterimageMaterial", LoadOrCreatePerfectDodgeAfterimageMaterial());
            ValidateAudioClipArray(
                director,
                "timeWarpClips",
                ActionFoundationCombatVfxSetup.GetPlayerPerfectDodgeTimeWarpClipPaths());
            ValidateAudioClipArray(
                director,
                "successClips",
                ActionFoundationCombatVfxSetup.GetPlayerPerfectDodgeSuccessClipPaths());
            ValidateFloat(director, "domainSeconds", 3f);
            ValidateFloat(director, "shockwaveSeconds", 0.72f);
            ValidateFloat(director, "counterWindowSeconds", 1.05f);
            ValidateFloat(director, "afterimageSeconds", 0.48f);
            ValidateFloat(director, "matrixDomainRadius", 7.2f);
            ValidateFloat(director, "shockwaveRadius", 14.5f);
            ValidateFloat(director, "worldIntensity", 1.35f);
            ValidateFloat(director, "threatRadius", 42f);
        }

        private static void ValidatePerfectDodgeTimeWarp(
            PerfectDodgeTimeWarp timeWarp,
            PlayerActionController actionController)
        {
            ValidateObjectReference(timeWarp, "actionController", actionController);
            ValidateFloat(timeWarp, "timeScale", 0.18f);
            ValidateFloat(timeWarp, "durationSeconds", 3f);
            ValidateFloat(timeWarp, "blendOutSeconds", 0.42f);
            ValidateFloat(timeWarp, "globalHitStopTimeScale", 0.08f);
            ValidateFloat(timeWarp, "globalHitStopSeconds", 0.055f);
            ValidateFloat(timeWarp, "radius", 42f);
            ValidateFloat(timeWarp, "innerRadius", 18f);
            ValidateFloat(timeWarp, "receiverRefreshIntervalSeconds", 0.08f);
        }

        private static void ValidateSummonEnergyVfxCuePresenter(
            SummonEnergyVfxCuePresenter presenter,
            SummonEnergyLadder energyLadder,
            CombatVfxCuePlayer cuePlayer,
            Transform directionTarget,
            PlayerCombatVfxCueDriver playerVfxCueDriver)
        {
            Transform attackAnchor = RequireReferencedObject<Transform>(playerVfxCueDriver, "attackAnchor");
            ValidateObjectReference(presenter, "energyLadder", energyLadder);
            ValidateObjectReference(presenter, "cuePlayer", cuePlayer);
            ValidateObjectReference(presenter, "cueAnchor", attackAnchor);
            ValidateObjectReference(presenter, "directionTarget", directionTarget);
            ValidateEnum(presenter, "forwardRiskCueId", (int)CombatVfxCueId.EliteAuraSignal);
            ValidateEnum(presenter, "tierReadyCueId", (int)CombatVfxCueId.SummonFollowupWindow);
            ValidateEnum(presenter, "spendCueId", (int)CombatVfxCueId.SummonFollowupMissed);
            ValidateFloat(presenter, "forwardRiskCueIntensity", 0.05f);
            ValidateFloat(presenter, "tierReadyCueIntensity", 0.82f);
            ValidateFloat(presenter, "spendCueIntensity", 0.5f);
            ValidateFloat(presenter, "tierIntensityStep", 0.12f);
            ValidateFloat(presenter, "forwardRiskCueCooldownSeconds", 0.75f);
        }

        private static BossBarragePocketReviewOwner CreatePocketOwner(
            Scene scene,
            CombatHealth playerHealth,
            CombatHealth closeThreatHealth,
            CombatHealth bossHealth,
            SummonEnergyLadder energyLadder,
            PlayerSkill1Action skill1Action,
            PlayerSummonSlot1Action summonSlot1Action,
            PlayerSupportSummonSlotAction summonSlot2Action,
            PlayerSupportSummonSlotAction summonSlot3Action,
            BossBarrageEmitter bossBarrageEmitter,
            BossBasicFireEmitter bossBasicFireEmitter,
            FrontlineWaveStageProfile stageProfile,
            BossPressureCostLadder bossPressureCost,
            BossPressureActionDirector bossPressureActionDirector,
            SummonLaneSpace laneSpace)
        {
            GameObject root = CreateRoot(scene, PocketOwnerRootName);
            BossBarragePocketReviewOwner owner = root.AddComponent<BossBarragePocketReviewOwner>();
            GameObject clearMarker = CreateResultMarker(
                root.transform,
                PocketClearMarkerName,
                laneSpace.GetBattlefieldWorldPoint(-laneSpace.HalfWidth - 1.35f, laneSpace.ForwardBoundaryZ - 0.5f, 0.75f),
                new Color(0.25f, 1f, 0.5f, 1f));
            GameObject failMarker = CreateResultMarker(
                root.transform,
                PocketFailMarkerName,
                laneSpace.GetBattlefieldWorldPoint(laneSpace.HalfWidth + 1.35f, laneSpace.ForwardBoundaryZ - 0.5f, 0.75f),
                new Color(1f, 0.16f, 0.18f, 1f));
            owner.Configure(
                playerHealth,
                closeThreatHealth,
                bossHealth,
                energyLadder,
                skill1Action,
                summonSlot1Action,
                bossBarrageEmitter,
                clearMarker,
                failMarker,
                bossPressureCost,
                bossPressureActionDirector,
                bossBasicFireEmitter);
            SetObjectReference(owner, "bossBasicFireEmitter", bossBasicFireEmitter);
            owner.ConfigureSupportSummonActions(summonSlot2Action, summonSlot3Action);
            SetObjectReference(owner, "summonSlot2Action", summonSlot2Action);
            SetObjectReference(owner, "summonSlot3Action", summonSlot3Action);
            SetObjectReference(
                owner,
                "summonPressureBlockOpportunity",
                LoadAsset<SummonOpportunityWindowProfile>(SummonOpportunityProfilePath));
            SetObjectReference(owner, "stageProfile", stageProfile);
            owner.AssignStageProfileForReview(stageProfile);
            SetFloat(owner, "skill1FollowupClearDelaySeconds", 0.75f);
            EditorUtility.SetDirty(owner);
            return owner;
        }

        private static GameObject CreateResultMarker(Transform parent, string name, Vector3 position, Color color)
        {
            Material material = LoadOrCreateMaterial(
                $"Assets/_Game/Art/Materials/ActionFoundation/{name}.mat",
                color);
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marker.name = name;
            marker.transform.SetParent(parent, worldPositionStays: true);
            marker.transform.position = position;
            marker.transform.rotation = Quaternion.identity;
            marker.transform.localScale = new Vector3(0.75f, 1.5f, 0.75f);
            marker.GetComponent<MeshRenderer>().sharedMaterial = material;
            marker.SetActive(false);
            return marker;
        }

        private static GameObject EnsureResultMarker(Transform parent, string name, Vector3 position, Color color)
        {
            Transform existing = parent.Find(name);
            if (existing == null)
            {
                return CreateResultMarker(parent, name, position, color);
            }

            Material material = LoadOrCreateMaterial(
                $"Assets/_Game/Art/Materials/ActionFoundation/{name}.mat",
                color);
            existing.position = position;
            existing.rotation = Quaternion.identity;
            existing.localScale = new Vector3(0.75f, 1.5f, 0.75f);
            MeshRenderer renderer = existing.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }

            existing.gameObject.SetActive(false);
            return existing.gameObject;
        }

        private static void ConfigurePocketCueBridges(
            BossBarragePocketReviewOwner pocketOwner,
            PlayerSummonSlot1Action summonSlot1Action,
            ActionCameraCueDriver cameraCueDriver,
            ActionCinematicCueDirector cinematicCueDirector,
            PlayerCombatVfxCueDriver playerVfxCueDriver,
            CombatVfxCuePlayer cuePlayer,
            Transform directionTarget)
        {
            BossBarragePocketCameraCueBridge cameraBridge =
                EnsureComponent<BossBarragePocketCameraCueBridge>(pocketOwner.gameObject);
            SetBehaviourEnabled(cameraBridge, true);
            SetObjectReference(cameraBridge, "pocketReviewOwner", pocketOwner);
            SetObjectReference(cameraBridge, "summonSlot1Action", summonSlot1Action);
            SetObjectReference(cameraBridge, "cameraCueDriver", cameraCueDriver);
            SetObjectReference(cameraBridge, "cinematicCueDirector", cinematicCueDirector);

            BossBarragePocketVfxCueBridge vfxBridge =
                EnsureComponent<BossBarragePocketVfxCueBridge>(pocketOwner.gameObject);
            SetObjectReference(vfxBridge, "pocketReviewOwner", pocketOwner);
            SetObjectReference(vfxBridge, "cuePlayer", cuePlayer);
            SetObjectReference(vfxBridge, "followupWindowAnchor", ReadObjectReference<Transform>(playerVfxCueDriver, "attackAnchor"));
            SetObjectReference(vfxBridge, "followupHitAnchor", directionTarget);
            SetObjectReference(vfxBridge, "followupMissedAnchor", ReadObjectReference<Transform>(playerVfxCueDriver, "dodgeAnchor"));
            SetObjectReference(vfxBridge, "pocketClearAnchor", directionTarget);
            SetObjectReference(vfxBridge, "pocketFailAnchor", ReadObjectReference<Transform>(playerVfxCueDriver, "dodgeAnchor"));
            SetObjectReference(vfxBridge, "directionTarget", directionTarget);
            SetFloat(vfxBridge, "hitIntensity", 1.18f);
            SetFloat(vfxBridge, "pocketClearIntensity", 0.92f);
            SetFloat(vfxBridge, "pocketFailIntensity", 1.02f);
            SetEnum(vfxBridge, "pocketFailAccentCueId", (int)CombatVfxCueId.EnemyClosePunishActive);
            SetFloat(vfxBridge, "pocketFailAccentIntensity", 0.88f);
            EditorUtility.SetDirty(cameraBridge);
            EditorUtility.SetDirty(vfxBridge);
        }

        private static void CreateReviewHud(
            Scene scene,
            CombatHealth playerHealth,
            CombatHealth closeThreatHealth,
            CombatHealth bossHealth,
            SummonEnergyLadder energyLadder,
            SummonLaneSpace laneSpace,
            Transform player,
            PlayerCombatModeController combatModeController,
            PlayerRangedAimController rangedAimController,
            PlayerRangedBasicAttackAction rangedBasicAttackAction,
            PlayerSkill1Action skill1Action,
            PlayerSummonSlot1Action summonSlot1Action,
            PlayerSupportSummonSlotAction summonSlot2Action,
            PlayerSupportSummonSlotAction summonSlot3Action,
            BossBarrageEmitter bossBarrageEmitter,
            BossBasicFireEmitter bossBasicFireEmitter,
            BossBarragePocketReviewOwner pocketOwner,
            FrontlineWaveStageProfile stageProfile,
            BossPressureCostLadder bossPressureCost,
            BossPressurePositionController bossPressurePosition,
            BossPressureActionDirector bossPressureActionDirector,
            BossSummonPressureAction bossSummonPressureAction)
        {
            GameObject hudRoot = CreateRoot(scene, HudRootName);
            BossBarrageLaneReviewHud hud = hudRoot.AddComponent<BossBarrageLaneReviewHud>();
            hud.Configure(
                playerHealth,
                closeThreatHealth,
                bossHealth,
                energyLadder,
                laneSpace,
                player,
                combatModeController,
                rangedAimController,
                rangedBasicAttackAction,
                skill1Action,
                summonSlot1Action,
                bossBarrageEmitter,
                pocketOwner,
                bossPressureCost,
                bossPressurePosition,
                bossPressureActionDirector,
                bossSummonPressureAction,
                summonSlot2Action,
                summonSlot3Action,
                bossBasicFireEmitter);
            SetObjectReference(hud, "bossBasicFireEmitter", bossBasicFireEmitter);
            SetObjectReference(hud, "duelReviewOwner", null);
            SetObjectReference(hud, "stageProfile", stageProfile);
            hud.AssignStageProfileForReview(stageProfile);
            SetBool(hud, "showCenterReticle", true);
            SetBool(hud, "showResultBanner", true);
            SetString(hud, "stageEpisodeLabel", stageProfile.StageEpisodeLabel);
            SetString(hud, "objectiveBadgeLabel", stageProfile.ObjectiveBadgeLabel);
            SetFloat(hud, "resultBannerWidth", 540f);
            SetFloat(hud, "resultBannerHeight", 82f);
            SetFloat(hud, "resultBannerBottomOffset", 112f);
            BossBarrageLaneReviewMobileHud mobileHud = hudRoot.AddComponent<BossBarrageLaneReviewMobileHud>();
            mobileHud.Configure(
                player.GetComponent<PlayerMovementController>(),
                player.GetComponent<PlayerActionController>(),
                combatModeController,
                rangedAimController,
                rangedBasicAttackAction,
                skill1Action,
                summonSlot1Action,
                energyLadder,
                summonSlot2Action,
                summonSlot3Action);
            SetObjectReference(mobileHud, "summonSlot2Action", summonSlot2Action);
            SetObjectReference(mobileHud, "summonSlot3Action", summonSlot3Action);
            SetString(mobileHud, "summonSlot2ActionName", BossBarrageSummonReviewContract.Slot2ActionName);
            SetString(mobileHud, "summonSlot3ActionName", BossBarrageSummonReviewContract.Slot3ActionName);
            SetBool(mobileHud, "useSingleSummonButton", BossBarrageSummonReviewContract.UseSingleSummonButton);
            SetString(mobileHud, "summonSlot1Label", BossBarrageSummonReviewContract.Slot1HudLabel);
            SetString(mobileHud, "summonSlot2Label", BossBarrageSummonReviewContract.Slot2HudLabel);
            SetString(mobileHud, "summonSlot3Label", BossBarrageSummonReviewContract.Slot3HudLabel);
            SetFloat(mobileHud, "buttonSize", 168f);
            SetFloat(mobileHud, "buttonGap", 38f);
            SetFloat(mobileHud, "margin", 72f);
            SetFloat(mobileHud, "minimumActionButtonSize", 124f);
            SetFloat(mobileHud, "minimumButtonGap", 30f);
            SetFloat(mobileHud, "minimumTouchEdgeInset", 64f);
            SetFloat(mobileHud, "summonButtonGroupCenterY01", 0.42f);
            SetFloat(mobileHud, "summonButtonGapMultiplier", 1.05f);
            SetFloat(mobileHud, "moveJoystickRadius", 154f);
            SetFloat(mobileHud, "moveJoystickKnobSize", 64f);
            SetFloat(mobileHud, "moveJoystickTouchRadiusScale", 1.45f);
            SetFloat(mobileHud, "minimumMoveJoystickRadius", 118f);
            SetFloat(mobileHud, "minimumMoveJoystickKnobSize", 52f);
            SetBool(mobileHud, "screenDragControlsAim", true);
            SetBool(mobileHud, "rightMouseDragControlsAim", false);
            SetBool(mobileHud, "leftMouseDragControlsAim", true);
            SetBool(mobileHud, "routeAimToMovementLook", false);
            SetBool(mobileHud, "keyboardPeekControlsAim", true);
            SetEnum(mobileHud, "keyboardPeekLeftKey", (int)Key.Q);
            SetEnum(mobileHud, "keyboardPeekRightKey", (int)Key.E);
            SetBool(mobileHud, "keyboardPeekRequiresActiveAim", true);
            SetFloat(mobileHud, "lookAimDragSensitivity", 0.00435f);
            SetBool(mobileHud, "fireAimReticleUsesScreenCenter", true);
            SetBool(mobileHud, "fireAimReticleFollowsAssist", true);
            SetFloat(mobileHud, "fireAimAssistReticleMaxOffset", 96f);

            ActionScreenCuePresenter screenCuePresenter = hudRoot.AddComponent<ActionScreenCuePresenter>();
            screenCuePresenter.Configure(
                player.GetComponent<PlayerActionController>(),
                playerHealth,
                rangedBasicAttackAction,
                energyLadder,
                skill1Action,
                summonSlot1Action,
                summonSlot2Action,
                summonSlot3Action,
                bossBarrageEmitter,
                bossPressureActionDirector,
                pocketOwner);
            SetObjectReference(screenCuePresenter, "duelReviewOwner", null);
            SetBool(screenCuePresenter, "showScreenCues", true);
            SetBool(screenCuePresenter, "showEventColorCues", false);
            SetFloat(screenCuePresenter, "maxFullScreenAlpha", 0.10f);
            SetFloat(screenCuePresenter, "maxEdgeAlpha", 0.26f);
            SetFloat(screenCuePresenter, "edgeThickness", 104f);
            SetFloat(screenCuePresenter, "maxPerfectDodgeDomainAlpha", 0.42f);
            SetFloat(screenCuePresenter, "maxPerfectDodgeInvertAlpha", 0.18f);
            SetFloat(screenCuePresenter, "maxPerfectDodgeEdgeAlpha", 0.48f);
            SetFloat(screenCuePresenter, "perfectDodgeDomainSeconds", 3f);
            SetFloat(screenCuePresenter, "perfectDodgePulseSeconds", 0.22f);
            SetFloat(screenCuePresenter, "perfectDodgeBandThickness", 26f);
            ConfigurePerfectDodgeScreenCueMaterials(screenCuePresenter);
            SetBool(screenCuePresenter, "useDamageScreenFeedback", false);
            SetFloat(screenCuePresenter, "maxDamageVignetteAlpha", 0.42f);
            SetFloat(screenCuePresenter, "maxDamageFlashAlpha", 0.11f);
            SetFloat(screenCuePresenter, "damageVignetteSeconds", 0.34f);
            SetFloat(screenCuePresenter, "pressureDamageFeedbackScale", 0.58f);
            SetFloat(screenCuePresenter, "controlLockDamageExtraSeconds", 0.10f);
            SetFloat(screenCuePresenter, "heavyDamageExtraSeconds", 0.14f);
            SetFloat(screenCuePresenter, "heavyDamageHealthRatio", 0.26f);
            SetFloat(screenCuePresenter, "criticalHealthThreshold", 0.32f);
            SetFloat(screenCuePresenter, "criticalHealthPulseAlpha", 0.13f);
            SetFloat(screenCuePresenter, "criticalHealthPulseSeconds", 0.9f);
            SetFloat(screenCuePresenter, "criticalHealthPulseRate", 2.3f);
            SetFloat(screenCuePresenter, "damageDirectionAccentAlpha", 0.24f);
            SetFloat(screenCuePresenter, "damageDirectionAccentThickness", 178f);

            BossBarrageLaneReviewOverlayHud overlayHud = hudRoot.AddComponent<BossBarrageLaneReviewOverlayHud>();
            overlayHud.Configure(
                pocketOwner,
                hud,
                mobileHud,
                screenCuePresenter);
            ConfigureOverlayRoutes(overlayHud);
            SetBool(hud, "showHud", false);
            SetBool(mobileHud, "drawHudVisuals", false);
            SetBool(overlayHud, "drawIdleButton", false);
            CreateCombatHudCanvas(
                scene,
                playerHealth,
                bossHealth,
                energyLadder,
                player.GetComponent<PlayerActionController>(),
                combatModeController,
                rangedBasicAttackAction,
                skill1Action,
                summonSlot1Action,
                summonSlot2Action,
                summonSlot3Action,
                pocketOwner,
                overlayHud);
            // Touch/reticle composition is review-scene HUD tuning. Keep it Inspector-authored.
            EditorUtility.SetDirty(hud);
            EditorUtility.SetDirty(mobileHud);
            EditorUtility.SetDirty(screenCuePresenter);
            EditorUtility.SetDirty(overlayHud);
        }

        private static void ConfigureOverlayRoutes(BossBarrageLaneReviewOverlayHud overlayHud)
        {
            UIScreenRouteTable routeTable = AssetDatabase.LoadAssetAtPath<UIScreenRouteTable>(UIRouteTablePath);
            UIScreenRouteTable.Route retryRoute = ResolveRoute(routeTable, UIRouteId.CombatHud);
            UIScreenRouteTable.Route stageSelectRoute = ResolveRoute(routeTable, UIRouteId.StageSelect);
            UIScreenRouteTable.Route lobbyRoute = ResolveRoute(routeTable, UIRouteId.Lobby);
            overlayHud.ConfigureRoutes(
                retryRoute.SceneName,
                retryRoute.ScenePath,
                stageSelectRoute.SceneName,
                stageSelectRoute.ScenePath,
                lobbyRoute.SceneName,
                lobbyRoute.ScenePath);
        }

        private static void ConfigurePerfectDodgeScreenCueMaterials(ActionScreenCuePresenter screenCuePresenter)
        {
            if (screenCuePresenter == null)
            {
                return;
            }

            SetObjectReference(screenCuePresenter, "perfectDodgeDomainMaterial", LoadOrCreatePerfectDodgeScreenDomainMaterial());
            SetObjectReference(
                screenCuePresenter,
                "perfectDodgeGlitchOverlayMaterial",
                LoadAsset<Material>(PerfectDodgeGlitchOverlayMaterialPath));
            SetFloat(screenCuePresenter, "perfectDodgeShaderIntensity", PerfectDodgeScreenShaderIntensity);
            SetFloat(screenCuePresenter, "perfectDodgeRadialWarpStrength", PerfectDodgeScreenRadialWarpStrength);
            SetFloat(screenCuePresenter, "perfectDodgeScanlineStrength", PerfectDodgeScreenScanlineStrength);
            SetFloat(screenCuePresenter, "perfectDodgeRadialBlurStrength", PerfectDodgeScreenRadialBlurStrength);
            SetFloat(screenCuePresenter, "perfectDodgeGridStrength", PerfectDodgeScreenGridStrength);
            SetFloat(screenCuePresenter, "perfectDodgeFractureStrength", PerfectDodgeScreenFractureStrength);
            SetFloat(screenCuePresenter, "perfectDodgeChromaticStrength", PerfectDodgeScreenChromaticStrength);
            SetFloat(screenCuePresenter, "perfectDodgeGlitchOverlayAlpha", PerfectDodgeGlitchOverlayAlpha);
            SetFloat(screenCuePresenter, "perfectDodgeGlitchNoiseStrength", PerfectDodgeGlitchNoiseStrength);
            SetFloat(screenCuePresenter, "perfectDodgeGlitchJitterStrength", PerfectDodgeGlitchJitterStrength);
            EditorUtility.SetDirty(screenCuePresenter);
        }

        private static UIScreenRouteTable.Route ResolveRoute(UIScreenRouteTable routeTable, UIRouteId routeId)
        {
            if (routeTable == null || !routeTable.TryGetRoute(routeId, out UIScreenRouteTable.Route route))
            {
                throw new InvalidOperationException($"Missing UI route {routeId} for boss barrage review overlay.");
            }

            return route;
        }

        private static void ConfigurePlayerEnergyActions(
            GameObject playerRoot,
            CombatHealth playerHealth,
            PlayerCombatTargetSelector targetSelector,
            CombatHealth frontlineTargetHealth,
            SummonEnergyLadder energyLadder,
            SummonLaneSpace laneSpace,
            LaneActionProjectile skill1ProjectilePrefab,
            LaneActionProjectile summonSlot1ProjectilePrefab,
            LaneActionProjectile summonSlot2ProjectilePrefab,
            LaneActionProjectile summonSlot3ProjectilePrefab,
            GameObject summonEntryCuePrefab,
            SummonFrontlineProxy summonActorPrefab,
            SummonFrontlineProxy summonSlot2ActorPrefab,
            SummonFrontlineProxy summonSlot3ActorPrefab,
            Transform projectileRoot,
            Transform actionCueRoot,
            Transform summonActorRoot)
        {
            PlayerSkill1Action skill1Action = EnsureComponent<PlayerSkill1Action>(playerRoot);
            SetObjectReference(skill1Action, "energyLadder", energyLadder);
            SetObjectReference(skill1Action, "sourceHealth", playerHealth);
            SetObjectReference(skill1Action, "targetSelector", targetSelector);
            SetObjectReference(skill1Action, "projectilePrefab", skill1ProjectilePrefab);
            SetObjectReference(skill1Action, "projectilePrefabObject", LoadAsset<GameObject>(Skill1ProjectilePrefabPath));
            SetObjectReference(skill1Action, "projectileRoot", projectileRoot);
            SetEnum(skill1Action, "sourceTeam", (int)DamageTeam.Player);
            SetInt(skill1Action, "prewarmCount", 6);
            ConfigureSkill1TierSettings(skill1Action);

            PlayerSummonSlot1Action summonSlot1Action = EnsureComponent<PlayerSummonSlot1Action>(playerRoot);
            CombatVfxCuePlayer playerCuePlayer =
                RequireComponent<CombatVfxCuePlayer>(playerRoot, "player combat VFX cue player");
            SetObjectReference(summonSlot1Action, "energyLadder", energyLadder);
            SetObjectReference(summonSlot1Action, "sourceHealth", playerHealth);
            SetObjectReference(summonSlot1Action, "targetSelector", targetSelector);
            SetObjectReference(summonSlot1Action, "frontlineTargetHealth", frontlineTargetHealth);
            SetObjectReference(summonSlot1Action, "laneSpace", laneSpace);
            SetObjectReference(summonSlot1Action, "projectilePrefab", summonSlot1ProjectilePrefab);
            SetObjectReference(summonSlot1Action, "projectilePrefabObject", LoadAsset<GameObject>(SummonSlot1ProjectilePrefabPath));
            SetObjectReference(summonSlot1Action, "entryCuePrefab", summonEntryCuePrefab);
            SetObjectReference(summonSlot1Action, "summonActorPrefab", summonActorPrefab);
            SetObjectReference(summonSlot1Action, "summonActorPrefabObject", LoadAsset<GameObject>(SummonSlot1ActorPrefabPath));
            SetObjectReference(summonSlot1Action, "projectileRoot", projectileRoot);
            SetObjectReference(summonSlot1Action, "cueRoot", actionCueRoot);
            SetObjectReference(summonSlot1Action, "summonActorRoot", summonActorRoot);
            SetObjectReference(summonSlot1Action, "combatVfxCuePlayer", playerCuePlayer);
            SetEnum(summonSlot1Action, "sourceTeam", (int)DamageTeam.AllySummon);
            SetInt(summonSlot1Action, "prewarmCount", 8);
            SetInt(summonSlot1Action, "actorPrewarmCount", 2);
            SetInt(summonSlot1Action, "maxActiveSummonActors", 1);
            SetFloat(summonSlot1Action, "entryForwardOffset", 1.35f);
            SetFloat(summonSlot1Action, "actorEntryCatchupSecondsPerMeter", 0.12f);
            summonSlot1Action.ConfigureRequiredSummonMana(BossBarrageSummonReviewContract.Slot1RequiredMana);
            summonSlot1Action.ConfigureSlotCooldown(BossBarrageSummonReviewContract.Slot1CooldownSeconds);
            summonSlot1Action.ConfigureSummonActionProfile(
                LoadAsset<SummonSlotActionProfile>(SummonSlot1ActionProfilePath));
            EditorUtility.SetDirty(summonSlot1Action);

            PlayerSupportSummonSlotAction summonSlot2Action =
                EnsureSupportSummonSlotAction(playerRoot, BossBarrageSummonReviewContract.Slot2ActionName);
            summonSlot2Action.ConfigureSlot(
                BossBarrageSummonReviewContract.Slot2ActionName,
                Key.Digit2,
                new Vector2(-1.55f, 0.35f));
            summonSlot2Action.ConfigureRequiredSummonMana(BossBarrageSummonReviewContract.Slot2RequiredMana);
            summonSlot2Action.ConfigureMinimumSummonTier(BossBarrageSummonReviewContract.Slot2MinimumTier);
            summonSlot2Action.ConfigureSlotCooldown(BossBarrageSummonReviewContract.Slot2CooldownSeconds);
            SetInt(summonSlot2Action, "maxActiveSummonActors", 1);
            SetFloat(summonSlot2Action, "entryForwardOffset", 1.35f);
            SetFloat(summonSlot2Action, "actorEntryCatchupSecondsPerMeter", 0.1f);
            summonSlot2Action.ConfigureSupportCadence(0.18f, 1.05f, 3);
            summonSlot2Action.ConfigureReferences(
                energyLadder,
                playerHealth,
                targetSelector,
                frontlineTargetHealth,
                laneSpace,
                summonSlot2ProjectilePrefab,
                summonEntryCuePrefab,
                summonSlot2ActorPrefab,
                projectileRoot,
                actionCueRoot,
                summonActorRoot,
                playerCuePlayer);
            SetObjectReference(summonSlot2Action, "combatVfxCuePlayer", playerCuePlayer);
            summonSlot2Action.ConfigureSummonActionProfile(
                LoadAsset<SummonSlotActionProfile>(SummonSlot2ActionProfilePath));
            EditorUtility.SetDirty(summonSlot2Action);

            PlayerSupportSummonSlotAction summonSlot3Action =
                EnsureSupportSummonSlotAction(playerRoot, BossBarrageSummonReviewContract.Slot3ActionName);
            summonSlot3Action.ConfigureSlot(
                BossBarrageSummonReviewContract.Slot3ActionName,
                Key.Digit3,
                new Vector2(1.55f, 0.55f));
            summonSlot3Action.ConfigureRequiredSummonMana(BossBarrageSummonReviewContract.Slot3RequiredMana);
            summonSlot3Action.ConfigureMinimumSummonTier(BossBarrageSummonReviewContract.Slot3MinimumTier);
            summonSlot3Action.ConfigureSlotCooldown(BossBarrageSummonReviewContract.Slot3CooldownSeconds);
            SetInt(summonSlot3Action, "maxActiveSummonActors", 1);
            SetFloat(summonSlot3Action, "entryForwardOffset", 1.35f);
            SetFloat(summonSlot3Action, "actorEntryCatchupSecondsPerMeter", 0.12f);
            summonSlot3Action.ConfigureSupportCadence(0.65f, 2.4f, 1);
            summonSlot3Action.ConfigureReferences(
                energyLadder,
                playerHealth,
                targetSelector,
                frontlineTargetHealth,
                laneSpace,
                summonSlot3ProjectilePrefab,
                summonEntryCuePrefab,
                summonSlot3ActorPrefab,
                projectileRoot,
                actionCueRoot,
                summonActorRoot,
                playerCuePlayer);
            SetObjectReference(summonSlot3Action, "combatVfxCuePlayer", playerCuePlayer);
            summonSlot3Action.ConfigureSummonActionProfile(
                LoadAsset<SummonSlotActionProfile>(SummonSlot3ActionProfilePath));
            EditorUtility.SetDirty(summonSlot3Action);
        }

        private static void ConfigureSkill1TierSettings(PlayerSkill1Action skill1Action)
        {
            SerializedObject serializedObject = new SerializedObject(skill1Action);
            SerializedProperty tierSettings = serializedObject.FindProperty("tierSettings");
            tierSettings.arraySize = 3;
            ConfigureSkill1TierSetting(
                tierSettings.GetArrayElementAtIndex(0),
                damage: 84f,
                projectileSpeed: 31f,
                lifetimeSeconds: 1.15f,
                radius: 0.42f,
                projectileCount: 1,
                lateralSpread: 0f,
                spawnForwardOffset: 0.85f,
                spawnHeight: 1.15f,
                targetHeight: 1.25f);
            ConfigureSkill1TierSetting(
                tierSettings.GetArrayElementAtIndex(1),
                damage: 208f,
                projectileSpeed: 34f,
                lifetimeSeconds: 1.25f,
                radius: 0.5f,
                projectileCount: 1,
                lateralSpread: 0f,
                spawnForwardOffset: 0.9f,
                spawnHeight: 1.2f,
                targetHeight: 1.25f);
            ConfigureSkill1TierSetting(
                tierSettings.GetArrayElementAtIndex(2),
                damage: 384f,
                projectileSpeed: 38f,
                lifetimeSeconds: 1.35f,
                radius: 0.62f,
                projectileCount: 1,
                lateralSpread: 0f,
                spawnForwardOffset: 0.95f,
                spawnHeight: 1.25f,
                targetHeight: 1.3f);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(skill1Action);
        }

        private static void ConfigureSkill1TierSetting(
            SerializedProperty property,
            float damage,
            float projectileSpeed,
            float lifetimeSeconds,
            float radius,
            int projectileCount,
            float lateralSpread,
            float spawnForwardOffset,
            float spawnHeight,
            float targetHeight)
        {
            property.FindPropertyRelative("Damage").floatValue = damage;
            property.FindPropertyRelative("ProjectileSpeed").floatValue = projectileSpeed;
            property.FindPropertyRelative("LifetimeSeconds").floatValue = lifetimeSeconds;
            property.FindPropertyRelative("Radius").floatValue = radius;
            property.FindPropertyRelative("ProjectileCount").intValue = projectileCount;
            property.FindPropertyRelative("LateralSpread").floatValue = lateralSpread;
            property.FindPropertyRelative("SpawnForwardOffset").floatValue = spawnForwardOffset;
            property.FindPropertyRelative("SpawnHeight").floatValue = spawnHeight;
            property.FindPropertyRelative("TargetHeight").floatValue = targetHeight;
        }

        private static PlayerSupportSummonSlotAction EnsureSupportSummonSlotAction(
            GameObject owner,
            string slotActionName)
        {
            PlayerSupportSummonSlotAction[] actions =
                owner.GetComponents<PlayerSupportSummonSlotAction>();
            for (int i = 0; i < actions.Length; i++)
            {
                if (actions[i] != null && actions[i].SlotActionName == slotActionName)
                {
                    return actions[i];
                }
            }

            PlayerSupportSummonSlotAction action = owner.AddComponent<PlayerSupportSummonSlotAction>();
            action.ConfigureSlot(slotActionName, slotActionName == "SummonSlot3" ? Key.Digit3 : Key.Digit2, Vector2.zero);
            return action;
        }

        private static PlayerSupportSummonSlotAction RequireSupportSummonSlotAction(
            GameObject owner,
            string slotActionName)
        {
            PlayerSupportSummonSlotAction[] actions =
                owner.GetComponents<PlayerSupportSummonSlotAction>();
            for (int i = 0; i < actions.Length; i++)
            {
                if (actions[i] != null && actions[i].SlotActionName == slotActionName)
                {
                    return actions[i];
                }
            }

            throw new InvalidOperationException($"{owner.name} is missing support summon action {slotActionName}.");
        }

        private static void ConfigureFixedRearCamera(
            ActionCameraController cameraController,
            Transform player,
            Transform bossProxy,
            Transform rearYawReference)
        {
            Vector3 lookTarget = player.position + CameraLookOffset;
            float orbitYaw = rearYawReference != null ? rearYawReference.eulerAngles.y : player.eulerAngles.y;
            Quaternion orbitRotation = Quaternion.Euler(0f, orbitYaw, 0f);
            Vector3 position = lookTarget + orbitRotation * CameraStartOffset;
            Vector3 lookDirection = lookTarget - position;
            cameraController.transform.SetPositionAndRotation(
                position,
                Quaternion.LookRotation(lookDirection.normalized, Vector3.up));

            Camera camera = cameraController.GetComponent<Camera>();
            if (camera != null)
            {
                camera.fieldOfView = 54f;
                EditorUtility.SetDirty(camera);
            }

            SetObjectReference(cameraController, "target", player);
            SetObjectReference(cameraController, "threat", bossProxy);
            SetVector3(cameraController, "cameraOffset", CameraStartOffset);
            SetVector3(cameraController, "lookOffset", CameraLookOffset);
            SetBool(cameraController, "useFixedRearYaw", true);
            SetObjectReference(cameraController, "fixedRearYawReference", rearYawReference);
            SetFloat(cameraController, "fixedRearYawOffsetDegrees", 0f);
            SetFloat(cameraController, "orbitYawDegrees", orbitYaw);
            SetBool(cameraController, "useDeviceFallbackWhenActionMissing", false);
            SetFloat(cameraController, "manualYawSpeedDegrees", 0f);
            SetFloat(cameraController, "mouseYawDegreesPerPixel", 0f);
            SetFloat(cameraController, "targetYawAssist", 0f);
            SetFloat(cameraController, "threatBias", 0f);
            SetFloat(cameraController, "maxThreatFocusOffset", 0.75f);
            SetFloat(cameraController, "maxLeadFromPlayerSpeed", 0f);
            SetVector3(cameraController, "aimCameraOffset", CameraAimOffset);
            SetVector3(cameraController, "aimFocusOffset", CameraAimFocusOffset);
            SetFloat(cameraController, "aimFieldOfViewDelta", CameraAimFieldOfViewDelta);
            SetFloat(cameraController, "aimBlendInSpeed", CameraAimBlendInSpeed);
            SetFloat(cameraController, "aimBlendOutSpeed", CameraAimBlendOutSpeed);
            SetBool(cameraController, "aimOrbitUsesInput", true);
            SetBool(cameraController, "aimOrbitRotatesCameraPosition", true);
            SetFloat(cameraController, "aimOrbitYawLimitDegrees", CameraAimYawLimitDegrees);
            SetBool(cameraController, "aimOrbitUsesPitchInput", true);
            SetFloat(cameraController, "aimOrbitPitchLimitDegrees", CameraAimPitchLimitDegrees);
            SetBool(cameraController, "aimAssistUsesYawTarget", false);
            SetFloat(cameraController, "aimAssistMaxYawBlend", 0.85f);
            SetFloat(cameraController, "aimAssistYawSpeedDegrees", 420f);
            SetFloat(cameraController, "aimAssistYawReturnSpeedDegrees", 520f);
        }

        private static void ConfigureRangedAimController(
            GameObject player,
            ActionCameraController cameraController,
            Animator rangedAnimator)
        {
            PlayerCombatModeController combatModeController =
                RequireComponent<PlayerCombatModeController>(player, "player combat mode controller");
            PlayerMovementController movement = RequireComponent<PlayerMovementController>(player, "player movement controller");
            PlayerRangedAimController aimController = EnsureComponent<PlayerRangedAimController>(player);
            aimController.ConfigureReferences(combatModeController, cameraController, rangedAnimator, movement);
            SetObjectReference(aimController, "combatModeController", combatModeController);
            SetObjectReference(aimController, "cameraController", cameraController);
            SetObjectReference(aimController, "movement", movement);
            SetObjectReference(aimController, "animator", rangedAnimator);
            SetBool(aimController, "holdToAim", true);
            // Review-only device fallback stays off keyboard so temporary test keys cannot collide with action keys.
            SetBool(aimController, "useDeviceFallbackWhenActionMissing", true);
            SetBool(aimController, "allowMouseAimFallback", false);
            SetString(aimController, "aimingParameter", string.Empty);
            SetBool(aimController, "faceCameraForwardWhileAiming", true);
            SetBool(aimController, "snapAimingFacing", false);
        }

        private static PlayerRangedBasicAttackAction ConfigurePlayerRangedBasicAttack(
            GameObject player,
            PlayerCombatModeController combatModeController,
            PlayerRangedAimController rangedAimController,
            PlayerMovementController movement,
            PlayerCombatTargetSelector targetSelector,
            CombatHealth playerHealth,
            ActionCameraController cameraController,
            Animator rangedAnimator,
            LaneActionProjectile projectilePrefab,
            Transform projectileRoot,
            Transform fireOrigin)
        {
            PlayerRangedBasicAttackAction rangedBasicAttackAction =
                EnsureComponent<PlayerRangedBasicAttackAction>(player);
            rangedBasicAttackAction.SetFireOrigin(fireOrigin);
            rangedBasicAttackAction.ConfigureReferences(
                combatModeController,
                rangedAimController,
                movement,
                targetSelector,
                playerHealth,
                cameraController,
                rangedAnimator);
            SetObjectReference(rangedBasicAttackAction, "combatModeController", combatModeController);
            SetObjectReference(rangedBasicAttackAction, "aimController", rangedAimController);
            SetObjectReference(rangedBasicAttackAction, "movement", movement);
            SetObjectReference(rangedBasicAttackAction, "targetSelector", targetSelector);
            SetObjectReference(rangedBasicAttackAction, "sourceHealth", playerHealth);
            SetObjectReference(rangedBasicAttackAction, "cameraController", cameraController);
            SetObjectReference(rangedBasicAttackAction, "animator", rangedAnimator);
            SetObjectReference(rangedBasicAttackAction, "projectilePrefab", projectilePrefab);
            SetObjectReference(rangedBasicAttackAction, "projectilePrefabObject", LoadAsset<GameObject>(RangedBasicProjectilePrefabPath));
            SetObjectReference(rangedBasicAttackAction, "projectileRoot", projectileRoot);
            SetObjectReference(rangedBasicAttackAction, "fireOrigin", fireOrigin);
            SetEnum(rangedBasicAttackAction, "sourceTeam", (int)DamageTeam.Player);
            SetInt(rangedBasicAttackAction, "prewarmCount", PlayerRangedBasicPrewarmCount);
            SetBool(rangedBasicAttackAction, "allowMouseFireFallback", false);
            SetBool(rangedBasicAttackAction, "requestFacingOnFire", false);
            SetBool(rangedBasicAttackAction, "snapFacingOnFire", false);
            SetBool(rangedBasicAttackAction, "suppressFacingOnFireWhileMoving", true);
            SetFloat(rangedBasicAttackAction, "movingFacingSuppressSpeed", 0.08f);
            SetBool(rangedBasicAttackAction, "useFixedCenterAimViewport", true);
            SetBool(rangedBasicAttackAction, "useStableAimOrigin", true);
            SetBool(rangedBasicAttackAction, "cameraAimIgnoresNonTargetHits", true);
            SetBool(rangedBasicAttackAction, "stabilizeDirectTargetAimHeight", false);
            SetBool(rangedBasicAttackAction, "useAimAssist", true);
            SetBool(rangedBasicAttackAction, "disableAimAssistWithManualInput", false);
            SetBool(rangedBasicAttackAction, "driveCameraAimAssist", false);
            SetBool(rangedBasicAttackAction, "useMagazineReload", true);
            SetInt(rangedBasicAttackAction, "magazineSize", PlayerRangedBasicMagazineSize);
            SetBool(rangedBasicAttackAction, "autoReloadWhenEmpty", true);
            SetBool(rangedBasicAttackAction, "reloadWhenAimReleased", true);
            SetBool(rangedBasicAttackAction, "cancelAimReleaseReloadOnAimResume", true);
            SetFloat(rangedBasicAttackAction, "damage", PlayerRangedBasicDamage);
            SetFloat(rangedBasicAttackAction, "projectileSpeed", PlayerRangedBasicProjectileSpeed);
            SetFloat(rangedBasicAttackAction, "projectileLifetimeSeconds", PlayerRangedBasicProjectileLifetimeSeconds);
            SetFloat(rangedBasicAttackAction, "projectileRadius", PlayerRangedBasicProjectileRadius);
            SetFloat(rangedBasicAttackAction, "fireIntervalSeconds", PlayerRangedBasicFireIntervalSeconds);
            SetFloat(rangedBasicAttackAction, "targetHeight", PlayerRangedBasicTargetHeight);
            SetFloat(rangedBasicAttackAction, "cameraAimFallbackDistance", PlayerRangedBasicCameraAimFallbackDistance);
            SetFloat(rangedBasicAttackAction, "cameraAimRaycastDistance", PlayerRangedBasicCameraAimRaycastDistance);
            SetFloat(rangedBasicAttackAction, "aimAssistDistance", PlayerRangedBasicAimAssistDistance);
            SetFloat(rangedBasicAttackAction, "hipAimAssistAngleDegrees", PlayerRangedBasicAimAssistAngleDegrees);
            SetFloat(rangedBasicAttackAction, "aimedAimAssistAngleDegrees", PlayerRangedBasicAimAssistAngleDegrees);
            SetFloat(rangedBasicAttackAction, "aimAssistMaxTurnDegrees", PlayerRangedBasicAimAssistAngleDegrees);
            SetFloat(rangedBasicAttackAction, "cameraAimAssistStrengthScale", 1f);
            SetFloat(rangedBasicAttackAction, "cameraAimAssistMinStrength", 0.05f);
            SetString(rangedBasicAttackAction, "fireTrigger", string.Empty);
            // Damage, shot cadence, aim assist, muzzle framing, and fire camera feedback are authored tuning.
            EditorUtility.SetDirty(rangedBasicAttackAction);
            return rangedBasicAttackAction;
        }

        private static void ConfigureActionCameraCueDriver(
            ActionCameraController cameraController,
            PlayerActionController actionController,
            PlayerMovementController movement,
            PlayerSkill1Action skill1Action,
            PlayerSummonSlot1Action summonSlot1Action,
            PlayerRangedBasicAttackAction rangedBasicAttackAction,
            CombatVfxCuePlayer playerCuePlayer,
            Animator cueAnimator,
            Animator cinematicSupportAnimator,
            Transform cinematicSupportAnchor,
            ActionCinematicCueProfile cinematicCueProfile)
        {
            ActionCinematicCueDirector cinematicCueDirector =
                ConfigureActionCinematicCueDirector(
                    cameraController,
                    movement.transform,
                    actionController,
                    movement,
                    skill1Action,
                    summonSlot1Action,
                    rangedBasicAttackAction,
                    playerCuePlayer,
                    cueAnimator,
                    cinematicSupportAnimator,
                    cinematicSupportAnchor,
                    cinematicCueProfile);
            ActionCameraCueDriver cueDriver = EnsureComponent<ActionCameraCueDriver>(cameraController.gameObject);
            SetBehaviourEnabled(cueDriver, true);
            SetObjectReference(cueDriver, "actionController", actionController);
            SetObjectReference(cueDriver, "movement", movement);
            SetObjectReference(cueDriver, "skill1Action", skill1Action);
            SetObjectReference(cueDriver, "summonSlot1Action", summonSlot1Action);
            SetObjectReference(cueDriver, "cameraController", cameraController);
            SetObjectReference(cueDriver, "cinematicCueDirector", cinematicCueDirector);
            SetObjectReference(cueDriver, "cueSpace", movement.transform);
        }

        private static ActionCinematicCueDirector ConfigureActionCinematicCueDirector(
            ActionCameraController cameraController,
            Transform cueSpace,
            PlayerActionController actionController,
            PlayerMovementController movement,
            PlayerSkill1Action skill1Action,
            PlayerSummonSlot1Action summonSlot1Action,
            PlayerRangedBasicAttackAction rangedBasicAttackAction,
            CombatVfxCuePlayer playerCuePlayer,
            Animator cueAnimator,
            Animator cinematicSupportAnimator,
            Transform cinematicSupportAnchor,
            ActionCinematicCueProfile cinematicCueProfile)
        {
            ActionCinematicCueDirector cueDirector =
                EnsureComponent<ActionCinematicCueDirector>(cameraController.gameObject);
            SetBehaviourEnabled(cueDirector, true);
            SetObjectReference(cueDirector, "cueProfile", cinematicCueProfile);
            SetObjectReference(cueDirector, "cameraController", cameraController);
            SetObjectReference(cueDirector, "cueSpace", cueSpace);
            SetObjectReference(cueDirector, "movement", movement);
            SetObjectReference(cueDirector, "actionController", actionController);
            SetObjectReference(cueDirector, "skill1Action", skill1Action);
            SetObjectReference(cueDirector, "summonSlot1Action", summonSlot1Action);
            SetObjectReference(cueDirector, "rangedBasicAttackAction", rangedBasicAttackAction);
            SetObjectReference(cueDirector, "cuePlayer", playerCuePlayer);
            SetObjectReference(cueDirector, "vfxAnchor", cueSpace);
            SetObjectReference(cueDirector, "cueAnimator", cueAnimator);
            CinematicSequenceRunner cinematicSequenceRunner =
                ConfigureBuildResubmissionCinematicSequenceRunner(
                    cameraController,
                    cueSpace,
                    playerCuePlayer,
                    cueAnimator,
                    cinematicSupportAnimator,
                    cinematicSupportAnchor);
            ActionCinematicSequenceBridge sequenceBridge =
                ConfigureBuildResubmissionActionCinematicSequenceBridge(
                    cameraController,
                    cinematicSequenceRunner);
            SetObjectReference(cueDirector, "sequenceBridge", sequenceBridge);
            SetBool(cueDirector, "allowCuePlayback", true);
            SetBool(cueDirector, "allowSequenceBridgePlayback", false);
            SetBool(cueDirector, "useUnscaledClock", true);
            SetBool(cueDirector, "drawCinematicBars", false);
            SetFloat(cueDirector, "maxBarScreenRatio", 0.085f);
            SetFloat(cueDirector, "maxBarAlpha", 0.62f);
            EditorUtility.SetDirty(cueDirector);
            return cueDirector;
        }

        private static CinematicSequenceRunner ConfigureBuildResubmissionCinematicSequenceRunner(
            ActionCameraController cameraController,
            Transform cueSpace,
            CombatVfxCuePlayer playerCuePlayer,
            Animator cueAnimator,
            Animator cinematicSupportAnimator,
            Transform cinematicSupportAnchor)
        {
            CinematicSequenceRunner runner = EnsureComponent<CinematicSequenceRunner>(cameraController.gameObject);
            SetBehaviourEnabled(runner, false);
            CinematicTutorialPromptPresenter promptPresenter =
                EnsureComponent<CinematicTutorialPromptPresenter>(cameraController.gameObject);
            SetBehaviourEnabled(promptPresenter, false);
            Camera camera = cameraController.GetComponent<Camera>();
            if (camera != null)
            {
                SetObjectReference(promptPresenter, "targetCamera", camera);
            }

            CinematicBlendShapeExpressionPlayer expressionPlayer = null;
            if (cueAnimator != null)
            {
                expressionPlayer =
                    EnsureComponent<CinematicBlendShapeExpressionPlayer>(cueAnimator.gameObject);
                expressionPlayer.Configure(CreateBuildResubmissionInoriExpressionPresets());
                EditorUtility.SetDirty(expressionPlayer);
            }

            SerializedObject serializedRunner = new SerializedObject(runner);
            RequireProperty(serializedRunner, "sequenceProfile").objectReferenceValue =
                LoadAsset<CinematicSequenceProfile>(CinematicUltimateProfilePath);
            RequireProperty(serializedRunner, "bodyControllerOverride").objectReferenceValue =
                LoadAsset<RuntimeAnimatorController>(InoriCinematicAnimatorControllerPath);
            RequireProperty(serializedRunner, "cameraController").objectReferenceValue = cameraController;
            RequireProperty(serializedRunner, "cinematicCamera").objectReferenceValue = camera;
            RequireProperty(serializedRunner, "driveCameraTransformFromProfile").boolValue = true;
            RequireProperty(serializedRunner, "disableActionCameraControllerDuringPoseDrive").boolValue = true;
            RequireProperty(serializedRunner, "combatVfxCuePlayer").objectReferenceValue = playerCuePlayer;
            RequireProperty(serializedRunner, "tutorialPromptPresenter").objectReferenceValue = promptPresenter;
            RequireProperty(serializedRunner, "cueSpace").objectReferenceValue = cueSpace;

            SerializedProperty bindings = RequireProperty(serializedRunner, "actorBindings");
            bindings.arraySize = cinematicSupportAnimator != null ? 2 : 1;
            SerializedProperty binding = bindings.GetArrayElementAtIndex(0);
            binding.FindPropertyRelative("role").enumValueIndex =
                (int)CinematicSequenceProfile.ActorRole.Inori;
            binding.FindPropertyRelative("bodyAnimator").objectReferenceValue = cueAnimator;
            binding.FindPropertyRelative("faceAnimator").objectReferenceValue = null;
            binding.FindPropertyRelative("expressionPlayer").objectReferenceValue = expressionPlayer;
            binding.FindPropertyRelative("anchor").objectReferenceValue =
                cueAnimator != null ? cueAnimator.transform : cueSpace;
            if (cinematicSupportAnimator != null)
            {
                SerializedProperty supportBinding = bindings.GetArrayElementAtIndex(1);
                supportBinding.FindPropertyRelative("role").enumValueIndex =
                    (int)CinematicSequenceProfile.ActorRole.Environment;
                supportBinding.FindPropertyRelative("bodyAnimator").objectReferenceValue = cinematicSupportAnimator;
                supportBinding.FindPropertyRelative("faceAnimator").objectReferenceValue = null;
                supportBinding.FindPropertyRelative("expressionPlayer").objectReferenceValue = null;
                supportBinding.FindPropertyRelative("anchor").objectReferenceValue =
                    cinematicSupportAnchor != null ? cinematicSupportAnchor : cinematicSupportAnimator.transform;
            }

            serializedRunner.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(promptPresenter);
            EditorUtility.SetDirty(runner);
            return runner;
        }

        private static ActionCinematicSequenceBridge ConfigureBuildResubmissionActionCinematicSequenceBridge(
            ActionCameraController cameraController,
            CinematicSequenceRunner runner)
        {
            ActionCinematicSequenceBridge bridge =
                EnsureComponent<ActionCinematicSequenceBridge>(cameraController.gameObject);
            SetBehaviourEnabled(bridge, false);
            SetObjectReference(bridge, "runner", runner);
            SetBool(bridge, "blockLegacyCameraShotsWhenPlayed", true);
            SetBool(bridge, "blockLegacySignalsWhenPlayed", true);
            SetFloat(bridge, "minimumLockSeconds", 0.12f);
            SetObjectReference(bridge, "skillCutInProfile", null);
            SetObjectReference(bridge, "summonEntryProfile", LoadAsset<CinematicSequenceProfile>(CinematicSummonProfilePath));
            SetObjectReference(bridge, "ultimateCutInProfile", LoadAsset<CinematicSequenceProfile>(CinematicUltimateProfilePath));
            SetObjectReference(bridge, "bossPressureBreakProfile", LoadAsset<CinematicSequenceProfile>(CinematicBossSummonPressureProfilePath));
            SetObjectReference(bridge, "summonFollowupHitProfile", LoadAsset<CinematicSequenceProfile>(CinematicSummonFollowupProfilePath));
            SetObjectReference(bridge, "summonEmpowerProfile", LoadAsset<CinematicSequenceProfile>(CinematicSummonEmpowerProfilePath));
            SetObjectReference(bridge, "summonRecallProfile", LoadAsset<CinematicSequenceProfile>(CinematicSummonRecallProfilePath));
            SetObjectReference(bridge, "pocketClearProfile", LoadAsset<CinematicSequenceProfile>(CinematicResultProfilePath));
            SetObjectReference(bridge, "pocketFailProfile", LoadAsset<CinematicSequenceProfile>(CinematicDangerProfilePath));
            SetObjectReference(bridge, "bossIntroProfile", LoadAsset<CinematicSequenceProfile>(CinematicBossIntroProfilePath));
            SetObjectReference(bridge, "phaseTransitionProfile", LoadAsset<CinematicSequenceProfile>(CinematicPhaseTransitionProfilePath));
            SetObjectReference(
                bridge,
                "dialogueReactionBeatProfile",
                LoadAsset<CinematicSequenceProfile>(CinematicDialogueReactionBeatProfilePath));
            EditorUtility.SetDirty(bridge);
            return bridge;
        }

        private static CinematicBlendShapeExpressionPlayer.ExpressionPreset[]
            CreateBuildResubmissionInoriExpressionPresets()
        {
            return new[]
            {
                Preset("Surprised",
                    Shape("browInnerUpSurprised", 80f),
                    Shape("vrc.v_oh", 70f),
                    Shape("eyeWideRight", 82f),
                    Shape("eyeWideLeft", 82f),
                    Shape("jawOpen", 48f),
                    Shape("mouthStretchLeft", 24f),
                    Shape("mouthStretchRight", 24f)),
                Preset("Confused",
                    Shape("browInnerUpSurprised", 46f),
                    Shape("vrc.v_ou", 42f),
                    Shape("jawOpen", 15f)),
                Preset("Angry",
                    Shape("browDownRight", 70f),
                    Shape("browDownLeft", 70f),
                    Shape("noseSneerRight", 42f),
                    Shape("noseSneerLeft", 42f),
                    Shape("mouthFrownRight", 56f),
                    Shape("mouthFrownLeft", 56f)),
                Preset("CalmEye",
                    Shape("eyeBlinkRight", 14f),
                    Shape("eyeBlinkLeft", 14f)),
                Preset("Smile",
                    Shape("mouthSmileRight", 64f),
                    Shape("mouthSmileLeft", 64f),
                    Shape("eyeBlinkRight", 10f),
                    Shape("eyeBlinkLeft", 10f)),
                Preset("Joy",
                    Shape("eyeBlinkRight", 28f),
                    Shape("eyeBlinkLeft", 28f),
                    Shape("cheekSquintRight", 42f),
                    Shape("cheekSquintLeft", 42f),
                    Shape("mouthSmileRight", 78f),
                    Shape("mouthSmileLeft", 78f))
            };
        }

        private static CinematicBlendShapeExpressionPlayer.ExpressionPreset Preset(
            string expressionName,
            params CinematicBlendShapeExpressionPlayer.ShapeWeight[] shapes)
        {
            return new CinematicBlendShapeExpressionPlayer.ExpressionPreset(expressionName, shapes);
        }

        private static CinematicBlendShapeExpressionPlayer.ShapeWeight Shape(string shapeName, float weight)
        {
            return new CinematicBlendShapeExpressionPlayer.ShapeWeight(shapeName, weight);
        }

        private static void ConfigureBossBarrageCameraCueDriver(
            ActionCameraController cameraController,
            BossBarrageEmitter bossBarrageEmitter,
            BossPressureActionDirector bossPressureActionDirector,
            Transform cueSpace)
        {
            BossBarrageCameraCueDriver cueDriver = EnsureComponent<BossBarrageCameraCueDriver>(cameraController.gameObject);
            SetBehaviourEnabled(cueDriver, true);
            cueDriver.Configure(bossBarrageEmitter, cameraController, cueSpace, bossPressureActionDirector);
            SetObjectReference(cueDriver, "bossBarrageEmitter", bossBarrageEmitter);
            SetObjectReference(cueDriver, "bossPressureActionDirector", bossPressureActionDirector);
            SetObjectReference(cueDriver, "cameraController", cameraController);
            SetObjectReference(cueDriver, "cueSpace", cueSpace);
            EditorUtility.SetDirty(cueDriver);
        }

        private static void ValidateFixedRearCamera(
            ActionCameraController cameraController,
            Transform player,
            Transform rearYawReference)
        {
            Vector3 planarOffset = Vector3.ProjectOnPlane(cameraController.transform.position - player.position, Vector3.up);
            if (Vector3.Dot(player.forward, planarOffset) >= -0.1f)
            {
                throw new InvalidOperationException("Boss barrage lane camera should start behind the player.");
            }

            ValidateBool(cameraController, "useFixedRearYaw", true);
            ValidateObjectReference(cameraController, "fixedRearYawReference", rearYawReference);
            ValidateBool(cameraController, "useDeviceFallbackWhenActionMissing", false);
            ValidateBool(cameraController, "aimOrbitUsesInput", true);
            ValidateBool(cameraController, "aimOrbitRotatesCameraPosition", true);
            ValidateFloat(cameraController, "aimOrbitYawLimitDegrees", CameraAimYawLimitDegrees);
            ValidateBool(cameraController, "aimOrbitUsesPitchInput", true);
            ValidateFloat(cameraController, "aimOrbitPitchLimitDegrees", CameraAimPitchLimitDegrees);
            ValidateBool(cameraController, "aimAssistUsesYawTarget", false);
            ValidateFloat(cameraController, "aimAssistMaxYawBlend", 0.85f);
            ValidateFloat(cameraController, "aimAssistYawSpeedDegrees", 420f);
            ValidateFloat(cameraController, "aimAssistYawReturnSpeedDegrees", 520f);
        }

        private static void ValidateRangedAimController(
            PlayerRangedAimController aimController,
            PlayerCombatModeController combatModeController,
            ActionCameraController cameraController,
            Animator rangedAnimator)
        {
            ValidateObjectReference(aimController, "combatModeController", combatModeController);
            ValidateObjectReference(aimController, "cameraController", cameraController);
            ValidateObjectReference(aimController, "movement", combatModeController.GetComponent<PlayerMovementController>());
            ValidateObjectReference(aimController, "animator", rangedAnimator);
            ValidateBool(aimController, "holdToAim", true);
            ValidateBool(aimController, "allowMouseAimFallback", false);
            ValidateBool(aimController, "faceCameraForwardWhileAiming", true);
            ValidateString(aimController, "aimingParameter", string.Empty);
        }

        private static void ValidatePlayerRangedBasicAttack(
            PlayerRangedBasicAttackAction rangedBasicAttackAction,
            PlayerCombatModeController combatModeController,
            PlayerRangedAimController rangedAimController,
            PlayerMovementController movement,
            PlayerCombatTargetSelector targetSelector,
            CombatHealth playerHealth,
            ActionCameraController cameraController,
            Animator rangedAnimator,
            Transform projectileRoot,
            Transform fireOrigin)
        {
            ValidateObjectReference(rangedBasicAttackAction, "combatModeController", combatModeController);
            ValidateObjectReference(rangedBasicAttackAction, "aimController", rangedAimController);
            ValidateObjectReference(rangedBasicAttackAction, "movement", movement);
            ValidateObjectReference(rangedBasicAttackAction, "targetSelector", targetSelector);
            ValidateObjectReference(rangedBasicAttackAction, "sourceHealth", playerHealth);
            ValidateObjectReference(rangedBasicAttackAction, "cameraController", cameraController);
            ValidateObjectReference(rangedBasicAttackAction, "animator", rangedAnimator);
            ValidateObjectReference(
                rangedBasicAttackAction,
                "projectilePrefabObject",
                LoadAsset<GameObject>(RangedBasicProjectilePrefabPath));
            ValidateObjectReference(rangedBasicAttackAction, "projectileRoot", projectileRoot);
            ValidateObjectReference(rangedBasicAttackAction, "fireOrigin", fireOrigin);
            ValidateEnum(rangedBasicAttackAction, "sourceTeam", (int)DamageTeam.Player);
            ValidateInt(rangedBasicAttackAction, "prewarmCount", PlayerRangedBasicPrewarmCount);
            ValidateBool(rangedBasicAttackAction, "allowMouseFireFallback", false);
            ValidateBool(rangedBasicAttackAction, "requestFacingOnFire", false);
            ValidateBool(rangedBasicAttackAction, "snapFacingOnFire", false);
            ValidateBool(rangedBasicAttackAction, "suppressFacingOnFireWhileMoving", true);
            ValidateFloat(rangedBasicAttackAction, "movingFacingSuppressSpeed", 0.08f);
            ValidateBool(rangedBasicAttackAction, "useFixedCenterAimViewport", true);
            ValidateBool(rangedBasicAttackAction, "useStableAimOrigin", true);
            ValidateBool(rangedBasicAttackAction, "cameraAimIgnoresNonTargetHits", true);
            ValidateBool(rangedBasicAttackAction, "stabilizeDirectTargetAimHeight", false);
            ValidateBool(rangedBasicAttackAction, "useAimAssist", true);
            ValidateBool(rangedBasicAttackAction, "disableAimAssistWithManualInput", false);
            ValidateBool(rangedBasicAttackAction, "driveCameraAimAssist", false);
            ValidateBool(rangedBasicAttackAction, "useMagazineReload", true);
            ValidateInt(rangedBasicAttackAction, "magazineSize", PlayerRangedBasicMagazineSize);
            ValidateBool(rangedBasicAttackAction, "autoReloadWhenEmpty", true);
            ValidateBool(rangedBasicAttackAction, "reloadWhenAimReleased", true);
            ValidateBool(rangedBasicAttackAction, "cancelAimReleaseReloadOnAimResume", true);
            ValidateFloat(rangedBasicAttackAction, "damage", PlayerRangedBasicDamage);
            ValidateFloat(rangedBasicAttackAction, "projectileSpeed", PlayerRangedBasicProjectileSpeed);
            ValidateFloat(rangedBasicAttackAction, "projectileLifetimeSeconds", PlayerRangedBasicProjectileLifetimeSeconds);
            ValidateFloat(rangedBasicAttackAction, "projectileRadius", PlayerRangedBasicProjectileRadius);
            ValidateFloat(rangedBasicAttackAction, "fireIntervalSeconds", PlayerRangedBasicFireIntervalSeconds);
            ValidateFloat(rangedBasicAttackAction, "targetHeight", PlayerRangedBasicTargetHeight);
            ValidateFloat(rangedBasicAttackAction, "cameraAimFallbackDistance", PlayerRangedBasicCameraAimFallbackDistance);
            ValidateFloat(rangedBasicAttackAction, "cameraAimRaycastDistance", PlayerRangedBasicCameraAimRaycastDistance);
            ValidateFloat(rangedBasicAttackAction, "aimAssistDistance", PlayerRangedBasicAimAssistDistance);
            ValidateFloat(rangedBasicAttackAction, "hipAimAssistAngleDegrees", PlayerRangedBasicAimAssistAngleDegrees);
            ValidateFloat(rangedBasicAttackAction, "aimedAimAssistAngleDegrees", PlayerRangedBasicAimAssistAngleDegrees);
            ValidateFloat(rangedBasicAttackAction, "aimAssistMaxTurnDegrees", PlayerRangedBasicAimAssistAngleDegrees);
            ValidateFloat(rangedBasicAttackAction, "cameraAimAssistStrengthScale", 1f);
            ValidateFloat(rangedBasicAttackAction, "cameraAimAssistMinStrength", 0.05f);
            ValidateString(rangedBasicAttackAction, "fireTrigger", string.Empty);
        }

        private static void ValidateActionCameraCueDriver(
            ActionCameraCueDriver cueDriver,
            PlayerActionController actionController,
            PlayerMovementController movement,
            ActionCameraController cameraController,
            PlayerSkill1Action skill1Action,
            PlayerSummonSlot1Action summonSlot1Action,
            ActionCinematicCueDirector cinematicCueDirector)
        {
            ValidateBehaviourEnabled(cueDriver, true);
            ValidateObjectReference(cueDriver, "actionController", actionController);
            ValidateObjectReference(cueDriver, "movement", movement);
            ValidateObjectReference(cueDriver, "skill1Action", skill1Action);
            ValidateObjectReference(cueDriver, "summonSlot1Action", summonSlot1Action);
            ValidateObjectReference(cueDriver, "cameraController", cameraController);
            ValidateObjectReference(cueDriver, "cinematicCueDirector", cinematicCueDirector);
            ValidateObjectReference(cueDriver, "cueSpace", movement.transform);
        }

        private static void ValidateActionCinematicCueDirector(
            ActionCinematicCueDirector cueDirector,
            ActionCameraController cameraController,
            Transform cueSpace,
            PlayerMovementController movement,
            PlayerActionController actionController,
            PlayerSkill1Action skill1Action,
            PlayerSummonSlot1Action summonSlot1Action,
            PlayerRangedBasicAttackAction rangedBasicAttackAction,
            CombatVfxCuePlayer playerCuePlayer,
            Animator cueAnimator,
            Animator cinematicSupportAnimator,
            Transform cinematicSupportAnchor)
        {
            ActionCinematicCueProfile profile =
                LoadAsset<ActionCinematicCueProfile>(ActionFoundationProfileSetup.CinematicCueProfilePath);
            ValidateBehaviourEnabled(cueDirector, true);
            ValidateObjectReference(cueDirector, "cueProfile", profile);
            ValidateObjectReference(cueDirector, "cameraController", cameraController);
            ValidateObjectReference(cueDirector, "cueSpace", cueSpace);
            ValidateObjectReference(cueDirector, "movement", movement);
            ValidateObjectReference(cueDirector, "actionController", actionController);
            ValidateObjectReference(cueDirector, "skill1Action", skill1Action);
            ValidateObjectReference(cueDirector, "summonSlot1Action", summonSlot1Action);
            ValidateObjectReference(cueDirector, "rangedBasicAttackAction", rangedBasicAttackAction);
            ValidateObjectReference(cueDirector, "cuePlayer", playerCuePlayer);
            ValidateObjectReference(cueDirector, "vfxAnchor", cueSpace);
            ValidateObjectReference(cueDirector, "cueAnimator", cueAnimator);
            CinematicSequenceRunner cinematicSequenceRunner =
                RequireComponent<CinematicSequenceRunner>(
                    cueDirector.gameObject,
                    "build-resubmission cinematic sequence runner");
            ActionCinematicSequenceBridge sequenceBridge =
                RequireComponent<ActionCinematicSequenceBridge>(
                    cueDirector.gameObject,
                    "build-resubmission action cinematic sequence bridge");
            ValidateObjectReference(cueDirector, "sequenceBridge", sequenceBridge);
            ValidateBool(cueDirector, "allowCuePlayback", true);
            ValidateBool(cueDirector, "allowSequenceBridgePlayback", false);
            ValidateBool(cueDirector, "drawCinematicBars", false);
            ValidateBehaviourEnabled(cinematicSequenceRunner, false);
            ValidateObjectReference(
                cinematicSequenceRunner,
                "sequenceProfile",
                LoadAsset<CinematicSequenceProfile>(CinematicUltimateProfilePath));
            ValidateObjectReference(
                cinematicSequenceRunner,
                "bodyControllerOverride",
                LoadAsset<RuntimeAnimatorController>(InoriCinematicAnimatorControllerPath));
            ValidateObjectReference(cinematicSequenceRunner, "cameraController", cameraController);
            ValidateObjectReference(cinematicSequenceRunner, "cinematicCamera", cameraController.GetComponent<Camera>());
            ValidateBool(cinematicSequenceRunner, "driveCameraTransformFromProfile", true);
            ValidateBool(cinematicSequenceRunner, "disableActionCameraControllerDuringPoseDrive", true);
            ValidateObjectReference(cinematicSequenceRunner, "combatVfxCuePlayer", playerCuePlayer);
            CinematicTutorialPromptPresenter promptPresenter =
                ValidateAssignedObjectReference<CinematicTutorialPromptPresenter>(
                    cinematicSequenceRunner,
                    "tutorialPromptPresenter");
            ValidateBehaviourEnabled(promptPresenter, false);
            ValidateObjectReference(cinematicSequenceRunner, "cueSpace", cueSpace);
            ValidateRunnerActorBinding(
                cinematicSequenceRunner,
                CinematicSequenceProfile.ActorRole.Environment,
                cinematicSupportAnimator,
                cinematicSupportAnchor);
            ValidateBehaviourEnabled(sequenceBridge, false);
            ValidateObjectReference(sequenceBridge, "runner", cinematicSequenceRunner);
            ValidateBool(sequenceBridge, "blockLegacyCameraShotsWhenPlayed", true);
            ValidateBool(sequenceBridge, "blockLegacySignalsWhenPlayed", true);
            ValidateObjectReference(sequenceBridge, "skillCutInProfile", null);
            ValidateObjectReference(
                sequenceBridge,
                "summonEntryProfile",
                LoadAsset<CinematicSequenceProfile>(CinematicSummonProfilePath));
            ValidateObjectReference(
                sequenceBridge,
                "ultimateCutInProfile",
                LoadAsset<CinematicSequenceProfile>(CinematicUltimateProfilePath));
            ValidateObjectReference(
                sequenceBridge,
                "bossPressureBreakProfile",
                LoadAsset<CinematicSequenceProfile>(CinematicBossSummonPressureProfilePath));
            ValidateObjectReference(
                sequenceBridge,
                "summonFollowupHitProfile",
                LoadAsset<CinematicSequenceProfile>(CinematicSummonFollowupProfilePath));
            ValidateObjectReference(
                sequenceBridge,
                "summonEmpowerProfile",
                LoadAsset<CinematicSequenceProfile>(CinematicSummonEmpowerProfilePath));
            ValidateObjectReference(
                sequenceBridge,
                "summonRecallProfile",
                LoadAsset<CinematicSequenceProfile>(CinematicSummonRecallProfilePath));
            ValidateObjectReference(
                sequenceBridge,
                "pocketClearProfile",
                LoadAsset<CinematicSequenceProfile>(CinematicResultProfilePath));
            ValidateObjectReference(
                sequenceBridge,
                "pocketFailProfile",
                LoadAsset<CinematicSequenceProfile>(CinematicDangerProfilePath));
            ValidateObjectReference(
                sequenceBridge,
                "bossIntroProfile",
                LoadAsset<CinematicSequenceProfile>(CinematicBossIntroProfilePath));
            ValidateObjectReference(
                sequenceBridge,
                "phaseTransitionProfile",
                LoadAsset<CinematicSequenceProfile>(CinematicPhaseTransitionProfilePath));
            ValidateObjectReference(
                sequenceBridge,
                "dialogueReactionBeatProfile",
                LoadAsset<CinematicSequenceProfile>(CinematicDialogueReactionBeatProfilePath));
            ValidateAssignedObjectReference(cueAnimator, "m_Controller");
            CinematicBlendShapeExpressionPlayer expressionPlayer =
                RequireComponent<CinematicBlendShapeExpressionPlayer>(
                    cueAnimator.gameObject,
                    "build-resubmission Inori expression player");
            SerializedProperty presets =
                RequireProperty(new SerializedObject(expressionPlayer), "presets");
            if (presets.arraySize < 6)
            {
                throw new InvalidOperationException("Build-resubmission Inori expression player must expose at least six presets.");
            }

            ValidateBool(cueDirector, "useUnscaledClock", true);
            ValidateBool(cueDirector, "drawCinematicBars", false);
            ValidateFloat(cueDirector, "maxBarScreenRatio", 0.085f);
            ValidateFloat(cueDirector, "maxBarAlpha", 0.62f);
            ValidateCinematicCueContract(
                profile.SkillCutIn,
                "SkillCutIn",
                ActionCinematicCueProfile.CueTier.CombatCue,
                cueAnimator);
            ValidateCinematicCueContract(
                profile.SummonEntry,
                "SummonEntry",
                ActionCinematicCueProfile.CueTier.MicroCinematic,
                cueAnimator);
            ValidateCinematicCueContract(
                profile.UltimateCutIn,
                "UltimateCutIn",
                ActionCinematicCueProfile.CueTier.CombatCutIn,
                cueAnimator);
            ValidateCinematicCueContract(
                profile.BossPressureBreak,
                "BossPressureBreak",
                ActionCinematicCueProfile.CueTier.MicroCinematic,
                cueAnimator);
            ValidateCinematicCueContract(
                profile.SummonFollowupHit,
                "SummonFollowupHit",
                ActionCinematicCueProfile.CueTier.MicroCinematic,
                cueAnimator);
            ValidateCinematicCueContract(
                profile.SummonEmpower,
                "SummonEmpower",
                ActionCinematicCueProfile.CueTier.MicroCinematic,
                cueAnimator);
            ValidateCinematicCueContract(
                profile.SummonRecall,
                "SummonRecall",
                ActionCinematicCueProfile.CueTier.MicroCinematic,
                cueAnimator);
            ValidateCinematicCueContract(
                profile.PocketClear,
                "PocketClear",
                ActionCinematicCueProfile.CueTier.MicroCinematic,
                cueAnimator);
            ValidateCinematicCueContract(
                profile.PocketFail,
                "PocketFail",
                ActionCinematicCueProfile.CueTier.MicroCinematic,
                cueAnimator);
            ValidateCinematicCueContract(
                profile.BossIntro,
                "BossIntro",
                ActionCinematicCueProfile.CueTier.CombatCutIn,
                cueAnimator);
            ValidateCinematicCueContract(
                profile.PhaseTransition,
                "PhaseTransition",
                ActionCinematicCueProfile.CueTier.CombatCutIn,
                cueAnimator);
            ValidateCinematicCueContract(
                profile.DialogueReactionBeat,
                "DialogueReactionBeat",
                ActionCinematicCueProfile.CueTier.MicroCinematic,
                cueAnimator);
            if (!profile.TryGetSequence(ActionCinematicCueProfile.CueKind.SummonEntry, out var summonEntry)
                || summonEntry.ShotCount < 3)
            {
                throw new InvalidOperationException("Action cinematic profile must author a multi-shot summon entry cut-in.");
            }

            if (summonEntry.movementLockSeconds < 0.4f || summonEntry.inputLockSeconds < 0.5f)
            {
                throw new InvalidOperationException("Summon entry cut-in must author a short movement/input lock.");
            }

            if (summonEntry.SignalCount < 2)
            {
                throw new InvalidOperationException("Summon entry cut-in must author spawn and landing presentation signals.");
            }

            if (!profile.TryGetSequence(ActionCinematicCueProfile.CueKind.UltimateCutIn, out var ultimateCutIn)
                || ultimateCutIn.ShotCount < 3)
            {
                throw new InvalidOperationException("Action cinematic profile must author a multi-shot ultimate-style cut-in.");
            }

            if (ultimateCutIn.inputLockSeconds < 0.6f || ultimateCutIn.SignalCount < 2)
            {
                throw new InvalidOperationException("Ultimate-style cut-in must author lock and charge/impact signals.");
            }
        }

        private static void ValidateRunnerActorBinding(
            CinematicSequenceRunner runner,
            CinematicSequenceProfile.ActorRole expectedRole,
            Animator expectedBodyAnimator,
            Transform expectedAnchor)
        {
            SerializedProperty bindings = RequireProperty(new SerializedObject(runner), "actorBindings");
            for (int i = 0; i < bindings.arraySize; i++)
            {
                SerializedProperty binding = bindings.GetArrayElementAtIndex(i);
                if (binding.FindPropertyRelative("role").enumValueIndex != (int)expectedRole)
                {
                    continue;
                }

                UnityEngine.Object bodyAnimator =
                    binding.FindPropertyRelative("bodyAnimator").objectReferenceValue;
                UnityEngine.Object anchor =
                    binding.FindPropertyRelative("anchor").objectReferenceValue;
                if (bodyAnimator != expectedBodyAnimator)
                {
                    throw new InvalidOperationException(
                        $"Cinematic runner {runner.name} binds {expectedRole} to {bodyAnimator}, expected {expectedBodyAnimator}.");
                }

                if (anchor != expectedAnchor)
                {
                    throw new InvalidOperationException(
                        $"Cinematic runner {runner.name} binds {expectedRole} anchor to {anchor}, expected {expectedAnchor}.");
                }

                return;
            }

            throw new InvalidOperationException(
                $"Cinematic runner {runner.name} is missing an actor binding for {expectedRole}.");
        }

        private static void ValidateCinematicCueContract(
            ActionCinematicCueProfile.CueSequence sequence,
            string label,
            ActionCinematicCueProfile.CueTier expectedTier,
            Animator cueAnimator)
        {
            if (!sequence.enabled)
            {
                throw new InvalidOperationException($"{label} cinematic cue must stay enabled in the review profile.");
            }

            if (sequence.tier != expectedTier)
            {
                throw new InvalidOperationException($"{label} cinematic cue must declare tier {expectedTier}.");
            }

            if (!string.Equals(
                    sequence.returnTargetId,
                    ActionCinematicCueProfile.GameplayReturnTargetId,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"{label} cinematic cue must declare gameplay camera return target.");
            }

            if (sequence.returnPolicy != ActionCinematicCueProfile.CameraReturnPolicy.ActionCameraCueRecovery)
            {
                throw new InvalidOperationException($"{label} cinematic cue must return through the action camera recovery policy.");
            }

            if (sequence.signals == null)
            {
                return;
            }

            for (int i = 0; i < sequence.signals.Length; i++)
            {
                ActionCinematicCueProfile.CueSignal signal = sequence.signals[i];
                if (!signal.enabled)
                {
                    continue;
                }

                if (signal.tierIntensityScale <= 0f)
                {
                    throw new InvalidOperationException($"{label} signal {i} must author a positive tier intensity scale.");
                }

                if (signal.requireAnimatorTrigger && !HasAnimatorTrigger(cueAnimator, signal.animatorTrigger))
                {
                    throw new InvalidOperationException(
                        $"{label} signal {i} requires missing Animator trigger '{signal.animatorTrigger}'.");
                }
            }
        }

        private static bool HasAnimatorTrigger(Animator animator, string triggerName)
        {
            if (animator == null || string.IsNullOrWhiteSpace(triggerName))
            {
                return false;
            }

            AnimatorControllerParameter[] parameters = animator.parameters;
            for (int i = 0; i < parameters.Length; i++)
            {
                AnimatorControllerParameter parameter = parameters[i];
                if (parameter.type == AnimatorControllerParameterType.Trigger
                    && string.Equals(parameter.name, triggerName, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static void ValidateBossBarrageCameraCueDriver(
            BossBarrageCameraCueDriver cueDriver,
            ActionCameraController cameraController,
            BossBarrageEmitter bossBarrageEmitter,
            Transform cueSpace)
        {
            ValidateBehaviourEnabled(cueDriver, true);
            ValidateObjectReference(cueDriver, "bossBarrageEmitter", bossBarrageEmitter);
            ValidateObjectReference(
                cueDriver,
                "bossPressureActionDirector",
                RequireComponent<BossPressureActionDirector>(bossBarrageEmitter.gameObject, "boss pressure action director"));
            ValidateObjectReference(cueDriver, "cameraController", cameraController);
            ValidateObjectReference(cueDriver, "cueSpace", cueSpace);
        }

        private static void ValidateBossBarrageLaneTelegraphPresenter(
            BossBarrageLaneTelegraphPresenter presenter,
            BossBarrageEmitter bossBarrageEmitter,
            SummonLaneSpace laneSpace)
        {
            if (presenter.gameObject.activeSelf)
            {
                throw new InvalidOperationException("Boss barrage telegraph marker root should stay inactive during the VFX cleanup pass.");
            }

            ValidateObjectReference(presenter, "bossBarrageEmitter", bossBarrageEmitter);
            ValidateObjectReference(presenter, "laneSpace", laneSpace);
            ValidateAssignedObjectReference(presenter, "markerRoot");

            if (presenter.MarkerCount < 9)
            {
                throw new InvalidOperationException("Boss barrage lane telegraph presenter should own nine authored marker slots.");
            }

            for (int i = 0; i < 9; i++)
            {
                ValidateArrayAssignedReference(presenter, "markerTransforms", i);
                Renderer renderer = ValidateArrayAssignedReference<Renderer>(presenter, "markerRenderers", i);
                ValidateGameOwnedAsset(renderer.sharedMaterial, $"boss barrage telegraph marker {i} material");
            }
        }

        private static void ValidateBossBarrageProjectilePrefab()
        {
            GameObject projectilePrefab = LoadAsset<GameObject>(ProjectilePrefabPath);
            MeshRenderer renderer = projectilePrefab.GetComponent<MeshRenderer>();
            if (renderer == null)
            {
                throw new InvalidOperationException("Boss barrage projectile prefab should keep a hidden root MeshRenderer for runtime presentation state.");
            }

            if (renderer.enabled)
            {
                throw new InvalidOperationException("Boss barrage projectile root MeshRenderer must stay hidden behind the promoted Forge3D missile VFX.");
            }

            MeshFilter meshFilter = projectilePrefab.GetComponent<MeshFilter>();
            if (meshFilter == null)
            {
                throw new InvalidOperationException("Boss barrage projectile prefab should include a hidden root MeshFilter for stable runtime scaling.");
            }

            ValidateExactAssetReference(
                meshFilter.sharedMesh,
                "boss barrage projectile hidden primitive mesh",
                LoadPrimitiveMesh(PrimitiveType.Sphere));
            ValidateExactAssetReference(
                renderer.sharedMaterial,
                "boss barrage projectile material",
                LoadAsset<Material>(ProjectileMaterialPath));
            ValidateGameOwnedAsset(renderer.sharedMaterial, "boss barrage projectile material");
            ValidateRenderableMaterialShader(renderer.sharedMaterial, "boss barrage projectile material shader");

            BossBarrageProjectile projectile = projectilePrefab.GetComponent<BossBarrageProjectile>();
            SerializedObject projectileObject = new SerializedObject(projectile);
            SerializedProperty visualRenderers = RequireProperty(projectileObject, "visualRenderers");
            if (!visualRenderers.isArray
                || visualRenderers.arraySize != 1
                || visualRenderers.GetArrayElementAtIndex(0).objectReferenceValue != renderer)
            {
                throw new InvalidOperationException(
                    "Boss barrage projectile should runtime-tint only the hidden root renderer, not the authored Forge3D missile materials.");
            }

            if (projectilePrefab.GetComponent<TrailRenderer>() != null)
            {
                throw new InvalidOperationException("Boss barrage projectile should not fall back to generated TrailRenderer visuals.");
            }

            ValidatePromotedForge3DMissileVfx(
                projectilePrefab.transform.Find(BossBarrageForge3DMissileChildName),
                "boss barrage Forge3D missile projectile",
                minimumParticleSystems: 2);
            ValidateNoImportedDependencies(projectilePrefab, "boss barrage projectile prefab");
        }

        private static void ValidateExactAssetReference(
            UnityEngine.Object actual,
            string label,
            UnityEngine.Object expected)
        {
            if (actual == expected)
            {
                return;
            }

            string expectedPath = expected != null ? AssetDatabase.GetAssetPath(expected) : "null";
            string actualPath = actual != null ? AssetDatabase.GetAssetPath(actual) : "null";
            throw new InvalidOperationException($"{label} expected {expectedPath}, found {actualPath}.");
        }

        private static void ValidateRangedBasicProjectilePrefab()
        {
            GameObject projectilePrefab = LoadAsset<GameObject>(RangedBasicProjectilePrefabPath);
            MeshRenderer rootRenderer = projectilePrefab.GetComponent<MeshRenderer>();
            if (rootRenderer != null && rootRenderer.enabled)
            {
                throw new InvalidOperationException("Player ranged basic projectile root MeshRenderer must stay disabled so the Vefects asset shot is the only visible projectile body.");
            }

            Transform shotVfxRoot = projectilePrefab.transform.Find("RangedBasicProjectileVfx_VefectsRifleShotLoop");
            if (shotVfxRoot == null)
            {
                throw new InvalidOperationException("Player ranged basic projectile prefab should use the promoted Vefects rifle shot loop asset VFX.");
            }

            if (projectilePrefab.GetComponent<TrailRenderer>() != null)
            {
                throw new InvalidOperationException("Player ranged basic projectile prefab should not fall back to generated TrailRenderer visuals.");
            }

            ParticleSystem[] particleSystems =
                shotVfxRoot.GetComponentsInChildren<ParticleSystem>(includeInactive: true);
            if (particleSystems.Length < 4)
            {
                throw new InvalidOperationException("Player ranged basic projectile should keep the authored multi-part Vefects particle setup.");
            }

            for (int i = 0; i < particleSystems.Length; i++)
            {
                ParticleSystem.LightsModule lights = particleSystems[i].lights;
                if (lights.enabled && lights.light != null)
                {
                    ValidateGameOwnedAsset(lights.light, $"{particleSystems[i].name} projectile Vefects light");
                    ValidateNoImportedDependencies(lights.light, $"{particleSystems[i].name} projectile Vefects light");
                }
            }

            ParticleSystemRenderer[] renderers =
                shotVfxRoot.GetComponentsInChildren<ParticleSystemRenderer>(includeInactive: true);
            if (renderers.Length == 0)
            {
                throw new InvalidOperationException("Player ranged basic projectile Vefects asset should expose particle renderers.");
            }

            for (int i = 0; i < renderers.Length; i++)
            {
                ParticleSystemRenderer renderer = renderers[i];
                ValidateGameOwnedAsset(renderer.sharedMaterial, $"{renderer.name} projectile Vefects material");
                ValidateRenderableMaterialShader(renderer.sharedMaterial, $"{renderer.name} projectile Vefects material shader");
                ValidateVefectsFlipbookMaterial(renderer.sharedMaterial, $"{renderer.name} projectile Vefects material");
                if (renderer.mesh != null)
                {
                    ValidateGameOwnedAsset(renderer.mesh, $"{renderer.name} projectile Vefects mesh");
                }
            }
        }

        private static void ValidateMagicMissilesLaneProjectilePrefab(
            string prefabPath,
            string childName,
            string label)
        {
            GameObject projectilePrefab = LoadAsset<GameObject>(prefabPath);
            MeshRenderer rootRenderer = projectilePrefab.GetComponent<MeshRenderer>();
            if (rootRenderer == null)
            {
                throw new InvalidOperationException($"{label} should keep a hidden collision root MeshRenderer.");
            }

            if (rootRenderer.enabled)
            {
                throw new InvalidOperationException($"{label} root MeshRenderer must stay hidden behind the asset VFX.");
            }

            ValidatePromotedParticleVfx(projectilePrefab.transform.Find(childName), label, 2);
            if (projectilePrefab.GetComponent<TrailRenderer>() != null)
            {
                throw new InvalidOperationException($"{label} should not fall back to generated TrailRenderer visuals.");
            }
        }

        private static void ValidatePromotedLaserLaneProjectilePrefab(
            string prefabPath,
            string childName,
            string label,
            int minimumParticleSystems = 4)
        {
            GameObject projectilePrefab = LoadAsset<GameObject>(prefabPath);
            MeshRenderer rootRenderer = projectilePrefab.GetComponent<MeshRenderer>();
            if (rootRenderer == null)
            {
                throw new InvalidOperationException($"{label} should keep a hidden collision root MeshRenderer.");
            }

            if (rootRenderer.enabled)
            {
                throw new InvalidOperationException($"{label} root MeshRenderer must stay hidden behind the FORGE3D beam VFX.");
            }

            ValidatePromotedParticleVfx(projectilePrefab.transform.Find(childName), label, minimumParticleSystems);
            if (projectilePrefab.GetComponent<TrailRenderer>() != null)
            {
                throw new InvalidOperationException($"{label} should not fall back to generated TrailRenderer visuals.");
            }
        }

        private static void ValidatePrimitiveLaneProjectilePrefab(
            string prefabPath,
            string childName,
            string label)
        {
            GameObject projectilePrefab = LoadAsset<GameObject>(prefabPath);
            MeshRenderer rootRenderer = projectilePrefab.GetComponent<MeshRenderer>();
            if (rootRenderer == null)
            {
                throw new InvalidOperationException($"{label} should keep a hidden collision root MeshRenderer.");
            }

            if (rootRenderer.enabled)
            {
                throw new InvalidOperationException($"{label} root MeshRenderer must stay hidden behind the authored primitive VFX.");
            }

            Transform visual = projectilePrefab.transform.Find(childName);
            if (visual == null)
            {
                throw new InvalidOperationException($"{label} is missing {childName}.");
            }

            MeshRenderer renderer = visual.GetComponent<MeshRenderer>();
            if (renderer == null || !renderer.enabled)
            {
                throw new InvalidOperationException($"{label} primitive VFX should expose an enabled MeshRenderer.");
            }

            ValidateGameOwnedAsset(renderer.sharedMaterial, $"{label} primitive material");
            ValidateRenderableMaterialShader(renderer.sharedMaterial, $"{label} primitive material shader");
            if (projectilePrefab.GetComponent<TrailRenderer>() != null)
            {
                throw new InvalidOperationException($"{label} should not fall back to generated TrailRenderer visuals.");
            }
        }

        private static void ValidateSummonEntryCueVfx(GameObject entryCuePrefab)
        {
            MeshRenderer rootRenderer = entryCuePrefab.GetComponent<MeshRenderer>();
            if (rootRenderer == null || rootRenderer.enabled)
            {
                throw new InvalidOperationException("summon entry cue should hide its collision/repair root renderer.");
            }

            ValidateSuppressedTemporaryVfx(
                entryCuePrefab.transform.Find("SummonEntryVfx_MagicMissilesArcaneCircle"),
                "summon entry MagicMissiles circle");
        }

        private static void ValidateSummonActorVfx(
            GameObject actorPrefab,
            string pulseRootName,
            bool expectPressureScreen,
            string label)
        {
            if (actorPrefab.transform.Find(pulseRootName) == null)
            {
                throw new InvalidOperationException($"{label} is missing {pulseRootName}.");
            }

            ValidateSuppressedTemporaryVfx(
                actorPrefab.transform.Find("SummonPulseVfx_MagicMissilesPulse"),
                $"{label} MagicMissiles pulse");
            ValidateSuppressedTemporaryVfx(
                FindChildWithPrefix(actorPrefab.transform, "SummonStateVfx_"),
                $"{label} MagicMissiles state aura");
            if (!expectPressureScreen)
            {
                return;
            }

            ValidateSuppressedTemporaryVfx(
                actorPrefab.transform.Find("SummonShieldVfx_MagicMissilesShieldCircle"),
                $"{label} MagicMissiles shield circle");
        }

        private static void ValidateSuppressedTemporaryVfx(Transform root, string label)
        {
            if (root == null)
            {
                return;
            }

            if (root.gameObject.activeSelf)
            {
                throw new InvalidOperationException($"{label} should not be active in the gameplay cleanup pass.");
            }

            ParticleSystem[] particleSystems = root.GetComponentsInChildren<ParticleSystem>(includeInactive: true);
            for (int i = 0; i < particleSystems.Length; i++)
            {
                ParticleSystem.MainModule main = particleSystems[i].main;
                if (main.playOnAwake)
                {
                    throw new InvalidOperationException($"{label} particle systems should not play on awake.");
                }
            }
        }

        private static void ValidateBossBarrageCombatCueAssetOverlays()
        {
            CombatVfxCueProfile profile =
                LoadAsset<CombatVfxCueProfile>(ActionFoundationCombatVfxSetup.CombatVfxCueProfilePath);
            if (profile.PlaybackMode != CombatVfxCuePlaybackMode.ReviewedCombatFeedbackOnly)
            {
                throw new InvalidOperationException("Combat VFX cue profile should allow only reviewed combat feedback cues.");
            }

            if (!profile.AllowsPlayback(CombatVfxCueId.PlayerRangedMuzzleFlash))
            {
                throw new InvalidOperationException("Player ranged muzzle flash should stay enabled as the only reviewed gun VFX cue.");
            }

            if (!profile.AllowsPlayback(CombatVfxCueId.EnemyHit))
            {
                throw new InvalidOperationException("Enemy hit VFX should play in the reviewed combat feedback pass.");
            }

            if (!profile.AllowsPlayback(CombatVfxCueId.PlayerPerfectDodgeTimeField)
                || !profile.AllowsPlayback(CombatVfxCueId.PlayerPerfectDodgePulsewave)
                || !profile.AllowsPlayback(CombatVfxCueId.PlayerPerfectDodgeHoloCube)
                || !profile.AllowsPlayback(CombatVfxCueId.PlayerPerfectDodgeWindow))
            {
                throw new InvalidOperationException("Reviewed perfect dodge VFX cues should play in the combat feedback cleanup pass.");
            }

            if (!profile.AllowsPlayback(CombatVfxCueId.PlayerRangedProjectileImpact))
            {
                throw new InvalidOperationException("Player ranged projectile impact cue should stay enabled for reviewed hit SFX.");
            }

            ValidateCombatCueAssetOverlay(
                profile,
                CombatVfxCueId.PlayerRangedProjectileImpact,
                "CueAssetVfx_MagicMissilesLightImpact",
                "player ranged impact MagicMissiles overlay");
            ValidateCombatCuePromotedParticlePrefab(
                profile,
                CombatVfxCueId.PlayerDamaged,
                "player damaged Vefects hit reference");
            ValidateCombatCuePromotedParticlePrefab(
                profile,
                CombatVfxCueId.PlayerCritical,
                "player critical Vefects hit reference");
            ValidateCombatCuePromotedParticlePrefab(
                profile,
                CombatVfxCueId.EnemyHit,
                "enemy hit Vefects impact");
            ValidateCombatCueAssetOverlay(
                profile,
                CombatVfxCueId.EnemyDeath,
                "CueAssetVfx_MagicMissilesDeathBurst",
                "enemy death MagicMissiles overlay");
            ValidateCombatCueAssetOverlay(
                profile,
                CombatVfxCueId.EliteShieldSignal,
                "CueAssetVfx_MagicMissilesGuardState",
                "elite shield MagicMissiles overlay");
            ValidateCombatCueAssetOverlay(
                profile,
                CombatVfxCueId.EliteAuraSignal,
                "CueAssetVfx_MagicMissilesActiveAura",
                "elite aura MagicMissiles overlay");
            ValidateCombatCueAssetOverlay(
                profile,
                CombatVfxCueId.EliteSummonSignal,
                "CueAssetVfx_MagicMissilesSummonState",
                "elite summon MagicMissiles overlay");
            ValidateCombatCueHasNoAssetOverlay(
                profile,
                CombatVfxCueId.SummonBlockOpportunity,
                "CueAssetVfx_MagicMissilesPressureStorm",
                "summon block opportunity MagicMissiles pressure storm overlay");
            ValidateCombatCueHasNoAssetOverlay(
                profile,
                CombatVfxCueId.SummonFollowupWindow,
                "CueAssetVfx_MagicMissilesFollowupCircle",
                "summon follow-up window MagicMissiles circle overlay");
            ValidateCombatCueVisualPrefab(
                profile,
                CombatVfxCueId.PlayerPerfectDodgeTimeField,
                "perfect dodge time field");
            ValidateCombatCueVisualPrefab(
                profile,
                CombatVfxCueId.PlayerPerfectDodgePulsewave,
                "perfect dodge pulsewave");
            ValidateCombatCueVisualPrefab(
                profile,
                CombatVfxCueId.PlayerPerfectDodgeHoloCube,
                "perfect dodge holo cube");
            ValidateCombatCueVisualPrefab(
                profile,
                CombatVfxCueId.PlayerPerfectDodgeWindow,
                "perfect dodge follow-up window");
        }

        private static void ValidateCombatCueAssetOverlay(
            CombatVfxCueProfile profile,
            CombatVfxCueId cueId,
            string childName,
            string label)
        {
            if (!profile.TryGetCue(cueId, out CombatVfxCue cue) || cue.Prefab == null)
            {
                throw new InvalidOperationException($"Boss barrage combat cue profile is missing {cueId}.");
            }

            ValidatePromotedParticleVfx(cue.Prefab.transform.Find(childName), label, 1);
            string prefabPath = AssetDatabase.GetAssetPath(cue.Prefab).Replace('\\', '/');
            ValidateNoImportedAssetReference(prefabPath);
        }

        private static void ValidateCombatCueHasNoAssetOverlay(
            CombatVfxCueProfile profile,
            CombatVfxCueId cueId,
            string childName,
            string label)
        {
            if (!profile.TryGetCue(cueId, out CombatVfxCue cue) || cue.Prefab == null)
            {
                throw new InvalidOperationException($"Boss barrage combat cue profile is missing {cueId}.");
            }

            if (cue.Prefab.transform.Find(childName) != null)
            {
                throw new InvalidOperationException($"{cueId} should not keep ambiguous {label}.");
            }

            string prefabPath = AssetDatabase.GetAssetPath(cue.Prefab).Replace('\\', '/');
            ValidateNoImportedAssetReference(prefabPath);
        }

        private static void ValidateCombatCuePromotedParticlePrefab(
            CombatVfxCueProfile profile,
            CombatVfxCueId cueId,
            string label)
        {
            if (!profile.TryGetCue(cueId, out CombatVfxCue cue) || cue.Prefab == null)
            {
                throw new InvalidOperationException($"Boss barrage combat cue profile is missing {cueId}.");
            }

            ValidatePromotedParticleVfx(cue.Prefab.transform, label, 1);
            string prefabPath = AssetDatabase.GetAssetPath(cue.Prefab).Replace('\\', '/');
            ValidateNoImportedAssetReference(prefabPath);
        }

        private static void ValidateCombatCueVisualPrefab(
            CombatVfxCueProfile profile,
            CombatVfxCueId cueId,
            string label)
        {
            if (!profile.TryGetCue(cueId, out CombatVfxCue cue) || cue.Prefab == null)
            {
                throw new InvalidOperationException($"Boss barrage combat cue profile is missing {cueId}.");
            }

            string prefabPath = AssetDatabase.GetAssetPath(cue.Prefab).Replace('\\', '/');
            ValidateNoImportedAssetReference(prefabPath);

            if (cue.Prefab.GetComponentInChildren<CombatVfxCueVisual>(includeInactive: true) == null)
            {
                throw new InvalidOperationException($"{label} should include a promoted CombatVfxCueVisual.");
            }

            Renderer[] renderers = cue.Prefab.GetComponentsInChildren<Renderer>(includeInactive: true);
            if (renderers.Length == 0)
            {
                throw new InvalidOperationException($"{label} should expose promoted renderers.");
            }

            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                Material[] materials = renderer.sharedMaterials;
                for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                {
                    if (materials[materialIndex] == null)
                    {
                        continue;
                    }

                    ValidateGameOwnedAsset(materials[materialIndex], $"{label}.{renderer.name} material");
                    ValidateRenderableMaterialShader(materials[materialIndex], $"{label}.{renderer.name} material shader");
                    ValidateNoImportedDependencies(materials[materialIndex], $"{label}.{renderer.name} material");
                }
            }

            CombatVfxCueAudioRandomizer[] randomizers =
                cue.Prefab.GetComponentsInChildren<CombatVfxCueAudioRandomizer>(includeInactive: true);
            for (int randomizerIndex = 0; randomizerIndex < randomizers.Length; randomizerIndex++)
            {
                CombatVfxCueAudioRandomizer randomizer = randomizers[randomizerIndex];
                for (int clipIndex = 0; clipIndex < randomizer.ClipCount; clipIndex++)
                {
                    AudioClip clip = randomizer.GetClip(clipIndex);
                    ValidateGameOwnedAsset(clip, $"{label}.{randomizer.name} clip {clipIndex + 1}");
                    ValidateNoImportedDependencies(clip, $"{label}.{randomizer.name} clip {clipIndex + 1}");
                }
            }
        }

        private static void ValidatePromotedParticleVfx(Transform root, string label, int minimumParticleSystems)
        {
            if (root == null)
            {
                throw new InvalidOperationException($"{label} should be authored as visual-only promoted VFX.");
            }

            if (root.GetComponentInChildren<Collider>(includeInactive: true) != null)
            {
                throw new InvalidOperationException($"{label} must remain visual-only and should not own a Collider.");
            }

            ParticleSystem[] particleSystems = root.GetComponentsInChildren<ParticleSystem>(includeInactive: true);
            if (particleSystems.Length < minimumParticleSystems)
            {
                throw new InvalidOperationException(
                    $"{label} should preserve its authored particle system stack.");
            }

            ParticleSystemRenderer[] renderers =
                root.GetComponentsInChildren<ParticleSystemRenderer>(includeInactive: true);
            if (renderers.Length == 0)
            {
                throw new InvalidOperationException($"{label} should expose promoted particle renderers.");
            }

            for (int i = 0; i < renderers.Length; i++)
            {
                ParticleSystemRenderer renderer = renderers[i];
                ValidateGameOwnedAsset(renderer.sharedMaterial, $"{label}.{renderer.name} material");
                ValidateRenderableMaterialShader(renderer.sharedMaterial, $"{label}.{renderer.name} material shader");
                ValidateNoImportedDependencies(renderer.sharedMaterial, $"{label}.{renderer.name} material");
                if (renderer.mesh != null)
                {
                    ValidateGameOwnedAsset(renderer.mesh, $"{label}.{renderer.name} mesh");
                    ValidateNoImportedDependencies(renderer.mesh, $"{label}.{renderer.name} mesh");
                }
            }

            AudioSource[] audioSources = root.GetComponentsInChildren<AudioSource>(includeInactive: true);
            for (int i = 0; i < audioSources.Length; i++)
            {
                AudioSource audioSource = audioSources[i];
                if (audioSource.clip == null)
                {
                    continue;
                }

                ValidateGameOwnedAsset(audioSource.clip, $"{label}.{audioSource.name} audio clip");
                ValidateNoImportedDependencies(audioSource.clip, $"{label}.{audioSource.name} audio clip");
            }
        }

        private static void ValidateBossBasicFire(
            BossBasicFireEmitter basicFireEmitter,
            SummonLaneSpace laneSpace,
            Transform playerTransform,
            CombatHealth bossHealth,
            Transform projectileRoot)
        {
            ValidateObjectReference(basicFireEmitter, "laneSpace", laneSpace);
            ValidateObjectReference(basicFireEmitter, "trackedPlayer", playerTransform);
            ValidateObjectReference(basicFireEmitter, "sourceHealth", bossHealth);
            Transform expectedFireOrigin = FindDescendant(basicFireEmitter.transform, BossBasicFireMuzzleName);
            if (expectedFireOrigin == null)
            {
                throw new InvalidOperationException("Boss basic fire should expose a weapon-mounted BossBasicFireMuzzle fire origin.");
            }

            ValidateObjectReference(basicFireEmitter, "fireOrigin", expectedFireOrigin);
            ValidateObjectReference(
                basicFireEmitter,
                "fireProfile",
                LoadAsset<BossBasicFireProfile>(BossBasicFireProfilePath));
            ValidateObjectReference(basicFireEmitter, "projectilePrefabObject", LoadAsset<GameObject>(ProjectilePrefabPath));
            ValidateObjectReference(basicFireEmitter, "projectileRoot", projectileRoot);
            ValidateEnum(basicFireEmitter, "sourceTeam", (int)DamageTeam.Enemy);
            ValidateBool(basicFireEmitter, "firingEnabled", true);
            ValidateFloat(basicFireEmitter, "resumeCooldownAfterSuppressionSeconds", 0.25f);
            ValidateInt(basicFireEmitter, "prewarmCount", 10);
            AudioSource volleyAudioSource = RequireReferencedObject<AudioSource>(basicFireEmitter, "volleyAudioSource");
            if (volleyAudioSource.transform.parent != basicFireEmitter.transform
                || !string.Equals(volleyAudioSource.name, "BossBasicFireAudio", StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Boss basic fire audio source should live under BossBasicFireAudio.");
            }

            ValidateArrayReference(
                basicFireEmitter,
                "volleySfxClips",
                0,
                LoadAsset<AudioClip>(BossBasicFireSfxClipPath));
            ValidateFloat(basicFireEmitter, "volleySfxVolume", 0.34f);
            ValidateVector2(basicFireEmitter, "volleySfxPitchRange", new Vector2(0.96f, 1.04f));

            BossBasicFireProfile profile = LoadAsset<BossBasicFireProfile>(BossBasicFireProfilePath);
            ValidateString(profile, "fireId", "LanePoke");
            ValidateString(profile, "readoutLabel", "Rifle Poke");
            ValidateFloat(profile, "initialDelaySeconds", 1.05f);
            ValidateFloat(profile, "fireIntervalSeconds", 1.95f);
            ValidateInt(profile, "projectilesPerVolley", 2);
            ValidateFloat(profile, "damage", 3.6f);
            ValidateFloat(profile, "projectileSpeed", 24f);
            ValidateFloat(profile, "projectileLifetimeSeconds", 1.35f);
            ValidateFloat(profile, "projectileRadius", 0.22f);
            ValidateFloat(profile, "backlineHalfSpread", 0.45f);
            ValidateFloat(profile, "forwardHalfSpread", 0.18f);
            ValidateFloat(profile, "spawnLateralFollowRatio", 0.92f);
            ValidateFloat(profile, "spawnHeight", 1.2f);
            ValidateFloat(profile, "targetHeight", 1.1f);
            ValidateColor(profile, "projectileColor", new Color(1f, 0.55f, 0.18f, 1f));
            ValidateVector3(profile, "projectileVisualScale", Vector3.one);
            ValidateObjectReference(
                profile,
                "projectileMaterial",
                LoadAsset<Material>(BossBasicFireProjectileMaterialPath));
        }

        private static void ValidateLaneAmbientVfx(Scene scene)
        {
            Transform root = RequireRoot(scene, AmbientVfxRootName).transform;
            if (root.gameObject.activeSelf)
            {
                throw new InvalidOperationException("Lane ambient VFX root should stay inactive during the VFX cleanup pass.");
            }

            ValidateAmbientVisual(root, "AmbientFlow_LeftRail_00", LaneAmbientFlowMaterialPath, expectMotion: true, expectFloating: false);
            ValidateAmbientVisual(root, "AmbientFlow_RightRail_00", LaneAmbientFlowMaterialPath, expectMotion: true, expectFloating: false);
            ValidateAmbientVisual(root, "AmbientDepthTick_00", LaneAmbientFlowMaterialPath, expectMotion: false, expectFloating: false);
            ValidateAmbientVisual(root, "AmbientDepthTick_04", BossPressureHorizonMaterialPath, expectMotion: false, expectFloating: false);
            ValidateAmbientVisual(root, "BossPressureHorizon_Curtain", BossPressureHorizonMaterialPath, expectMotion: true, expectFloating: false);
            ValidateAmbientVisual(root, "SummonRouteWisp_00", SummonRouteWispMaterialPath, expectMotion: false, expectFloating: true);
            ValidateAmbientVisual(root, "SummonRouteWisp_03", SummonRouteWispMaterialPath, expectMotion: false, expectFloating: true);
        }

        private static void ValidateSuppressedSceneVfxRoot(Scene scene, string rootName)
        {
            GameObject root = RequireRoot(scene, rootName);
            if (root.activeSelf)
            {
                throw new InvalidOperationException($"{rootName} should stay inactive during the VFX cleanup pass.");
            }
        }

        private static void ValidatePlayerDamageShaderFeedback(
            Scene scene,
            GameObject player,
            CombatHealth playerHealth,
            GameObject closeThreat,
            CombatHealth closeThreatHealth)
        {
            CombatHitFeedback playerFeedback =
                RequireComponent<CombatHitFeedback>(player, "player damage shader feedback");
            ValidateObjectReference(playerFeedback, "health", playerHealth);
            ValidateBool(playerFeedback, "renderHitFeedback", true);
            ValidateBool(playerFeedback, "applyIdleColorOnEnable", false);
            ValidateFloat(playerFeedback, "flashSeconds", 0.12f);
            ValidateColor(playerFeedback, "hitColor", new Color(1f, 0.46f, 0.38f, 1f));

            SerializedProperty flashRenderers =
                RequireProperty(new SerializedObject(playerFeedback), "flashRenderers");
            if (!flashRenderers.isArray || flashRenderers.arraySize < 3)
            {
                throw new InvalidOperationException(
                    "Player damage shader feedback should bind multiple promoted player renderers.");
            }

            for (int i = 0; i < flashRenderers.arraySize; i++)
            {
                if (flashRenderers.GetArrayElementAtIndex(i).objectReferenceValue is not Renderer)
                {
                    throw new InvalidOperationException(
                        $"Player damage shader feedback renderer slot {i} should reference a Renderer.");
                }
            }

            CombatHitFeedback closeThreatFeedback =
                ValidateCloseThreatDamageShaderFeedback(closeThreat, closeThreatHealth);
            GameObject[] rootObjects = scene.GetRootGameObjects();
            for (int i = 0; i < rootObjects.Length; i++)
            {
                CombatHitFeedback[] hitFeedbacks =
                    rootObjects[i].GetComponentsInChildren<CombatHitFeedback>(includeInactive: true);
                for (int j = 0; j < hitFeedbacks.Length; j++)
                {
                    if (hitFeedbacks[j] == playerFeedback || hitFeedbacks[j] == closeThreatFeedback)
                    {
                        continue;
                    }

                    ValidateBool(hitFeedbacks[j], "renderHitFeedback", false);
                }
            }
        }

        private static CombatHitFeedback ValidateCloseThreatDamageShaderFeedback(
            GameObject closeThreat,
            CombatHealth closeThreatHealth)
        {
            CombatHitFeedback feedback =
                RequireComponent<CombatHitFeedback>(closeThreat, "close threat damage shader feedback");
            ValidateObjectReference(feedback, "health", closeThreatHealth);
            ValidateBool(feedback, "renderHitFeedback", true);
            ValidateBool(feedback, "applyIdleColorOnEnable", false);
            ValidateFloat(feedback, "flashSeconds", 0.12f);
            ValidateColor(feedback, "hitColor", new Color(1f, 0.36f, 0.18f, 1f));

            SerializedProperty flashRenderers =
                RequireProperty(new SerializedObject(feedback), "flashRenderers");
            if (!flashRenderers.isArray || flashRenderers.arraySize == 0)
            {
                throw new InvalidOperationException(
                    "Close threat damage shader feedback should bind promoted enemy body renderers.");
            }

            for (int i = 0; i < flashRenderers.arraySize; i++)
            {
                if (flashRenderers.GetArrayElementAtIndex(i).objectReferenceValue is not Renderer)
                {
                    throw new InvalidOperationException(
                        $"Close threat damage shader feedback renderer slot {i} should reference a Renderer.");
                }
            }

            return feedback;
        }

        private static void ValidateLaneAmbientAudio(Scene scene)
        {
            Transform root = RequireRoot(scene, AmbientAudioRootName).transform;
            ValidateAmbientAudio(root, "AmbientAudio_ArenaStormBed", AmbientArenaStormClipPath, 0f, 0.05f, 0.07f);
            ValidateAmbientAudio(root, "AmbientAudio_ArenaEnergyWind", AmbientArenaEnergyWindClipPath, 0.2f, 0.028f, 0.042f);
            ValidateAmbientAudio(root, "AmbientAudio_ArenaEnergyWave", AmbientArenaEnergyWaveClipPath, 0.24f, 0.026f, 0.038f);
            ValidateAmbientAudio(root, "AmbientAudio_LeftRailDustFlow", AmbientRailDustFlowClipPath, 0.45f, 0.03f, 0.055f);
            ValidateAmbientAudio(root, "AmbientAudio_RightRailDustFlow", AmbientRailDustFlowClipPath, 0.45f, 0.03f, 0.055f);
            if (root.Find("AmbientAudio_LaneEnergyHum") != null)
            {
                throw new InvalidOperationException("AmbientAudio_LaneEnergyHum uses a clipped review hum and should not be present in the review loop bed.");
            }
        }

        private static void ValidateReviewSceneBgmSlot(Scene scene)
        {
            GameObject root = RequireRoot(scene, BgmAudioRootName);
            AudioSource source = RequireComponent<AudioSource>(root, "boss barrage BGM source");
            if (!source.playOnAwake || !source.loop)
            {
                throw new InvalidOperationException("Boss barrage BGM source should be a play-on-awake loop slot.");
            }

            if (source.spatialBlend > 0.001f)
            {
                throw new InvalidOperationException("Boss barrage BGM source should be 2D and independent from positional SFX.");
            }

            if (source.priority > 70
                || !source.bypassEffects
                || !source.bypassListenerEffects
                || !source.bypassReverbZones)
            {
                throw new InvalidOperationException("Boss barrage BGM source should stay isolated from scene SFX routing.");
            }
        }

        private static void ValidateAmbientAudio(
            Transform root,
            string childName,
            string clipPath,
            float minimumSpatialBlend,
            float minimumVolume,
            float maximumVolume)
        {
            Transform child = root.Find(childName);
            if (child == null)
            {
                throw new InvalidOperationException($"Missing ambient audio source {childName}.");
            }

            AudioSource source = RequireComponent<AudioSource>(child.gameObject, childName);
            AudioClip expectedClip = LoadAsset<AudioClip>(clipPath);
            if (source.clip != expectedClip)
            {
                string actualName = source.clip != null ? source.clip.name : "null";
                throw new InvalidOperationException($"{childName} should use {expectedClip.name}, found {actualName}.");
            }

            ValidateGameOwnedAsset(source.clip, $"{childName} clip");
            if (!source.playOnAwake || !source.loop)
            {
                throw new InvalidOperationException($"{childName} should be an authored play-on-awake loop.");
            }

            if (source.volume < minimumVolume || source.volume > maximumVolume)
            {
                throw new InvalidOperationException($"{childName} volume should stay between {minimumVolume:0.###} and {maximumVolume:0.###}.");
            }

            if (source.spatialBlend < minimumSpatialBlend || source.priority < 180)
            {
                throw new InvalidOperationException($"{childName} should stay subtle and lower priority than combat SFX.");
            }
        }

        private static void ValidateBossBarrageLaneReviewFootstepAudio(Scene scene)
        {
            PlayerMovementController player = RequireObject<PlayerMovementController>(scene, "player movement");
            ValidateFootstepAudio(
                player.gameObject,
                PlayerFootstepAudioName,
                PlayerFootstepClipPaths,
                player,
                0.34f,
                0.25f,
                0.82f);
            ValidateFootstepAudio(
                RequireRoot(scene, CloseThreatRootName),
                CloseThreatFootstepAudioName,
                ArmoredFootstepClipPaths,
                null,
                0.32f,
                0.65f,
                0.74f);
            ValidateFootstepAudio(
                RequireRoot(scene, BossProxyRootName),
                BossProxyFootstepAudioName,
                ArmoredFootstepClipPaths,
                null,
                0.24f,
                0.75f,
                0.7f);
            ValidatePrefabFootstepAudio(SummonSlot1ActorPrefabPath, HeavyFootstepClipPaths, 0.28f, 0.65f, 0.58f);
            ValidatePrefabFootstepAudio(SummonSlot2ActorPrefabPath, ArmoredFootstepClipPaths, 0.24f, 0.6f, 0.54f);
            ValidatePrefabFootstepAudio(SummonSlot3ActorPrefabPath, HeavyFootstepClipPaths, 0.34f, 0.7f, 0.6f);
            ValidatePrefabFootstepAudio(BossSummonPressureActorPrefabPath, HeavyFootstepClipPaths, 0.32f, 0.72f, 0.62f);
        }

        private static void ValidatePrefabFootstepAudio(
            string prefabPath,
            string[] expectedClipPaths,
            float maximumBaseVolume,
            float minimumSpatialBlend,
            float expectedPlaybackVolumeScale)
        {
            GameObject prefab = LoadAsset<GameObject>(prefabPath);
            ValidateFootstepAudio(
                prefab,
                SummonActorFootstepAudioName,
                expectedClipPaths,
                null,
                maximumBaseVolume,
                minimumSpatialBlend,
                expectedPlaybackVolumeScale);
        }

        private static void ValidateFootstepAudio(
            GameObject root,
            string childName,
            string[] expectedClipPaths,
            PlayerMovementController expectedPlayerMovement,
            float maximumBaseVolume,
            float minimumSpatialBlend,
            float expectedPlaybackVolumeScale)
        {
            Transform child = root.transform.Find(childName);
            if (child == null)
            {
                throw new InvalidOperationException($"{root.name} is missing reviewed footstep audio child {childName}.");
            }

            AudioSource source = RequireComponent<AudioSource>(child.gameObject, $"{childName} source");
            MovementFootstepAudioPresenter presenter =
                RequireComponent<MovementFootstepAudioPresenter>(child.gameObject, $"{childName} presenter");
            ValidateObjectReference(presenter, "source", source);
            ValidateObjectReference(presenter, "trackedTransform", root.transform);
            ValidateObjectReference(presenter, "playerMovement", expectedPlayerMovement);
            if (source.clip != null || source.loop || source.playOnAwake)
            {
                throw new InvalidOperationException($"{childName} should play randomized one-shot footsteps only.");
            }

            if (source.volume > maximumBaseVolume || presenter.BaseVolume > maximumBaseVolume)
            {
                throw new InvalidOperationException($"{childName} footstep volume is too high for review-scene ambience.");
            }

            if (source.spatialBlend < minimumSpatialBlend)
            {
                throw new InvalidOperationException($"{childName} should keep positional space.");
            }

            if (Mathf.Abs(presenter.PlaybackVolumeScale - expectedPlaybackVolumeScale) > 0.001f)
            {
                throw new InvalidOperationException(
                    $"{childName} playback volume scale should stay at {expectedPlaybackVolumeScale:0.##}.");
            }

            if (source.priority < 130 || source.priority > 170)
            {
                throw new InvalidOperationException($"{childName} should sit between combat SFX and ambient loop priority.");
            }

            if (presenter.ClipCount != expectedClipPaths.Length)
            {
                throw new InvalidOperationException(
                    $"{childName} should use {expectedClipPaths.Length} reviewed footstep variations.");
            }

            for (int i = 0; i < expectedClipPaths.Length; i++)
            {
                AudioClip clip = presenter.GetClip(i);
                AudioClip expectedClip = LoadAsset<AudioClip>(expectedClipPaths[i]);
                if (clip != expectedClip)
                {
                    string actualPath = clip != null ? AssetDatabase.GetAssetPath(clip) : "null";
                    throw new InvalidOperationException($"{childName} clip {i} should use {expectedClipPaths[i]}, found {actualPath}.");
                }

                ValidateGameOwnedAsset(clip, $"{childName} clip {i}");
            }
        }

        private static void ValidateAmbientVisual(
            Transform root,
            string childName,
            string materialPath,
            bool expectMotion,
            bool expectFloating)
        {
            Transform child = root.Find(childName);
            if (child == null)
            {
                throw new InvalidOperationException($"Missing ambient VFX visual {childName}.");
            }

            if (child.GetComponent<Collider>() != null)
            {
                throw new InvalidOperationException($"{childName} must stay visual-only and should not block movement.");
            }

            Renderer renderer = RequireComponent<Renderer>(child.gameObject, childName);
            ValidateObjectReference(renderer, "m_Materials.Array.data[0]", LoadAsset<Material>(materialPath));
            ValidateGameOwnedAsset(renderer.sharedMaterial, $"{childName} material");
            ValidateRenderableMaterialShader(renderer.sharedMaterial, $"{childName} material shader");

            if (expectMotion && child.GetComponent<ActionFoundationArenaTransformMotion>() == null)
            {
                throw new InvalidOperationException($"{childName} should use arena transform motion for ambient movement.");
            }

            if (expectFloating && child.GetComponent<ActionFoundationArenaFloatingShape>() == null)
            {
                throw new InvalidOperationException($"{childName} should use arena floating pulse for summon-route read.");
            }
        }

        private static void ValidateSummonForwardSpace(SummonLaneSpace laneSpace)
        {
            Vector3 playerIllegalPoint = laneSpace.GetLaneWorldPoint(0f, laneSpace.BossProxyZ, 0f);
            Vector3 clamped = laneSpace.ClampPlayerPosition(playerIllegalPoint);
            if (laneSpace.IsPastForwardBoundary(clamped))
            {
                throw new InvalidOperationException("Player clamp must keep the player before the forward boundary.");
            }

            Vector3 summonEntry = laneSpace.GetLaneWorldPoint(0f, laneSpace.SummonEntryZ, 0f);
            if (!laneSpace.IsPastForwardBoundary(summonEntry))
            {
                throw new InvalidOperationException("Summon entry must remain valid beyond the player forward boundary.");
            }

            Vector3 offLaneSummonPoint = laneSpace.GetBattlefieldWorldPoint(laneSpace.HalfWidth + 1f, laneSpace.SummonEntryZ, 0f);
            if (laneSpace.GetLaneCoordinates(offLaneSummonPoint).x <= laneSpace.HalfWidth)
            {
                throw new InvalidOperationException("Summon battlefield coordinates must be able to cross lateral lane rails.");
            }
        }

        private static void ValidatePlayerEnergyActions(
            PlayerSkill1Action skill1Action,
            PlayerSummonSlot1Action summonSlot1Action,
            SummonEnergyLadder energyLadder,
            CombatHealth playerHealth,
            PlayerCombatTargetSelector targetSelector,
            CombatHealth frontlineTargetHealth,
            SummonLaneSpace laneSpace)
        {
            GameObject projectileRoot = RequireRoot(SceneManager.GetActiveScene(), ProjectilePoolRootName);
            GameObject actionCueRoot = RequireRoot(SceneManager.GetActiveScene(), ActionCuePoolRootName);
            GameObject summonActorRoot = RequireRoot(SceneManager.GetActiveScene(), SummonActorPoolRootName);

            ValidateObjectReference(skill1Action, "energyLadder", energyLadder);
            ValidateObjectReference(skill1Action, "sourceHealth", playerHealth);
            ValidateObjectReference(skill1Action, "targetSelector", targetSelector);
            ValidateObjectReference(skill1Action, "projectilePrefabObject", LoadAsset<GameObject>(Skill1ProjectilePrefabPath));
            ValidateObjectReference(skill1Action, "projectileRoot", projectileRoot.transform);
            ValidateEnum(skill1Action, "sourceTeam", (int)DamageTeam.Player);

            ValidateObjectReference(summonSlot1Action, "energyLadder", energyLadder);
            ValidateObjectReference(summonSlot1Action, "sourceHealth", playerHealth);
            ValidateObjectReference(summonSlot1Action, "targetSelector", targetSelector);
            ValidateObjectReference(summonSlot1Action, "frontlineTargetHealth", frontlineTargetHealth);
            ValidateObjectReference(summonSlot1Action, "laneSpace", laneSpace);
            ValidateObjectReference(summonSlot1Action, "projectilePrefabObject", LoadAsset<GameObject>(SummonSlot1ProjectilePrefabPath));
            ValidateObjectReference(summonSlot1Action, "entryCuePrefab", LoadAsset<GameObject>(SummonSlot1EntryCuePrefabPath));
            ValidateSummonEntryCueVfx(LoadAsset<GameObject>(SummonSlot1EntryCuePrefabPath));
            ValidateObjectReference(summonSlot1Action, "summonActorPrefabObject", LoadAsset<GameObject>(SummonSlot1ActorPrefabPath));
            ValidateObjectReference(summonSlot1Action, "projectileRoot", projectileRoot.transform);
            ValidateObjectReference(summonSlot1Action, "cueRoot", actionCueRoot.transform);
            ValidateObjectReference(summonSlot1Action, "summonActorRoot", summonActorRoot.transform);
            ValidateCombatVfxCuePlayerReference(
                summonSlot1Action,
                "combatVfxCuePlayer",
                RequireComponent<CombatVfxCuePlayer>(summonSlot1Action.gameObject, "player combat VFX cue player"));
            ValidateEnum(summonSlot1Action, "sourceTeam", (int)DamageTeam.AllySummon);
            ValidateInt(summonSlot1Action, "maxActiveSummonActors", 1);
            ValidateFloat(summonSlot1Action, "entryForwardOffset", 1.35f);
            ValidateFloatAtLeast(summonSlot1Action, "actorEntryCatchupSecondsPerMeter", 0.1f);
            ValidateFloat(
                summonSlot1Action,
                "requiredSummonMana",
                BossBarrageSummonReviewContract.Slot1RequiredMana);
            SummonSlotActionProfile summonSlot1Profile = LoadAsset<SummonSlotActionProfile>(SummonSlot1ActionProfilePath);
            ValidateObjectReference(
                summonSlot1Action,
                "summonActionProfile",
                summonSlot1Profile);
            ValidateSummonSlotReadout(
                summonSlot1Profile,
                1,
                "LV1 Charge Break",
                "Mid-cost bruiser that spends a saved bar for one obvious forward rush impact, then stays as melee pressure.",
                "Hold EN until a boss summon or recovery window is worth a visible charge answer.",
                "SciFi bruiser spawns on the frontline, rushes forward with a ground trail, hits with a clear impact burst, and keeps punching.");
            ValidateSummonSlotReadout(
                summonSlot1Profile,
                2,
                "LV2 Heavy Charge",
                "Higher stored-EN version with a wider body-check screen and enough health to hold contact longer.",
                "Use when the boss is about to stay in a punishable lane and a cheap laser will not change the exchange.",
                "Longer rush, two shock bolts, a broader collision burst, and steadier melee lockdown.");
            ValidateSummonSlotReadout(
                summonSlot1Profile,
                3,
                "LV3 Breakthrough Rush",
                "High-stored-EN payoff that should visibly interrupt the lane and keep fighting after impact.",
                "Save for the exchange where one big arrival has to change the screen immediately.",
                "Fast ground rush, three shock bolts, large forward impact, and a durable bruiser body that remains in melee without out-DPSing the boss summons.");
            ValidateSummonSlotTier(
                summonSlot1Profile,
                1,
                "ChargeBruiser",
                expectedActorScale: 2.0f,
                expectedActorMaxHealth: 250f,
                expectedActorMoveSpeed: 3.4f,
                expectedActorEngageRadius: 1.1f,
                expectedActorAttackDamagePerSecond: 12f,
                expectedActorAttackIntervalSeconds: 1.05f,
                expectedActorLifetimeSeconds: 0f,
                expectedActorAdvanceDistance: 4.0f,
                expectedScreenIntercepts: 1,
                expectedScreenRadius: 1.55f,
                expectedScreenLifetimeSeconds: 1.45f,
                expectedCounterDamage: 18.56f);
            ValidateSummonSlotTier(
                summonSlot1Profile,
                2,
                "ChargeBruiser",
                expectedActorScale: 2.36f,
                expectedActorMaxHealth: 420f,
                expectedActorMoveSpeed: 3.8f,
                expectedActorEngageRadius: 1.2f,
                expectedActorAttackDamagePerSecond: 20f,
                expectedActorAttackIntervalSeconds: 1.1f,
                expectedActorLifetimeSeconds: 0f,
                expectedActorAdvanceDistance: 4.8f,
                expectedScreenIntercepts: 2,
                expectedScreenRadius: 1.75f,
                expectedScreenLifetimeSeconds: 1.8f,
                expectedCounterDamage: 29.44f);
            ValidateSummonSlotTier(
                summonSlot1Profile,
                3,
                "ChargeBruiser",
                expectedActorScale: 2.74f,
                expectedActorMaxHealth: 600f,
                expectedActorMoveSpeed: 4.2f,
                expectedActorEngageRadius: 1.32f,
                expectedActorAttackDamagePerSecond: 30f,
                expectedActorAttackIntervalSeconds: 1.15f,
                expectedActorLifetimeSeconds: 0f,
                expectedActorAdvanceDistance: 5.6f,
                expectedScreenIntercepts: 3,
                expectedScreenRadius: 1.95f,
                expectedScreenLifetimeSeconds: 2.15f,
                expectedCounterDamage: 40.32f);

            SummonFrontlineProxy summonActorPrefab = LoadPrefabComponent<SummonFrontlineProxy>(SummonSlot1ActorPrefabPath);
            SummonPressureScreen pressureScreen = LoadPrefabComponent<SummonPressureScreen>(SummonSlot1ActorPrefabPath);
            SummonPressureScreenPresenter presenter =
                LoadPrefabComponent<SummonPressureScreenPresenter>(SummonSlot1ActorPrefabPath);
            SummonFrontlineProxyPresenter actorPresenter =
                LoadPrefabComponent<SummonFrontlineProxyPresenter>(SummonSlot1ActorPrefabPath);
            SummonFrontlineClash summonClash = LoadPrefabComponent<SummonFrontlineClash>(SummonSlot1ActorPrefabPath);
            CombatHealth summonHealth = LoadPrefabComponent<CombatHealth>(SummonSlot1ActorPrefabPath);
            Transform pressureScreenVisual = summonActorPrefab.transform.Find("PressureScreenVisual");
            if (pressureScreenVisual == null)
            {
                throw new InvalidOperationException("SummonSlot1 actor prefab is missing PressureScreenVisual.");
            }

            MeshRenderer pressureScreenRenderer = pressureScreenVisual.GetComponent<MeshRenderer>();
            if (pressureScreenRenderer == null)
            {
                throw new InvalidOperationException("PressureScreenVisual is missing a MeshRenderer.");
            }

            Transform tierPulseCore = summonActorPrefab.transform.Find("TierPulseCore");
            if (tierPulseCore == null)
            {
                throw new InvalidOperationException("SummonSlot1 actor prefab is missing TierPulseCore.");
            }

            MeshRenderer pulseRenderer = tierPulseCore.GetComponent<MeshRenderer>();
            if (pulseRenderer == null)
            {
                throw new InvalidOperationException("TierPulseCore is missing a MeshRenderer.");
            }

            MeshRenderer rootRenderer = summonActorPrefab.GetComponent<MeshRenderer>();
            if (rootRenderer != null && rootRenderer.enabled)
            {
                throw new InvalidOperationException(
                    "SummonSlot1 actor root mesh renderer must stay disabled so the promoted model reads first.");
            }

            ValidateSummonActorVfx(
                summonActorPrefab.gameObject,
                "TierPulseCore",
                expectPressureScreen: true,
                label: "SummonSlot1 actor prefab");
            ValidateObjectReference(summonActorPrefab, "pressureScreen", pressureScreen);
            ValidateObjectReference(summonActorPrefab, "health", summonHealth);
            ValidateObjectReference(summonClash, "proxy", summonActorPrefab);
            ValidateObjectReference(summonClash, "health", summonHealth);
            ValidateSummonActorBodyContract(
                summonActorPrefab.gameObject,
                summonActorPrefab,
                summonClash,
                summonHealth,
                DamageTeam.AllySummon,
                "SummonSlot1 actor prefab");
            ValidateSummonHealthBar(
                summonActorPrefab.gameObject,
                summonActorPrefab,
                summonHealth,
                "SummonSlot1 actor prefab");
            ValidateEnum(pressureScreen, "ownerTeam", (int)DamageTeam.AllySummon);
            ValidateInt(pressureScreen, "defaultMaxIntercepts", 2);
            ValidateFloat(pressureScreen, "defaultLifetimeSeconds", 1.2f);
            ValidateFloat(pressureScreen, "defaultRadius", 1.35f);
            ValidateObjectReference(presenter, "pressureScreen", pressureScreen);
            ValidateObjectReference(presenter, "visualRoot", pressureScreenVisual);
            ValidateArrayReference(presenter, "screenRenderers", 0, pressureScreenRenderer);
            ValidateBool(presenter, "renderVisuals", false);
            ValidateObjectReference(actorPresenter, "proxy", summonActorPrefab);
            ValidateObjectReference(actorPresenter, "clash", summonClash);
            ValidateObjectReference(actorPresenter, "pulseRoot", tierPulseCore);
            ValidateBool(actorPresenter, "renderPulseVisuals", false);
            Transform summonActorVisual = ValidateSummonActorRoleVisual(
                summonActorPrefab.gameObject,
                SummonSlot1ActorVisualName);
            Renderer[] summonActorVisualRenderers = CollectEnabledRenderers(summonActorVisual.gameObject);
            if (summonActorVisualRenderers.Length == 0)
            {
                throw new InvalidOperationException(
                    $"{SummonSlot1ActorVisualName} should expose at least one enabled renderer.");
            }

            SummonProxyVisualMotionPresenter motionPresenter =
                RequireComponent<SummonProxyVisualMotionPresenter>(
                    summonActorPrefab.gameObject,
                    "SummonSlot1 visual motion presenter");
            if (FindDescendant(summonActorPrefab.transform, "ChargeReadyAura") != null
                || FindDescendant(summonActorPrefab.transform, "ChargeRushTrail") != null
                || FindDescendant(summonActorPrefab.transform, "PF_SummonChargeRushTrail_SPECIAL") != null
                || FindDescendant(summonActorPrefab.transform, "ChargeImpactBurst") != null
                || FindDescendant(summonActorPrefab.transform, "PF_SummonChargeImpact_SPECIAL") != null
                || FindDescendant(summonActorPrefab.transform, "JumpSlamAirTrail") != null
                || FindDescendant(summonActorPrefab.transform, "PF_SummonJumpSlamAirTrail_SPECIAL") != null
                || FindDescendant(summonActorPrefab.transform, "SlamImpactBurst") != null
                || FindDescendant(summonActorPrefab.transform, "PF_SummonJumpSlamImpact_SPECIAL") != null)
            {
                throw new InvalidOperationException("SummonSlot1 must not keep retired charge or jump-slam VFX children.");
            }

            ValidateFloat(motionPresenter, "jumpArcHeight", 0f);
            ValidateFloat(motionPresenter, "tierArcHeightStep", 0f);
            ValidateFloat(motionPresenter, "landingSettleSeconds", 0f);
            ValidateFloat(motionPresenter, "landingDip", 0f);
            ValidateObjectReference(motionPresenter, "movementVfxRoot", null);
            if (motionPresenter.MovementVfxParticleCount != 0)
            {
                throw new InvalidOperationException(
                    "SummonSlot1 visual motion presenter should not drive the retired charge rush VFX stack.");
            }

            if (summonActorPrefab.GetComponent<SummonAttackBeamPresenter>() != null)
            {
                throw new InvalidOperationException(
                    "SummonSlot1 should not keep the retired charge impact presenter.");
            }

            ValidatePulseOnlyActorRenderers(actorPresenter, pulseRenderer, "TierPulseCore");
            ValidateSummonActorDamageFlashRenderers(
                actorPresenter,
                summonActorVisual,
                $"{SummonSlot1ActorVisualName} body flash");
            ValidateSummonActorAnimatorPresentation(
                actorPresenter,
                summonActorVisual,
                "SummonSlot1 actor prefab",
                expectedAnimatorMoveSpeedScale: 0.42f);
            ValidateFloat(actorPresenter, "entryFlashSeconds", 0.22f);
            ValidateFloat(actorPresenter, "impactFlashSeconds", 0.18f);
            ValidateFloat(actorPresenter, "clashFlashSeconds", 0.14f);
            ValidateFloat(actorPresenter, "impactFlashProgress", 0.86f);
            ValidateFloat(actorPresenter, "pulseSpeed", 8f);
            ValidateFloat(actorPresenter, "pulseScale", 0.08f);
            ValidateFloat(actorPresenter, "tierScaleStep", 0.18f);
            ValidateFloat(actorPresenter, "flashScale", 0.22f);
            ValidateFloat(actorPresenter, "clashFlashScale", 0.16f);
        }

        private static void ValidateSupportSummonSlotAction(
            PlayerSupportSummonSlotAction action,
            string slotActionName,
            SummonEnergyLadder energyLadder,
            CombatHealth playerHealth,
            PlayerCombatTargetSelector targetSelector,
            CombatHealth frontlineTargetHealth,
            SummonLaneSpace laneSpace,
            string projectilePrefabPath,
            string actorPrefabPath,
            string actorVisualName,
            string actionProfilePath,
            int expectedMinimumSummonTier,
            float expectedRequiredSummonMana,
            float expectedMaxHealth,
            bool expectPressureScreen,
            float firstVolleyDelaySeconds,
            float volleyIntervalSeconds,
            int maxVolleyCount)
        {
            GameObject projectileRoot = RequireRoot(SceneManager.GetActiveScene(), ProjectilePoolRootName);
            GameObject actionCueRoot = RequireRoot(SceneManager.GetActiveScene(), ActionCuePoolRootName);
            GameObject summonActorRoot = RequireRoot(SceneManager.GetActiveScene(), SummonActorPoolRootName);

            ValidateString(action, "slotActionName", slotActionName);
            ValidateObjectReference(action, "energyLadder", energyLadder);
            ValidateObjectReference(action, "sourceHealth", playerHealth);
            ValidateObjectReference(action, "targetSelector", targetSelector);
            ValidateObjectReference(action, "frontlineTargetHealth", frontlineTargetHealth);
            ValidateObjectReference(action, "laneSpace", laneSpace);
            ValidateObjectReference(action, "projectilePrefabObject", LoadAsset<GameObject>(projectilePrefabPath));
            ValidateObjectReference(action, "entryCuePrefab", LoadAsset<GameObject>(SummonSlot1EntryCuePrefabPath));
            ValidateObjectReference(action, "summonActorPrefabObject", LoadAsset<GameObject>(actorPrefabPath));
            ValidateObjectReference(action, "projectileRoot", projectileRoot.transform);
            ValidateObjectReference(action, "cueRoot", actionCueRoot.transform);
            ValidateObjectReference(action, "summonActorRoot", summonActorRoot.transform);
            ValidateCombatVfxCuePlayerReference(
                action,
                "combatVfxCuePlayer",
                RequireComponent<CombatVfxCuePlayer>(action.gameObject, "player combat VFX cue player"));
            ValidateEnum(action, "sourceTeam", (int)DamageTeam.AllySummon);
            ValidateFloat(action, "requiredSummonMana", expectedRequiredSummonMana);
            ValidateInt(action, "minimumSummonTier", expectedMinimumSummonTier);
            ValidateInt(action, "maxActiveSummonActors", 1);
            ValidateFloat(action, "entryForwardOffset", 1.35f);
            ValidateFloatAtLeast(action, "actorEntryCatchupSecondsPerMeter", 0.1f);
            SummonSlotActionProfile actionProfile = LoadAsset<SummonSlotActionProfile>(actionProfilePath);
            ValidateObjectReference(action, "summonActionProfile", actionProfile);
            ValidateSupportSummonRoleProfile(actionProfile, slotActionName);
            ValidateFloat(action, "firstVolleyDelaySeconds", firstVolleyDelaySeconds);
            ValidateFloat(action, "volleyIntervalSeconds", volleyIntervalSeconds);
            ValidateInt(action, "maxVolleyCount", maxVolleyCount);

            SummonFrontlineProxy actorPrefab = LoadPrefabComponent<SummonFrontlineProxy>(actorPrefabPath);
            SummonFrontlineProxyPresenter actorPresenter =
                LoadPrefabComponent<SummonFrontlineProxyPresenter>(actorPrefabPath);
            SummonFrontlineClash actorClash = LoadPrefabComponent<SummonFrontlineClash>(actorPrefabPath);
            CombatHealth actorHealth = LoadPrefabComponent<CombatHealth>(actorPrefabPath);
            Transform pulseCore = actorPrefab.transform.Find("TierPulseCore");
            if (pulseCore == null)
            {
                throw new InvalidOperationException($"{slotActionName} actor prefab is missing TierPulseCore.");
            }

            MeshRenderer pulseRenderer = pulseCore.GetComponent<MeshRenderer>();
            if (pulseRenderer == null)
            {
                throw new InvalidOperationException($"{slotActionName} TierPulseCore is missing a MeshRenderer.");
            }

            ValidateObjectReference(actorPresenter, "proxy", actorPrefab);
            ValidateObjectReference(actorPresenter, "pulseRoot", pulseCore);
            ValidateBool(actorPresenter, "renderPulseVisuals", false);
            ValidateObjectReference(actorPrefab, "health", actorHealth);
            ValidateObjectReference(actorClash, "proxy", actorPrefab);
            ValidateObjectReference(actorClash, "health", actorHealth);
            ValidateObjectReference(actorPresenter, "clash", actorClash);
            ValidateFloat(actorHealth, "maxHealth", expectedMaxHealth);
            ValidateSummonActorBodyContract(
                actorPrefab.gameObject,
                actorPrefab,
                actorClash,
                actorHealth,
                DamageTeam.AllySummon,
                $"{slotActionName} actor prefab");
            ValidateSummonHealthBar(
                actorPrefab.gameObject,
                actorPrefab,
                actorHealth,
                $"{slotActionName} actor prefab");
            ValidateSupportSummonPressureScreen(actorPrefab, expectPressureScreen, slotActionName);
            ValidateSummonActorVfx(
                actorPrefab.gameObject,
                "TierPulseCore",
                expectPressureScreen,
                $"{slotActionName} actor prefab");
            ValidatePulseOnlyActorRenderers(actorPresenter, pulseRenderer, "TierPulseCore");
            Transform actorVisual = ValidateSummonActorRoleVisual(actorPrefab.gameObject, actorVisualName);
            ValidateSummonActorDamageFlashRenderers(
                actorPresenter,
                actorVisual,
                $"{slotActionName} promoted body flash");
            ValidateSummonActorAnimatorPresentation(
                actorPresenter,
                actorVisual,
                $"{slotActionName} actor prefab",
                expectedAnimatorMoveSpeedScale: 0.46f);
            ValidateFloat(actorPresenter, "clashFlashSeconds", 0.14f);
            ValidateFloat(actorPresenter, "clashFlashScale", 0.14f);
            ValidateNoImportedAssetReference(projectilePrefabPath);
            ValidateNoImportedAssetReference(actorPrefabPath);
            ValidateNoImportedAssetReference(actionProfilePath);
        }

        private static void ValidateSupportSummonRoleProfile(
            SummonSlotActionProfile profile,
            string slotActionName)
        {
            if (string.Equals(slotActionName, "SummonSlot2", StringComparison.Ordinal))
            {
                ValidateSummonSlotReadout(
                    profile,
                    1,
                    "LV1 Laser Tap",
                    "Low-return ranged helper that sets up cleanly but cannot block pressure.",
                    "Spend when the boss lane is open and a cheap ranged body is enough.",
                    "SciFi rifleman slides into a side lane, flashes a cyan muzzle beam, and fires one clean laser line per volley.");
                ValidateSummonSlotTier(
                    profile,
                    1,
                    "LaserSoldier",
                    expectedActorScale: 2.08f,
                    expectedActorMaxHealth: 170f,
                    expectedActorMoveSpeed: 2.8f,
                    expectedActorEngageRadius: 0.72f,
                    expectedActorAttackDamagePerSecond: 9f,
                    expectedActorAttackIntervalSeconds: 1.15f,
                    expectedActorLifetimeSeconds: 0f,
                    expectedActorAdvanceDistance: 1.35f,
                    expectedScreenIntercepts: 0);
                ValidateSummonSlotReadout(
                    profile,
                    2,
                    "LV2 Split Laser",
                    "Mid-tier ranged support with two visible lines and controlled sustained pressure.",
                    "Hold EN if the boss will stay exposed for more than one volley.",
                    "Laser soldier fires one sharper cyan line with a larger beam flash while staying fragile.");
                ValidateSummonSlotTier(
                    profile,
                    2,
                    "LaserSoldier",
                    expectedActorScale: 2.08f,
                    expectedActorMaxHealth: 205f,
                    expectedActorMoveSpeed: 3.1f,
                    expectedActorEngageRadius: 0.78f,
                    expectedActorAttackDamagePerSecond: 12f,
                    expectedActorAttackIntervalSeconds: 1.25f,
                    expectedActorLifetimeSeconds: 0f,
                    expectedActorAdvanceDistance: 1.6f,
                    expectedScreenIntercepts: 0);
                ValidateSummonSlotReadout(
                    profile,
                    3,
                    "LV3 Prism Burst",
                    "High-tier glass-cannon support that widens the lane punish without becoming a turret.",
                    "Use when the player has created a long punish window and does not need a blocker.",
                    "Wider two-line laser burst and stronger muzzle beam, but a slower cadence and low body safety.");
                ValidateSummonSlotTier(
                    profile,
                    3,
                    "LaserSoldier",
                    expectedActorScale: 2.08f,
                    expectedActorMaxHealth: 250f,
                    expectedActorMoveSpeed: 3.4f,
                    expectedActorEngageRadius: 0.84f,
                    expectedActorAttackDamagePerSecond: 16f,
                    expectedActorAttackIntervalSeconds: 1.35f,
                    expectedActorLifetimeSeconds: 0f,
                    expectedActorAdvanceDistance: 1.85f,
                    expectedScreenIntercepts: 0);
                return;
            }

            if (string.Equals(slotActionName, "SummonSlot3", StringComparison.Ordinal))
            {
                ValidateSummonSlotReadout(
                    profile,
                    1,
                    "LV1 Fire Breath",
                    "Expensive ranged summon that trades speed and cadence for a wide flame lane.",
                    "Spend only when the boss is committed and the player can live without a blocker.",
                    "Fire dragon hovers above the lane and breathes one broad fire lance from a visible orange beam.");
                ValidateSummonSlotTier(
                    profile,
                    1,
                    "FireDragon",
                    expectedActorScale: 2.42f,
                    expectedActorMaxHealth: 520f,
                    expectedActorMoveSpeed: 2.35f,
                    expectedActorEngageRadius: 1.18f,
                    expectedActorAttackDamagePerSecond: 32f,
                    expectedActorAttackIntervalSeconds: 1.9f,
                    expectedActorLifetimeSeconds: 0f,
                    expectedActorAdvanceDistance: 1.35f,
                    expectedScreenIntercepts: 0);
                ValidateSummonSlotReadout(
                    profile,
                    2,
                    "LV2 Furnace Sweep",
                    "Mid-tier dragon breath covers a wider lane and rewards a longer punish read.",
                    "Hold EN when the boss will remain exposed after the first breath tick.",
                    "Larger hovering dragon, two fire chunks, wider lateral spread, and a stronger breath beam.");
                ValidateSummonSlotTier(
                    profile,
                    2,
                    "FireDragon",
                    expectedActorScale: 2.72f,
                    expectedActorMaxHealth: 680f,
                    expectedActorMoveSpeed: 2.65f,
                    expectedActorEngageRadius: 1.26f,
                    expectedActorAttackDamagePerSecond: 46f,
                    expectedActorAttackIntervalSeconds: 2.1f,
                    expectedActorLifetimeSeconds: 0f,
                    expectedActorAdvanceDistance: 1.6f,
                    expectedScreenIntercepts: 0);
                ValidateSummonSlotReadout(
                    profile,
                    3,
                    "LV3 Inferno Beam",
                    "High-risk high-return dragon that should visibly dominate a punish window.",
                    "Save for the long boss recovery where raw damage matters more than defense.",
                    "Largest hover silhouette, three wide fire chunks, long orange breath beam, and slow high-cost burn pressure.");
                ValidateSummonSlotTier(
                    profile,
                    3,
                    "FireDragon",
                    expectedActorScale: 3.06f,
                    expectedActorMaxHealth: 900f,
                    expectedActorMoveSpeed: 2.95f,
                    expectedActorEngageRadius: 1.36f,
                    expectedActorAttackDamagePerSecond: 68f,
                    expectedActorAttackIntervalSeconds: 2.3f,
                    expectedActorLifetimeSeconds: 0f,
                    expectedActorAdvanceDistance: 1.85f,
                    expectedScreenIntercepts: 0);
            }
        }

        private static void ValidateSupportSummonPressureScreen(
            SummonFrontlineProxy actorPrefab,
            bool expectPressureScreen,
            string slotActionName)
        {
            if (!expectPressureScreen)
            {
                if (actorPrefab.PressureScreen != null)
                {
                    throw new InvalidOperationException($"{slotActionName} should stay a ranged support actor without a pressure screen.");
                }

                return;
            }

            SummonPressureScreen pressureScreen = actorPrefab.PressureScreen;
            if (pressureScreen == null)
            {
                throw new InvalidOperationException($"{slotActionName} must carry a proxy-local pressure screen.");
            }

            if (pressureScreen.transform == actorPrefab.transform)
            {
                throw new InvalidOperationException($"{slotActionName} pressure screen must stay separate from the body hitbox.");
            }

            SphereCollider screenCollider = pressureScreen.GetComponent<SphereCollider>();
            if (screenCollider == null || !screenCollider.isTrigger || screenCollider.radius <= 0f)
            {
                throw new InvalidOperationException($"{slotActionName} pressure screen needs a positive trigger SphereCollider.");
            }

            Rigidbody screenRigidbody = pressureScreen.GetComponent<Rigidbody>();
            if (screenRigidbody == null || !screenRigidbody.isKinematic || screenRigidbody.useGravity)
            {
                throw new InvalidOperationException($"{slotActionName} pressure screen Rigidbody must be kinematic and gravity-free.");
            }

            SummonPressureScreenPresenter screenPresenter =
                actorPrefab.GetComponent<SummonPressureScreenPresenter>();
            if (screenPresenter == null)
            {
                throw new InvalidOperationException($"{slotActionName} pressure screen must have a presenter.");
            }

            Transform pressureScreenVisual = actorPrefab.transform.Find("PressureScreenVisual");
            if (pressureScreenVisual == null)
            {
                throw new InvalidOperationException($"{slotActionName} pressure screen is missing PressureScreenVisual.");
            }

            MeshRenderer pressureScreenRenderer = pressureScreenVisual.GetComponent<MeshRenderer>();
            if (pressureScreenRenderer == null)
            {
                throw new InvalidOperationException($"{slotActionName} PressureScreenVisual is missing a MeshRenderer.");
            }

            ValidateObjectReference(actorPrefab, "pressureScreen", pressureScreen);
            ValidateObjectReference(screenPresenter, "pressureScreen", pressureScreen);
            ValidateObjectReference(screenPresenter, "visualRoot", pressureScreenVisual);
            ValidateArrayReference(screenPresenter, "screenRenderers", 0, pressureScreenRenderer);
            ValidateBool(screenPresenter, "renderVisuals", false);
            if (pressureScreenVisual.gameObject.activeSelf || pressureScreenRenderer.enabled)
            {
                throw new InvalidOperationException($"{slotActionName} PressureScreenVisual should stay inactive during the VFX cleanup pass.");
            }
        }

        private static void ValidateBossPressureLoop(
            BossPressureCostLadder bossPressureCost,
            BossPressureActionDirector bossPressureActionDirector,
            BossBasicFireEmitter bossBasicFireEmitter,
            BossSummonPressureAction bossSummonPressureAction,
            BossPressurePositionController bossPressurePosition,
            SummonLaneSpace laneSpace,
            Transform bossTransform,
            BossBarrageEmitter bossBarrageEmitter,
            Transform playerTransform)
        {
            ValidateObjectReference(bossPressureCost, "laneSpace", laneSpace);
            ValidateObjectReference(bossPressureCost, "trackedBoss", bossTransform);
            ValidateFloat(bossPressureCost, "baseCostPerSecond", BossPressureBaseCostPerSecond);
            ValidateFloat(bossPressureCost, "fallbackBossForwardRisk01", 0.25f);

            ValidateObjectReference(bossPressurePosition, "laneSpace", laneSpace);
            ValidateObjectReference(bossPressurePosition, "costLadder", bossPressureCost);
            ValidateObjectReference(bossPressurePosition, "actionDirector", bossPressureActionDirector);
            ValidateObjectReference(bossPressurePosition, "movedTransform", bossTransform);
            ValidateFloat(bossPressurePosition, "restRisk01", 0.18f);
            ValidateFloat(bossPressurePosition, "maxCommitRisk01", 0.9f);
            ValidateFloat(bossPressurePosition, "advanceRiskPerSecond", 0.46f);
            ValidateFloat(bossPressurePosition, "retreatRiskPerSecond", 0.38f);
            ValidateBool(bossPressurePosition, "returnToRestWhenActionsDisabled", true);
            ValidateBool(bossPressurePosition, "movementEnabled", true);
            ValidateFloat(bossPressurePosition, "actionIntentHoldSeconds", 1.65f);
            ValidateFloat(bossPressurePosition, "holdBacklineRisk01", 0.22f);
            ValidateFloat(bossPressurePosition, "strafeFireRisk01", 0.52f);
            ValidateFloat(bossPressurePosition, "specialCommitRisk01", 0.82f);
            ValidateFloat(bossPressurePosition, "summonRetreatRisk01", 0.1f);
            ValidateFloat(bossPressurePosition, "punishCommitRisk01", 0.9f);
            ValidateBool(bossPressurePosition, "lateralStrafeEnabled", true);
            ValidateFloat(bossPressurePosition, "lateralStrafeUnitsPerSecond", 0.9f);
            ValidateFloat(bossPressurePosition, "lateralStrafeHalfWidthRatio", 0.34f);
            ValidateObjectReference(bossPressurePosition, "trackedPlayer", playerTransform);
            ValidateBool(bossPressurePosition, "playerResponseEnabled", true);
            ValidateFloat(bossPressurePosition, "playerLateralFollowStrength", 0.82f);
            ValidateFloat(bossPressurePosition, "playerResponseHalfWidthRatio", 0.52f);
            ValidateFloat(bossPressurePosition, "playerResponseLateralUnitsPerSecond", 2.6f);
            ValidateFloat(bossPressurePosition, "playerFlankOffsetRatio", 0.18f);
            ValidateFloat(bossPressurePosition, "playerFlankSwitchSeconds", 0.9f);
            ValidateFloat(bossPressurePosition, "commitPlayerFollowBoost", 0.24f);
            ValidateBool(bossPressurePosition, "faceTrackedPlayer", true);
            ValidateFloat(bossPressurePosition, "turnDegreesPerSecond", 780f);
            ValidateBool(bossPressurePosition, "forwardPressureOscillationEnabled", true);
            ValidateFloat(bossPressurePosition, "idleForwardRiskAmplitude", 0.025f);
            ValidateFloat(bossPressurePosition, "actionForwardRiskAmplitude", 0.05f);
            ValidateFloat(bossPressurePosition, "forwardOscillationSeconds", 2.35f);
            ValidateFloat(bossPressurePosition, "commitRiskBoost", 0.04f);
            ValidateFloat(bossPressurePosition, "retreatRiskDip", 0.035f);
            ValidateString(bossPressurePosition, "movementSpeedParameter", "MoveSpeed");
            ValidateString(bossPressurePosition, "alternateMovementSpeedParameter", "Speed");
            ValidateString(bossPressurePosition, "basicFireTrigger", "Attack");
            ValidateString(bossPressurePosition, "retreatStepTrigger", "RetreatBackstep");
            ValidateFloat(bossPressurePosition, "animatorMoveSpeedScale", 0.28f);
            ValidateFloat(bossPressurePosition, "animatorDampSeconds", 0.1f);
            ValidateFloat(bossPressurePosition, "basicFireMovementLockSeconds", 0.34f);
            ValidateFloat(bossPressurePosition, "retreatAnimationRiskDelta", 0.025f);
            ValidateFloat(bossPressurePosition, "retreatTriggerCooldownSeconds", 1.05f);

            ValidateObjectReference(bossSummonPressureAction, "laneSpace", laneSpace);
            ValidateObjectReference(bossSummonPressureAction, "trackedPlayer", playerTransform);
            ValidateObjectReference(
                bossSummonPressureAction,
                "summonActorPrefabObject",
                LoadAsset<GameObject>(BossSummonPressureActorPrefabPath));
            ValidateObjectReference(
                bossSummonPressureAction,
                "summonActorRoot",
                RequireRoot(SceneManager.GetActiveScene(), BossSummonActorPoolRootName).transform);
            ValidateCombatVfxCuePlayerReference(
                bossSummonPressureAction,
                "combatVfxCuePlayer",
                RequireComponent<CombatVfxCuePlayer>(playerTransform.gameObject, "player combat VFX cue player"));
            ValidateEnum(bossSummonPressureAction, "ownerTeam", (int)DamageTeam.Enemy);
            ValidateInt(bossSummonPressureAction, "actorPrewarmCount", 3);
            ValidateInt(bossSummonPressureAction, "maxActiveSummonActors", 2);
            ValidateFloatAtLeast(bossSummonPressureAction, "actorEntryCatchupSecondsPerMeter", 0.3f);
            ValidateFloatAtLeast(bossSummonPressureAction, "minimumPlayerSideTargetDepth", 0.8f);
            BossSummonPressureProfile bossSummonPressureProfile = LoadAsset<BossSummonPressureProfile>(BossSummonPressureProfilePath);
            ValidateObjectReference(
                bossSummonPressureAction,
                "pressureProfile",
                bossSummonPressureProfile);
            EnemySummonPacingDirector enemySummonPacingDirector =
                RequireComponent<EnemySummonPacingDirector>(bossTransform.gameObject, "enemy summon pacing director");
            ValidateObjectReference(enemySummonPacingDirector, "summonPressureAction", bossSummonPressureAction);
            ValidateBool(enemySummonPacingDirector, "pacingEnabled", true);
            ValidateInt(enemySummonPacingDirector, "summonTier", 1);
            ValidateFloat(enemySummonPacingDirector, "initialDelaySeconds", EnemySummonPacingInitialDelaySeconds);
            ValidateFloat(enemySummonPacingDirector, "respawnIntervalSeconds", EnemySummonPacingRespawnIntervalSeconds);
            ValidateFloat(enemySummonPacingDirector, "retryIntervalSeconds", EnemySummonPacingRetryIntervalSeconds);
            ValidateIntArray(enemySummonPacingDirector, "summonTierSequence", BossEnemySummonPacingTierSequence);
            ValidateBossSummonPressureReadout(
                bossSummonPressureProfile,
                1,
                "Response 1 Laser Soldier",
                "Low-cost boss rifleman that creates the first readable dodge-line check without waiting for a high-tier bank.",
                "Read the thin aim line, dodge after the lock, then punish the rifleman before the next boss action.",
                "A cheap summon can body-clash the rifleman, but the primary read is movement first.");
            ValidateBossSummonPressureTier(
                bossSummonPressureProfile,
                1,
                expectedEntryForwardBlend01: 0.28f,
                expectedActorScale: 2.08f,
                expectedActorLifetimeSeconds: 0f,
                expectedActorAdvanceDistance: 2.8f,
                expectedActorRoleId: "LaserSoldier",
                expectedActorMaxHealth: 460f,
                expectedActorMoveSpeed: 3.5f,
                expectedActorEngageRadius: 1.05f,
                expectedActorAttackDamagePerSecond: 34f,
                expectedActorAttackIntervalSeconds: 0.18f,
                expectedScreenIntercepts: 0,
                expectedScreenLifetimeSeconds: 0.2f);
            ValidateBossSummonPressureReadout(
                bossSummonPressureProfile,
                2,
                "Response 2 Pressure Screen",
                "Boss-side summon pressure that contests the frontline for several seconds and blocks player follow-up shots.",
                "Take EN only long enough to prepare a clean response, then break the screen before the next boss pattern layers on top.",
                "Use SummonSlot1 or Vanguard support to absorb the curtain and reopen ranged punish time.");
            ValidateBossSummonPressureTier(
                bossSummonPressureProfile,
                2,
                expectedEntryForwardBlend01: 0.38f,
                expectedActorScale: 2.12f,
                expectedActorLifetimeSeconds: 0f,
                expectedActorAdvanceDistance: 3.8f,
                expectedActorRoleId: "PressureScreen",
                expectedActorMaxHealth: 700f,
                expectedActorMoveSpeed: 3.6f,
                expectedActorEngageRadius: 1.35f,
                expectedActorAttackDamagePerSecond: 62f,
                expectedActorAttackIntervalSeconds: 0.84f,
                expectedScreenIntercepts: 5,
                expectedScreenLifetimeSeconds: 4.0f);
            ValidateBossSummonPressureReadout(
                bossSummonPressureProfile,
                3,
                "Response 3 Laser Soldier",
                "High-cost boss laser summon that creates a dodgeable line threat instead of another pressure screen.",
                "Read the thin line, dodge after the aim locks, then punish during the rifleman's recovery.",
                "Boss laser soldier repositions, draws a cyan warning line, locks aim, then fires a short ticking beam.");
            ValidateBossSummonPressureTier(
                bossSummonPressureProfile,
                3,
                expectedEntryForwardBlend01: 0.5f,
                expectedActorScale: 2.08f,
                expectedActorLifetimeSeconds: 0f,
                expectedActorAdvanceDistance: 4.4f,
                expectedActorRoleId: "LaserSoldier",
                expectedActorMaxHealth: 760f,
                expectedActorMoveSpeed: 4.0f,
                expectedActorEngageRadius: 1.15f,
                expectedActorAttackDamagePerSecond: 58f,
                expectedActorAttackIntervalSeconds: 0.12f,
                expectedScreenIntercepts: 0,
                expectedScreenLifetimeSeconds: 0.2f);

            BossSummonPressureAction.BossSummonTierSettings[] bossSummonTiers =
                bossSummonPressureProfile.CopyTierSettings();
            SummonFrontlineProxy bossLaserSummonActorPrefab =
                LoadPrefabComponent<SummonFrontlineProxy>(BossLaserSummonActorPrefabPath);
            if (bossSummonTiers[0].ActorPrefabOverride != bossLaserSummonActorPrefab
                || bossSummonTiers[2].ActorPrefabOverride != bossLaserSummonActorPrefab)
            {
                throw new InvalidOperationException(
                    "Boss summon pressure response slots 1 and 3 should use the reviewed boss laser summon actor prefab.");
            }

            if (bossSummonTiers[1].ActorPrefabOverride != null)
            {
                throw new InvalidOperationException(
                    "Boss summon pressure response slot 2 should keep the pressure-screen default actor prefab.");
            }

            ValidateFloat(bossLaserSummonActorPrefab, "advanceStartDelaySeconds", 0.16f);
            SummonFrontlineProxyPresenter bossLaserSummonPresenter =
                LoadPrefabComponent<SummonFrontlineProxyPresenter>(BossLaserSummonActorPrefabPath);
            ValidateBool(bossLaserSummonPresenter, "lockAdvanceDuringSpawnState", true);
            ValidateFloat(bossLaserSummonPresenter, "spawnMovementLockSeconds", 0.22f);
            BossLaserSummonPattern bossLaserSummonPattern =
                LoadPrefabComponent<BossLaserSummonPattern>(BossLaserSummonActorPrefabPath);
            ValidateObjectReference(
                bossLaserSummonPattern,
                "telegraphVfxPrefab",
                LoadAsset<GameObject>(BossLaserTelegraphVfxPrefabPath));
            ValidateObjectReference(
                bossLaserSummonPattern,
                "telegraphSfx",
                LoadAsset<AudioClip>(BossLaserTelegraphSfxClipPath));
            ValidateObjectReference(
                bossLaserSummonPattern,
                "laserFireSfx",
                LoadAsset<AudioClip>(BossLaserFireSfxClipPath));
            ValidateFloat(bossLaserSummonPattern, "telegraphSfxVolume", 0.72f);
            ValidateFloat(bossLaserSummonPattern, "laserFireSfxVolume", 0.9f);
            ValidateFloat(bossLaserSummonPattern, "retargetSettleSeconds", 0.18f);
            ValidateFloat(bossLaserSummonPattern, "aimTurnSpeedDegrees", 720f);
            ValidateColor(bossLaserSummonPattern, "telegraphStartColor", new Color(1f, 0.18f, 0.08f, 0.26f));
            ValidateColor(bossLaserSummonPattern, "telegraphEndColor", new Color(1f, 0.28f, 0.12f, 0.96f));

            SummonFrontlineProxy bossSummonActorPrefab =
                LoadPrefabComponent<SummonFrontlineProxy>(BossSummonPressureActorPrefabPath);
            SummonFrontlineProxyPresenter bossSummonActorPresenter =
                LoadPrefabComponent<SummonFrontlineProxyPresenter>(BossSummonPressureActorPrefabPath);
            SummonFrontlineClash bossSummonClash =
                LoadPrefabComponent<SummonFrontlineClash>(BossSummonPressureActorPrefabPath);
            CombatHealth bossSummonHealth = LoadPrefabComponent<CombatHealth>(BossSummonPressureActorPrefabPath);
            Transform bossSummonVisual = ValidateSummonActorRoleVisual(
                bossSummonActorPrefab.gameObject,
                BossSummonPressureActorVisualName);
            Renderer[] bossSummonVisualRenderers = CollectEnabledRenderers(bossSummonVisual.gameObject);
            Transform tierPressureCore = bossSummonActorPrefab.transform.Find("TierPressureCore");
            if (tierPressureCore == null)
            {
                throw new InvalidOperationException("Boss summon pressure actor prefab is missing TierPressureCore.");
            }

            MeshRenderer tierPressureRenderer = tierPressureCore.GetComponent<MeshRenderer>();
            if (tierPressureRenderer == null)
            {
                throw new InvalidOperationException("TierPressureCore is missing a MeshRenderer.");
            }

            MeshRenderer bossRootRenderer = bossSummonActorPrefab.GetComponent<MeshRenderer>();
            if (bossRootRenderer != null && bossRootRenderer.enabled)
            {
                throw new InvalidOperationException(
                    "Boss summon pressure actor root mesh renderer must stay disabled so the promoted model reads first.");
            }

            ValidateSummonActorVfx(
                bossSummonActorPrefab.gameObject,
                "TierPressureCore",
                expectPressureScreen: true,
                label: "Boss summon pressure actor prefab");
            ValidateObjectReference(bossSummonActorPresenter, "proxy", bossSummonActorPrefab);
            ValidateObjectReference(bossSummonActorPresenter, "clash", bossSummonClash);
            ValidateObjectReference(bossSummonActorPresenter, "pulseRoot", tierPressureCore);
            ValidateBool(bossSummonActorPresenter, "renderPulseVisuals", false);
            ValidateObjectReference(bossSummonActorPrefab, "health", bossSummonHealth);
            ValidateObjectReference(bossSummonClash, "proxy", bossSummonActorPrefab);
            ValidateObjectReference(bossSummonClash, "health", bossSummonHealth);
            ValidateSummonActorBodyContract(
                bossSummonActorPrefab.gameObject,
                bossSummonActorPrefab,
                bossSummonClash,
                bossSummonHealth,
                DamageTeam.Enemy,
                "Boss summon pressure actor prefab");
            ValidateSummonHealthBar(
                bossSummonActorPrefab.gameObject,
                bossSummonActorPrefab,
                bossSummonHealth,
                "Boss summon pressure actor prefab");
            if (bossSummonVisualRenderers.Length == 0)
            {
                throw new InvalidOperationException(
                    $"{BossSummonPressureActorVisualName} should expose at least one enabled renderer.");
            }

            ValidatePulseOnlyActorRenderers(bossSummonActorPresenter, tierPressureRenderer, "TierPressureCore");
            ValidateSummonActorDamageFlashRenderers(
                bossSummonActorPresenter,
                bossSummonVisual,
                "Boss summon pressure body flash");
            ValidateSummonActorAnimatorPresentation(
                bossSummonActorPresenter,
                bossSummonVisual,
                "Boss summon pressure actor prefab",
                expectedAnimatorMoveSpeedScale: 0.52f);
            ValidateFloat(bossSummonActorPresenter, "clashFlashSeconds", 0.14f);
            ValidateFloat(bossSummonActorPresenter, "clashFlashScale", 0.18f);

            ValidateObjectReference(bossPressureActionDirector, "costLadder", bossPressureCost);
            ValidateObjectReference(bossPressureActionDirector, "bossBarrageEmitter", bossBarrageEmitter);
            ValidateObjectReference(bossPressureActionDirector, "basicFireEmitter", bossBasicFireEmitter);
            ValidateObjectReference(bossPressureActionDirector, "summonPressureAction", bossSummonPressureAction);
            ValidateObjectReference(bossPressureActionDirector, "laneSpace", laneSpace);
            ValidateObjectReference(bossPressureActionDirector, "trackedPlayer", playerTransform);
            ValidateObjectReference(
                bossPressureActionDirector,
                "actionDeckProfile",
                LoadAsset<BossPressureActionDeckProfile>(BossPressureActionDeckProfilePath));
            ValidateBool(bossPressureActionDirector, "actionsEnabled", true);
            ValidateBool(bossPressureActionDirector, "holdForNextTierActionWhenGateAllows", true);
            ValidateFloat(bossPressureActionDirector, "globalRecoverySeconds", 1.65f);
            ValidateFloat(bossPressureActionDirector, "decisionThinkIntervalSeconds", 0.25f);
            ValidateFloat(bossPressureActionDirector, "playerSummonResponseWindowSeconds", 4f);
            ValidateFloat(bossPressureActionDirector, "basicFireSuppressionSecondsAfterPressureAction", 0.2f);
            ValidateInt(bossPressureActionDirector, "minimumBasicFireVolleysBeforePressureAction", 4);
            ValidateFloat(bossPressureActionDirector, "minimumBasicFireAgeBeforePressureActionSeconds", 0.08f);
            ValidateBossPressureActionSlot(
                bossPressureActionDirector,
                0,
                LoadAsset<BossBarragePatternProfile>(LinePressurePatternProfilePath),
                BossPressureActionKind.SpecialSkill,
                1,
                "DodgeBossLinePressureSpecial",
                "LV1 boss special shot that asks the player to read a committed rail before spending summon resources.",
                "Strafe or dodge out of the rail, then punish with ranged fire when the lane is clear.",
                "No summon is required; enemy summon pressure now runs on its own pacing lane.",
                false,
                0f,
                1f,
                false,
                1,
                15,
                0,
                0,
                BossPressureMovementIntent.StrafeFire);
            ValidateBossPressureActionSlot(
                bossPressureActionDirector,
                1,
                LoadAsset<BossBarragePatternProfile>(PunishNetPatternProfilePath),
                BossPressureActionKind.PunishOverextend,
                3,
                "RetreatOrSpendHighTierAnswer",
                "LV3 overextend punish that closes gaps when the player stays near the forward boundary too long.",
                "Retreat from forward-risk space or dodge through the shrinking net before firing back.",
                "A prepared high-tier summon screen can buy the follow-up window, but it should cost the player's stored EN.",
                true,
                0.66f,
                1f,
                false,
                1,
                80,
                80,
                0,
                BossPressureMovementIntent.CommitForward);
        }

        private static void ValidateBossPressureActionSlot(
            BossPressureActionDirector bossPressureActionDirector,
            int index,
            BossBarragePatternProfile expectedPattern,
            BossPressureActionKind expectedKind,
            int expectedMinimumTier,
            string expectedResponseId,
            string expectedStageLoopRole,
            string expectedPlayerAnswer,
            string expectedSummonAnswer,
            bool expectedUsePlayerForwardRiskGate,
            float expectedMinimumPlayerForwardRisk01,
            float expectedMaximumPlayerForwardRisk01,
            bool expectedUsePlayerSummonResponseGate = false,
            int expectedMinimumPlayerSummonTier = 1,
            int expectedSelectionPriority = 0,
            int expectedForwardRiskPriorityBonus = 0,
            int expectedSummonResponsePriorityBonus = 0,
            BossPressureMovementIntent expectedMovementIntent = BossPressureMovementIntent.CostPressure)
        {
            if (!bossPressureActionDirector.TryGetActionSlot(
                    index,
                    out BossPressureActionDirector.BossPressureActionSlot slot))
            {
                throw new InvalidOperationException($"Boss pressure action slot {index} is missing.");
            }

            if (slot.Pattern != expectedPattern)
            {
                throw new InvalidOperationException($"Boss pressure action slot {index} points to the wrong pattern.");
            }

            if (slot.ActionKind != expectedKind)
            {
                throw new InvalidOperationException($"Boss pressure action slot {index} has the wrong action kind.");
            }

            if (slot.MinimumTier != expectedMinimumTier)
            {
                throw new InvalidOperationException($"Boss pressure action slot {index} has the wrong minimum tier.");
            }

            if (!slot.HasResponsePlan)
            {
                throw new InvalidOperationException($"Boss pressure action slot {index} is missing its response plan.");
            }

            ValidateString(slot.ResponseId, expectedResponseId, $"Boss pressure action slot {index} has the wrong response id.");
            ValidateString(slot.StageLoopRole, expectedStageLoopRole, $"Boss pressure action slot {index} has the wrong stage-loop role.");
            ValidateString(slot.PlayerAnswer, expectedPlayerAnswer, $"Boss pressure action slot {index} has the wrong player answer.");
            ValidateString(slot.SummonAnswer, expectedSummonAnswer, $"Boss pressure action slot {index} has the wrong summon answer.");

            if (slot.UsePlayerForwardRiskGate != expectedUsePlayerForwardRiskGate)
            {
                throw new InvalidOperationException($"Boss pressure action slot {index} has the wrong player risk gate setting.");
            }

            if (!Mathf.Approximately(slot.MinimumPlayerForwardRisk01, expectedMinimumPlayerForwardRisk01)
                || !Mathf.Approximately(slot.MaximumPlayerForwardRisk01, expectedMaximumPlayerForwardRisk01))
            {
                throw new InvalidOperationException($"Boss pressure action slot {index} has the wrong player risk gate range.");
            }

            if (slot.UsePlayerSummonResponseGate != expectedUsePlayerSummonResponseGate)
            {
                throw new InvalidOperationException($"Boss pressure action slot {index} has the wrong player summon response gate setting.");
            }

            if (slot.MinimumPlayerSummonTier != expectedMinimumPlayerSummonTier)
            {
                throw new InvalidOperationException($"Boss pressure action slot {index} has the wrong player summon response tier.");
            }

            if (slot.SelectionPriority != expectedSelectionPriority)
            {
                throw new InvalidOperationException($"Boss pressure action slot {index} has the wrong selection priority.");
            }

            if (slot.ForwardRiskPriorityBonus != expectedForwardRiskPriorityBonus)
            {
                throw new InvalidOperationException($"Boss pressure action slot {index} has the wrong forward-risk priority bonus.");
            }

            if (slot.SummonResponsePriorityBonus != expectedSummonResponsePriorityBonus)
            {
                throw new InvalidOperationException($"Boss pressure action slot {index} has the wrong summon-response priority bonus.");
            }

            if (slot.MovementIntent != expectedMovementIntent)
            {
                throw new InvalidOperationException($"Boss pressure action slot {index} has the wrong movement intent.");
            }
        }

        private static void ValidateSummonSlotReadout(
            SummonSlotActionProfile profile,
            int tier,
            string expectedTierLabel,
            string expectedStageRole,
            string expectedPlayerUse,
            string expectedSummonRead)
        {
            if (profile == null)
            {
                throw new InvalidOperationException("SummonSlot1 action profile is missing.");
            }

            if (!profile.TryGetTierReadout(tier, out SummonSlotActionProfile.SummonTierReadout readout))
            {
                throw new InvalidOperationException($"SummonSlot1 profile is missing tier {tier} readout.");
            }

            ValidateString(readout.TierLabel, expectedTierLabel, $"SummonSlot1 tier {tier} has the wrong label.");
            ValidateString(readout.StageRole, expectedStageRole, $"SummonSlot1 tier {tier} has the wrong stage role.");
            ValidateString(readout.PlayerUse, expectedPlayerUse, $"SummonSlot1 tier {tier} has the wrong player-use note.");
            ValidateString(readout.SummonRead, expectedSummonRead, $"SummonSlot1 tier {tier} has the wrong summon-read note.");
        }

        private static void ValidateBossSummonPressureReadout(
            BossSummonPressureProfile profile,
            int tier,
            string expectedTierLabel,
            string expectedStageRole,
            string expectedPlayerRead,
            string expectedSummonRead)
        {
            if (profile == null)
            {
                throw new InvalidOperationException("Boss summon pressure profile is missing.");
            }

            if (!profile.TryGetResponseSlotReadout(tier, out BossSummonPressureProfile.BossSummonTierReadout readout))
            {
                throw new InvalidOperationException($"Boss summon pressure profile is missing tier {tier} readout.");
            }

            ValidateString(readout.TierLabel, expectedTierLabel, $"Boss summon pressure response slot {tier} has the wrong label.");
            ValidateString(readout.StageRole, expectedStageRole, $"Boss summon pressure response slot {tier} has the wrong stage role.");
            ValidateString(readout.PlayerRead, expectedPlayerRead, $"Boss summon pressure response slot {tier} has the wrong player-read note.");
            ValidateString(readout.SummonRead, expectedSummonRead, $"Boss summon pressure response slot {tier} has the wrong summon-read note.");
        }

        private static void ValidateBossSummonPressureTier(
            BossSummonPressureProfile profile,
            int tier,
            float expectedEntryForwardBlend01,
            float expectedActorScale,
            float expectedActorLifetimeSeconds,
            float expectedActorAdvanceDistance,
            string expectedActorRoleId,
            float expectedActorMaxHealth,
            float expectedActorMoveSpeed,
            float expectedActorEngageRadius,
            float expectedActorAttackDamagePerSecond,
            float expectedActorAttackIntervalSeconds,
            int expectedScreenIntercepts,
            float expectedScreenLifetimeSeconds)
        {
            if (profile == null)
            {
                throw new InvalidOperationException("Boss summon pressure profile is missing.");
            }

            BossSummonPressureAction.BossSummonTierSettings[] tierSettings = profile.CopyTierSettings();
            int index = tier - 1;
            if (index < 0 || index >= tierSettings.Length)
            {
                throw new InvalidOperationException($"Boss summon pressure profile is missing tier {tier} settings.");
            }

            BossSummonPressureAction.BossSummonTierSettings settings = tierSettings[index];
            if (!string.Equals(settings.ActorRoleId, expectedActorRoleId, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Boss summon pressure response slot {tier} has the wrong actor role id.");
            }

            ValidateFloatValue(
                settings.EntryForwardBlend01,
                expectedEntryForwardBlend01,
                $"Boss summon pressure response slot {tier} has the wrong entry forward blend.");
            ValidateFloatValue(
                settings.ActorLifetimeSeconds,
                expectedActorLifetimeSeconds,
                $"Boss summon pressure response slot {tier} has the wrong actor lifetime.");
            ValidateFloatValue(
                settings.ActorScale,
                expectedActorScale,
                $"Boss summon pressure response slot {tier} has the wrong actor scale.");
            ValidateFloatValue(
                settings.ActorAdvanceDistance,
                expectedActorAdvanceDistance,
                $"Boss summon pressure response slot {tier} has the wrong actor advance distance.");
            ValidateFloatValue(
                settings.ActorMaxHealth,
                expectedActorMaxHealth,
                $"Boss summon pressure response slot {tier} has the wrong actor max health.");
            ValidateFloatValue(
                settings.ActorMoveSpeed,
                expectedActorMoveSpeed,
                $"Boss summon pressure response slot {tier} has the wrong actor move speed.");
            ValidateFloatValue(
                settings.ActorEngageRadius,
                expectedActorEngageRadius,
                $"Boss summon pressure response slot {tier} has the wrong actor engage radius.");
            ValidateFloatValue(
                settings.ActorAttackDamagePerSecond,
                expectedActorAttackDamagePerSecond,
                $"Boss summon pressure response slot {tier} has the wrong actor attack damage.");
            ValidateFloatValue(
                settings.ActorAttackIntervalSeconds,
                expectedActorAttackIntervalSeconds,
                $"Boss summon pressure response slot {tier} has the wrong actor attack interval.");
            if (settings.ScreenIntercepts != expectedScreenIntercepts)
            {
                throw new InvalidOperationException(
                    $"Boss summon pressure response slot {tier} has the wrong screen intercept count.");
            }

            ValidateFloatValue(
                settings.ScreenLifetimeSeconds,
                expectedScreenLifetimeSeconds,
                $"Boss summon pressure response slot {tier} has the wrong screen lifetime.");
        }

        private static void ValidateSummonSlotTier(
            SummonSlotActionProfile profile,
            int tier,
            string expectedActorRoleId,
            float expectedActorScale,
            float expectedActorMaxHealth,
            float expectedActorMoveSpeed,
            float expectedActorEngageRadius,
            float expectedActorAttackDamagePerSecond,
            float expectedActorAttackIntervalSeconds,
            float expectedActorLifetimeSeconds,
            float expectedActorAdvanceDistance,
            int expectedScreenIntercepts,
            float expectedScreenRadius = -1f,
            float expectedScreenLifetimeSeconds = -1f,
            float expectedCounterDamage = -1f)
        {
            if (profile == null)
            {
                throw new InvalidOperationException("Summon slot action profile is missing.");
            }

            PlayerSummonSlot1Action.SummonTierSettings[] tierSettings = profile.CopyTierSettings();
            int index = tier - 1;
            if (index < 0 || index >= tierSettings.Length)
            {
                throw new InvalidOperationException($"Summon slot action profile is missing tier {tier} settings.");
            }

            PlayerSummonSlot1Action.SummonTierSettings settings = tierSettings[index];
            if (!string.Equals(settings.ActorRoleId, expectedActorRoleId, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Summon slot tier {tier} has the wrong actor role id.");
            }

            ValidateFloatValue(
                settings.ActorScale,
                expectedActorScale,
                $"Summon slot tier {tier} has the wrong actor scale.");
            ValidateFloatValue(
                settings.ActorMaxHealth,
                expectedActorMaxHealth,
                $"Summon slot tier {tier} has the wrong actor max health.");
            ValidateFloatValue(
                settings.ActorMoveSpeed,
                expectedActorMoveSpeed,
                $"Summon slot tier {tier} has the wrong actor move speed.");
            ValidateFloatValue(
                settings.ActorEngageRadius,
                expectedActorEngageRadius,
                $"Summon slot tier {tier} has the wrong actor engage radius.");
            ValidateFloatValue(
                settings.ActorAttackDamagePerSecond,
                expectedActorAttackDamagePerSecond,
                $"Summon slot tier {tier} has the wrong actor attack damage.");
            ValidateFloatValue(
                settings.ActorAttackIntervalSeconds,
                expectedActorAttackIntervalSeconds,
                $"Summon slot tier {tier} has the wrong actor attack interval.");
            ValidateFloatValue(
                settings.ActorLifetimeSeconds,
                expectedActorLifetimeSeconds,
                $"Summon slot tier {tier} has the wrong actor lifetime.");
            ValidateFloatValue(
                settings.ActorAdvanceDistance,
                expectedActorAdvanceDistance,
                $"Summon slot tier {tier} has the wrong actor advance distance.");
            if (settings.ScreenIntercepts != expectedScreenIntercepts)
            {
                throw new InvalidOperationException($"Summon slot tier {tier} has the wrong screen intercept count.");
            }

            if (expectedScreenRadius >= 0f)
            {
                ValidateFloatValue(
                    settings.ScreenRadius,
                    expectedScreenRadius,
                    $"Summon slot tier {tier} has the wrong screen radius.");
            }

            if (expectedScreenLifetimeSeconds >= 0f)
            {
                ValidateFloatValue(
                    settings.ScreenLifetimeSeconds,
                    expectedScreenLifetimeSeconds,
                    $"Summon slot tier {tier} has the wrong screen lifetime.");
            }

            if (expectedCounterDamage >= 0f)
            {
                ValidateFloatValue(
                    settings.CounterDamage,
                    expectedCounterDamage,
                    $"Summon slot tier {tier} has the wrong counter damage.");
            }
        }

        private static void ValidateCloseThreat(
            GameObject closeThreat,
            CombatHealth closeThreatHealth,
            CombatHealth playerHealth,
            ActionCameraController cameraController)
        {
            BasicSoldierEnemy soldier = RequireComponent<BasicSoldierEnemy>(closeThreat, "close threat soldier");
            CombatTargetSensor targetSensor = RequireComponent<CombatTargetSensor>(closeThreat, "close threat target sensor");
            EnemyActionCameraCueDriver cameraCueDriver =
                RequireComponent<EnemyActionCameraCueDriver>(closeThreat, "close threat camera cue driver");

            ValidateObjectReference(targetSensor, "selfHealth", closeThreatHealth);
            ValidateArrayReference(targetSensor, "targetCandidates", 0, playerHealth);
            ValidateObjectReference(soldier, "targetSensor", targetSensor);
            ValidateObjectReference(soldier, "selfHealth", closeThreatHealth);
            ValidateObjectReference(cameraCueDriver, "agentSource", soldier);
            ValidateObjectReference(cameraCueDriver, "cameraController", cameraController);
            ValidateObjectReference(cameraCueDriver, "cueSpace", closeThreat.transform);
            ValidateFloat(closeThreatHealth, "maxHealth", 72f);
            ValidateCloseThreatBodyContract(closeThreat, closeThreatHealth);
        }

        private static void ValidateCloseThreatBodyContract(GameObject closeThreat, CombatHealth closeThreatHealth)
        {
            if (closeThreat.GetComponent<CombatHealth>() != closeThreatHealth)
            {
                throw new InvalidOperationException("Close-threat health must stay on the root body object.");
            }

            if (!closeThreat.activeSelf)
            {
                throw new InvalidOperationException("Close threat must stay active in the lane review scene.");
            }

            Transform hitbox = closeThreat.transform.Find(CloseThreatBodyHitboxName);
            if (hitbox == null)
            {
                throw new InvalidOperationException($"Close threat must keep child hitbox {CloseThreatBodyHitboxName}.");
            }

            SphereCollider bodyCollider = RequireComponent<SphereCollider>(hitbox.gameObject, "close threat body collider");
            if (bodyCollider.isTrigger)
            {
                throw new InvalidOperationException("Close-threat body collider must be a solid child collider for local defense and ranged aim-assist hits.");
            }

            if (bodyCollider.radius < CloseThreatBodyHitboxRadius - 0.001f)
            {
                throw new InvalidOperationException("Close-threat body collider radius is too small for local defense hits.");
            }

            if ((bodyCollider.center - CloseThreatBodyHitboxCenter).sqrMagnitude > 0.0001f)
            {
                throw new InvalidOperationException("Close-threat body collider center must stay aligned to the readable humanoid body.");
            }

            Rigidbody bodyRigidbody = RequireComponent<Rigidbody>(closeThreat, "close threat body Rigidbody");
            if (!bodyRigidbody.isKinematic || bodyRigidbody.useGravity)
            {
                throw new InvalidOperationException("Close-threat body Rigidbody must be kinematic and gravity-free for reliable hit dispatch.");
            }

            bool hasCombatHitCollider = false;
            Collider[] colliders = closeThreat.GetComponentsInChildren<Collider>(includeInactive: true);
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] == null)
                {
                    continue;
                }

                CombatHealth parentHealth = colliders[i].GetComponentInParent<CombatHealth>();
                if (parentHealth == closeThreatHealth)
                {
                    hasCombatHitCollider = true;
                    break;
                }
            }

            if (!hasCombatHitCollider)
            {
                throw new InvalidOperationException("Close threat must expose at least one collider under its CombatHealth root.");
            }
        }

        private static void ValidateBossProxyVisual(GameObject bossProxy, CombatVfxCuePlayer expectedPlayerCuePlayer = null)
        {
            Transform visual = bossProxy.transform.Find(BossProxyHumanoidVisualName);
            if (visual == null)
            {
                throw new InvalidOperationException($"Boss proxy should include {BossProxyHumanoidVisualName}.");
            }

            Animator animator = visual.GetComponent<Animator>();
            if (animator == null || animator.runtimeAnimatorController == null)
            {
                throw new InvalidOperationException($"{BossProxyHumanoidVisualName} should keep the reviewed boss cue Animator.");
            }

            string controllerPath = AssetDatabase.GetAssetPath(animator.runtimeAnimatorController).Replace('\\', '/');
            if (!string.Equals(controllerPath, ActionFoundationSciFiSoldier01VisualSetup.ControllerPath, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"{BossProxyHumanoidVisualName} should use {ActionFoundationSciFiSoldier01VisualSetup.ControllerPath}, found {controllerPath}.");
            }

            ValidateGameOwnedAsset(animator.runtimeAnimatorController, $"{BossProxyHumanoidVisualName} Animator Controller");

            if (visual.GetComponentInChildren<CombatHealth>(true) != null
                || visual.GetComponentInChildren<BasicSoldierEnemy>(true) != null
                || visual.GetComponentInChildren<CombatTargetSensor>(true) != null
                || visual.GetComponentInChildren<EnemyElitePatternController>(true) != null)
            {
                throw new InvalidOperationException($"{BossProxyHumanoidVisualName} must be visual-only and must not duplicate enemy gameplay components.");
            }

            Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(includeInactive: true);
            if (renderers.Length == 0)
            {
                throw new InvalidOperationException($"{BossProxyHumanoidVisualName} should expose source Commando renderers.");
            }

            for (int i = 0; i < renderers.Length; i++)
            {
                ValidateBossProxyCommandoRendererAssets(renderers[i], $"{BossProxyHumanoidVisualName}.{renderers[i].name}");
            }

            ValidateBossProxyCommandoPrefabSource(visual.gameObject);
            ValidateBossProxyCommandoMeshSource(visual);

            Transform projectileCore = bossProxy.transform.Find(BossProxyMarkerName);
            if (projectileCore == null)
            {
                throw new InvalidOperationException($"Boss proxy should include {BossProxyMarkerName} as the hidden projectile source anchor.");
            }

            MeshRenderer projectileCoreRenderer = projectileCore.GetComponent<MeshRenderer>();
            if (projectileCoreRenderer == null || projectileCoreRenderer.sharedMaterial == null)
            {
                throw new InvalidOperationException($"{BossProxyMarkerName} should keep a game-owned material.");
            }

            if (projectileCoreRenderer.enabled)
            {
                throw new InvalidOperationException($"{BossProxyMarkerName} renderer should stay disabled; it is a VFX/projectile anchor, not an in-game marker.");
            }

            ValidateGameOwnedAsset(projectileCoreRenderer.sharedMaterial, $"{BossProxyMarkerName} material");

            BossBarrageVisualCueDriver cueDriver = RequireComponent<BossBarrageVisualCueDriver>(
                bossProxy,
                "boss barrage visual cue driver");
            BossBarrageEmitter emitter = RequireComponent<BossBarrageEmitter>(bossProxy, "boss barrage emitter");
            BossPressureActionDirector bossPressureActionDirector =
                RequireComponent<BossPressureActionDirector>(bossProxy, "boss pressure action director");
            if (cueDriver.BossBarrageEmitter != emitter)
            {
                throw new InvalidOperationException("Boss visual cue driver should read from the boss barrage emitter.");
            }

            if (cueDriver.BossPressureActionDirector != bossPressureActionDirector)
            {
                throw new InvalidOperationException("Boss visual cue driver should read boss pressure action selections.");
            }

            if (cueDriver.Animator != animator)
            {
                throw new InvalidOperationException("Boss visual cue driver should drive the source Commando humanoid Animator.");
            }

            if (cueDriver.PulseRoot != projectileCore)
            {
                throw new InvalidOperationException("Boss visual cue driver should pulse the authored projectile source core.");
            }

            CombatVfxCuePlayer playerCuePlayer =
                expectedPlayerCuePlayer != null
                    ? expectedPlayerCuePlayer
                    : ResolveScenePlayerCuePlayer(bossProxy.scene, cueDriver.CuePlayer);
            if (playerCuePlayer == null || cueDriver.CuePlayer != playerCuePlayer)
            {
                throw new InvalidOperationException("Boss visual cue driver should request promoted world VFX through the player combat VFX cue player.");
            }

            if (cueDriver.VfxAnchor != projectileCore)
            {
                throw new InvalidOperationException("Boss visual cue driver should anchor promoted world VFX at the projectile source core.");
            }

            if (cueDriver.VfxDirectionTarget == null)
            {
                throw new InvalidOperationException("Boss visual cue driver should aim promoted world VFX toward the player side.");
            }

            if (cueDriver.WindupCueId != CombatVfxCueId.EliteAuraSignal
                || cueDriver.ReleaseCueId != CombatVfxCueId.EnemyRetreatShotActive
                || cueDriver.SkillPressureCueId != CombatVfxCueId.EnemyLinePressureWindup
                || cueDriver.SummonPressureCueId != CombatVfxCueId.EliteSummonSignal
                || cueDriver.PunishPressureCueId != CombatVfxCueId.EliteArmorBreakSignal)
            {
                throw new InvalidOperationException("Boss visual cue driver should use promoted combat VFX cues for windup, release, and pressure states.");
            }

            if (cueDriver.PatternCueCount < 10)
            {
                throw new InvalidOperationException("Boss visual cue driver should map every current boss barrage pattern.");
            }

            ValidateBossVisualCueBindings(cueDriver, animator);
            ValidateBossPressureActionCueBindings(cueDriver, animator);

            if (cueDriver.PulseRendererCount <= 0)
            {
                throw new InvalidOperationException("Boss visual cue driver should have at least one pulse renderer.");
            }
        }

        private static void ValidateBossProxyCommandoMeshSource(Transform visual)
        {
            bool foundSourceCommandoMesh = false;
            SkinnedMeshRenderer[] skinnedRenderers = visual.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true);
            for (int i = 0; i < skinnedRenderers.Length; i++)
            {
                Mesh mesh = skinnedRenderers[i].sharedMesh;
                if (mesh == null)
                {
                    continue;
                }

                string meshPath = AssetDatabase.GetAssetPath(mesh).Replace('\\', '/');
                if (string.Equals(meshPath, BossProxyLineCasterVariantModelPath, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        $"{BossProxyHumanoidVisualName} should not use the LineCaster role variant mesh: {meshPath}.");
                }

                if (string.Equals(meshPath, BossProxyHumanoidSourceModelPath, StringComparison.Ordinal))
                {
                    foundSourceCommandoMesh = true;
                }
            }

            if (!foundSourceCommandoMesh)
            {
                throw new InvalidOperationException(
                    $"{BossProxyHumanoidVisualName} should use the source SciFiSoldier_01_Commando model at {BossProxyHumanoidSourceModelPath}.");
            }

            Transform assaultRifle = FindDescendant(visual, BossProxyHumanoidSourceAssaultRifleName);
            if (assaultRifle == null)
            {
                throw new InvalidOperationException(
                    $"{BossProxyHumanoidVisualName} should carry {BossProxyHumanoidSourceAssaultRifleName} from the source Commando prefab.");
            }

            MeshFilter[] assaultRifleMeshes = assaultRifle.GetComponentsInChildren<MeshFilter>(includeInactive: true);
            bool foundAssaultRifleMesh = false;
            for (int i = 0; i < assaultRifleMeshes.Length; i++)
            {
                Mesh mesh = assaultRifleMeshes[i].sharedMesh;
                if (mesh == null)
                {
                    continue;
                }

                string meshPath = AssetDatabase.GetAssetPath(mesh).Replace('\\', '/');
                if (string.Equals(meshPath, BossProxyHumanoidSourceAssaultRifleModelPath, StringComparison.Ordinal))
                {
                    foundAssaultRifleMesh = true;
                }
            }

            if (!foundAssaultRifleMesh)
            {
                throw new InvalidOperationException(
                    $"{BossProxyHumanoidVisualName} assault rifle should render {BossProxyHumanoidSourceAssaultRifleModelPath}.");
            }
        }

        private static void ValidateBossProxyBodyContract(GameObject bossProxy, CombatHealth bossHealth)
        {
            if (bossProxy.GetComponent<CombatHealth>() != bossHealth)
            {
                throw new InvalidOperationException("Boss proxy health must stay on the root body object.");
            }

            SphereCollider bodyCollider = RequireComponent<SphereCollider>(bossProxy, "boss proxy body collider");
            if (bodyCollider.isTrigger)
            {
                throw new InvalidOperationException("Boss proxy body collider must be a solid root collider for summon body contacts.");
            }

            if (bodyCollider.radius < BossProxyBodyHitboxRadius - 0.001f)
            {
                throw new InvalidOperationException("Boss proxy body collider radius is too small for frontline summon contact.");
            }

            if ((bodyCollider.center - BossProxyBodyHitboxCenter).sqrMagnitude > 0.0001f)
            {
                throw new InvalidOperationException("Boss proxy body collider center must stay aligned to the readable humanoid body.");
            }

            Rigidbody bodyRigidbody = RequireComponent<Rigidbody>(bossProxy, "boss proxy body Rigidbody");
            if (!bodyRigidbody.isKinematic || bodyRigidbody.useGravity)
            {
                throw new InvalidOperationException("Boss proxy body Rigidbody must be kinematic and gravity-free for moving trigger contacts.");
            }
        }

        private static Transform AttachRoleVisualOnly(
            Transform parent,
            string roleId,
            string rolePrefabPath,
            string targetVisualName,
            Vector3 localPosition,
            Vector3 localEulerAngles,
            Vector3 localScale)
        {
            Transform existingVisual = parent.Find(targetVisualName);
            if (existingVisual != null)
            {
                existingVisual.localPosition = localPosition;
                existingVisual.localRotation = Quaternion.Euler(localEulerAngles);
                existingVisual.localScale = localScale;
                RemapRoleVisualImportedDependencies(existingVisual.gameObject);
                ValidateSummonActorRoleVisualContents(existingVisual.gameObject, targetVisualName);
                return existingVisual;
            }

            string visualPrefix = targetVisualName.Contains("_", StringComparison.Ordinal)
                ? targetVisualName.Substring(0, targetVisualName.LastIndexOf('_') + 1)
                : targetVisualName;
            RemoveChildrenWithPrefix(parent, visualPrefix);

            EnemyRoleVisualSpec visualSpec = ActionFoundationEnemyRoleVisualSetup.CreateForRole(roleId);
            GameObject prefabContents = PrefabUtility.LoadPrefabContents(rolePrefabPath);
            try
            {
                Transform sourceVisual = prefabContents.transform.Find(visualSpec.VisualName);
                if (sourceVisual == null)
                {
                    throw new InvalidOperationException($"{rolePrefabPath} is missing {visualSpec.VisualName}.");
                }

                GameObject visual = UnityEngine.Object.Instantiate(sourceVisual.gameObject);
                visual.name = targetVisualName;
                visual.transform.SetParent(parent, worldPositionStays: false);
                visual.transform.localPosition = localPosition;
                visual.transform.localRotation = Quaternion.Euler(localEulerAngles);
                visual.transform.localScale = localScale;
                RemapRoleVisualImportedDependencies(visual);
                ValidateSummonActorRoleVisualContents(visual, targetVisualName);
                return visual.transform;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabContents);
            }
        }

        private static void RemapRoleVisualImportedDependencies(GameObject visual)
        {
            if (visual == null)
            {
                return;
            }

            Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(includeInactive: true);
            for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
            {
                Material[] materials = renderers[rendererIndex].sharedMaterials;
                for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                {
                    Material material = materials[materialIndex];
                    if (material == null)
                    {
                        continue;
                    }

                    RemapImportedMaterialTextures(material, SummonRoleVisualTextureRoot);
                    RemapImportedSerializedMaterialTextures(material, SummonRoleVisualTextureRoot);
                    EditorUtility.SetDirty(material);
                    string materialPath = AssetDatabase.GetAssetPath(material).Replace('\\', '/');
                    if (!string.IsNullOrWhiteSpace(materialPath)
                        && materialPath.StartsWith("Assets/_Game/", StringComparison.Ordinal))
                    {
                        AssetDatabase.ImportAsset(materialPath, ImportAssetOptions.ForceUpdate);
                    }
                }
            }

            AssetDatabase.SaveAssets();
        }

        private static void RemoveChildrenWithPrefix(Transform parent, string prefix)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Transform child = parent.GetChild(i);
                if (child.name.StartsWith(prefix, StringComparison.Ordinal))
                {
                    UnityEngine.Object.DestroyImmediate(child.gameObject);
                }
            }
        }

        private static Transform FindChildWithPrefix(Transform parent, string prefix)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child.name.StartsWith(prefix, StringComparison.Ordinal))
                {
                    return child;
                }
            }

            return null;
        }

        private static UnityEngine.Object[] BuildPulseRendererReferenceArray(Renderer pulseRenderer)
        {
            return new UnityEngine.Object[] { pulseRenderer };
        }

        private static void ConfigureSummonActorAnimatorPresentation(
            SummonFrontlineProxyPresenter actorPresenter,
            Transform visual,
            float animatorMoveSpeedScale)
        {
            if (actorPresenter == null)
            {
                throw new InvalidOperationException("Summon actor presenter is missing.");
            }

            Animator animator = visual != null ? visual.GetComponent<Animator>() : null;
            if (animator == null)
            {
                throw new InvalidOperationException(
                    $"{actorPresenter.name} requires a visual Animator for summon actor state feedback.");
            }

            SetObjectReference(actorPresenter, "animator", animator);
            SetString(actorPresenter, "moveSpeedParameter", SummonActorMoveSpeedParameter);
            SetString(actorPresenter, "spawnTrigger", SummonActorSpawnTrigger);
            SetString(actorPresenter, "attackTrigger", SummonActorAttackTrigger);
            SetString(actorPresenter, "hitTrigger", string.Empty);
            SetString(actorPresenter, "deathTrigger", SummonActorDeathTrigger);
            SetFloat(actorPresenter, "animatorMoveSpeedScale", animatorMoveSpeedScale);
            SetBool(actorPresenter, "playDamageVfx", true);
            SetBool(actorPresenter, "renderDamageFeedback", true);
            SetColor(actorPresenter, "damageFlashColor", new Color(1f, 0.34f, 0.18f, 1f));
            SetColor(actorPresenter, "damageFlashEmissionColor", new Color(1f, 0.72f, 0.24f, 1f));
            SetFloat(actorPresenter, "damageFlashSeconds", 0.2f);
            SetFloat(actorPresenter, "damageFlashScale", 0.22f);
            SetFloat(actorPresenter, "damageFlashColorBlend", 0.98f);
            SetFloat(actorPresenter, "damageFlashEmissionBoost", 3.4f);
        }

        private static void ValidatePulseOnlyActorRenderers(
            SummonFrontlineProxyPresenter actorPresenter,
            Renderer pulseRenderer,
            string pulseLabel)
        {
            SerializedObject serializedPresenter = new SerializedObject(actorPresenter);
            SerializedProperty actorRenderers = RequireProperty(serializedPresenter, "actorRenderers");
            if (!actorRenderers.isArray || actorRenderers.arraySize != 1)
            {
                throw new InvalidOperationException(
                    $"{actorPresenter.name}.actorRenderers should tint only {pulseLabel}, not the promoted summon model.");
            }

            ValidateArrayReference(actorPresenter, "actorRenderers", 0, pulseRenderer);
        }

        private static void ValidateSummonActorDamageFlashRenderers(
            SummonFrontlineProxyPresenter actorPresenter,
            Transform visual,
            string label)
        {
            Renderer[] visualRenderers = CollectEnabledRenderers(visual.gameObject);
            if (visualRenderers.Length == 0)
            {
                throw new InvalidOperationException($"{label} needs at least one enabled renderer for body hit flash.");
            }

            SerializedProperty damageFlashRenderers =
                RequireProperty(new SerializedObject(actorPresenter), "damageFlashRenderers");
            if (!damageFlashRenderers.isArray || damageFlashRenderers.arraySize != visualRenderers.Length)
            {
                throw new InvalidOperationException(
                    $"{actorPresenter.name}.damageFlashRenderers should bind every enabled renderer on {label}.");
            }

            for (int i = 0; i < visualRenderers.Length; i++)
            {
                ValidateArrayReference(actorPresenter, "damageFlashRenderers", i, visualRenderers[i]);
            }

            Transform damageVfxAnchor = RequireProperty(new SerializedObject(actorPresenter), "damageVfxAnchor")
                .objectReferenceValue as Transform;
            if (damageVfxAnchor == null)
            {
                throw new InvalidOperationException(
                    $"{actorPresenter.name}.damageVfxAnchor should bind a torso-height body VFX anchor.");
            }

            if (damageVfxAnchor == actorPresenter.transform || damageVfxAnchor == actorPresenter.PulseRoot)
            {
                throw new InvalidOperationException(
                    $"{actorPresenter.name}.damageVfxAnchor must not use the floor/root or hidden tier pulse anchor.");
            }

            if (!string.Equals(damageVfxAnchor.name, "DamageVfxAnchor", StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"{actorPresenter.name}.damageVfxAnchor should use the reviewed DamageVfxAnchor transform.");
            }

            if (damageVfxAnchor.localPosition.y < 0.35f)
            {
                throw new InvalidOperationException(
                    $"{actorPresenter.name}.damageVfxAnchor should sit at torso height, not at the actor root.");
            }

            if (TryResolveEnabledRendererBounds(visualRenderers, out Bounds visualBounds)
                && actorPresenter.transform.InverseTransformPoint(visualBounds.center).y >= 0.35f
                && Vector3.Distance(damageVfxAnchor.position, visualBounds.center) > 0.05f)
            {
                throw new InvalidOperationException(
                    $"{actorPresenter.name}.damageVfxAnchor should sit on the promoted body bounds center, not at the feet.");
            }
        }

        private static void ValidateSummonActorAnimatorPresentation(
            SummonFrontlineProxyPresenter actorPresenter,
            Transform visual,
            string label,
            float expectedAnimatorMoveSpeedScale)
        {
            if (actorPresenter == null || visual == null)
            {
                throw new InvalidOperationException($"{label} must keep a presenter and promoted visual.");
            }

            Animator animator = visual.GetComponent<Animator>();
            if (animator == null)
            {
                throw new InvalidOperationException($"{label} promoted visual is missing an Animator.");
            }

            ValidateObjectReference(actorPresenter, "animator", animator);
            ValidateString(actorPresenter, "moveSpeedParameter", SummonActorMoveSpeedParameter);
            ValidateString(actorPresenter, "spawnTrigger", SummonActorSpawnTrigger);
            ValidateString(actorPresenter, "attackTrigger", SummonActorAttackTrigger);
            ValidateString(actorPresenter, "hitTrigger", string.Empty);
            ValidateString(actorPresenter, "deathTrigger", SummonActorDeathTrigger);
            ValidateFloat(actorPresenter, "animatorMoveSpeedScale", expectedAnimatorMoveSpeedScale);
            ValidateEnum(actorPresenter, "entryCueId", (int)CombatVfxCueId.EliteSummonSignal);
            ValidateEnum(actorPresenter, "attackCueId", (int)CombatVfxCueId.EnemyAttackActive);
            ValidateEnum(actorPresenter, "clashCueId", (int)CombatVfxCueId.EliteShieldSignal);
            ValidateEnum(actorPresenter, "damageCueId", (int)CombatVfxCueId.EnemyHit);
            ValidateEnum(actorPresenter, "deathCueId", (int)CombatVfxCueId.EnemyDeath);
            ValidateFloat(actorPresenter, "pressureDamageCueScale", 0.64f);
            ValidateBool(actorPresenter, "playDamageVfx", true);
            ValidateBool(actorPresenter, "renderDamageFeedback", true);
            ValidateColor(actorPresenter, "damageFlashColor", new Color(1f, 0.34f, 0.18f, 1f));
            ValidateColor(actorPresenter, "damageFlashEmissionColor", new Color(1f, 0.72f, 0.24f, 1f));
            ValidateFloat(actorPresenter, "damageFlashSeconds", 0.2f);
            ValidateFloat(actorPresenter, "damageFlashScale", 0.22f);
            ValidateFloat(actorPresenter, "damageFlashColorBlend", 0.98f);
            ValidateFloat(actorPresenter, "damageFlashEmissionBoost", 3.4f);
            ValidateAnimatorParameter(
                animator,
                SummonActorMoveSpeedParameter,
                AnimatorControllerParameterType.Float,
                $"{label} walk read");
            ValidateAnimatorParameter(
                animator,
                SummonActorSpawnTrigger,
                AnimatorControllerParameterType.Trigger,
                $"{label} spawn read");
            ValidateAnimatorParameter(
                animator,
                SummonActorAttackTrigger,
                AnimatorControllerParameterType.Trigger,
                $"{label} attack read");
            ValidateAnimatorParameter(
                animator,
                SummonActorDeathTrigger,
                AnimatorControllerParameterType.Trigger,
                $"{label} death read");
        }

        private static void ValidateSummonActorBodyContract(
            GameObject prefabRoot,
            SummonFrontlineProxy proxy,
            SummonFrontlineClash clash,
            CombatHealth health,
            DamageTeam expectedTeam,
            string label)
        {
            if (prefabRoot == null || proxy == null || clash == null || health == null)
            {
                throw new InvalidOperationException($"{label} must keep proxy, clash, and health components together.");
            }

            if (prefabRoot.GetComponent<SummonFrontlineProxy>() != proxy
                || prefabRoot.GetComponent<SummonFrontlineClash>() != clash
                || prefabRoot.GetComponent<CombatHealth>() != health)
            {
                throw new InvalidOperationException(
                    $"{label} must keep proxy, clash, and health on the prefab root body.");
            }

            SphereCollider bodyCollider = prefabRoot.GetComponent<SphereCollider>();
            if (bodyCollider == null)
            {
                throw new InvalidOperationException($"{label} must keep a root SphereCollider body hitbox.");
            }

            if (!bodyCollider.isTrigger)
            {
                throw new InvalidOperationException($"{label} body collider must be a trigger for summon clash contacts.");
            }

            if (bodyCollider.radius <= 0f)
            {
                throw new InvalidOperationException($"{label} body collider radius must be positive.");
            }

            Rigidbody bodyRigidbody = prefabRoot.GetComponent<Rigidbody>();
            if (bodyRigidbody == null)
            {
                throw new InvalidOperationException($"{label} must keep a root Rigidbody for trigger contact dispatch.");
            }

            if (!bodyRigidbody.isKinematic || bodyRigidbody.useGravity)
            {
                throw new InvalidOperationException($"{label} Rigidbody must be kinematic and gravity-free.");
            }

            ValidateEnum(health, "team", (int)expectedTeam);
            ValidateBool(health, "startAtFullHealth", true);
            if (health.MaxHealth <= 0f)
            {
                throw new InvalidOperationException($"{label} must have positive max health.");
            }
        }

        private static void ValidateSummonHealthBar(
            GameObject prefabRoot,
            SummonFrontlineProxy proxy,
            CombatHealth health,
            string label)
        {
            SummonFrontlineHealthBarPresenter healthBarPresenter =
                prefabRoot.GetComponent<SummonFrontlineHealthBarPresenter>();
            if (healthBarPresenter == null)
            {
                throw new InvalidOperationException($"{label} must keep a summon health bar presenter.");
            }

            Transform barRoot = prefabRoot.transform.Find(SummonHealthBarRootName);
            if (barRoot == null)
            {
                throw new InvalidOperationException($"{label} is missing {SummonHealthBarRootName}.");
            }

            Transform back = barRoot.Find(SummonHealthBarBackName);
            Transform fill = barRoot.Find(SummonHealthBarFillName);
            if (back == null || fill == null)
            {
                throw new InvalidOperationException($"{label} health bar must keep back and fill children.");
            }

            MeshRenderer backRenderer = back.GetComponent<MeshRenderer>();
            MeshRenderer fillRenderer = fill.GetComponent<MeshRenderer>();
            if (backRenderer == null || fillRenderer == null)
            {
                throw new InvalidOperationException($"{label} health bar children must have MeshRenderers.");
            }

            ValidateObjectReference(healthBarPresenter, "proxy", proxy);
            ValidateObjectReference(healthBarPresenter, "health", health);
            ValidateObjectReference(healthBarPresenter, "barRoot", barRoot);
            ValidateObjectReference(healthBarPresenter, "fillRoot", fill);
            ValidateArrayReference(healthBarPresenter, "barRenderers", 0, backRenderer);
            ValidateArrayReference(healthBarPresenter, "barRenderers", 1, fillRenderer);
            ValidateRenderableMaterialShader(backRenderer.sharedMaterial, $"{label} health bar back material");
            ValidateRenderableMaterialShader(fillRenderer.sharedMaterial, $"{label} health bar fill material");
            ValidateGameOwnedAsset(backRenderer.sharedMaterial, $"{label} health bar back material");
            ValidateGameOwnedAsset(fillRenderer.sharedMaterial, $"{label} health bar fill material");
        }

        private static Transform ValidateSummonActorRoleVisual(GameObject prefabRoot, string visualName)
        {
            Transform visual = prefabRoot.transform.Find(visualName);
            if (visual == null)
            {
                throw new InvalidOperationException($"{prefabRoot.name} should include {visualName}.");
            }

            ValidateSummonActorRoleVisualContents(visual.gameObject, visualName);
            return visual;
        }

        private static void ValidateSummonPresentationCandidateProfiles()
        {
            CombatVfxCueProfile vfxCueProfile =
                LoadAsset<CombatVfxCueProfile>(ActionFoundationCombatVfxSetup.CombatVfxCueProfilePath);

            ValidateSummonPresentationCandidateProfile(
                LoadAsset<SummonPresentationCandidateProfile>(SummonSlot1PresentationCandidateProfilePath),
                "PlayerSummon.ChargeBruiser",
                SummonPresentationSide.PlayerSummon,
                SummonSlot1ActorPrefabPath,
                ActionFoundationEnemyRoleCandidateSetup.ShieldBreakerEliteCandidateProfilePath,
                SummonSlot1ActorVisualName,
                SummonSlot1ActorVisualRoleId,
                vfxCueProfile);

            ValidateSummonPresentationCandidateProfile(
                LoadAsset<SummonPresentationCandidateProfile>(SummonSlot2PresentationCandidateProfilePath),
                "PlayerSummon.LaserSoldier",
                SummonPresentationSide.PlayerSummon,
                SummonSlot2ActorPrefabPath,
                ActionFoundationEnemyRoleCandidateSetup.LineCasterCandidateProfilePath,
                SummonSlot2ActorVisualName,
                SummonSlot2ActorVisualRoleId,
                vfxCueProfile);

            ValidateSummonPresentationCandidateProfile(
                LoadAsset<SummonPresentationCandidateProfile>(SummonSlot3PresentationCandidateProfilePath),
                "PlayerSummon.FireDragon",
                SummonPresentationSide.PlayerSummon,
                SummonSlot3ActorPrefabPath,
                ActionFoundationEnemyRoleCandidateSetup.FinalStandCommanderEliteCandidateProfilePath,
                SummonSlot3ActorVisualName,
                SummonSlot3ActorVisualRoleId,
                vfxCueProfile,
                SummonSlot3DragonVisualPrefabPath);

            ValidateSummonPresentationCandidateProfile(
                LoadAsset<SummonPresentationCandidateProfile>(BossSummonPressurePresentationCandidateProfilePath),
                "BossPressure.AuraCaptain",
                SummonPresentationSide.BossPressure,
                BossSummonPressureActorPrefabPath,
                ActionFoundationEnemyRoleCandidateSetup.AuraCaptainEliteCandidateProfilePath,
                BossSummonPressureActorVisualName,
                BossSummonPressureActorVisualRoleId,
                vfxCueProfile);
        }

        private static void ValidateSummonPresentationCandidateProfile(
            SummonPresentationCandidateProfile profile,
            string expectedCandidateId,
            SummonPresentationSide expectedSide,
            string actorPrefabPath,
            string roleCandidateProfilePath,
            string visualChildName,
            string sourceRoleId,
            CombatVfxCueProfile vfxCueProfile,
            string visualSourceOverridePath = null)
        {
            GameObject actorPrefab = LoadAsset<GameObject>(actorPrefabPath);
            CombatEnemyRoleCandidateProfile roleCandidate =
                LoadAsset<CombatEnemyRoleCandidateProfile>(roleCandidateProfilePath);
            GameObject visualSourceAsset = !string.IsNullOrWhiteSpace(visualSourceOverridePath)
                ? LoadAsset<GameObject>(visualSourceOverridePath)
                : roleCandidate.PromotedVisualSource;
            RuntimeAnimatorController animatorController =
                ResolveActorVisualAnimatorController(actorPrefab, visualChildName);

            if (!string.Equals(profile.CandidateId, expectedCandidateId, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"{profile.name} has the wrong summon presentation candidate id.");
            }

            if (profile.Side != expectedSide)
            {
                throw new InvalidOperationException($"{profile.name} has the wrong summon presentation side.");
            }

            if (profile.ActorPrefab != actorPrefab)
            {
                throw new InvalidOperationException($"{profile.name} points to the wrong actor prefab.");
            }

            if (profile.VisualSourceAsset != visualSourceAsset)
            {
                throw new InvalidOperationException($"{profile.name} points to the wrong promoted visual source.");
            }

            if (!string.Equals(profile.VisualChildName, visualChildName, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"{profile.name} has the wrong visual child name.");
            }

            if (!string.Equals(profile.SourceRoleId, sourceRoleId, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"{profile.name} has the wrong source role id.");
            }

            if (profile.AnimatorController != animatorController)
            {
                throw new InvalidOperationException($"{profile.name} points to the wrong Animator controller.");
            }

            if (profile.VfxCueProfile != vfxCueProfile)
            {
                throw new InvalidOperationException($"{profile.name} points to the wrong VFX cue profile.");
            }

            if (string.IsNullOrWhiteSpace(profile.DisplayName)
                || string.IsNullOrWhiteSpace(profile.AnimationRead)
                || string.IsNullOrWhiteSpace(profile.VfxRead)
                || string.IsNullOrWhiteSpace(profile.ReplacementPlan)
                || string.IsNullOrWhiteSpace(profile.OwnershipNotes))
            {
                throw new InvalidOperationException($"{profile.name} should document display, animation, VFX, replacement, and ownership notes.");
            }

            ValidateGameOwnedAsset(profile, $"{profile.name} asset");
            ValidateGameOwnedAsset(profile.ActorPrefab, $"{profile.name} actor prefab");
            ValidateGameOwnedAsset(profile.VisualSourceAsset, $"{profile.name} visual source");
            ValidateGameOwnedAsset(profile.AnimatorController, $"{profile.name} Animator controller");
            ValidateGameOwnedAsset(profile.VfxCueProfile, $"{profile.name} VFX cue profile");
            Transform visual = ValidateSummonActorRoleVisual(actorPrefab, visualChildName);
            ValidateSummonActorRoleVisualContents(visual.gameObject, profile.name);
        }

        private static void ValidateSummonActorRoleVisualContents(GameObject visual, string label)
        {
            if (visual.GetComponentInChildren<CombatHealth>(true) != null
                || visual.GetComponentInChildren<BasicSoldierEnemy>(true) != null
                || visual.GetComponentInChildren<CombatTargetSensor>(true) != null
                || visual.GetComponentInChildren<EnemyElitePatternController>(true) != null)
            {
                throw new InvalidOperationException($"{label} must be visual-only and must not duplicate enemy gameplay components.");
            }

            Animator animator = visual.GetComponent<Animator>();
            if (animator == null || animator.runtimeAnimatorController == null)
            {
                throw new InvalidOperationException($"{label} should keep a promoted role Animator.");
            }

            ValidateGameOwnedAsset(animator.runtimeAnimatorController, $"{label} Animator Controller");

            Renderer[] renderers = CollectEnabledRenderers(visual);
            if (renderers.Length == 0)
            {
                throw new InvalidOperationException($"{label} should expose promoted enabled renderers.");
            }

            for (int i = 0; i < renderers.Length; i++)
            {
                ValidateRendererAssets(renderers[i], $"{label}.{renderers[i].name}");
            }
        }

        private static Renderer[] CollectEnabledRenderers(GameObject root)
        {
            Renderer[] allRenderers = root.GetComponentsInChildren<Renderer>(includeInactive: true);
            var renderers = new List<Renderer>();
            for (int i = 0; i < allRenderers.Length; i++)
            {
                if (allRenderers[i].enabled)
                {
                    renderers.Add(allRenderers[i]);
                }
            }

            return renderers.ToArray();
        }

        private static void ValidateBossVisualCueBindings(BossBarrageVisualCueDriver cueDriver, Animator animator)
        {
            CombatVfxCueProfile cueProfile = cueDriver.CuePlayer != null ? cueDriver.CuePlayer.Profile : null;
            if (cueProfile == null)
            {
                throw new InvalidOperationException("Boss visual cue driver should reference the shared combat VFX cue profile.");
            }

            var foundPatternIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < cueDriver.PatternCueCount; i++)
            {
                if (!cueDriver.TryGetPatternCue(i, out BossBarrageVisualCueDriver.PatternAnimationCue cue))
                {
                    throw new InvalidOperationException($"Boss visual cue driver could not read pattern cue at index {i}.");
                }

                if (string.IsNullOrWhiteSpace(cue.PatternId))
                {
                    throw new InvalidOperationException($"Boss visual cue at index {i} has no pattern id.");
                }

                foundPatternIds.Add(cue.PatternId);
                ValidateAnimatorTrigger(animator, cue.WindupTrigger, $"{cue.PatternId} windup trigger");
                ValidateAnimatorTrigger(animator, cue.ReleaseTrigger, $"{cue.PatternId} release trigger");
                ValidateBossPatternWorldVfxCue(cueProfile, cue);
            }

            for (int i = 0; i < RequiredBossPatternCueIds.Length; i++)
            {
                if (!foundPatternIds.Contains(RequiredBossPatternCueIds[i]))
                {
                    throw new InvalidOperationException($"Boss visual cue driver is missing pattern cue {RequiredBossPatternCueIds[i]}.");
                }
            }
        }

        private static void ValidateBossPatternWorldVfxCue(
            CombatVfxCueProfile cueProfile,
            BossBarrageVisualCueDriver.PatternAnimationCue cue)
        {
            if (!cue.UseWorldVfxCueOverride)
            {
                throw new InvalidOperationException(
                    $"Boss pattern {cue.PatternId} should choose pattern-specific world VFX cues.");
            }

            if (!cueProfile.TryGetCue(cue.WindupWorldCueId, out _))
            {
                throw new InvalidOperationException(
                    $"Boss pattern {cue.PatternId} windup world VFX cue {cue.WindupWorldCueId} is missing from the combat VFX profile.");
            }

            if (!cueProfile.TryGetCue(cue.ReleaseWorldCueId, out _))
            {
                throw new InvalidOperationException(
                    $"Boss pattern {cue.PatternId} release world VFX cue {cue.ReleaseWorldCueId} is missing from the combat VFX profile.");
            }
        }

        private static void ValidateBossPressureActionCueBindings(BossBarrageVisualCueDriver cueDriver, Animator animator)
        {
            var foundActionKinds = new HashSet<BossPressureActionKind>();
            for (int i = 0; i < cueDriver.PressureActionCueCount; i++)
            {
                if (!cueDriver.TryGetPressureActionCue(i, out BossBarrageVisualCueDriver.PressureActionCue cue))
                {
                    throw new InvalidOperationException($"Boss visual cue driver could not read pressure action cue at index {i}.");
                }

                foundActionKinds.Add(cue.ActionKind);
                ValidateAnimatorTrigger(animator, cue.Trigger, $"{cue.ActionKind} pressure action trigger");
            }

            for (int i = 0; i < RequiredBossPressureActionCueKinds.Length; i++)
            {
                if (!foundActionKinds.Contains(RequiredBossPressureActionCueKinds[i]))
                {
                    throw new InvalidOperationException(
                        $"Boss visual cue driver is missing pressure action cue {RequiredBossPressureActionCueKinds[i]}.");
                }
            }
        }

        private static void ValidateAnimatorTrigger(Animator animator, string triggerName, string label)
        {
            if (string.IsNullOrWhiteSpace(triggerName))
            {
                throw new InvalidOperationException($"Boss visual cue {label} is empty.");
            }

            AnimatorControllerParameter[] parameters = animator.parameters;
            for (int i = 0; i < parameters.Length; i++)
            {
                AnimatorControllerParameter parameter = parameters[i];
                if (parameter.type == AnimatorControllerParameterType.Trigger
                    && string.Equals(parameter.name, triggerName, StringComparison.Ordinal))
                {
                    return;
                }
            }

            throw new InvalidOperationException($"Boss visual cue {label} references missing Animator trigger {triggerName}.");
        }

        private static void ValidateAnimatorParameter(
            Animator animator,
            string parameterName,
            AnimatorControllerParameterType expectedType,
            string label)
        {
            if (animator == null)
            {
                throw new InvalidOperationException($"{label} requires an Animator.");
            }

            if (string.IsNullOrWhiteSpace(parameterName))
            {
                throw new InvalidOperationException($"{label} Animator parameter is empty.");
            }

            AnimatorControllerParameter[] parameters = animator.parameters;
            for (int i = 0; i < parameters.Length; i++)
            {
                AnimatorControllerParameter parameter = parameters[i];
                if (parameter.type == expectedType
                    && string.Equals(parameter.name, parameterName, StringComparison.Ordinal))
                {
                    return;
                }
            }

            throw new InvalidOperationException(
                $"{label} references missing Animator {expectedType} parameter {parameterName}.");
        }

        private static void ValidateRendererAssets(Renderer renderer, string label)
        {
            MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                ValidateGameOwnedAsset(meshFilter.sharedMesh, $"{label} mesh");
            }

            SkinnedMeshRenderer skinnedMeshRenderer = renderer as SkinnedMeshRenderer;
            if (skinnedMeshRenderer != null && skinnedMeshRenderer.sharedMesh != null)
            {
                ValidateGameOwnedAsset(skinnedMeshRenderer.sharedMesh, $"{label} mesh");
            }

            Material[] materials = renderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] != null)
                {
                    ValidateGameOwnedAsset(materials[i], $"{label} material");
                }
            }
        }

        private static void ValidateBossProxyCommandoPrefabSource(GameObject visual)
        {
            GameObject source = PrefabUtility.GetCorrespondingObjectFromSource(visual);
            string sourcePath = AssetDatabase.GetAssetPath(source).Replace('\\', '/');
            if (string.IsNullOrEmpty(sourcePath))
            {
                sourcePath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(visual).Replace('\\', '/');
            }

            if (!string.Equals(sourcePath, BossProxyHumanoidSourcePrefabPath, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"{BossProxyHumanoidVisualName} should be a prefab instance of {BossProxyHumanoidSourcePrefabPath}, found {sourcePath}.");
            }
        }

        private static void ValidateBossProxyCommandoRendererAssets(Renderer renderer, string label)
        {
            MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                ValidateBossProxyCommandoVisualAsset(meshFilter.sharedMesh, $"{label} mesh");
            }

            SkinnedMeshRenderer skinnedMeshRenderer = renderer as SkinnedMeshRenderer;
            if (skinnedMeshRenderer != null && skinnedMeshRenderer.sharedMesh != null)
            {
                ValidateBossProxyCommandoVisualAsset(skinnedMeshRenderer.sharedMesh, $"{label} mesh");
            }

            Material[] materials = renderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] != null)
                {
                    ValidateBossProxyCommandoVisualAsset(materials[i], $"{label} material");
                }
            }
        }

        private static void ValidateBossProxyCommandoVisualAsset(UnityEngine.Object asset, string label)
        {
            if (asset == null)
            {
                throw new InvalidOperationException($"{label} must be assigned.");
            }

            string assetPath = AssetDatabase.GetAssetPath(asset).Replace('\\', '/');
            bool isGameOwned = assetPath.StartsWith("Assets/_Game/", StringComparison.Ordinal)
                && !assetPath.Contains("/_Imported/", StringComparison.Ordinal);
            bool isExactCommandoSource = assetPath.StartsWith(BossProxyHumanoidShooterRoot + "/SciFiSoldier_01/", StringComparison.Ordinal)
                || assetPath.StartsWith(BossProxyHumanoidCommonWeaponRoot + "/", StringComparison.Ordinal);
            if (!isGameOwned && !isExactCommandoSource)
            {
                throw new InvalidOperationException(
                    $"{label} should reference the exact SciFiSoldier_01_Commando source or a promoted `_Game` asset, found {assetPath}.");
            }
        }

        private static void ValidateGameOwnedAsset(UnityEngine.Object asset, string label)
        {
            if (asset == null)
            {
                throw new InvalidOperationException($"{label} must be assigned.");
            }

            string assetPath = AssetDatabase.GetAssetPath(asset).Replace('\\', '/');
            if (!assetPath.StartsWith("Assets/_Game/", StringComparison.Ordinal)
                || assetPath.Contains("/_Imported/", StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"{label} should reference a promoted `_Game` asset, found {assetPath}.");
            }
        }

        private static void ValidatePocketOwner(
            BossBarragePocketReviewOwner owner,
            CombatHealth playerHealth,
            CombatHealth closeThreatHealth,
            CombatHealth bossHealth,
            SummonEnergyLadder energyLadder,
            PlayerSkill1Action skill1Action,
            PlayerSummonSlot1Action summonSlot1Action,
            PlayerSupportSummonSlotAction summonSlot2Action,
            PlayerSupportSummonSlotAction summonSlot3Action,
            BossBarrageEmitter bossBarrageEmitter,
            BossBasicFireEmitter bossBasicFireEmitter,
            FrontlineWaveStageProfile stageProfile,
            BossPressureCostLadder bossPressureCost,
            BossPressureActionDirector bossPressureActionDirector)
        {
            ValidateObjectReference(owner, "playerHealth", playerHealth);
            ValidateObjectReference(owner, "closeThreatHealth", closeThreatHealth);
            ValidateObjectReference(owner, "bossHealth", bossHealth);
            ValidateObjectReference(owner, "energyLadder", energyLadder);
            ValidateObjectReference(owner, "skill1Action", skill1Action);
            ValidateObjectReference(owner, "summonSlot1Action", summonSlot1Action);
            ValidateObjectReference(owner, "summonSlot2Action", summonSlot2Action);
            ValidateObjectReference(owner, "summonSlot3Action", summonSlot3Action);
            ValidateObjectReference(owner, "bossBarrageEmitter", bossBarrageEmitter);
            ValidateObjectReference(owner, "bossBasicFireEmitter", bossBasicFireEmitter);
            ValidateObjectReference(owner, "stageProfile", stageProfile);
            ValidateObjectReference(owner, "bossPressureCostLadder", bossPressureCost);
            ValidateObjectReference(owner, "bossPressureActionDirector", bossPressureActionDirector);
            ValidateObjectReference(
                owner,
                "summonPressureBlockOpportunity",
                LoadAsset<SummonOpportunityWindowProfile>(SummonOpportunityProfilePath));
            ValidateBool(owner, "stopBarrageOnClear", true);
            ValidateBool(owner, "stopBarrageOnFail", true);
            ValidateBool(owner, "stopBossPressureCostOnEnd", true);
            ValidateBool(owner, "stopBossPressureActionsOnEnd", true);
            ValidateBool(owner, "stopEnergyGainOnEnd", true);
            if (owner.StageProfile != stageProfile)
            {
                throw new InvalidOperationException($"{owner.name}.StageProfile is not bound to {stageProfile.name}.");
            }

            if (owner.ObjectiveStepCount != stageProfile.ObjectiveStepCount)
            {
                throw new InvalidOperationException("Pocket owner objective count does not match the frontline stage profile.");
            }

            ValidateFloat(owner, "skill1FollowupClearDelaySeconds", 0.75f);
            ValidateAssignedObjectReference(owner, "clearMarker");
            ValidateAssignedObjectReference(owner, "failMarker");
        }

        private static void ConfigureBossSummonDuelOwner(
            BossSummonDuelReviewOwner owner,
            CombatHealth playerHealth,
            CombatHealth bossHealth,
            SummonEnergyLadder energyLadder,
            PlayerSkill1Action skill1Action,
            PlayerSummonSlot1Action summonSlot1Action,
            PlayerSupportSummonSlotAction summonSlot2Action,
            PlayerSupportSummonSlotAction summonSlot3Action,
            BossBarrageEmitter bossBarrageEmitter,
            BossBasicFireEmitter bossBasicFireEmitter,
            BossPressureCostLadder bossPressureCost,
            BossPressureActionDirector bossPressureActionDirector,
            BossSummonPressureAction bossSummonPressureAction,
            GameObject clearMarker,
            GameObject failMarker)
        {
            SetObjectReference(owner, "playerHealth", playerHealth);
            SetObjectReference(owner, "bossHealth", bossHealth);
            SetObjectReference(owner, "energyLadder", energyLadder);
            SetObjectReference(owner, "skill1Action", skill1Action);
            SetObjectReference(owner, "summonSlot1Action", summonSlot1Action);
            SetObjectReference(owner, "summonSlot2Action", summonSlot2Action);
            SetObjectReference(owner, "summonSlot3Action", summonSlot3Action);
            SetObjectReference(owner, "bossBarrageEmitter", bossBarrageEmitter);
            SetObjectReference(owner, "bossBasicFireEmitter", bossBasicFireEmitter);
            SetObjectReference(owner, "bossPressureCostLadder", bossPressureCost);
            SetObjectReference(owner, "bossPressureActionDirector", bossPressureActionDirector);
            SetObjectReference(owner, "bossSummonPressureAction", bossSummonPressureAction);
            SetObjectReference(owner, "clearMarker", clearMarker);
            SetObjectReference(owner, "failMarker", failMarker);
            SetBool(owner, "grantPlayerEnergyOnStart", true);
            SetFloat(owner, "startingPlayerEnergy", 150f);
            SetBool(owner, "grantBossCostOnStart", true);
            SetFloat(owner, "startingBossCost", 150f);
            SetBool(owner, "stopBarrageOnEnd", true);
            SetBool(owner, "stopBossPressureCostOnEnd", true);
            SetBool(owner, "stopBossPressureActionsOnEnd", true);
            SetBool(owner, "stopEnergyGainOnEnd", true);
            SetInt(owner, "requiredBossPressureActions", 2);
            SetInt(owner, "requiredBossSkillPatterns", 1);
            SetInt(owner, "requiredBossSummonPressureActions", 1);
            SetInt(owner, "requiredBossPunishPatterns", 0);
            SetInt(owner, "requiredBossSummonReleases", 1);
            SetInt(owner, "requiredBossPressureBlocks", 1);
            SetInt(owner, "requiredPlayerSummonUses", 2);
            SetInt(owner, "requiredSupportSummonUses", 1);
            SetInt(owner, "requiredBossResponsesToPlayerSummons", 1);
            SetInt(owner, "requiredAllyPressureBlocks", 1);
            SetInt(owner, "requiredSummonClashes", 1);
            SetInt(owner, "requiredSummonActorDefeats", 1);
            SetInt(owner, "requiredBossRepressureAfterSummonDefeat", 1);
            SetInt(owner, "requiredFrontlineLoopCycles", 1);
            SetInt(owner, "requiredSkill1ResponseUses", 1);
            SetFloat(owner, "requiredSkill1ResponseDamage", 60f);
            SetFloat(owner, "skill1ResponseDamageWindowSeconds", 2.5f);
            SetFloat(owner, "requiredBossDamage", 220f);
            SetBool(owner, "failWhenPlayerDies", true);
            EditorUtility.SetDirty(owner);
        }

        private static void ValidateBossSummonDuelOwner(
            BossSummonDuelReviewOwner owner,
            CombatHealth playerHealth,
            CombatHealth bossHealth,
            SummonEnergyLadder energyLadder,
            PlayerSkill1Action skill1Action,
            PlayerSummonSlot1Action summonSlot1Action,
            PlayerSupportSummonSlotAction summonSlot2Action,
            PlayerSupportSummonSlotAction summonSlot3Action,
            BossBarrageEmitter bossBarrageEmitter,
            BossBasicFireEmitter bossBasicFireEmitter,
            BossPressureCostLadder bossPressureCost,
            BossPressureActionDirector bossPressureActionDirector,
            BossSummonPressureAction bossSummonPressureAction,
            GameObject clearMarker,
            GameObject failMarker)
        {
            ValidateObjectReference(owner, "playerHealth", playerHealth);
            ValidateObjectReference(owner, "bossHealth", bossHealth);
            ValidateObjectReference(owner, "energyLadder", energyLadder);
            ValidateObjectReference(owner, "skill1Action", skill1Action);
            ValidateObjectReference(owner, "summonSlot1Action", summonSlot1Action);
            ValidateObjectReference(owner, "summonSlot2Action", summonSlot2Action);
            ValidateObjectReference(owner, "summonSlot3Action", summonSlot3Action);
            ValidateObjectReference(owner, "bossBarrageEmitter", bossBarrageEmitter);
            ValidateObjectReference(owner, "bossBasicFireEmitter", bossBasicFireEmitter);
            ValidateObjectReference(owner, "bossPressureCostLadder", bossPressureCost);
            ValidateObjectReference(owner, "bossPressureActionDirector", bossPressureActionDirector);
            ValidateObjectReference(owner, "bossSummonPressureAction", bossSummonPressureAction);
            ValidateObjectReference(owner, "clearMarker", clearMarker);
            ValidateObjectReference(owner, "failMarker", failMarker);
            ValidateBool(owner, "grantPlayerEnergyOnStart", true);
            ValidateFloat(owner, "startingPlayerEnergy", 150f);
            ValidateBool(owner, "grantBossCostOnStart", true);
            ValidateFloat(owner, "startingBossCost", 150f);
            ValidateBool(owner, "stopBarrageOnEnd", true);
            ValidateBool(owner, "stopBossPressureCostOnEnd", true);
            ValidateBool(owner, "stopBossPressureActionsOnEnd", true);
            ValidateBool(owner, "stopEnergyGainOnEnd", true);
            ValidateInt(owner, "requiredBossPressureActions", 2);
            ValidateInt(owner, "requiredBossSkillPatterns", 1);
            ValidateInt(owner, "requiredBossSummonPressureActions", 1);
            ValidateInt(owner, "requiredBossPunishPatterns", 0);
            ValidateInt(owner, "requiredBossSummonReleases", 1);
            ValidateInt(owner, "requiredBossPressureBlocks", 1);
            ValidateInt(owner, "requiredPlayerSummonUses", 2);
            ValidateInt(owner, "requiredSupportSummonUses", 1);
            ValidateInt(owner, "requiredBossResponsesToPlayerSummons", 1);
            ValidateInt(owner, "requiredAllyPressureBlocks", 1);
            ValidateInt(owner, "requiredSummonClashes", 1);
            ValidateInt(owner, "requiredSummonActorDefeats", 1);
            ValidateInt(owner, "requiredBossRepressureAfterSummonDefeat", 1);
            ValidateInt(owner, "requiredFrontlineLoopCycles", 1);
            ValidateInt(owner, "requiredSkill1ResponseUses", 1);
            ValidateFloat(owner, "requiredSkill1ResponseDamage", 60f);
            ValidateFloat(owner, "skill1ResponseDamageWindowSeconds", 2.5f);
            ValidateFloat(owner, "requiredBossDamage", 220f);
            ValidateBool(owner, "failWhenPlayerDies", true);
        }

        private static void ValidatePocketCueBridges(
            BossBarragePocketReviewOwner owner,
            PlayerSummonSlot1Action summonSlot1Action,
            ActionCameraCueDriver cameraCueDriver,
            ActionCinematicCueDirector cinematicCueDirector,
            PlayerCombatVfxCueDriver playerVfxCueDriver,
            CombatVfxCuePlayer cuePlayer,
            Transform directionTarget)
        {
            BossBarragePocketCameraCueBridge cameraBridge =
                RequireComponent<BossBarragePocketCameraCueBridge>(owner.gameObject, "pocket camera cue bridge");
            ValidateBehaviourEnabled(cameraBridge, true);
            ValidateObjectReference(cameraBridge, "pocketReviewOwner", owner);
            ValidateObjectReference(cameraBridge, "summonSlot1Action", summonSlot1Action);
            ValidateObjectReference(cameraBridge, "cameraCueDriver", cameraCueDriver);
            ValidateObjectReference(cameraBridge, "cinematicCueDirector", cinematicCueDirector);

            BossBarragePocketVfxCueBridge vfxBridge =
                RequireComponent<BossBarragePocketVfxCueBridge>(owner.gameObject, "pocket VFX cue bridge");
            ValidateObjectReference(vfxBridge, "pocketReviewOwner", owner);
            ValidateObjectReference(vfxBridge, "cuePlayer", cuePlayer);
            ValidateObjectReference(
                vfxBridge,
                "followupWindowAnchor",
                ReadObjectReference<Transform>(playerVfxCueDriver, "attackAnchor"));
            ValidateObjectReference(vfxBridge, "followupHitAnchor", directionTarget);
            ValidateObjectReference(
                vfxBridge,
                "followupMissedAnchor",
                ReadObjectReference<Transform>(playerVfxCueDriver, "dodgeAnchor"));
            ValidateObjectReference(vfxBridge, "pocketClearAnchor", directionTarget);
            ValidateObjectReference(
                vfxBridge,
                "pocketFailAnchor",
                ReadObjectReference<Transform>(playerVfxCueDriver, "dodgeAnchor"));
            ValidateObjectReference(vfxBridge, "directionTarget", directionTarget);
            ValidateFloat(vfxBridge, "hitIntensity", 1.18f);
            ValidateFloat(vfxBridge, "pocketClearIntensity", 0.92f);
            ValidateFloat(vfxBridge, "pocketFailIntensity", 1.02f);
            ValidateEnum(vfxBridge, "pocketFailAccentCueId", (int)CombatVfxCueId.EnemyClosePunishActive);
            ValidateFloat(vfxBridge, "pocketFailAccentIntensity", 0.88f);
        }

        private static void ValidateReviewHud(
            BossBarrageLaneReviewHud hud,
            CombatHealth playerHealth,
            CombatHealth closeThreatHealth,
            CombatHealth bossHealth,
            SummonEnergyLadder energyLadder,
            SummonLaneSpace laneSpace,
            Transform player,
            PlayerCombatModeController combatModeController,
            PlayerRangedAimController rangedAimController,
            PlayerRangedBasicAttackAction rangedBasicAttackAction,
            PlayerSkill1Action skill1Action,
            PlayerSummonSlot1Action summonSlot1Action,
            BossBarrageEmitter bossBarrageEmitter,
            BossBasicFireEmitter bossBasicFireEmitter,
            BossBarragePocketReviewOwner pocketOwner,
            FrontlineWaveStageProfile stageProfile,
            BossPressureCostLadder bossPressureCost,
            BossPressurePositionController bossPressurePosition,
            BossPressureActionDirector bossPressureActionDirector,
            BossSummonPressureAction bossSummonPressureAction,
            PlayerSupportSummonSlotAction summonSlot2Action,
            PlayerSupportSummonSlotAction summonSlot3Action)
        {
            ValidateObjectReference(hud, "playerHealth", playerHealth);
            ValidateObjectReference(hud, "closeThreatHealth", closeThreatHealth);
            ValidateObjectReference(hud, "bossHealth", bossHealth);
            ValidateObjectReference(hud, "energyLadder", energyLadder);
            ValidateObjectReference(hud, "laneSpace", laneSpace);
            ValidateObjectReference(hud, "player", player);
            ValidateObjectReference(hud, "combatModeController", combatModeController);
            ValidateObjectReference(hud, "rangedAimController", rangedAimController);
            ValidateObjectReference(hud, "rangedBasicAttackAction", rangedBasicAttackAction);
            ValidateObjectReference(hud, "skill1Action", skill1Action);
            ValidateObjectReference(hud, "summonSlot1Action", summonSlot1Action);
            ValidateObjectReference(hud, "summonSlot2Action", summonSlot2Action);
            ValidateObjectReference(hud, "summonSlot3Action", summonSlot3Action);
            ValidateObjectReference(hud, "bossBarrageEmitter", bossBarrageEmitter);
            ValidateObjectReference(hud, "bossBasicFireEmitter", bossBasicFireEmitter);
            ValidateObjectReference(hud, "bossPressureCostLadder", bossPressureCost);
            ValidateObjectReference(hud, "bossPressurePositionController", bossPressurePosition);
            ValidateObjectReference(hud, "bossPressureActionDirector", bossPressureActionDirector);
            ValidateObjectReference(hud, "bossSummonPressureAction", bossSummonPressureAction);
            ValidateObjectReference(hud, "pocketReviewOwner", pocketOwner);
            ValidateObjectReference(hud, "duelReviewOwner", null);
            ValidateObjectReference(hud, "stageProfile", stageProfile);
            if (hud.StageProfileForReview != stageProfile)
            {
                throw new InvalidOperationException($"{hud.name}.StageProfileForReview is not bound to {stageProfile.name}.");
            }

            ValidateBool(hud, "showCenterReticle", true);
            ValidateBool(hud, "showResultBanner", true);
            ValidateString(hud, "stageEpisodeLabel", stageProfile.StageEpisodeLabel);
            ValidateString(hud, "objectiveBadgeLabel", stageProfile.ObjectiveBadgeLabel);
            ValidateFloat(hud, "resultBannerWidth", 540f);
            ValidateFloat(hud, "resultBannerHeight", 82f);
            ValidateFloat(hud, "resultBannerBottomOffset", 112f);
        }

        private static void ValidateMobileReviewHud(
            BossBarrageLaneReviewMobileHud hud,
            PlayerMovementController movement,
            PlayerActionController actionController,
            PlayerCombatModeController combatModeController,
            PlayerRangedAimController rangedAimController,
            PlayerRangedBasicAttackAction rangedBasicAttackAction,
            PlayerSkill1Action skill1Action,
            PlayerSummonSlot1Action summonSlot1Action,
            PlayerSupportSummonSlotAction summonSlot2Action,
            PlayerSupportSummonSlotAction summonSlot3Action,
            SummonEnergyLadder energyLadder)
        {
            ValidateObjectReference(hud, "movement", movement);
            ValidateObjectReference(hud, "actionController", actionController);
            ValidateObjectReference(hud, "combatModeController", combatModeController);
            ValidateObjectReference(hud, "aimController", rangedAimController);
            ValidateObjectReference(hud, "rangedBasicAttackAction", rangedBasicAttackAction);
            ValidateObjectReference(hud, "skill1Action", skill1Action);
            ValidateObjectReference(hud, "summonSlot1Action", summonSlot1Action);
            ValidateObjectReference(hud, "summonSlot2Action", summonSlot2Action);
            ValidateObjectReference(hud, "summonSlot3Action", summonSlot3Action);
            ValidateObjectReference(hud, "energyLadder", energyLadder);
            ValidateString(hud, "moveActionName", "Move");
            ValidateString(hud, "basicDefenseActionName", "BasicDefenseAttack");
            ValidateString(hud, "dodgeActionName", "Dodge");
            ValidateString(hud, "skill1ActionName", "Skill1");
            ValidateString(hud, "summonSlot1ActionName", "SummonSlot1");
            ValidateString(hud, "summonSlot2ActionName", BossBarrageSummonReviewContract.Slot2ActionName);
            ValidateString(hud, "summonSlot3ActionName", BossBarrageSummonReviewContract.Slot3ActionName);
            ValidateString(hud, "rangedAimActionName", "RangedAim");
            ValidateString(hud, "weaponSwapActionName", "WeaponSwap");
            ValidateBool(hud, "useSingleSummonButton", BossBarrageSummonReviewContract.UseSingleSummonButton);
            ValidateString(hud, "summonSlot1Label", BossBarrageSummonReviewContract.Slot1HudLabel);
            ValidateString(hud, "summonSlot2Label", BossBarrageSummonReviewContract.Slot2HudLabel);
            ValidateString(hud, "summonSlot3Label", BossBarrageSummonReviewContract.Slot3HudLabel);
            ValidateString(hud, "lockedSummonLabel", BossBarrageSummonReviewContract.LockedSummonLabel);
            ValidateFloat(hud, "buttonSize", 168f);
            ValidateFloat(hud, "buttonGap", 38f);
            ValidateFloat(hud, "margin", 72f);
            ValidateFloat(hud, "minimumActionButtonSize", 124f);
            ValidateFloat(hud, "minimumButtonGap", 30f);
            ValidateFloat(hud, "minimumTouchEdgeInset", 64f);
            ValidateFloat(hud, "summonButtonGroupCenterY01", 0.42f);
            ValidateFloat(hud, "summonButtonGapMultiplier", 1.05f);
            ValidateFloat(hud, "moveJoystickRadius", 154f);
            ValidateFloat(hud, "moveJoystickKnobSize", 64f);
            ValidateFloat(hud, "moveJoystickTouchRadiusScale", 1.45f);
            ValidateFloat(hud, "minimumMoveJoystickRadius", 118f);
            ValidateFloat(hud, "minimumMoveJoystickKnobSize", 52f);
            ValidateBool(hud, "screenDragControlsAim", true);
            ValidateBool(hud, "rightMouseDragControlsAim", false);
            ValidateBool(hud, "leftMouseDragControlsAim", true);
            ValidateBool(hud, "routeAimToMovementLook", false);
            ValidateBool(hud, "keyboardPeekControlsAim", true);
            ValidateEnum(hud, "keyboardPeekLeftKey", (int)Key.Q);
            ValidateEnum(hud, "keyboardPeekRightKey", (int)Key.E);
            ValidateBool(hud, "keyboardPeekRequiresActiveAim", true);
            ValidateFloat(hud, "lookAimDragSensitivity", 0.00435f);
            ValidateBool(hud, "fireAimReticleUsesScreenCenter", true);
            ValidateBool(hud, "fireAimReticleFollowsAssist", true);
            ValidateFloat(hud, "fireAimAssistReticleMaxOffset", 96f);
        }

        private static void ValidateActionScreenCuePresenter(
            ActionScreenCuePresenter presenter,
            PlayerActionController actionController,
            CombatHealth playerHealth,
            PlayerRangedBasicAttackAction rangedBasicAttackAction,
            SummonEnergyLadder energyLadder,
            PlayerSkill1Action skill1Action,
            PlayerSummonSlot1Action summonSlot1Action,
            PlayerSupportSummonSlotAction summonSlot2Action,
            PlayerSupportSummonSlotAction summonSlot3Action,
            BossBarrageEmitter bossBarrageEmitter,
            BossPressureActionDirector bossPressureActionDirector,
            BossBarragePocketReviewOwner pocketOwner)
        {
            ValidateObjectReference(presenter, "actionController", actionController);
            ValidateObjectReference(presenter, "playerHealth", playerHealth);
            ValidateObjectReference(presenter, "rangedBasicAttackAction", rangedBasicAttackAction);
            ValidateObjectReference(presenter, "energyLadder", energyLadder);
            ValidateObjectReference(presenter, "skill1Action", skill1Action);
            ValidateObjectReference(presenter, "summonSlot1Action", summonSlot1Action);
            ValidateObjectReference(presenter, "summonSlot2Action", summonSlot2Action);
            ValidateObjectReference(presenter, "summonSlot3Action", summonSlot3Action);
            ValidateObjectReference(presenter, "bossBarrageEmitter", bossBarrageEmitter);
            ValidateObjectReference(presenter, "bossPressureActionDirector", bossPressureActionDirector);
            ValidateObjectReference(presenter, "pocketReviewOwner", pocketOwner);
            ValidateObjectReference(presenter, "duelReviewOwner", null);
            ValidateBool(presenter, "showScreenCues", true);
            ValidateBool(presenter, "showEventColorCues", false);
            ValidateFloat(presenter, "maxFullScreenAlpha", 0.10f);
            ValidateFloat(presenter, "maxEdgeAlpha", 0.26f);
            ValidateFloat(presenter, "edgeThickness", 104f);
            ValidateFloat(presenter, "maxPerfectDodgeDomainAlpha", 0.42f);
            ValidateFloat(presenter, "maxPerfectDodgeInvertAlpha", 0.18f);
            ValidateFloat(presenter, "maxPerfectDodgeEdgeAlpha", 0.48f);
            ValidateFloat(presenter, "perfectDodgeDomainSeconds", 3f);
            ValidateFloat(presenter, "perfectDodgePulseSeconds", 0.22f);
            ValidateFloat(presenter, "perfectDodgeBandThickness", 26f);
            ValidateObjectReference(presenter, "perfectDodgeDomainMaterial", LoadOrCreatePerfectDodgeScreenDomainMaterial());
            ValidateObjectReference(
                presenter,
                "perfectDodgeGlitchOverlayMaterial",
                LoadAsset<Material>(PerfectDodgeGlitchOverlayMaterialPath));
            ValidateFloat(presenter, "perfectDodgeShaderIntensity", PerfectDodgeScreenShaderIntensity);
            ValidateFloat(presenter, "perfectDodgeRadialWarpStrength", PerfectDodgeScreenRadialWarpStrength);
            ValidateFloat(presenter, "perfectDodgeScanlineStrength", PerfectDodgeScreenScanlineStrength);
            ValidateFloat(presenter, "perfectDodgeRadialBlurStrength", PerfectDodgeScreenRadialBlurStrength);
            ValidateFloat(presenter, "perfectDodgeGridStrength", PerfectDodgeScreenGridStrength);
            ValidateFloat(presenter, "perfectDodgeFractureStrength", PerfectDodgeScreenFractureStrength);
            ValidateFloat(presenter, "perfectDodgeChromaticStrength", PerfectDodgeScreenChromaticStrength);
            ValidateFloat(presenter, "perfectDodgeGlitchOverlayAlpha", PerfectDodgeGlitchOverlayAlpha);
            ValidateFloat(presenter, "perfectDodgeGlitchNoiseStrength", PerfectDodgeGlitchNoiseStrength);
            ValidateFloat(presenter, "perfectDodgeGlitchJitterStrength", PerfectDodgeGlitchJitterStrength);
            ValidateBool(presenter, "useDamageScreenFeedback", false);
            ValidateFloat(presenter, "maxDamageVignetteAlpha", 0.42f);
            ValidateFloat(presenter, "maxDamageFlashAlpha", 0.11f);
            ValidateFloat(presenter, "damageVignetteSeconds", 0.34f);
            ValidateFloat(presenter, "pressureDamageFeedbackScale", 0.58f);
            ValidateFloat(presenter, "controlLockDamageExtraSeconds", 0.10f);
            ValidateFloat(presenter, "heavyDamageExtraSeconds", 0.14f);
            ValidateFloat(presenter, "heavyDamageHealthRatio", 0.26f);
            ValidateFloat(presenter, "criticalHealthThreshold", 0.32f);
            ValidateFloat(presenter, "criticalHealthPulseAlpha", 0.13f);
            ValidateFloat(presenter, "criticalHealthPulseSeconds", 0.9f);
            ValidateFloat(presenter, "criticalHealthPulseRate", 2.3f);
            ValidateFloat(presenter, "damageDirectionAccentAlpha", 0.24f);
            ValidateFloat(presenter, "damageDirectionAccentThickness", 178f);
        }

        private static void ConfigureArenaInfluenceTargets(Scene scene, Transform player, params Transform[] influenceTargets)
        {
            ActionFoundationArenaShapeInfluenceDriver[] drivers = CollectComponents<ActionFoundationArenaShapeInfluenceDriver>(scene);
            var targets = new UnityEngine.Object[1 + (influenceTargets != null ? influenceTargets.Length : 0)];
            targets[0] = player;
            if (influenceTargets != null)
            {
                for (int i = 0; i < influenceTargets.Length; i++)
                {
                    targets[i + 1] = influenceTargets[i];
                }
            }

            for (int i = 0; i < drivers.Length; i++)
            {
                SetObjectReferenceArray(drivers[i], "influenceTargets", targets);
            }
        }

        private static void DestroyChildIfPresent(Transform parent, string childName)
        {
            Transform existing = parent.Find(childName);
            if (existing != null)
            {
                UnityEngine.Object.DestroyImmediate(existing.gameObject);
            }
        }

        private static void DestroyDescendantsIfPresent(Transform root, params string[] childNames)
        {
            if (root == null || childNames == null)
            {
                return;
            }

            for (int i = 0; i < childNames.Length; i++)
            {
                string childName = childNames[i];
                if (string.IsNullOrWhiteSpace(childName))
                {
                    continue;
                }

                while (true)
                {
                    Transform existing = FindDescendant(root, childName);
                    if (existing == null || existing == root)
                    {
                        break;
                    }

                    UnityEngine.Object.DestroyImmediate(existing.gameObject);
                }
            }
        }

        private static GameObject FindPlayerMeleeVisualRoot(Transform player)
        {
            Transform swordShieldVisual = FindDescendant(player, "CombatGirlSwordShield_PlayerVisual");
            if (swordShieldVisual != null)
            {
                return swordShieldVisual.gameObject;
            }

            Transform placeholderBody = FindDescendant(player, "CombatGirlPlaceholderBody");
            return placeholderBody != null ? placeholderBody.gameObject : null;
        }

        private static Transform FindLikelyRightHandSocket(Transform root)
        {
            Transform humanoidHand = FindHumanoidHandSocket(root, HumanBodyBones.RightHand);
            if (humanoidHand != null)
            {
                return humanoidHand;
            }

            Transform[] candidates = root.GetComponentsInChildren<Transform>(includeInactive: true);
            for (int i = 0; i < candidates.Length; i++)
            {
                string normalized = NormalizeTransformName(candidates[i].name);
                if (normalized.Contains("righthand", StringComparison.Ordinal)
                    || normalized.Contains("rhand", StringComparison.Ordinal)
                    || normalized.Contains("handr", StringComparison.Ordinal)
                    || (normalized.Contains("right", StringComparison.Ordinal)
                        && normalized.Contains("hand", StringComparison.Ordinal)))
                {
                    return candidates[i];
                }
            }

            return null;
        }

        private static Transform FindLikelyLeftHandSocket(Transform root)
        {
            Transform humanoidHand = FindHumanoidHandSocket(root, HumanBodyBones.LeftHand);
            if (humanoidHand != null)
            {
                return humanoidHand;
            }

            Transform[] candidates = root.GetComponentsInChildren<Transform>(includeInactive: true);
            for (int i = 0; i < candidates.Length; i++)
            {
                string normalized = NormalizeTransformName(candidates[i].name);
                if (normalized.Contains("lefthand", StringComparison.Ordinal)
                    || normalized.Contains("lhand", StringComparison.Ordinal)
                    || normalized.Contains("handl", StringComparison.Ordinal)
                    || (normalized.Contains("left", StringComparison.Ordinal)
                        && normalized.Contains("hand", StringComparison.Ordinal)))
                {
                    return candidates[i];
                }
            }

            return null;
        }

        private static Transform FindHumanoidHandSocket(Transform root, HumanBodyBones handBone)
        {
            if (root == null)
            {
                return null;
            }

            Animator[] animators = root.GetComponentsInChildren<Animator>(includeInactive: true);
            for (int i = 0; i < animators.Length; i++)
            {
                Animator animator = animators[i];
                if (animator == null || animator.avatar == null || !animator.avatar.isHuman)
                {
                    continue;
                }

                Transform hand = animator.GetBoneTransform(handBone);
                if (hand != null && hand.IsChildOf(root))
                {
                    return hand;
                }
            }

            return null;
        }

        private static Transform FindDescendant(Transform root, string childName)
        {
            if (root == null)
            {
                return null;
            }

            Transform[] children = root.GetComponentsInChildren<Transform>(includeInactive: true);
            for (int i = 0; i < children.Length; i++)
            {
                if (string.Equals(children[i].name, childName, StringComparison.Ordinal))
                {
                    return children[i];
                }
            }

            return null;
        }

        private static Transform RequireDescendant(Transform root, string childName)
        {
            Transform descendant = FindDescendant(root, childName);
            if (descendant == null)
            {
                throw new InvalidOperationException($"{root.name} must contain descendant {childName}.");
            }

            return descendant;
        }

        private static string NormalizeTransformName(string value)
        {
            return value
                .Replace(" ", string.Empty)
                .Replace("_", string.Empty)
                .Replace("-", string.Empty)
                .Replace(":", string.Empty)
                .Replace(".", string.Empty)
                .ToLowerInvariant();
        }

        private static void AssignInoriPlayerMaterials(GameObject visualRoot)
        {
            Renderer[] renderers = visualRoot.GetComponentsInChildren<Renderer>(includeInactive: true);
            for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
            {
                Renderer renderer = renderers[rendererIndex];
                Material[] materials = renderer.sharedMaterials;
                for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                {
                    string hint = materials[materialIndex]?.name ?? string.Empty;
                    materials[materialIndex] =
                        ActionFoundationInoriPlayerVisualAssetSetup.ResolvePromotedMaterial(hint, materialIndex);
                }

                renderer.sharedMaterials = materials;
                EditorUtility.SetDirty(renderer);
            }
        }

        private static void AssignRangedCandidateMaterials(GameObject visualRoot)
        {
            Renderer[] renderers = visualRoot.GetComponentsInChildren<Renderer>(includeInactive: true);
            for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
            {
                Renderer renderer = renderers[rendererIndex];
                Material[] materials = renderer.sharedMaterials;
                for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                {
                    string hint = $"{renderer.name} {materials[materialIndex]?.name ?? string.Empty}";
                    materials[materialIndex] = ResolveRangedCandidateMaterial(hint, materialIndex);
                }

                renderer.sharedMaterials = materials;
                EditorUtility.SetDirty(renderer);
            }
        }

        private static void RemapInoriPlayerMeshes(GameObject visualRoot)
        {
            Dictionary<string, Mesh> promotedMeshes =
                LoadPromotedMeshMap(ActionFoundationInoriPlayerVisualAssetSetup.ModelPath);

            MeshFilter[] meshFilters = visualRoot.GetComponentsInChildren<MeshFilter>(includeInactive: true);
            for (int i = 0; i < meshFilters.Length; i++)
            {
                Mesh promotedMesh = ResolvePromotedMesh(meshFilters[i].sharedMesh, promotedMeshes, $"{meshFilters[i].name} mesh");
                if (meshFilters[i].sharedMesh != promotedMesh)
                {
                    meshFilters[i].sharedMesh = promotedMesh;
                    EditorUtility.SetDirty(meshFilters[i]);
                }
            }

            SkinnedMeshRenderer[] skinnedRenderers =
                visualRoot.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true);
            for (int i = 0; i < skinnedRenderers.Length; i++)
            {
                Mesh promotedMesh = ResolvePromotedMesh(
                    skinnedRenderers[i].sharedMesh,
                    promotedMeshes,
                    $"{skinnedRenderers[i].name} skinned mesh");
                if (skinnedRenderers[i].sharedMesh != promotedMesh)
                {
                    skinnedRenderers[i].sharedMesh = promotedMesh;
                    EditorUtility.SetDirty(skinnedRenderers[i]);
                }
            }
        }

        private static void RemapRangedCandidateMeshes(GameObject visualRoot)
        {
            Dictionary<string, Mesh> promotedMeshes = LoadPromotedMeshMap(
                ActionFoundationPlayerCombatModeAssetSetup.RangedCandidateModelPath,
                ActionFoundationPlayerCombatModeAssetSetup.RangedCandidateWeaponModelPath);

            MeshFilter[] meshFilters = visualRoot.GetComponentsInChildren<MeshFilter>(includeInactive: true);
            for (int i = 0; i < meshFilters.Length; i++)
            {
                Mesh promotedMesh = ResolvePromotedMesh(meshFilters[i].sharedMesh, promotedMeshes, $"{meshFilters[i].name} mesh");
                if (meshFilters[i].sharedMesh != promotedMesh)
                {
                    meshFilters[i].sharedMesh = promotedMesh;
                    EditorUtility.SetDirty(meshFilters[i]);
                }
            }

            SkinnedMeshRenderer[] skinnedRenderers =
                visualRoot.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true);
            for (int i = 0; i < skinnedRenderers.Length; i++)
            {
                Mesh promotedMesh = ResolvePromotedMesh(
                    skinnedRenderers[i].sharedMesh,
                    promotedMeshes,
                    $"{skinnedRenderers[i].name} skinned mesh");
                if (skinnedRenderers[i].sharedMesh != promotedMesh)
                {
                    skinnedRenderers[i].sharedMesh = promotedMesh;
                    EditorUtility.SetDirty(skinnedRenderers[i]);
                }
            }
        }

        private static Dictionary<string, Mesh> LoadPromotedMeshMap(params string[] assetPaths)
        {
            var meshes = new Dictionary<string, Mesh>(StringComparer.Ordinal);
            for (int pathIndex = 0; pathIndex < assetPaths.Length; pathIndex++)
            {
                UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPaths[pathIndex]);
                for (int assetIndex = 0; assetIndex < assets.Length; assetIndex++)
                {
                    if (assets[assetIndex] is Mesh mesh && !meshes.ContainsKey(mesh.name))
                    {
                        meshes.Add(mesh.name, mesh);
                    }
                }
            }

            if (meshes.Count == 0)
            {
                throw new InvalidOperationException("Promoted RifleGirl mesh assets are missing.");
            }

            return meshes;
        }

        private static Mesh ResolvePromotedMesh(
            Mesh sourceMesh,
            IReadOnlyDictionary<string, Mesh> promotedMeshes,
            string label)
        {
            if (sourceMesh == null)
            {
                return null;
            }

            string sourcePath = AssetDatabase.GetAssetPath(sourceMesh).Replace('\\', '/');
            if (sourcePath.StartsWith("Assets/_Game/", StringComparison.Ordinal))
            {
                return sourceMesh;
            }

            if (!sourcePath.Contains("/_Imported/", StringComparison.Ordinal))
            {
                return sourceMesh;
            }

            if (promotedMeshes.TryGetValue(sourceMesh.name, out Mesh promotedMesh))
            {
                return promotedMesh;
            }

            throw new InvalidOperationException($"Missing promoted RifleGirl mesh for {label}: {sourceMesh.name}.");
        }

        private static Avatar LoadPromotedRifleGirlAvatar()
        {
            string assetPath = ActionFoundationPlayerCombatModeAssetSetup.RangedCandidateModelPath;
            GameObject promotedModel = LoadAsset<GameObject>(assetPath);
            Animator promotedAnimator = promotedModel.GetComponent<Animator>();
            if (promotedAnimator != null && promotedAnimator.avatar != null)
            {
                return promotedAnimator.avatar;
            }

            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is Avatar avatar)
                {
                    return avatar;
                }
            }

            throw new InvalidOperationException("Promoted RifleGirl model must expose a game-owned Avatar.");
        }

        private static InoriRiflePoseTuningProfile LoadInoriRiflePoseTuningProfile()
        {
            InoriRiflePoseTuningProfile profile =
                AssetDatabase.LoadAssetAtPath<InoriRiflePoseTuningProfile>(
                    ActionFoundationInoriPlayerVisualAssetSetup.RiflePoseTuningProfilePath);
            if (profile == null)
            {
                throw new InvalidOperationException(
                    $"Missing Inori rifle pose tuning profile at {ActionFoundationInoriPlayerVisualAssetSetup.RiflePoseTuningProfilePath}.");
            }

            return profile;
        }

        private static void StripNonGameMonoBehaviours(GameObject root)
        {
            Transform[] transforms = root.GetComponentsInChildren<Transform>(includeInactive: true);
            for (int i = 0; i < transforms.Length; i++)
            {
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(transforms[i].gameObject);
            }

            MonoBehaviour[] behaviours = root.GetComponentsInChildren<MonoBehaviour>(includeInactive: true);
            for (int i = behaviours.Length - 1; i >= 0; i--)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour == null)
                {
                    continue;
                }

                MonoScript script = MonoScript.FromMonoBehaviour(behaviour);
                string scriptPath = script != null
                    ? AssetDatabase.GetAssetPath(script).Replace('\\', '/')
                    : string.Empty;
                if (!scriptPath.StartsWith("Assets/_Game/", StringComparison.Ordinal)
                    && !ShouldPreserveImportedRuntimeMonoBehaviour(scriptPath))
                {
                    UnityEngine.Object.DestroyImmediate(behaviour);
                }
            }
        }

        private static bool ShouldPreserveImportedRuntimeMonoBehaviour(string scriptPath)
        {
            if (string.IsNullOrWhiteSpace(scriptPath))
            {
                return false;
            }

            for (int i = 0; i < PreservedImportedRuntimeScriptPrefixes.Length; i++)
            {
                if (scriptPath.StartsWith(PreservedImportedRuntimeScriptPrefixes[i], StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static Material ResolveRangedCandidateMaterial(string hint, int slotIndex)
        {
            string lower = hint.ToLowerInvariant();
            if (lower.Contains("eye"))
            {
                return LoadAsset<Material>("Assets/_Game/Art/Characters/Player/RifleGirl/Materials/DB_RifleGirl_Eye.mat");
            }

            if (lower.Contains("face"))
            {
                return LoadAsset<Material>("Assets/_Game/Art/Characters/Player/RifleGirl/Materials/DB_RifleGirl_Face.mat");
            }

            if (lower.Contains("hair"))
            {
                return LoadAsset<Material>("Assets/_Game/Art/Characters/Player/RifleGirl/Materials/DB_RifleGirl_Hair01.mat");
            }

            if (lower.Contains("cloth"))
            {
                return LoadAsset<Material>("Assets/_Game/Art/Characters/Player/RifleGirl/Materials/DB_RifleGirl_Cloth01.mat");
            }

            if (lower.Contains("sport"))
            {
                return LoadAsset<Material>("Assets/_Game/Art/Characters/Player/RifleGirl/Materials/DB_RifleGirl_Sportswear.mat");
            }

            if (lower.Contains("weapon") || lower.Contains("rifle"))
            {
                return LoadAsset<Material>("Assets/_Game/Art/Characters/Player/RifleGirl/Materials/DB_RifleGirl_RangedFocus.mat");
            }

            return slotIndex switch
            {
                1 => LoadAsset<Material>("Assets/_Game/Art/Characters/Player/RifleGirl/Materials/DB_RifleGirl_Face.mat"),
                2 => LoadAsset<Material>("Assets/_Game/Art/Characters/Player/RifleGirl/Materials/DB_RifleGirl_Eye.mat"),
                3 => LoadAsset<Material>("Assets/_Game/Art/Characters/Player/RifleGirl/Materials/DB_RifleGirl_Hair01.mat"),
                _ => LoadAsset<Material>("Assets/_Game/Art/Characters/Player/RifleGirl/Materials/DB_RifleGirl_Body.mat")
            };
        }

        private static void AssignMaterialToAllRenderers(GameObject visualRoot, Material material)
        {
            Renderer[] renderers = visualRoot.GetComponentsInChildren<Renderer>(includeInactive: true);
            for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
            {
                Material[] materials = renderers[rendererIndex].sharedMaterials;
                for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                {
                    materials[materialIndex] = material;
                }

                renderers[rendererIndex].sharedMaterials = materials;
                EditorUtility.SetDirty(renderers[rendererIndex]);
            }
        }

        private static void ValidatePlayerCombatModeVisual(
            GameObject rangedRoot,
            Animator rangedAnimator,
            GameObject rangedWeaponRoot,
            GameObject meleeWeaponRoot)
        {
            if (!rangedRoot.activeSelf)
            {
                throw new InvalidOperationException("Ranged player visual root should be active for the review scene starting mode.");
            }

            if (rangedAnimator.runtimeAnimatorController
                != LoadAsset<RuntimeAnimatorController>(InoriRifleAnimatorControllerPath))
            {
                throw new InvalidOperationException("Inori ranged player visual must use its dedicated controller, not the RifleGirl source controller.");
            }

            ValidateGameOwnedAsset(rangedAnimator.avatar, "Ranged player visual Avatar");
            if (rangedAnimator.runtimeAnimatorController is not AnimatorController inoriController
                || inoriController.layers.Length == 0
                || !inoriController.layers[0].iKPass)
            {
                throw new InvalidOperationException("Ranged Animator Controller must keep IK pass enabled for the support hand.");
            }

            if (rangedAnimator.cullingMode != AnimatorCullingMode.AlwaysAnimate)
            {
                throw new InvalidOperationException("Ranged player Animator must always animate so weapon IK and ranged clips keep updating off-center.");
            }

            Renderer[] renderers = rangedRoot.GetComponentsInChildren<Renderer>(includeInactive: true);
            if (renderers.Length == 0)
            {
                throw new InvalidOperationException("Ranged player visual must contain renderers.");
            }

            for (int i = 0; i < renderers.Length; i++)
            {
                ValidateRendererUsesGameOwnedAssets(renderers[i], renderers[i].name);
                Material[] materials = renderers[i].sharedMaterials;
                for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                {
                    if (materials[materialIndex] != null)
                    {
                        ValidateRenderableMaterialShader(materials[materialIndex], $"{renderers[i].name} material shader");
                    }
                }
            }

            Transform weapon = FindDescendant(rangedRoot.transform, RangedPlayerWeaponName);
            if (weapon == null)
            {
                throw new InvalidOperationException($"Ranged player visual is missing {RangedPlayerWeaponName}.");
            }

            if (weapon.parent == rangedRoot.transform)
            {
                throw new InvalidOperationException("Ranged weapon must stay under the RifleGirl authored weapon hierarchy.");
            }

            if (!weapon.IsChildOf(rangedAnimator.transform))
            {
                throw new InvalidOperationException("Ranged weapon must be parented to the active player model.");
            }

            if (weapon.GetComponentsInChildren<Renderer>(includeInactive: true).Length == 0)
            {
                throw new InvalidOperationException($"{RangedPlayerWeaponName} must include visible renderers.");
            }

            ValidateInoriRetargetedRifleMeshCorrection(weapon);

            RifleGirlWeaponSocketDriver weaponSocketDriver =
                rangedAnimator.GetComponent<RifleGirlWeaponSocketDriver>();
            if (weaponSocketDriver == null || !weaponSocketDriver.IsConfigured)
            {
                throw new InvalidOperationException("Ranged player visual must bind the RifleGirl rifle socket driver.");
            }

            ValidateObjectReference(weaponSocketDriver, "animator", rangedAnimator);
            ParentConstraint weaponConstraint = weapon.GetComponent<ParentConstraint>();
            RetargetedHandWeaponAttachment retargetedAttachment =
                weapon.GetComponent<RetargetedHandWeaponAttachment>();
            Transform rightHandSocket = FindLikelyRightHandSocket(rangedRoot.transform);
            bool hasManualHandParent = rightHandSocket != null && weapon.IsChildOf(rightHandSocket);
            if (weaponConstraint == null
                && (retargetedAttachment == null || !retargetedAttachment.IsConfigured)
                && !hasManualHandParent)
            {
                throw new InvalidOperationException($"{RangedPlayerWeaponName} must keep a valid hand attachment.");
            }

            if (weaponConstraint != null)
            {
                ValidateObjectReference(weaponSocketDriver, "rifleConstraint", weaponConstraint);
            }

            Transform leftHandle = FindDescendant(weapon.transform, "Left_Handle");
            if (leftHandle == null)
            {
                throw new InvalidOperationException($"{RangedPlayerWeaponName} must expose Left_Handle for support-hand IK.");
            }

            ValidateObjectReference(weaponSocketDriver, "leftHandIkTarget", leftHandle);
            ValidateString(weaponSocketDriver, "defaultCommands", "To_Hand_R_Socket, IK_OFF_Left_Handle");
            ValidateString(weaponSocketDriver, "handSocketCommand", "To_Hand_R_Socket");
            ValidateString(weaponSocketDriver, "holsterSocketCommand", "To_Put_Socket_Rifle");
            ValidateString(weaponSocketDriver, "aimSocketCommand", "To_add_weapon_r");
            ValidateString(weaponSocketDriver, "leftIkOnCommand", "IK_ON_Left_Handle");
            ValidateString(weaponSocketDriver, "leftIkOffCommand", "IK_OFF_Left_Handle");
            ValidateBool(weaponSocketDriver, "ignoreRedundantSocketCommands", true);
            InoriRiflePoseTuningProfile tuningProfile = LoadInoriRiflePoseTuningProfile();
            float expectedLeftIkPositionWeight = tuningProfile.EnabledForGameplay
                ? tuningProfile.LeftIkPositionWeight
                : 0f;
            float expectedLeftIkRotationWeight = tuningProfile.EnabledForGameplay
                ? tuningProfile.LeftIkRotationWeight
                : 0f;
            ValidateFloat(weaponSocketDriver, "leftIkMaxWeight", expectedLeftIkPositionWeight);
            ValidateFloat(weaponSocketDriver, "leftIkRotationMaxWeight", expectedLeftIkRotationWeight);

            if (rightHandSocket == null)
            {
                throw new InvalidOperationException("Ranged player visual must expose a right-hand socket for weapon attachment.");
            }

            if (FindLikelyLeftHandSocket(rangedRoot.transform) == null)
            {
                throw new InvalidOperationException("Ranged player visual must expose a left-hand socket for support-hand IK.");
            }

            if (rangedWeaponRoot != weapon.gameObject)
            {
                throw new InvalidOperationException("Combat mode controller must reference the actual ranged weapon root.");
            }

            if (!rangedWeaponRoot.activeSelf)
            {
                throw new InvalidOperationException("Ranged weapon should start active with the ranged channel.");
            }

            if (meleeWeaponRoot.activeSelf)
            {
                throw new InvalidOperationException("Extracted melee weapons should start inactive because the review scene starts in ranged mode.");
            }

            if (meleeWeaponRoot.GetComponentsInChildren<Renderer>(includeInactive: true).Length == 0)
            {
                throw new InvalidOperationException("Extracted melee weapon root must include visible sword/shield renderers.");
            }

            if (!meleeWeaponRoot.transform.IsChildOf(rangedRoot.transform))
            {
                throw new InvalidOperationException("Extracted melee weapons should live under the persistent player visual root.");
            }

            CombatGirlWeaponSocketBinder meleeWeaponBinder =
                meleeWeaponRoot.GetComponent<CombatGirlWeaponSocketBinder>();
            if (meleeWeaponBinder == null || !meleeWeaponBinder.AllBindingsValid)
            {
                throw new InvalidOperationException("Extracted melee weapons must bind to the persistent player hand sockets.");
            }
        }

        private static void ValidateInoriRetargetedRifleMeshCorrection(Transform weaponRoot)
        {
            InoriRiflePoseTuningProfile tuningProfile = LoadInoriRiflePoseTuningProfile();
            Transform rifleMesh = FindDescendant(weaponRoot, tuningProfile.RifleMeshName);
            if (rifleMesh == null)
            {
                throw new InvalidOperationException($"{weaponRoot.name} is missing {tuningProfile.RifleMeshName}.");
            }

            if ((rifleMesh.localPosition - tuningProfile.RifleMeshLocalPosition).sqrMagnitude > 0.000001f)
            {
                throw new InvalidOperationException(
                    $"{tuningProfile.RifleMeshName}.localPosition drifted from the reviewed Inori hand alignment.");
            }

            if (Quaternion.Angle(rifleMesh.localRotation, tuningProfile.RifleMeshLocalRotation) > 0.01f)
            {
                throw new InvalidOperationException(
                    $"{tuningProfile.RifleMeshName}.localRotation drifted from the reviewed Inori hand alignment.");
            }
        }

        private static void ValidateRendererUsesGameOwnedAssets(Renderer renderer, string label)
        {
            MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                ValidateGameOwnedAsset(meshFilter.sharedMesh, $"{label} mesh");
            }

            SkinnedMeshRenderer skinnedMeshRenderer = renderer as SkinnedMeshRenderer;
            if (skinnedMeshRenderer != null && skinnedMeshRenderer.sharedMesh != null)
            {
                ValidateGameOwnedAsset(skinnedMeshRenderer.sharedMesh, $"{label} skinned mesh");
            }

            Material[] materials = renderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] != null)
                {
                    ValidateGameOwnedAsset(materials[i], $"{label} material");
                    ValidateRenderableMaterialShader(materials[i], $"{label} material shader");
                }
            }
        }

        private static void ValidateRenderableMaterialShader(Material material, string label)
        {
            if (material.shader == null ||
                string.Equals(material.shader.name, "Hidden/InternalErrorShader", StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"{label} must not use Unity's missing/error shader.");
            }
        }

        private static void ValidateVefectsFlipbookMaterial(Material material, string label)
        {
            ValidateGameOwnedAsset(material.shader, $"{label} shader");
            if (!material.HasProperty("_Flipbook"))
            {
                throw new InvalidOperationException($"{label} should use the promoted Vefects flipbook shader.");
            }

            Texture flipbook = material.GetTexture("_Flipbook");
            if (flipbook == null)
            {
                throw new InvalidOperationException($"{label} should keep an assigned Vefects flipbook texture.");
            }

            ValidateGameOwnedAsset(flipbook, $"{label} flipbook texture");
        }

        private static Material LoadOrCreateMaterial(string assetPath, Color color)
        {
            EnsureFolderForAsset(assetPath);
            Material material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            if (material == null)
            {
                material = new Material(ResolveUnlitShader());
                AssetDatabase.CreateAsset(material, assetPath);
            }

            if (material.shader == null)
            {
                material.shader = ResolveUnlitShader();
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
                material.SetColor("_EmissionColor", color * 1.35f);
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material LoadOrCreateTransparentMaterial(string assetPath, Color color)
        {
            Material material = LoadOrCreateMaterial(assetPath, color);
            SetMaterialFloatIfPresent(material, "_Surface", 1f);
            SetMaterialFloatIfPresent(material, "_Blend", 0f);
            SetMaterialFloatIfPresent(material, "_SrcBlend", (float)BlendMode.SrcAlpha);
            SetMaterialFloatIfPresent(material, "_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            SetMaterialFloatIfPresent(material, "_ZWrite", 0f);
            material.renderQueue = (int)RenderQueue.Transparent;
            material.SetOverrideTag("RenderType", "Transparent");
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material LoadOrCreatePerfectDodgeScreenDomainMaterial()
        {
            EnsureFolderForAsset(PerfectDodgeScreenDomainMaterialPath);
            Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(PerfectDodgeScreenDomainShaderPath);
            if (shader == null)
            {
                throw new InvalidOperationException(
                    $"Missing perfect dodge screen domain shader at {PerfectDodgeScreenDomainShaderPath}.");
            }

            Material material = AssetDatabase.LoadAssetAtPath<Material>(PerfectDodgeScreenDomainMaterialPath);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, PerfectDodgeScreenDomainMaterialPath);
            }

            material.shader = shader;
            material.renderQueue = (int)RenderQueue.Overlay;
            SetMaterialColorIfPresent(material, "_DomainColor", new Color(0.025f, 0.035f, 0.045f, 1f));
            SetMaterialColorIfPresent(material, "_EdgeColor", new Color(0.12f, 0.96f, 1f, 1f));
            SetMaterialColorIfPresent(material, "_InvertColor", new Color(0.92f, 1f, 1f, 1f));
            SetMaterialFloatIfPresent(material, "_DomainAlpha", 0.42f);
            SetMaterialFloatIfPresent(material, "_InvertAlpha", 0.18f);
            SetMaterialFloatIfPresent(material, "_EdgeAlpha", 0.48f);
            SetMaterialFloatIfPresent(material, "_BandAlpha", 0.13f);
            SetMaterialFloatIfPresent(material, "_Intensity", PerfectDodgeScreenShaderIntensity);
            SetMaterialFloatIfPresent(material, "_Sustain", 1f);
            SetMaterialFloatIfPresent(material, "_Age01", 0f);
            SetMaterialFloatIfPresent(material, "_Pulse", 0f);
            SetMaterialFloatIfPresent(material, "_RadialWarp", PerfectDodgeScreenRadialWarpStrength);
            SetMaterialFloatIfPresent(material, "_ScanlineStrength", PerfectDodgeScreenScanlineStrength);
            SetMaterialFloatIfPresent(material, "_RadialBlurStrength", PerfectDodgeScreenRadialBlurStrength);
            SetMaterialFloatIfPresent(material, "_GridStrength", PerfectDodgeScreenGridStrength);
            SetMaterialFloatIfPresent(material, "_FractureStrength", PerfectDodgeScreenFractureStrength);
            SetMaterialFloatIfPresent(material, "_ChromaticStrength", PerfectDodgeScreenChromaticStrength);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material LoadOrCreatePerfectDodgeWorldFxMaterial()
        {
            Material material = LoadOrCreatePerfectDodgeMaterial(
                PerfectDodgeWorldFxMaterialPath,
                PerfectDodgeWorldFxShaderPath);
            SetMaterialColorIfPresent(material, "_ColorA", new Color(0.12f, 0.96f, 1f, 0.82f));
            SetMaterialColorIfPresent(material, "_ColorB", new Color(0.58f, 0.24f, 1f, 0.72f));
            SetMaterialFloatIfPresent(material, "_Alpha", 0.7f);
            SetMaterialFloatIfPresent(material, "_Intensity", 1.35f);
            SetMaterialFloatIfPresent(material, "_RimPower", 2.8f);
            SetMaterialFloatIfPresent(material, "_NoiseScale", 7f);
            return material;
        }

        private static Material LoadOrCreatePerfectDodgeAfterimageMaterial()
        {
            Material material = LoadOrCreatePerfectDodgeMaterial(
                PerfectDodgeAfterimageMaterialPath,
                PerfectDodgeAfterimageShaderPath);
            SetMaterialColorIfPresent(material, "_BaseColor", new Color(0.34f, 0.98f, 1f, 0.42f));
            SetMaterialColorIfPresent(material, "_RimColor", new Color(0.72f, 0.36f, 1f, 0.9f));
            SetMaterialFloatIfPresent(material, "_Alpha", 0.42f);
            SetMaterialFloatIfPresent(material, "_Intensity", 1f);
            SetMaterialFloatIfPresent(material, "_FresnelPower", 2.2f);
            SetMaterialFloatIfPresent(material, "_ScanStrength", 0.48f);
            return material;
        }

        private static Material LoadOrCreatePerfectDodgeMaterial(string materialPath, string shaderPath)
        {
            EnsureFolderForAsset(materialPath);
            Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(shaderPath);
            if (shader == null)
            {
                throw new InvalidOperationException($"Missing perfect dodge shader at {shaderPath}.");
            }

            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, materialPath);
            }

            material.shader = shader;
            material.enableInstancing = true;
            material.renderQueue = (int)RenderQueue.Transparent;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static AudioClip[] LoadAudioClipArray(string[] clipPaths)
        {
            AudioClip[] clips = new AudioClip[clipPaths.Length];
            for (int i = 0; i < clipPaths.Length; i++)
            {
                clips[i] = LoadAsset<AudioClip>(clipPaths[i]);
            }

            return clips;
        }

        private static void ValidateAudioClipArray(UnityEngine.Object target, string propertyName, string[] expectedClipPaths)
        {
            SerializedProperty array = RequireProperty(new SerializedObject(target), propertyName);
            if (!array.isArray || array.arraySize != expectedClipPaths.Length)
            {
                throw new InvalidOperationException(
                    $"{target.name}.{propertyName} expected {expectedClipPaths.Length} audio clips, found {array.arraySize}.");
            }

            for (int i = 0; i < expectedClipPaths.Length; i++)
            {
                ValidateArrayReference(target, propertyName, i, LoadAsset<AudioClip>(expectedClipPaths[i]));
            }
        }

        private static void SetMaterialFloatIfPresent(Material material, string propertyName, float value)
        {
            if (material != null && material.HasProperty(propertyName))
            {
                material.SetFloat(propertyName, value);
            }
        }

        private static void SetMaterialColorIfPresent(Material material, string propertyName, Color value)
        {
            if (material != null && material.HasProperty(propertyName))
            {
                material.SetColor(propertyName, value);
            }
        }

        private static Mesh LoadPrimitiveMesh(PrimitiveType primitiveType)
        {
            string meshPath = ActionFoundationPrimitiveMeshRoot + "/DB_Primitive_" + primitiveType + ".asset";
            EnsureFolderForAsset(meshPath);
            Mesh promotedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
            if (promotedMesh != null)
            {
                return promotedMesh;
            }

            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            try
            {
                Mesh sourceMesh = primitive.GetComponent<MeshFilter>().sharedMesh;
                promotedMesh = UnityEngine.Object.Instantiate(sourceMesh);
                promotedMesh.name = "DB_Primitive_" + primitiveType;
                AssetDatabase.CreateAsset(promotedMesh, meshPath);
                AssetDatabase.ImportAsset(meshPath, ImportAssetOptions.ForceUpdate);
                return promotedMesh;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(primitive);
            }
        }

        private static Shader ResolveUnlitShader()
        {
            return Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("Standard");
        }

        private static void RemoveReviewAndEnemyRoots(Scene scene)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = roots.Length - 1; i >= 0; i--)
            {
                GameObject root = roots[i];
                if (root == null || !ShouldRemoveRoot(root.name))
                {
                    continue;
                }

                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static bool ShouldRemoveRoot(string rootName)
        {
            return rootName.StartsWith(ReviewRootPrefix, StringComparison.Ordinal)
                || rootName.StartsWith("Enemy_SciFiSoldier_", StringComparison.Ordinal)
                || rootName.StartsWith("EnemyPrefabReview_", StringComparison.Ordinal)
                || rootName.StartsWith("EnemyRoleReview_", StringComparison.Ordinal)
                || rootName.StartsWith("ReadableAttackTelegraph", StringComparison.Ordinal);
        }

        private static GameObject CreateRoot(Scene scene, string rootName)
        {
            GameObject root = new GameObject(rootName);
            SceneManager.MoveGameObjectToScene(root, scene);
            return root;
        }

        private static T RequireObject<T>(Scene scene, string label) where T : Component
        {
            T[] found = CollectComponents<T>(scene);
            if (found.Length == 0)
            {
                throw new InvalidOperationException($"Missing {label} in {scene.path}.");
            }

            return found[0];
        }

        private static CombatVfxCuePlayer ResolveScenePlayerCuePlayer(
            Scene scene,
            CombatVfxCuePlayer preferredCuePlayer)
        {
            PlayerMovementController[] players = CollectComponents<PlayerMovementController>(scene);
            if (preferredCuePlayer != null)
            {
                for (int i = 0; i < players.Length; i++)
                {
                    if (players[i] != null && players[i].GetComponent<CombatVfxCuePlayer>() == preferredCuePlayer)
                    {
                        return preferredCuePlayer;
                    }
                }
            }

            for (int i = 0; i < players.Length; i++)
            {
                if (players[i] == null)
                {
                    continue;
                }

                CombatVfxCuePlayer cuePlayer = players[i].GetComponent<CombatVfxCuePlayer>();
                if (cuePlayer != null)
                {
                    return cuePlayer;
                }
            }

            return null;
        }

        private static Transform ResolveScenePlayerDirectionTarget(Scene scene, CombatVfxCuePlayer cuePlayer)
        {
            if (cuePlayer != null)
            {
                PlayerMovementController player = cuePlayer.GetComponent<PlayerMovementController>();
                if (player != null)
                {
                    return player.transform;
                }
            }

            PlayerMovementController[] players = CollectComponents<PlayerMovementController>(scene);
            return players.Length > 0 && players[0] != null
                ? players[0].transform
                : cuePlayer != null ? cuePlayer.transform : null;
        }

        private static GameObject RequireRoot(Scene scene, string rootName)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i] != null && string.Equals(roots[i].name, rootName, StringComparison.Ordinal))
                {
                    return roots[i];
                }
            }

            throw new InvalidOperationException($"Missing root {rootName} in {scene.path}.");
        }

        private static GameObject RequireSceneObject(Scene scene, string objectName)
        {
            GameObject root = FindRoot(scene, objectName);
            if (root != null)
            {
                return root;
            }

            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                Transform match = FindDescendant(roots[i].transform, objectName);
                if (match != null)
                {
                    return match.gameObject;
                }
            }

            throw new InvalidOperationException($"Missing object {objectName} in {scene.path}.");
        }

        private static GameObject FindRoot(Scene scene, string rootName)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i] != null && string.Equals(roots[i].name, rootName, StringComparison.Ordinal))
                {
                    return roots[i];
                }
            }

            return null;
        }

        private static T RequireComponent<T>(GameObject root, string label) where T : Component
        {
            T component = root.GetComponent<T>();
            if (component == null)
            {
                throw new InvalidOperationException($"{label} is missing required component {typeof(T).Name}.");
            }

            return component;
        }

        private static T EnsureComponent<T>(GameObject root) where T : Component
        {
            T component = root.GetComponent<T>();
            return component != null ? component : root.AddComponent<T>();
        }

        private static Transform EnsureChild(Transform parent, string childName)
        {
            Transform child = parent.Find(childName);
            if (child != null)
            {
                return child;
            }

            var childObject = new GameObject(childName);
            childObject.transform.SetParent(parent, worldPositionStays: false);
            return childObject.transform;
        }

        private static Transform RequireChild(Transform parent, string childName)
        {
            Transform child = parent.Find(childName);
            if (child == null)
            {
                throw new InvalidOperationException($"{parent.name} is missing child {childName}.");
            }

            return child;
        }

        private static T[] CollectComponents<T>(Scene scene) where T : Component
        {
            var results = new List<T>();
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                results.AddRange(roots[i].GetComponentsInChildren<T>(includeInactive: true));
            }

            return results.ToArray();
        }

        private static T LoadAsset<T>(string assetPath) where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset == null)
            {
                throw new InvalidOperationException($"Missing required asset at {assetPath}.");
            }

            return asset;
        }

        private static T LoadPrefabComponent<T>(string assetPath) where T : Component
        {
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is T component)
                {
                    return component;
                }
            }

            GameObject prefab = LoadAsset<GameObject>(assetPath);
            T loadedComponent = prefab.GetComponent<T>();
            if (loadedComponent == null)
            {
                throw new InvalidOperationException($"{assetPath} is missing required component {typeof(T).Name}.");
            }

            return loadedComponent;
        }

        private readonly struct PlayerCombatModeVisualBinding
        {
            public PlayerCombatModeVisualBinding(
                GameObject rangedRoot,
                GameObject meleeRoot,
                GameObject rangedWeaponRoot,
                Transform rangedFireOrigin,
                GameObject meleeWeaponRoot,
                RifleGirlNativeGameplayAnimatorBridge nativeAnimatorBridge,
                Animator rangedAnimator,
                Animator meleeAnimator)
            {
                RangedRoot = rangedRoot;
                MeleeRoot = meleeRoot;
                RangedWeaponRoot = rangedWeaponRoot;
                RangedFireOrigin = rangedFireOrigin;
                MeleeWeaponRoot = meleeWeaponRoot;
                NativeAnimatorBridge = nativeAnimatorBridge;
                RangedAnimator = rangedAnimator;
                MeleeAnimator = meleeAnimator;
            }

            public GameObject RangedRoot { get; }
            public GameObject MeleeRoot { get; }
            public GameObject RangedWeaponRoot { get; }
            public Transform RangedFireOrigin { get; }
            public GameObject MeleeWeaponRoot { get; }
            public RifleGirlNativeGameplayAnimatorBridge NativeAnimatorBridge { get; }
            public Animator RangedAnimator { get; }
            public Animator MeleeAnimator { get; }
        }
    }
}
