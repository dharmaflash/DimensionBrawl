using System.Collections;
using System.IO;
using DimensionBrawl.Combat;
using DimensionBrawl.Player;
using DimensionBrawl.Presentation;
using DimensionBrawl.Test;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace DimensionBrawl.Tests
{
    public sealed class ActionFoundationBossBarrageVisualSmokeTests
    {
        private const string ScenePath = "Assets/_Game/Scenes/ActionFoundationBossBarrageLaneReview.unity";
        private const string BossRootName = "BossBarrageLaneReview_BossProxy_NeedleLock";
        private const string CloseThreatRootName = "BossBarrageLaneReview_CloseThreat_ClosePunish";
        private const string PocketOwnerRootName = "BossBarrageLaneReview_PocketOwner";
        private const string HudRootName = "BossBarrageLaneReview_DebugHud";
        private const int CaptureWidth = 960;
        private const int CaptureHeight = 540;

        [UnitySetUp]
        public IEnumerator LoadBossBarrageLaneReviewScene()
        {
            Time.timeScale = 1f;
            EditorSceneManager.LoadSceneInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator ResetTimeScale()
        {
            Time.timeScale = 1f;
            yield return null;
        }

        [UnityTest]
        public IEnumerator ReviewSceneRendersReadableCombatVfxFrame()
        {
            yield return null;

            Camera camera = RequireGameplayCamera();

            BossBasicFireEmitter bossBasicFire = Object.FindFirstObjectByType<BossBasicFireEmitter>();
            BossBarrageEmitter bossBarrage = Object.FindFirstObjectByType<BossBarrageEmitter>();
            BossSummonPressureAction bossSummonPressure = Object.FindFirstObjectByType<BossSummonPressureAction>();
            SummonEnergyLadder energyLadder = Object.FindFirstObjectByType<SummonEnergyLadder>();
            PlayerSummonSlot1Action summonSlot1 = Object.FindFirstObjectByType<PlayerSummonSlot1Action>();
            PlayerSupportSummonSlotAction summonSlot2 = RequireSupportSummonAction("SummonSlot2");
            PlayerSupportSummonSlotAction summonSlot3 = RequireSupportSummonAction("SummonSlot3");

            Assert.IsNotNull(bossBasicFire, "Boss basic fire should be present for regular projectile pressure.");
            Assert.IsNotNull(bossBarrage, "Boss barrage emitter should be present for pattern projectile pressure.");
            Assert.IsNotNull(energyLadder, "Summon energy should be present before forcing a visual summon read.");
            Assert.IsNotNull(summonSlot1, "SummonSlot1 should be present before forcing a visual summon read.");

            int basicProjectiles = bossBasicFire.FireVolley();
            Assert.Greater(basicProjectiles, 0, "Regular boss fire must spawn visible projectile actors.");

            bossBarrage.BeginWindup();
            int barrageProjectiles = bossBarrage.FirePendingWave();
            Assert.Greater(barrageProjectiles, 0, "Committed boss barrage must spawn visible projectile actors.");

            energyLadder.GrantCurrentTierEnergy(100f);
            Assert.IsTrue(summonSlot1.TryUseSummonSlot1(), "SummonSlot1 should spend LV1 energy for a visible actor/state read.");
            energyLadder.GrantCurrentTierEnergy(200f);
            Assert.IsTrue(summonSlot2.TryUseSummon(), "SummonSlot2 should spend LV2 energy for a visible marksman actor/volley read.");
            energyLadder.GrantCurrentTierEnergy(300f);
            Assert.IsTrue(summonSlot3.TryUseSummon(), "SummonSlot3 should spend LV3 energy for a visible vanguard actor/screen read.");

            if (bossSummonPressure != null)
            {
                Assert.IsTrue(
                    bossSummonPressure.TryReleasePressureSummon(2),
                    "Boss pressure summon should be releasable for a visual clash/state read.");
            }

            yield return new WaitForSeconds(0.25f);

            Assert.Greater(
                bossBasicFire.ActiveProjectileCount + bossBarrage.ActiveProjectileCount,
                0,
                "At least one boss projectile should still be visible when the frame is captured.");
            Assert.Greater(
                summonSlot1.ActiveCueCount + summonSlot1.ActiveSummonActorCount
                    + summonSlot2.ActiveSummonActorCount + summonSlot2.ActiveProjectileCount
                    + summonSlot3.ActiveSummonActorCount + summonSlot3.ActiveProjectileCount
                    + (bossSummonPressure != null ? bossSummonPressure.ActiveSummonActorCount : 0),
                0,
                "At least one player or boss summon/state visual should still be visible when the frame is captured.");
            Assert.Greater(
                summonSlot2.ActiveSummonActorCount + summonSlot2.ActiveProjectileCount,
                0,
                "The smoke frame should include the promoted SummonSlot2 marksman actor or volley, not only the shield slot.");
            Assert.Greater(
                summonSlot3.ActiveSummonActorCount + summonSlot3.ActiveProjectileCount,
                0,
                "The smoke frame should include the promoted SummonSlot3 vanguard actor or volley, not only the shield slot.");

            string capturePath = Path.GetFullPath(Path.Combine(
                Application.dataPath,
                "..",
                "Logs",
                "boss_barrage_visual_smoke.png"));
            Texture2D frame = CaptureCamera(camera, capturePath);
            try
            {
                FrameColorStats stats = AnalyzeFrame(frame);
                Assert.Greater(stats.VisiblePixelCount, frame.width * frame.height * 0.55f);
                Assert.Greater(stats.SaturatedPixelCount, frame.width * frame.height * 0.035f);
                Assert.Greater(stats.WarmProjectilePixelCount, 180);
                Assert.Greater(stats.CyanOrGreenStatePixelCount, 420);
                Assert.Greater(stats.MagentaStatePixelCount, 160);
                Assert.Less(stats.NearWhitePixelCount, frame.width * frame.height * 0.34f);
            }
            finally
            {
                Object.Destroy(frame);
            }
        }

        [UnityTest]
        public IEnumerator ReviewSceneRendersReadablePocketClearResultFrame()
        {
            yield return null;

            Camera camera = RequireGameplayCamera();
            PlayerMovementController player = RequireObject<PlayerMovementController>();
            CombatHealth playerHealth = RequireComponent<CombatHealth>(player.gameObject, "player health");
            SummonEnergyLadder energyLadder = RequireComponent<SummonEnergyLadder>(player.gameObject, "summon energy ladder");
            PlayerSummonSlot1Action summonSlot1 = RequireComponent<PlayerSummonSlot1Action>(player.gameObject, "SummonSlot1 action");
            PlayerSkill1Action skill1Action = RequireComponent<PlayerSkill1Action>(player.gameObject, "Skill1 action");
            GameObject bossRoot = RequireRoot(BossRootName);
            CombatHealth bossHealth = RequireComponent<CombatHealth>(bossRoot, "boss health");
            Collider bossHitCollider = RequireCombatHitCollider(bossRoot, bossHealth, "boss proxy");
            BossBarrageEmitter bossBarrage = RequireComponent<BossBarrageEmitter>(bossRoot, "boss barrage emitter");
            CombatHealth closeThreatHealth =
                RequireComponent<CombatHealth>(RequireRoot(CloseThreatRootName), "close threat health");
            BossBarragePocketReviewOwner pocketOwner =
                RequireComponent<BossBarragePocketReviewOwner>(RequireRoot(PocketOwnerRootName), "pocket review owner");
            BossBarragePocketVfxCueBridge pocketVfxCueBridge =
                RequireComponent<BossBarragePocketVfxCueBridge>(RequireRoot(PocketOwnerRootName), "pocket VFX cue bridge");
            ActionScreenCuePresenter screenCuePresenter =
                RequireComponent<ActionScreenCuePresenter>(RequireRoot(HudRootName), "screen cue presenter");

            energyLadder.GrantCurrentTierEnergy(100f);
            Assert.IsTrue(summonSlot1.TryUseSummonSlot1(), "SummonSlot1 should be usable to create the result flow.");
            closeThreatHealth.TryApplyDamage(new DamageInfo(
                playerHealth,
                DamageTeam.Player,
                closeThreatHealth.MaxHealth + 10f,
                closeThreatHealth.transform.position,
                Vector3.forward,
                0f));
            yield return null;

            SummonPressureScreen activeScreen = RequireActiveAllyPressureScreen();
            Assert.IsTrue(bossBarrage.BeginWindup());
            Assert.Greater(bossBarrage.FirePendingWave(), 0);
            BossBarrageProjectile bossProjectile = RequireActiveBossProjectile();
            Assert.IsTrue(activeScreen.TryIntercept(bossProjectile));
            pocketOwner.Tick(0f);

            Assert.IsTrue(skill1Action.TryUseSkill1());
            LaneActionProjectile followupProjectile = RequireActivePlayerSkillProjectile();
            Assert.IsTrue(followupProjectile.TryApplyImpact(bossHitCollider, followupProjectile.transform.position));
            pocketOwner.Tick(0f);

            int resultCueCountBeforeClear = screenCuePresenter.ResultCueRequestCount;
            int worldCueCountBeforeClear = pocketVfxCueBridge.PocketClearCueRequestCount;
            pocketOwner.Tick(0.77f);
            yield return null;

            Assert.IsTrue(pocketOwner.IsCleared, "The clear result frame should be reached through the authored summon-follow-up flow.");
            Assert.AreEqual(resultCueCountBeforeClear + 1, screenCuePresenter.ResultCueRequestCount);
            Assert.AreEqual("Pocket.Cleared", screenCuePresenter.LastCueId);
            Assert.IsTrue(screenCuePresenter.HasActiveCue);
            Assert.AreEqual(worldCueCountBeforeClear + 1, pocketVfxCueBridge.PocketClearCueRequestCount);

            string capturePath = Path.GetFullPath(Path.Combine(
                Application.dataPath,
                "..",
                "Logs",
                "boss_barrage_visual_smoke_clear.png"));
            CaptureAndAssertReadableResultFrame(camera, capturePath);
        }

        [UnityTest]
        public IEnumerator ReviewSceneRendersReadablePocketFailResultFrame()
        {
            yield return null;

            Camera camera = RequireGameplayCamera();
            PlayerMovementController player = RequireObject<PlayerMovementController>();
            CombatHealth playerHealth = RequireComponent<CombatHealth>(player.gameObject, "player health");
            BossBarragePocketReviewOwner pocketOwner =
                RequireComponent<BossBarragePocketReviewOwner>(RequireRoot(PocketOwnerRootName), "pocket review owner");
            BossBarragePocketVfxCueBridge pocketVfxCueBridge =
                RequireComponent<BossBarragePocketVfxCueBridge>(RequireRoot(PocketOwnerRootName), "pocket VFX cue bridge");
            ActionScreenCuePresenter screenCuePresenter =
                RequireComponent<ActionScreenCuePresenter>(RequireRoot(HudRootName), "screen cue presenter");

            int resultCueCountBeforeFail = screenCuePresenter.ResultCueRequestCount;
            int worldCueCountBeforeFail = pocketVfxCueBridge.PocketFailCueRequestCount;
            int accentCueCountBeforeFail = pocketVfxCueBridge.PocketFailAccentCueRequestCount;
            playerHealth.TryApplyDamage(new DamageInfo(
                null,
                DamageTeam.Enemy,
                playerHealth.MaxHealth + 10f,
                player.transform.position,
                Vector3.back,
                0f));
            yield return null;

            Assert.IsTrue(pocketOwner.IsFailed, "The fail result frame should be reached when the player is defeated.");
            Assert.AreEqual(resultCueCountBeforeFail + 1, screenCuePresenter.ResultCueRequestCount);
            Assert.AreEqual("Pocket.Failed", screenCuePresenter.LastCueId);
            Assert.IsTrue(screenCuePresenter.HasActiveCue);
            Assert.AreEqual(worldCueCountBeforeFail + 1, pocketVfxCueBridge.PocketFailCueRequestCount);
            Assert.AreEqual(accentCueCountBeforeFail + 1, pocketVfxCueBridge.PocketFailAccentCueRequestCount);

            string capturePath = Path.GetFullPath(Path.Combine(
                Application.dataPath,
                "..",
                "Logs",
                "boss_barrage_visual_smoke_fail.png"));
            CaptureAndAssertReadableResultFrame(camera, capturePath);
        }

        private static Texture2D CaptureCamera(Camera camera, string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            RenderTexture previousTargetTexture = camera.targetTexture;
            RenderTexture previousActive = RenderTexture.active;
            RenderTexture renderTexture = new RenderTexture(CaptureWidth, CaptureHeight, 24, RenderTextureFormat.ARGB32);
            Texture2D texture = new Texture2D(CaptureWidth, CaptureHeight, TextureFormat.RGBA32, false);
            try
            {
                camera.targetTexture = renderTexture;
                RenderTexture.active = renderTexture;
                camera.Render();
                texture.ReadPixels(new Rect(0, 0, CaptureWidth, CaptureHeight), 0, 0);
                texture.Apply(updateMipmaps: false, makeNoLongerReadable: false);
                File.WriteAllBytes(path, texture.EncodeToPNG());
                return texture;
            }
            finally
            {
                camera.targetTexture = previousTargetTexture;
                RenderTexture.active = previousActive;
                Object.Destroy(renderTexture);
            }
        }

        private static void CaptureAndAssertReadableResultFrame(Camera camera, string capturePath)
        {
            Texture2D frame = CaptureCamera(camera, capturePath);
            try
            {
                FrameColorStats stats = AnalyzeFrame(frame);
                Assert.Greater(stats.VisiblePixelCount, frame.width * frame.height * 0.55f);
                Assert.Greater(stats.SaturatedPixelCount, frame.width * frame.height * 0.025f);
                Assert.Less(stats.NearWhitePixelCount, frame.width * frame.height * 0.38f);
            }
            finally
            {
                Object.Destroy(frame);
            }
        }

        private static Camera RequireGameplayCamera()
        {
            Camera camera = Camera.main != null
                ? Camera.main
                : Object.FindFirstObjectByType<Camera>();
            Assert.IsNotNull(camera, "Boss barrage visual smoke needs a renderable gameplay camera.");
            return camera;
        }

        private static GameObject RequireRoot(string rootName)
        {
            GameObject root = GameObject.Find(rootName);
            Assert.IsNotNull(root, $"Scene root {rootName} is missing.");
            return root;
        }

        private static T RequireObject<T>() where T : Component
        {
            T component = Object.FindFirstObjectByType<T>();
            Assert.IsNotNull(component, $"Scene is missing {typeof(T).Name}.");
            return component;
        }

        private static T RequireComponent<T>(GameObject root, string label) where T : Component
        {
            T component = root.GetComponent<T>();
            Assert.IsNotNull(component, $"{label} is missing {typeof(T).Name}.");
            return component;
        }

        private static PlayerSupportSummonSlotAction RequireSupportSummonAction(string slotActionName)
        {
            PlayerSupportSummonSlotAction[] actions = Object.FindObjectsByType<PlayerSupportSummonSlotAction>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
            for (int i = 0; i < actions.Length; i++)
            {
                if (actions[i] != null && actions[i].SlotActionName == slotActionName)
                {
                    return actions[i];
                }
            }

            Assert.Fail($"Scene is missing {slotActionName} support summon action.");
            return null;
        }

        private static Collider RequireCombatHitCollider(GameObject root, CombatHealth expectedHealth, string label)
        {
            Collider[] colliders = root.GetComponentsInChildren<Collider>(includeInactive: true);
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null && colliders[i].GetComponentInParent<CombatHealth>() == expectedHealth)
                {
                    return colliders[i];
                }
            }

            Assert.Fail($"{label} should expose at least one child collider under its CombatHealth root.");
            return null;
        }

        private static SummonPressureScreen RequireActiveAllyPressureScreen()
        {
            SummonPressureScreen[] pressureScreens = Object.FindObjectsByType<SummonPressureScreen>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
            for (int i = 0; i < pressureScreens.Length; i++)
            {
                if (pressureScreens[i].IsActive && pressureScreens[i].OwnerTeam == DamageTeam.AllySummon)
                {
                    return pressureScreens[i];
                }
            }

            Assert.Fail("Expected an active AllySummon pressure screen.");
            return null;
        }

        private static BossBarrageProjectile RequireActiveBossProjectile()
        {
            BossBarrageProjectile[] bossProjectiles = Object.FindObjectsByType<BossBarrageProjectile>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
            for (int i = 0; i < bossProjectiles.Length; i++)
            {
                if (bossProjectiles[i].IsActive && bossProjectiles[i].SourceTeam == DamageTeam.Enemy)
                {
                    return bossProjectiles[i];
                }
            }

            Assert.Fail("Expected an active enemy boss barrage projectile.");
            return null;
        }

        private static LaneActionProjectile RequireActivePlayerSkillProjectile()
        {
            LaneActionProjectile[] projectiles = Object.FindObjectsByType<LaneActionProjectile>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
            for (int i = 0; i < projectiles.Length; i++)
            {
                if (projectiles[i].IsActive && projectiles[i].SourceTeam == DamageTeam.Player)
                {
                    return projectiles[i];
                }
            }

            Assert.Fail("Expected an active Player Skill1 projectile.");
            return null;
        }

        private static FrameColorStats AnalyzeFrame(Texture2D frame)
        {
            Color32[] pixels = frame.GetPixels32();
            FrameColorStats stats = default;
            for (int i = 0; i < pixels.Length; i++)
            {
                Color32 pixel = pixels[i];
                int max = Mathf.Max(pixel.r, Mathf.Max(pixel.g, pixel.b));
                int min = Mathf.Min(pixel.r, Mathf.Min(pixel.g, pixel.b));
                if (max > 18)
                {
                    stats.VisiblePixelCount++;
                }

                if (max - min > 55 && max > 92)
                {
                    stats.SaturatedPixelCount++;
                }

                if (pixel.r > 150 && pixel.g > 100 && pixel.b < 95)
                {
                    stats.WarmProjectilePixelCount++;
                }

                if (pixel.g > 105 && pixel.b > 90 && pixel.r < 115)
                {
                    stats.CyanOrGreenStatePixelCount++;
                }

                if (pixel.r > 130 && pixel.b > 115 && pixel.g < 120)
                {
                    stats.MagentaStatePixelCount++;
                }

                if (pixel.r > 236 && pixel.g > 236 && pixel.b > 236)
                {
                    stats.NearWhitePixelCount++;
                }
            }

            return stats;
        }

        private struct FrameColorStats
        {
            public int VisiblePixelCount;
            public int SaturatedPixelCount;
            public int WarmProjectilePixelCount;
            public int CyanOrGreenStatePixelCount;
            public int MagentaStatePixelCount;
            public int NearWhitePixelCount;
        }
    }
}
