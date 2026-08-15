using System;
using System.Collections.Generic;
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
using UnityEngine.UI;

namespace DimensionBrawl.Editor.Review.Cinematics
{
    /// <summary>
    /// Creates a reopenable editor-only proof of the verified TPK C34 eye-opening
    /// push-in. Exact C34 camera/actor clips drive DimensionBrawl's current Akaza
    /// model and material binding. Source geometry, materials, background, audio,
    /// VFX, and completed Timeline are intentionally absent.
    /// </summary>
    public static class C34EyeOpenCurrentAkazaReviewSetup
    {
        public const string ReviewRoot =
            "Assets/_Game/Editor/Review/Cinematics/C34EyeOpenCurrentAkaza";
        public const string ScenePath = ReviewRoot + "/C34EyeOpenCurrentAkazaReview.unity";
        public const string ProfilePath = ReviewRoot + "/DB_Cinematic_C34EyeOpen_CurrentAkaza.asset";
        public const string TimelinePath = ReviewRoot + "/DB_Timeline_C34EyeOpen_SourcePlayable.playable";
        public const string CameraAssetPath =
            "Assets/_Game/Art/Animations/Cinematics/LegacyCameraGrammar/C34_Cam.fbx";
        public const string ActorAnimationAssetPath =
            "Assets/_Game/Art/Characters/Bosses/Akaza/Animations/Source/C34_Akaza.fbx";
        public const string CurrentAkazaPrefabPath =
            "Assets/_Game/Prefabs/Enemies/ActionFoundation/PF_Boss_Akaza_Phase2Review.prefab";

        private const string CameraCueId = "legacy_c34_camera_exact";
        private const string ActorCueId = "legacy_c34_akaza_exact";
        private const string CurrentActorName = "C34_CurrentAkaza_CurrentURPMaterials";
        private const string DirectorName = "C34EyeOpen_SourcePlayableDirector";
        private const string ActorTrackName = "C34 Actor - Source FBX (RemoveStartOffset)";
        private const string CameraTrackName = "C34 Camera - Source FBX (RemoveStartOffset)";
        private const float DurationSeconds = 2.3666667f;
        private const float TimelineStartSeconds = 89.933333f;
        private const float TimelineEndSeconds = 92.300000f;
        private const int PreviewFps = 30;
        private const int PreviewWidth = 640;
        private const int PreviewHeight = 360;
        private const string OutputRoot = "C:/tmp/DimensionBrawl-C34EyeOpenCandidate";
        private const string VerificationPath =
            OutputRoot + "/C34_ReopenableScene_Verification.png";
        private const string EvidenceDirectory = OutputRoot + "/EvidenceFrames";
        private const string VideoFrameDirectory = OutputRoot + "/VideoFrames30fps";
        private const string ContactSheetPath = OutputRoot + "/C34_ClosedSlitOpen_ContactSheet.png";
        private const string HeroTriptychPath = OutputRoot + "/C34_ClosedSlitOpen_HeroTriptych.png";
        private const string PreviewVideoPath = OutputRoot + "/C34_Akaza_EyeOpen_2.3667s_30fps.mp4";
        private const string ReportPath = OutputRoot + "/README.md";

        private static readonly float[] VerificationTimes =
        {
            0.116667f,
            0.616667f,
            1.366667f,
            2.296667f
        };

        private static readonly EvidenceBeat[] EvidenceBeats =
        {
            new EvidenceBeat("pre-roll", 89.950000f, "PRE-ROLL"),
            new EvidenceBeat("eyes-closed", 90.050000f, "EYES CLOSED"),
            new EvidenceBeat("eye-slit", 90.550000f, "EYE SLIT"),
            new EvidenceBeat("eyes-open", 91.300000f, "EYES FULLY OPEN"),
            new EvidenceBeat("push-in-settle", 92.230000f, "PUSH-IN SETTLE")
        };

        private readonly struct EvidenceBeat
        {
            public EvidenceBeat(string fileLabel, float sourceSeconds, string displayLabel)
            {
                FileLabel = fileLabel;
                SourceSeconds = sourceSeconds;
                DisplayLabel = displayLabel;
            }

            public string FileLabel { get; }
            public float SourceSeconds { get; }
            public float LocalSeconds => SourceSeconds - TimelineStartSeconds;
            public string DisplayLabel { get; }
        }

        [MenuItem("DimensionBrawl/Review/Cinematics/Rebuild C34 Eye-Open Current Akaza Review")]
        public static void RebuildReviewMenu()
        {
            BuildReview(openScene: true);
            Debug.Log($"C34 eye-open review rebuilt at {ScenePath}.");
        }

