using DimensionBrawl.Combat;
using DimensionBrawl.Player;
using UnityEngine;

namespace DimensionBrawl.Presentation
{
    [DisallowMultipleComponent]
    public sealed class PlayerCombatVfxCueDriver : MonoBehaviour
    {
        private const float PerfectDodgeBlockVfxRepeatIntervalSeconds = 0.16f;

        [SerializeField] private PlayerActionController actionController;
        [SerializeField] private CombatHealth playerHealth;
        [SerializeField] private CombatVfxCuePlayer cuePlayer;
        [SerializeField] private PerfectDodgeVfxDirector perfectDodgeVfxDirector;
        [SerializeField] private Transform attackAnchor;
        [SerializeField] private Transform dodgeAnchor;
        [SerializeField] private Transform damageAnchor;
        [SerializeField] private CombatVfxCueId damagedCueId = CombatVfxCueId.PlayerDamaged;
        [SerializeField] private CombatVfxCueId criticalCueId = CombatVfxCueId.PlayerCritical;
        [SerializeField, Min(0f)] private float damagedCueIntensity = 1f;
        [SerializeField, Min(0f)] private float perfectDodgeCueIntensity = 1.55f;
        [SerializeField] private CombatVfxCueId perfectDodgeTimeFieldCueId = CombatVfxCueId.PlayerPerfectDodgeTimeField;
        [SerializeField] private CombatVfxCueId perfectDodgePulsewaveCueId = CombatVfxCueId.PlayerPerfectDodgePulsewave;
        [SerializeField] private CombatVfxCueId perfectDodgeHoloCubeCueId = CombatVfxCueId.PlayerPerfectDodgeHoloCube;
        [SerializeField] private CombatVfxCueId perfectDodgeWindowCueId = CombatVfxCueId.PlayerPerfectDodgeWindow;
        [SerializeField] private CombatVfxCueId perfectDodgeProjectileBlockCueId = CombatVfxCueId.PlayerPerfectDodgeShieldBlockImpact;
        [SerializeField, Min(0f)] private float perfectDodgeTimeFieldIntensity = 1f;
        [SerializeField, Min(0f)] private float perfectDodgePulsewaveIntensity = 1.12f;
        [SerializeField, Min(0f)] private float perfectDodgeHoloCubeIntensity = 0.92f;
        [SerializeField, Min(0f)] private float perfectDodgeWindowIntensity = 1f;
        [SerializeField, Min(0f)] private float perfectDodgeProjectileBlockIntensity = 1.18f;
        [SerializeField, Min(0.05f)] private float perfectDodgeShieldBlockRadius = 0.86f;
        [SerializeField, Min(0f)] private float perfectDodgeAudioIntensity = 1f;
        [SerializeField, Range(0.1f, 1f)] private float pressureDamageCueScale = 0.62f;
        [SerializeField, Range(0.05f, 0.95f)] private float criticalHealthRatio = 0.35f;
        [SerializeField, Min(0f)] private float criticalCueIntensity = 1.18f;
        [SerializeField] private bool playDamageVfx;
        [SerializeField] private bool playCriticalVfx;
        [SerializeField] private bool playPerfectDodgeProjectileBlockVfx = true;

        private bool actionSubscribed;
        private bool healthSubscribed;
        private bool criticalCuePlayed;
        private int damageVfxCueRequestCount;
        private int criticalVfxCueRequestCount;
        private int perfectDodgeProjectileBlockCueRequestCount;
        private float lastDamageCueIntensity;
        private float lastDamageCuePolicyScale = 1f;
        private DamageResponsePolicy lastDamageResponsePolicy = DamageResponsePolicy.Default;
        private CombatControlLockPolicy lastDamageControlLockPolicy = CombatControlLockPolicy.InterruptAction;
        private bool lastDamageCueInterruptedAction;
        private float lastPerfectDodgeBlockVfxTime = float.NegativeInfinity;
        private Vector3 lastPerfectDodgeBlockVfxPoint;

