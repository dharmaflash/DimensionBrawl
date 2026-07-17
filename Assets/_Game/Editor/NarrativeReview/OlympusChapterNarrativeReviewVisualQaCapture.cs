using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Presentation;
using DimensionBrawl.UI.NarrativeReview;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DimensionBrawl.Editor.NarrativeReview
{
    /// <summary>
    /// Play-mode visual QA harness for the independent Olympus narrative review scene.
    ///
    /// Batch invocation must omit Unity's -quit argument because this runner owns the asynchronous
    /// play-mode lifecycle and exits the Editor with a verified code when capture finishes:
    /// -executeMethod DimensionBrawl.Editor.NarrativeReview.OlympusChapterNarrativeReviewVisualQaCapture.RunBatchCaptureAndVerify
    /// </summary>
    [InitializeOnLoad]
    public static class OlympusChapterNarrativeReviewVisualQaCapture
    {
        public const string ScenePath =
            "Assets/_Game/Scenes/Review/UI_OlympusChapterNarrativeReview.unity";
        public const string OutputDirectory =
            "C:/tmp/DimensionBrawl-OlympusNarrativeReview-QA";

        private const string ManifestPath = OutputDirectory + "/capture-manifest.json";
        private const string ReportPath = OutputDirectory + "/capture-report.md";
        private const string SessionPrefix =
            "DimensionBrawl.OlympusNarrativeReview.VisualQa.";
        private const string ActiveKey = SessionPrefix + "Active";
        private const string BatchExitKey = SessionPrefix + "BatchExit";
        private const string PhaseKey = SessionPrefix + "Phase";
        private const string FailureKey = SessionPrefix + "Failure";
        private const string StartedUtcTicksKey = SessionPrefix + "StartedUtcTicks";
        private const int InitialWarmupFrames = 8;
        private const int StateSettleFrames = 4;
        private const int ExpectedCaptureCount = 15;
        private const double LaunchTimeoutSeconds = 120d;

        private enum RunnerPhase
        {
            None = 0,
            RequestedPlayMode = 1,
            Capturing = 2,
            SuccessAwaitingEditMode = 3,
            FailureAwaitingEditMode = 4
        }

        private enum ReviewCaptureState
        {
            ChapterEntry = 0,
            VisualNovel = 1,
            TutorialCutscene = 2,
            StageBriefing = 3,
            Complete = 4
        }

        private static readonly CapturePlan[] Plans = BuildCapturePlans();
        private static readonly List<CaptureRecord> Records = new List<CaptureRecord>();

        private static OlympusChapterNarrativeReviewController controller;
        private static Camera reviewCamera;
        private static Canvas reviewCanvas;
        private static CanvasScaler reviewCanvasScaler;
        private static int planIndex;
        private static int readyAtFrame;
        private static bool statePrepared;
        private static bool runtimeInitialized;

        static OlympusChapterNarrativeReviewVisualQaCapture()
        {
            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
            EditorApplication.playModeStateChanged -= HandlePlayModeStateChanged;
            EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
        }

        [MenuItem("Tools/DimensionBrawl/Review/Capture Olympus Narrative Review Visual QA")]
        public static void CaptureFromMenu()
        {
            StartCapture(exitEditorWhenFinished: false);
        }

        public static void RunBatchCaptureAndVerify()
        {
            StartCapture(exitEditorWhenFinished: true);
        }

        public static void RunBatchCapture()
        {
            RunBatchCaptureAndVerify();
        }

        private static void StartCapture(bool exitEditorWhenFinished)
        {
            try
            {
                if (SessionState.GetBool(ActiveKey, false))
                {
                    throw new InvalidOperationException(
                        "An Olympus narrative review visual QA capture is already active.");
                }

                if (EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    throw new InvalidOperationException(
                        "Visual QA capture must be started from Edit mode.");
                }

                if (!File.Exists(AssetPathToAbsolutePath(ScenePath)))
                {
                    throw new FileNotFoundException(
                        "Generate the independent narrative review scene before capture.",
                        ScenePath);
                }

                if (!exitEditorWhenFinished
                    && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    return;
                }

                ResetOutputArtifacts();
                Records.Clear();
                ResetRuntimeFields();
                SessionState.SetBool(ActiveKey, true);
                SessionState.SetBool(BatchExitKey, exitEditorWhenFinished);
                SessionState.SetInt(PhaseKey, (int)RunnerPhase.RequestedPlayMode);
                SessionState.SetString(FailureKey, string.Empty);
                SessionState.SetString(
                    StartedUtcTicksKey,
                    DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture));

                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                Debug.Log(
                    $"[OlympusNarrativeReviewVisualQA] Entering Play mode for {ExpectedCaptureCount} exact-resolution captures.");
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
                SessionState.SetInt(PhaseKey, (int)RunnerPhase.Capturing);
                ResetRuntimeFields();
                readyAtFrame = Time.frameCount + InitialWarmupFrames;
            }
        }

        private static void Tick()
        {
            if (!SessionState.GetBool(ActiveKey, false))
            {
                return;
            }

            RunnerPhase phase = (RunnerPhase)SessionState.GetInt(
                PhaseKey,
                (int)RunnerPhase.None);
            if ((phase == RunnerPhase.SuccessAwaitingEditMode
                    || phase == RunnerPhase.FailureAwaitingEditMode)
                && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                FinalizeEditorSession(phase == RunnerPhase.SuccessAwaitingEditMode);
                return;
            }

            if (phase == RunnerPhase.RequestedPlayMode)
            {
                if (EditorApplication.isPlaying)
                {
                    SessionState.SetInt(PhaseKey, (int)RunnerPhase.Capturing);
                    ResetRuntimeFields();
                    readyAtFrame = Time.frameCount + InitialWarmupFrames;
                    return;
                }

                if (HasLaunchTimedOut())
                {
                    FinishWithFailure("Timed out while waiting for Play mode to start.");
                }

                return;
            }

            if (phase != RunnerPhase.Capturing
                || !EditorApplication.isPlaying
                || EditorApplication.isPaused
                || EditorApplication.isCompiling)
            {
                return;
            }

            try
            {
                TickCaptureInPlayMode();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                FinishWithFailure(exception.ToString());
            }
        }

        private static void TickCaptureInPlayMode()
        {
            if (!runtimeInitialized)
            {
                ResolveRuntimeBindings();
                runtimeInitialized = true;
                readyAtFrame = Time.frameCount + InitialWarmupFrames;
                return;
            }

            if (Time.frameCount < readyAtFrame)
            {
                return;
            }

            if (planIndex >= Plans.Length)
            {
                CompleteCaptureSet();
                return;
            }

            CapturePlan plan = Plans[planIndex];
            if (!statePrepared)
            {
                PrepareState(plan.State);
                statePrepared = true;
                readyAtFrame = Time.frameCount + StateSettleFrames;
                return;
            }

            ValidateExpectedRuntimeState(plan.State);
            CaptureRecord record = CapturePlanFrame(plan);
            Records.Add(record);
            Debug.Log(
                $"[OlympusNarrativeReviewVisualQA] CAPTURE_PASS "
                + $"{record.State} {record.Width}x{record.Height} `{record.Path}`");

            planIndex++;
            statePrepared = false;
            readyAtFrame = Time.frameCount + 1;
        }

        private static void ResolveRuntimeBindings()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (!activeScene.IsValid()
                || !string.Equals(activeScene.path, ScenePath, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Active Play mode scene is `{activeScene.path}`, expected `{ScenePath}`.");
            }

            controller = FindSingleInScene<OlympusChapterNarrativeReviewController>(activeScene);
            reviewCamera = FindSingleInScene<Camera>(activeScene);
            reviewCanvas = FindSingleInScene<Canvas>(activeScene);
            reviewCanvasScaler = reviewCanvas.GetComponent<CanvasScaler>()
                ?? throw new InvalidOperationException(
                    "Narrative review Canvas is missing its CanvasScaler.");

            if (!controller.HasValidCutsceneBoundary)
            {
                throw new InvalidOperationException(
                    "Narrative review controller has no valid StageCutscenePort/PlayableDirector boundary.");
            }

            if (reviewCanvasScaler.uiScaleMode
                != CanvasScaler.ScaleMode.ScaleWithScreenSize)
            {
                throw new InvalidOperationException(
                    "Narrative review CanvasScaler must use ScaleWithScreenSize.");
            }
        }

        private static void PrepareState(ReviewCaptureState state)
        {
            controller.BeginChapterEntry();
            if (state == ReviewCaptureState.ChapterEntry)
            {
                return;
            }

            controller.BeginVisualNovel();
            if (state == ReviewCaptureState.VisualNovel)
            {
                controller.RevealCurrentNarrativeLine();
                return;
            }

            if (controller.NarrativeSession == null)
            {
                throw new InvalidOperationException(
                    "Visual QA could not resolve the active narrative session before completion.");
            }

            controller.NarrativeSession.Skip();
            if (state == ReviewCaptureState.TutorialCutscene)
            {
                PlayableDirector director = FindSingleInScene<PlayableDirector>(
                    SceneManager.GetActiveScene());
                director.time = 3.4d;
                director.Evaluate();
                return;
            }

            controller.SkipCutscene();
            IntroGatePodDialogueOverlay overlay =
                FindOptionalInScene<IntroGatePodDialogueOverlay>(SceneManager.GetActiveScene());
            overlay?.Clear();
            if (state == ReviewCaptureState.StageBriefing)
            {
                return;
            }

            controller.CompleteReview();
        }

        private static void ValidateExpectedRuntimeState(ReviewCaptureState state)
        {
            NarrativeReviewPhase expected = state switch
            {
                ReviewCaptureState.ChapterEntry => NarrativeReviewPhase.ChapterEntry,
                ReviewCaptureState.VisualNovel => NarrativeReviewPhase.VisualNovel,
                ReviewCaptureState.TutorialCutscene => NarrativeReviewPhase.TutorialCutscene,
                ReviewCaptureState.StageBriefing => NarrativeReviewPhase.StageBriefing,
                ReviewCaptureState.Complete => NarrativeReviewPhase.Complete,
                _ => NarrativeReviewPhase.None
            };
            if (controller.CurrentPhase != expected)
            {
                throw new InvalidOperationException(
                    $"Expected controller phase {expected}, got {controller.CurrentPhase}.");
            }

            if (controller.StageProjection == null)
            {
                throw new InvalidOperationException(
                    $"Canonical UIStageCatalog projection is unavailable in {expected}.");
            }

            bool requiresCompletedNarrative =
                state == ReviewCaptureState.TutorialCutscene
                || state == ReviewCaptureState.StageBriefing
                || state == ReviewCaptureState.Complete;
            if (requiresCompletedNarrative
                && controller.NarrativeSession?.IsCompleted != true)
            {
                throw new InvalidOperationException(
                    $"Narrative session must be completed before capturing {state}.");
            }

            int expectedFinalizerCount =
                state == ReviewCaptureState.StageBriefing
                || state == ReviewCaptureState.Complete
                    ? 1
                    : 0;
            if (controller.CompletionDispatchCount != expectedFinalizerCount)
            {
                throw new InvalidOperationException(
                    $"Expected cutscene finalizer dispatch {expectedFinalizerCount} in {state}, "
                    + $"got {controller.CompletionDispatchCount}.");
            }

            if (StageRunRuntime.HasActiveContext)
            {
                throw new InvalidOperationException(
                    $"Review capture {state} unexpectedly created an active StageRun context.");
            }

            if (state == ReviewCaptureState.StageBriefing
                && controller.StageProjection.Briefing == null)
            {
                throw new InvalidOperationException(
                    "Canonical stage briefing read model is unavailable.");
            }
        }

        private static CaptureRecord CapturePlanFrame(CapturePlan plan)
        {
            string path = $"{OutputDirectory}/{plan.Sequence:00}_{plan.State}_{plan.Width}x{plan.Height}.png";
            RenderTexture previousTarget = reviewCamera.targetTexture;
            RenderTexture previousActive = RenderTexture.active;
            RenderMode previousRenderMode = reviewCanvas.renderMode;
            Camera previousWorldCamera = reviewCanvas.worldCamera;
            float previousPlaneDistance = reviewCanvas.planeDistance;
            float previousCanvasScaleFactor = reviewCanvas.scaleFactor;
            float previousCameraAspect = reviewCamera.aspect;
            bool previousScalerEnabled = reviewCanvasScaler.enabled;

            RenderTexture target = new RenderTexture(
                plan.Width,
                plan.Height,
                24,
                RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.sRGB)
            {
                name = $"OlympusNarrativeReviewQA_{plan.Width}x{plan.Height}",
                antiAliasing = 1,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            Texture2D image = new Texture2D(
                plan.Width,
                plan.Height,
                TextureFormat.RGBA32,
                mipChain: false,
                linear: false);

            try
            {
                target.Create();
                reviewCamera.targetTexture = target;
                reviewCamera.aspect = plan.Width / (float)plan.Height;
                reviewCanvas.renderMode = RenderMode.ScreenSpaceCamera;
                reviewCanvas.worldCamera = reviewCamera;
                reviewCanvas.planeDistance = Mathf.Max(
                    reviewCamera.nearClipPlane + 0.10f,
                    0.50f);
                reviewCanvasScaler.enabled = false;
                reviewCanvas.scaleFactor = ResolveCanvasScaleFactor(
                    plan.Width,
                    plan.Height,
                    reviewCanvasScaler.referenceResolution,
                    reviewCanvasScaler.matchWidthOrHeight);

                Canvas.ForceUpdateCanvases();
                reviewCamera.Render();
                RenderTexture.active = target;
                image.ReadPixels(
                    new Rect(0f, 0f, plan.Width, plan.Height),
                    0,
                    0,
                    recalculateMipMaps: false);
                image.Apply(updateMipmaps: false, makeNoLongerReadable: false);

                PixelAudit audit = AuditPixels(image);
                if (!audit.IsUsable)
                {
                    throw new InvalidOperationException(
                        $"Captured frame `{path}` is blank or has insufficient visual range "
                        + $"(mean={audit.MeanLuminance:0.0000}, range={audit.LuminanceRange:0.0000}).");
                }

                byte[] png = image.EncodeToPNG();
                if (png == null || png.Length < 1024)
                {
                    throw new InvalidOperationException(
                        $"Captured frame `{path}` produced an invalid PNG payload.");
                }

                File.WriteAllBytes(path, png);
                return new CaptureRecord
                {
                    Sequence = plan.Sequence,
                    State = plan.State.ToString(),
                    Width = plan.Width,
                    Height = plan.Height,
                    Path = path,
                    FileBytes = png.LongLength,
                    MeanLuminance = audit.MeanLuminance,
                    LuminanceRange = audit.LuminanceRange,
                    ControllerPhase = controller.CurrentPhase.ToString(),
                    StageProjectionResolved = controller.StageProjection != null
                };
            }
            finally
            {
                reviewCanvasScaler.enabled = previousScalerEnabled;
                reviewCanvas.scaleFactor = previousCanvasScaleFactor;
                reviewCanvas.renderMode = previousRenderMode;
                reviewCanvas.worldCamera = previousWorldCamera;
                reviewCanvas.planeDistance = previousPlaneDistance;
                reviewCamera.targetTexture = previousTarget;
                reviewCamera.aspect = previousCameraAspect;
                RenderTexture.active = previousActive;
                target.Release();
                UnityEngine.Object.DestroyImmediate(image);
                UnityEngine.Object.DestroyImmediate(target);
                Canvas.ForceUpdateCanvases();
            }
        }

        private static float ResolveCanvasScaleFactor(
            int width,
            int height,
            Vector2 referenceResolution,
            float matchWidthOrHeight)
        {
            float safeReferenceWidth = Mathf.Max(1f, referenceResolution.x);
            float safeReferenceHeight = Mathf.Max(1f, referenceResolution.y);
            float logWidth = Mathf.Log(width / safeReferenceWidth, 2f);
            float logHeight = Mathf.Log(height / safeReferenceHeight, 2f);
            float logWeightedAverage = Mathf.Lerp(
                logWidth,
                logHeight,
                Mathf.Clamp01(matchWidthOrHeight));
            return Mathf.Pow(2f, logWeightedAverage);
        }

        private static PixelAudit AuditPixels(Texture2D image)
        {
            const int HorizontalSamples = 32;
            const int VerticalSamples = 18;
            float minimum = 1f;
            float maximum = 0f;
            double total = 0d;
            int count = 0;
            for (int sampleY = 0; sampleY < VerticalSamples; sampleY++)
            {
                int y = Mathf.RoundToInt(
                    (image.height - 1) * (sampleY / (float)(VerticalSamples - 1)));
                for (int sampleX = 0; sampleX < HorizontalSamples; sampleX++)
                {
                    int x = Mathf.RoundToInt(
                        (image.width - 1) * (sampleX / (float)(HorizontalSamples - 1)));
                    Color pixel = image.GetPixel(x, y);
                    float luminance = (pixel.r * 0.2126f)
                        + (pixel.g * 0.7152f)
                        + (pixel.b * 0.0722f);
                    minimum = Mathf.Min(minimum, luminance);
                    maximum = Mathf.Max(maximum, luminance);
                    total += luminance;
                    count++;
                }
            }

            float mean = count > 0 ? (float)(total / count) : 0f;
            float range = Mathf.Max(0f, maximum - minimum);
            return new PixelAudit(
                mean,
                range,
                maximum > 0.045f && range > 0.018f && mean > 0.006f);
        }

        private static void CompleteCaptureSet()
        {
            List<string> issues = ValidateOutputSet();
            bool passed = issues.Count == 0;
            string failure = passed ? string.Empty : string.Join("\n", issues);
            WriteReports(passed, failure);
            if (passed)
            {
                Debug.Log(
                    $"[OlympusNarrativeReviewVisualQA] BATCH_VISUAL_QA_PASS "
                    + $"captures={Records.Count} output=`{OutputDirectory}`");
                SessionState.SetInt(
                    PhaseKey,
                    (int)RunnerPhase.SuccessAwaitingEditMode);
            }
            else
            {
                Debug.LogError(
                    "[OlympusNarrativeReviewVisualQA] BATCH_VISUAL_QA_FAIL\n" + failure);
                SessionState.SetString(FailureKey, failure);
                SessionState.SetInt(
                    PhaseKey,
                    (int)RunnerPhase.FailureAwaitingEditMode);
            }

            EditorApplication.ExitPlaymode();
        }

        private static List<string> ValidateOutputSet()
        {
            var issues = new List<string>();
            if (Records.Count != ExpectedCaptureCount)
            {
                issues.Add(
                    $"Expected {ExpectedCaptureCount} capture records, found {Records.Count}.");
            }

            string[] pngFiles = Directory.Exists(OutputDirectory)
                ? Directory.GetFiles(OutputDirectory, "*.png", SearchOption.TopDirectoryOnly)
                : Array.Empty<string>();
            if (pngFiles.Length != ExpectedCaptureCount)
            {
                issues.Add(
                    $"Expected exactly {ExpectedCaptureCount} PNG files in the dedicated output directory; "
                    + $"found {pngFiles.Length}.");
            }

            for (int i = 0; i < Plans.Length; i++)
            {
                CapturePlan plan = Plans[i];
                CaptureRecord record = Records.FirstOrDefault(
                    candidate => candidate.Sequence == plan.Sequence);
                if (record == null)
                {
                    issues.Add($"Missing capture record {plan.Sequence:00} ({plan.State}).");
                    continue;
                }

                if (!File.Exists(record.Path))
                {
                    issues.Add($"Capture file is missing: `{record.Path}`.");
                    continue;
                }

                byte[] png = File.ReadAllBytes(record.Path);
                Texture2D decoded = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                try
                {
                    if (!decoded.LoadImage(png, markNonReadable: false))
                    {
                        issues.Add($"Could not decode capture PNG `{record.Path}`.");
                    }
                    else if (decoded.width != plan.Width || decoded.height != plan.Height)
                    {
                        issues.Add(
                            $"Capture `{record.Path}` is {decoded.width}x{decoded.height}; "
                            + $"expected {plan.Width}x{plan.Height}.");
                    }
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(decoded);
                }

                if (!string.Equals(record.State, plan.State.ToString(), StringComparison.Ordinal))
                {
                    issues.Add(
                        $"Capture {plan.Sequence:00} state mismatch: {record.State} vs {plan.State}.");
                }

                if (!record.StageProjectionResolved)
                {
                    issues.Add(
                        $"Capture {plan.Sequence:00} did not resolve canonical stage projection.");
                }
            }

            foreach (IGrouping<string, CaptureRecord> stateGroup in Records.GroupBy(
                         record => record.State,
                         StringComparer.Ordinal))
            {
                if (stateGroup.Count() != 3)
                {
                    issues.Add(
                        $"State {stateGroup.Key} has {stateGroup.Count()} captures; expected 3.");
                }
            }

            return issues;
        }

        private static void ResetOutputArtifacts()
        {
            Directory.CreateDirectory(OutputDirectory);
            foreach (string pngPath in Directory.GetFiles(
                         OutputDirectory,
                         "*.png",
                         SearchOption.TopDirectoryOnly))
            {
                File.Delete(pngPath);
            }

            if (File.Exists(ManifestPath))
            {
                File.Delete(ManifestPath);
            }

            if (File.Exists(ReportPath))
            {
                File.Delete(ReportPath);
            }
        }

        private static void FinishWithFailure(string failure)
        {
            string resolvedFailure = string.IsNullOrWhiteSpace(failure)
                ? "Unknown visual QA failure."
                : failure;
            SessionState.SetString(FailureKey, resolvedFailure);
            WriteReports(passed: false, resolvedFailure);
            Debug.LogError(
                "[OlympusNarrativeReviewVisualQA] BATCH_VISUAL_QA_FAIL\n"
                + resolvedFailure);
            SessionState.SetInt(
                PhaseKey,
                (int)RunnerPhase.FailureAwaitingEditMode);
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.ExitPlaymode();
            }
        }

        private static void WriteReports(bool passed, string failure)
        {
            Directory.CreateDirectory(OutputDirectory);
            var manifest = new CaptureManifest
            {
                Scene = ScenePath,
                GeneratedUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
                Passed = passed,
                Failure = failure ?? string.Empty,
                ExpectedCaptureCount = ExpectedCaptureCount,
                Captures = Records.ToArray()
            };
            File.WriteAllText(
                ManifestPath,
                JsonUtility.ToJson(manifest, prettyPrint: true),
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

            var report = new StringBuilder();
            report.AppendLine("# Olympus Chapter Narrative Review Visual QA");
            report.AppendLine();
            report.AppendLine(passed ? "Status: PASS" : "Status: FAIL");
            report.AppendLine();
            report.AppendLine($"- Scene: `{ScenePath}`");
            report.AppendLine($"- Output: `{OutputDirectory}`");
            report.AppendLine($"- Captures: `{Records.Count}` / `{ExpectedCaptureCount}`");
            report.AppendLine("- Rendering: Camera RenderTexture + ScreenSpaceCamera Canvas");
            report.AppendLine("- Canonical StageRun mutation: none");
            report.AppendLine();
            if (!string.IsNullOrWhiteSpace(failure))
            {
                report.AppendLine("## Failure");
                report.AppendLine();
                report.AppendLine("```");
                report.AppendLine(failure.Trim());
                report.AppendLine("```");
                report.AppendLine();
            }

            report.AppendLine("## Captures");
            report.AppendLine();
            report.AppendLine("| # | State | Resolution | Bytes | Luma mean/range | Path |");
            report.AppendLine("|---:|---|---:|---:|---:|---|");
            for (int i = 0; i < Records.Count; i++)
            {
                CaptureRecord record = Records[i];
                report.AppendLine(
                    $"| {record.Sequence:00} | {record.State} | "
                    + $"{record.Width}x{record.Height} | {record.FileBytes} | "
                    + $"{record.MeanLuminance:0.0000}/{record.LuminanceRange:0.0000} | "
                    + $"`{record.Path}` |");
            }

            File.WriteAllText(
                ReportPath,
                report.ToString(),
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }

        private static void HandleLaunchFailure(Exception exception, bool exitEditorWhenFinished)
        {
            Debug.LogException(exception);
            Directory.CreateDirectory(OutputDirectory);
            Records.Clear();
            WriteReports(passed: false, exception.ToString());
            ClearSessionState();
            Debug.LogError("[OlympusNarrativeReviewVisualQA] BATCH_VISUAL_QA_FAIL");
            if (exitEditorWhenFinished)
            {
                EditorApplication.Exit(1);
            }
        }

        private static void FinalizeEditorSession(bool passed)
        {
            bool exitEditor = SessionState.GetBool(BatchExitKey, false);
            string failure = SessionState.GetString(FailureKey, string.Empty);
            ClearSessionState();
            if (passed)
            {
                Debug.Log(
                    $"[OlympusNarrativeReviewVisualQA] Visual QA complete: `{ReportPath}`.");
            }
            else
            {
                Debug.LogError(
                    "[OlympusNarrativeReviewVisualQA] Visual QA failed: " + failure);
            }

            if (exitEditor)
            {
                EditorApplication.Exit(passed ? 0 : 1);
            }
        }

        private static bool HasLaunchTimedOut()
        {
            string ticksText = SessionState.GetString(StartedUtcTicksKey, string.Empty);
            if (!long.TryParse(
                    ticksText,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out long ticks))
            {
                return false;
            }

            DateTime started = new DateTime(ticks, DateTimeKind.Utc);
            return (DateTime.UtcNow - started).TotalSeconds > LaunchTimeoutSeconds;
        }

        private static void ResetRuntimeFields()
        {
            controller = null;
            reviewCamera = null;
            reviewCanvas = null;
            reviewCanvasScaler = null;
            planIndex = 0;
            readyAtFrame = 0;
            statePrepared = false;
            runtimeInitialized = false;
            Records.Clear();
        }

        private static void ClearSessionState()
        {
            SessionState.EraseBool(ActiveKey);
            SessionState.EraseBool(BatchExitKey);
            SessionState.EraseInt(PhaseKey);
            SessionState.EraseString(FailureKey);
            SessionState.EraseString(StartedUtcTicksKey);
            ResetRuntimeFields();
        }

        private static CapturePlan[] BuildCapturePlans()
        {
            var plans = new List<CapturePlan>(ExpectedCaptureCount);
            int sequence = 1;
            AddResolution(plans, ref sequence, 1920, 1080);
            AddResolution(plans, ref sequence, 2400, 1080);
            AddResolution(plans, ref sequence, 2520, 1080);
            return plans.ToArray();
        }

        private static void AddResolution(
            List<CapturePlan> plans,
            ref int sequence,
            int width,
            int height)
        {
            plans.Add(new CapturePlan(sequence++, width, height, ReviewCaptureState.ChapterEntry));
            plans.Add(new CapturePlan(sequence++, width, height, ReviewCaptureState.VisualNovel));
            plans.Add(new CapturePlan(sequence++, width, height, ReviewCaptureState.TutorialCutscene));
            plans.Add(new CapturePlan(sequence++, width, height, ReviewCaptureState.StageBriefing));
            plans.Add(new CapturePlan(sequence++, width, height, ReviewCaptureState.Complete));
        }

        private static T FindSingleInScene<T>(Scene scene) where T : Component
        {
            T[] components = FindAllInScene<T>(scene);
            if (components.Length != 1)
            {
                throw new InvalidOperationException(
                    $"Scene `{scene.path}` must contain exactly one {typeof(T).Name}; found {components.Length}.");
            }

            return components[0];
        }

        private static T FindOptionalInScene<T>(Scene scene) where T : Component
        {
            T[] components = FindAllInScene<T>(scene);
            return components.Length > 0 ? components[0] : null;
        }

        private static T[] FindAllInScene<T>(Scene scene) where T : Component
        {
            var components = new List<T>();
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                components.AddRange(root.GetComponentsInChildren<T>(includeInactive: true));
            }

            return components.ToArray();
        }

        private static string AssetPathToAbsolutePath(string assetPath)
        {
            return Path.GetFullPath(
                Path.Combine(
                    Directory.GetCurrentDirectory(),
                    assetPath.Replace('/', Path.DirectorySeparatorChar)));
        }

        private readonly struct CapturePlan
        {
            public CapturePlan(
                int sequence,
                int width,
                int height,
                ReviewCaptureState state)
            {
                Sequence = sequence;
                Width = width;
                Height = height;
                State = state;
            }

            public int Sequence { get; }
            public int Width { get; }
            public int Height { get; }
            public ReviewCaptureState State { get; }
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

        [Serializable]
        private sealed class CaptureRecord
        {
            public int Sequence;
            public string State;
            public int Width;
            public int Height;
            public string Path;
            public long FileBytes;
            public float MeanLuminance;
            public float LuminanceRange;
            public string ControllerPhase;
            public bool StageProjectionResolved;
        }

        [Serializable]
        private sealed class CaptureManifest
        {
            public string Scene;
            public string GeneratedUtc;
            public bool Passed;
            public string Failure;
            public int ExpectedCaptureCount;
            public CaptureRecord[] Captures;
        }
    }
}
