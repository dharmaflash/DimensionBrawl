using System;
using System.Collections.Generic;
using UnityEngine;

namespace DimensionBrawl.UI
{
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
            runtimeBindings.Add(new TargetBinding(proxyHudObject, targets));
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
    }
}
