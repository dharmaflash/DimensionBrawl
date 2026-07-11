using System.Collections.Generic;
using DimensionBrawl.Player;
using UnityEngine;

namespace DimensionBrawl.Presentation
{
    [DisallowMultipleComponent]
    public sealed class MovementFootstepAudioScheduler : MonoBehaviour
    {
        private static MovementFootstepAudioScheduler instance;

        private readonly List<MovementFootstepAudioPresenter> presenters = new(8);

        public static int RegisteredPresenterCount => instance != null ? instance.presenters.Count : 0;
        public static bool IsTicking => instance != null && instance.enabled;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            instance = null;
        }

        public static void Register(MovementFootstepAudioPresenter presenter)
        {
            if (presenter == null)
            {
                return;
            }

            MovementFootstepAudioScheduler scheduler = EnsureInstance();
            if (!scheduler.presenters.Contains(presenter))
            {
                scheduler.presenters.Add(presenter);
            }

            scheduler.enabled = scheduler.presenters.Count > 0;
        }

        public static void Unregister(MovementFootstepAudioPresenter presenter)
        {
            if (instance == null || presenter == null)
            {
                return;
            }

            instance.presenters.Remove(presenter);
            instance.enabled = instance.presenters.Count > 0;
        }

        private static MovementFootstepAudioScheduler EnsureInstance()
        {
            if (instance != null)
            {
                return instance;
            }

            GameObject root = new("[MovementFootstepAudioScheduler]");
            DontDestroyOnLoad(root);
            instance = root.AddComponent<MovementFootstepAudioScheduler>();
            return instance;
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            int index = 0;
            while (index < presenters.Count)
            {
                MovementFootstepAudioPresenter presenter = presenters[index];
                if (presenter == null)
                {
                    presenters.RemoveAt(index);
                    continue;
                }

                if (presenter.isActiveAndEnabled)
                {
                    presenter.Tick(deltaTime);
                }

                index++;
            }

            if (presenters.Count == 0)
            {
                enabled = false;
            }
        }
    }

    [DisallowMultipleComponent]
    public sealed class MovementFootstepAudioPresenter : MonoBehaviour
    {
        [SerializeField] private AudioSource source;
        [SerializeField] private Transform trackedTransform;
        [SerializeField] private PlayerMovementController playerMovement;
        [SerializeField] private AudioClip[] clips = System.Array.Empty<AudioClip>();
        [SerializeField, Min(0f)] private float baseVolume = 0.34f;
        [SerializeField, Min(0f)] private float minimumSpeed = 0.55f;
        [SerializeField, Min(0.1f)] private float metersPerStep = 1.25f;
        [SerializeField, Min(0f)] private float minimumIntervalSeconds = 0.16f;
        [SerializeField, Min(0f)] private float minimumPitch = 0.96f;
        [SerializeField, Min(0f)] private float maximumPitch = 1.04f;
        [SerializeField, Min(0f)] private float minimumVolumeMultiplier = 0.88f;
        [SerializeField, Min(0f)] private float maximumVolumeMultiplier = 1.08f;
        [SerializeField, Min(0f)] private float playbackVolumeScale = 1f;

        private Vector3 lastPosition;
        private float accumulatedDistance;
        private float intervalTimer;
        private int lastClipIndex = -1;
        private bool hasLastPosition;

        public AudioSource Source => source;
        public Transform TrackedTransform => trackedTransform;
        public PlayerMovementController PlayerMovement => playerMovement;
        public int ClipCount => clips != null ? clips.Length : 0;
        public float BaseVolume => baseVolume;
        public float MinimumSpeed => minimumSpeed;
        public float MetersPerStep => metersPerStep;
        public float MinimumIntervalSeconds => minimumIntervalSeconds;
        public float MinimumPitch => minimumPitch;
        public float MaximumPitch => maximumPitch;
        public float MinimumVolumeMultiplier => minimumVolumeMultiplier;
        public float MaximumVolumeMultiplier => maximumVolumeMultiplier;
        public float PlaybackVolumeScale => playbackVolumeScale;

        private void Reset()
        {
            source = GetComponent<AudioSource>();
            trackedTransform = transform;
            playerMovement = GetComponent<PlayerMovementController>();
        }

        private void Awake()
        {
            if (trackedTransform == null)
            {
                trackedTransform = transform;
            }

            if (source == null)
            {
                source = GetComponent<AudioSource>();
            }
        }

        private void OnEnable()
        {
            ResetStepState();
            MovementFootstepAudioScheduler.Register(this);
        }

        private void OnDisable()
        {
            MovementFootstepAudioScheduler.Unregister(this);
        }

        public void Configure(
            AudioSource audioSource,
            Transform newTrackedTransform,
            PlayerMovementController newPlayerMovement,
            AudioClip[] audioClips,
            float configuredBaseVolume,
            float configuredMinimumSpeed,
            float configuredMetersPerStep,
            float configuredMinimumIntervalSeconds,
            float configuredMinimumPitch,
            float configuredMaximumPitch,
            float configuredMinimumVolumeMultiplier,
            float configuredMaximumVolumeMultiplier,
            float configuredPlaybackVolumeScale)
        {
            source = audioSource;
            trackedTransform = newTrackedTransform != null ? newTrackedTransform : transform;
            playerMovement = newPlayerMovement;
            clips = audioClips != null ? (AudioClip[])audioClips.Clone() : System.Array.Empty<AudioClip>();
            baseVolume = Mathf.Max(0f, configuredBaseVolume);
            minimumSpeed = Mathf.Max(0f, configuredMinimumSpeed);
            metersPerStep = Mathf.Max(0.1f, configuredMetersPerStep);
            minimumIntervalSeconds = Mathf.Max(0f, configuredMinimumIntervalSeconds);
            minimumPitch = Mathf.Max(0f, Mathf.Min(configuredMinimumPitch, configuredMaximumPitch));
            maximumPitch = Mathf.Max(minimumPitch, configuredMaximumPitch);
            minimumVolumeMultiplier = Mathf.Max(0f, Mathf.Min(configuredMinimumVolumeMultiplier, configuredMaximumVolumeMultiplier));
            maximumVolumeMultiplier = Mathf.Max(minimumVolumeMultiplier, configuredMaximumVolumeMultiplier);
            playbackVolumeScale = Mathf.Max(0f, configuredPlaybackVolumeScale);
            ResetStepState();
        }

        public AudioClip GetClip(int index)
        {
            return clips[index];
        }

        internal void Tick(float deltaTime)
        {
            if (source == null || trackedTransform == null || clips == null || clips.Length == 0)
            {
                return;
            }

            if (deltaTime <= 0f)
            {
                return;
            }

            Vector3 currentPosition = trackedTransform.position;
            if (!hasLastPosition)
            {
                lastPosition = currentPosition;
                hasLastPosition = true;
                return;
            }

            float speed = ResolvePlanarSpeed(currentPosition, deltaTime);
            if (speed < minimumSpeed)
            {
                accumulatedDistance = 0f;
                intervalTimer = Mathf.Max(0f, intervalTimer - deltaTime);
                lastPosition = currentPosition;
                return;
            }

            accumulatedDistance += speed * deltaTime;
            intervalTimer = Mathf.Max(0f, intervalTimer - deltaTime);
            if (accumulatedDistance >= metersPerStep && intervalTimer <= 0f)
            {
                PlayFootstep();
                accumulatedDistance = Mathf.Repeat(accumulatedDistance, metersPerStep);
                intervalTimer = minimumIntervalSeconds;
            }

            lastPosition = currentPosition;
        }

        private float ResolvePlanarSpeed(Vector3 currentPosition, float deltaTime)
        {
            if (playerMovement != null)
            {
                return Vector3.ProjectOnPlane(playerMovement.PlanarVelocity, Vector3.up).magnitude;
            }

            Vector3 delta = Vector3.ProjectOnPlane(currentPosition - lastPosition, Vector3.up);
            return delta.magnitude / deltaTime;
        }

        private void PlayFootstep()
        {
            int clipIndex = PickClipIndex();
            AudioClip clip = clips[clipIndex];
            if (clip == null)
            {
                return;
            }

            lastClipIndex = clipIndex;
            source.pitch = Random.Range(minimumPitch, maximumPitch);
            float volume = baseVolume
                * playbackVolumeScale
                * Random.Range(minimumVolumeMultiplier, maximumVolumeMultiplier);
            source.PlayOneShot(clip, volume);
        }

        private int PickClipIndex()
        {
            if (clips.Length <= 1)
            {
                return 0;
            }

            int clipIndex = Random.Range(0, clips.Length);
            if (clipIndex == lastClipIndex)
            {
                clipIndex = (clipIndex + 1) % clips.Length;
            }

            return clipIndex;
        }

        private void ResetStepState()
        {
            hasLastPosition = trackedTransform != null;
            lastPosition = trackedTransform != null ? trackedTransform.position : Vector3.zero;
            accumulatedDistance = 0f;
            intervalTimer = 0f;
            lastClipIndex = -1;
        }
    }
}
