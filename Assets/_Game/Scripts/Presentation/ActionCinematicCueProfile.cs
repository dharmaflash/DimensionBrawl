using System;
using UnityEngine;

namespace DimensionBrawl.Presentation
{
    [CreateAssetMenu(menuName = "DimensionBrawl/Profiles/Action Cinematic Cue Profile")]
    public sealed class ActionCinematicCueProfile : ScriptableObject
    {
        public enum CueKind
        {
            SkillCutIn,
            SummonEntry,
            UltimateCutIn,
            BossPressureBreak,
            SummonFollowupHit,
            PocketClear,
            PocketFail,
            SummonEmpower,
            SummonRecall
        }

        public enum CueTier
        {
            CombatCue,
            MicroCinematic,
            CombatCutIn
        }

        public enum CameraReturnPolicy
        {
            ActionCameraCueRecovery
        }

        public const string GameplayReturnTargetId = "action_camera_controller";

        [Serializable]
        public struct CameraShot
        {
            public bool enabled;
            public Vector3 localOffset;
            public float planarDirectionOffset;
            public float fieldOfViewDelta;
            public float cameraDistanceDelta;
            public float focusHeightDelta;
            public float durationSeconds;
            public float pauseAfterSeconds;
            public float tierScale;
        }

        [Serializable]
        public struct CueSequence
        {
            public bool enabled;
            public string cueId;
            public CueTier tier;
            public int priority;
            public bool canBeInterrupted;
            public float movementLockSeconds;
            public float inputLockSeconds;
            public float timeScale;
            public float timeScaleSeconds;
            public string returnTargetId;
            public CameraReturnPolicy returnPolicy;
            public CameraShot[] shots;
            public CueSignal[] signals;

            public int ShotCount => shots != null ? shots.Length : 0;
            public int SignalCount => signals != null ? signals.Length : 0;
        }

        [Serializable]
        public struct CueSignal
        {
            public bool enabled;
            public float delaySeconds;
            public string signalId;
            public string animatorTrigger;
            public bool requireAnimatorTrigger;
            public bool playVfx;
            public CombatVfxCueId vfxCueId;
            public float vfxIntensity;
            public float tierIntensityScale;
        }

        [Header("Player Actions")]
        [SerializeField] private CueSequence skillCutIn = new CueSequence
        {
            enabled = true,
            cueId = "skill1_short_cutin",
            tier = CueTier.CombatCue,
            priority = 35,
            canBeInterrupted = true,
            movementLockSeconds = 0.16f,
            inputLockSeconds = 0.22f,
            timeScale = 1f,
            timeScaleSeconds = 0f,
            returnTargetId = GameplayReturnTargetId,
            returnPolicy = CameraReturnPolicy.ActionCameraCueRecovery,
            shots = new[]
            {
                CreateShot(new Vector3(-0.10f, 0.06f, -0.18f), 0.10f, 1.4f, -0.12f, 0.05f, 0.16f, 0.02f, 1.08f),
                CreateShot(new Vector3(0.05f, 0.02f, 0.18f), 0.08f, -2.1f, 0.18f, 0.02f, 0.18f, 0.02f, 1.18f),
                CreateShot(new Vector3(0f, 0.04f, -0.10f), 0.04f, 0.7f, -0.08f, 0.02f, 0.18f, 0f, 1.04f)
            },
            signals = new[]
            {
                CreateSignal("skill1_fire_signal", 0.08f, "SHOOT", CombatVfxCueId.PlayerRangedMuzzleFlash, 1.05f)
            }
        };

        [SerializeField] private CueSequence summonEntry = new CueSequence
        {
            enabled = true,
            cueId = "summon_break_entry_cutin",
            tier = CueTier.MicroCinematic,
            priority = 60,
            canBeInterrupted = false,
            movementLockSeconds = 0.45f,
            inputLockSeconds = 0.55f,
            timeScale = 0.92f,
            timeScaleSeconds = 0.16f,
            returnTargetId = GameplayReturnTargetId,
            returnPolicy = CameraReturnPolicy.ActionCameraCueRecovery,
            shots = new[]
            {
                CreateShot(new Vector3(0f, 0.08f, -0.22f), 0.18f, 2.8f, -0.30f, 0.09f, 0.18f, 0.03f, 1.12f),
                CreateShot(new Vector3(-0.18f, 0.05f, 0.12f), 0.08f, -2.2f, 0.18f, 0.03f, 0.24f, 0.02f, 1.22f),
                CreateShot(new Vector3(0.16f, 0.09f, -0.24f), 0.16f, 3.0f, -0.32f, 0.10f, 0.28f, 0.04f, 1.18f),
                CreateShot(new Vector3(0f, 0.04f, -0.10f), 0.04f, 0.8f, -0.08f, 0.04f, 0.18f, 0f, 1.04f)
            },
            signals = new[]
            {
                CreateSignal("summon_spawn_signal", 0.10f, string.Empty, CombatVfxCueId.EliteSummonSignal, 1.12f),
                CreateSignal("summon_landing_signal", 0.42f, string.Empty, CombatVfxCueId.SummonBlockOpportunity, 0.92f)
            }
        };

