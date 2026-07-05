using System;
using UnityEngine;
using UnityEngine.Audio;

namespace DimensionBrawl.Presentation
{
    [CreateAssetMenu(
        fileName = "SceneBgmSettings",
        menuName = "Dimension Brawl/Presentation/Scene BGM Settings")]
    public sealed class SceneBgmSettings : ScriptableObject
    {
        [SerializeField] private SceneBgmEntry[] entries = Array.Empty<SceneBgmEntry>();
        [SerializeField] private bool stopWhenSceneIsUnlisted = true;

        public bool StopWhenSceneIsUnlisted => stopWhenSceneIsUnlisted;

        public bool TryGetEntry(string sceneName, out SceneBgmEntry entry)
        {
            if (!string.IsNullOrEmpty(sceneName))
            {
                for (int i = 0; i < entries.Length; i++)
                {
                    SceneBgmEntry candidate = entries[i];
                    if (candidate != null && string.Equals(candidate.SceneName, sceneName, StringComparison.Ordinal))
                    {
                        entry = candidate;
                        return true;
                    }
                }
            }

            entry = null;
            return false;
        }
    }

    [Serializable]
    public sealed class SceneBgmEntry
    {
        [SerializeField] private string sceneName;
        [SerializeField] private AudioClip bgmClip;
        [SerializeField, Range(0f, 1f)] private float volume = 0.55f;
        [SerializeField] private bool restartWhenSceneLoads;
        [SerializeField] private AudioMixerGroup outputMixerGroup;

        public string SceneName => sceneName;
        public AudioClip BgmClip => bgmClip;
        public float Volume => volume;
        public bool RestartWhenSceneLoads => restartWhenSceneLoads;
        public AudioMixerGroup OutputMixerGroup => outputMixerGroup;
    }
}
