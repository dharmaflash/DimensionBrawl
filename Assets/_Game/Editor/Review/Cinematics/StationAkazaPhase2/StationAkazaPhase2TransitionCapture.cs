using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using DimensionBrawl.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;

namespace DimensionBrawl.Editor.Review.Cinematics
{
    /// <summary>
    /// Graphics-enabled, editor-only evidence capture for the authored Station
    /// C33 wing-deploy -> C34 eye-open phase transition. This utility only reads
    /// product assets and never saves the temporary scene state used for capture.
    /// </summary>
    public static class StationAkazaPhase2TransitionCapture
    {
        public const string StationScenePath =
            "Assets/_Game/Scenes/OlympusStationCombatStage.unity";
        public const string OutputRoot =
            "C:/tmp/DimensionBrawl-StationAkazaPhase2Intro";

        private const string TransitionRootName = "OlympusStation_AkazaPhase2TransitionRig";
        private const string ActorName = "AkazaPhase2_CinematicActor";
        private const string GameplayVisualName = "OlympusStation_AkazaPhase2GameplayVisual";
        private const string PhaseOneVisualName =
            "BossBarrageLaneReview_HumanoidBossVisual_SciFiSoldier_01_Commando";
        private const string DirectorName = "AkazaPhase2_MasterPlayableDirector";
        private const string WingCameraRigName = "C33_WingDeployCameraRig";
        private const string EyeCameraRigName = "C34_EyeOpenCameraRig";
        private const string EyeRendererName = "CHakazaA:eyeBall";
        private const double CameraSwitchSeconds = 1.60d;
        private const double MasterDurationSeconds = 3.9666667d;
        private const int CaptureFps = 30;
        private const int FrameWidth = 640;
        private const int FrameHeight = 360;
        private const int KeyWidth = 960;
        private const int KeyHeight = 540;
        private const int GameplayProofLayer = 31;
        private const int MinimumGameplayProofPixels = 6000;
        private const float WingClosedSampleSeconds = 0.30f;
        private const float WingOpenSampleSeconds = 1.10f;
        private const float MinimumWingSpanGrowth = 0.20f;
        private const float MinimumWingSpanRatio = 1.04f;
        private const float EyeClosedSampleSeconds = 1.716667f;
        private const float EyeOpenSampleSeconds = 2.966667f;
        private const int MinimumOpenIrisPixels = 24;
        private const int MinimumIrisPixelGrowth = 8;
        private const float TerminalContinuitySampleSeconds = 3.933333f;
        private const float MaximumGameplayHorizontalSnap = 0.35f;
        private const float MaximumGameplayFloorSnap = 0.25f;
        private const float MinimumGameplayHeightRatio = 0.70f;
        private const float MaximumGameplayHeightRatio = 1.35f;
        private const float MaximumGameplayFacingSnapDegrees = 5f;

        private static readonly string[] WingRendererNames =
        {
            "CHakazaA:akWp_BladeA_geo",
            "CHakazaA:akWp_BladeB_geo",
            "CHakazaA:akWp_BladeC_geo",
            "CHakazaA:akWp_BladeD_geo",
            "CHakazaA:akWp_BladeE_geo",
            "CHakazaA:akWp_BladeF_geo"
        };

        private static readonly KeyBeat[] KeyBeats =
        {
            new KeyBeat("c33_pre_deploy", 0.033333f),
            new KeyBeat("c33_wing_span_start", WingClosedSampleSeconds),
            new KeyBeat("c33_wing_mid", 0.733333f),
            new KeyBeat("c33_wing_span_open", WingOpenSampleSeconds),
            new KeyBeat("c33_settle", 1.566667f),
            new KeyBeat("c34_cut_closed", 1.600000f),
            new KeyBeat("c34_closed_regression", EyeClosedSampleSeconds),
            new KeyBeat("c34_eye_slit", 2.216667f),
            new KeyBeat("c34_eye_open", EyeOpenSampleSeconds),
            new KeyBeat("c34_push_in_settle", 3.896667f)
        };

        private static string FrameDirectory => Path.Combine(OutputRoot, "Frames30fps");
        private static string KeyFrameDirectory => Path.Combine(OutputRoot, "KeyFrames");
        private static string ContactSheetPath => Path.Combine(OutputRoot, "ContactSheet.png");
        private static string MetricsPath => Path.Combine(OutputRoot, "Metrics.csv");
        private static string ReportPath => Path.Combine(OutputRoot, "README.md");

        [MenuItem("DimensionBrawl/Review/Cinematics/Capture Station Akaza Phase 2 Intro")]
        public static void CaptureMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            CaptureProductTransition();
            Debug.Log($"Station Akaza phase-two transition evidence written to {OutputRoot}.");
        }

