using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace DimensionBrawl.Editor.AuditionPV
{
    [Serializable]
    internal sealed class AuditionPvCaptureManifest
    {
        public string schemaVersion = AuditionPvCaptureContract.SchemaVersion;
        public string captureId = string.Empty;
        public string createdAtUtc = string.Empty;
        public string outputRoot = string.Empty;
        public string outputDirectory = string.Empty;
        public string sourceFormat = AuditionPvCaptureContract.SourceFormat;
        public int width = AuditionPvCaptureContract.Width;
        public int height = AuditionPvCaptureContract.Height;
        public int fps = AuditionPvCaptureContract.Fps;
        public string gitCommitSha = string.Empty;
        public string gitBranch = string.Empty;
        public bool gitWorktreeDirty;
        public string worktreeDirtyHashSha256 = string.Empty;
        public string worktreeDirtyHashAlgorithm = AuditionPvGitSnapshot.DirtyHashAlgorithm;
        public string unityVersion = string.Empty;
        public string unityVersionWithRevision = string.Empty;
        public string recorderPackageVersion = string.Empty;
        public string urpPackageVersion = string.Empty;
        public string activeRenderPipelineAssetPath = string.Empty;
        public AuditionPvShotManifestEntry[] shots = Array.Empty<AuditionPvShotManifestEntry>();
        public AuditionPvBaselineManifestEntry[] baselines = Array.Empty<AuditionPvBaselineManifestEntry>();
        public AuditionPvDependencyHash[] dependencyHashes = Array.Empty<AuditionPvDependencyHash>();
        public AuditionPvTestResult[] testResults = Array.Empty<AuditionPvTestResult>();
    }

    [Serializable]
    internal sealed class AuditionPvShotManifestEntry
    {
        public string id = string.Empty;
        public string scenePath = string.Empty;
        public int startFrame;
        public int endFrame;
        public int expectedFrameCount;
        public string hudMode = string.Empty;
        public string notes = string.Empty;
    }

    [Serializable]
    internal sealed class AuditionPvBaselineManifestEntry
    {
        public string id = string.Empty;
        public string shotId = string.Empty;
        public int sourceFrame;
        public string fileName = string.Empty;
        public string hudMode = string.Empty;
        public string status = string.Empty;
    }

    [Serializable]
    internal sealed class AuditionPvDependencyHash
    {
        public string path = string.Empty;
        public bool exists;
        public long byteLength;
        public string sha256 = string.Empty;
    }

    [Serializable]
    internal sealed class AuditionPvTestResult
    {
        public string suite = string.Empty;
        public string name = string.Empty;
        public string status = string.Empty;
        public long durationMilliseconds;
        public string details = string.Empty;
        public string artifactPath = string.Empty;
    }

    internal static class AuditionPvCaptureManifestFactory
    {
        public static AuditionPvCaptureManifest Create(
            string captureId,
            string outputDirectory,
            IEnumerable<AuditionPvShotManifestEntry> shots,
            IEnumerable<AuditionPvBaselineManifestEntry> baselines,
            IEnumerable<AuditionPvTestResult> testResults,
            IEnumerable<string> additionalDependencyPaths = null,
            DateTime? createdAtUtc = null)
        {
            return CreateForRoot(
                captureId,
                AuditionPvCaptureContract.OutputRoot,
                outputDirectory,
                shots,
                baselines,
                testResults,
                additionalDependencyPaths,
                createdAtUtc);
        }

        internal static AuditionPvCaptureManifest CreateForRoot(
            string captureId,
            string outputRoot,
            string outputDirectory,
            IEnumerable<AuditionPvShotManifestEntry> shots,
            IEnumerable<AuditionPvBaselineManifestEntry> baselines,
            IEnumerable<AuditionPvTestResult> testResults,
            IEnumerable<string> additionalDependencyPaths = null,
            DateTime? createdAtUtc = null,
            AuditionPvGitSnapshot gitSnapshot = null,
            AuditionPvEngineSnapshot engineSnapshot = null,
            AuditionPvDependencyHash[] dependencyHashSnapshot = null)
        {
            string expectedDirectory = AuditionPvOutputPaths.ResolveOutputDirectory(outputRoot, captureId);
            string normalizedOutputDirectory = Path.GetFullPath(outputDirectory).Replace('\\', '/').TrimEnd('/');
            if (!string.Equals(expectedDirectory.TrimEnd('/'), normalizedOutputDirectory, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Manifest output directory must match its output root and capture ID.", nameof(outputDirectory));
            }

            AuditionPvGitSnapshot git = gitSnapshot ?? AuditionPvEnvironmentProbe.ReadGitSnapshot();
            AuditionPvEngineSnapshot engine = engineSnapshot ?? AuditionPvEnvironmentProbe.ReadEngineSnapshot();
            AuditionPvDependencyHash[] dependencyHashes = dependencyHashSnapshot;
            if (dependencyHashes == null)
            {
                string[] dependencyPaths = AuditionPvEnvironmentProbe.CollectCaptureDependencyPaths(additionalDependencyPaths);
                dependencyHashes = AuditionPvEnvironmentProbe.HashDependencies(dependencyPaths);
            }

            return new AuditionPvCaptureManifest
            {
                captureId = captureId,
                createdAtUtc = (createdAtUtc ?? DateTime.UtcNow).ToUniversalTime().ToString("O"),
                outputRoot = Path.GetFullPath(outputRoot).Replace('\\', '/').TrimEnd('/'),
                outputDirectory = normalizedOutputDirectory,
                gitCommitSha = git.commitSha,
                gitBranch = git.branch,
                gitWorktreeDirty = git.isDirty,
                worktreeDirtyHashSha256 = git.dirtyStateHashSha256,
                unityVersion = engine.unityVersion,
                unityVersionWithRevision = engine.unityVersionWithRevision,
                recorderPackageVersion = engine.recorderPackageVersion,
                urpPackageVersion = engine.urpPackageVersion,
                activeRenderPipelineAssetPath = engine.activeRenderPipelineAssetPath,
                shots = (shots ?? Array.Empty<AuditionPvShotManifestEntry>()).ToArray(),
                baselines = (baselines ?? Array.Empty<AuditionPvBaselineManifestEntry>()).ToArray(),
                dependencyHashes = dependencyHashes,
                testResults = (testResults ?? Array.Empty<AuditionPvTestResult>()).ToArray()
            };
        }
    }

    internal static class AuditionPvCaptureManifestWriter
    {
        public static string WriteNew(AuditionPvCaptureManifest manifest)
        {
            Validate(manifest);
            Directory.CreateDirectory(manifest.outputDirectory);

            string manifestPath = Path.Combine(manifest.outputDirectory, AuditionPvCaptureContract.ManifestFileName);
            if (File.Exists(manifestPath))
            {
                throw new IOException($"Capture manifest already exists and will not be overwritten: {manifestPath}");
            }

            string temporaryPath = manifestPath + ".tmp-" + Guid.NewGuid().ToString("N");
            try
            {
                string json = JsonUtility.ToJson(manifest, true) + Environment.NewLine;
                File.WriteAllText(temporaryPath, json, new UTF8Encoding(false));
                File.Move(temporaryPath, manifestPath);
                return manifestPath.Replace('\\', '/');
            }
            finally
            {
                if (File.Exists(temporaryPath))
                {
                    File.Delete(temporaryPath);
                }
            }
        }

        public static void Validate(AuditionPvCaptureManifest manifest)
        {
            if (manifest == null)
            {
                throw new ArgumentNullException(nameof(manifest));
            }

            if (!string.Equals(manifest.schemaVersion, AuditionPvCaptureContract.SchemaVersion, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Unsupported audition PV capture manifest schema.");
            }

            AuditionPvOutputPaths.ValidateOutputId(manifest.captureId);
            string expectedDirectory = AuditionPvOutputPaths.ResolveOutputDirectory(manifest.outputRoot, manifest.captureId);
            string actualDirectory = Path.GetFullPath(manifest.outputDirectory).Replace('\\', '/').TrimEnd('/');
            if (!string.Equals(expectedDirectory.TrimEnd('/'), actualDirectory, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Manifest output directory is outside its declared output root.");
            }

            if (manifest.width != AuditionPvCaptureContract.Width ||
                manifest.height != AuditionPvCaptureContract.Height ||
                manifest.fps != AuditionPvCaptureContract.Fps ||
                !string.Equals(manifest.sourceFormat, AuditionPvCaptureContract.SourceFormat, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Manifest source settings do not match the audition PV golden-source contract.");
            }

            if (!AuditionPvSha256.IsSha256(manifest.worktreeDirtyHashSha256))
            {
                throw new InvalidOperationException("Manifest is missing a valid worktree dirty-state SHA-256.");
            }

            if (string.IsNullOrWhiteSpace(manifest.gitCommitSha) ||
                manifest.gitCommitSha.Length < 7 ||
                manifest.gitCommitSha.Any(character =>
                    !(character >= '0' && character <= '9' || character >= 'a' && character <= 'f')))
            {
                throw new InvalidOperationException("Manifest is missing a valid Git commit SHA.");
            }

            if (string.IsNullOrWhiteSpace(manifest.gitBranch) ||
                string.IsNullOrWhiteSpace(manifest.unityVersion) ||
                string.IsNullOrWhiteSpace(manifest.unityVersionWithRevision) ||
                string.IsNullOrWhiteSpace(manifest.activeRenderPipelineAssetPath) ||
                !string.Equals(
                    manifest.recorderPackageVersion,
                    AuditionPvCaptureContract.RecorderPackageVersion,
                    StringComparison.Ordinal) ||
                string.IsNullOrWhiteSpace(manifest.urpPackageVersion) ||
                string.Equals(manifest.urpPackageVersion, "missing", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Manifest is missing required Unity, Recorder, URP, or Git provenance.");
            }

            if (!DateTime.TryParse(
                    manifest.createdAtUtc,
                    null,
                    System.Globalization.DateTimeStyles.RoundtripKind,
                    out _))
            {
                throw new InvalidOperationException("Manifest creation time must be an ISO-8601 round-trip timestamp.");
            }

            manifest.shots ??= Array.Empty<AuditionPvShotManifestEntry>();
            manifest.baselines ??= Array.Empty<AuditionPvBaselineManifestEntry>();
            manifest.dependencyHashes ??= Array.Empty<AuditionPvDependencyHash>();
            manifest.testResults ??= Array.Empty<AuditionPvTestResult>();
            if (manifest.shots.Length == 0 ||
                manifest.baselines.Length == 0 ||
                manifest.dependencyHashes.Length == 0 ||
                manifest.testResults.Length == 0)
            {
                throw new InvalidOperationException(
                    "A finalized capture manifest must include shots, baselines, dependency hashes, and test results.");
            }

            HashSet<string> shotIds = new(StringComparer.Ordinal);
            foreach (AuditionPvShotManifestEntry shot in manifest.shots)
            {
                if (shot == null || string.IsNullOrWhiteSpace(shot.id) || !shotIds.Add(shot.id))
                {
                    throw new InvalidOperationException("Manifest shot IDs must be non-empty and unique.");
                }

                int expectedFrameCount = shot.endFrame - shot.startFrame + 1;
                if (shot.startFrame < 0 || shot.endFrame < shot.startFrame || shot.expectedFrameCount != expectedFrameCount)
                {
                    throw new InvalidOperationException($"Shot '{shot.id}' has an invalid deterministic frame interval.");
                }
            }

            HashSet<string> baselineIds = new(StringComparer.Ordinal);
            foreach (AuditionPvBaselineManifestEntry baseline in manifest.baselines)
            {
                if (baseline == null || string.IsNullOrWhiteSpace(baseline.id) || !baselineIds.Add(baseline.id))
                {
                    throw new InvalidOperationException("Manifest baseline IDs must be non-empty and unique.");
                }

                if (!shotIds.Contains(baseline.shotId))
                {
                    throw new InvalidOperationException($"Baseline '{baseline.id}' references unknown shot '{baseline.shotId}'.");
                }

                AuditionPvShotManifestEntry sourceShot = manifest.shots.First(shot => shot.id == baseline.shotId);
                if (baseline.sourceFrame < sourceShot.startFrame || baseline.sourceFrame > sourceShot.endFrame)
                {
                    throw new InvalidOperationException($"Baseline '{baseline.id}' frame is outside its source shot interval.");
                }
            }

            foreach (AuditionPvDependencyHash dependency in manifest.dependencyHashes)
            {
                if (dependency == null || string.IsNullOrWhiteSpace(dependency.path))
                {
                    throw new InvalidOperationException("Manifest contains an invalid dependency hash entry.");
                }

                if (dependency.exists && !AuditionPvSha256.IsSha256(dependency.sha256))
                {
                    throw new InvalidOperationException($"Dependency '{dependency.path}' is missing a valid SHA-256.");
                }
            }

            AuditionPvDependencyHash pipelineDependency = manifest.dependencyHashes.FirstOrDefault(dependency =>
                string.Equals(
                    dependency.path,
                    manifest.activeRenderPipelineAssetPath,
                    StringComparison.OrdinalIgnoreCase));
            if (pipelineDependency == null || !pipelineDependency.exists || !AuditionPvSha256.IsSha256(pipelineDependency.sha256))
            {
                throw new InvalidOperationException("Manifest must hash the active render pipeline asset.");
            }
        }
    }
}
