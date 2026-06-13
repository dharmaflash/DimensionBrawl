using System;
using System.Collections.Generic;
using DimensionBrawl.AI;
using UnityEditor;
using UnityEngine;

namespace DimensionBrawl.Editor
{
    public static class ActionFoundationEnemyArchetypeSetup
    {
        public const string ArchetypeRoot = ActionFoundationProfileSetup.ProfileRoot + "/EnemyArchetypes";

        public const string SciFiMeleeSoldierPath = ArchetypeRoot + "/DB_Archetype_SciFiSoldier_Melee.asset";
        public const string SciFiRangedSoldierPath = ArchetypeRoot + "/DB_Archetype_SciFiSoldier_Ranged.asset";
        public const string SciFiEliteSoldierPath = ArchetypeRoot + "/DB_Archetype_SciFiSoldier_Elite.asset";
        public const string Forge3DLineTurretPath = ArchetypeRoot + "/DB_Archetype_FORGE3D_LineTurret.asset";
        public const string Forge3DMissileTurretPath = ArchetypeRoot + "/DB_Archetype_FORGE3D_MissileTurret.asset";
        public const string DragonBossFuturePath = ArchetypeRoot + "/DB_Archetype_DragonBoss_Future.asset";

        private const string MaintenanceWorkerVisualPath = "Assets/_Game/Art/Characters/Enemies/SciFiSoldiers/MaintenanceWorker/Models/SK_MaintenanceWorkerAllMeshes.fbx";
        private const string MeleeSoldierGameplayPrefabPath = "Assets/_Game/Prefabs/Enemies/ActionFoundation/PF_Enemy_SciFiSoldier_Melee_ClosePunish.prefab";
        private const string GeneralDeckSoldierGameplayPrefabPath = "Assets/_Game/Prefabs/Enemies/ActionFoundation/PF_Enemy_SciFiSoldier_GeneralDeck.prefab";
        private const string EliteDeckSoldierGameplayPrefabPath = "Assets/_Game/Prefabs/Enemies/ActionFoundation/PF_Enemy_SciFiSoldier_EliteDeck.prefab";
        private const string Forge3DLineTurretCandidate = "FORGE3D Sci-Fi Effects URP package: TURRET_BASE_Mobile_LOD0 + TURRET_HEAD_Double_LOD0 + TURRET_BARREL_Electric/Gatling/Repeater + railgun/plasma/laser beam effects.";
        private const string Forge3DMissileTurretCandidate = "FORGE3D Sci-Fi Effects URP package: TURRET_BASE_Mobile_LOD0 + TURRET_BARREL_HeavyMissle_Mobile_LOD0 + missile_02/missile_03/missile_04 prefabs.";
        private const string DragonBossCandidate = "HEROIC FANTASY CREATURES FULL PACK VOL3 raw dragon prefabs remain local-only; choose one dragon in a later boss-authoring slice.";

        private static readonly string[] ArchetypeProfilePaths =
        {
            SciFiMeleeSoldierPath,
            SciFiRangedSoldierPath,
            SciFiEliteSoldierPath,
            Forge3DLineTurretPath,
            Forge3DMissileTurretPath,
            DragonBossFuturePath
        };

        [MenuItem("DimensionBrawl/Reapply Action Foundation Enemy Archetypes")]
        public static void ReapplyEnemyArchetypesMenu()
        {
            EnsureEnemyArchetypeAssets();
            Debug.Log("Reapplied ActionFoundation enemy archetype assets.");
        }

        [MenuItem("DimensionBrawl/Validate Action Foundation Enemy Archetypes")]
        public static void ValidateEnemyArchetypesMenu()
        {
            ValidateEnemyArchetypeAssets();
            Debug.Log("Action foundation enemy archetype validation passed.");
        }

