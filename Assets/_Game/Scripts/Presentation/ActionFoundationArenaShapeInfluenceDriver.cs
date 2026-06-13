using UnityEngine;

namespace DimensionBrawl.Presentation
{
    [DisallowMultipleComponent]
    public sealed class ActionFoundationArenaShapeInfluenceDriver : MonoBehaviour
    {
        private static readonly int[] TargetIds =
        {
            Shader.PropertyToID("_target"),
            Shader.PropertyToID("_target2"),
            Shader.PropertyToID("_target3"),
            Shader.PropertyToID("_target4")
        };

        private static readonly int[] ActivateIds =
        {
            Shader.PropertyToID("_Activate_Target"),
            Shader.PropertyToID("_Activate_Target_2"),
            Shader.PropertyToID("_Activate_Target_3"),
            Shader.PropertyToID("_Activate_Target_4")
        };

        [SerializeField] private Renderer[] renderers;
        [SerializeField] private Transform[] influenceTargets;

        private MaterialPropertyBlock propertyBlock;

        public void Configure(Renderer[] targetRenderers, Transform[] targets)
        {
            renderers = targetRenderers;
            influenceTargets = targets;
            ApplyTargets();
        }

        private void Awake()
        {
            CacheRenderers();
        }

        private void OnEnable()
        {
            CacheRenderers();
            ApplyTargets();
        }

        private void LateUpdate()
        {
            ApplyTargets();
        }

        private void CacheRenderers()
        {
            if (renderers == null || renderers.Length == 0)
            {
                renderers = GetComponentsInChildren<Renderer>(includeInactive: true);
            }

            propertyBlock ??= new MaterialPropertyBlock();
        }

        private void ApplyTargets()
        {
            if (renderers == null || renderers.Length == 0)
            {
                return;
            }

            propertyBlock ??= new MaterialPropertyBlock();
            for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
            {
                Renderer targetRenderer = renderers[rendererIndex];
                if (targetRenderer == null)
                {
                    continue;
                }

                targetRenderer.GetPropertyBlock(propertyBlock);
                for (int i = 0; i < TargetIds.Length; i++)
                {
                    Transform influenceTarget = influenceTargets != null && i < influenceTargets.Length
                        ? influenceTargets[i]
                        : null;
                    bool active = influenceTarget != null;
                    propertyBlock.SetFloat(ActivateIds[i], active ? 1f : 0f);
                    propertyBlock.SetVector(TargetIds[i], active ? influenceTarget.position : Vector3.zero);
                }

                targetRenderer.SetPropertyBlock(propertyBlock);
            }
        }
    }
}