        [SerializeField] private CueSequence ultimateCutIn = new CueSequence
        {
            enabled = true,
            cueId = "skill1_lv3_ultimate_short_cutin",
            tier = CueTier.CombatCutIn,
            priority = 85,
            canBeInterrupted = false,
            movementLockSeconds = 0.62f,
            inputLockSeconds = 0.74f,
            timeScale = 0.84f,
            timeScaleSeconds = 0.24f,
            returnTargetId = GameplayReturnTargetId,
            returnPolicy = CameraReturnPolicy.ActionCameraCueRecovery,
            shots = new[]
            {
                CreateShot(new Vector3(-0.16f, 0.08f, -0.16f), 0.10f, 2.4f, -0.18f, 0.06f, 0.26f, 0.04f, 1.1f),
                CreateShot(new Vector3(0.10f, 0.03f, 0.22f), 0.08f, -3.2f, 0.28f, 0.03f, 0.32f, 0.03f, 1.28f),
                CreateShot(new Vector3(-0.06f, 0.08f, -0.24f), 0.20f, 3.6f, -0.36f, 0.10f, 0.30f, 0.04f, 1.24f),
                CreateShot(new Vector3(0f, 0.04f, -0.08f), 0.04f, 0.9f, -0.10f, 0.03f, 0.22f, 0f, 1.05f)
            },
            signals = new[]
            {
                CreateSignal("ultimate_charge_signal", 0.12f, "AUTO SHOOT", CombatVfxCueId.ElitePhaseSwapSignal, 1.18f),
                CreateSignal("ultimate_impact_signal", 0.58f, string.Empty, CombatVfxCueId.SummonFollowupHit, 1.25f)
            }
        };

        [Header("Boss Pocket")]
        [SerializeField] private CueSequence bossPressureBreak = new CueSequence
        {
            enabled = true,
            cueId = "boss_pressure_break_reframe",
            tier = CueTier.MicroCinematic,
            priority = 65,
            canBeInterrupted = true,
            movementLockSeconds = 0.18f,
            inputLockSeconds = 0.24f,
            timeScale = 0.9f,
            timeScaleSeconds = 0.12f,
            returnTargetId = GameplayReturnTargetId,
            returnPolicy = CameraReturnPolicy.ActionCameraCueRecovery,
            shots = new[]
            {
                CreateShot(new Vector3(0.18f, 0.07f, -0.24f), 0.18f, 2.8f, -0.30f, 0.09f, 0.22f, 0.03f, 1.14f),
                CreateShot(new Vector3(-0.14f, 0.04f, 0.10f), 0.08f, -1.8f, 0.14f, 0.02f, 0.24f, 0.02f, 1.2f),
                CreateShot(new Vector3(0f, 0.05f, -0.14f), 0.08f, 1.4f, -0.16f, 0.04f, 0.22f, 0f, 1.08f)
            },
            signals = new[]
            {
                CreateSignal("pressure_break_signal", 0.10f, string.Empty, CombatVfxCueId.SummonFollowupWindow, 1.0f)
            }
        };

