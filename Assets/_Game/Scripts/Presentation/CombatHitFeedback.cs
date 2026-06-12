using System.Collections;
using DimensionBrawl.Combat;
using UnityEngine;

namespace DimensionBrawl.Presentation
{
    public sealed class CombatHitFeedback : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CombatHealth health;
        [SerializeField] private Renderer[] flashRenderers;

        [Header("Flash")]
        [SerializeField] private bool applyIdleColorOnEnable = true;
        [SerializeField] private Color idleColor = new Color(0.3f, 0.85f, 1f);
        [Tooltip("First-pass visible feedback value. The combat timing source only gives stagger/hit-stop ranges, so this remains Inspector-visible.")]
        [SerializeField, Min(0f)] private float flashSeconds = 0.08f;
        [SerializeField] private Color hitColor = Color.white;
        [SerializeField] private Color deathColor = new Color(0.15f, 0.15f, 0.15f);

        [Header("Hit Stop")]
        [Tooltip("DamageInfo.HitStopSeconds is sourced from collected hit-stop ranges: neutral 0.02-0.04, empowered 0.04-0.07.")]
        [SerializeField] private bool useDamageHitStop = true;
        [SerializeField, Range(0f, 1f)] private float hitStopTimeScale = 0.04f;

        private static float hitStopUntilRealtime;
        private static Coroutine activeHitStop;
        private static float originalFixedDeltaTime;

        private MaterialPropertyBlock propertyBlock;
        private Coroutine flashRoutine;

        private void Awake()
        {
            if (health == null)
            {
                health = GetComponent<CombatHealth>();
            }

            propertyBlock = new MaterialPropertyBlock();
            originalFixedDeltaTime = Time.fixedDeltaTime;
        }

        private void OnEnable()
        {
            if (health == null)
            {
                return;
            }

            health.Damaged += HandleDamaged;
            health.Died += HandleDied;

            if (applyIdleColorOnEnable)
            {
                ApplyColor(idleColor);
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
            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
            }

            flashRoutine = StartCoroutine(Flash(hitColor, flashSeconds, clearAfter: true));

            if (useDamageHitStop && damageInfo.HitStopSeconds > 0f)
            {
                RequestHitStop(damageInfo.HitStopSeconds);
            }
        }

        private void HandleDied()
        {
            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
            }

            ApplyColor(deathColor);
        }

        private IEnumerator Flash(Color color, float seconds, bool clearAfter)
        {
            ApplyColor(color);
            yield return new WaitForSecondsRealtime(seconds);

            if (clearAfter)
            {
                if (applyIdleColorOnEnable)
                {
                    ApplyColor(idleColor);
                }
                else
                {
                    ClearColor();
                }
            }

            flashRoutine = null;
        }

        private void RequestHitStop(float seconds)
        {
            hitStopUntilRealtime = Mathf.Max(hitStopUntilRealtime, Time.realtimeSinceStartup + seconds);

            if (activeHitStop == null)
            {
                activeHitStop = StartCoroutine(HitStopRoutine());
            }
        }

        private IEnumerator HitStopRoutine()
        {
            Time.timeScale = hitStopTimeScale;
            Time.fixedDeltaTime = originalFixedDeltaTime * hitStopTimeScale;

            while (Time.realtimeSinceStartup < hitStopUntilRealtime)
            {
                yield return null;
            }

            Time.timeScale = 1f;
            Time.fixedDeltaTime = originalFixedDeltaTime;
            activeHitStop = null;
        }

        private void ApplyColor(Color color)
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
                propertyBlock.SetColor("_BaseColor", color);
                propertyBlock.SetColor("_Color", color);
                targetRenderer.SetPropertyBlock(propertyBlock);
            }
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