        public CombatHealth PlayerHealth => playerHealth;
        public Transform DamageAnchor => damageAnchor != null ? damageAnchor : attackAnchor;
        public CombatVfxCueId DamagedCueId => damagedCueId;
        public CombatVfxCueId CriticalCueId => criticalCueId;
        public float PerfectDodgeCueIntensity => perfectDodgeCueIntensity;
        public CombatVfxCueId PerfectDodgeTimeFieldCueId => perfectDodgeTimeFieldCueId;
        public CombatVfxCueId PerfectDodgePulsewaveCueId => perfectDodgePulsewaveCueId;
        public CombatVfxCueId PerfectDodgeHoloCubeCueId => perfectDodgeHoloCubeCueId;
        public CombatVfxCueId PerfectDodgeWindowCueId => perfectDodgeWindowCueId;
        public CombatVfxCueId PerfectDodgeProjectileBlockCueId => perfectDodgeProjectileBlockCueId;
        public float PerfectDodgeTimeFieldIntensity => perfectDodgeTimeFieldIntensity;
        public float PerfectDodgePulsewaveIntensity => perfectDodgePulsewaveIntensity;
        public float PerfectDodgeHoloCubeIntensity => perfectDodgeHoloCubeIntensity;
        public float PerfectDodgeWindowIntensity => perfectDodgeWindowIntensity;
        public float PerfectDodgeProjectileBlockIntensity => perfectDodgeProjectileBlockIntensity;
        public float PerfectDodgeShieldBlockRadius => perfectDodgeShieldBlockRadius;
        public float PerfectDodgeAudioIntensity => perfectDodgeAudioIntensity;
        public PerfectDodgeVfxDirector PerfectDodgeVfxDirector => perfectDodgeVfxDirector;
        public int DamageVfxCueRequestCount => damageVfxCueRequestCount;
        public int CriticalVfxCueRequestCount => criticalVfxCueRequestCount;
        public int PerfectDodgeProjectileBlockCueRequestCount => perfectDodgeProjectileBlockCueRequestCount;
        public float PressureDamageCueScale => pressureDamageCueScale;
        public bool PlayDamageVfx => playDamageVfx;
        public bool PlayCriticalVfx => playCriticalVfx;
        public bool PlayPerfectDodgeProjectileBlockVfx => playPerfectDodgeProjectileBlockVfx;
        public float LastDamageCueIntensity => lastDamageCueIntensity;
        public float LastDamageCuePolicyScale => lastDamageCuePolicyScale;
        public DamageResponsePolicy LastDamageResponsePolicy => lastDamageResponsePolicy;
        public CombatControlLockPolicy LastDamageControlLockPolicy => lastDamageControlLockPolicy;
        public bool LastDamageCueInterruptedAction => lastDamageCueInterruptedAction;

        public void ConfigureDamageFeedback(CombatHealth newPlayerHealth, Transform newDamageAnchor)
        {
            UnsubscribeHealth();
            playerHealth = newPlayerHealth;
            damageAnchor = newDamageAnchor;
            SubscribeHealth();
        }

        private void Awake()
        {
            if (actionController == null)
            {
                actionController = GetComponent<PlayerActionController>();
            }

            if (playerHealth == null)
            {
                playerHealth = GetComponent<CombatHealth>();
            }

            if (cuePlayer == null)
            {
                cuePlayer = GetComponent<CombatVfxCuePlayer>();
            }

            if (perfectDodgeVfxDirector == null)
            {
                perfectDodgeVfxDirector = GetComponent<PerfectDodgeVfxDirector>();
            }

            if (perfectDodgeVfxDirector == null)
            {
                perfectDodgeVfxDirector = gameObject.AddComponent<PerfectDodgeVfxDirector>();
            }

            perfectDodgeVfxDirector.Configure(actionController, playerHealth);
        }

        private void OnEnable()
        {
            SubscribeActions();
            SubscribeHealth();
        }

        private void OnDisable()
        {
            UnsubscribeActions();
            UnsubscribeHealth();
        }

        private void SubscribeActions()
        {
            if (actionSubscribed || actionController == null)
            {
                return;
            }

            actionController.BasicAttackStarted += HandleBasicAttackStarted;
            actionController.BasicAttackHit += HandleBasicAttackHit;
            actionController.DodgeStarted += HandleDodgeStarted;
            actionController.PerfectDodgeTriggered += HandlePerfectDodgeTriggered;
            actionSubscribed = true;
        }

        private void UnsubscribeActions()
        {
            if (!actionSubscribed || actionController == null)
            {
                actionSubscribed = false;
                return;
            }

            actionController.BasicAttackStarted -= HandleBasicAttackStarted;
            actionController.BasicAttackHit -= HandleBasicAttackHit;
            actionController.DodgeStarted -= HandleDodgeStarted;
            actionController.PerfectDodgeTriggered -= HandlePerfectDodgeTriggered;
            actionSubscribed = false;
        }

        private void SubscribeHealth()
        {
            if (healthSubscribed || playerHealth == null)
            {
                return;
            }

            playerHealth.Damaged += HandlePlayerDamaged;
            playerHealth.DamageBlockedByInvulnerability += HandleDamageBlockedByInvulnerability;
            playerHealth.Died += HandlePlayerDied;
            healthSubscribed = true;
        }

