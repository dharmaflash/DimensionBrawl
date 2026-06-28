using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class ChapterMapPrototypeStageNode : MonoBehaviour
    {
        [SerializeField] private string regionId;
        [SerializeField] private string stageCode;
        [SerializeField] private string stageTitle;
        [SerializeField] private string stageSubtitle;
        [SerializeField] private string objectiveText;
        [SerializeField] private string rewardText;
        [SerializeField] private string energyCostText = "0";
        [SerializeField] private bool locked;
        [SerializeField] private bool cleared;
        [SerializeField] private bool keyStage;
        [SerializeField] private Button button;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image bodyImage;
        [SerializeField] private Image ringImage;
        [SerializeField] private Image selectedGlowImage;
        [SerializeField] private Image clearedBadgeImage;
        [SerializeField] private TMP_Text codeLabel;
        [SerializeField] private TMP_Text titleLabel;
        [SerializeField] private TMP_Text statusLabel;

        private static readonly Color ActiveBodyColor = new Color(0.08f, 0.16f, 0.28f, 0.96f);
        private static readonly Color ActiveRingColor = new Color(0.55f, 0.86f, 1f, 1f);
        private static readonly Color KeyRingColor = new Color(1f, 0.65f, 0.22f, 1f);
        private static readonly Color LockedBodyColor = new Color(0.16f, 0.16f, 0.18f, 0.78f);
        private static readonly Color LockedRingColor = new Color(0.58f, 0.58f, 0.62f, 0.92f);
        private static readonly Color ClearedBadgeColor = new Color(0.35f, 0.92f, 1f, 0.95f);

        public event Action<ChapterMapPrototypeStageNode> Clicked;

        public string RegionId => regionId;
        public string StageCode => stageCode;
        public string StageTitle => stageTitle;
        public string StageSubtitle => stageSubtitle;
        public string ObjectiveText => objectiveText;
        public string RewardText => rewardText;
        public string EnergyCostText => energyCostText;
        public bool IsLocked => locked;
        public bool IsCleared => cleared;
        public bool IsKeyStage => keyStage;
        public RectTransform RectTransform => transform as RectTransform;

        private void Reset()
        {
            ResolveReferences();
        }

        private void Awake()
        {
            ResolveReferences();
            ApplyVisualState(false);
        }

        private void OnEnable()
        {
            if (button != null)
            {
                button.onClick.AddListener(HandleClicked);
            }
        }

        private void OnDisable()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(HandleClicked);
            }
        }

        public void SetRegionActive(bool active)
        {
            if (canvasGroup != null)
            {
                canvasGroup.interactable = active;
                canvasGroup.blocksRaycasts = active;
            }

            if (button != null)
            {
                button.interactable = active;
            }
        }

        public void SetSelected(bool selected)
        {
            ApplyVisualState(selected);
        }

        private void HandleClicked()
        {
            Clicked?.Invoke(this);
        }

        private void ResolveReferences()
        {
            if (button == null)
            {
                button = GetComponent<Button>();
            }

            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }
        }

        private void ApplyVisualState(bool selected)
        {
            SetText(codeLabel, stageCode);
            SetText(titleLabel, stageTitle);

            if (bodyImage != null)
            {
                bodyImage.color = locked ? LockedBodyColor : ActiveBodyColor;
            }

            if (ringImage != null)
            {
                ringImage.color = locked ? LockedRingColor : keyStage ? KeyRingColor : ActiveRingColor;
            }

            if (selectedGlowImage != null)
            {
                selectedGlowImage.gameObject.SetActive(selected);
            }

            if (clearedBadgeImage != null)
            {
                clearedBadgeImage.gameObject.SetActive(cleared);
                clearedBadgeImage.color = ClearedBadgeColor;
            }

            if (statusLabel != null)
            {
                statusLabel.text = locked ? "LOCK" : cleared ? "CLEAR" : keyStage ? "KEY" : "READY";
                statusLabel.color = locked ? LockedRingColor : keyStage ? KeyRingColor : ActiveRingColor;
            }
        }

        private static void SetText(TMP_Text target, string value)
        {
            if (target != null)
            {
                target.text = value;
            }
        }
    }
}
