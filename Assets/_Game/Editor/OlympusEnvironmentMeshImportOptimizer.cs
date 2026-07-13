using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;

namespace DimensionBrawl.Editor
{
    public static class OlympusEnvironmentMeshImportOptimizer
    {
        private const string EnvironmentAssetRoot =
            "Assets/_Game/Art/Environment/OlympusTemple/";
        private const string ReportPath =
            "C:/tmp/DimensionBrawl-OlympusEnvironmentMeshImportOptimization.md";

        private static readonly string[] CanonicalScenePaths =
        {
            "Assets/_Game/Scenes/OlympusCorridorInvasionStage.unity"
        };

        [MenuItem("DimensionBrawl/Performance/Apply Olympus Render-Only Mesh Memory Optimization")]
        public static void ApplyMenuOptimization()
        {
            ApplyBatchOptimization();
        }

        public static void ApplyBatchOptimization()
        {
            if (HasDirtyOpenScene(out string dirtyScenePath))
            {
                throw new InvalidOperationException(
                    $"Cannot inspect canonical scenes while an open scene is dirty: {dirtyScenePath}");
            }

            SceneSetup[] originalSetup = EditorSceneManager.GetSceneManagerSetup();
            UsageSnapshot snapshot = new();
            try
            {
                for (int i = 0; i < CanonicalScenePaths.Length; i++)
                {
                    CollectSceneUsage(CanonicalScenePaths[i], snapshot);
                }
            }
            finally
            {
                if (originalSetup.Length > 0)
                {
                    EditorSceneManager.RestoreSceneManagerSetup(originalSetup);
                }
            }

            List<string> candidates = BuildCandidates(snapshot);
            List<string> changed = new();
            List<string> alreadyOptimized = new();
            List<string> invalidImporters = new();

            for (int i = 0; i < candidates.Count; i++)
            {
                string assetPath = candidates[i];
                ModelImporter importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
                if (importer == null)
                {
                    invalidImporters.Add(assetPath);
                    continue;
                }

                if (!importer.isReadable)
                {
                    alreadyOptimized.Add(assetPath);
                    continue;
                }

                importer.isReadable = false;
                importer.SaveAndReimport();
                changed.Add(assetPath);
            }

            AssetDatabase.SaveAssets();
            WriteReport(snapshot, candidates, changed, alreadyOptimized, invalidImporters);

            if (invalidImporters.Count > 0)
            {
                throw new InvalidOperationException(
                    $"{invalidImporters.Count} Olympus mesh candidate(s) did not have a ModelImporter. See {ReportPath}");
            }

            for (int i = 0; i < candidates.Count; i++)
            {
                ModelImporter importer = AssetImporter.GetAtPath(candidates[i]) as ModelImporter;
                if (importer != null && importer.isReadable)
                {
                    throw new InvalidOperationException(
                        $"Olympus render-only mesh remained readable after optimization: {candidates[i]}");
                }
            }

            Debug.Log(
                $"Olympus render-only mesh memory optimization completed: {changed.Count} changed, " +
                $"{alreadyOptimized.Count} already optimized, {snapshot.ColliderAssetPaths.Count} collision asset paths preserved. " +
                $"See {ReportPath}");
        }

        private static bool HasDirtyOpenScene(out string dirtyScenePath)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (!scene.isDirty)
                {
                    continue;
                }

                dirtyScenePath = string.IsNullOrWhiteSpace(scene.path) ? scene.name : scene.path;
                return true;
            }

