using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;

namespace DimensionBrawl.Editor.AuditionPV
{
    /// <summary>
    /// Produces the deterministic G04 golden PNG source directly from the authored
    /// Olympus Station C33 -> C34 Timeline. This utility never saves its temporary
    /// product-scene state; it restores all leases and reopens the product scene.
    /// </summary>
    public static class AuditionPvStationTransitionGoldenCapture
    {
        internal const string StationScenePath =
            "Assets/_Game/Scenes/OlympusStationCombatStage.unity";
        internal const string TimelinePath =
            "Assets/_Game/DesignData/Profiles/Cinematics/DB_Timeline_OlympusStationAkazaPhase2Intro.playable";
        internal const string C33ActorPath =
            "Assets/_Game/Art/Characters/Bosses/Akaza/Animations/Source/C33_Akaza.fbx";
        internal const string C34ActorPath =
            "Assets/_Game/Art/Characters/Bosses/Akaza/Animations/Source/C34_Akaza.fbx";
        internal const string C33CameraPath =
            "Assets/_Game/Art/Animations/Cinematics/LegacyCameraGrammar/C33_Cam.fbx";
        internal const string C34CameraPath =
            "Assets/_Game/Art/Animations/Cinematics/LegacyCameraGrammar/C34_Cam.fbx";
        internal const string CaptureScriptPath =
            "Assets/_Game/Editor/AuditionPV/AuditionPvStationTransitionGoldenCapture.cs";

        internal const string ShotId = "g04";
        internal const string FramesFolderName = "G04_C33_C34_PNG";
        internal const string BaselinesFolderName = "baselines";
        internal const string Bl04FileName =
            "BL04_AKAZA_C33_WING_OPEN__HUDOFF__t01.100000.png";
        internal const string Bl05FileName =
            "BL05_AKAZA_C34_EYE_OPEN__HUDOFF__t02.966667.png";
        internal const int FirstFrame = 0;
        internal const int LastFrame = 237;
        internal const int ExpectedFrameCount = 238;
        internal const int FirstC34Frame = 96;
        internal const int Bl04SourceFrame = 66;
        internal const int Bl05SourceFrame = 178;

        private const string TransitionRootName = "OlympusStation_AkazaPhase2TransitionRig";
        private const string ActorName = "AkazaPhase2_CinematicActor";
        private const string PhaseOneVisualName =
            "BossBarrageLaneReview_HumanoidBossVisual_SciFiSoldier_01_Commando";
        private const string DirectorName = "AkazaPhase2_MasterPlayableDirector";
        private const string WingCameraRigName = "C33_WingDeployCameraRig";
        private const string EyeCameraRigName = "C34_EyeOpenCameraRig";
        private const string EyeRendererName = "CHakazaA:eyeBall";
        private const string HudSerializedPropertyName = "combatHudCanvasGroup";
        private const double MasterDurationSeconds = 3.9666667d;
        private const int WingClosedFrame = 18;
        private const int EyeClosedFrame = 103;
        private const float MinimumWingSpanGrowth = 0.20f;
        private const float MinimumWingSpanRatio = 1.04f;
        private const int MinimumOpenIrisPixels = 24;
        private const int MinimumIrisPixelGrowth = 8;
        private const int VisualSampleColumns = 32;
        private const int VisualSampleRows = 18;

        private static readonly string[] WingRendererNames =
        {
            "CHakazaA:akWp_BladeA_geo",
            "CHakazaA:akWp_BladeB_geo",
            "CHakazaA:akWp_BladeC_geo",
            "CHakazaA:akWp_BladeD_geo",
            "CHakazaA:akWp_BladeE_geo",
            "CHakazaA:akWp_BladeF_geo"
        };

        [MenuItem("DimensionBrawl/Audition PV/Capture G04 Station C33-C34 Golden Source")]
        public static void CaptureMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            string outputDirectory = CaptureGoldenSource();
            EditorUtility.RevealInFinder(outputDirectory);
            Debug.Log($"[AuditionPV] G04 Station golden source passed: {outputDirectory}");
        }

