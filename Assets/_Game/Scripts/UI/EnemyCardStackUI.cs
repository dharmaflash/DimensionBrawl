using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IsekaiBrawl.Gameplay
{
    public class EnemyCardStackUI : MonoBehaviour
    {
        [SerializeField] private Image nextCardImage;
        [SerializeField] private TMP_Text countdownText;
        [SerializeField] private TMP_Text projectileCountdownText;
        [SerializeField] private TMP_Text phaseText;
        [SerializeField] private TMP_Text bossCueText;
        [SerializeField] private TMP_Text energyText;
        [SerializeField] private TMP_Text summonIntentText;
        [SerializeField] private TMP_Text volleyPatternText;
        [SerializeField] private TMP_Text headerText;
        [SerializeField] private EnemyAI enemyAI;
        [SerializeField] private Sprite fallbackSprite;
        [SerializeField, Min(1f)] private float readoutRefreshRate = 15f;

        private const float EnemyLookupInterval = 0.5f;
        private bool isSubscribed;
        private float summonCountdown;
        private float projectileCountdown;
        private Color currentPhaseColor = Color.white;
        private RectTransform panelRoot;
        private Image panelBackground;
        private float nextReadoutRefreshTime;
        private float nextEnemyLookupTime;
        private float lastLayoutWidth = float.NaN;
        private bool layoutDirty = true;
        private int lastSummonCountdownTenths = int.MinValue;
        private bool lastSummonBanking;
        private string lastProjectileCountdownText;
        private string lastBossCue;
        private Color lastBossCueColor;
        private bool hasBossCueDisplay;
        private int lastEnergyCurrent = int.MinValue;
        private int lastEnergyLimit = int.MinValue;
        private bool lastEnergyBanking;
        private bool lastEnergySyncing;
        private Color lastEnergyColor;
        private bool hasEnergyDisplay;

        private void Awake()
        {
            panelRoot = transform as RectTransform;
            EnsureSupplementalLabels();
            ApplyPanelLayout();
        }

        private void OnEnable()
        {
            ResetDisplayCaches();
            EnsureSupplementalLabels();
            layoutDirty = true;
            ApplyPanelLayout();
            TryResolveEnemyAI(true);
            TrySubscribe();
            SynchronizeEnemyState();
        }

        private void OnDisable()
        {
            if (enemyAI != null && isSubscribed)
            {
                enemyAI.OnNextSummonDecided -= UpdateNextCard;
                enemyAI.OnNextSummonCountdownChanged -= UpdateCountdown;
                enemyAI.OnProjectileCountdownChanged -= UpdateProjectileCountdown;
                enemyAI.OnPhaseChanged -= UpdatePhaseText;
                enemyAI.OnSummonIntentChanged -= UpdateSummonIntentText;
                enemyAI.OnVolleyPatternChanged -= UpdateVolleyPatternText;
            }

            isSubscribed = false;
        }

        private void Start()
        {
            EnsureSupplementalLabels();
            TryResolveEnemyAI(true);
            TrySubscribe();
            SynchronizeEnemyState();
            ApplyPanelLayout();
        }

        private void Update()
        {
            bool resolvedEnemyAI = TryResolveEnemyAI(false);
            if (!isSubscribed)
            {
                TrySubscribe();
            }

            if (resolvedEnemyAI)
            {
                SynchronizeEnemyState();
            }

            AnimateIntentPanel();

            float now = Time.unscaledTime;
            if (now < nextReadoutRefreshTime)
            {
                return;
            }

            nextReadoutRefreshTime = now + (1f / Mathf.Max(1f, readoutRefreshRate));
            UpdateBossCueText();
            UpdateEnergyText();
            ApplyPanelLayout();
        }

        private void UpdateNextCard(SummonData summonData)
        {
            if (nextCardImage == null)
            {
                return;
            }

            nextCardImage.sprite = summonData != null && summonData.cardSprite != null ? summonData.cardSprite : fallbackSprite;
            nextCardImage.color = summonData == null
                ? new Color(1f, 1f, 1f, 0.25f)
                : summonData.cardSprite != null
                    ? Color.white
                    : SummonPresentationUtility.GetCardColor(summonData);

            nextCardImage.transform.localScale = Vector3.one * 1.08f;
        }

        private void UpdateCountdown(float timeRemaining)
        {
            summonCountdown = timeRemaining;
            if (countdownText == null)
            {
                return;
            }

            bool isBanking = enemyAI != null && enemyAI.IsBankingForNextSummon && timeRemaining > 0.05f;
            int displayedTenths = Mathf.Max(0, Mathf.RoundToInt(timeRemaining * 10f));
            if (displayedTenths == lastSummonCountdownTenths && isBanking == lastSummonBanking)
            {
                return;
            }

            lastSummonCountdownTenths = displayedTenths;
            lastSummonBanking = isBanking;
            countdownText.SetText(
                isBanking ? "BANK  {0:0.0}s" : "SUMMON  {0:0.0}s",
                displayedTenths * 0.1f);
        }

        private void UpdateProjectileCountdown(float timeRemaining)
        {
            projectileCountdown = timeRemaining;
            if (projectileCountdownText == null)
            {
                return;
            }

            string displayText = ResolveProjectileCountdownText(timeRemaining);
            if (displayText == lastProjectileCountdownText)
            {
                return;
            }

            lastProjectileCountdownText = displayText;
            projectileCountdownText.text = displayText;
        }

        private void UpdatePhaseText(string phaseName)
        {
            Color phaseColor = ResolvePhaseColor(phaseName);
            if (phaseText != null)
            {
                phaseText.text = $"PHASE  {phaseName}";
                phaseText.color = phaseColor;
            }

            currentPhaseColor = phaseColor;
            hasBossCueDisplay = false;
            hasEnergyDisplay = false;
            if (nextCardImage != null)
            {
                nextCardImage.transform.localScale = Vector3.one * 1.12f;
            }
        }

        private void UpdateBossCueText()
        {
            if (bossCueText == null)
            {
                return;
            }

            if (enemyAI == null)
            {
                const string cue = "reading";
                Color color = Color.Lerp(currentPhaseColor, Color.white, 0.2f);
                ApplyBossCue(cue, color);
                return;
            }

            ApplyBossCue(
                enemyAI.CurrentBossCueShort,
                Color.Lerp(enemyAI.CurrentSignalColor, Color.white, 0.2f));
        }

        private void UpdateEnergyText()
        {
            if (energyText == null)
            {
                return;
            }

            if (enemyAI == null)
            {
                Color color = Color.Lerp(currentPhaseColor, Color.white, 0.2f);
                if (!hasEnergyDisplay || !lastEnergySyncing || color != lastEnergyColor)
                {
                    energyText.text = "ENERGY  syncing";
                    energyText.color = color;
                    lastEnergySyncing = true;
                    lastEnergyColor = color;
                    hasEnergyDisplay = true;
                }

                return;
            }

            bool isBanking = enemyAI.IsBankingForNextSummon;
            int currentEnergy = Mathf.FloorToInt(enemyAI.CurrentEnergy);
            int energyLimit = isBanking
                ? Mathf.CeilToInt(enemyAI.NextSummonCost)
                : Mathf.FloorToInt(enemyAI.MaxEnergy);
            Color displayColor = isBanking
                ? Color.Lerp(new Color(1f, 0.68f, 0.42f, 1f), Color.white, 0.15f)
                : Color.Lerp(currentPhaseColor, Color.white, 0.15f);

            if (!hasEnergyDisplay ||
                lastEnergySyncing ||
                lastEnergyBanking != isBanking ||
                lastEnergyCurrent != currentEnergy ||
                lastEnergyLimit != energyLimit)
            {
                energyText.SetText("ENERGY  {0:0}/{1:0}", currentEnergy, energyLimit);
            }

            if (!hasEnergyDisplay || displayColor != lastEnergyColor)
            {
                energyText.color = displayColor;
            }

            lastEnergyCurrent = currentEnergy;
            lastEnergyLimit = energyLimit;
            lastEnergyBanking = isBanking;
            lastEnergySyncing = false;
            lastEnergyColor = displayColor;
            hasEnergyDisplay = true;
        }

        private void ApplyBossCue(string cue, Color color)
        {
            if (!hasBossCueDisplay || cue != lastBossCue)
            {
                bossCueText.text = $"PRESSURE  {cue}";
            }

            if (!hasBossCueDisplay || color != lastBossCueColor)
            {
                bossCueText.color = color;
            }

            lastBossCue = cue;
            lastBossCueColor = color;
            hasBossCueDisplay = true;
        }

        private void UpdateVolleyPatternText(string volleyPattern)
        {
            if (volleyPatternText != null)
            {
                volleyPatternText.text = $"READ  {ResolveMaskedVolleyHint(volleyPattern)}";
                volleyPatternText.color = Color.Lerp(currentPhaseColor, Color.white, 0.2f);
            }
        }

        private void UpdateSummonIntentText(string summonIntent)
        {
            if (summonIntentText == null)
            {
                return;
            }

            string maskedHint = enemyAI != null
                ? enemyAI.CurrentSummonCueShort
                : ResolveMaskedSummonIntentHint(summonIntent);
            summonIntentText.text = $"WAVE  {maskedHint}";
            summonIntentText.color = Color.Lerp(currentPhaseColor, Color.white, 0.12f);
        }

        private void TrySubscribe()
        {
            if (enemyAI == null || isSubscribed)
            {
                return;
            }

            enemyAI.OnNextSummonDecided += UpdateNextCard;
            enemyAI.OnNextSummonCountdownChanged += UpdateCountdown;
            enemyAI.OnProjectileCountdownChanged += UpdateProjectileCountdown;
            enemyAI.OnPhaseChanged += UpdatePhaseText;
            enemyAI.OnSummonIntentChanged += UpdateSummonIntentText;
            enemyAI.OnVolleyPatternChanged += UpdateVolleyPatternText;
            isSubscribed = true;
        }

        private bool TryResolveEnemyAI(bool force)
        {
            if (enemyAI != null)
            {
                return false;
            }

            isSubscribed = false;
            float now = Time.unscaledTime;
            if (!force && now < nextEnemyLookupTime)
            {
                return false;
            }

            nextEnemyLookupTime = now + EnemyLookupInterval;
            enemyAI = FindFirstObjectByType<EnemyAI>();
            if (enemyAI == null)
            {
                return false;
            }

            ResetDisplayCaches();
            return true;
        }

        private void SynchronizeEnemyState()
        {
            if (enemyAI == null)
            {
                UpdateBossCueText();
                UpdateEnergyText();
                return;
            }

            UpdateNextCard(enemyAI.NextSummon);
            UpdateCountdown(enemyAI.RemainingSummonCountdown);
            UpdateProjectileCountdown(enemyAI.RemainingProjectileCountdown);
            UpdatePhaseText(enemyAI.CurrentPhaseName);
            UpdateBossCueText();
            UpdateEnergyText();
            UpdateSummonIntentText(enemyAI.CurrentSummonIntentName);
            UpdateVolleyPatternText(enemyAI.CurrentVolleyPatternName);
        }

        private void ResetDisplayCaches()
        {
            nextReadoutRefreshTime = 0f;
            nextEnemyLookupTime = 0f;
            lastSummonCountdownTenths = int.MinValue;
            lastSummonBanking = false;
            lastProjectileCountdownText = null;
            lastBossCue = null;
            hasBossCueDisplay = false;
            lastEnergyCurrent = int.MinValue;
            lastEnergyLimit = int.MinValue;
            lastEnergyBanking = false;
            lastEnergySyncing = false;
            hasEnergyDisplay = false;
        }

        private void AnimateIntentPanel()
        {
            if (nextCardImage != null)
            {
                float summonPulse = summonCountdown <= 1.1f
                    ? 1f + (Mathf.Sin(Time.unscaledTime * 10f) * 0.06f)
                    : Mathf.Lerp(nextCardImage.transform.localScale.x, 1f, 8f * Time.deltaTime);
                nextCardImage.transform.localScale = Vector3.Lerp(nextCardImage.transform.localScale, Vector3.one * summonPulse, 10f * Time.deltaTime);
                nextCardImage.color = Color.Lerp(nextCardImage.color, Color.Lerp(nextCardImage.color, currentPhaseColor, 0.22f), 6f * Time.deltaTime);
            }

            if (countdownText != null && summonCountdown <= 1.1f)
            {
                countdownText.color = Color.Lerp(Color.white, new Color(1f, 0.66f, 0.46f, 1f), 0.65f + (Mathf.Sin(Time.unscaledTime * 9f) * 0.2f));
            }
            else if (countdownText != null)
            {
                countdownText.color = Color.Lerp(countdownText.color, Color.white, 8f * Time.deltaTime);
            }

            if (projectileCountdownText != null)
            {
                if (projectileCountdown <= 0.9f)
                {
                    projectileCountdownText.color = Color.Lerp(
                        new Color(1f, 0.82f, 0.44f, 1f),
                        new Color(1f, 0.48f, 0.36f, 1f),
                        0.5f + (Mathf.Sin(Time.unscaledTime * 12f) * 0.25f));
                }
                else
                {
                    projectileCountdownText.color = Color.Lerp(projectileCountdownText.color, Color.white, 8f * Time.deltaTime);
                }
            }
        }

        private static string ResolveProjectileCountdownText(float timeRemaining)
        {
            if (timeRemaining <= 0.35f)
            {
                return "HEX  NOW";
            }

            if (timeRemaining <= 0.9f)
            {
                return "HEX  IMMINENT";
            }

            if (timeRemaining <= 1.8f)
            {
                return "HEX  SOON";
            }

            return "HEX  CHARGING";
        }

        private static string ResolveMaskedVolleyHint(string volleyPattern)
        {
            return volleyPattern switch
            {
                "Anchor Curse" => "Anchor curse",
                "Punish Net" => "Hero punish",
                "Cover Fire" => "Mid cover",
                "Escort Screen" => "Escort lane",
                "Needle Lock" => "Track lane",
                "Left Clamp" => "Side clamp",
                "Right Clamp" => "Side clamp",
                "Left Wall + Core" => "Wide spread",
                "Right Wall + Core" => "Wide spread",
                "Left Crush" => "All-in hex",
                "Right Crush" => "All-in hex",
                _ => "Lane pressure"
            };
        }

        private static string ResolveMaskedSummonIntentHint(string summonIntent)
        {
            return summonIntent switch
            {
                "Probe" => "Scout wave",
                "Hold Line" => "Hold lane",
                "Escort Push" => "Escort stack",
                "Break Post" => "Break structure",
                "Punish Hero" => "Hero punish",
                "Base Rush" => "Core rush",
                _ => "Flexible pressure"
            };
        }

        private static Color ResolvePhaseColor(string phaseName)
        {
            return phaseName switch
            {
                "Opening" => new Color(0.62f, 0.92f, 1f, 1f),
                "Pressure" => new Color(1f, 0.78f, 0.42f, 1f),
                "Siege" => new Color(1f, 0.58f, 0.35f, 1f),
                "Final Push" => new Color(1f, 0.34f, 0.34f, 1f),
                _ => Color.white
            };
        }

        private void EnsureSupplementalLabels()
        {
            RectTransform root = panelRoot != null ? panelRoot : transform as RectTransform;
            if (root == null)
            {
                return;
            }

            TMP_FontAsset font = countdownText != null ? countdownText.font : TMP_Settings.defaultFontAsset;
            if (font == null)
            {
                return;
            }

            if (headerText == null)
            {
                headerText = CreateRuntimeText(root, "HeaderText", font, 12f, new Vector2(0f, 96f), new Vector2(160f, 18f));
            }

            if (phaseText == null)
            {
                phaseText = CreateRuntimeText(root, "PhaseText", font, 18f, new Vector2(0f, 76f), new Vector2(148f, 22f));
            }

            if (bossCueText == null)
            {
                bossCueText = CreateRuntimeText(root, "BossCueText", font, 15f, new Vector2(0f, 56f), new Vector2(148f, 20f));
            }

            if (energyText == null)
            {
                energyText = CreateRuntimeText(root, "EnergyText", font, 14f, new Vector2(0f, 36f), new Vector2(148f, 18f));
            }

            if (countdownText == null)
            {
                countdownText = CreateRuntimeText(root, "CountdownText", font, 18f, new Vector2(0f, -42f), new Vector2(148f, 22f));
            }

            if (projectileCountdownText == null)
            {
                projectileCountdownText = CreateRuntimeText(root, "ProjectileCountdownText", font, 16f, new Vector2(0f, -66f), new Vector2(148f, 20f));
            }

            if (summonIntentText == null)
            {
                summonIntentText = CreateRuntimeText(root, "SummonIntentText", font, 15f, new Vector2(0f, -88f), new Vector2(148f, 20f));
            }

            if (volleyPatternText == null)
            {
                volleyPatternText = CreateRuntimeText(root, "VolleyPatternText", font, 15f, new Vector2(0f, -108f), new Vector2(148f, 20f));
            }

            layoutDirty = true;
        }

        private void ApplyPanelLayout()
        {
            RectTransform root = panelRoot != null ? panelRoot : transform as RectTransform;
            if (root == null)
            {
                return;
            }

            panelRoot = root;
            float width = root.rect.width > 1f ? root.rect.width : 180f;
            if (!layoutDirty && Mathf.Abs(lastLayoutWidth - width) < 0.5f)
            {
                return;
            }

            layoutDirty = false;
            lastLayoutWidth = width;
            if (panelBackground == null)
            {
                panelBackground = root.GetComponent<Image>();
                if (panelBackground == null)
                {
                    panelBackground = root.gameObject.AddComponent<Image>();
                }
            }

            panelBackground.sprite = RuntimeUISpriteUtility.GetPanelSprite();
            panelBackground.type = Image.Type.Sliced;
            panelBackground.color = new Color(0.12f, 0.05f, 0.08f, 0.72f);
            panelBackground.raycastTarget = false;

            bool compact = width <= 160f;
            float cardSize = compact ? 46f : 56f;
            float leftColumnX = 10f;
            float textColumnX = compact ? 64f : 76f;
            float textWidth = Mathf.Max(64f, width - textColumnX - 10f);

            if (headerText != null)
            {
                headerText.text = "ENEMY PRESSURE";
                ApplyTextLayout(headerText, new Vector2(width - 20f, 16f), new Vector2(10f, -8f), compact ? 10.5f : 11.5f, TextAlignmentOptions.Left);
                headerText.color = new Color(1f, 0.76f, 0.42f, 0.96f);
                headerText.fontStyle = FontStyles.Bold;
            }

            if (nextCardImage != null)
            {
                RectTransform cardRect = nextCardImage.rectTransform;
                cardRect.anchorMin = new Vector2(0f, 1f);
                cardRect.anchorMax = new Vector2(0f, 1f);
                cardRect.pivot = new Vector2(0f, 1f);
                cardRect.sizeDelta = new Vector2(cardSize, cardSize);
                cardRect.anchoredPosition = new Vector2(leftColumnX, -28f);
            }

            ApplyInfoLabel(phaseText, textColumnX, width, compact ? 13f : 14f, compact ? 28f : 30f);
            ApplyInfoLabel(bossCueText, textColumnX, width, compact ? 12.5f : 13.5f, compact ? 46f : 50f);
            ApplyInfoLabel(summonIntentText, textColumnX, width, compact ? 12f : 13f, compact ? 64f : 70f);
            ApplyInfoLabel(volleyPatternText, textColumnX, width, compact ? 12f : 13f, compact ? 82f : 90f);
            ApplyInfoLabel(energyText, textColumnX, width, compact ? 11.75f : 12.5f, compact ? 100f : 110f);
            ApplyInfoLabel(countdownText, textColumnX, width, compact ? 11.75f : 12.5f, compact ? 118f : 130f);
            ApplyInfoLabel(projectileCountdownText, textColumnX, width, compact ? 11.75f : 12.5f, compact ? 136f : 150f);

            if (phaseText != null)
            {
                phaseText.fontStyle = FontStyles.Bold;
            }

            if (bossCueText != null)
            {
                bossCueText.fontStyle = FontStyles.Bold;
            }

            if (summonIntentText != null)
            {
                summonIntentText.fontStyle = FontStyles.Bold;
            }

            if (volleyPatternText != null)
            {
                volleyPatternText.fontStyle = FontStyles.Bold;
            }

            if (energyText != null)
            {
                energyText.fontStyle = FontStyles.Bold;
            }

            if (countdownText != null)
            {
                countdownText.fontStyle = FontStyles.Bold;
            }

            if (projectileCountdownText != null)
            {
                projectileCountdownText.fontStyle = FontStyles.Bold;
            }
        }

        private static void ApplyInfoLabel(TMP_Text text, float x, float width, float fontSize, float y)
        {
            if (text == null)
            {
                return;
            }

            ApplyTextLayout(text, new Vector2(Mathf.Max(64f, width - x - 10f), 16f), new Vector2(x, -y), fontSize, TextAlignmentOptions.Left);
        }

        private static void ApplyTextLayout(TMP_Text text, Vector2 size, Vector2 anchoredPosition, float fontSize, TextAlignmentOptions alignment)
        {
            if (text == null)
            {
                return;
            }

            RectTransform rect = text.rectTransform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Ellipsis;
        }

        private static TMP_Text CreateRuntimeText(RectTransform parent, string name, TMP_FontAsset font, float fontSize, Vector2 anchoredPosition, Vector2 size)
        {
            Transform existing = parent.Find(name);
            GameObject textObject = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(parent, false);

            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
            text.font = font;
            text.fontSize = fontSize;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            return text;
        }
    }
}
