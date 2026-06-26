using DimensionBrawl.Combat;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Player;
using DimensionBrawl.Test;
using DimensionBrawl.UI;
using UnityEngine;

namespace DimensionBrawl.Presentation
{
    // Review-only readout for the boss barrage lane slice; production HUD should be authored separately.
    public sealed class BossBarrageLaneReviewHud : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CombatHealth playerHealth;
        [SerializeField] private CombatHealth closeThreatHealth;
        [SerializeField] private CombatHealth bossHealth;
        [SerializeField] private SummonEnergyLadder energyLadder;
        [SerializeField] private SummonLaneSpace laneSpace;
        [SerializeField] private Transform player;
        [SerializeField] private PlayerCombatModeController combatModeController;
        [SerializeField] private PlayerRangedAimController rangedAimController;
        [SerializeField] private PlayerRangedBasicAttackAction rangedBasicAttackAction;
        [SerializeField] private PlayerSkill1Action skill1Action;
        [SerializeField] private PlayerSummonSlot1Action summonSlot1Action;
        [SerializeField] private PlayerSupportSummonSlotAction summonSlot2Action;
        [SerializeField] private PlayerSupportSummonSlotAction summonSlot3Action;
        [SerializeField] private BossBarrageEmitter bossBarrageEmitter;
        [SerializeField] private BossBasicFireEmitter bossBasicFireEmitter;
        [SerializeField] private BossPressureCostLadder bossPressureCostLadder;
        [SerializeField] private BossPressurePositionController bossPressurePositionController;
        [SerializeField] private BossPressureActionDirector bossPressureActionDirector;
        [SerializeField] private BossSummonPressureAction bossSummonPressureAction;
        [SerializeField] private BossBarragePocketReviewOwner pocketReviewOwner;
        [SerializeField] private BossSummonDuelReviewOwner duelReviewOwner;

        [Header("Frontline Stage Review")]
        [SerializeField] private FrontlineWaveStageProfile stageProfile;

        [Header("Display")]
        [SerializeField] private bool showHud = true;
        [SerializeField] private bool showDetailedTelemetry;
        [SerializeField, Min(1f)] private float width = 390f;
        [SerializeField, Min(1f)] private float height = 230f;
        [SerializeField, Min(0f)] private float margin = 18f;
        [SerializeField] private bool showCenterReticle;
        [SerializeField] private bool usePremiumCompactHud = true;
        [SerializeField] private string stageEpisodeLabel = "EP 03 Rift Stabilization";
        [SerializeField] private string objectiveBadgeLabel = "LANE";
        [SerializeField] private string bossDisplayName = "Dimensional Rift Guardian";
        [SerializeField] private string playerDisplayName = "Player";

        [Header("Result Banner")]
        [SerializeField] private bool showResultBanner = true;
        [SerializeField, Min(1f)] private float resultBannerWidth = 540f;
        [SerializeField, Min(1f)] private float resultBannerHeight = 82f;
        [SerializeField, Min(0f)] private float resultBannerBottomOffset = 112f;
        [SerializeField] private Color resultClearBackColor = new Color(0.08f, 0.54f, 0.28f, 0.82f);
        [SerializeField] private Color resultFailBackColor = new Color(0.66f, 0.08f, 0.08f, 0.86f);
        [SerializeField] private Color resultBannerTextColor = Color.white;

        [Header("Resource Bars")]
        [SerializeField] private bool showResourceBars = true;
        [SerializeField, Min(8f)] private float resourceBarHeight = 18f;
        [SerializeField, Min(0f)] private float resourceBarGap = 3f;
        [SerializeField] private Color resourceBarBackColor = new Color(0.02f, 0.025f, 0.035f, 0.82f);
        [SerializeField] private Color playerHealthColor = new Color(0.25f, 1f, 0.46f, 1f);
        [SerializeField] private Color bossHealthColor = new Color(1f, 0.22f, 0.32f, 1f);
        [SerializeField] private Color threatHealthColor = new Color(1f, 0.76f, 0.24f, 1f);
        [SerializeField] private Color resourceTextColor = Color.white;
        [SerializeField] private Color resourceReadyTextColor = new Color(0.84f, 1f, 0.42f, 1f);

        private GUIStyle labelStyle;
        private GUIStyle titleStyle;
        private GUIStyle boxStyle;
        private GUIStyle resourceBarStyle;
        private GUIStyle resultBannerTitleStyle;
        private GUIStyle resultBannerDetailStyle;

        public string FrontlineLoopReadout => ResolveFrontlineLoopLine();
        public string FrontlineTuningReadout => ResolveFrontlineTuningLine();
        public string StageBriefingReadout => ResolveStageBriefingLine();
        public string CompactStageBriefingReadout => ResolveCompactStageBriefingLine();
        public string StageBeatReadout => ResolveStageBeatLine();
        public string PressureSlotReadout => ResolvePressureSlotLine();
        public string RouteRecordReadout => ResolveRouteRecordLine();
        public string RouteStabilityReadout => ResolveRouteStabilityLine();
        public string BossPressureReadout => ResolveBossPressureLine();
        public string BossPressureResponseReadout => ResolveBossPressureResponseLine();
        public bool ShowDetailedTelemetry => showDetailedTelemetry;
        public string CombatCueReadout => ResolveCombatCueLine();
        public string FrontlineCueReadout => ResolveFrontlineCueLine();
        public string CompactObjectiveReadout => ResolveCompactObjectiveLine();
        public string RouteIncentiveReadout => ResolveCompactRouteIncentiveLine();
        public string CompactCombatCueReadout => ResolveCompactCombatCueLine();
        public string CompactFrontlineCueReadout => ResolveCompactFrontlineCueLine();
        public bool ShouldShowResultBanner => TryResolveResultBanner(out _, out _, out _);
        public string ResultBannerTitle => TryResolveResultBanner(out string title, out _, out _) ? title : string.Empty;
        public string ResultBannerDetail => TryResolveResultBanner(out _, out string detail, out _) ? detail : string.Empty;
        public FrontlineWaveStageProfile StageProfileForReview => stageProfile;
        private FrontlineWaveStageProfile ActiveStageProfile =>
            stageProfile != null ? stageProfile : pocketReviewOwner != null ? pocketReviewOwner.StageProfile : null;

        public readonly struct PremiumHudLayout
        {
            public PremiumHudLayout(Rect objectiveRect, Rect bossBarRect, Rect playerPanelRect, bool isStacked)
            {
                ObjectiveRect = objectiveRect;
                BossBarRect = bossBarRect;
                PlayerPanelRect = playerPanelRect;
                IsStacked = isStacked;
            }

            public Rect ObjectiveRect { get; }
            public Rect BossBarRect { get; }
            public Rect PlayerPanelRect { get; }
            public bool IsStacked { get; }
        }

#if UNITY_EDITOR
        public void AssignStageProfileForReview(FrontlineWaveStageProfile newStageProfile)
        {
            stageProfile = newStageProfile;
        }
#endif

        public void SetDetailedTelemetryVisible(bool visible)
        {
            showDetailedTelemetry = visible;
        }

        public void Configure(
            CombatHealth newPlayerHealth,
            CombatHealth newCloseThreatHealth,
            CombatHealth newBossHealth,
            SummonEnergyLadder newEnergyLadder,
            SummonLaneSpace newLaneSpace,
            Transform newPlayer,
            PlayerCombatModeController newCombatModeController,
            PlayerRangedAimController newRangedAimController,
            PlayerRangedBasicAttackAction newRangedBasicAttackAction,
            PlayerSkill1Action newSkill1Action,
            PlayerSummonSlot1Action newSummonSlot1Action,
            BossBarrageEmitter newBossBarrageEmitter,
            BossBarragePocketReviewOwner newPocketReviewOwner,
            BossPressureCostLadder newBossPressureCostLadder = null,
            BossPressurePositionController newBossPressurePositionController = null,
            BossPressureActionDirector newBossPressureActionDirector = null,
            BossSummonPressureAction newBossSummonPressureAction = null,
            PlayerSupportSummonSlotAction newSummonSlot2Action = null,
            PlayerSupportSummonSlotAction newSummonSlot3Action = null,
            BossBasicFireEmitter newBossBasicFireEmitter = null)
        {
            playerHealth = newPlayerHealth;
            closeThreatHealth = newCloseThreatHealth;
            bossHealth = newBossHealth;
            energyLadder = newEnergyLadder;
            laneSpace = newLaneSpace;
            player = newPlayer;
            combatModeController = newCombatModeController;
            rangedAimController = newRangedAimController;
            rangedBasicAttackAction = newRangedBasicAttackAction;
            skill1Action = newSkill1Action;
            summonSlot1Action = newSummonSlot1Action;
            summonSlot2Action = newSummonSlot2Action;
            summonSlot3Action = newSummonSlot3Action;
            bossBarrageEmitter = newBossBarrageEmitter;
            bossBasicFireEmitter = newBossBasicFireEmitter;
            bossPressureCostLadder = newBossPressureCostLadder;
            bossPressurePositionController = newBossPressurePositionController;
            bossPressureActionDirector = newBossPressureActionDirector;
            bossSummonPressureAction = newBossSummonPressureAction;
            pocketReviewOwner = newPocketReviewOwner;
            duelReviewOwner = null;
        }

