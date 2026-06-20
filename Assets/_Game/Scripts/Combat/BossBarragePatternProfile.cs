using UnityEngine;

namespace DimensionBrawl.Combat
{
    public enum BossBarrageLateralShape
    {
        CenterSpread = 0,
        TwinColumns = 1,
        SideClamp = 2,
        PunishNet = 3,
        LinePressure = 4,
        EscortScreen = 5,
        LayeredSalvo = 6,
        StaggeredCrossfire = 7,
    }

    public enum BossBarrageTargetingRule
    {
        TrackedPlayer = 0,
        LaneCenter = 1,
    }

    public enum LaneSkillPatternFamily
    {
        DirectLock = 0,
        CenterCover = 1,
        EscortScreen = 2,
        LayeredSalvo = 3,
        StaggeredCrossfire = 4,
        TwinSweep = 5,
        SideClamp = 6,
        PunishNet = 7,
        LinePressure = 8,
    }

    public enum LaneSkillTransferMode
    {
        BossOnly = 0,
        CostedPlayerSkillCandidate = 1,
        SharedPvpSkillCandidate = 2,
    }

    [CreateAssetMenu(
        fileName = "DB_BossBarragePattern",
        menuName = "DimensionBrawl/Combat/Boss Barrage Pattern")]
    public sealed class BossBarragePatternProfile : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string patternId = "NeedleLock";

        [Header("Shared Skill Grammar")]
        [SerializeField] private LaneSkillPatternFamily skillPatternFamily = LaneSkillPatternFamily.DirectLock;
        [SerializeField] private LaneSkillTransferMode skillTransferMode = LaneSkillTransferMode.BossOnly;
        [Tooltip("How this boss pattern could become a costed player/PvP skill without becoming basic fire.")]
        [SerializeField, TextArea(1, 3)] private string playerSkillTranslationNote;
        [Tooltip("Readable answer the opposing side should have before this pattern is reused as a shared skill verb.")]
        [SerializeField, TextArea(1, 3)] private string counterplayNote;

        [Header("Telegraph Read")]
        [SerializeField] private Color telegraphWindupColor = new Color(1f, 0.62f, 0.18f, 0.64f);
        [SerializeField] private Color telegraphReleaseColor = new Color(1f, 0.96f, 0.44f, 0.9f);
        [Tooltip("Pattern-specific width multiplier for in-world incoming-lane markers.")]
        [SerializeField, Min(0.05f)] private float telegraphMarkerWidthScale = 1f;
        [Tooltip("Pattern-specific depth multiplier for in-world incoming-lane markers.")]
        [SerializeField, Min(0.05f)] private float telegraphMarkerDepthScale = 1f;
        [Tooltip("Pattern-specific pulse strength for in-world incoming-lane markers.")]
        [SerializeField, Min(0f)] private float telegraphPulseScale = 1f;

        [Header("Projectile Read")]
        [Tooltip("Pattern-specific color applied to the actual fired projectile, not only the telegraph.")]
        [SerializeField] private Color projectileColor = new Color(1f, 0.72f, 0.12f, 1f);
        [Tooltip("Pattern-specific visual scale applied to fired projectiles. Keep gameplay radius in Projectile separate.")]
        [SerializeField] private Vector3 projectileVisualScale = Vector3.one;
        [Tooltip("Optional game-owned material for patterns that need a stronger visual read than material property overrides.")]
        [SerializeField] private Material projectileMaterial;

        [Header("Timing")]
        [SerializeField, Min(0f)] private float initialDelaySeconds = 1.2f;
        [SerializeField, Range(0.45f, 1.2f)] private float windupSeconds = 0.9f;
        [SerializeField, Min(0.5f)] private float waveIntervalSeconds = 5.6f;

        [Header("Projectile")]
        [SerializeField, Range(1, 9)] private int projectilesPerWave = 3;
        [SerializeField, Min(0f)] private float damage = 18f;
        [SerializeField, Min(0.1f)] private float projectileSpeed = 13f;
        [SerializeField, Min(0.1f)] private float projectileLifetimeSeconds = 4.5f;
        [SerializeField, Min(0f)] private float projectileRadius = 0.32f;

        [Header("Targeting")]
        [SerializeField] private BossBarrageTargetingRule targetingRule = BossBarrageTargetingRule.TrackedPlayer;
        [Tooltip("LaneCenter targeting uses this authored lateral position instead of tracking the player's current side.")]
        [SerializeField, Range(-1f, 1f)] private float laneCenterLateralRatio;