            dirtyScenePath = string.Empty;
            return false;
        }

        private static void CollectSceneUsage(string scenePath, UsageSnapshot snapshot)
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                MeshRenderer[] meshRenderers = roots[i].GetComponentsInChildren<MeshRenderer>(true);
                for (int rendererIndex = 0; rendererIndex < meshRenderers.Length; rendererIndex++)
                {
                    MeshFilter meshFilter = meshRenderers[rendererIndex].GetComponent<MeshFilter>();
                    CollectRenderedMesh(meshFilter != null ? meshFilter.sharedMesh : null, snapshot);
                }

                SkinnedMeshRenderer[] skinnedRenderers =
                    roots[i].GetComponentsInChildren<SkinnedMeshRenderer>(true);
                for (int rendererIndex = 0; rendererIndex < skinnedRenderers.Length; rendererIndex++)
                {
                    Mesh mesh = skinnedRenderers[rendererIndex].sharedMesh;
                    string assetPath = GetEnvironmentModelPath(mesh);
                    if (!string.IsNullOrEmpty(assetPath))
                    {
                        snapshot.RuntimeSensitiveAssetPaths.Add(assetPath);
                    }
                }

                MeshCollider[] meshColliders = roots[i].GetComponentsInChildren<MeshCollider>(true);
                for (int colliderIndex = 0; colliderIndex < meshColliders.Length; colliderIndex++)
                {
                    string assetPath = GetEnvironmentModelPath(meshColliders[colliderIndex].sharedMesh);
                    if (!string.IsNullOrEmpty(assetPath))
                    {
                        snapshot.ColliderAssetPaths.Add(assetPath);
                    }
                }
            }
        }

        private static void CollectRenderedMesh(Mesh mesh, UsageSnapshot snapshot)
        {
            string assetPath = GetEnvironmentModelPath(mesh);
            if (string.IsNullOrEmpty(assetPath))
            {
                return;
            }

            snapshot.RenderAssetPaths.Add(assetPath);
            if (mesh == null || !snapshot.MeasuredMeshIds.Add(mesh.GetInstanceID()))
            {
                return;
            }

            long bytes = Profiler.GetRuntimeMemorySizeLong(mesh);
            snapshot.RenderMeshRuntimeBytes += bytes;
            if (!snapshot.RuntimeBytesByAssetPath.TryGetValue(assetPath, out long assetBytes))
            {
                assetBytes = 0L;
            }

            snapshot.RuntimeBytesByAssetPath[assetPath] = assetBytes + bytes;
        }

        private static string GetEnvironmentModelPath(Mesh mesh)
        {
            if (mesh == null)
            {
                return string.Empty;
            }

            string assetPath = AssetDatabase.GetAssetPath(mesh);
            return assetPath.StartsWith(EnvironmentAssetRoot, StringComparison.OrdinalIgnoreCase)
                && assetPath.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase)
                ? assetPath
                : string.Empty;
        }

        private static List<string> BuildCandidates(UsageSnapshot snapshot)
        {
            List<string> candidates = new();
            foreach (string assetPath in snapshot.RenderAssetPaths)
            {
                if (snapshot.ColliderAssetPaths.Contains(assetPath)
                    || snapshot.RuntimeSensitiveAssetPaths.Contains(assetPath))
                {
                    continue;
                }

                candidates.Add(assetPath);
            }

            candidates.Sort(StringComparer.Ordinal);
            return candidates;
        }

        private static void WriteReport(
            UsageSnapshot snapshot,
            List<string> candidates,
            List<string> changed,
            List<string> alreadyOptimized,
            List<string> invalidImporters)
        {
            long candidateRuntimeBytes = 0L;
            for (int i = 0; i < candidates.Count; i++)
            {
                if (snapshot.RuntimeBytesByAssetPath.TryGetValue(candidates[i], out long bytes))
                {
                    candidateRuntimeBytes += bytes;
                }
            }

            StringBuilder builder = new();
            builder.AppendLine("# Olympus Environment Mesh Import Optimization");
            builder.AppendLine();
            builder.AppendLine($"- Generated UTC: {DateTime.UtcNow:O}");
            builder.AppendLine($"- Canonical scenes inspected: {CanonicalScenePaths.Length}");
            builder.AppendLine($"- Environment render model assets: {snapshot.RenderAssetPaths.Count}");
            builder.AppendLine($"- Environment MeshCollider model assets preserved: {snapshot.ColliderAssetPaths.Count}");
            builder.AppendLine($"- Runtime-sensitive/skinned model assets preserved: {snapshot.RuntimeSensitiveAssetPaths.Count}");
            builder.AppendLine($"- Render-only candidate model assets: {candidates.Count}");
            builder.AppendLine($"- Candidate measured mesh footprint: {candidateRuntimeBytes / (1024d * 1024d):N2} MiB");
            builder.AppendLine($"- Changed this run: {changed.Count}");
            builder.AppendLine($"- Already optimized: {alreadyOptimized.Count}");
            builder.AppendLine($"- Invalid importers: {invalidImporters.Count}");
            builder.AppendLine();
            builder.AppendLine("Read/Write is disabled only when the FBX is rendered by a canonical Olympus scene and no mesh from that FBX is referenced by a MeshCollider or SkinnedMeshRenderer in either canonical scene.");
            builder.AppendLine();
            builder.AppendLine("## Candidates");
            builder.AppendLine();
            for (int i = 0; i < candidates.Count; i++)
            {
                snapshot.RuntimeBytesByAssetPath.TryGetValue(candidates[i], out long bytes);
                string state = changed.Contains(candidates[i]) ? "changed" : "already optimized";
                builder.AppendLine($"- `{candidates[i]}` ({bytes / (1024d * 1024d):N2} MiB, {state})");
            }

            if (snapshot.ColliderAssetPaths.Count > 0)
            {
                builder.AppendLine();
                builder.AppendLine("## Preserved Collision Assets");
                builder.AppendLine();
                List<string> colliderPaths = new(snapshot.ColliderAssetPaths);
                colliderPaths.Sort(StringComparer.Ordinal);
                for (int i = 0; i < colliderPaths.Count; i++)
                {
                    builder.AppendLine($"- `{colliderPaths[i]}`");
                }
            }

            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath) ?? "C:/tmp");
            File.WriteAllText(ReportPath, builder.ToString(), Encoding.UTF8);
        }

        private sealed class UsageSnapshot
        {
            public readonly HashSet<string> RenderAssetPaths = new(StringComparer.Ordinal);
            public readonly HashSet<string> ColliderAssetPaths = new(StringComparer.Ordinal);
            public readonly HashSet<string> RuntimeSensitiveAssetPaths = new(StringComparer.Ordinal);
            public readonly HashSet<int> MeasuredMeshIds = new();
            public readonly Dictionary<string, long> RuntimeBytesByAssetPath = new(StringComparer.Ordinal);
            public long RenderMeshRuntimeBytes;
        }
    }
}
