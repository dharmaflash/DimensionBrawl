using UnityEngine;

namespace DimensionBrawl.UI
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class LobbyCharacterToonRenderTuner : MonoBehaviour
    {
        private static readonly int UnlitIntensityId = Shader.PropertyToID("_Unlit_Intensity");
        private static readonly int RimLightPowerId = Shader.PropertyToID("_RimLight_Power");
        private static readonly int OutlineWidthId = Shader.PropertyToID("_Outline_Width");
        private static readonly int OutlineDepthOffsetId = Shader.PropertyToID("_Offset_Z");
        private static readonly int BaseColorStepId = Shader.PropertyToID("_BaseColor_Step");
        private static readonly int BaseShadeFeatherId = Shader.PropertyToID("_BaseShade_Feather");

        [SerializeField] private Transform targetRoot;
        [SerializeField, Min(0f)] private float unlitIntensity = 1f;
        [SerializeField, Min(0f)] private float rimLightPower = 0.18f;
        [SerializeField, Min(0f)] private float outlineWidth = 1.2f;
        [SerializeField, Min(0f)] private float outlineDepthOffset = 0.2f;
        [SerializeField, Range(0f, 1f)] private float baseColorStep = 0.78f;
        [SerializeField, Range(0f, 0.5f)] private float baseShadeFeather = 0.12f;
        [SerializeField] private bool clearOnDisable = true;

        private readonly MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();

        private void OnEnable()
        {
            Apply();
        }

        private void OnValidate()
        {
            Apply();
        }

        private void OnDisable()
        {
            if (clearOnDisable)
            {
                Clear();
            }
        }

        public void Apply()
        {
            if (targetRoot == null)
            {
                return;
            }

            Renderer[] renderers = targetRoot.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer targetRenderer in renderers)
            {
                ApplyToRenderer(targetRenderer);
            }
        }

        public void Clear()
        {
            if (targetRoot == null)
            {
                return;
            }

            Renderer[] renderers = targetRoot.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer targetRenderer in renderers)
            {
                if (targetRenderer == null)
                {
                    continue;
                }

                int materialCount = targetRenderer.sharedMaterials.Length;
                for (int materialIndex = 0; materialIndex < materialCount; materialIndex++)
                {
                    propertyBlock.Clear();
                    targetRenderer.SetPropertyBlock(propertyBlock, materialIndex);
                }
            }
        }

        private void ApplyToRenderer(Renderer targetRenderer)
        {
            if (targetRenderer == null)
            {
                return;
            }

            Material[] materials = targetRenderer.sharedMaterials;
            for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
            {
                Material material = materials[materialIndex];
                if (material == null)
                {
                    continue;
                }

                bool hasUnlitIntensity = material.HasProperty(UnlitIntensityId);
                bool hasRimLightPower = material.HasProperty(RimLightPowerId);
                bool hasOutlineWidth = material.HasProperty(OutlineWidthId);
                bool hasOutlineDepthOffset = material.HasProperty(OutlineDepthOffsetId);
                bool hasBaseColorStep = material.HasProperty(BaseColorStepId);
                bool hasBaseShadeFeather = material.HasProperty(BaseShadeFeatherId);
                if (!hasUnlitIntensity
                    && !hasRimLightPower
                    && !hasOutlineWidth
                    && !hasOutlineDepthOffset
                    && !hasBaseColorStep
                    && !hasBaseShadeFeather)
                {
                    continue;
                }

                targetRenderer.GetPropertyBlock(propertyBlock, materialIndex);
                if (hasUnlitIntensity)
                {
                    propertyBlock.SetFloat(UnlitIntensityId, unlitIntensity);
                }

                if (hasRimLightPower)
                {
                    propertyBlock.SetFloat(RimLightPowerId, rimLightPower);
                }

                if (hasOutlineWidth)
                {
                    propertyBlock.SetFloat(OutlineWidthId, outlineWidth);
                }

                if (hasOutlineDepthOffset)
                {
                    propertyBlock.SetFloat(OutlineDepthOffsetId, outlineDepthOffset);
                }

                if (hasBaseColorStep)
                {
                    propertyBlock.SetFloat(BaseColorStepId, baseColorStep);
                }

                if (hasBaseShadeFeather)
                {
                    propertyBlock.SetFloat(BaseShadeFeatherId, baseShadeFeather);
                }

                targetRenderer.SetPropertyBlock(propertyBlock, materialIndex);
                propertyBlock.Clear();
            }
        }
    }
}
