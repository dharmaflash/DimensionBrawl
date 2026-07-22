using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using DimensionBrawl.Combat;
using DimensionBrawl.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DimensionBrawl.Editor
{
    /// <summary>
    /// Captures the isolated B1-1 compact stage without publishing it to Build Settings.
    /// Batch invocation must omit Unity's -quit argument because this runner owns the
    /// asynchronous Play-mode lifecycle and exits with an exact PASS/FAIL code.
    /// </summary>
    [InitializeOnLoad]
    public static class OlympusCourtyardDrillStageVisualQaCapture
    {
        public const string ScenePath =
            OlympusCourtyardDrillStageSceneSetup.ScenePath;
        public const string OutputPath =
            @"C:\tmp\DimensionBrawl-B1-1-CourtyardDrill.png";

        private const int CaptureWidth = 1600;
        private const int CaptureHeight = 900;
        private const int WarmupFrames = 90;
        private const double TimeoutSeconds = 150d;
        private const long MinimumPngBytes = 1024L;
        private const string LogPrefix =
            "[OlympusCourtyardDrillStageVisualQaCapture]";
        private const string SessionPrefix =
            "DimensionBrawl.B1_1.CourtyardDrill.VisualQa.";
        private const string ActiveKey = SessionPrefix + "Active";
        private const string BatchExitKey = SessionPrefix + "BatchExit";
        private const string PhaseKey = SessionPrefix + "Phase";
        private const string FailureKey = SessionPrefix + "Failure";
        private const string StartedUtcTicksKey = SessionPrefix + "StartedUtcTicks";
        private const string BuildSettingsBeforeKey =
            SessionPrefix + "BuildSettingsBefore";
        private const string SceneHashBeforeKey = SessionPrefix + "SceneHashBefore";

        private enum RunnerPhase
        {
            None = 0,
            RequestedPlayMode = 1,
            Capturing = 2,
            SuccessAwaitingEditMode = 3,
            FailureAwaitingEditMode = 4
        }

        private static int readyAtFrame;
        private static bool captureAttempted;

        static OlympusCourtyardDrillStageVisualQaCapture()
        {
            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
            EditorApplication.playModeStateChanged -= HandlePlayModeStateChanged;
            EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
        }

        public static void RunBatchCapture()
        {
            StartCapture(exitEditorWhenFinished: true);
        }

        private static void StartCapture(bool exitEditorWhenFinished)
        {
            try
            {
                if (SessionState.GetBool(ActiveKey, false))
                {
                    throw new InvalidOperationException(
                        "A B1-1 Courtyard Drill visual QA capture is already active.");
                }

                if (EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    throw new InvalidOperationException(
                        "B1-1 visual QA must start from Edit mode.");
                }

                RefuseDirtyOpenScenes();
                SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
                if (sceneAsset == null)
                {
                    throw new FileNotFoundException(
                        "Generate the B1-1 Courtyard Drill scene before visual QA.",
                        ScenePath);
                }

                ResetOutputFile();
                ClearSessionState();
                SessionState.SetBool(BatchExitKey, exitEditorWhenFinished);
                SessionState.SetString(BuildSettingsBeforeKey, CaptureBuildSettingsSnapshot());
                SessionState.SetString(
                    SceneHashBeforeKey,
                    AssetDatabase.GetAssetDependencyHash(ScenePath).ToString());
                SessionState.SetString(
                    StartedUtcTicksKey,
                    DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture));
                SessionState.SetString(FailureKey, string.Empty);
                SessionState.SetInt(PhaseKey, (int)RunnerPhase.RequestedPlayMode);
                SessionState.SetBool(ActiveKey, true);
                ResetRuntimeState();

                Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                if (!scene.IsValid()
                    || !string.Equals(scene.path, ScenePath, StringComparison.Ordinal)
                    || scene.isDirty)
                {
                    throw new InvalidOperationException(
                        "The B1-1 scene did not open as the exact clean capture source.");
                }

                Debug.Log(
                    $"{LogPrefix} Entering Play mode for {CaptureWidth}x{CaptureHeight} "
                    + "direct-scene capture; Build Settings remain untouched.");
                EditorApplication.EnterPlaymode();
            }
            catch (Exception exception)
            {
                HandleLaunchFailure(exception, exitEditorWhenFinished);
            }
        }

        private static void HandlePlayModeStateChanged(PlayModeStateChange state)
        {
            if (!SessionState.GetBool(ActiveKey, false))
            {
                return;
            }

            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                BeginCapturePhase();
                return;
            }

            if (state == PlayModeStateChange.EnteredEditMode)
            {
                RunnerPhase phase = ReadPhase();
                if (phase == RunnerPhase.SuccessAwaitingEditMode
                    || phase == RunnerPhase.FailureAwaitingEditMode)
                {
                    FinalizeEditorSession(
                        phase == RunnerPhase.SuccessAwaitingEditMode);
                }
            }
        }

        private static void Tick()
        {
            if (!SessionState.GetBool(ActiveKey, false))
            {
                return;
            }

            RunnerPhase phase = ReadPhase();
            if ((phase == RunnerPhase.SuccessAwaitingEditMode
                    || phase == RunnerPhase.FailureAwaitingEditMode)
                && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                FinalizeEditorSession(phase == RunnerPhase.SuccessAwaitingEditMode);
                return;
            }

            if (HasTimedOut())
            {
                FinishWithFailure(
                    $"B1-1 visual QA exceeded {TimeoutSeconds:0} seconds.");
                return;
            }

            if (phase == RunnerPhase.RequestedPlayMode && EditorApplication.isPlaying)
            {
                BeginCapturePhase();
                phase = RunnerPhase.Capturing;
            }

            if (phase != RunnerPhase.Capturing || !EditorApplication.isPlaying)
            {
                return;
            }

            if (readyAtFrame <= 0)
            {
                readyAtFrame = Time.frameCount + WarmupFrames;
                return;
            }

            if (captureAttempted || Time.frameCount < readyAtFrame)
            {
                return;
            }

            captureAttempted = true;
            try
            {
                CaptureStableGameplayFrame();
                SessionState.SetInt(
                    PhaseKey,
                    (int)RunnerPhase.SuccessAwaitingEditMode);
                Debug.Log(
                    $"{LogPrefix} CAPTURE_PASS {CaptureWidth}x{CaptureHeight} "
                    + $"`{OutputPath}`");
                EditorApplication.ExitPlaymode();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                FinishWithFailure(exception.ToString());
            }
        }

        private static void BeginCapturePhase()
        {
            SessionState.SetInt(PhaseKey, (int)RunnerPhase.Capturing);
            ResetRuntimeState();
            readyAtFrame = Time.frameCount + WarmupFrames;
        }

        private static void CaptureStableGameplayFrame()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (!activeScene.IsValid()
                || !string.Equals(activeScene.path, ScenePath, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Active Play-mode scene is `{activeScene.path}`, expected `{ScenePath}`.");
            }

            Camera captureCamera = FindSingleInScene<Camera>(activeScene);
            CombatEncounterController encounter =
                FindSingleInScene<CombatEncounterController>(activeScene);
            OneRowCombatHudBinder hudBinder =
                FindSingleInScene<OneRowCombatHudBinder>(activeScene);
            FindSingleInScene<CombatHudPresenter>(activeScene);
            if (!captureCamera.isActiveAndEnabled)
            {
                throw new InvalidOperationException(
                    "The B1-1 capture camera is not active and enabled.");
            }

            if (!hudBinder.isActiveAndEnabled)
            {
                throw new InvalidOperationException(
                    "The B1-1 one-row combat HUD binder is not active and enabled.");
            }

            if (!encounter.isActiveAndEnabled
                || !encounter.IsRunning
                || encounter.PlayerHealth == null
                || encounter.EnemyHealth == null
                || !encounter.PlayerHealth.IsAlive
                || !encounter.EnemyHealth.IsAlive)
            {
                throw new InvalidOperationException(
                    "The B1-1 encounter was not a live stable player-versus-terminal-subject run.");
            }

            Canvas[] canvases = FindAllInScene<Canvas>(activeScene);
            RenderCameraAndCanvases(captureCamera, canvases);
        }

        private static void RenderCameraAndCanvases(Camera captureCamera, Canvas[] canvases)
        {
            RenderTexture previousTarget = captureCamera.targetTexture;
            RenderTexture previousActive = RenderTexture.active;
            float previousAspect = captureCamera.aspect;
            float previousTimeScale = Time.timeScale;
            var canvasStates = new List<CanvasCaptureState>(canvases.Length);
            var target = new RenderTexture(
                CaptureWidth,
                CaptureHeight,
                24,
                RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.sRGB)
            {
                name = "DimensionBrawl_B1_1_CourtyardDrill_VisualQA",
                antiAliasing = 1,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            var image = new Texture2D(
                CaptureWidth,
                CaptureHeight,
                TextureFormat.RGBA32,
                mipChain: false,
                linear: false);

            try
            {
                Time.timeScale = 0f;
                target.Create();
                if (!target.IsCreated())
                {
                    throw new InvalidOperationException(
                        "Failed to create the B1-1 visual QA RenderTexture.");
                }

                captureCamera.targetTexture = target;
                captureCamera.aspect = CaptureWidth / (float)CaptureHeight;
                for (int i = 0; i < canvases.Length; i++)
                {
                    Canvas canvas = canvases[i];
                    if (canvas == null)
                    {
                        continue;
                    }

                    var state = new CanvasCaptureState(canvas);
                    canvasStates.Add(state);
                    if (canvas.renderMode == RenderMode.ScreenSpaceOverlay
                        || canvas.renderMode == RenderMode.ScreenSpaceCamera)
                    {
                        canvas.renderMode = RenderMode.ScreenSpaceCamera;
                        canvas.worldCamera = captureCamera;
                        canvas.planeDistance = Mathf.Max(
                            captureCamera.nearClipPlane + 0.10f,
                            0.50f);
                        ApplyExactResolutionScale(canvas, state.Scaler);
                    }
                }

                Canvas.ForceUpdateCanvases();
                captureCamera.Render();
                RenderTexture.active = target;
                image.ReadPixels(
                    new Rect(0f, 0f, CaptureWidth, CaptureHeight),
                    0,
                    0,
                    recalculateMipMaps: false);
                image.Apply(updateMipmaps: false, makeNoLongerReadable: false);

                PixelAudit audit = AuditPixels(image);
                if (!audit.IsUsable)
                {
                    throw new InvalidOperationException(
                        "The B1-1 capture was blank or lacked visual range "
                        + $"(mean={audit.MeanLuminance:0.0000}, "
                        + $"range={audit.LuminanceRange:0.0000}).");
                }

                byte[] png = image.EncodeToPNG();
                if (!IsValidPngPayload(png))
                {
                    throw new InvalidOperationException(
                        "The B1-1 visual QA capture produced an invalid PNG payload.");
                }

                string outputDirectory = Path.GetDirectoryName(OutputPath);
                if (string.IsNullOrWhiteSpace(outputDirectory))
                {
                    throw new InvalidOperationException(
                        "The B1-1 visual QA output directory is invalid.");
                }

                Directory.CreateDirectory(outputDirectory);
                File.WriteAllBytes(OutputPath, png);
                var outputInfo = new FileInfo(OutputPath);
                if (!outputInfo.Exists || outputInfo.Length != png.LongLength)
                {
                    throw new IOException(
                        "The B1-1 visual QA PNG was not written exactly once and completely.");
                }
            }
            finally
            {
                for (int i = canvasStates.Count - 1; i >= 0; i--)
                {
                    canvasStates[i].Restore();
                }

                captureCamera.targetTexture = previousTarget;
                captureCamera.aspect = previousAspect;
                RenderTexture.active = previousActive;
                Time.timeScale = previousTimeScale;
                if (target.IsCreated())
                {
                    target.Release();
                }

                UnityEngine.Object.DestroyImmediate(image);
                UnityEngine.Object.DestroyImmediate(target);
                Canvas.ForceUpdateCanvases();
            }
        }

        private static void ApplyExactResolutionScale(Canvas canvas, CanvasScaler scaler)
        {
            if (scaler == null
                || scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize)
            {
                return;
            }

            Vector2 reference = scaler.referenceResolution;
            float referenceWidth = Mathf.Max(1f, reference.x);
            float referenceHeight = Mathf.Max(1f, reference.y);
            float logWidth = Mathf.Log(CaptureWidth / referenceWidth, 2f);
            float logHeight = Mathf.Log(CaptureHeight / referenceHeight, 2f);
            float logScale = scaler.screenMatchMode switch
            {
                CanvasScaler.ScreenMatchMode.Expand => Mathf.Min(logWidth, logHeight),
                CanvasScaler.ScreenMatchMode.Shrink => Mathf.Max(logWidth, logHeight),
                _ => Mathf.Lerp(logWidth, logHeight, scaler.matchWidthOrHeight)
            };
            scaler.enabled = false;
            canvas.scaleFactor = Mathf.Pow(2f, logScale);
        }

        private static PixelAudit AuditPixels(Texture2D image)
        {
            Color32[] pixels = image.GetPixels32();
            if (pixels == null || pixels.Length == 0)
            {
                return default;
            }

            float minimum = 1f;
            float maximum = 0f;
            double total = 0d;
            int count = 0;
            int stride = Mathf.Max(1, pixels.Length / 18000);
            for (int i = 0; i < pixels.Length; i += stride)
            {
                Color32 pixel = pixels[i];
                float luminance = ((pixel.r / 255f) * 0.2126f)
                    + ((pixel.g / 255f) * 0.7152f)
                    + ((pixel.b / 255f) * 0.0722f);
                minimum = Mathf.Min(minimum, luminance);
                maximum = Mathf.Max(maximum, luminance);
                total += luminance;
                count++;
            }

            float mean = count > 0 ? (float)(total / count) : 0f;
            float range = Mathf.Max(0f, maximum - minimum);
            return new PixelAudit(
                mean,
                range,
                maximum > 0.045f && range > 0.018f && mean > 0.006f);
        }

        private static bool IsValidPngPayload(byte[] png)
        {
            return png != null
                && png.LongLength >= MinimumPngBytes
                && png.Length >= 8
                && png[0] == 0x89
                && png[1] == 0x50
                && png[2] == 0x4E
                && png[3] == 0x47
                && png[4] == 0x0D
                && png[5] == 0x0A
                && png[6] == 0x1A
                && png[7] == 0x0A;
        }

        private static void FinishWithFailure(string failure)
        {
            RunnerPhase phase = ReadPhase();
            if (phase == RunnerPhase.FailureAwaitingEditMode)
            {
                return;
            }

            string resolved = string.IsNullOrWhiteSpace(failure)
                ? "Unknown B1-1 visual QA failure."
                : failure;
            SessionState.SetString(FailureKey, resolved);
            SessionState.SetInt(PhaseKey, (int)RunnerPhase.FailureAwaitingEditMode);
            ResetOutputFile();
            Debug.LogError($"{LogPrefix} BATCH_CAPTURE_FAIL\n{resolved}");
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.ExitPlaymode();
                return;
            }

            FinalizeEditorSession(capturePhasePassed: false);
        }

        private static void FinalizeEditorSession(bool capturePhasePassed)
        {
            bool exitEditor = SessionState.GetBool(BatchExitKey, false);
            string failure = SessionState.GetString(FailureKey, string.Empty);
            var issues = new List<string>();
            if (!string.IsNullOrWhiteSpace(failure))
            {
                issues.Add(failure.Trim());
            }

            if (!string.Equals(
                    SessionState.GetString(BuildSettingsBeforeKey, string.Empty),
                    CaptureBuildSettingsSnapshot(),
                    StringComparison.Ordinal))
            {
                issues.Add("Editor Build Settings changed during B1-1 visual QA.");
            }

            string sceneHashBefore = SessionState.GetString(SceneHashBeforeKey, string.Empty);
            string sceneHashAfter = AssetDatabase.GetAssetDependencyHash(ScenePath).ToString();
            if (string.IsNullOrWhiteSpace(sceneHashBefore)
                || !string.Equals(sceneHashBefore, sceneHashAfter, StringComparison.Ordinal))
            {
                issues.Add("The authored B1-1 scene or one of its dependencies changed during capture.");
            }

            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.IsValid()
                && string.Equals(activeScene.path, ScenePath, StringComparison.Ordinal)
                && activeScene.isDirty)
            {
                issues.Add("The authored B1-1 scene is dirty after visual QA.");
            }

            if (!File.Exists(OutputPath)
                || new FileInfo(OutputPath).Length < MinimumPngBytes)
            {
                issues.Add("The exact B1-1 visual QA PNG is missing or incomplete.");
            }

            bool success = capturePhasePassed && issues.Count == 0;
            if (!success)
            {
                ResetOutputFile();
            }

            string finalFailure = success ? string.Empty : string.Join("\n", issues);
            ClearSessionState();
            if (success)
            {
                Debug.Log(
                    $"{LogPrefix} BATCH_CAPTURE_PASS "
                    + $"{CaptureWidth}x{CaptureHeight} `{OutputPath}`");
            }
            else
            {
                Debug.LogError($"{LogPrefix} BATCH_CAPTURE_FAIL\n{finalFailure}");
            }

            if (exitEditor)
            {
                EditorApplication.Exit(success ? 0 : 1);
            }
        }

        private static void HandleLaunchFailure(
            Exception exception,
            bool exitEditorWhenFinished)
        {
            Debug.LogException(exception);
            ResetOutputFile();
            ClearSessionState();
            Debug.LogError($"{LogPrefix} BATCH_CAPTURE_FAIL\n{exception}");
            if (exitEditorWhenFinished)
            {
                EditorApplication.Exit(1);
            }
        }

        private static bool HasTimedOut()
        {
            string ticksText = SessionState.GetString(StartedUtcTicksKey, string.Empty);
            if (!long.TryParse(
                    ticksText,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out long ticks)
                || ticks <= 0)
            {
                return true;
            }

            DateTime started = new DateTime(ticks, DateTimeKind.Utc);
            return (DateTime.UtcNow - started).TotalSeconds > TimeoutSeconds;
        }

        private static string CaptureBuildSettingsSnapshot()
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            var snapshot = new StringBuilder();
            snapshot.Append(scenes.Length).Append('|');
            for (int i = 0; i < scenes.Length; i++)
            {
                string path = scenes[i].path ?? string.Empty;
                snapshot.Append(scenes[i].enabled ? '1' : '0')
                    .Append(':')
                    .Append(path.Length)
                    .Append(':')
                    .Append(path)
                    .Append('|');
            }

            return snapshot.ToString();
        }

        private static void RefuseDirtyOpenScenes()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.isDirty)
                {
                    throw new InvalidOperationException(
                        $"Refusing to replace dirty open scene `{scene.path}` for visual QA.");
                }
            }
        }

        private static T FindSingleInScene<T>(Scene scene)
            where T : Component
        {
            T[] components = FindAllInScene<T>(scene);
            if (components.Length != 1)
            {
                throw new InvalidOperationException(
                    $"Scene `{scene.path}` requires exactly one {typeof(T).Name}; "
                    + $"found {components.Length}.");
            }

            return components[0];
        }

        private static T[] FindAllInScene<T>(Scene scene)
            where T : Component
        {
            var components = new List<T>();
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                components.AddRange(root.GetComponentsInChildren<T>(includeInactive: true));
            }

            return components.ToArray();
        }

        private static void ResetOutputFile()
        {
            if (File.Exists(OutputPath))
            {
                File.Delete(OutputPath);
            }
        }

        private static RunnerPhase ReadPhase()
        {
            return (RunnerPhase)SessionState.GetInt(
                PhaseKey,
                (int)RunnerPhase.None);
        }

        private static void ResetRuntimeState()
        {
            readyAtFrame = 0;
            captureAttempted = false;
        }

        private static void ClearSessionState()
        {
            SessionState.EraseBool(ActiveKey);
            SessionState.EraseBool(BatchExitKey);
            SessionState.EraseInt(PhaseKey);
            SessionState.EraseString(FailureKey);
            SessionState.EraseString(StartedUtcTicksKey);
            SessionState.EraseString(BuildSettingsBeforeKey);
            SessionState.EraseString(SceneHashBeforeKey);
            ResetRuntimeState();
        }

        private readonly struct PixelAudit
        {
            public PixelAudit(
                float meanLuminance,
                float luminanceRange,
                bool isUsable)
            {
                MeanLuminance = meanLuminance;
                LuminanceRange = luminanceRange;
                IsUsable = isUsable;
            }

            public float MeanLuminance { get; }
            public float LuminanceRange { get; }
            public bool IsUsable { get; }
        }

        private sealed class CanvasCaptureState
        {
            public CanvasCaptureState(Canvas canvas)
            {
                Canvas = canvas;
                RenderMode = canvas.renderMode;
                WorldCamera = canvas.worldCamera;
                PlaneDistance = canvas.planeDistance;
                ScaleFactor = canvas.scaleFactor;
                Scaler = canvas.GetComponent<CanvasScaler>();
                ScalerEnabled = Scaler != null && Scaler.enabled;
            }

            public Canvas Canvas { get; }
            public RenderMode RenderMode { get; }
            public Camera WorldCamera { get; }
            public float PlaneDistance { get; }
            public float ScaleFactor { get; }
            public CanvasScaler Scaler { get; }
            public bool ScalerEnabled { get; }

            public void Restore()
            {
                if (Canvas == null)
                {
                    return;
                }

                Canvas.renderMode = RenderMode;
                Canvas.worldCamera = WorldCamera;
                Canvas.planeDistance = PlaneDistance;
                Canvas.scaleFactor = ScaleFactor;
                if (Scaler != null)
                {
                    Scaler.enabled = ScalerEnabled;
                }
            }
        }
    }
}