        /// <summary>
        /// Unity batch entry point. Invoke with -batchmode -noaudio and without
        /// -nographics so Camera.Render and the pixel regressions remain valid.
        /// </summary>
        public static void RunBatchCapture()
        {
            try
            {
                RequireNoAudioCommandLine();
                CaptureProductTransition();
                Debug.Log("Station Akaza phase-two transition capture passed.");
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        public static void CaptureProductTransition()
        {
            Scene scene = EditorSceneManager.OpenScene(StationScenePath, OpenSceneMode.Single);
            GameObject transitionRoot = RequireRoot(scene, TransitionRootName);
            GameObject actor = RequireDescendant(transitionRoot, ActorName);
            GameObject gameplayVisual = RequireSceneObject(scene, GameplayVisualName);
            GameObject phaseOneVisual = RequireSceneObject(scene, PhaseOneVisualName);
            PlayableDirector director = RequireDescendant(transitionRoot, DirectorName)
                .GetComponent<PlayableDirector>()
                ?? throw new InvalidOperationException(
                    $"{DirectorName} has no PlayableDirector component.");
            Camera wingCamera = RequireCamera(transitionRoot, WingCameraRigName);
            Camera eyeCamera = RequireCamera(transitionRoot, EyeCameraRigName);
            AkazaPhase2CinematicLookDriver lookDriver =
                transitionRoot.GetComponent<AkazaPhase2CinematicLookDriver>()
                ?? throw new InvalidOperationException(
                    "Station phase-two transition requires its cinematic look driver.");
            if (wingCamera == eyeCamera)
            {
                throw new InvalidOperationException("C33 and C34 must use distinct authored cameras.");
            }

            TimelineAsset timeline = director.playableAsset as TimelineAsset
                ?? throw new InvalidOperationException(
                    "Station Akaza phase-two PlayableDirector is not bound to a TimelineAsset.");
            ValidateTimeline(timeline);

            Renderer[] wingRenderers = RequireWingRenderers(actor);
            Renderer eyeRenderer = actor.GetComponentsInChildren<Renderer>(includeInactive: true)
                .SingleOrDefault(renderer => string.Equals(
                    renderer.gameObject.name,
                    EyeRendererName,
                    StringComparison.Ordinal))
                ?? throw new InvalidOperationException(
                    $"Cinematic Akaza is missing the required {EyeRendererName} renderer.");
            SkinnedMeshRenderer[] skinnedMeshes =
                actor.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true);
            if (skinnedMeshes.Length == 0)
            {
                throw new InvalidOperationException(
                    "Cinematic Akaza has no SkinnedMeshRenderers for offline capture.");
            }

            Camera[] sceneCameras = FindSceneComponents<Camera>(scene);
            AudioSource[] audioSources = FindSceneComponents<AudioSource>(scene);
            bool transitionWasActive = transitionRoot.activeSelf;
            bool phaseOneWasActive = phaseOneVisual.activeSelf;
            bool[] cameraEnabled = sceneCameras.Select(camera => camera.enabled).ToArray();
            bool[] updateWhenOffscreen =
                skinnedMeshes.Select(renderer => renderer.updateWhenOffscreen).ToArray();
            bool[] forceMatrixRecalculation =
                skinnedMeshes.Select(renderer => renderer.forceMatrixRecalculationPerRender).ToArray();
            bool[] sourceMute = audioSources.Select(source => source.mute).ToArray();
            float listenerVolume = AudioListener.volume;
            bool listenerPaused = AudioListener.pause;
            double directorTime = director.time;
            List<Texture2D> keyFrames = new List<Texture2D>(KeyBeats.Length);

            try
            {
                AudioListener.volume = 0f;
                AudioListener.pause = true;
                foreach (AudioSource source in audioSources)
                {
                    source.mute = true;
                }

                // Mirror the runtime presentation lease. Leaving the Phase 1
                // Commando visible occludes the exact C33/C34 Akaza cameras and
                // produces a false capture that can never occur in gameplay.
                phaseOneVisual.SetActive(false);
                transitionRoot.SetActive(true);
                lookDriver.BeginManualLightingLease();
                foreach (Camera camera in sceneCameras)
                {
                    camera.enabled = false;
                }

                foreach (SkinnedMeshRenderer renderer in skinnedMeshes)
                {
                    // Same-tick PlayableDirector.Evaluate + Camera.Render otherwise
                    // risks reusing an earlier deformation in editor batch mode.
                    renderer.updateWhenOffscreen = true;
                    renderer.forceMatrixRecalculationPerRender = true;
                }

                director.Stop();
                director.RebuildGraph();
                ValidateDirectorDuration(director);
                ResetOutputRoot();

                float wingStartSpan = -1f;
                float wingOpenSpan = -1f;
                int closedIrisPixels = -1;
                int openIrisPixels = -1;
                RectInt closedIrisRegion = default;
                RectInt openIrisRegion = default;

                for (int index = 0; index < KeyBeats.Length; index++)
                {
                    KeyBeat beat = KeyBeats[index];
                    Camera activeCamera = SampleDirector(
                        director,
                        wingCamera,
                        eyeCamera,
                        beat.Seconds);
                    Texture2D frame = CaptureCamera(activeCamera, KeyWidth, KeyHeight);
                    keyFrames.Add(frame);
                    File.WriteAllBytes(
                        Path.Combine(
                            KeyFrameDirectory,
                            FormattableString.Invariant(
                                $"{index:00}_{beat.Label}_t-{beat.Seconds:0.000000}s.png")),
                        frame.EncodeToPNG());

                    if (Approximately(beat.Seconds, WingClosedSampleSeconds))
                    {
                        wingStartSpan = MeasureVerticalWingSpan(actor, wingRenderers);
                    }
                    else if (Approximately(beat.Seconds, WingOpenSampleSeconds))
                    {
                        wingOpenSpan = MeasureVerticalWingSpan(actor, wingRenderers);
                    }

                    if (Approximately(beat.Seconds, EyeClosedSampleSeconds))
                    {
                        closedIrisPixels = CountTurquoiseIrisPixels(
                            frame,
                            activeCamera,
                            eyeRenderer,
                            out closedIrisRegion);
                    }
                    else if (Approximately(beat.Seconds, EyeOpenSampleSeconds))
                    {
                        openIrisPixels = CountTurquoiseIrisPixels(
                            frame,
                            activeCamera,
                            eyeRenderer,
                            out openIrisRegion);
                    }
                }

                ValidateWingExpansion(wingStartSpan, wingOpenSpan);
                ValidateIrisGrowth(closedIrisPixels, openIrisPixels);
                ValidateExactCameraCut(director, wingCamera, eyeCamera);
                SampleDirector(
                    director,
                    wingCamera,
                    eyeCamera,
                    TerminalContinuitySampleSeconds);
                GameplayContinuity gameplayContinuity = ValidateTerminalGameplayPose(
                    actor,
                    gameplayVisual);
                CaptureGameplayProofFrames(scene, actor, gameplayVisual, keyFrames);

                Texture2D contactSheet = BuildContactSheet(
                    keyFrames,
                    tileWidth: 320,
                    tileHeight: 180,
                    columns: 4);
                try
                {
                    File.WriteAllBytes(ContactSheetPath, contactSheet.EncodeToPNG());
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(contactSheet);
                }

                int frameCount = CaptureFrameSequence(director, wingCamera, eyeCamera);
                WriteMetrics(
                    wingStartSpan,
                    wingOpenSpan,
                    closedIrisPixels,
                    openIrisPixels,
                    closedIrisRegion,
                    openIrisRegion,
                    gameplayContinuity);
                WriteReport(
                    timeline,
                    frameCount,
                    skinnedMeshes.Length,
                    wingStartSpan,
                    wingOpenSpan,
                    closedIrisPixels,
                    openIrisPixels,
                    gameplayContinuity);
            }
            finally
            {
                for (int index = 0; index < keyFrames.Count; index++)
                {
                    UnityEngine.Object.DestroyImmediate(keyFrames[index]);
                }

                for (int index = 0; index < skinnedMeshes.Length; index++)
                {
                    if (skinnedMeshes[index] == null)
                    {
                        continue;
                    }

                    skinnedMeshes[index].updateWhenOffscreen = updateWhenOffscreen[index];
                    skinnedMeshes[index].forceMatrixRecalculationPerRender =
                        forceMatrixRecalculation[index];
                }

                for (int index = 0; index < sceneCameras.Length; index++)
                {
                    if (sceneCameras[index] != null)
                    {
                        sceneCameras[index].enabled = cameraEnabled[index];
                    }
                }

                for (int index = 0; index < audioSources.Length; index++)
                {
                    if (audioSources[index] != null)
                    {
                        audioSources[index].mute = sourceMute[index];
                    }
                }

                AudioListener.volume = listenerVolume;
                AudioListener.pause = listenerPaused;
                if (director != null && director.playableAsset != null)
                {
                    director.time = Math.Max(
                        0d,
                        Math.Min(director.duration, directorTime));
                    director.Evaluate();
                    director.Pause();
                }

                if (transitionRoot != null)
                {
                    lookDriver.EndManualLightingLease();
                    transitionRoot.SetActive(transitionWasActive);
                }

                if (phaseOneVisual != null)
                {
                    phaseOneVisual.SetActive(phaseOneWasActive);
                }

                // Discard all temporary active/camera/skinning state and leave the
                // product scene clean. This utility deliberately never saves it.
                EditorSceneManager.OpenScene(StationScenePath, OpenSceneMode.Single);
            }
        }

        private static int CaptureFrameSequence(
            PlayableDirector director,
            Camera wingCamera,
            Camera eyeCamera)
        {
            int frameCount = Mathf.RoundToInt((float)MasterDurationSeconds * CaptureFps);
            if (frameCount != 119)
            {
                throw new InvalidOperationException(
                    $"Expected 119 frames for the 3.9667s/30fps master, got {frameCount}.");
            }

            for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
            {
                float seconds = frameIndex / (float)CaptureFps;
                Camera activeCamera = SampleDirector(
                    director,
                    wingCamera,
                    eyeCamera,
                    seconds);
                Texture2D frame = CaptureCamera(activeCamera, FrameWidth, FrameHeight);
                try
                {
                    File.WriteAllBytes(
                        Path.Combine(FrameDirectory, $"frame_{frameIndex:0000}.png"),
                        frame.EncodeToPNG());
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(frame);
                }
            }

            int actualCount = Directory.GetFiles(FrameDirectory, "frame_*.png").Length;
            if (actualCount != frameCount)
            {
                throw new InvalidOperationException(
                    $"Expected {frameCount} 30fps PNG frames, found {actualCount}.");
            }

            File.WriteAllLines(Path.Combine(FrameDirectory, "README.txt"), new[]
            {
                $"Frames: {frameCount}",
                $"Frame rate: {CaptureFps} fps",
                $"Master duration: {MasterDurationSeconds:0.0000000} seconds",
                $"Resolution: {FrameWidth}x{FrameHeight}",
                $"Camera cut: C33 -> C34 at {CameraSwitchSeconds:0.000000} seconds",
                "Audio: none",
                "Range: frame_0000.png through frame_0118.png",
                "ffmpeg -framerate 30 -i frame_%04d.png -c:v libx264 -pix_fmt yuv420p StationAkazaPhase2Intro.mp4"
            });
            return frameCount;
        }

        private static Camera SampleDirector(
            PlayableDirector director,
            Camera wingCamera,
            Camera eyeCamera,
            double seconds)
        {
            if (seconds < -0.0001d || seconds > MasterDurationSeconds + 0.0001d)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(seconds),
                    $"Sample {seconds:0.000000}s is outside the phase-transition master.");
            }

