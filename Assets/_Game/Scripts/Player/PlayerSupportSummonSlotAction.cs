using System;
using System.Collections;
using DimensionBrawl.Combat;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Presentation;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DimensionBrawl.Player
{
    public sealed class PlayerSupportSummonSlotAction : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private string slotActionName = "SummonSlot2";
        [SerializeField] private InputActionReference summonAction;
        [SerializeField] private bool useKeyboardWhenActionMissing = true;
        [SerializeField] private Key keyboardTestKey = Key.Digit2;

        [Header("References")]
        [SerializeField] private SummonEnergyLadder energyLadder;
        [SerializeField] private CombatHealth sourceHealth;
        [SerializeField] private PlayerCombatTargetSelector targetSelector;
        [SerializeField] private CombatHealth frontlineTargetHealth;
        [SerializeField] private SummonLaneSpace laneSpace;

        [Header("Summon Action")]
        [SerializeField] private LaneActionProjectile projectilePrefab;
        [SerializeField] private GameObject projectilePrefabObject;
        [SerializeField] private GameObject entryCuePrefab;
        [SerializeField] private SummonFrontlineProxy summonActorPrefab;
        [SerializeField] private GameObject summonActorPrefabObject;
        [SerializeField] private Transform projectileRoot;
        [SerializeField] private Transform cueRoot;
        [SerializeField] private Transform summonActorRoot;
        [SerializeField] private CombatVfxCuePlayer combatVfxCuePlayer;
        [SerializeField] private DamageTeam sourceTeam = DamageTeam.AllySummon;
        [SerializeField] private Vector2 laneOffset = Vector2.zero;

        [Header("Entry")]
        [Tooltip("Support summons keep their authored lane side offset, but appear in front of the player body before crossing the frontline.")]
        [SerializeField, Min(0f)] private float entryForwardOffset = 1.35f;
        [Tooltip("Extra travel time per meter after the authored advance distance. Keep this high enough that summons march instead of snapping to the target.")]
        [SerializeField, Min(0f)] private float actorEntryCatchupSecondsPerMeter = 0.55f;

        [Header("Support Cadence")]
        [SerializeField, Min(1f)] private float requiredSummonMana = 100f;
        [SerializeField, Range(1, 3)] private int minimumSummonTier = 1;
        [SerializeField, Min(0f)] private float slotCooldownSeconds = 1.5f;
        [SerializeField, Min(0f)] private float firstVolleyDelaySeconds = 0.08f;
        [SerializeField, Min(0.1f)] private float volleyIntervalSeconds = 0.85f;
        [SerializeField, Min(1)] private int maxVolleyCount = 4;
        [SerializeField, Min(1)] private int maxActiveSummonActors = 1;

        [Header("Profile")]
        [SerializeField] private SummonSlotActionProfile summonActionProfile;

        [Header("Failure Feedback")]
        [SerializeField, Min(0f)] private float useBlockedHintSeconds = 0.75f;

        private SupportSummonSlotExecutor executor;
        private bool actionEnabledHere;
        private bool queued;
        private int lastSpentTier;
        private int totalUseCount;
        private float blockedHintTimer;
        private string lastBlockedReason;
        private float slotCooldownRemaining;
        private int lastPressureScreenInterceptTier;
        private int totalPressureScreenInterceptCount;

        public string SlotActionName => slotActionName;
        public float RequiredSummonMana => Mathf.Max(1f, requiredSummonMana);
        public int MinimumSummonTier => Mathf.Clamp(minimumSummonTier, 1, 3);
        public float SlotCooldownSeconds => Mathf.Max(0f, slotCooldownSeconds);
        public float SlotCooldownRemaining => slotCooldownRemaining;
        public bool IsSlotOnCooldown => slotCooldownRemaining > 0f;
        public int LastSpentTier => lastSpentTier;
        public int TotalUseCount => totalUseCount;
        public int LastPressureScreenInterceptTier => lastPressureScreenInterceptTier;
        public int TotalPressureScreenInterceptCount => totalPressureScreenInterceptCount;
        public int ActiveProjectileCount => Executor.ActiveProjectileCount;
        public int ActiveSummonActorCount => Executor.ActiveSummonActorCount;
        public string LastSummonActorRoleId => Executor.LastSummonActorRoleId;
        public int LastVolleyWaveCount => Executor.LastVolleyWaveCount;
        public int TotalVolleyWaveCount => Executor.TotalVolleyWaveCount;
        public bool LastSummonActorHasHealth => Executor.LastSummonActorHasHealth;
        public float LastSummonActorHealthRatio => Executor.LastSummonActorHealthRatio;
        public float LastSummonActorRemainingLifetimeSeconds => Executor.LastSummonActorRemainingLifetimeSeconds;
        public float ActiveSummonActorAdvanceProgress01 => Executor.ActiveSummonActorAdvanceProgress01;
        public bool LastSummonActorIsClashing => Executor.LastSummonActorIsClashing;
        public int LastSummonActorClashCount => Executor.LastSummonActorClashCount;
        public SummonFrontlineProxyExitReason LastSummonActorExitReason => Executor.LastSummonActorExitReason;
        public bool HasRequiredPresentation => Executor.HasRequiredPresentation;
        public bool ShowUseBlockedHint => blockedHintTimer > 0f;
        public string LastUseBlockedReason => lastBlockedReason;

        internal CombatHealth SourceHealth => sourceHealth;
        internal DamageTeam SourceTeam => sourceTeam;
        internal PlayerCombatTargetSelector TargetSelector => targetSelector;
        internal CombatHealth FrontlineTargetHealth => frontlineTargetHealth;
        internal SummonLaneSpace LaneSpace => laneSpace;
        internal Vector2 LaneOffset => laneOffset;
        internal Transform ProjectileRoot => projectileRoot;
        internal Transform CueRoot => cueRoot;
        internal Transform SummonActorRoot => summonActorRoot;
        internal CombatVfxCuePlayer CombatVfxCuePlayer => combatVfxCuePlayer;
        internal GameObject EntryCuePrefab => entryCuePrefab;
        internal float FirstVolleyDelaySeconds => Mathf.Max(0f, firstVolleyDelaySeconds);
        internal float VolleyIntervalSeconds => Mathf.Max(0.1f, volleyIntervalSeconds);
        internal int MaxVolleyCount => Mathf.Max(1, maxVolleyCount);
        internal int MaxActiveSummonActors => Mathf.Max(1, maxActiveSummonActors);

        public event Action<PlayerSupportSummonSlotAction, int> SummonUsed;
        public event Action<PlayerSupportSummonSlotAction, int> SummonPressureBlocked;
        public event Action<PlayerSupportSummonSlotAction, LaneActionProjectile, CombatHealth, Vector3, Vector3> SummonProjectileDamageApplied;

        private SupportSummonSlotExecutor Executor => executor ??= new SupportSummonSlotExecutor(this);

        private void Awake()
        {
            energyLadder ??= GetComponent<SummonEnergyLadder>();
            sourceHealth ??= GetComponent<CombatHealth>();
            targetSelector ??= GetComponent<PlayerCombatTargetSelector>();
            combatVfxCuePlayer ??= GetComponent<CombatVfxCuePlayer>();
        }

        private void OnValidate()
        {
            laneOffset.x = Mathf.Clamp(laneOffset.x, -8f, 8f);
            laneOffset.y = Mathf.Clamp(laneOffset.y, -4f, 8f);
            entryForwardOffset = Mathf.Max(0f, entryForwardOffset);
            actorEntryCatchupSecondsPerMeter = Mathf.Max(0f, actorEntryCatchupSecondsPerMeter);
            requiredSummonMana = Mathf.Max(1f, requiredSummonMana);
            minimumSummonTier = Mathf.Clamp(minimumSummonTier, 1, 3);
            slotCooldownSeconds = Mathf.Max(0f, slotCooldownSeconds);
            firstVolleyDelaySeconds = Mathf.Max(0f, firstVolleyDelaySeconds);
            volleyIntervalSeconds = Mathf.Max(0.1f, volleyIntervalSeconds);
            maxVolleyCount = Mathf.Max(1, maxVolleyCount);
            maxActiveSummonActors = Mathf.Max(1, maxActiveSummonActors);
        }

        private void OnEnable()
        {
            actionEnabledHere = EnableActionIfNeeded(summonAction);
            Executor.Prewarm();
        }

        private void OnDisable()
        {
            executor?.Detach();
            DisableActionIfOwned(summonAction, actionEnabledHere);
            actionEnabledHere = false;
        }

        private void Update()
        {
            TickFeedback(Time.deltaTime);
            if (ReadSummonPressed())
            {
                TryUseSummon();
            }
        }

        public void ConfigureReferences(
            SummonEnergyLadder newEnergyLadder,
            CombatHealth newSourceHealth,
            PlayerCombatTargetSelector newTargetSelector,
            CombatHealth newFrontlineTargetHealth,
            SummonLaneSpace newLaneSpace,
            LaneActionProjectile newProjectilePrefab,
            GameObject newEntryCuePrefab,
            SummonFrontlineProxy newSummonActorPrefab,
            Transform newProjectileRoot,
            Transform newCueRoot,
            Transform newSummonActorRoot,
            CombatVfxCuePlayer newCombatVfxCuePlayer = null)
        {
            energyLadder = newEnergyLadder;
            sourceHealth = newSourceHealth;
            targetSelector = newTargetSelector;
            frontlineTargetHealth = newFrontlineTargetHealth;
            laneSpace = newLaneSpace;
            projectilePrefab = newProjectilePrefab;
            projectilePrefabObject = newProjectilePrefab != null ? newProjectilePrefab.gameObject : null;
            entryCuePrefab = newEntryCuePrefab;
            summonActorPrefab = newSummonActorPrefab;
            summonActorPrefabObject = newSummonActorPrefab != null ? newSummonActorPrefab.gameObject : null;
            projectileRoot = newProjectileRoot;
            cueRoot = newCueRoot;
            summonActorRoot = newSummonActorRoot;
            if (newCombatVfxCuePlayer != null)
            {
                combatVfxCuePlayer = newCombatVfxCuePlayer;
            }
        }

        public void ConfigureSlot(string newSlotActionName, Key newKeyboardTestKey, Vector2 newLaneOffset)
        {
            slotActionName = string.IsNullOrWhiteSpace(newSlotActionName) ? slotActionName : newSlotActionName;
            keyboardTestKey = newKeyboardTestKey;
            laneOffset = newLaneOffset;
        }

        public void ConfigureSummonActionProfile(SummonSlotActionProfile newSummonActionProfile)
        {
            summonActionProfile = newSummonActionProfile;
        }

        public void ConfigureSupportCadence(float firstDelaySeconds, float intervalSeconds, int volleyCount)
        {
            firstVolleyDelaySeconds = Mathf.Max(0f, firstDelaySeconds);
            volleyIntervalSeconds = Mathf.Max(0.1f, intervalSeconds);
            maxVolleyCount = Mathf.Max(1, volleyCount);
        }

        public void ConfigureMinimumSummonTier(int minimumTier)
        {
            minimumSummonTier = Mathf.Clamp(minimumTier, 1, 3);
        }

        public void ConfigureRequiredSummonMana(float requiredMana)
        {
            requiredSummonMana = Mathf.Max(1f, requiredMana);
        }

        public void ConfigureSlotCooldown(float cooldownSeconds)
        {
            slotCooldownSeconds = Mathf.Max(0f, cooldownSeconds);
            slotCooldownRemaining = Mathf.Min(slotCooldownRemaining, slotCooldownSeconds);
        }

        public bool TryGetTierReadout(int tier, out SummonSlotActionProfile.SummonTierReadout readout)
        {
            if (summonActionProfile == null)
            {
                readout = default;
                return false;
            }

            return summonActionProfile.TryGetTierReadout(tier, out readout);
        }

        public void QueueSummon()
        {
            queued = true;
        }

        public bool TryUseSummon()
        {
            if (energyLadder == null)
            {
                SetUseBlocked("Energy system missing");
                return false;
            }

            if (!Executor.HasRequiredPresentation)
            {
                SetUseBlocked("Summon presentation missing");
                return false;
            }

            if (IsSlotOnCooldown)
            {
                SetUseBlocked($"Cooldown {slotCooldownRemaining:0.0}s");
                return false;
            }

            int availableTier = energyLadder.AvailableTier;
            if (availableTier < MinimumSummonTier)
            {
                SetUseBlocked($"Requires LV{MinimumSummonTier} EN");
                return false;
            }

            if (!TryResolveTierSettings(availableTier, out PlayerSummonSlot1Action.SummonTierSettings tierSettings))
            {
                SetUseBlocked("Summon profile missing");
                return false;
            }

            if (!energyLadder.TrySpend(RequiredSummonMana, out int spentTier))
            {
                SetUseBlocked($"Requires {RequiredSummonMana:0} EN");
                return false;
            }

            lastSpentTier = Mathf.Clamp(spentTier, 1, 3);
            totalUseCount++;
            slotCooldownRemaining = SlotCooldownSeconds;
            lastPressureScreenInterceptTier = 0;
            blockedHintTimer = 0f;
            lastBlockedReason = null;
            Executor.FireTier(lastSpentTier, tierSettings);
            SummonUsed?.Invoke(this, lastSpentTier);
            return true;
        }

        internal LaneActionProjectile ResolveProjectilePrefab()
        {
            if (projectilePrefab != null)
            {
                return projectilePrefab;
            }

            if (projectilePrefabObject != null)
            {
                projectilePrefab = projectilePrefabObject.GetComponent<LaneActionProjectile>();
            }

            return projectilePrefab;
        }

        internal SummonFrontlineProxy ResolveSummonActorPrefab()
        {
            if (summonActorPrefab != null)
            {
                return summonActorPrefab;
            }

            if (summonActorPrefabObject != null)
            {
                summonActorPrefab = summonActorPrefabObject.GetComponent<SummonFrontlineProxy>();
            }

            return summonActorPrefab;
        }

        internal void RunRoutine(IEnumerator routine)
        {
            StartCoroutine(routine);
        }

        internal void NotifySummonPressureBlocked(int tier)
        {
            lastPressureScreenInterceptTier = Mathf.Clamp(tier, 1, 3);
            totalPressureScreenInterceptCount++;
            SummonPressureBlocked?.Invoke(this, lastPressureScreenInterceptTier);
        }

        internal void NotifySummonProjectileDamageApplied(
            LaneActionProjectile projectile,
            CombatHealth targetHealth,
            Vector3 impactPoint,
            Vector3 impactDirection)
        {
            SummonProjectileDamageApplied?.Invoke(this, projectile, targetHealth, impactPoint, impactDirection);
        }

        internal float ResolveEntryLaneZ(float playerLaneZ)
        {
            return playerLaneZ + entryForwardOffset + laneOffset.y;
        }

        internal float ResolveActorAdvanceSeconds(
            float resolvedAdvanceDistance,
            PlayerSummonSlot1Action.SummonTierSettings settings)
        {
            float extraDistance = Mathf.Max(0f, resolvedAdvanceDistance - settings.ActorAdvanceDistance);
            return settings.ActorAdvanceSeconds + extraDistance * actorEntryCatchupSecondsPerMeter;
        }

        private bool TryResolveTierSettings(int tier, out PlayerSummonSlot1Action.SummonTierSettings tierSettings)
        {
            PlayerSummonSlot1Action.SummonTierSettings[] settings = summonActionProfile != null
                ? summonActionProfile.CopyTierSettings()
                : null;
            if (settings == null || settings.Length == 0)
            {
                tierSettings = default;
                return false;
            }

            tierSettings = settings[Mathf.Clamp(tier - 1, 0, settings.Length - 1)];
            tierSettings.Normalize();
            return true;
        }

        private bool ReadSummonPressed()
        {
            bool pressed = queued;
            queued = false;
            if (summonAction != null && summonAction.action != null)
            {
                pressed |= summonAction.action.WasPressedThisFrame();
            }

            if (pressed || !useKeyboardWhenActionMissing || !IsActionMissing(summonAction))
            {
                return pressed;
            }

            return Keyboard.current != null
                && Keyboard.current[keyboardTestKey] != null
                && Keyboard.current[keyboardTestKey].wasPressedThisFrame;
        }

        private void SetUseBlocked(string reason)
        {
            lastBlockedReason = string.IsNullOrWhiteSpace(reason) ? "Unavailable" : reason;
            blockedHintTimer = useBlockedHintSeconds;
        }

        private void TickFeedback(float deltaTime)
        {
            if (slotCooldownRemaining > 0f)
            {
                slotCooldownRemaining = Mathf.Max(0f, slotCooldownRemaining - deltaTime);
            }

            if (blockedHintTimer <= 0f)
            {
                return;
            }

            blockedHintTimer = Mathf.Max(0f, blockedHintTimer - deltaTime);
            if (blockedHintTimer <= 0f)
            {
                lastBlockedReason = null;
            }
        }

        private static bool EnableActionIfNeeded(InputActionReference actionReference)
        {
            if (actionReference == null || actionReference.action == null || actionReference.action.enabled)
            {
                return false;
            }

            actionReference.action.Enable();
            return true;
        }

        private static void DisableActionIfOwned(InputActionReference actionReference, bool enabledHere)
        {
            if (enabledHere && actionReference != null && actionReference.action != null)
            {
                actionReference.action.Disable();
            }
        }

        private static bool IsActionMissing(InputActionReference actionReference)
        {
            return actionReference == null || actionReference.action == null;
        }
    }
}
