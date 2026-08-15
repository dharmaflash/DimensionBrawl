using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace DimensionBrawl.Editor.AuditionPV
{
    internal static class AuditionPvCaptureFoundationValidator
    {
        private const string ValidationReportPath = "C:/tmp/DimensionBrawl-AuditionPvCaptureFoundationValidation.json";

        [MenuItem("DimensionBrawl/Audition PV/Validate Capture Foundation")]
        public static void ValidateMenu()
        {
            ValidateBatch();
        }

        public static void ValidateBatch()
        {
            DateTime startedAtUtc = DateTime.UtcNow;
            string temporaryRoot = Path.Combine(
                Path.GetTempPath(),
                "DimensionBrawl_AuditionPV_Foundation_" + Guid.NewGuid().ToString("N"));
            var checks = new List<ValidationCheck>();

            try
            {
                AuditionPvGitSnapshot git = AuditionPvEnvironmentProbe.ReadGitSnapshot();
                Require(git.probeSucceeded, "git-probe", git.probeError, checks);
                Require(!string.IsNullOrWhiteSpace(git.commitSha), "git-commit-sha", git.commitSha, checks);
                Require(AuditionPvSha256.IsSha256(git.dirtyStateHashSha256), "git-dirty-state-hash", git.dirtyStateHashSha256, checks);

                AuditionPvEngineSnapshot engine = AuditionPvEnvironmentProbe.ReadEngineSnapshot();
                Require(
                    string.Equals(engine.recorderPackageVersion, AuditionPvCaptureContract.RecorderPackageVersion, StringComparison.Ordinal),
                    "recorder-package-version",
                    engine.recorderPackageVersion,
                    checks);
                Require(engine.urpPackageVersion != "missing", "urp-package-version", engine.urpPackageVersion, checks);
                Require(!string.IsNullOrWhiteSpace(engine.activeRenderPipelineAssetPath), "active-urp-asset", engine.activeRenderPipelineAssetPath, checks);
                Require(!string.IsNullOrWhiteSpace(engine.unityVersionWithRevision), "unity-version-with-revision", engine.unityVersionWithRevision, checks);

                string[] dependencyPaths = AuditionPvEnvironmentProbe.CollectCaptureDependencyPaths();
                AuditionPvDependencyHash[] dependencyHashes = AuditionPvEnvironmentProbe.HashDependencies(dependencyPaths);
                foreach (string corePath in AuditionPvCaptureContract.CoreDependencyPaths)
                {
                    AuditionPvDependencyHash dependency = dependencyHashes.FirstOrDefault(item =>
                        string.Equals(item.path, corePath, StringComparison.OrdinalIgnoreCase));
                    Require(
                        dependency != null && dependency.exists && AuditionPvSha256.IsSha256(dependency.sha256),
                        "dependency-hash:" + corePath,
                        dependency?.sha256 ?? "missing",
                        checks);
                }

                string captureId = AuditionPvOutputPaths.CreateOutputId(
                    "foundation-validation",
                    startedAtUtc,
                    git.commitSha,
                    git.isDirty,
                    git.dirtyStateHashSha256);
                string outputDirectory = AuditionPvOutputPaths.ResolveOutputDirectory(temporaryRoot, captureId);

                using (AuditionPvRecorderSettingsBundle recorder =
                       AuditionPvRecorderSettingsFactory.CreateLosslessPngSequence(outputDirectory, "validation-shot"))
                {
                    AuditionPvRecorderSettingsFactory.Validate(recorder);
                    Require(true, "recorder-settings", recorder.imageSettings.OutputFile, checks);
                }

                AuditionPvShotManifestEntry[] shots =
                {
                    new()
                    {
                        id = "validation-shot",
                        scenePath = "Assets/_Game/Scenes/OlympusStationCombatStage.unity",
                        startFrame = 0,
                        endFrame = 59,
                        expectedFrameCount = 60,
                        hudMode = "hud-on",
                        notes = "Foundation validation only; no scene orchestration was executed."
                    }
                };
                AuditionPvBaselineManifestEntry[] baselines =
                {
                    new()
                    {
                        id = "validation-baseline",
                        shotId = "validation-shot",
                        sourceFrame = 30,
                        fileName = "VALIDATION_BASELINE__HUDON.png",
                        hudMode = "hud-on",
                        status = "schema-only"
                    }
                };
                AuditionPvTestResult[] testResults =
                {
                    new()
                    {
                        suite = nameof(AuditionPvCaptureFoundationValidator),
                        name = "foundation-contract",
                        status = "passed",
                        durationMilliseconds = (long)(DateTime.UtcNow - startedAtUtc).TotalMilliseconds,
                        details = "Recorder settings, environment provenance, dependency hashing, and manifest writer passed.",
                        artifactPath = ValidationReportPath
                    }
                };

                AuditionPvCaptureManifest manifest = AuditionPvCaptureManifestFactory.CreateForRoot(
                    captureId,
                    temporaryRoot,
                    outputDirectory,
                    shots,
                    baselines,
                    testResults,
                    createdAtUtc: startedAtUtc,
                    gitSnapshot: git,
                    engineSnapshot: engine,
                    dependencyHashSnapshot: dependencyHashes);
                AuditionPvCaptureManifestWriter.Validate(manifest);
                string manifestPath = AuditionPvCaptureManifestWriter.WriteNew(manifest);
                Require(File.Exists(manifestPath), "manifest-write", manifestPath, checks);

                string json = File.ReadAllText(manifestPath, Encoding.UTF8);
                AuditionPvCaptureManifest roundTripped = JsonUtility.FromJson<AuditionPvCaptureManifest>(json);
                Require(roundTripped != null && roundTripped.shots.Length == 1, "manifest-shot-roundtrip", "1 shot", checks);
                Require(roundTripped != null && roundTripped.baselines.Length == 1, "manifest-baseline-roundtrip", "1 baseline", checks);
                Require(roundTripped != null && roundTripped.dependencyHashes.Length > 0, "manifest-dependency-roundtrip", dependencyHashes.Length.ToString(), checks);
                Require(roundTripped != null && roundTripped.testResults.Length == 1, "manifest-test-result-roundtrip", "1 result", checks);

                WriteValidationReport(startedAtUtc, DateTime.UtcNow, checks, true, string.Empty);
                Debug.Log(
                    $"[AuditionPvCaptureFoundation] PASS ({checks.Count} checks). " +
                    $"Recorder {engine.recorderPackageVersion}, URP {engine.urpPackageVersion}, " +
                    $"{AuditionPvCaptureContract.Width}x{AuditionPvCaptureContract.Height}@{AuditionPvCaptureContract.Fps}. " +
                    $"Report: {ValidationReportPath}");
            }
            catch (Exception exception)
            {
                WriteValidationReport(startedAtUtc, DateTime.UtcNow, checks, false, exception.ToString());
                throw;
            }
            finally
            {
                if (Directory.Exists(temporaryRoot))
                {
                    Directory.Delete(temporaryRoot, true);
                }
            }
        }

        private static void Require(bool condition, string name, string details, ICollection<ValidationCheck> checks)
        {
            checks.Add(new ValidationCheck
            {
                name = name,
                passed = condition,
                details = details ?? string.Empty
            });
            if (!condition)
            {
                throw new InvalidOperationException($"Audition PV capture foundation check failed: {name} ({details})");
            }
        }

        private static void WriteValidationReport(
            DateTime startedAtUtc,
            DateTime completedAtUtc,
            List<ValidationCheck> checks,
            bool passed,
            string error)
        {
            var report = new ValidationReport
            {
                schemaVersion = "dimension-brawl.audition-pv.foundation-validation.v1",
                startedAtUtc = startedAtUtc.ToUniversalTime().ToString("O"),
                completedAtUtc = completedAtUtc.ToUniversalTime().ToString("O"),
                passed = passed,
                error = error ?? string.Empty,
                checks = checks.ToArray()
            };
            string reportDirectory = Path.GetDirectoryName(ValidationReportPath);
            if (!string.IsNullOrWhiteSpace(reportDirectory))
            {
                Directory.CreateDirectory(reportDirectory);
            }

            File.WriteAllText(
                ValidationReportPath,
                JsonUtility.ToJson(report, true) + Environment.NewLine,
                new UTF8Encoding(false));
        }

        [Serializable]
        private sealed class ValidationReport
        {
            public string schemaVersion = string.Empty;
            public string startedAtUtc = string.Empty;
            public string completedAtUtc = string.Empty;
            public bool passed;
            public string error = string.Empty;
            public ValidationCheck[] checks = Array.Empty<ValidationCheck>();
        }

        [Serializable]
        private sealed class ValidationCheck
        {
            public string name = string.Empty;
            public bool passed;
            public string details = string.Empty;
        }
    }
}
