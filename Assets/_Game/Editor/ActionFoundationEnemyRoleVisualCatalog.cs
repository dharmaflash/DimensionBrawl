using System;
using System.Collections.Generic;
using System.Linq;
using DimensionBrawl.Enemies;
using DimensionBrawl.Presentation;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace DimensionBrawl.Editor
{
    internal static partial class ActionFoundationEnemyRoleVisualSetup
    {
        private static EnemyRoleVisualSpec CommonWeaponRole(
            string roleId,
            string roleTag,
            string modelFolder,
            string sourceModelPath,
            string sourcePrefabPath,
            string defaultMaterialPath,
            string weaponAnimationTag,
            RoleWeaponSpec[] weapons,
            Vector3 scale,
            string visualRead,
            string animationRead,
            TelegraphStyle telegraphStyle)
        {
            return CreateBaseSpec(
                roleId,
                roleTag,
                modelFolder,
                sourceModelPath,
                sourcePrefabPath,
                new[] { defaultMaterialPath },
                CommonAnimationClips(roleTag, weaponAnimationTag),
                weapons,
                scale,
                visualRead,
                animationRead,
                telegraphStyle);
        }

        private static EnemyRoleVisualSpec AranianRole(
            string roleId,
            string roleTag,
            Vector3 scale,
            string visualRead,
            string animationRead,
            TelegraphStyle telegraphStyle)
        {
            return CreateBaseSpec(
                roleId,
                roleTag,
                "Aranian",
                ShooterRoot + "/Aranian/FBX files/SK_Aranian.fbx",
                ShooterRoot + "/Aranian/Prefabs/Aranian.prefab",
                new[]
                {
                    ShooterRoot + "/Aranian/Materials/M_Aranian.mat",
                    ShooterRoot + "/Aranian/Materials/M_AranianEquipment.mat"
                },
                AranianSwordClips(roleTag),
                new[]
                {
                    new RoleWeaponSpec("PistoBlade", ShooterRoot + "/Aranian/FBX files/SM_PistoBlade.fbx", ShooterRoot + "/Aranian/Materials/M_LaserBlade.mat", "RefPos_LaserPistoBlade"),
                    new RoleWeaponSpec("EnergyShield", ShooterRoot + "/Aranian/FBX files/SM_ForceShield.fbx", ShooterRoot + "/Aranian/Materials/M_EnergyShield.mat", "RefPos_EnergyShield")
                },
                scale,
                visualRead,
                animationRead,
                telegraphStyle);
        }

        private static EnemyRoleVisualSpec AranianPistolRole(
            string roleId,
            string roleTag,
            Vector3 scale,
            string visualRead,
            string animationRead,
            TelegraphStyle telegraphStyle)
        {
            return CreateBaseSpec(
                roleId,
                roleTag,
                "Aranian",
                ShooterRoot + "/Aranian/FBX files/SK_Aranian.fbx",
                ShooterRoot + "/Aranian/Prefabs/Aranian.prefab",
                new[]
                {
                    ShooterRoot + "/Aranian/Materials/M_Aranian.mat",
                    ShooterRoot + "/Aranian/Materials/M_AranianEquipment.mat"
                },
                AranianPistolClips(roleTag),
                new[] { new RoleWeaponSpec("PistoBlade", ShooterRoot + "/Aranian/FBX files/SM_PistoBlade.fbx", ShooterRoot + "/Aranian/Materials/M_LaserBlade.mat", "RefPos_LaserPistoBlade") },
                scale,
                visualRead,
                animationRead,
                telegraphStyle);
        }

        private static EnemyRoleVisualSpec TherionideRole(
            string roleId,
            string roleTag,
            Vector3 scale,
            string visualRead,
            string animationRead,
            TelegraphStyle telegraphStyle)
        {
            return CreateBaseSpec(
                roleId,
                roleTag,
                "Therionide",
                ShooterRoot + "/Therionide/FBX files/SK_Therionide.fbx",
                ShooterRoot + "/Therionide/Prefab/Therionide.prefab",
                new[]
                {
                    ShooterRoot + "/Therionide/Materials/M_Therionide.mat",
                    ShooterRoot + "/Therionide/Materials/M_TherionideWeapon.mat",
                    ShooterRoot + "/Therionide/Materials/M_Laser.mat"
                },
                TherionideClips(roleTag),
                Array.Empty<RoleWeaponSpec>(),
                scale,
                visualRead,
                animationRead,
                telegraphStyle);
        }

        private static EnemyRoleVisualSpec MaintenanceWorkerRole(
            string roleId,
            string roleTag,
            Vector3 scale,
            string visualRead,
            string animationRead,
            TelegraphStyle telegraphStyle,
            bool createSummonIntentAnchor)
        {
            EnemyRoleVisualSpec spec = CreateBaseSpec(
                roleId,
                roleTag,
                "MaintenanceWorker",
                ShooterRoot + "/MaintenanceWorker/FBX Files/SK_MaintenanceWorkerAllMeshes.fbx",
                ShooterRoot + "/MaintenanceWorker/Prefabs/MaintenanceWorker@Gastarian_Orange Variant.prefab",
                new[]
                {
                    ShooterRoot + "/MaintenanceWorker/Materials/M_MaintenanceWorkerOutfit_Orange.mat",
                    ShooterRoot + "/MaintenanceWorker/Materials/M_GastarianHead.mat"
                },
                MaintenanceWorkerClips(roleTag),
                Array.Empty<RoleWeaponSpec>(),
                scale,
                visualRead,
                animationRead,
                telegraphStyle);
            spec.CreateSummonIntentAnchor = createSummonIntentAnchor;
            return spec;
        }

        private static EnemyRoleVisualSpec HeavyArmorRole(
            string roleId,
            string roleTag,
            Vector3 scale,
            string visualRead,
            string animationRead,
            TelegraphStyle telegraphStyle)
        {
            return CreateBaseSpec(
                roleId,
                roleTag,
                "SciFiHeavyBattleArmor",
                ShooterRoot + "/SciFiHeavyBattleArmor/FBX files/SK_SciFiHeavyBattleArmor.fbx",
                ShooterRoot + "/SciFiHeavyBattleArmor/Prefabs/SciFiHeavyBattleArmor_Commando Variant.prefab",
                new[] { ShooterRoot + "/SciFiHeavyBattleArmor/Materials/M_HeavyBattleArmor_Commando.mat" },
                HeavyArmorClips(roleTag),
                Array.Empty<RoleWeaponSpec>(),
                scale,
                visualRead,
                animationRead,
                telegraphStyle);
        }

        private static EnemyRoleVisualSpec CreateBaseSpec(
            string roleId,
            string roleTag,
            string modelFolder,
            string sourceModelPath,
            string sourcePrefabPath,
            string[] defaultMaterialPaths,
            RoleAnimationClipSpec[] clips,
            RoleWeaponSpec[] weapons,
            Vector3 scale,
            string visualRead,
            string animationRead,
            TelegraphStyle telegraphStyle)
        {
            string roleRoot = $"{PromotedCharacterRoot}/RoleVariants/{roleTag}";
            return new EnemyRoleVisualSpec
            {
                RoleId = roleId,
                RoleTag = roleTag,
                VisualName = RoleVisualPrefix + roleTag,
                SourceModelPath = sourceModelPath,
                SourcePrefabPath = sourcePrefabPath,
                TargetModelPath = $"{roleRoot}/Models/SK_{roleTag}_{modelFolder}.fbx",
                MaterialRoot = $"{roleRoot}/Materials",
                TextureRoot = $"{roleRoot}/Textures",
                AnimationRoot = $"{PromotedAnimationRoot}/{roleTag}",
                ControllerPath = $"{PromotedAnimationRoot}/{roleTag}/DB_{roleTag}_Role.controller",
                DefaultMaterialPaths = defaultMaterialPaths,
                Clips = clips,
                Weapons = weapons ?? Array.Empty<RoleWeaponSpec>(),
                VisualScale = scale,
                VisualRead = visualRead,
                AnimationRead = animationRead,
                Telegraph = CreateTelegraphSpec(telegraphStyle)
            };
        }

        private static RoleWeaponSpec CommonWeapon(string modelFile, string materialFile, string name, string socketName)
        {
            return new RoleWeaponSpec(
                name,
                $"{CommonWeaponRoot}/FBX Files/{modelFile}",
                $"{CommonWeaponRoot}/Materials/{materialFile}",
                socketName);
        }

        private static RoleAnimationClipSpec[] CommonAnimationClips(string roleTag, string weaponTag)
        {
            return new[]
            {
                Clip("Idle", $"Humanoid@IdleAim{weaponTag}.FBX", roleTag, true),
                Clip("Run", $"Humanoid@RunForward{weaponTag}.FBX", roleTag, true),
                Clip("Walk", $"Humanoid@WalkForward{weaponTag}.FBX", roleTag, true),
                Clip("Attack", $"Humanoid@ShootPrimary{weaponTag}.FBX", roleTag, false),
                Clip("AttackCombo2", $"Humanoid@2HitCombo{weaponTag}.FBX", roleTag, false),
                Clip("AttackCombo3", $"Humanoid@ShootSecondary{weaponTag}.FBX", roleTag, false),
                Clip("AttackHeavy", $"Humanoid@2HitCombo{weaponTag}.FBX", roleTag, false, 0.78f),
                Clip("AttackLinePressure", $"Humanoid@ShootPrimary{weaponTag}.FBX", roleTag, false, 0.92f),
                Clip("AttackFanPressure", $"Humanoid@ShootSecondary{weaponTag}.FBX", roleTag, false, 0.96f),
                Clip("AttackRetreatShot", $"Humanoid@ShootPrimary{weaponTag}.FBX", roleTag, false, 1.08f),
                Clip("AttackRetreatBlink", $"Humanoid@CrouchForward{weaponTag}.FBX", roleTag, false, 1.18f),
                Clip("AttackGuardBreak", $"Humanoid@2HitCombo{weaponTag}.FBX", roleTag, false, 0.72f),
                Clip("RetreatBackstep", $"Humanoid@WalkForward{weaponTag}.FBX", roleTag, true, 1.16f),
                Clip("RetreatBlink", $"Humanoid@CrouchForward{weaponTag}.FBX", roleTag, false, 1.38f),
                Clip("EliteShieldCycle", $"Humanoid@CrouchAim{weaponTag}.FBX", roleTag, false, 0.9f),
                Clip("EliteArmorBreak", "Humanoid@GetHitFrontHeavyUnarmed.FBX", roleTag, false),
                Clip("EliteAuraBuffer", $"Humanoid@CrouchAim{weaponTag}.FBX", roleTag, false, 0.84f),
                Clip("EliteSummonPackage", $"Humanoid@CrouchAim{weaponTag}.FBX", roleTag, false, 0.78f),
                Clip("ElitePhaseSwap", $"Humanoid@CrouchForward{weaponTag}.FBX", roleTag, false, 0.92f),
                Clip("Hit", "Humanoid@GetHitFrontLightUnarmed.FBX", roleTag, false),
                Clip("HitHeavy", "Humanoid@GetHitFrontHeavyUnarmed.FBX", roleTag, false),
                Clip("Death", "Humanoid@DeathFrontUnarmed.FBX", roleTag, false, 1f, true)
            };
        }

        private static RoleAnimationClipSpec[] AranianSwordClips(string roleTag)
        {
            string root = ShooterRoot + "/Aranian/FBX files";
            return WithSourceRoot(root, new[]
            {
                Clip("Idle", "Aranian@IdleSwordAndShield.fbx", roleTag, true),
                Clip("Run", "Aranian@RunForwardSwordAndShield.fbx", roleTag, true),
                Clip("Walk", "Aranian@WalkForwardSwordAndShield.fbx", roleTag, true),
                Clip("Attack", "Aranian@2HitComboSwordAndShield.fbx", roleTag, false),
                Clip("AttackCombo2", "Aranian@3HitComboSwordAndShield.fbx", roleTag, false),
                Clip("AttackCombo3", "Aranian@SpinningAttackSwordAndShield.fbx", roleTag, false),
                Clip("AttackHeavy", "Aranian@SpinningAttackSwordAndShield.fbx", roleTag, false, 0.74f),
                Clip("AttackLinePressure", "Aranian@DashForwardSwordAndShield.fbx", roleTag, false, 1.08f),
                Clip("AttackFanPressure", "Aranian@SpinningAttackSwordAndShield.fbx", roleTag, false, 0.88f),
                Clip("AttackRetreatShot", "Aranian@DashForwardSwordAndShield.fbx", roleTag, false, 1.12f),
                Clip("AttackRetreatBlink", "Aranian@DashForwardSwordAndShield.fbx", roleTag, false, 1.22f),
                Clip("AttackGuardBreak", "Aranian@BlockedSwordAndShield.fbx", roleTag, false, 0.76f),
                Clip("RetreatBackstep", "Aranian@WalkBackwardsSwordAndShield.fbx", roleTag, true, 1.1f),
                Clip("RetreatBlink", "Aranian@DashForwardSwordAndShield.fbx", roleTag, false, 1.35f),
                Clip("EliteShieldCycle", "Aranian@BlockedSwordAndShield.fbx", roleTag, false),
                Clip("EliteArmorBreak", "Aranian@GetHitFrontHeavySwordAndShield.fbx", roleTag, false),
                Clip("EliteAuraBuffer", "Aranian@DrawSwordAndShield.fbx", roleTag, false),
                Clip("EliteSummonPackage", "Aranian@DrawSwordAndShield.fbx", roleTag, false, 0.86f),
                Clip("ElitePhaseSwap", "Aranian@Turn90RightSwordAndShield.fbx", roleTag, false),
                Clip("Hit", "Aranian@GetHitFrontLightSwordAndShield.fbx", roleTag, false),
                Clip("HitHeavy", "Aranian@GetHitFrontHeavySwordAndShield.fbx", roleTag, false),
                Clip("Death", "Aranian@DeathFrontSwordAndShield.fbx", roleTag, false, 1f, true)
            });
        }

        private static RoleAnimationClipSpec[] AranianPistolClips(string roleTag)
        {
            string root = ShooterRoot + "/Aranian/FBX files";
            return WithSourceRoot(root, new[]
            {
                Clip("Idle", "Aranian@IdleAimingPistol.fbx", roleTag, true),
                Clip("Run", "Aranian@RunForwardAimingPistol.fbx", roleTag, true),
                Clip("Walk", "Aranian@WalkForwardAimingPistol.fbx", roleTag, true),
                Clip("Attack", "Aranian@DashForwardSwordAndShield.fbx", roleTag, false),
                Clip("AttackCombo2", "Aranian@2HitComboACombat.fbx", roleTag, false),
                Clip("AttackCombo3", "Aranian@3HitComboCombat.fbx", roleTag, false),
                Clip("AttackHeavy", "Aranian@4HitComboCombat.fbx", roleTag, false, 0.78f),
                Clip("AttackLinePressure", "Aranian@RunForwardAimingPistol.fbx", roleTag, false, 1.1f),
                Clip("AttackFanPressure", "Aranian@2HitComboBCombat.fbx", roleTag, false),
                Clip("AttackRetreatShot", "Aranian@DashForwardSwordAndShield.fbx", roleTag, false, 1.2f),
                Clip("AttackRetreatBlink", "Aranian@DashForwardSwordAndShield.fbx", roleTag, false, 1.32f),
                Clip("AttackGuardBreak", "Aranian@4HitComboCombat.fbx", roleTag, false, 0.75f),
                Clip("RetreatBackstep", "Aranian@RunBackwardsCombat.fbx", roleTag, true, 1.12f),
                Clip("RetreatBlink", "Aranian@DashForwardSwordAndShield.fbx", roleTag, false, 1.38f),
                Clip("EliteShieldCycle", "Aranian@CrouchIdlePistol.fbx", roleTag, false),
                Clip("EliteArmorBreak", "Aranian@GetHitFrontHeavyPistol.fbx", roleTag, false),
                Clip("EliteAuraBuffer", "Aranian@CrouchIdlePistol.fbx", roleTag, false),
                Clip("EliteSummonPackage", "Aranian@CrouchIdlePistol.fbx", roleTag, false),
                Clip("ElitePhaseSwap", "Aranian@DashForwardSwordAndShield.fbx", roleTag, false),
                Clip("Hit", "Aranian@GetHitFrontLightPistol.fbx", roleTag, false),
                Clip("HitHeavy", "Aranian@GetHitFrontHeavyPistol.fbx", roleTag, false),
                Clip("Death", "Aranian@DeathFrontPistol.fbx", roleTag, false, 1f, true)
            });
        }

        private static RoleAnimationClipSpec[] TherionideClips(string roleTag)
        {
            string root = ShooterRoot + "/Therionide/FBX files";
            return WithSourceRoot(root, new[]
            {
                Clip("Idle", "Therionide@IdleMelee.fbx", roleTag, true),
                Clip("Run", "Therionide@RunMelee.fbx", roleTag, true),
                Clip("Walk", "Therionide@WalkMelee.fbx", roleTag, true),
                Clip("Attack", "Therionide@Attack1Forward.fbx", roleTag, false),
                Clip("AttackCombo2", "Therionide@Attack2Forward.fbx", roleTag, false),
                Clip("AttackCombo3", "Therionide@Attack3Forward.fbx", roleTag, false),
                Clip("AttackHeavy", "Therionide@Attack4.fbx", roleTag, false, 0.82f),
                Clip("AttackLinePressure", "Therionide@Attack1Forward.fbx", roleTag, false, 1.12f),
                Clip("AttackFanPressure", "Therionide@Attack3.fbx", roleTag, false),
                Clip("AttackRetreatShot", "Therionide@Attack2Forward.fbx", roleTag, false, 1.08f),
                Clip("AttackRetreatBlink", "Therionide@Attack1Forward.fbx", roleTag, false, 1.3f),
                Clip("AttackGuardBreak", "Therionide@Attack4.fbx", roleTag, false, 0.78f),
                Clip("RetreatBackstep", "Therionide@WalkBackwardsMelee.fbx", roleTag, true, 1.16f),
                Clip("RetreatBlink", "Therionide@RunMelee.fbx", roleTag, true, 1.28f),
                Clip("EliteShieldCycle", "Therionide@IdleMeleeCrouching.fbx", roleTag, false),
                Clip("EliteArmorBreak", "Therionide@GetHit2Melee.fbx", roleTag, false),
                Clip("EliteAuraBuffer", "Therionide@IdleMeleeCrouching.fbx", roleTag, false),
                Clip("EliteSummonPackage", "Therionide@IdleMeleeCrouching.fbx", roleTag, false),
                Clip("ElitePhaseSwap", "Therionide@Attack4.fbx", roleTag, false, 0.9f),
                Clip("Hit", "Therionide@GetHit1Melee.fbx", roleTag, false),
                Clip("HitHeavy", "Therionide@GetHit2Melee.fbx", roleTag, false),
                Clip("Death", "Therionide@DeathMelee.fbx", roleTag, false, 1f, true)
            });
        }

        private static RoleAnimationClipSpec[] MaintenanceWorkerClips(string roleTag)
        {
            string root = ShooterRoot + "/MaintenanceWorker/FBX Files";
            return WithSourceRoot(root, new[]
            {
                Clip("Idle", "MaintenanceWorker@IdleCombat.fbx", roleTag, true),
                Clip("Run", "MaintenanceWorker@RunCombat.fbx", roleTag, true),
                Clip("Walk", "MaintenanceWorker@WalkCombat.fbx", roleTag, true),
                Clip("Attack", "MaintenanceWorker@RepairHigh.fbx", roleTag, false),
                Clip("AttackCombo2", "MaintenanceWorker@RepairLow.fbx", roleTag, false),
                Clip("AttackCombo3", "MaintenanceWorker@TypeOnConsole.fbx", roleTag, false),
                Clip("AttackHeavy", "MaintenanceWorker@3HitComboCombat.fbx", roleTag, false, 0.8f),
                Clip("AttackLinePressure", "MaintenanceWorker@TypeOnConsole.fbx", roleTag, false, 0.92f),
                Clip("AttackFanPressure", "MaintenanceWorker@RepairHigh.fbx", roleTag, false),
                Clip("AttackRetreatShot", "MaintenanceWorker@TypeOnConsole.fbx", roleTag, false, 1.05f),
                Clip("AttackRetreatBlink", "MaintenanceWorker@CrouchForward.fbx", roleTag, false, 1.24f),
                Clip("AttackGuardBreak", "MaintenanceWorker@3HitComboCombat.fbx", roleTag, false, 0.76f),
                Clip("RetreatBackstep", "MaintenanceWorker@WalkCombat.fbx", roleTag, true, 1.12f),
                Clip("RetreatBlink", "MaintenanceWorker@CrouchForward.fbx", roleTag, false, 1.34f),
                Clip("EliteShieldCycle", "MaintenanceWorker@RepairLow.fbx", roleTag, false),
                Clip("EliteArmorBreak", "MaintenanceWorker@GetHitFrontHeavy.fbx", roleTag, false),
                Clip("EliteAuraBuffer", "MaintenanceWorker@RepairHigh.fbx", roleTag, false, 0.88f),
                Clip("EliteSummonPackage", "MaintenanceWorker@TypeOnConsole.fbx", roleTag, false, 0.82f),
                Clip("ElitePhaseSwap", "MaintenanceWorker@CrouchForward.fbx", roleTag, false),
                Clip("Hit", "MaintenanceWorker@GetHitFrontLight.fbx", roleTag, false),
                Clip("HitHeavy", "MaintenanceWorker@GetHitFrontHeavy.fbx", roleTag, false),
                Clip("Death", "MaintenanceWorker@DeathFront.fbx", roleTag, false, 1f, true)
            });
        }

        private static RoleAnimationClipSpec[] HeavyArmorClips(string roleTag)
        {
            string root = ShooterRoot + "/SciFiHeavyBattleArmor/FBX files";
            return WithSourceRoot(root, new[]
            {
                Clip("Idle", "SciFiHeavyBattleArmor@IdleWeapon.fbx", roleTag, true),
                Clip("Run", "SciFiHeavyBattleArmor@RunWeapon.fbx", roleTag, true),
                Clip("Walk", "SciFiHeavyBattleArmor@WalkWeapon.fbx", roleTag, true),
                Clip("Attack", "SciFiHeavyBattleArmor@MeleeAttackWeapon.fbx", roleTag, false),
                Clip("AttackCombo2", "SciFiHeavyBattleArmor@MeleeAttackForwardWeapon.fbx", roleTag, false),
                Clip("AttackCombo3", "SciFiHeavyBattleArmor@ShootStanding.fbx", roleTag, false),
                Clip("AttackHeavy", "SciFiHeavyBattleArmor@MeleeAttackForwardWeapon.fbx", roleTag, false, 0.72f),
                Clip("AttackLinePressure", "SciFiHeavyBattleArmor@ShootStanding.fbx", roleTag, false, 0.9f),
                Clip("AttackFanPressure", "SciFiHeavyBattleArmor@ShootStanding.fbx", roleTag, false, 0.94f),
                Clip("AttackRetreatShot", "SciFiHeavyBattleArmor@ShootStanding.fbx", roleTag, false, 1.04f),
                Clip("AttackRetreatBlink", "SciFiHeavyBattleArmor@CrouchWeapon.fbx", roleTag, false, 1.18f),
                Clip("AttackGuardBreak", "SciFiHeavyBattleArmor@MeleeAttackForwardWeapon.fbx", roleTag, false, 0.68f),
                Clip("RetreatBackstep", "SciFiHeavyBattleArmor@WalkWeapon.fbx", roleTag, true, 1.08f),
                Clip("RetreatBlink", "SciFiHeavyBattleArmor@CrouchWeapon.fbx", roleTag, false, 1.28f),
                Clip("EliteShieldCycle", "SciFiHeavyBattleArmor@CrouchWeapon.fbx", roleTag, false),
                Clip("EliteArmorBreak", "SciFiHeavyBattleArmor@GetHitBackWeapon.fbx", roleTag, false),
                Clip("EliteAuraBuffer", "SciFiHeavyBattleArmor@DrawWeaponStanding.fbx", roleTag, false, 0.88f),
                Clip("EliteSummonPackage", "SciFiHeavyBattleArmor@DrawWeaponStanding.fbx", roleTag, false, 0.82f),
                Clip("ElitePhaseSwap", "SciFiHeavyBattleArmor@Turn90RightWeapon.fbx", roleTag, false),
                Clip("Hit", "SciFiHeavyBattleArmor@GetHitFrontWeapon.fbx", roleTag, false),
                Clip("HitHeavy", "SciFiHeavyBattleArmor@GetHitBackWeapon.fbx", roleTag, false),
                Clip("Death", "SciFiHeavyBattleArmor@DeathFrontWeapon.fbx", roleTag, false, 1f, true)
            });
        }

        private static RoleAnimationClipSpec[] WithSourceRoot(string sourceRoot, RoleAnimationClipSpec[] clips)
        {
            for (int i = 0; i < clips.Length; i++)
            {
                clips[i].SourceRoot = sourceRoot;
            }

            return clips;
        }

        private static RoleAnimationClipSpec Clip(string key, string sourceFileName, string roleTag, bool loopTime, float speed = 1f, bool heightFromFeet = false)
        {
            return new RoleAnimationClipSpec
            {
                Key = key,
                SourceRoot = CommonAnimationRoot,
                SourceFileName = sourceFileName,
                TargetClipName = roleTag + "_" + key,
                LoopTime = loopTime,
                Speed = speed,
                HeightFromFeet = heightFromFeet
            };
        }
    }
}
