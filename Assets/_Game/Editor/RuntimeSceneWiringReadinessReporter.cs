using System.Collections.Generic;
using System.IO;
using System.Text;
using DimensionBrawl.Test;
using DimensionBrawl.UI;
using IsekaiBrawl.Gameplay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DimensionBrawl.Editor
{
    public static class RuntimeSceneWiringReadinessReporter
    {
        private const string ReportPath = "C:/tmp/DimensionBrawl-RuntimeSceneWiringReadinessReport.md";

        private static readonly SceneExpectation[] MinimumSceneExpectations =
        {
            new("Assets/_Game/Scenes/ActionFoundationBossBarrageLaneReview.unity", SceneContractKind.BossBarrageReview),
            new("Assets/_Game/Scenes/UI/UI_CombatHudTest.unity", SceneContractKind.UiRoute)
        };

        [MenuItem("DimensionBrawl/Reports/Runtime Scene Wiring Readiness")]
        public static void ReportMenu()
        {
            ReportCurrentReadiness();
        }

        public static bool ReportCurrentReadiness()
        {
            ReportBuilder report = new();
            SceneSetup[] setup = EditorSceneManager.GetSceneManagerSetup();
            if (HasDirtyOpenScene(out string dirtyScenePath))
            {
                report.AddIssue($"Open scene is dirty before inspection: {dirtyScenePath}");
                report.AppendSummary();
                WriteReport(report);
                Debug.LogWarning($"Runtime scene wiring readiness failed before inspection. See {ReportPath}");
                return false;
            }

            try
            {
                for (int i = 0; i < MinimumSceneExpectations.Length; i++)
                {
                    InspectScene(MinimumSceneExpectations[i], report);
                }
            }
            finally
            {
                if (setup.Length > 0)
                {
                    EditorSceneManager.RestoreSceneManagerSetup(setup);
                }
            }

            report.AppendSummary();
            WriteReport(report);

            if (report.Passed)
            {
                Debug.Log($"Runtime scene wiring readiness passed. Report is read-only; no assets, prefabs, scenes, or ProjectSettings were modified. See {ReportPath}");
                return true;
            }

            Debug.LogWarning($"Runtime scene wiring readiness found issues. Report is read-only; no assets, prefabs, scenes, or ProjectSettings were modified. See {ReportPath}");
            return false;
        }

        private static bool HasDirtyOpenScene(out string dirtyScenePath)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.isDirty)
                {
                    dirtyScenePath = string.IsNullOrWhiteSpace(scene.path) ? scene.name : scene.path;
                    return true;
                }
            }

            dirtyScenePath = string.Empty;
            return false;
        }

        private static void InspectScene(SceneExpectation expectation, ReportBuilder report)
        {
            Scene scene = EditorSceneManager.OpenScene(expectation.ScenePath, OpenSceneMode.Single);
            bool dirtyBefore = scene.isDirty;
            GameObject[] roots = scene.GetRootGameObjects();
            int transformCount = 0;
            int missingScriptCount = 0;

            for (int i = 0; i < roots.Length; i++)
            {
                transformCount += roots[i].GetComponentsInChildren<Transform>(true).Length;
                missingScriptCount += GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(roots[i]);
            }

            report.InspectedSceneCount++;
            report.AppendLine($"## {expectation.ScenePath}");
            report.AppendLine();
            report.AppendLine($"- roots: {roots.Length}");
            report.AppendLine($"- transforms: {transformCount}");
            report.AppendLine($"- missing scripts: {missingScriptCount}");
            report.AppendLine($"- dirty before inspection: {dirtyBefore}");

            if (missingScriptCount > 0)
            {
                report.AddIssue($"{expectation.ScenePath}: missing MonoBehaviour script slots = {missingScriptCount}");
            }

            CheckGenericRuntimeComponents(expectation, report);
            CheckSceneRoleContract(expectation, report);

            bool dirtyAfter = scene.isDirty;
            report.AppendLine($"- dirty after inspection: {dirtyAfter}");
            if (dirtyBefore != dirtyAfter || dirtyAfter)
            {
                report.AddIssue($"{expectation.ScenePath}: dirty flag changed or remained dirty during inspection.");
            }

            report.AppendLine();
        }

        private static void CheckGenericRuntimeComponents(SceneExpectation expectation, ReportBuilder report)
        {
            BattleManager[] managers = Object.FindObjectsByType<BattleManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            report.CheckedContractCount++;
            report.AppendLine($"- BattleManager count: {managers.Length}");

            if (managers.Length > 1)
            {
                report.AddIssue($"{expectation.ScenePath}: scene has multiple BattleManager instances.");
            }

            BattleHUD[] huds = Object.FindObjectsByType<BattleHUD>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            report.CheckedContractCount++;
            report.AppendLine($"- BattleHUD count: {huds.Length}");

            for (int i = 0; i < huds.Length; i++)
            {
                Canvas canvas = huds[i].GetComponentInParent<Canvas>(true);
                if (canvas == null)
                {
                    report.AddIssue($"{expectation.ScenePath}: BattleHUD '{huds[i].name}' is not under a Canvas.");
                }
            }
            MobileBattleControls[] controls =
                Object.FindObjectsByType<MobileBattleControls>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            report.CheckedContractCount++;
            report.AppendLine($"- MobileBattleControls count: {controls.Length}");

            if (controls.Length == 0 && huds.Length > 0)
            {
                report.AppendLine("- MobileBattleControls absence classified as runtime-created by BattleHUD.");
            }

            for (int i = 0; i < controls.Length; i++)
            {
                Canvas canvas = controls[i].GetComponentInParent<Canvas>(true);
                if (canvas == null)
                {
                    report.AddIssue($"{expectation.ScenePath}: MobileBattleControls '{controls[i].name}' is not under a Canvas.");
                }
            }

            PveEncounterDirector[] directors =
                Object.FindObjectsByType<PveEncounterDirector>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            report.CheckedContractCount++;
            report.AppendLine($"- PveEncounterDirector count: {directors.Length}");

            for (int i = 0; i < directors.Length; i++)
            {
                SerializedObject serializedDirector = new(directors[i]);
                SerializedProperty defaultStage = serializedDirector.FindProperty("defaultStage");
                SerializedProperty allowRuntimeStageRootBootstrap =
                    serializedDirector.FindProperty("allowRuntimeStageRootBootstrap");
                bool hasDefaultStage = defaultStage != null && defaultStage.objectReferenceValue != null;
                bool allowsRuntimeStageRootBootstrap =
                    allowRuntimeStageRootBootstrap != null && allowRuntimeStageRootBootstrap.boolValue;

                report.AppendLine(
                    $"- PveEncounterDirector '{directors[i].name}': defaultStage={(hasDefaultStage ? "set" : "null")}, runtime root bootstrap={allowsRuntimeStageRootBootstrap}");

                if (!hasDefaultStage && !allowsRuntimeStageRootBootstrap)
                {
                    report.AddIssue(
                        $"{expectation.ScenePath}: PveEncounterDirector '{directors[i].name}' has no defaultStage and no documented runtime bootstrap fallback.");
                }
            }
        }

        private static void CheckSceneRoleContract(SceneExpectation expectation, ReportBuilder report)
        {
            report.CheckedContractCount++;
            report.AppendLine($"- scene contract kind: {expectation.ContractKind}");

            switch (expectation.ContractKind)
            {
                case SceneContractKind.BossBarrageReview:
                    CheckBossBarrageReviewContract(expectation, report);
                    break;
                case SceneContractKind.UiRoute:
                    CheckUiRouteContract(expectation, report);
                    break;
                default:
                    report.AddIssue($"{expectation.ScenePath}: unknown scene contract kind {expectation.ContractKind}.");
                    break;
            }
        }

        private static void CheckBossBarrageReviewContract(SceneExpectation expectation, ReportBuilder report)
        {
            RequireSingle<BossBarragePocketReviewOwner>(expectation, report);
            RequireSingle<CombatHudPresenter>(expectation, report);
            RequireSingle<CombatHudInputBridge>(expectation, report);
            RequireSingle<BossBarrageLaneReviewCombatHudBinder>(expectation, report);
            RequireAtLeastOne<BossBarrageLaneReviewOverlayHud>(expectation, report);
        }

        private static void CheckUiRouteContract(SceneExpectation expectation, ReportBuilder report)
        {
            RequireAtLeastOne<MonoBehaviour>(expectation, report, "UI route MonoBehaviour");
            if (Object.FindObjectsByType<CombatHudPresenter>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length == 0)
            {
                report.AppendLine("- UI_CombatHudTest has no CombatHudPresenter; classified as route/transition smoke, not runtime HUD proof.");
            }
        }

        private static void RequireSingle<T>(SceneExpectation expectation, ReportBuilder report, string label = null)
            where T : Object
        {
            T[] instances = Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            string resolvedLabel = label ?? typeof(T).Name;
            report.AppendLine($"- {resolvedLabel} count: {instances.Length}");
            if (instances.Length != 1)
            {
                report.AddIssue($"{expectation.ScenePath}: expected exactly one {resolvedLabel}, found {instances.Length}.");
            }
        }

        private static void RequireAtLeastOne<T>(SceneExpectation expectation, ReportBuilder report, string label = null)
            where T : Object
        {
            T[] instances = Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            string resolvedLabel = label ?? typeof(T).Name;
            report.AppendLine($"- {resolvedLabel} count: {instances.Length}");
            if (instances.Length == 0)
            {
                report.AddIssue($"{expectation.ScenePath}: expected at least one {resolvedLabel}, found none.");
            }
        }

        private static void WriteReport(ReportBuilder report)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath));
            File.WriteAllText(ReportPath, report.ToString(), Encoding.UTF8);
        }

        private readonly struct SceneExpectation
        {
            public SceneExpectation(string scenePath, SceneContractKind contractKind)
            {
                ScenePath = scenePath;
                ContractKind = contractKind;
            }

            public string ScenePath { get; }
            public SceneContractKind ContractKind { get; }
        }

        private enum SceneContractKind
        {
            BossBarrageReview,
            UiRoute
        }

        private sealed class ReportBuilder
        {
            private readonly StringBuilder builder = new();
            private readonly List<string> issues = new();

            public int InspectedSceneCount { get; set; }
            public int CheckedContractCount { get; set; }
            public bool Passed => InspectedSceneCount > 0 && CheckedContractCount > 0 && issues.Count == 0;

            public ReportBuilder()
            {
                builder.AppendLine("# Runtime Scene Wiring Readiness Report");
                builder.AppendLine();
                builder.AppendLine("Authority: read-only reporter.");
                builder.AppendLine("Not proved: this edit-mode inspection does not prove full PlayMode behavior, input feel, HUD animation, or actual mobile touch flow.");
                builder.AppendLine();
            }

            public void AppendLine(string value = "")
            {
                builder.AppendLine(value);
            }

            public void AddIssue(string issue)
            {
                issues.Add(issue);
            }

            public void AppendSummary()
            {
                builder.AppendLine("## Summary");
                builder.AppendLine();
                builder.AppendLine($"- inspected scenes: {InspectedSceneCount}");
                builder.AppendLine($"- checked contracts: {CheckedContractCount}");
                builder.AppendLine($"- unexpected issues: {issues.Count}");
                builder.AppendLine($"- result: {(Passed ? "PASS" : "FAIL")}");
                builder.AppendLine();

                if (issues.Count == 0)
                {
                    builder.AppendLine("No unexpected scene-wiring issues were detected.");
                    return;
                }

                builder.AppendLine("Issues:");
                for (int i = 0; i < issues.Count; i++)
                {
                    builder.Append("- ");
                    builder.AppendLine(issues[i]);
                }
            }

            public override string ToString()
            {
                return builder.ToString();
            }
        }
    }
}