        /// <summary>
        /// Unity batch entry point. Invoke with -batchmode -noaudio and without
        /// -nographics; Camera.Render and the pixel regressions require graphics.
        /// </summary>
        public static void RunBatchCapture()
        {
            try
            {
                RequireNoAudioCommandLine();
                string outputDirectory = CaptureGoldenSource();
                Debug.Log($"[AuditionPV] G04 batch capture passed: {outputDirectory}");
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        public static string CaptureGoldenSource()
        {
            DateTime startedAtUtc = DateTime.UtcNow;
            AuditionPvGitSnapshot gitAtStart = AuditionPvEnvironmentProbe.ReadGitSnapshot();
            if (!gitAtStart.probeSucceeded)
            {
                throw new InvalidOperationException(
                    "G04 capture requires a successful Git provenance probe: "
                    + gitAtStart.probeError);
            }

            AuditionPvEngineSnapshot engineAtStart = AuditionPvEnvironmentProbe.ReadEngineSnapshot();
            string[] dependencyPaths = CollectCaptureDependencyPaths();
            AuditionPvDependencyHash[] dependenciesAtStart =
                AuditionPvEnvironmentProbe.HashDependencies(dependencyPaths);
            string requestedCaptureId = AuditionPvOutputPaths.CreateOutputId(
                "g04-station-c33-c34",
                startedAtUtc,
                gitAtStart.commitSha,
                gitAtStart.isDirty,
                gitAtStart.dirtyStateHashSha256);
            string outputDirectory =
                AuditionPvOutputPaths.CreateUniqueGoldenOutputDirectory(requestedCaptureId);
            string captureId = new DirectoryInfo(outputDirectory).Name;
            string frameDirectory = Path.Combine(outputDirectory, FramesFolderName);
            string baselineDirectory = Path.Combine(outputDirectory, BaselinesFolderName);

            try
            {
                CreateNewDirectory(frameDirectory);
                CreateNewDirectory(baselineDirectory);
                CaptureMetrics metrics = CaptureProductTimeline(
                    outputDirectory,
                    frameDirectory,
                    baselineDirectory);

                ValidateFrameSequence(frameDirectory);
                ValidatePngFile(
                    Path.Combine(baselineDirectory, Bl04FileName),
                    AuditionPvCaptureContract.Width,
                    AuditionPvCaptureContract.Height);
                ValidatePngFile(
                    Path.Combine(baselineDirectory, Bl05FileName),
                    AuditionPvCaptureContract.Width,
                    AuditionPvCaptureContract.Height);
                ValidateVisualSanity(
                    metrics.sampleCount,
                    metrics.blackSampleCount,
                    metrics.magentaSampleCount,
                    metrics.healthyFrameCount,
                    metrics.magentaFrameCount);
                ValidateWingExpansion(metrics.wingClosedSpan, metrics.wingOpenSpan);
                ValidateIrisGrowth(metrics.closedIrisPixels, metrics.openIrisPixels);

                AuditionPvGitSnapshot gitAtEnd = AuditionPvEnvironmentProbe.ReadGitSnapshot();
                ValidateStableGitSnapshot(gitAtStart, gitAtEnd);
                AuditionPvDependencyHash[] dependenciesAtEnd =
                    AuditionPvEnvironmentProbe.HashDependencies(dependencyPaths);
                ValidateStableDependencies(dependenciesAtStart, dependenciesAtEnd);

                AuditionPvShotManifestEntry[] shots = { CreateShotManifestEntry() };
                AuditionPvBaselineManifestEntry[] baselines = CreateBaselineManifestEntries();
                AuditionPvTestResult[] testResults = CreateTestResults(
                    outputDirectory,
                    metrics,
                    startedAtUtc);
                AuditionPvCaptureManifest manifest =
                    AuditionPvCaptureManifestFactory.CreateForRoot(
                        captureId,
                        AuditionPvCaptureContract.OutputRoot,
                        outputDirectory,
                        shots,
                        baselines,
                        testResults,
                        createdAtUtc: startedAtUtc,
                        gitSnapshot: gitAtStart,
                        engineSnapshot: engineAtStart,
                        dependencyHashSnapshot: dependenciesAtStart);
                string manifestPath = AuditionPvCaptureManifestWriter.WriteNew(manifest);
                ValidateManifestRoundTrip(manifestPath);
                return outputDirectory.Replace('\\', '/');
            }
            catch (Exception exception)
            {
                TryWriteFailureArtifact(outputDirectory, exception);
                throw;
            }
        }

        internal static bool UsesWingCamera(int frameIndex)
        {
            ValidateFrameIndex(frameIndex);
            return frameIndex < FirstC34Frame;
        }

        internal static string FrameFileName(int frameIndex)
        {
            ValidateFrameIndex(frameIndex);
            return $"frame_{frameIndex:0000}.png";
        }

        internal static AuditionPvShotManifestEntry CreateShotManifestEntry()
        {
            return new AuditionPvShotManifestEntry
            {
                id = ShotId,
                scenePath = StationScenePath,
                startFrame = FirstFrame,
                endFrame = LastFrame,
                expectedFrameCount = ExpectedFrameCount,
                hudMode = "hud-off",
                notes =
                    "Authored Station Timeline direct Evaluate. C33 frames 0-95; "
                    + "C34 frames 96-237; 2560x1440 PNG at 60fps."
            };
        }

        internal static AuditionPvBaselineManifestEntry[] CreateBaselineManifestEntries()
        {
            return new[]
            {
                new AuditionPvBaselineManifestEntry
                {
                    id = "bl04",
                    shotId = ShotId,
                    sourceFrame = Bl04SourceFrame,
                    fileName = Bl04FileName,
                    hudMode = "hud-off",
                    status = "captured"
                },
                new AuditionPvBaselineManifestEntry
                {
                    id = "bl05",
                    shotId = ShotId,
                    sourceFrame = Bl05SourceFrame,
                    fileName = Bl05FileName,
                    hudMode = "hud-off",
                    status = "captured"
                }
            };
        }

        internal static string[] ExplicitProductDependencyPaths()
        {
            return new[]
            {
                StationScenePath,
                TimelinePath,
                C33ActorPath,
                C34ActorPath,
                C33CameraPath,
                C34CameraPath,
                CaptureScriptPath
            };
        }

        internal static void WriteBytesNew(string path, byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            string parent = Path.GetDirectoryName(path)
                ?? throw new ArgumentException("Output file must have a parent directory.", nameof(path));
            if (!Directory.Exists(parent))
            {
                throw new DirectoryNotFoundException(parent);
            }

            using FileStream stream = new FileStream(
                path,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None);
            stream.Write(bytes, 0, bytes.Length);
            stream.Flush(flushToDisk: true);
        }

        internal static void ValidatePngFile(string path, int expectedWidth, int expectedHeight)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Expected PNG frame is missing.", path);
            }

            byte[] header = new byte[24];
            using (FileStream stream = File.OpenRead(path))
            {
                if (stream.Length < header.Length || stream.Read(header, 0, header.Length) != header.Length)
                {
                    throw new InvalidDataException($"PNG is truncated: {path}");
                }
            }

            byte[] signature = { 137, 80, 78, 71, 13, 10, 26, 10 };
            for (int index = 0; index < signature.Length; index++)
            {
                if (header[index] != signature[index])
                {
                    throw new InvalidDataException($"PNG signature mismatch: {path}");
                }
            }

            if (header[12] != (byte)'I' || header[13] != (byte)'H' ||
                header[14] != (byte)'D' || header[15] != (byte)'R')
            {
                throw new InvalidDataException($"PNG does not begin with IHDR: {path}");
            }

            int width = ReadBigEndianInt32(header, 16);
            int height = ReadBigEndianInt32(header, 20);
            if (width != expectedWidth || height != expectedHeight)
            {
                throw new InvalidDataException(
                    $"PNG dimensions are {width}x{height}; expected "
                    + $"{expectedWidth}x{expectedHeight}: {path}");
            }
        }

