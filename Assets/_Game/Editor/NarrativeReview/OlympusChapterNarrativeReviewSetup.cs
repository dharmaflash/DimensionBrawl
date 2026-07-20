using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Presentation;
using DimensionBrawl.Presentation.Narrative;
using DimensionBrawl.UI;
using DimensionBrawl.UI.NarrativeReview;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;
using UnityEngine.UI;

namespace DimensionBrawl.Editor.NarrativeReview
{
    /// <summary>
    /// Builds a self-contained, review-only chapter entry sample. The generated scene intentionally
    /// stays outside build settings and consumes canonical stage data without mutating StageRun.
    /// </summary>
    public static class OlympusChapterNarrativeReviewSetup
    {
        public const string ScenePath =
            "Assets/_Game/Scenes/Review/UI_OlympusChapterNarrativeReview.unity";
        public const string NarrativeProfilePath =
            "Assets/_Game/DesignData/Narrative/Review/DB_Narrative_OlympusChapterEntryReview.asset";
        public const string TimelinePath =
            "Assets/_Game/DesignData/Timelines/Review/DB_Timeline_OlympusTutorialReview.playable";

        private const string CameraAnimationPath =
            "Assets/_Game/DesignData/Timelines/Review/DB_Anim_OlympusTutorialReviewCamera.anim";
        private const string StageCatalogPath = "Assets/_Game/DesignData/UI/DB_UIStageCatalog.asset";
        private const string MediumFontPath =
            "Assets/_Game/Art/Fonts/Pretendard/TMP_Pretendard_Medium_Dynamic.asset";
        private const string SemiBoldFontPath =
            "Assets/_Game/Art/Fonts/Pretendard/TMP_Pretendard_SemiBold_Dynamic.asset";
        private const string BackgroundArtPath =
            "Assets/_Game/UI/NarrativeReview/Art/BG_OlympusGateInterior_Review.png";
        public const string SignalWarningVoicePath =
            "Assets/_Game/Art/Audio/Voice/NarrativeReview/VO_Operator_SignalWarning_ko_TEMP.mp3";
        private const string MaterialRoot = "Assets/_Game/Art/Materials/Review/Narrative";

        private const string CameraTrackName = "Review Camera Push-In";
        private const string DialogueTrackName = "Review Gate Dialogue";
        private const string SequenceId = "review.olympus.prologue.gate_signal";
        private const double TimelineDurationSeconds = 8d;

        private static readonly string[] CanonicalAssetsThatMustRemainUntouched =
        {
            "Assets/_Game/Scenes/OlympusCorridorInvasionStage.unity",
            "Assets/_Game/DesignData/Timelines/Cinematics/DB_Timeline_IntroGatePodAwakening.playable",
            "Assets/_Game/DesignData/Timelines/Cinematics/DB_Timeline_IntroGatePodBombingReview.playable",
            "Assets/_Game/DesignData/Timelines/Cinematics/DB_Timeline_IntroGatePodAwakening_OlympusBombingPrelude.playable"
        };

        private static readonly Color Ink = new Color(0.018f, 0.027f, 0.055f, 0.98f);
        private static readonly Color InkSoft = new Color(0.035f, 0.055f, 0.10f, 0.94f);
        private static readonly Color Panel = new Color(0.055f, 0.075f, 0.13f, 0.94f);
        private static readonly Color PanelSoft = new Color(0.08f, 0.11f, 0.18f, 0.86f);
        private static readonly Color Cyan = new Color(0.20f, 0.88f, 1.00f, 1f);
        private static readonly Color CyanSoft = new Color(0.34f, 0.68f, 0.86f, 1f);
        private static readonly Color Gold = new Color(1.00f, 0.77f, 0.29f, 1f);
        private static readonly Color White = new Color(0.93f, 0.97f, 1.00f, 1f);
        private static readonly Color Muted = new Color(0.58f, 0.67f, 0.78f, 1f);

        [MenuItem("Tools/DimensionBrawl/Review/Setup Olympus Chapter Narrative Review")]
        public static void SetupMenu()
        {
            Setup();
        }

        [MenuItem("Tools/DimensionBrawl/Review/Validate Olympus Chapter Narrative Review")]
        public static void ValidateMenu()
        {
            RunBatchVerification();
        }

        public static void RunBatchSetup()
        {
            Setup();
        }

        public static void RunBatchVerification()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            List<string> issues = ValidateGeneratedReview(scene);
            if (issues.Count > 0)
            {
                throw new InvalidOperationException(
                    "Olympus chapter narrative review validation failed:\n- "
                    + string.Join("\n- ", issues));
            }

            Debug.Log(
                "Olympus chapter narrative review validation passed. "
                + "The scene remains review-only and outside build settings.");
        }

        private static void Setup()
        {
            Dictionary<string, DateTime?> canonicalTimestamps = CaptureCanonicalTimestamps();

            EnsureAssetFolder(PathParent(ScenePath));
            EnsureAssetFolder(PathParent(NarrativeProfilePath));
            EnsureAssetFolder(PathParent(TimelinePath));
            EnsureAssetFolder(MaterialRoot);

            NarrativeSequenceProfile narrativeProfile = EnsureNarrativeProfile();
            UIStageCatalog stageCatalog = LoadRequired<UIStageCatalog>(StageCatalogPath);
            TMP_FontAsset mediumFont = LoadRequired<TMP_FontAsset>(MediumFontPath);
            TMP_FontAsset semiBoldFont = LoadRequired<TMP_FontAsset>(SemiBoldFontPath);
            Sprite reviewBackground = EnsureReviewBackgroundSprite();
            ReviewMaterials materials = EnsureMaterials();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            ConfigureSceneEnvironment();

            GameObject sceneRoot = new GameObject("OlympusChapterNarrativeReview_Root");
            SceneManager.MoveGameObjectToScene(sceneRoot, scene);

            DioramaRefs diorama = CreateDiorama(sceneRoot.transform, materials);
            ReviewUiRefs ui = CreateReviewUi(
                sceneRoot.transform,
                mediumFont,
                semiBoldFont,
                reviewBackground);
            EnsureEventSystem(sceneRoot.transform);

            TimelineAsset timeline = EnsureTimelineAsset();
            AnimationClip cameraAnimation = EnsureCameraAnimation();
            TimelineRefs timelineRefs = ConfigureTimeline(timeline, cameraAnimation);

            GameObject flowObject = new GameObject(
                "NarrativeReviewFlow",
                typeof(PlayableDirector),
                typeof(StageCutscenePort),
                typeof(AudioSource),
                typeof(OlympusChapterNarrativeReviewController),
                typeof(ReviewTutorialStartProbe),
                typeof(ReviewGameplayInputProbe),
                typeof(OlympusStoryTutorialTransitionReviewGate));
            flowObject.transform.SetParent(sceneRoot.transform, false);

            PlayableDirector director = flowObject.GetComponent<PlayableDirector>();
            director.playableAsset = timeline;
            director.playOnAwake = false;
            director.extrapolationMode = DirectorWrapMode.None;
            director.timeUpdateMode = DirectorUpdateMode.GameTime;
            director.SetGenericBinding(timelineRefs.CameraTrack, diorama.CameraAnimator);
            director.SetGenericBinding(timelineRefs.DialogueTrack, ui.CutsceneDialogueOverlay);

            StageCutscenePort cutscenePort = flowObject.GetComponent<StageCutscenePort>();
            cutscenePort.Configure(
                "review.olympus.prologue.gate_signal.cutscene",
                StageCutscenePortKind.Intro,
                "review.olympus.prologue.to_briefing",
                "review.olympus.gate.diorama",
                "review.narrative.tutorial_cutscene",
                diorama.PayloadRoot,
                "Review-only Gate Pod tutorial handoff. Never mutates StageRun or canonical Olympus scenes.");
            cutscenePort.ConfigurePresentationBinding(null, director);

            AudioSource voiceSource = flowObject.GetComponent<AudioSource>();
            voiceSource.playOnAwake = false;
            voiceSource.loop = false;
            voiceSource.spatialBlend = 0f;

            OlympusChapterNarrativeReviewController controller =
                flowObject.GetComponent<OlympusChapterNarrativeReviewController>();
            ReviewTutorialStartProbe tutorialStartProbe =
                flowObject.GetComponent<ReviewTutorialStartProbe>();
            ReviewGameplayInputProbe gameplayInputProbe =
                flowObject.GetComponent<ReviewGameplayInputProbe>();
            OlympusStoryTutorialTransitionReviewGate transitionGate =
                flowObject.GetComponent<OlympusStoryTutorialTransitionReviewGate>();
            transitionGate.Configure(
                diorama.Camera,
                diorama.NarrativeCamera,
                ui.GameplayHudProbeGroup,
                gameplayInputProbe,
                diorama.Listener,
                diorama.NarrativeListener,
                tutorialStartProbe);
            ConfigureController(
                controller,
                narrativeProfile,
                stageCatalog,
                director,
                cutscenePort,
                voiceSource,
                ui,
                transitionGate);

            SetInitialVisibility(ui);
            EditorUtility.SetDirty(controller);
            EditorUtility.SetDirty(transitionGate);
            EditorUtility.SetDirty(tutorialStartProbe);
            EditorUtility.SetDirty(gameplayInputProbe);
            EditorUtility.SetDirty(cutscenePort);
            EditorUtility.SetDirty(director);
            EditorUtility.SetDirty(sceneRoot);

            if (!EditorSceneManager.SaveScene(scene, ScenePath))
            {
                throw new InvalidOperationException($"Failed to save review scene `{ScenePath}`.");
            }

            AssetDatabase.SaveAssetIfDirty(narrativeProfile);
            AssetDatabase.SaveAssetIfDirty(cameraAnimation);
            AssetDatabase.SaveAssetIfDirty(timeline);
            materials.SaveIfDirty();

            List<string> issues = ValidateGeneratedReview(scene);
            AppendCanonicalTimestampIssues(canonicalTimestamps, issues);
            if (issues.Count > 0)
            {
                throw new InvalidOperationException(
                    "Olympus chapter narrative review setup failed:\n- "
                    + string.Join("\n- ", issues));
            }

            Debug.Log(
                $"Created independent narrative review scene `{ScenePath}` with 8 staging lines, "
                + "a two-choice rejoin, a bound tutorial Timeline, and canonical briefing projection.");
        }

