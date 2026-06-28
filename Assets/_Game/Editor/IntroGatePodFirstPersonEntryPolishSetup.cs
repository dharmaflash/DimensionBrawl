using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DimensionBrawl.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;
using UnityEngine.UI;

namespace DimensionBrawl.Editor
{
    public static class IntroGatePodFirstPersonEntryPolishSetup
    {
        private const string OlympusStageScenePath = "Assets/_Game/Scenes/OlympusCorridorInvasionStage.unity";
        private const string OlympusCombinedTimelinePath =
            "Assets/_Game/DesignData/Timelines/Cinematics/DB_Timeline_IntroGatePodAwakening_OlympusBombingPrelude.playable";
        private const string OverlayObjectName = "IntroGatePodReview_FirstPersonBootOverlay";
        private const string BootTrackName = "First Person Boot HUD Glitch";
        private const string BootClipName = "HUD Boot Static Lines";
        private const string LegacyHandoffShockTrackName = "Bombing Handoff Strong Camera Shock";
        private const string LegacyEntryShockTrackName = "First Person Entry Strong Camera Shock";
        private const string FadeInEntryShockTrackName = "Frame 265 Fade-In Camera Shake";
        private const string FirstPersonShotName = "CM_02_src_c03_first_person_eye_open";
        private const string FadeInEntryShotName = "CM_01_src_c01_capsule_left_dolly";
        private const string FirstPersonClipName = "src_c03_first_person_eye_open";
        private const string InvasionBridgeObjectName = "IntroGatePodReview_InvasionBridge";
        private const string CommandoFootstepAudioSourcePrefix = "IntroGatePodReview_CommandoFootstepTimelineAudio_";
        private const string CommandoFootstepTrackPrefix = "Commando Footsteps ";
        private const string GlitchShaderPath =
            "Assets/_Game/Art/Shaders/Cinematics/IntroGatePod/DB_UI_FirstPersonGlitchOverlay.shader";
        private const string GlitchMaterialPath =
            "Assets/_Game/Art/Materials/Cinematics/IntroGatePod/DB_UI_FirstPersonGlitchOverlay.mat";
        private const string AnimationRoot =
            "Assets/_Game/DesignData/Animations/Cinematics/IntroGatePodBombingReview";
        private const string FadeInEntryShockClipPath =
            AnimationRoot + "/AC_IntroGatePod_Frame265FadeInCameraShake.anim";
        private const string ReportPath = "C:/tmp/DimensionBrawl-IntroGatePodFirstPersonEntryPolish.md";
        private static readonly string[] ArmoredFootstepClipPaths =
        {
            "Assets/_Game/Art/Audio/SFX/Footsteps/DB_SFX_Footstep_ArmoredMedium_01.wav",
            "Assets/_Game/Art/Audio/SFX/Footsteps/DB_SFX_Footstep_ArmoredMedium_02.wav",
            "Assets/_Game/Art/Audio/SFX/Footsteps/DB_SFX_Footstep_ArmoredMedium_03.wav"
        };

        private const double FadeInEntryShockDurationSeconds = 1.48d;
        private const double FirstPersonBootDurationSeconds = 1.12d;
        private const float CommandoHoldUntilFrame = 863f;
        private const float TimelineFrameRate = 30f;
        private const float CommandoHoldBufferSeconds = 0.12f;
        private const float BombDropTempoPullForwardSeconds = 0.30f;
        private const float FootstepEndLeadSeconds = 0.18f;
        private const float FootstepCycleRate = 1.35f;
        private const float FootstepClipDurationSeconds = 0.30f;

        [MenuItem("Tools/DimensionBrawl/Intro GatePod/Setup First Person Entry Polish")]
        public static void SetupFirstPersonEntryPolishMenu()
        {
            SetupFirstPersonEntryPolish(writeReport: true);
        }

        public static void RunBatchSetupFirstPersonEntryPolish()
        {
            SetupFirstPersonEntryPolish(writeReport: true);
        }

