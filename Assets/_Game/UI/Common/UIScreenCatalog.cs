using System;
using UnityEngine;

namespace DimensionBrawl.UI
{
    [CreateAssetMenu(menuName = "DimensionBrawl/UI/Screen Catalog")]
    public sealed class UIScreenCatalog : ScriptableObject
    {
        [Serializable]
        public struct ScreenEntry
        {
            [SerializeField] private UIRouteId routeId;
            [SerializeField] private string screenId;
            [SerializeField] private GameObject screenPrefab;
            [SerializeField] private GameObject presentationPrefab;
            [SerializeField] private string bgmContextId;
            [SerializeField] private UICachePolicy cachePolicy;

            public UIRouteId RouteId => routeId;
            public string ScreenId => screenId;
            public GameObject ScreenPrefab => screenPrefab;
            public GameObject PresentationPrefab => presentationPrefab;
            public string BgmContextId => bgmContextId;
            public UICachePolicy CachePolicy => cachePolicy;
        }

        [SerializeField] private ScreenEntry[] screens = Array.Empty<ScreenEntry>();

        public bool TryGetScreen(UIRouteId routeId, out ScreenEntry screen)
        {
            for (int i = 0; i < screens.Length; i++)
            {
                if (screens[i].RouteId == routeId)
                {
                    screen = screens[i];
                    return true;
                }
            }

            screen = default;
            return false;
        }
    }
}
