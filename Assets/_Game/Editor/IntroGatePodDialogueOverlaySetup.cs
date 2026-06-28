using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DimensionBrawl.Presentation;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.Timeline;
using UnityEngine.UI;

namespace DimensionBrawl.Editor
{
    public static class IntroGatePodDialogueOverlaySetup
    {
        private const string OlympusStageScenePath = "Assets/_Game/Scenes/OlympusCorridorInvasionStage.unity";
        private const string OlympusCombinedTimelinePath =
            "Assets/_Game/DesignData/Timelines/Cinematics/DB_Timeline_IntroGatePodAwakening_OlympusBombingPrelude.playable";
        private const string OverlayObjectName = "IntroGatePodReview_DialogueOverlay";
        private const string DialogueTrackName = "Bombing Prelude Dialogue";
        private const string ReportPath = "C:/tmp/DimensionBrawl-IntroGatePodDialogueOverlaySetup.md";
        private const string PretendardMediumSourceFontPath = "Assets/_Game/Art/Fonts/Pretendard/Pretendard-Medium.otf";
        private const string PretendardSemiBoldSourceFontPath = "Assets/_Game/Art/Fonts/Pretendard/Pretendard-SemiBold.otf";
        private const string DialogueFontAssetPath = "Assets/_Game/Art/Fonts/Pretendard/TMP_Pretendard_Medium_Dynamic.asset";
        private const string SpeakerFontAssetPath = "Assets/_Game/Art/Fonts/Pretendard/TMP_Pretendard_SemiBold_Dynamic.asset";
        private const string KoreanProbeText = "\uD55C\uAE00 \uD14C\uC2A4\uD2B8 \uD654\uC790 \uB300\uC0AC";

        [MenuItem("Tools/DimensionBrawl/Intro GatePod/Setup Dialogue Overlay")]
        public static void SetupDialogueOverlayMenu()
        {
            SetupDialogueOverlay(writeReport: true);
        }

        public static void RunBatchSetupDialogueOverlay()
        {
            SetupDialogueOverlay(writeReport: true);
        }

        private static void SetupDialogueOverlay(bool writeReport)
        {
            AssetDatabase.Refresh();
            Scene scene = EditorSceneManager.OpenScene(OlympusStageScenePath, OpenSceneMode.Single);
            TimelineAsset timeline = LoadRequired<TimelineAsset>(OlympusCombinedTimelinePath);
            PlayableDirector director = FindDirectorBoundToTimeline(scene, timeline)
                ?? throw new InvalidOperationException("Could not find the Olympus intro PlayableDirector bound to the combined Timeline.");

            TMP_FontAsset speakerFont = EnsureDialogueFontAsset(
                PretendardSemiBoldSourceFontPath,
                SpeakerFontAssetPath,
                "TMP_Pretendard_SemiBold_Dynamic");
            TMP_FontAsset dialogueFont = EnsureDialogueFontAsset(
                PretendardMediumSourceFontPath,
                DialogueFontAssetPath,
                "TMP_Pretendard_Medium_Dynamic");

            IntroGatePodDialogueOverlay overlay = EnsureDialogueOverlay(scene, speakerFont, dialogueFont);
            IntroGatePodDialogueTrack dialogueTrack = EnsureDialogueTrack(timeline);
            director.SetGenericBinding(dialogueTrack, overlay);
            int createdClipCount = EnsureDialogueClips(timeline, dialogueTrack);

            EditorUtility.SetDirty(overlay);
            EditorUtility.SetDirty(dialogueTrack);
            EditorUtility.SetDirty(timeline);
            EditorUtility.SetDirty(director);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            List<string> issues = ValidateSetup(scene, timeline, director);
            if (writeReport)
            {
                WriteReport(issues, createdClipCount, CountBombingVoiceAudioClips(timeline));
            }

            if (issues.Count > 0)
            {
                throw new InvalidOperationException(
                    "Intro GatePod dialogue overlay setup failed:\n" + string.Join("\n", issues));
            }
        }

