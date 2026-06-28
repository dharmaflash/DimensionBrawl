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
    public static class IntroGatePodLetterboxOverlaySetup
    {
        private const string OlympusStageScenePath = "Assets/_Game/Scenes/OlympusCorridorInvasionStage.unity";
        private const string OlympusCombinedTimelinePath =
            "Assets/_Game/DesignData/Timelines/Cinematics/DB_Timeline_IntroGatePodAwakening_OlympusBombingPrelude.playable";
        private const string OverlayObjectName = "IntroGatePodReview_LetterboxOverlay";
        private const string LetterboxTrackName = "Cutscene Letterbox";
        private const string LetterboxClipName = "Letterbox - Cutscene Frame";
        private const string ReportPath = "C:/tmp/DimensionBrawl-IntroGatePodLetterboxOverlaySetup.md";
        private const float DefaultBarHeight = 39.333332f;
        private const double MinimumClipDuration = 8.0d;

        [MenuItem("Tools/DimensionBrawl/Intro GatePod/Setup Letterbox Overlay")]
        public static void SetupLetterboxOverlayMenu()
        {
            SetupLetterboxOverlay(writeReport: true);
        }

        public static void RunBatchSetupLetterboxOverlay()
        {
            SetupLetterboxOverlay(writeReport: true);
        }

        private static void SetupLetterboxOverlay(bool writeReport)
        {
            AssetDatabase.Refresh();
            Scene scene = EditorSceneManager.OpenScene(OlympusStageScenePath, OpenSceneMode.Single);
            TimelineAsset timeline = LoadRequired<TimelineAsset>(OlympusCombinedTimelinePath);
            PlayableDirector director = FindDirectorBoundToTimeline(scene, timeline)
                ?? throw new InvalidOperationException("Could not find the Olympus intro PlayableDirector bound to the combined Timeline.");

            IntroGatePodLetterboxOverlay overlay = EnsureLetterboxOverlay(scene);
            IntroGatePodLetterboxTrack track = EnsureLetterboxTrack(timeline);
            director.SetGenericBinding(track, overlay);
            bool createdClip = EnsureLetterboxClip(timeline, track);

            EditorUtility.SetDirty(overlay);
            EditorUtility.SetDirty(track);
            EditorUtility.SetDirty(timeline);
            EditorUtility.SetDirty(director);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            List<string> issues = ValidateSetup(scene, timeline, director);
            if (writeReport)
            {
                WriteReport(issues, createdClip);
            }

            if (issues.Count > 0)
            {
                throw new InvalidOperationException(
                    "Intro GatePod letterbox overlay setup failed:\n" + string.Join("\n", issues));
            }
        }

        private static IntroGatePodLetterboxOverlay EnsureLetterboxOverlay(Scene scene)
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
                    typeof(IntroGatePodLetterboxOverlay));
                SceneManager.MoveGameObjectToScene(overlayObject, scene);
            }

            RectTransform overlayRect = overlayObject.GetComponent<RectTransform>();
            StretchFullScreen(overlayRect);

            Canvas canvas = overlayObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 825;

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

            Image topBar = EnsureImage(overlayRect, "TopBar");
            topBar.color = new Color(0f, 0f, 0f, 0.98f);
            topBar.raycastTarget = false;
            ConfigureBar(topBar.rectTransform, anchorY: 1f, pivotY: 1f);

            Image bottomBar = EnsureImage(overlayRect, "BottomBar");
            bottomBar.color = new Color(0f, 0f, 0f, 0.98f);
            bottomBar.raycastTarget = false;
            ConfigureBar(bottomBar.rectTransform, anchorY: 0f, pivotY: 0f);

            IntroGatePodLetterboxOverlay overlay = overlayObject.GetComponent<IntroGatePodLetterboxOverlay>();
            overlay.Configure(canvasGroup, topBar.rectTransform, bottomBar.rectTransform, DefaultBarHeight);
            overlay.Clear();

            EditorUtility.SetDirty(overlayObject);
            return overlay;
        }

        private static IntroGatePodLetterboxTrack EnsureLetterboxTrack(TimelineAsset timeline)
        {
            IntroGatePodLetterboxTrack existing = FindTrack<IntroGatePodLetterboxTrack>(timeline, LetterboxTrackName);
            if (existing != null)
            {
                return existing;
            }

            IntroGatePodLetterboxTrack track = timeline.CreateTrack<IntroGatePodLetterboxTrack>(LetterboxTrackName);
            EditorUtility.SetDirty(track);
            return track;
        }

        private static bool EnsureLetterboxClip(TimelineAsset timeline, IntroGatePodLetterboxTrack track)
        {
            TimelineClip existing = FindClip(track, LetterboxClipName);
            if (existing != null)
            {
                existing.start = 0d;
                existing.duration = Math.Max(existing.duration, Math.Max(timeline.duration, MinimumClipDuration));
                ConfigureClipAsset(existing.asset as IntroGatePodLetterboxClip);
                EditorUtility.SetDirty(track);
                return false;
            }

            TimelineClip clip = track.CreateClip<IntroGatePodLetterboxClip>();
            clip.displayName = LetterboxClipName;
            clip.start = 0d;
            clip.duration = Math.Max(timeline.duration, MinimumClipDuration);
            clip.easeInDuration = 0d;
            clip.easeOutDuration = 0d;
            ConfigureClipAsset(clip.asset as IntroGatePodLetterboxClip);
            EditorUtility.SetDirty(track);
            return true;
        }

        private static void ConfigureClipAsset(IntroGatePodLetterboxClip clip)
        {
            if (clip == null)
            {
                return;
            }

            clip.BarHeight = DefaultBarHeight;
            clip.MaxAlpha = 0.96f;
            clip.FadeInSeconds = 0.55f;
            clip.FadeOutSeconds = 0.45f;
            clip.FadeOutAtClipEnd = true;
            EditorUtility.SetDirty(clip);
        }

        private static List<string> ValidateSetup(Scene scene, TimelineAsset timeline, PlayableDirector director)
        {
            List<string> issues = new List<string>();
            IntroGatePodLetterboxOverlay overlay = FindObjectInScene(scene, OverlayObjectName)?.GetComponent<IntroGatePodLetterboxOverlay>();
            if (overlay == null)
            {
                issues.Add("Missing letterbox overlay in the Olympus scene.");
            }
            else if (!overlay.HasBindings)
            {
                issues.Add("Letterbox overlay is missing CanvasGroup/TopBar/BottomBar bindings.");
            }

            IntroGatePodLetterboxTrack track = FindTrack<IntroGatePodLetterboxTrack>(timeline, LetterboxTrackName);
            if (track == null)
            {
                issues.Add("Combined Timeline is missing the Cutscene Letterbox track.");
            }
            else
            {
                if (FindClip(track, LetterboxClipName) == null)
                {
                    issues.Add("Cutscene Letterbox track is missing the framing clip.");
                }

                if (overlay != null && director.GetGenericBinding(track) != overlay)
                {
                    issues.Add("Cutscene Letterbox track is not bound to the scene letterbox overlay.");
                }
            }

            return issues;
        }

        private static void WriteReport(IReadOnlyCollection<string> issues, bool createdClip)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# Intro GatePod Letterbox Overlay Setup");
            builder.AppendLine();
            builder.AppendLine(issues.Count == 0 ? "Status: PASS" : "Status: FAIL");
            builder.AppendLine();
            builder.AppendLine($"- Scene: `{OlympusStageScenePath}`");
            builder.AppendLine($"- Timeline: `{OlympusCombinedTimelinePath}`");
            builder.AppendLine($"- Overlay object: `{OverlayObjectName}`");
            builder.AppendLine($"- Letterbox track: `{LetterboxTrackName}`");
            builder.AppendLine($"- Letterbox clip created this run: `{createdClip}`");
            builder.AppendLine("- Start transition: `0.55s`");
            builder.AppendLine("- End transition: `0.45s` at clip end");
            builder.AppendLine("- Later, extend the letterbox clip to the final cutscene end if the ending section grows.");
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

        private static void ConfigureBar(RectTransform rectTransform, float anchorY, float pivotY)
        {
            rectTransform.anchorMin = new Vector2(0f, anchorY);
            rectTransform.anchorMax = new Vector2(1f, anchorY);
            rectTransform.pivot = new Vector2(0.5f, pivotY);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = new Vector2(0f, 0f);
        }

        private static void StretchFullScreen(RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;
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

        private static TimelineClip FindClip(TrackAsset track, string clipName)
        {
            foreach (TimelineClip clip in track.GetClips())
            {
                if (string.Equals(clip.displayName, clipName, StringComparison.Ordinal))
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

        private static T LoadRequired<T>(string path)
            where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                throw new InvalidOperationException($"Missing required asset: {path}");
            }

            return asset;
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
                Transform match = FindChildRecursive(roots[i].transform, objectName);
                if (match != null)
                {
                    return match.gameObject;
                }
            }

            return null;
        }

        private static Transform FindChildRecursive(Transform root, string objectName)
        {
            if (root.name == objectName)
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform match = FindChildRecursive(root.GetChild(i), objectName);
                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }
    }
}
