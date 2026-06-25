using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DimensionBrawl.Combat;
using DimensionBrawl.Player;
using DimensionBrawl.Presentation;
using UnityEngine;

namespace DimensionBrawl.Test
{
    [DisallowMultipleComponent]
    public sealed class BossBarrageActionBridgeRouteProbe : MonoBehaviour
    {
        [SerializeField] private string resultPath = "C:/tmp/DimensionBrawl-BossBarrageActionBridgeRoute.result";
        [SerializeField, Min(0.25f)] private float routeTimeoutSeconds = 12f;
        [SerializeField, Min(0.25f)] private float idleTimeoutSeconds = 12f;
        [SerializeField, Min(300f)] private float tierThreeGrantEnergy = 1000f;
        [SerializeField, Min(0f)] private float settleSeconds = 0.2f;
        [SerializeField] private string outputDirectory =
            "C:/tmp/DimensionBrawl-BossBarrageActionBridgeRouteFrames";
        [SerializeField, Min(16)] private int captureWidth = 1280;
        [SerializeField, Min(16)] private int captureHeight = 720;

        private bool verificationStarted;
        private bool lastStepPassed;

        private sealed class ProbeContext
        {
            public SummonEnergyLadder Energy;
            public PlayerSkill1Action Skill1Action;
            public PlayerSummonSlot1Action SummonSlot1Action;
            public ActionCameraCueDriver CameraCueDriver;
            public ActionCinematicCueDirector CueDirector;
            public ActionCinematicSequenceBridge SequenceBridge;
            public CinematicSequenceRunner SequenceRunner;
        }

        private struct CapturedRouteFrame
        {
            public string Label;
            public string Path;
            public bool CaptureSucceeded;
            public string SequenceId;
            public string LastCameraCueId;
            public string LastActorCueId;
            public string LastVfxCueId;
        }

        private void Start()
        {
            BeginVerification();
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
            report.AppendLine("ROUTE=BossBarrageActionBridgeInput");
            List<CapturedRouteFrame> capturedFrames = new List<CapturedRouteFrame>(7);

            ProbeContext context;
            try
            {
                context = ResolveContext();
                AppendDependencySnapshot(report, context);
            }
            catch (Exception exception)
            {
                report.AppendLine("DEPENDENCIES=FAIL");
                report.AppendLine(exception.ToString());
                WriteResult(false, report.ToString());
                yield break;
            }

            yield return VerifyActionRoute(
                context,
                "skill1_tier3_ultimate",
                ActionCinematicCueProfile.CueKind.UltimateCutIn,
                "ultimate_cutin",
                () => context.Skill1Action.TryUseSkill1(),
                report,
                capturedFrames);
            bool skillRoutePassed = lastStepPassed;

            yield return WaitForIdle(context, "after_skill1_tier3_ultimate", report);
            bool skillIdlePassed = lastStepPassed;

            yield return VerifyActionRoute(
                context,
                "summon_slot1_tier3_entry",
                ActionCinematicCueProfile.CueKind.SummonEntry,
                "summon_entry",
                () => context.SummonSlot1Action.TryUseSummonSlot1(),
                report,
                capturedFrames);
            bool summonRoutePassed = lastStepPassed;

            yield return WaitForIdle(context, "after_summon_slot1_tier3_entry", report);
            bool summonIdlePassed = lastStepPassed;

            yield return VerifyDirectorRoute(
                context,
                "boss_summon_pressure_direct_bridge",
                ActionCinematicCueProfile.CueKind.BossPressureBreak,
                "boss_summon_pressure",
                1.02f,
                report,
                capturedFrames);
            bool bossSummonPressureRoutePassed = lastStepPassed;

            yield return WaitForIdle(context, "after_boss_summon_pressure_direct_bridge", report);
            bool bossSummonPressureIdlePassed = lastStepPassed;

            yield return VerifyDirectorRoute(
                context,
                "summon_empower_direct_bridge",
                ActionCinematicCueProfile.CueKind.SummonEmpower,
                "summon_empower",
                1.12f,
                report,
                capturedFrames);
            bool summonEmpowerRoutePassed = lastStepPassed;

            yield return WaitForIdle(context, "after_summon_empower_direct_bridge", report);
            bool summonEmpowerIdlePassed = lastStepPassed;

            yield return VerifyDirectorRoute(
                context,
                "summon_recall_direct_bridge",
                ActionCinematicCueProfile.CueKind.SummonRecall,
                "summon_recall",
                1.08f,
                report,
                capturedFrames);
            bool summonRecallRoutePassed = lastStepPassed;

            yield return WaitForIdle(context, "after_summon_recall_direct_bridge", report);
            bool summonRecallIdlePassed = lastStepPassed;

            bool passed = skillRoutePassed
                && skillIdlePassed
                && summonRoutePassed
                && summonIdlePassed
                && bossSummonPressureRoutePassed
                && bossSummonPressureIdlePassed
                && summonEmpowerRoutePassed
                && summonEmpowerIdlePassed
                && summonRecallRoutePassed
                && summonRecallIdlePassed;
            AppendCaptureSummary(report, capturedFrames);
            WriteResult(passed, report.ToString());
        }

