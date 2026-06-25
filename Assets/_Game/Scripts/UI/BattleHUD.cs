using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace IsekaiBrawl.Gameplay
{
    internal static class RuntimeCanvasLayoutUtility
    {
        private static readonly Vector2 DefaultReferenceResolution = new(720f, 1280f);

        public static float ResolveScale(RectTransform context)
        {
            if (context == null)
            {
                return 1f;
            }

            Vector2 referenceResolution = ResolveReferenceResolution(context);
            Vector2 canvasSize = ResolveCanvasSize(context, referenceResolution);

            float widthScale = canvasSize.x / Mathf.Max(1f, referenceResolution.x);
            float heightScale = canvasSize.y / Mathf.Max(1f, referenceResolution.y);

            CanvasScaler scaler = context.GetComponentInParent<CanvasScaler>();
            float match = scaler != null ? scaler.matchWidthOrHeight : 0.5f;
            float resolved = Mathf.Lerp(widthScale, heightScale, match);
            if (!float.IsFinite(resolved) || resolved <= 0.01f)
            {
                return 1f;
            }

            return resolved;
        }

        public static float ResolveSoftScale(RectTransform context, float influence = 0.42f, float maxScale = 1.56f)
        {
            float raw = ResolveScale(context);
            float softened = Mathf.Lerp(1f, raw, Mathf.Clamp01(influence));
            if (!float.IsFinite(softened) || softened <= 0.01f)
            {
                return 1f;
            }

            return Mathf.Min(maxScale, softened);
        }

        private static Vector2 ResolveReferenceResolution(RectTransform context)
        {
            CanvasScaler scaler = context.GetComponentInParent<CanvasScaler>();
            if (scaler != null &&
                scaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize &&
                scaler.referenceResolution.x > 0f &&
                scaler.referenceResolution.y > 0f)
            {
                return scaler.referenceResolution;
            }

            return DefaultReferenceResolution;
        }

        private static Vector2 ResolveCanvasSize(RectTransform context, Vector2 fallback)
        {
            Canvas canvas = context.GetComponentInParent<Canvas>();
            RectTransform canvasRect = canvas != null ? canvas.transform as RectTransform : null;

            float width = canvasRect != null && canvasRect.rect.width > 1f
                ? canvasRect.rect.width
                : context.rect.width > 1f ? context.rect.width : (Screen.width > 0 ? Screen.width : fallback.x);

            float height = canvasRect != null && canvasRect.rect.height > 1f
                ? canvasRect.rect.height
                : context.rect.height > 1f ? context.rect.height : (Screen.height > 0 ? Screen.height : fallback.y);

            return new Vector2(width, height);
        }
    }

    public class BattleHUD : MonoBehaviour
    {
        [SerializeField] private Slider enemyBaseHpSlider;
        [SerializeField] private TMP_Text enemyBaseHpText;
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private Slider energySlider;
        [SerializeField] private TMP_Text energyText;
        [SerializeField] private Slider playerHpSlider;
        [SerializeField] private TMP_Text playerHpText;
        [SerializeField] private TMP_Text modeText;
        [SerializeField] private TMP_Text skillText;
        [SerializeField] private TMP_Text passiveText;
        [SerializeField] private TMP_Text warningText;
        [SerializeField] private TMP_Text frontlineText;
        [SerializeField] private TMP_Text bossTacticText;
        [SerializeField] private TMP_Text summonStripText;
        [SerializeField] private TMP_Text statusHeaderText;
        [SerializeField] private RectTransform miniMapPanel;
        [SerializeField] private RectTransform miniMapField;
        [SerializeField] private RectTransform miniMapViewport;
        [SerializeField] private RectTransform miniMapFrontlineMarker;
        [SerializeField] private RectTransform miniMapPlayerMarker;
        [SerializeField] private RectTransform miniMapEnemyBossMarker;
        [SerializeField] private RectTransform miniMapPlayerBaseMarker;
        [SerializeField] private RectTransform miniMapEnemyBaseMarker;
        [SerializeField] private TMP_Text miniMapHintText;
        [SerializeField] private Color normalTimerColor = Color.white;
        [SerializeField] private Color warningTimerColor = Color.red;
        [SerializeField] private Color skillReadyColor = new(0.5f, 1f, 0.72f, 1f);
        [SerializeField] private Color skillCooldownColor = new(1f, 0.86f, 0.45f, 1f);
        [SerializeField] private Color skillUnavailableColor = new(1f, 0.6f, 0.6f, 1f);
        [SerializeField] private Color enemyBaseHealthyColor = new(0.22f, 0.9f, 0.54f, 1f);
        [SerializeField] private Color enemyBaseCriticalColor = new(1f, 0.38f, 0.34f, 1f);
        [SerializeField] private Color energyLowColor = new(0.32f, 0.66f, 1f, 1f);
        [SerializeField] private Color energyHighColor = new(0.22f, 0.95f, 0.62f, 1f);
        [SerializeField] private Color playerHpHealthyColor = new(0.24f, 0.82f, 0.96f, 1f);
        [SerializeField] private Color playerHpCriticalColor = new(1f, 0.48f, 0.48f, 1f);
        [SerializeField] private Color frontlineAdvantageColor = new(0.34f, 0.96f, 0.62f, 1f);
        [SerializeField] private Color frontlineDangerColor = new(1f, 0.42f, 0.42f, 1f);
        [SerializeField] private Color frontlineNeutralColor = new(0.84f, 0.92f, 1f, 1f);
        [SerializeField] private Color miniMapFriendlyColor = new(0.35f, 0.9f, 1f, 1f);
        [SerializeField] private Color miniMapEnemyColor = new(1f, 0.54f, 0.38f, 1f);
        [SerializeField] private Color miniMapViewportColor = new(1f, 1f, 1f, 0.18f);
        [SerializeField] private Color bottomDockColor = new(0.03f, 0.05f, 0.08f, 0.0035f);
        [SerializeField] private Color bottomDockAccentColor = new(0.24f, 0.56f, 0.94f, 0.0016f);
        [SerializeField] private bool allowRuntimeUiBootstrap;

        private BattleManager battleManager;
        private BattleEnergySystem energySystem;
        private PlayerController playerController;
        private PlayerSkillController playerSkillController;
        private PlayerCombatController playerCombatController;
        private BattleCamera battleCamera;
        private EnemyAI enemyAI;
        private Image enemyBaseFillImage;
        private Image energyFillImage;
        private Image playerHpFillImage;
        private bool isSubscribedToBattleManager;
        private bool isSubscribedToEnergySystem;
        private bool isSubscribedToPlayer;
        private bool isSubscribedToSkill;
        private readonly List<Image> friendlyMiniMapMarkers = new();
        private readonly List<Image> enemyMiniMapMarkers = new();
        private RectTransform bottomDockPanel;
        private RectTransform bottomDockAccent;

        private void Awake()
        {
            EnsureCanvasScaling();
            EnsureSupplementalUi();
            RuntimeUIFontUtility.ApplyRecursively(transform);
            ResolveReferences();
            CacheSliderFillImages();
            ApplyResponsiveLayout();
            NormalizeVisibleHudText();
        }

        private void OnEnable()
        {
            EnsureCanvasScaling();
            EnsureSupplementalUi();
            RuntimeUIFontUtility.ApplyRecursively(transform);
            ResolveReferences();
            CacheSliderFillImages();
            ApplyResponsiveLayout();
            NormalizeVisibleHudText();
            TrySubscribe();
        }

        private void OnDisable()
        {
            if (battleManager != null && isSubscribedToBattleManager)
            {
                battleManager.OnEnemyBaseHPChanged -= UpdateEnemyBaseUI;
                isSubscribedToBattleManager = false;
            }

            if (energySystem != null && isSubscribedToEnergySystem)
            {
                energySystem.OnEnergyChanged -= UpdateEnergyUI;
                isSubscribedToEnergySystem = false;
            }

            if (playerController != null && isSubscribedToPlayer)
            {
                playerController.OnHPChanged -= UpdatePlayerHpUI;
                isSubscribedToPlayer = false;
            }

            if (playerSkillController != null && isSubscribedToSkill)
            {
                playerSkillController.OnCooldownChanged -= HandleSkillCooldownChanged;
                playerSkillController.OnSkillActivated -= HandleSkillActivated;
                isSubscribedToSkill = false;
            }
        }

        private void Start()
        {
            ResolveReferences();
            CacheSliderFillImages();
            TrySubscribe();

            if (battleManager != null)
            {
                UpdateEnemyBaseUI(battleManager.CurrentEnemyBaseHP);
            }

            if (energySystem != null)
            {
                UpdateEnergyUI(energySystem.CurrentEnergy, energySystem.MaxEnergy);
            }

            if (playerController != null)
            {
                UpdatePlayerHpUI(playerController.CurrentHP, playerController.MaxHP);
            }

            UpdateTimer();
            UpdateModeUI();
            if (playerController != null && playerController.IsRespawning)
            {
                UpdatePlayerHpUI(playerController.CurrentHP, playerController.MaxHP);
            }
            UpdateSkillUI();
            UpdateWarningUI();
            UpdateFrontlineUI();
            UpdateBossTacticUI();
            UpdateMiniMap();
            ApplyResponsiveLayout();
            NormalizeVisibleHudText();
        }

        private void Update()
        {
            if (playerController == null || battleManager == null || energySystem == null || playerSkillController == null || playerCombatController == null)
            {
                ResolveReferences();
                TrySubscribe();
            }

            UpdateTimer();
            UpdateModeUI();
            UpdateSkillUI();
            UpdateWarningUI();
            UpdateFrontlineUI();
            UpdateBossTacticUI();
            UpdateMiniMap();
            ApplyResponsiveLayout();
            NormalizeVisibleHudText();
        }

        private void ResolveReferences()
        {
            battleManager = BattleManager.Instance;
            energySystem = BattleEnergySystem.Instance;

            if (battleManager != null && battleManager.PlayerController != null)
            {
                playerController = battleManager.PlayerController;
            }
            else if (playerController == null)
            {
                playerController = FindFirstObjectByType<PlayerController>();
            }

            if (playerController != null)
            {
                playerSkillController = playerController.GetComponent<PlayerSkillController>();
                playerCombatController = playerController.GetComponent<PlayerCombatController>();
            }
            else if (playerSkillController == null)
            {
                playerSkillController = FindFirstObjectByType<PlayerSkillController>();
            }

            if (playerCombatController == null)
            {
                playerCombatController = FindFirstObjectByType<PlayerCombatController>();
            }

            if (battleCamera == null)
            {
                battleCamera = FindFirstObjectByType<BattleCamera>();
            }

            if (enemyAI == null)
            {
                enemyAI = FindFirstObjectByType<EnemyAI>();
            }
        }

        private void TrySubscribe()
        {
            if (battleManager != null && !isSubscribedToBattleManager)
            {
                battleManager.OnEnemyBaseHPChanged += UpdateEnemyBaseUI;
                isSubscribedToBattleManager = true;
            }

            if (energySystem != null && !isSubscribedToEnergySystem)
            {
                energySystem.OnEnergyChanged += UpdateEnergyUI;
                isSubscribedToEnergySystem = true;
            }

            if (playerController != null && !isSubscribedToPlayer)
            {
                playerController.OnHPChanged += UpdatePlayerHpUI;
                isSubscribedToPlayer = true;
            }

            if (playerSkillController != null && !isSubscribedToSkill)
            {
                playerSkillController.OnCooldownChanged += HandleSkillCooldownChanged;
                playerSkillController.OnSkillActivated += HandleSkillActivated;
                isSubscribedToSkill = true;
            }
        }

        private void NormalizeVisibleHudText()
        {
            NormalizeEnergyAndHpText();
            NormalizeModeText();
            NormalizeSkillAndPassiveText();
            NormalizeWarningText();
            NormalizeFrontlineText();
            NormalizeBossTacticText();
            NormalizeSupplementalLabels();
        }

        private void NormalizeEnergyAndHpText()
        {
            if (energyText != null)
            {
                if (energySystem != null)
                {
                    energyText.text = $"\uC5D0\uB108\uC9C0  {Mathf.FloorToInt(energySystem.CurrentEnergy)}/{Mathf.FloorToInt(energySystem.MaxEnergy)}";
                }
                else
                {
                    energyText.text = "\uC5D0\uB108\uC9C0  0/0";
                }

                energyText.fontStyle = FontStyles.Bold;
            }

            if (playerHpText != null)
            {
                if (playerController != null && playerController.IsRespawning)
                {
                    playerHpText.text = $"\uC544\uAD70  \uC7AC\uBC30\uCE58 {playerController.RemainingRespawnTime:0.0}\uCD08";
                }
                else if (playerController != null)
                {
                    playerHpText.text = $"\uC544\uAD70  {Mathf.CeilToInt(playerController.CurrentHP)}/{Mathf.CeilToInt(playerController.MaxHP)}";
                }
                else
                {
                    playerHpText.text = "\uC544\uAD70  0/0";
                }

                playerHpText.fontStyle = FontStyles.Bold;
            }
        }

        private void NormalizeModeText()
        {
            if (modeText == null)
            {
                return;
            }

            if (playerController == null)
            {
                modeText.text = "\uB300\uAE30";
                modeText.color = new Color(0.86f, 0.93f, 1f, 0.94f);
                return;
            }

            if (playerController.CurrentMotionState == PlayerMotionState.Retreating ||
                playerController.CurrentLanePressureState == BattleManager.LanePressureState.Collapse)
            {
                modeText.text = "\uD6C4\uD1F4";
                modeText.color = new Color(1f, 0.68f, 0.42f, 0.96f);
                return;
            }

            if (playerController.TryGetManualTargetLock(out _, out _, out _))
            {
                modeText.text = "\uC9D1\uC911 \uACF5\uACA9";
                modeText.color = new Color(1f, 0.78f, 0.4f, 0.98f);
                return;
            }

            if (playerController.CurrentEscortPhase == BattleManager.EscortPhase.Ready)
            {
                modeText.text = "\uB300\uAE30";
                modeText.color = new Color(0.84f, 0.93f, 1f, 0.94f);
                return;
            }

            modeText.text = "\uD638\uC704 \uC911";
            modeText.color = new Color(0.72f, 0.95f, 0.82f, 0.98f);
        }

        private void NormalizeSkillAndPassiveText()
        {
            if (skillText == null || passiveText == null)
            {
                return;
            }

            if (playerSkillController == null)
            {
                skillText.text = "\uBC84\uC2A4\uD2B8  \uB3D9\uAE30\uD654 \uC911";
                skillText.color = skillUnavailableColor;
                passiveText.text = "\uD328\uC2DC\uBE0C  \uB3D9\uAE30\uD654 \uC911";
                return;
            }

            if (playerController != null && playerController.IsRespawning)
            {
                skillText.text = $"\uBC84\uC2A4\uD2B8  \uC7AC\uBC30\uCE58 {playerController.RemainingRespawnTime:0.0}\uCD08";
                skillText.color = skillUnavailableColor;
                passiveText.text = "\uD328\uC2DC\uBE0C  \uC804\uC120 \uC720\uC9C0 \uC911";
                return;
            }

            float energy = energySystem != null ? energySystem.CurrentEnergy : 0f;
            float cost = playerSkillController.ActiveSkillEnergyCost;
            if (playerSkillController.CooldownRemaining > 0.01f)
            {
                skillText.text = $"\uBC84\uC2A4\uD2B8  {playerSkillController.ActiveSkillName}  \uB300\uAE30 {playerSkillController.CooldownRemaining:0.0}\uCD08";
                skillText.color = skillCooldownColor;
            }
            else if (energy < cost)
            {
                float shortage = Mathf.Max(0f, cost - energy);
                skillText.text = $"\uBC84\uC2A4\uD2B8  {playerSkillController.ActiveSkillName}  +{Mathf.CeilToInt(shortage)}E";
                skillText.color = skillUnavailableColor;
            }
            else
            {
                skillText.text = $"\uBC84\uC2A4\uD2B8  {playerSkillController.ActiveSkillName}  \uC900\uBE44";
                skillText.color = skillReadyColor;
            }

            passiveText.text = $"\uD328\uC2DC\uBE0C  {playerSkillController.PassiveSummary}";
        }

        private void NormalizeWarningText()
        {
            if (warningText == null || playerController == null || playerController.MaxHP <= 0.001f)
            {
                return;
            }

            if (playerController.IsRespawning)
            {
                warningText.text = $"{playerController.RemainingRespawnTime:0.0}\uCD08 \uD6C4 \uBCF5\uADC0";
                return;
            }

            if (HasAnyDirectProjectileWarning())
            {
                warningText.text = "\uC9C1\uACA9 \uC704\uD5D8, \uC9C0\uAE08 \uD68C\uD53C";
                return;
            }

            if (!playerController.CurrentLaneHasLiveAllies &&
                (playerController.CurrentMotionState == PlayerMotionState.Retreating || playerController.CurrentLanePressureState == BattleManager.LanePressureState.Collapse))
            {
                warningText.text = "\uAC19\uC740 \uB808\uC778 \uC544\uAD70\uC774 \uC5C6\uC5B4 \uD6C4\uD1F4";
                return;
            }

            if (playerController.CurrentMotionState == PlayerMotionState.Retreating ||
                playerController.CurrentLanePressureState == BattleManager.LanePressureState.Collapse)
            {
                warningText.text = "\uC804\uC120 \uBD95\uAD34, \uB4A4\uB85C \uBCF5\uADC0";
                return;
            }

            if (battleManager != null &&
                battleManager.TryGetLaneCombatState(playerController.EscortLaneIndex, out BattleManager.LaneCombatState laneState) &&
                laneState.HasFrontlineStructure &&
                playerController.CurrentLaneHasLiveAllies)
            {
                warningText.text = $"{playerController.EscortLaneIndex + 1}\uBC88 \uCC28\uB2E8\uBB3C \uD30C\uAD34 \uD544\uC694";
                return;
            }

            if (playerController.TryGetManualTargetLock(out _, out int lockedLaneIndex, out ManualTargetLockKind lockedTargetKind))
            {
                warningText.text = $"{lockedLaneIndex + 1}\uBC88 {ResolveLockedTargetLabelSafe(lockedTargetKind)} \uC9D1\uC911 \uACF5\uACA9";
                return;
            }

            if (playerController.CurrentEscortPhase == BattleManager.EscortPhase.Ready)
            {
                warningText.text = "2\uBC88 \uB610\uB294 4\uBC88 \uB808\uC778\uC5D0 \uC18C\uD658";
                return;
            }

            if (TryFindRewardDirectiveLane(out int rewardLaneIndex))
            {
                warningText.text = $"{rewardLaneIndex + 1}\uBC88 \uBCF4\uC0C1 \uBAA9\uD45C \uB178\uCD9C";
                return;
            }

            if (playerSkillController != null && energySystem != null)
            {
                float shortage = Mathf.Max(0f, playerSkillController.ActiveSkillEnergyCost - energySystem.CurrentEnergy);
                if (shortage > 0.01f)
                {
                    warningText.text = $"\uBC84\uC2A4\uD2B8 \uC5D0\uB108\uC9C0 \uBD80\uC871  +{Mathf.CeilToInt(shortage)}E";
                    return;
                }
            }

            warningText.text = $"{playerController.EscortLaneIndex + 1}\uBC88 \uB808\uC778 \uC720\uC9C0";
        }

        private void NormalizeFrontlineText()
        {
            if (frontlineText == null || battleManager == null || !battleManager.TryGetFrontlineState(out BattleManager.FrontlineState frontlineState))
            {
                return;
            }

            if (frontlineState.PlayerUnitCount == 0 && frontlineState.EnemyUnitCount == 0)
            {
                frontlineText.text = "\uC804\uC120  \uCCAB \uC18C\uD658 \uB300\uAE30";
                return;
            }

            string stateLabel = frontlineState.Balance switch
            {
                >= 0.25f => "\uBC00\uC5B4\uBD99\uC784",
                <= -0.25f => "\uC555\uBC15 \uBC1B\uC74C",
                _ => "\uAD50\uC804 \uC911"
            };

            frontlineText.text = $"\uC804\uC120  {stateLabel}  {frontlineState.PlayerUnitCount}v{frontlineState.EnemyUnitCount}";
        }

        private void NormalizeBossTacticText()
        {
            if (bossTacticText == null)
            {
                return;
            }

            bossTacticText.text = enemyAI == null
                ? "\uC804\uD669  \uB3D9\uAE30\uD654 \uC911..."
                : $"\uC804\uD669  {enemyAI.CurrentBossCue}";
        }

        private void NormalizeSupplementalLabels()
        {
            if (summonStripText != null)
            {
                bool mobileLayout = MobileBattleControls.ShouldUseMobileLayout(transform as RectTransform);
                summonStripText.text = mobileLayout ? "\uC18C\uD658" : "\uC18C\uD658 \uC190\uD328";
            }

            if (miniMapHintText != null)
            {
                bool mobileLayout = MobileBattleControls.ShouldUseMobileLayout(transform as RectTransform);
                bool overviewMode = battleCamera != null && battleCamera.IsOverviewMode;
                miniMapHintText.text = overviewMode
                    ? mobileLayout ? "\uC804\uD669 \uBCF4\uAE30  [\uC804\uD669]" : "\uC804\uD669 \uBCF4\uAE30  [Tab] [\uB4DC\uB798\uADF8]"
                    : mobileLayout ? "\uCD94\uC801 \uD654\uBA74  [\uC804\uD669]" : "\uCD94\uC801 \uD654\uBA74  [Tab]";
            }
        }

        private static string ResolveLockedTargetLabelSafe(ManualTargetLockKind lockedTargetKind)
        {
            return lockedTargetKind switch
            {
                ManualTargetLockKind.Boss => "\uBCF4\uC2A4",
                ManualTargetLockKind.Structure => "\uBAA9\uD45C",
                _ => "\uC801"
            };
        }

        private void UpdateEnemyBaseUI(float currentHp)
        {
            if (battleManager == null)
            {
                return;
            }

            if (enemyBaseHpSlider != null)
            {
                enemyBaseHpSlider.maxValue = battleManager.EnemyBaseMaxHP;
                enemyBaseHpSlider.value = currentHp;
            }

            if (enemyBaseHpText != null)
            {
                enemyBaseHpText.text = $"CORE  {Mathf.CeilToInt(currentHp)}/{Mathf.CeilToInt(battleManager.EnemyBaseMaxHP)}";
                enemyBaseHpText.fontStyle = FontStyles.Bold;
            }

            if (enemyBaseFillImage != null)
            {
                float normalized = battleManager.EnemyBaseMaxHP <= 0.001f ? 0f : currentHp / battleManager.EnemyBaseMaxHP;
                enemyBaseFillImage.color = Color.Lerp(enemyBaseCriticalColor, enemyBaseHealthyColor, normalized);
            }
        }

        private void UpdateEnergyUI(float currentEnergy, float maxEnergy)
        {
            if (energySlider != null)
            {
                energySlider.maxValue = maxEnergy;
                energySlider.value = currentEnergy;
            }

            if (energyText != null)
            {
                energyText.text = $"에너지  {Mathf.FloorToInt(currentEnergy)}/{Mathf.FloorToInt(maxEnergy)}";
                energyText.fontStyle = FontStyles.Bold;
            }

            if (energyFillImage != null)
            {
                float normalized = maxEnergy <= 0.001f ? 0f : currentEnergy / maxEnergy;
                energyFillImage.color = Color.Lerp(energyLowColor, energyHighColor, normalized);
            }

            UpdateSkillUI();
        }

        private void UpdatePlayerHpUI(float currentHp, float maxHp)
        {
            if (playerHpSlider != null)
            {
                playerHpSlider.maxValue = maxHp;
                playerHpSlider.value = currentHp;
            }

            if (playerHpText != null)
            {
                if (playerController != null && playerController.IsRespawning)
                {
                    playerHpText.text = $"아군  재배치 {playerController.RemainingRespawnTime:0.0}초";
                }
                else
                {
                    playerHpText.text = $"아군  {Mathf.CeilToInt(currentHp)}/{Mathf.CeilToInt(maxHp)}";
                }

                playerHpText.fontStyle = FontStyles.Bold;
            }

            if (playerHpFillImage != null)
            {
                if (playerController != null && playerController.IsRespawning)
                {
                    playerHpFillImage.color = Color.Lerp(playerHpCriticalColor, Color.white, 0.18f);
                }
                else
                {
                    float normalized = maxHp <= 0.001f ? 0f : currentHp / maxHp;
                    playerHpFillImage.color = Color.Lerp(playerHpCriticalColor, playerHpHealthyColor, normalized);
                }
            }
        }

        private void UpdateTimer()
        {
            if (timerText == null || GameManager.Instance == null)
            {
                return;
            }

            float remainingTime = Mathf.Max(0f, GameManager.Instance.RemainingTime);
            int minutes = Mathf.FloorToInt(remainingTime / 60f);
            int seconds = Mathf.FloorToInt(remainingTime % 60f);
            timerText.text = $"{minutes:00}:{seconds:00}";
            timerText.color = remainingTime <= 3f ? warningTimerColor : normalTimerColor;
        }

        private void UpdateModeUI()
        {
            if (modeText == null)
            {
                return;
            }

            if (playerController == null)
            {
                modeText.text = "대기";
                modeText.color = new Color(0.86f, 0.93f, 1f, 0.94f);
                return;
            }

            if (playerController.CurrentRetreatReason != PlayerRetreatReason.None)
            {
                modeText.text = "후퇴";
                modeText.color = new Color(1f, 0.68f, 0.42f, 0.96f);
                return;
            }

            if (playerController.TryGetManualTargetLock(out _, out _, out _))
            {
                modeText.text = "집중 공격";
                modeText.color = new Color(1f, 0.78f, 0.4f, 0.98f);
                return;
            }

            if (playerController.CurrentEscortPhase == BattleManager.EscortPhase.Ready)
            {
                modeText.text = "대기";
                modeText.color = new Color(0.84f, 0.93f, 1f, 0.94f);
                return;
            }

            modeText.text = "호위 중";
            modeText.color = new Color(0.72f, 0.95f, 0.82f, 0.98f);
        }

        private void UpdateSkillUI()
        {
            if (skillText == null || passiveText == null)
            {
                return;
            }

            if (playerSkillController == null)
            {
                skillText.text = "버스트  동기화 중";
                skillText.color = skillUnavailableColor;
                passiveText.text = "패시브  동기화 중";
                return;
            }

            if (playerController != null && playerController.IsRespawning)
            {
                skillText.text = $"버스트  재배치 {playerController.RemainingRespawnTime:0.0}초";
                skillText.color = skillUnavailableColor;
                passiveText.text = "패시브  전선 유지 중";
                return;
            }

            float energy = energySystem != null ? energySystem.CurrentEnergy : 0f;
            float cost = playerSkillController.ActiveSkillEnergyCost;
            if (playerSkillController.CooldownRemaining > 0.01f)
            {
                skillText.text = $"버스트  {playerSkillController.ActiveSkillName}  대기 {playerSkillController.CooldownRemaining:0.0}초";
                skillText.color = skillCooldownColor;
            }
            else if (energy < cost)
            {
                float shortage = Mathf.Max(0f, cost - energy);
                skillText.text = $"버스트  {playerSkillController.ActiveSkillName}  +{Mathf.CeilToInt(shortage)}E";
                skillText.color = skillUnavailableColor;
            }
            else
            {
                skillText.text = $"버스트  {playerSkillController.ActiveSkillName}  준비";
                skillText.color = skillReadyColor;
            }

            passiveText.text = $"패시브  {playerSkillController.PassiveSummary}";
        }

        private void HandleSkillCooldownChanged(float _, float __)
        {
            UpdateSkillUI();
        }

        private void HandleSkillActivated()
        {
            UpdateSkillUI();
        }

        private void UpdateWarningUI()
        {
            if (warningText == null)
            {
                return;
            }

            if (playerController == null || playerController.MaxHP <= 0.001f)
            {
                warningText.text = string.Empty;
                return;
            }

            if (playerController.IsRespawning)
            {
                warningText.text = $"{playerController.RemainingRespawnTime:0.0}초 후 복귀";
                warningText.color = Color.Lerp(new Color(1f, 0.62f, 0.36f, 1f), Color.white, 0.18f + (Mathf.Sin(Time.unscaledTime * 8f) * 0.08f));
                return;
            }

            if (HasAnyDirectProjectileDanger(out bool hasActiveProjectileDanger, out bool hasLockingDanger))
            {
                if (hasActiveProjectileDanger)
                {
                    warningText.text = "직격 위험, 지금 회피";
                    warningText.color = Color.Lerp(new Color(1f, 0.5f, 0.34f, 1f), Color.white, 0.32f + (Mathf.Sin(Time.unscaledTime * 12f) * 0.14f));
                    return;
                }

                if (hasLockingDanger)
                {
                    warningText.text = "직격 위험, 지금 회피";
                    warningText.color = new Color(1f, 0.74f, 0.42f, 1f);
                    return;
                }
            }

            switch (playerController.CurrentRetreatReason)
            {
                case PlayerRetreatReason.NoAlliedFrontline:
                    warningText.text = "같은 레인 아군이 없어 후퇴";
                    warningText.color = new Color(1f, 0.58f, 0.42f, 1f);
                    return;

                case PlayerRetreatReason.LaneCollapse:
                    warningText.text = "전선 붕괴, 뒤로 복귀";
                    warningText.color = new Color(1f, 0.58f, 0.42f, 1f);
                    return;

                case PlayerRetreatReason.Overextended:
                    warningText.text = "직선보다 너무 앞섬";
                    warningText.color = new Color(1f, 0.68f, 0.42f, 1f);
                    return;
            }

            if (playerController.TryGetManualTargetLock(out _, out int lockedLaneIndex, out ManualTargetLockKind lockedTargetKind))
            {
                warningText.text = $"{lockedLaneIndex + 1}번 {ResolveLockedTargetLabel(lockedTargetKind)} 집중 공격";
                warningText.color = new Color(1f, 0.78f, 0.4f, 1f);
                return;
            }

            if (playerController.CurrentEscortPhase == BattleManager.EscortPhase.Ready)
            {
                warningText.text = "2번 또는 4번 레인에 소환";
                warningText.color = new Color(0.84f, 0.92f, 1f, 0.9f);
                return;
            }

            if (battleManager != null &&
                battleManager.TryGetLaneCombatState(playerController.EscortLaneIndex, out BattleManager.LaneCombatState laneState) &&
                laneState.EscortPhase == BattleManager.EscortPhase.BlockerHold &&
                playerController.CurrentLaneHasLiveAllies)
            {
                warningText.text = $"{playerController.EscortLaneIndex + 1}번 차단물 파괴 필요";
                warningText.color = new Color(1f, 0.82f, 0.42f, 1f);
                return;
            }

            if (TryFindRewardDirectiveLane(out int rewardLaneIndex))
            {
                warningText.text = $"{rewardLaneIndex + 1}번 보상 목표 노출";
                warningText.color = new Color(0.54f, 1f, 0.78f, 0.98f);
                return;
            }

            if (playerSkillController != null && energySystem != null)
            {
                float shortage = Mathf.Max(0f, playerSkillController.ActiveSkillEnergyCost - energySystem.CurrentEnergy);
                if (shortage > 0.01f)
                {
                    warningText.text = $"버스트 에너지 부족  +{Mathf.CeilToInt(shortage)}E";
                    warningText.color = new Color(1f, 0.84f, 0.42f, 1f);
                    return;
                }
            }

            warningText.text = $"{playerController.EscortLaneIndex + 1}번 레인 유지";
            warningText.color = new Color(0.84f, 0.92f, 1f, 0.88f);
        }

        private bool HasAnyDirectProjectileWarning()
        {
            return HasAnyDirectProjectileDanger(out _, out _);
        }

        private bool HasAnyDirectProjectileDanger(out bool hasActiveProjectileDanger, out bool hasLockingDanger)
        {
            hasActiveProjectileDanger = enemyAI != null &&
                enemyAI.IsDirectProjectileDangerActive &&
                enemyAI.ActiveDirectProjectileCount > 0;
            hasLockingDanger = enemyAI != null && enemyAI.IsDirectProjectileLocking;

            if (PveProjectileEmitter.HasAnyDirectProjectileDanger)
            {
                hasActiveProjectileDanger = true;
            }

            if (PveProjectileEmitter.HasAnyDirectProjectileLocking)
            {
                hasLockingDanger = true;
            }

            return hasActiveProjectileDanger || hasLockingDanger;
        }

        private string ResolveRetreatReason()
        {
            if (playerController == null)
            {
                return "재정비";
            }

            return playerController.CurrentRetreatReason switch
            {
                PlayerRetreatReason.NoAlliedFrontline => "같은 레인 아군이 없습니다",
                PlayerRetreatReason.LaneCollapse => "전선 붕괴로 뒤로 복귀합니다",
                PlayerRetreatReason.Overextended => "직선보다 너무 앞섰습니다",
                _ => "후열로 복귀합니다"
            };
        }

        private bool TryFindRewardDirectiveLane(out int laneIndex)
        {
            laneIndex = -1;
            if (playerController == null)
            {
                return false;
            }

            bool hasDirectiveContext = playerController.CurrentLaneHasLiveAllies ||
                playerController.CurrentEscortPhase == BattleManager.EscortPhase.Join;
            if (!hasDirectiveContext)
            {
                return false;
            }

            int[] priorityLanes =
            {
                BattleLaneUtility.ClampLaneIndex(playerController.EscortLaneIndex),
                1,
                3,
                2
            };

            for (int index = 0; index < priorityLanes.Length; index++)
            {
                int candidateLane = BattleLaneUtility.ClampLaneIndex(priorityLanes[index]);
                if (BattleStructure.FindNearestRoleInLaneAlongAdvance(candidateLane, isPlayerTeam: true, BattleStructureRole.RewardObjective) != null)
                {
                    laneIndex = candidateLane;
                    return true;
                }
            }

            return false;
        }

        private static string ResolveLockedTargetLabel(ManualTargetLockKind lockedTargetKind)
        {
            return lockedTargetKind switch
            {
                ManualTargetLockKind.Boss => "보스",
                ManualTargetLockKind.Structure => "목표",
                _ => "적"
            };
        }

        private void UpdateFrontlineUI()
        {
            if (frontlineText == null || battleManager == null || !battleManager.TryGetFrontlineState(out BattleManager.FrontlineState frontlineState))
            {
                return;
            }

            if (frontlineState.PlayerUnitCount == 0 && frontlineState.EnemyUnitCount == 0)
            {
                frontlineText.text = "FRONT  Waiting for first summon";
                frontlineText.color = frontlineNeutralColor;
                return;
            }

            string stateLabel;
            Color stateColor;
            if (frontlineState.Balance >= 0.25f)
            {
                stateLabel = "Push";
                stateColor = frontlineAdvantageColor;
            }
            else if (frontlineState.Balance <= -0.25f)
            {
                stateLabel = "Under pressure";
                stateColor = frontlineDangerColor;
            }
            else
            {
                stateLabel = "Contested";
                stateColor = frontlineNeutralColor;
            }

            frontlineText.text = $"FRONT  {stateLabel}  {frontlineState.PlayerUnitCount}v{frontlineState.EnemyUnitCount}";
            frontlineText.color = stateColor;
        }

        private void UpdateBossTacticUI()
        {
            if (bossTacticText == null)
            {
                return;
            }

            if (enemyAI == null)
            {
                bossTacticText.text = "PRESSURE  syncing...";
                bossTacticText.color = new Color(0.86f, 0.9f, 1f, 0.88f);
                return;
            }

            bossTacticText.text = $"PRESSURE  {enemyAI.CurrentBossCue}";
            bossTacticText.color = Color.Lerp(enemyAI.CurrentSignalColor, Color.white, 0.16f);
        }

        private void EnsureSupplementalUi()
        {
            RectTransform root = transform as RectTransform;
            if (root == null)
            {
                return;
            }

            TMP_FontAsset font = timerText != null ? timerText.font : TMP_Settings.defaultFontAsset;
            if (font == null)
            {
                return;
            }

            if (bottomDockPanel == null)
            {
                bottomDockPanel = transform.Find("BottomDockPanel") as RectTransform;
                if (bottomDockPanel == null && allowRuntimeUiBootstrap)
                {
                    bottomDockPanel = EnsureRuntimePanel("BottomDockPanel", root, bottomDockColor);
                    Image dockImage = bottomDockPanel.GetComponent<Image>();
                    if (dockImage != null)
                    {
                        dockImage.raycastTarget = false;
                    }
                }
            }

            if (bottomDockPanel != null)
            {
                bottomDockPanel.SetAsFirstSibling();
            }

            if (bottomDockAccent == null && bottomDockPanel != null)
            {
                bottomDockAccent = bottomDockPanel.Find("BottomDockAccent") as RectTransform;
                if (bottomDockAccent == null && allowRuntimeUiBootstrap)
                {
                    bottomDockAccent = EnsureRuntimePanel("BottomDockAccent", bottomDockPanel, bottomDockAccentColor);
                    Image accentImage = bottomDockAccent.GetComponent<Image>();
                    if (accentImage != null)
                    {
                        accentImage.raycastTarget = false;
                    }
                }
            }

            GameObject panel = null;
            if (statusHeaderText == null || modeText == null || skillText == null || passiveText == null || bossTacticText == null)
            {
                Transform existingPanel = transform.Find("BattleStatusPanel");
                panel = existingPanel != null ? existingPanel.gameObject : null;
            }

            if (panel == null && allowRuntimeUiBootstrap &&
                (statusHeaderText == null || modeText == null || skillText == null || passiveText == null || bossTacticText == null))
            {
                panel = new GameObject("BattleStatusPanel", typeof(RectTransform), typeof(Image));
                panel.transform.SetParent(root, false);
                RectTransform panelRect = panel.GetComponent<RectTransform>();
                panelRect.anchorMin = new Vector2(0f, 1f);
                panelRect.anchorMax = new Vector2(0f, 1f);
                panelRect.sizeDelta = new Vector2(420f, 164f);
                panelRect.anchoredPosition = new Vector2(220f, -58f);

                Image background = panel.GetComponent<Image>();
                background.sprite = RuntimeUISpriteUtility.GetPanelSprite();
                background.type = Image.Type.Sliced;
                background.color = new Color(0.03f, 0.06f, 0.1f, 0.62f);
                background.raycastTarget = false;
            }

            RectTransform panelRoot = panel != null ? panel.GetComponent<RectTransform>() : transform.Find("BattleStatusPanel") as RectTransform;
            if (panelRoot == null)
            {
                return;
            }

            bool useMobileLayout = MobileBattleControls.ShouldUseMobileLayout(root);
            bool isOverviewMode = battleCamera != null && battleCamera.IsOverviewMode;
            float uiScale = RuntimeCanvasLayoutUtility.ResolveScale(root);
            ApplyHudLayout(root, panelRoot, useMobileLayout, isOverviewMode, uiScale);

            if (statusHeaderText == null && allowRuntimeUiBootstrap)
            {
                statusHeaderText = CreateRuntimeText("StatusHeaderText", panelRoot, font, 12.5f, new Vector2(12f, -10f), new Vector2(396f, 18f));
            }

            if (modeText == null && allowRuntimeUiBootstrap)
            {
                modeText = CreateRuntimeText("ModeText", panelRoot, font, 20f, new Vector2(12f, -14f), new Vector2(396f, 24f));
            }

            if (skillText == null && allowRuntimeUiBootstrap)
            {
                skillText = CreateRuntimeText("SkillText", panelRoot, font, 18f, new Vector2(12f, -40f), new Vector2(396f, 24f));
            }

            if (passiveText == null && allowRuntimeUiBootstrap)
            {
                passiveText = CreateRuntimeText("PassiveText", panelRoot, font, 15f, new Vector2(12f, -66f), new Vector2(396f, 22f));
            }

            if (warningText == null && allowRuntimeUiBootstrap)
            {
                warningText = CreateRuntimeText("WarningText", panelRoot, font, 15f, new Vector2(12f, -88f), new Vector2(396f, 22f));
            }

            if (frontlineText == null && allowRuntimeUiBootstrap)
            {
                frontlineText = CreateRuntimeText("FrontlineText", panelRoot, font, 15f, new Vector2(12f, -110f), new Vector2(396f, 22f));
            }

            if (bossTacticText == null && allowRuntimeUiBootstrap)
            {
                bossTacticText = CreateRuntimeText("BossTacticText", panelRoot, font, 15f, new Vector2(12f, -132f), new Vector2(396f, 22f));
            }

            if (summonStripText == null && allowRuntimeUiBootstrap)
            {
                summonStripText = CreateRuntimeText("SummonStripText", root, font, 13f, Vector2.zero, new Vector2(180f, 18f));
                summonStripText.alignment = TextAlignmentOptions.Left;
                summonStripText.color = new Color(0.84f, 0.93f, 1f, 0.8f);
                summonStripText.text = "SUMMON";
            }

            ApplyHudLayout(root, panelRoot, useMobileLayout, isOverviewMode, uiScale);
            ApplyStatusTextLayout(panelRoot, useMobileLayout, isOverviewMode, uiScale);

            if (allowRuntimeUiBootstrap)
            {
                EnsureMiniMapUi(root, font);
                EnsureMobileControls(root);
            }
        }

        private void EnsureMiniMapUi(RectTransform root, TMP_FontAsset font)
        {
            if (miniMapPanel == null)
            {
                Transform existingMiniMap = transform.Find("MiniMapPanel");
                GameObject panelObject = existingMiniMap != null ? existingMiniMap.gameObject : new GameObject("MiniMapPanel", typeof(RectTransform), typeof(Image));
                panelObject.transform.SetParent(root, false);
                miniMapPanel = panelObject.GetComponent<RectTransform>();
                miniMapPanel.anchorMin = new Vector2(1f, 1f);
                miniMapPanel.anchorMax = new Vector2(1f, 1f);
                miniMapPanel.pivot = new Vector2(1f, 1f);
                miniMapPanel.sizeDelta = new Vector2(220f, 178f);
                miniMapPanel.anchoredPosition = new Vector2(-22f, -18f);

                Image panelImage = panelObject.GetComponent<Image>();
                panelImage.color = new Color(0f, 0f, 0f, 0.42f);
            }

            if (miniMapField == null)
            {
                GameObject fieldObject = new("MiniMapField", typeof(RectTransform), typeof(Image));
                fieldObject.transform.SetParent(miniMapPanel, false);
                miniMapField = fieldObject.GetComponent<RectTransform>();
                miniMapField.anchorMin = new Vector2(0.5f, 0.5f);
                miniMapField.anchorMax = new Vector2(0.5f, 0.5f);
                miniMapField.sizeDelta = new Vector2(188f, 110f);
                miniMapField.anchoredPosition = new Vector2(0f, -6f);

                Image fieldImage = fieldObject.GetComponent<Image>();
                fieldImage.color = new Color(0.1f, 0.14f, 0.2f, 0.88f);
            }

            if (miniMapViewport == null)
            {
                miniMapViewport = CreateMiniMapRect("Viewport", miniMapField, new Vector2(44f, 22f), miniMapViewportColor, true);
            }

            if (miniMapFrontlineMarker == null)
            {
                miniMapFrontlineMarker = CreateMiniMapRect("Frontline", miniMapField, new Vector2(96f, 4f), frontlineNeutralColor, false);
            }

            if (miniMapPlayerMarker == null)
            {
                miniMapPlayerMarker = CreateMiniMapRect("PlayerMarker", miniMapField, new Vector2(10f, 10f), Color.white, false);
            }

            if (miniMapEnemyBossMarker == null)
            {
                miniMapEnemyBossMarker = CreateMiniMapRect("EnemyBossMarker", miniMapField, new Vector2(12f, 12f), new Color(1f, 0.36f, 0.72f, 1f), false);
            }

            if (miniMapPlayerBaseMarker == null)
            {
                miniMapPlayerBaseMarker = CreateMiniMapRect("PlayerBaseMarker", miniMapField, new Vector2(18f, 7f), new Color(0.34f, 0.78f, 1f, 1f), false);
            }

            if (miniMapEnemyBaseMarker == null)
            {
                miniMapEnemyBaseMarker = CreateMiniMapRect("EnemyBaseMarker", miniMapField, new Vector2(18f, 7f), new Color(1f, 0.44f, 0.38f, 1f), false);
            }

            if (miniMapHintText == null)
            {
                miniMapHintText = CreateRuntimeText("MiniMapHintText", miniMapPanel, font, 14f, new Vector2(14f, -14f), new Vector2(188f, 20f));
            }

            ApplyMiniMapLayout(root, RuntimeCanvasLayoutUtility.ResolveScale(root));

            Transform enemyIntentRoot = transform.Find("EnemyCardStackUI");
            if (enemyIntentRoot is RectTransform enemyIntentRect)
            {
                enemyIntentRect.anchorMin = new Vector2(1f, 1f);
                enemyIntentRect.anchorMax = new Vector2(1f, 1f);
                enemyIntentRect.pivot = new Vector2(1f, 1f);
                enemyIntentRect.anchoredPosition = new Vector2(-16f, -186f);
            }
        }

        private void UpdateMiniMap()
        {
            if (miniMapField == null || battleManager == null)
            {
                return;
            }

            float laneHalfWidth = Mathf.Max(0.1f, battleManager.LaneHalfWidth);
            float laneLength = Mathf.Max(1f, battleManager.LaneLength);

            if (miniMapPlayerBaseMarker != null)
            {
                miniMapPlayerBaseMarker.anchoredPosition = WorldToMiniMap(new Vector3(0f, 0f, 0f), laneHalfWidth, laneLength);
            }

            if (miniMapEnemyBaseMarker != null)
            {
                miniMapEnemyBaseMarker.anchoredPosition = WorldToMiniMap(new Vector3(0f, 0f, laneLength), laneHalfWidth, laneLength);
            }

            if (playerController != null && miniMapPlayerMarker != null)
            {
                miniMapPlayerMarker.anchoredPosition = WorldToMiniMap(playerController.transform.position, laneHalfWidth, laneLength);
            }

            if (enemyAI != null && miniMapEnemyBossMarker != null)
            {
                miniMapEnemyBossMarker.anchoredPosition = WorldToMiniMap(enemyAI.transform.position, laneHalfWidth, laneLength);
            }

            UpdateMiniMapUnitMarkers(laneHalfWidth, laneLength);
            UpdateMiniMapFrontline(laneLength);
            UpdateMiniMapViewport(laneHalfWidth, laneLength);

            if (miniMapHintText != null)
            {
                bool mobileLayout = MobileBattleControls.ShouldUseMobileLayout(transform as RectTransform);
                miniMapHintText.text = battleCamera != null && battleCamera.IsOverviewMode
                    ? mobileLayout ? "전황 보기  [전황]" : "전황 보기  [Tab] [드래그]"
                    : mobileLayout ? "추적 화면  [전황]" : "추적 화면  [Tab]";
                miniMapHintText.color = battleCamera != null && battleCamera.IsOverviewMode ? frontlineAdvantageColor : Color.white;
            }
        }

        private void UpdateMiniMapUnitMarkers(float laneHalfWidth, float laneLength)
        {
            SummonUnit[] units = FindObjectsByType<SummonUnit>(FindObjectsSortMode.None);
            int friendlyIndex = 0;
            int enemyIndex = 0;

            for (int index = 0; index < units.Length; index++)
            {
                SummonUnit summonUnit = units[index];
                if (summonUnit == null || !summonUnit.IsAlive)
                {
                    continue;
                }

                Image marker = summonUnit.IsPlayerTeam
                    ? EnsureMiniMapMarker(friendlyMiniMapMarkers, friendlyIndex++, miniMapFriendlyColor)
                    : EnsureMiniMapMarker(enemyMiniMapMarkers, enemyIndex++, miniMapEnemyColor);

                if (marker == null)
                {
                    continue;
                }

                marker.rectTransform.anchoredPosition = WorldToMiniMap(summonUnit.transform.position, laneHalfWidth, laneLength);
            }

            SetMarkerPoolActive(friendlyMiniMapMarkers, friendlyIndex);
            SetMarkerPoolActive(enemyMiniMapMarkers, enemyIndex);
        }

        private void UpdateMiniMapFrontline(float laneLength)
        {
            if (miniMapFrontlineMarker == null || !battleManager.TryGetFrontlineState(out BattleManager.FrontlineState frontlineState))
            {
                return;
            }

            miniMapFrontlineMarker.anchoredPosition = new Vector2(0f, ((frontlineState.ClashCenterNormalized - 0.5f) * miniMapField.rect.height));
            miniMapFrontlineMarker.sizeDelta = new Vector2(Mathf.Lerp(82f, 140f, Mathf.Abs(frontlineState.Balance)), 4f);

            Image frontlineImage = miniMapFrontlineMarker.GetComponent<Image>();
            if (frontlineImage != null)
            {
                frontlineImage.color = frontlineState.Balance >= 0.25f
                    ? frontlineAdvantageColor
                    : frontlineState.Balance <= -0.25f
                        ? frontlineDangerColor
                        : frontlineNeutralColor;
            }
        }

        private void UpdateMiniMapViewport(float laneHalfWidth, float laneLength)
        {
            if (miniMapViewport == null || battleCamera == null)
            {
                return;
            }

            miniMapViewport.anchoredPosition = WorldToMiniMap(battleCamera.CurrentFocusPosition, laneHalfWidth, laneLength);
            float widthNormalized = Mathf.Clamp01(battleCamera.EstimatedVisibleWidth / Mathf.Max(0.1f, laneHalfWidth * 2f));
            float lengthNormalized = Mathf.Clamp01(battleCamera.EstimatedVisibleLength / Mathf.Max(0.1f, laneLength));
            miniMapViewport.sizeDelta = new Vector2(
                Mathf.Max(18f, miniMapField.rect.width * widthNormalized),
                Mathf.Max(16f, miniMapField.rect.height * lengthNormalized));
        }

        private Vector2 WorldToMiniMap(Vector3 worldPosition, float laneHalfWidth, float laneLength)
        {
            float normalizedX = Mathf.InverseLerp(-laneHalfWidth, laneHalfWidth, worldPosition.x);
            float normalizedY = Mathf.InverseLerp(0f, laneLength, worldPosition.z);
            return new Vector2(
                (normalizedX - 0.5f) * miniMapField.rect.width,
                (normalizedY - 0.5f) * miniMapField.rect.height);
        }

        private Image EnsureMiniMapMarker(List<Image> pool, int index, Color color)
        {
            while (pool.Count <= index)
            {
                RectTransform markerRect = CreateMiniMapRect($"Unit_{pool.Count}", miniMapField, new Vector2(6f, 6f), color, false);
                Image markerImage = markerRect.GetComponent<Image>();
                pool.Add(markerImage);
            }

            Image marker = pool[index];
            if (marker != null)
            {
                marker.color = color;
                marker.gameObject.SetActive(true);
            }

            return marker;
        }

        private static void SetMarkerPoolActive(List<Image> pool, int activeCount)
        {
            for (int index = 0; index < pool.Count; index++)
            {
                if (pool[index] != null)
                {
                    pool[index].gameObject.SetActive(index < activeCount);
                }
            }
        }

        private static RectTransform CreateMiniMapRect(string name, RectTransform parent, Vector2 size, Color color, bool setAsFirstSibling)
        {
            GameObject markerObject = new(name, typeof(RectTransform), typeof(Image));
            markerObject.transform.SetParent(parent, false);
            RectTransform rect = markerObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;

            Image image = markerObject.GetComponent<Image>();
            image.sprite = RuntimeUISpriteUtility.GetPanelSprite();
            image.type = Image.Type.Sliced;
            image.color = color;

            if (setAsFirstSibling)
            {
                rect.SetAsFirstSibling();
            }

            return rect;
        }

        private void CacheSliderFillImages()
        {
            enemyBaseFillImage = ResolveFillImage(enemyBaseHpSlider);
            energyFillImage = ResolveFillImage(energySlider);
            playerHpFillImage = ResolveFillImage(playerHpSlider);
        }

        private static Image ResolveFillImage(Slider slider)
        {
            if (slider == null || slider.fillRect == null)
            {
                return null;
            }

            return slider.fillRect.GetComponent<Image>();
        }

        private static TMP_Text CreateRuntimeText(string name, RectTransform parent, TMP_FontAsset font, float fontSize, Vector2 anchoredPosition, Vector2 size)
        {
            Transform existing = parent.Find(name);
            GameObject textObject = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(parent, false);

            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
            text.font = font;
            text.fontSize = fontSize;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Left;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.raycastTarget = false;
            RuntimeUIFontUtility.ApplyToText(text);
            return text;
        }

        private static RectTransform EnsureRuntimePanel(string name, RectTransform parent, Color tint)
        {
            Transform existing = parent.Find(name);
            GameObject gameObject = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform), typeof(Image));
            gameObject.transform.SetParent(parent, false);

            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.localScale = Vector3.one;

            Image image = gameObject.GetComponent<Image>();
            image.sprite = RuntimeUISpriteUtility.GetPanelSprite();
            image.type = Image.Type.Sliced;
            image.color = tint;
            image.raycastTarget = false;
            return rect;
        }

        private void EnsureMobileControls(RectTransform root)
        {
            if (root == null)
            {
                return;
            }

            Transform existing = root.Find("MobileBattleControls");
            GameObject controlsObject = existing != null
                ? existing.gameObject
                : new GameObject("MobileBattleControls", typeof(RectTransform), typeof(MobileBattleControls));
            controlsObject.transform.SetParent(root, false);

            RectTransform controlsRect = controlsObject.GetComponent<RectTransform>();
            controlsRect.anchorMin = Vector2.zero;
            controlsRect.anchorMax = Vector2.one;
            controlsRect.offsetMin = Vector2.zero;
            controlsRect.offsetMax = Vector2.zero;
            controlsRect.SetAsLastSibling();
        }

        private void EnsureCanvasScaling()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                return;
            }

            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                return;
            }

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(720f, 1280f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.6f;
        }

        private void ApplyResponsiveLayout()
        {
            RectTransform root = transform as RectTransform;
            if (root == null)
            {
                return;
            }

            float uiScale = RuntimeCanvasLayoutUtility.ResolveScale(root);
            ResolveSafeAreaPadding(root, out float safeLeft, out float safeRight, out float safeTop, out float safeBottom);
            bool useMobileLayout = MobileBattleControls.ShouldUseMobileLayout(root);
            float layoutWidth = ResolveLayoutWidth(root);
            bool compactMobile = useMobileLayout && layoutWidth <= 620f;
            float mobileSideInset = useMobileLayout ? (compactMobile ? 10f : 14f) : 0f;
            float mobileLeftActionReserve = 0f;
            float mobileRightActionReserve = 0f;
            float mobileContentCenterOffset = 0f;
            if (useMobileLayout)
            {
                mobileLeftActionReserve = compactMobile ? 20f : 26f;
                mobileRightActionReserve = compactMobile ? 120f : 132f;
            }

            RectTransform panelRoot = transform.Find("BattleStatusPanel") as RectTransform;
            if (panelRoot != null)
            {
                ApplyHudLayout(root, panelRoot, useMobileLayout, battleCamera != null && battleCamera.IsOverviewMode, uiScale);
                panelRoot.anchoredPosition = new Vector2(safeLeft + (16f * uiScale), -(safeTop + ((useMobileLayout ? 68f : 72f) * uiScale)));
                ApplyStatusTextLayout(panelRoot, useMobileLayout, battleCamera != null && battleCamera.IsOverviewMode, uiScale);
            }

            ApplyMiniMapLayout(root, uiScale);
            if (miniMapPanel != null)
            {
                miniMapPanel.anchoredPosition = new Vector2(-safeRight - (16f * uiScale), -safeTop - (16f * uiScale));
            }

            ApplyBottomDockLayout(root, safeLeft, safeRight, safeBottom, useMobileLayout, uiScale);
            ApplyTopBarsLayout(root, safeLeft, safeRight, safeTop, safeBottom, useMobileLayout, mobileSideInset, mobileLeftActionReserve, mobileRightActionReserve, mobileContentCenterOffset, uiScale);
            ApplyHandLayout(root, safeLeft, safeRight, safeBottom, useMobileLayout, mobileSideInset, mobileLeftActionReserve, mobileRightActionReserve, mobileContentCenterOffset, uiScale);
            ApplyEnemyIntentLayout(safeRight, safeTop, useMobileLayout, uiScale);
            ApplyMobileHudVisibility(useMobileLayout, battleCamera != null && battleCamera.IsOverviewMode);
        }

        private void ApplyBottomDockLayout(RectTransform root, float safeLeft, float safeRight, float safeBottom, bool useMobileLayout, float uiScale)
        {
            if (bottomDockPanel == null)
            {
                return;
            }

            if (!useMobileLayout)
            {
                bottomDockPanel.gameObject.SetActive(false);
                return;
            }

            bottomDockPanel.gameObject.SetActive(true);
            bottomDockPanel.SetAsFirstSibling();

            float rootWidth = ResolveLayoutWidth(root);
            float mobileWidthBlend = Mathf.Clamp01((rootWidth - 360f) / 280f);
            float layoutScale = useMobileLayout ? RuntimeCanvasLayoutUtility.ResolveSoftScale(root, 0.38f, 1.48f) : uiScale;
            float dockVisibleHeight = Mathf.Lerp(34f, 40f, mobileWidthBlend) * layoutScale;
            float dockTopPadding = Mathf.Lerp(0.75f, 1.25f, mobileWidthBlend) * layoutScale;
            float dockHeight = dockVisibleHeight + dockTopPadding;
            float dockSideInset = Mathf.Lerp(10f, 16f, mobileWidthBlend) * layoutScale;

            bottomDockPanel.anchorMin = new Vector2(0f, 0f);
            bottomDockPanel.anchorMax = new Vector2(1f, 0f);
            bottomDockPanel.pivot = new Vector2(0.5f, 0f);
            bottomDockPanel.offsetMin = new Vector2(safeLeft + dockSideInset, safeBottom);
            bottomDockPanel.offsetMax = new Vector2(-(safeRight + dockSideInset), safeBottom + dockHeight);

            Image dockImage = bottomDockPanel.GetComponent<Image>();
            if (dockImage != null)
            {
                dockImage.color = bottomDockColor;
                dockImage.raycastTarget = false;
            }

            if (bottomDockAccent != null)
            {
                bottomDockAccent.anchorMin = new Vector2(0f, 1f);
                bottomDockAccent.anchorMax = new Vector2(1f, 1f);
                bottomDockAccent.pivot = new Vector2(0.5f, 1f);
                float accentInset = Mathf.Lerp(12f, 18f, mobileWidthBlend) * uiScale;
                float accentHeight = Mathf.Lerp(3f, 4f, mobileWidthBlend) * uiScale;
                bottomDockAccent.offsetMin = new Vector2(accentInset, -accentHeight);
                bottomDockAccent.offsetMax = new Vector2(-accentInset, 0f);
                bottomDockAccent.SetAsLastSibling();

                Image accentImage = bottomDockAccent.GetComponent<Image>();
                if (accentImage != null)
                {
                    Color accentTint = bottomDockAccentColor;
                    accentTint.a = Mathf.Lerp(0.014f, 0.026f, mobileWidthBlend);
                    accentImage.color = accentTint;
                    accentImage.raycastTarget = false;
                }
            }
        }

        private void ApplyTopBarsLayout(RectTransform root, float safeLeft, float safeRight, float safeTop, float safeBottom, bool useMobileLayout, float mobileSideInset, float mobileLeftActionReserve, float mobileRightActionReserve, float mobileContentCenterOffset, float uiScale)
        {
            float rootWidth = ResolveLayoutWidth(root);
            float mobileWidthBlend = useMobileLayout ? Mathf.Clamp01((rootWidth - 360f) / 280f) : 1f;
            float layoutScale = useMobileLayout ? RuntimeCanvasLayoutUtility.ResolveSoftScale(root, 0.38f, 1.48f) : uiScale;
            float handBottomMargin = (useMobileLayout ? Mathf.Lerp(10f, 14f, mobileWidthBlend) : 18f) * layoutScale;
            float handHeight = (useMobileLayout ? Mathf.Lerp(138f, 146f, mobileWidthBlend) : 152f) * layoutScale;
            float handCenterY = safeBottom + handBottomMargin + (handHeight * 0.5f);
            float handTopY = handCenterY + (handHeight * 0.5f);
            float energySliderY = handTopY + (useMobileLayout ? Mathf.Lerp(10f, 14f, mobileWidthBlend) : 20f) * layoutScale;
            float energyTextY = energySliderY + (9f * layoutScale);
            float energyWidth = useMobileLayout
                ? Mathf.Clamp(rootWidth - safeLeft - safeRight - mobileLeftActionReserve - mobileRightActionReserve - (mobileSideInset * 2f) - (12f * layoutScale), Mathf.Lerp(188f, 212f, mobileWidthBlend) * layoutScale, 360f * layoutScale)
                : Mathf.Clamp(rootWidth - safeLeft - safeRight - (420f * uiScale), 240f * uiScale, 420f * uiScale);
            float playerHpWidth = (useMobileLayout ? Mathf.Lerp(104f, 118f, mobileWidthBlend) : 156f) * layoutScale;
            float playerHpHeight = (useMobileLayout ? Mathf.Lerp(14f, 16f, mobileWidthBlend) : 22f) * layoutScale;
            float playerHpCenterX = safeLeft + (playerHpWidth * 0.5f) + ((useMobileLayout ? Mathf.Lerp(12f, 16f, mobileWidthBlend) : 24f) * layoutScale);
            float playerHpBarY = useMobileLayout ? energySliderY : safeBottom + (150f * uiScale);
            float playerHpTextY = useMobileLayout ? energyTextY : safeBottom + (176f * uiScale);

            if (enemyBaseHpSlider != null)
            {
                RectTransform sliderRect = enemyBaseHpSlider.transform as RectTransform;
                if (sliderRect != null)
                {
                    sliderRect.anchorMin = new Vector2(0.5f, 1f);
                    sliderRect.anchorMax = new Vector2(0.5f, 1f);
                    float reservedWidth = useMobileLayout ? mobileRightActionReserve + (20f * uiScale) : 180f * uiScale;
                    sliderRect.sizeDelta = new Vector2(Mathf.Clamp(rootWidth - safeLeft - safeRight - reservedWidth, 220f * layoutScale, 420f * layoutScale), (useMobileLayout ? Mathf.Lerp(20f, 23f, mobileWidthBlend) : 30f) * layoutScale);
                    sliderRect.anchoredPosition = new Vector2(0f, -safeTop - (24f * layoutScale));
                }
            }

            if (enemyBaseHpText != null)
            {
                RectTransform textRect = enemyBaseHpText.rectTransform;
                textRect.anchorMin = new Vector2(0.5f, 1f);
                textRect.anchorMax = new Vector2(0.5f, 1f);
                float reservedWidth = useMobileLayout ? mobileRightActionReserve + (20f * uiScale) : 180f * uiScale;
                textRect.sizeDelta = new Vector2(Mathf.Clamp(rootWidth - safeLeft - safeRight - reservedWidth, 220f * layoutScale, 420f * layoutScale), 28f * layoutScale);
                textRect.anchoredPosition = new Vector2(0f, -safeTop - (4f * layoutScale));
                enemyBaseHpText.fontSize = (useMobileLayout ? Mathf.Lerp(13.5f, 15f, mobileWidthBlend) : 17f) * layoutScale;
                enemyBaseHpText.alignment = TextAlignmentOptions.Center;
            }

            if (timerText != null)
            {
                RectTransform timerRect = timerText.rectTransform;
                timerRect.anchorMin = new Vector2(0.5f, 1f);
                timerRect.anchorMax = new Vector2(0.5f, 1f);
                timerRect.anchoredPosition = new Vector2(0f, -safeTop - (56f * layoutScale));
                timerText.fontSize = (useMobileLayout ? Mathf.Lerp(24f, 28f, mobileWidthBlend) : 35f) * layoutScale;
                timerText.fontStyle = FontStyles.Bold;
                timerText.alignment = TextAlignmentOptions.Center;
            }

            if (energySlider != null)
            {
                RectTransform sliderRect = energySlider.transform as RectTransform;
                if (sliderRect != null)
                {
                    sliderRect.anchorMin = new Vector2(0.5f, 0f);
                    sliderRect.anchorMax = new Vector2(0.5f, 0f);
                    sliderRect.sizeDelta = new Vector2(energyWidth, (useMobileLayout ? Mathf.Lerp(14f, 16f, mobileWidthBlend) : 28f) * layoutScale);
                    sliderRect.anchoredPosition = new Vector2(useMobileLayout ? mobileContentCenterOffset : 0f, useMobileLayout ? energySliderY : safeBottom + (150f * uiScale));
                }
            }

            if (energyText != null)
            {
                RectTransform textRect = energyText.rectTransform;
                textRect.anchorMin = new Vector2(0.5f, 0f);
                textRect.anchorMax = new Vector2(0.5f, 0f);
                textRect.sizeDelta = new Vector2(energyWidth, 26f * layoutScale);
                textRect.anchoredPosition = new Vector2(useMobileLayout ? mobileContentCenterOffset : 0f, useMobileLayout ? energyTextY : safeBottom + (176f * uiScale));
                energyText.fontSize = (useMobileLayout ? Mathf.Lerp(11.8f, 13f, mobileWidthBlend) : 16.5f) * layoutScale;
                energyText.alignment = TextAlignmentOptions.Center;
            }

            if (playerHpSlider != null)
            {
                RectTransform sliderRect = playerHpSlider.transform as RectTransform;
                if (sliderRect != null)
                {
                    sliderRect.anchorMin = new Vector2(0f, 0f);
                    sliderRect.anchorMax = new Vector2(0f, 0f);
                    sliderRect.pivot = new Vector2(0.5f, 0.5f);
                    sliderRect.sizeDelta = new Vector2(playerHpWidth, playerHpHeight);
                    sliderRect.anchoredPosition = new Vector2(playerHpCenterX, playerHpBarY);
                }
            }

            if (playerHpText != null)
            {
                RectTransform textRect = playerHpText.rectTransform;
                textRect.anchorMin = new Vector2(0f, 0f);
                textRect.anchorMax = new Vector2(0f, 0f);
                textRect.pivot = new Vector2(0.5f, 0.5f);
                textRect.sizeDelta = new Vector2(playerHpWidth + (18f * layoutScale), 22f * layoutScale);
                textRect.anchoredPosition = new Vector2(playerHpCenterX, playerHpTextY);
                playerHpText.fontSize = (useMobileLayout ? Mathf.Lerp(12f, 13.2f, mobileWidthBlend) : 15.5f) * layoutScale;
                playerHpText.alignment = TextAlignmentOptions.Center;
            }
        }

        private void ApplyHandLayout(RectTransform root, float safeLeft, float safeRight, float safeBottom, bool useMobileLayout, float mobileSideInset, float mobileLeftActionReserve, float mobileRightActionReserve, float mobileContentCenterOffset, float uiScale)
        {
            RectTransform handRect = transform.Find("CardHandUI") as RectTransform;
            if (handRect == null)
            {
                return;
            }

            if (useMobileLayout)
            {
                handRect.SetAsLastSibling();
            }

            RectTransform slotRect = FindHandSlotContent(handRect);
            Image handImage = handRect.GetComponent<Image>();
            float rootWidth = ResolveLayoutWidth(root);
            float availableWidth = rootWidth - safeLeft - safeRight;
            float mobileWidthBlend = useMobileLayout ? Mathf.Clamp01((rootWidth - 360f) / 280f) : 1f;
            float layoutScale = useMobileLayout ? RuntimeCanvasLayoutUtility.ResolveSoftScale(root, 0.38f, 1.48f) : uiScale;
            float handHeight = (useMobileLayout ? Mathf.Lerp(140f, 148f, mobileWidthBlend) : 152f) * layoutScale;
            float handBottomMargin = (useMobileLayout ? Mathf.Lerp(10f, 14f, mobileWidthBlend) : 18f) * layoutScale;
            float handCenterY = safeBottom + handBottomMargin + (handHeight * 0.5f);

            if (useMobileLayout)
            {
                handRect.anchorMin = new Vector2(0.5f, 0f);
                handRect.anchorMax = new Vector2(0.5f, 0f);
                handRect.pivot = new Vector2(0.5f, 0.5f);

                float handWidth = Mathf.Clamp(availableWidth - ((mobileSideInset * 2f) * layoutScale), 280f * layoutScale, 880f * layoutScale);
                handRect.sizeDelta = new Vector2(handWidth, handHeight);
                handRect.anchoredPosition = new Vector2(mobileContentCenterOffset, handCenterY);
                if (handImage != null)
                {
                    handImage.color = new Color(0f, 0f, 0f, 0.0035f);
                    handImage.raycastTarget = false;
                }

                if (slotRect != null)
                {
                    slotRect.localScale = Vector3.one;
                }
            }
            else
            {
                handRect.anchorMin = new Vector2(0.5f, 0f);
                handRect.anchorMax = new Vector2(0.5f, 0f);
                handRect.pivot = new Vector2(0.5f, 0.5f);
                handRect.sizeDelta = new Vector2(820f * uiScale, 140f * uiScale);
                handRect.anchoredPosition = new Vector2(0f, safeBottom + (60f * uiScale));
                if (handImage != null)
                {
                    handImage.color = new Color(0f, 0f, 0f, 0.35f);
                    handImage.raycastTarget = false;
                }

                if (slotRect != null)
                {
                    slotRect.localScale = Vector3.one;
                }
            }

            if (summonStripText != null)
            {
                RectTransform labelRect = summonStripText.rectTransform;
                if (useMobileLayout)
                {
                    labelRect.anchorMin = new Vector2(0.5f, 0f);
                    labelRect.anchorMax = new Vector2(0.5f, 0f);
                    labelRect.pivot = new Vector2(0f, 0.5f);
                    float handLeft = handRect.anchoredPosition.x - (handRect.sizeDelta.x * 0.5f);
                    labelRect.anchoredPosition = new Vector2(handLeft + (12f * layoutScale), handRect.anchoredPosition.y + (handRect.sizeDelta.y * 0.5f) + (Mathf.Lerp(6f, 8f, mobileWidthBlend) * layoutScale));
                    summonStripText.alignment = TextAlignmentOptions.Left;
                }
                else
                {
                    labelRect.anchorMin = new Vector2(0.5f, 0f);
                    labelRect.anchorMax = new Vector2(0.5f, 0f);
                    labelRect.pivot = new Vector2(0f, 0.5f);
                    float handLeft = handRect.anchoredPosition.x - (handRect.sizeDelta.x * 0.5f);
                    labelRect.anchoredPosition = new Vector2(handLeft + (12f * uiScale), handRect.anchoredPosition.y + (handRect.sizeDelta.y * 0.5f) + (16f * uiScale));
                    summonStripText.alignment = TextAlignmentOptions.Left;
                }
                labelRect.sizeDelta = new Vector2(Mathf.Min(handRect.sizeDelta.x - (24f * layoutScale), 260f * layoutScale), 20f * layoutScale);
                summonStripText.fontSize = (useMobileLayout ? Mathf.Lerp(13.5f, 14.5f, mobileWidthBlend) : 14f) * layoutScale;
                summonStripText.text = useMobileLayout ? "소환" : "소환 손패";
                summonStripText.raycastTarget = false;
                summonStripText.gameObject.SetActive(true);
                if (useMobileLayout)
                {
                    summonStripText.rectTransform.SetAsLastSibling();
                }
            }
        }

        private static RectTransform FindHandSlotContent(RectTransform handRect)
        {
            if (handRect == null)
            {
                return null;
            }

            Transform direct = handRect.Find("Slots");
            if (direct is RectTransform directRect)
            {
                return directRect;
            }

            Transform viewport = handRect.Find("Viewport/Slots");
            return viewport as RectTransform;
        }

        private void ApplyEnemyIntentLayout(float safeRight, float safeTop, bool useMobileLayout, float uiScale)
        {
            RectTransform enemyIntentRect = transform.Find("EnemyCardStackUI") as RectTransform;
            if (enemyIntentRect == null)
            {
                return;
            }

            enemyIntentRect.anchorMin = new Vector2(1f, 1f);
            enemyIntentRect.anchorMax = new Vector2(1f, 1f);
            enemyIntentRect.pivot = new Vector2(1f, 1f);
            enemyIntentRect.sizeDelta = (useMobileLayout ? new Vector2(150f, 148f) : new Vector2(180f, 180f)) * uiScale;
            enemyIntentRect.anchoredPosition = new Vector2(
                -safeRight - (16f * uiScale),
                -safeTop - ((useMobileLayout ? 156f : 186f) * uiScale));
        }

        private static void ResolveSafeAreaPadding(RectTransform root, out float safeLeft, out float safeRight, out float safeTop, out float safeBottom)
        {
            float rootWidth = ResolveLayoutWidth(root);
            float rootHeight = ResolveLayoutHeight(root);
            Rect safeArea = Screen.safeArea;

            safeLeft = Screen.width <= 0 ? 0f : rootWidth * (safeArea.xMin / Screen.width);
            safeRight = Screen.width <= 0 ? 0f : rootWidth * (1f - (safeArea.xMax / Screen.width));
            safeBottom = Screen.height <= 0 ? 0f : rootHeight * (safeArea.yMin / Screen.height);
            safeTop = Screen.height <= 0 ? 0f : rootHeight * (1f - (safeArea.yMax / Screen.height));
        }

        private static float ResolveLayoutWidth(RectTransform root)
        {
            return root != null && root.rect.width > 1f
                ? root.rect.width
                : Screen.width > 0 ? Screen.width : 1080f;
        }

        private static float ResolveLayoutHeight(RectTransform root)
        {
            return root != null && root.rect.height > 1f
                ? root.rect.height
                : Screen.height > 0 ? Screen.height : 1920f;
        }

        private void ApplyHudLayout(RectTransform root, RectTransform panelRoot, bool useMobileLayout, bool isOverviewMode, float uiScale)
        {
            float rootWidth = root.rect.width > 1f ? root.rect.width : 1080f;
            float panelWidth = useMobileLayout
                ? Mathf.Clamp(rootWidth * 0.56f, 248f * uiScale, 352f * uiScale)
                : Mathf.Clamp(rootWidth * 0.42f, 320f * uiScale, 384f * uiScale);
            float panelHeight = useMobileLayout
                ? 70f * uiScale
                : 102f * uiScale;

            panelRoot.anchorMin = new Vector2(0f, 1f);
            panelRoot.anchorMax = new Vector2(0f, 1f);
            panelRoot.pivot = new Vector2(0f, 1f);
            panelRoot.sizeDelta = new Vector2(panelWidth, panelHeight);
            panelRoot.anchoredPosition = new Vector2(14f * uiScale, -68f * uiScale);

            Image background = panelRoot.GetComponent<Image>();
            if (background != null)
            {
                background.sprite = RuntimeUISpriteUtility.GetPanelSprite();
                background.type = Image.Type.Sliced;
                background.color = useMobileLayout
                    ? new Color(0.03f, 0.06f, 0.1f, isOverviewMode ? 0.3f : 0.22f)
                    : new Color(0.03f, 0.06f, 0.1f, 0.56f);
                background.raycastTarget = false;
            }
        }

        private void ApplyStatusTextLayout(RectTransform panelRoot, bool useMobileLayout, bool isOverviewMode, float uiScale)
        {
            float contentWidth = panelRoot.sizeDelta.x - (20f * uiScale);
            if (statusHeaderText != null)
            {
                SetTextActive(statusHeaderText, false);
            }

            float modeFont = (useMobileLayout ? 16.5f : 18.5f) * uiScale;
            float actionFont = (useMobileLayout ? 14.5f : 16f) * uiScale;
            ConfigureStatusText(modeText, panelRoot, modeFont, new Vector2(12f, -12f) * uiScale, new Vector2(contentWidth, 24f * uiScale), false);
            ConfigureStatusText(warningText, panelRoot, actionFont, new Vector2(12f, -34f) * uiScale, new Vector2(contentWidth, 22f * uiScale), false);
            SetTextActive(modeText, true);
            SetTextActive(warningText, true);
            SetTextActive(frontlineText, false);
            SetTextActive(skillText, false);
            SetTextActive(passiveText, false);
            SetTextActive(bossTacticText, false);

            if (modeText != null)
            {
                modeText.fontStyle = FontStyles.Bold;
            }

            if (warningText != null)
            {
                warningText.fontStyle = FontStyles.Bold;
            }
        }

        private static void ConfigureStatusText(TMP_Text text, RectTransform parent, float fontSize, Vector2 anchoredPosition, Vector2 size, bool wrap)
        {
            if (text == null)
            {
                return;
            }

            text.transform.SetParent(parent, false);
            RectTransform rect = text.rectTransform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            text.fontSize = fontSize;
            text.alignment = TextAlignmentOptions.Left;
            text.textWrappingMode = wrap ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Ellipsis;
        }

        private static void SetTextActive(TMP_Text text, bool isActive)
        {
            if (text != null && text.gameObject.activeSelf != isActive)
            {
                text.gameObject.SetActive(isActive);
            }
        }

        private void ApplyMobileHudVisibility(bool useMobileLayout, bool isOverviewMode)
        {
            if (miniMapPanel != null)
            {
                bool showMiniMap = !useMobileLayout || isOverviewMode;
                if (miniMapPanel.gameObject.activeSelf != showMiniMap)
                {
                    miniMapPanel.gameObject.SetActive(showMiniMap);
                }
            }

            RectTransform enemyIntentRect = transform.Find("EnemyCardStackUI") as RectTransform;
            if (enemyIntentRect != null)
            {
                bool showEnemyIntent = isOverviewMode;
                if (enemyIntentRect.gameObject.activeSelf != showEnemyIntent)
                {
                    enemyIntentRect.gameObject.SetActive(showEnemyIntent);
                }
            }

            if (playerHpSlider != null)
            {
                bool showHpBar = true;
                if (playerHpSlider.gameObject.activeSelf != showHpBar)
                {
                    playerHpSlider.gameObject.SetActive(showHpBar);
                }
            }

            if (playerHpText != null)
            {
                bool showHpText = true;
                if (playerHpText.gameObject.activeSelf != showHpText)
                {
                    playerHpText.gameObject.SetActive(showHpText);
                }
            }
        }

        private void ApplyMiniMapLayout(RectTransform root, float uiScale)
        {
            if (miniMapPanel == null)
            {
                return;
            }

            float rootWidth = root.rect.width > 1f ? root.rect.width : 1080f;
            float panelWidth = (rootWidth < 900f ? 188f : 208f) * uiScale;
            float panelHeight = (rootWidth < 900f ? 146f : 164f) * uiScale;

            miniMapPanel.anchorMin = new Vector2(1f, 1f);
            miniMapPanel.anchorMax = new Vector2(1f, 1f);
            miniMapPanel.pivot = new Vector2(1f, 1f);
            miniMapPanel.sizeDelta = new Vector2(panelWidth, panelHeight);
            miniMapPanel.anchoredPosition = new Vector2(-16f * uiScale, -16f * uiScale);

            if (miniMapField != null)
            {
                miniMapField.anchorMin = new Vector2(0.5f, 0.5f);
                miniMapField.anchorMax = new Vector2(0.5f, 0.5f);
                miniMapField.pivot = new Vector2(0.5f, 0.5f);
                miniMapField.sizeDelta = new Vector2(panelWidth - (24f * uiScale), panelHeight - (50f * uiScale));
                miniMapField.anchoredPosition = new Vector2(0f, -6f * uiScale);
            }

            if (miniMapHintText != null)
            {
                miniMapHintText.transform.SetParent(miniMapPanel, false);
                RectTransform hintRect = miniMapHintText.rectTransform;
                hintRect.anchorMin = new Vector2(0f, 1f);
                hintRect.anchorMax = new Vector2(0f, 1f);
                hintRect.pivot = new Vector2(0f, 1f);
                hintRect.anchoredPosition = new Vector2(10f, -10f) * uiScale;
                hintRect.sizeDelta = new Vector2(panelWidth - (20f * uiScale), 18f * uiScale);
                miniMapHintText.fontSize = 12f * uiScale;
                miniMapHintText.alignment = TextAlignmentOptions.Left;
                miniMapHintText.overflowMode = TextOverflowModes.Ellipsis;
                miniMapHintText.textWrappingMode = TextWrappingModes.NoWrap;
            }
        }
    }

    public enum MobileMoveInputMode
    {
        Joystick = 0,
        TapAssist = 1
    }

    public class MobileBattleControls : MonoBehaviour
    {
        public enum SummonPlacementFailureReason
        {
            None = 0,
            LaneNotSelected = 1
        }

        [SerializeField] private Color panelTint = new(0.05f, 0.08f, 0.14f, 0.42f);
        [SerializeField] private Color stickReadyTint = new(0.34f, 0.86f, 1f, 0.28f);
        [SerializeField] private Color stickActiveTint = new(0.42f, 0.95f, 1f, 0.46f);
        [SerializeField] private Color skillReadyTint = new(0.24f, 0.86f, 0.54f, 0.42f);
        [SerializeField] private Color skillBlockedTint = new(0.86f, 0.4f, 0.4f, 0.68f);
        [SerializeField] private Color skillCooldownTint = new(0.95f, 0.72f, 0.34f, 0.56f);
        [SerializeField] private Color utilityButtonTint = new(0.28f, 0.42f, 0.94f, 0.26f);
        [SerializeField] private Color tapAssistButtonTint = new(0.14f, 0.7f, 0.96f, 0.96f);
        [SerializeField] private Color directDodgeLockTint = new(1f, 0.62f, 0.3f, 0.1f);
        [SerializeField] private Color directDodgeActiveTint = new(1f, 0.42f, 0.28f, 0.14f);
        [SerializeField] private MobileMoveInputMode defaultInputMode = MobileMoveInputMode.Joystick;
        [SerializeField] private float tapArrivalDistance = 0.32f;
        [SerializeField] private float tapCancelDistance = 0.42f;

        private RectTransform safeAreaRoot;
        private RectTransform moveTrayRoot;
        private RectTransform actionTrayRoot = null;
        private RectTransform stickRoot;
        private RectTransform stickVisual;
        private RectTransform stickKnob;
        private RectTransform posturePushButtonRoot = null;
        private RectTransform postureHoldButtonRoot = null;
        private RectTransform postureFallbackButtonRoot = null;
        private RectTransform skillButtonRoot;
        private RectTransform mapButtonRoot;
        private RectTransform moveModeButtonRoot = null;
        private RectTransform laneRailRoot;
        private RectTransform laneMarkerContainer;
        private RectTransform summonPlacementPreviewRoot;
        private RectTransform summonPlacementLaneRibbon;
        private RectTransform summonPlacementGuideLine;
        private RectTransform summonPlacementGuideMarker;
        private RectTransform summonPlacementLandingMarker;
        private RectTransform summonPlacementPocketMarker;
        private RectTransform summonPlacementBlockerMarker;
        private RectTransform summonPlacementRewardMarker;
        private Transform summonPlacementWorldPreviewRoot;
        private Transform summonPlacementWorldLandingMarker;
        private Transform summonPlacementWorldPocketMarker;
        private Transform summonPlacementWorldBlockerMarker;
        private Transform summonPlacementWorldRewardMarker;
        private LineRenderer summonPlacementWorldGuideLine;
        private RectTransform tapMovePadRoot;
        private RectTransform targetLockMarkerRoot;
        private RectTransform directDodgePadRoot;
        private RectTransform overviewPadRoot;
        private RectTransform zoomInButtonRoot;
        private RectTransform zoomOutButtonRoot;
        private RectTransform centerButtonRoot;
        private Image stickVisualImage;
        private Image stickKnobImage;
        private Image skillButtonImage;
        private Image mapButtonImage;
        private Image moveModeButtonImage = null;
        private Image laneRailImage;
        private Image laneLeftButtonImage;
        private Image laneRightButtonImage;
        private Image summonPlacementLaneRibbonImage;
        private Image summonPlacementGuideLineImage;
        private Image summonPlacementGuideMarkerImage;
        private Image summonPlacementLandingMarkerImage;
        private Image summonPlacementPocketMarkerImage;
        private Image summonPlacementBlockerMarkerImage;
        private Image summonPlacementRewardMarkerImage;
        private Image targetLockMarkerImage;
        private Image zoomInButtonImage;
        private Image zoomOutButtonImage;
        private Image centerButtonImage;
        private TMP_Text moveHintText;
        private TMP_Text posturePushButtonText = null;
        private TMP_Text postureHoldButtonText = null;
        private TMP_Text postureFallbackButtonText = null;
        private TMP_Text skillButtonText;
        private TMP_Text mapButtonText;
        private TMP_Text moveModeButtonText = null;
        private TMP_Text laneLeftButtonText;
        private TMP_Text laneRightButtonText;
        private TMP_Text laneStatusText;
        private TMP_Text summonPlacementPreviewText;
        private TMP_Text summonPlacementLandingText;
        private TMP_Text summonPlacementPocketText;
        private TMP_Text summonPlacementBlockerText;
        private TMP_Text summonPlacementRewardText;
        private TMP_Text tapMovePadHintText;
        private TMP_Text targetLockMarkerText;
        private TMP_Text directDodgeTitleText;
        private TMP_Text directDodgeLeftText;
        private TMP_Text directDodgeRightText;
        private TMP_Text overviewPadHintText;
        private TMP_Text zoomInButtonText;
        private TMP_Text zoomOutButtonText;
        private TMP_Text centerButtonText;
        private Button skillButton;
        private Button mapButton;
        private Button laneLeftButton = null;
        private Button laneRightButton = null;
        private Button zoomInButton;
        private Button zoomOutButton;
        private Button centerButton;
        private TMP_FontAsset runtimeFont;
        private PlayerController playerController;
        private PlayerSkillController playerSkillController;
        private BattleEnergySystem battleEnergySystem;
        private BattleCamera battleCamera;
        private Camera battleViewCamera;
        private EnemyAI enemyAI;
        private SummonSpawner summonSpawner;
        private bool pendingSkillPress;
        private bool pendingMapTogglePress;
        private bool pendingOverviewCenterPress;
        private float pendingOverviewZoomStep;
        private Vector2 pendingOverviewDragDelta;
        private bool isTouchLayoutActive;
        private Vector2 moveVector;
        private float pendingDirectDodgeDirection;
        private int pendingFocusLaneIndex = -1;
        private MobileMoveInputMode currentInputMode;
        private Vector3 tapMoveDestination;
        private bool hasTapMoveDestination;
        private bool isDirectDodgeModeActive;
        private bool isSummonPlacementActive;
        private int previewLaneIndex = BattleLaneUtility.DefaultLaneCount / 2;
        private bool hasPreviewDropWorldPosition;
        private Vector3 previewDropWorldPosition;
        private SummonPlacementFailureReason lastPlacementFailureReason = SummonPlacementFailureReason.None;
        private Rect lastSafeArea;
        private Vector2 lastRootSize;
        private int defaultDragThreshold = -1;
        private readonly List<Image> laneMarkerImages = new();
        private readonly List<Button> laneMarkerButtons = new();
        private readonly List<TMP_Text> laneMarkerTexts = new();

        public static MobileBattleControls Instance { get; private set; }

        public bool IsTouchLayoutActive => isTouchLayoutActive;
        public MobileMoveInputMode CurrentInputMode => currentInputMode;
        public static bool IsAutoCombatMovementEnabled => Instance != null && Instance.isTouchLayoutActive;
        public static bool IsDirectDodgeModeActive => Instance != null && Instance.isTouchLayoutActive && Instance.isDirectDodgeModeActive;
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            currentInputMode = defaultInputMode;
            EnsureUi();
            ApplySafeAreaAndLayout(forceRefresh: true);
        }

        private void OnEnable()
        {
            EnsureUi();
            ApplySafeAreaAndLayout(forceRefresh: true);
        }

        private void OnDisable()
        {
            moveVector = Vector2.zero;
            ClearTapMoveDestination();
            pendingSkillPress = false;
            pendingMapTogglePress = false;
            pendingOverviewCenterPress = false;
            pendingOverviewZoomStep = 0f;
            pendingOverviewDragDelta = Vector2.zero;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Update()
        {
            EnsureUi();
            ResolveReferences();
            bool shouldUseTouchLayout = ShouldUseMobileLayout(transform as RectTransform);
            if (shouldUseTouchLayout != isTouchLayoutActive)
            {
                isTouchLayoutActive = shouldUseTouchLayout;
                if (!isTouchLayoutActive)
                {
                    ClearTapMoveDestination();
                    pendingSkillPress = false;
                    pendingMapTogglePress = false;
                    pendingOverviewCenterPress = false;
                    pendingOverviewZoomStep = 0f;
                }

                SetRootActiveState(isTouchLayoutActive);
                ApplySafeAreaAndLayout(forceRefresh: true);
            }
            else
            {
                ApplySafeAreaAndLayout(forceRefresh: false);
            }

            UpdateDirectDodgeModeState();
            UpdateButtonVisualState();
            UpdateDragThreshold();
        }

        public static bool ShouldUseMobileLayout(RectTransform root)
        {
            float screenWidth = Screen.width > 0 ? Screen.width : (root != null && root.rect.width > 1f ? root.rect.width : 1080f);
            float screenHeight = Screen.height > 0 ? Screen.height : (root != null && root.rect.height > 1f ? root.rect.height : 1920f);
            float aspect = screenWidth <= 0.001f ? 1f : screenHeight / screenWidth;
            bool hasTouchDevice =
#if ENABLE_INPUT_SYSTEM
                Touchscreen.current != null;
#else
                Input.touchSupported;
#endif

            return Application.isMobilePlatform || hasTouchDevice || screenWidth <= 900f || aspect >= 1.35f;
        }

        public static bool TryGetMoveInput(out Vector2 moveInput)
        {
            moveInput = Vector2.zero;
            return false;
        }

        public static bool TryConsumeDirectDodge(out float directionSign)
        {
            if (Instance != null && Instance.isTouchLayoutActive && Mathf.Abs(Instance.pendingDirectDodgeDirection) > 0.001f)
            {
                directionSign = Instance.pendingDirectDodgeDirection;
                Instance.pendingDirectDodgeDirection = 0f;
                return true;
            }

            directionSign = 0f;
            return false;
        }

        public static bool TryConsumeFocusLaneSelection(out int focusLaneIndex)
        {
            if (Instance != null && Instance.isTouchLayoutActive && Instance.pendingFocusLaneIndex >= 0)
            {
                focusLaneIndex = Instance.pendingFocusLaneIndex;
                Instance.pendingFocusLaneIndex = -1;
                return true;
            }

            focusLaneIndex = BattleLaneUtility.DefaultLaneCount / 2;
            return false;
        }

        public static bool BeginSummonPlacement(Vector2 screenPosition)
        {
            if (Instance == null)
            {
                return false;
            }

            return Instance.BeginSummonPlacementInternal(screenPosition);
        }

        public static void UpdateSummonPlacement(Vector2 screenPosition)
        {
            Instance?.UpdateSummonPlacementInternal(screenPosition);
        }

        public static bool TryCompleteSummonPlacement(Vector2 screenPosition, out int laneIndex)
        {
            return TryCompleteSummonPlacement(screenPosition, out laneIndex, out _);
        }

        public static bool TryCompleteSummonPlacement(
            Vector2 screenPosition,
            out int laneIndex,
            out SummonPlacementFailureReason failureReason)
        {
            if (Instance != null)
            {
                return Instance.TryCompleteSummonPlacementInternal(screenPosition, out laneIndex, out failureReason);
            }

            laneIndex = BattleLaneUtility.DefaultLaneCount / 2;
            failureReason = SummonPlacementFailureReason.LaneNotSelected;
            return false;
        }

        public static void CancelSummonPlacement()
        {
            Instance?.CancelSummonPlacementInternal();
        }

        public static bool ConsumeSkillPressed()
        {
            if (Instance == null || !Instance.isTouchLayoutActive || !Instance.pendingSkillPress)
            {
                return false;
            }

            Instance.pendingSkillPress = false;
            return true;
        }

        public static bool ConsumeMapTogglePressed()
        {
            if (Instance == null || !Instance.isTouchLayoutActive || !Instance.pendingMapTogglePress)
            {
                return false;
            }

            Instance.pendingMapTogglePress = false;
            return true;
        }

        public static bool TryConsumeOverviewDrag(out Vector2 dragDelta)
        {
            if (Instance != null && Instance.isTouchLayoutActive && Instance.pendingOverviewDragDelta.sqrMagnitude > 0.001f)
            {
                dragDelta = Instance.pendingOverviewDragDelta;
                Instance.pendingOverviewDragDelta = Vector2.zero;
                return true;
            }

            dragDelta = Vector2.zero;
            return false;
        }

        public static bool TryConsumeOverviewZoomStep(out float zoomStep)
        {
            if (Instance != null && Instance.isTouchLayoutActive && Mathf.Abs(Instance.pendingOverviewZoomStep) > 0.001f)
            {
                zoomStep = Instance.pendingOverviewZoomStep;
                Instance.pendingOverviewZoomStep = 0f;
                return true;
            }

            zoomStep = 0f;
            return false;
        }

        public static bool ConsumeOverviewCenterPressed()
        {
            if (Instance == null || !Instance.isTouchLayoutActive || !Instance.pendingOverviewCenterPress)
            {
                return false;
            }

            Instance.pendingOverviewCenterPress = false;
            return true;
        }

        internal void SetMoveVector(Vector2 normalizedMove)
        {
            moveVector = Vector2.ClampMagnitude(normalizedMove, 1f);
            RefreshStickVisual();
        }

        internal void ClearMoveVector()
        {
            moveVector = Vector2.zero;
            RefreshStickVisual();
        }

        internal void HandleTapMovePointer(PointerEventData eventData)
        {
            if (!isTouchLayoutActive || currentInputMode != MobileMoveInputMode.TapAssist)
            {
                return;
            }

            if (battleCamera != null && battleCamera.IsOverviewMode)
            {
                return;
            }

            if (!TryResolveTapMoveDestination(eventData.position, eventData.pressEventCamera, out Vector3 targetPosition))
            {
                return;
            }

            if (playerController == null)
            {
                return;
            }

            Vector3 currentPosition = playerController.transform.position;
            Vector3 planarDelta = targetPosition - currentPosition;
            planarDelta.y = 0f;
            if (planarDelta.sqrMagnitude <= tapCancelDistance * tapCancelDistance)
            {
                ClearTapMoveDestination();
                return;
            }

            tapMoveDestination = targetPosition;
            hasTapMoveDestination = true;
        }

        internal void HandleBattlefieldTapRelease(PointerEventData eventData, Vector2 pointerDownScreenPosition)
        {
            if (!isTouchLayoutActive || isSummonPlacementActive || isDirectDodgeModeActive)
            {
                return;
            }

            if (battleCamera != null && battleCamera.IsOverviewMode)
            {
                return;
            }

            if ((eventData.position - pointerDownScreenPosition).sqrMagnitude > 28f * 28f)
            {
                return;
            }

            ResolveReferences();
            if (playerController == null)
            {
                return;
            }

            if (!TryResolveLockableTargetAtScreenPosition(eventData.position, out SummonUnit summonTarget, out EnemyAI bossTarget, out BattleStructure structureTarget))
            {
                return;
            }

            bool changed = summonTarget != null
                ? playerController.ToggleManualTarget(summonTarget)
                : structureTarget != null
                    ? playerController.ToggleManualTarget(structureTarget)
                    : bossTarget != null && playerController.ToggleManualTarget(bossTarget);
            if (changed)
            {
                CardHandTouchCoordinator.SuppressClicks(0.08f);
            }
        }

        private Vector2 ResolveMoveInput()
        {
            if (isDirectDodgeModeActive)
            {
                return Vector2.zero;
            }

            return currentInputMode == MobileMoveInputMode.TapAssist
                ? ResolveTapAssistMoveInput()
                : moveVector;
        }

        private Vector2 ResolveTapAssistMoveInput()
        {
            if (!hasTapMoveDestination || playerController == null)
            {
                return Vector2.zero;
            }

            Vector3 planarDelta = tapMoveDestination - playerController.transform.position;
            planarDelta.y = 0f;
            if (planarDelta.sqrMagnitude <= tapArrivalDistance * tapArrivalDistance)
            {
                ClearTapMoveDestination();
                return Vector2.zero;
            }

            return Vector2.ClampMagnitude(new Vector2(planarDelta.x, planarDelta.z), 1f);
        }

        private void UpdateTapAssistMovement()
        {
            if (currentInputMode != MobileMoveInputMode.TapAssist)
            {
                return;
            }

            if (battleCamera != null && battleCamera.IsOverviewMode)
            {
                ClearTapMoveDestination();
            }
        }

        private void UpdateDirectDodgeModeState()
        {
            bool shouldEnable =
                isTouchLayoutActive &&
                HasAnyDirectProjectileWarning() &&
                (battleCamera == null || !battleCamera.IsOverviewMode);

            if (shouldEnable == isDirectDodgeModeActive)
            {
                return;
            }

            isDirectDodgeModeActive = shouldEnable;
            pendingDirectDodgeDirection = 0f;
            isSummonPlacementActive = false;
            moveVector = Vector2.zero;
            ClearTapMoveDestination();
            RefreshStickVisual();
        }

        private bool HasAnyDirectProjectileWarning()
        {
            return HasAnyDirectProjectileDanger(out _, out _);
        }

        private bool HasAnyDirectProjectileDanger(out bool hasActiveProjectileDanger, out bool hasLockingDanger)
        {
            hasActiveProjectileDanger = enemyAI != null &&
                enemyAI.IsDirectProjectileDangerActive &&
                enemyAI.ActiveDirectProjectileCount > 0;
            hasLockingDanger = enemyAI != null && enemyAI.IsDirectProjectileLocking;

            if (PveProjectileEmitter.HasAnyDirectProjectileDanger)
            {
                hasActiveProjectileDanger = true;
            }

            if (PveProjectileEmitter.HasAnyDirectProjectileLocking)
            {
                hasLockingDanger = true;
            }

            return hasActiveProjectileDanger || hasLockingDanger;
        }

        internal void HandleDirectDodgeRelease(PointerEventData eventData, Vector2 pointerDownScreenPosition)
        {
            if (!isDirectDodgeModeActive || directDodgePadRoot == null)
            {
                return;
            }

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    directDodgePadRoot,
                    pointerDownScreenPosition,
                    eventData.pressEventCamera,
                    out Vector2 startLocalPoint))
            {
                return;
            }

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    directDodgePadRoot,
                    eventData.position,
                    eventData.pressEventCamera,
                    out Vector2 endLocalPoint))
            {
                return;
            }

            float minimumFlickDistance = Mathf.Max(36f, directDodgePadRoot.rect.width * 0.08f);
            float deltaX = endLocalPoint.x - startLocalPoint.x;
            if (Mathf.Abs(deltaX) < minimumFlickDistance)
            {
                return;
            }

            pendingDirectDodgeDirection = Mathf.Sign(deltaX);
            CardHandTouchCoordinator.SuppressClicks(0.08f);
        }

        internal void HandleLaneSwipeRelease(PointerEventData eventData, Vector2 pointerDownScreenPosition)
        {
            // Manual player lane selection is retired in the escort-lock prototype.
        }

        private void SetInputMode(MobileMoveInputMode nextMode)
        {
            if (currentInputMode == nextMode)
            {
                return;
            }

            currentInputMode = nextMode;
            moveVector = Vector2.zero;
            ClearTapMoveDestination();
            RefreshStickVisual();
        }

        private void ToggleInputMode()
        {
            SetInputMode(currentInputMode == MobileMoveInputMode.Joystick
                ? MobileMoveInputMode.TapAssist
                : MobileMoveInputMode.Joystick);
        }

        private bool TryResolveTapMoveDestination(Vector2 screenPosition, Camera eventCamera, out Vector3 targetPosition)
        {
            ResolveReferences();
            if (playerController == null)
            {
                targetPosition = Vector3.zero;
                return false;
            }

            Camera worldCamera = battleViewCamera != null
                ? battleViewCamera
                : battleCamera != null
                    ? battleCamera.GetComponent<Camera>()
                    : Camera.main;
            if (worldCamera == null)
            {
                targetPosition = Vector3.zero;
                return false;
            }

            float groundY = playerController.transform.position.y;
            Plane groundPlane = new(Vector3.up, new Vector3(0f, groundY, 0f));
            Ray inputRay = worldCamera.ScreenPointToRay(screenPosition);
            if (!groundPlane.Raycast(inputRay, out float hitDistance))
            {
                targetPosition = Vector3.zero;
                return false;
            }

            targetPosition = inputRay.GetPoint(hitDistance);
            targetPosition = playerController.ClampToMovementBounds(targetPosition);
            targetPosition.y = groundY;
            return true;
        }

        private bool TryResolveLockableTargetAtScreenPosition(
            Vector2 screenPosition,
            out SummonUnit summonTarget,
            out EnemyAI bossTarget,
            out BattleStructure structureTarget)
        {
            summonTarget = null;
            bossTarget = null;
            structureTarget = null;

            Camera worldCamera = battleViewCamera != null
                ? battleViewCamera
                : battleCamera != null
                    ? battleCamera.GetComponent<Camera>()
                    : Camera.main;
            if (worldCamera == null)
            {
                return false;
            }

            Ray ray = worldCamera.ScreenPointToRay(screenPosition);
            RaycastHit[] hits = Physics.RaycastAll(ray, 256f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide);
            if (hits == null || hits.Length == 0)
            {
                return false;
            }

            float bestDistance = float.MaxValue;
            for (int index = 0; index < hits.Length; index++)
            {
                Collider hitCollider = hits[index].collider;
                if (hitCollider == null)
                {
                    continue;
                }

                SummonUnit hitSummon = hitCollider.GetComponent<SummonUnit>();
                if (hitSummon == null)
                {
                    hitSummon = hitCollider.GetComponentInParent<SummonUnit>();
                }

                if (hitSummon != null && hitSummon.IsAlive && !hitSummon.IsPlayerTeam && hits[index].distance < bestDistance)
                {
                    summonTarget = hitSummon;
                    bossTarget = null;
                    structureTarget = null;
                    bestDistance = hits[index].distance;
                    continue;
                }

                BattleStructure hitStructure = hitCollider.GetComponent<BattleStructure>();
                if (hitStructure == null)
                {
                    hitStructure = hitCollider.GetComponentInParent<BattleStructure>();
                }

                if (hitStructure != null && !hitStructure.IsDestroyed && hits[index].distance < bestDistance)
                {
                    summonTarget = null;
                    bossTarget = null;
                    structureTarget = hitStructure;
                    bestDistance = hits[index].distance;
                    continue;
                }

                EnemyAI hitBoss = hitCollider.GetComponent<EnemyAI>();
                if (hitBoss == null)
                {
                    hitBoss = hitCollider.GetComponentInParent<EnemyAI>();
                }

                if (hitBoss != null && hitBoss.isActiveAndEnabled && hitBoss.gameObject.activeInHierarchy && hits[index].distance < bestDistance)
                {
                    summonTarget = null;
                    bossTarget = hitBoss;
                    structureTarget = null;
                    bestDistance = hits[index].distance;
                }
            }

            return summonTarget != null || bossTarget != null || structureTarget != null;
        }

        private void ClearTapMoveDestination()
        {
            hasTapMoveDestination = false;
            tapMoveDestination = Vector3.zero;
        }

        internal void AddOverviewDrag(Vector2 dragDelta)
        {
            pendingOverviewDragDelta += dragDelta;
        }

        internal void QueueOverviewZoomStep(float zoomStep)
        {
            pendingOverviewZoomStep += zoomStep;
        }

        private void HandleSkillButtonPressed()
        {
            if (!isTouchLayoutActive)
            {
                return;
            }

            pendingSkillPress = true;
        }

        private void HandleMapButtonPressed()
        {
            if (!isTouchLayoutActive)
            {
                return;
            }

            pendingMapTogglePress = true;
        }

        private void HandleMoveModeButtonPressed()
        {
            if (!isTouchLayoutActive)
            {
                return;
            }

            // Legacy move mode toggle intentionally disabled in the focus-lane prototype.
        }

        private void HandleFocusLanePressed(int laneIndex)
        {
            // Manual player lane selection is retired in the escort-lock prototype.
        }

        private int ResolveCurrentFocusLaneForSwipe()
        {
            if (playerController != null)
            {
                return BattleLaneUtility.ClampLaneIndex(playerController.PreferredLaneIndex);
            }

            if (summonSpawner != null)
            {
                return BattleLaneUtility.ClampLaneIndex(summonSpawner.SelectedLaneIndex);
            }

            return BattleLaneUtility.DefaultLaneCount / 2;
        }

        private bool BeginSummonPlacementInternal(Vector2 screenPosition)
        {
            if (!isTouchLayoutActive || isDirectDodgeModeActive || laneRailRoot == null)
            {
                return false;
            }

            isSummonPlacementActive = true;
            hasPreviewDropWorldPosition = false;
            previewDropWorldPosition = Vector3.zero;
            lastPlacementFailureReason = SummonPlacementFailureReason.None;
            int initialLane = summonSpawner != null
                ? summonSpawner.SelectedLaneIndex
                : BattleLaneUtility.DefaultLaneCount / 2;
            previewLaneIndex = BattleLaneUtility.ClampLaneIndex(initialLane);
            if (TryResolveDefaultSummonDropWorldPosition(previewLaneIndex, out Vector3 defaultDropWorldPosition))
            {
                previewDropWorldPosition = defaultDropWorldPosition;
                hasPreviewDropWorldPosition = true;
            }
            UpdateSummonPlacementInternal(screenPosition);
            return true;
        }

        private void UpdateSummonPlacementInternal(Vector2 screenPosition)
        {
            if (!isSummonPlacementActive)
            {
                return;
            }

            if (TryResolveSummonDropFromScreenPosition(screenPosition, out int laneIndex, out Vector3 dropWorldPosition))
            {
                previewLaneIndex = laneIndex;
                previewDropWorldPosition = dropWorldPosition;
                hasPreviewDropWorldPosition = true;
                summonSpawner?.SetSelectedLane(previewLaneIndex);
            }
        }

        private bool TryCompleteSummonPlacementInternal(
            Vector2 screenPosition,
            out int laneIndex,
            out SummonPlacementFailureReason failureReason)
        {
            laneIndex = previewLaneIndex;
            failureReason = SummonPlacementFailureReason.None;
            if (!isSummonPlacementActive)
            {
                failureReason = SummonPlacementFailureReason.LaneNotSelected;
                return false;
            }

            bool resolved = TryResolveSummonDropFromScreenPosition(screenPosition, out laneIndex, out Vector3 dropWorldPosition);
            if (resolved)
            {
                previewLaneIndex = laneIndex;
                previewDropWorldPosition = dropWorldPosition;
                hasPreviewDropWorldPosition = true;
                summonSpawner?.SetSelectedLane(previewLaneIndex);
                if (hasPreviewDropWorldPosition)
                {
                    summonSpawner?.SetPendingPlacementWorldPosition(previewDropWorldPosition);
                }
                lastPlacementFailureReason = SummonPlacementFailureReason.None;
            }
            else
            {
                failureReason = SummonPlacementFailureReason.LaneNotSelected;
                lastPlacementFailureReason = failureReason;
                summonSpawner?.ClearPendingPlacementWorldPosition();
                ShowPlacementFailureFeedback(failureReason);
            }

            isSummonPlacementActive = false;
            return resolved;
        }

        private void CancelSummonPlacementInternal()
        {
            isSummonPlacementActive = false;
            hasPreviewDropWorldPosition = false;
            previewDropWorldPosition = Vector3.zero;
            summonSpawner?.ClearPendingPlacementWorldPosition();
            lastPlacementFailureReason = SummonPlacementFailureReason.None;
        }

        private bool TryResolveSummonDropFromScreenPosition(Vector2 screenPosition, out int laneIndex, out Vector3 dropWorldPosition)
        {
            laneIndex = BattleLaneUtility.DefaultLaneCount / 2;
            dropWorldPosition = Vector3.zero;
            ResolveReferences();
            if (safeAreaRoot == null || playerController == null)
            {
                return false;
            }

            BattleManager battleManager = BattleManager.Instance;
            Camera worldCamera = battleViewCamera != null
                ? battleViewCamera
                : battleCamera != null
                    ? battleCamera.GetComponent<Camera>()
                    : Camera.main;
            if (battleManager == null || worldCamera == null)
            {
                return false;
            }

            float groundY = playerController.transform.position.y;
            Plane groundPlane = new(Vector3.up, new Vector3(0f, groundY, 0f));
            Ray inputRay = worldCamera.ScreenPointToRay(screenPosition);
            if (!groundPlane.Raycast(inputRay, out float hitDistance))
            {
                return false;
            }

            Vector3 rawWorldPosition = inputRay.GetPoint(hitDistance);
            dropWorldPosition = ClampSummonDropWorldPosition(rawWorldPosition, battleManager, out laneIndex);
            return true;
        }

        private bool TryResolveDefaultSummonDropWorldPosition(int laneIndex, out Vector3 dropWorldPosition)
        {
            ResolveReferences();
            if (playerController == null)
            {
                dropWorldPosition = Vector3.zero;
                return false;
            }

            BattleManager battleManager = BattleManager.Instance;
            Vector3 basePosition = playerController.transform.position + new Vector3(0f, 0f, 1.25f);
            dropWorldPosition = ClampSummonDropWorldPosition(basePosition, battleManager, out int resolvedLaneIndex);
            previewLaneIndex = resolvedLaneIndex;
            return true;
        }

        private Vector3 ClampSummonDropWorldPosition(Vector3 worldPosition, BattleManager battleManager, out int laneIndex)
        {
            Vector3 playerPosition = playerController.transform.position;
            float halfWidth = battleManager != null
                ? Mathf.Max(4.8f, battleManager.LaneHalfWidth * 0.86f)
                : 5.4f;
            float minZ = playerPosition.z - 0.7f;
            float maxZ = playerPosition.z + 2.8f;

            Vector3 clamped = worldPosition;
            clamped.x = Mathf.Clamp(clamped.x, playerPosition.x - halfWidth, playerPosition.x + halfWidth);
            clamped.z = Mathf.Clamp(clamped.z, minZ, maxZ);
            clamped.y = playerPosition.y;
            clamped = playerController.ClampToMovementBounds(clamped);

            laneIndex = battleManager != null
                ? battleManager.GetNearestLaneIndex(clamped.x)
                : BattleLaneUtility.GetNearestLaneIndex(clamped.x, BattleLaneUtility.BuildLaneAnchors(5.75f));
            float laneCenterX = battleManager != null
                ? battleManager.GetLaneCenterX(laneIndex)
                : BattleLaneUtility.GetLaneCenterX(laneIndex, 5.75f);
            clamped.x = Mathf.Lerp(clamped.x, laneCenterX, 0.12f);
            return clamped;
        }

        private bool TryGetLaneScreenSamplePoint(int laneIndex, out Vector3 laneWorld)
        {
            laneWorld = Vector3.zero;
            BattleManager battleManager = BattleManager.Instance;
            if (battleManager == null)
            {
                return false;
            }

            int clampedLane = BattleLaneUtility.ClampLaneIndex(laneIndex, battleManager.LaneCount);
            if (battleManager.TryGetSummonLanePreview(clampedLane, out BattleManager.SummonLanePreview preview))
            {
                laneWorld = preview.LandingPosition + new Vector3(0f, 0.1f, 0f);
                return true;
            }

            float sampleY = playerController != null ? playerController.transform.position.y : 0f;
            float sampleZ = battleManager.SummonSpawnPoint != null
                ? battleManager.SummonSpawnPoint.position.z + 0.8f
                : Mathf.Clamp(battleManager.LaneLength * 0.14f, 4.5f, battleManager.LaneLength - 10f);
            laneWorld = new Vector3(battleManager.GetLaneCenterX(clampedLane), sampleY, sampleZ);
            return true;
        }

        private void ShowPlacementFailureFeedback(SummonPlacementFailureReason failureReason)
        {
            if (failureReason == SummonPlacementFailureReason.None)
            {
                return;
            }

            Vector3 feedbackPosition = playerController != null
                ? playerController.transform.position + new Vector3(0f, 2.2f, 0f)
                : Vector3.zero;
            BattlePresentationController.Instance?.ShowWorldText(
                feedbackPosition,
                "레인 선택 안 됨",
                new Color(1f, 0.56f, 0.44f, 1f),
                3.8f,
                0.7f);
        }

        private void HandlePosturePushPressed()
        { }

        private void HandlePostureHoldPressed()
        { }

        private void HandlePostureFallbackPressed()
        { }

        private void HandleZoomInButtonPressed()
        {
            if (!isTouchLayoutActive)
            {
                return;
            }

            pendingOverviewZoomStep += 1f;
        }

        private void HandleZoomOutButtonPressed()
        {
            if (!isTouchLayoutActive)
            {
                return;
            }

            pendingOverviewZoomStep -= 1f;
        }

        private void HandleCenterButtonPressed()
        {
            if (!isTouchLayoutActive)
            {
                return;
            }

            pendingOverviewCenterPress = true;
        }

        private void ResolveReferences()
        {
            if (battleEnergySystem == null)
            {
                battleEnergySystem = BattleEnergySystem.Instance;
            }

            if (battleCamera == null)
            {
                battleCamera = FindFirstObjectByType<BattleCamera>();
            }

            if (battleViewCamera == null && battleCamera != null)
            {
                battleViewCamera = battleCamera.GetComponent<Camera>();
            }

            if (enemyAI == null)
            {
                enemyAI = FindFirstObjectByType<EnemyAI>();
            }

            if (summonSpawner == null)
            {
                summonSpawner = FindFirstObjectByType<SummonSpawner>();
            }

            if (playerController == null)
            {
                playerController = BattleManager.Instance != null ? BattleManager.Instance.PlayerController : null;
                if (playerController == null)
                {
                    playerController = FindFirstObjectByType<PlayerController>();
                }
            }

            if (playerSkillController == null)
            {
                PlayerController resolvedPlayer = BattleManager.Instance != null ? BattleManager.Instance.PlayerController : null;
                if (resolvedPlayer == null)
                {
                    resolvedPlayer = FindFirstObjectByType<PlayerController>();
                }

                if (resolvedPlayer != null)
                {
                    playerController = resolvedPlayer;
                    playerSkillController = resolvedPlayer.GetComponent<PlayerSkillController>();
                }
            }
        }

        private void EnsureUi()
        {
            RectTransform root = transform as RectTransform;
            if (root == null)
            {
                return;
            }

            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero;
            root.offsetMax = Vector2.zero;

            runtimeFont ??= RuntimeUIFontUtility.EnsureKoreanFallback();
            if (runtimeFont == null)
            {
                return;
            }

            if (safeAreaRoot == null)
            {
                safeAreaRoot = EnsureRect("SafeAreaRoot", root);
                safeAreaRoot.SetAsLastSibling();
            }

            if (stickRoot == null)
            {
                stickRoot = EnsurePanel("MoveStickRoot", safeAreaRoot, panelTint);
                stickVisual = EnsurePanel("StickVisual", stickRoot, stickReadyTint);
                stickKnob = EnsurePanel("StickKnob", stickRoot, new Color(1f, 1f, 1f, 0.92f));
                stickVisualImage = stickVisual.GetComponent<Image>();
                stickKnobImage = stickKnob.GetComponent<Image>();
                moveHintText = EnsureText("MoveHintText", stickRoot, runtimeFont, 14f, TextAlignmentOptions.Center);
                moveHintText.text = "\uC774\uB3D9";

                MobileStickPad stickPad = stickRoot.gameObject.GetComponent<MobileStickPad>();
                if (stickPad == null)
                {
                    stickPad = stickRoot.gameObject.AddComponent<MobileStickPad>();
                }

                stickPad.Initialize(this, stickRoot, stickKnob);
            }

            if (moveTrayRoot == null)
            {
                moveTrayRoot = EnsurePanel("MoveTray", safeAreaRoot, new Color(0.03f, 0.06f, 0.11f, 0.3f));
                Image moveTrayImage = moveTrayRoot.GetComponent<Image>();
                if (moveTrayImage != null)
                {
                    moveTrayImage.raycastTarget = false;
                }
                moveTrayRoot.SetAsFirstSibling();
            }

            if (tapMovePadRoot == null)
            {
                tapMovePadRoot = EnsurePanel("TapMovePad", safeAreaRoot, new Color(1f, 1f, 1f, 0.01f));
                tapMovePadHintText = EnsureText("TapMoveHintText", tapMovePadRoot, runtimeFont, 15f, TextAlignmentOptions.Center);
                tapMovePadHintText.text = "적 또는 목표를 눌러 고정";
                tapMovePadHintText.color = new Color(0.82f, 0.92f, 1f, 0.84f);
                tapMovePadHintText.text = "\uC801 \uB610\uB294 \uBAA9\uD45C\uB97C \uB20C\uB7EC \uACE0\uC815";

                MobileTapMovePad tapMovePad = tapMovePadRoot.gameObject.GetComponent<MobileTapMovePad>();
                if (tapMovePad == null)
                {
                    tapMovePad = tapMovePadRoot.gameObject.AddComponent<MobileTapMovePad>();
                }

                tapMovePad.Initialize(this);
                tapMovePadRoot.SetAsFirstSibling();
            }

            if (targetLockMarkerRoot == null)
            {
                targetLockMarkerRoot = EnsurePanel("TargetLockMarker", safeAreaRoot, new Color(0.08f, 0.2f, 0.34f, 0.9f));
                targetLockMarkerImage = targetLockMarkerRoot.GetComponent<Image>();
                if (targetLockMarkerImage != null)
                {
                    targetLockMarkerImage.raycastTarget = false;
                }

                targetLockMarkerText = EnsureText("Label", targetLockMarkerRoot, runtimeFont, 12f, TextAlignmentOptions.Center);
                targetLockMarkerText.text = "LOCK";
                targetLockMarkerText.color = new Color(0.94f, 0.98f, 1f, 1f);
                targetLockMarkerRoot.gameObject.SetActive(false);
                targetLockMarkerRoot.SetAsLastSibling();
            }

            if (directDodgePadRoot == null)
            {
                directDodgePadRoot = EnsurePanel("DirectDodgePad", safeAreaRoot, directDodgeLockTint);
                directDodgeTitleText = EnsureText("DirectDodgeTitle", directDodgePadRoot, runtimeFont, 17f, TextAlignmentOptions.Center);
                directDodgeTitleText.text = "직격 위험";
                directDodgeLeftText = EnsureText("DirectDodgeLeft", directDodgePadRoot, runtimeFont, 15f, TextAlignmentOptions.Center);
                directDodgeLeftText.text = "왼쪽 회피";
                directDodgeRightText = EnsureText("DirectDodgeRight", directDodgePadRoot, runtimeFont, 15f, TextAlignmentOptions.Center);
                directDodgeRightText.text = "오른쪽 회피";

                MobileDirectDodgePad directDodgePad = directDodgePadRoot.gameObject.GetComponent<MobileDirectDodgePad>();
                if (directDodgePad == null)
                {
                    directDodgePad = directDodgePadRoot.gameObject.AddComponent<MobileDirectDodgePad>();
                }

                directDodgeTitleText.text = "\uC9C1\uACA9 \uC704\uD5D8";
                directDodgeLeftText.text = "\uC67C\uCABD \uD68C\uD53C";
                directDodgeRightText.text = "\uC624\uB978\uCABD \uD68C\uD53C";
                directDodgePad.Initialize(this);
                directDodgePadRoot.SetAsLastSibling();
            }

            if (skillButtonRoot == null)
            {
                skillButtonRoot = EnsurePanel("SkillButton", safeAreaRoot, skillReadyTint);
                skillButtonImage = skillButtonRoot.GetComponent<Image>();
                skillButton = skillButtonRoot.GetComponent<Button>();
                if (skillButton == null)
                {
                    skillButton = skillButtonRoot.gameObject.AddComponent<Button>();
                }

                skillButton.onClick.RemoveListener(HandleSkillButtonPressed);
                skillButton.onClick.AddListener(HandleSkillButtonPressed);
                skillButtonText = EnsureText("Label", skillButtonRoot, runtimeFont, 18f, TextAlignmentOptions.Center);
            }

            if (mapButtonRoot == null)
            {
                mapButtonRoot = EnsurePanel("MapButton", safeAreaRoot, utilityButtonTint);
                mapButtonImage = mapButtonRoot.GetComponent<Image>();
                mapButton = mapButtonRoot.GetComponent<Button>();
                if (mapButton == null)
                {
                    mapButton = mapButtonRoot.gameObject.AddComponent<Button>();
                }

                mapButton.onClick.RemoveListener(HandleMapButtonPressed);
                mapButton.onClick.AddListener(HandleMapButtonPressed);
                mapButtonText = EnsureText("Label", mapButtonRoot, runtimeFont, 16f, TextAlignmentOptions.Center);
            }

            if (laneRailRoot == null)
            {
                laneRailRoot = EnsurePanel("LaneRail", safeAreaRoot, new Color(0.05f, 0.09f, 0.16f, 0.08f));
                laneRailImage = laneRailRoot.GetComponent<Image>();
                MobileLaneSwipePad laneSwipePad = laneRailRoot.gameObject.GetComponent<MobileLaneSwipePad>();
                if (laneSwipePad == null)
                {
                    laneSwipePad = laneRailRoot.gameObject.AddComponent<MobileLaneSwipePad>();
                }

                laneSwipePad.Initialize(this);

                laneStatusText = EnsureText("LaneStatus", laneRailRoot, runtimeFont, 12f, TextAlignmentOptions.Center);
                laneStatusText.text = "추천: 2번 레인 소환";
                laneStatusText.raycastTarget = false;
                laneStatusText.text = "\uCD94\uCC9C: 2\uBC88 \uB808\uC778 \uC18C\uD658";

                laneMarkerContainer = EnsureRect("LaneMarkers", laneRailRoot);
                laneMarkerImages.Clear();
                laneMarkerButtons.Clear();
                laneMarkerTexts.Clear();
                for (int index = 0; index < BattleLaneUtility.DefaultLaneCount; index++)
                {
                    RectTransform markerRect = EnsurePanel($"LaneMarker{index}", laneMarkerContainer, new Color(0.34f, 0.82f, 1f, 0.16f));
                    Image markerImage = markerRect.GetComponent<Image>();
                    if (markerImage != null)
                    {
                        markerImage.raycastTarget = false;
                    }

                    Button markerButton = markerRect.GetComponent<Button>() ?? markerRect.gameObject.AddComponent<Button>();
                    markerButton.onClick.RemoveAllListeners();
                    markerButton.interactable = false;
                    TMP_Text markerText = EnsureText("LaneLabel", markerRect, runtimeFont, 11f, TextAlignmentOptions.Center);
                    markerText.text = (index + 1).ToString();
                    markerText.raycastTarget = false;
                    laneMarkerImages.Add(markerImage);
                    laneMarkerButtons.Add(markerButton);
                    laneMarkerTexts.Add(markerText);
                }
            }

            if (summonPlacementPreviewRoot == null)
            {
                summonPlacementPreviewRoot = EnsureRect("SummonPlacementPreview", safeAreaRoot);
                summonPlacementPreviewRoot.SetAsLastSibling();

                summonPlacementLaneRibbon = EnsurePanel("LaneRibbon", summonPlacementPreviewRoot, new Color(0.18f, 0.78f, 1f, 0.12f));
                summonPlacementLaneRibbonImage = summonPlacementLaneRibbon.GetComponent<Image>();
                if (summonPlacementLaneRibbonImage != null)
                {
                    summonPlacementLaneRibbonImage.raycastTarget = false;
                }

                summonPlacementGuideLine = EnsurePanel("LaneGuideLine", summonPlacementPreviewRoot, new Color(0.24f, 0.88f, 1f, 0.24f));
                summonPlacementGuideLineImage = summonPlacementGuideLine.GetComponent<Image>();
                if (summonPlacementGuideLineImage != null)
                {
                    summonPlacementGuideLineImage.raycastTarget = false;
                }

                summonPlacementGuideMarker = EnsurePanel("LaneGuideMarker", summonPlacementPreviewRoot, new Color(0.09f, 0.2f, 0.32f, 0.94f));
                summonPlacementGuideMarkerImage = summonPlacementGuideMarker.GetComponent<Image>();
                if (summonPlacementGuideMarkerImage != null)
                {
                    summonPlacementGuideMarkerImage.raycastTarget = false;
                }

                summonPlacementPreviewText = EnsureText("LaneGuideText", summonPlacementGuideMarker, runtimeFont, 14f, TextAlignmentOptions.Center);
                summonPlacementPreviewText.text = "DROP LANE 3";
                summonPlacementPreviewText.color = new Color(0.88f, 0.97f, 1f, 1f);

                summonPlacementLandingMarker = EnsurePanel("LandingMarker", summonPlacementPreviewRoot, new Color(0.98f, 0.58f, 0.96f, 0.95f));
                summonPlacementLandingMarkerImage = summonPlacementLandingMarker.GetComponent<Image>();
                summonPlacementLandingText = EnsureText("Label", summonPlacementLandingMarker, runtimeFont, 11f, TextAlignmentOptions.Center);
                summonPlacementLandingText.text = "소환";
                summonPlacementLandingText.color = new Color(0.12f, 0.05f, 0.13f, 1f);

                summonPlacementPocketMarker = EnsurePanel("PocketMarker", summonPlacementPreviewRoot, new Color(0.45f, 0.95f, 1f, 0.95f));
                summonPlacementPocketMarkerImage = summonPlacementPocketMarker.GetComponent<Image>();
                summonPlacementPocketText = EnsureText("Label", summonPlacementPocketMarker, runtimeFont, 11f, TextAlignmentOptions.Center);
                summonPlacementPocketText.text = "POCKET";
                summonPlacementPocketText.color = new Color(0.05f, 0.12f, 0.16f, 1f);

                summonPlacementBlockerMarker = EnsurePanel("BlockerMarker", summonPlacementPreviewRoot, new Color(1f, 0.78f, 0.34f, 0.95f));
                summonPlacementBlockerMarkerImage = summonPlacementBlockerMarker.GetComponent<Image>();
                summonPlacementBlockerText = EnsureText("Label", summonPlacementBlockerMarker, runtimeFont, 11f, TextAlignmentOptions.Center);
                summonPlacementBlockerText.text = "BREAK";
                summonPlacementBlockerText.color = new Color(0.18f, 0.12f, 0.05f, 1f);

                summonPlacementRewardMarker = EnsurePanel("RewardMarker", summonPlacementPreviewRoot, new Color(0.42f, 1f, 0.68f, 0.95f));
                summonPlacementRewardMarkerImage = summonPlacementRewardMarker.GetComponent<Image>();
                summonPlacementRewardText = EnsureText("Label", summonPlacementRewardMarker, runtimeFont, 11f, TextAlignmentOptions.Center);
                summonPlacementRewardText.text = "REWARD";
                summonPlacementRewardText.color = new Color(0.06f, 0.14f, 0.08f, 1f);
            }

            if (overviewPadRoot == null)
            {
                overviewPadRoot = EnsurePanel("OverviewDragPad", safeAreaRoot, new Color(1f, 1f, 1f, 0.01f));
                overviewPadHintText = EnsureText("OverviewHintText", overviewPadRoot, runtimeFont, 15f, TextAlignmentOptions.Center);
                overviewPadHintText.text = "DRAG TO PAN";
                overviewPadHintText.color = new Color(0.82f, 0.92f, 1f, 0.84f);

                MobileOverviewPad overviewPad = overviewPadRoot.gameObject.GetComponent<MobileOverviewPad>();
                if (overviewPad == null)
                {
                    overviewPad = overviewPadRoot.gameObject.AddComponent<MobileOverviewPad>();
                }

                overviewPad.Initialize(this);
                overviewPadRoot.SetAsFirstSibling();
            }

            if (zoomInButtonRoot == null)
            {
                zoomInButtonRoot = EnsurePanel("ZoomInButton", safeAreaRoot, utilityButtonTint);
                zoomInButtonImage = zoomInButtonRoot.GetComponent<Image>();
                zoomInButton = zoomInButtonRoot.GetComponent<Button>() ?? zoomInButtonRoot.gameObject.AddComponent<Button>();
                zoomInButton.onClick.RemoveListener(HandleZoomInButtonPressed);
                zoomInButton.onClick.AddListener(HandleZoomInButtonPressed);
                zoomInButtonText = EnsureText("Label", zoomInButtonRoot, runtimeFont, 20f, TextAlignmentOptions.Center);
                zoomInButtonText.text = "+";
            }

            if (zoomOutButtonRoot == null)
            {
                zoomOutButtonRoot = EnsurePanel("ZoomOutButton", safeAreaRoot, utilityButtonTint);
                zoomOutButtonImage = zoomOutButtonRoot.GetComponent<Image>();
                zoomOutButton = zoomOutButtonRoot.GetComponent<Button>() ?? zoomOutButtonRoot.gameObject.AddComponent<Button>();
                zoomOutButton.onClick.RemoveListener(HandleZoomOutButtonPressed);
                zoomOutButton.onClick.AddListener(HandleZoomOutButtonPressed);
                zoomOutButtonText = EnsureText("Label", zoomOutButtonRoot, runtimeFont, 20f, TextAlignmentOptions.Center);
                zoomOutButtonText.text = "-";
            }

            if (centerButtonRoot == null)
            {
                centerButtonRoot = EnsurePanel("CenterButton", safeAreaRoot, utilityButtonTint);
                centerButtonImage = centerButtonRoot.GetComponent<Image>();
                centerButton = centerButtonRoot.GetComponent<Button>() ?? centerButtonRoot.gameObject.AddComponent<Button>();
                centerButton.onClick.RemoveListener(HandleCenterButtonPressed);
                centerButton.onClick.AddListener(HandleCenterButtonPressed);
                centerButtonText = EnsureText("Label", centerButtonRoot, runtimeFont, 12f, TextAlignmentOptions.Center);
                centerButtonText.text = "CENTER";
            }

            RuntimeUIFontUtility.ApplyRecursively(safeAreaRoot);
        }

        private void ApplySafeAreaAndLayout(bool forceRefresh)
        {
            if (safeAreaRoot == null)
            {
                return;
            }

            RectTransform root = transform as RectTransform;
            if (root == null)
            {
                return;
            }

            Rect safeArea = Screen.safeArea;
            Vector2 rootSize = root.rect.size;
            if (!forceRefresh && safeArea == lastSafeArea && rootSize == lastRootSize)
            {
                return;
            }

            lastSafeArea = safeArea;
            lastRootSize = rootSize;

            Vector2 anchorMin = new(
                Screen.width <= 0 ? 0f : safeArea.xMin / Screen.width,
                Screen.height <= 0 ? 0f : safeArea.yMin / Screen.height);
            Vector2 anchorMax = new(
                Screen.width <= 0 ? 1f : safeArea.xMax / Screen.width,
                Screen.height <= 0 ? 1f : safeArea.yMax / Screen.height);

            safeAreaRoot.anchorMin = anchorMin;
            safeAreaRoot.anchorMax = anchorMax;
            safeAreaRoot.offsetMin = Vector2.zero;
            safeAreaRoot.offsetMax = Vector2.zero;

            if (summonPlacementPreviewRoot != null)
            {
                summonPlacementPreviewRoot.anchorMin = Vector2.zero;
                summonPlacementPreviewRoot.anchorMax = Vector2.one;
                summonPlacementPreviewRoot.offsetMin = Vector2.zero;
                summonPlacementPreviewRoot.offsetMax = Vector2.zero;
            }

            float safeWidth = safeAreaRoot.rect.width > 1f ? safeAreaRoot.rect.width : root.rect.width;
            float safeHeight = safeAreaRoot.rect.height > 1f ? safeAreaRoot.rect.height : root.rect.height;
            float scale = Mathf.Clamp(safeWidth / 720f, 0.84f, 1.12f);
            float aspect = safeWidth <= 0.001f ? 1f : safeHeight / safeWidth;
            bool compact = safeWidth <= 560f || aspect >= 1.65f;
            float widthBlend = Mathf.Clamp01((safeWidth - 360f) / 300f);
            float handLayoutScale = RuntimeCanvasLayoutUtility.ResolveSoftScale(root, 0.38f, 1.48f);
            float handBottomMargin = Mathf.Lerp(10f, 14f, widthBlend) * handLayoutScale;
            float handHeight = Mathf.Lerp(140f, 148f, widthBlend) * handLayoutScale;
            float handTop = handBottomMargin + handHeight;
            float moveTrayCenterY = handTop + Mathf.Lerp(64f, 78f, widthBlend);
            float tapPadBottomOffset = handTop + Mathf.Lerp(2f, 6f, widthBlend);
            float actionTrayCenterY = handTop + Mathf.Lerp(184f, 210f, widthBlend);

            if (stickRoot != null && stickRoot.TryGetComponent(out Image stickPanelImage))
            {
                stickPanelImage.color = new Color(0.04f, 0.08f, 0.13f, compact ? 0.18f : 0.22f);
            }

            if (stickRoot != null)
            {
                float stickSize = Mathf.Lerp(126f, 146f, widthBlend);
                stickRoot.anchorMin = new Vector2(0f, 0f);
                stickRoot.anchorMax = new Vector2(0f, 0f);
                stickRoot.pivot = new Vector2(0.5f, 0.5f);
                stickRoot.sizeDelta = Vector2.one * stickSize;
                stickRoot.anchoredPosition = new Vector2((stickSize * 0.5f) + (12f * scale), moveTrayCenterY);

                if (stickVisual != null)
                {
                    stickVisual.anchorMin = new Vector2(0.5f, 0.5f);
                    stickVisual.anchorMax = new Vector2(0.5f, 0.5f);
                    stickVisual.pivot = new Vector2(0.5f, 0.5f);
                    stickVisual.sizeDelta = Vector2.one * (stickSize * 0.76f);
                    stickVisual.anchoredPosition = Vector2.zero;
                }

                if (stickKnob != null)
                {
                    stickKnob.anchorMin = new Vector2(0.5f, 0.5f);
                    stickKnob.anchorMax = new Vector2(0.5f, 0.5f);
                    stickKnob.pivot = new Vector2(0.5f, 0.5f);
                    stickKnob.sizeDelta = Vector2.one * (stickSize * 0.34f);
                }

                if (moveHintText != null)
                {
                    RectTransform hintRect = moveHintText.rectTransform;
                    hintRect.anchorMin = new Vector2(0.5f, 1f);
                    hintRect.anchorMax = new Vector2(0.5f, 1f);
                    hintRect.pivot = new Vector2(0.5f, 0f);
                    hintRect.sizeDelta = new Vector2(96f, 20f);
                    hintRect.anchoredPosition = new Vector2(0f, 6f);
                    moveHintText.fontSize = Mathf.Lerp(11f, 14f, widthBlend);
                }
            }

            if (moveTrayRoot != null)
            {
                float trayWidth = Mathf.Lerp(144f, 164f, widthBlend);
                float trayHeight = Mathf.Lerp(156f, 178f, widthBlend);
                moveTrayRoot.anchorMin = new Vector2(0f, 0f);
                moveTrayRoot.anchorMax = new Vector2(0f, 0f);
                moveTrayRoot.pivot = new Vector2(0.5f, 0.5f);
                moveTrayRoot.sizeDelta = new Vector2(trayWidth, trayHeight);
                moveTrayRoot.anchoredPosition = new Vector2((trayWidth * 0.5f) + (8f * scale), moveTrayCenterY);

                if (moveTrayRoot.TryGetComponent(out Image moveTrayImage))
                {
                    moveTrayImage.color = new Color(0.06f, 0.1f, 0.16f, compact ? 0.22f : 0.26f);
                }
            }

            if (tapMovePadRoot != null)
            {
                tapMovePadRoot.anchorMin = new Vector2(0f, 0f);
                tapMovePadRoot.anchorMax = new Vector2(1f, 1f);
                tapMovePadRoot.offsetMin = new Vector2(8f, tapPadBottomOffset);
                tapMovePadRoot.offsetMax = new Vector2(-8f, -8f);

                if (tapMovePadHintText != null)
                {
                    RectTransform hintRect = tapMovePadHintText.rectTransform;
                    hintRect.anchorMin = new Vector2(0.5f, 1f);
                    hintRect.anchorMax = new Vector2(0.5f, 1f);
                    hintRect.pivot = new Vector2(0.5f, 1f);
                    hintRect.sizeDelta = new Vector2(220f, 22f);
                    hintRect.anchoredPosition = new Vector2(0f, -8f);
                    tapMovePadHintText.fontSize = Mathf.Lerp(12f, 15f, widthBlend);
                }
            }

            if (targetLockMarkerRoot != null)
            {
                targetLockMarkerRoot.anchorMin = new Vector2(0.5f, 0.5f);
                targetLockMarkerRoot.anchorMax = new Vector2(0.5f, 0.5f);
                targetLockMarkerRoot.pivot = new Vector2(0.5f, 0.5f);
                targetLockMarkerRoot.sizeDelta = new Vector2(Mathf.Lerp(74f, 92f, widthBlend), Mathf.Lerp(26f, 32f, widthBlend));

                if (targetLockMarkerText != null)
                {
                    RectTransform markerTextRect = targetLockMarkerText.rectTransform;
                    markerTextRect.anchorMin = Vector2.zero;
                    markerTextRect.anchorMax = Vector2.one;
                    markerTextRect.offsetMin = Vector2.zero;
                    markerTextRect.offsetMax = Vector2.zero;
                    targetLockMarkerText.fontSize = Mathf.Lerp(11f, 13f, widthBlend);
                }
            }

            if (directDodgePadRoot != null)
            {
                float dodgePadBottom = handTop + Mathf.Lerp(6f, 10f, widthBlend);
                float dodgePadTop = dodgePadBottom + Mathf.Lerp(46f, 56f, widthBlend);
                directDodgePadRoot.anchorMin = new Vector2(0f, 0f);
                directDodgePadRoot.anchorMax = new Vector2(1f, 0f);
                directDodgePadRoot.offsetMin = new Vector2(8f, dodgePadBottom);
                directDodgePadRoot.offsetMax = new Vector2(-8f, dodgePadTop);
                directDodgePadRoot.SetAsLastSibling();

                if (directDodgeTitleText != null)
                {
                    RectTransform titleRect = directDodgeTitleText.rectTransform;
                    titleRect.anchorMin = new Vector2(0.5f, 1f);
                    titleRect.anchorMax = new Vector2(0.5f, 1f);
                    titleRect.pivot = new Vector2(0.5f, 1f);
                    titleRect.sizeDelta = new Vector2(200f, 20f);
                    titleRect.anchoredPosition = new Vector2(0f, -7f);
                    directDodgeTitleText.fontSize = Mathf.Lerp(12.5f, 14.5f, widthBlend);
                }

                if (directDodgeLeftText != null)
                {
                    RectTransform leftRect = directDodgeLeftText.rectTransform;
                    leftRect.anchorMin = new Vector2(0.25f, 0.5f);
                    leftRect.anchorMax = new Vector2(0.25f, 0.5f);
                    leftRect.pivot = new Vector2(0.5f, 0.5f);
                    leftRect.sizeDelta = new Vector2(108f, 18f);
                    leftRect.anchoredPosition = new Vector2(0f, -5f);
                    directDodgeLeftText.fontSize = Mathf.Lerp(10.5f, 12.5f, widthBlend);
                }

                if (directDodgeRightText != null)
                {
                    RectTransform rightRect = directDodgeRightText.rectTransform;
                    rightRect.anchorMin = new Vector2(0.75f, 0.5f);
                    rightRect.anchorMax = new Vector2(0.75f, 0.5f);
                    rightRect.pivot = new Vector2(0.5f, 0.5f);
                    rightRect.sizeDelta = new Vector2(108f, 18f);
                    rightRect.anchoredPosition = new Vector2(0f, -5f);
                    directDodgeRightText.fontSize = Mathf.Lerp(10.5f, 12.5f, widthBlend);
                }
            }

            if (laneRailRoot != null)
            {
                float laneRailBottom = handTop + Mathf.Lerp(66f, 80f, widthBlend);
                float laneRailHeight = Mathf.Lerp(30f, 34f, widthBlend);
                laneRailRoot.anchorMin = new Vector2(0f, 0f);
                laneRailRoot.anchorMax = new Vector2(1f, 0f);
                float railInset = Mathf.Lerp(18f, 24f, widthBlend);
                laneRailRoot.offsetMin = new Vector2(railInset, laneRailBottom);
                laneRailRoot.offsetMax = new Vector2(-railInset, laneRailBottom + laneRailHeight);

                RectTransform laneLeftRoot = laneLeftButton != null ? laneLeftButton.transform as RectTransform : null;
                if (laneLeftRoot != null)
                {
                    laneLeftRoot.anchorMin = new Vector2(0f, 0.5f);
                    laneLeftRoot.anchorMax = new Vector2(0f, 0.5f);
                    laneLeftRoot.pivot = new Vector2(0f, 0.5f);
                    laneLeftRoot.sizeDelta = new Vector2(Mathf.Lerp(44f, 52f, widthBlend), Mathf.Lerp(30f, 36f, widthBlend));
                    laneLeftRoot.anchoredPosition = new Vector2(8f, 0f);
                }

                RectTransform laneRightRoot = laneRightButton != null ? laneRightButton.transform as RectTransform : null;
                if (laneRightRoot != null)
                {
                    laneRightRoot.anchorMin = new Vector2(1f, 0.5f);
                    laneRightRoot.anchorMax = new Vector2(1f, 0.5f);
                    laneRightRoot.pivot = new Vector2(1f, 0.5f);
                    laneRightRoot.sizeDelta = new Vector2(Mathf.Lerp(44f, 52f, widthBlend), Mathf.Lerp(30f, 36f, widthBlend));
                    laneRightRoot.anchoredPosition = new Vector2(-8f, 0f);
                }

                if (laneStatusText != null)
                {
                    RectTransform statusRect = laneStatusText.rectTransform;
                    statusRect.anchorMin = new Vector2(0.5f, 0f);
                    statusRect.anchorMax = new Vector2(0.5f, 0f);
                    statusRect.pivot = new Vector2(0.5f, 0f);
                    statusRect.sizeDelta = new Vector2(292f, 16f);
                    statusRect.anchoredPosition = new Vector2(0f, 2.5f);
                    laneStatusText.fontSize = Mathf.Lerp(11f, 12.2f, widthBlend);
                }

                if (laneMarkerContainer != null)
                {
                    laneMarkerContainer.anchorMin = new Vector2(0.5f, 1f);
                    laneMarkerContainer.anchorMax = new Vector2(0.5f, 1f);
                    laneMarkerContainer.pivot = new Vector2(0.5f, 1f);
                    laneMarkerContainer.sizeDelta = new Vector2(Mathf.Lerp(196f, 236f, widthBlend), 14f);
                    laneMarkerContainer.anchoredPosition = new Vector2(0f, -2f);

                    float markerSpacing = laneMarkerImages.Count > 1
                        ? laneMarkerContainer.sizeDelta.x / (laneMarkerImages.Count - 1f)
                        : 0f;
                    for (int index = 0; index < laneMarkerImages.Count; index++)
                    {
                        Image markerImage = laneMarkerImages[index];
                        if (markerImage == null)
                        {
                            continue;
                        }

                        RectTransform markerRect = markerImage.rectTransform;
                        markerRect.anchorMin = new Vector2(0f, 0.5f);
                        markerRect.anchorMax = new Vector2(0f, 0.5f);
                        markerRect.pivot = new Vector2(0.5f, 0.5f);
                        markerRect.sizeDelta = new Vector2(Mathf.Lerp(20f, 26f, widthBlend), Mathf.Lerp(11f, 13f, widthBlend));
                        markerRect.anchoredPosition = new Vector2((markerSpacing * index) - (laneMarkerContainer.sizeDelta.x * 0.5f), 0f);

                        if (index < laneMarkerTexts.Count && laneMarkerTexts[index] != null)
                        {
                            RectTransform labelRect = laneMarkerTexts[index].rectTransform;
                            labelRect.anchorMin = new Vector2(0.5f, 0.5f);
                            labelRect.anchorMax = new Vector2(0.5f, 0.5f);
                            labelRect.pivot = new Vector2(0.5f, 0.5f);
                            labelRect.sizeDelta = markerRect.sizeDelta;
                            labelRect.anchoredPosition = Vector2.zero;
                            laneMarkerTexts[index].fontSize = Mathf.Lerp(8.8f, 9.8f, widthBlend);
                        }
                    }
                }
            }

            if (skillButtonRoot != null)
            {
                float skillWidth = Mathf.Lerp(48f, 54f, widthBlend);
                float skillHeight = Mathf.Lerp(29f, 34f, widthBlend);
                float actionBaseY = handTop + Mathf.Lerp(16f, 20f, widthBlend);
                skillButtonRoot.anchorMin = new Vector2(1f, 0f);
                skillButtonRoot.anchorMax = new Vector2(1f, 0f);
                skillButtonRoot.pivot = new Vector2(0.5f, 0.5f);
                skillButtonRoot.sizeDelta = new Vector2(skillWidth, skillHeight);
                skillButtonRoot.anchoredPosition = new Vector2(-(skillWidth * 0.5f) - (14f * scale), actionBaseY);
                if (skillButtonText != null)
                {
                    skillButtonText.fontSize = Mathf.Lerp(9.5f, 10.5f, widthBlend);
                }
            }

            if (mapButtonRoot != null)
            {
                float mapWidth = Mathf.Lerp(36f, 44f, widthBlend);
                float mapHeight = Mathf.Lerp(14f, 18f, widthBlend);
                float mapBaseY = handTop + Mathf.Lerp(42f, 48f, widthBlend);
                mapButtonRoot.anchorMin = new Vector2(1f, 0f);
                mapButtonRoot.anchorMax = new Vector2(1f, 0f);
                mapButtonRoot.pivot = new Vector2(0.5f, 0.5f);
                mapButtonRoot.sizeDelta = new Vector2(mapWidth, mapHeight);
                mapButtonRoot.anchoredPosition = new Vector2(-(mapWidth * 0.5f) - (14f * scale), mapBaseY);
                if (mapButtonText != null)
                {
                    mapButtonText.fontSize = Mathf.Lerp(8.1f, 9.2f, widthBlend);
                }
            }

            if (moveModeButtonRoot != null)
            {
                float modeWidth = Mathf.Lerp(74f, 90f, widthBlend);
                float modeHeight = Mathf.Lerp(30f, 38f, widthBlend);
                float modeCenterY = handTop + Mathf.Lerp(158f, 182f, widthBlend);
                moveModeButtonRoot.anchorMin = new Vector2(0f, 0f);
                moveModeButtonRoot.anchorMax = new Vector2(0f, 0f);
                moveModeButtonRoot.pivot = new Vector2(0.5f, 0.5f);
                moveModeButtonRoot.sizeDelta = new Vector2(modeWidth, modeHeight);
                moveModeButtonRoot.anchoredPosition = new Vector2((modeWidth * 0.5f) + (12f * scale), modeCenterY);
            }

            if (actionTrayRoot != null)
            {
                float trayWidth = Mathf.Lerp(112f, 128f, widthBlend);
                float trayHeight = Mathf.Lerp(172f, 196f, widthBlend);
                actionTrayRoot.anchorMin = new Vector2(1f, 0f);
                actionTrayRoot.anchorMax = new Vector2(1f, 0f);
                actionTrayRoot.pivot = new Vector2(0.5f, 0.5f);
                actionTrayRoot.sizeDelta = new Vector2(trayWidth, trayHeight);
                actionTrayRoot.anchoredPosition = new Vector2(-(trayWidth * 0.5f) - (12f * scale), actionTrayCenterY);

                if (actionTrayRoot.TryGetComponent(out Image actionTrayImage))
                {
                    actionTrayImage.color = new Color(0.06f, 0.1f, 0.16f, compact ? 0.26f : 0.3f);
                }
            }

            if (posturePushButtonRoot != null)
            {
                ConfigurePostureButton(posturePushButtonRoot, posturePushButtonText, 0.5f, 0.82f, widthBlend);
            }

            if (postureHoldButtonRoot != null)
            {
                ConfigurePostureButton(postureHoldButtonRoot, postureHoldButtonText, 0.5f, 0.5f, widthBlend);
            }

            if (postureFallbackButtonRoot != null)
            {
                ConfigurePostureButton(postureFallbackButtonRoot, postureFallbackButtonText, 0.5f, 0.18f, widthBlend);
            }

            if (overviewPadRoot != null)
            {
                overviewPadRoot.anchorMin = new Vector2(0f, 0f);
                overviewPadRoot.anchorMax = new Vector2(1f, 1f);
                overviewPadRoot.offsetMin = new Vector2(8f, tapPadBottomOffset);
                overviewPadRoot.offsetMax = new Vector2(-8f, -8f);

                if (overviewPadHintText != null)
                {
                    RectTransform hintRect = overviewPadHintText.rectTransform;
                    hintRect.anchorMin = new Vector2(0.5f, 1f);
                    hintRect.anchorMax = new Vector2(0.5f, 1f);
                    hintRect.pivot = new Vector2(0.5f, 1f);
                    hintRect.sizeDelta = new Vector2(220f, 22f);
                    hintRect.anchoredPosition = new Vector2(0f, -8f);
                    overviewPadHintText.fontSize = Mathf.Lerp(12f, 15f, widthBlend);
                }
            }

            float utilityColumnX = -(compact ? 74f : 82f) - (18f * scale);
            if (zoomInButtonRoot != null)
            {
                zoomInButtonRoot.anchorMin = new Vector2(1f, 0.5f);
                zoomInButtonRoot.anchorMax = new Vector2(1f, 0.5f);
                zoomInButtonRoot.pivot = new Vector2(0.5f, 0.5f);
                zoomInButtonRoot.sizeDelta = new Vector2(compact ? 52f : 58f, compact ? 42f : 46f);
                zoomInButtonRoot.anchoredPosition = new Vector2(utilityColumnX, compact ? 28f : 36f);
            }

            if (zoomOutButtonRoot != null)
            {
                zoomOutButtonRoot.anchorMin = new Vector2(1f, 0.5f);
                zoomOutButtonRoot.anchorMax = new Vector2(1f, 0.5f);
                zoomOutButtonRoot.pivot = new Vector2(0.5f, 0.5f);
                zoomOutButtonRoot.sizeDelta = new Vector2(compact ? 52f : 58f, compact ? 42f : 46f);
                zoomOutButtonRoot.anchoredPosition = new Vector2(utilityColumnX, compact ? -22f : -18f);
            }

            if (centerButtonRoot != null)
            {
                centerButtonRoot.anchorMin = new Vector2(1f, 0.5f);
                centerButtonRoot.anchorMax = new Vector2(1f, 0.5f);
                centerButtonRoot.pivot = new Vector2(0.5f, 0.5f);
                centerButtonRoot.sizeDelta = new Vector2(compact ? 68f : 78f, compact ? 30f : 34f);
                centerButtonRoot.anchoredPosition = new Vector2(utilityColumnX, compact ? -70f : -66f);
            }

            RefreshStickVisual();
        }

        private void UpdateButtonVisualState()
        {
            bool isOverviewMode = battleCamera != null && battleCamera.IsOverviewMode;
            bool showDirectDodgePad = isTouchLayoutActive && isDirectDodgeModeActive && !isOverviewMode;
            bool showJoystickControls = false;
            bool showTapMovePad = isTouchLayoutActive && !isOverviewMode && !showDirectDodgePad && !isSummonPlacementActive;

            if (stickRoot != null)
            {
                stickRoot.gameObject.SetActive(showJoystickControls);
            }

            if (moveTrayRoot != null)
            {
                moveTrayRoot.gameObject.SetActive(showJoystickControls);
            }

            if (tapMovePadRoot != null)
            {
                tapMovePadRoot.gameObject.SetActive(showTapMovePad);
            }

            if (directDodgePadRoot != null)
            {
                directDodgePadRoot.gameObject.SetActive(showDirectDodgePad);
                if (showDirectDodgePad && directDodgePadRoot.TryGetComponent(out Image directDodgeImage))
                {
                    directDodgeImage.color = HasAnyDirectProjectileDanger(out bool hasActiveProjectileDanger, out _) && hasActiveProjectileDanger
                        ? directDodgeActiveTint
                        : directDodgeLockTint;
                }
            }

            if (moveModeButtonRoot != null)
            {
                moveModeButtonRoot.gameObject.SetActive(false);
            }

            if (laneRailRoot != null)
            {
                laneRailRoot.gameObject.SetActive(isTouchLayoutActive && !isOverviewMode && !showDirectDodgePad);
            }

            UpdateSummonPlacementPreviewVisuals(isOverviewMode, showDirectDodgePad);

            if (moveModeButtonImage != null)
            {
                moveModeButtonImage.color = currentInputMode == MobileMoveInputMode.TapAssist
                    ? tapAssistButtonTint
                    : utilityButtonTint;
            }

            if (moveModeButtonText != null)
            {
                moveModeButtonText.text = currentInputMode == MobileMoveInputMode.TapAssist ? "TAP" : "STICK";
            }

            if (tapMovePadHintText != null)
            {
                tapMovePadHintText.text = playerController != null && playerController.HasManualTargetLock
                    ? "목표 고정"
                    : "적 또는 목표를 눌러 고정";
                tapMovePadHintText.gameObject.SetActive(false);
            }

            if (directDodgeTitleText != null)
            {
                directDodgeTitleText.text = HasAnyDirectProjectileDanger(out bool hasActiveProjectileDanger, out _) && hasActiveProjectileDanger
                    ? "직격 위험"
                    : "회피 준비";
            }

            if (actionTrayRoot != null)
            {
                actionTrayRoot.gameObject.SetActive(false);
            }

            if (skillButtonRoot != null)
            {
                skillButtonRoot.gameObject.SetActive(isTouchLayoutActive && !showDirectDodgePad);
            }

            if (mapButtonRoot != null)
            {
                mapButtonRoot.gameObject.SetActive(isTouchLayoutActive && !showDirectDodgePad);
            }

            if (skillButtonImage != null)
            {
                if (playerSkillController == null)
                {
                    skillButtonImage.color = skillBlockedTint;
                    if (skillButtonText != null)
                    {
                        skillButtonText.text = "버스트";
                    }
                }
                else
                {
                    float currentEnergy = battleEnergySystem != null ? battleEnergySystem.CurrentEnergy : 0f;
                    float cost = playerSkillController.ActiveSkillEnergyCost;
                    if (playerSkillController.CooldownRemaining > 0.01f)
                    {
                        skillButtonImage.color = skillCooldownTint;
                        if (skillButtonText != null)
                        {
                            skillButtonText.text = $"버스트\n{playerSkillController.CooldownRemaining:0.0}s";
                        }
                    }
                    else if (currentEnergy < cost)
                    {
                        skillButtonImage.color = skillBlockedTint;
                        if (skillButtonText != null)
                        {
                            float shortage = Mathf.Max(0f, cost - currentEnergy);
                            skillButtonText.text = $"버스트\n+{Mathf.CeilToInt(shortage)}E";
                        }
                    }
                    else
                    {
                        skillButtonImage.color = skillReadyTint;
                        if (skillButtonText != null)
                        {
                            skillButtonText.text = "버스트\n준비";
                        }
                    }
                }
            }

            if (mapButtonImage != null)
            {
                mapButtonImage.color = Color.Lerp(utilityButtonTint, new Color(0.1f, 0.16f, 0.24f, 0.9f), 0.48f);
            }

            if (mapButtonText != null)
            {
                mapButtonText.text = isOverviewMode ? "복귀" : "전황";
            }

            if (overviewPadRoot != null)
            {
                overviewPadRoot.gameObject.SetActive(isTouchLayoutActive && isOverviewMode);
                if (!isOverviewMode)
                {
                    pendingOverviewDragDelta = Vector2.zero;
                    pendingOverviewZoomStep = 0f;
                    pendingOverviewCenterPress = false;
                }
            }

            bool showOverviewUtilities = battleCamera != null && battleCamera.IsOverviewMode && isTouchLayoutActive;
            if (zoomInButtonRoot != null)
            {
                zoomInButtonRoot.gameObject.SetActive(showOverviewUtilities);
            }

            if (zoomOutButtonRoot != null)
            {
                zoomOutButtonRoot.gameObject.SetActive(showOverviewUtilities);
            }

            if (centerButtonRoot != null)
            {
                centerButtonRoot.gameObject.SetActive(showOverviewUtilities);
            }

            if (zoomInButtonImage != null)
            {
                zoomInButtonImage.color = utilityButtonTint;
            }

            if (zoomOutButtonImage != null)
            {
                zoomOutButtonImage.color = utilityButtonTint;
            }

            if (centerButtonImage != null)
            {
                centerButtonImage.color = utilityButtonTint;
            }

            UpdateLaneRailVisuals();
            UpdateTargetLockMarker();
        }

        private void UpdateSummonPlacementPreviewVisuals(bool isOverviewMode, bool showDirectDodgePad)
        {
            if (summonPlacementPreviewRoot == null)
            {
                return;
            }

            bool shouldShow =
                isTouchLayoutActive &&
                isSummonPlacementActive &&
                !isOverviewMode &&
                !showDirectDodgePad;

            UpdateSummonPlacementWorldPreview(shouldShow);
            summonPlacementPreviewRoot.gameObject.SetActive(false);
            if (!shouldShow || safeAreaRoot == null)
            {
                return;
            }

            int laneIndex = BattleLaneUtility.ClampLaneIndex(previewLaneIndex);
            if (!TryResolveLanePreviewLocalPoint(laneIndex, out Vector2 laneLocalPoint, out float laneScreenWidth))
            {
                summonPlacementPreviewRoot.gameObject.SetActive(false);
                return;
            }

            BattleManager battleManager = BattleManager.Instance;
            BattleManager.SummonLanePreview preview = default;
            bool hasPreview = battleManager != null && battleManager.TryGetSummonLanePreview(laneIndex, out preview);
            float railTop = laneRailRoot != null
                ? laneRailRoot.offsetMax.y + 6f
                : safeAreaRoot.rect.yMin + (safeAreaRoot.rect.height * 0.16f);
            Vector2 guidePoint = laneLocalPoint;
            Vector2 landingPoint = laneLocalPoint;
            Vector2 pocketPoint = laneLocalPoint;
            Vector2 blockerPoint = laneLocalPoint;
            Vector2 rewardPoint = laneLocalPoint;
            if (hasPreviewDropWorldPosition)
            {
                if (TryResolveWorldPreviewLocalPoint(previewDropWorldPosition + (Vector3.up * 0.12f), out Vector2 resolvedLandingPoint))
                {
                    landingPoint = resolvedLandingPoint;
                    guidePoint = resolvedLandingPoint;
                }
            }

            if (hasPreview)
            {
                TryResolveWorldPreviewLocalPoint(preview.FirstPocketPosition + (Vector3.up * 0.25f), out pocketPoint);
                if (preview.HasBlocker)
                {
                    TryResolveWorldPreviewLocalPoint(preview.BlockerPosition + (Vector3.up * 1.1f), out blockerPoint);
                }

                if (preview.HasRewardObjective)
                {
                    TryResolveWorldPreviewLocalPoint(preview.RewardObjectivePosition + (Vector3.up * 1f), out rewardPoint);
                }
            }

            float markerY = Mathf.Clamp(
                Mathf.Max(guidePoint.y, pocketPoint.y),
                railTop + 56f,
                safeAreaRoot.rect.yMax - 76f);
            float topY = markerY;
            if (hasPreview && preview.HasBlocker)
            {
                topY = Mathf.Max(topY, blockerPoint.y);
            }

            if (hasPreview && preview.HasRewardObjective)
            {
                topY = Mathf.Max(topY, rewardPoint.y);
            }

            float guideHeight = Mathf.Max(40f, topY - railTop - 20f);
            float lineWidth = Mathf.Clamp(laneScreenWidth * 0.18f, 8f, 16f);
            float markerWidth = Mathf.Clamp(laneScreenWidth * 0.72f, 72f, 124f);
            float ribbonWidth = Mathf.Clamp(laneScreenWidth * 0.82f, 64f, 140f);
            float pulse = 0.82f + (Mathf.Sin(Time.unscaledTime * 8.4f) * 0.12f);

            if (summonPlacementLaneRibbon != null)
            {
                summonPlacementLaneRibbon.anchorMin = new Vector2(0.5f, 0f);
                summonPlacementLaneRibbon.anchorMax = new Vector2(0.5f, 0f);
                summonPlacementLaneRibbon.pivot = new Vector2(0.5f, 0f);
                summonPlacementLaneRibbon.anchoredPosition = new Vector2(guidePoint.x, railTop);
                summonPlacementLaneRibbon.sizeDelta = new Vector2(ribbonWidth, guideHeight + 20f);
            }

            if (summonPlacementLaneRibbonImage != null)
            {
                summonPlacementLaneRibbonImage.color = new Color(0.18f, 0.78f, 1f, Mathf.Clamp01(pulse * 0.14f));
            }

            if (summonPlacementGuideLine != null)
            {
                summonPlacementGuideLine.anchorMin = new Vector2(0.5f, 0f);
                summonPlacementGuideLine.anchorMax = new Vector2(0.5f, 0f);
                summonPlacementGuideLine.pivot = new Vector2(0.5f, 0f);
                summonPlacementGuideLine.anchoredPosition = new Vector2(guidePoint.x, railTop);
                summonPlacementGuideLine.sizeDelta = new Vector2(lineWidth, guideHeight);
            }

            if (summonPlacementGuideLineImage != null)
            {
                summonPlacementGuideLineImage.color = new Color(0.28f, 0.9f, 1f, Mathf.Clamp01(pulse * 0.34f));
            }

            if (summonPlacementGuideMarker != null)
            {
                summonPlacementGuideMarker.anchorMin = new Vector2(0.5f, 0f);
                summonPlacementGuideMarker.anchorMax = new Vector2(0.5f, 0f);
                summonPlacementGuideMarker.pivot = new Vector2(0.5f, 0.5f);
                summonPlacementGuideMarker.anchoredPosition = new Vector2(guidePoint.x, markerY);
                summonPlacementGuideMarker.sizeDelta = new Vector2(markerWidth, 34f);
            }

            if (summonPlacementGuideMarkerImage != null)
            {
                summonPlacementGuideMarkerImage.color = new Color(0.08f, 0.18f, 0.3f, Mathf.Clamp01(0.88f + (pulse * 0.08f)));
            }

            string stateLabel = hasPreview
                ? preview.PreviewState switch
                {
                    BattleManager.SummonLanePreviewState.Break => "돌파",
                    BattleManager.SummonLanePreviewState.Reward => "보상",
                    _ => "배치"
                }
                : "배치";
            if (summonPlacementPreviewText != null)
            {
                summonPlacementPreviewText.text = $"소환: {laneIndex + 1}번 {stateLabel}";
                summonPlacementPreviewText.fontSize = markerWidth >= 104f ? 14f : 12.5f;
            }

            PositionSummonPreviewMarker(
                summonPlacementLandingMarker,
                summonPlacementLandingMarkerImage,
                summonPlacementLandingText,
                hasPreviewDropWorldPosition,
                landingPoint,
                laneScreenWidth,
                new Color(0.98f, 0.58f, 0.96f, 0.95f),
                "소환");

            PositionSummonPreviewMarker(
                summonPlacementPocketMarker,
                summonPlacementPocketMarkerImage,
                summonPlacementPocketText,
                true,
                pocketPoint,
                laneScreenWidth,
                new Color(0.45f, 0.95f, 1f, 0.95f),
                "합류");

            PositionSummonPreviewMarker(
                summonPlacementBlockerMarker,
                summonPlacementBlockerMarkerImage,
                summonPlacementBlockerText,
                hasPreview && preview.HasBlocker,
                blockerPoint,
                laneScreenWidth,
                new Color(1f, 0.78f, 0.34f, 0.95f),
                "차단");

            PositionSummonPreviewMarker(
                summonPlacementRewardMarker,
                summonPlacementRewardMarkerImage,
                summonPlacementRewardText,
                hasPreview && preview.HasRewardObjective,
                rewardPoint,
                laneScreenWidth,
                new Color(0.42f, 1f, 0.68f, 0.95f),
                "보상");
        }

        private void UpdateSummonPlacementWorldPreview(bool shouldShow)
        {
            EnsureSummonPlacementWorldPreview();
            if (summonPlacementWorldPreviewRoot == null)
            {
                return;
            }

            summonPlacementWorldPreviewRoot.gameObject.SetActive(shouldShow);
            if (!shouldShow)
            {
                return;
            }

            ResolveReferences();
            BattleManager battleManager = BattleManager.Instance;
            if (battleManager == null)
            {
                summonPlacementWorldPreviewRoot.gameObject.SetActive(false);
                return;
            }

            int laneIndex = BattleLaneUtility.ClampLaneIndex(previewLaneIndex);
            BattleManager.SummonLanePreview preview = default;
            bool hasPreview = battleManager.TryGetSummonLanePreview(laneIndex, out preview);
            float worldY = playerController != null ? playerController.transform.position.y : 0f;
            Vector3 landingPosition = hasPreviewDropWorldPosition
                ? previewDropWorldPosition
                : hasPreview
                    ? preview.LandingPosition
                    : new Vector3(
                        battleManager.GetLaneCenterX(laneIndex),
                        worldY,
                        battleManager.SummonSpawnPoint != null ? battleManager.SummonSpawnPoint.position.z : 0f);
            Vector3 pocketPosition = hasPreview
                ? preview.FirstPocketPosition
                : landingPosition + new Vector3(0f, 0f, 1.15f);

            SetSummonPlacementWorldMarker(
                summonPlacementWorldLandingMarker,
                true,
                landingPosition + new Vector3(0f, 0.04f, 0f),
                new Vector3(0.95f, 0.02f, 0.95f));
            SetSummonPlacementWorldMarker(
                summonPlacementWorldPocketMarker,
                true,
                pocketPosition + new Vector3(0f, 0.24f, 0f),
                new Vector3(0.24f, 0.44f, 0.24f));
            SetSummonPlacementWorldMarker(
                summonPlacementWorldBlockerMarker,
                hasPreview && preview.HasBlocker,
                preview.BlockerPosition + new Vector3(0f, 0.92f, 0f),
                new Vector3(0.34f, 0.8f, 0.34f));
            SetSummonPlacementWorldMarker(
                summonPlacementWorldRewardMarker,
                hasPreview && preview.HasRewardObjective,
                preview.RewardObjectivePosition + new Vector3(0f, 0.88f, 0f),
                new Vector3(0.34f, 0.74f, 0.34f));

            if (summonPlacementWorldGuideLine != null)
            {
                summonPlacementWorldGuideLine.gameObject.SetActive(true);
                summonPlacementWorldGuideLine.SetPosition(0, landingPosition + new Vector3(0f, 0.05f, 0f));
                summonPlacementWorldGuideLine.SetPosition(1, pocketPosition + new Vector3(0f, 0.2f, 0f));
            }
        }

        private void EnsureSummonPlacementWorldPreview()
        {
            if (summonPlacementWorldPreviewRoot != null)
            {
                return;
            }

            GameObject rootObject = new("SummonPlacementWorldPreview");
            summonPlacementWorldPreviewRoot = rootObject.transform;
            summonPlacementWorldPreviewRoot.SetParent(transform.parent, false);

            summonPlacementWorldLandingMarker = CreateSummonPlacementWorldMarker(
                "Landing",
                PrimitiveType.Cylinder,
                new Color(0.98f, 0.58f, 0.96f, 0.9f));
            summonPlacementWorldPocketMarker = CreateSummonPlacementWorldMarker(
                "Pocket",
                PrimitiveType.Cube,
                new Color(0.45f, 0.95f, 1f, 0.92f));
            summonPlacementWorldBlockerMarker = CreateSummonPlacementWorldMarker(
                "Blocker",
                PrimitiveType.Cube,
                new Color(1f, 0.78f, 0.34f, 0.92f));
            summonPlacementWorldRewardMarker = CreateSummonPlacementWorldMarker(
                "Reward",
                PrimitiveType.Cube,
                new Color(0.42f, 1f, 0.68f, 0.92f));

            GameObject lineObject = new("GuideLine");
            lineObject.transform.SetParent(summonPlacementWorldPreviewRoot, false);
            summonPlacementWorldGuideLine = lineObject.AddComponent<LineRenderer>();
            summonPlacementWorldGuideLine.useWorldSpace = true;
            summonPlacementWorldGuideLine.loop = false;
            summonPlacementWorldGuideLine.positionCount = 2;
            summonPlacementWorldGuideLine.numCapVertices = 3;
            summonPlacementWorldGuideLine.startWidth = 0.08f;
            summonPlacementWorldGuideLine.endWidth = 0.08f;
            summonPlacementWorldGuideLine.alignment = LineAlignment.View;
            Material lineMaterial = CreateSummonPlacementWorldMaterial(new Color(0.45f, 0.95f, 1f, 0.8f));
            summonPlacementWorldGuideLine.material = lineMaterial;
            summonPlacementWorldGuideLine.startColor = lineMaterial.color;
            summonPlacementWorldGuideLine.endColor = lineMaterial.color;
        }

        private Transform CreateSummonPlacementWorldMarker(string name, PrimitiveType primitiveType, Color color)
        {
            GameObject markerObject = GameObject.CreatePrimitive(primitiveType);
            markerObject.name = $"SummonWorld{name}";
            markerObject.transform.SetParent(summonPlacementWorldPreviewRoot, false);
            Collider markerCollider = markerObject.GetComponent<Collider>();
            if (markerCollider != null)
            {
                Destroy(markerCollider);
            }

            Renderer markerRenderer = markerObject.GetComponent<Renderer>();
            if (markerRenderer != null)
            {
                markerRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                markerRenderer.receiveShadows = false;
                markerRenderer.material = CreateSummonPlacementWorldMaterial(color);
            }

            return markerObject.transform;
        }

        private static Material CreateSummonPlacementWorldMaterial(Color color)
        {
            Shader shader = Shader.Find("Sprites/Default");
            shader ??= Shader.Find("Universal Render Pipeline/Unlit");
            shader ??= Shader.Find("Standard");
            Material material = new(shader);
            material.color = color;
            return material;
        }

        private static void SetSummonPlacementWorldMarker(Transform marker, bool isActive, Vector3 position, Vector3 scale)
        {
            if (marker == null)
            {
                return;
            }

            marker.gameObject.SetActive(isActive);
            if (!isActive)
            {
                return;
            }

            marker.position = position;
            marker.localScale = scale;
        }

        private bool TryResolveLanePreviewLocalPoint(int laneIndex, out Vector2 localPoint, out float laneScreenWidth)
        {
            localPoint = Vector2.zero;
            laneScreenWidth = 104f;
            ResolveReferences();
            if (safeAreaRoot == null)
            {
                return false;
            }

            BattleManager battleManager = BattleManager.Instance;
            if (battleManager != null && battleViewCamera != null)
            {
                int clampedLane = BattleLaneUtility.ClampLaneIndex(laneIndex);
                if (!TryGetLaneScreenSamplePoint(clampedLane, out Vector3 laneWorld))
                {
                    return false;
                }

                Vector3 laneScreen = battleViewCamera.WorldToScreenPoint(laneWorld);
                if (laneScreen.z > 0f &&
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(safeAreaRoot, laneScreen, null, out localPoint))
                {
                    float referenceDistance = 0f;
                    if (clampedLane > 0)
                    {
                        if (TryGetLaneScreenSamplePoint(clampedLane - 1, out Vector3 leftLaneWorld))
                        {
                            referenceDistance = Mathf.Max(referenceDistance, Mathf.Abs(laneScreen.x - battleViewCamera.WorldToScreenPoint(leftLaneWorld).x));
                        }
                    }

                    if (clampedLane < battleManager.LaneCount - 1)
                    {
                        if (TryGetLaneScreenSamplePoint(clampedLane + 1, out Vector3 rightLaneWorld))
                        {
                            referenceDistance = Mathf.Max(referenceDistance, Mathf.Abs(laneScreen.x - battleViewCamera.WorldToScreenPoint(rightLaneWorld).x));
                        }
                    }

                    laneScreenWidth = referenceDistance > 1f ? referenceDistance : 104f;
                    return true;
                }
            }

            Rect rect = safeAreaRoot.rect;
            float normalized = BattleLaneUtility.DefaultLaneCount <= 1
                ? 0.5f
                : laneIndex / (float)(BattleLaneUtility.DefaultLaneCount - 1);
            localPoint = new Vector2(
                Mathf.Lerp(rect.xMin + 28f, rect.xMax - 28f, normalized),
                Mathf.Lerp(rect.yMin + (rect.height * 0.36f), rect.yMin + (rect.height * 0.52f), 0.5f));
            laneScreenWidth = rect.width / Mathf.Max(1f, BattleLaneUtility.DefaultLaneCount + 0.6f);
            return true;
        }

        private bool TryResolveWorldPreviewLocalPoint(Vector3 worldPosition, out Vector2 localPoint)
        {
            localPoint = Vector2.zero;
            ResolveReferences();
            if (safeAreaRoot == null || battleViewCamera == null)
            {
                return false;
            }

            Vector3 screenPoint = battleViewCamera.WorldToScreenPoint(worldPosition);
            if (screenPoint.z <= 0f)
            {
                return false;
            }

            return RectTransformUtility.ScreenPointToLocalPointInRectangle(safeAreaRoot, screenPoint, null, out localPoint);
        }

        private static void PositionSummonPreviewMarker(
            RectTransform markerRoot,
            Image markerImage,
            TMP_Text markerText,
            bool shouldShow,
            Vector2 localPoint,
            float laneScreenWidth,
            Color color,
            string label)
        {
            if (markerRoot == null)
            {
                return;
            }

            markerRoot.gameObject.SetActive(shouldShow);
            if (!shouldShow)
            {
                return;
            }

            markerRoot.anchorMin = new Vector2(0.5f, 0f);
            markerRoot.anchorMax = new Vector2(0.5f, 0f);
            markerRoot.pivot = new Vector2(0.5f, 0.5f);
            markerRoot.anchoredPosition = localPoint + new Vector2(0f, 10f);
            markerRoot.sizeDelta = new Vector2(Mathf.Clamp(laneScreenWidth * 0.62f, 72f, 112f), 24f);
            if (markerImage != null)
            {
                markerImage.color = color;
                markerImage.raycastTarget = false;
            }

            if (markerText != null)
            {
                markerText.text = label;
                markerText.raycastTarget = false;
            }
        }

        private void UpdateTargetLockMarker()
        {
            if (targetLockMarkerRoot == null)
            {
                return;
            }

            Transform lockedTargetTransform = null;
            ManualTargetLockKind lockedTargetKind = ManualTargetLockKind.None;
            bool shouldShowMarker =
                isTouchLayoutActive &&
                !isDirectDodgeModeActive &&
                (battleCamera == null || !battleCamera.IsOverviewMode) &&
                playerController != null &&
                playerController.TryGetManualTargetLock(out lockedTargetTransform, out _, out lockedTargetKind) &&
                lockedTargetTransform != null &&
                battleViewCamera != null &&
                safeAreaRoot != null;
            targetLockMarkerRoot.gameObject.SetActive(shouldShowMarker);
            if (!shouldShowMarker)
            {
                return;
            }

            Vector3 worldMarkerPosition = lockedTargetTransform.position + new Vector3(
                0f,
                lockedTargetKind == ManualTargetLockKind.Boss ? 1.55f : 1.15f,
                0f);
            Vector3 screenPoint = battleViewCamera.WorldToScreenPoint(worldMarkerPosition);
            if (screenPoint.z <= 0f ||
                !RectTransformUtility.ScreenPointToLocalPointInRectangle(safeAreaRoot, screenPoint, null, out Vector2 localPoint))
            {
                targetLockMarkerRoot.gameObject.SetActive(false);
                return;
            }

            targetLockMarkerRoot.anchoredPosition = localPoint + new Vector2(0f, 24f);
            targetLockMarkerRoot.SetAsLastSibling();

            if (targetLockMarkerText != null)
            {
                targetLockMarkerText.text = lockedTargetKind switch
                {
                    ManualTargetLockKind.Boss => "보스",
                    ManualTargetLockKind.Structure => "목표",
                    _ => "집중"
                };
            }

            if (targetLockMarkerImage != null)
            {
                targetLockMarkerImage.color = lockedTargetKind switch
                {
                    ManualTargetLockKind.Boss => new Color(0.38f, 0.16f, 0.16f, 0.92f),
                    ManualTargetLockKind.Structure => new Color(0.24f, 0.2f, 0.08f, 0.92f),
                    _ => new Color(0.08f, 0.2f, 0.34f, 0.92f)
                };
            }
        }

        private void UpdateLaneRailVisuals()
        {
            if (laneRailRoot == null)
            {
                return;
            }

            int activeLane = playerController != null
                ? playerController.CurrentLaneIndex
                : BattleLaneUtility.DefaultLaneCount / 2;
            int escortLane = playerController != null
                ? playerController.EscortLaneIndex
                : activeLane;
            int summonLane = summonSpawner != null
                ? summonSpawner.SelectedLaneIndex
                : BattleLaneUtility.DefaultLaneCount / 2;
            int previewLane = isSummonPlacementActive ? previewLaneIndex : summonLane;
            int lockedLaneIndex = -1;
            ManualTargetLockKind lockedTargetKind = ManualTargetLockKind.None;
            bool hasLockedTarget = playerController != null &&
                playerController.TryGetManualTargetLock(out _, out lockedLaneIndex, out lockedTargetKind);

            activeLane = BattleLaneUtility.ClampLaneIndex(activeLane);
            escortLane = BattleLaneUtility.ClampLaneIndex(escortLane);
            previewLane = BattleLaneUtility.ClampLaneIndex(previewLane);
            lockedLaneIndex = hasLockedTarget ? BattleLaneUtility.ClampLaneIndex(lockedLaneIndex) : -1;

            int recommendedLane = escortLane;
            string ribbonText;
            if (isSummonPlacementActive)
            {
                recommendedLane = previewLane;
                string previewStateLabel = "배치";
                BattleManager currentBattleManager = BattleManager.Instance;
                if (currentBattleManager != null &&
                    currentBattleManager.TryGetSummonLanePreview(previewLane, out BattleManager.SummonLanePreview preview))
                {
                    previewStateLabel = preview.PreviewState switch
                    {
                        BattleManager.SummonLanePreviewState.Break => "돌파",
                        BattleManager.SummonLanePreviewState.Reward => "보상",
                        _ => "배치"
                    };
                }

                ribbonText = $"배치: {previewLane + 1}번 {previewStateLabel}";
            }
            else if (playerController == null)
            {
                recommendedLane = ResolveDefaultRecommendLane(summonLane);
                ribbonText = $"추천: {recommendedLane + 1}번 소환";
            }
            else if (playerController.CurrentRetreatReason != PlayerRetreatReason.None)
            {
                recommendedLane = escortLane;
                ribbonText = $"후퇴: {escortLane + 1}번 이탈";
            }
            else if (hasLockedTarget)
            {
                recommendedLane = lockedLaneIndex;
                ribbonText = $"집중: {lockedLaneIndex + 1}번 {ResolveLockedTargetLabel(lockedTargetKind)}";
            }
            else if (playerController.CurrentEscortPhase == BattleManager.EscortPhase.Ready)
            {
                recommendedLane = ResolveDefaultRecommendLane(summonLane);
                ribbonText = $"추천: {recommendedLane + 1}번 소환";
            }
            else if (BattleManager.Instance != null &&
                BattleManager.Instance.TryGetLaneCombatState(escortLane, out BattleManager.LaneCombatState escortLaneState) &&
                escortLaneState.EscortPhase == BattleManager.EscortPhase.BlockerHold)
            {
                recommendedLane = escortLane;
                ribbonText = $"추천: {escortLane + 1}번 차단 파괴";
            }
            else if (TryFindRewardDirectiveLane(out int rewardLaneIndex))
            {
                recommendedLane = rewardLaneIndex;
                ribbonText = $"추천: {rewardLaneIndex + 1}번 보상";
            }
            else
            {
                recommendedLane = escortLane;
                ribbonText = playerController.CurrentEscortPhase == BattleManager.EscortPhase.Join
                    ? $"호위: {escortLane + 1}번 합류"
                    : $"호위: {escortLane + 1}번 유지";
            }

            if (laneStatusText != null)
            {
                laneStatusText.text = ribbonText;
                laneStatusText.color = new Color(0.93f, 0.97f, 1f, 0.98f);
            }

            if (laneRailImage != null)
            {
                laneRailImage.color = isSummonPlacementActive
                    ? new Color(0.08f, 0.15f, 0.22f, 0.055f)
                    : new Color(0.05f, 0.09f, 0.16f, 0.022f);
            }

            for (int index = 0; index < laneMarkerImages.Count; index++)
            {
                Image markerImage = laneMarkerImages[index];
                if (markerImage == null)
                {
                    continue;
                }

                bool isActiveLane = index == activeLane;
                bool isEscortLane = index == escortLane;
                bool isLockedLane = hasLockedTarget && index == lockedLaneIndex;
                bool isRecommendedLane = index == recommendedLane;
                Color markerColor = new Color(0.26f, 0.36f, 0.5f, 0.16f);
                if (isLockedLane)
                {
                    markerColor = new Color(1f, 0.58f, 0.22f, 0.88f);
                }
                else if (isRecommendedLane)
                {
                    markerColor = new Color(0.42f, 0.97f, 1f, 0.78f);
                }
                else if (isEscortLane)
                {
                    markerColor = new Color(0.94f, 0.97f, 1f, 0.64f);
                }
                else if (isActiveLane)
                {
                    markerColor = new Color(0.58f, 0.78f, 0.96f, 0.56f);
                }

                markerImage.color = markerColor;
                markerImage.rectTransform.sizeDelta = isLockedLane || isRecommendedLane
                    ? new Vector2(markerImage.rectTransform.sizeDelta.x, 26f)
                    : isEscortLane
                        ? new Vector2(markerImage.rectTransform.sizeDelta.x, 24f)
                        : new Vector2(markerImage.rectTransform.sizeDelta.x, 21f);

                if (index < laneMarkerTexts.Count && laneMarkerTexts[index] != null)
                {
                    TMP_Text markerText = laneMarkerTexts[index];
                    markerText.text = (index + 1).ToString();
                    markerText.color = isLockedLane || isRecommendedLane
                        ? new Color(0.05f, 0.12f, 0.16f, 1f)
                        : isEscortLane
                            ? new Color(0.18f, 0.28f, 0.36f, 1f)
                            : new Color(0.84f, 0.93f, 1f, 0.96f);
                    markerText.fontStyle = isLockedLane || isRecommendedLane ? FontStyles.Bold : FontStyles.Normal;
                }
            }
        }

        private string ResolveRetreatReason()
        {
            if (playerController == null)
            {
                return "재정비";
            }

            if (!playerController.CurrentLaneHasLiveAllies)
            {
                return "같은 레인 아군이 없습니다";
            }

            BattleManager currentBattleManager = BattleManager.Instance;
            if (currentBattleManager != null &&
                currentBattleManager.TryGetPlayerTerritoryState(out BattleManager.PlayerTerritoryState territoryState))
            {
                if (territoryState.IsInEnemyBaseZone)
                {
                    return "너무 깊게 진입했습니다";
                }

                if (territoryState.IsInCoverBreakGrace)
                {
                    return "전선이 무너졌습니다";
                }

                if (territoryState.OverextendDistance > 0.01f)
                {
                    return $"안전선보다 {territoryState.OverextendDistance:0.0}m 앞서 있습니다";
                }
            }

            if (playerController.CurrentLanePressureState == BattleManager.LanePressureState.Collapse)
            {
                return "적 압박이 우세합니다";
            }

            return "후열로 복귀합니다";
        }

        private static int ResolveDefaultRecommendLane(int referenceLane)
        {
            return BattleLaneUtility.ClampLaneIndex(referenceLane <= 2 ? 1 : 3);
        }

        private bool TryFindRewardDirectiveLane(out int laneIndex)
        {
            laneIndex = -1;
            if (playerController == null)
            {
                return false;
            }

            bool hasDirectiveContext = playerController.CurrentLaneHasLiveAllies ||
                playerController.CurrentEscortPhase == BattleManager.EscortPhase.Join;
            if (!hasDirectiveContext)
            {
                return false;
            }

            int[] priorityLanes =
            {
                BattleLaneUtility.ClampLaneIndex(playerController.EscortLaneIndex),
                1,
                3,
                2
            };

            for (int index = 0; index < priorityLanes.Length; index++)
            {
                int candidateLane = BattleLaneUtility.ClampLaneIndex(priorityLanes[index]);
                if (BattleStructure.FindNearestRoleInLaneAlongAdvance(candidateLane, isPlayerTeam: true, BattleStructureRole.RewardObjective) != null)
                {
                    laneIndex = candidateLane;
                    return true;
                }
            }

            return false;
        }

        private static string ResolveLockedTargetLabel(ManualTargetLockKind lockedTargetKind)
        {
            return lockedTargetKind switch
            {
                ManualTargetLockKind.Boss => "보스",
                ManualTargetLockKind.Structure => "목표",
                _ => "적"
            };
        }

        private void RefreshStickVisual()
        {
            if (stickKnob == null || stickRoot == null)
            {
                return;
            }

            float radius = Mathf.Min(stickRoot.rect.width, stickRoot.rect.height) * 0.24f;
            stickKnob.anchoredPosition = moveVector * radius;
            if (stickVisualImage != null)
            {
                stickVisualImage.color = moveVector.sqrMagnitude > 0.01f ? stickActiveTint : stickReadyTint;
            }

            if (stickKnobImage != null)
            {
                stickKnobImage.color = new Color(1f, 1f, 1f, moveVector.sqrMagnitude > 0.01f ? 1f : 0.92f);
            }
        }

        private static void ConfigurePostureButton(RectTransform buttonRoot, TMP_Text label, float anchorX, float anchorY, float widthBlend)
        {
            if (buttonRoot == null)
            {
                return;
            }

            buttonRoot.anchorMin = new Vector2(anchorX, anchorY);
            buttonRoot.anchorMax = new Vector2(anchorX, anchorY);
            buttonRoot.pivot = new Vector2(0.5f, 0.5f);
            buttonRoot.sizeDelta = new Vector2(Mathf.Lerp(88f, 102f, widthBlend), Mathf.Lerp(40f, 48f, widthBlend));
            buttonRoot.anchoredPosition = Vector2.zero;

            if (label != null)
            {
                RectTransform labelRect = label.rectTransform;
                labelRect.anchorMin = Vector2.zero;
                labelRect.anchorMax = Vector2.one;
                labelRect.offsetMin = Vector2.zero;
                labelRect.offsetMax = Vector2.zero;
                label.fontSize = Mathf.Lerp(13f, 15.5f, widthBlend);
            }
        }

        private void SetRootActiveState(bool isActive)
        {
            if (safeAreaRoot != null && safeAreaRoot.gameObject.activeSelf != isActive)
            {
                safeAreaRoot.gameObject.SetActive(isActive);
            }
        }

        private void UpdateDragThreshold()
        {
            if (EventSystem.current == null)
            {
                return;
            }

            if (defaultDragThreshold < 0)
            {
                defaultDragThreshold = EventSystem.current.pixelDragThreshold;
            }

            EventSystem.current.pixelDragThreshold = isTouchLayoutActive
                ? Mathf.Max(defaultDragThreshold, 24)
                : defaultDragThreshold;
        }

        private static RectTransform EnsureRect(string name, RectTransform parent)
        {
            Transform existing = parent.Find(name);
            GameObject gameObject = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform));
            gameObject.transform.SetParent(parent, false);
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.localScale = Vector3.one;
            return rect;
        }

        private static RectTransform EnsurePanel(string name, RectTransform parent, Color tint)
        {
            Transform existing = parent.Find(name);
            GameObject gameObject = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform), typeof(Image));
            gameObject.transform.SetParent(parent, false);
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.localScale = Vector3.one;

            Image image = gameObject.GetComponent<Image>();
            image.sprite = RuntimeUISpriteUtility.GetPanelSprite();
            image.type = Image.Type.Sliced;
            image.color = tint;
            image.raycastTarget = true;
            return rect;
        }

        private static TMP_Text EnsureText(string name, RectTransform parent, TMP_FontAsset font, float fontSize, TextAlignmentOptions alignment)
        {
            Transform existing = parent.Find(name);
            GameObject gameObject = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            gameObject.transform.SetParent(parent, false);
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.localScale = Vector3.one;

            TextMeshProUGUI text = gameObject.GetComponent<TextMeshProUGUI>();
            text.font = font;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.text = string.Empty;
            text.raycastTarget = false;
            RuntimeUIFontUtility.ApplyToText(text);
            return text;
        }
    }

    public class MobileStickPad : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        private MobileBattleControls owner;
        private RectTransform boundsRect;
        private RectTransform knobRect;
        private int activePointerId = int.MinValue;

        public void Initialize(MobileBattleControls controls, RectTransform bounds, RectTransform knob)
        {
            owner = controls;
            boundsRect = bounds;
            knobRect = knob;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            activePointerId = eventData.pointerId;
            UpdateStick(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.pointerId != activePointerId)
            {
                return;
            }

            UpdateStick(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId != activePointerId)
            {
                return;
            }

            activePointerId = int.MinValue;
            owner?.ClearMoveVector();
            if (knobRect != null)
            {
                knobRect.anchoredPosition = Vector2.zero;
            }
        }

        private void UpdateStick(PointerEventData eventData)
        {
            if (owner == null || boundsRect == null)
            {
                return;
            }

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(boundsRect, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
            {
                owner.ClearMoveVector();
                return;
            }

            float radius = Mathf.Min(boundsRect.rect.width, boundsRect.rect.height) * 0.5f;
            if (radius <= 0.001f)
            {
                owner.ClearMoveVector();
                return;
            }

            Vector2 normalized = Vector2.ClampMagnitude(localPoint / radius, 1f);
            owner.SetMoveVector(normalized);
        }
    }

    public class MobileTapMovePad : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        private MobileBattleControls owner;
        private int activePointerId = int.MinValue;
        private Vector2 pointerDownScreenPosition;

        public void Initialize(MobileBattleControls controls)
        {
            owner = controls;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            activePointerId = eventData.pointerId;
            pointerDownScreenPosition = eventData.position;
        }

        public void OnDrag(PointerEventData eventData)
        {
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId != activePointerId)
            {
                return;
            }

            activePointerId = int.MinValue;
            owner?.HandleBattlefieldTapRelease(eventData, pointerDownScreenPosition);
        }
    }

    public class MobileDirectDodgePad : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        private MobileBattleControls owner;
        private int activePointerId = int.MinValue;
        private Vector2 pointerDownScreenPosition;

        public void Initialize(MobileBattleControls controls)
        {
            owner = controls;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            activePointerId = eventData.pointerId;
            pointerDownScreenPosition = eventData.position;
        }

        public void OnDrag(PointerEventData eventData)
        {
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId != activePointerId)
            {
                return;
            }

            activePointerId = int.MinValue;
            owner?.HandleDirectDodgeRelease(eventData, pointerDownScreenPosition);
        }
    }

    public class MobileLaneSwipePad : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        private MobileBattleControls owner;
        private int activePointerId = int.MinValue;
        private Vector2 pointerDownScreenPosition;

        public void Initialize(MobileBattleControls controls)
        {
            owner = controls;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            activePointerId = eventData.pointerId;
            pointerDownScreenPosition = eventData.position;
        }

        public void OnDrag(PointerEventData eventData)
        {
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId != activePointerId)
            {
                return;
            }

            activePointerId = int.MinValue;
            owner?.HandleLaneSwipeRelease(eventData, pointerDownScreenPosition);
        }
    }

    public class MobileOverviewPad : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        private MobileBattleControls owner;
        private int activePointerId = int.MinValue;

        public void Initialize(MobileBattleControls controls)
        {
            owner = controls;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            activePointerId = eventData.pointerId;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.pointerId != activePointerId || owner == null)
            {
                return;
            }

            owner.AddOverviewDrag(eventData.delta);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId == activePointerId)
            {
                activePointerId = int.MinValue;
            }
        }
    }
}
