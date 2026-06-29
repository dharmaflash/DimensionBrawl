using DimensionBrawl.Combat;
using DimensionBrawl.Player;
using UnityEngine;

namespace DimensionBrawl.Presentation
{
    public sealed class SupportSummonPresentationCueDriver : MonoBehaviour
    {
        [SerializeField] private PlayerSupportSummonSlotAction summonAction;
        [SerializeField] private CombatVfxCuePlayer cuePlayer;
        [SerializeField] private Transform fallbackAnchor;
        [SerializeField] private string requiredSlotActionName = "SummonSlot2";
        [SerializeField] private string requiredActorRoleId = "BacklineMarksman";
        [SerializeField] private CombatVfxCueId summonUseCueId = CombatVfxCueId.SummonSlot2BeamLock;
        [SerializeField, Min(0f)] private float summonUseCueIntensity = 0.82f;
        [SerializeField, Min(0f)] private float summonUseCueAudioIntensity = 0.48f;
        [SerializeField, Min(0f)] private float summonUseTierIntensityStep = 0.10f;
        [SerializeField] private CombatVfxCueId volleyCueId = CombatVfxCueId.SummonSlot2BeamFire;
        [SerializeField, Min(0f)] private float volleyCueIntensity = 0.92f;
        [SerializeField, Min(0f)] private float volleyCueAudioIntensity = 0.66f;
        [SerializeField, Min(0f)] private float volleyTierIntensityStep = 0.11f;
        [SerializeField, Min(0f)] private float volleyProjectileIntensityStep = 0.045f;
        [SerializeField] private CombatVfxCueId impactCueId = CombatVfxCueId.SummonSlot2BeamHit;
        [SerializeField, Min(0f)] private float impactCueIntensity = 0.94f;
        [SerializeField, Min(0f)] private float impactCueAudioIntensity = 0.58f;
        [SerializeField, Min(0f)] private float impactTierIntensityStep = 0.09f;

        private bool subscribed;
        private int summonUseCueRequestCount;
        private int volleyCueRequestCount;
        private int impactCueRequestCount;

        public PlayerSupportSummonSlotAction SummonAction => summonAction;
        public CombatVfxCuePlayer CuePlayer => cuePlayer;
        public Transform FallbackAnchor => fallbackAnchor;
        public CombatVfxCueId SummonUseCueId => summonUseCueId;
        public CombatVfxCueId VolleyCueId => volleyCueId;
        public CombatVfxCueId ImpactCueId => impactCueId;
        public int SummonUseCueRequestCount => summonUseCueRequestCount;
        public int VolleyCueRequestCount => volleyCueRequestCount;
        public int ImpactCueRequestCount => impactCueRequestCount;

        private void Awake()
        {
            if (cuePlayer == null)
            {
                cuePlayer = GetComponent<CombatVfxCuePlayer>();
            }

            if (summonAction == null)
            {
                summonAction = FindConfiguredSupportAction();
            }
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        public void Configure(
            PlayerSupportSummonSlotAction newSummonAction,
            CombatVfxCuePlayer newCuePlayer,
            Transform newFallbackAnchor)
        {
            Unsubscribe();
            summonAction = newSummonAction;
            cuePlayer = newCuePlayer;
            fallbackAnchor = newFallbackAnchor;

            if (isActiveAndEnabled)
            {
                Subscribe();
            }
        }

        private void Subscribe()
        {
            if (subscribed || summonAction == null)
            {
                return;
            }

            summonAction.SummonUsed += HandleSummonUsed;
            summonAction.SummonVolleyFired += HandleSummonVolleyFired;
            summonAction.SummonProjectileDamageApplied += HandleSummonProjectileDamageApplied;
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed || summonAction == null)
            {
                subscribed = false;
                return;
            }

            summonAction.SummonUsed -= HandleSummonUsed;
            summonAction.SummonVolleyFired -= HandleSummonVolleyFired;
            summonAction.SummonProjectileDamageApplied -= HandleSummonProjectileDamageApplied;
            subscribed = false;
        }

