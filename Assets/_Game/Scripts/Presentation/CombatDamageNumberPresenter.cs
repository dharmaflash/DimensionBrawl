using System;
using System.Collections.Generic;
using DimensionBrawl.Combat;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DimensionBrawl.Presentation
{
    [DefaultExecutionOrder(700)]
    public sealed class CombatDamageNumberPresenter : MonoBehaviour
    {
        private const float ReferenceWidth = 2560f;
        private const float ReferenceHeight = 1440f;
        private const int InitialPoolSize = 24;

        private static CombatDamageNumberPresenter instance;

        private readonly Dictionary<CombatHealth, Action<DamageInfo>> damageHandlers =
            new Dictionary<CombatHealth, Action<DamageInfo>>();
        private readonly List<CombatHealth> staleHealth = new List<CombatHealth>(16);
        private readonly List<FloatingDamageEntry> activeEntries = new List<FloatingDamageEntry>(32);
        private readonly Stack<FloatingDamageEntry> pooledEntries = new Stack<FloatingDamageEntry>(32);

        [SerializeField] private Color enemyDamageColor = new Color(1f, 0.93f, 0.56f, 1f);
        [SerializeField] private Color heavyEnemyDamageColor = new Color(1f, 0.52f, 0.12f, 1f);
        [SerializeField] private Color playerDamageColor = new Color(1f, 0.16f, 0.08f, 1f);
        [SerializeField] private Color summonDamageColor = new Color(0.2f, 0.95f, 1f, 1f);
        [SerializeField] private Color neutralDamageColor = new Color(0.92f, 0.96f, 1f, 1f);
        [SerializeField, Min(0.1f)] private float lifetimeSeconds = 0.85f;
        [SerializeField, Min(8f)] private float baseFontSize = 42f;
        [SerializeField, Min(1f)] private float heavyScale = 1.28f;

        private Canvas canvas;
        private RectTransform canvasRect;
        private Camera cachedCamera;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (instance != null)
            {
                return;
            }

            CombatDamageNumberPresenter existing = FindFirstObjectByType<CombatDamageNumberPresenter>();
            if (existing != null)
            {
                instance = existing;
                return;
            }

            GameObject presenterObject = new GameObject(nameof(CombatDamageNumberPresenter));
            instance = presenterObject.AddComponent<CombatDamageNumberPresenter>();
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureCanvas();
            WarmPool();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += HandleSceneLoaded;
            CombatHealth.BecameActive += Subscribe;
            CombatHealth.BecameInactive += Unsubscribe;
            SubscribeActiveHealth();
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            CombatHealth.BecameActive -= Subscribe;
            CombatHealth.BecameInactive -= Unsubscribe;
            UnsubscribeAll();
        }

        private void Update()
        {
            EnsureCanvas();
            UpdateEntries();
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            cachedCamera = null;
            SubscribeActiveHealth();
        }

        private void SubscribeActiveHealth()
        {
            IReadOnlyList<CombatHealth> healthComponents = CombatHealth.ActiveInstances;
            for (int i = 0; i < healthComponents.Count; i++)
            {
                Subscribe(healthComponents[i]);
            }
        }

        private void Subscribe(CombatHealth health)
        {
            if (health == null || damageHandlers.ContainsKey(health))
            {
                return;
            }

            Action<DamageInfo> handler = damageInfo => HandleDamaged(health, damageInfo);
            damageHandlers.Add(health, handler);
            health.Damaged += handler;
        }

        private void Unsubscribe(CombatHealth health)
        {
            if (ReferenceEquals(health, null)
                || !damageHandlers.TryGetValue(health, out Action<DamageInfo> handler))
            {
                return;
            }

            if (health != null)
            {
                health.Damaged -= handler;
            }

            damageHandlers.Remove(health);
        }

        private void UnsubscribeAll()
        {
            staleHealth.Clear();
            foreach (CombatHealth health in damageHandlers.Keys)
            {
                staleHealth.Add(health);
            }

            for (int i = 0; i < staleHealth.Count; i++)
            {
                Unsubscribe(staleHealth[i]);
            }

            staleHealth.Clear();
        }

        private void HandleDamaged(CombatHealth target, DamageInfo damageInfo)
        {
            if (target == null || damageInfo.Amount <= 0f)
            {
                return;
            }

            EnsureCanvas();
            FloatingDamageEntry entry = GetEntry();
            bool heavy = IsHeavyDamage(damageInfo);
            Vector2 anchoredPosition = ResolveAnchoredPosition(target, damageInfo);
            entry.Show(
                FormatAmount(damageInfo.Amount, target.Team),
                ResolveColor(target, damageInfo, heavy),
                anchoredPosition,
                ResolveFontSize(target, heavy),
                lifetimeSeconds,
                heavy ? heavyScale : 1f);
            activeEntries.Add(entry);
        }

        private Vector2 ResolveAnchoredPosition(CombatHealth target, DamageInfo damageInfo)
        {
            Camera camera = ResolveCamera();
            Vector3 worldPosition = damageInfo.Point;
            if (!IsValidWorldPoint(worldPosition))
            {
                worldPosition = target.transform.position + Vector3.up * 1.75f;
            }

            if (camera != null)
            {
                Vector3 viewport = camera.WorldToViewportPoint(worldPosition);
                if (viewport.z < 0.01f)
                {
                    worldPosition = target.transform.position + Vector3.up * 1.75f;
                    viewport = camera.WorldToViewportPoint(worldPosition);
                }

                Vector3 screen = camera.WorldToScreenPoint(worldPosition);
                if (viewport.z > 0.01f)
                {
                    screen.x = Mathf.Clamp(screen.x, 80f, Screen.width - 80f);
                    screen.y = Mathf.Clamp(screen.y, 96f, Screen.height - 96f);
                    if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        canvasRect,
                        screen,
                        null,
                        out Vector2 anchoredPosition))
                    {
                        return anchoredPosition;
                    }
                }
            }

            Vector2 fallback = new Vector2(
                UnityEngine.Random.Range(-ReferenceWidth * 0.2f, ReferenceWidth * 0.2f),
                UnityEngine.Random.Range(ReferenceHeight * 0.04f, ReferenceHeight * 0.22f));
            return fallback;
        }

        private Camera ResolveCamera()
        {
            if (cachedCamera != null && cachedCamera.isActiveAndEnabled)
            {
                return cachedCamera;
            }

            cachedCamera = Camera.main;
            if (cachedCamera == null)
            {
                cachedCamera = FindFirstObjectByType<Camera>();
            }

            return cachedCamera;
        }

        private void EnsureCanvas()
        {
            if (canvas != null && canvasRect != null)
            {
                return;
            }

            GameObject canvasObject = new GameObject(
                "CombatDamageNumberCanvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler));
            canvasObject.transform.SetParent(transform, worldPositionStays: false);
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 145;
            canvasRect = canvasObject.GetComponent<RectTransform>();
            canvasRect.anchorMin = Vector2.zero;
            canvasRect.anchorMax = Vector2.one;
            canvasRect.offsetMin = Vector2.zero;
            canvasRect.offsetMax = Vector2.zero;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1f;
        }

        private void WarmPool()
        {
            for (int i = 0; i < InitialPoolSize; i++)
            {
                FloatingDamageEntry entry = CreateEntry();
                entry.SetVisible(false);
                pooledEntries.Push(entry);
            }
        }

        private FloatingDamageEntry GetEntry()
        {
            FloatingDamageEntry entry = pooledEntries.Count > 0 ? pooledEntries.Pop() : CreateEntry();
            entry.SetVisible(true);
            entry.Rect.SetAsLastSibling();
            return entry;
        }

        private FloatingDamageEntry CreateEntry()
        {
            EnsureCanvas();
            GameObject entryObject = new GameObject(
                "DamageNumber",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(TextMeshProUGUI));
            entryObject.transform.SetParent(canvasRect, worldPositionStays: false);
            RectTransform rectTransform = entryObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = new Vector2(220f, 82f);

            TextMeshProUGUI text = entryObject.GetComponent<TextMeshProUGUI>();
            text.alignment = TextAlignmentOptions.Center;
            text.fontStyle = FontStyles.Bold;
            text.raycastTarget = false;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.outlineColor = new Color(0f, 0f, 0f, 0.92f);
            text.outlineWidth = 0.22f;
            text.isOverlay = true;

            return new FloatingDamageEntry(rectTransform, text);
        }

        private void UpdateEntries()
        {
            float deltaTime = Time.unscaledDeltaTime > 0f ? Time.unscaledDeltaTime : 1f / 60f;
            for (int i = activeEntries.Count - 1; i >= 0; i--)
            {
                FloatingDamageEntry entry = activeEntries[i];
                if (entry.Update(deltaTime))
                {
                    continue;
                }

                entry.SetVisible(false);
                activeEntries.RemoveAt(i);
                pooledEntries.Push(entry);
            }
        }

        private Color ResolveColor(CombatHealth target, DamageInfo damageInfo, bool heavy)
        {
            if (target.Team == DamageTeam.Player)
            {
                return playerDamageColor;
            }

            if (target.Team == DamageTeam.AllySummon)
            {
                return summonDamageColor;
            }

            if (target.Team == DamageTeam.Enemy)
            {
                return heavy ? heavyEnemyDamageColor : enemyDamageColor;
            }

            if (CombatTeamUtility.IsPlayerSide(damageInfo.SourceTeam))
            {
                return enemyDamageColor;
            }

            return neutralDamageColor;
        }

        private float ResolveFontSize(CombatHealth target, bool heavy)
        {
            float size = baseFontSize;
            if (target.Team == DamageTeam.Player)
            {
                size += 6f;
            }

            return heavy ? size * heavyScale : size;
        }

        private static string FormatAmount(float amount, DamageTeam targetTeam)
        {
            int rounded = Mathf.CeilToInt(Mathf.Max(0f, amount));
            return targetTeam == DamageTeam.Player ? "-" + rounded.ToString("0") : rounded.ToString("0");
        }

        private static bool IsHeavyDamage(DamageInfo damageInfo)
        {
            return damageInfo.ResponsePolicy == DamageResponsePolicy.Stagger
                || damageInfo.ResponsePolicy == DamageResponsePolicy.Break
                || damageInfo.ResponsePolicy == DamageResponsePolicy.Knockdown
                || damageInfo.Amount >= 180f;
        }

        private static bool IsValidWorldPoint(Vector3 position)
        {
            return !float.IsNaN(position.x)
                && !float.IsInfinity(position.x)
                && !float.IsNaN(position.y)
                && !float.IsInfinity(position.y)
                && !float.IsNaN(position.z)
                && !float.IsInfinity(position.z)
                && position.sqrMagnitude > 0.0001f;
        }

        private sealed class FloatingDamageEntry
        {
            private readonly TextMeshProUGUI text;
            private Vector2 startPosition;
            private Vector2 drift;
            private Color baseColor;
            private float age;
            private float lifetime;
            private float scaleMultiplier;

            public FloatingDamageEntry(RectTransform rect, TextMeshProUGUI text)
            {
                Rect = rect;
                this.text = text;
            }

            public RectTransform Rect { get; }

            public void Show(
                string amount,
                Color color,
                Vector2 anchoredPosition,
                float fontSize,
                float lifetimeSeconds,
                float scale)
            {
                text.text = amount;
                text.color = color;
                text.fontSize = fontSize;
                text.SetVerticesDirty();
                startPosition = anchoredPosition + new Vector2(
                    UnityEngine.Random.Range(-24f, 24f),
                    UnityEngine.Random.Range(-8f, 18f));
                drift = new Vector2(
                    UnityEngine.Random.Range(-70f, 70f),
                    UnityEngine.Random.Range(118f, 168f));
                baseColor = color;
                age = 0f;
                lifetime = Mathf.Max(0.1f, lifetimeSeconds);
                scaleMultiplier = Mathf.Max(0.1f, scale);
                Rect.anchoredPosition = startPosition;
                Rect.localScale = Vector3.one * 0.7f * scaleMultiplier;
            }

            public bool Update(float deltaTime)
            {
                age += deltaTime;
                float t = Mathf.Clamp01(age / lifetime);
                float move = 1f - Mathf.Pow(1f - t, 2.3f);
                Rect.anchoredPosition = startPosition + drift * move + new Vector2(0f, -40f * t * t);

                float pop = t < 0.18f
                    ? Mathf.Lerp(0.7f, 1.28f, Mathf.SmoothStep(0f, 1f, t / 0.18f))
                    : Mathf.Lerp(1.28f, 1f, Mathf.SmoothStep(0f, 1f, (t - 0.18f) / 0.82f));
                Rect.localScale = Vector3.one * pop * scaleMultiplier;

                float alpha = t < 0.58f ? 1f : Mathf.SmoothStep(1f, 0f, (t - 0.58f) / 0.42f);
                Color color = baseColor;
                color.a *= alpha;
                text.color = color;
                return age < lifetime;
            }

            public void SetVisible(bool visible)
            {
                if (Rect != null)
                {
                    Rect.gameObject.SetActive(visible);
                }
            }
        }
    }
}
