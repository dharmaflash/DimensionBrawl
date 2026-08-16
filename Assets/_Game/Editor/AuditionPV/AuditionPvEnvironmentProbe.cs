using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Debug = UnityEngine.Debug;
using PackageManagerPackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace DimensionBrawl.Editor.AuditionPV
{
    internal sealed class AuditionPvGitSnapshot
    {
        public const string DirtyHashAlgorithm = "sha256(head + git-status-porcelain-v1-z + sorted working-file sha256)";

        public string commitSha = string.Empty;
        public string branch = string.Empty;
        public bool isDirty;
        public string dirtyStateHashSha256 = string.Empty;
        public bool probeSucceeded;
        public string probeError = string.Empty;
    }

    [Serializable]
    internal sealed class AuditionPvEngineSnapshot
    {
        public string unityVersion = string.Empty;
        public string unityVersionWithRevision = string.Empty;
        public string recorderPackageVersion = string.Empty;
        public string urpPackageVersion = string.Empty;
        public string activeRenderPipelineAssetPath = string.Empty;
    }

    internal static class AuditionPvEnvironmentProbe
    {
        private static readonly StringComparer PathComparer = StringComparer.OrdinalIgnoreCase;

        public static AuditionPvGitSnapshot ReadGitSnapshot()
        {
            string projectRoot = ProjectRoot;
            var snapshot = new AuditionPvGitSnapshot();

            bool commitSucceeded = TryRunGit(projectRoot, "rev-parse HEAD", out string commitSha, out string commitError);
            bool branchSucceeded = TryRunGit(projectRoot, "branch --show-current", out string branch, out string branchError);
            bool statusSucceeded = TryRunGit(
                projectRoot,
                "status --porcelain=v1 -z --untracked-files=all",
                out string status,
                out string statusError);
            if (!commitSucceeded || !branchSucceeded || !statusSucceeded)
            {
                snapshot.commitSha = "unknown";
                snapshot.branch = "unknown";
                snapshot.isDirty = true;
                snapshot.probeSucceeded = false;
                snapshot.probeError = string.Join(" | ", new[] { commitError, branchError, statusError }
                    .Where(value => !string.IsNullOrWhiteSpace(value)));
                snapshot.dirtyStateHashSha256 = ComputeDirtyStateHash(
                    snapshot.commitSha,
                    "probe-failed:" + snapshot.probeError,
                    Array.Empty<string>());
                return snapshot;
            }

            snapshot.commitSha = commitSha.Trim();
            snapshot.branch = string.IsNullOrWhiteSpace(branch) ? "detached" : branch.Trim();
            snapshot.isDirty = !string.IsNullOrEmpty(status);
            snapshot.probeSucceeded = true;

            var fileStates = new List<string>();
            foreach (string relativePath in ExtractDirtyPaths(status).Distinct(PathComparer).OrderBy(path => path, PathComparer))
            {
                string absolutePath = Path.GetFullPath(Path.Combine(projectRoot, relativePath));
                string normalizedProjectRoot = Path.GetFullPath(projectRoot)
                    .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
                if (!absolutePath.StartsWith(normalizedProjectRoot, StringComparison.OrdinalIgnoreCase))
                {
                    fileStates.Add(relativePath.Replace('\\', '/') + "|outside-project");
                    continue;
                }

                if (File.Exists(absolutePath))
                {
                    var info = new FileInfo(absolutePath);
                    fileStates.Add(relativePath.Replace('\\', '/') + "|" + info.Length + "|" + AuditionPvSha256.FileHash(absolutePath));
                }
                else
                {
                    fileStates.Add(relativePath.Replace('\\', '/') + "|missing");
                }
            }

            snapshot.dirtyStateHashSha256 = ComputeDirtyStateHash(snapshot.commitSha, status, fileStates);
            return snapshot;
        }

        public static AuditionPvEngineSnapshot ReadEngineSnapshot()
        {
            PackageManagerPackageInfo recorder =
                PackageManagerPackageInfo.FindForPackageName(AuditionPvCaptureContract.RecorderPackageName);
            PackageManagerPackageInfo urp =
                PackageManagerPackageInfo.FindForPackageName(AuditionPvCaptureContract.UrpPackageName);
            RenderPipelineAsset pipeline = GraphicsSettings.currentRenderPipeline;

            return new AuditionPvEngineSnapshot
            {
                unityVersion = Application.unityVersion,
                unityVersionWithRevision = ReadUnityVersionWithRevision(),
                recorderPackageVersion = recorder?.version ?? "missing",
                urpPackageVersion = urp?.version ?? "missing",
                activeRenderPipelineAssetPath = pipeline != null ? AssetDatabase.GetAssetPath(pipeline) : string.Empty
            };
        }

        public static string[] CollectCaptureDependencyPaths(IEnumerable<string> additionalDependencyPaths = null)
        {
            var paths = new HashSet<string>(AuditionPvCaptureContract.CoreDependencyPaths, PathComparer);
            RenderPipelineAsset pipeline = GraphicsSettings.currentRenderPipeline;
            string pipelinePath = pipeline != null ? AssetDatabase.GetAssetPath(pipeline) : string.Empty;
            if (!string.IsNullOrWhiteSpace(pipelinePath))
            {
                foreach (string dependency in AssetDatabase.GetDependencies(pipelinePath, true))
                {
                    paths.Add(dependency.Replace('\\', '/'));
                }
            }

            if (additionalDependencyPaths != null)
            {
                foreach (string dependency in additionalDependencyPaths.Where(value => !string.IsNullOrWhiteSpace(value)))
                {
                    paths.Add(dependency.Replace('\\', '/'));
                }
            }

            return paths.OrderBy(path => path, PathComparer).ToArray();
        }

        public static AuditionPvDependencyHash[] HashDependencies(IEnumerable<string> dependencyPaths)
        {
            var results = new Dictionary<string, AuditionPvDependencyHash>(PathComparer);
            foreach (string path in (dependencyPaths ?? Array.Empty<string>())
                         .Where(value => !string.IsNullOrWhiteSpace(value))
                         .Distinct(PathComparer)
                         .OrderBy(value => value, PathComparer))
            {
                string normalizedPath = path.Replace('\\', '/');
                string absolutePath = ResolveProjectOrPackagePath(normalizedPath);
                bool exists = !string.IsNullOrWhiteSpace(absolutePath) && File.Exists(absolutePath);
                results[normalizedPath] = new AuditionPvDependencyHash
                {
                    path = normalizedPath,
                    exists = exists,
                    byteLength = exists ? new FileInfo(absolutePath).Length : 0,
                    sha256 = exists ? AuditionPvSha256.FileHash(absolutePath) : string.Empty
                };

                if (!exists || normalizedPath.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string metaPath = normalizedPath + ".meta";
                string absoluteMetaPath = ResolveProjectOrPackagePath(metaPath);
                if (!string.IsNullOrWhiteSpace(absoluteMetaPath) && File.Exists(absoluteMetaPath))
                {
                    results[metaPath] = new AuditionPvDependencyHash
                    {
                        path = metaPath,
                        exists = true,
                        byteLength = new FileInfo(absoluteMetaPath).Length,
                        sha256 = AuditionPvSha256.FileHash(absoluteMetaPath)
                    };
                }
            }

            return results.Values.OrderBy(result => result.path, PathComparer).ToArray();
        }

        internal static string ComputeDirtyStateHash(
            string commitSha,
            string porcelainStatus,
            IEnumerable<string> sortedWorkingFileStates)
        {
            var material = new StringBuilder();
            material.Append("head\0").Append(commitSha ?? string.Empty).Append("\0status\0");
            material.Append(porcelainStatus ?? string.Empty).Append("\0files\0");
            foreach (string state in (sortedWorkingFileStates ?? Array.Empty<string>()).OrderBy(value => value, StringComparer.Ordinal))
            {
                material.Append(state).Append('\0');
            }

            return AuditionPvSha256.TextHash(material.ToString());
        }

        internal static IEnumerable<string> ExtractDirtyPaths(string porcelainStatus)
        {
            if (string.IsNullOrEmpty(porcelainStatus))
            {
                yield break;
            }

            string[] tokens = porcelainStatus.Split(new[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
            for (int index = 0; index < tokens.Length; index++)
            {
                string token = tokens[index];
                string path = token.Length >= 4 && token[2] == ' ' ? token.Substring(3) : token;
                if (!string.IsNullOrWhiteSpace(path))
                {
                    yield return path;
                }
            }
        }

        private static string ProjectRoot => Directory.GetParent(Application.dataPath)?.FullName ??
                                             throw new InvalidOperationException("Could not resolve Unity project root.");

        private static string ReadUnityVersionWithRevision()
        {
            string path = Path.Combine(ProjectRoot, "ProjectSettings", "ProjectVersion.txt");
            if (!File.Exists(path))
            {
                return Application.unityVersion;
            }

            foreach (string line in File.ReadLines(path))
            {
                const string key = "m_EditorVersionWithRevision:";
                if (line.StartsWith(key, StringComparison.Ordinal))
                {
                    return line.Substring(key.Length).Trim();
                }
            }

            return Application.unityVersion;
        }

        private static string ResolveProjectOrPackagePath(string path)
        {
            if (Path.IsPathRooted(path))
            {
                return Path.GetFullPath(path);
            }

            string directPath = Path.GetFullPath(Path.Combine(ProjectRoot, path));
            if (File.Exists(directPath))
            {
                return directPath;
            }

            if (!path.StartsWith("Packages/", StringComparison.OrdinalIgnoreCase))
            {
                return directPath;
            }

            PackageManagerPackageInfo package = PackageManagerPackageInfo.FindForAssetPath(path);
            if (package == null || string.IsNullOrWhiteSpace(package.assetPath) || string.IsNullOrWhiteSpace(package.resolvedPath))
            {
                return directPath;
            }

            string relativeToPackage = path.Substring(package.assetPath.Length).TrimStart('/');
            return Path.GetFullPath(Path.Combine(package.resolvedPath, relativeToPackage));
        }

        private static bool TryRunGit(
            string workingDirectory,
            string arguments,
            out string standardOutput,
            out string error)
        {
            standardOutput = string.Empty;
            error = string.Empty;
            try
            {
                using var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "git",
                        Arguments = arguments,
                        WorkingDirectory = workingDirectory,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        StandardOutputEncoding = new UTF8Encoding(false),
                        StandardErrorEncoding = new UTF8Encoding(false)
                    }
                };

                if (!process.Start())
                {
                    error = $"git {arguments} did not start.";
                    return false;
                }

                standardOutput = process.StandardOutput.ReadToEnd();
                string standardError = process.StandardError.ReadToEnd();
                if (!process.WaitForExit(30000))
                {
                    process.Kill();
                    error = $"git {arguments} timed out.";
                    return false;
                }

                if (process.ExitCode != 0)
                {
                    error = $"git {arguments} failed ({process.ExitCode}): {standardError.Trim()}";
                    return false;
                }

                return true;
            }
            catch (Exception exception)
            {
                error = $"git {arguments} failed: {exception.Message}";
                Debug.LogException(exception);
                return false;
            }
        }
    }
}
