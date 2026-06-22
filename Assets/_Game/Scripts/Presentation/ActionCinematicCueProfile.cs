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
            PocketFail
        }

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
            public int priority;
            public bool canBeInterrupted;
            public float timeScale;
            public float timeScaleSeconds;
            public CameraShot[] shots;

            public int ShotCount => shots != null ? shots.Length : 0;
        }

        [Header("Player Actions")]
        [SerializeField] private CueSequence skillCutIn = new CueSequence
        {
            enabled = true,
            cueId = "skill1_short_cutin",
            priority = 35,
            canBeInterrupted = true,
            timeScale = 1f,
            timeScaleSeconds = 0f,
            shots = new[]
            {
                CreateShot(new Vector3(-0.10f, 0.06f, -0.18f), 0.10f, 1.4f, -0.12f, 0.05f, 0.16f, 0.02f, 1.08f),
                CreateShot(new Vector3(0.05f, 0.02f, 0.18f), 0.08f, -2.1f, 0.18f, 0.02f, 0.18f, 0.02f, 1.18f),
                CreateShot(new Vector3(0f, 0.04f, -0.10f), 0.04f, 0.7f, -0.08f, 0.02f, 0.18f, 0f, 1.04f)
            }
        };

        [SerializeField] private CueSequence summonEntry = new CueSequence
        {
            enabled = true,
            cueId = "summon_break_entry_cutin",
            priority = 60,
            canBeInterrupted = false,
            timeScale = 0.92f,
            timeScaleSeconds = 0.16f,
            shots = new[]
            {
                CreateShot(new Vector3(0f, 0.08f, -0.22f), 0.18f, 2.8f, -0.30f, 0.09f, 0.18f, 0.03f, 1.12f),
                CreateShot(new Vector3(-0.18f, 0.05f, 0.12f), 0.08f, -2.2f, 0.18f, 0.03f, 0.24f, 0.02f, 1.22f),
                CreateShot(new Vector3(0.16f, 0.09f, -0.24f), 0.16f, 3.0f, -0.32f, 0.10f, 0.28f, 0.04f, 1.18f),
                CreateShot(new Vector3(0f, 0.04f, -0.10f), 0.04f, 0.8f, -0.08f, 0.04f, 0.18f, 0f, 1.04f)
            }
        };

        [SerializeField] private CueSequence ultimateCutIn = new CueSequence
        {
            enabled = true,
            cueId = "skill1_lv3_ultimate_short_cutin",
            priority = 85,
            canBeInterrupted = false,
            timeScale = 0.84f,
            timeScaleSeconds = 0.24f,
            shots = new[]
            {
                CreateShot(new Vector3(-0.16f, 0.08f, -0.16f), 0.10f, 2.4f, -0.18f, 0.06f, 0.26f, 0.04f, 1.1f),
                CreateShot(new Vector3(0.10f, 0.03f, 0.22f), 0.08f, -3.2f, 0.28f, 0.03f, 0.32f, 0.03f, 1.28f),
                CreateShot(new Vector3(-0.06f, 0.08f, -0.24f), 0.20f, 3.6f, -0.36f, 0.10f, 0.30f, 0.04f, 1.24f),
                CreateShot(new Vector3(0f, 0.04f, -0.08f), 0.04f, 0.9f, -0.10f, 0.03f, 0.22f, 0f, 1.05f)
            }
        };

        [Header("Boss Pocket")]
        [SerializeField] private CueSequence bossPressureBreak = new CueSequence
        {
            enabled = true,
            cueId = "boss_pressure_break_reframe",
            priority = 65,
            canBeInterrupted = true,
            timeScale = 0.9f,
            timeScaleSeconds = 0.12f,
            shots = new[]
            {
                CreateShot(new Vector3(0.18f, 0.07f, -0.24f), 0.18f, 2.8f, -0.30f, 0.09f, 0.22f, 0.03f, 1.14f),
                CreateShot(new Vector3(-0.14f, 0.04f, 0.10f), 0.08f, -1.8f, 0.14f, 0.02f, 0.24f, 0.02f, 1.2f),
                CreateShot(new Vector3(0f, 0.05f, -0.14f), 0.08f, 1.4f, -0.16f, 0.04f, 0.22f, 0f, 1.08f)
            }
        };

        [SerializeField] private CueSequence summonFollowupHit = new CueSequence
        {
            enabled = true,
            cueId = "summon_followup_hit_confirm",
            priority = 72,
            canBeInterrupted = true,
            timeScale = 0.88f,
            timeScaleSeconds = 0.10f,
            shots = new[]
            {
                CreateShot(new Vector3(-0.08f, 0.05f, -0.10f), 0.08f, 1.6f, -0.14f, 0.04f, 0.14f, 0.02f, 1.08f),
                CreateShot(new Vector3(0.08f, 0.03f, 0.22f), 0.12f, -3.0f, 0.24f, 0.03f, 0.22f, 0.03f, 1.28f),
                CreateShot(new Vector3(0f, 0.04f, -0.08f), 0.04f, 0.8f, -0.08f, 0.02f, 0.18f, 0f, 1.04f)
            }
        };

        [SerializeField] private CueSequence pocketClear = new CueSequence
        {
            enabled = true,
            cueId = "pocket_result_clear_bridge",
            priority = 75,
            canBeInterrupted = false,
            timeScale = 0.86f,
            timeScaleSeconds = 0.14f,
            shots = new[]
            {
                CreateShot(new Vector3(0f, 0.08f, -0.28f), 0.20f, 3.2f, -0.34f, 0.10f, 0.26f, 0.04f, 1.12f),
                CreateShot(new Vector3(0.12f, 0.04f, 0.16f), 0.10f, -2.4f, 0.20f, 0.04f, 0.26f, 0.04f, 1.22f),
                CreateShot(new Vector3(0f, 0.05f, -0.12f), 0.04f, 0.9f, -0.10f, 0.04f, 0.22f, 0f, 1.04f)
            }
        };

        [SerializeField] private CueSequence pocketFail = new CueSequence
        {
            enabled = true,
            cueId = "pocket_result_fail_bridge",
            priority = 75,
            canBeInterrupted = false,
            timeScale = 0.92f,
            timeScaleSeconds = 0.10f,
            shots = new[]
            {
                CreateShot(new Vector3(0f, -0.04f, -0.18f), -0.06f, 1.8f, -0.20f, -0.04f, 0.24f, 0.03f, 1.05f),
                CreateShot(new Vector3(-0.08f, 0.02f, -0.10f), -0.04f, 0.6f, -0.08f, -0.03f, 0.22f, 0f, 1f)
            }
        };

        public CueSequence SkillCutIn => skillCutIn;
        public CueSequence SummonEntry => summonEntry;
        public CueSequence UltimateCutIn => ultimateCutIn;
        public CueSequence BossPressureBreak => bossPressureBreak;
        public CueSequence SummonFollowupHit => summonFollowupHit;
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
    }
}