        private static void SetupFirstPersonEntryPolish(bool writeReport)
        {
            AssetDatabase.Refresh();
            Scene scene = EditorSceneManager.OpenScene(OlympusStageScenePath, OpenSceneMode.Single);
            TimelineAsset timeline = LoadRequired<TimelineAsset>(OlympusCombinedTimelinePath);
            PlayableDirector director = FindDirectorBoundToTimeline(scene, timeline)
                ?? throw new InvalidOperationException("Could not find the Olympus intro PlayableDirector bound to the combined Timeline.");

            Material glitchMaterial = EnsureGlitchMaterial();
            IntroGatePodFirstPersonBootOverlay overlay = EnsureBootOverlay(scene, glitchMaterial);
            IntroGatePodFirstPersonBootTrack bootTrack = EnsureBootTrack(timeline);
            director.SetGenericBinding(bootTrack, overlay);

            double firstPersonStartSeconds = FindClipStart(timeline, FirstPersonClipName);
            double openingDollyStartSeconds = FindClipStart(timeline, "src_c01_capsule_left_dolly");
            double fadeInEntryShockStartSeconds = openingDollyStartSeconds + (1d / TimelineFrameRate);
            EnsureBootClip(bootTrack, firstPersonStartSeconds);

            RemoveTimelineTrack(timeline, LegacyHandoffShockTrackName, director);
            RemoveTimelineTrack(timeline, LegacyEntryShockTrackName, director);
            RemoveTimelineTrack(timeline, FadeInEntryShockTrackName, director);
            RemoveCommandoFootstepTracks(timeline, director);
            Transform fadeInEntryShot = RequireObjectInScene(scene, FadeInEntryShotName).transform;
            AddCameraShockTrack(
                timeline,
                director,
                FadeInEntryShockTrackName,
                fadeInEntryShot,
                FadeInEntryShockClipPath,
                "AC_IntroGatePod_Frame265FadeInCameraShake",
                fadeInEntryShockStartSeconds,
                FadeInEntryShockDurationSeconds,
                new Vector3(0.20f, 0.145f, 0.065f),
                new Vector3(2.65f, 1.95f, 4.25f));

            int extendedCommandos = ExtendCommandoCues(scene);
            int footstepClipCount = AddCommandoFootstepTracks(scene, timeline, director);

            director.time = 0d;
            director.Evaluate();
            overlay.Clear();
            IntroGatePodInvasionBridgeCue bridge =
                FindObjectInScene(scene, InvasionBridgeObjectName)?.GetComponent<IntroGatePodInvasionBridgeCue>();
            bridge?.Sample(0f);

            EditorUtility.SetDirty(overlay);
            if (bridge != null)
            {
                EditorUtility.SetDirty(bridge);
            }

            EditorUtility.SetDirty(bootTrack);
            EditorUtility.SetDirty(timeline);
            EditorUtility.SetDirty(director);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            List<string> issues = ValidateSetup(scene, timeline, director, firstPersonStartSeconds, fadeInEntryShockStartSeconds);
            if (writeReport)
            {
                WriteReport(issues, firstPersonStartSeconds, fadeInEntryShockStartSeconds, extendedCommandos, footstepClipCount);
            }

            if (issues.Count > 0)
            {
                throw new InvalidOperationException(
                    "Intro GatePod first-person entry polish setup failed:\n" + string.Join("\n", issues));
            }
        }

        private static IntroGatePodFirstPersonBootOverlay EnsureBootOverlay(Scene scene, Material glitchMaterial)
        {
            GameObject overlayObject = FindObjectInScene(scene, OverlayObjectName);
            if (overlayObject == null)
            {
                overlayObject = new GameObject(
                    OverlayObjectName,
                    typeof(RectTransform),
                    typeof(Canvas),
                    typeof(CanvasScaler),
                    typeof(GraphicRaycaster),
                    typeof(CanvasGroup),
                    typeof(IntroGatePodFirstPersonBootOverlay));
                SceneManager.MoveGameObjectToScene(overlayObject, scene);
            }

            RectTransform overlayRect = overlayObject.GetComponent<RectTransform>();
            StretchFullScreen(overlayRect);

            Canvas canvas = overlayObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 842;

            CanvasScaler scaler = overlayObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            GraphicRaycaster raycaster = overlayObject.GetComponent<GraphicRaycaster>();
            raycaster.enabled = false;

            CanvasGroup rootGroup = overlayObject.GetComponent<CanvasGroup>();
            rootGroup.alpha = 0f;
            rootGroup.interactable = false;
            rootGroup.blocksRaycasts = false;

            Image glitchImage = EnsureImage(overlayRect, "GlitchStaticOverlay");
            StretchFullScreen(glitchImage.rectTransform);
            glitchImage.color = Color.white;
            glitchImage.material = glitchMaterial;
            glitchImage.raycastTarget = false;

            RectTransform hudRoot = EnsureChild(overlayRect, "StatusLineRoot");
            StretchFullScreen(hudRoot);
            CanvasGroup hudGroup = hudRoot.GetComponent<CanvasGroup>();
            if (hudGroup == null)
            {
                hudGroup = hudRoot.gameObject.AddComponent<CanvasGroup>();
            }

            hudGroup.alpha = 0f;
            hudGroup.interactable = false;
            hudGroup.blocksRaycasts = false;

            Image leftBar = EnsureImage(hudRoot, "LeftStatusLine");
            ConfigureStatusBar(leftBar.rectTransform, pivotX: 1f, anchorOffsetX: -18f);
            leftBar.color = new Color(0.70f, 0.93f, 1f, 0.92f);
            leftBar.raycastTarget = false;

            Image rightBar = EnsureImage(hudRoot, "RightStatusLine");
            ConfigureStatusBar(rightBar.rectTransform, pivotX: 0f, anchorOffsetX: 18f);
            rightBar.color = new Color(0.70f, 0.93f, 1f, 0.92f);
            rightBar.raycastTarget = false;

            IntroGatePodFirstPersonBootOverlay overlay =
                overlayObject.GetComponent<IntroGatePodFirstPersonBootOverlay>();
            overlay.Configure(
                rootGroup,
                glitchImage,
                glitchMaterial,
                hudGroup,
                leftBar.rectTransform,
                rightBar.rectTransform,
                430f,
                2f);
            overlay.Clear();

            EditorUtility.SetDirty(overlayObject);
            return overlay;
        }

