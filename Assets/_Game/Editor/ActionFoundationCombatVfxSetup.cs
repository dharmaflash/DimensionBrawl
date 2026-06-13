using System;
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

        private const string ScenePath = ActionFoundationProfileSetup.ScenePath;
        private const string CombatVfxRoot = "Assets/_Game/Art/VFX/CombatCues";
        private const string MaterialRoot = CombatVfxRoot + "/Materials";
        private const string PrefabRoot = CombatVfxRoot + "/Prefabs";
        private const string PoolRootName = "ActionFoundation_CombatVfxPool";

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

        [MenuItem("DimensionBrawl/Validate Action Foundation Combat VFX Cues")]
        public static void ValidateCombatVfxCuesMenu()
        {
            CombatVfxCueProfile profile = AssetDatabase.LoadAssetAtPath<CombatVfxCueProfile>(CombatVfxCueProfilePath);
            if (profile == null)
            {
                throw new InvalidOperationException($"Missing combat VFX cue profile at {CombatVfxCueProfilePath}.");
            }

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

                if (cue.Prefab.GetComponentInChildren<ParticleSystem>(includeInactive: true) != null)
                {
                    throw new InvalidOperationException($"{cueId} should not use first-pass particle shards for ActionFoundation readability.");
                }
            }

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
                throw new InvalidOperationException("ActionFoundationTest has no BasicSoldierEnemy samples to validate.");
            }

            for (int i = 0; i < soldiers.Length; i++)
            {
                ValidateEnemyCombatVfx(soldiers[i], profile);
            }

            ValidateProjectileCueVisual(profile, CombatVfxCueId.EnemyLinePressureActive, 3f);
            ValidateProjectileCueVisual(profile, CombatVfxCueId.EnemyRetreatShotActive, 2.8f);

            Debug.Log("Action foundation combat VFX cue validation passed.");
        }

        public static CombatVfxCueProfile EnsureCombatVfxAssets()
        {
            EnsureFolder("Assets/_Game/Art/VFX");
            EnsureFolder(CombatVfxRoot);
            EnsureFolder(MaterialRoot);
            EnsureFolder(PrefabRoot);

            Material cyan = LoadOrCreateParticleMaterial("DB_CombatVfx_Cyan", new Color(0.22f, 0.88f, 1f, 0.82f));
            Material blue = LoadOrCreateParticleMaterial("DB_CombatVfx_Blue", new Color(0.18f, 0.45f, 1f, 0.82f));
            Material orange = LoadOrCreateParticleMaterial("DB_CombatVfx_Orange", new Color(1f, 0.42f, 0.08f, 0.86f));
            Material red = LoadOrCreateParticleMaterial("DB_CombatVfx_Red", new Color(1f, 0.12f, 0.06f, 0.86f));
            Material violet = LoadOrCreateParticleMaterial("DB_CombatVfx_Violet", new Color(0.62f, 0.28f, 1f, 0.82f));
            Material gold = LoadOrCreateParticleMaterial("DB_CombatVfx_Gold", new Color(1f, 0.78f, 0.18f, 0.88f));
            Material white = LoadOrCreateParticleMaterial("DB_CombatVfx_White", new Color(0.92f, 0.98f, 1f, 0.9f));
            Material smoke = LoadOrCreateParticleMaterial("DB_CombatVfx_Smoke", new Color(0.45f, 0.52f, 0.58f, 0.55f), additive: false);

            CombatCuePrefabs prefabs = new CombatCuePrefabs
            {
                PlayerAttackStart = SaveBurstPrefab("DB_VFX_PlayerAttackStart", cyan, ParticleSystemShapeType.Cone, 0.16f, 24f, 115f, 0.16f, 0.36f, 0.12f, 0.32f, 18, new Color(0.26f, 0.95f, 1f, 0.86f), new Color(0.14f, 0.42f, 1f, 0f)),
                PlayerAttackHit = SaveBurstPrefab("DB_VFX_PlayerAttackHit", white, ParticleSystemShapeType.Sphere, 0.32f, 35f, 360f, 0.12f, 0.28f, 0.16f, 0.42f, 34, new Color(1f, 0.95f, 0.75f, 0.95f), new Color(0.18f, 0.74f, 1f, 0f)),
                PlayerDodgeStart = SaveBurstPrefab("DB_VFX_PlayerDodgeStart", blue, ParticleSystemShapeType.Cone, 0.22f, 18f, 75f, 0.18f, 0.40f, 0.18f, 0.46f, 28, new Color(0.18f, 0.58f, 1f, 0.75f), new Color(0.1f, 0.2f, 0.7f, 0f)),
                EnemyWindup = SaveBurstPrefab("DB_VFX_EnemyWindup_Generic", orange, ParticleSystemShapeType.Cone, 0.28f, 9f, 150f, 0.28f, 0.54f, 0.10f, 0.30f, 24, new Color(1f, 0.44f, 0.08f, 0.78f), new Color(1f, 0.12f, 0f, 0f)),
                EnemyAttackActive = SaveBurstPrefab("DB_VFX_EnemyAttackActive_Generic", white, ParticleSystemShapeType.Cone, 0.36f, 42f, 120f, 0.10f, 0.24f, 0.22f, 0.55f, 36, new Color(1f, 0.9f, 0.55f, 0.95f), new Color(1f, 0.2f, 0.02f, 0f)),
                EnemyHit = SaveBurstPrefab("DB_VFX_EnemyHit", white, ParticleSystemShapeType.Sphere, 0.28f, 28f, 360f, 0.12f, 0.26f, 0.10f, 0.34f, 24, new Color(1f, 0.96f, 0.72f, 0.9f), new Color(0.8f, 0.16f, 0.04f, 0f)),
                EnemyDeath = SaveBurstPrefab("DB_VFX_EnemyDeath", smoke, ParticleSystemShapeType.Sphere, 0.42f, 16f, 360f, 0.30f, 0.72f, 0.20f, 0.68f, 42, new Color(0.58f, 0.66f, 0.72f, 0.58f), new Color(0.08f, 0.12f, 0.16f, 0f)),
                ClosePunishWindup = SaveBurstPrefab("DB_VFX_ClosePunishWindup", orange, ParticleSystemShapeType.Cone, 0.22f, 12f, 105f, 0.26f, 0.50f, 0.12f, 0.34f, 24, new Color(1f, 0.42f, 0.08f, 0.8f), new Color(1f, 0.06f, 0f, 0f)),
                ClosePunishActive = SaveBurstPrefab("DB_VFX_ClosePunishActive", red, ParticleSystemShapeType.Cone, 0.34f, 42f, 95f, 0.09f, 0.22f, 0.20f, 0.52f, 38, new Color(1f, 0.24f, 0.05f, 0.92f), new Color(1f, 0.8f, 0.16f, 0f)),
                LungeWindup = SaveBurstPrefab("DB_VFX_LungeStrikeWindup", red, ParticleSystemShapeType.Cone, 0.20f, 16f, 60f, 0.24f, 0.48f, 0.14f, 0.34f, 26, new Color(1f, 0.18f, 0.05f, 0.82f), new Color(1f, 0.65f, 0.12f, 0f)),
                LungeActive = SaveBurstPrefab("DB_VFX_LungeStrikeActive", red, ParticleSystemShapeType.Cone, 0.26f, 62f, 45f, 0.10f, 0.26f, 0.24f, 0.62f, 46, new Color(1f, 0.38f, 0.08f, 0.95f), new Color(1f, 0.08f, 0.02f, 0f)),
                HeavyWindup = SaveBurstPrefab("DB_VFX_HeavyWindupCharge", gold, ParticleSystemShapeType.Sphere, 0.34f, 10f, 360f, 0.38f, 0.70f, 0.18f, 0.44f, 42, new Color(1f, 0.74f, 0.12f, 0.86f), new Color(1f, 0.18f, 0.02f, 0f)),
                HeavyActive = SaveBurstPrefab("DB_VFX_HeavyWindupImpact", gold, ParticleSystemShapeType.Circle, 0.65f, 34f, 360f, 0.13f, 0.34f, 0.30f, 0.78f, 56, new Color(1f, 0.9f, 0.36f, 0.96f), new Color(1f, 0.28f, 0.04f, 0f)),
                LineWindup = SaveBurstPrefab("DB_VFX_LinePressureWindup", cyan, ParticleSystemShapeType.Cone, 0.18f, 18f, 34f, 0.30f, 0.58f, 0.10f, 0.24f, 30, new Color(0.22f, 0.94f, 1f, 0.82f), new Color(0.02f, 0.28f, 1f, 0f)),
                LineActive = SaveBurstPrefab("DB_VFX_LinePressureActive", cyan, ParticleSystemShapeType.Cone, 0.20f, 74f, 24f, 0.12f, 0.30f, 0.16f, 0.46f, 58, new Color(0.42f, 0.96f, 1f, 0.96f), new Color(0.04f, 0.3f, 1f, 0f), 3.4f),
                FanWindup = SaveBurstPrefab("DB_VFX_FanPressureWindup", cyan, ParticleSystemShapeType.Cone, 0.24f, 14f, 95f, 0.30f, 0.56f, 0.12f, 0.32f, 34, new Color(0.18f, 0.94f, 0.88f, 0.8f), new Color(0.02f, 0.52f, 0.9f, 0f)),
                FanActive = SaveBurstPrefab("DB_VFX_FanPressureActive", cyan, ParticleSystemShapeType.Cone, 0.36f, 48f, 120f, 0.13f, 0.34f, 0.20f, 0.54f, 64, new Color(0.36f, 1f, 0.86f, 0.94f), new Color(0.02f, 0.58f, 1f, 0f)),
                RetreatShotWindup = SaveBurstPrefab("DB_VFX_RetreatShotWindup", blue, ParticleSystemShapeType.Cone, 0.20f, 18f, 45f, 0.24f, 0.48f, 0.10f, 0.28f, 22, new Color(0.24f, 0.72f, 1f, 0.84f), new Color(0.06f, 0.28f, 1f, 0f)),
                RetreatShotActive = SaveBurstPrefab("DB_VFX_RetreatShotActive", blue, ParticleSystemShapeType.Cone, 0.18f, 70f, 22f, 0.08f, 0.22f, 0.14f, 0.40f, 44, new Color(0.48f, 0.92f, 1f, 0.96f), new Color(0.05f, 0.2f, 1f, 0f), 3.2f),
                RetreatBlinkWindup = SaveBurstPrefab("DB_VFX_RetreatBlinkWindup", violet, ParticleSystemShapeType.Sphere, 0.36f, 12f, 360f, 0.22f, 0.46f, 0.12f, 0.36f, 32, new Color(0.64f, 0.26f, 1f, 0.82f), new Color(0.1f, 0.03f, 0.52f, 0f)),
                RetreatBlinkActive = SaveBurstPrefab("DB_VFX_RetreatBlinkActive", violet, ParticleSystemShapeType.Circle, 0.52f, 38f, 360f, 0.10f, 0.28f, 0.18f, 0.56f, 50, new Color(0.78f, 0.48f, 1f, 0.95f), new Color(0.18f, 0.06f, 0.8f, 0f)),
                GuardBreakWindup = SaveBurstPrefab("DB_VFX_GuardBreakWindup", gold, ParticleSystemShapeType.Sphere, 0.38f, 14f, 360f, 0.42f, 0.78f, 0.20f, 0.48f, 48, new Color(1f, 0.76f, 0.16f, 0.88f), new Color(1f, 0.22f, 0.02f, 0f)),
                GuardBreakActive = SaveBurstPrefab("DB_VFX_GuardBreakActive", gold, ParticleSystemShapeType.Circle, 0.72f, 42f, 360f, 0.14f, 0.36f, 0.34f, 0.86f, 72, new Color(1f, 0.92f, 0.42f, 0.98f), new Color(1f, 0.3f, 0.04f, 0f)),
                EliteShield = SaveBurstPrefab("DB_VFX_EliteShieldSignal", blue, ParticleSystemShapeType.Circle, 0.64f, 20f, 360f, 0.28f, 0.62f, 0.18f, 0.58f, 48, new Color(0.28f, 0.72f, 1f, 0.82f), new Color(0.04f, 0.16f, 1f, 0f)),
                EliteArmorBreak = SaveBurstPrefab("DB_VFX_EliteArmorBreakSignal", gold, ParticleSystemShapeType.Sphere, 0.48f, 24f, 360f, 0.18f, 0.48f, 0.18f, 0.58f, 42, new Color(1f, 0.86f, 0.18f, 0.9f), new Color(1f, 0.18f, 0.02f, 0f)),
                EliteAura = SaveBurstPrefab("DB_VFX_EliteAuraSignal", cyan, ParticleSystemShapeType.Circle, 0.70f, 18f, 360f, 0.42f, 0.84f, 0.16f, 0.52f, 58, new Color(0.18f, 1f, 0.78f, 0.76f), new Color(0.04f, 0.36f, 0.86f, 0f)),
                EliteSummon = SaveBurstPrefab("DB_VFX_EliteSummonSignal", violet, ParticleSystemShapeType.Sphere, 0.58f, 24f, 360f, 0.32f, 0.74f, 0.18f, 0.62f, 64, new Color(0.74f, 0.38f, 1f, 0.86f), new Color(0.12f, 0.04f, 0.7f, 0f)),
                ElitePhaseSwap = SaveBurstPrefab("DB_VFX_ElitePhaseSwapSignal", white, ParticleSystemShapeType.Circle, 0.82f, 44f, 360f, 0.18f, 0.48f, 0.26f, 0.82f, 84, new Color(0.9f, 0.98f, 1f, 0.96f), new Color(0.26f, 0.46f, 1f, 0f))
            };

            CombatVfxCueProfile profile = LoadOrCreate<CombatVfxCueProfile>(CombatVfxCueProfilePath);
            ConfigureCombatVfxCueProfile(profile, prefabs);
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
            Transform attackAnchor = EnsureChild(player.transform, "Player_CombatVfx_AttackAnchor", new Vector3(0f, 1.05f, 0.65f));
            Transform dodgeAnchor = EnsureChild(player.transform, "Player_CombatVfx_DodgeAnchor", new Vector3(0f, 0.18f, -0.22f));

            SetObjectReference(cuePlayer, "profile", profile);
            SetObjectReference(cuePlayer, "pooledRoot", poolRoot);
            SetObjectReference(driver, "actionController", player);
            SetObjectReference(driver, "cuePlayer", cuePlayer);
            SetObjectReference(driver, "attackAnchor", attackAnchor);
            SetObjectReference(driver, "dodgeAnchor", dodgeAnchor);
            EditorUtility.SetDirty(player.gameObject);
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

        private static void ConfigureCombatVfxCueProfile(CombatVfxCueProfile profile, CombatCuePrefabs prefabs)
        {
            CueDefinition[] cues =
            {
                new CueDefinition(CombatVfxCueId.PlayerBasicAttackStart, prefabs.PlayerAttackStart, new Vector3(0f, 0f, 0.35f), Vector3.zero, new Vector3(1f, 1f, 1.25f), 0.40f, false, true),
                new CueDefinition(CombatVfxCueId.PlayerBasicAttackHit, prefabs.PlayerAttackHit, new Vector3(0f, 0f, 0.82f), Vector3.zero, Vector3.one, 0.34f, false, true),
                new CueDefinition(CombatVfxCueId.PlayerDodgeStart, prefabs.PlayerDodgeStart, new Vector3(0f, 0f, -0.15f), Vector3.zero, new Vector3(1.1f, 0.8f, 1.5f), 0.46f, false, true),
                new CueDefinition(CombatVfxCueId.EnemyWindup, prefabs.EnemyWindup, Vector3.zero, Vector3.zero, Vector3.one, 0.55f, true, true),
                new CueDefinition(CombatVfxCueId.EnemyAttackActive, prefabs.EnemyAttackActive, new Vector3(0f, 0f, 0.7f), Vector3.zero, Vector3.one, 0.28f, false, true),
                new CueDefinition(CombatVfxCueId.EnemyHit, prefabs.EnemyHit, new Vector3(0f, 0.1f, 0f), Vector3.zero, Vector3.one, 0.32f, false, true),
                new CueDefinition(CombatVfxCueId.EnemyDeath, prefabs.EnemyDeath, new Vector3(0f, 0.05f, 0f), Vector3.zero, new Vector3(1.25f, 1f, 1.25f), 0.82f, false, false),
                new CueDefinition(CombatVfxCueId.EliteSignal, prefabs.EliteShield, new Vector3(0f, 0.1f, 0f), Vector3.zero, Vector3.one, 0.65f, true, false),
                new CueDefinition(CombatVfxCueId.EnemyClosePunishWindup, prefabs.ClosePunishWindup, Vector3.zero, Vector3.zero, Vector3.one, 0.52f, true, true),
                new CueDefinition(CombatVfxCueId.EnemyClosePunishActive, prefabs.ClosePunishActive, new Vector3(0f, 0f, 0.75f), Vector3.zero, Vector3.one, 0.28f, false, true),
                new CueDefinition(CombatVfxCueId.EnemyLungeStrikeWindup, prefabs.LungeWindup, new Vector3(0f, 0f, 0.25f), Vector3.zero, new Vector3(1f, 1f, 1.3f), 0.50f, true, true),
                new CueDefinition(CombatVfxCueId.EnemyLungeStrikeActive, prefabs.LungeActive, new Vector3(0f, 0f, 1.2f), Vector3.zero, new Vector3(1f, 1f, 1.75f), 0.28f, false, true),
                new CueDefinition(CombatVfxCueId.EnemyHeavyWindupWindup, prefabs.HeavyWindup, new Vector3(0f, 0.12f, 0f), Vector3.zero, new Vector3(1.35f, 1.2f, 1.35f), 0.72f, true, false),
                new CueDefinition(CombatVfxCueId.EnemyHeavyWindupActive, prefabs.HeavyActive, new Vector3(0f, -0.15f, 0.85f), Vector3.zero, new Vector3(1.55f, 0.6f, 1.55f), 0.40f, false, true),
                new CueDefinition(CombatVfxCueId.EnemyLinePressureWindup, prefabs.LineWindup, new Vector3(0f, 0f, 0.45f), Vector3.zero, new Vector3(0.8f, 1f, 2.1f), 0.58f, true, true),
                new CueDefinition(CombatVfxCueId.EnemyLinePressureActive, prefabs.LineActive, new Vector3(0f, 0f, 2.6f), Vector3.zero, new Vector3(0.7f, 1f, 3.2f), 0.32f, false, true),
                new CueDefinition(CombatVfxCueId.EnemyFanPressureWindup, prefabs.FanWindup, new Vector3(0f, 0f, 0.45f), Vector3.zero, new Vector3(1.25f, 1f, 1.65f), 0.58f, true, true),
                new CueDefinition(CombatVfxCueId.EnemyFanPressureActive, prefabs.FanActive, new Vector3(0f, 0f, 1.35f), Vector3.zero, new Vector3(1.6f, 1f, 2.25f), 0.36f, false, true),
                new CueDefinition(CombatVfxCueId.EnemyRetreatShotWindup, prefabs.RetreatShotWindup, new Vector3(0f, 0f, 0.35f), Vector3.zero, Vector3.one, 0.48f, true, true),
                new CueDefinition(CombatVfxCueId.EnemyRetreatShotActive, prefabs.RetreatShotActive, new Vector3(0f, 0f, 1.65f), Vector3.zero, new Vector3(0.8f, 1f, 2.6f), 0.26f, false, true),
                new CueDefinition(CombatVfxCueId.EnemyRetreatBlinkWindup, prefabs.RetreatBlinkWindup, Vector3.zero, Vector3.zero, new Vector3(1.1f, 1.1f, 1.1f), 0.48f, true, false),
                new CueDefinition(CombatVfxCueId.EnemyRetreatBlinkActive, prefabs.RetreatBlinkActive, new Vector3(0f, -0.05f, 0f), Vector3.zero, new Vector3(1.35f, 0.65f, 1.35f), 0.34f, false, false),
                new CueDefinition(CombatVfxCueId.EnemyGuardBreakWindup, prefabs.GuardBreakWindup, new Vector3(0f, 0.1f, 0f), Vector3.zero, new Vector3(1.25f, 1.1f, 1.25f), 0.82f, true, false),
                new CueDefinition(CombatVfxCueId.EnemyGuardBreakActive, prefabs.GuardBreakActive, new Vector3(0f, -0.12f, 0.8f), Vector3.zero, new Vector3(1.55f, 0.65f, 1.55f), 0.42f, false, true),
                new CueDefinition(CombatVfxCueId.EliteShieldSignal, prefabs.EliteShield, Vector3.zero, Vector3.zero, new Vector3(1.2f, 0.9f, 1.2f), 0.72f, true, false),
                new CueDefinition(CombatVfxCueId.EliteArmorBreakSignal, prefabs.EliteArmorBreak, Vector3.zero, Vector3.zero, new Vector3(1.15f, 1f, 1.15f), 0.58f, true, false),
                new CueDefinition(CombatVfxCueId.EliteAuraSignal, prefabs.EliteAura, Vector3.zero, Vector3.zero, new Vector3(1.55f, 0.8f, 1.55f), 0.88f, true, false),
                new CueDefinition(CombatVfxCueId.EliteSummonSignal, prefabs.EliteSummon, Vector3.zero, Vector3.zero, new Vector3(1.45f, 1.1f, 1.45f), 0.86f, true, false),
                new CueDefinition(CombatVfxCueId.ElitePhaseSwapSignal, prefabs.ElitePhaseSwap, Vector3.zero, Vector3.zero, new Vector3(1.85f, 0.8f, 1.85f), 0.68f, true, false)
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

        private static void SetPatternCueOverrides(EnemyCombatVfxCueDriver driver)
        {
            CombatPatternVfxCueOverride[] overrides =
            {
                new CombatPatternVfxCueOverride(LoadPattern(ActionFoundationProfileSetup.EnemyPatternProfilePath), CombatVfxCueId.EnemyClosePunishWindup, CombatVfxCueId.EnemyClosePunishActive, 1f, 1f),
                new CombatPatternVfxCueOverride(LoadPattern(ActionFoundationProfileSetup.EnemyLungePatternProfilePath), CombatVfxCueId.EnemyLungeStrikeWindup, CombatVfxCueId.EnemyLungeStrikeActive, 1.05f, 1.08f),
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
            AddCueGeometry(root, material, shapeType, radius, speed, arcDegrees, minSize, maxSize, renderers);

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
            float forwardTravelDistance)
        {
            SerializedObject serializedObject = new SerializedObject(visual);
            SerializedProperty rendererArray = RequireProperty(serializedObject, "renderers");
            rendererArray.arraySize = renderers.Length;
            for (int i = 0; i < renderers.Length; i++)
            {
                rendererArray.GetArrayElementAtIndex(i).objectReferenceValue = renderers[i];
            }

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
            public GameObject ElitePhaseSwap;
        }
    }
}