        public static void EnsureEnemyArchetypeAssets()
        {
            ActionFoundationEnemyRoleDeckSetup.EnsureEnemyRoleAssets();
            ActionFoundationSciFiSoldier01VisualSetup.EnsureGeneralDeckVisualAssets();
            ActionFoundationSciFiEliteSoldierVisualSetup.EnsureEliteDeckVisualAssets();
            EnsureFolder(ArchetypeRoot);

            GameObject maintenanceWorkerVisual = LoadGameObject(MaintenanceWorkerVisualPath);
            GameObject sciFiSoldier01Visual = LoadGameObject(ActionFoundationSciFiSoldier01VisualSetup.ModelPath);
            GameObject heavyBattleArmorVisual = LoadGameObject(ActionFoundationSciFiEliteSoldierVisualSetup.ModelPath);
            RoleRefs roles = LoadRoleRefs();

            ConfigureSoldierArchetypes(roles, maintenanceWorkerVisual, sciFiSoldier01Visual, heavyBattleArmorVisual);
            ConfigureStaticTurretArchetypes(roles);
            ConfigureFutureBossArchetype();

            AssetDatabase.SaveAssets();
        }

        public static void ValidateEnemyArchetypeAssets()
        {
            var coveredRoleIds = new HashSet<string>();
            bool hasStaticTurretCandidate = false;
            bool hasBossCandidate = false;

            for (int i = 0; i < ArchetypeProfilePaths.Length; i++)
            {
                CombatEnemyArchetypeProfile archetype = AssetDatabase.LoadAssetAtPath<CombatEnemyArchetypeProfile>(ArchetypeProfilePaths[i]);
                if (archetype == null)
                {
                    throw new InvalidOperationException($"Missing enemy archetype profile at {ArchetypeProfilePaths[i]}.");
                }

                ValidateArchetype(archetype, coveredRoleIds);
                hasStaticTurretCandidate |= archetype.ArchetypeKind == CombatEnemyArchetypeKind.StaticTurret;
                hasBossCandidate |= archetype.ArchetypeKind == CombatEnemyArchetypeKind.BossCandidate;
            }

            RequireCoveredRole(coveredRoleIds, "SciFiSoldier.EntryProbe");
            RequireCoveredRole(coveredRoleIds, "SciFiSoldier.CloseGuard");
            RequireCoveredRole(coveredRoleIds, "SciFiSoldier.LungeChaser");
            RequireCoveredRole(coveredRoleIds, "SciFiSoldier.Skirmisher");
            RequireCoveredRole(coveredRoleIds, "SciFiSoldier.LineCaster");
            RequireCoveredRole(coveredRoleIds, "SciFiSoldier.FanSuppressor");
            RequireCoveredRole(coveredRoleIds, "SciFiSoldier.BacklineShooter");
            RequireCoveredRole(coveredRoleIds, "SciFiSoldier.Elite.ShieldBreaker");
            RequireCoveredRole(coveredRoleIds, "SciFiSoldier.Elite.AuraCaptain");
            RequireCoveredRole(coveredRoleIds, "SciFiSoldier.Elite.SummonCaller");
            RequireCoveredRole(coveredRoleIds, "SciFiSoldier.Elite.PhaseDuelist");
            RequireCoveredRole(coveredRoleIds, "SciFiSoldier.Elite.FinalStandCommander");

            if (!hasStaticTurretCandidate)
            {
                throw new InvalidOperationException("Enemy archetype catalog should include at least one static turret candidate.");
            }

            if (!hasBossCandidate)
            {
                throw new InvalidOperationException("Enemy archetype catalog should track the future dragon boss candidate outside soldier role decks.");
            }
        }

