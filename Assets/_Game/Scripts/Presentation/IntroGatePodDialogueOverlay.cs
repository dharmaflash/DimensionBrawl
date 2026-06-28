using TMPro;
using UnityEngine;

namespace DimensionBrawl.Presentation
{
    [DisallowMultipleComponent]
    public sealed class IntroGatePodDialogueOverlay : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] private float alpha;
        [SerializeField] private string speakerName = string.Empty;
        [SerializeField, TextArea(2, 4)] private string dialogueText = string.Empty;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TMP_Text speakerText;
        [SerializeField] private TMP_Text lineText;

        public bool HasBindings => canvasGroup != null && speakerText != null && lineText != null;

        public void Configure(CanvasGroup newCanvasGroup, TMP_Text newSpeakerText, TMP_Text newLineText)
        {
            canvasGroup = newCanvasGroup;
            speakerText = newSpeakerText;
            lineText = newLineText;
            ApplyState();
        }

        public void Apply(string newSpeakerName, string newDialogueText, float newAlpha)
        {
            speakerName = newSpeakerName ?? string.Empty;
            dialogueText = newDialogueText ?? string.Empty;
            alpha = string.IsNullOrWhiteSpace(speakerName) && string.IsNullOrWhiteSpace(dialogueText)
                ? 0f
                : Mathf.Clamp01(newAlpha);
            ApplyState();
        }

        public void Clear()
        {
            alpha = 0f;
            ApplyState();
        }

        private void Awake()
        {
            ApplyState();
        }

        private void OnValidate()
        {
            ApplyState();
        }

        private void ApplyState()
        {
            float resolvedAlpha = Mathf.Clamp01(alpha);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = resolvedAlpha;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }

            if (speakerText != null)
            {
                bool hasSpeaker = !string.IsNullOrWhiteSpace(speakerName);
                speakerText.text = speakerName;
                speakerText.gameObject.SetActive(hasSpeaker);
            }

            if (lineText != null)
            {
                lineText.text = dialogueText;
                lineText.gameObject.SetActive(!string.IsNullOrWhiteSpace(dialogueText));
            }
        }
    }
}