        internal static void ValidateVisualSanity(
            long sampleCount,
            long blackSampleCount,
            long magentaSampleCount,
            int healthyFrameCount,
            int magentaFrameCount)
        {
            if (sampleCount <= 0)
            {
                throw new InvalidOperationException("Visual sanity did not sample any pixels.");
            }

            double blackRatio = blackSampleCount / (double)sampleCount;
            double magentaRatio = magentaSampleCount / (double)sampleCount;
            if (blackRatio >= 0.90d || healthyFrameCount < ExpectedFrameCount * 0.90d)
            {
                throw new InvalidOperationException(
                    $"G04 black-frame sanity failed: black={blackRatio:P2}, "
                    + $"healthyFrames={healthyFrameCount}/{ExpectedFrameCount}.");
            }

            if (magentaRatio >= 0.01d || magentaFrameCount > 0)
            {
                throw new InvalidOperationException(
                    $"G04 missing-shader sanity failed: magenta={magentaRatio:P2}, "
                    + $"affectedFrames={magentaFrameCount}/{ExpectedFrameCount}.");
            }
        }

        private static CaptureMetrics CaptureProductTimeline(
            string outputDirectory,
            string frameDirectory,
            string baselineDirectory)
        {
            SceneLease lease = null;
            bool openedStationScene = false;
            try
            {
                Scene scene = EditorSceneManager.OpenScene(StationScenePath, OpenSceneMode.Single);
                openedStationScene = true;
                CaptureBindings bindings = ResolveBindings(scene);
                ValidateBindings(bindings);
                lease = new SceneLease(scene, bindings);
                lease.Apply();
                bindings.director.Stop();
                bindings.director.RebuildGraph();
                ValidateDirectorDuration(bindings.director);

                var metrics = new CaptureMetrics();
                for (int frameIndex = FirstFrame; frameIndex <= LastFrame; frameIndex++)
                {
                    Camera camera = SampleDirector(bindings, frameIndex);
                    Texture2D frame = CaptureCamera(
                        camera,
                        AuditionPvCaptureContract.Width,
                        AuditionPvCaptureContract.Height);
                    try
                    {
                        AccumulateVisualSanity(frame, metrics);
                        if (frameIndex == WingClosedFrame)
                        {
                            metrics.wingClosedSpan = MeasureVerticalWingSpan(
                                bindings.actor,
                                bindings.wingRenderers);
                        }
                        else if (frameIndex == Bl04SourceFrame)
                        {
                            metrics.wingOpenSpan = MeasureVerticalWingSpan(
                                bindings.actor,
                                bindings.wingRenderers);
                        }

                        if (frameIndex == EyeClosedFrame)
                        {
                            metrics.closedIrisPixels = CountTurquoiseIrisPixels(
                                frame,
                                camera,
                                bindings.eyeRenderer);
                        }
                        else if (frameIndex == Bl05SourceFrame)
                        {
                            metrics.openIrisPixels = CountTurquoiseIrisPixels(
                                frame,
                                camera,
                                bindings.eyeRenderer);
                        }

                        byte[] png = frame.EncodeToPNG();
                        if (png == null || png.Length == 0)
                        {
                            throw new InvalidOperationException(
                                $"Unity returned an empty PNG for G04 frame {frameIndex}.");
                        }

                        WriteBytesNew(
                            Path.Combine(frameDirectory, FrameFileName(frameIndex)),
                            png);
                    }
                    finally
                    {
                        UnityEngine.Object.DestroyImmediate(frame);
                    }
                }

                ValidateExactCameraCut(bindings);
                CopyNew(
                    Path.Combine(frameDirectory, FrameFileName(Bl04SourceFrame)),
                    Path.Combine(baselineDirectory, Bl04FileName));
                CopyNew(
                    Path.Combine(frameDirectory, FrameFileName(Bl05SourceFrame)),
                    Path.Combine(baselineDirectory, Bl05FileName));
                WriteCaptureReadme(outputDirectory, metrics);
                return metrics;
            }
            finally
            {
                try
                {
                    lease?.Restore();
                }
                finally
                {
                    if (openedStationScene)
                    {
                        // Discards every temporary scene mutation, including any
                        // state an evaluated animation track may have written.
                        EditorSceneManager.OpenScene(StationScenePath, OpenSceneMode.Single);
                    }
                }
            }
        }

