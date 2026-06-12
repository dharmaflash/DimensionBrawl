using UnityEngine;

namespace DimensionBrawl.Presentation
{
    public sealed class EnemyAttackTelegraphPresenter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject telegraphObject;
        [SerializeField] private Transform telegraphTransform;
        [SerializeField] private Renderer telegraphRenderer;
        [SerializeField] private Transform poseRoot;

        [Header("Telegraph Shape")]
        [SerializeField] private Vector3 windupStartScale = new Vector3(0.35f, 0.02f, 0.65f);
        [SerializeField] private Vector3 windupEndScale = new Vector3(1.05f, 0.02f, 1.55f);
        [SerializeField] private Vector3 activeScale = new Vector3(1.25f, 0.025f, 1.8f);
        [SerializeField] private Vector3 windupPoseOffset = new Vector3(0f, 0f, -0.08f);
        [SerializeField] private Vector3 activePoseOffset = new Vector3(0f, 0f, 0.12f);

        [Header("Telegraph Color")]
        [SerializeField] private Color windupStartColor = new Color(1f, 0.45f, 0.08f, 1f);
        [SerializeField] private Color windupEndColor = new Color(1f, 0.08f, 0.02f, 1f);
        [SerializeField] private Color activeColor = Color.white;

        private readonly int baseColorId = Shader.PropertyToID("_BaseColor");
        private readonly int colorId = Shader.PropertyToID("_Color");

        private MaterialPropertyBlock propertyBlock;
        private Vector3 authoredTelegraphScale;
        private Vector3 authoredPosePosition;

        public bool IsVisible => telegraphObject != null && telegraphObject.activeSelf;
        public Renderer TelegraphRenderer => telegraphRenderer;
        public Transform PoseRoot => poseRoot;

        public void ConfigureStyle(
            Vector3 newWindupStartScale,
            Vector3 newWindupEndScale,
            Vector3 newActiveScale,
            Vector3 newWindupPoseOffset,
            Vector3 newActivePoseOffset,
            Color newWindupStartColor,
            Color newWindupEndColor,
            Color newActiveColor)
        {
            windupStartScale = newWindupStartScale;
            windupEndScale = newWindupEndScale;
            activeScale = newActiveScale;
            windupPoseOffset = newWindupPoseOffset;
            activePoseOffset = newActivePoseOffset;
            windupStartColor = newWindupStartColor;
            windupEndColor = newWindupEndColor;
            activeColor = newActiveColor;
        }

        private void Awake()
        {
            if (telegraphObject == null)
            {
                telegraphObject = gameObject;
            }

            if (telegraphTransform == null && telegraphObject != null)
            {
                telegraphTransform = telegraphObject.transform;
            }

            if (telegraphRenderer == null && telegraphObject != null)
            {
                telegraphRenderer = telegraphObject.GetComponentInChildren<Renderer>(includeInactive: true);
            }

            propertyBlock = new MaterialPropertyBlock();
            authoredTelegraphScale = telegraphTransform != null ? telegraphTransform.localScale : Vector3.one;
            authoredPosePosition = poseRoot != null ? poseRoot.localPosition : Vector3.zero;
            Hide();
        }

        private void OnDisable()
        {
            Hide();
        }

        public void Hide()
        {
            if (telegraphObject != null)
            {
                telegraphObject.SetActive(false);
            }

            if (telegraphTransform != null)
            {
                telegraphTransform.localScale = authoredTelegraphScale;
            }

            if (poseRoot != null)
            {
                poseRoot.localPosition = authoredPosePosition;
            }
        }

        public void ShowWindup(float normalizedProgress)
        {
            float progress = Mathf.Clamp01(normalizedProgress);
            float easedProgress = Mathf.SmoothStep(0f, 1f, progress);
            SetTelegraphVisible();

            if (telegraphTransform != null)
            {
                telegraphTransform.localScale = Vector3.Lerp(windupStartScale, windupEndScale, easedProgress);
            }

            if (poseRoot != null)
            {
                poseRoot.localPosition = authoredPosePosition + windupPoseOffset * easedProgress;
            }

            ApplyTelegraphColor(Color.Lerp(windupStartColor, windupEndColor, easedProgress));
        }

        public void ShowActive(float normalizedProgress)
        {
            float progress = Mathf.Clamp01(normalizedProgress);
            float returnProgress = Mathf.SmoothStep(0f, 1f, progress);
            SetTelegraphVisible();

            if (telegraphTransform != null)
            {
                telegraphTransform.localScale = Vector3.Lerp(activeScale, windupEndScale, returnProgress);
            }

            if (poseRoot != null)
            {
                poseRoot.localPosition = authoredPosePosition + Vector3.Lerp(activePoseOffset, Vector3.zero, returnProgress);
            }

            ApplyTelegraphColor(Color.Lerp(activeColor, windupEndColor, returnProgress));
        }

        private void SetTelegraphVisible()
        {
            if (telegraphObject != null && !telegraphObject.activeSelf)
            {
                telegraphObject.SetActive(true);
            }
        }

        private void ApplyTelegraphColor(Color color)
        {
            if (telegraphRenderer == null)
            {
                return;
            }

            propertyBlock ??= new MaterialPropertyBlock();
            telegraphRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(baseColorId, color);
            propertyBlock.SetColor(colorId, color);
            telegraphRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}
