using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class LobbyScreenPresenter : MonoBehaviour
    {
        [SerializeField] private Text guideNameText;
        [SerializeField] private Text guideLineText;
        [SerializeField] private Text statusText;
        [SerializeField] private Text primaryCtaText;
        [SerializeField] private Button primaryCtaButton;
        [SerializeField] private UISceneFlowRouter router;
        [SerializeField] private UIRouteId primaryRoute = UIRouteId.StageSelect;
        [SerializeField] private LobbyGuideFeedbackCatalog guideFeedbackCatalog;
        [SerializeField] private UITextCatalog textCatalog;
        [SerializeField] private LobbyGuideCondition guideCondition = LobbyGuideCondition.Default;
        [SerializeField] private string guideNameTextKey;
        [SerializeField] private string primaryCtaTextKey;
        [SerializeField] private UnityEvent primaryCtaRequested = new UnityEvent();

        private void OnEnable()
        {
            ApplyGuideCondition(guideCondition);

            if (primaryCtaButton != null)
            {
                primaryCtaButton.onClick.AddListener(HandlePrimaryCtaClicked);
            }
        }

        private void OnDisable()
        {
            if (primaryCtaButton != null)
            {
                primaryCtaButton.onClick.RemoveListener(HandlePrimaryCtaClicked);
            }
        }

        public void SetGuideCondition(LobbyGuideCondition condition)
        {
            guideCondition = condition;
            ApplyGuideCondition(guideCondition);
        }

        public void HandlePrimaryCtaClicked()
        {
            primaryCtaRequested.Invoke();

            if (router != null)
            {
                router.RequestRoute(primaryRoute);
            }
        }

        private void ApplyGuideCondition(LobbyGuideCondition condition)
        {
            SetCatalogText(guideNameText, guideNameTextKey);
            SetCatalogText(primaryCtaText, primaryCtaTextKey);
            SetText(statusText, string.Empty);

            if (TryGetGuideLineKey(condition, out string lineKey))
            {
                SetCatalogText(guideLineText, lineKey);
                return;
            }

            SetText(guideLineText, string.Empty);
        }

        private bool TryGetGuideLineKey(LobbyGuideCondition condition, out string lineKey)
        {
            if (TryGetGuideLineKeyExact(condition, out lineKey))
            {
                return true;
            }

            return condition != LobbyGuideCondition.Default &&
                TryGetGuideLineKeyExact(LobbyGuideCondition.Default, out lineKey);
        }

        private bool TryGetGuideLineKeyExact(LobbyGuideCondition condition, out string lineKey)
        {
            if (guideFeedbackCatalog != null &&
                guideFeedbackCatalog.TryGetFirst(condition, out LobbyGuideFeedbackCatalog.FeedbackEntry entry) &&
                !string.IsNullOrWhiteSpace(entry.LineKey))
            {
                lineKey = entry.LineKey;
                return true;
            }

            lineKey = string.Empty;
            return false;
        }

        private void SetCatalogText(Text target, string key)
        {
            if (target == null)
            {
                return;
            }

            if (textCatalog != null &&
                !string.IsNullOrWhiteSpace(key) &&
                textCatalog.TryGetText(key, out string value))
            {
                target.text = value;
                return;
            }

            target.text = string.Empty;
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
