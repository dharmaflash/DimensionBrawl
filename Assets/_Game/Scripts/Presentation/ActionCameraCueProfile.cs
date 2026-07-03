using System;
using UnityEngine;

namespace DimensionBrawl.Presentation
{
    [CreateAssetMenu(menuName = "DimensionBrawl/Profiles/Action Camera Cue Profile")]
    public sealed class ActionCameraCueProfile : ScriptableObject
    {
        [Serializable]
        public struct CameraCue
        {
            public bool enabled;
            public Vector3 localOffset;
            public float planarDirectionOffset;
            public float fieldOfViewDelta;
            public float cameraDistanceDelta;
            public float focusHeightDelta;
            public float durationSeconds;
            public float finisherScale;
        }

        [Header("Cue Profiles")]
        [SerializeField] private CameraCue runStartCue = new CameraCue
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

        [SerializeField] private CameraCue stopSettleCue = new CameraCue
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

        [SerializeField] private CameraCue sharpTurnCue = new CameraCue
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

        [SerializeField] private CameraCue dodgeCue = new CameraCue
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

        [SerializeField] private CameraCue perfectDodgeCue = new CameraCue
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

        [SerializeField] private CameraCue attackStartCue = new CameraCue
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

        [SerializeField] private CameraCue attackHitCue = new CameraCue
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

        [SerializeField] private CameraCue skill1Cue = new CameraCue
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

        [SerializeField] private CameraCue summonSlot1Cue = new CameraCue
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

        [SerializeField] private CameraCue summonPressureBlockCue = new CameraCue
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

        [SerializeField] private CameraCue summonBlockOpportunityCue = new CameraCue
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

        [SerializeField] private CameraCue summonFollowupWindowCue = new CameraCue
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

        [SerializeField] private CameraCue summonFollowupHitCue = new CameraCue
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

        [SerializeField] private CameraCue summonFollowupMissedCue = new CameraCue
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

        [SerializeField] private CameraCue counterWaveCue = new CameraCue
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

        [SerializeField] private CameraCue counterWaveStabilizedCue = new CameraCue
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

        [SerializeField] private CameraCue pocketClearCue = new CameraCue
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

        [SerializeField] private CameraCue pocketFailCue = new CameraCue
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

        public CameraCue RunStartCue => runStartCue;
        public CameraCue StopSettleCue => stopSettleCue;
        public CameraCue SharpTurnCue => sharpTurnCue;
        public CameraCue DodgeCue => dodgeCue;
        public CameraCue PerfectDodgeCue => perfectDodgeCue;
        public CameraCue AttackStartCue => attackStartCue;
        public CameraCue AttackHitCue => attackHitCue;
        public CameraCue Skill1Cue => skill1Cue;
        public CameraCue SummonSlot1Cue => summonSlot1Cue;
        public CameraCue SummonPressureBlockCue => summonPressureBlockCue;
        public CameraCue SummonBlockOpportunityCue => summonBlockOpportunityCue;
        public CameraCue SummonFollowupWindowCue => summonFollowupWindowCue;
        public CameraCue SummonFollowupHitCue => summonFollowupHitCue;
        public CameraCue SummonFollowupMissedCue => summonFollowupMissedCue;
        public CameraCue CounterWaveCue => counterWaveCue;
        public CameraCue CounterWaveStabilizedCue => counterWaveStabilizedCue;
        public CameraCue PocketClearCue => pocketClearCue;
        public CameraCue PocketFailCue => pocketFailCue;
    }
}
