using System;
using UnityEngine;
using UnityEngine.UI;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class CombatHudPresenter : MonoBehaviour
    {
        [Serializable]
        public sealed class ActionSlotBinding
        {
            [SerializeField] private CombatHudActionId actionId;
            [SerializeField] private Text labelText;
            [SerializeField] private Text cooldownText;
            [SerializeField] private Image cooldownFill;
            [SerializeField] private CanvasGroup canvasGroup;

            public CombatHudActionId ActionId => actionId;

            public void SetCooldown(float normalizedRemaining, string label)
            {
                if (labelText != null && !string.IsNullOrWhiteSpace(label))
                {
                    labelText.text = label;
                }

                float clamped = Mathf.Clamp01(normalizedRemaining);
                if (cooldownFill != null)
                {
                    cooldownFill.fillAmount = clamped;
                }

                if (cooldownText != null)
                {
                    cooldownText.text = clamped > 0f ? $"{Mathf.CeilToInt(clamped * 10f) / 10f:0.0}s" : string.Empty;
                }

                if (canvasGroup != null)
                {
                    canvasGroup.alpha = clamped > 0f ? 0.65f : 1f;
                }
            }
        }

        [Serializable]
        public sealed class SummonSlotBinding
        {
            [SerializeField] private CombatHudActionId actionId;
            [SerializeField] private Text labelText;
            [SerializeField] private Text stateText;
            [SerializeField] private Image cooldownFill;
            [SerializeField] private CanvasGroup canvasGroup;

            public CombatHudActionId ActionId => actionId;

            public void SetState(string label, string state, bool enabled)
            {
                if (labelText != null)
                {
                    labelText.text = label;
                }

                if (stateText != null)
                {
                    stateText.text = state;
                }

                if (cooldownFill != null)
                {
                    cooldownFill.fillAmount = enabled ? 0f : 1f;
                }

                if (canvasGroup != null)
                {
                    canvasGroup.alpha = enabled ? 1f : 0.55f;
                    canvasGroup.interactable = enabled;
                    canvasGroup.blocksRaycasts = enabled;
                }
            }
        }

        [SerializeField] private Text objectiveText;
        [SerializeField] private Text timerText;
        [SerializeField] private Text healthText;
        [SerializeField] private Text resourceText;
        [SerializeField] private Text inputModeText;
        [SerializeField] private Text actionFeedbackText;
        [SerializeField] private Image healthFill;
        [SerializeField] private Image resourceFill;
        [SerializeField] private Text bossHealthText;
        [SerializeField] private Image bossHealthFill;
        [SerializeField] private RectTransform aimReticleRoot;
        [SerializeField] private Image[] aimReticleSegments = Array.Empty<Image>();
        [SerializeField] private Color aimReticleColor = new Color(0.82f, 0.96f, 1f, 0.88f);
        [SerializeField] private Color aimReticleActiveColor = new Color(0.42f, 0.95f, 1f, 0.96f);
        [SerializeField] private CombatHudActionCatalog actionCatalog;
        [SerializeField] private ActionSlotBinding[] actionSlots = Array.Empty<ActionSlotBinding>();
        [SerializeField] private SummonSlotBinding[] summonSlots = Array.Empty<SummonSlotBinding>();

        public float BossHealthFillAmount => bossHealthFill != null ? bossHealthFill.fillAmount : 0f;
        public bool AimReticleVisible => aimReticleRoot != null && aimReticleRoot.gameObject.activeInHierarchy;

        private void Awake()
        {
            ResolveOptionalRuntimeReferences();
            EnsureAimReticle();
        }

        public void SetObjective(string objective)
        {
            SetText(objectiveText, objective);
        }

        public void SetTimer(float secondsRemaining)
        {
            float clamped = Mathf.Max(0f, secondsRemaining);
            int minutes = Mathf.FloorToInt(clamped / 60f);
            int seconds = Mathf.FloorToInt(clamped % 60f);
            SetText(timerText, $"{minutes:00}:{seconds:00}");
        }

        public void SetHealth(float current, float max)
        {
            float ratio = max > 0f ? Mathf.Clamp01(current / max) : 0f;
            if (healthFill != null)
            {
                healthFill.fillAmount = ratio;
            }

            SetText(healthText, $"{Mathf.CeilToInt(Mathf.Max(0f, current))}/{Mathf.CeilToInt(Mathf.Max(0f, max))}");
        }

        public void SetBossHealth(float current, float max)
        {
            ResolveOptionalRuntimeReferences();
            float ratio = max > 0f ? Mathf.Clamp01(current / max) : 0f;
            if (bossHealthFill != null)
            {
                bossHealthFill.type = Image.Type.Filled;
                bossHealthFill.fillMethod = Image.FillMethod.Horizontal;
                bossHealthFill.fillOrigin = (int)Image.OriginHorizontal.Left;
                bossHealthFill.fillAmount = ratio;
            }

            SetText(bossHealthText, $"{Mathf.CeilToInt(Mathf.Max(0f, current))}/{Mathf.CeilToInt(Mathf.Max(0f, max))}");
        }

        public void SetAimReticleVisible(bool visible, bool active)
        {
            EnsureAimReticle();
            if (aimReticleRoot == null)
            {
                return;
            }

            aimReticleRoot.gameObject.SetActive(visible);
            Color color = active ? aimReticleActiveColor : aimReticleColor;
            for (int i = 0; i < aimReticleSegments.Length; i++)
            {
                if (aimReticleSegments[i] != null)
                {
                    aimReticleSegments[i].color = color;
                }
            }
        }

        public void SetResource(float current, float max)
        {
            float ratio = max > 0f ? Mathf.Clamp01(current / max) : 0f;
            if (resourceFill != null)
            {
                resourceFill.fillAmount = ratio;
            }

            SetText(resourceText, $"{Mathf.CeilToInt(Mathf.Max(0f, current))}/{Mathf.CeilToInt(Mathf.Max(0f, max))}");
        }

        public void SetInputMode(string label)
        {
            SetText(inputModeText, label);
        }

        public void SetSkillCooldown(CombatHudActionId actionId, float normalizedRemaining, string label)
        {
            ActionSlotBinding slot = FindActionSlot(actionId);
            slot?.SetCooldown(normalizedRemaining, label);
        }

        public void SetSummonSlotState(CombatHudActionId actionId, string label, string state, bool enabled)
        {
            SummonSlotBinding slot = FindSummonSlot(actionId);
            slot?.SetState(label, state, enabled);
        }

        public void SetActionFeedback(CombatHudActionId actionId)
        {
            if (actionId == CombatHudActionId.None)
            {
                SetActionFeedbackText(string.Empty);
                return;
            }

            if (actionCatalog != null && actionCatalog.TryGetAction(actionId, out CombatHudActionCatalog.ActionEntry action))
            {
                SetActionFeedbackText(action.DisplayName);
                return;
            }

            SetActionFeedbackText(string.Empty);
        }

        public void SetActionFeedbackText(string feedback)
        {
            SetText(actionFeedbackText, feedback);
        }

        private void ResolveOptionalRuntimeReferences()
        {
            if (bossHealthFill == null)
            {
                bossHealthFill = FindImage("BossHpFill");
            }

            if (bossHealthText == null)
            {
                bossHealthText = FindText("BossHpText");
            }
        }

        private void EnsureAimReticle()
        {
            if (aimReticleRoot == null)
            {
                Transform existing = FindDeepChild(transform, "CenterAimReticle");
                aimReticleRoot = existing != null ? existing.GetComponent<RectTransform>() : null;
            }

            if (aimReticleRoot == null)
            {
                GameObject root = new GameObject("CenterAimReticle", typeof(RectTransform));
                root.transform.SetParent(transform, worldPositionStays: false);
                aimReticleRoot = root.GetComponent<RectTransform>();
                aimReticleRoot.anchorMin = new Vector2(0.5f, 0.5f);
                aimReticleRoot.anchorMax = new Vector2(0.5f, 0.5f);
                aimReticleRoot.pivot = new Vector2(0.5f, 0.5f);
                aimReticleRoot.sizeDelta = new Vector2(96f, 96f);
                aimReticleRoot.anchoredPosition = Vector2.zero;
                aimReticleRoot.SetAsLastSibling();
            }

            if (aimReticleSegments == null || aimReticleSegments.Length < 4)
            {
                aimReticleSegments = new[]
                {
                    EnsureReticleSegment("Left", new Vector2(-23f, 0f), new Vector2(18f, 2f)),
                    EnsureReticleSegment("Right", new Vector2(23f, 0f), new Vector2(18f, 2f)),
                    EnsureReticleSegment("Top", new Vector2(0f, 23f), new Vector2(2f, 18f)),
                    EnsureReticleSegment("Bottom", new Vector2(0f, -23f), new Vector2(2f, 18f))
                };
            }
        }

        private Image EnsureReticleSegment(string name, Vector2 anchoredPosition, Vector2 size)
        {
            Transform child = aimReticleRoot.Find(name);
            if (child == null)
            {
                child = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).transform;
                child.SetParent(aimReticleRoot, worldPositionStays: false);
            }

            RectTransform rectTransform = child.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = size;

            Image image = child.GetComponent<Image>();
            image.raycastTarget = false;
            image.color = aimReticleColor;
            return image;
        }

        private ActionSlotBinding FindActionSlot(CombatHudActionId actionId)
        {
            for (int i = 0; i < actionSlots.Length; i++)
            {
                if (actionSlots[i] != null && actionSlots[i].ActionId == actionId)
                {
                    return actionSlots[i];
                }
            }

            return null;
        }

        private SummonSlotBinding FindSummonSlot(CombatHudActionId actionId)
        {
            for (int i = 0; i < summonSlots.Length; i++)
            {
                if (summonSlots[i] != null && summonSlots[i].ActionId == actionId)
                {
                    return summonSlots[i];
                }
            }

            return null;
        }

        private Image FindImage(string objectName)
        {
            Transform found = FindDeepChild(transform, objectName);
            return found != null ? found.GetComponent<Image>() : null;
        }

        private Text FindText(string objectName)
        {
            Transform found = FindDeepChild(transform, objectName);
            return found != null ? found.GetComponent<Text>() : null;
        }

        private static Transform FindDeepChild(Transform root, string objectName)
        {
            if (root == null)
            {
                return null;
            }

            if (root.name == objectName)
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindDeepChild(root.GetChild(i), objectName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static void SetText(Text target, string value)
        {
            if (target != null)
            {
                target.text = value;
            }
        }
    }
}