        private static CaptureBindings ResolveBindings(Scene scene)
        {
            GameObject transitionRoot = RequireSceneObject(scene, TransitionRootName);
            GameObject actor = RequireDescendant(transitionRoot, ActorName);
            PlayableDirector director = RequireDescendant(transitionRoot, DirectorName)
                .GetComponent<PlayableDirector>()
                ?? throw new InvalidOperationException(
                    $"{DirectorName} has no PlayableDirector component.");
            Camera wingCamera = RequireCamera(transitionRoot, WingCameraRigName);
            Camera eyeCamera = RequireCamera(transitionRoot, EyeCameraRigName);
            AkazaPhase2CinematicLookDriver lookDriver =
                transitionRoot.GetComponent<AkazaPhase2CinematicLookDriver>()
                ?? throw new InvalidOperationException(
                    "Station transition root has no Akaza cinematic look driver.");
            OlympusStationAkazaPhase2FlowController[] flowControllers =
                FindSceneComponents<OlympusStationAkazaPhase2FlowController>(scene);
            if (flowControllers.Length != 1)
            {
                throw new InvalidOperationException(
                    "G04 requires exactly one Station phase-two flow controller; "
                    + $"found {flowControllers.Length}.");
            }

            Renderer eyeRenderer = actor.GetComponentsInChildren<Renderer>(includeInactive: true)
                .SingleOrDefault(renderer => string.Equals(
                    renderer.gameObject.name,
                    EyeRendererName,
                    StringComparison.Ordinal))
                ?? throw new InvalidOperationException(
                    $"Cinematic Akaza is missing {EyeRendererName}.");
            SkinnedMeshRenderer[] skinnedMeshes =
                actor.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true);
            if (skinnedMeshes.Length == 0)
            {
                throw new InvalidOperationException(
                    "Cinematic Akaza has no SkinnedMeshRenderers for deterministic capture.");
            }

