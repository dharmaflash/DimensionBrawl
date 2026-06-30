using DimensionBrawl.Combat;
using UnityEngine;

namespace DimensionBrawl.Presentation
{
    [DisallowMultipleComponent]
    public sealed class SummonAttackBeamPresenter : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        [SerializeField] private SummonFrontlineProxy proxy;
        [SerializeField] private Transform beamRoot;
        [SerializeField] private Renderer[] beamRenderers = System.Array.Empty<Renderer>();
        [SerializeField] private Color tierOneColor = new Color(0.28f, 0.95f, 1f, 0.78f);
        [SerializeField] private Color tierTwoColor = new Color(0.62f, 0.86f, 1f, 0.86f);
        [SerializeField] private Color tierThreeColor = new Color(1f, 0.72f, 0.22f, 0.92f);
        [SerializeField, Min(0f)] private float tierScaleStep = 0.22f;
        [SerializeField, Min(0f)] private float pulseScale = 0.08f;
        [SerializeField, Min(0.01f)] private float pulseSpeed = 18f;

        private MaterialPropertyBlock propertyBlock;
        private Vector3 baseLocalScale = Vector3.one;
        private bool hasBaseScale;

        public SummonFrontlineProxy Proxy => proxy;
        public Transform BeamRoot => beamRoot;
        public int BeamRendererCount => beamRenderers != null ? beamRenderers.Length : 0;

        private void Awake()
        {
            ResolveReferences();
            CaptureBaseScale();
            Refresh();
        }

        private void OnEnable()
        {
            ResolveReferences();
            CaptureBaseScale();
            Refresh();
        }

        private void OnDisable()
        {
            SetBeamVisible(false);
        }

        private void Update()
        {
            Refresh();
        }

        private void Refresh()
        {
            ResolveReferences();
            if (beamRoot == null)
            {
                return;
            }

            bool active = proxy != null
                && proxy.IsActive
                && proxy.CurrentState == SummonFrontlineProxyState.Attacking;
            SetBeamVisible(active);
            if (!active)
            {
                return;
            }

            int tier = Mathf.Clamp(proxy.ActiveTier, 1, 3);
            float tierScale = 1f + Mathf.Max(0, tier - 1) * tierScaleStep;
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseScale;
            beamRoot.localScale = baseLocalScale * Mathf.Max(0.01f, tierScale * pulse);
            ApplyColor(ResolveTierColor(tier));
        }

        private void SetBeamVisible(bool visible)
        {
            if (beamRoot != null && beamRoot.gameObject.activeSelf != visible)
            {
                beamRoot.gameObject.SetActive(visible);
            }
        }

        private Color ResolveTierColor(int tier)
        {
            return tier switch
            {
                1 => tierOneColor,
                2 => tierTwoColor,
                _ => tierThreeColor
            };
        }

        private void ApplyColor(Color color)
        {
            propertyBlock ??= new MaterialPropertyBlock();
            if (beamRenderers == null)
            {
                return;
            }

            for (int i = 0; i < beamRenderers.Length; i++)
            {
                Renderer renderer = beamRenderers[i];
                if (renderer == null)
                {
                    continue;
                }

                renderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor(BaseColorId, color);
                propertyBlock.SetColor(ColorId, color);
                propertyBlock.SetColor(EmissionColorId, color * 1.5f);
                renderer.SetPropertyBlock(propertyBlock);
            }
        }

        private void ResolveReferences()
        {
            if (proxy == null)
            {
                proxy = GetComponent<SummonFrontlineProxy>();
            }

            if (beamRenderers == null || beamRenderers.Length == 0)
            {
                beamRenderers = beamRoot != null
                    ? beamRoot.GetComponentsInChildren<Renderer>(includeInactive: true)
                    : System.Array.Empty<Renderer>();
            }
        }

        private void CaptureBaseScale()
        {
            if (hasBaseScale || beamRoot == null)
            {
                return;
            }

            baseLocalScale = beamRoot.localScale;
            hasBaseScale = true;
        }
    }
}
