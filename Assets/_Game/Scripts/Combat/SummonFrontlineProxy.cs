using System;
using System.Collections.Generic;
using UnityEngine;

namespace DimensionBrawl.Combat
{
    public enum SummonFrontlineProxyExitReason
    {
        None = 0,
        LifetimeExpired = 1,
        Defeated = 2,
        Recalled = 3
    }

    public enum SummonFrontlineProxyState
    {
        Inactive = 0,
        Spawned = 1,
        Advancing = 2,
        Engaging = 3,
        Attacking = 4,
        Defeated = 5
    }

    [DisallowMultipleComponent]
    public sealed class SummonFrontlineProxy : MonoBehaviour
    {
        private static readonly List<SummonFrontlineProxy> ActiveRegistry = new List<SummonFrontlineProxy>(16);

        [SerializeField] private Transform projectileOrigin;
        [SerializeField] private SummonPressureScreen pressureScreen;
        [SerializeField] private CombatHealth health;
        [SerializeField] private bool resetHealthOnActivate = true;
        [SerializeField] private bool faceTargetOnActivate = true;
        [SerializeField, Min(0f)] private float defaultAdvanceDistance = 0f;
        [SerializeField, Min(0.01f)] private float defaultAdvanceSeconds = 0.25f;
        [SerializeField, Min(0f)] private float defeatedLingerSeconds = 0.22f;

        private Vector3 baseScale = Vector3.one;
        private float remainingLifetime;
        private float lifetimeSeconds;
        private Vector3 advanceStartPosition;
        private Vector3 advanceTargetPosition;
        private float advanceSeconds = 0.25f;
        private float advanceElapsed;
        private float advanceDistance;
        private float advanceSpeed;
        private float advanceHoldTimer;
        private float attackStateTimer;
        private float defeatedLingerTimer;
        private bool active;
        private int activeTier;
        private bool subscribedToHealth;
        private SummonFrontlineProxyState currentState = SummonFrontlineProxyState.Inactive;
        private SummonFrontlineProxyExitReason lastExitReason = SummonFrontlineProxyExitReason.None;

        public bool IsActive => active && gameObject.activeInHierarchy;
        public bool IsPresentationVisible => IsActive || defeatedLingerTimer > 0f;
        public int ActiveTier => activeTier;
        public Transform ProjectileOrigin => projectileOrigin != null ? projectileOrigin : transform;
        public SummonPressureScreen PressureScreen => pressureScreen;
        public CombatHealth Health => health;
        public bool HasHealth => health != null;
        public bool HasLifetimeLimit => lifetimeSeconds > 0f;
        public float CurrentHealth => health != null ? health.CurrentHealth : 0f;
        public float MaxHealth => health != null ? health.MaxHealth : 0f;
        public float HealthRatio => health != null ? health.HealthRatio : 0f;
        public float RemainingLifetimeSeconds => remainingLifetime;
        public float LifetimeProgress01 => lifetimeSeconds > 0f
            ? 1f - Mathf.Clamp01(remainingLifetime / lifetimeSeconds)
            : 0f;
        public SummonFrontlineProxyState CurrentState => currentState;
        public SummonFrontlineProxyExitReason LastExitReason => lastExitReason;
        public Vector3 AdvanceStartPosition => advanceStartPosition;
        public Vector3 AdvanceTargetPosition => advanceTargetPosition;
        public float AdvanceDistance => advanceDistance;
        public float ActiveMoveSpeed => advanceSpeed;
        public float DefeatedLingerRemainingSeconds => defeatedLingerTimer;
        public float AdvanceProgress01 => advanceDistance > 0f ? Mathf.Clamp01(advanceElapsed / advanceDistance) : 1f;
        public bool IsAdvanceHeld => IsActive && advanceHoldTimer > 0f;
        public bool IsAdvancing => IsActive
            && AdvanceProgress01 < 1f
            && !IsAdvanceHeld
            && (advanceTargetPosition - advanceStartPosition).sqrMagnitude > 0.0001f;

        public event Action<SummonFrontlineProxy, SummonFrontlineProxyExitReason> Exited;

