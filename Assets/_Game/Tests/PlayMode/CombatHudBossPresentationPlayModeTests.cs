using System.Collections;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace DimensionBrawl.Tests
{
    public sealed class CombatHudBossPresentationPlayModeTests
    {
        private const string CombatHudPrefabPath = "Assets/_Game/UI/CombatHud/PF_UI_CombatHud.prefab";
        private const int CaptureWidth = 960;
        private const int CaptureHeight = 540;

        [UnityTest]
        public IEnumerator CanonicalPrefabRendersBossHeaderPixels()
        {
            GameObject cameraObject = null;
            GameObject canvasObject = null;
            GameObject hudInstance = null;
            RenderTexture renderTexture = null;
            Texture2D frame = null;
            RenderTexture previousActive = RenderTexture.active;

            try
            {
                cameraObject = new GameObject("BossHudRenderCamera", typeof(Camera));
                Camera camera = cameraObject.GetComponent<Camera>();
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.02f, 0.025f, 0.03f, 1f);
                camera.cullingMask = ~0;
                camera.nearClipPlane = 0.1f;
                camera.farClipPlane = 10f;
                cameraObject.transform.position = new Vector3(0f, 0f, -5f);

                renderTexture = new RenderTexture(
                    CaptureWidth,
                    CaptureHeight,
                    24,
                    RenderTextureFormat.ARGB32);
                renderTexture.Create();
                camera.targetTexture = renderTexture;

                canvasObject = new GameObject(
                    "BossHudRenderCanvas",
                    typeof(RectTransform),
                    typeof(Canvas),
                    typeof(CanvasScaler),
                    typeof(GraphicRaycaster));
                Canvas canvas = canvasObject.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = camera;
                canvas.planeDistance = 1f;
                canvas.sortingOrder = 40;

                CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(2560f, 1440f);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 1f;

                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CombatHudPrefabPath);
                Assert.IsNotNull(prefab);
                hudInstance = Object.Instantiate(prefab, canvasObject.transform, worldPositionStays: false);
                RectTransform hudRect = hudInstance.GetComponent<RectTransform>();
                hudRect.anchorMin = Vector2.zero;
                hudRect.anchorMax = Vector2.one;
                hudRect.offsetMin = Vector2.zero;
                hudRect.offsetMax = Vector2.zero;
                hudRect.localScale = Vector3.one;

                Component presenter = FindComponentByTypeName(
                    hudInstance,
                    "DimensionBrawl.UI.CombatHudPresenter");
                Assert.IsNotNull(presenter);
                InvokePresenter(presenter, "SetBossHudVisible", true);
                InvokePresenter(presenter, "SetBossHealth", 100f, 100f);
                InvokePresenter(presenter, "SetBossResource", 3f, 3f);

                Canvas.ForceUpdateCanvases();
                yield return null;
                Canvas.ForceUpdateCanvases();

                camera.Render();
                RenderTexture.active = renderTexture;
                frame = new Texture2D(CaptureWidth, CaptureHeight, TextureFormat.RGBA32, false);
                frame.ReadPixels(new Rect(0f, 0f, CaptureWidth, CaptureHeight), 0, 0);
                frame.Apply();

                string captureDirectory = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Logs"));
                Directory.CreateDirectory(captureDirectory);
                File.WriteAllBytes(
                    Path.Combine(captureDirectory, "boss_hud_prefab_smoke.png"),
                    frame.EncodeToPNG());

                int redHealthPixels = CountBossHealthPixels(frame);
                Assert.GreaterOrEqual(
                    redHealthPixels,
                    120,
                    "The canonical HUD prefab should render the authored red boss HP fill in the upper center.");
            }
            finally
            {
                RenderTexture.active = previousActive;
                if (cameraObject != null)
                {
                    cameraObject.GetComponent<Camera>().targetTexture = null;
                }

                if (frame != null)
                {
                    Object.Destroy(frame);
                }

                if (renderTexture != null)
                {
                    renderTexture.Release();
                    Object.Destroy(renderTexture);
                }

                if (hudInstance != null)
                {
                    Object.Destroy(hudInstance);
                }

                if (canvasObject != null)
                {
                    Object.Destroy(canvasObject);
                }

                if (cameraObject != null)
                {
                    Object.Destroy(cameraObject);
                }
            }
        }

        private static int CountBossHealthPixels(Texture2D frame)
        {
            int xMin = Mathf.FloorToInt(frame.width * 0.24f);
            int xMax = Mathf.CeilToInt(frame.width * 0.76f);
            int yMin = Mathf.FloorToInt(frame.height * 0.68f);
            int yMax = Mathf.CeilToInt(frame.height * 0.97f);
            int count = 0;
            for (int y = yMin; y < yMax; y++)
            {
                for (int x = xMin; x < xMax; x++)
                {
                    Color32 pixel = frame.GetPixel(x, y);
                    if (pixel.r >= 190 && pixel.g <= 90 && pixel.b <= 90 && pixel.a >= 180)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        private static Component FindComponentByTypeName(GameObject root, string fullTypeName)
        {
            Component[] components = root.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                Component component = components[i];
                if (component != null && component.GetType().FullName == fullTypeName)
                {
                    return component;
                }
            }

            return null;
        }

        private static void InvokePresenter(Component presenter, string methodName, params object[] arguments)
        {
            MethodInfo method = presenter.GetType().GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.Public);
            Assert.IsNotNull(method, $"CombatHudPresenter.{methodName} is missing.");
            method.Invoke(presenter, arguments);
        }
    }
}
