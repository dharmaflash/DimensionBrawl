using DimensionBrawl.Combat;
using DimensionBrawl.LevelDesign;
using UnityEngine;

namespace DimensionBrawl.Presentation
{
    [DisallowMultipleComponent]
    public sealed class BossBarrageLaneTelegraphPresenter : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        [Header("References")]
        [SerializeField] private BossBarrageEmitter bossBarrageEmitter;
        [SerializeField] private SummonLaneSpace laneSpace;
        [SerializeField] private Transform markerRoot;
        [SerializeField] private Transform[] markerTransforms = System.Array.Empty<Transform>();
        [SerializeField] private Renderer[] markerRenderers = System.Array.Empty<Renderer>();

        [Header("Readability")]
        [SerializeField] private Color windupColor = new Color(1f, 0.62f, 0.18f, 0.64f);
        [SerializeField] private Color releaseColor = new Color(1f, 0.96f, 0.44f, 0.9f);
        [SerializeField, Min(0f)] private float markerHeight = 0.075f;
        [SerializeField, Min(0.01f)] private float markerThickness = 0.035f;
        [SerializeField, Min(0.05f)] private float backlineMarkerWidth = 1.2f;
        [SerializeField, Min(0.05f)] private float forwardMarkerWidth = 0.68f;
        [SerializeField, Min(0.05f)] private float backlineMarkerDepth = 1.18f;
        [SerializeField, Min(0.05f)] private float forwardMarkerDepth = 0.76f;
        [SerializeField, Min(0.01f)] private float releaseFlashSeconds = 0.18f;
        [SerializeField, Min(0f)] private float pulseScale = 0.08f;
        [SerializeField, Min(0.01f)] private float pulseSpeed = 10f;

        private MaterialPropertyBlock propertyBlock;
        private Vector2[] previewBuffer = System.Array.Empty<Vector2>();
        private bool subscribed;
        private float releaseFlashTimer;
        private int visibleMarkerCount;
        private int lastPreviewCount;
        private int windupRefreshCount;
        private int releaseFlashCount;
        private string lastPatternId = string.Empty;

        public BossBarrageEmitter BossBarrageEmitter => bossBarrageEmitter;
        public SummonLaneSpace LaneSpace => laneSpace;
        public int MarkerCount => markerTransforms != null ? markerTransforms.Length : 0;
        public int VisibleMarkerCount => visibleMarkerCount;
        public int LastPreviewCount => lastPreviewCount;
        public int WindupRefreshCount => windupRefreshCount;
        public int ReleaseFlashCount => releaseFlashCount;
        public string LastPatternId => lastPatternId;

        public void Configure(
            BossBarrageEmitter newEmitter,
            SummonLaneSpace newLaneSpace,
            Transform newMarkerRoot,
            Transform[] newMarkerTransforms,
            Renderer[] newMarkerRenderers)
        {
            Unsubscribe();
            bossBarrageEmitter = newEmitter;
            laneSpace = newLaneSpace;
            markerRoot = newMarkerRoot;
            markerTransforms = newMarkerTransforms ?? System.Array.Empty<Transform>();
            markerRenderers = newMarkerRenderers ?? System.Array.Empty<Renderer>();
            EnsurePreviewBuffer();
            Subscribe();
            RefreshNow();
        }

        public void RefreshNow()
        {
            EnsurePreviewBuffer();
            bool shouldShow = bossBarrageEmitter != null
                && laneSpace != null
                && markerTransforms != null
                && (bossBarrageEmitter.IsWindupActive || releaseFlashTimer > 0f);

            if (!shouldShow)
            {
                HideMarkers();
                return;
            }

            if (bossBarrageEmitter.IsWindupActive)
            {
                lastPreviewCount = bossBarrageEmitter.BuildPendingLaneTargetPreview(previewBuffer);
            }

            int count = Mathf.Min(lastPreviewCount, markerTransforms.Length);
            float risk01 = Mathf.Clamp01(bossBarrageEmitter.PendingForwardRisk01);
            float release01 = releaseFlashSeconds > 0f ? Mathf.Clamp01(releaseFlashTimer / releaseFlashSeconds) : 0f;

            for (int i = 0; i < markerTransforms.Length; i++)
            {
                bool visible = i < count && markerTransforms[i] != null;
                SetMarkerVisible(i, visible);
                if (!visible)
                {
                    continue;
                }

                RefreshMarkerTransform(i, previewBuffer[i], risk01);
                ApplyColor(ResolveColor(risk01, release01), ResolveRenderer(i));
            }

            visibleMarkerCount = count;
        }

