using UnityEngine;

namespace DimensionBrawl.Core
{
    public static class MobileFrameRateBootstrap
    {
        private const int TargetMobileFrameRate = 60;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Apply()
        {
#if UNITY_ANDROID || UNITY_IOS
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = TargetMobileFrameRate;
#endif
        }
    }
}
