using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

namespace DimensionBrawl.Editor
{
    public static class MobileContentBudgetReporter
    {
        private const string MarkdownReportPath = "C:/tmp/DimensionBrawl-MobileContentBudget.md";
        private const string JsonReportPath = "C:/tmp/DimensionBrawl-MobileContentBudget.json";
        private const long OneMebibyte = 1024L * 1024L;

        private static readonly SceneTarget[] SceneTargets =
        {
            new("Olympus Corridor", "Assets/_Game/Scenes/OlympusCorridorInvasionStage.unity"),
            new("Olympus Station", "Assets/_Game/Scenes/OlympusStationCombatStage.unity"),
            new("Boss Barrage", "Assets/_Game/Scenes/ActionFoundationBossBarrageLaneReview.unity"),
            new("Frontline Motivation", "Assets/_Game/Scenes/ActionFoundationFrontlineMotivationReview.unity")
        };

        [MenuItem("DimensionBrawl/Performance/Generate Mobile Content Budget")]
        public static void GenerateMenuReport()
        {
            GenerateBatchReport();
        }

        public static void GenerateBatchReport()
        {
            ContentBudgetReport report = new()
            {
                GeneratedUtc = DateTime.UtcNow.ToString("O"),
                UnityVersion = Application.unityVersion,
                ActiveBuildTarget = EditorUserBuildSettings.activeBuildTarget.ToString(),
                AndroidTextureSubtarget = EditorUserBuildSettings.androidBuildSubtarget.ToString()
            };
            Dictionary<string, HashSet<string>> textureSceneUsage = new(StringComparer.Ordinal);
            Dictionary<string, HashSet<string>> audioSceneUsage = new(StringComparer.Ordinal);

            for (int i = 0; i < SceneTargets.Length; i++)
            {
                SceneTarget target = SceneTargets[i];
                SceneDependencySummary scene = InspectSceneDependencies(
                    target,
                    textureSceneUsage,
                    audioSceneUsage,
                    report.Errors);
                report.Scenes.Add(scene);
            }

            foreach (KeyValuePair<string, HashSet<string>> pair in textureSceneUsage)
            {
                TextureBudgetEntry entry = InspectTexture(pair.Key, pair.Value);
                report.Textures.Add(entry);
                report.UniqueTextureRuntimeBytes += entry.EditorRuntimeBytes;
                report.UniqueTextureSourceBytes += entry.SourceBytes;
                if (entry.CandidateReasons.Count > 0)
                {
                    report.TextureCandidateCount++;
                }
            }

            foreach (KeyValuePair<string, HashSet<string>> pair in audioSceneUsage)
            {
                AudioBudgetEntry entry = InspectAudio(pair.Key, pair.Value);
                report.AudioClips.Add(entry);
                report.UniqueAudioRuntimeBytes += entry.EditorRuntimeBytes;
                report.UniqueAudioSourceBytes += entry.SourceBytes;
                if (entry.CandidateReasons.Count > 0)
                {
                    report.AudioCandidateCount++;
                }
            }

            report.Textures.Sort((left, right) => right.EditorRuntimeBytes.CompareTo(left.EditorRuntimeBytes));
            report.AudioClips.Sort((left, right) => right.EditorRuntimeBytes.CompareTo(left.EditorRuntimeBytes));
            WriteReports(report);

            if (report.Errors.Count > 0)
            {
                throw new InvalidOperationException(
                    $"Mobile content budget completed with {report.Errors.Count} error(s). See {MarkdownReportPath}");
            }

            Debug.Log(
                $"Mobile content budget written to {MarkdownReportPath} and {JsonReportPath}. " +
                $"Unique dependencies: {report.Textures.Count} textures, {report.AudioClips.Count} audio clips.");
        }

