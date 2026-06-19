using DimensionBrawl.Combat;
using UnityEngine;

namespace DimensionBrawl.Presentation
{
    [DisallowMultipleComponent]
    public sealed class SummonFrontlineProxyPresenter : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        [SerializeField] private SummonFrontlineProxy proxy;
        [SerializeField] private SummonFrontlineClash clash;
        [SerializeField] private Transform pulseRoot;
        [SerializeField] private Renderer[] actorRenderers = System.Array.Empty<Renderer>();
        [SerializeField] private Color tierOneColor = new Color(0.24f, 1f, 0.78f, 0.78f);
        [SerializeField] private Color tierTwoColor = new Color(0.38f, 0.74f, 1f, 0.9f);
        [SerializeField] private Color tierThreeColor = new Color(1f, 0.76f, 0.24f, 1f);
        [SerializeField] private Color flashColor = new Color(1f, 1f, 1f, 1f);
        [SerializeField] private Color clashFlashColor = new Color(1f, 0.9f, 0.38f, 1f);
        [SerializeField, Min(0f)] private float entryFlashSeconds = 0.22f;
        [SerializeField, Min(0f)] private float impactFlashSeconds = 0.18f;
        [SerializeField, Min(0f)] private float clashFlashSeconds = 0.14f;
        [SerializeField, Range(0.2f, 1f)] private float impactFlashProgress = 0.86f;
        [SerializeField, Min(0.01f)] private float pulseSpeed = 8f;
        [SerializeField, Min(0f)] private float pulseScale = 0.08f;
        [SerializeField, Min(0f)] private float tierScaleStep = 0.18f;
        [SerializeField, Min(0f)] private float flashScale = 0.22f;
        [SerializeField, Min(0f)] private float clashFlashScale = 0.16f;

        private MaterialPropertyBlock propertyBlock;
        private Vector3 pulseBaseScale = Vector3.one;
        private float entryFlashTimer;
        private float impactFlashTimer;
        private float clashFlashTimer;
        private bool wasActive;
        private bool impactFlashedThisActivation;
        private int lastObservedTier;
        private int lastObservedClashCount;
        private int entryFlashCount;
        private int impactFlashCount;
        private int clashFlashCount;

        public SummonFrontlineProxy Proxy => proxy;
        public SummonFrontlineClash Clash => clash;
        public Transform PulseRoot => pulseRoot;
        public int RendererCount => actorRenderers != null ? actorRenderers.Length : 0;
        public bool IsShowing => proxy != null && proxy.IsActive;
        public int LastObservedTier => lastObservedTier;
        public int LastObservedClashCount => lastObservedClashCount;
        public int EntryFlashCount => entryFlashCount;
        public int ImpactFlashCount => impactFlashCount;
        public int ClashFlashCount => clashFlashCount;

        private void Awake()
        {
            propertyBlock ??= new MaterialPropertyBlock();
            if (proxy == null)
            {
                proxy = GetComponent<SummonFrontlineProxy>();
            }

            if (clash == null)
            {
                clash = GetComponent<SummonFrontlineClash>();
            }

            if (pulseRoot != null)
            {
                pulseBaseScale = pulseRoot.localScale;
            }

            if (actorRenderers == null || actorRenderers.Length == 0)
            {
                actorRenderers = GetComponentsInChildren<Renderer>(includeInactive: true);
            }
        }

        private void OnEnable()
        {
            RefreshNow();
        }

        private void OnDisable()
        {
            wasActive = false;
            SetPulseVisible(false);
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            if (entryFlashTimer > 0f)
            {
                entryFlashTimer = Mathf.Max(0f, entryFlashTimer - deltaTime);
            }

            if (impactFlashTimer > 0f)
            {
                impactFlashTimer = Mathf.Max(0f, impactFlashTimer - deltaTime);
            }

            if (clashFlashTimer > 0f)
            {
                clashFlashTimer = Mathf.Max(0f, clashFlashTimer - deltaTime);
            }

            RefreshNow();
        }

