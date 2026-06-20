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

        private MaterialPropertyBlock propertyBlock;

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
                    MaterialPropertyBlock block = PropertyBlock;
                    block.Clear();
                    targetRenderer.SetPropertyBlock(block, materialIndex);
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

                MaterialPropertyBlock block = PropertyBlock;
                targetRenderer.GetPropertyBlock(block, materialIndex);
                if (hasUnlitIntensity)
                {
                    block.SetFloat(UnlitIntensityId, unlitIntensity);
                }

                if (hasRimLightPower)
                {
                    block.SetFloat(RimLightPowerId, rimLightPower);
                }

                if (hasOutlineWidth)
                {
                    block.SetFloat(OutlineWidthId, outlineWidth);
                }

                if (hasOutlineDepthOffset)
                {
                    block.SetFloat(OutlineDepthOffsetId, outlineDepthOffset);
                }

                if (hasBaseColorStep)
                {
                    block.SetFloat(BaseColorStepId, baseColorStep);
                }

                if (hasBaseShadeFeather)
                {
                    block.SetFloat(BaseShadeFeatherId, baseShadeFeather);
                }

                targetRenderer.SetPropertyBlock(block, materialIndex);
                block.Clear();
            }
        }

        private MaterialPropertyBlock PropertyBlock
        {
            get
            {
                if (propertyBlock == null)
                {
                    propertyBlock = new MaterialPropertyBlock();
                }

                return propertyBlock;
            }
        }
    }
}