            return new CaptureBindings
            {
                transitionRoot = transitionRoot,
                phaseOneVisual = RequireSceneObject(scene, PhaseOneVisualName),
                actor = actor,
                director = director,
                timeline = director.playableAsset as TimelineAsset,
                wingCamera = wingCamera,
                eyeCamera = eyeCamera,
                lookDriver = lookDriver,
                flowController = flowControllers[0],
                wingRenderers = RequireWingRenderers(actor),
                eyeRenderer = eyeRenderer,
                skinnedMeshes = skinnedMeshes
            };
        }

        private static void ValidateBindings(CaptureBindings bindings)
        {
            if (bindings.timeline == null)
            {
                throw new InvalidOperationException(
                    "Station phase-two director is not bound to a TimelineAsset.");
            }

            string authoredTimelinePath = AssetDatabase.GetAssetPath(bindings.timeline);
            if (!string.Equals(authoredTimelinePath, TimelinePath, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"G04 must use the authored product Timeline at {TimelinePath}; "
                    + $"found {authoredTimelinePath}.");
            }

            if (bindings.wingCamera == bindings.eyeCamera)
            {
                throw new InvalidOperationException("C33 and C34 must use distinct cameras.");
            }

            if (Math.Abs(bindings.timeline.duration - MasterDurationSeconds) > 0.02d)
            {
                throw new InvalidOperationException(
                    $"Station Timeline duration is {bindings.timeline.duration:0.000000}s; "
                    + $"expected {MasterDurationSeconds:0.000000}s.");
            }

            AudioTrack[] audioTracks = bindings.timeline.GetOutputTracks()
                .OfType<AudioTrack>()
                .ToArray();
            if (audioTracks.Length != 0)
            {
                throw new InvalidOperationException(
                    "G04 source must remain audio-free; found "
                    + $"{audioTracks.Length} Timeline AudioTrack(s).");
            }
        }

        private static Camera SampleDirector(CaptureBindings bindings, int frameIndex)
        {
            ValidateFrameIndex(frameIndex);
            double seconds = frameIndex / (double)AuditionPvCaptureContract.Fps;
            bindings.director.time = seconds;
            bindings.director.Evaluate();
            bindings.lookDriver.ApplyCurrentTime();
            Physics.SyncTransforms();

            bool useWing = UsesWingCamera(frameIndex);
            bindings.wingCamera.enabled = useWing;
            bindings.eyeCamera.enabled = !useWing;
            if (bindings.wingCamera.enabled == bindings.eyeCamera.enabled)
            {
                throw new InvalidOperationException(
                    $"Cinematic camera exclusivity failed on G04 frame {frameIndex}.");
            }

            return useWing ? bindings.wingCamera : bindings.eyeCamera;
        }

        private static void ValidateExactCameraCut(CaptureBindings bindings)
        {
            Camera before = SampleDirector(bindings, FirstC34Frame - 1);
            Camera atCut = SampleDirector(bindings, FirstC34Frame);
            if (before != bindings.wingCamera || atCut != bindings.eyeCamera)
            {
                throw new InvalidOperationException(
                    "G04 camera cut must show C33 on frame 95 and C34 on frame 96 "
                    + "at exactly 1.600000 seconds.");
            }
        }

        private static Texture2D CaptureCamera(Camera camera, int width, int height)
        {
            RenderTexture previousTarget = camera.targetTexture;
            RenderTexture previousActive = RenderTexture.active;
            var renderTexture = new RenderTexture(
                width,
                height,
                24,
                RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.sRGB);
            var texture = new Texture2D(
                width,
                height,
                TextureFormat.RGBA32,
                mipChain: false,
                linear: false);
            try
            {
                renderTexture.Create();
                camera.targetTexture = renderTexture;
                RenderTexture.active = renderTexture;
                camera.Render();
                texture.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
                texture.Apply(updateMipmaps: false, makeNoLongerReadable: false);
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

        private static void AccumulateVisualSanity(Texture2D frame, CaptureMetrics metrics)
        {
            int black = 0;
            int magenta = 0;
            int count = VisualSampleColumns * VisualSampleRows;
            for (int row = 0; row < VisualSampleRows; row++)
            {
                int y = Mathf.RoundToInt(
                    (row + 0.5f) * frame.height / VisualSampleRows - 0.5f);
                for (int column = 0; column < VisualSampleColumns; column++)
                {
                    int x = Mathf.RoundToInt(
                        (column + 0.5f) * frame.width / VisualSampleColumns - 0.5f);
                    Color pixel = frame.GetPixel(x, y);
                    float luminance = pixel.r * 0.2126f + pixel.g * 0.7152f + pixel.b * 0.0722f;
                    if (luminance <= 0.015f)
                    {
                        black++;
                    }

                    if (pixel.r >= 0.80f && pixel.b >= 0.80f && pixel.g <= 0.20f)
                    {
                        magenta++;
                    }
                }
            }

            metrics.sampleCount += count;
            metrics.blackSampleCount += black;
            metrics.magentaSampleCount += magenta;
            if (black < count * 0.95f)
            {
                metrics.healthyFrameCount++;
            }

            if (magenta >= count * 0.10f)
            {
                metrics.magentaFrameCount++;
            }
        }

        private static float MeasureVerticalWingSpan(
            GameObject actor,
            IReadOnlyList<Renderer> wingRenderers)
        {
            float minimumY = float.PositiveInfinity;
            float maximumY = float.NegativeInfinity;
            foreach (Renderer renderer in wingRenderers)
            {
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
                    float actorY = actor.transform.InverseTransformPoint(worldCorner).y;
                    minimumY = Mathf.Min(minimumY, actorY);
                    maximumY = Mathf.Max(maximumY, actorY);
                }
            }

            float span = maximumY - minimumY;
            if (!IsFinite(span) || span <= 0.001f)
            {
                throw new InvalidOperationException($"C33 wing span is invalid: {span}.");
            }

            return span;
        }

        private static int CountTurquoiseIrisPixels(
            Texture2D frame,
            Camera camera,
            Renderer eyeRenderer)
        {
            RectInt region = CalculateRendererPixelRegion(frame, camera, eyeRenderer, 0.35f);
            Color32[] pixels = frame.GetPixels32();
            int count = 0;
            for (int y = region.yMin; y < region.yMax; y++)
            {
                int row = y * frame.width;
                for (int x = region.xMin; x < region.xMax; x++)
                {
                    Color32 pixel = pixels[row + x];
                    if (pixel.r <= 80 &&
                        pixel.g >= 90 &&
                        pixel.b >= 90 &&
                        pixel.g >= pixel.r + 40 &&
                        pixel.b >= pixel.r + 40)
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
            RectInt region = new RectInt(xMin, yMin, xMax - xMin, yMax - yMin);
            if (region.width < 4 || region.height < 4)
            {
                throw new InvalidOperationException($"C34 eye region is too small: {region}.");
            }

            return region;
        }

        private static void ValidateWingExpansion(float closedSpan, float openSpan)
        {
            if (!IsFinite(closedSpan) || !IsFinite(openSpan) || closedSpan <= 0f)
            {
                throw new InvalidOperationException(
                    "C33 wing samples were not captured on frames 18 and 66.");
            }

            float growth = openSpan - closedSpan;
            float ratio = openSpan / closedSpan;
            if (growth < MinimumWingSpanGrowth || ratio < MinimumWingSpanRatio)
            {
                throw new InvalidOperationException(
                    "C33 wing-deploy regression failed. "
                    + $"closed={closedSpan:0.0000}, open={openSpan:0.0000}, "
                    + $"growth={growth:0.0000}, ratio={ratio:0.0000}.");
            }
        }

        private static void ValidateIrisGrowth(int closedPixels, int openPixels)
        {
            if (closedPixels < 0 || openPixels < 0)
            {
                throw new InvalidOperationException(
                    "C34 iris samples were not captured on frames 103 and 178.");
            }

            int required = Math.Max(MinimumOpenIrisPixels, closedPixels + MinimumIrisPixelGrowth);
            if (openPixels < required)
            {
                throw new InvalidOperationException(
                    "C34 rendered-eye regression failed. "
                    + $"closed={closedPixels}, open={openPixels}, required={required}.");
            }
        }

        private static void ValidateFrameSequence(string frameDirectory)
        {
            string[] frames = Directory.GetFiles(
                    frameDirectory,
                    "frame_*.png",
                    SearchOption.TopDirectoryOnly)
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();
            if (frames.Length != ExpectedFrameCount)
            {
                throw new InvalidOperationException(
                    $"G04 requires {ExpectedFrameCount} PNGs; found {frames.Length}.");
            }

            for (int frameIndex = FirstFrame; frameIndex <= LastFrame; frameIndex++)
            {
                string expectedPath = Path.Combine(frameDirectory, FrameFileName(frameIndex));
                if (!string.Equals(
                        Path.GetFullPath(frames[frameIndex]),
                        Path.GetFullPath(expectedPath),
                        StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        $"G04 frame sequence is not contiguous at frame {frameIndex}.");
                }

                ValidatePngFile(
                    expectedPath,
                    AuditionPvCaptureContract.Width,
                    AuditionPvCaptureContract.Height);
            }
        }

        private static string[] CollectCaptureDependencyPaths()
        {
            var dependencies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string explicitPath in ExplicitProductDependencyPaths())
            {
                if (AssetDatabase.LoadMainAssetAtPath(explicitPath) == null &&
                    !File.Exists(Path.GetFullPath(explicitPath)))
                {
                    throw new FileNotFoundException(
                        "G04 explicit product dependency is missing.",
                        explicitPath);
                }

                dependencies.Add(explicitPath);
                foreach (string nested in AssetDatabase.GetDependencies(explicitPath, true))
                {
                    dependencies.Add(nested.Replace('\\', '/'));
                }
            }

            return AuditionPvEnvironmentProbe.CollectCaptureDependencyPaths(dependencies);
        }

        private static void ValidateStableGitSnapshot(
            AuditionPvGitSnapshot start,
            AuditionPvGitSnapshot end)
        {
            if (!end.probeSucceeded ||
                !string.Equals(start.commitSha, end.commitSha, StringComparison.Ordinal) ||
                !string.Equals(start.branch, end.branch, StringComparison.Ordinal) ||
                start.isDirty != end.isDirty ||
                !string.Equals(
                    start.dirtyStateHashSha256,
                    end.dirtyStateHashSha256,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "Git source state changed while G04 was rendering; discard this take.");
            }
        }

        private static void ValidateStableDependencies(
            AuditionPvDependencyHash[] start,
            AuditionPvDependencyHash[] end)
        {
            Dictionary<string, AuditionPvDependencyHash> endByPath = end.ToDictionary(
                dependency => dependency.path,
                StringComparer.OrdinalIgnoreCase);
            foreach (AuditionPvDependencyHash dependency in start)
            {
                if (!endByPath.TryGetValue(dependency.path, out AuditionPvDependencyHash current) ||
                    dependency.exists != current.exists ||
                    dependency.byteLength != current.byteLength ||
                    !string.Equals(dependency.sha256, current.sha256, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        $"Capture dependency changed while G04 was rendering: {dependency.path}");
                }
            }

            if (start.Length != end.Length)
            {
                throw new InvalidOperationException(
                    "G04 dependency set changed while rendering; discard this take.");
            }
        }

        private static AuditionPvTestResult[] CreateTestResults(
            string outputDirectory,
            CaptureMetrics metrics,
            DateTime startedAtUtc)
        {
            long duration = (long)(DateTime.UtcNow - startedAtUtc).TotalMilliseconds;
            double blackRatio = metrics.blackSampleCount / (double)metrics.sampleCount;
            double magentaRatio = metrics.magentaSampleCount / (double)metrics.sampleCount;
            return new[]
            {
                Passed("scene-orchestration", duration,
                    "Product scene reopened after exact HUD/look/audio/camera/skinning leases."),
                Passed("deterministic-frame-sequence", duration,
                    "Frames 0..237 inclusive, 2560x1440 PNG, direct Timeline Evaluate at 60fps.",
                    Path.Combine(outputDirectory, FramesFolderName)),
                Passed("exact-camera-cut", 0,
                    "C33 frame 95 -> C34 frame 96 at t=1.600000s."),
                Passed("c33-wing-expansion", 0,
                    $"frame18={metrics.wingClosedSpan:0.0000}; frame66={metrics.wingOpenSpan:0.0000}."),
                Passed("c34-iris-growth", 0,
                    $"frame103={metrics.closedIrisPixels}; frame178={metrics.openIrisPixels}."),
                Passed("black-magenta-sanity", 0,
                    $"black={blackRatio:P3}; magenta={magentaRatio:P3}; "
                    + $"healthy={metrics.healthyFrameCount}/{ExpectedFrameCount}."),
                Passed("source-stability", duration,
                    "Git dirty-state hash and every captured dependency hash remained stable."),
                Passed("baseline-extraction", 0,
                    $"BL04<-frame{Bl04SourceFrame}; BL05<-frame{Bl05SourceFrame}.",
                    Path.Combine(outputDirectory, BaselinesFolderName))
            };
        }

        private static AuditionPvTestResult Passed(
            string name,
            long durationMilliseconds,
            string details,
            string artifactPath = "")
        {
            return new AuditionPvTestResult
            {
                suite = nameof(AuditionPvStationTransitionGoldenCapture),
                name = name,
                status = "passed",
                durationMilliseconds = durationMilliseconds,
                details = details,
                artifactPath = artifactPath.Replace('\\', '/')
            };
        }

        private static void ValidateManifestRoundTrip(string manifestPath)
        {
            AuditionPvCaptureManifest manifest = JsonUtility.FromJson<AuditionPvCaptureManifest>(
                File.ReadAllText(manifestPath));
            AuditionPvCaptureManifestWriter.Validate(manifest);
            AuditionPvShotManifestEntry shot = manifest.shots.Single();
            if (shot.id != ShotId || shot.startFrame != FirstFrame ||
                shot.endFrame != LastFrame ||
                shot.expectedFrameCount != ExpectedFrameCount ||
                shot.hudMode != "hud-off" ||
                manifest.baselines.Length != 2)
            {
                throw new InvalidDataException(
                    "G04 capture manifest did not round-trip its exact shot contract.");
            }
        }

        private static void WriteCaptureReadme(string outputDirectory, CaptureMetrics metrics)
        {
            string path = Path.Combine(outputDirectory, "G04_CAPTURE_README.txt");
            string[] lines =
            {
                "DimensionBrawl Audition PV G04 - Station C33 to C34",
                $"Frames: {FirstFrame}..{LastFrame} ({ExpectedFrameCount})",
                $"Resolution: {AuditionPvCaptureContract.Width}x{AuditionPvCaptureContract.Height}",
                $"Frame rate: {AuditionPvCaptureContract.Fps} fps",
                "HUD: off (serialized Station flow combatHudCanvasGroup lease)",
                "Camera: C33 frames 0..95; C34 frames 96..237",
                $"BL04: frame {Bl04SourceFrame} / t=1.100000 / {Bl04FileName}",
                $"BL05: frame {Bl05SourceFrame} / t=2.966667 / {Bl05FileName}",
                $"Wing span: {metrics.wingClosedSpan:0.0000} -> {metrics.wingOpenSpan:0.0000}",
                $"Iris pixels: {metrics.closedIrisPixels} -> {metrics.openIrisPixels}",
                "Audio: muted; no audio is embedded in PNG source",
                "No-overwrite: capture directory and every artifact are create-new",
                "ffmpeg -framerate 60 -i G04_C33_C34_PNG/frame_%04d.png "
                    + "-c:v prores_ks -profile:v 3 G04_Station_C33_C34.mov"
            };
            WriteBytesNew(path, System.Text.Encoding.UTF8.GetBytes(
                string.Join(Environment.NewLine, lines) + Environment.NewLine));
        }

        private static void TryWriteFailureArtifact(string outputDirectory, Exception exception)
        {
            try
            {
                string path = Path.Combine(outputDirectory, "CAPTURE_FAILED.txt");
                if (!File.Exists(path))
                {
                    WriteBytesNew(
                        path,
                        System.Text.Encoding.UTF8.GetBytes(
                            DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture)
                            + Environment.NewLine
                            + exception
                            + Environment.NewLine));
                }
            }
            catch (Exception artifactException)
            {
                Debug.LogWarning(
                    "Could not write G04 failure artifact: " + artifactException.Message);
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
                .ToArray();
            if (renderers.Length != WingRendererNames.Length ||
                foundNames.Length != WingRendererNames.Length)
            {
                string missing = string.Join(", ",
                    WingRendererNames.Except(foundNames, StringComparer.Ordinal));
                throw new InvalidOperationException(
                    "C33 requires exactly six floating-blade renderers; "
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

        private static void CreateNewDirectory(string path)
        {
            if (Directory.Exists(path) || File.Exists(path))
            {
                throw new IOException($"Capture path already exists and will not be overwritten: {path}");
            }

            Directory.CreateDirectory(path);
        }

        private static void CopyNew(string sourcePath, string destinationPath)
        {
            if (!File.Exists(sourcePath))
            {
                throw new FileNotFoundException("Baseline source frame is missing.", sourcePath);
            }

            File.Copy(sourcePath, destinationPath, overwrite: false);
        }

        private static int ReadBigEndianInt32(byte[] bytes, int offset)
        {
            return bytes[offset] << 24 |
                   bytes[offset + 1] << 16 |
                   bytes[offset + 2] << 8 |
                   bytes[offset + 3];
        }

        private static void ValidateFrameIndex(int frameIndex)
        {
            if (frameIndex < FirstFrame || frameIndex > LastFrame)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(frameIndex),
                    $"G04 frame must be within {FirstFrame}..{LastFrame}.");
            }
        }

        private static void RequireNoAudioCommandLine()
        {
            if (!Environment.GetCommandLineArgs().Any(argument =>
                    string.Equals(argument, "-noaudio", StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException(
                    "Batch G04 capture requires -noaudio for deterministic silent source.");
            }
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private sealed class CaptureBindings
        {
            public GameObject transitionRoot;
            public GameObject phaseOneVisual;
            public GameObject actor;
            public PlayableDirector director;
            public TimelineAsset timeline;
            public Camera wingCamera;
            public Camera eyeCamera;
            public AkazaPhase2CinematicLookDriver lookDriver;
            public OlympusStationAkazaPhase2FlowController flowController;
            public Renderer[] wingRenderers;
            public Renderer eyeRenderer;
            public SkinnedMeshRenderer[] skinnedMeshes;
        }

        private sealed class CaptureMetrics
        {
            public long sampleCount;
            public long blackSampleCount;
            public long magentaSampleCount;
            public int healthyFrameCount;
            public int magentaFrameCount;
            public float wingClosedSpan = float.NaN;
            public float wingOpenSpan = float.NaN;
            public int closedIrisPixels = -1;
            public int openIrisPixels = -1;
        }

        private sealed class HudOffLease
        {
            private readonly CanvasGroup group;
            private readonly float alpha;
            private readonly bool interactable;
            private readonly bool blocksRaycasts;
            private bool held;

            public HudOffLease(OlympusStationAkazaPhase2FlowController flowController)
            {
                var serialized = new SerializedObject(flowController);
                serialized.Update();
                SerializedProperty property = serialized.FindProperty(HudSerializedPropertyName)
                    ?? throw new InvalidOperationException(
                        $"Station flow is missing serialized field {HudSerializedPropertyName}.");
                if (property.propertyType != SerializedPropertyType.ObjectReference)
                {
                    throw new InvalidOperationException(
                        $"Station flow field {HudSerializedPropertyName} is not an object reference.");
                }

                group = property.objectReferenceValue as CanvasGroup
                    ?? throw new InvalidOperationException(
                        $"Station flow field {HudSerializedPropertyName} has no CanvasGroup reference.");
                alpha = group.alpha;
                interactable = group.interactable;
                blocksRaycasts = group.blocksRaycasts;
            }

            public void Acquire()
            {
                if (held)
                {
                    return;
                }

                group.alpha = 0f;
                group.interactable = false;
                group.blocksRaycasts = false;
                held = true;
            }

            public void Release()
            {
                if (!held || group == null)
                {
                    return;
                }

                group.alpha = alpha;
                group.interactable = interactable;
                group.blocksRaycasts = blocksRaycasts;
                held = false;
            }
        }

        private sealed class SceneLease
        {
            private readonly CaptureBindings bindings;
            private readonly Camera[] cameras;
            private readonly bool[] cameraEnabled;
            private readonly AudioSource[] audioSources;
            private readonly bool[] audioMuted;
            private readonly bool[] updateWhenOffscreen;
            private readonly bool[] forceMatrixRecalculation;
            private readonly HudOffLease hudLease;
            private readonly bool transitionWasActive;
            private readonly bool phaseOneWasActive;
            private readonly double directorTime;
            private readonly float listenerVolume;
            private readonly bool listenerPaused;
            private readonly Volume phaseTwoLookVolume;
            private readonly float phaseTwoLookWeight;
            private bool applied;

            public SceneLease(Scene scene, CaptureBindings bindings)
            {
                this.bindings = bindings;
                cameras = FindSceneComponents<Camera>(scene);
                cameraEnabled = cameras.Select(camera => camera.enabled).ToArray();
                audioSources = FindSceneComponents<AudioSource>(scene);
                audioMuted = audioSources.Select(source => source.mute).ToArray();
                updateWhenOffscreen = bindings.skinnedMeshes
                    .Select(renderer => renderer.updateWhenOffscreen)
                    .ToArray();
                forceMatrixRecalculation = bindings.skinnedMeshes
                    .Select(renderer => renderer.forceMatrixRecalculationPerRender)
                    .ToArray();
                hudLease = new HudOffLease(bindings.flowController);
                GameplayLookStateController lookStateController =
                    bindings.lookDriver.LookStateController
                    ?? throw new InvalidOperationException(
                        "Station cinematic look driver has no GameplayLookStateController.");
                phaseTwoLookVolume = lookStateController.GetOverlayVolume(
                    GameplayLookState.Phase2Cinematic)
                    ?? throw new InvalidOperationException(
                        "Station look-state controller has no Phase2Cinematic Volume binding.");
                phaseTwoLookWeight = phaseTwoLookVolume.weight;
                transitionWasActive = bindings.transitionRoot.activeSelf;
                phaseOneWasActive = bindings.phaseOneVisual.activeSelf;
                directorTime = bindings.director.time;
                listenerVolume = AudioListener.volume;
                listenerPaused = AudioListener.pause;
            }

            public void Apply()
            {
                applied = true;
                hudLease.Acquire();
                AudioListener.volume = 0f;
                AudioListener.pause = true;
                foreach (AudioSource source in audioSources)
                {
                    source.mute = true;
                }

                bindings.phaseOneVisual.SetActive(false);
                bindings.transitionRoot.SetActive(true);
                bindings.lookDriver.BeginManualLightingLease();
                if (!bindings.lookDriver.PhaseTwoLookLeaseHeld)
                {
                    throw new InvalidOperationException(
                        "Station cinematic look driver could not acquire its Phase2 look lease.");
                }

                // Direct editor sampling has no runtime Update tick to advance
                // the blend. Hold the leased shot Volume at its final weight and
                // restore the authored weight when capture ends.
                phaseTwoLookVolume.weight = 1f;
                foreach (Camera camera in cameras)
                {
                    camera.enabled = false;
                }

                foreach (SkinnedMeshRenderer renderer in bindings.skinnedMeshes)
                {
                    renderer.updateWhenOffscreen = true;
                    renderer.forceMatrixRecalculationPerRender = true;
                }
            }

            public void Restore()
            {
                if (!applied)
                {
                    return;
                }

                Exception firstFailure = null;
                try
                {
                    try
                    {
                        if (bindings.director != null && bindings.director.playableAsset != null)
                        {
                            bindings.director.time = Math.Max(
                                0d,
                                Math.Min(bindings.director.duration, directorTime));
                            bindings.director.Evaluate();
                            bindings.director.Pause();
                        }
                    }
                    catch (Exception exception)
                    {
                        firstFailure = exception;
                    }

                    try
                    {
                        if (bindings.lookDriver != null)
                        {
                            bindings.lookDriver.EndManualLightingLease();
                        }
                    }
                    catch (Exception exception)
                    {
                        firstFailure ??= exception;
                    }

                    try
                    {
                        if (phaseTwoLookVolume != null)
                        {
                            phaseTwoLookVolume.weight = phaseTwoLookWeight;
                        }
                    }
                    catch (Exception exception)
                    {
                        firstFailure ??= exception;
                    }

                    try
                    {
                        if (bindings.transitionRoot != null)
                        {
                            bindings.transitionRoot.SetActive(transitionWasActive);
                        }

                        if (bindings.phaseOneVisual != null)
                        {
                            bindings.phaseOneVisual.SetActive(phaseOneWasActive);
                        }
                    }
                    catch (Exception exception)
                    {
                        firstFailure ??= exception;
                    }

                    try
                    {
                        for (int index = 0; index < bindings.skinnedMeshes.Length; index++)
                        {
                            SkinnedMeshRenderer renderer = bindings.skinnedMeshes[index];
                            if (renderer == null)
                            {
                                continue;
                            }

                            renderer.updateWhenOffscreen = updateWhenOffscreen[index];
                            renderer.forceMatrixRecalculationPerRender =
                                forceMatrixRecalculation[index];
                        }
                    }
                    catch (Exception exception)
                    {
                        firstFailure ??= exception;
                    }

                    try
                    {
                        for (int index = 0; index < audioSources.Length; index++)
                        {
                            if (audioSources[index] != null)
                            {
                                audioSources[index].mute = audioMuted[index];
                            }
                        }

                        AudioListener.volume = listenerVolume;
                        AudioListener.pause = listenerPaused;
                    }
                    catch (Exception exception)
                    {
                        firstFailure ??= exception;
                    }

                    try
                    {
                        hudLease.Release();
                    }
                    catch (Exception exception)
                    {
                        firstFailure ??= exception;
                    }

                    try
                    {
                        for (int index = 0; index < cameras.Length; index++)
                        {
                            if (cameras[index] != null)
                            {
                                cameras[index].enabled = cameraEnabled[index];
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        firstFailure ??= exception;
                    }
                }
                finally
                {
                    applied = false;
                }

                if (firstFailure != null)
                {
                    throw new InvalidOperationException(
                        "G04 scene lease restoration encountered an error.",
                        firstFailure);
                }
            }
        }
    }
}
