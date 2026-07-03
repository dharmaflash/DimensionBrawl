using DimensionBrawl.Combat;
using DimensionBrawl.Player;
using UnityEngine;

namespace DimensionBrawl.Presentation
{
    public sealed class ActionCameraCueDriver : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerActionController actionController;
        [SerializeField] private PlayerMovementController movement;
        [SerializeField] private PlayerSkill1Action skill1Action;
        [SerializeField] private PlayerSummonSlot1Action summonSlot1Action;
        [SerializeField] private ActionCameraController cameraController;
        [SerializeField] private ActionCinematicCueDirector cinematicCueDirector;
        [SerializeField] private Transform cueSpace;

        [Header("Profile")]
        [SerializeField] private ActionCameraCueProfile cueProfile;

        [Header("Cue Profiles")]
        [Tooltip("Run-start cue. Uses the low end of short action cue timing so movement start feels deliberate without camera lock.")]
        [SerializeField] private ActionCameraCueProfile.CameraCue runStartCue = new ActionCameraCueProfile.CameraCue
        {
            enabled = true,
            localOffset = new Vector3(0f, 0.02f, -0.10f),
            planarDirectionOffset = 0.08f,
            fieldOfViewDelta = 0.8f,
            cameraDistanceDelta = -0.08f,
            focusHeightDelta = 0.01f,
            durationSeconds = 0.20f,
            finisherScale = 1f
        };

        [Tooltip("Short stop-settle cue. Uses the 0.15-0.35s dodge/hit emphasis range conservatively.")]
        [SerializeField] private ActionCameraCueProfile.CameraCue stopSettleCue = new ActionCameraCueProfile.CameraCue
        {
            enabled = true,
            localOffset = new Vector3(0f, -0.02f, -0.06f),
            planarDirectionOffset = -0.02f,
            fieldOfViewDelta = -0.8f,
            cameraDistanceDelta = -0.12f,
            focusHeightDelta = -0.02f,
            durationSeconds = 0.22f,
            finisherScale = 1f
        };

        [Tooltip("Sharp movement turn cue. Keeps 90-degree direction changes readable without a full lock-on camera.")]
        [SerializeField] private ActionCameraCueProfile.CameraCue sharpTurnCue = new ActionCameraCueProfile.CameraCue
        {
            enabled = true,
            localOffset = new Vector3(0.08f, 0f, -0.10f),
            planarDirectionOffset = 0.06f,
            fieldOfViewDelta = 0.6f,
            cameraDistanceDelta = -0.06f,
            focusHeightDelta = 0f,
            durationSeconds = 0.24f,
            finisherScale = 1f
        };

        [Tooltip("Dodge read cue. Uses the collected short camera cue range around 0.20-0.32s.")]
        [SerializeField] private ActionCameraCueProfile.CameraCue dodgeCue = new ActionCameraCueProfile.CameraCue
        {
            enabled = true,
            localOffset = new Vector3(0f, 0.04f, -0.20f),
            planarDirectionOffset = -0.18f,
            fieldOfViewDelta = 2.2f,
            cameraDistanceDelta = -0.20f,
            focusHeightDelta = 0.03f,
            durationSeconds = 0.28f,
            finisherScale = 1f
        };

        [Tooltip("Perfect-dodge domain cue. Wider and slightly pulled back so the slowed threat field reads without stealing control.")]
        [SerializeField] private ActionCameraCueProfile.CameraCue perfectDodgeCue = new ActionCameraCueProfile.CameraCue
        {
            enabled = true,
            localOffset = new Vector3(0f, 0.12f, -0.32f),
            planarDirectionOffset = -0.30f,
            fieldOfViewDelta = 3.6f,
            cameraDistanceDelta = -0.34f,
            focusHeightDelta = 0.08f,
            durationSeconds = 0.34f,
            finisherScale = 1f
        };