        [Header("Forward Risk Shape")]
        [SerializeField] private BossBarrageLateralShape lateralShape = BossBarrageLateralShape.CenterSpread;
        [Tooltip("Backline gaps should be wider so defensive play is safer but charges EN slower.")]
        [SerializeField, Min(0f)] private float backlineHalfSpread = 2.8f;
        [Tooltip("Forward-risk gaps are tighter, making aggressive EN charging more dangerous.")]
        [SerializeField, Min(0f)] private float forwardHalfSpread = 1.05f;
        [Tooltip("Twin-column patterns leave a readable middle gap while still tightening near the forward boundary.")]
        [SerializeField, Range(0f, 0.8f)] private float twinColumnInnerSpreadRatio = 0.38f;
        [Tooltip("Negative values clamp from the left, positive values clamp from the right.")]
        [SerializeField, Range(-1f, 1f)] private float sideClampDirection = -1f;
        [Tooltip("How far the clamped side reaches across center. Higher values leave a smaller opposite-side gap.")]
        [SerializeField, Range(0f, 0.65f)] private float sideClampCrossReachRatio = 0.28f;
        [Tooltip("Player-centered net patterns place inner shots near center before the outer ring.")]
        [SerializeField, Range(0.05f, 0.75f)] private float punishNetInnerSpreadRatio = 0.34f;
        [Tooltip("Escort-screen patterns alternate left/right curtain shots and preserve this inner gap around the escorted path.")]
        [SerializeField, Range(0.08f, 0.85f)] private float escortScreenInnerGapRatio = 0.28f;
        [Tooltip("Layered salvo patterns compress multi-round barrage reads into this many target-depth rows.")]
        [SerializeField, Range(2, 5)] private int layeredSalvoRowCount = 3;
        [Tooltip("Staggered crossfire patterns leave a late inner gap after the first wide crossing row.")]
        [SerializeField, Range(0.08f, 0.8f)] private float crossfireInnerGapRatio = 0.32f;
        [Tooltip("Negative values commit the line to the left of the sampled target, positive values commit it to the right.")]
        [SerializeField, Range(-1f, 1f)] private float linePressureDirection = 1f;
        [Tooltip("How far from the sampled target the pressure line sits, expressed against the current spread.")]
        [SerializeField, Range(0.2f, 1f)] private float linePressureCenterRatio = 0.72f;
        [Tooltip("Small local scatter around the committed line so projectiles read as a lane, not one stacked shot.")]
        [SerializeField, Range(0f, 0.35f)] private float linePressureHalfSpreadRatio = 0.08f;
        [Tooltip("Backline line-pressure depth spacing. Wider spacing makes the safe back zone more readable.")]
        [SerializeField, Min(0f)] private float backlineDepthSpread = 2.2f;
        [Tooltip("Forward-risk line-pressure depth spacing. Tighter spacing increases risk near the boundary.")]
        [SerializeField, Min(0f)] private float forwardDepthSpread = 0.85f;
        [SerializeField, Min(0f)] private float spawnHeight = 1.25f;
        [SerializeField, Min(0f)] private float targetHeight = 1f;

        public string PatternId => patternId;
        public LaneSkillPatternFamily SkillPatternFamily => skillPatternFamily;
        public LaneSkillTransferMode SkillTransferMode => skillTransferMode;
        public string PlayerSkillTranslationNote => playerSkillTranslationNote;
        public string CounterplayNote => counterplayNote;
        public bool IsPlayerSkillCandidate => skillTransferMode != LaneSkillTransferMode.BossOnly;
        public Color TelegraphWindupColor => telegraphWindupColor;
        public Color TelegraphReleaseColor => telegraphReleaseColor;
        public float TelegraphMarkerWidthScale => telegraphMarkerWidthScale;
        public float TelegraphMarkerDepthScale => telegraphMarkerDepthScale;
        public float TelegraphPulseScale => telegraphPulseScale;
        public Color ProjectileColor => projectileColor;
        public Vector3 ProjectileVisualScale => SanitizeVisualScale(projectileVisualScale);
        public Material ProjectileMaterial => projectileMaterial;
        public BossBarrageLateralShape LateralShape => lateralShape;
        public float InitialDelaySeconds => initialDelaySeconds;
        public float WindupSeconds => windupSeconds;
        public float WaveIntervalSeconds => waveIntervalSeconds;
        public int ProjectilesPerWave => projectilesPerWave;
        public float Damage => damage;
        public float ProjectileSpeed => projectileSpeed;
        public float ProjectileLifetimeSeconds => projectileLifetimeSeconds;
        public float ProjectileRadius => projectileRadius;
        public BossBarrageTargetingRule TargetingRule => targetingRule;
        public float LaneCenterLateralRatio => laneCenterLateralRatio;
        public float SideClampDirection => sideClampDirection;
        public float SideClampCrossReachRatio => sideClampCrossReachRatio;
        public float PunishNetInnerSpreadRatio => punishNetInnerSpreadRatio;
        public float EscortScreenInnerGapRatio => escortScreenInnerGapRatio;
        public int LayeredSalvoRowCount => layeredSalvoRowCount;
        public float CrossfireInnerGapRatio => crossfireInnerGapRatio;
        public float LinePressureDirection => linePressureDirection;
        public float LinePressureCenterRatio => linePressureCenterRatio;
        public float LinePressureHalfSpreadRatio => linePressureHalfSpreadRatio;
        public float SpawnHeight => spawnHeight;
        public float TargetHeight => targetHeight;

