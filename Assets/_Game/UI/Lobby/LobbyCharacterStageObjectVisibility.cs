using System;
using UnityEngine;

namespace DimensionBrawl.UI
{
    [DefaultExecutionOrder(-100)]
    [DisallowMultipleComponent]
    public sealed class LobbyCharacterStageObjectVisibility : MonoBehaviour
    {
        [Serializable]
        private struct VisibilityOverride
        {
            [SerializeField] private string objectName;
            [SerializeField] private bool isVisible;

            public string ObjectName => objectName;
            public bool IsVisible => isVisible;
        }

        [SerializeField] private Transform searchRoot;
        [SerializeField] private VisibilityOverride[] visibilityOverrides = Array.Empty<VisibilityOverride>();

        private void Awake()
        {
            ApplyVisibility();
        }

        private void OnEnable()
        {
            ApplyVisibility();
        }

        [ContextMenu("Apply Lobby Object Visibility")]
        public void ApplyVisibility()
        {
            Transform root = searchRoot != null ? searchRoot : transform;
            if (root == null)
            {
                return;
            }

            Transform[] children = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < visibilityOverrides.Length; i++)
            {
                ApplyOverride(children, visibilityOverrides[i]);
            }
        }

        private static void ApplyOverride(Transform[] children, VisibilityOverride visibilityOverride)
        {
            string objectName = visibilityOverride.ObjectName;
            if (string.IsNullOrWhiteSpace(objectName))
            {
                return;
            }

            for (int i = 0; i < children.Length; i++)
            {
                Transform child = children[i];
                if (child == null || !string.Equals(child.name, objectName, StringComparison.Ordinal))
                {
                    continue;
                }

                child.gameObject.SetActive(visibilityOverride.IsVisible);
            }
        }
    }
}
