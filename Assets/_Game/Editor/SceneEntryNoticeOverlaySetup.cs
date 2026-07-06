#if UNITY_EDITOR
using System;
using DimensionBrawl.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DimensionBrawl.Editor
{
    public static class SceneEntryNoticeOverlaySetup
    {
        private const string PrefabPath = "Assets/_Game/UI/Transitions/PF_UI_SceneEntryNoticeOverlay.prefab";
        private const string ProfileFolder = "Assets/_Game/DesignData/UI/SceneEntryNotices";
        private const string InstanceRootName = "SceneEntryNoticeOverlay";
        private const string CutsceneScenePath = "Assets/_Game/Scenes/OlympusCorridorInvasionStage.unity";
        private const string CombatScenePath = "Assets/_Game/Scenes/OlympusStationCombatStage.unity";
        private const string DefaultStartBeepClipGuid = "480de4e28dbc0da4e9a1bdffcbca163d";

        [MenuItem("DimensionBrawl/UI/Scene Entry Notice/Apply To Olympus Scenes")]
        public static void ApplyToOlympusScenesMenu()
        {
            ApplyToOlympusScenes();
        }

        public static void RunBatchApply()
        {
            try
            {
                ApplyToOlympusScenes();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        [MenuItem("DimensionBrawl/UI/Scene Entry Notice/Remove From Cutscene Scene")]
        public static void RemoveFromCutsceneSceneMenu()
        {
            RemoveFromScene(CutsceneScenePath);
        }

        public static void RunBatchRemoveCutscene()
        {
            try
            {
                RemoveFromScene(CutsceneScenePath);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        public static void ApplyToOlympusScenes()
        {
            AssetDatabase.Refresh();
            EnsureFolder(ProfileFolder);

            AudioClip startBeepClip = ResolveStartBeepClip();
            SceneEntryNoticeProfile combatProfile = EnsureProfile(
                ProfileFolder + "/DB_SceneEntryNotice_OlympusStationCombat.asset",
                "COMBAT NOTICE",
                "Olympus Station Combat Zone",
                "Hostile dimensional signature locked. Maintain lane discipline and finish the guardian.",
                "TARGET // LOCKED",
                "SUPPORT // ONLINE",
                new Color(0.22f, 0.94f, 1f, 1f),
                new Color(0.006f, 0.038f, 0.06f, 0.88f),
                new Color(0.58f, 1f, 1f, 0.76f),
                new Color(0f, 0.02f, 0.035f, 0.15f),
                0.16f,
                0.34f,
                1.7f,
                0.32f,
                startBeepClip);
            GameObject prefab = EnsurePrefabAsset();
            EnsurePrefabDefaultProfile(prefab, combatProfile);

            RemoveFromScene(CutsceneScenePath);
            ApplyToScene(CombatScenePath, prefab, combatProfile);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Applied scene entry notice overlay to Olympus scenes.");
        }

        private static void ApplyToScene(string scenePath, GameObject prefab, SceneEntryNoticeProfile profile)
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            SceneEntryNoticeOverlay overlay = FindSceneComponent<SceneEntryNoticeOverlay>(scene);
            GameObject root = overlay != null ? overlay.gameObject : FindRoot(scene, InstanceRootName);
            if (root == null)
            {
                root = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
                root.name = InstanceRootName;
            }

            overlay = root.GetComponent<SceneEntryNoticeOverlay>();
            if (overlay == null)
            {
                overlay = root.AddComponent<SceneEntryNoticeOverlay>();
            }

            Canvas canvas = root.GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 72;
            }

            SerializedObject serializedOverlay = new SerializedObject(overlay);
            serializedOverlay.FindProperty("profile").objectReferenceValue = profile;
            serializedOverlay.FindProperty("playOnStart").boolValue = true;
            serializedOverlay.FindProperty("startBeepVolume").floatValue = 0.85f;
            serializedOverlay.FindProperty("replayOnEnable").boolValue = false;
            serializedOverlay.FindProperty("useUnscaledTime").boolValue = true;
            serializedOverlay.FindProperty("pauseGameplayDuringNotice").boolValue = true;
            serializedOverlay.FindProperty("blockPointerInputDuringNotice").boolValue = true;
            serializedOverlay.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.RecordPrefabInstancePropertyModifications(overlay);

            EditorUtility.SetDirty(root);
            EditorUtility.SetDirty(overlay);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, scenePath))
            {
                throw new InvalidOperationException($"Failed to save scene entry notice scene: {scenePath}");
            }
        }

        private static void RemoveFromScene(string scenePath)
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            SceneEntryNoticeOverlay overlay = FindSceneComponent<SceneEntryNoticeOverlay>(scene);
            GameObject root = overlay != null ? overlay.gameObject : FindRoot(scene, InstanceRootName);
            if (root == null)
            {
                return;
            }

            UnityEngine.Object.DestroyImmediate(root);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, scenePath))
            {
                throw new InvalidOperationException($"Failed to remove scene entry notice from scene: {scenePath}");
            }
        }

        private static GameObject EnsurePrefabAsset()
        {
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (existing != null)
            {
                return existing;
            }

            GameObject root = CreatePrefabGraph();
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            if (prefab == null)
            {
                throw new InvalidOperationException($"Failed to create {PrefabPath}.");
            }

            AssetDatabase.ImportAsset(PrefabPath);
            return AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        }

        private static void EnsurePrefabDefaultProfile(GameObject prefab, SceneEntryNoticeProfile profile)
        {
            string path = AssetDatabase.GetAssetPath(prefab);
            GameObject contents = PrefabUtility.LoadPrefabContents(path);
            try
            {
                SceneEntryNoticeOverlay overlay = contents.GetComponent<SceneEntryNoticeOverlay>();
                if (overlay == null)
                {
                    throw new InvalidOperationException($"{path} is missing {nameof(SceneEntryNoticeOverlay)}.");
                }

                SerializedObject serializedOverlay = new SerializedObject(overlay);
                serializedOverlay.FindProperty("profile").objectReferenceValue = profile;
                serializedOverlay.FindProperty("startBeepVolume").floatValue = 0.85f;
                serializedOverlay.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(overlay);
                PrefabUtility.SaveAsPrefabAsset(contents, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contents);
            }
        }

        private static GameObject CreatePrefabGraph()
        {
            Font font = ResolveDefaultFont();
            GameObject root = new GameObject(
                "PF_UI_SceneEntryNoticeOverlay",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(CanvasGroup),
                typeof(SceneEntryNoticeOverlay));
            Stretch(root.GetComponent<RectTransform>());

            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 72;

            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(2560f, 1440f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            CanvasGroup rootGroup = root.GetComponent<CanvasGroup>();
            rootGroup.alpha = 0f;
            rootGroup.interactable = false;
            rootGroup.blocksRaycasts = false;

            Image dimImage = CreateImage(root.transform, "ScreenDim", new Color(0f, 0.025f, 0.035f, 0.16f));
            Stretch(dimImage.rectTransform);

            GameObject panel = new GameObject("SystemStatusBand", typeof(RectTransform), typeof(CanvasGroup));
            panel.transform.SetParent(root.transform, false);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.12f, 0.56f);
            panelRect.anchorMax = new Vector2(0.88f, 0.68f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = Vector2.zero;

            CanvasGroup panelGroup = panel.GetComponent<CanvasGroup>();
            panelGroup.alpha = 0f;
            panelGroup.interactable = false;
            panelGroup.blocksRaycasts = false;

            Image background = CreateImage(panel.transform, "BandBackground", new Color(0.006f, 0.038f, 0.06f, 0.88f));
            Stretch(background.rectTransform);

            Image topLine = CreateImage(panel.transform, "TopSignalLine", new Color(0.22f, 0.94f, 1f, 1f));
            SetAnchorRect(topLine.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 0f), new Vector2(0f, 4f));

            Image bottomLine = CreateImage(panel.transform, "BottomSignalLine", new Color(0.22f, 0.94f, 1f, 1f));
            SetAnchorRect(bottomLine.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 0f), new Vector2(0f, 4f));

            Image leftAccent = CreateImage(panel.transform, "LeftSignalLatch", new Color(0.22f, 0.94f, 1f, 1f));
            SetAnchorRect(leftAccent.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f), new Vector2(0f, 0f), new Vector2(8f, 0f));

            Image rightAccent = CreateImage(panel.transform, "RightSignalLatch", new Color(0.22f, 0.94f, 1f, 1f));
            SetAnchorRect(rightAccent.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(1f, 0.5f), new Vector2(0f, 0f), new Vector2(8f, 0f));

            Image scanLine = CreateImage(panel.transform, "HorizontalScanSweep", new Color(0.58f, 1f, 1f, 0.76f));
            SetAnchorRect(scanLine.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(18f, 0f));

            Text eyebrowText = CreateText(panel.transform, "EyebrowText", font, 24, FontStyle.Bold, TextAnchor.MiddleLeft, new Color(0.22f, 0.94f, 1f, 1f));
            SetAnchorRect(eyebrowText.rectTransform, new Vector2(0.045f, 0.67f), new Vector2(0.34f, 0.93f), new Vector2(0f, 0.5f), Vector2.zero, Vector2.zero);

            Text titleText = CreateText(panel.transform, "TitleText", font, 42, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.93f, 1f, 1f, 1f));
            SetAnchorRect(titleText.rectTransform, new Vector2(0.18f, 0.38f), new Vector2(0.82f, 0.78f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            Text bodyText = CreateText(panel.transform, "BodyText", font, 25, FontStyle.Normal, TextAnchor.MiddleCenter, new Color(0.78f, 0.96f, 1f, 0.94f));
            SetAnchorRect(bodyText.rectTransform, new Vector2(0.16f, 0.13f), new Vector2(0.84f, 0.43f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            Text leftStatusText = CreateText(panel.transform, "LeftStatusText", font, 19, FontStyle.Bold, TextAnchor.MiddleLeft, new Color(0.22f, 0.94f, 1f, 1f));
            SetAnchorRect(leftStatusText.rectTransform, new Vector2(0.045f, 0.12f), new Vector2(0.26f, 0.33f), new Vector2(0f, 0.5f), Vector2.zero, Vector2.zero);

            Text rightStatusText = CreateText(panel.transform, "RightStatusText", font, 19, FontStyle.Bold, TextAnchor.MiddleRight, new Color(0.22f, 0.94f, 1f, 1f));
            SetAnchorRect(rightStatusText.rectTransform, new Vector2(0.74f, 0.12f), new Vector2(0.955f, 0.33f), new Vector2(1f, 0.5f), Vector2.zero, Vector2.zero);

            AudioSource audioSource = root.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;

            SceneEntryNoticeOverlay overlay = root.GetComponent<SceneEntryNoticeOverlay>();
            SerializedObject serializedOverlay = new SerializedObject(overlay);
            serializedOverlay.FindProperty("rootGroup").objectReferenceValue = rootGroup;
            serializedOverlay.FindProperty("panelGroup").objectReferenceValue = panelGroup;
            serializedOverlay.FindProperty("panelRoot").objectReferenceValue = panelRect;
            serializedOverlay.FindProperty("dimImage").objectReferenceValue = dimImage;
            serializedOverlay.FindProperty("panelBackgroundImage").objectReferenceValue = background;
            serializedOverlay.FindProperty("topLineImage").objectReferenceValue = topLine;
            serializedOverlay.FindProperty("bottomLineImage").objectReferenceValue = bottomLine;
            serializedOverlay.FindProperty("leftAccentImage").objectReferenceValue = leftAccent;
            serializedOverlay.FindProperty("rightAccentImage").objectReferenceValue = rightAccent;
            serializedOverlay.FindProperty("scanLineImage").objectReferenceValue = scanLine;
            serializedOverlay.FindProperty("eyebrowText").objectReferenceValue = eyebrowText;
            serializedOverlay.FindProperty("titleText").objectReferenceValue = titleText;
            serializedOverlay.FindProperty("bodyText").objectReferenceValue = bodyText;
            serializedOverlay.FindProperty("leftStatusText").objectReferenceValue = leftStatusText;
            serializedOverlay.FindProperty("rightStatusText").objectReferenceValue = rightStatusText;
            serializedOverlay.FindProperty("audioSource").objectReferenceValue = audioSource;
            serializedOverlay.FindProperty("playOnStart").boolValue = true;
            serializedOverlay.FindProperty("startBeepVolume").floatValue = 0.85f;
            serializedOverlay.FindProperty("replayOnEnable").boolValue = false;
            serializedOverlay.FindProperty("useUnscaledTime").boolValue = true;
            serializedOverlay.FindProperty("pauseGameplayDuringNotice").boolValue = true;
            serializedOverlay.FindProperty("blockPointerInputDuringNotice").boolValue = true;
            serializedOverlay.ApplyModifiedPropertiesWithoutUndo();

            return root;
        }

        private static SceneEntryNoticeProfile EnsureProfile(
            string path,
            string eyebrowText,
            string titleText,
            string bodyText,
            string leftStatusText,
            string rightStatusText,
            Color accentColor,
            Color backgroundColor,
            Color scanColor,
            Color dimColor,
            float startupDelay,
            float revealSeconds,
            float holdSeconds,
            float dismissSeconds,
            AudioClip startBeepClip)
        {
            SceneEntryNoticeProfile profile = AssetDatabase.LoadAssetAtPath<SceneEntryNoticeProfile>(path);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<SceneEntryNoticeProfile>();
                AssetDatabase.CreateAsset(profile, path);
            }

            SerializedObject serializedProfile = new SerializedObject(profile);
            serializedProfile.FindProperty("eyebrowText").stringValue = eyebrowText;
            serializedProfile.FindProperty("titleText").stringValue = titleText;
            serializedProfile.FindProperty("bodyText").stringValue = bodyText;
            serializedProfile.FindProperty("leftStatusText").stringValue = leftStatusText;
            serializedProfile.FindProperty("rightStatusText").stringValue = rightStatusText;
            serializedProfile.FindProperty("accentColor").colorValue = accentColor;
            serializedProfile.FindProperty("backgroundColor").colorValue = backgroundColor;
            serializedProfile.FindProperty("scanColor").colorValue = scanColor;
            serializedProfile.FindProperty("dimColor").colorValue = dimColor;
            serializedProfile.FindProperty("startupDelaySeconds").floatValue = startupDelay;
            serializedProfile.FindProperty("revealSeconds").floatValue = revealSeconds;
            serializedProfile.FindProperty("holdSeconds").floatValue = holdSeconds;
            serializedProfile.FindProperty("dismissSeconds").floatValue = dismissSeconds;
            serializedProfile.FindProperty("typewriterCharactersPerSecond").floatValue = 48f;
            serializedProfile.FindProperty("startBeepClip").objectReferenceValue = startBeepClip;
            serializedProfile.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static AudioClip ResolveStartBeepClip()
        {
            string path = AssetDatabase.GUIDToAssetPath(DefaultStartBeepClipGuid);
            return string.IsNullOrEmpty(path)
                ? null
                : AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }

        private static Image CreateImage(Transform parent, string name, Color color)
        {
            GameObject owner = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            owner.transform.SetParent(parent, false);
            Image image = owner.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            image.maskable = true;
            return image;
        }

        private static Text CreateText(
            Transform parent,
            string name,
            Font font,
            int fontSize,
            FontStyle fontStyle,
            TextAnchor alignment,
            Color color)
        {
            GameObject owner = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text), typeof(Outline));
            owner.transform.SetParent(parent, false);
            Text text = owner.GetComponent<Text>();
            text.font = font;
            text.fontSize = fontSize;
            text.fontStyle = fontStyle;
            text.alignment = alignment;
            text.color = color;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = Mathf.Max(12, Mathf.RoundToInt(fontSize * 0.68f));
            text.resizeTextMaxSize = fontSize;

            Outline outline = owner.GetComponent<Outline>();
            outline.effectColor = new Color(0f, 0.01f, 0.018f, 0.92f);
            outline.effectDistance = new Vector2(1.2f, -1.2f);
            outline.useGraphicAlpha = true;
            return text;
        }

        private static Font ResolveDefaultFont()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return font != null ? font : Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        private static void Stretch(RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;
        }

        private static void SetAnchorRect(
            RectTransform rectTransform,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 anchoredPosition,
            Vector2 sizeDelta)
        {
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.pivot = pivot;
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = sizeDelta;
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string[] parts = folderPath.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }

        private static T FindSceneComponent<T>(Scene scene)
            where T : Component
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                T component = roots[i].GetComponentInChildren<T>(includeInactive: true);
                if (component != null)
                {
                    return component;
                }
            }

            return null;
        }

        private static GameObject FindRoot(Scene scene, string rootName)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                if (string.Equals(roots[i].name, rootName, StringComparison.Ordinal))
                {
                    return roots[i];
                }
            }

            return null;
        }
    }
}
#endif
