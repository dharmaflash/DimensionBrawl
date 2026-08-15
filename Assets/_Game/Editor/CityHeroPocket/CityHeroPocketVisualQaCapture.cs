using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using DimensionBrawl.Combat;
using DimensionBrawl.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DimensionBrawl.Editor.CityHeroPocket
{
    /// <summary>
    /// Produces a single ungraded QHD product-frame proof for CITY-GATE-01.
    /// The scene is opened read-only, rendered through its authored camera and
    /// restored without saving. This is a visual admission check, not a PV take.
    /// </summary>
    [InitializeOnLoad]
    public static class CityHeroPocketVisualQaCapture
    {
        public const string OutputRoot =
            @"D:\DimensionBrawl_PV\03_review\PREEDIT_REVIEW";

        private const int CaptureWidth = 2560;
        private const int CaptureHeight = 1440;
        private const int WarmupFrames = 90;
        private const double TimeoutSeconds = 180d;
        private const long MinimumPngBytes = 1024L;
        internal const float DefaultMinimumSubjectViewportY = -0.02f;
        private const string LogPrefix = "[CityHeroPocketVisualQaCapture]";
        private const string SessionPrefix =
            "DimensionBrawl.CityHeroPocket.ProductVisualQa.";
        private const string ActiveKey = SessionPrefix + "Active";
        private const string BatchExitKey = SessionPrefix + "BatchExit";
        private const string PhaseKey = SessionPrefix + "Phase";
        private const string FailureKey = SessionPrefix + "Failure";
        private const string OutputDirectoryKey = SessionPrefix + "OutputDirectory";
        private const string SceneHashBeforeKey = SessionPrefix + "SceneHashBefore";
        private const string SceneSetupBeforeKey = SessionPrefix + "SceneSetupBefore";
        private const string ReportDraftKey = SessionPrefix + "ReportDraft";
        private const string StartedUtcTicksKey = SessionPrefix + "StartedUtcTicks";

        private static int readyAtFrame;
        private static bool captureAttempted;

        private enum RunnerPhase
        {
            None = 0,
            RequestedPlayMode = 1,
            Capturing = 2,
            SuccessAwaitingEditMode = 3,
            FailureAwaitingEditMode = 4
        }

        static CityHeroPocketVisualQaCapture()
        {
            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
            EditorApplication.playModeStateChanged -= HandlePlayModeStateChanged;
            EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
        }

        [MenuItem("DimensionBrawl/City Hero Pocket/Capture Product Visual QA")]
        public static void CaptureFromMenu()
        {
            StartCapture(exitEditorWhenFinished: false);
        }

        public static void RunBatchCapture()
        {
            StartCapture(exitEditorWhenFinished: true);
        }

        private static void StartCapture(bool exitEditorWhenFinished)
        {
            if (SessionState.GetBool(ActiveKey, false))
            {
                Debug.LogError(
                    $"{LogPrefix} A City product visual QA capture is already active; "
                    + "the existing owner was left untouched.");
                return;
            }

            try
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    throw new InvalidOperationException(
                        "City visual QA must start from Edit mode.");
                }

                RefuseDirtyOpenScenes();
                string previousSetup = CaptureSceneSetupSnapshot();
                CityHeroPocketAuthoredPackValidator.ValidateAuthoredOutputs();
                string outputDirectory = ReserveOutputDirectory();

                ClearSessionState();
                SessionState.SetBool(ActiveKey, true);
                SessionState.SetBool(BatchExitKey, exitEditorWhenFinished);
                SessionState.SetInt(PhaseKey, (int)RunnerPhase.RequestedPlayMode);
                SessionState.SetString(FailureKey, string.Empty);
                SessionState.SetString(OutputDirectoryKey, outputDirectory);
                SessionState.SetString(SceneSetupBeforeKey, previousSetup);
                SessionState.SetString(
                    SceneHashBeforeKey,
                    AssetDatabase.GetAssetDependencyHash(
                        CityHeroPocketSceneSetup.ScenePath).ToString());
                SessionState.SetString(
                    StartedUtcTicksKey,
                    DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture));
                ResetRuntimeState();

                Scene scene = EditorSceneManager.OpenScene(
                    CityHeroPocketSceneSetup.ScenePath,
                    OpenSceneMode.Single);
                CityHeroPocketAuthoredPackValidator.ValidateLoadedScene(scene);
                Debug.Log(
                    $"{LogPrefix} Entering Play mode for an ungraded "
                    + $"{CaptureWidth}x{CaptureHeight} product-frame proof.");
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
                FinalizeEditorSession(
                    phase == RunnerPhase.SuccessAwaitingEditMode);
                return;
            }

            if (HasTimedOut())
            {
                FinishWithFailure(
                    $"City product visual QA exceeded {TimeoutSeconds:0} seconds.");
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
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid()
                || !string.Equals(
                    scene.path,
                    CityHeroPocketSceneSetup.ScenePath,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Active Play-mode scene is `{scene.path}`, expected CityHeroPocket.");
            }

            Camera captureCamera = FindSingleInScene<Camera>(scene);
            CombatEncounterController encounter =
                FindSingleInScene<CombatEncounterController>(scene);
            OneRowCombatHudBinder hudBinder =
                FindSingleInScene<OneRowCombatHudBinder>(scene);
            CombatHudPresenter hudPresenter =
                FindSingleInScene<CombatHudPresenter>(scene);
            if (!captureCamera.isActiveAndEnabled
                || !hudBinder.isActiveAndEnabled
                || !hudPresenter.isActiveAndEnabled)
            {
                throw new InvalidOperationException(
                    "The City camera and runtime HUD are not all active and enabled.");
            }

            if (!encounter.isActiveAndEnabled
                || !encounter.IsRunning
                || encounter.PlayerHealth == null
                || encounter.EnemyHealth == null
                || !encounter.PlayerHealth.IsAlive
                || !encounter.EnemyHealth.IsAlive)
            {
                throw new InvalidOperationException(
                    "The City encounter is not a live product gameplay state.");
            }

            string outputDirectory = SessionState.GetString(
                OutputDirectoryKey,
                string.Empty);
            if (string.IsNullOrWhiteSpace(outputDirectory))
            {
                throw new InvalidOperationException(
                    "The City visual QA output reservation was lost.");
            }

            string pngPath = Path.Combine(
                outputDirectory,
                "CITY_G02_PRODUCT_HUDON_QHD.png");
            PixelAudit audit = RenderProductFrame(
                captureCamera,
                FindAllInScene<Canvas>(scene),
                encounter.PlayerHealth.transform,
                encounter.EnemyHealth.transform,
                pngPath,
                out ViewportProof player,
                out ViewportProof enemy);
            RequireHealthyPixels(audit);
            RequireReadableSubject(
                "player",
                player,
                DefaultMinimumSubjectViewportY);
            RequireReadableSubject(
                "enemy",
                enemy,
                DefaultMinimumSubjectViewportY);

            var pngInfo = new FileInfo(pngPath);
            if (!pngInfo.Exists || pngInfo.Length < MinimumPngBytes)
            {
                throw new IOException(
                    "The City visual QA PNG was not written completely.");
            }

            var report = new VisualQaReport
            {
                passed = false,
                capturedUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
                unityVersion = Application.unityVersion,
                scenePath = CityHeroPocketSceneSetup.ScenePath,
                sceneDependencyHash = SessionState.GetString(
                    SceneHashBeforeKey,
                    string.Empty),
                width = CaptureWidth,
                height = CaptureHeight,
                pngFile = Path.GetFileName(pngPath),
                pngBytes = pngInfo.Length,
                pngSha256 = ComputeSha256(pngPath),
                meanLuminance = audit.meanLuminance,
                luminanceRange = audit.luminanceRange,
                blackPixelRatio = audit.blackPixelRatio,
                hardWhitePixelRatio = audit.hardWhitePixelRatio,
                magentaPixelRatio = audit.magentaPixelRatio,
                player = player,
                enemy = enemy
            };
            SessionState.SetString(
                ReportDraftKey,
                JsonUtility.ToJson(report));
        }

        private static PixelAudit RenderProductFrame(
            Camera captureCamera,
            Canvas[] canvases,
            Transform playerRoot,
            Transform enemyRoot,
            string outputPath,
            out ViewportProof player,
            out ViewportProof enemy)
        {
            player = default;
            enemy = default;
            RenderTexture previousTarget = captureCamera.targetTexture;
            RenderTexture previousActive = RenderTexture.active;
            float previousAspect = captureCamera.aspect;
            var canvasStates = new List<CanvasCaptureState>(canvases.Length);
            var target = new RenderTexture(
                CaptureWidth,
                CaptureHeight,
                24,
                RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.sRGB)
            {
                name = "DimensionBrawl_CityHeroPocket_VisualQA",
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
                target.Create();
                if (!target.IsCreated())
                {
                    throw new InvalidOperationException(
                        "Failed to create the City visual QA RenderTexture.");
                }

                captureCamera.targetTexture = target;
                captureCamera.aspect = CaptureWidth / (float)CaptureHeight;
                Physics.SyncTransforms();
                player = BuildViewportProof(
                    captureCamera,
                    playerRoot,
                    preferCharacterControllerBounds: true);
                enemy = BuildViewportProof(
                    captureCamera,
                    enemyRoot,
                    preferCharacterControllerBounds: false);
                for (int i = 0; i < canvases.Length; i++)
                {
                    Canvas canvas = canvases[i];
                    if (canvas == null || !canvas.isActiveAndEnabled)
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
                        ApplyExactResolutionScale(canvas, state.scaler);
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
                byte[] png = image.EncodeToPNG();
                if (png == null
                    || png.LongLength < MinimumPngBytes
                    || png[0] != 0x89
                    || png[1] != 0x50
                    || png[2] != 0x4E
                    || png[3] != 0x47)
                {
                    throw new InvalidOperationException(
                        "The City visual QA renderer produced an invalid PNG payload.");
                }

                File.WriteAllBytes(outputPath, png);
                return audit;
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
                if (target.IsCreated())
                {
                    target.Release();
                }

                UnityEngine.Object.DestroyImmediate(image);
                UnityEngine.Object.DestroyImmediate(target);
                Canvas.ForceUpdateCanvases();
            }
        }

        private static void ApplyExactResolutionScale(
            Canvas canvas,
            CanvasScaler scaler)
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
                throw new InvalidOperationException(
                    "The City visual QA texture had no decoded pixels.");
            }

            double luminanceTotal = 0d;
            float minimum = 1f;
            float maximum = 0f;
            long black = 0;
            long hardWhite = 0;
            long magenta = 0;
            for (int i = 0; i < pixels.Length; i++)
            {
                Color32 pixel = pixels[i];
                float luminance = (
                    (0.2126f * pixel.r)
                    + (0.7152f * pixel.g)
                    + (0.0722f * pixel.b)) / 255f;
                luminanceTotal += luminance;
                minimum = Mathf.Min(minimum, luminance);
                maximum = Mathf.Max(maximum, luminance);
                if (pixel.r <= 8 && pixel.g <= 8 && pixel.b <= 8)
                {
                    black++;
                }

                if (pixel.r >= 250 && pixel.g >= 250 && pixel.b >= 250)
                {
                    hardWhite++;
                }

                if (pixel.r >= 200 && pixel.b >= 200 && pixel.g <= 80)
                {
                    magenta++;
                }
            }

            double count = pixels.LongLength;
            return new PixelAudit
            {
                meanLuminance = (float)(luminanceTotal / count),
                luminanceRange = maximum - minimum,
                blackPixelRatio = (float)(black / count),
                hardWhitePixelRatio = (float)(hardWhite / count),
                magentaPixelRatio = (float)(magenta / count)
            };
        }

        private static void RequireHealthyPixels(PixelAudit audit)
        {
            if (audit.meanLuminance < 0.05f
                || audit.meanLuminance > 0.95f
                || audit.luminanceRange < 0.20f
                || audit.blackPixelRatio >= 0.90f
                || audit.magentaPixelRatio >= 0.005f)
            {
                throw new InvalidOperationException(
                    "The City product frame failed pixel admission: "
                    + $"mean={audit.meanLuminance:0.0000}, "
                    + $"range={audit.luminanceRange:0.0000}, "
                    + $"black={audit.blackPixelRatio:0.000000}, "
                    + $"magenta={audit.magentaPixelRatio:0.000000}.");
            }
        }

        private static ViewportProof BuildViewportProof(
            Camera camera,
            Transform subjectRoot,
            bool preferCharacterControllerBounds)
        {
            Renderer[] renderers = subjectRoot.GetComponentsInChildren<Renderer>(true);
            bool hasBounds = false;
            Bounds bounds = default;
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy)
                {
                    continue;
                }

                if (renderer is not MeshRenderer
                    && renderer is not SkinnedMeshRenderer)
                {
                    continue;
                }

                if (!hasBounds)
                {
                    bounds = renderer.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            if (!hasBounds)
            {
                throw new InvalidOperationException(
                    $"`{subjectRoot.name}` has no active rendered bounds.");
            }

            if (preferCharacterControllerBounds)
            {
                CharacterController characterController =
                    subjectRoot.GetComponentInChildren<CharacterController>(true);
                if (characterController == null
                    || !characterController.enabled
                    || !characterController.gameObject.activeInHierarchy)
                {
                    throw new InvalidOperationException(
                        $"`{subjectRoot.name}` has no active CharacterController framing authority.");
                }

                // Skinned renderer bounds are deliberately conservative and can
                // fluctuate with animation. The validated gameplay capsule is the
                // stable product-space framing authority; active mesh renderers are
                // still required above so an invisible actor cannot pass this gate.
                return ProjectCharacterControllerToViewport(
                    camera,
                    characterController);
            }

            return ProjectBoundsToViewport(camera, bounds, Matrix4x4.identity);
        }

        private static ViewportProof ProjectCharacterControllerToViewport(
            Camera camera,
            CharacterController characterController)
        {
            const int azimuthSamples = 24;
            const int hemisphereSteps = 8;

            float radius = characterController.radius;
            float height = Mathf.Max(characterController.height, radius * 2f);
            float straightHalfHeight = (height * 0.5f) - radius;
            Matrix4x4 localToWorld = characterController.transform.localToWorldMatrix;
            float minX = float.PositiveInfinity;
            float minY = float.PositiveInfinity;
            float maxX = float.NegativeInfinity;
            float maxY = float.NegativeInfinity;
            float minimumDepth = float.PositiveInfinity;

            for (int cap = -1; cap <= 1; cap += 2)
            {
                Vector3 capCenter = characterController.center
                    + (Vector3.up * (straightHalfHeight * cap));
                for (int latitude = 0; latitude <= hemisphereSteps; latitude++)
                {
                    float polar = Mathf.PI * 0.5f * latitude / hemisphereSteps;
                    float horizontal = Mathf.Sin(polar);
                    float vertical = Mathf.Cos(polar) * cap;
                    for (int azimuth = 0; azimuth < azimuthSamples; azimuth++)
                    {
                        float angle = Mathf.PI * 2f * azimuth / azimuthSamples;
                        Vector3 direction = new Vector3(
                            Mathf.Cos(angle) * horizontal,
                            vertical,
                            Mathf.Sin(angle) * horizontal);
                        AccumulateViewportPoint(
                            camera,
                            localToWorld.MultiplyPoint3x4(
                                capCenter + (direction * radius)),
                            ref minX,
                            ref minY,
                            ref maxX,
                            ref maxY,
                            ref minimumDepth);
                    }
                }
            }

            Vector3 center = camera.WorldToViewportPoint(
                localToWorld.MultiplyPoint3x4(characterController.center));
            return CreateViewportProof(
                center,
                minX,
                minY,
                maxX,
                maxY,
                minimumDepth);
        }

        private static ViewportProof ProjectBoundsToViewport(
            Camera camera,
            Bounds bounds,
            Matrix4x4 localToWorld)
        {
            Vector3 minimum = bounds.min;
            Vector3 maximum = bounds.max;
            float minX = float.PositiveInfinity;
            float minY = float.PositiveInfinity;
            float maxX = float.NegativeInfinity;
            float maxY = float.NegativeInfinity;
            float minimumDepth = float.PositiveInfinity;
            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    for (int z = 0; z < 2; z++)
                    {
                        Vector3 localPoint = new Vector3(
                            x == 0 ? minimum.x : maximum.x,
                            y == 0 ? minimum.y : maximum.y,
                            z == 0 ? minimum.z : maximum.z);
                        Vector3 point = localToWorld.MultiplyPoint3x4(localPoint);
                        AccumulateViewportPoint(
                            camera,
                            point,
                            ref minX,
                            ref minY,
                            ref maxX,
                            ref maxY,
                            ref minimumDepth);
                    }
                }
            }

            Vector3 center = camera.WorldToViewportPoint(
                localToWorld.MultiplyPoint3x4(bounds.center));
            return CreateViewportProof(
                center,
                minX,
                minY,
                maxX,
                maxY,
                minimumDepth);
        }

        private static void AccumulateViewportPoint(
            Camera camera,
            Vector3 worldPoint,
            ref float minX,
            ref float minY,
            ref float maxX,
            ref float maxY,
            ref float minimumDepth)
        {
            Vector3 viewport = camera.WorldToViewportPoint(worldPoint);
            minX = Mathf.Min(minX, viewport.x);
            minY = Mathf.Min(minY, viewport.y);
            maxX = Mathf.Max(maxX, viewport.x);
            maxY = Mathf.Max(maxY, viewport.y);
            minimumDepth = Mathf.Min(minimumDepth, viewport.z);
        }

        private static ViewportProof CreateViewportProof(
            Vector3 center,
            float minX,
            float minY,
            float maxX,
            float maxY,
            float minimumDepth)
        {
            return new ViewportProof
            {
                centerX = center.x,
                centerY = center.y,
                minimumDepth = minimumDepth,
                minX = minX,
                minY = minY,
                maxX = maxX,
                maxY = maxY,
                width = maxX - minX,
                height = maxY - minY
            };
        }

        private static void RequireReadableSubject(
            string label,
            ViewportProof proof,
            float minimumViewportY)
        {
            if (!IsReadableSubjectProof(proof, minimumViewportY))
            {
                throw new InvalidOperationException(
                    $"The City {label} is not readable in the product frame: "
                    + JsonUtility.ToJson(proof));
            }
        }

        internal static bool IsReadableSubjectProof(
            ViewportProof proof,
            float minimumViewportY)
        {
            return proof.minimumDepth > 0f
                && proof.centerX > 0.02f
                && proof.centerX < 0.98f
                && proof.centerY > 0.02f
                && proof.centerY < 0.98f
                && proof.minX >= -0.02f
                && proof.maxX <= 1.02f
                && proof.minY >= minimumViewportY
                && proof.maxY <= 1.02f
                && proof.width > 0.005f
                && proof.height > 0.01f;
        }

        private static T FindSingleInScene<T>(Scene scene)
            where T : Component
        {
            T found = null;
            int count = 0;
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                T[] candidates = roots[i].GetComponentsInChildren<T>(true);
                for (int candidateIndex = 0;
                    candidateIndex < candidates.Length;
                    candidateIndex++)
                {
                    T candidate = candidates[candidateIndex];
                    if (candidate == null || candidate.gameObject.scene != scene)
                    {
                        continue;
                    }

                    found = candidate;
                    count++;
                }
            }

            if (count != 1 || found == null)
            {
                throw new InvalidOperationException(
                    $"Expected exactly one `{typeof(T).Name}` in City scene, found {count}.");
            }

            return found;
        }

        private static Canvas[] FindAllInScene<Canvas>(Scene scene)
            where Canvas : Component
        {
            var found = new List<Canvas>();
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                Canvas[] candidates = roots[i].GetComponentsInChildren<Canvas>(true);
                for (int candidateIndex = 0;
                    candidateIndex < candidates.Length;
                    candidateIndex++)
                {
                    Canvas candidate = candidates[candidateIndex];
                    if (candidate != null && candidate.gameObject.scene == scene)
                    {
                        found.Add(candidate);
                    }
                }
            }

            return found.ToArray();
        }

        private static void FinishWithFailure(string failure)
        {
            if (ReadPhase() == RunnerPhase.FailureAwaitingEditMode)
            {
                return;
            }

            SessionState.SetString(
                FailureKey,
                string.IsNullOrWhiteSpace(failure)
                    ? "Unknown City product visual QA failure."
                    : failure);
            SessionState.SetInt(
                PhaseKey,
                (int)RunnerPhase.FailureAwaitingEditMode);
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.ExitPlaymode();
                return;
            }

            FinalizeEditorSession(capturePhasePassed: false);
        }

        private static void FinalizeEditorSession(bool capturePhasePassed)
        {
            if (!SessionState.GetBool(ActiveKey, false))
            {
                return;
            }

            bool exitEditor = SessionState.GetBool(BatchExitKey, false);
            string outputDirectory = SessionState.GetString(
                OutputDirectoryKey,
                string.Empty);
            string setupSnapshot = SessionState.GetString(
                SceneSetupBeforeKey,
                string.Empty);
            var issues = new List<string>();
            VisualQaReport report = null;
            bool success = false;
            SessionState.SetBool(ActiveKey, false);
            try
            {
                try
                {
                    string recordedFailure = SessionState.GetString(
                        FailureKey,
                        string.Empty);
                    if (!string.IsNullOrWhiteSpace(recordedFailure))
                    {
                        issues.Add(recordedFailure.Trim());
                    }

                    string sceneHashBefore = SessionState.GetString(
                        SceneHashBeforeKey,
                        string.Empty);
                    string sceneHashAfter = AssetDatabase.GetAssetDependencyHash(
                        CityHeroPocketSceneSetup.ScenePath).ToString();
                    if (string.IsNullOrWhiteSpace(sceneHashBefore)
                        || !string.Equals(
                            sceneHashBefore,
                            sceneHashAfter,
                            StringComparison.Ordinal))
                    {
                        issues.Add(
                            "The City scene or one of its dependencies changed during visual QA.");
                    }

                    Scene activeScene = SceneManager.GetActiveScene();
                    if (activeScene.IsValid()
                        && string.Equals(
                            activeScene.path,
                            CityHeroPocketSceneSetup.ScenePath,
                            StringComparison.Ordinal)
                        && activeScene.isDirty)
                    {
                        issues.Add("The authored City scene is dirty after visual QA.");
                    }

                    string pngPath = string.IsNullOrWhiteSpace(outputDirectory)
                        ? string.Empty
                        : Path.Combine(
                            outputDirectory,
                            "CITY_G02_PRODUCT_HUDON_QHD.png");
                    if (string.IsNullOrWhiteSpace(pngPath)
                        || !File.Exists(pngPath)
                        || new FileInfo(pngPath).Length < MinimumPngBytes)
                    {
                        issues.Add(
                            "The exact City QHD product-frame PNG is missing or incomplete.");
                    }

                    string draftJson = SessionState.GetString(
                        ReportDraftKey,
                        string.Empty);
                    report = string.IsNullOrWhiteSpace(draftJson)
                        ? null
                        : JsonUtility.FromJson<VisualQaReport>(draftJson);
                    if (report == null)
                    {
                        issues.Add("The City visual QA report draft is missing or invalid.");
                    }
                }
                catch (Exception validationException)
                {
                    Debug.LogException(validationException);
                    issues.Add(
                        "City visual QA postflight threw: "
                        + validationException.Message);
                }

                try
                {
                    RestoreSceneSetupSnapshot(setupSnapshot);
                }
                catch (Exception restoreException)
                {
                    Debug.LogException(restoreException);
                    issues.Add(
                        "The original Editor scene setup was not restored: "
                        + restoreException.Message);
                }

                success = capturePhasePassed && issues.Count == 0 && report != null;
                if (success)
                {
                    try
                    {
                        report.passed = true;
                        string reportPath = Path.Combine(
                            outputDirectory,
                            "city_hero_pocket_visual_qa_report.json");
                        File.WriteAllText(
                            reportPath,
                            JsonUtility.ToJson(report, prettyPrint: true));
                    }
                    catch (Exception reportException)
                    {
                        Debug.LogException(reportException);
                        issues.Add(
                            "The final PASS report could not be written: "
                            + reportException.Message);
                        success = false;
                    }
                }

                if (success)
                {
                    Debug.Log(
                        $"{LogPrefix} PASS output=`{outputDirectory}` "
                        + $"mean={report.meanLuminance:0.0000} "
                        + $"range={report.luminanceRange:0.0000} "
                        + $"black={report.blackPixelRatio:0.000000} "
                        + $"magenta={report.magentaPixelRatio:0.000000}");
                }
                else
                {
                    string finalFailure = issues.Count == 0
                        ? "City product visual QA did not reach a valid success state."
                        : string.Join("\n", issues);
                    TryWriteFailure(
                        outputDirectory,
                        new InvalidOperationException(finalFailure));
                    Debug.LogError($"{LogPrefix} FAIL\n{finalFailure}");
                }
            }
            finally
            {
                ClearSessionState();
                if (exitEditor)
                {
                    EditorApplication.Exit(success ? 0 : 1);
                }
            }
        }

        private static void HandleLaunchFailure(
            Exception exception,
            bool exitEditorWhenFinished)
        {
            Debug.LogException(exception);
            string outputDirectory = SessionState.GetString(
                OutputDirectoryKey,
                string.Empty);
            string setupSnapshot = SessionState.GetString(
                SceneSetupBeforeKey,
                string.Empty);
            if (!string.IsNullOrWhiteSpace(setupSnapshot))
            {
                try
                {
                    RestoreSceneSetupSnapshot(setupSnapshot);
                }
                catch (Exception restoreException)
                {
                    Debug.LogException(restoreException);
                }
            }

            TryWriteFailure(outputDirectory, exception);
            ClearSessionState();
            if (exitEditorWhenFinished)
            {
                EditorApplication.Exit(1);
            }
        }

        private static RunnerPhase ReadPhase()
        {
            return (RunnerPhase)SessionState.GetInt(
                PhaseKey,
                (int)RunnerPhase.None);
        }

        private static bool HasTimedOut()
        {
            string ticksText = SessionState.GetString(
                StartedUtcTicksKey,
                string.Empty);
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
            SessionState.EraseString(OutputDirectoryKey);
            SessionState.EraseString(SceneHashBeforeKey);
            SessionState.EraseString(SceneSetupBeforeKey);
            SessionState.EraseString(ReportDraftKey);
            SessionState.EraseString(StartedUtcTicksKey);
            ResetRuntimeState();
        }

        private static string CaptureSceneSetupSnapshot()
        {
            SceneSetup[] setup = EditorSceneManager.GetSceneManagerSetup();
            var snapshot = new SceneSetupSnapshot
            {
                scenes = new SceneSetupRecord[setup.Length]
            };
            for (int i = 0; i < setup.Length; i++)
            {
                snapshot.scenes[i] = new SceneSetupRecord
                {
                    path = setup[i].path,
                    isLoaded = setup[i].isLoaded,
                    isActive = setup[i].isActive
                };
            }

            return JsonUtility.ToJson(snapshot);
        }

        private static void RestoreSceneSetupSnapshot(string json)
        {
            SceneSetupSnapshot snapshot = string.IsNullOrWhiteSpace(json)
                ? null
                : JsonUtility.FromJson<SceneSetupSnapshot>(json);
            if (snapshot?.scenes == null || snapshot.scenes.Length == 0)
            {
                EditorSceneManager.NewScene(
                    NewSceneSetup.EmptyScene,
                    NewSceneMode.Single);
                return;
            }

            var restorable = new List<SceneSetup>();
            for (int i = 0; i < snapshot.scenes.Length; i++)
            {
                SceneSetupRecord record = snapshot.scenes[i];
                if (string.IsNullOrWhiteSpace(record.path))
                {
                    continue;
                }

                restorable.Add(new SceneSetup
                {
                    path = record.path,
                    isLoaded = record.isLoaded,
                    isActive = record.isActive
                });
            }

            if (restorable.Count == 0)
            {
                EditorSceneManager.NewScene(
                    NewSceneSetup.EmptyScene,
                    NewSceneMode.Single);
                return;
            }

            EditorSceneManager.RestoreSceneManagerSetup(restorable.ToArray());
        }

        private static string ReserveOutputDirectory()
        {
            Directory.CreateDirectory(OutputRoot);
            string baseId = DateTime.UtcNow.ToString(
                "yyyyMMddTHHmmssZ",
                CultureInfo.InvariantCulture)
                + "_city-hero-pocket-product-visual-qa_v001";
            for (int revision = 1; revision <= 999; revision++)
            {
                string id = revision == 1
                    ? baseId
                    : $"{baseId}_r{revision:000}";
                string candidate = Path.Combine(OutputRoot, id);
                if (Directory.Exists(candidate) || File.Exists(candidate))
                {
                    continue;
                }

                Directory.CreateDirectory(candidate);
                return candidate;
            }

            throw new IOException(
                "Could not reserve a create-new City visual QA output directory.");
        }

        private static void RefuseDirtyOpenScenes()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.IsValid() && scene.isDirty)
                {
                    throw new InvalidOperationException(
                        $"Refusing visual QA while open scene `{scene.path}` is dirty.");
                }
            }
        }

        private static string ComputeSha256(string path)
        {
            using FileStream stream = File.OpenRead(path);
            using SHA256 sha256 = SHA256.Create();
            return BitConverter.ToString(sha256.ComputeHash(stream))
                .Replace("-", string.Empty)
                .ToLowerInvariant();
        }

        private static void TryWriteFailure(
            string outputDirectory,
            Exception exception)
        {
            if (string.IsNullOrWhiteSpace(outputDirectory))
            {
                return;
            }

            try
            {
                File.WriteAllText(
                    Path.Combine(outputDirectory, "visual_qa_failure.txt"),
                    exception.ToString());
            }
            catch (Exception writeException)
            {
                Debug.LogException(writeException);
            }
        }

        private readonly struct CanvasCaptureState
        {
            private readonly Canvas canvas;
            private readonly RenderMode renderMode;
            private readonly Camera worldCamera;
            private readonly float planeDistance;
            private readonly float scaleFactor;
            private readonly bool scalerEnabled;

            public readonly CanvasScaler scaler;

            public CanvasCaptureState(Canvas canvas)
            {
                this.canvas = canvas;
                renderMode = canvas.renderMode;
                worldCamera = canvas.worldCamera;
                planeDistance = canvas.planeDistance;
                scaleFactor = canvas.scaleFactor;
                scaler = canvas.GetComponent<CanvasScaler>();
                scalerEnabled = scaler != null && scaler.enabled;
            }

            public void Restore()
            {
                if (canvas == null)
                {
                    return;
                }

                canvas.renderMode = renderMode;
                canvas.worldCamera = worldCamera;
                canvas.planeDistance = planeDistance;
                canvas.scaleFactor = scaleFactor;
                if (scaler != null)
                {
                    scaler.enabled = scalerEnabled;
                }
            }
        }

        [Serializable]
        private struct PixelAudit
        {
            public float meanLuminance;
            public float luminanceRange;
            public float blackPixelRatio;
            public float hardWhitePixelRatio;
            public float magentaPixelRatio;
        }

        [Serializable]
        public struct ViewportProof
        {
            public float centerX;
            public float centerY;
            public float minimumDepth;
            public float minX;
            public float minY;
            public float maxX;
            public float maxY;
            public float width;
            public float height;
        }

        [Serializable]
        private sealed class VisualQaReport
        {
            public bool passed;
            public string capturedUtc;
            public string unityVersion;
            public string scenePath;
            public string sceneDependencyHash;
            public int width;
            public int height;
            public string pngFile;
            public long pngBytes;
            public string pngSha256;
            public float meanLuminance;
            public float luminanceRange;
            public float blackPixelRatio;
            public float hardWhitePixelRatio;
            public float magentaPixelRatio;
            public ViewportProof player;
            public ViewportProof enemy;
        }

        [Serializable]
        private sealed class SceneSetupSnapshot
        {
            public SceneSetupRecord[] scenes;
        }

        [Serializable]
        private struct SceneSetupRecord
        {
            public string path;
            public bool isLoaded;
            public bool isActive;
        }
    }
}