        private static void ConfigureSoldierArchetypes(
            RoleRefs roles,
            GameObject maintenanceWorkerVisual,
            GameObject sciFiSoldier01Visual,
            GameObject heavyBattleArmorVisual)
        {
            GameObject meleeSoldierPrefab = LoadOptionalGameObject(MeleeSoldierGameplayPrefabPath);
            GameObject generalDeckSoldierPrefab = LoadOptionalGameObject(GeneralDeckSoldierGameplayPrefabPath);
            GameObject eliteDeckSoldierPrefab = LoadOptionalGameObject(EliteDeckSoldierGameplayPrefabPath);

            ConfigureArchetype(
                LoadOrCreate<CombatEnemyArchetypeProfile>(SciFiMeleeSoldierPath),
                "SciFiSoldier.Melee",
                "Sci-fi Melee Soldier",
                CombatEnemyArchetypeKind.MobileSoldier,
                true,
                new[] { roles.EntryProbe, roles.CloseGuard, roles.LungeChaser, roles.Skirmisher },
                meleeSoldierPrefab,
                maintenanceWorkerVisual,
                meleeSoldierPrefab == null,
                true,
                "Promoted game-owned MaintenanceWorker visual.",
                meleeSoldierPrefab != null
                    ? "Use the authored `_Game` melee soldier prefab candidate as the first reusable close-combat baseline."
                    : "Generate the authored melee soldier prefab candidate before assigning reusable gameplay prefab refs.",
                "Covers close guard, chase, skirmish, and entry-read roles without changing role decks.");

            ConfigureArchetype(
                LoadOrCreate<CombatEnemyArchetypeProfile>(SciFiRangedSoldierPath),
                "SciFiSoldier.Ranged",
                "Sci-fi Ranged Soldier",
                CombatEnemyArchetypeKind.MobileSoldier,
                true,
                new[] { roles.LineCaster, roles.FanSuppressor, roles.BacklineShooter },
                generalDeckSoldierPrefab,
                sciFiSoldier01Visual,
                generalDeckSoldierPrefab == null,
                true,
                "Promoted game-owned SciFiSoldier_01 Commando visual with assault-rifle animation set.",
                generalDeckSoldierPrefab != null
                    ? "Use the authored GeneralDeck prefab candidate for line, fan, and backline role review."
                    : "Generate the authored GeneralDeck prefab candidate before assigning reusable ranged gameplay prefab refs.",
                "Covers line, fan, and backline pressure with a distinct rifle soldier while behavior remains in pattern/deck data.");

            ConfigureArchetype(
                LoadOrCreate<CombatEnemyArchetypeProfile>(SciFiEliteSoldierPath),
                "SciFiSoldier.Elite",
                "Sci-fi Elite Soldier",
                CombatEnemyArchetypeKind.MobileSoldier,
                true,
                new[]
                {
                    roles.ShieldBreakerElite,
                    roles.AuraCaptainElite,
                    roles.SummonCallerElite,
                    roles.PhaseDuelistElite,
                    roles.FinalStandCommanderElite
                },
                eliteDeckSoldierPrefab,
                heavyBattleArmorVisual,
                eliteDeckSoldierPrefab == null,
                true,
                "Promoted game-owned SciFiHeavyBattleArmor visual with elite animation set.",
                eliteDeckSoldierPrefab != null
                    ? "Use the authored EliteDeck prefab candidate for shield, aura, summon-signal, phase, and final-stand role review."
                    : "Generate the authored EliteDeck prefab candidate before assigning reusable elite gameplay prefab refs.",
                "Covers all five elite soldier roles with a stronger silhouette while boss-only behavior remains out of scope.");
        }

        private static void ConfigureStaticTurretArchetypes(RoleRefs roles)
        {
            ConfigureArchetype(
                LoadOrCreate<CombatEnemyArchetypeProfile>(Forge3DLineTurretPath),
                "Forge3D.LineTurret",
                "FORGE3D Line Turret Candidate",
                CombatEnemyArchetypeKind.StaticTurret,
                true,
                new[] { roles.LineCaster, roles.BacklineShooter },
                null,
                null,
                true,
                false,
                Forge3DLineTurretCandidate,
                "Import/promote only the reviewed URP turret parts and beam VFX into `_Game` before assigning prefab refs.",
                "Good first fixed enemy for lane/line pressure; do not put raw package prefabs into role assets.");

            ConfigureArchetype(
                LoadOrCreate<CombatEnemyArchetypeProfile>(Forge3DMissileTurretPath),
                "Forge3D.MissileTurret",
                "FORGE3D Missile Turret Candidate",
                CombatEnemyArchetypeKind.StaticTurret,
                true,
                new[] { roles.FanSuppressor, roles.BacklineShooter, roles.SummonCallerElite },
                null,
                null,
                true,
                false,
                Forge3DMissileTurretCandidate,
                "Promote missile turret parts and one projectile/VFX chain only after fixed-turret gameplay is accepted.",
                "Pressure-rescue or priority-target candidate; it remains data-only until a turret prefab is authored.");
        }

