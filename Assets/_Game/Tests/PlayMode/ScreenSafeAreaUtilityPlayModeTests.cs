using DimensionBrawl.UI;
using NUnit.Framework;
using UnityEngine;

namespace DimensionBrawl.Tests
{
    public sealed class ScreenSafeAreaUtilityPlayModeTests
    {
        [Test]
        public void FullScreenSafeAreaProducesNoCanvasInsets()
        {
            var screenSize = new Vector2(2560f, 1440f);
            var rawSafeArea = new Rect(Vector2.zero, screenSize);

            Rect guiSafeArea = ScreenSafeAreaUtility.ResolveGuiSafeArea(rawSafeArea, screenSize);
            ScreenSafeAreaInsets insets = ScreenSafeAreaUtility.ResolveCanvasInsets(
                rawSafeArea,
                screenSize,
                screenSize);

            Assert.That(guiSafeArea, Is.EqualTo(rawSafeArea));
            Assert.That(insets.Left, Is.Zero.Within(0.001f));
            Assert.That(insets.Right, Is.Zero.Within(0.001f));
            Assert.That(insets.Top, Is.Zero.Within(0.001f));
            Assert.That(insets.Bottom, Is.Zero.Within(0.001f));
        }

        [Test]
        public void CutoutSafeAreaMapsBottomOriginScreenSpaceToTopOriginGuiAndCanvasInsets()
        {
            var screenSize = new Vector2(2400f, 1080f);
            var rawSafeArea = new Rect(120f, 40f, 2160f, 1000f);

            Rect guiSafeArea = ScreenSafeAreaUtility.ResolveGuiSafeArea(rawSafeArea, screenSize);
            ScreenSafeAreaInsets insets = ScreenSafeAreaUtility.ResolveCanvasInsets(
                rawSafeArea,
                screenSize,
                new Vector2(3200f, 1440f));

            Assert.That(guiSafeArea, Is.EqualTo(new Rect(120f, 40f, 2160f, 1000f)));
            Assert.That(insets.Left, Is.EqualTo(160f).Within(0.001f));
            Assert.That(insets.Right, Is.EqualTo(160f).Within(0.001f));
            Assert.That(insets.Top, Is.EqualTo(53.33333f).Within(0.001f));
            Assert.That(insets.Bottom, Is.EqualTo(53.33333f).Within(0.001f));
        }

        [Test]
        public void EmptySafeAreaFallsBackToTheWholeScreen()
        {
            var screenSize = new Vector2(1920f, 1080f);

            Rect guiSafeArea = ScreenSafeAreaUtility.ResolveGuiSafeArea(default, screenSize);

            Assert.That(guiSafeArea, Is.EqualTo(new Rect(Vector2.zero, screenSize)));
        }

        [Test]
        public void AsymmetricCutoutAnchorRoundTripsWithoutApplyingTheInsetTwice()
        {
            var screenSize = new Vector2(2400f, 1080f);
            var rawSafeArea = new Rect(180f, 24f, 2100f, 1016f);
            var actualHudGuiPoint = new Vector2(2140f, 890f);

            Vector2 normalizedAnchor = ScreenSafeAreaUtility.ResolveNormalizedAnchorFromGuiPoint(
                actualHudGuiPoint,
                rawSafeArea,
                screenSize);
            Vector2 resolvedMarkerPoint = ScreenSafeAreaUtility.ResolveGuiPointFromNormalizedAnchor(
                normalizedAnchor,
                rawSafeArea,
                screenSize);

            Assert.That(resolvedMarkerPoint.x, Is.EqualTo(actualHudGuiPoint.x).Within(0.001f));
            Assert.That(resolvedMarkerPoint.y, Is.EqualTo(actualHudGuiPoint.y).Within(0.001f));
        }
    }
}
