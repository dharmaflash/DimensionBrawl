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
        public const string RoleVisualPrefix = "RoleVisual_";
        public const string RejectedRolePresentationMarkerName = "RolePresentationMarker";
        public const string RejectedRoleStaticVisualName = "RoleStaticVisual";
        public const string RejectedRoleSummonSignalName = "RoleSummonSignal";

        private const string ProtofactorRoot = "Assets/_Imported/AssetStore/Protofactor/Sci Fi";
        private const string ShooterRoot = ProtofactorRoot + "/SciFiCharactersMegaPackVol3/SciFiShooterCharactersPackVol3";
        private const string CommonAnimationRoot = ProtofactorRoot + "/Common/Animations";
        private const string CommonWeaponRoot = ProtofactorRoot + "/Common/Weapons";
        private const string PromotedCharacterRoot = "Assets/_Game/Art/Characters/Enemies/SciFiSoldiers";
        private const string PromotedAnimationRoot = "Assets/_Game/Art/Animations/Enemies/SciFiSoldiers/RoleVariants";
        private const string PromotedWeaponRoot = "Assets/_Game/Art/Characters/Enemies/SciFiSoldiers/RoleWeapons";
        private const string CanonicalSciFiSoldierModelPath =
            "Assets/_Game/Art/Characters/Enemies/SciFiSoldiers/SciFiSoldier01/Models/SK_SciFiSoldier01.fbx";
        private const string CanonicalSciFiSoldierPrefabPath =
            "Assets/_Game/Prefabs/Enemies/ActionFoundation/PF_SciFiSoldier01_CommandoVisual.prefab";
        private const string CanonicalSciFiSoldierMaterialPath =
            "Assets/_Game/Art/Characters/Enemies/SciFiSoldiers/SciFiSoldier01/Materials/M_SciFiSoldier_01.mat";
        private const string CanonicalSciFiSoldierMaterialRoot =
            "Assets/_Game/Art/Characters/Enemies/SciFiSoldiers/SciFiSoldier01/Materials";
        private const string CanonicalSciFiSoldierTextureRoot =
            "Assets/_Game/Art/Characters/Enemies/SciFiSoldiers/SciFiSoldier01/Textures";
        private const string SummonSignalName = "SummonIntentAnchor";

        private static readonly string[] VisualChildNamesToRemove =
        {
            RejectedRolePresentationMarkerName,
            RejectedRoleStaticVisualName,
            RejectedRoleSummonSignalName,
            "MaintenanceWorker_BasicSoldierVisual",
            ActionFoundationSciFiSoldier01VisualSetup.VisualName,
            ActionFoundationSciFiEliteSoldierVisualSetup.VisualName
        };

        public static EnemyRoleVisualSpec CreateForRole(string roleId)
        {
            return roleId switch
            {
                "SciFiSoldier.EntryProbe" => CommonWeaponRole(
                    roleId,
                    "EntryProbe",
                    "SciFiMaleCombatSuit",
                    ShooterRoot + "/SciFiMaleCombatSuit/FBX Files/SK_SciFiMaleCombatSuit.fbx",
                    ShooterRoot + "/SciFiMaleCombatSuit/Prefabs/SciFiMaleCombatSuit_Commando.prefab",
                    ShooterRoot + "/SciFiMaleCombatSuit/Materials/M_SciFiMaleCombatSuit.mat",
                    "2Guns",
                    new[]
                    {
                        CommonWeapon("SM_SciFiSkorp-IO.FBX", "M_Skorp-IO.mat", "SkorpIO_Right", "RefPosSkorp-IORight_Action"),
                        CommonWeapon("SM_SciFiSkorp-IO.FBX", "M_Skorp-IO.mat", "SkorpIO_Left", "RefPosSkorp-IOLeft_Action")
                    },
                    new Vector3(0.98f, 0.98f, 0.98f),
                    "Male combat-suit scout with dual Skorp-IO pistols for a light first-read enemy.",
                    "Dual-pistol common humanoid set: quick aim idle, run, primary/secondary shots, crouch retreat, hit, death.",
                    TelegraphStyle.SmallRead),
                "SciFiSoldier.CloseGuard" => AranianRole(
                    roleId,
                    "CloseGuard",
                    new Vector3(1.04f, 1.04f, 1.04f),
                    "Aranian sword-and-shield guard with actual blade/shield props, not a recolored worker.",
                    "Aranian sword-and-shield set: shield idle/run, 2-hit, 3-hit, spinning attack, block, hit, death.",
                    TelegraphStyle.Guard),
                "SciFiSoldier.LungeChaser" => TherionideRole(
                    roleId,
                    "LungeChaser",
                    new Vector3(1.02f, 1.02f, 1.02f),
                    "Therionide melee alien for committed lunge pressure.",
                    "Therionide melee set: melee idle/run, forward attacks, hit, death.",
                    TelegraphStyle.Lunge),
                "SciFiSoldier.LineCaster" => CanonicalLineCasterRole(roleId),
                "SciFiSoldier.FanSuppressor" => CommonWeaponRole(
                    roleId,
                    "FanSuppressor",
                    "SciFiFemaleCombatSuit",
                    ShooterRoot + "/SciFiFemaleCombatSuit/FBX Files/SK_SciFiFemaleCombatSuit.fbx",
                    ShooterRoot + "/SciFiFemaleCombatSuit/Prefabs/SciFiFemaleCombatSuit_Commando Variant.prefab",
                    ShooterRoot + "/SciFiFemaleCombatSuit/Materials/M_FemaleCombatSuit_Commando.mat",
                    "Shotgun",
                    new[] { CommonWeapon("SM_SciFiShotgun.FBX", "M_SciFiShotgun.mat", "Shotgun", "RefPosShotgun_Action") },
                    Vector3.one,
                    "Female combat-suit shotgunner for wide cone suppression.",
                    "Shotgun common humanoid set: shotgun hold, primary/secondary blasts, fan-pressure read, hit, death.",
                    TelegraphStyle.Fan),
                "SciFiSoldier.BacklineShooter" => CommonWeaponRole(
                    roleId,
                    "BacklineShooter",
                    "Spikarian",
                    ShooterRoot + "/Spikarian/FBX Files/SK_Spikarian.fbx",
                    ShooterRoot + "/Spikarian/Prefabs/Spikarian_YellowArmor.prefab",
                    ShooterRoot + "/Spikarian/Materials/M_SpikarianArmor.mat",
                    "MissileLauncher",
                    new[] { CommonWeapon("SM_SciFiMissileLauncher.FBX", "M_SciFiMissileLauncher.mat", "MissileLauncher", "RefPosMissileLauncher_Action") },
                    new Vector3(1.03f, 1.03f, 1.03f),
                    "Spikarian missile backliner; the previous turret overlay is intentionally removed.",
                    "Missile-launcher common humanoid set for long windup, retreat shot, and line pressure.",
                    TelegraphStyle.Line),
                "SciFiSoldier.Skirmisher" => AranianPistolRole(
                    roleId,
                    "Skirmisher",
                    new Vector3(0.96f, 0.96f, 0.96f),
                    "Aranian pistol-blade skirmisher using dash/pistol reads instead of the CloseGuard shield set.",
                    "Aranian pistol/dash set: pistol idle/run, dash forward, pistol hit reactions, death.",
                    TelegraphStyle.Lunge),
                "SciFiSoldier.Elite.ShieldBreaker" => HeavyArmorRole(
                    roleId,
                    "ShieldBreakerElite",
                    new Vector3(1.08f, 1.08f, 1.08f),
                    "Heavy battle armor shield breaker with heavy melee/guard-break reads.",
                    "HeavyBattleArmor weapon set: weapon idle/run, melee forward, guard break, heavy hit, death.",
                    TelegraphStyle.EliteGuard),
                "SciFiSoldier.Elite.AuraCaptain" => CommonWeaponRole(
                    roleId,
                    "AuraCaptainElite",
                    "SciFiFemaleCombatSuit",
                    ShooterRoot + "/SciFiFemaleCombatSuit/FBX Files/SK_SciFiFemaleCombatSuit.fbx",
                    ShooterRoot + "/SciFiFemaleCombatSuit/Prefabs/SciFiFemaleCombatSuit_Commando Variant.prefab",
                    ShooterRoot + "/SciFiFemaleCombatSuit/Materials/M_FemaleCombatSuit_Commando.mat",
                    "BeamGun",
                    new[] { CommonWeapon("SM_SciFiLightingGun.FBX", "M_LaserGun_01.mat", "BeamGun", "RefPosLightningGun_Action") },
                    new Vector3(1.05f, 1.05f, 1.05f),
                    "Female combat-suit beam officer for support/aura pressure, visually separate from HeavyArmor.",
                    "Beam-gun common humanoid set plus elite signal states for aura and shield support.",
                    TelegraphStyle.EliteAura),
                "SciFiSoldier.Elite.SummonCaller" => MaintenanceWorkerRole(
                    roleId,
                    "SummonCallerElite",
                    new Vector3(1.04f, 1.04f, 1.04f),
                    "MaintenanceWorker tech caller using repair/type-console gestures; no runtime summon spawning.",
                    "MaintenanceWorker utility set: repair high/low, type-on-console, crouch, hit, death, elite signals.",
                    TelegraphStyle.EliteAura,
                    createSummonIntentAnchor: true),
                "SciFiSoldier.Elite.PhaseDuelist" => CommonWeaponRole(
                    roleId,
                    "PhaseDuelistElite",
                    "Spikarian",
                    ShooterRoot + "/Spikarian/FBX Files/SK_Spikarian.fbx",
                    ShooterRoot + "/Spikarian/Prefabs/Spikarian_BlueArmor Variant.prefab",
                    ShooterRoot + "/Spikarian/Materials/M_SpikarianArmor_2.mat",
                    "2HandedGun",
                    new[] { CommonWeapon("SM_SciFiLaserGun.FBX", "M_LaserGun_01.mat", "LaserGun", "RefPos2HandedGun_Action") },
                    new Vector3(1.02f, 1.02f, 1.02f),
                    "Spikarian laser duelist for phase-swap/duel pressure, not a recolored heavy armor.",
                    "Two-handed laser-gun common humanoid set with crouch/retreat and elite phase signal states.",
                    TelegraphStyle.EliteLine),
                "SciFiSoldier.Elite.FinalStandCommander" => HeavyArmorRole(
                    roleId,
                    "FinalStandCommanderElite",
                    new Vector3(1.14f, 1.14f, 1.14f),
                    "Largest heavy battle armor commander for final-stand pressure.",
                    "HeavyBattleArmor weapon set reused deliberately for the commander, with slower heavy/line states.",
                    TelegraphStyle.FinalStand),
                _ => throw new InvalidOperationException($"No enemy role visual spec exists for {roleId}.")
            };
        }

        private static EnemyRoleVisualSpec CanonicalLineCasterRole(string roleId)
        {
            EnemyRoleVisualSpec spec = CommonWeaponRole(
                roleId,
                "LineCaster",
                "SciFiSoldier01",
                CanonicalSciFiSoldierModelPath,
                CanonicalSciFiSoldierPrefabPath,
                CanonicalSciFiSoldierMaterialPath,
                "GatlinGun",
                new[]
                {
                    CommonWeapon(
                        "SK_SciFiLaserGatlinGun.FBX",
                        "M_LaserGatlinGun.mat",
                        "SK_SciFiLaserGatlinGun",
                        "RefPosLaserGatlinGun_Action")
                },
                Vector3.one,
                "Laser-gatlin soldier kept as the clean line-pressure caster; no turret is layered on top.",
                "Gatlin-gun common humanoid set: aim idle, run, primary/secondary shots, line shot, retreat shot.",
                TelegraphStyle.Line);
            spec.TargetModelPath = CanonicalSciFiSoldierModelPath;
            spec.MaterialRoot = CanonicalSciFiSoldierMaterialRoot;
            spec.TextureRoot = CanonicalSciFiSoldierTextureRoot;
            return spec;
        }

        public static GameObject LoadPromotedVisualSource(EnemyRoleVisualSpec spec)
        {
            EnsureRoleVisualAssets(spec);
            GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(spec.TargetModelPath);
            if (source == null)
            {
                throw new InvalidOperationException($"{spec.RoleTag} promoted model is missing at {spec.TargetModelPath}.");
            }

            return source;
        }

        public static void Apply(GameObject root, EnemyRoleVisualSpec spec)
        {
            EnsureRoleVisualAssets(spec);
            RemoveRoleVisualChildren(root.transform);

            GameObject visual = RecreateVisual(root.transform, spec);
            ReapplyPromotedMaterials(visual, spec);
            AttachWeapons(visual, spec);
            Animator animator = EnsureAnimator(visual, spec);
            Renderer[] renderers = CollectPresentableRenderers(visual);
            if (renderers.Length == 0)
            {
                throw new InvalidOperationException($"{spec.RoleTag} visual has no promoted renderers.");
            }

            BasicSoldierEnemy soldier = RequireComponent<BasicSoldierEnemy>(root, root.name);
            EnemyAttackTelegraphPresenter telegraphPresenter = RequireComponent<EnemyAttackTelegraphPresenter>(root, root.name);
            CombatHitFeedback hitFeedback = RequireComponent<CombatHitFeedback>(root, root.name);
            Renderer bodyRenderer = renderers[0];

            SetObjectReference(soldier, "animator", animator);
            SetObjectReference(soldier, "bodyRenderer", bodyRenderer);
            SetBool(soldier, "usePrototypeBodyColors", false);
            SetObjectReference(telegraphPresenter, "poseRoot", visual.transform);
            SetObjectReferenceArray(hitFeedback, "flashRenderers", renderers.Cast<UnityEngine.Object>().ToArray());
            SetBool(hitFeedback, "renderHitFeedback", true);
            SetBool(hitFeedback, "applyIdleColorOnEnable", false);
            ConfigureTelegraph(telegraphPresenter, spec);
            ConfigureElitePresentation(root, animator, bodyRenderer, spec);
            EditorUtility.SetDirty(root);
        }

        public static void Validate(GameObject root, string roleId, EnemyRoleVisualSpec spec)
        {
            Transform visual = root.transform.Find(spec.VisualName);
            if (visual == null)
            {
                throw new InvalidOperationException($"{roleId} should contain {spec.VisualName}.");
            }

            if (root.transform.Find(RejectedRolePresentationMarkerName) != null
                || root.transform.Find(RejectedRoleStaticVisualName) != null
                || root.transform.Find(RejectedRoleSummonSignalName) != null)
            {
                throw new InvalidOperationException($"{roleId} should not keep color-marker/static-overlay presentation children.");
            }

            Animator animator = visual.GetComponent<Animator>();
            if (animator == null)
            {
                throw new InvalidOperationException($"{roleId} visual should carry a local Animator.");
            }

            string controllerPath = AssetDatabase.GetAssetPath(animator.runtimeAnimatorController).Replace('\\', '/');
            if (!string.Equals(controllerPath, spec.ControllerPath, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"{roleId} should use {spec.ControllerPath}, found {controllerPath}.");
            }

            ValidatePrefabSourcePath(visual.gameObject, spec.TargetModelPath);
            ValidateRendererAssets(visual.gameObject, roleId);
            ValidateWeapons(visual, spec);
            ValidateCombatReferences(root, visual, animator, roleId);
            ValidateElitePresentation(root, roleId, spec);
        }
    }
}