        public static int ActiveRegisteredProxyCount
        {
            get
            {
                CompactActiveRegistry();
                return ActiveRegistry.Count;
            }
        }

        private void Awake()
        {
            baseScale = transform.localScale;
            if (pressureScreen == null)
            {
                pressureScreen = GetComponentInChildren<SummonPressureScreen>(includeInactive: true);
            }

            if (health == null)
            {
                health = GetComponent<CombatHealth>();
            }
        }

        private void OnEnable()
        {
            SubscribeHealth();
            if (active)
            {
                RegisterActiveProxy();
            }
        }

        private void OnDisable()
        {
            UnregisterActiveProxy();
            UnsubscribeHealth();
        }

        private void OnDestroy()
        {
            UnregisterActiveProxy();
        }

        public void ConfigurePresentation(Transform newProjectileOrigin, SummonPressureScreen newPressureScreen)
        {
            projectileOrigin = newProjectileOrigin;
            pressureScreen = newPressureScreen;
        }

        public void ConfigureHealth(CombatHealth newHealth)
        {
            UnsubscribeHealth();
            health = newHealth;
            SubscribeHealth();
        }

        public void Activate(
            Vector3 position,
            Vector3 facingDirection,
            int tier,
            float lifetimeSeconds,
            float scaleMultiplier)
        {
            Activate(
                position,
                facingDirection,
                tier,
                lifetimeSeconds,
                scaleMultiplier,
                defaultAdvanceDistance,
                defaultAdvanceSeconds);
        }

        public void Activate(
            Vector3 position,
            Vector3 facingDirection,
            int tier,
            float lifetimeSeconds,
            float scaleMultiplier,
            float advanceDistance,
            float advanceDurationSeconds)
        {
            Vector3 planarDirection = ResolvePlanarDirection(facingDirection);
            Activate(
                position,
                planarDirection,
                tier,
                lifetimeSeconds,
                scaleMultiplier,
                position + planarDirection * Mathf.Max(0f, advanceDistance),
                advanceDurationSeconds);
        }

        public void Activate(
            Vector3 position,
            Vector3 facingDirection,
            int tier,
            float lifetimeSeconds,
            float scaleMultiplier,
            float advanceDistance,
            float advanceDurationSeconds,
            float actorMaxHealth,
            float moveSpeed)
        {
            Vector3 planarDirection = ResolvePlanarDirection(facingDirection);
            Activate(
                position,
                planarDirection,
                tier,
                lifetimeSeconds,
                scaleMultiplier,
                position + planarDirection * Mathf.Max(0f, advanceDistance),
                advanceDurationSeconds,
                actorMaxHealth,
                moveSpeed);
        }

        public void Activate(
            Vector3 position,
            Vector3 facingDirection,
            int tier,
            float lifetimeSeconds,
            float scaleMultiplier,
            Vector3 targetPosition,
            float advanceDurationSeconds)
        {
            Activate(
                position,
                facingDirection,
                tier,
                lifetimeSeconds,
                scaleMultiplier,
                targetPosition,
                advanceDurationSeconds,
                0f,
                0f);
        }

        public void Activate(
            Vector3 position,
            Vector3 facingDirection,
            int tier,
            float lifetimeSeconds,
            float scaleMultiplier,
            Vector3 targetPosition,
            float advanceDurationSeconds,
            float actorMaxHealth,
            float moveSpeed)
        {
            activeTier = Mathf.Clamp(tier, 1, 3);
            ResetLifecycle(lifetimeSeconds);
            transform.position = position;
            transform.localScale = baseScale * Mathf.Max(0.01f, scaleMultiplier);
            ResetHealthIfNeeded(actorMaxHealth);

            Vector3 planarDirection = ResetAdvance(position, facingDirection, targetPosition, advanceDurationSeconds, moveSpeed);
            ApplyFacing(planarDirection);

            active = true;
            defeatedLingerTimer = 0f;
            currentState = advanceDistance > 0f ? SummonFrontlineProxyState.Advancing : SummonFrontlineProxyState.Spawned;
            gameObject.SetActive(true);
            RegisterActiveProxy();
        }

        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return;
            }

