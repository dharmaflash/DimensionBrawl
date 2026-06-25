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
        [SerializeField] private string routeStripPath =
            "C:/tmp/DimensionBrawl-BossBarrageActionBridgeRouteStrip.png";
        [SerializeField] private string routeReportPath =
            "C:/tmp/DimensionBrawl-BossBarrageActionBridgeRoute.md";
        [SerializeField, Min(1)] private int routeStripColumns = 2;
        [SerializeField, Min(64)] private int routeStripThumbnailWidth = 480;
        [SerializeField, Min(64)] private int routeStripThumbnailHeight = 270;
        [SerializeField] private bool requireDragonVisibilityForSummonRoutes = true;

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
            public bool ExpectedDragonVisible;
            public bool SupportDragonVisible;
            public string SupportDragonName;
            public int SupportDragonRendererCount;
            public bool SupportDragonInCameraFrustum;
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
            List<CapturedRouteFrame> capturedFrames = new List<CapturedRouteFrame>(9);

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

            yield return VerifyDirectorRoute(
                context,
                "pocket_clear_result_direct_bridge",
                ActionCinematicCueProfile.CueKind.PocketClear,
                "result_bridge",
                1.18f,
                report,
                capturedFrames);
            bool pocketClearRoutePassed = lastStepPassed;

            yield return WaitForIdle(context, "after_pocket_clear_result_direct_bridge", report);
            bool pocketClearIdlePassed = lastStepPassed;

            yield return VerifyDirectorRoute(
                context,
                "pocket_fail_danger_direct_bridge",
                ActionCinematicCueProfile.CueKind.PocketFail,
                "danger_cue",
                0.72f,
                report,
                capturedFrames);
            bool pocketFailRoutePassed = lastStepPassed;

            yield return WaitForIdle(context, "after_pocket_fail_danger_direct_bridge", report);
            bool pocketFailIdlePassed = lastStepPassed;

            bool passed = skillRoutePassed
                && skillIdlePassed
                && summonRoutePassed
                && summonIdlePassed
                && bossSummonPressureRoutePassed
                && bossSummonPressureIdlePassed
                && summonEmpowerRoutePassed
                && summonEmpowerIdlePassed
                && summonRecallRoutePassed
                && summonRecallIdlePassed
                && pocketClearRoutePassed
                && pocketClearIdlePassed
                && pocketFailRoutePassed
                && pocketFailIdlePassed;
            AppendCaptureSummary(report, capturedFrames);
            bool dragonVisibilityPassed = !requireDragonVisibilityForSummonRoutes
                || ValidateExpectedDragonVisibility(capturedFrames, report);
            bool routeEvidencePassed = WriteRouteEvidence(capturedFrames, report);
            passed = passed && dragonVisibilityPassed && routeEvidencePassed;
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
            bool expectedDragonVisible = IsDragonExpectedFrame(label);
            bool supportDragonVisible = TryResolveSupportDragonRead(
                camera,
                out string supportDragonName,
                out int supportDragonRendererCount,
                out bool supportDragonInCameraFrustum);
            capturedFrames.Add(new CapturedRouteFrame
            {
                Label = label,
                Path = framePath,
                CaptureSucceeded = captured,
                SequenceId = ResolveSequenceId(context.SequenceRunner.SequenceProfile),
                LastCameraCueId = context.SequenceRunner.LastCameraCueId,
                LastActorCueId = context.SequenceRunner.LastActorCueId,
                LastVfxCueId = context.SequenceRunner.LastVfxCueId,
                ExpectedDragonVisible = expectedDragonVisible,
                SupportDragonVisible = supportDragonVisible,
                SupportDragonName = supportDragonName,
                SupportDragonRendererCount = supportDragonRendererCount,
                SupportDragonInCameraFrustum = supportDragonInCameraFrustum
            });
            report.AppendLine($"CAPTURE {label}={(captured ? "PASS" : "FAIL")} path:{framePath}");
            report.AppendLine(
                $"DRAGON {label}=expected:{expectedDragonVisible} visible:{supportDragonVisible} frustum:{supportDragonInCameraFrustum} renderers:{supportDragonRendererCount} object:{NormalizeReportValue(supportDragonName)}");
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
                    $"CAPTURE_FRAME {frame.Label}=success:{frame.CaptureSucceeded} sequence:{frame.SequenceId} camera:{frame.LastCameraCueId} actor:{frame.LastActorCueId} vfx:{frame.LastVfxCueId} dragonExpected:{frame.ExpectedDragonVisible} dragonVisible:{frame.SupportDragonVisible} dragonFrustum:{frame.SupportDragonInCameraFrustum} dragonRenderers:{frame.SupportDragonRendererCount} dragon:{NormalizeReportValue(frame.SupportDragonName)} path:{frame.Path}");
            }
        }

        private bool ValidateExpectedDragonVisibility(
            List<CapturedRouteFrame> capturedFrames,
            StringBuilder report)
        {
            bool passed = true;
            for (int i = 0; i < capturedFrames.Count; i++)
            {
                CapturedRouteFrame frame = capturedFrames[i];
                if (!frame.ExpectedDragonVisible || frame.SupportDragonVisible)
                {
                    continue;
                }

                passed = false;
                report.AppendLine(
                    $"DRAGON_VISIBILITY_RESULT {frame.Label}=FAIL expected support dragon in camera frame.");
            }

            report.AppendLine($"DRAGON_VISIBILITY_RESULT={(passed ? "PASS" : "FAIL")}");
            return passed;
        }

        private bool WriteRouteEvidence(List<CapturedRouteFrame> capturedFrames, StringBuilder report)
        {
            try
            {
                CapturedRouteFrame[] frames = capturedFrames.ToArray();
                CreateRouteContactSheet(
                    frames,
                    routeStripPath,
                    routeStripThumbnailWidth,
                    routeStripThumbnailHeight,
                    routeStripColumns);
                WriteRouteReport(frames);
                report.AppendLine("ROUTE_EVIDENCE=PASS");
                report.AppendLine($"ROUTE_STRIP={routeStripPath}");
                report.AppendLine($"ROUTE_REPORT={routeReportPath}");
                return true;
            }
            catch (Exception exception)
            {
                report.AppendLine("ROUTE_EVIDENCE=FAIL");
                report.AppendLine(exception.ToString());
                return false;
            }
        }

        private static bool IsDragonExpectedFrame(string label)
        {
            if (string.IsNullOrWhiteSpace(label)
                || label.IndexOf("summon", StringComparison.OrdinalIgnoreCase) < 0)
            {
                return false;
            }

            return !label.EndsWith("_command", StringComparison.OrdinalIgnoreCase);
        }

        private static bool TryResolveSupportDragonRead(
            Camera camera,
            out string supportDragonName,
            out int supportDragonRendererCount,
            out bool supportDragonInCameraFrustum)
        {
            supportDragonName = string.Empty;
            supportDragonRendererCount = 0;
            supportDragonInCameraFrustum = false;

            Transform[] transforms = FindObjectsByType<Transform>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int i = 0; i < transforms.Length; i++)
            {
                Transform candidate = transforms[i];
                if (candidate == null || !IsSupportDragonCandidate(candidate))
                {
                    continue;
                }

                if (!candidate.gameObject.activeInHierarchy)
                {
                    supportDragonName = candidate.name;
                    continue;
                }

                if (!TryBuildRendererBounds(
                        candidate.gameObject,
                        out Bounds bounds,
                        out int rendererCount))
                {
                    supportDragonName = candidate.name;
                    continue;
                }

                bool inFrustum = camera == null
                    || GeometryUtility.TestPlanesAABB(
                        GeometryUtility.CalculateFrustumPlanes(camera),
                        bounds);
                supportDragonName = candidate.name;
                supportDragonRendererCount = rendererCount;
                supportDragonInCameraFrustum = inFrustum;
                return inFrustum && rendererCount > 0;
            }

            return false;
        }

        private static bool IsSupportDragonCandidate(Transform candidate)
        {
            string name = candidate.name;
            if (name.IndexOf("CinematicSupportDragon", StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("VolcanoDragon", StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("Volcano Dragon", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            return name.IndexOf("Dragon", StringComparison.OrdinalIgnoreCase) >= 0
                && candidate.GetComponentInChildren<Animator>(includeInactive: true) != null;
        }

        private static bool TryBuildRendererBounds(
            GameObject root,
            out Bounds bounds,
            out int rendererCount)
        {
            bounds = default;
            rendererCount = 0;
            if (root == null)
            {
                return false;
            }

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(includeInactive: false);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy)
                {
                    continue;
                }

                if (rendererCount == 0)
                {
                    bounds = renderer.bounds;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }

                rendererCount++;
            }

            return rendererCount > 0;
        }

        private void CreateRouteContactSheet(
            CapturedRouteFrame[] frames,
            string outputPath,
            int thumbnailWidth,
            int thumbnailHeight,
            int columns)
        {
            if (frames == null || frames.Length == 0)
            {
                throw new InvalidOperationException("Cannot create route contact sheet without captured frames.");
            }

            int resolvedColumns = Mathf.Max(1, columns);
            int rows = Mathf.CeilToInt(frames.Length / (float)resolvedColumns);
            Texture2D sheet = new Texture2D(
                thumbnailWidth * resolvedColumns,
                thumbnailHeight * rows,
                TextureFormat.RGBA32,
                mipChain: false);

            try
            {
                Color[] background = new Color[sheet.width * sheet.height];
                Color backgroundColor = new Color(0.045f, 0.052f, 0.064f, 1f);
                for (int i = 0; i < background.Length; i++)
                {
                    background[i] = backgroundColor;
                }

                sheet.SetPixels(background);
                for (int i = 0; i < frames.Length; i++)
                {
                    CapturedRouteFrame frame = frames[i];
                    int column = i % resolvedColumns;
                    int row = i / resolvedColumns;
                    int targetX = column * thumbnailWidth;
                    int targetY = sheet.height - ((row + 1) * thumbnailHeight);

                    if (File.Exists(frame.Path))
                    {
                        Texture2D source = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: false);
                        Texture2D resized = null;
                        try
                        {
                            if (!source.LoadImage(File.ReadAllBytes(frame.Path)))
                            {
                                throw new InvalidOperationException($"Failed to load route frame {frame.Path}.");
                            }

                            resized = ResizeTexture(source, thumbnailWidth, thumbnailHeight);
                            sheet.SetPixels(targetX, targetY, thumbnailWidth, thumbnailHeight, resized.GetPixels());
                        }
                        finally
                        {
                            UnityEngine.Object.Destroy(source);
                            if (resized != null)
                            {
                                UnityEngine.Object.Destroy(resized);
                            }
                        }
                    }

                    DrawLabelOverlay(sheet, targetX, targetY, thumbnailWidth, thumbnailHeight, BuildRouteLabelLines(frame, i + 1));
                }

                sheet.Apply();
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? "C:/tmp");
                File.WriteAllBytes(outputPath, sheet.EncodeToPNG());
            }
            finally
            {
                UnityEngine.Object.Destroy(sheet);
            }
        }

        private static Texture2D ResizeTexture(Texture2D source, int width, int height)
        {
            Texture2D resized = new Texture2D(width, height, TextureFormat.RGBA32, mipChain: false);
            Color[] pixels = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                float v = height > 1 ? y / (float)(height - 1) : 0f;
                for (int x = 0; x < width; x++)
                {
                    float u = width > 1 ? x / (float)(width - 1) : 0f;
                    pixels[x + (y * width)] = source.GetPixelBilinear(u, v);
                }
            }

            resized.SetPixels(pixels);
            resized.Apply();
            return resized;
        }

        private void WriteRouteReport(CapturedRouteFrame[] frames)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# DimensionBrawl Boss Barrage Action Bridge Route Capture");
            builder.AppendLine();
            builder.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            builder.AppendLine($"Contact sheet: `{routeStripPath}`");
            builder.AppendLine($"Frame directory: `{outputDirectory}`");
            builder.AppendLine();
            builder.AppendLine("This route is captured from the boss-barrage review scene after invoking the action bridge path. Dragon visibility is required on summon-labeled frames so support-creature use cannot silently regress.");
            builder.AppendLine();
            builder.AppendLine("| # | Label | Sequence | Camera | Actor | VFX | Dragon expected | Dragon visible | Dragon object | Frame |");
            builder.AppendLine("|---|-------|----------|--------|-------|-----|-----------------|----------------|---------------|-------|");

            for (int i = 0; i < frames.Length; i++)
            {
                CapturedRouteFrame frame = frames[i];
                builder.AppendLine(
                    $"| {i + 1} | {frame.Label} | `{frame.SequenceId}` | `{frame.LastCameraCueId}` | `{frame.LastActorCueId}` | `{frame.LastVfxCueId}` | {frame.ExpectedDragonVisible} | {frame.SupportDragonVisible} ({frame.SupportDragonRendererCount} renderers, frustum {frame.SupportDragonInCameraFrustum}) | `{NormalizeReportValue(frame.SupportDragonName)}` | `{frame.Path}` |");
            }

            Directory.CreateDirectory(Path.GetDirectoryName(routeReportPath) ?? "C:/tmp");
            File.WriteAllText(routeReportPath, builder.ToString(), Encoding.UTF8);
        }

        private static string[] BuildRouteLabelLines(CapturedRouteFrame frame, int index)
        {
            return new[]
            {
                $"{index:00} {NormalizeLabel(frame.Label)}",
                $"SEQ {NormalizeLabel(frame.SequenceId)}",
                $"CAM {NormalizeLabel(frame.LastCameraCueId)}",
                frame.ExpectedDragonVisible
                    ? $"DRAGON {(frame.SupportDragonVisible ? "VISIBLE" : "MISSING")}"
                    : "DRAGON N/A"
            };
        }

        private static string NormalizeLabel(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "NONE";
            }

            return value
                .Replace('_', ' ')
                .Replace('-', ' ')
                .ToUpperInvariant();
        }

        private static string NormalizeReportValue(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "<none>" : value.Replace('|', '/');
        }

        private static void DrawLabelOverlay(Texture2D target, int tileX, int tileY, int tileWidth, int tileHeight, string[] lines)
        {
            if (target == null || lines == null || lines.Length == 0)
            {
                return;
            }

            const int scale = 2;
            const int glyphHeight = 7;
            const int lineGap = 3;
            int panelHeight = Mathf.Clamp(
                10 + (lines.Length * glyphHeight * scale) + ((lines.Length - 1) * lineGap),
                42,
                Mathf.Max(42, tileHeight / 2));
            int panelX = tileX + 4;
            int panelY = tileY + tileHeight - panelHeight - 4;
            int panelWidth = Mathf.Max(1, tileWidth - 8);

            DrawFilledRect(target, panelX, panelY, panelWidth, panelHeight, new Color(0.02f, 0.025f, 0.035f, 0.78f));
            DrawFilledRect(target, panelX, panelY, 4, panelHeight, new Color(0.28f, 0.75f, 1f, 0.9f));

            int textX = panelX + 10;
            int textWidth = Mathf.Max(16, panelWidth - 18);
            for (int i = 0; i < lines.Length; i++)
            {
                string line = TrimForPixelWidth(lines[i], textWidth, scale);
                int lineBottomY = panelY + panelHeight - 7 - ((i + 1) * glyphHeight * scale) - (i * lineGap);
                Color color = i == 0
                    ? new Color(1f, 0.93f, 0.45f, 1f)
                    : Color.white;
                DrawBitmapText(target, line, textX + 1, lineBottomY - 1, scale, new Color(0f, 0f, 0f, 0.85f));
                DrawBitmapText(target, line, textX, lineBottomY, scale, color);
            }
        }

        private static void DrawFilledRect(Texture2D target, int x, int y, int width, int height, Color color)
        {
            int maxX = Mathf.Min(target.width, x + width);
            int maxY = Mathf.Min(target.height, y + height);
            for (int yy = Mathf.Max(0, y); yy < maxY; yy++)
            {
                for (int xx = Mathf.Max(0, x); xx < maxX; xx++)
                {
                    Color existing = target.GetPixel(xx, yy);
                    target.SetPixel(xx, yy, Color.Lerp(existing, color, color.a));
                }
            }
        }

        private static string TrimForPixelWidth(string value, int maxWidth, int scale)
        {
            string resolved = string.IsNullOrWhiteSpace(value) ? "NONE" : value;
            if (MeasureBitmapTextWidth(resolved, scale) <= maxWidth)
            {
                return resolved;
            }

            const string suffix = "...";
            while (resolved.Length > 0 && MeasureBitmapTextWidth(resolved + suffix, scale) > maxWidth)
            {
                resolved = resolved.Substring(0, resolved.Length - 1);
            }

            return string.IsNullOrEmpty(resolved) ? suffix : resolved + suffix;
        }

        private static int MeasureBitmapTextWidth(string text, int scale)
        {
            if (string.IsNullOrEmpty(text))
            {
                return 0;
            }

            int width = 0;
            for (int i = 0; i < text.Length; i++)
            {
                width += ((text[i] == ' ' ? 3 : 5) * scale) + scale;
            }

            return Mathf.Max(0, width - scale);
        }

        private static void DrawBitmapText(Texture2D target, string text, int x, int bottomY, int scale, Color color)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            int cursorX = x;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                string[] glyph = GetBitmapGlyph(c);
                if (glyph != null)
                {
                    DrawGlyph(target, glyph, cursorX, bottomY, scale, color);
                }

                cursorX += ((c == ' ' ? 3 : 5) * scale) + scale;
            }
        }

        private static void DrawGlyph(Texture2D target, string[] glyph, int x, int bottomY, int scale, Color color)
        {
            for (int row = 0; row < glyph.Length; row++)
            {
                string pattern = glyph[row];
                for (int column = 0; column < pattern.Length; column++)
                {
                    if (pattern[column] != '1')
                    {
                        continue;
                    }

                    int baseX = x + (column * scale);
                    int baseY = bottomY + ((glyph.Length - 1 - row) * scale);
                    for (int yy = 0; yy < scale; yy++)
                    {
                        int pixelY = baseY + yy;
                        if (pixelY < 0 || pixelY >= target.height)
                        {
                            continue;
                        }

                        for (int xx = 0; xx < scale; xx++)
                        {
                            int pixelX = baseX + xx;
                            if (pixelX < 0 || pixelX >= target.width)
                            {
                                continue;
                            }

                            Color existing = target.GetPixel(pixelX, pixelY);
                            target.SetPixel(pixelX, pixelY, Color.Lerp(existing, color, color.a));
                        }
                    }
                }
            }
        }

        private static string[] GetBitmapGlyph(char c)
        {
            switch (char.ToUpperInvariant(c))
            {
                case 'A': return new[] { "01110", "10001", "10001", "11111", "10001", "10001", "10001" };
                case 'B': return new[] { "11110", "10001", "10001", "11110", "10001", "10001", "11110" };
                case 'C': return new[] { "01111", "10000", "10000", "10000", "10000", "10000", "01111" };
                case 'D': return new[] { "11110", "10001", "10001", "10001", "10001", "10001", "11110" };
                case 'E': return new[] { "11111", "10000", "10000", "11110", "10000", "10000", "11111" };
                case 'F': return new[] { "11111", "10000", "10000", "11110", "10000", "10000", "10000" };
                case 'G': return new[] { "01111", "10000", "10000", "10011", "10001", "10001", "01111" };
                case 'H': return new[] { "10001", "10001", "10001", "11111", "10001", "10001", "10001" };
                case 'I': return new[] { "11111", "00100", "00100", "00100", "00100", "00100", "11111" };
                case 'J': return new[] { "00111", "00010", "00010", "00010", "00010", "10010", "01100" };
                case 'K': return new[] { "10001", "10010", "10100", "11000", "10100", "10010", "10001" };
                case 'L': return new[] { "10000", "10000", "10000", "10000", "10000", "10000", "11111" };
                case 'M': return new[] { "10001", "11011", "10101", "10101", "10001", "10001", "10001" };
                case 'N': return new[] { "10001", "11001", "10101", "10011", "10001", "10001", "10001" };
                case 'O': return new[] { "01110", "10001", "10001", "10001", "10001", "10001", "01110" };
                case 'P': return new[] { "11110", "10001", "10001", "11110", "10000", "10000", "10000" };
                case 'Q': return new[] { "01110", "10001", "10001", "10001", "10101", "10010", "01101" };
                case 'R': return new[] { "11110", "10001", "10001", "11110", "10100", "10010", "10001" };
                case 'S': return new[] { "01111", "10000", "10000", "01110", "00001", "00001", "11110" };
                case 'T': return new[] { "11111", "00100", "00100", "00100", "00100", "00100", "00100" };
                case 'U': return new[] { "10001", "10001", "10001", "10001", "10001", "10001", "01110" };
                case 'V': return new[] { "10001", "10001", "10001", "10001", "01010", "01010", "00100" };
                case 'W': return new[] { "10001", "10001", "10001", "10101", "10101", "10101", "01010" };
                case 'X': return new[] { "10001", "01010", "00100", "00100", "00100", "01010", "10001" };
                case 'Y': return new[] { "10001", "01010", "00100", "00100", "00100", "00100", "00100" };
                case 'Z': return new[] { "11111", "00001", "00010", "00100", "01000", "10000", "11111" };
                case '0': return new[] { "01110", "10001", "10011", "10101", "11001", "10001", "01110" };
                case '1': return new[] { "00100", "01100", "00100", "00100", "00100", "00100", "01110" };
                case '2': return new[] { "01110", "10001", "00001", "00010", "00100", "01000", "11111" };
                case '3': return new[] { "11110", "00001", "00001", "01110", "00001", "00001", "11110" };
                case '4': return new[] { "00010", "00110", "01010", "10010", "11111", "00010", "00010" };
                case '5': return new[] { "11111", "10000", "10000", "11110", "00001", "00001", "11110" };
                case '6': return new[] { "01110", "10000", "10000", "11110", "10001", "10001", "01110" };
                case '7': return new[] { "11111", "00001", "00010", "00100", "01000", "01000", "01000" };
                case '8': return new[] { "01110", "10001", "10001", "01110", "10001", "10001", "01110" };
                case '9': return new[] { "01110", "10001", "10001", "01111", "00001", "00001", "01110" };
                case '.': return new[] { "00000", "00000", "00000", "00000", "00000", "01100", "01100" };
                case ':': return new[] { "00000", "01100", "01100", "00000", "01100", "01100", "00000" };
                case '+': return new[] { "00000", "00100", "00100", "11111", "00100", "00100", "00000" };
                case '-': return new[] { "00000", "00000", "00000", "11111", "00000", "00000", "00000" };
                case '/': return new[] { "00001", "00010", "00010", "00100", "01000", "01000", "10000" };
                case '?': return new[] { "01110", "10001", "00001", "00010", "00100", "00000", "00100" };
                case ' ': return new[] { "00000", "00000", "00000", "00000", "00000", "00000", "00000" };
                default: return GetBitmapGlyph('?');
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

            string body = $"RESULT={(passed ? "PASS" : "FAIL")}{Environment.NewLine}STRIP={routeStripPath}{Environment.NewLine}REPORT={routeReportPath}{Environment.NewLine}{details}";
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