        public void ConfigurePresentation(
            SummonFrontlineProxy newProxy,
            Transform newPulseRoot,
            Renderer[] newActorRenderers)
        {
            proxy = newProxy;
            pulseRoot = newPulseRoot;
            pulseBaseScale = pulseRoot != null ? pulseRoot.localScale : Vector3.one;
            actorRenderers = newActorRenderers ?? System.Array.Empty<Renderer>();
            RefreshNow();
        }

        public void ConfigureClashReference(SummonFrontlineClash newClash)
        {
            clash = newClash;
            lastObservedClashCount = clash != null ? clash.TotalClashCount : 0;
            RefreshNow();
        }

        public void RefreshNow()
        {
            bool active = proxy != null && proxy.IsActive;
            if (active)
            {
                int tier = Mathf.Clamp(proxy.ActiveTier, 1, 3);
                if (!wasActive)
                {
                    entryFlashTimer = Mathf.Max(entryFlashTimer, entryFlashSeconds);
                    impactFlashedThisActivation = false;
                    entryFlashCount++;
                }

                lastObservedTier = tier;
                ObserveClashCount();
                if (!impactFlashedThisActivation && proxy.AdvanceProgress01 >= impactFlashProgress)
                {
                    impactFlashTimer = Mathf.Max(impactFlashTimer, impactFlashSeconds);
                    impactFlashedThisActivation = true;
                    impactFlashCount++;
                }

                SetPulseVisible(true);
                RefreshVisual(tier);
            }
            else
            {
                SetPulseVisible(false);
            }

            wasActive = active;
        }

        private void RefreshVisual(int tier)
        {
            Color tierColor = ResolveTierColor(tier);
            float flash = ResolveEntryImpactFlashWeight();
            float clashFlash = ResolveClashFlashWeight();
            Color color = Color.Lerp(tierColor, flashColor, flash);
            color = Color.Lerp(color, clashFlashColor, clashFlash);
            ApplyColor(color);

            if (pulseRoot == null)
            {
                return;
            }

            float tierScale = 1f + (Mathf.Clamp(tier, 1, 3) - 1) * tierScaleStep;
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseScale;
            float scale = tierScale * (pulse + flash * flashScale + clashFlash * clashFlashScale);
            pulseRoot.localScale = pulseBaseScale * Mathf.Max(0.01f, scale);
        }

        private void ObserveClashCount()
        {
            if (clash == null)
            {
                return;
            }

            int currentClashCount = clash.TotalClashCount;
            if (currentClashCount > lastObservedClashCount)
            {
                clashFlashTimer = Mathf.Max(clashFlashTimer, clashFlashSeconds);
                clashFlashCount += currentClashCount - lastObservedClashCount;
            }

            lastObservedClashCount = currentClashCount;
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

        private float ResolveEntryImpactFlashWeight()
        {
            float entry = entryFlashSeconds > 0f ? Mathf.Clamp01(entryFlashTimer / entryFlashSeconds) : 0f;
            float impact = impactFlashSeconds > 0f ? Mathf.Clamp01(impactFlashTimer / impactFlashSeconds) : 0f;
            return Mathf.Max(entry, impact);
        }

        private float ResolveClashFlashWeight()
        {
            return clashFlashSeconds > 0f ? Mathf.Clamp01(clashFlashTimer / clashFlashSeconds) : 0f;
        }

        private void ApplyColor(Color color)
        {
            propertyBlock ??= new MaterialPropertyBlock();
            if (actorRenderers == null)
            {
                return;
            }

            for (int i = 0; i < actorRenderers.Length; i++)
            {
                Renderer actorRenderer = actorRenderers[i];
                if (actorRenderer == null)
                {
                    continue;
                }

                actorRenderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor(BaseColorId, color);
                propertyBlock.SetColor(ColorId, color);
                propertyBlock.SetColor(EmissionColorId, color * 1.25f);
                actorRenderer.SetPropertyBlock(propertyBlock);
            }
        }

        private void SetPulseVisible(bool value)
        {
            if (pulseRoot != null && pulseRoot.gameObject.activeSelf != value)
            {
                pulseRoot.gameObject.SetActive(value);
            }
        }
    }
}
