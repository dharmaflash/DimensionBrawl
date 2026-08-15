using UnityEngine;

namespace DimensionBrawl.UI
{
    /// <summary>
    /// Immutable 2560x1440 geometry contract for the approved dark-angular combat HUD.
    /// This marker is intentionally distinct from V22 so review staging can coexist with
    /// the canonical prefab without silently changing the live scene layout.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CombatHudCelestialTargetLayoutProfile : MonoBehaviour
    {
        public const int LayoutVersion = 23;
        public const float DesignWidth = 2560f;
        public const float DesignHeight = 1440f;

        public static readonly Rect ObjectiveFrame = new Rect(0f, 327f, 806f, 167f);
        public static readonly Rect ObjectiveText = new Rect(96f, 327f, 642f, 167f);
        public const float ObjectiveMinimumReadableWidth = 420f;

        public static Rect ResolveObjectiveText(float safeLeft)
        {
            float resolvedSafeLeft = Mathf.Max(0f, safeLeft);
            float x = ObjectiveText.xMin + resolvedSafeLeft;
            float preferredWidth = Mathf.Max(
                ObjectiveMinimumReadableWidth,
                ObjectiveText.width - resolvedSafeLeft);
            float availableWidth = Mathf.Max(0f, ObjectiveFrame.xMax - x);
            return new Rect(
                x,
                ObjectiveText.yMin,
                Mathf.Min(preferredWidth, availableWidth),
                ObjectiveText.height);
        }

        public static readonly Rect BossChassis = new Rect(827f, 61f, 945f, 126f);
        public static readonly Rect BossName = new Rect(847f, 61f, 304f, 46f);
        public static readonly Rect BossHpTrack = new Rect(827f, 98f, 945f, 45f);
        public static readonly Rect BossHpFill = new Rect(850f, 108f, 738f, 24f);
        public static readonly Rect BossHpValue = new Rect(1588f, 98f, 136f, 42f);
        public static readonly Rect BossCostTrack = new Rect(837f, 148f, 895f, 39f);
        public static readonly Rect BossCostFill = new Rect(850f, 157f, 738f, 20f);
        public static readonly Rect BossCostValue = new Rect(1594f, 148f, 120f, 38f);

        // The timer is opt-in at runtime and is not part of the default approved plate.
        public static readonly Rect MissionTimerBacking = new Rect(2014f, 47f, 184f, 86f);
        public static readonly Rect MissionTimerText = new Rect(2026f, 47f, 160f, 86f);
        public static readonly Rect PauseVisual = new Rect(2402f, 44f, 103f, 96f);
        public static readonly Rect PauseHit = CenteredHitRect(PauseVisual, 160f);

        public static readonly Rect SummonSlot1 = new Rect(2206f, 173f, 297f, 259f);
        public static readonly Rect SummonSlot2 = new Rect(2212f, 430f, 276f, 197f);
        public static readonly Rect SummonSlot3 = new Rect(2214f, 640f, 260f, 185f);

        // User-facing order: weapon swap / ultimate, then dash / ranged attack.
        public static readonly Rect WeaponSwap = new Rect(1991f, 928.5f, 208f, 208f);
        public static readonly Rect Ultimate = new Rect(2229f, 891.5f, 208f, 208f);
        public static readonly Rect Dash = new Rect(1909f, 1137.5f, 208f, 208f);
        public static readonly Rect BasicAttack = new Rect(2167f, 1120f, 260f, 260f);

        public static readonly Rect JoystickVisual = new Rect(190f, 966f, 296f, 305f);
        public static readonly Rect JoystickKnob = new Rect(287f, 1067f, 102f, 102f);
        public static readonly Rect JoystickActivation = CenteredHitRect(JoystickVisual, 381f);

        public static readonly Rect PlayerComposite = new Rect(686f, 1245f, 1182f, 170f);
        public static readonly Rect PlayerPortrait = new Rect(686f, 1262f, 153f, 153f);
        public static readonly Rect PlayerHpText = new Rect(900f, 1263f, 260f, 44f);
        public static readonly Rect PlayerHpTrack = new Rect(888f, 1307f, 672f, 32f);
        public static readonly Rect PlayerHpFill = new Rect(898f, 1314f, 652f, 20f);
        public static readonly Rect PlayerCostTrack = new Rect(888f, 1347f, 672f, 28f);
        public static readonly Rect PlayerCostFill = new Rect(898f, 1353f, 652f, 16f);
        public static readonly Rect PlayerModeGlyph = new Rect(1580f, 1294f, 64f, 64f);
        public static readonly Rect PlayerAmmo = new Rect(1654f, 1290f, 194f, 68f);
        public static readonly Rect PlayerAmmoText = new Rect(1734f, 1290f, 104f, 68f);
        public static readonly Rect Reticle = new Rect(1224f, 664f, 112f, 112f);

        public const float MinimumPlayerActionGap = 41f;
        public const float MaximumPlayerLeftShift = 80f;
        public const float MinimumPlayerScale = 0.90f;
        public const float MinimumJoystickActivationSize = 320f;
        public const float MinimumTimerScale = 0.82f;

        [SerializeField, HideInInspector] private int version = LayoutVersion;
        public int Version => version;

        private static Rect CenteredHitRect(Rect visual, float size)
        {
            return new Rect(
                visual.center.x - size * 0.5f,
                visual.center.y - size * 0.5f,
                size,
                size);
        }
    }
}
