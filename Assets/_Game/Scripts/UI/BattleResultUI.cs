using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace IsekaiBrawl.Gameplay
{
    public class BattleResultUI : MonoBehaviour
    {
        private const string BattleSceneName = "Battle";

        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TMP_Text resultText;
        [SerializeField] private TMP_Text subtitleText;
        [SerializeField] private Button restartButton;
        [SerializeField] private bool allowRuntimeUiBootstrap;

        private bool isSubscribed;
        private bool subscribedBattleManager;
        private bool subscribedPlayerController;
        private bool subscribedSkillController;
        private bool subscribedStructureEvents;
        private Coroutine revealRoutine;
        private BattleManager battleManager;
        private PlayerController playerController;
        private PlayerSkillController playerSkillController;
        private int playerStructuresBroken;
        private int enemyStructuresBroken;
        private int skillCasts;
        private int justDodges;
        private int advanceRewardsCollected;
        private float advanceEnergyEarned;
        private float justDodgeEnergyEarned;
        private float enemyBaseDamageDealt;
        private float playerBaseDamageTaken;

        private void Awake()
        {
            EnsureRuntimeChildren();
            RuntimeUIFontUtility.ApplyRecursively(transform);
            ResolveBattleReferences();
            if (restartButton != null)
            {
                restartButton.onClick.AddListener(RestartBattle);
            }

            SetVisible(false);
        }

        private void OnDestroy()
        {
            if (restartButton != null)
            {
                restartButton.onClick.RemoveListener(RestartBattle);
            }
        }

        private void OnEnable()
        {
            RuntimeUIFontUtility.ApplyRecursively(transform);
            ResolveBattleReferences();
            TrySubscribe();
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null && isSubscribed)
            {
                GameManager.Instance.OnStateChanged -= HandleStateChanged;
                isSubscribed = false;
            }

            if (battleManager != null && subscribedBattleManager)
            {
                battleManager.OnBaseDamaged -= HandleBaseDamaged;
                subscribedBattleManager = false;
            }

            if (playerController != null && subscribedPlayerController)
            {
                playerController.OnJustDodgeRewarded -= HandleJustDodgeRewarded;
                subscribedPlayerController = false;
            }

            if (playerSkillController != null && subscribedSkillController)
            {
                playerSkillController.OnSkillActivated -= HandleSkillActivated;
                subscribedSkillController = false;
            }

            if (subscribedStructureEvents)
            {
                BattleStructure.OnStructureDestroyed -= HandleStructureDestroyed;
                subscribedStructureEvents = false;
            }
        }

        private void Start()
        {
            EnsureRuntimeChildren();
            RuntimeUIFontUtility.ApplyRecursively(transform);
            ResolveBattleReferences();
            TrySubscribe();
            if (GameManager.Instance != null)
            {
                HandleStateChanged(GameManager.Instance.CurrentState);
            }
        }

        private void TrySubscribe()
        {
            if (GameManager.Instance != null && !isSubscribed)
            {
                GameManager.Instance.OnStateChanged += HandleStateChanged;
                isSubscribed = true;
            }

            if (battleManager != null && !subscribedBattleManager)
            {
                battleManager.OnBaseDamaged += HandleBaseDamaged;
                subscribedBattleManager = true;
            }

            if (playerController != null && !subscribedPlayerController)
            {
                playerController.OnJustDodgeRewarded += HandleJustDodgeRewarded;
                subscribedPlayerController = true;
            }

            if (playerSkillController != null && !subscribedSkillController)
            {
                playerSkillController.OnSkillActivated += HandleSkillActivated;
                subscribedSkillController = true;
            }

            if (!subscribedStructureEvents)
            {
                BattleStructure.OnStructureDestroyed += HandleStructureDestroyed;
                subscribedStructureEvents = true;
            }
        }

        private void HandleStateChanged(GameState state)
        {
            switch (state)
            {
                case GameState.BattleStart:
                    ResetBattleStats();
                    SetVisible(false);
                    break;
                case GameState.Victory:
                    SetResult("\uC2B9\uB9AC", "Enemy core destroyed. Push secured.", BuildBattleSummary());
                    break;
                case GameState.Defeat:
                    SetResult("\uD328\uBC30", "Our line collapsed before the final push.", BuildBattleSummary());
                    break;
                case GameState.TimeUp:
                    SetResult("\uC2DC\uAC04 \uC885\uB8CC", "Neither side finished the battle before time expired.", BuildBattleSummary());
                    break;
                default:
                    SetVisible(false);
                    break;
            }
        }

        private void SetResult(string textValue, string subtitleValue, string summaryValue)
        {
            if (resultText != null)
            {
                resultText.text = textValue;
            }

            if (subtitleText != null)
            {
                subtitleText.text = string.IsNullOrWhiteSpace(summaryValue)
                    ? subtitleValue
                    : $"{subtitleValue}\n{summaryValue}";
            }

            if (revealRoutine != null)
            {
                StopCoroutine(revealRoutine);
            }

            revealRoutine = StartCoroutine(RevealRoutine());
        }

        private void ResolveBattleReferences()
        {
            battleManager = battleManager != null ? battleManager : BattleManager.Instance;
            if (battleManager != null && battleManager.PlayerController != null)
            {
                playerController = battleManager.PlayerController;
            }

            if (playerController == null)
            {
                playerController = FindFirstObjectByType<PlayerController>();
            }

            if (playerController != null)
            {
                playerSkillController = playerController.GetComponent<PlayerSkillController>();
            }
        }

        private void ResetBattleStats()
        {
            playerStructuresBroken = 0;
            enemyStructuresBroken = 0;
            skillCasts = 0;
            justDodges = 0;
            advanceRewardsCollected = 0;
            advanceEnergyEarned = 0f;
            justDodgeEnergyEarned = 0f;
            enemyBaseDamageDealt = 0f;
            playerBaseDamageTaken = 0f;
        }

        private void HandleBaseDamaged(bool isPlayerBase, float damageAmount, float _)
        {
            if (isPlayerBase)
            {
                playerBaseDamageTaken += Mathf.Max(0f, damageAmount);
                return;
            }

            enemyBaseDamageDealt += Mathf.Max(0f, damageAmount);
        }

        private void HandleAdvanceRewarded(float amount)
        {
            advanceRewardsCollected++;
            advanceEnergyEarned += Mathf.Max(0f, amount);
        }

        private void HandleJustDodgeRewarded(float amount)
        {
            justDodges++;
            justDodgeEnergyEarned += Mathf.Max(0f, amount);
        }

        private void HandleSkillActivated()
        {
            skillCasts++;
        }

        private void HandleStructureDestroyed(BattleStructure _, bool causedByPlayerTeam)
        {
            if (causedByPlayerTeam)
            {
                playerStructuresBroken++;
                return;
            }

            enemyStructuresBroken++;
        }

        private string BuildBattleSummary()
        {
            string statLine = $"Advance {advanceRewardsCollected} (+{Mathf.CeilToInt(advanceEnergyEarned)}E)  |  Dodge {justDodges} (+{Mathf.CeilToInt(justDodgeEnergyEarned)}E)  |  Rally {skillCasts}";
            string pressureLine = $"Structures {playerStructuresBroken}-{enemyStructuresBroken}  |  Core {Mathf.CeilToInt(enemyBaseDamageDealt)} dealt / {Mathf.CeilToInt(playerBaseDamageTaken)} taken";
            return $"{statLine}\n{pressureLine}\n{BuildPrototypeRead()}";
        }

        private string BuildPrototypeRead()
        {
            if (skillCasts == 0 && justDodges == 0)
            {
                return "Read: the read-react-swing loop did not fire. Hero hexes may still feel optional or the reward may be too hidden.";
            }

            if (justDodges > 0 && skillCasts == 0)
            {
                return "Read: dodges happened, but they were not converted into Rally Order. The payoff likely still needs stronger pull.";
            }

            if (advanceRewardsCollected < 2)
            {
                return "Read: forward pressure stayed low. The lane may still feel too risky compared to the reward.";
            }

            if (playerStructuresBroken == 0)
            {
                return "Read: the lane was held but never converted into a structure break. Summon follow-through may need more bite.";
            }

            if (enemyBaseDamageDealt > playerBaseDamageTaken && skillCasts > 0 && justDodges > 0)
            {
                return "Read: the intended loop fired. You read threat, stayed forward, and converted tempo into real push value.";
            }

            return "Read: the core loop partially fired. Keep watching whether summon timing changes which lane actually breaks.";
        }

        private void SetVisible(bool visible)
        {
            if (canvasGroup == null)
            {
                return;
            }

            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
        }

        private IEnumerator RevealRoutine()
        {
            SetVisible(true);
            RectTransform rectTransform = transform as RectTransform;
            if (rectTransform != null)
            {
                rectTransform.localScale = new Vector3(0.84f, 0.84f, 1f);
            }

            if (restartButton != null)
            {
                restartButton.interactable = false;
            }

            canvasGroup.alpha = 0f;
            float elapsed = 0f;
            while (elapsed < 0.24f)
            {
                elapsed += Time.deltaTime;
                float normalized = Mathf.Clamp01(elapsed / 0.24f);
                canvasGroup.alpha = normalized;
                if (rectTransform != null)
                {
                    rectTransform.localScale = Vector3.Lerp(new Vector3(0.84f, 0.84f, 1f), Vector3.one, normalized);
                }

                yield return null;
            }

            canvasGroup.alpha = 1f;
            if (restartButton != null)
            {
                restartButton.interactable = true;
            }

            revealRoutine = null;
        }

        private void EnsureRuntimeChildren()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            RectTransform root = transform as RectTransform;
            if (root == null)
            {
                return;
            }

            float uiScale = RuntimeCanvasLayoutUtility.ResolveScale(root);
            float rootWidth = root.rect.width > 1f ? root.rect.width : (Screen.width > 0 ? Screen.width : 720f);
            bool compactWidth = rootWidth < 720f;

            root.anchorMin = new Vector2(0.5f, 0.5f);
            root.anchorMax = new Vector2(0.5f, 0.5f);
            root.pivot = new Vector2(0.5f, 0.5f);
            root.sizeDelta = new Vector2(compactWidth ? 300f : 360f, compactWidth ? 188f : 220f) * uiScale;
            root.anchoredPosition = new Vector2(0f, compactWidth ? 10f : 18f) * uiScale;

            Image background = root.GetComponent<Image>();
            if (background != null)
            {
                background.color = new Color(0.07f, 0.1f, 0.16f, 0.82f);
                background.raycastTarget = true;
            }

            if (resultText != null)
            {
                RectTransform rect = resultText.rectTransform;
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(compactWidth ? 220f : 260f, 40f) * uiScale;
                rect.anchoredPosition = new Vector2(0f, compactWidth ? 50f : 56f) * uiScale;
                resultText.fontSize = (compactWidth ? 25f : 30f) * uiScale;
                resultText.alignment = TextAlignmentOptions.Center;
                resultText.fontStyle = FontStyles.Bold;
            }

            if (subtitleText == null)
            {
                Transform existing = transform.Find("SubtitleText");
                if (existing != null || allowRuntimeUiBootstrap)
                {
                    GameObject subtitleObject = existing != null
                        ? existing.gameObject
                        : new GameObject("SubtitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
                    subtitleObject.transform.SetParent(root, false);
                    RectTransform rect = subtitleObject.GetComponent<RectTransform>();
                    rect.anchorMin = new Vector2(0.5f, 0.5f);
                    rect.anchorMax = new Vector2(0.5f, 0.5f);
                    rect.sizeDelta = new Vector2(260f, 50f);
                    rect.anchoredPosition = new Vector2(0f, -8f);

                    subtitleText = subtitleObject.GetComponent<TextMeshProUGUI>();
                    subtitleText.font = RuntimeUIFontUtility.EnsureKoreanFallback() ?? TMP_Settings.defaultFontAsset;
                    subtitleText.fontSize = 15f;
                    subtitleText.alignment = TextAlignmentOptions.Center;
                    subtitleText.color = new Color(1f, 1f, 1f, 0.88f);
                    subtitleText.textWrappingMode = TextWrappingModes.Normal;
                    RuntimeUIFontUtility.ApplyToText(subtitleText);
                }
            }

            if (subtitleText != null)
            {
                RectTransform rect = subtitleText.rectTransform;
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(compactWidth ? 248f : 310f, compactWidth ? 88f : 116f) * uiScale;
                rect.anchoredPosition = new Vector2(0f, compactWidth ? -2f : -10f) * uiScale;
                subtitleText.fontSize = (compactWidth ? 12f : 14f) * uiScale;
                subtitleText.textWrappingMode = TextWrappingModes.Normal;
                subtitleText.overflowMode = TextOverflowModes.Ellipsis;
            }

            if (restartButton != null)
            {
                RectTransform buttonRect = restartButton.transform as RectTransform;
                if (buttonRect != null)
                {
                    buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
                    buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
                    buttonRect.pivot = new Vector2(0.5f, 0.5f);
                    buttonRect.sizeDelta = new Vector2(compactWidth ? 168f : 184f, compactWidth ? 40f : 46f) * uiScale;
                    buttonRect.anchoredPosition = new Vector2(0f, compactWidth ? -62f : -74f) * uiScale;
                }

                TMP_Text buttonLabel = restartButton.GetComponentInChildren<TMP_Text>(true);
                if (buttonLabel != null)
                {
                    buttonLabel.text = "다시 시작";
                    buttonLabel.fontSize = (compactWidth ? 13.5f : 15f) * uiScale;
                    buttonLabel.alignment = TextAlignmentOptions.Center;
                    RuntimeUIFontUtility.ApplyToText(buttonLabel);
                }
            }

            RuntimeUIFontUtility.ApplyRecursively(transform);
        }

        private static void RestartBattle()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.IsValid() && !string.IsNullOrWhiteSpace(activeScene.name))
            {
                SceneManager.LoadScene(activeScene.name, LoadSceneMode.Single);
                return;
            }

            SceneManager.LoadScene(BattleSceneName, LoadSceneMode.Single);
        }
    }

    public class BattlePresentationController : MonoBehaviour
    {
        private const int MaxFeedMessages = 3;
        private const string PresentationCanvasName = "BattlePresentationCanvas";
        private const int PresentationCanvasSortingOrder = 120;

        public static BattlePresentationController Instance { get; private set; }

        [SerializeField] private CanvasGroup curtainGroup;
        [SerializeField] private Image curtainImage;
        [SerializeField] private CanvasGroup flashGroup;
        [SerializeField] private Image flashImage;
        [SerializeField] private CanvasGroup introGroup;
        [SerializeField] private TMP_Text introTitleText;
        [SerializeField] private TMP_Text introSubtitleText;
        [SerializeField] private TMP_Text countdownText;
        [SerializeField] private CanvasGroup bannerGroup;
        [SerializeField] private Image bannerAccentImage;
        [SerializeField] private TMP_Text bannerTitleText;
        [SerializeField] private TMP_Text bannerSubtitleText;
        [SerializeField] private CanvasGroup objectiveGroup;
        [SerializeField] private Image objectiveAccentImage;
        [SerializeField] private TMP_Text objectiveText;
        [SerializeField] private TMP_Text modeText;
        [SerializeField] private CanvasGroup feedGroup;
        [SerializeField] private TMP_Text feedText;
        [SerializeField] private bool allowRuntimeUiBootstrap;

        private readonly Queue<string> feedMessages = new();

        private GameManager gameManager;
        private BattleManager battleManager;
        private EnemyAI enemyAi;
        private SummonSpawner summonSpawner;
        private PlayerSkillController playerSkillController;
        private PlayerController playerController;
        private bool subscribedGameManager;
        private bool subscribedBattleManager;
        private bool subscribedEnemyAi;
        private bool subscribedSummonSpawner;
        private bool subscribedSkillController;
        private bool subscribedPlayerController;
        private bool subscribedStructureEvents;
        private bool warnedSixtySeconds;
        private bool warnedThirtySeconds;
        private bool warnedTenSeconds;
        private bool warnedPlayerBaseCritical;
        private bool warnedEnemyBaseCritical;
        private bool showedAdvanceTutorial;
        private Coroutine introRoutine;
        private Coroutine bannerRoutine;
        private Coroutine flashRoutine;

        private bool HasRuntimeUi =>
            !MissingRuntimeUi(curtainGroup, flashGroup, introGroup, bannerGroup, objectiveGroup, feedGroup);

        public static BattlePresentationController EnsureExists()
        {
            if (Instance != null)
            {
                return Instance;
            }

            BattlePresentationController existing = FindFirstObjectByType<BattlePresentationController>();
            if (existing != null)
            {
                return existing;
            }

            Debug.LogWarning(
                "BattlePresentationController is missing from the authored scene. Runtime UI bootstrap is disabled.");
            return null;
        }

        private static bool MissingRuntimeUi(
            CanvasGroup curtain,
            CanvasGroup flash,
            CanvasGroup intro,
            CanvasGroup banner,
            CanvasGroup objective,
            CanvasGroup feed)
        {
            return curtain == null ||
                   flash == null ||
                   intro == null ||
                   banner == null ||
                   objective == null ||
                   feed == null;
        }

        private void BindExistingRuntimeUi(RectTransform root)
        {
            curtainGroup ??= root.Find("Curtain")?.GetComponent<CanvasGroup>();
            curtainImage ??= root.Find("Curtain")?.GetComponent<Image>();
            flashGroup ??= root.Find("Flash")?.GetComponent<CanvasGroup>();
            flashImage ??= root.Find("Flash")?.GetComponent<Image>();
            introGroup ??= root.Find("IntroGroup")?.GetComponent<CanvasGroup>();
            bannerGroup ??= root.Find("BannerGroup")?.GetComponent<CanvasGroup>();
            objectiveGroup ??= root.Find("ObjectiveGroup")?.GetComponent<CanvasGroup>();
            feedGroup ??= root.Find("FeedGroup")?.GetComponent<CanvasGroup>();

            if (introGroup != null)
            {
                Transform introRoot = introGroup.transform;
                introTitleText ??= introRoot.Find("IntroTitle")?.GetComponent<TMP_Text>();
                introSubtitleText ??= introRoot.Find("IntroSubtitle")?.GetComponent<TMP_Text>();
                countdownText ??= introRoot.Find("CountdownText")?.GetComponent<TMP_Text>();
            }

            if (bannerGroup != null)
            {
                Transform bannerRoot = bannerGroup.transform;
                bannerAccentImage ??= bannerRoot.Find("Accent")?.GetComponent<Image>();
                bannerTitleText ??= bannerRoot.Find("BannerTitle")?.GetComponent<TMP_Text>();
                bannerSubtitleText ??= bannerRoot.Find("BannerSubtitle")?.GetComponent<TMP_Text>();
            }

            if (objectiveGroup != null)
            {
                Transform objectiveRoot = objectiveGroup.transform;
                objectiveAccentImage ??= objectiveRoot.Find("ObjectiveAccent")?.GetComponent<Image>();
                objectiveText ??= objectiveRoot.Find("ObjectiveText")?.GetComponent<TMP_Text>();
                modeText ??= objectiveRoot.Find("ModeText")?.GetComponent<TMP_Text>();
            }

            if (feedGroup != null)
            {
                feedText ??= feedGroup.transform.Find("FeedText")?.GetComponent<TMP_Text>();
            }
        }

        private void EnsureRuntimeUi()
        {
            RectTransform root = transform as RectTransform;
            if (root == null)
            {
                return;
            }

            BindExistingRuntimeUi(root);
            if (!allowRuntimeUiBootstrap &&
                MissingRuntimeUi(curtainGroup, flashGroup, introGroup, bannerGroup, objectiveGroup, feedGroup))
            {
                ApplyOverlayLayout(root);
                return;
            }

            EnsurePresentationCanvasBinding();
            float uiScale = RuntimeCanvasLayoutUtility.ResolveScale(root);

            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero;
            root.offsetMax = Vector2.zero;

            TMP_FontAsset font = RuntimeUIFontUtility.EnsureKoreanFallback() ?? TMP_Settings.defaultFontAsset;
            Sprite sprite = RuntimeUISpriteUtility.GetPanelSprite();

            if (curtainGroup == null)
            {
                GameObject curtain = CreateFill("Curtain", root, sprite, new Color(0f, 0f, 0f, 0f));
                curtainGroup = curtain.AddComponent<CanvasGroup>();
                curtainImage = curtain.GetComponent<Image>();
            }

            if (flashGroup == null)
            {
                GameObject flash = CreateFill("Flash", root, sprite, new Color(1f, 1f, 1f, 0f));
                flashGroup = flash.AddComponent<CanvasGroup>();
                flashImage = flash.GetComponent<Image>();
            }

            if (introGroup == null)
            {
                GameObject introRoot = new("IntroGroup", typeof(RectTransform), typeof(CanvasGroup));
                introRoot.transform.SetParent(root, false);
                RectTransform introRect = introRoot.GetComponent<RectTransform>();
                Stretch(introRect);
                introGroup = introRoot.GetComponent<CanvasGroup>();

                introTitleText = CreateText("IntroTitle", introRect, font, 24f * uiScale, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(260f, 34f) * uiScale, new Vector2(0f, -132f) * uiScale, TextAlignmentOptions.Center);
                introSubtitleText = CreateText("IntroSubtitle", introRect, font, 14f * uiScale, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(420f, 32f) * uiScale, new Vector2(0f, -168f) * uiScale, TextAlignmentOptions.Center);
                countdownText = CreateText("CountdownText", introRect, font, 52f * uiScale, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(180f, 64f) * uiScale, new Vector2(0f, -214f) * uiScale, TextAlignmentOptions.Center);
            }

            if (bannerGroup == null)
            {
                GameObject bannerRoot = CreatePanel("BannerGroup", root, sprite, new Color(0f, 0f, 0f, 0.18f));
                RectTransform bannerRect = bannerRoot.GetComponent<RectTransform>();
                bannerRect.anchorMin = new Vector2(0.5f, 0.5f);
                bannerRect.anchorMax = new Vector2(0.5f, 0.5f);
                bannerRect.sizeDelta = new Vector2(360f, 58f) * uiScale;
                bannerRect.anchoredPosition = new Vector2(0f, 54f) * uiScale;
                bannerGroup = bannerRoot.AddComponent<CanvasGroup>();

                bannerAccentImage = CreateImage("Accent", bannerRect, sprite, new Vector2(420f, 4f) * uiScale, new Vector2(0f, 26f) * uiScale);
                bannerTitleText = CreateText("BannerTitle", bannerRect, font, 20f * uiScale, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(420f, 32f) * uiScale, new Vector2(0f, 4f) * uiScale, TextAlignmentOptions.Center);
                bannerSubtitleText = CreateText("BannerSubtitle", bannerRect, font, 12f * uiScale, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(420f, 22f) * uiScale, new Vector2(0f, -18f) * uiScale, TextAlignmentOptions.Center);
                if (bannerSubtitleText != null)
                {
                    bannerSubtitleText.color = new Color(1f, 1f, 1f, 0.88f);
                    bannerSubtitleText.gameObject.SetActive(false);
                }
            }

            if (objectiveGroup == null)
            {
                GameObject objectiveRoot = CreatePanel("ObjectiveGroup", root, sprite, new Color(0f, 0f, 0f, 0.34f));
                RectTransform objectiveRect = objectiveRoot.GetComponent<RectTransform>();
                objectiveRect.anchorMin = new Vector2(0.5f, 1f);
                objectiveRect.anchorMax = new Vector2(0.5f, 1f);
                objectiveRect.sizeDelta = new Vector2(620f, 68f) * uiScale;
                objectiveRect.anchoredPosition = new Vector2(0f, -102f) * uiScale;
                objectiveGroup = objectiveRoot.AddComponent<CanvasGroup>();

                objectiveAccentImage = CreateImage("ObjectiveAccent", objectiveRect, sprite, new Vector2(560f, 4f) * uiScale, new Vector2(0f, 20f) * uiScale);
                objectiveAccentImage.color = new Color(0.44f, 0.92f, 1f, 1f);
                modeText = CreateText("ModeText", objectiveRect, font, 16f * uiScale, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(300f, 22f) * uiScale, new Vector2(0f, -10f) * uiScale, TextAlignmentOptions.Center);
                objectiveText = CreateText("ObjectiveText", objectiveRect, font, 17f * uiScale, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(580f, 30f) * uiScale, new Vector2(0f, -12f) * uiScale, TextAlignmentOptions.Center);
            }

            if (feedGroup == null)
            {
                GameObject feedRoot = CreatePanel("FeedGroup", root, sprite, new Color(0f, 0f, 0f, 0.3f));
                RectTransform feedRect = feedRoot.GetComponent<RectTransform>();
                feedRect.anchorMin = new Vector2(0f, 1f);
                feedRect.anchorMax = new Vector2(0f, 1f);
                feedRect.sizeDelta = new Vector2(360f, 134f) * uiScale;
                feedRect.anchoredPosition = new Vector2(212f, -108f) * uiScale;
                feedGroup = feedRoot.AddComponent<CanvasGroup>();
                feedText = CreateText("FeedText", feedRect, font, 16f * uiScale, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(320f, 108f) * uiScale, new Vector2(18f, -12f) * uiScale, TextAlignmentOptions.TopLeft);
                if (feedText != null)
                {
                    feedText.textWrappingMode = TextWrappingModes.Normal;
                    feedText.color = new Color(1f, 1f, 1f, 0.92f);
                }
            }

            RuntimeUIFontUtility.ApplyRecursively(root);
            ApplyOverlayLayout(root);
        }

        private static Canvas ResolvePresentationCanvas(bool allowCreate)
        {
            GameObject existing = GameObject.Find(PresentationCanvasName);
            Canvas canvas = existing != null ? existing.GetComponent<Canvas>() : null;
            if (canvas == null && allowCreate)
            {
                GameObject canvasObject = new(PresentationCanvasName, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas = canvasObject.GetComponent<Canvas>();
            }

            if (canvas == null)
            {
                return null;
            }

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = PresentationCanvasSortingOrder;

            RectTransform canvasRect = canvas.transform as RectTransform;
            if (canvasRect != null)
            {
                canvasRect.anchorMin = Vector2.zero;
                canvasRect.anchorMax = Vector2.one;
                canvasRect.offsetMin = Vector2.zero;
                canvasRect.offsetMax = Vector2.zero;
            }

            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null && allowCreate)
            {
                scaler = canvas.gameObject.AddComponent<CanvasScaler>();
            }

            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(720f, 1280f);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.6f;
            }

            if (allowCreate && canvas.GetComponent<GraphicRaycaster>() == null)
            {
                canvas.gameObject.AddComponent<GraphicRaycaster>();
            }

            return canvas;
        }

        private void EnsurePresentationCanvasBinding()
        {
            Canvas canvas = ResolvePresentationCanvas(allowRuntimeUiBootstrap);
            if (canvas == null)
            {
                return;
            }

            if (transform.parent != canvas.transform)
            {
                transform.SetParent(canvas.transform, false);
            }

            RectTransform rect = transform as RectTransform;
            if (rect != null)
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }

            transform.SetAsLastSibling();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            EnsurePresentationCanvasBinding();
            EnsureRuntimeUi();
            RuntimeUIFontUtility.ApplyRecursively(transform);
            SetCanvasGroupVisible(curtainGroup, false);
            SetCanvasGroupVisible(flashGroup, false);
            SetCanvasGroupVisible(introGroup, false);
            SetCanvasGroupVisible(bannerGroup, false);
            SetCanvasGroupVisible(feedGroup, false);
            SetCanvasGroupVisible(objectiveGroup, false);
        }

        private void OnEnable()
        {
            EnsurePresentationCanvasBinding();
            EnsureRuntimeUi();
            RuntimeUIFontUtility.ApplyRecursively(transform);
            ResolveReferences();
            TrySubscribe();
        }

        private void Start()
        {
            EnsurePresentationCanvasBinding();
            EnsureRuntimeUi();
            RuntimeUIFontUtility.ApplyRecursively(transform);
            ResolveReferences();
            TrySubscribe();
            SyncCurrentState();
        }

        private void Update()
        {
            if (gameManager == null || battleManager == null || enemyAi == null || summonSpawner == null || playerSkillController == null)
            {
                ResolveReferences();
                TrySubscribe();
            }

            UpdateTimedAlerts();
            UpdateObjectiveStrip();
        }

        private void OnDisable()
        {
            if (gameManager != null && subscribedGameManager)
            {
                gameManager.OnStateChanged -= HandleGameStateChanged;
                subscribedGameManager = false;
            }

            if (battleManager != null && subscribedBattleManager)
            {
                battleManager.OnBaseDamaged -= HandleBaseDamaged;
                subscribedBattleManager = false;
            }

            if (enemyAi != null && subscribedEnemyAi)
            {
                enemyAi.OnPhaseChanged -= HandlePhaseChanged;
                enemyAi.OnSummonSpawned -= HandleSummonSpawned;
                subscribedEnemyAi = false;
            }

            if (summonSpawner != null && subscribedSummonSpawner)
            {
                summonSpawner.OnSummonSpawned -= HandleSummonSpawned;
                subscribedSummonSpawner = false;
            }

            if (playerSkillController != null && subscribedSkillController)
            {
                playerSkillController.OnSkillActivated -= HandleSkillActivated;
                playerSkillController.OnFrontlineHoldChanged -= HandleFrontlineHoldChanged;
                playerSkillController.OnOverdriveTriggered -= HandleOverdriveTriggered;
                subscribedSkillController = false;
            }

            if (playerController != null && subscribedPlayerController)
            {
                playerController.OnJustDodgeRewarded -= HandleJustDodgeRewarded;
                subscribedPlayerController = false;
            }

            if (subscribedStructureEvents)
            {
                BattleStructure.OnStructureDamaged -= HandleStructureDamaged;
                BattleStructure.OnStructureDestroyed -= HandleStructureDestroyed;
                subscribedStructureEvents = false;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void ShowWorldText(Vector3 worldPosition, string textValue, Color color, float fontSize = 4.2f, float lifetime = 0.95f)
        {
            BattleWorldText.Create(worldPosition, textValue, color, fontSize, lifetime);
        }

        public void SpawnBurst(Vector3 worldPosition, Color color, int burstCount = 18, float startSize = 0.24f, float startSpeed = 3.2f, float radius = 0.12f, float lifetime = 0.55f)
        {
            GameObject effectObject = new("BattlePresentationBurst");
            effectObject.transform.position = worldPosition;

            ParticleSystem particleSystem = effectObject.AddComponent<ParticleSystem>();
            particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ParticleSystem.MainModule main = particleSystem.main;
            main.playOnAwake = false;
            main.duration = 0.28f;
            main.loop = false;
            main.startLifetime = lifetime * 0.45f;
            main.startSpeed = startSpeed;
            main.startSize = startSize;
            main.startColor = color;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            ParticleSystem.EmissionModule emission = particleSystem.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)Mathf.Clamp(burstCount, 1, 60)) });

            ParticleSystem.ShapeModule shape = particleSystem.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = radius;

            ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particleSystem.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(color, 0f),
                    new GradientColorKey(Color.Lerp(color, Color.white, 0.35f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0f, 1f)
                });
            colorOverLifetime.color = gradient;

            particleSystem.Play();
            Destroy(effectObject, lifetime);
        }

        public void ShowScreenFlash(Color color, float peakAlpha = 0.16f, float duration = 0.24f)
        {
            if (flashGroup == null)
            {
                return;
            }

            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
            }

            flashRoutine = StartCoroutine(ScreenFlashRoutine(color, peakAlpha, duration));
        }

        public void ShowBanner(string title, string subtitle, Color accentColor, float holdDuration = 1.5f)
        {
            if (bannerGroup == null)
            {
                return;
            }

            if (bannerRoutine != null)
            {
                StopCoroutine(bannerRoutine);
            }

            bannerRoutine = StartCoroutine(BannerRoutine(title, subtitle, accentColor, holdDuration));
        }

        public void AddFeedMessage(string message, Color color)
        {
            _ = message;
            _ = color;
        }

        private void ResolveReferences()
        {
            gameManager = GameManager.Instance;
            battleManager = BattleManager.Instance;
            enemyAi = enemyAi != null ? enemyAi : FindFirstObjectByType<EnemyAI>();
            summonSpawner = summonSpawner != null ? summonSpawner : FindFirstObjectByType<SummonSpawner>();

            if (battleManager != null && battleManager.PlayerController != null)
            {
                playerController = battleManager.PlayerController;
                playerSkillController = battleManager.PlayerController.GetComponent<PlayerSkillController>();
            }

            if (playerController == null)
            {
                playerController = FindFirstObjectByType<PlayerController>();
            }

            if (playerSkillController == null)
            {
                playerSkillController = FindFirstObjectByType<PlayerSkillController>();
            }
        }

        private void TrySubscribe()
        {
            if (gameManager != null && !subscribedGameManager)
            {
                gameManager.OnStateChanged += HandleGameStateChanged;
                subscribedGameManager = true;
            }

            if (battleManager != null && !subscribedBattleManager)
            {
                battleManager.OnBaseDamaged += HandleBaseDamaged;
                subscribedBattleManager = true;
            }

            if (enemyAi != null && !subscribedEnemyAi)
            {
                enemyAi.OnPhaseChanged += HandlePhaseChanged;
                enemyAi.OnSummonSpawned += HandleSummonSpawned;
                subscribedEnemyAi = true;
            }

            if (summonSpawner != null && !subscribedSummonSpawner)
            {
                summonSpawner.OnSummonSpawned += HandleSummonSpawned;
                subscribedSummonSpawner = true;
            }

            if (playerSkillController != null && !subscribedSkillController)
            {
                playerSkillController.OnSkillActivated += HandleSkillActivated;
                playerSkillController.OnFrontlineHoldChanged += HandleFrontlineHoldChanged;
                playerSkillController.OnOverdriveTriggered += HandleOverdriveTriggered;
                subscribedSkillController = true;
            }

            if (playerController != null && !subscribedPlayerController)
            {
                playerController.OnJustDodgeRewarded += HandleJustDodgeRewarded;
                subscribedPlayerController = true;
            }

            if (!subscribedStructureEvents)
            {
                BattleStructure.OnStructureDamaged += HandleStructureDamaged;
                BattleStructure.OnStructureDestroyed += HandleStructureDestroyed;
                subscribedStructureEvents = true;
            }
        }

        private void SyncCurrentState()
        {
            if (gameManager != null)
            {
                HandleGameStateChanged(gameManager.CurrentState);
            }

            UpdateObjectiveStrip();
        }

        private void HandleGameStateChanged(GameState state)
        {
            if (!HasRuntimeUi)
            {
                return;
            }

            switch (state)
            {
                case GameState.BattleStart:
                    if (introRoutine != null)
                    {
                        StopCoroutine(introRoutine);
                    }

                    introRoutine = StartCoroutine(IntroSequence());
                    break;
                case GameState.Battle:
                    SetCanvasGroupVisible(curtainGroup, false);
                    SetCanvasGroupVisible(introGroup, false);
                    SetCanvasGroupVisible(objectiveGroup, false);
                    SetCanvasGroupVisible(feedGroup, false);
                    break;
                case GameState.Victory:
                    SetCanvasGroupVisible(objectiveGroup, false);
                    SetCanvasGroupVisible(feedGroup, false);
                    ShowScreenFlash(new Color(0.34f, 1f, 0.68f, 1f), 0.16f, 0.42f);
                    ShowBanner("승리", string.Empty, new Color(0.34f, 1f, 0.68f, 1f), 2.4f);
                    break;
                case GameState.Defeat:
                    SetCanvasGroupVisible(objectiveGroup, false);
                    SetCanvasGroupVisible(feedGroup, false);
                    ShowScreenFlash(new Color(1f, 0.38f, 0.38f, 1f), 0.18f, 0.46f);
                    ShowBanner("패배", string.Empty, new Color(1f, 0.38f, 0.38f, 1f), 2.4f);
                    break;
                case GameState.TimeUp:
                    SetCanvasGroupVisible(objectiveGroup, false);
                    SetCanvasGroupVisible(feedGroup, false);
                    ShowScreenFlash(new Color(1f, 0.82f, 0.35f, 1f), 0.14f, 0.4f);
                    ShowBanner("시간 종료", string.Empty, new Color(1f, 0.82f, 0.35f, 1f), 2.2f);
                    break;
            }
        }

        private void HandleSummonSpawned(SummonData summonData, Vector3 spawnPosition, bool isPlayerTeam)
        {
            if (summonData == null)
            {
                return;
            }

            Color accent = isPlayerTeam ? new Color(0.45f, 0.9f, 1f, 1f) : new Color(1f, 0.56f, 0.42f, 1f);
            ShowWorldText(spawnPosition + new Vector3(0f, 1.9f, 0f), summonData.shortLabel, accent, 3.8f, 0.85f);
            SpawnBurst(spawnPosition + Vector3.up * 0.8f, accent, 16, 0.18f, 2.5f, 0.1f, 0.45f);
        }

        private void HandlePhaseChanged(string phaseName)
        {
            _ = phaseName;
        }

        private void HandleSkillActivated()
        {
            if (battleManager?.PlayerController != null)
            {
                Vector3 playerPosition = battleManager.PlayerController.transform.position + new Vector3(0f, 1.8f, 0f);
                ShowWorldText(playerPosition, "버스트", new Color(0.95f, 0.9f, 0.35f, 1f), 4.5f, 0.95f);
                SpawnBurst(playerPosition, new Color(0.95f, 0.9f, 0.35f, 1f), 22, 0.22f, 3.8f, 0.18f, 0.6f);
            }

            ShowScreenFlash(new Color(0.95f, 0.9f, 0.35f, 1f), 0.14f, 0.26f);
        }

        private void HandleAdvanceRewarded(float amount)
        {
            if (!showedAdvanceTutorial)
            {
                showedAdvanceTutorial = true;
                ShowBanner("길 열림", string.Empty, new Color(0.58f, 0.9f, 1f, 1f), 0.54f);
            }

            if (playerController != null)
            {
                ShowWorldText(
                    playerController.transform.position + new Vector3(0f, 2.1f, 0f),
                    $"+{Mathf.CeilToInt(amount)}E",
                    new Color(0.64f, 0.92f, 1f, 1f),
                    3.9f,
                    0.8f);
            }
        }

        private void HandleJustDodgeRewarded(float amount)
        {
            if (playerController != null)
            {
                ShowWorldText(
                    playerController.transform.position + new Vector3(0f, 2.1f, 0f),
                    $"+{Mathf.CeilToInt(amount)}E",
                    new Color(0.6f, 0.98f, 1f, 1f),
                    3.9f,
                    0.8f);
            }
        }

        private void HandleOverdriveTriggered()
        {
            if (playerController != null)
            {
                SpawnBurst(playerController.transform.position + new Vector3(0f, 1.4f, 0f), new Color(0.58f, 0.96f, 1f, 1f), 18, 0.2f, 3.2f, 0.12f, 0.55f);
            }

            ShowBanner("버스트 강화", string.Empty, new Color(0.58f, 0.96f, 1f, 1f), 0.8f);
        }

        private void HandleFrontlineHoldChanged(bool isActive)
        {
            if (!isActive)
            {
                return;
            }

            ShowBanner("전선 유지", string.Empty, new Color(0.52f, 1f, 0.76f, 1f), 0.8f);
        }

        private void HandleStructureDamaged(BattleStructure structure, float amount)
        {
            if (structure == null || structure.IsDestroyed || amount < 18f)
            {
                return;
            }

            ShowWorldText(structure.transform.position + new Vector3(0f, 1.7f, 0f), $"-{Mathf.CeilToInt(amount)}", new Color(1f, 0.84f, 0.55f, 1f), 3.2f, 0.7f);
            SpawnBurst(structure.transform.position + Vector3.up * 0.8f, new Color(1f, 0.82f, 0.45f, 1f), 10, 0.14f, 2.2f, 0.08f, 0.35f);
        }

        private void HandleStructureDestroyed(BattleStructure structure, bool causedByPlayerTeam)
        {
            if (structure == null)
            {
                return;
            }

            Color accent = causedByPlayerTeam ? new Color(0.52f, 1f, 0.74f, 1f) : new Color(1f, 0.52f, 0.42f, 1f);
            string message = causedByPlayerTeam
                ? structure.Role switch
                {
                    BattleStructureRole.RewardObjective => $"+{Mathf.CeilToInt(structure.EnergyReward)}E",
                    BattleStructureRole.SiegeObjective => "돌파",
                    _ => "길 열림"
                }
                : structure.Role switch
                {
                    BattleStructureRole.RewardObjective => "보상 상실",
                    BattleStructureRole.SiegeObjective => "중앙 붕괴",
                    _ => "방어 붕괴"
                };
            string bannerTitle = causedByPlayerTeam
                ? structure.Role switch
                {
                    BattleStructureRole.RewardObjective => "보상 획득",
                    BattleStructureRole.SiegeObjective => "중앙 돌파 가능",
                    _ => "길 열림"
                }
                : structure.Role switch
                {
                    BattleStructureRole.RewardObjective => "보상 상실",
                    BattleStructureRole.SiegeObjective => "중앙 붕괴",
                    _ => "후퇴"
                };

            ShowWorldText(structure.transform.position + new Vector3(0f, 2f, 0f), message, accent, 4f, 1.05f);
            SpawnBurst(structure.transform.position + Vector3.up * 1.1f, accent, 26, 0.3f, 4.4f, 0.22f, 0.7f);
            ShowBanner(bannerTitle, string.Empty, accent, 0.54f);
        }

        private void HandleBaseDamaged(bool isPlayerBase, float damageAmount, float remainingHp)
        {
            if (battleManager == null)
            {
                return;
            }

            Transform baseTransform = battleManager.GetBaseTransform(isPlayerBase);
            if (baseTransform != null)
            {
                Color accent = isPlayerBase ? new Color(1f, 0.48f, 0.48f, 1f) : new Color(0.48f, 0.92f, 1f, 1f);
                ShowWorldText(baseTransform.position + new Vector3(0f, 1.9f, 0f), $"-{Mathf.CeilToInt(damageAmount)}", accent, 3.8f, 0.78f);
                SpawnBurst(baseTransform.position + Vector3.up * 0.6f, accent, 18, 0.2f, 2.8f, 0.14f, 0.45f);
            }

            float maxHp = isPlayerBase ? battleManager.PlayerBaseMaxHP : battleManager.EnemyBaseMaxHP;
            float ratio = maxHp <= 0.001f ? 0f : remainingHp / maxHp;
            if (isPlayerBase && ratio <= 0.3f && !warnedPlayerBaseCritical)
            {
                warnedPlayerBaseCritical = true;
                ShowBanner("후퇴", string.Empty, new Color(1f, 0.4f, 0.4f, 1f), 0.68f);
            }
            else if (!isPlayerBase && ratio <= 0.3f && !warnedEnemyBaseCritical)
            {
                warnedEnemyBaseCritical = true;
                ShowBanner("마무리", string.Empty, new Color(0.48f, 0.95f, 1f, 1f), 0.9f);
            }
        }

        private void UpdateTimedAlerts()
        {
            if (gameManager == null || gameManager.CurrentState != GameState.Battle)
            {
                return;
            }

            float remaining = gameManager.RemainingTime;
            if (!warnedSixtySeconds && remaining <= 60f)
            {
                warnedSixtySeconds = true;
                ShowBanner("1분", string.Empty, new Color(1f, 0.82f, 0.42f, 1f), 0.8f);
            }

            if (!warnedThirtySeconds && remaining <= 30f)
            {
                warnedThirtySeconds = true;
                ShowBanner("30초", string.Empty, new Color(1f, 0.62f, 0.38f, 1f), 0.8f);
            }

            if (!warnedTenSeconds && remaining <= 10f)
            {
                warnedTenSeconds = true;
                ShowBanner("10초", string.Empty, new Color(1f, 0.34f, 0.34f, 1f), 0.9f);
            }
        }

        private void UpdateObjectiveStrip()
        {
            SetCanvasGroupVisible(objectiveGroup, false);
            SetCanvasGroupVisible(feedGroup, false);
        }

        private string ResolveBattleObjectiveLine()
        {
            if (gameManager == null)
            {
                return "Escort your summons, break structures, and crush the enemy core.";
            }

            if (playerSkillController != null && playerSkillController.IsDodgeOverdriveActive)
            {
                return "Overdrive active. Cast Rally Order into the busiest lane now.";
            }

            if (gameManager.ElapsedBattleTime < 16f)
            {
                return "Opening: step forward for energy, hold one lane, and remember the boss is pressure while the top bar is the enemy core.";
            }

            int activeStructures = CountActiveStructures();
            if (activeStructures >= 5)
            {
                return "First break: escort two summons into a structure fight before the first heavy hex lands.";
            }

            if (playerSkillController != null && playerSkillController.IsFrontlineHoldActive)
            {
                return "Frontline Hold active. Spend the cheaper Rally Order while your escorts are still stacked.";
            }

            if (activeStructures >= 3)
            {
                return "Mid fight: read the hex, just dodge, then swing the cracked lane.";
            }

            if (battleManager != null && battleManager.EnemyBaseMaxHP > 0.001f)
            {
                float enemyBaseRatio = battleManager.CurrentEnemyBaseHP / battleManager.EnemyBaseMaxHP;
                if (enemyBaseRatio <= 0.35f)
                {
                    return "Enemy core is exposed. Ignore side skirmishes and finish the push.";
                }
            }

            return "Read the hex, hold one active lane, and end on the core.";
        }

        private Color ResolveObjectiveAccent()
        {
            if (gameManager == null)
            {
                return new Color(0.44f, 0.92f, 1f, 1f);
            }

            if (gameManager.CurrentState == GameState.Victory)
            {
                return new Color(0.34f, 1f, 0.68f, 1f);
            }

            if (gameManager.CurrentState == GameState.Defeat)
            {
                return new Color(1f, 0.38f, 0.38f, 1f);
            }

            if (gameManager.CurrentState == GameState.TimeUp)
            {
                return new Color(1f, 0.82f, 0.35f, 1f);
            }

            if (playerSkillController != null && playerSkillController.IsDodgeOverdriveActive)
            {
                return new Color(0.58f, 0.96f, 1f, 1f);
            }

            if (playerSkillController != null && playerSkillController.IsFrontlineHoldActive)
            {
                return new Color(0.52f, 1f, 0.76f, 1f);
            }

            if (CountActiveStructures() <= 2)
            {
                return new Color(1f, 0.72f, 0.34f, 1f);
            }

            return new Color(0.44f, 0.92f, 1f, 1f);
        }

        private static int CountActiveStructures()
        {
            BattleStructure[] structures = FindObjectsByType<BattleStructure>(FindObjectsSortMode.None);
            int activeCount = 0;
            for (int index = 0; index < structures.Length; index++)
            {
                if (structures[index] != null && !structures[index].IsDestroyed)
                {
                    activeCount++;
                }
            }

            return activeCount;
        }

        private IEnumerator IntroSequence()
        {
            if (introTitleText != null)
            {
                introTitleText.text = string.Empty;
                introTitleText.gameObject.SetActive(false);
            }

            if (introSubtitleText != null)
            {
                introSubtitleText.text = string.Empty;
                introSubtitleText.gameObject.SetActive(false);
            }

            SetCanvasGroupVisible(curtainGroup, true);
            SetCanvasGroupVisible(introGroup, true);
            curtainGroup.alpha = 0.72f;
            introGroup.alpha = 0f;
            yield return LerpCanvasGroup(introGroup, 0f, 1f, 0.14f);

            if (countdownText != null)
            {
                countdownText.text = "3";
            }

            yield return LerpCanvasGroup(curtainGroup, 0.72f, 0.16f, 0.18f);
            yield return CountdownBeat("3", new Color(0.5f, 0.88f, 1f, 1f), 0.22f);
            yield return CountdownBeat("2", new Color(0.72f, 0.94f, 1f, 1f), 0.22f);
            yield return CountdownBeat("1", new Color(0.98f, 0.92f, 0.42f, 1f), 0.22f);

            if (countdownText != null)
            {
                countdownText.text = "시작";
                countdownText.color = new Color(0.46f, 1f, 0.74f, 1f);
            }

            ShowScreenFlash(new Color(0.46f, 1f, 0.74f, 1f), 0.1f, 0.18f);
            yield return new WaitForSeconds(0.16f);
            yield return LerpCanvasGroup(introGroup, introGroup.alpha, 0f, 0.12f);
            yield return LerpCanvasGroup(curtainGroup, curtainGroup.alpha, 0f, 0.1f);
            SetCanvasGroupVisible(introGroup, false);
            SetCanvasGroupVisible(curtainGroup, false);
            introRoutine = null;
        }

        private IEnumerator CountdownBeat(string label, Color accent, float beatDuration)
        {
            if (countdownText != null)
            {
                countdownText.text = label;
                countdownText.color = accent;
                countdownText.rectTransform.localScale = Vector3.one * 1.12f;
            }

            ShowScreenFlash(accent, 0.06f, 0.12f);
            float elapsed = 0f;
            float safeDuration = Mathf.Max(0.12f, beatDuration);
            while (elapsed < safeDuration)
            {
                elapsed += Time.deltaTime;
                if (countdownText != null)
                {
                    float normalized = Mathf.Clamp01(elapsed / safeDuration);
                    countdownText.rectTransform.localScale = Vector3.Lerp(Vector3.one * 1.12f, Vector3.one, normalized);
                }

                yield return null;
            }
        }

        private IEnumerator ScreenFlashRoutine(Color color, float peakAlpha, float duration)
        {
            SetCanvasGroupVisible(flashGroup, true);
            if (flashImage != null)
            {
                flashImage.color = color;
            }

            float halfDuration = Mathf.Max(0.04f, duration * 0.5f);
            yield return LerpCanvasGroup(flashGroup, 0f, peakAlpha, halfDuration);
            yield return LerpCanvasGroup(flashGroup, flashGroup.alpha, 0f, halfDuration);
            SetCanvasGroupVisible(flashGroup, false);
            flashRoutine = null;
        }

        private IEnumerator BannerRoutine(string title, string subtitle, Color accentColor, float holdDuration)
        {
            if (bannerTitleText != null)
            {
                bannerTitleText.text = title;
                bannerTitleText.color = accentColor;
            }

            if (bannerSubtitleText != null)
            {
                bannerSubtitleText.text = subtitle;
            }

            if (bannerAccentImage != null)
            {
                bannerAccentImage.color = accentColor;
            }

            SetCanvasGroupVisible(bannerGroup, true);
            RectTransform bannerRect = bannerGroup.transform as RectTransform;
            if (bannerRect != null)
            {
                bannerRect.localScale = new Vector3(0.92f, 0.92f, 1f);
            }

            bannerGroup.alpha = 0f;
            yield return LerpCanvasGroup(bannerGroup, 0f, 1f, 0.16f);
            if (bannerRect != null)
            {
                float elapsed = 0f;
                while (elapsed < 0.2f)
                {
                    elapsed += Time.deltaTime;
                    float normalized = Mathf.Clamp01(elapsed / 0.2f);
                    bannerRect.localScale = Vector3.Lerp(new Vector3(0.92f, 0.92f, 1f), Vector3.one, normalized);
                    yield return null;
                }
            }

            yield return new WaitForSeconds(holdDuration);
            yield return LerpCanvasGroup(bannerGroup, bannerGroup.alpha, 0f, 0.18f);
            SetCanvasGroupVisible(bannerGroup, false);
            bannerRoutine = null;
        }

        private static GameObject CreateFill(string name, RectTransform parent, Sprite sprite, Color color)
        {
            Transform existing = parent.Find(name);
            GameObject panel = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            RectTransform rect = panel.GetComponent<RectTransform>();
            Stretch(rect);
            Image image = panel.GetComponent<Image>();
            image.sprite = sprite;
            image.type = Image.Type.Simple;
            image.color = color;
            image.raycastTarget = false;
            return panel;
        }

        private static GameObject CreatePanel(string name, RectTransform parent, Sprite sprite, Color color)
        {
            Transform existing = parent.Find(name);
            GameObject panel = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            RectTransform rect = panel.GetComponent<RectTransform>();
            Stretch(rect);
            Image image = panel.GetComponent<Image>();
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            image.color = color;
            return panel;
        }

        private static Image CreateImage(string name, RectTransform parent, Sprite sprite, Vector2 size, Vector2 anchoredPosition)
        {
            Transform existing = parent.Find(name);
            GameObject imageObject = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform), typeof(Image));
            imageObject.transform.SetParent(parent, false);
            RectTransform rect = imageObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;
            Image image = imageObject.GetComponent<Image>();
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            return image;
        }

        private static TMP_Text CreateText(
            string name,
            RectTransform parent,
            TMP_FontAsset font,
            float fontSize,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 size,
            Vector2 anchoredPosition,
            TextAlignmentOptions alignment)
        {
            Transform existing = parent.Find(name);
            GameObject textObject = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(parent, false);
            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;

            TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
            text.font = font;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            RuntimeUIFontUtility.ApplyToText(text);
            return text;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void SetCanvasGroupVisible(CanvasGroup group, bool visible)
        {
            if (group == null)
            {
                return;
            }

            group.alpha = visible ? Mathf.Max(group.alpha, 0f) : 0f;
            group.interactable = visible;
            group.blocksRaycasts = visible;
        }

        private void ApplyOverlayLayout(RectTransform root)
        {
            float rootWidth = root.rect.width > 1f ? root.rect.width : 1080f;
            float rootHeight = root.rect.height > 1f ? root.rect.height : 1920f;
            bool compactWidth = rootWidth < 900f;
            float uiScale = RuntimeCanvasLayoutUtility.ResolveScale(root);
            bool shortHeight = rootHeight < 1700f;

            if (introTitleText != null)
            {
                introTitleText.gameObject.SetActive(false);
                RectTransform introTitleRect = introTitleText.rectTransform;
                introTitleRect.anchorMin = new Vector2(0.5f, 1f);
                introTitleRect.anchorMax = new Vector2(0.5f, 1f);
                introTitleRect.pivot = new Vector2(0.5f, 1f);
                introTitleRect.sizeDelta = new Vector2(compactWidth ? 220f : 260f, compactWidth ? 28f : 34f) * uiScale;
                introTitleRect.anchoredPosition = new Vector2(0f, -(shortHeight ? 124f : 132f)) * uiScale;
                introTitleText.fontSize = (compactWidth ? 20f : 24f) * uiScale;
                introTitleText.overflowMode = TextOverflowModes.Ellipsis;
                introTitleText.color = new Color(0.94f, 0.97f, 1f, 0.88f);
            }

            if (introSubtitleText != null)
            {
                RectTransform introSubtitleRect = introSubtitleText.rectTransform;
                introSubtitleText.gameObject.SetActive(false);
                introSubtitleRect.anchorMin = new Vector2(0.5f, 1f);
                introSubtitleRect.anchorMax = new Vector2(0.5f, 1f);
                introSubtitleRect.pivot = new Vector2(0.5f, 1f);
                introSubtitleRect.sizeDelta = new Vector2(compactWidth ? 300f : 420f, 28f) * uiScale;
                introSubtitleRect.anchoredPosition = new Vector2(0f, -(shortHeight ? 154f : 168f)) * uiScale;
            }

            if (countdownText != null)
            {
                RectTransform countdownRect = countdownText.rectTransform;
                countdownRect.anchorMin = new Vector2(0.5f, 1f);
                countdownRect.anchorMax = new Vector2(0.5f, 1f);
                countdownRect.pivot = new Vector2(0.5f, 1f);
                countdownRect.sizeDelta = new Vector2(compactWidth ? 150f : 180f, compactWidth ? 52f : 64f) * uiScale;
                countdownRect.anchoredPosition = new Vector2(0f, -(shortHeight ? 186f : 214f)) * uiScale;
                countdownText.fontSize = (compactWidth ? 44f : 52f) * uiScale;
            }

            if (objectiveGroup != null)
            {
                RectTransform objectiveRect = objectiveGroup.transform as RectTransform;
                if (objectiveRect != null)
                {
                    objectiveRect.anchorMin = new Vector2(0.5f, 1f);
                    objectiveRect.anchorMax = new Vector2(0.5f, 1f);
                    objectiveRect.pivot = new Vector2(0.5f, 1f);
                    objectiveRect.sizeDelta = new Vector2(compactWidth ? 320f : 392f, 44f) * uiScale;
                    objectiveRect.anchoredPosition = new Vector2(0f, -74f) * uiScale;
                }

                if (modeText != null)
                {
                    modeText.fontSize = (compactWidth ? 12.5f : 13.5f) * uiScale;
                }

                if (objectiveText != null)
                {
                    objectiveText.fontSize = (compactWidth ? 13.5f : 14.5f) * uiScale;
                    objectiveText.textWrappingMode = TextWrappingModes.Normal;
                    objectiveText.overflowMode = TextOverflowModes.Ellipsis;
                }
            }

            if (feedGroup != null)
            {
                RectTransform feedRect = feedGroup.transform as RectTransform;
                if (feedRect != null)
                {
                    feedRect.anchorMin = new Vector2(0f, 1f);
                    feedRect.anchorMax = new Vector2(0f, 1f);
                    feedRect.pivot = new Vector2(0f, 1f);
                    feedRect.sizeDelta = new Vector2(compactWidth ? 320f : 340f, compactWidth ? 88f : 96f) * uiScale;
                    feedRect.anchoredPosition = new Vector2(16f, compactWidth ? -236f : -246f) * uiScale;
                }

                if (feedText != null)
                {
                    RectTransform textRect = feedText.rectTransform;
                    textRect.anchorMin = new Vector2(0f, 1f);
                    textRect.anchorMax = new Vector2(0f, 1f);
                    textRect.pivot = new Vector2(0f, 1f);
                    textRect.anchoredPosition = new Vector2(14f, -10f) * uiScale;
                    textRect.sizeDelta = new Vector2((compactWidth ? 320f : 340f) - 28f, compactWidth ? 70f : 78f) * uiScale;
                    feedText.fontSize = (compactWidth ? 14f : 15f) * uiScale;
                    feedText.textWrappingMode = TextWrappingModes.Normal;
                    feedText.overflowMode = TextOverflowModes.Ellipsis;
                }
            }
        }

        private static IEnumerator LerpCanvasGroup(CanvasGroup group, float startAlpha, float endAlpha, float duration)
        {
            if (group == null)
            {
                yield break;
            }

            group.alpha = startAlpha;
            float elapsed = 0f;
            float safeDuration = Mathf.Max(0.01f, duration);
            while (elapsed < safeDuration)
            {
                elapsed += Time.deltaTime;
                float normalized = Mathf.Clamp01(elapsed / safeDuration);
                group.alpha = Mathf.Lerp(startAlpha, endAlpha, normalized);
                yield return null;
            }

            group.alpha = endAlpha;
        }
    }

    public class BattleWorldText : MonoBehaviour
    {
        private TMP_Text textComponent;
        private Color baseColor;
        private float lifetime = 0.95f;
        private float elapsed;
        private Vector3 driftVelocity = new(0f, 1f, 0f);
        private Camera cachedMainCamera;

        public static void Create(Vector3 worldPosition, string textValue, Color color, float fontSize, float duration)
        {
            GameObject root = new("BattleWorldText", typeof(BattleWorldText), typeof(TextMeshPro));
            root.transform.position = worldPosition;

            BattleWorldText worldText = root.GetComponent<BattleWorldText>();
            worldText.textComponent = root.GetComponent<TextMeshPro>();
            worldText.baseColor = color;
            worldText.lifetime = Mathf.Max(0.2f, duration);
            worldText.Configure(textValue, color, fontSize);
        }

        private void Configure(string textValue, Color color, float fontSize)
        {
            if (textComponent == null)
            {
                textComponent = GetComponent<TextMeshPro>();
            }

            textComponent.font = RuntimeUIFontUtility.EnsureKoreanFallback() ?? TMP_Settings.defaultFontAsset;
            textComponent.text = textValue;
            textComponent.fontSize = fontSize;
            textComponent.alignment = TextAlignmentOptions.Center;
            textComponent.color = color;
            textComponent.outlineWidth = 0.12f;
            textComponent.outlineColor = new Color(0f, 0f, 0f, 0.65f);
            textComponent.textWrappingMode = TextWrappingModes.NoWrap;
            RuntimeUIFontUtility.ApplyToText(textComponent);
            transform.localScale = Vector3.one * 0.14f;
        }

        private void Update()
        {
            elapsed += Time.deltaTime;
            transform.position += driftVelocity * Time.deltaTime;

            if (cachedMainCamera == null || !cachedMainCamera.isActiveAndEnabled)
            {
                cachedMainCamera = Camera.main;
            }

            if (cachedMainCamera != null)
            {
                transform.rotation = Quaternion.LookRotation(cachedMainCamera.transform.position - transform.position, Vector3.up);
            }

            if (textComponent != null)
            {
                float normalized = Mathf.Clamp01(elapsed / lifetime);
                Color color = baseColor;
                color.a = Mathf.Lerp(1f, 0f, normalized);
                textComponent.color = color;
                transform.localScale = Vector3.one * Mathf.Lerp(0.14f, 0.18f, normalized);
            }

            if (elapsed >= lifetime)
            {
                Destroy(gameObject);
            }
        }
    }
}
