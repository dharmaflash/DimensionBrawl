using System;
using System.Collections.Generic;
using UnityEngine;

namespace DimensionBrawl.UI
{
    public interface IProxyCombatHudScreenRectProvider
    {
        bool TryGetProxyHudScreenRect(string proxyHudObject, out Rect screenRect);
    }

    [DisallowMultipleComponent]
    public sealed class ProxyCombatHudTargetResolver : MonoBehaviour
    {
        [Serializable]
        public sealed class TargetBinding
        {
            [SerializeField] private string proxyHudObject;
            [SerializeField] private RectTransform[] targets = Array.Empty<RectTransform>();

            public TargetBinding(string proxyHudObject, RectTransform[] targets)
            {
                this.proxyHudObject = proxyHudObject;
                this.targets = targets ?? Array.Empty<RectTransform>();
            }

            public string ProxyHudObject => proxyHudObject;
            public RectTransform[] Targets => targets ?? Array.Empty<RectTransform>();
        }

        [SerializeField] private TargetBinding[] targetBindings = Array.Empty<TargetBinding>();

        private readonly List<TargetBinding> runtimeBindings = new List<TargetBinding>();
        private readonly List<RectTransform> resolvedTargets = new List<RectTransform>();

        public void Configure(TargetBinding[] bindings)
        {
            targetBindings = bindings ?? Array.Empty<TargetBinding>();
        }

        public void RegisterTarget(string proxyHudObject, RectTransform target)
        {
            RegisterTargetGroup(proxyHudObject, target != null ? new[] { target } : Array.Empty<RectTransform>());
        }

        public void RegisterTargetGroup(string proxyHudObject, params RectTransform[] targets)
        {
            RemoveRuntimeBinding(proxyHudObject);
            runtimeBindings.Add(new TargetBinding(proxyHudObject, targets));
        }

        public void ClearRuntimeTargets()
        {
            runtimeBindings.Clear();
        }

        public bool TryResolve(string proxyHudObject, out IReadOnlyList<RectTransform> targets)
        {
            resolvedTargets.Clear();
            AppendMatches(targetBindings, proxyHudObject);
            AppendMatches(runtimeBindings, proxyHudObject);
            targets = resolvedTargets;
            return resolvedTargets.Count > 0;
        }

        private void AppendMatches(IReadOnlyList<TargetBinding> bindings, string proxyHudObject)
        {
            if (bindings == null)
            {
                return;
            }

            for (int i = 0; i < bindings.Count; i++)
            {
                TargetBinding binding = bindings[i];
                if (binding == null ||
                    !string.Equals(binding.ProxyHudObject, proxyHudObject, StringComparison.Ordinal))
                {
                    continue;
                }

                RectTransform[] targets = binding.Targets;
                for (int j = 0; j < targets.Length; j++)
                {
                    if (targets[j] != null && !resolvedTargets.Contains(targets[j]))
                    {
                        resolvedTargets.Add(targets[j]);
                    }
                }
            }
        }

        private void RemoveRuntimeBinding(string proxyHudObject)
        {
            for (int i = runtimeBindings.Count - 1; i >= 0; i--)
            {
                TargetBinding binding = runtimeBindings[i];
                if (binding != null &&
                    string.Equals(binding.ProxyHudObject, proxyHudObject, StringComparison.Ordinal))
                {
                    runtimeBindings.RemoveAt(i);
                }
            }
        }
    }

    [DisallowMultipleComponent]
    public sealed partial class ProxyCombatHudTargetSurface : MonoBehaviour
    {
        [Serializable]
        public sealed class TargetAnchorBinding
        {
            [SerializeField] private string proxyHudObject;
            [SerializeField] private RectTransform target;

            public TargetAnchorBinding(string proxyHudObject, RectTransform target)
            {
                this.proxyHudObject = proxyHudObject;
                this.target = target;
            }

            public string ProxyHudObject => proxyHudObject;
            public RectTransform Target => target;
        }

