using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class UICuePresenter : MonoBehaviour
    {
        [SerializeField] private UICueBundleCatalog catalog;
        [SerializeField] private UIMotionPresenter motionPresenter;
        [SerializeField] private Text cueIdText;
        [SerializeField] private Text detailText;
        [SerializeField] private string defaultCueId;
        [SerializeField] private bool playOnEnable;
        [SerializeField] private UnityEvent<string> cuePlayed = new UnityEvent<string>();

        private void OnEnable()
        {
            if (playOnEnable)
            {
                PlayDefault();
            }
        }

        public void PlayDefault()
        {
            PlayCue(defaultCueId);
        }

        public void PlayCue(string cueId)
        {
            if (catalog == null || string.IsNullOrWhiteSpace(cueId) || !catalog.TryGetCue(cueId, out UICueBundleCatalog.CueBundle cue))
            {
                return;
            }

            SetText(cueIdText, cue.Id);
            SetText(detailText, BuildDetail(cue));

            if (motionPresenter != null && !string.IsNullOrWhiteSpace(cue.UiMotionId))
            {
                motionPresenter.PlayMotion(cue.UiMotionId);
            }

            cuePlayed.Invoke(cue.Id);
        }

        private static string BuildDetail(UICueBundleCatalog.CueBundle cue)
        {
            return $"Event {cue.EventId} | Motion {cue.UiMotionId} | SFX {cue.SfxId} | Cleanup {cue.CleanupPolicy}";
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
