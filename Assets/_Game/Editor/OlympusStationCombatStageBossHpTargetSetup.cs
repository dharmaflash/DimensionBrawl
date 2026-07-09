#if UNITY_EDITOR
using System;
using System.IO;
using System.Reflection;
using DimensionBrawl.Combat;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Player;
using DimensionBrawl.Presentation;
using DimensionBrawl.Test;
using DimensionBrawl.UI;
using DimensionBrawl.UI.StageClear;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DimensionBrawl.Editor
{
    public static class OlympusStationCombatStageBossHpTargetSetup
    {
        private const string ScenePath = "Assets/_Game/Scenes/OlympusStationCombatStage.unity";
        private const string ClearUiScenePath = "Assets/_Game/Scenes/Experiments/UI_StageClearTest.unity";
        private const string BossProxyRootName = "BossBarrageLaneReview_BossProxy_NeedleLock";
        private const string RuntimeBinderRootName = "OlympusStation_BossHpTargetRuntimeBinder";
        private const float BossTargetingDistance = 80f;
        private static readonly Vector3 CombatAimFocusOffset = new Vector3(0.45f, 0.06f, 1.05f);

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
                VerifyLockTargetCameraAimAssist();
                VerifyDocxCombatPolish();
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

        public static void RunBatchVerifyLockTargetCameraAimAssist()
        {
            try
            {
                VerifyLockTargetCameraAimAssist();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        public static void RunBatchVerifyDocxCombatPolish()
        {
            try
            {
                VerifyDocxCombatPolish();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        public static void RunBatchVerifyStageClearAndQuickBalance()
        {
            try
            {
                VerifyStageClearAuthoredOverlayContract();
                for (int pass = 0; pass < 3; pass++)
                {
                    VerifySustainedPlayerRangedFireDamageAndHud();
                }

                VerifyLockTargetCameraAimAssist();
                Debug.Log("Verified OlympusStation stage clear contract and 3 quick balance passes.");
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

            ActionCameraController cameraController = FirstSceneComponent<ActionCameraController>(scene);
            if (cameraController != null)
            {
                SetVector3(cameraController, "aimFocusOffset", CombatAimFocusOffset);
                SetBool(cameraController, "aimAssistUsesYawTarget", true);
            }

            BossBarrageLaneReviewMobileHud mobileHud = FirstSceneComponent<BossBarrageLaneReviewMobileHud>(scene);
            if (mobileHud != null)
            {
                SetBool(mobileHud, "fireAimReticleUsesScreenCenter", false);
            }

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
            SetBool(rangedBasicAttack, "fallbackToSelectedTargetWhenCameraAimMisses", true);
            SetFloat(rangedBasicAttack, "selectedTargetFallbackDistance", BossTargetingDistance);
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
            SetBool(rangedBasicAttack, "fallbackToSelectedTargetWhenCameraAimMisses", true);
            SetFloat(rangedBasicAttack, "selectedTargetFallbackDistance", BossTargetingDistance);
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

        private static void VerifyLockTargetCameraAimAssist()
        {
            AssetDatabase.Refresh();
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            CombatHealth bossHealth = ResolveBossHealth(scene);
            CombatHealth playerHealth = ResolvePlayerHealth(scene);
            Collider bossCollider = ResolveActiveBossCollider(bossHealth);
            PlayerCombatModeController combatModeController =
                RequireSceneComponent<PlayerCombatModeController>(scene, "player combat mode controller");
            PlayerCombatTargetSelector targetSelector =
                RequireSceneComponent<PlayerCombatTargetSelector>(scene, "player target selector");
            PlayerLockTargetController lockTargetController =
                RequireSceneComponent<PlayerLockTargetController>(scene, "player lock target controller");
            PlayerRangedBasicAttackAction rangedBasicAttack =
                RequireSceneComponent<PlayerRangedBasicAttackAction>(scene, "player ranged basic attack");
            ActionCameraController cameraController =
                RequireSceneComponent<ActionCameraController>(scene, "action camera controller");

            combatModeController.SetRangedMode();
            playerHealth.ConfigureTeam(DamageTeam.Player);
            bossHealth.ConfigureTeam(DamageTeam.Enemy);

            SetObjectReference(targetSelector, "selfHealth", playerHealth);
            SetObjectReferenceArray(targetSelector, "targetCandidates", new UnityEngine.Object[] { bossHealth });
            SetFloat(targetSelector, "selectionRadius", BossTargetingDistance);
            SetFloat(targetSelector, "attackAimRadius", BossTargetingDistance);
            targetSelector.ConfigureTargetCandidates(new[] { bossHealth }, refreshNow: true);

            SetObjectReference(lockTargetController, "targetSelector", targetSelector);
            SetObjectReference(lockTargetController, "sourceHealth", playerHealth);
            SetObjectReference(lockTargetController, "cameraController", cameraController);
            Vector3 lockPoint = bossCollider.bounds.center;
            SetPrivateField(lockTargetController, "currentTargetHealth", bossHealth);
            SetPrivateField(lockTargetController, "currentTargetPoint", lockPoint);
            SetPrivateField(lockTargetController, "currentLockType", PlayerLockTargetController.LockTargetType.HardLock);
            SetPrivateField(lockTargetController, "currentStrength01", 1f);
            SetPrivateField(lockTargetController, "requestedHardLockTarget", bossHealth);

            SetObjectReference(rangedBasicAttack, "combatModeController", combatModeController);
            SetObjectReference(rangedBasicAttack, "targetSelector", targetSelector);
            SetObjectReference(rangedBasicAttack, "lockTargetController", lockTargetController);
            SetObjectReference(rangedBasicAttack, "sourceHealth", playerHealth);
            SetObjectReference(rangedBasicAttack, "cameraController", cameraController);
            SetBool(rangedBasicAttack, "driveCameraAimAssist", true);
            SetBool(rangedBasicAttack, "useAimAssist", true);
            SetBool(rangedBasicAttack, "fallbackToSelectedTargetWhenCameraAimMisses", true);
            SetFloat(rangedBasicAttack, "selectedTargetFallbackDistance", BossTargetingDistance);
            SetBool(rangedBasicAttack, "disableAimAssistWithManualInput", false);
            SetBool(rangedBasicAttack, "cameraAimIgnoresNonTargetHits", true);
            SetFloat(rangedBasicAttack, "cameraAimAssistStrengthScale", 1f);
            SetFloat(rangedBasicAttack, "cameraAimAssistMinStrength", 0.01f);
            SetFloat(rangedBasicAttack, "aimAssistDistance", BossTargetingDistance);
            SetFloat(rangedBasicAttack, "hipAimAssistAngleDegrees", 45f);
            SetFloat(rangedBasicAttack, "aimedAimAssistAngleDegrees", 45f);
            SetFloat(rangedBasicAttack, "aimAssistMaxTurnDegrees", 45f);
            PrepareRangedFireProbe(rangedBasicAttack);
            SetPrivateField(cameraController, "hasAimAssistYawTarget", false);
            SetPrivateField(cameraController, "requestedAimAssistStrength01", 0f);

            Physics.SyncTransforms();
            rangedBasicAttack.SetFireHeld(true);
            if (!rangedBasicAttack.TryGetAimPreviewDirection(out Vector3 previewDirection)
                || !rangedBasicAttack.HasAimAssistTarget
                || rangedBasicAttack.AimAssistTargetHealth != bossHealth)
            {
                throw new InvalidOperationException(
                    "Lock target aim preview did not resolve the OlympusStation boss. " +
                    $"hasTarget={rangedBasicAttack.HasAimAssistTarget} " +
                    $"target={NameOf(rangedBasicAttack.AimAssistTargetHealth)} direction={previewDirection}.");
            }

            bool aimAssistMayDriveCamera =
                GetPrivateField<bool>(rangedBasicAttack, "aimAssistMayDriveCamera");
            if (!aimAssistMayDriveCamera)
            {
                throw new InvalidOperationException(
                    "Lock target aim assist resolved, but camera drive remained disabled.");
            }

            InvokePrivateMethod(rangedBasicAttack, "UpdateCameraAimAssistIfNeeded");
            bool cameraRequestQueued =
                GetPrivateField<bool>(cameraController, "hasAimAssistYawTarget");
            float requestedStrength =
                GetPrivateField<float>(cameraController, "requestedAimAssistStrength01");
            if (!cameraRequestQueued || requestedStrength <= 0f)
            {
                throw new InvalidOperationException(
                    "Lock target aim assist did not request a camera yaw target. " +
                    $"queued={cameraRequestQueued} strength={requestedStrength:0.###}.");
            }

            rangedBasicAttack.SetFireHeld(false);
            Debug.Log(
                "Verified OlympusStation lock-target ranged aim can drive camera aim assist. " +
                $"target={GetHierarchyPath(bossHealth.transform)} strength={requestedStrength:0.###}.");
        }

        private static void VerifyDocxCombatPolish()
        {
            VerifyAimReticlePresentation();
            VerifyCombatInputRelease();
            VerifySerializedDocxPolishFlags();
        }

        private static void VerifyStageClearAuthoredOverlayContract()
        {
            AssetDatabase.Refresh();
            Scene clearScene = EditorSceneManager.OpenScene(ClearUiScenePath, OpenSceneMode.Single);
            UIStageClearTestPresenter presenter =
                RequireSceneComponent<UIStageClearTestPresenter>(clearScene, "authored stage clear presenter");
            RequireSerializedObjectReference(presenter, "retryButton");
            RequireSerializedObjectReference(presenter, "nextStageButton");

            if (!IsBuildSettingsSceneEnabled(ClearUiScenePath))
            {
                throw new InvalidOperationException($"{ClearUiScenePath} is not enabled in Build Settings.");
            }

            Type overlayType = typeof(OlympusStationStageClearOverlay);
            RequireMethod(overlayType, "LockCombatAfterClear", BindingFlags.Instance | BindingFlags.NonPublic);
            RequireMethod(overlayType, "StopHostileCombat", BindingFlags.Static | BindingFlags.NonPublic);
            RequireMethod(overlayType, "DisableCombatResultOverlays", BindingFlags.Static | BindingFlags.NonPublic);
            RequireMethod(overlayType, "DisableEncounterFailureHooks", BindingFlags.Static | BindingFlags.NonPublic);
            if (overlayType.GetMethod("CreateButton", BindingFlags.Static | BindingFlags.NonPublic) != null
                || overlayType.GetMethod("CreateImage", BindingFlags.Static | BindingFlags.NonPublic) != null)
            {
                throw new InvalidOperationException(
                    "OlympusStationStageClearOverlay still has runtime UI fallback creation methods.");
            }

            if (typeof(LobbySceneResolutionAdapter).GetMethod(
                    "ApplyNow",
                    BindingFlags.Instance | BindingFlags.Public) == null)
            {
                throw new InvalidOperationException("LobbySceneResolutionAdapter.ApplyNow is missing.");
            }

            FieldInfo mobileOutlineField = typeof(PlayerLockTargetVisualPresenter).GetField(
                "useBodyOutlineOnMobile",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (mobileOutlineField == null)
            {
                throw new InvalidOperationException(
                    "PlayerLockTargetVisualPresenter is missing the mobile body-outline guard.");
            }

            Debug.Log("Verified OlympusStation authored stage clear UI contract.");
        }

        private static void VerifyAimReticlePresentation()
        {
            AssetDatabase.Refresh();
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            BossBarrageLaneReviewMobileHud mobileHud =
                RequireSceneComponent<BossBarrageLaneReviewMobileHud>(scene, "mobile review HUD");
            ActionCameraController cameraController =
                RequireSceneComponent<ActionCameraController>(scene, "action camera controller");

            if (GetSerializedBool(mobileHud, "fireAimReticleUsesScreenCenter"))
            {
                throw new InvalidOperationException(
                    "Mobile HUD fire reticle is still pinned to raw screen center instead of ranged fire preview.");
            }

            Vector3 aimFocusOffset = GetSerializedVector3(cameraController, "aimFocusOffset");
            if ((aimFocusOffset - CombatAimFocusOffset).sqrMagnitude > 0.0001f)
            {
                throw new InvalidOperationException(
                    "Action camera aim focus offset is not aligned with the shoulder camera center line. " +
                    $"expected={CombatAimFocusOffset} actual={aimFocusOffset}.");
            }

            Debug.Log(
                "Verified OlympusStation aim reticle presentation uses ranged fire preview and centered aim focus. " +
                $"aimFocusOffset={aimFocusOffset}.");
        }

        private static void VerifyCombatInputRelease()
        {
            AssetDatabase.Refresh();
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            OlympusStationCombatStageRuntimeBossTargetBinder runtimeBinder =
                RequireSceneComponent<OlympusStationCombatStageRuntimeBossTargetBinder>(scene, "runtime boss target binder");
            PlayerMovementController movement =
                RequireSceneComponent<PlayerMovementController>(scene, "player movement controller");
            PlayerActionController actionController =
                RequireSceneComponent<PlayerActionController>(scene, "player action controller");
            PlayerCombatModeController combatModeController =
                RequireSceneComponent<PlayerCombatModeController>(scene, "player combat mode controller");
            PlayerRangedBasicAttackAction rangedBasicAttack =
                RequireSceneComponent<PlayerRangedBasicAttackAction>(scene, "player ranged basic attack");
            PlayerSkill1Action skill1Action =
                RequireSceneComponent<PlayerSkill1Action>(scene, "player skill1 action");
            PlayerSummonSlot1Action summonSlot1Action =
                RequireSceneComponent<PlayerSummonSlot1Action>(scene, "player summon slot1 action");
            PlayerSupportSummonSlotAction[] supportSummons =
                CollectComponents<PlayerSupportSummonSlotAction>(scene);

            movement.SetCinematicMoveInputSpeedScale(0f);
            movement.SetActionMoveInputSpeedScale(0f);
            actionController.SetCinematicInputLocked(true);
            combatModeController.SetCinematicInputLocked(true);
            rangedBasicAttack.SetCinematicInputLocked(true);
            skill1Action.SetCinematicInputLocked(true);
            summonSlot1Action.SetCinematicInputLocked(true);
            for (int i = 0; i < supportSummons.Length; i++)
            {
                supportSummons[i]?.SetCinematicInputLocked(true);
            }

            SetPrivateField(rangedBasicAttack, "externalAimPreviewHeld", true);
            SetPrivateField(rangedBasicAttack, "currentFireHeld", true);
            SetPrivateField(rangedBasicAttack, "mobileFireHeld", true);
            SetPrivateField(rangedBasicAttack, "aimInput", Vector2.one);

            runtimeBinder.ApplyBindings();

            if (movement.IsCinematicMoveInputLocked)
            {
                throw new InvalidOperationException("Runtime binder did not clear player movement cinematic lock.");
            }

            movement.SetMoveInput(Vector2.up);
            if (!movement.TryGetCurrentMoveDirection(out _))
            {
                throw new InvalidOperationException("Player movement still rejects shared move input after runtime binding.");
            }

            ValidatePrivateBoolFalse(actionController, "cinematicInputLocked");
            ValidatePrivateBoolFalse(combatModeController, "cinematicInputLocked");
            ValidatePrivateBoolFalse(rangedBasicAttack, "cinematicInputLocked");
            ValidatePrivateBoolFalse(rangedBasicAttack, "externalAimPreviewHeld");
            ValidatePrivateBoolFalse(rangedBasicAttack, "currentFireHeld");
            ValidatePrivateBoolFalse(rangedBasicAttack, "mobileFireHeld");
            if (GetPrivateField<Vector2>(rangedBasicAttack, "aimInput").sqrMagnitude > 0f)
            {
                throw new InvalidOperationException("Runtime binder did not clear ranged aim input.");
            }

            ValidatePrivateBoolFalse(skill1Action, "cinematicInputLocked");
            ValidatePrivateBoolFalse(summonSlot1Action, "cinematicInputLocked");
            for (int i = 0; i < supportSummons.Length; i++)
            {
                if (supportSummons[i] != null)
                {
                    ValidatePrivateBoolFalse(supportSummons[i], "cinematicInputLocked");
                }
            }

            Debug.Log(
                "Verified OlympusStation runtime binder clears tutorial/cinematic input locks for combat entry. " +
                $"supportSummons={supportSummons.Length}.");
        }

        private static void VerifySerializedDocxPolishFlags()
        {
            EnsureTextAbsent("Assets/_Game/Scenes/OlympusStationCombatStage.unity", "m_RenderPostProcessing: 0");
            EnsureTextAbsent("Assets/_Game/Scenes/OlympusStationCombatStage.unity", "fireAimReticleUsesScreenCenter: 1");
            EnsureTextAbsent("Assets/_Game/Scenes/OlympusStationCombatStage.unity", "aimFocusOffset: {x: 0.89");
            EnsureTextAbsent("Assets/_Game/Scenes/OlympusCorridorInvasionStage.unity", "m_RenderPostProcessing: 0");
            EnsureTextAbsent("Assets/_Game/Scenes/OlympusCorridorInvasionStage.unity", "fireAimReticleUsesScreenCenter: 1");
            EnsureTextAbsent("Assets/_Game/Scenes/OlympusCorridorInvasionStage.unity", "aimFocusOffset: {x: 0.89");
            EnsureTextAbsent("Assets/_Game/Scenes/OlympusCorridorInvasionStage.unity", "playDeathVfx: 0");
            EnsureTextAbsent(
                "Assets/_Game/Prefabs/Enemies/ActionFoundation/PF_Enemy_SciFiSoldier_EliteDeck.prefab",
                "playDeathVfx: 0");
            EnsureTextAbsent(
                "Assets/_Game/Prefabs/Enemies/ActionFoundation/PF_Enemy_SciFiSoldier_GeneralDeck.prefab",
                "playDeathVfx: 0");
            EnsureTextAbsent(
                "Assets/_Game/Prefabs/Enemies/ActionFoundation/PF_Enemy_SciFiSoldier_Melee_ClosePunish.prefab",
                "playDeathVfx: 0");
            Debug.Log("Verified docx combat polish serialized flags: postprocessing enabled and death VFX enabled.");
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

        private static void SetVector3(UnityEngine.Object target, string propertyName, Vector3 value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                throw new InvalidOperationException(
                    $"Missing serialized property {propertyName} on {target.name}.");
            }

            property.vector3Value = value;
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

        private static T GetPrivateField<T>(object target, string fieldName)
        {
            if (target == null)
            {
                throw new InvalidOperationException($"Cannot read {fieldName} on null target.");
            }

            FieldInfo field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field == null)
            {
                throw new InvalidOperationException(
                    $"Missing field {fieldName} on {target.GetType().Name}.");
            }

            return (T)field.GetValue(target);
        }

        private static void InvokePrivateMethod(object target, string methodName)
        {
            if (target == null)
            {
                throw new InvalidOperationException($"Cannot invoke {methodName} on null target.");
            }

            MethodInfo method = target.GetType().GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (method == null)
            {
                throw new InvalidOperationException(
                    $"Missing method {methodName} on {target.GetType().Name}.");
            }

            method.Invoke(target, Array.Empty<object>());
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

        private static void ValidatePrivateBoolFalse(object target, string fieldName)
        {
            if (GetPrivateField<bool>(target, fieldName))
            {
                throw new InvalidOperationException(
                    $"{target.GetType().Name}.{fieldName} stayed true after runtime binding.");
            }
        }

        private static bool GetSerializedBool(UnityEngine.Object target, string propertyName)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                throw new InvalidOperationException(
                    $"Missing serialized property {propertyName} on {target.name}.");
            }

            return property.boolValue;
        }

        private static void RequireSerializedObjectReference(UnityEngine.Object target, string propertyName)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || property.objectReferenceValue == null)
            {
                throw new InvalidOperationException(
                    $"{target.name}.{propertyName} must reference an authored scene object.");
            }
        }

        private static bool IsBuildSettingsSceneEnabled(string scenePath)
        {
            EditorBuildSettingsScene[] scenes =
                EditorBuildSettings.scenes ?? Array.Empty<EditorBuildSettingsScene>();
            for (int i = 0; i < scenes.Length; i++)
            {
                EditorBuildSettingsScene scene = scenes[i];
                if (scene.enabled && string.Equals(scene.path, scenePath, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static void RequireMethod(Type type, string methodName, BindingFlags flags)
        {
            if (type.GetMethod(methodName, flags) == null)
            {
                throw new InvalidOperationException($"{type.Name}.{methodName} is missing.");
            }
        }

        private static Vector3 GetSerializedVector3(UnityEngine.Object target, string propertyName)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                throw new InvalidOperationException(
                    $"Missing serialized property {propertyName} on {target.name}.");
            }

            return property.vector3Value;
        }

        private static void EnsureTextAbsent(string assetPath, string forbiddenText)
        {
            string fullPath = Path.GetFullPath(assetPath);
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"Could not find asset for serialized verification: {assetPath}", fullPath);
            }

            string content = File.ReadAllText(fullPath);
            if (content.Contains(forbiddenText, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"{assetPath} still contains '{forbiddenText}'.");
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