        private static SceneDependencySummary InspectSceneDependencies(
            SceneTarget target,
            Dictionary<string, HashSet<string>> textureSceneUsage,
            Dictionary<string, HashSet<string>> audioSceneUsage,
            List<string> errors)
        {
            SceneDependencySummary summary = new()
            {
                Label = target.Label,
                ScenePath = target.Path
            };
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(target.Path) == null)
            {
                errors.Add($"Scene asset is missing: {target.Path}");
                return summary;
            }

            string[] dependencies = AssetDatabase.GetDependencies(target.Path, recursive: true);
            summary.DependencyCount = dependencies.Length;
            for (int i = 0; i < dependencies.Length; i++)
            {
                string dependency = dependencies[i];
                AssetImporter importer = AssetImporter.GetAtPath(dependency);
                if (importer is TextureImporter)
                {
                    summary.TextureCount++;
                    AddSceneUsage(textureSceneUsage, dependency, target.Label);
                }
                else if (importer is AudioImporter)
                {
                    summary.AudioClipCount++;
                    AddSceneUsage(audioSceneUsage, dependency, target.Label);
                }
            }

            return summary;
        }

        private static void AddSceneUsage(
            Dictionary<string, HashSet<string>> usage,
            string assetPath,
            string sceneLabel)
        {
            if (!usage.TryGetValue(assetPath, out HashSet<string> scenes))
            {
                scenes = new HashSet<string>(StringComparer.Ordinal);
                usage.Add(assetPath, scenes);
            }

            scenes.Add(sceneLabel);
        }

        private static TextureBudgetEntry InspectTexture(string path, HashSet<string> scenes)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            Texture texture = AssetDatabase.LoadAssetAtPath<Texture>(path);
            TextureImporterPlatformSettings android = importer.GetPlatformTextureSettings("Android");
            TextureBudgetEntry entry = new()
            {
                AssetPath = path,
                SceneUsage = JoinSorted(scenes),
                SceneCount = scenes.Count,
                Width = texture != null ? texture.width : 0,
                Height = texture != null ? texture.height : 0,
                EditorRuntimeBytes = texture != null ? Profiler.GetRuntimeMemorySizeLong(texture) : 0L,
                SourceBytes = GetSourceBytes(path),
                TextureType = importer.textureType.ToString(),
                DefaultMaxSize = importer.maxTextureSize,
                Compression = importer.textureCompression.ToString(),
                CrunchCompression = importer.crunchedCompression,
                Mipmaps = importer.mipmapEnabled,
                StreamingMipmaps = importer.streamingMipmaps,
                Readable = importer.isReadable,
                Srgb = importer.sRGBTexture,
                AndroidOverride = android.overridden,
                AndroidMaxSize = android.maxTextureSize,
                AndroidFormat = android.format.ToString(),
                AndroidCompressionQuality = android.compressionQuality
            };

            if (entry.Readable)
            {
                entry.CandidateReasons.Add("Read/Write duplicates texture memory");
            }

            if (importer.textureType == TextureImporterType.Sprite && entry.Mipmaps)
            {
                entry.CandidateReasons.Add("Sprite mipmaps are enabled");
            }

            if (importer.textureCompression == TextureImporterCompression.Uncompressed
                && entry.EditorRuntimeBytes >= OneMebibyte)
            {
                entry.CandidateReasons.Add("Large texture is imported uncompressed");
            }

            int largestAxis = Math.Max(entry.Width, entry.Height);
            if (largestAxis >= 2048
                && entry.Mipmaps
                && !entry.StreamingMipmaps
                && importer.textureType == TextureImporterType.Default)
            {
                entry.CandidateReasons.Add("Large mipped texture is not streamable");
            }

            if (!entry.AndroidOverride && entry.EditorRuntimeBytes >= 4L * OneMebibyte)
            {
                entry.CandidateReasons.Add("Large texture has no explicit Android budget");
            }

