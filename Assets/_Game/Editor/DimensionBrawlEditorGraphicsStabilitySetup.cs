using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace DimensionBrawl.Editor
{
    /// <summary>
    /// Applies the project-owned Windows Editor stability policy used after the recorded D3D12
    /// device-removal incident. Android graphics APIs are deliberately outside this policy.
    /// </summary>
    public static class DimensionBrawlEditorGraphicsStabilitySetup
    {
        private static readonly GraphicsDeviceType[] WindowsSafeGraphicsApis =
        {
            GraphicsDeviceType.Direct3D11,
        };

        [MenuItem("Tools/DimensionBrawl/Safety/Apply Editor Graphics Stability Settings")]
        public static void ApplyMenu()
        {
            RunBatchSetup();
        }

        [MenuItem("Tools/DimensionBrawl/Safety/Validate Editor Graphics Stability Settings")]
        public static void ValidateMenu()
        {
            RunBatchVerification();
        }

        public static void RunBatchSetup()
        {
            // Full reloads are slower, but keep scene/native state deterministic when entering
            // and leaving Play Mode. The recorded crash followed a no-domain/no-scene-reload run.
            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.None;

            // Unity selects the Windows Editor API from the Windows player API list when no
            // command-line override is supplied. Keep Android/mobile player APIs untouched.
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64, false);
            PlayerSettings.SetGraphicsAPIs(
                BuildTarget.StandaloneWindows64,
                WindowsSafeGraphicsApis);

            AssetDatabase.SaveAssets();
            RunBatchVerification();
        }

        public static void RunBatchVerification()
        {
            List<string> issues = CollectIssues();
            if (issues.Count > 0)
            {
                throw new InvalidOperationException(
                    "Editor graphics stability settings are invalid:\n- "
                    + string.Join("\n- ", issues));
            }

            Debug.Log(
                "Editor graphics stability settings passed: Windows uses D3D11 only, and "
                + "Play Mode performs full Domain and Scene reloads.");
        }

        internal static List<string> CollectIssues()
        {
            var issues = new List<string>();

            if (EditorSettings.enterPlayModeOptions != EnterPlayModeOptions.None)
            {
                issues.Add("Enter Play Mode Options flags must be None.");
            }

            if (PlayerSettings.GetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64))
            {
                issues.Add("Windows graphics API selection must not be automatic.");
            }

            GraphicsDeviceType[] configuredApis =
                PlayerSettings.GetGraphicsAPIs(BuildTarget.StandaloneWindows64);
            if (configuredApis == null
                || !configuredApis.SequenceEqual(WindowsSafeGraphicsApis))
            {
                string actual = configuredApis == null
                    ? "<null>"
                    : string.Join(", ", configuredApis.Select(api => api.ToString()));
                issues.Add($"Windows graphics APIs must be exactly Direct3D11; found {actual}.");
            }

            return issues;
        }
    }
}