        private void UnsubscribeHealth()
        {
            if (!healthSubscribed || playerHealth == null)
            {
                healthSubscribed = false;
                return;
            }

            playerHealth.Damaged -= HandlePlayerDamaged;
            playerHealth.DamageBlockedByInvulnerability -= HandleDamageBlockedByInvulnerability;
            playerHealth.Died -= HandlePlayerDied;
            healthSubscribed = false;
        }

        private void HandleBasicAttackStarted(int comboIndex)
        {
            Play(CombatVfxCueId.PlayerBasicAttackStart, attackAnchor, actionController.LastAttackDirection, ResolveComboIntensity(comboIndex));
        }

        private void HandleBasicAttackHit(int comboIndex)
        {
            Play(CombatVfxCueId.PlayerBasicAttackHit, attackAnchor, actionController.LastAttackDirection, ResolveComboIntensity(comboIndex));
        }

        private void HandleDodgeStarted()
        {
            Play(CombatVfxCueId.PlayerDodgeStart, dodgeAnchor, actionController.LastDodgeDirection, 1f);
        }

        private void HandlePerfectDodgeTriggered(DamageInfo damageInfo)
        {
            Vector3 dodgeDirection = actionController != null ? actionController.LastDodgeDirection : transform.forward;
            Transform anchor = dodgeAnchor != null ? dodgeAnchor : transform;
            float masterIntensity = Mathf.Max(0f, perfectDodgeCueIntensity);
            Play(perfectDodgeTimeFieldCueId, anchor, dodgeDirection, perfectDodgeTimeFieldIntensity * masterIntensity, 0f, Vector3.zero);
            Play(perfectDodgePulsewaveCueId, anchor, dodgeDirection, perfectDodgePulsewaveIntensity * masterIntensity, 0f, Vector3.zero);
            Play(perfectDodgeHoloCubeCueId, anchor, dodgeDirection, perfectDodgeHoloCubeIntensity * masterIntensity, 0f, Vector3.zero);
            if (perfectDodgeProjectileBlockCueId != perfectDodgeWindowCueId)
            {
                Play(perfectDodgeWindowCueId, anchor, dodgeDirection, perfectDodgeWindowIntensity, 0f, Vector3.zero);
            }

            if (perfectDodgeVfxDirector != null)
            {
                perfectDodgeVfxDirector.Play(
                    damageInfo,
                    anchor,
                    dodgeDirection,
                    masterIntensity,
                    perfectDodgeAudioIntensity);
            }

            TryPlayPerfectDodgeBlockVfx(damageInfo, anchor, dodgeDirection);
        }

        private void HandleDamageBlockedByInvulnerability(DamageInfo damageInfo)
        {
            if (!playPerfectDodgeProjectileBlockVfx
                || damageInfo.SourceTeam != DamageTeam.Enemy
                || cuePlayer == null)
            {
                return;
            }

            if (perfectDodgeProjectileBlockCueId == perfectDodgeWindowCueId)
            {
                return;
            }

            Transform anchor = dodgeAnchor != null ? dodgeAnchor : DamageAnchor;
            TryPlayPerfectDodgeBlockVfx(damageInfo, anchor != null ? anchor : transform, Vector3.zero);
        }

        private bool TryPlayPerfectDodgeBlockVfx(DamageInfo damageInfo, Transform anchor, Vector3 fallbackDirection)
        {
            if (!playPerfectDodgeProjectileBlockVfx
                || damageInfo.SourceTeam != DamageTeam.Enemy
                || cuePlayer == null)
            {
                return false;
            }

            anchor = anchor != null ? anchor : transform;
            bool centeredShieldBlock = perfectDodgeProjectileBlockCueId == perfectDodgeWindowCueId;
            Vector3 blockPoint = ResolvePerfectDodgeShieldBlockPoint(damageInfo, anchor);
            Vector3 duplicatePoint = centeredShieldBlock ? anchor.position : blockPoint;
            if (Time.time - lastPerfectDodgeBlockVfxTime <= PerfectDodgeBlockVfxRepeatIntervalSeconds
                && (duplicatePoint - lastPerfectDodgeBlockVfxPoint).sqrMagnitude <= 0.09f)
            {
                return false;
            }

            Vector3 localBlockOffset = centeredShieldBlock
                ? Vector3.zero
                : anchor.InverseTransformPoint(blockPoint) - new Vector3(0f, 0.04f, 0f);
            Vector3 blockDirection = damageInfo.Direction.sqrMagnitude > 0.0001f
                ? -Vector3.ProjectOnPlane(damageInfo.Direction, Vector3.up).normalized
                : fallbackDirection.sqrMagnitude > 0.0001f
                    ? Vector3.ProjectOnPlane(fallbackDirection, Vector3.up).normalized
                    : anchor.forward;

            if (Play(
                    perfectDodgeProjectileBlockCueId,
                    anchor,
                    blockDirection,
                    perfectDodgeProjectileBlockIntensity,
                    -1f,
                    localBlockOffset))
            {
                lastPerfectDodgeBlockVfxTime = Time.time;
                lastPerfectDodgeBlockVfxPoint = duplicatePoint;
                perfectDodgeProjectileBlockCueRequestCount++;
                return true;
            }

            return false;
        }

