using System;
using Unity.Cinemachine;
using UnityEngine;

namespace DimensionBrawl.Presentation
{
    [DisallowMultipleComponent]
    public sealed class IntroGatePodCinemachineShotPlayer : MonoBehaviour
    {
        [Serializable]
        public struct Shot
        {
            [SerializeField] private string shotId;
            [SerializeField, Min(0f)] private float startSeconds;
            [SerializeField] private CinemachineCamera camera;
            [SerializeField] private CinemachineBlendDefinition.Styles blendStyle;
            [SerializeField, Min(0f)] private float blendSeconds;

            public Shot(
                string shotId,
                float startSeconds,
                CinemachineCamera camera,
                CinemachineBlendDefinition.Styles blendStyle,
                float blendSeconds)
            {
                this.shotId = shotId;
                this.startSeconds = Mathf.Max(0f, startSeconds);
                this.camera = camera;
                this.blendStyle = blendStyle;
                this.blendSeconds = Mathf.Max(0f, blendSeconds);
            }

            public string ShotId => shotId;
            public float StartSeconds => Mathf.Max(0f, startSeconds);
            public CinemachineCamera Camera => camera;
            public CinemachineBlendDefinition.Styles BlendStyle => blendStyle;
            public float BlendSeconds => Mathf.Max(0f, blendSeconds);
        }

        [SerializeField] private CinemachineBrain brain;
        [SerializeField] private Shot[] shots = Array.Empty<Shot>();
        [SerializeField] private bool playOnStart = true;
        [SerializeField] private bool useUnscaledClock = true;
        [SerializeField, Min(0)] private int livePriority = 100;
        [SerializeField, Min(0)] private int standbyPriority;

        private float elapsedSeconds;
        private int activeShotIndex = -1;
        private bool playing;

        public CinemachineBrain Brain => brain;
        public Shot[] Shots => shots ?? Array.Empty<Shot>();
        public int ActiveShotIndex => activeShotIndex;
        public string ActiveShotId => activeShotIndex >= 0 && activeShotIndex < Shots.Length
            ? Shots[activeShotIndex].ShotId
            : string.Empty;
        public CinemachineCamera ActiveCamera => activeShotIndex >= 0 && activeShotIndex < Shots.Length
            ? Shots[activeShotIndex].Camera
            : null;

        public void Configure(CinemachineBrain newBrain, Shot[] newShots, bool newPlayOnStart, bool newUseUnscaledClock)
        {
            brain = newBrain;
            shots = newShots ?? Array.Empty<Shot>();
            playOnStart = newPlayOnStart;
            useUnscaledClock = newUseUnscaledClock;
            ApplySampleForReview(0f);
        }

        private void Awake()
        {
            if (brain == null)
            {
                brain = FindFirstObjectByType<CinemachineBrain>();
            }

            ApplySampleForReview(0f);
            playing = playOnStart;
            elapsedSeconds = 0f;
        }

        private void Update()
        {
            if (!playing || shots == null || shots.Length == 0)
            {
                return;
            }

            elapsedSeconds += useUnscaledClock ? Time.unscaledDeltaTime : Time.deltaTime;
            ActivateShot(ResolveShotIndex(elapsedSeconds), false);
        }

        public void Play()
        {
            elapsedSeconds = 0f;
            playing = true;
            ActivateShot(0, true);
        }

        public void Stop()
        {
            playing = false;
        }

        public void ApplySampleForReview(float sampleSeconds)
        {
            elapsedSeconds = Mathf.Max(0f, sampleSeconds);
            ActivateShot(ResolveShotIndex(elapsedSeconds), true);
        }

        private int ResolveShotIndex(float sampleSeconds)
        {
            Shot[] resolvedShots = Shots;
            if (resolvedShots.Length == 0)
            {
                return -1;
            }

            int index = 0;
            for (int i = 0; i < resolvedShots.Length; i++)
            {
                if (sampleSeconds >= resolvedShots[i].StartSeconds)
                {
                    index = i;
                }
            }

            return index;
        }

        private void ActivateShot(int shotIndex, bool forceCut)
        {
            Shot[] resolvedShots = Shots;
            if (shotIndex < 0 || shotIndex >= resolvedShots.Length)
            {
                return;
            }

            if (activeShotIndex == shotIndex && !forceCut)
            {
                return;
            }

            Shot shot = resolvedShots[shotIndex];
            if (brain != null)
            {
                CinemachineBlendDefinition.Styles style = forceCut
                    ? CinemachineBlendDefinition.Styles.Cut
                    : shot.BlendStyle;
                brain.DefaultBlend = new CinemachineBlendDefinition(style, shot.BlendSeconds);
            }

            for (int i = 0; i < resolvedShots.Length; i++)
            {
                CinemachineCamera camera = resolvedShots[i].Camera;
                if (camera == null)
                {
                    continue;
                }

                camera.gameObject.SetActive(true);
                camera.Priority = i == shotIndex ? livePriority : standbyPriority;
            }

            activeShotIndex = shotIndex;
        }
    }
}