        private static Vector3 SanitizeVisualScale(Vector3 scale)
        {
            return new Vector3(
                Mathf.Max(0.05f, scale.x),
                Mathf.Max(0.05f, scale.y),
                Mathf.Max(0.05f, scale.z));
        }

        public float EvaluateHalfSpread(float forwardRisk01)
        {
            return Mathf.Lerp(backlineHalfSpread, forwardHalfSpread, Mathf.Clamp01(forwardRisk01));
        }

        public float ResolveTargetLateralX(float trackedLateralX, float laneHalfWidth)
        {
            if (targetingRule == BossBarrageTargetingRule.LaneCenter)
            {
                return Mathf.Clamp(laneCenterLateralRatio, -1f, 1f) * Mathf.Max(0f, laneHalfWidth);
            }

            return trackedLateralX;
        }

        public float GetLateralOffset(int projectileIndex, int count, float forwardRisk01)
        {
            if (lateralShape == BossBarrageLateralShape.TwinColumns)
            {
                return GetTwinColumnOffset(projectileIndex, count, forwardRisk01);
            }

            if (lateralShape == BossBarrageLateralShape.SideClamp)
            {
                return GetSideClampOffset(projectileIndex, count, forwardRisk01);
            }

            if (lateralShape == BossBarrageLateralShape.PunishNet)
            {
                return GetPunishNetOffset(projectileIndex, count, forwardRisk01);
            }

            if (lateralShape == BossBarrageLateralShape.LinePressure)
            {
                return GetLinePressureOffset(projectileIndex, count, forwardRisk01);
            }

            if (lateralShape == BossBarrageLateralShape.EscortScreen)
            {
                return GetEscortScreenOffset(projectileIndex, count, forwardRisk01);
            }

            if (lateralShape == BossBarrageLateralShape.LayeredSalvo)
            {
                return GetLayeredSalvoOffset(projectileIndex, count, forwardRisk01);
            }

            if (lateralShape == BossBarrageLateralShape.StaggeredCrossfire)
            {
                return GetStaggeredCrossfireOffset(projectileIndex, count, forwardRisk01);
            }

            int safeCount = Mathf.Max(1, count);
            if (safeCount <= 1)
            {
                return 0f;
            }

            float halfSpread = EvaluateHalfSpread(forwardRisk01);
            float normalizedIndex = Mathf.Clamp01((float)projectileIndex / (safeCount - 1));
            return Mathf.Lerp(-halfSpread, halfSpread, normalizedIndex);
        }

        public float GetTargetDepthOffset(int projectileIndex, int count, float forwardRisk01)
        {
            if (lateralShape != BossBarrageLateralShape.LinePressure
                && lateralShape != BossBarrageLateralShape.EscortScreen
                && lateralShape != BossBarrageLateralShape.LayeredSalvo
                && lateralShape != BossBarrageLateralShape.StaggeredCrossfire)
            {
                return 0f;
            }

            int safeCount = Mathf.Max(1, count);
            if (safeCount <= 1)
            {
                return 0f;
            }

            if (lateralShape == BossBarrageLateralShape.LayeredSalvo)
            {
                return GetLayeredSalvoDepthOffset(projectileIndex, count, forwardRisk01);
            }

            if (lateralShape == BossBarrageLateralShape.StaggeredCrossfire)
            {
                return GetStaggeredCrossfireDepthOffset(projectileIndex, count, forwardRisk01);
            }

            float depthSpread = EvaluateDepthSpread(forwardRisk01);
            float normalizedIndex = Mathf.Clamp01((float)Mathf.Clamp(projectileIndex, 0, safeCount - 1) / (safeCount - 1));
            return Mathf.Lerp(-depthSpread, depthSpread, normalizedIndex);
        }

