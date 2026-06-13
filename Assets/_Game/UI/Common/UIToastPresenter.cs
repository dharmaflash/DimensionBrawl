using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class UIToastPresenter : MonoBehaviour
    {
        [SerializeField] private UIToastCatalog toastCatalog;
        [SerializeField] private UITextCatalog textCatalog;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Text messageText;
        [SerializeField] private Image accentImage;
        [SerializeField] private UIIconPresenter iconPresenter;
        [SerializeField, Min(0f)] private float defaultDurationSeconds = 1.4f;

        private Coroutine hideRoutine;

        private void Reset()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        private void Awake()
        {
            HideImmediate();
        }

        public void ShowMessage(string message)
        {
            ShowResolvedMessage(message, defaultDurationSeconds);
        }

        public void ShowToast(string toastId)
        {
            if (toastCatalog == null || !toastCatalog.TryGetToast(toastId, out UIToastCatalog.ToastEntry toast))
            {
                ShowResolvedMessage(toastId, defaultDurationSeconds);
                return;
            }

            string message = toast.MessageTextKey;
            if (textCatalog != null && textCatalog.TryGetText(toast.MessageTextKey, out string resolvedText))
            {
                message = resolvedText;
            }

            if (accentImage != null)
            {
                accentImage.color = toast.AccentColor;
            }

            if (iconPresenter != null)
            {
                iconPresenter.SetIcon(toast.IconKey);
            }

            ShowResolvedMessage(message, toast.DurationSeconds > 0f ? toast.DurationSeconds : defaultDurationSeconds);
        }

        private void ShowResolvedMessage(string message, float durationSeconds)
        {
            if (messageText != null)
            {
                messageText.text = message;
            }

            SetVisible(true);

            if (hideRoutine != null)
            {
                StopCoroutine(hideRoutine);
            }

            hideRoutine = StartCoroutine(HideAfterDelay(durationSeconds));
        }

        public void HideImmediate()
        {
            SetVisible(false);
        }

        private IEnumerator HideAfterDelay(float delaySeconds)
        {
            yield return new WaitForSecondsRealtime(delaySeconds);
            SetVisible(false);
            hideRoutine = null;
        }

        private void SetVisible(bool visible)
        {
            if (canvasGroup == null)
            {
                gameObject.SetActive(visible);
                return;
            }

            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
    }
}
