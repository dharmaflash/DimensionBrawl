using System.Collections;
using DimensionBrawl.Combat;
using UnityEngine;

namespace DimensionBrawl.Presentation
{
    public sealed class CombatHitFeedback : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        [Header("References")]
        [SerializeField] private CombatHealth health;
        [SerializeField] private Renderer[] flashRenderers;

        [Header("Flash")]
        [SerializeField] private bool renderHitFeedback;
        [SerializeField] private bool applyIdleColorOnEnable;
        [SerializeField] private Color idleColor = new Color(0.3f, 0.85f, 1f);
        [Tooltip("First-pass visible feedback value. The combat timing source only gives stagger/hit-stop ranges, so this remains Inspector-visible.")]
        [SerializeField, Min(0f)] private float flashSeconds = 0.12f;
        [SerializeField] private Color hitColor = new Color(1f, 0.36f, 0.18f, 1f);
        [SerializeField] private Color hitEmissionColor = new Color(1f, 0.7f, 0.28f, 1f);
        [SerializeField, Range(0f, 1f)] private float hitColorBlend = 0.72f;
        [SerializeField, Min(0f)] private float hitEmissionBoost = 1.35f;
        [SerializeField] private Color deathColor = new Color(0.15f, 0.15f, 0.15f);

        private MaterialPropertyBlock propertyBlock;
        private Coroutine flashRoutine;
        private int damageFlashCount;

        public bool RenderHitFeedback => renderHitFeedback;
        public int FlashRendererCount => flashRenderers != null ? flashRenderers.Length : 0;
        public int DamageFlashCount => damageFlashCount;

        private void Awake()
        {
            if (health == null)
            {
                health = GetComponent<CombatHealth>();
            }

            propertyBlock = new MaterialPropertyBlock();
        }

        private void OnEnable()
        {
            if (health == null || !renderHitFeedback)
            {
                return;
            }

            health.Damaged += HandleDamaged;
            health.Died += HandleDied;

            if (applyIdleColorOnEnable)
            {
                ApplyFlatColor(idleColor, Color.black);
            }
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.Damaged -= HandleDamaged;
                health.Died -= HandleDied;
            }

            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
                flashRoutine = null;
            }
        }

        private void HandleDamaged(DamageInfo damageInfo)
        {
            if (!renderHitFeedback)
            {
                return;
            }

            if (!DamageResponsePolicyUtility.PlaysDamagePresentation(damageInfo.ResponsePolicy))
            {
                return;
            }

            damageFlashCount++;
            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
            }

            flashRoutine = StartCoroutine(Flash(hitColor, hitEmissionColor, flashSeconds, clearAfter: true));
        }

        private void HandleDied()
        {
            if (!renderHitFeedback)
            {
                return;
            }

            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
            }

            ApplyFlatColor(deathColor, Color.black);
        }

        private IEnumerator Flash(Color color, Color emissionColor, float seconds, bool clearAfter)
        {
            float duration = Mathf.Max(0.001f, seconds);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float normalized = Mathf.Clamp01(1f - elapsed / duration);
                float weight = Mathf.SmoothStep(0f, 1f, normalized);
                ApplyFlash(color, emissionColor, weight);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            if (clearAfter)
            {
                if (applyIdleColorOnEnable)
                {
                    ApplyFlatColor(idleColor, Color.black);
                }
                else
                {
                    ClearColor();
                }
            }

            flashRoutine = null;
        }

        private void ApplyFlash(Color color, Color emissionColor, float weight)
        {
            if (flashRenderers == null)
            {
                return;
            }

            for (int i = 0; i < flashRenderers.Length; i++)
            {
                Renderer targetRenderer = flashRenderers[i];
                if (targetRenderer == null)
                {
                    continue;
                }

                propertyBlock ??= new MaterialPropertyBlock();
                targetRenderer.GetPropertyBlock(propertyBlock);
                Color baseColor = ResolveRendererBaseColor(targetRenderer);
                Color flashColor = Color.Lerp(baseColor, color, Mathf.Clamp01(hitColorBlend * weight));
                flashColor.a = baseColor.a;
                Color boostedEmission = emissionColor * (hitEmissionBoost * weight);
                boostedEmission.a = emissionColor.a;

                propertyBlock.SetColor(BaseColorId, flashColor);
                propertyBlock.SetColor(ColorId, flashColor);
                propertyBlock.SetColor(EmissionColorId, boostedEmission);
                targetRenderer.SetPropertyBlock(propertyBlock);
            }
        }

        private void ApplyFlatColor(Color color, Color emissionColor)
        {
            if (flashRenderers == null)
            {
                return;
            }

            for (int i = 0; i < flashRenderers.Length; i++)
            {
                Renderer targetRenderer = flashRenderers[i];
                if (targetRenderer == null)
                {
                    continue;
                }

                propertyBlock ??= new MaterialPropertyBlock();
                targetRenderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor(BaseColorId, color);
                propertyBlock.SetColor(ColorId, color);
                propertyBlock.SetColor(EmissionColorId, emissionColor);
                targetRenderer.SetPropertyBlock(propertyBlock);
            }
        }

        private static Color ResolveRendererBaseColor(Renderer targetRenderer)
        {
            Material[] materials = targetRenderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                Material material = materials[i];
                if (material == null)
                {
                    continue;
                }

                if (material.HasProperty(BaseColorId))
                {
                    return material.GetColor(BaseColorId);
                }

                if (material.HasProperty(ColorId))
                {
                    return material.GetColor(ColorId);
                }
            }

            return Color.white;
        }

        private void ClearColor()
        {
            if (flashRenderers == null)
            {
                return;
            }

            for (int i = 0; i < flashRenderers.Length; i++)
            {
                if (flashRenderers[i] != null)
                {
                    flashRenderers[i].SetPropertyBlock(null);
                }
            }
        }
    }
}