        [Tooltip("Basic attack entry cue. Small by default so normal attacks do not become cinematic locks.")]
        [SerializeField] private ActionCameraCueProfile.CameraCue attackStartCue = new ActionCameraCueProfile.CameraCue
        {
            enabled = true,
            localOffset = new Vector3(0f, -0.03f, 0.14f),
            planarDirectionOffset = 0.08f,
            fieldOfViewDelta = -1.2f,
            cameraDistanceDelta = 0.12f,
            focusHeightDelta = -0.02f,
            durationSeconds = 0.22f,
            finisherScale = 1.2f
        };

        [Tooltip("Successful hit cue. Kept shorter than attack state emphasis; hit-stop already carries impact.")]
        [SerializeField] private ActionCameraCueProfile.CameraCue attackHitCue = new ActionCameraCueProfile.CameraCue
        {
            enabled = true,
            localOffset = new Vector3(0f, 0.03f, 0.12f),
            planarDirectionOffset = 0.06f,
            fieldOfViewDelta = -1.8f,
            cameraDistanceDelta = 0.16f,
            focusHeightDelta = 0.01f,
            durationSeconds = 0.18f,
            finisherScale = 1.3f
        };

        [Tooltip("Immediate Skill1 cue. Stays small so the player still reads incoming boss fire.")]
        [SerializeField] private ActionCameraCueProfile.CameraCue skill1Cue = new ActionCameraCueProfile.CameraCue
        {
            enabled = true,
            localOffset = new Vector3(0f, 0.02f, 0.12f),
            planarDirectionOffset = 0.10f,
            fieldOfViewDelta = -1.2f,
            cameraDistanceDelta = 0.10f,
            focusHeightDelta = 0.01f,
            durationSeconds = 0.24f,
            finisherScale = 1.2f
        };

        [Tooltip("SummonSlot1 entry cue. Uses a short pullback/widen so the proxy and pressure screen read as the main exchange.")]
        [SerializeField] private ActionCameraCueProfile.CameraCue summonSlot1Cue = new ActionCameraCueProfile.CameraCue
        {
            enabled = true,
            localOffset = new Vector3(0f, 0.08f, -0.18f),
            planarDirectionOffset = 0.16f,
            fieldOfViewDelta = 2.4f,
            cameraDistanceDelta = -0.26f,
            focusHeightDelta = 0.08f,
            durationSeconds = 0.34f,
            finisherScale = 1.35f
        };

        [Tooltip("Summon pressure-screen block cue. Short additive read for boss fire being absorbed by the summon side.")]
        [SerializeField] private ActionCameraCueProfile.CameraCue summonPressureBlockCue = new ActionCameraCueProfile.CameraCue
        {
            enabled = true,
            localOffset = new Vector3(0f, 0.06f, -0.10f),
            planarDirectionOffset = 0.06f,
            fieldOfViewDelta = 1.4f,
            cameraDistanceDelta = -0.14f,
            focusHeightDelta = 0.04f,
            durationSeconds = 0.18f,
            finisherScale = 1.25f
        };

        [Tooltip("Close-threat defeat cue. Briefly widens before the player answers boss pressure with a summon block.")]
        [SerializeField] private ActionCameraCueProfile.CameraCue summonBlockOpportunityCue = new ActionCameraCueProfile.CameraCue
        {
            enabled = true,
            localOffset = new Vector3(0f, 0.07f, -0.16f),
            planarDirectionOffset = 0.04f,
            fieldOfViewDelta = 1.2f,
            cameraDistanceDelta = -0.18f,
            focusHeightDelta = 0.05f,
            durationSeconds = 0.22f,
            finisherScale = 1.1f
        };

        [Tooltip("Readable opening after a correct summon pressure block. Briefly widens so the Skill1 follow-up choice reads.")]
        [SerializeField] private ActionCameraCueProfile.CameraCue summonFollowupWindowCue = new ActionCameraCueProfile.CameraCue
        {
            enabled = true,
            localOffset = new Vector3(0f, 0.05f, -0.14f),
            planarDirectionOffset = 0.04f,
            fieldOfViewDelta = 1.6f,
            cameraDistanceDelta = -0.16f,
            focusHeightDelta = 0.03f,
            durationSeconds = 0.22f,
            finisherScale = 1.2f
        };

