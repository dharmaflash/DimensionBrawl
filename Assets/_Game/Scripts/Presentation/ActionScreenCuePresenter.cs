using DimensionBrawl.Combat;
using DimensionBrawl.Player;
using DimensionBrawl.Test;
using UnityEngine;

namespace DimensionBrawl.Presentation
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(900)]
    public sealed class ActionScreenCuePresenter : MonoBehaviour
    {
        private enum ScreenCueCategory
        {
            Player,
            Boss,
            Frontline,
            Followup,
            Result
        }

        [Header("References")]
        [SerializeField] private PlayerActionController actionController;
        [SerializeField] private CombatHealth playerHealth;
        [SerializeField] private PlayerRangedBasicAttackAction rangedBasicAttackAction;
        [SerializeField] private SummonEnergyLadder energyLadder;
        [SerializeField] private PlayerSkill1Action skill1Action;
        [SerializeField] private PlayerSummonSlot1Action summonSlot1Action;
        [SerializeField] private PlayerSupportSummonSlotAction summonSlot2Action;
        [SerializeField] private PlayerSupportSummonSlotAction summonSlot3Action;
        [SerializeField] private BossBarrageEmitter bossBarrageEmitter;
        [SerializeField] private BossPressureActionDirector bossPressureActionDirector;
        [SerializeField] private BossBarragePocketReviewOwner pocketReviewOwner;

        [Header("Display")]
        [SerializeField] private bool showScreenCues = true;
        [SerializeField, Range(0f, 0.35f)] private float maxFullScreenAlpha = 0.10f;
        [SerializeField, Range(0f, 0.65f)] private float maxEdgeAlpha = 0.26f;
        [SerializeField, Min(0f)] private float edgeThickness = 104f;

        [Header("Damage Screen Feedback")]
        [SerializeField] private bool useDamageScreenFeedback = true;
        [SerializeField, Range(0f, 0.65f)] private float maxDamageVignetteAlpha = 0.42f;
        [SerializeField, Range(0f, 0.3f)] private float maxDamageFlashAlpha = 0.11f;
        [SerializeField, Min(0.01f)] private float damageVignetteSeconds = 0.34f;
        [SerializeField, Min(0f)] private float heavyDamageExtraSeconds = 0.14f;
        [SerializeField, Range(0.01f, 1f)] private float heavyDamageHealthRatio = 0.26f;
        [SerializeField, Range(0f, 0.45f)] private float criticalHealthThreshold = 0.32f;
        [SerializeField, Range(0f, 0.4f)] private float criticalHealthPulseAlpha = 0.13f;
        [SerializeField, Min(0f)] private float criticalHealthPulseSeconds = 0.9f;
        [SerializeField, Min(0f)] private float criticalHealthPulseRate = 2.3f;
        [SerializeField, Range(0f, 0.45f)] private float damageDirectionAccentAlpha = 0.24f;
        [SerializeField, Min(0f)] private float damageDirectionAccentThickness = 178f;

        [Header("Player Colors")]
        [SerializeField] private Color dodgeColor = new Color(0.18f, 0.92f, 1f, 1f);
        [SerializeField] private Color hitColor = new Color(1f, 0.92f, 0.46f, 1f);
        [SerializeField] private Color damagedColor = new Color(1f, 0.18f, 0.12f, 1f);
        [SerializeField] private Color skillColor = new Color(0.46f, 1f, 0.78f, 1f);
        [SerializeField] private Color summonColor = new Color(0.18f, 1f, 0.62f, 1f);
        [SerializeField] private Color summonBlockColor = new Color(0.85f, 1f, 1f, 1f);

        [Header("Energy Colors")]
        [SerializeField] private Color forwardRiskColor = new Color(0.35f, 1f, 0.72f, 1f);
        [SerializeField] private Color energyReadyColor = new Color(0.92f, 1f, 0.34f, 1f);
        [SerializeField] private Color energySpendColor = new Color(0.62f, 0.74f, 0.92f, 1f);

        [Header("Boss Colors")]
        [SerializeField] private Color bossWindupColor = new Color(1f, 0.62f, 0.16f, 1f);
        [SerializeField] private Color bossFireColor = new Color(1f, 0.2f, 0.1f, 1f);
        [SerializeField] private Color bossPressureColor = new Color(1f, 0.24f, 0.58f, 1f);

        [Header("Frontline Colors")]
        [SerializeField] private Color frontlineBeatColor = new Color(0.52f, 0.92f, 1f, 1f);
        [SerializeField] private Color frontlineWarningColor = new Color(1f, 0.72f, 0.22f, 1f);

        [Header("Follow-up Colors")]
        [SerializeField] private Color followupWindowColor = new Color(0.34f, 1f, 0.64f, 1f);
        [SerializeField] private Color followupHitColor = new Color(1f, 0.78f, 0.22f, 1f);
        [SerializeField] private Color followupMissedColor = new Color(0.68f, 0.74f, 0.82f, 1f);

        [Header("Result Colors")]
        [SerializeField] private Color pocketClearColor = new Color(0.22f, 1f, 0.42f, 1f);
        [SerializeField] private Color pocketFailColor = new Color(1f, 0.18f, 0.12f, 1f);

        private bool subscribed;
        private float flashTimer;
        private float flashDuration;
        private float vignetteTimer;
        private float vignetteDuration;
        private float damageFeedbackTimer;
        private float damageFeedbackDuration;
        private float damageFeedbackIntensity;
        private float criticalHealthPulseTimer;
        private Vector2 damageScreenDirection = Vector2.zero;
        private Texture2D damageVignetteTexture;
        private float activeIntensity = 1f;
        private Color activeFlashColor = Color.clear;
        private Color activeVignetteColor = Color.clear;
        private ScreenCueCategory activeCategory = ScreenCueCategory.Player;
        private int cueRequestCount;
        private int playerCueRequestCount;
        private int bossCueRequestCount;
        private int frontlineCueRequestCount;
        private int followupCueRequestCount;
        private int resultCueRequestCount;
        private int playerDamageCueRequestCount;
        private int energyCueRequestCount;
        private int forwardRiskCueRequestCount;
        private int energyReadyCueRequestCount;
        private int energySpendCueRequestCount;
        private int damageFeedbackRequestCount;
        private int suppressedCueRequestCount;
        private int frontlineStabilityCueRequestCount;
        private string lastCueId = string.Empty;
        private Color lastCueColor = Color.clear;
        private float lastCueIntensity;
        private float lastDamageFeedbackIntensity;
        private float lastDamageFeedbackDuration;
        private Vector2 lastDamageScreenDirection = Vector2.zero;
        private int lastEnergyCueTier;
        private int lastFrontlineBeatIndex = -1;
        private string lastFrontlineBeatLabel = string.Empty;
        private BossBarragePocketReviewOwner.RouteStabilityBand lastFrontlineStabilityBand =
            BossBarragePocketReviewOwner.RouteStabilityBand.Stable;
        private float lastFrontlineStability01 = 1f;
        private SummonEnergyRiskBand lastEnergyRiskBand = SummonEnergyRiskBand.BackSafety;

        public bool ShowScreenCues => showScreenCues;
        public bool HasActiveCue => flashTimer > 0f || vignetteTimer > 0f;
        public bool HasActiveDamageFeedback => damageFeedbackTimer > 0f || criticalHealthPulseTimer > 0f;
        public float EdgeThickness => edgeThickness;
        public float MaxFullScreenAlpha => maxFullScreenAlpha;
        public float MaxEdgeAlpha => maxEdgeAlpha;
        public bool UseDamageScreenFeedback => useDamageScreenFeedback;
        public float MaxDamageVignetteAlpha => maxDamageVignetteAlpha;
        public float MaxDamageFlashAlpha => maxDamageFlashAlpha;
        public float DamageVignetteSeconds => damageVignetteSeconds;
        public float HeavyDamageExtraSeconds => heavyDamageExtraSeconds;
        public float HeavyDamageHealthRatio => heavyDamageHealthRatio;
        public float CriticalHealthThreshold => criticalHealthThreshold;
        public float CriticalHealthPulseAlpha => criticalHealthPulseAlpha;
        public float CriticalHealthPulseSeconds => criticalHealthPulseSeconds;
        public float CriticalHealthPulseRate => criticalHealthPulseRate;
        public float DamageDirectionAccentAlpha => damageDirectionAccentAlpha;
        public float DamageDirectionAccentThickness => damageDirectionAccentThickness;
        public int CueRequestCount => cueRequestCount;
        public int PlayerCueRequestCount => playerCueRequestCount;
        public int BossCueRequestCount => bossCueRequestCount;
        public int FrontlineCueRequestCount => frontlineCueRequestCount;
        public int FollowupCueRequestCount => followupCueRequestCount;
        public int ResultCueRequestCount => resultCueRequestCount;
        public int PlayerDamageCueRequestCount => playerDamageCueRequestCount;
        public int EnergyCueRequestCount => energyCueRequestCount;
        public int ForwardRiskCueRequestCount => forwardRiskCueRequestCount;
        public int EnergyReadyCueRequestCount => energyReadyCueRequestCount;
        public int EnergySpendCueRequestCount => energySpendCueRequestCount;
        public int DamageFeedbackRequestCount => damageFeedbackRequestCount;
        public int SuppressedCueRequestCount => suppressedCueRequestCount;
        public int FrontlineStabilityCueRequestCount => frontlineStabilityCueRequestCount;
        public string LastCueId => lastCueId;
        public Color LastCueColor => lastCueColor;
        public float LastCueIntensity => lastCueIntensity;
        public float LastDamageFeedbackIntensity => lastDamageFeedbackIntensity;
        public float LastDamageFeedbackDuration => lastDamageFeedbackDuration;
        public Vector2 LastDamageScreenDirection => lastDamageScreenDirection;
        public int LastEnergyCueTier => lastEnergyCueTier;
        public int LastFrontlineBeatIndex => lastFrontlineBeatIndex;
        public string LastFrontlineBeatLabel => lastFrontlineBeatLabel;
        public BossBarragePocketReviewOwner.RouteStabilityBand LastFrontlineStabilityBand => lastFrontlineStabilityBand;
        public float LastFrontlineStability01 => lastFrontlineStability01;
        public SummonEnergyRiskBand LastEnergyRiskBand => lastEnergyRiskBand;

        public void SetScreenCuesVisible(bool visible)
        {
            showScreenCues = visible;
        }

        public void Configure(
            PlayerActionController newActionController,
            CombatHealth newPlayerHealth,
            PlayerRangedBasicAttackAction newRangedBasicAttackAction,
            SummonEnergyLadder newEnergyLadder,
            PlayerSkill1Action newSkill1Action,
            PlayerSummonSlot1Action newSummonSlot1Action,
            PlayerSupportSummonSlotAction newSummonSlot2Action,
            PlayerSupportSummonSlotAction newSummonSlot3Action,
            BossBarrageEmitter newBossBarrageEmitter,
            BossPressureActionDirector newBossPressureActionDirector,
            BossBarragePocketReviewOwner newPocketReviewOwner)
        {
            Unsubscribe();
            actionController = newActionController;
            playerHealth = newPlayerHealth;
            rangedBasicAttackAction = newRangedBasicAttackAction;
            energyLadder = newEnergyLadder;
            skill1Action = newSkill1Action;
            summonSlot1Action = newSummonSlot1Action;
            summonSlot2Action = newSummonSlot2Action;
            summonSlot3Action = newSummonSlot3Action;
            bossBarrageEmitter = newBossBarrageEmitter;
            bossPressureActionDirector = newBossPressureActionDirector;
            pocketReviewOwner = newPocketReviewOwner;
            Subscribe();
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void OnDestroy()
        {
            if (damageVignetteTexture == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(damageVignetteTexture);
            }
            else
            {
                DestroyImmediate(damageVignetteTexture);
            }

            damageVignetteTexture = null;
        }

        private void Update()
        {
            float deltaTime = Time.unscaledDeltaTime > 0f ? Time.unscaledDeltaTime : Time.deltaTime;
            flashTimer = Mathf.Max(0f, flashTimer - deltaTime);
            vignetteTimer = Mathf.Max(0f, vignetteTimer - deltaTime);
            damageFeedbackTimer = Mathf.Max(0f, damageFeedbackTimer - deltaTime);
            criticalHealthPulseTimer = Mathf.Max(0f, criticalHealthPulseTimer - deltaTime);
        }

        private void OnGUI()
        {
            if (!showScreenCues || (!HasActiveCue && !HasActiveDamageFeedback))
            {
                return;
            }

            int previousDepth = GUI.depth;
            Color previousColor = GUI.color;
            GUI.depth = 1000;
            if (flashTimer > 0f)
            {
                float alpha = maxFullScreenAlpha * activeIntensity * ResolveFade01(flashTimer, flashDuration);
                DrawRect(new Rect(0f, 0f, Screen.width, Screen.height), WithAlpha(activeFlashColor, alpha));
            }

            if (vignetteTimer > 0f)
            {
                float alpha = maxEdgeAlpha * activeIntensity * ResolveFade01(vignetteTimer, vignetteDuration);
                DrawVignette(WithAlpha(activeVignetteColor, alpha));
            }

            DrawDamageFeedback();

            GUI.color = previousColor;
            GUI.depth = previousDepth;
        }

        private void HandleBasicAttackStarted(int comboIndex)
        {
            RequestScreenCue("Player.BasicStart", hitColor, 0.10f, 0.35f, ScreenCueCategory.Player);
        }

        private void HandleBasicAttackHit(int comboIndex)
        {
            float scale = Mathf.Lerp(0.72f, 1.08f, Mathf.Clamp01(comboIndex / 4f));
            RequestScreenCue("Player.BasicHit", hitColor, 0.14f, scale, ScreenCueCategory.Player);
        }

        private void HandleDodgeStarted()
        {
            RequestScreenCue("Player.Dodge", dodgeColor, 0.18f, 0.78f, ScreenCueCategory.Player);
        }

        private void HandlePlayerDamaged(DamageInfo damageInfo)
        {
            playerDamageCueRequestCount++;
            float healthScale = playerHealth != null && playerHealth.MaxHealth > 0f
                ? Mathf.Clamp01(damageInfo.Amount / playerHealth.MaxHealth)
                : 0f;
            RequestScreenCue(
                "Player.Damaged",
                damagedColor,
                0.20f,
                0.74f + healthScale * 0.42f,
                ScreenCueCategory.Player);
            RequestDamageFeedback(damageInfo, healthScale);
        }

        private void HandleEnergyRiskBandChanged(SummonEnergyRiskBand riskBand)
        {
            lastEnergyRiskBand = riskBand;
            if (riskBand != SummonEnergyRiskBand.ForwardRisk)
            {
                return;
            }

            energyCueRequestCount++;
            forwardRiskCueRequestCount++;
            RequestScreenCue("Energy.ForwardRisk", forwardRiskColor, 0.18f, 0.58f, ScreenCueCategory.Player);
        }

        private void HandleEnergyTierAvailable(int tier)
        {
            int safeTier = Mathf.Clamp(tier, 1, 3);
            lastEnergyCueTier = safeTier;
            energyCueRequestCount++;
            energyReadyCueRequestCount++;
            RequestScreenCue(
                $"Energy.ReadyLV{safeTier}",
                energyReadyColor,
                0.18f,
                ResolveTierIntensity(safeTier, 0.68f),
                ScreenCueCategory.Player);
        }

        private void HandleEnergySpent(int tier)
        {
            int safeTier = Mathf.Clamp(tier, 1, 3);
            lastEnergyCueTier = safeTier;
            energyCueRequestCount++;
            energySpendCueRequestCount++;
            RequestScreenCue(
                $"Energy.SpentLV{safeTier}",
                energySpendColor,
                0.12f,
                0.42f,
                ScreenCueCategory.Player);
        }

        private void HandleSkill1Used(int tier)
        {
            RequestScreenCue("Player.Skill1", skillColor, 0.16f, ResolveTierIntensity(tier, 0.78f), ScreenCueCategory.Player);
        }

        private void HandleSummonSlot1Used(int tier)
        {
            RequestScreenCue("Player.SummonSlot1", summonColor, 0.22f, ResolveTierIntensity(tier, 0.82f), ScreenCueCategory.Player);
        }

        private void HandleSupportSummonUsed(PlayerSupportSummonSlotAction action, int tier)
        {
            RequestScreenCue("Player.SupportSummon", summonColor, 0.18f, ResolveTierIntensity(tier, 0.68f), ScreenCueCategory.Player);
        }

        private void HandleSummonPressureBlocked(int tier)
        {
            RequestScreenCue("Player.SummonBlock", summonBlockColor, 0.16f, ResolveTierIntensity(tier, 0.95f), ScreenCueCategory.Player);
        }

        private void HandleSupportSummonPressureBlocked(PlayerSupportSummonSlotAction action, int tier)
        {
            RequestScreenCue("Player.SupportSummonBlock", summonBlockColor, 0.16f, ResolveTierIntensity(tier, 0.82f), ScreenCueCategory.Player);
        }

        private void HandleBossWindupStarted(BossBarrageEmitter emitter, BossBarragePatternProfile pattern)
        {
            RequestScreenCue("Boss.Windup", bossWindupColor, 0.18f, ResolveProjectileIntensity(pattern), ScreenCueCategory.Boss);
        }

        private void HandleBossWaveFired(BossBarrageEmitter emitter, BossBarragePatternProfile pattern, int spawnedCount)
        {
            RequestScreenCue("Boss.Fire", bossFireColor, 0.16f, ResolveProjectileIntensity(pattern, spawnedCount), ScreenCueCategory.Boss);
        }

        private void HandleBossPressureActionQueued(
            BossPressureActionDirector director,
            BossPressureActionKind actionKind,
            BossBarragePatternProfile pattern,
            int spentTier)
        {
            Color color = actionKind == BossPressureActionKind.PunishOverextend
                ? bossFireColor
                : bossPressureColor;
            RequestScreenCue(
                $"BossPressure.{actionKind}",
                color,
                0.20f,
                ResolveTierIntensity(spentTier, 0.78f),
                ScreenCueCategory.Boss);
        }

        private void HandleStageBeatChanged(int beatIndex)
        {
            string beatId = ResolveFrontlineBeatId(beatIndex, out string beatLabel);
            lastFrontlineBeatIndex = beatIndex;
            lastFrontlineBeatLabel = beatLabel;
            RequestScreenCue(
                $"FrontlineBeat.{beatId}",
                frontlineBeatColor,
                0.20f,
                0.58f,
                ScreenCueCategory.Frontline);
        }

        private void HandleRouteStabilityBandChanged(
            BossBarragePocketReviewOwner.RouteStabilityBand band,
            float stability01)
        {
            lastFrontlineStabilityBand = band;
            lastFrontlineStability01 = Mathf.Clamp01(stability01);
            if (band == BossBarragePocketReviewOwner.RouteStabilityBand.Stable)
            {
                return;
            }

            frontlineStabilityCueRequestCount++;
            Color color = band == BossBarragePocketReviewOwner.RouteStabilityBand.Critical
                ? pocketFailColor
                : frontlineWarningColor;
            float intensity = band == BossBarragePocketReviewOwner.RouteStabilityBand.Critical ? 0.96f : 0.68f;
            float duration = band == BossBarragePocketReviewOwner.RouteStabilityBand.Critical ? 0.36f : 0.24f;
            RequestScreenCue(
                $"FrontlineStability.{band}",
                color,
                duration,
                intensity,
                ScreenCueCategory.Frontline);
        }

        private void HandleSummonBlockOpportunityOpened()
        {
            RequestScreenCue("Followup.BlockOpportunity", followupWindowColor, 0.20f, 0.72f, ScreenCueCategory.Followup);
        }

        private void HandleSummonFollowupWindowOpened(int tier)
        {
            RequestScreenCue("Followup.Window", followupWindowColor, 0.24f, ResolveTierIntensity(tier, 0.82f), ScreenCueCategory.Followup);
        }

        private void HandleSummonFollowupHitConfirmed(int tier, float damage)
        {
            float damageWeight = Mathf.Clamp01(damage / 120f);
            RequestScreenCue(
                "Followup.Hit",
                followupHitColor,
                0.30f,
                ResolveTierIntensity(tier, 1.08f) + damageWeight * 0.22f,
                ScreenCueCategory.Followup);
        }

        private void HandleSummonFollowupMissed()
        {
            RequestScreenCue("Followup.Missed", followupMissedColor, 0.18f, 0.52f, ScreenCueCategory.Followup);
        }

        private void HandlePocketCleared()
        {
            RequestScreenCue("Pocket.Cleared", pocketClearColor, 0.78f, 0.92f, ScreenCueCategory.Result);
        }

        private void HandlePocketFailed()
        {
            RequestScreenCue("Pocket.Failed", pocketFailColor, 0.86f, 1.02f, ScreenCueCategory.Result);
        }

        private void RequestScreenCue(
            string cueId,
            Color cueColor,
            float durationSeconds,
            float intensity,
            ScreenCueCategory category)
        {
            float safeDuration = Mathf.Max(0.01f, durationSeconds);
            float safeIntensity = Mathf.Clamp(intensity, 0f, 1.6f);
            if (ShouldSuppressScreenCue(category))
            {
                suppressedCueRequestCount++;
                return;
            }

            activeFlashColor = cueColor;
            activeVignetteColor = cueColor;
            activeIntensity = safeIntensity;
            activeCategory = category;
            flashDuration = Mathf.Max(0.01f, safeDuration * 0.58f);
            flashTimer = flashDuration;
            vignetteDuration = safeDuration;
            vignetteTimer = safeDuration;
            cueRequestCount++;
            lastCueId = cueId;
            lastCueColor = cueColor;
            lastCueIntensity = safeIntensity;

            switch (category)
            {
                case ScreenCueCategory.Result:
                    resultCueRequestCount++;
                    break;
                case ScreenCueCategory.Frontline:
                    frontlineCueRequestCount++;
                    break;
                case ScreenCueCategory.Boss:
                    bossCueRequestCount++;
                    break;
                case ScreenCueCategory.Followup:
                    followupCueRequestCount++;
                    break;
                default:
                    playerCueRequestCount++;
                    break;
            }
        }

        private void RequestDamageFeedback(DamageInfo damageInfo, float healthScale)
        {
            if (!useDamageScreenFeedback)
            {
                return;
            }

            if (HasActiveCue && activeCategory == ScreenCueCategory.Result)
            {
                return;
            }

            float safeHealthScale = Mathf.Clamp01(healthScale);
            float heavyDamage01 = Mathf.Clamp01(safeHealthScale / heavyDamageHealthRatio);
            float healthDanger01 = playerHealth != null
                ? Mathf.Clamp01((criticalHealthThreshold - playerHealth.HealthRatio) / Mathf.Max(0.01f, criticalHealthThreshold))
                : 0f;
            float safeDuration = damageVignetteSeconds + heavyDamageExtraSeconds * Mathf.Max(heavyDamage01, healthDanger01);
            damageFeedbackDuration = Mathf.Max(0.01f, safeDuration);
            damageFeedbackTimer = damageFeedbackDuration;
            damageFeedbackIntensity = Mathf.Clamp01(0.54f + heavyDamage01 * 0.30f + healthDanger01 * 0.24f);
            damageScreenDirection = ResolveDamageScreenDirection(damageInfo.Direction);
            criticalHealthPulseTimer = Mathf.Max(criticalHealthPulseTimer, criticalHealthPulseSeconds * healthDanger01);
            damageFeedbackRequestCount++;
            lastDamageFeedbackIntensity = damageFeedbackIntensity;
            lastDamageFeedbackDuration = damageFeedbackDuration;
            lastDamageScreenDirection = damageScreenDirection;
        }

        private bool ShouldSuppressScreenCue(ScreenCueCategory category)
        {
            if (!HasActiveCue)
            {
                return false;
            }

            return ResolveCuePriority(category) < ResolveCuePriority(activeCategory);
        }

        private static int ResolveCuePriority(ScreenCueCategory category)
        {
            switch (category)
            {
                case ScreenCueCategory.Result:
                    return 5;
                case ScreenCueCategory.Followup:
                    return 4;
                case ScreenCueCategory.Frontline:
                    return 3;
                case ScreenCueCategory.Boss:
                    return 2;
                default:
                    return 1;
            }
        }

        private string ResolveFrontlineBeatId(int beatIndex, out string beatLabel)
        {
            beatLabel = $"Beat {beatIndex + 1}";
            if (pocketReviewOwner == null || pocketReviewOwner.StageProfile == null)
            {
                return $"B{Mathf.Max(0, beatIndex)}";
            }

            int beatCount = pocketReviewOwner.StageProfile.BeatCount;
            if (beatCount <= 0)
            {
                return $"B{Mathf.Max(0, beatIndex)}";
            }

            int safeBeatIndex = Mathf.Clamp(beatIndex, 0, beatCount - 1);
            var beat = pocketReviewOwner.StageProfile.GetBeat(safeBeatIndex);
            beatLabel = string.IsNullOrWhiteSpace(beat.Label) ? beatLabel : beat.Label;
            return string.IsNullOrWhiteSpace(beat.BeatId) ? beatLabel : beat.BeatId;
        }

        private void Subscribe()
        {
            if (subscribed)
            {
                return;
            }

            if (actionController != null)
            {
                actionController.BasicAttackStarted += HandleBasicAttackStarted;
                actionController.BasicAttackHit += HandleBasicAttackHit;
                actionController.DodgeStarted += HandleDodgeStarted;
            }

            if (playerHealth != null)
            {
                playerHealth.Damaged += HandlePlayerDamaged;
            }

            if (energyLadder != null)
            {
                energyLadder.RiskBandChanged += HandleEnergyRiskBandChanged;
                energyLadder.TierAvailable += HandleEnergyTierAvailable;
                energyLadder.EnergySpent += HandleEnergySpent;
            }

            if (skill1Action != null)
            {
                skill1Action.Skill1Used += HandleSkill1Used;
            }

            if (summonSlot1Action != null)
            {
                summonSlot1Action.SummonSlot1Used += HandleSummonSlot1Used;
                summonSlot1Action.SummonPressureBlocked += HandleSummonPressureBlocked;
            }

            SubscribeSupportSummon(summonSlot2Action);
            SubscribeSupportSummon(summonSlot3Action);

            if (bossBarrageEmitter != null)
            {
                bossBarrageEmitter.WindupStarted += HandleBossWindupStarted;
                bossBarrageEmitter.WaveFired += HandleBossWaveFired;
            }

            if (bossPressureActionDirector != null)
            {
                bossPressureActionDirector.ActionQueued += HandleBossPressureActionQueued;
            }

            if (pocketReviewOwner != null)
            {
                pocketReviewOwner.StageBeatChanged += HandleStageBeatChanged;
                pocketReviewOwner.RouteStabilityBandChanged += HandleRouteStabilityBandChanged;
                pocketReviewOwner.SummonBlockOpportunityOpened += HandleSummonBlockOpportunityOpened;
                pocketReviewOwner.SummonFollowupWindowOpened += HandleSummonFollowupWindowOpened;
                pocketReviewOwner.SummonFollowupHitConfirmed += HandleSummonFollowupHitConfirmed;
                pocketReviewOwner.SummonFollowupMissed += HandleSummonFollowupMissed;
                pocketReviewOwner.PocketCleared += HandlePocketCleared;
                pocketReviewOwner.PocketFailed += HandlePocketFailed;
            }

            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed)
            {
                return;
            }

            if (actionController != null)
            {
                actionController.BasicAttackStarted -= HandleBasicAttackStarted;
                actionController.BasicAttackHit -= HandleBasicAttackHit;
                actionController.DodgeStarted -= HandleDodgeStarted;
            }

            if (playerHealth != null)
            {
                playerHealth.Damaged -= HandlePlayerDamaged;
            }

            if (energyLadder != null)
            {
                energyLadder.RiskBandChanged -= HandleEnergyRiskBandChanged;
                energyLadder.TierAvailable -= HandleEnergyTierAvailable;
                energyLadder.EnergySpent -= HandleEnergySpent;
            }

            if (skill1Action != null)
            {
                skill1Action.Skill1Used -= HandleSkill1Used;
            }

            if (summonSlot1Action != null)
            {
                summonSlot1Action.SummonSlot1Used -= HandleSummonSlot1Used;
                summonSlot1Action.SummonPressureBlocked -= HandleSummonPressureBlocked;
            }

            UnsubscribeSupportSummon(summonSlot2Action);
            UnsubscribeSupportSummon(summonSlot3Action);

            if (bossBarrageEmitter != null)
            {
                bossBarrageEmitter.WindupStarted -= HandleBossWindupStarted;
                bossBarrageEmitter.WaveFired -= HandleBossWaveFired;
            }

            if (bossPressureActionDirector != null)
            {
                bossPressureActionDirector.ActionQueued -= HandleBossPressureActionQueued;
            }

            if (pocketReviewOwner != null)
            {
                pocketReviewOwner.StageBeatChanged -= HandleStageBeatChanged;
                pocketReviewOwner.RouteStabilityBandChanged -= HandleRouteStabilityBandChanged;
                pocketReviewOwner.SummonBlockOpportunityOpened -= HandleSummonBlockOpportunityOpened;
                pocketReviewOwner.SummonFollowupWindowOpened -= HandleSummonFollowupWindowOpened;
                pocketReviewOwner.SummonFollowupHitConfirmed -= HandleSummonFollowupHitConfirmed;
                pocketReviewOwner.SummonFollowupMissed -= HandleSummonFollowupMissed;
                pocketReviewOwner.PocketCleared -= HandlePocketCleared;
                pocketReviewOwner.PocketFailed -= HandlePocketFailed;
            }

            subscribed = false;
        }

        private void SubscribeSupportSummon(PlayerSupportSummonSlotAction action)
        {
            if (action == null)
            {
                return;
            }

            action.SummonUsed += HandleSupportSummonUsed;
            action.SummonPressureBlocked += HandleSupportSummonPressureBlocked;
        }

        private void UnsubscribeSupportSummon(PlayerSupportSummonSlotAction action)
        {
            if (action == null)
            {
                return;
            }

            action.SummonUsed -= HandleSupportSummonUsed;
            action.SummonPressureBlocked -= HandleSupportSummonPressureBlocked;
        }

        private void DrawVignette(Color color)
        {
            float thickness = Mathf.Min(edgeThickness, Mathf.Min(Screen.width, Screen.height) * 0.45f);
            DrawRect(new Rect(0f, 0f, Screen.width, thickness), color);
            DrawRect(new Rect(0f, Screen.height - thickness, Screen.width, thickness), color);
            DrawRect(new Rect(0f, 0f, thickness, Screen.height), color);
            DrawRect(new Rect(Screen.width - thickness, 0f, thickness, Screen.height), color);
        }

        private void DrawDamageFeedback()
        {
            if (!useDamageScreenFeedback)
            {
                return;
            }

            float damageAlpha = 0f;
            if (damageFeedbackTimer > 0f)
            {
                float fade01 = ResolveDamageFade01(damageFeedbackTimer, damageFeedbackDuration);
                damageAlpha = maxDamageVignetteAlpha * damageFeedbackIntensity * fade01;
                float flashAlpha = maxDamageFlashAlpha * damageFeedbackIntensity * ResolveDamageFlash01(damageFeedbackTimer, damageFeedbackDuration);
                DrawRect(new Rect(0f, 0f, Screen.width, Screen.height), WithAlpha(damagedColor, flashAlpha));
                DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), EnsureDamageVignetteTexture(), WithAlpha(damagedColor, damageAlpha));
                DrawDamageDirectionAccent(damageAlpha);
            }

            if (criticalHealthPulseTimer > 0f && criticalHealthPulseAlpha > 0f)
            {
                float pulseFade = ResolveFade01(criticalHealthPulseTimer, criticalHealthPulseSeconds);
                float wave = 0.55f + Mathf.Sin(Time.unscaledTime * criticalHealthPulseRate * Mathf.PI * 2f) * 0.45f;
                float pulseAlpha = criticalHealthPulseAlpha * pulseFade * Mathf.Clamp01(wave);
                DrawTexture(
                    new Rect(0f, 0f, Screen.width, Screen.height),
                    EnsureDamageVignetteTexture(),
                    WithAlpha(damagedColor, pulseAlpha));
            }
        }

        private void DrawDamageDirectionAccent(float baseAlpha)
        {
            if (damageDirectionAccentAlpha <= 0f || damageDirectionAccentThickness <= 0f)
            {
                return;
            }

            Vector2 direction = damageScreenDirection;
            if (direction.sqrMagnitude <= 0.01f)
            {
                return;
            }

            float thickness = Mathf.Min(damageDirectionAccentThickness, Mathf.Min(Screen.width, Screen.height) * 0.38f);
            float alpha = Mathf.Min(damageDirectionAccentAlpha, baseAlpha * 0.72f);
            Color color = WithAlpha(damagedColor, alpha);
            if (direction.x < -0.12f)
            {
                DrawRect(new Rect(0f, 0f, thickness * Mathf.Abs(direction.x), Screen.height), color);
            }
            else if (direction.x > 0.12f)
            {
                float width = thickness * direction.x;
                DrawRect(new Rect(Screen.width - width, 0f, width, Screen.height), color);
            }

            if (direction.y > 0.12f)
            {
                DrawRect(new Rect(0f, 0f, Screen.width, thickness * direction.y), color);
            }
            else if (direction.y < -0.12f)
            {
                float height = thickness * Mathf.Abs(direction.y);
                DrawRect(new Rect(0f, Screen.height - height, Screen.width, height), color);
            }
        }

        private static void DrawRect(Rect rect, Color color)
        {
            Color previousColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previousColor;
        }

        private static void DrawTexture(Rect rect, Texture2D texture, Color color)
        {
            Color previousColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, texture);
            GUI.color = previousColor;
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            color.a = Mathf.Clamp01(alpha);
            return color;
        }

        private static float ResolveFade01(float timer, float duration)
        {
            if (duration <= 0f)
            {
                return 0f;
            }

            float t = Mathf.Clamp01(timer / duration);
            return t * t * (3f - 2f * t);
        }

        private static float ResolveDamageFade01(float timer, float duration)
        {
            if (duration <= 0f)
            {
                return 0f;
            }

            float t = Mathf.Clamp01(timer / duration);
            return t * t * (3f - 2f * t);
        }

        private static float ResolveDamageFlash01(float timer, float duration)
        {
            if (duration <= 0f)
            {
                return 0f;
            }

            float age01 = 1f - Mathf.Clamp01(timer / duration);
            return Mathf.Clamp01(1f - age01 * 4.2f);
        }

        private static Vector2 ResolveDamageScreenDirection(Vector3 damageDirection)
        {
            if (damageDirection.sqrMagnitude <= 0.0001f)
            {
                return Vector2.zero;
            }

            Vector3 normalized = damageDirection.normalized;
            Vector2 screenDirection = new Vector2(normalized.x, -normalized.z);
            return screenDirection.sqrMagnitude > 0.0001f ? screenDirection.normalized : Vector2.zero;
        }

        private static float ResolveTierIntensity(int tier, float baseIntensity)
        {
            float tierWeight = Mathf.Clamp01((Mathf.Max(1, tier) - 1) / 2f);
            return baseIntensity + tierWeight * 0.35f;
        }

        private static float ResolveProjectileIntensity(BossBarragePatternProfile pattern, int spawnedCount = 0)
        {
            int count = Mathf.Max(spawnedCount, pattern != null ? pattern.ProjectilesPerWave : 1);
            return 0.58f + Mathf.Clamp01((count - 1) / 6f) * 0.38f;
        }

        private Texture2D EnsureDamageVignetteTexture()
        {
            if (damageVignetteTexture != null)
            {
                return damageVignetteTexture;
            }

            const int size = 96;
            damageVignetteTexture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                hideFlags = HideFlags.HideAndDontSave,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            Color[] pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float u = (x + 0.5f) / size * 2f - 1f;
                    float v = (y + 0.5f) / size * 2f - 1f;
                    float distance = Mathf.Sqrt(u * u + v * v);
                    float edge01 = Mathf.Clamp01((distance - 0.46f) / 0.52f);
                    float alpha = edge01 * edge01 * (3f - 2f * edge01);
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            damageVignetteTexture.SetPixels(pixels);
            damageVignetteTexture.Apply(false, true);
            return damageVignetteTexture;
        }
    }
}
