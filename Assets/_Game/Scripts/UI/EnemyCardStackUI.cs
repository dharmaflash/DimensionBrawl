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

        private bool isSubscribed;
        private float summonCountdown;
        private float projectileCountdown;
        private Color currentPhaseColor = Color.white;

        private void Awake()
        {
            EnsureSupplementalLabels();
            ApplyPanelLayout();
        }

        private void OnEnable()
        {
            if (enemyAI == null)
            {
                enemyAI = FindFirstObjectByType<EnemyAI>();
            }

            EnsureSupplementalLabels();
            ApplyPanelLayout();
            TrySubscribe();
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
                isSubscribed = false;
            }
        }

        private void Start()
        {
            if (enemyAI == null)
            {
                enemyAI = FindFirstObjectByType<EnemyAI>();
            }

            EnsureSupplementalLabels();
            TrySubscribe();

            if (enemyAI != null)
            {
                UpdateNextCard(enemyAI.NextSummon);
                UpdateCountdown(enemyAI.RemainingSummonCountdown);
                UpdateProjectileCountdown(enemyAI.RemainingProjectileCountdown);
                UpdatePhaseText(enemyAI.CurrentPhaseName);
                UpdateBossCueText();
                UpdateEnergyText();
                UpdateSummonIntentText(enemyAI.CurrentSummonIntentName);
                UpdateVolleyPatternText(enemyAI.CurrentVolleyPatternName);
            }

            ApplyPanelLayout();
        }

        private void Update()
        {
            if (enemyAI == null)
            {
                enemyAI = FindFirstObjectByType<EnemyAI>();
            }

            if (!isSubscribed)
            {
                TrySubscribe();
            }

            UpdateBossCueText();
            UpdateEnergyText();
            AnimateIntentPanel();
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
            if (countdownText != null)
            {
                bool isBanking = enemyAI != null && enemyAI.IsBankingForNextSummon && timeRemaining > 0.05f;
                countdownText.text = isBanking
                    ? $"BANK  {timeRemaining:0.0}s"
                    : $"SUMMON  {timeRemaining:0.0}s";
            }
        }

        private void UpdateProjectileCountdown(float timeRemaining)
        {
            projectileCountdown = timeRemaining;
            if (projectileCountdownText != null)
            {
                projectileCountdownText.text = $"HEX  {ResolveProjectileCountdownLabel(timeRemaining)}";
            }
        }

        private void UpdatePhaseText(string phaseName)
        {
            if (phaseText != null)
            {
                phaseText.text = $"PHASE  {phaseName}";
                phaseText.color = ResolvePhaseColor(phaseName);
            }

            currentPhaseColor = ResolvePhaseColor(phaseName);
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
                bossCueText.text = "PRESSURE  reading";
                bossCueText.color = Color.Lerp(currentPhaseColor, Color.white, 0.2f);
                return;
            }

            bossCueText.text = $"PRESSURE  {enemyAI.CurrentBossCueShort}";
            bossCueText.color = Color.Lerp(enemyAI.CurrentSignalColor, Color.white, 0.2f);
        }

        private void UpdateEnergyText()
        {
            if (energyText == null)
            {
                return;
            }

            if (enemyAI == null)
            {
                energyText.text = "ENERGY  syncing";
                energyText.color = Color.Lerp(currentPhaseColor, Color.white, 0.2f);
                return;
            }

            if (enemyAI.IsBankingForNextSummon)
            {
                energyText.text = $"ENERGY  {Mathf.FloorToInt(enemyAI.CurrentEnergy)}/{Mathf.CeilToInt(enemyAI.NextSummonCost)}";
                energyText.color = Color.Lerp(new Color(1f, 0.68f, 0.42f, 1f), Color.white, 0.15f);
                return;
            }

            energyText.text = $"ENERGY  {Mathf.FloorToInt(enemyAI.CurrentEnergy)}/{Mathf.FloorToInt(enemyAI.MaxEnergy)}";
            energyText.color = Color.Lerp(currentPhaseColor, Color.white, 0.15f);
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

        private static string ResolveProjectileCountdownLabel(float timeRemaining)
        {
            if (timeRemaining <= 0.35f)
            {
                return "NOW";
            }

            if (timeRemaining <= 0.9f)
            {
                return "IMMINENT";
            }

            if (timeRemaining <= 1.8f)
            {
                return "SOON";
            }

            return "CHARGING";
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
            RectTransform root = transform as RectTransform;
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
        }

        private void ApplyPanelLayout()
        {
            RectTransform root = transform as RectTransform;
            if (root == null)
            {
                return;
            }

            Image background = root.GetComponent<Image>();
            if (background == null)
            {
                background = root.gameObject.AddComponent<Image>();
            }

            background.sprite = RuntimeUISpriteUtility.GetPanelSprite();
            background.type = Image.Type.Sliced;
            background.color = new Color(0.12f, 0.05f, 0.08f, 0.72f);
            background.raycastTarget = false;

            float width = root.rect.width > 1f ? root.rect.width : 180f;
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