        [MenuItem("DimensionBrawl/Review/Cinematics/Capture C34 Reopenable Review Verification")]
        public static void CaptureVerificationMenu()
        {
            BuildReview(openScene: true);
            CaptureVerification();
            Debug.Log($"C34 reopenable review verification written to {VerificationPath}.");
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
                CaptureVerification();
            });
        }

        public static void BuildReview(bool openScene)
        {
            EnsureFolder(ReviewRoot);
            AssetDatabase.ImportAsset(CameraAssetPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset(ActorAnimationAssetPath, ImportAssetOptions.ForceUpdate);

            AnimationClip cameraClip = LoadPrimaryClip(CameraAssetPath)
                ?? throw new InvalidOperationException($"C34 camera clip missing at {CameraAssetPath}.");
            AnimationClip actorClip = LoadPrimaryClip(ActorAnimationAssetPath)
                ?? throw new InvalidOperationException($"C34 actor clip missing at {ActorAnimationAssetPath}.");
            ValidateClipLength("camera", cameraClip);
            ValidateClipLength("actor", actorClip);
            CinematicSequenceProfile profile = EnsureProfile(cameraClip, actorClip);
            TimelineBindings timelineBindings = EnsureTimeline(cameraClip, actorClip);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "C34EyeOpenCurrentAkazaReview";
            GameObject actor = InstantiateCurrentAkaza(scene);
            Animator actorAnimator = EnsureTimelineAnimator(actor, "actor");

            GameObject sourceCameraRig = InstantiateAsset(CameraAssetPath, scene);
            sourceCameraRig.name = "C34_ExactSourceCameraRig_Playable";
            Animator cameraAnimator = EnsureTimelineAnimator(sourceCameraRig, "camera");
            Camera sourceCamera = sourceCameraRig.GetComponentInChildren<Camera>(includeInactive: true)
                ?? throw new InvalidOperationException("Imported C34 camera asset has no Camera component.");
            sourceCamera.gameObject.tag = "MainCamera";
            sourceCamera.enabled = true;
            sourceCamera.clearFlags = CameraClearFlags.SolidColor;
            sourceCamera.backgroundColor = new Color(0.005f, 0.008f, 0.016f, 1f);
            sourceCamera.nearClipPlane = 0.01f;
            sourceCamera.farClipPlane = 500f;
            sourceCamera.allowHDR = true;
            sourceCamera.allowMSAA = true;

            CreateNeutralLighting();
            GameObject directorObject = new GameObject(DirectorName);
            PlayableDirector director = directorObject.AddComponent<PlayableDirector>();
            director.playableAsset = timelineBindings.Timeline;
            director.timeUpdateMode = DirectorUpdateMode.UnscaledGameTime;
            director.extrapolationMode = DirectorWrapMode.None;
            director.playOnAwake = true;
            director.initialTime = 0d;
            director.SetGenericBinding(timelineBindings.ActorTrack, actorAnimator);
            director.SetGenericBinding(timelineBindings.CameraTrack, cameraAnimator);
            director.RebuildGraph();
            director.time = 0d;
            director.Evaluate();

            GameObject marker = new GameObject(
                "EDITOR_ONLY__C34_SOURCE_PLAYABLE__REMOVE_START_OFFSET__NO_TRACK_OFFSETS");
            marker.transform.position = new Vector3(10000f, 10000f, 10000f);

            ValidateScene(
                scene,
                profile,
                timelineBindings,
                cameraClip,
                actorClip,
                actor,
                sourceCamera,
                actorAnimator,
                cameraAnimator,
                director);
            EditorUtility.SetDirty(director);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
            {
                throw new InvalidOperationException($"Failed to save C34 review scene at {ScenePath}.");
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
                "c34_eye_open_current_akaza_review",
                "C34 Eye Opening Push-In - Current Akaza Review",
                CinematicSequenceProfile.SequenceCategory.BossIntro,
                "Verified editor-only proof: exact C34 camera and actor clips, current Akaza model/material binding. Source 90.050 closed, 90.550 slit, 91.300 fully open.",
                DurationSeconds,
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
                    DurationSeconds));
            profile.ConfigureSourceActorAnimation(
                new CinematicSequenceProfile.SourceActorAnimationCue(
                    ActorCueId,
                    actorClip,
                    0f,
                    0f,
                    DurationSeconds));
            profile.ConfigureSourceActorGrades(Array.Empty<CinematicSequenceProfile.SourceActorGradeCue>());
            profile.ConfigureScreenFades(Array.Empty<CinematicSequenceProfile.ScreenFadeCue>());
            profile.ConfigureStageContext(
                null,
                string.Empty,
                string.Empty,
                string.Empty,
                "Editor-only review; deliberately excluded from product stage routing.",
                false);
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static TimelineBindings EnsureTimeline(
            AnimationClip cameraClip,
            AnimationClip actorClip)
        {
            TimelineAsset timeline = AssetDatabase.LoadAssetAtPath<TimelineAsset>(TimelinePath);
            if (timeline == null)
            {
                timeline = ScriptableObject.CreateInstance<TimelineAsset>();
                AssetDatabase.CreateAsset(timeline, TimelinePath);
            }

            foreach (TrackAsset track in timeline.GetRootTracks().ToArray())
            {
                timeline.DeleteTrack(track);
            }

            timeline.durationMode = TimelineAsset.DurationMode.FixedLength;
            timeline.fixedDuration = DurationSeconds;
            AnimationTrack actorTrack = CreateSourceAnimationTrack(
                timeline,
                ActorTrackName,
                actorClip);
            AnimationTrack cameraTrack = CreateSourceAnimationTrack(
                timeline,
                CameraTrackName,
                cameraClip);
            EditorUtility.SetDirty(timeline);
            return new TimelineBindings(timeline, actorTrack, cameraTrack);
        }

        private static AnimationTrack CreateSourceAnimationTrack(
            TimelineAsset timeline,
            string trackName,
            AnimationClip animationClip)
        {
            AnimationTrack track = timeline.CreateTrack<AnimationTrack>(trackName);
            track.trackOffset = TrackOffset.Auto;
            TimelineClip timelineClip = track.CreateClip(animationClip);
            timelineClip.displayName = animationClip.name;
            timelineClip.start = 0d;
            timelineClip.clipIn = 0d;
            timelineClip.duration = DurationSeconds;
            timelineClip.timeScale = 1d;
            timelineClip.easeInDuration = 0d;
            timelineClip.easeOutDuration = 0d;
            SetTimelineClipExtrapolation(timelineClip, TimelineClip.ClipExtrapolation.None);

            if (timelineClip.asset is not AnimationPlayableAsset playableAsset)
            {
                throw new InvalidOperationException(
                    $"C34 Timeline track {trackName} did not create an AnimationPlayableAsset.");
            }

            playableAsset.clip = animationClip;
            playableAsset.position = Vector3.zero;
            playableAsset.rotation = Quaternion.identity;
            playableAsset.removeStartOffset = true;
            playableAsset.applyFootIK = false;
            playableAsset.loop = AnimationPlayableAsset.LoopMode.Off;
            playableAsset.useTrackMatchFields = false;

            SerializedObject serializedTrack = new SerializedObject(track);
            SerializedProperty applyOffsets = serializedTrack.FindProperty("m_ApplyOffsets");
            if (applyOffsets != null)
            {
                applyOffsets.boolValue = false;
                serializedTrack.ApplyModifiedPropertiesWithoutUndo();
            }

            EditorUtility.SetDirty(playableAsset);
            EditorUtility.SetDirty(track);
            return track;
        }

        private static void SetTimelineClipExtrapolation(
            TimelineClip timelineClip,
            TimelineClip.ClipExtrapolation extrapolation)
        {
            const System.Reflection.BindingFlags Flags =
                System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.NonPublic;
            typeof(TimelineClip).GetField("m_PreExtrapolationMode", Flags)?.SetValue(
                timelineClip,
                extrapolation);
            typeof(TimelineClip).GetField("m_PostExtrapolationMode", Flags)?.SetValue(
                timelineClip,
                extrapolation);
        }

        private static Animator EnsureTimelineAnimator(GameObject root, string label)
        {
            Animator rootAnimator = root.GetComponent<Animator>();
            if (rootAnimator == null)
            {
                rootAnimator = root.AddComponent<Animator>();
            }

            Animator[] animators = root.GetComponentsInChildren<Animator>(true);
            foreach (Animator animator in animators)
            {
                animator.runtimeAnimatorController = null;
                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                animator.updateMode = AnimatorUpdateMode.UnscaledTime;
                animator.enabled = true;
            }

            if (!rootAnimator.enabled || rootAnimator.runtimeAnimatorController != null)
            {
                throw new InvalidOperationException(
                    $"C34 {label} Timeline Animator could not be configured controller-free.");
            }

            return rootAnimator;
        }

        private readonly struct TimelineBindings
        {
            public TimelineBindings(
                TimelineAsset timeline,
                AnimationTrack actorTrack,
                AnimationTrack cameraTrack)
            {
                Timeline = timeline;
                ActorTrack = actorTrack;
                CameraTrack = cameraTrack;
            }

            public TimelineAsset Timeline { get; }
            public AnimationTrack ActorTrack { get; }
            public AnimationTrack CameraTrack { get; }
        }

        private static GameObject InstantiateCurrentAkaza(Scene scene)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CurrentAkazaPrefabPath)
                ?? throw new InvalidOperationException($"Current Akaza prefab missing at {CurrentAkazaPrefabPath}.");
            GameObject actor = PrefabUtility.InstantiatePrefab(prefab, scene) as GameObject
                ?? throw new InvalidOperationException("Failed to instantiate current Akaza prefab.");
            actor.name = CurrentActorName;
            actor.transform.SetParent(null, false);
            actor.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            actor.transform.localScale = Vector3.one;
            actor.SetActive(true);

            foreach (MonoBehaviour behaviour in actor.GetComponentsInChildren<MonoBehaviour>(true))
            {
                UnityEngine.Object.DestroyImmediate(behaviour);
            }

            foreach (AudioSource audio in actor.GetComponentsInChildren<AudioSource>(true))
            {
                UnityEngine.Object.DestroyImmediate(audio);
            }

            foreach (ParticleSystem particles in actor.GetComponentsInChildren<ParticleSystem>(true))
            {
                UnityEngine.Object.DestroyImmediate(particles.gameObject);
            }

            foreach (TrailRenderer trail in actor.GetComponentsInChildren<TrailRenderer>(true))
            {
                UnityEngine.Object.DestroyImmediate(trail);
            }

            foreach (LineRenderer line in actor.GetComponentsInChildren<LineRenderer>(true))
            {
                UnityEngine.Object.DestroyImmediate(line);
            }

            Renderer[] renderers = actor.GetComponentsInChildren<Renderer>(true);
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

            PrepareOfflineSkinnedMeshCapture(actor);
            return actor;
        }

        private static int PrepareOfflineSkinnedMeshCapture(GameObject actor)
        {
            SkinnedMeshRenderer[] skinnedMeshes =
                actor.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true);
            if (skinnedMeshes.Length == 0)
            {
                throw new InvalidOperationException(
                    "C34 current-Akaza review actor has no skinned meshes to capture.");
            }

            foreach (SkinnedMeshRenderer skinnedMesh in skinnedMeshes)
            {
                // Unity can otherwise reuse the first sampled deformation when several
                // PlayableDirector.Evaluate + Camera.Render calls happen in one Editor tick.
                skinnedMesh.updateWhenOffscreen = true;
                skinnedMesh.forceMatrixRecalculationPerRender = true;
            }

            return skinnedMeshes.Length;
        }

        private static void CreateNeutralLighting()
        {
            GameObject key = new GameObject("C34_NeutralKeyLight");
            Light keyLight = key.AddComponent<Light>();
            keyLight.type = LightType.Directional;
            keyLight.color = new Color(0.82f, 0.90f, 1f);
            keyLight.intensity = 1.15f;
            key.transform.rotation = Quaternion.Euler(42f, -28f, 0f);

            GameObject rim = new GameObject("C34_NeutralRimLight");
            Light rimLight = rim.AddComponent<Light>();
            rimLight.type = LightType.Directional;
            rimLight.color = new Color(0.36f, 0.55f, 1f);
            rimLight.intensity = 0.55f;
            rim.transform.rotation = Quaternion.Euler(28f, 145f, 0f);
        }

        private static void ValidateScene(
            Scene scene,
            CinematicSequenceProfile profile,
            TimelineBindings timelineBindings,
            AnimationClip cameraClip,
            AnimationClip actorClip,
            GameObject actor,
            Camera sourceCamera,
            Animator actorAnimator,
            Animator cameraAnimator,
            PlayableDirector director)
        {
            ValidateClipLength("camera", cameraClip);
            ValidateClipLength("actor", actorClip);
            if (profile.SourceCameraAnimations.Length != 1 || profile.SourceActorAnimations.Length != 1)
            {
                throw new InvalidOperationException("C34 profile requires exactly one camera and one actor source cue.");
            }

            ValidateSourceActorClipPath(actorClip, "loaded actor clip");
            ValidateSourceActorClipPath(
                profile.SourceActorAnimations[0].Clip,
                "profile source-actor cue");
            if (profile.SourceActorAnimations[0].Clip != actorClip)
            {
                throw new InvalidOperationException(
                    "C34 profile source-actor cue does not reference the loaded Source FBX clip.");
            }

            ValidateTimelineTrack(
                timelineBindings.ActorTrack,
                actorClip,
                ActorAnimationAssetPath,
                "actor");
            ValidateTimelineTrack(
                timelineBindings.CameraTrack,
                cameraClip,
                CameraAssetPath,
                "camera");
            if (director.playableAsset != timelineBindings.Timeline
                || director.GetGenericBinding(timelineBindings.ActorTrack) != actorAnimator
                || director.GetGenericBinding(timelineBindings.CameraTrack) != cameraAnimator)
            {
                throw new InvalidOperationException("C34 PlayableDirector Timeline bindings are incomplete.");
            }

            if (actorAnimator.runtimeAnimatorController != null
                || cameraAnimator.runtimeAnimatorController != null
                || !actorAnimator.enabled
                || !cameraAnimator.enabled)
            {
                throw new InvalidOperationException(
                    "C34 actor/camera Animators must remain enabled with null controllers for Timeline playback.");
            }

            Material[] materials = actor.GetComponentsInChildren<Renderer>(true)
                .SelectMany(renderer => renderer.sharedMaterials)
                .Where(material => material != null)
                .Distinct()
                .ToArray();
            if (materials.Length == 0 || materials.Any(material =>
                    AssetDatabase.GetAssetPath(material).StartsWith(
                        "Assets/_Game/Art/Animations/Cinematics/LegacyCameraGrammar/",
                        StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("Visible C34 Akaza is not using only current project materials.");
            }

            if (!sourceCamera.enabled || !sourceCamera.CompareTag("MainCamera"))
            {
                throw new InvalidOperationException(
                    "C34 exact source camera must be the enabled MainCamera for Timeline playback.");
            }

            if (FindSceneComponents<Camera>(scene).Count(camera => camera.enabled) != 1)
            {
                throw new InvalidOperationException("C34 Timeline review requires exactly one enabled camera.");
            }

            if (FindSceneComponents<CinematicSequenceRunner>(scene).Length != 0)
            {
                throw new InvalidOperationException(
                    "C34 Timeline review must not fall back to stateful AnimationClip.SampleAnimation playback.");
            }

            SkinnedMeshRenderer[] skinnedMeshes =
                actor.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true);
            if (skinnedMeshes.Length == 0 || skinnedMeshes.Any(renderer =>
                    !renderer.updateWhenOffscreen || !renderer.forceMatrixRecalculationPerRender))
            {
                throw new InvalidOperationException(
                    "C34 review requires updateWhenOffscreen and forceMatrixRecalculationPerRender "
                    + "on every skinned mesh for deterministic same-tick offline sampling.");
            }

            if (FindSceneComponents<AudioSource>(scene).Length != 0
                || FindSceneComponents<ParticleSystem>(scene).Length != 0)
            {
                throw new InvalidOperationException("C34 review contains audio or particle VFX.");
            }

            if (EditorBuildSettings.scenes.Any(entry =>
                    string.Equals(entry.path, ScenePath, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("Editor-only C34 review leaked into build settings.");
            }
        }

        private static void ValidateTimelineTrack(
            AnimationTrack track,
            AnimationClip expectedClip,
            string expectedAssetPath,
            string label)
        {
            TimelineClip[] clips = track.GetClips().ToArray();
            if (clips.Length != 1 || clips[0].asset is not AnimationPlayableAsset playableAsset)
            {
                throw new InvalidOperationException(
                    $"C34 {label} Timeline track requires exactly one AnimationPlayableAsset.");
            }

            string actualPath = AssetDatabase.GetAssetPath(playableAsset.clip).Replace('\\', '/');
            if (playableAsset.clip != expectedClip
                || !string.Equals(actualPath, expectedAssetPath, StringComparison.OrdinalIgnoreCase)
                || !playableAsset.removeStartOffset
                || track.trackOffset != TrackOffset.Auto)
            {
                throw new InvalidOperationException(
                    $"C34 {label} Timeline track is not source-faithful: path={actualPath}, "
                    + $"removeStartOffset={playableAsset.removeStartOffset}, trackOffset={track.trackOffset}.");
            }

            SerializedObject serializedTrack = new SerializedObject(track);
            SerializedProperty applyOffsets = serializedTrack.FindProperty("m_ApplyOffsets");
            if (applyOffsets != null && applyOffsets.boolValue)
            {
                throw new InvalidOperationException(
                    $"C34 {label} Timeline track must preserve original ApplyOffsets=false.");
            }
        }

        private static void ValidateSourceActorClipPath(AnimationClip clip, string label)
        {
            if (clip == null)
            {
                throw new InvalidOperationException($"C34 {label} is missing.");
            }

            string path = AssetDatabase.GetAssetPath(clip).Replace('\\', '/');
            if (!string.Equals(path, ActorAnimationAssetPath, StringComparison.OrdinalIgnoreCase)
                || path.IndexOf("/Sanitized/", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                throw new InvalidOperationException(
                    $"C34 {label} must be the unmodified Source FBX clip at "
                    + $"{ActorAnimationAssetPath}; got {path}. The in-place extraction breaks exact camera alignment.");
            }
        }

        private static void CaptureVerification()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            PlayableDirector director = FindSceneComponent<PlayableDirector>(scene)
                ?? throw new InvalidOperationException("C34 source PlayableDirector missing during verification.");
            CinematicSequenceProfile profile =
                AssetDatabase.LoadAssetAtPath<CinematicSequenceProfile>(ProfilePath)
                ?? throw new InvalidOperationException("C34 review profile missing during verification.");
            Camera camera = FindSceneComponents<Camera>(scene).SingleOrDefault(component => component.enabled)
                ?? throw new InvalidOperationException("C34 enabled source camera missing during verification.");
            GameObject actor = scene.GetRootGameObjects()
                .FirstOrDefault(root => string.Equals(root.name, CurrentActorName, StringComparison.Ordinal))
                ?? throw new InvalidOperationException("C34 current-Akaza actor missing during verification.");
            ValidateSourceActorClipPath(profile.SourceActorAnimations[0].Clip, "captured profile cue");
            PrepareOfflineSkinnedMeshCapture(actor);
            director.RebuildGraph();

            Directory.CreateDirectory(OutputRoot);
            ResetOutputDirectory(EvidenceDirectory);
            ResetOutputDirectory(VideoFrameDirectory);
            if (File.Exists(PreviewVideoPath))
            {
                File.Delete(PreviewVideoPath);
            }

            Text overlayLabel = CreateEvidenceOverlay(camera);
            List<Texture2D> verificationFrames = new List<Texture2D>(VerificationTimes.Length);
            List<Texture2D> evidenceFrames = new List<Texture2D>(EvidenceBeats.Length);
            string finalFaceEvidence = string.Empty;
            string irisPixelEvidence = string.Empty;
            try
            {
                for (int i = 0; i < VerificationTimes.Length; i++)
                {
                    float seconds = VerificationTimes[i];
                    ApplyDirectorSample(director, seconds);
                    overlayLabel.text = $"C34 SOURCE REGRESSION  |  LOCAL +{seconds:0.000}s";
                    Canvas.ForceUpdateCanvases();
                    verificationFrames.Add(CaptureCamera(camera, 960, 540));
                }

                finalFaceEvidence = ValidateFinalFaceFrustum(actor, camera, VerificationTimes[^1]);
                irisPixelEvidence = ValidateRenderedEyeSequence(verificationFrames);
                Texture2D verificationSheet = BuildContactSheet(verificationFrames, 480, 270);
                try
                {
                    File.WriteAllBytes(VerificationPath, verificationSheet.EncodeToPNG());
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(verificationSheet);
                }

                for (int i = 0; i < EvidenceBeats.Length; i++)
                {
                    EvidenceBeat beat = EvidenceBeats[i];
                    ApplyDirectorSample(director, beat.LocalSeconds);
                    overlayLabel.text =
                        $"C34 SOURCE  |  SOURCE {beat.SourceSeconds:00.000}s  |  "
                        + $"LOCAL +{beat.LocalSeconds:0.000}s  |  {beat.DisplayLabel}";
                    Canvas.ForceUpdateCanvases();
                    Texture2D frame = CaptureCamera(camera, 1280, 720);
                    evidenceFrames.Add(frame);
                    string filePath = Path.Combine(
                        EvidenceDirectory,
                        $"{i + 1:00}_C34_{beat.FileLabel}_source-{beat.SourceSeconds:00.000}s.png");
                    File.WriteAllBytes(filePath, frame.EncodeToPNG());
                }

                Texture2D contactSheet = BuildContactSheet(evidenceFrames, 320, 180);
                try
                {
                    File.WriteAllBytes(ContactSheetPath, contactSheet.EncodeToPNG());
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(contactSheet);
                }

                Texture2D heroTriptych = BuildContactSheet(evidenceFrames, 480, 270, 1, 3);
                try
                {
                    File.WriteAllBytes(HeroTriptychPath, heroTriptych.EncodeToPNG());
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(heroTriptych);
                }

                int videoFrameCount = CaptureVideoFrames(director, camera, overlayLabel);
                WriteCaptureReport(
                    profile,
                    actor,
                    videoFrameCount,
                    finalFaceEvidence,
                    irisPixelEvidence);
            }
            finally
            {
                for (int i = 0; i < verificationFrames.Count; i++)
                {
                    UnityEngine.Object.DestroyImmediate(verificationFrames[i]);
                }

                for (int i = 0; i < evidenceFrames.Count; i++)
                {
                    UnityEngine.Object.DestroyImmediate(evidenceFrames[i]);
                }
            }
        }

        private static int CaptureVideoFrames(
            PlayableDirector director,
            Camera camera,
            Text overlayLabel)
        {
            int frameCount = Mathf.RoundToInt(DurationSeconds * PreviewFps);
            for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
            {
                float localSeconds = frameIndex / (float)PreviewFps;
                float sourceSeconds = TimelineStartSeconds + localSeconds;
                ApplyDirectorSample(director, localSeconds);
                overlayLabel.text =
                    $"C34 SOURCE 30 FPS  |  SOURCE {sourceSeconds:00.000}s  |  "
                    + $"LOCAL +{localSeconds:0.000}s  |  FRAME {frameIndex:0000}/{frameCount - 1:0000}";
                Canvas.ForceUpdateCanvases();
                Texture2D frame = CaptureCamera(camera, PreviewWidth, PreviewHeight);
                try
                {
                    File.WriteAllBytes(
                        Path.Combine(VideoFrameDirectory, $"frame_{frameIndex:0000}.png"),
                        frame.EncodeToPNG());
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(frame);
                }
            }

            int actualCount = Directory.GetFiles(VideoFrameDirectory, "frame_*.png").Length;
            if (actualCount != frameCount || frameCount != 71)
            {
                throw new InvalidOperationException(
                    $"Expected exactly 71 current-source C34 video frames, found {actualCount}.");
            }

            File.WriteAllLines(Path.Combine(VideoFrameDirectory, "README.txt"), new[]
            {
                $"Frames: {frameCount}",
                $"Frame rate: {PreviewFps} fps",
                $"Exact playback duration: {frameCount / (float)PreviewFps:0.000000} seconds",
                $"Resolution: {PreviewWidth}x{PreviewHeight}",
                "Audio: none",
                $"Actor clip asset: {ActorAnimationAssetPath}",
                $"Source timeline: {TimelineStartSeconds:0.000000}-{TimelineEndSeconds:0.000000}",
                $"Range: frame_0000.png through frame_{frameCount - 1:0000}.png"
            });
            return frameCount;
        }

        private static void ApplyDirectorSample(PlayableDirector director, float localSeconds)
        {
            if (director == null || director.playableAsset == null)
            {
                throw new InvalidOperationException("C34 source PlayableDirector is not configured.");
            }

            director.time = Math.Max(0d, Math.Min(DurationSeconds, localSeconds));
            director.Evaluate();
            Physics.SyncTransforms();
        }

        private static string ValidateFinalFaceFrustum(GameObject actor, Camera camera, float seconds)
        {
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
            Renderer[] renderers = actor.GetComponentsInChildren<Renderer>(true);
            string[] requiredNames =
            {
                "CHakazaA:head",
                "CHakazaA:eyeBall",
                "CHakazaA:eyeline",
                "CHakazaA:eyeHighLight"
            };
            List<string> evidence = new List<string>(requiredNames.Length);
            foreach (string requiredName in requiredNames)
            {
                Renderer renderer = renderers.FirstOrDefault(candidate =>
                    string.Equals(candidate.gameObject.name, requiredName, StringComparison.Ordinal));
                if (renderer == null)
                {
                    throw new InvalidOperationException(
                        $"C34 final-frame regression: renderer {requiredName} is missing.");
                }

                bool inFrustum = GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
                if (!renderer.enabled || !renderer.gameObject.activeInHierarchy || !inFrustum)
                {
                    throw new InvalidOperationException(
                        $"C34 final-frame regression at {seconds:0.000}s: {requiredName} "
                        + $"enabled={renderer.enabled}, active={renderer.gameObject.activeInHierarchy}, "
                        + $"inFrustum={inFrustum}. Verify that the Source FBX clip, not Sanitized/InPlace, is bound.");
                }

                evidence.Add(requiredName + "=enabled/active/inFrustum");
            }

            return string.Join(", ", evidence);
        }

        private static string ValidateRenderedEyeSequence(IReadOnlyList<Texture2D> frames)
        {
            if (frames.Count != VerificationTimes.Length)
            {
                throw new InvalidOperationException(
                    $"C34 rendered-eye regression expected {VerificationTimes.Length} frames; got {frames.Count}.");
            }

            int[] turquoiseCounts = frames.Select(CountTurquoiseIrisPixels).ToArray();
            const int MinimumVisibleIrisPixels = 24;
            if (turquoiseCounts[1] < MinimumVisibleIrisPixels
                || turquoiseCounts[2] < MinimumVisibleIrisPixels
                || turquoiseCounts[3] < MinimumVisibleIrisPixels)
            {
                throw new InvalidOperationException(
                    "C34 rendered-eye regression failed: the slit/open/final frames must contain "
                    + $"at least {MinimumVisibleIrisPixels} turquoise iris pixels. Counts at "
                    + string.Join(", ", VerificationTimes.Select((time, index) =>
                        $"+{time:0.000}s={turquoiseCounts[index]}"))
                    + ". Renderer bounds alone are not accepted because a stale skinning cache can render a blank face.");
            }

            if (turquoiseCounts[1] <= turquoiseCounts[0]
                || turquoiseCounts[2] <= turquoiseCounts[0]
                || turquoiseCounts[3] <= turquoiseCounts[0])
            {
                throw new InvalidOperationException(
                    "C34 rendered-eye regression failed: slit/open/final iris evidence must exceed "
                    + $"the closed-frame baseline ({turquoiseCounts[0]} pixels). Counts: "
                    + string.Join(", ", turquoiseCounts));
            }

            return string.Join(", ", VerificationTimes.Select((time, index) =>
                $"+{time:0.000}s={turquoiseCounts[index]} turquoise pixels"));
        }

        private static int CountTurquoiseIrisPixels(Texture2D frame)
        {
            Color32[] pixels = frame.GetPixels32();
            int count = 0;
            for (int i = 0; i < pixels.Length; i++)
            {
                Color32 pixel = pixels[i];
                if (pixel.r <= 80
                    && pixel.g >= 90
                    && pixel.b >= 90
                    && pixel.g >= pixel.r + 40
                    && pixel.b >= pixel.r + 40)
                {
                    count++;
                }
            }

            return count;
        }

        private static Text CreateEvidenceOverlay(Camera camera)
        {
            GameObject canvasObject = new GameObject("C34_SourceFrameEvidenceOverlay");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 0.05f;
            canvas.sortingOrder = 1000;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;

            GameObject backgroundObject = new GameObject("EvidenceLabelBackground", typeof(RectTransform));
            backgroundObject.transform.SetParent(canvasObject.transform, false);
            Image background = backgroundObject.AddComponent<Image>();
            background.color = new Color(0f, 0f, 0f, 0.72f);
            RectTransform backgroundRect = background.rectTransform;
            backgroundRect.anchorMin = new Vector2(0f, 0f);
            backgroundRect.anchorMax = new Vector2(1f, 0f);
            backgroundRect.pivot = new Vector2(0.5f, 0f);
            backgroundRect.anchoredPosition = Vector2.zero;
            backgroundRect.sizeDelta = new Vector2(0f, 52f);

            GameObject textObject = new GameObject("EvidenceLabel", typeof(RectTransform));
            textObject.transform.SetParent(backgroundObject.transform, false);
            Text label = textObject.AddComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 22;
            label.fontStyle = FontStyle.Bold;
            label.alignment = TextAnchor.MiddleLeft;
            label.color = Color.white;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            RectTransform textRect = label.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(18f, 0f);
            textRect.offsetMax = new Vector2(-18f, 0f);
            return label;
        }

        private static void ResetOutputDirectory(string path)
        {
            string normalized = Path.GetFullPath(path).Replace('\\', '/');
            string expectedRoot = Path.GetFullPath(OutputRoot).Replace('\\', '/').TrimEnd('/') + "/";
            if (!normalized.StartsWith(expectedRoot, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Refusing to reset output outside {OutputRoot}: {path}");
            }

            if (Directory.Exists(normalized))
            {
                Directory.Delete(normalized, true);
            }

            Directory.CreateDirectory(normalized);
        }

        private static void WriteCaptureReport(
            CinematicSequenceProfile profile,
            GameObject actor,
            int videoFrameCount,
            string finalFaceEvidence,
            string irisPixelEvidence)
        {
            AnimationClip actorClip = profile.SourceActorAnimations[0].Clip;
            ValidateSourceActorClipPath(actorClip, "report profile cue");
            Material[] materials = actor.GetComponentsInChildren<Renderer>(true)
                .SelectMany(renderer => renderer.sharedMaterials)
                .Where(material => material != null)
                .Distinct()
                .ToArray();
            StringBuilder report = new StringBuilder();
            report.AppendLine("# TPK C34 eye-opening / current Akaza source-profile proof");
            report.AppendLine();
            report.AppendLine("- Result: PASS");
            report.AppendLine($"- Actor clip asset: `{AssetDatabase.GetAssetPath(actorClip)}`");
            report.AppendLine("- Sanitized/InPlace actor clip accepted: NO (guarded regression)");
            report.AppendLine($"- Source actor clip length: {actorClip.length:0.000000}s");
            report.AppendLine($"- Source timeline: {TimelineStartSeconds:0.000000}-{TimelineEndSeconds:0.000000}");
            report.AppendLine("- Playback: PlayableDirector + AnimationTracks (RemoveStartOffset=true, ApplyOffsets=false)");
            report.AppendLine("- Offline skinning: updateWhenOffscreen=true, forceMatrixRecalculationPerRender=true");
            report.AppendLine($"- 30fps preview frames: {videoFrameCount} ({videoFrameCount / (float)PreviewFps:0.000000}s)");
            report.AppendLine($"- Final face regression at +{VerificationTimes[^1]:0.000}s: PASS");
            report.AppendLine($"- Final face evidence: {finalFaceEvidence}");
            report.AppendLine($"- Rendered turquoise-iris regression: PASS ({irisPixelEvidence})");
            report.AppendLine($"- Visible current-project materials: {materials.Length}");
            report.AppendLine("- Audio: none");
            report.AppendLine($"- Verification: `{VerificationPath}`");
            report.AppendLine($"- Contact sheet: `{ContactSheetPath}`");
            report.AppendLine($"- Hero triptych: `{HeroTriptychPath}`");
            report.AppendLine($"- MP4 target: `{PreviewVideoPath}`");
            report.AppendLine();
            report.AppendLine(
                "The actor and exact camera are evaluated by a reopenable editor-only Timeline using the "
                + "existing Source FBX clips. Source geometry/materials, background, audio, VFX, and the "
                + "completed source Timeline are not instantiated.");
            File.WriteAllText(ReportPath, report.ToString(), Encoding.UTF8);
        }

        private static Texture2D CaptureCamera(Camera camera, int width, int height)
        {
            RenderTexture previousTarget = camera.targetTexture;
            RenderTexture previousActive = RenderTexture.active;
            RenderTexture renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
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

        private static Texture2D BuildContactSheet(
            IReadOnlyList<Texture2D> frames,
            int width,
            int height)
        {
            return BuildContactSheet(frames, width, height, 0, frames.Count);
        }

        private static Texture2D BuildContactSheet(
            IReadOnlyList<Texture2D> frames,
            int width,
            int height,
            int firstIndex,
            int count)
        {
            if (firstIndex < 0 || count <= 0 || firstIndex + count > frames.Count)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(firstIndex),
                    $"Invalid contact-sheet range {firstIndex}+{count} for {frames.Count} frames.");
            }

            Texture2D sheet = new Texture2D(width * count, height, TextureFormat.RGBA32, false);
            sheet.SetPixels(Enumerable.Repeat(Color.black, sheet.width * sheet.height).ToArray());
            for (int i = 0; i < count; i++)
            {
                Color[] pixels = new Color[width * height];
                for (int y = 0; y < height; y++)
                {
                    float v = y / (float)Mathf.Max(1, height - 1);
                    for (int x = 0; x < width; x++)
                    {
                        float u = x / (float)Mathf.Max(1, width - 1);
                        pixels[x + (y * width)] = frames[firstIndex + i].GetPixelBilinear(u, v);
                    }
                }

                sheet.SetPixels(i * width, 0, width, height, pixels);
            }

            sheet.Apply();
            return sheet;
        }

        private static void ValidateClipLength(string label, AnimationClip clip)
        {
            if (Mathf.Abs(clip.length - DurationSeconds) > 0.02f)
            {
                throw new InvalidOperationException(
                    $"C34 {label} clip is {clip.length:0.000000}s; expected {DurationSeconds:0.000000}s.");
            }
        }

        private static AnimationClip LoadPrimaryClip(string path)
        {
            return AssetDatabase.LoadAllAssetsAtPath(path)
                .OfType<AnimationClip>()
                .FirstOrDefault(clip => !clip.name.StartsWith("__preview__", StringComparison.Ordinal));
        }

        private static GameObject InstantiateAsset(string path, Scene scene)
        {
            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path)
                ?? throw new InvalidOperationException($"GameObject asset missing at {path}.");
            GameObject instance = PrefabUtility.InstantiatePrefab(asset, scene) as GameObject
                ?? throw new InvalidOperationException($"Failed to instantiate {path}.");
            instance.transform.SetParent(null, false);
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

        private static void RunBatch(Action action)
        {
            try
            {
                action();
                Debug.Log("C34 eye-open current-Akaza review batch operation passed.");
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