        [Tooltip("Confirmed follow-up Skill1 boss hit. Short punch-in, not a global slow-motion effect.")]
        [SerializeField] private ActionCameraCueProfile.CameraCue summonFollowupHitCue = new ActionCameraCueProfile.CameraCue
        {
            enabled = true,
            localOffset = new Vector3(0f, 0.04f, 0.16f),
            planarDirectionOffset = 0.08f,
            fieldOfViewDelta = -2.4f,
            cameraDistanceDelta = 0.18f,
            focusHeightDelta = 0.02f,
            durationSeconds = 0.20f,
            finisherScale = 1.3f
        };

        [Tooltip("Missed follow-up window. Small release cue as boss pressure returns.")]
        [SerializeField] private ActionCameraCueProfile.CameraCue summonFollowupMissedCue = new ActionCameraCueProfile.CameraCue
        {
            enabled = true,
            localOffset = new Vector3(0f, -0.02f, -0.08f),
            planarDirectionOffset = -0.02f,
            fieldOfViewDelta = 0.8f,
            cameraDistanceDelta = -0.08f,
            focusHeightDelta = -0.02f,
            durationSeconds = 0.18f,
            finisherScale = 1f
        };

        [Tooltip("Counter wave pressure cue. A short widen/pullback so the player reads the lane has become contested again.")]
        [SerializeField] private ActionCameraCueProfile.CameraCue counterWaveCue = new ActionCameraCueProfile.CameraCue
        {
            enabled = true,
            localOffset = new Vector3(0f, 0.06f, -0.22f),
            planarDirectionOffset = -0.12f,
            fieldOfViewDelta = 2.0f,
            cameraDistanceDelta = -0.22f,
            focusHeightDelta = 0.05f,
            durationSeconds = 0.20f,
            finisherScale = 1.2f
        };

        [Tooltip("Counter recovery cue. Smaller than the follow-up cue so it marks the line hold without stealing the Skill1 confirm.")]
        [SerializeField] private ActionCameraCueProfile.CameraCue counterWaveStabilizedCue = new ActionCameraCueProfile.CameraCue
        {
            enabled = true,
            localOffset = new Vector3(0f, 0.04f, -0.10f),
            planarDirectionOffset = 0.08f,
            fieldOfViewDelta = 1.0f,
            cameraDistanceDelta = -0.10f,
            focusHeightDelta = 0.03f,
            durationSeconds = 0.18f,
            finisherScale = 1.1f
        };

        [Tooltip("Pocket clear result cue. A short stable widen so the completed response loop reads after the follow-up hit.")]
        [SerializeField] private ActionCameraCueProfile.CameraCue pocketClearCue = new ActionCameraCueProfile.CameraCue
        {
            enabled = true,
            localOffset = new Vector3(0f, 0.06f, -0.18f),
            planarDirectionOffset = 0.04f,
            fieldOfViewDelta = 1.4f,
            cameraDistanceDelta = -0.18f,
            focusHeightDelta = 0.05f,
            durationSeconds = 0.32f,
            finisherScale = 1.15f
        };

        [Tooltip("Pocket fail result cue. Pulls back slightly as boss pressure returns, without becoming a cinematic lock.")]
        [SerializeField] private ActionCameraCueProfile.CameraCue pocketFailCue = new ActionCameraCueProfile.CameraCue
        {
            enabled = true,
            localOffset = new Vector3(0f, -0.04f, -0.12f),
            planarDirectionOffset = -0.06f,
            fieldOfViewDelta = 1.6f,
            cameraDistanceDelta = -0.18f,
            focusHeightDelta = -0.02f,
            durationSeconds = 0.34f,
            finisherScale = 1.05f
        };

        private int summonPressureBlockCueRequestCount;
        private int lastSummonPressureBlockTier;
        private int summonBlockOpportunityCueRequestCount;
        private int summonFollowupWindowCueRequestCount;
        private int summonFollowupHitCueRequestCount;
        private int summonFollowupMissedCueRequestCount;
        private int bossScreenSuppressCueRequestCount;
        private int counterWaveCueRequestCount;
        private int counterWaveStabilizedCueRequestCount;
        private int pocketClearCueRequestCount;
        private int pocketFailCueRequestCount;
        private int lastSummonFollowupWindowTier;
        private int lastSummonFollowupHitTier;
        private int lastBossScreenSuppressTier;
        private int lastCounterWaveTier;
        private int lastCounterWaveStabilizedTier;
        private int lastPocketClearTier;
        private int lastPocketFailTier;
        private float lastSummonFollowupHitDamage;

