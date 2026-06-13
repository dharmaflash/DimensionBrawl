using System;
using System.Collections.Generic;
using System.Linq;
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
            ActionFoundationForge3DLineTurretSetup.EnsureLineTurretVisualCandidate();
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
            GameObject lineTurretVisualPrefab = LoadOptionalGameObject(ActionFoundationForge3DLineTurretSetup.PrefabPath);

            ConfigureArchetype(
                LoadOrCreate<CombatEnemyArchetypeProfile>(Forge3DLineTurretPath),
                "Forge3D.LineTurret",
                "FORGE3D Line Turret Candidate",
                CombatEnemyArchetypeKind.StaticTurret,
                true,
                new[] { roles.LineCaster, roles.BacklineShooter },
                null,
                lineTurretVisualPrefab,
                true,
                false,
                "FORGE3D Sci-Fi Effects URP package: selected Mobile Base + Double Head + Energy Breech + Electric Barrel visual candidate promoted into `_Game`.",
                lineTurretVisualPrefab != null
                    ? "Visual candidate is promoted. Keep gameplay prefab promotion pending until a dedicated static-turret controller, hit shape, and reviewed beam cue are authored."
                    : "Promote only the reviewed URP turret parts and beam VFX into `_Game` before assigning prefab refs.",
                "Good first fixed enemy for lane/line pressure; visual candidate is ready for review, but do not put raw package prefabs or turret gameplay into role assets yet.");

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
                throw new InvalidOperationException($"{archetype.ArchetypeId} should remain marked for dedicated prefab promotion until a gameplay-ready `_Game` turret prefab exists.");
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

    public static class ActionFoundationForge3DLineTurretSetup
    {
        public const string VisualName = "PF_Enemy_FORGE3D_LineTurret_Presentation";
        public const string ModelPath = VisualRoot + "/Models/SM_FORGE3D_Turret.fbx";
        public const string PrefabPath = "Assets/_Game/Prefabs/Enemies/ActionFoundation/PF_Enemy_FORGE3D_LineTurret_Presentation.prefab";

        private const string VisualRoot = "Assets/_Game/Art/Characters/Enemies/Forge3D/LineTurret";
        private const string MaterialRoot = VisualRoot + "/Materials";
        private const string TextureRoot = VisualRoot + "/Textures";
        private const string MaterialPath = MaterialRoot + "/M_FORGE3D_LineTurret_Mobile.mat";
        private const string DiffuseTexturePath = TextureRoot + "/T_FORGE3D_Turret_Mobile_DiffuseSpec.png";
        private const string NormalTexturePath = TextureRoot + "/T_FORGE3D_Turret_Mobile_Normal_GL.png";

        private const string BaseMeshName = "TURRET_Base_LOD1";
        private const string SwivelMeshName = "TURRET_Swivel_LOD1";
        private const string MountMeshName = "MOUNT_Double_LOD1";
        private const string HeadMeshName = "TURRET_Head_Double_LOD1";
        private const string BreechMeshName = "BREECH_Energy_LOD1";
        private const string BarrelMeshName = "BARREL_Electric_LOD1";
        private const string YawPivotName = "YawPivot";
        private const string PitchPivotName = "PitchPivot";
        private const string MuzzleLeftName = "Muzzle_Left";
        private const string MuzzleRightName = "Muzzle_Right";

        [MenuItem("DimensionBrawl/Reapply Action Foundation FORGE3D Line Turret Visual")]
        public static void ReapplyLineTurretVisualMenu()
        {
            EnsureLineTurretVisualCandidate();
            Debug.Log("Reapplied FORGE3D line turret visual candidate.");
        }

        [MenuItem("DimensionBrawl/Validate Action Foundation FORGE3D Line Turret Visual")]
        public static void ValidateLineTurretVisualMenu()
        {
            ValidateLineTurretVisualCandidate();
            Debug.Log("ActionFoundation FORGE3D line turret visual validation passed.");
        }

        public static void EnsureLineTurretVisualCandidate()
        {
            EnsureFolder(VisualRoot);
            EnsureFolder(VisualRoot + "/Models");
            EnsureFolder(TextureRoot);
            EnsureFolder(MaterialRoot);
            EnsureFolder(ActionFoundationEnemyPrefabSetup.PrefabRoot);

            ConfigureModelImporter();
            ConfigureTextureImporter(DiffuseTexturePath, TextureUsage.Color);
            ConfigureTextureImporter(NormalTexturePath, TextureUsage.Normal);

            Material material = EnsureMaterial();
            GameObject prefabRoot = BuildVisualPrefab(material);
            try
            {
                GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(prefabRoot, PrefabPath);
                if (savedPrefab == null)
                {
                    throw new InvalidOperationException($"Failed to save FORGE3D line turret visual prefab at {PrefabPath}.");
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(prefabRoot);
            }

            AssetDatabase.SaveAssets();
        }

        public static void ValidateLineTurretVisualCandidate()
        {
            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefabAsset == null)
            {
                throw new InvalidOperationException($"Missing FORGE3D line turret visual prefab at {PrefabPath}.");
            }

            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                if (!string.Equals(prefabRoot.name, VisualName, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException($"FORGE3D line turret visual root should be named {VisualName}.");
                }

                RequireChild(prefabRoot.transform, YawPivotName);
                RequireChild(prefabRoot.transform, YawPivotName + "/" + PitchPivotName);
                RequireChild(prefabRoot.transform, YawPivotName + "/" + PitchPivotName + "/" + MuzzleLeftName);
                RequireChild(prefabRoot.transform, YawPivotName + "/" + PitchPivotName + "/" + MuzzleRightName);

                Renderer[] renderers = prefabRoot.GetComponentsInChildren<Renderer>(includeInactive: true);
                if (renderers.Length < 7)
                {
                    throw new InvalidOperationException($"{VisualName} should keep the selected base, swivel, mount, head, breech, and barrel renderers.");
                }

                for (int i = 0; i < renderers.Length; i++)
                {
                    ValidateRenderer(renderers[i]);
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        private static GameObject BuildVisualPrefab(Material material)
        {
            Mesh baseMesh = LoadMesh(BaseMeshName);
            Mesh swivelMesh = LoadMesh(SwivelMeshName);
            Mesh mountMesh = LoadMesh(MountMeshName);
            Mesh headMesh = LoadMesh(HeadMeshName);
            Mesh breechMesh = LoadMesh(BreechMeshName);
            Mesh barrelMesh = LoadMesh(BarrelMeshName);

            var root = new GameObject(VisualName);
            AddMeshChild(root.transform, "Base", baseMesh, material, Vector3.zero);

            Transform yawPivot = AddEmptyChild(root.transform, YawPivotName, new Vector3(0f, 1.1260008f, 0f));
            AddMeshChild(yawPivot, "Swivel", swivelMesh, material, Vector3.zero);

            Transform pitchPivot = AddEmptyChild(yawPivot, PitchPivotName, new Vector3(0f, 0.75f, -0.069f));
            AddMeshChild(pitchPivot, "Mount_Double", mountMesh, material, Vector3.zero);
            AddMeshChild(pitchPivot, "Head_Double", headMesh, material, new Vector3(0f, 0f, 0.069f));

            AddWeaponSide(pitchPivot, "Left", new Vector3(-0.79f, -0.048f, 0.086f), breechMesh, barrelMesh, material);
            AddWeaponSide(pitchPivot, "Right", new Vector3(0.78f, -0.048f, 0.086f), breechMesh, barrelMesh, material);
            AddEmptyChild(pitchPivot, MuzzleLeftName, new Vector3(-0.79f, 0.038f, 2.45f));
            AddEmptyChild(pitchPivot, MuzzleRightName, new Vector3(0.78f, 0.038f, 2.45f));

            return root;
        }

        private static void AddWeaponSide(
            Transform parent,
            string side,
            Vector3 breechPosition,
            Mesh breechMesh,
            Mesh barrelMesh,
            Material material)
        {
            Transform breechPivot = AddEmptyChild(parent, $"Breech_{side}", breechPosition);
            AddMeshChild(breechPivot, $"BreechMesh_{side}", breechMesh, material, Vector3.zero);
            Transform barrelSocket = AddEmptyChild(breechPivot, $"BarrelSocket_{side}", new Vector3(0f, -0.074f, 0.723f));
            AddMeshChild(barrelSocket, $"Barrel_Electric_{side}", barrelMesh, material, Vector3.zero);
        }

        private static Transform AddEmptyChild(Transform parent, string childName, Vector3 localPosition)
        {
            var child = new GameObject(childName);
            child.transform.SetParent(parent, worldPositionStays: false);
            child.transform.localPosition = localPosition;
            child.transform.localRotation = Quaternion.identity;
            child.transform.localScale = Vector3.one;
            return child.transform;
        }

        private static void AddMeshChild(Transform parent, string childName, Mesh mesh, Material material, Vector3 localPosition)
        {
            Transform child = AddEmptyChild(parent, childName, localPosition);
            MeshFilter meshFilter = child.gameObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = child.gameObject.AddComponent<MeshRenderer>();
            meshFilter.sharedMesh = mesh;
            meshRenderer.sharedMaterial = material;
        }

        private static Mesh LoadMesh(string meshName)
        {
            Mesh mesh = AssetDatabase
                .LoadAllAssetsAtPath(ModelPath)
                .OfType<Mesh>()
                .FirstOrDefault(candidate => string.Equals(candidate.name, meshName, StringComparison.Ordinal));
            if (mesh == null)
            {
                string available = string.Join(", ", AssetDatabase.LoadAllAssetsAtPath(ModelPath).OfType<Mesh>().Select(candidate => candidate.name));
                throw new InvalidOperationException($"Missing FORGE3D turret mesh {meshName} in {ModelPath}. Available meshes: {available}");
            }

            return mesh;
        }

        private static Material EnsureMaterial()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Material material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, MaterialPath);
            }
            else
            {
                material.shader = shader;
            }

            Texture diffuse = LoadAsset<Texture>(DiffuseTexturePath);
            Texture normal = LoadAsset<Texture>(NormalTexturePath);
            SetTextureIfAvailable(material, "_BaseMap", diffuse);
            SetTextureIfAvailable(material, "_MainTex", diffuse);
            SetTextureIfAvailable(material, "_BumpMap", normal);
            SetFloatIfAvailable(material, "_Metallic", 0.28f);
            SetFloatIfAvailable(material, "_Smoothness", 0.56f);
            material.EnableKeyword("_NORMALMAP");
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void ConfigureModelImporter()
        {
            ModelImporter importer = AssetImporter.GetAtPath(ModelPath) as ModelImporter;
            if (importer == null)
            {
                throw new InvalidOperationException($"Missing FORGE3D turret model importer at {ModelPath}. Promote the selected TURRET.FBX into `_Game` before reapplying this setup.");
            }

            importer.globalScale = 0.01f;
            importer.importAnimation = false;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.isReadable = true;
            importer.SaveAndReimport();
        }

        private static void ConfigureTextureImporter(string path, TextureUsage usage)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                throw new InvalidOperationException($"Missing FORGE3D turret texture importer at {path}.");
            }

            importer.textureType = usage == TextureUsage.Normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
            importer.sRGBTexture = usage == TextureUsage.Color;
            importer.SaveAndReimport();
        }

        private static void ValidateRenderer(Renderer renderer)
        {
            if (renderer.sharedMaterial == null)
            {
                throw new InvalidOperationException($"{renderer.name} has no FORGE3D line turret material.");
            }

            ValidateAssetPath(renderer.sharedMaterial, $"{renderer.name} material");
            string[] textureProperties = renderer.sharedMaterial.GetTexturePropertyNames();
            for (int i = 0; i < textureProperties.Length; i++)
            {
                Texture texture = renderer.sharedMaterial.GetTexture(textureProperties[i]);
                if (texture != null)
                {
                    ValidateAssetPath(texture, $"{renderer.sharedMaterial.name}.{textureProperties[i]}");
                }
            }

            MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
            if (meshFilter == null || meshFilter.sharedMesh == null)
            {
                throw new InvalidOperationException($"{renderer.name} should have a promoted turret mesh.");
            }

            ValidateAssetPath(meshFilter.sharedMesh, $"{renderer.name} mesh");
        }

        private static void ValidateAssetPath(UnityEngine.Object asset, string label)
        {
            string assetPath = AssetDatabase.GetAssetPath(asset).Replace('\\', '/');
            if (!assetPath.StartsWith("Assets/_Game/", StringComparison.Ordinal) || assetPath.Contains("/_Imported/", StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"{label} should reference a game-owned asset, found {assetPath}.");
            }
        }

        private static Transform RequireChild(Transform root, string path)
        {
            Transform child = root.Find(path);
            if (child == null)
            {
                throw new InvalidOperationException($"{root.name} is missing child {path}.");
            }

            return child;
        }

        private static T LoadAsset<T>(string assetPath) where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset == null)
            {
                throw new InvalidOperationException($"Missing FORGE3D line turret asset at {assetPath}.");
            }

            return asset;
        }

        private static void SetTextureIfAvailable(Material material, string propertyName, Texture texture)
        {
            if (material.HasProperty(propertyName))
            {
                material.SetTexture(propertyName, texture);
            }
        }

        private static void SetFloatIfAvailable(Material material, string propertyName, float value)
        {
            if (material.HasProperty(propertyName))
            {
                material.SetFloat(propertyName, value);
            }
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

        private enum TextureUsage
        {
            Color,
            Normal
        }
    }
}