        private IEnumerator VerifyActionRoute(
            ProbeContext context,
            string label,
            ActionCinematicCueProfile.CueKind expectedKind,
            string expectedSequenceId,
            Func<bool> triggerAction,
            StringBuilder report,
            List<CapturedRouteFrame> capturedFrames)
        {
            lastStepPassed = false;
            PrepareTierThreeEnergy(context.Energy, tierThreeGrantEnergy);

            int availableTier = context.Energy.AvailableTier;
            if (availableTier < 3)
            {
                report.AppendLine($"ROUTE_RESULT {label}=FAIL");
                report.AppendLine($"REASON {label}=Energy tier was {availableTier}; expected 3.");
                yield break;
            }

            int bridgeCountBefore = context.SequenceBridge.TotalPlayCount;
            int directorCountBefore = context.CueDirector.TotalPlayCount;
            int cameraCueCountBefore = context.SequenceRunner.TotalCameraCueCount;
            int actorCueCountBefore = context.SequenceRunner.TotalActorCueCount;
            int boundActorCueCountBefore = context.SequenceRunner.TotalBoundActorCueCount;
            int vfxCueCountBefore = context.SequenceRunner.TotalVfxCueCount;
            string lastCameraCueBefore = context.SequenceRunner.LastCameraCueId;
            string lastActorCueBefore = context.SequenceRunner.LastActorCueId;
            string lastVfxCueBefore = context.SequenceRunner.LastVfxCueId;

            bool triggerSucceeded = false;
            string triggerException = null;
            try
            {
                triggerSucceeded = triggerAction();
            }
            catch (Exception exception)
            {
                triggerException = exception.ToString();
            }

            if (!string.IsNullOrEmpty(triggerException))
            {
                report.AppendLine($"ROUTE_RESULT {label}=FAIL");
                report.AppendLine($"EXCEPTION {label}={triggerException}");
                yield break;
            }

            if (!triggerSucceeded)
            {
                report.AppendLine($"ROUTE_RESULT {label}=FAIL");
                report.AppendLine($"REASON {label}=Action trigger returned false.");
                yield break;
            }

            float startedAt = Time.realtimeSinceStartup;
            bool bridgeMatched = false;
            bool directorMatched = false;
            bool runnerDispatched = false;
            while (Time.realtimeSinceStartup - startedAt < routeTimeoutSeconds)
            {
                bridgeMatched |= context.SequenceBridge.TotalPlayCount > bridgeCountBefore
                    && context.SequenceBridge.LastPlayedKind == expectedKind
                    && context.SequenceBridge.LastPlayedTier == 3
                    && context.SequenceBridge.LastPlayedProfile != null
                    && string.Equals(
                        context.SequenceBridge.LastPlayedProfile.SequenceId,
                        expectedSequenceId,
                        StringComparison.Ordinal);
                directorMatched |= context.CueDirector.TotalPlayCount > directorCountBefore
                    && context.CueDirector.LastPlayedKind == expectedKind
                    && context.CueDirector.LastPlayedTier == 3;
                runnerDispatched |= context.SequenceRunner.TotalCameraCueCount > cameraCueCountBefore
                    || context.SequenceRunner.TotalActorCueCount > actorCueCountBefore
                    || context.SequenceRunner.TotalVfxCueCount > vfxCueCountBefore
                    || HasChangedCue(lastCameraCueBefore, context.SequenceRunner.LastCameraCueId)
                    || HasChangedCue(lastActorCueBefore, context.SequenceRunner.LastActorCueId)
                    || HasChangedCue(lastVfxCueBefore, context.SequenceRunner.LastVfxCueId);

                if (bridgeMatched && directorMatched && runnerDispatched)
                {
                    break;
                }

                yield return null;
            }

            if (settleSeconds > 0f)
            {
                yield return new WaitForSecondsRealtime(settleSeconds);
            }

            bool boundActorDispatched = context.SequenceRunner.TotalBoundActorCueCount > 0;
            bool routeSignalsPassed = bridgeMatched && directorMatched && runnerDispatched && boundActorDispatched;
            bool capturePassed = routeSignalsPassed
                && CaptureRouteFrame(context, label, report, capturedFrames);
            if (capturePassed && string.Equals(label, "summon_slot1_tier3_entry", StringComparison.Ordinal))
            {
                yield return new WaitForSecondsRealtime(0.95f);
                capturePassed &= CaptureRouteFrame(context, label + "_command", report, capturedFrames);
                yield return new WaitForSecondsRealtime(0.80f);
                capturePassed &= CaptureRouteFrame(context, label + "_hit", report, capturedFrames);
            }

            lastStepPassed = routeSignalsPassed && capturePassed;
            report.AppendLine($"ROUTE_RESULT {label}={(lastStepPassed ? "PASS" : "FAIL")}");
            report.AppendLine($"OBSERVED {label}=bridge:{bridgeMatched} director:{directorMatched} runner:{runnerDispatched} boundActor:{boundActorDispatched}");
            report.AppendLine($"BRIDGE {label}=count:{context.SequenceBridge.TotalPlayCount - bridgeCountBefore} kind:{context.SequenceBridge.LastPlayedKind} tier:{context.SequenceBridge.LastPlayedTier} profile:{ResolveSequenceId(context.SequenceBridge.LastPlayedProfile)}");
            report.AppendLine($"DIRECTOR {label}=count:{context.CueDirector.TotalPlayCount - directorCountBefore} kind:{context.CueDirector.LastPlayedKind} tier:{context.CueDirector.LastPlayedTier} cue:{context.CueDirector.LastPlayedCueId}");
            report.AppendLine($"RUNNER {label}=cameraCount:{cameraCueCountBefore}->{context.SequenceRunner.TotalCameraCueCount} actorCount:{actorCueCountBefore}->{context.SequenceRunner.TotalActorCueCount} boundActorCount:{boundActorCueCountBefore}->{context.SequenceRunner.TotalBoundActorCueCount} vfxCount:{vfxCueCountBefore}->{context.SequenceRunner.TotalVfxCueCount} lastCamera:{context.SequenceRunner.LastCameraCueId} lastActor:{context.SequenceRunner.LastActorCueId} lastVfx:{context.SequenceRunner.LastVfxCueId}");
        }