        private void HandlePlayerDamaged(DamageInfo damageInfo)
        {
            if (!DamageResponsePolicyUtility.PlaysDamagePresentation(damageInfo.ResponsePolicy))
            {
                return;
            }

            float damageScale = playerHealth != null && playerHealth.MaxHealth > 0f
                ? Mathf.Clamp01(damageInfo.Amount / playerHealth.MaxHealth)
                : 0f;
            float policyScale = ResolveDamageCuePolicyScale(damageInfo);
            float intensity = (damagedCueIntensity + damageScale * 0.35f) * policyScale;
            bool interruptsAction = DamageResponsePolicyUtility.InterruptsAction(damageInfo.ControlLockPolicy);
            lastDamageCueIntensity = intensity;
            lastDamageCuePolicyScale = policyScale;
            lastDamageResponsePolicy = damageInfo.ResponsePolicy;
            lastDamageControlLockPolicy = damageInfo.ControlLockPolicy;
            lastDamageCueInterruptedAction = interruptsAction;

            if (playDamageVfx && Play(damagedCueId, DamageAnchor, damageInfo.Direction, intensity))
            {
                damageVfxCueRequestCount++;
            }

            if (playerHealth == null || playerHealth.HealthRatio > criticalHealthRatio)
            {
                criticalCuePlayed = false;
                return;
            }

            if (criticalCuePlayed || !playCriticalVfx)
            {
                return;
            }

            if (Play(criticalCueId, DamageAnchor, damageInfo.Direction, criticalCueIntensity))
            {
                criticalVfxCueRequestCount++;
                criticalCuePlayed = true;
            }
        }

        private Vector3 ResolvePerfectDodgeShieldBlockPoint(DamageInfo damageInfo, Transform anchor)
        {
            Vector3 center = anchor.position + Vector3.up * 0.72f;
            Vector3 incomingDirection = damageInfo.Direction.sqrMagnitude > 0.0001f
                ? damageInfo.Direction.normalized
                : (center - damageInfo.Point).normalized;
            if (incomingDirection.sqrMagnitude <= 0.0001f)
            {
                incomingDirection = -anchor.forward;
            }

            Vector3 shieldSurface = center - incomingDirection * Mathf.Max(0.05f, perfectDodgeShieldBlockRadius);
            if ((damageInfo.Point - center).sqrMagnitude > 0.0001f)
            {
                return Vector3.Lerp(damageInfo.Point, shieldSurface, 0.65f);
            }

            return shieldSurface;
        }

        private float ResolveDamageCuePolicyScale(DamageInfo damageInfo)
        {
            return DamageResponsePolicyUtility.InterruptsAction(damageInfo.ControlLockPolicy)
                ? 1f
                : Mathf.Clamp(pressureDamageCueScale, 0.1f, 1f);
        }

        private void HandlePlayerDied()
        {
            if (criticalCuePlayed || !playCriticalVfx)
            {
                return;
            }

            if (Play(criticalCueId, DamageAnchor, Vector3.back, criticalCueIntensity + 0.22f))
            {
                criticalVfxCueRequestCount++;
                criticalCuePlayed = true;
            }
        }

        private bool Play(CombatVfxCueId cueId, Transform anchor, Vector3 direction, float intensity)
        {
            return Play(cueId, anchor, direction, intensity, -1f, Vector3.zero);
        }

        private bool Play(
            CombatVfxCueId cueId,
            Transform anchor,
            Vector3 direction,
            float intensity,
            float audioIntensity,
            Vector3 additionalLocalPositionOffset)
        {
            if (cuePlayer == null)
            {
                return false;
            }

            return cuePlayer.PlayCue(
                cueId,
                anchor != null ? anchor : transform,
                direction,
                intensity,
                audioIntensity,
                additionalLocalPositionOffset);
        }

        private static float ResolveComboIntensity(int comboIndex)
        {
            return Mathf.Lerp(1f, 1.35f, Mathf.Clamp01(comboIndex / 4f));
        }
    }
}
