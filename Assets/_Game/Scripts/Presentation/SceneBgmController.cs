using UnityEngine;
using UnityEngine.SceneManagement;

namespace DimensionBrawl.Presentation
{
    [DefaultExecutionOrder(-2000)]
    [DisallowMultipleComponent]
    public sealed class SceneBgmController : MonoBehaviour
    {
        private const string SettingsResourcePath = "SceneBgmSettings";

        private static SceneBgmController instance;

        private AudioSource source;
        private SceneBgmSettings settings;
        private string currentSceneName;

        public static void PlayStagePhase(AudioClip clip, float volume)
        {
            if (clip == null)
            {
                return;
            }

            EnsureInstance().PlayClip(clip, Mathf.Clamp01(volume), restart: true);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
            EnsureInstance().ApplyScene(SceneManager.GetActiveScene());
        }

        private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EnsureInstance().ApplyScene(scene);
        }

        private static SceneBgmController EnsureInstance()
        {
            if (instance != null)
            {
                return instance;
            }

            SceneBgmController existing = FindFirstObjectByType<SceneBgmController>();
            if (existing != null)
            {
                instance = existing;
                DontDestroyOnLoad(existing.gameObject);
                return instance;
            }

            GameObject host = new GameObject("SceneBgmController");
            instance = host.AddComponent<SceneBgmController>();
            DontDestroyOnLoad(host);
            return instance;
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            source = GetComponent<AudioSource>();
            if (source == null)
            {
                source = gameObject.AddComponent<AudioSource>();
            }

            source.playOnAwake = false;
            source.loop = true;
            source.spatialBlend = 0f;
            source.priority = 16;
            settings = Resources.Load<SceneBgmSettings>(SettingsResourcePath);
        }

        private void ApplyScene(Scene scene)
        {
            if (settings == null)
            {
                settings = Resources.Load<SceneBgmSettings>(SettingsResourcePath);
            }

            if (settings == null)
            {
                return;
            }

            currentSceneName = scene.name;
            if (settings.TryGetEntry(currentSceneName, out SceneBgmEntry entry) && entry.BgmClip != null)
            {
                Play(entry);
                return;
            }

            if (settings.StopWhenSceneIsUnlisted)
            {
                source.Stop();
                source.clip = null;
                source.outputAudioMixerGroup = null;
            }
        }

        private void Play(SceneBgmEntry entry)
        {
            bool clipChanged = source.clip != entry.BgmClip;
            source.outputAudioMixerGroup = entry.OutputMixerGroup;
            source.volume = entry.Volume;
            source.loop = true;
            source.spatialBlend = 0f;
            source.priority = 16;

            if (clipChanged || entry.RestartWhenSceneLoads)
            {
                source.clip = entry.BgmClip;
                source.Play();
                return;
            }

            if (!source.isPlaying)
            {
                source.Play();
            }
        }

        private void PlayClip(AudioClip clip, float volume, bool restart)
        {
            bool clipChanged = source.clip != clip;
            source.outputAudioMixerGroup = null;
            source.volume = volume;
            source.loop = true;
            source.spatialBlend = 0f;
            source.priority = 16;

            if (clipChanged || restart)
            {
                source.clip = clip;
                source.Play();
                return;
            }

            if (!source.isPlaying)
            {
                source.Play();
            }
        }

    }
}
