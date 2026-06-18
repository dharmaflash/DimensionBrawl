using System;
using DimensionBrawl.Combat;
using DimensionBrawl.LevelDesign;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DimensionBrawl.Player
{
    [DisallowMultipleComponent]
    public sealed partial class PlayerSummonSlot1Action : MonoBehaviour
    {
        [Serializable]
        public struct SummonTierSettings
        {
            [Min(0f)] public float Damage;
            [Min(0f)] public float ProjectileSpeed;
            [Min(0.01f)] public float LifetimeSeconds;
            [Min(0.01f)] public float Radius;
            [Min(1)] public int ProjectileCount;
            [Min(0f)] public float LateralReach;
            [Min(0f)] public float EntryHeight;
            [Min(0f)] public float TargetHeight;
            [Min(0f)] public float CueScale;
            [Min(0f)] public float CueLifetimeSeconds;
            [Min(0.05f)] public float ActorLifetimeSeconds;
            [Min(0.01f)] public float ActorScale;
            [Min(0f)] public float ActorAdvanceDistance;
            [Min(0.01f)] public float ActorAdvanceSeconds;
            [Min(0)] public int ScreenIntercepts;
            [Min(0.05f)] public float ScreenRadius;
            [Min(0.05f)] public float ScreenLifetimeSeconds;
            [Min(0f)] public float CounterDamage;
            [Min(0f)] public float CounterProjectileSpeed;
            [Min(0.01f)] public float CounterLifetimeSeconds;
            [Min(0.01f)] public float CounterRadius;
            [Min(0f)] public float CounterTargetHeight;

            public void Normalize()
            {
                Damage = Mathf.Max(0f, Damage);
                ProjectileSpeed = Mathf.Max(0f, ProjectileSpeed);
                LifetimeSeconds = Mathf.Max(0.01f, LifetimeSeconds);
                Radius = Mathf.Max(0.01f, Radius);
                ProjectileCount = Mathf.Max(1, ProjectileCount);
                LateralReach = Mathf.Max(0f, LateralReach);
                EntryHeight = Mathf.Max(0f, EntryHeight);
                TargetHeight = Mathf.Max(0f, TargetHeight);
                CueScale = Mathf.Max(0f, CueScale);
                CueLifetimeSeconds = Mathf.Max(0f, CueLifetimeSeconds);
                ActorLifetimeSeconds = Mathf.Max(0.05f, ActorLifetimeSeconds);
                ActorScale = Mathf.Max(0.01f, ActorScale);
                ActorAdvanceDistance = Mathf.Max(0f, ActorAdvanceDistance);
                ActorAdvanceSeconds = Mathf.Max(0.01f, ActorAdvanceSeconds);
                ScreenIntercepts = Mathf.Max(0, ScreenIntercepts);
                ScreenRadius = Mathf.Max(0.05f, ScreenRadius);
                ScreenLifetimeSeconds = Mathf.Max(0.05f, ScreenLifetimeSeconds);
                CounterDamage = Mathf.Max(0f, CounterDamage);
                CounterProjectileSpeed = Mathf.Max(0f, CounterProjectileSpeed);
                CounterLifetimeSeconds = Mathf.Max(0.01f, CounterLifetimeSeconds);
                CounterRadius = Mathf.Max(0.01f, CounterRadius);
                CounterTargetHeight = Mathf.Max(0f, CounterTargetHeight);
            }
        }

        [Header("Input")]
        [SerializeField] private InputActionReference summonAction;
        [SerializeField] private bool useKeyboardWhenActionMissing = true;
        [SerializeField] private Key keyboardTestKey = Key.Digit1;

        [Header("References")]
        [SerializeField] private SummonEnergyLadder energyLadder;
        [SerializeField] private CombatHealth sourceHealth;
        [SerializeField] private PlayerCombatTargetSelector targetSelector;
        [Tooltip("Preferred far/frontline target for summon exchanges. Local target selection is reserved for close defense and is only a fallback here.")]
        [SerializeField] private CombatHealth frontlineTargetHealth;
        [Tooltip("Summon actions use battlefield coordinates, not player clamping, because summons may cross lane rails and the player forward boundary.")]
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
        [SerializeField] private DamageTeam sourceTeam = DamageTeam.AllySummon;
        [SerializeField, Min(0)] private int prewarmCount = 6;
        [SerializeField, Min(0)] private int actorPrewarmCount = 2;
        [Header("Failure Feedback")]
        [SerializeField, Min(0f)] private float useBlockedHintSeconds = 0.75f;

        [Header("Tier Tuning")]
        [SerializeField] private SummonSlotActionProfile summonActionProfile;
        [SerializeField] private SummonTierSettings[] tierSettings = CreateDefaultTierSettings();

        private SummonExecutionRuntime executionRuntime;
        private bool actionEnabledHere;
        private bool queued;
        private int lastSpentTier;
        private int totalUseCount;
        private float blockedHintTimer;
        private string lastBlockedReason;

        public int LastSpentTier => lastSpentTier;
        public int LastFiredProjectileCount => executionRuntime != null ? executionRuntime.LastFiredProjectileCount : 0;
        public int LastPressureScreenMaxIntercepts => executionRuntime != null ? executionRuntime.LastPressureScreenMaxIntercepts : 0;
        public int LastPressureScreenInterceptCount => executionRuntime != null ? executionRuntime.LastPressureScreenInterceptCount : 0;
        public int LastPressureScreenInterceptTier => executionRuntime != null ? executionRuntime.LastPressureScreenInterceptTier : 0;
        public int TotalPressureScreenInterceptCount => executionRuntime != null ? executionRuntime.TotalPressureScreenInterceptCount : 0;
        public int TotalUseCount => totalUseCount;
        public Vector3 LastEntryPosition => executionRuntime != null ? executionRuntime.LastEntryPosition : Vector3.zero;
        public Vector3 LastSummonActorPosition => executionRuntime != null ? executionRuntime.LastSummonActorPosition : Vector3.zero;
        public int ActiveProjectileCount => executionRuntime != null ? executionRuntime.ActiveProjectileCount : 0;
        public int ActiveCueCount => executionRuntime != null ? executionRuntime.ActiveCueCount : 0;
        public int ActiveSummonActorCount => executionRuntime != null ? executionRuntime.ActiveSummonActorCount : 0;
        public int ActivePressureScreenCount => executionRuntime != null ? executionRuntime.ActivePressureScreenCount : 0;
        public int ActivePressureScreenRemainingIntercepts => executionRuntime != null
            ? executionRuntime.ActivePressureScreenRemainingIntercepts
            : 0;
        public SummonSlotActionProfile SummonActionProfile => summonActionProfile;
        public bool HasSummonActionProfile => summonActionProfile != null;
        public bool ShowUseBlockedHint => blockedHintTimer > 0f;
        public string LastUseBlockedReason => lastBlockedReason;

        public event Action<int> SummonSlot1Used;
        public event Action<int> SummonPressureBlocked;
        public event Action SummonSlot1UseBlocked;

        private void Awake()
        {
            if (energyLadder == null)
            {
                energyLadder = GetComponent<SummonEnergyLadder>();
            }

            if (sourceHealth == null)
            {
                sourceHealth = GetComponent<CombatHealth>();
            }

            if (targetSelector == null)
            {
                targetSelector = GetComponent<PlayerCombatTargetSelector>();
            }
        }

        private void OnValidate()
        {
            ApplySummonActionProfile();
            if (tierSettings == null)
            {
                return;
            }

            for (int i = 0; i < tierSettings.Length; i++)
            {
                SummonTierSettings settings = tierSettings[i];
                settings.Normalize();
                tierSettings[i] = settings;
            }
        }

        private void OnEnable()
        {
            ApplySummonActionProfile();
            actionEnabledHere = EnableActionIfNeeded(summonAction);
            EnsureExecutionRuntime();
            executionRuntime.Prewarm();
        }

        private void OnDisable()
        {
            executionRuntime?.Detach();
            DisableActionIfOwned(summonAction, actionEnabledHere);
            actionEnabledHere = false;
        }

        private void Update()
        {
            TickFeedback(Time.deltaTime);
            if (ReadSummonPressed())
            {
                TryUseSummonSlot1();
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
            Transform newProjectileRoot,
            Transform newCueRoot,
            SummonFrontlineProxy newSummonActorPrefab = null,
            Transform newSummonActorRoot = null)
        {
            energyLadder = newEnergyLadder;
            sourceHealth = newSourceHealth;
            targetSelector = newTargetSelector;
            frontlineTargetHealth = newFrontlineTargetHealth;
            laneSpace = newLaneSpace;
            projectilePrefab = newProjectilePrefab;
            projectilePrefabObject = newProjectilePrefab != null ? newProjectilePrefab.gameObject : null;
            entryCuePrefab = newEntryCuePrefab;
            projectileRoot = newProjectileRoot;
            cueRoot = newCueRoot;

            if (newSummonActorPrefab != null)
            {
                summonActorPrefab = newSummonActorPrefab;
                summonActorPrefabObject = newSummonActorPrefab.gameObject;
            }

            if (newSummonActorRoot != null)
            {
                summonActorRoot = newSummonActorRoot;
            }
        }

        public void ResetToDefaultTierSettings()
        {
            summonActionProfile = null;
            tierSettings = CreateDefaultTierSettings();
        }

        public void ConfigureSummonActionProfile(SummonSlotActionProfile newSummonActionProfile)
        {
            summonActionProfile = newSummonActionProfile;
            ApplySummonActionProfile();
        }

        public void QueueSummonSlot1()
        {
            queued = true;
        }

        public bool TryUseSummonSlot1()
        {
            if (energyLadder == null)
            {
                SetUseBlocked("Energy system missing");
                return false;
            }

            if (!energyLadder.TrySpend(out int spentTier))
            {
                SetUseBlocked("EN not ready");
                return false;
            }

            lastSpentTier = Mathf.Clamp(spentTier, 1, 3);
            totalUseCount++;
            blockedHintTimer = 0f;
            lastBlockedReason = null;
            EnsureExecutionRuntime();
            executionRuntime.FireTier(lastSpentTier);
            SummonSlot1Used?.Invoke(lastSpentTier);
            return true;
        }

        private void EnsureExecutionRuntime()
        {
            if (executionRuntime == null)
            {
                executionRuntime = new SummonExecutionRuntime(this);
            }
        }

        private void ApplySummonActionProfile()
        {
            if (summonActionProfile == null)
            {
                return;
            }

            tierSettings = summonActionProfile.CopyTierSettings();
        }

        private void NotifySummonPressureBlocked(int tier)
        {
            SummonPressureBlocked?.Invoke(tier);
        }

        private void SetUseBlocked(string reason)
        {
            lastBlockedReason = string.IsNullOrWhiteSpace(reason) ? "Unavailable" : reason;
            blockedHintTimer = useBlockedHintSeconds;
            SummonSlot1UseBlocked?.Invoke();
        }

        private void TickFeedback(float deltaTime)
        {
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

        private SummonTierSettings ResolveTierSettings(int tier)
        {
            if (tierSettings == null || tierSettings.Length == 0)
            {
                return new SummonTierSettings
                {
                    Damage = 55f,
                    ProjectileSpeed = 17f,
                    LifetimeSeconds = 2.4f,
                    Radius = 0.34f,
                    ProjectileCount = 1,
                    LateralReach = 1f,
                    EntryHeight = 0.18f,
                    TargetHeight = 1.3f,
                    CueScale = 1.5f,
                    CueLifetimeSeconds = 0.85f,
                    ActorLifetimeSeconds = 1.25f,
                    ActorScale = 1f,
                    ActorAdvanceDistance = 1f,
                    ActorAdvanceSeconds = 0.24f,
                    ScreenIntercepts = 2,
                    ScreenRadius = 1.25f,
                    ScreenLifetimeSeconds = 1.15f,
                    CounterDamage = 16f,
                    CounterProjectileSpeed = 20f,
                    CounterLifetimeSeconds = 1.65f,
                    CounterRadius = 0.24f,
                    CounterTargetHeight = 1.35f
                };
            }

            return tierSettings[Mathf.Clamp(tier - 1, 0, tierSettings.Length - 1)];
        }

        private static SummonTierSettings[] CreateDefaultTierSettings()
        {
            return new[]
            {
                new SummonTierSettings
                {
                    Damage = 58f,
                    ProjectileSpeed = 17f,
                    LifetimeSeconds = 2.4f,
                    Radius = 0.34f,
                    ProjectileCount = 1,
                    LateralReach = 1.2f,
                    EntryHeight = 0.18f,
                    TargetHeight = 1.35f,
                    CueScale = 1.45f,
                    CueLifetimeSeconds = 0.85f,
                    ActorLifetimeSeconds = 1.25f,
                    ActorScale = 0.9f,
                    ActorAdvanceDistance = 1.05f,
                    ActorAdvanceSeconds = 0.24f,
                    ScreenIntercepts = 2,
                    ScreenRadius = 1.25f,
                    ScreenLifetimeSeconds = 1.15f,
                    CounterDamage = 16f,
                    CounterProjectileSpeed = 20f,
                    CounterLifetimeSeconds = 1.65f,
                    CounterRadius = 0.24f,
                    CounterTargetHeight = 1.35f
                },
                new SummonTierSettings
                {
                    Damage = 66f,
                    ProjectileSpeed = 18.5f,
                    LifetimeSeconds = 2.65f,
                    Radius = 0.38f,
                    ProjectileCount = 2,
                    LateralReach = 4.2f,
                    EntryHeight = 0.18f,
                    TargetHeight = 1.35f,
                    CueScale = 1.85f,
                    CueLifetimeSeconds = 1f,
                    ActorLifetimeSeconds = 1.55f,
                    ActorScale = 1.08f,
                    ActorAdvanceDistance = 1.65f,
                    ActorAdvanceSeconds = 0.3f,
                    ScreenIntercepts = 4,
                    ScreenRadius = 1.55f,
                    ScreenLifetimeSeconds = 1.4f,
                    CounterDamage = 22f,
                    CounterProjectileSpeed = 21.5f,
                    CounterLifetimeSeconds = 1.8f,
                    CounterRadius = 0.27f,
                    CounterTargetHeight = 1.4f
                },
                new SummonTierSettings
                {
                    Damage = 78f,
                    ProjectileSpeed = 20f,
                    LifetimeSeconds = 2.9f,
                    Radius = 0.42f,
                    ProjectileCount = 3,
                    LateralReach = 6.8f,
                    EntryHeight = 0.18f,
                    TargetHeight = 1.45f,
                    CueScale = 2.25f,
                    CueLifetimeSeconds = 1.15f,
                    ActorLifetimeSeconds = 1.85f,
                    ActorScale = 1.28f,
                    ActorAdvanceDistance = 2.35f,
                    ActorAdvanceSeconds = 0.36f,
                    ScreenIntercepts = 7,
                    ScreenRadius = 1.9f,
                    ScreenLifetimeSeconds = 1.7f,
                    CounterDamage = 30f,
                    CounterProjectileSpeed = 23f,
                    CounterLifetimeSeconds = 2f,
                    CounterRadius = 0.3f,
                    CounterTargetHeight = 1.45f
                }
            };
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
