using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DimensionBrawl.Editor
{
    public static class AndroidPerformanceBenchmarkBuild
    {
        private const string ApkPath = "C:/tmp/DimensionBrawl-MobilePerformance.apk";
        private const string MarkdownReportPath = "C:/tmp/DimensionBrawl-MobilePerformanceBuild.md";
        private const string JsonReportPath = "C:/tmp/DimensionBrawl-MobilePerformanceBuild.json";
        private const string RunnerScriptPath = "C:/tmp/Run-DimensionBrawl-MobilePerformance.ps1";
        private const string BenchmarkDefine = "DIMENSIONBRAWL_MOBILE_PERF";
        private const string PackageName = "com.dharmaflash.dimensionbrawl";

        private static readonly string[] BenchmarkScenes =
        {
            "Assets/_Game/Scenes/OlympusStationCombatStage.unity",
            "Assets/_Game/Scenes/OlympusCorridorInvasionStage.unity",
            "Assets/_Game/Scenes/ActionFoundationBossBarrageLaneReview.unity",
            "Assets/_Game/Scenes/ActionFoundationFrontlineMotivationReview.unity"
        };

        [MenuItem("DimensionBrawl/Performance/Build Android Performance Benchmark")]
        public static void BuildMenu()
        {
            BuildBatch();
        }

        public static void BuildBatch()
        {
            ValidateBuildEnvironment();
            CanonicalMobileContentImportOptimizer.ApplyBatchOptimization();
            EnsureBatchBuildSceneContext();
            Directory.CreateDirectory(Path.GetDirectoryName(ApkPath) ?? "C:/tmp");
            if (File.Exists(ApkPath))
            {
                File.Delete(ApkPath);
            }

            DeleteStaleGradleApkOutputs();

            BuildPlayerOptions options = new()
            {
                scenes = BenchmarkScenes,
                locationPathName = ApkPath,
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = BuildOptions.Development | BuildOptions.AllowDebugging,
                extraScriptingDefines = new[] { BenchmarkDefine }
            };

            DateTime startedUtc = DateTime.UtcNow;
            BuildReport buildReport = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = buildReport.summary;
            AndroidPerformanceBuildReport report = new()
            {
                GeneratedUtc = DateTime.UtcNow.ToString("O"),
                StartedUtc = startedUtc.ToString("O"),
                UnityVersion = Application.unityVersion,
                Result = summary.result.ToString(),
                TotalSeconds = summary.totalTime.TotalSeconds,
                BuildSummaryBytes = (long)summary.totalSize,
                ApkBytes = File.Exists(ApkPath) ? new FileInfo(ApkPath).Length : 0L,
                TotalErrors = summary.totalErrors,
                TotalWarnings = summary.totalWarnings,
                ApkPath = ApkPath,
                PackageName = PackageName,
                AndroidTextureSubtarget = EditorUserBuildSettings.androidBuildSubtarget.ToString(),
                ScriptingBackend = PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android).ToString(),
                TargetArchitectures = PlayerSettings.Android.targetArchitectures.ToString(),
                DevelopmentBuild = true,
                BenchmarkDefine = BenchmarkDefine,
                Scenes = BenchmarkScenes
            };
            WriteReports(report);
            WriteRunnerScript(ResolveAdbPath());

            if (summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Android performance benchmark build failed with {summary.totalErrors} error(s). " +
                    $"See {MarkdownReportPath}");
            }

            Debug.Log(
                $"Android performance benchmark built: {ApkPath} " +
                $"({summary.totalSize / (1024d * 1024d):N1} MiB). Runner: {RunnerScriptPath}");
        }

        private static void ValidateBuildEnvironment()
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                throw new InvalidOperationException(
                    "Android must be the active build target. Launch batch mode with -buildTarget Android.");
            }

            for (int i = 0; i < BenchmarkScenes.Length; i++)
            {
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(BenchmarkScenes[i]) == null)
                {
                    throw new InvalidOperationException($"Benchmark scene is missing: {BenchmarkScenes[i]}");
                }
            }

            string configuredPackageName = PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android);
            if (!string.Equals(configuredPackageName, PackageName, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Unexpected Android package name. Expected {PackageName}, got {configuredPackageName}.");
            }
        }

        private static void EnsureBatchBuildSceneContext()
        {
            if (Application.isBatchMode
                && string.IsNullOrEmpty(SceneManager.GetActiveScene().path))
            {
                EditorSceneManager.OpenScene(BenchmarkScenes[0], OpenSceneMode.Single);
            }
        }

        private static void DeleteStaleGradleApkOutputs()
        {
            string projectRoot = Path.GetDirectoryName(Application.dataPath) ?? string.Empty;
            string debugApkDirectory = Path.Combine(
                projectRoot,
                "Library",
                "Bee",
                "Android",
                "Prj",
                "IL2CPP",
                "Gradle",
                "launcher",
                "build",
                "outputs",
                "apk",
                "debug");
            if (!Directory.Exists(debugApkDirectory))
            {
                return;
            }

            string[] staleApks = Directory.GetFiles(
                debugApkDirectory,
                "*.apk",
                SearchOption.TopDirectoryOnly);
            for (int i = 0; i < staleApks.Length; i++)
            {
                File.Delete(staleApks[i]);
            }
        }

        private static string ResolveAdbPath()
        {
            string editorDirectory = Path.GetDirectoryName(EditorApplication.applicationPath) ?? string.Empty;
            string adbPath = Path.Combine(
                editorDirectory,
                "Data",
                "PlaybackEngines",
                "AndroidPlayer",
                "SDK",
                "platform-tools",
                "adb.exe");
            return File.Exists(adbPath) ? adbPath.Replace('\\', '/') : "adb";
        }

        private static void WriteReports(AndroidPerformanceBuildReport report)
        {
            File.WriteAllText(JsonReportPath, JsonUtility.ToJson(report, true), Encoding.UTF8);

            StringBuilder builder = new();
            builder.AppendLine("# DimensionBrawl Android Performance Benchmark Build");
            builder.AppendLine();
            builder.AppendLine($"- Generated UTC: {report.GeneratedUtc}");
            builder.AppendLine($"- Unity: {report.UnityVersion}");
            builder.AppendLine($"- Result: {report.Result}");
            builder.AppendLine($"- Duration: {report.TotalSeconds:N1}s");
            builder.AppendLine($"- APK artifact size: {report.ApkBytes / (1024d * 1024d):N1} MiB");
            builder.AppendLine($"- Unity build summary size: {report.BuildSummaryBytes / (1024d * 1024d):N1} MiB");
            builder.AppendLine($"- Errors / warnings: {report.TotalErrors} / {report.TotalWarnings}");
            builder.AppendLine($"- Package: `{report.PackageName}`");
            builder.AppendLine($"- Scripting backend: {report.ScriptingBackend}");
            builder.AppendLine($"- Architectures: {report.TargetArchitectures}");
            builder.AppendLine($"- Android texture subtarget: {report.AndroidTextureSubtarget}");
            builder.AppendLine($"- APK: `{report.ApkPath}`");
            builder.AppendLine($"- Device runner: `{RunnerScriptPath}`");
            builder.AppendLine($"- Runner command: `powershell.exe -NoProfile -ExecutionPolicy Bypass -File {RunnerScriptPath}`");
            builder.AppendLine();
            builder.AppendLine("## Scenes");
            builder.AppendLine();
            for (int i = 0; i < report.Scenes.Length; i++)
            {
                builder.AppendLine($"- `{report.Scenes[i]}`");
            }

            builder.AppendLine();
            builder.AppendLine("The runner installs the APK, captures logcat, PSS, thermal and battery snapshots, waits for the in-game benchmark, and pulls the final JSON report.");
            File.WriteAllText(MarkdownReportPath, builder.ToString(), Encoding.UTF8);
        }

        private static void WriteRunnerScript(string adbPath)
        {
            string normalizedAdb = adbPath.Replace("'", "''");
            StringBuilder builder = new();
            builder.AppendLine("$ErrorActionPreference = 'Stop'");
            builder.AppendLine($"$adb = '{normalizedAdb}'");
            builder.AppendLine($"$apk = '{ApkPath}'");
            builder.AppendLine($"$package = '{PackageName}'");
            builder.AppendLine("$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'");
            builder.AppendLine("$output = \"C:/tmp/DimensionBrawl-AndroidDevice/$stamp\"");
            builder.AppendLine("function Invoke-Adb {");
            builder.AppendLine("    param([string[]] $Arguments)");
            builder.AppendLine("    $previousPreference = $ErrorActionPreference");
            builder.AppendLine("    $ErrorActionPreference = 'Continue'");
            builder.AppendLine("    try {");
            builder.AppendLine("        $result = & $adb @Arguments 2>&1");
            builder.AppendLine("        $exitCode = $LASTEXITCODE");
            builder.AppendLine("    } finally {");
            builder.AppendLine("        $ErrorActionPreference = $previousPreference");
            builder.AppendLine("    }");
            builder.AppendLine("    if ($exitCode -ne 0) { throw \"adb failed ($exitCode): $($Arguments -join ' ')`n$($result -join '`n')\" }");
            builder.AppendLine("    return $result");
            builder.AppendLine("}");
            builder.AppendLine("function Add-AdbSnapshot {");
            builder.AppendLine("    param([string[]] $Arguments, [string] $Path)");
            builder.AppendLine("    \"=== $(Get-Date -Format o) ===\" | Add-Content -LiteralPath $Path");
            builder.AppendLine("    $previousPreference = $ErrorActionPreference");
            builder.AppendLine("    $ErrorActionPreference = 'Continue'");
            builder.AppendLine("    try {");
            builder.AppendLine("        (& $adb @Arguments 2>&1) | Add-Content -LiteralPath $Path");
            builder.AppendLine("    } finally {");
            builder.AppendLine("        $ErrorActionPreference = $previousPreference");
            builder.AppendLine("    }");
            builder.AppendLine("}");
            builder.AppendLine("New-Item -ItemType Directory -Force -Path $output | Out-Null");
            builder.AppendLine("Invoke-Adb @('start-server') | Out-Null");
            builder.AppendLine("$devices = Invoke-Adb @('devices')");
            builder.AppendLine("if (($devices | Select-String -Pattern \"\\tdevice$\").Count -ne 1) { throw 'Exactly one authorized Android device is required.' }");
            builder.AppendLine("Invoke-Adb @('install', '-r', $apk) | Out-Null");
            builder.AppendLine("Invoke-Adb @('logcat', '-c') | Out-Null");
            builder.AppendLine("Invoke-Adb @('shell', 'am', 'force-stop', $package) | Out-Null");
            builder.AppendLine("Invoke-Adb @('shell', 'monkey', '-p', $package, '-c', 'android.intent.category.LAUNCHER', '1') | Out-Null");
            builder.AppendLine("$deadline = (Get-Date).AddMinutes(12)");
            builder.AppendLine("$complete = $false");
            builder.AppendLine("while ((Get-Date) -lt $deadline) {");
            builder.AppendLine("    $log = Invoke-Adb @('logcat', '-d', '-s', 'Unity')");
            builder.AppendLine("    $log | Set-Content -LiteralPath \"$output/logcat.txt\"");
            builder.AppendLine("    Add-AdbSnapshot @('shell', 'dumpsys', 'meminfo', $package) \"$output/meminfo.txt\"");
            builder.AppendLine("    Add-AdbSnapshot @('shell', 'dumpsys', 'thermalservice') \"$output/thermal.txt\"");
            builder.AppendLine("    Add-AdbSnapshot @('shell', 'dumpsys', 'battery') \"$output/battery.txt\"");
            builder.AppendLine("    if ($log -match '\\[MOBILE_PERF\\] COMPLETE') { $complete = $true; break }");
            builder.AppendLine("    Start-Sleep -Seconds 10");
            builder.AppendLine("}");
            builder.AppendLine("$remote = \"/sdcard/Android/data/$package/files/DimensionBrawl-MobilePerformance.json\"");
            builder.AppendLine("Invoke-Adb @('pull', $remote, \"$output/DimensionBrawl-MobilePerformance.json\") | Out-Null");
            builder.AppendLine("if (-not $complete) { throw 'Benchmark did not complete within twelve minutes. Partial diagnostics were preserved.' }");
            builder.AppendLine("Write-Output \"Benchmark complete: $output\"");
            File.WriteAllText(RunnerScriptPath, builder.ToString(), new UTF8Encoding(false));
        }

        [Serializable]
        private sealed class AndroidPerformanceBuildReport
        {
            public string GeneratedUtc;
            public string StartedUtc;
            public string UnityVersion;
            public string Result;
            public double TotalSeconds;
            public long BuildSummaryBytes;
            public long ApkBytes;
            public int TotalErrors;
            public int TotalWarnings;
            public string ApkPath;
            public string PackageName;
            public string AndroidTextureSubtarget;
            public string ScriptingBackend;
            public string TargetArchitectures;
            public bool DevelopmentBuild;
            public string BenchmarkDefine;
            public string[] Scenes;
        }
    }
}