        private IEnumerator VerifyDirectorRoute(
            ProbeContext context,
            string label,
            ActionCinematicCueProfile.CueKind expectedKind,
            string expectedSequenceId,
            float sampleDelaySeconds,
            StringBuilder report,
            List<CapturedRouteFrame> capturedFrames)
        {
            lastStepPassed = false;

            int bridgeCountBefore = context.SequenceBridge.TotalPlayCount;
            int directorCountBefore = context.CueDirector.TotalPlayCount;
            int cameraCueCountBefore = context.SequenceRunner.TotalCameraCueCount;
            int actorCueCountBefore = context.SequenceRunner.TotalActorCueCount;
            int boundActorCueCountBefore = context.SequenceRunner.TotalBoundActorCueCount;
            int vfxCueCountBefore = context.SequenceRunner.TotalVfxCueCount;
            string lastCameraCueBefore = context.SequenceRunner.LastCameraCueId;
            string lastActorCueBefore = context.SequenceRunner.LastActorCueId;
            string lastVfxCueBefore = context.SequenceRunner.LastVfxCueId;

            bool triggerSucceeded = false;
            string triggerException = null;
            try
            {
                triggerSucceeded = context.CueDirector.TryPlay(expectedKind, 3, Vector3.forward);
            }
            catch (Exception exception)
            {
                triggerException = exception.ToString();
            }

            if (!string.IsNullOrEmpty(triggerException))
            {
                report.AppendLine($"DIRECT_ROUTE_RESULT {label}=FAIL");
                report.AppendLine($"EXCEPTION {label}={triggerException}");
                yield break;
            }

            if (!triggerSucceeded)
            {
                report.AppendLine($"DIRECT_ROUTE_RESULT {label}=FAIL");
                report.AppendLine($"REASON {label}=Cue director returned false.");
                yield break;
            }

            float startedAt = Time.realtimeSinceStartup;
            bool bridgeMatched = false;
            bool directorMatched = false;
            bool runnerDispatched = false;
            while (Time.realtimeSinceStartup - startedAt < routeTimeoutSeconds)
            {
                bridgeMatched |= context.SequenceBridge.TotalPlayCount > bridgeCountBefore
                    && context.SequenceBridge.LastPlayedKind == expectedKind
                    && context.SequenceBridge.LastPlayedTier == 3
                    && context.SequenceBridge.LastPlayedProfile != null
                    && string.Equals(
                        context.SequenceBridge.LastPlayedProfile.SequenceId,
                        expectedSequenceId,
                        StringComparison.Ordinal);
                directorMatched |= context.CueDirector.TotalPlayCount > directorCountBefore
                    && context.CueDirector.LastPlayedKind == expectedKind
                    && context.CueDirector.LastPlayedTier == 3;
                runnerDispatched |= context.SequenceRunner.TotalCameraCueCount > cameraCueCountBefore
                    || context.SequenceRunner.TotalActorCueCount > actorCueCountBefore
                    || context.SequenceRunner.TotalVfxCueCount > vfxCueCountBefore
                    || HasChangedCue(lastCameraCueBefore, context.SequenceRunner.LastCameraCueId)
                    || HasChangedCue(lastActorCueBefore, context.SequenceRunner.LastActorCueId)
                    || HasChangedCue(lastVfxCueBefore, context.SequenceRunner.LastVfxCueId);

                if (bridgeMatched && directorMatched && runnerDispatched)
                {
                    break;
                }

                yield return null;
            }

            if (sampleDelaySeconds > 0f)
            {
                yield return new WaitForSecondsRealtime(sampleDelaySeconds);
            }

            bool boundActorDispatched = context.SequenceRunner.TotalBoundActorCueCount > 0;
            bool routeSignalsPassed = bridgeMatched && directorMatched && runnerDispatched && boundActorDispatched;
            bool capturePassed = routeSignalsPassed
                && CaptureRouteFrame(context, label, report, capturedFrames);

            lastStepPassed = routeSignalsPassed && capturePassed;
            report.AppendLine($"DIRECT_ROUTE_RESULT {label}={(lastStepPassed ? "PASS" : "FAIL")}");
            report.AppendLine($"OBSERVED {label}=bridge:{bridgeMatched} director:{directorMatched} runner:{runnerDispatched} boundActor:{boundActorDispatched}");
            report.AppendLine($"BRIDGE {label}=count:{context.SequenceBridge.TotalPlayCount - bridgeCountBefore} kind:{context.SequenceBridge.LastPlayedKind} tier:{context.SequenceBridge.LastPlayedTier} profile:{ResolveSequenceId(context.SequenceBridge.LastPlayedProfile)}");
            report.AppendLine($"DIRECTOR {label}=count:{context.CueDirector.TotalPlayCount - directorCountBefore} kind:{context.CueDirector.LastPlayedKind} tier:{context.CueDirector.LastPlayedTier} cue:{context.CueDirector.LastPlayedCueId}");
            report.AppendLine($"RUNNER {label}=cameraCount:{cameraCueCountBefore}->{context.SequenceRunner.TotalCameraCueCount} actorCount:{actorCueCountBefore}->{context.SequenceRunner.TotalActorCueCount} boundActorCount:{boundActorCueCountBefore}->{context.SequenceRunner.TotalBoundActorCueCount} vfxCount:{vfxCueCountBefore}->{context.SequenceRunner.TotalVfxCueCount} lastCamera:{context.SequenceRunner.LastCameraCueId} lastActor:{context.SequenceRunner.LastActorCueId} lastVfx:{context.SequenceRunner.LastVfxCueId}");
        }

