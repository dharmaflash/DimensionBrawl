using System;
using System.IO;
using DimensionBrawl.AI;
using DimensionBrawl.Combat;
using DimensionBrawl.Enemies;
using DimensionBrawl.Player;
using DimensionBrawl.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace DimensionBrawl.Editor
{
    public static class ActionFoundationCombatVfxSetup
    {
        public const string CombatVfxCueProfilePath = ActionFoundationProfileSetup.ProfileRoot + "/DB_CombatVfxCues_ActionFoundation.asset";

        private const string ScenePath = ActionFoundationCombatAssetPaths.OlympusStationScenePath;
        private const string CombatVfxRoot = "Assets/_Game/Art/VFX/CombatCues";
        private const string MaterialRoot = CombatVfxRoot + "/Materials";
        private const string PrefabRoot = CombatVfxRoot + "/Prefabs";
        private const string TextureRoot = CombatVfxRoot + "/Textures";
        private const string ShaderRoot = CombatVfxRoot + "/Shaders";
        private const string MeshRoot = CombatVfxRoot + "/Meshes";
        private const string PerfectDodgeWorldFxShaderPath = ShaderRoot + "/DB_PerfectDodgeWorldFx.shader";
        private const string PerfectDodgeAfterimageShaderPath = ShaderRoot + "/DB_PerfectDodgeAfterimage.shader";
        private const string CombatCueAudioRoot = "Assets/_Game/Art/Audio/SFX/CombatCues";
        private const string ImportedGunshotAudioRoot =
            "Assets/_Imported/AssetStore/Gun Sounds Pack Vol 1/Gun Shot";
        private const string ImportedActionRpgSfxCombatRoot =
            "Assets/_Imported/AssetStore/Action RPG SFX V2/Combat";
        private const string ImportedActionRpgSfxDesignedSkillRoot =
            "Assets/_Imported/AssetStore/Action RPG SFX V2/Designed Skill";
        private const string ImportedActionRpgSfxMovementRoot =
            "Assets/_Imported/AssetStore/Action RPG SFX V2/Movement";
        private const string ImportedRpgMagicDarkMagicRoot =
            "Assets/_Imported/AssetStore/RPG Magic Sound Effects Pack 3 [ELEMENTAL]/Dark Magic";
        private const string ImportedMuzzleFlashRoot =
            "Assets/_Imported/AssetStore/VFX/Vefects_ShotsVFXURP/Shots VFX URP/Shots/Muzzle Flash/Textures";
        private const string ImportedMuzzleFlashShaderRoot =
            "Assets/_Imported/AssetStore/VFX/Vefects_ShotsVFXURP/Shots VFX URP/Shots/Muzzle Flash/Shaders";
        private const string ImportedRifleMuzzleFlashPrefabPath =
            "Assets/_Imported/AssetStore/VFX/Vefects_ShotsVFXURP/Shots VFX URP/Shots/Muzzle Flash/Once/VFX_Muzzle_Flash_Rifle.prefab";
        private const string ImportedSharedTextureRoot =
            "Assets/_Imported/AssetStore/VFX/Vefects_ShotsVFXURP/Shots VFX URP/Shared/Textures";
        private const string MuzzleFlashFrontSourcePath = ImportedMuzzleFlashRoot + "/T_VFX_MuzzleFlash_Front.tga";
        private const string MuzzleFlashSideSourcePath = ImportedMuzzleFlashRoot + "/T_VFX_MuzzleFlash_Side.tga";
        private const string MuzzleSmokeSourcePath = ImportedSharedTextureRoot + "/T_VFX_SmokePuff_Animated_NonDir01_Optimized.tga";
        private const string MuzzleFlashShaderSourcePath = ImportedMuzzleFlashShaderRoot + "/SH_Vefects_URP_VFX_Muzzle_Flash.shader";
        private const string MuzzleSmokeShaderSourcePath = ImportedMuzzleFlashShaderRoot + "/SH_Vefects_URP_VFX_Muzzle_Smoke.shader";
        private const string MuzzleFlashFrontTexturePath = TextureRoot + "/T_VFX_MuzzleFlash_Front.tga";
        private const string MuzzleFlashSideTexturePath = TextureRoot + "/T_VFX_MuzzleFlash_Side.tga";
        private const string MuzzleSmokeTexturePath = TextureRoot + "/T_VFX_SmokePuff_Animated_NonDir01_Optimized.tga";
        private const string MuzzleFlashShaderPath = ShaderRoot + "/SH_Vefects_URP_VFX_Muzzle_Flash.shader";
        private const string MuzzleSmokeShaderPath = ShaderRoot + "/SH_Vefects_URP_VFX_Muzzle_Smoke.shader";
        private const string ImportedVefectsHitOnceRoot =
            "Assets/_Imported/AssetStore/VFX/Vefects/Combat Flipbook VFX/VFX/Hit/Particles/Once";
        private const string ImportedVefectsHit05DirectionalPrefabPath =
            ImportedVefectsHitOnceRoot + "/VFX_Hit_05_Directional_Bunch_01.prefab";
        private const string ImportedVefectsHit06DirectionalPrefabPath =
            ImportedVefectsHitOnceRoot + "/VFX_Hit_06_Directional_Bunch_01.prefab";
        private const string ShapesFxRoot = "Assets/_Game/Art/VFX/ShapesFXArena";
        private const string ShapesFxGeometryRoot = ShapesFxRoot + "/Geometry";
        private const string ShapesFxMaterialRoot = ShapesFxRoot + "/Materials/ShapesFX";
        private const string ShapesFxTextureRoot = ShapesFxRoot + "/Textures";
        private const string ImportedShapesFxTextureRoot = "Assets/_Imported/AssetStore/VFX/ShapesFX_Pack";
        private const string ShapesFxTorusMeshPath = ShapesFxGeometryRoot + "/Geo_Torus_Hex_Hi.fbx";
        private const string ShapesFxDodecaMeshPath = ShapesFxGeometryRoot + "/Geo_Dodecahedron.fbx";
        private const string ShapesFxIcosaMeshPath = ShapesFxGeometryRoot + "/Geo_Icosahedron_Hex.fbx";
        private const string ShapesFxPortalOuterMaterialPath = ShapesFxMaterialRoot + "/DB_ShapesFX_PortalOuter.mat";
        private const string ShapesFxPortalInnerMaterialPath = ShapesFxMaterialRoot + "/DB_ShapesFX_PortalInner.mat";
        private const string ShapesFxCyanDodecaMaterialPath = ShapesFxMaterialRoot + "/DB_ShapesFX_CyanDodeca.mat";
        private const string ShapesFxVioletIcosaMaterialPath = ShapesFxMaterialRoot + "/DB_ShapesFX_VioletIcosa.mat";
        private const string ImportedHovlSciFiEffectsPrefabRoot =
            "Assets/_Imported/AssetStore/VFX/Hovl Studio/Sci-fi effects 2/Prefabs";
        private const string ImportedHovlHexShieldPrefabPath =
            ImportedHovlSciFiEffectsPrefabRoot + "/Hex shield.prefab";
        private const string ImportedHovlShieldBlockImpactPrefabPath =
            ImportedHovlSciFiEffectsPrefabRoot + "/Hit sci-fi.prefab";
        private const string ImportedHovlSciFiBuffPrefabPath =
            ImportedHovlSciFiEffectsPrefabRoot + "/Sci-fi buff.prefab";
        private const string ImportedHovlSciFiBuffSpherePrefabPath =
            ImportedHovlSciFiEffectsPrefabRoot + "/Sci-fi buff sphere.prefab";
        private const string ImportedHovlLaserTeleportPrefabPath =
            ImportedHovlSciFiEffectsPrefabRoot + "/Laser teleport.prefab";
        private const string HovlSciFiEffectsRoot = "Assets/_Game/Art/VFX/HovlSciFiEffects";
        private const string HovlSciFiEffectsMaterialRoot = HovlSciFiEffectsRoot + "/Materials";
        private const string HovlSciFiEffectsTextureRoot = HovlSciFiEffectsRoot + "/Textures";
        private const string HovlSciFiEffectsShaderRoot = HovlSciFiEffectsRoot + "/Shaders";
        private const string HovlSciFiEffectsMeshRoot = HovlSciFiEffectsRoot + "/Meshes";
        private const string PerfectDodgeHovlHexShieldChildName = "PerfectDodgeVfx_HovlHexShield";
        private const string PerfectDodgeHovlHexShieldMaterialName = "HexShield3shield";
        private const float PerfectDodgeHovlHexShieldOpacity = 0.42f;
        private static readonly string[] PlayerRangedGunshotClipPaths =
        {
            "Assets/_Game/Art/Audio/SFX/Guns/DB_SFX_PlayerRanged_Gunshot_01.wav",
            "Assets/_Game/Art/Audio/SFX/Guns/DB_SFX_PlayerRanged_Gunshot_02.wav",
            "Assets/_Game/Art/Audio/SFX/Guns/DB_SFX_PlayerRanged_Gunshot_03.wav",
            "Assets/_Game/Art/Audio/SFX/Guns/DB_SFX_PlayerRanged_Gunshot_04.wav",
            "Assets/_Game/Art/Audio/SFX/Guns/DB_SFX_PlayerRanged_Gunshot_05.wav",
        };
        private static readonly string[] PlayerRangedGunshotSourceClipPaths =
        {
            ImportedGunshotAudioRoot + "/Gunshot 1-1.wav",
            ImportedGunshotAudioRoot + "/Gunshot 1-2.wav",
            ImportedGunshotAudioRoot + "/Gunshot 1-3.wav",
            ImportedGunshotAudioRoot + "/Gunshot 1-4.wav",
            ImportedGunshotAudioRoot + "/Gunshot 1-5.wav",
        };

        private const string PlayerRangedGunshotAudioName = "ReviewedGunshot_PlayerRangedBasic";
        private const float PlayerRangedGunshotBaseVolume = 0.78f;
        private const float PlayerRangedGunshotMinimumPitch = 1.02f;
        private const float PlayerRangedGunshotMaximumPitch = 1.08f;
        private const float PlayerRangedGunshotMinimumVolumeMultiplier = 0.98f;
        private const float PlayerRangedGunshotMaximumVolumeMultiplier = 1.06f;
        private const string PlayerRangedProjectileImpactAudioName = "ReviewedSfx_PlayerRangedProjectileImpact";
        private const string PlayerDodgeStartAudioName = "ReviewedSfx_PlayerDodgeStart";
        private const string PlayerPerfectDodgeTimeWarpAudioName = "ReviewedSfx_PlayerPerfectDodgeTimeWarp";
        private const string PlayerPerfectDodgeSuccessAudioName = "ReviewedSfx_PlayerPerfectDodgeSuccess";
        private const string MissileShieldAudioRoot = "Assets/_Game/Art/Audio/SFX/MissileShield";
        private const string EliteSummonSignalAudioName = "ReviewedSfx_EliteSummonSignal";
        private const string SummonBlockOpportunityAudioName = "ReviewedSfx_SummonBlockOpportunity";
        private const string SummonFollowupWindowAudioName = "ReviewedSfx_SummonFollowupWindow";
        private const string PlayerSummonPreSpawnPortalAudioName = "ReviewedSfx_PlayerSummonPreSpawnPortal";
        private const string PlayerSummonLandingCraterAudioName = "ReviewedSfx_PlayerSummonLandingCrater";
        private const string PlayerSummonDragonBreathAudioName = "ReviewedSfx_PlayerSummonDragonBreath";
        private static readonly string[] PlayerDodgeStartSourceClipPaths =
        {
            ImportedActionRpgSfxMovementRoot + "/Whoosh_Dodge_Cloth_Movement_Light_01_01.wav",
            ImportedActionRpgSfxMovementRoot + "/Whoosh_Dodge_Cloth_Movement_Light_01_02.wav",
            ImportedActionRpgSfxMovementRoot + "/Whoosh_Dodge_Cloth_Movement_Light_01_03.wav"
        };

        private static readonly string[] PlayerDodgeStartClipPaths =
        {
            CombatCueAudioRoot + "/DB_SFX_PlayerDodgeStart_01.wav",
            CombatCueAudioRoot + "/DB_SFX_PlayerDodgeStart_02.wav",
            CombatCueAudioRoot + "/DB_SFX_PlayerDodgeStart_03.wav"
        };

        private static readonly string[] PlayerPerfectDodgeTimeWarpSourceClipPaths =
        {
            ImportedRpgMagicDarkMagicRoot + "/RPG3_DarkMagicEpic_SlowMo_Cast01.wav",
            ImportedRpgMagicDarkMagicRoot + "/RPG3_DarkMagicEpic_SlowMo_Cast02.wav",
            ImportedRpgMagicDarkMagicRoot + "/RPG3_DarkMagicEpic_SlowMo_Cast03.wav"
        };

        private static readonly string[] PlayerPerfectDodgeTimeWarpClipPaths =
        {
            CombatCueAudioRoot + "/DB_SFX_PlayerPerfectDodgeTimeWarp_01.wav",
            CombatCueAudioRoot + "/DB_SFX_PlayerPerfectDodgeTimeWarp_02.wav",
            CombatCueAudioRoot + "/DB_SFX_PlayerPerfectDodgeTimeWarp_03.wav"
        };

        private static readonly string[] PlayerPerfectDodgeSuccessSourceClipPaths =
        {
            ImportedActionRpgSfxCombatRoot + "/Ranged_Magic_Swing_Shimmer_Ring_01_01.wav",
            ImportedActionRpgSfxCombatRoot + "/Ranged_Magic_Swing_Shimmer_Ring_01_02.wav",
            ImportedActionRpgSfxCombatRoot + "/Ranged_Magic_Swing_Shimmer_Ring_01_03.wav"
        };

        private static readonly string[] PlayerPerfectDodgeSuccessClipPaths =
        {
            CombatCueAudioRoot + "/DB_SFX_PlayerPerfectDodgeSuccess_01.wav",
            CombatCueAudioRoot + "/DB_SFX_PlayerPerfectDodgeSuccess_02.wav",
            CombatCueAudioRoot + "/DB_SFX_PlayerPerfectDodgeSuccess_03.wav"
        };

        private static readonly string[] PlayerPerfectDodgeShieldActivateClipPaths =
        {
            MissileShieldAudioRoot + "/DB_SFX_Shield_Activate_01.mp3"
        };

        private static readonly string[] PlayerPerfectDodgeShieldBlockClipPaths =
        {
            MissileShieldAudioRoot + "/DB_SFX_Shield_Block_Projectile_01.mp3",
            MissileShieldAudioRoot + "/DB_SFX_Missile_Impact_Shield_01.mp3"
        };

        private static readonly string[] EliteSummonSignalSourceClipPaths =
        {
            ImportedActionRpgSfxDesignedSkillRoot + "/Designed_Skill_Magic_Shield_Rise_Reverse_Thud_Magic_Spell_End_01_01.wav",
            ImportedActionRpgSfxDesignedSkillRoot + "/Designed_Skill_Magic_Shield_Rise_Reverse_Thud_Magic_Spell_End_01_02.wav",
            ImportedActionRpgSfxDesignedSkillRoot + "/Designed_Skill_Magic_Shield_Rise_Reverse_Thud_Magic_Spell_End_01_03.wav"
        };

        private static readonly string[] EliteSummonSignalClipPaths =
        {
            CombatCueAudioRoot + "/DB_SFX_EliteSummonSignal_01.wav",
            CombatCueAudioRoot + "/DB_SFX_EliteSummonSignal_02.wav",
            CombatCueAudioRoot + "/DB_SFX_EliteSummonSignal_03.wav"
        };

        private static readonly string[] SummonBlockOpportunitySourceClipPaths =
        {
            ImportedActionRpgSfxDesignedSkillRoot + "/Designed_Skill_Magic_Shield_Deep_Impact_Skill_Cast_01_01.wav",
            ImportedActionRpgSfxDesignedSkillRoot + "/Designed_Skill_Magic_Shield_Deep_Impact_Skill_Cast_01_02.wav",
            ImportedActionRpgSfxDesignedSkillRoot + "/Designed_Skill_Magic_Shield_Deep_Impact_Skill_Cast_01_03.wav"
        };

        private static readonly string[] SummonBlockOpportunityClipPaths =
        {
            CombatCueAudioRoot + "/DB_SFX_SummonBlockOpportunity_01.wav",
            CombatCueAudioRoot + "/DB_SFX_SummonBlockOpportunity_02.wav",
            CombatCueAudioRoot + "/DB_SFX_SummonBlockOpportunity_03.wav"
        };

        private static readonly string[] SummonFollowupWindowSourceClipPaths =
        {
            ImportedActionRpgSfxCombatRoot + "/Ranged_Magic_Swing_Shimmer_Ring_01_01.wav",
            ImportedActionRpgSfxCombatRoot + "/Ranged_Magic_Swing_Shimmer_Ring_01_02.wav",
            ImportedActionRpgSfxCombatRoot + "/Ranged_Magic_Swing_Shimmer_Ring_01_03.wav"
        };

        private static readonly string[] SummonFollowupWindowClipPaths =
        {
            CombatCueAudioRoot + "/DB_SFX_SummonFollowupWindow_01.wav",
            CombatCueAudioRoot + "/DB_SFX_SummonFollowupWindow_02.wav",
            CombatCueAudioRoot + "/DB_SFX_SummonFollowupWindow_03.wav"
        };
        private static readonly string[] PlayerSummonPreSpawnPortalClipPaths =
        {
            "Assets/_Game/Art/Audio/SFX/Reviewed/DB_SFX_EnemySummon_Spawn_01.mp3"
        };
        private static readonly string[] PlayerSummonLandingCraterClipPaths =
        {
            "Assets/_Game/Art/Audio/SFX/Reviewed/DB_SFX_Summon_Land_Heavy_01.mp3",
            "Assets/_Game/Art/Audio/SFX/Reviewed/DB_SFX_Summon_Land_Heavy_02.mp3"
        };
        private static readonly string[] PlayerSummonDragonBreathClipPaths =
        {
            "Assets/_Game/Art/Audio/SFX/Reviewed/DB_SFX_Dragon_Breath_01.mp3"
        };
        private const string PoolRootName = "ActionFoundation_CombatVfxPool";

        public static string[] GetPlayerRangedGunshotClipPaths()
        {
            return (string[])PlayerRangedGunshotClipPaths.Clone();
        }

        public static string[] GetPlayerDodgeStartClipPaths()
        {
            return (string[])PlayerDodgeStartClipPaths.Clone();
        }

        public static string[] GetPlayerPerfectDodgeTimeWarpClipPaths()
        {
            return (string[])PlayerPerfectDodgeTimeWarpClipPaths.Clone();
        }

        public static string[] GetPlayerPerfectDodgeSuccessClipPaths()
        {
            return (string[])PlayerPerfectDodgeSuccessClipPaths.Clone();
        }

        public static string[] GetEliteSummonSignalClipPaths()
        {
            return (string[])EliteSummonSignalClipPaths.Clone();
        }

        public static string[] GetSummonBlockOpportunityClipPaths()
        {
            return (string[])SummonBlockOpportunityClipPaths.Clone();
        }

        public static string[] GetSummonFollowupWindowClipPaths()
        {
            return (string[])SummonFollowupWindowClipPaths.Clone();
        }

        [MenuItem("DimensionBrawl/Reapply Action Foundation Combat VFX Cues")]
        public static void ReapplyCombatVfxCuesMenu()
        {
            CombatVfxCueProfile profile = EnsureCombatVfxAssets();
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != ScenePath)
            {
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }

            ConfigureSceneCombatVfx(scene, profile);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Reapplied ActionFoundation combat VFX cue assets and scene bindings.");
        }

        [MenuItem("DimensionBrawl/Refresh Player Ranged Gunshot Audio Bank")]
        public static void RefreshPlayerRangedGunshotAudioBankMenu()
        {
            const string prefabPath = PrefabRoot + "/DB_VFX_PlayerRangedMuzzleFlash.prefab";
            PromoteReviewedAudioClips(PlayerRangedGunshotSourceClipPaths, PlayerRangedGunshotClipPaths);
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                AttachReviewedGunshotAudio(prefabRoot);
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }

            AssetDatabase.SaveAssets();
            Debug.Log("Refreshed player ranged gunshot audio bank on the promoted muzzle flash prefab.");
        }

        [MenuItem("DimensionBrawl/Refresh Reviewed Combat Cue Audio Banks")]
        public static void RefreshReviewedCombatCueAudioBanksMenu()
        {
            EnsureReviewedCombatCueAudioBanks();
            ActionFoundationHitFeedbackSfxSetup.ApplyMasterHitFeedbackCueAudioToProfile();
            CombatVfxCueProfile profile = AssetDatabase.LoadAssetAtPath<CombatVfxCueProfile>(CombatVfxCueProfilePath);
            if (profile != null)
            {
                ApplyMissileShieldCueProfileAudio(profile);
            }

            AssetDatabase.SaveAssets();
            Debug.Log("Refreshed reviewed combat cue audio banks on promoted cue prefabs.");
        }

        public static void RefreshSummonProjectileInterceptAudioBatch()
        {
            PromoteReviewedAudioClips(
                SummonBlockOpportunitySourceClipPaths,
                SummonBlockOpportunityClipPaths);
            ConfigureSummonProjectileInterceptAudioBank();
            AssetDatabase.SaveAssets();
            Debug.Log("Refreshed the reviewed summon projectile-intercept audio bank.");
        }

        [MenuItem("DimensionBrawl/Refresh Reviewed Hit Feedback VFX")]
        public static void RefreshReviewedHitFeedbackVfxMenu()
        {
            EnsureFolder("Assets/_Game/Art/VFX");
            EnsureFolder(CombatVfxRoot);
            EnsureFolder(MaterialRoot);
            EnsureFolder(PrefabRoot);
            EnsureFolder(TextureRoot);
            EnsureFolder(ShaderRoot);
            EnsureFolder(MeshRoot);

            SavePromotedHitFeedbackPrefab(
                "DB_VFX_EnemyHit",
                ImportedVefectsHit05DirectionalPrefabPath,
                0.30f);
            SavePromotedHitFeedbackPrefab(
                "DB_VFX_EnemyHit_Alt_Hit06",
                ImportedVefectsHit06DirectionalPrefabPath,
                0.30f);

            AssetDatabase.SaveAssets();
            Debug.Log("Refreshed reviewed hit feedback VFX prefabs.");
        }

        [MenuItem("DimensionBrawl/Validate Action Foundation Combat VFX Cues")]
        public static void ValidateCombatVfxCuesMenu()
        {
            CombatVfxCueProfile profile = LoadCombatVfxCueProfileForValidation();
            ValidateCombatVfxAssetProfile(profile);

            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != ScenePath)
            {
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }

            GameObject[] roots = scene.GetRootGameObjects();
            PlayerActionController player = RequireObject<PlayerActionController>(roots, "player action controller");
            ValidateCuePlayer(player.gameObject, profile, "player");
            if (player.GetComponent<PlayerCombatVfxCueDriver>() == null)
            {
                throw new InvalidOperationException("Player root is missing PlayerCombatVfxCueDriver.");
            }

            BasicSoldierEnemy[] soldiers = CollectSoldiers(roots);
            if (soldiers.Length == 0)
            {
                throw new InvalidOperationException("The canonical combat review has no BasicSoldierEnemy samples to validate.");
            }

            for (int i = 0; i < soldiers.Length; i++)
            {
                ValidateEnemyCombatVfx(soldiers[i], profile);
            }

            ValidateProjectileCueVisual(profile, CombatVfxCueId.EnemyLinePressureActive, 3f);
            ValidateProjectileCueVisual(profile, CombatVfxCueId.EnemyRetreatShotActive, 2.8f);

            Debug.Log("Action foundation combat VFX cue validation passed.");
        }

        [MenuItem("DimensionBrawl/Validate Action Foundation Combat VFX Assets")]
        public static void ValidateCombatVfxAssetsBatch()
        {
            CombatVfxCueProfile profile = LoadCombatVfxCueProfileForValidation();
            ValidateCombatVfxAssetProfile(profile);
            Debug.Log("Action foundation combat VFX asset validation passed.");
        }

        private static CombatVfxCueProfile LoadCombatVfxCueProfileForValidation()
        {
            CombatVfxCueProfile profile = AssetDatabase.LoadAssetAtPath<CombatVfxCueProfile>(CombatVfxCueProfilePath);
            if (profile == null)
            {
                throw new InvalidOperationException($"Missing combat VFX cue profile at {CombatVfxCueProfilePath}.");
            }

            return profile;
        }

        private static void ValidateCombatVfxAssetProfile(CombatVfxCueProfile profile)
        {
            foreach (CombatVfxCueId cueId in Enum.GetValues(typeof(CombatVfxCueId)))
            {
                if (!profile.TryGetCue(cueId, out CombatVfxCue cue))
                {
                    throw new InvalidOperationException($"{CombatVfxCueProfilePath} is missing cue {cueId}.");
                }

                string prefabPath = AssetDatabase.GetAssetPath(cue.Prefab).Replace('\\', '/');
                if (!prefabPath.StartsWith(PrefabRoot + "/", StringComparison.Ordinal))
                {
                    throw new InvalidOperationException($"{cueId} should reference a promoted combat VFX prefab, found {prefabPath}.");
                }

                if (prefabPath.Contains("/_Imported/", StringComparison.Ordinal))
                {
                    throw new InvalidOperationException($"{cueId} should not reference raw imported VFX assets.");
                }

                if (cue.Prefab.GetComponentInChildren<CombatVfxCueVisual>(includeInactive: true) == null)
                {
                    throw new InvalidOperationException($"{cueId} should use a stable promoted CombatVfxCueVisual prefab, found {prefabPath}.");
                }

                ValidateNoImportedAssetDependencies(cueId, prefabPath);

                if (cueId == CombatVfxCueId.PlayerPerfectDodgeWindow)
                {
                    ValidatePerfectDodgeHovlShieldPrefab(cue.Prefab);
                }
            }

            ValidateReviewedCombatFeedbackPlayback(profile);
        }

        public static CombatVfxCueProfile EnsureCombatVfxAssets()
        {
            EnsureFolder("Assets/_Game/Art/VFX");
            EnsureFolder(CombatVfxRoot);
            EnsureFolder(MaterialRoot);
            EnsureFolder(PrefabRoot);
            EnsureFolder(TextureRoot);
            EnsureFolder(ShaderRoot);
            EnsureFolder(MeshRoot);
            EnsureFolder(HovlSciFiEffectsRoot);
            EnsureFolder(HovlSciFiEffectsMaterialRoot);
            EnsureFolder(HovlSciFiEffectsTextureRoot);
            EnsureFolder(HovlSciFiEffectsShaderRoot);
            EnsureFolder(HovlSciFiEffectsMeshRoot);
            EnsureFolder("Assets/_Game/Art/Audio");
            EnsureFolder("Assets/_Game/Art/Audio/SFX");
            EnsureFolder("Assets/_Game/Art/Audio/SFX/Guns");
            PromoteReviewedAudioClips(PlayerRangedGunshotSourceClipPaths, PlayerRangedGunshotClipPaths);

            Shader muzzleFlashShader = EnsurePromotedShader(MuzzleFlashShaderSourcePath, MuzzleFlashShaderPath);
            Shader muzzleSmokeShader = EnsurePromotedShader(MuzzleSmokeShaderSourcePath, MuzzleSmokeShaderPath);
            Texture2D muzzleFront = EnsurePromotedTexture(MuzzleFlashFrontSourcePath, MuzzleFlashFrontTexturePath);
            Texture2D muzzleSide = EnsurePromotedTexture(MuzzleFlashSideSourcePath, MuzzleFlashSideTexturePath);
            Texture2D muzzleSmoke = EnsurePromotedTexture(MuzzleSmokeSourcePath, MuzzleSmokeTexturePath);

            Material cyan = LoadOrCreateParticleMaterial("DB_CombatVfx_Cyan", new Color(0.22f, 0.88f, 1f, 0.82f));
            Material blue = LoadOrCreateParticleMaterial("DB_CombatVfx_Blue", new Color(0.18f, 0.45f, 1f, 0.82f));
            Material orange = LoadOrCreateParticleMaterial("DB_CombatVfx_Orange", new Color(1f, 0.42f, 0.08f, 0.86f));
            Material red = LoadOrCreateParticleMaterial("DB_CombatVfx_Red", new Color(1f, 0.12f, 0.06f, 0.86f));
            Material violet = LoadOrCreateParticleMaterial("DB_CombatVfx_Violet", new Color(0.62f, 0.28f, 1f, 0.82f));
            Material gold = LoadOrCreateParticleMaterial("DB_CombatVfx_Gold", new Color(1f, 0.78f, 0.18f, 0.88f));
            Material white = LoadOrCreateParticleMaterial("DB_CombatVfx_White", new Color(0.92f, 0.98f, 1f, 0.9f));
            Material smoke = LoadOrCreateVefectsSmokeMaterial(
                "DB_CombatVfx_Smoke",
                muzzleSmokeShader,
                muzzleSmoke,
                new Color(0.45f, 0.52f, 0.58f, 0.55f));
            Material muzzleFrontMaterial = LoadOrCreateVefectsMuzzleFlashMaterial(
                "DB_CombatVfx_MuzzleFlashFront",
                muzzleFlashShader,
                muzzleFront,
                new Color(1f, 0.86f, 0.42f, 0.96f),
                (float)BlendMode.One,
                (float)BlendMode.One,
                0.03f,
                1f);
            Material muzzleSideMaterial = LoadOrCreateVefectsMuzzleFlashMaterial(
                "DB_CombatVfx_MuzzleFlashSide",
                muzzleFlashShader,
                muzzleSide,
                new Color(1f, 0.56f, 0.14f, 0.92f),
                (float)BlendMode.SrcAlpha,
                (float)BlendMode.OneMinusSrcAlpha,
                0.02f,
                0f);
            Material shapesPortalOuter = LoadReviewedShapesFxMaterial(ShapesFxPortalOuterMaterialPath);
            Material shapesPortalInner = LoadReviewedShapesFxMaterial(ShapesFxPortalInnerMaterialPath);
            Material shapesCyanDodeca = LoadReviewedShapesFxMaterial(ShapesFxCyanDodecaMaterialPath);
            Material shapesVioletIcosa = LoadReviewedShapesFxMaterial(ShapesFxVioletIcosaMaterialPath);
            Mesh shapesTorus = LoadRequiredMesh(ShapesFxTorusMeshPath);
            Mesh shapesDodeca = LoadRequiredMesh(ShapesFxDodecaMeshPath);
            Mesh shapesIcosa = LoadRequiredMesh(ShapesFxIcosaMeshPath);
            GameObject enemyHitFeedback = SavePromotedHitFeedbackPrefab(
                "DB_VFX_EnemyHit",
                ImportedVefectsHit05DirectionalPrefabPath,
                0.30f);
            SavePromotedHitFeedbackPrefab(
                "DB_VFX_EnemyHit_Alt_Hit06",
                ImportedVefectsHit06DirectionalPrefabPath,
                0.30f);

            CombatCuePrefabs prefabs = new CombatCuePrefabs
            {
                PlayerAttackStart = SaveBurstPrefab("DB_VFX_PlayerAttackStart", cyan, ParticleSystemShapeType.Cone, 0.16f, 24f, 115f, 0.16f, 0.36f, 0.12f, 0.32f, 18, new Color(0.26f, 0.95f, 1f, 0.86f), new Color(0.14f, 0.42f, 1f, 0f)),
                PlayerAttackHit = SaveBurstPrefab("DB_VFX_PlayerAttackHit", white, ParticleSystemShapeType.Sphere, 0.32f, 35f, 360f, 0.12f, 0.28f, 0.16f, 0.42f, 34, new Color(1f, 0.95f, 0.75f, 0.95f), new Color(0.18f, 0.74f, 1f, 0f)),
                PlayerDodgeStart = SaveBurstPrefab("DB_VFX_PlayerDodgeStart", blue, ParticleSystemShapeType.Cone, 0.22f, 18f, 75f, 0.18f, 0.40f, 0.18f, 0.46f, 28, new Color(0.18f, 0.58f, 1f, 0.75f), new Color(0.1f, 0.2f, 0.7f, 0f)),
                PlayerPerfectDodgeTimeField = SavePromotedHovlSciFiCuePrefab("DB_VFX_PlayerPerfectDodgeTimeField", ImportedHovlSciFiBuffPrefabPath, Vector3.zero, Vector3.zero, Vector3.one, loopParticles: true, playOnAwake: true, lifetimeSeconds: 3f, minimumParticleSystems: 8),
                PlayerPerfectDodgePulsewave = SavePromotedHovlSciFiCuePrefab("DB_VFX_PlayerPerfectDodgePulsewave", ImportedHovlSciFiBuffSpherePrefabPath, Vector3.zero, Vector3.zero, Vector3.one, loopParticles: false, playOnAwake: true, lifetimeSeconds: 0.9f, minimumParticleSystems: 4),
                PlayerPerfectDodgeHoloCube = SavePromotedHovlSciFiCuePrefab("DB_VFX_PlayerPerfectDodgeHoloCube", ImportedHovlSciFiBuffPrefabPath, Vector3.zero, Vector3.zero, new Vector3(0.82f, 0.82f, 0.82f), loopParticles: true, playOnAwake: true, lifetimeSeconds: 1.2f, minimumParticleSystems: 8),
                PlayerPerfectDodgeWindow = SavePerfectDodgeWindowPrefab("DB_VFX_PlayerPerfectDodgeWindow", cyan, ParticleSystemShapeType.Circle, 1.15f, 22f, 360f, 0.86f, 1.15f, 0.18f, 0.42f, 96, new Color(0.42f, 0.98f, 1f, 0.62f), new Color(0.05f, 0.18f, 0.9f, 0f)),
                PlayerPerfectDodgeShieldBlockImpact = SavePromotedHovlSciFiCuePrefab("DB_VFX_PlayerPerfectDodgeShieldBlockImpact", ImportedHovlShieldBlockImpactPrefabPath, Vector3.zero, Vector3.zero, Vector3.one, loopParticles: false, playOnAwake: true, lifetimeSeconds: 1f, minimumParticleSystems: 9),
                PlayerSummonPreSpawnPortal = SaveSummonPreSpawnPortalPrefab("DB_VFX_PlayerSummonPreSpawnPortal", cyan, blue, white, shapesPortalOuter, shapesPortalInner, shapesTorus),
                PlayerSummonLandingCrater = SaveSummonLandingCraterPrefab("DB_VFX_PlayerSummonLandingCrater", gold, smoke, shapesPortalOuter, shapesTorus),
                PlayerSummonDragonBreathAudio = SaveAudioOnlyCuePrefab("DB_VFX_PlayerSummonDragonBreathAudio"),
                PlayerRangedMuzzleFlash = SaveMuzzleFlashPrefab("DB_VFX_PlayerRangedMuzzleFlash", muzzleFrontMaterial, muzzleSideMaterial, smoke),
                PlayerRangedProjectileImpact = SavePromotedHitFeedbackPrefab(
                    "DB_VFX_PlayerRangedProjectileImpact",
                    ImportedVefectsHit06DirectionalPrefabPath,
                    0.38f),
                EnemyWindup = SaveBurstPrefab("DB_VFX_EnemyWindup_Generic", orange, ParticleSystemShapeType.Cone, 0.28f, 9f, 150f, 0.28f, 0.54f, 0.10f, 0.30f, 24, new Color(1f, 0.44f, 0.08f, 0.78f), new Color(1f, 0.12f, 0f, 0f)),
                EnemyAttackActive = SaveBurstPrefab("DB_VFX_EnemyAttackActive_Generic", white, ParticleSystemShapeType.Cone, 0.36f, 42f, 120f, 0.10f, 0.24f, 0.22f, 0.55f, 36, new Color(1f, 0.9f, 0.55f, 0.95f), new Color(1f, 0.2f, 0.02f, 0f)),
                EnemyHit = enemyHitFeedback,
                EnemyDeath = SaveBurstPrefab("DB_VFX_EnemyDeath", smoke, ParticleSystemShapeType.Sphere, 0.42f, 16f, 360f, 0.30f, 0.72f, 0.20f, 0.68f, 42, new Color(0.58f, 0.66f, 0.72f, 0.58f), new Color(0.08f, 0.12f, 0.16f, 0f)),
                ClosePunishWindup = SaveBurstPrefab("DB_VFX_ClosePunishWindup", orange, ParticleSystemShapeType.Cone, 0.22f, 12f, 105f, 0.26f, 0.50f, 0.12f, 0.34f, 24, new Color(1f, 0.42f, 0.08f, 0.8f), new Color(1f, 0.06f, 0f, 0f)),
                ClosePunishActive = SaveBurstPrefab("DB_VFX_ClosePunishActive", red, ParticleSystemShapeType.Cone, 0.34f, 42f, 95f, 0.09f, 0.22f, 0.20f, 0.52f, 38, new Color(1f, 0.24f, 0.05f, 0.92f), new Color(1f, 0.8f, 0.16f, 0f)),
                LungeWindup = SaveBurstPrefab("DB_VFX_LungeStrikeWindup", orange, ParticleSystemShapeType.Cone, 0.10f, 7f, 22f, 0.14f, 0.26f, 0.05f, 0.14f, 8, new Color(1f, 0.44f, 0.12f, 0.36f), new Color(1f, 0.18f, 0.02f, 0f)),
                LungeActive = SaveBurstPrefab("DB_VFX_LungeStrikeActive", red, ParticleSystemShapeType.Cone, 0.08f, 48f, 8f, 0.06f, 0.14f, 0.05f, 0.16f, 10, new Color(1f, 0.72f, 0.36f, 0.58f), new Color(1f, 0.1f, 0.02f, 0f), 1.35f),
                HeavyWindup = SaveBurstPrefab("DB_VFX_HeavyWindupCharge", gold, ParticleSystemShapeType.Sphere, 0.34f, 10f, 360f, 0.38f, 0.70f, 0.18f, 0.44f, 42, new Color(1f, 0.74f, 0.12f, 0.86f), new Color(1f, 0.18f, 0.02f, 0f)),
                HeavyActive = SaveBurstPrefab("DB_VFX_HeavyWindupImpact", gold, ParticleSystemShapeType.Circle, 0.65f, 34f, 360f, 0.13f, 0.34f, 0.30f, 0.78f, 56, new Color(1f, 0.9f, 0.36f, 0.96f), new Color(1f, 0.28f, 0.04f, 0f)),
                LineWindup = SaveBurstPrefab("DB_VFX_LinePressureWindup", cyan, ParticleSystemShapeType.Cone, 0.18f, 18f, 34f, 0.30f, 0.58f, 0.10f, 0.24f, 30, new Color(0.22f, 0.94f, 1f, 0.82f), new Color(0.02f, 0.28f, 1f, 0f)),
                LineActive = SaveBurstPrefab("DB_VFX_LinePressureActive", cyan, ParticleSystemShapeType.Cone, 0.20f, 74f, 24f, 0.12f, 0.36f, 0.16f, 0.46f, 58, new Color(0.42f, 0.96f, 1f, 0.96f), new Color(0.04f, 0.3f, 1f, 0f), 3.4f),
                FanWindup = SaveBurstPrefab("DB_VFX_FanPressureWindup", cyan, ParticleSystemShapeType.Cone, 0.24f, 14f, 95f, 0.30f, 0.56f, 0.12f, 0.32f, 34, new Color(0.18f, 0.94f, 0.88f, 0.8f), new Color(0.02f, 0.52f, 0.9f, 0f)),
                FanActive = SaveBurstPrefab("DB_VFX_FanPressureActive", cyan, ParticleSystemShapeType.Cone, 0.36f, 48f, 120f, 0.13f, 0.34f, 0.20f, 0.54f, 64, new Color(0.36f, 1f, 0.86f, 0.94f), new Color(0.02f, 0.58f, 1f, 0f)),
                RetreatShotWindup = SaveBurstPrefab("DB_VFX_RetreatShotWindup", blue, ParticleSystemShapeType.Cone, 0.20f, 18f, 45f, 0.24f, 0.48f, 0.10f, 0.28f, 22, new Color(0.24f, 0.72f, 1f, 0.84f), new Color(0.06f, 0.28f, 1f, 0f)),
                RetreatShotActive = SaveBurstPrefab("DB_VFX_RetreatShotActive", blue, ParticleSystemShapeType.Cone, 0.18f, 70f, 22f, 0.08f, 0.32f, 0.14f, 0.40f, 44, new Color(0.48f, 0.92f, 1f, 0.96f), new Color(0.05f, 0.2f, 1f, 0f), 3.2f),
                RetreatBlinkWindup = SaveBurstPrefab("DB_VFX_RetreatBlinkWindup", violet, ParticleSystemShapeType.Sphere, 0.36f, 12f, 360f, 0.22f, 0.46f, 0.12f, 0.36f, 32, new Color(0.64f, 0.26f, 1f, 0.82f), new Color(0.1f, 0.03f, 0.52f, 0f)),
                RetreatBlinkActive = SaveBurstPrefab("DB_VFX_RetreatBlinkActive", violet, ParticleSystemShapeType.Circle, 0.52f, 38f, 360f, 0.10f, 0.28f, 0.18f, 0.56f, 50, new Color(0.78f, 0.48f, 1f, 0.95f), new Color(0.18f, 0.06f, 0.8f, 0f)),
                GuardBreakWindup = SaveBurstPrefab("DB_VFX_GuardBreakWindup", gold, ParticleSystemShapeType.Sphere, 0.38f, 14f, 360f, 0.42f, 0.78f, 0.20f, 0.48f, 48, new Color(1f, 0.76f, 0.16f, 0.88f), new Color(1f, 0.22f, 0.02f, 0f)),
                GuardBreakActive = SaveBurstPrefab("DB_VFX_GuardBreakActive", gold, ParticleSystemShapeType.Circle, 0.72f, 42f, 360f, 0.14f, 0.36f, 0.34f, 0.86f, 72, new Color(1f, 0.92f, 0.42f, 0.98f), new Color(1f, 0.3f, 0.04f, 0f)),
                EliteShield = SaveBurstPrefab("DB_VFX_EliteShieldSignal", blue, ParticleSystemShapeType.Circle, 0.64f, 20f, 360f, 0.22f, 0.46f, 0.12f, 0.24f, 48, new Color(0.28f, 0.72f, 1f, 0.64f), new Color(0.04f, 0.16f, 1f, 0f)),
                EliteArmorBreak = SaveBurstPrefab("DB_VFX_EliteArmorBreakSignal", gold, ParticleSystemShapeType.Sphere, 0.48f, 24f, 360f, 0.18f, 0.48f, 0.18f, 0.58f, 42, new Color(1f, 0.86f, 0.18f, 0.9f), new Color(1f, 0.18f, 0.02f, 0f)),
                EliteAura = SaveBurstPrefab("DB_VFX_EliteAuraSignal", cyan, ParticleSystemShapeType.Circle, 0.70f, 18f, 360f, 0.42f, 0.84f, 0.16f, 0.52f, 58, new Color(0.18f, 1f, 0.78f, 0.76f), new Color(0.04f, 0.36f, 0.86f, 0f)),
                EliteSummon = SaveBurstPrefab("DB_VFX_EliteSummonSignal", violet, ParticleSystemShapeType.Sphere, 0.58f, 24f, 360f, 0.24f, 0.48f, 0.14f, 0.26f, 64, new Color(0.74f, 0.38f, 1f, 0.64f), new Color(0.12f, 0.04f, 0.7f, 0f)),
                SummonFollowupWindow = SaveBurstPrefab("DB_VFX_SummonFollowupWindow", cyan, ParticleSystemShapeType.Cone, 0.20f, 24f, 62f, 0.10f, 0.24f, 0.06f, 0.14f, 24, new Color(0.42f, 0.95f, 1f, 0.50f), new Color(0.04f, 0.28f, 0.88f, 0f), 0.34f),
                SummonBlockOpportunity = SaveBurstPrefab("DB_VFX_SummonBlockOpportunity", gold, ParticleSystemShapeType.Cone, 0.22f, 22f, 70f, 0.12f, 0.26f, 0.07f, 0.15f, 28, new Color(1f, 0.82f, 0.28f, 0.52f), new Color(0.86f, 0.24f, 0.02f, 0f), 0.30f),
                ElitePhaseSwap = SaveBurstPrefab("DB_VFX_ElitePhaseSwapSignal", white, ParticleSystemShapeType.Circle, 0.82f, 44f, 360f, 0.18f, 0.48f, 0.26f, 0.82f, 84, new Color(0.9f, 0.98f, 1f, 0.96f), new Color(0.26f, 0.46f, 1f, 0f)),
                SummonProjectileIntercept = SaveBurstPrefab("DB_VFX_SummonProjectileIntercept", cyan, ParticleSystemShapeType.Circle, 0.54f, 58f, 360f, 0.08f, 0.26f, 0.16f, 0.48f, 58, new Color(0.78f, 1f, 1f, 0.92f), new Color(0.05f, 0.38f, 1f, 0f))
            };

            EnsureReviewedCombatCueAudioBanks();
            CombatVfxCueProfile profile = LoadOrCreate<CombatVfxCueProfile>(CombatVfxCueProfilePath);
            ConfigureCombatVfxCueProfile(profile, prefabs);
            ApplyMissileShieldCueProfileAudio(profile);
            ActionFoundationHitFeedbackSfxSetup.ApplyMasterHitFeedbackCueAudioToProfile();
            return profile;
        }

        private static void ConfigureSceneCombatVfx(Scene scene, CombatVfxCueProfile profile)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            Transform poolRoot = EnsureRoot(scene, PoolRootName).transform;
            PlayerActionController player = RequireObject<PlayerActionController>(roots, "player action controller");
            ConfigurePlayerCombatVfx(player, profile, poolRoot);

            BasicSoldierEnemy[] soldiers = CollectSoldiers(roots);
            for (int i = 0; i < soldiers.Length; i++)
            {
                ConfigureEnemyCombatVfx(soldiers[i], profile, poolRoot);
            }
        }

        private static void ConfigurePlayerCombatVfx(PlayerActionController player, CombatVfxCueProfile profile, Transform poolRoot)
        {
            CombatVfxCuePlayer cuePlayer = EnsureComponent<CombatVfxCuePlayer>(player.gameObject);
            PlayerCombatVfxCueDriver driver = EnsureComponent<PlayerCombatVfxCueDriver>(player.gameObject);
            PerfectDodgeVfxDirector perfectDodgeDirector = EnsureComponent<PerfectDodgeVfxDirector>(player.gameObject);
            CombatHealth playerHealth = player.GetComponent<CombatHealth>();
            PlayerRangedBasicAttackAction rangedBasicAttackAction = player.GetComponent<PlayerRangedBasicAttackAction>();
            Transform attackAnchor = EnsureChild(player.transform, "Player_CombatVfx_AttackAnchor", new Vector3(0f, 1.05f, 0.65f));
            Transform dodgeAnchor = EnsureChild(player.transform, "Player_CombatVfx_DodgeAnchor", new Vector3(0f, 0.18f, -0.22f));

            SetObjectReference(cuePlayer, "profile", profile);
            SetObjectReference(cuePlayer, "pooledRoot", poolRoot);
            SetObjectReference(driver, "actionController", player);
            SetObjectReference(driver, "playerHealth", playerHealth);
            SetObjectReference(driver, "perfectDodgeVfxDirector", perfectDodgeDirector);
            SetObjectReference(driver, "cuePlayer", cuePlayer);
            SetObjectReference(driver, "attackAnchor", attackAnchor);
            SetObjectReference(driver, "dodgeAnchor", dodgeAnchor);
            SetObjectReference(driver, "damageAnchor", attackAnchor);
            SetEnum(driver, "damagedCueId", (int)CombatVfxCueId.PlayerDamaged);
            SetEnum(driver, "criticalCueId", (int)CombatVfxCueId.PlayerCritical);
            ConfigurePerfectDodgeCueDriverDefaults(driver);
            ConfigurePerfectDodgeVfxDirector(perfectDodgeDirector, player, playerHealth);
            SetFloat(driver, "pressureDamageCueScale", 0.62f);
            if (rangedBasicAttackAction != null)
            {
                PlayerRangedBasicVfxCueDriver rangedDriver =
                    EnsureComponent<PlayerRangedBasicVfxCueDriver>(player.gameObject);
                Transform muzzleAnchor = rangedBasicAttackAction.FireOrigin != null
                    ? rangedBasicAttackAction.FireOrigin
                    : attackAnchor;
                SetObjectReference(rangedDriver, "rangedBasicAttackAction", rangedBasicAttackAction);
                SetObjectReference(rangedDriver, "cuePlayer", cuePlayer);
                SetObjectReference(rangedDriver, "muzzleAnchor", muzzleAnchor);
                SetEnum(rangedDriver, "muzzleFlashCueId", (int)CombatVfxCueId.PlayerRangedMuzzleFlash);
                SetFloat(rangedDriver, "muzzleFlashIntensity", 1f);
                SetFloat(rangedDriver, "muzzleFlashAudioIntensity", 1f);
                SetBool(rangedDriver, "playImpactVfx", PlayerRangedBasicVfxCueDriver.DefaultPlayImpactVfx);
                SetBool(rangedDriver, "playImpactAudio", PlayerRangedBasicVfxCueDriver.DefaultPlayImpactAudio);
                SetEnum(rangedDriver, "impactCueId", (int)PlayerRangedBasicVfxCueDriver.DefaultImpactCueId);
                SetFloat(rangedDriver, "impactIntensity", PlayerRangedBasicVfxCueDriver.DefaultImpactIntensity);
                SetFloat(rangedDriver, "impactAudioIntensity", PlayerRangedBasicVfxCueDriver.DefaultImpactAudioIntensity);
            }

            EditorUtility.SetDirty(player.gameObject);
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

        private static void ConfigurePerfectDodgeVfxDirector(
            PerfectDodgeVfxDirector director,
            PlayerActionController actionController,
            CombatHealth playerHealth)
        {
            director.Configure(actionController, playerHealth);
            SetObjectReference(director, "worldFxMaterial", LoadOrCreatePerfectDodgeWorldFxMaterial());
            SetObjectReference(director, "afterimageMaterial", LoadOrCreatePerfectDodgeAfterimageMaterial());
            SetBool(director, "playProceduralVisuals", false);
            director.ConfigureAudio(
                LoadReviewedAudioClips(PlayerPerfectDodgeTimeWarpClipPaths),
                Array.Empty<AudioClip>());
            EditorUtility.SetDirty(director);
        }

        private static void ConfigureEnemyCombatVfx(BasicSoldierEnemy soldier, CombatVfxCueProfile profile, Transform poolRoot)
        {
            CombatVfxCuePlayer cuePlayer = EnsureComponent<CombatVfxCuePlayer>(soldier.gameObject);
            EnemyCombatVfxCueDriver driver = EnsureComponent<EnemyCombatVfxCueDriver>(soldier.gameObject);
            CombatHealth health = RequireComponent<CombatHealth>(soldier.gameObject, $"{soldier.name} health");
            EnemyElitePatternController eliteController = soldier.GetComponent<EnemyElitePatternController>();
            Transform anchor = EnsureChild(soldier.transform, "Enemy_CombatVfx_CueAnchor", new Vector3(0f, 1.05f, 0.35f));

            SetObjectReference(cuePlayer, "profile", profile);
            SetObjectReference(cuePlayer, "pooledRoot", poolRoot);
            SetObjectReference(driver, "agentSource", soldier);
            SetObjectReference(driver, "health", health);
            SetObjectReference(driver, "cuePlayer", cuePlayer);
            SetObjectReference(driver, "cueAnchor", anchor);
            SetObjectReference(driver, "elitePatternController", eliteController);
            SetFloat(driver, "damageCueIntensity", 1f);
            SetFloat(driver, "pressureDamageCueScale", 0.66f);
            SetBool(driver, "playDamageVfx", true);
            SetPatternCueOverrides(driver);
            SetEliteCueOverrides(driver);
            ConfigureThreatTelegraphVisual(soldier);
            EditorUtility.SetDirty(soldier.gameObject);
        }

        private static void ConfigureThreatTelegraphVisual(BasicSoldierEnemy soldier)
        {
            EnemyAttackTelegraphPresenter presenter = soldier.GetComponent<EnemyAttackTelegraphPresenter>();
            if (presenter == null || presenter.TelegraphRenderer == null)
            {
                return;
            }

            Material warningMaterial = LoadOrCreateParticleMaterial("DB_CombatTelegraph_Warning", new Color(1f, 0.2f, 0.04f, 0.7f));
            Material highlightMaterial = LoadOrCreateParticleMaterial("DB_CombatTelegraph_Highlight", new Color(1f, 0.86f, 0.24f, 0.86f));
            Renderer telegraphRenderer = presenter.TelegraphRenderer;
            telegraphRenderer.sharedMaterial = warningMaterial;
            ConfigureRendererForCue(telegraphRenderer);

            MeshFilter meshFilter = telegraphRenderer.GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                meshFilter.sharedMesh = LoadPrimitiveMesh(PrimitiveType.Cylinder);
            }

            EnsurePrimitiveChild(
                telegraphRenderer.transform,
                "ReadableAttackTelegraph_CenterLine",
                PrimitiveType.Cube,
                highlightMaterial,
                new Vector3(0f, 0.04f, 0.18f),
                Vector3.zero,
                new Vector3(0.12f, 0.02f, 1.35f));

            EnsurePrimitiveChild(
                telegraphRenderer.transform,
                "ReadableAttackTelegraph_ReleaseEdge",
                PrimitiveType.Cube,
                highlightMaterial,
                new Vector3(0f, 0.05f, 0.72f),
                new Vector3(0f, 90f, 0f),
                new Vector3(0.64f, 0.02f, 0.06f));

            presenter.ConfigureStyle(
                new Vector3(0.28f, 0.018f, 0.56f),
                new Vector3(1.08f, 0.02f, 1.65f),
                new Vector3(1.32f, 0.024f, 1.95f),
                new Vector3(0f, 0f, -0.08f),
                new Vector3(0f, 0f, 0.12f),
                new Color(1f, 0.48f, 0.08f, 0.62f),
                new Color(1f, 0.08f, 0.02f, 0.86f),
                new Color(1f, 0.92f, 0.32f, 0.95f));
            EditorUtility.SetDirty(presenter);
        }

        private static void ValidateCuePlayer(GameObject owner, CombatVfxCueProfile profile, string label)
        {
            CombatVfxCuePlayer cuePlayer = owner.GetComponent<CombatVfxCuePlayer>();
            if (cuePlayer == null)
            {
                throw new InvalidOperationException($"{label} is missing CombatVfxCuePlayer.");
            }

            if (cuePlayer.Profile != profile)
            {
                throw new InvalidOperationException($"{label} CombatVfxCuePlayer should reference {CombatVfxCueProfilePath}.");
            }
        }

        private static void ValidateReviewedCombatFeedbackPlayback(CombatVfxCueProfile profile)
        {
            if (profile.PlaybackMode != CombatVfxCuePlaybackMode.ReviewedCombatFeedbackOnly)
            {
                return;
            }

            ValidateReviewedPlaybackCue(profile, CombatVfxCueId.PlayerDodgeStart);
            ValidateReviewedPlaybackCue(profile, CombatVfxCueId.PlayerRangedMuzzleFlash);
            ValidateReviewedPlaybackCue(profile, CombatVfxCueId.PlayerRangedProjectileImpact);
            ValidateReviewedPlaybackCue(profile, CombatVfxCueId.PlayerDamaged);
            ValidateReviewedPlaybackCue(profile, CombatVfxCueId.PlayerCritical);
            ValidateReviewedPlaybackCue(profile, CombatVfxCueId.EnemyHit);
            ValidateReviewedPlaybackCue(profile, CombatVfxCueId.EliteSummonSignal);
            ValidateReviewedPlaybackCue(profile, CombatVfxCueId.SummonBlockOpportunity);
            ValidateReviewedPlaybackCue(profile, CombatVfxCueId.SummonFollowupWindow);
            ValidateReviewedPlaybackCue(profile, CombatVfxCueId.SummonFollowupHit);
            ValidateReviewedPlaybackCue(profile, CombatVfxCueId.SummonProjectileIntercept);
            ValidateReviewedPlaybackCue(profile, CombatVfxCueId.PlayerPerfectDodgeTimeField);
            ValidateReviewedPlaybackCue(profile, CombatVfxCueId.PlayerPerfectDodgePulsewave);
            ValidateReviewedPlaybackCue(profile, CombatVfxCueId.PlayerPerfectDodgeHoloCube);
            ValidateReviewedPlaybackCue(profile, CombatVfxCueId.PlayerPerfectDodgeWindow);
            ValidateReviewedPlaybackCue(profile, CombatVfxCueId.PlayerPerfectDodgeShieldBlockImpact);
            ValidateReviewedPlaybackCue(profile, CombatVfxCueId.PlayerSummonPreSpawnPortal);
            ValidateReviewedPlaybackCue(profile, CombatVfxCueId.PlayerSummonLandingCrater);
            ValidateReviewedPlaybackCue(profile, CombatVfxCueId.PlayerSummonDragonBreathAudio);

            ValidateCueEmbeddedAudioBank(profile, CombatVfxCueId.PlayerDodgeStart, PlayerDodgeStartAudioName);
            ValidateCueEmbeddedAudioBank(profile, CombatVfxCueId.PlayerRangedMuzzleFlash, PlayerRangedGunshotAudioName);
            ValidateCueDoesNotHaveEmbeddedAudioBank(
                profile,
                CombatVfxCueId.PlayerRangedProjectileImpact,
                PlayerRangedProjectileImpactAudioName);
            ValidateCueProfileAudioBank(profile, CombatVfxCueId.PlayerRangedProjectileImpact);
            ValidateCueEmbeddedAudioBank(profile, CombatVfxCueId.EliteSummonSignal, EliteSummonSignalAudioName);
            ValidateCueEmbeddedAudioBank(profile, CombatVfxCueId.SummonBlockOpportunity, SummonBlockOpportunityAudioName);
            ValidateCueEmbeddedAudioBank(profile, CombatVfxCueId.SummonProjectileIntercept, SummonBlockOpportunityAudioName);
            ValidateCueEmbeddedAudioBank(profile, CombatVfxCueId.SummonFollowupWindow, SummonFollowupWindowAudioName);
            ValidateCueEmbeddedAudioBank(profile, CombatVfxCueId.PlayerPerfectDodgeTimeField, PlayerPerfectDodgeTimeWarpAudioName);
            ValidateCueEmbeddedAudioBank(profile, CombatVfxCueId.PlayerPerfectDodgeWindow, PlayerPerfectDodgeSuccessAudioName);
            ValidateCueProfileAudioBank(profile, CombatVfxCueId.PlayerPerfectDodgeShieldBlockImpact);
            ValidateCueEmbeddedAudioBank(profile, CombatVfxCueId.PlayerSummonPreSpawnPortal, PlayerSummonPreSpawnPortalAudioName);
            ValidateCueEmbeddedAudioBank(profile, CombatVfxCueId.PlayerSummonLandingCrater, PlayerSummonLandingCraterAudioName);
            ValidateCueEmbeddedAudioBank(profile, CombatVfxCueId.PlayerSummonDragonBreathAudio, PlayerSummonDragonBreathAudioName);
            ValidateCueProfileAudioBank(profile, CombatVfxCueId.PlayerDamaged);
            ValidatePlayerRangedMuzzleFlashPrefab(profile);
            ValidateCueDoesNotHaveChild(profile, CombatVfxCueId.SummonBlockOpportunity, "CueAssetVfx_MagicMissilesPressureStorm");
            ValidateCueDoesNotHaveChild(profile, CombatVfxCueId.SummonFollowupWindow, "CueAssetVfx_MagicMissilesFollowupCircle");
        }

        private static void ValidateReviewedPlaybackCue(CombatVfxCueProfile profile, CombatVfxCueId cueId)
        {
            if (!profile.AllowsPlayback(cueId))
            {
                throw new InvalidOperationException($"{cueId} is part of reviewed combat feedback but is blocked by {profile.PlaybackMode}.");
            }
        }

        private static void ValidateCueEmbeddedAudioBank(
            CombatVfxCueProfile profile,
            CombatVfxCueId cueId,
            string audioObjectName)
        {
            if (!profile.TryGetCue(cueId, out CombatVfxCue cue) || cue.Prefab == null)
            {
                throw new InvalidOperationException($"{cueId} should reference a cue prefab before validating reviewed audio.");
            }

            Transform audioRoot = cue.Prefab.transform.Find(audioObjectName);
            if (audioRoot == null)
            {
                throw new InvalidOperationException($"{cueId} should include reviewed audio child {audioObjectName}.");
            }

            AudioSource source = audioRoot.GetComponent<AudioSource>();
            CombatVfxCueAudioRandomizer randomizer = audioRoot.GetComponent<CombatVfxCueAudioRandomizer>();
            if (source == null || randomizer == null || randomizer.Source != source)
            {
                throw new InvalidOperationException($"{cueId} reviewed audio child {audioObjectName} should have a linked AudioSource and CombatVfxCueAudioRandomizer.");
            }

            if (!source.enabled || randomizer.ClipCount <= 0 || randomizer.BaseVolume <= 0f)
            {
                throw new InvalidOperationException($"{cueId} reviewed audio child {audioObjectName} should be enabled and contain audible clips.");
            }
        }

        private static void ValidatePlayerRangedMuzzleFlashPrefab(CombatVfxCueProfile profile)
        {
            if (!profile.TryGetCue(CombatVfxCueId.PlayerRangedMuzzleFlash, out CombatVfxCue cue)
                || cue.Prefab == null)
            {
                throw new InvalidOperationException("PlayerRangedMuzzleFlash should reference a cue prefab.");
            }

            if (cue.Prefab.transform.Find("MuzzleFlash_VefectsRifleDemo") == null)
            {
                throw new InvalidOperationException("PlayerRangedMuzzleFlash should preserve the promoted Vefects rifle muzzle prefab.");
            }

            ValidatePromotedMuzzleSmokeParticle(
                cue.Prefab.transform.Find("MuzzleFlash_VefectsRifleDemo/Smoke Front"),
                "Smoke Front",
                0.12f,
                0.42f);
            ValidatePromotedMuzzleSmokeParticle(
                cue.Prefab.transform.Find("MuzzleFlash_VefectsRifleDemo/Smoke Side"),
                "Smoke Side",
                0.10f,
                0.34f);

            string[] blockedChildren =
            {
                "RifleFlash_FrontBurst",
                "RifleFlash_SideTongues",
                "RifleFlash_SmokePuff",
                "RifleFlash_LightSpark"
            };

            for (int i = 0; i < blockedChildren.Length; i++)
            {
                if (cue.Prefab.transform.Find(blockedChildren[i]) != null)
                {
                    throw new InvalidOperationException(
                        $"PlayerRangedMuzzleFlash should use the promoted rifle muzzle asset, not handcrafted smoke layer {blockedChildren[i]}.");
                }
            }
        }

        private static void ValidatePromotedMuzzleSmokeParticle(
            Transform smokeRoot,
            string label,
            float minimumAlpha,
            float minimumSize)
        {
            if (smokeRoot == null)
            {
                throw new InvalidOperationException($"PlayerRangedMuzzleFlash should keep promoted Vefects {label}.");
            }

            ParticleSystem particleSystem = smokeRoot.GetComponent<ParticleSystem>();
            ParticleSystemRenderer renderer = smokeRoot.GetComponent<ParticleSystemRenderer>();
            if (particleSystem == null || renderer == null || !renderer.enabled)
            {
                throw new InvalidOperationException($"{label} should keep an enabled promoted muzzle smoke particle renderer.");
            }

            ParticleSystem.MainModule main = particleSystem.main;
            ParticleSystem.MinMaxGradient startColor = main.startColor;
            float alpha = Mathf.Max(startColor.color.a, startColor.colorMax.a, startColor.colorMin.a);
            if (alpha < minimumAlpha)
            {
                throw new InvalidOperationException($"{label} muzzle smoke alpha is too subtle for gameplay readability.");
            }

            ParticleSystem.MinMaxCurve startSize = main.startSize;
            float size = Mathf.Max(startSize.constant, startSize.constantMax, startSize.constantMin);
            if (size < minimumSize)
            {
                throw new InvalidOperationException($"{label} muzzle smoke size is too small for gameplay readability.");
            }
        }

        private static void ValidateCueDoesNotHaveChild(
            CombatVfxCueProfile profile,
            CombatVfxCueId cueId,
            string childName)
        {
            if (!profile.TryGetCue(cueId, out CombatVfxCue cue) || cue.Prefab == null)
            {
                throw new InvalidOperationException($"{cueId} should reference a cue prefab.");
            }

            if (cue.Prefab.transform.Find(childName) != null)
            {
                throw new InvalidOperationException($"{cueId} should not include ambiguous magic-circle overlay {childName}.");
            }
        }

        private static void ValidateCueDoesNotHaveEmbeddedAudioBank(
            CombatVfxCueProfile profile,
            CombatVfxCueId cueId,
            string audioObjectName)
        {
            if (!profile.TryGetCue(cueId, out CombatVfxCue cue) || cue.Prefab == null)
            {
                throw new InvalidOperationException($"{cueId} should reference a cue prefab before validating reviewed audio.");
            }

            if (cue.Prefab.transform.Find(audioObjectName) != null)
            {
                throw new InvalidOperationException(
                    $"{cueId} should use profile-owned hit SFX only; remove duplicate embedded audio child {audioObjectName}.");
            }
        }

        private static void ValidateCueProfileAudioBank(CombatVfxCueProfile profile, CombatVfxCueId cueId)
        {
            if (!profile.TryGetCue(cueId, out CombatVfxCue cue))
            {
                throw new InvalidOperationException($"{cueId} should exist before validating profile audio.");
            }

            if (cue.AudioClipCount <= 0 || cue.AudioBaseVolume <= 0f)
            {
                throw new InvalidOperationException($"{cueId} should keep its reviewed profile audio bank.");
            }
        }

        private static void ValidateEnemyCombatVfx(BasicSoldierEnemy soldier, CombatVfxCueProfile profile)
        {
            ValidateCuePlayer(soldier.gameObject, profile, soldier.name);
            EnemyCombatVfxCueDriver driver = soldier.GetComponent<EnemyCombatVfxCueDriver>();
            if (driver == null)
            {
                throw new InvalidOperationException($"{soldier.name} is missing EnemyCombatVfxCueDriver.");
            }

            SerializedObject serializedObject = new SerializedObject(driver);
            if (serializedObject.FindProperty("agentSource").objectReferenceValue != soldier)
            {
                throw new InvalidOperationException($"{soldier.name} VFX driver should reference its local BasicSoldierEnemy.");
            }

            if (serializedObject.FindProperty("health").objectReferenceValue != soldier.SelfHealth)
            {
                throw new InvalidOperationException($"{soldier.name} VFX driver should reference local CombatHealth.");
            }

            ValidateFloat(driver, "damageCueIntensity", 1f);
            ValidateFloat(driver, "pressureDamageCueScale", 0.66f);

            if (serializedObject.FindProperty("patternCueOverrides").arraySize != 8)
            {
                throw new InvalidOperationException($"{soldier.name} should have 8 pattern VFX cue overrides.");
            }

            if (serializedObject.FindProperty("eliteCueOverrides").arraySize != 5)
            {
                throw new InvalidOperationException($"{soldier.name} should have 5 elite VFX cue overrides.");
            }
        }

        private static void ValidateProjectileCueVisual(CombatVfxCueProfile profile, CombatVfxCueId cueId, float minimumTravelDistance)
        {
            if (!profile.TryGetCue(cueId, out CombatVfxCue cue) || cue.Prefab == null)
            {
                throw new InvalidOperationException($"{cueId} should reference a promoted projectile cue prefab.");
            }

            CombatVfxCueVisual visual = cue.Prefab.GetComponentInChildren<CombatVfxCueVisual>(includeInactive: true);
            if (visual == null)
            {
                throw new InvalidOperationException($"{cueId} should include CombatVfxCueVisual.");
            }

            SerializedObject visualObject = new SerializedObject(visual);
            float forwardTravelDistance = RequireProperty(visualObject, "forwardTravelDistance").floatValue;
            if (forwardTravelDistance < minimumTravelDistance)
            {
                throw new InvalidOperationException($"{cueId} projectile cue should travel forward at least {minimumTravelDistance:0.0}m, found {forwardTravelDistance:0.0}m.");
            }
        }

        private static void ValidateNoImportedAssetDependencies(CombatVfxCueId cueId, string prefabPath)
        {
            string[] dependencies = AssetDatabase.GetDependencies(prefabPath, recursive: true);
            for (int i = 0; i < dependencies.Length; i++)
            {
                string dependency = dependencies[i].Replace('\\', '/');
                if (dependency.Contains("/_Imported/", StringComparison.Ordinal))
                {
                    throw new InvalidOperationException($"{cueId} should not depend on raw imported VFX asset {dependency}.");
                }
            }
        }

        private static void ConfigureCombatVfxCueProfile(CombatVfxCueProfile profile, CombatCuePrefabs prefabs)
        {
            CueDefinition[] cues =
            {
                new CueDefinition(CombatVfxCueId.PlayerBasicAttackStart, prefabs.PlayerAttackStart, new Vector3(0f, 0f, 0.35f), Vector3.zero, new Vector3(1f, 1f, 1.25f), 0.40f, false, true),
                new CueDefinition(CombatVfxCueId.PlayerBasicAttackHit, prefabs.PlayerAttackHit, new Vector3(0f, 0f, 0.82f), Vector3.zero, Vector3.one, 0.34f, false, true),
                new CueDefinition(CombatVfxCueId.PlayerDodgeStart, prefabs.PlayerDodgeStart, new Vector3(0f, 0f, -0.15f), Vector3.zero, new Vector3(1.1f, 0.8f, 1.5f), 0.46f, false, true),
                new CueDefinition(CombatVfxCueId.PlayerPerfectDodgeTimeField, prefabs.PlayerPerfectDodgeTimeField, new Vector3(0f, 0.52f, 0f), Vector3.zero, new Vector3(0.72f, 0.72f, 0.72f), 3.0f, true, false),
                new CueDefinition(CombatVfxCueId.PlayerPerfectDodgePulsewave, prefabs.PlayerPerfectDodgePulsewave, new Vector3(0f, 0.58f, 0f), Vector3.zero, new Vector3(0.9f, 0.9f, 0.9f), 0.9f, false, false),
                new CueDefinition(CombatVfxCueId.PlayerPerfectDodgeHoloCube, prefabs.PlayerPerfectDodgeHoloCube, new Vector3(0f, 0.74f, 0.05f), Vector3.zero, new Vector3(0.58f, 0.58f, 0.58f), 1.2f, true, false),
                new CueDefinition(CombatVfxCueId.PlayerPerfectDodgeWindow, prefabs.PlayerPerfectDodgeWindow, new Vector3(0f, 0.34f, 0f), Vector3.zero, Vector3.one, 1.15f, true, false),
                new CueDefinition(CombatVfxCueId.PlayerPerfectDodgeShieldBlockImpact, prefabs.PlayerPerfectDodgeShieldBlockImpact, Vector3.zero, Vector3.zero, new Vector3(0.55f, 0.55f, 0.55f), 1.0f, false, true),
                new CueDefinition(CombatVfxCueId.PlayerSummonPreSpawnPortal, prefabs.PlayerSummonPreSpawnPortal, Vector3.zero, Vector3.zero, new Vector3(0.92f, 0.92f, 0.92f), 0.62f, false, true),
                new CueDefinition(CombatVfxCueId.PlayerSummonLandingCrater, prefabs.PlayerSummonLandingCrater, new Vector3(0f, 0.035f, 0f), Vector3.zero, new Vector3(1.08f, 0.72f, 1.08f), 0.84f, false, false),
                new CueDefinition(CombatVfxCueId.PlayerSummonDragonBreathAudio, prefabs.PlayerSummonDragonBreathAudio, Vector3.zero, Vector3.zero, Vector3.one, 0.05f, false, true),
                new CueDefinition(CombatVfxCueId.PlayerDamaged, prefabs.EnemyHit, new Vector3(0f, 0.72f, 0f), Vector3.zero, new Vector3(0.14f, 0.12f, 0.14f), 0.22f, true, false),
                new CueDefinition(CombatVfxCueId.PlayerCritical, prefabs.EnemyHit, new Vector3(0f, 0.82f, 0f), Vector3.zero, new Vector3(0.1f, 0.09f, 0.1f), 0.22f, true, false),
                new CueDefinition(CombatVfxCueId.PlayerRangedMuzzleFlash, prefabs.PlayerRangedMuzzleFlash, new Vector3(0f, 0f, 0.08f), Vector3.zero, new Vector3(0.82f, 0.82f, 0.82f), 0.58f, false, true),
                new CueDefinition(CombatVfxCueId.PlayerRangedProjectileImpact, prefabs.PlayerRangedProjectileImpact, new Vector3(0f, 0.04f, 0f), Vector3.zero, new Vector3(0.9f, 0.9f, 0.9f), 0.46f, false, true),
                new CueDefinition(CombatVfxCueId.EnemyWindup, prefabs.EnemyWindup, Vector3.zero, Vector3.zero, Vector3.one, 0.55f, true, true),
                new CueDefinition(CombatVfxCueId.EnemyAttackActive, prefabs.EnemyAttackActive, new Vector3(0f, 0f, 0.7f), Vector3.zero, Vector3.one, 0.28f, false, true),
                new CueDefinition(CombatVfxCueId.EnemyHit, prefabs.EnemyHit, new Vector3(0f, 0.1f, 0f), Vector3.zero, Vector3.one, 0.32f, false, true),
                new CueDefinition(CombatVfxCueId.EnemyDeath, prefabs.EnemyDeath, new Vector3(0f, 0.05f, 0f), Vector3.zero, new Vector3(1.25f, 1f, 1.25f), 0.82f, false, false),
                new CueDefinition(CombatVfxCueId.EliteSignal, prefabs.EliteShield, new Vector3(0f, 0.1f, 0f), Vector3.zero, Vector3.one, 0.65f, true, false),
                new CueDefinition(CombatVfxCueId.EnemyClosePunishWindup, prefabs.ClosePunishWindup, Vector3.zero, Vector3.zero, Vector3.one, 0.52f, true, true),
                new CueDefinition(CombatVfxCueId.EnemyClosePunishActive, prefabs.ClosePunishActive, new Vector3(0f, 0f, 0.75f), Vector3.zero, Vector3.one, 0.28f, false, true),
                new CueDefinition(CombatVfxCueId.EnemyLungeStrikeWindup, prefabs.LungeWindup, new Vector3(0f, 0f, 0.18f), Vector3.zero, new Vector3(0.48f, 0.48f, 0.58f), 0.22f, true, true),
                new CueDefinition(CombatVfxCueId.EnemyLungeStrikeActive, prefabs.LungeActive, new Vector3(0f, 0f, 0.62f), Vector3.zero, new Vector3(0.52f, 0.52f, 0.7f), 0.16f, false, true),
                new CueDefinition(CombatVfxCueId.EnemyHeavyWindupWindup, prefabs.HeavyWindup, new Vector3(0f, 0.12f, 0f), Vector3.zero, new Vector3(1.35f, 1.2f, 1.35f), 0.72f, true, false),
                new CueDefinition(CombatVfxCueId.EnemyHeavyWindupActive, prefabs.HeavyActive, new Vector3(0f, -0.15f, 0.85f), Vector3.zero, new Vector3(1.55f, 0.6f, 1.55f), 0.40f, false, true),
                new CueDefinition(CombatVfxCueId.EnemyLinePressureWindup, prefabs.LineWindup, new Vector3(0f, 0f, 0.45f), Vector3.zero, new Vector3(0.8f, 1f, 2.1f), 0.58f, true, true),
                new CueDefinition(CombatVfxCueId.EnemyLinePressureActive, prefabs.LineActive, new Vector3(0f, 0f, 0.85f), Vector3.zero, new Vector3(1.15f, 1.15f, 1.15f), 0.38f, false, true),
                new CueDefinition(CombatVfxCueId.EnemyFanPressureWindup, prefabs.FanWindup, new Vector3(0f, 0f, 0.45f), Vector3.zero, new Vector3(1.25f, 1f, 1.65f), 0.58f, true, true),
                new CueDefinition(CombatVfxCueId.EnemyFanPressureActive, prefabs.FanActive, new Vector3(0f, 0f, 1.35f), Vector3.zero, new Vector3(1.6f, 1f, 2.25f), 0.36f, false, true),
                new CueDefinition(CombatVfxCueId.EnemyRetreatShotWindup, prefabs.RetreatShotWindup, new Vector3(0f, 0f, 0.35f), Vector3.zero, Vector3.one, 0.48f, true, true),
                new CueDefinition(CombatVfxCueId.EnemyRetreatShotActive, prefabs.RetreatShotActive, new Vector3(0f, 0f, 0.75f), Vector3.zero, Vector3.one, 0.34f, false, true),
                new CueDefinition(CombatVfxCueId.EnemyRetreatBlinkWindup, prefabs.RetreatBlinkWindup, Vector3.zero, Vector3.zero, new Vector3(1.1f, 1.1f, 1.1f), 0.48f, true, false),
                new CueDefinition(CombatVfxCueId.EnemyRetreatBlinkActive, prefabs.RetreatBlinkActive, new Vector3(0f, -0.05f, 0f), Vector3.zero, new Vector3(1.35f, 0.65f, 1.35f), 0.34f, false, false),
                new CueDefinition(CombatVfxCueId.EnemyGuardBreakWindup, prefabs.GuardBreakWindup, new Vector3(0f, 0.1f, 0f), Vector3.zero, new Vector3(1.25f, 1.1f, 1.25f), 0.82f, true, false),
                new CueDefinition(CombatVfxCueId.EnemyGuardBreakActive, prefabs.GuardBreakActive, new Vector3(0f, -0.12f, 0.8f), Vector3.zero, new Vector3(1.55f, 0.65f, 1.55f), 0.42f, false, true),
                new CueDefinition(CombatVfxCueId.EliteShieldSignal, prefabs.EliteShield, Vector3.zero, Vector3.zero, new Vector3(0.46f, 0.34f, 0.46f), 0.28f, true, false),
                new CueDefinition(CombatVfxCueId.EliteArmorBreakSignal, prefabs.EliteArmorBreak, Vector3.zero, Vector3.zero, new Vector3(1.15f, 1f, 1.15f), 0.58f, true, false),
                new CueDefinition(CombatVfxCueId.EliteAuraSignal, prefabs.EliteAura, Vector3.zero, Vector3.zero, new Vector3(1.55f, 0.8f, 1.55f), 0.88f, true, false),
                new CueDefinition(CombatVfxCueId.EliteSummonSignal, prefabs.EliteSummon, Vector3.zero, Vector3.zero, new Vector3(0.28f, 0.22f, 0.28f), 0.28f, true, false),
                new CueDefinition(CombatVfxCueId.ElitePhaseSwapSignal, prefabs.ElitePhaseSwap, Vector3.zero, Vector3.zero, new Vector3(1.85f, 0.8f, 1.85f), 0.68f, true, false),
                new CueDefinition(CombatVfxCueId.SummonFollowupWindow, prefabs.SummonFollowupWindow, new Vector3(0f, 0.36f, 0.2f), Vector3.zero, new Vector3(0.20f, 0.16f, 0.20f), 0.26f, true, false),
                new CueDefinition(CombatVfxCueId.SummonFollowupHit, prefabs.PlayerRangedProjectileImpact, new Vector3(0f, 1.05f, -0.35f), Vector3.zero, new Vector3(1.08f, 0.9f, 1.08f), 0.30f, false, true),
                new CueDefinition(CombatVfxCueId.SummonFollowupMissed, prefabs.EnemyDeath, new Vector3(0f, 0.08f, -0.2f), Vector3.zero, new Vector3(0.46f, 0.36f, 0.46f), 0.38f, false, false),
                new CueDefinition(CombatVfxCueId.SummonBlockOpportunity, prefabs.SummonBlockOpportunity, new Vector3(0f, 0.24f, 0.42f), Vector3.zero, new Vector3(0.19f, 0.14f, 0.19f), 0.22f, true, false),
                new CueDefinition(CombatVfxCueId.PocketCleared, prefabs.EliteSummon, new Vector3(0f, 0.42f, 0f), Vector3.zero, new Vector3(0.036f, 0.027f, 0.036f), 0.18f, true, false),
                new CueDefinition(CombatVfxCueId.PocketFailed, prefabs.EnemyHit, new Vector3(0f, 0.18f, -0.08f), Vector3.zero, new Vector3(0.10f, 0.085f, 0.10f), 0.20f, false, true),
                new CueDefinition(CombatVfxCueId.SummonProjectileIntercept, prefabs.SummonProjectileIntercept, Vector3.zero, new Vector3(90f, 0f, 0f), new Vector3(0.82f, 0.82f, 0.82f), 0.30f, true, true)
            };

            SerializedObject serializedObject = new SerializedObject(profile);
            SerializedProperty cueArray = RequireProperty(serializedObject, "cues");
            cueArray.arraySize = cues.Length;
            for (int i = 0; i < cues.Length; i++)
            {
                SetCue(cueArray.GetArrayElementAtIndex(i), cues[i]);
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static void ApplyMissileShieldCueProfileAudio(CombatVfxCueProfile profile)
        {
            SerializedObject serializedObject = new SerializedObject(profile);
            SerializedProperty cues = RequireProperty(serializedObject, "cues");
            SerializedProperty cue = FindCueProperty(cues, CombatVfxCueId.PlayerPerfectDodgeShieldBlockImpact);
            if (cue == null)
            {
                throw new InvalidOperationException(
                    $"{CombatVfxCueProfilePath} is missing {CombatVfxCueId.PlayerPerfectDodgeShieldBlockImpact}.");
            }

            SetCueProfileAudio(
                cue,
                PlayerPerfectDodgeShieldBlockClipPaths,
                0.5f,
                0.96f,
                1.04f,
                0.88f,
                1.04f,
                0.10f,
                3f,
                28f,
                126);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static SerializedProperty FindCueProperty(SerializedProperty cues, CombatVfxCueId cueId)
        {
            int cueIdValue = (int)cueId;
            for (int i = 0; i < cues.arraySize; i++)
            {
                SerializedProperty cue = cues.GetArrayElementAtIndex(i);
                if (cue.FindPropertyRelative("cueId").intValue == cueIdValue)
                {
                    return cue;
                }
            }

            return null;
        }

        private static void SetCueProfileAudio(
            SerializedProperty cue,
            string[] clipPaths,
            float baseVolume,
            float minimumPitch,
            float maximumPitch,
            float minimumVolumeMultiplier,
            float maximumVolumeMultiplier,
            float spatialBlend,
            float minDistance,
            float maxDistance,
            int priority)
        {
            AudioClip[] clips = LoadReviewedAudioClips(clipPaths);
            SerializedProperty audioClips = cue.FindPropertyRelative("audioClips");
            audioClips.arraySize = clips.Length;
            for (int i = 0; i < clips.Length; i++)
            {
                audioClips.GetArrayElementAtIndex(i).objectReferenceValue = clips[i];
            }

            cue.FindPropertyRelative("audioBaseVolume").floatValue = baseVolume;
            cue.FindPropertyRelative("audioMinimumPitch").floatValue = minimumPitch;
            cue.FindPropertyRelative("audioMaximumPitch").floatValue = maximumPitch;
            cue.FindPropertyRelative("audioMinimumVolumeMultiplier").floatValue = minimumVolumeMultiplier;
            cue.FindPropertyRelative("audioMaximumVolumeMultiplier").floatValue = maximumVolumeMultiplier;
            cue.FindPropertyRelative("audioSpatialBlend").floatValue = spatialBlend;
            cue.FindPropertyRelative("audioMinDistance").floatValue = minDistance;
            cue.FindPropertyRelative("audioMaxDistance").floatValue = maxDistance;
            cue.FindPropertyRelative("audioPriority").intValue = priority;
        }

        private static void SetPatternCueOverrides(EnemyCombatVfxCueDriver driver)
        {
            CombatPatternVfxCueOverride[] overrides =
            {
                new CombatPatternVfxCueOverride(LoadPattern(ActionFoundationProfileSetup.EnemyPatternProfilePath), CombatVfxCueId.EnemyClosePunishWindup, CombatVfxCueId.EnemyClosePunishActive, 1f, 1f),
                new CombatPatternVfxCueOverride(LoadPattern(ActionFoundationProfileSetup.EnemyLungePatternProfilePath), CombatVfxCueId.EnemyLungeStrikeWindup, CombatVfxCueId.EnemyLungeStrikeActive, 0.55f, 0.48f),
                new CombatPatternVfxCueOverride(LoadPattern(ActionFoundationProfileSetup.EnemyHeavyWindupPatternProfilePath), CombatVfxCueId.EnemyHeavyWindupWindup, CombatVfxCueId.EnemyHeavyWindupActive, 1.18f, 1.25f),
                new CombatPatternVfxCueOverride(LoadPattern(ActionFoundationProfileSetup.EnemyLinePressurePatternProfilePath), CombatVfxCueId.EnemyLinePressureWindup, CombatVfxCueId.EnemyLinePressureActive, 1f, 1.05f),
                new CombatPatternVfxCueOverride(LoadPattern(ActionFoundationProfileSetup.EnemyFanPressurePatternProfilePath), CombatVfxCueId.EnemyFanPressureWindup, CombatVfxCueId.EnemyFanPressureActive, 1f, 1.08f),
                new CombatPatternVfxCueOverride(LoadPattern(ActionFoundationEnemyPatternExpansionSetup.RetreatShotPatternPath), CombatVfxCueId.EnemyRetreatShotWindup, CombatVfxCueId.EnemyRetreatShotActive, 0.95f, 1f),
                new CombatPatternVfxCueOverride(LoadPattern(ActionFoundationEnemyPatternExpansionSetup.RetreatBlinkPatternPath), CombatVfxCueId.EnemyRetreatBlinkWindup, CombatVfxCueId.EnemyRetreatBlinkActive, 1f, 1.15f),
                new CombatPatternVfxCueOverride(LoadPattern(ActionFoundationEnemyPatternExpansionSetup.GuardBreakPatternPath), CombatVfxCueId.EnemyGuardBreakWindup, CombatVfxCueId.EnemyGuardBreakActive, 1.2f, 1.3f)
            };

            SerializedObject serializedObject = new SerializedObject(driver);
            SerializedProperty array = RequireProperty(serializedObject, "patternCueOverrides");
            array.arraySize = overrides.Length;
            for (int i = 0; i < overrides.Length; i++)
            {
                SetPatternCueOverride(array.GetArrayElementAtIndex(i), overrides[i]);
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(driver);
        }

        private static void SetEliteCueOverrides(EnemyCombatVfxCueDriver driver)
        {
            CombatEliteVfxCueOverride[] overrides =
            {
                new CombatEliteVfxCueOverride(LoadElite(ActionFoundationEnemyPatternExpansionSetup.ShieldCycleEliteProfilePath), CombatVfxCueId.EliteShieldSignal, 1f),
                new CombatEliteVfxCueOverride(LoadElite(ActionFoundationEnemyPatternExpansionSetup.ArmorBreakEliteProfilePath), CombatVfxCueId.EliteArmorBreakSignal, 1.05f),
                new CombatEliteVfxCueOverride(LoadElite(ActionFoundationEnemyPatternExpansionSetup.AuraBufferEliteProfilePath), CombatVfxCueId.EliteAuraSignal, 1.1f),
                new CombatEliteVfxCueOverride(LoadElite(ActionFoundationEnemyPatternExpansionSetup.SummonPackageEliteProfilePath), CombatVfxCueId.EliteSummonSignal, 1.1f),
                new CombatEliteVfxCueOverride(LoadElite(ActionFoundationEnemyPatternExpansionSetup.PhaseSwapEliteProfilePath), CombatVfxCueId.ElitePhaseSwapSignal, 1.18f)
            };

            SerializedObject serializedObject = new SerializedObject(driver);
            SerializedProperty array = RequireProperty(serializedObject, "eliteCueOverrides");
            array.arraySize = overrides.Length;
            for (int i = 0; i < overrides.Length; i++)
            {
                SetEliteCueOverride(array.GetArrayElementAtIndex(i), overrides[i]);
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(driver);
        }

        private static GameObject SaveBurstPrefab(
            string name,
            Material material,
            ParticleSystemShapeType shapeType,
            float radius,
            float speed,
            float arcDegrees,
            float minLifetime,
            float maxLifetime,
            float minSize,
            float maxSize,
            int burstCount,
            Color startColor,
            Color endColor,
            float forwardTravelDistance = 0f)
        {
            string prefabPath = $"{PrefabRoot}/{name}.prefab";
            GameObject root = new GameObject(name);
            var renderers = new System.Collections.Generic.List<Renderer>();
            if (forwardTravelDistance > 0f)
            {
                AddProjectileCueGeometry(root, material, radius, speed, minSize, maxSize, renderers);
            }
            else
            {
                AddCueGeometry(root, material, shapeType, radius, speed, arcDegrees, minSize, maxSize, renderers);
            }

            CombatVfxCueVisual visual = root.AddComponent<CombatVfxCueVisual>();
            Vector3 startVisualScale = Vector3.one * Mathf.Clamp(1f - minSize, 0.72f, 1f);
            Vector3 endVisualScale = Vector3.one * Mathf.Clamp(1f + maxSize * 1.35f, 1.12f, 1.9f);
            float visualSpin = ResolveVisualSpin(shapeType, speed);
            float verticalLift = shapeType == ParticleSystemShapeType.Sphere ? Mathf.Clamp(radius * 0.45f, 0.04f, 0.22f) : 0f;
            ConfigureCueVisual(visual, renderers.ToArray(), Mathf.Max(0.12f, maxLifetime), startColor, endColor, startVisualScale, endVisualScale, visualSpin, verticalLift, forwardTravelDistance);
            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            return savedPrefab;
        }

        private static GameObject SavePerfectDodgeWindowPrefab(
            string name,
            Material material,
            ParticleSystemShapeType shapeType,
            float radius,
            float speed,
            float arcDegrees,
            float minLifetime,
            float maxLifetime,
            float minSize,
            float maxSize,
            int burstCount,
            Color startColor,
            Color endColor)
        {
            string prefabPath = $"{PrefabRoot}/{name}.prefab";
            GameObject root = new GameObject(name);
            GameObject savedPrefab;
            try
            {
                AttachPromotedHovlSciFiVfxPrefab(
                    root.transform,
                    PerfectDodgeHovlHexShieldChildName,
                    ImportedHovlHexShieldPrefabPath,
                    new Vector3(0f, 0.48f, 0f),
                    Vector3.zero,
                    Vector3.one * 0.92f,
                    loopParticles: true,
                    playOnAwake: true);
                ConfigureStablePromotedCueVisual(root, maxLifetime);

                savedPrefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                if (savedPrefab == null)
                {
                    throw new InvalidOperationException($"Failed to save perfect dodge Hovl shield prefab at {prefabPath}.");
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }

            AssetDatabase.ImportAsset(prefabPath, ImportAssetOptions.ForceUpdate);
            ValidatePerfectDodgeHovlShieldPrefab(savedPrefab);
            ValidateNoImportedAssetDependencies(CombatVfxCueId.PlayerPerfectDodgeWindow, prefabPath);
            return savedPrefab;
        }

        private static GameObject SavePromotedHovlSciFiCuePrefab(
            string name,
            string sourcePrefabPath,
            Vector3 localPosition,
            Vector3 localEuler,
            Vector3 localScale,
            bool? loopParticles,
            bool playOnAwake,
            float lifetimeSeconds,
            int minimumParticleSystems)
        {
            string prefabPath = $"{PrefabRoot}/{name}.prefab";
            GameObject root = new GameObject(name);
            GameObject savedPrefab;
            try
            {
                AttachPromotedHovlSciFiVfxPrefab(
                    root.transform,
                    name + "_Hovl",
                    sourcePrefabPath,
                    localPosition,
                    localEuler,
                    localScale,
                    loopParticles,
                    playOnAwake);
                ConfigureStablePromotedCueVisual(root, lifetimeSeconds);

                savedPrefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                if (savedPrefab == null)
                {
                    throw new InvalidOperationException($"Failed to save promoted Hovl cue prefab at {prefabPath}.");
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }

            AssetDatabase.ImportAsset(prefabPath, ImportAssetOptions.ForceUpdate);
            savedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            ValidatePromotedHovlSciFiCuePrefab(savedPrefab, name, minimumParticleSystems);
            ValidateNoImportedAssetDependencies(CombatVfxCueId.PlayerPerfectDodgeTimeField, prefabPath);
            return savedPrefab;
        }

        private static void ConfigureStablePromotedCueVisual(GameObject root, float lifetimeSeconds)
        {
            CombatVfxCueVisual visual = root.GetComponent<CombatVfxCueVisual>();
            if (visual == null)
            {
                visual = root.AddComponent<CombatVfxCueVisual>();
            }

            ConfigureCueVisual(
                visual,
                root.GetComponentsInChildren<Renderer>(includeInactive: true),
                Mathf.Max(0.05f, lifetimeSeconds),
                Color.white,
                Color.white,
                Vector3.one,
                Vector3.one,
                0f,
                0f,
                0f,
                overrideRendererColors: false);
        }

        private static void ValidatePromotedHovlSciFiCuePrefab(
            GameObject prefab,
            string name,
            int minimumParticleSystems)
        {
            if (prefab == null)
            {
                throw new InvalidOperationException($"{name} promoted Hovl cue prefab should be assigned.");
            }

            ParticleSystem[] particleSystems =
                prefab.GetComponentsInChildren<ParticleSystem>(includeInactive: true);
            if (particleSystems.Length < minimumParticleSystems)
            {
                throw new InvalidOperationException(
                    $"{name} should preserve its authored Hovl particle stack.");
            }

            Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(includeInactive: true);
            if (renderers.Length == 0)
            {
                throw new InvalidOperationException($"{name} should expose promoted Hovl renderers.");
            }

            CombatVfxCueVisual visual = prefab.GetComponent<CombatVfxCueVisual>();
            if (visual == null)
            {
                throw new InvalidOperationException($"{name} should include a stable CombatVfxCueVisual controller.");
            }

            if (visual.OverridesRendererColors)
            {
                throw new InvalidOperationException(
                    $"{name} should preserve the authored colors of its promoted renderers.");
            }

            if (prefab.GetComponentInChildren<Collider>(includeInactive: true) != null
                || prefab.GetComponentInChildren<Rigidbody>(includeInactive: true) != null)
            {
                throw new InvalidOperationException($"{name} promoted Hovl cue must remain visual-only.");
            }
        }

        private static void FreezeCueVisualScale(GameObject root)
        {
            CombatVfxCueVisual visual = root.GetComponent<CombatVfxCueVisual>();
            if (visual == null)
            {
                return;
            }

            SerializedObject serializedObject = new SerializedObject(visual);
            RequireProperty(serializedObject, "startScale").vector3Value = Vector3.one;
            RequireProperty(serializedObject, "endScale").vector3Value = Vector3.one;
            RequireProperty(serializedObject, "verticalLift").floatValue = 0f;
            RequireProperty(serializedObject, "forwardTravelDistance").floatValue = 0f;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(visual);
        }

        private static GameObject AttachPromotedHovlSciFiVfxPrefab(
            Transform parent,
            string childName,
            string sourcePrefabPath,
            Vector3 localPosition,
            Vector3 localEuler,
            Vector3 localScale,
            bool? loopParticles,
            bool playOnAwake)
        {
            DestroyChildIfPresent(parent, childName);
            GameObject sourcePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(sourcePrefabPath);
            if (sourcePrefab == null)
            {
                throw new InvalidOperationException($"Missing source Hovl Sci-fi VFX prefab at {sourcePrefabPath}.");
            }

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
            RemoveRigidbodies(vfxInstance);
            RemoveColliders(vfxInstance);
            DisableVfxAudioSources(vfxInstance);
            ConfigurePromotedHovlSciFiParticles(vfxInstance, loopParticles, playOnAwake);
            RemapPromotedHovlSciFiRendererDependencies(vfxInstance);
            EditorUtility.SetDirty(vfxInstance);
            return vfxInstance;
        }

        private static void ConfigurePromotedHovlSciFiParticles(
            GameObject root,
            bool? loopParticles,
            bool playOnAwake)
        {
            ParticleSystem[] particleSystems = root.GetComponentsInChildren<ParticleSystem>(includeInactive: true);
            for (int i = 0; i < particleSystems.Length; i++)
            {
                ParticleSystem particleSystem = particleSystems[i];
                ParticleSystem.MainModule main = particleSystem.main;
                if (loopParticles.HasValue)
                {
                    main.loop = loopParticles.Value;
                }

                main.simulationSpace = ParticleSystemSimulationSpace.Local;
                main.scalingMode = ParticleSystemScalingMode.Hierarchy;
                main.playOnAwake = playOnAwake;
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

        private static void RemoveRigidbodies(GameObject root)
        {
            Rigidbody[] rigidbodies = root.GetComponentsInChildren<Rigidbody>(includeInactive: true);
            for (int i = rigidbodies.Length - 1; i >= 0; i--)
            {
                UnityEngine.Object.DestroyImmediate(rigidbodies[i]);
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

        private static void RemapPromotedHovlSciFiRendererDependencies(GameObject root)
        {
            MeshFilter[] meshFilters = root.GetComponentsInChildren<MeshFilter>(includeInactive: true);
            for (int i = 0; i < meshFilters.Length; i++)
            {
                if (meshFilters[i].sharedMesh != null)
                {
                    meshFilters[i].sharedMesh = EnsurePromotedHovlSciFiMesh(meshFilters[i].sharedMesh);
                    EditorUtility.SetDirty(meshFilters[i]);
                }
            }

            SkinnedMeshRenderer[] skinnedRenderers =
                root.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true);
            for (int i = 0; i < skinnedRenderers.Length; i++)
            {
                if (skinnedRenderers[i].sharedMesh != null)
                {
                    skinnedRenderers[i].sharedMesh = EnsurePromotedHovlSciFiMesh(skinnedRenderers[i].sharedMesh);
                    EditorUtility.SetDirty(skinnedRenderers[i]);
                }
            }

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(includeInactive: true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                Material[] materials = renderer.sharedMaterials;
                for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                {
                    if (materials[materialIndex] != null)
                    {
                        materials[materialIndex] = EnsurePromotedHovlSciFiMaterial(materials[materialIndex]);
                    }
                }

                renderer.sharedMaterials = materials;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                renderer.allowOcclusionWhenDynamic = false;
                EditorUtility.SetDirty(renderer);
            }

            ParticleSystemRenderer[] particleRenderers =
                root.GetComponentsInChildren<ParticleSystemRenderer>(includeInactive: true);
            for (int i = 0; i < particleRenderers.Length; i++)
            {
                ParticleSystemRenderer particleRenderer = particleRenderers[i];
                Mesh mesh = particleRenderer.mesh;
                if (mesh != null)
                {
                    particleRenderer.mesh = EnsurePromotedHovlSciFiMesh(mesh);
                }

                int meshCount = particleRenderer.meshCount;
                if (meshCount > 0)
                {
                    Mesh[] meshes = new Mesh[meshCount];
                    int copiedCount = particleRenderer.GetMeshes(meshes);
                    for (int meshIndex = 0; meshIndex < copiedCount; meshIndex++)
                    {
                        if (meshes[meshIndex] != null)
                        {
                            meshes[meshIndex] = EnsurePromotedHovlSciFiMesh(meshes[meshIndex]);
                        }
                    }

                    particleRenderer.SetMeshes(meshes, copiedCount);
                }

                EditorUtility.SetDirty(particleRenderer);
            }
        }

        private static Material EnsurePromotedHovlSciFiMaterial(Material sourceMaterial)
        {
            string sourcePath = AssetDatabase.GetAssetPath(sourceMaterial).Replace('\\', '/');
            if (sourcePath.StartsWith("Assets/_Game/", StringComparison.Ordinal))
            {
                return sourceMaterial;
            }

            string targetPath = HovlSciFiEffectsMaterialRoot + "/DB_HovlSciFi_"
                + SanitizeAssetFileName(sourceMaterial.name)
                + ".mat";
            EnsureFolder(Path.GetDirectoryName(targetPath).Replace('\\', '/'));

            Material material = AssetDatabase.LoadAssetAtPath<Material>(targetPath);
            if (material == null)
            {
                material = new Material(EnsurePromotedHovlSciFiShader(sourceMaterial.shader));
                AssetDatabase.CreateAsset(material, targetPath);
            }

            Shader promotedShader = EnsurePromotedHovlSciFiShader(sourceMaterial.shader);
            material.shader = promotedShader;
            material.CopyPropertiesFromMaterial(sourceMaterial);
            material.shader = promotedShader;
            material.renderQueue = sourceMaterial.renderQueue;

            string[] textureProperties = sourceMaterial.GetTexturePropertyNames();
            for (int i = 0; i < textureProperties.Length; i++)
            {
                Texture texture = sourceMaterial.GetTexture(textureProperties[i]);
                if (texture != null)
                {
                    SetMaterialTextureIfPresent(
                        material,
                        textureProperties[i],
                        EnsurePromotedHovlSciFiTexture(texture));
                }
            }

            RemapSerializedHovlSciFiTextures(material);
            ApplyPerfectDodgeShieldReadabilityPolish(sourceMaterial, material);
            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(targetPath, ImportAssetOptions.ForceUpdate);
            return material;
        }

        private static void ApplyPerfectDodgeShieldReadabilityPolish(
            Material sourceMaterial,
            Material promotedMaterial)
        {
            if (sourceMaterial == null
                || promotedMaterial == null
                || !string.Equals(
                    sourceMaterial.name,
                    PerfectDodgeHovlHexShieldMaterialName,
                    StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            string[] opacityProperties = { "_Opacity", "_Textureopacity" };
            for (int i = 0; i < opacityProperties.Length; i++)
            {
                string propertyName = opacityProperties[i];
                if (promotedMaterial.HasProperty(propertyName))
                {
                    promotedMaterial.SetFloat(propertyName, PerfectDodgeHovlHexShieldOpacity);
                }
            }

            if (promotedMaterial.HasProperty("_Color"))
            {
                Color color = promotedMaterial.GetColor("_Color");
                color.a = PerfectDodgeHovlHexShieldOpacity;
                promotedMaterial.SetColor("_Color", color);
            }
        }

        private static Shader EnsurePromotedHovlSciFiShader(Shader sourceShader)
        {
            if (sourceShader == null)
            {
                return FindParticleShader();
            }

            string sourcePath = AssetDatabase.GetAssetPath(sourceShader).Replace('\\', '/');
            if (sourcePath.StartsWith("Assets/_Game/", StringComparison.Ordinal)
                || string.IsNullOrWhiteSpace(sourcePath)
                || !sourcePath.StartsWith("Assets/", StringComparison.Ordinal))
            {
                return sourceShader;
            }

            string targetPath = HovlSciFiEffectsShaderRoot + "/DB_HovlSciFi_"
                + SanitizeAssetFileName(Path.GetFileName(sourcePath));
            EnsureFolder(Path.GetDirectoryName(targetPath).Replace('\\', '/'));
            if (AssetDatabase.LoadAssetAtPath<Shader>(targetPath) == null)
            {
                if (!AssetDatabase.CopyAsset(sourcePath, targetPath))
                {
                    throw new InvalidOperationException($"Failed to promote Hovl Sci-fi shader from {sourcePath} to {targetPath}.");
                }
            }

            RemapPromotedHovlSciFiShaderDependencies(sourcePath, targetPath);
            AssetDatabase.ImportAsset(targetPath, ImportAssetOptions.ForceUpdate);
            Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(targetPath);
            if (shader == null)
            {
                throw new InvalidOperationException($"Failed to load promoted Hovl Sci-fi shader at {targetPath}.");
            }

            return shader;
        }

        private static void RemapPromotedHovlSciFiShaderDependencies(
            string sourceShaderPath,
            string targetShaderPath)
        {
            string targetAbsolutePath = ToProjectAbsolutePath(targetShaderPath);
            if (!File.Exists(targetAbsolutePath))
            {
                return;
            }

            string contents = File.ReadAllText(targetAbsolutePath);
            string[] dependencies = AssetDatabase.GetDependencies(sourceShaderPath, recursive: true);
            bool changed = false;
            for (int i = 0; i < dependencies.Length; i++)
            {
                string dependency = dependencies[i].Replace('\\', '/');
                if (dependency == sourceShaderPath
                    || !dependency.StartsWith("Assets/_Imported/", StringComparison.Ordinal)
                    || !IsPromotableHovlShaderDependency(dependency))
                {
                    continue;
                }

                string promotedDependency = EnsurePromotedHovlSciFiShaderFile(dependency);
                string sourceGuid = AssetDatabase.AssetPathToGUID(dependency);
                string promotedGuid = AssetDatabase.AssetPathToGUID(promotedDependency);
                if (string.IsNullOrWhiteSpace(sourceGuid)
                    || string.IsNullOrWhiteSpace(promotedGuid)
                    || sourceGuid == promotedGuid)
                {
                    continue;
                }

                string rewritten = contents.Replace(sourceGuid, promotedGuid);
                if (!string.Equals(rewritten, contents, StringComparison.Ordinal))
                {
                    contents = rewritten;
                    changed = true;
                }
            }

            if (changed)
            {
                File.WriteAllText(targetAbsolutePath, contents);
            }
        }

        private static bool IsPromotableHovlShaderDependency(string assetPath)
        {
            string extension = Path.GetExtension(assetPath).ToLowerInvariant();
            return extension == ".hlsl"
                || extension == ".cginc"
                || extension == ".shader"
                || extension == ".shadergraph"
                || extension == ".compute";
        }

        private static string EnsurePromotedHovlSciFiShaderFile(string sourcePath)
        {
            string targetPath = HovlSciFiEffectsShaderRoot + "/DB_HovlSciFi_"
                + SanitizeAssetFileName(Path.GetFileName(sourcePath));
            EnsureFolder(Path.GetDirectoryName(targetPath).Replace('\\', '/'));
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(targetPath) == null)
            {
                if (!AssetDatabase.CopyAsset(sourcePath, targetPath))
                {
                    throw new InvalidOperationException(
                        $"Failed to promote Hovl Sci-fi shader dependency from {sourcePath} to {targetPath}.");
                }
            }

            AssetDatabase.ImportAsset(targetPath, ImportAssetOptions.ForceUpdate);
            return targetPath;
        }

        private static Texture EnsurePromotedHovlSciFiTexture(Texture sourceTexture)
        {
            string sourcePath = AssetDatabase.GetAssetPath(sourceTexture).Replace('\\', '/');
            if (sourcePath.StartsWith("Assets/_Game/", StringComparison.Ordinal)
                || string.IsNullOrWhiteSpace(sourcePath)
                || !sourcePath.StartsWith("Assets/", StringComparison.Ordinal))
            {
                return sourceTexture;
            }

            string targetPath = HovlSciFiEffectsTextureRoot + "/DB_HovlSciFi_"
                + SanitizeAssetFileName(Path.GetFileName(sourcePath));
            EnsureFolder(Path.GetDirectoryName(targetPath).Replace('\\', '/'));
            if (AssetDatabase.LoadAssetAtPath<Texture>(targetPath) == null)
            {
                if (!AssetDatabase.CopyAsset(sourcePath, targetPath))
                {
                    throw new InvalidOperationException($"Failed to promote Hovl Sci-fi texture from {sourcePath} to {targetPath}.");
                }
            }

            AssetDatabase.ImportAsset(targetPath, ImportAssetOptions.ForceUpdate);
            Texture texture = AssetDatabase.LoadAssetAtPath<Texture>(targetPath);
            if (texture == null)
            {
                throw new InvalidOperationException($"Failed to load promoted Hovl Sci-fi texture at {targetPath}.");
            }

            return texture;
        }

        private static Mesh EnsurePromotedHovlSciFiMesh(Mesh sourceMesh)
        {
            string sourcePath = AssetDatabase.GetAssetPath(sourceMesh).Replace('\\', '/');
            if (sourcePath.StartsWith("Assets/_Game/", StringComparison.Ordinal))
            {
                return sourceMesh;
            }

            string sourceName = string.IsNullOrWhiteSpace(sourceMesh.name) ? "BuiltinParticleMesh" : sourceMesh.name;
            string targetPath = HovlSciFiEffectsMeshRoot + "/DB_HovlSciFi_"
                + SanitizeAssetFileName(sourceName)
                + ".asset";
            EnsureFolder(Path.GetDirectoryName(targetPath).Replace('\\', '/'));
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(targetPath);
            if (mesh != null)
            {
                return mesh;
            }

            mesh = UnityEngine.Object.Instantiate(sourceMesh);
            mesh.name = sourceMesh.name;
            AssetDatabase.CreateAsset(mesh, targetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(targetPath, ImportAssetOptions.ForceUpdate);

            Mesh promotedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(targetPath);
            if (promotedMesh == null)
            {
                throw new InvalidOperationException($"Failed to load promoted Hovl Sci-fi mesh at {targetPath}.");
            }

            return promotedMesh;
        }

        private static void RemapSerializedHovlSciFiTextures(Material material)
        {
            SerializedObject serializedMaterial = new SerializedObject(material);
            SerializedProperty texEnvs = serializedMaterial.FindProperty("m_SavedProperties.m_TexEnvs");
            if (texEnvs == null || !texEnvs.isArray)
            {
                return;
            }

            for (int i = 0; i < texEnvs.arraySize; i++)
            {
                SerializedProperty entry = texEnvs.GetArrayElementAtIndex(i);
                SerializedProperty textureRef = entry.FindPropertyRelative("second.m_Texture");
                if (textureRef == null || textureRef.objectReferenceValue is not Texture texture)
                {
                    continue;
                }

                string texturePath = AssetDatabase.GetAssetPath(texture).Replace('\\', '/');
                if (texturePath.StartsWith("Assets/_Imported/", StringComparison.Ordinal))
                {
                    textureRef.objectReferenceValue = EnsurePromotedHovlSciFiTexture(texture);
                }
            }

            serializedMaterial.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(material);
        }

        private static void ValidatePerfectDodgeHovlShieldPrefab(GameObject prefab)
        {
            if (prefab == null)
            {
                throw new InvalidOperationException("Perfect dodge window prefab should be assigned.");
            }

            Transform shieldRoot = prefab.transform.Find(PerfectDodgeHovlHexShieldChildName);
            if (shieldRoot == null)
            {
                throw new InvalidOperationException("Perfect dodge window should include the promoted Hovl Hex shield overlay.");
            }

            if (shieldRoot.GetComponentInChildren<Collider>(includeInactive: true) != null
                || shieldRoot.GetComponentInChildren<Rigidbody>(includeInactive: true) != null
                || shieldRoot.GetComponentInChildren<AudioSource>(includeInactive: true) != null)
            {
                throw new InvalidOperationException("Perfect dodge Hovl shield must remain visual-only.");
            }

            ParticleSystem[] particleSystems =
                shieldRoot.GetComponentsInChildren<ParticleSystem>(includeInactive: true);
            if (particleSystems.Length < 4)
            {
                throw new InvalidOperationException("Perfect dodge Hovl shield should preserve its authored particle stack.");
            }

            Renderer[] renderers = shieldRoot.GetComponentsInChildren<Renderer>(includeInactive: true);
            if (renderers.Length == 0)
            {
                throw new InvalidOperationException("Perfect dodge Hovl shield should expose promoted renderers.");
            }
        }

        private static GameObject SavePerfectDodgeTimeFieldPrefab(
            string name,
            Material cyanMaterial,
            Material blueMaterial,
            Material whiteMaterial,
            Material shapesPortalOuterMaterial,
            Material shapesPortalInnerMaterial,
            Mesh shapesTorusMesh)
        {
            string prefabPath = $"{PrefabRoot}/{name}.prefab";
            GameObject root = new GameObject(name);
            var renderers = new System.Collections.Generic.List<Renderer>
            {
                AddMesh(
                    root.transform,
                    "PerfectDodge_TimeField_ShapesFxOuterTorus",
                    shapesTorusMesh,
                    shapesPortalOuterMaterial,
                    new Vector3(0f, 0.052f, 0f),
                    new Vector3(90f, 0f, 0f),
                    new Vector3(0.82f, 0.82f, 0.82f)),
                AddMesh(
                    root.transform,
                    "PerfectDodge_TimeField_ShapesFxInnerTorus",
                    shapesTorusMesh,
                    shapesPortalInnerMaterial,
                    new Vector3(0f, 0.074f, 0f),
                    new Vector3(90f, 35f, 0f),
                    new Vector3(0.48f, 0.48f, 0.48f)),
                AddPrimitive(
                    root.transform,
                    "PerfectDodge_TimeField_OuterDisc",
                    PrimitiveType.Cylinder,
                    cyanMaterial,
                    Vector3.zero,
                    Vector3.zero,
                    new Vector3(1.08f, 0.014f, 1.08f)),
                AddPrimitive(
                    root.transform,
                    "PerfectDodge_TimeField_InnerDisc",
                    PrimitiveType.Cylinder,
                    blueMaterial,
                    new Vector3(0f, 0.018f, 0f),
                    Vector3.zero,
                    new Vector3(0.62f, 0.012f, 0.62f)),
                AddPrimitive(
                    root.transform,
                    "PerfectDodge_TimeField_CrossA",
                    PrimitiveType.Cube,
                    cyanMaterial,
                    new Vector3(0f, 0.036f, 0f),
                    Vector3.zero,
                    new Vector3(1.35f, 0.018f, 0.035f)),
                AddPrimitive(
                    root.transform,
                    "PerfectDodge_TimeField_CrossB",
                    PrimitiveType.Cube,
                    whiteMaterial,
                    new Vector3(0f, 0.04f, 0f),
                    new Vector3(0f, 90f, 0f),
                    new Vector3(1.35f, 0.018f, 0.035f)),
                AddPrimitive(
                    root.transform,
                    "PerfectDodge_TimeField_VerticalGlintA",
                    PrimitiveType.Cube,
                    whiteMaterial,
                    new Vector3(0.46f, 0.38f, 0.08f),
                    new Vector3(0f, 14f, 0f),
                    new Vector3(0.028f, 0.58f, 0.028f)),
                AddPrimitive(
                    root.transform,
                    "PerfectDodge_TimeField_VerticalGlintB",
                    PrimitiveType.Cube,
                    whiteMaterial,
                    new Vector3(-0.38f, 0.3f, -0.16f),
                    new Vector3(0f, -21f, 0f),
                    new Vector3(0.024f, 0.46f, 0.024f))
            };

            AddParticleBurst(
                root.transform,
                "PerfectDodge_TimeField_SparkShell",
                cyanMaterial,
                new Vector3(0f, 0.22f, 0f),
                Vector3.zero,
                3.0f,
                1.85f,
                0.28f,
                0.08f,
                84,
                ParticleSystemShapeType.Sphere,
                0.72f,
                360f,
                new Color(0.34f, 0.96f, 1f, 0.62f));
            AddParticleBurst(
                root.transform,
                "PerfectDodge_TimeField_MicroTicks",
                whiteMaterial,
                new Vector3(0f, 0.08f, 0f),
                Vector3.zero,
                2.8f,
                1.6f,
                0.32f,
                0.05f,
                72,
                ParticleSystemShapeType.Circle,
                0.86f,
                360f,
                new Color(0.9f, 1f, 1f, 0.74f));

            CombatVfxCueVisual visual = root.AddComponent<CombatVfxCueVisual>();
            ConfigureCueVisual(
                visual,
                renderers.ToArray(),
                3.0f,
                new Color(0.36f, 0.98f, 1f, 0.78f),
                new Color(0.08f, 0.18f, 0.86f, 0f),
                new Vector3(0.92f, 0.72f, 0.92f),
                new Vector3(3.25f, 0.92f, 3.25f),
                86f,
                0.02f,
                0f);

            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            return savedPrefab;
        }

        private static GameObject SavePerfectDodgePulsewavePrefab(
            string name,
            Material cyanMaterial,
            Material whiteMaterial,
            Material shapesPortalOuterMaterial,
            Mesh shapesTorusMesh)
        {
            string prefabPath = $"{PrefabRoot}/{name}.prefab";
            GameObject root = new GameObject(name);
            var renderers = new System.Collections.Generic.List<Renderer>
            {
                AddMesh(
                    root.transform,
                    "PerfectDodge_Pulsewave_ShapesFxShockTorus",
                    shapesTorusMesh,
                    shapesPortalOuterMaterial,
                    new Vector3(0f, 0.03f, 0f),
                    new Vector3(90f, 18f, 0f),
                    new Vector3(0.72f, 0.72f, 0.72f)),
                AddPrimitive(
                    root.transform,
                    "PerfectDodge_Pulsewave_Outer",
                    PrimitiveType.Cylinder,
                    cyanMaterial,
                    Vector3.zero,
                    Vector3.zero,
                    new Vector3(0.84f, 0.012f, 0.84f)),
                AddPrimitive(
                    root.transform,
                    "PerfectDodge_Pulsewave_Inner",
                    PrimitiveType.Cylinder,
                    whiteMaterial,
                    new Vector3(0f, 0.018f, 0f),
                    Vector3.zero,
                    new Vector3(0.38f, 0.01f, 0.38f)),
                AddPrimitive(
                    root.transform,
                    "PerfectDodge_Pulsewave_SlashA",
                    PrimitiveType.Cube,
                    cyanMaterial,
                    new Vector3(0f, 0.035f, 0f),
                    new Vector3(0f, 28f, 0f),
                    new Vector3(1.18f, 0.016f, 0.04f)),
                AddPrimitive(
                    root.transform,
                    "PerfectDodge_Pulsewave_SlashB",
                    PrimitiveType.Cube,
                    whiteMaterial,
                    new Vector3(0f, 0.04f, 0f),
                    new Vector3(0f, -34f, 0f),
                    new Vector3(1.05f, 0.014f, 0.032f))
            };

            AddParticleBurst(
                root.transform,
                "PerfectDodge_Pulsewave_EdgeSparks",
                whiteMaterial,
                new Vector3(0f, 0.05f, 0f),
                Vector3.zero,
                0.72f,
                0.18f,
                2.4f,
                0.04f,
                96,
                ParticleSystemShapeType.Circle,
                1.15f,
                360f,
                new Color(0.88f, 1f, 1f, 0.78f));

            CombatVfxCueVisual visual = root.AddComponent<CombatVfxCueVisual>();
            ConfigureCueVisual(
                visual,
                renderers.ToArray(),
                0.9f,
                new Color(0.56f, 1f, 1f, 0.72f),
                new Color(0.04f, 0.18f, 0.95f, 0f),
                new Vector3(0.64f, 0.58f, 0.64f),
                new Vector3(4.2f, 0.8f, 4.2f),
                240f,
                0f,
                0f);

            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            return savedPrefab;
        }

        private static GameObject SavePerfectDodgeHoloCubePrefab(
            string name,
            Material cyanMaterial,
            Material violetMaterial,
            Material whiteMaterial,
            Material shapesCyanDodecaMaterial,
            Material shapesVioletIcosaMaterial,
            Mesh shapesDodecaMesh,
            Mesh shapesIcosaMesh)
        {
            string prefabPath = $"{PrefabRoot}/{name}.prefab";
            GameObject root = new GameObject(name);
            var renderers = new System.Collections.Generic.List<Renderer>
            {
                AddMesh(
                    root.transform,
                    "PerfectDodge_HoloCube_ShapesFxCoreDodeca",
                    shapesDodecaMesh,
                    shapesCyanDodecaMaterial,
                    new Vector3(0f, 0.5f, 0f),
                    new Vector3(18f, 36f, 12f),
                    new Vector3(0.2f, 0.2f, 0.2f)),
                AddMesh(
                    root.transform,
                    "PerfectDodge_HoloCube_ShapesFxLeftIcosa",
                    shapesIcosaMesh,
                    shapesVioletIcosaMaterial,
                    new Vector3(-0.32f, 0.58f, -0.1f),
                    new Vector3(-24f, 28f, -18f),
                    new Vector3(0.12f, 0.12f, 0.12f)),
                AddMesh(
                    root.transform,
                    "PerfectDodge_HoloCube_ShapesFxRightIcosa",
                    shapesIcosaMesh,
                    shapesVioletIcosaMaterial,
                    new Vector3(0.34f, 0.72f, 0.08f),
                    new Vector3(22f, -34f, 26f),
                    new Vector3(0.105f, 0.105f, 0.105f)),
                AddPrimitive(
                    root.transform,
                    "PerfectDodge_HoloCube_Core",
                    PrimitiveType.Cube,
                    cyanMaterial,
                    new Vector3(0f, 0.42f, 0f),
                    new Vector3(21f, 34f, 12f),
                    new Vector3(0.24f, 0.24f, 0.24f)),
                AddPrimitive(
                    root.transform,
                    "PerfectDodge_HoloCube_FragmentA",
                    PrimitiveType.Cube,
                    whiteMaterial,
                    new Vector3(0.34f, 0.68f, 0.04f),
                    new Vector3(12f, -22f, 38f),
                    new Vector3(0.12f, 0.12f, 0.12f)),
                AddPrimitive(
                    root.transform,
                    "PerfectDodge_HoloCube_FragmentB",
                    PrimitiveType.Cube,
                    violetMaterial,
                    new Vector3(-0.28f, 0.55f, -0.08f),
                    new Vector3(-18f, 35f, -24f),
                    new Vector3(0.10f, 0.10f, 0.10f)),
                AddPrimitive(
                    root.transform,
                    "PerfectDodge_HoloCube_FragmentC",
                    PrimitiveType.Cube,
                    whiteMaterial,
                    new Vector3(0.06f, 0.78f, -0.28f),
                    new Vector3(36f, 12f, -18f),
                    new Vector3(0.08f, 0.08f, 0.08f)),
                AddPrimitive(
                    root.transform,
                    "PerfectDodge_HoloCube_FragmentD",
                    PrimitiveType.Cube,
                    cyanMaterial,
                    new Vector3(-0.1f, 0.34f, 0.26f),
                    new Vector3(-34f, -12f, 27f),
                    new Vector3(0.09f, 0.09f, 0.09f))
            };

            AddParticleBurst(
                root.transform,
                "PerfectDodge_HoloCube_Glints",
                whiteMaterial,
                new Vector3(0f, 0.54f, 0f),
                Vector3.zero,
                0.38f,
                0.22f,
                0.62f,
                0.045f,
                34,
                ParticleSystemShapeType.Sphere,
                0.34f,
                360f,
                new Color(0.88f, 1f, 1f, 0.8f));

            CombatVfxCueVisual visual = root.AddComponent<CombatVfxCueVisual>();
            ConfigureCueVisual(
                visual,
                renderers.ToArray(),
                1.2f,
                new Color(0.58f, 1f, 1f, 0.86f),
                new Color(0.62f, 0.28f, 1f, 0f),
                new Vector3(0.62f, 0.62f, 0.62f),
                new Vector3(1.85f, 1.85f, 1.85f),
                180f,
                0.12f,
                0f);

            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            return savedPrefab;
        }

        private static GameObject SaveSummonPreSpawnPortalPrefab(
            string name,
            Material cyanMaterial,
            Material blueMaterial,
            Material whiteMaterial,
            Material shapesPortalOuterMaterial,
            Material shapesPortalInnerMaterial,
            Mesh shapesTorusMesh)
        {
            return SavePromotedHovlSciFiCuePrefab(
                name,
                ImportedHovlLaserTeleportPrefabPath,
                Vector3.zero,
                Vector3.zero,
                Vector3.one,
                loopParticles: false,
                playOnAwake: true,
                lifetimeSeconds: 0.62f,
                minimumParticleSystems: 4);
        }

        private static GameObject SaveSummonLandingCraterPrefab(
            string name,
            Material impactMaterial,
            Material smokeMaterial,
            Material shapesPortalOuterMaterial,
            Mesh shapesTorusMesh)
        {
            string prefabPath = $"{PrefabRoot}/{name}.prefab";
            GameObject root = new GameObject(name);
            var renderers = new System.Collections.Generic.List<Renderer>
            {
                AddMesh(
                    root.transform,
                    "SummonLanding_ShapesFxShockRing",
                    shapesTorusMesh,
                    shapesPortalOuterMaterial,
                    new Vector3(0f, 0.045f, 0f),
                    new Vector3(90f, 18f, 0f),
                    new Vector3(0.74f, 0.74f, 0.74f)),
                AddPrimitive(
                    root.transform,
                    "SummonLanding_CraterDisc",
                    PrimitiveType.Cylinder,
                    smokeMaterial,
                    new Vector3(0f, 0.015f, 0f),
                    Vector3.zero,
                    new Vector3(0.72f, 0.01f, 0.72f)),
                AddPrimitive(
                    root.transform,
                    "SummonLanding_CrackA",
                    PrimitiveType.Cube,
                    impactMaterial,
                    new Vector3(0f, 0.04f, 0f),
                    new Vector3(0f, 18f, 0f),
                    new Vector3(1.18f, 0.018f, 0.035f)),
                AddPrimitive(
                    root.transform,
                    "SummonLanding_CrackB",
                    PrimitiveType.Cube,
                    impactMaterial,
                    new Vector3(0f, 0.045f, 0f),
                    new Vector3(0f, -34f, 0f),
                    new Vector3(0.92f, 0.016f, 0.03f)),
                AddPrimitive(
                    root.transform,
                    "SummonLanding_CoreKick",
                    PrimitiveType.Sphere,
                    impactMaterial,
                    new Vector3(0f, 0.12f, 0f),
                    Vector3.zero,
                    new Vector3(0.24f, 0.08f, 0.24f))
            };

            AddParticleBurst(
                root.transform,
                "SummonLanding_DustBurst",
                smokeMaterial,
                new Vector3(0f, 0.08f, 0f),
                Vector3.zero,
                0.72f,
                0.42f,
                1.05f,
                0.22f,
                42,
                ParticleSystemShapeType.Circle,
                0.52f,
                360f,
                new Color(0.62f, 0.66f, 0.68f, 0.44f));
            AddParticleBurst(
                root.transform,
                "SummonLanding_StoneSparks",
                impactMaterial,
                new Vector3(0f, 0.1f, 0f),
                Vector3.zero,
                0.48f,
                0.32f,
                1.9f,
                0.06f,
                28,
                ParticleSystemShapeType.Circle,
                0.38f,
                360f,
                new Color(1f, 0.82f, 0.36f, 0.76f));

            CombatVfxCueVisual visual = root.AddComponent<CombatVfxCueVisual>();
            ConfigureCueVisual(
                visual,
                renderers.ToArray(),
                0.84f,
                new Color(1f, 0.82f, 0.36f, 0.78f),
                new Color(0.28f, 0.24f, 0.20f, 0f),
                new Vector3(0.72f, 0.42f, 0.72f),
                new Vector3(2.25f, 0.65f, 2.25f),
                96f,
                0.02f,
                0f);

            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            return savedPrefab;
        }

        private static GameObject SaveAudioOnlyCuePrefab(string name)
        {
            string prefabPath = $"{PrefabRoot}/{name}.prefab";
            GameObject root = new GameObject(name);
            CombatVfxCueVisual visual = root.AddComponent<CombatVfxCueVisual>();
            ConfigureCueVisual(
                visual,
                Array.Empty<Renderer>(),
                0.05f,
                Color.clear,
                Color.clear,
                Vector3.one,
                Vector3.one,
                0f,
                0f,
                0f);

            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            return savedPrefab;
        }

        private static GameObject SavePromotedHitFeedbackPrefab(
            string name,
            string sourcePrefabPath,
            float lifetimeSeconds)
        {
            string prefabPath = $"{PrefabRoot}/{name}.prefab";
            GameObject sourcePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(sourcePrefabPath);
            if (sourcePrefab == null)
            {
                throw new InvalidOperationException($"Missing source hit feedback VFX prefab at {sourcePrefabPath}.");
            }

            GameObject root = PrefabUtility.InstantiatePrefab(sourcePrefab) as GameObject;
            if (root == null)
            {
                root = UnityEngine.Object.Instantiate(sourcePrefab);
            }

            try
            {
                if (PrefabUtility.IsPartOfPrefabInstance(root))
                {
                    PrefabUtility.UnpackPrefabInstance(
                        root,
                        PrefabUnpackMode.Completely,
                        InteractionMode.AutomatedAction);
                }

                root.name = name;
                root.transform.localPosition = Vector3.zero;
                root.transform.localRotation = Quaternion.identity;
                root.transform.localScale = Vector3.one;
                UnpackNestedPrefabInstances(root);
                StripNonGameMonoBehaviours(root);
                RemoveColliders(root);
                DisableVfxAudioSources(root);
                ConfigurePromotedHitFeedbackParticles(root);
                DisablePromotedParticleLightModules(root);
                RemapPromotedHitFeedbackMeshes(root);
                RemapPromotedHitFeedbackRenderers(root);

                Renderer[] renderers = root.GetComponentsInChildren<Renderer>(includeInactive: true);
                CombatVfxCueVisual visual = root.GetComponent<CombatVfxCueVisual>();
                if (visual == null)
                {
                    visual = root.AddComponent<CombatVfxCueVisual>();
                }

                ConfigureCueVisual(
                    visual,
                    renderers,
                    lifetimeSeconds,
                    Color.white,
                    Color.white,
                    Vector3.one,
                    Vector3.one,
                    0f,
                    0f,
                    0f);

                GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                if (savedPrefab == null)
                {
                    throw new InvalidOperationException($"Failed to save promoted hit feedback VFX prefab at {prefabPath}.");
                }

                AssetDatabase.ImportAsset(prefabPath, ImportAssetOptions.ForceUpdate);
                ValidateNoImportedAssetDependencies(CombatVfxCueId.EnemyHit, prefabPath);
                return savedPrefab;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static void ConfigurePromotedHitFeedbackParticles(GameObject root)
        {
            ParticleSystem[] particleSystems = root.GetComponentsInChildren<ParticleSystem>(includeInactive: true);
            if (particleSystems.Length == 0)
            {
                throw new InvalidOperationException($"{root.name} should preserve authored hit feedback particles.");
            }

            for (int i = 0; i < particleSystems.Length; i++)
            {
                ParticleSystem particleSystem = particleSystems[i];
                ParticleSystem.MainModule main = particleSystem.main;
                main.loop = false;
                main.playOnAwake = false;
                main.simulationSpace = ParticleSystemSimulationSpace.Local;
                main.scalingMode = ParticleSystemScalingMode.Hierarchy;

                ParticleSystem.EmissionModule emission = particleSystem.emission;
                emission.enabled = true;
                particleSystem.Clear(withChildren: true);
                EditorUtility.SetDirty(particleSystem);
            }
        }

        private static void RemapPromotedHitFeedbackRenderers(GameObject root)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(includeInactive: true);
            Material fallbackMaterial = null;
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                Material[] materials = renderer.sharedMaterials;
                for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                {
                    if (materials[materialIndex] != null)
                    {
                        materials[materialIndex] = EnsurePromotedHitFeedbackMaterial(materials[materialIndex]);
                        fallbackMaterial ??= materials[materialIndex];
                    }
                }

                renderer.sharedMaterials = materials;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                renderer.allowOcclusionWhenDynamic = false;
                EditorUtility.SetDirty(renderer);
            }

            if (fallbackMaterial == null)
            {
                throw new InvalidOperationException($"{root.name} should expose at least one promoted hit feedback material.");
            }

            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
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

        private static Material EnsurePromotedHitFeedbackMaterial(Material sourceMaterial)
        {
            string sourcePath = AssetDatabase.GetAssetPath(sourceMaterial).Replace('\\', '/');
            if (sourcePath.StartsWith("Assets/_Game/", StringComparison.Ordinal))
            {
                return sourceMaterial;
            }

            string targetPath = MaterialRoot + "/DB_CombatHit_"
                + SanitizeAssetFileName(sourceMaterial.name)
                + ".mat";
            EnsureFolder(Path.GetDirectoryName(targetPath).Replace('\\', '/'));

            Material material = AssetDatabase.LoadAssetAtPath<Material>(targetPath);
            if (material == null)
            {
                material = new Material(EnsurePromotedHitFeedbackShader(sourceMaterial.shader));
                AssetDatabase.CreateAsset(material, targetPath);
            }

            Shader promotedShader = EnsurePromotedHitFeedbackShader(sourceMaterial.shader);
            material.shader = promotedShader;
            material.CopyPropertiesFromMaterial(sourceMaterial);
            material.shader = promotedShader;
            material.renderQueue = sourceMaterial.renderQueue;

            string[] textureProperties = sourceMaterial.GetTexturePropertyNames();
            for (int i = 0; i < textureProperties.Length; i++)
            {
                Texture texture = sourceMaterial.GetTexture(textureProperties[i]);
                if (texture == null)
                {
                    continue;
                }

                Texture promotedTexture = EnsurePromotedHitFeedbackTexture(texture);
                SetMaterialTextureIfPresent(material, textureProperties[i], promotedTexture);
            }

            RemapImportedSerializedHitFeedbackTextures(material);
            ReplacePromotedHitFeedbackCoreTexture(material);
            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(targetPath, ImportAssetOptions.ForceUpdate);
            return material;
        }

        private static void RemapImportedSerializedHitFeedbackTextures(Material material)
        {
            SerializedObject serializedMaterial = new SerializedObject(material);
            SerializedProperty texEnvs = serializedMaterial.FindProperty("m_SavedProperties.m_TexEnvs");
            if (texEnvs == null || !texEnvs.isArray)
            {
                return;
            }

            for (int i = 0; i < texEnvs.arraySize; i++)
            {
                SerializedProperty entry = texEnvs.GetArrayElementAtIndex(i);
                SerializedProperty propertyName = entry.FindPropertyRelative("first");
                SerializedProperty textureRef = entry.FindPropertyRelative("second.m_Texture");
                if (textureRef != null
                    && propertyName != null
                    && propertyName.stringValue == "_disolveMap")
                {
                    Texture promotedNoise =
                        AssetDatabase.LoadAssetAtPath<Texture>(TextureRoot + "/DB_CombatHit_T_VFX_Noise_Perlin_01.tga");
                    if (promotedNoise != null)
                    {
                        textureRef.objectReferenceValue = promotedNoise;
                    }

                    continue;
                }

                if (textureRef == null || textureRef.objectReferenceValue is not Texture texture)
                {
                    continue;
                }

                string texturePath = AssetDatabase.GetAssetPath(texture).Replace('\\', '/');
                if (!texturePath.StartsWith("Assets/_Imported/", StringComparison.Ordinal))
                {
                    continue;
                }

                textureRef.objectReferenceValue = EnsurePromotedHitFeedbackTexture(texture);
            }

            serializedMaterial.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(material);
        }

        private static Shader EnsurePromotedHitFeedbackShader(Shader sourceShader)
        {
            if (sourceShader == null)
            {
                return FindParticleShader();
            }

            string sourcePath = AssetDatabase.GetAssetPath(sourceShader).Replace('\\', '/');
            if (sourcePath.StartsWith("Assets/_Game/", StringComparison.Ordinal))
            {
                return sourceShader;
            }

            if (string.IsNullOrWhiteSpace(sourcePath) || !sourcePath.StartsWith("Assets/", StringComparison.Ordinal))
            {
                return sourceShader;
            }

            string targetPath = ShaderRoot + "/DB_CombatHit_"
                + SanitizeAssetFileName(Path.GetFileName(sourcePath));
            EnsureFolder(Path.GetDirectoryName(targetPath).Replace('\\', '/'));
            if (AssetDatabase.LoadAssetAtPath<Shader>(targetPath) == null)
            {
                if (!AssetDatabase.CopyAsset(sourcePath, targetPath))
                {
                    throw new InvalidOperationException($"Failed to promote hit feedback shader from {sourcePath} to {targetPath}.");
                }
            }

            RemapPromotedShaderDefaultTextures(sourcePath, targetPath);
            AssetDatabase.ImportAsset(targetPath, ImportAssetOptions.ForceUpdate);
            Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(targetPath);
            if (shader == null)
            {
                throw new InvalidOperationException($"Failed to load promoted hit feedback shader at {targetPath}.");
            }

            return shader;
        }

        private static void RemapPromotedShaderDefaultTextures(string sourceShaderPath, string targetShaderPath)
        {
            string sourceMetaPath = ToProjectAbsolutePath(sourceShaderPath + ".meta");
            string targetMetaPath = ToProjectAbsolutePath(targetShaderPath + ".meta");
            if (!File.Exists(sourceMetaPath) || !File.Exists(targetMetaPath))
            {
                return;
            }

            System.Collections.Generic.List<string> defaultTextureLines =
                ExtractPromotedShaderDefaultTextureLines(File.ReadAllLines(sourceMetaPath));
            if (defaultTextureLines.Count == 0)
            {
                return;
            }

            for (int i = 0; i < defaultTextureLines.Count; i++)
            {
                defaultTextureLines[i] = RemapSerializedTextureGuidsToPromotedAssets(defaultTextureLines[i]);
            }

            string[] lines = File.ReadAllLines(targetMetaPath);
            var rewritten = new System.Collections.Generic.List<string>(lines.Length + defaultTextureLines.Count);
            bool skippingDefaultTextures = false;
            bool changed = false;
            bool wroteDefaultTextures = false;
            for (int i = 0; i < lines.Length; i++)
            {
                string trimmed = lines[i].TrimStart();
                if (!skippingDefaultTextures && (trimmed == "defaultTextures:" || trimmed == "defaultTextures: []"))
                {
                    rewritten.AddRange(defaultTextureLines);
                    skippingDefaultTextures = trimmed == "defaultTextures:";
                    changed = true;
                    wroteDefaultTextures = true;
                    continue;
                }

                if (skippingDefaultTextures)
                {
                    if (trimmed.StartsWith("nonModifiableTextures:", StringComparison.Ordinal))
                    {
                        rewritten.Add(lines[i]);
                        skippingDefaultTextures = false;
                    }

                    continue;
                }

                if (!wroteDefaultTextures && trimmed.StartsWith("nonModifiableTextures:", StringComparison.Ordinal))
                {
                    rewritten.AddRange(defaultTextureLines);
                    wroteDefaultTextures = true;
                    changed = true;
                }

                rewritten.Add(lines[i]);
            }

            if (!wroteDefaultTextures)
            {
                rewritten.AddRange(defaultTextureLines);
                changed = true;
            }

            if (changed)
            {
                File.WriteAllLines(targetMetaPath, rewritten);
            }
        }

        private static System.Collections.Generic.List<string> ExtractPromotedShaderDefaultTextureLines(string[] sourceMetaLines)
        {
            var defaultTextureLines = new System.Collections.Generic.List<string>();
            bool copying = false;
            for (int i = 0; i < sourceMetaLines.Length; i++)
            {
                string trimmed = sourceMetaLines[i].TrimStart();
                if (!copying && trimmed == "defaultTextures:")
                {
                    copying = true;
                    defaultTextureLines.Add(sourceMetaLines[i]);
                    continue;
                }

                if (!copying)
                {
                    continue;
                }

                if (trimmed.StartsWith("nonModifiableTextures:", StringComparison.Ordinal))
                {
                    break;
                }

                defaultTextureLines.Add(sourceMetaLines[i]);
            }

            return defaultTextureLines;
        }

        private static string RemapSerializedTextureGuidsToPromotedAssets(string line)
        {
            return System.Text.RegularExpressions.Regex.Replace(
                line,
                "guid: ([0-9a-fA-F]{32})",
                match =>
                {
                    string sourceAssetPath = AssetDatabase.GUIDToAssetPath(match.Groups[1].Value).Replace('\\', '/');
                    Texture texture = AssetDatabase.LoadAssetAtPath<Texture>(sourceAssetPath);
                    if (texture == null)
                    {
                        return match.Value;
                    }

                    Texture promotedTexture = EnsurePromotedHitFeedbackTexture(texture);
                    string promotedPath = AssetDatabase.GetAssetPath(promotedTexture).Replace('\\', '/');
                    string promotedGuid = AssetDatabase.AssetPathToGUID(promotedPath);
                    return string.IsNullOrWhiteSpace(promotedGuid) ? match.Value : "guid: " + promotedGuid;
                });
        }

        private static Texture EnsurePromotedHitFeedbackTexture(Texture sourceTexture)
        {
            string sourcePath = AssetDatabase.GetAssetPath(sourceTexture).Replace('\\', '/');
            if (sourcePath.StartsWith("Assets/_Game/", StringComparison.Ordinal)
                || string.IsNullOrWhiteSpace(sourcePath)
                || !sourcePath.StartsWith("Assets/", StringComparison.Ordinal))
            {
                return sourceTexture;
            }

            string targetPath = TextureRoot + "/DB_CombatHit_"
                + SanitizeAssetFileName(Path.GetFileName(sourcePath));
            EnsureFolder(Path.GetDirectoryName(targetPath).Replace('\\', '/'));
            if (AssetDatabase.LoadAssetAtPath<Texture>(targetPath) == null)
            {
                if (!AssetDatabase.CopyAsset(sourcePath, targetPath))
                {
                    throw new InvalidOperationException($"Failed to promote hit feedback texture from {sourcePath} to {targetPath}.");
                }
            }

            AssetDatabase.ImportAsset(targetPath, ImportAssetOptions.ForceUpdate);
            Texture texture = AssetDatabase.LoadAssetAtPath<Texture>(targetPath);
            if (texture == null)
            {
                throw new InvalidOperationException($"Failed to load promoted hit feedback texture at {targetPath}.");
            }

            return texture;
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
                if (!scriptPath.StartsWith("Assets/_Game/", StringComparison.Ordinal))
                {
                    UnityEngine.Object.DestroyImmediate(behaviour);
                }
            }
        }

        private static void RemoveColliders(GameObject root)
        {
            Collider[] colliders = root.GetComponentsInChildren<Collider>(includeInactive: true);
            for (int i = colliders.Length - 1; i >= 0; i--)
            {
                UnityEngine.Object.DestroyImmediate(colliders[i]);
            }
        }

        private static void DisableVfxAudioSources(GameObject root)
        {
            AudioSource[] audioSources = root.GetComponentsInChildren<AudioSource>(includeInactive: true);
            for (int i = audioSources.Length - 1; i >= 0; i--)
            {
                UnityEngine.Object.DestroyImmediate(audioSources[i]);
            }
        }

        private static GameObject SaveMuzzleFlashPrefab(
            string name,
            Material frontMaterial,
            Material sideMaterial,
            Material smokeMaterial)
        {
            string prefabPath = $"{PrefabRoot}/{name}.prefab";
            GameObject root = new GameObject(name);
            _ = frontMaterial;
            _ = sideMaterial;
            _ = smokeMaterial;

            GameObject promotedMuzzleFlash = AttachPromotedMuzzleFlashAsset(root.transform);

            AttachReviewedGunshotAudio(root);

            CombatVfxCueVisual visual = root.AddComponent<CombatVfxCueVisual>();
            ConfigureCueVisual(
                visual,
                CollectMuzzleFlashCueVisualRenderers(promotedMuzzleFlash),
                0.20f,
                new Color(1f, 0.88f, 0.44f, 0.82f),
                new Color(1f, 0.12f, 0.02f, 0f),
                new Vector3(0.9f, 0.9f, 0.9f),
                new Vector3(1.08f, 1.08f, 1.08f),
                0f,
                0f,
                0.04f);

            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            return savedPrefab;
        }

        private static Renderer[] CollectMuzzleFlashCueVisualRenderers(GameObject promotedMuzzleFlash)
        {
            Renderer[] renderers = promotedMuzzleFlash.GetComponentsInChildren<Renderer>(includeInactive: true);
            var flashRenderers = new System.Collections.Generic.List<Renderer>(renderers.Length);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || IsMuzzleSmokeRenderer(renderer))
                {
                    continue;
                }

                flashRenderers.Add(renderer);
            }

            return flashRenderers.ToArray();
        }

        private static bool IsMuzzleSmokeRenderer(Renderer renderer)
        {
            Transform current = renderer.transform;
            while (current != null)
            {
                if (string.Equals(current.name, "Smoke Front", StringComparison.Ordinal)
                    || string.Equals(current.name, "Smoke Side", StringComparison.Ordinal))
                {
                    return true;
                }

                current = current.parent;
            }

            return false;
        }

        private static GameObject AttachPromotedMuzzleFlashAsset(Transform parent)
        {
            const string ChildName = "MuzzleFlash_VefectsRifleDemo";
            Transform existing = parent.Find(ChildName);
            if (existing != null)
            {
                UnityEngine.Object.DestroyImmediate(existing.gameObject);
            }

            GameObject sourcePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ImportedRifleMuzzleFlashPrefabPath);
            if (sourcePrefab == null)
            {
                throw new InvalidOperationException($"Missing source rifle muzzle flash VFX prefab at {ImportedRifleMuzzleFlashPrefabPath}.");
            }

            GameObject assetRoot = PrefabUtility.InstantiatePrefab(sourcePrefab) as GameObject;
            if (assetRoot == null)
            {
                assetRoot = UnityEngine.Object.Instantiate(sourcePrefab);
            }

            if (PrefabUtility.IsPartOfPrefabInstance(assetRoot))
            {
                PrefabUtility.UnpackPrefabInstance(
                    assetRoot,
                    PrefabUnpackMode.Completely,
                    InteractionMode.AutomatedAction);
            }

            assetRoot.name = ChildName;
            assetRoot.transform.SetParent(parent, worldPositionStays: false);
            assetRoot.transform.localPosition = new Vector3(0f, 0f, 0.08f);
            assetRoot.transform.localRotation = Quaternion.identity;
            assetRoot.transform.localScale = new Vector3(0.78f, 0.78f, 0.9f);

            UnpackNestedPrefabInstances(assetRoot);
            StripNonGameMonoBehaviours(assetRoot);
            RemoveColliders(assetRoot);
            DisableVfxAudioSources(assetRoot);
            ConfigurePromotedMuzzleFlashParticles(assetRoot);
            ConfigurePromotedMuzzleSmokeParticles(assetRoot);
            DisablePromotedParticleLightModules(assetRoot);
            AttachLocalMuzzleFlashLight(assetRoot.transform);
            RemapPromotedHitFeedbackMeshes(assetRoot);
            RemapPromotedHitFeedbackRenderers(assetRoot);
            EditorUtility.SetDirty(assetRoot);
            return assetRoot;
        }

        private static void DisablePromotedParticleLightModules(GameObject root)
        {
            ParticleSystem[] particleSystems = root.GetComponentsInChildren<ParticleSystem>(includeInactive: true);
            for (int i = 0; i < particleSystems.Length; i++)
            {
                ParticleSystem.LightsModule lights = particleSystems[i].lights;
                if (!lights.enabled && lights.light == null)
                {
                    continue;
                }

                lights.enabled = false;
                lights.light = null;
                EditorUtility.SetDirty(particleSystems[i]);
            }
        }

        private static void AttachLocalMuzzleFlashLight(Transform parent)
        {
            const string LightName = "MuzzleFlash_LocalBurstLight";
            Transform existing = parent.Find(LightName);
            if (existing != null)
            {
                UnityEngine.Object.DestroyImmediate(existing.gameObject);
            }

            GameObject lightObject = new GameObject(LightName);
            lightObject.transform.SetParent(parent, worldPositionStays: false);
            lightObject.transform.localPosition = new Vector3(0f, 0f, 0.28f);
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.64f, 0.22f, 1f);
            light.intensity = 4.2f;
            light.range = 2.8f;
            light.shadows = LightShadows.None;
            light.renderMode = LightRenderMode.ForcePixel;
            EditorUtility.SetDirty(lightObject);
            EditorUtility.SetDirty(light);
        }

        private static void RemapPromotedHitFeedbackMeshes(GameObject root)
        {
            MeshFilter[] meshFilters = root.GetComponentsInChildren<MeshFilter>(includeInactive: true);
            for (int i = 0; i < meshFilters.Length; i++)
            {
                MeshFilter meshFilter = meshFilters[i];
                if (meshFilter.sharedMesh == null)
                {
                    continue;
                }

                meshFilter.sharedMesh = EnsurePromotedHitFeedbackMesh(meshFilter.sharedMesh);
                EditorUtility.SetDirty(meshFilter);
            }

            SkinnedMeshRenderer[] skinnedRenderers =
                root.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true);
            for (int i = 0; i < skinnedRenderers.Length; i++)
            {
                SkinnedMeshRenderer skinnedRenderer = skinnedRenderers[i];
                if (skinnedRenderer.sharedMesh == null)
                {
                    continue;
                }

                skinnedRenderer.sharedMesh = EnsurePromotedHitFeedbackMesh(skinnedRenderer.sharedMesh);
                EditorUtility.SetDirty(skinnedRenderer);
            }

            ParticleSystemRenderer[] particleRenderers =
                root.GetComponentsInChildren<ParticleSystemRenderer>(includeInactive: true);
            for (int i = 0; i < particleRenderers.Length; i++)
            {
                ParticleSystemRenderer particleRenderer = particleRenderers[i];
                Mesh mesh = particleRenderer.mesh;
                if (mesh != null)
                {
                    particleRenderer.mesh = EnsurePromotedHitFeedbackMesh(mesh);
                }

                int meshCount = particleRenderer.meshCount;
                if (meshCount > 0)
                {
                    Mesh[] meshes = new Mesh[meshCount];
                    int copiedCount = particleRenderer.GetMeshes(meshes);
                    for (int meshIndex = 0; meshIndex < copiedCount; meshIndex++)
                    {
                        if (meshes[meshIndex] != null)
                        {
                            meshes[meshIndex] = EnsurePromotedHitFeedbackMesh(meshes[meshIndex]);
                        }
                    }

                    particleRenderer.SetMeshes(meshes, copiedCount);
                }

                EditorUtility.SetDirty(particleRenderer);
            }
        }

        private static Mesh EnsurePromotedHitFeedbackMesh(Mesh sourceMesh)
        {
            string sourcePath = AssetDatabase.GetAssetPath(sourceMesh).Replace('\\', '/');
            if (sourcePath.StartsWith("Assets/_Game/", StringComparison.Ordinal))
            {
                return sourceMesh;
            }

            if (string.IsNullOrWhiteSpace(sourcePath) || !sourcePath.StartsWith("Assets/", StringComparison.Ordinal))
            {
                return sourceMesh;
            }

            string targetPath = MeshRoot + "/DB_CombatHit_"
                + SanitizeAssetFileName(sourceMesh.name)
                + ".asset";
            EnsureFolder(Path.GetDirectoryName(targetPath).Replace('\\', '/'));

            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(targetPath);
            if (mesh != null)
            {
                return mesh;
            }

            mesh = UnityEngine.Object.Instantiate(sourceMesh);
            mesh.name = sourceMesh.name;
            AssetDatabase.CreateAsset(mesh, targetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(targetPath, ImportAssetOptions.ForceUpdate);

            Mesh promotedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(targetPath);
            if (promotedMesh == null)
            {
                throw new InvalidOperationException($"Failed to promote hit feedback mesh from {sourcePath} to {targetPath}.");
            }

            return promotedMesh;
        }

        private static void ConfigurePromotedMuzzleFlashParticles(GameObject root)
        {
            ParticleSystem[] particleSystems = root.GetComponentsInChildren<ParticleSystem>(includeInactive: true);
            if (particleSystems.Length == 0)
            {
                throw new InvalidOperationException($"{root.name} should preserve authored Vefects muzzle flash particles.");
            }

            for (int i = 0; i < particleSystems.Length; i++)
            {
                ParticleSystem particleSystem = particleSystems[i];
                ParticleSystem.MainModule main = particleSystem.main;
                main.loop = false;
                main.playOnAwake = false;
                main.simulationSpace = ParticleSystemSimulationSpace.Local;
                main.scalingMode = ParticleSystemScalingMode.Hierarchy;

                ParticleSystem.EmissionModule emission = particleSystem.emission;
                emission.enabled = true;
                particleSystem.Clear(withChildren: true);
                EditorUtility.SetDirty(particleSystem);
            }
        }

        private static void ConfigurePromotedMuzzleSmokeParticles(GameObject root)
        {
            ConfigurePromotedMuzzleSmokeParticle(
                root.transform.Find("Smoke Front"),
                "Smoke Front",
                new Vector3(1.55f, 1.55f, 1.2f),
                0.34f,
                0.62f,
                0.42f,
                0.72f,
                new Color(0.72f, 0.76f, 0.78f, 0.18f),
                new Color(0.95f, 0.98f, 1f, 0.28f),
                7,
                12);
            ConfigurePromotedMuzzleSmokeParticle(
                root.transform.Find("Smoke Side"),
                "Smoke Side",
                new Vector3(1.8f, 1.8f, 1.3f),
                0.30f,
                0.56f,
                0.34f,
                0.62f,
                new Color(0.70f, 0.74f, 0.78f, 0.14f),
                new Color(0.92f, 0.96f, 1f, 0.24f),
                6,
                10);
        }

        private static void ConfigurePromotedMuzzleSmokeParticle(
            Transform smokeRoot,
            string label,
            Vector3 localScale,
            float lifetimeMin,
            float lifetimeMax,
            float sizeMin,
            float sizeMax,
            Color colorMin,
            Color colorMax,
            short burstMin,
            short burstMax)
        {
            if (smokeRoot == null)
            {
                throw new InvalidOperationException($"{ImportedRifleMuzzleFlashPrefabPath} is missing {label}.");
            }

            smokeRoot.localScale = localScale;

            ParticleSystem particleSystem = smokeRoot.GetComponent<ParticleSystem>();
            if (particleSystem == null)
            {
                throw new InvalidOperationException($"{label} should be a ParticleSystem.");
            }

            ParticleSystem.MainModule main = particleSystem.main;
            main.startLifetime = CreateTwoConstantCurve(lifetimeMin, lifetimeMax);
            main.startSize = CreateTwoConstantCurve(sizeMin, sizeMax);
            main.startSpeed = CreateTwoConstantCurve(0.04f, 0.16f);
            main.startColor = new ParticleSystem.MinMaxGradient(colorMin, colorMax);

            ParticleSystem.EmissionModule emission = particleSystem.emission;
            emission.enabled = true;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, burstMin, burstMax) });

            ParticleSystemRenderer renderer = smokeRoot.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                renderer.enabled = true;
                renderer.sortingFudge = 0.25f;
                renderer.minParticleSize = 0.02f;
                renderer.maxParticleSize = 1f;
                EditorUtility.SetDirty(renderer);
            }

            particleSystem.Clear(withChildren: true);
            EditorUtility.SetDirty(smokeRoot);
            EditorUtility.SetDirty(particleSystem);
        }

        private static ParticleSystem.MinMaxCurve CreateTwoConstantCurve(float min, float max)
        {
            ParticleSystem.MinMaxCurve curve = new ParticleSystem.MinMaxCurve();
            curve.mode = ParticleSystemCurveMode.TwoConstants;
            curve.constantMin = min;
            curve.constantMax = max;
            return curve;
        }

        private static void AttachReviewedGunshotAudio(GameObject root)
        {
            AudioClip[] clips = LoadReviewedGunshotClips();
            Transform existing = root.transform.Find(PlayerRangedGunshotAudioName);
            if (existing != null)
            {
                UnityEngine.Object.DestroyImmediate(existing.gameObject);
            }

            GameObject audioObject = new GameObject(PlayerRangedGunshotAudioName);
            audioObject.transform.SetParent(root.transform, worldPositionStays: false);
            AudioSource source = audioObject.AddComponent<AudioSource>();
            source.clip = null;
            source.playOnAwake = false;
            source.loop = false;
            source.volume = PlayerRangedGunshotBaseVolume;
            source.pitch = 1f;
            source.spatialBlend = 0f;
            source.dopplerLevel = 0f;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = 4f;
            source.maxDistance = 24f;
            CombatVfxCueAudioRandomizer randomizer = audioObject.AddComponent<CombatVfxCueAudioRandomizer>();
            randomizer.Configure(
                source,
                clips,
                PlayerRangedGunshotBaseVolume,
                PlayerRangedGunshotMinimumPitch,
                PlayerRangedGunshotMaximumPitch,
                PlayerRangedGunshotMinimumVolumeMultiplier,
                PlayerRangedGunshotMaximumVolumeMultiplier);
            EditorUtility.SetDirty(source);
            EditorUtility.SetDirty(randomizer);
        }

        private static void EnsureReviewedCombatCueAudioBanks()
        {
            PromoteReviewedAudioClips(PlayerDodgeStartSourceClipPaths, PlayerDodgeStartClipPaths);
            PromoteReviewedAudioClips(PlayerPerfectDodgeTimeWarpSourceClipPaths, PlayerPerfectDodgeTimeWarpClipPaths);
            PromoteReviewedAudioClips(PlayerPerfectDodgeSuccessSourceClipPaths, PlayerPerfectDodgeSuccessClipPaths);
            ImportReviewedAudioClips(PlayerPerfectDodgeShieldActivateClipPaths);
            ImportReviewedAudioClips(PlayerPerfectDodgeShieldBlockClipPaths);
            PromoteReviewedAudioClips(EliteSummonSignalSourceClipPaths, EliteSummonSignalClipPaths);
            PromoteReviewedAudioClips(SummonBlockOpportunitySourceClipPaths, SummonBlockOpportunityClipPaths);
            PromoteReviewedAudioClips(SummonFollowupWindowSourceClipPaths, SummonFollowupWindowClipPaths);
            ImportReviewedAudioClips(PlayerSummonPreSpawnPortalClipPaths);
            ImportReviewedAudioClips(PlayerSummonLandingCraterClipPaths);
            ImportReviewedAudioClips(PlayerSummonDragonBreathClipPaths);

            StripReviewedCueAudio(
                PrefabRoot + "/DB_VFX_PlayerDodgeStart.prefab",
                PlayerDodgeStartAudioName);
            AttachReviewedCueAudio(
                PrefabRoot + "/DB_VFX_PlayerDodgeStart.prefab",
                PlayerDodgeStartAudioName,
                PlayerDodgeStartClipPaths,
                0.34f,
                1.02f,
                1.08f,
                0.9f,
                1.04f,
                0.08f,
                142);

            StripReviewedCueAudio(
                PrefabRoot + "/DB_VFX_PlayerPerfectDodgeTimeField.prefab",
                PlayerPerfectDodgeTimeWarpAudioName);
            AttachReviewedCueAudio(
                PrefabRoot + "/DB_VFX_PlayerPerfectDodgeTimeField.prefab",
                PlayerPerfectDodgeTimeWarpAudioName,
                PlayerPerfectDodgeTimeWarpClipPaths,
                0.38f,
                1.02f,
                1.06f,
                0.86f,
                0.98f,
                0.04f,
                130);

            StripReviewedCueAudio(
                PrefabRoot + "/DB_VFX_PlayerPerfectDodgeWindow.prefab",
                PlayerPerfectDodgeSuccessAudioName);
            AttachReviewedCueAudio(
                PrefabRoot + "/DB_VFX_PlayerPerfectDodgeWindow.prefab",
                PlayerPerfectDodgeSuccessAudioName,
                PlayerPerfectDodgeShieldActivateClipPaths,
                0.34f,
                0.98f,
                1.04f,
                0.9f,
                1.02f,
                0.10f,
                132);

            StripReviewedCueAudio(
                PrefabRoot + "/DB_VFX_PlayerRangedProjectileImpact.prefab",
                PlayerRangedProjectileImpactAudioName);

            StripReviewedCueAudio(
                PrefabRoot + "/DB_VFX_EliteSummonSignal.prefab",
                EliteSummonSignalAudioName);
            AttachReviewedCueAudio(
                PrefabRoot + "/DB_VFX_EliteSummonSignal.prefab",
                EliteSummonSignalAudioName,
                EliteSummonSignalClipPaths,
                0.44f,
                0.97f,
                1.04f,
                0.9f,
                1.03f,
                0.22f,
                134);

            StripReviewedCueAudio(
                PrefabRoot + "/DB_VFX_SummonBlockOpportunity.prefab",
                SummonBlockOpportunityAudioName);
            AttachReviewedCueAudio(
                PrefabRoot + "/DB_VFX_SummonBlockOpportunity.prefab",
                SummonBlockOpportunityAudioName,
                SummonBlockOpportunityClipPaths,
                0.52f,
                0.98f,
                1.06f,
                0.9f,
                1.05f,
                0.2f,
                132);

            ConfigureSummonProjectileInterceptAudioBank();

            StripReviewedCueAudio(
                PrefabRoot + "/DB_VFX_SummonFollowupWindow.prefab",
                SummonFollowupWindowAudioName);
            AttachReviewedCueAudio(
                PrefabRoot + "/DB_VFX_SummonFollowupWindow.prefab",
                SummonFollowupWindowAudioName,
                SummonFollowupWindowClipPaths,
                0.4f,
                0.99f,
                1.06f,
                0.9f,
                1.04f,
                0.16f,
                136);

            StripReviewedCueAudio(
                PrefabRoot + "/DB_VFX_PlayerSummonPreSpawnPortal.prefab",
                PlayerSummonPreSpawnPortalAudioName);
            AttachReviewedCueAudio(
                PrefabRoot + "/DB_VFX_PlayerSummonPreSpawnPortal.prefab",
                PlayerSummonPreSpawnPortalAudioName,
                PlayerSummonPreSpawnPortalClipPaths,
                0.42f,
                0.98f,
                1.04f,
                0.88f,
                1.02f,
                0.16f,
                136);

            StripReviewedCueAudio(
                PrefabRoot + "/DB_VFX_PlayerSummonLandingCrater.prefab",
                PlayerSummonLandingCraterAudioName);
            AttachReviewedCueAudio(
                PrefabRoot + "/DB_VFX_PlayerSummonLandingCrater.prefab",
                PlayerSummonLandingCraterAudioName,
                PlayerSummonLandingCraterClipPaths,
                0.56f,
                0.94f,
                1.05f,
                0.88f,
                1.04f,
                0.28f,
                128);

            StripReviewedCueAudio(
                PrefabRoot + "/DB_VFX_PlayerSummonDragonBreathAudio.prefab",
                PlayerSummonDragonBreathAudioName);
            AttachReviewedCueAudio(
                PrefabRoot + "/DB_VFX_PlayerSummonDragonBreathAudio.prefab",
                PlayerSummonDragonBreathAudioName,
                PlayerSummonDragonBreathClipPaths,
                0.48f,
                0.96f,
                1.03f,
                0.9f,
                1.02f,
                0.35f,
                126);
        }

        private static void ConfigureSummonProjectileInterceptAudioBank()
        {
            const string prefabPath = PrefabRoot + "/DB_VFX_SummonProjectileIntercept.prefab";
            StripReviewedCueAudio(prefabPath, SummonBlockOpportunityAudioName);
            AttachReviewedCueAudio(
                prefabPath,
                SummonBlockOpportunityAudioName,
                SummonBlockOpportunityClipPaths,
                0.52f,
                0.98f,
                1.06f,
                0.9f,
                1.05f,
                0.2f,
                132);
        }

        private static void StripReviewedCueAudio(string prefabPath, string childName)
        {
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                Transform existing = prefabRoot.transform.Find(childName);
                if (existing != null)
                {
                    UnityEngine.Object.DestroyImmediate(existing.gameObject);
                }

                CombatVfxCueAudioRandomizer[] randomizers =
                    prefabRoot.GetComponentsInChildren<CombatVfxCueAudioRandomizer>(includeInactive: true);
                for (int i = 0; i < randomizers.Length; i++)
                {
                    if (randomizers[i] != null)
                    {
                        UnityEngine.Object.DestroyImmediate(randomizers[i]);
                    }
                }

                AudioSource[] audioSources = prefabRoot.GetComponentsInChildren<AudioSource>(includeInactive: true);
                for (int i = 0; i < audioSources.Length; i++)
                {
                    if (audioSources[i] != null)
                    {
                        UnityEngine.Object.DestroyImmediate(audioSources[i]);
                    }
                }

                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        private static void PromoteReviewedAudioClips(string[] sourceClipPaths, string[] targetClipPaths)
        {
            if (sourceClipPaths.Length != targetClipPaths.Length)
            {
                throw new InvalidOperationException("Reviewed audio source and target banks must have matching lengths.");
            }

            for (int i = 0; i < sourceClipPaths.Length; i++)
            {
                if (AssetDatabase.LoadAssetAtPath<AudioClip>(sourceClipPaths[i]) == null)
                {
                    throw new FileNotFoundException($"Missing source reviewed audio clip at {sourceClipPaths[i]}.");
                }

                EnsureFolder(Path.GetDirectoryName(targetClipPaths[i]).Replace('\\', '/'));
                File.Copy(sourceClipPaths[i], targetClipPaths[i], overwrite: true);
                AssetDatabase.ImportAsset(targetClipPaths[i], ImportAssetOptions.ForceUpdate);
            }
        }

        private static void ImportReviewedAudioClips(string[] clipPaths)
        {
            for (int i = 0; i < clipPaths.Length; i++)
            {
                string absolutePath = ToProjectAbsolutePath(clipPaths[i]);
                if (!File.Exists(absolutePath))
                {
                    throw new FileNotFoundException($"Missing reviewed audio clip at {clipPaths[i]}.");
                }

                AssetDatabase.ImportAsset(clipPaths[i], ImportAssetOptions.ForceUpdate);
            }
        }

        private static void AttachReviewedCueAudio(
            string prefabPath,
            string childName,
            string[] clipPaths,
            float baseVolume,
            float minimumPitch,
            float maximumPitch,
            float minimumVolumeMultiplier,
            float maximumVolumeMultiplier,
            float spatialBlend,
            int priority)
        {
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                AttachReviewedAudioBank(
                    prefabRoot,
                    childName,
                    clipPaths,
                    baseVolume,
                    minimumPitch,
                    maximumPitch,
                    minimumVolumeMultiplier,
                    maximumVolumeMultiplier,
                    spatialBlend,
                    priority);
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        private static void AttachReviewedAudioBank(
            GameObject root,
            string childName,
            string[] clipPaths,
            float baseVolume,
            float minimumPitch,
            float maximumPitch,
            float minimumVolumeMultiplier,
            float maximumVolumeMultiplier,
            float spatialBlend,
            int priority)
        {
            Transform existing = root.transform.Find(childName);
            if (existing != null)
            {
                UnityEngine.Object.DestroyImmediate(existing.gameObject);
            }

            GameObject audioObject = new GameObject(childName);
            audioObject.transform.SetParent(root.transform, worldPositionStays: false);
            AudioSource source = audioObject.AddComponent<AudioSource>();
            source.clip = null;
            source.playOnAwake = false;
            source.loop = false;
            source.volume = baseVolume;
            source.pitch = 1f;
            source.spatialBlend = spatialBlend;
            source.dopplerLevel = 0f;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = 3f;
            source.maxDistance = 28f;
            source.priority = priority;
            CombatVfxCueAudioRandomizer randomizer = audioObject.AddComponent<CombatVfxCueAudioRandomizer>();
            randomizer.Configure(
                source,
                LoadReviewedAudioClips(clipPaths),
                baseVolume,
                minimumPitch,
                maximumPitch,
                minimumVolumeMultiplier,
                maximumVolumeMultiplier);
            EditorUtility.SetDirty(audioObject);
            EditorUtility.SetDirty(source);
            EditorUtility.SetDirty(randomizer);
        }

        private static AudioClip[] LoadReviewedGunshotClips()
        {
            return LoadReviewedAudioClips(PlayerRangedGunshotClipPaths);
        }

        private static AudioClip[] LoadReviewedAudioClips(string[] clipPaths)
        {
            AudioClip[] clips = new AudioClip[clipPaths.Length];
            for (int i = 0; i < clipPaths.Length; i++)
            {
                AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(clipPaths[i]);
                if (clip == null)
                {
                    throw new FileNotFoundException($"Missing reviewed audio clip at {clipPaths[i]}.");
                }

                clips[i] = clip;
            }

            return clips;
        }

        private static GameObject SaveRangedProjectileImpactPrefab(
            string name,
            Material flashMaterial,
            Material sparkMaterial,
            Material smokeMaterial)
        {
            string prefabPath = $"{PrefabRoot}/{name}.prefab";
            GameObject root = new GameObject(name);
            var renderers = new System.Collections.Generic.List<Renderer>
            {
                AddPrimitive(
                    root.transform,
                    "ImpactFlashCore",
                    PrimitiveType.Sphere,
                    flashMaterial,
                    new Vector3(0f, 0.16f, 0f),
                    Vector3.zero,
                    new Vector3(0.34f, 0.26f, 0.34f)),
                AddPrimitive(
                    root.transform,
                    "ImpactSparkFan",
                    PrimitiveType.Cube,
                    sparkMaterial,
                    new Vector3(0f, 0.18f, -0.12f),
                    Vector3.zero,
                    new Vector3(0.18f, 0.04f, 0.78f)),
                AddPrimitive(
                    root.transform,
                    "ImpactSparkLeft",
                    PrimitiveType.Cube,
                    sparkMaterial,
                    new Vector3(-0.18f, 0.16f, -0.05f),
                    new Vector3(0f, -24f, 0f),
                    new Vector3(0.06f, 0.035f, 0.52f)),
                AddPrimitive(
                    root.transform,
                    "ImpactSparkRight",
                    PrimitiveType.Cube,
                    sparkMaterial,
                    new Vector3(0.18f, 0.16f, -0.05f),
                    new Vector3(0f, 24f, 0f),
                    new Vector3(0.06f, 0.035f, 0.52f))
            };

            AddParticleBurst(
                root.transform,
                "Impact_FrontFlash",
                flashMaterial,
                new Vector3(0f, 0.22f, 0f),
                Vector3.zero,
                0.16f,
                0.08f,
                0.6f,
                0.42f,
                8,
                ParticleSystemShapeType.Cone,
                0.05f,
                28f,
                new Color(1f, 0.96f, 0.72f, 0.96f));
            AddParticleBurst(
                root.transform,
                "Impact_EmberSpray",
                sparkMaterial,
                new Vector3(0f, 0.14f, -0.04f),
                Vector3.zero,
                0.34f,
                0.26f,
                2.6f,
                0.08f,
                18,
                ParticleSystemShapeType.Cone,
                0.04f,
                22f,
                new Color(1f, 0.68f, 0.2f, 0.95f));
            AddParticleBurst(
                root.transform,
                "Impact_SmokePuff",
                smokeMaterial,
                new Vector3(0f, 0.12f, -0.05f),
                Vector3.zero,
                0.42f,
                0.34f,
                0.48f,
                0.24f,
                10,
                ParticleSystemShapeType.Cone,
                0.08f,
                34f,
                new Color(0.6f, 0.64f, 0.66f, 0.36f));

            CombatVfxCueVisual visual = root.AddComponent<CombatVfxCueVisual>();
            ConfigureCueVisual(
                visual,
                renderers.ToArray(),
                0.34f,
                new Color(1f, 0.92f, 0.48f, 0.98f),
                new Color(1f, 0.24f, 0.08f, 0f),
                new Vector3(0.55f, 0.55f, 0.55f),
                new Vector3(1.65f, 1.3f, 1.65f),
                30f,
                0.05f,
                0.08f);

            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            return savedPrefab;
        }

        private static ParticleSystem AddParticleBurst(
            Transform parent,
            string name,
            Material material,
            Vector3 localPosition,
            Vector3 localEuler,
            float duration,
            float startLifetime,
            float startSpeed,
            float startSize,
            int burstCount,
            ParticleSystemShapeType shapeType,
            float radius,
            float angle,
            Color startColor)
        {
            GameObject particleObject = new GameObject(name);
            particleObject.transform.SetParent(parent, worldPositionStays: false);
            particleObject.transform.localPosition = localPosition;
            particleObject.transform.localRotation = Quaternion.Euler(localEuler);
            particleObject.transform.localScale = Vector3.one;

            ParticleSystem particleSystem = particleObject.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particleSystem.main;
            main.loop = false;
            main.duration = Mathf.Max(0.02f, duration);
            main.startLifetime = Mathf.Max(0.01f, startLifetime);
            main.startSpeed = Mathf.Max(0f, startSpeed);
            main.startSize = Mathf.Max(0.001f, startSize);
            main.startColor = startColor;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.playOnAwake = true;
            main.maxParticles = Mathf.Max(16, burstCount * 3);

            ParticleSystem.EmissionModule emission = particleSystem.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[]
            {
                new ParticleSystem.Burst(0f, (short)Mathf.Max(1, burstCount))
            });

            ParticleSystem.ShapeModule shape = particleSystem.shape;
            shape.enabled = true;
            shape.shapeType = shapeType;
            shape.radius = Mathf.Max(0f, radius);
            shape.angle = Mathf.Max(0f, angle);

            ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particleSystem.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(startColor, 0f),
                    new GradientColorKey(new Color(startColor.r, startColor.g, startColor.b, 0f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(startColor.a, 0f),
                    new GradientAlphaKey(0f, 1f)
                });
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

            ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = particleSystem.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(
                1f,
                new AnimationCurve(
                    new Keyframe(0f, 0.55f),
                    new Keyframe(0.18f, 1f),
                    new Keyframe(1f, 0f)));

            ParticleSystem.NoiseModule noise = particleSystem.noise;
            noise.enabled = true;
            noise.strength = 0.05f;
            noise.frequency = 1.8f;

            ParticleSystemRenderer renderer = particleObject.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sharedMaterial = material;
            renderer.sortingFudge = 1f;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            return particleSystem;
        }

        private static void AddProjectileCueGeometry(
            GameObject root,
            Material material,
            float radius,
            float speed,
            float minSize,
            float maxSize,
            System.Collections.Generic.List<Renderer> renderers)
        {
            float speedFactor = Mathf.InverseLerp(45f, 80f, speed);
            float sizeFactor = Mathf.Clamp01((minSize + maxSize) * 1.35f);
            float coreWidth = Mathf.Lerp(0.18f, 0.28f, sizeFactor);
            float coreLength = Mathf.Lerp(0.44f, 0.66f, speedFactor);
            float trailLength = Mathf.Lerp(0.95f, 1.45f, speedFactor);
            float sideSparkLength = Mathf.Lerp(0.42f, 0.68f, speedFactor);
            float height = Mathf.Lerp(0.95f, 1.18f, Mathf.Clamp01(radius * 3.2f));

            renderers.Add(AddPrimitive(root.transform, "ProjectileCore", PrimitiveType.Sphere, material, new Vector3(0f, height, 0.36f), Vector3.zero, new Vector3(coreWidth, coreWidth, coreLength)));
            renderers.Add(AddPrimitive(root.transform, "ProjectileTrail", PrimitiveType.Cube, material, new Vector3(0f, height, -0.28f), Vector3.zero, new Vector3(coreWidth * 0.54f, coreWidth * 0.34f, trailLength)));
            renderers.Add(AddPrimitive(root.transform, "ProjectileLeftSpark", PrimitiveType.Cube, material, new Vector3(-coreWidth * 0.62f, height + 0.02f, -0.05f), new Vector3(0f, -18f, 0f), new Vector3(coreWidth * 0.25f, coreWidth * 0.24f, sideSparkLength)));
            renderers.Add(AddPrimitive(root.transform, "ProjectileRightSpark", PrimitiveType.Cube, material, new Vector3(coreWidth * 0.62f, height - 0.02f, -0.05f), new Vector3(0f, 18f, 0f), new Vector3(coreWidth * 0.25f, coreWidth * 0.24f, sideSparkLength)));
        }

        private static void AddCueGeometry(
            GameObject root,
            Material material,
            ParticleSystemShapeType shapeType,
            float radius,
            float speed,
            float arcDegrees,
            float minSize,
            float maxSize,
            System.Collections.Generic.List<Renderer> renderers)
        {
            float radiusScale = Mathf.Max(0.28f, radius * 2.4f);
            float sizeScale = Mathf.Max(0.2f, (minSize + maxSize) * 1.8f);

            if (shapeType == ParticleSystemShapeType.Circle)
            {
                renderers.Add(AddPrimitive(root.transform, "GroundPulseDisc", PrimitiveType.Cylinder, material, Vector3.zero, Vector3.zero, new Vector3(radiusScale, 0.018f, radiusScale)));
                renderers.Add(AddPrimitive(root.transform, "GroundPulseLineA", PrimitiveType.Cube, material, new Vector3(0f, 0.025f, 0f), Vector3.zero, new Vector3(radiusScale * 0.8f, 0.018f, 0.035f)));
                renderers.Add(AddPrimitive(root.transform, "GroundPulseLineB", PrimitiveType.Cube, material, new Vector3(0f, 0.03f, 0f), new Vector3(0f, 90f, 0f), new Vector3(radiusScale * 0.8f, 0.018f, 0.035f)));
                return;
            }

            if (shapeType == ParticleSystemShapeType.Sphere)
            {
                renderers.Add(AddPrimitive(root.transform, "CorePulse", PrimitiveType.Sphere, material, new Vector3(0f, 0.22f, 0f), Vector3.zero, Vector3.one * Mathf.Max(0.22f, radiusScale * 0.36f + sizeScale * 0.3f)));
                renderers.Add(AddPrimitive(root.transform, "GroundEcho", PrimitiveType.Cylinder, material, new Vector3(0f, 0.02f, 0f), Vector3.zero, new Vector3(radiusScale * 0.82f, 0.012f, radiusScale * 0.82f)));
                return;
            }

            float arcFactor = Mathf.InverseLerp(24f, 150f, arcDegrees);
            float speedFactor = Mathf.InverseLerp(8f, 75f, speed);
            float width = Mathf.Lerp(0.22f, 0.9f, arcFactor) + sizeScale * 0.28f;
            float length = Mathf.Lerp(0.65f, 1.75f, speedFactor) + radiusScale * 0.45f;
            renderers.Add(AddPrimitive(root.transform, "ForwardSweep", PrimitiveType.Cube, material, new Vector3(0f, 0.04f, length * 0.42f), Vector3.zero, new Vector3(width, 0.035f, length)));

            if (arcDegrees >= 72f)
            {
                float sideAngle = Mathf.Min(42f, arcDegrees * 0.23f);
                renderers.Add(AddPrimitive(root.transform, "ForwardSweepLeftEdge", PrimitiveType.Cube, material, new Vector3(-width * 0.18f, 0.05f, length * 0.38f), new Vector3(0f, -sideAngle, 0f), new Vector3(width * 0.28f, 0.028f, length * 0.92f)));
                renderers.Add(AddPrimitive(root.transform, "ForwardSweepRightEdge", PrimitiveType.Cube, material, new Vector3(width * 0.18f, 0.05f, length * 0.38f), new Vector3(0f, sideAngle, 0f), new Vector3(width * 0.28f, 0.028f, length * 0.92f)));
            }
        }

        private static void ConfigureCueVisual(
            CombatVfxCueVisual visual,
            Renderer[] renderers,
            float lifetimeSeconds,
            Color startColor,
            Color endColor,
            Vector3 startScale,
            Vector3 endScale,
            float spinDegreesPerSecond,
            float verticalLift,
            float forwardTravelDistance,
            bool overrideRendererColors = true)
        {
            SerializedObject serializedObject = new SerializedObject(visual);
            SerializedProperty rendererArray = RequireProperty(serializedObject, "renderers");
            rendererArray.arraySize = renderers.Length;
            for (int i = 0; i < renderers.Length; i++)
            {
                rendererArray.GetArrayElementAtIndex(i).objectReferenceValue = renderers[i];
            }

            RequireProperty(serializedObject, "overrideRendererColors").boolValue = overrideRendererColors;
            RequireProperty(serializedObject, "startColor").colorValue = startColor;
            RequireProperty(serializedObject, "endColor").colorValue = endColor;
            RequireProperty(serializedObject, "startScale").vector3Value = startScale;
            RequireProperty(serializedObject, "endScale").vector3Value = endScale;
            RequireProperty(serializedObject, "lifetimeSeconds").floatValue = lifetimeSeconds;
            RequireProperty(serializedObject, "spinDegreesPerSecond").floatValue = spinDegreesPerSecond;
            RequireProperty(serializedObject, "verticalLift").floatValue = verticalLift;
            RequireProperty(serializedObject, "forwardTravelDistance").floatValue = forwardTravelDistance;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(visual);
        }

        private static float ResolveVisualSpin(ParticleSystemShapeType shapeType, float speed)
        {
            if (shapeType == ParticleSystemShapeType.Circle)
            {
                return Mathf.Clamp(speed * 2.2f, 28f, 180f);
            }

            if (shapeType == ParticleSystemShapeType.Sphere)
            {
                return Mathf.Clamp(speed * 1.4f, 12f, 120f);
            }

            return 0f;
        }

        private static Renderer AddPrimitive(
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
            ConfigurePrimitiveObject(primitive, primitiveType, material, localPosition, localEuler, localScale);
            return primitive.GetComponent<Renderer>();
        }

        private static Renderer AddMesh(
            Transform parent,
            string name,
            Mesh mesh,
            Material material,
            Vector3 localPosition,
            Vector3 localEuler,
            Vector3 localScale)
        {
            if (mesh == null)
            {
                throw new InvalidOperationException($"{name} requires a mesh.");
            }

            if (material == null)
            {
                throw new InvalidOperationException($"{name} requires a material.");
            }

            GameObject meshObject = new GameObject(name);
            meshObject.transform.SetParent(parent, worldPositionStays: false);

            MeshFilter meshFilter = meshObject.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;
            MeshRenderer renderer = meshObject.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            ConfigureRendererForCue(renderer);

            meshObject.transform.localPosition = localPosition;
            meshObject.transform.localRotation = Quaternion.Euler(localEuler);
            meshObject.transform.localScale = localScale;
            return renderer;
        }

        private static Renderer EnsurePrimitiveChild(
            Transform parent,
            string name,
            PrimitiveType primitiveType,
            Material material,
            Vector3 localPosition,
            Vector3 localEuler,
            Vector3 localScale)
        {
            Transform child = parent.Find(name);
            GameObject childObject = child != null ? child.gameObject : GameObject.CreatePrimitive(primitiveType);
            childObject.name = name;
            childObject.transform.SetParent(parent, worldPositionStays: false);
            ConfigurePrimitiveObject(childObject, primitiveType, material, localPosition, localEuler, localScale);
            EditorUtility.SetDirty(childObject);
            return childObject.GetComponent<Renderer>();
        }

        private static void ConfigurePrimitiveObject(
            GameObject primitive,
            PrimitiveType primitiveType,
            Material material,
            Vector3 localPosition,
            Vector3 localEuler,
            Vector3 localScale)
        {
            MeshFilter meshFilter = primitive.GetComponent<MeshFilter>();
            if (meshFilter == null)
            {
                meshFilter = primitive.AddComponent<MeshFilter>();
            }

            MeshRenderer renderer = primitive.GetComponent<MeshRenderer>();
            if (renderer == null)
            {
                renderer = primitive.AddComponent<MeshRenderer>();
            }

            meshFilter.sharedMesh = LoadPrimitiveMesh(primitiveType);
            renderer.sharedMaterial = material;
            ConfigureRendererForCue(renderer);

            Collider collider = primitive.GetComponent<Collider>();
            if (collider != null)
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }

            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.Euler(localEuler);
            primitive.transform.localScale = localScale;
        }

        private static void ConfigureRendererForCue(Renderer renderer)
        {
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.allowOcclusionWhenDynamic = false;
        }

        private static Mesh LoadPrimitiveMesh(PrimitiveType primitiveType)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            Mesh mesh = primitive.GetComponent<MeshFilter>().sharedMesh;
            UnityEngine.Object.DestroyImmediate(primitive);
            return mesh;
        }

        private static Material LoadRequiredMaterial(string assetPath)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            if (material == null)
            {
                throw new InvalidOperationException($"Missing required material at {assetPath}.");
            }

            return material;
        }

        private static Material LoadReviewedShapesFxMaterial(string assetPath)
        {
            Material material = LoadRequiredMaterial(assetPath);
            RemapShapesFxImportedTextures(material);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void RemapShapesFxImportedTextures(Material material)
        {
            if (material == null)
            {
                return;
            }

            string[] propertyNames = material.GetTexturePropertyNames();
            for (int i = 0; i < propertyNames.Length; i++)
            {
                Texture texture = material.GetTexture(propertyNames[i]);
                if (texture == null)
                {
                    continue;
                }

                string sourcePath = AssetDatabase.GetAssetPath(texture).Replace('\\', '/');
                if (!sourcePath.StartsWith(ImportedShapesFxTextureRoot + "/", StringComparison.Ordinal))
                {
                    continue;
                }

                Texture promotedTexture = ResolveReviewedShapesFxTexture(sourcePath);
                material.SetTexture(propertyNames[i], promotedTexture);
            }
        }

        private static Texture ResolveReviewedShapesFxTexture(string sourcePath)
        {
            string relativePath = sourcePath.Substring(ImportedShapesFxTextureRoot.Length + 1);
            string targetPath = ShapesFxTextureRoot + "/" + relativePath;
            Texture texture = AssetDatabase.LoadAssetAtPath<Texture>(targetPath);
            if (texture != null)
            {
                return texture;
            }

            EnsureFolder(Path.GetDirectoryName(targetPath).Replace('\\', '/'));
            if (!AssetDatabase.CopyAsset(sourcePath, targetPath))
            {
                throw new InvalidOperationException($"Failed to promote Shapes FX texture from {sourcePath} to {targetPath}.");
            }

            AssetDatabase.ImportAsset(targetPath);
            texture = AssetDatabase.LoadAssetAtPath<Texture>(targetPath);
            if (texture == null)
            {
                throw new InvalidOperationException($"Promoted Shapes FX texture did not import at {targetPath}.");
            }

            return texture;
        }

        private static Mesh LoadRequiredMesh(string assetPath)
        {
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
            if (mesh != null)
            {
                return mesh;
            }

            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is Mesh embeddedMesh && !embeddedMesh.name.Contains("__preview"))
                {
                    return embeddedMesh;
                }
            }

            throw new InvalidOperationException($"Missing required mesh at {assetPath}.");
        }

        private static GameObject LoadRequiredPrefab(string assetPath, string label)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab == null)
            {
                throw new InvalidOperationException($"Missing required {label} prefab at {assetPath}.");
            }

            return prefab;
        }

        private static Material LoadOrCreateParticleMaterial(string name, Color color, bool additive = true)
        {
            string path = $"{MaterialRoot}/{name}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(FindParticleShader());
                AssetDatabase.CreateAsset(material, path);
            }

            material.shader = FindParticleShader();
            material.SetColor("_BaseColor", color);
            material.SetColor("_Color", color);
            if (additive)
            {
                SetMaterialFloatIfPresent(material, "_Surface", 1f);
                SetMaterialFloatIfPresent(material, "_Blend", 2f);
                SetMaterialFloatIfPresent(material, "_SrcBlend", (float)BlendMode.SrcAlpha);
                SetMaterialFloatIfPresent(material, "_DstBlend", (float)BlendMode.One);
                SetMaterialFloatIfPresent(material, "_ZWrite", 0f);
                material.renderQueue = (int)RenderQueue.Transparent;
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            }
            else
            {
                SetMaterialFloatIfPresent(material, "_Surface", 1f);
                SetMaterialFloatIfPresent(material, "_Blend", 0f);
                SetMaterialFloatIfPresent(material, "_SrcBlend", (float)BlendMode.SrcAlpha);
                SetMaterialFloatIfPresent(material, "_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                SetMaterialFloatIfPresent(material, "_ZWrite", 0f);
                material.renderQueue = (int)RenderQueue.Transparent;
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material LoadOrCreatePerfectDodgeWorldFxMaterial()
        {
            Material material = LoadOrCreatePerfectDodgeMaterial(
                "DB_PerfectDodgeWorldFx",
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
                "DB_PerfectDodgeAfterimage",
                PerfectDodgeAfterimageShaderPath);
            SetMaterialColorIfPresent(material, "_BaseColor", new Color(0.34f, 0.98f, 1f, 0.42f));
            SetMaterialColorIfPresent(material, "_RimColor", new Color(0.72f, 0.36f, 1f, 0.9f));
            SetMaterialFloatIfPresent(material, "_Alpha", 0.42f);
            SetMaterialFloatIfPresent(material, "_Intensity", 1f);
            SetMaterialFloatIfPresent(material, "_FresnelPower", 2.2f);
            SetMaterialFloatIfPresent(material, "_ScanStrength", 0.48f);
            return material;
        }

        private static Material LoadOrCreatePerfectDodgeMaterial(string materialName, string shaderPath)
        {
            EnsureFolder(MaterialRoot);
            Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(shaderPath);
            if (shader == null)
            {
                throw new InvalidOperationException($"Missing perfect dodge shader at {shaderPath}.");
            }

            string materialPath = $"{MaterialRoot}/{materialName}.mat";
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

        private static void ReplacePromotedHitFeedbackCoreTexture(Material material)
        {
            Texture directionalMask = material.HasProperty("_MaskTexture")
                ? material.GetTexture("_MaskTexture")
                : null;
            if (directionalMask == null)
            {
                return;
            }

            SetMaterialTextureIfPresent(material, "_MainTexture", directionalMask);
            SetMaterialTextureIfPresent(material, "_MainTex", directionalMask);
            SetMaterialTextureIfPresent(material, "_BaseMap", directionalMask);
        }

        private static Texture2D EnsurePromotedTexture(string sourcePath, string targetPath)
        {
            string absoluteTarget = ToProjectAbsolutePath(targetPath);
            if (!File.Exists(absoluteTarget))
            {
                string absoluteSource = ToProjectAbsolutePath(sourcePath);
                if (!File.Exists(absoluteSource))
                {
                    throw new InvalidOperationException($"Missing source muzzle flash texture at {sourcePath}.");
                }

                FileUtil.CopyFileOrDirectory(absoluteSource, absoluteTarget);
            }

            AssetDatabase.ImportAsset(targetPath, ImportAssetOptions.ForceUpdate);
            if (AssetImporter.GetAtPath(targetPath) is TextureImporter importer)
            {
                bool changed = false;
                changed |= SetImporterValue(importer.textureType, TextureImporterType.Default, value => importer.textureType = value);
                changed |= SetImporterValue(importer.alphaIsTransparency, true, value => importer.alphaIsTransparency = value);
                changed |= SetImporterValue(importer.mipmapEnabled, false, value => importer.mipmapEnabled = value);
                changed |= SetImporterValue(importer.sRGBTexture, true, value => importer.sRGBTexture = value);
                if (changed)
                {
                    importer.SaveAndReimport();
                }
            }

            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(targetPath);
            if (texture == null)
            {
                throw new InvalidOperationException($"Failed to load promoted muzzle flash texture at {targetPath}.");
            }

            return texture;
        }

        private static Shader EnsurePromotedShader(string sourcePath, string targetPath)
        {
            string absoluteTarget = ToProjectAbsolutePath(targetPath);
            if (!File.Exists(absoluteTarget))
            {
                string absoluteSource = ToProjectAbsolutePath(sourcePath);
                if (!File.Exists(absoluteSource))
                {
                    throw new InvalidOperationException($"Missing source Vefects shader at {sourcePath}.");
                }

                FileUtil.CopyFileOrDirectory(absoluteSource, absoluteTarget);
            }

            AssetDatabase.ImportAsset(targetPath, ImportAssetOptions.ForceUpdate);
            Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(targetPath);
            if (shader == null)
            {
                throw new InvalidOperationException($"Failed to load promoted Vefects shader at {targetPath}.");
            }

            return shader;
        }

        private static bool SetImporterValue<T>(T currentValue, T desiredValue, Action<T> applyValue)
        {
            if (Equals(currentValue, desiredValue))
            {
                return false;
            }

            applyValue(desiredValue);
            return true;
        }

        private static Material LoadOrCreateTextureMaterial(string name, Texture texture, Color color)
        {
            string path = $"{MaterialRoot}/{name}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(FindParticleShader());
                AssetDatabase.CreateAsset(material, path);
            }

            material.shader = FindParticleShader();
            material.SetColor("_BaseColor", color);
            material.SetColor("_Color", color);
            SetMaterialTextureIfPresent(material, "_BaseMap", texture);
            SetMaterialTextureIfPresent(material, "_MainTex", texture);
            SetMaterialFloatIfPresent(material, "_Surface", 1f);
            SetMaterialFloatIfPresent(material, "_Blend", 2f);
            SetMaterialFloatIfPresent(material, "_SrcBlend", (float)BlendMode.SrcAlpha);
            SetMaterialFloatIfPresent(material, "_DstBlend", (float)BlendMode.One);
            SetMaterialFloatIfPresent(material, "_ZWrite", 0f);
            material.renderQueue = (int)RenderQueue.Transparent;
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material LoadOrCreateVefectsMuzzleFlashMaterial(
            string name,
            Shader shader,
            Texture texture,
            Color color,
            float sourceBlend,
            float destinationBlend,
            float depthFade,
            float sideFadeStrength)
        {
            Material material = LoadOrCreateMaterialWithShader(name, shader);
            ConfigureVefectsFlipbookMaterial(material, texture, color);
            SetMaterialFloatIfPresent(material, "_Src", sourceBlend);
            SetMaterialFloatIfPresent(material, "_Dst", destinationBlend);
            SetMaterialFloatIfPresent(material, "_IsAdditive", 1f);
            SetMaterialFloatIfPresent(material, "_DepthFade", depthFade);
            SetMaterialFloatIfPresent(material, "_SideFadeStrength", sideFadeStrength);
            SetMaterialFloatIfPresent(material, "_EmissiveOverall", 1f);
            SetMaterialFloatIfPresent(material, "_Cull", 0f);
            SetMaterialFloatIfPresent(material, "_ZTest", 2f);
            SetMaterialFloatIfPresent(material, "_ZWrite", 0f);
            SetMaterialFloatIfPresent(material, "_ReceiveShadows", 1f);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material LoadOrCreateVefectsSmokeMaterial(
            string name,
            Shader shader,
            Texture texture,
            Color color)
        {
            Material material = LoadOrCreateMaterialWithShader(name, shader);
            ConfigureVefectsFlipbookMaterial(material, texture, color);
            SetMaterialFloatIfPresent(material, "_Src", (float)BlendMode.SrcAlpha);
            SetMaterialFloatIfPresent(material, "_Dst", (float)BlendMode.OneMinusSrcAlpha);
            SetMaterialFloatIfPresent(material, "_DepthFade", 0.3f);
            SetMaterialFloatIfPresent(material, "_TextureTint", 0f);
            SetMaterialFloatIfPresent(material, "_SSEroSoft", 1f);
            SetMaterialFloatIfPresent(material, "_Cull", 0f);
            SetMaterialFloatIfPresent(material, "_ZTest", 2f);
            SetMaterialFloatIfPresent(material, "_ZWrite", 0f);
            SetMaterialFloatIfPresent(material, "_ReceiveShadows", 1f);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material LoadOrCreateMaterialWithShader(string name, Shader shader)
        {
            string path = $"{MaterialRoot}/{name}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            material.shader = shader;
            material.enableInstancing = true;
            material.renderQueue = -1;
            material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
            return material;
        }

        private static void ConfigureVefectsFlipbookMaterial(Material material, Texture texture, Color color)
        {
            SetMaterialTextureIfPresent(material, "_Flipbook", texture);
            SetMaterialTextureIfPresent(material, "_BaseMap", texture);
            SetMaterialTextureIfPresent(material, "_MainTex", texture);
            SetMaterialTextureIfPresent(material, "_MainTexture", texture);
            SetMaterialColorIfPresent(material, "_BaseColor", color);
            SetMaterialColorIfPresent(material, "_Color", color);
            SetMaterialColorIfPresent(material, "_EmissionColor", Color.white);
        }

        private static Shader FindParticleShader()
        {
            return Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Universal Render Pipeline/Particles/Unlit")
                ?? Shader.Find("Unlit/Color")
                ?? Shader.Find("Sprites/Default");
        }

        private static void SetMaterialFloatIfPresent(Material material, string propertyName, float value)
        {
            if (material.HasProperty(propertyName))
            {
                material.SetFloat(propertyName, value);
            }
        }

        private static void SetMaterialTextureIfPresent(Material material, string propertyName, Texture texture)
        {
            if (material.HasProperty(propertyName))
            {
                material.SetTexture(propertyName, texture);
            }
        }

        private static void SetMaterialColorIfPresent(Material material, string propertyName, Color color)
        {
            if (material.HasProperty(propertyName))
            {
                material.SetColor(propertyName, color);
            }
        }

        private static string SanitizeAssetFileName(string rawName)
        {
            if (string.IsNullOrWhiteSpace(rawName))
            {
                return "Asset";
            }

            char[] invalid = Path.GetInvalidFileNameChars();
            string safe = rawName.Trim();
            for (int i = 0; i < invalid.Length; i++)
            {
                safe = safe.Replace(invalid[i], '_');
            }

            return safe.Replace(' ', '_');
        }

        private static string ToProjectAbsolutePath(string assetPath)
        {
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string relativePath = assetPath.Replace('/', Path.DirectorySeparatorChar);
            return Path.Combine(projectRoot, relativePath);
        }

        private static void SetCue(SerializedProperty cue, CueDefinition definition)
        {
            SetRelativeEnum(cue, "cueId", definition.CueId);
            SetRelativeObject(cue, "prefab", definition.Prefab);
            SetRelativeVector3(cue, "localPositionOffset", definition.LocalPositionOffset);
            SetRelativeVector3(cue, "localEulerOffset", definition.LocalEulerOffset);
            SetRelativeVector3(cue, "localScale", definition.LocalScale);
            SetRelativeFloat(cue, "lifetimeSeconds", definition.LifetimeSeconds);
            SetRelativeInt(cue, "prewarmCount", 0);
            SetRelativeBool(cue, "parentToAnchor", definition.ParentToAnchor);
            SetRelativeBool(cue, "alignForwardToDirection", definition.AlignForwardToDirection);
        }

        private static void SetPatternCueOverride(SerializedProperty property, CombatPatternVfxCueOverride value)
        {
            SetRelativeObject(property, "patternProfile", value.PatternProfile);
            SetRelativeEnum(property, "windupCueId", value.WindupCueId);
            SetRelativeEnum(property, "attackActiveCueId", value.AttackActiveCueId);
            SetRelativeFloat(property, "windupIntensity", value.WindupIntensity);
            SetRelativeFloat(property, "attackActiveIntensity", value.AttackActiveIntensity);
        }

        private static void SetEliteCueOverride(SerializedProperty property, CombatEliteVfxCueOverride value)
        {
            SetRelativeObject(property, "eliteProfile", value.EliteProfile);
            SetRelativeEnum(property, "signalCueId", value.SignalCueId);
            SetRelativeFloat(property, "intensity", value.Intensity);
        }

        private static T LoadOrCreate<T>(string assetPath) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset != null)
            {
                return asset;
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, assetPath);
            return asset;
        }

        private static CombatAiPatternProfile LoadPattern(string assetPath)
        {
            CombatAiPatternProfile profile = AssetDatabase.LoadAssetAtPath<CombatAiPatternProfile>(assetPath);
            if (profile == null)
            {
                throw new InvalidOperationException($"Missing pattern profile at {assetPath}.");
            }

            return profile;
        }

        private static CombatAiElitePatternProfile LoadElite(string assetPath)
        {
            CombatAiElitePatternProfile profile = AssetDatabase.LoadAssetAtPath<CombatAiElitePatternProfile>(assetPath);
            if (profile == null)
            {
                throw new InvalidOperationException($"Missing elite pattern profile at {assetPath}.");
            }

            return profile;
        }

        private static BasicSoldierEnemy[] CollectSoldiers(GameObject[] roots)
        {
            var soldiers = new System.Collections.Generic.List<BasicSoldierEnemy>();
            for (int i = 0; i < roots.Length; i++)
            {
                soldiers.AddRange(roots[i].GetComponentsInChildren<BasicSoldierEnemy>(includeInactive: true));
            }

            return soldiers.ToArray();
        }

        private static GameObject EnsureRoot(Scene scene, string name)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i].name == name)
                {
                    return roots[i];
                }
            }

            GameObject root = new GameObject(name);
            SceneManager.MoveGameObjectToScene(root, scene);
            return root;
        }

        private static Transform EnsureChild(Transform parent, string name, Vector3 localPosition)
        {
            Transform child = parent.Find(name);
            if (child == null)
            {
                child = new GameObject(name).transform;
                child.SetParent(parent, worldPositionStays: false);
            }

            child.localPosition = localPosition;
            child.localRotation = Quaternion.identity;
            child.localScale = Vector3.one;
            EditorUtility.SetDirty(child.gameObject);
            return child;
        }

        private static T RequireObject<T>(GameObject[] roots, string label) where T : Component
        {
            for (int i = 0; i < roots.Length; i++)
            {
                T component = roots[i].GetComponentInChildren<T>(true);
                if (component != null)
                {
                    return component;
                }
            }

            throw new InvalidOperationException($"Missing required {label}.");
        }

        private static T RequireComponent<T>(GameObject owner, string label) where T : Component
        {
            if (owner.TryGetComponent(out T component))
            {
                return component;
            }

            throw new InvalidOperationException($"Missing required {label}.");
        }

        private static T EnsureComponent<T>(GameObject owner) where T : Component
        {
            if (!owner.TryGetComponent(out T component))
            {
                component = owner.AddComponent<T>();
            }

            return component;
        }

        private static void SetObjectReference(UnityEngine.Object target, string propertyName, UnityEngine.Object value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = RequireProperty(serializedObject, propertyName);
            property.objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetEnum(UnityEngine.Object target, string propertyName, int value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = RequireProperty(serializedObject, propertyName);
            property.enumValueIndex = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetFloat(UnityEngine.Object target, string propertyName, float value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = RequireProperty(serializedObject, propertyName);
            property.floatValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetBool(UnityEngine.Object target, string propertyName, bool value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = RequireProperty(serializedObject, propertyName);
            property.boolValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void ValidateFloat(UnityEngine.Object target, string propertyName, float expected)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = RequireProperty(serializedObject, propertyName);
            if (!Mathf.Approximately(property.floatValue, expected))
            {
                throw new InvalidOperationException(
                    $"{target.name}.{propertyName} expected {expected}, found {property.floatValue}.");
            }
        }

        private static void SetRelativeEnum<TEnum>(SerializedProperty property, string propertyName, TEnum value) where TEnum : Enum
        {
            SerializedProperty relative = property.FindPropertyRelative(propertyName);
            relative.enumValueIndex = Convert.ToInt32(value);
        }

        private static void SetRelativeObject(SerializedProperty property, string propertyName, UnityEngine.Object value)
        {
            property.FindPropertyRelative(propertyName).objectReferenceValue = value;
        }

        private static void SetRelativeVector3(SerializedProperty property, string propertyName, Vector3 value)
        {
            property.FindPropertyRelative(propertyName).vector3Value = value;
        }

        private static void SetRelativeFloat(SerializedProperty property, string propertyName, float value)
        {
            property.FindPropertyRelative(propertyName).floatValue = value;
        }

        private static void SetRelativeInt(SerializedProperty property, string propertyName, int value)
        {
            property.FindPropertyRelative(propertyName).intValue = value;
        }

        private static void SetRelativeBool(SerializedProperty property, string propertyName, bool value)
        {
            property.FindPropertyRelative(propertyName).boolValue = value;
        }

        private static SerializedProperty RequireProperty(SerializedObject serializedObject, string propertyName)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                throw new InvalidOperationException($"{serializedObject.targetObject.name} is missing serialized property {propertyName}.");
            }

            return property;
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            int separatorIndex = folderPath.LastIndexOf('/');
            string parent = folderPath.Substring(0, separatorIndex);
            string name = folderPath.Substring(separatorIndex + 1);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }

        private struct CueDefinition
        {
            public CueDefinition(
                CombatVfxCueId cueId,
                GameObject prefab,
                Vector3 localPositionOffset,
                Vector3 localEulerOffset,
                Vector3 localScale,
                float lifetimeSeconds,
                bool parentToAnchor,
                bool alignForwardToDirection)
            {
                CueId = cueId;
                Prefab = prefab;
                LocalPositionOffset = localPositionOffset;
                LocalEulerOffset = localEulerOffset;
                LocalScale = localScale;
                LifetimeSeconds = lifetimeSeconds;
                ParentToAnchor = parentToAnchor;
                AlignForwardToDirection = alignForwardToDirection;
            }

            public CombatVfxCueId CueId { get; }
            public GameObject Prefab { get; }
            public Vector3 LocalPositionOffset { get; }
            public Vector3 LocalEulerOffset { get; }
            public Vector3 LocalScale { get; }
            public float LifetimeSeconds { get; }
            public bool ParentToAnchor { get; }
            public bool AlignForwardToDirection { get; }
        }

        private struct CombatCuePrefabs
        {
            public GameObject PlayerAttackStart;
            public GameObject PlayerAttackHit;
            public GameObject PlayerDodgeStart;
            public GameObject PlayerPerfectDodgeTimeField;
            public GameObject PlayerPerfectDodgePulsewave;
            public GameObject PlayerPerfectDodgeHoloCube;
            public GameObject PlayerPerfectDodgeWindow;
            public GameObject PlayerPerfectDodgeShieldBlockImpact;
            public GameObject PlayerSummonPreSpawnPortal;
            public GameObject PlayerSummonLandingCrater;
            public GameObject PlayerSummonDragonBreathAudio;
            public GameObject PlayerRangedMuzzleFlash;
            public GameObject PlayerRangedProjectileImpact;
            public GameObject EnemyWindup;
            public GameObject EnemyAttackActive;
            public GameObject EnemyHit;
            public GameObject EnemyDeath;
            public GameObject ClosePunishWindup;
            public GameObject ClosePunishActive;
            public GameObject LungeWindup;
            public GameObject LungeActive;
            public GameObject HeavyWindup;
            public GameObject HeavyActive;
            public GameObject LineWindup;
            public GameObject LineActive;
            public GameObject FanWindup;
            public GameObject FanActive;
            public GameObject RetreatShotWindup;
            public GameObject RetreatShotActive;
            public GameObject RetreatBlinkWindup;
            public GameObject RetreatBlinkActive;
            public GameObject GuardBreakWindup;
            public GameObject GuardBreakActive;
            public GameObject EliteShield;
            public GameObject EliteArmorBreak;
            public GameObject EliteAura;
            public GameObject EliteSummon;
            public GameObject SummonFollowupWindow;
            public GameObject SummonBlockOpportunity;
            public GameObject ElitePhaseSwap;
            public GameObject SummonProjectileIntercept;
        }
    }
}
