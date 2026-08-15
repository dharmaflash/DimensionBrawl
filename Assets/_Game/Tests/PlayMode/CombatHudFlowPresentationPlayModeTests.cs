using NUnit.Framework;
using UnityEngine;

namespace DimensionBrawl.Tests.PlayMode
{
    public sealed class CombatHudFlowPresentationPlayModeTests
    {
        private const string FlowShaderName = "DimensionBrawl/UI/CelestialFlow";

        [Test]
        public void SingleMapFlowShaderExposesUiDefaultAndMotionContract()
        {
            Shader shader = Shader.Find(FlowShaderName);
            Assert.That(shader, Is.Not.Null, $"Missing shader '{FlowShaderName}'.");

            Material material = new Material(shader);
            try
            {
                Assert.That(material.HasProperty("_MainTex"), Is.True);
                Assert.That(material.HasProperty("_Color"), Is.True);
                Assert.That(material.HasProperty("_FlowTex"), Is.True);
                Assert.That(material.HasProperty("_FlowTint"), Is.True);
                Assert.That(material.HasProperty("_FlowStrength"), Is.True);
                Assert.That(material.HasProperty("_FlowTiling"), Is.True);
                Assert.That(material.HasProperty("_FlowSpeed"), Is.True);
                Assert.That(material.HasProperty("_FlowPhase"), Is.True);
                Assert.That(material.HasProperty("_StencilComp"), Is.True);
                Assert.That(material.HasProperty("_Stencil"), Is.True);
                Assert.That(material.HasProperty("_StencilOp"), Is.True);
                Assert.That(material.HasProperty("_StencilWriteMask"), Is.True);
                Assert.That(material.HasProperty("_StencilReadMask"), Is.True);
                Assert.That(material.HasProperty("_ColorMask"), Is.True);
                Assert.That(material.HasProperty("_UseUIAlphaClip"), Is.True);
            }
            finally
            {
                Object.DestroyImmediate(material);
            }
        }

        [Test]
        public void SingleMapFlowShaderDoesNotCarryAtlasAnimationState()
        {
            Shader shader = Shader.Find(FlowShaderName);
            Assert.That(shader, Is.Not.Null, $"Missing shader '{FlowShaderName}'.");

            Material material = new Material(shader);
            try
            {
                Assert.That(material.HasProperty("_FlowAtlas"), Is.False);
                Assert.That(material.HasProperty("_AtlasColumns"), Is.False);
                Assert.That(material.HasProperty("_AtlasRows"), Is.False);
                Assert.That(material.HasProperty("_FrameCount"), Is.False);
                Assert.That(material.HasProperty("_FramesPerSecond"), Is.False);
                Assert.That(material.HasProperty("_PingPong"), Is.False);
                Assert.That(material.GetFloat("_FlowStrength"), Is.InRange(0f, 0.1f));
            }
            finally
            {
                Object.DestroyImmediate(material);
            }
        }
    }
}
