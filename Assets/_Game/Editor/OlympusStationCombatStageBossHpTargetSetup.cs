#if UNITY_EDITOR
using System;
using System.Reflection;
using DimensionBrawl.Combat;
using DimensionBrawl.Player;
using DimensionBrawl.Presentation;
using DimensionBrawl.Test;
using DimensionBrawl.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DimensionBrawl.Editor
{
    public static class OlympusStationCombatStageBossHpTargetSetup
    {
        private const string ScenePath = "Assets/_Game/Scenes/OlympusStationCombatStage.unity";
        private const string BossProxyRootName = "BossBarrageLaneReview_BossProxy_NeedleLock";
        private const string RuntimeBinderRootName = "OlympusStation_BossHpTargetRuntimeBinder";
        private const float BossTargetingDistance = 80f;

        [MenuItem("DimensionBrawl/Stage/Olympus Station/Fix Boss HP Targets")]
        public static void ApplyMenu()
        {
            ApplyToScene();
        }

        public static void RunBatchApplyBossHpTargets()
        {
            try
            {
                ApplyToScene();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        public static void RunBatchVerifyBossHpTargets()
        {
            try
            {
                VerifyScene();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        public static void RunBatchVerifyBossProjectileDamage()
        {
            try
            {
                VerifyProjectileDamage();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        public static void RunBatchApplyAndVerifyBossHpTargets()
        {
            try
            {
                ApplyToScene();
                VerifyScene();
                VerifyProjectileDamage();
                VerifyPlayerRangedFireDamage();
                VerifySustainedPlayerRangedFireDamageAndHud();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        public static void RunBatchVerifyPlayerRangedFireBossDamage()
        {
            try
            {
                VerifyPlayerRangedFireDamage();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        public static void RunBatchVerifySustainedPlayerRangedFireBossHudDamage()
        {
            try
            {
                VerifySustainedPlayerRangedFireDamageAndHud();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        public static void ApplyToScene()
        {
            AssetDatabase.Refresh();
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            CombatHealth bossHealth = ResolveBossHealth(scene);
            CombatHealth playerHealth = FirstSceneComponent<PlayerCombatTargetSelector>(scene)?.SelfHealth;

            if (playerHealth == null)
            {
                playerHealth = ResolvePlayerHealth(scene);
            }

            bossHealth.ConfigureTeam(DamageTeam.Enemy);
            bossHealth.ConfigureMaxHealth(Mathf.Max(1f, bossHealth.MaxHealth), resetToFull: true);

            PlayerCombatTargetSelector targetSelector =
                RequireSceneComponent<PlayerCombatTargetSelector>(scene, "player target selector");
            SetObjectReference(targetSelector, "selfHealth", playerHealth);
            SetObjectReferenceArray(targetSelector, "targetCandidates", new UnityEngine.Object[] { bossHealth });
            SetFloat(targetSelector, "selectionRadius", BossTargetingDistance);
            SetFloat(targetSelector, "attackAimRadius", BossTargetingDistance);

            PlayerLockTargetController lockTargetController =
                FirstSceneComponent<PlayerLockTargetController>(scene);
            if (lockTargetController != null)
            {
                SetObjectReference(lockTargetController, "targetSelector", targetSelector);
                SetObjectReference(lockTargetController, "sourceHealth", playerHealth);
                SetFloat(lockTargetController, "softLockDistance", BossTargetingDistance);
                SetFloat(lockTargetController, "lockBreakDistance", BossTargetingDistance);
                SetFloat(lockTargetController, "softLockAngleDegrees", 120f);
                SetFloat(lockTargetController, "retainedLockAngleDegrees", 160f);
            }

            PlayerRangedBasicAttackAction rangedBasicAttack =
                RequireSceneComponent<PlayerRangedBasicAttackAction>(scene, "player ranged basic attack");
            SetObjectReference(rangedBasicAttack, "targetSelector", targetSelector);
            SetObjectReference(rangedBasicAttack, "lockTargetController", lockTargetController);
            SetObjectReference(rangedBasicAttack, "sourceHealth", playerHealth);
            SetBool(rangedBasicAttack, "useAimAssist", true);
            SetBool(rangedBasicAttack, "disableAimAssistWithManualInput", false);
            SetFloat(rangedBasicAttack, "aimAssistDistance", BossTargetingDistance);
            SetFloat(rangedBasicAttack, "hipAimAssistAngleDegrees", 45f);
            SetFloat(rangedBasicAttack, "aimedAimAssistAngleDegrees", 45f);
            SetFloat(rangedBasicAttack, "aimAssistMaxTurnDegrees", 45f);
            SetBool(rangedBasicAttack, "cameraAimIgnoresNonTargetHits", true);

            ActionFoundationTestEncounter encounter =
                RequireSceneComponent<ActionFoundationTestEncounter>(scene, "test encounter");
            SetObjectReference(encounter, "playerHealth", playerHealth);
            SetObjectReference(encounter, "enemyHealth", bossHealth);

            PlayerSummonSlot1Action summonSlot1 = FirstSceneComponent<PlayerSummonSlot1Action>(scene);
            if (summonSlot1 != null)
            {
                SetObjectReference(summonSlot1, "frontlineTargetHealth", bossHealth);
            }

            PlayerSupportSummonSlotAction[] supportSummons =
                CollectComponents<PlayerSupportSummonSlotAction>(scene);
            for (int i = 0; i < supportSummons.Length; i++)
            {
                SetObjectReference(supportSummons[i], "frontlineTargetHealth", bossHealth);
            }

            BossBarragePocketReviewOwner pocketOwner = FirstSceneComponent<BossBarragePocketReviewOwner>(scene);
            if (pocketOwner != null)
            {
                SetObjectReference(pocketOwner, "playerHealth", playerHealth);
                SetObjectReference(pocketOwner, "closeThreatHealth", null);
                SetObjectReference(pocketOwner, "bossHealth", bossHealth);
            }

            BossBarrageLaneReviewHud reviewHud = FirstSceneComponent<BossBarrageLaneReviewHud>(scene);
            if (reviewHud != null)
            {
                SetObjectReference(reviewHud, "playerHealth", playerHealth);
                SetObjectReference(reviewHud, "closeThreatHealth", null);
                SetObjectReference(reviewHud, "bossHealth", bossHealth);
            }

            BossBarrageLaneReviewCombatHudBinder combatHudBinder =
                FirstSceneComponent<BossBarrageLaneReviewCombatHudBinder>(scene);
            if (combatHudBinder != null)
            {
                SetObjectReference(combatHudBinder, "playerHealth", playerHealth);
                SetObjectReference(combatHudBinder, "bossHealth", bossHealth);
            }

            Component runtimeBinder =
                EnsureRuntimeBinder(scene, playerHealth, bossHealth, targetSelector, lockTargetController, rangedBasicAttack, encounter);

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene))
            {
                throw new InvalidOperationException($"Failed to save {ScenePath}.");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log(
                $"Applied OlympusStation boss HP targets. boss={GetHierarchyPath(bossHealth.transform)} " +
                $"maxHealth={bossHealth.MaxHealth} candidates=1 encounterEnemy=boss runtimeBinder={runtimeBinder.name}.");
        }

        private static void VerifyScene()
        {
            AssetDatabase.Refresh();
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            CombatHealth bossHealth = ResolveBossHealth(scene);
            PlayerCombatTargetSelector targetSelector =
                RequireSceneComponent<PlayerCombatTargetSelector>(scene, "player target selector");
            ActionFoundationTestEncounter encounter =
                RequireSceneComponent<ActionFoundationTestEncounter>(scene, "test encounter");

            ValidateArrayReference(targetSelector, "targetCandidates", 0, bossHealth, expectedSize: 1);
            ValidateObjectReference(encounter, "enemyHealth", bossHealth);

            bossHealth.ConfigureTeam(DamageTeam.Enemy);
            bossHealth.ConfigureMaxHealth(Mathf.Max(1f, bossHealth.MaxHealth), resetToFull: true);
            float before = bossHealth.CurrentHealth;
            bool applied = bossHealth.TryApplyDamage(new DamageInfo(
                null,
                DamageTeam.Player,
                30f,
                bossHealth.transform.position,
                Vector3.back,
                0f,
                DamageResponsePolicy.FlashOnly,
                CombatControlLockPolicy.None));
            float after = bossHealth.CurrentHealth;
            int colliderCount = bossHealth.GetComponentsInChildren<Collider>(includeInactive: true).Length;

            if (!applied || !(after < before))
            {
                throw new InvalidOperationException(
                    $"Boss direct damage failed. applied={applied} before={before} after={after}.");
            }

            if (colliderCount <= 0)
            {
                throw new InvalidOperationException("Boss health has no child colliders for projectile hits.");
            }

            Debug.Log(
                $"Verified OlympusStation boss HP damage. before={before} after={after} " +
                $"colliders={colliderCount} targetCandidates=1 encounterEnemy=boss.");
        }

        private static void VerifyProjectileDamage()
        {
            AssetDatabase.Refresh();
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            CombatHealth bossHealth = ResolveBossHealth(scene);
            CombatHealth playerHealth = ResolvePlayerHealth(scene);
            Collider bossCollider = ResolveActiveBossCollider(bossHealth);

            bossHealth.ConfigureTeam(DamageTeam.Enemy);
            bossHealth.ConfigureMaxHealth(Mathf.Max(1f, bossHealth.MaxHealth), resetToFull: true);
            float before = bossHealth.CurrentHealth;

            GameObject probeObject = new GameObject("OlympusStation_BossProjectileDamageProbe");
            try
            {
                SceneManager.MoveGameObjectToScene(probeObject, scene);
                probeObject.AddComponent<SphereCollider>();
                probeObject.AddComponent<Rigidbody>();
                LaneActionProjectile projectile = probeObject.AddComponent<LaneActionProjectile>();
                projectile.Configure(
                    playerHealth,
                    DamageTeam.Player,
                    30f,
                    Vector3.forward,
                    24f,
                    1f,
                    0.3f,
                    DamageResponsePolicy.FlashOnly,
                    CombatControlLockPolicy.None);

                bool applied = projectile.TryApplyImpact(bossCollider, bossCollider.bounds.center);
                float after = bossHealth.CurrentHealth;
                if (!applied
                    || projectile.LastImpactResult != ProjectileImpactResult.AppliedDamage
                    || !(after < before))
                {
                    throw new InvalidOperationException(
                        "Boss projectile damage failed. " +
                        $"applied={applied} impact={projectile.LastImpactResult} before={before} after={after} " +
                        $"collider={GetHierarchyPath(bossCollider.transform)}.");
                }

            Debug.Log(
                $"Verified OlympusStation projectile boss damage. before={before} after={after} " +
                $"impact={projectile.LastImpactResult} collider={GetHierarchyPath(bossCollider.transform)}.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(probeObject);
            }
        }

        private static void VerifyPlayerRangedFireDamage()
        {
            AssetDatabase.Refresh();
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            CombatHealth bossHealth = ResolveBossHealth(scene);
            CombatHealth playerHealth = ResolvePlayerHealth(scene);
            Collider bossCollider = ResolveActiveBossCollider(bossHealth);
            PlayerCombatTargetSelector targetSelector =
                RequireSceneComponent<PlayerCombatTargetSelector>(scene, "player target selector");
            PlayerLockTargetController lockTargetController =
                RequireSceneComponent<PlayerLockTargetController>(scene, "player lock target controller");
            PlayerRangedBasicAttackAction rangedBasicAttack =
                RequireSceneComponent<PlayerRangedBasicAttackAction>(scene, "player ranged basic attack");

            playerHealth.ConfigureTeam(DamageTeam.Player);
            bossHealth.ConfigureTeam(DamageTeam.Enemy);
            bossHealth.ConfigureMaxHealth(Mathf.Max(1f, bossHealth.MaxHealth), resetToFull: true);

            SetObjectReference(targetSelector, "selfHealth", playerHealth);
            SetObjectReferenceArray(targetSelector, "targetCandidates", new UnityEngine.Object[] { bossHealth });
            SetFloat(targetSelector, "selectionRadius", BossTargetingDistance);
            SetFloat(targetSelector, "attackAimRadius", BossTargetingDistance);
            targetSelector.ConfigureTargetCandidates(new[] { bossHealth }, refreshNow: true);

            SetObjectReference(lockTargetController, "targetSelector", targetSelector);
            SetObjectReference(lockTargetController, "sourceHealth", playerHealth);
            Vector3 lockPoint = bossCollider.bounds.center;
            SetPrivateField(lockTargetController, "currentTargetHealth", bossHealth);
            SetPrivateField(lockTargetController, "currentTargetPoint", lockPoint);
            SetPrivateField(lockTargetController, "currentLockType", PlayerLockTargetController.LockTargetType.HardLock);
            SetPrivateField(lockTargetController, "currentStrength01", 1f);
            SetPrivateField(lockTargetController, "requestedHardLockTarget", bossHealth);

            SetObjectReference(rangedBasicAttack, "targetSelector", targetSelector);
            SetObjectReference(rangedBasicAttack, "lockTargetController", lockTargetController);
            SetObjectReference(rangedBasicAttack, "sourceHealth", playerHealth);
            SetBool(rangedBasicAttack, "requireAimToFire", false);
            SetBool(rangedBasicAttack, "useAimAssist", true);
            SetBool(rangedBasicAttack, "disableAimAssistWithManualInput", false);
            SetBool(rangedBasicAttack, "cameraAimIgnoresNonTargetHits", true);
            SetFloat(rangedBasicAttack, "aimAssistDistance", BossTargetingDistance);
            SetFloat(rangedBasicAttack, "hipAimAssistAngleDegrees", 45f);
            SetFloat(rangedBasicAttack, "aimedAimAssistAngleDegrees", 45f);
            SetFloat(rangedBasicAttack, "aimAssistMaxTurnDegrees", 45f);
            SetPrivateField(rangedBasicAttack, "cinematicInputLocked", false);
            SetPrivateField(rangedBasicAttack, "nextFireTime", 0f);
            SetPrivateField(rangedBasicAttack, "isReloading", false);
            SetPrivateField(rangedBasicAttack, "reloadStartedByAimRelease", false);
            SetPrivateField(rangedBasicAttack, "reloadFinishTime", 0f);
            SetPrivateField(rangedBasicAttack, "ammoInitialized", true);
            SetPrivateField(rangedBasicAttack, "currentAmmo", Mathf.Max(1, rangedBasicAttack.MagazineSize));
            SetPrivateField(rangedBasicAttack, "aimInput", Vector2.zero);
            SetPrivateField(rangedBasicAttack, "hasCachedFirePreview", false);
            SetPrivateField(rangedBasicAttack, "firePreviewFrame", -1);

            Physics.SyncTransforms();
            float before = bossHealth.CurrentHealth;
            LaneActionProjectile firedProjectile = null;
            void CaptureProjectile(LaneActionProjectile projectile)
            {
                firedProjectile = projectile;
            }

            rangedBasicAttack.RangedProjectileFired += CaptureProjectile;
            bool fired = rangedBasicAttack.TryFire();
            rangedBasicAttack.RangedProjectileFired -= CaptureProjectile;
            if (!fired || firedProjectile == null)
            {
                throw new InvalidOperationException(
                    "Player ranged fire did not create a projectile. " +
                    $"fired={fired} blocked='{rangedBasicAttack.LastUseBlockedReason}'.");
            }

            try
            {
                firedProjectile.name = "OlympusStation_PlayerRangedFireDamageProbe";
                for (int i = 0; i < 240 && bossHealth.CurrentHealth >= before; i++)
                {
                    Physics.SyncTransforms();
                    firedProjectile.Tick(1f / 60f);
                }

                float after = bossHealth.CurrentHealth;
                if (!(after < before)
                    || firedProjectile.LastImpactResult != ProjectileImpactResult.AppliedDamage
                    || firedProjectile.LastImpactTargetHealth != bossHealth)
                {
                    throw new InvalidOperationException(
                        "Player ranged fire did not damage boss HP. " +
                        $"before={before} after={after} impact={firedProjectile.LastImpactResult} " +
                        $"target={NameOf(firedProjectile.LastImpactTargetHealth)} " +
                        $"direction={firedProjectile.TravelDirection} " +
                        $"projectilePosition={firedProjectile.transform.position} lockPoint={lockPoint}.");
                }

                Debug.Log(
                    $"Verified OlympusStation player ranged fire boss damage. before={before} after={after} " +
                    $"impact={firedProjectile.LastImpactResult} target={GetHierarchyPath(bossHealth.transform)} " +
                    $"direction={firedProjectile.TravelDirection}.");
            }
            finally
            {
                if (firedProjectile != null)
                {
                    UnityEngine.Object.DestroyImmediate(firedProjectile.gameObject);
                }
            }
        }

        private static void VerifySustainedPlayerRangedFireDamageAndHud()
        {
            const int shotsToFire = 30;

            AssetDatabase.Refresh();
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            CombatHealth bossHealth = ResolveBossHealth(scene);
            CombatHealth playerHealth = ResolvePlayerHealth(scene);
            Collider bossCollider = ResolveActiveBossCollider(bossHealth);
            PlayerCombatTargetSelector targetSelector =
                RequireSceneComponent<PlayerCombatTargetSelector>(scene, "player target selector");
            PlayerLockTargetController lockTargetController =
                RequireSceneComponent<PlayerLockTargetController>(scene, "player lock target controller");
            PlayerRangedBasicAttackAction rangedBasicAttack =
                RequireSceneComponent<PlayerRangedBasicAttackAction>(scene, "player ranged basic attack");
            CombatHudPresenter hudPresenter =
                RequireSceneComponent<CombatHudPresenter>(scene, "combat HUD presenter");

            playerHealth.ConfigureTeam(DamageTeam.Player);
            bossHealth.ConfigureTeam(DamageTeam.Enemy);
            bossHealth.ConfigureMaxHealth(Mathf.Max(1f, bossHealth.MaxHealth), resetToFull: true);

            SetObjectReference(targetSelector, "selfHealth", playerHealth);
            SetObjectReferenceArray(targetSelector, "targetCandidates", new UnityEngine.Object[] { bossHealth });
            SetFloat(targetSelector, "selectionRadius", BossTargetingDistance);
            SetFloat(targetSelector, "attackAimRadius", BossTargetingDistance);
            targetSelector.ConfigureTargetCandidates(new[] { bossHealth }, refreshNow: true);

            SetObjectReference(lockTargetController, "targetSelector", targetSelector);
            SetObjectReference(lockTargetController, "sourceHealth", playerHealth);
            Vector3 lockPoint = bossCollider.bounds.center;
            SetPrivateField(lockTargetController, "currentTargetHealth", bossHealth);
            SetPrivateField(lockTargetController, "currentTargetPoint", lockPoint);
            SetPrivateField(lockTargetController, "currentLockType", PlayerLockTargetController.LockTargetType.HardLock);
            SetPrivateField(lockTargetController, "currentStrength01", 1f);
            SetPrivateField(lockTargetController, "requestedHardLockTarget", bossHealth);

            SetObjectReference(rangedBasicAttack, "targetSelector", targetSelector);
            SetObjectReference(rangedBasicAttack, "lockTargetController", lockTargetController);
            SetObjectReference(rangedBasicAttack, "sourceHealth", playerHealth);
            SetBool(rangedBasicAttack, "requireAimToFire", false);
            SetBool(rangedBasicAttack, "useAimAssist", true);
            SetBool(rangedBasicAttack, "disableAimAssistWithManualInput", false);
            SetBool(rangedBasicAttack, "cameraAimIgnoresNonTargetHits", true);
            SetFloat(rangedBasicAttack, "aimAssistDistance", BossTargetingDistance);
            SetFloat(rangedBasicAttack, "hipAimAssistAngleDegrees", 45f);
            SetFloat(rangedBasicAttack, "aimedAimAssistAngleDegrees", 45f);
            SetFloat(rangedBasicAttack, "aimAssistMaxTurnDegrees", 45f);

            BossBarrageLaneReviewCombatHudBinder combatHudBinder =
                FirstSceneComponent<BossBarrageLaneReviewCombatHudBinder>(scene);
            if (combatHudBinder != null)
            {
                SetObjectReference(combatHudBinder, "playerHealth", playerHealth);
                SetObjectReference(combatHudBinder, "bossHealth", bossHealth);
            }

            Physics.SyncTransforms();
            float before = bossHealth.CurrentHealth;
            int successfulHits = 0;
            for (int shotIndex = 0; shotIndex < shotsToFire; shotIndex++)
            {
                PrepareRangedFireProbe(rangedBasicAttack);
                LaneActionProjectile firedProjectile = null;
                void CaptureProjectile(LaneActionProjectile projectile)
                {
                    firedProjectile = projectile;
                }

                float beforeShot = bossHealth.CurrentHealth;
                rangedBasicAttack.RangedProjectileFired += CaptureProjectile;
                bool fired = rangedBasicAttack.TryFire();
                rangedBasicAttack.RangedProjectileFired -= CaptureProjectile;
                if (!fired || firedProjectile == null)
                {
                    throw new InvalidOperationException(
                        "Sustained player ranged fire stopped creating projectiles. " +
                        $"shot={shotIndex + 1} fired={fired} blocked='{rangedBasicAttack.LastUseBlockedReason}'.");
                }

                try
                {
                    firedProjectile.name = $"OlympusStation_SustainedRangedFireDamageProbe_{shotIndex + 1}";
                    for (int i = 0; i < 240 && bossHealth.CurrentHealth >= beforeShot; i++)
                    {
                        Physics.SyncTransforms();
                        firedProjectile.Tick(1f / 60f);
                    }

                    if (bossHealth.CurrentHealth < beforeShot
                        && firedProjectile.LastImpactResult == ProjectileImpactResult.AppliedDamage
                        && firedProjectile.LastImpactTargetHealth == bossHealth)
                    {
                        successfulHits++;
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            "Sustained player ranged fire missed boss HP. " +
                            $"shot={shotIndex + 1} before={beforeShot} after={bossHealth.CurrentHealth} " +
                            $"impact={firedProjectile.LastImpactResult} target={NameOf(firedProjectile.LastImpactTargetHealth)} " +
                            $"direction={firedProjectile.TravelDirection} projectilePosition={firedProjectile.transform.position}.");
                    }
                }
                finally
                {
                    if (firedProjectile != null)
                    {
                        UnityEngine.Object.DestroyImmediate(firedProjectile.gameObject);
                    }
                }
            }

            float after = bossHealth.CurrentHealth;
            float damageDelta = before - after;
            if (successfulHits < shotsToFire || damageDelta <= 100f)
            {
                throw new InvalidOperationException(
                    "Sustained player ranged fire did not reduce boss HP enough. " +
                    $"shots={shotsToFire} hits={successfulHits} before={before} after={after} delta={damageDelta}.");
            }

            hudPresenter.SetBossHealth(after, bossHealth.MaxHealth);
            float expectedFill = Mathf.Clamp01(after / Mathf.Max(1f, bossHealth.MaxHealth));
            float actualFill = hudPresenter.BossHealthFillAmount;
            if (actualFill >= 0.99f || Mathf.Abs(actualFill - expectedFill) > 0.005f)
            {
                throw new InvalidOperationException(
                    "Combat HUD boss HP fill did not follow boss health. " +
                    $"expectedFill={expectedFill} actualFill={actualFill} hp={after}/{bossHealth.MaxHealth}.");
            }

            Debug.Log(
                "Verified OlympusStation sustained player ranged fire boss HP and HUD. " +
                $"shots={shotsToFire} hits={successfulHits} before={before} after={after} " +
                $"delta={damageDelta} hudFill={actualFill:0.###}.");
        }

        private static void PrepareRangedFireProbe(PlayerRangedBasicAttackAction rangedBasicAttack)
        {
            SetPrivateField(rangedBasicAttack, "cinematicInputLocked", false);
            SetPrivateField(rangedBasicAttack, "nextFireTime", 0f);
            SetPrivateField(rangedBasicAttack, "isReloading", false);
            SetPrivateField(rangedBasicAttack, "reloadStartedByAimRelease", false);
            SetPrivateField(rangedBasicAttack, "reloadFinishTime", 0f);
            SetPrivateField(rangedBasicAttack, "ammoInitialized", true);
            SetPrivateField(rangedBasicAttack, "currentAmmo", Mathf.Max(1, rangedBasicAttack.MagazineSize));
            SetPrivateField(rangedBasicAttack, "aimInput", Vector2.zero);
            SetPrivateField(rangedBasicAttack, "hasCachedFirePreview", false);
            SetPrivateField(rangedBasicAttack, "firePreviewFrame", -1);
        }

        private static Component EnsureRuntimeBinder(
            Scene scene,
            CombatHealth playerHealth,
            CombatHealth bossHealth,
            PlayerCombatTargetSelector targetSelector,
            PlayerLockTargetController lockTargetController,
            PlayerRangedBasicAttackAction rangedBasicAttack,
            ActionFoundationTestEncounter encounter)
        {
            GameObject root = FindRoot(scene, RuntimeBinderRootName);
            if (root == null)
            {
                root = new GameObject(RuntimeBinderRootName);
                SceneManager.MoveGameObjectToScene(root, scene);
            }

            Type binderType = ResolveRuntimeBinderType();
            Component binder = root.GetComponent(binderType);
            if (binder == null)
            {
                binder = root.AddComponent(binderType);
            }

            SetObjectReference(binder, "playerHealth", playerHealth);
            SetObjectReference(binder, "bossHealth", bossHealth);
            SetObjectReference(binder, "targetSelector", targetSelector);
            SetObjectReference(binder, "lockTargetController", lockTargetController);
            SetObjectReference(binder, "rangedBasicAttackAction", rangedBasicAttack);
            SetObjectReference(binder, "encounter", encounter);
            EditorUtility.SetDirty(root);
            EditorUtility.SetDirty(binder);
            return binder;
        }

        private static Type ResolveRuntimeBinderType()
        {
            const string typeName =
                "DimensionBrawl.LevelDesign.OlympusStationCombatStageRuntimeBossTargetBinder";
            Type type = Type.GetType(typeName + ", DimensionBrawl.Runtime")
                ?? Type.GetType(typeName + ", Assembly-CSharp")
                ?? Type.GetType(typeName);
            if (type == null || !typeof(Component).IsAssignableFrom(type))
            {
                throw new InvalidOperationException($"Could not resolve runtime binder type {typeName}.");
            }

            return type;
        }

        private static CombatHealth ResolveBossHealth(Scene scene)
        {
            GameObject bossRoot = FindRoot(scene, BossProxyRootName);
            if (bossRoot != null)
            {
                CombatHealth health = bossRoot.GetComponent<CombatHealth>();
                if (health != null)
                {
                    return health;
                }
            }

            CombatHealth[] healths = CollectComponents<CombatHealth>(scene);
            for (int i = 0; i < healths.Length; i++)
            {
                if (healths[i] != null
                    && healths[i].name.Contains("Boss", StringComparison.Ordinal)
                    && healths[i].MaxHealth > 1000f)
                {
                    return healths[i];
                }
            }

            throw new InvalidOperationException($"Could not resolve boss CombatHealth in {ScenePath}.");
        }

        private static CombatHealth ResolvePlayerHealth(Scene scene)
        {
            CombatHealth[] healths = CollectComponents<CombatHealth>(scene);
            for (int i = 0; i < healths.Length; i++)
            {
                if (healths[i] != null && healths[i].GetComponent<PlayerCombatTargetSelector>() != null)
                {
                    return healths[i];
                }
            }

            throw new InvalidOperationException($"Could not resolve player CombatHealth in {ScenePath}.");
        }

        private static Collider ResolveActiveBossCollider(CombatHealth bossHealth)
        {
            Collider[] colliders = bossHealth.GetComponentsInChildren<Collider>(includeInactive: true);
            for (int i = 0; i < colliders.Length; i++)
            {
                Collider collider = colliders[i];
                if (collider != null && collider.enabled && collider.gameObject.activeInHierarchy)
                {
                    return collider;
                }
            }

            throw new InvalidOperationException(
                $"Could not find an active boss collider under {GetHierarchyPath(bossHealth.transform)}.");
        }

        private static T RequireSceneComponent<T>(Scene scene, string label)
            where T : Component
        {
            T component = FirstSceneComponent<T>(scene);
            if (component == null)
            {
                throw new InvalidOperationException($"Could not find {label} in {ScenePath}.");
            }

            return component;
        }

        private static T FirstSceneComponent<T>(Scene scene)
            where T : Component
        {
            T[] components = CollectComponents<T>(scene);
            return components.Length > 0 ? components[0] : null;
        }

        private static T[] CollectComponents<T>(Scene scene)
            where T : Component
        {
            var results = new System.Collections.Generic.List<T>();
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                results.AddRange(roots[i].GetComponentsInChildren<T>(includeInactive: true));
            }

            return results.ToArray();
        }

        private static GameObject FindRoot(Scene scene, string rootName)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                if (string.Equals(roots[i].name, rootName, StringComparison.Ordinal))
                {
                    return roots[i];
                }
            }

            return null;
        }

        private static void SetObjectReference(UnityEngine.Object target, string propertyName, UnityEngine.Object value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                throw new InvalidOperationException(
                    $"Missing serialized property {propertyName} on {target.name}.");
            }

            property.objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetFloat(UnityEngine.Object target, string propertyName, float value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                throw new InvalidOperationException(
                    $"Missing serialized property {propertyName} on {target.name}.");
            }

            property.floatValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetBool(UnityEngine.Object target, string propertyName, bool value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                throw new InvalidOperationException(
                    $"Missing serialized property {propertyName} on {target.name}.");
            }

            property.boolValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            if (target == null)
            {
                throw new InvalidOperationException($"Cannot set {fieldName} on null target.");
            }

            FieldInfo field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field == null)
            {
                throw new InvalidOperationException(
                    $"Missing field {fieldName} on {target.GetType().Name}.");
            }

            field.SetValue(target, value);
        }

        private static void SetObjectReferenceArray(
            UnityEngine.Object target,
            string propertyName,
            UnityEngine.Object[] values)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || !property.isArray)
            {
                throw new InvalidOperationException(
                    $"Missing serialized array property {propertyName} on {target.name}.");
            }

            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void ValidateObjectReference(
            UnityEngine.Object target,
            string propertyName,
            UnityEngine.Object expected)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            UnityEngine.Object actual = property != null ? property.objectReferenceValue : null;
            if (actual != expected)
            {
                throw new InvalidOperationException(
                    $"{target.name}.{propertyName} expected {NameOf(expected)} but was {NameOf(actual)}.");
            }
        }

        private static void ValidateArrayReference(
            UnityEngine.Object target,
            string propertyName,
            int index,
            UnityEngine.Object expected,
            int expectedSize)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || !property.isArray)
            {
                throw new InvalidOperationException(
                    $"Missing serialized array property {propertyName} on {target.name}.");
            }

            if (property.arraySize != expectedSize)
            {
                throw new InvalidOperationException(
                    $"{target.name}.{propertyName} expected size {expectedSize} but was {property.arraySize}.");
            }

            UnityEngine.Object actual = property.GetArrayElementAtIndex(index).objectReferenceValue;
            if (actual != expected)
            {
                throw new InvalidOperationException(
                    $"{target.name}.{propertyName}[{index}] expected {NameOf(expected)} but was {NameOf(actual)}.");
            }
        }

        private static string NameOf(UnityEngine.Object obj)
        {
            return obj != null ? obj.name : "null";
        }

        private static string GetHierarchyPath(Transform transform)
        {
            if (transform == null)
            {
                return "null";
            }

            string path = transform.name;
            Transform current = transform.parent;
            while (current != null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }

            return path;
        }
    }
}
#endif