        private bool CaptureRouteFrame(
            ProbeContext context,
            string label,
            StringBuilder report,
            List<CapturedRouteFrame> capturedFrames)
        {
            Camera camera = context.SequenceRunner.CinematicCamera;
            if (camera == null)
            {
                report.AppendLine($"CAPTURE {label}=FAIL path:<none> reason:Missing cinematic camera.");
                return false;
            }

            string framePath = Path.Combine(
                    outputDirectory,
                    $"{capturedFrames.Count + 1:00}_{SanitizeFileName(label)}.png")
                .Replace('\\', '/');
            bool captured = CaptureCamera(camera, framePath, captureWidth, captureHeight);
            capturedFrames.Add(new CapturedRouteFrame
            {
                Label = label,
                Path = framePath,
                CaptureSucceeded = captured,
                SequenceId = ResolveSequenceId(context.SequenceRunner.SequenceProfile),
                LastCameraCueId = context.SequenceRunner.LastCameraCueId,
                LastActorCueId = context.SequenceRunner.LastActorCueId,
                LastVfxCueId = context.SequenceRunner.LastVfxCueId
            });
            report.AppendLine($"CAPTURE {label}={(captured ? "PASS" : "FAIL")} path:{framePath}");
            return captured;
        }

        private IEnumerator WaitForIdle(ProbeContext context, string label, StringBuilder report)
        {
            lastStepPassed = false;
            float startedAt = Time.realtimeSinceStartup;
            while (context.CueDirector.IsPlaying || context.SequenceRunner.IsPlaying)
            {
                if (Time.realtimeSinceStartup - startedAt > idleTimeoutSeconds)
                {
                    report.AppendLine($"IDLE_RESULT {label}=FAIL");
                    report.AppendLine($"REASON {label}=Timed out while cueDirector:{context.CueDirector.IsPlaying} runner:{context.SequenceRunner.IsPlaying}.");
                    yield break;
                }

                yield return null;
            }

            lastStepPassed = true;
            report.AppendLine($"IDLE_RESULT {label}=PASS");
        }

