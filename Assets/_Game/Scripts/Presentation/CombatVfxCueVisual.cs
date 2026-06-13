using UnityEngine;

namespace DimensionBrawl.Presentation
{
    [DisallowMultipleComponent]
    public sealed class CombatVfxCueVisual : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        [SerializeField] private Renderer[] renderers = System.Array.Empty<Renderer>();
        [SerializeField] private Color startColor = Color.white;
        [SerializeField] private Color endColor = new Color(1f, 1f, 1f, 0f);
        [SerializeField] private Vector3 startScale = Vector3.one;
        [SerializeField] private Vector3 endScale = new Vector3(1.25f, 1.25f, 1.25f);
        [SerializeField] private float lifetimeSeconds = 0.35f;
        [SerializeField] private float spinDegreesPerSecond;
        [SerializeField] private float verticalLift;

        private MaterialPropertyBlock propertyBlock;
        private Vector3 authoredLocalScale;
        private Vector3 authoredLocalPosition;
        private float elapsedSeconds;
        private bool isPlaying;

        private void Awake()
        {
            propertyBlock ??= new MaterialPropertyBlock();
            CaptureAuthoredTransform();
            if (renderers == null || renderers.Length == 0)
            {
                renderers = GetComponentsInChildren<Renderer>(includeInactive: true);
            }
        }

        private void OnEnable()
        {
            Restart();
        }

        private void Update()
        {
            if (!isPlaying)
            {
                return;
            }

            elapsedSeconds += Time.deltaTime;
            float normalizedTime = lifetimeSeconds > 0f ? Mathf.Clamp01(elapsedSeconds / lifetimeSeconds) : 1f;
            ApplyVisualState(normalizedTime);

            if (Mathf.Abs(spinDegreesPerSecond) > 0.01f)
            {
                transform.Rotate(Vector3.up, spinDegreesPerSecond * Time.deltaTime, Space.Self);
            }

            if (normalizedTime >= 1f)
            {
                isPlaying = false;
            }
        }

        public void Restart()
        {
            CaptureAuthoredTransform();
            elapsedSeconds = 0f;
            isPlaying = true;
            ApplyVisualState(0f);
        }

        public void StopNow()
        {
            isPlaying = false;
            ApplyVisualState(1f);
        }

        private void CaptureAuthoredTransform()
        {
            authoredLocalScale = transform.localScale;
            authoredLocalPosition = transform.localPosition;
        }

        private void ApplyVisualState(float normalizedTime)
        {
            propertyBlock ??= new MaterialPropertyBlock();
            float eased = Mathf.SmoothStep(0f, 1f, normalizedTime);
            transform.localScale = Vector3.Scale(authoredLocalScale, Vector3.Lerp(startScale, endScale, eased));
            transform.localPosition = authoredLocalPosition + Vector3.up * verticalLift * eased;

            Color color = Color.Lerp(startColor, endColor, eased);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                renderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor(BaseColorId, color);
                propertyBlock.SetColor(ColorId, color);
                renderer.SetPropertyBlock(propertyBlock);
            }
        }
    }
}
