using System;
using DimensionBrawl.Player;
using UnityEngine;

namespace DimensionBrawl.Presentation
{
    [DisallowMultipleComponent]
    public sealed class PlayerRangedReloadSfxDriver : MonoBehaviour
    {
        [SerializeField] private PlayerRangedBasicAttackAction rangedBasicAttackAction;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip[] reloadClips = Array.Empty<AudioClip>();
        [SerializeField, Min(0f)] private float baseVolume = 0.62f;
        [SerializeField, Min(0f)] private float minimumPitch = 0.97f;
        [SerializeField, Min(0f)] private float maximumPitch = 1.03f;
        [SerializeField, Range(0f, 1f)] private float spatialBlend = 0f;

        private bool subscribed;

        public int ReloadClipCount => reloadClips != null ? reloadClips.Length : 0;

        private void Awake()
        {
            if (rangedBasicAttackAction == null)
            {
                rangedBasicAttackAction = GetComponent<PlayerRangedBasicAttackAction>();
            }

            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        public void Configure(
            PlayerRangedBasicAttackAction newRangedBasicAttackAction,
            AudioSource newAudioSource,
            AudioClip[] newReloadClips)
        {
            Unsubscribe();
            rangedBasicAttackAction = newRangedBasicAttackAction;
            audioSource = newAudioSource;
            reloadClips = newReloadClips ?? Array.Empty<AudioClip>();

            if (isActiveAndEnabled)
            {
                Subscribe();
            }
        }

        private void Subscribe()
        {
            if (subscribed || rangedBasicAttackAction == null)
            {
                return;
            }

            rangedBasicAttackAction.RangedReloadStarted += HandleReloadStarted;
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed || rangedBasicAttackAction == null)
            {
                subscribed = false;
                return;
            }

            rangedBasicAttackAction.RangedReloadStarted -= HandleReloadStarted;
            subscribed = false;
        }

        private void HandleReloadStarted()
        {
            if (audioSource == null || reloadClips == null || reloadClips.Length == 0)
            {
                return;
            }

            AudioClip clip = reloadClips[UnityEngine.Random.Range(0, reloadClips.Length)];
            if (clip == null)
            {
                return;
            }

            audioSource.spatialBlend = spatialBlend;
            audioSource.pitch = ResolvePitch();
            audioSource.PlayOneShot(clip, baseVolume);
        }

        private float ResolvePitch()
        {
            float min = minimumPitch > 0f ? minimumPitch : 1f;
            float max = maximumPitch > 0f ? Mathf.Max(min, maximumPitch) : min;
            return UnityEngine.Random.Range(min, max);
        }
    }
}
