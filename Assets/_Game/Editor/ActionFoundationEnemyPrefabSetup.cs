using System;
using System.Collections.Generic;
using DimensionBrawl.AI;
using DimensionBrawl.Combat;
using DimensionBrawl.Enemies;
using DimensionBrawl.Presentation;
using UnityEditor;
using UnityEngine;

namespace DimensionBrawl.Editor
{
    public static class ActionFoundationEnemyPrefabSetup
    {
        public const string PrefabRoot = "Assets/_Game/Prefabs/Enemies/ActionFoundation";
        public const string MeleeSoldierRoleSourcePrefabPath =
            PrefabRoot + "/PF_Enemy_SciFiSoldier_Melee_ClosePunish.prefab";
        public const string MeleeSoldierPrefabPath =
            PrefabRoot + "/PF_Enemy_SciFiSoldier_Melee_HeavyWindup.prefab";
        public const string GeneralDeckSoldierPrefabPath = PrefabRoot + "/PF_Enemy_SciFiSoldier_GeneralDeck.prefab";
        public const string EliteDeckSoldierPrefabPath = PrefabRoot + "/PF_Enemy_SciFiSoldier_EliteDeck.prefab";

        private const string MeleeSoldierRoleSourcePrefabName = "PF_Enemy_SciFiSoldier_Melee_ClosePunish";
        private const string MeleeSoldierPrefabName = "PF_Enemy_SciFiSoldier_Melee_HeavyWindup";
        private const string GeneralDeckSoldierPrefabName = "PF_Enemy_SciFiSoldier_GeneralDeck";
        private const string EliteDeckSoldierPrefabName = "PF_Enemy_SciFiSoldier_EliteDeck";
        private const string VfxPoolChildName = "CombatVfxPool";
        private const string ProjectileOriginChildName = "ProjectileOrigin";

        [MenuItem("DimensionBrawl/Reapply Action Foundation Enemy Prefab Candidates")]
        public static void ReapplyEnemyPrefabCandidatesMenu()
        {
            EnsureEnemyPrefabCandidates();
            Debug.Log("Reapplied ActionFoundation enemy prefab candidates.");
        }

        [MenuItem("DimensionBrawl/Validate Action Foundation Enemy Prefab Candidates")]
        public static void ValidateEnemyPrefabCandidatesMenu()
        {
            ValidateEnemyPrefabCandidates();
            Debug.Log("ActionFoundation enemy prefab candidate validation passed.");
        }

        public static void EnsureEnemyPrefabCandidates()
        {
            EnsureFolder(PrefabRoot);
            ActionFoundationEnemyPatternExpansionSetup.EnsureExtendedPatternAssets();

            CombatAiPatternProfile closePunishProfile =
                LoadAsset<CombatAiPatternProfile>(ActionFoundationProfileSetup.EnemyPatternProfilePath);
            CombatAiPatternProfile heavyWindupProfile =
                LoadAsset<CombatAiPatternProfile>(ActionFoundationProfileSetup.EnemyHeavyWindupPatternProfilePath);
            CombatAiPatternDeck generalDeck =
                LoadAsset<CombatAiPatternDeck>(ActionFoundationEnemyPatternExpansionSetup.GeneralPatternDeckPath);
            CombatAiPatternProfile guardBreakProfile =
                LoadAsset<CombatAiPatternProfile>(ActionFoundationEnemyPatternExpansionSetup.GuardBreakPatternPath);
            CombatAiPatternDeck eliteDeck =
                LoadAsset<CombatAiPatternDeck>(ActionFoundationEnemyPatternExpansionSetup.ElitePatternDeckPath);
            CombatAiElitePatternProfile[] eliteProfiles = LoadEliteProfiles();
            ActionFoundationSciFiSoldier01VisualSetup.EnsureGeneralDeckVisualAssets();
            ActionFoundationSciFiEliteSoldierVisualSetup.EnsureEliteDeckVisualAssets();

            EnsureSoldierPrefabCandidate(
                MeleeSoldierRoleSourcePrefabName,
                MeleeSoldierRoleSourcePrefabPath,
                closePunishProfile,
                null,
                Array.Empty<CombatAiElitePatternProfile>(),
                SoldierVisualCandidate.MaintenanceWorker);
            EnsureSoldierPrefabCandidateFromSource(
                MeleeSoldierPrefabName,
                MeleeSoldierRoleSourcePrefabPath,
                MeleeSoldierPrefabPath,
                heavyWindupProfile,
                null,
                Array.Empty<CombatAiElitePatternProfile>(),
                SoldierVisualCandidate.MaintenanceWorker);
            EnsureSoldierPrefabCandidate(
                GeneralDeckSoldierPrefabName,
                GeneralDeckSoldierPrefabPath,
                closePunishProfile,
                generalDeck,
                Array.Empty<CombatAiElitePatternProfile>(),
                SoldierVisualCandidate.GeneralDeckRifle);
            EnsureSoldierPrefabCandidate(
                EliteDeckSoldierPrefabName,
                EliteDeckSoldierPrefabPath,
                guardBreakProfile,
                eliteDeck,
                eliteProfiles,
                SoldierVisualCandidate.EliteHeavyArmor);

            ActionFoundationEnemyArchetypeSetup.EnsureEnemyArchetypeAssets();
            AssetDatabase.SaveAssets();
        }

