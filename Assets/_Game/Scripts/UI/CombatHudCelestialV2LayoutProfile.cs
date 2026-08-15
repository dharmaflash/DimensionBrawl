using UnityEngine;

namespace DimensionBrawl.UI
{
    /// <summary>
    /// Marks an authored combat HUD prefab as the reviewed celestial V22 composition.
    /// The static rectangles are the single 2560x1440 layout contract shared by the
    /// editor assembler, runtime safe-area pass, and focused prefab tests.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CombatHudCelestialV2LayoutProfile : MonoBehaviour
    {
        public const int LayoutVersion = 22;
        public const float DesignWidth = 2560f;
        public const float DesignHeight = 1440f;

        public static readonly Rect ObjectiveFrame = new Rect(0f, 327f, 806f, 167f);
        public static readonly Rect ObjectiveText = new Rect(88f, 327f, 650f, 167f);
        public const float ObjectiveMinimumReadableWidth = 420f;

        public static Rect ResolveObjectiveText(float safeLeft)
        {
            float resolvedSafeLeft = Mathf.Max(0f, safeLeft);
            float x = ObjectiveText.xMin + resolvedSafeLeft;
            float preferredWidth = Mathf.Max(
                ObjectiveMinimumReadableWidth,
                ObjectiveText.width - resolvedSafeLeft);
            float availableWidth = Mathf.Max(0f, ObjectiveFrame.xMax - x);
            float width = Mathf.Min(preferredWidth, availableWidth);
            return new Rect(x, ObjectiveText.yMin, width, ObjectiveText.height);
        }
        public static readonly Rect BossFrame = new Rect(796f, 52f, 1056f, 132f);
        public static readonly Rect BossName = new Rect(850f, 57f, 500f, 46f);
        public static readonly Rect BossHpTrack = new Rect(839f, 104f, 913f, 18f);
        public static readonly Rect BossHpFill = new Rect(842f, 103f, 741f, 29f);
        public static readonly Rect BossHpValue = new Rect(1588f, 72f, 180f, 46f);
        public static readonly Rect BossCostTrack = new Rect(839f, 147f, 913f, 14f);
        public static readonly Rect BossCostFill = new Rect(842f, 138f, 821f, 13f);
        public static readonly Rect BossCostValue = new Rect(1660f, 124f, 120f, 38f);
        public static readonly Rect MissionTimerBacking = new Rect(2014f, 47f, 184f, 86f);
        public static readonly Rect MissionTimerText = new Rect(2026f, 47f, 160f, 86f);
        public static readonly Rect PauseHit = new Rect(2368.5f, 8.5f, 160f, 160f);
        public static readonly Rect PauseVisual = new Rect(2404f, 44f, 89f, 89f);
        public static readonly Rect SummonSlot1 = new Rect(2263f, 171f, 211f, 226f);
        public static readonly Rect SummonSlot2 = new Rect(2275f, 413f, 193f, 211f);
        public static readonly Rect SummonSlot3 = new Rect(2275f, 640f, 193f, 211f);
        public static readonly Rect WeaponSwap = new Rect(2059f, 967f, 171f, 171f);
        public static readonly Rect Skill = new Rect(2261f, 926f, 187f, 187f);
        public static readonly Rect Dodge = new Rect(2046f, 1177f, 184f, 184f);
        public static readonly Rect BasicAttack = new Rect(2248f, 1131f, 273f, 272f);
        public static readonly Rect JoystickVisual = new Rect(201f, 979f, 269f, 269f);
        public static readonly Rect JoystickKnob = new Rect(285f, 1063f, 101f, 101f);
        public static readonly Rect JoystickActivation = new Rect(145f, 923f, 381f, 381f);
        public static readonly Rect PlayerComposite = new Rect(686f, 1246f, 1182f, 169f);
        public static readonly Rect PlayerPortrait = new Rect(686f, 1262f, 153f, 153f);
        public static readonly Rect PlayerHpText = new Rect(900f, 1264f, 235f, 44f);
        public static readonly Rect PlayerHpTrack = new Rect(888f, 1317f, 456f, 26f);
        public static readonly Rect PlayerHpFill = new Rect(892f, 1322f, 444f, 16f);
        public static readonly Rect PlayerEnTrack = new Rect(888f, 1347f, 456f, 26f);
        public static readonly Rect PlayerEnFill = new Rect(892f, 1352f, 444f, 14f);
        public static readonly Rect PlayerEnText = new Rect(1200f, 1338f, 140f, 38f);
        public static readonly Rect PlayerMode = new Rect(1360f, 1284f, 240f, 77f);
        public static readonly Rect PlayerAmmo = new Rect(1605f, 1284f, 263f, 77f);
        public static readonly Rect Reticle = new Rect(1224f, 664f, 112f, 112f);

        public const float MinimumPlayerActionGap = 64f;
        public const float MaximumPlayerLeftShift = 96f;
        public const float MinimumPlayerScale = 0.92f;
        public const float MinimumJoystickActivationSize = 320f;
        public const float MinimumTimerScale = 0.82f;

        [SerializeField, HideInInspector] private int version = LayoutVersion;
        public int Version => version;
    }
}
