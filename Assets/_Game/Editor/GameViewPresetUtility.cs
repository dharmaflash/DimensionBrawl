using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace IsekaiBrawl.EditorTools
{
    public static class GameViewPresetUtility
    {
        private const string WorkPresetLabel = "IsekaiBrawl Work 1280x720";
        private const string CapturePresetLabel = "IsekaiBrawl MCP Capture 2560x1440";

        [MenuItem("Tools/IsekaiBrawl/Game View/Apply Work Preset")]
        public static void ApplyWorkPresetMenu()
        {
            TryApplyWorkPreset(out string message);
            Debug.Log($"[GameViewPresetUtility] {message}");
        }

        [MenuItem("Tools/IsekaiBrawl/Game View/Apply MCP Capture Preset")]
        public static void ApplyCapturePresetMenu()
        {
            TryApplyCapturePreset(out string message);
            Debug.Log($"[GameViewPresetUtility] {message}");
        }

        public static bool TryApplyWorkPreset(out string message)
        {
            return TryApplyPreset(WorkPresetLabel, 1280, 720, out message);
        }

        public static bool TryApplyCapturePreset(out string message)
        {
            return TryApplyPreset(CapturePresetLabel, 2560, 1440, out message);
        }

        private static bool TryApplyPreset(string label, int width, int height, out string message)
        {
            try
            {
                EditorWindow gameView = GetGameViewWindow();
                object sizeGroup = GetCurrentSizeGroup();
                if (gameView == null || sizeGroup == null)
                {
                    message = "Game View preset utility could not resolve the active Game View group.";
                    return false;
                }

                int sizeIndex = EnsurePresetIndex(sizeGroup, label, width, height);
                if (sizeIndex < 0)
                {
                    message = $"Failed to locate or create Game View preset '{label}'.";
                    return false;
                }

                SetSelectedSizeIndex(gameView, sizeIndex);
                gameView.Repaint();
                message = $"Applied Game View preset '{label}' ({width}x{height}).";
                return true;
            }
            catch (Exception exception)
            {
                message = $"Game View preset apply failed: {exception.Message}";
                return false;
            }
        }

        private static EditorWindow GetGameViewWindow()
        {
            Type gameViewType = Type.GetType("UnityEditor.GameView,UnityEditor");
            return gameViewType != null ? EditorWindow.GetWindow(gameViewType, false, null, false) : null;
        }

        private static object GetCurrentSizeGroup()
        {
            Type sizesType = Type.GetType("UnityEditor.GameViewSizes,UnityEditor");
            if (sizesType == null)
            {
                return null;
            }

            Type singletonType = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
            object instance = singletonType.GetProperty("instance", BindingFlags.Public | BindingFlags.Static)?.GetValue(null, null);
            if (instance == null)
            {
                return null;
            }

            PropertyInfo currentGroupProperty = sizesType.GetProperty("currentGroup", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (currentGroupProperty != null)
            {
                return currentGroupProperty.GetValue(instance, null);
            }

            PropertyInfo currentGroupTypeProperty = sizesType.GetProperty("currentGroupType", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            object currentGroupType = currentGroupTypeProperty?.GetValue(instance, null);
            if (currentGroupType == null)
            {
                return null;
            }

            MethodInfo getGroupMethod = sizesType.GetMethod("GetGroup", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { currentGroupType.GetType() }, null);
            if (getGroupMethod != null)
            {
                return getGroupMethod.Invoke(instance, new[] { currentGroupType });
            }

            MethodInfo intGroupMethod = sizesType.GetMethod("GetGroup", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(int) }, null);
            return intGroupMethod?.Invoke(instance, new object[] { Convert.ToInt32(currentGroupType) });
        }

        private static int EnsurePresetIndex(object sizeGroup, string label, int width, int height)
        {
            int existingIndex = FindPresetIndex(sizeGroup, label, width, height);
            if (existingIndex >= 0)
            {
                return existingIndex;
            }

            Type sizeType = Type.GetType("UnityEditor.GameViewSize,UnityEditor");
            Type sizeEnumType = Type.GetType("UnityEditor.GameViewSizeType,UnityEditor");
            if (sizeType == null || sizeEnumType == null)
            {
                return -1;
            }

            object fixedResolutionValue = Enum.Parse(sizeEnumType, "FixedResolution");
            ConstructorInfo constructor =
                sizeType.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { sizeEnumType, typeof(int), typeof(int), typeof(string) }, null) ??
                sizeType.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(int), typeof(int), typeof(int), typeof(string) }, null);

            if (constructor == null)
            {
                return -1;
            }

            object newSize = constructor.GetParameters()[0].ParameterType == sizeEnumType
                ? constructor.Invoke(new[] { fixedResolutionValue, (object)width, height, label })
                : constructor.Invoke(new object[] { Convert.ToInt32(fixedResolutionValue), width, height, label });

            MethodInfo addCustomSizeMethod = sizeGroup.GetType().GetMethod("AddCustomSize", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            addCustomSizeMethod?.Invoke(sizeGroup, new[] { newSize });

            return FindPresetIndex(sizeGroup, label, width, height);
        }

        private static int FindPresetIndex(object sizeGroup, string label, int width, int height)
        {
            MethodInfo getBuiltinCountMethod = sizeGroup.GetType().GetMethod("GetBuiltinCount", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            MethodInfo getCustomCountMethod = sizeGroup.GetType().GetMethod("GetCustomCount", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            MethodInfo getGameViewSizeMethod = sizeGroup.GetType().GetMethod("GetGameViewSize", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (getBuiltinCountMethod == null || getCustomCountMethod == null || getGameViewSizeMethod == null)
            {
                return -1;
            }

            int totalCount = Convert.ToInt32(getBuiltinCountMethod.Invoke(sizeGroup, null)) +
                             Convert.ToInt32(getCustomCountMethod.Invoke(sizeGroup, null));

            for (int index = 0; index < totalCount; index++)
            {
                object size = getGameViewSizeMethod.Invoke(sizeGroup, new object[] { index });
                if (size == null)
                {
                    continue;
                }

                int sizeWidth = GetIntProperty(size, "width");
                int sizeHeight = GetIntProperty(size, "height");
                string sizeLabel = GetStringProperty(size, "baseText") ?? GetStringProperty(size, "displayText") ?? size.ToString();

                if (sizeWidth == width && sizeHeight == height &&
                    sizeLabel != null &&
                    sizeLabel.IndexOf(label, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return index;
                }
            }

            return -1;
        }

        private static void SetSelectedSizeIndex(EditorWindow gameView, int sizeIndex)
        {
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            MethodInfo sizeSelectionCallback = gameView.GetType().GetMethod("SizeSelectionCallback", flags);
            if (sizeSelectionCallback != null)
            {
                ParameterInfo[] parameters = sizeSelectionCallback.GetParameters();
                if (parameters.Length == 2)
                {
                    sizeSelectionCallback.Invoke(gameView, new object[] { sizeIndex, null });
                }
                else if (parameters.Length == 1)
                {
                    sizeSelectionCallback.Invoke(gameView, new object[] { sizeIndex });
                }
            }

            PropertyInfo selectedSizeIndexProperty = gameView.GetType().GetProperty("selectedSizeIndex", flags);
            selectedSizeIndexProperty?.SetValue(gameView, sizeIndex, null);
        }

        private static int GetIntProperty(object target, string propertyName)
        {
            PropertyInfo property = target.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            object value = property?.GetValue(target, null);
            return value != null ? Convert.ToInt32(value) : 0;
        }

        private static string GetStringProperty(object target, string propertyName)
        {
            PropertyInfo property = target.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return property?.GetValue(target, null) as string;
        }
    }
}
