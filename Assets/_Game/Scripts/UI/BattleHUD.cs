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

        [Header("Performance")]
        [SerializeField, Range(10f, 60f)] private float hudRefreshRate = 30f;
        [SerializeField, Min(0.1f)] private float layoutRefreshInterval = 0.5f;

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
        private Image miniMapFrontlineImage;
        private bool isSubscribedToBattleManager;
        private bool isSubscribedToEnergySystem;
        private bool isSubscribedToPlayer;
        private bool isSubscribedToSkill;
        private readonly List<Image> friendlyMiniMapMarkers = new();
        private readonly List<Image> enemyMiniMapMarkers = new();
        private RectTransform bottomDockPanel;
        private RectTransform bottomDockAccent;
        private float nextHudRefreshTime;
        private float nextLayoutRefreshTime;
        private float lastLayoutWidth;
        private float lastLayoutHeight;
        private Rect lastLayoutSafeArea;
        private bool lastLayoutWasMobile;
        private bool lastLayoutWasOverview;
        private bool responsiveLayoutInitialized;

        private void Awake()
        {
            EnsureCanvasScaling();
            EnsureSupplementalUi();
            RuntimeUIFontUtility.ApplyRecursively(transform);
            ResolveReferences();
            CacheSliderFillImages();
            ApplyResponsiveLayout(force: true);
            NormalizeVisibleHudText();
        }

        private void OnEnable()
        {
            EnsureCanvasScaling();
            EnsureSupplementalUi();
            RuntimeUIFontUtility.ApplyRecursively(transform);
            ResolveReferences();
            CacheSliderFillImages();
            nextHudRefreshTime = 0f;
            nextLayoutRefreshTime = 0f;
            responsiveLayoutInitialized = false;
            ApplyResponsiveLayout(force: true);
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
            ApplyResponsiveLayout(force: true);
            NormalizeVisibleHudText();
        }

        private void Update()
        {
            if (playerController == null || battleManager == null || energySystem == null || playerSkillController == null || playerCombatController == null)
            {
                ResolveReferences();
                TrySubscribe();
            }

            ApplyResponsiveLayout();
            float unscaledTime = Time.unscaledTime;
            if (unscaledTime < nextHudRefreshTime)
            {
                return;
            }

            nextHudRefreshTime = unscaledTime + 1f / Mathf.Max(1f, hudRefreshRate);

            UpdateTimer();
            UpdateModeUI();
            UpdateSkillUI();
            UpdateWarningUI();
            UpdateFrontlineUI();
            UpdateBossTacticUI();
            UpdateMiniMap();
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
            IReadOnlyList<SummonUnit> units = SummonUnit.ActiveUnits;
            int friendlyIndex = 0;
            int enemyIndex = 0;

            for (int index = 0; index < units.Count; index++)
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

            if (miniMapFrontlineImage == null)
            {
                miniMapFrontlineImage = miniMapFrontlineMarker.GetComponent<Image>();
            }

            if (miniMapFrontlineImage != null)
            {
                Color targetColor = frontlineState.Balance >= 0.25f
                    ? frontlineAdvantageColor
                    : frontlineState.Balance <= -0.25f
                        ? frontlineDangerColor
                        : frontlineNeutralColor;
                if (miniMapFrontlineImage.color != targetColor)
                {
                    miniMapFrontlineImage.color = targetColor;
                }
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
                if (marker.color != color)
                {
                    marker.color = color;
                }

                if (!marker.gameObject.activeSelf)
                {
                    marker.gameObject.SetActive(true);
                }
            }

            return marker;
        }

        private static void SetMarkerPoolActive(List<Image> pool, int activeCount)
        {
            for (int index = 0; index < pool.Count; index++)
            {
                if (pool[index] != null)
                {
                    bool shouldBeActive = index < activeCount;
                    if (pool[index].gameObject.activeSelf != shouldBeActive)
                    {
                        pool[index].gameObject.SetActive(shouldBeActive);
                    }
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

        private void ApplyResponsiveLayout(bool force = false)
        {
            RectTransform root = transform as RectTransform;
            if (root == null)
            {
                return;
            }

            float layoutWidth = ResolveLayoutWidth(root);
            float layoutHeight = ResolveLayoutHeight(root);
            Rect safeArea = Screen.safeArea;
            bool useMobileLayout = MobileBattleControls.ShouldUseMobileLayout(root);
            bool isOverviewMode = battleCamera != null && battleCamera.IsOverviewMode;
            float unscaledTime = Application.isPlaying ? Time.unscaledTime : 0f;
            bool layoutStateUnchanged = responsiveLayoutInitialized
                && Mathf.Approximately(lastLayoutWidth, layoutWidth)
                && Mathf.Approximately(lastLayoutHeight, layoutHeight)
                && lastLayoutSafeArea == safeArea
                && lastLayoutWasMobile == useMobileLayout
                && lastLayoutWasOverview == isOverviewMode;
            if (!force && layoutStateUnchanged && unscaledTime < nextLayoutRefreshTime)
            {
                return;
            }

            responsiveLayoutInitialized = true;
            lastLayoutWidth = layoutWidth;
            lastLayoutHeight = layoutHeight;
            lastLayoutSafeArea = safeArea;
            lastLayoutWasMobile = useMobileLayout;
            lastLayoutWasOverview = isOverviewMode;
            nextLayoutRefreshTime = unscaledTime + Mathf.Max(0.1f, layoutRefreshInterval);

            float uiScale = RuntimeCanvasLayoutUtility.ResolveScale(root);
            ResolveSafeAreaPadding(root, out float safeLeft, out float safeRight, out float safeTop, out float safeBottom);
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
                ApplyHudLayout(root, panelRoot, useMobileLayout, isOverviewMode, uiScale);
                panelRoot.anchoredPosition = new Vector2(safeLeft + (16f * uiScale), -(safeTop + ((useMobileLayout ? 68f : 72f) * uiScale)));
                ApplyStatusTextLayout(panelRoot, useMobileLayout, isOverviewMode, uiScale);
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
            ApplyMobileHudVisibility(useMobileLayout, isOverviewMode);
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

}