            director.time = Math.Max(0d, Math.Min(MasterDurationSeconds, seconds));
            director.Evaluate();
            director.GetComponentInParent<AkazaPhase2CinematicLookDriver>()
                ?.ApplyCurrentTime();
            Physics.SyncTransforms();

            bool showWing = seconds < CameraSwitchSeconds;
            wingCamera.enabled = showWing;
            eyeCamera.enabled = !showWing;
            if (wingCamera.enabled == eyeCamera.enabled)
            {
                throw new InvalidOperationException(
                    $"Camera exclusivity failed at {seconds:0.000000}s.");
            }

            return showWing ? wingCamera : eyeCamera;
        }

        private static void ValidateExactCameraCut(
            PlayableDirector director,
            Camera wingCamera,
            Camera eyeCamera)
        {
            Camera before = SampleDirector(
                director,
                wingCamera,
                eyeCamera,
                CameraSwitchSeconds - (1d / CaptureFps));
            Camera atCut = SampleDirector(
                director,
                wingCamera,
                eyeCamera,
                CameraSwitchSeconds);
            if (before != wingCamera || atCut != eyeCamera)
            {
                throw new InvalidOperationException(
                    "The 30fps master must show C33 on frame 47 and C34 on frame 48 at exactly 1.600000s.");
            }
        }

        private static float MeasureVerticalWingSpan(
            GameObject actor,
            IReadOnlyList<Renderer> wingRenderers)
        {
            float minimumY = float.PositiveInfinity;
            float maximumY = float.NegativeInfinity;
            for (int rendererIndex = 0; rendererIndex < wingRenderers.Count; rendererIndex++)
            {
                Renderer renderer = wingRenderers[rendererIndex];
                if (!renderer.enabled || !renderer.gameObject.activeInHierarchy)
                {
                    throw new InvalidOperationException(
                        $"Wing renderer {renderer.gameObject.name} is not visible during C33.");
                }

                Bounds bounds = renderer.bounds;
                for (int corner = 0; corner < 8; corner++)
                {
                    Vector3 worldCorner = bounds.center + Vector3.Scale(
                        bounds.extents,
                        new Vector3(
                            (corner & 1) == 0 ? -1f : 1f,
                            (corner & 2) == 0 ? -1f : 1f,
                            (corner & 4) == 0 ? -1f : 1f));
                    float actorSpaceY = actor.transform.InverseTransformPoint(worldCorner).y;
                    minimumY = Mathf.Min(minimumY, actorSpaceY);
                    maximumY = Mathf.Max(maximumY, actorSpaceY);
                }
            }

            float span = maximumY - minimumY;
            if (!IsFinite(span) || span <= 0.001f)
            {
                throw new InvalidOperationException(
                    $"C33 wing renderer span is invalid: {span}.");
            }

            return span;
        }

        private static void ValidateWingExpansion(float startSpan, float openSpan)
        {
            if (!IsFinite(startSpan) || !IsFinite(openSpan) || startSpan <= 0f)
            {
                throw new InvalidOperationException(
                    "C33 wing expansion samples were not captured at +0.300s and +1.100s.");
            }

            float growth = openSpan - startSpan;
            float ratio = openSpan / startSpan;
            if (growth < MinimumWingSpanGrowth || ratio < MinimumWingSpanRatio)
            {
                throw new InvalidOperationException(
                    "C33 wing-deploy regression failed: the six floating blades must expand "
                    + $"vertically between +{WingClosedSampleSeconds:0.000}s and "
                    + $"+{WingOpenSampleSeconds:0.000}s. start={startSpan:0.0000}, "
                    + $"open={openSpan:0.0000}, growth={growth:0.0000}, ratio={ratio:0.0000}.");
            }
        }

        private static int CountTurquoiseIrisPixels(
            Texture2D frame,
            Camera camera,
            Renderer eyeRenderer,
            out RectInt region)
        {
            region = CalculateRendererPixelRegion(frame, camera, eyeRenderer, expansion: 0.35f);
            Color32[] pixels = frame.GetPixels32();
            int count = 0;
            for (int y = region.yMin; y < region.yMax; y++)
            {
                int row = y * frame.width;
                for (int x = region.xMin; x < region.xMax; x++)
                {
                    Color32 pixel = pixels[row + x];
                    if (pixel.r <= 80
                        && pixel.g >= 90
                        && pixel.b >= 90
                        && pixel.g >= pixel.r + 40
                        && pixel.b >= pixel.r + 40)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        private static RectInt CalculateRendererPixelRegion(
            Texture2D frame,
            Camera camera,
            Renderer renderer,
            float expansion)
        {
            Bounds bounds = renderer.bounds;
            float minimumU = float.PositiveInfinity;
            float minimumV = float.PositiveInfinity;
            float maximumU = float.NegativeInfinity;
            float maximumV = float.NegativeInfinity;
            int visibleCorners = 0;
            for (int corner = 0; corner < 8; corner++)
            {
                Vector3 worldCorner = bounds.center + Vector3.Scale(
                    bounds.extents,
                    new Vector3(
                        (corner & 1) == 0 ? -1f : 1f,
                        (corner & 2) == 0 ? -1f : 1f,
                        (corner & 4) == 0 ? -1f : 1f));
                Vector3 viewport = camera.WorldToViewportPoint(worldCorner);
                if (viewport.z <= 0f)
                {
                    continue;
                }

                visibleCorners++;
                minimumU = Mathf.Min(minimumU, viewport.x);
                minimumV = Mathf.Min(minimumV, viewport.y);
                maximumU = Mathf.Max(maximumU, viewport.x);
                maximumV = Mathf.Max(maximumV, viewport.y);
            }

            if (visibleCorners == 0)
            {
                throw new InvalidOperationException(
                    $"{EyeRendererName} is behind the C34 camera.");
            }

            float marginU = Mathf.Max(0.005f, (maximumU - minimumU) * expansion);
            float marginV = Mathf.Max(0.005f, (maximumV - minimumV) * expansion);
            minimumU = Mathf.Clamp01(minimumU - marginU);
            minimumV = Mathf.Clamp01(minimumV - marginV);
            maximumU = Mathf.Clamp01(maximumU + marginU);
            maximumV = Mathf.Clamp01(maximumV + marginV);

            int xMin = Mathf.Clamp(Mathf.FloorToInt(minimumU * frame.width), 0, frame.width - 1);
            int yMin = Mathf.Clamp(Mathf.FloorToInt(minimumV * frame.height), 0, frame.height - 1);
            int xMax = Mathf.Clamp(Mathf.CeilToInt(maximumU * frame.width), xMin + 1, frame.width);
            int yMax = Mathf.Clamp(Mathf.CeilToInt(maximumV * frame.height), yMin + 1, frame.height);
            RectInt pixelRegion = new RectInt(xMin, yMin, xMax - xMin, yMax - yMin);
            if (pixelRegion.width < 4 || pixelRegion.height < 4)
            {
                throw new InvalidOperationException(
                    $"C34 eye pixel region is too small: {pixelRegion}.");
            }

            return pixelRegion;
        }

        private static void ValidateIrisGrowth(int closedPixels, int openPixels)
        {
            if (closedPixels < 0 || openPixels < 0)
            {
                throw new InvalidOperationException(
                    "C34 eye samples were not captured at the closed and open regression times.");
            }

            int requiredOpenPixels = Math.Max(
                MinimumOpenIrisPixels,
                closedPixels + MinimumIrisPixelGrowth);
            if (openPixels < requiredOpenPixels)
            {
                throw new InvalidOperationException(
                    "C34 rendered-eye regression failed: turquoise iris evidence inside the "
                    + $"projected eye region must grow from closed to open. closed={closedPixels}, "
                    + $"open={openPixels}, requiredOpen={requiredOpenPixels}.");
            }
        }

        private static GameplayContinuity ValidateTerminalGameplayPose(
            GameObject cinematicActor,
            GameObject gameplayVisual)
        {
            bool gameplayWasActive = gameplayVisual.activeSelf;
            SkinnedMeshRenderer[] gameplayMeshes =
                gameplayVisual.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true);
            bool[] updateWhenOffscreen =
                gameplayMeshes.Select(renderer => renderer.updateWhenOffscreen).ToArray();
            bool[] forceMatrixRecalculation = gameplayMeshes
                .Select(renderer => renderer.forceMatrixRecalculationPerRender)
                .ToArray();
            try
            {
                gameplayVisual.SetActive(true);
                foreach (SkinnedMeshRenderer renderer in gameplayMeshes)
                {
                    renderer.updateWhenOffscreen = true;
                    renderer.forceMatrixRecalculationPerRender = true;
                }

                Animator gameplayAnimator = gameplayVisual.GetComponentInChildren<Animator>(
                    includeInactive: true);
                if (gameplayAnimator == null || gameplayAnimator.runtimeAnimatorController == null)
                {
                    throw new InvalidOperationException(
                        "Gameplay Akaza requires its deployed-pose Animator for terminal continuity.");
                }

                gameplayAnimator.Rebind();
                gameplayAnimator.Play("Hover", 0, 0f);
                gameplayAnimator.Update(0f);

                Physics.SyncTransforms();
                Bounds cinematicBounds = CalculateBodyBounds(cinematicActor);
                Bounds gameplayBounds = CalculateBodyBounds(gameplayVisual);
                Bounds cinematicStructureBounds = CalculateStructureBounds(cinematicActor);
                Bounds gameplayStructureBounds = CalculateStructureBounds(gameplayVisual);
                ValidateStructureContinuity(
                    cinematicStructureBounds,
                    gameplayStructureBounds);
                Transform cinematicFacing = RequireDescendant(
                    cinematicActor,
                    "CHakazaA:world_trs").transform;
                Transform gameplayFacing = RequireDescendant(
                    gameplayVisual,
                    "CHakazaA:world_trs").transform;
                float horizontalDelta = Vector2.Distance(
                    new Vector2(cinematicBounds.center.x, cinematicBounds.center.z),
                    new Vector2(gameplayBounds.center.x, gameplayBounds.center.z));
                float floorDelta = Mathf.Abs(cinematicBounds.min.y - gameplayBounds.min.y);
                float heightRatio = gameplayBounds.size.y / cinematicBounds.size.y;
                float facingDelta = Quaternion.Angle(
                    cinematicFacing.rotation,
                    gameplayFacing.rotation);
                GameplayContinuity result = new GameplayContinuity(
                    horizontalDelta,
                    floorDelta,
                    heightRatio,
                    facingDelta,
                    cinematicBounds,
                    gameplayBounds);

                if (horizontalDelta > MaximumGameplayHorizontalSnap
                    || floorDelta > MaximumGameplayFloorSnap
                    || heightRatio < MinimumGameplayHeightRatio
                    || heightRatio > MaximumGameplayHeightRatio
                    || facingDelta > MaximumGameplayFacingSnapDegrees)
                {
                    throw new InvalidOperationException(
                        "Terminal gameplay-pose continuity failed at the last 30fps C34 sample: "
                        + $"horizontalSnap={horizontalDelta:0.0000}m "
                        + $"(max {MaximumGameplayHorizontalSnap:0.00}), "
                        + $"floorSnap={floorDelta:0.0000}m "
                        + $"(max {MaximumGameplayFloorSnap:0.00}), "
                        + $"heightRatio={heightRatio:0.0000} "
                        + $"(range {MinimumGameplayHeightRatio:0.00}-{MaximumGameplayHeightRatio:0.00}), "
                        + $"facingSnap={facingDelta:0.0000}deg "
                        + $"(max {MaximumGameplayFacingSnapDegrees:0.0}). "
                        + $"cinematicRenderers=[{DescribeActiveBodyBounds(cinematicActor)}], "
                        + $"gameplayRenderers=[{DescribeActiveBodyBounds(gameplayVisual)}].");
                }

                return result;
            }
            finally
            {
                for (int index = 0; index < gameplayMeshes.Length; index++)
                {
                    if (gameplayMeshes[index] == null)
                    {
                        continue;
                    }

                    gameplayMeshes[index].updateWhenOffscreen = updateWhenOffscreen[index];
                    gameplayMeshes[index].forceMatrixRecalculationPerRender =
                        forceMatrixRecalculation[index];
                }

                gameplayVisual.SetActive(gameplayWasActive);
            }
        }

        private static Bounds CalculateBodyBounds(GameObject root)
        {
            SkinnedMeshRenderer[] bodyRenderers = root
                .GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true)
                .Where(renderer => renderer.enabled
                    && renderer.gameObject.activeInHierarchy
                    && !IsWingStructureRenderer(renderer.gameObject.name))
                .ToArray();
            if (bodyRenderers.Length == 0)
            {
                throw new InvalidOperationException(
                    $"{root.name} has no active body SkinnedMeshRenderers for handoff continuity.");
            }

            Bounds bounds = CalculateBakedWorldBounds(bodyRenderers);

            if (!IsFinite(bounds.size.y) || bounds.size.y <= 0.001f)
            {
                throw new InvalidOperationException(
                    $"{root.name} body bounds are invalid for handoff continuity: {bounds}.");
            }

            return bounds;
        }

        private static Bounds CalculateStructureBounds(GameObject root)
        {
            SkinnedMeshRenderer[] renderers = root
                .GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true)
                .Where(renderer => renderer.enabled
                    && renderer.gameObject.activeInHierarchy
                    && IsWingStructureRenderer(renderer.gameObject.name))
                .ToArray();
            if (renderers.Length == 0)
            {
                throw new InvalidOperationException(
                    $"{root.name} has no active six-wing structure renderers.");
            }

            return CalculateBakedWorldBounds(renderers);
        }

        private static Bounds CalculateBakedWorldBounds(
            IReadOnlyCollection<SkinnedMeshRenderer> renderers)
        {
            bool hasBounds = false;
            Bounds combined = default;
            foreach (SkinnedMeshRenderer renderer in renderers)
            {
                if (renderer == null || renderer.sharedMesh == null)
                {
                    continue;
                }

                Mesh baked = new Mesh();
                try
                {
                    renderer.BakeMesh(baked, useScale: false);
                    if (baked.vertexCount == 0)
                    {
                        continue;
                    }

                    Bounds world = TransformBounds(
                        baked.bounds,
                        renderer.transform.localToWorldMatrix);
                    if (!hasBounds)
                    {
                        combined = world;
                        hasBounds = true;
                    }
                    else
                    {
                        combined.Encapsulate(world.min);
                        combined.Encapsulate(world.max);
                    }
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(baked);
                }
            }

            if (!hasBounds)
            {
                throw new InvalidOperationException(
                    "Akaza render proof could not bake any active skinned geometry.");
            }

            return combined;
        }

        private static Bounds TransformBounds(Bounds localBounds, Matrix4x4 localToWorld)
        {
            Vector3 center = localToWorld.MultiplyPoint3x4(localBounds.center);
            Vector3 extents = localBounds.extents;
            Vector3 axisX = localToWorld.MultiplyVector(new Vector3(extents.x, 0f, 0f));
            Vector3 axisY = localToWorld.MultiplyVector(new Vector3(0f, extents.y, 0f));
            Vector3 axisZ = localToWorld.MultiplyVector(new Vector3(0f, 0f, extents.z));
            return new Bounds(
                center,
                new Vector3(
                    Mathf.Abs(axisX.x) + Mathf.Abs(axisY.x) + Mathf.Abs(axisZ.x),
                    Mathf.Abs(axisX.y) + Mathf.Abs(axisY.y) + Mathf.Abs(axisZ.y),
                    Mathf.Abs(axisX.z) + Mathf.Abs(axisY.z) + Mathf.Abs(axisZ.z)) * 2f);
        }

        private static void ValidateStructureContinuity(Bounds cinematic, Bounds gameplay)
        {
            float centerDelta = Vector3.Distance(cinematic.center, gameplay.center);
            Vector3 sizeRatio = new Vector3(
                gameplay.size.x / Mathf.Max(0.0001f, cinematic.size.x),
                gameplay.size.y / Mathf.Max(0.0001f, cinematic.size.y),
                gameplay.size.z / Mathf.Max(0.0001f, cinematic.size.z));
            bool sizeMatches = sizeRatio.x >= 0.98f && sizeRatio.x <= 1.02f
                && sizeRatio.y >= 0.98f && sizeRatio.y <= 1.02f
                && sizeRatio.z >= 0.98f && sizeRatio.z <= 1.02f;
            if (centerDelta > 0.05f || !sizeMatches)
            {
                throw new InvalidOperationException(
                    "Merged gameplay six-wing geometry does not match the authored C34 terminal pose: "
                    + $"centerDelta={centerDelta:0.0000}m, sizeRatio={sizeRatio}, "
                    + $"cinematic={cinematic}, gameplay={gameplay}.");
            }
        }

        private static bool IsWingStructureRenderer(string rendererName)
        {
            return string.Equals(
                    rendererName,
                    "CHakazaA:BackParts",
                    StringComparison.Ordinal)
                || rendererName.StartsWith("CHakazaA:akArm", StringComparison.Ordinal)
                || rendererName.StartsWith("CHakazaA:akWp_", StringComparison.Ordinal);
        }

        private static string DescribeActiveBodyBounds(GameObject root)
        {
            return string.Join(
                "; ",
                root.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true)
                    .Where(renderer => renderer.enabled
                        && renderer.gameObject.activeInHierarchy
                        && !IsWingStructureRenderer(renderer.gameObject.name))
                    .Select(renderer => FormattableString.Invariant(
                        $"{renderer.gameObject.name}:center={renderer.bounds.center},size={renderer.bounds.size}")));
        }

        private static void CaptureGameplayProofFrames(
            Scene scene,
            GameObject cinematicActor,
            GameObject gameplayVisual,
            ICollection<Texture2D> keyFrames)
        {
            GameObject transitionRoot = RequireRoot(scene, TransitionRootName);
            bool transitionRootWasActive = transitionRoot.activeSelf;
            bool actorWasActive = cinematicActor.activeSelf;
            bool gameplayWasActive = gameplayVisual.activeSelf;
            Renderer[] renderers = gameplayVisual
                .GetComponentsInChildren<Renderer>(includeInactive: true)
                .Where(renderer => renderer != null && renderer.enabled)
                .ToArray();
            int[] savedLayers = renderers.Select(renderer => renderer.gameObject.layer).ToArray();
            SkinnedMeshRenderer[] skinned = renderers.OfType<SkinnedMeshRenderer>().ToArray();
            bool[] savedUpdateWhenOffscreen = skinned
                .Select(renderer => renderer.updateWhenOffscreen)
                .ToArray();
            bool[] savedForceMatrixRecalculation = skinned
                .Select(renderer => renderer.forceMatrixRecalculationPerRender)
                .ToArray();
            GameObject cameraObject = null;
            GameObject lightObject = null;
            try
            {
                cinematicActor.SetActive(false);
                gameplayVisual.SetActive(true);
                if (renderers.Length != 4)
                {
                    throw new InvalidOperationException(
                        $"Gameplay proof expected four merged renderers, found {renderers.Length}.");
                }

                for (int index = 0; index < renderers.Length; index++)
                {
                    renderers[index].gameObject.layer = GameplayProofLayer;
                }

                foreach (SkinnedMeshRenderer renderer in skinned)
                {
                    renderer.updateWhenOffscreen = true;
                    renderer.forceMatrixRecalculationPerRender = true;
                }

                Animator animator = gameplayVisual.GetComponentInChildren<Animator>(
                    includeInactive: true)
                    ?? throw new InvalidOperationException(
                        "Gameplay Akaza proof requires its authored Animator.");
                Transform facing = RequireDescendant(gameplayVisual, "CHakazaA:world_trs").transform;

                cameraObject = new GameObject("AkazaPhase2_GameplayProofCamera");
                SceneManager.MoveGameObjectToScene(cameraObject, scene);
                Camera camera = cameraObject.AddComponent<Camera>();
                camera.enabled = false;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.006f, 0.01f, 0.018f, 1f);
                camera.cullingMask = 1 << GameplayProofLayer;
                camera.fieldOfView = 32f;
                camera.aspect = KeyWidth / (float)KeyHeight;
                camera.nearClipPlane = 0.05f;
                camera.farClipPlane = 200f;
                camera.allowHDR = false;
                camera.allowMSAA = true;

                lightObject = new GameObject("AkazaPhase2_GameplayProofLight");
                SceneManager.MoveGameObjectToScene(lightObject, scene);
                Light light = lightObject.AddComponent<Light>();
                light.type = LightType.Directional;
                light.color = new Color(1f, 0.72f, 0.58f, 1f);
                light.intensity = 1.25f;
                light.shadows = LightShadows.None;
                light.cullingMask = 1 << GameplayProofLayer;
                light.transform.rotation = Quaternion.Euler(34f, -28f, 0f);

                CaptureGameplayPose(
                    animator,
                    facing,
                    renderers,
                    camera,
                    "Base Layer.Hover",
                    0f,
                    true,
                    "10_gameplay_merged_hover.png",
                    keyFrames);
                CaptureGameplayPose(
                    animator,
                    facing,
                    renderers,
                    camera,
                    "Base Layer.HeavyCrush",
                    0.52f,
                    false,
                    "11_gameplay_merged_c27_heavy.png",
                    keyFrames);

                Camera productCamera = FindSceneComponents<Camera>(scene)
                    .SingleOrDefault(candidate => string.Equals(
                        candidate.gameObject.name,
                        "Main Camera",
                        StringComparison.Ordinal))
                    ?? throw new InvalidOperationException(
                        "Station gameplay proof requires the authored Main Camera.");
                // Product-camera proof represents the post-handoff gameplay look.
                // Release the cinematic light/Volume lease before rendering it.
                transitionRoot.SetActive(false);
                light.enabled = false;
                CaptureGameplayProductCameraPose(
                    animator,
                    productCamera,
                    "Base Layer.Hover",
                    0f,
                    "12_gameplay_product_camera_hover.png",
                    keyFrames);
                CaptureGameplayProductCameraPose(
                    animator,
                    productCamera,
                    "Base Layer.HeavyCrush",
                    0.52f,
                    "13_gameplay_product_camera_c27_heavy.png",
                    keyFrames);
                transitionRoot.SetActive(transitionRootWasActive);
                if (transitionRootWasActive)
                {
                    transitionRoot.GetComponent<AkazaPhase2CinematicLookDriver>()
                        ?.BeginManualLightingLease();
                }
            }
            finally
            {
                for (int index = 0; index < renderers.Length; index++)
                {
                    if (renderers[index] != null)
                    {
                        renderers[index].gameObject.layer = savedLayers[index];
                    }
                }

                for (int index = 0; index < skinned.Length; index++)
                {
                    if (skinned[index] == null)
                    {
                        continue;
                    }

                    skinned[index].updateWhenOffscreen = savedUpdateWhenOffscreen[index];
                    skinned[index].forceMatrixRecalculationPerRender =
                        savedForceMatrixRecalculation[index];
                }

                if (cameraObject != null)
                {
                    UnityEngine.Object.DestroyImmediate(cameraObject);
                }

                if (lightObject != null)
                {
                    UnityEngine.Object.DestroyImmediate(lightObject);
                }

                transitionRoot.SetActive(transitionRootWasActive);
                if (transitionRootWasActive)
                {
                    transitionRoot.GetComponent<AkazaPhase2CinematicLookDriver>()
                        ?.BeginManualLightingLease();
                }

                cinematicActor.SetActive(actorWasActive);
                gameplayVisual.SetActive(gameplayWasActive);
            }
        }

        private static void CaptureGameplayPose(
            Animator animator,
            Transform facing,
            IReadOnlyList<Renderer> renderers,
            Camera camera,
            string stateName,
            float normalizedTime,
            bool frameStructure,
            string fileName,
            ICollection<Texture2D> keyFrames)
        {
            int stateHash = Animator.StringToHash(stateName);
            if (!animator.HasState(0, stateHash))
            {
                throw new InvalidOperationException(
                    $"Gameplay Akaza proof is missing Animator state {stateName}.");
            }

            animator.Rebind();
            animator.Play(stateHash, 0, normalizedTime);
            animator.Update(0f);
            Physics.SyncTransforms();

            SkinnedMeshRenderer[] framingRenderers = renderers
                .OfType<SkinnedMeshRenderer>()
                .Where(renderer => frameStructure
                    || !IsWingStructureRenderer(renderer.gameObject.name))
                .ToArray();
            Bounds bounds = CalculateBakedWorldBounds(framingRenderers);

            Vector3 target = bounds.center;
            Vector3 faceForward = Vector3.ProjectOnPlane(facing.forward, Vector3.up).normalized;
            if (faceForward.sqrMagnitude <= 0.0001f)
            {
                faceForward = Vector3.back;
            }

            float verticalHalfAngle = camera.fieldOfView * Mathf.Deg2Rad * 0.5f;
            float verticalDistance = bounds.extents.y / Mathf.Tan(verticalHalfAngle);
            float horizontalDistance = bounds.extents.x
                / Mathf.Max(0.001f, Mathf.Tan(verticalHalfAngle) * camera.aspect);
            float distance = Mathf.Max(verticalDistance, horizontalDistance) * 1.18f;
            camera.transform.position = target + faceForward * Mathf.Max(1.5f, distance);
            camera.transform.rotation = Quaternion.LookRotation(
                target - camera.transform.position,
                Vector3.up);

            Texture2D frame = CaptureCamera(camera, KeyWidth, KeyHeight);
            int visiblePixels = CountGameplayProofPixels(frame, camera.backgroundColor);
            if (visiblePixels < MinimumGameplayProofPixels)
            {
                UnityEngine.Object.DestroyImmediate(frame);
                throw new InvalidOperationException(
                    $"Gameplay Akaza {stateName} render proof is empty or culled: "
                    + $"visiblePixels={visiblePixels}, required={MinimumGameplayProofPixels}.");
            }

            keyFrames.Add(frame);
            File.WriteAllBytes(Path.Combine(KeyFrameDirectory, fileName), frame.EncodeToPNG());
        }

        private static void CaptureGameplayProductCameraPose(
            Animator animator,
            Camera productCamera,
            string stateName,
            float normalizedTime,
            string fileName,
            ICollection<Texture2D> keyFrames)
        {
            int stateHash = Animator.StringToHash(stateName);
            if (!animator.HasState(0, stateHash))
            {
                throw new InvalidOperationException(
                    $"Gameplay Akaza product-camera proof is missing Animator state {stateName}.");
            }

            animator.Rebind();
            animator.Play(stateHash, 0, normalizedTime);
            animator.Update(0f);
            Physics.SyncTransforms();

            Texture2D frame = CaptureCamera(productCamera, KeyWidth, KeyHeight);
            keyFrames.Add(frame);
            File.WriteAllBytes(Path.Combine(KeyFrameDirectory, fileName), frame.EncodeToPNG());
        }

        private static int CountGameplayProofPixels(Texture2D frame, Color background)
        {
            Color32[] pixels = frame.GetPixels32();
            Color32 background32 = background;
            int count = 0;
            for (int index = 0; index < pixels.Length; index++)
            {
                Color32 pixel = pixels[index];
                int delta = Math.Abs(pixel.r - background32.r)
                    + Math.Abs(pixel.g - background32.g)
                    + Math.Abs(pixel.b - background32.b);
                if (delta >= 24)
                {
                    count++;
                }
            }

            return count;
        }

        private static Texture2D CaptureCamera(Camera camera, int width, int height)
        {
            RenderTexture previousTarget = camera.targetTexture;
            RenderTexture previousActive = RenderTexture.active;
            RenderTexture renderTexture = new RenderTexture(
                width,
                height,
                24,
                RenderTextureFormat.ARGB32);
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            try
            {
                renderTexture.Create();
                camera.targetTexture = renderTexture;
                RenderTexture.active = renderTexture;
                camera.Render();
                texture.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
                texture.Apply();
                return texture;
            }
            catch
            {
                UnityEngine.Object.DestroyImmediate(texture);
                throw;
            }
            finally
            {
                camera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                renderTexture.Release();
                UnityEngine.Object.DestroyImmediate(renderTexture);
            }
        }

        private static Texture2D BuildContactSheet(
            IReadOnlyList<Texture2D> frames,
            int tileWidth,
            int tileHeight,
            int columns)
        {
            if (frames.Count == 0 || columns <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(frames));
            }

            int rows = Mathf.CeilToInt(frames.Count / (float)columns);
            Texture2D sheet = new Texture2D(
                tileWidth * columns,
                tileHeight * rows,
                TextureFormat.RGBA32,
                false);
            Color[] black = Enumerable.Repeat(Color.black, sheet.width * sheet.height).ToArray();
            sheet.SetPixels(black);
            for (int frameIndex = 0; frameIndex < frames.Count; frameIndex++)
            {
                Color[] tile = new Color[tileWidth * tileHeight];
                for (int y = 0; y < tileHeight; y++)
                {
                    float v = y / (float)Mathf.Max(1, tileHeight - 1);
                    for (int x = 0; x < tileWidth; x++)
                    {
                        float u = x / (float)Mathf.Max(1, tileWidth - 1);
                        tile[x + (y * tileWidth)] = frames[frameIndex].GetPixelBilinear(u, v);
                    }
                }

                int column = frameIndex % columns;
                int sourceRow = frameIndex / columns;
                int destinationRow = rows - 1 - sourceRow;
                sheet.SetPixels(
                    column * tileWidth,
                    destinationRow * tileHeight,
                    tileWidth,
                    tileHeight,
                    tile);
            }

            sheet.Apply();
            return sheet;
        }

        private static void ValidateTimeline(TimelineAsset timeline)
        {
            if (Math.Abs(timeline.duration - MasterDurationSeconds) > 0.02d)
            {
                throw new InvalidOperationException(
                    $"Station Akaza master duration is {timeline.duration:0.000000}s; "
                    + $"expected {MasterDurationSeconds:0.000000}s.");
            }

            AudioTrack[] audioTracks = timeline.GetOutputTracks().OfType<AudioTrack>().ToArray();
            if (audioTracks.Length != 0)
            {
                throw new InvalidOperationException(
                    "Station Akaza phase transition must remain audio-free; "
                    + $"found {audioTracks.Length} AudioTrack(s).");
            }
        }

        private static void ValidateDirectorDuration(PlayableDirector director)
        {
            if (Math.Abs(director.duration - MasterDurationSeconds) > 0.02d)
            {
                throw new InvalidOperationException(
                    $"PlayableDirector duration is {director.duration:0.000000}s; "
                    + $"expected {MasterDurationSeconds:0.000000}s.");
            }
        }

        private static Renderer[] RequireWingRenderers(GameObject actor)
        {
            Renderer[] renderers = actor.GetComponentsInChildren<Renderer>(includeInactive: true)
                .Where(renderer => WingRendererNames.Contains(
                    renderer.gameObject.name,
                    StringComparer.Ordinal))
                .ToArray();
            string[] foundNames = renderers
                .Select(renderer => renderer.gameObject.name)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray();
            if (renderers.Length != WingRendererNames.Length
                || foundNames.Length != WingRendererNames.Length)
            {
                string missing = string.Join(
                    ", ",
                    WingRendererNames.Except(foundNames, StringComparer.Ordinal));
                throw new InvalidOperationException(
                    "C33 wing regression requires exactly six floating-blade renderers. "
                    + $"found={renderers.Length}, missing=[{missing}].");
            }

            return renderers;
        }

        private static Camera RequireCamera(GameObject transitionRoot, string rigName)
        {
            GameObject rig = RequireDescendant(transitionRoot, rigName);
            return rig.GetComponentInChildren<Camera>(includeInactive: true)
                ?? throw new InvalidOperationException($"{rigName} has no Camera component.");
        }

        private static GameObject RequireRoot(Scene scene, string name)
        {
            GameObject[] matches = scene.GetRootGameObjects()
                .Where(root => string.Equals(root.name, name, StringComparison.Ordinal))
                .ToArray();
            if (matches.Length != 1)
            {
                throw new InvalidOperationException(
                    $"Expected exactly one root named {name}; found {matches.Length}.");
            }

            return matches[0];
        }

        private static GameObject RequireSceneObject(Scene scene, string name)
        {
            GameObject[] matches = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Transform>(includeInactive: true))
                .Where(transform => string.Equals(
                    transform.gameObject.name,
                    name,
                    StringComparison.Ordinal))
                .Select(transform => transform.gameObject)
                .ToArray();
            if (matches.Length != 1)
            {
                throw new InvalidOperationException(
                    $"Expected exactly one scene object named {name}; found {matches.Length}.");
            }

            return matches[0];
        }

        private static GameObject RequireDescendant(GameObject root, string name)
        {
            GameObject[] matches = root.GetComponentsInChildren<Transform>(includeInactive: true)
                .Where(transform => string.Equals(
                    transform.gameObject.name,
                    name,
                    StringComparison.Ordinal))
                .Select(transform => transform.gameObject)
                .ToArray();
            if (matches.Length != 1)
            {
                throw new InvalidOperationException(
                    $"Expected exactly one {name} below {root.name}; found {matches.Length}.");
            }

            return matches[0];
        }

        private static T[] FindSceneComponents<T>(Scene scene) where T : Component
        {
            return scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<T>(includeInactive: true))
                .ToArray();
        }

        private static void ResetOutputRoot()
        {
            string normalized = Path.GetFullPath(OutputRoot)
                .Replace('\\', '/')
                .TrimEnd('/');
            const string Expected = "C:/tmp/DimensionBrawl-StationAkazaPhase2Intro";
            if (!string.Equals(normalized, Expected, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Refusing to reset unexpected capture path: {normalized}.");
            }

            if (Directory.Exists(normalized))
            {
                Directory.Delete(normalized, recursive: true);
            }

            Directory.CreateDirectory(normalized);
            Directory.CreateDirectory(FrameDirectory);
            Directory.CreateDirectory(KeyFrameDirectory);
        }

        private static void WriteMetrics(
            float wingStartSpan,
            float wingOpenSpan,
            int closedIrisPixels,
            int openIrisPixels,
            RectInt closedIrisRegion,
            RectInt openIrisRegion,
            GameplayContinuity gameplayContinuity)
        {
            CultureInfo invariant = CultureInfo.InvariantCulture;
            StringBuilder metrics = new StringBuilder();
            metrics.AppendLine("metric,start_time,start_value,end_time,end_value,delta,ratio,region_start,region_end");
            metrics.Append("c33_blade_vertical_span,")
                .Append(WingClosedSampleSeconds.ToString("0.000000", invariant)).Append(',')
                .Append(wingStartSpan.ToString("0.000000", invariant)).Append(',')
                .Append(WingOpenSampleSeconds.ToString("0.000000", invariant)).Append(',')
                .Append(wingOpenSpan.ToString("0.000000", invariant)).Append(',')
                .Append((wingOpenSpan - wingStartSpan).ToString("0.000000", invariant)).Append(',')
                .Append((wingOpenSpan / wingStartSpan).ToString("0.000000", invariant))
                .AppendLine(",,");
            metrics.Append("c34_turquoise_iris_pixels,")
                .Append(EyeClosedSampleSeconds.ToString("0.000000", invariant)).Append(',')
                .Append(closedIrisPixels).Append(',')
                .Append(EyeOpenSampleSeconds.ToString("0.000000", invariant)).Append(',')
                .Append(openIrisPixels).Append(',')
                .Append(openIrisPixels - closedIrisPixels).Append(',')
                .Append(closedIrisPixels > 0
                    ? (openIrisPixels / (float)closedIrisPixels).ToString("0.000000", invariant)
                    : "infinite")
                .Append(',').Append(FormatRect(closedIrisRegion))
                .Append(',').Append(FormatRect(openIrisRegion))
                .AppendLine();
            metrics.Append("terminal_gameplay_pose_continuity,")
                .Append(TerminalContinuitySampleSeconds.ToString("0.000000", invariant)).Append(',')
                .Append("cinematic_terminal,")
                .Append(TerminalContinuitySampleSeconds.ToString("0.000000", invariant)).Append(',')
                .Append("gameplay_spawn,")
                .Append(gameplayContinuity.HorizontalDelta.ToString("0.000000", invariant)).Append(',')
                .Append(gameplayContinuity.HeightRatio.ToString("0.000000", invariant)).Append(',')
                .Append("floorDelta=").Append(gameplayContinuity.FloorDelta.ToString("0.000000", invariant))
                .Append(",facingDelta=").Append(gameplayContinuity.FacingDeltaDegrees.ToString("0.000000", invariant))
                .AppendLine();
            File.WriteAllText(MetricsPath, metrics.ToString(), Encoding.UTF8);
        }

        private static void WriteReport(
            TimelineAsset timeline,
            int frameCount,
            int skinnedMeshCount,
            float wingStartSpan,
            float wingOpenSpan,
            int closedIrisPixels,
            int openIrisPixels,
            GameplayContinuity gameplayContinuity)
        {
            StringBuilder report = new StringBuilder();
            report.AppendLine("# Station Akaza phase-two intro capture");
            report.AppendLine();
            report.AppendLine("- Result: PASS");
            report.AppendLine($"- Product scene: `{StationScenePath}`");
            report.AppendLine($"- Timeline: `{AssetDatabase.GetAssetPath(timeline)}`");
            report.AppendLine($"- Master duration: {timeline.duration:0.000000}s");
            report.AppendLine($"- Camera cut: C33 -> C34 at +{CameraSwitchSeconds:0.000000}s");
            report.AppendLine($"- 30fps sequence: {frameCount} frames ({FrameWidth}x{FrameHeight})");
            report.AppendLine($"- Offline SMR safeguards: {skinnedMeshCount} renderers");
            report.AppendLine(
                $"- C33 blade vertical span: {wingStartSpan:0.0000} at +{WingClosedSampleSeconds:0.000}s "
                + $"-> {wingOpenSpan:0.0000} at +{WingOpenSampleSeconds:0.000}s (PASS)");
            report.AppendLine(
                $"- C34 turquoise iris pixels: {closedIrisPixels} closed -> {openIrisPixels} open (PASS)");
            report.AppendLine(
                $"- Terminal gameplay pose: horizontal {gameplayContinuity.HorizontalDelta:0.0000}m, "
                + $"floor {gameplayContinuity.FloorDelta:0.0000}m, "
                + $"height ratio {gameplayContinuity.HeightRatio:0.0000}, "
                + $"facing {gameplayContinuity.FacingDeltaDegrees:0.0000}deg (PASS)");
            report.AppendLine("- Audio: none; batch capture requires `-noaudio`, scene sources are muted, and the Timeline has no AudioTrack");
            report.AppendLine($"- Contact sheet: `{ContactSheetPath}`");
            report.AppendLine($"- Key frames: `{KeyFrameDirectory}`");
            report.AppendLine($"- 30fps frames: `{FrameDirectory}`");
            report.AppendLine($"- Metrics: `{MetricsPath}`");
            report.AppendLine();
            report.AppendLine(
                "The capture evaluates the product PlayableDirector directly. C33 is the only enabled "
                + "cinematic camera before 1.600000s; C34 is the only enabled cinematic camera at and "
                + "after the cut. Camera.Render is used for every evidence and sequence frame.");
            File.WriteAllText(ReportPath, report.ToString(), Encoding.UTF8);
        }

        private static string FormatRect(RectInt rect)
        {
            return $"{rect.x}:{rect.y}:{rect.width}:{rect.height}";
        }

        private static void RequireNoAudioCommandLine()
        {
            bool noAudio = Environment.GetCommandLineArgs().Any(argument =>
                string.Equals(argument, "-noaudio", StringComparison.OrdinalIgnoreCase));
            if (!noAudio)
            {
                throw new InvalidOperationException(
                    "RunBatchCapture requires Unity's -noaudio argument so capture is silent.");
            }
        }

        private static bool Approximately(float left, float right)
        {
            return Mathf.Abs(left - right) <= 0.00001f;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private readonly struct KeyBeat
        {
            public KeyBeat(string label, float seconds)
            {
                Label = label;
                Seconds = seconds;
            }

            public string Label { get; }
            public float Seconds { get; }
        }

        private readonly struct GameplayContinuity
        {
            public GameplayContinuity(
                float horizontalDelta,
                float floorDelta,
                float heightRatio,
                float facingDeltaDegrees,
                Bounds cinematicBounds,
                Bounds gameplayBounds)
            {
                HorizontalDelta = horizontalDelta;
                FloorDelta = floorDelta;
                HeightRatio = heightRatio;
                FacingDeltaDegrees = facingDeltaDegrees;
                CinematicBounds = cinematicBounds;
                GameplayBounds = gameplayBounds;
            }

            public float HorizontalDelta { get; }
            public float FloorDelta { get; }
            public float HeightRatio { get; }
            public float FacingDeltaDegrees { get; }
            public Bounds CinematicBounds { get; }
            public Bounds GameplayBounds { get; }
        }
    }
}