        private static void ConfigureFutureBossArchetype()
        {
            ConfigureArchetype(
                LoadOrCreate<CombatEnemyArchetypeProfile>(DragonBossFuturePath),
                "DragonBoss.Future",
                "Dragon Boss Future Candidate",
                CombatEnemyArchetypeKind.BossCandidate,
                false,
                Array.Empty<CombatEnemyRoleProfile>(),
                null,
                null,
                true,
                false,
                DragonBossCandidate,
                "Keep dragon promotion for a separate boss slice with its own boss pattern profiles.",
                "Tracked here only to prevent forcing boss art into soldier role decks.");
        }

        private static void ValidateArchetype(CombatEnemyArchetypeProfile archetype, HashSet<string> coveredRoleIds)
        {
            if (string.IsNullOrWhiteSpace(archetype.ArchetypeId))
            {
                throw new InvalidOperationException($"{archetype.name} has no archetype id.");
            }

            if (archetype.ParticipatesInActionFoundationRoleMap && archetype.CompatibleRoleCount == 0)
            {
                throw new InvalidOperationException($"{archetype.ArchetypeId} participates in the role map but has no compatible roles.");
            }

            for (int i = 0; i < archetype.CompatibleRoleCount; i++)
            {
                CombatEnemyRoleProfile role = archetype.GetCompatibleRole(i);
                if (role == null)
                {
                    throw new InvalidOperationException($"{archetype.ArchetypeId} has an empty compatible role at index {i}.");
                }

                coveredRoleIds.Add(role.RoleId);
            }

            ValidateGameOwnedObjectReference(archetype, archetype.GameplayPrefab, "gameplay prefab");
            ValidateGameOwnedObjectReference(archetype, archetype.VisualPrefab, "visual prefab");

            if (archetype.ArchetypeKind == CombatEnemyArchetypeKind.StaticTurret && !archetype.RequiresDedicatedPrefabPromotion)
            {
                throw new InvalidOperationException($"{archetype.ArchetypeId} should remain marked for dedicated prefab promotion until a `_Game` turret prefab exists.");
            }
        }

        private static void ValidateGameOwnedObjectReference(CombatEnemyArchetypeProfile archetype, GameObject value, string label)
        {
            if (value == null)
            {
                return;
            }

            string assetPath = AssetDatabase.GetAssetPath(value).Replace('\\', '/');
            if (!assetPath.StartsWith("Assets/_Game/", StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"{archetype.ArchetypeId} {label} must reference a promoted `_Game` asset, found {assetPath}.");
            }

            if (assetPath.Contains("/_Imported/", StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"{archetype.ArchetypeId} {label} must not reference raw imported assets.");
            }
        }

        private static void RequireCoveredRole(HashSet<string> coveredRoleIds, string roleId)
        {
            if (!coveredRoleIds.Contains(roleId))
            {
                throw new InvalidOperationException($"Enemy archetype catalog does not cover role {roleId}.");
            }
        }

        private static RoleRefs LoadRoleRefs()
        {
            return new RoleRefs
            {
                EntryProbe = LoadRole(ActionFoundationEnemyRoleDeckSetup.EntryProbeRolePath),
                CloseGuard = LoadRole(ActionFoundationEnemyRoleDeckSetup.CloseGuardRolePath),
                LungeChaser = LoadRole(ActionFoundationEnemyRoleDeckSetup.LungeChaserRolePath),
                LineCaster = LoadRole(ActionFoundationEnemyRoleDeckSetup.LineCasterRolePath),
                FanSuppressor = LoadRole(ActionFoundationEnemyRoleDeckSetup.FanSuppressorRolePath),
                BacklineShooter = LoadRole(ActionFoundationEnemyRoleDeckSetup.BacklineShooterRolePath),
                Skirmisher = LoadRole(ActionFoundationEnemyRoleDeckSetup.SkirmisherRolePath),
                ShieldBreakerElite = LoadRole(ActionFoundationEnemyRoleDeckSetup.ShieldBreakerEliteRolePath),
                AuraCaptainElite = LoadRole(ActionFoundationEnemyRoleDeckSetup.AuraCaptainEliteRolePath),
                SummonCallerElite = LoadRole(ActionFoundationEnemyRoleDeckSetup.SummonCallerEliteRolePath),
                PhaseDuelistElite = LoadRole(ActionFoundationEnemyRoleDeckSetup.PhaseDuelistEliteRolePath),
                FinalStandCommanderElite = LoadRole(ActionFoundationEnemyRoleDeckSetup.FinalStandCommanderEliteRolePath)
            };
        }