        private float GetTwinColumnOffset(int projectileIndex, int count, float forwardRisk01)
        {
            int safeCount = Mathf.Max(1, count);
            if (safeCount <= 1)
            {
                return 0f;
            }

            float halfSpread = EvaluateHalfSpread(forwardRisk01);
            float innerSpread = halfSpread * Mathf.Clamp01(twinColumnInnerSpreadRatio);
            if (safeCount <= 2)
            {
                return projectileIndex <= 0 ? -halfSpread : halfSpread;
            }

            int safeIndex = Mathf.Clamp(projectileIndex, 0, safeCount - 1);
            int leftCount = safeCount / 2;
            bool isLeft = safeIndex < leftCount;
            int sideIndex = isLeft ? safeIndex : safeIndex - leftCount;
            int sideCount = isLeft ? leftCount : safeCount - leftCount;
            float side01 = sideCount <= 1 ? 0f : Mathf.Clamp01((float)sideIndex / (sideCount - 1));
            float magnitude = isLeft
                ? Mathf.Lerp(halfSpread, innerSpread, side01)
                : Mathf.Lerp(innerSpread, halfSpread, side01);

            return isLeft ? -magnitude : magnitude;
        }

        private float GetSideClampOffset(int projectileIndex, int count, float forwardRisk01)
        {
            int safeCount = Mathf.Max(1, count);
            if (safeCount <= 1)
            {
                return 0f;
            }

            float halfSpread = EvaluateHalfSpread(forwardRisk01);
            float direction = sideClampDirection < 0f ? -1f : 1f;
            float outerEdge = direction * halfSpread;
            float crossReach = -direction * halfSpread * Mathf.Clamp01(sideClampCrossReachRatio);
            float normalizedIndex = Mathf.Clamp01((float)Mathf.Clamp(projectileIndex, 0, safeCount - 1) / (safeCount - 1));
            return Mathf.Lerp(outerEdge, crossReach, normalizedIndex);
        }

        private float GetPunishNetOffset(int projectileIndex, int count, float forwardRisk01)
        {
            int safeCount = Mathf.Max(1, count);
            if (safeCount <= 1)
            {
                return 0f;
            }

            int safeIndex = Mathf.Clamp(projectileIndex, 0, safeCount - 1);
            if (safeIndex == 0)
            {
                return 0f;
            }

            float halfSpread = EvaluateHalfSpread(forwardRisk01);
            float innerSpread = halfSpread * Mathf.Clamp01(punishNetInnerSpreadRatio);
            int pairIndex = (safeIndex - 1) / 2;
            int pairCount = Mathf.Max(1, safeCount / 2);
            float pair01 = pairCount <= 1 ? 0f : Mathf.Clamp01((float)pairIndex / (pairCount - 1));
            float magnitude = Mathf.Lerp(innerSpread, halfSpread, pair01);
            return safeIndex % 2 == 1 ? -magnitude : magnitude;
        }

        private float GetLinePressureOffset(int projectileIndex, int count, float forwardRisk01)
        {
            float halfSpread = EvaluateHalfSpread(forwardRisk01);
            float direction = linePressureDirection < 0f ? -1f : 1f;
            float center = direction * halfSpread * Mathf.Clamp(linePressureCenterRatio, 0.2f, 1f);
            int safeCount = Mathf.Max(1, count);
            if (safeCount <= 1)
            {
                return center;
            }

            float localHalfSpread = halfSpread * Mathf.Clamp01(linePressureHalfSpreadRatio);
            float normalizedIndex = Mathf.Clamp01((float)Mathf.Clamp(projectileIndex, 0, safeCount - 1) / (safeCount - 1));
            return center + Mathf.Lerp(-localHalfSpread, localHalfSpread, normalizedIndex);
        }

        private float GetEscortScreenOffset(int projectileIndex, int count, float forwardRisk01)
        {
            int safeCount = Mathf.Max(1, count);
            if (safeCount <= 1)
            {
                return 0f;
            }

            float halfSpread = EvaluateHalfSpread(forwardRisk01);
            float innerGap = halfSpread * Mathf.Clamp(escortScreenInnerGapRatio, 0.08f, 0.85f);
            int safeIndex = Mathf.Clamp(projectileIndex, 0, safeCount - 1);
            int pairIndex = safeIndex / 2;
            int pairCount = Mathf.Max(1, (safeCount + 1) / 2);
            float pair01 = pairCount <= 1 ? 0f : Mathf.Clamp01((float)pairIndex / (pairCount - 1));
            float magnitude = Mathf.Lerp(halfSpread, innerGap, pair01);
            return safeIndex % 2 == 0 ? -magnitude : magnitude;
        }

