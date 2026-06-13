using System;
using System.Collections.Generic;
using DimensionBrawl.AI;
using DimensionBrawl.Combat;
using DimensionBrawl.Enemies;
using DimensionBrawl.Presentation;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace DimensionBrawl.Editor
{
    public static class ActionFoundationEnemyRoleCandidateSetup
    {
        public const string RoleCandidateProfileRoot = ActionFoundationProfileSetup.ProfileRoot + "/EnemyRoleCandidates";
        public const string RoleCandidatePrefabRoot = ActionFoundationEnemyPrefabSetup.PrefabRoot + "/RoleCandidates";

        public const string EntryProbeCandidateProfilePath = RoleCandidateProfileRoot + "/DB_RoleCandidate_EntryProbe.asset";
        public const string CloseGuardCandidateProfilePath = RoleCandidateProfileRoot + "/DB_RoleCandidate_CloseGuard.asset";
        public const string LungeChaserCandidateProfilePath = RoleCandidateProfileRoot + "/DB_RoleCandidate_LungeChaser.asset";
        public const string LineCasterCandidateProfilePath = RoleCandidateProfileRoot + "/DB_RoleCandidate_LineCaster.asset";
        public const string FanSuppressorCandidateProfilePath = RoleCandidateProfileRoot + "/DB_RoleCandidate_FanSuppressor.asset";
        public const string BacklineShooterCandidateProfilePath = RoleCandidateProfileRoot + "/DB_RoleCandidate_BacklineShooter.asset";
        public const string SkirmisherCandidateProfilePath = RoleCandidateProfileRoot + "/DB_RoleCandidate_Skirmisher.asset";
        public const string ShieldBreakerEliteCandidateProfilePath = RoleCandidateProfileRoot + "/DB_RoleCandidate_ShieldBreakerElite.asset";
        public const string AuraCaptainEliteCandidateProfilePath = RoleCandidateProfileRoot + "/DB_RoleCandidate_AuraCaptainElite.asset";
        public const string SummonCallerEliteCandidateProfilePath = RoleCandidateProfileRoot + "/DB_RoleCandidate_SummonCallerElite.asset";
        public const string PhaseDuelistEliteCandidateProfilePath = RoleCandidateProfileRoot + "/DB_RoleCandidate_PhaseDuelistElite.asset";
        public const string FinalStandCommanderEliteCandidateProfilePath = RoleCandidateProfileRoot + "/DB_RoleCandidate_FinalStandCommanderElite.asset";

        public const string EntryProbePrefabPath = RoleCandidatePrefabRoot + "/PF_Enemy_Role_EntryProbe.prefab";
        public const string CloseGuardPrefabPath = RoleCandidatePrefabRoot + "/PF_Enemy_Role_CloseGuard.prefab";
        public const string LungeChaserPrefabPath = RoleCandidatePrefabRoot + "/PF_Enemy_Role_LungeChaser.prefab";
        public const string LineCasterPrefabPath = RoleCandidatePrefabRoot + "/PF_Enemy_Role_LineCaster.prefab";
        public const string FanSuppressorPrefabPath = RoleCandidatePrefabRoot + "/PF_Enemy_Role_FanSuppressor.prefab";
        public const string BacklineShooterPrefabPath = RoleCandidatePrefabRoot + "/PF_Enemy_Role_BacklineShooter.prefab";
        public const string SkirmisherPrefabPath = RoleCandidatePrefabRoot + "/PF_Enemy_Role_Skirmisher.prefab";
        public const string ShieldBreakerElitePrefabPath = RoleCandidatePrefabRoot + "/PF_Enemy_Role_ShieldBreakerElite.prefab";
        public const string AuraCaptainElitePrefabPath = RoleCandidatePrefabRoot + "/PF_Enemy_Role_AuraCaptainElite.prefab";
        public const string SummonCallerElitePrefabPath = RoleCandidatePrefabRoot + "/PF_Enemy_Role_SummonCallerElite.prefab";
        public const string PhaseDuelistElitePrefabPath = RoleCandidatePrefabRoot + "/PF_Enemy_Role_PhaseDuelistElite.prefab";
        public const string FinalStandCommanderElitePrefabPath = RoleCandidatePrefabRoot + "/PF_Enemy_Role_FinalStandCommanderElite.prefab";

        private const string MaintenanceWorkerVisualPath = "Assets/_Game/Art/Characters/Enemies/SciFiSoldiers/MaintenanceWorker/Models/SK_MaintenanceWorkerAllMeshes.fbx";
        private const string VfxPoolChildName = "CombatVfxPool";

        private static readonly string[] CandidateProfilePaths =
        {
            EntryProbeCandidateProfilePath,
            CloseGuardCandidateProfilePath,
            LungeChaserCandidateProfilePath,
            LineCasterCandidateProfilePath,
            FanSuppressorCandidateProfilePath,
            BacklineShooterCandidateProfilePath,
            SkirmisherCandidateProfilePath,
            ShieldBreakerEliteCandidateProfilePath,
            AuraCaptainEliteCandidateProfilePath,
            SummonCallerEliteCandidateProfilePath,
            PhaseDuelistEliteCandidateProfilePath,
            FinalStandCommanderEliteCandidateProfilePath
        };

        [MenuItem("DimensionBrawl/Reapply Action Foundation Enemy Role Candidates")]
        public static void ReapplyEnemyRoleCandidatesMenu()
        {
            EnsureEnemyRoleCandidates();
            Debug.Log("Reapplied ActionFoundation enemy role candidate prefabs and profiles.");
        }

        [MenuItem("DimensionBrawl/Validate Action Foundation Enemy Role Candidates")]
        public static void ValidateEnemyRoleCandidatesMenu()
        {
            ValidateEnemyRoleCandidates();
            Debug.Log("ActionFoundation enemy role candidate validation passed.");
        }

        public static void EnsureEnemyRoleCandidates()
        {
            EnsureFolder(RoleCandidateProfileRoot);
            EnsureFolder(RoleCandidatePrefabRoot);

            CombatVfxCueProfile vfxCueProfile = ActionFoundationCombatVfxSetup.EnsureCombatVfxAssets();
            ActionFoundationEnemyPrefabSetup.EnsureEnemyPrefabCandidates();
            ActionFoundationEnemyArchetypeSetup.EnsureEnemyArchetypeAssets();
            ActionFoundationForge3DLineTurretSetup.EnsureLineTurretVisualCandidate();

            CandidateRefs refs = LoadRefs();
            RoleCandidateSpec[] specs = CreateCandidateSpecs(refs);
            for (int i = 0; i < specs.Length; i++)
            {
                EnsureRoleCandidate(specs[i], vfxCueProfile);
            }

            AssetDatabase.SaveAssets();
        }

        public static void ValidateEnemyRoleCandidates()
        {
            CombatVfxCueProfile vfxCueProfile = LoadAsset<CombatVfxCueProfile>(ActionFoundationCombatVfxSetup.CombatVfxCueProfilePath);
            var coveredRoleIds = new HashSet<string>();

            for (int i = 0; i < CandidateProfilePaths.Length; i++)
            {
                CombatEnemyRoleCandidateProfile candidate =
                    LoadAsset<CombatEnemyRoleCandidateProfile>(CandidateProfilePaths[i]);
                ValidateRoleCandidateProfile(candidate, vfxCueProfile, coveredRoleIds);
            }

            RequireCoveredRole(coveredRoleIds, "SciFiSoldier.EntryProbe");
            RequireCoveredRole(coveredRoleIds, "SciFiSoldier.CloseGuard");
            RequireCoveredRole(coveredRoleIds, "SciFiSoldier.LungeChaser");
            RequireCoveredRole(coveredRoleIds, "SciFiSoldier.LineCaster");
            RequireCoveredRole(coveredRoleIds, "SciFiSoldier.FanSuppressor");
            RequireCoveredRole(coveredRoleIds, "SciFiSoldier.BacklineShooter");
            RequireCoveredRole(coveredRoleIds, "SciFiSoldier.Skirmisher");
            RequireCoveredRole(coveredRoleIds, "SciFiSoldier.Elite.ShieldBreaker");
            RequireCoveredRole(coveredRoleIds, "SciFiSoldier.Elite.AuraCaptain");
            RequireCoveredRole(coveredRoleIds, "SciFiSoldier.Elite.SummonCaller");
            RequireCoveredRole(coveredRoleIds, "SciFiSoldier.Elite.PhaseDuelist");
            RequireCoveredRole(coveredRoleIds, "SciFiSoldier.Elite.FinalStandCommander");
        }

        private static void EnsureRoleCandidate(RoleCandidateSpec spec, CombatVfxCueProfile vfxCueProfile)
        {
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(spec.SourcePrefabPath);
            try
            {
                prefabRoot.name = spec.PrefabName;
                ConfigureRolePrefab(prefabRoot, spec.Role, vfxCueProfile);

                GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(prefabRoot, spec.PrefabPath);
                if (savedPrefab == null)
                {
                    throw new InvalidOperationException($"Failed to save role candidate prefab at {spec.PrefabPath}.");
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }

            GameObject rolePrefab = LoadAsset<GameObject>(spec.PrefabPath);
            CombatEnemyRoleCandidateProfile candidate =
                LoadOrCreate<CombatEnemyRoleCandidateProfile>(spec.ProfilePath);
            ConfigureRoleCandidateProfile(candidate, spec, rolePrefab, vfxCueProfile);
        }

        private static void ConfigureRolePrefab(
            GameObject root,
            CombatEnemyRoleProfile role,
            CombatVfxCueProfile vfxCueProfile)
        {
            BasicSoldierEnemy soldier = RequireComponent<BasicSoldierEnemy>(root, root.name);
            CombatHealth health = RequireComponent<CombatHealth>(root, root.name);
            CombatTargetSensor targetSensor = RequireComponent<CombatTargetSensor>(root, root.name);
            CombatVfxCuePlayer cuePlayer = RequireComponent<CombatVfxCuePlayer>(root, root.name);
            EnemyCombatVfxCueDriver vfxCueDriver = RequireComponent<EnemyCombatVfxCueDriver>(root, root.name);

            SetString(soldier, "enemyTypeId", role.RoleId);
            SetString(soldier, "patternId", role.StartingPattern.PatternId);
            SetObjectReference(soldier, "patternProfile", role.StartingPattern);
            SetObjectReference(soldier, "patternDeck", role.PatternDeck);
            SetObjectReference(soldier, "target", null);
            SetObjectReference(soldier, "targetHealth", null);
            SetObjectReference(soldier, "selfHealth", health);
            SetObjectReference(soldier, "targetSensor", targetSensor);
            SetObjectReferenceArray(targetSensor, "targetCandidates", Array.Empty<UnityEngine.Object>());

            SetObjectReference(cuePlayer, "profile", vfxCueProfile);
            SetObjectReference(cuePlayer, "pooledRoot", EnsureLocalChild(root.transform, VfxPoolChildName));
            SetObjectReference(vfxCueDriver, "agentSource", soldier);
            SetObjectReference(vfxCueDriver, "health", health);
            SetObjectReference(vfxCueDriver, "cuePlayer", cuePlayer);
            SetObjectReference(vfxCueDriver, "cueAnchor", root.transform);
            SetPatternCueOverrides(vfxCueDriver);
            SetEliteCueOverrides(vfxCueDriver);

            CombatAiElitePatternProfile[] eliteProfiles = GetEliteProfiles(role);
            EnemyElitePatternController eliteController = root.GetComponent<EnemyElitePatternController>();
            if (role.EliteRole)
            {
                if (eliteController == null)
                {
                    eliteController = root.AddComponent<EnemyElitePatternController>();
                }

                Animator animator = root.GetComponentInChildren<Animator>(includeInactive: true);
                Renderer bodyRenderer = GetObjectReference<Renderer>(soldier, "bodyRenderer");
                SetObjectReference(eliteController, "health", health);
                SetObjectReference(eliteController, "soldier", soldier);
                SetObjectReference(eliteController, "animator", animator);
                SetObjectReference(eliteController, "cueRenderer", bodyRenderer);
                SetObjectReferenceArray(eliteController, "eliteProfiles", eliteProfiles);
                SetObjectReferenceArray(eliteController, "auraProtectedTargets", Array.Empty<UnityEngine.Object>());
                SetObjectReferenceArray(eliteController, "summonSignalObjects", Array.Empty<UnityEngine.Object>());
                SetObjectReference(vfxCueDriver, "elitePatternController", eliteController);
            }
            else
            {
                if (eliteController != null)
                {
                    SetObjectReferenceArray(eliteController, "eliteProfiles", Array.Empty<UnityEngine.Object>());
                    SetObjectReferenceArray(eliteController, "auraProtectedTargets", Array.Empty<UnityEngine.Object>());
                    SetObjectReferenceArray(eliteController, "summonSignalObjects", Array.Empty<UnityEngine.Object>());
                }

                SetObjectReference(vfxCueDriver, "elitePatternController", null);
            }

            ValidateNoRawImportedOrExternalSceneReferences(root);
        }

        private static void ConfigureRoleCandidateProfile(
            CombatEnemyRoleCandidateProfile candidate,
            RoleCandidateSpec spec,
            GameObject rolePrefab,
            CombatVfxCueProfile vfxCueProfile)
        {
            SerializedObject serializedObject = new SerializedObject(candidate);
            SetObjectReference(serializedObject, "role", spec.Role);
            SetObjectReference(serializedObject, "primaryArchetype", spec.PrimaryArchetype);
            SetObjectReference(serializedObject, "rolePrefab", rolePrefab);
            SetObjectReference(serializedObject, "promotedVisualSource", spec.PromotedVisualSource);
            SetObjectReference(serializedObject, "optionalStaticTurretVisualPrefab", spec.OptionalStaticTurretVisualPrefab);
            SetObjectReference(serializedObject, "vfxCueProfile", vfxCueProfile);
            SetString(serializedObject, "prefabStrategy", spec.PrefabStrategy);
            SetString(serializedObject, "animationRead", spec.AnimationRead);
            SetString(serializedObject, "vfxRead", spec.VfxRead);
            SetString(serializedObject, "reuseJustification", spec.ReuseJustification);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(candidate);
        }

        private static RoleCandidateSpec[] CreateCandidateSpecs(CandidateRefs refs)
        {
            return new[]
            {
                CreateSpec(
                    refs.EntryProbe,
                    refs.MeleeArchetype,
                    ActionFoundationEnemyPrefabSetup.MeleeSoldierPrefabPath,
                    "PF_Enemy_Role_EntryProbe",
                    EntryProbePrefabPath,
                    EntryProbeCandidateProfilePath,
                    refs.MaintenanceWorkerVisual,
                    null,
                    "Dedicated melee role prefab variant for first-read close pressure.",
                    "Uses the promoted MaintenanceWorker Animator with ClosePunish/Lunge/Line deck triggers for entry teaching.",
                    "ClosePunish windup/active plus EntryProbe deck cue overrides stay on the shared combat VFX profile."),
                CreateSpec(
                    refs.CloseGuard,
                    refs.MeleeArchetype,
                    ActionFoundationEnemyPrefabSetup.MeleeSoldierPrefabPath,
                    "PF_Enemy_Role_CloseGuard",
                    CloseGuardPrefabPath,
                    CloseGuardCandidateProfilePath,
                    refs.MaintenanceWorkerVisual,
                    null,
                    "Dedicated melee guard role prefab variant with BreakGateBruiser deck data.",
                    "Uses the promoted MaintenanceWorker Animator for close guard, heavy windup, and lunge reads.",
                    "ClosePunish/HeavyWindup/Lunge/Line cue overrides make guard pressure readable."),
                CreateSpec(
                    refs.LungeChaser,
                    refs.MeleeArchetype,
                    ActionFoundationEnemyPrefabSetup.MeleeSoldierPrefabPath,
                    "PF_Enemy_Role_LungeChaser",
                    LungeChaserPrefabPath,
                    LungeChaserCandidateProfilePath,
                    refs.MaintenanceWorkerVisual,
                    null,
                    "Dedicated melee chase role prefab variant with Skirmisher deck data.",
                    "Uses the promoted MaintenanceWorker Animator for committed lunge and counter windows.",
                    "LungeStrike and RetreatBlink cue overrides separate chase pressure from idle approach."),
                CreateSpec(
                    refs.LineCaster,
                    refs.RangedArchetype,
                    ActionFoundationEnemyPrefabSetup.GeneralDeckSoldierPrefabPath,
                    "PF_Enemy_Role_LineCaster",
                    LineCasterPrefabPath,
                    LineCasterCandidateProfilePath,
                    refs.SciFiSoldier01Visual,
                    refs.LineTurretVisualPrefab,
                    "Dedicated ranged role prefab variant; FORGE3D line turret remains the static visual alternative.",
                    "Uses promoted rifle-soldier animation clips while the line turret visual is tracked for fixed-lane promotion.",
                    "LinePressure projectile cue overrides and optional turret visual candidate keep lane reads explicit."),
                CreateSpec(
                    refs.FanSuppressor,
                    refs.RangedArchetype,
                    ActionFoundationEnemyPrefabSetup.GeneralDeckSoldierPrefabPath,
                    "PF_Enemy_Role_FanSuppressor",
                    FanSuppressorPrefabPath,
                    FanSuppressorCandidateProfilePath,
                    refs.SciFiSoldier01Visual,
                    null,
                    "Dedicated ranged role prefab variant for cone-space pressure.",
                    "Uses promoted rifle-soldier animation clips for fan, line, and lunge blend review.",
                    "FanPressure cue overrides carry the mid-range cone read without adding controller branches."),
                CreateSpec(
                    refs.BacklineShooter,
                    refs.RangedArchetype,
                    ActionFoundationEnemyPrefabSetup.GeneralDeckSoldierPrefabPath,
                    "PF_Enemy_Role_BacklineShooter",
                    BacklineShooterPrefabPath,
                    BacklineShooterCandidateProfilePath,
                    refs.SciFiSoldier01Visual,
                    refs.LineTurretVisualPrefab,
                    "Dedicated ranged role prefab variant; FORGE3D line turret remains the static backline alternative.",
                    "Uses promoted rifle-soldier animation clips for retreat shot and line-fire review.",
                    "RetreatShot/LinePressure cue overrides plus optional turret visual candidate preserve backline readability."),
                CreateSpec(
                    refs.Skirmisher,
                    refs.MeleeArchetype,
                    ActionFoundationEnemyPrefabSetup.MeleeSoldierPrefabPath,
                    "PF_Enemy_Role_Skirmisher",
                    SkirmisherPrefabPath,
                    SkirmisherCandidateProfilePath,
                    refs.MaintenanceWorkerVisual,
                    null,
                    "Dedicated mobile melee role prefab variant for blink-and-lunge pressure.",
                    "Uses the promoted MaintenanceWorker Animator with RetreatBlink/LungeStrike deck data.",
                    "RetreatBlink and LungeStrike cue overrides distinguish reposition from committed attack."),
                CreateSpec(
                    refs.ShieldBreakerElite,
                    refs.EliteArchetype,
                    ActionFoundationEnemyPrefabSetup.EliteDeckSoldierPrefabPath,
                    "PF_Enemy_Role_ShieldBreakerElite",
                    ShieldBreakerElitePrefabPath,
                    ShieldBreakerEliteCandidateProfilePath,
                    refs.HeavyBattleArmorVisual,
                    null,
                    "Dedicated elite role prefab variant with only ShieldCycle and ArmorBreak traits.",
                    "Uses HeavyBattleArmor elite Animator clips for guard break, stagger, and death readability.",
                    "GuardBreak, EliteShield, and EliteArmorBreak cues bind through data instead of role branches."),
                CreateSpec(
                    refs.AuraCaptainElite,
                    refs.EliteArchetype,
                    ActionFoundationEnemyPrefabSetup.EliteDeckSoldierPrefabPath,
                    "PF_Enemy_Role_AuraCaptainElite",
                    AuraCaptainElitePrefabPath,
                    AuraCaptainEliteCandidateProfilePath,
                    refs.HeavyBattleArmorVisual,
                    null,
                    "Dedicated elite support role prefab variant with AuraBuffer and ShieldCycle traits.",
                    "Uses HeavyBattleArmor elite Animator clips while aura targets remain scene-injected, not prefab-stored.",
                    "FanPressure, EliteAura, and EliteShield cues make support priority readable."),
                CreateSpec(
                    refs.SummonCallerElite,
                    refs.EliteArchetype,
                    ActionFoundationEnemyPrefabSetup.EliteDeckSoldierPrefabPath,
                    "PF_Enemy_Role_SummonCallerElite",
                    SummonCallerElitePrefabPath,
                    SummonCallerEliteCandidateProfilePath,
                    refs.HeavyBattleArmorVisual,
                    null,
                    "Dedicated elite role prefab variant with summon-signal data but no runtime spawning.",
                    "Uses HeavyBattleArmor elite Animator clips; summon markers remain explicit scene-provided signals.",
                    "RetreatShot, EliteSummon, and EliteAura cues communicate the caller role without adding spawners."),
                CreateSpec(
                    refs.PhaseDuelistElite,
                    refs.EliteArchetype,
                    ActionFoundationEnemyPrefabSetup.EliteDeckSoldierPrefabPath,
                    "PF_Enemy_Role_PhaseDuelistElite",
                    PhaseDuelistElitePrefabPath,
                    PhaseDuelistEliteCandidateProfilePath,
                    refs.HeavyBattleArmorVisual,
                    null,
                    "Dedicated elite duel role prefab variant with PhaseSwap and ArmorBreak traits.",
                    "Uses HeavyBattleArmor elite Animator clips for phase handoff and guard-break review.",
                    "GuardBreak, ElitePhaseSwap, and EliteArmorBreak cues separate phase tells from damage windows."),
                CreateSpec(
                    refs.FinalStandCommanderElite,
                    refs.EliteArchetype,
                    ActionFoundationEnemyPrefabSetup.EliteDeckSoldierPrefabPath,
                    "PF_Enemy_Role_FinalStandCommanderElite",
                    FinalStandCommanderElitePrefabPath,
                    FinalStandCommanderEliteCandidateProfilePath,
                    refs.HeavyBattleArmorVisual,
                    null,
                    "Dedicated elite final-stand role prefab variant with shield, armor, and phase traits.",
                    "Uses HeavyBattleArmor elite Animator clips across GuardBreak, HeavyWindup, line, fan, and phase pressure.",
                    "FinalStand deck pattern cues plus three elite signal cues make the commander readable.")
            };
        }

        private static RoleCandidateSpec CreateSpec(
            CombatEnemyRoleProfile role,
            CombatEnemyArchetypeProfile primaryArchetype,
            string sourcePrefabPath,
            string prefabName,
            string prefabPath,
            string profilePath,
            GameObject promotedVisualSource,
            GameObject optionalStaticTurretVisualPrefab,
            string prefabStrategy,
            string animationRead,
            string vfxRead)
        {
            return new RoleCandidateSpec
            {
                Role = role,
                PrimaryArchetype = primaryArchetype,
                SourcePrefabPath = sourcePrefabPath,
                PrefabName = prefabName,
                PrefabPath = prefabPath,
                ProfilePath = profilePath,
                PromotedVisualSource = promotedVisualSource,
                OptionalStaticTurretVisualPrefab = optionalStaticTurretVisualPrefab,
                PrefabStrategy = prefabStrategy,
                AnimationRead = animationRead,
                VfxRead = vfxRead,
                ReuseJustification = "Candidate keeps role behavior in data so the same pattern/deck contract can be reused by future summon AI."
            };
        }

        private static void ValidateRoleCandidateProfile(
            CombatEnemyRoleCandidateProfile candidate,
            CombatVfxCueProfile expectedVfxCueProfile,
            HashSet<string> coveredRoleIds)
        {
            if (candidate.Role == null)
            {
                throw new InvalidOperationException($"{candidate.name} has no role.");
            }

            if (candidate.PrimaryArchetype == null)
            {
                throw new InvalidOperationException($"{candidate.Role.RoleId} has no primary archetype.");
            }

            if (!ArchetypeContainsRole(candidate.PrimaryArchetype, candidate.Role))
            {
                throw new InvalidOperationException(
                    $"{candidate.PrimaryArchetype.ArchetypeId} does not list {candidate.Role.RoleId} as a compatible role.");
            }

            if (candidate.RolePrefab == null)
            {
                throw new InvalidOperationException($"{candidate.Role.RoleId} has no role prefab candidate.");
            }

            if (candidate.PromotedVisualSource == null)
            {
                throw new InvalidOperationException($"{candidate.Role.RoleId} has no promoted visual source.");
            }

            if (candidate.VfxCueProfile != expectedVfxCueProfile)
            {
                throw new InvalidOperationException($"{candidate.Role.RoleId} should use the shared ActionFoundation combat VFX cue profile.");
            }

            ValidateRequiredText(candidate.Role.RoleId, "prefab strategy", candidate.PrefabStrategy);
            ValidateRequiredText(candidate.Role.RoleId, "animation read", candidate.AnimationRead);
            ValidateRequiredText(candidate.Role.RoleId, "VFX read", candidate.VfxRead);
            ValidateRequiredText(candidate.Role.RoleId, "reuse justification", candidate.ReuseJustification);

            string rolePrefabPath = AssetDatabase.GetAssetPath(candidate.RolePrefab).Replace('\\', '/');
            if (!rolePrefabPath.StartsWith(RoleCandidatePrefabRoot + "/", StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"{candidate.Role.RoleId} role prefab should be under {RoleCandidatePrefabRoot}, found {rolePrefabPath}.");
            }

            ValidateGameOwnedAsset(candidate.PromotedVisualSource, $"{candidate.Role.RoleId} promoted visual source");
            if (candidate.OptionalStaticTurretVisualPrefab != null)
            {
                ValidateGameOwnedAsset(candidate.OptionalStaticTurretVisualPrefab, $"{candidate.Role.RoleId} optional static turret visual");
            }

            ValidateRolePrefab(candidate, rolePrefabPath);
            coveredRoleIds.Add(candidate.Role.RoleId);
        }

        private static void ValidateRolePrefab(CombatEnemyRoleCandidateProfile candidate, string rolePrefabPath)
        {
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(rolePrefabPath);
            try
            {
                BasicSoldierEnemy soldier = RequireComponent<BasicSoldierEnemy>(prefabRoot, candidate.Role.RoleId);
                CombatHealth health = RequireComponent<CombatHealth>(prefabRoot, candidate.Role.RoleId);
                CombatTargetSensor targetSensor = RequireComponent<CombatTargetSensor>(prefabRoot, candidate.Role.RoleId);
                CombatVfxCuePlayer cuePlayer = RequireComponent<CombatVfxCuePlayer>(prefabRoot, candidate.Role.RoleId);
                EnemyCombatVfxCueDriver vfxCueDriver = RequireComponent<EnemyCombatVfxCueDriver>(prefabRoot, candidate.Role.RoleId);
                Animator animator = prefabRoot.GetComponentInChildren<Animator>(includeInactive: true);

                if (soldier.PatternProfile != candidate.Role.StartingPattern)
                {
                    throw new InvalidOperationException($"{candidate.Role.RoleId} prefab should start from {candidate.Role.StartingPattern.name}.");
                }

                if (soldier.PatternDeck != candidate.Role.PatternDeck)
                {
                    throw new InvalidOperationException($"{candidate.Role.RoleId} prefab should use {candidate.Role.PatternDeck.name}.");
                }

                if (soldier.SelfHealth != health)
                {
                    throw new InvalidOperationException($"{candidate.Role.RoleId} soldier should use its local health.");
                }

                if (soldier.TargetSensor != targetSensor)
                {
                    throw new InvalidOperationException($"{candidate.Role.RoleId} soldier should use its local target sensor.");
                }

                if (targetSensor.TargetCandidateCount != 0)
                {
                    throw new InvalidOperationException($"{candidate.Role.RoleId} prefab target candidates should be scene-injected.");
                }

                ValidateObjectReference(cuePlayer, "profile", candidate.VfxCueProfile);
                ValidateLocalReference(cuePlayer, "pooledRoot", prefabRoot);
                ValidateRoleAnimationAssignment(candidate.Role, soldier, animator);
                ValidateObjectReference(vfxCueDriver, "agentSource", soldier);
                ValidateObjectReference(vfxCueDriver, "health", health);
                ValidateObjectReference(vfxCueDriver, "cuePlayer", cuePlayer);
                ValidateObjectReference(vfxCueDriver, "cueAnchor", prefabRoot.transform);
                ValidatePatternCueOverrides(vfxCueDriver);

                if (candidate.Role.EliteRole)
                {
                    EnemyElitePatternController eliteController = RequireComponent<EnemyElitePatternController>(prefabRoot, candidate.Role.RoleId);
                    ValidateObjectReference(vfxCueDriver, "elitePatternController", eliteController);
                    ValidateEliteProfileArray(candidate.Role, eliteController);
                    ValidateEliteCueOverrides(vfxCueDriver);
                }
                else
                {
                    ValidateObjectReference(vfxCueDriver, "elitePatternController", null);
                }

                ValidateNoRawImportedOrExternalSceneReferences(prefabRoot);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        private static CandidateRefs LoadRefs()
        {
            return new CandidateRefs
            {
                EntryProbe = LoadAsset<CombatEnemyRoleProfile>(ActionFoundationEnemyRoleDeckSetup.EntryProbeRolePath),
                CloseGuard = LoadAsset<CombatEnemyRoleProfile>(ActionFoundationEnemyRoleDeckSetup.CloseGuardRolePath),
                LungeChaser = LoadAsset<CombatEnemyRoleProfile>(ActionFoundationEnemyRoleDeckSetup.LungeChaserRolePath),
                LineCaster = LoadAsset<CombatEnemyRoleProfile>(ActionFoundationEnemyRoleDeckSetup.LineCasterRolePath),
                FanSuppressor = LoadAsset<CombatEnemyRoleProfile>(ActionFoundationEnemyRoleDeckSetup.FanSuppressorRolePath),
                BacklineShooter = LoadAsset<CombatEnemyRoleProfile>(ActionFoundationEnemyRoleDeckSetup.BacklineShooterRolePath),
                Skirmisher = LoadAsset<CombatEnemyRoleProfile>(ActionFoundationEnemyRoleDeckSetup.SkirmisherRolePath),
                ShieldBreakerElite = LoadAsset<CombatEnemyRoleProfile>(ActionFoundationEnemyRoleDeckSetup.ShieldBreakerEliteRolePath),
                AuraCaptainElite = LoadAsset<CombatEnemyRoleProfile>(ActionFoundationEnemyRoleDeckSetup.AuraCaptainEliteRolePath),
                SummonCallerElite = LoadAsset<CombatEnemyRoleProfile>(ActionFoundationEnemyRoleDeckSetup.SummonCallerEliteRolePath),
                PhaseDuelistElite = LoadAsset<CombatEnemyRoleProfile>(ActionFoundationEnemyRoleDeckSetup.PhaseDuelistEliteRolePath),
                FinalStandCommanderElite = LoadAsset<CombatEnemyRoleProfile>(ActionFoundationEnemyRoleDeckSetup.FinalStandCommanderEliteRolePath),
                MeleeArchetype = LoadAsset<CombatEnemyArchetypeProfile>(ActionFoundationEnemyArchetypeSetup.SciFiMeleeSoldierPath),
                RangedArchetype = LoadAsset<CombatEnemyArchetypeProfile>(ActionFoundationEnemyArchetypeSetup.SciFiRangedSoldierPath),
                EliteArchetype = LoadAsset<CombatEnemyArchetypeProfile>(ActionFoundationEnemyArchetypeSetup.SciFiEliteSoldierPath),
                MaintenanceWorkerVisual = LoadAsset<GameObject>(MaintenanceWorkerVisualPath),
                SciFiSoldier01Visual = LoadAsset<GameObject>(ActionFoundationSciFiSoldier01VisualSetup.ModelPath),
                HeavyBattleArmorVisual = LoadAsset<GameObject>(ActionFoundationSciFiEliteSoldierVisualSetup.ModelPath),
                LineTurretVisualPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ActionFoundationForge3DLineTurretSetup.PrefabPath)
            };
        }

        private static void SetPatternCueOverrides(EnemyCombatVfxCueDriver driver)
        {
            CombatPatternVfxCueOverride[] overrides =
            {
                new CombatPatternVfxCueOverride(LoadAsset<CombatAiPatternProfile>(ActionFoundationProfileSetup.EnemyPatternProfilePath), CombatVfxCueId.EnemyClosePunishWindup, CombatVfxCueId.EnemyClosePunishActive, 1f, 1f),
                new CombatPatternVfxCueOverride(LoadAsset<CombatAiPatternProfile>(ActionFoundationProfileSetup.EnemyLungePatternProfilePath), CombatVfxCueId.EnemyLungeStrikeWindup, CombatVfxCueId.EnemyLungeStrikeActive, 1.05f, 1.08f),
                new CombatPatternVfxCueOverride(LoadAsset<CombatAiPatternProfile>(ActionFoundationProfileSetup.EnemyHeavyWindupPatternProfilePath), CombatVfxCueId.EnemyHeavyWindupWindup, CombatVfxCueId.EnemyHeavyWindupActive, 1.18f, 1.25f),
                new CombatPatternVfxCueOverride(LoadAsset<CombatAiPatternProfile>(ActionFoundationProfileSetup.EnemyLinePressurePatternProfilePath), CombatVfxCueId.EnemyLinePressureWindup, CombatVfxCueId.EnemyLinePressureActive, 1f, 1.05f),
                new CombatPatternVfxCueOverride(LoadAsset<CombatAiPatternProfile>(ActionFoundationProfileSetup.EnemyFanPressurePatternProfilePath), CombatVfxCueId.EnemyFanPressureWindup, CombatVfxCueId.EnemyFanPressureActive, 1f, 1.08f),
                new CombatPatternVfxCueOverride(LoadAsset<CombatAiPatternProfile>(ActionFoundationEnemyPatternExpansionSetup.RetreatShotPatternPath), CombatVfxCueId.EnemyRetreatShotWindup, CombatVfxCueId.EnemyRetreatShotActive, 0.95f, 1f),
                new CombatPatternVfxCueOverride(LoadAsset<CombatAiPatternProfile>(ActionFoundationEnemyPatternExpansionSetup.RetreatBlinkPatternPath), CombatVfxCueId.EnemyRetreatBlinkWindup, CombatVfxCueId.EnemyRetreatBlinkActive, 1f, 1.15f),
                new CombatPatternVfxCueOverride(LoadAsset<CombatAiPatternProfile>(ActionFoundationEnemyPatternExpansionSetup.GuardBreakPatternPath), CombatVfxCueId.EnemyGuardBreakWindup, CombatVfxCueId.EnemyGuardBreakActive, 1.2f, 1.3f)
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
                new CombatEliteVfxCueOverride(LoadAsset<CombatAiElitePatternProfile>(ActionFoundationEnemyPatternExpansionSetup.ShieldCycleEliteProfilePath), CombatVfxCueId.EliteShieldSignal, 1f),
                new CombatEliteVfxCueOverride(LoadAsset<CombatAiElitePatternProfile>(ActionFoundationEnemyPatternExpansionSetup.ArmorBreakEliteProfilePath), CombatVfxCueId.EliteArmorBreakSignal, 1.05f),
                new CombatEliteVfxCueOverride(LoadAsset<CombatAiElitePatternProfile>(ActionFoundationEnemyPatternExpansionSetup.AuraBufferEliteProfilePath), CombatVfxCueId.EliteAuraSignal, 1.1f),
                new CombatEliteVfxCueOverride(LoadAsset<CombatAiElitePatternProfile>(ActionFoundationEnemyPatternExpansionSetup.SummonPackageEliteProfilePath), CombatVfxCueId.EliteSummonSignal, 1.1f),
                new CombatEliteVfxCueOverride(LoadAsset<CombatAiElitePatternProfile>(ActionFoundationEnemyPatternExpansionSetup.PhaseSwapEliteProfilePath), CombatVfxCueId.ElitePhaseSwapSignal, 1.18f)
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

        private static void ValidatePatternCueOverrides(EnemyCombatVfxCueDriver driver)
        {
            SerializedProperty array = RequireProperty(new SerializedObject(driver), "patternCueOverrides");
            if (array.arraySize < 8)
            {
                throw new InvalidOperationException($"{driver.name} should carry all pattern VFX cue overrides.");
            }
        }

        private static void ValidateEliteCueOverrides(EnemyCombatVfxCueDriver driver)
        {
            SerializedProperty array = RequireProperty(new SerializedObject(driver), "eliteCueOverrides");
            if (array.arraySize < 5)
            {
                throw new InvalidOperationException($"{driver.name} should carry all elite VFX cue overrides.");
            }
        }

        private static void ValidateEliteProfileArray(CombatEnemyRoleProfile role, EnemyElitePatternController eliteController)
        {
            SerializedProperty array = RequireProperty(new SerializedObject(eliteController), "eliteProfiles");
            if (array.arraySize != role.EliteProfileCount)
            {
                throw new InvalidOperationException($"{role.RoleId} should carry {role.EliteProfileCount} elite profiles, found {array.arraySize}.");
            }

            for (int i = 0; i < role.EliteProfileCount; i++)
            {
                UnityEngine.Object actual = array.GetArrayElementAtIndex(i).objectReferenceValue;
                if (actual != role.GetEliteProfile(i))
                {
                    throw new InvalidOperationException($"{role.RoleId} elite profile {i} should be {role.GetEliteProfile(i).name}.");
                }
            }
        }

        private static void ValidateRoleAnimationAssignment(
            CombatEnemyRoleProfile role,
            BasicSoldierEnemy soldier,
            Animator animator)
        {
            if (animator == null)
            {
                throw new InvalidOperationException($"{role.RoleId} prefab should have a promoted local Animator.");
            }

            if (animator.runtimeAnimatorController == null)
            {
                throw new InvalidOperationException($"{role.RoleId} Animator should have a promoted controller assigned.");
            }

            if (animator.runtimeAnimatorController.animationClips == null ||
                animator.runtimeAnimatorController.animationClips.Length == 0)
            {
                throw new InvalidOperationException($"{role.RoleId} Animator controller should carry promoted animation clips.");
            }

            if (animator.applyRootMotion)
            {
                throw new InvalidOperationException($"{role.RoleId} Animator should keep root motion disabled; BasicSoldierEnemy owns movement.");
            }

            ValidateObjectReference(soldier, "animator", animator);

            AnimatorController controller = animator.runtimeAnimatorController as AnimatorController;
            if (controller == null)
            {
                throw new InvalidOperationException($"{role.RoleId} Animator should use an inspectable AnimatorController.");
            }

            ValidateRolePatternAnimationParameters(role, controller);
            ValidateRoleEliteSignalAnimationParameters(role, controller);
        }

        private static void ValidateRolePatternAnimationParameters(
            CombatEnemyRoleProfile role,
            AnimatorController controller)
        {
            var patterns = new List<CombatAiPatternProfile>();
            AddPattern(patterns, role.StartingPattern);
            AddDeckPatterns(patterns, role.PatternDeck);

            for (int i = 0; i < role.EliteProfileCount; i++)
            {
                CombatAiElitePatternProfile eliteProfile = role.GetEliteProfile(i);
                AddPattern(patterns, eliteProfile.ReplacementPatternProfile);
                AddDeckPatterns(patterns, eliteProfile.ReplacementPatternDeck);
            }

            for (int i = 0; i < patterns.Count; i++)
            {
                CombatAiPatternProfile profile = patterns[i];
                string label = $"{role.RoleId}/{profile.PatternId}";
                ValidateAnimatorParameter(controller, profile.MoveSpeedParameter, AnimatorControllerParameterType.Float, label, true);
                ValidateAnimatorParameter(controller, profile.PrepareTrigger, AnimatorControllerParameterType.Trigger, label, false);
                ValidateAnimatorParameter(controller, profile.AttackTrigger, AnimatorControllerParameterType.Trigger, label, true);
                ValidateAnimatorParameter(controller, profile.HitTrigger, AnimatorControllerParameterType.Trigger, label, true);
                ValidateAnimatorParameter(controller, profile.DeathTrigger, AnimatorControllerParameterType.Trigger, label, true);
            }
        }

        private static void ValidateRoleEliteSignalAnimationParameters(
            CombatEnemyRoleProfile role,
            AnimatorController controller)
        {
            for (int i = 0; i < role.EliteProfileCount; i++)
            {
                CombatAiElitePatternProfile profile = role.GetEliteProfile(i);
                ValidateAnimatorParameter(
                    controller,
                    profile.SignalAnimationTrigger,
                    AnimatorControllerParameterType.Trigger,
                    $"{role.RoleId}/{profile.PatternId}",
                    true);
            }
        }

        private static void AddDeckPatterns(List<CombatAiPatternProfile> patterns, CombatAiPatternDeck deck)
        {
            if (deck == null)
            {
                return;
            }

            for (int i = 0; i < deck.EntryCount; i++)
            {
                AddPattern(patterns, deck.GetEntry(i).Profile);
            }
        }

        private static void AddPattern(List<CombatAiPatternProfile> patterns, CombatAiPatternProfile pattern)
        {
            if (pattern == null || patterns.Contains(pattern))
            {
                return;
            }

            patterns.Add(pattern);
        }

        private static void ValidateAnimatorParameter(
            AnimatorController controller,
            string parameterName,
            AnimatorControllerParameterType expectedType,
            string label,
            bool required)
        {
            if (string.IsNullOrWhiteSpace(parameterName))
            {
                if (required)
                {
                    throw new InvalidOperationException($"{label} should define a non-empty {expectedType} animation parameter.");
                }

                return;
            }

            AnimatorControllerParameter[] parameters = controller.parameters;
            for (int i = 0; i < parameters.Length; i++)
            {
                AnimatorControllerParameter parameter = parameters[i];
                if (!string.Equals(parameter.name, parameterName, StringComparison.Ordinal))
                {
                    continue;
                }

                if (parameter.type != expectedType)
                {
                    throw new InvalidOperationException(
                        $"{label} animation parameter {parameterName} should be {expectedType}, found {parameter.type}.");
                }

                return;
            }

            throw new InvalidOperationException(
                $"{label} AnimatorController {controller.name} is missing {expectedType} parameter {parameterName}.");
        }

        private static CombatAiElitePatternProfile[] GetEliteProfiles(CombatEnemyRoleProfile role)
        {
            var profiles = new CombatAiElitePatternProfile[role.EliteProfileCount];
            for (int i = 0; i < profiles.Length; i++)
            {
                profiles[i] = role.GetEliteProfile(i);
            }

            return profiles;
        }

        private static bool ArchetypeContainsRole(CombatEnemyArchetypeProfile archetype, CombatEnemyRoleProfile role)
        {
            for (int i = 0; i < archetype.CompatibleRoleCount; i++)
            {
                if (archetype.GetCompatibleRole(i) == role)
                {
                    return true;
                }
            }

            return false;
        }

        private static void RequireCoveredRole(HashSet<string> coveredRoleIds, string roleId)
        {
            if (!coveredRoleIds.Contains(roleId))
            {
                throw new InvalidOperationException($"Enemy role candidate catalog does not cover {roleId}.");
            }
        }

        private static void ValidateRequiredText(string roleId, string label, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException($"{roleId} has no {label} note.");
            }
        }

        private static void ValidateGameOwnedAsset(UnityEngine.Object asset, string label)
        {
            string assetPath = AssetDatabase.GetAssetPath(asset).Replace('\\', '/');
            if (!assetPath.StartsWith("Assets/_Game/", StringComparison.Ordinal) || assetPath.Contains("/_Imported/", StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"{label} should be promoted under Assets/_Game, found {assetPath}.");
            }
        }

        private static void ValidateNoRawImportedOrExternalSceneReferences(GameObject root)
        {
            Component[] components = root.GetComponentsInChildren<Component>(includeInactive: true);
            for (int i = 0; i < components.Length; i++)
            {
                Component component = components[i];
                if (component == null)
                {
                    continue;
                }

                var serializedObject = new SerializedObject(component);
                SerializedProperty property = serializedObject.GetIterator();
                bool enterChildren = true;
                while (property.NextVisible(enterChildren))
                {
                    enterChildren = true;
                    if (property.propertyType != SerializedPropertyType.ObjectReference)
                    {
                        continue;
                    }

                    UnityEngine.Object reference = property.objectReferenceValue;
                    if (reference == null)
                    {
                        continue;
                    }

                    string assetPath = AssetDatabase.GetAssetPath(reference).Replace('\\', '/');
                    if (assetPath.Contains("/_Imported/", StringComparison.Ordinal))
                    {
                        throw new InvalidOperationException(
                            $"{root.name}.{component.GetType().Name}.{property.propertyPath} references raw imported asset {assetPath}.");
                    }

                    if (!string.IsNullOrEmpty(assetPath) || EditorUtility.IsPersistent(reference))
                    {
                        continue;
                    }

                    if (!IsLocalPrefabReference(reference, root))
                    {
                        throw new InvalidOperationException(
                            $"{root.name}.{component.GetType().Name}.{property.propertyPath} carries an external scene reference to {reference.name}.");
                    }
                }
            }
        }

        private static bool IsLocalPrefabReference(UnityEngine.Object reference, GameObject root)
        {
            if (reference is GameObject gameObject)
            {
                return gameObject == root || gameObject.transform.IsChildOf(root.transform);
            }

            if (reference is Component component)
            {
                return component.gameObject == root || component.transform.IsChildOf(root.transform);
            }

            return false;
        }

        private static void ValidateObjectReference(UnityEngine.Object target, string propertyName, UnityEngine.Object expected)
        {
            UnityEngine.Object actual = RequireProperty(new SerializedObject(target), propertyName).objectReferenceValue;
            if (actual != expected)
            {
                string expectedName = expected != null ? expected.name : "null";
                string actualName = actual != null ? actual.name : "null";
                throw new InvalidOperationException($"{target.name}.{propertyName} expected {expectedName}, found {actualName}.");
            }
        }

        private static void ValidateLocalReference(UnityEngine.Object target, string propertyName, GameObject root)
        {
            UnityEngine.Object reference = RequireProperty(new SerializedObject(target), propertyName).objectReferenceValue;
            if (reference == null || !IsLocalPrefabReference(reference, root))
            {
                string referenceName = reference != null ? reference.name : "null";
                throw new InvalidOperationException($"{target.name}.{propertyName} should reference a local prefab object, found {referenceName}.");
            }
        }

        private static Transform EnsureLocalChild(Transform parent, string childName)
        {
            Transform existing = parent.Find(childName);
            if (existing != null)
            {
                return existing;
            }

            var child = new GameObject(childName);
            child.transform.SetParent(parent, worldPositionStays: false);
            return child.transform;
        }

        private static T GetObjectReference<T>(UnityEngine.Object target, string propertyName) where T : UnityEngine.Object
        {
            return RequireProperty(new SerializedObject(target), propertyName).objectReferenceValue as T;
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

        private static T LoadAsset<T>(string assetPath) where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset == null)
            {
                throw new InvalidOperationException($"Missing required asset at {assetPath}.");
            }

            return asset;
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

        private static void SetPatternCueOverride(SerializedProperty property, CombatPatternVfxCueOverride value)
        {
            SetObjectReference(property, "patternProfile", value.PatternProfile);
            SetEnum(property, "windupCueId", (int)value.WindupCueId);
            SetEnum(property, "attackActiveCueId", (int)value.AttackActiveCueId);
            SetFloat(property, "windupIntensity", value.WindupIntensity);
            SetFloat(property, "attackActiveIntensity", value.AttackActiveIntensity);
        }

        private static void SetEliteCueOverride(SerializedProperty property, CombatEliteVfxCueOverride value)
        {
            SetObjectReference(property, "eliteProfile", value.EliteProfile);
            SetEnum(property, "signalCueId", (int)value.SignalCueId);
            SetFloat(property, "intensity", value.Intensity);
        }

        private static void SetObjectReference(UnityEngine.Object target, string propertyName, UnityEngine.Object value)
        {
            var serializedObject = new SerializedObject(target);
            RequireProperty(serializedObject, propertyName).objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetObjectReference(SerializedObject serializedObject, string propertyName, UnityEngine.Object value)
        {
            RequireProperty(serializedObject, propertyName).objectReferenceValue = value;
        }

        private static void SetObjectReference(SerializedProperty property, string propertyName, UnityEngine.Object value)
        {
            RequireProperty(property, propertyName).objectReferenceValue = value;
        }

        private static void SetObjectReferenceArray(UnityEngine.Object target, string propertyName, UnityEngine.Object[] values)
        {
            var serializedObject = new SerializedObject(target);
            SerializedProperty array = RequireProperty(serializedObject, propertyName);
            array.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                array.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetString(UnityEngine.Object target, string propertyName, string value)
        {
            var serializedObject = new SerializedObject(target);
            RequireProperty(serializedObject, propertyName).stringValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetString(SerializedObject serializedObject, string propertyName, string value)
        {
            RequireProperty(serializedObject, propertyName).stringValue = value;
        }

        private static void SetEnum(SerializedProperty property, string propertyName, int value)
        {
            RequireProperty(property, propertyName).enumValueIndex = value;
        }

        private static void SetFloat(SerializedProperty property, string propertyName, float value)
        {
            RequireProperty(property, propertyName).floatValue = value;
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

        private static SerializedProperty RequireProperty(SerializedProperty parent, string propertyName)
        {
            SerializedProperty property = parent.FindPropertyRelative(propertyName);
            if (property == null)
            {
                throw new InvalidOperationException($"{parent.propertyPath} is missing serialized property {propertyName}.");
            }

            return property;
        }

        private sealed class CandidateRefs
        {
            public CombatEnemyRoleProfile EntryProbe;
            public CombatEnemyRoleProfile CloseGuard;
            public CombatEnemyRoleProfile LungeChaser;
            public CombatEnemyRoleProfile LineCaster;
            public CombatEnemyRoleProfile FanSuppressor;
            public CombatEnemyRoleProfile BacklineShooter;
            public CombatEnemyRoleProfile Skirmisher;
            public CombatEnemyRoleProfile ShieldBreakerElite;
            public CombatEnemyRoleProfile AuraCaptainElite;
            public CombatEnemyRoleProfile SummonCallerElite;
            public CombatEnemyRoleProfile PhaseDuelistElite;
            public CombatEnemyRoleProfile FinalStandCommanderElite;
            public CombatEnemyArchetypeProfile MeleeArchetype;
            public CombatEnemyArchetypeProfile RangedArchetype;
            public CombatEnemyArchetypeProfile EliteArchetype;
            public GameObject MaintenanceWorkerVisual;
            public GameObject SciFiSoldier01Visual;
            public GameObject HeavyBattleArmorVisual;
            public GameObject LineTurretVisualPrefab;
        }

        private sealed class RoleCandidateSpec
        {
            public CombatEnemyRoleProfile Role;
            public CombatEnemyArchetypeProfile PrimaryArchetype;
            public string SourcePrefabPath;
            public string PrefabName;
            public string PrefabPath;
            public string ProfilePath;
            public GameObject PromotedVisualSource;
            public GameObject OptionalStaticTurretVisualPrefab;
            public string PrefabStrategy;
            public string AnimationRead;
            public string VfxRead;
            public string ReuseJustification;
        }
    }
}