        private static Material EnsureGlitchMaterial()
        {
            EnsureAssetFolder(PathParent(GlitchMaterialPath));
            Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(GlitchShaderPath);
            if (shader == null)
            {
                shader = Shader.Find("DimensionBrawl/UI/FirstPersonGlitchOverlay");
            }

            if (shader == null)
            {
                throw new InvalidOperationException($"Missing glitch overlay shader `{GlitchShaderPath}`.");
            }

            Material material = AssetDatabase.LoadAssetAtPath<Material>(GlitchMaterialPath);
            if (material == null)
            {
                material = new Material(shader)
                {
                    name = Path.GetFileNameWithoutExtension(GlitchMaterialPath)
                };
                AssetDatabase.CreateAsset(material, GlitchMaterialPath);
            }
            else
            {
                material.shader = shader;
            }

            material.SetColor("_Tint", new Color(0.58f, 0.92f, 1f, 1f));
            material.SetFloat("_Alpha", 0f);
            material.SetFloat("_NoiseStrength", 0f);
            material.SetFloat("_ScanlineStrength", 0f);
            material.SetFloat("_JitterStrength", 0f);
            material.SetFloat("_Phase", 0f);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static IntroGatePodFirstPersonBootTrack EnsureBootTrack(TimelineAsset timeline)
        {
            IntroGatePodFirstPersonBootTrack existing = FindTrack<IntroGatePodFirstPersonBootTrack>(timeline, BootTrackName);
            if (existing != null)
            {
                return existing;
            }

            IntroGatePodFirstPersonBootTrack track = timeline.CreateTrack<IntroGatePodFirstPersonBootTrack>(BootTrackName);
            EditorUtility.SetDirty(track);
            return track;
        }

        private static void EnsureBootClip(IntroGatePodFirstPersonBootTrack track, double firstPersonStartSeconds)
        {
            TimelineClip clip = FindClip(track, BootClipName);
            if (clip == null)
            {
                clip = track.CreateClip<IntroGatePodFirstPersonBootClip>();
            }

            clip.displayName = BootClipName;
            clip.start = Math.Max(0d, firstPersonStartSeconds + 0.06d);
            clip.duration = FirstPersonBootDurationSeconds;
            clip.easeInDuration = 0d;
            clip.easeOutDuration = 0d;

            IntroGatePodFirstPersonBootClip asset = clip.asset as IntroGatePodFirstPersonBootClip;
            if (asset != null)
            {
                asset.GlitchFadeInSeconds = 0.04f;
                asset.GlitchHoldSeconds = 0.18f;
                asset.GlitchFadeOutSeconds = 0.58f;
                asset.GlitchMaxAlpha = 0.42f;
                asset.GlitchStrength = 1.05f;
                asset.HudDelaySeconds = 0.11f;
                asset.HudOpenSeconds = 0.22f;
                asset.HudHoldSeconds = 0.35f;
                asset.HudFadeOutSeconds = 0.34f;
                asset.HudMaxAlpha = 0.62f;
                asset.StatusBarMaxWidth = 430f;
                asset.StatusBarThickness = 2f;
                EditorUtility.SetDirty(asset);
            }

            EditorUtility.SetDirty(track);
        }

        private static void AddCameraShockTrack(
            TimelineAsset timeline,
            PlayableDirector director,
            string trackName,
            Transform target,
            string clipPath,
            string clipName,
            double startSeconds,
            double durationSeconds,
            Vector3 positionAmplitude,
            Vector3 eulerAmplitude)
        {
            Animator animator = EnsureTimelineAnimator(target.gameObject);
            AnimationClip clipAsset = CreateCameraShockClip(
                clipPath,
                clipName,
                target.localPosition,
                target.localEulerAngles,
                (float)durationSeconds,
                positionAmplitude,
                eulerAmplitude);

            AnimationTrack track = timeline.CreateTrack<AnimationTrack>(trackName);
            track.trackOffset = TrackOffset.Auto;
            director.SetGenericBinding(track, animator);

            TimelineClip clip = track.CreateClip(clipAsset);
            clip.displayName = clipAsset.name;
            clip.start = startSeconds;
            clip.duration = durationSeconds;
            clip.easeInDuration = 0d;
            clip.easeOutDuration = 0d;
            AnimationPlayableAsset playableAsset = clip.asset as AnimationPlayableAsset;
            if (playableAsset != null)
            {
                playableAsset.removeStartOffset = false;
                playableAsset.applyFootIK = false;
                playableAsset.loop = AnimationPlayableAsset.LoopMode.Off;
                EditorUtility.SetDirty(playableAsset);
            }

            EditorUtility.SetDirty(track);
        }

        private static AnimationClip CreateCameraShockClip(
            string assetPath,
            string clipName,
            Vector3 baseLocalPosition,
            Vector3 baseLocalEuler,
            float durationSeconds,
            Vector3 positionAmplitude,
            Vector3 eulerAmplitude)
        {
            EnsureAssetFolder(PathParent(assetPath));
            AnimationClip existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPath);
            if (existing != null)
            {
                AssetDatabase.DeleteAsset(assetPath);
            }

            AnimationClip clip = new AnimationClip
            {
                name = clipName,
                frameRate = 30f,
                legacy = false,
                wrapMode = WrapMode.ClampForever
            };
            AssetDatabase.CreateAsset(clip, assetPath);

            float d = Mathf.Max(0.08f, durationSeconds);
            SetCurve(clip, "m_LocalPosition.x", ShockCurve(baseLocalPosition.x, positionAmplitude.x, d, 1.00f, -0.70f, 0.44f, -0.25f, 0.10f));
            SetCurve(clip, "m_LocalPosition.y", ShockCurve(baseLocalPosition.y, positionAmplitude.y, d, -0.80f, 1.00f, -0.55f, 0.30f, -0.12f));
            SetCurve(clip, "m_LocalPosition.z", ShockCurve(baseLocalPosition.z, positionAmplitude.z, d, -0.70f, 0.36f, -0.18f, 0.10f, 0.00f));
            SetCurve(clip, "localEulerAnglesRaw.x", ShockCurve(baseLocalEuler.x, eulerAmplitude.x, d, -0.90f, 0.58f, -0.35f, 0.18f, 0.00f));
            SetCurve(clip, "localEulerAnglesRaw.y", ShockCurve(baseLocalEuler.y, eulerAmplitude.y, d, 0.84f, -0.52f, 0.34f, -0.18f, 0.00f));
            SetCurve(clip, "localEulerAnglesRaw.z", ShockCurve(baseLocalEuler.z, eulerAmplitude.z, d, 1.00f, -0.78f, 0.52f, -0.24f, 0.00f));
            EditorUtility.SetDirty(clip);
            return clip;
        }