        [SerializeField] private CueSequence summonFollowupHit = new CueSequence
        {
            enabled = true,
            cueId = "summon_followup_hit_confirm",
            tier = CueTier.MicroCinematic,
            priority = 72,
            canBeInterrupted = true,
            movementLockSeconds = 0.12f,
            inputLockSeconds = 0.18f,
            timeScale = 0.88f,
            timeScaleSeconds = 0.10f,
            returnTargetId = GameplayReturnTargetId,
            returnPolicy = CameraReturnPolicy.ActionCameraCueRecovery,
            shots = new[]
            {
                CreateShot(new Vector3(-0.08f, 0.05f, -0.10f), 0.08f, 1.6f, -0.14f, 0.04f, 0.14f, 0.02f, 1.08f),
                CreateShot(new Vector3(0.08f, 0.03f, 0.22f), 0.12f, -3.0f, 0.24f, 0.03f, 0.22f, 0.03f, 1.28f),
                CreateShot(new Vector3(0f, 0.04f, -0.08f), 0.04f, 0.8f, -0.08f, 0.02f, 0.18f, 0f, 1.04f)
            },
            signals = new[]
            {
                CreateSignal("followup_hit_signal", 0.08f, string.Empty, CombatVfxCueId.SummonFollowupHit, 1.16f)
            }
        };

        [SerializeField] private CueSequence summonEmpower = new CueSequence
        {
            enabled = true,
            cueId = "summon_empower_transfer_micro",
            tier = CueTier.MicroCinematic,
            priority = 74,
            canBeInterrupted = true,
            movementLockSeconds = 0.26f,
            inputLockSeconds = 0.34f,
            timeScale = 0.9f,
            timeScaleSeconds = 0.12f,
            returnTargetId = GameplayReturnTargetId,
            returnPolicy = CameraReturnPolicy.ActionCameraCueRecovery,
            shots = new[]
            {
                CreateShot(new Vector3(-0.10f, 0.05f, -0.14f), 0.08f, 1.4f, -0.16f, 0.04f, 0.18f, 0.02f, 1.08f),
                CreateShot(new Vector3(0.08f, 0.05f, 0.18f), 0.10f, -2.4f, 0.22f, 0.02f, 0.24f, 0.02f, 1.22f),
                CreateShot(new Vector3(0f, 0.04f, -0.08f), 0.04f, 0.8f, -0.08f, 0.02f, 0.18f, 0f, 1.04f)
            },
            signals = new[]
            {
                CreateSignal("summon_empower_transfer_signal", 0.10f, string.Empty, CombatVfxCueId.EliteAuraSignal, 1.05f),
                CreateSignal("summon_empower_guard_signal", 0.42f, string.Empty, CombatVfxCueId.EliteShieldSignal, 0.92f)
            }
        };

        [SerializeField] private CueSequence summonRecall = new CueSequence
        {
            enabled = true,
            cueId = "summon_recall_collapse_micro",
            tier = CueTier.MicroCinematic,
            priority = 70,
            canBeInterrupted = true,
            movementLockSeconds = 0.22f,
            inputLockSeconds = 0.30f,
            timeScale = 0.92f,
            timeScaleSeconds = 0.10f,
            returnTargetId = GameplayReturnTargetId,
            returnPolicy = CameraReturnPolicy.ActionCameraCueRecovery,
            shots = new[]
            {
                CreateShot(new Vector3(-0.08f, 0.04f, -0.12f), 0.08f, 1.2f, -0.14f, 0.04f, 0.18f, 0.02f, 1.05f),
                CreateShot(new Vector3(0.04f, 0.02f, 0.14f), 0.08f, -1.8f, 0.16f, 0.02f, 0.22f, 0.02f, 1.18f),
                CreateShot(new Vector3(0f, 0.04f, -0.08f), 0.04f, 0.8f, -0.08f, 0.02f, 0.18f, 0f, 1.04f)
            },
            signals = new[]
            {
                CreateSignal("summon_recall_collapse_signal", 0.08f, string.Empty, CombatVfxCueId.SummonBlockOpportunity, 0.88f),
                CreateSignal("summon_recall_exit_signal", 0.52f, string.Empty, CombatVfxCueId.PocketCleared, 0.72f)
            }
        };

        [SerializeField] private CueSequence pocketClear = new CueSequence
        {
            enabled = true,
            cueId = "pocket_result_clear_bridge",
            tier = CueTier.MicroCinematic,
            priority = 75,
            canBeInterrupted = false,
            movementLockSeconds = 0.30f,
            inputLockSeconds = 0.42f,
            timeScale = 0.86f,
            timeScaleSeconds = 0.14f,
            returnTargetId = GameplayReturnTargetId,
            returnPolicy = CameraReturnPolicy.ActionCameraCueRecovery,
            shots = new[]
            {
                CreateShot(new Vector3(0f, 0.08f, -0.28f), 0.20f, 3.2f, -0.34f, 0.10f, 0.26f, 0.04f, 1.12f),
                CreateShot(new Vector3(0.12f, 0.04f, 0.16f), 0.10f, -2.4f, 0.20f, 0.04f, 0.26f, 0.04f, 1.22f),
                CreateShot(new Vector3(0f, 0.05f, -0.12f), 0.04f, 0.9f, -0.10f, 0.04f, 0.22f, 0f, 1.04f)
            },
            signals = new[]
            {
                CreateSignal("pocket_clear_signal", 0.12f, string.Empty, CombatVfxCueId.PocketCleared, 0.98f)
            }
        };

