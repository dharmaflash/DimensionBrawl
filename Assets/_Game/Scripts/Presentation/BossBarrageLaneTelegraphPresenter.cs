using System.Collections;
using System.Collections.Generic;
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
        private readonly Dictionary<Renderer, Material[]> originalMarkerMaterials =
            new Dictionary<Renderer, Material[]>();
        private readonly Dictionary<Renderer, Material[]> runtimeMarkerMaterials =
            new Dictionary<Renderer, Material[]>();
        private Vector2[] previewBuffer = System.Array.Empty<Vector2>();
        private bool subscribed;
        private float releaseFlashTimer;
        private int visibleMarkerCount;
        private int lastPreviewCount;
        private int windupRefreshCount;
        private int releaseFlashCount;
        private string lastPatternId = string.Empty;
        private BossBarragePatternProfile visiblePattern;
        private Vector3 lastMarkerScale;
        private Color lastMarkerColor;
        private bool countedCurrentWindup;
        private bool countedCurrentRelease;
        private Coroutine refreshRoutine;

        public BossBarrageEmitter BossBarrageEmitter => bossBarrageEmitter;
        public SummonLaneSpace LaneSpace => laneSpace;
        public int MarkerCount => markerTransforms != null ? markerTransforms.Length : 0;
        public int VisibleMarkerCount => visibleMarkerCount;
        public int LastPreviewCount => lastPreviewCount;
        public int WindupRefreshCount => windupRefreshCount;
        public int ReleaseFlashCount => releaseFlashCount;
        public string LastPatternId => lastPatternId;
        public BossBarragePatternProfile VisiblePattern => visiblePattern;
        public Vector3 LastMarkerScale => lastMarkerScale;
        public Color LastMarkerColor => lastMarkerColor;
        public bool IsRefreshing => refreshRoutine != null;

        public void Configure(
            BossBarrageEmitter newEmitter,
            SummonLaneSpace newLaneSpace,
            Transform newMarkerRoot,
            Transform[] newMarkerTransforms,
            Renderer[] newMarkerRenderers)
        {
            StopRefreshRoutine();
            Unsubscribe();
            ReleaseRuntimeMarkerMaterials();
            bossBarrageEmitter = newEmitter;
            laneSpace = newLaneSpace;
            markerRoot = newMarkerRoot;
            markerTransforms = newMarkerTransforms ?? System.Array.Empty<Transform>();
            markerRenderers = newMarkerRenderers ?? System.Array.Empty<Renderer>();
            EnsurePreviewBuffer();
            Subscribe();
            RefreshNow();
            StartRefreshRoutineIfNeeded();
        }

        public void RefreshNow()
        {
            EnsurePreviewBuffer();
            RefreshReleaseFlashFallback();
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
                visiblePattern = bossBarrageEmitter.CurrentPattern;
                lastPatternId = visiblePattern != null ? visiblePattern.PatternId : string.Empty;
                lastPreviewCount = bossBarrageEmitter.BuildPendingLaneTargetPreview(previewBuffer);
                CountWindupRefreshOnce();
                countedCurrentRelease = false;
            }
            else
            {
                countedCurrentWindup = false;
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

                RefreshMarkerTransform(i, previewBuffer[i], risk01, visiblePattern);
                ApplyColor(ResolveColor(visiblePattern, risk01, release01), ResolveRenderer(i));
            }

            visibleMarkerCount = count;
            StartRefreshRoutineIfNeeded();
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
            StartRefreshRoutineIfNeeded();
        }

        private void OnDisable()
        {
            StopRefreshRoutine();
            Unsubscribe();
            HideMarkers();
            ReleaseRuntimeMarkerMaterials();
        }

        private void OnDestroy()
        {
            StopRefreshRoutine();
            Unsubscribe();
            ReleaseRuntimeMarkerMaterials();
        }

        private IEnumerator RefreshWhileVisible()
        {
            yield return null;

            while (isActiveAndEnabled)
            {
                if (releaseFlashTimer > 0f)
                {
                    releaseFlashTimer = Mathf.Max(0f, releaseFlashTimer - Time.deltaTime);
                }

                RefreshNow();
                if (!ShouldAnimate())
                {
                    break;
                }

                yield return null;
            }

            refreshRoutine = null;
        }

        private void HandleWindupStarted(BossBarrageEmitter emitter, BossBarragePatternProfile pattern)
        {
            visiblePattern = pattern;
            lastPatternId = pattern != null ? pattern.PatternId : string.Empty;
            CountWindupRefreshOnce();
            RefreshNow();
            StartRefreshRoutineIfNeeded();
        }

        private void CountWindupRefreshOnce()
        {
            if (countedCurrentWindup)
            {
                return;
            }

            windupRefreshCount++;
            countedCurrentWindup = true;
        }

        private void HandleWaveFired(BossBarrageEmitter emitter, BossBarragePatternProfile pattern, int spawnedCount)
        {
            visiblePattern = pattern;
            lastPatternId = pattern != null ? pattern.PatternId : string.Empty;
            releaseFlashTimer = releaseFlashSeconds;
            releaseFlashCount++;
            countedCurrentRelease = true;
            RefreshNow();
            StartRefreshRoutineIfNeeded();
        }

        private bool ShouldAnimate()
        {
            return bossBarrageEmitter != null
                && (bossBarrageEmitter.IsWindupActive || releaseFlashTimer > 0f);
        }

        private void StartRefreshRoutineIfNeeded()
        {
            if (refreshRoutine == null && Application.isPlaying && isActiveAndEnabled && ShouldAnimate())
            {
                refreshRoutine = StartCoroutine(RefreshWhileVisible());
            }
        }

        private void StopRefreshRoutine()
        {
            if (refreshRoutine == null)
            {
                return;
            }

            StopCoroutine(refreshRoutine);
            refreshRoutine = null;
        }

        private void RefreshReleaseFlashFallback()
        {
            if (bossBarrageEmitter == null
                || bossBarrageEmitter.IsWindupActive
                || countedCurrentRelease
                || releaseFlashTimer > 0f
                || lastPreviewCount <= 0
                || bossBarrageEmitter.ActiveProjectileCount <= 0)
            {
                return;
            }

            releaseFlashTimer = releaseFlashSeconds;
            releaseFlashCount++;
            countedCurrentRelease = true;
        }

        private void RefreshMarkerTransform(
            int index,
            Vector2 lanePoint,
            float risk01,
            BossBarragePatternProfile pattern)
        {
            Transform marker = markerTransforms[index];
            marker.position = laneSpace.GetLaneWorldPoint(lanePoint.x, lanePoint.y, markerHeight);
            marker.rotation = laneSpace.transform.rotation;
            float patternPulseScale = pattern != null ? pattern.TelegraphPulseScale : 1f;
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed + index * 0.6f) * pulseScale * patternPulseScale;
            float widthScale = pattern != null ? pattern.TelegraphMarkerWidthScale : 1f;
            float depthScale = pattern != null ? pattern.TelegraphMarkerDepthScale : 1f;
            marker.localScale = new Vector3(
                Mathf.Lerp(backlineMarkerWidth, forwardMarkerWidth, risk01) * widthScale * pulse,
                markerThickness,
                Mathf.Lerp(backlineMarkerDepth, forwardMarkerDepth, risk01) * depthScale * pulse);
            lastMarkerScale = marker.localScale;
        }

        private Color ResolveColor(BossBarragePatternProfile pattern, float risk01, float release01)
        {
            Color baseWindupColor = pattern != null ? pattern.TelegraphWindupColor : windupColor;
            Color baseReleaseColor = pattern != null ? pattern.TelegraphReleaseColor : releaseColor;
            Color color = Color.Lerp(baseWindupColor, baseReleaseColor, release01);
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
            ApplyRuntimeMaterialColor(markerRenderer, color);
            lastMarkerColor = color;
        }

        private void ApplyRuntimeMaterialColor(Renderer markerRenderer, Color color)
        {
            Material[] materials = GetOrCreateRuntimeMarkerMaterials(markerRenderer);
            Color emission = color * 1.2f;
            emission.a = color.a;
            for (int index = 0; index < materials.Length; index++)
            {
                Material material = materials[index];
                if (material == null)
                {
                    continue;
                }

                if (material.HasProperty(BaseColorId))
                {
                    material.SetColor(BaseColorId, color);
                }

                if (material.HasProperty(ColorId))
                {
                    material.SetColor(ColorId, color);
                }

                if (material.HasProperty(EmissionColorId))
                {
                    material.SetColor(EmissionColorId, emission);
                }
            }
        }

        private Material[] GetOrCreateRuntimeMarkerMaterials(Renderer markerRenderer)
        {
            if (runtimeMarkerMaterials.TryGetValue(
                    markerRenderer,
                    out Material[] existing))
            {
                return existing;
            }

            Material[] originals = markerRenderer.sharedMaterials;
            Material[] runtime = new Material[originals.Length];
            for (int index = 0; index < originals.Length; index++)
            {
                Material original = originals[index];
                if (original == null)
                {
                    continue;
                }

                runtime[index] = new Material(original)
                {
                    name = original.name + " (Boss Barrage Telegraph Runtime)",
                    hideFlags = HideFlags.DontSave
                };
            }

            originalMarkerMaterials.Add(markerRenderer, originals);
            runtimeMarkerMaterials.Add(markerRenderer, runtime);
            markerRenderer.sharedMaterials = runtime;
            return runtime;
        }

        private void ReleaseRuntimeMarkerMaterials()
        {
            foreach (KeyValuePair<Renderer, Material[]> pair in runtimeMarkerMaterials)
            {
                Renderer markerRenderer = pair.Key;
                if (markerRenderer != null)
                {
                    if (originalMarkerMaterials.TryGetValue(
                            markerRenderer,
                            out Material[] originals))
                    {
                        markerRenderer.sharedMaterials = originals;
                    }

                    markerRenderer.SetPropertyBlock(null);
                }

                Material[] runtime = pair.Value;
                for (int index = 0; index < runtime.Length; index++)
                {
                    Material material = runtime[index];
                    if (material == null)
                    {
                        continue;
                    }

                    if (Application.isPlaying)
                    {
                        Destroy(material);
                    }
                    else
                    {
                        DestroyImmediate(material);
                    }
                }
            }

            runtimeMarkerMaterials.Clear();
            originalMarkerMaterials.Clear();
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

            visiblePattern = null;
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