        public ActionCameraCueProfile CueProfile => cueProfile;
        public ActionCinematicCueDirector CinematicCueDirector => cinematicCueDirector;
        public int SummonPressureBlockCueRequestCount => summonPressureBlockCueRequestCount;
        public int LastSummonPressureBlockTier => lastSummonPressureBlockTier;
        public int SummonBlockOpportunityCueRequestCount => summonBlockOpportunityCueRequestCount;
        public int SummonFollowupWindowCueRequestCount => summonFollowupWindowCueRequestCount;
        public int SummonFollowupHitCueRequestCount => summonFollowupHitCueRequestCount;
        public int SummonFollowupMissedCueRequestCount => summonFollowupMissedCueRequestCount;
        public int BossScreenSuppressCueRequestCount => bossScreenSuppressCueRequestCount;
        public int CounterWaveCueRequestCount => counterWaveCueRequestCount;
        public int CounterWaveStabilizedCueRequestCount => counterWaveStabilizedCueRequestCount;
        public int PocketClearCueRequestCount => pocketClearCueRequestCount;
        public int PocketFailCueRequestCount => pocketFailCueRequestCount;
        public int LastSummonFollowupWindowTier => lastSummonFollowupWindowTier;
        public int LastSummonFollowupHitTier => lastSummonFollowupHitTier;
        public int LastBossScreenSuppressTier => lastBossScreenSuppressTier;
        public int LastCounterWaveTier => lastCounterWaveTier;
        public int LastCounterWaveStabilizedTier => lastCounterWaveStabilizedTier;
        public int LastPocketClearTier => lastPocketClearTier;
        public int LastPocketFailTier => lastPocketFailTier;
        public float LastSummonFollowupHitDamage => lastSummonFollowupHitDamage;

        private ActionCameraCueProfile.CameraCue ActiveRunStartCue => cueProfile != null ? cueProfile.RunStartCue : runStartCue;
        private ActionCameraCueProfile.CameraCue ActiveStopSettleCue => cueProfile != null ? cueProfile.StopSettleCue : stopSettleCue;
        private ActionCameraCueProfile.CameraCue ActiveSharpTurnCue => cueProfile != null ? cueProfile.SharpTurnCue : sharpTurnCue;
        private ActionCameraCueProfile.CameraCue ActiveDodgeCue => cueProfile != null ? cueProfile.DodgeCue : dodgeCue;
        private ActionCameraCueProfile.CameraCue ActivePerfectDodgeCue =>
            cueProfile != null ? cueProfile.PerfectDodgeCue : perfectDodgeCue;
        private ActionCameraCueProfile.CameraCue ActiveAttackStartCue => cueProfile != null ? cueProfile.AttackStartCue : attackStartCue;
        private ActionCameraCueProfile.CameraCue ActiveAttackHitCue => cueProfile != null ? cueProfile.AttackHitCue : attackHitCue;
        private ActionCameraCueProfile.CameraCue ActiveSkill1Cue => cueProfile != null ? cueProfile.Skill1Cue : skill1Cue;
        private ActionCameraCueProfile.CameraCue ActiveSummonSlot1Cue => cueProfile != null ? cueProfile.SummonSlot1Cue : summonSlot1Cue;
        private ActionCameraCueProfile.CameraCue ActiveSummonPressureBlockCue =>
            cueProfile != null ? cueProfile.SummonPressureBlockCue : summonPressureBlockCue;
        private ActionCameraCueProfile.CameraCue ActiveSummonBlockOpportunityCue =>
            cueProfile != null ? cueProfile.SummonBlockOpportunityCue : summonBlockOpportunityCue;
        private ActionCameraCueProfile.CameraCue ActiveSummonFollowupWindowCue =>
            cueProfile != null ? cueProfile.SummonFollowupWindowCue : summonFollowupWindowCue;
        private ActionCameraCueProfile.CameraCue ActiveSummonFollowupHitCue =>
            cueProfile != null ? cueProfile.SummonFollowupHitCue : summonFollowupHitCue;
        private ActionCameraCueProfile.CameraCue ActiveSummonFollowupMissedCue =>
            cueProfile != null ? cueProfile.SummonFollowupMissedCue : summonFollowupMissedCue;
        private ActionCameraCueProfile.CameraCue ActiveCounterWaveCue =>
            cueProfile != null ? cueProfile.CounterWaveCue : counterWaveCue;
        private ActionCameraCueProfile.CameraCue ActiveCounterWaveStabilizedCue =>
            cueProfile != null ? cueProfile.CounterWaveStabilizedCue : counterWaveStabilizedCue;
        private ActionCameraCueProfile.CameraCue ActivePocketClearCue =>
            cueProfile != null ? cueProfile.PocketClearCue : pocketClearCue;
        private ActionCameraCueProfile.CameraCue ActivePocketFailCue =>
            cueProfile != null ? cueProfile.PocketFailCue : pocketFailCue;

