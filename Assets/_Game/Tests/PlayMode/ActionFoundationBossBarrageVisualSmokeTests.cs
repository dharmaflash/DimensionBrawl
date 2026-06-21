using System.Collections;
using System.IO;
using DimensionBrawl.Combat;
using DimensionBrawl.Player;
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

            Camera camera = Camera.main != null
                ? Camera.main
                : Object.FindFirstObjectByType<Camera>();
            Assert.IsNotNull(camera, "Boss barrage visual smoke needs a renderable gameplay camera.");

            BossBasicFireEmitter bossBasicFire = Object.FindFirstObjectByType<BossBasicFireEmitter>();
            BossBarrageEmitter bossBarrage = Object.FindFirstObjectByType<BossBarrageEmitter>();
            BossSummonPressureAction bossSummonPressure = Object.FindFirstObjectByType<BossSummonPressureAction>();
            SummonEnergyLadder energyLadder = Object.FindFirstObjectByType<SummonEnergyLadder>();
            PlayerSummonSlot1Action summonSlot1 = Object.FindFirstObjectByType<PlayerSummonSlot1Action>();

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
                    + (bossSummonPressure != null ? bossSummonPressure.ActiveSummonActorCount : 0),
                0,
                "At least one summon/state visual should still be visible when the frame is captured.");

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
