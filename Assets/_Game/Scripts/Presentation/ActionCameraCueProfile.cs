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

        public CameraCue RunStartCue => runStartCue;
        public CameraCue StopSettleCue => stopSettleCue;
        public CameraCue SharpTurnCue => sharpTurnCue;
        public CameraCue DodgeCue => dodgeCue;
        public CameraCue AttackStartCue => attackStartCue;
        public CameraCue AttackHitCue => attackHitCue;
    }
}