        private void Awake()
        {
            if (cameraController == null)
            {
                cameraController = GetComponent<ActionCameraController>();
            }

            if (cinematicCueDirector == null)
            {
                cinematicCueDirector = GetComponent<ActionCinematicCueDirector>();
            }
        }

        private void OnEnable()
        {
            if (movement != null)
            {
                movement.RunStarted += HandleRunStarted;
                movement.StopSettleStarted += HandleStopSettleStarted;
                movement.SharpTurnStarted += HandleSharpTurnStarted;
            }

            if (actionController != null)
            {
                actionController.DodgeStarted += HandleDodgeStarted;
                actionController.PerfectDodgeTriggered += HandlePerfectDodgeTriggered;
                actionController.BasicAttackStarted += HandleBasicAttackStarted;
                actionController.BasicAttackHit += HandleBasicAttackHit;
            }

            if (skill1Action != null)
            {
                skill1Action.Skill1Used += HandleSkill1Used;
            }

            if (summonSlot1Action != null)
            {
                summonSlot1Action.SummonSlot1Used += HandleSummonSlot1Used;
                summonSlot1Action.SummonPressureBlocked += HandleSummonPressureBlocked;
            }
        }

        private void OnDisable()
        {
            if (movement != null)
            {
                movement.RunStarted -= HandleRunStarted;
                movement.StopSettleStarted -= HandleStopSettleStarted;
                movement.SharpTurnStarted -= HandleSharpTurnStarted;
            }

            if (actionController != null)
            {
                actionController.DodgeStarted -= HandleDodgeStarted;
                actionController.PerfectDodgeTriggered -= HandlePerfectDodgeTriggered;
                actionController.BasicAttackStarted -= HandleBasicAttackStarted;
                actionController.BasicAttackHit -= HandleBasicAttackHit;
            }

            if (skill1Action != null)
            {
                skill1Action.Skill1Used -= HandleSkill1Used;
            }

            if (summonSlot1Action != null)
            {
                summonSlot1Action.SummonSlot1Used -= HandleSummonSlot1Used;
                summonSlot1Action.SummonPressureBlocked -= HandleSummonPressureBlocked;
            }
        }

        private void HandleRunStarted()
        {
            RequestCue(ActiveRunStartCue, ResolvePlanarDirection(), 1f);
        }

        private void HandleStopSettleStarted()
        {
            RequestCue(ActiveStopSettleCue, -ResolvePlanarDirection(), 1f);
        }

        private void HandleSharpTurnStarted(float signedAngle)
        {
            float side = signedAngle < 0f ? -1f : 1f;
            Vector3 turnDirection = Quaternion.AngleAxis(35f * side, Vector3.up) * ResolvePlanarDirection();
            RequestCue(ActiveSharpTurnCue, turnDirection, 1f);
        }

        private void HandleDodgeStarted()
        {
            Vector3 dodgeDirection = actionController != null ? actionController.LastDodgeDirection : ResolvePlanarDirection();
            RequestCue(ActiveDodgeCue, dodgeDirection, 1f);
        }

