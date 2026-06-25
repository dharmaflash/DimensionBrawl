using UnityEngine;
using UnityEngine.Playables;

namespace DimensionBrawl.Presentation
{
    [DisallowMultipleComponent]
    public sealed class IntroGatePodFirstPersonRendererMask : MonoBehaviour
    {
        [SerializeField] private PlayableDirector director;
        [SerializeField] private Renderer[] hiddenRenderers = System.Array.Empty<Renderer>();
        [SerializeField, Min(0f)] private float hideStartSeconds;
        [SerializeField, Min(0f)] private float hideEndSeconds;

        public int HiddenRendererCount => hiddenRenderers != null ? hiddenRenderers.Length : 0;

        public void Configure(
            PlayableDirector newDirector,
            Renderer[] newHiddenRenderers,
            float newHideStartSeconds,
            float newHideEndSeconds)
        {
            director = newDirector;
            hiddenRenderers = newHiddenRenderers ?? System.Array.Empty<Renderer>();
            hideStartSeconds = Mathf.Max(0f, newHideStartSeconds);
            hideEndSeconds = Mathf.Max(hideStartSeconds, newHideEndSeconds);
            ApplyForReview(0f);
        }

        public void ApplyForReview(float elapsedSeconds)
        {
            bool hide = elapsedSeconds >= hideStartSeconds && elapsedSeconds <= hideEndSeconds;
            SetRendererVisibility(!hide);
        }

        private void LateUpdate()
        {
            if (director == null)
            {
                return;
            }

            ApplyForReview((float)director.time);
        }

        private void OnDisable()
        {
            SetRendererVisibility(true);
        }

        private void SetRendererVisibility(bool visible)
        {
            Renderer[] renderers = hiddenRenderers ?? System.Array.Empty<Renderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    renderers[i].enabled = visible;
                }
            }
        }
    }
}