        private static readonly string[] DefaultProxyTargets =
        {
            "Hud.BasicAttackButton",
            "Hud.DodgeButton",
            "Hud.SignatureSkillButton",
            "Hud.PartnerSkillButton",
            "Hud.PartyPortraitSlots[1]",
            "Hud.PartyPortraitSlots[2]"
        };

        [Header("References")]
        [SerializeField] private ProxyCombatHudTargetResolver targetResolver;
        [SerializeField] private MonoBehaviour screenRectProvider;
        [SerializeField] private Canvas canvas;
        [SerializeField] private RectTransform targetRoot;

        [Header("Proxy Anchors")]
        [SerializeField] private TargetAnchorBinding[] targetAnchors = Array.Empty<TargetAnchorBinding>();

        public ProxyCombatHudTargetResolver TargetResolver => targetResolver;
        public MonoBehaviour ScreenRectProvider => screenRectProvider;
        public Canvas Canvas => canvas;
        public RectTransform TargetRoot => targetRoot;
        public TargetAnchorBinding[] TargetAnchors => targetAnchors ?? Array.Empty<TargetAnchorBinding>();

        private void Awake()
        {
            ResolveReferences();
            EnsureDefaultTargets();
            ConfigureResolverBindings();
            SyncTargetRects();
        }

        private void LateUpdate()
        {
            SyncTargetRects();
        }

        public void Configure(
            ProxyCombatHudTargetResolver newTargetResolver,
            MonoBehaviour newScreenRectProvider,
            Canvas newCanvas = null,
            RectTransform newTargetRoot = null)
        {
            targetResolver = newTargetResolver;
            screenRectProvider = newScreenRectProvider;
            canvas = newCanvas;
            targetRoot = newTargetRoot;
            ResolveReferences();
            EnsureDefaultTargets();
            ConfigureResolverBindings();
            SyncTargetRects();
        }

        public void RebuildDefaultTargets()
        {
            ResolveReferences();
            targetAnchors = Array.Empty<TargetAnchorBinding>();
            EnsureDefaultTargets();
            ConfigureResolverBindings();
            SyncTargetRects();
        }

        public void SyncTargetRects()
        {
            IProxyCombatHudScreenRectProvider provider = screenRectProvider as IProxyCombatHudScreenRectProvider;
            if (provider == null)
            {
                return;
            }

            TargetAnchorBinding[] anchors = TargetAnchors;
            for (int i = 0; i < anchors.Length; i++)
            {
                TargetAnchorBinding anchor = anchors[i];
                if (anchor == null || anchor.Target == null)
                {
                    continue;
                }

                if (provider.TryGetProxyHudScreenRect(anchor.ProxyHudObject, out Rect screenRect)
                    && screenRect.width > 0f
                    && screenRect.height > 0f)
                {
                    ApplyScreenRect(anchor.Target, screenRect);
                    anchor.Target.gameObject.SetActive(true);
                }
                else
                {
                    anchor.Target.gameObject.SetActive(false);
                }
            }
        }

        private void ResolveReferences()
        {
            if (targetResolver == null)
            {
                targetResolver = GetComponent<ProxyCombatHudTargetResolver>();
            }

            if (screenRectProvider == null)
            {
                MonoBehaviour[] behaviours = GetComponents<MonoBehaviour>();
                for (int i = 0; i < behaviours.Length; i++)
                {
                    if (behaviours[i] is IProxyCombatHudScreenRectProvider)
                    {
                        screenRectProvider = behaviours[i];
                        break;
                    }
                }
            }

            if (canvas == null)
            {
                canvas = GetComponent<Canvas>();
            }

            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 100;
            }

            if (targetRoot == null)
            {
                Transform existing = transform.Find("ProxyCombatHudTargets");
                targetRoot = existing as RectTransform;
                if (targetRoot == null)
                {
                    var targetRootObject = new GameObject("ProxyCombatHudTargets", typeof(RectTransform));
                    targetRootObject.transform.SetParent(transform, worldPositionStays: false);
                    targetRoot = targetRootObject.GetComponent<RectTransform>();
                }
            }