        private static NarrativeSequenceProfile EnsureNarrativeProfile()
        {
            AudioClip signalWarningVoice = LoadRequired<AudioClip>(SignalWarningVoicePath);
            NarrativeSequenceProfile profile =
                AssetDatabase.LoadAssetAtPath<NarrativeSequenceProfile>(NarrativeProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<NarrativeSequenceProfile>();
                profile.name = "DB_Narrative_OlympusChapterEntryReview";
                AssetDatabase.CreateAsset(profile, NarrativeProfilePath);
            }

            NarrativeSequenceProfile.ChoiceEntry[] entryChoices =
            {
                new NarrativeSequenceProfile.ChoiceEntry(
                    "review.olympus.prologue.choice.enter_now",
                    "review.olympus.prologue.choice.enter_now.text",
                    "즉시 진입한다",
                    "review.olympus.prologue.choice.enter_now.response",
                    "review.olympus.prologue.choice.enter_now.response.text",
                    "알겠습니다. 즉시 개방 절차를 승인합니다."),
                new NarrativeSequenceProfile.ChoiceEntry(
                    "review.olympus.prologue.choice.scan_again",
                    "review.olympus.prologue.choice.scan_again.text",
                    "상황을 한 번 더 확인한다",
                    "review.olympus.prologue.choice.scan_again.response",
                    "review.olympus.prologue.choice.scan_again.response.text",
                    "추가 스캔 완료. 위험도는 변함없습니다. 개방 절차를 승인합니다.")
            };

            NarrativeSequenceProfile.LineEntry[] lines =
            {
                Line(
                    1,
                    "올림포스 게이트 신호를 포착했습니다. 생체 반응을 확인합니다.",
                    "system",
                    NarrativePortraitSlot.None,
                    "signal"),
                Line(
                    2,
                    "여기는… 게이트 포드 내부. 통신 상태를 확인해 줘.",
                    "field_agent",
                    NarrativePortraitSlot.Center,
                    "neutral"),
                Line(
                    3,
                    "통신 상태는 양호합니다. 게이트 너머 복도에서 차원 편차가 빠르게 커지고 있어요.",
                    "operator",
                    NarrativePortraitSlot.Right,
                    "alert",
                    voiceClip: signalWarningVoice),
                Line(
                    4,
                    "외부 격벽 개방 준비. 잔여 동력은 62퍼센트입니다.",
                    "system",
                    NarrativePortraitSlot.None,
                    "status"),
                Line(
                    5,
                    "문이 열리면 우측 엄폐선을 확보하세요. 미확인 반응이 접근 중입니다.",
                    "operator",
                    NarrativePortraitSlot.Right,
                    "focused"),
                Line(
                    6,
                    "소환 장비가 불안정해. 그래도 한 번은 전개할 수 있어.",
                    "field_agent",
                    NarrativePortraitSlot.Left,
                    "alert"),
                Line(
                    7,
                    "진입 판단을 요청합니다. 어느 쪽이든 같은 게이트 개방 절차로 합류합니다.",
                    "operator",
                    NarrativePortraitSlot.Right,
                    "decision",
                    entryChoices),
                Line(
                    8,
                    "좋아. 게이트를 연다. 작전 시작.",
                    "field_agent",
                    NarrativePortraitSlot.Center,
                    "resolve")
            };

            profile.Configure(SequenceId, 0.042f, lines);
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static NarrativeSequenceProfile.LineEntry Line(
            int index,
            string fallbackKorean,
            string speakerId,
            NarrativePortraitSlot portraitSlot,
            string expressionId,
            NarrativeSequenceProfile.ChoiceEntry[] choices = null,
            AudioClip voiceClip = null)
        {
            string lineId = $"review.olympus.prologue.line.{index:00}";
            return new NarrativeSequenceProfile.LineEntry(
                lineId,
                lineId + ".text",
                fallbackKorean,
                speakerId,
                portraitSlot,
                expressionId,
                null,
                voiceClip,
                0f,
                choices);
        }

        private static TimelineAsset EnsureTimelineAsset()
        {
            TimelineAsset timeline = AssetDatabase.LoadAssetAtPath<TimelineAsset>(TimelinePath);
            if (timeline == null)
            {
                timeline = ScriptableObject.CreateInstance<TimelineAsset>();
                timeline.name = "DB_Timeline_OlympusTutorialReview";
                AssetDatabase.CreateAsset(timeline, TimelinePath);
            }

            List<TrackAsset> oldTracks = timeline.GetRootTracks().ToList();
            for (int i = 0; i < oldTracks.Count; i++)
            {
                timeline.DeleteTrack(oldTracks[i]);
            }

            timeline.durationMode = TimelineAsset.DurationMode.FixedLength;
            timeline.fixedDuration = TimelineDurationSeconds;
            EditorUtility.SetDirty(timeline);
            return timeline;
        }

        private static AnimationClip EnsureCameraAnimation()
        {
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(CameraAnimationPath);
            if (clip == null)
            {
                clip = new AnimationClip
                {
                    name = "DB_Anim_OlympusTutorialReviewCamera",
                    frameRate = 30f,
                    legacy = false,
                    wrapMode = WrapMode.ClampForever
                };
                AssetDatabase.CreateAsset(clip, CameraAnimationPath);
            }

            EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(clip);
            for (int i = 0; i < bindings.Length; i++)
            {
                AnimationUtility.SetEditorCurve(clip, bindings[i], null);
            }

            SetCurve(clip, "m_LocalPosition.x", SmoothCurve(
                Key(0f, -0.38f), Key(2.4f, 0.18f), Key(5.2f, -0.12f), Key(8f, 0f)));
            SetCurve(clip, "m_LocalPosition.y", SmoothCurve(
                Key(0f, 2.55f), Key(3f, 2.35f), Key(6f, 2.18f), Key(8f, 2.10f)));
            SetCurve(clip, "m_LocalPosition.z", SmoothCurve(
                Key(0f, -10.8f), Key(2.4f, -8.1f), Key(5.2f, -4.6f), Key(8f, -1.8f)));
            SetCurve(clip, "localEulerAnglesRaw.x", SmoothCurve(
                Key(0f, 5.8f), Key(3f, 4.2f), Key(8f, 2.8f)));
            SetCurve(clip, "localEulerAnglesRaw.y", SmoothCurve(
                Key(0f, -2.4f), Key(2.6f, 1.6f), Key(5.6f, -0.8f), Key(8f, 0f)));
            SetCurve(clip, "localEulerAnglesRaw.z", SmoothCurve(
                Key(0f, 0.8f), Key(3.2f, -0.4f), Key(8f, 0f)));

            EditorUtility.SetDirty(clip);
            return clip;
        }

        private static TimelineRefs ConfigureTimeline(TimelineAsset timeline, AnimationClip cameraAnimation)
        {
            AnimationTrack cameraTrack = timeline.CreateTrack<AnimationTrack>(CameraTrackName);
            cameraTrack.trackOffset = TrackOffset.Auto;
            TimelineClip cameraClip = cameraTrack.CreateClip(cameraAnimation);
            cameraClip.displayName = "Gate Corridor Push-In";
            cameraClip.start = 0d;
            cameraClip.duration = TimelineDurationSeconds;
            cameraClip.easeInDuration = 0d;
            cameraClip.easeOutDuration = 0.35d;
            if (cameraClip.asset is AnimationPlayableAsset animationPlayable)
            {
                animationPlayable.removeStartOffset = false;
                animationPlayable.applyFootIK = false;
                animationPlayable.loop = AnimationPlayableAsset.LoopMode.Off;
                EditorUtility.SetDirty(animationPlayable);
            }

            IntroGatePodDialogueTrack dialogueTrack =
                timeline.CreateTrack<IntroGatePodDialogueTrack>(DialogueTrackName);
            AddDialogueClip(
                dialogueTrack,
                0.45d,
                2.15d,
                "SYSTEM",
                "격벽 해제 절차를 시작합니다.");
            AddDialogueClip(
                dialogueTrack,
                2.95d,
                2.20d,
                "작전 오퍼레이터",
                "게이트 너머의 신호가 움직입니다. 시야를 확보하세요.");
            AddDialogueClip(
                dialogueTrack,
                5.55d,
                1.95d,
                "현장 요원",
                "확인. 회랑 진입 준비 완료.");

            EditorUtility.SetDirty(cameraTrack);
            EditorUtility.SetDirty(dialogueTrack);
            EditorUtility.SetDirty(timeline);
            return new TimelineRefs(cameraTrack, dialogueTrack);
        }

        private static void AddDialogueClip(
            IntroGatePodDialogueTrack track,
            double start,
            double duration,
            string speaker,
            string dialogue)
        {
            TimelineClip clip = track.CreateClip<IntroGatePodDialogueClip>();
            clip.displayName = $"{speaker} / {dialogue}";
            clip.start = start;
            clip.duration = duration;
            clip.blendInDuration = 0.12d;
            clip.blendOutDuration = 0.16d;
            if (clip.asset is IntroGatePodDialogueClip dialogueAsset)
            {
                dialogueAsset.SpeakerName = speaker;
                dialogueAsset.DialogueText = dialogue;
                dialogueAsset.FadeInSeconds = 0.12f;
                dialogueAsset.FadeOutSeconds = 0.16f;
                dialogueAsset.MaxAlpha = 1f;
                EditorUtility.SetDirty(dialogueAsset);
            }
        }

        private static void ConfigureController(
            OlympusChapterNarrativeReviewController controller,
            NarrativeSequenceProfile narrativeProfile,
            UIStageCatalog stageCatalog,
            PlayableDirector director,
            StageCutscenePort cutscenePort,
            AudioSource voiceSource,
            ReviewUiRefs ui,
            OlympusStoryTutorialTransitionReviewGate transitionGate)
        {
            controller.ConfigureCore(
                narrativeProfile,
                stageCatalog,
                director,
                cutscenePort,
                voiceSource);
            controller.ConfigureStoryTutorialTransitionGate(transitionGate);
            controller.ConfigureFlowGroups(
                ui.ChapterEntryGroup,
                ui.VisualNovelGroup,
                ui.CutsceneControlsGroup,
                ui.StageBriefingGroup,
                ui.CompleteGroup);
            controller.ConfigureChapterView(
                ui.ChapterEyebrow,
                ui.ChapterTitle,
                ui.ChapterStageTitle,
                ui.ChapterObjective,
                ui.ChapterStatus,
                ui.ChapterEnterButton);
            controller.ConfigureNarrativeView(
                ui.NarrativeSequence,
                ui.NarrativeSpeaker,
                ui.NarrativeLine,
                ui.NarrativeProgress,
                ui.LeftPortraitGroup,
                ui.CenterPortraitGroup,
                ui.RightPortraitGroup,
                ui.LeftPortraitImage,
                ui.CenterPortraitImage,
                ui.RightPortraitImage,
                ui.NarrativeNextButton,
                ui.NarrativeAutoButton,
                ui.NarrativeAutoButtonText,
                ui.NarrativeSkipButton,
                ui.NarrativeLogButton,
                ui.NarrativeChoiceGroup,
                ui.FirstChoiceButton,
                ui.FirstChoiceText,
                ui.SecondChoiceButton,
                ui.SecondChoiceText);
            controller.ConfigureCutsceneView(
                ui.CutsceneLabel,
                ui.CutsceneProgress,
                ui.CutsceneSkipButton);
            controller.ConfigureBriefingView(
                ui.BriefingTitle,
                ui.BriefingObjective,
                ui.BriefingCombatLesson,
                ui.BriefingThreat,
                ui.BriefingSummon,
                ui.BriefingDuration,
                ui.BriefingRewardRow,
                ui.BriefingReward,
                ui.BriefingDigest,
                ui.BriefingStatus,
                ui.BriefingCompleteButton);
            controller.ConfigureCompleteView(
                ui.CompleteTitle,
                ui.CompleteSummary,
                ui.RestartButton);
            controller.ConfigureUtilityPanels(
                ui.LogGroup,
                ui.LogText,
                ui.LogCloseButton,
                ui.SkipConfirmGroup,
                ui.SkipConfirmButton,
                ui.SkipCancelButton);
        }

        private static void ConfigureSceneEnvironment()
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.045f, 0.075f, 0.12f);
            RenderSettings.ambientEquatorColor = new Color(0.025f, 0.045f, 0.075f);
            RenderSettings.ambientGroundColor = new Color(0.012f, 0.018f, 0.032f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.018f, 0.055f, 0.075f);
            RenderSettings.fogDensity = 0.014f;
        }

        private static DioramaRefs CreateDiorama(Transform parent, ReviewMaterials materials)
        {
            Transform root = CreateChild(parent, "Review_GateCorridor_Diorama");
            root.localPosition = Vector3.zero;

            CreatePrimitive(
                PrimitiveType.Cube,
                root,
                "CorridorFloor",
                new Vector3(0f, -0.15f, 4f),
                new Vector3(10f, 0.3f, 30f),
                materials.Floor);
            CreatePrimitive(
                PrimitiveType.Cube,
                root,
                "CorridorCeiling",
                new Vector3(0f, 6.15f, 4f),
                new Vector3(10f, 0.3f, 30f),
                materials.Structure);
            CreatePrimitive(
                PrimitiveType.Cube,
                root,
                "LeftWall",
                new Vector3(-5.1f, 3f, 4f),
                new Vector3(0.35f, 6.3f, 30f),
                materials.Structure);
            CreatePrimitive(
                PrimitiveType.Cube,
                root,
                "RightWall",
                new Vector3(5.1f, 3f, 4f),
                new Vector3(0.35f, 6.3f, 30f),
                materials.Structure);

            for (int i = 0; i < 7; i++)
            {
                float z = -7f + (i * 4f);
                CreatePrimitive(
                    PrimitiveType.Cube,
                    root,
                    $"LeftRib_{i:00}",
                    new Vector3(-4.65f, 3f, z),
                    new Vector3(0.26f, 5.7f, 0.35f),
                    materials.Frame);
                CreatePrimitive(
                    PrimitiveType.Cube,
                    root,
                    $"RightRib_{i:00}",
                    new Vector3(4.65f, 3f, z),
                    new Vector3(0.26f, 5.7f, 0.35f),
                    materials.Frame);
                CreatePrimitive(
                    PrimitiveType.Cube,
                    root,
                    $"CeilingRib_{i:00}",
                    new Vector3(0f, 5.74f, z),
                    new Vector3(9.5f, 0.24f, 0.35f),
                    materials.Frame);

                Material stripMaterial = i % 3 == 0 ? materials.Warning : materials.Emissive;
                CreatePrimitive(
                    PrimitiveType.Cube,
                    root,
                    $"LeftSignalStrip_{i:00}",
                    new Vector3(-4.43f, 1.05f, z + 0.2f),
                    new Vector3(0.08f, 1.1f, 1.4f),
                    stripMaterial);
                CreatePrimitive(
                    PrimitiveType.Cube,
                    root,
                    $"RightSignalStrip_{i:00}",
                    new Vector3(4.43f, 1.05f, z + 0.2f),
                    new Vector3(0.08f, 1.1f, 1.4f),
                    stripMaterial);
            }

            Transform gate = CreateChild(root, "GateAssembly");
            gate.localPosition = new Vector3(0f, 0f, 10.5f);
            CreatePrimitive(
                PrimitiveType.Cube,
                gate,
                "GateHeader",
                new Vector3(0f, 5.25f, 0f),
                new Vector3(9.4f, 1.25f, 1.2f),
                materials.Frame);
            CreatePrimitive(
                PrimitiveType.Cube,
                gate,
                "GateLeftColumn",
                new Vector3(-4.15f, 2.4f, 0f),
                new Vector3(1.1f, 5.7f, 1.2f),
                materials.Frame);
            CreatePrimitive(
                PrimitiveType.Cube,
                gate,
                "GateRightColumn",
                new Vector3(4.15f, 2.4f, 0f),
                new Vector3(1.1f, 5.7f, 1.2f),
                materials.Frame);
            CreatePrimitive(
                PrimitiveType.Cube,
                gate,
                "GateDoorLeft",
                new Vector3(-1.72f, 2.35f, 0.12f),
                new Vector3(3.35f, 4.55f, 0.5f),
                materials.Door);
            CreatePrimitive(
                PrimitiveType.Cube,
                gate,
                "GateDoorRight",
                new Vector3(1.72f, 2.35f, 0.12f),
                new Vector3(3.35f, 4.55f, 0.5f),
                materials.Door);
            CreatePrimitive(
                PrimitiveType.Cube,
                gate,
                "GateSignalSpine",
                new Vector3(0f, 2.35f, -0.18f),
                new Vector3(0.09f, 4.2f, 0.7f),
                materials.Emissive);
            CreatePrimitive(
                PrimitiveType.Cylinder,
                gate,
                "GateSignalCore",
                new Vector3(0f, 4.82f, -0.52f),
                new Vector3(0.45f, 0.16f, 0.45f),
                materials.Warning,
                new Vector3(90f, 0f, 0f));

            CreateLight(
                root,
                "GateCyanKey",
                LightType.Point,
                new Vector3(0f, 4.1f, 7.8f),
                new Color(0.18f, 0.78f, 1f),
                13f,
                15f);
            CreateLight(
                root,
                "GateWarmAccent",
                LightType.Point,
                new Vector3(0f, 1.2f, 10f),
                new Color(1f, 0.48f, 0.16f),
                6f,
                8f);

            Transform cameraTransform = CreateChild(parent, "ReviewCutsceneCamera");
            cameraTransform.localPosition = new Vector3(-0.38f, 2.55f, -10.8f);
            cameraTransform.localEulerAngles = new Vector3(5.8f, -2.4f, 0.8f);
            Camera camera = cameraTransform.gameObject.AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.005f, 0.012f, 0.026f);
            camera.fieldOfView = 57f;
            camera.nearClipPlane = 0.06f;
            camera.farClipPlane = 120f;
            camera.depth = 0f;
            AudioListener listener = cameraTransform.gameObject.AddComponent<AudioListener>();
            Animator animator = cameraTransform.gameObject.AddComponent<Animator>();
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            Transform narrativeCameraTransform =
                CreateChild(parent, "ReviewNarrativePresentationCamera");
            narrativeCameraTransform.localPosition = cameraTransform.localPosition;
            narrativeCameraTransform.localRotation = cameraTransform.localRotation;
            Camera narrativeCamera = narrativeCameraTransform.gameObject.AddComponent<Camera>();
            narrativeCamera.tag = "Untagged";
            narrativeCamera.clearFlags = camera.clearFlags;
            narrativeCamera.backgroundColor = camera.backgroundColor;
            narrativeCamera.fieldOfView = camera.fieldOfView;
            narrativeCamera.nearClipPlane = camera.nearClipPlane;
            narrativeCamera.farClipPlane = camera.farClipPlane;
            narrativeCamera.depth = camera.depth;
            narrativeCamera.enabled = false;
            AudioListener narrativeListener =
                narrativeCameraTransform.gameObject.AddComponent<AudioListener>();
            narrativeListener.enabled = false;

            return new DioramaRefs(
                root,
                camera,
                listener,
                animator,
                narrativeCamera,
                narrativeListener);
        }

        private static ReviewUiRefs CreateReviewUi(
            Transform parent,
            TMP_FontAsset mediumFont,
            TMP_FontAsset semiBoldFont,
            Sprite reviewBackground)
        {
            var refs = new ReviewUiRefs();
            GameObject canvasObject = new GameObject(
                "NarrativeReviewCanvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(parent, false);
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            scaler.referencePixelsPerUnit = 100f;

            RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
            refs.Canvas = canvas;
            refs.CanvasScaler = scaler;
            refs.ChapterEntryGroup = CreateFlowGroup(canvasRect, "ChapterEntry", true);
            BuildChapterEntry(
                refs.ChapterEntryGroup.transform as RectTransform,
                mediumFont,
                semiBoldFont,
                reviewBackground,
                refs);

            refs.VisualNovelGroup = CreateFlowGroup(canvasRect, "VisualNovel", false);
            BuildVisualNovel(
                refs.VisualNovelGroup.transform as RectTransform,
                mediumFont,
                semiBoldFont,
                reviewBackground,
                refs);

            refs.CutsceneControlsGroup = CreateFlowGroup(canvasRect, "CutsceneControls", false);
            BuildCutsceneControls(
                refs.CutsceneControlsGroup.transform as RectTransform,
                mediumFont,
                semiBoldFont,
                refs);

            refs.StageBriefingGroup = CreateFlowGroup(canvasRect, "StageBriefing", false);
            BuildStageBriefing(
                refs.StageBriefingGroup.transform as RectTransform,
                mediumFont,
                semiBoldFont,
                refs);

            refs.CompleteGroup = CreateFlowGroup(canvasRect, "Complete", false);
            BuildComplete(refs.CompleteGroup.transform as RectTransform, mediumFont, semiBoldFont, refs);

            Image gameplayHudProbe = CreateImage(
                canvasRect,
                "ReviewGameplayHudProbe",
                new Color(0.025f, 0.09f, 0.12f, 0.88f),
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -18f),
                new Vector2(360f, 46f));
            refs.GameplayHudProbeGroup =
                gameplayHudProbe.gameObject.AddComponent<CanvasGroup>();
            refs.GameplayHudProbeGroup.alpha = 0.86f;
            refs.GameplayHudProbeGroup.interactable = false;
            refs.GameplayHudProbeGroup.blocksRaycasts = false;
            CreateText(
                gameplayHudProbe.rectTransform,
                "ReviewGameplayHudProbeLabel",
                "LOCAL GAMEPLAY HUD  /  RESTORE PROBE",
                semiBoldFont,
                18f,
                new Color(0.42f, 0.92f, 1f, 1f),
                TextAlignmentOptions.Center,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero);

            BuildCutsceneDialogueOverlay(canvasRect, mediumFont, semiBoldFont, refs);
            BuildLogModal(canvasRect, mediumFont, semiBoldFont, refs);
            BuildSkipConfirmModal(canvasRect, mediumFont, semiBoldFont, refs);
            return refs;
        }

        private static void BuildChapterEntry(
            RectTransform root,
            TMP_FontAsset mediumFont,
            TMP_FontAsset semiBoldFont,
            Sprite reviewBackground,
            ReviewUiRefs refs)
        {
            Image keyVisual = CreateImage(
                root,
                "ChapterKeyVisual",
                new Color(0.70f, 0.78f, 0.86f, 1f),
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero);
            keyVisual.sprite = reviewBackground;
            keyVisual.type = Image.Type.Simple;
            keyVisual.preserveAspect = false;
            CreateImage(
                root,
                "BackdropWash",
                new Color(Ink.r, Ink.g, Ink.b, 0.77f),
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero);
            CreateImage(
                root,
                "SignalGlow",
                new Color(0.03f, 0.32f, 0.45f, 0.25f),
                new Vector2(0.60f, 0f),
                Vector2.one,
                Vector2.zero,
                Vector2.zero);
            CreateImage(
                root,
                "LeftRail",
                Cyan,
                new Vector2(0f, 0f),
                new Vector2(0f, 1f),
                new Vector2(88f, 0f),
                new Vector2(4f, -160f));
            CreateText(
                root,
                "ReviewTag",
                "REVIEW SAMPLE  /  TEMP_DO_NOT_SHIP",
                semiBoldFont,
                22f,
                Gold,
                TextAlignmentOptions.Left,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(96f, -76f),
                new Vector2(700f, 42f));
            refs.ChapterEyebrow = CreateText(
                root,
                "ChapterEyebrow",
                "CHAPTER 00 / OLYMPUS SIGNAL",
                semiBoldFont,
                25f,
                Cyan,
                TextAlignmentOptions.Left,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(96f, -180f),
                new Vector2(740f, 46f));
            refs.ChapterTitle = CreateText(
                root,
                "ChapterTitle",
                "게이트 신호",
                semiBoldFont,
                84f,
                White,
                TextAlignmentOptions.Left,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(92f, -280f),
                new Vector2(800f, 120f));
            CreateText(
                root,
                "ChapterIndex",
                "00",
                semiBoldFont,
                220f,
                new Color(0.18f, 0.46f, 0.60f, 0.14f),
                TextAlignmentOptions.Right,
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                new Vector2(710f, 190f),
                new Vector2(620f, 280f));

            Image stageCard = CreateImage(
                root,
                "StageCard",
                Panel,
                new Vector2(1f, 0.5f),
                new Vector2(1f, 0.5f),
                new Vector2(-390f, -8f),
                new Vector2(650f, 650f));
            CreateImage(
                stageCard.rectTransform,
                "CardAccent",
                Cyan,
                new Vector2(0f, 0f),
                new Vector2(0f, 1f),
                new Vector2(0f, 0f),
                new Vector2(5f, 0f));
            CreateText(
                stageCard.rectTransform,
                "EntryLabel",
                "PROLOGUE ENTRY",
                semiBoldFont,
                18f,
                CyanSoft,
                TextAlignmentOptions.Left,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(44f, -52f),
                new Vector2(540f, 34f));
            refs.ChapterStageTitle = CreateText(
                stageCard.rectTransform,
                "StageTitle",
                "작전 경로 확인 중",
                semiBoldFont,
                38f,
                White,
                TextAlignmentOptions.Left,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(44f, -105f),
                new Vector2(550f, 76f));
            refs.ChapterObjective = CreateText(
                stageCard.rectTransform,
                "Objective",
                "DB_UIStageCatalog에서 정식 브리핑을 읽습니다.",
                mediumFont,
                25f,
                new Color(0.77f, 0.84f, 0.91f),
                TextAlignmentOptions.TopLeft,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(44f, -205f),
                new Vector2(550f, 160f));
            refs.ChapterStatus = CreateText(
                stageCard.rectTransform,
                "Status",
                "STORY ENTRY READY  ·  REVIEW-ONLY",
                semiBoldFont,
                17f,
                Gold,
                TextAlignmentOptions.Left,
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                new Vector2(44f, 126f),
                new Vector2(540f, 34f));
            refs.ChapterEnterButton = CreateButton(
                stageCard.rectTransform,
                "EnterButton",
                "프롤로그 시작",
                semiBoldFont,
                27f,
                Cyan,
                new Color(0.015f, 0.075f, 0.11f),
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 50f),
                new Vector2(548f, 82f));
            CreateText(
                root,
                "FlowCaption",
                "CHAPTER ENTRY  →  VISUAL NOVEL  →  TUTORIAL CUTSCENE  →  STAGE BRIEFING",
                mediumFont,
                17f,
                Muted,
                TextAlignmentOptions.Left,
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                new Vector2(96f, 82f),
                new Vector2(980f, 36f));
        }

        private static void BuildVisualNovel(
            RectTransform root,
            TMP_FontAsset mediumFont,
            TMP_FontAsset semiBoldFont,
            Sprite reviewBackground,
            ReviewUiRefs refs)
        {
            Image keyVisual = CreateImage(
                root,
                "VisualNovelKeyVisual",
                new Color(0.74f, 0.82f, 0.90f, 1f),
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero);
            keyVisual.sprite = reviewBackground;
            keyVisual.type = Image.Type.Simple;
            keyVisual.preserveAspect = false;
            CreateImage(
                root,
                "BackdropWash",
                new Color(0.012f, 0.025f, 0.05f, 0.58f),
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero);
            CreateImage(
                root,
                "HorizonGlow",
                new Color(0.08f, 0.55f, 0.72f, 0.13f),
                new Vector2(0f, 0.36f),
                new Vector2(1f, 0.70f),
                Vector2.zero,
                Vector2.zero);

            refs.NarrativeSequence = CreateText(
                root,
                "SequenceLabel",
                "REVIEW SAMPLE / OLYMPUS GATE SIGNAL",
                semiBoldFont,
                18f,
                Cyan,
                TextAlignmentOptions.Left,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(72f, -54f),
                new Vector2(930f, 34f));
            refs.NarrativeProgress = CreateText(
                root,
                "Progress",
                "01 / 08",
                semiBoldFont,
                18f,
                Muted,
                TextAlignmentOptions.Right,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-548f, -54f),
                new Vector2(150f, 34f));

            PortraitRefs left = CreatePortrait(
                root,
                "FieldAgentLeft",
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                new Vector2(318f, 430f),
                new Vector2(490f, 690f),
                new Color(0.12f, 0.48f, 0.62f, 0.80f));
            PortraitRefs center = CreatePortrait(
                root,
                "FieldAgentCenter",
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 430f),
                new Vector2(520f, 720f),
                new Color(0.16f, 0.66f, 0.78f, 0.82f));
            PortraitRefs right = CreatePortrait(
                root,
                "OperatorRight",
                new Vector2(1f, 0f),
                new Vector2(1f, 0f),
                new Vector2(-318f, 430f),
                new Vector2(490f, 690f),
                new Color(0.56f, 0.32f, 0.72f, 0.80f));
            refs.LeftPortraitGroup = left.Group;
            refs.LeftPortraitImage = left.Body;
            refs.CenterPortraitGroup = center.Group;
            refs.CenterPortraitImage = center.Body;
            refs.RightPortraitGroup = right.Group;
            refs.RightPortraitImage = right.Body;

            Image dialoguePanel = CreateImage(
                root,
                "DialoguePanel",
                new Color(0.018f, 0.032f, 0.065f, 0.97f),
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 36f),
                new Vector2(1770f, 290f));
            CreateImage(
                dialoguePanel.rectTransform,
                "DialogueSignalLine",
                Cyan,
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(0f, 0f),
                new Vector2(0f, 3f));
            refs.NarrativeSpeaker = CreateText(
                dialoguePanel.rectTransform,
                "Speaker",
                "SYSTEM",
                semiBoldFont,
                24f,
                Gold,
                TextAlignmentOptions.Left,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(42f, -34f),
                new Vector2(620f, 42f));
            refs.NarrativeLine = CreateText(
                dialoguePanel.rectTransform,
                "Line",
                "올림포스 게이트 신호를 포착했습니다.",
                mediumFont,
                31f,
                White,
                TextAlignmentOptions.TopLeft,
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(42f, -92f),
                new Vector2(-84f, 104f));
            refs.NarrativeNextButton = CreateButton(
                dialoguePanel.rectTransform,
                "NextButton",
                "NEXT  ›",
                semiBoldFont,
                18f,
                Cyan,
                new Color(0.03f, 0.11f, 0.16f),
                new Vector2(1f, 0f),
                new Vector2(1f, 0f),
                new Vector2(-115f, 34f),
                new Vector2(180f, 58f));

            refs.NarrativeAutoButton = CreateButton(
                root,
                "AutoButton",
                "AUTO  OFF",
                semiBoldFont,
                16f,
                CyanSoft,
                new Color(0.025f, 0.055f, 0.09f, 0.9f),
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-380f, -56f),
                new Vector2(150f, 50f),
                out refs.NarrativeAutoButtonText);
            refs.NarrativeLogButton = CreateButton(
                root,
                "LogButton",
                "LOG",
                semiBoldFont,
                16f,
                CyanSoft,
                new Color(0.025f, 0.055f, 0.09f, 0.9f),
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-218f, -56f),
                new Vector2(130f, 50f));
            refs.NarrativeSkipButton = CreateButton(
                root,
                "SkipButton",
                "SKIP",
                semiBoldFont,
                16f,
                Gold,
                new Color(0.10f, 0.065f, 0.03f, 0.9f),
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-78f, -56f),
                new Vector2(130f, 50f));

            Image choicePanel = CreateImage(
                root,
                "ChoicePanel",
                new Color(0.015f, 0.025f, 0.05f, 0.96f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 64f),
                new Vector2(820f, 250f));
            refs.NarrativeChoiceGroup = choicePanel.gameObject.AddComponent<CanvasGroup>();
            CreateText(
                choicePanel.rectTransform,
                "ChoicePrompt",
                "진입 판단",
                semiBoldFont,
                20f,
                Gold,
                TextAlignmentOptions.Center,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -30f),
                new Vector2(720f, 38f));
            refs.FirstChoiceButton = CreateButton(
                choicePanel.rectTransform,
                "ChoiceA",
                "즉시 진입한다",
                semiBoldFont,
                21f,
                White,
                new Color(0.04f, 0.15f, 0.21f),
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -93f),
                new Vector2(700f, 60f),
                out refs.FirstChoiceText);
            refs.SecondChoiceButton = CreateButton(
                choicePanel.rectTransform,
                "ChoiceB",
                "상황을 한 번 더 확인한다",
                semiBoldFont,
                21f,
                White,
                new Color(0.055f, 0.075f, 0.12f),
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -165f),
                new Vector2(700f, 60f),
                out refs.SecondChoiceText);
            SetGroup(refs.NarrativeChoiceGroup, false);
        }

        private static void BuildCutsceneControls(
            RectTransform root,
            TMP_FontAsset mediumFont,
            TMP_FontAsset semiBoldFont,
            ReviewUiRefs refs)
        {
            CreateImage(
                root,
                "TopShade",
                new Color(0.005f, 0.01f, 0.02f, 0.72f),
                new Vector2(0f, 0.82f),
                Vector2.one,
                Vector2.zero,
                Vector2.zero);
            refs.CutsceneLabel = CreateText(
                root,
                "CutsceneLabel",
                "TUTORIAL CUTSCENE / GATE LINK",
                semiBoldFont,
                18f,
                Cyan,
                TextAlignmentOptions.Left,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(68f, -52f),
                new Vector2(740f, 34f));
            refs.CutsceneProgress = CreateText(
                root,
                "CutsceneProgress",
                "SIGNAL LINK  00%",
                mediumFont,
                17f,
                White,
                TextAlignmentOptions.Left,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(68f, -92f),
                new Vector2(380f, 32f));
            refs.CutsceneSkipButton = CreateButton(
                root,
                "CutsceneSkipButton",
                "SKIP CUTSCENE",
                semiBoldFont,
                16f,
                Gold,
                new Color(0.075f, 0.045f, 0.02f, 0.82f),
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-112f, -58f),
                new Vector2(190f, 52f));
            CreateText(
                root,
                "ReviewOnlyLabel",
                "INDEPENDENT REVIEW DIORAMA  ·  NO CANONICAL TIMELINE MUTATION",
                mediumFont,
                14f,
                new Color(0.60f, 0.72f, 0.82f, 0.75f),
                TextAlignmentOptions.Right,
                new Vector2(1f, 0f),
                new Vector2(1f, 0f),
                new Vector2(-62f, 36f),
                new Vector2(760f, 30f));
        }

        private static void BuildStageBriefing(
            RectTransform root,
            TMP_FontAsset mediumFont,
            TMP_FontAsset semiBoldFont,
            ReviewUiRefs refs)
        {
            CreateImage(root, "Backdrop", new Color(0.008f, 0.018f, 0.038f, 0.97f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            CreateText(
                root,
                "SectionEyebrow",
                "CANONICAL STAGE BRIEFING",
                semiBoldFont,
                20f,
                Cyan,
                TextAlignmentOptions.Left,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(86f, -72f),
                new Vector2(720f, 38f));
            refs.BriefingTitle = CreateText(
                root,
                "BriefingTitle",
                "작전 브리핑",
                semiBoldFont,
                58f,
                White,
                TextAlignmentOptions.Left,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(82f, -138f),
                new Vector2(920f, 88f));
            refs.BriefingDigest = CreateText(
                root,
                "BriefingDigest",
                "BRIEFING DIGEST  --------",
                mediumFont,
                15f,
                Muted,
                TextAlignmentOptions.Right,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-78f, -84f),
                new Vector2(520f, 32f));

            Image contentFrame = CreateImage(
                root,
                "BriefingContentFrame",
                Color.clear,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -26f),
                new Vector2(1500f, 610f));
            Image objectiveCard = CreateImage(
                contentFrame.rectTransform,
                "ObjectiveCard",
                Panel,
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                Vector2.zero,
                new Vector2(720f, 610f));
            CreateText(
                objectiveCard.rectTransform,
                "ObjectiveLabel",
                "PRIMARY OBJECTIVE",
                semiBoldFont,
                17f,
                Gold,
                TextAlignmentOptions.Left,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(38f, -40f),
                new Vector2(630f, 32f));
            refs.BriefingObjective = CreateText(
                objectiveCard.rectTransform,
                "Objective",
                "정식 스테이지 목표를 불러오는 중입니다.",
                semiBoldFont,
                27f,
                White,
                TextAlignmentOptions.TopLeft,
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(38f, -96f),
                new Vector2(-76f, 150f));
            CreateText(
                objectiveCard.rectTransform,
                "LessonLabel",
                "COMBAT LESSON",
                semiBoldFont,
                17f,
                CyanSoft,
                TextAlignmentOptions.Left,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(38f, -285f),
                new Vector2(630f, 32f));
            refs.BriefingCombatLesson = CreateText(
                objectiveCard.rectTransform,
                "CombatLesson",
                "전투 학습 목표",
                mediumFont,
                21f,
                new Color(0.80f, 0.87f, 0.94f),
                TextAlignmentOptions.TopLeft,
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(38f, -330f),
                new Vector2(-76f, 118f));
            refs.BriefingStatus = CreateText(
                objectiveCard.rectTransform,
                "BriefingStatus",
                "CANONICAL DATA / REWARD HIDDEN WHEN UNVERIFIED",
                semiBoldFont,
                15f,
                Gold,
                TextAlignmentOptions.Left,
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                new Vector2(38f, 34f),
                new Vector2(640f, 30f));

            Image intelCard = CreateImage(
                contentFrame.rectTransform,
                "IntelCard",
                PanelSoft,
                new Vector2(1f, 0.5f),
                new Vector2(1f, 0.5f),
                Vector2.zero,
                new Vector2(720f, 610f));
            refs.BriefingThreat = CreateIntelRow(
                intelCard.rectTransform,
                "ThreatRow",
                "THREAT TAGS",
                "--",
                46f,
                mediumFont,
                semiBoldFont);
            refs.BriefingSummon = CreateIntelRow(
                intelCard.rectTransform,
                "SummonRow",
                "RECOMMENDED SUMMON",
                "--",
                154f,
                mediumFont,
                semiBoldFont);
            refs.BriefingDuration = CreateIntelRow(
                intelCard.rectTransform,
                "DurationRow",
                "EXPECTED RUN",
                "03:00—05:00",
                262f,
                mediumFont,
                semiBoldFont);

            Image rewardRow = CreateImage(
                intelCard.rectTransform,
                "RewardRow",
                new Color(0.11f, 0.085f, 0.035f, 0.75f),
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -424f),
                new Vector2(640f, 88f));
            refs.BriefingRewardRow = rewardRow.gameObject;
            CreateText(
                rewardRow.rectTransform,
                "RewardLabel",
                "VERIFIED REWARD",
                semiBoldFont,
                14f,
                Gold,
                TextAlignmentOptions.Left,
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(24f, 0f),
                new Vector2(200f, 30f));
            refs.BriefingReward = CreateText(
                rewardRow.rectTransform,
                "Reward",
                string.Empty,
                mediumFont,
                21f,
                White,
                TextAlignmentOptions.Right,
                new Vector2(1f, 0.5f),
                new Vector2(1f, 0.5f),
                new Vector2(-24f, 0f),
                new Vector2(400f, 42f));
            rewardRow.gameObject.SetActive(false);

            refs.BriefingCompleteButton = CreateButton(
                root,
                "BriefingCompleteButton",
                "검토 완료",
                semiBoldFont,
                25f,
                Cyan,
                new Color(0.018f, 0.09f, 0.13f),
                new Vector2(1f, 0f),
                new Vector2(1f, 0f),
                new Vector2(-190f, 68f),
                new Vector2(300f, 76f));
        }

        private static TMP_Text CreateIntelRow(
            RectTransform parent,
            string name,
            string label,
            string value,
            float topOffset,
            TMP_FontAsset mediumFont,
            TMP_FontAsset semiBoldFont)
        {
            Image row = CreateImage(
                parent,
                name,
                new Color(0.035f, 0.06f, 0.10f, 0.92f),
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -topOffset),
                new Vector2(640f, 88f));
            CreateText(
                row.rectTransform,
                "Label",
                label,
                semiBoldFont,
                14f,
                CyanSoft,
                TextAlignmentOptions.Left,
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(24f, 0f),
                new Vector2(260f, 30f));
            return CreateText(
                row.rectTransform,
                "Value",
                value,
                mediumFont,
                21f,
                White,
                TextAlignmentOptions.Right,
                new Vector2(1f, 0.5f),
                new Vector2(1f, 0.5f),
                new Vector2(-24f, 0f),
                new Vector2(330f, 42f));
        }

        private static void BuildComplete(
            RectTransform root,
            TMP_FontAsset mediumFont,
            TMP_FontAsset semiBoldFont,
            ReviewUiRefs refs)
        {
            CreateImage(root, "Backdrop", Ink, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            CreateImage(
                root,
                "CompletionHalo",
                new Color(0.04f, 0.65f, 0.72f, 0.16f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                new Vector2(940f, 940f));
            CreateText(
                root,
                "CompleteEyebrow",
                "REVIEW CHECKPOINT",
                semiBoldFont,
                20f,
                Cyan,
                TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 170f),
                new Vector2(800f, 38f));
            refs.CompleteTitle = CreateText(
                root,
                "CompleteTitle",
                "REVIEW FLOW COMPLETE",
                semiBoldFont,
                52f,
                White,
                TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 92f),
                new Vector2(1050f, 86f));
            refs.CompleteSummary = CreateText(
                root,
                "CompleteSummary",
                "ChapterEntry → VisualNovel → TutorialCutscene → StageBriefing",
                mediumFont,
                23f,
                new Color(0.76f, 0.84f, 0.91f),
                TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -28f),
                new Vector2(1040f, 160f));
            refs.RestartButton = CreateButton(
                root,
                "RestartButton",
                "처음부터 다시 보기",
                semiBoldFont,
                22f,
                Cyan,
                new Color(0.02f, 0.095f, 0.13f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -180f),
                new Vector2(340f, 70f));
        }

        private static void BuildCutsceneDialogueOverlay(
            RectTransform canvasRoot,
            TMP_FontAsset mediumFont,
            TMP_FontAsset semiBoldFont,
            ReviewUiRefs refs)
        {
            Image panel = CreateImage(
                canvasRoot,
                "CutsceneDialogueOverlay",
                new Color(0.012f, 0.022f, 0.045f, 0.94f),
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 76f),
                new Vector2(1360f, 150f));
            CanvasGroup group = panel.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;
            CreateImage(
                panel.rectTransform,
                "SignalLine",
                Cyan,
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                Vector2.zero,
                new Vector2(0f, 3f));
            TMP_Text speaker = CreateText(
                panel.rectTransform,
                "Speaker",
                string.Empty,
                semiBoldFont,
                18f,
                Gold,
                TextAlignmentOptions.Center,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -31f),
                new Vector2(440f, 32f));
            TMP_Text line = CreateText(
                panel.rectTransform,
                "Line",
                string.Empty,
                mediumFont,
                27f,
                White,
                TextAlignmentOptions.Center,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 45f),
                new Vector2(1240f, 64f));
            IntroGatePodDialogueOverlay overlay =
                panel.gameObject.AddComponent<IntroGatePodDialogueOverlay>();
            overlay.Configure(group, speaker, line);
            refs.CutsceneDialogueOverlay = overlay;
        }

        private static void BuildLogModal(
            RectTransform canvasRoot,
            TMP_FontAsset mediumFont,
            TMP_FontAsset semiBoldFont,
            ReviewUiRefs refs)
        {
            Image modal = CreateImage(
                canvasRoot,
                "NarrativeLogModal",
                new Color(0.003f, 0.008f, 0.018f, 0.92f),
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero);
            refs.LogGroup = modal.gameObject.AddComponent<CanvasGroup>();
            Image card = CreateImage(
                modal.rectTransform,
                "LogCard",
                InkSoft,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                new Vector2(1220f, 790f));
            CreateText(
                card.rectTransform,
                "Title",
                "DIALOGUE LOG  /  CURRENT SEQUENCE",
                semiBoldFont,
                22f,
                Cyan,
                TextAlignmentOptions.Left,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(46f, -42f),
                new Vector2(820f, 40f));
            refs.LogText = CreateText(
                card.rectTransform,
                "LogText",
                "아직 표시된 대사가 없습니다.",
                mediumFont,
                23f,
                new Color(0.82f, 0.88f, 0.94f),
                TextAlignmentOptions.TopLeft,
                new Vector2(0f, 0f),
                new Vector2(1f, 1f),
                Vector2.zero,
                new Vector2(-92f, -166f));
            refs.LogCloseButton = CreateButton(
                card.rectTransform,
                "CloseButton",
                "닫기",
                semiBoldFont,
                19f,
                Cyan,
                new Color(0.02f, 0.09f, 0.13f),
                new Vector2(1f, 0f),
                new Vector2(1f, 0f),
                new Vector2(-100f, 42f),
                new Vector2(150f, 54f));
            SetGroup(refs.LogGroup, false);
        }

        private static void BuildSkipConfirmModal(
            RectTransform canvasRoot,
            TMP_FontAsset mediumFont,
            TMP_FontAsset semiBoldFont,
            ReviewUiRefs refs)
        {
            Image modal = CreateImage(
                canvasRoot,
                "NarrativeSkipConfirmModal",
                new Color(0.003f, 0.008f, 0.018f, 0.90f),
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero);
            refs.SkipConfirmGroup = modal.gameObject.AddComponent<CanvasGroup>();
            Image card = CreateImage(
                modal.rectTransform,
                "ConfirmCard",
                Panel,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                new Vector2(720f, 340f));
            CreateText(
                card.rectTransform,
                "Title",
                "스토리를 건너뛸까요?",
                semiBoldFont,
                34f,
                White,
                TextAlignmentOptions.Center,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -72f),
                new Vector2(620f, 60f));
            CreateText(
                card.rectTransform,
                "Body",
                "튜토리얼 컷신으로 이동하며, 최종 핸드오프는 동일하게 실행됩니다.",
                mediumFont,
                21f,
                Muted,
                TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 30f),
                new Vector2(610f, 74f));
            refs.SkipCancelButton = CreateButton(
                card.rectTransform,
                "CancelButton",
                "계속 보기",
                semiBoldFont,
                20f,
                White,
                new Color(0.06f, 0.08f, 0.13f),
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                new Vector2(190f, 48f),
                new Vector2(260f, 62f));
            refs.SkipConfirmButton = CreateButton(
                card.rectTransform,
                "ConfirmButton",
                "건너뛰기",
                semiBoldFont,
                20f,
                Gold,
                new Color(0.12f, 0.07f, 0.025f),
                new Vector2(1f, 0f),
                new Vector2(1f, 0f),
                new Vector2(-190f, 48f),
                new Vector2(260f, 62f));
            SetGroup(refs.SkipConfirmGroup, false);
        }

        private static PortraitRefs CreatePortrait(
            RectTransform parent,
            string name,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Vector2 size,
            Color silhouetteColor)
        {
            Image frame = CreateImage(
                parent,
                name,
                new Color(0.03f, 0.06f, 0.10f, 0.16f),
                anchorMin,
                anchorMax,
                anchoredPosition,
                size);
            CanvasGroup group = frame.gameObject.AddComponent<CanvasGroup>();
            Image scanLine = CreateImage(
                frame.rectTransform,
                "ScanLine",
                new Color(silhouetteColor.r, silhouetteColor.g, silhouetteColor.b, 0.45f),
                new Vector2(0f, 0.18f),
                new Vector2(1f, 0.18f),
                Vector2.zero,
                new Vector2(0f, 3f));
            scanLine.raycastTarget = false;
            Image body = CreateImage(
                frame.rectTransform,
                "PortraitBody",
                silhouetteColor,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 170f),
                new Vector2(size.x * 0.58f, size.y * 0.52f));
            body.raycastTarget = false;
            Image shoulders = CreateImage(
                frame.rectTransform,
                "Shoulders",
                new Color(silhouetteColor.r * 0.82f, silhouetteColor.g * 0.82f, silhouetteColor.b * 0.82f, silhouetteColor.a),
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 72f),
                new Vector2(size.x * 0.78f, size.y * 0.22f));
            shoulders.raycastTarget = false;
            Image head = CreateImage(
                frame.rectTransform,
                "Head",
                new Color(silhouetteColor.r * 1.08f, silhouetteColor.g * 1.08f, silhouetteColor.b * 1.08f, silhouetteColor.a),
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, size.y * 0.57f),
                new Vector2(size.x * 0.29f, size.x * 0.35f));
            head.raycastTarget = false;
            return new PortraitRefs(group, body);
        }

        private static CanvasGroup CreateFlowGroup(RectTransform parent, string name, bool visible)
        {
            GameObject groupObject = new GameObject(name, typeof(RectTransform), typeof(CanvasGroup));
            RectTransform rect = groupObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            Stretch(rect);
            CanvasGroup group = groupObject.GetComponent<CanvasGroup>();
            SetGroup(group, visible);
            return group;
        }

        private static void EnsureEventSystem(Transform parent)
        {
            GameObject eventSystemObject = new GameObject(
                "EventSystem",
                typeof(EventSystem),
                typeof(InputSystemUIInputModule));
            eventSystemObject.transform.SetParent(parent, false);
        }

        private static void SetInitialVisibility(ReviewUiRefs refs)
        {
            SetGroup(refs.ChapterEntryGroup, true);
            SetGroup(refs.VisualNovelGroup, false);
            SetGroup(refs.CutsceneControlsGroup, false);
            SetGroup(refs.StageBriefingGroup, false);
            SetGroup(refs.CompleteGroup, false);
            SetGroup(refs.NarrativeChoiceGroup, false);
            SetGroup(refs.LogGroup, false);
            SetGroup(refs.SkipConfirmGroup, false);
            refs.BriefingRewardRow.SetActive(false);
        }

        private static Image CreateImage(
            RectTransform parent,
            string name,
            Color color,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Vector2 sizeDelta)
        {
            GameObject imageObject = new GameObject(
                name,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            RectTransform rect = imageObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            ConfigureRect(rect, anchorMin, anchorMax, anchoredPosition, sizeDelta);
            Image image = imageObject.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static TMP_Text CreateText(
            RectTransform parent,
            string name,
            string value,
            TMP_FontAsset font,
            float fontSize,
            Color color,
            TextAlignmentOptions alignment,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Vector2 sizeDelta)
        {
            GameObject textObject = new GameObject(
                name,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(TextMeshProUGUI));
            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            ConfigureRect(rect, anchorMin, anchorMax, anchoredPosition, sizeDelta);
            TMP_Text text = textObject.GetComponent<TMP_Text>();
            text.font = font;
            text.fontSize = fontSize;
            text.fontStyle = FontStyles.Normal;
            text.color = color;
            text.alignment = alignment;
            text.text = value ?? string.Empty;
            text.raycastTarget = false;
            text.richText = true;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.overflowMode = TextOverflowModes.Ellipsis;
            return text;
        }

        private static Button CreateButton(
            RectTransform parent,
            string name,
            string label,
            TMP_FontAsset font,
            float fontSize,
            Color accent,
            Color background,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Vector2 sizeDelta)
        {
            return CreateButton(
                parent,
                name,
                label,
                font,
                fontSize,
                accent,
                background,
                anchorMin,
                anchorMax,
                anchoredPosition,
                sizeDelta,
                out _);
        }

        private static Button CreateButton(
            RectTransform parent,
            string name,
            string label,
            TMP_FontAsset font,
            float fontSize,
            Color accent,
            Color background,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Vector2 sizeDelta,
            out TMP_Text labelText)
        {
            Image image = CreateImage(
                parent,
                name,
                background,
                anchorMin,
                anchorMax,
                anchoredPosition,
                sizeDelta);
            image.raycastTarget = true;
            Button button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.08f, 1.08f, 1.08f, 1f);
            colors.pressedColor = new Color(0.72f, 0.86f, 0.92f, 1f);
            colors.selectedColor = Color.white;
            colors.disabledColor = new Color(0.36f, 0.42f, 0.48f, 0.65f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.08f;
            button.colors = colors;
            Navigation navigation = button.navigation;
            navigation.mode = Navigation.Mode.Automatic;
            button.navigation = navigation;
            labelText = CreateText(
                image.rectTransform,
                "Label",
                label,
                font,
                fontSize,
                accent,
                TextAlignmentOptions.Center,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero);
            return button;
        }

        private static void ConfigureRect(
            RectTransform rect,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Vector2 sizeDelta)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(
                Mathf.Approximately(anchorMin.x, anchorMax.x) ? anchorMin.x : 0.5f,
                Mathf.Approximately(anchorMin.y, anchorMax.y) ? anchorMin.y : 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
            rect.localScale = Vector3.one;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
            rect.localScale = Vector3.one;
        }

        private static void SetGroup(CanvasGroup group, bool visible)
        {
            if (group == null)
            {
                return;
            }

            group.alpha = visible ? 1f : 0f;
            group.interactable = visible;
            group.blocksRaycasts = visible;
        }

        private static Transform CreateChild(Transform parent, string name)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(parent, false);
            return child.transform;
        }

        private static GameObject CreatePrimitive(
            PrimitiveType primitiveType,
            Transform parent,
            string name,
            Vector3 localPosition,
            Vector3 localScale,
            Material material,
            Vector3? localEulerAngles = null)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent, false);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localEulerAngles = localEulerAngles ?? Vector3.zero;
            primitive.transform.localScale = localScale;
            if (primitive.TryGetComponent(out Renderer renderer))
            {
                renderer.sharedMaterial = material;
            }

            if (primitive.TryGetComponent(out Collider collider))
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }

            return primitive;
        }

        private static Light CreateLight(
            Transform parent,
            string name,
            LightType lightType,
            Vector3 localPosition,
            Color color,
            float intensity,
            float range)
        {
            Transform lightTransform = CreateChild(parent, name);
            lightTransform.localPosition = localPosition;
            Light light = lightTransform.gameObject.AddComponent<Light>();
            light.type = lightType;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.shadows = LightShadows.None;
            return light;
        }

        private static ReviewMaterials EnsureMaterials()
        {
            Shader litShader = Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("Standard")
                ?? throw new InvalidOperationException("Could not find a lit shader for the review diorama.");

            return new ReviewMaterials(
                EnsureMaterial("M_ReviewCorridorFloor", litShader, new Color(0.035f, 0.055f, 0.075f), Color.black, 0.62f),
                EnsureMaterial("M_ReviewCorridorStructure", litShader, new Color(0.075f, 0.09f, 0.12f), Color.black, 0.45f),
                EnsureMaterial("M_ReviewCorridorFrame", litShader, new Color(0.12f, 0.16f, 0.21f), Color.black, 0.68f),
                EnsureMaterial("M_ReviewGateDoor", litShader, new Color(0.055f, 0.075f, 0.105f), Color.black, 0.76f),
                EnsureMaterial("M_ReviewSignalCyan", litShader, new Color(0.03f, 0.22f, 0.28f), new Color(0.10f, 1.05f, 1.55f), 0.36f),
                EnsureMaterial("M_ReviewSignalWarning", litShader, new Color(0.30f, 0.10f, 0.025f), new Color(1.75f, 0.35f, 0.04f), 0.40f));
        }

        private static Sprite EnsureReviewBackgroundSprite()
        {
            if (AssetDatabase.LoadAssetAtPath<Texture2D>(BackgroundArtPath) == null)
            {
                AssetDatabase.ImportAsset(BackgroundArtPath, ImportAssetOptions.ForceSynchronousImport);
            }

            TextureImporter importer = AssetImporter.GetAtPath(BackgroundArtPath) as TextureImporter;
            if (importer == null)
            {
                throw new InvalidOperationException(
                    $"Review background `{BackgroundArtPath}` has no TextureImporter.");
            }

            bool changed = false;
            changed |= SetIfDifferent(importer.textureType, TextureImporterType.Sprite, value => importer.textureType = value);
            changed |= SetIfDifferent(importer.spriteImportMode, SpriteImportMode.Single, value => importer.spriteImportMode = value);
            changed |= SetIfDifferent(importer.sRGBTexture, true, value => importer.sRGBTexture = value);
            changed |= SetIfDifferent(importer.wrapMode, TextureWrapMode.Clamp, value => importer.wrapMode = value);
            changed |= SetIfDifferent(importer.mipmapEnabled, false, value => importer.mipmapEnabled = value);
            changed |= SetIfDifferent(importer.filterMode, FilterMode.Bilinear, value => importer.filterMode = value);
            changed |= SetIfDifferent(importer.alphaIsTransparency, false, value => importer.alphaIsTransparency = value);
            if (!Mathf.Approximately(importer.spritePixelsPerUnit, 100f))
            {
                importer.spritePixelsPerUnit = 100f;
                changed = true;
            }

            if (importer.maxTextureSize != 2048)
            {
                importer.maxTextureSize = 2048;
                changed = true;
            }

            if (changed)
            {
                importer.SaveAndReimport();
            }

            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(BackgroundArtPath);
            if (sprite == null)
            {
                throw new InvalidOperationException(
                    $"Could not load `{BackgroundArtPath}` as a Sprite after import setup.");
            }

            return sprite;
        }

        private static bool SetIfDifferent<T>(T current, T desired, Action<T> setter)
        {
            if (EqualityComparer<T>.Default.Equals(current, desired))
            {
                return false;
            }

            setter(desired);
            return true;
        }

        private static Material EnsureMaterial(
            string assetName,
            Shader shader,
            Color baseColor,
            Color emissionColor,
            float smoothness)
        {
            string path = $"{MaterialRoot}/{assetName}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader) { name = assetName };
                AssetDatabase.CreateAsset(material, path);
            }
            else if (material.shader != shader)
            {
                material.shader = shader;
            }

            SetMaterialColor(material, "_BaseColor", baseColor);
            SetMaterialColor(material, "_Color", baseColor);
            SetMaterialFloat(material, "_Smoothness", smoothness);
            SetMaterialFloat(material, "_Glossiness", smoothness);
            if (emissionColor.maxColorComponent > 0.001f)
            {
                material.EnableKeyword("_EMISSION");
                SetMaterialColor(material, "_EmissionColor", emissionColor);
            }
            else
            {
                material.DisableKeyword("_EMISSION");
                SetMaterialColor(material, "_EmissionColor", Color.black);
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static void SetMaterialColor(Material material, string propertyName, Color value)
        {
            if (material.HasProperty(propertyName))
            {
                material.SetColor(propertyName, value);
            }
        }

        private static void SetMaterialFloat(Material material, string propertyName, float value)
        {
            if (material.HasProperty(propertyName))
            {
                material.SetFloat(propertyName, value);
            }
        }

        private static Keyframe Key(float time, float value)
        {
            return new Keyframe(time, value);
        }

        private static AnimationCurve SmoothCurve(params Keyframe[] keys)
        {
            AnimationCurve curve = new AnimationCurve(keys);
            for (int i = 0; i < curve.length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Auto);
                AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Auto);
            }

            return curve;
        }

        private static void SetCurve(AnimationClip clip, string propertyName, AnimationCurve curve)
        {
            AnimationUtility.SetEditorCurve(
                clip,
                EditorCurveBinding.FloatCurve(string.Empty, typeof(Transform), propertyName),
                curve);
        }

        private static List<string> ValidateGeneratedReview(Scene scene)
        {
            var issues = new List<string>();
            if (!scene.IsValid() || !scene.isLoaded)
            {
                issues.Add("Review scene is not loaded.");
                return issues;
            }

            if (!string.Equals(scene.path, ScenePath, StringComparison.Ordinal))
            {
                issues.Add($"Loaded scene path is `{scene.path}` instead of `{ScenePath}`.");
            }

            if (EditorBuildSettings.scenes.Any(
                    entry => string.Equals(entry.path, ScenePath, StringComparison.Ordinal)))
            {
                issues.Add("Review scene must remain entirely outside build settings.");
            }

            NarrativeSequenceProfile profile =
                AssetDatabase.LoadAssetAtPath<NarrativeSequenceProfile>(NarrativeProfilePath);
            if (profile == null)
            {
                issues.Add("Narrative profile is missing.");
            }
            else
            {
                if (!profile.TryValidate(out string validationError))
                {
                    issues.Add("Narrative profile validation failed: " + validationError);
                }

                if (!string.Equals(profile.SequenceId, SequenceId, StringComparison.Ordinal))
                {
                    issues.Add($"Narrative sequence id must be `{SequenceId}`.");
                }

                if (profile.LineCount != 8)
                {
                    issues.Add($"Narrative profile must contain exactly 8 lines; found {profile.LineCount}.");
                }

                AudioClip expectedVoice =
                    AssetDatabase.LoadAssetAtPath<AudioClip>(SignalWarningVoicePath);
                NarrativeSequenceProfile.LineEntry signalWarningLine = profile.GetLine(2);
                if (expectedVoice == null || signalWarningLine?.VoiceClip != expectedVoice)
                {
                    issues.Add(
                        "Narrative line 03 must bind the reviewed temporary operator signal-warning voice clip.");
                }

                int choiceLineCount = profile.Lines.Count(line => line != null && line.HasChoices);
                int choiceCount = profile.Lines.Sum(line => line?.Choices.Length ?? 0);
                if (choiceLineCount != 1 || choiceCount != 2)
                {
                    issues.Add(
                        $"Narrative profile must have one two-choice rejoin; found {choiceLineCount} choice lines and {choiceCount} choices.");
                }

                NarrativeSequenceProfile.ChoiceEntry[] choices = profile.Lines
                    .Where(line => line != null && line.HasChoices)
                    .SelectMany(line => line.Choices)
                    .ToArray();
                if (choices.Any(choice => choice == null
                    || !choice.HasResponse
                    || string.IsNullOrWhiteSpace(choice.ResponseLineId)))
                {
                    issues.Add("Both review choices must author a local response before rejoining the next profile line.");
                }

                var requiredSpeakers = new HashSet<string>(
                    new[] { "system", "field_agent", "operator" },
                    StringComparer.Ordinal);
                foreach (NarrativeSequenceProfile.LineEntry line in profile.Lines)
                {
                    if (line != null)
                    {
                        requiredSpeakers.Remove(line.SpeakerId);
                    }
                }

                if (requiredSpeakers.Count > 0)
                {
                    issues.Add("Narrative profile is missing speakers: " + string.Join(", ", requiredSpeakers));
                }
            }

            TimelineAsset timeline = AssetDatabase.LoadAssetAtPath<TimelineAsset>(TimelinePath);
            if (timeline == null)
            {
                issues.Add("Review tutorial Timeline is missing.");
            }

            PlayableDirector[] directors = FindComponentsInScene<PlayableDirector>(scene);
            if (directors.Length != 1)
            {
                issues.Add($"Review scene must contain exactly one PlayableDirector; found {directors.Length}.");
            }

            PlayableDirector director = directors.Length == 1 ? directors[0] : null;
            if (director != null && director.playableAsset != timeline)
            {
                issues.Add("PlayableDirector is not assigned to the review tutorial Timeline.");
            }

            if (director != null
                && (director.playOnAwake
                    || director.extrapolationMode != DirectorWrapMode.None
                    || director.timeUpdateMode != DirectorUpdateMode.GameTime))
            {
                issues.Add("Review PlayableDirector must be stopped-on-load, non-looping, and GameTime driven.");
            }

            if (timeline != null && director != null)
            {
                AnimationTrack cameraTrack = FindTrack<AnimationTrack>(timeline, CameraTrackName);
                IntroGatePodDialogueTrack dialogueTrack =
                    FindTrack<IntroGatePodDialogueTrack>(timeline, DialogueTrackName);
                if (cameraTrack == null)
                {
                    issues.Add("Timeline is missing its review camera animation track.");
                }
                else
                {
                    Animator boundAnimator = director.GetGenericBinding(cameraTrack) as Animator;
                    if (boundAnimator == null || !string.Equals(
                            boundAnimator.gameObject.name,
                            "ReviewCutsceneCamera",
                            StringComparison.Ordinal))
                    {
                        issues.Add("Review camera track is not bound to ReviewCutsceneCamera's Animator.");
                    }

                    if (!cameraTrack.GetClips().Any())
                    {
                        issues.Add("Review camera track has no animation clip.");
                    }
                }

                if (dialogueTrack == null)
                {
                    issues.Add("Timeline is missing its IntroGatePodDialogueTrack.");
                }
                else
                {
                    IntroGatePodDialogueOverlay overlay =
                        director.GetGenericBinding(dialogueTrack) as IntroGatePodDialogueOverlay;
                    if (overlay == null || !overlay.HasBindings)
                    {
                        issues.Add("Dialogue track is not bound to a complete review subtitle overlay.");
                    }

                    if (dialogueTrack.GetClips().Count() != 3)
                    {
                        issues.Add("Review dialogue track must contain exactly three subtitle clips.");
                    }
                }
            }

            StageCutscenePort[] ports = FindComponentsInScene<StageCutscenePort>(scene);
            if (ports.Length != 1)
            {
                issues.Add($"Review scene must contain exactly one StageCutscenePort; found {ports.Length}.");
            }
            else
            {
                StageCutscenePort port = ports[0];
                if (!string.Equals(
                        port.PortId,
                        "review.olympus.prologue.gate_signal.cutscene",
                        StringComparison.Ordinal)
                    || port.PortKind != StageCutscenePortKind.Intro
                    || !string.Equals(
                        port.HandoffId,
                        "review.olympus.prologue.to_briefing",
                        StringComparison.Ordinal)
                    || !string.Equals(
                        port.AnchorId,
                        "review.olympus.gate.diorama",
                        StringComparison.Ordinal)
                    || !string.Equals(
                        port.RuntimeStateId,
                        "review.narrative.tutorial_cutscene",
                        StringComparison.Ordinal))
                {
                    issues.Add("StageCutscenePort review metadata is incomplete or stale.");
                }

                if (!port.HasPayloadRoot
                    || port.RuntimeDirector != director
                    || port.PresentationProfile != null)
                {
                    issues.Add("StageCutscenePort review payload/director/profile binding is stale.");
                }

                if (director != null
                    && (port.gameObject != director.gameObject
                        || port.PayloadRoot == null
                        || !string.Equals(
                            port.PayloadRoot.gameObject.scene.path,
                            ScenePath,
                            StringComparison.Ordinal)))
                {
                    issues.Add("Review port, Director, and payload must remain local to the review flow/scene.");
                }
            }

            OlympusChapterNarrativeReviewController[] controllers =
                FindComponentsInScene<OlympusChapterNarrativeReviewController>(scene);
            if (controllers.Length != 1)
            {
                issues.Add(
                    $"Review scene must contain exactly one OlympusChapterNarrativeReviewController; found {controllers.Length}.");
            }
            else
            {
                ValidateControllerReferences(
                    controllers[0],
                    profile,
                    director,
                    ports.Length == 1 ? ports[0] : null,
                    issues);
            }

            Canvas[] canvases = FindComponentsInScene<Canvas>(scene);
            if (canvases.Length != 1)
            {
                issues.Add($"Review scene must contain exactly one UI Canvas; found {canvases.Length}.");
            }
            else
            {
                CanvasScaler scaler = canvases[0].GetComponent<CanvasScaler>();
                if (scaler == null
                    || scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize
                    || scaler.referenceResolution != new Vector2(1920f, 1080f)
                    || scaler.screenMatchMode != CanvasScaler.ScreenMatchMode.MatchWidthOrHeight
                    || scaler.matchWidthOrHeight < 0f
                    || scaler.matchWidthOrHeight > 1f)
                {
                    issues.Add("Review CanvasScaler is not safe for a 1920x1080 mobile landscape reference.");
                }
            }

            TextureImporter backgroundImporter =
                AssetImporter.GetAtPath(BackgroundArtPath) as TextureImporter;
            if (backgroundImporter == null
                || backgroundImporter.textureType != TextureImporterType.Sprite
                || backgroundImporter.spriteImportMode != SpriteImportMode.Single
                || !backgroundImporter.sRGBTexture
                || backgroundImporter.wrapMode != TextureWrapMode.Clamp)
            {
                issues.Add("Review background must import as a single sRGB Sprite using clamp wrap mode.");
            }

            Sprite backgroundSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BackgroundArtPath);
            Image[] images = FindComponentsInScene<Image>(scene);
            Image chapterBackground = images.FirstOrDefault(
                image => string.Equals(image.gameObject.name, "ChapterKeyVisual", StringComparison.Ordinal));
            Image narrativeBackground = images.FirstOrDefault(
                image => string.Equals(image.gameObject.name, "VisualNovelKeyVisual", StringComparison.Ordinal));
            if (backgroundSprite == null
                || chapterBackground == null
                || chapterBackground.sprite != backgroundSprite
                || narrativeBackground == null
                || narrativeBackground.sprite != backgroundSprite)
            {
                issues.Add("Chapter and Visual Novel groups must both use the promoted review background Sprite.");
            }

            EventSystem[] eventSystems = FindComponentsInScene<EventSystem>(scene);
            if (eventSystems.Length != 1
                || eventSystems[0].GetComponent<InputSystemUIInputModule>() == null)
            {
                issues.Add("Review scene needs one EventSystem using InputSystemUIInputModule.");
            }

            Camera[] cameras = FindComponentsInScene<Camera>(scene);
            Camera gameplayCamera = cameras.FirstOrDefault(
                candidate => string.Equals(
                    candidate.gameObject.name,
                    "ReviewCutsceneCamera",
                    StringComparison.Ordinal));
            Camera narrativeCamera = cameras.FirstOrDefault(
                candidate => string.Equals(
                    candidate.gameObject.name,
                    "ReviewNarrativePresentationCamera",
                    StringComparison.Ordinal));
            if (cameras.Length != 2
                || gameplayCamera == null
                || narrativeCamera == null
                || !string.Equals(gameplayCamera.tag, "MainCamera", StringComparison.Ordinal)
                || !gameplayCamera.enabled
                || !string.Equals(narrativeCamera.tag, "Untagged", StringComparison.Ordinal)
                || narrativeCamera.enabled)
            {
                issues.Add(
                    "Review scene needs one enabled gameplay MainCamera and one disabled narrative presentation camera.");
            }

            AudioListener[] listeners = FindComponentsInScene<AudioListener>(scene);
            if (listeners.Length != 2
                || gameplayCamera == null
                || narrativeCamera == null
                || gameplayCamera.GetComponent<AudioListener>() == null
                || !gameplayCamera.GetComponent<AudioListener>().enabled
                || narrativeCamera.GetComponent<AudioListener>() == null
                || narrativeCamera.GetComponent<AudioListener>().enabled)
            {
                issues.Add(
                    "Review cameras need distinct gameplay/narrative listeners with exact initial enabled states.");
            }

            Light[] reviewLights = FindComponentsInScene<Light>(scene);
            if (reviewLights.Length != 2
                || reviewLights.Any(light => light.shadows != LightShadows.None))
            {
                issues.Add(
                    "Review diorama must contain exactly two shadow-free lights; realtime shadows "
                    + "are outside this UI transition lab's graphics budget.");
            }

            OlympusStoryTutorialTransitionReviewGate[] transitionGates =
                FindComponentsInScene<OlympusStoryTutorialTransitionReviewGate>(scene);
            ReviewTutorialStartProbe[] tutorialStartProbes =
                FindComponentsInScene<ReviewTutorialStartProbe>(scene);
            ReviewGameplayInputProbe[] gameplayInputProbes =
                FindComponentsInScene<ReviewGameplayInputProbe>(scene);
            if (transitionGates.Length != 1
                || tutorialStartProbes.Length != 1
                || gameplayInputProbes.Length != 1)
            {
                issues.Add(
                    "Review scene needs exactly one story transition gate, tutorial-start probe, and gameplay-input probe.");
            }
            else
            {
                OlympusStoryTutorialTransitionReviewGate gate = transitionGates[0];
                if (!gate.HasValidBindings
                    || gate.GameplayCamera != gameplayCamera
                    || gate.NarrativePresentationCamera != narrativeCamera
                    || gate.GameplayListener
                        != (gameplayCamera != null
                            ? gameplayCamera.GetComponent<AudioListener>()
                            : null)
                    || gate.NarrativePresentationListener
                        != (narrativeCamera != null
                            ? narrativeCamera.GetComponent<AudioListener>()
                            : null)
                    || gate.GameplayInput != gameplayInputProbes[0]
                    || gate.TutorialStartProbe != tutorialStartProbes[0]
                    || gate.GameplayHud == null
                    || !string.Equals(
                        gate.GameplayHud.gameObject.name,
                        "ReviewGameplayHudProbe",
                        StringComparison.Ordinal))
                {
                    issues.Add("Story transition gate direct bindings are missing, indirect, or stale.");
                }
            }

            Button[] reviewButtons = FindComponentsInScene<Button>(scene);
            if (reviewButtons.Any(button => button.onClick.GetPersistentEventCount() != 0))
            {
                issues.Add("Review buttons must not contain serialized route or runtime callbacks.");
            }

            var allowedDimensionBrawlBehaviourTypes = new HashSet<string>(
                new[]
                {
                    "DimensionBrawl.LevelDesign.StageCutscenePort",
                    "DimensionBrawl.Presentation.IntroGatePodDialogueOverlay",
                    "DimensionBrawl.UI.NarrativeReview.OlympusChapterNarrativeReviewController",
                    "DimensionBrawl.UI.NarrativeReview.OlympusStoryTutorialTransitionReviewGate",
                    "DimensionBrawl.UI.NarrativeReview.ReviewGameplayInputProbe",
                    "DimensionBrawl.UI.NarrativeReview.ReviewTutorialStartProbe"
                },
                StringComparer.Ordinal);
            MonoBehaviour[] behaviours = FindComponentsInScene<MonoBehaviour>(scene);
            string[] unexpectedProductBehaviours = behaviours
                .Where(component => component != null)
                .Select(component => component.GetType().FullName)
                .Where(typeName =>
                    !string.IsNullOrWhiteSpace(typeName)
                    && typeName.StartsWith("DimensionBrawl.", StringComparison.Ordinal)
                    && !allowedDimensionBrawlBehaviourTypes.Contains(typeName))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(typeName => typeName, StringComparer.Ordinal)
                .ToArray();
            if (unexpectedProductBehaviours.Length > 0)
            {
                issues.Add(
                    "Review scene contains non-allowlisted DimensionBrawl runtime owners: "
                    + string.Join(", ", unexpectedProductBehaviours));
            }

            return issues;
        }

        private static void ValidateControllerReferences(
            OlympusChapterNarrativeReviewController controller,
            NarrativeSequenceProfile profile,
            PlayableDirector director,
            StageCutscenePort cutscenePort,
            List<string> issues)
        {
            SerializedObject serialized = new SerializedObject(controller);
            ValidateObjectReference(serialized, "narrativeProfile", profile, issues);
            ValidateObjectReference(
                serialized,
                "stageCatalog",
                AssetDatabase.LoadAssetAtPath<UIStageCatalog>(StageCatalogPath),
                issues);
            ValidateObjectReference(serialized, "cutsceneDirector", director, issues);
            ValidateObjectReference(serialized, "cutscenePort", cutscenePort, issues);

            OlympusStoryTutorialTransitionReviewGate transitionGate =
                controller.GetComponent<OlympusStoryTutorialTransitionReviewGate>();
            ValidateObjectReference(
                serialized,
                "storyTutorialTransitionGate",
                transitionGate,
                issues);

            string[] requiredReferences =
            {
                "chapterEntryGroup",
                "visualNovelGroup",
                "cutsceneControlsGroup",
                "stageBriefingGroup",
                "completeGroup",
                "chapterEnterButton",
                "narrativeLineText",
                "narrativeNextButton",
                "narrativeAutoButton",
                "narrativeSkipButton",
                "narrativeLogButton",
                "narrativeChoiceGroup",
                "firstChoiceButton",
                "secondChoiceButton",
                "cutsceneSkipButton",
                "briefingRewardRow",
                "briefingCompleteButton",
                "restartButton",
                "logGroup",
                "skipConfirmGroup"
            };
            for (int i = 0; i < requiredReferences.Length; i++)
            {
                SerializedProperty property = serialized.FindProperty(requiredReferences[i]);
                if (property == null || property.objectReferenceValue == null)
                {
                    issues.Add($"Review controller is missing `{requiredReferences[i]}`.");
                }
            }
        }

        private static void ValidateObjectReference(
            SerializedObject serialized,
            string propertyName,
            UnityEngine.Object expected,
            List<string> issues)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property == null || property.objectReferenceValue != expected)
            {
                issues.Add($"Review controller `{propertyName}` is missing or stale.");
            }
        }

        private static T FindTrack<T>(TimelineAsset timeline, string trackName) where T : TrackAsset
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

        private static T[] FindComponentsInScene<T>(Scene scene) where T : Component
        {
            var results = new List<T>();
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                results.AddRange(root.GetComponentsInChildren<T>(includeInactive: true));
            }

            return results.ToArray();
        }

        private static Dictionary<string, DateTime?> CaptureCanonicalTimestamps()
        {
            var timestamps = new Dictionary<string, DateTime?>(StringComparer.Ordinal);
            for (int i = 0; i < CanonicalAssetsThatMustRemainUntouched.Length; i++)
            {
                string assetPath = CanonicalAssetsThatMustRemainUntouched[i];
                CaptureTimestamp(assetPath, timestamps);
                CaptureTimestamp(assetPath + ".meta", timestamps);
            }

            return timestamps;
        }

        private static void CaptureTimestamp(
            string assetPath,
            Dictionary<string, DateTime?> timestamps)
        {
            string absolutePath = AssetPathToAbsolutePath(assetPath);
            timestamps[assetPath] = File.Exists(absolutePath)
                ? File.GetLastWriteTimeUtc(absolutePath)
                : (DateTime?)null;
        }

        private static void AppendCanonicalTimestampIssues(
            Dictionary<string, DateTime?> before,
            List<string> issues)
        {
            foreach (KeyValuePair<string, DateTime?> pair in before)
            {
                string absolutePath = AssetPathToAbsolutePath(pair.Key);
                DateTime? after = File.Exists(absolutePath)
                    ? File.GetLastWriteTimeUtc(absolutePath)
                    : (DateTime?)null;
                if (pair.Value != after)
                {
                    issues.Add($"Canonical asset timestamp changed unexpectedly: `{pair.Key}`.");
                }
            }
        }

        private static string AssetPathToAbsolutePath(string assetPath)
        {
            return Path.GetFullPath(
                Path.Combine(
                    Directory.GetCurrentDirectory(),
                    assetPath.Replace('/', Path.DirectorySeparatorChar)));
        }

        private static void EnsureAssetFolder(string folderPath)
        {
            string normalized = folderPath.Replace('\\', '/').TrimEnd('/');
            if (AssetDatabase.IsValidFolder(normalized))
            {
                return;
            }

            string parent = PathParent(normalized);
            EnsureAssetFolder(parent);
            string folderName = normalized.Substring(parent.Length + 1);
            AssetDatabase.CreateFolder(parent, folderName);
        }

        private static string PathParent(string assetPath)
        {
            int slash = assetPath.LastIndexOf('/');
            if (slash <= 0)
            {
                throw new InvalidOperationException($"Asset path has no parent folder: `{assetPath}`.");
            }

            return assetPath.Substring(0, slash);
        }

        private static T LoadRequired<T>(string assetPath) where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset == null)
            {
                throw new InvalidOperationException($"Missing required asset `{assetPath}`.");
            }

            return asset;
        }

        private readonly struct TimelineRefs
        {
            public TimelineRefs(AnimationTrack cameraTrack, IntroGatePodDialogueTrack dialogueTrack)
            {
                CameraTrack = cameraTrack;
                DialogueTrack = dialogueTrack;
            }

            public AnimationTrack CameraTrack { get; }
            public IntroGatePodDialogueTrack DialogueTrack { get; }
        }

        private readonly struct DioramaRefs
        {
            public DioramaRefs(
                Transform payloadRoot,
                Camera camera,
                AudioListener listener,
                Animator cameraAnimator,
                Camera narrativeCamera,
                AudioListener narrativeListener)
            {
                PayloadRoot = payloadRoot;
                Camera = camera;
                Listener = listener;
                CameraAnimator = cameraAnimator;
                NarrativeCamera = narrativeCamera;
                NarrativeListener = narrativeListener;
            }

            public Transform PayloadRoot { get; }
            public Camera Camera { get; }
            public AudioListener Listener { get; }
            public Animator CameraAnimator { get; }
            public Camera NarrativeCamera { get; }
            public AudioListener NarrativeListener { get; }
        }

        private readonly struct PortraitRefs
        {
            public PortraitRefs(CanvasGroup group, Image body)
            {
                Group = group;
                Body = body;
            }

            public CanvasGroup Group { get; }
            public Image Body { get; }
        }

        private readonly struct ReviewMaterials
        {
            public ReviewMaterials(
                Material floor,
                Material structure,
                Material frame,
                Material door,
                Material emissive,
                Material warning)
            {
                Floor = floor;
                Structure = structure;
                Frame = frame;
                Door = door;
                Emissive = emissive;
                Warning = warning;
            }

            public Material Floor { get; }
            public Material Structure { get; }
            public Material Frame { get; }
            public Material Door { get; }
            public Material Emissive { get; }
            public Material Warning { get; }

            public void SaveIfDirty()
            {
                AssetDatabase.SaveAssetIfDirty(Floor);
                AssetDatabase.SaveAssetIfDirty(Structure);
                AssetDatabase.SaveAssetIfDirty(Frame);
                AssetDatabase.SaveAssetIfDirty(Door);
                AssetDatabase.SaveAssetIfDirty(Emissive);
                AssetDatabase.SaveAssetIfDirty(Warning);
            }
        }

        private sealed class ReviewUiRefs
        {
            public Canvas Canvas;
            public CanvasScaler CanvasScaler;

            public CanvasGroup ChapterEntryGroup;
            public CanvasGroup VisualNovelGroup;
            public CanvasGroup CutsceneControlsGroup;
            public CanvasGroup StageBriefingGroup;
            public CanvasGroup CompleteGroup;
            public CanvasGroup GameplayHudProbeGroup;

            public TMP_Text ChapterEyebrow;
            public TMP_Text ChapterTitle;
            public TMP_Text ChapterStageTitle;
            public TMP_Text ChapterObjective;
            public TMP_Text ChapterStatus;
            public Button ChapterEnterButton;

            public TMP_Text NarrativeSequence;
            public TMP_Text NarrativeSpeaker;
            public TMP_Text NarrativeLine;
            public TMP_Text NarrativeProgress;
            public CanvasGroup LeftPortraitGroup;
            public CanvasGroup CenterPortraitGroup;
            public CanvasGroup RightPortraitGroup;
            public Image LeftPortraitImage;
            public Image CenterPortraitImage;
            public Image RightPortraitImage;
            public Button NarrativeNextButton;
            public Button NarrativeAutoButton;
            public TMP_Text NarrativeAutoButtonText;
            public Button NarrativeSkipButton;
            public Button NarrativeLogButton;
            public CanvasGroup NarrativeChoiceGroup;
            public Button FirstChoiceButton;
            public TMP_Text FirstChoiceText;
            public Button SecondChoiceButton;
            public TMP_Text SecondChoiceText;

            public TMP_Text CutsceneLabel;
            public TMP_Text CutsceneProgress;
            public Button CutsceneSkipButton;
            public IntroGatePodDialogueOverlay CutsceneDialogueOverlay;

            public TMP_Text BriefingTitle;
            public TMP_Text BriefingObjective;
            public TMP_Text BriefingCombatLesson;
            public TMP_Text BriefingThreat;
            public TMP_Text BriefingSummon;
            public TMP_Text BriefingDuration;
            public GameObject BriefingRewardRow;
            public TMP_Text BriefingReward;
            public TMP_Text BriefingDigest;
            public TMP_Text BriefingStatus;
            public Button BriefingCompleteButton;

            public TMP_Text CompleteTitle;
            public TMP_Text CompleteSummary;
            public Button RestartButton;

            public CanvasGroup LogGroup;
            public TMP_Text LogText;
            public Button LogCloseButton;
            public CanvasGroup SkipConfirmGroup;
            public Button SkipConfirmButton;
            public Button SkipCancelButton;
        }
    }
}
