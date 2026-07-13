#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Text;
using DimensionBrawl.Player;
using DimensionBrawl.Presentation;
using UnityEngine;

namespace DimensionBrawl.Verification
{
    [DisallowMultipleComponent]
    public sealed class CombatCameraFeedbackProbe : MonoBehaviour
    {
        [SerializeField] private string resultPath = "C:/tmp/DimensionBrawl-CombatCameraFeedback.result";
        [SerializeField, Min(0.25f)] private float timeoutSeconds = 4f;
        [SerializeField, Min(1)] private int sampleFrames = 12;

        private bool verificationStarted;

        private void Start()
        {
            BeginVerification();
        }

        public void Configure(string newResultPath, float newTimeoutSeconds)
        {
            if (!string.IsNullOrWhiteSpace(newResultPath))
            {
                resultPath = newResultPath;
            }

            timeoutSeconds = Mathf.Max(0.25f, newTimeoutSeconds);
        }

        public void BeginVerification()
        {
            if (verificationStarted)
            {
                return;
            }

            verificationStarted = true;
            StartCoroutine(VerifyRoutine());
        }

        private IEnumerator VerifyRoutine()
        {
            yield return null;
            yield return null;

            StringBuilder report = new StringBuilder(2048);
            report.AppendLine("CHECK=CombatCameraFeedback");

            ActionCameraController cameraController =
                FindFirstObjectByType<ActionCameraController>(FindObjectsInactive.Exclude);
            PlayerRangedBasicAttackAction rangedAction =
                FindFirstObjectByType<PlayerRangedBasicAttackAction>(FindObjectsInactive.Exclude);
            PlayerCombatModeController modeController =
                FindFirstObjectByType<PlayerCombatModeController>(FindObjectsInactive.Exclude);

            if (cameraController == null || rangedAction == null)
            {
                report.AppendLine($"cameraController={(cameraController != null)}");
                report.AppendLine($"rangedAction={(rangedAction != null)}");
                WriteResult(false, report.ToString());
                yield break;
            }

            modeController?.SetRangedMode();

            float startedAt = Time.realtimeSinceStartup;
            while (!rangedAction.IsFireReady && Time.realtimeSinceStartup - startedAt < timeoutSeconds)
            {
                yield return null;
            }

            Camera camera = cameraController.GetComponent<Camera>();
            int beforeRifleCount = cameraController.RifleFireFeedbackRequestCount;
            int beforeMicroShakeCount = cameraController.MicroShakeRequestCount;
            Vector3 basePosition = cameraController.transform.position;
            Quaternion baseRotation = cameraController.transform.rotation;
            float baseFieldOfView = camera != null ? camera.fieldOfView : 0f;

            bool fired = rangedAction.TryFire();

            float maxPositionDelta = 0f;
            float maxRotationDelta = 0f;
            float maxFieldOfViewDelta = 0f;
            float maxLocalShake = 0f;
            float maxEulerShake = 0f;
            bool observedActiveCue = false;
            bool observedActiveMicroShake = false;
            int frames = Mathf.Max(1, sampleFrames);
            for (int i = 0; i < frames; i++)
            {
                yield return null;

                maxPositionDelta = Mathf.Max(
                    maxPositionDelta,
                    Vector3.Distance(basePosition, cameraController.transform.position));
                maxRotationDelta = Mathf.Max(
                    maxRotationDelta,
                    Quaternion.Angle(baseRotation, cameraController.transform.rotation));
                if (camera != null)
                {
                    maxFieldOfViewDelta = Mathf.Max(
                        maxFieldOfViewDelta,
                        Mathf.Abs(camera.fieldOfView - baseFieldOfView));
                }

                maxLocalShake = Mathf.Max(maxLocalShake, cameraController.LastMicroShakeLocalOffset.magnitude);
                maxEulerShake = Mathf.Max(maxEulerShake, cameraController.LastMicroShakeEulerOffset.magnitude);
                observedActiveCue |= cameraController.HasActiveCue;
                observedActiveMicroShake |= cameraController.HasActiveMicroShake;
            }

            rangedAction.SetFireHeld(false);
            rangedAction.SetExternalAimPreviewHeld(false);

            int rifleCountDelta = cameraController.RifleFireFeedbackRequestCount - beforeRifleCount;
            int microShakeCountDelta = cameraController.MicroShakeRequestCount - beforeMicroShakeCount;
            bool requestReachedCamera = rifleCountDelta > 0 && microShakeCountDelta > 0;
            bool cameraMoved = maxPositionDelta > 0.001f
                || maxRotationDelta > 0.01f
                || maxFieldOfViewDelta > 0.01f
                || maxLocalShake > 0.0001f
                || maxEulerShake > 0.001f;

            report.AppendLine($"fired={fired}");
            report.AppendLine($"rifleCountDelta={rifleCountDelta}");
            report.AppendLine($"microShakeCountDelta={microShakeCountDelta}");
            report.AppendLine($"observedActiveCue={observedActiveCue}");
            report.AppendLine($"observedActiveMicroShake={observedActiveMicroShake}");
            report.AppendLine($"maxPositionDelta={maxPositionDelta:F6}");
            report.AppendLine($"maxRotationDelta={maxRotationDelta:F6}");
            report.AppendLine($"maxFieldOfViewDelta={maxFieldOfViewDelta:F6}");
            report.AppendLine($"maxLocalShake={maxLocalShake:F6}");
            report.AppendLine($"maxEulerShake={maxEulerShake:F6}");
            report.AppendLine($"lastRifleFireFeedbackTime={cameraController.LastRifleFireFeedbackTime:F6}");
            report.AppendLine($"blockedReason={rangedAction.LastUseBlockedReason}");

            WriteResult(fired && requestReachedCamera && cameraMoved, report.ToString());
        }

        private void WriteResult(bool passed, string details)
        {
            string resolvedPath = string.IsNullOrWhiteSpace(resultPath)
                ? "C:/tmp/DimensionBrawl-CombatCameraFeedback.result"
                : resultPath;
            string directory = Path.GetDirectoryName(resolvedPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string body = $"RESULT={(passed ? "PASS" : "FAIL")}{Environment.NewLine}{details}";
            File.WriteAllText(resolvedPath, body, Encoding.UTF8);

            if (passed)
            {
                Debug.Log($"[CombatCameraFeedbackProbe] Passed. See {resolvedPath}.");
            }
            else
            {
                Debug.LogError($"[CombatCameraFeedbackProbe] Failed. See {resolvedPath}.");
            }
        }
    }
}
#endif
