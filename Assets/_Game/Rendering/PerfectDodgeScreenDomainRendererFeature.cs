using DimensionBrawl.Presentation;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

namespace DimensionBrawl.Rendering
{
    public sealed class PerfectDodgeScreenDomainRendererFeature : ScriptableRendererFeature
    {
        [SerializeField] private Material passMaterial;
        [SerializeField] private RenderPassEvent injectionPoint = RenderPassEvent.BeforeRenderingPostProcessing;

        private PerfectDodgeScreenDomainPass pass;
        private Material runtimePassMaterial;

        public void SetPassMaterial(Material material)
        {
            passMaterial = material;
            RebuildRuntimePassMaterial();
            pass?.Setup(runtimePassMaterial, injectionPoint);
        }

        public override void Create()
        {
            pass ??= new PerfectDodgeScreenDomainPass();
            RebuildRuntimePassMaterial();
            pass.Setup(runtimePassMaterial, injectionPoint);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.cameraType != CameraType.Game)
            {
                return;
            }

            if (runtimePassMaterial == null || !PerfectDodgeScreenDomainRuntime.HasActiveCue)
            {
                return;
            }

            pass.Setup(runtimePassMaterial, injectionPoint);
            renderer.EnqueuePass(pass);
        }

        protected override void Dispose(bool disposing)
        {
            pass = null;
            CoreUtils.Destroy(runtimePassMaterial);
            runtimePassMaterial = null;
        }

        private void RebuildRuntimePassMaterial()
        {
            CoreUtils.Destroy(runtimePassMaterial);
            runtimePassMaterial = null;
            if (passMaterial == null)
            {
                return;
            }

            runtimePassMaterial = new Material(passMaterial)
            {
                name = $"{passMaterial.name} (Runtime)",
                hideFlags = HideFlags.HideAndDontSave
            };
        }

        private sealed class PerfectDodgeScreenDomainPass : ScriptableRenderPass
        {
            private const string PassName = "PerfectDodgeScreenDomain";
            private Material material;

            public void Setup(Material passMaterial, RenderPassEvent passEvent)
            {
                material = passMaterial;
                renderPassEvent = passEvent;
            }

#if URP_COMPATIBILITY_MODE
#pragma warning disable 618, 672
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                if (material == null || !PerfectDodgeScreenDomainRuntime.HasActiveCue)
                {
                    return;
                }

                RTHandle source = renderingData.cameraData.renderer.cameraColorTargetHandle;
                if (source == null)
                {
                    return;
                }

                RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
                descriptor.depthBufferBits = 0;
                descriptor.msaaSamples = 1;
                RenderingUtils.ReAllocateIfNeeded(
                    ref compatibilityTarget,
                    descriptor,
                    FilterMode.Bilinear,
                    TextureWrapMode.Clamp,
                    name: "_PerfectDodgeScreenDomainColor");

                PerfectDodgeScreenDomainRuntime.ApplyToMaterial(material, descriptor.width, descriptor.height);
                CommandBuffer cmd = CommandBufferPool.Get(PassName);
                Blitter.BlitCameraTexture(cmd, source, compatibilityTarget, material, 0);
                Blitter.BlitCameraTexture(cmd, compatibilityTarget, source);
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                CommandBufferPool.Release(cmd);
            }

            private RTHandle compatibilityTarget;
#pragma warning restore 618, 672
#endif

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                if (material == null || !PerfectDodgeScreenDomainRuntime.HasActiveCue)
                {
                    return;
                }

                UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
                UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
                if (cameraData.camera.cameraType != CameraType.Game)
                {
                    return;
                }

                TextureHandle source = resourceData.activeColorTexture;
                if (!source.IsValid())
                {
                    return;
                }

                RenderTextureDescriptor descriptor = cameraData.cameraTargetDescriptor;
                descriptor.depthBufferBits = 0;
                descriptor.msaaSamples = 1;
                TextureHandle destination = UniversalRenderer.CreateRenderGraphTexture(
                    renderGraph,
                    descriptor,
                    "_PerfectDodgeScreenDomainColor",
                    false,
                    FilterMode.Bilinear);

                if (!destination.IsValid())
                {
                    return;
                }

                PerfectDodgeScreenDomainRuntime.ApplyToMaterial(material, descriptor.width, descriptor.height);
                RenderGraphUtils.BlitMaterialParameters parameters = new(source, destination, material, 0);
                renderGraph.AddBlitPass(parameters, PassName);
                resourceData.cameraColor = destination;
            }
        }
    }
}
