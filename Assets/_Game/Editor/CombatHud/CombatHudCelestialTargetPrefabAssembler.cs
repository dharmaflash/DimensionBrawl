using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using DimensionBrawl.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DimensionBrawl.Editor
{
    /// <summary>
    /// Builds the approved target HUD while retaining canonical functional binding objects.
    /// Review staging and canonical promotion use separate explicit entry points but share the
    /// same validated atomic-role configuration pipeline.
    /// </summary>
    public static class CombatHudCelestialTargetPrefabAssembler
    {
        public const string CanonicalPrefabPath =
            "Assets/_Game/UI/CombatHud/PF_UI_CombatHud.prefab";
        public const string StagingPrefabPath =
            "Assets/_Game/UI/CombatHud/PF_UI_CombatHud_CelestialTarget_Staging.prefab";
        public const string AssemblySpecPath =
            "Assets/_Game/UI/CombatHud/CombatHudCelestialTargetAssemblySpec.json";
        public const string TargetArtRoot =
            "Assets/_Game/UI/CombatHud/Art/CelestialHudTarget/Runtime";

        private const string HudFontPath =
            "Assets/_Game/Art/Fonts/Pretendard/Pretendard-SemiBold.otf";
        private static readonly string[] CanonicalScenePaths =
        {
            "Assets/_Game/Scenes/OlympusStationCombatStage.unity",
            "Assets/_Game/Scenes/OlympusCorridorInvasionStage.unity",
            "Assets/_Game/Scenes/OlympusCourtyardDrillStage.unity"
        };
        private static readonly string[] PromotionTransactionAssetPaths =
        {
            CanonicalPrefabPath,
            CanonicalPrefabPath + ".meta",
            "Assets/_Game/Scenes/OlympusStationCombatStage.unity",
            "Assets/_Game/Scenes/OlympusCorridorInvasionStage.unity",
            "Assets/_Game/Scenes/OlympusCourtyardDrillStage.unity"
        };
        private static readonly HashSet<string> ManagedSceneVisualRoots =
            new HashSet<string>(StringComparer.Ordinal)
            {
                "DimensionHudSkinRoot", "TopLeftPanel", "Objective", "Timer",
                "MissionTimerBacking",
                "SettingsButton", "BossHudRoot", "BossSymbol", "BossNameArea",
                "BossHpBackground", "BossHpFill", "BossCostBackground",
                "BossCostFill", "ActionFeedback", "PauseButton",
                "SummonRailTargetRoot", "SummonRailV22Root", "SummonSlot1Button",
                "SummonSlot2Button",
                "SummonSlot3Button", "UltimateButton", "Skill1Button",
                "DodgeButton", "BasicAttackButton", "MoveJoystickRing",
                "MoveJoystickKnob", "JoystickActivationHit", "PlayerHudTargetRoot",
                "HealthBar_Track", "HealthBar", "HealthText",
                "ResourceBar_Track", "ResourceBar", "ResourceText", "AmmoText",
                "PlayerHudV22Root", "InputMode", "PlayerSymbol", "PlayerNameArea",
                "PlayerHpAmountArea", "PlayerMpAmountArea", "CenterAimReticle"
            };
        private static readonly HashSet<string> LegacySummonVisualNames =
            new HashSet<string>(StringComparer.Ordinal)
            {
                "ReadyGlow", "Icon", "IconDisabled", "ReadyRing", "ReadySparkRing"
            };
        private static readonly HashSet<string> LegacySummonParentNames =
            new HashSet<string>(StringComparer.Ordinal)
            {
                "SummonSlot1Button", "SummonSlot2Button", "SummonSlot3Button"
            };
        private static readonly HashSet<string> PresenterDirectVisualBindings =
            new HashSet<string>(StringComparer.Ordinal)
            {
                "objectiveText", "timerText", "healthText", "resourceText",
                "inputModeText", "ammoText", "actionFeedbackText", "healthFill",
                "resourceFill", "bossHudRoot", "bossHealthText", "bossResourceText",
                "bossHealthFill", "bossResourceFill", "aimReticleRoot",
                "playerDamageOverlayImage"
            };

        private sealed class SceneMigrationAudit
        {
            public string ScenePath;
            public string FunctionalSignature;
            public HashSet<string> LegacyVisualObjectIds;
            public HashSet<string> LegacyOutlineIds;
            public int ManagedOverrideCount;
            public int PresenterBindingOverrideCount;
        }

        private sealed class SceneMigrationResult
        {
            public int RemovedOverrides;
            public int RemovedPresenterBindings;
            public int RemovedLegacyVisualObjects;
            public int RemovedLegacyOutlines;
            public bool Saved;
        }

        private enum DesignAnchor
        {
            LeftTop,
            LeftBottom,
            RightTop,
            RightBottom,
            CenterTop,
            CenterBottom,
            CenterScreen
        }

        [Serializable]
        private sealed class AssemblySpec
        {
            public int version;
            public string artRoot;
            public SpriteSpec[] sprites = Array.Empty<SpriteSpec>();
        }

        [Serializable]
        private sealed class SpriteSpec
        {
            public string role;
            public string path;
            public bool required;
        }

        private sealed class SpriteCatalog
        {
            private readonly Dictionary<string, Sprite> sprites;

            public SpriteCatalog(Dictionary<string, Sprite> sprites)
            {
                this.sprites = sprites;
            }

            public Sprite Require(string role)
            {
                if (!sprites.TryGetValue(role, out Sprite sprite) || sprite == null)
                {
                    throw new InvalidOperationException(
                        $"Target HUD sprite role '{role}' is not loaded.");
                }

                return sprite;
            }

            public bool TryGet(string role, out Sprite sprite)
            {
                return sprites.TryGetValue(role, out sprite) && sprite != null;
            }
        }

        [MenuItem("DimensionBrawl/UI Target/Validate Atomic Asset Pack")]
        public static void ValidateFromMenu()
        {
            ValidateAssetsForBatchMode();
            Debug.Log("Celestial target HUD atomic asset manifest is valid.");
        }

        [MenuItem("DimensionBrawl/UI Target/Build Review Staging Prefab")]
        public static void BuildStagingFromMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            BuildStagingForBatchMode();
        }

        [MenuItem("DimensionBrawl/UI Target/Apply Reviewed Target To Canonical Prefab")]
        public static void ApplyReviewedTargetToCanonicalFromMenu()
        {
            if (!EditorUtility.DisplayDialog(
                    "Apply reviewed Target v23 HUD?",
                    "This updates the canonical combat HUD prefab in place, then selectively "
                        + "removes only legacy visual overrides from the three canonical combat scenes. "
                        + "Scene-owned input and joystick components are preserved and audited.",
                    "Apply Target v23",
                    "Cancel"))
            {
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            ApplyReviewedTargetToCanonicalForBatchMode();
        }

        public static void ValidateAssetsForBatchMode()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            AssemblySpec spec = LoadAndValidateSpec();
            ValidateReferencedFiles(spec);
        }

        public static void BuildStagingForBatchMode()
        {
            Assemble(CanonicalPrefabPath, StagingPrefabPath, "review staging");
        }

        /// <summary>
        /// Read-only scene migration audit used before the explicit canonical transaction.
        /// </summary>
        public static void AuditCanonicalSceneMigrationForBatchMode()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            Dictionary<string, string> before = CaptureAssetHashes(
                PromotionTransactionAssetPaths);
            PreflightCanonicalSceneMigrations();
            Dictionary<string, string> after = CaptureAssetHashes(
                PromotionTransactionAssetPaths);
            string[] changed = before
                .Where(pair => !string.Equals(
                    pair.Value,
                    after[pair.Key],
                    StringComparison.Ordinal))
                .Select(pair => pair.Key)
                .ToArray();
            if (changed.Length > 0)
            {
                throw new InvalidOperationException(
                    "Read-only Target migration audit changed assets: "
                    + string.Join(", ", changed));
            }

            Debug.Log(
                "Target canonical scene migration preflight passed without file writes: "
                + FormatHashes(after));
        }

        /// <summary>
        /// Explicit approved mutation entry point. The canonical asset is loaded and saved at
        /// the same path, then only Target-managed presentation overrides are removed from the
        /// canonical scene instances. A byte-for-byte snapshot is restored if any step fails.
        /// </summary>
        public static void ApplyReviewedTargetToCanonicalForBatchMode()
        {
            Dictionary<string, byte[]> byteSnapshots = CaptureAssetBytes(
                PromotionTransactionAssetPaths);
            Dictionary<string, string> hashesBefore = CaptureAssetHashes(
                PromotionTransactionAssetPaths);
            SceneSetup[] sceneSetupBefore = EditorSceneManager.GetSceneManagerSetup();
            Dictionary<string, SceneMigrationAudit> audits = null;

            Debug.Log(
                "Target canonical transaction snapshot: "
                + FormatHashes(hashesBefore));
            try
            {
                // Every scene must pass a read-only identity and cleanup-scope audit before
                // the canonical prefab or any scene file is written.
                audits = PreflightCanonicalSceneMigrations();
                Assemble(CanonicalPrefabPath, CanonicalPrefabPath, "canonical");

                var results = new Dictionary<string, SceneMigrationResult>(StringComparer.Ordinal);
                for (int i = 0; i < CanonicalScenePaths.Length; i++)
                {
                    string scenePath = CanonicalScenePaths[i];
                    results.Add(scenePath, MigrateCanonicalScene(audits[scenePath]));
                }

                ValidateCanonicalSceneMigrations(audits, results, hashesBefore);
                Debug.Log(
                    "Target canonical transaction committed: "
                    + FormatHashes(CaptureAssetHashes(PromotionTransactionAssetPaths)));
            }
            catch (Exception exception)
            {
                RestoreAssetBytes(byteSnapshots, sceneSetupBefore);
                throw new InvalidOperationException(
                    "Target canonical promotion failed; canonical prefab/meta and all three "
                        + "scene files were restored from the preflight byte snapshot.",
                    exception);
            }
        }

        /// <summary>
        /// Re-applies the already-reviewed Target v23 configuration to the canonical prefab
        /// without opening or migrating canonical scenes. This entry point is intentionally
        /// idempotent: the shared deterministic assembler updates the existing prefab in place,
        /// while GUID/meta/local-ID preservation and byte-identical scene hashes are mandatory.
        /// </summary>
        public static void ApplyTargetLayoutOnlyToCanonicalForBatchMode()
        {
            Dictionary<string, byte[]> byteSnapshots = CaptureAssetBytes(
                PromotionTransactionAssetPaths);
            Dictionary<string, string> hashesBefore = CaptureAssetHashes(
                PromotionTransactionAssetPaths);
            SceneSetup[] sceneSetupBefore = EditorSceneManager.GetSceneManagerSetup();
            Dictionary<string, string> protectedHashesBefore = hashesBefore
                .Where(pair => !string.Equals(
                    pair.Key,
                    CanonicalPrefabPath,
                    StringComparison.Ordinal))
                .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);

            GameObject canonical = RequireAsset<GameObject>(CanonicalPrefabPath);
            CombatHudCelestialTargetLayoutProfile target =
                canonical.GetComponent<CombatHudCelestialTargetLayoutProfile>();
            CombatHudCelestialV2LayoutProfile legacy =
                canonical.GetComponent<CombatHudCelestialV2LayoutProfile>();
            if (target == null
                || target.Version != CombatHudCelestialTargetLayoutProfile.LayoutVersion
                || legacy == null
                || legacy.enabled)
            {
                throw new InvalidOperationException(
                    "Layout-only Target apply requires the canonical prefab to already be "
                        + "Target v23 with its preserved V22 marker disabled.");
            }

            Debug.Log(
                "Target layout-only transaction snapshot: "
                + FormatHashes(hashesBefore));
            try
            {
                // Assemble is deterministic for an existing Target v23 prefab. No scene
                // preflight/migration is invoked by this size-only entry point.
                Assemble(
                    CanonicalPrefabPath,
                    CanonicalPrefabPath,
                    "canonical layout-only",
                    layoutOnly: true);
                ValidateProtectedAssetHashes(protectedHashesBefore);
                Debug.Log(
                    "Target layout-only transaction committed with canonical scenes unchanged: "
                    + FormatHashes(CaptureAssetHashes(PromotionTransactionAssetPaths)));
            }
            catch (Exception exception)
            {
                RestoreAssetBytes(byteSnapshots, sceneSetupBefore);
                throw new InvalidOperationException(
                    "Target layout-only apply failed; canonical prefab/meta and all three "
                        + "scene files were restored from the transaction snapshot.",
                    exception);
            }
        }

        private static void Assemble(
            string sourcePrefabPath,
            string destinationPrefabPath,
            string label,
            bool layoutOnly = false)
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            AssemblySpec spec = null;
            SpriteCatalog sprites = null;
            Font font = null;
            if (!layoutOnly)
            {
                spec = LoadAndValidateSpec();
                ValidateReferencedFiles(spec);
                ConfigureReferencedSpriteImporters(spec);
                sprites = LoadSprites(spec);
                font = RequireAsset<Font>(HudFontPath);
            }
            bool canonicalPromotion = string.Equals(
                sourcePrefabPath,
                CanonicalPrefabPath,
                StringComparison.Ordinal)
                && string.Equals(
                    destinationPrefabPath,
                    CanonicalPrefabPath,
                    StringComparison.Ordinal);

            GameObject source = RequireAsset<GameObject>(sourcePrefabPath);
            if (!string.Equals(source.name, "PF_UI_CombatHud", StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Refusing to assemble unexpected prefab root '{source.name}' at {sourcePrefabPath}.");
            }
            string canonicalGuidBefore = canonicalPromotion
                ? AssetDatabase.AssetPathToGUID(CanonicalPrefabPath)
                : string.Empty;
            HashSet<long> canonicalIdsBefore = canonicalPromotion
                ? CollectPrefabLocalIds(source)
                : null;
            Dictionary<string, string> protectedHashesBefore = canonicalPromotion
                ? CaptureAssetHashes(new[] { CanonicalPrefabPath + ".meta" })
                : null;
            if (canonicalPromotion)
            {
                Debug.Log(
                    "Target promotion preflight: "
                    + $"canonical={ComputeFileSha256(CanonicalPrefabPath)}, "
                    + $"guid={canonicalGuidBefore}, localIds={canonicalIdsBefore.Count}, "
                    + FormatHashes(protectedHashesBefore));
            }

            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(sourcePrefabPath);
            try
            {
                Transform root = prefabRoot.transform;
                ValidateBindingRoots(root);
                if (layoutOnly)
                {
                    ConfigureActionLayoutsOnly(root);
                    ConfigureSummonLayoutsOnly(root);
                    ConfigureJoystickLayoutOnly(root);
                }
                else
                {
                    ConfigureLayoutMarker(prefabRoot, preserveLegacyMarker: canonicalPromotion);
                    ConfigureObjective(root, sprites, font);
                    ConfigureBoss(root, sprites, font);
                    ConfigurePause(root, sprites);
                    ConfigureSummons(root, sprites, font);
                    ConfigureActions(root, sprites, font);
                    ConfigureJoystick(root, sprites);
                    ConfigurePlayer(root, sprites, font);
                    ConfigureReticle(root, sprites);
                    ConfigurePresenterBindings(prefabRoot);
                }
                ValidateRaycastOwnership(root);

                GameObject saved = PrefabUtility.SaveAsPrefabAsset(prefabRoot, destinationPrefabPath);
                if (saved == null)
                {
                    throw new InvalidOperationException(
                        $"Could not save target {label} prefab: {destinationPrefabPath}");
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            if (canonicalPromotion)
            {
                ValidateCanonicalIdentityAfterPromotion(
                    canonicalGuidBefore,
                    canonicalIdsBefore);
                ValidateProtectedAssetHashes(protectedHashesBefore);
                Debug.Log(
                    "Target promotion postflight: "
                    + $"canonical={ComputeFileSha256(CanonicalPrefabPath)}, "
                    + $"guid={AssetDatabase.AssetPathToGUID(CanonicalPrefabPath)}, "
                    + FormatHashes(CaptureAssetHashes(new[] { CanonicalPrefabPath + ".meta" })));
            }
            Debug.Log(
                $"Assembled Target v23 {label} prefab at {destinationPrefabPath}. "
                + "Prefab configuration completed without replacing its asset identity.");
        }

        private static Dictionary<string, byte[]> CaptureAssetBytes(
            IEnumerable<string> assetPaths)
        {
            var result = new Dictionary<string, byte[]>(StringComparer.Ordinal);
            foreach (string assetPath in assetPaths)
            {
                string absolutePath = ToAbsoluteProjectPath(assetPath);
                if (!File.Exists(absolutePath))
                {
                    throw new InvalidOperationException(
                        $"Missing promotion transaction asset: {assetPath}");
                }

                result.Add(assetPath, File.ReadAllBytes(absolutePath));
            }

            return result;
        }

        private static void RestoreAssetBytes(
            IReadOnlyDictionary<string, byte[]> snapshots,
            SceneSetup[] sceneSetupBefore)
        {
            // Reload any transaction scene that was open before the command so its in-memory
            // state cannot retain a partially migrated hierarchy after the on-disk rollback.
            for (int i = 0; i < CanonicalScenePaths.Length; i++)
            {
                Scene loaded = SceneManager.GetSceneByPath(CanonicalScenePaths[i]);
                if (loaded.IsValid() && loaded.isLoaded)
                {
                    EditorSceneManager.CloseScene(loaded, removeScene: true);
                }
            }

            foreach (KeyValuePair<string, byte[]> pair in snapshots)
            {
                File.WriteAllBytes(ToAbsoluteProjectPath(pair.Key), pair.Value);
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            foreach (string assetPath in snapshots.Keys)
            {
                if (assetPath.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                AssetDatabase.ImportAsset(
                    assetPath,
                    ImportAssetOptions.ForceSynchronousImport
                        | ImportAssetOptions.ForceUpdate);
            }
            if (sceneSetupBefore != null && sceneSetupBefore.Length > 0)
            {
                EditorSceneManager.RestoreSceneManagerSetup(sceneSetupBefore);
            }

            foreach (KeyValuePair<string, byte[]> pair in snapshots)
            {
                byte[] restored = File.ReadAllBytes(ToAbsoluteProjectPath(pair.Key));
                if (!restored.SequenceEqual(pair.Value))
                {
                    throw new InvalidOperationException(
                        $"Rollback did not restore byte-identical asset '{pair.Key}'.");
                }
            }

            Debug.Log("Target canonical transaction rollback restored all five files byte-for-byte.");
        }

        private static Dictionary<string, SceneMigrationAudit>
            PreflightCanonicalSceneMigrations()
        {
            var result = new Dictionary<string, SceneMigrationAudit>(StringComparer.Ordinal);
            for (int i = 0; i < CanonicalScenePaths.Length; i++)
            {
                string scenePath = CanonicalScenePaths[i];
                SceneMigrationAudit audit = AuditCanonicalScene(scenePath);
                ValidateExpectedPreflightCounts(audit);
                result.Add(scenePath, audit);
                Debug.Log(
                    $"Target scene preflight {scenePath}: managedOverrides={audit.ManagedOverrideCount}, "
                    + $"presenterBindings={audit.PresenterBindingOverrideCount}, "
                    + $"legacyVisuals={audit.LegacyVisualObjectIds.Count}, "
                    + $"legacyOutlines={audit.LegacyOutlineIds.Count}.");
            }

            return result;
        }

        private static SceneMigrationAudit AuditCanonicalScene(string scenePath)
        {
            Scene scene = SceneManager.GetSceneByPath(scenePath);
            bool openedHere = !scene.IsValid() || !scene.isLoaded;
            if (openedHere)
            {
                scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            }

            try
            {
                if (!scene.IsValid() || !scene.isLoaded)
                {
                    throw new InvalidOperationException(
                        $"Could not open Target migration scene: {scenePath}");
                }
                if (scene.isDirty)
                {
                    throw new InvalidOperationException(
                        $"Target migration preflight refuses dirty scene '{scenePath}'. "
                        + "Save or discard its pending editor changes first.");
                }

                GameObject instanceRoot = RequireCanonicalPrefabInstance(scene);
                ValidateFunctionalSceneContracts(instanceRoot, scenePath);
                GameObject[] legacyVisuals = FindLegacyAddedVisualObjects(instanceRoot);
                Outline[] legacyOutlines = FindLegacyAddedOutlines(instanceRoot);
                PropertyModification[] modifications =
                    PrefabUtility.GetPropertyModifications(instanceRoot)
                    ?? Array.Empty<PropertyModification>();

                return new SceneMigrationAudit
                {
                    ScenePath = scenePath,
                    FunctionalSignature = CaptureFunctionalSceneSignature(instanceRoot),
                    LegacyVisualObjectIds = legacyVisuals
                        .Select(GetGlobalObjectId)
                        .ToHashSet(StringComparer.Ordinal),
                    LegacyOutlineIds = legacyOutlines
                        .Select(GetGlobalObjectId)
                        .ToHashSet(StringComparer.Ordinal),
                    ManagedOverrideCount = modifications.Count(IsTargetManagedSceneOverride),
                    PresenterBindingOverrideCount = modifications.Count(
                        IsPresenterVisualBindingOverride)
                };
            }
            finally
            {
                if (openedHere && scene.IsValid() && scene.isLoaded)
                {
                    EditorSceneManager.CloseScene(scene, removeScene: true);
                }
            }
        }

        private static void ValidateExpectedPreflightCounts(SceneMigrationAudit audit)
        {
            int expectedVisuals;
            int expectedOutlines;
            int expectedPresenterBindings;
            int expectedManagedOverrides;
            if (string.Equals(
                    audit.ScenePath,
                    CanonicalScenePaths[0],
                    StringComparison.Ordinal))
            {
                expectedVisuals = 16;
                expectedOutlines = 9;
                expectedPresenterBindings = 7;
                expectedManagedOverrides = 129;
            }
            else if (string.Equals(
                         audit.ScenePath,
                         CanonicalScenePaths[1],
                         StringComparison.Ordinal))
            {
                expectedVisuals = 16;
                expectedOutlines = 9;
                expectedPresenterBindings = 8;
                expectedManagedOverrides = 129;
            }
            else
            {
                expectedVisuals = 0;
                expectedOutlines = 2;
                expectedPresenterBindings = 0;
                expectedManagedOverrides = 2;
            }

            if (audit.LegacyVisualObjectIds.Count != expectedVisuals
                || audit.LegacyOutlineIds.Count != expectedOutlines
                || audit.PresenterBindingOverrideCount != expectedPresenterBindings
                || audit.ManagedOverrideCount != expectedManagedOverrides)
            {
                throw new InvalidOperationException(
                    $"Target migration preflight scope changed for {audit.ScenePath}: "
                    + $"legacyVisuals {audit.LegacyVisualObjectIds.Count}/{expectedVisuals}, "
                    + $"legacyOutlines {audit.LegacyOutlineIds.Count}/{expectedOutlines}, "
                    + $"presenterBindings {audit.PresenterBindingOverrideCount}/"
                    + $"{expectedPresenterBindings}, managedOverrides "
                    + $"{audit.ManagedOverrideCount}/{expectedManagedOverrides}. "
                    + "Refusing a broader scene mutation.");
            }
        }

        private static SceneMigrationResult MigrateCanonicalScene(SceneMigrationAudit expected)
        {
            Scene scene = SceneManager.GetSceneByPath(expected.ScenePath);
            bool openedHere = !scene.IsValid() || !scene.isLoaded;
            if (openedHere)
            {
                scene = EditorSceneManager.OpenScene(expected.ScenePath, OpenSceneMode.Additive);
            }

            try
            {
                if (!scene.IsValid() || !scene.isLoaded || scene.isDirty)
                {
                    throw new InvalidOperationException(
                        $"Target migration cannot safely edit scene '{expected.ScenePath}'.");
                }

                GameObject instanceRoot = RequireCanonicalPrefabInstance(scene);
                ValidateFunctionalSceneContracts(instanceRoot, expected.ScenePath);
                string functionalBefore = CaptureFunctionalSceneSignature(instanceRoot);
                if (!string.Equals(
                        functionalBefore,
                        expected.FunctionalSignature,
                        StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        $"Functional HUD scene identity changed after prefab promotion in "
                        + $"'{expected.ScenePath}'. Refusing scene cleanup.");
                }

                GameObject[] legacyVisuals = FindLegacyAddedVisualObjects(instanceRoot);
                Outline[] legacyOutlines = FindLegacyAddedOutlines(instanceRoot);
                AssertExactGlobalIdSet(
                    legacyVisuals,
                    expected.LegacyVisualObjectIds,
                    expected.ScenePath,
                    "legacy visual GameObjects");
                AssertExactGlobalIdSet(
                    legacyOutlines,
                    expected.LegacyOutlineIds,
                    expected.ScenePath,
                    "legacy Outline components");

                PropertyModification[] modifications =
                    PrefabUtility.GetPropertyModifications(instanceRoot)
                    ?? Array.Empty<PropertyModification>();
                int presenterRemoved = modifications.Count(IsPresenterVisualBindingOverride);
                PropertyModification[] retained = modifications
                    .Where(modification => !IsTargetManagedSceneOverride(modification))
                    .ToArray();
                int removed = modifications.Length - retained.Length;
                if (presenterRemoved != expected.PresenterBindingOverrideCount
                    || removed != expected.ManagedOverrideCount)
                {
                    throw new InvalidOperationException(
                        $"Target-managed override scope changed for {expected.ScenePath}: "
                        + $"managed {removed}/{expected.ManagedOverrideCount}, presenter "
                        + $"{presenterRemoved}/{expected.PresenterBindingOverrideCount}.");
                }

                if (removed > 0)
                {
                    PrefabUtility.SetPropertyModifications(instanceRoot, retained);
                }

                for (int i = 0; i < legacyOutlines.Length; i++)
                {
                    PrefabUtility.RevertAddedComponent(
                        legacyOutlines[i],
                        InteractionMode.AutomatedAction);
                }
                for (int i = 0; i < legacyVisuals.Length; i++)
                {
                    PrefabUtility.RevertAddedGameObject(
                        legacyVisuals[i],
                        InteractionMode.AutomatedAction);
                }

                bool changed = removed > 0
                    || legacyOutlines.Length > 0
                    || legacyVisuals.Length > 0;
                if (changed)
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                }

                ValidateMigratedSceneState(
                    instanceRoot,
                    expected.ScenePath,
                    expected.FunctionalSignature);
                if (changed && !EditorSceneManager.SaveScene(scene))
                {
                    throw new InvalidOperationException(
                        $"Could not save Target-migrated scene: {expected.ScenePath}");
                }

                ValidateMigratedSceneState(
                    instanceRoot,
                    expected.ScenePath,
                    expected.FunctionalSignature);
                return new SceneMigrationResult
                {
                    RemovedOverrides = removed,
                    RemovedPresenterBindings = presenterRemoved,
                    RemovedLegacyVisualObjects = legacyVisuals.Length,
                    RemovedLegacyOutlines = legacyOutlines.Length,
                    Saved = changed
                };
            }
            finally
            {
                if (openedHere && scene.IsValid() && scene.isLoaded)
                {
                    EditorSceneManager.CloseScene(scene, removeScene: true);
                }
            }
        }

        private static void ValidateCanonicalSceneMigrations(
            IReadOnlyDictionary<string, SceneMigrationAudit> audits,
            IReadOnlyDictionary<string, SceneMigrationResult> results,
            IReadOnlyDictionary<string, string> hashesBefore)
        {
            string metaPath = CanonicalPrefabPath + ".meta";
            if (!string.Equals(
                    hashesBefore[metaPath],
                    ComputeFileSha256(metaPath),
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "Target promotion changed the canonical prefab meta file/GUID.");
            }

            GameObject canonical = RequireAsset<GameObject>(CanonicalPrefabPath);
            CombatHudCelestialTargetLayoutProfile target =
                canonical.GetComponent<CombatHudCelestialTargetLayoutProfile>();
            CombatHudCelestialV2LayoutProfile v22 =
                canonical.GetComponent<CombatHudCelestialV2LayoutProfile>();
            if (target == null
                || target.Version != CombatHudCelestialTargetLayoutProfile.LayoutVersion
                || v22 == null
                || v22.enabled)
            {
                throw new InvalidOperationException(
                    "Canonical HUD marker postflight is not Target v23 with disabled V22 identity.");
            }

            for (int i = 0; i < CanonicalScenePaths.Length; i++)
            {
                string scenePath = CanonicalScenePaths[i];
                SceneMigrationResult result = results[scenePath];
                string hashAfter = ComputeFileSha256(scenePath);
                bool hashChanged = !string.Equals(
                    hashesBefore[scenePath],
                    hashAfter,
                    StringComparison.Ordinal);
                if (hashChanged != result.Saved)
                {
                    throw new InvalidOperationException(
                        $"Scene hash/save invariant failed for {scenePath}: "
                        + $"saved={result.Saved}, hashChanged={hashChanged}.");
                }

                SceneMigrationAudit post = AuditCanonicalScene(scenePath);
                if (!string.Equals(
                        post.FunctionalSignature,
                        audits[scenePath].FunctionalSignature,
                        StringComparison.Ordinal)
                    || post.ManagedOverrideCount != 0
                    || post.PresenterBindingOverrideCount != 0
                    || post.LegacyVisualObjectIds.Count != 0
                    || post.LegacyOutlineIds.Count != 0)
                {
                    throw new InvalidOperationException(
                        $"Target scene postflight failed for {scenePath}.");
                }

                Debug.Log(
                    $"Target scene migrated {scenePath}: overrides={result.RemovedOverrides} "
                    + $"(presenter={result.RemovedPresenterBindings}), "
                    + $"visualGOs={result.RemovedLegacyVisualObjects}, "
                    + $"outlines={result.RemovedLegacyOutlines}, "
                    + $"sha256={hashAfter}.");
            }
        }

        private static void ValidateMigratedSceneState(
            GameObject instanceRoot,
            string scenePath,
            string expectedFunctionalSignature)
        {
            PropertyModification[] modifications =
                PrefabUtility.GetPropertyModifications(instanceRoot)
                ?? Array.Empty<PropertyModification>();
            if (modifications.Any(IsTargetManagedSceneOverride)
                || FindLegacyAddedVisualObjects(instanceRoot).Length > 0
                || FindLegacyAddedOutlines(instanceRoot).Length > 0)
            {
                throw new InvalidOperationException(
                    $"Legacy Target-managed scene presentation remains in {scenePath}.");
            }

            ValidateFunctionalSceneContracts(instanceRoot, scenePath);
            ValidateNoTargetChildDuplicates(instanceRoot, scenePath);
            string actualFunctionalSignature = CaptureFunctionalSceneSignature(instanceRoot);
            if (!string.Equals(
                    actualFunctionalSignature,
                    expectedFunctionalSignature,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Scene-owned HUD input identity or wiring changed in {scenePath}.");
            }
        }

        private static void AssertExactGlobalIdSet<T>(
            IEnumerable<T> objects,
            IReadOnlyCollection<string> expectedIds,
            string scenePath,
            string context)
            where T : UnityEngine.Object
        {
            HashSet<string> actual = objects
                .Select(GetGlobalObjectId)
                .ToHashSet(StringComparer.Ordinal);
            if (!actual.SetEquals(expectedIds))
            {
                throw new InvalidOperationException(
                    $"Preflight {context} identity changed in {scenePath}.");
            }
        }

        private static GameObject[] FindLegacyAddedVisualObjects(GameObject instanceRoot)
        {
            Transform[] transforms = instanceRoot.GetComponentsInChildren<Transform>(true);
            var result = new List<GameObject>();
            for (int i = 0; i < transforms.Length; i++)
            {
                GameObject candidate = transforms[i].gameObject;
                if (candidate == instanceRoot
                    || !PrefabUtility.IsAddedGameObjectOverride(candidate)
                    || (transforms[i].parent != null
                        && PrefabUtility.IsAddedGameObjectOverride(
                            transforms[i].parent.gameObject)))
                {
                    continue;
                }

                if (!IsStrictLegacyAddedVisual(candidate, instanceRoot))
                {
                    continue;
                }

                ValidatePresentationOnlyAddedObject(candidate);
                result.Add(candidate);
            }

            return result
                .OrderBy(candidate => GetGlobalObjectId(candidate), StringComparer.Ordinal)
                .ToArray();
        }

        private static bool IsStrictLegacyAddedVisual(
            GameObject candidate,
            GameObject instanceRoot)
        {
            Transform parent = candidate.transform.parent;
            if (parent == null)
            {
                return false;
            }

            GameObject sourceParent =
                PrefabUtility.GetCorrespondingObjectFromSource(parent.gameObject);
            if (sourceParent == null
                || !string.Equals(
                    AssetDatabase.GetAssetPath(sourceParent),
                    CanonicalPrefabPath,
                    StringComparison.Ordinal))
            {
                return false;
            }

            bool rootAmmo = string.Equals(candidate.name, "AmmoText", StringComparison.Ordinal)
                && sourceParent == PrefabUtility.GetCorrespondingObjectFromSource(instanceRoot);
            bool summonLayer = LegacySummonVisualNames.Contains(candidate.name)
                && LegacySummonParentNames.Contains(sourceParent.name);
            return rootAmmo || summonLayer;
        }

        private static void ValidatePresentationOnlyAddedObject(GameObject candidate)
        {
            Component[] components = candidate.GetComponentsInChildren<Component>(true);
            for (int i = 0; i < components.Length; i++)
            {
                Component component = components[i];
                bool allowed = component is Transform
                    || component is CanvasRenderer
                    || component is Graphic
                    || component is BaseMeshEffect
                    || component is Mask
                    || component is RectMask2D
                    || component is CanvasGroup;
                if (!allowed)
                {
                    throw new InvalidOperationException(
                        $"Refusing to delete legacy visual '{GetPath(candidate.transform)}' "
                        + $"because it owns functional component {component.GetType().FullName}.");
                }
            }
        }

        private static Outline[] FindLegacyAddedOutlines(GameObject instanceRoot)
        {
            return instanceRoot.GetComponentsInChildren<Outline>(true)
                .Where(outline => PrefabUtility.IsAddedComponentOverride(outline)
                    && IsManagedPrefabInstanceDescendant(outline.gameObject, instanceRoot))
                .OrderBy(outline => GetGlobalObjectId(outline), StringComparer.Ordinal)
                .ToArray();
        }

        private static bool IsManagedPrefabInstanceDescendant(
            GameObject instanceObject,
            GameObject instanceRoot)
        {
            GameObject sourceObject =
                PrefabUtility.GetCorrespondingObjectFromSource(instanceObject);
            GameObject sourceRoot =
                PrefabUtility.GetCorrespondingObjectFromSource(instanceRoot);
            if (sourceObject == null || sourceRoot == null)
            {
                return false;
            }

            string path = AnimationUtility.CalculateTransformPath(
                sourceObject.transform,
                sourceRoot.transform);
            return IsManagedSceneVisualPath(path);
        }

        private static bool IsTargetManagedSceneOverride(PropertyModification modification)
        {
            if (modification == null || modification.target == null)
            {
                return false;
            }
            if (!string.Equals(
                    AssetDatabase.GetAssetPath(modification.target),
                    CanonicalPrefabPath,
                    StringComparison.Ordinal))
            {
                return false;
            }
            if (IsPresenterVisualBindingOverride(modification))
            {
                return true;
            }

            GameObject targetGameObject = modification.target as GameObject;
            if (modification.target is Component component)
            {
                targetGameObject = component.gameObject;
            }
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CanonicalPrefabPath);
            if (targetGameObject == null || prefab == null)
            {
                return false;
            }

            string path = AnimationUtility.CalculateTransformPath(
                targetGameObject.transform,
                prefab.transform);
            if (string.IsNullOrEmpty(path) || !IsManagedSceneVisualPath(path))
            {
                // Root name, active state, placement and Canvas wiring remain scene-owned.
                return false;
            }

            string propertyPath = modification.propertyPath ?? string.Empty;
            if (modification.target is RectTransform)
            {
                return IsSceneLayoutProperty(propertyPath);
            }
            if (modification.target is Text)
            {
                return true;
            }
            if (modification.target is Graphic)
            {
                return IsSceneGraphicProperty(propertyPath);
            }
            if (modification.target is CanvasGroup)
            {
                return string.Equals(propertyPath, "m_Alpha", StringComparison.Ordinal)
                    || string.Equals(propertyPath, "m_Interactable", StringComparison.Ordinal)
                    || string.Equals(propertyPath, "m_BlocksRaycasts", StringComparison.Ordinal)
                    || string.Equals(propertyPath, "m_IgnoreParentGroups", StringComparison.Ordinal);
            }

            return modification.target is GameObject
                && string.Equals(propertyPath, "m_IsActive", StringComparison.Ordinal);
        }

        private static bool IsPresenterVisualBindingOverride(PropertyModification modification)
        {
            if (!(modification?.target is CombatHudPresenter))
            {
                return false;
            }
            if (!string.Equals(
                    AssetDatabase.GetAssetPath(modification.target),
                    CanonicalPrefabPath,
                    StringComparison.Ordinal))
            {
                return false;
            }

            string path = modification.propertyPath ?? string.Empty;
            if (PresenterDirectVisualBindings.Contains(path)
                || string.Equals(path, "aimReticleSegments.Array.size", StringComparison.Ordinal)
                || path.StartsWith("aimReticleSegments.Array.data[", StringComparison.Ordinal))
            {
                return true;
            }

            if (path.StartsWith("actionSlots.Array.data[", StringComparison.Ordinal))
            {
                return HasSerializedFieldSuffix(
                    path,
                    "labelText", "cooldownText", "cooldownFill", "readyProgressFill",
                    "readyGlowImage", "canvasGroup");
            }
            if (path.StartsWith("summonSlots.Array.data[", StringComparison.Ordinal))
            {
                return HasSerializedFieldSuffix(
                    path,
                    "labelText", "stateText", "cooldownFill", "iconImage",
                    "unavailableIconImage", "readyGlowImage", "readyRingImage",
                    "readySparkImage", "canvasGroup");
            }

            return false;
        }

        private static bool HasSerializedFieldSuffix(string path, params string[] names)
        {
            int separator = path.LastIndexOf('.');
            string suffix = separator >= 0 ? path.Substring(separator + 1) : path;
            return names.Contains(suffix, StringComparer.Ordinal);
        }

        private static bool IsManagedSceneVisualPath(string path)
        {
            int separator = path.IndexOf('/');
            string first = separator >= 0 ? path.Substring(0, separator) : path;
            return ManagedSceneVisualRoots.Contains(first);
        }

        private static bool IsSceneLayoutProperty(string propertyPath)
        {
            return propertyPath.StartsWith("m_AnchorMin.", StringComparison.Ordinal)
                || propertyPath.StartsWith("m_AnchorMax.", StringComparison.Ordinal)
                || propertyPath.StartsWith("m_AnchoredPosition.", StringComparison.Ordinal)
                || propertyPath.StartsWith("m_SizeDelta.", StringComparison.Ordinal)
                || propertyPath.StartsWith("m_Pivot.", StringComparison.Ordinal)
                || propertyPath.StartsWith("m_LocalScale.", StringComparison.Ordinal)
                || propertyPath.StartsWith("m_LocalRotation.", StringComparison.Ordinal)
                || propertyPath.StartsWith("m_LocalPosition.", StringComparison.Ordinal)
                || propertyPath.StartsWith("m_LocalEulerAnglesHint.", StringComparison.Ordinal)
                || string.Equals(propertyPath, "m_RootOrder", StringComparison.Ordinal)
                || string.Equals(
                    propertyPath,
                    "m_ConstrainProportionsScale",
                    StringComparison.Ordinal);
        }

        private static bool IsSceneGraphicProperty(string propertyPath)
        {
            return string.Equals(propertyPath, "m_Enabled", StringComparison.Ordinal)
                || string.Equals(propertyPath, "m_Sprite", StringComparison.Ordinal)
                || string.Equals(propertyPath, "m_Material", StringComparison.Ordinal)
                || string.Equals(propertyPath, "m_Type", StringComparison.Ordinal)
                || string.Equals(propertyPath, "m_PreserveAspect", StringComparison.Ordinal)
                || string.Equals(propertyPath, "m_FillMethod", StringComparison.Ordinal)
                || string.Equals(propertyPath, "m_FillOrigin", StringComparison.Ordinal)
                || string.Equals(propertyPath, "m_FillClockwise", StringComparison.Ordinal)
                || string.Equals(propertyPath, "m_FillAmount", StringComparison.Ordinal)
                || string.Equals(propertyPath, "m_RaycastTarget", StringComparison.Ordinal)
                || propertyPath.StartsWith("m_RaycastPadding.", StringComparison.Ordinal)
                || propertyPath.StartsWith("m_Color.", StringComparison.Ordinal);
        }

        private static void ValidateFunctionalSceneContracts(
            GameObject instanceRoot,
            string scenePath)
        {
            int expectedActions = string.Equals(
                scenePath,
                CanonicalScenePaths[2],
                StringComparison.Ordinal)
                ? 2
                : 7;
            CombatHudPointerActionInput[] actionInputs =
                instanceRoot.GetComponentsInChildren<CombatHudPointerActionInput>(true)
                    .Where(PrefabUtility.IsAddedComponentOverride)
                    .ToArray();
            if (actionInputs.Length != expectedActions)
            {
                throw new InvalidOperationException(
                    $"Expected {expectedActions} scene-owned HUD action inputs in {scenePath}, "
                    + $"found {actionInputs.Length}.");
            }

            int[] expectedIds = expectedActions == 2
                ? new[] { 100, 110 }
                : new[] { 100, 110, 120, 130, 200, 210, 220 };
            int[] actualIds = actionInputs
                .Select(input => (int)input.ActionId)
                .OrderBy(value => value)
                .ToArray();
            if (!actualIds.SequenceEqual(expectedIds.OrderBy(value => value)))
            {
                throw new InvalidOperationException(
                    $"Scene-owned HUD action IDs changed in {scenePath}: "
                    + string.Join(", ", actualIds));
            }
            for (int i = 0; i < actionInputs.Length; i++)
            {
                SerializedProperty bridge = new SerializedObject(actionInputs[i])
                    .FindProperty("inputBridge");
                if (bridge == null || bridge.objectReferenceValue == null)
                {
                    throw new InvalidOperationException(
                        $"HUD action {actionInputs[i].ActionId} lost its input bridge in {scenePath}.");
                }
            }

            CombatHudVirtualJoystick[] joysticks =
                instanceRoot.GetComponentsInChildren<CombatHudVirtualJoystick>(true)
                    .Where(PrefabUtility.IsAddedComponentOverride)
                    .ToArray();
            if (joysticks.Length != 1)
            {
                throw new InvalidOperationException(
                    $"Expected one scene-owned HUD joystick in {scenePath}, found {joysticks.Length}.");
            }
            var joystickSerialized = new SerializedObject(joysticks[0]);
            if (joystickSerialized.FindProperty("knob")?.objectReferenceValue == null
                || joystickSerialized.FindProperty("movementController")?.objectReferenceValue == null)
            {
                throw new InvalidOperationException(
                    $"Scene-owned HUD joystick lost its knob or movement binding in {scenePath}.");
            }

            Canvas canvas = instanceRoot.GetComponentInParent<Canvas>(true);
            CanvasScaler scaler = instanceRoot.GetComponentInParent<CanvasScaler>(true);
            if (canvas == null
                || scaler == null
                || canvas.gameObject != scaler.gameObject
                || scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize
                || scaler.referenceResolution != new Vector2(2560f, 1440f)
                || scaler.screenMatchMode != CanvasScaler.ScreenMatchMode.MatchWidthOrHeight
                || !Mathf.Approximately(scaler.matchWidthOrHeight, 1f))
            {
                throw new InvalidOperationException(
                    $"Canonical HUD Canvas/CanvasScaler contract is invalid in {scenePath}.");
            }

            CombatHudAimDragInput[] aimDragInputs =
                instanceRoot.GetComponentsInChildren<CombatHudAimDragInput>(true);
            int expectedAimDrag = string.Equals(
                scenePath,
                CanonicalScenePaths[2],
                StringComparison.Ordinal)
                ? 0
                : 1;
            if (aimDragInputs.Length != expectedAimDrag)
            {
                throw new InvalidOperationException(
                    $"AimDragArea preservation contract changed in {scenePath}.");
            }
            for (int i = 0; i < aimDragInputs.Length; i++)
            {
                if (!PrefabUtility.IsAddedGameObjectOverride(aimDragInputs[i].gameObject))
                {
                    throw new InvalidOperationException(
                        $"AimDragArea is no longer the preserved scene-owned object in {scenePath}.");
                }
            }
        }

        private static string CaptureFunctionalSceneSignature(GameObject instanceRoot)
        {
            var lines = new List<string>();
            RectTransform rootRect = instanceRoot.GetComponent<RectTransform>();
            lines.Add(
                $"ROOT|{GetGlobalObjectId(instanceRoot)}|{GetGlobalObjectId(rootRect)}|"
                + $"PARENT={GetGlobalObjectId(instanceRoot.transform.parent)}|"
                + $"A={FormatVector(rootRect.anchorMin)},{FormatVector(rootRect.anchorMax)}|"
                + $"P={FormatVector(rootRect.anchoredPosition)}|S={FormatVector(rootRect.sizeDelta)}");

            Canvas canvas = instanceRoot.GetComponentInParent<Canvas>(true);
            CanvasScaler scaler = instanceRoot.GetComponentInParent<CanvasScaler>(true);
            lines.Add(
                $"CANVAS|{GetGlobalObjectId(canvas)}|GO={GetGlobalObjectId(canvas.gameObject)}|"
                + $"MODE={(int)canvas.renderMode}|CAM={GetGlobalObjectId(canvas.worldCamera)}|"
                + $"SCALER={GetGlobalObjectId(scaler)}|MODE={(int)scaler.uiScaleMode}|"
                + $"REF={FormatVector(scaler.referenceResolution)}|MATCH="
                + scaler.matchWidthOrHeight.ToString("R", System.Globalization.CultureInfo.InvariantCulture));

            Component[] components = instanceRoot.GetComponentsInChildren<Component>(true);
            for (int i = 0; i < components.Length; i++)
            {
                Component component = components[i];
                if (component == null || component is Outline)
                {
                    continue;
                }

                bool addedComponent = PrefabUtility.IsAddedComponentOverride(component);
                bool projectScriptOnAddedObject = component is MonoBehaviour behaviour
                    && PrefabUtility.IsAddedGameObjectOverride(component.gameObject)
                    && IsProjectMonoBehaviour(behaviour);
                if (!addedComponent && !projectScriptOnAddedObject)
                {
                    continue;
                }

                lines.Add(CaptureFunctionalComponentSignature(component));
            }

            lines.Sort(StringComparer.Ordinal);
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(string.Join("\n", lines));
                return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", string.Empty);
            }
        }

        private static bool IsProjectMonoBehaviour(MonoBehaviour behaviour)
        {
            MonoScript script = MonoScript.FromMonoBehaviour(behaviour);
            string path = script != null ? AssetDatabase.GetAssetPath(script) : string.Empty;
            return path.StartsWith("Assets/_Game/", StringComparison.Ordinal);
        }

        private static string CaptureFunctionalComponentSignature(Component component)
        {
            var builder = new System.Text.StringBuilder();
            builder.Append(component.GetType().FullName)
                .Append('|').Append(GetGlobalObjectId(component))
                .Append("|GO=").Append(GetGlobalObjectId(component.gameObject));
            var serialized = new SerializedObject(component);
            SerializedProperty property = serialized.GetIterator();
            bool enterChildren = true;
            while (property.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (string.Equals(property.propertyPath, "m_Script", StringComparison.Ordinal))
                {
                    continue;
                }

                switch (property.propertyType)
                {
                    case SerializedPropertyType.ObjectReference:
                        builder.Append('|').Append(property.propertyPath).Append('=')
                            .Append(GetGlobalObjectId(property.objectReferenceValue));
                        break;
                    case SerializedPropertyType.Integer:
                    case SerializedPropertyType.Enum:
                    case SerializedPropertyType.LayerMask:
                    case SerializedPropertyType.Character:
                        builder.Append('|').Append(property.propertyPath).Append('=')
                            .Append(property.longValue);
                        break;
                    case SerializedPropertyType.Boolean:
                        builder.Append('|').Append(property.propertyPath).Append('=')
                            .Append(property.boolValue ? '1' : '0');
                        break;
                    case SerializedPropertyType.Float:
                        builder.Append('|').Append(property.propertyPath).Append('=')
                            .Append(property.doubleValue.ToString(
                                "R",
                                System.Globalization.CultureInfo.InvariantCulture));
                        break;
                    case SerializedPropertyType.String:
                        builder.Append('|').Append(property.propertyPath).Append('=')
                            .Append(property.stringValue);
                        break;
                }
            }

            return builder.ToString();
        }

        private static string GetGlobalObjectId(UnityEngine.Object target)
        {
            return target == null
                ? "null"
                : GlobalObjectId.GetGlobalObjectIdSlow(target).ToString();
        }

        private static string FormatVector(Vector2 value)
        {
            return value.x.ToString("R", System.Globalization.CultureInfo.InvariantCulture)
                + ","
                + value.y.ToString("R", System.Globalization.CultureInfo.InvariantCulture);
        }

        private static GameObject RequireCanonicalPrefabInstance(Scene scene)
        {
            var matches = new HashSet<GameObject>();
            GameObject[] roots = scene.GetRootGameObjects();
            for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
            {
                Transform[] transforms =
                    roots[rootIndex].GetComponentsInChildren<Transform>(true);
                for (int i = 0; i < transforms.Length; i++)
                {
                    GameObject candidate = transforms[i].gameObject;
                    if (!PrefabUtility.IsAnyPrefabInstanceRoot(candidate))
                    {
                        continue;
                    }

                    GameObject source =
                        PrefabUtility.GetCorrespondingObjectFromSource(candidate);
                    if (source != null
                        && string.Equals(
                            AssetDatabase.GetAssetPath(source),
                            CanonicalPrefabPath,
                            StringComparison.Ordinal))
                    {
                        matches.Add(candidate);
                    }
                }
            }

            if (matches.Count != 1)
            {
                throw new InvalidOperationException(
                    $"Expected one canonical combat HUD instance in {scene.path}, "
                    + $"found {matches.Count}.");
            }

            return matches.First();
        }

        private static void ValidateNoTargetChildDuplicates(
            GameObject instanceRoot,
            string scenePath)
        {
            string[] actionRoots =
            {
                "UltimateButton", "Skill1Button", "DodgeButton", "BasicAttackButton"
            };
            string[] actionChildren =
            {
                "Plate", "Glyph", "Cooldown", "ReadyArc", "TouchTarget"
            };
            for (int i = 0; i < actionRoots.Length; i++)
            {
                Transform action = RequireUniqueTransform(instanceRoot.transform, actionRoots[i]);
                AssertUniqueDescendantNames(action, actionChildren, scenePath);
            }

            string[] summonChildren =
            {
                "PortraitMask", "Icon", "IconDisabled", "Frame", "StateArc",
                "CostTab", "CostText", "StatusText", "TouchTarget"
            };
            for (int i = 1; i <= 3; i++)
            {
                Transform summon = RequireUniqueTransform(
                    instanceRoot.transform,
                    $"SummonSlot{i}Button");
                AssertUniqueDescendantNames(summon, summonChildren, scenePath);
            }

            RequireUniqueTransform(instanceRoot.transform, "AmmoText");
        }

        private static void AssertUniqueDescendantNames(
            Transform root,
            IEnumerable<string> names,
            string scenePath)
        {
            Transform[] descendants = root.GetComponentsInChildren<Transform>(true);
            foreach (string name in names)
            {
                int count = descendants.Count(candidate => string.Equals(
                    candidate.name,
                    name,
                    StringComparison.Ordinal));
                if (count != 1)
                {
                    throw new InvalidOperationException(
                        $"Target child '{name}' occurs {count} times below "
                        + $"{GetPath(root)} in {scenePath}.");
                }
            }
        }

        private static AssemblySpec LoadAndValidateSpec()
        {
            TextAsset json = AssetDatabase.LoadAssetAtPath<TextAsset>(AssemblySpecPath);
            if (json == null)
            {
                throw new InvalidOperationException(
                    $"Missing target HUD assembly spec: {AssemblySpecPath}");
            }

            AssemblySpec spec = JsonUtility.FromJson<AssemblySpec>(json.text);
            if (spec == null
                || spec.version != CombatHudCelestialTargetLayoutProfile.LayoutVersion
                || spec.sprites == null)
            {
                throw new InvalidOperationException(
                    $"Invalid or wrong-version target HUD spec: {AssemblySpecPath}");
            }

            string normalizedRoot = NormalizeAssetPath(spec.artRoot).TrimEnd('/');
            if (!string.Equals(normalizedRoot, TargetArtRoot, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Target HUD artRoot must be exactly '{TargetArtRoot}', got '{spec.artRoot}'.");
            }

            var roles = new HashSet<string>(StringComparer.Ordinal);
            var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < spec.sprites.Length; i++)
            {
                SpriteSpec entry = spec.sprites[i];
                if (entry == null
                    || string.IsNullOrWhiteSpace(entry.role)
                    || string.IsNullOrWhiteSpace(entry.path)
                    || !roles.Add(entry.role)
                    || !paths.Add(NormalizeAssetPath(entry.path)))
                {
                    throw new InvalidOperationException(
                        $"Target HUD spec has an invalid or duplicate entry at index {i}.");
                }
            }

            return spec;
        }

        private static void ValidateReferencedFiles(AssemblySpec spec)
        {
            string artRoot = NormalizeAssetPath(spec.artRoot).TrimEnd('/');
            for (int i = 0; i < spec.sprites.Length; i++)
            {
                SpriteSpec entry = spec.sprites[i];
                string assetPath = $"{artRoot}/{NormalizeAssetPath(entry.path).TrimStart('/')}";
                if (entry.required && !File.Exists(ToAbsoluteProjectPath(assetPath)))
                {
                    throw new InvalidOperationException(
                        $"Missing required target HUD sprite '{entry.role}': {assetPath}");
                }
            }
        }

        private static void ConfigureReferencedSpriteImporters(AssemblySpec spec)
        {
            string artRoot = NormalizeAssetPath(spec.artRoot).TrimEnd('/');
            for (int i = 0; i < spec.sprites.Length; i++)
            {
                SpriteSpec entry = spec.sprites[i];
                string assetPath = $"{artRoot}/{NormalizeAssetPath(entry.path).TrimStart('/')}";
                if (!File.Exists(ToAbsoluteProjectPath(assetPath)))
                {
                    continue;
                }

                TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (importer == null)
                {
                    throw new InvalidOperationException(
                        $"Target HUD asset is not a texture: {assetPath}");
                }

                bool changed = importer.textureType != TextureImporterType.Sprite
                    || importer.spriteImportMode != SpriteImportMode.Single
                    || importer.alphaIsTransparency == false
                    || importer.mipmapEnabled
                    || importer.wrapMode != TextureWrapMode.Clamp
                    || importer.filterMode != FilterMode.Bilinear
                    || importer.textureCompression != TextureImporterCompression.Uncompressed
                    || importer.npotScale != TextureImporterNPOTScale.None;
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.filterMode = FilterMode.Bilinear;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.npotScale = TextureImporterNPOTScale.None;
                if (changed)
                {
                    importer.SaveAndReimport();
                }
            }
        }

        private static SpriteCatalog LoadSprites(AssemblySpec spec)
        {
            string artRoot = NormalizeAssetPath(spec.artRoot).TrimEnd('/');
            var result = new Dictionary<string, Sprite>(StringComparer.Ordinal);
            for (int i = 0; i < spec.sprites.Length; i++)
            {
                SpriteSpec entry = spec.sprites[i];
                string assetPath = $"{artRoot}/{NormalizeAssetPath(entry.path).TrimStart('/')}";
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                if (entry.required && sprite == null)
                {
                    throw new InvalidOperationException(
                        $"Required target HUD sprite did not import as Sprite: {assetPath}");
                }

                if (sprite != null)
                {
                    result.Add(entry.role, sprite);
                }
            }

            return new SpriteCatalog(result);
        }

        private static void ConfigureLayoutMarker(
            GameObject prefabRoot,
            bool preserveLegacyMarker)
        {
            CombatHudCelestialV2LayoutProfile v22 =
                prefabRoot.GetComponent<CombatHudCelestialV2LayoutProfile>();
            if (v22 != null && preserveLegacyMarker)
            {
                // Scene prefab overrides may still reference this component fileID. The
                // Target marker owns runtime selection; retaining the disabled legacy marker
                // preserves every pre-promotion canonical local ID.
                v22.enabled = false;
            }
            else if (v22 != null)
            {
                UnityEngine.Object.DestroyImmediate(v22);
            }

            CombatHudCelestialTargetLayoutProfile[] targets =
                prefabRoot.GetComponents<CombatHudCelestialTargetLayoutProfile>();
            if (targets.Length > 1)
            {
                throw new InvalidOperationException("Target staging HUD has duplicate layout markers.");
            }

            if (targets.Length == 0)
            {
                prefabRoot.AddComponent<CombatHudCelestialTargetLayoutProfile>();
            }
        }

        private static HashSet<long> CollectPrefabLocalIds(GameObject prefab)
        {
            var ids = new HashSet<long>();
            Transform[] transforms = prefab.GetComponentsInChildren<Transform>(includeInactive: true);
            for (int i = 0; i < transforms.Length; i++)
            {
                AddLocalId(transforms[i].gameObject, ids);
                Component[] components = transforms[i].GetComponents<Component>();
                for (int componentIndex = 0; componentIndex < components.Length; componentIndex++)
                {
                    if (components[componentIndex] != null)
                    {
                        AddLocalId(components[componentIndex], ids);
                    }
                }
            }

            return ids;
        }

        private static void AddLocalId(UnityEngine.Object assetObject, ISet<long> ids)
        {
            if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(
                    assetObject,
                    out string _,
                    out long localId)
                || localId == 0)
            {
                throw new InvalidOperationException(
                    $"Could not resolve canonical local ID for {assetObject.name} ({assetObject.GetType().Name}).");
            }

            ids.Add(localId);
        }

        private static void ValidateCanonicalIdentityAfterPromotion(
            string expectedGuid,
            IReadOnlyCollection<long> localIdsBefore)
        {
            string actualGuid = AssetDatabase.AssetPathToGUID(CanonicalPrefabPath);
            if (!string.Equals(expectedGuid, actualGuid, StringComparison.Ordinal)
                || string.IsNullOrEmpty(actualGuid))
            {
                throw new InvalidOperationException(
                    $"Canonical HUD GUID changed during Target promotion: {expectedGuid} -> {actualGuid}.");
            }

            GameObject canonical = RequireAsset<GameObject>(CanonicalPrefabPath);
            HashSet<long> localIdsAfter = CollectPrefabLocalIds(canonical);
            long[] missing = localIdsBefore
                .Where(localId => !localIdsAfter.Contains(localId))
                .OrderBy(localId => localId)
                .ToArray();
            if (missing.Length > 0)
            {
                throw new InvalidOperationException(
                    $"Target promotion removed {missing.Length} canonical local IDs: "
                    + string.Join(", ", missing.Take(24)));
            }
        }

        private static Dictionary<string, string> CaptureAssetHashes(
            IEnumerable<string> assetPaths)
        {
            return assetPaths.ToDictionary(
                assetPath => assetPath,
                ComputeFileSha256,
                StringComparer.Ordinal);
        }

        private static void ValidateProtectedAssetHashes(
            IReadOnlyDictionary<string, string> expectedHashes)
        {
            Dictionary<string, string> actualHashes = CaptureAssetHashes(expectedHashes.Keys);
            string[] changed = expectedHashes
                .Where(pair => !string.Equals(
                    pair.Value,
                    actualHashes[pair.Key],
                    StringComparison.Ordinal))
                .Select(pair => pair.Key)
                .ToArray();
            if (changed.Length > 0)
            {
                throw new InvalidOperationException(
                    "Target promotion modified protected meta/scene assets: "
                    + string.Join(", ", changed));
            }
        }

        private static string FormatHashes(IReadOnlyDictionary<string, string> hashes)
        {
            return string.Join(
                ", ",
                hashes.Select(pair => $"{pair.Key}={pair.Value}"));
        }

        private static string ComputeFileSha256(string assetPath)
        {
            string absolutePath = ToAbsoluteProjectPath(assetPath);
            if (!File.Exists(absolutePath))
            {
                throw new InvalidOperationException($"Missing protected asset: {assetPath}");
            }

            using (FileStream stream = File.OpenRead(absolutePath))
            using (SHA256 sha = SHA256.Create())
            {
                return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty);
            }
        }

        private static void ConfigureObjective(Transform root, SpriteCatalog sprites, Font font)
        {
            RectTransform panel = RequireRect(root, "TopLeftPanel");
            SetDesignRect(
                panel,
                CombatHudCelestialTargetLayoutProfile.ObjectiveFrame,
                DesignAnchor.LeftTop);
            ClearImage(RequireImage(panel), false);

            ConfigureAtomicFullRect(
                panel,
                "ObjectiveBody",
                sprites.Require("objective.body"));
            ConfigureAtomicFullRect(
                panel,
                "ObjectiveTopFacets",
                sprites.Require("objective.topFacets"));
            ConfigureAtomicFullRect(
                panel,
                "ObjectiveBottomFacets",
                sprites.Require("objective.bottomFacets"));

            RectTransform objective = RequireRect(root, "Objective");
            SetDesignRect(
                objective,
                CombatHudCelestialTargetLayoutProfile.ObjectiveText,
                DesignAnchor.LeftTop);
            Text objectiveText = RequireText(objective);
            ConfigureText(
                objectiveText,
                font,
                "Break the pressure line",
                48,
                TextAnchor.MiddleLeft,
                new Color(0.96f, 0.97f, 0.98f, 1f));
            objectiveText.horizontalOverflow = HorizontalWrapMode.Wrap;
            objectiveText.verticalOverflow = VerticalWrapMode.Truncate;

            Image timerBacking = EnsureRootImage(root, "MissionTimerBacking");
            SetDesignRect(
                timerBacking.rectTransform,
                CombatHudCelestialTargetLayoutProfile.MissionTimerBacking,
                DesignAnchor.RightTop);
            ClearImage(timerBacking, false);
            RectTransform timer = RequireRect(root, "Timer");
            SetDesignRect(
                timer,
                CombatHudCelestialTargetLayoutProfile.MissionTimerText,
                DesignAnchor.RightTop);
            ConfigureText(
                RequireText(timer),
                font,
                string.Empty,
                42,
                TextAnchor.MiddleCenter,
                Color.white);
            timerBacking.gameObject.SetActive(false);
            timer.gameObject.SetActive(false);

            Transform settings = FindShallowestTransform(root, "SettingsButton");
            if (settings != null)
            {
                settings.gameObject.SetActive(false);
            }
        }

        private static void ConfigureBoss(Transform root, SpriteCatalog sprites, Font font)
        {
            RectTransform bossRoot = RequireRect(root, "BossHudRoot");
            Image chassis = EnsureDirectImage(bossRoot, "BossTargetChassis");
            SetDesignRect(
                chassis.rectTransform,
                CombatHudCelestialTargetLayoutProfile.BossChassis,
                DesignAnchor.CenterTop);
            ConfigureStaticImage(chassis, sprites.Require("boss.chassis"));

            RectTransform nameTab = RequireRect(root, "BossNameArea");
            SetDesignRect(
                nameTab,
                CombatHudCelestialTargetLayoutProfile.BossName,
                DesignAnchor.CenterTop);
            ConfigureStaticImage(RequireImage(nameTab), sprites.Require("boss.nameTab"));

            RectTransform hpTrack = RequireRect(root, "BossHpBackground");
            SetDesignRect(
                hpTrack,
                CombatHudCelestialTargetLayoutProfile.BossHpTrack,
                DesignAnchor.CenterTop);
            ConfigureStaticImage(RequireImage(hpTrack), sprites.Require("boss.hpTrack"));
            RectTransform hpFill = RequireRect(root, "BossHpFill");
            SetDesignRect(
                hpFill,
                CombatHudCelestialTargetLayoutProfile.BossHpFill,
                DesignAnchor.CenterTop);
            ConfigureHorizontalFill(RequireImage(hpFill), sprites.Require("boss.hpFill"));

            RectTransform costTrack = RequireRect(root, "BossCostBackground");
            SetDesignRect(
                costTrack,
                CombatHudCelestialTargetLayoutProfile.BossCostTrack,
                DesignAnchor.CenterTop);
            ConfigureStaticImage(RequireImage(costTrack), sprites.Require("boss.costTrack"));
            RectTransform costFill = RequireRect(root, "BossCostFill");
            SetDesignRect(
                costFill,
                CombatHudCelestialTargetLayoutProfile.BossCostFill,
                DesignAnchor.CenterTop);
            ConfigureHorizontalFill(RequireImage(costFill), sprites.Require("boss.costFill"));

            Text nameText = EnsureDirectText(bossRoot, "BossNameText", font);
            SetDesignRect(
                nameText.rectTransform,
                CombatHudCelestialTargetLayoutProfile.BossName,
                DesignAnchor.CenterTop);
            ConfigureText(
                nameText,
                font,
                "BOSS",
                30,
                TextAnchor.MiddleLeft,
                new Color(0.97f, 0.98f, 0.99f, 1f));

            Text hpText = EnsureDirectText(bossRoot, "BossHpText", font);
            SetDesignRect(
                hpText.rectTransform,
                CombatHudCelestialTargetLayoutProfile.BossHpValue,
                DesignAnchor.CenterTop);
            ConfigureText(hpText, font, "2400 / 2400", 24, TextAnchor.MiddleRight, Color.white);

            Text costText = EnsureDirectText(bossRoot, "BossCostText", font);
            SetDesignRect(
                costText.rectTransform,
                CombatHudCelestialTargetLayoutProfile.BossCostValue,
                DesignAnchor.CenterTop);
            ConfigureText(
                costText,
                font,
                "64 / 100",
                22,
                TextAnchor.MiddleRight,
                new Color(0.74f, 0.93f, 0.98f, 1f));

            chassis.transform.SetAsFirstSibling();
            hpTrack.SetSiblingIndex(1);
            hpFill.SetSiblingIndex(2);
            costTrack.SetSiblingIndex(3);
            costFill.SetSiblingIndex(4);
            nameTab.SetSiblingIndex(5);
            nameText.transform.SetAsLastSibling();
            hpText.transform.SetAsLastSibling();
            costText.transform.SetAsLastSibling();

            RequireUniqueTransform(root, "BossSymbol").gameObject.SetActive(false);
            RequireUniqueTransform(root, "ActionFeedback").gameObject.SetActive(false);
        }

        private static void ConfigurePause(Transform root, SpriteCatalog sprites)
        {
            RectTransform pause = RequireRect(root, "PauseButton");
            SetDesignRect(
                pause,
                CombatHudCelestialTargetLayoutProfile.PauseHit,
                DesignAnchor.RightTop);
            ConfigureInvisibleVisualRoot(RequireImage(pause));
            ConfigureTouchTarget(pause, Vector4.zero);

            Image plate = EnsureDirectImage(pause, "Plate");
            ConfigureStaticImage(plate, sprites.Require("pause.plate"));
            SetCenteredChildRect(
                plate.rectTransform,
                CombatHudCelestialTargetLayoutProfile.PauseVisual.size);
            Image glyph = EnsureDirectImage(pause, "Glyph");
            ConfigureStaticImage(glyph, sprites.Require("pause.glyph"));
            SetCenteredChildRect(
                glyph.rectTransform,
                CombatHudCelestialTargetLayoutProfile.PauseVisual.size);
            HideDirectChild(pause, "Label");
        }

        private static void ConfigureActions(Transform root, SpriteCatalog sprites, Font font)
        {
            ConfigureAction(
                RequireRect(root, "UltimateButton"),
                CombatHudCelestialTargetLayoutProfile.WeaponSwap,
                sprites.Require("action.weaponSwapGlyph"),
                sprites,
                font);
            ConfigureAction(
                RequireRect(root, "Skill1Button"),
                CombatHudCelestialTargetLayoutProfile.Ultimate,
                sprites.Require("action.ultimateGlyph"),
                sprites,
                font);
            ConfigureAction(
                RequireRect(root, "DodgeButton"),
                CombatHudCelestialTargetLayoutProfile.Dash,
                sprites.Require("action.dashGlyph"),
                sprites,
                font);
            ConfigureAction(
                RequireRect(root, "BasicAttackButton"),
                CombatHudCelestialTargetLayoutProfile.BasicAttack,
                sprites.Require("action.rangedGlyph"),
                sprites,
                font);
        }

        private static void ConfigureActionLayoutsOnly(Transform root)
        {
            ConfigureActionLayout(
                RequireRect(root, "UltimateButton"),
                CombatHudCelestialTargetLayoutProfile.WeaponSwap);
            ConfigureActionLayout(
                RequireRect(root, "Skill1Button"),
                CombatHudCelestialTargetLayoutProfile.Ultimate);
            ConfigureActionLayout(
                RequireRect(root, "DodgeButton"),
                CombatHudCelestialTargetLayoutProfile.Dash);
            ConfigureActionLayout(
                RequireRect(root, "BasicAttackButton"),
                CombatHudCelestialTargetLayoutProfile.BasicAttack);
        }

        private static void ConfigureActionLayout(RectTransform button, Rect designRect)
        {
            SetDesignRect(button, designRect, DesignAnchor.RightBottom);
            ConfigureTouchTarget(button, GetActionTouchInsets(button.name));
        }

        private static void ConfigureAction(
            RectTransform button,
            Rect designRect,
            Sprite glyphSprite,
            SpriteCatalog sprites,
            Font font)
        {
            ConfigureActionLayout(button, designRect);
            ConfigureInvisibleVisualRoot(RequireImage(button));

            Image plate = EnsureDirectImage(button, "Plate");
            ConfigureStaticImage(plate, sprites.Require("action.plate"), true);
            StretchToParent(plate.rectTransform);

            Image glyph = EnsureDirectImage(button, "Glyph");
            // Target glyphs retain the same 512x512 pivot canvas as the shared plate;
            // their authored transparent padding already defines the exact optical scale.
            ConfigureStaticImage(glyph, glyphSprite, true);
            StretchToParent(glyph.rectTransform);

            Image cooldown = TakeOrEnsureDirectImage(button, "Cooldown", "CooldownFill");
            ConfigureRadialFill(cooldown, sprites.Require("action.cooldownDisc"));
            StretchToParent(cooldown.rectTransform);

            Image readyArc = EnsureDirectImage(button, "ReadyArc");
            ConfigureRadialFill(readyArc, sprites.Require("action.readyArc"));
            StretchToParent(readyArc.rectTransform);

            Text cooldownText = RequireDescendantText(button, "CooldownText");
            ConfigureText(cooldownText, font, string.Empty, 30, TextAnchor.MiddleCenter, Color.white);
            StretchToParent(cooldownText.rectTransform);
            cooldownText.gameObject.SetActive(true);

            HideDirectChild(button, "Label");
            DisableNamedDescendant(button, "DodgeCooldownRing");
            DisableNamedDescendant(button, "ActionReadyGlow");
            DisableNamedDescendant(button, "ReadyGlow");
            plate.transform.SetSiblingIndex(0);
            glyph.transform.SetSiblingIndex(1);
            cooldown.transform.SetSiblingIndex(2);
            readyArc.transform.SetSiblingIndex(3);
            cooldownText.transform.SetAsLastSibling();
            button.Find("TouchTarget").SetAsLastSibling();
        }

        private static void ConfigureSummons(Transform root, SpriteCatalog sprites, Font font)
        {
            RectTransform rail = EnsureManagedGroup(
                root,
                "SummonRailTargetRoot",
                "SummonRailV22Root");
            ConfigureSummon(
                MoveIntoGroup(RequireRect(root, "SummonSlot1Button"), rail),
                CombatHudCelestialTargetLayoutProfile.SummonSlot1,
                1,
                sprites,
                font);
            ConfigureSummon(
                MoveIntoGroup(RequireRect(root, "SummonSlot2Button"), rail),
                CombatHudCelestialTargetLayoutProfile.SummonSlot2,
                2,
                sprites,
                font);
            ConfigureSummon(
                MoveIntoGroup(RequireRect(root, "SummonSlot3Button"), rail),
                CombatHudCelestialTargetLayoutProfile.SummonSlot3,
                3,
                sprites,
                font);
        }

        private static void ConfigureSummonLayoutsOnly(Transform root)
        {
            ConfigureSummonLayoutOnly(
                RequireRect(root, "SummonSlot1Button"),
                CombatHudCelestialTargetLayoutProfile.SummonSlot1,
                1);
            ConfigureSummonLayoutOnly(
                RequireRect(root, "SummonSlot2Button"),
                CombatHudCelestialTargetLayoutProfile.SummonSlot2,
                2);
            ConfigureSummonLayoutOnly(
                RequireRect(root, "SummonSlot3Button"),
                CombatHudCelestialTargetLayoutProfile.SummonSlot3,
                3);
        }

        private static void ConfigureSummonLayoutOnly(
            RectTransform slot,
            Rect designRect,
            int slotIndex)
        {
            SetDesignRect(slot, designRect, DesignAnchor.RightTop);
            float topTouchInset = slotIndex == 1 ? 8f : 6f;
            ConfigureTouchTarget(slot, new Vector4(6f, topTouchInset, 6f, 6f));

            // S1/S2 retain their approved widths; only their heights changed. Preserve the
            // imported horizontal child geometry byte-for-byte so a later SaveAssets cannot
            // oscillate 5.52f through RectTransform's parent-space round trip.
            Vector2 tabSourceSize = slotIndex == 1
                ? new Vector2(128f, 72f)
                : new Vector2(112f, 64f);
            Vector2 slotSourceSize = slotIndex == 1
                ? new Vector2(384f, 340f)
                : slotIndex == 2
                    ? new Vector2(360f, 260f)
                    : new Vector2(340f, 242f);
            float tabHeight = designRect.height * tabSourceSize.y / slotSourceSize.y;
            float tabY = designRect.height - tabHeight - designRect.height * 0.02f;
            SetLocalVerticalGeometry(RequireRect(slot, "CostTab"), tabY, tabHeight);
            SetLocalVerticalGeometry(RequireRect(slot, "CostText"), tabY, tabHeight);
            SetLocalVerticalGeometry(RequireRect(slot, "StatusText"), tabY, tabHeight);
        }

        private static void ConfigureSummonLayout(
            RectTransform slot,
            Rect designRect,
            int slotIndex)
        {
            SetDesignRect(slot, designRect, DesignAnchor.RightTop);
            float topTouchInset = slotIndex == 1 ? 8f : 6f;
            ConfigureTouchTarget(slot, new Vector4(6f, topTouchInset, 6f, 6f));

            float portraitCoverSize = Mathf.Max(designRect.width, designRect.height);
            Vector2 portraitSize = new Vector2(portraitCoverSize, portraitCoverSize);
            SetCenteredChildRect(RequireRect(slot, "Icon"), portraitSize);
            SetCenteredChildRect(RequireRect(slot, "IconDisabled"), portraitSize);

            Vector2 tabSourceSize = slotIndex == 1
                ? new Vector2(128f, 72f)
                : new Vector2(112f, 64f);
            Vector2 slotSourceSize = slotIndex == 1
                ? new Vector2(384f, 340f)
                : slotIndex == 2
                    ? new Vector2(360f, 260f)
                    : new Vector2(340f, 242f);
            float tabWidth = designRect.width * tabSourceSize.x / slotSourceSize.x;
            float tabHeight = designRect.height * tabSourceSize.y / slotSourceSize.y;
            float tabX = designRect.width * 0.02f;
            float tabY = designRect.height - tabHeight - designRect.height * 0.02f;
            Rect tabRect = new Rect(tabX, tabY, tabWidth, tabHeight);
            SetLocalTopLeftRect(RequireRect(slot, "CostTab"), tabRect);
            Rect textRect = new Rect(tabX, tabY, tabWidth * 0.78f, tabHeight);
            SetLocalTopLeftRect(RequireRect(slot, "CostText"), textRect);
            SetLocalTopLeftRect(RequireRect(slot, "StatusText"), textRect);
        }

        private static void ConfigureSummon(
            RectTransform slot,
            Rect designRect,
            int slotIndex,
            SpriteCatalog sprites,
            Font font)
        {
            // Child geometry is applied after all atomic layers exist.
            ConfigureInvisibleVisualRoot(RequireImage(slot));

            Image maskImage = EnsureDirectImage(slot, "PortraitMask");
            ConfigureStaticImage(maskImage, sprites.Require($"summon.mask{slotIndex}"));
            StretchToParent(maskImage.rectTransform);
            Mask mask = maskImage.GetComponent<Mask>();
            if (mask == null)
            {
                mask = maskImage.gameObject.AddComponent<Mask>();
            }
            mask.enabled = true;
            mask.showMaskGraphic = false;

            Image portrait = TakeOrEnsureChildImage(maskImage.rectTransform, "Icon", slot);
            ConfigureStaticImage(
                portrait,
                sprites.Require($"summon.portrait{slotIndex}"),
                true);
            portrait.maskable = true;

            Image disabledPortrait = TakeOrEnsureChildImage(
                maskImage.rectTransform,
                "IconDisabled",
                slot);
            ConfigureStaticImage(
                disabledPortrait,
                sprites.Require($"summon.portrait{slotIndex}"),
                true);
            disabledPortrait.maskable = true;
            disabledPortrait.color = new Color(0.34f, 0.37f, 0.41f, 0.96f);

            Image frame = TakeOrEnsureDirectImage(slot, "Frame", "FrameOverlay");
            ConfigureStaticImage(frame, sprites.Require($"summon.frame{slotIndex}"));
            StretchToParent(frame.rectTransform);

            Image stateArc = TakeOrEnsureDirectImage(slot, "StateArc", "CooldownFill");
            ConfigureRadialFill(stateArc, sprites.Require($"summon.accent{slotIndex}"));
            stateArc.preserveAspect = false;
            StretchToParent(stateArc.rectTransform);

            Image costTab = EnsureDirectImage(slot, "CostTab");
            ConfigureStaticImage(costTab, sprites.Require($"summon.costTab{slotIndex}"));

            Text costText = TakeOrEnsureDirectText(slot, "CostText", "Label", font);
            ConfigureText(
                costText,
                font,
                slotIndex == 1 ? "24" : slotIndex == 2 ? "18" : "12",
                slotIndex == 1 ? 42 : 36,
                TextAnchor.MiddleCenter,
                Color.white);

            Text statusText = TakeOrEnsureDirectText(slot, "StatusText", "State", font);
            ConfigureText(statusText, font, string.Empty, 32, TextAnchor.MiddleCenter, Color.white);
            ConfigureSummonLayout(slot, designRect, slotIndex);
            Transform costUnit = FindUniqueTransform(slot, "CostUnitText", false);
            if (costUnit != null)
            {
                costUnit.gameObject.SetActive(false);
            }

            DisableNamedDescendant(slot, "ReadyGlow");
            DisableNamedDescendant(slot, "ReadyRing");
            DisableNamedDescendant(slot, "ReadySparkRing");
            maskImage.transform.SetSiblingIndex(0);
            frame.transform.SetSiblingIndex(1);
            stateArc.transform.SetSiblingIndex(2);
            costTab.transform.SetSiblingIndex(3);
            costText.transform.SetAsLastSibling();
            statusText.transform.SetAsLastSibling();
            slot.Find("TouchTarget").SetAsLastSibling();
        }

        private static void ConfigureJoystickLayoutOnly(Transform root)
        {
            RectTransform ring = RequireRect(root, "MoveJoystickRing");
            SetDesignRect(
                ring,
                CombatHudCelestialTargetLayoutProfile.JoystickVisual,
                DesignAnchor.LeftBottom);

            RectTransform activationHit = RequireRect(root, "JoystickActivationHit");
            SetCenteredChildRect(
                activationHit,
                CombatHudCelestialTargetLayoutProfile.JoystickActivation.size);

            RectTransform knob = RequireRect(root, "MoveJoystickKnob");
            SetDesignRect(
                knob,
                CombatHudCelestialTargetLayoutProfile.JoystickKnob,
                DesignAnchor.LeftBottom);
        }

        private static void ConfigureJoystick(Transform root, SpriteCatalog sprites)
        {
            RectTransform ring = RequireRect(root, "MoveJoystickRing");
            ConfigureStaticImage(RequireImage(ring), sprites.Require("joystick.baseGlass"), true);

            Image ringTicks = EnsureDirectImage(ring, "RingTicks");
            ConfigureStaticImage(ringTicks, sprites.Require("joystick.ringTicks"), true);
            StretchToParent(ringTicks.rectTransform);
            if (sprites.TryGet("joystick.directionTicks", out Sprite directionTicksSprite))
            {
                Image directionTicks = EnsureDirectImage(ring, "DirectionTicks");
                ConfigureStaticImage(directionTicks, directionTicksSprite, true);
                StretchToParent(directionTicks.rectTransform);
            }

            Image activationHit = EnsureDirectImage(ring, "JoystickActivationHit");
            ClearImage(activationHit, true);
            activationHit.transform.SetAsLastSibling();

            RectTransform knob = RequireRect(root, "MoveJoystickKnob");
            ConfigureStaticImage(RequireImage(knob), sprites.Require("joystick.knob"), true);
            ConfigureJoystickLayoutOnly(root);
        }

        private static void ConfigurePlayer(Transform root, SpriteCatalog sprites, Font font)
        {
            RectTransform group = EnsureManagedGroup(
                root,
                "PlayerHudTargetRoot",
                "PlayerHudV22Root");
            RectTransform chassis = MoveIntoGroup(
                EnsureRootImage(root, "PlayerTargetChassis").rectTransform,
                group);
            RectTransform portraitFrame = MoveIntoGroup(
                EnsureRootImage(root, "PlayerPortraitFrame").rectTransform,
                group);
            RectTransform hpTrack = MoveIntoGroup(RequireRect(root, "HealthBar_Track"), group);
            RectTransform hpFill = MoveIntoGroup(RequireRect(root, "HealthBar"), group);
            RectTransform hpText = MoveIntoGroup(RequireRect(root, "HealthText"), group);
            RectTransform costTrack = MoveIntoGroup(RequireRect(root, "ResourceBar_Track"), group);
            RectTransform costFill = MoveIntoGroup(RequireRect(root, "ResourceBar"), group);
            RectTransform costText = MoveIntoGroup(RequireRect(root, "ResourceText"), group);
            RectTransform inputMode = MoveIntoGroup(RequireRect(root, "InputMode"), group);
            RectTransform ammoText = MoveIntoGroup(RequireRect(root, "AmmoText"), group);
            RectTransform ammoCell = MoveIntoGroup(
                EnsureRootImage(root, "PlayerAmmoChip").rectTransform,
                group);
            RectTransform modeCell = MoveIntoGroup(
                EnsureRootImage(root, "PlayerModeCell").rectTransform,
                group);

            SetDesignRect(
                chassis,
                CombatHudCelestialTargetLayoutProfile.PlayerComposite,
                DesignAnchor.CenterBottom);
            ConfigureStaticImage(RequireImage(chassis), sprites.Require("player.chassis"));

            SetDesignRect(
                portraitFrame,
                CombatHudCelestialTargetLayoutProfile.PlayerPortrait,
                DesignAnchor.CenterBottom);
            ClearImage(RequireImage(portraitFrame), false);
            Image portraitMask = EnsureDirectImage(portraitFrame, "PortraitMask");
            ConfigureStaticImage(portraitMask, sprites.Require("player.portraitMask"), true);
            SetCenteredChildRect(portraitMask.rectTransform, new Vector2(126f, 126f));
            Mask mask = portraitMask.GetComponent<Mask>();
            if (mask == null)
            {
                mask = portraitMask.gameObject.AddComponent<Mask>();
            }
            mask.enabled = true;
            mask.showMaskGraphic = false;
            Image portrait = TakeOrEnsureChildImage(
                portraitMask.rectTransform,
                "PlayerPortrait",
                portraitFrame);
            ConfigureStaticImage(portrait, sprites.Require("player.portrait"), true);
            portrait.maskable = true;
            SetCenteredChildRect(portrait.rectTransform, new Vector2(126f, 126f));
            Image portraitOverlay = EnsureDirectImage(portraitFrame, "FrameOverlay");
            ConfigureStaticImage(
                portraitOverlay,
                sprites.Require("player.portraitFrame"),
                true);
            StretchToParent(portraitOverlay.rectTransform);

            SetDesignRect(
                hpText,
                CombatHudCelestialTargetLayoutProfile.PlayerHpText,
                DesignAnchor.CenterBottom);
            ConfigureText(
                RequireText(hpText),
                font,
                "2400 / 2400",
                30,
                TextAnchor.MiddleLeft,
                new Color(0.98f, 0.97f, 0.94f, 1f));
            SetDesignRect(
                hpTrack,
                CombatHudCelestialTargetLayoutProfile.PlayerHpTrack,
                DesignAnchor.CenterBottom);
            ConfigureStaticImage(RequireImage(hpTrack), sprites.Require("player.hpTrack"));
            SetDesignRect(
                hpFill,
                CombatHudCelestialTargetLayoutProfile.PlayerHpFill,
                DesignAnchor.CenterBottom);
            ConfigureHorizontalFill(RequireImage(hpFill), sprites.Require("player.hpFill"));

            SetDesignRect(
                costTrack,
                CombatHudCelestialTargetLayoutProfile.PlayerCostTrack,
                DesignAnchor.CenterBottom);
            ConfigureStaticImage(RequireImage(costTrack), sprites.Require("player.costTrack"));
            SetDesignRect(
                costFill,
                CombatHudCelestialTargetLayoutProfile.PlayerCostFill,
                DesignAnchor.CenterBottom);
            ConfigureHorizontalFill(RequireImage(costFill), sprites.Require("player.costFill"));
            Image statePips = EnsureDirectImage(costFill, "StatePips");
            ConfigureStaticImage(statePips, sprites.Require("player.statePips"), true);
            SetLocalTopLeftRect(statePips.rectTransform, new Rect(400f, 2f, 96f, 24f));
            // The approved target carries state through the large cost rail; a second
            // numeric readout would compete with it, so the legacy resource text stays bound
            // but hidden and can be re-enabled later without changing presenter wiring.
            costText.gameObject.SetActive(false);

            SetDesignRect(
                modeCell,
                CombatHudCelestialTargetLayoutProfile.PlayerModeGlyph,
                DesignAnchor.CenterBottom);
            ClearImage(RequireImage(modeCell), false);
            Image modeGlyph = EnsureDirectImage(modeCell, "ModeGlyph");
            ConfigureStaticImage(modeGlyph, sprites.Require("player.modeGlyph"), true);
            StretchToParent(modeGlyph.rectTransform);
            inputMode.gameObject.SetActive(false);

            SetDesignRect(
                ammoCell,
                CombatHudCelestialTargetLayoutProfile.PlayerAmmo,
                DesignAnchor.CenterBottom);
            ConfigureStaticImage(RequireImage(ammoCell), sprites.Require("player.ammoPlate"));
            Image bulletGlyph = EnsureDirectImage(ammoCell, "BulletGlyph");
            ConfigureStaticImage(bulletGlyph, sprites.Require("player.bulletGlyph"), true);
            SetLocalTopLeftRect(bulletGlyph.rectTransform, new Rect(16f, 12f, 44f, 44f));
            Image separator = EnsureDirectImage(ammoCell, "AmmoSeparator");
            ConfigureStaticImage(separator, sprites.Require("player.ammoSeparator"), true);
            SetLocalTopLeftRect(separator.rectTransform, new Rect(64f, 10f, 12f, 48f));
            SetDesignRect(
                ammoText,
                CombatHudCelestialTargetLayoutProfile.PlayerAmmoText,
                DesignAnchor.CenterBottom);
            ConfigureText(
                RequireText(ammoText),
                font,
                "24 / 24",
                25,
                TextAnchor.MiddleRight,
                new Color(0.97f, 0.96f, 0.92f, 1f));

            chassis.SetSiblingIndex(0);
            hpTrack.SetSiblingIndex(1);
            hpFill.SetSiblingIndex(2);
            costTrack.SetSiblingIndex(3);
            costFill.SetSiblingIndex(4);
            portraitFrame.SetSiblingIndex(5);
            hpText.SetSiblingIndex(6);
            modeCell.SetSiblingIndex(7);
            ammoCell.SetSiblingIndex(8);
            ammoText.SetSiblingIndex(9);

            RequireUniqueTransform(root, "PlayerSymbol").gameObject.SetActive(false);
            RequireUniqueTransform(root, "PlayerNameArea").gameObject.SetActive(false);
            RequireUniqueTransform(root, "PlayerHpAmountArea").gameObject.SetActive(false);
            RequireUniqueTransform(root, "PlayerMpAmountArea").gameObject.SetActive(false);
        }

        private static void ConfigureReticle(Transform root, SpriteCatalog sprites)
        {
            Image rootImage = EnsureRootImage(root, "CenterAimReticle");
            RectTransform reticle = rootImage.rectTransform;
            SetDesignRect(
                reticle,
                CombatHudCelestialTargetLayoutProfile.Reticle,
                DesignAnchor.CenterScreen);
            ClearImage(rootImage, false);

            Image dot = EnsureDirectImage(reticle, "Dot");
            ConfigureStaticImage(dot, sprites.Require("reticle.dot"), true);
            StretchToParent(dot.rectTransform);
            ConfigureReticleNeedle(reticle, "NeedleTop", sprites.Require("reticle.needle"), 0f);
            ConfigureReticleNeedle(reticle, "NeedleRight", sprites.Require("reticle.needle"), -90f);
            ConfigureReticleNeedle(reticle, "NeedleBottom", sprites.Require("reticle.needle"), 180f);
            ConfigureReticleNeedle(reticle, "NeedleLeft", sprites.Require("reticle.needle"), 90f);
            dot.transform.SetAsLastSibling();
        }

        private static void ConfigureReticleNeedle(
            RectTransform parent,
            string name,
            Sprite sprite,
            float rotation)
        {
            Image needle = EnsureDirectImage(parent, name);
            ConfigureStaticImage(needle, sprite, true);
            StretchToParent(needle.rectTransform);
            needle.rectTransform.localRotation = Quaternion.Euler(0f, 0f, rotation);
        }

        private static void ConfigurePresenterBindings(GameObject prefabRoot)
        {
            CombatHudPresenter presenter = prefabRoot.GetComponent<CombatHudPresenter>();
            if (presenter == null)
            {
                throw new InvalidOperationException("HUD root is missing CombatHudPresenter.");
            }

            var serialized = new SerializedObject(presenter);
            SetObjectReference(
                serialized,
                "objectiveText",
                RequireUniqueComponent<Text>(prefabRoot.transform, "Objective"));
            SetObjectReference(
                serialized,
                "timerText",
                RequireUniqueComponent<Text>(prefabRoot.transform, "Timer"));
            SetObjectReference(
                serialized,
                "healthText",
                RequireUniqueComponent<Text>(prefabRoot.transform, "HealthText"));
            SetObjectReference(
                serialized,
                "resourceText",
                RequireUniqueComponent<Text>(prefabRoot.transform, "ResourceText"));
            SetObjectReference(
                serialized,
                "inputModeText",
                RequireUniqueComponent<Text>(prefabRoot.transform, "InputMode"));
            SetObjectReference(
                serialized,
                "ammoText",
                RequireUniqueComponent<Text>(prefabRoot.transform, "AmmoText"));
            SetObjectReference(
                serialized,
                "actionFeedbackText",
                RequireUniqueComponent<Text>(prefabRoot.transform, "ActionFeedback"));
            SetObjectReference(
                serialized,
                "healthFill",
                RequireUniqueComponent<Image>(prefabRoot.transform, "HealthBar"));
            SetObjectReference(
                serialized,
                "resourceFill",
                RequireUniqueComponent<Image>(prefabRoot.transform, "ResourceBar"));
            SetObjectReference(
                serialized,
                "bossHudRoot",
                RequireRect(prefabRoot.transform, "BossHudRoot"));
            SetObjectReference(
                serialized,
                "bossHealthText",
                RequireUniqueComponent<Text>(prefabRoot.transform, "BossHpText"));
            SetObjectReference(
                serialized,
                "bossResourceText",
                RequireUniqueComponent<Text>(prefabRoot.transform, "BossCostText"));
            SetObjectReference(
                serialized,
                "bossHealthFill",
                RequireUniqueComponent<Image>(prefabRoot.transform, "BossHpFill"));
            SetObjectReference(
                serialized,
                "bossResourceFill",
                RequireUniqueComponent<Image>(prefabRoot.transform, "BossCostFill"));
            SetObjectReference(
                serialized,
                "aimReticleRoot",
                RequireRect(prefabRoot.transform, "CenterAimReticle"));

            SerializedProperty segments = serialized.FindProperty("aimReticleSegments");
            string[] segmentNames =
            {
                "Dot", "NeedleTop", "NeedleRight", "NeedleBottom", "NeedleLeft"
            };
            segments.arraySize = segmentNames.Length;
            for (int i = 0; i < segmentNames.Length; i++)
            {
                segments.GetArrayElementAtIndex(i).objectReferenceValue =
                    RequireUniqueComponent<Image>(prefabRoot.transform, segmentNames[i]);
            }

            BindAction(serialized.FindProperty("actionSlots"), 100, prefabRoot.transform, "BasicAttackButton");
            BindAction(serialized.FindProperty("actionSlots"), 110, prefabRoot.transform, "DodgeButton");
            BindAction(serialized.FindProperty("actionSlots"), 120, prefabRoot.transform, "Skill1Button");
            BindAction(serialized.FindProperty("actionSlots"), 130, prefabRoot.transform, "UltimateButton");
            BindSummon(serialized.FindProperty("summonSlots"), 200, prefabRoot.transform, "SummonSlot1Button");
            BindSummon(serialized.FindProperty("summonSlots"), 210, prefabRoot.transform, "SummonSlot2Button");
            BindSummon(serialized.FindProperty("summonSlots"), 220, prefabRoot.transform, "SummonSlot3Button");
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(presenter);
        }

        private static void BindAction(
            SerializedProperty bindings,
            int actionId,
            Transform root,
            string buttonName)
        {
            SerializedProperty binding = RequireBinding(bindings, actionId);
            RectTransform button = RequireRect(root, buttonName);
            binding.FindPropertyRelative("cooldownFill").objectReferenceValue =
                RequireDescendantImage(button, "Cooldown");
            Image readyArc = RequireDescendantImage(button, "ReadyArc");
            // The approved plate shows the cyan readiness arc on the upper-left
            // weapon-swap control. Other actions retain cooldown discs without adding
            // another bright ring to the exact target composition.
            bool priority = actionId == 130;
            readyArc.gameObject.SetActive(priority);
            binding.FindPropertyRelative("readyProgressFill").objectReferenceValue =
                priority ? readyArc : null;
            binding.FindPropertyRelative("readyGlowImage").objectReferenceValue = null;
        }

        private static void BindSummon(
            SerializedProperty bindings,
            int actionId,
            Transform root,
            string buttonName)
        {
            SerializedProperty binding = RequireBinding(bindings, actionId);
            RectTransform button = RequireRect(root, buttonName);
            binding.FindPropertyRelative("labelText").objectReferenceValue =
                RequireDescendantText(button, "CostText");
            binding.FindPropertyRelative("stateText").objectReferenceValue =
                RequireDescendantText(button, "StatusText");
            binding.FindPropertyRelative("cooldownFill").objectReferenceValue =
                RequireDescendantImage(button, "StateArc");
            binding.FindPropertyRelative("iconImage").objectReferenceValue =
                RequireDescendantImage(button, "Icon");
            binding.FindPropertyRelative("unavailableIconImage").objectReferenceValue =
                RequireDescendantImage(button, "IconDisabled");
            binding.FindPropertyRelative("readyGlowImage").objectReferenceValue = null;
            binding.FindPropertyRelative("readyRingImage").objectReferenceValue = null;
            binding.FindPropertyRelative("readySparkImage").objectReferenceValue = null;
        }

        private static SerializedProperty RequireBinding(
            SerializedProperty bindings,
            int actionId)
        {
            if (bindings == null || !bindings.isArray)
            {
                throw new InvalidOperationException("CombatHudPresenter binding array is missing.");
            }

            for (int i = 0; i < bindings.arraySize; i++)
            {
                SerializedProperty candidate = bindings.GetArrayElementAtIndex(i);
                if (candidate.FindPropertyRelative("actionId").intValue == actionId)
                {
                    return candidate;
                }
            }

            throw new InvalidOperationException(
                $"Missing HUD presenter binding for action {actionId}.");
        }

        private static void SetObjectReference(
            SerializedObject serialized,
            string propertyName,
            UnityEngine.Object value)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property == null)
            {
                throw new InvalidOperationException(
                    $"CombatHudPresenter is missing serialized property '{propertyName}'.");
            }

            property.objectReferenceValue = value;
        }

        private static void ValidateBindingRoots(Transform root)
        {
            string[] names =
            {
                "TopLeftPanel", "Timer", "Objective", "ActionFeedback", "PauseButton",
                "BossHudRoot", "BossSymbol", "BossNameArea", "BossHpBackground",
                "BossHpFill", "BossCostBackground", "BossCostFill", "SummonSlot1Button",
                "SummonSlot2Button", "SummonSlot3Button", "UltimateButton", "Skill1Button",
                "DodgeButton", "BasicAttackButton", "MoveJoystickRing", "MoveJoystickKnob",
                "HealthBar_Track", "HealthBar", "HealthText", "ResourceBar_Track",
                "ResourceBar", "ResourceText", "AmmoText", "InputMode", "PlayerSymbol",
                "PlayerNameArea", "PlayerHpAmountArea", "PlayerMpAmountArea"
            };
            for (int i = 0; i < names.Length; i++)
            {
                RequireUniqueTransform(root, names[i]);
            }
        }

        private static void ValidateRaycastOwnership(Transform root)
        {
            string[] hitRoots =
            {
                "PauseButton", "UltimateButton", "Skill1Button", "DodgeButton",
                "BasicAttackButton", "SummonSlot1Button", "SummonSlot2Button",
                "SummonSlot3Button"
            };
            for (int i = 0; i < hitRoots.Length; i++)
            {
                RectTransform hitRoot = RequireRect(root, hitRoots[i]);
                Image rootImage = RequireImage(hitRoot);
                if (rootImage.raycastTarget)
                {
                    throw new InvalidOperationException(
                        $"{hitRoots[i]} visual root must not consume taps.");
                }

                Transform touchTransform = hitRoot.Find("TouchTarget");
                Image touchTarget = touchTransform != null
                    ? touchTransform.GetComponent<Image>()
                    : null;
                if (touchTarget == null || !touchTarget.raycastTarget)
                {
                    throw new InvalidOperationException(
                        $"{hitRoots[i]} must own one independent TouchTarget graphic.");
                }

                Image[] children = hitRoot.GetComponentsInChildren<Image>(includeInactive: false);
                for (int childIndex = 0; childIndex < children.Length; childIndex++)
                {
                    if (children[childIndex].transform != hitRoot
                        && children[childIndex] != touchTarget
                        && children[childIndex].raycastTarget)
                    {
                        throw new InvalidOperationException(
                            $"Decorative child {GetPath(children[childIndex].transform)} consumes taps.");
                    }
                }
            }

            Image joystickHit = RequireUniqueComponent<Image>(root, "JoystickActivationHit");
            if (!joystickHit.raycastTarget)
            {
                throw new InvalidOperationException(
                    "JoystickActivationHit must receive pointer acquisition.");
            }
        }

        private static RectTransform EnsureManagedGroup(
            Transform root,
            string targetName,
            string priorName)
        {
            Transform existing = FindUniqueTransform(root, targetName, false)
                ?? FindUniqueTransform(root, priorName, false);
            if (existing == null)
            {
                var gameObject = new GameObject(targetName, typeof(RectTransform));
                gameObject.layer = root.gameObject.layer;
                gameObject.transform.SetParent(root, false);
                existing = gameObject.transform;
            }

            existing.name = targetName;
            RectTransform rect = existing as RectTransform;
            if (rect == null)
            {
                throw new InvalidOperationException(
                    $"Managed target group '{targetName}' is not a RectTransform.");
            }

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
            return rect;
        }

        private static RectTransform MoveIntoGroup(RectTransform child, RectTransform group)
        {
            if (child.parent != group)
            {
                child.SetParent(group, false);
            }
            return child;
        }

        private static Image ConfigureAtomicFullRect(
            RectTransform parent,
            string name,
            Sprite sprite)
        {
            Image image = EnsureDirectImage(parent, name);
            ConfigureStaticImage(image, sprite);
            StretchToParent(image.rectTransform);
            return image;
        }

        private static Image TakeOrEnsureDirectImage(
            RectTransform parent,
            string targetName,
            string priorName)
        {
            Transform target = parent.Find(targetName);
            if (target == null)
            {
                target = FindUniqueTransform(parent, priorName, false);
                if (target != null)
                {
                    target.name = targetName;
                    target.SetParent(parent, false);
                }
            }
            return target != null ? RequireImage(target) : EnsureDirectImage(parent, targetName);
        }

        private static Text TakeOrEnsureDirectText(
            RectTransform parent,
            string targetName,
            string priorName,
            Font font)
        {
            Transform target = parent.Find(targetName);
            if (target == null)
            {
                target = FindUniqueTransform(parent, priorName, false);
                if (target != null)
                {
                    target.name = targetName;
                    target.SetParent(parent, false);
                }
            }
            return target != null ? RequireText(target) : EnsureDirectText(parent, targetName, font);
        }

        private static Image TakeOrEnsureChildImage(
            RectTransform targetParent,
            string name,
            RectTransform searchRoot)
        {
            Transform existing = targetParent.Find(name)
                ?? FindUniqueTransform(searchRoot, name, false);
            if (existing == null)
            {
                return EnsureDirectImage(targetParent, name);
            }
            if (existing.parent != targetParent)
            {
                existing.SetParent(targetParent, false);
            }
            return RequireImage(existing);
        }

        private static Image EnsureRootImage(Transform root, string name)
        {
            Transform existing = FindUniqueTransform(root, name, false);
            if (existing == null)
            {
                var gameObject = new GameObject(
                    name,
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image));
                gameObject.layer = root.gameObject.layer;
                gameObject.transform.SetParent(root, false);
                existing = gameObject.transform;
            }
            return RequireImage(existing);
        }

        private static Image EnsureDirectImage(RectTransform parent, string name)
        {
            Transform existing = parent.Find(name);
            if (existing == null)
            {
                var gameObject = new GameObject(
                    name,
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image));
                gameObject.layer = parent.gameObject.layer;
                gameObject.transform.SetParent(parent, false);
                existing = gameObject.transform;
            }
            return RequireImage(existing);
        }

        private static Text EnsureDirectText(RectTransform parent, string name, Font font)
        {
            Transform existing = parent.Find(name);
            if (existing == null)
            {
                var gameObject = new GameObject(
                    name,
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Text));
                gameObject.layer = parent.gameObject.layer;
                gameObject.transform.SetParent(parent, false);
                existing = gameObject.transform;
            }
            Text text = RequireText(existing);
            text.font = font;
            return text;
        }

        private static void ConfigureInvisibleVisualRoot(Image image)
        {
            ClearImage(image, false);
            image.alphaHitTestMinimumThreshold = 0f;
        }

        private static Image ConfigureTouchTarget(RectTransform parent, Vector4 insets)
        {
            Image touchTarget = EnsureDirectImage(parent, "TouchTarget");
            ClearImage(touchTarget, true);
            RectTransform rect = touchTarget.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = new Vector2(insets.x, insets.w);
            rect.offsetMax = new Vector2(-insets.z, -insets.y);
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
            return touchTarget;
        }

        private static Vector4 GetActionTouchInsets(string buttonName)
        {
            switch (buttonName)
            {
                case "UltimateButton":
                    return new Vector4(8f, 8f, 8f, 26f);
                case "Skill1Button":
                    return new Vector4(8f, 8f, 8f, 24f);
                case "DodgeButton":
                    return new Vector4(8f, 14f, 8f, 8f);
                case "BasicAttackButton":
                    return new Vector4(8f, 36f, 8f, 8f);
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(buttonName),
                        buttonName,
                        "Unknown target action button.");
            }
        }

        private static void ConfigureStaticImage(
            Image image,
            Sprite sprite,
            bool preserveAspect = false)
        {
            if (image == null || sprite == null)
            {
                throw new InvalidOperationException("Target HUD image or sprite is null.");
            }
            image.sprite = sprite;
            image.material = null;
            image.color = Color.white;
            image.raycastTarget = false;
            image.type = Image.Type.Simple;
            image.preserveAspect = preserveAspect;
            image.fillAmount = 1f;
        }

        private static void ConfigureHorizontalFill(Image image, Sprite sprite)
        {
            ConfigureStaticImage(image, sprite);
            image.type = Image.Type.Filled;
            image.fillMethod = Image.FillMethod.Horizontal;
            image.fillOrigin = (int)Image.OriginHorizontal.Left;
            image.fillClockwise = true;
            image.fillAmount = 1f;
        }

        private static void ConfigureRadialFill(Image image, Sprite sprite)
        {
            ConfigureStaticImage(image, sprite, true);
            image.type = Image.Type.Filled;
            image.fillMethod = Image.FillMethod.Radial360;
            image.fillOrigin = (int)Image.Origin360.Top;
            image.fillClockwise = true;
            image.fillAmount = 1f;
        }

        private static void ConfigureText(
            Text text,
            Font font,
            string value,
            int fontSize,
            TextAnchor alignment,
            Color color)
        {
            text.font = font;
            text.fontSize = fontSize;
            text.fontStyle = FontStyle.Normal;
            text.resizeTextForBestFit = false;
            text.alignment = alignment;
            text.alignByGeometry = true;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.color = color;
            text.raycastTarget = false;
            text.text = value;
        }

        private static void ClearImage(Image image, bool raycastTarget)
        {
            image.sprite = null;
            image.material = null;
            image.color = Color.clear;
            image.raycastTarget = raycastTarget;
            image.type = Image.Type.Simple;
            image.preserveAspect = false;
        }

        private static void DisableNamedDescendant(RectTransform parent, string name)
        {
            Transform found = FindUniqueTransform(parent, name, false);
            if (found != null)
            {
                found.gameObject.SetActive(false);
            }
        }

        private static void HideDirectChild(RectTransform parent, string name)
        {
            Transform child = parent.Find(name);
            if (child != null)
            {
                child.gameObject.SetActive(false);
            }
        }

        private static void SetDesignRect(
            RectTransform rect,
            Rect designRect,
            DesignAnchor anchor)
        {
            float rightInset = CombatHudCelestialTargetLayoutProfile.DesignWidth - designRect.xMax;
            float bottomInset = CombatHudCelestialTargetLayoutProfile.DesignHeight - designRect.yMax;
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
            switch (anchor)
            {
                case DesignAnchor.LeftTop:
                    rect.anchorMin = new Vector2(0f, 1f);
                    rect.anchorMax = new Vector2(0f, 1f);
                    rect.pivot = new Vector2(0f, 1f);
                    rect.anchoredPosition = new Vector2(designRect.xMin, -designRect.yMin);
                    break;
                case DesignAnchor.LeftBottom:
                    rect.anchorMin = Vector2.zero;
                    rect.anchorMax = Vector2.zero;
                    rect.pivot = Vector2.zero;
                    rect.anchoredPosition = new Vector2(designRect.xMin, bottomInset);
                    break;
                case DesignAnchor.RightTop:
                    rect.anchorMin = Vector2.one;
                    rect.anchorMax = Vector2.one;
                    rect.pivot = Vector2.one;
                    rect.anchoredPosition = new Vector2(-rightInset, -designRect.yMin);
                    break;
                case DesignAnchor.RightBottom:
                    rect.anchorMin = new Vector2(1f, 0f);
                    rect.anchorMax = new Vector2(1f, 0f);
                    rect.pivot = new Vector2(1f, 0f);
                    rect.anchoredPosition = new Vector2(-rightInset, bottomInset);
                    break;
                case DesignAnchor.CenterBottom:
                    rect.anchorMin = new Vector2(0.5f, 0f);
                    rect.anchorMax = new Vector2(0.5f, 0f);
                    rect.pivot = new Vector2(0.5f, 0f);
                    rect.anchoredPosition = new Vector2(
                        designRect.center.x
                            - CombatHudCelestialTargetLayoutProfile.DesignWidth * 0.5f,
                        bottomInset);
                    break;
                case DesignAnchor.CenterTop:
                case DesignAnchor.CenterScreen:
                    rect.anchorMin = new Vector2(0.5f, 0.5f);
                    rect.anchorMax = new Vector2(0.5f, 0.5f);
                    rect.pivot = new Vector2(0.5f, 0.5f);
                    rect.anchoredPosition = new Vector2(
                        designRect.center.x
                            - CombatHudCelestialTargetLayoutProfile.DesignWidth * 0.5f,
                        CombatHudCelestialTargetLayoutProfile.DesignHeight * 0.5f
                            - designRect.center.y);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(anchor), anchor, null);
            }
            rect.sizeDelta = designRect.size;
        }

        private static void SetCenteredChildRect(RectTransform rect, Vector2 size)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = size;
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
        }

        private static void SetLocalTopLeftRect(RectTransform rect, Rect localRect)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(localRect.x, -localRect.y);
            rect.sizeDelta = localRect.size;
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
        }

        private static void SetLocalVerticalGeometry(
            RectTransform rect,
            float localY,
            float height)
        {
            SetLocalTopLeftRect(
                rect,
                new Rect(
                    rect.anchoredPosition.x,
                    localY,
                    rect.sizeDelta.x,
                    height));
        }

        private static Rect GetLocalRect(RectTransform rect)
        {
            return new Rect(
                rect.anchoredPosition.x,
                -rect.anchoredPosition.y,
                rect.sizeDelta.x,
                rect.sizeDelta.y);
        }

        private static void StretchToParent(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
        }

        private static RectTransform RequireRect(Transform root, string name)
        {
            Transform found = RequireUniqueTransform(root, name);
            RectTransform rect = found as RectTransform;
            if (rect == null)
            {
                throw new InvalidOperationException(
                    $"Managed HUD object '{name}' is not a RectTransform.");
            }
            return rect;
        }

        private static Image RequireDescendantImage(RectTransform root, string name)
        {
            return RequireImage(RequireUniqueTransform(root, name));
        }

        private static Text RequireDescendantText(RectTransform root, string name)
        {
            return RequireText(RequireUniqueTransform(root, name));
        }

        private static T RequireUniqueComponent<T>(Transform root, string name)
            where T : Component
        {
            Transform found = RequireUniqueTransform(root, name);
            T component = found.GetComponent<T>();
            if (component == null)
            {
                throw new InvalidOperationException(
                    $"HUD object '{name}' is missing {typeof(T).Name}.");
            }
            return component;
        }

        private static Image RequireImage(Transform transform)
        {
            Image image = transform.GetComponent<Image>();
            if (image == null)
            {
                throw new InvalidOperationException(
                    $"HUD object '{GetPath(transform)}' is missing Image.");
            }
            return image;
        }

        private static Text RequireText(Transform transform)
        {
            Text text = transform.GetComponent<Text>();
            if (text == null)
            {
                throw new InvalidOperationException(
                    $"HUD object '{GetPath(transform)}' is missing Text.");
            }
            return text;
        }

        private static Transform RequireUniqueTransform(Transform root, string name)
        {
            return FindUniqueTransform(root, name, true);
        }

        private static Transform FindUniqueTransform(
            Transform root,
            string name,
            bool required)
        {
            Transform match = null;
            int count = 0;
            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                if (!string.Equals(transforms[i].name, name, StringComparison.Ordinal))
                {
                    continue;
                }
                match = transforms[i];
                count++;
            }

            if (count > 1 || (required && count != 1))
            {
                throw new InvalidOperationException(
                    $"Expected {(required ? "one" : "at most one")} '{name}' "
                    + $"under {root.name}, found {count}.");
            }
            return match;
        }

        private static Transform FindShallowestTransform(Transform root, string name)
        {
            return root.GetComponentsInChildren<Transform>(true)
                .Where(candidate => string.Equals(candidate.name, name, StringComparison.Ordinal))
                .OrderBy(candidate => GetDepthFrom(candidate, root))
                .FirstOrDefault();
        }

        private static int GetDepthFrom(Transform transform, Transform root)
        {
            int depth = 0;
            Transform current = transform;
            while (current != null && current != root)
            {
                depth++;
                current = current.parent;
            }
            return current == root ? depth : int.MaxValue;
        }

        private static T RequireAsset<T>(string path) where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                throw new InvalidOperationException(
                    $"Missing required {typeof(T).Name}: {path}");
            }
            return asset;
        }

        private static string NormalizeAssetPath(string path)
        {
            return path.Replace('\\', '/').Trim();
        }

        private static string ToAbsoluteProjectPath(string assetPath)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            if (string.IsNullOrEmpty(projectRoot))
            {
                throw new InvalidOperationException("Could not resolve Unity project root.");
            }
            return Path.Combine(projectRoot, NormalizeAssetPath(assetPath));
        }

        private static string GetPath(Transform transform)
        {
            var names = new Stack<string>();
            Transform current = transform;
            while (current != null)
            {
                names.Push(current.name);
                current = current.parent;
            }
            return string.Join("/", names);
        }
    }
}