        private static AnimationCurve ShockCurve(float baseValue, float amplitude, float duration, params float[] multipliers)
        {
            List<Keyframe> frames = new List<Keyframe>
            {
                new Keyframe(0f, baseValue)
            };

            float[] times = { 0.05f, 0.13f, 0.24f, 0.38f, 0.58f };
            for (int i = 0; i < multipliers.Length && i < times.Length; i++)
            {
                frames.Add(new Keyframe(Mathf.Min(duration * times[i] / 0.58f, duration), baseValue + amplitude * multipliers[i]));
            }

            frames.Add(new Keyframe(duration, baseValue));
            AnimationCurve curve = new AnimationCurve(frames.ToArray());
            for (int i = 0; i < curve.length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Auto);
                AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Auto);
            }

            return curve;
        }

        private static int ExtendCommandoCues(Scene scene)
        {
            IntroGatePodInvasionBridgeCue bridge =
                RequireObjectInScene(scene, InvasionBridgeObjectName).GetComponent<IntroGatePodInvasionBridgeCue>();
            if (bridge == null)
            {
                throw new InvalidOperationException($"`{InvasionBridgeObjectName}` is missing IntroGatePodInvasionBridgeCue.");
            }

            float holdUntilSeconds = ResolveCommandoHoldUntilSeconds();
            SerializedObject serialized = new SerializedObject(bridge);
            SerializedProperty commandos = serialized.FindProperty("commandos");
            if (commandos == null)
            {
                throw new InvalidOperationException("IntroGatePodInvasionBridgeCue is missing serialized commandos.");
            }

            int extended = 0;
            for (int i = 0; i < commandos.arraySize; i++)
            {
                SerializedProperty cue = commandos.GetArrayElementAtIndex(i);
                SerializedProperty start = cue.FindPropertyRelative("startSeconds");
                SerializedProperty attackStart = cue.FindPropertyRelative("attackStartSeconds");
                SerializedProperty hitStart = cue.FindPropertyRelative("hitStartSeconds");
                SerializedProperty end = cue.FindPropertyRelative("endSeconds");
                SerializedProperty attackState = cue.FindPropertyRelative("attackStateName");
                SerializedProperty hitState = cue.FindPropertyRelative("hitStateName");
                SerializedProperty startPosition = cue.FindPropertyRelative("startLocalPosition");
                SerializedProperty endPosition = cue.FindPropertyRelative("endLocalPosition");
                if (start == null || attackStart == null || hitStart == null || end == null)
                {
                    continue;
                }

                float oldEnd = end.floatValue;
                float targetEnd = holdUntilSeconds + (i * 0.04f);
                bool needsEndExtension = oldEnd < targetEnd;
                bool needsMovementExtension = string.IsNullOrWhiteSpace(attackState?.stringValue)
                    && attackStart.floatValue > start.floatValue + 0.001f
                    && attackStart.floatValue < Mathf.Max(oldEnd, targetEnd) - 0.001f
                    && startPosition != null
                    && endPosition != null;
                if (needsEndExtension || needsMovementExtension)
                {
                    float resolvedTargetEnd = Mathf.Max(oldEnd, targetEnd);
                    if (needsMovementExtension)
                    {
                        endPosition.vector3Value = ExtrapolateCommandoEndLocalPosition(
                            startPosition.vector3Value,
                            endPosition.vector3Value,
                            start.floatValue,
                            attackStart.floatValue,
                            resolvedTargetEnd);
                        attackStart.floatValue = resolvedTargetEnd;
                    }

                    if (string.IsNullOrWhiteSpace(hitState?.stringValue))
                    {
                        hitStart.floatValue = resolvedTargetEnd;
                    }

                    end.floatValue = resolvedTargetEnd;
                    extended++;
                }
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            bridge.Sample(0f);
            EditorUtility.SetDirty(bridge);
            return extended;
        }

        private static int AddCommandoFootstepTracks(
            Scene scene,
            TimelineAsset timeline,
            PlayableDirector director)
        {
            IntroGatePodInvasionBridgeCue bridge =
                RequireObjectInScene(scene, InvasionBridgeObjectName).GetComponent<IntroGatePodInvasionBridgeCue>();
            if (bridge == null)
            {
                throw new InvalidOperationException($"`{InvasionBridgeObjectName}` is missing IntroGatePodInvasionBridgeCue.");
            }

            AudioClip[] clips = LoadFootstepClips();
            int clipCount = 0;
            IntroGatePodInvasionBridgeCue.CommandoCue[] commandos = bridge.Commandos;
            for (int i = 0; i < commandos.Length; i++)
            {
                AudioSource source = EnsureFootstepAudioSource(scene, i);
                AudioTrack track = timeline.CreateTrack<AudioTrack>($"{CommandoFootstepTrackPrefix}{i + 1:00}");
                director.SetGenericBinding(track, source);
                clipCount += AddFootstepClipsForCommando(track, commandos[i], clips, i);
                EditorUtility.SetDirty(track);
            }

            return clipCount;
        }

        private static int AddFootstepClipsForCommando(
            AudioTrack track,
            IntroGatePodInvasionBridgeCue.CommandoCue cue,
            AudioClip[] clips,
            int commandoIndex)
        {
            float startSeconds = cue.StartSeconds + 0.08f;
            float endSeconds = Mathf.Max(startSeconds, cue.EndSeconds - FootstepEndLeadSeconds);
            float offset = cue.NormalizedTimeOffset;
            float cycle = 0f;
            int clipCount = 0;
            while (cycle < 64f)
            {
                float[] contacts = { cycle, cycle + 0.5f };
                for (int i = 0; i < contacts.Length; i++)
                {
                    float time = cue.StartSeconds + ((contacts[i] - offset) / FootstepCycleRate);
                    if (time < startSeconds - 0.0001f || time > endSeconds + 0.0001f)
                    {
                        continue;
                    }

                    AudioClip audioClip = clips[(clipCount + commandoIndex) % clips.Length];
                    TimelineClip timelineClip = track.CreateClip(audioClip);
                    timelineClip.displayName = $"Step {commandoIndex + 1:00}-{clipCount + 1:00}";
                    timelineClip.start = time;
                    timelineClip.duration = Math.Min(FootstepClipDurationSeconds, audioClip.length);
                    AudioPlayableAsset asset = timelineClip.asset as AudioPlayableAsset;
                    if (asset != null)
                    {
                        asset.loop = false;
                        EditorUtility.SetDirty(asset);
                    }

                    clipCount++;
                }

                if (cue.StartSeconds + ((cycle - offset) / FootstepCycleRate) > endSeconds + 1f)
                {
                    break;
                }

                cycle += 1f;
            }

            return clipCount;
        }

        private static AudioSource EnsureFootstepAudioSource(Scene scene, int commandoIndex)
        {
            string objectName = $"{CommandoFootstepAudioSourcePrefix}{commandoIndex + 1:00}";
            GameObject sourceObject = FindObjectInScene(scene, objectName);
            if (sourceObject == null)
            {
                sourceObject = new GameObject(objectName);
                SceneManager.MoveGameObjectToScene(sourceObject, scene);
            }

            AudioSource source = sourceObject.GetComponent<AudioSource>();
            if (source == null)
            {
                source = sourceObject.AddComponent<AudioSource>();
            }

            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0f;
            source.volume = 0.30f;
            source.priority = 118;
            source.panStereo = Mathf.Clamp((commandoIndex - 1) * 0.16f, -0.24f, 0.24f);
            source.pitch = 1f;
            EditorUtility.SetDirty(sourceObject);
            EditorUtility.SetDirty(source);
            return source;
        }

        private static AudioClip[] LoadFootstepClips()
        {
            AudioClip[] clips = new AudioClip[ArmoredFootstepClipPaths.Length];
            for (int i = 0; i < ArmoredFootstepClipPaths.Length; i++)
            {
                clips[i] = LoadRequired<AudioClip>(ArmoredFootstepClipPaths[i]);
            }

            return clips;
        }

        private static List<string> ValidateSetup(
            Scene scene,
            TimelineAsset timeline,
            PlayableDirector director,
            double firstPersonStartSeconds,
            double fadeInEntryShockStartSeconds)
        {
            List<string> issues = new List<string>();
            IntroGatePodFirstPersonBootOverlay overlay =
                FindObjectInScene(scene, OverlayObjectName)?.GetComponent<IntroGatePodFirstPersonBootOverlay>();
            if (overlay == null)
            {
                issues.Add("Missing first-person boot overlay in the Olympus scene.");
            }
            else if (!overlay.HasBindings)
            {
                issues.Add("First-person boot overlay is missing UI or material bindings.");
            }

            IntroGatePodFirstPersonBootTrack bootTrack = FindTrack<IntroGatePodFirstPersonBootTrack>(timeline, BootTrackName);
            if (bootTrack == null)
            {
                issues.Add($"Combined Timeline is missing `{BootTrackName}`.");
            }
            else
            {
                TimelineClip bootClip = FindClip(bootTrack, BootClipName);
                if (bootClip == null)
                {
                    issues.Add($"`{BootTrackName}` is missing `{BootClipName}`.");
                }
                else if (Math.Abs(bootClip.start - (firstPersonStartSeconds + 0.06d)) > 0.01d)
                {
                    issues.Add("First-person boot HUD clip is not aligned to the first-person camera start.");
                }

                if (overlay != null && director.GetGenericBinding(bootTrack) != overlay)
                {
                    issues.Add("First-person boot HUD track is not bound to the scene overlay.");
                }
            }

            AnimationTrack fadeInEntryTrack = RequireTrack<AnimationTrack>(timeline, FadeInEntryShockTrackName, issues);
            if (fadeInEntryTrack != null && director.GetGenericBinding(fadeInEntryTrack) == null)
            {
                issues.Add($"`{FadeInEntryShockTrackName}` is missing a camera Animator binding.");
            }
            else if (fadeInEntryTrack != null)
            {
                TimelineClip shockClip = FindClip(fadeInEntryTrack, "AC_IntroGatePod_Frame265FadeInCameraShake");
                if (shockClip == null)
                {
                    issues.Add($"`{FadeInEntryShockTrackName}` is missing the frame-265 shake clip.");
                }
                else if (Math.Abs(shockClip.start - fadeInEntryShockStartSeconds) > 0.01d)
                {
                    issues.Add("Frame-265 fade-in shake is not aligned to the shifted opening dolly.");
                }
            }

            for (int i = 0; i < 3; i++)
            {
                string trackName = $"{CommandoFootstepTrackPrefix}{i + 1:00}";
                AudioTrack footstepTrack = FindTrack<AudioTrack>(timeline, trackName);
                if (footstepTrack == null)
                {
                    issues.Add($"Combined Timeline is missing `{trackName}`.");
                }
                else if (director.GetGenericBinding(footstepTrack) == null)
                {
                    issues.Add($"`{trackName}` is missing an AudioSource binding.");
                }
            }

            IntroGatePodInvasionBridgeCue bridge =
                FindObjectInScene(scene, InvasionBridgeObjectName)?.GetComponent<IntroGatePodInvasionBridgeCue>();
            if (bridge == null)
            {
                issues.Add("Missing invasion bridge cue.");
            }
            else
            {
                float holdUntilSeconds = ResolveCommandoHoldUntilSeconds();
                IntroGatePodInvasionBridgeCue.CommandoCue[] commandos = bridge.Commandos;
                for (int i = 0; i < commandos.Length; i++)
                {
                    if (commandos[i].EndSeconds < holdUntilSeconds - 0.001f)
                    {
                        issues.Add($"Commando {i + 1:00} ends before frame {CommandoHoldUntilFrame:0}.");
                    }
                }
            }

            if (AssetDatabase.LoadAssetAtPath<Material>(GlitchMaterialPath) == null)
            {
                issues.Add("Missing first-person glitch overlay material.");
            }

            return issues;
        }

        private static void WriteReport(
            IReadOnlyCollection<string> issues,
            double firstPersonStartSeconds,
            double fadeInEntryShockStartSeconds,
            int extendedCommandos,
            int footstepClipCount)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# Intro GatePod First-Person Entry Polish");
            builder.AppendLine();
            builder.AppendLine(issues.Count == 0 ? "Status: PASS" : "Status: FAIL");
            builder.AppendLine();
            builder.AppendLine($"- Scene: `{OlympusStageScenePath}`");
            builder.AppendLine($"- Timeline: `{OlympusCombinedTimelinePath}`");
            builder.AppendLine($"- Frame 265 fade-in shake: `{fadeInEntryShockStartSeconds:0.000}s` for `{FadeInEntryShockDurationSeconds:0.000}s`");
            builder.AppendLine($"- First-person camera start: `{firstPersonStartSeconds:0.000}s`");
            builder.AppendLine($"- HUD/static boot clip: `{firstPersonStartSeconds + 0.06d:0.000}s` for `{FirstPersonBootDurationSeconds:0.000}s`");
            builder.AppendLine($"- Commando hold target: frame `{CommandoHoldUntilFrame:0}` at `{ResolveCommandoHoldUntilSeconds():0.000}s`");
            builder.AppendLine($"- Commando cues extended this run: `{extendedCommandos}`");
            builder.AppendLine($"- Commando footstep Timeline clips: `{footstepClipCount}`");
            builder.AppendLine($"- Overlay object: `{OverlayObjectName}`");
            builder.AppendLine($"- Glitch material: `{GlitchMaterialPath}`");
            if (issues.Count > 0)
            {
                builder.AppendLine();
                builder.AppendLine("## Issues");
                foreach (string issue in issues)
                {
                    builder.AppendLine("- " + issue);
                }
            }

            File.WriteAllText(ReportPath, builder.ToString(), Encoding.UTF8);
        }

        private static float ResolveCommandoHoldUntilSeconds()
        {
            return (CommandoHoldUntilFrame / TimelineFrameRate) + CommandoHoldBufferSeconds - BombDropTempoPullForwardSeconds;
        }

        private static Vector3 ExtrapolateCommandoEndLocalPosition(
            Vector3 startLocalPosition,
            Vector3 authoredEndLocalPosition,
            float startSeconds,
            float authoredEndSeconds,
            float extendedEndSeconds)
        {
            float authoredDuration = Mathf.Max(0.01f, authoredEndSeconds - startSeconds);
            float extendedDuration = Mathf.Max(authoredDuration, extendedEndSeconds - startSeconds);
            return startLocalPosition + ((authoredEndLocalPosition - startLocalPosition) * (extendedDuration / authoredDuration));
        }

        private static void SetCurve(AnimationClip clip, string propertyName, AnimationCurve curve)
        {
            clip.SetCurve(string.Empty, typeof(Transform), propertyName, curve);
        }

        private static Animator EnsureTimelineAnimator(GameObject target)
        {
            Animator animator = target.GetComponent<Animator>();
            if (animator == null)
            {
                animator = target.AddComponent<Animator>();
            }

            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            EditorUtility.SetDirty(animator);
            return animator;
        }

        private static void ConfigureStatusBar(RectTransform rect, float pivotX, float anchorOffsetX)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(pivotX, 0.5f);
            rect.anchoredPosition = new Vector2(anchorOffsetX, -22f);
            rect.sizeDelta = new Vector2(0f, 2f);
            rect.localScale = Vector3.one;
        }

        private static Image EnsureImage(RectTransform parent, string name)
        {
            RectTransform rect = EnsureChild(parent, name);
            Image image = rect.GetComponent<Image>();
            if (image == null)
            {
                image = rect.gameObject.AddComponent<Image>();
            }

            return image;
        }

        private static RectTransform EnsureChild(RectTransform parent, string name)
        {
            Transform existing = parent.Find(name);
            if (existing != null)
            {
                RectTransform existingRect = existing as RectTransform;
                if (existingRect != null)
                {
                    return existingRect;
                }
            }

            GameObject child = new GameObject(name, typeof(RectTransform));
            child.transform.SetParent(parent, worldPositionStays: false);
            return child.GetComponent<RectTransform>();
        }

        private static void StretchFullScreen(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
            rect.localScale = Vector3.one;
        }

        private static bool RemoveTimelineTrack(TimelineAsset timeline, string trackName, PlayableDirector director)
        {
            List<TrackAsset> matches = new List<TrackAsset>();
            foreach (TrackAsset track in timeline.GetOutputTracks())
            {
                if (track != null && string.Equals(track.name, trackName, StringComparison.Ordinal))
                {
                    matches.Add(track);
                }
            }

            for (int i = 0; i < matches.Count; i++)
            {
                director.ClearGenericBinding(matches[i]);
                timeline.DeleteTrack(matches[i]);
            }

            if (matches.Count > 0)
            {
                EditorUtility.SetDirty(timeline);
            }

            return matches.Count > 0;
        }

        private static void RemoveCommandoFootstepTracks(TimelineAsset timeline, PlayableDirector director)
        {
            for (int i = 0; i < 3; i++)
            {
                RemoveTimelineTrack(timeline, $"{CommandoFootstepTrackPrefix}{i + 1:00}", director);
            }
        }

        private static double FindClipStart(TimelineAsset timeline, string displayName)
        {
            foreach (TrackAsset track in timeline.GetOutputTracks())
            {
                if (track == null)
                {
                    continue;
                }

                foreach (TimelineClip clip in track.GetClips())
                {
                    if (string.Equals(clip.displayName, displayName, StringComparison.Ordinal))
                    {
                        return clip.start;
                    }
                }
            }

            throw new InvalidOperationException($"Timeline is missing clip `{displayName}`.");
        }

        private static TimelineClip FindClip(TrackAsset track, string displayName)
        {
            foreach (TimelineClip clip in track.GetClips())
            {
                if (string.Equals(clip.displayName, displayName, StringComparison.Ordinal))
                {
                    return clip;
                }
            }

            return null;
        }

        private static T FindTrack<T>(TimelineAsset timeline, string trackName)
            where T : TrackAsset
        {
            foreach (TrackAsset track in timeline.GetOutputTracks())
            {
                if (track is T typed && string.Equals(track.name, trackName, StringComparison.Ordinal))
                {
                    return typed;
                }
            }

            return null;
        }

        private static T RequireTrack<T>(TimelineAsset timeline, string trackName, List<string> issues)
            where T : TrackAsset
        {
            T track = FindTrack<T>(timeline, trackName);
            if (track == null)
            {
                issues.Add($"Combined Timeline is missing `{trackName}`.");
            }

            return track;
        }

        private static GameObject RequireObjectInScene(Scene scene, string objectName)
        {
            GameObject gameObject = FindObjectInScene(scene, objectName);
            if (gameObject == null)
            {
                throw new InvalidOperationException($"Missing `{objectName}` in `{scene.path}`.");
            }

            return gameObject;
        }

        private static GameObject FindObjectInScene(Scene scene, string objectName)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                Transform found = FindDescendantOrSelf(roots[i].transform, objectName);
                if (found != null)
                {
                    return found.gameObject;
                }
            }

            return null;
        }

        private static Transform FindDescendantOrSelf(Transform root, string objectName)
        {
            if (string.Equals(root.name, objectName, StringComparison.Ordinal))
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindDescendantOrSelf(root.GetChild(i), objectName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static PlayableDirector FindDirectorBoundToTimeline(Scene scene, TimelineAsset timeline)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                PlayableDirector[] directors = roots[i].GetComponentsInChildren<PlayableDirector>(includeInactive: true);
                for (int j = 0; j < directors.Length; j++)
                {
                    if (directors[j].playableAsset == timeline)
                    {
                        return directors[j];
                    }
                }
            }

            return null;
        }

        private static T LoadRequired<T>(string assetPath)
            where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset == null)
            {
                throw new InvalidOperationException($"Missing asset `{assetPath}`.");
            }

            return asset;
        }

        private static void EnsureAssetFolder(string assetFolder)
        {
            assetFolder = assetFolder.Replace('\\', '/');
            if (string.IsNullOrWhiteSpace(assetFolder) || string.Equals(assetFolder, "Assets", StringComparison.Ordinal))
            {
                return;
            }

            if (AssetDatabase.IsValidFolder(assetFolder))
            {
                return;
            }

            string parent = PathParent(assetFolder);
            EnsureAssetFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(assetFolder));
        }

        private static string PathParent(string assetPath)
        {
            return Path.GetDirectoryName(assetPath)?.Replace('\\', '/') ?? "Assets";
        }
    }
}