        private void OnGUI()
        {
            if (!showHud)
            {
                return;
            }

            GUI.depth = -1000;
            Matrix4x4 previousMatrix = GUI.matrix;
            float uiScale = ResolveUiScale();
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(uiScale, uiScale, 1f));
            EnsureStyles();
            if (showDetailedTelemetry || !usePremiumCompactHud)
            {
                float areaHeight = ResolveHudAreaHeight(uiScale);
                GUILayout.BeginArea(new Rect(margin, margin, width, areaHeight), boxStyle);
                GUILayout.Label("Boss Barrage", titleStyle);
                DrawCombatResourceBars();
                if (showDetailedTelemetry)
                {
                    DrawDetailedTelemetry();
                }
                else
                {
                    DrawCompactCombatCues();
                }

                GUILayout.EndArea();
            }
            else
            {
                DrawPremiumCompactHud(uiScale);
            }

            DrawResultBanner(uiScale);
            GUI.matrix = previousMatrix;
            DrawReticleIfNeeded();
        }

        private void DrawPremiumCompactHud(float uiScale)
        {
            float screenWidth = Screen.width / uiScale;
            float screenHeight = Screen.height / uiScale;
            PremiumHudLayout layout = ResolvePremiumHudLayoutForReview(screenWidth, screenHeight, margin);

            BossBarrageLaneReviewHudChrome.DrawObjectivePanel(
                layout.ObjectiveRect,
                ResolveStageEpisodeLabel(),
                ResolveCompactObjectiveLine(),
                ResolvePremiumObjectiveBadge(),
                ResolveCompactStageBriefingLine(),
                ResolveCompactRouteIncentiveLine());

            BossBarrageLaneReviewHudChrome.DrawBossBar(
                layout.BossBarRect,
                bossDisplayName,
                ResolveCompactCombatCueLine(),
                ResolveHealthFill01(bossHealth),
                ResolveBossCostFill01());

            BossBarrageLaneReviewHudChrome.DrawPlayerResourcePanel(
                layout.PlayerPanelRect,
                playerDisplayName,
                ResolveHealthValueText(playerHealth),
                ResolveHealthFill01(playerHealth),
                ResolveEnergyValueText(),
                ResolveEnergyFill01(),
                energyLadder != null && energyLadder.CanSpend);
        }

        public static PremiumHudLayout ResolvePremiumHudLayoutForReview(
            float screenWidth,
            float screenHeight,
            float margin)
        {
            float resolvedWidth = Mathf.Max(320f, screenWidth);
            float resolvedHeight = Mathf.Max(320f, screenHeight);
            float resolvedMargin = Mathf.Clamp(margin, 8f, 28f);
            float usableWidth = Mathf.Max(280f, resolvedWidth - resolvedMargin * 2f);

            Rect objectiveRect = new Rect(
                resolvedMargin,
                resolvedMargin,
                Mathf.Min(430f, Mathf.Max(300f, usableWidth * 0.32f)),
                124f);

            float rightHudReserve = Mathf.Clamp(resolvedWidth * 0.18f, 170f, 300f);
            float bossBarLeftLimit = objectiveRect.xMax + 44f;
            float bossBarRightLimit = resolvedWidth - resolvedMargin - rightHudReserve;
            bool shouldStackTopPanels = resolvedWidth < 720f || bossBarRightLimit - bossBarLeftLimit < 340f;
            Rect bossBarRect;
            if (shouldStackTopPanels)
            {
                objectiveRect = new Rect(resolvedMargin, resolvedMargin, usableWidth, 124f);
                bossBarRect = new Rect(
                    resolvedMargin,
                    objectiveRect.yMax + 10f,
                    usableWidth,
                    82f);
            }
            else
            {
                float bossBarMaxWidth = Mathf.Max(340f, bossBarRightLimit - bossBarLeftLimit);
                float bossBarWidth = Mathf.Min(Mathf.Clamp(resolvedWidth * 0.38f, 420f, 760f), bossBarMaxWidth);
                float bossBarX = Mathf.Max((resolvedWidth - bossBarWidth) * 0.5f, bossBarLeftLimit);
                bossBarX = Mathf.Min(bossBarX, resolvedWidth - resolvedMargin - bossBarWidth);
                bossBarRect = new Rect(
                    bossBarX,
                    resolvedMargin + 6f,
                    bossBarWidth,
                    86f);
            }

            float playerPanelWidth = shouldStackTopPanels
                ? usableWidth
                : Mathf.Min(Mathf.Clamp(resolvedWidth * 0.34f, 430f, 620f), usableWidth);
            float playerPanelHeight = 82f;
            Rect playerPanelRect = new Rect(
                (resolvedWidth - playerPanelWidth) * 0.5f,
                Mathf.Max(bossBarRect.yMax + 12f, resolvedHeight - 120f),
                playerPanelWidth,
                playerPanelHeight);
            if (playerPanelRect.yMax > resolvedHeight - resolvedMargin)
            {
                playerPanelRect.y = Mathf.Max(bossBarRect.yMax + 12f, resolvedHeight - resolvedMargin - playerPanelHeight);
            }

            return new PremiumHudLayout(objectiveRect, bossBarRect, playerPanelRect, shouldStackTopPanels);
        }

        private void DrawDetailedTelemetry()
        {
            GUILayout.Label(ResolveHealthLine(), labelStyle);
            GUILayout.Label(ResolvePhaseLine(), labelStyle);
            GUILayout.Label(ResolveEnergyLine(), labelStyle);
            GUILayout.Label(ResolveRiskLine(), labelStyle);
            GUILayout.Label(ResolveBossBasicFireLine(), labelStyle);
            GUILayout.Label(ResolveBossBarrageLine(), labelStyle);
            GUILayout.Label(ResolveBossPressureLine(), labelStyle);
            GUILayout.Label(ResolveBossPressureResponseLine(), labelStyle);
            GUILayout.Label(ResolveBossSummonLine(), labelStyle);
            GUILayout.Label(ResolveStageBriefingLine(), labelStyle);
            GUILayout.Label(ResolveStageBeatLine(), labelStyle);
            GUILayout.Label(ResolvePressureSlotLine(), labelStyle);
            GUILayout.Label(ResolveRouteStabilityLine(), labelStyle);
            GUILayout.Label(ResolveRouteRecordLine(), labelStyle);
            GUILayout.Label(ResolveFrontlineLoopLine(), labelStyle);
            GUILayout.Label(ResolveFrontlineTuningLine(), labelStyle);
            GUILayout.Label(ResolveWeaponModeLine(), labelStyle);
            GUILayout.Label(ResolveRangedFireLine(), labelStyle);
            GUILayout.Label(ResolveActionLine(), labelStyle);
            GUILayout.Label(ResolveActionHintLine(), labelStyle);
            GUILayout.Label(ResolveSummonExchangeLine(), labelStyle);
            string duelLine = ResolveDuelProgressLine();
            if (!string.IsNullOrEmpty(duelLine))
            {
                GUILayout.Label(duelLine, labelStyle);
            }

            GUILayout.Label(ResolveObjectiveLine(), labelStyle);
        }

        private void DrawCompactCombatCues()
        {
            GUILayout.Label(ResolveCompactObjectiveLine(), labelStyle);
            GUILayout.Label(ResolveCompactStageBeatLine(), labelStyle);
            GUILayout.Label(ResolveCompactRouteIncentiveLine(), labelStyle);
            GUILayout.Label(ResolveCompactPhaseLine(), labelStyle);
            GUILayout.Label(ResolveCompactCombatCueLine(), labelStyle);
            GUILayout.Label(ResolveCompactFrontlineCueLine(), labelStyle);

            string hintLine = ResolveActionHintLine();
            if (!BossBarrageLaneReviewHudText.IsEmptyHintLine(hintLine))
            {
                GUILayout.Label(hintLine, labelStyle);
            }

            string duelLine = ResolveDuelProgressLine();
            if (!string.IsNullOrEmpty(duelLine))
            {
                GUILayout.Label(duelLine, labelStyle);
            }
        }

        private string ResolvePremiumObjectiveBadge()
        {
            if (energyLadder == null)
            {
                return ResolveStageText(ActiveStageProfile?.ObjectiveBadgeLabel, objectiveBadgeLabel);
            }

            return energyLadder.CanSpend
                ? $"LV{energyLadder.AvailableTier}"
                : $"LV{energyLadder.ChargingTier}";
        }

        private string ResolveStageEpisodeLabel()
        {
            return ResolveStageText(ActiveStageProfile?.StageEpisodeLabel, stageEpisodeLabel);
        }

        private static float ResolveHealthFill01(CombatHealth health)
        {
            return health != null ? Mathf.Clamp01(health.HealthRatio) : 0f;
        }

        private static string ResolveHealthValueText(CombatHealth health)
        {
            if (health == null)
            {
                return "HP -";
            }

            return $"HP {Mathf.CeilToInt(Mathf.Max(0f, health.CurrentHealth))}/{Mathf.CeilToInt(Mathf.Max(0f, health.MaxHealth))}";
        }

        private string ResolveEnergyValueText()
        {
            if (energyLadder == null)
            {
                return "EN -";
            }

            if (energyLadder.CanSpend)
            {
                return $"Summon ready LV{energyLadder.AvailableTier}";
            }

            return $"Cost LV{energyLadder.ChargingTier} {energyLadder.CurrentTierEnergy:0}/{energyLadder.CurrentTierTarget:0}";
        }

        private float ResolveEnergyFill01()
        {
            return energyLadder != null ? energyLadder.CurrentTierFillRatio : 0f;
        }

        private float ResolveBossCostFill01()
        {
            if (bossPressureCostLadder == null)
            {
                return 0f;
            }

            return bossPressureCostLadder.CanSpend ? 1f : bossPressureCostLadder.CurrentTierFillRatio;
        }

        private string ResolveHealthLine()
        {
            string playerLine = CombatResourceReadout.FromHealth("HP", playerHealth, playerHealthColor).Line;
            string threatLine = CombatResourceReadout.FromHealth("Threat", closeThreatHealth, threatHealthColor).Line;
            string bossLine = CombatResourceReadout.FromHealth("Boss", bossHealth, bossHealthColor).Line;
            return $"{playerLine}   {threatLine}   {bossLine}";
        }

        private string ResolvePhaseLine()
        {
            if (duelReviewOwner != null)
            {
                return $"Phase Duel {duelReviewOwner.CurrentPhase}";
            }

            if (pocketReviewOwner == null)
            {
                return "Phase -";
            }

            return $"Phase {pocketReviewOwner.CurrentPhase}";
        }

        private string ResolveEnergyLine()
        {
            if (energyLadder == null)
            {
                return "EN -";
            }

            return CombatResourceReadout.FromEnergy("EN next", energyLadder).Line;
        }

        private string ResolveRiskLine()
        {
            float risk = energyLadder != null
                ? energyLadder.CurrentForwardRisk01
                : laneSpace != null && player != null
                    ? laneSpace.EvaluateForwardRisk01(player.position)
                    : 0f;
            float gain = energyLadder != null ? energyLadder.CurrentGainMultiplier : 0f;
            string band = energyLadder != null
                ? BossBarrageLaneReviewHudText.ResolveRiskBandLabel(energyLadder.CurrentRiskBand)
                : "BackSafety";
            return $"Risk {band} {risk * 100f:0}%   EN Gain x{gain:0.00}";
        }

        private string ResolveBossBarrageLine()
        {
            if (bossBarrageEmitter == null)
            {
                return "Boss Pattern -";
            }

            BossBarragePatternProfile pattern = bossBarrageEmitter.CurrentPattern;
            if (pattern == null)
            {
                return "Boss Pattern -";
            }

            string pressureState = bossBarrageEmitter.IsWindupActive
                ? $"windup risk {bossBarrageEmitter.PendingForwardRisk01 * 100f:0}%"
                : $"shots {bossBarrageEmitter.ActiveProjectileCount}";
            string source = bossBarrageEmitter.CurrentPatternIsPriority ? "Costed" : "Basic";
            string priorityWaves = bossBarrageEmitter.CurrentPatternIsPriority
                ? $" q{bossBarrageEmitter.QueuedPriorityWavesRemaining}"
                : string.Empty;
            return $"Boss {source} P{bossBarrageEmitter.CurrentPatternSequenceIndex + 1}{priorityWaves}: "
                + $"{pattern.PatternId} [{pattern.LateralShape}]   {pressureState}";
        }

        private string ResolveBossBasicFireLine()
        {
            if (bossBasicFireEmitter == null)
            {
                return "Boss Basic Fire -";
            }

            BossBasicFireProfile profile = bossBasicFireEmitter.FireProfile;
            string label = profile != null ? profile.ReadoutLabel : "-";
            string state = bossBasicFireEmitter.IsFiringEnabled
                ? $"next {bossBasicFireEmitter.CooldownRemaining:0.0}s"
                : "off";
            return $"Boss Basic Fire {label}   {state}   shots {bossBasicFireEmitter.ActiveProjectileCount} "
                + $"volley {bossBasicFireEmitter.LastVolleyProjectileCount} "
                + $"risk {bossBasicFireEmitter.LastForwardRisk01 * 100f:0}%";
        }

        private string ResolveBossPressureLine()
        {
            if (bossPressureCostLadder == null)
            {
                return "Boss Cost -";
            }

            CombatResourceReadout costReadout =
                CombatResourceReadout.FromBossCost("Boss Cost next", bossPressureCostLadder);
            string action = bossPressureActionDirector != null && bossPressureActionDirector.TotalActionCount > 0
                ? $"{bossPressureActionDirector.LastActionKind}"
                : "-";
            string pattern = bossPressureActionDirector != null && bossPressureActionDirector.LastQueuedPattern != null
                ? bossPressureActionDirector.LastQueuedPattern.PatternId
                : "-";
            string hold = ResolveBossPressureHoldText();
            string position = bossPressurePositionController != null
                ? $" Pos {bossPressurePositionController.CurrentRisk01 * 100f:0}->{bossPressurePositionController.CurrentTargetRisk01 * 100f:0}%"
                : string.Empty;
            return $"{costReadout.Line}   "
                + $"Risk {bossPressureCostLadder.CurrentRiskBand} x{bossPressureCostLadder.CurrentGainMultiplier:0.00}{position}{hold}   "
                + $"Last {action}/{pattern}";
        }

        private string ResolveBossPressureHoldText()
        {
            if (bossPressureActionDirector == null
                || !bossPressureActionDirector.TryGetHeldNextTierAction(out BossPressureActionDirector.BossPressureActionSlot slot, out int nextTier))
            {
                return string.Empty;
            }

            string patternId = slot.Pattern != null ? slot.Pattern.PatternId : "-";
            string responseId = string.IsNullOrWhiteSpace(slot.ResponseId) ? string.Empty : $" {slot.ResponseId}";
            return $" Hold->LV{nextTier} {slot.ActionKind}/{patternId}{responseId}";
        }

        private string ResolveBossPressureResponseLine()
        {
            if (bossPressureActionDirector == null)
            {
                return "Boss Answer -";
            }

            if (bossPressureActionDirector.IsPlayerSummonResponseWindowActive)
            {
                string extension = bossPressureActionDirector.HeldResponseWindowExtensionRemainingSeconds > 0f
                    ? $" ext {bossPressureActionDirector.HeldResponseWindowExtensionRemainingSeconds:0.0}s"
                    : string.Empty;
                string waiting = ResolveBossPressureResponseWaitingText();
                return $"Boss Answer window summon LV{bossPressureActionDirector.LastObservedPlayerSummonTier} "
                    + $"{bossPressureActionDirector.PlayerSummonResponseRemainingSeconds:0.0}s{extension}{waiting}";
            }

            if (!bossPressureActionDirector.HasLastQueuedActionSlot)
            {
                return "Boss Answer -";
            }

            BossPressureActionDirector.BossPressureActionSlot slot = bossPressureActionDirector.LastQueuedActionSlot;
            if (!slot.HasResponsePlan)
            {
                return "Boss Answer missing response plan";
            }

            string answer = slot.ActionKind == BossPressureActionKind.SummonPressure
                ? slot.SummonAnswer
                : slot.PlayerAnswer;
            return $"Boss Answer {slot.ResponseId}: {answer}";
        }

        private string ResolveBossPressureResponseWaitingText()
        {
            if (bossPressureActionDirector == null
                || !bossPressureActionDirector.TryGetHeldNextTierAction(
                    out BossPressureActionDirector.BossPressureActionSlot slot,
                    out int nextTier))
            {
                return string.Empty;
            }

            string responseId = string.IsNullOrWhiteSpace(slot.ResponseId)
                ? slot.ActionKind.ToString()
                : slot.ResponseId;
            return $" waiting LV{nextTier} {responseId}";
        }

        private string ResolveCombatCueLine()
        {
            string cue = ResolvePrimaryCombatCueText();
            string risk = ResolveCompactRiskText();
            string ranged = rangedBasicAttackAction != null
                ? rangedBasicAttackAction.IsFireReady
                    ? "Fire ready"
                    : $"Fire {rangedBasicAttackAction.FireCooldownRemaining:0.0}s"
                : "Fire -";
            return $"{cue}   {risk}   {ranged}";
        }

        private string ResolvePrimaryCombatCueText()
        {
            if (bossPressureActionDirector != null
                && bossPressureActionDirector.IsPlayerSummonResponseWindowActive)
            {
                string waiting = ResolveBossPressureResponseWaitingText();
                return $"Answer: Summon shield LV{bossPressureActionDirector.LastObservedPlayerSummonTier} "
                    + $"{bossPressureActionDirector.PlayerSummonResponseRemainingSeconds:0.0}s{waiting}";
            }

            if (pocketReviewOwner != null && pocketReviewOwner.IsSummonPressureBreakActive)
            {
                if (pocketReviewOwner.IsSummonFollowupWindowActive)
                {
                    return $"Follow-up: Skill1 {pocketReviewOwner.SummonFollowupWindowRemainingSeconds:0.0}s";
                }

                return "Relief: EN pulse";
            }

            if (bossBarrageEmitter != null && bossBarrageEmitter.IsWindupActive)
            {
                BossBarragePatternProfile pattern = bossBarrageEmitter.CurrentPattern;
                string patternId = pattern != null ? pattern.PatternId : "-";
                return $"Dodge: {patternId} risk {bossBarrageEmitter.PendingForwardRisk01 * 100f:0}%";
            }

            if (bossPressureCostLadder != null && bossPressureCostLadder.CanSpend)
            {
                return $"Boss ready LV{bossPressureCostLadder.AvailableTier}";
            }

            if (bossBarrageEmitter != null && bossBarrageEmitter.CurrentPattern != null)
            {
                return $"Boss: {bossBarrageEmitter.CurrentPattern.PatternId}";
            }

            return "Cue: Hold lane";
        }

        private string ResolveCompactObjectiveLine()
        {
            if (duelReviewOwner != null)
            {
                if (duelReviewOwner.IsCleared)
                {
                    return "Goal: Duel cleared";
                }

                if (duelReviewOwner.IsFailed)
                {
                    return "Goal: Duel failed";
                }

                return "Goal: Win summon duel";
            }

            if (pocketReviewOwner == null)
            {
                return "Goal: -";
            }

            if (pocketReviewOwner.IsCleared)
            {
                return $"Goal: Frontline stabilized {ResolvePocketProgressText()}";
            }

            if (pocketReviewOwner.IsFailed)
            {
                return $"Goal: Line collapsed {ResolvePocketProgressText()}";
            }

            if (pocketReviewOwner.IsCounterWaveCompletionRecorded
                && !pocketReviewOwner.IsCounterWaveStabilized
                && !pocketReviewOwner.Skill1FollowupHitConfirmed)
            {
                return $"{ResolvePocketStepPrefix()}: Hold counter wave";
            }

            if (pocketReviewOwner.IsSkill1FollowupClearCountdownActive)
            {
                return $"{ResolvePocketStepPrefix()}: Confirm summon route {pocketReviewOwner.Skill1FollowupClearRemainingSeconds:0.0}s";
            }

            if (pocketReviewOwner.IsSummonFollowupWindowActive)
            {
                return $"{ResolvePocketStepPrefix()}: {ResolveCompactSkillFollowupText()} route window {pocketReviewOwner.SummonFollowupWindowRemainingSeconds:0.0}s";
            }

            if (pocketReviewOwner.IsSummonPressureBreakActive)
            {
                return $"{ResolvePocketStepPrefix()}: Suppress boss curtain {pocketReviewOwner.SummonPressureBreakRemainingSeconds:0.0}s";
            }

            if (pocketReviewOwner.IsCounterWaveCompletionRecorded && !pocketReviewOwner.Skill1FollowupHitConfirmed)
            {
                return pocketReviewOwner.IsCounterWaveStabilized
                    ? $"{ResolvePocketStepPrefix()}: Counter held"
                    : $"{ResolvePocketStepPrefix()}: Hold counter wave";
            }

            if (pocketReviewOwner.IsSummonBlockOpportunityCueActive)
            {
                return $"{ResolvePocketStepPrefix()}: Open summon route {pocketReviewOwner.SummonBlockOpportunityRemainingSeconds:0.0}s";
            }

            if (pocketReviewOwner.IsAwaitingSummonPressureBlock)
            {
                return $"{ResolvePocketStepPrefix()}: {ResolveCompactSummonBlockText()} NOW";
            }

            if (pocketReviewOwner.CloseThreatDefeated)
            {
                return energyLadder != null && !energyLadder.CanSpend
                    ? $"{ResolvePocketStepPrefix()}: Build EN for summon route"
                    : $"{ResolvePocketStepPrefix()}: {ResolveCompactSummonBlockText()}";
            }

            return energyLadder != null && !energyLadder.CanSpend
                ? $"{ResolvePocketStepPrefix()}: Hold line for EN"
                : $"{ResolvePocketStepPrefix()}: Stop close probe";
        }

        private string ResolveCompactRouteIncentiveLine()
        {
            if (pocketReviewOwner == null)
            {
                return string.Empty;
            }

            FrontlineWaveStageProfile profile = ActiveStageProfile;
            if (pocketReviewOwner.IsFailed)
            {
                return ResolveStageText(
                    profile?.CollapseWarningRecordPreview,
                    "Record warning: line collapse logs failure analysis, not boss progress.");
            }

            if (pocketReviewOwner.IsCleared)
            {
                return $"Record sealed: {ResolveClearedPocketRouteType()}";
            }

            if (pocketReviewOwner.IsRouteStabilityActive
                && pocketReviewOwner.CurrentRouteStabilityBand == BossBarragePocketReviewOwner.RouteStabilityBand.Critical)
            {
                return ResolveStageText(
                    profile?.CollapseWarningRecordPreview,
                    "Record warning: line collapse logs failure analysis, not boss progress.");
            }

            if (pocketReviewOwner.IsCounterWaveCompletionRecorded && !pocketReviewOwner.Skill1FollowupHitConfirmed)
            {
                return ResolveStageText(
                    profile?.CounterRecoveryRecordPreview,
                    "Record preview: hold counter wave to reopen final follow-up.");
            }

            if (pocketReviewOwner.IsSummonFollowupWindowActive
                || pocketReviewOwner.IsSkill1FollowupClearCountdownActive
                || pocketReviewOwner.IsSummonPressureBreakActive)
            {
                return ResolveStageText(
                    profile?.CleanFollowupRecordPreview,
                    "Record preview: Skill1 now secures clean route before counter wave.");
            }

            if (pocketReviewOwner.IsAwaitingSummonPressureBlock
                || pocketReviewOwner.IsSummonBlockOpportunityCueActive
                || pocketReviewOwner.CloseThreatDefeated)
            {
                return ResolveStageText(
                    profile?.SummonRecordPreview,
                    "Record preview: summon block opens the Skill1 route record.");
            }

            return ResolveStageText(
                profile?.OpeningRecordPreview,
                "Record preview: stop close probe, block curtain, confirm Skill1.");
        }

        private string ResolveStageBriefingLine()
        {
            FrontlineWaveStageProfile profile = ActiveStageProfile;
            if (profile == null)
            {
                return "Stage Briefing -";
            }

            string displayName = ResolveStageText(profile.DisplayName, "Frontline Review");
            string promise = ResolveStageText(profile.CombatPromise, "Bodies split; summons contest the line");
            string entryCue = ResolveStageText(profile.EntryCue, "Hold line; prove summon route");
            return $"{displayName}: {promise} | {entryCue}";
        }

        private string ResolveCompactStageBriefingLine()
        {
            FrontlineWaveStageProfile profile = ActiveStageProfile;
            if (profile == null)
            {
                return string.Empty;
            }

            return ResolveStageText(profile.EntryCue, "Hold line; prove summon route");
        }

        private string ResolveStageBeatLine()
        {
            if (!TryResolveCurrentStageBeat(out FrontlineWaveStageProfile.StageBeat beat, out int beatIndex, out int beatCount))
            {
                return "Beat -";
            }

            string source = string.IsNullOrWhiteSpace(beat.SourcePattern) ? "-" : beat.SourcePattern;
            return $"Beat {beatIndex + 1}/{beatCount} {beat.Label}: {beat.ObjectiveCue} | observe {beat.ObservedEvent} | {source}";
        }

        private string ResolveCompactStageBeatLine()
        {
            if (!TryResolveCurrentStageBeat(out FrontlineWaveStageProfile.StageBeat beat, out int beatIndex, out int beatCount))
            {
                return "Beat -";
            }

            string pressureSlotLabel = TryResolveCurrentPressureSlot(out FrontlineWaveStageProfile.PressureSlot slot, out _, out _)
                ? slot.Label
                : "-";
            return $"Beat {beatIndex + 1}/{beatCount} {beat.Label} | {pressureSlotLabel}";
        }

        private string ResolvePressureSlotLine()
        {
            if (!TryResolveCurrentPressureSlot(out FrontlineWaveStageProfile.PressureSlot slot, out int slotIndex, out int slotCount))
            {
                return "Pressure Slot -";
            }

            return $"Pressure Slot {slotIndex + 1}/{slotCount} {slot.Label}: {slot.SpawnFamily} {slot.WavePathPattern} | {slot.PlayerRead} | observe {slot.ObserverEvent}";
        }

        private bool TryResolveCurrentStageBeat(
            out FrontlineWaveStageProfile.StageBeat beat,
            out int beatIndex,
            out int beatCount)
        {
            beat = default;
            beatIndex = 0;
            beatCount = 0;
            FrontlineWaveStageProfile profile = ActiveStageProfile;
            if (profile == null || profile.BeatCount <= 0)
            {
                return false;
            }

            beatCount = profile.BeatCount;
            int currentBeatIndex = pocketReviewOwner != null ? pocketReviewOwner.CurrentStageBeatIndex : 0;
            beatIndex = Mathf.Clamp(currentBeatIndex, 0, beatCount - 1);
            beat = profile.GetBeat(beatIndex);
            return true;
        }

        private bool TryResolveCurrentPressureSlot(
            out FrontlineWaveStageProfile.PressureSlot slot,
            out int slotIndex,
            out int slotCount)
        {
            slot = default;
            slotIndex = 0;
            slotCount = 0;
            FrontlineWaveStageProfile profile = ActiveStageProfile;
            if (profile == null || profile.PressureSlotCount <= 0)
            {
                return false;
            }

            slotCount = profile.PressureSlotCount;
            int currentSlotIndex = pocketReviewOwner != null
                ? pocketReviewOwner.CurrentPressureSlotIndex
                : 0;
            slotIndex = Mathf.Clamp(currentSlotIndex, 0, slotCount - 1);
            slot = profile.GetPressureSlot(slotIndex);
            return true;
        }

        private string ResolvePocketStepPrefix()
        {
            if (pocketReviewOwner == null)
            {
                return "Goal";
            }

            int total = Mathf.Max(1, pocketReviewOwner.ObjectiveStepCount);
            int nextStep = Mathf.Clamp(pocketReviewOwner.CompletedObjectiveStepCount + 1, 1, total);
            string prefix = ResolveStageText(ActiveStageProfile?.StepPrefix, "Step");
            return $"{prefix} {nextStep}/{total}";
        }

        private string ResolvePocketProgressText()
        {
            if (pocketReviewOwner == null)
            {
                return "0/0";
            }

            int total = Mathf.Max(1, pocketReviewOwner.ObjectiveStepCount);
            int completed = Mathf.Clamp(pocketReviewOwner.CompletedObjectiveStepCount, 0, total);
            return $"{completed}/{total}";
        }

        private string ResolveCompactSummonBlockText()
        {
            return $"{ResolveSummonTierLabel(ResolveCompactSummonTier())} block";
        }

        private int ResolveCompactSummonTier()
        {
            if (pocketReviewOwner != null && pocketReviewOwner.LastSummonPressureBreakTier > 0)
            {
                return pocketReviewOwner.LastSummonPressureBreakTier;
            }

            if (energyLadder != null && energyLadder.CanSpend)
            {
                return energyLadder.AvailableTier;
            }

            return 1;
        }

        private string ResolveCompactSkillFollowupText()
        {
            int tier = energyLadder != null && energyLadder.CanSpend
                ? energyLadder.AvailableTier
                : pocketReviewOwner != null && pocketReviewOwner.LastSummonPressureBreakTier > 0
                    ? pocketReviewOwner.LastSummonPressureBreakTier
                    : 1;
            return $"Skill1 LV{Mathf.Clamp(tier, 1, 3)}";
        }

        private string ResolveCompactPhaseLine()
        {
            if (duelReviewOwner != null)
            {
                return $"Phase: {BossBarrageLaneReviewHudText.ResolveCompactPhaseLabel(duelReviewOwner.CurrentPhase.ToString())}";
            }

            if (pocketReviewOwner == null)
            {
                return "Phase: -";
            }

            return $"Phase: {BossBarrageLaneReviewHudText.ResolveCompactPhaseLabel(pocketReviewOwner.CurrentPhase.ToString())}";
        }

        private string ResolveCompactCombatCueLine()
        {
            return $"{ResolveCompactPrimaryCombatCueText()} | {ResolveCompactRiskText()} | {ResolveCompactFireText()}";
        }

        private string ResolveCompactPrimaryCombatCueText()
        {
            if (bossPressureActionDirector != null
                && bossPressureActionDirector.IsPlayerSummonResponseWindowActive)
            {
                return $"Answer: Summon LV{bossPressureActionDirector.LastObservedPlayerSummonTier} "
                    + $"{bossPressureActionDirector.PlayerSummonResponseRemainingSeconds:0.0}s";
            }

            if (pocketReviewOwner != null && pocketReviewOwner.IsSummonPressureBreakActive)
            {
                return pocketReviewOwner.IsSummonFollowupWindowActive
                    ? $"Follow-up {pocketReviewOwner.SummonFollowupWindowRemainingSeconds:0.0}s"
                    : "Break: EN pulse";
            }

            if (bossBarrageEmitter != null && bossBarrageEmitter.IsWindupActive)
            {
                return $"Dodge: {BossBarrageLaneReviewHudText.ShortenPatternId(bossBarrageEmitter.CurrentPattern?.PatternId)}";
            }

            if (bossPressureCostLadder != null && bossPressureCostLadder.CanSpend)
            {
                return $"Boss LV{bossPressureCostLadder.AvailableTier} ready";
            }

            if (bossBarrageEmitter != null && bossBarrageEmitter.CurrentPattern != null)
            {
                return $"Boss: {BossBarrageLaneReviewHudText.ShortenPatternId(bossBarrageEmitter.CurrentPattern.PatternId)}";
            }

            return "Hold lane";
        }

        private string ResolveCompactFireText()
        {
            if (rangedBasicAttackAction == null)
            {
                return "Fire -";
            }

            return rangedBasicAttackAction.IsFireReady
                ? "Fire ready"
                : $"Fire {rangedBasicAttackAction.FireCooldownRemaining:0.0}s";
        }

        private string ResolveCompactRiskText()
        {
            float risk = energyLadder != null
                ? energyLadder.CurrentForwardRisk01
                : laneSpace != null && player != null
                    ? laneSpace.EvaluateForwardRisk01(player.position)
                    : 0f;
            string band = energyLadder != null
                ? BossBarrageLaneReviewHudText.ResolveCompactRiskBandLabel(energyLadder.CurrentRiskBand)
                : "Back";
            return $"Risk {band} {risk * 100f:0}%";
        }

        private string ResolveBossSummonLine()
        {
            if (bossSummonPressureAction == null)
            {
                return "Boss Summon -";
            }

            string tier = bossSummonPressureAction.LastReleasedTier > 0
                ? ResolveBossSummonTierLabel(bossSummonPressureAction.LastReleasedTier)
                : "LV-";
            return $"Boss Summon {tier} proxy {bossSummonPressureAction.ActiveSummonActorCount} "
                + $"shield {bossSummonPressureAction.ActivePressureScreenCount} "
                + $"blocks {bossSummonPressureAction.ActivePressureScreenRemainingIntercepts} "
                + BossBarrageLaneReviewHudText.ResolveSummonLifecycleLine(
                    bossSummonPressureAction.ActiveSummonActorCount,
                    bossSummonPressureAction.LastSummonActorRemainingLifetimeSeconds,
                    bossSummonPressureAction.ActiveSummonActorAdvanceProgress01,
                    bossSummonPressureAction.LastSummonActorHasHealth,
                    bossSummonPressureAction.LastSummonActorHealthRatio,
                    bossSummonPressureAction.LastSummonActorIsClashing,
                    bossSummonPressureAction.LastSummonActorClashCount,
                    bossSummonPressureAction.LastSummonActorExitReason)
                + $" used {bossSummonPressureAction.TotalReleaseCount}";
        }

        private string ResolveFrontlineCueLine()
        {
            BossBarrageFrontlineReadout readout = BossBarrageLaneReviewHudText.ResolveFrontlineProxyReadout();
            return $"Frontline {readout.State} A{readout.AllyCount} {readout.AllyHealthText} / "
                + $"E{readout.EnemyCount} {readout.EnemyHealthText}   {ResolvePlayerSummonCueText()}";
        }

        private string ResolveCompactFrontlineCueLine()
        {
            BossBarrageFrontlineReadout readout = BossBarrageLaneReviewHudText.ResolveFrontlineProxyReadout();
            return $"Front {readout.State}: {ResolveCompactRouteStabilityText()} A{readout.AllyCount} {readout.AllyHealthText} / "
                + $"E{readout.EnemyCount} {readout.EnemyHealthText} | {ResolveCompactSummonText()}";
        }

        private string ResolveCompactSummonText()
        {
            if (energyLadder == null)
            {
                return "S1 -";
            }

            return energyLadder.CanSpend
                ? $"S1 ready LV{energyLadder.AvailableTier}"
                : $"S1 LV{energyLadder.ChargingTier} {energyLadder.CurrentTierFillRatio * 100f:0}%";
        }

        private string ResolvePlayerSummonCueText()
        {
            if (energyLadder == null)
            {
                return "Summon -";
            }

            return energyLadder.CanSpend
                ? $"Summon ready LV{energyLadder.AvailableTier}"
                : $"Summon charge LV{energyLadder.ChargingTier} {energyLadder.CurrentTierFillRatio * 100f:0}%";
        }

        private string ResolveFrontlineLoopLine()
        {
            BossBarrageFrontlineReadout readout = BossBarrageLaneReviewHudText.ResolveFrontlineProxyReadout();
            string loop = duelReviewOwner != null
                ? duelReviewOwner.CurrentPhase.ToString()
                : pocketReviewOwner != null
                    ? pocketReviewOwner.CurrentPhase.ToString()
                    : "-";
            string player = energyLadder != null
                ? energyLadder.CanSpend
                    ? $"player ready LV{energyLadder.AvailableTier}"
                    : $"player build LV{energyLadder.ChargingTier} {energyLadder.CurrentTierFillRatio * 100f:0}%"
                : "player -";
            string boss = ResolveBossLoopReadout();
            return $"Loop {loop}   frontline {readout.State} "
                + $"{ResolveRouteStabilityText()}   "
                + $"ally {readout.AllyCount} hp {readout.AllyHealthText} "
                + $"enemy {readout.EnemyCount} hp {readout.EnemyHealthText}   "
                + $"{player}   {boss}";
        }

        private string ResolveFrontlineTuningLine()
        {
            return $"Tune EN {ResolveEnergyTuningText()}   Cost {ResolveBossCostTuningText()}   "
                + BossBarrageLaneReviewHudText.ResolveActiveFrontlineTuningText();
        }

        private string ResolveEnergyTuningText()
        {
            if (energyLadder == null)
            {
                return "-";
            }

            string ready = energyLadder.CanSpend ? $" ready LV{energyLadder.AvailableTier}" : string.Empty;
            return $"LV{energyLadder.ChargingTier} {energyLadder.CurrentTierEnergy:0}/{energyLadder.CurrentTierTarget:0}{ready}";
        }

        private string ResolveBossCostTuningText()
        {
            if (bossPressureCostLadder == null)
            {
                return "-";
            }

            string ready = bossPressureCostLadder.CanSpend ? $" ready LV{bossPressureCostLadder.AvailableTier}" : string.Empty;
            return $"LV{bossPressureCostLadder.ChargingTier} {bossPressureCostLadder.CurrentTierCost:0}/{bossPressureCostLadder.CurrentTierTarget:0}{ready}";
        }

        private string ResolveBossLoopReadout()
        {
            if (bossPressureActionDirector != null
                && bossPressureActionDirector.IsPlayerSummonResponseWindowActive)
            {
                return $"boss reply {bossPressureActionDirector.PlayerSummonResponseRemainingSeconds:0.0}s";
            }

            if (bossPressureCostLadder == null)
            {
                return "boss -";
            }

            if (bossPressureCostLadder.CanSpend)
            {
                return $"boss ready LV{bossPressureCostLadder.AvailableTier}";
            }

            return $"boss build LV{bossPressureCostLadder.ChargingTier} {bossPressureCostLadder.CurrentTierFillRatio * 100f:0}%";
        }

        private string ResolveActionLine()
        {
            string skill = skill1Action != null && energyLadder != null && energyLadder.CanSpend
                ? $"Skill1 LV{energyLadder.AvailableTier}"
                : "Skill1 not ready";
            string summon = summonSlot1Action != null && energyLadder != null && energyLadder.CanSpend
                ? $"SummonSlot1 {ResolveSummonTierLabel(energyLadder.AvailableTier)}"
                : "SummonSlot1 not ready";
            return $"{skill}   {summon}";
        }

        private string ResolveWeaponModeLine()
        {
            string mode = combatModeController != null ? combatModeController.CurrentMode.ToString() : "-";
            string aim = rangedAimController != null && rangedAimController.IsAiming ? "AIM" : "hip";
            return $"Weapon {mode}   Aim {aim}";
        }

        private string ResolveRangedFireLine()
        {
            if (rangedBasicAttackAction == null)
            {
                return "Ranged Fire -";
            }

            string ready = rangedBasicAttackAction.IsFireReady
                ? "READY"
                : $"{rangedBasicAttackAction.FireCooldownRemaining:0.00}s";
            return $"Ranged Fire {ready}   bolts {rangedBasicAttackAction.ActiveProjectileCount}";
        }

        private string ResolveSummonExchangeLine()
        {
            string skillShots = skill1Action != null
                ? $"Skill bolts {skill1Action.ActiveProjectileCount}"
                : "Skill bolts -";
            if (summonSlot1Action == null)
            {
                return $"{skillShots}   Summon -";
            }

            string tier = summonSlot1Action.LastSpentTier > 0
                ? ResolveSummonTierLabel(summonSlot1Action.LastSpentTier)
                : "LV-";
            return $"{skillShots}   Summon {tier} proxy {summonSlot1Action.ActiveSummonActorCount} "
                + $"bolts {summonSlot1Action.ActiveProjectileCount} shield {summonSlot1Action.ActivePressureScreenCount} "
                + $"blocks {summonSlot1Action.ActivePressureScreenRemainingIntercepts}"
                + BossBarrageLaneReviewHudText.ResolveSummonLifecycleLine(
                    summonSlot1Action.ActiveSummonActorCount,
                    summonSlot1Action.LastSummonActorRemainingLifetimeSeconds,
                    summonSlot1Action.ActiveSummonActorAdvanceProgress01,
                    summonSlot1Action.LastSummonActorHasHealth,
                    summonSlot1Action.LastSummonActorHealthRatio,
                    summonSlot1Action.LastSummonActorIsClashing,
                    summonSlot1Action.LastSummonActorClashCount,
                    summonSlot1Action.LastSummonActorExitReason)
                + $"   {ResolveSupportSummonLine("S2", summonSlot2Action)}"
                + $"   {ResolveSupportSummonLine("S3", summonSlot3Action)}"
                + ResolveSummonBlockWindowLine()
                + ResolveFollowupLine();
        }

        private static string ResolveSupportSummonLine(string label, PlayerSupportSummonSlotAction action)
        {
            if (action == null)
            {
                return $"{label} -";
            }

            string tier = action.LastSpentTier > 0 ? $"LV{action.LastSpentTier}" : "LV-";
            string role = string.IsNullOrWhiteSpace(action.LastSummonActorRoleId)
                ? "-"
                : action.LastSummonActorRoleId;
            return $"{label} {tier} {role} proxy {action.ActiveSummonActorCount} "
                + $"volley {action.LastVolleyWaveCount} bolts {action.ActiveProjectileCount} "
                + $"blocks {action.TotalPressureScreenInterceptCount}"
                + BossBarrageLaneReviewHudText.ResolveSummonLifecycleLine(
                    action.ActiveSummonActorCount,
                    action.LastSummonActorRemainingLifetimeSeconds,
                    action.ActiveSummonActorAdvanceProgress01,
                    action.LastSummonActorHasHealth,
                    action.LastSummonActorHealthRatio,
                    action.LastSummonActorIsClashing,
                    action.LastSummonActorClashCount,
                    action.LastSummonActorExitReason);
        }

        private string ResolveDuelProgressLine()
        {
            return duelReviewOwner != null ? duelReviewOwner.ProgressLine : string.Empty;
        }

        private string ResolveSummonTierLabel(int tier)
        {
            if (summonSlot1Action != null
                && summonSlot1Action.TryGetTierReadout(tier, out SummonSlotActionProfile.SummonTierReadout readout))
            {
                return readout.TierLabel;
            }

            return $"LV{Mathf.Clamp(tier, 1, 3)}";
        }

        private string ResolveBossSummonTierLabel(int tier)
        {
            if (bossSummonPressureAction != null
                && bossSummonPressureAction.TryGetTierReadout(tier, out BossSummonPressureProfile.BossSummonTierReadout readout))
            {
                return readout.TierLabel;
            }

            return $"LV{Mathf.Clamp(tier, 1, 3)}";
        }

        private string ResolveActionHintLine()
        {
            if (skill1Action != null && skill1Action.ShowUseBlockedHint && !string.IsNullOrWhiteSpace(skill1Action.LastUseBlockedReason))
            {
                return $"Hint: {skill1Action.LastUseBlockedReason}";
            }

            if (summonSlot1Action != null && summonSlot1Action.ShowUseBlockedHint && !string.IsNullOrWhiteSpace(summonSlot1Action.LastUseBlockedReason))
            {
                return $"Hint: {summonSlot1Action.LastUseBlockedReason}";
            }

            if (rangedBasicAttackAction != null
                && rangedBasicAttackAction.ShowUseBlockedHint
                && !string.IsNullOrWhiteSpace(rangedBasicAttackAction.LastUseBlockedReason))
            {
                return $"Hint: {rangedBasicAttackAction.LastUseBlockedReason}";
            }

            return "Hint: -";
        }

        private string ResolveSummonBlockWindowLine()
        {
            if (pocketReviewOwner == null)
            {
                return string.Empty;
            }

            if (pocketReviewOwner.IsSummonBlockOpportunityCueActive)
            {
                return $" cue {pocketReviewOwner.SummonBlockOpportunityRemainingSeconds:0.0}s";
            }

            return pocketReviewOwner.IsAwaitingSummonPressureBlock
                ? " block NOW"
                : string.Empty;
        }

        private string ResolveFollowupLine()
        {
            if (pocketReviewOwner == null || !pocketReviewOwner.IsSummonPressureBreakActive)
            {
                return ResolveLastPressureRewardLine();
            }

            string followup = pocketReviewOwner.IsSummonFollowupWindowActive
                ? $" follow-up {pocketReviewOwner.SummonFollowupWindowRemainingSeconds:0.0}s"
                : " relief";
            if (pocketReviewOwner.Skill1FollowupHitConfirmed)
            {
                return $"{followup} Skill1 hit {pocketReviewOwner.Skill1FollowupDamage:0}";
            }

            if (pocketReviewOwner.BossBlockedSkill1Followup)
            {
                return $"{followup} boss shield blocked Skill1";
            }

            return pocketReviewOwner.UsedSkill1DuringSummonFollowup
                ? $"{followup} Skill1 fired"
                : $"{followup} EN pulse";
        }

        private string ResolveLastPressureRewardLine()
        {
            if (pocketReviewOwner == null || pocketReviewOwner.LastSummonPressureBreakTier <= 0)
            {
                return string.Empty;
            }

            return $" last break LV{pocketReviewOwner.LastSummonPressureBreakTier}"
                + $" {pocketReviewOwner.LastSummonPressureBreakDuration:0.0}s"
                + $" pulse {pocketReviewOwner.SummonFollowupEnergyPulse:0}";
        }

        private string ResolveObjectiveLine()
        {
            if (duelReviewOwner != null)
            {
                string duelState = duelReviewOwner.IsCleared
                    ? "CLEARED"
                    : duelReviewOwner.IsFailed
                        ? "FAILED"
                        : "RUNNING";
                return $"{duelState}: {duelReviewOwner.ObjectiveCue}";
            }

            if (pocketReviewOwner == null)
            {
                return "Objective -";
            }

            string state = pocketReviewOwner.IsCleared
                ? "CLEARED"
                : pocketReviewOwner.IsFailed
                    ? "FAILED"
                    : "RUNNING";
            return $"{state}: {pocketReviewOwner.ObjectiveCue}";
        }

        private void EnsureStyles()
        {
            if (labelStyle != null && titleStyle != null && boxStyle != null && resourceBarStyle != null)
            {
                return;
            }

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
            labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                wordWrap = true,
                normal = { textColor = Color.white }
            };
            boxStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(14, 14, 12, 12),
                normal = { textColor = Color.white }
            };
            resourceBarStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                padding = new RectOffset(6, 6, 0, 0),
                normal = { textColor = resourceTextColor }
            };
            resultBannerTitleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperCenter,
                fontSize = 28,
                fontStyle = FontStyle.Bold,
                normal = { textColor = resultBannerTextColor }
            };
            resultBannerDetailStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperCenter,
                fontSize = 15,
                wordWrap = true,
                normal = { textColor = resultBannerTextColor }
            };
        }

        private void DrawResultBanner(float uiScale)
        {
            if (!TryResolveResultBanner(out string title, out string detail, out Color backColor))
            {
                return;
            }

            float scaledScreenWidth = Screen.width / uiScale;
            float scaledScreenHeight = Screen.height / uiScale;
            float bannerWidth = Mathf.Min(resultBannerWidth, Mathf.Max(1f, scaledScreenWidth - margin * 2f));
            float bannerHeight = Mathf.Min(resultBannerHeight, Mathf.Max(1f, scaledScreenHeight - margin * 2f));
            float x = (scaledScreenWidth - bannerWidth) * 0.5f;
            float y = Mathf.Clamp(
                scaledScreenHeight - bannerHeight - resultBannerBottomOffset,
                margin,
                Mathf.Max(margin, scaledScreenHeight - bannerHeight - margin));
            Rect rect = new Rect(x, y, bannerWidth, bannerHeight);

            Color previousColor = GUI.color;
            GUI.color = backColor;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = resultBannerTextColor;
            GUI.Label(new Rect(rect.x + 12f, rect.y + 9f, rect.width - 24f, 34f), title, resultBannerTitleStyle);
            GUI.Label(new Rect(rect.x + 18f, rect.y + 45f, rect.width - 36f, rect.height - 48f), detail, resultBannerDetailStyle);
            GUI.color = previousColor;
        }

        private bool TryResolveResultBanner(out string title, out string detail, out Color backColor)
        {
            title = string.Empty;
            detail = string.Empty;
            backColor = Color.clear;
            if (!showResultBanner)
            {
                return false;
            }

            if (duelReviewOwner != null)
            {
                if (duelReviewOwner.IsCleared)
                {
                    title = "DUEL CLEAR";
                    detail = duelReviewOwner.ObjectiveCue;
                    backColor = resultClearBackColor;
                    return true;
                }

                if (duelReviewOwner.IsFailed)
                {
                    title = "DUEL FAILED";
                    detail = duelReviewOwner.ObjectiveCue;
                    backColor = resultFailBackColor;
                    return true;
                }

                return false;
            }

            if (pocketReviewOwner == null)
            {
                return false;
            }

            if (pocketReviewOwner.IsCleared)
            {
                FrontlineWaveStageProfile activeProfile = ActiveStageProfile;
                title = ResolveStageText(activeProfile?.ClearTitle, "FRONTLINE STABILIZED");
                detail = pocketReviewOwner.Skill1FollowupHitConfirmed
                    ? $"{ResolveStageText(ResolvePocketClearFollowupDetail(activeProfile), "Summon route analyzed; Skill1 follow-up confirmed")} ({pocketReviewOwner.Skill1FollowupDamage:0}) | {ResolvePocketResultSuffix()}"
                    : $"{ResolveStageText(activeProfile?.ClearPressureDetail, "Boss curtain suppressed; frontline route recorded")} | {ResolvePocketResultSuffix()}";
                backColor = resultClearBackColor;
                return true;
            }

            if (pocketReviewOwner.IsFailed)
            {
                FrontlineWaveStageProfile activeProfile = ActiveStageProfile;
                title = ResolveStageText(activeProfile?.FailTitle, "LINE COLLAPSED");
                detail = $"{ResolvePocketFailDetail(activeProfile)} | {ResolvePocketResultSuffix()}";
                backColor = resultFailBackColor;
                return true;
            }

            return false;
        }

        private string ResolvePocketResultSuffix()
        {
            if (pocketReviewOwner == null)
            {
                return "Route - | Record -";
            }

            string prefix = ResolveStageText(ActiveStageProfile?.StepPrefix, "Route");
            return $"{prefix} {ResolvePocketProgressText()} | {ResolveRouteRecordSummary()}";
        }

        private string ResolveRouteRecordLine()
        {
            return $"Route Record: {ResolveRouteRecordSummary()}";
        }

        private string ResolveRouteStabilityLine()
        {
            return $"Route Stability: {ResolveRouteStabilityText()}";
        }

        private string ResolveRouteRecordSummary()
        {
            if (pocketReviewOwner == null)
            {
                return "-";
            }

            FrontlineWaveStageProfile profile = ActiveStageProfile;
            float targetSeconds = Mathf.Max(1f, profile != null ? profile.TargetDurationSeconds : 90f);
            string targetText = $"{pocketReviewOwner.ResultElapsedSeconds:0.0}/{targetSeconds:0.0}s";
            if (pocketReviewOwner.IsFailed)
            {
                return $"Incomplete {ResolvePocketProgressText()} {ResolvePocketFailureReasonText()} {ResolveRouteStabilityText()} ({targetText}) | {ResolveCompletionRecordText()}";
            }

            if (!pocketReviewOwner.IsCleared)
            {
                string hook = ResolveStageText(profile?.RewardHook, "Review-only route record");
                return $"Pending {ResolvePocketProgressText()} {ResolveRouteStabilityText()} target {targetSeconds:0}s | {ResolveCompletionRecordText()} | {hook}";
            }

            string grade = ResolveRouteRecordGrade(targetSeconds);
            string routeType = ResolveClearedPocketRouteType();
            return $"Record {grade}: {routeType} {targetText} {ResolveRouteStabilityText()} | {ResolveCompletionRecordText()}";
        }

        private string ResolvePocketClearFollowupDetail(FrontlineWaveStageProfile activeProfile)
        {
            if (IsCounterRecoveryClear())
            {
                return activeProfile?.ClearCounterDetail;
            }

            return activeProfile?.ClearFollowupDetail;
        }

        private string ResolveClearedPocketRouteType()
        {
            if (IsCounterRecoveryClear())
            {
                return "Counter recovery";
            }

            return pocketReviewOwner.Skill1FollowupHitConfirmed
                ? "Summon follow-up"
                : "Pressure suppression";
        }

        private bool IsCounterRecoveryClear()
        {
            return pocketReviewOwner != null
                && pocketReviewOwner.Skill1FollowupHitConfirmed
                && (pocketReviewOwner.IsCounterWaveStabilized || pocketReviewOwner.IsCounterWaveFinalWindowOpened);
        }

        private string ResolveCompletionRecordText()
        {
            return pocketReviewOwner != null
                ? pocketReviewOwner.CompletionRecordReadout
                : "close:pending summon:pending followup:pending counter:pending(none) counter_answer:pending(none) counter_window:pending(none) decision:build_route(hold_line)";
        }

        private string ResolveRouteStabilityText()
        {
            if (pocketReviewOwner == null || !pocketReviewOwner.IsRouteStabilityActive)
            {
                return "stability -";
            }

            string band = pocketReviewOwner.CurrentRouteStabilityBand.ToString().ToLowerInvariant();
            return $"stability {pocketReviewOwner.RouteStabilityPercent:0}% {band} {pocketReviewOwner.FrontlinePresenceReadout}";
        }

        private string ResolveCompactRouteStabilityText()
        {
            if (pocketReviewOwner == null || !pocketReviewOwner.IsRouteStabilityActive)
            {
                return "Route -";
            }

            string band = pocketReviewOwner.CurrentRouteStabilityBand.ToString().ToLowerInvariant();
            return $"Route {pocketReviewOwner.RouteStabilityPercent:0}% {band} {pocketReviewOwner.FrontlinePresenceReadout}";
        }

        private string ResolveRouteRecordGrade(float targetSeconds)
        {
            if (pocketReviewOwner == null || !pocketReviewOwner.IsCleared)
            {
                return "-";
            }

            float timeRatio = pocketReviewOwner.ResultElapsedSeconds / Mathf.Max(1f, targetSeconds);
            if (IsCounterRecoveryClear())
            {
                if (timeRatio > 1f)
                {
                    return "C";
                }

                return pocketReviewOwner.LastCounterWaveFinalWindowRouteScale < 0.999f ? "B" : "A";
            }

            if (pocketReviewOwner.Skill1FollowupHitConfirmed && timeRatio <= 0.6f)
            {
                return "S";
            }

            if (pocketReviewOwner.Skill1FollowupHitConfirmed && timeRatio <= 1f)
            {
                return "A";
            }

            return timeRatio <= 1f ? "B" : "C";
        }

        private string ResolvePocketFailDetail(FrontlineWaveStageProfile activeProfile)
        {
            if (pocketReviewOwner != null && pocketReviewOwner.FailedFromRouteStabilityCollapse)
            {
                return ResolveStageText(
                    activeProfile?.RouteCollapseFailDetail,
                    "Route stability collapsed before the frontline could stabilize");
            }

            return ResolveStageText(
                activeProfile?.FailDetail,
                "Player down before the frontline route could stabilize");
        }

        private string ResolvePocketFailureReasonText()
        {
            if (pocketReviewOwner == null || !pocketReviewOwner.IsFailed)
            {
                return "reason -";
            }

            return pocketReviewOwner.FailureReason switch
            {
                BossBarragePocketReviewOwner.RouteFailureReason.RouteStabilityCollapsed => "reason route collapse",
                BossBarragePocketReviewOwner.RouteFailureReason.PlayerDown => "reason player down",
                _ => "reason failed"
            };
        }

        private static string ResolveStageText(string profileText, string fallback)
        {
            return string.IsNullOrWhiteSpace(profileText) ? fallback : profileText;
        }

        private void DrawCombatResourceBars()
        {
            if (!showResourceBars)
            {
                return;
            }

            DrawResourceBar(CombatResourceReadout.FromHealth("HP", playerHealth, playerHealthColor));
            DrawResourceBar(CombatResourceReadout.FromEnergy("EN", energyLadder));
            DrawResourceBar(CombatResourceReadout.FromHealth("Boss", bossHealth, bossHealthColor));
            DrawResourceBar(CombatResourceReadout.FromBossCost("Cost", bossPressureCostLadder));
            if (closeThreatHealth != null && closeThreatHealth.IsAlive)
            {
                DrawResourceBar(CombatResourceReadout.FromHealth("Threat", closeThreatHealth, threatHealthColor));
            }
        }

        private void DrawResourceBar(CombatResourceReadout readout)
        {
            Rect rect = GUILayoutUtility.GetRect(1f, resourceBarHeight, GUILayout.ExpandWidth(true));
            Color previousColor = GUI.color;
            GUI.color = resourceBarBackColor;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);

            Rect fillRect = rect;
            fillRect.width *= readout.Fill01;
            GUI.color = readout.FillColor;
            GUI.DrawTexture(fillRect, Texture2D.whiteTexture);

            resourceBarStyle.normal.textColor = readout.IsReady ? resourceReadyTextColor : resourceTextColor;
            GUI.color = Color.white;
            GUI.Label(rect, readout.Line, resourceBarStyle);
            GUI.color = previousColor;

            if (resourceBarGap > 0f)
            {
                GUILayout.Space(resourceBarGap);
            }
        }

        private static float ResolveUiScale()
        {
            return Mathf.Clamp(Screen.height / 1440f, 0.9f, 1.2f);
        }

        private float ResolveHudAreaHeight(float uiScale)
        {
            float maxAreaHeight = Mathf.Max(1f, (Screen.height / uiScale) - (margin * 2f));
            float requestedHeight = showDetailedTelemetry ? maxAreaHeight : height;
            return Mathf.Clamp(requestedHeight, 1f, maxAreaHeight);
        }

        private void DrawReticleIfNeeded()
        {
            if (!showCenterReticle)
            {
                return;
            }

            if (combatModeController != null && !combatModeController.IsRangedMode)
            {
                return;
            }

            if (rangedBasicAttackAction == null)
            {
                return;
            }

            bool aiming = rangedAimController != null && rangedAimController.IsAiming;
            float centerX = Screen.width * 0.5f;
            float centerY = Screen.height * 0.5f;
            float gap = aiming ? 9f : 6f;
            float length = aiming ? 18f : 10f;
            float thickness = aiming ? 3f : 2f;
            Color previousColor = GUI.color;
            GUI.color = aiming
                ? new Color(0.42f, 0.95f, 1f, 0.92f)
                : new Color(1f, 1f, 1f, 0.42f);

            GUI.DrawTexture(new Rect(centerX - gap - length, centerY - thickness * 0.5f, length, thickness), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(centerX + gap, centerY - thickness * 0.5f, length, thickness), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(centerX - thickness * 0.5f, centerY - gap - length, thickness, length), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(centerX - thickness * 0.5f, centerY + gap, thickness, length), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(centerX - thickness * 0.5f, centerY - thickness * 0.5f, thickness, thickness), Texture2D.whiteTexture);
            GUI.color = previousColor;
        }
    }
}