        private static void ConfigureArchetype(
            CombatEnemyArchetypeProfile archetype,
            string archetypeId,
            string displayName,
            CombatEnemyArchetypeKind archetypeKind,
            bool participatesInActionFoundationRoleMap,
            CombatEnemyRoleProfile[] compatibleRoles,
            GameObject gameplayPrefab,
            GameObject visualPrefab,
            bool requiresDedicatedPrefabPromotion,
            bool candidateForFutureSummonAiReuse,
            string sourceCandidate,
            string promotionPlan,
            string usageNotes)
        {
            SerializedObject serializedObject = new SerializedObject(archetype);
            SetString(serializedObject, "archetypeId", archetypeId);
            SetString(serializedObject, "displayName", displayName);
            SetEnum(serializedObject, "archetypeKind", (int)archetypeKind);
            SetBool(serializedObject, "participatesInActionFoundationRoleMap", participatesInActionFoundationRoleMap);
            SetObjectReferenceArray(serializedObject, "compatibleRoles", compatibleRoles);
            SetObjectReference(serializedObject, "gameplayPrefab", gameplayPrefab);
            SetObjectReference(serializedObject, "visualPrefab", visualPrefab);
            SetBool(serializedObject, "requiresDedicatedPrefabPromotion", requiresDedicatedPrefabPromotion);
            SetBool(serializedObject, "candidateForFutureSummonAiReuse", candidateForFutureSummonAiReuse);
            SetString(serializedObject, "sourceCandidate", sourceCandidate);
            SetString(serializedObject, "promotionPlan", promotionPlan);
            SetString(serializedObject, "usageNotes", usageNotes);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(archetype);
        }

        private static CombatEnemyRoleProfile LoadRole(string assetPath)
        {
            CombatEnemyRoleProfile role = AssetDatabase.LoadAssetAtPath<CombatEnemyRoleProfile>(assetPath);
            if (role == null)
            {
                throw new InvalidOperationException($"Missing enemy role profile at {assetPath}.");
            }

            return role;
        }

        private static GameObject LoadGameObject(string assetPath)
        {
            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (asset == null)
            {
                throw new InvalidOperationException($"Missing game object asset at {assetPath}.");
            }

            return asset;
        }

        private static GameObject LoadOptionalGameObject(string assetPath)
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
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

        private static void SetString(SerializedObject serializedObject, string propertyName, string value)
        {
            RequireProperty(serializedObject, propertyName).stringValue = value;
        }

        private static void SetBool(SerializedObject serializedObject, string propertyName, bool value)
        {
            RequireProperty(serializedObject, propertyName).boolValue = value;
        }

        private static void SetEnum(SerializedObject serializedObject, string propertyName, int value)
        {
            RequireProperty(serializedObject, propertyName).enumValueIndex = value;
        }

        private static void SetObjectReference(SerializedObject serializedObject, string propertyName, UnityEngine.Object value)
        {
            RequireProperty(serializedObject, propertyName).objectReferenceValue = value;
        }

        private static void SetObjectReferenceArray(SerializedObject serializedObject, string propertyName, UnityEngine.Object[] values)
        {
            SerializedProperty array = RequireProperty(serializedObject, propertyName);
            array.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                array.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
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

        private struct RoleRefs
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
        }
    }
}