        private void HandlePerfectDodgeTriggered(DamageInfo damageInfo)
        {
            Vector3 dodgeDirection = actionController != null ? actionController.LastDodgeDirection : ResolvePlanarDirection();
            RequestCue(ActivePerfectDodgeCue, dodgeDirection, 1f);
        }

        private void HandleBasicAttackStarted(int comboIndex)
        {
            ActionCameraCueProfile.CameraCue cue = ActiveAttackStartCue;
            RequestCue(cue, ResolvePlanarDirection(), ResolveComboScale(comboIndex, cue));
        }

        private void HandleBasicAttackHit(int comboIndex)
        {
            ActionCameraCueProfile.CameraCue cue = ActiveAttackHitCue;
            RequestCue(cue, ResolvePlanarDirection(), ResolveComboScale(comboIndex, cue));
        }

        private void HandleSkill1Used(int tier)
        {
            ActionCameraCueProfile.CameraCue cue = ActiveSkill1Cue;
            Vector3 direction = ResolvePlanarDirection();
            RequestCue(cue, direction, ResolveTierScale(tier, cue));
            RequestCinematic(
                tier >= 3
                    ? ActionCinematicCueProfile.CueKind.UltimateCutIn
                    : ActionCinematicCueProfile.CueKind.SkillCutIn,
                tier,
                direction);
        }

        private void HandleSummonSlot1Used(int tier)
        {
            ActionCameraCueProfile.CameraCue cue = ActiveSummonSlot1Cue;
            Vector3 direction = ResolvePlanarDirection();
            RequestCue(cue, direction, ResolveTierScale(tier, cue));
            RequestCinematic(ActionCinematicCueProfile.CueKind.SummonEntry, tier, direction);
        }

        private void HandleSummonPressureBlocked(int tier)
        {
            RequestSummonPressureBlockCue(tier);
        }

        public void RequestSummonPressureBlockCue(int tier)
        {
            ActionCameraCueProfile.CameraCue cue = ActiveSummonPressureBlockCue;
            if (RequestCue(cue, ResolvePlanarDirection(), ResolveTierScale(tier, cue)))
            {
                summonPressureBlockCueRequestCount++;
                lastSummonPressureBlockTier = tier;
            }
        }

        public void RequestSummonBlockOpportunityCue()
        {
            if (RequestCue(ActiveSummonBlockOpportunityCue, ResolvePlanarDirection(), 1f))
            {
                summonBlockOpportunityCueRequestCount++;
            }
        }

        public void RequestSummonFollowupWindowCue(int tier)
        {
            ActionCameraCueProfile.CameraCue cue = ActiveSummonFollowupWindowCue;
            if (RequestCue(cue, ResolvePlanarDirection(), ResolveTierScale(tier, cue)))
            {
                summonFollowupWindowCueRequestCount++;
                lastSummonFollowupWindowTier = tier;
            }
        }

        public void RequestSummonFollowupHitCue(int tier, float damage)
        {
            ActionCameraCueProfile.CameraCue cue = ActiveSummonFollowupHitCue;
            if (RequestCue(cue, ResolvePlanarDirection(), ResolveTierScale(tier, cue)))
            {
                summonFollowupHitCueRequestCount++;
                lastSummonFollowupHitTier = tier;
                lastSummonFollowupHitDamage = damage;
            }
        }

        public void RequestSummonFollowupMissedCue()
        {
            if (RequestCue(ActiveSummonFollowupMissedCue, -ResolvePlanarDirection(), 1f))
            {
                summonFollowupMissedCueRequestCount++;
            }
        }

        public void RequestBossScreenSuppressCue(int tier)
        {
            int resolvedTier = Mathf.Clamp(tier, 1, 3);
            ActionCameraCueProfile.CameraCue cue = ActiveSummonFollowupWindowCue;
            if (RequestCue(cue, ResolvePlanarDirection(), ResolveTierScale(resolvedTier, cue)))
            {
                bossScreenSuppressCueRequestCount++;
                lastBossScreenSuppressTier = resolvedTier;
            }
        }