            TickDefeatedLinger(deltaTime);
            if (!active)
            {
                return;
            }

            if (health != null && !health.IsAlive)
            {
                Deactivate(SummonFrontlineProxyExitReason.Defeated);
                return;
            }

            if (attackStateTimer > 0f)
            {
                attackStateTimer = Mathf.Max(0f, attackStateTimer - deltaTime);
            }

            Advance(deltaTime);
            if (advanceHoldTimer > 0f)
            {
                advanceHoldTimer = Mathf.Max(0f, advanceHoldTimer - deltaTime);
            }

            RefreshState();

            if (lifetimeSeconds <= 0f)
            {
                return;
            }

            remainingLifetime -= deltaTime;
            if (remainingLifetime <= 0f)
            {
                Deactivate(SummonFrontlineProxyExitReason.LifetimeExpired);
            }
        }

        public void RequestAdvanceHold(float seconds)
        {
            if (!active || seconds <= 0f)
            {
                return;
            }

            advanceHoldTimer = Mathf.Max(advanceHoldTimer, seconds);
            if (attackStateTimer <= 0f)
            {
                currentState = SummonFrontlineProxyState.Engaging;
            }
        }

        public void NotifyAttackPerformed(float feedbackSeconds)
        {
            if (!active)
            {
                return;
            }

            attackStateTimer = Mathf.Max(attackStateTimer, Mathf.Max(0.01f, feedbackSeconds));
            currentState = SummonFrontlineProxyState.Attacking;
        }

        public void FaceTowards(Vector3 worldPosition)
        {
            if (!active)
            {
                return;
            }

            ApplyFacing(ResolvePlanarDirection(worldPosition - transform.position));
        }

        public void Deactivate()
        {
            Deactivate(SummonFrontlineProxyExitReason.Recalled);
        }

        public void Deactivate(SummonFrontlineProxyExitReason reason)
        {
            if (pressureScreen != null)
            {
                pressureScreen.Deactivate();
            }

            UnregisterActiveProxy();
            bool shouldReportExit = active || reason != SummonFrontlineProxyExitReason.None;
            if (shouldReportExit)
            {
                lastExitReason = reason;
                Exited?.Invoke(this, reason);
            }

            active = false;
            remainingLifetime = 0f;
            advanceHoldTimer = 0f;
            attackStateTimer = 0f;
            advanceElapsed = advanceDistance;
            currentState = reason == SummonFrontlineProxyExitReason.Defeated
                ? SummonFrontlineProxyState.Defeated
                : SummonFrontlineProxyState.Inactive;
            defeatedLingerTimer = reason == SummonFrontlineProxyExitReason.Defeated
                ? Mathf.Max(0f, defeatedLingerSeconds)
                : 0f;

            if (defeatedLingerTimer <= 0f)
            {
                gameObject.SetActive(false);
            }
        }

        public static bool TryGetActiveRegisteredProxy(int index, out SummonFrontlineProxy proxy)
        {
            CompactActiveRegistry();
            if (index < 0 || index >= ActiveRegistry.Count)
            {
                proxy = null;
                return false;
            }

            proxy = ActiveRegistry[index];
            return proxy != null && proxy.IsActive;
        }

        private void Update()
        {
            Tick(Time.deltaTime);
        }

        private void Advance(float deltaTime)
        {
            if (advanceHoldTimer > 0f
                || advanceElapsed >= advanceDistance
                || advanceStartPosition == advanceTargetPosition)
            {
                return;
            }

            advanceElapsed = Mathf.Min(advanceDistance, advanceElapsed + advanceSpeed * deltaTime);
            transform.position = Vector3.LerpUnclamped(
                advanceStartPosition,
                advanceTargetPosition,
                AdvanceProgress01);
        }

        private void TickDefeatedLinger(float deltaTime)
        {
            if (defeatedLingerTimer <= 0f)
            {
                return;
            }

            defeatedLingerTimer = Mathf.Max(0f, defeatedLingerTimer - deltaTime);
            if (defeatedLingerTimer <= 0f && !active)
            {
                gameObject.SetActive(false);
            }
        }

        private void RefreshState()
        {
            if (!active)
            {
                return;
            }

            if (attackStateTimer > 0f)
            {
                currentState = SummonFrontlineProxyState.Attacking;
            }
            else if (advanceHoldTimer > 0f)
            {
                currentState = SummonFrontlineProxyState.Engaging;
            }
            else if (AdvanceProgress01 < 1f)
            {
                currentState = SummonFrontlineProxyState.Advancing;
            }
            else
            {
                currentState = SummonFrontlineProxyState.Spawned;
            }
        }

        private void ResetLifecycle(float requestedLifetimeSeconds)
        {
            lifetimeSeconds = Mathf.Max(0f, requestedLifetimeSeconds);
            remainingLifetime = lifetimeSeconds > 0f
                ? Mathf.Max(0.05f, lifetimeSeconds)
                : float.PositiveInfinity;
            lifetimeSeconds = lifetimeSeconds > 0f ? remainingLifetime : 0f;
            lastExitReason = SummonFrontlineProxyExitReason.None;
        }

        private void ResetHealthIfNeeded(float actorMaxHealth)
        {
            if (health == null)
            {
                return;
            }

            if (actorMaxHealth > 0f)
            {
                health.ConfigureMaxHealth(actorMaxHealth, resetHealthOnActivate);
            }
            else if (resetHealthOnActivate)
            {
                health.ResetHealthToFull();
            }
        }

        private Vector3 ResetAdvance(
            Vector3 position,
            Vector3 facingDirection,
            Vector3 targetPosition,
            float advanceDurationSeconds,
            float moveSpeed)
        {
            targetPosition.y = position.y;
            Vector3 planarDirection = ResolvePlanarDirection(targetPosition - position);
            if (planarDirection.sqrMagnitude <= 0.0001f)
            {
                planarDirection = ResolvePlanarDirection(facingDirection);
            }

            advanceSeconds = Mathf.Max(0.01f, advanceDurationSeconds);
            advanceElapsed = 0f;
            advanceDistance = Vector3.Distance(position, targetPosition);
            advanceSpeed = moveSpeed > 0f
                ? moveSpeed
                : advanceDistance / advanceSeconds;
            advanceHoldTimer = 0f;
            attackStateTimer = 0f;
            advanceStartPosition = position;
            advanceTargetPosition = targetPosition;
            return planarDirection;
        }

        private void ApplyFacing(Vector3 planarDirection)
        {
            if (faceTargetOnActivate && planarDirection.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.LookRotation(planarDirection, Vector3.up);
            }
        }

        private void RegisterActiveProxy()
        {
            if (!active || ActiveRegistry.Contains(this))
            {
                return;
            }

            ActiveRegistry.Add(this);
        }

        private void UnregisterActiveProxy()
        {
            ActiveRegistry.Remove(this);
        }

        private static void CompactActiveRegistry()
        {
            for (int i = ActiveRegistry.Count - 1; i >= 0; i--)
            {
                SummonFrontlineProxy proxy = ActiveRegistry[i];
                if (proxy == null || !proxy.IsActive)
                {
                    ActiveRegistry.RemoveAt(i);
                }
            }
        }

        private static Vector3 ResolvePlanarDirection(Vector3 direction)
        {
            Vector3 planarDirection = Vector3.ProjectOnPlane(direction, Vector3.up);
            if (planarDirection.sqrMagnitude > 0.0001f)
            {
                return planarDirection.normalized;
            }

            return Vector3.forward;
        }

        private void SubscribeHealth()
        {
            if (health == null || subscribedToHealth)
            {
                return;
            }

            health.Died += HandleHealthDied;
            subscribedToHealth = true;
        }

        private void UnsubscribeHealth()
        {
            if (health == null || !subscribedToHealth)
            {
                return;
            }

            health.Died -= HandleHealthDied;
            subscribedToHealth = false;
        }

        private void HandleHealthDied()
        {
            if (active)
            {
                Deactivate(SummonFrontlineProxyExitReason.Defeated);
            }
            else
            {
                lastExitReason = SummonFrontlineProxyExitReason.Defeated;
            }
        }
    }

    internal sealed class SummonFrontlineProxyPool
    {
        private readonly List<SummonFrontlineProxy> actors = new List<SummonFrontlineProxy>(4);
        private readonly Queue<SummonFrontlineProxy> queuedActors = new Queue<SummonFrontlineProxy>(4);

        public SummonFrontlineProxy Get(SummonFrontlineProxy prefab, Transform parent)
        {
            if (prefab == null)
            {
                return null;
            }

            while (queuedActors.Count > 0)
            {
                SummonFrontlineProxy pooled = queuedActors.Dequeue();
                if (pooled != null)
                {
                    pooled.gameObject.SetActive(true);
                    return pooled;
                }
            }

            for (int i = 0; i < actors.Count; i++)
            {
                SummonFrontlineProxy reusable = actors[i];
                if (reusable != null && !reusable.IsActive)
                {
                    reusable.gameObject.SetActive(true);
                    return reusable;
                }
            }

            SummonFrontlineProxy instance = UnityEngine.Object.Instantiate(prefab, parent);
            instance.name = prefab.name;
            actors.Add(instance);
            return instance;
        }

        public void Prewarm(SummonFrontlineProxy prefab, Transform parent, int count)
        {
            if (prefab == null || count <= 0)
            {
                return;
            }

            while (actors.Count < count)
            {
                SummonFrontlineProxy actor = UnityEngine.Object.Instantiate(prefab, parent);
                actor.name = prefab.name;
                actor.Deactivate(SummonFrontlineProxyExitReason.None);
                actors.Add(actor);
                queuedActors.Enqueue(actor);
            }
        }

        public void TrimActiveCountBeforeSpawn(int maxActiveActors)
        {
            int allowedExistingActors = Mathf.Max(0, maxActiveActors - 1);
            while (CountActive() > allowedExistingActors)
            {
                SummonFrontlineProxy activeActor = ResolveFirstActive();
                if (activeActor == null)
                {
                    return;
                }

                activeActor.Deactivate(SummonFrontlineProxyExitReason.Recalled);
            }
        }

        public void ForEach(Action<SummonFrontlineProxy> action)
        {
            if (action == null)
            {
                return;
            }

            for (int i = 0; i < actors.Count; i++)
            {
                SummonFrontlineProxy actor = actors[i];
                if (actor != null)
                {
                    action(actor);
                }
            }
        }

        public SummonFrontlineProxy FindForPressureScreen(SummonPressureScreen screen)
        {
            if (screen == null)
            {
                return null;
            }

            for (int i = 0; i < actors.Count; i++)
            {
                SummonFrontlineProxy actor = actors[i];
                if (actor != null && actor.PressureScreen == screen)
                {
                    return actor;
                }
            }

            return null;
        }

        public SummonFrontlineProxy ResolveActive(SummonFrontlineProxy preferred)
        {
            if (preferred != null && preferred.IsActive)
            {
                return preferred;
            }

            return ResolveFirstActive();
        }

        public int CountActive()
        {
            int count = 0;
            for (int i = 0; i < actors.Count; i++)
            {
                if (actors[i] != null && actors[i].IsActive)
                {
                    count++;
                }
            }

            return count;
        }

        public int CountActivePressureScreens()
        {
            int count = 0;
            for (int i = 0; i < actors.Count; i++)
            {
                SummonFrontlineProxy actor = actors[i];
                if (actor != null
                    && actor.PressureScreen != null
                    && actor.PressureScreen.IsActive)
                {
                    count++;
                }
            }

            return count;
        }

        public int CountActivePressureScreenRemainingIntercepts()
        {
            int count = 0;
            for (int i = 0; i < actors.Count; i++)
            {
                SummonFrontlineProxy actor = actors[i];
                if (actor != null
                    && actor.PressureScreen != null
                    && actor.PressureScreen.IsActive)
                {
                    count += actor.PressureScreen.RemainingIntercepts;
                }
            }

            return count;
        }

        private SummonFrontlineProxy ResolveFirstActive()
        {
            for (int i = 0; i < actors.Count; i++)
            {
                SummonFrontlineProxy actor = actors[i];
                if (actor != null && actor.IsActive)
                {
                    return actor;
                }
            }

            return null;
        }
    }
}