            ConfigureStretch(targetRoot);
        }

        private void EnsureDefaultTargets()
        {
            var anchors = new List<TargetAnchorBinding>(TargetAnchors);
            for (int i = 0; i < DefaultProxyTargets.Length; i++)
            {
                string proxyHudObject = DefaultProxyTargets[i];
                if (FindAnchor(anchors, proxyHudObject) != null)
                {
                    continue;
                }

                RectTransform anchor = EnsureAnchorObject(proxyHudObject);
                anchors.Add(new TargetAnchorBinding(proxyHudObject, anchor));
            }

            targetAnchors = anchors.ToArray();
        }

        private void ConfigureResolverBindings()
        {
            if (targetResolver == null)
            {
                return;
            }

            TargetAnchorBinding[] anchors = TargetAnchors;
            var bindings = new ProxyCombatHudTargetResolver.TargetBinding[anchors.Length];
            for (int i = 0; i < anchors.Length; i++)
            {
                RectTransform target = anchors[i] != null ? anchors[i].Target : null;
                bindings[i] = new ProxyCombatHudTargetResolver.TargetBinding(
                    anchors[i] != null ? anchors[i].ProxyHudObject : string.Empty,
                    target != null ? new[] { target } : Array.Empty<RectTransform>());
            }

            targetResolver.Configure(bindings);
        }

        private RectTransform EnsureAnchorObject(string proxyHudObject)
        {
            string objectName = "ProxyTarget_" + SanitizeName(proxyHudObject);
            Transform existing = targetRoot != null ? targetRoot.Find(objectName) : null;
            if (existing is RectTransform existingRect)
            {
                return existingRect;
            }

            var anchorObject = new GameObject(objectName, typeof(RectTransform));
            anchorObject.transform.SetParent(targetRoot, worldPositionStays: false);
            RectTransform anchor = anchorObject.GetComponent<RectTransform>();
            anchor.pivot = new Vector2(0.5f, 0.5f);
            anchor.anchorMin = new Vector2(0.5f, 0.5f);
            anchor.anchorMax = new Vector2(0.5f, 0.5f);
            anchor.sizeDelta = Vector2.zero;
            return anchor;
        }

        private static TargetAnchorBinding FindAnchor(List<TargetAnchorBinding> anchors, string proxyHudObject)
        {
            for (int i = 0; i < anchors.Count; i++)
            {
                if (anchors[i] != null &&
                    string.Equals(anchors[i].ProxyHudObject, proxyHudObject, StringComparison.Ordinal))
                {
                    return anchors[i];
                }
            }

            return null;
        }

        private static void ApplyScreenRect(RectTransform target, Rect screenRect)
        {
            float width = Mathf.Max(1f, Screen.width);
            float height = Mathf.Max(1f, Screen.height);
            float xMin = Mathf.Clamp01(screenRect.xMin / width);
            float xMax = Mathf.Clamp01(screenRect.xMax / width);
            float yMin = Mathf.Clamp01((height - screenRect.yMax) / height);
            float yMax = Mathf.Clamp01((height - screenRect.yMin) / height);

            target.anchorMin = new Vector2(xMin, yMin);
            target.anchorMax = new Vector2(xMax, yMax);
            target.offsetMin = Vector2.zero;
            target.offsetMax = Vector2.zero;
            target.pivot = new Vector2(0.5f, 0.5f);
        }

        private static void ConfigureStretch(RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                return;
            }

            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
        }

        private static string SanitizeName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "Unnamed";
            }

            return value
                .Replace("Hud.", string.Empty)
                .Replace("[", "_")
                .Replace("]", string.Empty)
                .Replace(".", "_")
                .Replace("/", "_");
        }
    }
}
