using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DimensionBrawl.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DimensionBrawl.Editor.Review.Cinematics
{
    /// <summary>
    /// Builds an editor-only compatibility proof for the authored C29 camera and actor
    /// animation. The actor clip is sampled onto DimensionBrawl's current Akaza prefab,
    /// keeping its current URP materials. No source geometry, background, audio, VFX, or
    /// completed Timeline is instantiated.
    /// </summary>
    public static class C29AkazaLegacyActorGrammarReviewSetup
    {
        public const string ReviewRoot =
            "Assets/_Game/Editor/Review/Cinematics/C29AkazaLegacyActorGrammar";
        public const string ScenePath = ReviewRoot + "/C29AkazaLegacyActorGrammarReview.unity";
        public const string ProfilePath = ReviewRoot + "/DB_Cinematic_C29_AkazaLegacyActorGrammar.asset";
        public const string CameraAssetPath =
            "Assets/_Game/Art/Animations/Cinematics/LegacyCameraGrammar/C29_cam.fbx";
        public const string ActorAssetPath =
            "Assets/_Game/Art/Animations/Cinematics/LegacyActorGrammar/C29_akaza.fbx";
        public const string CurrentAkazaPrefabPath =
            "Assets/_Game/Prefabs/Enemies/ActionFoundation/PF_Boss_Akaza_Phase2Review.prefab";

        private const string CameraCueId = "legacy_c29_camera_exact";
        private const string ActorCueId = "legacy_c29_akaza_exact";
        private const string MainCameraName = "C29Akaza_MainCamera";
        private const string SourceCameraRigName = "C29Akaza_SourceCameraRig";
        private const string CurrentActorName = "C29Akaza_CurrentModel_CurrentURPMaterials";
        private const string RunnerName = "C29Akaza_SequenceRunner";
        private const float AuthoredDurationSeconds = 4.5f;
        private const string CaptureDirectory = "C:/tmp/DimensionBrawl-C29AkazaLegacyActorGrammar-Frames";
        private const string VideoFrameDirectory = "C:/tmp/DimensionBrawl-C29AkazaLegacyActorGrammar-VideoFrames";
        private const string ContactSheetPath = "C:/tmp/DimensionBrawl-C29AkazaLegacyActorGrammar-ContactSheet.png";
        private const string ReportPath = "C:/tmp/DimensionBrawl-C29AkazaLegacyActorGrammar-Report.md";

        private static readonly float[] CaptureTimes = { 0.05f, 1.35f, 2.10f, 3.20f, 4.45f };
        private static readonly string[] CaptureLabels =
        {
            "establish",
            "approach",
            "closeup-beat",
            "reaction",
            "settle"
        };

        [MenuItem("DimensionBrawl/Review/Cinematics/Rebuild C29 Akaza Legacy Actor Grammar")]
        public static void RebuildReviewMenu()
        {
            BuildReview(openScene: true);
            Debug.Log($"C29 Akaza legacy actor review rebuilt at {ScenePath}.");
        }

        [MenuItem("DimensionBrawl/Review/Cinematics/Capture C29 Akaza Legacy Actor Grammar")]
        public static void CaptureReviewMenu()
        {
            BuildReview(openScene: true);
            CaptureReviewFrames();
            Debug.Log($"C29 Akaza contact sheet written to {ContactSheetPath}.");
        }

        [MenuItem("DimensionBrawl/Review/Cinematics/Capture C29 Akaza 30fps Video Frames")]
        public static void CaptureVideoFramesMenu()
        {
            BuildReview(openScene: true);
            CaptureReviewVideoFrames();
            Debug.Log($"C29 Akaza 30fps video frames written to {VideoFrameDirectory}.");
        }

        public static void RunBatchSetup()
        {
            RunBatch(() => BuildReview(openScene: false));
        }

        public static void RunBatchCapture()
        {
            RunBatch(() =>
            {
                BuildReview(openScene: true);
                CaptureReviewFrames();
            });
        }

        public static void RunBatchVideoFrames()
        {
            RunBatch(() =>
            {
                BuildReview(openScene: true);
                CaptureReviewVideoFrames();
            });
        }

        public static void BuildReview(bool openScene)
        {
            EnsureFolder(ReviewRoot);
            AssetDatabase.ImportAsset(CameraAssetPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset(ActorAssetPath, ImportAssetOptions.ForceUpdate);

            AnimationClip cameraClip = LoadPrimaryClip(CameraAssetPath)
                ?? throw new InvalidOperationException($"C29 camera clip missing at {CameraAssetPath}.");
            AnimationClip actorClip = LoadPrimaryClip(ActorAssetPath)
                ?? throw new InvalidOperationException($"C29 actor clip missing at {ActorAssetPath}.");
            ValidateClipLength("camera", cameraClip);
            ValidateClipLength("actor", actorClip);

            CinematicSequenceProfile profile = EnsureProfile(cameraClip, actorClip);
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "C29AkazaLegacyActorGrammarReview";

            GameObject currentAkaza = InstantiateCurrentAkaza(scene);
            GameObject sourceCameraRig = InstantiateAsset(CameraAssetPath, null);
            sourceCameraRig.name = SourceCameraRigName;
            SceneManager.MoveGameObjectToScene(sourceCameraRig, scene);
            Camera sourceCamera = sourceCameraRig.GetComponentInChildren<Camera>(includeInactive: true)
                ?? throw new InvalidOperationException("Imported C29 camera asset has no Camera component.");
            sourceCamera.enabled = false;

            GameObject mainCameraObject = new GameObject(MainCameraName);
            mainCameraObject.tag = "MainCamera";
            Camera mainCamera = mainCameraObject.AddComponent<Camera>();
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = new Color(0.006f, 0.009f, 0.018f, 1f);
            mainCamera.nearClipPlane = 0.01f;
            mainCamera.farClipPlane = 500f;
            mainCamera.allowHDR = true;
            mainCamera.allowMSAA = true;
            ApplySourceCameraSample(cameraClip, sourceCameraRig, sourceCamera, mainCamera, 0f);

            CreateNeutralReviewLighting();

            GameObject runnerObject = new GameObject(RunnerName);
            CinematicSequenceRunner runner = runnerObject.AddComponent<CinematicSequenceRunner>();
            ConfigureRunner(runner, profile, currentAkaza, mainCamera, sourceCameraRig, sourceCamera);

            CinematicSequenceAutoPlay autoPlay = runnerObject.AddComponent<CinematicSequenceAutoPlay>();
            SerializedObject autoPlayObject = new SerializedObject(autoPlay);
            SetObject(autoPlayObject, "runner", runner);
            SetBool(autoPlayObject, "playOnStart", true);
            SetFloat(autoPlayObject, "startDelaySeconds", 0.1f);
            autoPlayObject.ApplyModifiedPropertiesWithoutUndo();

            CreateReviewMarker(cameraClip, actorClip);
            ValidateScene(scene, profile, cameraClip, actorClip, currentAkaza, sourceCamera, mainCamera);

            EditorUtility.SetDirty(runner);
            EditorUtility.SetDirty(autoPlay);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
            {
                throw new InvalidOperationException($"Failed to save review scene at {ScenePath}.");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            if (!openScene)
            {
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            }
        }

        private static CinematicSequenceProfile EnsureProfile(
            AnimationClip cameraClip,
            AnimationClip actorClip)
        {
            CinematicSequenceProfile profile =
                AssetDatabase.LoadAssetAtPath<CinematicSequenceProfile>(ProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CinematicSequenceProfile>();
                AssetDatabase.CreateAsset(profile, ProfilePath);
            }

            profile.Configure(
                "c29_akaza_legacy_actor_grammar_review",
                "C29 Akaza - Exact Camera and Actor Grammar Review",
                CinematicSequenceProfile.SequenceCategory.BossIntro,
                "Editor-only compatibility proof: exact 4.5-second C29 camera and Akaza actor clips sampled onto the current Akaza model with current URP materials. Source geometry, background, audio, VFX, and completed Timeline are excluded.",
                AuthoredDurationSeconds,
                100,
                newLockMovement: true,
                newLockInput: true,
                newHideHud: true,
                newCanSkip: true,
                newUseUnscaledClock: true,
                Array.Empty<CinematicSequenceProfile.CameraCue>(),
                Array.Empty<CinematicSequenceProfile.ActorCue>(),
                Array.Empty<CinematicSequenceProfile.VfxCue>(),
                Array.Empty<CinematicSequenceProfile.TutorialCue>(),
                default);
            profile.ConfigureSourceCameraAnimation(
                new CinematicSequenceProfile.SourceCameraAnimationCue(
                    CameraCueId,
                    cameraClip,
                    0f,
                    0f,
                    AuthoredDurationSeconds));
            profile.ConfigureSourceActorAnimation(
                new CinematicSequenceProfile.SourceActorAnimationCue(
                    ActorCueId,
                    actorClip,
                    0f,
                    0f,
                    AuthoredDurationSeconds));
            profile.ConfigureSourceActorGrades(Array.Empty<CinematicSequenceProfile.SourceActorGradeCue>());
            profile.ConfigureScreenFades(Array.Empty<CinematicSequenceProfile.ScreenFadeCue>());
            profile.ConfigureStageContext(
                null,
                string.Empty,
                string.Empty,
                string.Empty,
                "Editor-only playback proof; deliberately excluded from product stage routing.",
                false);
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static GameObject InstantiateCurrentAkaza(Scene scene)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CurrentAkazaPrefabPath)
                ?? throw new InvalidOperationException($"Current Akaza prefab missing at {CurrentAkazaPrefabPath}.");
            GameObject actor = PrefabUtility.InstantiatePrefab(prefab, scene) as GameObject
                ?? throw new InvalidOperationException("Failed to instantiate the current Akaza prefab.");
            actor.name = CurrentActorName;
            actor.transform.SetParent(null, false);
            actor.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            actor.transform.localScale = Vector3.one;
            actor.SetActive(true);

            MonoBehaviour[] behaviours = actor.GetComponentsInChildren<MonoBehaviour>(includeInactive: true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                UnityEngine.Object.DestroyImmediate(behaviours[i]);
            }

            Animator[] animators = actor.GetComponentsInChildren<Animator>(includeInactive: true);
            for (int i = 0; i < animators.Length; i++)
            {
                UnityEngine.Object.DestroyImmediate(animators[i]);
            }

            AudioSource[] audioSources = actor.GetComponentsInChildren<AudioSource>(includeInactive: true);
            for (int i = 0; i < audioSources.Length; i++)
            {
                UnityEngine.Object.DestroyImmediate(audioSources[i]);
            }

            ParticleSystem[] particles = actor.GetComponentsInChildren<ParticleSystem>(includeInactive: true);
            for (int i = 0; i < particles.Length; i++)
            {
                UnityEngine.Object.DestroyImmediate(particles[i].gameObject);
            }

            foreach (TrailRenderer trail in actor.GetComponentsInChildren<TrailRenderer>(includeInactive: true))
            {
                UnityEngine.Object.DestroyImmediate(trail);
            }

            foreach (LineRenderer line in actor.GetComponentsInChildren<LineRenderer>(includeInactive: true))
            {
                UnityEngine.Object.DestroyImmediate(line);
            }

            Renderer[] renderers = actor.GetComponentsInChildren<Renderer>(includeInactive: true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                bool reviewVfx = renderer.gameObject.name.IndexOf("aura", StringComparison.OrdinalIgnoreCase) >= 0
                    || renderer.sharedMaterials.Any(material => material != null
                        && material.name.IndexOf("aura", StringComparison.OrdinalIgnoreCase) >= 0);
                if (reviewVfx)
                {
                    UnityEngine.Object.DestroyImmediate(renderer.gameObject);
                }
            }

            return actor;
        }

        private static void ConfigureRunner(
            CinematicSequenceRunner runner,
            CinematicSequenceProfile profile,
            GameObject currentAkaza,
            Camera mainCamera,
            GameObject sourceCameraRig,
            Camera sourceCamera)
        {
            SerializedObject serialized = new SerializedObject(runner);
            SetObject(serialized, "sequenceProfile", profile);
            SetObject(serialized, "cinematicCamera", mainCamera);
            SetBool(serialized, "driveCameraTransformFromProfile", true);
            SetBool(serialized, "disableActionCameraControllerDuringPoseDrive", true);
            SetObject(serialized, "sourceCameraRigRoot", sourceCameraRig);
            SetObject(serialized, "sourceCameraTransform", sourceCamera.transform);
            SetObject(serialized, "sourceCameraComponent", sourceCamera);
            SetObject(serialized, "sourceActorRigRoot", currentAkaza);
            SetObject(serialized, "sourceActorVisibilityRoot", currentAkaza);
            SetObject(serialized, "cueSpace", currentAkaza.transform);
            SetFloat(serialized, "maxPlaybackDeltaSeconds", 1f / 60f);

            SerializedProperty cameraBindings = Require(serialized, "sourceCameraBindings");
            cameraBindings.arraySize = 1;
            SerializedProperty cameraBinding = cameraBindings.GetArrayElementAtIndex(0);
            RequireRelative(cameraBinding, "cueId").stringValue = CameraCueId;
            RequireRelative(cameraBinding, "rigRoot").objectReferenceValue = sourceCameraRig;
            RequireRelative(cameraBinding, "cameraTransform").objectReferenceValue = sourceCamera.transform;
            RequireRelative(cameraBinding, "cameraComponent").objectReferenceValue = sourceCamera;

            SerializedProperty actorBindings = Require(serialized, "sourceActorBindings");
            actorBindings.arraySize = 1;
            SerializedProperty actorBinding = actorBindings.GetArrayElementAtIndex(0);
            RequireRelative(actorBinding, "cueId").stringValue = ActorCueId;
            RequireRelative(actorBinding, "rigRoot").objectReferenceValue = currentAkaza;
            RequireRelative(actorBinding, "visibilityRoot").objectReferenceValue = currentAkaza;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateNeutralReviewLighting()
        {
            GameObject key = new GameObject("C29Akaza_NeutralKeyLight");
            Light keyLight = key.AddComponent<Light>();
            keyLight.type = LightType.Directional;
            keyLight.color = new Color(0.82f, 0.90f, 1f);
            keyLight.intensity = 1.15f;
            key.transform.rotation = Quaternion.Euler(42f, -28f, 0f);

            GameObject rim = new GameObject("C29Akaza_NeutralRimLight");
            Light rimLight = rim.AddComponent<Light>();
            rimLight.type = LightType.Directional;
            rimLight.color = new Color(0.36f, 0.55f, 1f);
            rimLight.intensity = 0.55f;
            rim.transform.rotation = Quaternion.Euler(28f, 145f, 0f);
        }

        private static void CreateReviewMarker(AnimationClip cameraClip, AnimationClip actorClip)
        {
            GameObject marker = new GameObject(
                $"EDITOR_ONLY__C29_CAMERA_{cameraClip.length:0.###}s_ACTOR_{actorClip.length:0.###}s__NO_SOURCE_GEOMETRY_AUDIO_VFX_TIMELINE");
            marker.transform.position = new Vector3(10000f, 10000f, 10000f);
        }

        private static void ValidateScene(
            Scene scene,
            CinematicSequenceProfile profile,
            AnimationClip cameraClip,
            AnimationClip actorClip,
            GameObject actor,
            Camera sourceCamera,
            Camera mainCamera)
        {
            ValidateClipLength("camera", cameraClip);
            ValidateClipLength("actor", actorClip);
            if (profile.SourceCameraAnimations.Length != 1 || profile.SourceActorAnimations.Length != 1)
            {
                throw new InvalidOperationException("Review profile must contain exactly one source camera and one source actor cue.");
            }

            Renderer[] renderers = actor.GetComponentsInChildren<Renderer>(includeInactive: true);
            if (renderers.Length == 0)
            {
                throw new InvalidOperationException("Current Akaza review actor has no renderers.");
            }

            Material[] materials = renderers
                .SelectMany(renderer => renderer.sharedMaterials)
                .Where(material => material != null)
                .Distinct()
                .ToArray();
            string[] materialPaths = materials.Select(AssetDatabase.GetAssetPath).ToArray();
            if (materials.Length == 0
                || materials.Any(material => material.shader == null
                    || string.Equals(material.shader.name, "Hidden/InternalErrorShader", StringComparison.Ordinal)
                    || !material.shader.isSupported)
                || materialPaths.Any(path => string.IsNullOrWhiteSpace(path)
                    || path.StartsWith(
                        "Assets/_Game/Art/Animations/Cinematics/LegacyActorGrammar/",
                        StringComparison.OrdinalIgnoreCase)))
            {
                string evidence = string.Join(", ", materials.Select(material => material == null
                    ? "<null>"
                    : $"{AssetDatabase.GetAssetPath(material)}:{material.shader?.name}"));
                throw new InvalidOperationException(
                    $"Visible actor is not using only the current supported Akaza material set: {evidence}");
            }

            if (sourceCamera.enabled || !mainCamera.enabled)
            {
                throw new InvalidOperationException("Review must render through the main camera only.");
            }

            if (FindSceneComponents<AudioSource>(scene).Length != 0)
            {
                throw new InvalidOperationException("C29 Akaza review must not contain audio sources.");
            }

            if (FindSceneComponents<ParticleSystem>(scene).Length != 0
                || FindSceneComponents<TrailRenderer>(scene).Length != 0
                || FindSceneComponents<LineRenderer>(scene).Length != 0)
            {
                throw new InvalidOperationException("C29 Akaza review must not contain VFX renderers or particle systems.");
            }

            if (EditorBuildSettings.scenes.Any(entry =>
                    string.Equals(entry.path, ScenePath, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("Editor-only C29 Akaza review leaked into build settings.");
            }
        }

        private static void ValidateClipLength(string label, AnimationClip clip)
        {
            if (clip.length + 0.02f < AuthoredDurationSeconds)
            {
                throw new InvalidOperationException(
                    $"C29 {label} clip is {clip.length:0.###}s; expected at least {AuthoredDurationSeconds:0.###}s.");
            }
        }

        private static void ApplySourceCameraSample(
            AnimationClip clip,
            GameObject rigRoot,
            Camera source,
            Camera destination,
            float seconds)
        {
            clip.SampleAnimation(rigRoot, Mathf.Clamp(seconds, 0f, clip.length));
            destination.transform.SetPositionAndRotation(source.transform.position, source.transform.rotation);
            destination.fieldOfView = source.fieldOfView;
        }

        private static void CaptureReviewFrames()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            CinematicSequenceRunner runner = FindSceneComponent<CinematicSequenceRunner>(scene)
                ?? throw new InvalidOperationException("C29 Akaza runner missing during capture.");
            CinematicSequenceProfile profile =
                AssetDatabase.LoadAssetAtPath<CinematicSequenceProfile>(ProfilePath)
                ?? throw new InvalidOperationException("C29 Akaza profile missing during capture.");
            Camera camera = runner.CinematicCamera
                ?? throw new InvalidOperationException("C29 Akaza camera missing during capture.");
            GameObject actor = scene.GetRootGameObjects().FirstOrDefault(root =>
                string.Equals(root.name, CurrentActorName, StringComparison.Ordinal))
                ?? throw new InvalidOperationException("Current Akaza actor missing during capture.");

            Directory.CreateDirectory(CaptureDirectory);
            List<Texture2D> frames = new List<Texture2D>(CaptureTimes.Length);
            List<string> framePaths = new List<string>(CaptureTimes.Length);
            List<float> foregroundRatios = new List<float>(CaptureTimes.Length);
            try
            {
                for (int i = 0; i < CaptureTimes.Length; i++)
                {
                    float sampleTime = CaptureTimes[i];
                    if (!runner.TryApplyProfileSampleForReview(profile, sampleTime, Vector3.forward))
                    {
                        throw new InvalidOperationException($"Failed to sample C29 Akaza review at {sampleTime:0.###}s.");
                    }

                    string framePath = Path.Combine(
                            CaptureDirectory,
                            $"{i + 1:00}_{CaptureLabels[i]}_t{sampleTime:0.00}.png")
                        .Replace('\\', '/');
                    Texture2D frame = CaptureCamera(camera, 1280, 720);
                    File.WriteAllBytes(framePath, frame.EncodeToPNG());
                    frames.Add(frame);
                    framePaths.Add(framePath);
                    foregroundRatios.Add(MeasureForegroundRatio(frame, camera.backgroundColor));
                }

                if (foregroundRatios.Max() < 0.01f)
                {
                    throw new InvalidOperationException("Captured C29 frames contain no visible Akaza silhouette.");
                }

                Texture2D sheet = BuildContactSheet(frames, 384, 216);
                try
                {
                    File.WriteAllBytes(ContactSheetPath, sheet.EncodeToPNG());
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(sheet);
                }

                WriteReport(profile, actor, framePaths, foregroundRatios, camera);
            }
            finally
            {
                for (int i = 0; i < frames.Count; i++)
                {
                    UnityEngine.Object.DestroyImmediate(frames[i]);
                }
            }
        }

        private static void CaptureReviewVideoFrames()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            CinematicSequenceRunner runner = FindSceneComponent<CinematicSequenceRunner>(scene)
                ?? throw new InvalidOperationException("C29 Akaza runner missing during video-frame capture.");
            CinematicSequenceProfile profile =
                AssetDatabase.LoadAssetAtPath<CinematicSequenceProfile>(ProfilePath)
                ?? throw new InvalidOperationException("C29 Akaza profile missing during video-frame capture.");
            Camera camera = runner.CinematicCamera
                ?? throw new InvalidOperationException("C29 Akaza camera missing during video-frame capture.");

            if (Directory.Exists(VideoFrameDirectory))
            {
                Directory.Delete(VideoFrameDirectory, recursive: true);
            }

            Directory.CreateDirectory(VideoFrameDirectory);
            const int framesPerSecond = 30;
            const int width = 640;
            const int height = 360;
            int frameCount = Mathf.RoundToInt(AuthoredDurationSeconds * framesPerSecond);
            for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
            {
                float sampleTime = frameIndex / (float)framesPerSecond;
                if (!runner.TryApplyProfileSampleForReview(profile, sampleTime, Vector3.forward))
                {
                    throw new InvalidOperationException(
                        $"Failed to sample C29 Akaza video frame {frameIndex} at {sampleTime:0.###}s.");
                }

                Texture2D frame = CaptureCamera(camera, width, height);
                try
                {
                    string framePath = Path.Combine(
                        VideoFrameDirectory,
                        $"frame_{frameIndex:0000}.png");
                    File.WriteAllBytes(framePath, frame.EncodeToPNG());
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(frame);
                }
            }

            string readmePath = Path.Combine(VideoFrameDirectory, "README.txt");
            File.WriteAllLines(readmePath, new[]
            {
                $"Frames: {frameCount}",
                $"Frame rate: {framesPerSecond} fps",
                $"Exact playback duration: {frameCount / (float)framesPerSecond:0.000} seconds",
                $"Resolution: {width}x{height}",
                "Audio: none",
                "Range: frame_0000.png through frame_0134.png",
                "ffmpeg -framerate 30 -i frame_%04d.png -c:v libx264 -pix_fmt yuv420p C29_Akaza_Grammar_4p5s.mp4"
            });

            string[] writtenFrames = Directory.GetFiles(VideoFrameDirectory, "frame_*.png");
            if (writtenFrames.Length != frameCount)
            {
                throw new InvalidOperationException(
                    $"Expected {frameCount} video frames but found {writtenFrames.Length}.");
            }
        }

        private static Texture2D CaptureCamera(Camera camera, int width, int height)
        {
            RenderTexture previousTarget = camera.targetTexture;
            RenderTexture previousActive = RenderTexture.active;
            RenderTexture renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, mipChain: false);
            try
            {
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
                UnityEngine.Object.DestroyImmediate(renderTexture);
            }
        }

        private static float MeasureForegroundRatio(Texture2D texture, Color background)
        {
            Color[] pixels = texture.GetPixels();
            int foreground = 0;
            for (int i = 0; i < pixels.Length; i += 8)
            {
                Color pixel = pixels[i];
                float delta = Mathf.Abs(pixel.r - background.r)
                    + Mathf.Abs(pixel.g - background.g)
                    + Mathf.Abs(pixel.b - background.b);
                if (delta > 0.08f)
                {
                    foreground++;
                }
            }

            return foreground / (float)Mathf.Max(1, Mathf.CeilToInt(pixels.Length / 8f));
        }

        private static Texture2D BuildContactSheet(IReadOnlyList<Texture2D> frames, int width, int height)
        {
            Texture2D sheet = new Texture2D(width * frames.Count, height, TextureFormat.RGBA32, false);
            sheet.SetPixels(Enumerable.Repeat(
                new Color(0.01f, 0.015f, 0.025f, 1f),
                sheet.width * sheet.height).ToArray());
            for (int i = 0; i < frames.Count; i++)
            {
                Texture2D source = frames[i];
                Color[] pixels = new Color[width * height];
                for (int y = 0; y < height; y++)
                {
                    float v = y / (float)Mathf.Max(1, height - 1);
                    for (int x = 0; x < width; x++)
                    {
                        float u = x / (float)Mathf.Max(1, width - 1);
                        pixels[x + (y * width)] = source.GetPixelBilinear(u, v);
                    }
                }

                sheet.SetPixels(i * width, 0, width, height, pixels);
            }

            sheet.Apply();
            return sheet;
        }

        private static void WriteReport(
            CinematicSequenceProfile profile,
            GameObject actor,
            IReadOnlyList<string> framePaths,
            IReadOnlyList<float> foregroundRatios,
            Camera camera)
        {
            Renderer[] renderers = actor.GetComponentsInChildren<Renderer>(includeInactive: true);
            string[] materialEvidence = renderers
                .SelectMany(renderer => renderer.sharedMaterials)
                .Where(material => material != null)
                .Distinct()
                .Select(material =>
                    $"`{AssetDatabase.GetAssetPath(material)}` ({material.shader.name})")
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            AnimationClip cameraClip = profile.SourceCameraAnimations[0].Clip;
            AnimationClip actorClip = profile.SourceActorAnimations[0].Clip;
            List<string> lines = new List<string>
            {
                "# C29 Akaza exact camera + actor grammar review",
                string.Empty,
                "- Result: PASS",
                $"- Scene: `{ScenePath}`",
                $"- Profile: `{ProfilePath}`",
                $"- Exact camera clip: `{CameraAssetPath}` ({cameraClip.length:0.###}s)",
                $"- Exact actor clip, animation only: `{ActorAssetPath}` ({actorClip.length:0.###}s)",
                $"- Current visible actor prefab: `{CurrentAkazaPrefabPath}`",
                $"- Current Akaza renderers: {renderers.Length}",
                $"- Runtime camera FOV after final sample: {camera.fieldOfView:0.##}",
                $"- Source actor cues: {profile.SourceActorAnimations.Length}",
                $"- Source camera cues: {profile.SourceCameraAnimations.Length}",
                $"- Audio sources: {FindSceneComponents<AudioSource>(actor.scene).Length}",
                $"- Particle systems: {FindSceneComponents<ParticleSystem>(actor.scene).Length}",
                $"- Contact sheet: `{ContactSheetPath}`",
                string.Empty,
                "## Current URP material evidence",
                string.Empty
            };
            lines.AddRange(materialEvidence.Select(value => "- " + value));
            lines.Add(string.Empty);
            lines.Add("## Captured frames");
            lines.Add(string.Empty);
            for (int i = 0; i < framePaths.Count; i++)
            {
                lines.Add($"- T+{CaptureTimes[i]:0.00}s foreground {foregroundRatios[i] * 100f:0.00}% `{framePaths[i]}`");
            }

            lines.Add(string.Empty);
            lines.Add("The source FBX is never instantiated as geometry. Only its AnimationClip is sampled onto the current Akaza prefab. No source background, audio, VFX, or completed Timeline is present.");
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath) ?? "C:/tmp");
            File.WriteAllLines(ReportPath, lines);
        }

        private static AnimationClip LoadPrimaryClip(string path)
        {
            return AssetDatabase.LoadAllAssetsAtPath(path)
                .OfType<AnimationClip>()
                .FirstOrDefault(clip => !clip.name.StartsWith("__preview__", StringComparison.Ordinal));
        }

        private static GameObject InstantiateAsset(string path, Transform parent)
        {
            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path)
                ?? throw new InvalidOperationException($"GameObject asset missing at {path}.");
            GameObject instance = PrefabUtility.InstantiatePrefab(asset) as GameObject
                ?? throw new InvalidOperationException($"Failed to instantiate {path}.");
            if (parent != null)
            {
                instance.transform.SetParent(parent, false);
            }

            return instance;
        }

        private static T FindSceneComponent<T>(Scene scene) where T : Component
        {
            return FindSceneComponents<T>(scene).FirstOrDefault();
        }

        private static T[] FindSceneComponents<T>(Scene scene) where T : Component
        {
            return scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<T>(includeInactive: true))
                .ToArray();
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string normalized = path.Replace('\\', '/');
            int split = normalized.LastIndexOf('/');
            string parent = normalized.Substring(0, split);
            string name = normalized.Substring(split + 1);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }

        private static void SetObject(SerializedObject serialized, string propertyName, UnityEngine.Object value)
        {
            Require(serialized, propertyName).objectReferenceValue = value;
        }

        private static void SetBool(SerializedObject serialized, string propertyName, bool value)
        {
            Require(serialized, propertyName).boolValue = value;
        }

        private static void SetFloat(SerializedObject serialized, string propertyName, float value)
        {
            Require(serialized, propertyName).floatValue = value;
        }

        private static SerializedProperty Require(SerializedObject serialized, string propertyName)
        {
            return serialized.FindProperty(propertyName)
                ?? throw new InvalidOperationException(
                    $"Missing serialized property {serialized.targetObject.GetType().Name}.{propertyName}.");
        }

        private static SerializedProperty RequireRelative(SerializedProperty parent, string propertyName)
        {
            return parent.FindPropertyRelative(propertyName)
                ?? throw new InvalidOperationException($"Missing serialized relative property {propertyName}.");
        }

        private static void RunBatch(Action action)
        {
            try
            {
                action();
                Debug.Log("C29 Akaza legacy actor grammar batch operation passed.");
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(0);
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }

                throw;
            }
        }
    }

}
