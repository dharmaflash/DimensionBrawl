using UnityEngine;

namespace DimensionBrawl.UI
{
    [CreateAssetMenu(
        fileName = "DB_SceneEntryNoticeProfile",
        menuName = "DimensionBrawl/UI/Scene Entry Notice Profile")]
    public sealed class SceneEntryNoticeProfile : ScriptableObject
    {
        [SerializeField] private string eyebrowText = "SYSTEM NOTICE";
        [SerializeField] private string titleText = "Operation Link Established";
        [SerializeField, TextArea] private string bodyText = "Tactical support window is online.";
        [SerializeField] private string leftStatusText = "GUIDE // ONLINE";
        [SerializeField] private string rightStatusText = "SYNC // STABLE";
        [SerializeField] private Color accentColor = new Color(0.22f, 0.94f, 1f, 1f);
        [SerializeField] private Color backgroundColor = new Color(0.01f, 0.045f, 0.065f, 0.86f);
        [SerializeField] private Color scanColor = new Color(0.5f, 1f, 1f, 0.72f);
        [SerializeField] private Color dimColor = new Color(0f, 0.025f, 0.035f, 0.16f);
        [SerializeField, Min(0f)] private float startupDelaySeconds = 0.18f;
        [SerializeField, Min(0.01f)] private float revealSeconds = 0.36f;
        [SerializeField, Min(0f)] private float holdSeconds = 1.72f;
        [SerializeField, Min(0.01f)] private float dismissSeconds = 0.32f;
        [SerializeField, Min(0f)] private float typewriterCharactersPerSecond = 46f;
        [SerializeField] private AudioClip startBeepClip;

        public string EyebrowText => eyebrowText;
        public string TitleText => titleText;
        public string BodyText => bodyText;
        public string LeftStatusText => leftStatusText;
        public string RightStatusText => rightStatusText;
        public Color AccentColor => accentColor;
        public Color BackgroundColor => backgroundColor;
        public Color ScanColor => scanColor;
        public Color DimColor => dimColor;
        public float StartupDelaySeconds => startupDelaySeconds;
        public float RevealSeconds => revealSeconds;
        public float HoldSeconds => holdSeconds;
        public float DismissSeconds => dismissSeconds;
        public float TypewriterCharactersPerSecond => typewriterCharactersPerSecond;
        public AudioClip StartBeepClip => startBeepClip;
    }
}