        [SerializeField] private CueSequence pocketFail = new CueSequence
        {
            enabled = true,
            cueId = "pocket_result_fail_bridge",
            tier = CueTier.MicroCinematic,
            priority = 75,
            canBeInterrupted = false,
            movementLockSeconds = 0.24f,
            inputLockSeconds = 0.34f,
            timeScale = 0.92f,
            timeScaleSeconds = 0.10f,
            returnTargetId = GameplayReturnTargetId,
            returnPolicy = CameraReturnPolicy.ActionCameraCueRecovery,
            shots = new[]
            {
                CreateShot(new Vector3(0f, -0.04f, -0.18f), -0.06f, 1.8f, -0.20f, -0.04f, 0.24f, 0.03f, 1.05f),
                CreateShot(new Vector3(-0.08f, 0.02f, -0.10f), -0.04f, 0.6f, -0.08f, -0.03f, 0.22f, 0f, 1f)
            },
            signals = new[]
            {
                CreateSignal("pocket_fail_signal", 0.08f, string.Empty, CombatVfxCueId.PocketFailed, 1.02f)
            }
        };

        public CueSequence SkillCutIn => skillCutIn;
        public CueSequence SummonEntry => summonEntry;
        public CueSequence UltimateCutIn => ultimateCutIn;
        public CueSequence BossPressureBreak => bossPressureBreak;
        public CueSequence SummonFollowupHit => summonFollowupHit;
        public CueSequence SummonEmpower => summonEmpower;
        public CueSequence SummonRecall => summonRecall;
        public CueSequence PocketClear => pocketClear;
        public CueSequence PocketFail => pocketFail;

        public bool TryGetSequence(CueKind kind, out CueSequence sequence)
        {
            sequence = kind switch
            {
                CueKind.SkillCutIn => skillCutIn,
                CueKind.SummonEntry => summonEntry,
                CueKind.UltimateCutIn => ultimateCutIn,
                CueKind.BossPressureBreak => bossPressureBreak,
                CueKind.SummonFollowupHit => summonFollowupHit,
                CueKind.SummonEmpower => summonEmpower,
                CueKind.SummonRecall => summonRecall,
                CueKind.PocketClear => pocketClear,
                CueKind.PocketFail => pocketFail,
                _ => default
            };

            return sequence.enabled && sequence.ShotCount > 0;
        }

        private static CameraShot CreateShot(
            Vector3 localOffset,
            float planarDirectionOffset,
            float fieldOfViewDelta,
            float cameraDistanceDelta,
            float focusHeightDelta,
            float durationSeconds,
            float pauseAfterSeconds,
            float tierScale)
        {
            return new CameraShot
            {
                enabled = true,
                localOffset = localOffset,
                planarDirectionOffset = planarDirectionOffset,
                fieldOfViewDelta = fieldOfViewDelta,
                cameraDistanceDelta = cameraDistanceDelta,
                focusHeightDelta = focusHeightDelta,
                durationSeconds = durationSeconds,
                pauseAfterSeconds = pauseAfterSeconds,
                tierScale = tierScale
            };
        }

        private static CueSignal CreateSignal(
            string signalId,
            float delaySeconds,
            string animatorTrigger,
            CombatVfxCueId vfxCueId,
            float vfxIntensity,
            float tierIntensityScale = 1.18f)
        {
            return new CueSignal
            {
                enabled = true,
                delaySeconds = delaySeconds,
                signalId = signalId,
                animatorTrigger = animatorTrigger,
                requireAnimatorTrigger = !string.IsNullOrWhiteSpace(animatorTrigger),
                playVfx = true,
                vfxCueId = vfxCueId,
                vfxIntensity = vfxIntensity,
                tierIntensityScale = tierIntensityScale
            };
        }
    }
}