        public static void ValidateEnemyPrefabCandidates()
        {
            GameObject meleeRoleSourcePrefabAsset =
                AssetDatabase.LoadAssetAtPath<GameObject>(MeleeSoldierRoleSourcePrefabPath);
            if (meleeRoleSourcePrefabAsset == null)
            {
                throw new InvalidOperationException(
                    $"Missing melee soldier role source prefab at {MeleeSoldierRoleSourcePrefabPath}.");
            }

            GameObject meleePrefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(MeleeSoldierPrefabPath);
            if (meleePrefabAsset == null)
            {
                throw new InvalidOperationException($"Missing melee soldier prefab candidate at {MeleeSoldierPrefabPath}.");
            }

            GameObject generalDeckPrefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(GeneralDeckSoldierPrefabPath);
            if (generalDeckPrefabAsset == null)
            {
                throw new InvalidOperationException($"Missing general-deck soldier prefab candidate at {GeneralDeckSoldierPrefabPath}.");
            }

            GameObject eliteDeckPrefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(EliteDeckSoldierPrefabPath);
            if (eliteDeckPrefabAsset == null)
            {
                throw new InvalidOperationException($"Missing elite-deck soldier prefab candidate at {EliteDeckSoldierPrefabPath}.");
            }

            CombatAiPatternProfile closePunishProfile =
                LoadAsset<CombatAiPatternProfile>(ActionFoundationProfileSetup.EnemyPatternProfilePath);
            CombatAiPatternProfile heavyWindupProfile =
                LoadAsset<CombatAiPatternProfile>(ActionFoundationProfileSetup.EnemyHeavyWindupPatternProfilePath);
            CombatAiPatternDeck generalDeck =
                LoadAsset<CombatAiPatternDeck>(ActionFoundationEnemyPatternExpansionSetup.GeneralPatternDeckPath);
            CombatAiPatternProfile guardBreakProfile =
                LoadAsset<CombatAiPatternProfile>(ActionFoundationEnemyPatternExpansionSetup.GuardBreakPatternPath);
            CombatAiPatternDeck eliteDeck =
                LoadAsset<CombatAiPatternDeck>(ActionFoundationEnemyPatternExpansionSetup.ElitePatternDeckPath);
            CombatAiElitePatternProfile[] eliteProfiles = LoadEliteProfiles();

            ValidateSoldierPrefabAsset(
                MeleeSoldierRoleSourcePrefabPath,
                MeleeSoldierRoleSourcePrefabName,
                closePunishProfile,
                null,
                Array.Empty<CombatAiElitePatternProfile>(),
                SoldierVisualCandidate.MaintenanceWorker);
            ValidateSoldierPrefabAsset(
                MeleeSoldierPrefabPath,
                MeleeSoldierPrefabName,
                heavyWindupProfile,
                null,
                Array.Empty<CombatAiElitePatternProfile>(),
                SoldierVisualCandidate.MaintenanceWorker);
            ValidateMeleeParticipationPrefabAsset(MeleeSoldierPrefabPath, heavyWindupProfile);
            ValidateSoldierPrefabAsset(
                GeneralDeckSoldierPrefabPath,
                GeneralDeckSoldierPrefabName,
                closePunishProfile,
                generalDeck,
                Array.Empty<CombatAiElitePatternProfile>(),
                SoldierVisualCandidate.GeneralDeckRifle);
            ValidateSoldierPrefabAsset(
                EliteDeckSoldierPrefabPath,
                EliteDeckSoldierPrefabName,
                guardBreakProfile,
                eliteDeck,
                eliteProfiles,
                SoldierVisualCandidate.EliteHeavyArmor);

            CombatEnemyArchetypeProfile meleeArchetype =
                LoadAsset<CombatEnemyArchetypeProfile>(ActionFoundationEnemyArchetypeSetup.SciFiMeleeSoldierPath);
            string gameplayPrefabPath = AssetDatabase.GetAssetPath(meleeArchetype.GameplayPrefab).Replace('\\', '/');
            if (!string.Equals(gameplayPrefabPath, MeleeSoldierPrefabPath, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Sci-fi melee archetype should use {MeleeSoldierPrefabPath}, found {gameplayPrefabPath}.");
            }

            CombatEnemyArchetypeProfile rangedArchetype =
                LoadAsset<CombatEnemyArchetypeProfile>(ActionFoundationEnemyArchetypeSetup.SciFiRangedSoldierPath);
            gameplayPrefabPath = AssetDatabase.GetAssetPath(rangedArchetype.GameplayPrefab).Replace('\\', '/');
            if (!string.Equals(gameplayPrefabPath, GeneralDeckSoldierPrefabPath, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Sci-fi ranged archetype should use {GeneralDeckSoldierPrefabPath}, found {gameplayPrefabPath}.");
            }

            CombatEnemyArchetypeProfile eliteArchetype =
                LoadAsset<CombatEnemyArchetypeProfile>(ActionFoundationEnemyArchetypeSetup.SciFiEliteSoldierPath);
            gameplayPrefabPath = AssetDatabase.GetAssetPath(eliteArchetype.GameplayPrefab).Replace('\\', '/');
            if (!string.Equals(gameplayPrefabPath, EliteDeckSoldierPrefabPath, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Sci-fi elite archetype should use {EliteDeckSoldierPrefabPath}, found {gameplayPrefabPath}.");
            }
        }

        private static void EnsureSoldierPrefabCandidate(
            string prefabName,
            string prefabPath,
            CombatAiPatternProfile startingProfile,
            CombatAiPatternDeck patternDeck,
            CombatAiElitePatternProfile[] eliteProfiles,
            SoldierVisualCandidate visualCandidate)
        {
            EnsureSoldierPrefabCandidateFromSource(
                prefabName,
                prefabPath,
                prefabPath,
                startingProfile,
                patternDeck,
                eliteProfiles,
                visualCandidate);
        }

        private static void EnsureSoldierPrefabCandidateFromSource(
            string prefabName,
            string sourcePrefabPath,
            string targetPrefabPath,
            CombatAiPatternProfile startingProfile,
            CombatAiPatternDeck patternDeck,
            CombatAiElitePatternProfile[] eliteProfiles,
            SoldierVisualCandidate visualCandidate)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(sourcePrefabPath) == null)
            {
                throw new InvalidOperationException(
                    $"Missing source prefab {sourcePrefabPath}; retired review scenes are no longer used to bootstrap enemy prefabs.");
            }

            GameObject candidate = PrefabUtility.LoadPrefabContents(sourcePrefabPath);
            try
            {
                candidate.name = prefabName;
                candidate.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                candidate.transform.localScale = Vector3.one;
                candidate.SetActive(true);
                SanitizeSoldierCandidate(candidate, candidate.transform, startingProfile, patternDeck);
                ConfigureEliteProfiles(candidate, eliteProfiles);
                ConfigureSoldierContactDamagePresentation(candidate, visualCandidate);

                if (visualCandidate == SoldierVisualCandidate.GeneralDeckRifle)
                {
                    ActionFoundationSciFiSoldier01VisualSetup.ApplyGeneralDeckVisual(candidate);
                }
                else if (visualCandidate == SoldierVisualCandidate.EliteHeavyArmor)
                {
                    ActionFoundationSciFiEliteSoldierVisualSetup.ApplyEliteDeckVisual(candidate);
                }

                if (PrefabUtility.SaveAsPrefabAsset(candidate, targetPrefabPath) == null)
                {
                    throw new InvalidOperationException(
                        $"Failed to refresh enemy prefab candidate at {targetPrefabPath} from {sourcePrefabPath}.");
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(candidate);
            }
        }

        private static void ValidateSoldierPrefabAsset(
            string prefabPath,
            string expectedName,
            CombatAiPatternProfile expectedProfile,
            CombatAiPatternDeck expectedDeck,
            CombatAiElitePatternProfile[] expectedEliteProfiles,
            SoldierVisualCandidate visualCandidate)
        {
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                ValidateSoldierPrefab(prefabRoot, expectedName, expectedProfile, expectedDeck, expectedEliteProfiles, visualCandidate);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        private static void ValidateMeleeParticipationPrefabAsset(
            string prefabPath,
            CombatAiPatternProfile expectedProfile)
        {
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                BasicSoldierEnemy soldier = RequireComponent<BasicSoldierEnemy>(prefabRoot, prefabRoot.name);
                CombatTargetSensor targetSensor =
                    RequireComponent<CombatTargetSensor>(prefabRoot, prefabRoot.name);
                if (!ReferenceEquals(soldier.PatternProfile, expectedProfile)
                    || soldier.PatternDeck != null
                    || expectedProfile.AttackShape == CombatAiAttackShape.ProjectileLine)
                {
                    throw new InvalidOperationException(
                        $"{prefabRoot.name} must use the exact non-projectile melee profile without a pattern deck.");
                }

                if (expectedProfile.AttackRange > targetSensor.SearchRadius)
                {
                    throw new InvalidOperationException(
                        $"{prefabRoot.name} attack range {expectedProfile.AttackRange:0.###} exceeds sensor radius {targetSensor.SearchRadius:0.###}.");
                }

                if (prefabRoot.GetComponentsInChildren<BasicSoldierProjectileAttackDriver>(true).Length != 0)
                {
                    throw new InvalidOperationException(
                        $"{prefabRoot.name} must not carry a projectile attack driver for melee participation.");
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        private static void SanitizeSoldierCandidate(
            GameObject root,
            Transform sourceRoot,
            CombatAiPatternProfile startingProfile,
            CombatAiPatternDeck patternDeck)
        {
            BasicSoldierEnemy soldier = RequireComponent<BasicSoldierEnemy>(root, $"{root.name} prefab candidate");
            CombatHealth health = RequireComponent<CombatHealth>(root, $"{root.name} prefab candidate");
            CombatTargetSensor targetSensor = RequireComponent<CombatTargetSensor>(root, $"{root.name} prefab candidate");
            CharacterController controller = RequireComponent<CharacterController>(root, $"{root.name} prefab candidate");
            EnemyAttackTelegraphPresenter telegraphPresenter = RequireComponent<EnemyAttackTelegraphPresenter>(root, $"{root.name} prefab candidate");
            Animator animator = root.GetComponentInChildren<Animator>(includeInactive: true);
            if (animator == null)
            {
                throw new InvalidOperationException($"{root.name} prefab candidate needs the promoted enemy Animator.");
            }

            GameObject telegraphObject = EnsureLocalTelegraphObject(root, sourceRoot, telegraphPresenter);
            Renderer telegraphRenderer = telegraphObject.GetComponentInChildren<Renderer>(includeInactive: true);
            Renderer bodyRenderer = FindPreferredBodyRenderer(root, telegraphObject);
            Renderer[] feedbackRenderers = CollectFeedbackRenderers(root, telegraphObject);

            SetObjectReference(soldier, "patternProfile", startingProfile);
            SetObjectReference(soldier, "patternDeck", patternDeck);
            SetObjectReference(soldier, "targetSensor", targetSensor);
            SetObjectReference(soldier, "target", null);
            SetObjectReference(soldier, "targetHealth", null);
            SetObjectReference(soldier, "selfHealth", health);
            SetObjectReference(soldier, "characterController", controller);
            SetObjectReference(soldier, "animator", animator);
            SetObjectReference(soldier, "telegraphIndicator", telegraphObject);
            SetObjectReference(soldier, "telegraphPresenter", telegraphPresenter);
            SetObjectReference(soldier, "bodyRenderer", bodyRenderer);
            SetObjectReference(
                soldier,
                "contactDamageVfxPrefab",
                LoadAsset<GameObject>(ActionFoundationCombatAssetPaths.ContactDamageVfxPrefabPath));
            SetFloat(soldier, "contactDamageVfxScale", 0.46f);
            SetFloat(soldier, "contactDamageVfxHeightOffset", 0.58f);
            SetFloat(soldier, "contactDamageVfxLifetimeSeconds", 0.72f);

            SetObjectReference(targetSensor, "selfHealth", health);
            SetObjectReferenceArray(targetSensor, "targetCandidates", Array.Empty<UnityEngine.Object>());
            ConfigureProjectileAttackDriver(root, soldier, health, targetSensor, startingProfile);

            SetObjectReference(telegraphPresenter, "telegraphObject", telegraphObject);
            SetObjectReference(telegraphPresenter, "telegraphTransform", telegraphObject.transform);
            SetObjectReference(telegraphPresenter, "telegraphRenderer", telegraphRenderer);
            SetObjectReference(telegraphPresenter, "poseRoot", animator.transform);

            CombatHitFeedback hitFeedback = root.GetComponent<CombatHitFeedback>();
            if (hitFeedback != null)
            {
                SetObjectReference(hitFeedback, "health", health);
                SetObjectReferenceArray(hitFeedback, "flashRenderers", feedbackRenderers);
                SetBool(hitFeedback, "renderHitFeedback", true);
                SetBool(hitFeedback, "applyIdleColorOnEnable", false);
            }

            EnemyActionCameraCueDriver cameraCueDriver = root.GetComponent<EnemyActionCameraCueDriver>();
            if (cameraCueDriver != null)
            {
                SetObjectReference(cameraCueDriver, "agentSource", soldier);
                SetObjectReference(cameraCueDriver, "cameraController", null);
                SetObjectReference(cameraCueDriver, "cueSpace", root.transform);
            }

            CombatVfxCuePlayer cuePlayer = root.GetComponent<CombatVfxCuePlayer>();
            if (cuePlayer != null)
            {
                SetObjectReference(cuePlayer, "pooledRoot", EnsureLocalChild(root.transform, VfxPoolChildName));
            }

            EnemyElitePatternController eliteController = root.GetComponent<EnemyElitePatternController>();
            if (eliteController != null)
            {
                SetObjectReference(eliteController, "health", health);
                SetObjectReference(eliteController, "soldier", soldier);
                SetObjectReference(eliteController, "animator", animator);
                SetObjectReference(eliteController, "cueRenderer", bodyRenderer);
                SetObjectReferenceArray(eliteController, "auraProtectedTargets", Array.Empty<UnityEngine.Object>());
                SetObjectReferenceArray(eliteController, "summonSignalObjects", Array.Empty<UnityEngine.Object>());
            }

            EnemyCombatVfxCueDriver vfxCueDriver = root.GetComponent<EnemyCombatVfxCueDriver>();
            if (vfxCueDriver != null)
            {
                SetObjectReference(vfxCueDriver, "agentSource", soldier);
                SetObjectReference(vfxCueDriver, "health", health);
                SetObjectReference(vfxCueDriver, "cuePlayer", cuePlayer);
                SetObjectReference(vfxCueDriver, "cueAnchor", root.transform);
                SetObjectReference(vfxCueDriver, "elitePatternController", eliteController);
                SetFloat(vfxCueDriver, "damageCueIntensity", 1f);
                SetFloat(vfxCueDriver, "pressureDamageCueScale", 0.66f);
                SetBool(vfxCueDriver, "playDamageVfx", true);
            }
        }

        private static void ConfigureProjectileAttackDriver(
            GameObject root,
            BasicSoldierEnemy soldier,
            CombatHealth health,
            CombatTargetSensor targetSensor,
            CombatAiPatternProfile startingProfile)
        {
            BasicSoldierProjectileAttackDriver driver = root.GetComponent<BasicSoldierProjectileAttackDriver>();
            if (startingProfile == null || startingProfile.AttackShape != CombatAiAttackShape.ProjectileLine)
            {
                if (driver != null)
                {
                    UnityEngine.Object.DestroyImmediate(driver);
                }

                return;
            }

            if (driver == null)
            {
                driver = root.AddComponent<BasicSoldierProjectileAttackDriver>();
            }

            Transform projectileOrigin = EnsureLocalChild(root.transform, ProjectileOriginChildName);
            projectileOrigin.localPosition = new Vector3(0f, 1.22f, 0.32f);
            projectileOrigin.localRotation = Quaternion.identity;
            projectileOrigin.localScale = Vector3.one;

            GameObject projectilePrefabObject =
                LoadAsset<GameObject>(ActionFoundationCombatAssetPaths.SummonSlot2ProjectilePrefabPath);
            LaneActionProjectile projectilePrefab = projectilePrefabObject.GetComponent<LaneActionProjectile>();
            if (projectilePrefab == null)
            {
                throw new InvalidOperationException(
                    $"{ActionFoundationCombatAssetPaths.SummonSlot2ProjectilePrefabPath} is missing LaneActionProjectile.");
            }

            SetObjectReference(driver, "soldier", soldier);
            SetObjectReference(driver, "sourceHealth", health);
            SetObjectReference(driver, "targetSensor", targetSensor);
            SetObjectReference(driver, "projectileOrigin", projectileOrigin);
            SetObjectReference(driver, "projectilePrefab", projectilePrefab);
            SetObjectReference(driver, "projectileRoot", EnsureLocalChild(root.transform, VfxPoolChildName));
            SetFloat(driver, "projectileDamageOverride", 0f);
            SetFloat(driver, "projectileSpeed", 18f);
            SetFloat(driver, "projectileLifetimeSeconds", 1.25f);
            SetFloat(driver, "projectileRadius", 0.2f);
            SetFloat(driver, "originHeight", 1.22f);
            SetFloat(driver, "targetHeightOffset", 0.9f);
        }

        private static void ConfigureEliteProfiles(GameObject root, CombatAiElitePatternProfile[] eliteProfiles)
        {
            if (eliteProfiles.Length == 0)
            {
                return;
            }

            EnemyElitePatternController eliteController = root.GetComponent<EnemyElitePatternController>();
            if (eliteController == null)
            {
                eliteController = root.AddComponent<EnemyElitePatternController>();
            }

            BasicSoldierEnemy soldier = RequireComponent<BasicSoldierEnemy>(root, root.name);
            CombatHealth health = RequireComponent<CombatHealth>(root, root.name);
            Animator animator = root.GetComponentInChildren<Animator>(includeInactive: true);
            if (animator == null)
            {
                throw new InvalidOperationException($"{root.name} elite prefab candidate needs an Animator before elite profile binding.");
            }

            Renderer bodyRenderer = GetObjectReference<Renderer>(soldier, "bodyRenderer");
            SetObjectReference(eliteController, "health", health);
            SetObjectReference(eliteController, "soldier", soldier);
            SetObjectReference(eliteController, "animator", animator);
            SetObjectReference(eliteController, "cueRenderer", bodyRenderer);
            SetObjectReferenceArray(eliteController, "eliteProfiles", eliteProfiles);
            SetObjectReferenceArray(eliteController, "auraProtectedTargets", Array.Empty<UnityEngine.Object>());
            SetObjectReferenceArray(eliteController, "summonSignalObjects", Array.Empty<UnityEngine.Object>());

            EnemyCombatVfxCueDriver vfxCueDriver = root.GetComponent<EnemyCombatVfxCueDriver>();
            if (vfxCueDriver != null)
            {
                SetObjectReference(vfxCueDriver, "elitePatternController", eliteController);
            }
        }

        private static void ConfigureSoldierContactDamagePresentation(
            GameObject root,
            SoldierVisualCandidate visualCandidate)
        {
            BasicSoldierEnemy soldier = RequireComponent<BasicSoldierEnemy>(root, root.name);
            SetObjectReference(
                soldier,
                "contactDamageVfxPrefab",
                LoadAsset<GameObject>(ActionFoundationCombatAssetPaths.ContactDamageVfxPrefabPath));
            SetFloat(soldier, "contactDamageVfxScale", 0.46f);
            SetFloat(soldier, "contactDamageVfxHeightOffset", 0.58f);
            SetFloat(soldier, "contactDamageVfxLifetimeSeconds", 0.72f);

            if (visualCandidate == SoldierVisualCandidate.GeneralDeckRifle
                || visualCandidate == SoldierVisualCandidate.EliteHeavyArmor)
            {
                SetFloat(soldier, "attackRangeSlowdownDistance", 1.05f);
                SetFloat(soldier, "attackRange", 2.25f);
            }
        }

        private static void ValidateSoldierPrefab(
            GameObject root,
            string expectedName,
            CombatAiPatternProfile expectedProfile,
            CombatAiPatternDeck expectedDeck,
            CombatAiElitePatternProfile[] expectedEliteProfiles,
            SoldierVisualCandidate visualCandidate)
        {
            if (!string.Equals(root.name, expectedName, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Soldier prefab root should be named {expectedName}.");
            }

            BasicSoldierEnemy soldier = RequireComponent<BasicSoldierEnemy>(root, root.name);
            CombatHealth health = RequireComponent<CombatHealth>(root, root.name);
            CombatTargetSensor targetSensor = RequireComponent<CombatTargetSensor>(root, root.name);
            CharacterController controller = RequireComponent<CharacterController>(root, root.name);
            EnemyAttackTelegraphPresenter telegraphPresenter = RequireComponent<EnemyAttackTelegraphPresenter>(root, root.name);
            Animator animator = root.GetComponentInChildren<Animator>(includeInactive: true);
            if (animator == null)
            {
                throw new InvalidOperationException($"{root.name} is missing the promoted enemy Animator.");
            }

            ValidateObjectReference(soldier, "patternProfile", expectedProfile);
            ValidateObjectReference(soldier, "patternDeck", expectedDeck);
            ValidateObjectReference(soldier, "targetSensor", targetSensor);
            ValidateObjectReference(soldier, "target", null);
            ValidateObjectReference(soldier, "targetHealth", null);
            ValidateObjectReference(soldier, "selfHealth", health);
            ValidateObjectReference(soldier, "characterController", controller);
            ValidateObjectReference(soldier, "animator", animator);
            ValidateObjectReference(soldier, "telegraphPresenter", telegraphPresenter);
            ValidateObjectReference(
                soldier,
                "contactDamageVfxPrefab",
                AssetDatabase.LoadAssetAtPath<GameObject>(
                    ActionFoundationCombatAssetPaths.ContactDamageVfxPrefabPath));
            ValidateFloat(soldier, "contactDamageVfxScale", 0.46f);
            ValidateFloat(soldier, "contactDamageVfxHeightOffset", 0.58f);
            ValidateFloat(soldier, "contactDamageVfxLifetimeSeconds", 0.72f);

            if (targetSensor.TargetCandidateCount != 0)
            {
                throw new InvalidOperationException($"{root.name} prefab target sensor should not carry scene target candidates.");
            }

            ValidateObjectReference(targetSensor, "selfHealth", health);
            ValidateLocalReference(telegraphPresenter, "telegraphObject", root);
            ValidateLocalReference(telegraphPresenter, "telegraphTransform", root);
            ValidateLocalReference(telegraphPresenter, "telegraphRenderer", root);
            ValidateLocalReference(telegraphPresenter, "poseRoot", root);
            ValidateProjectileAttackDriver(root, soldier, health, targetSensor, expectedProfile);

            EnemyActionCameraCueDriver cameraCueDriver = root.GetComponent<EnemyActionCameraCueDriver>();
            if (cameraCueDriver != null)
            {
                ValidateObjectReference(cameraCueDriver, "agentSource", soldier);
                ValidateObjectReference(cameraCueDriver, "cameraController", null);
                ValidateObjectReference(cameraCueDriver, "cueSpace", root.transform);
            }

            CombatVfxCuePlayer cuePlayer = RequireComponent<CombatVfxCuePlayer>(root, root.name);
            ValidateAssetReferencePath(cuePlayer, "profile", "Assets/_Game/");
            ValidateLocalReference(cuePlayer, "pooledRoot", root);

            EnemyCombatVfxCueDriver vfxCueDriver = RequireComponent<EnemyCombatVfxCueDriver>(root, root.name);
            ValidateObjectReference(vfxCueDriver, "agentSource", soldier);
            ValidateObjectReference(vfxCueDriver, "health", health);
            ValidateObjectReference(vfxCueDriver, "cuePlayer", cuePlayer);
            ValidateObjectReference(vfxCueDriver, "cueAnchor", root.transform);
            ValidateFloat(vfxCueDriver, "damageCueIntensity", 1f);
            ValidateFloat(vfxCueDriver, "pressureDamageCueScale", 0.66f);

            if (expectedDeck != null)
            {
                if (visualCandidate == SoldierVisualCandidate.GeneralDeckRifle)
                {
                    ActionFoundationSciFiSoldier01VisualSetup.ValidateGeneralDeckVisual(root);
                }
                else if (visualCandidate == SoldierVisualCandidate.EliteHeavyArmor)
                {
                    ActionFoundationSciFiEliteSoldierVisualSetup.ValidateEliteDeckVisual(root);
                }
            }

            if (expectedEliteProfiles.Length > 0)
            {
                EnemyElitePatternController eliteController = RequireComponent<EnemyElitePatternController>(root, root.name);
                ValidateObjectReference(eliteController, "health", health);
                ValidateObjectReference(eliteController, "soldier", soldier);
                ValidateObjectReference(eliteController, "animator", animator);
                ValidateObjectReferenceArray(eliteController, "eliteProfiles", expectedEliteProfiles);
                ValidateObjectReferenceArray(eliteController, "auraProtectedTargets", Array.Empty<UnityEngine.Object>());
                ValidateObjectReferenceArray(eliteController, "summonSignalObjects", Array.Empty<UnityEngine.Object>());
                ValidateObjectReference(vfxCueDriver, "elitePatternController", eliteController);
            }

            ValidateNoRawImportedOrExternalSceneReferences(root);
        }

        private static void ValidateProjectileAttackDriver(
            GameObject root,
            BasicSoldierEnemy soldier,
            CombatHealth health,
            CombatTargetSensor targetSensor,
            CombatAiPatternProfile expectedProfile)
        {
            BasicSoldierProjectileAttackDriver driver = root.GetComponent<BasicSoldierProjectileAttackDriver>();
            if (expectedProfile == null || expectedProfile.AttackShape != CombatAiAttackShape.ProjectileLine)
            {
                if (driver != null)
                {
                    throw new InvalidOperationException($"{root.name} should not carry a projectile attack driver.");
                }

                return;
            }

            if (driver == null)
            {
                throw new InvalidOperationException($"{root.name} should carry a projectile attack driver for {expectedProfile.PatternId}.");
            }

            ValidateObjectReference(driver, "soldier", soldier);
            ValidateObjectReference(driver, "sourceHealth", health);
            ValidateObjectReference(driver, "targetSensor", targetSensor);
            ValidateLocalReference(driver, "projectileOrigin", root);
            ValidateObjectReference(
                driver,
                "projectilePrefab",
                LoadAsset<GameObject>(ActionFoundationCombatAssetPaths.SummonSlot2ProjectilePrefabPath)
                    .GetComponent<LaneActionProjectile>());
            ValidateLocalReference(driver, "projectileRoot", root);
            ValidateFloat(driver, "projectileSpeed", 18f);
            ValidateFloat(driver, "projectileLifetimeSeconds", 1.25f);
            ValidateFloat(driver, "projectileRadius", 0.2f);
        }

        private static GameObject EnsureLocalTelegraphObject(
            GameObject root,
            Transform sourceRoot,
            EnemyAttackTelegraphPresenter presenter)
        {
            GameObject telegraphObject = GetObjectReference<GameObject>(presenter, "telegraphObject");
            if (telegraphObject != null && IsLocalPrefabReference(telegraphObject, root))
            {
                return telegraphObject;
            }

            GameObject localTelegraph;
            if (telegraphObject != null)
            {
                localTelegraph = UnityEngine.Object.Instantiate(telegraphObject);
                localTelegraph.name = telegraphObject.name;
                localTelegraph.transform.SetParent(root.transform, worldPositionStays: false);
                ApplySourceRelativeTransform(localTelegraph.transform, telegraphObject.transform, sourceRoot);
            }
            else
            {
                localTelegraph = GameObject.CreatePrimitive(PrimitiveType.Cube);
                localTelegraph.name = "ReadableAttackTelegraph";
                localTelegraph.transform.SetParent(root.transform, worldPositionStays: false);
                localTelegraph.transform.localPosition = new Vector3(0f, 0.03f, 1f);
                localTelegraph.transform.localRotation = Quaternion.identity;
                localTelegraph.transform.localScale = new Vector3(1.05f, 0.02f, 1.55f);
            }

            localTelegraph.SetActive(false);
            return localTelegraph;
        }

        private static void ApplySourceRelativeTransform(Transform target, Transform source, Transform sourceRoot)
        {
            if (sourceRoot == null)
            {
                target.localPosition = source.localPosition;
                target.localRotation = source.localRotation;
                target.localScale = source.localScale;
                return;
            }

            target.localPosition = sourceRoot.InverseTransformPoint(source.position);
            target.localRotation = Quaternion.Inverse(sourceRoot.rotation) * source.rotation;
            target.localScale = source.localScale;
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

        private static Renderer FindPreferredBodyRenderer(GameObject root, GameObject telegraphObject)
        {
            Renderer[] renderers = CollectFeedbackRenderers(root, telegraphObject);
            if (renderers.Length == 0)
            {
                throw new InvalidOperationException($"{root.name} needs at least one non-telegraph renderer for hit feedback.");
            }

            return renderers[0];
        }

        private static Renderer[] CollectFeedbackRenderers(GameObject root, GameObject telegraphObject)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(includeInactive: true);
            var feedbackRenderers = new List<Renderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || IsInside(renderer.transform, telegraphObject))
                {
                    continue;
                }

                feedbackRenderers.Add(renderer);
            }

            return feedbackRenderers.ToArray();
        }

        private static bool IsInside(Transform transform, GameObject possibleParent)
        {
            return possibleParent != null && transform != null && transform.IsChildOf(possibleParent.transform);
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

        private static CombatAiElitePatternProfile[] LoadEliteProfiles()
        {
            return new[]
            {
                LoadAsset<CombatAiElitePatternProfile>(ActionFoundationEnemyPatternExpansionSetup.ShieldCycleEliteProfilePath),
                LoadAsset<CombatAiElitePatternProfile>(ActionFoundationEnemyPatternExpansionSetup.ArmorBreakEliteProfilePath),
                LoadAsset<CombatAiElitePatternProfile>(ActionFoundationEnemyPatternExpansionSetup.AuraBufferEliteProfilePath),
                LoadAsset<CombatAiElitePatternProfile>(ActionFoundationEnemyPatternExpansionSetup.SummonPackageEliteProfilePath),
                LoadAsset<CombatAiElitePatternProfile>(ActionFoundationEnemyPatternExpansionSetup.PhaseSwapEliteProfilePath)
            };
        }

        private static T GetObjectReference<T>(UnityEngine.Object target, string propertyName) where T : UnityEngine.Object
        {
            SerializedProperty property = RequireProperty(new SerializedObject(target), propertyName);
            return property.objectReferenceValue as T;
        }

        private static void SetObjectReference(UnityEngine.Object target, string propertyName, UnityEngine.Object value)
        {
            var serializedObject = new SerializedObject(target);
            RequireProperty(serializedObject, propertyName).objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
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

        private static void SetFloat(UnityEngine.Object target, string propertyName, float value)
        {
            var serializedObject = new SerializedObject(target);
            RequireProperty(serializedObject, propertyName).floatValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetBool(UnityEngine.Object target, string propertyName, bool value)
        {
            var serializedObject = new SerializedObject(target);
            RequireProperty(serializedObject, propertyName).boolValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
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

        private static void ValidateObjectReferenceArray(UnityEngine.Object target, string propertyName, UnityEngine.Object[] expected)
        {
            SerializedProperty array = RequireProperty(new SerializedObject(target), propertyName);
            if (array.arraySize != expected.Length)
            {
                throw new InvalidOperationException($"{target.name}.{propertyName} expected {expected.Length} entries, found {array.arraySize}.");
            }

            for (int i = 0; i < expected.Length; i++)
            {
                UnityEngine.Object actual = array.GetArrayElementAtIndex(i).objectReferenceValue;
                if (actual != expected[i])
                {
                    string expectedName = expected[i] != null ? expected[i].name : "null";
                    string actualName = actual != null ? actual.name : "null";
                    throw new InvalidOperationException($"{target.name}.{propertyName}[{i}] expected {expectedName}, found {actualName}.");
                }
            }
        }

        private static void ValidateFloat(UnityEngine.Object target, string propertyName, float expected)
        {
            float actual = RequireProperty(new SerializedObject(target), propertyName).floatValue;
            if (!Mathf.Approximately(actual, expected))
            {
                throw new InvalidOperationException($"{target.name}.{propertyName} expected {expected}, found {actual}.");
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

        private static void ValidateAssetReferencePath(UnityEngine.Object target, string propertyName, string requiredPrefix)
        {
            UnityEngine.Object reference = RequireProperty(new SerializedObject(target), propertyName).objectReferenceValue;
            if (reference == null)
            {
                throw new InvalidOperationException($"{target.name}.{propertyName} is not assigned.");
            }

            string assetPath = AssetDatabase.GetAssetPath(reference).Replace('\\', '/');
            if (!assetPath.StartsWith(requiredPrefix, StringComparison.Ordinal) || assetPath.Contains("/_Imported/", StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"{target.name}.{propertyName} should reference {requiredPrefix}, found {assetPath}.");
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

        private enum SoldierVisualCandidate
        {
            MaintenanceWorker,
            GeneralDeckRifle,
            EliteHeavyArmor
        }
    }
}