        private void Awake()
        {
            if (markerRoot == null)
            {
                markerRoot = transform;
            }

            propertyBlock ??= new MaterialPropertyBlock();
            EnsurePreviewBuffer();
        }

        private void OnEnable()
        {
            Subscribe();
            RefreshNow();
        }

        private void OnDisable()
        {
            Unsubscribe();
            HideMarkers();
        }

        private void Update()
        {
            if (releaseFlashTimer > 0f)
            {
                releaseFlashTimer = Mathf.Max(0f, releaseFlashTimer - Time.deltaTime);
            }

            RefreshNow();
        }

        private void HandleWindupStarted(BossBarrageEmitter emitter, BossBarragePatternProfile pattern)
        {
            lastPatternId = pattern != null ? pattern.PatternId : string.Empty;
            windupRefreshCount++;
            RefreshNow();
        }

        private void HandleWaveFired(BossBarrageEmitter emitter, BossBarragePatternProfile pattern, int spawnedCount)
        {
            lastPatternId = pattern != null ? pattern.PatternId : string.Empty;
            releaseFlashTimer = releaseFlashSeconds;
            releaseFlashCount++;
            RefreshNow();
        }

        private void RefreshMarkerTransform(int index, Vector2 lanePoint, float risk01)
        {
            Transform marker = markerTransforms[index];
            marker.position = laneSpace.GetLaneWorldPoint(lanePoint.x, lanePoint.y, markerHeight);
            marker.rotation = laneSpace.transform.rotation;
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed + index * 0.6f) * pulseScale;
            marker.localScale = new Vector3(
                Mathf.Lerp(backlineMarkerWidth, forwardMarkerWidth, risk01) * pulse,
                markerThickness,
                Mathf.Lerp(backlineMarkerDepth, forwardMarkerDepth, risk01) * pulse);
        }

        private Color ResolveColor(float risk01, float release01)
        {
            Color color = Color.Lerp(windupColor, releaseColor, release01);
            color.a = Mathf.Clamp01(Mathf.Lerp(0.42f, color.a, risk01));
            return color;
        }

        private void ApplyColor(Color color, Renderer markerRenderer)
        {
            if (markerRenderer == null)
            {
                return;
            }

            propertyBlock ??= new MaterialPropertyBlock();
            markerRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(BaseColorId, color);
            propertyBlock.SetColor(ColorId, color);
            propertyBlock.SetColor(EmissionColorId, color * 1.2f);
            markerRenderer.SetPropertyBlock(propertyBlock);
        }

        private Renderer ResolveRenderer(int index)
        {
            if (markerRenderers != null && index < markerRenderers.Length && markerRenderers[index] != null)
            {
                return markerRenderers[index];
            }

            return markerTransforms[index] != null ? markerTransforms[index].GetComponent<Renderer>() : null;
        }

        private void SetMarkerVisible(int index, bool visible)
        {
            if (markerTransforms[index] != null && markerTransforms[index].gameObject.activeSelf != visible)
            {
                markerTransforms[index].gameObject.SetActive(visible);
            }
        }

        private void HideMarkers()
        {
            visibleMarkerCount = 0;
            if (markerTransforms == null)
            {
                return;
            }

            for (int i = 0; i < markerTransforms.Length; i++)
            {
                SetMarkerVisible(i, false);
            }
        }

        private void EnsurePreviewBuffer()
        {
            int markerCount = markerTransforms != null ? markerTransforms.Length : 0;
            if (previewBuffer.Length != markerCount)
            {
                previewBuffer = markerCount > 0 ? new Vector2[markerCount] : System.Array.Empty<Vector2>();
            }
        }

        private void Subscribe()
        {
            if (subscribed || bossBarrageEmitter == null)
            {
                return;
            }

            bossBarrageEmitter.WindupStarted += HandleWindupStarted;
            bossBarrageEmitter.WaveFired += HandleWaveFired;
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed || bossBarrageEmitter == null)
            {
                subscribed = false;
                return;
            }

            bossBarrageEmitter.WindupStarted -= HandleWindupStarted;
            bossBarrageEmitter.WaveFired -= HandleWaveFired;
            subscribed = false;
        }
    }
}