        private static ProbeContext ResolveContext()
        {
            ProbeContext context = new ProbeContext
            {
                Energy = FindRequired<SummonEnergyLadder>("summon energy ladder"),
                Skill1Action = FindRequired<PlayerSkill1Action>("player Skill1 action"),
                SummonSlot1Action = FindRequired<PlayerSummonSlot1Action>("player SummonSlot1 action"),
                CameraCueDriver = FindRequired<ActionCameraCueDriver>("action camera cue driver"),
                CueDirector = FindRequired<ActionCinematicCueDirector>("action cinematic cue director"),
                SequenceBridge = FindRequired<ActionCinematicSequenceBridge>("action cinematic sequence bridge")
            };

            context.SequenceRunner = context.SequenceBridge.Runner != null
                ? context.SequenceBridge.Runner
                : FindRequired<CinematicSequenceRunner>("cinematic sequence runner");
            return context;
        }

        private static T FindRequired<T>(string label) where T : UnityEngine.Object
        {
            T found = FindFirstObjectByType<T>();
            if (found == null)
            {
                throw new InvalidOperationException($"Missing {label}.");
            }

            return found;
        }

        private static void PrepareTierThreeEnergy(SummonEnergyLadder energy, float grantEnergy)
        {
            energy.SetGainEnabled(false);
            energy.ResetLadder();
            energy.GrantCurrentTierEnergy(grantEnergy);
        }

