using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using DimensionBrawl.Combat;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Presentation;
using DimensionBrawl.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace DimensionBrawl.Tests
{
    public sealed class CombatSessionOverlayPlayModeTests
    {
        private const string CombatHudPrefabPath = "Assets/_Game/UI/CombatHud/PF_UI_CombatHud.prefab";

        [Test]
        public void CombatHudPrefabOwnsOnePauseAndFailureSurface()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CombatHudPrefabPath);
            Assert.That(prefab, Is.Not.Null);
            GameObject instance = UnityEngine.Object.Instantiate(prefab);
            float originalTimeScale = Time.timeScale;

            try
            {
                ICombatSessionOverlay surface = FindSingleSurface(instance);
                LayoutGroup[] layoutGroups = instance.GetComponentsInChildren<LayoutGroup>(includeInactive: true);
                Assert.That(layoutGroups, Is.Not.Empty);
                Assert.That(Array.TrueForAll(layoutGroups, group => !group.enabled), Is.True);
                Time.timeScale = 0.75f;

                surface.ShowPause();
                Assert.That(surface.Mode, Is.EqualTo(CombatSessionOverlayMode.Pause));
                Assert.That(surface.IsVisible, Is.True);
                Assert.That(Array.TrueForAll(layoutGroups, group => group.enabled), Is.True);
                Assert.That(Time.timeScale, Is.Zero);

                surface.ShowSettings();
                Assert.That(surface.Mode, Is.EqualTo(CombatSessionOverlayMode.Settings));
                Assert.That(Time.timeScale, Is.Zero);

                surface.Resume();
                Assert.That(surface.Mode, Is.EqualTo(CombatSessionOverlayMode.Hidden));
                Assert.That(surface.IsVisible, Is.False);
                Assert.That(Array.TrueForAll(layoutGroups, group => !group.enabled), Is.True);
                Assert.That(Time.timeScale, Is.EqualTo(0.75f).Within(0.0001f));

                surface.ShowFailure();
                Assert.That(surface.Mode, Is.EqualTo(CombatSessionOverlayMode.Failure));
                Assert.That(surface.IsVisible, Is.True);
                Assert.That(Time.timeScale, Is.EqualTo(0.75f).Within(0.0001f));

                surface.DismissForStageClear();
                Assert.That(surface.Mode, Is.EqualTo(CombatSessionOverlayMode.Hidden));
                Assert.That(surface.IsVisible, Is.False);
                Assert.That(Array.TrueForAll(layoutGroups, group => !group.enabled), Is.True);
            }
            finally
            {
                Time.timeScale = originalTimeScale;
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }

        [UnityTest]
        public IEnumerator AndroidBackSettingsCloseRejectsTransientTimeWarpAsResumeBaseline()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CombatHudPrefabPath);
            Assert.That(prefab, Is.Not.Null);
            GameObject instance = UnityEngine.Object.Instantiate(prefab);
            float originalTimeScale = Time.timeScale;

            try
            {
                ICombatSessionOverlay surface = FindSingleSurface(instance);
                bool? lastInputBlock = null;
                surface.CombatInputBlockChanged += blocked => lastInputBlock = blocked;
                // PerfectDodgeTimeWarp's global hit stop can already own this scale before
                // Android Back opens settings, then decline to restore while the menu owns zero.
                Time.timeScale = 0.08f;

                surface.ShowSettings();
                Assert.That(surface.Mode, Is.EqualTo(CombatSessionOverlayMode.Settings));
                Assert.That(Time.timeScale, Is.Zero);
                Assert.That(lastInputBlock, Is.True);

                yield return null;
                Assert.That(Time.timeScale, Is.Zero);

                surface.Resume();
                Assert.That(surface.Mode, Is.EqualTo(CombatSessionOverlayMode.Hidden));
                Assert.That(Time.timeScale, Is.EqualTo(1f).Within(0.0001f));
                Assert.That(lastInputBlock, Is.False);
            }
            finally
            {
                Time.timeScale = originalTimeScale;
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }

        [UnityTest]
        public IEnumerator PauseTracksStableRestoreWrittenWhileMenuOwnsTimeScale()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CombatHudPrefabPath);
            Assert.That(prefab, Is.Not.Null);
            GameObject instance = UnityEngine.Object.Instantiate(prefab);
            float originalTimeScale = Time.timeScale;

            try
            {
                ICombatSessionOverlay surface = FindSingleSurface(instance);
                Time.timeScale = 0.18f;
                surface.ShowPause();
                Assert.That(Time.timeScale, Is.Zero);

                // CombatHitFeedback restores its pre-hit-stop scale even while this unscaled
                // menu is visible. The finite pause guard captures the stable value before reclaiming zero.
                Time.timeScale = 1f;
                yield return null;
                Assert.That(Time.timeScale, Is.Zero);

                surface.Resume();
                Assert.That(Time.timeScale, Is.EqualTo(1f).Within(0.0001f));
            }
            finally
            {
                Time.timeScale = originalTimeScale;
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }

        [UnityTest]
        public IEnumerator AndroidBackDoesNotReleaseAnExistingHardPauseOwner()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CombatHudPrefabPath);
            Assert.That(prefab, Is.Not.Null);
            GameObject instance = UnityEngine.Object.Instantiate(prefab);
            float originalTimeScale = Time.timeScale;

            try
            {
                ICombatSessionOverlay surface = FindSingleSurface(instance);
                Time.timeScale = 0f;

                surface.ShowSettings();
                Assert.That(surface.Mode, Is.EqualTo(CombatSessionOverlayMode.Settings));

                // A late presentation writer may try to restore its own pre-hit-stop scale while
                // the entry/stage-clear surface still owns the original hard pause. The menu must
                // reclaim zero without adopting that transient restore as its resume baseline.
                Time.timeScale = 1f;
                yield return null;
                Assert.That(Time.timeScale, Is.Zero);

                surface.Resume();

                Assert.That(surface.Mode, Is.EqualTo(CombatSessionOverlayMode.Hidden));
                Assert.That(
                    Time.timeScale,
                    Is.Zero,
                    "Android Back must not release a SceneEntry/StageClear hard-pause lease owned by another surface.");
            }
            finally
            {
                Time.timeScale = originalTimeScale;
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }

        [Test]
        public void StationResultPresenterRoutesPlayerFailureToProductSurfaceOnce()
        {
            GameObject resultRoot = new("StationResultRoot");
            GameObject playerObject = new("StationResultPlayer");
            GameObject enemyObject = new("StationResultEnemy");
            resultRoot.SetActive(false);

            try
            {
                CombatHealth playerHealth = CreateHealth(playerObject, DamageTeam.Player);
                CombatHealth enemyHealth = CreateHealth(enemyObject, DamageTeam.Enemy);
                CombatEncounterController encounter = resultRoot.AddComponent<CombatEncounterController>();
                encounter.ConfigureCombatants(playerHealth, enemyHealth);
                OlympusStageClearOverlay clearOverlay = resultRoot.AddComponent<OlympusStageClearOverlay>();
                CombatSessionOverlayProbe resultSurface = resultRoot.AddComponent<CombatSessionOverlayProbe>();
                OlympusStationCombatResultPresenter presenter =
                    resultRoot.AddComponent<OlympusStationCombatResultPresenter>();
                SetField(presenter, "encounter", encounter);
                SetField(presenter, "stageClearOverlay", clearOverlay);
                SetField(presenter, "resultSurfaceBehaviour", resultSurface);

                resultRoot.SetActive(true);
                Assert.That(ApplyLethalDamage(playerHealth, DamageTeam.Enemy), Is.True);
                Assert.That(resultSurface.FailureCount, Is.EqualTo(1));
                Assert.That(resultSurface.Mode, Is.EqualTo(CombatSessionOverlayMode.Failure));

                Assert.That(ApplyLethalDamage(enemyHealth, DamageTeam.Player), Is.True);
                Assert.That(resultSurface.FailureCount, Is.EqualTo(1));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(resultRoot);
                UnityEngine.Object.DestroyImmediate(enemyObject);
                UnityEngine.Object.DestroyImmediate(playerObject);
            }
        }

        [UnityTest]
        public IEnumerator CombatHudOverlayKeepsPauseAndFailureInsideMobileLandscapeLayout()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CombatHudPrefabPath);
            Assert.That(prefab, Is.Not.Null);

            var cameraRoot = new GameObject("Combat Session Overlay Visual Camera", typeof(Camera));
            Camera captureCamera = cameraRoot.GetComponent<Camera>();
            captureCamera.clearFlags = CameraClearFlags.SolidColor;
            captureCamera.backgroundColor = new Color(0.015f, 0.02f, 0.028f, 1f);
            captureCamera.cullingMask = 0;
            if (UnityEngine.Object.FindFirstObjectByType<AudioListener>() == null)
            {
                cameraRoot.AddComponent<AudioListener>();
            }

            var canvasRoot = new GameObject(
                "Combat Session Overlay Visual Canvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            Canvas canvas = canvasRoot.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;
            CanvasScaler scaler = canvasRoot.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(2560f, 1440f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1f;

            GameObject instance = UnityEngine.Object.Instantiate(prefab, canvasRoot.transform, false);
            float originalTimeScale = Time.timeScale;

            try
            {
                Screen.SetResolution(1280, 720, FullScreenMode.Windowed);
                yield return null;
                yield return null;

                ICombatSessionOverlay surface = FindSingleSurface(instance);
                surface.Configure(null, null);
                surface.ShowPause();
                Canvas.ForceUpdateCanvases();
                AssertVisibleSurfaceLayout(surface, CombatSessionOverlayMode.Pause);

                surface.Resume();
                surface.ShowFailure();
                Canvas.ForceUpdateCanvases();
                AssertVisibleSurfaceLayout(surface, CombatSessionOverlayMode.Failure);
            }
            finally
            {
                Time.timeScale = originalTimeScale;
                UnityEngine.Object.DestroyImmediate(canvasRoot);
                UnityEngine.Object.DestroyImmediate(cameraRoot);
            }
        }

        private static ICombatSessionOverlay FindSingleSurface(GameObject root)
        {
            MonoBehaviour[] behaviours = root.GetComponentsInChildren<MonoBehaviour>(includeInactive: true);
            ICombatSessionOverlay found = null;
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is not ICombatSessionOverlay surface)
                {
                    continue;
                }

                Assert.That(found, Is.Null, "Combat HUD must not own duplicate session overlays.");
                found = surface;
            }

            Assert.That(found, Is.Not.Null, "Combat HUD is missing its product session overlay.");
            return found;
        }

        private static void AssertVisibleSurfaceLayout(
            ICombatSessionOverlay surface,
            CombatSessionOverlayMode expectedMode)
        {
            Assert.That(surface.Mode, Is.EqualTo(expectedMode));
            Assert.That(surface.IsVisible, Is.True);
            Component presenter = surface as Component;
            Assert.That(presenter, Is.Not.Null);

            CanvasGroup canvasGroup = ReadField<CanvasGroup>(presenter, "canvasGroup");
            Assert.That(canvasGroup.alpha, Is.EqualTo(1f).Within(0.001f));
            Assert.That(canvasGroup.interactable, Is.True);
            Assert.That(canvasGroup.blocksRaycasts, Is.True);

            RectTransform overlayRect = presenter.transform as RectTransform;
            Assert.That(overlayRect, Is.Not.Null);
            Assert.That(
                presenter.transform.GetSiblingIndex(),
                Is.EqualTo(presenter.transform.parent.childCount - 1),
                "Visible session overlay must render above runtime-created HUD layers.");
            RectTransform panelTransform = presenter.transform.Find("Panel") as RectTransform;
            Assert.That(panelTransform, Is.Not.Null);
            Rect panelRect = GetLocalRect(overlayRect, panelTransform);
            AssertRectInside(overlayRect.rect, panelRect, panelTransform.name);
            var visibleButtonRects = new List<(string Name, Rect Rect)>();
            string[] buttonFields =
            {
                "resumeButton",
                "retryButton",
                "settingsButton",
                "stageSelectButton",
                "lobbyButton",
                "backButton"
            };

            for (int i = 0; i < buttonFields.Length; i++)
            {
                Button button = ReadField<Button>(presenter, buttonFields[i]);
                if (!button.gameObject.activeInHierarchy)
                {
                    continue;
                }

                Rect rect = GetLocalRect(overlayRect, button.transform as RectTransform);
                AssertRectInside(panelRect, rect, button.name);
                visibleButtonRects.Add((button.name, rect));
            }

            Assert.That(visibleButtonRects.Count, Is.GreaterThanOrEqualTo(3));
            for (int i = 0; i < visibleButtonRects.Count; i++)
            {
                for (int j = i + 1; j < visibleButtonRects.Count; j++)
                {
                    Assert.That(
                        visibleButtonRects[i].Rect.Overlaps(visibleButtonRects[j].Rect),
                        Is.False,
                        $"{expectedMode} buttons overlap: {visibleButtonRects[i].Name} and {visibleButtonRects[j].Name}.");
                }
            }

            AssertTextFits(ReadField<Text>(presenter, "titleText"));
            AssertTextFits(ReadField<Text>(presenter, "bodyText"));
            AssertTextFits(ReadField<Text>(presenter, "detailText"));
        }

        private static Rect GetLocalRect(RectTransform root, RectTransform target)
        {
            Assert.That(target, Is.Not.Null);
            Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(root, target);
            return Rect.MinMaxRect(bounds.min.x, bounds.min.y, bounds.max.x, bounds.max.y);
        }

        private static void AssertRectInside(Rect container, Rect content, string label)
        {
            const float tolerance = 1f;
            Assert.That(content.xMin, Is.GreaterThanOrEqualTo(container.xMin - tolerance), $"{label} exits the left edge.");
            Assert.That(content.xMax, Is.LessThanOrEqualTo(container.xMax + tolerance), $"{label} exits the right edge.");
            Assert.That(content.yMin, Is.GreaterThanOrEqualTo(container.yMin - tolerance), $"{label} exits the bottom edge.");
            Assert.That(content.yMax, Is.LessThanOrEqualTo(container.yMax + tolerance), $"{label} exits the top edge.");
        }

        private static void AssertTextFits(Text text)
        {
            Assert.That(text, Is.Not.Null);
            Assert.That(text.gameObject.activeInHierarchy, Is.True);
            Rect rect = text.rectTransform.rect;
            Assert.That(rect.width, Is.GreaterThan(0f), $"{text.name} has no layout width.");
            Assert.That(rect.height, Is.GreaterThan(0f), $"{text.name} has no layout height.");
            Assert.That(text.preferredHeight, Is.LessThanOrEqualTo(rect.height + 2f), $"{text.name} text is vertically clipped.");
        }

        private static T ReadField<T>(Component target, string fieldName)
            where T : UnityEngine.Object
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Missing field {target.GetType().Name}.{fieldName}.");
            T value = field.GetValue(target) as T;
            Assert.That(value, Is.Not.Null, $"Missing reference {target.GetType().Name}.{fieldName}.");
            return value;
        }

        private static CombatHealth CreateHealth(GameObject owner, DamageTeam team)
        {
            CombatHealth health = owner.AddComponent<CombatHealth>();
            health.ConfigureTeam(team);
            health.ConfigureMaxHealth(100f);
            return health;
        }

        private static bool ApplyLethalDamage(CombatHealth target, DamageTeam sourceTeam)
        {
            return target.TryApplyDamage(new DamageInfo(
                null,
                sourceTeam,
                target.MaxHealth + 1f,
                target.transform.position,
                Vector3.forward,
                0f,
                DamageResponsePolicy.DamageOnly,
                CombatControlLockPolicy.None));
        }

        private static void SetField<T>(T target, string fieldName, object value)
        {
            FieldInfo field = typeof(T).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Missing field {typeof(T).Name}.{fieldName}.");
            field.SetValue(target, value);
        }
    }

    public sealed class CombatSessionOverlayProbe : MonoBehaviour, ICombatSessionOverlay
    {
        public CombatSessionOverlayMode Mode { get; private set; }
        public bool IsVisible => Mode != CombatSessionOverlayMode.Hidden;
        public int FailureCount { get; private set; }

        public event Action<bool> CombatInputBlockChanged;

        public void Configure(
            BossBarrageEncounterController resultSource,
            ActionScreenCuePresenter screenCuePresenter)
        {
        }

        public void ShowPause()
        {
            SetMode(CombatSessionOverlayMode.Pause);
        }

        public void ShowSettings()
        {
            SetMode(CombatSessionOverlayMode.Settings);
        }

        public void ShowFailure()
        {
            FailureCount++;
            SetMode(CombatSessionOverlayMode.Failure);
        }

        public void Resume()
        {
            SetMode(CombatSessionOverlayMode.Hidden);
        }

        public void DismissForStageClear()
        {
            SetMode(CombatSessionOverlayMode.Hidden);
        }

        private void SetMode(CombatSessionOverlayMode newMode)
        {
            bool wasVisible = IsVisible;
            Mode = newMode;
            if (wasVisible != IsVisible)
            {
                CombatInputBlockChanged?.Invoke(IsVisible);
            }
        }
    }
}