        private float GetLayeredSalvoOffset(int projectileIndex, int count, float forwardRisk01)
        {
            int safeCount = Mathf.Max(1, count);
            if (safeCount <= 1)
            {
                return 0f;
            }

            int rowCount = ResolveLayeredSalvoRowCount(safeCount);
            int columnCount = Mathf.Max(1, Mathf.CeilToInt((float)safeCount / rowCount));
            int safeIndex = Mathf.Clamp(projectileIndex, 0, safeCount - 1);
            int rowIndex = Mathf.Clamp(safeIndex / columnCount, 0, rowCount - 1);
            int columnIndex = safeIndex % columnCount;
            float row01 = rowCount <= 1 ? 0f : Mathf.Clamp01((float)rowIndex / (rowCount - 1));
            float column01 = columnCount <= 1 ? 0.5f : Mathf.Clamp01((float)columnIndex / (columnCount - 1));
            float rowHalfSpread = EvaluateHalfSpread(forwardRisk01) * Mathf.Lerp(1f, 0.55f, row01);
            float offset = Mathf.Lerp(-rowHalfSpread, rowHalfSpread, column01);
            return rowIndex % 2 == 0 ? offset : -offset * 0.85f;
        }

        private float GetLayeredSalvoDepthOffset(int projectileIndex, int count, float forwardRisk01)
        {
            int safeCount = Mathf.Max(1, count);
            if (safeCount <= 1)
            {
                return 0f;
            }

            int rowCount = ResolveLayeredSalvoRowCount(safeCount);
            int columnCount = Mathf.Max(1, Mathf.CeilToInt((float)safeCount / rowCount));
            int safeIndex = Mathf.Clamp(projectileIndex, 0, safeCount - 1);
            int rowIndex = Mathf.Clamp(safeIndex / columnCount, 0, rowCount - 1);
            float row01 = rowCount <= 1 ? 0.5f : Mathf.Clamp01((float)rowIndex / (rowCount - 1));
            float depthSpread = EvaluateDepthSpread(forwardRisk01);
            return Mathf.Lerp(-depthSpread, depthSpread, row01);
        }

        private float GetStaggeredCrossfireOffset(int projectileIndex, int count, float forwardRisk01)
        {
            int safeCount = Mathf.Max(1, count);
            if (safeCount <= 1)
            {
                return 0f;
            }

            int safeIndex = Mathf.Clamp(projectileIndex, 0, safeCount - 1);
            int pairIndex = safeIndex / 2;
            int pairCount = Mathf.Max(1, Mathf.CeilToInt(safeCount / 2f));
            float pair01 = pairCount <= 1 ? 0f : Mathf.Clamp01((float)pairIndex / (pairCount - 1));
            float halfSpread = EvaluateHalfSpread(forwardRisk01);
            float innerGap = halfSpread * Mathf.Clamp(crossfireInnerGapRatio, 0.08f, 0.8f);
            float magnitude = Mathf.Lerp(halfSpread, innerGap, pair01);
            bool startsLeft = pairIndex % 2 == 0;
            bool isFirstInPair = safeIndex % 2 == 0;
            return startsLeft == isFirstInPair ? -magnitude : magnitude;
        }

        private float GetStaggeredCrossfireDepthOffset(int projectileIndex, int count, float forwardRisk01)
        {
            int safeCount = Mathf.Max(1, count);
            if (safeCount <= 1)
            {
                return 0f;
            }

            int pairIndex = Mathf.Clamp(projectileIndex, 0, safeCount - 1) / 2;
            int pairCount = Mathf.Max(1, Mathf.CeilToInt(safeCount / 2f));
            float pair01 = pairCount <= 1 ? 0.5f : Mathf.Clamp01((float)pairIndex / (pairCount - 1));
            float depthSpread = EvaluateDepthSpread(forwardRisk01);
            return Mathf.Lerp(-depthSpread, depthSpread, pair01);
        }

        private int ResolveLayeredSalvoRowCount(int count)
        {
            return Mathf.Clamp(layeredSalvoRowCount, 2, Mathf.Max(2, Mathf.Min(5, count)));
        }

        private float EvaluateDepthSpread(float forwardRisk01)
        {
            return Mathf.Lerp(backlineDepthSpread, forwardDepthSpread, Mathf.Clamp01(forwardRisk01));
        }
    }
}