        private static void AppendDependencySnapshot(StringBuilder report, ProbeContext context)
        {
            report.AppendLine("DEPENDENCIES=PASS");
            report.AppendLine($"ENERGY={context.Energy.name}");
            report.AppendLine($"SKILL1={context.Skill1Action.name}");
            report.AppendLine($"SUMMON_SLOT1={context.SummonSlot1Action.name}");
            report.AppendLine($"CAMERA_CUE_DRIVER={context.CameraCueDriver.name}");
            report.AppendLine($"CUE_DIRECTOR={context.CueDirector.name}");
            report.AppendLine($"SEQUENCE_BRIDGE={context.SequenceBridge.name}");
            report.AppendLine($"SEQUENCE_RUNNER={context.SequenceRunner.name}");
        }

        private static string ResolveSequenceId(CinematicSequenceProfile profile)
        {
            return profile != null ? profile.SequenceId : "<none>";
        }

        private static bool HasChangedCue(string before, string after)
        {
            return !string.IsNullOrWhiteSpace(after)
                && !string.Equals(before, after, StringComparison.Ordinal);
        }

        private bool CaptureCamera(Camera camera, string path, int width, int height)
        {
            RenderTexture previousTarget = camera.targetTexture;
            RenderTexture previousActive = RenderTexture.active;
            RenderTexture renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            Texture2D image = new Texture2D(width, height, TextureFormat.RGBA32, mipChain: false);
            try
            {
                camera.targetTexture = renderTexture;
                RenderTexture.active = renderTexture;
                camera.Render();
                image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                image.Apply();

                if (!IsUsableTexture(image))
                {
                    return false;
                }

                Directory.CreateDirectory(Path.GetDirectoryName(path) ?? outputDirectory);
                File.WriteAllBytes(path, image.EncodeToPNG());
                return true;
            }
            finally
            {
                camera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                Destroy(image);
                Destroy(renderTexture);
            }
        }

        private static bool IsUsableTexture(Texture2D image)
        {
            if (image == null)
            {
                return false;
            }

            int readableSamples = 0;
            int sampleStepX = Mathf.Max(1, image.width / 16);
            int sampleStepY = Mathf.Max(1, image.height / 9);
            for (int y = 0; y < image.height; y += sampleStepY)
            {
                for (int x = 0; x < image.width; x += sampleStepX)
                {
                    Color pixel = image.GetPixel(x, y);
                    if (pixel.a > 0.05f && pixel.maxColorComponent > 0.03f)
                    {
                        readableSamples++;
                        if (readableSamples >= 6)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private static string SanitizeFileName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "route";
            }

            char[] invalid = Path.GetInvalidFileNameChars();
            StringBuilder builder = new StringBuilder(value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                char current = value[i];
                bool isInvalid = false;
                for (int j = 0; j < invalid.Length; j++)
                {
                    if (current == invalid[j])
                    {
                        isInvalid = true;
                        break;
                    }
                }

                builder.Append(isInvalid ? '_' : current);
            }

            return builder.ToString();
        }

        private static void AppendCaptureSummary(
            StringBuilder report,
            List<CapturedRouteFrame> capturedFrames)
        {
            report.AppendLine($"CAPTURE_COUNT={capturedFrames.Count}");
            for (int i = 0; i < capturedFrames.Count; i++)
            {
                CapturedRouteFrame frame = capturedFrames[i];
                report.AppendLine(
                    $"CAPTURE_FRAME {frame.Label}=success:{frame.CaptureSucceeded} sequence:{frame.SequenceId} camera:{frame.LastCameraCueId} actor:{frame.LastActorCueId} vfx:{frame.LastVfxCueId} path:{frame.Path}");
            }
        }

        private void WriteResult(bool passed, string details)
        {
            string resolvedPath = string.IsNullOrWhiteSpace(resultPath)
                ? "C:/tmp/DimensionBrawl-BossBarrageActionBridgeRoute.result"
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
                Debug.Log($"[BossBarrageActionBridgeRouteProbe] Passed. See {resolvedPath}.");
            }
            else
            {
                Debug.LogError($"[BossBarrageActionBridgeRouteProbe] Failed. See {resolvedPath}.");
            }
        }
    }
}