        private static IntroGatePodDialogueOverlay EnsureDialogueOverlay(
            Scene scene,
            TMP_FontAsset speakerFont,
            TMP_FontAsset dialogueFont)
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
                    typeof(IntroGatePodDialogueOverlay));
                SceneManager.MoveGameObjectToScene(overlayObject, scene);
            }

            RectTransform overlayRect = overlayObject.GetComponent<RectTransform>();
            StretchFullScreen(overlayRect);

            Canvas canvas = overlayObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 850;

            CanvasScaler scaler = overlayObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            GraphicRaycaster raycaster = overlayObject.GetComponent<GraphicRaycaster>();
            raycaster.enabled = false;

            CanvasGroup canvasGroup = overlayObject.GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            RectTransform root = EnsureChild(overlayRect, "DialogueRoot");
            ConfigureAnchored(root, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(1180f, 132f), new Vector2(0f, 96f));

            Image backplate = EnsureImage(root, "Backplate");
            backplate.color = new Color(0.015f, 0.025f, 0.045f, 0.44f);
            backplate.raycastTarget = false;
            StretchFullScreen(backplate.rectTransform);

            Image topLine = EnsureImage(root, "TopSignalLine");
            topLine.color = new Color(0.20f, 0.56f, 1.00f, 0.72f);
            topLine.raycastTarget = false;
            ConfigureAnchored(topLine.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(860f, 3f), new Vector2(0f, -18f));

            Image centerNotch = EnsureImage(root, "CenterNotch");
            centerNotch.color = new Color(0.82f, 0.92f, 1.00f, 0.88f);
            centerNotch.raycastTarget = false;
            ConfigureAnchored(centerNotch.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(92f, 5f), new Vector2(0f, -16f));

            TMP_Text speakerText = EnsureText(root, "SpeakerText", 21f, TextAlignmentOptions.Center);
            speakerText.color = new Color(1.00f, 0.86f, 0.48f, 1f);
            speakerText.font = speakerFont;
            speakerText.fontStyle = FontStyles.Normal;
            ConfigureAnchored(speakerText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(320f, 30f), new Vector2(0f, -30f));

            TMP_Text lineText = EnsureText(root, "LineText", 28f, TextAlignmentOptions.Center);
            lineText.color = new Color(0.94f, 0.97f, 1.00f, 1f);
            lineText.font = dialogueFont;
            lineText.fontStyle = FontStyles.Normal;
            lineText.enableWordWrapping = true;
            lineText.overflowMode = TextOverflowModes.Ellipsis;
            ConfigureAnchored(lineText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(1080f, 74f), new Vector2(0f, -62f));

            IntroGatePodDialogueOverlay overlay = overlayObject.GetComponent<IntroGatePodDialogueOverlay>();
            overlay.Configure(canvasGroup, speakerText, lineText);

            EditorUtility.SetDirty(overlayObject);
            return overlay;
        }

        private static TMP_FontAsset EnsureDialogueFontAsset(string sourceFontPath, string fontAssetPath, string assetName)
        {
            if (!File.Exists(AssetPathToAbsolutePath(sourceFontPath)))
            {
                throw new FileNotFoundException("Missing imported Pretendard source font.", sourceFontPath);
            }

            AssetDatabase.ImportAsset(sourceFontPath, ImportAssetOptions.ForceUpdate);
            Font sourceFont = AssetDatabase.LoadAssetAtPath<Font>(sourceFontPath);
            if (sourceFont == null)
            {
                throw new InvalidOperationException($"Could not import Pretendard source font `{sourceFontPath}`.");
            }

            TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontAssetPath);
            if (fontAsset != null && !IsUsableDialogueFontAsset(fontAsset))
            {
                AssetDatabase.DeleteAsset(fontAssetPath);
                AssetDatabase.Refresh();
                fontAsset = null;
            }

            if (fontAsset == null)
            {
                fontAsset = TMP_FontAsset.CreateFontAsset(
                    sourceFont,
                    90,
                    9,
                    GlyphRenderMode.SDFAA,
                    2048,
                    2048,
                    AtlasPopulationMode.Dynamic,
                    enableMultiAtlasSupport: true);
                fontAsset.name = assetName;
                AssetDatabase.CreateAsset(fontAsset, fontAssetPath);
                PersistFontAssetSubAssets(fontAsset, assetName);
            }

            fontAsset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            if (!fontAsset.TryAddCharacters(KoreanProbeText, out string missingCharacters))
            {
                throw new InvalidOperationException(
                    $"Pretendard TMP font asset could not render the Korean probe text. Missing: `{missingCharacters}`.");
            }

            EditorUtility.SetDirty(fontAsset);
            if (fontAsset.atlasTextures != null)
            {
                for (int i = 0; i < fontAsset.atlasTextures.Length; i++)
                {
                    if (fontAsset.atlasTextures[i] != null)
                    {
                        EditorUtility.SetDirty(fontAsset.atlasTextures[i]);
                    }
                }
            }

            if (fontAsset.material != null)
            {
                EditorUtility.SetDirty(fontAsset.material);
            }

            AssetDatabase.SaveAssets();
            return fontAsset;
        }

        private static bool IsUsableDialogueFontAsset(TMP_FontAsset fontAsset)
        {
            return fontAsset != null
                && fontAsset.atlasTextures != null
                && fontAsset.atlasTextures.Length > 0
                && fontAsset.atlasTextures[0] != null
                && fontAsset.material != null;
        }

        private static void PersistFontAssetSubAssets(TMP_FontAsset fontAsset, string assetName)
        {
            if (fontAsset.atlasTextures == null || fontAsset.atlasTextures.Length == 0 || fontAsset.atlasTextures[0] == null)
            {
                throw new InvalidOperationException("TMP dialogue font asset was created without an atlas texture.");
            }

            if (fontAsset.material == null)
            {
                throw new InvalidOperationException("TMP dialogue font asset was created without a material.");
            }

            Texture2D atlasTexture = fontAsset.atlasTextures[0];
            atlasTexture.name = assetName + " Atlas";
            fontAsset.material.name = assetName + " Material";
            fontAsset.material.mainTexture = atlasTexture;

            AssetDatabase.AddObjectToAsset(atlasTexture, fontAsset);
            AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
        }

        private static IntroGatePodDialogueTrack EnsureDialogueTrack(TimelineAsset timeline)
        {
            IntroGatePodDialogueTrack existing = FindTrack<IntroGatePodDialogueTrack>(timeline, DialogueTrackName);
            if (existing != null)
            {
                return existing;
            }

            IntroGatePodDialogueTrack track = timeline.CreateTrack<IntroGatePodDialogueTrack>(DialogueTrackName);
            EditorUtility.SetDirty(track);
            return track;
        }

        private static int EnsureDialogueClips(TimelineAsset timeline, IntroGatePodDialogueTrack dialogueTrack)
        {
            if (HasAnyClips(dialogueTrack))
            {
                return 0;
            }

            List<TimelineClip> audioClips = CollectBombingVoiceAudioClips(timeline);
            int created = 0;
            for (int i = 0; i < audioClips.Count; i++)
            {
                TimelineClip source = audioClips[i];
                TimelineClip dialogueClip = dialogueTrack.CreateClip<IntroGatePodDialogueClip>();
                dialogueClip.displayName = $"Dialogue - {source.displayName}";
                dialogueClip.start = source.start;
                dialogueClip.duration = source.duration;
                dialogueClip.blendInDuration = Math.Min(0.10d, source.duration * 0.20d);
                dialogueClip.blendOutDuration = Math.Min(0.12d, source.duration * 0.20d);
                dialogueClip.easeInDuration = dialogueClip.blendInDuration;
                dialogueClip.easeOutDuration = dialogueClip.blendOutDuration;

                IntroGatePodDialogueClip asset = dialogueClip.asset as IntroGatePodDialogueClip;
                if (asset != null)
                {
                    asset.SpeakerName = string.Empty;
                    asset.DialogueText = string.Empty;
                    asset.FadeInSeconds = 0.10f;
                    asset.FadeOutSeconds = 0.12f;
                    asset.MaxAlpha = 1f;
                    EditorUtility.SetDirty(asset);
                }

                created++;
            }

            return created;
        }

        private static List<string> ValidateSetup(Scene scene, TimelineAsset timeline, PlayableDirector director)
        {
            List<string> issues = new List<string>();
            IntroGatePodDialogueOverlay overlay = FindObjectInScene(scene, OverlayObjectName)?.GetComponent<IntroGatePodDialogueOverlay>();
            TMP_FontAsset speakerFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(SpeakerFontAssetPath);
            TMP_FontAsset dialogueFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(DialogueFontAssetPath);

            if (overlay == null)
            {
                issues.Add("Missing dialogue overlay in the Olympus scene.");
            }
            else if (!overlay.HasBindings)
            {
                issues.Add("Dialogue overlay is missing CanvasGroup/SpeakerText/LineText bindings.");
            }
            else
            {
                TMP_Text speakerText = FindObjectInScene(scene, "SpeakerText")?.GetComponent<TMP_Text>();
                TMP_Text lineText = FindObjectInScene(scene, "LineText")?.GetComponent<TMP_Text>();
                if (!IsUsableDialogueFontAsset(speakerFont))
                {
                    issues.Add("Missing usable Pretendard SemiBold TMP font asset for speaker text.");
                }

                if (!IsUsableDialogueFontAsset(dialogueFont))
                {
                    issues.Add("Missing usable Pretendard Medium TMP font asset for dialogue text.");
                }

                if (speakerText == null || speakerText.font != speakerFont)
                {
                    issues.Add("SpeakerText is not using the Pretendard SemiBold TMP font asset.");
                }

                if (lineText == null || lineText.font != dialogueFont)
                {
                    issues.Add("LineText is not using the Pretendard Medium TMP font asset.");
                }
            }

            IntroGatePodDialogueTrack track = FindTrack<IntroGatePodDialogueTrack>(timeline, DialogueTrackName);
            if (track == null)
            {
                issues.Add("Combined Timeline is missing the Bombing Prelude Dialogue track.");
            }
            else
            {
                if (!HasAnyClips(track))
                {
                    issues.Add("Bombing Prelude Dialogue track has no editable dialogue clips.");
                }

                if (overlay != null && director.GetGenericBinding(track) != overlay)
                {
                    issues.Add("Bombing Prelude Dialogue track is not bound to the scene dialogue overlay.");
                }
            }

            if (CountBombingVoiceAudioClips(timeline) == 0)
            {
                issues.Add("Could not find the current bombing voice audio clips in the combined Timeline.");
            }

            return issues;
        }

        private static List<TimelineClip> CollectBombingVoiceAudioClips(TimelineAsset timeline)
        {
            List<TimelineClip> clips = new List<TimelineClip>();
            foreach (TrackAsset track in timeline.GetOutputTracks())
            {
                if (!(track is AudioTrack))
                {
                    continue;
                }

                foreach (TimelineClip clip in track.GetClips())
                {
                    string displayName = clip.displayName ?? string.Empty;
                    if (displayName.StartsWith("Cinematic_bombing", StringComparison.Ordinal))
                    {
                        clips.Add(clip);
                    }
                }
            }

            clips.Sort((left, right) => left.start.CompareTo(right.start));
            return clips;
        }

        private static int CountBombingVoiceAudioClips(TimelineAsset timeline)
        {
            return CollectBombingVoiceAudioClips(timeline).Count;
        }

        private static void WriteReport(List<string> issues, int createdClipCount, int sourceAudioClipCount)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# Intro GatePod Dialogue Overlay Setup");
            builder.AppendLine();
            builder.AppendLine(issues.Count == 0 ? "Status: PASS" : "Status: FAIL");
            builder.AppendLine();
            builder.AppendLine($"- Scene: `{OlympusStageScenePath}`");
            builder.AppendLine($"- Timeline: `{OlympusCombinedTimelinePath}`");
            builder.AppendLine($"- Overlay object: `{OverlayObjectName}`");
            builder.AppendLine($"- Dialogue track: `{DialogueTrackName}`");
            builder.AppendLine($"- Speaker TMP font: `{SpeakerFontAssetPath}`");
            builder.AppendLine($"- Dialogue TMP font: `{DialogueFontAssetPath}`");
            builder.AppendLine($"- Korean probe text: `{KoreanProbeText}`");
            builder.AppendLine($"- Source bombing voice clips found: `{sourceAudioClipCount}`");
            builder.AppendLine($"- Dialogue clips created this run: `{createdClipCount}`");
            builder.AppendLine("- Edit each Timeline dialogue clip's `Speaker Name` and `Dialogue Text` fields in the Inspector.");
            if (issues.Count > 0)
            {
                builder.AppendLine();
                builder.AppendLine("## Issues");
                for (int i = 0; i < issues.Count; i++)
                {
                    builder.AppendLine($"- {issues[i]}");
                }
            }

            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath) ?? "C:/tmp");
            File.WriteAllText(ReportPath, builder.ToString(), Encoding.UTF8);
        }

        private static TMP_Text EnsureText(RectTransform parent, string objectName, float fontSize, TextAlignmentOptions alignment)
        {
            RectTransform rect = EnsureChild(parent, objectName);
            TextMeshProUGUI text = rect.GetComponent<TextMeshProUGUI>();
            if (text == null)
            {
                text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            }

            text.text = string.Empty;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.raycastTarget = false;
            text.enableWordWrapping = false;
            return text;
        }

        private static Image EnsureImage(RectTransform parent, string objectName)
        {
            RectTransform rect = EnsureChild(parent, objectName);
            Image image = rect.GetComponent<Image>();
            if (image == null)
            {
                image = rect.gameObject.AddComponent<Image>();
            }

            return image;
        }

        private static RectTransform EnsureChild(RectTransform parent, string objectName)
        {
            Transform existing = parent.Find(objectName);
            GameObject child = existing != null
                ? existing.gameObject
                : new GameObject(objectName, typeof(RectTransform));
            child.transform.SetParent(parent, worldPositionStays: false);
            return child.GetComponent<RectTransform>();
        }

        private static void StretchFullScreen(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
        }

        private static void ConfigureAnchored(
            RectTransform rect,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 sizeDelta,
            Vector2 anchoredPosition)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.sizeDelta = sizeDelta;
            rect.anchoredPosition = anchoredPosition;
            rect.localScale = Vector3.one;
        }

        private static bool HasAnyClips(TrackAsset track)
        {
            foreach (TimelineClip ignored in track.GetClips())
            {
                return true;
            }

            return false;
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

        private static GameObject FindObjectInScene(Scene scene, string objectName)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                GameObject found = FindObjectRecursive(roots[i].transform, objectName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static GameObject FindObjectRecursive(Transform root, string objectName)
        {
            if (string.Equals(root.name, objectName, StringComparison.Ordinal))
            {
                return root.gameObject;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                GameObject found = FindObjectRecursive(root.GetChild(i), objectName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static T LoadRequired<T>(string path)
            where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                throw new InvalidOperationException($"Missing asset `{path}`.");
            }

            return asset;
        }

        private static string AssetPathToAbsolutePath(string assetPath)
        {
            if (!assetPath.StartsWith("Assets/", StringComparison.Ordinal))
            {
                throw new ArgumentException($"Expected an asset path under Assets: {assetPath}", nameof(assetPath));
            }

            string relativePath = assetPath.Substring("Assets/".Length).Replace('/', Path.DirectorySeparatorChar);
            return Path.Combine(Application.dataPath, relativePath);
        }
    }
}
