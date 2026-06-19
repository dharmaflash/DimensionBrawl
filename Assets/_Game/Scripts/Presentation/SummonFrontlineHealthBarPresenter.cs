using DimensionBrawl.Combat;
using UnityEngine;

namespace DimensionBrawl.Presentation
{
    [DisallowMultipleComponent]
    public sealed class SummonFrontlineHealthBarPresenter : MonoBehaviour
    {
        [SerializeField] private SummonFrontlineProxy proxy;
        [SerializeField] private CombatHealth health;
        [SerializeField] private Transform barRoot;
        [SerializeField] private Transform fillRoot;
        [SerializeField] private Renderer[] barRenderers = System.Array.Empty<Renderer>();
        [SerializeField] private Vector3 fullFillLocalScale = Vector3.one;
        [SerializeField] private Vector3 fullFillLocalPosition;

        private float lastHealthRatio;

        public SummonFrontlineProxy Proxy => proxy;
        public CombatHealth Health => health;
        public Transform BarRoot => barRoot;
        public Transform FillRoot => fillRoot;
        public int RendererCount => barRenderers != null ? barRenderers.Length : 0;
        public float LastHealthRatio => lastHealthRatio;
        public bool IsShowing => barRoot != null && barRoot.gameObject.activeSelf;

        private void Awake()
        {
            ResolveReferences();
            if (fillRoot != null && fullFillLocalScale == Vector3.zero)
            {
                CacheFillTransform();
            }
        }

        private void OnEnable()
        {
            RefreshNow();
        }

        private void OnDisable()
        {
            SetVisible(false);
        }

        private void Update()
        {
            RefreshNow();
        }

        public void ConfigurePresentation(
            SummonFrontlineProxy newProxy,
            CombatHealth newHealth,
            Transform newBarRoot,
            Transform newFillRoot,
            Renderer[] newBarRenderers)
        {
            proxy = newProxy;
            health = newHealth;
            barRoot = newBarRoot;
            fillRoot = newFillRoot;
            barRenderers = newBarRenderers ?? System.Array.Empty<Renderer>();
            CacheFillTransform();
            if (Application.isPlaying)
            {
                RefreshNow();
            }
            else
            {
                SetVisible(false);
            }
        }

        public void RefreshNow()
        {
            ResolveReferences();
            bool shouldShow = proxy != null
                && proxy.IsActive
                && health != null
                && health.IsAlive;

            lastHealthRatio = shouldShow ? Mathf.Clamp01(health.HealthRatio) : 0f;
            ApplyFill(lastHealthRatio);
            SetVisible(shouldShow);
        }

        private void ResolveReferences()
        {
            if (proxy == null)
            {
                proxy = GetComponent<SummonFrontlineProxy>();
            }

            if (health == null)
            {
                health = proxy != null ? proxy.Health : GetComponent<CombatHealth>();
            }

            if ((barRenderers == null || barRenderers.Length == 0) && barRoot != null)
            {
                barRenderers = barRoot.GetComponentsInChildren<Renderer>(includeInactive: true);
            }
        }

        private void CacheFillTransform()
        {
            if (fillRoot == null)
            {
                fullFillLocalScale = Vector3.one;
                fullFillLocalPosition = Vector3.zero;
                return;
            }

            fullFillLocalScale = fillRoot.localScale;
            fullFillLocalPosition = fillRoot.localPosition;
        }

        private void ApplyFill(float healthRatio)
        {
            if (fillRoot == null)
            {
                return;
            }

            float ratio = Mathf.Clamp01(healthRatio);
            Vector3 scale = fullFillLocalScale;
            scale.x = fullFillLocalScale.x * ratio;
            fillRoot.localScale = scale;

            Vector3 position = fullFillLocalPosition;
            position.x -= (fullFillLocalScale.x - scale.x) * 0.5f;
            fillRoot.localPosition = position;
        }

        private void SetVisible(bool value)
        {
            if (barRoot != null && barRoot.gameObject.activeSelf != value)
            {
                barRoot.gameObject.SetActive(value);
            }

            if (barRenderers == null)
            {
                return;
            }

            for (int i = 0; i < barRenderers.Length; i++)
            {
                if (barRenderers[i] != null)
                {
                    barRenderers[i].enabled = value;
                }
            }
        }
    }
}