            return entry;
        }

        private static AudioBudgetEntry InspectAudio(string path, HashSet<string> scenes)
        {
            AudioImporter importer = AssetImporter.GetAtPath(path) as AudioImporter;
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            AudioImporterSampleSettings defaults = importer.defaultSampleSettings;
            bool androidOverride = importer.ContainsSampleSettingsOverride("Android");
            AudioImporterSampleSettings android = androidOverride
                ? importer.GetOverrideSampleSettings("Android")
                : defaults;
            AudioBudgetEntry entry = new()
            {
                AssetPath = path,
                SceneUsage = JoinSorted(scenes),
                SceneCount = scenes.Count,
                LengthSeconds = clip != null ? clip.length : 0f,
                Channels = clip != null ? clip.channels : 0,
                Frequency = clip != null ? clip.frequency : 0,
                EditorRuntimeBytes = clip != null ? Profiler.GetRuntimeMemorySizeLong(clip) : 0L,
                SourceBytes = GetSourceBytes(path),
                LoadType = defaults.loadType.ToString(),
                CompressionFormat = defaults.compressionFormat.ToString(),
                Quality = defaults.quality,
                PreloadAudioData = defaults.preloadAudioData,
                LoadInBackground = importer.loadInBackground,
                ForceToMono = importer.forceToMono,
                AndroidOverride = androidOverride,
                AndroidLoadType = android.loadType.ToString(),
                AndroidCompressionFormat = android.compressionFormat.ToString(),
                AndroidQuality = android.quality,
                AndroidPreloadAudioData = android.preloadAudioData
            };

            if (android.compressionFormat == AudioCompressionFormat.PCM && entry.SourceBytes >= OneMebibyte)
            {
                entry.CandidateReasons.Add("Large clip uses PCM");
            }

            if (android.loadType == AudioClipLoadType.DecompressOnLoad && entry.LengthSeconds >= 10f)
            {
                entry.CandidateReasons.Add("Long clip decompresses on load");
            }

            if (android.preloadAudioData && entry.LengthSeconds >= 15f)
            {
                entry.CandidateReasons.Add("Long clip preloads audio data");
            }

            if (!entry.AndroidOverride && entry.SourceBytes >= OneMebibyte)
            {
                entry.CandidateReasons.Add("Large clip has no explicit Android budget");
            }

            return entry;
        }

        private static long GetSourceBytes(string assetPath)
        {
            string fullPath = Path.GetFullPath(assetPath);
            return File.Exists(fullPath) ? new FileInfo(fullPath).Length : 0L;
        }

        private static string JoinSorted(HashSet<string> values)
        {
            string[] sorted = new string[values.Count];
            values.CopyTo(sorted);
            Array.Sort(sorted, StringComparer.Ordinal);
            return string.Join(", ", sorted);
        }

        private static void WriteReports(ContentBudgetReport report)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(MarkdownReportPath) ?? "C:/tmp");
            File.WriteAllText(JsonReportPath, JsonUtility.ToJson(report, true), Encoding.UTF8);
            File.WriteAllText(MarkdownReportPath, BuildMarkdown(report), Encoding.UTF8);
        }

        private static string BuildMarkdown(ContentBudgetReport report)
        {
            StringBuilder builder = new();
            builder.AppendLine("# DimensionBrawl Mobile Content Budget");
            builder.AppendLine();
            builder.AppendLine($"- Generated UTC: {report.GeneratedUtc}");
            builder.AppendLine($"- Unity: {report.UnityVersion}");
            builder.AppendLine($"- Active build target: {report.ActiveBuildTarget}");
            builder.AppendLine($"- Android texture subtarget: {report.AndroidTextureSubtarget}");
            builder.AppendLine($"- Unique canonical dependencies: {report.Textures.Count:N0} textures, {report.AudioClips.Count:N0} audio clips");
            builder.AppendLine($"- Editor runtime estimates: {FormatMebibytes(report.UniqueTextureRuntimeBytes)} textures, {FormatMebibytes(report.UniqueAudioRuntimeBytes)} audio");
            builder.AppendLine($"- Source payload: {FormatMebibytes(report.UniqueTextureSourceBytes)} textures, {FormatMebibytes(report.UniqueAudioSourceBytes)} audio");
            builder.AppendLine($"- Review candidates: {report.TextureCandidateCount:N0} textures, {report.AudioCandidateCount:N0} audio clips");
            builder.AppendLine("- Scope: dependencies referenced by canonical scenes. Editor runtime memory is a comparison estimate, not an Android resident-memory capture.");
            builder.AppendLine();

            builder.AppendLine("## Scene Dependencies");
            builder.AppendLine();
            builder.AppendLine("| Scene | Dependencies | Textures | Audio clips | Path |");
            builder.AppendLine("|---|---:|---:|---:|---|");
            for (int i = 0; i < report.Scenes.Count; i++)
            {
                SceneDependencySummary scene = report.Scenes[i];
                builder.AppendLine($"| {scene.Label} | {scene.DependencyCount:N0} | {scene.TextureCount:N0} | {scene.AudioClipCount:N0} | `{scene.ScenePath}` |");
            }

            builder.AppendLine();
            builder.AppendLine("## Texture Candidates");
            builder.AppendLine();
            builder.AppendLine("| Runtime | Size | Type | Mips | Streaming | Readable | Compression | Android | Scenes | Reasons | Asset |");
            builder.AppendLine("|---:|---:|---|---|---|---|---|---|---:|---|---|");
            AppendTextureRows(builder, report.Textures, candidatesOnly: true);
            builder.AppendLine();
            builder.AppendLine("## Largest Textures");
            builder.AppendLine();
            builder.AppendLine("| Runtime | Size | Type | Mips | Streaming | Readable | Compression | Android | Scenes | Reasons | Asset |");
            builder.AppendLine("|---:|---:|---|---|---|---|---|---|---:|---|---|");
            AppendTextureRows(builder, report.Textures, candidatesOnly: false, maximumRows: 60);

            builder.AppendLine();
            builder.AppendLine("## Audio Candidates");
            builder.AppendLine();
            builder.AppendLine("| Runtime | Length | Load | Format | Preload | Background | Android | Scenes | Reasons | Asset |");
            builder.AppendLine("|---:|---:|---|---|---|---|---|---:|---|---|");
            AppendAudioRows(builder, report.AudioClips, candidatesOnly: true);
            builder.AppendLine();
            builder.AppendLine("## Largest Audio Clips");
            builder.AppendLine();
            builder.AppendLine("| Runtime | Length | Load | Format | Preload | Background | Android | Scenes | Reasons | Asset |");
            builder.AppendLine("|---:|---:|---|---|---|---|---|---:|---|---|");
            AppendAudioRows(builder, report.AudioClips, candidatesOnly: false, maximumRows: 40);

            if (report.Errors.Count > 0)
            {
                builder.AppendLine();
                builder.AppendLine("## Errors");
                builder.AppendLine();
                for (int i = 0; i < report.Errors.Count; i++)
                {
                    builder.AppendLine($"- {report.Errors[i]}");
                }
            }

            return builder.ToString();
        }

        private static void AppendTextureRows(
            StringBuilder builder,
            List<TextureBudgetEntry> entries,
            bool candidatesOnly,
            int maximumRows = int.MaxValue)
        {
            int appended = 0;
            for (int i = 0; i < entries.Count && appended < maximumRows; i++)
            {
                TextureBudgetEntry entry = entries[i];
                if (candidatesOnly && entry.CandidateReasons.Count == 0)
                {
                    continue;
                }

                string android = entry.AndroidOverride
                    ? $"{entry.AndroidMaxSize}/{entry.AndroidFormat}"
                    : "default";
                builder.AppendLine(
                    $"| {FormatMebibytes(entry.EditorRuntimeBytes)} | {entry.Width}x{entry.Height} | {entry.TextureType} | " +
                    $"{entry.Mipmaps} | {entry.StreamingMipmaps} | {entry.Readable} | {entry.Compression} | {android} | " +
                    $"{entry.SceneCount} | {EscapeCell(string.Join("; ", entry.CandidateReasons))} | `{entry.AssetPath}` |");
                appended++;
            }
        }

        private static void AppendAudioRows(
            StringBuilder builder,
            List<AudioBudgetEntry> entries,
            bool candidatesOnly,
            int maximumRows = int.MaxValue)
        {
            int appended = 0;
            for (int i = 0; i < entries.Count && appended < maximumRows; i++)
            {
                AudioBudgetEntry entry = entries[i];
                if (candidatesOnly && entry.CandidateReasons.Count == 0)
                {
                    continue;
                }

                string android = entry.AndroidOverride
                    ? $"{entry.AndroidLoadType}/{entry.AndroidCompressionFormat}"
                    : "default";
                builder.AppendLine(
                    $"| {FormatMebibytes(entry.EditorRuntimeBytes)} | {entry.LengthSeconds:0.0}s | {entry.LoadType} | " +
                    $"{entry.CompressionFormat} | {entry.PreloadAudioData} | {entry.LoadInBackground} | {android} | " +
                    $"{entry.SceneCount} | {EscapeCell(string.Join("; ", entry.CandidateReasons))} | `{entry.AssetPath}` |");
                appended++;
            }
        }

        private static string EscapeCell(string value)
        {
            return string.IsNullOrEmpty(value) ? "-" : value.Replace("|", "\\|");
        }

        private static string FormatMebibytes(long bytes)
        {
            return $"{bytes / (1024d * 1024d):N2} MiB";
        }

        private readonly struct SceneTarget
        {
            public SceneTarget(string label, string path)
            {
                Label = label;
                Path = path;
            }

            public string Label { get; }
            public string Path { get; }
        }

        [Serializable]
        private sealed class ContentBudgetReport
        {
            public string GeneratedUtc;
            public string UnityVersion;
            public string ActiveBuildTarget;
            public string AndroidTextureSubtarget;
            public long UniqueTextureRuntimeBytes;
            public long UniqueTextureSourceBytes;
            public long UniqueAudioRuntimeBytes;
            public long UniqueAudioSourceBytes;
            public int TextureCandidateCount;
            public int AudioCandidateCount;
            public List<SceneDependencySummary> Scenes = new();
            public List<TextureBudgetEntry> Textures = new();
            public List<AudioBudgetEntry> AudioClips = new();
            public List<string> Errors = new();
        }

        [Serializable]
        private sealed class SceneDependencySummary
        {
            public string Label;
            public string ScenePath;
            public int DependencyCount;
            public int TextureCount;
            public int AudioClipCount;
        }

        [Serializable]
        private sealed class TextureBudgetEntry
        {
            public string AssetPath;
            public string SceneUsage;
            public int SceneCount;
            public int Width;
            public int Height;
            public long EditorRuntimeBytes;
            public long SourceBytes;
            public string TextureType;
            public int DefaultMaxSize;
            public string Compression;
            public bool CrunchCompression;
            public bool Mipmaps;
            public bool StreamingMipmaps;
            public bool Readable;
            public bool Srgb;
            public bool AndroidOverride;
            public int AndroidMaxSize;
            public string AndroidFormat;
            public int AndroidCompressionQuality;
            public List<string> CandidateReasons = new();
        }

        [Serializable]
        private sealed class AudioBudgetEntry
        {
            public string AssetPath;
            public string SceneUsage;
            public int SceneCount;
            public float LengthSeconds;
            public int Channels;
            public int Frequency;
            public long EditorRuntimeBytes;
            public long SourceBytes;
            public string LoadType;
            public string CompressionFormat;
            public float Quality;
            public bool PreloadAudioData;
            public bool LoadInBackground;
            public bool ForceToMono;
            public bool AndroidOverride;
            public string AndroidLoadType;
            public string AndroidCompressionFormat;
            public float AndroidQuality;
            public bool AndroidPreloadAudioData;
            public List<string> CandidateReasons = new();
        }
    }
}