        public void RequestCounterWaveCue(int tier)
        {
            int resolvedTier = Mathf.Clamp(tier, 1, 3);
            ActionCameraCueProfile.CameraCue cue = ActiveCounterWaveCue;
            if (RequestCue(cue, -ResolvePlanarDirection(), ResolveTierScale(resolvedTier, cue)))
            {
                counterWaveCueRequestCount++;
                lastCounterWaveTier = resolvedTier;
            }
        }

        public void RequestCounterWaveStabilizedCue(int tier)
        {
            int resolvedTier = Mathf.Clamp(tier, 1, 3);
            ActionCameraCueProfile.CameraCue cue = ActiveCounterWaveStabilizedCue;
            if (RequestCue(cue, ResolvePlanarDirection(), ResolveTierScale(resolvedTier, cue)))
            {
                counterWaveStabilizedCueRequestCount++;
                lastCounterWaveStabilizedTier = resolvedTier;
            }
        }

        public void RequestPocketClearCue(int tier)
        {
            int resolvedTier = Mathf.Clamp(tier, 1, 3);
            ActionCameraCueProfile.CameraCue cue = ActivePocketClearCue;
            if (RequestCue(cue, ResolvePlanarDirection(), ResolveTierScale(resolvedTier, cue)))
            {
                pocketClearCueRequestCount++;
                lastPocketClearTier = resolvedTier;
            }
        }

        public void RequestPocketFailCue(int tier)
        {
            int resolvedTier = Mathf.Clamp(tier, 1, 3);
            ActionCameraCueProfile.CameraCue cue = ActivePocketFailCue;
            if (RequestCue(cue, -ResolvePlanarDirection(), ResolveTierScale(resolvedTier, cue)))
            {
                pocketFailCueRequestCount++;
                lastPocketFailTier = resolvedTier;
            }
        }

        private bool RequestCue(ActionCameraCueProfile.CameraCue cue, Vector3 planarDirection, float scale)
        {
            if (!cue.enabled || cameraController == null)
            {
                return false;
            }

            Transform space = cueSpace != null ? cueSpace : (movement != null ? movement.transform : transform);
            Vector3 offset = space.TransformDirection(cue.localOffset);
            Vector3 direction = Vector3.ProjectOnPlane(planarDirection, Vector3.up);
            if (direction.sqrMagnitude > 0.0001f)
            {
                offset += direction.normalized * cue.planarDirectionOffset;
            }

            float clampedScale = Mathf.Max(0f, scale);
            cameraController.RequestCue(
                offset * clampedScale,
                cue.durationSeconds,
                cue.fieldOfViewDelta * clampedScale,
                cue.cameraDistanceDelta * clampedScale,
                cue.focusHeightDelta * clampedScale);
            return true;
        }

        private bool RequestCinematic(ActionCinematicCueProfile.CueKind kind, int tier, Vector3 planarDirection)
        {
            return cinematicCueDirector != null && cinematicCueDirector.TryPlay(kind, tier, planarDirection);
        }

        private Vector3 ResolvePlanarDirection()
        {
            if (movement == null)
            {
                return transform.forward;
            }

            Vector3 intent = movement.MoveIntentDirection;
            if (intent.sqrMagnitude > 0.0001f)
            {
                return intent.normalized;
            }

            Vector3 velocity = Vector3.ProjectOnPlane(movement.PlanarVelocity, Vector3.up);
            if (velocity.sqrMagnitude > 0.0001f)
            {
                return velocity.normalized;
            }

            return movement.FacingDirection;
        }

        private static float ResolveComboScale(int comboIndex, ActionCameraCueProfile.CameraCue cue)
        {
            if (comboIndex <= 0)
            {
                return 1f;
            }

            float comboWeight = Mathf.Clamp01(comboIndex / 4f);
            return Mathf.Lerp(1f, cue.finisherScale, comboWeight);
        }

        private static float ResolveTierScale(int tier, ActionCameraCueProfile.CameraCue cue)
        {
            float tierWeight = Mathf.Clamp01((Mathf.Max(1, tier) - 1) / 2f);
            return Mathf.Lerp(1f, cue.finisherScale, tierWeight);
        }
    }
}
