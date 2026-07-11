using DimensionBrawl.Core;
using NUnit.Framework;

namespace DimensionBrawl.Tests
{
    public sealed class MobilePerformanceGovernorPlayModeTests
    {
        [Test]
        public void InitialTierSelectionProtectsConstrainedDevices()
        {
            Assert.That(
                MobilePerformanceGovernor.SelectInitialTier(4096, 2048, 8, 2400, 50),
                Is.EqualTo(MobilePerformanceTier.Low));
            Assert.That(
                MobilePerformanceGovernor.SelectInitialTier(6144, 1024, 8, 2400, 50),
                Is.EqualTo(MobilePerformanceTier.Low));
            Assert.That(
                MobilePerformanceGovernor.SelectInitialTier(6144, 2048, 4, 2600, 50),
                Is.EqualTo(MobilePerformanceTier.Low));
            Assert.That(
                MobilePerformanceGovernor.SelectInitialTier(6144, 2048, 8, 2200, 40),
                Is.EqualTo(MobilePerformanceTier.Low));
        }

        [Test]
        public void InitialTierSelectionRequiresClearHighEndHeadroom()
        {
            Assert.That(
                MobilePerformanceGovernor.SelectInitialTier(6144, 2048, 8, 2400, 50),
                Is.EqualTo(MobilePerformanceTier.Balanced));
            Assert.That(
                MobilePerformanceGovernor.SelectInitialTier(8192, 0, 8, 2400, 50),
                Is.EqualTo(MobilePerformanceTier.High));
            Assert.That(
                MobilePerformanceGovernor.SelectInitialTier(12288, 4096, 8, 2600, 50),
                Is.EqualTo(MobilePerformanceTier.High));
        }

        [Test]
        public void TierProfilesIncreaseVisualBudgetsMonotonically()
        {
            MobilePerformanceProfile low = MobilePerformanceGovernor.GetProfile(MobilePerformanceTier.Low);
            MobilePerformanceProfile balanced = MobilePerformanceGovernor.GetProfile(MobilePerformanceTier.Balanced);
            MobilePerformanceProfile high = MobilePerformanceGovernor.GetProfile(MobilePerformanceTier.High);

            Assert.That(low.TargetFrameRate, Is.EqualTo(30));
            Assert.That(balanced.TargetFrameRate, Is.EqualTo(60));
            Assert.That(high.TargetFrameRate, Is.EqualTo(60));
            Assert.That(low.RenderScale, Is.LessThan(balanced.RenderScale));
            Assert.That(balanced.RenderScale, Is.LessThan(high.RenderScale));
            Assert.That(low.LodBias, Is.LessThan(balanced.LodBias));
            Assert.That(balanced.LodBias, Is.LessThan(high.LodBias));
            Assert.That(low.ShadowDistance, Is.LessThan(balanced.ShadowDistance));
            Assert.That(balanced.ShadowDistance, Is.LessThanOrEqualTo(high.ShadowDistance));
            Assert.That(low.MaxAdditionalLights, Is.LessThan(balanced.MaxAdditionalLights));
            Assert.That(balanced.MaxAdditionalLights, Is.LessThan(high.MaxAdditionalLights));
            Assert.That(low.StreamingMipmapsMemoryBudget, Is.LessThan(balanced.StreamingMipmapsMemoryBudget));
            Assert.That(balanced.StreamingMipmapsMemoryBudget, Is.LessThan(high.StreamingMipmapsMemoryBudget));
        }

        [Test]
        public void EveryTierStaysInsideMobileSafetyBounds()
        {
            foreach (MobilePerformanceTier tier in System.Enum.GetValues(typeof(MobilePerformanceTier)))
            {
                MobilePerformanceProfile profile = MobilePerformanceGovernor.GetProfile(tier);
                Assert.That(profile.MinimumRenderScale, Is.InRange(0.6f, profile.RenderScale));
                Assert.That(profile.RenderScale, Is.InRange(0.6f, 1f));
                Assert.That(profile.ShadowDistance, Is.InRange(20f, 50f));
                Assert.That(profile.MaxAdditionalLights, Is.InRange(2, 4));
                Assert.That(profile.StreamingMipmapsMemoryBudget, Is.InRange(128f, 384f));
                Assert.That(profile.StreamingMipmapsRenderersPerFrame, Is.InRange(32, 256));
                Assert.That(profile.StreamingMipmapsMaxFileIoRequests, Is.InRange(32, 256));
            }
        }
    }
}
