using UnityEngine;
using UnityEngine.Video;

namespace DimensionBrawl.UI
{
    [CreateAssetMenu(menuName = "DimensionBrawl/UI/Login Video Background Profile")]
    public sealed class LoginVideoBackgroundProfile : ScriptableObject
    {
        [SerializeField] private VideoClip backgroundClip;
        [SerializeField] private bool loop = true;
        [SerializeField] private bool muteAudio = true;
        [SerializeField] private bool hideVideoUntilPrepared = true;
        [SerializeField, Min(16)] private int fallbackWidth = 2560;
        [SerializeField, Min(16)] private int fallbackHeight = 1440;

        public VideoClip BackgroundClip => backgroundClip;
        public bool Loop => loop;
        public bool MuteAudio => muteAudio;
        public bool HideVideoUntilPrepared => hideVideoUntilPrepared;
        public int FallbackWidth => fallbackWidth;
        public int FallbackHeight => fallbackHeight;
    }
}
