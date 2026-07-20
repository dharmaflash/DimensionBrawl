using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DimensionBrawl.Editor.ChapterHubReview;
using DimensionBrawl.Editor.ContentFactoryReview;
using DimensionBrawl.Editor.LobbyOperationsReview;
using DimensionBrawl.Editor.NarrativeReview;
using DimensionBrawl.Editor.StagePreparationReview;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace DimensionBrawl.Editor
{
    /// <summary>
    /// Opens every game scene in one D3D11 Editor process, validates scene integrity, and forces
    /// one synchronous camera render/readback per scene. This is a bounded Editor smoke test, not
    /// a replacement for PlayMode route or gameplay verification.
    /// </summary>
    public static class SceneGraphicsStabilityAudit
    {
        public const string JsonReportPath =
            "C:/tmp/DimensionBrawl-SceneGraphicsStabilityAudit.json";
        public const string MarkdownReportPath =
            "C:/tmp/DimensionBrawl-SceneGraphicsStabilityAudit.md";

        [MenuItem("Tools/DimensionBrawl/Safety/Run All Scene D3D11 Stability Audit")]
        public static void RunMenu()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogError("Stop Play Mode before running the all-scene stability audit.");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            RunBatchVerification();
        }

        public static void RunBatchVerification()
        {
            if (SystemInfo.graphicsDeviceType != GraphicsDeviceType.Direct3D11)
            {
                throw new InvalidOperationException(
                    "The scene graphics stability audit is intentionally D3D11-only. "
                    + $"Current API: {SystemInfo.graphicsDeviceType}. Restart with -force-d3d11.");
            }

            DimensionBrawlEditorGraphicsStabilitySetup.RunBatchVerification();
            ProjectSceneAssetOpenGuard.RunBatchVerification();
            OlympusChapterNarrativeReviewSetup.RunBatchVerification();
            OlympusChapterHubReviewSetup.RunBatchVerification();
            LobbyOperationsDrawerReviewSetup.RunBatchVerification();
            OlympusStagePreparationReviewSetup.RunBatchVerification();
            ContentFactoryEncounterPlanReviewSetup.RunBatchVerification();
            PlayableStageDefinitionValidator.ValidateOrThrow();

            AuditReport report = AuditAllScenes();
            WriteReports(report);

            if (report.issues.Count > 0)
            {
                throw new InvalidOperationException(
                    "All-scene graphics stability audit failed:\n- "
                    + string.Join("\n- ", report.issues));
            }

            Debug.Log(
                $"All-scene D3D11 stability audit passed for {report.scenes.Count} scenes. "
                + $"Reports: {JsonReportPath} and {MarkdownReportPath}");
        }

        private static AuditReport AuditAllScenes()
        {
            string originalScenePath = SceneManager.GetActiveScene().path;
            string[] scenePaths = AssetDatabase.FindAssets(
                    "t:Scene",
                    new[] { "Assets/_Game/Scenes" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => path.EndsWith(".unity", StringComparison.OrdinalIgnoreCase))
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();

            var report = new AuditReport
            {
                generatedAtUtc = DateTime.UtcNow.ToString("O"),
                unityVersion = Application.unityVersion,
                graphicsApi = SystemInfo.graphicsDeviceType.ToString(),
                graphicsDevice = SystemInfo.graphicsDeviceName,
                scenes = new List<SceneAuditRow>(),
                issues = new List<string>(),
            };

            if (scenePaths.Length == 0)
            {
                report.issues.Add("No scenes were found under Assets/_Game/Scenes.");
                return report;
            }

            try
            {
                foreach (string scenePath in scenePaths)
                {
                    Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                    SceneAuditRow row = InspectScene(scene);
                    report.scenes.Add(row);
                    foreach (string issue in row.issues)
                    {
                        report.issues.Add($"{scenePath}: {issue}");
                    }
                }
            }
            finally
            {
                if (!string.IsNullOrWhiteSpace(originalScenePath)
                    && File.Exists(Path.GetFullPath(originalScenePath)))
                {
                    EditorSceneManager.OpenScene(originalScenePath, OpenSceneMode.Single);
                }
            }

            return report;
        }

        private static SceneAuditRow InspectScene(Scene scene)
        {
            var row = new SceneAuditRow
            {
                path = scene.path,
                inEnabledBuildSettings = EditorBuildSettings.scenes.Any(
                    entry => entry.enabled
                        && string.Equals(entry.path, scene.path, StringComparison.Ordinal)),
                issues = new List<string>(),
            };

            if (!scene.IsValid() || !scene.isLoaded)
            {
                row.issues.Add("Scene is invalid or not loaded.");
                return row;
            }

            GameObject[] roots = scene.GetRootGameObjects();
            Transform[] transforms = roots
                .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .ToArray();
            Camera[] cameras = FindComponents<Camera>(roots);
            AudioListener[] listeners = FindComponents<AudioListener>(roots);
            Light[] lights = FindComponents<Light>(roots);
            TMP_Text[] texts = FindComponents<TMP_Text>(roots);
            PlayableDirector[] directors = FindComponents<PlayableDirector>(roots);
            Renderer[] renderers = FindComponents<Renderer>(roots);
            Canvas[] canvases = FindComponents<Canvas>(roots);

            row.rootCount = roots.Length;
            row.gameObjectCount = transforms.Length;
            row.cameraCount = cameras.Length;
            row.activeCameraCount = cameras.Count(IsActiveAndEnabled);
            row.audioListenerCount = listeners.Length;
            row.activeAudioListenerCount = listeners.Count(IsActiveAndEnabled);
            row.lightCount = lights.Length;
            row.activeShadowLightCount = lights.Count(
                light => IsActiveAndEnabled(light) && light.shadows != LightShadows.None);
            row.tmpTextCount = texts.Length;
            row.playableDirectorCount = directors.Length;
            row.rendererCount = renderers.Length;
            row.canvasCount = canvases.Length;
            row.visualEffectCount = transforms.Sum(
                transform => transform.GetComponents<Component>().Count(
                    component => component != null
                        && string.Equals(
                            component.GetType().FullName,
                            "UnityEngine.VFX.VisualEffect",
                            StringComparison.Ordinal)));
            row.missingScriptCount = transforms.Sum(
                transform => GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(
                    transform.gameObject));

            if (row.missingScriptCount > 0)
            {
                row.issues.Add($"Contains {row.missingScriptCount} missing MonoBehaviour scripts.");
            }

            if (row.activeAudioListenerCount > 1)
            {
                row.issues.Add(
                    $"Contains {row.activeAudioListenerCount} simultaneously active AudioListeners.");
            }

            Camera smokeCamera = cameras
                .Where(IsActiveAndEnabled)
                .OrderByDescending(camera => camera.CompareTag("MainCamera"))
                .ThenBy(camera => camera.depth)
                .FirstOrDefault();
            if (smokeCamera != null)
            {
                row.smokeCamera = smokeCamera.name;
                try
                {
                    bool includeReadback = row.visualEffectCount == 0;
                    ForceRender(smokeCamera, includeReadback);
                    row.renderSmokeResult = includeReadback
                        ? "PASS_RENDER_READBACK"
                        : "PASS_RENDER_ONLY_VFX";
                }
                catch (Exception exception)
                {
                    row.renderSmokeResult = "FAIL";
                    row.issues.Add(
                        $"Camera render/readback failed for {smokeCamera.name}: "
                        + exception.GetType().Name
                        + ": "
                        + exception.Message);
                }
            }
            else if (row.canvasCount > 0)
            {
                // UI_StageClear is intentionally Screen Space Overlay-only. A Camera.Render call
                // cannot include that canvas, so structural validation is the truthful boundary.
                row.smokeCamera = "<overlay-only>";
                row.renderSmokeResult = "SKIP_OVERLAY_ONLY";
            }
            else
            {
                row.renderSmokeResult = "FAIL_NO_CAMERA";
                row.issues.Add("Contains neither an active camera nor an overlay UI canvas.");
            }

            return row;
        }

        private static void ForceRender(Camera camera, bool includeReadback)
        {
            RenderTexture previousTarget = camera.targetTexture;
            RenderTexture previousActive = RenderTexture.active;
            RenderTexture target = RenderTexture.GetTemporary(
                320,
                180,
                24,
                RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.Linear);
            target.antiAliasing = 1;
            Texture2D readback = null;

            try
            {
                camera.targetTexture = target;
                camera.Render();
                if (includeReadback)
                {
                    RenderTexture.active = target;
                    readback = new Texture2D(1, 1, TextureFormat.RGBA32, false, true);
                    readback.ReadPixels(new Rect(0f, 0f, 1f, 1f), 0, 0, false);
                    readback.Apply(false, false);
                    readback.GetPixel(0, 0);
                }
            }
            finally
            {
                camera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                if (readback != null)
                {
                    UnityEngine.Object.DestroyImmediate(readback);
                }

                RenderTexture.ReleaseTemporary(target);
            }
        }

        private static T[] FindComponents<T>(IEnumerable<GameObject> roots)
            where T : Component
        {
            return roots.SelectMany(root => root.GetComponentsInChildren<T>(true)).ToArray();
        }

        private static bool IsActiveAndEnabled(Behaviour behaviour)
        {
            return behaviour != null && behaviour.isActiveAndEnabled;
        }

        private static void WriteReports(AuditReport report)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(JsonReportPath) ?? "C:/tmp");
            File.WriteAllText(JsonReportPath, JsonUtility.ToJson(report, true), Encoding.UTF8);

            var markdown = new StringBuilder();
            markdown.AppendLine("# DimensionBrawl Scene Graphics Stability Audit");
            markdown.AppendLine();
            markdown.AppendLine($"- Generated: `{report.generatedAtUtc}`");
            markdown.AppendLine($"- Unity: `{report.unityVersion}`");
            markdown.AppendLine($"- Graphics API: `{report.graphicsApi}`");
            markdown.AppendLine($"- Device: `{report.graphicsDevice}`");
            markdown.AppendLine($"- Result: `{(report.issues.Count == 0 ? "PASS" : "FAIL")}`");
            markdown.AppendLine();
            markdown.AppendLine(
                "| Scene | Build | Cameras active/total | Listeners active/total | "
                + "Shadow lights | TMP | Renderers | VFX | Missing | Render smoke |");
            markdown.AppendLine(
                "|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|");
            foreach (SceneAuditRow scene in report.scenes)
            {
                markdown.AppendLine(
                    $"| `{scene.path}` | {(scene.inEnabledBuildSettings ? "Yes" : "No")} | "
                    + $"{scene.activeCameraCount}/{scene.cameraCount} | "
                    + $"{scene.activeAudioListenerCount}/{scene.audioListenerCount} | "
                    + $"{scene.activeShadowLightCount} | {scene.tmpTextCount} | "
                    + $"{scene.rendererCount} | {scene.visualEffectCount} | "
                    + $"{scene.missingScriptCount} | "
                    + $"{scene.renderSmokeResult} |");
            }

            if (report.issues.Count > 0)
            {
                markdown.AppendLine();
                markdown.AppendLine("## Issues");
                markdown.AppendLine();
                foreach (string issue in report.issues)
                {
                    markdown.AppendLine("- " + issue);
                }
            }

            File.WriteAllText(MarkdownReportPath, markdown.ToString(), Encoding.UTF8);
        }

        [Serializable]
        private sealed class AuditReport
        {
            public string generatedAtUtc;
            public string unityVersion;
            public string graphicsApi;
            public string graphicsDevice;
            public List<SceneAuditRow> scenes;
            public List<string> issues;
        }

        [Serializable]
        private sealed class SceneAuditRow
        {
            public string path;
            public bool inEnabledBuildSettings;
            public int rootCount;
            public int gameObjectCount;
            public int cameraCount;
            public int activeCameraCount;
            public int audioListenerCount;
            public int activeAudioListenerCount;
            public int lightCount;
            public int activeShadowLightCount;
            public int tmpTextCount;
            public int playableDirectorCount;
            public int rendererCount;
            public int canvasCount;
            public int visualEffectCount;
            public int missingScriptCount;
            public string smokeCamera;
            public string renderSmokeResult;
            public List<string> issues;
        }
    }
}