        private void HandleSummonUsed(PlayerSupportSummonSlotAction action, int tier)
        {
            if (cuePlayer == null || !MatchesConfiguredRead(action, action != null ? action.LastSummonActorRoleId : string.Empty))
            {
                return;
            }

            Transform anchor = ResolveFallbackAnchor(action);
            Vector3 direction = ResolveDirection(
                action != null ? action.transform.forward : transform.forward,
                anchor != null ? anchor.forward : transform.forward,
                transform.forward);
            float intensity = summonUseCueIntensity + Mathf.Max(0, tier - 1) * summonUseTierIntensityStep;

            if (cuePlayer.PlayCue(summonUseCueId, anchor, direction, intensity, summonUseCueAudioIntensity))
            {
                summonUseCueRequestCount++;
            }
        }

        private void HandleSummonVolleyFired(
            PlayerSupportSummonSlotAction action,
            SupportSummonVolleyPresentationEvent volleyEvent)
        {
            if (cuePlayer == null || !MatchesConfiguredRead(action, volleyEvent.ActorRoleId))
            {
                return;
            }

            Transform anchor = volleyEvent.SourceAnchor != null
                ? volleyEvent.SourceAnchor
                : ResolveFallbackAnchor(action);
            Vector3 direction = ResolveDirection(
                volleyEvent.PlanarDirection,
                volleyEvent.TargetPosition - volleyEvent.SourcePosition,
                action != null ? action.transform.forward : transform.forward);
            float intensity = volleyCueIntensity
                + Mathf.Max(0, volleyEvent.Tier - 1) * volleyTierIntensityStep
                + Mathf.Max(0, volleyEvent.ProjectileCount - 1) * volleyProjectileIntensityStep;

            if (cuePlayer.PlayCue(volleyCueId, anchor, direction, intensity, volleyCueAudioIntensity))
            {
                volleyCueRequestCount++;
            }
        }

        private void HandleSummonProjectileDamageApplied(
            PlayerSupportSummonSlotAction action,
            LaneActionProjectile projectile,
            CombatHealth targetHealth,
            Vector3 impactPoint,
            Vector3 impactDirection)
        {
            if (cuePlayer == null || !MatchesConfiguredRead(action, action != null ? action.LastSummonActorRoleId : string.Empty))
            {
                return;
            }

            Transform anchor = projectile != null ? projectile.transform : ResolveFallbackAnchor(action);
            Vector3 direction = ResolveDirection(
                impactDirection,
                anchor != null ? impactPoint - anchor.position : impactDirection,
                action != null ? action.transform.forward : transform.forward);
            float intensity = impactCueIntensity + Mathf.Max(0, action.LastSpentTier - 1) * impactTierIntensityStep;

            if (cuePlayer.PlayCue(impactCueId, anchor, direction, intensity, impactCueAudioIntensity))
            {
                impactCueRequestCount++;
            }
        }

        private bool MatchesConfiguredRead(PlayerSupportSummonSlotAction action, string actorRoleId)
        {
            if (action == null || action != summonAction)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(requiredSlotActionName)
                && !string.Equals(action.SlotActionName, requiredSlotActionName, System.StringComparison.Ordinal))
            {
                return false;
            }

            return string.IsNullOrWhiteSpace(requiredActorRoleId)
                || string.Equals(actorRoleId, requiredActorRoleId, System.StringComparison.Ordinal);
        }

        private Transform ResolveFallbackAnchor(PlayerSupportSummonSlotAction action)
        {
            if (fallbackAnchor != null)
            {
                return fallbackAnchor;
            }

            return action != null ? action.transform : transform;
        }

        private PlayerSupportSummonSlotAction FindConfiguredSupportAction()
        {
            PlayerSupportSummonSlotAction[] actions = GetComponents<PlayerSupportSummonSlotAction>();
            for (int i = 0; i < actions.Length; i++)
            {
                if (actions[i] != null
                    && string.Equals(actions[i].SlotActionName, requiredSlotActionName, System.StringComparison.Ordinal))
                {
                    return actions[i];
                }
            }

            return actions.Length > 0 ? actions[0] : null;
        }

        private static Vector3 ResolveDirection(Vector3 preferred, Vector3 fallback, Vector3 lastFallback)
        {
            Vector3 planar = Vector3.ProjectOnPlane(preferred, Vector3.up);
            if (planar.sqrMagnitude > 0.0001f)
            {
                return planar.normalized;
            }

            planar = Vector3.ProjectOnPlane(fallback, Vector3.up);
            if (planar.sqrMagnitude > 0.0001f)
            {
                return planar.normalized;
            }

            planar = Vector3.ProjectOnPlane(lastFallback, Vector3.up);
            return planar.sqrMagnitude > 0.0001f ? planar.normalized : Vector3.forward;
        }
    }
}
