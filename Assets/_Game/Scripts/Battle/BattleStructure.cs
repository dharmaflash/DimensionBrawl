using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace IsekaiBrawl.Gameplay
{
    public enum BattleStructureRole
    {
        FrontlineBlocker = 0,
        RewardObjective = 1,
        SiegeObjective = 2
    }

    [RequireComponent(typeof(Collider))]
    public class BattleStructure : MonoBehaviour
    {
        private static readonly List<BattleStructure> ActiveStructures = new();

        public static event System.Action<BattleStructure, float> OnStructureDamaged;
        public static event System.Action<BattleStructure, bool> OnStructureDestroyed;

        [SerializeField] private float maxHP = 80f;
        [SerializeField] private float energyReward = 30f;
        [SerializeField] private float destroyDelay = 0.2f;
        [SerializeField] private float hitFlashDuration = 0.08f;
        [SerializeField] private Color hitFlashColor = new(1f, 0.9f, 0.65f, 1f);
        [SerializeField] private Vector3 healthBarOffset = new(0f, 1.35f, 0f);
        [SerializeField] private BattleStructureRole structureRole = BattleStructureRole.FrontlineBlocker;

        private Collider cachedCollider;
        private Renderer[] cachedRenderers;
        private Color[] baseColors;
        private float currentHP;
        private bool isDestroyed;
        private Coroutine flashRoutine;
        private Transform healthBarRoot;
        private Transform healthBarFill;
        private Renderer healthBarFillRenderer;
        private Transform roleMarkerRoot;
        private Renderer roleMarkerBackRenderer;
        private Renderer roleMarkerPlateRenderer;
        private TextMeshPro roleMarkerText;

        public bool IsDestroyed => isDestroyed;
        public float CurrentHP => currentHP;
        public float MaxHP => maxHP;
        public float EnergyReward => energyReward;
        public BattleStructureRole Role => structureRole;

        private void OnEnable()
        {
            if (!ActiveStructures.Contains(this))
            {
                ActiveStructures.Add(this);
            }
        }

        private void OnDisable()
        {
            ActiveStructures.Remove(this);
        }

        private void Awake()
        {
            cachedCollider = GetComponent<Collider>();
            cachedRenderers = GetComponentsInChildren<Renderer>(true);
            baseColors = new Color[cachedRenderers.Length];
            for (int index = 0; index < cachedRenderers.Length; index++)
            {
                Renderer renderer = cachedRenderers[index];
                baseColors[index] = renderer != null && renderer.material.HasProperty("_Color")
                    ? renderer.material.color
                    : Color.white;
            }

            currentHP = maxHP;
            EnsureHealthBar();
            UpdateRoleMarker();
            UpdateHealthBar();
        }

        public void Configure(float newMaxHP, float newEnergyReward, BattleStructureRole newRole = BattleStructureRole.FrontlineBlocker)
        {
            maxHP = Mathf.Max(1f, newMaxHP);
            energyReward = Mathf.Max(0f, newEnergyReward);
            structureRole = newRole;
            currentHP = maxHP;
            isDestroyed = false;
            EnsureHealthBar();
            if (healthBarRoot != null)
            {
                healthBarRoot.gameObject.SetActive(true);
            }

            UpdateRoleMarker();
            UpdateHealthBar();
        }

        public static BattleStructure FindClosestActive(Vector3 position, float range)
        {
            return FindClosestActive(position, range, null);
        }

        public static BattleStructure FindClosestActive(Vector3 position, float range, BattleStructureRole? requiredRole)
        {
            float rangeSquared = range * range;
            float closestDistance = float.MaxValue;
            BattleStructure bestTarget = null;

            for (int index = 0; index < ActiveStructures.Count; index++)
            {
                BattleStructure structure = ActiveStructures[index];
                if (structure == null || structure.isDestroyed)
                {
                    continue;
                }

                if (requiredRole.HasValue && structure.structureRole != requiredRole.Value)
                {
                    continue;
                }

                float distanceSquared = (structure.transform.position - position).sqrMagnitude;
                if (distanceSquared > rangeSquared || distanceSquared >= closestDistance)
                {
                    continue;
                }

                closestDistance = distanceSquared;
                bestTarget = structure;
            }

            return bestTarget;
        }

        public static BattleStructure FindClosestActiveInLaneAnyRole(
            Vector3 position,
            float range,
            int laneIndex,
            bool isPlayerTeam,
            float backwardAllowance = 0.5f)
        {
            BattleManager battleManager = BattleManager.Instance;
            if (battleManager == null)
            {
                return FindClosestActive(position, range);
            }

            float rangeSquared = range * range;
            float closestDistance = float.MaxValue;
            BattleStructure bestTarget = null;
            int resolvedLaneIndex = BattleLaneUtility.ClampLaneIndex(laneIndex, battleManager.LaneCount);

            for (int index = 0; index < ActiveStructures.Count; index++)
            {
                BattleStructure structure = ActiveStructures[index];
                if (structure == null || structure.isDestroyed)
                {
                    continue;
                }

                if (battleManager.GetNearestLaneIndex(structure.transform.position.x) != resolvedLaneIndex)
                {
                    continue;
                }

                float signedForwardOffset = isPlayerTeam
                    ? structure.transform.position.z - position.z
                    : position.z - structure.transform.position.z;
                if (signedForwardOffset < -backwardAllowance)
                {
                    continue;
                }

                float distanceSquared = (structure.transform.position - position).sqrMagnitude;
                if (distanceSquared > rangeSquared || distanceSquared >= closestDistance)
                {
                    continue;
                }

                closestDistance = distanceSquared;
                bestTarget = structure;
            }

            return bestTarget;
        }

        public static BattleStructure FindClosestActiveInLane(
            Vector3 position,
            float range,
            int laneIndex,
            bool isPlayerTeam,
            BattleStructureRole requiredRole = BattleStructureRole.FrontlineBlocker,
            float backwardAllowance = 0.5f)
        {
            BattleManager battleManager = BattleManager.Instance;
            if (battleManager == null)
            {
                return FindClosestActive(position, range, requiredRole);
            }

            float rangeSquared = range * range;
            float closestDistance = float.MaxValue;
            BattleStructure bestTarget = null;
            int resolvedLaneIndex = BattleLaneUtility.ClampLaneIndex(laneIndex, battleManager.LaneCount);

            for (int index = 0; index < ActiveStructures.Count; index++)
            {
                BattleStructure structure = ActiveStructures[index];
                if (structure == null || structure.isDestroyed)
                {
                    continue;
                }

                if (structure.structureRole != requiredRole)
                {
                    continue;
                }

                if (battleManager.GetNearestLaneIndex(structure.transform.position.x) != resolvedLaneIndex)
                {
                    continue;
                }

                float signedForwardOffset = isPlayerTeam
                    ? structure.transform.position.z - position.z
                    : position.z - structure.transform.position.z;
                if (signedForwardOffset < -backwardAllowance)
                {
                    continue;
                }

                float distanceSquared = (structure.transform.position - position).sqrMagnitude;
                if (distanceSquared > rangeSquared || distanceSquared >= closestDistance)
                {
                    continue;
                }

                closestDistance = distanceSquared;
                bestTarget = structure;
            }

            return bestTarget;
        }

        public static BattleStructure FindNearestActiveInLaneAlongAdvance(int laneIndex, bool isPlayerTeam)
        {
            BattleManager battleManager = BattleManager.Instance;
            if (battleManager == null)
            {
                return null;
            }

            int resolvedLaneIndex = BattleLaneUtility.ClampLaneIndex(laneIndex, battleManager.LaneCount);
            BattleStructure bestTarget = null;
            float bestDepth = isPlayerTeam ? float.MaxValue : float.MinValue;

            for (int index = 0; index < ActiveStructures.Count; index++)
            {
                BattleStructure structure = ActiveStructures[index];
                if (structure == null || structure.isDestroyed)
                {
                    continue;
                }

                if (battleManager.GetNearestLaneIndex(structure.transform.position.x) != resolvedLaneIndex)
                {
                    continue;
                }

                float depth = structure.transform.position.z;
                if (isPlayerTeam)
                {
                    if (depth >= bestDepth)
                    {
                        continue;
                    }
                }
                else if (depth <= bestDepth)
                {
                    continue;
                }

                bestDepth = depth;
                bestTarget = structure;
            }

            return bestTarget;
        }

        public static BattleStructure FindNearestRoleInLaneAlongAdvance(
            int laneIndex,
            bool isPlayerTeam,
            BattleStructureRole requiredRole = BattleStructureRole.FrontlineBlocker)
        {
            BattleManager battleManager = BattleManager.Instance;
            if (battleManager == null)
            {
                return null;
            }

            int resolvedLaneIndex = BattleLaneUtility.ClampLaneIndex(laneIndex, battleManager.LaneCount);
            BattleStructure bestTarget = null;
            float bestDepth = isPlayerTeam ? float.MaxValue : float.MinValue;

            for (int index = 0; index < ActiveStructures.Count; index++)
            {
                BattleStructure structure = ActiveStructures[index];
                if (structure == null || structure.isDestroyed || structure.structureRole != requiredRole)
                {
                    continue;
                }

                if (battleManager.GetNearestLaneIndex(structure.transform.position.x) != resolvedLaneIndex)
                {
                    continue;
                }

                float depth = structure.transform.position.z;
                if (isPlayerTeam)
                {
                    if (depth >= bestDepth)
                    {
                        continue;
                    }
                }
                else if (depth <= bestDepth)
                {
                    continue;
                }

                bestDepth = depth;
                bestTarget = structure;
            }

            return bestTarget;
        }

        public void TakeDamage(float amount, bool causedByPlayerTeam)
        {
            if (amount <= 0f || isDestroyed)
            {
                return;
            }

            currentHP = Mathf.Max(0f, currentHP - amount);
            PlayHitFlash();
            UpdateHealthBar();
            BattlePresentationController.Instance?.ShowWorldText(transform.position + new Vector3(0f, 1.8f, 0f), $"-{Mathf.CeilToInt(amount)}", hitFlashColor, 3f, 0.52f);
            OnStructureDamaged?.Invoke(this, amount);

            if (currentHP > 0f)
            {
                return;
            }

            StartCoroutine(DestroyRoutine(causedByPlayerTeam));
        }

        private IEnumerator DestroyRoutine(bool causedByPlayerTeam)
        {
            if (isDestroyed)
            {
                yield break;
            }

            isDestroyed = true;
            if (cachedCollider != null)
            {
                cachedCollider.enabled = false;
            }

            if (healthBarRoot != null)
            {
                healthBarRoot.gameObject.SetActive(false);
            }

            if (causedByPlayerTeam)
            {
                BattleEnergySystem.Instance?.AddEnergy(energyReward);
            }

            OnStructureDestroyed?.Invoke(this, causedByPlayerTeam);

            Vector3 initialScale = transform.localScale;
            float elapsed = 0f;
            while (elapsed < destroyDelay)
            {
                elapsed += Time.deltaTime;
                float normalizedTime = destroyDelay <= 0.001f ? 1f : Mathf.Clamp01(elapsed / destroyDelay);
                transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, normalizedTime);
                yield return null;
            }

            Destroy(gameObject);
        }

        private void PlayHitFlash()
        {
            if (!gameObject.activeInHierarchy || cachedRenderers == null || cachedRenderers.Length == 0)
            {
                return;
            }

            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
            }

            flashRoutine = StartCoroutine(HitFlashRoutine());
        }

        private IEnumerator HitFlashRoutine()
        {
            SetRendererColors(hitFlashColor);
            yield return new WaitForSeconds(hitFlashDuration);
            RestoreBaseColors();
            flashRoutine = null;
        }

        private void SetRendererColors(Color color)
        {
            for (int index = 0; index < cachedRenderers.Length; index++)
            {
                Renderer renderer = cachedRenderers[index];
                if (renderer == null || !renderer.material.HasProperty("_Color"))
                {
                    continue;
                }

                renderer.material.color = color;
            }
        }

        private void RestoreBaseColors()
        {
            for (int index = 0; index < cachedRenderers.Length; index++)
            {
                Renderer renderer = cachedRenderers[index];
                if (renderer == null || !renderer.material.HasProperty("_Color"))
                {
                    continue;
                }

                renderer.material.color = baseColors[index];
            }
        }

        private void LateUpdate()
        {
            UpdateHealthBarFacing();
        }

        private void EnsureHealthBar()
        {
            if (healthBarRoot != null)
            {
                return;
            }

            GameObject rootObject = new("WorldHealthBar");
            rootObject.transform.SetParent(transform, false);
            rootObject.transform.localPosition = healthBarOffset;
            healthBarRoot = rootObject.transform;

            GameObject backObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            backObject.name = "Back";
            backObject.transform.SetParent(healthBarRoot, false);
            backObject.transform.localScale = new Vector3(1.05f, 0.11f, 0.05f);
            backObject.transform.localPosition = Vector3.zero;
            Destroy(backObject.GetComponent<Collider>());
            Renderer backRenderer = backObject.GetComponent<Renderer>();
            if (backRenderer != null)
            {
                backRenderer.material = new Material(Shader.Find("Sprites/Default"));
                backRenderer.material.color = new Color(0f, 0f, 0f, 0.55f);
            }

            GameObject fillObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fillObject.name = "Fill";
            fillObject.transform.SetParent(healthBarRoot, false);
            fillObject.transform.localScale = new Vector3(0.96f, 0.06f, 0.03f);
            fillObject.transform.localPosition = new Vector3(-0.02f, 0f, -0.01f);
            Destroy(fillObject.GetComponent<Collider>());
            healthBarFill = fillObject.transform;
            healthBarFillRenderer = fillObject.GetComponent<Renderer>();
            if (healthBarFillRenderer != null)
            {
                healthBarFillRenderer.material = new Material(Shader.Find("Sprites/Default"));
            }

            GameObject markerRootObject = new("RoleMarker");
            markerRootObject.transform.SetParent(healthBarRoot, false);
            markerRootObject.transform.localPosition = new Vector3(0f, 0.34f, -0.02f);
            roleMarkerRoot = markerRootObject.transform;

            GameObject plateBackObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plateBackObject.name = "PlateBack";
            plateBackObject.transform.SetParent(roleMarkerRoot, false);
            plateBackObject.transform.localScale = new Vector3(0.76f, 0.2f, 0.055f);
            plateBackObject.transform.localPosition = new Vector3(0f, 0f, 0.01f);
            Destroy(plateBackObject.GetComponent<Collider>());
            roleMarkerBackRenderer = plateBackObject.GetComponent<Renderer>();
            if (roleMarkerBackRenderer != null)
            {
                roleMarkerBackRenderer.material = new Material(Shader.Find("Sprites/Default"));
                roleMarkerBackRenderer.material.color = new Color(0f, 0f, 0f, 0.58f);
            }

            GameObject plateObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plateObject.name = "Plate";
            plateObject.transform.SetParent(roleMarkerRoot, false);
            plateObject.transform.localScale = new Vector3(0.7f, 0.16f, 0.05f);
            Destroy(plateObject.GetComponent<Collider>());
            roleMarkerPlateRenderer = plateObject.GetComponent<Renderer>();
            if (roleMarkerPlateRenderer != null)
            {
                roleMarkerPlateRenderer.material = new Material(Shader.Find("Sprites/Default"));
            }

            GameObject textObject = new("RoleText", typeof(TextMeshPro));
            textObject.transform.SetParent(roleMarkerRoot, false);
            textObject.transform.localPosition = new Vector3(0f, -0.012f, -0.04f);
            textObject.transform.localScale = Vector3.one * 0.19f;
            roleMarkerText = textObject.GetComponent<TextMeshPro>();
            if (roleMarkerText != null)
            {
                roleMarkerText.font = TMP_Settings.defaultFontAsset;
                roleMarkerText.fontSize = 5.6f;
                roleMarkerText.alignment = TextAlignmentOptions.Center;
                roleMarkerText.fontStyle = FontStyles.Bold;
                roleMarkerText.color = new Color(0.98f, 0.99f, 1f, 1f);
                roleMarkerText.raycastTarget = false;
                RuntimeUIFontUtility.ApplyToText(roleMarkerText);
            }
        }

        private void UpdateRoleMarker()
        {
            if (roleMarkerRoot == null)
            {
                return;
            }

            string label = structureRole switch
            {
                BattleStructureRole.RewardObjective => "\uBCF4\uC0C1",
                BattleStructureRole.SiegeObjective => "\uB3CC\uD30C",
                _ => "\uCC28\uB2E8"
            };
            Color plateColor = structureRole switch
            {
                BattleStructureRole.RewardObjective => new Color(0.18f, 0.78f, 0.46f, 1f),
                BattleStructureRole.SiegeObjective => new Color(0.9f, 0.42f, 0.2f, 1f),
                _ => new Color(0.92f, 0.66f, 0.18f, 1f)
            };
            Vector3 plateScale = structureRole switch
            {
                BattleStructureRole.RewardObjective => new Vector3(0.56f, 0.16f, 0.05f),
                BattleStructureRole.SiegeObjective => new Vector3(0.74f, 0.18f, 0.05f),
                _ => new Vector3(0.66f, 0.16f, 0.05f)
            };

            roleMarkerRoot.localPosition = structureRole switch
            {
                BattleStructureRole.SiegeObjective => new Vector3(0f, 0.38f, -0.02f),
                _ => new Vector3(0f, 0.31f, -0.02f)
            };

            if (roleMarkerBackRenderer != null)
            {
                Vector3 backScale = plateScale;
                backScale.x += 0.08f;
                backScale.y += 0.04f;
                roleMarkerBackRenderer.transform.localScale = backScale;
            }

            if (roleMarkerPlateRenderer != null)
            {
                roleMarkerPlateRenderer.transform.localScale = plateScale;
                roleMarkerPlateRenderer.material.color = plateColor;
            }

            if (roleMarkerText != null)
            {
                roleMarkerText.text = label;
                RuntimeUIFontUtility.ApplyToText(roleMarkerText);
            }
        }

        private void UpdateHealthBar()
        {
            if (healthBarFill == null)
            {
                return;
            }

            float normalized = maxHP <= 0.001f ? 0f : Mathf.Clamp01(currentHP / maxHP);
            healthBarFill.localScale = new Vector3(Mathf.Max(0.04f, 0.96f * normalized), 0.06f, 0.03f);
            healthBarFill.localPosition = new Vector3((-0.48f) + (0.48f * normalized), 0f, -0.01f);
            if (healthBarFillRenderer != null)
            {
                healthBarFillRenderer.material.color = Color.Lerp(new Color(1f, 0.36f, 0.3f, 1f), new Color(0.35f, 0.96f, 0.55f, 1f), normalized);
            }
        }

        private void UpdateHealthBarFacing()
        {
            if (healthBarRoot == null || Camera.main == null)
            {
                return;
            }

            healthBarRoot.forward = Camera.main.transform.forward;
        }
    }
}
