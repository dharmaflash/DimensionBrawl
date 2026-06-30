using UnityEngine;

namespace DimensionBrawl.Presentation
{
    [DisallowMultipleComponent]
    public sealed class CombatVfxCueAudioRandomizer : MonoBehaviour
    {
        [SerializeField] private AudioSource source;
        [SerializeField] private AudioClip[] clips = System.Array.Empty<AudioClip>();
        [SerializeField, Min(0f)] private float baseVolume = 0.72f;
        [SerializeField, Min(0f)] private float minimumPitch = 1.02f;
        [SerializeField, Min(0f)] private float maximumPitch = 1.08f;
        [SerializeField, Min(0f)] private float minimumVolumeMultiplier = 0.94f;
        [SerializeField, Min(0f)] private float maximumVolumeMultiplier = 1.04f;

        private int lastClipIndex = -1;

        public AudioSource Source => source;
        public int ClipCount => clips != null ? clips.Length : 0;
        public float BaseVolume => baseVolume;
        public float MinimumPitch => minimumPitch;
        public float MaximumPitch => maximumPitch;
        public float MinimumVolumeMultiplier => minimumVolumeMultiplier;
        public float MaximumVolumeMultiplier => maximumVolumeMultiplier;
        public float LastPlayedClipDurationSeconds { get; private set; }

        private void Reset()
        {
            source = GetComponent<AudioSource>();
        }

        private void Awake()
        {
            if (source == null)
            {
                source = GetComponent<AudioSource>();
            }
        }

        public void Configure(
            AudioSource audioSource,
            AudioClip[] audioClips,
            float configuredBaseVolume,
            float configuredMinimumPitch,
            float configuredMaximumPitch,
            float configuredMinimumVolumeMultiplier,
            float configuredMaximumVolumeMultiplier)
        {
            source = audioSource;
            clips = audioClips != null ? (AudioClip[])audioClips.Clone() : System.Array.Empty<AudioClip>();
            baseVolume = Mathf.Max(0f, configuredBaseVolume);
            minimumPitch = Mathf.Max(0f, Mathf.Min(configuredMinimumPitch, configuredMaximumPitch));
            maximumPitch = Mathf.Max(minimumPitch, configuredMaximumPitch);
            minimumVolumeMultiplier = Mathf.Max(0f, Mathf.Min(configuredMinimumVolumeMultiplier, configuredMaximumVolumeMultiplier));
            maximumVolumeMultiplier = Mathf.Max(minimumVolumeMultiplier, configuredMaximumVolumeMultiplier);
        }

        public AudioClip GetClip(int index)
        {
            return clips[index];
        }

        public bool Play(float volumeScale = 1f)
        {
            if (source == null || !source.enabled || !source.gameObject.activeInHierarchy || clips == null || clips.Length == 0)
            {
                LastPlayedClipDurationSeconds = 0f;
                return false;
            }

            int clipIndex = PickClipIndex();
            AudioClip clip = clips[clipIndex];
            if (clip == null)
            {
                LastPlayedClipDurationSeconds = 0f;
                return false;
            }

            lastClipIndex = clipIndex;
            source.Stop();
            source.clip = clip;
            source.volume = baseVolume
                * Mathf.Max(0f, volumeScale)
                * Random.Range(minimumVolumeMultiplier, maximumVolumeMultiplier);
            source.pitch = Random.Range(minimumPitch, maximumPitch);
            LastPlayedClipDurationSeconds = source.pitch > 0.001f ? clip.length / source.pitch : clip.length;
            source.Play();
            return true;
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
    }
}
